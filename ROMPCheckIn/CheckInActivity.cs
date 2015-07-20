
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

namespace ROMPCheckIn
{
	[Activity (Label = "CheckInActivity")]			
	public class CheckInActivity : Activity, IGoogleApiClientConnectionCallbacks, IGoogleApiClientOnConnectionFailedListener
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

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			mGeofenceStorage = new ROMPGeofenceStore (this);
			geofenceList = new List<IGeofence> ();
			mInProgress = false;


			// Create your application here
		}



		public void OnConnectionFailed(Android.Gms.Common.ConnectionResult result)
		{
			mInProgress = false;

			if (result.HasResolution) {
				try {
					result.StartResolutionForResult (this, 1);
				} catch (Exception ex) {
					Log.Error ("ERROR", "Exception while resolving connection error.", ex);
				}
			} else {
				int errorCode = result.ErrorCode;
				Log.Error ("ERROR", "Connection to Google Play Services Failed with Code " + errorCode);
			}
		}

		public void OnConnectionSuspended(int i)
		{
		}

		public void OnConnected(Bundle connectionHint)
		{
			if (mRequestType == RequestType.Add) {
				geofenceRequestIntent = GeofenceTransitionPendingIntent;
				LocationServices.GeofencingApi.AddGeofences (apiClient, geofenceList, geofenceRequestIntent);
			}
		}

		public void OnDisconnected()
		{
			mInProgress = false;
			apiClient = null;
		}
	}
}

