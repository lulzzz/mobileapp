using Android.Graphics;
using Android.Support.V4.Graphics;

namespace Toggl.Giskard.Autocomplete
{
    public sealed class ProjectTokenSpan : TokenSpan
    {
        public ProjectTokenSpan(Color projectColor)
            : base(Color.White, projectColor, false)
        {
        }
    }
}
