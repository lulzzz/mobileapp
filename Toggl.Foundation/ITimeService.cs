using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace Toggl.Foundation
{
    public interface ITimeService
    {
        DateTimeOffset CurrentDateTime { get; }

        IConnectableObservable<DateTimeOffset> CurrentDateTimeObservable { get; } 

        IConnectableObservable<DateTimeOffset> MidnightObservable { get; }

        Task RunAfterDelay(TimeSpan delay, Action action);
    }
}
