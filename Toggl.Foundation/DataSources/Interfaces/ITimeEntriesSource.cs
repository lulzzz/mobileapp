using System;
using System.Collections.Generic;
using System.Reactive;
using Toggl.Foundation.DataSources.Interfaces;
using Toggl.Foundation.DTOs;
using Toggl.Foundation.Models.Interfaces;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.DataSources
{
    public interface ITimeEntriesSource
        : IObservableDataSource<IThreadsafeTimeEntry, IDatabaseTimeEntry>
    {
        IObservable<IThreadsafeTimeEntry> CurrentlyRunningTimeEntry { get; }

        IObservable<bool> IsEmpty { get; }

        IObservable<Unit> SoftDelete(IThreadsafeTimeEntry timeEntry);
        
        IObservable<IThreadsafeTimeEntry> Stop(DateTimeOffset stopTime);

        IObservable<IThreadsafeTimeEntry> Update(EditTimeEntryDto dto);
    }
}
