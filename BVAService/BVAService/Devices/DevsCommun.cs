// .NET
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
// UnilTests
using UniTestsHW.Data;

namespace BVAService.Devices
{
    public class DevsCmns
    {
        private static byte[] cmdDataReset = new byte[] { (byte)UniTestsHW.Communs.CmnConsts.EnPaketCmdModeData.ResetSW };

        public static bool EnableHW { get; set; } = false;

        // pouzite 'Device(s)' ...
        public class EnDeviceIdent
        {
            // DevIdent(s) - string
            public const string Laser = "00000011";
            public const string Motor = "00000012";
            public const string AnchorsBase = "00000020";
        }

        // seznam znamych / ocekavanych 'Device(s)'
        public static Dictionary<string,string> DevsSerNumToName { get; set; } = new Dictionary<string, string>()
        {
            // Laser
            { "00000011", "Laser" },
            // Motor
            { "00000012", "Motor" },
            // Anchor(s)
            { "00000021", "Anchor1" }, { "00000022", "Anchor2" }, { "00000023", "Anchor3" }, { "00000024", "Anchor4" }, { "00000025", "Anchor5" }, { "00000026", "Anchor6" }, { "00000027", "Anchor7" }, { "00000028", "Anchor8" }
        };

        // nastaveni hlavni cinnosti
        public enum EnMainAction
        {
            Enable = 1,
            Disable = 2
        }

        // pouzite 'Element(s)' v 'Device-Laser'
        public enum EnLaserElement
        {
            // ElmIdent(s) - string
            PosXY = 1,
            OnOff = 2
        }
        // pouzite 'Element(s)' v 'Device-Motor'
        public enum EnMotorElement
        {
            // ElmIdent(s) - string
            Move = 1,
            Zero = 2,
            Rotary = 3
        }

        // timer pro zapis dat do externich zarizeni
        private static Timer timer = new Timer(50);

        static DevsCmns()
        {
            // udalost od casovace
            timer.Elapsed += OnTimedEvent;
        }

        // nalezeni vsech pripojenych zarizeni
        // (vraceni poctu skutecne nalezenych zarizeni)
        public static string[] FindAllDevice()
        {
            if (EnableHW == true)
            {
                // nalezeni vsech dostupnych zarizeni
                int count = UniTestsHW.Communs.CmnMain.FindAllDevice();
                // pokud neni zadne zarizeni, tak rovno navrat
                if (count == 0) { return null; }
                // vytvoreni textoveho seznamu nalezenych USB zarizeni
                List<string> devicesAdr = new List<string>();
                // sestaveni seznamu adres nalezenych zarizeni
                foreach (ItmDevice itmDevice in DataMain.devices)
                {
                    // adresa jednoho nalezeneho zarizeni
                    devicesAdr.Add(itmDevice.Ident);
                }
                // vracei seznamu jako pole
                return devicesAdr.ToArray();
            }
            return null;
        }

        // spusteni / zastaveni vsech externich zarizeni
        public static void SetMode(EnMainAction action)
        {
            if (EnableHW == true)
            {
                byte[] cmdData = null;
                // vytvoreni seznamu cinnosti pro vykonani
                switch(action)
                {
                    case EnMainAction.Enable: { cmdData = new byte[]
                        { (byte)UniTestsHW.Communs.CmnConsts.EnPaketCmdModeData.ResetSW, (byte)UniTestsHW.Communs.CmnConsts.EnPaketCmdModeData.Enable }; break; }
                    case EnMainAction.Disable: { cmdData = new byte[]
                        { (byte)UniTestsHW.Communs.CmnConsts.EnPaketCmdModeData.Disable, (byte)UniTestsHW.Communs.CmnConsts.EnPaketCmdModeData.ResetSW }; break; }
                }
                // zaslani povelu do zarizeni
                UniTestsHW.Communs.CmnMain.CmdWrite(null /* vsecm 'Device(s)' */,
                    (byte)UniTestsHW.Communs.CmnConsts.EnPaketCmd.Mode, (byte)UniTestsHW.Communs.CmnConsts.EnPaketCmdModeSub.Device, cmdData);

                // !!! Test !!!
                //byte number = 4;
                //AnchorSetImage(number, "SipkaLevo 16x16", EnImageParms.Set);
                //AnchorSetImage(number, "SipkaPravo 16x16", EnImageParms.Or);
                //AnchorSetImage(number, "SipkaPravo 16x16", EnImageParms.NonAnd);
            }
        }

