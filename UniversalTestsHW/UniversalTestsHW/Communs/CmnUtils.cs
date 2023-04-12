// .NET
using System;
using System.Linq;
// Local
using UniTestsHW.Data;

namespace UniTestsHW.Communs;

public class CmnUtils
{
    // vlozeni informaci o In/Out do 'Device'
    public static void CreateDevsAndElmsAndDatas(CmnConsts.EnCmnType cmnType, string serialNumber,
        object cmnObj, byte[] report, byte pos)
    {
        // vyzvednuti 'Device' podle 'SerNum' (pokud jiz existuje)
        // (muze se jednat do dalsi cast velkeho 'Info' ze zarizeni)
        // (je jedno zda se casti 'Info' slozi a pak zpracuji, nebo se zpracuji samostatne)
        // (pokud se zpracuji samostatne, tak musi koncit zpravne rozdeleny, tedy koncem lementu)
        ItmDevice device = (ItmDevice)DataMain.devices.FirstOrDefault(
            i => ((i is ItmDevice) && (((ItmDevice)i).Ident == serialNumber)));
        // pokud takoveto jeste neni, tak se vytvori a vlozi do seznamu
        if (device == null)
        {
            // vytvoreni tridy 'ItmDevice' a ulozeni zakladnich udaju (odkaz na otevreny USB-HID)
            device = new ItmDevice() { Ident = serialNumber, CmnType = cmnType, CmnObject = cmnObj };
            device.OnDeviceChanged = CmnMain.OnDeviceChanged;
            DataMain.devices.Add(device);
        }
        // vycteni vsech informaci o In/Out 'Element(s)'
        // postupne projiti vsech dat v 'Info'
        // V1(InfoVer=1), ElmIdent(bit7 => 0 In / 1 Out), ElmStatus, ElmType, DataType
        // V2(InfoVer=2), ElmIdent, ElmStatus, ElmType, DataCount, DataValue, DataType, DataValue, DataType, ...
        while (report[pos] != 0)
        {
            // vyzvednuti 'Ident' a 'Dir' (pro V1)
            byte elmIdent = (byte)(report[pos] & 0x7F);
            CmnConsts.EnDir elmDirV1 = ((report[pos] & 0x80) == 0x80) ? CmnConsts.EnDir.Out : CmnConsts.EnDir.In;
            pos++;
            // nalezeni zda tento 'Element' jiz neni vytvoren
            // (muze se jednat do dalsi cast velkeho 'Info' ze zarizeni)
            // (je jedno zda se casti 'Info' slozi a pak zpracuji, nebo se zpracuji samostatne)
            // (pokud se zpracuji samostatne, tak musi koncit zpravne rozdeleny, tedy koncem lementu)
            ItmElement element = (ItmElement)device.Elements.FirstOrDefault(item => (item.Ident == elmIdent));
            // pokud neni, tak se musi vytvorit
            if (element == null)
            {
                // vytvoreni noveho 'Element'
                element = new ItmElement() { Ident = elmIdent, RefDevice = device };
                element.ElmStatus = (DataConsts.EnElmStatus)report[pos++];
                element.ElmType = (DataConsts.EnElmType)report[pos++];
                device.Elements.Add(element);
                // pro V1 obsahuje 'ItmElement' pouze jedny 'ItmData'
                //if (infoVer == 1)
                //{
                //    DataConsts.EnDataType dataType = (DataConsts.EnDataType)report[pos++];
                //    // vytvoreni datove tridy
                //    ItmDataBase dataBase = DataUtils.CreateItmDataFromType(dataType);
                //    dataBase.Ident = "1"; // (zatim vzdy '1')
                //    dataBase.DataType = dataType;
                //    dataBase.Dir = elmDirV1;
                //    dataBase.RefElement = element;
                //    // a jeji ulozeni k 'Element'
                //    element.Items.Add(dataBase);
                //}
                //// pro V1 obsahuje 'ItmElement' nekolik 'ItmData'
                //if (infoVer == 2)
                //{
                // vyzvednuti poctu datovy polozek
                byte count = report[pos++];
                // 'Ident' se k datum prirazuje automaticky inkrementalne, podle jejich poradi
                byte dataIdent = 1;
                // zpracovani vsech datovych polozek
                // (vzdy dvojice: 'DataValue', 'DataType')
                while (count-- > 0)
                {
                    // vyzvednuti 'Dir', 'DataValue' a 'DataType'
                    CmnConsts.EnDir dataDirV2 = ((report[pos] & 0x80) == 0x80) ? CmnConsts.EnDir.Out : CmnConsts.EnDir.In;
                    DataConsts.EnElmType dataValue = (DataConsts.EnElmType)report[pos++];
                    DataConsts.EnDataType dataType = (DataConsts.EnDataType)report[pos++];
                    // vytvoreni datove tridy
                    ItmDataBase dataBase = DataUtils.CreateItmDataFromType(dataType);
                    dataBase.Ident = dataIdent; dataIdent++;
                    //dataBase.DataValue = dataValue;
                    dataBase.DataType = dataType;
                    dataBase.Dir = dataDirV2;
                    dataBase.RefElement = element;
                    // a jeji ulozeni k 'Element'
                    element.Items.Add(dataBase);
                }
                //}
            }
        }
        // rovnez je potreba se podivat, zda pro toto 'Device' neni popis
        Configs.ConfigMain.LoadConfig(device);
    }

