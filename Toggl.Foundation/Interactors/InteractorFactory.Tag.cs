using System;
using System.Collections.Generic;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Interactors
{
    public sealed partial class InteractorFactory : IInteractorFactory
    {
        public IInteractor<IObservable<IEnumerable<IDatabaseTag>>> GetTagsThatFailedToSync()
            => new GetTagsThatFailedToSyncInteractor(database);
    }
}
