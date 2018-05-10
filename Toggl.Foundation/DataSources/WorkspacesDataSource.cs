using Toggl.Foundation.Models;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Foundation.Sync.ConflictResolution;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.DataSources
{
    public sealed class WorkspacesDataSource : DataSource<IThreadsafeWorkspace, IDatabaseWorkspace>
    {
        public WorkspacesDataSource(IRepository<IDatabaseWorkspace> repository)
            : base(repository)
        {
        }

        protected override IThreadsafeWorkspace Convert(IDatabaseWorkspace entity)
            => Workspace.From(entity);

        protected override ConflictResolutionMode ResolveConflicts(IDatabaseWorkspace first, IDatabaseWorkspace second)
            => Resolver.ForWorkspaces.Resolve(first, second);
    }
}
