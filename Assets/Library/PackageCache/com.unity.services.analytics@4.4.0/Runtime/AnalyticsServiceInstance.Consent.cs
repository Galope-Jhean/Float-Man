using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Analytics.Internal;
using Unity.Services.Core;
using UnityEngine;

namespace Unity.Services.Analytics
{
    partial class AnalyticsServiceInstance
    {
        public async Task<List<string>> CheckForRequiredConsents()
        {
            var response = await m_ConsentTracker.CheckGeoIP();

            if (response.identifier == Consent.None)
            {
                return new List<string>();
            }

            if (m_ConsentTracker.IsConsentDenied())
            {
                return new List<string>();
            }

            if (!m_ConsentTracker.IsConsentGiven())
            {
                return new List<string> { response.identifier };
            }

            return new List<string>();
        }

        public void ProvideOptInConsent(string identifier, bool consent)
        {
            m_CoreStatsHelper.SetCoreStatsConsent(consent);

            if (!m_ConsentTracker.IsGeoIpChecked())
            {
                throw new ConsentCheckException(ConsentCheckExceptionReason.ConsentFlowNotKnown,
                    CommonErrorCodes.Unknown,
                    "The required consent flow cannot be determined. Make sure CheckForRequiredConsents() method was successfully called.",
                    null);
            }

            if (consent == false)
            {
                if (m_ConsentTracker.IsConsentGiven(identifier))
                {
                    m_ConsentTracker.BeginOptOutProcess(identifier);
                    RevokeWithForgetEvent();
                    return;
                }

                Revoke();
            }

            m_ConsentTracker.SetUserConsentStatus(identifier, consent);
        }

        public void OptOut()
        {
            Debug.Log(m_ConsentTracker.IsConsentDenied()
                ? "This user has opted out. Any cached events have been discarded and no more will be collected."
                : "This user has opted out and is in the process of being forgotten...");

            if (m_ConsentTracker.IsConsentGiven())
            {
                // We have revoked consent but have not yet sent the ForgetMe signal
                // Thus we need to keep some of the dispatcher alive until that is done
                m_ConsentTracker.BeginOptOutProcess();
                RevokeWithForgetEvent();

                return;
            }

            if (m_ConsentTracker.IsOptingOutInProgress())
            {
                RevokeWithForgetEvent();
                return;
            }

            Revoke();
            m_ConsentTracker.SetDenyConsentToAll();
            m_CoreStatsHelper.SetCoreStatsConsent(false);
        }

        void Revoke()
        {
            // We have already been forgotten and so do not need to send the ForgetMe signal
            SwapToRevokedBuffer();

            AnalyticsContainer.DestroyContainer();
        }

        internal void RevokeWithForgetEvent()
        {
            SwapToRevokedBuffer();

            m_AnalyticsForgetter.AttemptToForget(k_ForgetCallingId, m_CollectURL, m_InstallId.GetOrCreateIdentifier(), BufferX.SerializeDateTime(DateTime.Now), ForgetMeEventUploaded);
        }

        internal void ForgetMeEventUploaded()
        {
            AnalyticsContainer.DestroyContainer();
            m_ConsentTracker.FinishOptOutProcess();

#if UNITY_ANALYTICS_EVENT_LOGS
            Debug.Log("User opted out successfully and has been forgotten!");
#endif
        }
    }
}
