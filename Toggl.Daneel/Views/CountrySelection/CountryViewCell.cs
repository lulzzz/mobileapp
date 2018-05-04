using System;
using Foundation;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Binding.iOS.Views;
using UIKit;

namespace Toggl.Daneel.Views.CountrySelection
{
    public partial class CountryViewCell : MvxTableViewCell
    {
        public static readonly NSString Key = new NSString(nameof(CountryViewCell));
        public static readonly UINib Nib;

        static CountryViewCell()
        {
            Nib = UINib.FromName(nameof(CountryViewCell), NSBundle.MainBundle);
        }

        protected CountryViewCell(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            this.DelayBind(() =>
            {
                var bindingSet = this.CreateBindingSet<CountryViewCell, string>();

                bindingSet.Bind(NameLabel).To(vm => vm);

                bindingSet.Apply();
            });
        }
    }
}
