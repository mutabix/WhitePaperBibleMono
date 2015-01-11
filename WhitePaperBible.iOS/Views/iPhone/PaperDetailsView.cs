using System;
using CoreGraphics;
using Foundation;
using UIKit;
using WhitePaperBible.Core.Models;
using WhitePaperBible.Core.Services;
using System.Collections.Generic;
using ObjCRuntime;
using MessageUI;
using Twitter;
using System.Web;
using MonkeyArms;
using RestSharp;
using WhitePaperBible.Core.Views;
using Social;
using IOS.Util;
using BigTed;

namespace WhitePaperBible.iOS
{
	public partial class PaperDetailsView : UIViewController, IMediatorTarget, IPaperDetailView
	{
		public Paper Paper {
			get;
			set;
		}

		bool IsFavorite;
				
		public PaperDetailsView (Paper paper) : base ("PaperDetailsView", null)
		{
			this.Paper = paper;
			this.Title = Paper.title;

			this.HidesBottomBarWhenPushed = true;
		}

		public Invoker ToggleFavorite {
			get;
			private set;
		}

		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			UIKit.UIApplication.SharedApplication.NetworkActivityIndicatorVisible = true;

			ToggleFavorite = new Invoker ();

			this.Title = Paper.title;
			webView.ScrollView.ScrollEnabled = true;
			webView.ShouldStartLoad += webViewShouldStartLoad;
			
			var tapRecognizer = new UITapGestureRecognizer ((g)=> {
				Console.WriteLine ("getting taps");

				// so the question is how long were we holding it down? Has the user initialized text selection?
				var selection = webView.EvaluateJavascript ("window.getSelection().toString()");
				Console.WriteLine ("selection {0}", selection);

				if (selection.Length <= 0) {			
					// TODO animate these
					if (toolbar.Alpha.Equals (1f)) {
						toolbar.Alpha = 0f;
					} else {
						toolbar.Alpha = 1f;
					}
				} else {
					Console.WriteLine ("user is selecting text");
				}

			});
			tapRecognizer.NumberOfTapsRequired = 1;
			tapRecognizer.Delegate = new GestureDelegate ();
			
			webView.AddGestureRecognizer (tapRecognizer);

			AnalyticsUtil.TrackScreen (this.Title);
		}

		public void DismissController()
		{
			InvokeOnMainThread (() => {
				this.NavigationController.PopViewController (true);
			});

		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

			DI.RequestMediator (this);
			
			toolbar.Alpha = 1f;
			// TODO fix timer
//			var timer = NSTimer.CreateScheduledTimer (2, ()=> {
//				toolbar.Alpha = 0f;
//				Console.WriteLine ("hiding toolbar");
//			});
		}

		public override void ViewWillDisappear(bool animated)
		{
			DI.DestroyMediator (this);
		}

		bool webViewShouldStartLoad (UIWebView webView, NSUrlRequest request, UIWebViewNavigationType navigationType)
		{
			Console.WriteLine (request.Url.AbsoluteString);
			if (request.Url.AbsoluteString.IndexOf ("#back") > -1) {
				NavigationController.PopViewController (true);
				return false;
			}
		
			return true;
		}

		private void onFailure (String msg)
		{
			// Ooops
		}

		public void SetPaper (Paper paper, bool isFavorite, bool isOwned)
		{
			this.Paper = paper;
			this.IsFavorite = isFavorite;
			InvokeOnMainThread (delegate {
				SetFavoriteImage (this.favoriteButton);
				SetReferences (Paper.HtmlContent);
				if(isOwned){
					NavigationItem.SetRightBarButtonItem (
						new UIBarButtonItem ("Edit", UIBarButtonItemStyle.Plain, (sender, args)=> {
							var editPaperView = new EditPaperView(this.Paper);
							var editNav = new UINavigationController (editPaperView);
							this.PresentViewController (editNav, true, null);
						})
						, true
					);

				}
			});
		}

		public void SetReferences (string html)
		{
			UIKit.UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false;
			webView.LoadHtmlString (html, NSBundle.MainBundle.BundleUrl);//NSBundle.MainBundle.BundleUrl
		}

