using System;
using System.IO;
using Android.Bluetooth;
using Android.Content;
using Android.Widget;
using Java.Lang;
using Java.Util;
using PhxGauging.Common.Services;
using Exception = System.Exception;
using String = System.String;
using System.Collections.Generic;

namespace PhxGauging.Common.Android.Services
{
    public class AndroidBluetoothService : IBluetoothService
    {
        private readonly Context context;
        private static UUID SERIAL_UUID = UUID.FromString("00001101-0000-1000-8000-00805f9b34fb");

        private BluetoothAdapter bluetoothAdapter;
        public Receiver receiver;
        private BluetoothSocket bluetoothSocket;
        public BluetoothDevice bluetoothDevice;

        public event DeviceDiscoveredHandler DeviceDiscovered;
        public event EventHandler DeviceDiscoveryComplete;

        public Stream InputStream { get { return bluetoothSocket == null ? null : bluetoothSocket.InputStream; } }
        public Stream OutputStream { get { return bluetoothSocket == null ? null : bluetoothSocket.OutputStream; } }

        public AndroidBluetoothService(Context context)
        {
            this.context = context;
            bluetoothAdapter = BluetoothAdapter.DefaultAdapter;
            if(bluetoothAdapter.IsEnabled==false)
            {
                bluetoothAdapter.Enable();
            }
            // Register for broadcasts when a device is discovered
            receiver = new Receiver();
            receiver.OnDeviceDiscoveredFunc = OnDeviceDiscovered;
            receiver.OnDiscoveryCompleteFunc = OnDeviceDiscoveryComplete;

            var filter = new IntentFilter();
            filter.AddAction(BluetoothDevice.ActionFound);
            filter.AddAction(BluetoothAdapter.ActionDiscoveryFinished);
            context.RegisterReceiver(receiver, filter);
        }

        public void StartDiscovery()
        {
            if (bluetoothAdapter.IsDiscovering == false)
            {
                bluetoothAdapter.StartDiscovery();
            }            
        }

        protected virtual void OnDeviceDiscovered(IDevice device)
        {
            var handler = DeviceDiscovered;
            if (handler != null) handler(new IDevice[] { device });
        }

        private bool ignite = false;

        public void Connect(IDevice device)
        {
            this.bluetoothSocket = this.bluetoothDevice.CreateRfcommSocketToServiceRecord(SERIAL_UUID);
            this.bluetoothSocket.Connect();
        }
        public void StopDiscovery()
        {
            bluetoothAdapter.CancelDiscovery();
        }

        public void Disconnect(IDevice device)
        {
            bluetoothSocket.Close();
        }

        protected virtual void OnDeviceDiscoveryComplete()
        {
            var handler = DeviceDiscoveryComplete;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public void GetSignalStrength(string name, Action<int> signalStrengthCallback)
        {
            /* bluetoothAdapter = BluetoothAdapter.DefaultAdapter;

             bluetoothSocket.Close();
             // Register for broadcasts when a device is discovered
             receiver = new Receiver();
             receiver.OnDeviceDiscoveredFunc = d =>
             {
                 if (d.Name == name)
                 {
                     signalStrengthCallback(d.SignalStrength);
                     Connect(d);
                 }
             };

             var filter = new IntentFilter(BluetoothDevice.ActionFound);
             context.RegisterReceiver(receiver, filter);

             // Register for broadcasts when discovery has finished
             filter = new IntentFilter(BluetoothAdapter.ActionDiscoveryFinished);
             context.RegisterReceiver(receiver, filter);

             StartDiscovery();*/
        }
    }

    public class Receiver : BroadcastReceiver
    {
        public Action<IDevice> OnDeviceDiscoveredFunc;
        public Action OnDiscoveryCompleteFunc;

        public BluetoothDevice phxDevice;

        public List<BluetoothDevice> phxDevices = new List<BluetoothDevice>();

        public BluetoothDevice getBluetooth()
        {
            return this.phxDevice;
        }

        public List<BluetoothDevice> getBluetooths()
        {
            return this.phxDevices;
        }

        public override void OnReceive(Context context, Intent intent)
        {

            string action = intent.Action;

            // When discovery finds a device
            if (action == BluetoothDevice.ActionFound)
            {
                // Get the BluetoothDevice object from the Intent
                BluetoothDevice device = (BluetoothDevice)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);

                if (string.IsNullOrEmpty(device.Name))
                    return;

                int rssi = intent.GetShortExtra(BluetoothDevice.ExtraRssi, Short.MinValue);
                //String name = intent.GetStringExtra(BluetoothDevice.ExtraName);

                // If it's already paired, skip it, because it's been listed already
                //if (device.BondState != Bond.Bonded)
                //{
                OnDeviceDiscoveredFunc(new Device(device, rssi));
                //}
                // When discovery is finished, change the Activity title

                phxDevice = (BluetoothDevice)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);
                int num = 0;
                if (this.phxDevices == null)
                {
                    if (this.phxDevice.Name.ToLower().Contains("phx"))
                    {
                        this.phxDevices.Add(this.phxDevice);
                    }
                }
                else
                {
                    foreach (BluetoothDevice current in this.phxDevices)
                    {
                        if (this.phxDevice.Name == current.Name)
                        {
                            num = 1;
                            break;
                        }
                    }
                    if (num == 0 && this.phxDevice.Name.ToLower().Contains("phx"))
                    {
                        this.phxDevices.Add(this.phxDevice);
                    }
                }
            }
            else if (action == BluetoothAdapter.ActionDiscoveryFinished)
            {
                if (OnDiscoveryCompleteFunc != null)
                    OnDiscoveryCompleteFunc(); 
            }
        }
    }
}