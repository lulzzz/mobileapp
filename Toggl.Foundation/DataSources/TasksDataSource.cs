﻿using System;
using System.Collections.Generic;
using Toggl.Foundation.Models;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Foundation.Sync.ConflictResolution;
using Toggl.Multivac;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.DataSources
{
    public sealed class TasksDataSource
        : DataSource<IThreadsafeTask, IDatabaseTask>, ITasksSource
    {
        public TasksDataSource(IRepository<IDatabaseTask> repository)
            : base(repository)
        {
        }

        protected override IThreadsafeTask Convert(IDatabaseTask entity)
            => Task.From(entity);

        protected override ConflictResolutionMode ResolveConflicts(IDatabaseTask first, IDatabaseTask second)
            => Resolver.ForTasks.Resolve(first, second);
    }
}
