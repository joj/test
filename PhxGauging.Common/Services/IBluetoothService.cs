using System;
using System.Collections.Generic;
using System.IO;

namespace PhxGauging.Common.Services
{
    public delegate void DeviceDiscoveredHandler(IEnumerable<IDevice> devices);
    public interface IBluetoothService
    {
        event DeviceDiscoveredHandler DeviceDiscovered;
        event EventHandler DeviceDiscoveryComplete;

        Stream InputStream { get; }
        Stream OutputStream { get; }
        //Stream GetStream(IDevice device);

        void StartDiscovery();
        void Connect(IDevice device);
        void StopDiscovery();
        void Disconnect(IDevice device);
        //void GetSignalStrength(string name, Action<int> signalStrengthCallback);
    }
}