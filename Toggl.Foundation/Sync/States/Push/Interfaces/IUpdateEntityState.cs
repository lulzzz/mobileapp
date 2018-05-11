using System;
using Toggl.Foundation.Models.Interfaces;

namespace Toggl.Foundation.Sync.States.Push.Interfaces
{
    public interface IUpdateEntityState<TModel, TThreadsafeModel> : IPushEntityState<TThreadsafeModel>
        where TThreadsafeModel : TModel, IThreadsafeModel
    {
        StateResult<TModel> EntityChanged { get; }

        StateResult<TModel> UpdatingSucceeded { get; }

        IObservable<ITransition> Start(TThreadsafeModel entity);
    }
}
