using MvvmCross.Binding.BindingContext;
using Toggl.Daneel.ViewSources;
using Toggl.Foundation.MvvmCross.Helper;
using Toggl.Foundation.MvvmCross.ViewModels;
using UIKit;

namespace Toggl.Daneel.ViewControllers
{
    public partial class SelectCountryViewController : KeyboardAwareViewController<SelectCountryViewModel>
    {
        public SelectCountryViewController() : base(nameof(SelectCountryViewController))
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var source = new CountryTableViewSource(CountriesTableView);
            CountriesTableView.Source = source;

            var bindingSet = this.CreateBindingSet<SelectCountryViewController, SelectCountryViewModel>();

            bindingSet.Bind(source).To(vm => vm.Suggestions);
            bindingSet.Bind(SearchTextField).To(vm => vm.Text);
            bindingSet.Bind(CloseButton).To(vm => vm.CloseCommand);
            bindingSet.Bind(source)
                      .For(v => v.SelectionChangedCommand)
                      .To(vm => vm.SelectCountryCommand);

            bindingSet.Bind(source)
                      .For(v => v.Text)
                      .To(vm => vm.Text);

            bindingSet.Apply();

            SearchTextField.BecomeFirstResponder();
        }

        protected override void KeyboardWillShow(object sender, UIKeyboardEventArgs e)
        {
            BottomConstraint.Constant = e.FrameEnd.Height;
            UIView.Animate(Animation.Timings.EnterTiming, () => View.LayoutIfNeeded());
        }

        protected override void KeyboardWillHide(object sender, UIKeyboardEventArgs e)
        {
            BottomConstraint.Constant = 0;
            UIView.Animate(Animation.Timings.EnterTiming, () => View.LayoutIfNeeded());
        }
    }
}

