﻿using System;
using Toggl.Foundation.Models.Interfaces;
using Toggl.PrimeRadiant;

namespace Toggl.Foundation.DataSources.Interfaces
{
    public interface IObservableDataSource<TThreadsafe, TDatabase>
        : IDataSource<TThreadsafe, TDatabase>
        where TDatabase : IDatabaseSyncable
        where TThreadsafe : IThreadsafeModel, TDatabase
    {
        IObservable<TThreadsafe> Created { get; }
        
        IObservable<EntityUpdate<TThreadsafe>> Updated { get; }
        
        IObservable<long> Deleted { get; }
    }
}
