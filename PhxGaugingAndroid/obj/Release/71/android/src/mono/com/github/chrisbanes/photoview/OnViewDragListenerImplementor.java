package mono.com.github.chrisbanes.photoview;


public class OnViewDragListenerImplementor
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		com.github.chrisbanes.photoview.OnViewDragListener
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onDrag:(FF)V:GetOnDrag_FFHandler:Com.Github.Chrisbanes.Photoview.IOnViewDragListenerInvoker, Xamarin.Bindings.PhotoView\n" +
			"";
		mono.android.Runtime.register ("Com.Github.Chrisbanes.Photoview.IOnViewDragListenerImplementor, Xamarin.Bindings.PhotoView", OnViewDragListenerImplementor.class, __md_methods);
	}


	public OnViewDragListenerImplementor ()
	{
		super ();
		if (getClass () == OnViewDragListenerImplementor.class)
			mono.android.TypeManager.Activate ("Com.Github.Chrisbanes.Photoview.IOnViewDragListenerImplementor, Xamarin.Bindings.PhotoView", "", this, new java.lang.Object[] {  });
	}


	public void onDrag (float p0, float p1)
	{
		n_onDrag (p0, p1);
	}

	private native void n_onDrag (float p0, float p1);

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
