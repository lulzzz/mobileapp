using System;
using System.Reactive;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Foundation.Sync.ConflictResolution;
using Toggl.PrimeRadiant;

namespace Toggl.Foundation.DataSources.Interfaces
{
    public interface ISingleObjectDataSource<TThreadsafe, TDatabase>
        : IOverridingDataSource<TThreadsafe>,
          IConflictResolutionUpdatingDataSource<TThreadsafe, TDatabase>
        where TDatabase : IDatabaseSyncable
        where TThreadsafe : IThreadsafeModel, TDatabase
    {
        IObservable<TThreadsafe> Current { get; }

        IObservable<TThreadsafe> Get();

        IObservable<Unit> Delete();

        IObservable<TThreadsafe> Create(TThreadsafe entity);
    }
}
