using Android.Graphics;
using MvvmCross.Plugins.Color.Droid;
using FoundationColor = Toggl.Foundation.MvvmCross.Helper.Color;

namespace Toggl.Giskard.Autocomplete
{
    public sealed class TagsTokenSpan : TokenSpan
    {
        public int TagIndex { get; }

        public TagsTokenSpan(int tagIndex)
            : base(Color.White, Color.White, true)
        {
            TagIndex = tagIndex;
        }
    }
}
