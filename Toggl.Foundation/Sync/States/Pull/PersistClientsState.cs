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
    internal sealed class PersistClientsState : BasePersistState<IClient, IDatabaseClient>
    {
        public PersistClientsState(IRepository<IDatabaseClient> repository, ISinceParameterRepository sinceParameterRepository)
            : base(repository, sinceParameterRepository, Resolver.ForClients())
        {
        }

        protected override long GetId(IDatabaseClient entity) => entity.Id;

        protected override IObservable<IEnumerable<IClient>> FetchObservable(FetchObservables fetch)
            => fetch.Clients;

        protected override IDatabaseClient ConvertToDatabaseEntity(IClient entity)
            => Client.Clean(entity);
    }
}
