using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using PhxGauging.Common.IO;
using PhxGauging.CommonDevices;

namespace PhxGauging.Common.Devices
{
    public class DataPolledEventArgs : EventArgs
    {
        public Phx21Status Status { get; protected set; }

        public DataPolledEventArgs(Phx21Status status)
        {
            Status = status;
        }
    }

    public class ErrorEventArgs : EventArgs
    {
        public Exception Exception { get; set; }

        public ErrorEventArgs(Exception exception)
        {
            Exception = exception;
        }
    }

    public class WriteFlashProgressEventArgs : EventArgs
    {
        public int Progress { get; set; }

        public WriteFlashProgressEventArgs(int progress)
        {
            Progress = progress;
        }
    }

    public class ReadFlashProgressEventArgs : EventArgs
    {
        public int Progress { get; set; }

        public ReadFlashProgressEventArgs(int progress)
        {
            Progress = progress;
        }
    }

    /// <summary>
    /// This class handles all of the low level serial communication with a phx21.
    /// It was ported from a c++ project, which explains some of the way things
    /// are layed out and named.
    /// 
    /// To command a phx21 you have to send the correct command byte (they're all defined as CMD_FIDM_*)
    /// along with the appropriate command parameters struct. All (or most maybe?) commands will
    /// elicit a response from the phx21.
    /// 
    /// Command responses from the phx21 come back over serial and are also defined structs.
    /// All responses start with the byte SYNC_CODE_RES. The next byte is the response length and the
    /// third is the command byte (CMD_FIDM_*) that the response matches to. Subsequent data varies and is as
    /// defined in the struct that matches to the type of command sent.
    /// 
    /// For example, to get the firmware version number:
    /// Send:       Byte CMD_FIDM_CONFIGURATION_READ with a ConfigurationReadParams struct
    /// Receive:    A ConfigurationResponse struct where the first byte is SYNC_CODE_RES.
    ///             The second byte is length 41 which matches the length of the ConfigurationResponse struct
    ///             The third byte is CMD_FIDM_CONFIGURATION_READ.
    ///             The rest of the bytes are the data for the struct, one of which is the firmware version
    /// </summary>
    public sealed class Phx21
    {
        private Timer PollingTimer;
        private Timer LoggingTimer;
        public bool IsLoging;
        public int LogTimeInterval;

        #region Define Constants

        private const int FLASH_BLOCK_READ_TIMEOUT = 1000;
        private const byte MAX_CMD_LENGTH_BYTES = 255;
        private const int CMD_START_TIMEOUT_MS = 300;

        /// <summary>
        /// Sync codes, these signal the start of a new message
        /// </summary>
        private const byte SYNC_CODE_CMD = 0x5A;
        private const byte SYNC_CODE_RES = 0xA5;

        private const byte ERROR_LIST_LENGTH = 4;

        /// <summary>
        /// Field positions common to all messages received
        /// </summary>
        private const byte FIELD_SYNC_CODE = 0;
        private const byte FIELD_LENGTH_BYTES = 1;
        private const byte FIELD_CMD_ID = 2;

        /// <summary>
        /// command bytes
        /// </summary>
        private const byte CMD_FIDM_NO_OP = 0x00;
        private const byte CMD_FIDM_PUMP_CONTROL = 0x01;
        private const byte CMD_FIDM_SOLENOID_CONTROL = 0x02;
        private const byte CMD_FIDM_IGNITE_PULSE = 0x03;
        private const byte CMD_FIDM_SET_SAMPLING_PARAMETERS = 0x04;
        private const byte CMD_FIDM_READ_DATA = 0x05;
        private const byte CMD_FIDM_RESET_FIRMWARE = 0x06;
        private const byte CMD_FIDM_FLASH_WRITE = 0x07;
        private const byte CMD_FIDM_FLASH_READ = 0x08;
        private const byte CMD_FIDM_FLASH_ERASE = 0x09;
        private const byte CMD_FIDM_CONFIGURATION_READ = 0x0A;
        private const byte CMD_FIDM_DEBUG = 0x0B;
        private const byte CMD_FIDM_INTEGRATION_CONTROL = 0x0C;
        private const byte CMD_FIDM_HIGH_VOLTAGE_ON_OFF = 0x0D;
        private const byte CMD_FIDM_FIDM_CONFIGURATION_READ = 0x0E;
        private const byte CMD_FIDM_SET_BT_WATCHDOG = 0x0F;
        private const byte CMD_FIDM_SET_TC_CALIB_LO = 0x10;
        private const byte CMD_FIDM_SET_TC_CALIB_HI = 0x11;
        private const byte CMD_FIDM_SET_TM_CALIB_LO = 0x12;
        private const byte CMD_FIDM_SET_TM_CALIB_HI = 0x13;
        private const byte CMD_FIDM_FLASH_START_STREAM_WRITE = 0x14;
        private const byte CMD_FIDM_FLASH_WRITE_STREAM_DATA = 0x15;
        private const byte CMD_FIDM_FLASH_STOP_STREAM_WRITE = 0x16;
        private const byte CMD_FIDM_FLASH_START_STREAM_READ = 0x17;
        private const byte CMD_FIDM_FLASH_STOP_STREAM_READ = 0x18;
        private const byte CMD_FIDM_NEEDLE_VALVE_STEP = 0x19;
        private const byte CMD_FIDM_GET_SYSTEM_CURRENT = 0x1A;
        private const byte CMD_FIDM_PUMP_AUX_1_CONTROL = 0x1B;
        private const byte CMD_FIDM_SET_PUMPA_CTRL_PARAMS = 0x1C;
        private const byte CMD_FIDM_SET_PUMPA_CLOSED_LOOP = 0x1D;
        private const byte CMD_FIDM_SET_DEADHEAD_PARAMS = 0x1E;
        private const byte CMD_FIDM_GET_ERROR_LIST = 0x1F;
        private const byte CMD_FIDM_AUTO_IGNITION_SEQUENCE = 0x20;
        private const byte CMD_FIDM_GENERATE_PPM_CALIBRATION = 0x21;
        private const byte CMD_FIDM_SET_PPM_CALIBRATION = 0x22;
        private const byte CMD_FIDM_GET_PPM_CALIBRATION = 0x23;
        private const byte CMD_FIDM_SET_CAL_H2PRES_COMPENSATION = 0x24;
        private const byte CMD_FIDM_READ_DATA_EXTENDED = 0x25;
        private const byte LAST_VALID_CMD = 0x25;

        private const byte STATUS_PUMP_A_ON = 0x01;
        private const byte STATUS_PUMP_B_ON = 0x02;
        private const byte STATUS_SOLENOID_A_ON = 0x04;
        private const byte STATUS_SOLENOID_B_ON = 0x08;
        private const byte STATUS_GLOW_PLUG_A_ON = 0x10;
        private const byte STATUS_GLOW_PLUG_B_ON = 0x20;
        private const byte STATUS_HV_ON = 0x40;
        private const byte STATUS_NEW_ERROR = 0x80;

        private const byte ERROR_NO_ERROR = 0x00;
        private const byte ERROR_UNKNOWN_CMD = 0xFF;
        private const byte ERROR_INCORRECT_NUM_PARAMS = 0xFE;
        private const byte ERROR_INVALID_PARAM = 0xFD;
        private const byte ERROR_FLASH_STREAM_SEQUENCE_LOST = 0xFC;
        private const byte ERROR_NEEDLE_VALVE_MOVING = 0xFB;
        private const byte ERROR_BATT_TOO_LOW = 0xFA;
        private const byte ERROR_NO_EMPTY_CAL_SLOTS = 0xF9;

        private const byte ERROR_DEAD_HEAD = 1;
        private const byte ERROR_IGN_SEQ_FAILED_PRES = 2;
        private const byte ERROR_IGN_SEQ_FAILED_TEMP = 3;

        private const byte RANGE_MODE_0_LO = 0;
        private const byte RANGE_MODE_1_MID = 1;
        private const byte RANGE_MODE_2_HI = 2;
        private const byte RANGE_MODE_3_MAX = 3;

        private const byte FLAG_RES_RECEIVING_RESPONSE = 0x01;
        private const byte FLAG_RES_COMPLETE = 0x02;
        private const byte FLAG_RES_CRC_VALID = 0x04;
        private const byte FLAG_RES_CORRECT_NUM_RESULTS = 0x08;
        private const byte FLAG_RES_KNOWN_RES = 0x10;

        private const byte MAX_FLASH_BYTES_PER_OP = 192;

        /// <summary>
        /// States for used while receiving data
        /// </summary>
        private const byte STATE_WAITING_FOR_SYNC_CODE = 0;
        private const byte STATE_WAITING_FOR_LENGTH = 1;
        private const byte STATE_WAITING_FOR_RESPONSE_ID = 2;
        private const byte STATE_WAITING_FOR_RESPONSE_DATA = 3;
        public const byte STATE_RESPONSE_COMPLETE = 4;

        public const byte PID_LOG_SIZE = 5;
        private int junkDataCount = 0;

        #endregion Define Constants

        public event EventHandler<ErrorEventArgs> Error;
        public event EventHandler WriteFlashComplete;

