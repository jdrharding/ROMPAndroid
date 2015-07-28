
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security;
using ROMPCheckIn.cms.romponline.com;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Gms.Location;
using Android.Util;
using Android.Gms.Common.Apis;
using Android.Gms.Common;
using Android.Support.V7.App;

namespace ROMPCheckIn
{
	[Activity (Label = "CheckInPassiveActivity")]			
	public class CheckInPassiveActivity : ActionBarActivity, IGoogleApiClientConnectionCallbacks, IGoogleApiClientOnConnectionFailedListener, ResultCallback<Statuses>
	{
		IGoogleApiClient apiClient;
		List<IGeofence> geofenceList;
		bool mGeofencesAdded;
		PendingIntent geofenceRequestIntent;
		ISharedPreferences mSharedPreferences;

		enum RequestType { Add }
		RequestType mRequestType;

		bool mInProgress;

		protected override void OnCreate (Bundle bundle)
		{
			RequestWindowFeature(WindowFeatures.NoTitle);
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.CheckIn);
			geofenceList = new List<IGeofence> ();
			geofenceRequestIntent = null;
			mGeofencesAdded = false;
			mSharedPreferences = this.GetSharedPreferences ("CheckInPrefs", FileCreationMode.Private);
			string sessionKey = Intent.GetStringExtra ("SessionKey");
			int groupID = Intent.GetIntExtra ("GroupID", 0);
			int userID = Intent.GetIntExtra ("UserID", 0);
			ISharedPreferencesEditor editor = mSharedPreferences.Edit ();
			editor.PutInt ("GroupID", groupID);
			editor.PutInt ("UserID", userID);
			editor.PutString ("SessionKey", sessionKey);
			CreateGeofences ();

			BuildGoogleApiClient ();

			// Create your application here
		}

		public void BuildGoogleApiClient() {
			apiClient = new GoogleApiClientBuilder ()
				.AddConnectionCallbacks (this)
				.AddOnConnectionFailedListener (this)
				.AddApi (LocationServices.API)
				.Build ();
		}

		protected override void OnStart ()
		{
			base.OnStart ();
			apiClient.Connect ();
		}

		protected override void OnStop()
		{
			base.OnStop ();
			apiClient.Disconnect ();
		}

		public void OnConnected(Bundle connectionHint)
		{
			Log.Debug ("ROMPCheckIn", "Connected to Google API Client");
		}

		public void OnConnectionFailed(Android.Gms.Common.ConnectionResult result)
		{			
			Log.Error ("ERROR", "Connection to Google Play Services Failed with Code " + errorCode);
		}

		public void OnConnectionSuspended(int i)
		{
			Log.Error ("ERROR", "Connection Suspended");
		}

		private GeofencingRequest getGeofencingRequest () {
			GeofencingRequest.Builder builder = new GeofencingRequest.Builder ();
			builder.SetInitialTrigger (GeofencingRequest.InitialTriggerEnter);
			builder.AddGeofences (geofenceList);
			return builder.Build ();
		}

		private void logSecurityException(SecurityException securityEx) {
			Log.Error ("ROMPCheckIn", "Invalid location permissions. ACCESS_FINE_LOCATION required", securityEx);
		}

		public void OnResult(Statuses status) {
			if (status.IsSuccess) {
				var myHandler = new Handler ();
				myHandler.Post (() => {
					Android.Widget.Toast.MakeText (this, "Geofencing Started.", Android.Widget.ToastLength.Long).Show ();
				});
			} else {
				var myHandler = new Handler ();
				myHandler.Post (() => {
					Android.Widget.Toast.MakeText (this, "Error Starting Geofencing" +
						".", Android.Widget.ToastLength.Long).Show ();
				});
			}
		}

		private PendingIntent getGeofencePendingIntent() {
			if (geofenceRequestIntent != null) {
				return geofenceRequestIntent;
			}
			Intent intent = new Intent (this, GeofenceTransitionPendingIntent.Class);
			return PendingIntent.GetService (this, 0, intent, PendingIntentFlags.UpdateCurrent);
		}

