using System;
using System.Collections.Generic;
using Toggl.Foundation.DataSources.Interfaces;
using Toggl.Foundation.Models.Interfaces;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.DataSources
{
    public interface IClientsSource : IDataSource<IThreadsafeClient, IDatabaseClient>
    {
        IObservable<IThreadsafeClient> Create(string name, long workspaceId);

        IObservable<IEnumerable<IThreadsafeClient>> GetAllInWorkspace(long workspaceId);
    }
}