        private void OnWriteFlashComplete()
        {
            var handler = WriteFlashComplete;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler ReadFlashComplete;

        private void OnReadFlashComplete()
        {
            var handler = ReadFlashComplete;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler<WriteFlashProgressEventArgs> WriteFlashProgress;

        private void OnWriteFlashProgress(int progress)
        {
            var handler = WriteFlashProgress;
            if (handler != null) handler(this, new WriteFlashProgressEventArgs(progress));
        }

        public event EventHandler<ReadFlashProgressEventArgs> ReadFlashProgress;

        private void OnReadFlashProgress(int progress)
        {
            var handler = ReadFlashProgress;
            if (handler != null) handler(this, new ReadFlashProgressEventArgs(progress));
        }

        private void OnError(ErrorEventArgs e)
        {
            if (e.Exception.Message.Contains("No data available to read"))
            {
                EventHandler<ErrorEventArgs> handler = Error;
                if (handler != null) handler(this, e);
            }
            else
            {
                //throw e.Exception;
            }
        }

        public event EventHandler<DataPolledEventArgs> DataPolled;

        private readonly IInputStream inputStream;
        private readonly IOutputStream outputStream;
        private readonly IFileManager fileManager;

        private Task loggingTask = null;
        private bool isLoggingConfigured;

        private int pollingInterval = 100;
        private int loggingInterval = 500;

        private DateTime pollingDateTime;
        private DateTime loggingDateTime;
        public string LogFilePath { get; private set; }

        private RepeatedTaskScheduler scheduler;

        public Phx21Status CurrentStatus { get; private set; }

        public string Name { get; set; }
        public bool IsRunning { get; set; }
        public string Status { get; set; }
        public int UseAvgPerc { get; set; }
        public int LongAverageCount { get; set; }
        public int ShortAverageCount { get; set; }
        public int AverageCutoffPpm { get; set; }

        private byte currentHardwareAvg = 10;

        public Phx21(IInputStream inputStream, IOutputStream outputStream, IFileManager fileManager, string name)
        {
            this.inputStream = inputStream;
            this.outputStream = outputStream;
            this.fileManager = fileManager;

            UseAvgPerc = 10;
            LongAverageCount = 25;
            ShortAverageCount = 5;
            AverageCutoffPpm = 40;

            Name = name;

            InitPollingAndLoggingActions();

            SetSamplingParameters(RANGE_MODE_0_LO);
            //second to last is the # samples for hw averaging
            SetIntegrationControlParams(0, 1, 7, 50000, currentHardwareAvg, 0);
            SetDeadHeadParams(true, 150, 100);
            SetCalH2PressureCompensation((long)(10000 * (-0.3)), (long)(10000 * 0.3));
        }

        private void InitPollingAndLoggingActions()
        {
            pollingAction = () =>
            {
                try
                {
                    CurrentStatus = ReadDataExtended();
                }
                catch (Exception ex)
                {
                    return;
                }

                if (!IsRunning)
                {
                    if (CurrentStatus.IsIgnited)
                    {
                        IsRunning = true;
                        Status = "Ignited";
                    }
                }
                else
                {
                    if (!CurrentStatus.IsIgnited)
                    {
                        IsRunning = false;
                        Status = "Connected";
                    }
                }

                DateTime now = DateTime.Now;

                OnDataPolled(new DataPolledEventArgs(CurrentStatus));
                pollingDateTime = now;

            };

            loggingAction = () =>
            {
                if (CurrentStatus == null)
                    return;
                try
                {
                    AppendToFile(LogFilePath, GetLineForLog(CurrentStatus));
                }
                catch (Exception ex)
                {
                    // we can't really log it now, can we?
                }

            };
        }

        public void UseScheduler(RepeatedTaskScheduler scheduler)
        {
            this.scheduler = scheduler;
        }

        public void StartPollingData()
        {
            StartPollingData(100);
        }

        private int changeCount = 0;

        public void StartPollingData(int intervalInMilliseconds)
        {
            if (isLoggingConfigured != true)
            {
                throw new Exception("Logging is not configured.  Please call ConfigureLogging before polling data.");
            }

            pollingInterval = intervalInMilliseconds;

            if (LoggingInterval < pollingInterval)
            {
                LoggingInterval = pollingInterval;
            }

            if (scheduler != null)
            {
                RegisterPollingInScheduler();
                RegisterLoggingInScheduler();
                return;
            }

            PollingTimer = new Timer(PollingTimerCallback, null, 250, 100);
            //判断是否需要启动状态日志记录
            if (IsLoging)
            {
                LoggingTimer = new Timer(LoggingTimerCallback, null, 250, LogTimeInterval);
            }
        }

        private void PollingTimerCallback(object stateInfo)
        {
            pollingAction();
        }

        private void LoggingTimerCallback(object stateInfo)
        {
            loggingAction();
        }

        /// <summary>
        /// Takes a Phx21Status and determines if the phx21 is ignited
        /// </summary>
        /// <param name="status">The status to check</param>
        /// <returns>A bool representing whether or not the phx21 is ignited</returns>
        private bool CheckIfIgnited(Phx21Status status)
        {
            return status.ThermoCouple > 75 && status.IsSolenoidAOn && status.IsPumpAOn;
        }

        private void RegisterPollingInScheduler()
        {
            scheduler.UnregisterAction("Phx21Polling-" + Name);
            scheduler.RegisterAction("Phx21Polling-" + Name, pollingAction, TimeSpan.FromMilliseconds(pollingInterval));
        }

        private void RegisterLoggingInScheduler()
        {
            scheduler.UnregisterAction("Phx21Logging-" + Name);
            scheduler.RegisterAction("Phx21Logging-" + Name, loggingAction, TimeSpan.FromMilliseconds(LoggingInterval));
        }

        public void StopPollingData()
        {
            if (scheduler != null)
            {
                scheduler.UnregisterAction("Phx21Polling-" + Name);
                scheduler.UnregisterAction("Phx21Logging-" + Name);
            }
            if (PollingTimer != null)
            {
                PollingTimer.Dispose();
                PollingTimer = null;
            }

            if (LoggingTimer != null)
            {
                LoggingTimer.Dispose();
                LoggingTimer = null;
            }
        }

        public void ConfigureLogging()
        {
            ConfigureLogging(LoggingInterval);
        }

        public void ConfigureLogging(int intervalInMilliseconds)
        {
            ConfigureLogging(fileManager.DataDirectory + "/LDARtools/", intervalInMilliseconds);
        }

        public void ConfigureLogging(string loggingDirectory)
        {
            ConfigureLogging(loggingDirectory, LoggingInterval);
        }

        public string LoggingDirectory { get; set; }

        public int LoggingInterval
        {
            get { return loggingInterval; }
            set { loggingInterval = value; }
        }

        public void ConfigureLogging(string loggingDirectory, int intervalInMilliseconds)
        {
            isLoggingConfigured = true;
            LoggingDirectory = loggingDirectory;

            string newFilePath = Path.Combine(loggingDirectory, GetFileName());

            CreateDirectory(loggingDirectory);
            CreateFile(newFilePath);

            LogFilePath = newFilePath;
            LoggingInterval = intervalInMilliseconds;

            if (scheduler != null)
            {
                RegisterLoggingInScheduler();
            }
        }

        public void WriteToPhxLog(string contents)
        {
            if (LogFilePath != null) AppendToFile(LogFilePath, string.Format("{0}, {1}", DateTime.Now, contents));
        }

        private void AppendToFile(string filePath, string contents)
        {
            lock (filePath)
            {
                try
                {
                    fileManager.AppendToFile(filePath, contents + "\r\n");
                }
                catch (IOException ex)
                {
                    //nothing to do... just keep rollin'
                }
            }
        }

        /// <summary>
        /// Takes a Phx21Status and returns some of the parameters in a comma delimites string.
        /// Here's the format:
        /// {0} - current date
        /// {1} - AirPressure
        /// {2} - BatteryVoltage
        /// {3} - ChamberOuterTemp
        /// {4} - IsPumpAOn
        /// {5} - PpmStr
        /// {6} - SamplePressure
        /// {7} - TankPressure
        /// {8} - ThermoCouple
        /// {9} - PumpPower
        /// {10} - FIDRange 
        /// {11} - PicoAmps
        /// </summary>
        /// <param name="status">The status you want broken out into a string</param>
        /// <returns>A comma delimited string of some status fields</returns>
        private string GetLineForLog(Phx21Status status)
        {
            return string.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}",
                DateTime.Now,
                status.AirPressure, status.BatteryVoltage, status.ChamberOuterTemp, status.IsPumpAOn,
                status.PpmStr, status.SamplePressure, status.TankPressure, status.ThermoCouple, status.PumpPower,
                status.FIDRange, status.PicoAmps);
        }

        private void CreateFile(string filePath)
        {
            if (!fileManager.FileExists(filePath))
            {
                fileManager.CreateFile(filePath);
            }
        }

        private void CreateDirectory(string loggingDirectory)
        {
            if (!fileManager.DirectoryExists(loggingDirectory))
            {
                fileManager.CreateDirectory(loggingDirectory);
            }
        }

        private string GetFileName()
        {
            return string.Format("{0}_{1}.log", Name, DateTime.Now.ToString("yyyyMMdd"));
        }

        /// <summary>
        /// Ignites the Phx21
        /// </summary>
        public void IgniteOn()
        {
            Ignite(true);
        }

        public void IgniteOn(bool useSecondaryGlowPlug)
        {
            Ignite(true, useSecondaryGlowPlug ? (byte)1 : (byte)0);
        }

        /// <summary>
        /// Extinguishes the flame of the Phx21
        /// Turns pump A and solenoid A off
        /// </summary>
        public void IgniteOff()
        {
            SetPumpACtrlLoop(false, 0);
            ControlPumpAux1(0, 0, 0);
            ControlSolenoid(0, 0);
        }

        public void SetSolenoidAOn()
        {
            ControlSolenoid(0, 100);
        }

        public void SetSolenoidAOff()
        {
            ControlSolenoid(0, 0);
        }

        public void SetSolenoidBOn()
        {
            ControlSolenoid(1, 100);
        }

        public void SetSolenoidBOff()
        {
            ControlSolenoid(1, 0);
        }

        private void OnDataPolled(DataPolledEventArgs e)
        {
            var handler = DataPolled;
            if (handler != null) handler(this, e);
        }

        private object obj = new object();
        private Action loggingAction;
        private Action pollingAction;

        /// <summary>
        /// Receives the current status of the Phx21
        /// 
        /// SENDS: CMD_FIDM_READ_DATA_EXTENDED with READ_DATA_PARAMS
        /// RECEIVES: DEFAULT_RESPONSE_EXTENDED which is then passed to GetStatusFromFidmStatusExtended()
        /// to get the Phx21Status that is returned
        /// </summary>
        /// <returns>The current status from a phx21</returns>
        public Phx21Status ReadDataExtended()
        {

            if (!isLoggingConfigured)
            {
                throw new Exception("Must call StartLogging before reading data.");
            }

            READ_DATA_PARAMS pCmd = new READ_DATA_PARAMS();
            DEFAULT_RESPONSE_EXTENDED Rsp = new DEFAULT_RESPONSE_EXTENDED();
            bool result;

            byte nLength = (byte)Marshal.SizeOf(typeof(READ_DATA_PARAMS));
            byte nCmd = CMD_FIDM_READ_DATA_EXTENDED;

            lock (obj)
            {
                outputStream.Flush();
                inputStream.Flush();
                if (TransmitSerialCmd(nCmd, GetBytes(pCmd), nLength, nLength, true) != 0)
                {
                    Rsp = ReceiveCmdResponse<DEFAULT_RESPONSE_EXTENDED>(nCmd);
                }
            }

            return GetStatusFromFidmStatusExtended(Rsp.status);
        }

        private void SetIntegrationControlParams(byte nMode, byte nChargeMultiplier, byte nRange,
            uint nIntegrationTimeUs,
            byte nSamplesToAvg, byte nReportMode)
        {
            IntegrationControlParams pCmd = new IntegrationControlParams();
            bool result = false;

            byte nLength = (byte)Marshal.SizeOf(typeof(IntegrationControlParams));
            byte nCmd = CMD_FIDM_INTEGRATION_CONTROL;

            pCmd.nMode = nMode;
            pCmd.nChargeMultiplier = nChargeMultiplier;
            pCmd.nRange = nRange;
            pCmd.nIntegrationTimeUs0 = DwordToByte0(nIntegrationTimeUs);
            pCmd.nIntegrationTimeUs1 = DwordToByte1(nIntegrationTimeUs);
            pCmd.nSamplesToAvg = nSamplesToAvg;
            pCmd.nReportMode = nReportMode;

            lock (obj)
            {
                WriteToPhxLog("SetIntegrationControlParams");
                TransmitSerialCmd(nCmd, GetBytes(pCmd), nLength, nLength, true);
                ReceiveCmdResponse<FIDM_STATUS>(nCmd);
            }
        }



