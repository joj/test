using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using PhxGaugingAndroid.Entity;
using HashCode = PhxGaugingAndroid.Entity.HashCode;

namespace PhxGaugingAndroid.Common
{
    public class FileHelper
    {
        /// <summary>
        /// 获取文件的MD5码
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static string GetMD5HashFromFile(string fileName)
        {
            using (FileStream file = new FileStream(fileName, System.IO.FileMode.Open, FileAccess.Read))
            {
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }
        /// <summary>
        /// 获取文件的SHA1码
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static string GetSha1FromFile(string fileName)
        {
            using (FileStream file = new FileStream(fileName, System.IO.FileMode.Open, FileAccess.Read))
            {
                SHA1 sha1 = new SHA1CryptoServiceProvider();
                byte[] retVal = sha1.ComputeHash(file);
                file.Close();
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }
        /// <summary>
        /// 生成Hash校验码并上传服务器
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetHashCodeFromFile(string filePath, EnumDataType type, string url = "")
        {
            //获取Hash码
            string hashCode = GetSha1FromFile(filePath);
            string newPath = filePath.Replace(".xlsx", "_" + hashCode + ".xlsx");
            //重命名文件
            //File.Move(filePath, newPath);
            Java.IO.File file = new Java.IO.File(filePath);
            file.RenameTo(new Java.IO.File(newPath));
            //Hash码上传服务器
            string json = Utility.DecryptDES(UserPreferences.GetString("CrrentUser"));
            var JsonModel = JsonConvert.DeserializeObject<AndroidUser>(json);
            HashCode model = new HashCode { SHA1Code = hashCode, BizId = type, InUserName = JsonModel.UserName, FileName = Path.GetFileName(newPath) };
            //统一从云平台服务器校验
            //if ((type == EnumDataType.Check || type == EnumDataType.ReCheck) && url != null && url != "")
            //{
            //    Utility.CallService<bool>(url, "V10Enterprise/LdarSHA1MD5/Add", model, 10000, null);
            //}
            //else
            //{
            //    Utility.CallService<bool>("V10Enterprise/LdarSHA1MD5/Add", model, 10000, null);
            //}
            Utility.CallService<bool>("V10Enterprise/LdarSHA1MD5/Add", model, 10000, null);
            return newPath;
        }
    }

    public enum EnumDataType
    {
        //检测
        Check = 0,
        //复检
        ReCheck = 1,
        //校准
        Calibration = 2,
        //背景值
        Background = 3
    }
}