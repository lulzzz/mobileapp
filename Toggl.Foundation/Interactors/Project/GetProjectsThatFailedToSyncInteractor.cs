using System;
using System.Linq;
using System.Reactive.Linq;
using System.Collections.Generic;
using Toggl.Multivac;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Interactors.Project
{
    public class GetProjectsThatFailedToSyncInteractor : IInteractor<IObservable<IEnumerable<IDatabaseProject>>>
    {
        private readonly ITogglDatabase database;

        public GetProjectsThatFailedToSyncInteractor(ITogglDatabase database)
        {
            Ensure.Argument.IsNotNull(database, nameof(database));

            this.database = database;
        }

        public IObservable<IEnumerable<IDatabaseProject>> Execute()
            => database.Projects
                .GetAll()
                .Select(failedToSync);

        private IEnumerable<IDatabaseProject> failedToSync(IEnumerable<IDatabaseProject> projects)
        {
            return projects.Where(p => p.SyncStatus == SyncStatus.SyncFailed);
        }
    }
}