        /// <summary>
        /// Gets the PPM value calibrated at the given index.
        /// 
        /// SENDS: CMD_FIDM_GET_PPM_CALIBRATION with GetCalibration args
        /// RECEIVES: GetCalibrationResponse
        /// 
        /// The GetCalibrationResponse object is used to create PpmCalibrationInfo that is returned
        /// </summary>
        /// <param name="index">the calibration index you wish to receive</param>
        /// <returns>Calibration info</returns>
        public PpmCalibrationInfo GetPpmCalibration(int index)
        {
            GetCalibration Cmd = new GetCalibration();
            GetCalibrationResponse Rsp;

            byte nLength = (byte)Marshal.SizeOf(typeof(GetCalibration));
            byte nCmd = CMD_FIDM_GET_PPM_CALIBRATION;

            Cmd.index_number = (byte)index;

            lock (obj)
            {
                outputStream.Flush();
                inputStream.Flush();
                WriteToPhxLog("GetPpmCalibration");
                TransmitSerialCmd(nCmd, GetBytes(Cmd), nLength, nLength, true);
#if COMPACT_FRAMEWORK
                try
                {
                    byte[] bytes = GetResponse(nCmd);

                    Rsp = new GetCalibrationResponse();
                    Rsp.index_number = bytes[31];
                    Rsp.ppm_tenths = BytesToDword(bytes[35], bytes[34], bytes[33], bytes[32]);
                    Rsp.fid_current_tPa = BytesToDword(bytes[39], bytes[38], bytes[37], bytes[36]);
                    Rsp.H2_pressure_hPSI = (ushort)BytesToWord(bytes[41], bytes[40]);
                    Rsp.valid = bytes[42];
                }
                catch (Exception ex)
                {
                    WriteToPhxLog("GetPpmCalibration retry");
                    TransmitSerialCmd(nCmd, GetBytes(Cmd), nLength, nLength, true);

                    byte[] bytes = GetResponse(nCmd);

                    Rsp = new GetCalibrationResponse();
                    Rsp.index_number = bytes[31];
                    Rsp.ppm_tenths = BytesToDword(bytes[35], bytes[34], bytes[33], bytes[32]);
                    Rsp.fid_current_tPa = BytesToDword(bytes[39], bytes[38], bytes[37], bytes[36]);
                    Rsp.H2_pressure_hPSI = (ushort)BytesToWord(bytes[41], bytes[40]);
                    Rsp.valid = bytes[42];

                    return new PpmCalibrationInfo
                    {
                        Index = Rsp.index_number,
                        Ppm = (int)(0.1 * Rsp.ppm_tenths),
                        FidCurrent = Rsp.fid_current_tPa,
                        H2Pressure = Rsp.H2_pressure_hPSI / 100.0f,
                        IsValid = Rsp.valid > 0
                    };
                }


#else
                Rsp = ReceiveCmdResponse<GetCalibrationResponse>(nCmd);
#endif

            }

            return new PpmCalibrationInfo
            {
                Index = Rsp.index_number,
                Ppm = (int)(0.1 * Rsp.ppm_tenths),
                FidCurrent = Rsp.fid_current_tPa,
                H2Pressure = Rsp.H2_pressure_hPSI / 100.0f,
                IsValid = Rsp.valid > 0
            };
        }

        /// <summary>
        /// Convienience function for SetPpmCalibration with more args.
        /// Gets the pico amps and pressure from the current status, so if you
        /// use this make sure CurrentStatus is up to date.
        /// </summary>
        /// <param name="indexNumber">This is the calibration index, there are 6</param>
        /// <param name="ppmTenths">Tenth of ppm</param>
        public void SetPpmCalibration(int indexNumber, int ppmTenths)
        {
            SetPpmCalibration(indexNumber, ppmTenths, (int)(CurrentStatus.PicoAmps * 10),
                (ushort)(CurrentStatus.TankPressure * 10), true);
        }

        /// <summary>
        /// Sets a specific calibration slot. In general it is easier to use GeneratePpmCalibration() 
        /// or the SetPpmCalibration with less args.
        /// 
        /// SENDS: CMD_FIDM_SET_PPM_CALIBRATION with SetCalibration args
        /// RECEIVES: FIDM_STATUS. Response is ignored.
        /// </summary>
        /// <param name="indexNumber">This is the calibration index, there are 6</param>
        /// <param name="ppmTenths">Tenth of ppm</param>
        /// <param name="picoampsTenths">Tenth of pico amps</param>
        /// <param name="H2Pressure"></param>
        /// <param name="overwrite"></param>
        public void SetPpmCalibration(int indexNumber, int ppmTenths, int picoampsTenths, ushort H2Pressure,
            bool overwrite)
        {
            SetCalibration cmd = new SetCalibration();

            byte nLength = (byte)Marshal.SizeOf(typeof(SetCalibration));
            byte nCmd = CMD_FIDM_SET_PPM_CALIBRATION;

            cmd.index_number = (byte)indexNumber;
            cmd.ppm_tenths = ppmTenths;
            cmd.fid_current_tPa = picoampsTenths;
            cmd.H2_pressure_hPSI = H2Pressure;
            cmd.overwrite = overwrite ? (byte)1 : (byte)0;

            lock (obj)
            {
                WriteToPhxLog("SetPpmCalibration");
                TransmitSerialCmd(nCmd, GetBytes(cmd), nLength, nLength, true);
                ReceiveCmdResponse<FIDM_STATUS>(nCmd);
            }

        }

        /// <summary>
        /// Generates a PPM Calibration in the Phx given the ppm of the gas that is currently being processed.
        /// General use is to clear the calibrations, then use this function to generate the calibration when
        /// the different gasses are applied.
        /// 
        /// SENDS: A CMD_FIDM_GENERATE_PPM_CALIBRATION command with GenerateCalibration params
        /// RECEIVES: A FIDM_STATUS response. The response is ignored.
        /// </summary>
        /// <param name="ppm">The PPM of the gas currently being processed</param>
        public void GeneratePpmCalibration(int ppm)
        {
            GenerateCalibration cmd = new GenerateCalibration();

            byte nLength = (byte)Marshal.SizeOf(typeof(GenerateCalibration));
            byte nCmd = CMD_FIDM_GENERATE_PPM_CALIBRATION;

            cmd.ppm_tenths = ppm * 10; //to get tenths
            cmd.spare_for_alignment = 0;

            lock (obj)
            {
                WriteToPhxLog("GeneratePpmCalibration");
                TransmitSerialCmd(nCmd, GetBytes(cmd), nLength, nLength, true);
                ReceiveCmdResponse<FIDM_STATUS>(nCmd);
            }
        }
        /// <summary>
        /// 打开泵
        /// </summary>
        /// <param name="targetPressure"></param>
        public void TurnOnPumpToTargetPressure(double targetPressure)
        {
            SetPumpACtrlLoop(true, (long)(targetPressure * 100));
        }
        /// <summary>
        /// 关闭泵
        /// </summary>
        public void TurnOffPump()
        {
            SetPumpACtrlLoop(false, 0);
            ControlPumpAux1(0, 0, 0);
        }
        /// <summary>
        /// 点火预热塞1
        /// </summary>
        /// <param name="durationInSeconds"></param>
        public void IgniteGlowPlug1(int durationInSeconds)
        {
            IgniteGlowPlug(0, (byte)durationInSeconds);
        }
        /// <summary>
        /// 点火预热塞2
        /// </summary>
        /// <param name="durationInSeconds"></param>
        public void IgniteGlowPlug2(int durationInSeconds)
        {
            IgniteGlowPlug(1, (byte)durationInSeconds);
        }

        public void SetDeadHeadParams(bool enabled, ushort pressureLimit, ushort timeout)
        {
            DeadheadParams cmd = new DeadheadParams();

            byte nLength = (byte)Marshal.SizeOf(typeof(DeadheadParams));
            byte nCmd = CMD_FIDM_SET_DEADHEAD_PARAMS;

            cmd.enable = enabled ? (byte)1 : (byte)0;
            cmd.pressure_low_limit_hPSI = pressureLimit;
            cmd.max_duration_msec = timeout;

            lock (obj)
            {
                WriteToPhxLog("SetDeadHeadParams");
                TransmitSerialCmd(nCmd, GetBytes(cmd), nLength, nLength, true);
                ReceiveCmdResponse<FIDM_STATUS>(nCmd);
            }
        }

        /// <summary>
        /// Sets the LPH2 compensation.
        /// 
        /// SENDS: CMD_FIDM_SET_CAL_H2PRES_COMPENSATION with CalH2PressureCompensation params
        /// RECEIVES: FIDM_STATUS. Response is ignored.
        /// </summary>
        /// <param name="h2CompensationPos">LPH2 positive compensation</param>
        /// <param name="h2CompensationNeg">LPH2 negative compensation</param>
        public void SetCalH2PressureCompensation(long h2CompensationPos, long h2CompensationNeg)
        {
            CalH2PressureCompensation cmd = new CalH2PressureCompensation();

            byte nLength = (byte)Marshal.SizeOf(typeof(CalH2PressureCompensation));
            byte nCmd = CMD_FIDM_SET_CAL_H2PRES_COMPENSATION;

            cmd.H2_compensation_pos = h2CompensationPos;
            cmd.H2_compensation_neg = h2CompensationNeg;
            cmd.spare_for_alignment = 0;

            lock (obj)
            {
                WriteToPhxLog("SetCalH2PressureCompensation");
                TransmitSerialCmd(nCmd, GetBytes(cmd), nLength, nLength, true);
                ReceiveCmdResponse<FIDM_STATUS>(nCmd);
            }
        }

        public void ReadConfiguration()
        {
            throw new NotImplementedException();
            //ConfigurationR
        }

        private void ControlPumpAux1(byte nId, uint nPowerLevelTenthsPercent, byte nKickStartDurationSec)
        {
            PumpAux1ControlParams pCmd = new PumpAux1ControlParams();

            byte nLength = (byte)Marshal.SizeOf(typeof(PumpAux1ControlParams));
            byte nCmd = CMD_FIDM_PUMP_AUX_1_CONTROL;

            pCmd.nID = nId;
            pCmd.nPowerTenthsPercent0 = DwordToByte0(nPowerLevelTenthsPercent);
            pCmd.nPowerTenthsPercent1 = DwordToByte1(nPowerLevelTenthsPercent);
            pCmd.nKickStartDurationSec = nKickStartDurationSec;

            lock (obj)
            {
                WriteToPhxLog("ControlPumpAux1");
                TransmitSerialCmd(nCmd, GetBytes(pCmd), nLength, nLength, true);
            }

        }

