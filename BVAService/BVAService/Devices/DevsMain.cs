// .NET
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
// UnilTests
using UniTestsHW.Data;

namespace BVAService.Devices
{
    public class DevsMain
    {
        // seznam 'Anchor(s)'
        public static List<DeviceAnchor> devsAnchor = new List<DeviceAnchor>();

        // nacteni vsech dostupnych obrazku
        public static string[] LoadImages(string pathImages)
        {
            // vytvoreni 8x 'Anchor' pro test (pro zobarzeni v GUI v zalozce 'Manual')
            for (int i = 0; i < 8; i++)
            {
                // test zda tento 'Anchor' je skutecne pripojen
                if (DataUtils.ExistDevice((int.Parse(DevsCmns.EnDeviceIdent.AnchorsBase) + i + 1).ToString("D8")) == true)
                    // pokud ano, tak se zobrazi v seznamu dostupnych 'Anchor(s)'
                    { devsAnchor.Add(new DeviceAnchor() { Name = "Anchor " + (i + 1).ToString(), Number = (i + 1) }); }
            }
            // nacteni obrazku
            LoadImagesInt(pathImages);
            // vraceni seznamu vsech nactenych obrazku
            return GetImageNames();
        }

        // nacteni vsech dostupnych pozic
        public static string[] LoadPositions(string pathPositions)
        {
            // nacteni obrazku
            LoadPositionsInt(pathPositions);
            // vraceni seznamu vsech nactenych obrazku
            return LaserNames;
        }

        // zobrazeni informaci o nalezenych / nenalezenych zarizenich
        public static void ShowDevicesStatus()
        {
            // zobrazeni vsech nalezenych / dostupnych zarizeni pro zobrazeni 'Service'
            // (daty nezle pouzit navratovou hodnotu z 'FindAllDevice()', ta muze byt jiz volana externi aplikaci)
            // (je nutno projit seznam nalezenych 'Device(s)')
            // vytvoreni textoveho seznamu nalezenych USB zarizeni
            StringBuilder sb = new StringBuilder();
            // uvodni text
            sb.Append("Pocet pripojenych zarizeni: " + DataMain.devices.Count.ToString());
            sb.Append("\n\n");
            // projiti vsech pozadovanych zarizeni
            foreach (string ident in DevsCmns.DevsSerNumToName.Keys)
            {
                // test zda je toto zarizeni pripojeno / nalezeno
                ItmDevice itmDevice = (ItmDevice)DataMain.devices.FirstOrDefault(item => item.DeviceConfig.Ident == ident);
                // nalezeno / pripojeno
                if (itmDevice != null)
                {
                    // text do inforaci -> 'zarizeni je OK'
                    sb.Append(itmDevice.Ident).Append("  ").Append(DevsCmns.DevsSerNumToName[itmDevice.Ident]).Append(" -> [OK]\n");
                }
                // NEnalezeno / NEpripojeno
                else
                {
                    // text do inforaci -> 'zarizeni NENI'
                    sb.Append(ident).Append("  ").Append(DevsCmns.DevsSerNumToName[ident]).Append(" -> [Nenalezeno]").Append("\n");
                }
            }
            // hlaseni o nalezenych pripojenych zarizenich
            MessageBox.Show(sb.ToString(), "Nalezena USB zarizeni!");
        }

        // --- obrazky ---

        // typ obrazky (ulozeni dat)
        public enum EnImageDataType { None = 0, Type1 = 1, Type2 = 2 }
        // seznam dostupnych obrazku
        public static List<OneImage> images = new List<OneImage>();

