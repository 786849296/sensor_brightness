using Android.Content;
using Android.OS;

namespace sensor_brightness
{
    class BluetoothLeService : Service
    {
        private Binder binder;

        public BluetoothLeService() 
        {
            binder = new LocalBinder(this);
        }

        public override IBinder? OnBind(Intent? intent)
        {
            return binder;
        }

        public class LocalBinder : Binder
        {
            public BluetoothLeService service { get; }

            public LocalBinder(BluetoothLeService service)
            {
                this.service = service;
            }
        }
    }

    class ServiceConnection : Java.Lang.Object, IServiceConnection
    {
        private BluetoothLeService bluetoothService;

        public void OnServiceConnected(ComponentName? name, IBinder? service)
        {
            bluetoothService = ((BluetoothLeService.LocalBinder)service).service;
            if (bluetoothService != null) { }
        }

        public void OnServiceDisconnected(ComponentName? name)
        {
            bluetoothService = null;
        }
    }
}
