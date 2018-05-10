﻿using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Models.Interfaces
{
    public partial interface IThreadsafeClient
        : IThreadsafeModel, IDatabaseClient
    {
    }

    public partial interface IThreadsafePreferences
        : IThreadsafeModel, IDatabasePreferences
    {
    }

    public partial interface IThreadsafeProject
        : IThreadsafeModel, IDatabaseProject
    {
    }

    public partial interface IThreadsafeTag
        : IThreadsafeModel, IDatabaseTag
    {
    }

    public partial interface IThreadsafeTask
        : IThreadsafeModel, IDatabaseTask
    {
    }

    public partial interface IThreadsafeTimeEntry
        : IThreadsafeModel, IDatabaseTimeEntry
    {
    }

    public partial interface IThreadsafeUser
        : IThreadsafeModel, IDatabaseUser
    {
    }

    public partial interface IThreadsafeWorkspace
        : IThreadsafeModel, IDatabaseWorkspace
    {
    }

    public partial interface IThreadsafeWorkspaceFeature
        : IThreadsafeModel, IDatabaseWorkspaceFeature
    {
    }

    public partial interface IThreadsafeWorkspaceFeatureCollection
        : IThreadsafeModel, IDatabaseWorkspaceFeatureCollection
    {
    }
}
