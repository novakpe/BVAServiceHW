// .NET
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace XUtils.USBHID
{
    internal class USBHIDInfo
    {
        public string Path { get; private set; }            // path
        public string Vid { get; private set; }             // VID
        public string Pid { get; private set; }             // PID
        public string Manufacturer { get; private set; }    // manufacturer
        public string Product { get; set; }                 // product
        public string SerialNumber { get; private set; }    // serial number
        public UInt16 OutputReportBytesLength { get; private set; }
        public UInt16 InputReportBytesLength { get; private set; }
        public UInt16 FeatureReportBytesLength { get; private set; }

        // celkovy nazev / text / popis zarizeni pro jeho (snadnou) identifikaci
        public override string ToString()
        {
            string col = "00"; // poradove cislo ve slozenem zarizeni
            // pokud je pritomna polozka '&col', tak se vyzvedne za ni nasledujici hodnota (dva znaky)
            if (Path.IndexOf("&col") > 0) { col = Path.Substring(Path.IndexOf("&col") + 4, 2); }
            // vytvoreni vystupniho retezce
            return Manufacturer + " / " + Product + " / " + SerialNumber + " / " + col;
        }

        /* constructor */
        public USBHIDInfo(string path, string vid, string pid,
            string manufacturer, string product, string serial,
            UInt16 outputReportBytesLength, UInt16 inputReportBytesLength, UInt16 featureReportBytesLength)
        {
            Path = path;
            Vid = vid; Pid = pid;
            Manufacturer = manufacturer; Product = product; SerialNumber = serial;
            OutputReportBytesLength = outputReportBytesLength; InputReportBytesLength = inputReportBytesLength; FeatureReportBytesLength = featureReportBytesLength;
        }
    }

    internal class HIDUSBStatic
    {
        /* browse all HID class devices */
        public static List<USBHIDInfo> GetAll()
        {
            /* list of device information */
            List<USBHIDInfo> info = new List<USBHIDInfo>();
            try
            {
                /* hid device class guid */
                Guid gHid;
                /* obtain hid guid */
                HIDUSBExtMethods.HidD_GetHidGuid(out gHid);
                /* get list of present hid devices */
                var hInfoSet = HIDUSBExtMethods.SetupDiGetClassDevs(ref gHid, null, IntPtr.Zero,
                    HIDUSBExtMethods.DIGCF_DEVICEINTERFACE | HIDUSBExtMethods.DIGCF_PRESENT);

                /* allocate mem for interface descriptor */
                var iface = new HIDUSBExtMethods.DeviceInterfaceData();
                /* set size field */
                iface.Size = Marshal.SizeOf(iface);
                /* interface index */
                uint index = 0;

                /* iterate through all interfaces */
                while (HIDUSBExtMethods.SetupDiEnumDeviceInterfaces(hInfoSet, 0, ref gHid,
                    index, ref iface))
                {
                    try
                    {
                        /* get device path */
                        var path = GetPath(hInfoSet, ref iface);

                        /* open device */
                        var handle = Open(path);
                        /* device is opened? */
                        if (handle != HIDUSBExtMethods.INVALID_HANDLE_VALUE)
                        {
                            var preparsedData = new IntPtr();
                            HIDUSBExtMethods.HidD_GetPreparsedData(handle, ref preparsedData);
                            HIDUSBExtMethods.HIDCapabilities capabilities = new HIDUSBExtMethods.HIDCapabilities();
                            Int32 result = HIDUSBExtMethods.HidP_GetCaps(preparsedData, ref capabilities);

                            /* add to list */
                            info.Add(new USBHIDInfo(
                                path,
                                GetItem(EnumItem.Vid, handle),
                                GetItem(EnumItem.Pid, handle),
                                GetItem(EnumItem.Manufac, handle),
                                GetItem(EnumItem.Product, handle),
                                GetItem(EnumItem.SerNum, handle),
                                (UInt16)capabilities.OutputReportByteLength,
                                (UInt16)capabilities.InputReportByteLength,
                                (UInt16)capabilities.FeatureReportByteLength));

                            if (preparsedData != IntPtr.Zero)
                                { HIDUSBExtMethods.HidD_FreePreparsedData(preparsedData); }

                            /* close */
                            Close(handle);
                        }
                    }
                    catch { /* ??? */ }

                    index++;    /* next, please */
                }

                /* clean up */
                if (HIDUSBExtMethods.SetupDiDestroyDeviceInfoList(hInfoSet) == false)
                    { throw new Win32Exception(); }
            } catch { /* ??? */ }
            return info; /* return list */
        }

        /* open device */
        private static IntPtr Open(string path)
        {
            try
            {
                /* opens hid device file */
                return HIDUSBExtMethods.CreateFile(path,
                    HIDUSBExtMethods.GENERIC_READ | HIDUSBExtMethods.GENERIC_WRITE,
                    HIDUSBExtMethods.FILE_SHARE_READ | HIDUSBExtMethods.FILE_SHARE_WRITE,
                    IntPtr.Zero, HIDUSBExtMethods.OPEN_EXISTING, HIDUSBExtMethods.FILE_FLAG_OVERLAPPED,
                    IntPtr.Zero);
            } catch { return IntPtr.Zero; }
        }

        /* close device */
        private static void Close(IntPtr handle)
        {
            /* try to close handle */
            if (HIDUSBExtMethods.CloseHandle(handle) == false) { /* throw new Win32Exception(); */ }
        }

        /* get device path */
        private static string GetPath(IntPtr hInfoSet, ref HIDUSBExtMethods.DeviceInterfaceData iface)
        {
            try
            {
                /* detailed interface information */
                var detIface = new HIDUSBExtMethods.DeviceInterfaceDetailData();
                /* required size */
                uint reqSize = (uint)Marshal.SizeOf(detIface);

                /* set size. The cbSize member always contains the size of the 
                 * fixed part of the data structure, not a size reflecting the 
                 * variable-length string at the end. */
                /* now stay with me and look at that x64/x86 maddness! */
                detIface.Size = Marshal.SizeOf(typeof(IntPtr)) == 8 ? 8 : 5;

                /* get device path */
                bool status = HIDUSBExtMethods.SetupDiGetDeviceInterfaceDetail(hInfoSet,
                    ref iface, ref detIface, reqSize, ref reqSize, IntPtr.Zero);

                /* whops */
                if (!status) { throw new Win32Exception(); }

                /* return device path */
                return detIface.DevicePath;
            } catch { return String.Empty; }
}

        // vyzvednuti nejake (informacni) polozky
        private enum EnumItem { Vid, Pid, Manufac, Product, SerNum };
        private static string GetItem(EnumItem item, IntPtr handle)
        {
            try
            {
                // pro ostatni
                StringBuilder sb = new StringBuilder(256);
                switch(item)
                {
                    case EnumItem.Vid: // VID a PID
                    case EnumItem.Pid: {
                        /* attributes structure */
                        var attr = new HIDUSBExtMethods.HiddAttributtes();
                        /* set size */
                        attr.Size = Marshal.SizeOf(attr);

                        /* get attributes */
                        if (HIDUSBExtMethods.HidD_GetAttributes(handle, ref attr) == false)
                            { throw new Win32Exception(); }

                        // vraceni hodnoty v textovem AsciiHex tvaru
                        if (item == EnumItem.Vid) { return "0x" + attr.VendorID.ToString("X4"); }
                        if (item == EnumItem.Pid) { return "0x" + attr.ProductID.ToString("X4"); }
                        break; }
                    case EnumItem.Manufac: { // Manufacturer
                        if (HIDUSBExtMethods.HidD_GetManufacturerString(handle, sb, sb.Capacity))
                            { return sb.ToString(); }
                        break; }
                    case EnumItem.Product: { // Product (name)
                        if (HIDUSBExtMethods.HidD_GetProductString(handle, sb, sb.Capacity))
                            { return sb.ToString(); }
                        break; }
                    case EnumItem.SerNum: { // SerilNumber
                        if (HIDUSBExtMethods.HidD_GetSerialNumberString(handle, sb, sb.Capacity))
                            { return sb.ToString(); }
                        break; }
                }
            
            // vraceni ziskane hodnoty
            return String.Empty;
            } catch { return String.Empty; }
        }
    }
}
