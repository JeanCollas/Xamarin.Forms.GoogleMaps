using System;
namespace Xamarin.Forms.GoogleMaps
{
    public sealed class SelectedPinChangedEventArgs : EventArgs
    {
        public IPin SelectedPin
        {
            get;
            private set;
        }

        internal SelectedPinChangedEventArgs(IPin selectedPin)
        {
            this.SelectedPin = selectedPin;
        }
    }
}