        // nacteni vsech dostunych obrazku (interni)
        public static void LoadImagesInt(string directory)
        {
            System.IO.Directory.CreateDirectory(directory);
            // vsechny soubory s obrazky
            string[] files = System.IO.Directory.GetFiles(directory);
            // postupne zpracovani vsech souboru
            foreach (string file in files)
            {
                // pokud nazev souboru zacina '!', tak se preskoci
                if (System.IO.Path.GetFileNameWithoutExtension(file).StartsWith("!") == true) { continue; }
                // vytvoreni tridy pro obrazek
                OneImage oneImage = new OneImage();
                // nacteni vsech radku ze souboru
                string[] lines = System.IO.File.ReadAllLines(file);

                // typ uzlozenych dat obrazku
                EnImageDataType imageDataType = EnImageDataType.None;
                // radek obsahujici barvu (bude rozlozen pozdeji az se vytvori pole 'bytes')
                string colorStr = String.Empty;
                // radky obsahujici data obrazku
                List<string> imageStr = new List<string>();

                // projiti vsech radku souboru
                foreach (string line in lines)
                {
                    // pokud je to komnetar, tak se preskoci
                    if (line.StartsWith("!") == true) { continue; }
                    // tento radek obsahuje nazev obaazku
                    else if (line.StartsWith("@") == true) { oneImage.Name = line.Trim('@', ' '); }
                    // typ ulozeych / formatu dat
                    else if (line.StartsWith("$") == true) { imageDataType = Enum.Parse< EnImageDataType>(line.Trim('$', ' ')); }
                    // tento radek obsahuje barvu obrazku (pouze pro 'Type1')
                    else if (line.StartsWith("#") == true) { colorStr = line; }
                    // tento radek obsahuje bitova data obrazku (vlozi se do seznamu radku tvorici obrazek)
                    else { imageStr.Add(line.Trim()); }

                }

                // pro obrazek 'Type1'
                if (imageDataType == EnImageDataType.Type1)
                {
                    // vytvoreni pole pro vsechny data obrazku (barva + bitova mapa)
                    // (celkova velikost = typ + sizeX + sizeY + 3x Byte pro RGB + pocet v radku * pocet 8bit hodnot na radku)
                    // (8x8 => 14B / 16x16 -> 38B)
                    oneImage.Values = new byte[1 /* typ */ + 1 /* size X */ + 1 /* size Y */ + 3 /* brava RGB */ + imageStr.Count * (imageStr[0].Length / 8)];
                    // pozice zapisu do pole
                    int pos = 0;
                    // zapis typu ulozenych dat / obrazku (SizeX + SizeY + RGB-Color + Bitmap)
                    oneImage.Values[pos++ /* 0 */] = 1;
                    // nejprve je ulozen rozmer obrazku 'SizeX' a 'SizeY'
                    oneImage.Values[pos++ /* 1 */] = (byte)imageStr[0].Length; oneImage.Values[pos++ /* 2 */] = (byte)imageStr.Count;
                    // dale je ulozena brava v prvnich trech bytes, ve formatu R, G, B
                    string[] rgbStr = colorStr.Trim('#', ' ').Split(';', ',');
                    oneImage.Values[pos++] = Byte.Parse(rgbStr[0]); oneImage.Values[pos++] = Byte.Parse(rgbStr[1]); oneImage.Values[pos++] = Byte.Parse(rgbStr[2]);

                    // vyzvednuti bitove mapy obrazku
                    byte value = 0;
                    // postupne projiti vsech radku
                    foreach (string line in imageStr)
                    {
                        // postupne projiti vsech bitu na radku
                        for (int i = 0; i < line.Length; i++)
                        {
                            // bit7 <- bit6, ..., bit1 <- bit0
                            // (aby byly 'bits' v 'byte' v poradi v jakem jsou v 'TXT' souboru)
                            // (zcela levy = bit7, zcela pravy = bit0)
                            value <<= 1;
                            // pokud je na tomto miste v retezci '1' -> nastavi se nejvyssi bit v byte
                            value |= (byte)((line[i] == '1') ? 0x01 : 0x00);
                            // kazda osmice bites se musi ulozit (byte ma pouze 8 bites)
                            // (pokud je pozice 7, 15, 23, 31, ... -> tak je vzdy vyctenych celych 8 bites)
                            // (je to vzdy o '1' mensi nez nasobek '8-mi')
                            if (((i + 1) % 8) == 0) { oneImage.Values[pos++] = value; value = 0; }
                        }
                    }
                }
                // pro obrazek 'Type2'
                if (imageDataType == EnImageDataType.Type2)
                {

                }
                // vlozeni nacteneho obrazku do seznamu dostupnych obrazku
                images.Add(oneImage);
            }
        }

        // vraceni seznamu dostupnych obrazky pro 'ComboBox' v GUI
        public static string[] GetImageNames()
            { return images.Select(image => image.Name).ToArray(); }

        // --- pozice ---

        // seznam preddefinovanych pozic pro 'Laser'
        public static List<LaserPos> laserPositions = new List<LaserPos>();

        // nacteni vsech dostunych obrazku (interni)
        public static void LoadPositionsInt(string directory)
        {
            // vytvoreni pod-adresare pokud jeste neni (aby proste existoval)
            System.IO.Directory.CreateDirectory(directory);
            // ulozeni vsech dostupnych pozic
            List<LaserPos> positions = XUtils.Xml.XmlSerialization.XmlFileToClass<List<LaserPos>>(directory, "LaserPositions.xml");
            // pokud jsou nactene pozice, tak se vlozi do seznamu pozic
            if ((positions != null) && (positions.Count > 0)) { laserPositions.AddRange(positions); }
        }

        // ulozeni vsech dostunych obrazku (interni)
        public static void SavePositionsInt(string directory)
        {
            // vytvoreni pod-adresare pokud jeste neni
            System.IO.Directory.CreateDirectory(directory);
            // ulozeni vsech dostupnych pozic
            XUtils.Xml.XmlSerialization.ClassToXmlFile<List<LaserPos>>(directory, "LaserPositions.xml", laserPositions);
        }

        // vraceni seznamu dostupnych obrazky pro 'ComboBox' v GUI
        public static string[] LaserNames
            { get { return laserPositions.Select(position => position.Name).ToArray(); } }
    }
}
