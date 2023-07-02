using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Unity.Services.Analytics.Internal
{
    interface IDispatcher
    {
        void SetBuffer(IBuffer buffer);
        string CollectUrl { get; set; }
        void Flush();
    }

    class Dispatcher : IDispatcher
    {
        readonly IWebRequestHelper m_WebRequestHelper;
        readonly IConsentTracker m_ConsentTracker;

        IBuffer m_DataBuffer;
        IWebRequest m_FlushRequest;

        internal bool FlushInProgress { get; private set; }

        private int m_FlushBufferIndex;

        public string CollectUrl { get; set; }

        public Dispatcher(IWebRequestHelper webRequestHelper, IConsentTracker consentTracker)
        {
            m_WebRequestHelper = webRequestHelper;
            m_ConsentTracker = consentTracker;
        }

        public void SetBuffer(IBuffer buffer)
        {
            m_DataBuffer = buffer;
        }

        public void Flush()
        {
            if (FlushInProgress)
            {
                Debug.LogWarning("Analytics Dispatcher is already flushing.");
            }
            else if (!m_ConsentTracker.IsGeoIpChecked() || !m_ConsentTracker.IsConsentGiven())
            {
                // Also, check if the consent was definitely checked and given at this point.
                Debug.LogWarning("Required consent wasn't checked and given when trying to dispatch events, the events cannot be sent.");
            }
            else
            {
                FlushBufferToService();
            }
        }

        void FlushBufferToService()
        {
            FlushInProgress = true;

            var postBytes = m_DataBuffer.Serialize();
            m_FlushBufferIndex = m_DataBuffer.Length;

            if (postBytes == null || postBytes.Length == 0)
            {
                FlushInProgress = false;
                m_FlushBufferIndex = 0;
            }
            else
            {
                m_FlushRequest = m_WebRequestHelper.CreateWebRequest(CollectUrl, UnityWebRequest.kHttpVerbPOST, postBytes);

                if (m_ConsentTracker.IsGeoIpChecked() && m_ConsentTracker.IsConsentGiven())
                {
                    foreach (var header in m_ConsentTracker.requiredHeaders)
                    {
                        m_FlushRequest.SetRequestHeader(header.Key, header.Value);
                    }
                }

                m_WebRequestHelper.SendWebRequest(m_FlushRequest, UploadCompleted);

#if UNITY_ANALYTICS_EVENT_LOGS
                Debug.Log("Uploading events...");
#endif
            }
        }

        void UploadCompleted(long responseCode)
        {
            if (!m_FlushRequest.isNetworkError &&
                (responseCode == 204 || responseCode == 400))
            {
                // If we get a 400 response code, the JSON is malformed which means we have a bad event somewhere
                // in the queue. In this case, our only recourse is to clear the buffer and discard all events. If bad
                // events were left in the queue, they would recur forever and no more data would ever be uploaded.
                // So, slightly counter-intuitively, our actions on getting a success 204 and an error 400 are actually the same.
                // Other errors are likely transient and should not result in clearance of the buffer.

                m_DataBuffer.ClearBuffer(m_FlushBufferIndex);
                m_DataBuffer.ClearDiskCache();

#if UNITY_ANALYTICS_EVENT_LOGS
                if (responseCode == 204)
                {
                    Debug.Log("Events uploaded successfully!");
                }
                else
                {
                    Debug.Log("Events upload failed due to malformed JSON, likely from corrupt event. Event buffer has been cleared.");
                }
#endif
            }
            else
            {
                // Flush to disk in case we end up exiting before connectivity is re-established.
                m_DataBuffer.FlushToDisk();

#if UNITY_ANALYTICS_EVENT_LOGS
                if (m_FlushRequest.isNetworkError)
                {
                    Debug.Log("Events failed to upload (network error) -- will retry at next heartbeat.");
                }
                else
                {
                    Debug.LogFormat("Events failed to upload (code {0}) -- will retry at next heartbeat.", responseCode);
                }
#endif
            }

            // Clear the request now that we are done.
            FlushInProgress = false;
            m_FlushBufferIndex = 0;
            m_FlushRequest.Dispose();
            m_FlushRequest = null;
        }
    }
}
