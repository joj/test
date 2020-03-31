using Android.Views;
using Android.Widget;
using Java.IO;
using Java.Net;
using Java.Util;
using Newtonsoft.Json;
using PhxGaugingAndroid.Entity;
using RestSharp;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace PhxGaugingAndroid.Common
{
    public class Utility
    {
        public static void SetListViewHeightBasedOnChildren(ListView listView)
        {
            IListAdapter listAdapter = listView.Adapter;
            if (listAdapter == null)
            {
                return;
            }
            int totalHeight = 0;
            for (int i = 0; i < listAdapter.Count; i++)
            {
                View listItem = listAdapter.GetView(i, null, listView);
                listItem.Measure(0, 0);
                totalHeight += listItem.MeasuredHeight;
            }

            ViewGroup.LayoutParams param = listView.LayoutParameters;
            param.Height = totalHeight + (listView.DividerHeight * (listAdapter.Count - 1));
            listView.LayoutParameters = param;
        }
        //DES加密秘钥，要求为8位  
        private const string desKey = "xianglk1";
        //默认密钥向量
        private static byte[] Keys = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };

        /// <summary>  
        /// DES加密  
        /// </summary>  
        /// <param name="encryptString">待加密的字符串，未加密成功返回原串</param>  
        /// <returns></returns>  
        public static string EncryptDES(string encryptString)
        {
            try
            {
                byte[] rgbKey = Encoding.UTF8.GetBytes(desKey);
                byte[] rgbIV = Keys;
                byte[] inputByteArray = Encoding.UTF8.GetBytes(encryptString);
                DESCryptoServiceProvider dCSP = new DESCryptoServiceProvider();
                MemoryStream mStream = new MemoryStream();
                CryptoStream cStream = new CryptoStream(mStream, dCSP.CreateEncryptor(rgbKey, rgbIV), CryptoStreamMode.Write);
                cStream.Write(inputByteArray, 0, inputByteArray.Length);
                cStream.FlushFinalBlock();
                return Convert.ToBase64String(mStream.ToArray());
            }
            catch
            {
                return encryptString;
            }
        }

        /// <summary>  
        /// DES解密  
        /// </summary>  
        /// <param name="decryptString">待解密的字符串，未解密成功返回原串</param>  
        /// <returns></returns>  
        public static string DecryptDES(string decryptString)
        {
            try
            {
                byte[] rgbKey = Encoding.UTF8.GetBytes(desKey);
                byte[] rgbIV = Keys;
                byte[] inputByteArray = Convert.FromBase64String(decryptString);
                DESCryptoServiceProvider DCSP = new DESCryptoServiceProvider();
                MemoryStream mStream = new MemoryStream();
                CryptoStream cStream = new CryptoStream(mStream, DCSP.CreateDecryptor(rgbKey, rgbIV), CryptoStreamMode.Write);
                cStream.Write(inputByteArray, 0, inputByteArray.Length);
                cStream.FlushFinalBlock();
                return Encoding.UTF8.GetString(mStream.ToArray());
            }
            catch
            {
                return decryptString;
            }
        }
        /// <summary>
        /// 转换时间戳为C#时间
        /// </summary>
        /// <param name="timeStamp">时间戳 单位：毫秒</param>
        /// <returns>C#时间</returns>
        public static DateTime ConvertTimeStampToDateTime(long timeStamp)
        {
            DateTime defaultTime = new DateTime(1970, 1, 1, 0, 0, 0);
            long defaultTick = defaultTime.Ticks;
            long timeTick = defaultTick + timeStamp * 10000;
            //// 东八区 要加上8个小时
            DateTime dt = new DateTime(timeTick).AddHours(8);
            return dt;
        }
        /// <summary>
        /// DateTime时间格式转换为Unix时间戳格式
        /// </summary>
        /// <param name="time"> DateTime时间格式</param>
        /// <returns>Unix时间戳格式  单位：毫秒</returns>
        public static long ConvertToTimestamp(DateTime time)
        {
            DateTime startTime = System.TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            return (long)(time - startTime).TotalSeconds * 1000;
        }

        /// <summary>
        /// 请求api
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="url">请求API地址</param>   
        /// <param name="parms">请求API参数</param>
        /// <param name="token">APItoken</param>
        /// <returns></returns>
        public static BaseResult<T> CallService<T>(string url, object parms, int time, string token = null)
        {
            BaseResult<T> result = new BaseResult<T>();
            try
            {
                //构建客户端对象

#warning 部署前修改回来
                string url1 = @"http://vocs.cnldar.com/vocs/";
//#if DEBUG
//                url1 = @"http://192.168.0.136/VOCs/";
//                //RestClient client = new RestClient("http://192.168.0.136/VOCs/");
//#endif

                RestClient client = new RestClient(url1);
               // 
                //构建请求，getAppUrl请求地址
                RestRequest request = new RestRequest(url, RestSharp.Method.POST);
                //设置请求头部参数
                request.AddHeader("Authorization", "basic weixin|363F55B8-203E-4BE4-BB76-AAF7F99FF369" + ';' + token);
                //JSON 对象作为POST请求的数据包主体
                request.AddJsonBody(parms);
                client.Timeout = time;
                //获取返回值
                var _result = client.Execute(request);
                result = JsonConvert.DeserializeObject<BaseResult<T>>(_result.Content);
                return result;
            }
            catch (Exception ex)
            {
                result.Code = 100001;
                result.Message = "调用API错误:" + ex.Message;
                return result;
            }
        }
        /// <summary>
        /// 请求api
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="api">API服务器地址</param>
        /// <param name="url">请求API地址</param>
        /// <param name="parms">请求API参数</param>
        /// <param name="time">超时时间</param>
        /// <param name="token">APItoken</param>
        /// <returns></returns>
        public static BaseResult<T> CallService<T>(string api,string url, object parms, int time, string token = null)
        {
            BaseResult<T> result = new BaseResult<T>();
            try
            {
                //构建客户端对象
                RestClient client = new RestClient(api);
                //构建请求，getAppUrl请求地址
                RestRequest request = new RestRequest(url, RestSharp.Method.POST);
                //设置请求头部参数
                request.AddHeader("Authorization", "basic weixin|363F55B8-203E-4BE4-BB76-AAF7F99FF369" + ';' + token);
                //JSON 对象作为POST请求的数据包主体
                request.AddJsonBody(parms);
                client.Timeout = time;
                //获取返回值
                var _result = client.Execute(request);
                result = JsonConvert.DeserializeObject<BaseResult<T>>(_result.Content);
                return result;
            }
            catch (Exception ex)
            {
                result.Code = 100001;
                result.Message = "调用API错误:" + ex.Message;
                return result;
            }
        }

        /// <summary>
        /// 获取mac地址
        /// </summary>
        /// <returns></returns>
        public static String getLocalMacAddress()
        {
            String mac = null;
            try
            {
                String path = "sys/class/net/eth0/address";
                FileInputStream fis_name = new FileInputStream(path);
                byte[] buffer_name = new byte[17];
                int byteCount_name = fis_name.Read(buffer_name);
                Char[] StrMac = Encoding.ASCII.GetChars(buffer_name);
                if (byteCount_name > 0)
                {
                    foreach (var item in StrMac)
                    {
                        mac += item;
                    }
                }
                if (mac == null)
                {
                    fis_name.Close();
                    return "";
                }
                fis_name.Close();
            }
            catch (Exception io)
            {
                String path = "sys/class/net/wlan0/address";
                FileInputStream fis_name;
                try
                {
                    fis_name = new FileInputStream(path);
                    byte[] buffer_name = new byte[17];
                    int byteCount_name = fis_name.Read(buffer_name);
                    Char[] StrMac = Encoding.ASCII.GetChars(buffer_name);
                    if (byteCount_name > 0)
                    {
                        foreach (var item in StrMac)
                        {
                            mac += item;
                        }
                    }
                    fis_name.Close();
                }
                catch (Exception e)
                {
                    var s = e.Message;
                }
            }

            if (mac == null)
            {
                return "";
            }
            else
            {
                return mac.Trim().ToUpper();
            }

        }
        /// <summary>
        /// 获取mac地址  6.0系统必须开启WiFi才可以获取到
        /// </summary>
        /// <returns></returns>
        public static String getMacAddress()
        {
            try
            {
                var all = Collections.List(NetworkInterface.NetworkInterfaces);
                foreach (NetworkInterface item in all)
                {
                    if (!item.Name.Equals("wlan0"))
                    {
                    }
                    else
                    {
                        byte[] macBytes = item.GetHardwareAddress();
                        if (macBytes == null)
                        {
                            return null;
                        }
                        StringBuilder res1 = new StringBuilder();

                        foreach (byte b in macBytes)
                        {
                            res1.Append(b.ToString("X2") + ":");
                        }
                        return res1.ToString().TrimEnd(':');
                    }
                }
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }
            return null;
        }
    }
}