        // tika casovace pro moznou periodickou aktualizaci externich zarizeni
        private static bool newData = false;
        private static void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            // pokud jsou nova data, tak se vyslou do externich zarizeni
            if (newData == true)
                { newData = false; UpdateDevice(EnDeviceIdent.Laser); }
        }

        // --- laser ---

        // rozsah pohybu laseru (-/+ v X i Y)
        private const sbyte laserRangeMax = 100;

        // nastaveni nove pozice a jeji periodicky zapis do externiho zarizeni
        public enum EnLaserAction { None, Start, Move, End, Position }
        public static void LaserPosPercent(EnLaserAction action, double posX = 0.0, double posY = 0.0)
        {
            switch(action)
            {
                // zacatek pohybu - spusteni casovace
                case EnLaserAction.Start: { timer.Enabled = true; break; }
                // prubeh pohybu - nastaveni hodnot + zadost o zapis do externiho zarizeni
                case EnLaserAction.Move: {
                        double val = (sbyte)((double)laserRangeMax / 100.0 * posX);
                        if (val > 120) { val = 120; }
                        if (val < -120) { val = -120; }
                        LaserPosX = (sbyte)val;

                        val = (sbyte)((double)laserRangeMax / 100.0 * posY);
                        if (val > 120) { val = 120; }
                        if (val < -120) { val = -120; }
                        LaserPosY = (sbyte)val;
                        newData = true;
                        break; }
                // konec pohybu - zastaveni casovace
                case EnLaserAction.End: { timer.Enabled = false; break; }
                // nastaveni primo zadane pozice v procentech
                case EnLaserAction.Position: {
                        LaserPosX = (sbyte)posX; LaserPosY = (sbyte)posY;
                        UpdateDevice(EnDeviceIdent.Laser);
                        break; }
            }
        }

        // --- ---

        // potrebne Element(s) jako 'Propertie(s)' (toto neni nutne potreba, lze volat rovnou v miste pouziti)

        public static sbyte LaserPosX // Value = -/+127
        {
            get
            {
                if (EnableHW == false) { return 0; }
                return DataUtils.GetValue<ItmDataInt8>(EnDeviceIdent.Laser, (byte)EnLaserElement.PosXY, 1).Value;
            }
            set
            {
                if (EnableHW == true)
                    { DataUtils.GetValue<ItmDataInt8>(EnDeviceIdent.Laser, (byte)EnLaserElement.PosXY, 1).Value = value; }
            }
        }

        public static sbyte LaserPosY // Value = -/+127
        {
            get
            {
                if (EnableHW == false) { return 0; }
                return DataUtils.GetValue<ItmDataInt8>(EnDeviceIdent.Laser, (byte)EnLaserElement.PosXY, 2).Value;
            }
            set
            {
                if (EnableHW == true)
                    { DataUtils.GetValue<ItmDataInt8>(EnDeviceIdent.Laser, (byte)EnLaserElement.PosXY, 2).Value = value; }
            }
        }

        public static bool LaserOn // Value = 0/1
        {
            get
            {
                if (EnableHW == false) { return false; }
                return DataUtils.GetValue<ItmDataBool>(EnDeviceIdent.Laser, (byte)EnLaserElement.OnOff, 1).Value;
            }
            set
            {
                if (EnableHW == true)
                    { DataUtils.GetValue<ItmDataBool>(EnDeviceIdent.Laser, (byte)EnLaserElement.OnOff, 1).Value = value; }
            }
        }

        // --- obsluha z GUI ---

        public static bool laserMove = false;

