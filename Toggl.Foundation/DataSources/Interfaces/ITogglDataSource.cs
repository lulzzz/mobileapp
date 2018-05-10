using System;
using System.Reactive;
using Toggl.Foundation.Autocomplete;
using Toggl.Foundation.DataSources.Interfaces;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Foundation.Reports;
using Toggl.Foundation.Sync;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.DataSources
{
    public interface ITogglDataSource
    {
        ITagsSource Tags { get; }
        IUserSource User { get; }
        IPreferencesSource Preferences { get; }
        ITasksSource Tasks { get; }
        IClientsSource Clients { get; }
        IProjectsSource Projects { get; }
        ITimeEntriesSource TimeEntries { get; }
        IDataSource<IThreadsafeWorkspace, IDatabaseWorkspace> Workspaces { get; }
        IDataSource<IThreadsafeWorkspaceFeatureCollection, IDatabaseWorkspaceFeatureCollection> WorkspaceFeatures { get; }
        
        ISyncManager SyncManager { get; }
        IAutocompleteProvider AutocompleteProvider { get; }

        IObservable<Unit> StartSyncing();
        IReportsProvider ReportsProvider { get; }

        IObservable<bool> HasUnsyncedData();

        IObservable<Unit> Logout();
    }
}
