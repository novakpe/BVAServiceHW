// .NET
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace UniTestsHW.Data;

// --- tridy pro 'Device(s)' ---

public class ItmMainBase : INotifyPropertyChanged
{
    // odkaz na konfiguraci pro toto 'Device'
    public Configs.DeviceConfigBase DeviceConfig { get; set; } = null;

    // --- ---

    // pro vynuceni aktualizace zobrazeni v GUI
    // (volanim teto funkce s nazvem vlastnosti se automaticky aktualizje GUI)
    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
}

public class ItmSeparator : ItmMainBase
{
    public string Name { get; set; } = String.Empty;
}
public class ItmDevice : ItmMainBase
{
    // 'Name' / 'Decs' -> jsou v 'DeviceConfig'

    // ('Ident' a 'CmnType' nemuze byt v 'DeviceConfig' protoze se muze cist ze zarizeni a 'DeviceConfig' byt vubec nemusi)
    // identifikace pro 'Element'
    public string Ident { get; set; } = DataConsts.IdentDef;
    // typ komunikace
    public Communs.CmnConsts.EnCmnType CmnType { get; set; } = Communs.CmnConsts.EnCmnType.None;

    private bool changedIn = false;
    public bool ChangedIn { get { return changedIn; } set { changedIn = value; OnPropertyChanged("ChangedIn"); OnPropertyChanged("ChangedStr"); } }
    
    private bool changedOut = false;
    public bool ChangedOut { get { return changedOut; } set { changedOut = value; OnPropertyChanged("ChangedOut"); OnPropertyChanged("ChangedStr"); } }
    public String ChangedStr { get { return ((changedIn == false) ? "-" : "In") + "/" + ((changedOut == false) ? "-" : "Out"); } }

    // pocet prijatych hodnot
    private UInt16 countIn = 0;
    public UInt16 CountIn { get { return countIn; } set { countIn = value; OnPropertyChanged("CountIn"); /* pri zapisu se aktualizuje GUI */ } }
    // pocet prijatych hodnot
    private UInt16 countOut = 0;
    public UInt16 CountOut { get { return countOut; } set { countOut = value; OnPropertyChanged("CountOut"); /* pri zapisu se aktualizuje GUI */ } }

    // --- ---

    // odkaz na (vytvorenou) komunikace podle typu (pokud je potreba)
    public object CmnObject { get; set; } = null;

    // seznam obsazenych 'ItmElement(s)'
    public List<ItmElementBase> Elements { get; set; } = new List<ItmElementBase>();

    public Communs.CmnConsts.DelDeviceChanged OnDeviceChanged = null;

    // odkaz na vlastni objekt
    public object Tag { get; set; } = null;
}

// --- tridy pro 'Sensors(s)' ---

public class ItmElementBase : INotifyPropertyChanged
{
    // 'Name' / 'Decs' -> jsou v 'ElementConfig'

    // ('Ident' nemuze byt v 'ElementConfig' protoze se muze cist ze zarizeni a 'ElementConfig' byt vubec nemusi)
    // identifikace pro 'Element'
    public byte Ident { get; set; } = 0;
    public DataConsts.EnElmType ElmType { get; set; } = DataConsts.EnElmType.None;

    public DataConsts.EnElmStatus ElmStatus { get; set; } = DataConsts.EnElmStatus.None;

    private DataConsts.EnDataStatus status = DataConsts.EnDataStatus.None;
    public DataConsts.EnDataStatus Status
    {
        get { return status; }
        set { if (status != value) { status = value; Status |= DataConsts.EnDataStatus.Changed; OnPropertyChanged("StatusInt"); } }
    }
    // pouze zakladni stav
    public int StatusInt { get { return (int)(Status & DataConsts.EnDataStatus.BaseMask); } }

    // --- ---

    private bool changedIn = false;
    public bool ChangedIn { get { return changedIn; } set { changedIn = value; OnPropertyChanged("ChangedIn"); OnPropertyChanged("ChangedStr"); RefDevice.ChangedIn = value; } }
    
    private bool changedOut = false;
    public bool ChangedOut { get { return changedOut; } set { changedOut = value; OnPropertyChanged("ChangedOut"); OnPropertyChanged("ChangedStr"); RefDevice.ChangedOut = value; } }
    public String ChangedStr { get { return ((changedIn == false) ? "-" : "In") + "/" + ((changedOut == false) ? "-" : "Out"); } }

