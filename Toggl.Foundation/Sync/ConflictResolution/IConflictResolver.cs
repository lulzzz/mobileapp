using Toggl.PrimeRadiant;

namespace Toggl.Foundation.Sync.ConflictResolution
{
    public interface IConflictResolver<T>
    {
        ConflictResolutionMode Resolve(T localEntity, T serverEntity);
    }
}
