using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PokemonTracker.Utils
{
    /// <summary>
    /// Interaction logic for NumberSelector.xaml
    /// </summary>
    public partial class NumberSelector : UserControl
    {
        public delegate void ValueUpdatedCallback();
        public event ValueUpdatedCallback OnValueUpdated;

        private int _value = 0;
        public int Value 
        { 
            get { return _value; }
            set
            { 
                _value = Math.Clamp(value, MinValue, MaxValue);
                TextField.Text = _value.ToString();
                OnValueUpdated?.Invoke();
            }
        }
        public int MaxValue { get; set; }
        public int MinValue { get; set; }

        public NumberSelector()
        {
            InitializeComponent();
        }

        private void Button_Increment(object sender, RoutedEventArgs e)
        {
            ++Value;
        }

        private void Button_Decrement(object sender, RoutedEventArgs e)
        {
            --Value;
        }

        private void TextField_FilterBadInputs(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9.-]+");
            bool inputValid = !regex.IsMatch(e.Text);
            e.Handled = !inputValid;
        }

        private void TextField_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(TextField.Text, out int value))
            {
                if (value >= MinValue && value <= MaxValue)
                {
                    Value = value;
                }
            }
        }

        private void TextField_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (int.TryParse(TextField.Text, out int value))
                {
                    Value = value;
                }
            }
        }

        private void TextField_LostFocus(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(TextField.Text, out int value))
            {
                Value = value;
            }
        }
    }
}
