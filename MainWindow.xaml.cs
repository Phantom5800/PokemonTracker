using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PokemonTracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private enum GameList
        {
            [Description("Let's Go")]
            LetsGo,
            [Description("Let's Go, Pikachu")]
            LetsGoPikachu,
            [Description("Let's Go, Eevee")]
            LetsGoEevee,
            [Description("Poképark")]
            Pokepark
        }

        // program styles
        private Style _pokemonSelectorStyle = new Style(typeof(ToggleButton));

        // settings
        public static int PokemonButtonSize { get; set; } = 50;

        // page state
        private int _pokemonCnt = 0;
        private ResourceManager _previousResourceSet = null;

        public MainWindow()
        {
            InitializeComponent();

            Utils.Preferences.OnApplyPreferences += Preferences_OnApplyPreferences;

            // disable keyboard and mouse events from accidentally scrolling the game list
            GameSelector.KeyDown += KeyDown_DropEvent;
            GameSelector.PreviewKeyDown += KeyDown_DropEvent;

            // build styles
            _pokemonSelectorStyle.TargetType = typeof(ToggleButton);
            _pokemonSelectorStyle.Setters.Add(new Setter(BackgroundProperty, Brushes.DarkGray));
            Trigger checkedTrigger = new Trigger();
            checkedTrigger.Property = ToggleButton.IsCheckedProperty;
            checkedTrigger.Value = true;
            checkedTrigger.Setters.Add(new Setter(ToggleButton.BorderThicknessProperty, new Thickness(5)));
            checkedTrigger.Setters.Add(new Setter(BackgroundProperty, Brushes.DarkRed)); // this doesn't work because Microsoft is incompetent
            _pokemonSelectorStyle.Triggers.Add(checkedTrigger);

            // get the more descriptive names out of the provided enums
            foreach (var game in Enum.GetValues(typeof(GameList)))
            {
                DescriptionAttribute[] desc = (DescriptionAttribute[])game.GetType().GetField(game.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (desc?.Length > 0)
                {
                    GameSelector.Items.Add(desc[0].Description);
                }
            }
        }

        private void Preferences_OnApplyPreferences()
        {
            foreach (var child in ImageSet.Children)
            {
                ToggleButton button = child as ToggleButton;
                if (button != null)
                {
                    button.Width = button.Height = PokemonButtonSize;
                }
            }
        }

        private void UpdateImageSet(GameList game)
        {
            PokemonCount.Text = "0";
            ImageSet.Children.Clear();
            _previousResourceSet?.ReleaseAllResources();

            switch (game)
            {
                case GameList.LetsGo:
                case GameList.LetsGoPikachu:
                case GameList.LetsGoEevee:
                    BuildImageSetFromResources(PokemonTracker.Resources.LetsGo.ResourceManager, GetDropFilter(game));
                    break;

                case GameList.Pokepark:
                    BuildImageSetFromResources(PokemonTracker.Resources.Pokepark.ResourceManager);
                    break;
            }
        }

        private string GetDropFilter(GameList game)
        {
            switch (game)
            {
                case GameList.LetsGoEevee:
                    return "(Pikachu)";
                case GameList.LetsGoPikachu:
                    return "(Eevee)";
            }
            return string.Empty;
        }

        /// <summary>
        /// Load images based on given parameters.
        /// </summary>
        /// <param name="resourceManager">Resource set to load images from.</param>
        /// <param name="dropFilter">If a resource file contains this string, do not include it in the results.</param>
        private void BuildImageSetFromResources(ResourceManager resourceManager, string dropFilter = "")
        {
            _previousResourceSet = resourceManager;
            ResourceSet resourceList = resourceManager.GetResourceSet(System.Globalization.CultureInfo.InvariantCulture, true, false);

            if (resourceList != null)
            {
                List<string> resourceKeys = new List<string>();
                foreach (DictionaryEntry resource in resourceList)
                {
                    string key = resource.Key as string;
                    // do not include any images with the drop filter set, used to filter out version specifics
                    if (dropFilter == "" || !key.Contains(dropFilter))
                    {
                        resourceKeys.Add(resource.Key as string);
                    }
                }
                resourceKeys.Sort();

                for (int i = 0; i < resourceKeys.Count; ++i)
                {
                    object img = resourceManager.GetObject(resourceKeys[i]);
                    BitmapSource convertedImg = (new ImageSourceConverter()).ConvertFrom(img) as BitmapSource;

                    ToggleButton toggle = new ToggleButton();
                    toggle.Width = PokemonButtonSize;
                    toggle.Height = PokemonButtonSize;
                    toggle.Style = _pokemonSelectorStyle;
                    toggle.Click += PokemonButton_Click;
                    toggle.KeyDown += KeyDown_DropEvent;
                    toggle.PreviewKeyDown += KeyDown_DropEvent;

                    Image image = new Image();
                    image.Source = convertedImg;

                    toggle.Content = image;
                    ImageSet.Children.Add(toggle);
                }
            }
        }

        private void KeyDown_DropEvent(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }

        private void PokemonButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton button = sender as ToggleButton;
            if (button != null && button.IsChecked != null)
            {
                bool selected = button.IsChecked ?? false;
                int change = (selected) ? 1 : -1;
                _pokemonCnt += change;

                PokemonCount.Text = _pokemonCnt.ToString();
            }
        }

        private void GameSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selected = (sender as ComboBox)?.SelectedIndex ?? 0;
            UpdateImageSet((GameList)selected);
        }

        private void File_Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void File_Preferences_Click(object sender, RoutedEventArgs e)
        {
            Utils.Preferences prefWnd = new Utils.Preferences();
            prefWnd.Show();
        }

        private void Help_About_Click(object sender, RoutedEventArgs e)
        {
            Utils.About aboutWnd = new Utils.About();
            aboutWnd.Show();
        }
    }
}
