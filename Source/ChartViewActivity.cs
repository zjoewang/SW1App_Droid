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
        private string _deviceName;
        static private PlotModel _plotModel;
        static LineSeries seriesHR;
        static LineSeries seriesSP;
        private IHeartRate _heartRate;
        public PlotModel MyModel {
            get { return _plotModel; }
            set { ChartViewActivity._plotModel = value; }
        }
        PlotView plotView = null;
        static int curHR = -1;
        static int curSP = -1;
        static int nowHR = -1;
        static int nowSP = -1;
        static int curTS = 0;
        static double curTemp = 0;
        private PowerManager.WakeLock wakeLock;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.ChartView);

            _deviceName = Intent.GetStringExtra("device") ?? "---";

            var powerManager = (PowerManager)ApplicationContext.GetSystemService(Context.PowerService);
            wakeLock = powerManager.NewWakeLock(WakeLockFlags.Partial, "myNicolLock");
            wakeLock.Acquire();

            if (_plotModel == null)
            {
                _plotModel = new PlotModel();

                _plotModel.PlotMargins = new OxyThickness(40, 40, 40, 40);
                _plotModel.Background = OxyColors.Black;
                _plotModel.TitleFontSize = 40;
                _plotModel.TitleColor = OxyColors.Green;
                _plotModel.SubtitleFontSize = 24;
                _plotModel.TextColor = OxyColors.White;

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
                _plotModel.Axes.Add(linearAxisLeft);

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
                _plotModel.Axes.Add(linearAxisRight);

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

                _plotModel.Series.Add(seriesHR);

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

                _plotModel.Series.Add(seriesSP);
            }

            plotView = FindViewById<PlotView>(Resource.Id.plotViewModel);
    
            if (plotView.Model == null)
                plotView.Model = _plotModel;

            Button_start_hr_Click(null, null);

            MyModel.InvalidatePlot(true);
        }

        protected override void OnDestroy()
        {
            wakeLock.Release();
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
            bool bRfresh = false;

            switch (header)
            {
                case 0:
                    curHR = val;
                    ++curTS;
                    seriesHR.Points.Add(new DataPoint(curTS, curHR));
                    if (seriesHR.Points.Count > 200)
                        seriesHR.Points.RemoveRange(0, 50);
                    break;

                case 1:
                    curSP = val;
                    seriesSP.Points.Add(new DataPoint(curTS, curSP));
                    if (seriesSP.Points.Count > 200)
                        seriesSP.Points.RemoveRange(0, 50);
                    break;

                case 2:
                    curTemp = (double) val / 10.0;     // val is in 1/10th Fehrenhait unit
                    break;

                case 3:
                    nowHR = val;
                    break;

                case 4:
                    nowSP = val;
                    bRfresh = true;
                    break;

                default:
                    break;
            }

            if (bRfresh)
            {
                MyModel.Title = string.Format("HR = {0} bpm, SP = {1}%", curHR, curSP);
                MyModel.Subtitle = string.Format("T = {0} F, rHR = {1}, rSP = {2}%", curTemp, nowHR, nowSP);
                MyModel.InvalidatePlot(true);
            }
          
        }
    }
}
