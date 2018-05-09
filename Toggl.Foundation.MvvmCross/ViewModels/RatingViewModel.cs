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

        public bool GotAnswer { get; private set; }

        private readonly ITogglDataSource dataSource;
        private IDisposable emptyDatabaseDisposable;

        public MvxAsyncCommand<bool> ProcessAnswerCommand { get; set; }

        public RatingViewModel(ITogglDataSource dataSource)
        {
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));
            this.dataSource = dataSource;

            this.GotAnswer = false;

            ProcessAnswerCommand = new MvxAsyncCommand<bool>(processAnswer);
        }

        public async override Task Initialize()
        {
            await base.Initialize();

            //emptyDatabaseDisposable = dataSource
            //.TimeEntries
            //.IsEmpty
            //.FirstAsync()
            //.Subscribe();
        }

        private async Task processAnswer(bool answer)
        {
            GotAnswer = true;
        }
    }
}
