using System.Collections.Generic;

namespace Toggl.Multivac.Models
{
    public interface IWorkspaceFeatureCollection : IApiModel
    {
        long WorkspaceId { get; }

        IEnumerable<IWorkspaceFeature> Features { get; }

        bool IsEnabled(WorkspaceFeatureId feature);
    }
}
