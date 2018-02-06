using System;
namespace Xamarin.Forms.GoogleMaps
{
    public sealed class PinDragEventArgs : EventArgs
    {
        public IPin Pin
        {
            get;
            private set;
        }

        internal PinDragEventArgs(IPin pin)
        {
            this.Pin = pin;
        }
    }
}

