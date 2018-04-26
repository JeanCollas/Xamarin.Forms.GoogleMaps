using MapDance.Tools;
using System;

namespace Xamarin.Forms.GoogleMaps
{
    public class Pin : NotifyClass// BindableObject
    {
        public IPin _IPin;

        public string Label { get => _IPin.PinTitle; set { } }
        public string Address { get => _IPin.PinSubTitle; set { } }
        public BitmapDescriptor Icon { get => _IPin.PinIcon; set { } }
        public PinIconType IconType { get => _IPin.PinIconType; set { } }
        public bool IsDraggable { get => _IPin.PinIsDraggable; set { } }
        public Position Position { get => _IPin.PinPosition; set { } }
        public float Rotation { get => _IPin.PinRotation; set { } }
        public int ZIndex { get => _IPin.ZIndex; set { } }
        public AppearMarkerAnimation AppearAnimation{ get { return _IPin.PinConfig.AppearAnimation; } }

        //public PinType Type { get => _IPin.Typ; set { } }

        //private PinType _Type = default(PinType);
        //public PinType Type { get { return _Type; } set { bool changed = _Type != value; if (changed) { NotifyIAmChanging(); _Type = value; NotifyIChanged(); } } }

        //public static readonly BindableProperty TypeProperty 
        //    = BindableProperty.Create("Type", typeof(PinType), typeof(Pin), default(PinType));

        //public static readonly BindableProperty PositionProperty
        //    = BindableProperty.Create("Position", typeof(Position), typeof(Pin), default(Position));

        //public static readonly BindableProperty LabelProperty
        //    = BindableProperty.Create("Label", typeof(string), typeof(Pin), default(string));

        //public static readonly BindableProperty AddressProperty
        //    = BindableProperty.Create("Address", typeof(string), typeof(Pin), default(string));

        //public static readonly BindableProperty IconTypeProperty
        //    = BindableProperty.Create(nameof(IconType), typeof(PinIconType), typeof(Pin), default(PinIconType));

        //public static readonly BindableProperty IconProperty
        //    = BindableProperty.Create(nameof(Icon), typeof(BitmapDescriptor), typeof(Pin), default(BitmapDescriptor));

        //public static readonly BindableProperty IsDraggableProperty
        //    = BindableProperty.Create("IsDraggable", typeof(bool), typeof(Pin), false);

        //public static readonly BindableProperty ZIndexProperty
        //    = BindableProperty.Create(nameof(ZIndex), typeof(int), typeof(Pin), 0);

        //public static readonly BindableProperty RotationProperty
        //    = BindableProperty.Create("Rotation", typeof(float), typeof(Pin), 0f);

        //public static readonly BindableProperty AppearAnimationProperty
        //    = BindableProperty.Create(nameof(AppearAnimation), typeof(AppearMarkerAnimation), typeof(Pin), default(AppearMarkerAnimation));

        public Pin(IPin iPin)
        {
            this._IPin = iPin;
            iPin.PropertyChanged += IPin_PropertyChanged;
            if (iPin.PinConfig != null)
            {
                //pin.SetBinding(Pin.TypeProperty, nameof(IPin.PinConfig) + "." + nameof(IPinConfig.PinType));
                NotifyPropertyChanged(nameof(AppearAnimation)); //pin.SetBinding(Pin.AppearAnimationProperty, nameof(IPin.PinConfig) + "." + nameof(IPinConfig.AppearAnimation));
            }
        }

        private void IPin_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (_IPin == null) return;
            switch (e.PropertyName)
            {
                case nameof(IPin.PinTitle): NotifyPropertyChanged(nameof(Label)); break;// Label = _IPin.PinTitle; break;
                case nameof(IPin.PinSubTitle): NotifyPropertyChanged(nameof(Address)); break;// = _IPin.PinSubTitle; break;
                case nameof(IPin.PinIcon): NotifyPropertyChanged(nameof(Icon)); break;// = _IPin.PinIcon; break;
                case nameof(IPin.PinIconType): NotifyPropertyChanged(nameof(IconType)); break;// = _IPin.PinIconType; break;
                case nameof(IPin.PinIsDraggable): NotifyPropertyChanged(nameof(IsDraggable)); break;// = _IPin.PinIsDraggable; break;
                case nameof(IPin.PinPosition): NotifyPropertyChanged(nameof(Position)); break;// = _IPin.PinPosition; break;
                case nameof(IPin.PinRotation): NotifyPropertyChanged(nameof(Rotation)); break;// = _IPin.PinRotation; break;
                case nameof(IPin.ZIndex): NotifyPropertyChanged(nameof(ZIndex)); break;// = _IPin.ZIndex; break;
            }
        }



        //public string Label
        //{
        //    get { return (string)GetValue(LabelProperty); }
        //    set { SetValue(LabelProperty, value); }
        //}

        //public string Address
        //{
        //    get { return (string)GetValue(AddressProperty); }
        //    set { SetValue(AddressProperty, value); }
        //}

        //public int ZIndex
        //{
        //    get { return (int)GetValue(ZIndexProperty); }
        //    set { SetValue(ZIndexProperty, value); }
        //}

        //public Position Position
        //{
        //    get { return (Position)GetValue(PositionProperty); }
        //    set { SetValue(PositionProperty, value); }
        //}

        //public PinType Type
        //{
        //    get { return (PinType)GetValue(TypeProperty); }
        //    set { SetValue(TypeProperty, value); }
        //}

        //public PinIconType IconType
        //{
        //    get { return (PinIconType)GetValue(IconTypeProperty); }
        //    set { SetValue(IconTypeProperty, value); }
        //}

        //public BitmapDescriptor Icon
        //{
        //    get { return (BitmapDescriptor)GetValue(IconProperty); }
        //    set { SetValue(IconProperty, value); }
        //}

        //public bool IsDraggable
        //{
        //    get { return (bool)GetValue(IsDraggableProperty); }
        //    set { SetValue(IsDraggableProperty, value); }
        //}

        //public float Rotation
        //{
        //    get { return (float)GetValue(RotationProperty); }
        //    set { SetValue(RotationProperty, value); }
        //}

        //public AppearMarkerAnimation AppearAnimation
        //{
        //    get { return (AppearMarkerAnimation)GetValue(AppearAnimationProperty); }
        //    set { SetValue(AppearAnimationProperty, value); }
        //}

        public object Tag { get; set; }

        public object NativeObject { get; set; }

        [Obsolete("Please use Map.PinClicked instead of this")]
        public event EventHandler Clicked;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != GetType())
                return false;
            return Equals((Pin)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Label?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ Position.GetHashCode();
                //hashCode = (hashCode * 397) ^ (int)Type;
                hashCode = (hashCode * 397) ^ (Address?.GetHashCode() ?? 0);
                return hashCode;
            }
        }

        public static bool operator ==(Pin left, Pin right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Pin left, Pin right)
        {
            return !Equals(left, right);
        }

        public bool SendTap()
        {
            EventHandler handler = Clicked;
            if (handler == null)
                return false;

            handler(this, EventArgs.Empty);
            return true;
        }

        bool Equals(Pin other)
        {
            return string.Equals(Label, other.Label) && Equals(Position, other.Position) /*&& Type == other.Type*/ && string.Equals(Address, other.Address);
        }
    }
}