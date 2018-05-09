using System;
using Toggl.Foundation.Models.Interfaces;
using Toggl.PrimeRadiant;

namespace Toggl.Foundation.DataSources.Interfaces
{
    public interface ISingleObjectDataSource<TThreadsafe, TDatabase>
        where TDatabase : IDatabaseSyncable
        where TThreadsafe : IThreadsafeModel, TDatabase
    {
        IObservable<TThreadsafe> Current { get; }

        IObservable<TThreadsafe> Get();
    }
}
