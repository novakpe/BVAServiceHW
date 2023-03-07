// .NET
using System;
using System.Collections.Generic;

// pouziti
// List<XUtils.USBHID.USBHIDInfo> devices = XUtils.USBHID.USBHIDMain.GetAll();
// XUtils.USBHID.USBHIDDevice device =
//   XUtils.USBHID.USBHIDMain.OpenDevice("Vyrobce / Produkt / SerioveCislo / ReportID");
// if (device != null)
// {
//   device.ReportID = 2;
//   // moznost podedit od 'HIDRepCustBase' a vytvorit vklastni
//   //device.CreateReaportCustomOut<XUtils.USBHID.USBHIDConsts.HIDRepCustOwn>();
//   //device.CreateReaportCustomIn<XUtils.USBHID.USBHIDConsts.HIDRepCustOwn>();
//   // pokud nejsou vytvoreny, vytvori se vychozi z 'HIDRepCustBase'
//   device.ReceiveByThread = true;
//   device.OnReportInCust = OnReportInCust;
//   device.Start();
// }
//
// vyslani reportu (OUT)
// device.ReportOutCust.Clear();
// device.ReportOutCust.Cmd = ...;
// device.ReportOutCust.Status = ...;
// device.ReportOutCust.Data[0] = ...;
// device.ReportOutSend();
//
// prijem / zpracovani prijateho reportu (IN)
// void OnReportInCust(XUtils.USBHID.USBHIDConsts.HIDRepCustBase reportInCust)
// { }

namespace XUtils.USBHID
{
    internal class USBHIDMain
    {
        public static List<USBHIDInfo> DevList = null;

        // vraceni seznamu vsech HID zarizeni
        public static List<USBHIDInfo> GetAll()
        {
            DevList = HIDUSBStatic.GetAll();
            return DevList;
        }

        // otevreni zarizeni podle textoveho nazvu
        public static USBHIDDevice OpenDevice(string devStr)
        {
            USBHIDInfo devInfo = null;
            // pokud je seznam zariznei prazdny, tak se nactou dostupna zarizeni
            if (DevList == null) { GetAll();  }
            // postune projiti vsech dostupnych zarizeni
            foreach (USBHIDInfo devInfoTemp in DevList)
            {   // pokud je nalezen pozadovany textovy popis, tak je to hledane zarizeni
                if (devInfoTemp.ToString() == devStr) { devInfo = devInfoTemp; break; }
            }
            // hledane zarizeni nebylo nalezeno
            if (devInfo == null) { return null; }
            // vraceni nalezeneho zarizeni
            return OpenDevice(devInfo);
        }
        // otevreni zarizeni podle predenaho 'USBHIDInfo'
        public static USBHIDDevice OpenDevice(USBHIDInfo devInfo)
        {
            // otevirane zarizeni
            USBHIDDevice device = new USBHIDDevice();
            // otevre se pozadovane zarizeni
            if (device.Open(devInfo) == false) { return null; }
            // vraceni otevreneho zarizeni
            return device;
        }

        // --- ---

        public static void Error(string text)
        {
            //System.Windows.Forms.MessageBox.Show(text, "!!! Chyba HID zařízení !!!");
        }
    }
}