    // pocet prijatych hodnot
    private UInt16 countIn = 0;
    public UInt16 CountIn { get { return countIn; } set { countIn = value; OnPropertyChanged("CountIn"); RefDevice.CountIn++; } }
    // pocet prijatych hodnot
    private UInt16 countOut = 0;
    public UInt16 CountOut { get { return countOut; } set { countOut = value; OnPropertyChanged("CountOut"); RefDevice.CountOut++; } }
    
    // --- ---
    
    public virtual void Clear() { CountIn = CountOut = 0; Status = DataConsts.EnDataStatus.None; }

    // --- ---

    public virtual string ValueStr { get; set; } = String.Empty;
    public virtual string ValueLog { get { return (Status.HasFlag(DataConsts.EnDataStatus.Active) == true) ? "1" : "NaN"; } }

    public virtual string GetValueEval(string type = null)
    {
        // pokud je potreba 'Status', tak se vrati
        if (type == "Status") { return (Status & DataConsts.EnDataStatus.BaseMask).ToString(); }
        // jinak neni nic
        return null;
    }

    // --- ---

    // odkaz na konfiguraci pro tento 'Element'
    public Configs.ElementConfigBase ElementConfig { get; set; } = null;

    // odkaz na nadrazeny 'Device'
    public ItmDevice RefDevice { get; set; } = null;

    // odkaz na jakykoli / vlastni objekt
    public object Tag { get; set; } = null;

    public Communs.CmnConsts.DelElementChanged OnElementChanged = null;

    // --- ---

    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
}

// 'Element' obsahujici data
public class ItmElement : ItmElementBase
{
    public ItmElement() { }

    // zda je 'Element' (take) vstupni
    public bool IsIn
    {
        get
        {
            bool isIn = false;
            foreach(ItmDataBase dataValueBase in Items) { if (dataValueBase.IsIn == true) { isIn = true; } }
            return isIn;
        }
    }
    // zda je 'Element' (take) vystupni
    public bool IsOut
    {
        get
        {
            bool isOut = false;
            foreach (ItmDataBase dataValueBase in Items) { if (dataValueBase.IsOut == true) { isOut = true; } }
            return isOut;
        }
    }

    // seznam obsazenych 'ItmData(s)'
    public ObservableCollection<ItmDataBase> Items { get; set; } = new ObservableCollection<ItmDataBase>();
}

// --- ItmData...(s) ---

public class ItmDataBase : INotifyPropertyChanged
{
    // 'Name' / 'Decs' -> jsou v 'DataConfig'

    // ('Ident' nemuze byt v 'DataConfig' protoze se muze cist ze zarizeni a 'DataConfig' byt vubec nemusi)
    // identifikace pro 'Element'
    public byte Ident { get; set; } = 0;
    // typ datove polozky (pro snadny test)
    public DataConsts.EnDataType DataType { get; set; } = DataConsts.EnDataType.None;
    // smer prensu datove polozky
    public Communs.CmnConsts.EnDir Dir { get; set; } = Communs.CmnConsts.EnDir.None;

    // --- ---

    // zda byla zmena
    private bool changed = false;
    public bool Changed
    {
        get { return changed; }
        set
        {
            changed = value; OnPropertyChanged("Changed"); OnPropertyChanged("ChangedStr");
            if (Dir == Communs.CmnConsts.EnDir.In) { if (RefElement != null) { RefElement.ChangedIn = value; } }
            if (Dir == Communs.CmnConsts.EnDir.Out) { if (RefElement != null) { RefElement.ChangedOut = value; } }
        }
    }
    public String ChangedStr { get { return (changed == false) ? "-" : "Changed"; } }

    // pocet zmen
    private UInt16 count = 0;
    public UInt16 Count
    {
        get { return count; }
        set
        {
            count = value; OnPropertyChanged("Count");
            if (Dir == Communs.CmnConsts.EnDir.In) { if (RefElement != null) { RefElement.CountIn++; } }
            if (Dir == Communs.CmnConsts.EnDir.Out) { if (RefElement != null) { RefElement.CountOut++; } }
        }
    }

    // posledni aktualizace
    public DateTime LastUpdate { get; set; } = DateTime.Now;

    // --- ---

    // zda je 'Element' (take) vstupni
    public bool IsIn { get { return Dir.HasFlag(Communs.CmnConsts.EnDir.In); } }
    // zda je 'Element' (take) vystupni
    public bool IsOut { get { return Dir.HasFlag(Communs.CmnConsts.EnDir.Out); } }

    // --- ---

    // odkaz na konfiguraci pro tento 'Data'
    public Configs.DataConfigBase DataConfig { get; set; } = null;

    // --- ---

    // odkaz na nadrazeny 'Element'
    public ItmElement RefElement { get; set; } = null;

