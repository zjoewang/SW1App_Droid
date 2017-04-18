using System;

namespace ESB
{
    public interface IHeartRateEnumerator
    {
        bool StartDeviceScan();
        bool StopDeviceScan();
        event EventHandler<string> DeviceScanUpdate;
        event EventHandler DeviceScanTimeout;

        IHeartRate GetHeartRate(string name);
    }
}