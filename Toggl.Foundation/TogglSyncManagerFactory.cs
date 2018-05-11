using System;
using Toggl.Foundation.Sync;
using Toggl.Foundation.Sync.States;
using Toggl.PrimeRadiant;
using Toggl.Ultrawave;
using System.Reactive.Concurrency;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant.Models;
using Toggl.Foundation.DataSources;
using System.Reactive.Linq;
using System.Reactive;
using System.Reactive.Subjects;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.Models;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Foundation.Sync.States.Push;
using Toggl.Foundation.Sync.States.Push.Interfaces;

namespace Toggl.Foundation
{
    public static class TogglSyncManager
    {
        public static ISyncManager CreateSyncManager(
            ITogglDatabase database,
            ITogglApi api,
            ITogglDataSource dataSource,
            ITimeService timeService,
            IAnalyticsService analyticsService,
            TimeSpan? retryLimit,
            IScheduler scheduler)
        {
            var random = new Random();
            var queue = new SyncStateQueue();
            var entryPoints = new StateMachineEntryPoints();
            var transitions = new TransitionHandlerProvider();
            var apiDelay = new RetryDelayService(random, retryLimit);
            var delayCancellation = new Subject<Unit>();
            var delayCancellationObservable = delayCancellation.AsObservable().Replay();
            ConfigureTransitions(transitions, database, api, dataSource, apiDelay, scheduler, timeService, entryPoints, delayCancellationObservable);
            var stateMachine = new StateMachine(transitions, scheduler, delayCancellation);
            var orchestrator = new StateMachineOrchestrator(stateMachine, entryPoints);

            return new SyncManager(queue, orchestrator, analyticsService);
        }

        public static void ConfigureTransitions(
            TransitionHandlerProvider transitions,
            ITogglDatabase database,
            ITogglApi api,
            ITogglDataSource dataSource,
            IRetryDelayService apiDelay,
            IScheduler scheduler,
            ITimeService timeService,
            StateMachineEntryPoints entryPoints,
            IObservable<Unit> delayCancellation)
        {
            configurePullTransitions(transitions, database, api, dataSource, timeService, scheduler, entryPoints.StartPullSync, delayCancellation);
            configurePushTransitions(transitions, api, dataSource, apiDelay, scheduler, entryPoints.StartPushSync, delayCancellation);
        }

