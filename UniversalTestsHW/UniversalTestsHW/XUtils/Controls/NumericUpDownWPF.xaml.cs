// .NET
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

// Petr Novak 2021-02-27

// Min (double = 0) / Max (double = 100) - rozsah vystupni hodnoty
//  (nejprve nastavit 'Max' a teprve pote 'Min')
// Step (double = 1) - krok hodnoty
// DecPlaces (int = 0) - kolik je zobrazeno desetinnych mist za carkou
// RightZeroes (bool = True) - zda jsou zobrazeni nuly z prava
// ValueType (EnType) - typ zobrazovane a vystupni hodnoty
// Value (double) - vstupni / vystupni hodnota
// ValueChanged(object sender, double value) - udalost pri zmene hodnoty
// OnValueText(NumericUpDown numUpDown, double val, string valText = null) - pro zmenu zobrazene hodnoty

// Poznamky:
// - pokud se pouzivaji desetinna cisla, tak musi byt 'Values' i 'Value' typu 'double' !!!

// co obsahuje 'Stepper'
// - Values = List<int> OR List<double> -> 'Stepper' je index do zadaneho pole
// - Values = null -> 'Stepper' je vstupni / vystupni hodnota

// XAML
// xmlns:NameSpace= "clr-namespace:Project.Tasks"
// Values="{Binding Path=(NameSpace:Trida.Vlastnost)}"
// (Vlastnost vraci List<double>/List<int>)

namespace XUtils.GUIControls
{
    internal partial class NumericUpDown : UserControl
    {
        // Style="{StaticResource StyleCfgNumUpDownDbl}" Min="0.1" Max="20"

        // typ vystupni hodnoty
        [Flags] public enum EnType
        {
            ShowValue = 1, ShowIndex = 2,       // jaka hodnota se zobrazuje
            ReturnValue = 4, ReturnIndex = 8,   // jaka hodnota se vraci do kodu
            // toto se puziva pro nastaveni (XAML/C#)
            ShowValueReturnValue = ShowValue | ReturnValue,
            ShowIndexReturnValue = ShowIndex | ReturnValue,
            ShowValueReturnIndex = ShowValue | ReturnValue,
            ShowIndexReturnIndex = ShowIndex | ReturnValue
        }
        // zmena vystupni hodnoty
        public delegate void DelValueChange(object sender, double value);
        public event DelValueChange ValueChange = null;
        // moznost vlastniho zobrazeni hodnoty
        public delegate string DelValueText(NumericUpDown numUpDown, double val, string valText = null);
        public event DelValueText OnValueShow = null;

        // interni hodnota ('Stepper')
        private double stepper = 0f; // Double.NaN;

        public NumericUpDown()
        {
            InitializeComponent();

            // inicializace 'Min' a 'Mix' je ve vytvoreni 'DependencyProperty'
            ValueToShow();
        }

        // --- parametry ---

        // text pro jednotky
        public string ValueText
        {
            get { return (string)GetValue(ValueTextProperty); }
            set { SetValue(ValueTextProperty, value); }
        }

        public static readonly DependencyProperty ValueTextProperty = DependencyProperty.Register("ValueText", typeof(string), typeof(NumericUpDown),
            new PropertyMetadata(String.Empty, (sender, args) =>
            {
                NumericUpDown n = (NumericUpDown)sender;
                n.ValueText = (string)args.NewValue;
                n.ValueToShow();
            }));

        // --- ---

        // minimalni hodnota
        public double Min
        {
            get { return (double)GetValue(MinProperty); }
            set { SetValue(MinProperty, value); }
        }

        public static readonly DependencyProperty MinProperty = DependencyProperty.Register("Min", typeof(double), typeof(NumericUpDown),
            new PropertyMetadata((double)0.0, (sender, args) =>
            {
                NumericUpDown n = (NumericUpDown)sender;
                n.Min = (double)args.NewValue;
                n.ValueToShow();
            }));

        // maximalni hodnota
        public double Max
        {
            get { return (double)GetValue(MaxProperty); }
            set { SetValue(MaxProperty, value); }
        }

        public static readonly DependencyProperty MaxProperty = DependencyProperty.Register("Max", typeof(double), typeof(NumericUpDown),
            new PropertyMetadata((double)100.0, (sender, args) =>
            {
                NumericUpDown n = (NumericUpDown)sender;
                n.Max = (double)args.NewValue;
                n.ValueToShow();
            }));

        // krok pri zmene hodnoty
        public double Step
        {
            get { return (double)GetValue(StepProperty); }
            set { SetValue(StepProperty, value); }
        }

        public static readonly DependencyProperty StepProperty = DependencyProperty.Register("Step", typeof(double), typeof(NumericUpDown),
            new PropertyMetadata((double)1, (sender, args) =>
            {
                NumericUpDown n = (NumericUpDown)sender;
                n.Step = (double)args.NewValue;
                n.ValueToShow();
            }));

        // pocet zobrazovanych mist za teckou
        public int DecPlaces
        {
            get { return (int)GetValue(DecPlacesProperty); }
            set { SetValue(DecPlacesProperty, value); }
        }

