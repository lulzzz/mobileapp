using System;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;

namespace Toggl.Foundation.Reports
{
    [Preserve(AllMembers = true)]
    public struct ChartSegment
    {
        public TimeSpan TrackedTime { get; }

        public float BillableSeconds { get; }

        public float Percentage { get; }

        public string ProjectName { get; }

        public string ClientName { get; }

        public bool HasClient => !string.IsNullOrEmpty(ClientName);

        public string Color { get; }

        public DurationFormat DurationFormat { get; set; }

        public ChartSegment(
            string projectName,
            string clientName,
            float percentage,
            float trackedSeconds,
            float billableSeconds,
            string color,
            DurationFormat durationFormat = DurationFormat.Improved)
        {
            ProjectName = projectName;
            ClientName = clientName;
            Color = color;
            Percentage = percentage;
            TrackedTime = TimeSpan.FromSeconds(trackedSeconds);
            BillableSeconds = billableSeconds;
            DurationFormat = durationFormat;
        }
    }

    public static class ChartSegmentExtensions
    {
        private const int maxSegmentNameLength = 18;

        public static ChartSegment WithDurationFormat(this ChartSegment segment, DurationFormat durationFormat)
            => new ChartSegment(
                segment.ProjectName,
                segment.ClientName,
                segment.Percentage,
                (float)segment.TrackedTime.TotalSeconds,
                segment.BillableSeconds,
                segment.Color,
                durationFormat);

        public static string FormattedName(this ChartSegment segment)
            => segment.ProjectName.TruncatedAt(maxSegmentNameLength);
    }
}