        private static void configurePullTransitions(
            TransitionHandlerProvider transitions,
            ITogglDatabase database,
            ITogglApi api,
            ITogglDataSource dataSource,
            ITimeService timeService,
            IScheduler scheduler,
            StateResult entryPoint,
            IObservable<Unit> delayCancellation)
        {
            var rnd = new Random();
            var apiDelay = new RetryDelayService(rnd);
            var statusDelay = new RetryDelayService(rnd);

            var fetchAllSince = new FetchAllSinceState(database, api, timeService);
            var persistWorkspaces = new PersistWorkspacesState(database.Workspaces, database.SinceParameters);
            var persistWorkspaceFeatures = new PersistWorkspacesFeaturesState(database.WorkspaceFeatures, database.SinceParameters);
            var persistUser = new PersistUserState(database.User, database.SinceParameters);
            var persistTags = new PersistTagsState(database.Tags, database.SinceParameters);
            var persistClients = new PersistClientsState(database.Clients, database.SinceParameters);
            var persistPreferences = new PersistPreferencesState(database.Preferences, database.SinceParameters);
            var persistProjects = new PersistProjectsState(database.Projects, database.SinceParameters);
            var persistTimeEntries = new PersistTimeEntriesState(database.TimeEntries, database.SinceParameters, timeService);
            var persistTasks = new PersistTasksState(database.Tasks, database.SinceParameters);
            var checkServerStatus = new CheckServerStatusState(api, scheduler, apiDelay, statusDelay, delayCancellation);
            var finished = new ResetAPIDelayState(apiDelay);

            transitions.ConfigureTransition(entryPoint, fetchAllSince.Start);
            transitions.ConfigureTransition(fetchAllSince.FetchStarted, persistWorkspaces.Start);
            transitions.ConfigureTransition(persistWorkspaces.FinishedPersisting, persistUser.Start);
            transitions.ConfigureTransition(persistUser.FinishedPersisting, persistWorkspaceFeatures.Start);
            transitions.ConfigureTransition(persistWorkspaceFeatures.FinishedPersisting, persistPreferences.Start);
            transitions.ConfigureTransition(persistPreferences.FinishedPersisting, persistTags.Start);
            transitions.ConfigureTransition(persistTags.FinishedPersisting, persistClients.Start);
            transitions.ConfigureTransition(persistClients.FinishedPersisting, persistProjects.Start);
            transitions.ConfigureTransition(persistProjects.FinishedPersisting, persistTasks.Start);
            transitions.ConfigureTransition(persistTasks.FinishedPersisting, persistTimeEntries.Start);

            transitions.ConfigureTransition(persistWorkspaces.Failed, checkServerStatus.Start);
            transitions.ConfigureTransition(persistWorkspaceFeatures.Failed, checkServerStatus.Start);
            transitions.ConfigureTransition(persistPreferences.Failed, checkServerStatus.Start);
            transitions.ConfigureTransition(persistTags.Failed, checkServerStatus.Start);
            transitions.ConfigureTransition(persistClients.Failed, checkServerStatus.Start);
            transitions.ConfigureTransition(persistProjects.Failed, checkServerStatus.Start);
            transitions.ConfigureTransition(persistTasks.Failed, checkServerStatus.Start);
            transitions.ConfigureTransition(persistTimeEntries.Failed, checkServerStatus.Start);

            transitions.ConfigureTransition(checkServerStatus.Retry, checkServerStatus.Start);
            transitions.ConfigureTransition(checkServerStatus.ServerIsAvailable, finished.Start);
            transitions.ConfigureTransition(finished.Continue, fetchAllSince.Start);
        }
        
        private static void configurePushTransitions(
            TransitionHandlerProvider transitions,
            ITogglApi api,
            ITogglDataSource dataSource,
            IRetryDelayService apiDelay,
            IScheduler scheduler,
            StateResult entryPoint,
            IObservable<Unit> delayCancellation)
        {
            var pushingUsersFinished = configurePushTransitionsForUsers(transitions, dataSource, api, scheduler, entryPoint, delayCancellation);
            var pushingPreferencesFinished = configurePushTransitionsForPreferences(transitions, dataSource, api, scheduler, pushingUsersFinished, delayCancellation);
            var pushingTagsFinished = configurePushTransitionsForTags(transitions, dataSource, api, scheduler, pushingPreferencesFinished, delayCancellation);
            var pushingClientsFinished = configurePushTransitionsForClients(transitions, dataSource, api, scheduler, pushingTagsFinished, delayCancellation);
            var pushingProjectsFinished = configurePushTransitionsForProjects(transitions, dataSource, api, scheduler, pushingClientsFinished, delayCancellation);
            configurePushTransitionsForTimeEntries(transitions, dataSource, api, apiDelay, scheduler, pushingProjectsFinished, delayCancellation);
        }

