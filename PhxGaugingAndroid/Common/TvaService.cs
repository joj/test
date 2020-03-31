using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Util;
using PhxGauging.Common.Android.Services;
using PhxGauging.Common.Services;

namespace PhxGaugingAndroid.Common
{
    public class TvaService
    {
        private IBluetoothService bluetoothService;
        private IDevice device;
        public string Name { get; private set; }

        public bool IsConnected = false;

        //数据采集返回
        public delegate void DataPollEventHandler(float FID, string FIDStatus, float PID, string PIDStatus);
        public event DataPollEventHandler DataPoll;

        //点火返回
        public delegate void FireResultEventHandler(bool isSuccess);
        public event FireResultEventHandler FireResult;

        //连接状态
        public delegate void ConnectStatusEventHandler(bool isConnect);
        public event ConnectStatusEventHandler ConnectStatus;

        //提示消息
        public delegate void MessagerEventHandler(string msg);
        public event MessagerEventHandler Messager;

        public void Connect(IBluetoothService bluetoothService, IDevice device)
        {
            try
            {
                if (IsConnected)
                    return;
                this.bluetoothService = bluetoothService;
                this.device = device;
                Name = device.Name;
                bluetoothService.Connect(device);
                IsConnected = true;
                if (Messager != null)
                {
                    Messager("蓝牙连接成功");
                }
                if (ConnectStatus != null)
                {
                    ConnectStatus(IsConnected);
                }
                IsAccept = true;
                Thread thread = new Thread(new ThreadStart(accpClient));
                thread.IsBackground = true;
                thread.Start();
            }
            catch (Exception ex)
            {
                if (Messager != null)
                {
                    Messager("蓝牙连接失败," + ex.Message);
                }
                IsConnected = false;
                if (ConnectStatus != null)
                {
                    ConnectStatus(IsConnected);
                }
            }
        }
        public bool IsAccept = true;
        private void accpClient()
        {
            try
            {
                System.Threading.Thread.Sleep(2000);
                StringBuilder result = new StringBuilder();
                Stream netStream = bluetoothService.InputStream;
                while (IsAccept)
                {
                    byte[] Rec = new byte[1];
                    netStream.Read(Rec, 0, Rec.Length);
                    result.Append(Encoding.Default.GetString(Rec));
                    if (result.ToString().Contains("\r\n"))
                    {
                        if (result.Length == 44)
                        {
                            float pid = 0;
                            float.TryParse(result.ToString().Substring(0, 10), out pid);
                            string pidStatus = result.ToString().Substring(11, 9).Trim();
                            float fid = 0;
                            float.TryParse(result.ToString().Substring(21, 10), out fid);
                            string fidStatus = result.ToString().Substring(32, 9).Trim();
                            if (DataPoll != null)
                            {
                                DataPoll(fid, fidStatus, pid, pidStatus);
                            }
                        }
                        else
                        {

                        }
                        result.Clear();
                        System.Threading.Thread.Sleep(50);
                    }
                }
            }
            catch (Exception ex)
            {
                if (IsAccept == true)
                {
                    if (Messager != null)
                    {
                        Messager("接收数据发生错误" + ex.Message);
                    }
                }
                IsConnected = false;
                if (ConnectStatus != null)
                {
                    ConnectStatus(IsConnected);
                }
            }
        }

        #region 转换
        /// <summary> Convert a string of hex digits (ex: E4 CA B2) to a byte array. </summary>
        /// <param name="s"> The string containing the hex digits (with or without spaces). </param>
        /// <returns> Returns an array of bytes. </returns>
        public byte[] HexStringToByteArray(string s)
        {
            s = s.Replace(" ", "");
            byte[] buffer = new byte[s.Length / 2];
            for (int i = 0; i < s.Length; i += 2)
            {
                buffer[i / 2] = (byte)Convert.ToByte(s.Substring(i, 2), 16);
            }

            return buffer;
        }

        /// <summary> Converts an array of bytes into a formatted string of hex digits (ex: E4 CA B2)</summary>
        /// <param name="data"> The array of bytes to be translated into a string of hex digits. </param>
        /// <returns> Returns a well formatted string of hex digits with spacing. </returns>
        public string ByteArrayToHexString(byte[] data)
        {
            StringBuilder sb = new StringBuilder(data.Length * 3);
            foreach (byte b in data)
            {
                sb.Append(Convert.ToString(b, 16).PadLeft(2, '0').PadRight(3, ' '));
            }

            return sb.ToString().ToUpper();
        }

