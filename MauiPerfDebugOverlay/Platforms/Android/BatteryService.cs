using Android.App;
using Android.Content;
using Android.OS;
using Application = Android.App.Application;
namespace MauiPerfDebugOverlay.Platforms
{
    public static class BatteryService
    {

        public static double GetBatteryMilliW()
        {
            double _batteryMilliW = 0;
            var context = Application.Context;
            var bm = (BatteryManager)context.GetSystemService(Context.BatteryService);
            var filter = new IntentFilter(Intent.ActionBatteryChanged);
            var batteryStatus = context.RegisterReceiver(null, filter);

            int voltage = batteryStatus?.GetIntExtra(BatteryManager.ExtraVoltage, -1) ?? -1; // mV
            int currentMicroA = bm?.GetIntProperty((int)BatteryProperty.CurrentNow) ?? -1; // µA

            if (voltage > 0 && currentMicroA > 0)
            {
                // Calcul în mW: (mV * mA)
                _batteryMilliW = (currentMicroA / 1000.0) * (voltage / 1000.0);
            }
            else
            {
                _batteryMilliW = 0;
            }

            return _batteryMilliW;
        }
    }
}
