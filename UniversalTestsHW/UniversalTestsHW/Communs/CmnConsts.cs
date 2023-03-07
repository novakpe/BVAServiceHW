// .NET
using System;

// pakety

// report OUT - do zarizeni
// [0] - Cmd
// [1] - CmdSub
// [2] - Status
// [3] - Part
// [4] - Data ...

// report IN - ze zarizeni
// [0] - Cmd|0x80
// [1] - CmdSub
// [2] - Status (vysledek vykonani)
// [3] - Part
// [4] - Data ...

// EmlInfo: ident, elmType, status, dataType

namespace UniTestsHW.Communs;

public class CmnConsts
{
    // typ komunikace pro 'device'
    public enum EnCmnType { None, BlueTooth, ZigBee, USBHID, SerialPort }
    // pro obecne pouziti pri urceni smeru
    public enum EnDir { None = 0, In = 1, Out = 2, InOut = In | Out }

    // hlavni povel do zarizeni
    public enum EnPaketCmd : byte { None = 0, Info = 1, Mode = 11, Write = 21, Read = 31, Data = 41 }
    public enum EnPaketCmdInfoSub : byte { None = 0, In = 1, Out = 2 };
    public enum EnPaketCmdModeSub : byte { None = 0, Device = 1, Elements = 2 };
    public enum EnPaketStatus : byte { None = 0, OK = 1, NotSupport = 1 | 0x80 };

    public enum EnPaketCmdModeData : byte { None = 0, Enable = 1, Disable = 2, ResetSW = 3, ResetHW = 4 };

    // --- ---

    // pozice polozek v paketu
    public enum EnPaketPos { Cmd = 0, CmdSub = 1, Status = 2, Part = 3, Data = 4 /* pozice zacatku dat */ }

    // vyrobce / nazev zarizeni
    //public const string DeviceManufacturerString = "PN"; public const string DeviceProductString = "BVA Control V01";
    public const string DeviceManufacturerString = "PN"; public const string DeviceProductString = "UniversalTests V01";

    // --- ---

    public delegate void DelDeviceChanged(Data.ItmDevice device);
    public delegate void DelElementChanged(Data.ItmDevice device, Data.ItmElementBase element);
}