        // uchopeni 'Ellipse'
        public static void LaserMoveStart(Ellipse ellipse)
        {
            ellipse.CaptureMouse();
            laserMove = true;
            LaserPosPercent(EnLaserAction.Start);
        }
        // pohyb s 'Ellipse'
        public static void LaserMoveProcess(Point pos, Ellipse ellipse, Canvas canvas)
        {
            if (laserMove == true)
            {
                // nova pozice pro 'Ellipse'
                Canvas.SetLeft(ellipse, pos.X - ellipse.ActualWidth / 2);
                Canvas.SetTop(ellipse, pos.Y - ellipse.ActualHeight / 2);

                if ((pos.X < 10) || (pos.X > (canvas.ActualWidth - 10))) { return; }
                if ((pos.Y < 10) || (pos.Y > (canvas.ActualHeight - 10))) { return; }

                // nova pozice pro 'Servo(s)' v '%'
                double posX = ((pos.X - (canvas.ActualWidth / 2)) / (canvas.ActualWidth / 2) * 100.0);
                double posY = ((pos.Y - (canvas.ActualHeight / 2)) / (canvas.ActualHeight / 2) * 100.0);
                posY *= -1.0;
                LaserPosPercent(EnLaserAction.Move, posX, posY);
            }
        }
        // pusteni 'Ellipse'
        public static void LaserMoveEnd(Ellipse ellipse)
        {
            LaserPosPercent(EnLaserAction.End);
            ellipse.ReleaseMouseCapture();
            laserMove = false;
        }

        // zmena 'On' / 'Off' pro laser
        public enum EnLaserState { Off, On, Toggle };
        public static bool LaserOnOff(EnLaserState state)
        {
            switch(state)
            {
                case EnLaserState.Off: { LaserOn = false; break; }
                case EnLaserState.On: { LaserOn = true; break; }
                case EnLaserState.Toggle: { LaserOn = !LaserOn; break; }
            }
            UpdateDevice(EnDeviceIdent.Laser);
            return LaserOn;
        }

        // --- motor ---

        public enum EnFloorAction { Right, Left, Stop, Zero }

        // pohyb / otaceni podlahou
        public static void FloorMove(EnFloorAction action)
        {
            sbyte motorMoveAction = sbyte.MaxValue;
            UInt16 motorRotaryValue = UInt16.MaxValue;
            // hodnota povelu pro pohyb motoru
            switch(action)
            {
                case EnFloorAction.Left: { motorMoveAction = -1; break; }
                case EnFloorAction.Stop: { motorMoveAction = 0; break; }
                case EnFloorAction.Right: { motorMoveAction = +1; break; }
                case EnFloorAction.Zero: { motorRotaryValue = 0; break; }
            }
            // vykonani pohybu motoru
            if (EnableHW == true)
            {
                if (motorMoveAction != sbyte.MaxValue)
                {
                    DataUtils.GetValue<ItmDataInt8>(EnDeviceIdent.Motor, (byte)EnMotorElement.Move, 1).Value = motorMoveAction;
                    UpdateDevice(EnDeviceIdent.Motor);
                }
                if (motorRotaryValue != UInt16.MaxValue)
                {
                    DataUtils.GetValue<ItmDataInt32>(EnDeviceIdent.Motor, (byte)EnMotorElement.Rotary, 2).Value = motorRotaryValue;
                    UpdateDevice(EnDeviceIdent.Motor);
                }
            }
        }

        // aktualni otoceni podlahy
        public static Int32 FloorRotation
        {
            get
            {
                if (EnableHW == false) { return 0; }
                return DataUtils.GetValue<ItmDataInt32>(EnDeviceIdent.Motor, (byte)EnMotorElement.Rotary, 1).Value;
            }
        }

        // aktualni stav nuloveho kontaktu
        public static bool FloorSwitchZero
        {
            get
            {
                if (EnableHW == false) { return false; }
                return DataUtils.GetValue<ItmDataBool>(EnDeviceIdent.Motor, (byte)EnMotorElement.Zero, 1).Value;
            }
        }

        // --- Anchor(s) ---

        public enum EnImageParms : byte { Set = 0 /* toto nastavit */, Or = 16 /* toto pridat */, NonAnd = 32 /* toto smazat */, NoShow = 128 }

