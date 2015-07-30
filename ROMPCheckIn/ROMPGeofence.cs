using System;
using Android.Gms.Location;
using Android.Content;

namespace ROMPCheckIn
{
	public class ROMPGeofence
	{
		// Instance variables
		readonly string mId;
		readonly double mLatitude;
		readonly double mLongitude;

		// Instance field properies
		public string Id { get { return mId; } }
		public double Latitude { get { return mLatitude; } }
		public double Longitude { get { return mLongitude; } }

		/// <summary>
		/// Initializes a new instance of the <see cref="Geofencing.SimpleGeofence"/> class.
		/// </summary>
		/// <param name="geofenceId">The Geofence's request ID</param>
		/// <param name="latitude">Latitude of the Geofence's center in degrees</param>
		/// <param name="longitude">Longitude of the Geofence's center in degrees</param>
		public ROMPGeofence (String geofenceId, double latitude, double longitude)
		{
			// Set the instance fields from the constructor.
			this.mId = geofenceId;
			this.mLatitude = latitude;
			this.mLongitude = longitude;
		}
	}
}

