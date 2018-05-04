using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using Toggl.Foundation.MvvmCross.Parameters;
using Toggl.Multivac;
using Toggl.Multivac.Models;
using Toggl.Multivac.Extensions;
using static Toggl.Multivac.Extensions.StringExtensions;
using Toggl.Ultrawave;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class SelectCountryViewModel : MvxViewModel<SelectCountryParameter, ICountry>
    {
        private readonly ITogglApi togglApi;
        private readonly IMvxNavigationService navigationService;

        private IEnumerable<ICountry> allCountries;
        private ICountry selectedCountry;

        public IMvxAsyncCommand CloseCommand { get; }

        public IMvxAsyncCommand<ICountry> SelectCountryCommand { get; }

        public MvxObservableCollection<SelectableCountryViewModel> Suggestions { get; }
            = new MvxObservableCollection<SelectableCountryViewModel>();

        public string Text { get; set; } = "";

        public SelectCountryViewModel(ITogglApi togglApi, IMvxNavigationService navigationService)
        {
            Ensure.Argument.IsNotNull(togglApi, nameof(togglApi));
            Ensure.Argument.IsNotNull(navigationService, nameof(navigationService));

            this.togglApi = togglApi;
            this.navigationService = navigationService;

            CloseCommand = new MvxAsyncCommand(close);
            SelectCountryCommand = new MvxAsyncCommand<ICountry>(selectCountry);
        }

        public override async Task Initialize()
        {
            await base.Initialize();

            allCountries = await togglApi.Countries.GetAll();

            Suggestions.AddRange(allCountries.Select(country => new SelectableCountryViewModel(country, false)));
        }

        public override void Prepare(SelectCountryParameter parameter)
        {
            selectedCountry = parameter.SelectedCountry;
        }

        private void OnTextChanged()
        {
            Suggestions.Clear();
            var text = Text.Trim();
            Suggestions.AddRange(
                allCountries
                    .Where(c => c.Name.ContainsIgnoringCase(text))
                    .Select(c => new SelectableCountryViewModel(c, c.CountryCode == selectedCountry.CountryCode))
            );
        }

        private Task close()
            => navigationService.Close<ICountry>(this, null);

        private async Task selectCountry(ICountry country)
            => await navigationService.Close(this, country);
    }
}
