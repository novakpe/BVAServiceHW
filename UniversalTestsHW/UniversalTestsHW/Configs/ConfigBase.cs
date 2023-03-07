// .NET
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
// Local
using UniTestsHW.Data;

namespace UniTestsHW.Configs;

// --- ---

// konfigurace 'Device(s)' a 'Sensor(s)' ve jenom (logickem) miste
[XmlRoot("Config")]
public class DevsElmsConfig
{
    [XmlAttribute("Location")] public string Location { get; set; } = DataConsts.LocationDef;

    [XmlArray("Devices")]
    [XmlArrayItem(typeof(DeviceConfigBase), ElementName = "DeviceConfig")]
    //[XmlArrayItem(typeof(DeviceConfigZigBee), ElementName = "DeviceConfigZigBee")]
    public List<DeviceConfigBase> Devices { get; set; } = new List<DeviceConfigBase>();
}

// bazova trida pro konfiguraci 'Device(s)'
// ('Device' nikdy neni 'Sensor', 'Device' obsahuje 'Sensor(s)')
public class DeviceConfigBase
{
    // logicke umisteni 'Device'
    [XmlIgnore] public string Location { get; set; } = DataConsts.LocationDef;

    [XmlAttribute("Ident")] public string Ident { get; set; } = DataConsts.NameDef;
    [XmlAttribute("Name")] public string Name { get; set; } = DataConsts.NameDef;
    [XmlAttribute("Desc")] public string Desc { get; set; } = DataConsts.DescDef;

    // seznam 'Element(s)' v tomto 'Device'
    [XmlArray("Elements")]
    [XmlArrayItem(typeof(ElementConfigBase), ElementName = "ElementConfig")]
    //[XmlArrayItem(typeof(ElementConfigZigBee), ElementName = "ElementConfigZigBee")]
    public List<ElementConfigBase> Elements { get; set; } = new List<ElementConfigBase>();
}

// bazova trida pro konfiguraci 'Sensor(s)'
public class ElementConfigBase
{
    [XmlAttribute("Ident")] public byte Ident { get; set; } = 0;
    [XmlAttribute("Name")] public string Name { get; set; } = DataConsts.NameDef;
    [XmlAttribute("Desc")] public string Desc { get; set; } = DataConsts.DescDef;

    // seznam 'Data(s)' v tomto 'Element'
    [XmlArray("Datas")]
    [XmlArrayItem(typeof(DataConfigBase), ElementName = "DataConfig")]
    //[XmlArrayItem(typeof(DataConfigZigBee), ElementName = "DataConfigZigBee")]
    public List<DataConfigBase> Datas { get; set; } = new List<DataConfigBase>();
}

// bazova trida pro konfiguraci 'Data(s)'
public class DataConfigBase
{
    [XmlAttribute("Ident")] public byte Ident { get; set; } = 0;
    [XmlAttribute("Name")] public string Name { get; set; } = DataConsts.NameDef;
    [XmlAttribute("Desc")] public string Desc { get; set; } = DataConsts.DescDef;
}