        /// <summary>
        /// 将一条十六进制字符串转换为ASCII
        /// </summary>
        /// <param name="hexstring">一条十六进制字符串</param>
        /// <returns>返回一条ASCII码</returns>
        public static string HexStringToASCII(string hexstring)
        {
            byte[] bt = HexStringToBinary(hexstring);
            string lin = "";
            for (int i = 0; i < bt.Length; i++)
            {
                lin = lin + bt[i] + " ";
            }


            string[] ss = lin.Trim().Split(new char[] { ' ' });
            char[] c = new char[ss.Length];
            int a;
            for (int i = 0; i < c.Length; i++)
            {
                a = Convert.ToInt32(ss[i]);
                c[i] = Convert.ToChar(a);
            }

            string b = new string(c);
            return b;
        }


        /**/
        /// <summary>
        /// 16进制字符串转换为二进制数组
        /// </summary>
        /// <param name="hexstring">用空格切割字符串</param>
        /// <returns>返回一个二进制字符串</returns>
        public static byte[] HexStringToBinary(string hexstring)
        {

            string[] tmpary = hexstring.Trim().Split(' ');
            byte[] buff = new byte[tmpary.Length];
            for (int i = 0; i < buff.Length; i++)
            {
                buff[i] = Convert.ToByte(tmpary[i], 16);
            }
            return buff;
        }


        /// <summary>
        /// 将byte型转换为字符串
        /// </summary>
        /// <param name="arrInput">byte型数组</param>
        /// <returns>目标字符串</returns>
        private string ByteArrayToString(byte[] arrInput)
        {
            int i;
            StringBuilder sOutput = new StringBuilder(arrInput.Length);
            for (i = 0; i < arrInput.Length; i++)
            {
                sOutput.Append(arrInput[i].ToString("X2"));
            }
            //将此实例的值转换为System.String
            return sOutput.ToString();
        }



        /// <summary>
        /// 对接收到的数据进行解包（将接收到的byte型数组解包为Unicode字符串）
        /// </summary>
        /// <param name="recbytes">byte型数组</param>
        /// <returns>Unicode编码的字符串</returns>
        public string disPackage(byte[] recbytes)
        {
            string temp = "";
            foreach (byte b in recbytes)
                temp += b.ToString("X2") + " ";//ToString("X2") 为C#中的字符串格式控制符
            return temp;
        }

        /**
    * int转byte[]
    * 该方法将一个int类型的数据转换为byte[]形式，因为int为32bit，而byte为8bit所以在进行类型转换时，知会获取低8位，
    * 丢弃高24位。通过位移的方式，将32bit的数据转换成4个8bit的数据。注意 &0xff，在这当中，&0xff简单理解为一把剪刀，
    * 将想要获取的8位数据截取出来。
    * @param i 一个int数字
    * @return byte[]
    */
        public static byte[] int2ByteArray(int i)
        {
            byte[] result = new byte[4];
            result[0] = (byte)((i >> 24) & 0xFF);
            result[1] = (byte)((i >> 16) & 0xFF);
            result[2] = (byte)((i >> 8) & 0xFF);
            result[3] = (byte)(i & 0xFF);
            return result;
        }
        /**
         * byte[]转int
         * 利用int2ByteArray方法，将一个int转为byte[]，但在解析时，需要将数据还原。同样使用移位的方式，将适当的位数进行还原，
         * 0xFF为16进制的数据，所以在其后每加上一位，就相当于二进制加上4位。同时，使用|=号拼接数据，将其还原成最终的int数据
         * @param bytes byte类型数组
         * @return int数字
         */
        public static int bytes2Int(byte[] bytes)
        {
            int num = bytes[3] & 0xFF;
            num |= ((bytes[2] << 8) & 0xFF00);
            num |= ((bytes[1] << 16) & 0xFF0000);
            num |= ((bytes[0] << 24) & 0xFF0000);
            return num;
        }

        public static string Int2String(int str)
        {
            string S = Convert.ToString(str);
            return S;
        }

        public static int String2Int(string str)
        {
            int a;
            int.TryParse(str, out a);
            int a1 = Convert.ToInt32(str);
            return a1;
        }


