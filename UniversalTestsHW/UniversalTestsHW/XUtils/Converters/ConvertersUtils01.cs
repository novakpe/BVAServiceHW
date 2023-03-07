// .NET
using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

// Petr Novak
// 2022-04-23

// vlozit do zahlavi 'XAML'
//   xmlns:XUtilsConvertersUni01="clr-namespace:XUtils.ConvertersUni01"

namespace XUtils.ConvertersUni01
{
    // --- prevod 'bool' na  'NON bool' ---
    // vlozit do 'Resources' v 'XAML'
    //   <XUtilsConvertersUni01:BoolToNonBoolConverter x:Key="BoolToNonBoolConverter" />
    // priklady pouziti
    //   IsEnabled="{Binding ...(bool)..., Converter={StaticResource BoolToNonBoolConverter}}"
    //
    // '...(bool)...' - vlastnost typu 'bool'
    internal class BoolToNonBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            { return !(bool)value; }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            { throw new NotImplementedException(); }
    }

    // --- prevod 'bool' na  'Visibility Visible/Collapsed' ---
    // vlozit do 'Resources' v 'XAML'
    //    <XUtilsConvertersUni01:BoolToVisibilityConverter x:Key="BoolToVisibilityConverter" />
    // priklady pouziti
    //   Visibility="{Binding ...(bool)..., Converter={StaticResource BoolToVisibilityConverter}}"
    //   Visibility="{Binding ...(bool)..., Converter={StaticResource BoolToVisibilityConverter}, ConverterParameter=NON}"
    //
    // '...(bool)...' - vlastnost typu 'bool'
    // 'parameter' - 'null(neuveden)' => 'bool=true -> Visibility.Visible' / '!=null(neco)' => 'bool=false -> Visibility.Visible'
    //                                                                      (doporuceno napriklad 'NON' nebo '!')
    internal class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // pokud je vstup 'true', tak je vystup 'Visible'
            Visibility visibity = ((bool)value == true) ? Visibility.Visible : Visibility.Collapsed;
            // pokud je nejaky parametr, tak se vystup neguje (jakykoli parametr)
            if (parameter != null) { visibity = (visibity == Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible; }
            // vraceni vystupu
            return visibity;

            //// pokud neni zadan zadny parametr, tak 'bool=true -> Visibility.Visible'
            //if (parameter == null) { return ((bool)value == true) ? Visibility.Visible : Visibility.Collapsed; }
            //    // pokud je zadan nejaky parametr (jedno co), tak 'bool=false -> Visibility.Visible'
            //    else { return ((bool)value == false) ? Visibility.Visible : Visibility.Collapsed; }
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            { throw new NotImplementedException(); }
    }

    internal class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // pokud je retezec platny, tak je vystup 'Visible'
            Visibility visibity = ((value != null) && (value is string) && (String.IsNullOrWhiteSpace((string)value) == false)) ? Visibility.Visible : Visibility.Collapsed;
            // pokud je nejaky parametr, tak se vystup neguje
            if (parameter != null) { visibity = (visibity == Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible;  }
            // vraceni vystupu
            return visibity;

            //// pokud neni zadan zadny parametr, tak 'bool=true -> Visibility.Visible'
            //if (parameter == null) { return ((bool)value == true) ? Visibility.Visible : Visibility.Collapsed; }
            //// pokud je zadan nejaky parametr (jedno co), tak 'bool=false -> Visibility.Visible'
            //else { return ((value != null) && (String.IsNullOrWhiteSpace((string)value) == false)) ? Visibility.Visible : Visibility.Collapsed; }
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            { throw new NotImplementedException(); }
    }

    // --- prevod 'bool' na zadane 'Color' ---
    // vlozit do 'Resources' v 'XAML'
    //    <XUtilsConvertersUni01:BoolToColorConverter x:Key="BoolToColorConverter" />
    // priklady pouziti
    //   Foreground="{Binding ...(bool)..., Converter={StaticResource BoolToColorConverter}, ConverterParameter=Green}"
    //   Foreground="{Binding ...(bool)..., Converter={StaticResource BoolToColorConverter}, ConverterParameter=Black|Green}"
    //   Foreground="{Binding ...(bool)..., Converter={StaticResource BoolToColorConverter}, ConverterParameter=#000000|#FFFFFF}"
    //
    // '...(bool)...' - false => Color[0] / true => Color[1]
    // 'parameter' - obsahuje barvu jako 'Black' (nazev) / '#AABBCC'(ciselne)
    //   pokud je zadana jedna barva [0] => 'Black' / [1] => 'zadana barva'
    //   pokud jsou zadany dve barvy [0] => 'prvni zadana barva'  / [1] => 'druha zadana barva'
    internal class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // predvyplni se dve zakladni / cerne barvy
            string[] colorsStr = { "Black", "Black" };
            // pokud je zadana pouze jedna barva, tak je povazovana za druhou
            if (((string)parameter).Contains("|") == false) { colorsStr[1] = (string)parameter; }
            // pokud jsou zadany dve barvy (oddelene pomoci '|', tak se pouziji obe)
            else { colorsStr = ((string)parameter).Split("|"); }
            // vystupem je prvni nebo druha barva
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorsStr[((bool)value == false) ? 0 : 1]));
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            { throw new NotImplementedException(); }
    }

    // --- prevod 'int' na zadane 'Color' ---
    // vlozit do 'Resources' v 'XAML'
    //    <XUtilsConvertersUni01:IntToColorConverter x:Key="IntToColorConverter" />
    // priklady pouziti
    //   Foreground="{Binding ...(bool)..., Converter={StaticResource IntToColorConverter}, ConverterParameter=Green}"
    //   Foreground="{Binding ...(bool)..., Converter={StaticResource IntToColorConverter}, ConverterParameter=Black|Green}"
    //   Foreground="{Binding ...(bool)..., Converter={StaticResource IntToColorConverter}, ConverterParameter=#000000|#FFFFFF}"
    internal class IntToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // predvyplni se dve zakladni / cerne barvy
            string[] colorsStr = { "Black", "Black", "Black" };
            // pokud je zadana pouze jedna barva, tak je povazovana za druhou
            if (((string)parameter).Contains("|") == false) { colorsStr[1] = (string)parameter; }
            // pokud jsou zadany dve barvy (oddelene pomoci '|', tak se pouziji obe)
            else { colorsStr = ((string)parameter).Split("|"); }
            // vystupem je prvni nebo druha barva
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorsStr[(int)value]));
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            { throw new NotImplementedException(); }
    }
}
