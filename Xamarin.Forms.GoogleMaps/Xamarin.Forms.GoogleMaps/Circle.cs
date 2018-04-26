using MapDance.Tools;

namespace Xamarin.Forms.GoogleMaps
{
    public class Circle : NotifyClass//BindableObject
    {
        public ICircle _ICircle;

        public float StrokeWidth { get { return _ICircle.CircleStrokeWidth; } set { } }

        public Color StrokeColor { get { return _ICircle.CircleStrokeColor; } set { } }

        public Color FillColor { get { return _ICircle.CircleFillColor; } set { } }

        public Position Center { get { return _ICircle.CircleCenter; } set { } }

        public Distance Radius { get { return _ICircle.CircleRadius; } set { } }

        //public static readonly BindableProperty StrokeWidthProperty = BindableProperty.Create("StrokeWidth", typeof(float), typeof(float), 1f);
        //public static readonly BindableProperty StrokeColorProperty = BindableProperty.Create("StrokeColor", typeof(Color), typeof(Color), Color.Blue);
        //public static readonly BindableProperty FillColorProperty = BindableProperty.Create("FillColor", typeof(Color), typeof(Color), Color.Blue);
        ////public static readonly BindableProperty IsClickableProperty = BindableProperty.Create("IsClickable", typeof(bool), typeof(bool), false);

        //public static readonly BindableProperty CenterProperty = BindableProperty.Create("Center", typeof(Position), typeof(Position), default(Position));
        //public static readonly BindableProperty RadiusProperty = BindableProperty.Create("Radius", typeof(Distance), typeof(Distance), Distance.FromMeters(1));

        //public float StrokeWidth
        //{
        //    get { return (float)GetValue(StrokeWidthProperty); }
        //    set { SetValue(StrokeWidthProperty, value); }
        //}

        //public Color StrokeColor
        //{
        //    get { return (Color)GetValue(StrokeColorProperty); }
        //    set { SetValue(StrokeColorProperty, value); }
        //}

        //public Color FillColor
        //{
        //    get { return (Color)GetValue(FillColorProperty); }
        //    set { SetValue(FillColorProperty, value); }
        //}

        //public bool IsClickable
        //{
        //    get { return (bool)GetValue(IsClickableProperty); }
        //    set { SetValue(IsClickableProperty, value); }
        //}

        //public Position Center
        //{
        //    get { return (Position)GetValue(CenterProperty); }
        //    set { SetValue(CenterProperty, value); }
        //}

        //public Distance Radius
        //{
        //    get { return (Distance)GetValue(RadiusProperty); }
        //    set { SetValue(RadiusProperty, value); }
        //}

        public object Tag { get; set; }

        public object NativeObject { get; internal set; }

        //public event EventHandler Clicked;

        public Circle(ICircle icircle)
        {
            _ICircle = icircle;
            _ICircle.PropertyChanged += ICircle_PropertyChanged;
        }

        private void ICircle_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch(e.PropertyName)
            {
                case nameof(ICircle.CircleCenter):
                    NotifyPropertyChanged(nameof(Center));
                    break;
                case nameof(ICircle.CircleFillColor):
                    NotifyPropertyChanged(nameof(FillColor));
                    break;
                case nameof(ICircle.CircleRadius):
                    NotifyPropertyChanged(nameof(Radius));
                    break;
                case nameof(ICircle.CircleStrokeColor):
                    NotifyPropertyChanged(nameof(StrokeColor));
                    break;
                case nameof(ICircle.CircleStrokeWidth):
                    NotifyPropertyChanged(nameof(StrokeWidth));
                    break;
            } 
        }

        internal bool SendTap()
        {
            //EventHandler handler = Clicked;
            //if (handler == null)
            //    return false;

            //handler(this, EventArgs.Empty);
            //return true;
            return false;
        }
    }
}

