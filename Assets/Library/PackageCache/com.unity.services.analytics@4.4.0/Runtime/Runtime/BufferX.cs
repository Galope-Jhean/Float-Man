using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

namespace Unity.Services.Analytics.Internal
{
    internal interface IBufferSystemCalls
    {
        string GenerateGuid();
        DateTime Now();
    }

    class BufferSystemCalls : IBufferSystemCalls
    {
        public string GenerateGuid()
        {
            // NOTE: we are not using .ToByteArray because we do actually need a valid string.
            // Even though the buffer is all bytes, it is ultimately for JSON, so it has to be
            // UTF8 string bytes rather than raw bytes (the string also includes hyphenated
            // subsections).
            return Guid.NewGuid().ToString();
        }

        public DateTime Now()
        {
            return DateTime.Now;
        }
    }

    class BufferX : IBuffer
    {
        // 4MB: 4 * 1024 KB to make an MB and * 1024 bytes to make a KB
        // The Collect endpoint can actually accept payloads of up to 5MB (at time of writing, Jan 2023),
        // but we want to retain some headroom... just in case.
        const long k_UploadBatchMaximumSizeInBytes = 4 * 1024 * 1024;
        const string k_BufferHeader = "{\"eventList\":[";

        const string k_SecondDateFormat = "yyyy-MM-dd HH:mm:ss zzz";
        const string k_MillisecondDateFormat = "yyyy-MM-dd HH:mm:ss.fff zzz";
        readonly IBufferSystemCalls m_SystemCalls;
        readonly IDiskCache m_DiskCache;

        readonly List<int> m_EventEnds;

        MemoryStream m_SpareBuffer;
        MemoryStream m_Buffer;

        public string UserID { get; set; }
        public string InstallID { get; set; }
        public string PlayerID { get; set; }
        public string SessionID { get; set; }

        public int Length { get { return (int)m_Buffer.Length; } }

        /// <summary>
        /// The number of events that have been recorded into this buffer.
        /// </summary>
        internal int EventsRecorded { get { return m_EventEnds.Count; } }

        /// <summary>
        /// The byte index of the end of each event blob in the bytestream.
        /// </summary>
        internal IReadOnlyList<int> EventEndIndices => m_EventEnds;

        /// <summary>
        /// The raw contents of the underlying bytestream.
        /// Only exposed for unit testing.
        /// </summary>
        internal byte[] RawContents => m_Buffer.ToArray();

        public BufferX(IBufferSystemCalls eventIdGenerator, IDiskCache diskCache)
        {
            m_Buffer = new MemoryStream();
            m_SpareBuffer = new MemoryStream();
            m_EventEnds = new List<int>();

            m_SystemCalls = eventIdGenerator;
            m_DiskCache = diskCache;

            ClearBuffer();
        }

        private void WriteString(string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            for (var i = 0; i < bytes.Length; i++)
            {
                m_Buffer.WriteByte(bytes[i]);
            }
        }

        public void PushStartEvent(string name, DateTime datetime, long? eventVersion, bool addPlayerIdsToEventBody)
        {
#if UNITY_ANALYTICS_EVENT_LOGS
            Debug.LogFormat("Recording event {0} at {1} (UTC)...", name, SerializeDateTime(datetime));
#endif
            WriteString("{");
            WriteString("\"eventName\":\"");
            WriteString(name);
            WriteString("\",");
            WriteString("\"userID\":\"");
            WriteString(UserID);
            WriteString("\",");
            WriteString("\"sessionID\":\"");
            WriteString(SessionID);
            WriteString("\",");
            WriteString("\"eventUUID\":\"");
            WriteString(m_SystemCalls.GenerateGuid());
            WriteString("\",");

            WriteString("\"eventTimestamp\":\"");
            WriteString(SerializeDateTime(datetime));
            WriteString("\",");

            if (eventVersion != null)
            {
                WriteString("\"eventVersion\":");
                WriteString(eventVersion.ToString());
                WriteString(",");
            }

            if (addPlayerIdsToEventBody)
            {
                WriteString("\"unityInstallationID\":\"");
                WriteString(InstallID);
                WriteString("\",");

                if (!String.IsNullOrEmpty(PlayerID))
                {
                    WriteString("\"unityPlayerID\":\"");
                    WriteString(PlayerID);
                    WriteString("\",");
                }
            }

            WriteString("\"eventParams\":{");
        }

