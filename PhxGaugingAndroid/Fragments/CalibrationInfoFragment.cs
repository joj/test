using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using PhxGaugingAndroid.Common;
using PhxGaugingAndroid.Entity;

namespace PhxGaugingAndroid.Fragments
{
    public class CalibrationInfoFragment : Fragment
    {
        public delegate void GoBack();
        public event GoBack goBack;

        ClearnEditText tvDeviceName;
        ClearnEditText tvDeviceCode;
        ClearnEditText tvGasName;
        ClearnEditText tvPhxName;
        ClearnEditText tvPhxType;
        ClearnEditText tvModel;
        ClearnEditText tvPhxCode;
        ClearnEditText tvUser;
        ClearnEditText tvConfirm;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
            context = this.Activity;
        }
        Activity context;
        public static CalibrationInfoFragment NewInstance()
        {
            var frag = new CalibrationInfoFragment { Arguments = new Bundle() };
            return frag;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View v = inflater.Inflate(Resource.Layout.CalibrationInfo, container, false);
            Button button = v.FindViewById<Button>(Resource.Id.CalibrationBtnGotoLog);
            button.Click -= Button_Click;
            button.Click += Button_Click;
            tvDeviceName = v.FindViewById<PhxGaugingAndroid.Fragments.ClearnEditText>(Resource.Id.CalibrationTextDeviceName);
            tvDeviceCode = v.FindViewById<PhxGaugingAndroid.Fragments.ClearnEditText>(Resource.Id.CalibrationTextDeviceCode);
            tvGasName = v.FindViewById<PhxGaugingAndroid.Fragments.ClearnEditText>(Resource.Id.CalibrationTextGasName);
            tvPhxName = v.FindViewById<PhxGaugingAndroid.Fragments.ClearnEditText>(Resource.Id.CalibrationTextPhxName);
            tvPhxType = v.FindViewById<PhxGaugingAndroid.Fragments.ClearnEditText>(Resource.Id.CalibrationTextPhxType);
            tvModel = v.FindViewById<PhxGaugingAndroid.Fragments.ClearnEditText>(Resource.Id.CalibrationTextModel);
            tvPhxCode = v.FindViewById<PhxGaugingAndroid.Fragments.ClearnEditText>(Resource.Id.CalibrationTextPhxCode);
            if (App.phxDeviceService != null)
            {
                tvPhxCode.Text = App.phxDeviceService.Name;
            }
            tvUser = v.FindViewById<PhxGaugingAndroid.Fragments.ClearnEditText>(Resource.Id.CalibrationTextUser);
            string json = Utility.DecryptDES(UserPreferences.GetString("CrrentUser"));
            var JsonModel = JsonConvert.DeserializeObject<AndroidUser>(json);
            tvUser.Text = JsonModel.UserName;
            tvConfirm = v.FindViewById<PhxGaugingAndroid.Fragments.ClearnEditText>(Resource.Id.CalibrationTextConfirm);

            return v;
        }
        /// <summary>
        /// 开始校准
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, EventArgs e)
        {
            AndroidCalibration cali = new AndroidCalibration();
            cali.DeviceName = tvDeviceName.Text.Trim();
            cali.DeviceCode = tvDeviceCode.Text.Trim();
            cali.GasName = tvGasName.Text.Trim();
            cali.PhxName = tvPhxName.Text.Trim();
            cali.PhxType = tvPhxType.Text.Trim();
            cali.Model = tvModel.Text.Trim();
            cali.PhxCode = tvPhxCode.Text.Trim();
            cali.User = tvUser.Text.Trim();
            cali.Confirm = tvConfirm.Text.Trim();
            Intent i = new Intent(base.Activity, typeof(CalibrationLogActivity));
            Bundle b = new Bundle();
            b.PutString("AndroidCalibration", Newtonsoft.Json.JsonConvert.SerializeObject(cali));
            i.PutExtras(b);
            StartActivityForResult(i, 123);
        }

        public override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode == 123 && resultCode == Result.Ok)
            {
                goBack();
            }
        }
    }
}