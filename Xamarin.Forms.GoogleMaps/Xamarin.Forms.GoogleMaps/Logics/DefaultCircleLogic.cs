using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Xamarin.Forms.GoogleMaps.Logics
{
    internal abstract class DefaultCircleLogic<TNative, TNativeMap> : DefaultLogic<Circle, TNative, TNativeMap>
        where TNative : class
        where TNativeMap : class
    {
        protected override void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnItemPropertyChanged(sender, e);
            var outerItem = sender as Circle;
            var nativeItem = outerItem?.NativeObject as TNative;

            if (nativeItem == null)
                return;

            if (e.PropertyName == nameof(Circle.StrokeWidth)) OnUpdateStrokeWidth(outerItem, nativeItem);
            else if (e.PropertyName == nameof(Circle.StrokeColor)) OnUpdateStrokeColor(outerItem, nativeItem);
            else if (e.PropertyName == nameof(Circle.FillColor)) OnUpdateFillColor(outerItem, nativeItem);
            else if (e.PropertyName == nameof(Circle.Center)) OnUpdateCenter(outerItem, nativeItem);
            else if (e.PropertyName == nameof(Circle.Radius)) OnUpdateRadius(outerItem, nativeItem);
        }

        protected abstract void OnUpdateStrokeWidth(Circle outerItem, TNative nativeItem);

        protected abstract void OnUpdateStrokeColor(Circle outerItem, TNative nativeItem);

        protected abstract void OnUpdateFillColor(Circle outerItem, TNative nativeItem);

        protected abstract void OnUpdateCenter(Circle outerItem, TNative nativeItem);

        protected abstract void OnUpdateRadius(Circle outerItem, TNative nativeItem);
    }
}

