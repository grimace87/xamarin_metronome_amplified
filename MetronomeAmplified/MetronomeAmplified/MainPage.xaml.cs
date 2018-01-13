
/*
 * MainPage
 * Home screen of the Metronome, with the basic interface and the links to all of the settings pages
 * 
 * Created 25/09/2017 by Thomas Reichert
 *
 */

using System;

using Xamarin.Forms;

using MetronomeAmplified.Classes;

namespace MetronomeAmplified
{
    public partial class MainPage : ContentPage
    {
        // Settings and state and such
        private SettingsObject Settings;
        public PlaybackObject Playback;
        public IPlatformStorage PlatformStorage;
        private IPlatformAudioPlayer AudioPlayer;

        // The current song
        private Song song;

        // The timers
        private Timer timer;
        private bool flashTimerRunning;
        
        // The last system timer reading when the 'TAP' button was last pressed
        private Int32 LastTapTime;

        // The list of tone sets
        private string[] ToneStrings = { "Basic Drum Kit", "Wooden Blocks" };

        // Reference to the BoxView in the section display
        private BoxView SectionIndicatorBox;

        // Indicate that seeking was done and so the current note should be played without incrementing first
        private bool SeekWasUsed = false;

        // Flags to stop grid adjustments being made too frequently
        private bool LastProgressSection, LastProgressSong, LastProgressSession;

        public MainPage(IPlatformStorage platformStorage, IPlatformAudioPlayer audioPlayer)
        {
            // Initialise layout, and don't show the navigation action bar
            NavigationPage.SetHasNavigationBar(this, false);
            InitializeComponent();

            // Stash the platform storage and audio interface object locally
            PlatformStorage = platformStorage;
            AudioPlayer = audioPlayer;

            // Create the settings and playback objects
            Settings = new SettingsObject(platformStorage);
            Playback = new PlaybackObject();

            // Create a new default song
            song = new Song();

            // Update the section display
            this.SizeChanged += (sender, e) => {

                // Get content window sizes
                double width = this.Width;
                double height = this.Height;

                // Size according to orientation
                double ctrlSize = width > height ? 0.15 * (this.Height - 40) : 0.25 * (this.Width - 40);
                BottomCtrlRow.Height = ctrlSize;
                CtrlColumn1.Width = ctrlSize;
                CtrlColumn2.Width = ctrlSize;
                ctrlSize = width > height ? 0.15 * (this.Height + 20) : 0.25 * (this.Width + 20);
                PlaybackCtrlPlayStop.WidthRequest = ctrlSize * 0.5;
                PlaybackCtrlPlayStop.HeightRequest = ctrlSize * 0.5;
                PlaybackCtrlRewind.WidthRequest = ctrlSize * 0.3;
                PlaybackCtrlRewind.HeightRequest = ctrlSize * 0.3;
                PlaybackCtrlForward.WidthRequest = ctrlSize * 0.3;
                PlaybackCtrlForward.HeightRequest = ctrlSize * 0.3;
                PlaybackCtrlPause.WidthRequest = ctrlSize * 0.3;
                PlaybackCtrlPause.HeightRequest = ctrlSize * 0.3;

                // Populate the song display
                UpdateDisplay();


            };
            
            LastProgressSection = false;
            LastProgressSong = false;
            LastProgressSession = false;

            // Initialise the timer-type stuff
            timer = new Timer(TimerElapsed, 1000);
            TempoSliderChanged(null, null);
            flashTimerRunning = false;
            LastTapTime = -1;
            
        }

        protected async override void OnAppearing()
        {
            if (song.Name == null)
                song.Name = await PlatformStorage.GetNewSongName();
            AudioPlayer.Init();
            base.OnAppearing();
        }

        public void UpdateDisplay()
        {
            SectionIndicatorBox = song.GetSection(Playback.CurrentSection).PopulateSongLayout(null, layoutSectionDisplay, false);
        }
        