        public static readonly DependencyProperty DecPlacesProperty = DependencyProperty.Register("DecPlaces", typeof(int), typeof(NumericUpDown),
            new PropertyMetadata((int)0, (sender, args) =>
            {
                NumericUpDown n = (NumericUpDown)sender;
                n.DecPlaces = (int)args.NewValue;
                n.ValueToShow();
            }));

        // zda se zobrazuji nuly zprava (za desetinnou teckou)
        public bool RightZeroes
        {
            get { return (bool)GetValue(RightZeroesProperty); }
            set { SetValue(RightZeroesProperty, value); }
        }

        public static readonly DependencyProperty RightZeroesProperty = DependencyProperty.Register("RightZeroes", typeof(bool), typeof(NumericUpDown),
            new PropertyMetadata((bool)true, (sender, args) =>
            {
                NumericUpDown n = (NumericUpDown)sender;
                n.RightZeroes = (bool)args.NewValue;
                n.ValueToShow();
            }));

        // --- ---

        // typ vstupni / vystupni hodnoty
        public EnType ValueType
        {
            get { return (EnType)GetValue(PropertyValueType); }
            set { SetValue(PropertyValueType, value); }
        }

        public static readonly DependencyProperty PropertyValueType = DependencyProperty.Register("ValueType", typeof(EnType), typeof(NumericUpDown),
            new PropertyMetadata(EnType.ShowValueReturnValue, (sender, args) =>
            {
                NumericUpDown n = (NumericUpDown)sender;
                n.ValueType = (EnType)args.NewValue;
            }));

        // vystupni / vstupni hodnota
        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(double), typeof(NumericUpDown),
            new PropertyMetadata((double)0.0 /* tady musi byt cislo typu 'float' 0.0f / 'double' (double)0.0 */, (sender, args) =>
            {
                NumericUpDown n = (NumericUpDown)sender;
                n.stepper = n.ValueToStepper((double)args.NewValue); n.ValueToShow();
            }));

        // --- ---

        // seznam dostupnych hodnoty (jinak 'null')
        public object Values
        {
            get { return (object)GetValue(PropertyValues); }
            set { SetValue(PropertyValues, value); }
        }

        public static readonly DependencyProperty PropertyValues = DependencyProperty.Register("Values", typeof(object), typeof(NumericUpDown),
            new PropertyMetadata(null, (sender, args) =>
            {
                NumericUpDown n = (NumericUpDown)sender;
                // ulozeni sezanmu hodnot
                n.Values = (object)args.NewValue;
                // pokud je seznam hodnot, tak je vzdy 'Min = 0'
                n.Min = 0;
                // 'Max' je rovno poctu prvku v poli
                if (args.NewValue is List<int>) { n.Max = (((List<int>)args.NewValue).Count - 1); }
                if (args.NewValue is List<double>) { n.Max = (((List<double>)args.NewValue).Count - 1); }
                // vyzchozi zobrazeny prvek bude ten prvni v poli
                n.stepper = n.ValueToStepper(0); n.ValueToShow();
            }));

        // --- akce uzivatele ---

        // snizeni / zvyseni hodnoty
        private void ButtonOnClick(object sender, RoutedEventArgs e)
        {
            // snizeni / zvyseni hodnoty
            if ((String)((Button)sender).Tag == "Minus") { stepper -= Step; } else { stepper += Step; }

            // kontrola pripustnosti a nastaveni nove interni hodnoty
            // pokud je nova hodnota mensi nez minimum, tak se pouzije minimum
            if (stepper < Min) { stepper = Min; }
            // pokud je nova hodnota vetsi nez maximum, tak se pouzije maximum
            if (stepper > Max) { stepper = Max; }
            // ve 'val' je nova hodnota

            // vytvoreni 'Value' ze 'Stepper' a zobrazeni aktualniho hodnoty (hodnota/index)
            double petr = StepperToValue(stepper); ValueToShow();
            Value = StepperToValue(stepper); ValueToShow();
            // pokud ma nekdo zajem o zmenu hodnoty, tak se informuje
            // (delegat je stejne 'double', takze se preda 'double')
            if (ValueChange != null)
            {
                // pokud jde o 'List<int>' AND zobrazuje se hodnota
                if ((Values is List<int>) && (ValueType.HasFlag(EnType.ShowValue) == true))
                {
                    // vyzvednuti hodnoty z 'List' podle hodnoty 'Stepper' (indexu)
                    ValueChange(this, ((List<int>)Values)[(int)stepper]);
                }
                // pokud jde o 'List<double>' AND zobrazuje se hodnota
                else if ((Values is List<double>) && (ValueType.HasFlag(EnType.ShowValue) == true))
                {
                    // vyzvednuti hodnoty z 'List' podle hodnoty 'Stepper' (indexu)
                    ValueChange(this, ((List<double>)Values)[(int)stepper]);
                }
                // pokud jde o 'prostou' hodnotu OR vraci se index
                else
                {
                    ValueChange(this, stepper);
                }
            }
        }

        // --- 'Value' -> 'Stepper' ---

