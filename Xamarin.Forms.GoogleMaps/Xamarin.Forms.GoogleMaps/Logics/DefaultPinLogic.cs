using System;
using System.ComponentModel;

namespace Xamarin.Forms.GoogleMaps.Logics
{
    internal abstract class DefaultPinLogic<TNative, TNativeMap> : DefaultLogic<IPin, TNative, TNativeMap>
        where TNative : class
        where TNativeMap : class
    {
        protected override void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnItemPropertyChanged(sender, e);
            var outerItem = sender as IPin;
            var nativeItem = outerItem?.NativeObject as TNative;

            if (nativeItem == null)
                return;

            if (e.PropertyName == nameof(IPin.Address)) OnUpdateAddress(outerItem, nativeItem);
            else if (e.PropertyName == nameof(IPin.Label)) OnUpdateLabel(outerItem, nativeItem);
            else if (e.PropertyName == nameof(IPin.Position)) OnUpdatePosition(outerItem, nativeItem);
            else if (e.PropertyName == nameof(IPin.Type)) OnUpdateType(outerItem, nativeItem);
            else if (e.PropertyName == nameof(IPin.Icon)) OnUpdateIcon(outerItem, nativeItem);
            else if (e.PropertyName == nameof(IPin.IsDraggable)) OnUpdateIsDraggable(outerItem, nativeItem);
            else if (e.PropertyName == nameof(IPin.Rotation)) OnUpdateRotation(outerItem, nativeItem);
            else if (e.PropertyName == nameof(IPin.IsVisible)) OnUpdateIsVisible(outerItem, nativeItem);
            else if (e.PropertyName == nameof(IPin.Anchor)) OnUpdateAnchor(outerItem, nativeItem);
            else if (e.PropertyName == nameof(IPin.Flat)) OnUpdateFlat(outerItem, nativeItem);
            else if (e.PropertyName == nameof(IPin.InfoWindowAnchor)) OnUpdateInfoWindowAnchor(outerItem, nativeItem);
            else if (e.PropertyName == nameof(IPin.ZIndex)) OnUpdateZIndex(outerItem, nativeItem);
            else if (e.PropertyName == nameof(IPin.Transparency)) OnUpdateTransparency(outerItem, nativeItem);
        }

        protected abstract void OnUpdateAddress(IPin outerItem, TNative nativeItem);

        protected abstract void OnUpdateLabel(IPin outerItem, TNative nativeItem);

        protected abstract void OnUpdatePosition(IPin outerItem, TNative nativeItem);

        protected abstract void OnUpdateType(IPin outerItem, TNative nativeItem);

        protected abstract void OnUpdateIcon(IPin outerItem, TNative nativeItem);

        protected abstract void OnUpdateIsDraggable(IPin outerItem, TNative nativeItem);

        protected abstract void OnUpdateRotation(IPin outerItem, TNative nativeItem);

        protected abstract void OnUpdateIsVisible(IPin outerItem, TNative nativeItem);

        protected abstract void OnUpdateAnchor(IPin outerItem, TNative nativeItem);

        protected abstract void OnUpdateFlat(IPin outerItem, TNative nativeItem);

        protected abstract void OnUpdateInfoWindowAnchor(IPin outerItem, TNative nativeItem);

        protected abstract void OnUpdateZIndex(IPin outerItem, TNative nativeItem);

        protected abstract void OnUpdateTransparency(IPin outerItem, TNative nativeItem);
    }
}

