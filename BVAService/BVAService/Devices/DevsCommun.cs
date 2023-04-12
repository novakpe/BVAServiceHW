
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
using UniTestsHW.Communs;
using UniTestsHW.Data;

namespace BVAService.Devices
{
    public class DevsCmns
    {
        private static byte[] cmdDataReset = new byte[] { (byte)CmnConsts.EnPaketCmdModeData.ResetSW };

        public enum EnHwDevType { None, Real, Simul }
        public static EnHwDevType EnableHW { get; set; } = EnHwDevType.None;

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
            // pokud se pouzivaji realna zarizeni
            if (EnableHW == EnHwDevType.Real)
            {
                // nalezeni vsech dostupnych zarizeni
                int count = UniTestsHW.Communs.CmnMain.FindAllDevice();
                // pokud neni zadne zarizeni, tak rovno navrat
                if (count == 0) { return null; }
            }

            // pokud se pouzivaji simulovana zarizeni
            if (EnableHW == EnHwDevType.Simul)
            {
                // simulovane zarizeni 'Laser'
                // celkove zarizeni
                ItmDevice simulDevLaser = new ItmDevice() { Ident = "00000011", CmnType = CmnConsts.EnCmnType.None, CmnObject = null };
                UniTestsHW.Configs.ConfigMain.LoadConfig(simulDevLaser);
                DataMain.devices.Add(simulDevLaser);
                // 'Element' - Pozice
                ItmElement simulDevLaserElmPosXY = new ItmElement() { Ident = 1, RefDevice = simulDevLaser };
                simulDevLaserElmPosXY.ElmStatus = DataConsts.EnElmStatus.OK;
                simulDevLaserElmPosXY.ElmType = DataConsts.EnElmType.None;
                simulDevLaser.Elements.Add(simulDevLaserElmPosXY);
                // 'Data' - PosX (Int8) a PosY (Int8)
                ItmDataInt8 simulDevLaserElmDataPosX = new ItmDataInt8() { Ident = 1, DataType = DataConsts.EnDataType.Int8, Dir = CmnConsts.EnDir.Out, RefElement = simulDevLaserElmPosXY };
                simulDevLaserElmPosXY.Items.Add(simulDevLaserElmDataPosX);
                ItmDataInt8 simulDevElmDataPosY = new ItmDataInt8() { Ident = 2, DataType = DataConsts.EnDataType.Int8, Dir = CmnConsts.EnDir.Out, RefElement = simulDevLaserElmPosXY };
                simulDevLaserElmPosXY.Items.Add(simulDevElmDataPosY);
                // 'Element' - OnOff
                ItmElement simulDevLaserElmOnOff = new ItmElement() { Ident = 2, RefDevice = simulDevLaser };
                simulDevLaserElmOnOff.ElmStatus = DataConsts.EnElmStatus.OK;
                simulDevLaserElmOnOff.ElmType = DataConsts.EnElmType.None;
                simulDevLaser.Elements.Add(simulDevLaserElmOnOff);
                // 'Data' - OnOff (Bool)
                ItmDataBool simulDevLaserElmDataOnOff = new ItmDataBool() { Ident = 1, DataType = DataConsts.EnDataType.Bool, Dir = CmnConsts.EnDir.Out, RefElement = simulDevLaserElmOnOff };
                simulDevLaserElmOnOff.Items.Add(simulDevLaserElmDataOnOff);

                // simulovane zarizeni 'Motor'
                // celkove zarizeni
                ItmDevice simulDevMotor = new ItmDevice() { Ident = "00000012", CmnType = CmnConsts.EnCmnType.None, CmnObject = null };
                UniTestsHW.Configs.ConfigMain.LoadConfig(simulDevMotor);
                DataMain.devices.Add(simulDevMotor);
                // 'Element' - Pohyb
                ItmElement simulDevMotorElmMove = new ItmElement() { Ident = 1, RefDevice = simulDevMotor };
                simulDevMotorElmMove.ElmStatus = DataConsts.EnElmStatus.OK;
                simulDevMotorElmMove.ElmType = DataConsts.EnElmType.None;
                simulDevMotor.Elements.Add(simulDevMotorElmMove);
                // 'Data' - Pohyb (Int8)
                ItmDataInt8 simulDevMotorElmDataPosX = new ItmDataInt8() { Ident = 1, DataType = DataConsts.EnDataType.Int8, Dir = CmnConsts.EnDir.Out, RefElement = simulDevLaserElmPosXY };
                simulDevMotorElmMove.Items.Add(simulDevMotorElmDataPosX);
                // 'Element' - Zero Contact
                ItmElement simulDevMotorElmZero = new ItmElement() { Ident = 2, RefDevice = simulDevMotor };
                simulDevMotorElmZero.ElmStatus = DataConsts.EnElmStatus.OK;
                simulDevMotorElmZero.ElmType = DataConsts.EnElmType.None;
                simulDevMotor.Elements.Add(simulDevMotorElmZero);
                // 'Data' - Zero Contact (Bool)
                ItmDataBool simulDevMotorElmDataZero = new ItmDataBool() { Ident = 1, DataType = DataConsts.EnDataType.Bool, Dir = CmnConsts.EnDir.In, RefElement = simulDevLaserElmOnOff };
                simulDevMotorElmZero.Items.Add(simulDevMotorElmDataZero);
                // 'Element' - Rotate Count
                ItmElement simulDevMotorElmCount = new ItmElement() { Ident = 3, RefDevice = simulDevMotor };
                simulDevMotorElmCount.ElmStatus = DataConsts.EnElmStatus.OK;
                simulDevMotorElmCount.ElmType = DataConsts.EnElmType.None;
                simulDevMotor.Elements.Add(simulDevMotorElmCount);
                // 'Data' - Rotate Count (Bool)
                ItmDataInt32 simulDevMotorElmDataCount = new ItmDataInt32() { Ident = 1, DataType = DataConsts.EnDataType.Int32, Dir = CmnConsts.EnDir.InOut, RefElement = simulDevLaserElmOnOff };
                simulDevMotorElmCount.Items.Add(simulDevMotorElmDataCount);

                // simulovane zarizeni 'Anchor'
                for (int i = 21; i < 29; i++)
                {
                    // celkove zarizeni
                    ItmDevice simulDevAnchor = new ItmDevice() { Ident = i.ToString("D8") /* "00000021" */, CmnType = CmnConsts.EnCmnType.None, CmnObject = null };
                    UniTestsHW.Configs.ConfigMain.LoadConfig(simulDevAnchor);
                    DataMain.devices.Add(simulDevAnchor);
                    // 'Element' - Image
                    ItmElement simulDevAnchorElmImage = new ItmElement() { Ident = 1, RefDevice = simulDevAnchor };
                    simulDevAnchorElmImage.ElmStatus = DataConsts.EnElmStatus.OK;
                    simulDevAnchorElmImage.ElmType = DataConsts.EnElmType.None;
                    simulDevAnchor.Elements.Add(simulDevAnchorElmImage);
                    // 'Data' - Image (Bytes)
                    ItmDataInt32 simulDevAnchorElmDataImage = new ItmDataInt32() { Ident = 1, DataType = DataConsts.EnDataType.Int32, Dir = CmnConsts.EnDir.InOut, RefElement = simulDevLaserElmOnOff };
                    simulDevAnchorElmImage.Items.Add(simulDevAnchorElmDataImage);
                }
            }

