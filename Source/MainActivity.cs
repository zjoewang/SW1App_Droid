//
// Copyright (c) 2017-2018 Equine Smart Bits, LLC. All rights reserved

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
using Android.Content.PM;
using Android;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Permission = Plugin.Permissions.Abstractions.Permission;

namespace ESB
{
    class USBDeviceListAdapter : ArrayAdapter<string>
    {
        public USBDeviceListAdapter(Context context)
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
            // row.FindViewById<TextView>(global::Android.Resource.Id.Text2).Text = name;

            return row;
        }
    }

    [Activity (Label = "@string/app_name", MainLauncher = true, Icon = "@drawable/esblogo")]
	class MainActivity : Activity
	{
		static readonly string TAG = typeof(MainActivity).Name;
        string build_number;
        ListView listView;
		TextView progressBarTitle;
		ProgressBar progressBar;
        Button buttonData;
        Button buttonChart;
        private USBDeviceListAdapter listAdapter;
        private IHeartRateEnumerator _hrEnumerator;

        /*
        readonly string[] PermissionsLocation =
            {
                Manifest.Permission.AccessCoarseLocation,
                Manifest.Permission.AccessFineLocation
            };
        const int RequestLocationId = 0;

        bool GetPermissions()
        {
            if ((int)Build.VERSION.SdkInt < 23)
                return true;

            //Check to see if any permission in our group is available, if one, then all are
            const string permission = Manifest.Permission.AccessFineLocation;
            
            if (CheckSelfPermission(permission) == (int)Permission.Granted)
                return true;

            //Finally request permissions with the list of permissions and Id
            RequestPermissions(PermissionsLocation, RequestLocationId);

            return false;
        }

        public override async void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            switch (requestCode)
            {
                case RequestLocationId:
                    {
                        if (grantResults[0] == Permission.Granted)
                        {
                            ;       // good
                        }
                        else
                        {
                            //Permission Denied :(
                            //Disabling location functionality
                            ;       // sorry
                        }
                    }
                    break;
            }
        }
        */

        protected async override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);

            PackageManager manager = this.PackageManager;
            PackageInfo info = manager.GetPackageInfo(this.PackageName, 0);
            build_number = info.VersionName;

            SetContentView(Resource.Layout.Main);

            listAdapter = new USBDeviceListAdapter(this);

            this.Title += " (ver " + build_number + ")"; ;

			listView = FindViewById<ListView>(Resource.Id.deviceList);
            listView.Adapter = listAdapter;
            progressBar = FindViewById<ProgressBar>(Resource.Id.progressBar);
			progressBarTitle = FindViewById<TextView>(Resource.Id.progressBarTitle);
            HideProgressBar();
            buttonData = FindViewById<Button>(Resource.Id.button1);
            buttonChart = FindViewById<Button>(Resource.Id.button2);

            listView.ItemClick += (sender, e) => {
                OnItemClick(sender, e);
            };

            bool permitted = false;

            try
            {
                var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Location);
                if (status != PermissionStatus.Granted)
                {
                    if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Permission.Location))
                    {
                        Application.Current.MainPage.DisplayAlert("Need location", "Gunna need that location", "OK");
                    }

                    var results = await CrossPermissions.Current.RequestPermissionsAsync(Permission.Location);
                    //Best practice to always check that the key exists
                    if (results.ContainsKey(Permission.Location))
                        status = results[Permission.Location];
                }

                if (status == PermissionStatus.Granted)
                    permitted = true;
            }
            catch (Exception ex)
            {
                permitted = false;
            }
        }

        protected override void OnResume()
		{
			base.OnResume ();

			listView.ItemClick += (sender, e) => {
				OnItemClick(sender, e);
			};

            buttonData.Click += (sender, e) => {
                OnButtonDataClicked(sender, e);
            };

            buttonChart.Click += (sender, e) => {
                OnButtonChartClicked(sender, e);
            };
		}

        void OnButtonDataClicked(object sender, EventArgs e)
        {
            if (_hrEnumerator == null)
            {
                ShowProgressBar();

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
            if (deviceName != null)
                listAdapter.Add($"{sender.GetType().ToString()}:{deviceName}");
        }

        void OnButtonChartClicked(object sender, EventArgs e)
        {
            if (_hrEnumerator != null)
            {
                HideProgressBar();
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

		void OnItemClick(object sender, AdapterView.ItemClickEventArgs e)
		{
			Log.Info(TAG, "Pressed item " + e.Position);

            OnButtonChartClicked(sender, e);

            string selected = (string)listView.GetItemAtPosition(e.Position);

            var activityHeart = new Intent(this, typeof(ChartViewActivity));
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
            progressBarTitle.Text = "";
            progressBar.Visibility = ViewStates.Invisible;
		}
	}
}