    // --- ---

    public static byte PaketCreateHeader(byte[] paket, byte cmd, byte cmdSub, byte status, byte part = 0)
    {
        // vymazani vysilaneho reportu (pred vyslanim noveho)
        Array.Clear(paket);
        // zapis hlavicky do 'Report'
        paket[(byte)CmnConsts.EnPaketPos.Cmd] = cmd;
        paket[(byte)CmnConsts.EnPaketPos.CmdSub] = cmdSub;
        paket[(byte)CmnConsts.EnPaketPos.Status] = status;
        paket[(byte)CmnConsts.EnPaketPos.Part] = part;
        // preventivne se zapize ze nejsou data (mohou se vsak doplnit)
        paket[(byte)CmnConsts.EnPaketPos.Data] = 0x00;

        return 5;
    }


    // vlozeni dat do paketu
    public static void PaketDataWrite(byte[] paket, ref byte pos, ItmDevice device = null, ItmElement element = null)
    {
        // zapis novych hodnot dat (ident + data)
        foreach (ItmElement elm in device.Elements)
        {
            // pokud je zadan 'Element' tak se pouzije pouze ten zadany
            if ((element != null) && (element != elm)) { continue; }

            // pokud to neni vystup, tak preskocit
            if (elm.IsOut == false) { continue; }
            // pokud neni zmena, tak se preskoci
            if (elm.ChangedOut == false) { continue; }
            // 'Ident' pro zapisovany 'Element'
            paket[pos++] = elm.Ident;

            // projiti vsech 'ItmData(s)' v tomto 'Element'
            foreach (ItmDataBase datBase in elm.Items)
            {
                if (datBase.Dir.HasFlag(CmnConsts.EnDir.Out) == false) { continue; }

                // (pod 'Element' se zapisuji vzdy vsechny 'Data(s)')
                // data podle jejich typu
                switch (datBase.DataType)
                {
                    case DataConsts.EnDataType.Bool: { aketWriteBool(paket, ref pos, ((ItmDataBool)datBase).Value); break; }
                    case DataConsts.EnDataType.UInt8: { PaketWriteUInt8(paket, ref pos, ((ItmDataUInt8)datBase).Value); break; }
                    case DataConsts.EnDataType.Int8: { PaketWriteInt8(paket, ref pos, ((ItmDataInt8)datBase).Value); break; }
                    case DataConsts.EnDataType.UInt16: { PaketWriteUInt16(paket, ref pos, ((ItmDataUInt16)datBase).Value); break; }
                    case DataConsts.EnDataType.Int16: { TaketWriteInt16(paket, ref pos, ((ItmDataInt16)datBase).Value); break; }
                    case DataConsts.EnDataType.UInt32: { PaketWriteUInt32(paket, ref pos, ((ItmDataUInt32)datBase).Value); break; }
                    case DataConsts.EnDataType.Int32: { PaketWriteInt32(paket, ref pos, ((ItmDataInt32)datBase).Value); break; }
                    case DataConsts.EnDataType.Bytes: { PaketWriteBytes(paket, ref pos, ((ItmDataBytes)datBase).Value, ((ItmDataBytes)datBase).Length); break; }

                }
                // zmena zpracovana
                datBase.Changed = false;
            }
        }
        // konec obsahu reportu
        paket[pos++] = 0;
    }

