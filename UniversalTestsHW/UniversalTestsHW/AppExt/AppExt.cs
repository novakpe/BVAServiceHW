// .NET
using System;

// pro otevreni / zobrazeni dialogu pokud je 'IniversalTestsHW' spousten z jine aplikace

namespace UniTestsHW.App;

public class AppExt
{
    // zda je (hlavni) dialog otevren z jine / externi aplikace 
    public static bool FromExternal { get; private set; } = false;
    private static UniversalTestsHW.MainWindow dlg = null;

    // zobrazeni ladiciho dialogu
    public static void ShowDebug(System.Windows.Window parent)
    {
        // dialog je otevren z jine / externi aplikace
        FromExternal = true;
        // vytvoreni dialogu, pokud neni
        // (pokud je zadan 'Owner', tak pri jeho ukonceni se ukonci i z neho zobrazene / spustene dialogy)
        if (dlg == null) { dlg = new UniversalTestsHW.MainWindow() { Owner = parent}; }
        // zobrazeni dialogu
        dlg.Show();
    }
}