        private static IStateResult configurePushTransitionsForTimeEntries(
            TransitionHandlerProvider transitions,
            ITogglDataSource dataSource,
            ITogglApi api,
            IRetryDelayService apiDelay,
            IScheduler scheduler,
            IStateResult entryPoint,
            IObservable<Unit> delayCancellation)
        {
            var rnd = new Random();
            var statusDelay = new RetryDelayService(rnd);

            var push = new PushState<IDatabaseTimeEntry, IThreadsafeTimeEntry>(dataSource.TimeEntries);
            var pushOne = new PushOneEntityState<IThreadsafeTimeEntry>();
            var create = new CreateEntityState<ITimeEntry, IDatabaseTimeEntry, IThreadsafeTimeEntry>(api.TimeEntries, dataSource.TimeEntries, TimeEntry.Clean);
            var update = new UpdateEntityState<ITimeEntry, IDatabaseTimeEntry, IThreadsafeTimeEntry>(api.TimeEntries, dataSource.TimeEntries, TimeEntry.Clean);
            var delete = new DeleteEntityState<ITimeEntry, IDatabaseTimeEntry, IThreadsafeTimeEntry>(api.TimeEntries, dataSource.TimeEntries);
            var deleteLocal = new DeleteLocalEntityState<IDatabaseTimeEntry, IThreadsafeTimeEntry>(dataSource.TimeEntries);
            var tryResolveClientError = new TryResolveClientErrorState<IThreadsafeTimeEntry>();
            var unsyncable = new UnsyncableEntityState<IDatabaseTimeEntry, IThreadsafeTimeEntry>(dataSource.TimeEntries, TimeEntry.Unsyncable);
            var checkServerStatus = new CheckServerStatusState(api, scheduler, apiDelay, statusDelay, delayCancellation);
            var finished = new ResetAPIDelayState(apiDelay);

            return configurePush(transitions, entryPoint, push, pushOne, create, update, delete, deleteLocal, tryResolveClientError, unsyncable, checkServerStatus, finished);
        }

        private static IStateResult configurePushTransitionsForTags(
            TransitionHandlerProvider transitions,
            ITogglDataSource dataSource,
            ITogglApi api,
            IScheduler scheduler,
            IStateResult entryPoint,
            IObservable<Unit> delayCancellation)
        {
            var rnd = new Random();
            var apiDelay = new RetryDelayService(rnd);
            var statusDelay = new RetryDelayService(rnd);

            var push = new PushState<IDatabaseTag, IThreadsafeTag>(dataSource.Tags);
            var pushOne = new PushOneEntityState<IThreadsafeTag>();
            var create = new CreateEntityState<ITag, IDatabaseTag, IThreadsafeTag>(api.Tags, dataSource.Tags, Tag.Clean);
            var tryResolveClientError = new TryResolveClientErrorState<IThreadsafeTag>();
            var unsyncable = new UnsyncableEntityState<IDatabaseTag, IThreadsafeTag>(dataSource.Tags, Tag.Unsyncable);
            var checkServerStatus = new CheckServerStatusState(api, scheduler, apiDelay, statusDelay, delayCancellation);
            var finished = new ResetAPIDelayState(apiDelay);

            return configureCreateOnlyPush(transitions, entryPoint, push, pushOne, create, tryResolveClientError, unsyncable, checkServerStatus, finished);
        }

        private static IStateResult configurePushTransitionsForClients(
            TransitionHandlerProvider transitions,
            ITogglDataSource dataSource,
            ITogglApi api,
            IScheduler scheduler,
            IStateResult entryPoint,
            IObservable<Unit> delayCancellation)
        {
            var rnd = new Random();
            var apiDelay = new RetryDelayService(rnd);
            var statusDelay = new RetryDelayService(rnd);

            var push = new PushState<IDatabaseClient, IThreadsafeClient>(dataSource.Clients);
            var pushOne = new PushOneEntityState<IThreadsafeClient>();
            var create = new CreateEntityState<IClient, IDatabaseClient, IThreadsafeClient>(api.Clients, dataSource.Clients, Client.Clean);
            var tryResolveClientError = new TryResolveClientErrorState<IThreadsafeClient>();
            var unsyncable = new UnsyncableEntityState<IDatabaseClient, IThreadsafeClient>(dataSource.Clients, Client.Unsyncable);
            var checkServerStatus = new CheckServerStatusState(api, scheduler, apiDelay, statusDelay, delayCancellation);
            var finished = new ResetAPIDelayState(apiDelay);

            return configureCreateOnlyPush(transitions, entryPoint, push, pushOne, create, tryResolveClientError, unsyncable, checkServerStatus, finished);
        }

