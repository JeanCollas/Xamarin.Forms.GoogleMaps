using System;
using System.Drawing;
using System.IO;

namespace Xamarin.Forms.GoogleMaps
{
    public sealed class BitmapDescriptor
    {
        public BitmapDescriptorType Type { get; private set; }
        //public Color Color { get; private set; }
        public string BundleName { get; private set; }
        public Stream Stream { get; private set; }
        public string AbsolutePath { get; private set; }
        public object View { get; private set; }

        private BitmapDescriptor()
        {
        }

        public static BitmapDescriptor DefaultMarker(/*Color color*/)
        {
            return new BitmapDescriptor()
            {
                Type = BitmapDescriptorType.Default,
                //Color = color
            };
        }

        public static BitmapDescriptor FromBundle(string bundleName)
        {
            return new BitmapDescriptor()
            {
                Type = BitmapDescriptorType.Bundle,
                BundleName = bundleName
            };
        }

        public static BitmapDescriptor FromStream(Stream stream)
        {
            return new BitmapDescriptor()
            {
                Type = BitmapDescriptorType.Stream,
                Stream = stream
            };
        }

        public static BitmapDescriptor FromPath(string absolutePath)
        {
            return new BitmapDescriptor()
            {
                Type = BitmapDescriptorType.AbsolutePath,
                AbsolutePath = absolutePath
            };
        }

        //public static BitmapDescriptor FromView(View view)
        //{
        //    return new BitmapDescriptor()
        //    {
        //        Type = BitmapDescriptorType.View,
        //        View = view
        //    };
        //}
    }
}

