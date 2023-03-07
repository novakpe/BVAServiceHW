// .NET
using System;
using System.Windows;
using System.Windows.Controls;
// Project
using UniTestsHW.Communs;
using UniTestsHW.Data;

namespace UniversalTestsHW
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

        // zobarzeni dialogu
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // seznam dostupnyxch zarizeni
            lvDevices.ItemsSource = DataMain.devices;
        }

        private async void ServiceActionOnClick(object sender, RoutedEventArgs e)
        {
            // vyzvednuti typu cinnosti
            string actionStr = (string)((Button)sender).Tag;
            // vykonani podle typu cinnosti
            switch (actionStr)
            {
                // nelezeni vsech dostunych 'Device(s)' / 'Element(s)'
                case "Search":
                    {
                        // zjisteni vsech dostupnych 'Device(s)'
                        CmnMain.FindAllDevice();
                        break;
                    }
                // nacteni informaci / popisu k zarizeni(m)
                //case "LoadDescs": { AppConfig.LoadConfig(); break; }
                // cteni vsech hodnot z 'Device(s)' / 'Element(s)'
                case "Read": { break; }
                // zapis vsech zmen do 'Device(s)' / 'Element(s)'
                case "Write": { await DataMain.DataWrite(); break; }
            }
        }
        private void ListViewAnyOnClick(object sender, RoutedEventArgs e)
        {
            // pokud to neni 'Button', tak navrat
            if (e.OriginalSource is not Button) { return; }

            Button button = (Button)e.OriginalSource;
            // zapis dat do 'Sensor'
            if ((string)button.Content == "W")
                { DataMain.DataWrite(button.Tag); }
            if ((string)button.Content == "R")
                { DataMain.DataRead(button.Tag); }
        }

        // skryti dialogu
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // pokud je tento (hlavni) dialog otevren z jine / externi aplikace
            // dialog bude pouze skryt, ale nezrusi se (muze se pozdeji znovu otevrit)
            // (priste bude pouze zobrazen, nikoli znova vytvoren)
            if (UniTestsHW.App.AppExt.FromExternal == true)
                { Visibility = Visibility.Hidden; e.Cancel = true; }
        }
    }
}