        /// <summary>
        /// This function gets the firmware version from the phx21.获取固件版本
        /// 
        /// SENDS: CMD_FIDM_CONFIGURATION_READ command with ConfigurationReadParams
        /// RECEIVES: a ConfigurationResponse. The version is a field in ConfigurationResponse
        /// </summary>
        /// <returns>The firmware version number</returns>
        public string GetFirmwareVersion()
        {
            ConfigurationReadParams pCmd = new ConfigurationReadParams();

            byte nLength = (byte)Marshal.SizeOf(typeof(ConfigurationReadParams));
            byte nCmd = CMD_FIDM_CONFIGURATION_READ;

            int sanity = 0;
            bool receiveSuccess = false;

            ConfigurationResponse response = new ConfigurationResponse();

            lock (obj)
            {
                while (sanity < 10 && !receiveSuccess)
                {
                    sanity++;
                    WriteToPhxLog("GetFirmwareVersion");
                    TransmitSerialCmd(nCmd, GetBytes(pCmd), nLength, nLength, true);

                    response = ReceiveCmdResponse<ConfigurationResponse>(nCmd, out receiveSuccess);
                }
            }

            if (receiveSuccess)
            {
                return response.nVersion.ToString();
            }

            throw new Exception("Unable to read version");
        }

        /// <summary>
        /// This function is used to control the 2 solenoids
        /// 
        /// SENDS: CMD_FIDM_SOLENOID_CONTROL with SolenoidControlParams
        /// 
        /// Does not wait for a response, but methinks maybe it should?
        /// </summary>
        /// <param name="nId">Valid values are 0 for solenoid A and 1 for solenoid B</param>
        /// <param name="nPowerLevelPercent">The power level is usually set to 0 (off) or 100 (for all the way)</param>
        private void ControlSolenoid(byte nId, byte nPowerLevelPercent)
        {
            SolenoidControlParams pCmd = new SolenoidControlParams();

            byte nLength = (byte)Marshal.SizeOf(typeof(SolenoidControlParams));
            byte nCmd = CMD_FIDM_SOLENOID_CONTROL;

            pCmd.nID = nId;
            pCmd.nPower = nPowerLevelPercent;

            lock (obj)
            {
                WriteToPhxLog("ControlSolenoid");
                TransmitSerialCmd(nCmd, GetBytes(pCmd), nLength, nLength, true);
            }

        }

        private void SetPumpACtrlLoop(bool enable, long target)
        {
            PumpClosedLoop Cmd = new PumpClosedLoop();

            byte nLength = (byte)Marshal.SizeOf(typeof(PumpClosedLoop));
            byte nCmd = CMD_FIDM_SET_PUMPA_CLOSED_LOOP;

            Cmd.enable = enable ? (byte)1 : (byte)0;
            Cmd.target_hPSI = (short)target;

            lock (obj)
            {
                WriteToPhxLog("SetPumpACtrlLoop");
                TransmitSerialCmd(nCmd, GetBytes(Cmd), nLength, nLength, true);
            }

        }

        private void WriteFlash(uint startingAddress, uint nCount, byte[] data)
        {
            FlashWriteParams pCmd = new FlashWriteParams();

            byte nCmdLength = (byte)(Marshal.SizeOf(typeof(FlashWriteParams)) + MAX_FLASH_BYTES_PER_OP);
            byte nHeaderLength = (byte)Marshal.SizeOf(typeof(FlashWriteParams));

            byte nCRC = 0;
            byte nCmd = CMD_FIDM_FLASH_WRITE;
            bool result = false;

            uint nMaxNumCycles = nCount / MAX_FLASH_BYTES_PER_OP;
            byte nLastNumBytesToWrite = (byte)(nCount - (MAX_FLASH_BYTES_PER_OP * nMaxNumCycles));

            int currentProgress = 0;
            int progressInterval = nMaxNumCycles == 0 ? 0 : 100 / (int)nMaxNumCycles;

            FIDM_STATUS pStatus = new FIDM_STATUS();
            bool receiveSuccess;
            int tryCount = 0;

            lock (obj)
            {
                for (int ii = 0; ii < nMaxNumCycles; ii++)
                {
                    pCmd.nStartingAddress0 = DwordToByte0((uint)(startingAddress + (MAX_FLASH_BYTES_PER_OP * ii)));
                    pCmd.nStartingAddress1 = DwordToByte1((uint)(startingAddress + (MAX_FLASH_BYTES_PER_OP * ii)));
                    pCmd.nStartingAddress2 = DwordToByte2((uint)(startingAddress + (MAX_FLASH_BYTES_PER_OP * ii)));
                    pCmd.nStartingAddress3 = DwordToByte3((uint)(startingAddress + (MAX_FLASH_BYTES_PER_OP * ii)));
                    pCmd.nCount = MAX_FLASH_BYTES_PER_OP;

                    tryCount = 0;

                    do
                    {
                        WriteToPhxLog("WriteFlash " + ii + " of " + nMaxNumCycles + " try " + tryCount);
                        nCRC = TransmitSerialCmd(nCmd, GetBytes(pCmd), nCmdLength, nHeaderLength, false); // Don't send the CRC.
                        byte[] datatosend = data.Skip(MAX_FLASH_BYTES_PER_OP * ii).ToArray();
                        TransmitSerialData(nCRC, datatosend, MAX_FLASH_BYTES_PER_OP, true); // Send the CRC.

                        pStatus = ReceiveCmdResponse<FIDM_STATUS>(nCmd, out receiveSuccess);
                        if (!receiveSuccess)
                            Task.Delay(25).Wait();

                        tryCount++;

                        if (receiveSuccess)
                        {
                            byte[] readdata = ReadFlash((uint)(startingAddress + (MAX_FLASH_BYTES_PER_OP * ii)), MAX_FLASH_BYTES_PER_OP);

                            for (int j = 0; j < nLastNumBytesToWrite; j++)
                            {
                                if (readdata[j] != datatosend[j])
                                {
                                    receiveSuccess = false;
                                    WriteToPhxLog("WriteFlash data read does not match data written - retrying");
                                    break;
                                }
                            }
                        }

                        if (tryCount > 10 && !receiveSuccess)
                        {
                            WriteToPhxLog("WriteFlash failed after retrying " + tryCount + " times!!!");
                            throw new Exception("Unable to write flash data to phx21");
                        }

                    } while (!receiveSuccess);

                    currentProgress = (int)((100.00 / (double)nMaxNumCycles) * ii);
                    OnWriteFlashProgress(currentProgress);
                }

                nCmdLength = (byte)(Marshal.SizeOf(typeof(FlashWriteParams)) + nLastNumBytesToWrite);

                pCmd.nStartingAddress0 = DwordToByte0((uint)(startingAddress + (MAX_FLASH_BYTES_PER_OP * nMaxNumCycles)));
                pCmd.nStartingAddress1 = DwordToByte1((uint)(startingAddress + (MAX_FLASH_BYTES_PER_OP * nMaxNumCycles)));
                pCmd.nStartingAddress2 = DwordToByte2((uint)(startingAddress + (MAX_FLASH_BYTES_PER_OP * nMaxNumCycles)));
                pCmd.nStartingAddress3 = DwordToByte3((uint)(startingAddress + (MAX_FLASH_BYTES_PER_OP * nMaxNumCycles)));
                pCmd.nCount = nLastNumBytesToWrite;

                receiveSuccess = false;
                tryCount = 0;

                do
                {
                    WriteToPhxLog("WriteFlash remainder try " + tryCount);
                    nCRC = TransmitSerialCmd(nCmd, GetBytes(pCmd), nCmdLength, nHeaderLength, false);
                    // Don't send the CRC.
                    byte[] datatosend = data.Skip((int)(MAX_FLASH_BYTES_PER_OP * nMaxNumCycles)).ToArray();
                    TransmitSerialData(nCRC, datatosend, nLastNumBytesToWrite, true); // Send the CRC.

                    pStatus = ReceiveCmdResponse<FIDM_STATUS>(nCmd, out receiveSuccess);
                    if (!receiveSuccess)
                        Task.Delay(25).Wait();

                    tryCount++;

                    if (receiveSuccess)
                    {
                        byte[] readdata = ReadFlash((startingAddress + (MAX_FLASH_BYTES_PER_OP * nMaxNumCycles)), nLastNumBytesToWrite);

                        for (int j = 0; j < nLastNumBytesToWrite; j++)
                        {
                            if (readdata[j] != datatosend[j])
                            {
                                WriteToPhxLog("WriteFlash data read does not match data written - retrying");
                                receiveSuccess = false;
                                break;
                            }
                        }
                    }

                    if (tryCount > 10 && !receiveSuccess)
                    {
                        WriteToPhxLog("WriteFlash failed after retrying " + tryCount + " times!!!");
                        throw new Exception("Unable to write flash data to phx21");
                    }
                } while (!receiveSuccess);

                OnWriteFlashProgress(100);
                OnWriteFlashComplete();
            }
        }

        public byte[] ReadDataFromStoredLength(uint startingAddress)
        {
            byte[] lenBytes = ReadFlash(startingAddress, 4);

            int len = BytesToDword(lenBytes[3], lenBytes[2], lenBytes[1], lenBytes[0]);

            return ReadData(startingAddress + 4, (uint)len);
        }

        public byte[] ReadData(uint startingAddress, uint count)
        {
            byte[] compressedBytes = ReadFlash(startingAddress, count).ToArray();

            return compressedBytes;
        }

