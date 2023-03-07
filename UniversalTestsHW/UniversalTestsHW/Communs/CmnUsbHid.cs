// .NET
using System;
using System.Collections.Generic;
using System.Linq;
// Local
using UniTestsHW.Data;

namespace UniTestsHW.Communs;

public class CmnUsbHid
{
    // ziskani seznamu dostupnych zarizeni pro test
    public static void FindAllDevices(byte[] paket, byte start, byte length)
    {
        // zjisteni vsech pripojenych USB-HID zarizeni
        List<XUtils.USBHID.USBHIDInfo> devInfos = XUtils.USBHID.USBHIDMain.GetAll();
        // ponechaji se pouzit pouze ty hledane
        int index = (devInfos.Count == 0) ? 0 : devInfos.Count;
        while (index-- > 0)
        {
            // pokud to neni hledane zarizeni, tak se odstrani ze seznamu
            if ((devInfos[index].Manufacturer != CmnConsts.DeviceManufacturerString)
                && (devInfos[index].Product != CmnConsts.DeviceProductString))
            {
                devInfos.RemoveAt(index); 
            }
        }
        // zpracovani vsech nalezenych zarizeni
        foreach (XUtils.USBHID.USBHIDInfo devInfo in devInfos)
        {
            // otevreni / pripojeni zarizeni
            XUtils.USBHID.USBHIDDevice devHid = XUtils.USBHID.USBHIDMain.OpenDevice(devInfo);
            // kopie dat do 'Report' (zadost o 'Info')
            // v 'ReportOut' na pozici [0] je 'ReportID' ktee se nepouziva
            Array.Copy(paket, start, devHid.ReportOutBytes, 1, length);
            devHid.ReportOutBytes[0] = 0x00;
            // odeslani do zarizeni
            devHid.ReportOutSend();
            System.Threading.Thread.Sleep(100);
            // prijem odpovedi
            devHid.ReportInReceive();
            System.Threading.Thread.Sleep(100);
            // a jeho zpracovani (pokud bude delsi nez jeden 'Report', tak se volani opakuje)
            CmnUtils.CreateDevsAndElmsAndDatas(CmnConsts.EnCmnType.USBHID, devHid.devInfo.SerialNumber, devHid,
                // v 'ReportIn' na pozici [0] je 'ReportID' ktee se nepouziva
                devHid.ReportInBytes, (byte)(1 + CmnConsts.EnPaketPos.Data));

            // nastaveni komunikace radeji az po zjisteni potrebnych informaci
            devHid.ReceiveByThread = true;                 // prijem pomoci vlakna / udalosti
            devHid.OnReportInBytes = OnReportInBytes;      // prijmy od vsech zarizeni do jedne obsluhy
            devHid.IsThisAnswer = IsThisAnswer;            // zpusob kontroly odpovedi pri cekani na odpoved
            devHid.ReceiveStart();                         // spusteni prijmu pomoci vlakna / udalosti
        }
    }

    // uzavreni (otevrene) komunikace
    public static void Close(ItmDevice device)
    {
        // vyzvednuti odkazu na komunikaci
        XUtils.USBHID.USBHIDDevice devCmnHid = (XUtils.USBHID.USBHIDDevice)device.CmnObject;
        // ukonceni komunikace (zastaveni prijimaciho vlakna)
        devCmnHid.ReceiveStop(); devCmnHid.OnReportInBytes = null;
        devCmnHid.Dispose();
        devCmnHid = null;
    }

    // --- ---

    // vzor pro 'OnReportInBytes' (pouziva se)
    // ulozeni prijatych hodnot
    private static void OnReportInBytes(XUtils.USBHID.USBHIDInfo devInf, byte[] reportIn)
    {
        // nalezeni 'Device' ze ktereho je 'Report' (podle 'SerNum')
        ItmDevice device = (ItmDevice)DataMain.devices.FirstOrDefault(
            i => ((i is ItmDevice) && (((ItmDevice)i).Ident == devInf.SerialNumber)));
        if (device == null) { return; }
        // zpracovani vsech 'Element(s)' z prijateho 'Report' (jde o typ 'In')
        // (v 'ReportIn' na pozici [0] je 'ReportID' ktee se nepouziva)
        byte pos = (byte)(1 + CmnConsts.EnPaketPos.Data);
        // vyzvednuti vsech dat z paketu
        CmnUtils.PaketDataRead(reportIn, ref pos, device);
        // signalizace zmeny nejake hodnoty na tomto 'Device'
        if (device.OnDeviceChanged != null) { device.OnDeviceChanged(device); }
    }

    // vzor pro 'IsThisAnswer'
    // pokud souhlasi 'Cmd' a 'CmdSub'
    private static bool IsThisAnswer(byte[] reportOut, byte[] reportIn)
        // pokud je 'Cmd(out) == Cmd(in)|0x80' && 'CmdSub(out) == CmdSub(in)'
        { return (((reportIn[1] & 0x7F) == (reportOut[1] & 0x7F)) && (reportIn[2] == reportOut[2])); }

    // --- ---

    public static bool Send(ItmDevice device, byte[] paket, byte from, byte length, bool waitToAns = false)
    {
        // vyzvednuti odkazu na komunikaci
        XUtils.USBHID.USBHIDDevice devCmnHid = (XUtils.USBHID.USBHIDDevice)device.CmnObject;
        // kopie dat do 'Report'
        // (do 'Report' od pozice [1], na pozici [0] je 'ReportID' ktere se nepouziva)
        Array.Copy(paket, from, devCmnHid.ReportOutBytes, 1, length - from);
        devCmnHid.ReportOutBytes[0] = 0x00;
        // zaslani do externiho zarizeni
        if (waitToAns == false)
        {
            // neceka se na odpoved
            devCmnHid.ReportOutSend();
        }
        else
        {
            // ceka se na odpoved
            if (devCmnHid.ReportSendAndReceive() == XUtils.USBHID.USBHIDConsts.EnCmnStatus.OK)
            {
                OnReportInBytes(devCmnHid.devInfo, devCmnHid.ReportInBytes);
            }
        }
        return true;
    }
}