		public override void ViewDidUnload ()
		{
			base.ViewDidUnload ();
			
			// Clear any references to subviews of the main view in order to
			// allow the Garbage Collector to collect them sooner.
			//
			// e.g. myOutlet.Dispose (); myOutlet = null;
			
			ReleaseDesignerOutlets ();
		}

		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			// Return true for supported orientations
			return (toInterfaceOrientation != UIInterfaceOrientation.PortraitUpsideDown);
		}

		partial void sharePressed (UIKit.UIBarButtonItem sender1)
		{
			var actionSheet = new UIActionSheet ("Sharing", null, "Cancel", null, null) {
				Style = UIActionSheetStyle.BlackTranslucent
			};
			actionSheet.AddButton ("Email");
			actionSheet.AddButton ("Twitter");
			actionSheet.AddButton ("Facebook");
			actionSheet.AddButton ("Other");
			actionSheet.Clicked += HandleShareOptionClicked;
				
			actionSheet.ShowInView (View);
		}

		void HandleShareOptionClicked (object sender, UIButtonEventArgs e)
		{
			UIActionSheet myActionSheet = sender as UIActionSheet;
			Console.WriteLine ("Clicked on item {0}", e.ButtonIndex);
			if (e.ButtonIndex != myActionSheet.CancelButtonIndex) {
				
				string paperTitle = Paper.title;
				string urlTitle = Paper.url_title;
				string subject = "White Paper Bible: " + paperTitle;
				string paperFullURL = "http://whitepaperbible.org/" + urlTitle;

				string messageCombined = subject + Environment.NewLine + paperFullURL + Environment.NewLine + Paper.ToPlainText();

				if (e.ButtonIndex == 1) {
					//email
					if (MFMailComposeViewController.CanSendMail) {
						MFMailComposeViewController _mail = new MFMailComposeViewController ();
						_mail.SetSubject (subject);
						_mail.SetMessageBody (messageCombined, false);
						_mail.Finished += HandleMailFinished;
						_mail.ModalPresentationStyle = UIModalPresentationStyle.FormSheet;
						PresentViewController (_mail, true, null);
					}			
				} else if (e.ButtonIndex == 2) {
					Share ("twitter", paperTitle + " | " + paperFullURL);
				} else if (e.ButtonIndex == 3) {
					Share ("facebook", paperTitle + " | " + paperFullURL);
				}else{
					var activityItems = new NSObject[] { new NSString(Paper.ToPlainText()) };
					UIActivityViewController vc = new UIActivityViewController (activityItems, null);
					PresentViewController (vc, true, null);
				}
			}
		
		}

		void Share (string type, string msg)
		{
			SLComposeViewController slComposer;
			if (type == "twitter") {
				slComposer = SLComposeViewController.FromService (SLServiceType.Twitter);
			} else {
				slComposer = SLComposeViewController.FromService (SLServiceType.Facebook);
			}

			slComposer.SetInitialText (msg);
			slComposer.CompletionHandler += (result) => DismissViewController (true, null);
//			slComposer.AddImage (UIImage.FromFile("Images/AppIcon/114.png"));

			PresentViewController (slComposer, true, null);
		}

		void HandleMailFinished (object sender, MFComposeResultEventArgs e)
		{

			if (e.Result == MFMailComposeResult.Sent) {
				BTProgressHUD.ShowToast("Mail Sent", true, 3000);
			}
			e.Controller.DismissModalViewController (true);
		}

		void SetFavoriteImage (UIBarButtonItem sender)
		{
			if (IsFavorite) {
				sender.Image = UIImage.FromBundle ("favorited");
			}
			else {
				sender.Image = UIImage.FromBundle ("favorites");
			}
		}

		partial void favoritePressed (UIKit.UIBarButtonItem sender)
		{
			ToggleFavorite.Invoke();
			IsFavorite = !IsFavorite;

			SetFavoriteImage (sender);
		}

		protected UIWebView webViewTest;

		protected void AddWebView ()
		{
			var style = "<style type='text/css'>body { color: #000000; background-color: transparent; font-family: 'HelveticaNeue-Light', Helvetica, Arial, sans-serif; padding: 20px; } h1, h2, h3, h4, h5, h6 { padding: 0px; margin: 0px; font-style: normal; font-weight: normal; } h2 { font-family: 'HelveticaNeue-Light', Helvetica, Arial, sans-serif; font-size: 24px; font-weight: normal; } h4 { font-size: 16px; } p { font-family: Helvetica, Verdana, Arial, sans-serif; line-height:1.5; font-size: 16px; } .esv-text { padding: 0 0 10px 0; }</style>";
			var html = style + "<body><h2>Contact</h2><a href='http://mydiem.com/index.cfm?event=main.faq'>Help</a><br/><a href='mailto:support@mydiem.com'>Email Support</a><br/><br/>";
			html += "<h2>Our Story</h2><p>If you've ever sent your child empty-handed to a class party, attended parent/teacher conferences on the wrong day or arrived an hour early for a game, you're not alone. It's not bad parenting, it's bad scheduling and trust us, we've been there too.</p><p>Year after year we would receive paper calendars from our children's schools, get several emails a week, or would be encouraged to visit websites to view upcoming events (sports teams, Boy Scouts/ Girl Scouts, ballet, band and track club all had different sites!). Sometimes we would enter the information into a paper agenda, which couldn't be shared. Or we would type everything into our computer's calendar hoping the information wouldn't change. And, as the school year progressed, updates would take the form of crumpled notes, skimmed over emails and hurried messages from coaches and teachers that didn't always make it to the master calendar. We'd often ask ourselves, \"Why can't school events appear on our smart phones or computer calendar programs?\" or \"If there is a change, couldn't someone update the calendar for us so we don't have to keep track of emails, notes, etc? Then, one day, we stopped wishing and got to work.</p><p>We are parents to three school-age (and very busy) children and now we\'re also the proud parents of MyDiem.com. We're so happy to share this much-needed tool with other busy parents. Hopefully you will find this tool helpful in keeping track of your child's activities.</p>";
			html += "</body>";

			webViewTest = new UIWebView (new CGRect (0, 0, 320, WhitePaperBible.iOS.UI.Environment.DeviceScreenHeight));
			webViewTest.SizeToFit ();
			View.AddSubview (webViewTest);

			webViewTest.Opaque = false;
			webViewTest.BackgroundColor = UIColor.Clear;
			webViewTest.LoadHtmlString (html, NSUrl.FromString ("http://localhost"));
		}
	}

	public class GestureDelegate : UIGestureRecognizerDelegate
	{
		public override bool ShouldReceiveTouch (UIGestureRecognizer recognizer, UITouch touch)
		{
			return true;
		}

		public override bool ShouldBegin (UIGestureRecognizer recognizer)
		{
			return true;
		}

		public override bool ShouldRecognizeSimultaneously (UIGestureRecognizer gestureRecognizer, UIGestureRecognizer otherGestureRecognizer)
		{
			return true;
		}
	}
}

