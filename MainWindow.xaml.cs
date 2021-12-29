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
            [Description("Battle Network 6 Gregar")]
            BN6_Gregar
        }

        // program styles
        private Style _pokemonSelectorStyle = new Style(typeof(ToggleButton));

        // settings
        public static int PokemonButtonSize { get; set; } = 75;
        public static bool ShowPlannedPokemon { get; set; } = true;

        // page state
        private int _battleChipCnt = 0;
        private int _totalBattleChips = 0;
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
                        case ButtonState.Selected:
                            Background = Brushes.IndianRed;
                            BorderBrush = Brushes.DarkRed;
                            _state = ButtonState.Selected;
                            break;
                        case ButtonState.Planned:
                            Background = Brushes.DarkBlue;
                            BorderBrush = Brushes.CornflowerBlue;
                            _state = ButtonState.Planned;
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

            BattleChipCount.Text = "0 / 0";
        }

        private void Preferences_OnApplyPreferences()
        {
            foreach (var child in BattleChipSet.Children)
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
            _battleChipCnt = 0;
            _totalBattleChips = 0;
            BattleChipSet.Children.Clear();
            _previousResourceSet?.ReleaseAllResources();

            switch (game)
            {
                case GameList.BN6_Gregar:
                    BuildImageSetFromResources(PokemonTracker.Resources.BN6Chips.ResourceManager, GetDropFilter(game));
                    break;
            }
            BattleChipCount.Text = $"0 / {_totalBattleChips}";
        }

        private string[] GetDropFilter(GameList game)
        {
            switch (game)
            {
                case GameList.BN6_Gregar:
                    return new string[] { "(Falzar)", "(Jp)" };
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
                _totalBattleChips = resourceKeys.Count;

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
                    toggle.BorderThickness = new Thickness(10);
                    toggle.Click += PokemonButton_Click;
                    toggle.KeyDown += KeyDown_DropEvent;
                    toggle.PreviewKeyDown += KeyDown_DropEvent;

                    Image image = new Image();
                    image.Source = convertedImg;

                    toggle.Content = image;
                    BattleChipSet.Children.Add(toggle);
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
                    --_battleChipCnt;
                }
                else
                {
                    ++_battleChipCnt;
                    button.State = PokemonButton.ButtonState.Selected;
                }

                // Update pokemon count displays
                BattleChipCount.Text = string.Format("{0} / {1}", _battleChipCnt, _totalBattleChips);
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
                case GameList.BN6_Gregar:
                    return Settings.Default.BN6_Gregar_Reset;

                default:
                    return string.Empty;
            }
        }

        private void SetResetDataString(GameList game, string resetInfo)
        {
            switch (game)
            {
                case GameList.BN6_Gregar:
                    Settings.Default.BN6_Gregar_Reset = resetInfo;
                    break;
            }

            Settings.Default.Save();
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            _battleChipCnt = 0;

            if (GameSelector.SelectedIndex >= 0)
            {
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

                for (int i = 0; i < BattleChipSet.Children.Count; ++i)
                {
                    PokemonButton button = BattleChipSet.Children[i] as PokemonButton;
                    if (button != null)
                    {
                        if (resetData.Count > 0 && resetData[0] == i)
                        {
                            resetData.Remove(i);
                            button.State = PokemonButton.ButtonState.Selected;
                            ++_battleChipCnt;
                        }
                        else
                        {
                            button.State = PokemonButton.ButtonState.Unselected;
                        }
                    }
                }
            }

            BattleChipCount.Text = string.Format("{0} / {1}", _battleChipCnt, _totalBattleChips);
            //PACount.Text = string.Format("{0} / {1}", _battleChipCnt, _totalBattleChips);
        }

        private void SavePlanned_Click(object sender, RoutedEventArgs e)
        {
            if (GameSelector.SelectedIndex >= 0)
            {
                List<int> resetData = new List<int>();
                for (int i = 0; i < BattleChipSet.Children.Count; ++i)
                {
                    PokemonButton button = BattleChipSet.Children[i] as PokemonButton;
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
}
