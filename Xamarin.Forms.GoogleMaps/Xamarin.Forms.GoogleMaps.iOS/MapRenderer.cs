using System;
using System.ComponentModel;
using Xamarin.Forms.Platform.iOS;
using Google.Maps;
using CoreLocation;
using System.Drawing;
using Xamarin.Forms.GoogleMaps.Internals;
using Xamarin.Forms.GoogleMaps.Logics.iOS;
using Xamarin.Forms.GoogleMaps.Logics;
using Xamarin.Forms.GoogleMaps.iOS.Extensions;

namespace Xamarin.Forms.GoogleMaps.iOS
{
    public class MapRenderer : ViewRenderer
    {
        bool _shouldUpdateRegion = true;

        protected MapView NativeMap => (MapView)Control;
        protected Map Map => (Map)Element;

        readonly UiSettingsLogic _uiSettingsLogic = new UiSettingsLogic();
        readonly CameraLogic _cameraLogic;

        readonly BaseLogic<MapView>[] _logics;

        public MapRenderer()
        {
            _logics = new BaseLogic<MapView>[]
            {
                new PolylineLogic(),
                new PolygonLogic(),
                new CircleLogic(),
                new PinLogic(OnMarkerCreating, OnMarkerCreated, OnMarkerDeleting, OnMarkerDeleted),
                new TileLayerLogic(),
                new GroundOverlayLogic()
            };

            _cameraLogic = new CameraLogic(() =>
            {
                OnCameraPositionChanged(NativeMap.Camera);
            });
        }

        public override SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
        {
            return Control.GetSizeRequest(widthConstraint, heightConstraint);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {

                if (Element != null)
                {
                    var mapModel = (Map)Element;
                    MessagingCenter.Unsubscribe<Map, MoveToRegionMessage>(this, Map.MoveMessageName);
                    MessagingCenter.Unsubscribe<Map>(this, Map.CenterOnMyLocationMessageName);
                }
                Map.OnSnapshot -= OnSnapshot;

                foreach (var logic in _logics)
                    logic.Unregister(NativeMap, Map);

                _cameraLogic.Unregister();

                var mkMapView = (MapView)Control;
                if (mkMapView != null)
                {
                    mkMapView.CoordinateLongPressed -= CoordinateLongPressed;
                    mkMapView.CoordinateTapped -= CoordinateTapped;
                    mkMapView.CameraPositionChanged -= CameraPositionChanged;
                }
            }

            base.Dispose(disposing);
        }

