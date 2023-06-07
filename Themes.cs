﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace EO_Mod_Manager
{
    public class Themes
    {
        public enum ThemeOption
        {
            White,
            PitchBlack,
            DarkMode,
            Naoto,
            Invalid,
        }

        public struct ThemeData
        {
            public string mainBgColor;
            public string mainTextColor;
            public string mainTableBgColor;
            public string mainTableTextColor;
            public string buttonBgColor;
            public string buttonTextColor;
            public string textBoxBgColor;
            public string textBoxTextColor;
        }

        public static ThemeOption CURRENT_THEME = GetOptionFromString(Properties.Settings.Default.App_Theme);

        public static Dictionary<ThemeOption, ThemeData> THEMES_TABLE = new Dictionary<ThemeOption, ThemeData>() {
            {
                ThemeOption.White, new ThemeData {
                    mainBgColor = "#fff",
                    mainTextColor = "#000",
                    mainTableBgColor = "#F0F0F0",
                    mainTableTextColor = "#000",
                    buttonBgColor = "#fff",
                    buttonTextColor = "#000",
                    textBoxBgColor = "#fff",
                    textBoxTextColor = "#000"
                }
            },
            {
                ThemeOption.PitchBlack, new ThemeData
                {
                    mainBgColor = "#000",
                    mainTextColor = "#fff",
                    mainTableBgColor = "#1A1A1A",
                    mainTableTextColor = "#000",
                    buttonBgColor = "#000",
                    buttonTextColor = "#fff",
                    textBoxBgColor = "#000",
                    textBoxTextColor = "#fff"
                }
            },
			{
				ThemeOption.DarkMode, new ThemeData
				{
					mainBgColor = "#1A1A1A",
					mainTextColor = "#fff",
					mainTableBgColor = "#222",
					mainTableTextColor = "#fff",
					buttonBgColor = "#333",
					buttonTextColor = "#fff",
					textBoxBgColor = "#222",
					textBoxTextColor = "#fff"
				}
			},
			{
                ThemeOption.Naoto, new ThemeData
                {
                    mainBgColor = "#0E112A",
                    mainTextColor = "#FFFFFF",
                    mainTableBgColor = "#0E112A",
                    mainTableTextColor = "#FFFFFF",
                    buttonBgColor = "#0E112A",
                    buttonTextColor = "#FFFFFF",
                    textBoxBgColor = "#0E112A",
                    textBoxTextColor = "#FFFFFF"
				}
            }
        };
public static ThemeOption GetOptionFromString(string theme)
        {
            theme = string.Concat(theme.Split(' '));
            ThemeOption res;
            if (Enum.TryParse<ThemeOption>(theme, out res))
                return res;
            else
                return ThemeOption.Invalid;
        }

        public static string GetStringFromOption(ThemeOption theme)
        {
            return string.Concat(Enum.GetName(theme.GetType(), theme).Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');
        }

        public static void UpdateForm(ThemeOption theme, Window window)
        {
            if (!THEMES_TABLE.ContainsKey(theme))
                return;
            ThemeData selected_theme = THEMES_TABLE[theme];
            BrushConverter converter = new BrushConverter();
            if (selected_theme.mainBgColor != String.Empty)
                window.Background = (Brush)(converter.ConvertFrom(selected_theme.mainBgColor));
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount((DependencyObject)window.Content); i++)
            {
                Visual child = (Visual)VisualTreeHelper.GetChild((DependencyObject)window.Content, i);
                UpdateColors(child, selected_theme, converter);
            }
        }

        private static void UpdateColors(Visual vis, ThemeData selected_theme, BrushConverter converter)
        {

            switch (vis.GetType().Name)
            {
                case "Label":
                    if(selected_theme.mainTextColor != String.Empty)
                        ((Label)vis).Foreground = (Brush)(converter.ConvertFrom(selected_theme.mainTextColor));
                    break;
                case "Button":
                    if (selected_theme.buttonTextColor != String.Empty)
                        ((Button)vis).Foreground = (Brush)(converter.ConvertFrom(selected_theme.buttonTextColor));
                    if (selected_theme.buttonBgColor != String.Empty)
                        ((Button)vis).Background = (Brush)(converter.ConvertFrom(selected_theme.buttonBgColor));
                    break;
                case "DataGrid":
                    if (selected_theme.mainTableBgColor != String.Empty)
                        ((DataGrid)vis).Background = (Brush)(converter.ConvertFrom(selected_theme.mainTableBgColor));
                    if (selected_theme.mainTableTextColor != String.Empty)
                        ((DataGrid)vis).Foreground = (Brush)(converter.ConvertFrom(selected_theme.mainTableTextColor));
                    break;
                case "CheckBox":
                    if(selected_theme.mainTextColor != String.Empty)
                        ((CheckBox)vis).Foreground = (Brush)(converter.ConvertFrom(selected_theme.mainTextColor));
                    break;
                case "TextBox":
                    if(selected_theme.textBoxBgColor != String.Empty)
                        ((TextBox)vis).Background = (Brush)(converter.ConvertFrom(selected_theme.textBoxBgColor));
                    if (selected_theme.textBoxTextColor != String.Empty)
                        ((TextBox)vis).Foreground = (Brush)(converter.ConvertFrom(selected_theme.textBoxTextColor));
                    break;
                default:
                    break;
            }

            for(int i = 0; i < VisualTreeHelper.GetChildrenCount(vis); i++)
            {
                Visual child = (Visual)VisualTreeHelper.GetChild(vis, i);
                UpdateColors(child, selected_theme, converter);
            }
        }
    }
}