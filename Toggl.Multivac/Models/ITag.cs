namespace Toggl.Multivac.Models
{
    public interface ITag : IApiModel, IIdentifiable, IDeletable, ILastChangedDatable
    {
        long WorkspaceId { get; }

        string Name { get; }
    }
}
