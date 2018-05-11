namespace Toggl.Multivac.Models
{
    public interface IWorkspaceFeature : IApiModel
    {
        WorkspaceFeatureId FeatureId { get; }

        bool Enabled { get; }
    }
}