        // zaslani obrazku do jednoho zarizeni
        public static int AnchorSetImage(int number /* 1 - 8 */, string imageName /* nazev obrazku uvnitr souboru, nikoli nazev souboru */,
            EnImageParms prms = EnImageParms.Set /* parametry pro zobrazeni */)
        {
            // pokud neni povolen HW, tak navrat
            if (EnableHW == false) { return -1; }
            // vytvoreni aktualniho 'Ident' pro 'Device'
            string anchorIdent = (int.Parse(DevsCmns.EnDeviceIdent.AnchorsBase) + number).ToString("D8");
            // nalezeni obrazku podle jeho nazvu
            OneImage omeImage = DevsMain.images.FirstOrDefault(item => item.Name == imageName);
            // pokud obrazek nenalezen, tak chyba
            if (omeImage == null) { return -2; }
            // vlozeni parametru do prenasenych dat (do bit7-bit4 v typu dat)
            omeImage.Values[0] = (byte)((omeImage.Values[0] & 0x0F) | (byte)prms);
            // zaslani obrazku na externi zarizeni (pokud je platny)
            try { DataUtils.GetValue<ItmDataBytes>(anchorIdent, 1, 1).SetValue(omeImage.Values, 0, omeImage.Values.Length); }
            catch { return -3; }
            // skutecne zaslani dat do zarizeni
            UpdateDevice(anchorIdent);
            // vyreseno
            return 0;
        }

        // zaslani ruznych obrazku do nekolika zarizeni (pripadne zhasnuti vsech zarizeni)
        // 'null' -> zhasnuti vsech obrazku
        // 'Tuple<number, imageName>' -> seznam 'Anchor' a pro ne obrazku
        public static void AnchorSetImage(params Tuple<int, string>[] anchorAndImage)
        {
            // pokud neni povolen HW, tak navrat
            if (EnableHW == false) { return; }
            // pokud neni predan zadny parametr, tak jde o zhasnuti vsech 'Anchor(s)'
            bool resetAll = (anchorAndImage == null);
            // projiti vsech 'Anchor(s)'
            for (int i = 1; i <= 8; i++)
            {
                // seriove cislo pro akrualni 'Anchor'
                string anchorIdent = (int.Parse(DevsCmns.EnDeviceIdent.AnchorsBase) + i).ToString("D8");
                // vyzvednuti tohoto zarizeni
                ItmDevice device = DataUtils.GetDevice(anchorIdent);
                // pokud neni dostupne, tak preskocit
                if (device == null) { continue; }
                // pokud jde o 'Reset' pro vsecna zarizeni
                if (resetAll == true)
                {
                    // zaslani povelu 'Reset' do zarizeni
                    UniTestsHW.Communs.CmnMain.CmdWrite(device /* tomuto zarizeni */,
                        (byte)UniTestsHW.Communs.CmnConsts.EnPaketCmd.Mode, (byte)UniTestsHW.Communs.CmnConsts.EnPaketCmdModeSub.Device, cmdDataReset);
                }
                // pokud jde o zsalni obrazku
                else
                {
                    // nalezeni parametru jako 'Tuple' pro tento 'Anchor'
                    Tuple<int, string> tuple = anchorAndImage.FirstOrDefault(item => item.Item1 == i);
                    // pouze pokud je nalezen
                    if (tuple != null)
                    {
                        // nalezeni obrazku podle jeho nazvu
                        OneImage omeImage = DevsMain.images.FirstOrDefault(item => item.Name == tuple.Item2);
                        if (omeImage != null)
                        {
                            // zapis obrazku na externi zarizeni (zatim pouze jako data)
                            DataUtils.GetValue<ItmDataBytes>(anchorIdent, 1, 1).SetValue(omeImage.Values, 0, omeImage.Values.Length);
                            // skutecne zaslani dat do zarizeni
                            UpdateDevice(anchorIdent);
                        }
                    }
                }
            }
        }

        // --- ---

        // zapis zmen do HW 'Device(s)' / 'Element(s)'
        public static void UpdateDevice(string devIdent)
        {
            if (EnableHW == true)
                { DataMain.DataWrite(DataUtils.GetDevice(devIdent)); }
        }
    }
}
