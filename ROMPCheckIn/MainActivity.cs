using System;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;
using System.Diagnostics;
using ROMPCheckIn.cms.romponline.com;

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
	[Activity (Label = "ROMP Check-In", MainLauncher = true, Icon = "@drawable/icon", LaunchMode = Android.Content.PM.LaunchMode.SingleInstance)]
	public class MainActivity : Activity
	{
		ISharedPreferences mSharedPreferences;
		protected override void OnCreate (Bundle bundle)
		{
			RequestWindowFeature(WindowFeatures.NoTitle);
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.Main);
			mSharedPreferences = this.GetSharedPreferences ("CheckInPrefs", FileCreationMode.Private);
			string storedUser = mSharedPreferences.GetString ("StoredUser", "NONE");
			string storedPass = mSharedPreferences.GetString ("StoredPass", "NONE");
			if (!storedUser.Equals("NONE") || !storedPass.Equals("NONE")) {
				FindViewById<EditText>(Resource.Id.txtUsername).SetText(storedUser, TextView.BufferType.Normal);
				FindViewById<EditText>(Resource.Id.txtPassword).SetText(storedPass, TextView.BufferType.Normal);
				FindViewById<CheckBox>(Resource.Id.cbStoreUser).Checked = true;
			}
			FindViewById<CheckBox>(Resource.Id.cbStoreUser).CheckedChange += delegate {
				ISharedPreferencesEditor cbeditor = mSharedPreferences.Edit ();
				cbeditor.Remove("StoredUser");
				cbeditor.Remove("StoredPass");
				cbeditor.Commit();
			};
			Button button = FindViewById<Button> (Resource.Id.btnLogin);
			button.Click += delegate {
				TextView txtPassword = FindViewById<TextView> (Resource.Id.txtPassword);
				TextView txtUsername = FindViewById<TextView> (Resource.Id.txtUsername);
				if (string.IsNullOrEmpty(txtPassword.Text) || string.IsNullOrEmpty(txtUsername.Text)) {
					var myHandler = new Handler();
					myHandler.Post(() => {
						Android.Widget.Toast.MakeText(this, "Please Provide a Username and Password.", Android.Widget.ToastLength.Long).Show();
					});
				} else {
					try {						
						var locSvc = new ROMPLocation();
						var loginResp = new LoginResponse();
						loginResp = locSvc.LearnerLogin(txtUsername.Text, txtPassword.Text);
						if (loginResp.Success) {
							CheckBox rememberMe = FindViewById<CheckBox> (Resource.Id.cbStoreUser);
							ISharedPreferencesEditor editor = mSharedPreferences.Edit ();
							if (rememberMe.Checked) {
								editor.PutString("StoredUser", txtUsername.Text);
								editor.PutString("StoredPass", txtPassword.Text);
							} else {
								editor.Remove("StoredUser");
							}
							editor.Commit();
							if (loginResp.GroupID <= 2) {
								var nextActivity = new Intent(this, typeof(ChooseModeActivity));
								nextActivity.PutExtra("SessionKey", loginResp.SessionKey);
								nextActivity.PutExtra("GroupID", loginResp.GroupID);
								nextActivity.PutExtra("UserID", loginResp.UserID);
								StartActivity(nextActivity);
								Finish();
							} else {
								var nextActivity = new Intent(this, typeof(CheckInActivity));
								nextActivity.PutExtra("SessionKey", loginResp.SessionKey);
								nextActivity.PutExtra("GroupID", loginResp.GroupID);
								nextActivity.PutExtra("UserID", loginResp.UserID);
								StartActivity(nextActivity);
								Finish();
							}
						} else {
							var myHandler = new Handler();
							myHandler.Post(() => {
								Android.Widget.Toast.MakeText(this, "Login Failed. Please Try Again.", Android.Widget.ToastLength.Long).Show();
							});
						}
					} catch (Exception e) {
						var myHandler = new Handler();
						myHandler.Post(() => {
							Android.Widget.Toast.MakeText(this, e.Message, Android.Widget.ToastLength.Long).Show();
						});
						System.Diagnostics.Debug.Write(e.Message);
					}
				}
			};

		}

		public override void OnBackPressed() {
			var builder = new Android.App.AlertDialog.Builder(this);
			builder.SetTitle ("Exit.");
			builder.SetIcon (Android.Resource.Drawable.IcDialogAlert);
			builder.SetMessage("Exit App?");
			builder.SetPositiveButton("OK", (s, e) => 
				{ 
					System.Environment.Exit(0); 
				});
			builder.SetNegativeButton("Cancel", (s, e) => { });
			builder.Create().Show();
		}
	}
}