        /*将int转为低字节在后，高字节在前的byte数组
b[0] = 11111111(0xff) & 01100001
b[1] = 11111111(0xff) & 00000000
b[2] = 11111111(0xff) & 00000000
b[3] = 11111111(0xff) & 00000000
*/
        public byte[] IntToByteArray2(int value)
        {
            byte[] src = new byte[4];
            src[0] = (byte)((value >> 24) & 0xFF);
            src[1] = (byte)((value >> 16) & 0xFF);
            src[2] = (byte)((value >> 8) & 0xFF);
            src[3] = (byte)(value & 0xFF);
            return src;
        }
        //将高字节在前转为int，低字节在后的byte数组(与IntToByteArray2想对应)
        public int ByteArrayToInt2(byte[] bArr)
        {
            if (bArr.Length != 4)
            {
                return -1;
            }
            return (int)((((bArr[0] & 0xff) << 24)
                       | ((bArr[1] & 0xff) << 16)
                       | ((bArr[2] & 0xff) << 8)
   | ((bArr[3] & 0xff) << 0)));
        }

        public static string StringToHexArray(string input)
        {
            char[] values = input.ToCharArray();
            StringBuilder sb = new StringBuilder(input.Length * 3);
            foreach (char letter in values)
            {
                // Get the integral value of the character.
                int value = Convert.ToInt32(letter);
                // Convert the decimal value to a hexadecimal value in string form.
                string hexOutput = String.Format("{0:X}", value);
                sb.Append(Convert.ToString(value, 16).PadLeft(2, '0').PadRight(3, ' '));
            }

            return sb.ToString().ToUpper();

        }
        #endregion

        /// <summary>
        /// 点火
        /// </summary>
        public void FireAction()
        {
            try
            {
                if (bluetoothService != null)
                {
                    bluetoothService.OutputStream.Write(Encoding.Default.GetBytes("screen\r\n"));
                    System.Threading.Thread.Sleep(1000);
                    bluetoothService.OutputStream.Write(Encoding.Default.GetBytes("push 1\r\n"));
                    System.Threading.Thread.Sleep(6000);
                    bluetoothService.OutputStream.Write(Encoding.Default.GetBytes("quit\r\n"));
                    System.Threading.Thread.Sleep(7000);
                    bluetoothService.OutputStream.Write(Encoding.Default.GetBytes("log start\r\n"));
                    if (FireResult != null)
                    {
                        FireResult(true);
                    }
                }
            }
            catch (Exception ex)
            {
                if (Messager != null)
                {
                    Messager("点火失败：" + ex.Message);
                }
            }
        }

        public void PumpOnAction()
        {
            if (bluetoothService != null)
            {
                bluetoothService.OutputStream.Write(Encoding.Default.GetBytes("pump on\r\n"));
                System.Threading.Thread.Sleep(1000);
            }
        }

        public void PumpOffAction()
        {
            if (bluetoothService != null)
            {
                bluetoothService.OutputStream.Write(Encoding.Default.GetBytes("pump off\r\n"));
                System.Threading.Thread.Sleep(1000);
            }
        }
        public void Disconnect()
        {
            if (!IsConnected)
                return;
            try
            {
                IsAccept = false;
                bluetoothService.Disconnect(device);
            }
            catch (System.Exception ex)
            {
            }
            IsConnected = false;
        }
    }

    public class TvaAndroidBluetoothService : IBluetoothService
    {
        private readonly Context context;
        private static UUID SERIAL_UUID = UUID.FromString("00001101-0000-1000-8000-00805F9B34FB");

        private BluetoothAdapter bluetoothAdapter;
        public TvaReceiver receiver;
        private BluetoothSocket bluetoothSocket;
        public BluetoothDevice bluetoothDevice;

        public event DeviceDiscoveredHandler DeviceDiscovered;
        public event EventHandler DeviceDiscoveryComplete;

        public Stream InputStream { get { return bluetoothSocket == null ? null : bluetoothSocket.InputStream; } }
        public Stream OutputStream { get { return bluetoothSocket == null ? null : bluetoothSocket.OutputStream; } }

