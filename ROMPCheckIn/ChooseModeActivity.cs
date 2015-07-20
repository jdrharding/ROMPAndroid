
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ROMPCheckIn.cms.romponline.com;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace ROMPCheckIn
{
	[Activity (Label = "ChooseModeActivity")]			
	public class ChooseModeActivity : Activity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			string sessionKey = Intent.GetStringExtra ("SessionKey");
			var locSvc = new ROMPLocation ();
			Button button = FindViewById<Button> (Resource.Id.btnChoose);
			CheckBox cbpassive = FindViewById<CheckBox> (Resource.Id.cbPassive);
			CheckBox cbactive = FindViewById<CheckBox> (Resource.Id.cbActive);

			cbpassive.Click += delegate {
				if (cbpassive.Selected) {
					cbpassive.Selected = false;
				} else {
					cbpassive.Selected = true;
				}

				if (cbactive.Selected) {
					cbactive.Selected = false;
				} 
			};

			cbactive.Click += delegate {
				if (cbactive.Selected) {
					cbactive.Selected = false;
				} else {
					cbactive.Selected = true;
				}

				if (cbpassive.Selected) {
					cbpassive.Selected = false;
				} 
			};

			button.Click += delegate {
				if (cbactive.Selected) {
					var nextActivity = new Intent(this, typeof(CheckInActivity));
					nextActivity.PutExtra("SessionKey", sessionKey);
					StartActivity(nextActivity);
					Finish();
				} else if (cbpassive.Selected) {
					var nextActivity = new Intent(this, typeof(CheckInPassiveActivity));
					nextActivity.PutExtra("SessionKey", sessionKey);
					StartActivity(nextActivity);
					Finish();
				} else {
					string errmsg = "Please choose a check in method before proceeding.";
					Toast.MakeText(this, errmsg, ToastLength.Short);
				}
			};

			// Create your application here
		}
	}
}

