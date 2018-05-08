using System;
using System.Linq;
using System.Reactive.Linq;
using System.Collections.Generic;
using Toggl.Multivac;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Interactors
{
    public class GetNumberOfItemsThatFailedToSyncInteractor : IInteractor<IObservable<int>>
    {
        private readonly ITogglDatabase database;

        public GetNumberOfItemsThatFailedToSyncInteractor(ITogglDatabase database)
        {
            Ensure.Argument.IsNotNull(database, nameof(database));

            this.database = database;
        }

        public IObservable<int> Execute()
        {
            var projects = database.Projects.GetAll();
            var clients = database.Clients.GetAll();
            var tags = database.Tags.GetAll();

            return Observable.Empty<int>();
                                   
        }

        private IEnumerable<IDatabaseSyncable> failedToSync(IEnumerable<IDatabaseSyncable> items)
        {
            return items.Where(p => p.SyncStatus == SyncStatus.SyncFailed);
        }
    }
}
