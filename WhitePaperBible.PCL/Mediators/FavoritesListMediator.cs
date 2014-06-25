using System;
using MonkeyArms;
using WhitePaperBible.Core.Models;
using WhitePaperBible.Core.Invokers;
using WhitePaperBible.Core.Views;
using System.Linq;
using WhitePaperBible.Core.Services;

namespace WhitePaperBible.Core.Mediators
{
	public class FavoritesListMediator : Mediator
	{
		[Inject]
		public AppModel AM;

		[Inject]
		public FavoritesReceivedInvoker FavoritesReceived;

		[Inject]
		public GetFavoritesInvoker GetFavorites;

		IFavoritesView Target;

		public FavoritesListMediator (IFavoritesView view) : base (view)
		{
			this.Target = view;
		}

		public override void Register ()
		{
//			InvokerMap.Add (Target.Filter, HandleFilter);
			InvokerMap.Add (Target.OnPaperSelected, HandlerPaperSelected);
			InvokerMap.Add (FavoritesReceived, (object sender, EventArgs e) => SetPapers ());
//			Target.SearchPlaceHolderText = "Search Papers";

//			if([appDelegate isUserLoggedIn]){
//				self.navigationItem.rightBarButtonItem = self.editButtonItem;
//				[HRRestModel setDelegate:self];
//				[HRRestModel getPath:@"/favorite/index/?caller=wpb-iPhone" withOptions:nil object:self];
//				[UIApplication sharedApplication].networkActivityIndicatorVisible = YES;
//			}
//			else{
//				NotLoggedInViewController *notLoggedInViewController = [[NotLoggedInViewController alloc] initWithNibName:@"NotLoggedInViewController"  bundle:[NSBundle mainBundle]];
//				[self presentModalViewController:notLoggedInViewController animated:YES];
//				[notLoggedInViewController release];
//			}

			if (AM.IsLoggedIn) {
				SetPapers ();
			}else{
				Target.PromptForLogin ();
			}

		}

		void HandlerPaperSelected (object sender, EventArgs e)
		{
			AM.CurrentPaper = Target.SelectedPaper;
		}

		public void SetPapers ()
		{
			if (AM.Favorites != null && AM.Favorites != null) {
				Target.SetPapers (AM.Favorites);
			}else{
				GetFavorites.Invoke ();
			}
		}
	}
}