    // vyzvednuti dat z paketu
    public static void PaketDataRead(byte[] paket, ref byte pos, ItmDevice device)
    {
        while (paket[pos] != 0)
        {
            byte posTemp = pos; // 'ref/out' promenna nelze pouzit uvnitr 'lambda' vyrazu
            // nalezeni 'Element' podle 'ElmIdent' (hleda se pouze v 'In')
            ItmElement element = (ItmElement)device.Elements.FirstOrDefault(i => i.Ident == paket[posTemp]);
            pos++; // posun za 'ElmIdent'
            // pokud nenalezen, tak se tento 'Element' preskoci
            if (element == null) { continue; }
            // pokud nalezen, tak se ulozi prijata data (podle jejich typu)
            switch (element.Items[0].DataType)
            {
                case DataConsts.EnDataType.Bool: { ((ItmDataBool)element.Items[0]).Value = PaketReadBool(paket, ref pos); break; }
                case DataConsts.EnDataType.UInt8: { ((ItmDataUInt8)element.Items[0]).Value = PaketReadUInt8(paket, ref pos); break; }
                case DataConsts.EnDataType.Int8: { ((ItmDataInt8)element.Items[0]).Value = PaketReadInt8(paket, ref pos); break; }
                case DataConsts.EnDataType.UInt16: { ((ItmDataUInt16)element.Items[0]).Value = PaketReadUInt16(paket, ref pos); break; }
                case DataConsts.EnDataType.Int16: { ((ItmDataInt16)element.Items[0]).Value = PaketReadInt16(paket, ref pos); break; }
                case DataConsts.EnDataType.UInt32: { ((ItmDataUInt32)element.Items[0]).Value = PaketReadUInt32(paket, ref pos); break; }
                case DataConsts.EnDataType.Int32: { ((ItmDataInt32)element.Items[0]).Value = PaketReadInt32(paket, ref pos); break; }
                case DataConsts.EnDataType.Bytes: {
                        // vyzvednuti 'Bytes' z prijateho paketu
                        PaketReadBytes(paket, ref pos, element.Items[0]);
                        // signalizace zmeny (meni se pouze obsah / Bites nikoli cele 'Value')
                        element.Items[0].OnPropertyChanged("Value"); element.Items[0].OnPropertyChanged("ValueStr");
                        break; }
            }
            // signalizace zmeny
            element.ChangedIn = true;
            // pokud je pripojena udalost o zmene, tak se vyvola
            if (element.OnElementChanged != null) { element.OnElementChanged(element.RefDevice, element); }
        }
    }

    // --- zapis vystupnich hodnot do 'Paket' ---

    // zapis 'Bool' jako data do 'Paket'
    // (1B => 0/1)
    public static void aketWriteBool(byte[] buffer, ref byte pos, bool value)
    {
        buffer[pos++] = (value == false) ? (byte)0 : (byte)1;
    }
    // zapis 'UInt8' jako data do 'Paket'
    // (1B)
    public static void PaketWriteUInt8(byte[] buffer, ref byte pos, byte value)
    {
        buffer[pos++] |= value;
    }
    // zapis 'Int8' jako data do 'Paket'
    // (1B => High/Low, bit7 - znamenko)
    public static void PaketWriteInt8(byte[] buffer, ref byte pos, sbyte value)
    {
        if (value < 0) { value *= -1; buffer[pos] = 0x80; }
        buffer[pos++] |= (byte)(value & 0x7F);
    }
    // zapis 'UInt16' jako data do 'Paket'
    // (2B => High/Low)
    public static void PaketWriteUInt16(byte[] buffer, ref byte pos, UInt16 value)
    {
        buffer[pos++] |= (byte)((value >> 8) & 0xFF);
        buffer[pos++] = (byte)(value & 0xFF);
    }
    // zapis 'Int16' jako data do 'Paket'
    // (2B => High/Low, High-bit7 - znamenko)
    public static void TaketWriteInt16(byte[] buffer, ref byte pos, Int16 value)
    {
        if (value < 0) { value *= -1; buffer[pos] = 0x80; }
        buffer[pos++] |= (byte)((value >> 8) & 0x7F);
        buffer[pos++] = (byte)(value & 0xFF);
    }
    // zapis 'UInt32' jako data do 'Paket'
    // (2B => High/Low)
    public static void PaketWriteUInt32(byte[] buffer, ref byte pos, UInt32 value)
    {
        buffer[pos++] |= (byte)((value >> 24) & 0xFF); buffer[pos++] |= (byte)((value >> 16) & 0xFF);
        buffer[pos++] |= (byte)((value >> 8) & 0xFF); buffer[pos++] = (byte)(value & 0xFF);
    }
    // zapis 'Int32' jako data do 'Paket'
    // (2B => High/Low, High-bit7 - znamenko)
    public static void PaketWriteInt32(byte[] buffer, ref byte pos, Int32 value)
    {
        if (value < 0) { value *= -1; buffer[pos] = 0x80; }
        buffer[pos++] |= (byte)((value >> 24) & 0x7F);
        buffer[pos++] = (byte)((value >> 16) & 0xFF);
        buffer[pos++] = (byte)((value >> 8) & 0xFF);
        buffer[pos++] = (byte)(value & 0xFF);
    }
    // zapis 'Bytes' jako data do 'Paket'
    public static void PaketWriteBytes(byte[] buffer, ref byte pos, byte[] value, int length)
    {
        // prvni byte je delka dat (max 255)
        buffer[pos++] = (byte)(length & 0xFF);
        for (byte i = 0; i < length; i++) { buffer[pos++] = value[i]; }
    }

