using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using HandyControl.Controls;
using Language_Dictionary.Infrastructure.Commands;
using Language_Dictionary.Models;
using Language_Dictionary.Properties;
using Language_Dictionary.Services;
using Language_Dictionary.ViewModels.Base;
using Language_Dictionary.Views;
using Settings = Language_Dictionary.Models.Settings;

namespace Language_Dictionary.ViewModels
{
    public class MainWindowViewModel : ViewModel
    {
        #region SelectedFile : FileInfo - Выбранный файл

        /// <summary>Выбранный файл</summary>
        private FileInfo _selectedFile;

        /// <summary>Выбранный файл</summary>
        public FileInfo SelectedFile { get => _selectedFile; set => Set(ref _selectedFile, value); }

        #endregion

        #region State : State - Состояние

        /// <summary>Состояние</summary>
        private State _state = State.None;

        /// <summary>Состояние</summary>
        public State State { get => _state; set => Set(ref _state, value); }

        #endregion

        private readonly BackgroundWorker _worker;
        private readonly FilesHelper _filesHelper;

        public ObservableCollection<FileInfo> Files { get; set; } = new ObservableCollection<FileInfo>();

        public MainWindowViewModel()
        {
           
            Settings.Folder =  Environment.CurrentDirectory + "\\Files";
            Settings.DelayMin = "1";

            _filesHelper = new FilesHelper();

            _worker = new BackgroundWorker() { WorkerSupportsCancellation = true };
            _worker.DoWork += WorkerOnDoWork;
        }

        private void WorkerOnDoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                for (var i = int.Parse(Settings.DelayMin) * 5; i >= 0; i--)
                {
                    if (_worker.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }
                    Thread.Sleep(1000);
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    var window = new NewWordsWindow {DataContext = new NewWordsViewModel()};
                    window.ShowDialog();
                });
            }
        }

        #region Min/Max Window

        private ICommand _minMaxCommand;

        public ICommand MinMaxCommand => _minMaxCommand ?? new LambdaCommand(par =>
        {
            App.ActivedWindow.WindowState = App.ActivedWindow.WindowState switch
            {
                WindowState.Normal => WindowState.Maximized,
                WindowState.Maximized => WindowState.Normal,
                _ => App.ActivedWindow.WindowState
            };
        }, par => App.ActivedWindow != null);

        #endregion

        #region LoadFilesCommand

        private ICommand _loadFilesCommand;

        public ICommand LoadFilesCommand => _loadFilesCommand ?? new LambdaCommandAsync(async par =>
        {
            await Task.Run(() =>
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    Files.Clear();

                    foreach (var item in _filesHelper.GetAllFiles())
                        Files.Add(item);
                }));
            });
        }, par => State == State.None || State == State.Loaded);

        #endregion

        #region Открыть каталог

        private ICommand _opentFolderCommand;

        public ICommand OpentFolderCommand => _opentFolderCommand ?? new LambdaCommand(par =>
        {
            var startInfo = new ProcessStartInfo
            {
                Arguments = Settings.Folder,
                FileName = "explorer.exe"
            };

            Process.Start(startInfo);
        });

        #endregion

        #region Загрузить файл

        private ICommand _loadFileCommand;

        public ICommand LoadFileCommand => _loadFileCommand ?? new LambdaCommandAsync(async par =>
        {
            try
            {
                var res = await _filesHelper.GetAllLines(SelectedFile.FullName);
                if (res.Count == 0)
                {
                    Growl.Error("File is empty!");
                    return;
                }
                State = State.Loaded;
            }
            catch
            {
                Growl.Error("File upload error!");
            }

        }, (par) => SelectedFile != null);

        #endregion

        #region Запустить BackgroundWorker

        private ICommand _startWorkerCommand;

        public ICommand StartWorkerCommand => _startWorkerCommand ?? new LambdaCommand(par =>
        {
            State = State.Started;
            _worker.RunWorkerAsync();
        }, par => State == State.Loaded && !_worker.IsBusy);

        #endregion

        #region Остановить BackgroundWorker

        private ICommand _stopWorkerCommand;

        public ICommand StopWorkerCommand => _stopWorkerCommand ?? new LambdaCommand(par =>
        {
            State = State.Loaded;
            _worker.CancelAsync();
        }, par => State == State.Started);

        #endregion
    }
}