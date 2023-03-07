using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace BVATests
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // --- ---

        // vytvoreni aplikace, pred zobrazeim dialogu
        private void WindowOnLoaded(object sender, RoutedEventArgs e)
        {
            // adresa kde jsou testovaci / servisni data
            Project.Global.GlobalConsts.PathDataAppBase = "HWTestData";
            
            // A) neni HW
            // pristup k HW je blokovan
            //BVAService.Devices.DevsCmns.EnableHW = false;

            // B) je HW
            // pristup k HW je povolen
            BVAService.Devices.DevsCmns.EnableHW = true;
            // nalezeni vsech dostupnych externich zarizeni
            string[] devssStr = BVAService.Devices.DevsCmns.FindAllDevice();
            // zobrazeni seznamu nalezeni / nenalezenych zarizeni
            BVAService.Devices.DevsMain.ShowDevicesStatus();

        }

        // --- ---

        // odkaz na testovaci dialog
        private BVAService.MainWindow bvaServiceWindow = null;

        // stisk tlacitka 'Test'
        private void TestOnClick(object sender, RoutedEventArgs e)
        {
            // pokud dialog 'BVATest' neexistuje, tak se vytvori (bude se pouze skryvat a ne zavirat, je volano z externi aplikace)
            if (bvaServiceWindow == null) { bvaServiceWindow = new BVAService.MainWindow() { NoCloseOnlyHide = true, FromExternalApp = true }; }
            // (znova) zobrazeni dialogu
            bvaServiceWindow.Show();
        }

        // --- ---

        // ukoncovani aplikace, jeste neni zcela ukoncena
        private void WindowOnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // pri uzavreni 'BVA-App' se rovnez uzavre i 'BVA-Test'
            bvaServiceWindow.NoCloseOnlyHide = false; if (bvaServiceWindow != null) { bvaServiceWindow.Close(); }
        }
    }
}
