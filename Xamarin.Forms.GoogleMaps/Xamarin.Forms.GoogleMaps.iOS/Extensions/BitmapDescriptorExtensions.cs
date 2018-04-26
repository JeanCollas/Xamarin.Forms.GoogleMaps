using System;
using UIKit;
using Xamarin.Forms.Platform.iOS;
using Foundation;
namespace Xamarin.Forms.GoogleMaps.iOS.Extensions
{
    internal static class BitmapDescriptorExtensions
    {
        public static UIImage ToUIImage(this BitmapDescriptor self)
        {
            switch (self.Type)
            {

                case BitmapDescriptorType.Default:
                    //self.Color.ToUIColor()
                    return Google.Maps.Marker.MarkerImage(UIColor.FromRGB(216, 62, 54));
                case BitmapDescriptorType.Bundle:
                    // Resize to screen scale
                    var path = NSBundle.MainBundle.PathForResource(self.BundleName, "");
                    var data = NSData.FromFile(path);
                    return UIImage.LoadFromData(data, UIScreen.MainScreen.Scale);
                case BitmapDescriptorType.Stream:
                    self.Stream.Position = 0;
                    // Resize to screen scale
                    return UIImage.LoadFromData(NSData.FromStream(self.Stream), UIScreen.MainScreen.Scale);
                case BitmapDescriptorType.AbsolutePath:
                    return UIImage.FromFile(self.AbsolutePath);
                default:
                    return Google.Maps.Marker.MarkerImage(UIColor.Red);
            }
        }
    }
}

