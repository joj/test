package md5374eaaa2d0fff3704e2f1604f3b4c296;


public class CalibrationLogActivity
	extends android.app.Activity
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onCreate:(Landroid/os/Bundle;)V:GetOnCreate_Landroid_os_Bundle_Handler\n" +
			"";
		mono.android.Runtime.register ("PhxGaugingAndroid.CalibrationLogActivity, PhxGaugingAndroid", CalibrationLogActivity.class, __md_methods);
	}


	public CalibrationLogActivity ()
	{
		super ();
		if (getClass () == CalibrationLogActivity.class)
			mono.android.TypeManager.Activate ("PhxGaugingAndroid.CalibrationLogActivity, PhxGaugingAndroid", "", this, new java.lang.Object[] {  });
	}


	public void onCreate (android.os.Bundle p0)
	{
		n_onCreate (p0);
	}

	private native void n_onCreate (android.os.Bundle p0);

	private java.util.ArrayList refList;
	public void monodroidAddReference (java.lang.Object obj)
	{
		if (refList == null)
			refList = new java.util.ArrayList ();
		refList.add (obj);
	}

	public void monodroidClearReferences ()
	{
		if (refList != null)
			refList.clear ();
	}
}
