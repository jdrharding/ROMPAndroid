
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
			RequestWindowFeature(WindowFeatures.NoTitle);
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.ChooseMode);
			try {
				string sessionKey = Intent.GetStringExtra ("SessionKey");
				int groupID = Intent.GetIntExtra ("GroupID", 0);
				int userID = Intent.GetIntExtra ("UserID", 0);
				var locSvc = new ROMPLocation ();
				Button button = FindViewById<Button> (Resource.Id.btnChoose);
				CheckBox cbpassive = FindViewById<CheckBox> (Resource.Id.cbPassive);
				CheckBox cbactive = FindViewById<CheckBox> (Resource.Id.cbActive);

				cbpassive.Click += (o, e) => {
					if (cbactive.Checked) {
						cbactive.Checked = false;
					} 
				};

				cbactive.Click += (o, e) => {
					if (cbpassive.Checked) {
						cbpassive.Checked = false;
					} 
				};

				button.Click += delegate {
					if (cbactive.Checked) {
						var nextActivity = new Intent(this, typeof(CheckInActivity));
						nextActivity.PutExtra("SessionKey", sessionKey);
						nextActivity.PutExtra("GroupID", groupID);
						nextActivity.PutExtra("UserID", userID);
						StartActivity(nextActivity);
						Finish();
					} else if (cbpassive.Checked) {
						var nextActivity = new Intent(this, typeof(CheckInPassiveActivity));
						nextActivity.PutExtra("SessionKey", sessionKey);
						nextActivity.PutExtra("GroupID", groupID);
						nextActivity.PutExtra("UserID", userID);
						StartActivity(nextActivity);
						Finish();
					} else {
						string errmsg = "Please choose a check in method before proceeding.";
						var myHandler = new Handler();
						myHandler.Post(() => {
							Toast.MakeText(this, errmsg, ToastLength.Short).Show();
						});
					}
				};
			} catch (Exception e) {
				var myHandler = new Handler();
				myHandler.Post(() => {
					Android.Widget.Toast.MakeText(this, e.Message, Android.Widget.ToastLength.Long).Show();
				});
				System.Diagnostics.Debug.Write (e.Message);
			}
		}


		public override void OnBackPressed() {
			var builder = new Android.App.AlertDialog.Builder(this);
			builder.SetTitle ("Exit.");
			builder.SetIcon (Android.Resource.Drawable.IcDialogAlert);
			builder.SetMessage("Exit App?");
			builder.SetPositiveButton("OK", (s, e) => { Finish(); });
			builder.SetNegativeButton("Cancel", (s, e) => { });
			builder.Create().Show();
		}
	}
}

