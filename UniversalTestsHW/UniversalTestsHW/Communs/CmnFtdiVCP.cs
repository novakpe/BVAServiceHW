// .NET
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
// Others
using FTD2XX_NET;
// local
using UniTestsHW.Data;

namespace UniTestsHW.Communs;

public class CmnFtdiVCP
{
    public const int ConstBaudRate = 38400;
    public const int ConstReadTimeout = 100;
    public const int ConstSendBuffer = 100;
    public const int ConstRecvBuffer = 100;

    public static void FindAllDevices(byte[] paket, byte start, byte length)
    {
        FTDI ftdiDevice = new FTDI();
        // zjisteni poctu pripojenych FTDI zarizeni
        uint deviceNum = 0;
        ftdiDevice.GetNumberOfDevices(ref deviceNum);
        // pokud neni zadne pripojeno, tak navrat
        if (deviceNum == 0) { return; }

        FTDI.FT_STATUS status = FTDI.FT_STATUS.FT_OK;
        // zaznam vsech nalezenych / pripojenych FTDI zarizeni
        List<DevCmnVcp> devCmnVcps = new List<DevCmnVcp>();
        // pro vyzvednute hodnoty
        string desc = String.Empty; string serNum = String.Empty; string portName = String.Empty;
        // postupne projiti vsech nalezenych zarizeni
        for (uint i = 0; i < deviceNum; i++)
        {
            // otevreni zarizeni (zde pouze pro zjisteni informaci, nikoli pro komunikaci)
            status = ftdiDevice.OpenByIndex(i);
            // pokud se povedelo uspesne otevrit
            if (status == FTDI.FT_STATUS.FT_OK)
            {
                // vyzvenuti popisu zarizeni
                ftdiDevice.GetDescription(out desc);
                // pokud popis nesouhlasi, tak to neni nase zarizeni
                //if (desc != "xxx") { continue; }
                // vyzvednuti 'SerialNumber'
                ftdiDevice.GetSerialNumber(out serNum);
                // zjisteni COM portu od tohoto zarizeni
                status = ftdiDevice.GetCOMPort(out portName);
                // pokud byl COM port uspesne zjisten, tak zaznamenani tohoto 'device'
                if (status == FTDI.FT_STATUS.FT_OK) { devCmnVcps.Add(new DevCmnVcp() { SerNum = serNum, ComPort = portName }); }
                // uzavreni tohoto zarizeni
                ftdiDevice.Close();
            }
        }
        // pokud nebyl zjisten zadny (platny) COM port, tak konec
        if (devCmnVcps.Count == 0) { return; }

        //devices[0].SerNum = "BVA-TEST 0100";
        devCmnVcps[0].SerNum = "BVA-TEST 0100|8";

        // otevreni vsech nalezenych zarizeni
        foreach (DevCmnVcp devCmnVcp in devCmnVcps)
        {
            // otevreni COM portu (zatim pouze toho prvniho)
            devCmnVcp.serialPort.PortName = devCmnVcp.ComPort;
            devCmnVcp.serialPort.Open();
            // pokud se nepovedelo otevrit, tak konec
            if (devCmnVcp.serialPort.IsOpen == false) { continue; }

            // test zda uveden rozsah adres
            int pos = devCmnVcp.SerNum.IndexOf('|');
            int adrReange = 0;
            // pokud je '|' -> je rozsah adres zarizenich na externi sbernici
            if (pos != -1)
            {
                // pokud je rozsah adres na externi sbernici, tak se vyzvedne
                adrReange = Byte.Parse(devCmnVcp.SerNum.Substring(pos + 1));
                // // odtrzeni rozsah adres od zakladu 'SerialNumber'
                devCmnVcp.SerNum = devCmnVcp.SerNum.Substring(0, pos);
            }
            // vyzvednuti ciselen adresy z konce 'SerNum'
            (string identBaseStr, string identNumStr) = GetIdentBaseAndNum(devCmnVcp.SerNum);

            // adresa zarizeni na externi sbernici je vzdy od '1' (nikoli jako celkova adresa)
            // (nikdy neni '0', to je pro 'Broadcast', tedy je nutno '+1')
            // (bazova adresa tedy neni nikdy pouzita, muze byt pouzita pro 'Broadcast''?)
            for (byte adrExtDev = 1; adrExtDev < ((adrReange == 0) ? 2 : (adrReange + 1)); adrExtDev++)
            {
                // vymazani predchoziho obsahu
                Array.Clear(devCmnVcp.paketOutBin);
                // vlozeni vytvoreneho 'GetInfo' paketu do vysilaneho paketu
                Array.Copy(paket, start, devCmnVcp.paketOutBin, 1, length);
                // na pozici [0] je adresa
                devCmnVcp.paketOutBin[0] = adrExtDev;
                // previod paketu z 'byte[]' na 'String' ('[...]')
                string paketOutStr = BytesToStringPaket(devCmnVcp.paketOutBin, 0, (byte)(1 /* adresa na pozici [0] */ + length));
                // vyslani na 'SerialPort'
                devCmnVcp.serialPort.WriteLine(paketOutStr); //serialPort.Flush();
                // precteni odpovedi ze 'SerialPort'
                string paketInStr = null;
                try { paketInStr = devCmnVcp.serialPort.ReadTo("]"); }
                catch { paketInStr = null; }
                // pokud je odpoved platna (zatim pouze takto)
                if (String.IsNullOrWhiteSpace(paketInStr) == false)
                {
                    // vytvoreni 'Ident' pro toto nalezene zarizeni na externi sbernici
                    string identExtDev = String.Empty;
                    if (adrReange == 0)
                    {
                        // pokud neni rozsah adres, tak zarizeni na externi sbernici ma vzdy adresu '0x01'
                        // ta je nastavena prvnim pruchodem v 'adrExtDev', dalsi pruchd jiz neni
                        // a 'Ident' externiho zarizeni se rovna 'Identu' v FTDI
                        identExtDev = identBaseStr + identNumStr;
                    }
                    else
                    {
                        // pokud je zadan rozsah adres, tak se adresy cisluji podle vzrustajiciho 'adrExtDev'
                        // a musi se vytvorit 'SerialNumber' pro toto externi zarizeni
                        // (prevod ciselene casti na cislo - pricteni poradoveho cisla na sbernici - prevod opet na stejne dlouhy retezec)
                        int cisloMist = identNumStr.Length; UInt32 cislo = UInt32.Parse(identNumStr);
                        identExtDev = identBaseStr + (cislo + adrExtDev).ToString("D" + cisloMist);
                    }

                    // ulozeni prevodu globalniho 'Ident' na 'ArdExt' sbernice
                    devCmnVcp.identToAdrExt[identExtDev] = adrExtDev;

                    // prevod paketu ze 'String' na 'Byte[]'
                    byte pocet = StringPaketToBytes(devCmnVcp.paketInBin, paketInStr);
                    // a jeho zpracovani (pokud bude delsi nez jeden 'Report', tak se volani opakuje)
                    CmnUtils.CreateDevsAndElmsAndDatas(CmnConsts.EnCmnType.SerialPort, identExtDev, devCmnVcp,
                        devCmnVcp.paketInBin, (byte)(1 /* adresa na pozici [0] */ + CmnConsts.EnPaketPos.Data));
                }
            }
        }
    }

