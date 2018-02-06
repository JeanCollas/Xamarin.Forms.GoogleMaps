using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms.GoogleMaps.Events;
using Xamarin.Forms.GoogleMaps.Internals;

namespace Xamarin.Forms.GoogleMaps
{
    public class Map : View, IEnumerable<Pin>
    {
        public Map(MapSpan region)
        {
            LastMoveToRegion = MapRegion ?? region;

            VerticalOptions = HorizontalOptions = LayoutOptions.FillAndExpand;

            _pins.CollectionChanged += PinsOnCollectionChanged;
            _polylines.CollectionChanged += PolylinesOnCollectionChanged;
            _polygons.CollectionChanged += PolygonsOnCollectionChanged;
            _circles.CollectionChanged += CirclesOnCollectionChanged;
            _tileLayers.CollectionChanged += TileLayersOnCollectionChanged;
            _groundOverlays.CollectionChanged += GroundOverlays_CollectionChanged;
        }

        public void SetupMap(MapSpan region)
        {
            LastMoveToRegion = MapRegion ?? region;
        }

        // center on Rome by default
        public Map() : this(new MapSpan(new Position(41.890202, 12.492049), 0.1, 0.1)) { }
        public UiSettings UiSettings { get; } = new UiSettings();

        #region PinsSource Property
        public static readonly BindableProperty PinsSourceProperty = BindableProperty.Create(nameof(PinsSource), typeof(ObservableCollection<IPin>), typeof(Map), null, propertyChanged: OnPinsSourceChanged);
        public ObservableCollection<IPin> PinsSource { get { return (ObservableCollection<IPin>)GetValue(PinsSourceProperty); } set { SetValue(PinsSourceProperty, value); } }
        private void Register(ObservableCollection<IPin> source) { if (source != null) source.CollectionChanged += OnPinSourceCollectionChanged; }
        private void Unregister(ObservableCollection<IPin> source) { if (source != null) source.CollectionChanged -= OnPinSourceCollectionChanged; }

        static void OnPinsSourceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var map = bindable as Map;
            if (map == null) return;
            var source = oldValue as ObservableCollection<IPin>;
            if (source != null) map.Unregister(source);
            source = newValue as ObservableCollection<IPin>;
            if (source != null) map.Register(source);
        }

