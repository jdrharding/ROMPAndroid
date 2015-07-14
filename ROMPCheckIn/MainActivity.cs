using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Collections.Generic;
using Android.Gms.Location;
using Android.Util;
using Android.Gms.Common.Apis;
using Android.Gms.Common;

namespace ROMPCheckIn
{
	[Activity (Label = "ROMPCheckIn", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : Activity, IGoogleApiClientConnectionCallbacks, IGoogleApiClientOnConnectionFailedListener
	{
		List<IGeofence> geofenceList;


		// Persistent storage for geofences
		ROMPGeofenceStore mGeofenceStorage;

		IGoogleApiClient apiClient;
		// Stores the PendingIntent used to request geofence monitoring
		PendingIntent geofenceRequestIntent;

		// Defines the allowable request types (in this example, we only add geofences)
		enum RequestType { Add }
		RequestType mRequestType;
		// Flag that indicates if a request is underway
		bool mInProgress;

		bool IsGooglePlayServicesAvailable
		{
			get {
				int resultCode = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable (this);
				if (resultCode == ConnectionResult.Success) {
					/*if (Log.IsLoggable (Constants.TAG, LogPriority.Debug)) {
						Log.Debug (Constants.TAG, "Google Play services is available");
					}*/
					return true;
				} else {
					//Log.Error (Constants.TAG, "Google Play services is unavailable");
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

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			// Get our button from the layout resource,
			// and attach an event to it
			Button button = FindViewById<Button> (Resource.Id.myButton);

		}
	}
}


