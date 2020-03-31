using System;
using System.ComponentModel;
using PhxGauging.Common.Services;

namespace PhxGauging.Common.Devices.Services
{
    public interface IPhxDeviceService : IAnalyzerDeviceService
    {
        event EventHandler<DataPolledEventArgs> PhxDataPolled;
        int UseAvgPerc { get; set; }
        int LongAverageCount { get; set; }
        int ShortAverageCount { get; set; }

        void SetPpmCalibration(int indexNumber, int ppmTenths);
        void SetPpmCalibration(int indexNumber, int ppmTenths, int picoampsTenths, ushort H2Pressure, bool overwrite);
        void GenerateCalibration(int ppm);

        void TurnOnPump();
        void TurnOffPump();

        Phx21Status GetStatus();
        void WriteMemory(byte[] bytes);
        byte[] ReadMemory();
        void Start(bool useGlowPlugB);
        event EventHandler<WriteFlashProgressEventArgs> WriteFlashProgress;
        event EventHandler<ReadFlashProgressEventArgs> ReadFlashProgress;
        void WriteMemoryWithLengthAsync(byte[] bytes, Action callback, Action<Exception> errorCallback);
        void ReadMemoryAsync(Action<byte[]> callback);
        void WriteMemoryWithLength(byte[] bytes);
        bool IsPollingData { get; }
        void WriteToPhxLog(string text);
        string GetFirmwareVersion();
        PpmCalibrationInfo GetCalibration(int index);
    }
}