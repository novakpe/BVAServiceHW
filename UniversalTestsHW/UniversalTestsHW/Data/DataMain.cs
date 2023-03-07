// .NET
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
// Local
using UniTestsHW.Communs;

namespace UniTestsHW.Data;

public class DataMain
{
    // seznam evidovanych dat
    public static ObservableCollection<ItmMainBase> devices = new ObservableCollection<ItmMainBase>();

    //  zapis dat typu 'Out'
    // 'obj' muze byt 'ItmDevice' / 'ItmElement' / 'ItmData'
    public static async void DataWrite(object obj)
    {            
        // zapis do HW podle predane tridy
        await DataMain.DataWrite(
            (obj is ItmDevice) ? (ItmDevice)obj : null,
            (obj is ItmElement) ? (ItmElement)obj : null,
            (obj is ItmDataBase) ? (ItmDataBase)obj : null);
    }

    public static async Task<bool> DataWrite(ItmDevice device = null, ItmElement element = null, ItmDataBase data = null)
    {
        // pokud nejsou vsechny 'null', tak musi vzdy existovat ta vyssi uroven nez ta zadana
        // ('Data' -> 'Element' -> 'Device')

        // pokud je (zadany)  'Data' a neni 'Element' tak se musi 'Element' najit
        if ((data != null) && (element == null))
            { element = data.RefElement; }
        // pokud je (zadany/nelezeny) 'Element' a neni 'Device' tak se musi 'device' najit
        if ((element != null) && (device == null))
            { device = element.RefDevice; }
        // (I. tady je vse 'null' -> zapis vsech zmen u vsech 'Device(s)' / 'Element(s)' / 'Data(s)')
        // (II. pokud neni vse 'null', tak je minimalne 'Device' a mozna i 'Element' a pak mozna i 'Data')
        //    ('Device != null' -> zapis vsech zmen u vsech 'Element(s)' / 'Data(s)')
        //    ('Element != null' -> zapis vsech zmen u vsech 'Data(s)')

        // zapis do HW
        return await CmnMain.DataWrite(device, element);
    }

    // --- ---

    public static async void DataRead(object obj)
    {
        // zapis do HW podle predane tridy
        await DataMain.DataRead(
            (obj is ItmDevice) ? (ItmDevice)obj : null,
            (obj is ItmElement) ? (ItmElement)obj : null,
            (obj is ItmDataBase) ? (ItmDataBase)obj : null);
    }

    public static async Task DataRead(ItmDevice device = null, ItmElement element = null, ItmDataBase data = null)
    {
        // pokud nejsou vsechny 'null', tak musi vzdy existovat ta vyssi uroven nez ta zadana
        // ('Data' -> 'Element' -> 'Device')

        // pokud je (zadany)  'Data' a neni 'Element' tak se musi 'Element' najit
        if ((data != null) && (element == null))
            { element = data.RefElement; }
        // pokud je (zadany/nelezeny) 'Element' a neni 'Device' tak se musi 'device' najit
        if ((element != null) && (device == null))
            { device = element.RefDevice; }
        // (I. tady je vse 'null' -> zapis vsech zmen u vsech 'Device(s)' / 'Element(s)' / 'Data(s)')
        // (II. pokud neni vse 'null', tak je minimalne 'Device' a mozna i 'Element' a pak mozna i 'Data')
        //    ('Device != null' -> zapis vsech zmen u vsech 'Element(s)' / 'Data(s)')
        //    ('Element != null' -> zapis vsech zmen u vsech 'Data(s)')

        // zapis do HW
        await CmnMain.DataRead(device, element);
    }
}
