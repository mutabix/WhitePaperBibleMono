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
	[Register ("BibleSearchView")]
	partial class BibleSearchView
	{
		[Outlet]
		UIKit.UIView ResultsContainer { get; set; }

		[Outlet]
		UIKit.UISearchBar SearchBar { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint TopConstraint { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (SearchBar != null) {
				SearchBar.Dispose ();
				SearchBar = null;
			}

			if (TopConstraint != null) {
				TopConstraint.Dispose ();
				TopConstraint = null;
			}

			if (ResultsContainer != null) {
				ResultsContainer.Dispose ();
				ResultsContainer = null;
			}
		}
	}
}
