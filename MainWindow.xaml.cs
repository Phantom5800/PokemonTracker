using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Resources;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PokemonTracker.Properties;

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
            [Description("Let's Go, Pikachu (All Obtainable)")]
            LetsGoPikachu,
            [Description("Let's Go, Eevee (All Obtainable)")]
            LetsGoEevee,
            [Description("Poképark")]
            Pokepark
        }

        // program styles
        private Style _pokemonSelectorStyle = new Style(typeof(ToggleButton));

        // settings
        public static int PokemonButtonSize { get; set; } = 50;
        public static bool ShowPlannedPokemon { get; set; } = true;

        // page state
        private int _pokemonCnt = 0;
        private int _plannedCnt = 0;
        private ResourceManager _previousResourceSet = null;

        private class PokemonButton : ToggleButton
        {
            public enum ButtonState
            {
                Unselected,
                Planned,
                Selected
            }

            private ButtonState _state = ButtonState.Unselected;
            public ButtonState State
            {
                get
                {
                    return _state;
                }

                set
                {
                    switch (value)
                    {
                        case ButtonState.Unselected:
                            Background = Brushes.LightGray;
                            BorderBrush = Brushes.DarkGray;
                            _state = ButtonState.Unselected;
                            break;
                        case ButtonState.Planned:
                            Background = Brushes.DarkRed;
                            BorderBrush = Brushes.IndianRed;
                            _state = ButtonState.Planned;
                            break;
                        case ButtonState.Selected:
                            Background = Brushes.DarkBlue;
                            BorderBrush = Brushes.CornflowerBlue;
                            _state = ButtonState.Selected;
                            break;
                    }
                }
            }
        }

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

            PokemonCount.Text = (ShowPlannedPokemon) ? "0 (0)" : "0";
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
            _pokemonCnt = 0;
            _plannedCnt = 0;
            PokemonCount.Text = (ShowPlannedPokemon) ? "0 (0)" : "0";
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

        private string[] GetDropFilter(GameList game)
        {
            switch (game)
            {
                case GameList.LetsGoEevee:
                    return new string[] { "(Pikachu)", "(Trade)", "(Mythic)" };
                case GameList.LetsGoPikachu:
                    return new string[] { "(Eevee)", "(Trade)", "(Mythic)" };
            }
            return null;
        }

        /// <summary>
        /// Load images based on given parameters.
        /// </summary>
        /// <param name="resourceManager">Resource set to load images from.</param>
        /// <param name="dropFilter">If a resource file contains this string, do not include it in the results.</param>
        private void BuildImageSetFromResources(ResourceManager resourceManager, string[] dropFilter = null)
        {
            _previousResourceSet = resourceManager;
            ResourceSet resourceList = resourceManager.GetResourceSet(System.Globalization.CultureInfo.InvariantCulture, true, false);

            if (resourceList != null)
            {
                List<string> resourceKeys = new List<string>();
                foreach (DictionaryEntry resource in resourceList)
                {
                    string key = resource.Key as string;
                    bool drop = false;
                    if (dropFilter != null)
                    {
                        for (int i = 0; i < dropFilter.Length; ++i)
                        {
                            if (key.Contains(dropFilter[i]))
                            {
                                drop = true;
                                break;
                            }
                        }
                    }
                    // do not include any images with the drop filter set, used to filter out version specifics
                    if (!drop)
                    {
                        resourceKeys.Add(resource.Key as string);
                    }
                }
                resourceKeys.Sort();

                for (int i = 0; i < resourceKeys.Count; ++i)
                {
                    object img = resourceManager.GetObject(resourceKeys[i]);
                    BitmapSource convertedImg = (new ImageSourceConverter()).ConvertFrom(img) as BitmapSource;

                    PokemonButton toggle = new PokemonButton();
                    toggle.Background = Brushes.LightGray;
                    toggle.Width = PokemonButtonSize;
                    toggle.Height = PokemonButtonSize;
                    toggle.Style = _pokemonSelectorStyle;
                    toggle.BorderBrush = Brushes.DarkGray;
                    toggle.BorderThickness = new Thickness(3);
                    toggle.Click += PokemonButton_Click;
                    toggle.KeyDown += KeyDown_DropEvent;
                    toggle.PreviewKeyDown += KeyDown_DropEvent;
                    toggle.MouseRightButtonDown += Toggle_MouseRightButtonDown;

                    Image image = new Image();
                    image.Source = convertedImg;

                    toggle.Content = image;
                    ImageSet.Children.Add(toggle);
                }
            }
        }

        /// <summary>
        /// Handle right click event on PokemonButton, this is used to set planned captures.
        /// </summary>
        private void Toggle_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            PokemonButton button = sender as PokemonButton;
            if (button != null && button.IsChecked != null)
            {
                if (button.State == PokemonButton.ButtonState.Unselected)
                {
                    button.State = PokemonButton.ButtonState.Planned;
                    ++_plannedCnt;
                }
                else if (button.State == PokemonButton.ButtonState.Planned)
                {
                    button.State = PokemonButton.ButtonState.Unselected;
                    --_plannedCnt;
                }

                // Update pokemon count displays
                if (ShowPlannedPokemon)
                {
                    PokemonCount.Text = string.Format("{0} ({1})", _pokemonCnt, _plannedCnt);
                }
                else
                {
                    PokemonCount.Text = _pokemonCnt.ToString();
                }
            }
        }

        private void KeyDown_DropEvent(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }

        /// <summary>
        /// Handle left click event on PokemonButton, this is used for tracking completed captures.
        /// </summary>
        private void PokemonButton_Click(object sender, RoutedEventArgs e)
        {
            PokemonButton button = sender as PokemonButton;
            if (button != null && button.IsChecked != null)
            {
                button.IsChecked = false;

                // update button visuals and counts
                if (button.State == PokemonButton.ButtonState.Selected)
                {
                    button.State = PokemonButton.ButtonState.Unselected;
                    --_pokemonCnt;
                    --_plannedCnt;
                }
                else
                {
                    if (button.State == PokemonButton.ButtonState.Unselected)
                    {
                        ++_plannedCnt;
                    }
                    ++_pokemonCnt;
                    button.State = PokemonButton.ButtonState.Selected;
                }

                // Update pokemon count displays
                if (ShowPlannedPokemon)
                {
                    PokemonCount.Text = string.Format("{0} ({1})", _pokemonCnt, _plannedCnt);
                }
                else
                {
                    PokemonCount.Text = _pokemonCnt.ToString();
                }
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

        private string GetResetDataString(GameList game)
        {
            switch (game)
            {
                case GameList.LetsGoEevee:
                    return Settings.Default.LGEReset;
                case GameList.LetsGoPikachu:
                    return Settings.Default.LGPReset;
                case GameList.Pokepark:
                    return Settings.Default.PokeparkReset;

                default:
                    return string.Empty;
            }
        }

        private void SetResetDataString(GameList game, string resetInfo)
        {
            switch (game)
            {
                case GameList.LetsGoEevee:
                    Settings.Default.LGEReset = resetInfo;
                    break;
                case GameList.LetsGoPikachu:
                    Settings.Default.LGPReset = resetInfo;
                    break;
                case GameList.Pokepark:
                    Settings.Default.PokeparkReset = resetInfo;
                    break;
            }

            Settings.Default.Save();
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            _pokemonCnt = 0;
            _plannedCnt = 0;

            string resetDataStr = GetResetDataString((GameList)GameSelector.SelectedIndex);
            List<int> resetData = new List<int>();
            if (!string.IsNullOrEmpty(resetDataStr))
            {
                string[] resetDataPoints = resetDataStr.Split(',');
                for (int i = 0; i < resetDataPoints.Length; ++i)
                {
                    if (int.TryParse(resetDataPoints[i], out int value))
                    {
                        resetData.Add(value);
                    }
                }
            }

            for (int i = 0; i < ImageSet.Children.Count; ++i)
            {
                PokemonButton button = ImageSet.Children[i] as PokemonButton;
                if (button != null)
                {
                    if (resetData.Count > 0 && resetData[0] == i)
                    {
                        resetData.Remove(i);
                        button.State = PokemonButton.ButtonState.Planned;
                        ++_plannedCnt;
                    }
                    else
                    {
                        button.State = PokemonButton.ButtonState.Unselected;
                    }
                }
            }

            PokemonCount.Text = string.Format("{0} ({1})", _pokemonCnt, _plannedCnt);
        }

        private void SavePlanned_Click(object sender, RoutedEventArgs e)
        {
            List<int> resetData = new List<int>();
            for (int i = 0; i < ImageSet.Children.Count; ++i)
            {
                PokemonButton button = ImageSet.Children[i] as PokemonButton;
                if (button != null)
                {
                    if (button.State != PokemonButton.ButtonState.Unselected)
                    {
                        resetData.Add(i);
                    }
                }
            }

            string resetDataStr = "";
            for (int i = 0; i < resetData.Count; ++i)
            {
                resetDataStr += resetData[i].ToString();
                if (i != resetData.Count - 1)
                {
                    resetDataStr += ",";
                }
            }

            GameList selectedGame = (GameList)GameSelector.SelectedIndex;
            SetResetDataString(selectedGame, resetDataStr);
            MessageBox.Show($"Planned catches for \"{(selectedGame.GetType().GetField(selectedGame.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false)[0] as DescriptionAttribute).Description}\" have been saved.");
        }
    }
}
