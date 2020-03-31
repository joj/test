using Android.OS;
using Android.App;
using Android.Views;
using Android.Widget;
using System;
using PhxGaugingAndroid.Entity;
using PhxGaugingAndroid.Common;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Android.Content;
using Android.Graphics.Drawables;
using Newtonsoft.Json;
using Android.Text;
using Android.Util;
using Android.Runtime;
using Java.Lang;

namespace PhxGaugingAndroid.Fragments
{
    public class GroupFragment : Fragment
    {
        private AndroidWorkOrder workOrder;

        public GroupFragment(AndroidWorkOrder workOrder)
        {
            this.workOrder = workOrder;
        }

        public delegate void GotoSealPointEventHandler(Fragment f);
        public event GotoSealPointEventHandler GotoSealPoint;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your fragment here
            context = this.Activity;
        }
        Activity context;
        List<AndroidGroup> groupList;
        List<string> areas = new List<string>() { "全部" };
        Spinner spArea;
        ClearnEditText etGroupCode;
        ListView lvGroups;
        RadioGroup rdGroupCheck;
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View v = inflater.Inflate(Resource.Layout.Group, container, false);
            rdGroupCheck = v.FindViewById<RadioGroup>(Resource.Id.rdGroupCheck);
            lvGroups = v.FindViewById<ListView>(Resource.Id.lvGroups);
            lvGroups.ItemClick -= LvGroups_ItemClick;
            lvGroups.ItemClick += LvGroups_ItemClick;
            string CrrentUser = UserPreferences.GetString("CrrentUser");
            if (!string.IsNullOrEmpty(CrrentUser))
            {
                string json = Utility.DecryptDES(CrrentUser);
                var JsonModel = JsonConvert.DeserializeObject<AndroidUser>(json);
                if (JsonModel.IsBatchTest == 1)
                {
                    RegisterForContextMenu(lvGroups);
                }
            }
            spArea = v.FindViewById<Spinner>(Resource.Id.spArea);
            Button btnQueryOrder = v.FindViewById<Button>(Resource.Id.btnQueryOrder);
            btnQueryOrder.Click -= BtnQueryOrder_Click;
            btnQueryOrder.Click += BtnQueryOrder_Click;
            etGroupCode = v.FindViewById<ClearnEditText>(Resource.Id.etGroupCode);
            Button btnQBar = v.FindViewById<Button>(Resource.Id.btnGroupBar);
            btnQBar.Click -= BtnQBar_Click;
            btnQBar.Click += BtnQBar_Click;
            SQLite.SQLiteConnection connection = null;
            try
            {
                connection = new SQLite.SQLiteConnection(workOrder.DataPath);
                if (groupList == null)
                {
                    groupList = connection.Table<AndroidGroup>().ToList();
                    string dir = workOrder.DataPath.Replace("sqlite.db", "img");
                    groupList.ForEach(c => { c.ImgPath = Path.Combine(dir, c.GroupCode + ".jpg"); c.DataPath = workOrder.DataPath; });
                    areas.AddRange((from item in groupList
                                    group item by item.AreaName into m
                                    select m.Key
                 ).ToList());
                }
                lvGroups.Adapter = new GroupAdapter(this.Activity, groupList);
                spArea.Adapter = new ArrayAdapter(this.Activity, Resource.Layout.ListViewItem, areas);
                spArea.Prompt = "区域选择";
            }
            catch (System.Exception ex)
            {
                Toast.MakeText(this.Activity, "发生错误：" + ex.Message, ToastLength.Short).Show();
                LogHelper.ErrorLog("初始化群组数据列表", ex);
            }
            finally
            {
                if (connection != null)
                {
                    connection.Dispose();
                }
            }
            return v;
        }
        private void BtnQBar_Click(object sender, EventArgs e)
        {
            try
            {
                Intent i = new Intent(base.Activity, typeof(ScanBarCodeActivity));
                StartActivityForResult(i, 788);
            }
            catch (System.Exception ex)
            {
                LogHelper.ErrorLog("检测调用群组条形码扫描", ex);
            }
        }

        /// <summary>
        /// 创建菜单
        /// </summary>
        /// <param name="menu"></param>
        /// <param name="v"></param>
        /// <param name="menuInfo"></param>
        public override void OnCreateContextMenu(IContextMenu menu, View v, IContextMenuContextMenuInfo menuInfo)
        {
            if (v.Id == Resource.Id.lvGroups)
            {
                var info = (AdapterView.AdapterContextMenuInfo)menuInfo;
                menu.SetHeaderTitle(groupList[info.Position].GroupName);
                string[] menuItems = new string[1] { "批量检测" };
                for (var i = 0; i < menuItems.Length; i++)
                    menu.Add(Menu.None, i, i, menuItems[i]);
            }
        }
        /// <summary>
        /// 菜单事件
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override bool OnContextItemSelected(IMenuItem item)
        {
            var info = (AdapterView.AdapterContextMenuInfo)item.MenuInfo;
            var menuItemIndex = item.ItemId;
            string[] menuItems = new string[1] { "批量检测" };
            var menuItemName = menuItems[menuItemIndex];
            if (menuItemName == "批量检测")
            {
                Intent i = new Intent(this.Activity, typeof(GeneratePPMActivity));
                Bundle b = new Bundle();
                b.PutString("AndroidGroup", Newtonsoft.Json.JsonConvert.SerializeObject(groupList[info.Position]));
                i.PutExtras(b);
                this.StartActivityForResult(i, 136);
                return true;
            }
            return false;
        }

        public override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode == 136 && resultCode == Result.Ok)
            {
                string value = data.GetStringExtra("AndroidGroup");
                if (value != null && value != string.Empty)
                {
                    AndroidGroup group = Newtonsoft.Json.JsonConvert.DeserializeObject<AndroidGroup>(value);
                    for (int i = 0; i < groupList.Count; i++)
                    {
                        if (groupList[i].ID == group.ID)
                        {
                            groupList[i] = group;
                            int firstVisiblePosition = lvGroups.FirstVisiblePosition; //屏幕内当前可以看见的第一条数据
                            var view = lvGroups.GetChildAt(i - firstVisiblePosition);
                            // if (group.CompleteCount == (group.SealPointCount - group.UnReachCount))
                            if (group.CompleteCount == group.SealPointCount)
                            {
                                view.FindViewById<LinearLayout>(Resource.Id.linearGroup).Background = new ColorDrawable(Android.Graphics.Color.Rgb(193, 193, 193));
                            }
                            else
                            {
                                view.FindViewById<LinearLayout>(Resource.Id.linearGroup).Background = new ColorDrawable(Android.Graphics.Color.Rgb(250, 250, 248));
                            }
                            view.FindViewById<TextView>(Resource.Id.tvGroupProgress).Text = "检测进度:" + group.CompleteCount + "/" + (group.SealPointCount - group.UnReachCount).ToString();
                        }
                    }
                    var adapter = lvGroups.Adapter as GroupAdapter;
                    adapter.items = groupList;
                }
            }
            else if (requestCode == 788 && resultCode == Result.Ok)
            {
                string value = data.GetStringExtra("ScanResult");
                if (value != null && value != string.Empty)
                {
                    etGroupCode.Text = value;
                }
            }
        }

        private void BtnQueryOrder_Click(object sender, EventArgs e)
        {
            SQLite.SQLiteConnection connection = null;
            try
            {
                connection = new SQLite.SQLiteConnection(workOrder.DataPath);
                if (etGroupCode.Text.Trim() == "" && spArea.SelectedItemPosition == 0)
                {
                    groupList = connection.Table<AndroidGroup>().ToList();
                }
                else if (etGroupCode.Text.Trim() != "" && spArea.SelectedItemPosition != 0)
                {
                    string query = areas[spArea.SelectedItemPosition];
                    string gr = etGroupCode.Text.Trim();
                    groupList = connection.Table<AndroidGroup>().Where(c => c.AreaName == query && (c.GroupCode.ToUpper().Contains(gr.ToUpper()) || c.GroupName.ToUpper().Contains(gr.ToUpper()) || c.BarCode.ToUpper().Contains(gr.ToUpper()))).ToList();
                }
                else if (etGroupCode.Text.Trim() != "" && spArea.SelectedItemPosition == 0)
                {
                    string gr = etGroupCode.Text.Trim();
                    groupList = connection.Table<AndroidGroup>().Where(c => c.GroupCode.ToUpper().Contains(gr.ToUpper()) || c.GroupName.ToUpper().Contains(gr.ToUpper()) || c.BarCode.ToUpper().Contains(gr.ToUpper())).ToList();
                }
                else if (etGroupCode.Text.Trim() == "" && spArea.SelectedItemPosition != 0)
                {
                    string query = areas[spArea.SelectedItemPosition];
                    groupList = connection.Table<AndroidGroup>().Where(c => c.AreaName == query).ToList();
                }
                switch (rdGroupCheck.CheckedRadioButtonId)
                {
                    case Resource.Id.rbGroupAll:
                        break;
                    case Resource.Id.rbGroupYes:
                        //groupList = groupList.FindAll(c => c.CompleteCount == (c.SealPointCount - c.UnReachCount));
                        groupList = groupList.FindAll(c => c.CompleteCount == c.SealPointCount);
                        break;
                    case Resource.Id.rbGroupNo:
                        //groupList = groupList.FindAll(c => c.CompleteCount != (c.SealPointCount - c.UnReachCount));
                        groupList = groupList.FindAll(c => c.CompleteCount != c.SealPointCount);
                        break;
                }
                string dir = workOrder.DataPath.Replace("sqlite.db", "img");
                groupList.ForEach(c => { c.ImgPath = Path.Combine(dir, c.GroupCode + ".jpg"); c.DataPath = workOrder.DataPath; });
                lvGroups.Adapter = new GroupAdapter(this.Activity, groupList);
            }
            catch (System.Exception ex)
            {
                Toast.MakeText(this.Activity, "发生错误：" + ex.Message, ToastLength.Short).Show();
                LogHelper.ErrorLog("查询群组数据列表", ex);
            }
            finally
            {
                if (connection != null)
                {
                    connection.Dispose();
                }
            }
        }

        /// <summary>
        /// 跳转密封点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LvGroups_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            string LDARDevice = UserPreferences.GetString("SelectLDARDevice");
            if (LDARDevice == "EXEPC3100")
            {
                GotoSealPoint(new ExepcSealPointFragment(groupList, e.Position));
            }
            else if (LDARDevice == "TVA2020")
            {
                GotoSealPoint(new TvaSealPointFragment(groupList, e.Position));
            }
            else
            {
                GotoSealPoint(new SealPointFragment(groupList, e.Position));
            }
        }
    }

    public class ClearnEditText : EditText
    {
        private Drawable mClearDrawable;
        private bool hasFocus;

        public ClearnEditText(Context context) : base(context)
        {
            init();
            this.SetHorizontallyScrolling(true);
        }

        public ClearnEditText(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            init();
            this.SetHorizontallyScrolling(true);
        }

        public ClearnEditText(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
        {
            init();
            this.SetHorizontallyScrolling(true);
        }

        public ClearnEditText(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
            init();
            this.SetHorizontallyScrolling(true);
        }

        protected override void OnFocusChanged(bool gainFocus, [GeneratedEnum] FocusSearchDirection direction, Android.Graphics.Rect previouslyFocusedRect)
        {
            base.OnFocusChanged(gainFocus, direction, previouslyFocusedRect);
            this.hasFocus = gainFocus;
            if (hasFocus && Text.Length > 0)
            {
                setDrawableVisible(true); // 有焦点且有文字时显示图标  
            }
            else
            {
                setDrawableVisible(false);
            }
        }
        public void SetCloseVisible(int textLength)
        {
            if (hasFocus)
            {
                setDrawableVisible(textLength > 0);
            }
        }
        private void init()
        {
            mClearDrawable = GetCompoundDrawables()[2]; // 获取drawableRight  
            if (mClearDrawable == null)
            {
                // 如果为空，即没有设置drawableRight，则使用R.mipmap.close这张图片  
                mClearDrawable = Resources.GetDrawable(Resource.Mipmap.close);
                mClearDrawable.SetBounds(0, 0, this.Height * 3 / 4, this.Height * 3 / 4);
            }
            mClearDrawable.Bounds = new Android.Graphics.Rect(0, 0, mClearDrawable.IntrinsicWidth, mClearDrawable.IntrinsicHeight);
            TextChanged += ClearnEditText_TextChanged;
            // 默认隐藏图标  
            setDrawableVisible(false);
        }

        private void ClearnEditText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (hasFocus)
            {
                setDrawableVisible(e.Text.Count() > 0);
            }
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            if (e.Action == MotionEventActions.Up)
            {
                if (GetCompoundDrawables()[2] != null)
                {
                    int start = Width - TotalPaddingRight + PaddingRight; // 起始位置  
                    int end = Width; // 结束位置  
                    bool available = (e.GetX() > start) && (e.GetX() < end);
                    if (available)
                    {
                        this.Text = "";
                    }
                }
            }
            return base.OnTouchEvent(e);
        }
        protected void setDrawableVisible(bool visible)
        {
            if (Enabled)
            {
                Drawable right = visible ? mClearDrawable : null;
                SetCompoundDrawables(GetCompoundDrawables()[0], GetCompoundDrawables()[1], right, GetCompoundDrawables()[3]);
            }
        }
    }
}