		bool IsGooglePlayServicesAvailable
		{
			get {
				int resultCode = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable (this);
				if (resultCode == ConnectionResult.Success) {
					if (Log.IsLoggable ("Tag", LogPriority.Debug)) {
						Log.Debug ("Tag", "Google Play services is available");
					}
					return true;
				} else {
					Log.Error ("Tag", "Google Play services is unavailable");
					return false;
				}
			}
		}

		PendingIntent GeofenceTransitionPendingIntent {
			get {
				var intent = new Intent (this, typeof(GeofenceTransitionsIntentService));
				return PendingIntent.GetService (this, 0, intent, PendingIntentFlags.UpdateCurrent);
			}
		}

		public void CreateGeofences ()
		{

			string sessionKey = Intent.GetStringExtra ("SessionKey");
			int groupID = Intent.GetIntExtra ("GroupID", 0);
			int userID = Intent.GetIntExtra ("UserID", 0);
			var locSvc = new ROMPLocation ();
			FacilityCoordinates[] myFacilities = locSvc.GetLocations (sessionKey, groupID);
			foreach (FacilityCoordinates fc in myFacilities) {
				geofenceList.Add(new GeofenceBuilder ()
					.SetRequestId (fc.LocationID)
					.SetCircularRegion (fc.Latitude, fc.Longitude, 500)
					.SetExpirationDuration (Geofence.NeverExpire)
					.SetTransitionTypes (Geofence.GeofenceTransitionEnter | Geofence.GeofenceTransitionExit)
					.Build ());
			}
			try {
				LocationServices.GeofencingApi.AddGeofences(apiClient, getGeofencingRequest(), getGeofencePendingIntent()).SetResultCallback(this);
			} catch (SecurityException securityEx) {
				logSecurityException (securityEx);
			}
		}


		public void StartGeofencing() {
			if (!apiClient.IsConnected) {
				var myHandler = new Handler();
				myHandler.Post(() => {
					Android.Widget.Toast.MakeText(this, "Not Connected to Google Play Services.", Android.Widget.ToastLength.Long).Show();
				});
			}

			try 
			{
				LocationServices.GeofencingApi.AddGeofences(apiClient, getGeofencingRequest(), getGeofencePendingIntent()).SetResultCallback(this);

			} catch (Exception e) {
				var myHandler = new Handler();
				myHandler.Post(() => {
					Android.Widget.Toast.MakeText(this, e.Message, Android.Widget.ToastLength.Long).Show();
				});
			}
		}

		public void StopGeofencing() {
			if (!apiClient.IsConnected) {
				var myHandler = new Handler();
				myHandler.Post(() => {
					Android.Widget.Toast.MakeText(this, "Not Connected to Google Play Services.", Android.Widget.ToastLength.Long).Show();
				});
			}

			try 
			{
				LocationServices.GeofencingApi.RemoveGeofences(apiClient, getGeofencingRequest(), getGeofencePendingIntent()).SetResultCallback(this);

			} catch (Exception e) {
				var myHandler = new Handler();
				myHandler.Post(() => {
					Android.Widget.Toast.MakeText(this, e.Message, Android.Widget.ToastLength.Long).Show();
				});
			}
		}


		public void OnDisconnected()
		{
			mInProgress = false;
			apiClient = null;
		}

		public override void OnBackPressed() {
			var builder = new Android.App.AlertDialog.Builder(this);
			builder.SetTitle ("Exit.");
			builder.SetIcon (Android.Resource.Drawable.IcDialogAlert);
			builder.SetMessage("Exit App?");
			builder.SetPositiveButton("OK", (s, e) => { base.OnStop; });
			builder.SetNegativeButton("Cancel", (s, e) => { });
			builder.Create().Show();
		}


		protected override void OnDestroy()
		{
			base.OnDestroy ();
		}

		protected override void OnResume()
		{
			base.OnResume();
		}

		protected override void OnPause()
		{
			base.OnPause();
		}
	}
}

