
using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MetronomeAmplified
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NameUpdatePage : ContentPage
    {
        private SongPage ParentPage;
        public NameUpdatePage(SongPage page)
        {
            InitializeComponent();
            ParentPage = page;
        }
        public async void SaveName(object sender, EventArgs e)
        {
            string input = entryName.Text;
            if (input == null || input.Length == 0)
                await DisplayAlert("Issue", "Please enter a new title, or cancel.", "OK");
            else
            {
                ParentPage.SongName = input;
                ParentPage.SongHasUnsavedChanges = true;
                await Navigation.PopModalAsync();
            }
        }
        public async void Return(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}