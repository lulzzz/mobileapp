using System;
using System.Collections.Generic;
using System.Reactive;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Foundation.Sync.ConflictResolution;
using Toggl.PrimeRadiant;

namespace Toggl.Foundation.DataSources.Interfaces
{
    public interface IDataSource<TThreadsafe, TDatabase>
        : IOverridingDataSource<TThreadsafe>,
          IConflictResolutionUpdatingDataSource<TThreadsafe, TDatabase>
        where TDatabase : IDatabaseSyncable
        where TThreadsafe : IThreadsafeModel, TDatabase
    {
        IObservable<TThreadsafe> GetById(long id);

        IObservable<IEnumerable<TThreadsafe>> GetAll();

        IObservable<IEnumerable<TThreadsafe>> GetAll(Func<TDatabase, bool> predicate);

        IObservable<TThreadsafe> Create(TThreadsafe entity);

        IObservable<TThreadsafe> Update(TThreadsafe entity);

        IObservable<Unit> Delete(long id);

        IObservable<IEnumerable<IConflictResolutionResult<TThreadsafe>>> BatchUpdate(IEnumerable<TThreadsafe> entities);
    }
}
