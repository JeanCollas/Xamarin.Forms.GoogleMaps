using System.ComponentModel;
using System.Windows.Input;

namespace Xamarin.Forms.GoogleMaps
{
    public interface ICircle : INotifyPropertyChanged
    {
        float CircleStrokeWidth { get; }

        Color CircleStrokeColor { get; }

        Color CircleFillColor { get; }

        Position CircleCenter { get; }

        Distance CircleRadius { get; }
    }
    public static class ICircleExtensions
    {
        public static Circle ToCircle(this ICircle iCircle)
        {
            var circle = new Circle(iCircle);

            return circle;
        }

    }
}
