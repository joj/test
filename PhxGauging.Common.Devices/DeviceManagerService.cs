using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhxGauging.Common.Devices.Factories;
using PhxGauging.Common.Devices.Services;
using PhxGauging.Common.IO;
using PhxGauging.Common.Services;

namespace PhxGauging.Common.Devices
{
    public class DeviceManagerService : IDeviceManagerService
    {
        private readonly IBluetoothService bluetoothService;
        private readonly Func<IBluetoothService> bluetoothServiceFunc;
        private readonly IFileManager fileManager;
        private List<IDeviceServiceFactory> serviceFactories;
        public event DevicesFoundHandler DevicesFound;
        public event DeviceConnectedHandler DeviceConnected;
        public event DeviceDisconnectedHandler DeviceDisconnected;
        public event EventHandler DevicesFoundComplete;

        public bool IsSearching { get; protected set; }

        public IDeviceService[] ConnectedDevices { get; private set; }

        public IDeviceService[] Devices { get; private set; }

        public DeviceManagerService(Func<IBluetoothService> bluetoothServiceFunc, IFileManager fileManager,
            IEnumerable<IDeviceServiceFactory> deviceServiceFactories)
        {
            this.bluetoothServiceFunc = bluetoothServiceFunc;
            bluetoothService = bluetoothServiceFunc();

            serviceFactories = deviceServiceFactories.ToList();

            this.fileManager = fileManager;
            bluetoothService.DeviceDiscovered += BluetoothServiceOnDeviceDiscovered;
            bluetoothService.DeviceDiscoveryComplete += BluetoothServiceDeviceDiscoveryComplete;

            ConnectedDevices = new IDeviceService[0];
            Devices = new IDeviceService[0];
        }

        private void BluetoothServiceDeviceDiscoveryComplete(object sender, EventArgs e)
        {
            OnDevicesFoundComplete();
            IsSearching = false;
        }

        private void BluetoothServiceOnDeviceDiscovered(IEnumerable<IDevice> devices)
        {
            List<IDeviceService> deviceServices = new List<IDeviceService>(Devices);

            bool anychange = false;

                foreach (var device in devices)
                {
                    var deviceName = device.Name;

                    if (!device.Name.ToLower().Contains("phx"))
                        continue;

                    IDeviceServiceFactory chosenFactory = null;

                    foreach (var deviceServiceFactory in serviceFactories)
                    {
                        if (deviceServiceFactory.CanCreateServiceFrom(deviceName))
                        {
                            chosenFactory = deviceServiceFactory;
                            break;
                        }
                    }

                    if (chosenFactory == null)
                        return;


                    IDeviceService phxDeviceService = new PhxDeviceService(bluetoothServiceFunc(), device, fileManager);

                    //TODO: Is this correct?  Maybe multiple names mean multiple protocols
                    if (deviceServices.Any(a => a.Name == deviceName))
                        continue;

                    deviceServices.Add(phxDeviceService);
                    anychange = true;

                    foreach (IDeviceService deviceService in deviceServices)
                    {
                        deviceService.Connected -= DeviceOnConnected;
                        deviceService.Connected += DeviceOnConnected;

                        deviceService.Disconnected -= DeviceOnDisconnected;
                        deviceService.Disconnected += DeviceOnDisconnected;
                    }

                    //OnDevicesFound(new DevicesFoundEventArgs(new[] {phxDeviceService}));
                }
            

            if (anychange)
            {
                Devices = deviceServices.ToArray();
                OnDevicesFound(new DevicesFoundEventArgs(Devices));
            }
        }

        private void DeviceOnDisconnected(DisconnectedEventArgs args)
        {
            var deviceServices = ConnectedDevices.ToList();
            deviceServices.Remove(args.DisconnectedDevice);

            ConnectedDevices = deviceServices.ToArray();
            OnDeviceDisconnected(args.DisconnectedDevice);
        }

        public void FindDevices()
        {
            if (IsSearching) return;

            IsSearching = true;
            Devices = new IDeviceService[0];
            new Task(() => bluetoothService.StartDiscovery()).Start();
        }

        public void StopFindingDevices()
        {
            bluetoothService.StopDiscovery();
            IsSearching = false;
        }

        private void DeviceOnConnected(ConnectedEventArgs args)
        {
            var deviceServices = ConnectedDevices.ToList();
            deviceServices.Add(args.ConnectedDevice);

            ConnectedDevices = deviceServices.ToArray();
            OnDeviceConnected(args.ConnectedDevice);
        }

        protected virtual void OnDevicesFound(DevicesFoundEventArgs args)
        {
            var handler = DevicesFound;
            if (handler != null) handler(args);
        }

        protected virtual void OnDeviceConnected(IDeviceService device)
        {
            var handler = DeviceConnected;
            if (handler != null) handler(device);
        }

        protected virtual void OnDeviceDisconnected(IDeviceService device)
        {
            var handler = DeviceDisconnected;
            if (handler != null) handler(device);
        }

        protected virtual void OnDevicesFoundComplete()
        {
            EventHandler handler = DevicesFoundComplete;
            IsSearching = false;
            if (handler != null) handler(this, EventArgs.Empty);
        }

    }
}