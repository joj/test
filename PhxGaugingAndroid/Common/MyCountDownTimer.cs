using Android.OS;
using Android.Widget;
using System;

namespace PhxGaugingAndroid.Common
{
    public class MyCountDownTimer : CountDownTimer
    {
        Button Newbtn;
        public MyCountDownTimer(long millisInFuture, long countDownInterval, Button btn) : base(millisInFuture, countDownInterval)
        {
            Newbtn = btn;
        }
        public  void OnFinish(Button btn)
        {
            btn.Text = "重新获取验证码";
            //设置可点击  
            btn.Clickable = true;
        }

        public override void OnFinish()
        {
            OnFinish(Newbtn);
        }

        public  void OnTick(long millisUntilFinished,Button btn)
        {
            btn.Clickable = false;
            btn.Text = millisUntilFinished / 1000 + "s后点击重发";
        }

        public override void OnTick(long millisUntilFinished)
        {
            OnTick(millisUntilFinished, Newbtn);
        }
    }
}