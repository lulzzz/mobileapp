﻿using System;

namespace Toggl.Multivac.Models
{
    public interface IWorkspace : IApiModel, IIdentifiable, IDeletable, ILastChangedDatable
    {
        string Name { get; }

        bool Admin { get; }

        DateTimeOffset? SuspendedAt { get; }

        double? DefaultHourlyRate { get; }

        string DefaultCurrency { get; }

        bool OnlyAdminsMayCreateProjects { get; }

        bool OnlyAdminsSeeBillableRates { get; }

        bool OnlyAdminsSeeTeamDashboard { get; }

        bool ProjectsBillableByDefault { get; }

        int Rounding { get; }

        int RoundingMinutes { get; }

        string LogoUrl { get; }
    }
}
