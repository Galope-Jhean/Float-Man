using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Unity.Services.Analytics.Data;
using Unity.Services.Analytics.Internal;
using Unity.Services.Analytics.Platform;
using Unity.Services.Authentication.Internal;
using Unity.Services.Core.Configuration.Internal;
using Unity.Services.Core.Device.Internal;
using UnityEngine;
using Event = Unity.Services.Analytics.Internal.Event;

namespace Unity.Services.Analytics
{
    internal interface IAnalyticsServiceSystemCalls
    {
        DateTime UtcNow { get; }
    }

    internal class AnalyticsServiceSystemCalls : IAnalyticsServiceSystemCalls
    {
        public DateTime UtcNow
        {
            get { return DateTime.UtcNow; }
        }
    }

    internal interface IUnstructuredEventRecorder
    {
        void CustomData(string eventName,
            IDictionary<string, object> eventParams,
            Int32? eventVersion,
            bool includeCommonParams,
            bool includePlayerIds,
            string callingMethodIdentifier);
    }

    partial class AnalyticsServiceInstance : IAnalyticsService, IUnstructuredEventRecorder
    {
        public string PrivacyUrl => "https://unity3d.com/legal/privacy-policy";

        const string k_CollectUrlPattern = "https://collect.analytics.unity3d.com/api/analytics/collect/v1/projects/{0}/environments/{1}";
        const string k_ForgetCallingId = "com.unity.services.analytics.Events." + nameof(OptOut);
        const string m_StartUpCallingId = "com.unity.services.analytics.Events.Startup";

        readonly TimeSpan k_BackgroundSessionRefreshPeriod = TimeSpan.FromMinutes(5);

        readonly string m_CollectURL;
        readonly StdCommonParams m_CommonParams;
        readonly IPlayerId m_PlayerId;
        readonly IInstallationId m_InstallId;
        readonly IDataGenerator m_DataGenerator;
        readonly ICoreStatsHelper m_CoreStatsHelper;
        readonly IConsentTracker m_ConsentTracker;
        readonly IDispatcher m_DataDispatcher;
        readonly IAnalyticsForgetter m_AnalyticsForgetter;
        readonly IExternalUserId m_CustomUserId;
        readonly IAnalyticsServiceSystemCalls m_SystemCalls;

        readonly IBuffer m_RealBuffer;
        readonly IBuffer m_RevokedBuffer;

        internal IBuffer m_DataBuffer;

        internal string CustomAnalyticsId { get { return m_CustomUserId.UserId; } }
        internal bool ServiceEnabled { get; private set; } = true;

        public string SessionID { get; private set; }

        int m_BufferLengthAtLastGameRunning;
        DateTime m_ApplicationPauseTime;

        internal AnalyticsServiceInstance(IDataGenerator dataGenerator,
                                          IBuffer realBuffer,
                                          IBuffer revokedBuffer,
                                          ICoreStatsHelper coreStatsHelper,
                                          IConsentTracker consentTracker,
                                          IDispatcher dispatcher,
                                          IAnalyticsForgetter forgetter,
                                          ICloudProjectId cloudProjectId,
                                          IInstallationId installId,
                                          IPlayerId playerId,
                                          string environment,
                                          IExternalUserId customAnalyticsId,
                                          IAnalyticsServiceSystemCalls systemCalls)
        {
            m_CustomUserId = customAnalyticsId;

            m_DataGenerator = dataGenerator;
            m_SystemCalls = systemCalls;

            m_RealBuffer = realBuffer;
            m_RevokedBuffer = revokedBuffer;

            m_CoreStatsHelper = coreStatsHelper;
            m_ConsentTracker = consentTracker;
            m_DataDispatcher = dispatcher;

            SwapToRealBuffer();

            m_AnalyticsForgetter = forgetter;

            m_CommonParams = new StdCommonParams
            {
                ClientVersion = Application.version,
                ProjectID = Application.cloudProjectId,
                GameBundleID = Application.identifier,
                Platform = Runtime.Name(),
                BuildGuuid = Application.buildGUID,
                Idfv = SystemInfo.deviceUniqueIdentifier
            };

            m_InstallId = installId;
            m_PlayerId = playerId;

            string projectId = cloudProjectId?.GetCloudProjectId() ?? Application.cloudProjectId;
            m_CommonParams.ProjectID = projectId;
            m_CollectURL = String.Format(k_CollectUrlPattern, projectId, environment.ToLowerInvariant());

            m_DataBuffer.UserID = GetAnalyticsUserID();
            m_DataBuffer.InstallID = m_InstallId.GetOrCreateIdentifier();

            RefreshSessionID();

#if UNITY_ANALYTICS_DEVELOPMENT
            Debug.LogFormat("UA2 Setup\nCollectURL: {0}\nSessionID: {1}", m_CollectURL, SessionID);
#endif
        }

