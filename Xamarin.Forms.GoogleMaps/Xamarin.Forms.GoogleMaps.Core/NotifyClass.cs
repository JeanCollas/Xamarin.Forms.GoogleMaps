using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Xamarin.Forms.GoogleMaps
{
    public class NotifyClass : INotifyPropertyChanged
    {
        protected void NotifyPropertyChanging(string prop) { PropertyChanging?.Invoke(this, new PropertyChangedEventArgs(prop)); }
        protected void NotifyPropertyChanged(string prop) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop)); }
        protected void NotifyIAmChanging([CallerMemberName] string propertyName = null) => NotifyPropertyChanging(propertyName);
        protected void NotifyIChanged([CallerMemberName] string propertyName = null) => NotifyPropertyChanged(propertyName);
        public event PropertyChangedEventHandler PropertyChanging;
        public event PropertyChangedEventHandler PropertyChanged;
    }
}