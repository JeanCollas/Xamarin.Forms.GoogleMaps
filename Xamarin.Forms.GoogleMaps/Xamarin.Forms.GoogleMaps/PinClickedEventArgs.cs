using System;
namespace Xamarin.Forms.GoogleMaps
{
    public sealed class PinClickedEventArgs : EventArgs
    {
        public bool Handled { get; set; } = false;
        public IPin Pin { get; }

        internal PinClickedEventArgs(IPin pin)
        {
            this.Pin = pin;
        }
    }
}
