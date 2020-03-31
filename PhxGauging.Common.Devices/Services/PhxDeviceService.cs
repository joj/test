using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using PhxGauging.Common.IO;
using PhxGauging.Common.Services;

#if  COMPACT_FRAMEWORK
using System.Net.Sockets;
#endif

namespace PhxGauging.Common.Devices.Services
{
    public class PhxDeviceService : IPhxDeviceService
    {
        private readonly IBluetoothService bluetoothService;
        private readonly IDevice device;
        private readonly IFileManager fileManager; 
        private Stream stream;
        private bool isRunning;

        private Phx21 phx21;
        public string Name { get; private set; }
        public event ConnectedHandler Connected;
        public event DisconnectedHandler Disconnected;

        public event EventHandler StatusChanged;
        public event EventHandler<DataPolledEventArgs> PhxDataPolled;
        public event EventHandler<WriteFlashProgressEventArgs> WriteFlashProgress;

        protected virtual void OnWriteFlashProgress(WriteFlashProgressEventArgs e)
        {
            var handler = WriteFlashProgress;
            if (handler != null) handler(this, e);
        }

        public event EventHandler<ReadFlashProgressEventArgs> ReadFlashProgress;

        protected virtual void OnReadFlashProgress(ReadFlashProgressEventArgs e)
        {
            var handler = ReadFlashProgress;
            if (handler != null) handler(this, e);
        }


        /// <summary>
        /// only valid after Connect() when IsConnected == true
        /// </summary>
        public Stream Stream{ get { return stream; } }

        public bool IsPollingData { get; set; }

        public Phx21 Phx21 { get { return phx21; } }

        public string Status
        {
            get { return status; }
            protected set
            {
                status = value;

                OnStatusChanged();
            }
        }

        public int UseAvgPerc
        {
            get { return phx21.UseAvgPerc; }
            set { phx21.UseAvgPerc = value; }
        }

        public int LongAverageCount
        {
            get { return phx21.LongAverageCount; }
            set { phx21.LongAverageCount = value; }
        }

        public int ShortAverageCount
        {
            get { return phx21.ShortAverageCount; }
            set { phx21.ShortAverageCount = value; }
        }

        public bool IsConnected
        {
            get { return isConnected; }
            private set { isConnected = value; }
        }

        public event ReadingUpdatedHandler ReadingUpdated;

        protected virtual void InvokeReadingUpdated(ReadingUpdatedEventArgs args)
        {
            ReadingUpdatedHandler updated = ReadingUpdated;
            if (updated != null) updated(args);
        }

        public bool IsRunning
        {
            get { return isRunning;}
            set
            {
                if (Equals(isRunning, value))
                    return;

                isRunning = value;

                OnPropertyChanged("IsRunning");
            }
        }

        public void ConfigureLogging()
        {
            if (Phx21 == null)
                throw new Exception("Phx21 is not connected.  Connect to the device first.");

            Phx21.ConfigureLogging();
        }

        public void ConfigureLogging(string loggingDirectory)
        {
            if (Phx21 == null)
                throw new Exception("Phx21 is not connected.  Connect to the device first.");

            Phx21.ConfigureLogging(loggingDirectory);
        }

        public void UseScheduler(RepeatedTaskScheduler scheduler)
        {
            Phx21.UseScheduler(scheduler);
        }

        public void StartPollingData()
        {
            if (Phx21 == null)
                throw new Exception("Phx21 is not connected.  Connect to the device first.");

            Phx21.DataPolled -= Phx21OnDataPolled;
            Phx21.DataPolled += Phx21OnDataPolled;

            IsPollingData = true;

            Phx21.StartPollingData();
        }

        public void StopPollingData()
        {
            if (Phx21 == null)
                throw new Exception("Phx21 is not connected.  Connect to the device first.");

            Phx21.DataPolled -= Phx21OnDataPolled;

            IsPollingData = false;

            Phx21.StopPollingData();
        }

        public double LastBackgroundReading { get; set; }

        private bool isConnected;
        private string status;

        public PhxDeviceService(IBluetoothService bluetoothService, IDevice device, IFileManager fileManager)
        {
            this.bluetoothService = bluetoothService;
            this.device = device;
            this.fileManager = fileManager;

            Name = device.Name;
        }

        public void Connect()
        {
            if (IsConnected)
                return;

            //stream = bluetoothService.Connect(device);
            bluetoothService.Connect(device);
#if COMPACT_FRAMEWORK
            phx21 = new Phx21(new NetworkStreamAdapter(stream as NetworkStream),
                new NetworkStreamAdapter(stream as NetworkStream), 
                fileManager, 
                device.Name);
#else
            phx21 = new Phx21(new StreamAdapter(bluetoothService.InputStream),
                new StreamAdapter(bluetoothService.OutputStream),
                fileManager,
                device.Name);
#endif

            phx21.Error += Phx21OnError;
            phx21.WriteFlashProgress += Phx21OnWriteFlashProgress;
            phx21.ReadFlashProgress += Phx21OnReadFlashProgress;
            
            IsConnected = true;
            Status = "Connected";       
            OnConnected(new ConnectedEventArgs(this));
        }