        internal async Task Initialize()
        {
            if (ServiceEnabled)
            {
                AnalyticsContainer.Initialize();

                await InitializeUser();

                // Startup events require a user ID (either Installation or Custom).
                RecordStartupEvents();
            }
        }

        async Task InitializeUser()
        {
            SetVariableCommonParams();

            try
            {
                await m_ConsentTracker.CheckGeoIP();

                if (m_ConsentTracker.IsGeoIpChecked() && (m_ConsentTracker.IsConsentDenied() || m_ConsentTracker.IsOptingOutInProgress()))
                {
                    OptOut();
                }
            }
#if UNITY_ANALYTICS_EVENT_LOGS
            catch (ConsentCheckException e)
            {
                Debug.Log("Initial GeoIP lookup fail: " + e.Message);
            }
#else
            catch (ConsentCheckException)
            {
            }
#endif
        }

        void RecordStartupEvents()
        {
            // Startup Events.
            m_DataGenerator.SdkStartup(DateTime.Now, m_CommonParams, m_StartUpCallingId);
            m_DataGenerator.ClientDevice(DateTime.Now, m_CommonParams, m_StartUpCallingId, SystemInfo.processorType, SystemInfo.graphicsDeviceName, SystemInfo.processorCount, SystemInfo.systemMemorySize, Screen.width, Screen.height, (int)Screen.dpi);

#if UNITY_DOTSRUNTIME
            var isTiny = true;
#else
            var isTiny = false;
#endif

            m_DataGenerator.GameStarted(DateTime.Now, m_CommonParams, m_StartUpCallingId, Application.buildGUID, SystemInfo.operatingSystem, isTiny, DebugDevice.IsDebugDevice(), Locale.AnalyticsRegionLanguageCode());

            if (m_InstallId != null && new InternalNewPlayerHelper(m_InstallId).IsNewPlayer())
            {
                m_DataGenerator.NewPlayer(DateTime.Now, m_CommonParams, m_StartUpCallingId, SystemInfo.deviceModel);
            }
        }

        public string GetAnalyticsUserID()
        {
            return !String.IsNullOrEmpty(CustomAnalyticsId) ? CustomAnalyticsId : m_InstallId.GetOrCreateIdentifier();
        }

        internal void ApplicationPaused(bool paused)
        {
            if (paused)
            {
                m_ApplicationPauseTime = m_SystemCalls.UtcNow;
#if UNITY_ANALYTICS_DEVELOPMENT
                Debug.Log("Analytics SDK detected application pause at: " + m_ApplicationPauseTime.ToString());
#endif
            }
            else
            {
                DateTime now = m_SystemCalls.UtcNow;

#if UNITY_ANALYTICS_DEVELOPMENT
                Debug.Log("Analytics SDK detected application unpause at: " + now);
#endif
                if (now > m_ApplicationPauseTime + k_BackgroundSessionRefreshPeriod)
                {
                    RefreshSessionID();
                }
            }
        }

        internal void RefreshSessionID()
        {
            SessionID = Guid.NewGuid().ToString();
            m_DataBuffer.SessionID = SessionID;

#if UNITY_ANALYTICS_DEVELOPMENT
            Debug.Log("Analytics SDK started new session: " + SessionID);
#endif
        }

