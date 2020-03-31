using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace PhxGaugingAndroid.Common
{
    public class ExepcService
    {
        public System.Net.Sockets.TcpClient client = null;
        public string ExepcName = "EXEPC3100";
        /// <summary>
        /// 连接WIFI
        /// </summary>
        public void Connect()
        {
            if (client != null)
            {
                if (client.Connected == true)
                    return;
            }
            IPAddress myIPClient = IPAddress.Parse("192.168.1.1");
            IPEndPoint MyServer = new IPEndPoint(myIPClient, Int32.Parse("8899"));
            client = new TcpClient();
            try
            {
                client.Connect(MyServer);
                if (Messager != null)
                {
                    Messager("WIFI连接成功");
                }
                if (ConnectStatus != null)
                {
                    isConnected = true;
                    ConnectStatus(isConnected);
                }
                Thread thread = new Thread(new ThreadStart(accpClient));
                thread.IsBackground = true;
                thread.Start();
            }
            catch (Exception ex)
            {
                if (Messager != null)
                {
                    Messager("WIFI连接失败," + ex.Message);
                }
                client = null;
                if (ConnectStatus != null)
                {
                    isConnected = false;
                    ConnectStatus(isConnected);
                }
            }
        }
        /// <summary>
        /// 设置校准
        /// </summary>
        public void SetCalibration(List<float> pointList)
        {
            string head = "7D7B";
            string code = "01F501F33D66000A00";
            string end = "7D7D";
            if (client != null)
            {
                if (client.Connected == true)
                {
                    List<byte> byteSource = new List<byte>();
                    List<byte> valueList = new List<byte>();
                    byte[] headbyte = HexStringToBytes(head);
                    byteSource.AddRange(headbyte);
                    byte[] codebyte = HexStringToBytes(code);
                    byteSource.AddRange(codebyte);
                    valueList.AddRange(codebyte);
                    byte[] numbyte = HexStringToBytes("0" + pointList.Count.ToString());
                    byteSource.AddRange(numbyte);
                    valueList.AddRange(numbyte);
                    for (int i = 0; i < pointList.Count; i++)
                    {
                        byte[] valuebyte = BitConverter.GetBytes(pointList[i]);
                        Array.Reverse(valuebyte);
                        byteSource.AddRange(valuebyte);
                        valueList.AddRange(valuebyte);
                    }
                    byte[] crcbyte = CRC16(valueList.ToArray());
                    byteSource.AddRange(crcbyte);
                    byte[] endbyte = HexStringToBytes(end);
                    byteSource.AddRange(endbyte);
                    string comd = BytesToHexString(byteSource.ToArray());
                    client.Client.Send(byteSource.ToArray());
                }
            }
        }
        /// <summary>
        /// 点火
        /// </summary>
        public void FireAction()
        {
            if (client != null)
            {
                if (client.Connected == true)
                {
                    //16进制点火命令
                    string fireCommand = "7D7B01F501F33A660001034B047D7D";
                    Byte[] bytes = HexStringToBytes(fireCommand);
                    client.Client.Send(bytes);
                }
            }
        }
        /// <summary>
        /// 读取校准
        /// </summary>
        public void ReadCalibration()
        {
            string head = "7D7B";
            string adress = "01F501F3";
            string code = "3D550001";
            string fid = "00";
            string pid = "01";
            string end = "7D7D";

            if (client != null)
            {
                if (client.Connected == true)
                {
                    byte[] headbyte = HexStringToBytes(head);
                    byte[] valuebyte = HexStringToBytes(adress + code + fid);
                    byte[] crcbyte = CRC16(valuebyte);
                    byte[] endbyte = HexStringToBytes(end);
                    List<byte> byteSource = new List<byte>();
                    byteSource.AddRange(headbyte);
                    byteSource.AddRange(valuebyte);
                    byteSource.AddRange(crcbyte);
                    byteSource.AddRange(endbyte);
                    client.Client.Send(byteSource.ToArray());
                }
            }
        }

        //数据采集返回
        public delegate void DataPollEventHandler(float FID, float PID);
        public event DataPollEventHandler DataPoll;

        //点火返回
        public delegate void FireResultEventHandler(bool isSuccess);
        public event FireResultEventHandler FireResult;

        //读取校准返回
        public delegate void ReadCalibrationResultEventHandler(List<float> calibrationList);
        public event ReadCalibrationResultEventHandler ReadCalibrationResult;

        //设置校准返回
        public delegate void SetCalibrationResultEventHandler(bool isSuccess);
        public event SetCalibrationResultEventHandler SetCalibrationResult;

        public bool isConnected = false;
        //连接状态
        public delegate void ConnectStatusEventHandler(bool isConnect);
        public event ConnectStatusEventHandler ConnectStatus;


        //提示消息
        public delegate void MessagerEventHandler(string msg);
        public event MessagerEventHandler Messager;

        string valueHead = "7D7B01F301F560AA0008";
        string fireHead = "7D7B01F301F53A990001";
        string readHead = "7D7B01F301F53DAA";
        string setHead = "7D7B01F301F53D990001";
        private void accpClient()
        {
            try
            {
                string result = "";
                while (true)
                {
                    NetworkStream netStream = client.GetStream();
                    byte[] Rec = new byte[1];
                    netStream.Read(Rec, 0, Rec.Length);
                    result += BytesToHexString(Rec);
                    if (result.Length > 4 && result.Substring(result.Length - 4, 4) == "7D7D")
                    {
                        //检测数据
                        if (result.Substring(0, 20) == valueHead)
                        {
                            string vaule = "7D7B01F301F560AA0008C2C60000000000007D7D";
                            byte[] fidValue = HexStringToBytes(result.Substring(20, 8));
                            byte[] pidValue = HexStringToBytes(result.Substring(28, 8));
                            //高低位转换
                            Array.Reverse(fidValue);
                            Array.Reverse(pidValue);
                            float fid = BitConverter.ToSingle(fidValue, 0);
                            float pid = BitConverter.ToSingle(pidValue, 0);
                            if (DataPoll != null)
                            {
                                DataPoll(fid, pid);
                            }
                        }
                        //点火数据
                        else if (result.Substring(0, 20) == fireHead)
                        {
                            string vaule = "7D7B01F301F53A99000188BB3B7D7D";
                            string fireValue = result.Substring(20, 2);
                            if (fireValue == "88")
                            {
                                if (FireResult != null)
                                {
                                    FireResult(true);
                                }
                            }
                            else if (fireValue == "99")
                            {
                                if (FireResult != null)
                                {
                                    FireResult(false);
                                }
                            }
                        }
                        else if (result.Substring(0, 16) == readHead)
                        {
                            string value = "7D7B01F301F53DAA00260009,43FA0000,44480000,44FA0000,453B8000,459C4000,461C4000,466A6000,469C4000,46EA6000,CF0A7D7D";
                            string valueList = "500,800,2000,3000,5000,10000,15000,20000,30000";
                            int pointNum = int.Parse(result.Substring(22, 2));
                            List<float> pointList = new List<float>();
                            for (int i = 0; i < pointNum; i++)
                            {
                                byte[] pointValue = HexStringToBytes(result.Substring(24 + i * 8, 8));
                                //高低位转换
                                Array.Reverse(pointValue);
                                float point = BitConverter.ToSingle(pointValue, 0);
                                pointList.Add(point);
                            }
                            if (ReadCalibrationResult != null)
                            {
                                ReadCalibrationResult(pointList);
                            }
                        }
                        //校准设置
                        else if (result.Substring(0, 20) == setHead)
                        {
                            string vaule = "7D7B01F301F53A99000188BB3B7D7D";
                            string fireValue = result.Substring(20, 2);
                            if (fireValue == "88")
                            {
                                if (SetCalibrationResult != null)
                                {
                                    SetCalibrationResult(true);
                                }
                            }
                            else if (fireValue == "99")
                            {
                                if (SetCalibrationResult != null)
                                {
                                    SetCalibrationResult(false);
                                }
                            }
                        }
                        result = "";
                    }
                }
            }
            catch (Exception ex)
            {
                client.Close();
                client = null;
                if (Messager != null)
                {
                    Messager("接收数据发生错误" + ex.Message);
                }
                if (ConnectStatus != null)
                {
                    isConnected = false;
                    ConnectStatus(isConnected);
                }
            }
        }

        public static byte[] CRC16(byte[] data)
        {
            int len = data.Length;
            if (len > 0)
            {
                ushort crc = 0xFFFF;
                for (int i = 0; i < len; i++)
                {
                    crc = (ushort)(crc ^ (data[i]));
                    for (int j = 0; j < 8; j++)
                    {
                        crc = (crc & 1) != 0 ? (ushort)((crc >> 1) ^ 0xA001) : (ushort)(crc >> 1);
                    }
                }
                byte hi = (byte)((crc & 0xFF00) >> 8); //高位置
                byte lo = (byte)(crc & 0x00FF); //低位置
                return new byte[] { lo, hi };
            }
            return new byte[] { 0, 0 };
        }
        /// <summary>
        /// 16进制字符转bytes
        /// </summary>
        /// <param name="hs"></param>
        /// <returns></returns>
        private byte[] HexStringToBytes(string hs)
        {
            string strTemp = "";
            byte[] b = new byte[hs.Length / 2];
            for (int i = 0; i < hs.Length / 2; i++)
            {
                strTemp = hs.Substring(i * 2, 2);
                b[i] = Convert.ToByte(strTemp, 16);
            }
            //按照指定编码将字节数组变为字符串
            return b;
        }
        /// <summary>
        /// bytes转16进制字符
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        private string BytesToHexString(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder(bytes.Length * 3);
            foreach (byte b in bytes)
            {
                sb.Append(Convert.ToString(b, 16).PadLeft(2, '0'));
            }
            return sb.ToString().ToUpper();
        }
    }
    /// <summary>
    /// Exepc设备采集数据类型
    /// </summary>
    public enum ExpecDataTypeEnum
    {
        FID,
        FID_PID
    }
}