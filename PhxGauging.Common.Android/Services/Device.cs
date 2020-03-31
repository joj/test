using Android.Bluetooth;
using PhxGauging.Common.Services;

namespace PhxGauging.Common.Android.Services
{
    public class Device : IDevice
    {
        public string Name { get; protected set; }
        public bool IsConnected { get; protected set; }
        public int SignalStrength { get; protected set; }
        public string Address { get; protected set; }

        internal BluetoothDevice BluetoothDevice { get; set; }

        public Device(BluetoothDevice bluetoothDevice)
        {
            BluetoothDevice = bluetoothDevice;
            Name = bluetoothDevice.Name;
            Address = bluetoothDevice.Address;
        }

        public Device(BluetoothDevice bluetoothDevice, int signalStrength)
        {
            BluetoothDevice = bluetoothDevice;
            Name = bluetoothDevice.Name;
            Address = bluetoothDevice.Address;
            SignalStrength = signalStrength;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}