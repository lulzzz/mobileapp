using System;
using System.Linq;
using System.Reactive.Linq;
using System.Collections.Generic;
using Toggl.Multivac;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Interactors
{
    public class GetTagsThatFailedToSyncInteractor : IInteractor<IObservable<IEnumerable<IDatabaseTag>>>
    {
        private readonly ITogglDatabase database;

        public GetTagsThatFailedToSyncInteractor(ITogglDatabase database)
        {
            Ensure.Argument.IsNotNull(database, nameof(database));

            this.database = database;
        }

        public IObservable<IEnumerable<IDatabaseTag>> Execute()
            => database.Tags
                .GetAll()
                .Select(failedToSync);

        private IEnumerable<IDatabaseTag> failedToSync(IEnumerable<IDatabaseTag> tags)
        {
            return tags.Where(t => t.SyncStatus == SyncStatus.SyncFailed);
        }
    }
}