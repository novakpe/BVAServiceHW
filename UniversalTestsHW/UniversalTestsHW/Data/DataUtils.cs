// .NET
using System;
using System.Linq;

namespace UniTestsHW.Data;

public class DataUtils
{
    // 'devIdent' (string) => 'Device' (object)
    // ('Device' se vybira podle 'devIdent', neobsahuje 'Dir')
    public static ItmDevice GetDevice(string devIdent)
    {
        // nalezeni zarizeni do ktereho data patri
        ItmDevice device = (ItmDevice)DataMain.devices.FirstOrDefault(
            item => ((item is ItmDevice) && (((ItmDevice)item).Ident == devIdent)));
        return device;
    }

    // 'devIdent' (string) + 'elmIdent' (string) -> 'Element' (object)
    // ('Element' se vybira podle 'elmIdent', ne podle 'Dir', nemusi jej obsahovat)
    public static ItmElement GetElement(string devIdent, byte elmIdent)
    {
        // nalezeni zarizeni do ktereho data patri
        ItmDevice device = GetDevice(devIdent);
        // pokud nenalezeno, tak navrat
        if (device == null) { return null; }
        // nalezeni 'Element'
        ItmElement elementBase = (ItmElement)device.Elements.FirstOrDefault(
            item => ((item is ItmElement) && (item.Ident == elmIdent)));
        return elementBase;
    }

    // 'devIdent' (string) + 'elmIdent' (string) + 'datIdent' (string) + 'dir' (enum, nepovine) -> 'Data' (object)
    // (data obsahuji vzdy 'Dir')
    public static ItmDataBase GetData(string devIdent, byte elmIdent, byte datIdent = 1, Communs.CmnConsts.EnDir dir = Communs.CmnConsts.EnDir.None)
    {
        ItmElement element = (ItmElement)GetElement(devIdent, elmIdent);
        if (element == null) { return null; }
        ItmDataBase dataBase = (ItmDataBase)element.Items.FirstOrDefault(item => (item.Ident == datIdent));
        return dataBase;
    }

    // vyzvednuti 'Data...' podle 'DevIdent' + 'ElmIdent' + 'Dir' (nepovine)
    public static U GetValue<U>(string devIdent, byte elmIdent, byte datIdent = 1, Communs.CmnConsts.EnDir dir = Communs.CmnConsts.EnDir.None) where U : ItmDataBase
    {
        // nalezeni pozadovane 'Data'
        ItmDataBase dataBase = DataUtils.GetData(devIdent, elmIdent, datIdent, dir);
        // pretypovani a vraceni
        return (U)dataBase;
    }

    // --- ---

    // zda toto 'Device' / 'Element' / 'Data' existuje (zde je pripojeno / nalezeno)
    public static bool ExistDevice(string devIdent, byte elmIdent = 0, byte datIdent = 0, Communs.CmnConsts.EnDir dir = Communs.CmnConsts.EnDir.None)
    {
        // nalezeni pozadovaneho 'Device'
        ItmDevice device = (ItmDevice)DataMain.devices.FirstOrDefault(
            item => ((item is ItmDevice) && (((ItmDevice)item).Ident == devIdent)));
        // pokud neni zadan 'Element', tak vraceni zda nalezeno
        if (elmIdent == 0) { return device != null; }
        // nalezeni pozadovaneho 'Element'
        ItmElement element = (ItmElement)device.Elements.FirstOrDefault(
            item => ((item is ItmElement) && (item.Ident == elmIdent)));
        // pokud neni zadan 'Data', tak vraceni zda nalezeno
        if (datIdent == 0) {return (element != null); }
        // nalezeni pozadovaneho 'Data'
        ItmDataBase data = (ItmDataBase)element.Items.FirstOrDefault(item => (item.Ident == datIdent));
        // pokud neni zadan smer, tak vraceni zda nalezeno
        if (dir == Communs.CmnConsts.EnDir.None) { return (data != null); }
        // vraceni zda data obsahuji pozadovany smer
        return data.Dir.HasFlag(dir);
    }

    // --- ---

    // vytvoreni potrebneho 'ItmData...' podle predanaho 'Config'
    public static ItmDataBase CreateItmDataFromType(DataConsts.EnDataType dataType)
    {
        ItmDataBase itmDataBase = null;
        switch (dataType)
        {
            case DataConsts.EnDataType.Bool: { itmDataBase = new ItmDataBool(); break; }
            case DataConsts.EnDataType.UInt8: { itmDataBase = new ItmDataUInt8(); break; }
            case DataConsts.EnDataType.Int8: { itmDataBase = new ItmDataInt8(); break; }
            case DataConsts.EnDataType.UInt16: { itmDataBase = new ItmDataUInt16(); break; }
            case DataConsts.EnDataType.Int16: { itmDataBase = new ItmDataInt16(); break; }
            case DataConsts.EnDataType.Int32: { itmDataBase = new ItmDataInt32(); break; }
//            case DataConsts.EnDataType.Int64: { itmDataBase = new ItmDataInt64(); break; }
            case DataConsts.EnDataType.FloatBase: { itmDataBase = new ItmDataDouble(); break; }
            //case DataConsts.EnDataType.Point3DDouble: { dataValueBase = new DataValuePoint3DDouble(); break; }
            //case DataConsts.EnDataType.DateTime: { dataValueBase = new DataValueDateTime(); break; }
            //case DataConsts.EnDataType.Time: { dataValueBase = new DataValueTime(); break; }
            //case DataConsts.EnDataType.String: { dataValueBase = new DataValueString(); break; }
            case DataConsts.EnDataType.Bytes: { itmDataBase = new ItmDataBytes(); break; }
        }
        // vraceni vytvorene tridy pro 'Sensor' (nebo 'null')
        return itmDataBase;
    }
}
