﻿using System;
using System.Collections.Generic;
using System.Reactive;
using Toggl.Foundation.Models;
using Toggl.Foundation.Suggestions;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Interactors
{
    public interface IInteractorFactory
    {
        #region Time Entries

        IInteractor<IObservable<IDatabaseTimeEntry>> CreateTimeEntry(ITimeEntryPrototype prototype);

        IInteractor<IObservable<IDatabaseTimeEntry>> StartSuggestion(Suggestion suggestion);

        IInteractor<IObservable<IDatabaseTimeEntry>> ContinueTimeEntry(ITimeEntryPrototype prototype);

        IInteractor<IObservable<IDatabaseTimeEntry>> ContinueMostRecentTimeEntry();

        IInteractor<IObservable<Unit>> DeleteTimeEntry(long id);

        #endregion

        #region Projects

        IInteractor<IObservable<bool>> ProjectDefaultsToBillable(long projectId);

        IInteractor<IObservable<bool>> IsBillableAvailableForProject(long projectId);

        #endregion

        #region Workspaces

        IInteractor<IObservable<IDatabaseWorkspace>> GetDefaultWorkspace();

        IInteractor<IObservable<IEnumerable<IDatabaseWorkspace>>> GetAllWorkspaces();

        IInteractor<IObservable<IDatabaseWorkspace>> GetWorkspaceById(long workspaceId);

        IInteractor<IObservable<bool?>> AreProjectsBillableByDefault(long workspaceId);

        IInteractor<IObservable<bool>> WorkspaceAllowsBillableRates(long workspaceId);

        IInteractor<IObservable<bool>> AreCustomColorsEnabledForWorkspace(long workspaceId);

        IInteractor<IObservable<bool>> IsBillableAvailableForWorkspace(long workspaceId);

        #endregion
    }
}
