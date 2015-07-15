using System;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;
using System.Diagnostics;

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
	public class MainActivity : Activity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			// Get our button from the layout resource,
			// and attach an event to it
			Button button = FindViewById<Button> (Resource.Id.btnLogin);
			button.Click += delegate {
				TextView txtPassword = FindViewById<TextView> (Resource.Id.txtPassword);
				TextView txtUsername = FindViewById<TextView> (Resource.Id.txtUsername);
				string email = txtUsername.Text;
				Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
				Match match = regex.Match(email);

				if (!match.Success){
					Android.Widget.Toast.MakeText(this, "Not a valid Email Address", Android.Widget.ToastLength.Short).Show();
				} else {
					var request = HttpWebRequest.Create(string.Format(@"http://www.romponline.com/", rxcui));
					request.ContentType = "application/xml";
					request.Method = "GET";

					using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
					{
						if (response.StatusCode != HttpStatusCode.OK)
							Console.Out.WriteLine("Error fetching data. Server returned status code: {0}", response.StatusCode);
						using (StreamReader reader = new StreamReader(response.GetResponseStream()))
						{
							var content = reader.ReadToEnd();
							if(string.IsNullOrWhiteSpace(content)) {
								Console.Out.WriteLine("Response contained empty body...");
							}
							else {
								Console.Out.WriteLine("Response Body: \r\n {0}", content);
							}
						}
					}
				}
				//if (txtPassword.Text.Length
				var nextActivity = new Intent(this, typeof(ChooseModeActivity));

			};

		}
	}
}


