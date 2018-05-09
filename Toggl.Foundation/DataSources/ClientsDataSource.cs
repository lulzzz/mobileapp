using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Toggl.Foundation.Models;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Foundation.Sync.ConflictResolution;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.DataSources
{
    public sealed class ClientsDataSource
        : DataSource<IThreadsafeClient, IDatabaseClient>, IClientsSource
    {
        private readonly IIdProvider idProvider;
        private readonly ITimeService timeService;
        private readonly IRepository<IDatabaseClient> repository;

        public ClientsDataSource(IIdProvider idProvider, IRepository<IDatabaseClient> repository, ITimeService timeService)
            : base(repository)
        {
            Ensure.Argument.IsNotNull(idProvider, nameof(idProvider));
            Ensure.Argument.IsNotNull(timeService, nameof(timeService));

            this.idProvider = idProvider;
            this.timeService = timeService;
        }

        public IObservable<IThreadsafeClient> Create(string name, long workspaceId)
            => idProvider.GetNextIdentifier()
                .Apply(Client.Builder.Create)
                .SetName(name)
                .SetWorkspaceId(workspaceId)
                .SetAt(timeService.CurrentDateTime)
                .SetSyncStatus(SyncStatus.SyncNeeded)
                .Build()
                .Apply(Create);

        public IObservable<IEnumerable<IThreadsafeClient>> GetAllInWorkspace(long workspaceId)
            => GetAll(c => c.WorkspaceId == workspaceId);

        protected override IThreadsafeClient Convert(IDatabaseClient entity)
            => Client.From(entity);

        protected override ConflictResolutionMode ResolveConflicts(IDatabaseClient first, IDatabaseClient second)
            => Resolver.ForClients.Resolve(first, second);
    }
}
