//
// Copyright (c) 2017 Equine Smart Bits, LLC. All rights reserved

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Hardware.Usb;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using System.Globalization;
using System.Threading;

[assembly: UsesFeature ("android.hardware.usb.host")]

namespace ESB
{
	[Activity (Label = "@string/app_name", MainLauncher = true, Icon = "@drawable/esblogo")]
	class MainActivity : Activity
	{
		static readonly string TAG = typeof(MainActivity).Name;
        string build_number = "0.815";
		ListView listView;
		TextView progressBarTitle;
		ProgressBar progressBar;
        Button buttonData;
        Button buttonChart;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate(bundle);

			SetContentView(Resource.Layout.Main);

            this.Title += " (ver " + build_number + ")";

			listView = FindViewById<ListView>(Resource.Id.deviceList);
			progressBar = FindViewById<ProgressBar>(Resource.Id.progressBar);
			progressBarTitle = FindViewById<TextView>(Resource.Id.progressBarTitle);
            buttonData = FindViewById<Button>(Resource.Id.button1);
            buttonChart = FindViewById<Button>(Resource.Id.button2);
        }

		protected override async void OnResume ()
		{
			base.OnResume ();

			listView.ItemClick += async (sender, e) => {
				await OnItemClick(sender, e);
			};

            buttonData.Click += async (sender, e) => {
                await OnButtonDataClicked(sender, e);
            };

            buttonChart.Click += async (sender, e) => {
                await OnButtonChartClicked(sender, e);
            };

            await PopulateListAsync();
		}

        async Task OnButtonDataClicked(object sender, EventArgs e)
        {
            /*
            var permissionGranted = await usbManager.RequestPermissionAsync(selectedPort.Driver.Device, this);
            if (permissionGranted)
            {
                // start the SerialConsoleActivity for this device
                var intendDataView = new Intent(this, typeof(DataViewActivity));
                intendDataView.PutExtra(DataViewActivity.EXTRA_TAG, new UsbSerialPortInfo(selectedPort));
                StartActivity(intendDataView);
            }
            */
        }

        async Task OnButtonChartClicked(object sender, EventArgs e)
        {
            ;
        }

        protected override void OnPause()
		{
			base.OnPause();
		}

		protected override void OnDestroy()
		{
			base.OnDestroy ();

		}

		async Task OnItemClick(object sender, AdapterView.ItemClickEventArgs e)
		{
			Log.Info(TAG, "Pressed item " + e.Position);
		}

		async Task PopulateListAsync ()
		{
            /*
			ShowProgressBar ();

			Log.Info (TAG, "Refreshing device list ...");

			var drivers = await FindAllDriversAsync (usbManager);

			adapter.Clear ();
			foreach (var driver in drivers) {
				var ports = driver.Ports;
				Log.Info (TAG, string.Format ("+ {0}: {1} port{2}", driver, ports.Count, ports.Count == 1 ? string.Empty : "s"));
				foreach(var port in ports)
					adapter.Add (port);
			}

			adapter.NotifyDataSetChanged();
			progressBarTitle.Text = string.Format("{0} device{1} found", adapter.Count, adapter.Count == 1 ? string.Empty : "s");
			HideProgressBar();
			Log.Info(TAG, "Done refreshing, " + adapter.Count + " entries found.");

            if (adapter.Count == 1)
            {
                buttonData.Enabled = true;
                buttonChart.Enabled = true;
            }
            else
            {
                buttonData.Enabled = false;
                buttonChart.Enabled = false;
            }
            */
		}

		void ShowProgressBar()
		{
			progressBar.Visibility = ViewStates.Visible;
			progressBarTitle.Text = GetString(Resource.String.refreshing);
		}

		void HideProgressBar()
		{
			progressBar.Visibility = ViewStates.Invisible;
		}
	}
}


