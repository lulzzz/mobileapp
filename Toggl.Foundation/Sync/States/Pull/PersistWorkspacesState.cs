﻿using System;
using System.Collections.Generic;
using System.Linq;
using Toggl.Foundation.Models;
using Toggl.Foundation.Sync.ConflictResolution;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Sync.States
{
    internal sealed class PersistWorkspacesState : BasePersistState<IWorkspace, IDatabaseWorkspace>
    {
        public PersistWorkspacesState(IRepository<IDatabaseWorkspace> repository, ISinceParameterRepository sinceParameterRepository)
            : base(repository, sinceParameterRepository, Resolver.ForWorkspaces)
        {
        }

        protected override long GetId(IDatabaseWorkspace entity) => entity.Id;

        protected override IObservable<IEnumerable<IWorkspace>> FetchObservable(FetchObservables fetch)
            => fetch.Workspaces;

        protected override IDatabaseWorkspace ConvertToDatabaseEntity(IWorkspace entity)
            => Workspace.Clean(entity);
    }
}
