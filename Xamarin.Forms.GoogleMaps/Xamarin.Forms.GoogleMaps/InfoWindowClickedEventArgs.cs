using System;

namespace Xamarin.Forms.GoogleMaps
{
    public sealed class InfoWindowClickedEventArgs : EventArgs
    {
        public IPin Pin { get; }

        internal InfoWindowClickedEventArgs(IPin pin)
        {
            this.Pin = pin;
        }
    }
}