        private byte[] ReadFlash(uint startingAddress, uint nCount)
        {
            FlashReadParams pCmd = new FlashReadParams();

            byte nLength = (byte)Marshal.SizeOf(typeof(FlashReadParams));
            byte nCmd = CMD_FIDM_FLASH_READ;
            bool result = false;

            List<byte> data = new List<byte>();

            uint nMaxNumCycles = nCount / MAX_FLASH_BYTES_PER_OP;
            byte nLastNumBytesToWrite = (byte)(nCount - (MAX_FLASH_BYTES_PER_OP * nMaxNumCycles));

            DefaultResponse defResponse = new DefaultResponse();

            int currentProgress = 0;

            lock (obj)
            {
                for (int ii = 0; ii < nMaxNumCycles; ii++)
                {
                    pCmd.nStartingAddress0 = DwordToByte0((uint)(startingAddress + (MAX_FLASH_BYTES_PER_OP * ii)));
                    pCmd.nStartingAddress1 = DwordToByte1((uint)(startingAddress + (MAX_FLASH_BYTES_PER_OP * ii)));
                    pCmd.nStartingAddress2 = DwordToByte2((uint)(startingAddress + (MAX_FLASH_BYTES_PER_OP * ii)));
                    pCmd.nStartingAddress3 = DwordToByte3((uint)(startingAddress + (MAX_FLASH_BYTES_PER_OP * ii)));
                    pCmd.nCount = MAX_FLASH_BYTES_PER_OP;

                    TransmitSerialCmd(nCmd, GetBytes(pCmd), nLength, nLength, true);                     // Don't send the CRC.

                    defResponse = ReceiveCmdResponse<DefaultResponse>(nCmd);

                    for (int jj = 0; jj < MAX_FLASH_BYTES_PER_OP; jj++)
                    {
                        data.Add(m_nFlashMemContents[jj]);
                    }

                    currentProgress = (int)((100.00 / (double)nMaxNumCycles) * ii);
                    OnReadFlashProgress(currentProgress);
                }

                nLength = (byte)(Marshal.SizeOf(typeof(FlashReadParams)));

                pCmd.nStartingAddress0 = DwordToByte0((uint)(startingAddress + (MAX_FLASH_BYTES_PER_OP * nMaxNumCycles)));
                pCmd.nStartingAddress1 = DwordToByte1((uint)(startingAddress + (MAX_FLASH_BYTES_PER_OP * nMaxNumCycles)));
                pCmd.nStartingAddress2 = DwordToByte2((uint)(startingAddress + (MAX_FLASH_BYTES_PER_OP * nMaxNumCycles)));
                pCmd.nStartingAddress3 = DwordToByte3((uint)(startingAddress + (MAX_FLASH_BYTES_PER_OP * nMaxNumCycles)));
                pCmd.nCount = nLastNumBytesToWrite;

                TransmitSerialCmd(nCmd, GetBytes(pCmd), nLength, nLength, true);

                defResponse = ReceiveCmdResponse<DefaultResponse>(nCmd);

                for (int jj = 0; jj < nLastNumBytesToWrite; jj++)
                {
                    data.Add(m_nFlashMemContents[jj]);
                }
            }

            //lock (obj)
            //{
            //    for (int ii = 0; ii < nMaxNumCycles; ii++)
            //    {
            //        var timeoutThread = new MortalThread<Byte[]>
            //        (() =>
            //        {
            //            try
            //            {
            //                pCmd.nStartingAddress0 = DwordToByte0((uint) (startingAddress + (MAX_FLASH_BYTES_PER_OP*ii)));
            //                pCmd.nStartingAddress1 = DwordToByte1((uint) (startingAddress + (MAX_FLASH_BYTES_PER_OP*ii)));
            //                pCmd.nStartingAddress2 = DwordToByte2((uint) (startingAddress + (MAX_FLASH_BYTES_PER_OP*ii)));
            //                pCmd.nStartingAddress3 = DwordToByte3((uint) (startingAddress + (MAX_FLASH_BYTES_PER_OP*ii)));
            //                pCmd.nCount = MAX_FLASH_BYTES_PER_OP;

            //                TransmitSerialCmd(nCmd, GetBytes(pCmd), nLength, nLength, true); // Don't send the CRC.

            //                defResponse = ReceiveCmdResponse<DefaultResponse>(nCmd);
            //                return m_nFlashMemContents;
            //            }
            //            catch (ThreadAbortException tex)
            //            {
            //                return m_nFlashMemContents;
            //            }
            //        });
            //        timeoutThread.Start();
            //        timeoutThread.Join(FLASH_BLOCK_READ_TIMEOUT);

            //        //Debug.WriteLine("Flash block " + ii + " read completed in " + timeoutThread.CompletionTime + "ms");

            //        if (timeoutThread.Status == MortalThreadStatus.Fail)
            //        {
            //            throw new TimeoutException("Analyzer " + this.Name + " Flash Block " + ii + " read exceeded timeout of " + FLASH_BLOCK_READ_TIMEOUT + " ms.");
            //        }

            //        m_nFlashMemContents = timeoutThread.Result;

            //        for (int jj = 0; jj < MAX_FLASH_BYTES_PER_OP; jj++)
            //        {
            //            data.Add(m_nFlashMemContents[jj]);
            //        }

            //        currentProgress = (int)((100.00 / (double)nMaxNumCycles) * ii);
            //        OnReadFlashProgress(currentProgress);
            //    }

            //    nLength = (byte)(Marshal.SizeOf(typeof(FlashReadParams)));

            //    pCmd.nStartingAddress0 = DwordToByte0((uint)(startingAddress + (MAX_FLASH_BYTES_PER_OP * nMaxNumCycles)));
            //    pCmd.nStartingAddress1 = DwordToByte1((uint)(startingAddress + (MAX_FLASH_BYTES_PER_OP * nMaxNumCycles)));
            //    pCmd.nStartingAddress2 = DwordToByte2((uint)(startingAddress + (MAX_FLASH_BYTES_PER_OP * nMaxNumCycles)));
            //    pCmd.nStartingAddress3 = DwordToByte3((uint)(startingAddress + (MAX_FLASH_BYTES_PER_OP * nMaxNumCycles)));
            //    pCmd.nCount = nLastNumBytesToWrite;

            //    TransmitSerialCmd(nCmd, GetBytes(pCmd), nLength, nLength, true);

            //    defResponse = ReceiveCmdResponse<DefaultResponse>(nCmd);

            //    for (int jj = 0; jj < nLastNumBytesToWrite; jj++)
            //    {
            //        data.Add(m_nFlashMemContents[jj]);
            //    }
            //}

            OnReadFlashProgress(100);
            OnReadFlashComplete();

            return data.ToArray();
        }

        private void TransmitSerialData(byte nCurrChkSum, byte[] pBuffer, byte nLength, bool bSendCRC)
        {
            byte nCRC = ReComputeCRC(nCurrChkSum, pBuffer, nLength);

            outputStream.Write(pBuffer, 0, nLength);

            if (bSendCRC)
                outputStream.Write(new byte[] { nCRC }, 0, 1);

        }

        private byte ReComputeCRC(byte nCurrChkSum, byte[] pStream, byte nLength)
        {
            for (int ii = 0; ii < nLength; ii++)
            {
                nCurrChkSum = (byte)((nCurrChkSum << 1) | (nCurrChkSum >> 7));
                nCurrChkSum += pStream[ii];
            }

            return nCurrChkSum;
        }

        private void IgniteGlowPlug(byte nId, byte nDurationSec)
        {
            IgniteParams pCmd = new IgniteParams();

            byte nLength = (byte)Marshal.SizeOf(typeof(IgniteParams));
            byte nCmd = CMD_FIDM_IGNITE_PULSE;

            pCmd.nID = nId;
            pCmd.nDurationSec = nDurationSec;

            lock (obj)
            {
                WriteToPhxLog("IgniteGlowPlug");
                TransmitSerialCmd(nCmd, GetBytes(pCmd), nLength, nLength, true);
            }
        }

        private void SetSamplingParameters(byte nFIDMRange)
        {
            SetSamplingParams pCmd = new SetSamplingParams();

            byte nLength = (byte)Marshal.SizeOf(typeof(SetSamplingParams));
            byte nCmd = CMD_FIDM_SET_SAMPLING_PARAMETERS;

            pCmd.nRange = nFIDMRange;

            lock (obj)
            {
                WriteToPhxLog("SetSamplingParameters");
                TransmitSerialCmd(nCmd, GetBytes(pCmd), nLength, nLength, true);
                ReceiveCmdResponse<FIDM_STATUS>(nCmd);
            }
        }

        private int num0s = 0;
        private int ignitedChagedCount = 0;
        private bool prevIgnite = false;

        /// <summary>
        /// Takes a FIDM_STATUS_EXTENDED and creates a Phx21Status from it.
        /// 
        /// This is where junk data is filtered:
        /// if ((phx21Status.BatteryVoltage &gt; 15 || phx21Status.PicoAmps &lt; -10000 || phx21Status.ThermoCouple &lt; -400) && junkDataCount &lt; 10)
        /// its junk and will try to read status again.
        /// 
        /// This is also where it is determined if a phx21 is ignited if CheckIfIgnited() for this status and the last 3 status indicate ignition.
        /// 
        /// PPM ranging also happens in this function (wow, there's lots of fun stuff in here, huh?)
        /// 
        /// And ppm value averaging happens here as well
        /// 
        /// Check for pump power level > 85% is here too, shuts off pump
        /// 
        /// </summary>
        /// <param name="status">The status to convert</param>
        /// <returns>a Phx21Status from the status passed in</returns>
        private Phx21Status GetStatusFromFidmStatusExtended(FIDM_STATUS_EXTENDED status)
        {
            double ppm =
                Math.Round(
                    0.1f *
                    BytesToDword(status.nFIDTenthsPPM3, status.nFIDTenthsPPM2, status.nFIDTenthsPPM1,
                        status.nFIDTenthsPPM0), 1);

            if (ppm >= 100)
                ppm = Math.Round(ppm, 0);

            if (ppm < 0)
                ppm = 0;

            if (ppm == 0)
            {
                num0s++;

                if (num0s > 5)
                {
                    num0s = -5;
                }

                if (num0s < 0)
                {
                    ppm = 0.1;
                }
            }

            var phx21Status = new Phx21Status
            {
                IsPumpAOn = (status.nStatusFlags & STATUS_PUMP_A_ON) > 0,
                AirPressure = BytesToWord(status.nAirPressure_HPSI1, status.nAirPressure_HPSI0) / 100.0f,
                BatteryVoltage = BytesToWord(status.nBatt_mV1, status.nBatt_mV0) / 1000.0f,
                ChamberOuterTemp =
                    ConvertKelvinToFahrenheit(BytesToWord(status.nChamberOuterTemp_TK1, status.nChamberOuterTemp_TK0) /
                                              10.0f),
                RawPpm = ppm,
                SamplePressure = BytesToWord(status.nSamplePressure_HPSI1, status.nSamplePressure_HPSI0) / 100.0f,
                TankPressure = 10.0f * (BytesToWord(status.nH2Pressure_PSI1, status.nH2Pressure_PSI0) / 10),
                //this is copied... losing a fraction looks intentional
                ThermoCouple =
                    ConvertKelvinToFahrenheit(BytesToWord(status.nThermocouple_TK1, status.nThermocouple_TK0) / 10.0f),
                PicoAmps =
                    (double)
                        BytesToDword(status.nFIDTenthsPicoA_In13, status.nFIDTenthsPicoA_In12,
                            status.nFIDTenthsPicoA_In11, status.nFIDTenthsPicoA_In10) / (double)10.0,
                SystemCurrent = BytesToWord(status.nSystemCurrentMa1, status.nSystemCurrentMa0),
                PumpPower = status.nPumpA_power_pct,
                IsSolenoidAOn = (status.nStatusFlags & STATUS_SOLENOID_A_ON) > 0,
                IsSolenoidBOn = (status.nStatusFlags & STATUS_SOLENOID_B_ON) > 0,
                FIDRange = status.nFIDRange,
                Timestamp = DateTime.Now.ToString()
            };

            //check for ignition
            bool isIgnited = CheckIfIgnited(phx21Status);

            if (isIgnited != prevIgnite)
            {
                ignitedChagedCount++;

                if (ignitedChagedCount >= 3)
                {
                    prevIgnite = isIgnited;
                }
            }
            else
            {
                ignitedChagedCount = 0;
            }

            phx21Status.IsIgnited = prevIgnite;

            if (phx21Status.IsIgnited && phx21Status.PumpPower >= 85.0)
            {
                WriteToPhxLog("Pump power is above 85% (" + phx21Status.PumpPower + "%), shutting off pump!");
                TurnOffPump();
            }

            //Check for junk data
            //Reread if junk data
            if ((phx21Status.BatteryVoltage > 15 || phx21Status.PicoAmps < -10000 || phx21Status.ThermoCouple < -400) && junkDataCount < 10)
            {
                Task.Delay(20).Wait();
                junkDataCount++;
                return ReadDataExtended();
            }

            junkDataCount = 0;

            //This is where the ppm range is switched
            if (phx21Status.FIDRange == RANGE_MODE_0_LO && phx21Status.PicoAmps >= 6500)
            {
                changeCount++;

                if (changeCount >= 1)
                {
                    changeCount = 0;
                    SetSamplingParameters(RANGE_MODE_3_MAX);
                    Task.Delay(250).Wait();
                }

            }
            else if (phx21Status.FIDRange == RANGE_MODE_3_MAX && phx21Status.PicoAmps <= 6000)
            {
                changeCount++;

                if (changeCount >= 1)
                {
                    changeCount = 0;
                    SetSamplingParameters(RANGE_MODE_0_LO);
                    Task.Delay(250).Wait();
                }
            }

            pastPpms.Add(phx21Status.RawPpm);

            if (pastPpms.Count > maxPastPpms)
                pastPpms.Remove(pastPpms[0]);

            //apply averaging to the ppm value
            phx21Status.LongAveragePpm = pastPpms.Skip(Math.Max(pastPpms.Count - LongAverageCount, 0)).Average();

            phx21Status.LongAveragePpm = phx21Status.LongAveragePpm >= 100
                ? Math.Round(phx21Status.LongAveragePpm, 1)
                : Math.Round(phx21Status.LongAveragePpm, 0);

            var shortAveragePpms = pastPpms.Skip(Math.Max(pastPpms.Count - ShortAverageCount, 0)).ToArray();

            phx21Status.ShortAveragePpm = shortAveragePpms.Average();

            phx21Status.ShortAveragePpm = phx21Status.ShortAveragePpm >= 100
                ? Math.Round(phx21Status.ShortAveragePpm, 0)
                : Math.Round(phx21Status.ShortAveragePpm, 1);

            phx21Status.UseAverage = shortAveragePpms
                .All(p => ((p / phx21Status.LongAveragePpm) * 100 >= 100 - UseAvgPerc
                           && (p / phx21Status.LongAveragePpm) * 100 <= 100 + UseAvgPerc));

            if (phx21Status.UseAverage)
            {
                phx21Status.Ppm = phx21Status.FIDRange == RANGE_MODE_3_MAX ? phx21Status.LongAveragePpm : phx21Status.ShortAveragePpm;
            }
            else
            {
                phx21Status.Ppm = phx21Status.RawPpm;
            }

            phx21Status.PpmStr = phx21Status.IsIgnited ? phx21Status.Ppm.ToString() : "N/A";

            if (phx21Status.PicoAmps <= 100 && currentHardwareAvg == 10)
            {
                currentHardwareAvg = 50;
                SetIntegrationControlParams(0, 1, 7, 50000, currentHardwareAvg, 0);

            }
            else if (phx21Status.PicoAmps > 100 && currentHardwareAvg == 50)
            {
                currentHardwareAvg = 10;
                SetIntegrationControlParams(0, 1, 7, 50000, currentHardwareAvg, 0);
            }
            //取消状态日志记录
            //AppendToFile(LogFilePath, GetLineForLog(phx21Status));

            return phx21Status;
        }