        private static IStateResult configurePushTransitionsForProjects(
            TransitionHandlerProvider transitions,
            ITogglDataSource dataSource,
            ITogglApi api,
            IScheduler scheduler,
            IStateResult entryPoint,
            IObservable<Unit> delayCancellation)
        {
            var rnd = new Random();
            var apiDelay = new RetryDelayService(rnd);
            var statusDelay = new RetryDelayService(rnd);

            var push = new PushState<IDatabaseProject, IThreadsafeProject>(dataSource.Projects);
            var pushOne = new PushOneEntityState<IThreadsafeProject>();
            var create = new CreateEntityState<IProject, IDatabaseProject, IThreadsafeProject>(api.Projects, dataSource.Projects, Project.Clean);
            var tryResolveClientError = new TryResolveClientErrorState<IThreadsafeProject>();
            var unsyncable = new UnsyncableEntityState<IDatabaseProject, IThreadsafeProject>(dataSource.Projects, Project.Unsyncable);
            var checkServerStatus = new CheckServerStatusState(api, scheduler, apiDelay, statusDelay, delayCancellation);
            var finished = new ResetAPIDelayState(apiDelay);

            return configureCreateOnlyPush(transitions, entryPoint, push, pushOne, create, tryResolveClientError, unsyncable, checkServerStatus, finished);
        }

        private static IStateResult configurePushTransitionsForUsers(
            TransitionHandlerProvider transitions,
            ITogglDataSource dataSource,
            ITogglApi api,
            IScheduler scheduler,
            IStateResult entryPoint,
            IObservable<Unit> delayCancellation)
        {
            var rnd = new Random();
            var apiDelay = new RetryDelayService(rnd);
            var statusDelay = new RetryDelayService(rnd);

            var push = new PushState<IDatabaseUser, IThreadsafeUser>(dataSource.User);
            var pushOne = new PushOneEntityState<IThreadsafeUser>();
            var update = new UpdateEntityState<IUser, IDatabaseUser, IThreadsafeUser>(api.User, dataSource.User, User.Clean);
            var tryResolveClientError = new TryResolveClientErrorState<IThreadsafeUser>();
            var unsyncable = new UnsyncableEntityState<IDatabaseUser, IThreadsafeUser>(dataSource.User, User.Unsyncable);
            var checkServerStatus = new CheckServerStatusState(api, scheduler, apiDelay, statusDelay, delayCancellation);
            var finished = new ResetAPIDelayState(apiDelay);

            return configureUpdateOnlyPush(
                transitions, entryPoint, push, pushOne, update, tryResolveClientError, unsyncable, checkServerStatus, finished);
        }

        private static IStateResult configurePushTransitionsForPreferences(
            TransitionHandlerProvider transitions,
            ITogglDataSource dataSource,
            ITogglApi api,
            IScheduler scheduler,
            IStateResult entryPoint,
            IObservable<Unit> delayCancellation)
        {
            var rnd = new Random();
            var apiDelay = new RetryDelayService(rnd);
            var statusDelay = new RetryDelayService(rnd);

            var push = new PushSingleState<IDatabasePreferences, IThreadsafePreferences>(dataSource.Preferences);
            var pushOne = new PushOneEntityState<IThreadsafePreferences>();
            var update = new UpdateEntityState<IPreferences, IDatabasePreferences, IThreadsafePreferences>(api.Preferences, dataSource.Preferences, Preferences.Clean);
            var tryResolveClientError = new TryResolveClientErrorState<IThreadsafePreferences>();
            var unsyncable = new UnsyncableEntityState<IDatabasePreferences, IThreadsafePreferences>(dataSource.Preferences, Preferences.Unsyncable);
            var checkServerStatus = new CheckServerStatusState(api, scheduler, apiDelay, statusDelay, delayCancellation);
            var finished = new ResetAPIDelayState(apiDelay);

            return configureUpdateOnlyPush(
                transitions, entryPoint, push, pushOne, update, tryResolveClientError, unsyncable, checkServerStatus, finished);
        }

