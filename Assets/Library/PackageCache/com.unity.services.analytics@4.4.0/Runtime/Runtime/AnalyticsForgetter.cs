using System;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Unity.Services.Analytics.Internal
{
    interface IAnalyticsForgetter
    {
        void AttemptToForget(string collectUrl, string userId, string timestamp, string callingMethod, Action successfulUploadCallback);
    }

    class AnalyticsForgetter : IAnalyticsForgetter
    {
        string m_CollectUrl;
        byte[] m_Event;
        Action m_Callback;

        bool m_SuccessfullyUploaded;
        UnityWebRequestAsyncOperation m_Request;
        readonly IConsentTracker ConsentTracker;

        public AnalyticsForgetter(IConsentTracker consentTracker)
        {
            ConsentTracker = consentTracker;
        }

        public void AttemptToForget(string collectUrl, string userId, string timestamp, string callingMethod, Action successfulUploadCallback)
        {
            if (m_Request != null || m_SuccessfullyUploaded)
            {
                return;
            }

            m_CollectUrl = collectUrl;
            m_Callback = successfulUploadCallback;

            // NOTE: we cannot use String.Format on JSON because it gets confused by all the {}s
            var eventJson =
                "{\"eventList\":[{" +
                "\"eventName\":\"ddnaForgetMe\"," +
                "\"userID\":\"" + userId + "\"," +
                "\"eventUUID\":\"" + Guid.NewGuid().ToString() + "\"," +
                "\"eventTimestamp\":\"" + timestamp + "\"," +
                "\"eventVersion\":1," +
                "\"eventParams\":{" +
                "\"clientVersion\":\"" + Application.version + "\"," +
                "\"sdkMethod\":\"" + callingMethod + "\"" +
                "}}]}";

            m_Event = Encoding.UTF8.GetBytes(eventJson);

            var request = new UnityWebRequest(m_CollectUrl, UnityWebRequest.kHttpVerbPOST);
            var upload = new UploadHandlerRaw(m_Event)
            {
                contentType = "application/json"
            };
            request.uploadHandler = upload;

            if (ConsentTracker.IsGeoIpChecked() && ConsentTracker.IsOptingOutInProgress())
            {
                foreach (var header in ConsentTracker.requiredHeaders)
                {
                    request.SetRequestHeader(header.Key, header.Value);
                }
            }

            m_Request = request.SendWebRequest();
            m_Request.completed += UploadComplete;
        }

        void UploadComplete(AsyncOperation _)
        {
            var code = m_Request.webRequest.responseCode;

#if UNITY_2020_1_OR_NEWER
            if (m_Request.webRequest.result == UnityWebRequest.Result.Success && code == 204)
#else
            if (!m_Request.webRequest.isNetworkError && code == 204)
#endif
            {
                m_SuccessfullyUploaded = true;
                m_Callback();
            }

            // Clear the request to allow another request to be sent.
            m_Request.webRequest.Dispose();
            m_Request = null;
        }
    }
}
