﻿using System;
using System.Collections.Generic;
using System.Linq;
using Toggl.Foundation;
using static Toggl.Foundation.Helper.Constants;

namespace Toggl.Foundation.MvvmCross.Parameters
{
    public sealed class SelectTimeParameters
    {
        public DateTimeOffset Start { get; private set; }
        public DateTimeOffset? Stop { get; private set; }

        public int StartingTabIndex { get; private set; }
        public bool ShouldStartOnCalendar { get; private set; }

        public static SelectTimeParameters CreateFromBindingString(string bindingString, DateTimeOffset start, DateTimeOffset? stop = null)
        {
            var allowedParameters = new Dictionary<string, (int, bool)>
            {
                ["StartTime"] = (0, false),
                ["StartDate"] = (0, true),
                ["StopTime"] = (1, false),
                ["StopDate"] = (1, true),
                ["Duration"] = (2, false)
            };

            bindingString = bindingString.Trim();

            if (!allowedParameters.Keys.Contains(bindingString))
                throw new ArgumentException("SelectTimeCommand binding must have one of the following (case sensitive) parameters: StartTimeClock|StartTimeCalendar|StopTimeClock|StopTimeCalendar|Duration");

            var (tabIndex, shouldStartOnCalendar) = allowedParameters[bindingString];

            return new SelectTimeParameters
            {
                Start = start,
                Stop = stop,
                StartingTabIndex = tabIndex,
                ShouldStartOnCalendar = shouldStartOnCalendar
            };
        }
    }
}