        void OnPinSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            switch (notifyCollectionChangedEventArgs.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    AddPins(notifyCollectionChangedEventArgs.NewItems);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    RemovePins(notifyCollectionChangedEventArgs.OldItems);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    RemovePins(notifyCollectionChangedEventArgs.OldItems);
                    AddPins(notifyCollectionChangedEventArgs.NewItems);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    var pins = Pins.ToList();
                    foreach (var p in pins) Pins.Remove(p);
                    AddPins((IList)PinsSource);
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
            }
        }

        private void RemovePins(IList oldPins)
        {
            if (oldPins != null)
                foreach (IPin oldPin in oldPins)
                {
                    var pin = Pins.FirstOrDefault(p => ReferenceEquals(oldPin, p.BindingContext));
                    Pins.Remove(pin);
                }
        }

        private void AddPins(IList newPins)
        {
            if (newPins != null)
                foreach (IPin p in newPins) Pins.Add(p.ToPin());
        }
        #endregion PinsSource

        #region CirclesSource Property
        public static readonly BindableProperty CirclesSourceProperty = BindableProperty.Create(nameof(CirclesSource), typeof(ObservableCollection<ICircle>), typeof(Map), null, propertyChanged: OnCirclesSourceChanged);
        public ObservableCollection<ICircle> CirclesSource { get { return (ObservableCollection<ICircle>)GetValue(CirclesSourceProperty); } set { SetValue(CirclesSourceProperty, value); } }
        private void Register(ObservableCollection<ICircle> source) { if (source != null) source.CollectionChanged += OnCircleSourceCollectionChanged; }
        private void Unregister(ObservableCollection<ICircle> source) { if (source != null) source.CollectionChanged -= OnCircleSourceCollectionChanged; }

        static void OnCirclesSourceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var map = bindable as Map;
            if (map == null) return;
            var source = oldValue as ObservableCollection<ICircle>;
            if (source != null) map.Unregister(source);
            source = newValue as ObservableCollection<ICircle>;
            if (source != null) map.Register(source);
        }

        void OnCircleSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            switch (notifyCollectionChangedEventArgs.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    AddCircles(notifyCollectionChangedEventArgs.NewItems);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    RemoveCircles(notifyCollectionChangedEventArgs.OldItems);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    RemoveCircles(notifyCollectionChangedEventArgs.OldItems);
                    AddCircles(notifyCollectionChangedEventArgs.NewItems);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    var circles = Circles.ToList();
                    foreach (var p in circles) Circles.Remove(p);
                    AddCircles((IList)CirclesSource);
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
            }
        }

        private void RemoveCircles(IList oldCircles)
        {
            if (oldCircles != null)
                foreach (ICircle oldCircle in oldCircles)
                {
                    var circle = Circles.FirstOrDefault(p => ReferenceEquals(oldCircle, p.BindingContext));
                    Circles.Remove(circle);
                }
        }

        private void AddCircles(IList newCircles)
        {
            if (newCircles != null)
                foreach (ICircle p in newCircles) Circles.Add(p.ToCircle());
        }
        #endregion CirclesSource

        #region MapRegion 
        // Allows to set map center at initiation
        public static readonly BindableProperty MapRegionProperty = BindableProperty.Create(nameof(MapRegion), typeof(MapSpan), typeof(Map), null, propertyChanged: OnMapRegionChanged);
        public MapSpan MapRegion { get { return (MapSpan)GetValue(MapRegionProperty); } set { SetValue(MapRegionProperty, value); } }
        public static readonly BindableProperty CameraMovingProperty = BindableProperty.Create(nameof(CameraMoving), typeof(bool), typeof(Map), false);
        public bool CameraMoving { get { return (bool)GetValue(CameraMovingProperty); } set { SetValue(CameraMovingProperty, value); } }

        internal void SetMapRegionInternal(MapSpan newValue)
        {
            CameraMoving = true;
            MapRegion = newValue;
            CameraMoving = false;
            DelayOnMapMoveEnded();
        }

        Tools.Delayer MapMoveEndDelayer;
        private void DelayOnMapMoveEnded()
        {
            if (MapMoveEndDelayer == null) { MapMoveEndDelayer = new Tools.Delayer(1000); MapMoveEndDelayer.Action += MapMoveEndDelayer_Action; }
            MapMoveEndDelayer.ResetAndTick();
        }
        private void MapMoveEndDelayer_Action(object sender, EventArgs e)
            => SendMapMoveEnded();

        private static void OnMapRegionChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (newValue == oldValue) return;
            var newVal = newValue as MapSpan;
            if (newVal == null)
#if DEBUG
                try
                {
                    throw new ArgumentNullException(nameof(newValue));
                }
                catch { }
#else
                return;
