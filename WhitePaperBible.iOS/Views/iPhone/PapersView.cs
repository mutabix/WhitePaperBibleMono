using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using WhitePaperBible.Core.Models;
using WhitePaperBible.Core.Services;
using WhitePaperBible.iOS.TableSource;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.Dialog;
using WhitePaperBible.Core.Views;
using MonkeyArms;

namespace WhitePaperBible.iOS
{
	public partial class PapersView : DialogViewController, IPapersListView
	{
		List<Paper> Papers;

		public PapersView () : base (UITableViewStyle.Plain, null, true)
		{
			EnableSearch = true; 
			AutoHideSearch = true;
			SearchPlaceholder = @"Find Papers";
		}

		#region IPapersListView implementation

		public event EventHandler Filter;

		public event EventHandler OnPaperSelected;

		public void SetPapers (List<Paper> papers)
		{
				this.Papers = papers;

				InvokeOnMainThread (delegate {

					Root = new RootElement("Papers") {
						from node in papers
						group node by (node.title [0].ToString ().ToUpper ()) into alpha
						orderby alpha.Key
						select new Section (alpha.Key){
							from eachNode in alpha
							select (Element)new WhitePaperBible.iOS.UI.CustomElements.PaperElement (eachNode)
						}};

					TableView.ScrollToRow (NSIndexPath.FromRowSection (0, 0), UITableViewScrollPosition.Top, false);
				});

		}

		public string SearchPlaceHolderText {
			get;
			set;
		}

		public string SearchQuery {
			get;
			set;
		}

		public Paper SelectedPaper {
			get;
			set;
		}

		#endregion
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			DI.RequestMediator (this);

			SearchTextChanged += (sender, args) => {
				Console.WriteLine("search text changed");	
			};
			
		}

	}
}
