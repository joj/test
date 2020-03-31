using System;

namespace PhxGauging.Common.Services
{
    public delegate void DevicesFoundHandler(DevicesFoundEventArgs args);

    public delegate void DeviceConnectedHandler(IDeviceService analyzer);

    public delegate void DeviceDisconnectedHandler(IDeviceService analyzer);

    public interface IDeviceManagerService
    {
        event DevicesFoundHandler DevicesFound;
        event DeviceConnectedHandler DeviceConnected;
        event DeviceDisconnectedHandler DeviceDisconnected;
        event EventHandler DevicesFoundComplete;

        bool IsSearching { get; }

        IDeviceService[] ConnectedDevices { get;  }

        IDeviceService[] Devices { get; }

        void FindDevices();
        void StopFindingDevices();
    }

    public class DevicesFoundEventArgs
    {
        public IDeviceService[] Devices { get; set; }

        public DevicesFoundEventArgs(IDeviceService[] devices)
        {
            Devices = devices;
        }
    }
}