﻿using System;
using Language_Dictionary.Models;

namespace Language_Dictionary.Services
{
    public static class SettingsHelper
    {
        public static void GetSettings()
        {
            Settings.Folder = Properties.Settings.Default.Folder ?? Environment.CurrentDirectory + "\\Files";
            Settings.DelayMin = Properties.Settings.Default.DelayMin;
            if (Settings.DelayMin == 0) Settings.DelayMin = 10;
            Settings.CountWords = Properties.Settings.Default.CountWords;
            if (Settings.CountWords == 0) Settings.CountWords = 5;
        }

        public static void SetSettings()
        {
            Properties.Settings.Default.Folder = Settings.Folder;
            Properties.Settings.Default.DelayMin = Settings.DelayMin;
            Properties.Settings.Default.CountWords = Settings.CountWords;
        }
    }
}