        private void Phx21OnReadFlashProgress(object sender, ReadFlashProgressEventArgs readFlashProgressEventArgs)
        {
            OnReadFlashProgress(readFlashProgressEventArgs);
        }

        private void Phx21OnWriteFlashProgress(object sender, WriteFlashProgressEventArgs writeFlashProgressEventArgs)
        {
            OnWriteFlashProgress(writeFlashProgressEventArgs);
        }

        private void Phx21OnError(object sender, ErrorEventArgs errorEventArgs)
        {
            if (errorEventArgs.Exception.Message.Contains("No data available to read"))
            {
                //Disconnect();

                //string oldLoggingDir = phx21.LoggingDirectory;

                //Connect();

                //if (IsPollingData)
                //{
                //    ConfigureLogging(oldLoggingDir);
                //    StartPollingData();
                //}
                   
            }
            else if (errorEventArgs.Exception.Message.Contains("WSACancelBlockingCall"))
            {
                // do nothing, the socket has closed
            }
            else
            {
                //throw errorEventArgs.Exception;
            }
        }

        private void Phx21OnDataPolled(object sender, DataPolledEventArgs dataPolledEventArgs)
        {
            if((DateTime.Now - DateTime.Parse(dataPolledEventArgs.Status.Timestamp)).TotalSeconds > 1)
                return;
            IsRunning = dataPolledEventArgs.Status.IsIgnited;

            try
            {
                OnPhxDataPolled(dataPolledEventArgs);
            }
            catch (Exception ex)
            {
                //TODO: log error?
            }

            
        }

        public override string ToString()
        {
            return Name;
        }


        public void SetPpmCalibration(int indexNumber, int ppmTenths)
        {
            phx21.SetPpmCalibration(indexNumber, ppmTenths);
        }

        public void SetPpmCalibration(int indexNumber, int ppmTenths, int picoampsTenths, ushort H2Pressure, bool overwrite)
        {
            phx21.SetPpmCalibration(indexNumber, ppmTenths, picoampsTenths, H2Pressure, overwrite);
        }

        public void GenerateCalibration(int ppm)
        {
            phx21.GeneratePpmCalibration(ppm);
        }

        public void TurnOnPump()
        {
            phx21.TurnOnPumpToTargetPressure(1.75);
        }

        public void TurnOffPump()
        {
            phx21.TurnOffPump();
        }

        public Phx21Status GetStatus()
        {
            Phx21Status status = phx21.ReadDataExtended();

            InvokeReadingUpdated(new ReadingUpdatedEventArgs(status.Ppm));

            return status;
        }

        public void WriteToPhxLog(string text)
        {
            phx21.WriteToPhxLog(text);
        }

        public void WriteMemoryWithLength(byte[] bytes)
        {
            phx21.WriteDataWithLength(0, bytes);
        }

        public void WriteMemory(byte[] bytes)
        {
            phx21.WriteData(bytes);
        }

        public byte[] ReadMemory()
        {
            return phx21.ReadDataFromStoredLength(0);
        }

        public PpmCalibrationInfo GetCalibration(int index)
        {
            return phx21.GetPpmCalibration(index);
        }

        public void WriteMemoryWithLengthAsync(byte[] bytes, Action callback, Action<Exception> errorCallback)
        {
            Task task = new Task(() =>
            {
                try
                {
                    WriteMemoryWithLength(bytes);
                    callback();
                }
                catch (Exception ex)
                {
                    errorCallback(ex);
                }
            });

            task.Start();
        }

        public void ReadMemoryAsync(Action<byte[]> callback)
        {
            Task task = new Task(() =>
            {
                byte[] bytes = ReadMemory();
                callback(bytes);
            });

            task.Start();
        }

        public void Start()
        {
            Start(false);
        }

        public void Start(bool useGlowPlugB)
        {
            phx21.IgniteOn(useGlowPlugB);
        }

        public string GetFirmwareVersion()
        {
            return phx21.GetFirmwareVersion();
        }

        public void Stop()
        {
            phx21.IgniteOff();
            this.phx21.Ignite(false, 0);
            this.phx21.Ignite(false, 1);
            //continueReading = false;
            Status = "Connected";
        }

        public void Disconnect()
        {
            if (!IsConnected)
                return;
            try
            {
                phx21.StopPollingData();
                bluetoothService.Disconnect(device);
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Error trying to disconnect: " + ex.Message);

                //do nothing, I guess.....
            }
            IsConnected = false;
            Status = "Disconnected";

            OnDisconnected(new DisconnectedEventArgs(this));
        }


        protected virtual void OnConnected(ConnectedEventArgs args)
        {
            var handler = Connected;
            if (handler != null) handler(args);
        }

        protected void OnDisconnected(DisconnectedEventArgs args)
        {
            DisconnectedHandler handler = Disconnected;
            if (handler != null) handler(args);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnStatusChanged()
        {
            var handler = StatusChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        protected virtual void OnPhxDataPolled(DataPolledEventArgs e)
        {
            var handler = PhxDataPolled;
            if (handler != null) handler(this, e);

            InvokeReadingUpdated(new ReadingUpdatedEventArgs(e.Status.Ppm));
        }
    }
}