            // pokud se nepouzivaji zadna zarizeni
            if (EnableHW == EnHwDevType.None) { }

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

        // spusteni / zastaveni vsech externich zarizeni
        public static void SetMode(EnMainAction action)
        {
            if (EnableHW != EnHwDevType.None)
            {
                byte[] cmdData = null;
                // vytvoreni seznamu cinnosti pro vykonani
                switch(action)
                {
                    case EnMainAction.Enable: { cmdData = new byte[]
                        { (byte)CmnConsts.EnPaketCmdModeData.ResetSW, (byte)CmnConsts.EnPaketCmdModeData.Enable }; break; }
                    case EnMainAction.Disable: { cmdData = new byte[]
                        { (byte)CmnConsts.EnPaketCmdModeData.Disable, (byte)CmnConsts.EnPaketCmdModeData.ResetSW }; break; }
                }
                // zaslani povelu do zarizeni
                CmnMain.CmdWrite(null /* vsecm 'Device(s)' */,
                    (byte)CmnConsts.EnPaketCmd.Mode, (byte)CmnConsts.EnPaketCmdModeSub.Device, cmdData);

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

        public static sbyte LaserPosX // Value = -/+100
        {
            get
            {
                if (EnableHW == EnHwDevType.None) { return 0; }
                return DataUtils.GetValue<ItmDataInt8>(EnDeviceIdent.Laser, (byte)EnLaserElement.PosXY, 1).Value;
            }
            set
            {
                if (EnableHW != EnHwDevType.None)
                    { DataUtils.GetValue<ItmDataInt8>(EnDeviceIdent.Laser, (byte)EnLaserElement.PosXY, 1).Value = value; }
            }
        }

        public static sbyte LaserPosY // Value = -/+100
        {
            get
            {
                if (EnableHW == EnHwDevType.None) { return 0; }
                return DataUtils.GetValue<ItmDataInt8>(EnDeviceIdent.Laser, (byte)EnLaserElement.PosXY, 2).Value;
            }
            set
            {
                if (EnableHW != EnHwDevType.None)
                    { DataUtils.GetValue<ItmDataInt8>(EnDeviceIdent.Laser, (byte)EnLaserElement.PosXY, 2).Value = value; }
            }
        }

        public static bool LaserOn // Value = 0/1
        {
            get
            {
                if (EnableHW == EnHwDevType.None) { return false; }
                return DataUtils.GetValue<ItmDataBool>(EnDeviceIdent.Laser, (byte)EnLaserElement.OnOff, 1).Value;
            }
            set
            {
                if (EnableHW != EnHwDevType.None)
                    { DataUtils.GetValue<ItmDataBool>(EnDeviceIdent.Laser, (byte)EnLaserElement.OnOff, 1).Value = value; }
            }
        }

        // --- obsluha z GUI ---

        // zda se laser prave pohybuje
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
                // nova pozice pro 'Ellipse' na 'Canvas'
                Canvas.SetLeft(ellipse, pos.X - ellipse.ActualWidth / 2);
                Canvas.SetTop(ellipse, pos.Y - ellipse.ActualHeight / 2);
                // pokud je pozice mimo platnou plochu, tak konec
                if ((pos.X < 10) || (pos.X > (canvas.ActualWidth - 10))) { return; }
                if ((pos.Y < 10) || (pos.Y > (canvas.ActualHeight - 10))) { return; }
                // nova pozice pro 'Servo(s)' v '%' (-/+100)
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
            if (EnableHW != EnHwDevType.None)
            {
                if (motorMoveAction != sbyte.MaxValue)
                {
                    DataUtils.GetValue<ItmDataInt8>(EnDeviceIdent.Motor, (byte)EnMotorElement.Move, 1).Value = motorMoveAction;
                    UpdateDevice(EnDeviceIdent.Motor);
                }
                if (motorRotaryValue != UInt16.MaxValue)
                {
                    DataUtils.GetValue<ItmDataInt32>(EnDeviceIdent.Motor, (byte)EnMotorElement.Rotary, 1).Value = motorRotaryValue;
                    UpdateDevice(EnDeviceIdent.Motor);
                }
            }
        }

        // aktualni otoceni podlahy
        public static Int32 FloorRotation
        {
            get
            {
                if (EnableHW == EnHwDevType.None) { return 0; }
                return DataUtils.GetValue<ItmDataInt32>(EnDeviceIdent.Motor, (byte)EnMotorElement.Rotary, 1).Value;
            }
        }

        // aktualni stav nuloveho kontaktu
        public static bool FloorSwitchZero
        {
            get
            {
                if (EnableHW != EnHwDevType.None) { return false; }
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
            if (EnableHW == EnHwDevType.None) { return -1; }
            // vytvoreni aktualniho 'Ident' pro 'Device'
            string anchorIdent = (int.Parse(EnDeviceIdent.AnchorsBase) + number).ToString("D8");
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
            if (EnableHW == EnHwDevType.None) { return; }
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
                    CmnMain.CmdWrite(device /* tomuto zarizeni */,
                        (byte)CmnConsts.EnPaketCmd.Mode, (byte)CmnConsts.EnPaketCmdModeSub.Device, cmdDataReset);
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
            if (EnableHW != EnHwDevType.None)
                { DataMain.DataWrite(DataUtils.GetDevice(devIdent)); }
        }
    }
}
