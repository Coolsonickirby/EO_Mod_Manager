using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EO_Mod_Manager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Game> games = new List<Game>();
        public static Game selected_game;
        public static ObservableCollection<Mod> current_mods;
        public MainWindow()
        {
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            
            //Game.CreateNewCombinedBundle(
            //    @"D:\Games\Etrian Odyssey Origins Collection\Etrian Odyssey HD\Etrian Odyssey_Data\StreamingAssets\aa\StandaloneWindows64\charabigimgs_assets_all_a59a4657768e489ed89b02121d6faa42.bundle",
            //    new List<string>()
            //    {
            //        @"D:\Games\Etrian Odyssey Origins Collection\Etrian Odyssey HD\mods\Naoto over Joker\Etrian Odyssey_Data\StreamingAssets\aa\StandaloneWindows64\charabigimgs_assets_all_a59a4657768e489ed89b02121d6faa42.bundle",
            //        @"D:\Games\Etrian Odyssey Origins Collection\Etrian Odyssey HD\mods\Masculine Ringo\Etrian Odyssey_Data\StreamingAssets\aa\StandaloneWindows64\charabigimgs_assets_all_a59a4657768e489ed89b02121d6faa42.bundle"
            //    },
            //    "TEST.bundle"
            //);

            //mods.Add(new Mod
            //{
            //    Author = "Coolsonickirby",
            //    Name = "Cool Mod",
            //    Version = "1.0.0",
            //    Description = "My very cool mod",
            //    enabled = false
            //});
            //mods.Add(new Mod
            //{
            //    Author = "dasdcz",
            //    Name = "Cool Mod",
            //    Version = "1.0.0",
            //    Description = "My very cool mod",
            //    enabled = false
            //});
            //mods.Add(new Mod
            //{
            //    Author = "tes",
            //    Name = "Cool Mod",
            //    Version = "1.0.0",
            //    Description = "My very cool mod",
            //    enabled = false
            //});

            if(!OnePathSet()) // If no game path is set
                ShowSettings();
            if (!OnePathSet()) // If no game path is set and we already asked the user for one but they refused, then just close
                Environment.Exit(0);

            SetupGames();
            InitializeComponent();
            cboGames.ItemsSource = games;
            cboGames.SelectedIndex = 0;
            dataMods.AutoGenerateColumns = false;
            dataMods.CanUserAddRows = false;
            dataMods.ItemsSource = ((Game)(cboGames.SelectedItem)).GameMods;
            dataMods.SelectionMode = DataGridSelectionMode.Single;
        }

        private void SetupGames()
        {
            if (Directory.Exists(Properties.Settings.Default.EO1_Path))
                games.Add(new Game()
                {
                    key = "eo1_data",
                    GameName = "Etrian Odyssey HD",
                    GameFolderDataName = "Etrian Odyssey_Data",
                    GamePath = Properties.Settings.Default.EO1_Path,
                    GameExecutable = Properties.Settings.Default.EO1_EXE,
                    GameMods = Game.GetModsFromPath(Properties.Settings.Default.EO1_Path)
                });
            if (Directory.Exists(Properties.Settings.Default.EO2_Path))
                games.Add(new Game()
                {
                    key = "eo2_data",
                    GameName = "Etrian Odyssey II HD",
                    GameFolderDataName = "Etrian Odyssey 2_Data",
                    GamePath = Properties.Settings.Default.EO2_Path,
                    GameExecutable = Properties.Settings.Default.EO2_EXE,
                    GameMods = Game.GetModsFromPath(Properties.Settings.Default.EO2_Path)
                });
            if (Directory.Exists(Properties.Settings.Default.EO3_Path))
                games.Add(new Game()
                {
                    key = "eo3_data",
                    GameName = "Etrian Odyssey III HD",
                    GameFolderDataName = "Etrian Odyssey 3_Data",
                    GamePath = Properties.Settings.Default.EO3_Path,
                    GameExecutable = Properties.Settings.Default.EO3_EXE,
                    GameMods = Game.GetModsFromPath(Properties.Settings.Default.EO3_Path)
                });

            foreach (Game game in games)
                game.LoadDataFromKey();
        }

        private void btnMoveDown_Click(object sender, RoutedEventArgs e)
        {
            if (dataMods.SelectedIndex == -1)
                return;
            MoveMod(((Mod)(dataMods.SelectedItem)).mod_id, Direction.Down);
        }

        private bool OnePathSet()
        {
            return Directory.Exists(Properties.Settings.Default.EO1_Path)
                || Directory.Exists(Properties.Settings.Default.EO2_Path)
                || Directory.Exists(Properties.Settings.Default.EO3_Path);
        }


        private void btnMoveUp_Click(object sender, RoutedEventArgs e)
        {
            if (dataMods.SelectedIndex == -1)
                return;
            MoveMod(((Mod)(dataMods.SelectedItem)).mod_id, Direction.Up);
        }

        private enum Direction
        {
            Up,
            Down
        }

        private void MoveMod(int mod_id, Direction dir)
        {
            int mod_idx = current_mods.IndexOf(current_mods.First(a => a.mod_id == mod_id));
            switch (dir)
            {
                case Direction.Up:
                    if (mod_idx > 0)
                        current_mods.Move(mod_idx, mod_idx - 1);
                    break;
                case Direction.Down:
                    if (mod_idx < current_mods.Count - 1)
                        current_mods.Move(mod_idx, mod_idx + 1);
                    break;
                default:
                    break;
            }
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            ShowSettings();
        }

        private void ShowSettings()
        {
            Settings settings = new Settings()
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            settings.ShowDialog();
        }

        private void cboGames_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selected_game = (Game)(cboGames.SelectedItem);
            current_mods = selected_game.GameMods;
            dataMods.ItemsSource = current_mods;
        }

        private void StartInstall()
        {
            InstallProgress progressWindow = new InstallProgress(selected_game)
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            progressWindow.ShowDialog();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            StartInstall();
        }

        private void btnSaveAndPlay_Click(object sender, RoutedEventArgs e)
        {
            // Install the mods
            StartInstall();

            // Execute the game
            string exe_path = System.IO.Path.Combine(selected_game.GamePath, selected_game.GameExecutable);
            if (!File.Exists(exe_path))
            {
                MessageBox.Show($"Could not find {exe_path}!");
                return;
            }
            System.Diagnostics.Process.Start(exe_path);

            // Close app
            Environment.Exit(0);
        }

        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            CreateMod createModWindow = new CreateMod()
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            createModWindow.ShowDialog();
            current_mods = selected_game.GameMods;
            dataMods.ItemsSource = current_mods;
        }
    }
}
