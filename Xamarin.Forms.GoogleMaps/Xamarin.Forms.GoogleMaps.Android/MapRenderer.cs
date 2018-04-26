﻿using System;
using System.ComponentModel;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.OS;
using Java.Lang;
using Xamarin.Forms.Platform.Android;
using Math = System.Math;
using AView = Android.Views.View;
using AImageView = Android.Widget.ImageView;
using ARelativeLayout = Android.Widget.RelativeLayout;
using ALayoutDirection = Android.Views.LayoutDirection;
using AGravityFlags = Android.Views.GravityFlags;
using Android.Util;
using Android.App;
using Xamarin.Forms.GoogleMaps.Internals;
using Xamarin.Forms.GoogleMaps.Logics.Android;
using Xamarin.Forms.GoogleMaps.Logics;
using Xamarin.Forms.GoogleMaps.Android.Extensions;

namespace Xamarin.Forms.GoogleMaps.Android
{
    public class MapRenderer : ViewRenderer,
        GoogleMap.IOnCameraChangeListener,
        GoogleMap.IOnMapClickListener,
        GoogleMap.IOnMapLongClickListener
    {
        readonly BaseLogic<GoogleMap>[] _logics;

        public MapRenderer() : base()
        {
            SaveEnabled = false;
            AutoPackage = false;
            _logics = new BaseLogic<GoogleMap>[]
            {
                new PolylineLogic(),
                new PolygonLogic(),
                new CircleLogic(),
                new PinLogic(),
                new TileLayerLogic(),
                new GroundOverlayLogic()
            };
        }

        public MapRenderer(IntPtr javaReference, global::Android.Runtime.JniHandleOwnership transfer) : this() { }

        static Bundle s_bundle;
        internal static Bundle Bundle { set { s_bundle = value; } }

        protected GoogleMap NativeMap { get; private set; }

        protected Map Map => (Map)Element;

        private GoogleMap _oldNativeMap;
        private Map _oldMap;

        bool _ready = false;
        bool _onLayout = false;

        public override SizeRequest GetDesiredSize(int widthConstraint, int heightConstraint)
        {
            return new SizeRequest(new Size(Context.ToPixels(40), Context.ToPixels(40)));
        }

        protected async override void OnElementChanged(ElementChangedEventArgs<View> e)
        {
            if (ReferenceEquals(e.NewElement, e.OldElement)) return;
            var oldMapView = (MapView)Control;

            if (e.OldElement != null)
            {
                var oldMapModel = (Map)e.OldElement;
                MessagingCenter.Unsubscribe<Map, MoveToRegionMessage>(this, Map.MoveMessageName);
                MessagingCenter.Unsubscribe<Map>(this, Map.CenterOnMyLocationMessageName);

                var oldGoogleMap = await oldMapView.GetGoogleMapAsync();
                if (oldGoogleMap != null)
                {

                    oldGoogleMap.SetOnCameraChangeListener(null);
                    oldGoogleMap.SetOnMapClickListener(null);
                    oldGoogleMap.SetOnMapLongClickListener(null);
                }

                oldMapView.Dispose();
            }

            if (oldMapView != null)
            {
                _oldNativeMap = await oldMapView.GetGoogleMapAsync();
                _oldMap = (Map)e.OldElement;
            }


            base.OnElementChanged(e);

            var mapView = new MapView(Context);
            mapView.OnCreate(s_bundle);
            mapView.OnResume();
            SetNativeControl(mapView);

            var activity = Context as Activity;
            if (activity != null)
            {
                var metrics = new DisplayMetrics();
                activity.WindowManager.DefaultDisplay.GetMetrics(metrics);
                foreach (var logic in _logics)
                    logic.ScaledDensity = metrics.ScaledDensity;
            }

            MessagingCenter.Subscribe<Map, MoveToRegionMessage>(this, Map.MoveMessageName, OnMoveToRegionMessage, Map);
            MessagingCenter.Subscribe<Map>(this, Map.CenterOnMyLocationMessageName, (s) =>
            {
                var loc = NativeMap?.MyLocation;
                if (loc != null)
                {
                    Map.MyLocation = new Position(loc.Latitude, loc.Longitude);
                    NativeMap.AnimateCamera(CameraUpdateFactory.NewLatLng(new LatLng(loc.Latitude, loc.Longitude)));
                }
            });

            var mapView2 = (MapView)Control;

            //// To move the "My Location" button
            //            if (mapView2 != null &&
            //                mapView2.FindViewById(Integer.ParseInt("1")) != null)
            //            {
            //                // Get the button view
            //                AImageView locationButton = (AImageView)((AView)(mapView2.FindViewById(1).Parent)).FindViewById(2);

            //                // its parent places it on top right
            //                //var rl = locationButton.Parent as ARelativeLayout;
            //                //if (rl != null) rl.SetGravity(AGravityFlags.RelativeLayoutDirection | AGravityFlags.Top | AGravityFlags.Left);

            //                // and next place it, on bottom right (as Google Maps app)
            //                MarginLayoutParams layoutParams = //(LayoutParams)
            //                        locationButton.LayoutParameters as MarginLayoutParams;
            //                if (layoutParams != null)
            //                    layoutParams.SetMargins(0, 170, 15, 0);
            ////                    layoutParams.LeftMargin = 15;
            //            }

            NativeMap = await mapView2.GetGoogleMapAsync();
            OnMapReady(NativeMap);
        }

        void OnMapReady(GoogleMap map)
        {
            if (map != null)
            {
                map.SetOnMapClickListener(this);
                map.SetOnMapLongClickListener(this);
                map.UiSettings.MapToolbarEnabled = false;
                map.UiSettings.ZoomControlsEnabled = Map.HasZoomButtons;
                map.UiSettings.ZoomGesturesEnabled = Map.HasZoomEnabled;
                map.UiSettings.ScrollGesturesEnabled = Map.HasScrollEnabled;
                try
                {
                    map.MyLocationEnabled = Map.IsShowingUser;
                }
                catch { }
                map.UiSettings.MyLocationButtonEnabled = false;
                map.TrafficEnabled = Map.IsTrafficEnabled;

                SetMapType();
            }

            foreach (var logic in _logics)
            {
                logic.Register(_oldNativeMap, _oldMap, NativeMap, Map);
            }

            _ready = true;
            if (_ready && _onLayout)
            {
                InitializeLogic();
            }
            if (map != null)
                map.SetOnCameraChangeListener(this);
        }

        private void OnZoomButtonsMessage(Map map, bool enabled)
        {
            NativeMap.UiSettings.ZoomControlsEnabled = Map.HasZoomButtons;
        }

        void OnMoveToRegionMessage(Map s, MoveToRegionMessage m)
        {
            MoveToRegion(m.Span, m.Animate);
        }

        void MoveToRegion(MapSpan span, bool animate)
        {
            var map = NativeMap;
            if (map == null)
                return;

            Position center = span.Center;

            span = span.ClampLatitude(85, -85);
            var halfLat = span.LatitudeDegrees / 2d;
            var halfLong = span.LongitudeDegrees / 2d;

            var ne = new LatLng(center.Latitude + halfLat, center.Longitude + halfLong);
            var sw = new LatLng(center.Latitude - halfLat, center.Longitude - halfLong);
            var update = CameraUpdateFactory.NewLatLngBounds(new LatLngBounds(sw, ne), 0);

            try
            {
                if (animate)
                    map.AnimateCamera(update);
                else
                    map.MoveCamera(update);
            }
            catch (IllegalStateException exc)
            {
                System.Diagnostics.Debug.WriteLine("MoveToRegion exception: " + exc);
            }
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            base.OnLayout(changed, l, t, r, b);

            _onLayout = true;

            if (_ready && _onLayout)
            {
                InitializeLogic();
            }
            else if (changed && NativeMap != null)
            {
                UpdateMapRegion(NativeMap.CameraPosition.Target);
            }
        }

        void InitializeLogic()
        {
            MoveToRegion(((Map)Element).MapRegion, false);

            foreach (var logic in _logics)
            {
                if (logic.Map != null)
                {
                    logic.RestoreItems();
                    logic.OnMapPropertyChanged(new PropertyChangedEventArgs(Map.SelectedPinProperty.PropertyName));
                }
            }

            _ready = false;
            _onLayout = false;
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == nameof(Map.MapType))
            {
                SetMapType();
                return;
            }

            if (NativeMap == null)
                return;

            if (e.PropertyName == nameof(Map.IsShowingUser))
                try
                {
                    NativeMap.MyLocationEnabled = Map.IsShowingUser;
                }
                catch { }
            else if (e.PropertyName == nameof(Map.HasScrollEnabled))
                NativeMap.UiSettings.ScrollGesturesEnabled = Map.HasScrollEnabled;
            else if (e.PropertyName == nameof(Map.HasZoomEnabled))
                NativeMap.UiSettings.ZoomGesturesEnabled = Map.HasZoomEnabled;
            else if (e.PropertyName == nameof(Map.HasZoomEnabled))
                NativeMap.UiSettings.ZoomControlsEnabled = Map.HasZoomButtons;
            else if (e.PropertyName == nameof(Map.IsTrafficEnabled))
                NativeMap.TrafficEnabled = Map.IsTrafficEnabled;

            foreach (var logic in _logics)
                logic.OnMapPropertyChanged(e);
        }