        private List<double> pastPpms = new List<double>();
        private int maxPastPpms = 50;

        private float ConvertKelvinToFahrenheit(float kelvin)
        {
            return (float)Math.Round((kelvin - 273.15f) * 1.8f + 32, 1);
        }

        private byte DwordToByte0(uint dword)
        {
            return (byte)(0xFF & (dword));
        }

        private byte DwordToByte1(uint dword)
        {
            return (byte)(0xFF & ((dword) >> 8));
        }

        private byte DwordToByte2(uint dword)
        {
            return (byte)(0xFF & ((dword) >> 16));
        }

        private byte DwordToByte3(uint dword)
        {
            return (byte)(0xFF & ((dword) >> 24));
        }

        private int BytesToWord(byte b1, byte b0)
        {
            return (0xFFFF & ((int)((b1) << 8)) | ((int)(b0)));
        }

        private int BytesToDword(byte b3, byte b2, byte b1, byte b0)
        {
            return (int)(0xFFFFFFFF & (((int)((b3) << 24)) | ((int)((b2) << 16)) | ((int)((b1) << 8)) | (int)(b0)));
        }

        public void Ignite(bool onOff)
        {
            Ignite(onOff, 0);
        }

        /// <summary>
        /// Ignites the phx21
        /// SENDS: CMD_FIDM_AUTO_IGNITION_SEQUENCE with AUTO_IGNITION_SEQUENCE args
        /// RECEIVES: FIDM_STATUS. Response is ignored.
        /// </summary>
        /// <param name="onOff">true to ignite, false to extinguish - extinguish doesn't seem to be used, call IgniteOff() instead</param>
        /// <param name="glowplug">true to use glow plug B, false to use glow plug A</param>
        public void Ignite(bool onOff, byte glowplug)
        {
            byte nCmd = CMD_FIDM_AUTO_IGNITION_SEQUENCE;

            var ignition = BuildAutoIgnitionSequence();
            ignition.use_glow_plug_b = glowplug;
            ignition.start_stop = (byte)(onOff ? 1 : 0);

            var bytes = GetBytes(ignition);
            byte nLength = (byte)bytes.Length;

            lock (obj)
            {
                WriteToPhxLog("Ignite");
                TransmitSerialCmd(nCmd, bytes, nLength, nLength, true);
                ReceiveCmdResponse<FIDM_STATUS>(nCmd);
            }
        }

        public void WriteData(byte[] bytes)
        {
            WriteData(0, bytes);
        }

        public void WriteDataWithLength(int startingAddress, byte[] bytes)
        {
            // hmm, doesn't really seem compressed...
            byte[] compressedBytes;

            compressedBytes = bytes;

            // get file length
            uint len = (uint)compressedBytes.Length;

            if (len == 0)
            {
                return;
            }

            // allocate buffer
            byte[] buffer = new byte[4 + len]; // add 4 bytes for length prefix
            buffer[0] = DwordToByte0(len);
            buffer[1] = DwordToByte1(len);
            buffer[2] = DwordToByte2(len);
            buffer[3] = DwordToByte3(len);

            Array.Copy(compressedBytes, 0, buffer, 4, (int)len);

            WriteData(startingAddress, buffer);
        }

        public void WriteData(int startingAddress, byte[] bytes)
        {
            for (uint i = 0; i < (bytes.Length / 4096) + 1; i++)
            {
                EraseFlash(i);
                Task.Delay(50).Wait();
            }

            WriteFlash(0, (uint)bytes.Length, bytes);

        }

        public void EraseFlash(uint nSector)
        {
            FlashEraseParams pCmd = new FlashEraseParams();

            byte nLength = (byte)Marshal.SizeOf(typeof(FlashEraseParams));
            byte nCmd = CMD_FIDM_FLASH_ERASE;

            pCmd.nSectorNum0 = DwordToByte0(nSector);
            pCmd.nSectorNum1 = DwordToByte1(nSector);
            pCmd.nSectorNum2 = DwordToByte2(nSector);
            pCmd.nSectorNum3 = DwordToByte3(nSector);

            FIDM_STATUS status;

            lock (obj)
            {
                WriteToPhxLog("EraseFlash");
                TransmitSerialCmd(nCmd, GetBytes(pCmd), nLength, nLength, true);
                status = ReceiveCmdResponse<FIDM_STATUS>(nCmd);
            }
        }

        /// <summary>
        /// This is used to build the AUTO_IGNITION_SEQUENCE arguments for igniting a phx21
        /// </summary>
        /// <returns>A fully built AUTO_IGNITION_SEQUENCE</returns>
        private static AUTO_IGNITION_SEQUENCE BuildAutoIgnitionSequence()
        {
            AUTO_IGNITION_SEQUENCE ignition = new AUTO_IGNITION_SEQUENCE();
            ignition.start_stop = 1;
            ignition.target_hPSI = 175;
            ignition.tolerance_hPSI = 5;
            ignition.max_pressure_wait_msec = 10000;
            ignition.min_temperature_rise_tK = 10;
            ignition.max_ignition_wait_msec = 5000;
            ignition.sol_b_delay_msec = 1000;
            ignition.use_glow_plug_b = 0;
            ignition.pre_purge_pump_msec = 5000;
            ignition.pre_purge_sol_A_msec = 5000;

            ignition.param1 = 0;
            ignition.param2 = 0;
            return ignition;
        }

        private byte[] m_nFlashMemContents = new byte[255];

        /// <summary>
        /// Usually call this function to get a command response for the given command byte sent.
        /// Convienience function for ReceiveCmdResponse with more args for timeout and whether it was successful.
        /// </summary>
        /// <typeparam name="T">The type of response you wish to receive</typeparam>
        /// <param name="nCmd">The command byte that was just sent</param>
        /// <returns>A response message of type T</returns>
        private T ReceiveCmdResponse<T>(byte nCmd) where T
            : new()
        {
            bool success;
            return ReceiveCmdResponse<T>(nCmd, out success);
        }

        /// <summary>
        /// A wrapper around GetResponse() that formats the response bytes as a message of type T
        /// </summary>
        /// <typeparam name="T">The type of response you wish to receive</typeparam>
        /// <param name="nCmd">The command byte that was just sent</param>
        /// <param name="success">out param indicating if the response was received in the specified waitTime</param>
        /// <returns>A response message of type T</returns>
        private T ReceiveCmdResponse<T>(byte nCmd, out bool success) where T : new()
        {
            success = true;
            try
            {
                //Get response until bytes[1] > something?  or until bytes[2] == 8 (read)
                var bytes = new byte[3];

                while (bytes[2] != nCmd)
                {
                    bytes = GetResponse(nCmd);
                    if (bytes.Count() < 3)
                    {
                        bytes = new byte[3];
                    }
                }

                try
                {
                    T rsp = FromBytes<T>(bytes);

                    //DefaultResponse
                    if (rsp is DefaultResponse)
                    {
                        DefaultResponse pDefRes = FromBytes<DefaultResponse>(bytes);
                        int nNumReadBytes = pDefRes.nLengthBytes - (Marshal.SizeOf(typeof(DefaultResponse)) + 1);

                        for (int ii = 0; ii < nNumReadBytes; ii++)
                        {
                            m_nFlashMemContents[ii] = bytes[Marshal.SizeOf(typeof(DefaultResponse)) + ii];
                        }
                    }

                    return rsp;
                }
                catch (Exception ex)
                {
                    throw new Exception("Error parsing CmdResponse.", ex);
                }
            }
            catch (Exception ex)
            {
                OnError(new ErrorEventArgs(ex));
            }

            success = false;

            return new T();
        }

