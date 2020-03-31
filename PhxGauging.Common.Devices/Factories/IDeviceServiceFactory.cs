using PhxGauging.Common.IO;
using PhxGauging.Common.Services;

namespace PhxGauging.Common.Devices.Factories
{
    public interface IDeviceServiceFactory
    {
        bool CanCreateServiceFrom(string deviceName);
        IDeviceService Create(IBluetoothService bluetoothService, IDevice device, IFileManager fileManager);
    }
}