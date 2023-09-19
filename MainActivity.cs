using Android.Bluetooth;
using Android.Content;
using Android.Hardware;
using Android.Provider;
using Android.Util;
using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;

namespace sensor_brightness
{
    [Activity(Label = "@string/app_name", MainLauncher = true)]
    public class MainActivity : Activity, ISensorEventListener
    {
        const int REQUEST_ENABLE_BT = 1;

        private SensorManager sensors;
        private Sensor light;
        public IBluetoothLE ble;
        public IDevice desktop;

        private TextView text_light, text_brightness, text_device;
        private Spinner spinner_devices;

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

            ble = CrossBluetoothLE.Current;
            if (!ble.IsOn)
            {
                Intent enableBtIntent = new Intent(BluetoothAdapter.ActionRequestEnable);
                StartActivityForResult(enableBtIntent, REQUEST_ENABLE_BT);
            }
            else
            {
                while (ble.Adapter.GetSystemConnectedOrPairedDevices().Count == 0) ;
                desktop = ble.Adapter.GetSystemConnectedOrPairedDevices().First();
                ble.Adapter.ConnectToDeviceAsync(desktop);
                text_device.Text = desktop.Name;
            }
            ble.StateChanged += (s, e) =>
            {
                switch (e.NewState) 
                {
                case BluetoothState.On:
                    while (ble.Adapter.GetSystemConnectedOrPairedDevices().Count == 0) ;
                    desktop = ble.Adapter.GetSystemConnectedOrPairedDevices().First();
                    ble.Adapter.ConnectToDeviceAsync(desktop);
                    text_device.Text = desktop.Name;
                    break;
                case BluetoothState.Off:
                    desktop = null;
                    break;
                }
            };
        }

        public void OnAccuracyChanged(Sensor sensor, SensorStatus accuracy)
        {
            // throw new NotImplementedException();
        }

        public void OnSensorChanged(SensorEvent e)
        {
            lux = e.Values[0];
            text_light.Text = "light: " + lux.ToString();
            text_brightness.Text = "brightness: " + brightnessGet().ToString();
            // throw new NotImplementedException();
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
    }
}