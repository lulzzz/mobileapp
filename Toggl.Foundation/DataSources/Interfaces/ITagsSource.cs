using System;
using System.Collections.Generic;
using Toggl.Foundation.DataSources.Interfaces;
using Toggl.Foundation.Models.Interfaces;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.DataSources
{
    public interface ITagsSource : IDataSource<IThreadsafeTag, IDatabaseTag>
    {
        IObservable<IThreadsafeTag> Create(string name, long workspaceId);
    }
}