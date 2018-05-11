using Toggl.PrimeRadiant;
using Toggl.Multivac.Models;
using Toggl.Multivac.Extensions;

namespace Toggl.Foundation.Sync.ConflictResolution
{
    public class OverwriteIfLocalDidNotChange<TModel, TDatabase> : IConflictResolver<TDatabase>
        where TModel : IApiModel
        where TDatabase : TModel, IDatabaseSyncable
    {
        private readonly TModel localEntity;

        public OverwriteIfLocalDidNotChange(TModel localEntity)
        {
            this.localEntity = localEntity;
        }

        public ConflictResolutionMode Resolve(TDatabase currentLocal, TDatabase serverEntity)
            => localEntity.HasChanged(currentLocal)
                ? ConflictResolutionMode.Ignore
                : ConflictResolutionMode.Update;
    }
}
