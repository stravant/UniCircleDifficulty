﻿using System;
using System.Windows;
using Microsoft.Win32;

using UniCircleTools;
using UniCircleTools.Beatmaps;
using UniCircle.Difficulty.Standard;

namespace UniCircleVisualiser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Calculator Calculator { get; set; } = new Calculator();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            dataGridAimPoint.ItemsSource = Calculator.Aiming.CalculatedPoints;
            dataGridClickPoint.ItemsSource = Calculator.Clicking.CalculatedPoints;
            dataGridVisualPoint.ItemsSource = Calculator.Reading.CalculatedPoints;
        }

        private Mods GetMods()
        {
            Mods mods = Mods.None;

            // Settings mods
            if (checkboxEZ.IsChecked ?? false)
            {
                mods |= Mods.Easy;
            }
            else if (checkboxHR.IsChecked ?? false)
            {
                mods |= Mods.HardRock;
            }

            // Time mods
            if (checkboxHT.IsChecked ?? false)
            {
                mods |= Mods.HalfTime;
            }
            else if (checkboxDT.IsChecked ?? false)
            {
                mods |= Mods.DoubleTime;
            }

            // Visual mods
            if (checkboxHD.IsChecked ?? false)
            {
                mods |= Mods.Hidden;
            }
            if (checkboxFL.IsChecked ?? false)
            {
                mods |= Mods.Flashlight;
            }

            return mods;
        }

        private void Recalculate()
        {
            if (Calculator.Beatmap == null)
            {
                return;
            }

            Calculator.SetMods(GetMods());

            // TODO: expose skill constants so they can be modified at runtime for testing

            Calculator.CalculateDifficulty();

            DisplayData();
        }

        private void DisplayData()
        {
            // Populate Difficulty labels
            labelAimingDifficulty.Content = String.Format("Aiming: {0:0.##} stars", Calculator.Aiming.Value);
            labelClickingDifficulty.Content = String.Format("Clicking: {0:0.##} stars", Calculator.Clicking.Value);
            labelReadingDifficulty.Content = String.Format("Reading: {0:0.##} stars", Calculator.Reading.Value);

            // Refresh datagrids
            dataGridAimPoint.Items.Refresh();
            dataGridClickPoint.Items.Refresh();
            dataGridVisualPoint.Items.Refresh();
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "osu! Beatmap files (*.osu)|*.osu"
            };

            if (openFileDialog.ShowDialog(this) == true)
            {
                Calculator.SetBeatmap(new Beatmap(openFileDialog.FileName));
                textBlockOpenBeatmap.Text = String.Format("{0} - {1} [{2}]", Calculator.Beatmap.Artist, Calculator.Beatmap.Title, Calculator.Beatmap.Version);
                Recalculate();
            }
        }

        private void CheckboxMod_Changed(object sender, RoutedEventArgs e)
        {
            Recalculate();
        }

        private void CheckboxEZ_Checked(object sender, RoutedEventArgs e)
        {
            checkboxHR.IsChecked = false;
            CheckboxMod_Changed(sender, e);
        }

        private void CheckboxHR_Checked(object sender, RoutedEventArgs e)
        {
            checkboxEZ.IsChecked = false;
            CheckboxMod_Changed(sender, e);
        }

        private void CheckboxHT_Checked(object sender, RoutedEventArgs e)
        {
            checkboxDT.IsChecked = false;
            CheckboxMod_Changed(sender, e);
        }

        private void CheckboxDT_Checked(object sender, RoutedEventArgs e)
        {
            checkboxHT.IsChecked = false;
            CheckboxMod_Changed(sender, e);
        }

        private void SkillSetting_SourceUpdated(object sender, System.Windows.Data.DataTransferEventArgs e)
        {
            Recalculate();
        }
    }
}
