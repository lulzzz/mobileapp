using System;
using System.Linq;
using System.Reactive.Linq;
using System.Collections.Generic;
using Toggl.Multivac;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Interactors.Client
{
    public class GetClientsThatFailedToSync : IInteractor<IObservable<IEnumerable<IDatabaseClient>>>
    {
        private readonly ITogglDatabase database;

        public GetClientsThatFailedToSync(ITogglDatabase database)
        {
            Ensure.Argument.IsNotNull(database, nameof(database));

            this.database = database;
        }

        public IObservable<IEnumerable<IDatabaseClient>> Execute()
            => database.Clients
                .GetAll()
                .Select(failedToSync);

        private IEnumerable<IDatabaseClient> failedToSync(IEnumerable<IDatabaseClient> clients)
        {
            return clients.Where(c => c.SyncStatus == SyncStatus.SyncFailed);
        }
    }
}