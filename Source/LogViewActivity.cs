//
// Copyright (c) 2017 Equine Smart Bits, LLC. All rights reserved

using System;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Hardware.Usb;
using Android.OS;
using Android.Util;
using Android.Widget;
using System.Threading;

namespace ESB
{
    [Activity (Label = "@string/app_name", LaunchMode = LaunchMode.SingleTop)]			
	class LogViewActivity : Activity
	{
		static readonly string TAG = typeof(LogViewActivity).Name;
		public const string EXTRA_TAG = "PortInfo";
		TextView titleTextView;
		TextView dumpTextView;
		ScrollView scrollView;

		protected override void OnCreate (Bundle bundle)
		{
			Log.Info(TAG, "OnCreate");
			base.OnCreate(bundle);
			SetContentView(Resource.Layout.LogView);
			titleTextView = FindViewById<TextView>(Resource.Id.demoTitle);
			dumpTextView = FindViewById<TextView>(Resource.Id.consoleText);
			scrollView = FindViewById<ScrollView>(Resource.Id.demoScroller);
		}

		protected override void OnPause ()
		{
			Log.Info(TAG, "OnPause");
			base.OnPause();
		}

		protected async override void OnResume ()
		{
			Log.Info(TAG, "OnResume");
			base.OnResume();
		}

		void UpdateReceivedData(byte[] data)
		{
			/*var message = "Read " + data.Length + " bytes: \n"
				+ HexDump.DumpHexString (data) + "\n\n";
            dumpTextView.Append(message);*/
            string result = System.Text.Encoding.UTF8.GetString(data);

            dumpTextView.Append(result);
			scrollView.SmoothScrollTo(0, dumpTextView.Bottom);		
		}
	}
}

