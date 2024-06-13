using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace copy_flyouts
{
    public class Settings : INotifyPropertyChanged
    {
        private bool _flyoutsEnabled;
        private bool _startMinimized;

        public bool FlyoutsEnabled 
        { 
            get => _flyoutsEnabled; 
            set
            {
                _flyoutsEnabled = value;
                OnPropertyChanged(nameof(FlyoutsEnabled));
            }
        }

        public bool StartMinimized
        {
            get => _startMinimized;
            set
            {
                _startMinimized = value;
                OnPropertyChanged(nameof(StartMinimized));
            }
        }

        public Settings(bool flyoutsEnabled, bool startMinimized) 
        {
            FlyoutsEnabled = flyoutsEnabled;
            StartMinimized = startMinimized;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
