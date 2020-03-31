using System;
using PhxGauging.Common.Devices.Services;
using PhxGauging.Common.IO;
using PhxGauging.Common.Services;

namespace PhxGauging.Common.Devices.Factories
{
    public class PhxDeviceServiceFactory : IDeviceServiceFactory
    {
        private readonly Func<IBluetoothService, IDevice, IFileManager, IPhxDeviceService> phxDeviceServiceFunc;

        public PhxDeviceServiceFactory(Func<IBluetoothService, IDevice, IFileManager, IPhxDeviceService> phxDeviceServiceFunc)
        {
            this.phxDeviceServiceFunc = phxDeviceServiceFunc;
        }

        public bool CanCreateServiceFrom(string deviceName)
        {
            return deviceName.ToLower().Contains("phx");
        }

        public IDeviceService Create(IBluetoothService bluetoothService, IDevice device, IFileManager fileManager)
        {
            return phxDeviceServiceFunc(bluetoothService, device, fileManager);
        }
    }
}