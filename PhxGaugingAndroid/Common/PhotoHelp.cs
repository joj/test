using Android.Graphics;
using Android.Util;
using Android.Views;
using Android.Widget;
using System;
using static Android.Views.View;

namespace PhxGaugingAndroid.Common
{
    public class PhotoHelp : IOnTouchListener
    {
        //记录是拖拉照片模式还是放大缩小照片模式 
        private int mode = 0;// 初始状态 
        //拖拉照片模式 
        private static int MODE_DRAG = 1;
        //放大缩小照片模式 
        private static int MODE_ZOOM = 2;
        //用于记录开始时候的坐标位置 
        private Android.Graphics.PointF startPoint = new Android.Graphics.PointF();
        //用于记录拖拉图片移动的坐标位置 
        private Matrix matrix = new Matrix();
        //用于记录图片要进行拖拉时候的坐标位置  
        private Matrix currentMatrix = new Matrix();
        //两个手指的开始距离  
        private float startDis;
        //两个手指的中间点 
        private Android.Graphics.PointF midPoint;

        public IntPtr Handle => throw new NotImplementedException();

        public bool OnTouch(View v, MotionEvent e)
        {

            throw new NotImplementedException();
        }

        public bool OnTouch(View v, MotionEvent e, ImageView m)
        {
            switch (e.Action & MotionEventActions.Mask)
            {
                // 手指压下屏幕 
                case MotionEventActions.Down:
                    mode = MODE_DRAG;
                    // 记录ImageView当前的移动位置 
                    currentMatrix.Set(m.ImageMatrix);
                    startPoint.Set(e.GetX(), e.GetY());
                    break;
                // 手指在屏幕上移动，改事件会被不断触发 
                case MotionEventActions.Move:
                    if (mode == MODE_DRAG)
                    {
                        float dx = e.GetX() - startPoint.X;// 得到x轴的移动距离 
                        float dy = e.GetY() - startPoint.Y; // 得到x轴的移动距离 
                                                            // 在没有移动之前的位置上进行移动 
                        matrix.Set(currentMatrix);
                        matrix.PostTranslate(dx, dy);
                    }
                    // 放大缩小图片 
                    else if (mode == MODE_ZOOM)
                    {
                        float endDis = distance(e);// 结束距离 
                        if (endDis > 10f)// 两个手指并拢在一起的时候像素大于10 
                        {
                            float scale = endDis / startDis;// 得到缩放倍数 
                            matrix.Set(currentMatrix);
                            matrix.PostScale(scale, scale, midPoint.X, midPoint.Y);
                        }
                    }
                    break;
                // 手指离开屏幕 
                case MotionEventActions.Up:
                // 当触点离开屏幕，但是屏幕上还有触点(手指) 
                case MotionEventActions.PointerUp:
                    mode = 0;
                    break;
                // 当屏幕上已经有触点(手指)，再有一个触点压下屏幕 
                case MotionEventActions.PointerDown:
                    mode = MODE_ZOOM;
                    /** 计算两个手指间的距离 */
                    startDis = distance(e);
                    
                    if (startDis > 10f)/** 计算两个手指间的中间点 */
                    {
                        midPoint = mid(e);
                        //记录当前ImageView的缩放倍数 
                        currentMatrix.Set(m.ImageMatrix);
                    }
                    break;

            }
            m.ImageMatrix=matrix;
            return true; 
        }
        public void Dispose()
        {
            throw new NotImplementedException();
        }
        /** 计算两个手指间的距离 */
        private float distance(MotionEvent e)
        {
            float dx = e.GetX(1) - e.GetX(0);
            float dy = e.GetY(1) - e.GetY(0);
            /** 使用勾股定理返回两点之间的距离 */
            return FloatMath.Sqrt(dx * dx + dy * dy);
        }
        /** 计算两个手指间的中间点 */
        private PointF mid(MotionEvent e)
        {
            float midX = (e.GetX(1) + e.GetX(0)) / 2;
            float midY = (e.GetY(1) + e.GetY(0)) / 2;
            return new PointF(midX, midY);
        }
    }
}