using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PokemonTracker.Utils
{
    /// <summary>
    /// Interaction logic for Preferences.xaml
    /// </summary>
    public partial class Preferences : Window
    {
        public delegate void ApplyPreferencesCallback();
        public static event ApplyPreferencesCallback OnApplyPreferences;

        public Preferences()
        {
            InitializeComponent();
            PokemonSize.Value = MainWindow.PokemonButtonSize;
        }

        private void ApplyChanges_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.PokemonButtonSize = PokemonSize.Value;
            OnApplyPreferences?.Invoke();
        }
    }
}
