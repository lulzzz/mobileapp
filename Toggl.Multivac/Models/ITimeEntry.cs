﻿using System;
using System.Collections.Generic;

namespace Toggl.Multivac.Models
{
    public interface ITimeEntry : IApiModel, IIdentifiable, IDeletable, ILastChangedDatable
    {
        long WorkspaceId { get; }

        long? ProjectId { get; }

        long? TaskId { get; }

        bool Billable { get; }

        DateTimeOffset Start { get; }

        long? Duration { get; }

        string Description { get; }

        IEnumerable<long> TagIds { get; }

        long UserId { get; }
    }
}
