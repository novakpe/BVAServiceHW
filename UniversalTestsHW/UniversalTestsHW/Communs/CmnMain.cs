// .NET
using System;
using System.Threading.Tasks;
// local
using UniTestsHW.Data;

namespace UniTestsHW.Communs;

public class CmnMain
{
    // nalzeni vsech dostupnych zarizeni
    public static int FindAllDevice()
    {
        // zapis hlavicky do 'Report'
        byte length = CmnUtils.PaketCreateHeader(paketOut,
            (byte)CmnConsts.EnPaketCmd.Info, (byte)CmnConsts.EnPaketCmdInfoSub.None, (byte)CmnConsts.EnPaketStatus.OK, 0 /* part */);

        // hledani dostupnych zarizeni 'USB-HID'
        CmnUsbHid.FindAllDevices(paketOut, 0, length);
        //// hledani dostupnych zarizeni 'SerialPort' (USB-FTDI)
        //CmnFtdiVCP.FindAllDevices(paketOut, 0, length);

        // vraceni poctu nalezenych zarizeni
        return DataMain.devices.Count;
    }

    // ukonceni komunikace vsech 'Device(s)'
    public static void CloseAllDevice()
    {
        // postupne zavreni a uzavreni komunikace vsech 'Device(s)'
        int pos = DataMain.devices.Count - 1;
        // pokud neni otevreno zadne zarizeni, tak navrat
        if (pos < 0) return;
        do
        {
            // vyzvednuti jednoho 'Device'
            ItmDevice itmDevice = ((ItmDevice)DataMain.devices[pos]) as ItmDevice;
            // pokud neni opravdu 'Device', tak se preskoci
            if (itmDevice == null) { pos--; if (pos > 0) { continue; } else { break; } }
            // volani ukonceni komunikace podle typu uvedeneho v 'Device'
            switch (((ItmDevice)DataMain.devices[pos]).CmnType)
            {
                // USB-HID
                case CmnConsts.EnCmnType.USBHID: { CmnUsbHid.Close(itmDevice); break; }
                // SerialPort
                case CmnConsts.EnCmnType.SerialPort: { CmnFtdiVCP.Close(itmDevice); break; }
            }
            // odebrani tohoto 'Device' ze seznamu
            DataMain.devices.Remove(DataMain.devices[pos]);
            // jedno 'Device' uzavreno
            pos--;
            // musi se uzavrit vsechy 'Device'
        } while (pos > 0);
    }

    // --- udalost o zmene ze zarizeni ---

    public static CmnConsts.DelDeviceChanged OnDeviceChangedUp = null;

    public static void OnDeviceChanged(ItmDevice device)
    {
        if (OnDeviceChangedUp != null)
            { OnDeviceChangedUp(device); }
    }

    // --- odeslani ---

    // vysilany paket
    private static byte[] paketOut = new byte[100];

    // zapis (obecneho) povelu do zarizeni
    // device = null -> zaslani vsem 'Device(s)'
    public static async Task<bool> CmdWrite(ItmDevice device, byte cmd, byte cmdSub, byte[] data = null)
    {
        // vytvoreni zasilaneho paketu
        // zapis hlavicky do 'Paket'
        CmnUtils.PaketCreateHeader(paketOut, cmd, cmdSub, (byte)CmnConsts.EnPaketStatus.OK, 0 /* part */);
        // prepis dat do paketu, pokud jsou
        byte pos = (byte)CmnConsts.EnPaketPos.Data;
        if (data != null)
            { foreach (byte dataItem in data) { paketOut[pos++] = dataItem; } }
        // ukoncovaci znak (i kdyz nejsou data)
        paketOut[pos++] = 0x00;
        // (kazdy 'Device' muze mit jiny typ komunikace)
        // projiti vsech 'Device'
        foreach (ItmDevice dev in DataMain.devices)
        {
            // pokud je zadan 'Device' tak se pouzije pouze ten zadany
            if ((device != null) && (device != dev)) { continue; }

            // volani komunikace podle typu uvedeneho v 'Device'
            switch (dev.CmnType)
            {
                // USB-HID
                case CmnConsts.EnCmnType.USBHID: { CmnUsbHid.Send(dev, paketOut, 0 /* od */, pos /* pocet */); break; }
                // SerialPort
                case CmnConsts.EnCmnType.SerialPort: { CmnFtdiVCP.Send(dev, paketOut, 0 /* od */, pos /* pocet */); break; }
            }
        }
        // neznama komunikace
        return false;
    }

