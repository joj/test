using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace PhxGaugingAndroid.Entity
{
    public class GroupStyle
    {
        //连接线的颜色
        public int lineColor;
        //连接线宽度
        public int lineWidth;
        //密封点颜色
        public int pointColor;
        //密封点半径
        public int pointSize;
        //密封点标识信息方框的背景色
        public int textBackgroundColor;
        //文字颜色
        public int textColor;
        //默认给-986896
        public int textHintColor = -986896;
        //文字边距左
        public int textPaddingLeft;
        //文字边距上
        public int textPaddingTop;
        //文字边距右
        public int textPaddingRight;
        //文字边距下
        public int textPaddingBottom;
        //文字大小
        public int textSize;
        //密封点
        public List<PointStyle> markList;
    }
    public class PointStyle
    {
        //密封点标签内容
        public string hintText;
        //密封点标签内容
        public string markText;
        //连接线连接的位置 0左 1上 2右 3下
        public int pointDirection;
        //密封点中心点X坐标
        public double pointX;
        //密封点中心点Y坐标
        public double pointY;
        //文字左下角X坐标
        public double textX;
        //文字左下角Y坐标
        public double textY;
        //密封点信息
        public string exts;
    }
}