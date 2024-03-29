﻿// .NET
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace XUtils.USBHID
{
    internal class HIDUSBExtMethods
    {
        /* invalid handle value */
        public static IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        /* read access */
        public const uint GENERIC_READ = 0x80000000;
        /* write access */
        public const uint GENERIC_WRITE = 0x40000000;
        /* Enables subsequent open operations on a file or device to request 
         * write access.*/
        public const uint FILE_SHARE_WRITE = 0x2;
        /* Enables subsequent open operations on a file or device to request
         * read access. */
        public const uint FILE_SHARE_READ = 0x1;
        /* The file or device is being opened or created for asynchronous I/O. */
        public const uint FILE_FLAG_OVERLAPPED = 0x40000000;
        /* Opens a file or device, only if it exists. */
        public const uint OPEN_EXISTING = 3;
        /* Opens a file, always. */
        public const uint OPEN_ALWAYS = 4;

        [DllImport("kernel32.dll", SetLastError = true)]
        /* opens files that access usb hid devices */
        public static extern IntPtr CreateFile(
            [MarshalAs(UnmanagedType.LPStr)] string strName,
            uint nAccess, uint nShareMode, IntPtr lpSecurity,
            uint nCreationFlags, uint nAttributes, IntPtr lpTemplate);

        [DllImport("kernel32.dll", SetLastError = true)]
        /* closes file */
        public static extern bool CloseHandle(IntPtr hObject);


        /* The HIDD_ATTRIBUTES structure contains vendor information about a 
         * HIDClass device.*/
        [StructLayout(LayoutKind.Sequential)]
        public struct HiddAttributtes
        {
            /* size in bytes */
            public Int32 Size;
            /* vendor id */
            public Int16 VendorID;
            /* product id */
            public Int16 ProductID;
            /* hid vesion number */
            public Int16 VersionNumber;
        }

        public struct HIDCapabilities
        {
            public Int16 Usage;
            public Int16 UsagePage;
            public Int16 InputReportByteLength;
            public Int16 OutputReportByteLength;
            public Int16 FeatureReportByteLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
            public Int16[] Reserved;
            public Int16 NumberLinkCollectionNodes;
            public Int16 NumberInputButtonCaps;
            public Int16 NumberInputValueCaps;
            public Int16 NumberInputDataIndices;
            public Int16 NumberOutputButtonCaps;
            public Int16 NumberOutputValueCaps;
            public Int16 NumberOutputDataIndices;
            public Int16 NumberFeatureButtonCaps;
            public Int16 NumberFeatureValueCaps;
            public Int16 NumberFeatureDataIndices;
        }


        [DllImport("hid.dll", SetLastError = true)]
        /* gets HID class Guid */
        public static extern void HidD_GetHidGuid(out Guid gHid);

        /* gets hid device attributes */
        [DllImport("hid.dll", SetLastError = true)]
        public static extern Boolean HidD_GetAttributes(IntPtr hFile,
            ref HiddAttributtes attributes);

        /* gets usb manufacturer string */
        [DllImport("hid.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern Boolean HidD_GetManufacturerString(IntPtr hFile,
            StringBuilder buffer, Int32 bufferLength);

        /* gets product string */
        [DllImport("hid.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern Boolean HidD_GetProductString(IntPtr hFile,
            StringBuilder buffer, Int32 bufferLength);
        //byte[] buffer, uint bufferLength);

        /* gets serial number string */
        [DllImport("hid.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool HidD_GetSerialNumberString(IntPtr hDevice,
            StringBuilder buffer, Int32 bufferLength);

        [DllImport("hid.dll", SetLastError = true)]
        internal static extern Boolean HidD_GetPreparsedData(IntPtr hDevice, ref IntPtr PreparsedData);

        [DllImport("hid.dll", SetLastError = true)]
        internal static extern Int32 HidP_GetCaps(IntPtr PreparsedData, ref HIDCapabilities Capabilities);

        [DllImport("hid.dll", SetLastError = true)]
        internal static extern Boolean HidD_FreePreparsedData(IntPtr PreparsedData);

        //// This function retrieves the serial number string from the specified Hid device.
        //// A handle to a Hid Device Object.
        //// Buffer which on return will contain the serial number
        //// string returned from the device. This string is a
        //// wide-character string /// Length of Buffer (in bytes)
        //// TRUE if successful /// FALSE otherwise -- Use GetLastError() to get extended error information
        //[DllImport("hid.dll", SetLastError = true)]
        //public static extern bool HidD_GetSerialNumberString(
        //    IntPtr HidDeviceObject,
        //    [MarshalAs(UnmanagedType.LPWStr, SizeParamIndex = 2)] StringBuilder Buffer,
        //    int BufferLength);


        /* Return only devices that are currently present in a system. */
        public const int DIGCF_PRESENT = 0x02;
        /* Return devices that support device interfaces for the specified 
         * device interface classes. */
        public const int DIGCF_DEVICEINTERFACE = 0x10;

        /* structure returned by SetupDiEnumDeviceInterfaces */
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct DeviceInterfaceData
        {
            /* size of fixed part of structure */
            public int Size;
            /* The GUID for the class to which the device interface belongs. */
            public Guid InterfaceClassGuid;
            /* Can be one or more of the following: SPINT_ACTIVE, 
             * SPINT_DEFAULT, SPINT_REMOVED */
            public int Flags;
            /* do not use */
            public IntPtr Reserved;
        }

        /* A structure contains the path for a device interface.*/
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct DeviceInterfaceDetailData
        {
            /* size of fixed part of structure */
            public int Size;
            /* device path, as to be used by CreateFile */
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
            public string DevicePath;
        }

        /* function returns a handle to a device information set that contains
         * requested device information elements for a local computer */
        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern IntPtr SetupDiGetClassDevs(ref Guid gClass,
            [MarshalAs(UnmanagedType.LPStr)] string strEnumerator,
            IntPtr hParent, uint nFlags);

        /* The function enumerates the device interfaces that are contained in 
         * a device information set.*/
        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern bool SetupDiEnumDeviceInterfaces(
            IntPtr lpDeviceInfoSet, uint nDeviceInfoData, ref Guid gClass,
            uint nIndex, ref DeviceInterfaceData oInterfaceData);

        /* The SetupDiGetDeviceInterfaceDetail function returns details about 
         * a device interface.*/
        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern bool SetupDiGetDeviceInterfaceDetail(
            IntPtr lpDeviceInfoSet, ref DeviceInterfaceData oInterfaceData,
            ref DeviceInterfaceDetailData oDetailData,
            uint nDeviceInterfaceDetailDataSize, ref uint nRequiredSize,
            IntPtr lpDeviceInfoData);

        /* destroys device list */
        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern bool SetupDiDestroyDeviceInfoList(IntPtr lpInfoSet);
    }
}
