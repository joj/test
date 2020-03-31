using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace PhxGaugingAndroid
{
    public enum DialogResult
    {
        Yes,
        No,
        Ok,
        Cancel
    }
    public class DialogService
    {
        public void ShowYesNoCancel(Context ct, string title, string message, Action<DialogResult> dialogCallback)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(ct);

            builder.SetTitle(title);
            builder.SetMessage(message);

            builder.SetPositiveButton("��", (o, e) =>
            {
                dialogCallback(DialogResult.Yes);
            });

            builder.SetNeutralButton("��", (o, e) =>
            {
                dialogCallback(DialogResult.No);
            });

            builder.SetNegativeButton("ȡ��", (o, e) =>
            {
                dialogCallback(DialogResult.Cancel);
            });

            builder.Show();
        }

        public void ShowYesNo(Context ct, string title, string message, Action<DialogResult> dialogCallback)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(ct);

            builder.SetTitle(title);
            builder.SetMessage(message);

            builder.SetPositiveButton("��", (o, e) =>
            {
                dialogCallback(DialogResult.Yes);
            });

            builder.SetNegativeButton("��", (o, e) =>
            {
                dialogCallback(DialogResult.No);
            });

            builder.Show();
        }

        public void ShowOk(Context ct, string title, string message, Action<DialogResult> dialogCallback)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(ct);

            builder.SetTitle(title);
            builder.SetMessage(message);

            builder.SetPositiveButton("ȷ��", (o, e) =>
            {
                if (dialogCallback != null)
                {
                    dialogCallback(DialogResult.Ok);
                }
            });

            builder.Show();
        }

        public void ShowOkCancel(Context ct, string title, string message, Action<DialogResult> dialogCallback)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(ct);
            builder.SetTitle(title);
            builder.SetMessage(message);

            builder.SetPositiveButton("ȷ��", (o, e) =>
            {
                dialogCallback(DialogResult.Ok);
            });

            builder.SetNegativeButton("ȡ��", (o, e) =>
            {
                dialogCallback(DialogResult.Cancel);
            });

            builder.Show();
        }
    }
}