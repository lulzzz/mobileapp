using System;

namespace Toggl.Foundation.MvvmCross.Parameters
{
    public sealed class SelectTimeResultsParameters
    {
        public DateTimeOffset Start { get; }
        public DateTimeOffset? Stop { get; }

        public SelectTimeResultsParameters(DateTimeOffset start, DateTimeOffset? stop)
        {
            Start = start;
            Stop = stop;
        }
    }
}
