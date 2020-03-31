package md5ec005d2187bda49e3d487d9f4fef6e21;


public class ClearnEditText
	extends android.widget.EditText
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onFocusChanged:(ZILandroid/graphics/Rect;)V:GetOnFocusChanged_ZILandroid_graphics_Rect_Handler\n" +
			"n_onTouchEvent:(Landroid/view/MotionEvent;)Z:GetOnTouchEvent_Landroid_view_MotionEvent_Handler\n" +
			"";
		mono.android.Runtime.register ("PhxGaugingAndroid.Fragments.ClearnEditText, PhxGaugingAndroid", ClearnEditText.class, __md_methods);
	}


	public ClearnEditText (android.content.Context p0)
	{
		super (p0);
		if (getClass () == ClearnEditText.class)
			mono.android.TypeManager.Activate ("PhxGaugingAndroid.Fragments.ClearnEditText, PhxGaugingAndroid", "Android.Content.Context, Mono.Android", this, new java.lang.Object[] { p0 });
	}


	public ClearnEditText (android.content.Context p0, android.util.AttributeSet p1)
	{
		super (p0, p1);
		if (getClass () == ClearnEditText.class)
			mono.android.TypeManager.Activate ("PhxGaugingAndroid.Fragments.ClearnEditText, PhxGaugingAndroid", "Android.Content.Context, Mono.Android:Android.Util.IAttributeSet, Mono.Android", this, new java.lang.Object[] { p0, p1 });
	}


	public ClearnEditText (android.content.Context p0, android.util.AttributeSet p1, int p2)
	{
		super (p0, p1, p2);
		if (getClass () == ClearnEditText.class)
			mono.android.TypeManager.Activate ("PhxGaugingAndroid.Fragments.ClearnEditText, PhxGaugingAndroid", "Android.Content.Context, Mono.Android:Android.Util.IAttributeSet, Mono.Android:System.Int32, mscorlib", this, new java.lang.Object[] { p0, p1, p2 });
	}


	public ClearnEditText (android.content.Context p0, android.util.AttributeSet p1, int p2, int p3)
	{
		super (p0, p1, p2, p3);
		if (getClass () == ClearnEditText.class)
			mono.android.TypeManager.Activate ("PhxGaugingAndroid.Fragments.ClearnEditText, PhxGaugingAndroid", "Android.Content.Context, Mono.Android:Android.Util.IAttributeSet, Mono.Android:System.Int32, mscorlib:System.Int32, mscorlib", this, new java.lang.Object[] { p0, p1, p2, p3 });
	}


	public void onFocusChanged (boolean p0, int p1, android.graphics.Rect p2)
	{
		n_onFocusChanged (p0, p1, p2);
	}

	private native void n_onFocusChanged (boolean p0, int p1, android.graphics.Rect p2);


	public boolean onTouchEvent (android.view.MotionEvent p0)
	{
		return n_onTouchEvent (p0);
	}

	private native boolean n_onTouchEvent (android.view.MotionEvent p0);

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
