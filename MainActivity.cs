using Android.Content;
using Android.Hardware;
using Android.Provider;

namespace sensor_brightness
{
    [Activity(Label = "@string/app_name", MainLauncher = true)]
    public class MainActivity : Activity, ISensorEventListener
    {
        private SensorManager sensors;
        private Sensor light;

        private TextView text_light, text_brightness;
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