using System;
using System.Collections.Generic;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Interactors
{
    public sealed partial class InteractorFactory : IInteractorFactory
    {
        public IInteractor<IObservable<bool>> IsBillableAvailableForProject(long projectId)
            => new IsBillableAvailableForProjectInteractor(database, projectId);

        public IInteractor<IObservable<bool>> ProjectDefaultsToBillable(long projectId)
            => new ProjectDefaultsToBillableInteractor(database, projectId);

        public IInteractor<IObservable<IEnumerable<IDatabaseProject>>> GetProjectsThatFailedToSync()
            => new GetItemsThatFailedToSyncInteractor<IDatabaseProject>(database.Projects);
    }
}