    // rozdeleni 'SerialNumber' na pripadnou textovou cast (pokud existuje) a koncovou ciselnou cast (musi existovat)
    public static (string identStr, string identNum) GetIdentBaseAndNum(string ident)
    {
        string identStr = null; string identNum = null;
        // pujde se od konce 'Ident' (od posledniho znaku)
        int pos = ident.Length - 1;
        // pokud je tento znak 'cislice'
        while((pos >= 0) && (Char.IsDigit(ident[pos]) == true))
            // tak se povazuje za ciselny konec (cislice na konci 'Ident', cislice muze byt i cely ident)
            { pos--; }
        // pokud neni poizce uplne na prvnim znaku -> 'Ident' obsahuje textovou uvodni cast
        if (pos > 0) { identStr = ident.Substring(0, pos + 1); }
        // od pozice do konce je to ciselna cast
        identNum = ident.Substring(pos + 1);
        // vraceni nalezene textove a ciselne casti z 'Ident'
        return (identStr, identNum);
    }

    // uzavreni (otevrene) komunikace
    public static void Close(ItmDevice device)
    {
        // vyzvednuti odkazu na komunikaci
        DevCmnVcp devCmnVcp = (DevCmnVcp)device.CmnObject;
        // pokud je jiz komunikace zavrena, tak navrat
        // (nekolik 'Device(s)' sdili stejny 'Vcp')
        if ((devCmnVcp.serialPort == null) || (devCmnVcp.serialPort.IsOpen == false)) { return; }
        // ozavreni tohoto 'Vcp'
        devCmnVcp.serialPort.Close(); devCmnVcp.serialPort.Dispose(); devCmnVcp.serialPort = null;
    }

