using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Xamarin.Forms.GoogleMaps
{ 
    public class NotifyClass : INotifyPropertyChanged
    {
        protected virtual void NotifyPropertyChanging(string prop) { PropertyChanging?.Invoke(this, new PropertyChangedEventArgs(prop)); }
        protected virtual void NotifyPropertyChanged(string prop) { OnPropertyChanged(prop); }
        protected void NotifyIAmChanging([CallerMemberName] string propertyName = null) => NotifyPropertyChanging(propertyName);
        protected void NotifyIChanged([CallerMemberName] string propertyName = null) => NotifyPropertyChanged(propertyName);
        public event PropertyChangedEventHandler PropertyChanging;
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}