        protected override void OnElementChanged(ElementChangedEventArgs<View> e)
        {
            base.OnElementChanged(e);

            // For XAML Previewer or FormsGoogleMaps.Init not called.
            if (!FormsGoogleMaps.IsInitialized)
            {
                var label = new UILabel()
                {
                    Text = "Xamarin.Forms.GoogleMaps",
                    BackgroundColor = Color.Teal.ToUIColor(),
                    TextColor = Color.Black.ToUIColor(),
                    TextAlignment = UITextAlignment.Center
                };
                SetNativeControl(label);
                return;
            }


            var oldMapView = (MapView)Control;

            if (e.OldElement != null)
            {
                var mapModel = (Map)e.OldElement;
                MessagingCenter.Unsubscribe<Map, MoveToRegionMessage>(this, "MapMoveToRegion");

                oldMapModel.OnSnapshot -= OnSnapshot;
                _cameraLogic.Unregister();

                if (oldMapView != null)
                {
                    oldMapView.CoordinateLongPressed -= CoordinateLongPressed;
                    oldMapView.CoordinateTapped -= CoordinateTapped;
                    oldMapView.CameraPositionChanged -= CameraPositionChanged;
                    oldMapView.DidTapMyLocationButton = null;
                }
            }

            if (e.NewElement != null)
            {
                var mapModel = (Map)e.NewElement;

                if (Control == null)
                {
                    SetNativeControl(new MapView(RectangleF.Empty));
                    var mkMapView = (MapView)Control;
                    mkMapView.CameraPositionChanged += CameraPositionChanged;
                    mkMapView.CoordinateTapped += CoordinateTapped;
                    mkMapView.CoordinateLongPressed += CoordinateLongPressed;
                    mkMapView.DidTapMyLocationButton = DidTapMyLocation;
                }

                MessagingCenter.Subscribe<Map>(this, Map.CenterOnMyLocationMessageName, (s) => NativeMap.Animate(NativeMap.MyLocation.Coordinate), mapModel);
                MessagingCenter.Subscribe<Map, MoveToRegionMessage>(this, Map.MoveMessageName, (s, a) => MoveToRegion(a.Span, a.Animate), mapModel);
                if (mapModel.LastMoveToRegion != null)
                    MoveToRegion(mapModel.LastMoveToRegion, false);


                _cameraLogic.Register(Map, NativeMap);
                Map.OnSnapshot += OnSnapshot;

                _cameraLogic.MoveCamera(mapModel.InitialCameraUpdate);

                _uiSettingsLogic.Register(Map, NativeMap);
                UpdateMapType();
                UpdateIsShowingUser(_uiSettingsLogic.MyLocationButtonEnabled);
                UpdateHasScrollEnabled(_uiSettingsLogic.ScrollGesturesEnabled);
                UpdateHasZoomEnabled(_uiSettingsLogic.ZoomGesturesEnabled);
                UpdateHasRotationEnabled(_uiSettingsLogic.RotateGesturesEnabled);
                UpdateIsTrafficEnabled();
                UpdatePadding();
                UpdateMapStyle();
                UpdateMyLocationEnabled();
                _uiSettingsLogic.Initialize();

                foreach (var logic in _logics)
                {
                    logic.Register(oldMapView, (Map)e.OldElement, NativeMap, Map);
                    logic.RestoreItems();
                    logic.OnMapPropertyChanged(new PropertyChangedEventArgs(Map.SelectedPinProperty.PropertyName));
                }

            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            // For XAML Previewer or FormsGoogleMaps.Init not called.
            if (!FormsGoogleMaps.IsInitialized)
            {
                return;
            }

            var mapModel = (Map)Element;

            if (e.PropertyName == Map.MapTypeProperty.PropertyName)
            {
                UpdateMapType();
            }
            else if (e.PropertyName == Map.IsShowingUserProperty.PropertyName)
            {
                UpdateIsShowingUser();
            }
            else if (e.PropertyName == Map.MyLocationEnabledProperty.PropertyName)
            {
                UpdateMyLocationEnabled();
            }
            else if (e.PropertyName == Map.HasScrollEnabledProperty.PropertyName)
            {
                UpdateHasScrollEnabled();
            }
            else if (e.PropertyName == Map.HasRotationEnabledProperty.PropertyName)
            {
                UpdateHasRotationEnabled();
            }
            else if (e.PropertyName == Map.HasZoomEnabledProperty.PropertyName)
            {
                UpdateHasZoomEnabled();
            }
            else if (e.PropertyName == Map.IsTrafficEnabledProperty.PropertyName)
            {
                UpdateIsTrafficEnabled();
            }
            else if (e.PropertyName == VisualElement.IsVisibleProperty.PropertyName &&
                     ((Map)Element).LastMoveToRegion != null)
            {
                _shouldUpdateRegion = true;
            }
            else if (e.PropertyName == VisualElement.HeightProperty.PropertyName &&
                     ((Map)Element).InitialCameraUpdate != null)
            {
                _shouldUpdateRegion = true;
            }
            else if (e.PropertyName == Map.IndoorEnabledProperty.PropertyName)
            {
                UpdateHasIndoorEnabled();
            }
            else if (e.PropertyName == Map.PaddingProperty.PropertyName)
            {
                UpdatePadding();
            }
            else if (e.PropertyName == Map.MapStyleProperty.PropertyName)
            {
                UpdateMapStyle();
            }
            foreach (var logic in _logics)
            {
                logic.OnMapPropertyChanged(e);
            }
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            // For XAML Previewer or FormsGoogleMaps.Init not called.
            if (!FormsGoogleMaps.IsInitialized)
            {
                return;
            }

            if (_shouldUpdateRegion)
            {
                MoveToRegion(((Map)Element).LastMoveToRegion, false);
                _shouldUpdateRegion = false;
            }

        }

        void OnSnapshot(TakeSnapshotMessage snapshotMessage)
        {
            UIGraphics.BeginImageContextWithOptions(NativeMap.Frame.Size, false, 0f);
            NativeMap.Layer.RenderInContext(UIGraphics.GetCurrentContext());
            var snapshot = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();

            // Why using task? Because Android side is asynchronous. 
            Task.Run(() =>
            {
                snapshotMessage.OnSnapshot.Invoke(snapshot.AsPNG().AsStream());
            });
        }

        void CameraPositionChanged(object sender, GMSCameraEventArgs args)
        {
            OnCameraPositionChanged(args.Position);
        }

        void CameraPositionChanged(object sender, GMSCameraEventArgs mkMapViewChangeEventArgs)
        {
            if (Element == null)
                return;

            var mapModel = (Map)Element;
            var mkMapView = (MapView)Control;

            var region = mkMapView.Projection.VisibleRegion;
            var minLat = Math.Min(Math.Min(Math.Min(region.NearLeft.Latitude, region.NearRight.Latitude), region.FarLeft.Latitude), region.FarRight.Latitude);
            var minLon = Math.Min(Math.Min(Math.Min(region.NearLeft.Longitude, region.NearRight.Longitude), region.FarLeft.Longitude), region.FarRight.Longitude);
            var maxLat = Math.Max(Math.Max(Math.Max(region.NearLeft.Latitude, region.NearRight.Latitude), region.FarLeft.Latitude), region.FarRight.Latitude);
            var maxLon = Math.Max(Math.Max(Math.Max(region.NearLeft.Longitude, region.NearRight.Longitude), region.FarLeft.Longitude), region.FarRight.Longitude);
            try
            {
                mapModel.CameraMoving = true;
                mapModel.MapRegion = new MapSpan(mkMapViewChangeEventArgs.Position.Target.ToPosition(), maxLat - minLat, maxLon - minLon);
            }
            finally { mapModel.CameraMoving = false; }
            //var camera = pos.ToXamarinForms();
            //Map.CameraPosition = camera;
            //Map.SendCameraChanged(camera);
        }

        void CoordinateTapped(object sender, GMSCoordEventArgs e)
        {
            Map.SendMapClicked(e.Coordinate.ToPosition());
        }

        void CoordinateLongPressed(object sender, GMSCoordEventArgs e)
        {
            Map.SendMapLongClicked(e.Coordinate.ToPosition());
        }

        bool DidTapMyLocation(MapView mapView)
        {
            return Map.SendMyLocationClicked();
        }


        void MoveToRegion(MapSpan mapSpan, bool animated = true)
        {
            Position center = mapSpan.Center;
            var halfLat = mapSpan.LatitudeDegrees / 2d;
            var halfLong = mapSpan.LongitudeDegrees / 2d;
            var mapRegion = new CoordinateBounds(new CLLocationCoordinate2D(center.Latitude - halfLat, center.Longitude - halfLong),
                                                new CLLocationCoordinate2D(center.Latitude + halfLat, center.Longitude + halfLong));

            if (animated)
                ((MapView)Control).Animate(CameraUpdate.FitBounds(mapRegion));
            else
                ((MapView)Control).MoveCamera(CameraUpdate.FitBounds(mapRegion));
        }

        private void UpdateHasScrollEnabled(bool? initialScrollGesturesEnabled = null)
        {
            NativeMap.Settings.ScrollGestures = initialScrollGesturesEnabled ?? ((Map)Element).HasScrollEnabled;
        }

        private void UpdateHasZoomEnabled(bool? initialZoomGesturesEnabled = null)
        {
            NativeMap.Settings.ZoomGestures = initialZoomGesturesEnabled ?? ((Map)Element).HasZoomEnabled;
        }

        private void UpdateHasRotationEnabled(bool? initialRotateGesturesEnabled = null)
        {
            NativeMap.Settings.RotateGestures = initialRotateGesturesEnabled ?? ((Map)Element).HasRotationEnabled;
        }

        private void UpdateIsShowingUser(bool? initialMyLocationButtonEnabled = null)
        {
            ((MapView)Control).MyLocationEnabled = ((Map)Element).IsShowingUser;
            ((MapView)Control).Settings.MyLocationButton = initialMyLocationButtonEnabled ?? ((Map)Element).IsShowingUser;
        }

        void UpdateMyLocationEnabled()
        {
            ((MapView)Control).MyLocationEnabled = ((Map)Element).MyLocationEnabled;
        }

        void UpdateIsTrafficEnabled()
        {
            ((MapView)Control).TrafficEnabled = ((Map)Element).IsTrafficEnabled;
        }

        void UpdateHasIndoorEnabled()
        {
            ((MapView)Control).IndoorEnabled = ((Map)Element).IsIndoorEnabled;
        }

        //void MoveMyLocationButton(LayoutOptions horizontal, LayoutOptions vertical)
        //{
        //    var mapView = ((MapView)Control);
        //    var subView=mapView.Subviews.GetValue(mapView.Subviews.Length - 1);
        //    UIKit.UIButton button = subView as UIKit.UIButton;
        //    if(button!=null)
        //    {
        //        button.AutoresizingMask= UIKit.UIViewAutoresizing.FlexibleRightMargin | UIKit.UIViewAutoresizing.FlexibleTopMargin;
        //        CoreGraphics.CGRect frame = button.Frame;
        //        //frame.Origin.X = 5;
        //        button.Frame = frame;
        //    }
        //}

        void UpdateMapType()
        {
            switch (((Map)Element).MapType)
            {
                case MapType.Street:
                    ((MapView)Control).MapType = MapViewType.Normal;
                    break;
                case MapType.Satellite:
                    ((MapView)Control).MapType = MapViewType.Satellite;
                    break;
                case MapType.Hybrid:
                    ((MapView)Control).MapType = MapViewType.Hybrid;
                    break;
                case MapType.None:
                    ((MapView)Control).MapType = MapViewType.None;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}