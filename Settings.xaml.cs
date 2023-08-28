﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using Newtonsoft.Json;
using System.Windows.Controls; // Add this line for SelectionChangedEventArgs

namespace Concursus
{
	public partial class Settings : Window
	{
		public Settings()
		{
			InitializeComponent();
			Themes.UpdateForm(Themes.CURRENT_THEME, this);
			foreach (Themes.ThemeOption theme_option in Enum.GetValues(typeof(Themes.ThemeOption)))
			{
				if (theme_option == Themes.ThemeOption.Invalid)
					continue;
				cboThemes.Items.Add(Themes.GetStringFromOption(theme_option));
			}
			cboThemes.SelectedItem = Themes.GetStringFromOption(Themes.CURRENT_THEME);
		}

		private void cboThemes_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			Themes.CURRENT_THEME = Themes.GetOptionFromString(cboThemes.SelectedItem.ToString());
			Themes.UpdateForm(Themes.CURRENT_THEME, this);
			if (this.Owner != null)
				Themes.UpdateForm(Themes.CURRENT_THEME, this.Owner);
		}

		private void btnSave_Click(object sender, RoutedEventArgs e)
		{
			{
				Properties.Settings.Default.App_Theme = Themes.GetStringFromOption(Themes.CURRENT_THEME);
				Properties.Settings.Default.Save();
				this.Close();
			}
		}

		private void btnAddGame_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog ofd = new OpenFileDialog();
			if (ofd.ShowDialog() != true)
				return;

			string exePath = ofd.FileName;
			string dataDir = "";

			foreach (string dir in Directory.GetDirectories(Path.GetDirectoryName(exePath)))
			{
				if (dir.Contains("_Data"))
				{
					dataDir = dir;
					break;
				}
			}

			string key = Path.GetFileNameWithoutExtension(exePath); // Remove ".exe" extension
			string gameName = Path.GetFileName(Path.GetDirectoryName(exePath)); // Get the parent folder name

			List<Game> games = new List<Game>();
			games.Add(new Game()
			{
				key = key,
				GameName = gameName,
				GameFolderDataName = Path.GetFileName(dataDir),
				GamePath = Path.GetDirectoryName(exePath), // Use Path.GetDirectoryName(exePath)
				GameExecutable = exePath
			});

			string gameJson = JsonConvert.SerializeObject(games, Formatting.Indented);

			// Create the GameData folder if it doesn't exist
			string gameDataFolderPath = "GameData";
			if (!Directory.Exists(gameDataFolderPath))
			{
				Directory.CreateDirectory(gameDataFolderPath);
			}

			// Save the game's data in a separate JSON file named after the key
			string jsonFileName = Path.Combine(gameDataFolderPath, $"{key}.json");
			File.WriteAllText(jsonFileName, gameJson);

			MessageBox.Show("Game added and JSON data saved!");
		}
	}
}
