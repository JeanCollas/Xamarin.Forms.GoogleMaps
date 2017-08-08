using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.V7.App;
using static Android.Gms.Maps.GoogleMap;
using Gms=Android.Gms;
using Xamarin.Forms.GoogleMaps.Android.Extensions;
using Android.Support.V4.App;
using Android.Gms.Maps;
using Android.Content.PM;
using Android.Support.V4.Content;
using Android;

namespace Xamarin.Forms.GoogleMaps.Android.Services
{
    [Activity(Label = "MyLocationActivity")]
    public class MyLocationActivity : AppCompatActivity,
        IOnMyLocationButtonClickListener,
        IOnMapReadyCallback,
        ActivityCompat.IOnRequestPermissionsResultCallback
    {

        /**
         * Request code for location permission request.
         *
         * @see #onRequestPermissionsResult(int, String[], int[])
         */
        private static int LOCATION_PERMISSION_REQUEST_CODE = 1;

        /**
         * Flag indicating whether a requested permission has been denied after returning in
         * {@link #onRequestPermissionsResult(int, String[], int[])}.
         */
        private bool mPermissionDenied = false;

        private GoogleMap mMap;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(R.layout.my_location_demo);

            SupportMapFragment mapFragment =
                    (SupportMapFragment)GetSupportFragmentManager().findFragmentById(R.id.map);
            mapFragment.GetMapAsync(this);
        }

        public void OnMapReady(GoogleMap map)
        {
            mMap = map;

            mMap.SetOnMyLocationButtonClickListener(this);
            EnableMyLocation();
        }

        /**
         * Enables the My Location layer if the fine location permission has been granted.
         */
        private void EnableMyLocation()
        {
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation)
                    != Permission.Granted)
            {
                // Permission to access the location is missing.
                //                PermissionUtils;

                RequestPermissions(new[] { Manifest.Permission.AccessFineLocation }, LOCATION_PERMISSION_REQUEST_CODE);
            }
            else if (mMap != null)
            {
                // Access to the location has been granted to the app.
                mMap.MyLocationEnabled=true;
            }
        }

        public bool OnMyLocationButtonClick()
        {
            Toast.MakeText(this, "MyLocation button clicked", ToastLength.Short).Show();
            
            // Return false so that we don't consume the event and the default behavior still occurs
            // (the camera animates to the user's current position).
            return false;
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            
            if (requestCode != LOCATION_PERMISSION_REQUEST_CODE)
            {
                return;
            }

            if (PermissionUtils.IsPermissionGranted(permissions, grantResults,
                    Manifest.permission.ACCESS_FINE_LOCATION))
            {
                // Enable the my location layer if the permission has been granted.
                EnableMyLocation();
            }
            else
            {
                // Display the missing permission error dialog when the fragments resume.
                mPermissionDenied = true;
            }
        }
        protected override void OnResumeFragments()
        {
            base.OnResumeFragments();
            if (mPermissionDenied)
            {
                // Permission was not granted, display error dialog.
                showMissingPermissionError();
                mPermissionDenied = false;
            }
        }

        /**
         * Displays a dialog with error message explaining that the location permission is missing.
         */
        private void ShowMissingPermissionError()
        {
            SupportMapFragment smf = new SupportMapFragment();
            PermissionUtils.PermissionDeniedDialog
                    .newInstance(true).show(getSupportFragmentManager(), "dialog");
        }

    }

}