        private void StripTrailingCommaIfNecessary()
        {
            // Stripping the comma once at the end of something is probably
            // faster than checking to see if we need to add one before
            // every single property inside it. Even though it seems
            // a bit convoluted.

            m_Buffer.Seek(-1, SeekOrigin.End);
            char precedingChar = (char)m_Buffer.ReadByte();
            if (precedingChar == ',')
            {
                // Burn that comma, we don't need it and it breaks JSON!
                m_Buffer.Seek(-1, SeekOrigin.Current);
                m_Buffer.SetLength(m_Buffer.Length - 1);
            }
        }

        public void PushEndEvent()
        {
            StripTrailingCommaIfNecessary();

            // Close params block, close event object, comma to prepare for next item
            WriteString("}},");

            int bufferLength = (int)m_Buffer.Length;

            // If this event is too big to ever be uploaded, clear the buffer so we don't get stuck forever.
            int eventSize = m_EventEnds.Count > 0 ? bufferLength - m_EventEnds[m_EventEnds.Count - 1] : bufferLength;

            if (eventSize > k_UploadBatchMaximumSizeInBytes)
            {
                Debug.LogWarning($"Detected event that would be too big to upload (greater than {k_UploadBatchMaximumSizeInBytes / 1024}KB in size), discarding it to prevent blockage.");

                int previousBufferLength = m_EventEnds.Count > 0 ? m_EventEnds[m_EventEnds.Count - 1] : k_BufferHeader.Length;

                m_Buffer.SetLength(previousBufferLength);
                m_Buffer.Position = previousBufferLength;
            }
            else
            {
                m_EventEnds.Add(bufferLength);

#if UNITY_ANALYTICS_DEVELOPMENT
                Debug.Log($"Event {m_EventEnds.Count} ended at: {bufferLength}");
#endif
            }
        }

        public void PushObjectStart(string name = null)
        {
            if (name != null)
            {
                WriteString("\"");
                WriteString(name);
                WriteString("\":");
            }
            WriteString("{");
        }

        public void PushObjectEnd()
        {
            StripTrailingCommaIfNecessary();

            WriteString("},");
        }

        public void PushArrayStart(string name)
        {
            WriteString("\"");
            WriteString(name);
            WriteString("\":");
            WriteString("[");
        }

        public void PushArrayEnd()
        {
            StripTrailingCommaIfNecessary();

            WriteString("],");
        }

        public void PushDouble(double val, string name = null)
        {
            if (name != null)
            {
                WriteString("\"");
                WriteString(name);
                WriteString("\":");
            }
            var formatted = val.ToString(CultureInfo.InvariantCulture);
            WriteString(formatted);
            WriteString(",");
        }

        public void PushFloat(float val, string name = null)
        {
            if (name != null)
            {
                WriteString("\"");
                WriteString(name);
                WriteString("\":");
            }
            var formatted = val.ToString(CultureInfo.InvariantCulture);
            WriteString(formatted);
            WriteString(",");
        }

        public void PushString(string val, string name = null)
        {
#if UNITY_ANALYTICS_DEVELOPMENT
            Debug.AssertFormat(!String.IsNullOrEmpty(val), "Required to have a value");
#endif
            if (name != null)
            {
                WriteString("\"");
                WriteString(name);
                WriteString("\":");
            }

            // TODO: JsonConvert here is necessary to ensure strings don't break the JSON we are producing,
            // BUT it is also a major performance hotspot. Can we escape embedded JSON in a better way?
            WriteString(JsonConvert.ToString(val));
            WriteString(",");
        }

        public void PushInt64(long val, string name = null)
        {
            if (name != null)
            {
                WriteString("\"");
                WriteString(name);
                WriteString("\":");
            }
            WriteString(val.ToString());
            WriteString(",");
        }

        public void PushInt(int val, string name = null)
        {
            PushInt64(val, name);
        }

        public void PushBool(bool val, string name = null)
        {
            if (name != null)
            {
                WriteString("\"");
                WriteString(name);
                WriteString("\":");
            }
            WriteString(val ? "true" : "false");
            WriteString(",");
        }

        public void PushTimestamp(DateTime val, string name)
        {
            WriteString("\"");
            WriteString(name);
            WriteString("\":\"");
            WriteString(SerializeDateTime(val));
            WriteString("\",");
        }

