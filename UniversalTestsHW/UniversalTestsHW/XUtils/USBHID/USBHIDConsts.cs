// .NET
using System;

namespace XUtils.USBHID
{
    internal class USBHIDConsts
    {
        public enum EnCmnStatus
        {
            None = 0,
            OK = 1,
            SendError = 100,
            TimeOut = 200,
            Error = 254
        }
    }
}
