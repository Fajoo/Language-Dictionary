using System;
using System.Windows.Threading;
using Language_Dictionary.Models;
using Language_Dictionary.ViewModels.Base;

namespace Language_Dictionary.ViewModels.Other
{
    public class TimerViewModel : ViewModel
    {
        #region Value : int - Текущая минута

        /// <summary>Текущая минута</summary>
        private int _value;

        /// <summary>Текущая минута</summary>
        public int Value { get => _value; set => Set(ref _value, value); }

        #endregion

        #region MaxValue : int - Максимально

        /// <summary>Максимально</summary>
        private int _maxValue;

        /// <summary>Максимально</summary>
        public int MaxValue { get => _maxValue; set => Set(ref _maxValue, value); }

        #endregion

        #region NextInMin : int - Через мин

        /// <summary>Через мин</summary>
        private int _nextInMin = Settings.DelayMin;

        /// <summary>Через мин</summary>
        public int NextInMin { get => _nextInMin; set => Set(ref _nextInMin, value); }

        #endregion

        private readonly DispatcherTimer _timer;

        public TimerViewModel()
        {
            _timer = new DispatcherTimer();

            _timer.Tick += (sender, args) =>
            {
                Value++;
                NextInMin = Settings.DelayMin - Value;
            };

            _timer.Interval = new TimeSpan(0, 1, 0);
        }

        public void Start()
        {
            MaxValue = Settings.DelayMin;
            NextInMin = Settings.DelayMin;
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
            Value = 0;
            NextInMin = Settings.DelayMin;
        }
    }
}