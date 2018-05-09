using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using Toggl.Foundation.DataSources;
using Toggl.Multivac;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class RatingViewModel : MvxViewModel
    {
        public bool GotImpression { get; private set; }

        private readonly ITogglDataSource dataSource;
        private IDisposable emptyDatabaseDisposable;

        public MvxCommand<bool> RegisterImpressionCommand { get; set; }
        public MvxCommand LeaveReviewCommand { get; set; }
        public MvxCommand DismissViewCommand { get; set; }

        public RatingViewModel(ITogglDataSource dataSource)
        {
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));
            this.dataSource = dataSource;
            GotImpression = false;

            RegisterImpressionCommand = new MvxCommand<bool>(registerImpression);
            LeaveReviewCommand = new MvxCommand(leaveReview);
            DismissViewCommand = new MvxCommand(dismiss);
        }

        public async override Task Initialize()
        {
            await base.Initialize();
        }

        private void registerImpression(bool isPositive)
        {
            GotImpression = true;
        }

        private void leaveReview()
        {
            GotImpression = false;
        }

        private void dismiss()
        {
            GotImpression = true;
        }
    }
}
