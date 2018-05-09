using System;
using System.Collections.Generic;
using Toggl.Foundation.DataSources.Interfaces;
using Toggl.Foundation.Models.Interfaces;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.DataSources
{
    public interface ITasksSource
        : IDataSource<IThreadsafeTask, IDatabaseTask>
    {
    }
}