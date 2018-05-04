using Toggl.Multivac.Models;

namespace Toggl.Foundation.MvvmCross.Parameters
{
    public sealed class SelectCountryParameter
    {
        public ICountry SelectedCountry { get; set; }

        public static SelectCountryParameter WithIds(ICountry selectedCountry)
            => new SelectCountryParameter
            {
                SelectedCountry = selectedCountry
            };
    }
}