    // --- ---

    // vyslani paketu 'String' na 'SerialPort'
    public static bool Send(ItmDevice device, byte[] paket, byte start, byte length, bool waitToAns = false)
    {
        // vyzvednuti odkazu na komunikaci
        DevCmnVcp devCmnVcp = (DevCmnVcp)device.CmnObject;
        // vymazani predchoziho obsahu
        Array.Clear(devCmnVcp.paketOutBin);
        // vlozeni vytvoreneho 'Paketu' paketu do vysilaneho paketu
        Array.Copy(paket, start, devCmnVcp.paketOutBin, 1, length);
        // zjisteni 'AdrExt' podle globalniho 'Ident'
        devCmnVcp.paketOutBin[0] = devCmnVcp.identToAdrExt[device.Ident];
        string paketOutStr = BytesToStringPaket(devCmnVcp.paketOutBin, 0, (byte)(1 /* adresa na pozici [0] */ + length));
        devCmnVcp.serialPort.WriteLine(paketOutStr); //serialPort.Flush();
        return true;
    }

    // --- ---

    // prevod paketu z formatu 'byte[]' na format 'String'
    private static StringBuilder sb = new StringBuilder();
    public static string BytesToStringPaket(byte[] data, byte start, byte length)
    {
        sb.Clear();             // vymazani predchozi obsahu
        sb.Append("[");         // zacatek paketu
        // vlozeni vsech 'byte' jako dva 'AsciiHex' znaky
        for (byte i = start; i < (start + length); i++) { sb.Append(data[i].ToString("X2")); }
        sb.Append("]");         // konec paketu
        return sb.ToString();   // vraceni textoveho paektu
    }

    // prevod paketu z formatu 'String' na format 'byte[]'
    // (vystupni 'bin' paket je v 'paketInBin')
    public static byte StringPaketToBytes(byte[] paketBin, string paketStr)
    {
        // vymazani puvodniho obsahu
        Array.Clear(paketBin);
        // pozice zapisu do 'Bin' paketu, pozice cteni hodnot ze 'String' paketu
        byte posBin = 0; byte posStr = 1;
        // postupne projiti vsech dvojic 'AsciiHex' znaku a jejich prevod na 'byte'
        while (posStr < paketStr.Length - 1)
        {
            paketBin[posBin++] = Byte.Parse(paketStr.Substring(posStr, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
            posStr += 2;
        }
        // vraceni poctu dat v 'byte[]' paketu
        return posBin;
    }
}

// jedno nalezene zarizeni na 'SerialPort'
public class DevCmnVcp
{
    // puvondi 'SerialNumber' z FTDI
    public string SerNum = String.Empty;
    // otevreni 'VCP'
    public string ComPort = String.Empty;

    // zakladni parametry pro 'SerialPort'
    public System.IO.Ports.SerialPort serialPort = new System.IO.Ports.SerialPort()
        { BaudRate = CmnFtdiVCP.ConstBaudRate, DataBits = 8, StopBits = System.IO.Ports.StopBits.Two, ReadTimeout = CmnFtdiVCP.ConstReadTimeout };

    public byte[] paketOutBin = new byte[CmnFtdiVCP.ConstSendBuffer];     // vyslany paket na 'SerialPort' (v 'Bin' formatu)
    public byte[] paketInBin = new byte[CmnFtdiVCP.ConstRecvBuffer];      // prijimany paket ze 'SerialPort' (v 'Bin' formatu)

    // prevody globalnich 'Ident' na adrExt pro externi sbernici
    public Dictionary<string, byte> identToAdrExt = new Dictionary<string, byte>();
}
