using System;
using System.ComponentModel;

namespace Xamarin.Forms.GoogleMaps
{
    public interface ICircle : INotifyPropertyChanged
    {
        Position Center { get; set; }
        Color FillColor { get; set; }
        bool IsClickable { get; set; }
        object NativeObject { get; set; }
        Distance Radius { get; set; }
        Color StrokeColor { get; set; }
        float StrokeWidth { get; set; }
        object Tag { get; set; }
        int ZIndex { get; set; }

        event EventHandler Clicked;

        bool SendTap();
    }
}