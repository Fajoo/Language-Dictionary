using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Speech.Synthesis;
using System.Windows.Input;
using Language_Dictionary.Infrastructure.Commands;
using Language_Dictionary.Models;
using Language_Dictionary.ViewModels.Base;

namespace Language_Dictionary.ViewModels
{
    public class NewWordsViewModel : ViewModel
    {
        #region IsStarted : bool - Отображение слов

        /// <summary>Выбранный файл</summary>
        private bool _isStarted;

        /// <summary>Выбранный файл</summary>
        public bool IsStarted { get => _isStarted; set => Set(ref _isStarted, value); }

        #endregion

        public event Action CloseSuccess;

        private readonly SpeechSynthesizer _speech;

        public ObservableCollection<CheckWord> CheckWords { get; set; }

        public NewWordsViewModel(IEnumerable<string> words)
        {
            _speech = new SpeechSynthesizer();
            _speech.SelectVoice("Microsoft Zira Desktop");

            CheckWords = new ObservableCollection<CheckWord>(words.Select(i => new CheckWord
            {
                Word =  i,
                IsChecked = false
            }));
        }

        #region Start Command

        private ICommand _startCommand;

        public ICommand StartCommand => _startCommand ?? new LambdaCommand(par => IsStarted = true);

        #endregion

        #region Start Command

        private ICommand _audioCommand;

        public ICommand AudioCommand => _audioCommand ?? new LambdaCommand(par =>
        {
            var text = par as string;
            _speech.SpeakAsyncCancelAll();
            _speech.SpeakAsync(text ?? string.Empty);
        });

        #endregion

        #region Confirm button

        private ICommand _confirmButtonCommand;

        public ICommand ConfirmButtonCommand => _confirmButtonCommand ?? new LambdaCommand(par =>
        {
            CloseSuccess?.Invoke();
            App.ActivedWindow.Close();
        }, par => CheckWords.All(i => i.IsChecked));

        #endregion
    }
}