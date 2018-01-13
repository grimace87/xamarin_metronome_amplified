
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using MetronomeAmplified.Classes;
using System;

namespace MetronomeAmplified
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SongSelectPage : ContentPage
    {
        private SongPage ParentPage;
        public ObservableCollection<SongFile> SongFiles { get; set; }

        public SongSelectPage(SongPage parentPage, string[] songFiles)
        {
            InitializeComponent();

            // Don't show the navigation action bar
            NavigationPage.SetHasNavigationBar(this, false);
            
            ParentPage = parentPage;
            ObservableCollection<SongFile> files = new ObservableCollection<SongFile>();
            foreach (string s in songFiles)
                files.Add(new SongFile(s));
            this.SongFiles = files;
            BindingContext = this;
        }

        private async void LoadSong(object sender, EventArgs e)
        {
            // Return to song page and load the thing
            SongFile item = (SongFile)listSongs.SelectedItem;
            if (item == null) return;
            ParentPage.LoadSong(item.GetName);
            await Navigation.PopAsync();
        }

        private async void Cancel(object sender, EventArgs e)
        {
            // Return to song page without doing anything
            await Navigation.PopAsync();
        }

        private async void DeleteSong(object sender, EventArgs e)
        {
            // Get song item
            SongFile item = (SongFile)listSongs.SelectedItem;
            if (item == null) return;

            // Confirm deletion
            bool okay = await DisplayAlert("Delete file?", "File '" + item.GetName + "' will be deleted permanently.", "Okay", "Cancel");
            if (!okay) return;

            // Delete file (list view should update)
            bool success = await ParentPage.DeleteSong(item.GetName);
            if (success)
            {
                SongFiles.Remove(item);
                await DisplayAlert("Message", "Song deleted.", "OK");
            }
            else
                await DisplayAlert("Issue", "Song could not be deleted.", "Oh, ok...");

        }
    }
}