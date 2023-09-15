using Android.Bluetooth;
using Android.OS;
using Android.Util;

namespace sensor_brightness
{
    internal class BluetoothService
    {
        private const string TAG = "bluetooth";
        private static Handler handler;

        private interface MessageConstants
        {
            public const int MESSAGE_READ = 0;
            public const int MESSAGE_WRITE = 1;
        }

        private class ConnectedThread : Java.Lang.Thread
        {
            private BluetoothSocket socket;
            private Stream inputStream, outputStream;
            private byte[] buffer;

            public ConnectedThread(BluetoothSocket socket)
            {
                this.socket = socket;
                try { inputStream = socket.InputStream; }
                catch { Log.Error(TAG, "input stream create error"); }
                try { outputStream = socket.OutputStream; }
                catch { Log.Error(TAG, "input stream create error"); }
            }

            public void run() 
            {
                buffer = new byte[1024];
                int len;
                while (true)
                    try 
                    {
                        len = inputStream.Read(buffer);
                        Message redaMsg = handler.ObtainMessage(MessageConstants.MESSAGE_READ, len, -1, buffer);
                        redaMsg.SendToTarget();
                    } catch (IOException e) 
                    {
                        Log.Debug(TAG, "input stream disconnected", e);
                        break;
                    }
            }

            public void write(byte[] bytes)
            { try {
                    outputStream.Write(bytes);
                    Message writeMsg = handler.ObtainMessage(MessageConstants.MESSAGE_WRITE, -1, -1, buffer);
                    writeMsg.SendToTarget();
                } catch (IOException e){
                    Log.Error(TAG, "output stream error", e);
                } 
            }

            public void cancel()
            { try {
                    socket.Close();
                } catch (IOException e) {
                    Log.Error(TAG, "close the connect socket error", e);
                }
            }
        }
    }
}