    // odeslani dat na 'HW'
    // 'data' - pouze tyto 'Data'
    // 'element' - pouze tento 'Element' (pouze jeho zmenena 'Data(s)')
    // 'device' - pouze tento 'Device' (pouze jeho zmenene 'Element(s)' a pouze jejich zmenana 'Data(s)')
    // 'null','null','null' -> vsechny 'Device(s)','Element(s)','Data(s)'
    public static async Task<bool> DataWrite(ItmDevice device = null, ItmElement element = null)
    {
        // (kazdy 'Device' muze mit jiny typ komunikace)
        // projiti vsech 'Device'
        foreach (ItmDevice dev in DataMain.devices)
        {
            // pokud je zadan 'Device' tak se pouzije pouze ten zadany
            if ((device != null) && (device != dev)) { continue; }
            // pokud u neho neni zmena 'Out', tak se preskoci
            if (dev.ChangedOut == false) { continue; }

            // zapis hlavicky do 'Report'
            CmnUtils.PaketCreateHeader(paketOut, (byte)CmnConsts.EnPaketCmd.Write, 0 /* CmdSub */, (byte)CmnConsts.EnPaketStatus.OK, 0 /* part */);
            byte pos = (byte)CmnConsts.EnPaketPos.Data;
            // zapis zasilanych / novych hodnot dat (ident + data)
            CmnUtils.PaketDataWrite(paketOut, ref pos, dev, element);

            // volani komunikace podle typu uvedeneho v 'Device'
            switch (dev.CmnType)
            {
                // USB-HID
                case CmnConsts.EnCmnType.USBHID: { return CmnUsbHid.Send(dev, paketOut, 0 /* od */, pos /* pocet */); }
                // SerialPort
                case CmnConsts.EnCmnType.SerialPort: { return CmnFtdiVCP.Send(dev, paketOut, 0 /* od */, pos /* pocet */); }
            }
        }
        // neznama komunikace
        return false;
    }

    // cteni dat z HW
    public static async Task<bool> DataRead(ItmDevice device = null, ItmElement element = null)
    {
        // (kazdy 'Device' muze mit jiny typ komunikace)
        // projiti vsech 'Device'
        foreach (ItmDevice dev in DataMain.devices)
        {
            // pokud je zadan 'Device' tak se pouzije pouze ten zadany
            if ((device != null) && (device != dev)) { continue; }

            // zapis hlavicky do 'Report'
            CmnUtils.PaketCreateHeader(paketOut, (byte)CmnConsts.EnPaketCmd.Read, 0  /* CmdSub */, (byte)CmnConsts.EnPaketStatus.OK, 0 /* part */);
            byte pos = (byte)CmnConsts.EnPaketPos.Data;

            // zapis novych hodnot dat (ident + data)
            foreach (ItmElement elm in device.Elements)
            {
                // pokud je zadan 'Element' tak se pouzije pouze ten zadany
                if ((element != null) && (element != elm)) { continue; }
                // pokud to neni vystup, tak preskocit
                if (elm.IsIn == false) { continue; }
                // 'Ident' pro cteny 'Element'
                paketOut[pos++] = elm.Ident;
            }

            // volani komunikace podle typu uvedeneho v 'Device'
            switch (dev.CmnType)
            {
                // USB-HID
                case CmnConsts.EnCmnType.USBHID: { CmnUsbHid.Send(dev, paketOut, 0 /* od */, pos /* pocet */, true /* cekat */); break; }
                // SerialPort
                case CmnConsts.EnCmnType.SerialPort: { CmnFtdiVCP.Send(dev, paketOut, 0 /* od */, pos /* pocet */, true /* cekat */); break; }
            }
        }
        // neznama komunikace
        return false;
    }
}
