using Toggl.Multivac.Models;

namespace Toggl.Foundation.MvvmCross.Parameters
{
    public sealed class SelectCountryParameter
    {
        public long SelectedCountryId { get; set; }

        public static SelectCountryParameter WithCountryCode(long? selectedCountryId)
            => new SelectCountryParameter
            {
                SelectedCountryId = selectedCountryId ?? 0
            };
    }
}
