using System;
using System.Linq;
using System.Reactive.Linq;
using System.Collections.Generic;
using Toggl.Multivac;
using Toggl.PrimeRadiant;

namespace Toggl.Foundation.Interactors
{
    public class GetItemsThatFailedToSyncInteractor<T> : IInteractor<IObservable<IEnumerable<T>>> where T : IDatabaseSyncable
    {
        private readonly IRepository<T> repository;

        public GetItemsThatFailedToSyncInteractor(IRepository<T> repository)
        {
            Ensure.Argument.IsNotNull(repository, nameof(repository));

            this.repository = repository;
        }

        public IObservable<IEnumerable<T>> Execute()
            => repository.GetAll()
                .Select(failedToSync);

        private IEnumerable<T> failedToSync(IEnumerable<T> items)
        {
            return items.Where(p => p.SyncStatus == SyncStatus.SyncFailed);
        }
    }
}
