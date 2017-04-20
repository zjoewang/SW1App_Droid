using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace ESB
{
    public class HeartRateEnumeratorAndroid : IHeartRateEnumerator
    {
        private Plugin.BLE.Abstractions.Contracts.IAdapter _adapter;
        private List<IDevice> _devices;
        private CancellationTokenSource _cancellationTokenSource;

        public event EventHandler<string> DeviceScanUpdate;
        public event EventHandler DeviceScanTimeout;

        public bool StartDeviceScan()
        {
            _adapter = CrossBluetoothLE.Current.Adapter;
            _devices = new List<IDevice>();
            _cancellationTokenSource = new CancellationTokenSource();

            _adapter.DeviceDiscovered += _adapter_DeviceDiscovered;
            _adapter.ScanTimeoutElapsed += _adapter_ScanTimeoutElapsed;
            _adapter.ScanTimeout = 15000; // millisecondi

            foreach(var dev in _adapter.ConnectedDevices)
            {
                _adapter_DeviceDiscovered(dev);
            }

            _adapter.StartScanningForDevicesAsync(serviceUuids: null, deviceFilter: null, cancellationToken: _cancellationTokenSource.Token);
            return true;
        }

        private void _adapter_ScanTimeoutElapsed(object sender, EventArgs e)
        {
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;

            DeviceScanTimeout?.Invoke(this,null);
        }

        private void _adapter_DeviceDiscovered(object sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
        {
            _adapter_DeviceDiscovered(e.Device);
        }

        private void _adapter_DeviceDiscovered(IDevice device)
        {
            _devices?.Add(device);
            DeviceScanUpdate?.Invoke(this, device.Name);
        }

        public bool StopDeviceScan()
        {
            _cancellationTokenSource?.Cancel(true);
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;

            _adapter.DeviceDiscovered -= _adapter_DeviceDiscovered;
            _adapter.ScanTimeoutElapsed -= _adapter_ScanTimeoutElapsed;
            return true;
        }

        static public IHeartRate _GetHeartRate(string name)
        {
            var hr = new HeartRateAndroidBLE(name);
            return hr;
        }

        public IHeartRate GetHeartRate(string name)
        {
            return HeartRateEnumeratorAndroid._GetHeartRate(name);
        }
    }
}