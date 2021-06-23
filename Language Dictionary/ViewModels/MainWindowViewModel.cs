using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Speech.Synthesis;
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
using Language_Dictionary.ViewModels.Other;
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

        #region FileInformation : FileInformation - Информация о загруженном файле

        /// <summary>Информация о загруженном файле</summary>
        private FileInformation _fileInformation;

        /// <summary>Информация о загруженном файле</summary>
        public FileInformation FileInformation { get => _fileInformation; set => Set(ref _fileInformation, value); }

        #endregion

        private readonly BackgroundWorker _worker;
        private FilesHelper _filesHelper;
        private List<string> _worsds;
        private static readonly Random _r = new Random();

        public ObservableCollection<string> RepeatedWords { get; set; } = new ObservableCollection<string>();

        public ObservableCollection<FileInfo> Files { get; set; } = new ObservableCollection<FileInfo>();

        public TimerViewModel TimerViewModel { get; set; } = new TimerViewModel();

        public MainWindowViewModel()
        {
            _filesHelper = new FilesHelper();

            _worker = new BackgroundWorker() { WorkerSupportsCancellation = true };
            _worker.DoWork += WorkerOnDoWork;
        }

        private void WorkerOnDoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                if (!Settings.ToRepeatWords)
                {
                    _worsds = _worsds.Except(RepeatedWords).ToList();

                    if (_worsds.Count == 0) 
                    {
                        Application.Current.Dispatcher.Invoke(() => new CompletionWindow().ShowDialog());
                        StopWorkerCommand.Execute(null);
                    }
                }

                TimerViewModel.Start();

                for (var i = Settings.DelayMin * 60; i >= 0; i--)
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
                    var list = RandomList();

                    var context = new NewWordsViewModel(list);
                    context.CloseSuccess += () =>
                    {
                        list.ForEach(item =>
                        {
                            if(!(RepeatedWords.Any(i => i.Equals(item))))
                                RepeatedWords.Add(item);
                        });
                    };
                    var window = new NewWordsWindow {DataContext = context };
                    window.ShowDialog();
                });

                TimerViewModel.Stop();
            }
        }

        private List<string> RandomList()
        {
            var list = new List<string>();

            var count = Settings.CountWords;

            if (_worsds.Count < count)
                count = _worsds.Count;

            for (var i = 0; i < count; i++)
            {
                var item = _worsds[_r.Next(0, _worsds.Count)];
                if (!(list.Any(s => s.Equals(item))))
                    list.Add(item);
                else i--;
            }

            return list;
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
            if(_filesHelper.Path != Settings.Folder)
                _filesHelper = new FilesHelper();

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
                _worsds = await _filesHelper.GetAllLines(SelectedFile.FullName);
                if (_worsds.Count < Settings.CountWords)
                {
                    Growl.Error("Not enough words in file!");
                    return;
                }

                FileInformation = new FileInformation {Name = SelectedFile.Name, RowsCount = _worsds.Count};
                State = State.Loaded;
            }
            catch
            {
                Growl.Error("File upload error! Try to restart folder.");
            }

        }, (par) => SelectedFile != null && State != State.Started);

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

        public ICommand StopWorkerCommand => _stopWorkerCommand ?? new LambdaCommandAsync(async par =>
        {
            State = State.Loaded;
            RepeatedWords.Clear();
            TimerViewModel.Stop();

            if (!Settings.ToRepeatWords)
            {
                _worsds.Clear();
                _worsds = await _filesHelper.GetAllLines(SelectedFile.FullName);
            }

            _worker.CancelAsync();
        }, par => State == State.Started);

        #endregion

        #region Открыть настройки

        private ICommand _opentSettingsCommand;

        public ICommand OpentSettingsCommand => _opentSettingsCommand ?? new LambdaCommand(par => Dialog.Show(new SettingsControl()), par => State != State.Started);

        #endregion
    }
}