        [Obsolete("This mechanism is no longer supported and will be removed in a future version. Use the new Core IAnalyticsStandardEventComponent API instead.")]
        public void PushEvent(Event evt)
        {
            // Serialize event

            var dateTime = m_SystemCalls.Now();
            PushStartEvent(evt.Name, dateTime, evt.Version, false);

            // Serialize event params

            var eData = evt.Parameters;

            foreach (var data in eData.Data)
            {
                if (data.Value is float f32Val)
                {
                    PushFloat(f32Val, data.Key);
                }
                else if (data.Value is double f64Val)
                {
                    PushDouble(f64Val, data.Key);
                }
                else if (data.Value is string strVal)
                {
                    PushString(strVal, data.Key);
                }
                else if (data.Value is int intVal)
                {
                    PushInt(intVal, data.Key);
                }
                else if (data.Value is Int64 int64Val)
                {
                    PushInt64(int64Val, data.Key);
                }
                else if (data.Value is bool boolVal)
                {
                    PushBool(boolVal, data.Key);
                }
            }

            PushEndEvent();
        }

        public byte[] Serialize()
        {
            if (m_EventEnds.Count > 0)
            {
                long originalBufferPosition = m_Buffer.Position;

                // Tick through the event end indices until we find the last complete event
                // that fits into the maximum payload size.
                int end = m_EventEnds[0];
                int nextEnd = 0;
                while (nextEnd < m_EventEnds.Count &&
                       m_EventEnds[nextEnd] < k_UploadBatchMaximumSizeInBytes)
                {
                    end = m_EventEnds[nextEnd];
                    nextEnd++;
                }

                // Extend the payload so we can fit the suffix.
                byte[] payload = new byte[end + 1];
                m_Buffer.Position = 0;
                m_Buffer.Read(payload, 0, end);

                // NOTE: the final character will be a comma that we don't want,
                // so take this opportunity to overwrite it with the closing
                // bracket (event list) and brace (payload object).
                byte[] suffix = Encoding.UTF8.GetBytes("]}");
                payload[end - 1] = suffix[0];
                payload[end] = suffix[1];

                m_Buffer.Position = originalBufferPosition;

                return payload;
            }
            else
            {
                return null;
            }
        }

        public void ClearBuffer()
        {
            m_Buffer.SetLength(0);
            m_Buffer.Position = 0;
            WriteString(k_BufferHeader);

            m_EventEnds.Clear();
        }

        public void ClearBuffer(long upTo)
        {
            MemoryStream oldBuffer = m_Buffer;
            m_Buffer = m_SpareBuffer;
            m_SpareBuffer = oldBuffer;

            // We want to keep the end markers for events that have been copied over.
            // We have to account for the start point change AND remove markers for events before the clear point.

            int lastClearedEventIndex = 0;
            for (int i = 0; i < m_EventEnds.Count; i++)
            {
                m_EventEnds[i] = m_EventEnds[i] - (int)upTo + k_BufferHeader.Length;
                if (m_EventEnds[i] <= k_BufferHeader.Length)
                {
                    lastClearedEventIndex = i;
                }
            }
            m_EventEnds.RemoveRange(0, lastClearedEventIndex + 1);

            // Reset the buffer back to a blank state...
            m_Buffer.SetLength(0);
            m_Buffer.Position = 0;
            WriteString(k_BufferHeader);

            // ... and copy over anything that came after the cut-off point.
            m_SpareBuffer.Position = upTo;
            for (long i = upTo; i < m_SpareBuffer.Length; i++)
            {
                byte b = (byte)m_SpareBuffer.ReadByte();
                m_Buffer.WriteByte(b);
            }

            m_SpareBuffer.SetLength(0);
            m_SpareBuffer.Position = 0;
        }

        public void FlushToDisk()
        {
            m_DiskCache.Write(m_EventEnds, m_Buffer);
        }

        public void ClearDiskCache()
        {
            m_DiskCache.Clear();
        }

        public void LoadFromDisk()
        {
            bool success = m_DiskCache.Read(m_EventEnds, m_Buffer);

            if (!success)
            {
                // Reset the buffer in case we failed half-way through populating it.
                ClearBuffer();
            }
        }

        internal static string SerializeDateTime(DateTime dateTime)
        {
            return dateTime.ToString(k_MillisecondDateFormat, CultureInfo.InvariantCulture);
        }
    }
}
