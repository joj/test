using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace PhxGaugingAndroid.Common
{
    public static class BitmapHelpers
    {
        public static Bitmap LoadAndResizeBitmap(this string fileName, int width, int height)
        {
            // First we get the the dimensions of the file on disk
            BitmapFactory.Options options = new BitmapFactory.Options { InJustDecodeBounds = true };
            BitmapFactory.DecodeFile(fileName, options);

            // Next we calculate the ratio that we need to resize the image by
            // in order to fit the requested dimensions.
            int outHeight = options.OutHeight;
            int outWidth = options.OutWidth;
            int inSampleSize = 1;

            if (outHeight > height || outWidth > width)
            {
                inSampleSize = outWidth > outHeight
                                   ? outHeight / height
                                   : outWidth / width;
            }

            // Now we will load the image and have BitmapFactory resize it for us.
            options.InSampleSize = inSampleSize;
            options.InJustDecodeBounds = false;
            FileStream fis = new FileStream(fileName, FileMode.Open);
            Bitmap resizedBitmap = BitmapFactory.DecodeStream(fis, null, options);
            fis.Close();
            return resizedBitmap;
        }

        public static void SetImgBitMap(string ImgPath, ImageView img, Bitmap btp)
        {
            BitmapFactory.Options options = new BitmapFactory.Options();
            options.InJustDecodeBounds = false;
            options.InPreferredConfig = Bitmap.Config.Rgb565;
            options.InPurgeable = true;
            options.InInputShareable = true;
            options.InSampleSize = 1;
            FileStream fis = new FileStream(ImgPath, FileMode.Open);
            btp = BitmapFactory.DecodeStream(fis, null, options);
            img.SetImageBitmap(btp);
            fis.Close();
        }
    }
}