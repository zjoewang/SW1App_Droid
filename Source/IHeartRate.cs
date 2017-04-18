using System;

namespace ESB
{
    public class HeartRateData
    {
        public DateTime Timestamp;
        public int Value;
        public DateTime? TimestampBatteryLevel;
        public int? BatteryLevel;
    }
    
    public interface IHeartRate
    {
        string DeviceName { get; }
        bool Start();
        bool IsRunning { get; }
        void Stop();
        HeartRateData GetCurrentHeartRateValue();
    }
}