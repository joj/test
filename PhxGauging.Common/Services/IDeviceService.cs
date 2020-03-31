using System;
using System.ComponentModel;

namespace PhxGauging.Common.Services
{
    public delegate void ReadingUpdatedHandler(ReadingUpdatedEventArgs args);

    public delegate void ConnectedHandler(ConnectedEventArgs args);

    public delegate void DisconnectedHandler(DisconnectedEventArgs args);

    public interface IDeviceService : INotifyPropertyChanged
    {
        string Name { get; }
        event ConnectedHandler Connected;
        event DisconnectedHandler Disconnected;
        event EventHandler StatusChanged;
        string Status { get; }
        bool IsConnected { get; }
        void Connect();
        void Disconnect();
    }

    public interface IAnalyzerDeviceService : IDeviceService
    {
        double LastBackgroundReading { get; set; }
        event ReadingUpdatedHandler ReadingUpdated;
        bool IsRunning { get; }
        void ConfigureLogging();
        void ConfigureLogging(string loggingDirectory);
        void UseScheduler(RepeatedTaskScheduler scheduler);
        void StartPollingData();
        void StopPollingData();
        void Start();
        void Stop();
    }

    public class ConnectedEventArgs
    {
        public IDeviceService ConnectedDevice { get; set; }

        public ConnectedEventArgs(IDeviceService connectedDevice)
        {
            ConnectedDevice = connectedDevice;
        }
    }

    public class DisconnectedEventArgs
    {
        public IDeviceService DisconnectedDevice { get; set; }

        public DisconnectedEventArgs(IDeviceService connectedDevice)
        {
            DisconnectedDevice = connectedDevice;
        }
    }

    public class ReadingUpdatedEventArgs
    {
        public double Reading { get; set; }

        public ReadingUpdatedEventArgs(double reading)
        {
            Reading = reading;
        }
    }
}