        private static IStateResult configurePush<TModel, TDatabase, TThreadsafe>(
            TransitionHandlerProvider transitions,
            IStateResult entryPoint,
            PushState<TDatabase, TThreadsafe> push,
            PushOneEntityState<TThreadsafe> pushOne,
            CreateEntityState<TModel, TDatabase, TThreadsafe> create,
            UpdateEntityState<TModel, TDatabase, TThreadsafe> update,
            DeleteEntityState<TModel, TDatabase, TThreadsafe> delete,
            DeleteLocalEntityState<TDatabase, TThreadsafe> deleteLocal,
            TryResolveClientErrorState<TThreadsafe> tryResolveClientError,
            UnsyncableEntityState<TDatabase, TThreadsafe> markUnsyncable,
            CheckServerStatusState checkServerStatus,
            ResetAPIDelayState finished)
            where TModel : IApiModel, IIdentifiable, ILastChangedDatable
            where TDatabase : class, TModel, IDatabaseSyncable
            where TThreadsafe : class, TDatabase, IThreadsafeModel
        {
            transitions.ConfigureTransition(entryPoint, push.Start);
            transitions.ConfigureTransition(push.PushEntity, pushOne.Start);
            transitions.ConfigureTransition(pushOne.CreateEntity, create.Start);
            transitions.ConfigureTransition(pushOne.UpdateEntity, update.Start);
            transitions.ConfigureTransition(pushOne.DeleteEntity, delete.Start);
            transitions.ConfigureTransition(pushOne.DeleteEntityLocally, deleteLocal.Start);

            transitions.ConfigureTransition(create.ClientError, tryResolveClientError.Start);
            transitions.ConfigureTransition(update.ClientError, tryResolveClientError.Start);
            transitions.ConfigureTransition(delete.ClientError, tryResolveClientError.Start);

            transitions.ConfigureTransition(create.ServerError, checkServerStatus.Start);
            transitions.ConfigureTransition(update.ServerError, checkServerStatus.Start);
            transitions.ConfigureTransition(delete.ServerError, checkServerStatus.Start);

            transitions.ConfigureTransition(create.UnknownError, checkServerStatus.Start);
            transitions.ConfigureTransition(update.UnknownError, checkServerStatus.Start);
            transitions.ConfigureTransition(delete.UnknownError, checkServerStatus.Start);

            transitions.ConfigureTransition(tryResolveClientError.UnresolvedTooManyRequests, checkServerStatus.Start);
            transitions.ConfigureTransition(tryResolveClientError.Unresolved, markUnsyncable.Start);

            transitions.ConfigureTransition(checkServerStatus.Retry, checkServerStatus.Start);
            transitions.ConfigureTransition(checkServerStatus.ServerIsAvailable, push.Start);

            transitions.ConfigureTransition(create.CreatingFinished, finished.Start);
            transitions.ConfigureTransition(update.UpdatingSucceeded, finished.Start);
            transitions.ConfigureTransition(delete.DeletingFinished, finished.Start);
            transitions.ConfigureTransition(deleteLocal.Deleted, finished.Start);
            transitions.ConfigureTransition(deleteLocal.DeletingFailed, finished.Start);

            transitions.ConfigureTransition(finished.Continue, push.Start);

            return push.NothingToPush;
        }

