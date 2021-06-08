using System;
using HandyControl.Controls;
using Language_Dictionary.Models;

namespace Language_Dictionary.Services
{
    public static class SettingsHelper
    {
        public static void GetSettings()
        {
            Settings.Folder = Properties.Settings.Default.Folder;
            if(Settings.Folder == "")
                Settings.Folder = Environment.CurrentDirectory + "\\Files";
            Settings.DelayMin = Properties.Settings.Default.DelayMin;
            if (Settings.DelayMin == 0) Settings.DelayMin = 10;
            Settings.CountWords = Properties.Settings.Default.CountWords;
            if (Settings.CountWords == 0) Settings.CountWords = 5;
            Settings.ToRepeatWords = Properties.Settings.Default.ToRepeatWords;
        }

        public static void SetSettings()
        {
            Properties.Settings.Default.Folder = Settings.Folder;
            Properties.Settings.Default.DelayMin = Settings.DelayMin;
            Properties.Settings.Default.CountWords = Settings.CountWords;
            Properties.Settings.Default.ToRepeatWords = Settings.ToRepeatWords;

            Properties.Settings.Default.Save();
            Properties.Settings.Default.Reload();
        }
    }
}