        public void Flush()
        {
            if (!ServiceEnabled)
            {
                return;
            }

            if (m_ConsentTracker.IsGeoIpChecked() && m_ConsentTracker.IsConsentGiven())
            {
                m_DataBuffer.InstallID = m_InstallId.GetOrCreateIdentifier();
                m_DataBuffer.PlayerID = m_PlayerId?.PlayerId;
                m_DataBuffer.UserID = GetAnalyticsUserID();
                m_DataBuffer.SessionID = SessionID;

                m_DataDispatcher.CollectUrl = m_CollectURL;
                m_DataDispatcher.Flush();
            }

            if (m_ConsentTracker.IsOptingOutInProgress())
            {
                m_AnalyticsForgetter.AttemptToForget(k_ForgetCallingId, m_CollectURL, m_InstallId.GetOrCreateIdentifier(), BufferX.SerializeDateTime(DateTime.Now), ForgetMeEventUploaded);
            }
        }

        [Obsolete("This mechanism is no longer supported and will be removed in a future version. Use the new Core IAnalyticsStandardEventComponent API instead.")]
        public void RecordInternalEvent(Event eventToRecord)
        {
            if (!ServiceEnabled)
            {
                return;
            }

            m_DataBuffer.PushEvent(eventToRecord);
        }

        /// <summary>
        /// Records the gameEnded event, and flushes the buffer to upload all events.
        /// </summary>
        internal void GameEnded()
        {
            if (!ServiceEnabled)
            {
                return;
            }

            m_DataGenerator.GameEnded(DateTime.Now, m_CommonParams, "com.unity.services.analytics.Events.Shutdown", DataGenerator.SessionEndState.QUIT);

            // Need to null check the consent tracker here in case the game ends before the tracker can be initialised.
            if (m_ConsentTracker != null && m_ConsentTracker.IsGeoIpChecked())
            {
                Flush();
            }
        }

        internal void RecordGameRunningIfNecessary()
        {
            if (ServiceEnabled)
            {
                if (m_DataBuffer.Length == 0 || m_DataBuffer.Length == m_BufferLengthAtLastGameRunning)
                {
                    SetVariableCommonParams();
                    m_DataGenerator.GameRunning(DateTime.Now, m_CommonParams, "com.unity.services.analytics.AnalyticsServiceInstance.RecordGameRunningIfNecessary");
                    m_BufferLengthAtLastGameRunning = m_DataBuffer.Length;
                }
                else
                {
                    m_BufferLengthAtLastGameRunning = m_DataBuffer.Length;
                }
            }
        }

        // <summary>
        // Internal tick is called by the Heartbeat at set intervals.
        // </summary>
        internal void InternalTick()
        {
            if (ServiceEnabled &&
                m_ConsentTracker.IsGeoIpChecked())
            {
                Flush();
            }
        }

        void SetVariableCommonParams()
        {
            m_CommonParams.DeviceVolume = DeviceVolumeProvider.GetDeviceVolume();
            m_CommonParams.BatteryLoad = SystemInfo.batteryLevel;
            m_CommonParams.UasUserID = m_PlayerId?.PlayerId;
        }

        void GameEnded(DataGenerator.SessionEndState quitState)
        {
            if (!ServiceEnabled)
            {
                return;
            }

            m_DataGenerator.GameEnded(DateTime.Now, m_CommonParams, "com.unity.services.analytics.Events.GameEnded", quitState);
        }

        public async Task SetAnalyticsEnabled(bool enabled)
        {
            if (enabled && !ServiceEnabled)
            {
                SwapToRealBuffer();

                await InitializeUser();

                ServiceEnabled = true;
            }
            else if (!enabled && ServiceEnabled)
            {
                SwapToRevokedBuffer();

                ServiceEnabled = false;
            }
        }

        void SwapToRevokedBuffer()
        {
            // Clear everything out of the real buffer and replace it with a dummy
            // that will swallow all events and do nothing
            m_RealBuffer.ClearBuffer();
            m_RealBuffer.ClearDiskCache();
            m_DataBuffer = m_RevokedBuffer;
            m_DataGenerator.SetBuffer(m_RevokedBuffer);
            m_DataDispatcher.SetBuffer(m_RevokedBuffer);
        }

        void SwapToRealBuffer()
        {
            // Reinstate the real buffer so events will be recorded again
            m_DataBuffer = m_RealBuffer;
            m_DataGenerator.SetBuffer(m_RealBuffer);
            m_DataDispatcher.SetBuffer(m_RealBuffer);
        }
    }
}
