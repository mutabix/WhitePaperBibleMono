// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace WhitePaperBible.iOS
{
	[Register ("PaperDetailsView")]
	partial class PaperDetailsView
	{
		[Outlet]
		UIKit.UIBarButtonItem favoriteButton { get; set; }

		[Outlet]
		UIKit.UIToolbar toolbar { get; set; }

		[Outlet]
		UIKit.UIWebView webView { get; set; }

		[Action ("favoritePressed:")]
		partial void favoritePressed (UIKit.UIBarButtonItem sender);

		[Action ("sharePressed:")]
		partial void sharePressed (UIKit.UIBarButtonItem sender1);
		
		void ReleaseDesignerOutlets ()
		{
			if (webView != null) {
				webView.Dispose ();
				webView = null;
			}

			if (toolbar != null) {
				toolbar.Dispose ();
				toolbar = null;
			}

			if (favoriteButton != null) {
				favoriteButton.Dispose ();
				favoriteButton = null;
			}
		}
	}
}
