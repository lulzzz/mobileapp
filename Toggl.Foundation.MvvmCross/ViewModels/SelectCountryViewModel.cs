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
using Toggl.Foundation.Interactors;
using System.Diagnostics.Contracts;
using System;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class SelectCountryViewModel : MvxViewModel<SelectCountryParameter, SelectCountryParameter>
    {
        private readonly IMvxNavigationService navigationService;
        private readonly IInteractorFactory interactorFactory;

        private IEnumerable<ICountry> allCountries;
        private string selectedCountryCode;

        public IMvxAsyncCommand CloseCommand { get; }

        public IMvxAsyncCommand<SelectableCountryViewModel> SelectCountryCommand { get; }

        public MvxObservableCollection<SelectableCountryViewModel> Suggestions { get; }
            = new MvxObservableCollection<SelectableCountryViewModel>();

        public string Text { get; set; } = "";

        public SelectCountryViewModel(IInteractorFactory interactorFactory, IMvxNavigationService navigationService)
        {
            Ensure.Argument.IsNotNull(interactorFactory, nameof(interactorFactory));
            Ensure.Argument.IsNotNull(navigationService, nameof(navigationService));

            this.interactorFactory = interactorFactory;
            this.navigationService = navigationService;

            CloseCommand = new MvxAsyncCommand(close);
            SelectCountryCommand = new MvxAsyncCommand<SelectableCountryViewModel>(selectCountry);
        }

        public override async Task Initialize()
        {
            Contract.Ensures(Contract.Result<Task>() != null);
            await base.Initialize();

            var countries = await interactorFactory.GetAllCountries().Execute();
            countries = countries.ToList<ICountry>();
         
            var selectedElement = countries.Find(c => c.CountryCode == selectedCountryCode);
            
            countries.Remove(selectedElement);
            countries.Insert(0, selectedElement);

            allCountries = countries.AsEnumerable<ICountry>();
            
            Suggestions.AddRange(allCountries.Select(country => new SelectableCountryViewModel(country, country.CountryCode == selectedCountryCode)));
        }

        public override void Prepare(SelectCountryParameter parameter)
        {
            selectedCountryCode = parameter.SelectedCountryCode;
        }

        private void OnTextChanged()
        {
            Suggestions.Clear();
            var text = Text.Trim();
            Suggestions.AddRange(
                allCountries
                    .Where(c => c.Name.ContainsIgnoringCase(text))
                    .Select(c => new SelectableCountryViewModel(c, c.CountryCode == selectedCountryCode))
            );
        }

        private Task close()
            => navigationService.Close<SelectCountryParameter>(this, null);

        private async Task selectCountry(SelectableCountryViewModel vm)
            => await navigationService.Close(this, SelectCountryParameter.With(vm.Country.Name, vm.Country.CountryCode));
    }
}
