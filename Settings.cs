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
        public bool FlyoutsEnabled 
        { 
            get => _flyoutsEnabled; 
            set
            {
                _flyoutsEnabled = value;
                OnPropertyChanged(nameof(FlyoutsEnabled));
            }
        }

        public Settings(bool flyoutsEnabled) 
        {
            FlyoutsEnabled = flyoutsEnabled;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