        private static IStateResult configureCreateOnlyPush<TModel, TDatabase, TThreadsafe>(
            TransitionHandlerProvider transitions,
            IStateResult entryPoint,
            PushState<TDatabase, TThreadsafe> push,
            PushOneEntityState<TThreadsafe> pushOne,
            CreateEntityState<TModel, TDatabase, TThreadsafe> create,
            TryResolveClientErrorState<TThreadsafe> tryResolveClientError,
            UnsyncableEntityState<TDatabase, TThreadsafe> markUnsyncable,
            CheckServerStatusState checkServerStatus,
            ResetAPIDelayState finished)
            where TModel : IApiModel, IIdentifiable, ILastChangedDatable
            where TDatabase : class, TModel, IDatabaseSyncable
            where TThreadsafe : class, TDatabase, IThreadsafeModel
        {
            transitions.ConfigureTransition(entryPoint, push.Start);
            transitions.ConfigureTransition(push.PushEntity, pushOne.Start);
            transitions.ConfigureTransition(pushOne.CreateEntity, create.Start);

            transitions.ConfigureTransition(pushOne.UpdateEntity, new InvalidTransitionState($"Updating is not supported for {typeof(TModel).Name} during Push sync.").Start);
            transitions.ConfigureTransition(pushOne.DeleteEntity, new InvalidTransitionState($"Deleting is not supported for {typeof(TModel).Name} during Push sync.").Start);
            transitions.ConfigureTransition(pushOne.DeleteEntityLocally, new InvalidTransitionState($"Deleting locally is not supported for {typeof(TModel).Name} during Push sync.").Start);

            transitions.ConfigureTransition(create.ClientError, tryResolveClientError.Start);
            transitions.ConfigureTransition(create.ServerError, checkServerStatus.Start);
            transitions.ConfigureTransition(create.UnknownError, checkServerStatus.Start);

            transitions.ConfigureTransition(tryResolveClientError.UnresolvedTooManyRequests, checkServerStatus.Start);
            transitions.ConfigureTransition(tryResolveClientError.Unresolved, markUnsyncable.Start);

            transitions.ConfigureTransition(checkServerStatus.Retry, checkServerStatus.Start);
            transitions.ConfigureTransition(checkServerStatus.ServerIsAvailable, push.Start);

            transitions.ConfigureTransition(create.CreatingFinished, finished.Start);

            transitions.ConfigureTransition(finished.Continue, push.Start);

            return push.NothingToPush;
        }

        private static IStateResult configureUpdateOnlyPush<TModel, TDatabase, TThreadsafe>(
            TransitionHandlerProvider transitions,
            IStateResult entryPoint,
            IPushState<TThreadsafe> push,
            PushOneEntityState<TThreadsafe> pushOne,
            IUpdateEntityState<TModel, TThreadsafe> update,
            TryResolveClientErrorState<TThreadsafe> tryResolveClientError,
            UnsyncableEntityState<TDatabase, TThreadsafe> markUnsyncable,
            CheckServerStatusState checkServerStatus,
            ResetAPIDelayState finished)
            where TModel : IApiModel
            where TDatabase : class, TModel, IDatabaseSyncable
            where TThreadsafe : class, TDatabase, IThreadsafeModel
        {
            transitions.ConfigureTransition(entryPoint, push.Start);
            transitions.ConfigureTransition(push.PushEntity, pushOne.Start);
            transitions.ConfigureTransition(pushOne.UpdateEntity, update.Start);

            transitions.ConfigureTransition(pushOne.CreateEntity, new InvalidTransitionState($"Creating is not supported for {typeof(TModel).Name} during Push sync.").Start);
            transitions.ConfigureTransition(pushOne.DeleteEntity, new InvalidTransitionState($"Deleting is not supported for {typeof(TModel).Name} during Push sync.").Start);
            transitions.ConfigureTransition(pushOne.DeleteEntityLocally, new InvalidTransitionState($"Deleting locally is not supported for {typeof(TModel).Name} during Push sync.").Start);

            transitions.ConfigureTransition(update.ClientError, tryResolveClientError.Start);
            transitions.ConfigureTransition(update.ServerError, checkServerStatus.Start);
            transitions.ConfigureTransition(update.UnknownError, checkServerStatus.Start);

            transitions.ConfigureTransition(tryResolveClientError.UnresolvedTooManyRequests, checkServerStatus.Start);
            transitions.ConfigureTransition(tryResolveClientError.Unresolved, markUnsyncable.Start);

            transitions.ConfigureTransition(checkServerStatus.Retry, checkServerStatus.Start);
            transitions.ConfigureTransition(checkServerStatus.ServerIsAvailable, push.Start);

            transitions.ConfigureTransition(update.UpdatingSucceeded, finished.Start);

            transitions.ConfigureTransition(finished.Continue, push.Start);

            return push.NothingToPush;
        }
    }
}