        public TvaAndroidBluetoothService(Context context)
        {
            this.context = context;
            bluetoothAdapter = BluetoothAdapter.DefaultAdapter;
            if (bluetoothAdapter.IsEnabled == false)
            {
                bluetoothAdapter.Enable();
            }
            // Register for broadcasts when a device is discovered
            receiver = new TvaReceiver();
            receiver.OnDeviceDiscoveredFunc = OnDeviceDiscovered;
            receiver.OnDiscoveryCompleteFunc = OnDeviceDiscoveryComplete;

            var filter = new IntentFilter();
            filter.AddAction(BluetoothDevice.ActionFound);
            filter.AddAction(BluetoothAdapter.ActionDiscoveryFinished);
            context.RegisterReceiver(receiver, filter);
        }

        public void StartDiscovery()
        {
            if (bluetoothAdapter.IsDiscovering == false)
            {
                bluetoothAdapter.StartDiscovery();
            }
        }

        protected virtual void OnDeviceDiscovered(IDevice device)
        {
            var handler = DeviceDiscovered;
            if (handler != null) handler(new IDevice[] { device });
        }

        private bool ignite = false;

        public void Connect(IDevice device)
        {
            this.bluetoothSocket = this.bluetoothDevice.CreateRfcommSocketToServiceRecord(SERIAL_UUID);
            this.bluetoothSocket.Connect();
        }
        public void StopDiscovery()
        {
            bluetoothAdapter.CancelDiscovery();
        }

        public void Disconnect(IDevice device)
        {
            bluetoothSocket.Close();
        }

        protected virtual void OnDeviceDiscoveryComplete()
        {
            var handler = DeviceDiscoveryComplete;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public void GetSignalStrength(string name, Action<int> signalStrengthCallback)
        {
            /* bluetoothAdapter = BluetoothAdapter.DefaultAdapter;

             bluetoothSocket.Close();
             // Register for broadcasts when a device is discovered
             receiver = new Receiver();
             receiver.OnDeviceDiscoveredFunc = d =>
             {
                 if (d.Name == name)
                 {
                     signalStrengthCallback(d.SignalStrength);
                     Connect(d);
                 }
             };

             var filter = new IntentFilter(BluetoothDevice.ActionFound);
             context.RegisterReceiver(receiver, filter);

             // Register for broadcasts when discovery has finished
             filter = new IntentFilter(BluetoothAdapter.ActionDiscoveryFinished);
             context.RegisterReceiver(receiver, filter);

             StartDiscovery();*/
        }
    }

    public class TvaReceiver : BroadcastReceiver
    {
        public Action<IDevice> OnDeviceDiscoveredFunc;
        public Action OnDiscoveryCompleteFunc;

        public BluetoothDevice tvaDevice;

        public List<BluetoothDevice> tvaDevices = new List<BluetoothDevice>();

        public BluetoothDevice getBluetooth()
        {
            return this.tvaDevice;
        }

        public List<BluetoothDevice> getBluetooths()
        {
            return this.tvaDevices;
        }

        public override void OnReceive(Context context, Intent intent)
        {

            string action = intent.Action;

            // When discovery finds a device
            if (action == BluetoothDevice.ActionFound)
            {
                // Get the BluetoothDevice object from the Intent
                BluetoothDevice device = (BluetoothDevice)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);

                if (string.IsNullOrEmpty(device.Name))
                    return;

                int rssi = intent.GetShortExtra(BluetoothDevice.ExtraRssi, Java.Lang.Short.MinValue);
                //String name = intent.GetStringExtra(BluetoothDevice.ExtraName);

                // If it's already paired, skip it, because it's been listed already
                //if (device.BondState != Bond.Bonded)
                //{
                OnDeviceDiscoveredFunc(new Device(device, rssi));
                //}
                // When discovery is finished, change the Activity title

                tvaDevice = (BluetoothDevice)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);
                int num = 0;
                if (this.tvaDevices == null)
                {
                    if (this.tvaDevice.Name.ToLower().Contains("tva"))
                    {
                        this.tvaDevices.Add(this.tvaDevice);
                    }
                }
                else
                {
                    foreach (BluetoothDevice current in this.tvaDevices)
                    {
                        if (this.tvaDevice.Name == current.Name)
                        {
                            num = 1;
                            break;
                        }
                    }
                    if (num == 0 && this.tvaDevice.Name.ToLower().Contains("tva"))
                    {
                        this.tvaDevices.Add(this.tvaDevice);
                    }
                }
            }
            else if (action == BluetoothAdapter.ActionDiscoveryFinished)
            {
                if (OnDiscoveryCompleteFunc != null)
                    OnDiscoveryCompleteFunc();
            }
        }
    }
}