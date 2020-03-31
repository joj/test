package mono.com.github.chrisbanes.photoview;


public class OnPhotoTapListenerImplementor
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		com.github.chrisbanes.photoview.OnPhotoTapListener
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onPhotoTap:(Landroid/widget/ImageView;FF)V:GetOnPhotoTap_Landroid_widget_ImageView_FFHandler:Com.Github.Chrisbanes.Photoview.IOnPhotoTapListenerInvoker, Xamarin.Bindings.PhotoView\n" +
			"";
		mono.android.Runtime.register ("Com.Github.Chrisbanes.Photoview.IOnPhotoTapListenerImplementor, Xamarin.Bindings.PhotoView", OnPhotoTapListenerImplementor.class, __md_methods);
	}


	public OnPhotoTapListenerImplementor ()
	{
		super ();
		if (getClass () == OnPhotoTapListenerImplementor.class)
			mono.android.TypeManager.Activate ("Com.Github.Chrisbanes.Photoview.IOnPhotoTapListenerImplementor, Xamarin.Bindings.PhotoView", "", this, new java.lang.Object[] {  });
	}


	public void onPhotoTap (android.widget.ImageView p0, float p1, float p2)
	{
		n_onPhotoTap (p0, p1, p2);
	}

	private native void n_onPhotoTap (android.widget.ImageView p0, float p1, float p2);

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
