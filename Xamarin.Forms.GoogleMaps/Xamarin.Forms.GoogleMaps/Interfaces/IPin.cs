using System;
using System.ComponentModel;

namespace Xamarin.Forms.GoogleMaps
{
    public interface IPin : INotifyPropertyChanged
    {
        string Address { get; set; }
        Point Anchor { get; set; }
        bool Flat { get; set; }
        BitmapDescriptor Icon { get; set; }
        Point InfoWindowAnchor { get; set; }
        bool IsDraggable { get; set; }
        bool IsVisible { get; set; }
        string Label { get; set; }
        /// This should not be set outside the map library
        object NativeObject { get; set; }
        Position Position { get; set; }
        float Rotation { get; set; }
        object Tag { get; set; }
        float Transparency { get; set; }
        PinType Type { get; set; }
        int ZIndex { get; set; }
        bool SendTap();

        event EventHandler Clicked;

        bool Equals(object obj);
        int GetHashCode();
    }
}