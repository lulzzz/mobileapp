using System.Linq;
using Toggl.Multivac;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;

namespace Toggl.Foundation.Models
{
    internal partial class WorkspaceFeatureCollection
    {
        public long Id => WorkspaceId;

        public SyncStatus SyncStatus => SyncStatus.InSync;

        public string LastSyncErrorMessage => null;

        public bool IsDeleted => false;

        public bool IsEnabled(WorkspaceFeatureId feature)
            => Features.Any(x => x.FeatureId == feature);

        public static WorkspaceFeatureCollection From(IWorkspaceFeatureCollection entity)
            => new WorkspaceFeatureCollection(entity);
    }
}
