using Toggl.Multivac.Models;

namespace Toggl.Foundation.MvvmCross.Parameters
{
    public sealed class SelectCountryParameter
    {
        public string SelectedCountryName { get; set; }
        public string SelectedCountryCode { get; set; }

        public static SelectCountryParameter With(string selectedCountryName, string selectedCountryCode)
            => new SelectCountryParameter
            {
                SelectedCountryName = selectedCountryName,
                SelectedCountryCode = selectedCountryCode
            };
    }
}
