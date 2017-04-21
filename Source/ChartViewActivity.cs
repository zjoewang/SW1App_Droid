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
        private string _deviceName;

        private IHeartRate _heartRate;
        static readonly string TAG = typeof(ChartViewActivity).Name;
        private PlotView plotViewModel;
        public PlotModel MyModel { get; set; }
        LineSeries seriesHR;
        LineSeries seriesSP;
        int curHR = -1;
        int curSP = -1;
        int curTS = 0;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.ChartView);

            _deviceName = Intent.GetStringExtra("device") ?? "---";

            var powerManager = (PowerManager)ApplicationContext.GetSystemService(Context.PowerService);
            var wakeLock = powerManager.NewWakeLock(WakeLockFlags.Partial, "myNicolLock");
            wakeLock.Acquire();

            plotViewModel = FindViewById<PlotView>(Resource.Id.plotViewModel);

            var plotModel1 = new PlotModel();

            plotModel1.PlotMargins = new OxyThickness(40, 40, 40, 40);
            plotModel1.Background = OxyColors.Black;
            plotModel1.TitleFontSize = 40;
            plotModel1.TitleColor = OxyColors.Green;
            plotModel1.SubtitleFontSize = 24;
            plotModel1.TextColor = OxyColors.White;

            var linearAxis1 = new LinearAxis();
            linearAxis1.MajorGridlineStyle = LineStyle.Solid;
            linearAxis1.MinorGridlineStyle = LineStyle.Dot;
            linearAxis1.Title = "HR";
            linearAxis1.Key = "HR";
            linearAxis1.Position = AxisPosition.Left;
            linearAxis1.Minimum = 10.0;
            linearAxis1.Maximum = 250.0;
            plotModel1.Axes.Add(linearAxis1);
            var linearAxis2 = new LinearAxis();
            linearAxis2.MajorGridlineStyle = LineStyle.Solid;
            linearAxis2.MinorGridlineStyle = LineStyle.Dot;
            linearAxis2.Position = AxisPosition.Right;
            linearAxis2.Title = "%SpO2";
            linearAxis2.Key = "SP";
            linearAxis2.Minimum = 50.0;
            linearAxis2.Maximum = 100.0;
            plotModel1.Axes.Add(linearAxis2);

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

            plotModel1.Series.Add(seriesHR);

            MyModel = plotModel1;
            plotViewModel.Model = MyModel;

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

            plotModel1.Series.Add(seriesSP);

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
            double temp = 0;

            if (header == 0)
            {
                curHR = val;
                ++curTS;
                seriesHR.Points.Add(new DataPoint(curTS, curHR));
            }
            else if (header == 1)
            {
                curSP = val;
                seriesHR.Points.Add(new DataPoint(curTS, curSP));
            }
            else if (header == 2)
                temp = (double) val / 10.0;     // val is in 1/10th Fehrenhait unit

            // Thnning data so we don't accumulate too much historical data
            if (seriesHR.Points.Count > 200)
            {
                seriesHR.Points.RemoveRange(0, 50);
                seriesSP.Points.RemoveRange(0, 50);
            }

            MyModel.Title = string.Format("HR = {0} bpm, SP = {1}%", curHR, curSP);
            MyModel.Subtitle = $"T = {temp} F";
            MyModel.InvalidatePlot(true);
        }
    }
}
