
using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MetronomeAmplified
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SectionNameUpdatePage : ContentPage
    {
        private SectionPage ParentPage;
        public SectionNameUpdatePage(SectionPage page)
        {
            // Don't show the navigation action bar
            NavigationPage.SetHasNavigationBar(this, false);

            // Initialise Xamarin... Forms... layout... XAML... uh... thing?
            InitializeComponent();

            // And I want this too so the name can be saved back into a property of that page
            ParentPage = page;
        }
        public async void SaveName(object sender, EventArgs e)
        {
            string input = entryName.Text;
            if (input == null || input.Length == 0)
                await DisplayAlert("Issue", "Please enter a new name, or cancel.", "OK");
            else
            {
                ParentPage.SectionName = input;
                await Navigation.PopModalAsync();
            }
        }
        public async void Return(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}