        // nastavuje se hodnota pro 'Stepper'
        // pokud 'Values = List<int> OR List<double>' -> 'Stepper' je 'Value = List[Stepper]' ('Steper' je index)
        // pokud 'Values = null' -> 'Stepper' je primo 'Value'
        private double ValueToStepper(double val)
        {
            // pokud 'Values = List<int> OR List<double>' -> 'Stepper' je 'Value = List[Stepper]' (index)
            // (je predano pole hodnot, 'Stepper' je index v tomto poli)
            if ((Values is List<int>) || (Values is List<double>))
            {
                // v poli se pujde od posledni hodnoty
                int index = ((Values is List<int>) ? ((List<int>)Values).Count : ((List<double>)Values).Count);
                while (--index >= 0)
                {
                    // pokud hodnota v poli nalezena, tak se konci hledani
                    if (Values is List<int>)
                        { if ((int)val == ((List<int>)Values)[index]) { return index; } }
                    if (Values is List<double>)
                        { if (val == ((List<double>)Values)[index]) { return index; } }
                };
                // vratil se platny index do pole nebo '0' (coz je v podstate taky index do pole)
                return 0;
            }
            // pokud 'Values = null' -> 'Stepper' je primo 'Value'
            // (meni se primo vstupni / vystupni hodnota)
            else { return val; }
        }

        // --- 'Stepper' -> 'Value' ---

        // nastavuje se 'Value'
        // pokud 'Values = List<int> OR List<double>' -> 'Value = List[Stepper]' ('Steper' je index)
        // pokud 'Values = null' -> 'Value = Stepper'
        private double StepperToValue(double val)
        {
            // pokud 'Values = List<int>' -> 'Value = List[Stepper]'
            // (je predano pole hodnot, 'Stepper' je index v tomto poli)
            if (Values is List<int>) { return ((List<int>)Values)[(int)val]; }
            // pokud 'Values = List<double>' -> 'Value = List[Stepper]'
            // (je predano pole hodnot, 'Stepper' je index v tomto poli)
            else if (Values is List<double>) { return ((List<double>)Values)[(int)val]; }
            // pokud 'Values = null' -> 'Value = Stepper'
            // (meni se primo vstupni / vystupni hodnota)
            else { return val; }
        }

        // --- zobrazeni hodnoty ---

        // zobrazeni hodnoty podle 'Stepper' (nikoli podle 'Value')
        private void ValueToShow()
        {
            // pokud zadana metoda pro vlastni zobrazeni tak se pouzije
            if (OnValueShow != null)
            {
                // vytvoreni zcela vlastniho textu 
                tbValue.Text = OnValueShow(this, stepper, ValueText);
                // dale se jiz nepokracuje
                return;
            }

            // pokud jde o 'List<int>' AND zobrazuje se hodnota
            if ((Values is List<int>) && (ValueType.HasFlag(EnType.ShowValue) == true))
            {
                // vyzvednuti a zobrazeni hodnoty z 'Array' podle hodnoty 'Stepper' (indexu)
                tbValue.Text = String.Format("{0:D}{1}", ((List<int>)Values)[(int)stepper], ValueText);
            }
            // pokud jde o 'List<double>' AND zobrazuje se hodnota
            else if ((Values is List<double>) && (ValueType.HasFlag(EnType.ShowValue) == true))
            {
                // vyzvednuti hodnoty z 'Array' podle hodnoty 'Stepper' (indexu)
                tbValue.Text = String.Format("{0:F" + DecPlaces.ToString() + "}{1}", ((List<double>)Values)[(int)stepper], ValueText);
                // pokud nemaji byt na konci '0', tak se odtrhnout (pripadne i s desetinnou carkou / teckou)
                if (RightZeroes == false)
                    { tbValue.Text = tbValue.Text.TrimEnd('0'); tbValue.Text = tbValue.Text.TrimEnd('.', ','); }
            }
            // pokud jde o 'prostou' hodnotu OR zobrazuje se index
            else
            {
                // textove zobrazeni nove hodnoty (na zadany pocet desetinnych mist) a doplnkoveho textu
                tbValue.Text = String.Format("{0:F" + DecPlaces.ToString() + "}{1}", stepper, ValueText);
                // pokud nemaji byt na konci '0', tak se odtrhnout (pripadne i s desetinnou carkou / teckou)
                if (RightZeroes == false)
                    { tbValue.Text = tbValue.Text.TrimEnd('0'); tbValue.Text = tbValue.Text.TrimEnd('.', ','); }
            }
        }

        //// Dependency Property
        //public static readonly DependencyProperty TextBoxBackgroundProperty =
        //     DependencyProperty.Register("Background", typeof(System.Windows.Media.Brush),
        //     typeof(NumericUpDown), new FrameworkPropertyMetadata(null));

        //// .NET Property wrapper
        //public System.Windows.Media.Brush ButtonBackground
        //{
        //    get { return (System.Windows.Media.Brush)GetValue(TextBoxBackgroundProperty); }
        //    set { SetValue(TextBoxBackgroundProperty, value); }
        //}
    }
}