        // UI navigation button handlers
        public async void LoadPageTone(object sender, EventArgs e)
        {
            string choice = await DisplayActionSheet("Select a tone set.", "Cancel", null, ToneStrings);
            if (choice == "Cancel")
                return;
            if (choice == ToneStrings[0])
                AudioPlayer.LoadTracks(new string[] { "hat.wav", "kick.wav", "tom.wav", "snare.wav" });
            else if (choice == ToneStrings[1])
                AudioPlayer.LoadTracks(new string[] { "wood_gentle.wav", "wood_mid.wav", "wood_high.wav", "wood_low.wav" });
        }
        private async void LoadPageSong(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SongPage(this, song));
        }
        private void LoadPageHelp(object sender, EventArgs e)
        {
            DisplayAlert("Message", "I should probably load the help page now.", "But, nah...");
        }
        private async void LoadPageSettings(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SettingsPage(Settings, PlatformStorage));
        }

        // Functions to update progress meters
        public void UpdateProgressSection()
        {
            if (Playback.IsPlaying && (song.Sections[Playback.CurrentSection].Repetitions > 1))
            {
                if (LastProgressSection == false)
                {
                    bgSection.IsVisible = true;
                    boxProgressSection.IsVisible = true;
                    gridMain.RowDefinitions[5].Height = GridLength.Auto;
                }
                Rectangle rect = AbsoluteLayout.GetLayoutBounds(boxProgressSection);
                rect.Width = ((double)Playback.CurrentRep) / ((double)song.Sections[Playback.CurrentSection].Repetitions);
                AbsoluteLayout.SetLayoutBounds(boxProgressSection, rect);
                LastProgressSection = true;
            }
            else
            {
                if (LastProgressSection)
                {
                    bgSection.IsVisible = false;
                    boxProgressSection.IsVisible = false;
                    gridMain.RowDefinitions[5].Height = 0;
                }
                LastProgressSection = false;
            }
        }
        public void UpdateProgressSong()
        {
            if (Playback.IsPlaying && song.NumberOfSections > 1)
            {
                if (LastProgressSong == false)
                {
                    bgSong.IsVisible = true;
                    boxProgressSong.IsVisible = true;
                    gridMain.RowDefinitions[6].Height = GridLength.Auto;
                }
                Rectangle rect = AbsoluteLayout.GetLayoutBounds(boxProgressSong);
                rect.Width = ((double)Playback.CurrentSection) / ((double)song.NumberOfSections);
                AbsoluteLayout.SetLayoutBounds(boxProgressSong, rect);
                LastProgressSong = true;
            }
            else
            {
                if (LastProgressSong)
                {
                    bgSong.IsVisible = false;
                    boxProgressSong.IsVisible = false;
                    gridMain.RowDefinitions[6].Height = 0;
                }
                LastProgressSong = false;
            }
        }
        public void UpdateProgressSession()
        {
            if (Playback.IsPlaying && Playback.IsSession)
            {
                if (LastProgressSession == false)
                {
                    bgSession.IsVisible = true;
                    boxProgressSession.IsVisible = true;
                    gridMain.RowDefinitions[7].Height = GridLength.Auto;
                }
                Rectangle rect = AbsoluteLayout.GetLayoutBounds(boxProgressSession);
                rect.Width = Playback.SessionTime / Playback.SessionLimit;
                AbsoluteLayout.SetLayoutBounds(boxProgressSession, rect);
                LastProgressSession = true;
            }
            else
            {
                if (LastProgressSession)
                {
                    bgSession.IsVisible = false;
                    boxProgressSession.IsVisible = false;
                    gridMain.RowDefinitions[7].Height = 0;
                }
                LastProgressSession = false;
            }
        }

        // UI control button handlers
        private void PressTap(object sender, EventArgs e)
        {
            Int32 reading = System.Environment.TickCount;
            Int32 diff = reading - LastTapTime;
            if (LastTapTime >= 0 && diff < 2000)
                SetTempo(60000.0 / diff);
            LastTapTime = reading;
        }
        private void PressLift(object sender, EventArgs e)
        {
            if (Playback.IsLifting)
                Playback.IsLifting = false;
            else
            {
                if (Playback.IsPlaying == false)
                    ActionPlay();
                if (Settings.OneTapType == PlaybackObject.TempoLiftType.LiftGradual)
                    Playback.StartGradualLift(Playback.TempoGain * song.Sections[Playback.CurrentSection].Tempo, Settings.OneTapGain, Settings.OneTapDuration);
                else
                    Playback.StartIncremetalLift(Playback.TempoGain * song.Sections[Playback.CurrentSection].Tempo, Settings.OneTapGain);
            }
        }
        private async void PressTimer(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new TimerStartPage(Playback, this));
        }

        private void ResetGain(object sender, EventArgs e)
        {
            Playback.TempoGain = 1.0;
            Playback.IsLifting = false;
            RefreshTempo();
        }
        public void RefreshTempo()
        {
            SetTempo(song.Sections[Playback.CurrentSection].Tempo * Playback.TempoGain);
        }
        private void SetTempo(double tempo)
        {
            sliderTempo.Value = (tempo - 44.0) / 2.56;
            TempoSliderChanged(null, null);
        }
        private void TempoSliderChanged(object sender, EventArgs e)
        {
            // Adjust playback tempo gain
            double targetTempo = sliderTempo.Value * 2.56 + 44.0;
            displayTempo.Text = String.Format("{0:0}", targetTempo);
            Playback.TempoGain = targetTempo / song.GetSection(Playback.CurrentSection).Tempo;
        }

        // Press the play/pause button and the other buttons
        private void TapPlayButton(object sender, EventArgs e)
        {
            if (Playback.IsPlaying)
                ActionStop();
            else
                ActionPlay();
        }
        private void TapPauseButton(object sender, EventArgs e)
        {
            if (Playback.IsPlaying)
                ActionPause();
        }
        private void TapRewind(object sender, EventArgs e)
        {
            Playback.CurrentNote = 0;
            Playback.CurrentRep = 0;
            SeekWasUsed = true;
            if (Playback.CurrentSection > 0)
            {
                Playback.CurrentSection--;
                UpdateDisplay();
                UpdateProgressSection();
                UpdateProgressSong();
            }
            else if (song.Sections[Playback.CurrentSection].Repetitions > 0)
                UpdateProgressSong();
        }
        private void TapFastForward(object sender, EventArgs e)
        {
            Playback.CurrentNote = 0;
            Playback.CurrentRep = 0;
            SeekWasUsed = true;
            if (Playback.CurrentSection < song.Sections.Count - 1)
            {
                Playback.CurrentSection++;
                UpdateDisplay();
                UpdateProgressSection();
                UpdateProgressSong();
            }
            else if (song.Sections[Playback.CurrentSection].Repetitions > 0)
                UpdateProgressSong();
        }
        public void ActionPlay()
        {
            // Set the state to playing
            PlaybackCtrlPlayStop.Source = "stopbutton.png";
            Playback.IsPlaying = true;

            // Adjust the display
            SectionIndicatorBox.IsVisible = true;
            
            // Start the timer counting, and fire the event once
            SetTimerInterval();
            timer.Run(true);

            // Update progress meters
            UpdateProgressSection();
            UpdateProgressSong();
            UpdateProgressSession();
        }
        private void ActionPause()
        {
            Playback.IsPlaying = false;
            timer.Stop();
            AudioPlayer.StopAudio();
            PlaybackCtrlPlayStop.Source = "playbutton.png";
            SectionIndicatorBox.IsVisible = false;
        }
        private void ActionStop()
        {
            Playback.IsPlaying = false;
            timer.Stop();
            AudioPlayer.StopAudio();
            PlaybackCtrlPlayStop.Source = "playbutton.png";
            SectionIndicatorBox.IsVisible = false;

            if (Playback.CurrentSection > 0)
            {
                Playback.CurrentSection = 0;
                UpdateDisplay();
            }
            else
                Playback.CurrentSection = 0;

            Playback.CurrentRep = 0;
            Playback.CurrentNote = -1;
            SeekWasUsed = false;
            Playback.IsSession = false;
            Playback.IsLifting = false;
            UpdateProgressSection();
            UpdateProgressSong();
            UpdateProgressSession();
        }

        private void TimerElapsed(object sender, EventArgs e)
        {
            bool mustUpdateSectionDisplay = false;
            // Seek to the next note and update the timer A.S.A.P.
            if (SeekWasUsed) SeekWasUsed = false;
            else
                Playback.CurrentNote++;
            if (Playback.CurrentNote >= song.GetSection(Playback.CurrentSection).Sequence.Count)
            {
                Playback.CurrentNote = 0;
                Playback.CurrentRep++;
                if (Playback.CurrentRep >= song.GetSection(Playback.CurrentSection).Repetitions)
                {
                    Playback.CurrentRep = 0;
                    if (song.NumberOfSections > 1)
                    {
                        mustUpdateSectionDisplay = true;
                        Playback.CurrentSection++;
                        if (Playback.CurrentSection >= song.NumberOfSections)
                        {
                            Playback.CurrentSection = 0;
                            if (Playback.IsLifting && Playback.LiftType == PlaybackObject.TempoLiftType.LiftPerRep)
                                SetTempo(Playback.RunLiftAfterRep());
                        }
                        UpdateProgressSong();
                        RefreshTempo();
                    }
                    else if (Playback.IsLifting && Playback.LiftType == PlaybackObject.TempoLiftType.LiftPerRep)
                        SetTempo(Playback.RunLiftAfterRep());
                }
                UpdateProgressSection();
            }
            if (Playback.IsLifting && Playback.LiftType == PlaybackObject.TempoLiftType.LiftGradual)
                SetTempo(Playback.RunGradualLift(timer.Duration));
            if (Playback.IsSession)
                if (Playback.RunSession(timer.Duration) == false)
                    ActionStop();
            UpdateProgressSession();
            if (Playback.IsPlaying == false)
            {
                timer.Stop();
                return;
            }

            // Set the next interval for the timer
            SetTimerInterval();

            // Trigger the sound playing again
            Note note = song.GetSection(Playback.CurrentSection).Sequence[Playback.CurrentNote];
            if (note.IsSound && note.IsTieExtension == false)
                AudioPlayer.PlaySound(note.Accent);
            if (Settings.FlashCues && !flashTimerRunning)
            {
                flashTimerRunning = true;
                displayTempo.BackgroundColor = Settings.FlashColor;
                Device.StartTimer(TimeSpan.FromMilliseconds(50), () => { displayTempo.BackgroundColor = Color.White; flashTimerRunning = false; return false; });
            }

            // Update whatever needs to be displayed on the note display
            if (mustUpdateSectionDisplay)
                SectionIndicatorBox = song.GetSection(Playback.CurrentSection).PopulateSongLayout(null, layoutSectionDisplay, false);
            else
                AbsoluteLayout.SetLayoutBounds(
                    SectionIndicatorBox,
                    song.GetSection(Playback.CurrentSection).GetProgressRectangle(Playback.CurrentNote)
                );
                
        }

        private void SetTimerInterval()
        {
            // Find interval from the next note's value, and tempo parameters
            Note note;
            if(Playback.CurrentNote < 0) note = song.GetSection(Playback.CurrentSection).Sequence[0];
            else note = song.GetSection(Playback.CurrentSection).Sequence[Playback.CurrentNote];
            double interval = note.NoteValue * 60000.0 / (Playback.TempoGain * song.GetSection(Playback.CurrentSection).Tempo);

            // Set the timer interval to this value
            timer.Duration = (int)interval;
            if (timer.Duration == 0)
                throw new Exception("Timer cannot be set to zero duration.");
        }

    }
}
