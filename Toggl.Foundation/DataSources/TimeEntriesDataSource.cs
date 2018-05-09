using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Linq;
using System.Reactive.Subjects;
using Toggl.Foundation.DataSources.Interfaces;
using Toggl.Foundation.Models;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;
using Toggl.Foundation.DTOs;
using Toggl.Foundation.Exceptions;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Foundation.Sync.ConflictResolution;

namespace Toggl.Foundation.DataSources
{
    internal sealed class TimeEntriesDataSource
        : ObservableDataSource<IThreadsafeTimeEntry, IDatabaseTimeEntry>,
            ITimeEntriesSource
    {
        private long? currentlyRunningTimeEntryId;

        private readonly ITimeService timeService;
        private readonly Func<IDatabaseTimeEntry, IDatabaseTimeEntry, ConflictResolutionMode> alwaysCreate
            = (a, b) => ConflictResolutionMode.Create;

        public IObservable<bool> IsEmpty { get; }

        public IObservable<IThreadsafeTimeEntry> CurrentlyRunningTimeEntry { get; }

        protected override IRivalsResolver<IDatabaseTimeEntry> RivalsResolver { get; }

        public TimeEntriesDataSource(
            IRepository<IDatabaseTimeEntry> repository,
            ITimeService timeService)
            : base(repository)
        {
            Ensure.Argument.IsNotNull(timeService, nameof(timeService));

            this.timeService = timeService;

            CurrentlyRunningTimeEntry =
                GetAllNonDeleted(te => te.Duration == null)
                    .Select(tes => tes.SingleOrDefault())
                    .StartWith()
                    .Merge(Created.Where(te => te.IsRunning()))
                    .Merge(Updated.Where(tuple => tuple.Entity.Id == currentlyRunningTimeEntryId).Select(tuple => tuple.Entity))
                    .Merge(Deleted.Where(id => id == currentlyRunningTimeEntryId).Select(_ => null as IThreadsafeTimeEntry))
                    .Select(runningTimeEntry)
                    .Do(setRunningTimeEntryId)
                    .ConnectedReplay();

            IsEmpty =
                Observable.Return(default(IDatabaseTimeEntry))
                    .StartWith()
                    .Merge(Updated.Select(tuple => tuple.Entity))
                    .Merge(Created)
                    .SelectMany(_ => GetAllNonDeleted())
                    .Select(timeEntries => !timeEntries.Any());
            
            RivalsResolver = new TimeEntryRivalsResolver(timeService);
        }

        public override IObservable<IThreadsafeTimeEntry> Create(IThreadsafeTimeEntry entity)
            => Repository.UpdateWithConflictResolution(entity.Id, entity, alwaysCreate)
                .OfType<CreateResult<IDatabaseTimeEntry>>()
                .Select(result => result.Entity)
                .Select(Convert)
                .Do(CreatedSubject.OnNext);

        public IObservable<IEnumerable<IThreadsafeTimeEntry>> GetAllNonDeleted()
            => GetAll(te => !te.IsDeleted);

        public IObservable<IEnumerable<IThreadsafeTimeEntry>> GetAllNonDeleted(Func<IDatabaseTimeEntry, bool> predicate)
            => GetAll(predicate).Select(tes => tes.Where(te => !te.IsDeleted));

        public IObservable<IThreadsafeTimeEntry> Stop(DateTimeOffset stopTime)
            => GetAllNonDeleted(te => te.Duration == null)
                .Select(timeEntries => timeEntries.SingleOrDefault() ?? throw new NoRunningTimeEntryException())
                .SelectMany(timeEntry => timeEntry
                    .With((long)(stopTime - timeEntry.Start).TotalSeconds)
                    .Apply(Update));

        public override IObservable<IThreadsafeTimeEntry> Update(long id, IThreadsafeTimeEntry entity)
            => base.Update(id, entity)
                .Do(updatedEntity => maybeUpdateCurrentlyRunningTimeEntryId(id, updatedEntity));

        public IObservable<Unit> SoftDelete(IThreadsafeTimeEntry timeEntry)
            => Observable.Return(timeEntry)
                .Select(TimeEntry.DirtyDeleted)
                .SelectMany(Repository.Update)
                .Do(entity => DeletedSubject.OnNext(entity.Id))
                .Select(_ => Unit.Default);

        protected override IThreadsafeTimeEntry Convert(IDatabaseTimeEntry entity)
            => TimeEntry.From(entity);

        protected override ConflictResolutionMode ResolveConflicts(IDatabaseTimeEntry first, IDatabaseTimeEntry second)
            => Resolver.ForTimeEntries.Resolve(first, second);

        public IObservable<IThreadsafeTimeEntry> Update(EditTimeEntryDto dto)
            => GetById(dto.Id)
                 .Select(te => createUpdatedTimeEntry(te, dto))
                 .SelectMany(Update);

        private TimeEntry createUpdatedTimeEntry(IThreadsafeTimeEntry timeEntry, EditTimeEntryDto dto)
            => TimeEntry.Builder.Create(dto.Id)
                        .SetDescription(dto.Description)
                        .SetDuration(dto.StopTime.HasValue ? (long?)(dto.StopTime.Value - dto.StartTime).TotalSeconds : null)
                        .SetTagIds(dto.TagIds)
                        .SetStart(dto.StartTime)
                        .SetTaskId(dto.TaskId)
                        .SetBillable(dto.Billable)
                        .SetProjectId(dto.ProjectId)
                        .SetWorkspaceId(dto.WorkspaceId)
                        .SetUserId(timeEntry.UserId)
                        .SetIsDeleted(timeEntry.IsDeleted)
                        .SetServerDeletedAt(timeEntry.ServerDeletedAt)
                        .SetAt(timeService.CurrentDateTime)
                        .SetSyncStatus(SyncStatus.SyncNeeded)
                        .Build();

        private IThreadsafeTimeEntry runningTimeEntry(IThreadsafeTimeEntry timeEntry)
        {
            if (timeEntry == null || !timeEntry.IsRunning())
                return null;
           
            return TimeEntry.From(timeEntry);
        }

        private void setRunningTimeEntryId(IThreadsafeTimeEntry timeEntry)
        {
            currentlyRunningTimeEntryId = timeEntry?.Id;
        }

        private void maybeUpdateCurrentlyRunningTimeEntryId(long id, IThreadsafeTimeEntry entity)
        {
            if (id == currentlyRunningTimeEntryId)
                currentlyRunningTimeEntryId = entity.Id;
        }
    }
}
