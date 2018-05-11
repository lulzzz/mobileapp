namespace Toggl.Multivac.Models
{
    public interface IClient : IApiModel, IIdentifiable, IDeletable, ILastChangedDatable
    {
        long WorkspaceId { get; }

        string Name { get; }
    }
}
