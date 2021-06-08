using System.Windows;
using System.Windows.Input;
using Language_Dictionary.Infrastructure.Commands;
using Language_Dictionary.Models;
using Language_Dictionary.ViewModels.Base;
using System.Windows.Forms;

namespace Language_Dictionary.ViewModels
{
    public class SettingsViewModel : ViewModel
    {
        #region Folder : string - Путь до папки

        /// <summary>Путь до папки</summary>
        private string _folder;

        /// <summary>Путь до папки</summary>
        public string Folder { get => _folder; set => Set(ref _folder, value); }

        #endregion

        public SettingsViewModel()
        {
            Folder = Settings.Folder;
        }

        #region Новая папка

        private ICommand _newFolderCommand;

        public ICommand NewFolderCommand => _newFolderCommand ?? new LambdaCommand(par =>
        {
            var folder = new FolderBrowserDialog();
            if (folder.ShowDialog() == DialogResult.OK) 
                Folder = folder.SelectedPath;
            Settings.Folder = Folder;
        });

        #endregion
    }
}