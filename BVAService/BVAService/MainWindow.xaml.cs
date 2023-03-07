// .NET
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
// UniTest
using UniTestsHW.Data;

namespace BVAService
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public bool FromExternalApp { get; set; } = false;

        // zda se tento dialog spusteny z externi aplikace zavira nebo pouze zkryva
        public bool NoCloseOnlyHide { get; set; } = false;

        //private MediaPlayer mediaPlayer = new MediaPlayer();

        public MainWindow()
        {
            InitializeComponent();
        }

        // zobrazeni aplikace
        private void WindowOnLoaded(object sender, RoutedEventArgs e)
        {
            // vykonat pouze pokud 'Service' nespousti jina aplikace
            if (FromExternalApp == false)
            {
                // cesta k servisnim souborum
                UniTestsHW.Global.GlobalConsts.PathDevsElmsInfo = "../../../../../DataDevsElmsInfo";
                // cesta k obrazkum
                BVAService.Global.GlobalConsts.PathDataImages = "../../../../../DataImages";
                // HW zarizeni jsou povoleny
                Devices.DevsCmns.EnableHW = true;
                // nalezeni vsech USB-HID zarizeni
                /* string[] devsStr = */ Devices.DevsCmns.FindAllDevice();
                // zobrazeni seznamu nalezeni / nenalezenych zarizeni
                Devices.DevsMain.ShowDevicesStatus();
            }

            // musi se odblokovat casti dialogu podle nalezenych zarizeni
            foreach (string ident in Devices.DevsCmns.DevsSerNumToName.Keys)
            {
                // test zda je toto zarizeni pripojeno / nalezeno
                ItmDevice itmDevice = (ItmDevice)DataMain.devices.FirstOrDefault(item => item.DeviceConfig.Ident == ident);
                // nalezeno / pripojeno
                if (itmDevice != null)
                {
                    // povoleni / blokovani casti GUI podle typu nalezenych externich zarizeni
                    // Laser
                    if (Devices.DevsCmns.DevsSerNumToName[itmDevice.Ident] == "Laser") { rectLaserEnable.Visibility = Visibility.Collapsed; }
                    // Motor
                    if (Devices.DevsCmns.DevsSerNumToName[itmDevice.Ident] == "Motor") { rectMotorEnable.Visibility = Visibility.Collapsed; }
                    // Anchor(s)
                    if (Devices.DevsCmns.DevsSerNumToName[itmDevice.Ident].StartsWith("Anchor") == true) { rectAnchorEnable.Visibility = Visibility.Collapsed; }
                }
            }

            // nacteni dat pro test (toto je pouze pro inetrni test, ne pro externi aplikaci)
            string[] imagesNames = Devices.DevsMain.LoadImages(BVAService.Global.GlobalConsts.PathDataImages);
            // seznam dostupnych 'Anchor'
            lvDevAnchors.ItemsSource = Devices.DevsMain.devsAnchor;

            //// testovaci cilove pozice pro 'Laser'
            //lvLaserPoss.ItemsSource = new Devices.LaserPos[]
            //{
            //    new Devices.LaserPos() { Name = "TestPos1", PosX = 0, PosY = 0 },
            //    new Devices.LaserPos() { Name = "TestPos2", PosX = 50, PosY = 0 },
            //    new Devices.LaserPos() { Name = "TestPos3", PosX = -50, PosY = 0 },
            //    new Devices.LaserPos() { Name = "TestPos4", PosX = 0, PosY = 50 },
            //    new Devices.LaserPos() { Name = "TestPos5", PosX = 0, PosY = -50 },
            //};

            // zmena hodnoty v nejakem 'Device' (pro manual)
            UniTestsHW.Communs.CmnMain.OnDeviceChangedUp = OnDeviceChanged;
        }

        // ukonceni aplikace
        private void WindowOnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // pokud se 'Test' (aplikace uplne) ukoncuje, tak se zavrou vsechny 'Device(s)'
            if (NoCloseOnlyHide == false)
                // skutecne uzavreni pouze pokud neni 'Sevice' volano externi apliakci
                { if (FromExternalApp == false) { UniTestsHW.Communs.CmnMain.CloseAllDevice(); } }
            // pokud se 'Test' pouze skryva, tak se skryje dialog
            if (NoCloseOnlyHide == true) { e.Cancel = true; this.Hide(); }
        }

        // --- manual ---

        // zda jsou / nejsou zariznei / povolena
        //private bool DevicesEnable = false;

        // spusteni / zastaveni (stisk tlacitka 'Enable' / 'Disable')
        private void ManualModeOnClick(object sender, RoutedEventArgs e)
        {
            // vyzvednuti pozadovane akce
            string actionStr = (string)((Button)sender).Tag;
            // prevod 'String' na 'Enum'
            Devices.DevsCmns.EnMainAction action = Enum.Parse<Devices.DevsCmns.EnMainAction>(actionStr);
            // stav povoleni / blokovani zarizeni
            //DevicesEnable = (action == Devices.DevsCmns.EnMainAction.Enable);
            // spusteni / zastaveni vsech externich zarizeni
            Devices.DevsCmns.SetMode(action);

            bEnable.IsEnabled = (action == Devices.DevsCmns.EnMainAction.Disable);
            bDisable.IsEnabled = (action == Devices.DevsCmns.EnMainAction.Enable);
        }

        // vyvolani pomocneho dialogu pro primou komunikaci s HW
        private void ManulServiceOnClick(object sender, RoutedEventArgs e)
            { UniTestsHW.App.AppExt.ShowDebug(this); }

        // --

        // zmena stavu nektereho vystupu 'Anchor'
        private void DevAnchorStateChanged(object sender, EventArgs e)
        {
            // odkaz na zmeneny 'Anchor'
            Devices.DeviceAnchor devAnchor = (Devices.DeviceAnchor)((ComboBox)sender).Tag;
            // zaslani obrazku do zarizeni (cislo zarizeni, nazev obrazku)
            Devices.DevsCmns.AnchorSetImage(devAnchor.Number, devAnchor.Value);
        }

        // --

        // zobrazeni seznamu dostupnych svukovych souboru
        private void SoundToPlayOnDropDownOpened(object sender, EventArgs e)
        {
            //try
            //{
            //    // seznam adresaru (cele cesty)
            //    cbSoundToPlay.ItemsSource = System.IO.Directory.GetFiles(System.IO.Path.Combine(BVATests.Tests.TestMain.filePath, BVATests.Tests.TestMain.testActualDir)).
            //        Where(f => System.IO.Path.GetExtension(f) != ".xml").
            //        // vyzvednuti pouze nazvy koncoveho adresare
            //        Select(item => System.IO.Path.GetFileName(item)).
            //        // vraceni jako pole
            //        ToArray();
            //}
            //catch { }
        }

        // prehrani vybraneho zvukoveho souboru
        private void SoundToPlayOnClick(object sender, RoutedEventArgs e)
        {
            //mediaPlayer.Open(new Uri(System.IO.Path.Combine(Tests.TestMain.filePath, Tests.TestMain.testActualDir, (string)cbSoundToPlay.SelectedItem), UriKind.Relative));
            //mediaPlayer.Play();
        }

        // --

        // uchopeni 'Ellipse'
        private void ManualLaserOnMouseDown(object sender, MouseButtonEventArgs e)
        { Devices.DevsCmns.LaserMoveStart(ePosition); }
        // pohyb s 'Ellipse' po 'Canvas'
        private void ManualLaserOnMouseMove(object sender, MouseEventArgs e)
        {
            if (Devices.DevsCmns.laserMove == true)
            {
                Devices.DevsCmns.LaserMoveProcess(e.GetPosition(cPosition), ePosition, cPosition);
                manualLaserPos.Content = "Pos: " + Devices.DevsCmns.LaserPosX + " ; " + Devices.DevsCmns.LaserPosY;
            }
        }
        // pusteni 'Ellipse'
        private void ManualLaserOnMouseUp(object sender, MouseButtonEventArgs e)
        { Devices.DevsCmns.LaserMoveEnd(ePosition); }

        // vykonani cinnosti testu
        private void ManualLaserOnOffOnClick(object sender, RoutedEventArgs e)
        {
            ePosition.Fill = (Devices.DevsCmns.LaserOnOff(Devices.DevsCmns.EnLaserState.Toggle) == false) ? Brushes.Black : Brushes.Red;
        }

        // nastaveni pred-definovane pozice pro laser
        private void ManualLaserSetLocationOnClick(object sender, RoutedEventArgs e)
        {
            // vyzvednuti oznacene pred-definovane pozice
            Devices.LaserPos laserPos = (Devices.LaserPos)lvLaserPoss.SelectedItem;
            // pokud neni nic oznaceno, tak navrat
            if (laserPos == null) { return; }
            // nastaveni vybrae pozice
            Devices.DevsCmns.LaserPosPercent(Devices.DevsCmns.EnLaserAction.Position, laserPos.PosX, laserPos.PosY);
        }

        // ovladani motoru
        private void ManualMotorControlOnClick(object sender, RoutedEventArgs e)
        {
            // vyzvednuti typu cinnosti (stisknute tlacitko)
            string actionStr = (string)((Button)sender).Tag;
            // vykonani cinnosti
            Devices.DevsCmns.EnFloorAction floorAction = Enum.Parse<Devices.DevsCmns.EnFloorAction>(actionStr);
            Devices.DevsCmns.FloorMove(floorAction);
        }

        // zmena hodnoty v nejakem 'Device'
        public void OnDeviceChanged(ItmDevice device)
        {
            if (device.Ident == Devices.DevsCmns.EnDeviceIdent.Motor)
            {
                bool zero = DataUtils.GetValue<ItmDataBool>(
                    Devices.DevsCmns.EnDeviceIdent.Motor, (byte)Devices.DevsCmns.EnMotorElement.Zero, 1).Value;
                Int32 rotary = DataUtils.GetValue<ItmDataInt32>(
                    Devices.DevsCmns.EnDeviceIdent.Motor, (byte)Devices.DevsCmns.EnMotorElement.Rotary, 1).Value;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    manualZeroYes.Fill = (zero == false) ? Brushes.Gray : Brushes.Red;
                    manualRotaryPos.Text = rotary.ToString();
                });
            }
        }
    }
}
