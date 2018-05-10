using System.Collections.Generic;
using System.Linq;
using Realms;
using Toggl.Multivac;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant.Models;

namespace Toggl.PrimeRadiant.Realm
{
    internal partial class RealmWorkspaceFeatureCollection : RealmObject, IDatabaseWorkspaceFeatureCollection
    {
        [Ignored]
        public RealmWorkspace RealmWorkspace
        {
            get => RealmWorkspaceInternal;
            set
            {
                WorkspaceId = value.Id;
                RealmWorkspaceInternal = value;
            }
        }
        
        public RealmWorkspace RealmWorkspaceInternal { get; set; }

        public long WorkspaceId { get; set; }
        
        public IDatabaseWorkspace Workspace => RealmWorkspace;

        public IList<RealmWorkspaceFeature> RealmWorkspaceFeatures { get; }

        public IEnumerable<IDatabaseWorkspaceFeature> DatabaseFeatures => RealmWorkspaceFeatures;

        public IEnumerable<IWorkspaceFeature> Features => RealmWorkspaceFeatures;

        public bool IsEnabled(WorkspaceFeatureId feature)
            => Features.Any(f => f.FeatureId == feature && f.Enabled);
    }
}
