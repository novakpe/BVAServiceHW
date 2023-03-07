// .NET
using System;
using System.Linq;
using System.Windows;
using System.Windows.Data;
// Local
using UniTestsHW.Data;

namespace UniTestsHW.Configs;

public class ConfigMain
{
    // nacteni konfigurace pro 'Element(s)'
    public static void LoadConfig(ItmDevice device)
    {
        // poud adresar neni, tak se vytvori
        System.IO.Directory.CreateDirectory(UniTestsHW.Global.GlobalConsts.PathDevsElmsInfo);
        // necten nazvu vsech dostupnych souboru
        string[] descFiles = System.IO.Directory.GetFiles(UniTestsHW.Global.GlobalConsts.PathDevsElmsInfo);
        // nalezeni toho co ma v nazvu pozadovany 'Ident'
        string ident = "(Ident=" + device.Ident + ")";
        string fileDesc = descFiles.FirstOrDefault(item => item.Contains(ident));
        // pokud takovy soubor nenaleen, tak navrat
        if (fileDesc == null) { return; }

        // nacteni obsahu tohoto souboru
        DevsElmsConfig config = XUtils.Xml.XmlSerialization.XmlFileToClass<DevsElmsConfig>(
            System.IO.Path.GetDirectoryName(fileDesc), System.IO.Path.GetFileName(fileDesc));
        // pokud se nepovedlo nacist, tak chyba a konec
        if (config == null)
        {
            MessageBox.Show("Nepovedlo se nacist soubor '" + System.IO.Path.GetFullPath(fileDesc) + "'", "!!! DescFile - Error !!!");
            return;
        }

        // vyzvednuti popisu pro tento 'Device' (prvni 'Device' v souboru)
        DeviceConfigBase deviceConfig = config.Devices[0];

        // zapis (dodatecnych) hodnot do 'Device'
        device.DeviceConfig = deviceConfig;
        // projit vsech 'Element(s)' v tomto 'Device'
        foreach (ElementConfigBase elementConfig in deviceConfig.Elements)
        {
            // nelezeni 'Element' podle jeho 'Ident'
            ItmElement element = (ItmElement)DataUtils.GetElement(deviceConfig.Ident, elementConfig.Ident);
            // pokud nenalezeno, tak chyba
            if (element == null)
                { /* App.AppConfig.Warning(deviceConfig.Ident, elementConfig.Ident.ToString(), "---", Communs.CmnConsts.EnDir.Out); */ continue; }

            // zapis (dodatecnych) hodnot do 'Element'
            element.ElementConfig = elementConfig;
            // ...
            foreach (DataConfigBase dataConfig in elementConfig.Datas)
            {
                // nelezeni 'Data' podle jeho 'Ident'
                ItmDataBase data = DataUtils.GetData(deviceConfig.Ident, elementConfig.Ident, dataConfig.Ident);
                // pokud nenalezeno, tak chyba
                if (data == null)
                    { /* App.AppConfig.Warning(deviceConfig.Ident, elementConfig.Ident.ToString(), data.Ident.ToString(), Communs.CmnConsts.EnDir.Out); */ continue; }
                // zapis (dodatecnych) hodnot do 'Element'
                data.DataConfig = dataConfig;
            }
        }

        // vynuceni aktualizace GIO pri zmenen polozek v 'ObservableCollection' nemajici 'OnPropertyChanged(...)'
        CollectionViewSource.GetDefaultView(DataMain.devices).Refresh();
    }

    // vytvoreni vzoru pro 'Config'
    private static void CreateConfigExample(string filePath, string fileName)
    {
        // vytvoreni adrseare (pokud by nebyl jiz vytvoren)
        System.IO.Directory.CreateDirectory(filePath);
        // jeden 'Device(s)'
        DeviceConfigBase deviceConfig = new DeviceConfigBase() { Ident = "123", Name = "Laser", Desc = "NaStrope" };
        // tri 'Element(s)' a // tri 'Data(s)'
        ElementConfigBase elementConfigA = new ElementConfigBase() { Ident = 1, Name = "PosX", Desc = "PosunX" };
        //configElementA
        deviceConfig.Elements.Add(elementConfigA);
        ElementConfigBase elementConfigB = new ElementConfigBase() { Ident = 2, Name = "PosY", Desc = "PosunY" };
        deviceConfig.Elements.Add(elementConfigB);
        ElementConfigBase elementConfigC = new ElementConfigBase() { Ident = 3, Name = "OnOff", Desc = "Zapnuti" };
        deviceConfig.Elements.Add(elementConfigC);
        // hlavni trida konfigurace
        DevsElmsConfig configTestExample = new DevsElmsConfig() { Location = "Example" };
        configTestExample.Devices.Add(deviceConfig);
        // ulozeni do souboru
        XUtils.Xml.XmlSerialization.ClassToXmlFile<DevsElmsConfig>(filePath, fileName, configTestExample);

    }
}
