using System;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Foundation.Sync.ConflictResolution;
using Toggl.PrimeRadiant;

namespace Toggl.Foundation.DataSources.Interfaces
{
    public interface IConflictResolutionUpdatingDataSource<TThreadsafe, TDatabase>
        where TDatabase : IDatabaseSyncable
        where TThreadsafe : TDatabase, IThreadsafeModel
    {
        IObservable<IConflictResolutionResult<TThreadsafe>> UpdateWithConflictResolution(
            TThreadsafe original,
            TThreadsafe entity,
            IConflictResolver<TDatabase> conflictResolver = null,
            IRivalsResolver<TDatabase> rivalsResolver = null);
    }
}
