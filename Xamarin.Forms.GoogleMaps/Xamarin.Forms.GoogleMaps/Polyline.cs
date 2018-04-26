using MapDance.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Drawing;

namespace Xamarin.Forms.GoogleMaps
{
    public class Polyline : NotifyClass//BindableObject
    {
        void HandleAction(GoogleMaps.Polygon arg1, NotifyCollectionChangedEventArgs arg2)
        {

        }

        private float _StrokeWidth;
        public float StrokeWidth { get { return _StrokeWidth; } set { bool changed = _StrokeWidth != value; if (changed) { _StrokeWidth = value; NotifyIChanged(); } } }

        private Color _StrokeColor;
        public Color StrokeColor { get { return _StrokeColor; } set { bool changed = _StrokeColor != value; if (changed) { _StrokeColor = value; NotifyIChanged(); } } }

        private bool _IsClickable;
        public bool IsClickable { get { return _IsClickable; } set { bool changed = _IsClickable != value; if (changed) { _IsClickable = value; NotifyIChanged(); } } }

        //public static readonly BindableProperty StrokeWidthProperty = BindableProperty.Create("StrokeWidth", typeof(float), typeof(float), 1f);
        //public static readonly BindableProperty StrokeColorProperty = BindableProperty.Create("StrokeColor", typeof(Color), typeof(Color), Color.Blue);
        //public static readonly BindableProperty IsClickableProperty = BindableProperty.Create("IsClickable", typeof(bool), typeof(bool), false);

        private readonly ObservableCollection<Position> _positions = new ObservableCollection<Position>();

        private Action<Polyline, NotifyCollectionChangedEventArgs> _positionsChangedHandler = null;

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

        //public bool IsClickable
        //{
        //    get { return (bool)GetValue(IsClickableProperty); }
        //    set { SetValue(IsClickableProperty, value); }
        //}

        public IList<Position> Positions
        {
            get { return _positions; }
        }

        public object Tag { get; set; }

        public object NativeObject { get; internal set; }

        public event EventHandler Clicked;

        public Polyline()
        {
        }

        internal bool SendTap()
        {
            EventHandler handler = Clicked;
            if (handler == null)
                return false;

            handler(this, EventArgs.Empty);
            return true;
        }

        internal void SetOnPositionsChanged(Action<Polyline, NotifyCollectionChangedEventArgs> handler)
        {
            _positionsChangedHandler = handler;
            if (handler != null)
                _positions.CollectionChanged += OnCollectionChanged;
            else
                _positions.CollectionChanged -= OnCollectionChanged;
        }

        void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _positionsChangedHandler?.Invoke(this, e);
        }
    }
}

