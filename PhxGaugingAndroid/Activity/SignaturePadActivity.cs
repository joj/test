using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using PhxGaugingAndroid.Common;
using PhxGaugingAndroid.Fragments;
using Xamarin.Controls;
using Environment = Android.OS.Environment;

namespace PhxGaugingAndroid
{
    [Activity(Label = "SignaturePadActivity")]
    public class SignaturePadActivity : Activity
    {
        private SignaturePadView signatureView;
        private EditText etCheckName;
        private string Type;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            this.Type = this.Intent.GetStringExtra("SignatureType");
            var path = this.Intent.GetStringExtra("WorkOrderPath");
            var name = this.Intent.GetStringExtra("WorkOrderName");
            SetContentView(Resource.Layout.SignaturePad);
            signatureView = FindViewById<SignaturePadView>(Resource.Id.signatureView);
            signatureView.StrokeWidth = 3F;
            var btnSave = FindViewById<Button>(Resource.Id.btnSignaSave);
            var btnCancel = FindViewById<Button>(Resource.Id.btnSignaCancel);
            etCheckName = FindViewById<EditText>(Resource.Id.etCheckName);
            if (this.Type == "审核人签名")
            {
                etCheckName.Visibility = ViewStates.Visible;
            }
            btnSave.Click += delegate
            {
                try
                {
                    string file;
                    if (this.Type == "审核人签名")
                    {
                        if (etCheckName.Text.Trim() == string.Empty)
                        {
                            Toast.MakeText(this, "审核人姓名不能为空!", ToastLength.Short).Show();
                            return;
                        }
                        file = System.IO.Path.Combine(path, "signatureDouble.Png");

                        if (File.Exists(file))
                        {
                            File.Delete(file);
                        }

                        using (var bitmap = signatureView.GetImage(Android.Graphics.Color.Black, Android.Graphics.Color.White, 0.3f))
                        {

                            if (bitmap != null)
                            {
                                using (var dest = File.OpenWrite(file))
                                {
                                    bitmap.Compress(Android.Graphics.Bitmap.CompressFormat.Png, 80, dest);
                                }
                            }
                        
                        }

                        UserPreferences.SetString(name, etCheckName.Text.Trim());
                    }
                    else
                    {
                        file = System.IO.Path.Combine(path, "signature.Png");

                        if (File.Exists(file))
                        {
                            File.Delete(file);
                        }
                        using (var bitmap = signatureView.GetImage(Android.Graphics.Color.Black, Android.Graphics.Color.White, 0.3f))
                        {

                            if (bitmap == null)
                            {
                                Toast.MakeText(this, "请签名!", ToastLength.Short).Show();
                                return;
                            }
                            using (var dest = File.OpenWrite(file))
                            {
                                bitmap.Compress(Android.Graphics.Bitmap.CompressFormat.Png, 80, dest);
                            }
                        }
                    }

                    //if (this.Type == "审核人签名")
                    //{
                    //    UserPreferences.SetString(name, etCheckName.Text.Trim());
                    //}
                    Finish();
                }
                catch (Exception ex)
                {
                    LogHelper.ErrorLog("签名", ex);
                    throw ex;
                }
            };
            btnCancel.Click += delegate
            {
                Finish();
            };
        }
    }
}