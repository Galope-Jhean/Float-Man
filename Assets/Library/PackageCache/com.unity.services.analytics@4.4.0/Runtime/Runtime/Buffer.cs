using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

namespace Unity.Services.Analytics.Internal
{
    interface IBuffer
    {
        string UserID { get; set; }
        string InstallID { get; set; }
        string PlayerID { get; set; }
        string SessionID { get; set; }
        int Length { get; }
        byte[] Serialize();
        void PushStartEvent(string name, DateTime datetime, Int64? eventVersion, bool addPlayerIdsToEventBody = false);
        void PushEndEvent();
        void PushObjectStart(string name = null);
        void PushObjectEnd();
        void PushArrayStart(string name);
        void PushArrayEnd();
        void PushDouble(double val, string name = null);
        void PushFloat(float val, string name = null);
        void PushString(string val, string name = null);
        void PushInt64(Int64 val, string name = null);
        void PushInt(int val, string name = null);
        void PushBool(bool val, string name = null);
        void PushTimestamp(DateTime val, string name = null);
        void FlushToDisk();
        void ClearDiskCache();
        void ClearBuffer();
        void ClearBuffer(long upTo);
        void LoadFromDisk();

        [Obsolete("This mechanism is no longer supported and will be removed in a future version. Use the new Core IAnalyticsStandardEventComponent API instead.")]
        void PushEvent(Event evt);
    }

    class BufferRevoked : IBuffer
    {
        public string UserID { get; set; }
        public string InstallID { get; set; }
        public string PlayerID { get; set; }
        public string SessionID { get; set; }

        public int Length => 0;

        public void ClearBuffer()
        {
        }

        public void ClearBuffer(long upTo)
        {
        }

        public void ClearDiskCache()
        {
        }

        public void FlushToDisk()
        {
        }

        public void LoadFromDisk()
        {
        }

        public void PushArrayEnd()
        {
        }

        public void PushArrayStart(string name = null)
        {
        }

        public void PushBool(bool val, string name = null)
        {
        }

        public void PushDouble(double val, string name = null)
        {
        }

        public void PushEndEvent()
        {
        }

        [Obsolete("This mechanism is no longer supported and will be removed in a future version. Use the new Core IAnalyticsStandardEventComponent API instead.")]
        public void PushEvent(Event evt)
        {
        }

        public void PushFloat(float val, string name = null)
        {
        }

        public void PushInt(int val, string name = null)
        {
        }

        public void PushInt64(long val, string name = null)
        {
        }

        public void PushObjectEnd()
        {
        }

        public void PushObjectStart(string name = null)
        {
        }

        public void PushStartEvent(string name, DateTime datetime, long? eventVersion, bool addPlayerIdsToEventBody = false)
        {
        }

        public void PushString(string val, string name = null)
        {
        }

        public void PushTimestamp(DateTime val, string name = null)
        {
        }

        public byte[] Serialize()
        {
            return null;
        }
    }
}
