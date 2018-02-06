using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Xamarin.Forms.GoogleMaps.Logics
{
    internal abstract class DefaultCircleLogic<TNative, TNativeMap> : DefaultLogic<ICircle, TNative, TNativeMap>
        where TNative : class
        where TNativeMap : class
    {
        protected override void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnItemPropertyChanged(sender, e);
            var outerItem = sender as ICircle;
            var nativeItem = outerItem?.NativeObject as TNative;

            if (nativeItem == null)
                return;

            if (e.PropertyName == nameof(ICircle.StrokeWidth)) OnUpdateStrokeWidth(outerItem, nativeItem);
            else if (e.PropertyName == nameof(ICircle.StrokeColor)) OnUpdateStrokeColor(outerItem, nativeItem);
            else if (e.PropertyName == nameof(ICircle.FillColor)) OnUpdateFillColor(outerItem, nativeItem);
            else if (e.PropertyName == nameof(ICircle.Center)) OnUpdateCenter(outerItem, nativeItem);
            else if (e.PropertyName == nameof(ICircle.Radius)) OnUpdateRadius(outerItem, nativeItem);
            else if (e.PropertyName == nameof(ICircle.IsClickable)) OnUpdateIsClickable(outerItem, nativeItem);
            else if (e.PropertyName == nameof(ICircle.ZIndex)) OnUpdateZIndex(outerItem, nativeItem);
        }

        protected abstract void OnUpdateStrokeWidth(ICircle outerItem, TNative nativeItem);

        protected abstract void OnUpdateStrokeColor(ICircle outerItem, TNative nativeItem);

        protected abstract void OnUpdateFillColor(ICircle outerItem, TNative nativeItem);

        protected abstract void OnUpdateCenter(ICircle outerItem, TNative nativeItem);

        protected abstract void OnUpdateRadius(ICircle outerItem, TNative nativeItem);

        protected abstract void OnUpdateIsClickable(ICircle outerItem, TNative nativeItem);

        protected abstract void OnUpdateZIndex(ICircle outerItem, TNative nativeItem);
    }
}

