// .NET
using System;

namespace UniTestsHW.Data;

public class DataConsts
{
    public const string LocationDef = "---";
    public const string IdentDef = "---";
    public const string NameDef = "---";
    public const string DescDef = "---";

    [Flags]
    public enum EnDataType : byte // ((spodnich 7bits v 'DataType'))
    {
        None = 0,
        Bool = 1,
        UInt8 = 2, Int8 = 3, UInt16 = 4, Int16 = 5, UInt32 = 6, Int32 = 7, UInt64 = 8, Int64 = 9,
        
        FloatBase = 21,
        Bytes = 31, String = 32,
        UInt8x3 = 50,
    };

    public enum EnDataStatus { None = 0, Active = 1, Lost = 2, Changed = 4, BaseMask = 0x03 }

    // typy 'Element(s)'
    public enum EnElmType
    {
        None = 0,

        Led = 1,
        Button = 2,
        Buzzer = 3,
        NfcReader = 4,
        RgbMatrix = 5,

        // virtualni
        Virtual = 1000,
        Picture = 1001,
        Sound = 1002,
        Speech = 1003,

        //None = 0,
        //Switch = 1, Rotary = 2, Temp = 3,
        //Position = 4, PositionXY = 5,
        //Led = 10, Button = 11
    }
    // status
    public enum EnElmStatus { None = 0, OK = 1 }
}
