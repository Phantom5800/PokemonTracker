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
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : Window
    {
        public About()
        {
            InitializeComponent();
            MIT.Text = System.IO.File.ReadAllText("./LICENSE.md");
            Controls.Text =
@"Left click - track Pokémon as captured.
Right click - track Pokémon as planned (cannot overwrite if already captured).
";

            Disclosure.Text =
@"Pokémon is property of The Pokémon Company, Nintendo Co. Ltd., Game Freak Co. Ltd. and Creatures Inc.

Phantom Games LLC. makes no claims to Pokémon names or images shown in this tool.

Pokémon Tracker is a freely available tool intended for tracking captures in speedruns.
Source code can be found at: https://github.com/Phantom5800/PokemonTracker.
";
        }
    }
}