#endif
            var map = bindable as Map;
            if (map == null) return;
            if (map.CameraMoving)
            {
                map.MapRegionChanged?.Invoke(map, new Events.RegionEventArgs() { OldValue = oldValue as MapSpan, NewValue = newVal });
                return;
            }
            else
                map.MoveToRegion(newVal, true);
        }
        public event EventHandler<RegionEventArgs> MapRegionChanged;

        internal MapSpan LastMoveToRegion { get; private set; }

        public void MoveToRegion(MapSpan mapSpan, bool animate = true)
        {
            if (mapSpan == null)
#if DEBUG
                try
                {
                    throw new ArgumentNullException(nameof(mapSpan));
                }
                catch { return; }
#else
                return;
#endif
            if (LastMoveToRegion == mapSpan) return;
            LastMoveToRegion = mapSpan;
            MessagingCenter.Send(this, MoveMessageName, new MoveToRegionMessage(mapSpan, animate));
        }

        public const string MoveMessageName = "MapMoveToRegion";


        public Task<AnimationStatus> MoveCamera(CameraUpdate cameraUpdate)
        {
            var comp = new TaskCompletionSource<AnimationStatus>();

            SendMoveCamera(new CameraUpdateMessage(cameraUpdate, null, new DelegateAnimationCallback(
                () => comp.SetResult(AnimationStatus.Finished),
                () => comp.SetResult(AnimationStatus.Canceled))));

            return comp.Task;
        }

        public Task<AnimationStatus> AnimateCamera(CameraUpdate cameraUpdate, TimeSpan? duration = null)
        {
            var comp = new TaskCompletionSource<AnimationStatus>();

            SendAnimateCamera(new CameraUpdateMessage(cameraUpdate, duration, new DelegateAnimationCallback(
                () => comp.SetResult(AnimationStatus.Finished),
                () => comp.SetResult(AnimationStatus.Canceled))));

            return comp.Task;
        }


        public Task<Stream> TakeSnapshot()
        {
            var comp = new TaskCompletionSource<Stream>();

            SendTakeSnapshot(new TakeSnapshotMessage(image => comp.SetResult(image)));

            return comp.Task;
        }
        #endregion MapRegion

        #region Selected Pin/Item

        public static readonly BindableProperty SelectedPinProperty = BindableProperty.Create(nameof(SelectedPin), typeof(Pin), typeof(Map), default(Pin), defaultBindingMode: BindingMode.TwoWay, propertyChanged: OnSelectedPinChanged);

        public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create(nameof(SelectedItem), typeof(IPin), typeof(Map), default(IPin), defaultBindingMode: BindingMode.TwoWay, propertyChanged: OnSelectedItemChanged);

        private static void OnSelectedItemChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var map = bindable as Map; if (map == null) return;
            if (newValue == null && map.SelectedPin != null) { map.SelectedPin = null; return; }
            var pin = map.Pins.FirstOrDefault(p => p.BindingContext == newValue);
            // could add an exception if null, it means the object is not in the list
            map.SelectedPin = pin;
        }

        private static void OnSelectedPinChanged(BindableObject bindable, object oldValue, object newValue)
        { var map = bindable as Map; if (map == null) return; map.SelectedItem = map.SelectedPin?.BindingContext as IPin; }

        public Pin SelectedPin { get { return (Pin)GetValue(SelectedPinProperty); } set { SetValue(SelectedPinProperty, value); } }

        public IPin SelectedItem { get { return (IPin)GetValue(SelectedItemProperty); } set { SetValue(SelectedItemProperty, value); } }

        #endregion Selected Pin/Item

        #region Map Config
        public static readonly BindableProperty MapTypeProperty = BindableProperty.Create(nameof(MapType), typeof(MapType), typeof(Map), default(MapType));

        public static readonly BindableProperty IsShowingCompasProperty = BindableProperty.Create(nameof(IsShowingCompas), typeof(bool), typeof(Map), default(bool));

        public static readonly BindableProperty IsShowingUserProperty = BindableProperty.Create(nameof(IsShowingUser), typeof(bool), typeof(Map), default(bool));

        public static readonly BindableProperty HasRotationEnabledProperty = BindableProperty.Create(nameof(HasRotationEnabled), typeof(bool), typeof(Map), true);

        public static readonly BindableProperty MyLocationEnabledProperty = BindableProperty.Create(nameof(MyLocationEnabled), typeof(bool), typeof(Map), default(bool));

        public static readonly BindableProperty HasScrollEnabledProperty = BindableProperty.Create(nameof(HasScrollEnabled), typeof(bool), typeof(Map), true);

        public static readonly BindableProperty HasZoomEnabledProperty = BindableProperty.Create(nameof(HasZoomEnabled), typeof(bool), typeof(Map), true);

        public static readonly BindableProperty HasZoomButtonsProperty = BindableProperty.Create(nameof(HasZoomButtons), typeof(bool), typeof(Map), false);

        public static readonly BindableProperty IsTrafficEnabledProperty = BindableProperty.Create(nameof(IsTrafficEnabled), typeof(bool), typeof(Map), false);

        public static readonly BindableProperty IndoorEnabledProperty = BindableProperty.Create(nameof(IsIndoorEnabled), typeof(bool), typeof(Map), true);

        public static readonly BindableProperty InitialCameraUpdateProperty = BindableProperty.Create(nameof(InitialCameraUpdate), typeof(CameraUpdate), typeof(Map),
    CameraUpdateFactory.NewPositionZoom(new Position(41.89, 12.49), 10),  // center on Rome by default
    propertyChanged: (bindable, oldValue, newValue) => { ((Map)bindable)._useMoveToRegisonAsInitialBounds = false; });

        public static readonly BindableProperty PaddingProperty = BindableProperty.Create(nameof(PaddingProperty), typeof(Thickness), typeof(Map), default(Thickness));

        bool _useMoveToRegisonAsInitialBounds = true;

        public static readonly BindableProperty CameraPositionProperty = BindableProperty.Create(
            nameof(CameraPosition), typeof(CameraPosition), typeof(Map),
            defaultValueCreator: (bindable) => new CameraPosition(((Map)bindable).InitialCameraUpdate.Position, 10),
            defaultBindingMode: BindingMode.TwoWay);

        public static readonly BindableProperty MapStyleProperty = BindableProperty.Create(nameof(MapStyle), typeof(MapStyle), typeof(Map), null);

        [Obsolete("Please use Map.UiSettings.RotateGesturesEnabled instead of this")]
        public bool HasRotationEnabled
        {
            get { return (bool)GetValue(HasRotationEnabledProperty); }
            set { SetValue(HasRotationEnabledProperty, value); }
        }
        public bool HasScrollEnabled { get { return (bool)GetValue(HasScrollEnabledProperty); } set { SetValue(HasScrollEnabledProperty, value); } }

        public bool HasZoomEnabled { get { return (bool)GetValue(HasZoomEnabledProperty); } set { SetValue(HasZoomEnabledProperty, value); } }

        /// <summary>Show/hide zoom buttons (Android only)</summary>
        public bool HasZoomButtons { get { return (bool)GetValue(HasZoomButtonsProperty); } set { SetValue(HasZoomButtonsProperty, value); } }

        public bool IsTrafficEnabled { get { return (bool)GetValue(IsTrafficEnabledProperty); } set { SetValue(IsTrafficEnabledProperty, value); } }

        public bool IsShowingCompas { get { return (bool)GetValue(IsShowingCompasProperty); } set { SetValue(IsShowingCompasProperty, value); } }

        [Obsolete("Please use Map.MyLocationEnabled and Map.UiSettings.MyLocationButtonEnabled instead of this")]
        public bool IsShowingUser { get { return (bool)GetValue(IsShowingUserProperty); } set { SetValue(IsShowingUserProperty, value); } }

        public bool MyLocationEnabled { get { return (bool)GetValue(MyLocationEnabledProperty); } set { SetValue(MyLocationEnabledProperty, value); } }

        public MapType MapType { get { return (MapType)GetValue(MapTypeProperty); } set { SetValue(MapTypeProperty, value); } }

        public bool IsIndoorEnabled { get { return (bool)GetValue(IndoorEnabledProperty); } set { SetValue(IndoorEnabledProperty, value); } }

        [TypeConverter(typeof(CameraUpdateConverter))]
        public CameraUpdate InitialCameraUpdate { get { return (CameraUpdate)GetValue(InitialCameraUpdateProperty); } set { SetValue(InitialCameraUpdateProperty, value); } }

        public CameraPosition CameraPosition
        {
            get { return (CameraPosition)GetValue(CameraPositionProperty); }
            internal set { SetValue(CameraPositionProperty, value); }
        }

        public Thickness Padding
        {
            get { return (Thickness)GetValue(PaddingProperty); }
            set { SetValue(PaddingProperty, value); }
        }

        public MapStyle MapStyle
        {
            get { return (MapStyle)GetValue(MapStyleProperty); }
            set { SetValue(MapStyleProperty, value); }
        }
        #endregion Map Config

        #region Map Internal Data
        readonly ObservableCollection<Pin> _pins = new ObservableCollection<Pin>();
        readonly ObservableCollection<Polyline> _polylines = new ObservableCollection<Polyline>();
        readonly ObservableCollection<Polygon> _polygons = new ObservableCollection<Polygon>();
        readonly ObservableCollection<Circle> _circles = new ObservableCollection<Circle>();
        readonly ObservableCollection<TileLayer> _tileLayers = new ObservableCollection<TileLayer>();
        readonly ObservableCollection<GroundOverlay> _groundOverlays = new ObservableCollection<GroundOverlay>();

        public IList<Pin> Pins { get { return _pins; } }

        public IList<Polyline> Polylines { get { return _polylines; } }

        public IList<Polygon> Polygons { get { return _polygons; } }

        public IList<Circle> Circles { get { return _circles; } }

        public IList<TileLayer> TileLayers { get { return _tileLayers; } }

        public IList<GroundOverlay> GroundOverlays { get { return _groundOverlays; } }


        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<Pin> GetEnumerator() => _pins.GetEnumerator();

        #endregion Map Internal Data

        #region Data input consistency check

        void PinsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null && e.NewItems.Cast<Pin>().Any(pin => pin.Label == null))
                throw new ArgumentException("Pin must have a Label to be added to a map");
        }

        void PolylinesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null && e.NewItems.Cast<Polyline>().Any(polyline => polyline.Positions.Count < 2))
                throw new ArgumentException("Polyline must have a 2 positions to be added to a map");
        }

        void PolygonsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null && e.NewItems.Cast<Polygon>().Any(polygon => polygon.Positions.Count < 3))
                throw new ArgumentException("Polygon must have a 3 positions to be added to a map");
        }

        void CirclesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null && e.NewItems.Cast<Circle>().Any(circle => (
                circle?.Center == null || circle?.Radius == null || circle.Radius.Meters <= 0f)))
                throw new ArgumentException("Circle must have a center and radius");
        }

        void TileLayersOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            //if (e.NewItems != null && e.NewItems.Cast<ITileLayer>().Any(tileLayer => (circle.Center == null || circle.Radius == null || circle.Radius.Meters <= 0f)))
            //	throw new ArgumentException("Circle must have a center and radius");
        }

        void GroundOverlays_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
        }

        #endregion Data input consistency check

        #region Map events
        public event EventHandler<PinClickedEventArgs> PinClicked;
        public event EventHandler<SelectedPinChangedEventArgs> SelectedPinChanged;
        public event EventHandler<InfoWindowClickedEventArgs> InfoWindowClicked;
        public event EventHandler<InfoWindowLongClickedEventArgs> InfoWindowLongClicked;

        public event EventHandler<PinDragEventArgs> PinDragStart;
        public event EventHandler<PinDragEventArgs> PinDragEnd;
        public event EventHandler<PinDragEventArgs> PinDragging;

        public event EventHandler<MapClickedEventArgs> MapClicked;
        public event EventHandler<MapLongClickedEventArgs> MapLongClicked;

        public event EventHandler<MapMoveEndedEventArgs> MapMoveEnded;
        public event EventHandler<MyLocationButtonClickedEventArgs> MyLocationButtonClicked;

        [Obsolete("Please use Map.CameraIdled instead of this")]
        public event EventHandler<CameraChangedEventArgs> CameraChanged;
        public event EventHandler<CameraMoveStartedEventArgs> CameraMoveStarted;
        public event EventHandler<CameraMovingEventArgs> CameraMoving;
        public event EventHandler<CameraIdledEventArgs> CameraIdled;

        internal Action<MoveToRegionMessage> OnMoveToRegion { get; set; }

        internal Action<CameraUpdateMessage> OnMoveCamera { get; set; }

        internal Action<CameraUpdateMessage> OnAnimateCamera { get; set; }

        internal Action<TakeSnapshotMessage> OnSnapshot { get; set; }


        public static readonly BindableProperty SelectedPinChangedCommandProperty = BindableProperty.Create(nameof(SelectedPinChangedCommand), typeof(ICommand), typeof(Map), null);
        public ICommand SelectedPinChangedCommand { get { return (ICommand)GetValue(SelectedPinChangedCommandProperty); } set { SetValue(SelectedPinChangedCommandProperty, value); } }

        public static readonly BindableProperty PinClickedCommandProperty = BindableProperty.Create(nameof(PinClickedCommand), typeof(ICommand), typeof(Map), null);
        public ICommand PinClickedCommand { get { return (ICommand)GetValue(PinClickedCommandProperty); } set { SetValue(PinClickedCommandProperty, value); } }

        public static readonly BindableProperty InfoWindowClickedCommandProperty = BindableProperty.Create(nameof(InfoWindowClickedCommand), typeof(ICommand), typeof(Map), null);
        public ICommand InfoWindowClickedCommand { get { return (ICommand)GetValue(InfoWindowClickedCommandProperty); } set { SetValue(InfoWindowClickedCommandProperty, value); } }

        public static readonly BindableProperty PinDragStartCommandProperty = BindableProperty.Create(nameof(PinDragStartCommand), typeof(ICommand), typeof(Map), null);
        public ICommand PinDragStartCommand { get { return (ICommand)GetValue(PinDragStartCommandProperty); } set { SetValue(PinDragStartCommandProperty, value); } }

        public static readonly BindableProperty PinDragEndCommandProperty = BindableProperty.Create(nameof(PinDragEndCommand), typeof(ICommand), typeof(Map), null);
        public ICommand PinDragEndCommand { get { return (ICommand)GetValue(PinDragEndCommandProperty); } set { SetValue(PinDragEndCommandProperty, value); } }

        public static readonly BindableProperty PinDraggingCommandProperty = BindableProperty.Create(nameof(PinDraggingCommand), typeof(ICommand), typeof(Map), null);
        public ICommand PinDraggingCommand { get { return (ICommand)GetValue(PinDraggingCommandProperty); } set { SetValue(PinDraggingCommandProperty, value); } }

        public static readonly BindableProperty MapClickedCommandProperty = BindableProperty.Create(nameof(MapClickedCommand), typeof(ICommand), typeof(Map), null);
        public ICommand MapClickedCommand { get { return (ICommand)GetValue(MapClickedCommandProperty); } set { SetValue(MapClickedCommandProperty, value); } }

        public static readonly BindableProperty MapLongClickedCommandProperty = BindableProperty.Create(nameof(MapLongClickedCommand), typeof(ICommand), typeof(Map), null);
        public ICommand MapLongClickedCommand { get { return (ICommand)GetValue(MapLongClickedCommandProperty); } set { SetValue(MapLongClickedCommandProperty, value); } }

        public static readonly BindableProperty MapMoveEndedCommandProperty = BindableProperty.Create(nameof(MapMoveEndedCommand), typeof(ICommand), typeof(Map), null);
        public ICommand MapMoveEndedCommand { get { return (ICommand)GetValue(MapMoveEndedCommandProperty); } set { SetValue(MapMoveEndedCommandProperty, value); } }


        internal void SendSelectedPinChanged(Pin selectedPin)
        {
            var item = selectedPin?.BindingContext as IPin;
            var args = new SelectedPinChangedEventArgs(selectedPin, selectedPin?.BindingContext as IPin);
            SelectedPinChanged?.Invoke(this, args);
            if (SelectedPinChangedCommand?.CanExecute(args) ?? false) SelectedPinChangedCommand.Execute(args);
            if (item?.PinConfig?.PinSelectedCommand?.CanExecute(item?.PinConfig?.PinSelectedCommandParameter ?? args) ?? false)
                item.PinConfig?.PinSelectedCommand.Execute(item?.PinConfig?.PinSelectedCommandParameter ?? args);
        }

        internal bool SendPinClicked(Pin pin)
        {
            var item = pin?.BindingContext as IPin;
            var args = new PinClickedEventArgs(pin, item);
            PinClicked?.Invoke(this, args);
            if (PinClickedCommand?.CanExecute(args) ?? false) PinClickedCommand.Execute(args);
            if (item?.PinConfig?.PinClickedCommand?.CanExecute(item?.PinConfig?.PinClickedCommandParameter ?? args) ?? false)
                item.PinConfig?.InfoWindowClickedCommand.Execute(item?.PinConfig?.PinClickedCommandParameter ?? args);
            return args.Handled;
        }

        internal void SendInfoWindowClicked(Pin pin)
        {
            var item = pin?.BindingContext as IPin;
            var args = new InfoWindowClickedEventArgs(pin, item);
            InfoWindowClicked?.Invoke(this, args);
            if (InfoWindowClickedCommand?.CanExecute(args) ?? false) InfoWindowClickedCommand.Execute(args);
            if (item?.PinConfig?.InfoWindowClickedCommand?.CanExecute(item?.PinConfig?.InfoWindowClickedCommandParameter ?? args) ?? false)
                item.PinConfig?.InfoWindowClickedCommand.Execute(item?.PinConfig?.InfoWindowClickedCommandParameter ?? args);
        }

        internal void SendInfoWindowLongClicked(Pin pin)
        {
            var args = new InfoWindowLongClickedEventArgs(pin);
            InfoWindowLongClicked?.Invoke(this, args);
        }

        internal void SendPinDragStart(Pin pin)
        {
            var item = pin?.BindingContext as IPin;
            var args = new PinDragEventArgs(pin, pin?.BindingContext as IPin);
            PinDragStart?.Invoke(this, args);
            if (PinDragStartCommand?.CanExecute(args) ?? false) PinDragStartCommand.Execute(args);
            if (item?.PinConfig?.PinDragStartCommand?.CanExecute(item?.PinConfig?.PinDragStartCommandParameter ?? args) ?? false)
                item.PinConfig?.PinDragStartCommand.Execute(item?.PinConfig?.PinDragStartCommandParameter ?? args);
        }

        internal void SendPinDragEnd(Pin pin)
        {
            var item = pin?.BindingContext as IPin;
            var args = new PinDragEventArgs(pin, pin?.BindingContext as IPin);
            PinDragEnd?.Invoke(this, args);
            if (PinDragEndCommand?.CanExecute(args) ?? false) PinDragEndCommand.Execute(args);
            if (item?.PinConfig?.PinDragEndCommand?.CanExecute(item?.PinConfig?.PinDragEndCommandParameter ?? args) ?? false)
                item.PinConfig?.PinDragEndCommand.Execute(item?.PinConfig?.PinDragEndCommandParameter ?? args);
        }

        internal void SendPinDragging(Pin pin)
        {
            var item = pin?.BindingContext as IPin;
            var args = new PinDragEventArgs(pin, pin?.BindingContext as IPin);
            PinDragging?.Invoke(this, args);
            if (PinDraggingCommand?.CanExecute(args) ?? false) PinDraggingCommand.Execute(args);
            if (item?.PinConfig?.PinDraggingCommand?.CanExecute(item?.PinConfig?.PinDraggingCommandParameter ?? args) ?? false)
                item.PinConfig?.PinDraggingCommand.Execute(item?.PinConfig?.PinDraggingCommandParameter ?? args);
        }

        internal void SendMapClicked(Position point)
        {
            var args = new MapClickedEventArgs(point);
            MapClicked?.Invoke(this, args);
            if (MapClickedCommand?.CanExecute(args) ?? false) MapClickedCommand.Execute(args);
        }

        internal void SendMapLongClicked(Position point)
        {
            var args = new MapLongClickedEventArgs(point);
            MapLongClicked?.Invoke(this, args);
            if (MapLongClickedCommand?.CanExecute(args) ?? false) MapLongClickedCommand.Execute(args);
        }

        internal void SendMapMoveEnded()
        {
            var args = new MapMoveEndedEventArgs() { Region = MapRegion };
            MapMoveEnded?.Invoke(this, args);
            if (MapMoveEndedCommand?.CanExecute(args) ?? false) MapMoveEndedCommand.Execute(args);
        }


        #endregion Map events


        public const string CenterOnMyLocationMessageName = "CenterOnMyLocation";
        public void CenterOnMyLocation()
        {
            MessagingCenter.Send(this, CenterOnMyLocationMessageName);
        }

        internal bool SendMyLocationClicked()
        {
            var args = new MyLocationButtonClickedEventArgs();
            MyLocationButtonClicked?.Invoke(this, args);
            return args.Handled;
        }

        internal void SendCameraChanged(CameraPosition position)
        {
            CameraChanged?.Invoke(this, new CameraChangedEventArgs(position));
        }

        internal void SendCameraMoveStarted(bool isGesture)
        {
            CameraMoveStarted?.Invoke(this, new CameraMoveStartedEventArgs(isGesture));
        }

        internal void SendCameraMoving(CameraPosition position)
        {
            CameraMoving?.Invoke(this, new CameraMovingEventArgs(position));
        }

        internal void SendCameraIdled(CameraPosition position)
        {
            CameraIdled?.Invoke(this, new CameraIdledEventArgs(position));
        }

        private void SendMoveToRegion(MoveToRegionMessage message)
        {
            OnMoveToRegion?.Invoke(message);
        }

        void SendMoveCamera(CameraUpdateMessage message)
        {
            OnMoveCamera?.Invoke(message);
        }

        void SendAnimateCamera(CameraUpdateMessage message)
        {
            OnAnimateCamera?.Invoke(message);
        }

        void SendTakeSnapshot(TakeSnapshotMessage message)
        {
            OnSnapshot?.Invoke(message);
        }
    }
}
