using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Android.Hardware;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Util;
using Java.Util;
using System.Text;

namespace sensor_brightness
{
    [Activity(Label = "@string/app_name", MainLauncher = true)]
    public class MainActivity : Activity, ISensorEventListener
    {
        const int REQUEST_ENABLE_BT = 1;
        static UUID service_sensor = UUID.NameUUIDFromBytes(Encoding.UTF8.GetBytes("light"));

        private SensorManager sensors;
        private Sensor light;
        private BluetoothLeAdvertiser bluetoothLeAdvertiser;
        private AdvertiseData advertiseData;
        private SensorAdCallback adCallback = new();

        private TextView text_light, text_brightness, text_device;

        public float lux { set; get; }
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            sensors = (SensorManager)GetSystemService(Context.SensorService);
            light = sensors.GetDefaultSensor(SensorType.Light);
            text_light = FindViewById<TextView>(Resource.Id.light_value);
            text_brightness = FindViewById<TextView>(Resource.Id.brightness);
            text_device = FindViewById<TextView>(Resource.Id.device_name);

            AdvertisingSetParameters parameters = new AdvertisingSetParameters.Builder()
                .SetLegacyMode(false)
                .SetConnectable(false)
                .SetInterval(AdvertisingSetParameters.IntervalHigh)
                .SetTxPowerLevel(AdvertiseTxPower.Medium)
                .SetPrimaryPhy(Android.Bluetooth.BluetoothPhy.Le1m)
                .SetSecondaryPhy(Android.Bluetooth.BluetoothPhy.Le2m)
                .Build();

            advertiseData = new AdvertiseData.Builder()
                .SetIncludeDeviceName(true)
                .SetIncludeTxPowerLevel(false)
                .Build();
            //AdvertiseData scanResponse =  new AdvertiseData.Builder()
            //    .SetIncludeDeviceName(false)
            //    .SetIncludeTxPowerLevel(false)
            //    .AddServiceData(new ParcelUuid(service_sensor), data)
            //    .Build();
            BluetoothManager bluetoothManager = (BluetoothManager)GetSystemService(Context.BluetoothService);
            BluetoothAdapter bluetoothAdapter = bluetoothManager.Adapter;
            bluetoothAdapter.SetName("sensor_light");
            if (!bluetoothAdapter.IsEnabled)
            {
                Intent enableBtIntent = new Intent(BluetoothAdapter.ActionRequestEnable);
                StartActivityForResult(enableBtIntent, REQUEST_ENABLE_BT);
            }
            //todo：请打开蓝牙底部弹出警告
            while (!bluetoothAdapter.IsEnabled) ;
            bluetoothLeAdvertiser = bluetoothAdapter.BluetoothLeAdvertiser;
            bluetoothLeAdvertiser.StartAdvertisingSet(parameters, advertiseData, null, null, null, adCallback);
        }

        public void OnAccuracyChanged(Sensor sensor, SensorStatus accuracy)
        {
            // throw new NotImplementedException();
        }

        public void OnSensorChanged(SensorEvent e)
        {
            lux = e.Values[0];
            text_light.Text = "light: " + lux.ToString();
            var brightness = brightnessGet();
            text_brightness.Text = "brightness: " + brightness.ToString();
            if (bluetoothLeAdvertiser != null && adCallback.currentAdSet != null)
            {
                byte[] data_light = BitConverter.GetBytes(lux);
                byte[] data_brightness = BitConverter.GetBytes(brightness);
                List<byte> list = new List<byte>();
                list.AddRange(data_light);
                list.AddRange(data_brightness);
                byte[] data = list.ToArray();
                advertiseData.ServiceData.Clear();
                advertiseData.ServiceData.Add(new ParcelUuid(service_sensor), data);
                adCallback.currentAdSet.SetAdvertisingData(advertiseData);
            }
        }
        protected override void OnResume()
        {
            base.OnResume();
            sensors.RegisterListener(this, light, SensorDelay.Ui);
            text_brightness.Text = "light sensor started";
        }

        protected override void OnPause()
        {
            base.OnPause();
            sensors.UnregisterListener(this);
        }

        private int brightnessGet()
        {
            return Settings.System.GetInt(this.ContentResolver, Settings.System.ScreenBrightness);
        }

        private class SensorAdCallback : AdvertisingSetCallback
        {
            public AdvertisingSet currentAdSet { set; get; }

            public override void OnAdvertisingSetStarted(AdvertisingSet? advertisingSet, int txPower, [GeneratedEnum] AdvertiseResult status)
            {
                base.OnAdvertisingSetStarted(advertisingSet, txPower, status);
                if (status == AdvertiseResult.Success)
                {
                    Log.Debug("bluetooth", "advertising started");
                    currentAdSet = advertisingSet;
                }
                else
                    Log.Debug("bluetooth", "advertising failed. error code: " + status);
            }

            public override void OnAdvertisingSetStopped(AdvertisingSet? advertisingSet)
            {
                base.OnAdvertisingSetStopped(advertisingSet);
                Log.Debug("bluetooth", "advertising stopped");
            }
        }
    }
}