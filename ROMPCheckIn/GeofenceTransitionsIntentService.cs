using System;
using Android.Gms.Common.Apis;
using Android.App;
using Android.Gms.Wearable;
using Android.Gms.Location;
using Android.Util;
using Android.Content;
using Android.Support.V4;
using Java.Util.Concurrent;
using System.Collections.Generic;
using ROMPCheckIn.cms.romponline.com;

namespace ROMPCheckIn
{
	[Service(Exported = false)]
	public class GeofenceTransitionsIntentService : IntentService
	{
		ISharedPreferences mSharedPreferences;

		public GeofenceTransitionsIntentService ()
			:base(typeof(GeofenceTransitionsIntentService).Name)
		{
		}

		public override void OnCreate ()
		{
			base.OnCreate ();
		}

		/// <summary>
		/// Handles incoming intents
		/// </summary>
		/// <param name="intent">The intent sent by Location Services. This Intent is provided to Location Services (inside a PendingIntent)
		/// when AddGeofences() is called</param>
		protected override void OnHandleIntent (Android.Content.Intent intent)
		{	
			// First check for errors
			var geofencingEvent = GeofencingEvent.FromIntent (intent);
			if (geofencingEvent.HasError) {
				int errorCode = geofencingEvent.ErrorCode;
				Log.Error ("ROMPCheckIn", "Location Services error: " + errorCode);
				return;
			} else {
				// Get the type of Geofence transition (i.e. enter or exit in this sample).
				int transitionType = geofencingEvent.GeofenceTransition;
				// Create a DataItem when a user enters one of the geofences. The wearable app will receie this and create a
				// notification to prompt him/her to check in
				mSharedPreferences = this.GetSharedPreferences ("CheckInPrefs", FileCreationMode.Private);
				string sessionKey = mSharedPreferences.GetString("SessionKey", "");
				var locSvc = new ROMPLocation ();
				string result = "";
				int checkintype = -1;
				if (transitionType == Geofence.GeofenceTransitionEnter) {
					IList<IGeofence> triggeredGeofences = geofencingEvent.TriggeringGeofences;
					string triggeredGeofenceId = geofencingEvent.TriggeringGeofences[0].RequestId;
					Android.Locations.Location geoLocation = geofencingEvent.TriggeringLocation;
					result = locSvc.CheckInWithLocation(sessionKey, int.Parse(triggeredGeofenceId), geoLocation.Latitude, geoLocation.Longitude);
					checkintype = 0;
				} else if (transitionType == Geofence.GeofenceTransitionExit) {
					IList<IGeofence> triggeredGeofences = geofencingEvent.TriggeringGeofences;
					string triggeredGeofenceId = geofencingEvent.TriggeringGeofences[0].RequestId;
					result = locSvc.CheckOut (sessionKey, int.Parse(triggeredGeofenceId));
					checkintype = 1;
				}

				Intent notificationIntent = new Intent(this, typeof(CheckInPassiveActivity));
				//notificationIntent.SetFlags (ActivityFlags.SingleTop);
				TaskStackBuilder stackBldr = TaskStackBuilder.Create (this);
				stackBldr.AddParentStack (Java.Lang.Class.FromType(typeof(CheckInPassiveActivity)));
				stackBldr.AddNextIntent (notificationIntent);

				PendingIntent notificationPendingIntent = stackBldr.GetPendingIntent (0, PendingIntentFlags.UpdateCurrent);

				if (result == "Fail" && checkintype == 0) {
					Notification.Builder notBuilder = new Notification.Builder (this)
						.SetSmallIcon (Android.Resource.Drawable.IcDialogAlert)
						.SetContentTitle ("Check In Failed")
						.SetContentText ("You Failed To Check In To Your Location")
						.SetContentIntent (notificationPendingIntent)
						.SetAutoCancel (true);
					NotificationManager notificationManager = (NotificationManager)GetSystemService (NotificationService);
					notificationManager.Notify (666, notBuilder.Build ());
				} else if (result == "Fail" && checkintype == 1) {
					Notification.Builder notBuilder = new Notification.Builder (this)
						.SetSmallIcon (Android.Resource.Drawable.IcDialogAlert)
						.SetContentTitle ("Check Out Failed")
						.SetContentText ("You Failed To Check Out Of Your Location")
						.SetContentIntent (notificationPendingIntent)
						.SetAutoCancel (true);
					NotificationManager notificationManager = (NotificationManager)GetSystemService (NotificationService);
					notificationManager.Notify (666, notBuilder.Build ());
				} else if (result == "Success" && checkintype == 0) {
					Notification.Builder notBuilder = new Notification.Builder (this)
						.SetSmallIcon (Android.Resource.Drawable.IcDialogAlert)
						.SetContentTitle ("Check In Succeeded")
						.SetContentText ("You Successfully Checked In To Your Location")
						.SetContentIntent (notificationPendingIntent)
						.SetAutoCancel (true);
					NotificationManager notificationManager = (NotificationManager)GetSystemService (NotificationService);
					notificationManager.Notify (666, notBuilder.Build ());
				} else if (result == "Success" && checkintype == 1) {
					Notification.Builder notBuilder = new Notification.Builder (this)
						.SetSmallIcon (Android.Resource.Drawable.IcDialogAlert)
						.SetContentTitle ("Check Out Succeeded")
						.SetContentText ("You Successfully Checked Out Of Your Location")
						.SetContentIntent (notificationPendingIntent)
						.SetAutoCancel (true);
					NotificationManager notificationManager = (NotificationManager)GetSystemService (NotificationService);
					notificationManager.Notify (666, notBuilder.Build ());
				} else {
					Notification.Builder notBuilder = new Notification.Builder (this)
						.SetSmallIcon (Android.Resource.Drawable.IcDialogAlert)
						.SetContentTitle ("Service Failed")
						.SetContentText ("Check In/Check Out Unavailable")
						.SetContentIntent (notificationPendingIntent)
						.SetAutoCancel (true);
					NotificationManager notificationManager = (NotificationManager)GetSystemService (NotificationService);
					notificationManager.Notify (666, notBuilder.Build ());
				}
			}
		}

		public void OnConnected (Android.OS.Bundle connectionHint)
		{

		}

		public void OnConnectionSuspended (int cause)
		{

		}

		public void OnConnectionFailed (Android.Gms.Common.ConnectionResult result)
		{

		}
	}
}