        void SetMapType()
        {
            var map = NativeMap;
            if (map == null)
                return;

            switch (Map.MapType)
            {
                case MapType.Street:
                    map.MapType = GoogleMap.MapTypeNormal;
                    break;
                case MapType.Satellite:
                    map.MapType = GoogleMap.MapTypeSatellite;
                    break;
                case MapType.Hybrid:
                    map.MapType = GoogleMap.MapTypeHybrid;
                    break;
                case MapType.None:
                    map.MapType = GoogleMap.MapTypeNone;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(Map.MapType));
            }
        }

        public void OnCameraChange(CameraPosition pos)
        {
            UpdateMapRegion(pos.Target);
        }

        public void OnMapClick(LatLng point)
        {
            Map.SendMapClicked(point.ToPosition());
        }

        public void OnMapLongClick(LatLng point)
        {
            Map.SendMapLongClicked(point.ToPosition());
        }

        //        différencier mapcenter et screencenter

        void UpdateMapRegion(LatLng pos)
        {
            try
            {
                var map = NativeMap;
                if (map == null)
                    return;
                var projection = map.Projection;
                var width = Control.Width;
                var height = Control.Height;
                var ul = projection.FromScreenLocation(new global::Android.Graphics.Point(0, 0));
                var ur = projection.FromScreenLocation(new global::Android.Graphics.Point(width, 0));
                var ll = projection.FromScreenLocation(new global::Android.Graphics.Point(0, height));
                var lr = projection.FromScreenLocation(new global::Android.Graphics.Point(width, height));

                var center = projection.FromScreenLocation(new global::Android.Graphics.Point(width / 2, height / 2));

                var dlat = Math.Max(Math.Abs(ul.Latitude - lr.Latitude), Math.Abs(ur.Latitude - ll.Latitude));
                var dlong = Math.Max(Math.Abs(ul.Longitude - lr.Longitude), Math.Abs(ur.Longitude - ll.Longitude));


                var map2 = ((Map)Element);
                try
                {
                    //                var region = projection.VisibleRegion;

                    map2.CameraMoving = true;

                    ////var center1 = region.LatLngBounds.Center.ToPosition();
                    ////var center2 = pos.ToPosition(); gives wrong values!

                    //var bounds = region.ToBounds();
                    //map2.SetMapRegionInternal(MapSpan.FromBounds(bounds));
                    map2.SetMapRegionInternal(new MapSpan(center.ToPosition(), dlat, dlong));// MapSpan.From(bounds));
                }
                finally { map2.CameraMoving = false; }
            }
            catch { }

            /////////////////// iOS way
            //var mapModel = (Map)Element;
            //var mkMapView = (MapView)Control;

            //var region = mkMapView.Projection.VisibleRegion;
            //var minLat = Math.Min(Math.Min(Math.Min(region.NearLeft.Latitude, region.NearRight.Latitude), region.FarLeft.Latitude), region.FarRight.Latitude);
            //var minLon = Math.Min(Math.Min(Math.Min(region.NearLeft.Longitude, region.NearRight.Longitude), region.FarLeft.Longitude), region.FarRight.Longitude);
            //var maxLat = Math.Max(Math.Max(Math.Max(region.NearLeft.Latitude, region.NearRight.Latitude), region.FarLeft.Latitude), region.FarRight.Latitude);
            //var maxLon = Math.Max(Math.Max(Math.Max(region.NearLeft.Longitude, region.NearRight.Longitude), region.FarLeft.Longitude), region.FarRight.Longitude);
            //try
            //{
            //    mapModel.CameraMoving = true;
            //    mapModel.MapRegion = new MapSpan(mkMapViewChangeEventArgs.Position.Target.ToPosition(), maxLat - minLat, maxLon - minLon);
            //}
            //finally { mapModel.CameraMoving = false; }


        }

        bool _disposed;
        protected override void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                _disposed = true;
                try
                {
                    if (this.Map != null)
                    {
                        MessagingCenter.Unsubscribe<Map, MoveToRegionMessage>(this, Map.MoveMessageName);
                        MessagingCenter.Unsubscribe<Map>(this, Map.CenterOnMyLocationMessageName);
                    }

                    foreach (var logic in _logics)
                        logic.Unregister(NativeMap, Map);

                    if (NativeMap != null)
                    {
                        try
                        {
                            NativeMap.MyLocationEnabled = false;
                        }
                        catch { }
                        NativeMap.Dispose();
                    }
                }
                catch { }
            }

            base.Dispose(disposing);
        }
    }
}
