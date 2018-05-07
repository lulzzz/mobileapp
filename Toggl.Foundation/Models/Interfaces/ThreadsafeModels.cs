using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Models.Interfaces
{
    public interface IThreadsafeClient
        : IThreadsafeModel, IDatabaseClient
    {
    }

    public interface IThreadsafePreferences
        : IThreadsafeModel, IDatabasePreferences
    {
    }

    public interface IThreadsafeProject
        : IThreadsafeModel, IDatabaseProject
    {
    }

    public interface IThreadsafeTag
        : IThreadsafeModel, IDatabaseTag
    {
    }

    public interface IThreadsafeTask
        : IThreadsafeModel, IDatabaseTask
    {
    }

    public interface IThreadsafeTimeEntry
        : IThreadsafeModel, IDatabaseTimeEntry
    {
    }

    public interface IThreadsafeUser
        : IThreadsafeModel, IDatabaseUser
    {
    }

    public interface IThreadsafeWorkspace
        : IThreadsafeModel, IDatabaseWorkspace
    {
    }

    public interface IThreadsafeWorkspaceFeature
        : IThreadsafeModel, IDatabaseWorkspaceFeature
    {
    }

    public interface IThreadsafeWorkspaceFeatureCollection
        : IThreadsafeModel, IDatabaseWorkspaceFeatureCollection
    {
    }
}
