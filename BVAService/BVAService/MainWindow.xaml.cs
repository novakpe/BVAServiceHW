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
using BVAService.Devices;

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
                // cesta k obrazkum a pozicim
                BVAService.Global.GlobalConsts.PathDataImages = "../../../../../DataImages";
                BVAService.Global.GlobalConsts.PathDataPositions = "../../../../../DataPositions";
                // HW zarizeni jsou povoleny
                //DevsCmns.EnableHW = DevsCmns.EnHwDevType.None;
                DevsCmns.EnableHW = DevsCmns.EnHwDevType.Simul;
                //DevsCmns.EnableHW = DevsCmns.EnHwDevType.Real;
                // nalezeni vsech USB-HID zarizeni
                /* string[] devsStr = */
                DevsCmns.FindAllDevice();
                // zobrazeni seznamu nalezeni / nenalezenych zarizeni
                DevsMain.ShowDevicesStatus();
            }

            // musi se odblokovat casti dialogu podle nalezenych zarizeni
            foreach (string ident in DevsCmns.DevsSerNumToName.Keys)
            {
                // test zda je toto zarizeni pripojeno / nalezeno
                ItmDevice itmDevice = (ItmDevice)DataMain.devices.FirstOrDefault(item => item.DeviceConfig.Ident == ident);
                // nalezeno / pripojeno
                if (itmDevice != null)
                {
                    // povoleni / blokovani casti GUI podle typu nalezenych externich zarizeni
                    // Laser
                    if (DevsCmns.DevsSerNumToName[itmDevice.Ident] == "Laser") { rectLaserEnable.Visibility = Visibility.Collapsed; }
                    // Motor
                    if (DevsCmns.DevsSerNumToName[itmDevice.Ident] == "Motor") { rectMotorEnable.Visibility = Visibility.Collapsed; }
                    // Anchor(s)
                    if (DevsCmns.DevsSerNumToName[itmDevice.Ident].StartsWith("Anchor") == true) { rectAnchorEnable.Visibility = Visibility.Collapsed; }
                }
            }

            // Test
            //rectLaserEnable.Visibility = rectMotorEnable.Visibility = rectAnchorEnable.Visibility = Visibility.Collapsed;

            // nacteni dat pro test (toto je pouze pro inetrni test, ne pro externi aplikaci)
            string[] imagesNames = DevsMain.LoadImages(BVAService.Global.GlobalConsts.PathDataImages);
            string[] positionsNames = DevsMain.LoadPositions(BVAService.Global.GlobalConsts.PathDataPositions);
            lvLaserPoss.ItemsSource = DevsMain.laserPositions;
            // seznam dostupnych 'Anchor'
            lvDevAnchors.ItemsSource = DevsMain.devsAnchor;

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

        // --- rizeni manualniho rezimu ---

        // zda jsou / nejsou zariznei / povolena
        //private bool DevicesEnable = false;

        // spusteni / zastaveni (stisk tlacitka 'Enable' / 'Disable')
        private void ManualModeOnClick(object sender, RoutedEventArgs e)
        {
            // vyzvednuti pozadovane akce
            string actionStr = (string)((Button)sender).Tag;
            // prevod 'String' na 'Enum'
            DevsCmns.EnMainAction action = Enum.Parse<DevsCmns.EnMainAction>(actionStr);
            // stav povoleni / blokovani zarizeni
            //DevicesEnable = (action == Devices.DevsCmns.EnMainAction.Enable);
            // spusteni / zastaveni vsech externich zarizeni
            DevsCmns.SetMode(action);

            bEnable.IsEnabled = (action == DevsCmns.EnMainAction.Disable);
            bDisable.IsEnabled = (action == DevsCmns.EnMainAction.Enable);
        }

        // vyvolani pomocneho dialogu pro primou komunikaci s HW
        private void ManulServiceOnClick(object sender, RoutedEventArgs e)
            { UniTestsHW.App.AppExt.ShowDebug(this); }

        // --- kotvy / obrazky ---

        // zmena stavu nektereho vystupu 'Anchor'
        private void DevAnchorStateChanged(object sender, EventArgs e)
        {
            // odkaz na zmeneny 'Anchor'
            DeviceAnchor devAnchor = (DeviceAnchor)((ComboBox)sender).Tag;
            // zaslani obrazku do zarizeni (cislo zarizeni, nazev obrazku)
            DevsCmns.AnchorSetImage(devAnchor.Number, devAnchor.Value);
        }

        // --- zvuk ---

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

        // --- laser ---

        // uchopeni 'Ellipse'
        private void ManualLaserOnMouseDown(object sender, MouseButtonEventArgs e)
            { DevsCmns.LaserMoveStart(ePosition); }
        // pohyb s 'Ellipse' po 'Canvas'
        private void ManualLaserOnMouseMove(object sender, MouseEventArgs e)
        {
            if (DevsCmns.laserMove == true)
            {
                DevsCmns.LaserMoveProcess(e.GetPosition(cPosition), ePosition, cPosition);
                manualLaserPos.Content = "Pos: X = " + DevsCmns.LaserPosX + " / Y = " + DevsCmns.LaserPosY;
            }
        }
        // pusteni 'Ellipse'
        private void ManualLaserOnMouseUp(object sender, MouseButtonEventArgs e)
            { DevsCmns.LaserMoveEnd(ePosition); }

        // vykonani cinnosti testu
        private void ManualLaserOnOffOnClick(object sender, RoutedEventArgs e)
        {
            ePosition.Fill = (DevsCmns.LaserOnOff(DevsCmns.EnLaserState.Toggle) == false) ? Brushes.Black : Brushes.Red;
        }

        // krok laserem do pozadovaneho smeru
        private void ManualLaserStepOnClick(object sender, RoutedEventArgs e)
        {
            // vyzvednuti pozadovane akce
            string actStr = (string)((Button)sender).Tag;
            // vykonani pozadovane akce (posun o jeden krok pozadovanym smerem)
            switch(actStr)
            {
                case "Left": { if (DevsCmns.LaserPosX > -100) { DevsCmns.LaserPosX--; } break; }
                case "Up": { if (DevsCmns.LaserPosY < 100) { DevsCmns.LaserPosY++; } break; }
                case "Down": { if (DevsCmns.LaserPosY > -100) { DevsCmns.LaserPosY--; } break; }
                case "Right": { if (DevsCmns.LaserPosX < 100) { DevsCmns.LaserPosX++; } break; }
            }
            // zobrazeni nove / aktualni pozice
            manualLaserPos.Content = "Pos: X = " + DevsCmns.LaserPosX + " / Y = " + DevsCmns.LaserPosY;
            // prepocet pozice na plochu 'Canvas'
            double posCanX = (DevsCmns.LaserPosX / 100.0) * (cPosition.ActualWidth / 2) + (cPosition.ActualWidth / 2);
            double posCanY = (-DevsCmns.LaserPosY / 100.0) * (cPosition.ActualHeight / 2) + (cPosition.ActualHeight / 2);
            // nova pozice pro 'Ellipse' na 'Canvas' (podle krokoveho posunu)
            Canvas.SetLeft(ePosition, posCanX - ePosition.ActualWidth / 2);
            Canvas.SetTop(ePosition, posCanY - ePosition.ActualHeight / 2);
        }

        // vyber polozky z 'ListView'
        private void ManualLasetSelectNamedPosition(object sender, MouseButtonEventArgs e)
        {
            // vyzvednuti oznacene pred-definovane pozice
            LaserPos laserPos = (LaserPos)lvLaserPoss.SelectedItem;
            // pokud neni nic oznaceno, tak navrat
            if (laserPos == null) { return; }
            // nastaveni vybrae pozice
            DevsCmns.LaserPosPercent(DevsCmns.EnLaserAction.Position, laserPos.PosX, laserPos.PosY);

            // zobrazeni nove / aktualni pozice
            manualLaserPos.Content = "Pos: X = " + DevsCmns.LaserPosX + " / Y = " + DevsCmns.LaserPosY;
            // prepocet pozice na plochu 'Canvas'
            double posCanX = (DevsCmns.LaserPosX / 100.0) * (cPosition.ActualWidth / 2) + (cPosition.ActualWidth / 2);
            double posCanY = (-DevsCmns.LaserPosY / 100.0) * (cPosition.ActualHeight / 2) + (cPosition.ActualHeight / 2);
            // nova pozice pro 'Ellipse' na 'Canvas' (podle krokoveho posunu)
            Canvas.SetLeft(ePosition, posCanX - ePosition.ActualWidth / 2);
            Canvas.SetTop(ePosition, posCanY - ePosition.ActualHeight / 2);
        }

        // odtraneni polozky / pozice ze seznamu
        private void ManualLasetDeteleNamedPosition(object sender, RoutedEventArgs e)
        {
            // vyzvednuti oznacene pred-definovane pozice
            LaserPos laserPos = (LaserPos)lvLaserPoss.SelectedItem;
            // nutno vyzvednout obsah 'ItemsSource' jako 'IList'
            var itemsSource = lvLaserPoss.ItemsSource as IList<LaserPos>;
            if (itemsSource != null) itemsSource.Remove(laserPos);
            lvLaserPoss.ItemsSource = null; lvLaserPoss.ItemsSource = itemsSource;
            // ulozeni novych pozic
            DevsMain.SavePositionsInt(BVAService.Global.GlobalConsts.PathDataPositions);
        }

        // nastaveni pred-definovane pozice pro laser
        private void ManualLaserAddLocationOnClick(object sender, RoutedEventArgs e)
        {
            lvLaserPoss.ItemsSource = null;
            // pridani nove pozice do seznamu
            DevsMain.laserPositions.Add(new LaserPos() { PosX = DevsCmns.LaserPosX, PosY = DevsCmns.LaserPosY, Name = tbLasPosName.Text });
            // aktualizace zobrazeni seznamu
            lvLaserPoss.ItemsSource = DevsMain.laserPositions;
            // ulozeni novych pozic
            DevsMain.SavePositionsInt(BVAService.Global.GlobalConsts.PathDataPositions);
        }

        // --- motor / pohyb podlahy ---

        // ovladani motoru
        private void ManualMotorControlOnClick(object sender, RoutedEventArgs e)
        {
            // vyzvednuti typu cinnosti (stisknute tlacitko)
            string actionStr = (string)((Button)sender).Tag;
            // vykonani cinnosti
            DevsCmns.EnFloorAction floorAction = Enum.Parse<DevsCmns.EnFloorAction>(actionStr);
            DevsCmns.FloorMove(floorAction);
        }

        // zmena hodnoty v nejakem 'Device'
        public void OnDeviceChanged(ItmDevice device)
        {
            if (device.Ident == DevsCmns.EnDeviceIdent.Motor)
            {
                bool zero = DataUtils.GetValue<ItmDataBool>(
                    DevsCmns.EnDeviceIdent.Motor, (byte)DevsCmns.EnMotorElement.Zero, 1).Value;
                Int32 rotary = DataUtils.GetValue<ItmDataInt32>(
                    DevsCmns.EnDeviceIdent.Motor, (byte)DevsCmns.EnMotorElement.Rotary, 1).Value;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    manualZeroYes.Fill = (zero == false) ? Brushes.Gray : Brushes.Red;
                    manualRotaryPos.Text = rotary.ToString();
                });
            }
        }
    }
}
