﻿namespace Toggl.Multivac.Models
{
    public interface ITask : IApiModel, IIdentifiable, ILastChangedDatable
    {
        string Name { get; }

        long ProjectId { get; }

        long WorkspaceId { get; }

        long? UserId { get; }

        long EstimatedSeconds { get; }

        bool Active { get; }

        long TrackedSeconds { get; }
    }
}
