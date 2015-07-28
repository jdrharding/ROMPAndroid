
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
		int groupID;
		string sessionKey;
		int userID;

		protected override void OnCreate (Bundle bundle)
		{
			RequestWindowFeature(WindowFeatures.NoTitle);
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.CheckIn);
			FindViewById<TextView> (Resource.Id.btnCheckIn).Enabled = false;
			sessionKey = Intent.GetStringExtra ("SessionKey");
			groupID = Intent.GetIntExtra ("GroupID", 0);
			userID = Intent.GetIntExtra ("UserID", 0);
			var locSvc = new ROMPLocation ();
			//myFacilities[] = new FacilityCoordinates();
			myFacilities = locSvc.GetLocations (sessionKey, groupID);
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
			FindViewById<TextView> (Resource.Id.btnCheckIn).Enabled = true;
		}

		public void OnLocationChanged(Location location) {
			_currentLocation = location;
		}

		public void btnCheckIn_OnClick(object sender, EventArgs eventArgs)
		{
			try 
			{
				if (_currentLocation == null) {
					var myHandler = new Handler ();
					myHandler.Post (() => {
						Toast.MakeText (this, "Determining Location, Wait One Moment and Try Again.", ToastLength.Long).Show ();
					});
					return;
				} else {
					if (groupID <= 2) {
						string absResult = "You Are Not Within A Specified Zone.";
						Button button = FindViewById<Button>(Resource.Id.btnCheckIn);
						if (button.Text == "Check In") {
							foreach (FacilityCoordinates fc in myFacilities) {
								var fenceLat = fc.Latitude;
								var fenceLon = fc.Longitude;
								var R = 6371; // Radius of the earth in km
								var dLat = deg2rad(_currentLocation.Latitude - fenceLat);  // deg2rad below
								var dLon = deg2rad(_currentLocation.Longitude - fenceLon); 
								var a = 
									Math.Sin(dLat/2) * Math.Sin(dLat/2) +
									Math.Cos(deg2rad(fenceLat)) * Math.Cos(deg2rad(_currentLocation.Latitude)) * 
									Math.Sin(dLon/2) * Math.Sin(dLon/2)
									; 
								var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1-a)); 
								var d = R * c;
								if (d <= 1.0) {
									var locSvc = new ROMPLocation ();
									string result = locSvc.CheckIn(sessionKey, fc.LocationID);
									if (result == "Success"){
										absResult = "Check In Successful";
										button.Text = "Check Out";
									} else {
										absResult = "An Unexpected Error Occurred. Try Again";
									}
								}
							}
						} else if (button.Text == "Check Out") {
							var locSvc = new ROMPLocation ();
							string result = locSvc.CheckOutWithoutLocation(sessionKey);
							if (result == "Success"){
								absResult = "Check In Successful";
								button.Text = "Check In";
							} else {
								absResult = "An Unexpected Error Occurred. Try Again";
							}
						}
						var myHandler = new Handler ();
						myHandler.Post (() => {
							Toast.MakeText (this, absResult, ToastLength.Long).Show ();
						});
					} else if (groupID > 2 && groupID <= 7) {
						string absResult;
						Button button = FindViewById<Button>(Resource.Id.btnCheckIn);
						if (button.Text == "Check In") {
							var locSvc = new ROMPLocation ();
							string result = locSvc.CheckInWithLocation(sessionKey, -1, _currentLocation.Latitude, _currentLocation.Longitude);
							if (result == "Success"){
								absResult = "Check In Successful";
								button.Text = "Check Out";
							} else {
								absResult = "An Unexpected Error Occurred. Try Again";
							}
							var myHandler = new Handler ();
							myHandler.Post (() => {
								Toast.MakeText (this, absResult, ToastLength.Long).Show ();
							});
						} else if (button.Text == "Check Out") {
							var locSvc = new ROMPLocation ();
							string result = locSvc.CheckOutWithoutLocation(sessionKey);
							if (result == "Success"){
								absResult = "Check In Successful";
								button.Text = "Check In";
							} else {
								absResult = "An Unexpected Error Occurred. Try Again";
							}
							var myHandler = new Handler ();
							myHandler.Post (() => {
								Toast.MakeText (this, absResult, ToastLength.Long).Show ();
							});
						}
					} else if (groupID == 8) {
						var fenceLat = 48.46003187;
						var fenceLon = -89.18908003;
						var R = 6371; // Radius of the earth in km
						var dLat = deg2rad(_currentLocation.Latitude - fenceLat);  // deg2rad below
						var dLon = deg2rad(_currentLocation.Longitude - fenceLon); 
						var a = 
							Math.Sin(dLat/2) * Math.Sin(dLat/2) +
							Math.Cos(deg2rad(fenceLat)) * Math.Cos(deg2rad(_currentLocation.Latitude)) * 
							Math.Sin(dLon/2) * Math.Sin(dLon/2)
							; 
						var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1-a)); 
						var d = R * c;
						//double distance = 60 * 1.1515 * Math.Acos(Math.Sin(Math.PI * _currentLocation.Latitude / 180) * Math.Sin(Math.PI * 48.46003187 / 180) + Math.Cos(Math.PI * _currentLocation.Latitude / 180) * Math.Cos(Math.PI * 48.46003187 / 180) * Math.Cos((_currentLocation.Longitude - -89.18908003) * Math.PI / 180)  * 180 / Math.PI );
						var myHandler = new Handler ();
						string absResult = d.ToString();
						myHandler.Post (() => {
							Toast.MakeText (this, absResult, ToastLength.Long).Show ();
						});
					}
				}
			} catch (Exception e) {
				var myHandler = new Handler();
				myHandler.Post(() => {
					Android.Widget.Toast.MakeText(this, e.Message, Android.Widget.ToastLength.Long).Show();
				});
			}
		}

		private double deg2rad(double deg) {
			return deg * (Math.PI / 180);
		}

		public void OnProviderDisabled(string provider) {}

		public void OnProviderEnabled(string provider) {}

		public void OnStatusChanged(string provider, Availability status, Bundle extras) {}

		public override void OnBackPressed() {
			var builder = new Android.App.AlertDialog.Builder(this);
			builder.SetTitle ("Exit.");
			builder.SetIcon (Android.Resource.Drawable.IcDialogAlert);
			builder.SetMessage("Exit App?");
			builder.SetPositiveButton("OK", (s, e) => { OnStop(); });
			builder.SetNegativeButton("Cancel", (s, e) => { });
			builder.Create().Show();
		}

		protected override void OnStop()
		{
			base.OnStop ();
		}

		protected override void OnDestroy()
		{
			base.OnDestroy ();
		}

		protected override void OnResume()
		{
			base.OnResume();
			FindViewById<TextView> (Resource.Id.btnCheckIn).Enabled = false;
			_locationManager.RequestLocationUpdates(_locationProvider, 0, 0, this);
			FindViewById<TextView> (Resource.Id.btnCheckIn).Enabled = true;
		}

		protected override void OnPause()
		{
			base.OnPause();
			_locationManager.RemoveUpdates(this);
		}
	}
}

