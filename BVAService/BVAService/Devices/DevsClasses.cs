// .NET
using System;

namespace BVAService.Devices
{
    // trida pro 'Device-Anchor'
    public class DeviceAnchor //: DeviceBase
    {
        public string Name { get; set; } = String.Empty;

        public int Number { get; set; } = 0;

        public string Value { get; set; } = String.Empty;
    }

    // jedna pred-definovana pozice pro 'Laser'
    // (ulozit jejich seznam do XML souboru pro pouziti)
    public class LaserPos
    {
        public string Name { get; set; } = String.Empty;
        public sbyte PosX { get; set; } = 0;
        public sbyte PosY { get; set; } = 0;
    }

    // jeden dostupny obrazek pro 'Anchor' (nacteny ze souboru)
    public class OneImage
    {
        // nazev
        public string Name { get; set; } = String.Empty;

        // bytove hodnoty pro zaslani do externiho zarizeni
        // [0] - typ dat / obrazku:
        //      1 -> Rgb + Bitmap (0/1,bit)
        //      2 -> RgbPoints (3B na bod,RGB) (!!! zatim neni !!!)
        // [1] - size X (pocet bodu vodorovne)
        // [2] - size Y (pocet bodu svisle)
        // type 1
        //   [3,4,5] - R,G,B (barva obrazku, pro veschny body spolecna)
        //   [6,...] - data, vodorovne po radcich -> bit7,bit6,bit5, ..., bit0(1B) , bit7, bit6, bit5,..., bit0 (1B)
        // type 2
        //   [3,...] - data, vodorovne po radcich -> R0(1B),G0(1B),B0(1B),R1(1B),G1(1B),B1(1B),R2(1B),G2(1B),B2(1B),...
        public byte[] Values { get; set; } = null;
    }
}