    // --- vyzvednuti vstupnich dat z 'Paket' ---

    // cteni 'Bool' jako data z 'Paket'
    // (1B => 0/1)
    public static bool PaketReadBool(byte[] buffer, ref byte pos)
    {
        return (buffer[pos++] == 0) ? false : true;
    }
    // cteni 'UInt8' jako data z 'Paket'
    // (1B)
    public static byte PaketReadUInt8(byte[] buffer, ref byte pos)
    {
        return buffer[pos]++;
    }
    // cteni 'Int8' jako data z 'Paket'
    // (1B => bit7 - znamenko)
    public static sbyte PaketReadInt8(byte[] buffer, ref byte pos)
    {
        sbyte value = (sbyte)(buffer[pos] & 0x7F);
        if ((buffer[pos] & 0x80) != 0x00) { value *= -1; }
        pos += 1;
        return value;
    }
    // cteni 'UInt16' jako data z 'Paket'
    // (2B => High/Low)
    public static UInt16 PaketReadUInt16(byte[] buffer, ref byte pos)
    {
        return (UInt16)((buffer[pos++] << 8) | buffer[pos++]);
    }
    // cteni 'Int16' jako data z 'Paket'
    // (2B => High/Low, High-bit7 - znamenko)
    public static Int16 PaketReadInt16(byte[] buffer, ref byte pos)
    {
        Int16 value = (Int16)(((buffer[pos] & 0x7F) << 8) | buffer[pos + 1]);
        if ((buffer[pos] & 0x80) == 0x80) { value *= -1; }
        pos += 2;
        return value;
    }
    // cteni 'UInt32' jako data z 'Paket'
    // (2B => High/Low)
    public static UInt32 PaketReadUInt32(byte[] buffer, ref byte pos)
    {
        return (UInt32)((buffer[pos++] << 24) | (buffer[pos++] << 16) | (buffer[pos++] << 8) | buffer[pos++]);
    }
    // cteni 'Int32' jako data z 'Paket'
    // (2B => High/Low, High-bit7 - znamenko)
    public static Int32 PaketReadInt32(byte[] buffer, ref byte pos)
    {
        Int32 value = (Int32)(((buffer[pos] & 0x7F) << 24) | (buffer[pos + 1]<< 16) |
            (buffer[pos + 2] << 8) | buffer[pos + 3]);
        if ((buffer[pos] & 0x80) == 0x80) { value *= -1; }
        pos += 4;
        return value;
    }
    // cteni 'Bytes' jako binarniho bloku data z 'Paket'
    // (prvni 'Byte' je pocet nasledujicich dat / 'Bytes')
    public static void PaketReadBytes(byte[] buffer, ref byte pos, ItmDataBase ttmDataBase)
    {
        // prvni hodnota udava pocet 'Bytes' / dat
        byte count = buffer[pos++];
        // zpristupneni datove polozky podle aktualniho typu
        ItmDataBytes itmDataBytes = (ItmDataBytes)ttmDataBase;
        // pokud neni misto pro data, tak se vytvori
        if ((itmDataBytes.Value == null) || (itmDataBytes.Value.Length < count)) { itmDataBytes.Value = new byte[count]; }
        // vyzvednuti daneho poctu hodnot
        for (byte i = 0; i < count; i++) { itmDataBytes.Value[i] = buffer[pos++]; }
    }
}
