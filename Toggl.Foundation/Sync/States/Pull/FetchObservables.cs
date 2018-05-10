using System;
using System.Collections.Generic;
using System.Reactive.Linq;
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

        public FetchObservables(
            IObservable<List<IWorkspace>> workspaces,
            IObservable<List<IWorkspaceFeatureCollection>> workspaceFeatures, 
            IObservable<IUser> user,
            IObservable<List<IClient>> clients,
            IObservable<List<IProject>> projects,
            IObservable<List<ITimeEntry>> timeEntries,
            IObservable<List<ITag>> tags,
            IObservable<List<ITask>> tasks,
            IObservable<IPreferences> preferences)
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

        public IObservable<List<T>> GetByType<T>()
        {
            if (typeof(T) == typeof(IWorkspace))
                return (IObservable<List<T>>)Workspaces;

            if (typeof(T) == typeof(IWorkspaceFeatureCollection))
                return (IObservable<List<T>>)WorkspaceFeatures;

            if (typeof(T) == typeof(IClient))
                return (IObservable<List<T>>)Clients;

            if (typeof(T) == typeof(IProject))
                return (IObservable<List<T>>)Projects;

            if (typeof(T) == typeof(ITag))
                return (IObservable<List<T>>)Tags;

            if (typeof(T) == typeof(ITask))
                return (IObservable<List<T>>)Tasks;
            
            if (typeof(T) == typeof(IUser))
                return (IObservable<List<T>>)User.Select(user => new List<IUser> { user });

            if (typeof(T) == typeof(IPreferences))
                return (IObservable<List<T>>)Preferences.Select(preferences => new List<IPreferences> { preferences });

            if (typeof(T) == typeof(ITimeEntry))
                return (IObservable<List<T>>)TimeEntries;

            throw new ArgumentException($"Type {typeof(T).FullName} is not supported by the {nameof(FetchObservables)} class.");
        }
    }
}
