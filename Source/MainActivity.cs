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

namespace ESB
{
    class UsbSerialPortAdapter : ArrayAdapter<string>
    {
        public UsbSerialPortAdapter(Context context)
            : base(context, global::Android.Resource.Layout.SimpleExpandableListItem2)
        {
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var row = convertView;
            if (row == null)
            {
                var inflater = Context.GetSystemService(Context.LayoutInflaterService) as LayoutInflater;
                row = inflater.Inflate(global::Android.Resource.Layout.SimpleListItem2, null);
            }

            var name = this.GetItem(position);
  
            row.FindViewById<TextView>(global::Android.Resource.Id.Text1).Text = name;
            row.FindViewById<TextView>(global::Android.Resource.Id.Text2).Text = name;

            return row;
        }
    }

    [Activity (Label = "@string/app_name", MainLauncher = true, Icon = "@drawable/esblogo")]
	class MainActivity : Activity
	{
		static readonly string TAG = typeof(MainActivity).Name;
        string build_number = "0.005";
		ListView listView;
		TextView progressBarTitle;
		ProgressBar progressBar;
        Button buttonData;
        Button buttonChart;
        private UsbSerialPortAdapter listAdapter;
        private IHeartRateEnumerator _hrEnumerator;

        protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);

			SetContentView(Resource.Layout.Main);

            listAdapter = new UsbSerialPortAdapter(this);

            this.Title += " (ver " + build_number + ")"; ;

			listView = FindViewById<ListView>(Resource.Id.deviceList);
            listView.Adapter = listAdapter;
            progressBar = FindViewById<ProgressBar>(Resource.Id.progressBar);
			progressBarTitle = FindViewById<TextView>(Resource.Id.progressBarTitle);
            buttonData = FindViewById<Button>(Resource.Id.button1);
            buttonChart = FindViewById<Button>(Resource.Id.button2);

            listView.ItemClick += async (sender, e) => {
                await OnItemClick(sender, e);
            };
        }

        protected override async void OnResume()
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
		}

        async Task OnButtonDataClicked(object sender, EventArgs e)
        {
            if (_hrEnumerator == null)
            {
                progressBar.Visibility = ViewStates.Visible;

                _hrEnumerator = new HeartRateEnumeratorAndroid();
                _hrEnumerator.DeviceScanUpdate += _hrEnumerator_DeviceScanUpdate;
                _hrEnumerator.DeviceScanTimeout += _hrEnumerator_DeviceScanTimeout;
                _hrEnumerator.StartDeviceScan();

                listAdapter.Clear();
                listAdapter.Add($"> ble start");
            }
        }
 
        private void _hrEnumerator_DeviceScanTimeout(object sender, EventArgs e)
        {
            listAdapter.Add($"> timeout {sender.GetType().ToString()}");
            OnButtonChartClicked(sender, e);
        }

        private void _hrEnumerator_DeviceScanUpdate(object sender, string deviceName)
        {
            listAdapter.Add($"{sender.GetType().ToString()}:{deviceName}");
        }

        async Task OnButtonChartClicked(object sender, EventArgs e)
        {
            progressBar.Visibility = ViewStates.Invisible;

            if (_hrEnumerator != null)
            {
                _hrEnumerator.DeviceScanUpdate -= _hrEnumerator_DeviceScanUpdate;
                _hrEnumerator.DeviceScanTimeout -= _hrEnumerator_DeviceScanTimeout;
                _hrEnumerator.StopDeviceScan();
                _hrEnumerator = null;
                listAdapter.Add($"> ble stop");
            }
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

            await OnButtonChartClicked(sender, e);

            string selected = (string)listView.GetItemAtPosition(e.Position);

            var activityHeart = new Intent(this, typeof(HeartActivity));
            activityHeart.PutExtra($"device", selected);
            StartActivity(activityHeart);
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


