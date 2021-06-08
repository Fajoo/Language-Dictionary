using System.Windows.Input;
using Language_Dictionary.Infrastructure.Commands;
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

        public NewWordsViewModel()
        {

        }

        #region Start Command

        private ICommand _startCommand;

        public ICommand StartCommand => _startCommand ?? new LambdaCommand(par => IsStarted = true);

        #endregion
    }
}