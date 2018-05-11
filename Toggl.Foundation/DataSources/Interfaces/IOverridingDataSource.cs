using System;
using Toggl.Foundation.Models.Interfaces;

namespace Toggl.Foundation.DataSources.Interfaces
{
    public interface IOverridingDataSource<TThreadsafe>
        where TThreadsafe : IThreadsafeModel
    {
        IObservable<TThreadsafe> Overwrite(TThreadsafe original, TThreadsafe entity);
    }
}
