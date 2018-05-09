using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Toggl.Foundation.DTOs;
using Toggl.Foundation.Models;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Foundation.Sync.ConflictResolution;
using Toggl.Multivac;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.DataSources
{
    public sealed class PreferencesDataSource
        : SingleObjectDataSource<IThreadsafePreferences, IDatabasePreferences>, IPreferencesSource
    {
        public PreferencesDataSource(ISingleObjectStorage<IDatabasePreferences> storage)
            : base(storage, Preferences.DefaultPreferences)
        {
        }

        public IObservable<IThreadsafePreferences> Update(EditPreferencesDTO dto)
            => Get()
                .Select(preferences => updatedPreferences(preferences, dto))
                .SelectMany(Update);

        public override IObservable<Unit> Delete(long id)
        {
            throw new InvalidOperationException("Preferences cannot be deleted.");
        }

        protected override IThreadsafePreferences Convert(IDatabasePreferences entity)
            => Preferences.From(entity);

        protected override ConflictResolutionMode ConflictResolution(IDatabasePreferences first, IDatabasePreferences second)
            => Resolver.ForPreferences.Resolve(first, second);

        private IThreadsafePreferences updatedPreferences(IThreadsafePreferences existing, EditPreferencesDTO dto)
            => Preferences.Builder
                .FromExisting(existing)
                .SetFrom(dto)
                .SetSyncStatus(SyncStatus.SyncNeeded)
                .Build();
    }
}