        /// <summary>
        /// Workhorse function that reads bytes off the stream and attempts to return them as whole messages.
        /// </summary>
        /// <param name="nCmd">The command byte that we're waiting on a response for</param>
        /// <returns>A byte array containing data received from a phx21</returns>
        private byte[] GetResponse(byte nCmd)
        {
            byte currentState = STATE_WAITING_FOR_SYNC_CODE;
            byte numBytesReceived = 0;
            var responseBytes = new List<byte>();
            bool continueReading = true;

            while (continueReading)
            {
                //eliminating inputStream.flush from the end of this function

                byte readByte = inputStream.ReadByte();

                switch (currentState)
                {
                    case STATE_WAITING_FOR_SYNC_CODE:
                        if (readByte == SYNC_CODE_RES)
                        {
                            numBytesReceived = 0;
                            numBytesReceived++;
                            if (responseBytes.Any()) responseBytes = new List<byte>();
                            responseBytes.Add(readByte);
                            currentState = STATE_WAITING_FOR_LENGTH;
                        }
                        break;

                    case STATE_WAITING_FOR_LENGTH:
                        numBytesReceived++;
                        responseBytes.Add(readByte);
                        currentState = STATE_WAITING_FOR_RESPONSE_ID;

                        if (nCmd == CMD_FIDM_DEBUG)
                        {
                            //if (readByte < (sizeof(default_eng_response) + 1))
                            //currentState = STATE_WAITING_FOR_SYNC_CODE;
                        }
                        else
                        {
                            if (readByte < 3)
                                currentState = STATE_WAITING_FOR_SYNC_CODE;
                        }
                        break;

                    case STATE_WAITING_FOR_RESPONSE_ID:
                        numBytesReceived++;
                        responseBytes.Add(readByte);
                        currentState = STATE_WAITING_FOR_RESPONSE_DATA;
                        break;

                    case STATE_WAITING_FOR_RESPONSE_DATA:
                        numBytesReceived++;
                        responseBytes.Add(readByte);

                        if (numBytesReceived >= responseBytes[FIELD_LENGTH_BYTES])
                        // Receive all the command data as indicated by the count field.
                        {
                            //m_nFlags |= FLAG_RES_COMPLETE;
                            currentState = STATE_WAITING_FOR_SYNC_CODE; // Go back to waiting for the sync code.
                            continueReading = false;
                        }
                        break;

                    default:
                        currentState = STATE_WAITING_FOR_SYNC_CODE;
                        break;
                }
            }

            //inputStream.Flush();

            return responseBytes.ToArray();

        }

        /// <summary>
        /// This is the workhorse function used to send serial commands.
        /// Sets the first 3 bytes to SYNC_CODE_CMD, message length, and cmd id
        /// and optionally adds a crc to the end
        /// </summary>
        /// <param name="nCmd">The command byte to be send, should be one of CMD_FIDM_*</param>
        /// <param name="pStream">The struct defining the data to send</param>
        /// <param name="nTotalCmdLength"></param>
        /// <param name="nHeaderLength">usually the same as nTotalCmdLength</param>
        /// <param name="bSendCrc">true to send the crc at the end of the message</param>
        /// <returns>The crc of the message sent</returns>
        private byte TransmitSerialCmd(byte nCmd, byte[] pStream, byte nTotalCmdLength,
            byte nHeaderLength, bool bSendCrc)
        {
            try
            {
                byte nCRC = 0;
                byte[] pData = new byte[nHeaderLength + 1];

                pStream[FIELD_SYNC_CODE] = SYNC_CODE_CMD;
                pStream[FIELD_LENGTH_BYTES] = (byte)(nTotalCmdLength + 1);
                pStream[FIELD_CMD_ID] = nCmd;

                nCRC = ComputeCRC(pStream, nHeaderLength);

                Array.Copy(pStream, pData, nHeaderLength);

                if (bSendCrc)
                {
                    pData[nHeaderLength] = nCRC;

                    outputStream.Write(pData, 0, nHeaderLength + 1);
                }
                else
                {
                    outputStream.Write(pData, 0, nHeaderLength);
                }

                return nCRC;
            }
            catch (Exception ex)
            {
                OnError(new ErrorEventArgs(ex));
            }

            return 0;
        }

        private byte[] GetBytes<T>(T str)
        {
            int size = Marshal.SizeOf(str);
            byte[] arr = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.StructureToPtr(str, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);

            return arr;
        }

#if COMPACT_FRAMEWORK

        private T FromBytes<T>(byte[] bytes)
        {
            bytesIndex = 0;
            Stopwatch watch = new Stopwatch();
            watch.Start();
            T retVal = (T)FromBytes(typeof(T), bytes);
            watch.Stop();
            return retVal;
        }

        private int bytesIndex = 0;


        private object FromBytes(Type type, byte[] bytes)
        {
            object reconstruct = Activator.CreateInstance(type);

            foreach (FieldInfo field in type.GetFields())
            {
                object obj;

                if (field.FieldType.IsValueType && !field.FieldType.IsPrimitive)
                {
                    if (bytesIndex % 4 != 0)
                        bytesIndex += 4 - (bytesIndex % 4);

                    obj = FromBytes(field.FieldType, bytes);
                }
                else if (field.FieldType.IsArray)
                {
                    break;
                }
                else
                {
                    int size = Marshal.SizeOf(field.FieldType);
                    obj = Convert.ChangeType(GetVal(bytes, bytesIndex, size), field.FieldType, null);
                    bytesIndex += size;
                }

                field.SetValue(reconstruct, obj);


            }

            return reconstruct;
        }

        private static object GetVal(byte[] bytes, int bytesIndex, int size)
        {
            int mult = (size - 1) * 8;

            long l2 = 0;

            if (bytesIndex + size >= bytes.Length)
                return 0;

            try
            {
                for (int i = bytesIndex + size - 1; i >= bytesIndex; i--)
                {
                    l2 = l2 | bytes[i] << mult;
                    mult -= 8;
                }
            }
            catch (Exception ex)
            {
                throw;
            }



            return 0xFFFFFFFF & l2;
        }
#else
        private T FromBytes<T>(byte[] arr) where T : new()
        {
            try
            {
                GCHandle pinnedPacket = GCHandle.Alloc(arr, GCHandleType.Pinned);

                T obj = (T)Marshal.PtrToStructure(pinnedPacket.AddrOfPinnedObject(), typeof(T));
                pinnedPacket.Free();

                return obj;
            }
            catch (Exception ex)
            {

            }
            finally
            {

            }

            throw new Exception("BAD");
        }
#endif


        private byte ComputeCRC(byte[] pStream, byte nLengthBytes)
        {
            byte chksum;
            byte one = 1;
            byte seven = 7;
            chksum = 0xD5;

            for (int i = 0; i < nLengthBytes; i++)
            {

                chksum = (byte)((chksum << one) | (chksum >> seven));
                chksum += pStream[i];
            }

            return chksum;
        }


    }

#if COMPACT_FRAMEWORK
    [StructLayout(LayoutKind.Sequential)]
#else
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
#endif
    public struct DEFAULT_RESPONSE_EXTENDED
    {
        public byte nSyncCode;
        public byte nLengthBytes;
        public byte nCmdID;

        [MarshalAs(UnmanagedType.Struct)]
        public FIDM_STATUS_EXTENDED status;
    }

#if COMPACT_FRAMEWORK
    [StructLayout(LayoutKind.Sequential)]
#else
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
#endif
    public struct READ_DATA_PARAMS
    {
        public byte nSyncCode;
        public byte nLengthBytes;
        public byte nCmdID;
    }

#if COMPACT_FRAMEWORK
    [StructLayout(LayoutKind.Sequential)]
#else
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
#endif
    public struct FIDM_STATUS_EXTENDED
    {
        public byte nStatusFlags;
        public byte nReadingNumber;
        public byte nError;
        public byte nThermocouple_TK0; // Tenths of Degrees Kelvin, unsigned.
        public byte nThermocouple_TK1;
        public byte nBatt_mV0; // Millivolts, unsigned.
        public byte nBatt_mV1;
        public byte nChamberOuterTemp_TK0; // Tenths of Degrees Kelvin, unsigned.
        public byte nChamberOuterTemp_TK1;
        public byte nSamplePressure_HPSI0; // Hundredths of PSI, unsigned.
        public byte nSamplePressure_HPSI1;
        public byte nAirPressure_HPSI0; // Hundredths of PSI, unsigned.
        public byte nAirPressure_HPSI1;
        public byte nH2Pressure_PSI0; // PSI, unsigned.
        public byte nH2Pressure_PSI1;
        public byte nFIDRange;
        public byte nFIDTenthsPicoA_Sat0; // Saturation, Tenths of Pico Amps, signed.
        public byte nFIDTenthsPicoA_Sat1;
        public byte nFIDTenthsPicoA_Sat2;
        public byte nFIDTenthsPicoA_Sat3;
        public byte nFIDTenthsPicoA_In10; // Input1, Tenths of Pico Amps, signed.
        public byte nFIDTenthsPicoA_In11;
        public byte nFIDTenthsPicoA_In12;
        public byte nFIDTenthsPicoA_In13;
        public byte nFIDTenthsPicoA_In20; // Input2, Tenths of Pico Amps, signed.
        public byte nFIDTenthsPicoA_In21;
        public byte nFIDTenthsPicoA_In22;
        public byte nFIDTenthsPicoA_In23;
        // ---- new additions below this mark ----
        public byte nFIDTenthsPPM0;
        public byte nFIDTenthsPPM1;
        public byte nFIDTenthsPPM2;
        public byte nFIDTenthsPPM3;
        public byte nSystemCurrentMa0;
        public byte nSystemCurrentMa1;
        public byte nPumpA_power_pct;
        public byte spare;

        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct, SizeConst = Phx21.PID_LOG_SIZE)]
        public PID_LOG_ENTRY[] pid_log;
    }

#if COMPACT_FRAMEWORK
    [StructLayout(LayoutKind.Sequential)]
#else
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
#endif
    public struct PID_LOG_ENTRY
    {
        public ushort millisecond;
        public short derivative;
        public short p_error;
        public short err_acc;
        public short pump_pwr;
    }

#if COMPACT_FRAMEWORK
    [StructLayout(LayoutKind.Sequential)]
#else
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
#endif
    public struct AUTO_IGNITION_SEQUENCE
    {

        public byte nSyncCode;
        public byte nLengthBytes;
        public byte nCmdID;
        public byte start_stop; // 0 = abort if running, 1 = start
        public short target_hPSI;
        public short tolerance_hPSI;
        public ushort min_temperature_rise_tK;
        public ushort max_pressure_wait_msec;
        public ushort max_ignition_wait_msec;
        public ushort sol_b_delay_msec;
        public ushort pre_purge_pump_msec;
        public ushort pre_purge_sol_A_msec;
        public ushort param1;
        public ushort param2;
        public byte use_glow_plug_b; // 0 or 1

    }

#if COMPACT_FRAMEWORK
    [StructLayout(LayoutKind.Sequential)]
