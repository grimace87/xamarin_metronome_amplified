
using MetronomeAmplified.Classes;
using System;
using System.ComponentModel;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MetronomeAmplified
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TimerStartPage : ContentPage
    {
        private double Time = 300.0;
        private PlaybackObject Play;
        private MainPage ParentPage;
        public TimerStartPage(PlaybackObject play, MainPage page)
        {
            // Don't show the navigation action bar
            NavigationPage.SetHasNavigationBar(this, false);

            // Get stuff
            Play = play;
            ParentPage = page;

            // Set up the UI stuff
            InitializeComponent();
            sliderSessionTime.PropertyChanged += SessionSlider_Changed;
            updateSliderDisplay(300.0);
        }

        private void SessionSlider_Changed(object sender, PropertyChangedEventArgs e)
        {
            // Get the value and store it locally
            Time = 12.0 * sliderSessionTime.Value;

            // Update the display
            updateSliderDisplay(Time);
        }
        private void updateSliderDisplay(double duration)
        {
            // Update the display
            int minutes = (int)duration / 60;
            int seconds = (int)duration % 60;
            labelSessionTime.Text = String.Format("{0} min, {1} sec", minutes, seconds);
        }

        private async void SaveAndStart(object sender, EventArgs e)
        {
            // Save settings to the object
            Play.StartSession(Time);
            if (Play.IsPlaying == false)
                ParentPage.ActionPlay();
            
            // Return to main page
            await Navigation.PopModalAsync();
        }
        private async void CancelAndReturn(object sender, EventArgs e)
        {
            // Return to main page without saving to the settings object
            await Navigation.PopModalAsync();
        }
    }
}