    // --- ---

    public virtual void Reset() { Changed = false; Count = 0; }

    public virtual string ValueStr { get { return "X"; } set { } }

    // --- ---

    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
}

public class ItmDataBaseGener<T> : ItmDataBase
{
    public ItmDataBaseGener(DataConsts.EnDataType dataType) { DataType = dataType; }

    // interni datova hodnota
    protected T val = default(T);
    public T Value
    {
        get { return val; }
        set { val = value; Changed = true; Count++; OnPropertyChanged("Value"); OnPropertyChanged("ValueStr"); }
    }
    // vraceni hodnoty jako 'String' (pro strucny textovy vystup)
    public override string ValueStr { get { return (val == null) ? "---" : val.ToString(); } set { } }

}

public class ItmDataBool : ItmDataBaseGener<bool>
{
    public ItmDataBool() : base(DataConsts.EnDataType.Bool) { }

    public override string ValueStr => Value.ToString();
}

public class ItmDataUInt8 : ItmDataBaseGener<byte>
{
    public ItmDataUInt8() : base(DataConsts.EnDataType.UInt8) { }
    public byte ValueMin { get; set; } = Byte.MinValue;
    public byte ValueMax { get; set; } = Byte.MaxValue;
}

public class ItmDataInt8 : ItmDataBaseGener<sbyte>
{
    public ItmDataInt8() : base(DataConsts.EnDataType.Int8) { }
    public sbyte ValueMin { get; set; } = SByte.MinValue;
    public sbyte ValueMax { get; set; } = SByte.MaxValue;
}

public class ItmDataUInt16 : ItmDataBaseGener<UInt16>
{
    public ItmDataUInt16() : base(DataConsts.EnDataType.UInt16) { }
    public UInt16 ValueMin { get; set; } = UInt16.MinValue;
    public UInt16 ValueMax { get; set; } = UInt16.MaxValue;
}

public class ItmDataInt16 : ItmDataBaseGener<Int16>
{
    public ItmDataInt16() : base(DataConsts.EnDataType.Int16) { }
    public Int16 ValueMin { get; set; } = Int16.MinValue;
    public Int16 ValueMax { get; set; } = Int16.MaxValue;
}

public class ItmDataUInt32 : ItmDataBaseGener<UInt32>
{
    public ItmDataUInt32() : base(DataConsts.EnDataType.UInt32) { }
    public Int32 ValueMin { get; set; } = Int32.MinValue;
    public Int32 ValueMax { get; set; } = Int32.MaxValue;
}

public class ItmDataInt32 : ItmDataBaseGener<Int32>
{
    public ItmDataInt32() : base(DataConsts.EnDataType.Int32) { }
    public Int32 ValueMin { get; set; } = Int32.MinValue;
    public Int32 ValueMax { get; set; } = Int32.MaxValue;
}

public class ItmDataInt64 : ItmDataBaseGener<Int64>
{
    public ItmDataInt64() : base(DataConsts.EnDataType.Int64) { }
    public Int64 ValueMin { get; set; } = Int64.MinValue;
    public Int64 ValueMax { get; set; } = Int64.MaxValue;
}

public class ItmDataDouble : ItmDataBaseGener<double>
{
    public ItmDataDouble() : base(DataConsts.EnDataType.FloatBase) { }

    public override string ValueStr { get { return val.ToString("F4", System.Globalization.CultureInfo.InvariantCulture); } set { } }
}

public class ItmDataString : ItmDataBaseGener<string>
{
    public ItmDataString() : base(DataConsts.EnDataType.String) { }
}
public class ItmDataBytes : ItmDataBaseGener<byte[]>
{
    public ItmDataBytes() : base(DataConsts.EnDataType.Bytes) { }

    // pocet platnych hodnot v poli (nikoli velikost pole)
    public int Length { get; set; } = 0;

    // pro vlozeni binarnich dat
    public void SetValue(byte[] data, int start, int length)
    {
        // pokud pole jeste neni, nebo je male, tak se vytvori dostatecne velke
        if ((val == null) || (val.Length < (length - start)))
            { val = new byte[length - start]; }
        // prekopirovani dat do datove polozky
        Array.Copy(data, start, val, 0, length - start);
        // zapis delky platnych dat
        Length = length - start;
        // ...
        Changed = true; Count++;
        OnPropertyChanged("Value"); OnPropertyChanged("ValueStr");
    }

    public override string ValueStr
    {
        get { return (val == null) ? "---" : BitConverter.ToString(val).Replace("-", "."); }
        set { }
    }
}
