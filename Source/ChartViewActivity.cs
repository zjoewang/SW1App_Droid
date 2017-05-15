//
// Copyright (c) 2017 Equine Smart Bits, LLC. All rights reserved

using System;
using Android.App;
using Android.Content;
using Android.OS;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Xamarin.Android;
using Android.Content.PM;
using Android.Util;
using OxyPlot.Axes;
using Android.Views;
using Android.Widget;
using System.Threading.Tasks;

namespace ESB
{
    [Activity(Label = "@string/app_name", LaunchMode = LaunchMode.SingleTop)]
    public class ChartViewActivity : Activity
    {
        static readonly string TAG = typeof(ChartViewActivity).Name;
        static PlotView plotViewModelView = null;
        static private PlotModel plotModel;
        private string _deviceName;
        static LineSeries seriesHR;
        static LineSeries seriesSP;

        private IHeartRate _heartRate;
        public PlotModel MyModel { get; set; }
        int curHR = -1;
        int curSP = -1;
        int curTS = 0;
        double curTemp = 0;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.ChartView);

            _deviceName = Intent.GetStringExtra("device") ?? "---";

            var powerManager = (PowerManager)ApplicationContext.GetSystemService(Context.PowerService);
            var wakeLock = powerManager.NewWakeLock(WakeLockFlags.Partial, "myNicolLock");
            wakeLock.Acquire();

            if (plotViewModelView == null)
            {
                plotViewModelView = FindViewById<PlotView>(Resource.Id.plotViewModel);

                plotModel = new PlotModel();

                plotModel.PlotMargins = new OxyThickness(40, 40, 40, 40);
                plotModel.Background = OxyColors.Black;
                plotModel.TitleFontSize = 40;
                plotModel.TitleColor = OxyColors.Green;
                plotModel.SubtitleFontSize = 24;
                plotModel.TextColor = OxyColors.White;

                var linearAxisLeft = new LinearAxis();

                linearAxisLeft.MajorGridlineStyle = LineStyle.Solid;
                linearAxisLeft.MinorGridlineStyle = LineStyle.Dot;
                linearAxisLeft.Title = "HR";
                linearAxisLeft.Key = "HR";
                linearAxisLeft.Position = AxisPosition.Left;
                linearAxisLeft.Minimum = 10.0;
                linearAxisLeft.Maximum = 250.0;
                linearAxisLeft.AxislineColor = OxyColors.Magenta;
                linearAxisLeft.TextColor = OxyColors.Magenta;
                linearAxisLeft.TitleColor = OxyColors.Magenta;
                plotModel.Axes.Add(linearAxisLeft);

                var linearAxisRight = new LinearAxis();

                linearAxisRight.MajorGridlineStyle = LineStyle.Solid;
                linearAxisRight.MinorGridlineStyle = LineStyle.Dot;
                linearAxisRight.Position = AxisPosition.Right;
                linearAxisRight.Title = "%SpO2";
                linearAxisRight.Key = "SP";
                linearAxisRight.Minimum = 50.0;
                linearAxisRight.Maximum = 100.0;
                linearAxisRight.AxislineColor = OxyColors.Yellow;
                linearAxisRight.TitleColor = OxyColors.Yellow;
                linearAxisRight.TextColor = OxyColors.Yellow;
                plotModel.Axes.Add(linearAxisRight);

                seriesHR = new LineSeries
                {
                    Title = "Heart Rate (bpm)",
                          Color = OxyColors.Magenta,
                          MarkerType = MarkerType.Circle,
                          MarkerSize = 3,
                          MarkerStroke = OxyColors.Magenta,
                          MarkerFill = OxyColors.Magenta,
                          YAxisKey = "HR",
                          MarkerStrokeThickness = 1.0,
                          StrokeThickness = 5
                };

                plotModel.Series.Add(seriesHR);

                MyModel = plotModel;
                plotViewModelView.Model = MyModel;

                seriesSP = new LineSeries()
                {
                    Title = "Oxygen Saturation Level (%)",
                          Color = OxyColors.Yellow,
                          MarkerType = MarkerType.Circle,
                          MarkerSize = 3,
                          MarkerStroke = OxyColors.Yellow,
                          MarkerFill = OxyColors.Yellow,
                          YAxisKey = "SP",
                          MarkerStrokeThickness = 1.0,
                          StrokeThickness = 5
                };

                plotModel.Series.Add(seriesSP);
            }

            Button_start_hr_Click(null, null);
        }

        protected override void OnDestroy()
        {
            _heartRate?.Stop();

            base.OnDestroy();
        }

        protected override void OnPause()
        {
            Log.Info(TAG, "OnPause");
            base.OnPause();
        }

        protected async override void OnResume()
        {
            Log.Info(TAG, "OnResume");

            base.OnResume();
        }

        private void Button_stop_hr_Click(object sender, EventArgs e)
        {
            _heartRate.Stop();
        }

        private string BLE = typeof(HeartRateEnumeratorAndroid).ToString();
        private int _header, _value;

        private void Button_start_hr_Click(object sender, EventArgs e)
        {
            string[] split = _deviceName.Split(':');

            if (split[0] == BLE)
            {
                _heartRate = HeartRateEnumeratorAndroid._GetHeartRate(split[1]);

                _heartRate.SetUpdateFunc((int header, int val) => {
                    _header = header;
                    _value = val;
                    RunOnUiThread(() => UpdateUI(_header, _value));
                    return Task.Delay(1);
                 });

                _heartRate.Start();
            }
        }

        void UpdateUI(int header, int val)
        {
            if (header == 0)
            {
                curHR = val;
                ++curTS;
                seriesHR.Points.Add(new DataPoint(curTS, curHR));
            }
            else if (header == 1)
            {
                curSP = val;
                seriesSP.Points.Add(new DataPoint(curTS, curSP));
            }
            else if (header == 2)
                curTemp = (double) val / 10.0;     // val is in 1/10th Fehrenhait unit

            // Thnning data so we don't accumulate too much historical data
            if (seriesHR.Points.Count > 200)
            {
                seriesHR.Points.RemoveRange(0, 50);
                seriesSP.Points.RemoveRange(0, 50);
            }

            MyModel.Title = string.Format("HR = {0} bpm, SP = {1}%", curHR, curSP);
            MyModel.Subtitle = string.Format($"T = {curTemp} F");
            MyModel.InvalidatePlot(true);
        }
    }
}