#else
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
#endif
    public struct FIDM_STATUS // Status
    {
        public byte nStatusFlags;
        public byte nReadingNumber;
        public byte nError;
        public byte nThermocouple_TK0; // Tenths of Degrees Kelvin, unsigned.
        public byte nThermocouple_TK1;
        public byte nBatt_mV0; // Millivolts, unsigned.
        public byte nBatt_mV1;
        public byte nChamberOuterTemp_TK0; // Tenths of Degrees Kelvin, unsigned.
        public byte nChamberOuterTemp_TK1;
        public byte nSamplePressure_HPSI0; // Hundredths of PSI, unsigned.
        public byte nSamplePressure_HPSI1;
        public byte nAirPressure_HPSI0; // Hundredths of PSI, unsigned.
        public byte nAirPressure_HPSI1;
        public byte nH2Pressure_PSI0; // PSI, unsigned.
        public byte nH2Pressure_PSI1;
        public byte nFIDRange;
        public byte nFIDTenthsPicoA_Sat0; // Saturation, Tenths of Pico Amps, signed.
        public byte nFIDTenthsPicoA_Sat1;
        public byte nFIDTenthsPicoA_Sat2;
        public byte nFIDTenthsPicoA_Sat3;
        public byte nFIDTenthsPicoA_In10; // Input1, Tenths of Pico Amps, signed.
        public byte nFIDTenthsPicoA_In11;
        public byte nFIDTenthsPicoA_In12;
        public byte nFIDTenthsPicoA_In13;
        public byte nFIDTenthsPicoA_In20; // Input2, Tenths of Pico Amps, signed.
        public byte nFIDTenthsPicoA_In21;
        public byte nFIDTenthsPicoA_In22;
        public byte nFIDTenthsPicoA_In23;
    } // 28 bytes

    /*  
#if COMPACT_FRAMEWORK
    [StructLayout(LayoutKind.Sequential)]
#else
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
#endif
    public struct ConfigurationResponse
{
    public byte nSyncCode;
    public byte nLengthBytes;
    public byte nCmdID;

    [MarshalAs(UnmanagedType.Struct, SizeConst = 28)]
    FIDM_STATUS status;         // 28 bytes

    public byte nVersion;
    public byte nSectorSizeBytes0;
    public byte nSectorSizeBytes1;
    public byte nSectorSizeBytes2;
    public byte nSectorSizeBytes3;
    public byte nNumberSectors0;
    public byte nNumberSectors1;
    public byte nNumberSectors2;
    public byte nNumberSectors3;
}   // 40 bytes
*/
#if COMPACT_FRAMEWORK
    [StructLayout(LayoutKind.Sequential)]
#else
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
#endif
    public struct SetCalibration
    {
        public byte nSyncCode;
        public byte nLengthBytes;
        public byte nCmdID;
        public byte index_number;
        public int ppm_tenths;
        public int fid_current_tPa;
        public ushort H2_pressure_hPSI;
        public byte overwrite; // 1 = overwrite; 0 = erase
    }

#if COMPACT_FRAMEWORK
    [StructLayout(LayoutKind.Sequential)]
#else
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
#endif
    public struct GenerateCalibration
    {
        public byte nSyncCode;
        public byte nLengthBytes;
        public byte nCmdID;
        public byte spare_for_alignment;
        public int ppm_tenths;
    }

#if COMPACT_FRAMEWORK
    [StructLayout(LayoutKind.Sequential)]
#else
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
#endif
    public struct GetCalibration
    {
        public byte nSyncCode;
        public byte nLengthBytes;
        public byte nCmdID;
        public byte index_number;
    }

#if COMPACT_FRAMEWORK
    [StructLayout(LayoutKind.Sequential)]
#else
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
#endif
    public struct GetCalibrationResponse
    {
        public byte nSyncCode;
        public byte nLengthBytes;
        public byte nCmdID;

        [MarshalAs(UnmanagedType.Struct, SizeConst = 28)]
        public FIDM_STATUS status;

        public byte index_number;
        public int ppm_tenths;
        public int fid_current_tPa;
        public ushort H2_pressure_hPSI;
        public byte valid; // 1 = data valid; 0 = empty slot
    }

#if COMPACT_FRAMEWORK
    [StructLayout(LayoutKind.Sequential)]
#else
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
#endif
    public struct PumpAux1ControlParams
    {
        public byte nSyncCode;
        public byte nLengthBytes;
        public byte nCmdID;
        public byte nID;
        public byte nPowerTenthsPercent0;
        public byte nPowerTenthsPercent1;
        public byte nKickStartDurationSec;
    }

#if COMPACT_FRAMEWORK
    [StructLayout(LayoutKind.Sequential)]
#else
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
#endif
    public struct DefaultResponse                                       // Responses
    {
        public byte nSyncCode;
        public byte nLengthBytes;
        public byte nCmdID;
        FIDM_STATUS status;         // 28 bytes
    }            // 31 bytes

#if COMPACT_FRAMEWORK
    [StructLayout(LayoutKind.Sequential)]
#else
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
#endif
    public struct SolenoidControlParams
    {
        public byte nSyncCode;
        public byte nLengthBytes;
        public byte nCmdID;
        public byte nID;
        public byte nPower;
    }

#if COMPACT_FRAMEWORK
    [StructLayout(LayoutKind.Sequential)]
#else
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
#endif
    public struct PumpClosedLoop
    {
        public byte nSyncCode;
        public byte nLengthBytes;
        public byte nCmdID;
        public byte enable; // 0 or 1
        public short target_hPSI;
    }

#if COMPACT_FRAMEWORK
    [StructLayout(LayoutKind.Sequential)]
#else
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
#endif
    public struct IgniteParams
    {
        public byte nSyncCode;
        public byte nLengthBytes;
        public byte nCmdID;
        public byte nID;
        public byte nDurationSec;
    }

#if COMPACT_FRAMEWORK
    [StructLayout(LayoutKind.Sequential)]
#else
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
#endif
    public struct SetSamplingParams
    {
        public byte nSyncCode;
        public byte nLengthBytes;
        public byte nCmdID;
        public byte nRange; // 0, 1, 2 or 3
    }

#if COMPACT_FRAMEWORK
    [StructLayout(LayoutKind.Sequential)]
#else
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
#endif
    public struct DeadheadParams
    {
        public byte nSyncCode;
        public byte nLengthBytes;
        public byte nCmdID;
        public byte enable; // 0 or 1
        public ushort pressure_low_limit_hPSI;
        public ushort max_duration_msec;
    }

#if COMPACT_FRAMEWORK
    [StructLayout(LayoutKind.Sequential)]
#else
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
#endif
    public struct IntegrationControlParams
    {
        public byte nSyncCode;
        public byte nLengthBytes;
        public byte nCmdID;
        public byte nMode; // 0 or 1
        public byte nChargeMultiplier;
        public byte nRange;
        public byte nIntegrationTimeUs0;
        public byte nIntegrationTimeUs1;
        public byte nSamplesToAvg;
        public byte nReportMode;
    }

#if COMPACT_FRAMEWORK
    [StructLayout(LayoutKind.Sequential)]
#else
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
#endif
    public struct FlashWriteParams
    {
        public byte nSyncCode;
        public byte nLengthBytes;
        public byte nCmdID;
        public byte nStartingAddress0;
        public byte nStartingAddress1;
        public byte nStartingAddress2;
        public byte nStartingAddress3;
        public byte nCount;
    }

#if COMPACT_FRAMEWORK
    [StructLayout(LayoutKind.Sequential)]
#else
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
#endif
    public struct FlashReadParams
    {
        public byte nSyncCode;
        public byte nLengthBytes;
        public byte nCmdID;
        public byte nStartingAddress0;
        public byte nStartingAddress1;
        public byte nStartingAddress2;
        public byte nStartingAddress3;
        public byte nCount;
    }

#if COMPACT_FRAMEWORK
    [StructLayout(LayoutKind.Sequential)]
#else
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
#endif
    public struct FlashEraseParams
    {
        public byte nSyncCode;
        public byte nLengthBytes;
        public byte nCmdID;
        public byte nSectorNum0;
        public byte nSectorNum1;
        public byte nSectorNum2;
        public byte nSectorNum3;
    }

#if COMPACT_FRAMEWORK
    [StructLayout(LayoutKind.Sequential)]
#else
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
#endif
    public struct CalH2PressureCompensation
    {
        public byte nSyncCode;
        public byte nLengthBytes;
        public byte nCmdID;
        public byte spare_for_alignment;
        public long H2_compensation_pos; // (fraction * 10^6) per LPH2 hPSI that PPM will be adjusted
        public long H2_compensation_neg; // (fraction * 10^6) per LPH2 hPSI that PPM will be adjusted
    };

#if COMPACT_FRAMEWORK
    [StructLayout(LayoutKind.Sequential)]
#else
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
#endif
    public struct ConfigurationReadParams
    {
        public byte nSyncCode;
        public byte nLengthBytes;
        public byte nCmdID;
    };

#if COMPACT_FRAMEWORK
    [StructLayout(LayoutKind.Sequential)]
#else
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
#endif
    public struct ConfigurationResponse
    {
        public byte nSyncCode;
        public byte nLengthBytes;
        public byte nCmdID;
        // start fidm status
        // done this way because a struct within a struct doesn't work...
        public byte nStatusFlags;
        public byte nReadingNumber;
        public byte nError;
        public byte nThermocouple_TK0; // Tenths of Degrees Kelvin, unsigned.
        public byte nThermocouple_TK1;
        public byte nBatt_mV0; // Millivolts, unsigned.
        public byte nBatt_mV1;
        public byte nChamberOuterTemp_TK0; // Tenths of Degrees Kelvin, unsigned.
        public byte nChamberOuterTemp_TK1;
        public byte nSamplePressure_HPSI0; // Hundredths of PSI, unsigned.
        public byte nSamplePressure_HPSI1;
        public byte nAirPressure_HPSI0; // Hundredths of PSI, unsigned.
        public byte nAirPressure_HPSI1;
        public byte nH2Pressure_PSI0; // PSI, unsigned.
        public byte nH2Pressure_PSI1;
        public byte nFIDRange;
        public byte nFIDTenthsPicoA_Sat0; // Saturation, Tenths of Pico Amps, signed.
        public byte nFIDTenthsPicoA_Sat1;
        public byte nFIDTenthsPicoA_Sat2;
        public byte nFIDTenthsPicoA_Sat3;
        public byte nFIDTenthsPicoA_In10; // Input1, Tenths of Pico Amps, signed.
        public byte nFIDTenthsPicoA_In11;
        public byte nFIDTenthsPicoA_In12;
        public byte nFIDTenthsPicoA_In13;
        public byte nFIDTenthsPicoA_In20; // Input2, Tenths of Pico Amps, signed.
        public byte nFIDTenthsPicoA_In21;
        public byte nFIDTenthsPicoA_In22;
        public byte nFIDTenthsPicoA_In23;
        // end FIDM status
        public byte nVersion;
        public byte nSectorSizeBytes0;
        public byte nSectorSizeBytes1;
        public byte nSectorSizeBytes2;
        public byte nSectorSizeBytes3;
        public byte nNumberSectors0;
        public byte nNumberSectors1;
        public byte nNumberSectors2;
        public byte nNumberSectors3;
    };       // 40 bytes
}
