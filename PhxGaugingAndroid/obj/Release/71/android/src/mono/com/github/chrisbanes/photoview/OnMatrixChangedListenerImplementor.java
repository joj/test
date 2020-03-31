package mono.com.github.chrisbanes.photoview;


public class OnMatrixChangedListenerImplementor
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		com.github.chrisbanes.photoview.OnMatrixChangedListener
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onMatrixChanged:(Landroid/graphics/RectF;)V:GetOnMatrixChanged_Landroid_graphics_RectF_Handler:Com.Github.Chrisbanes.Photoview.IOnMatrixChangedListenerInvoker, Xamarin.Bindings.PhotoView\n" +
			"";
		mono.android.Runtime.register ("Com.Github.Chrisbanes.Photoview.IOnMatrixChangedListenerImplementor, Xamarin.Bindings.PhotoView", OnMatrixChangedListenerImplementor.class, __md_methods);
	}


	public OnMatrixChangedListenerImplementor ()
	{
		super ();
		if (getClass () == OnMatrixChangedListenerImplementor.class)
			mono.android.TypeManager.Activate ("Com.Github.Chrisbanes.Photoview.IOnMatrixChangedListenerImplementor, Xamarin.Bindings.PhotoView", "", this, new java.lang.Object[] {  });
	}


	public void onMatrixChanged (android.graphics.RectF p0)
	{
		n_onMatrixChanged (p0);
	}

	private native void n_onMatrixChanged (android.graphics.RectF p0);

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
