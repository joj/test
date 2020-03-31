using Android.App;
using Android.Widget;
using Android.OS;
using PhxGauging.Common.Android.Services;
using Android.Bluetooth;
using PhxGauging.Common.Devices.Services;
using System.Collections.Generic;
using System;
using Android.Util;
using Android.Runtime;
using PhxGauging.Common.Services;

namespace PhxGauging
{
    [Activity(Label = "设备列表")]
    public class MainActivity : Activity
    {
        public AndroidBluetoothService androidBluetoothService;

        private List<BluetoothDevice> bluetoothsDevices;

        public ListView listView;

        public PhxDeviceService _phxDeviceService;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            //this.androidBluetoothService = new AndroidBluetoothService(this);
            //Button button = base.FindViewById<Button>(Resource.Id.btnRefresh);
            //button.Click += new EventHandler(this.bluetoothButtonClick);
            //this.listView = base.FindViewById<ListView>(Resource.Id.PhxlistView);
            //this.listView.ItemClick += new EventHandler<AdapterView.ItemClickEventArgs>(this.listView_ItemClick);
        }

        public void bluetoothButtonClick(object sender, EventArgs e)
        {
            androidBluetoothService.StartDiscovery();
            Button button = base.FindViewById<Button>(Resource.Id.btnRefresh);
            this.bluetoothsDevices = this.androidBluetoothService.receiver.getBluetooths();
            List<IDictionary<string, object>> list = new List<IDictionary<string, object>>();
            foreach (BluetoothDevice current in this.bluetoothsDevices)
            {
                list.Add(new JavaDictionary<string, object>
                    {
                        {
                            "itemTitle",
                            current.Name
                        },
                        {
                            "itemObject",
                            current
                        }
                    });
            }
            SimpleAdapter adapter = new SimpleAdapter(this, list, Resource.Layout.Phx21, new string[]
            {
                    "itemTitle",
                    "itemText",
                    "itemObject"
            }, new int[]
            {
                    Resource.Id.itemTitle,
                    Resource.Id.itemText
            });
            this.listView.Adapter = adapter;
        }

        private void listView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            Toast.MakeText(this, "Click: " + this.bluetoothsDevices[e.Position].Name, ToastLength.Short).Show();
            this.androidBluetoothService.bluetoothDevice = this.bluetoothsDevices[e.Position];
            if (this.androidBluetoothService.bluetoothDevice != null)
            {
                IDevice device = null;
                try
                {
                    this.androidBluetoothService.Connect(device);
                    Log.Info("phxdemo", "connetSuccess");
                }
                catch
                {
                    Log.Info("phxdemo", "connetFail");
                }
            }
            Log.Info("phxdemo", "connetSuccess2");
        }
    }
}

