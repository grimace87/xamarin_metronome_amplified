
/*
 * SettingsPage
 * Settings page, which receives a reference to the Settings object from the main page, and saves changes when returning
 * 
 * Created 26/09/2017 by Thomas Reichert
 *
 */

using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using MetronomeAmplified.Classes;

namespace MetronomeAmplified
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPage : ContentPage
    {
        // The settings object passed by the caller
        private SettingsObject settings;
        private SettingsObject localSettings;

        // The storage object where settings will be saved
        IPlatformStorage platformStorage;

        // The strings where the lift type are mentioned
        private static string[] LiftTypeStrings = new string[] { "Continuously", "After each song repetition" };
        public string[] GetLiftTypeStrings { get { return LiftTypeStrings; } }

        // The string that go and describe those colours that the user gonna do and choose for
        private const string COLOUR1 = "Cyan", COLOUR2 = "Pink", COLOUR3 = "Yellow", COLOUR4 = "Lime", COLOUR5 = "Orange", COLOUR6 = "Black";
        private static readonly string[] ColourNames = new string[] { COLOUR1, COLOUR2, COLOUR3, COLOUR4, COLOUR5, COLOUR6 };

        // Standard constructor, receiving the settings object to modify if the save button is pressed
        public SettingsPage(SettingsObject _settings, IPlatformStorage _platformStorage)
        {
            // Don't show the navigation action bar
            NavigationPage.SetHasNavigationBar(this, false);

            // Stash settings objects locally and initialise the layout stuff
            settings = _settings;
            platformStorage = _platformStorage;
            InitializeComponent();
            BindingContext = this;

            // Make a local copy of the settings object
            localSettings = new SettingsObject(null);
            localSettings.FlashCues = settings.FlashCues;
            localSettings.FlashColor = new Color(
                settings.FlashColor.R,
                settings.FlashColor.G,
                settings.FlashColor.B
            );
            localSettings.OneTapGain = settings.OneTapGain;
            localSettings.OneTapDuration = settings.OneTapDuration;

            // Set what the sliders do
            sliderBPMGain.PropertyChanged += SliderBPMGain_PropertyChanged;
            sliderDuration.PropertyChanged += SliderDuration_PropertyChanged;

            // Initialise controls
            sliderBPMGain.Value = localSettings.OneTapGain * 100.0f;
            sliderDuration.Value = localSettings.OneTapDuration / 6.0f;
            updateCuesDisplay();
            updateSliderBPMGainDisplay(localSettings.OneTapGain);
            updateSliderBPMDurationDisplay(localSettings.OneTapDuration);

            // Initialise more controls
            if (settings.OneTapType == PlaybackObject.TempoLiftType.LiftGradual)
                pickLiftType.SelectedIndex = 0;
            else
                pickLiftType.SelectedIndex = 1;

        }

        private void updateCuesDisplay()
        {
            if (localSettings.FlashCues)
            {
                labelCuesToggle.Text = "On";
                layoutCueColor.IsVisible = true;
                gridCues.RowDefinitions[2].Height = 2;
                gridCues.RowDefinitions[3].Height = GridLength.Auto;
                Color color;
                for (int i = 0, len = ColourNames.Length; i < len; i++) {
                    color = GetColour(ColourNames[i]);
                    if (color.R == localSettings.FlashColor.R && color.G == localSettings.FlashColor.G && color.B == localSettings.FlashColor.B)
                    {
                        labelCueColor.Text = ColourNames[i];
                        return;
                    }
                }
                labelCueColor.Text = "(Custom)";
            }
            else
            {
                labelCuesToggle.Text = "Off";
                layoutCueColor.IsVisible = false;
                gridCues.RowDefinitions[2].Height = 0;
                gridCues.RowDefinitions[3].Height = 0;
            }
        }

        private void updateSliderBPMGainDisplay(float gain)
        {
            // Update the display
            labelBPMGain.Text = String.Format("{0:F0}%", gain * 100.0f);
        }

        private void updateSliderBPMDurationDisplay(float dur)
        {
            // Update the display
            int minutes = (int)dur;
            int seconds = minutes % 60;
            minutes = minutes / 60;
            labelDuration.Text = String.Format("{0} min, {1} sec", minutes, seconds);
        }

        private void SliderDuration_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // Get the value and store it locally
            float dur = (float)(sliderDuration.Value * 6.0f);
            localSettings.OneTapDuration = dur;

            // Update the display
            updateSliderBPMDurationDisplay(dur);
            
        }

        private void SliderBPMGain_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // Get the value and store it locally
            float gain = 0.01f * (float)sliderBPMGain.Value;
            localSettings.OneTapGain = gain;

            // Update the display
            updateSliderBPMGainDisplay(gain);
        }

        public void ToggleCues(object sender, EventArgs e)
        {
            localSettings.FlashCues = !localSettings.FlashCues;
            updateCuesDisplay();
        }

        public async void ChangeCueColor(object sender, EventArgs e)
        {
            if (localSettings.FlashCues)
            {
                // Do the stuff here
                string action = await DisplayActionSheet("Flash Color", "Cancel", null, ColourNames);
                if (action == "Cancel")
                    return;
                localSettings.FlashColor = GetColour(action);
                updateCuesDisplay();
            }
        }

        public void OpenLiftTypePicker()
        {
            pickLiftType.Focus();
        }

        private Color GetColour(string name)
        {
            switch(name)
            {
                case COLOUR1:
                    return new Color(0.0, 0.8, 0.8);
                case COLOUR2:
                    return new Color(1.0, 0.61, 0.97);
                case COLOUR3:
                    return new Color(1.0, 1.0, 0.6);
                case COLOUR4:
                    return new Color(0.8, 1.0, 0.6);
                case COLOUR5:
                    return new Color(1.0, 0.8, 0.6);
                case COLOUR6:
                    return new Color(0.0, 0.0, 0.0);
                default:
                    return new Color(0.5, 0.5, 0.5);
            }
        }

        private async void SaveAndReturn(object sender, EventArgs e)
        {
            // Save settings to the object
            settings.FlashCues = localSettings.FlashCues;
            settings.FlashColor = localSettings.FlashColor;
            settings.OneTapGain = localSettings.OneTapGain;
            settings.OneTapDuration = localSettings.OneTapDuration;
            settings.OneTapType = pickLiftType.SelectedIndex == 0
                ? PlaybackObject.TempoLiftType.LiftGradual
                : PlaybackObject.TempoLiftType.LiftPerRep;

            // Save to platform-specific storage
            platformStorage.StoreBoolean(SettingsObject.KEY_CUES, settings.FlashCues);
            platformStorage.StoreFloat(SettingsObject.KEY_COLOR_R, (float)settings.FlashColor.R);
            platformStorage.StoreFloat(SettingsObject.KEY_COLOR_G, (float)settings.FlashColor.G);
            platformStorage.StoreFloat(SettingsObject.KEY_COLOR_B, (float)settings.FlashColor.B);
            platformStorage.StoreFloat(SettingsObject.KEY_BPM_GAIN, settings.OneTapGain);
            platformStorage.StoreFloat(SettingsObject.KEY_BPM_DURATION, settings.OneTapDuration);
            platformStorage.StoreInt(SettingsObject.KEY_LIFT_TYPE, (int)settings.OneTapType);

            // Return to main page
            await Navigation.PopAsync();
        }

        private async void CancelAndReturn(object sender, EventArgs e)
        {
            // Return to main page without saving to the settings object
            await Navigation.PopAsync();
        }

        private void SetDefaults(object sender, EventArgs e)
        {
            // Adjust settings and reset sliders
            localSettings = new SettingsObject(null);
            updateCuesDisplay();
            updateSliderBPMGainDisplay(localSettings.OneTapGain);
            updateSliderBPMDurationDisplay(localSettings.OneTapDuration);
            sliderBPMGain.Value = 20;
            sliderDuration.Value = 30;
            pickLiftType.SelectedIndex = (int)localSettings.OneTapType;
            AdjustPicker(null, null);
        }

        private void AdjustPicker(object sender, EventArgs e)
        {
            localSettings.OneTapType = (PlaybackObject.TempoLiftType)pickLiftType.SelectedIndex;
            if (localSettings.OneTapType == PlaybackObject.TempoLiftType.LiftGradual)
            {
                layoutDuration.IsVisible = true;
                gridTempoLift.RowDefinitions[4].Height = 2;
                gridTempoLift.RowDefinitions[5].Height = GridLength.Auto;
            }
            else
            {
                layoutDuration.IsVisible = false;
                gridTempoLift.RowDefinitions[4].Height = 0;
                gridTempoLift.RowDefinitions[5].Height = 0;
            }
        }

    }
}