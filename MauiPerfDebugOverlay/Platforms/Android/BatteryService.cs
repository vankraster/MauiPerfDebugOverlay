using Android.App;
using Android.Content;
using Android.OS;
using Application = Android.App.Application;




namespace MauiPerfDebugOverlay.Platforms
{
    public static class BatteryService
    {
        private static readonly BatteryManager _batteryManager;
        private static readonly IntentFilter _batteryFilter; 

        static BatteryService()
        { 
            _batteryManager = Application.Context.GetSystemService(Context.BatteryService) as BatteryManager;
            _batteryFilter = new IntentFilter(Intent.ActionBatteryChanged);
        }

        public static double GetBatteryMilliW()
        {
            var batteryStatus = Application.Context.RegisterReceiver(null, _batteryFilter);

            int voltage = batteryStatus?.GetIntExtra(BatteryManager.ExtraVoltage, -1) ?? -1; // mV
            int currentMicroA = _batteryManager?.GetIntProperty((int)BatteryProperty.CurrentNow) ?? -1; // µA

            if (voltage > 0 && currentMicroA > 0)
            {
                return (currentMicroA / 1000.0) * (voltage / 1000.0); // mW
            }

            return 0;
        }
    }
}