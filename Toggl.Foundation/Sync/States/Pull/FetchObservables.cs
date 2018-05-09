using System;
using System.Collections.Generic;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Sync.States
{
    internal sealed class FetchObservables
    {
        public IObservable<IUser> User { get; }
        public IObservable<List<IWorkspace>> Workspaces { get; }
        public IObservable<List<IWorkspaceFeatureCollection>> WorkspaceFeatures { get; }
        public IObservable<List<IClient>> Clients { get; }
        public IObservable<List<IProject>> Projects { get; }
        public IObservable<List<ITag>> Tags { get; }
        public IObservable<List<ITask>> Tasks { get; }
        public IObservable<List<ITimeEntry>> TimeEntries { get; }
        public IObservable<IPreferences> Preferences { get; }

        public FetchObservables(FetchObservables old)
            : this(old.Workspaces, old.WorkspaceFeatures, old.User, old.Clients,
                   old.Projects, old.TimeEntries, old.Tags, old.Tasks, old.Preferences)
        {
        }

        public FetchObservables(
            IObservable<List<IWorkspace>> workspaces,
            IObservable<List<IWorkspaceFeatureCollection>> workspaceFeatures, 
            IObservable<IUser> user,
            IObservable<List<IClient>> clients,
            IObservable<List<IProject>> projects,
            IObservable<List<ITimeEntry>> timeEntries,
            IObservable<List<ITag>> tags,
            IObservable<List<ITask>> tasks,
            IObservable<IPreferences> preferences
        )
        {
            Workspaces = workspaces;
            WorkspaceFeatures = workspaceFeatures;
            User = user;
            Clients = clients;
            Projects = projects;
            TimeEntries = timeEntries;
            Tags = tags;
            Tasks = tasks;
            Preferences = preferences;
        }
    }
}
