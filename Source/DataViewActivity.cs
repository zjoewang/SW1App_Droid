//
// Copyright (c) 2017 Equine Smart Bits, LLC. All rights reserved


using Android.App;
using Android.Content.PM;
using Android.Hardware.Usb;
using Android.OS;
using Android.Util;
using Android.Widget;

namespace ESB
{
    [Activity (Label = "@string/app_name", LaunchMode = LaunchMode.SingleTop)]			
	class DataViewActivity : Activity
	{
		static readonly string TAG = typeof(DataViewActivity).Name;
		public const string EXTRA_TAG = "PortInfo";

		TextView hrTextView;
		TextView spTextView;
		TextView tempTextView;
		TextView rawhrTextView;
		TextView rawspTextView;

        string input_line;

		protected override void OnCreate(Bundle bundle)
		{
			Log.Info(TAG, "OnCreate");
			base.OnCreate(bundle);
			SetContentView(Resource.Layout.DataView);

			hrTextView = FindViewById<TextView>(Resource.Id.hr);
			spTextView = FindViewById<TextView>(Resource.Id.sp);
			tempTextView = FindViewById<TextView>(Resource.Id.temp);
			rawhrTextView = FindViewById<TextView>(Resource.Id.rawhr);
			rawspTextView = FindViewById<TextView>(Resource.Id.rawsp);
		}

		protected override void OnPause ()
		{
			Log.Info(TAG, "OnPause");
			base.OnPause ();
		}

		protected override void OnResume ()
		{
			Log.Info(TAG, "OnResume");
			base.OnResume();
		}

		void UpdateReceivedData(byte[] data)
		{
            string result = System.Text.Encoding.UTF8.GetString(data);

            input_line += result;

            int count = result.Length;

            if (!result.EndsWith("\n"))
                return;

            string line = input_line;

            input_line = "";

            int hr, sp;
            double  temp;
            bool    calculated;

            ParseLog.GetData(line, out hr, out sp, out temp, out calculated);

            if (temp > 0.0)
            {
                tempTextView.Text = "Temp = " + temp.ToString() + "F";
            }
            else if (calculated)
            {
                hrTextView.Text = "HR = " + hr.ToString() + " bpm";
                spTextView.Text = "SP = " + sp.ToString() + "%";
            }
            else
            {
                rawhrTextView.Text = "raw HR = " + hr.ToString() + " bpm";
                rawspTextView.Text = "raw SP = " + sp.ToString() + "%";
            }
		}
	}
}

