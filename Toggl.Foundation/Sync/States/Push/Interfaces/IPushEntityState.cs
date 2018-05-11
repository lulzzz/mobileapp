using System;
using Toggl.Foundation.Models.Interfaces;

namespace Toggl.Foundation.Sync.States.Push.Interfaces
{
    public interface IPushEntityState<TThreadsafeModel>
        where TThreadsafeModel : IThreadsafeModel
    {
        StateResult<(Exception, TThreadsafeModel)> ServerError { get; }

        StateResult<(Exception, TThreadsafeModel)> ClientError { get; }

        StateResult<(Exception, TThreadsafeModel)> UnknownError { get; }
    }
}
