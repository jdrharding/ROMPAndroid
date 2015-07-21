
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ROMPCheckIn.cms.romponline.com;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Util;
using Android.Gms.Common.Apis;
using Android.Gms.Common;
using Android.Locations;


namespace ROMPCheckIn
{
	[Activity (Label = "CheckInActivity")]			
	public class CheckInActivity : Activity, ILocationListener
	{

		Location _currentLocation;
		LocationManager _locationManager;
		String _locationProvider;
		FacilityCoordinates[] myFacilities;

		protected override void OnCreate (Bundle bundle)
		{
			RequestWindowFeature(WindowFeatures.NoTitle);
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.CheckIn);
			string sessionKey = Intent.GetStringExtra ("SessionKey");
			var locSvc = new ROMPLocation ();
			//myFacilities[] = new FacilityCoordinates();
			myFacilities = locSvc.GetLocations (sessionKey);
			FindViewById<TextView>(Resource.Id.btnCheckIn).Click += btnCheckIn_OnClick;

			InitializeLocationManager();
		}

		void InitializeLocationManager()
		{
			_locationManager = (LocationManager)GetSystemService(LocationService);
			Criteria criteriaForLocationService = new Criteria
			{
				Accuracy = Accuracy.Fine
			};
			IList<string> acceptableLocationProviders = _locationManager.GetProviders(criteriaForLocationService, true);

			if (acceptableLocationProviders.Any())
			{
				_locationProvider = acceptableLocationProviders.First();
			}
			else
			{
				_locationProvider = String.Empty;
			}
		}

		public void OnLocationChanged(Location location) {
			_currentLocation = location;
		}

		async void btnCheckIn_OnClick(object sender, EventArgs eventArgs)
		{
			if (_currentLocation == null) {
				var myHandler = new Handler ();
				myHandler.Post (() => {
					Toast.MakeText (this, "Can't determine the current location.", ToastLength.Long).Show ();
				});
				return;
			} else {
				string sessionKey = Intent.GetStringExtra ("SessionKey");
				string absResult = "You Are Not Within A Specified Zone.";
				foreach (FacilityCoordinates fc in myFacilities) {
					double distance = 60* 1.1515 * Math.Acos(Math.Sin(Math.PI * _currentLocation.Latitude / 180) * Math.Sin(Math.PI * fc.Latitude / 180) + 
						Math.Cos(Math.PI * _currentLocation.Latitude / 180) * Math.Cos(Math.PI * fc.Latitude / 180) * Math.Cos((_currentLocation.Longitude - fc.Longitude) * Math.PI / 180)  * 180 / Math.PI );
					if (distance <= 1.1) {
						var locSvc = new ROMPLocation ();
						string result = locSvc.CheckIn(sessionKey, fc.LocationID, 0);
						if (result == "Success"){
							absResult = "Check In Successful";
						} else {
							absResult = "An Unexpected Error Occurred. Try Again";
						}
					}
				}
				var myHandler = new Handler ();
				myHandler.Post (() => {
					Toast.MakeText (this, absResult, ToastLength.Long).Show ();
				});
				//locSvc.CheckIn(sessionKey,
			}
		}

		public void OnProviderDisabled(string provider) {}

		public void OnProviderEnabled(string provider) {}

		public void OnStatusChanged(string provider, Availability status, Bundle extras) {}

		protected override void OnResume()
		{
			base.OnResume();
			_locationManager.RequestLocationUpdates(_locationProvider, 0, 0, this);
		}

		protected override void OnPause()
		{
			base.OnPause();
			_locationManager.RemoveUpdates(this);
		}
	}
}

