﻿using System;
using System.Collections.Generic;
using System.Reactive;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Foundation.Sync.ConflictResolution;
using Toggl.PrimeRadiant;

namespace Toggl.Foundation.DataSources.Interfaces
{
    public interface IDataSource<TThreadsafe, TDatabase>
        where TDatabase : IDatabaseSyncable
        where TThreadsafe : IThreadsafeModel, TDatabase
    {
        IObservable<TThreadsafe> GetById(long id);

        IObservable<IEnumerable<TThreadsafe>> GetAll();

        IObservable<IEnumerable<TThreadsafe>> GetAll(Func<TDatabase, bool> predicate);

        IObservable<TThreadsafe> Create(TThreadsafe entity);

        IObservable<TThreadsafe> Update(TThreadsafe entity);
        
        IObservable<TThreadsafe> Update(long id, TThreadsafe entity);

        IObservable<Unit> Delete(long id);

        IObservable<IConflictResolutionResult<TThreadsafe>> UpdateWithConflictResolution(
            long id,
            TThreadsafe entity,
            IConflictResolver<TDatabase> conflictResolver = null,
            IRivalsResolver<TDatabase> rivalsResolver = null);

        IObservable<IEnumerable<IConflictResolutionResult<TThreadsafe>>> BatchUpdate(IEnumerable<TThreadsafe> entities);
    }
}
