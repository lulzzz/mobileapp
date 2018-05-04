using System;
using Foundation;
using MvvmCross.Binding.iOS.Views;
using Toggl.Daneel.Views.CountrySelection;
using UIKit;

namespace Toggl.Daneel.ViewSources
{
    public sealed class CountryTableViewSource : MvxTableViewSource
    {
        private const string cellIdentifier = nameof(CountryViewCell);

        public string Text { get; set; }

        public CountryTableViewSource(UITableView tableView)
            : base(tableView)
        {
            tableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            tableView.RegisterNibForCellReuse(CountryViewCell.Nib, cellIdentifier);
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var item = GetItemAt(indexPath);
            var cell = GetOrCreateCellFor(tableView, indexPath, item);
            cell.SelectionStyle = UITableViewCellSelectionStyle.None;

            if (item != null && cell is IMvxBindable bindable)
                bindable.DataContext = item;

            return cell;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
            => base.RowsInSection(tableview, section);

        protected override UITableViewCell GetOrCreateCellFor(UITableView tableView, NSIndexPath indexPath, object item)
        {
            return tableView.DequeueReusableCell(cellIdentifier, indexPath);
        }

        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath) => 48;
    }
}
