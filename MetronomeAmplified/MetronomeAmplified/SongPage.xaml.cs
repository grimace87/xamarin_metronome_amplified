
using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using MetronomeAmplified.Classes;
using System.Collections.Generic;

namespace MetronomeAmplified
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SongPage : ContentPage
    {
        // Use a property to bind the Song's Name to the UI
        private Song CurrentSong;
        public string SongName
        {
            get { return CurrentSong.Name; }
            set {
                CurrentSong.Name = value;
                OnPropertyChanged("SongName");
            }
        }
        public SectionList SongList
        {
            get {
                return CurrentSong.Sections;
            }
        }

        // Remember the parent page
        private MainPage ParentPage;

        // Flags regarding the status of the song being saved
        public bool SongHasUnsavedChanges;

        // Construct the page with a reference to the parent page's Song object
        public SongPage(MainPage parentPage, Song song)
        {
            // Don't show the navigation action bar
            NavigationPage.SetHasNavigationBar(this, false);

            ParentPage = parentPage;

            SongHasUnsavedChanges = false;

            CurrentSong = song;
            InitializeComponent();
            BindingContext = this;
            
        }
        
        // UI event handlers
        private void SectionDelete(object sender, EventArgs e)
        {
            if (CurrentSong.NumberOfSections < 2)
                DisplayAlert("Issue", "Cannot delete the last section.", "OK");
            else
            {
                Section selected = (Section)listSections.SelectedItem;
                int index = CurrentSong.Sections.IndexOf(selected);
                if (index == ParentPage.Playback.CurrentSection)
                {
                    if (ParentPage.Playback.CurrentSection > 0)
                        ParentPage.Playback.CurrentSection--;
                    ParentPage.Playback.CurrentNote = 0;
                    ParentPage.Playback.CurrentRep = 0;
                }
                if (selected == null)
                    return;
                CurrentSong.Sections.Remove(selected);
            }
        }
        private void SectionCopy(object sender, EventArgs e)
        {
            Section selected = (Section)listSections.SelectedItem;
            if (selected == null)
                return;
            CurrentSong.Sections.Add(Section.DuplicateSection(selected));
            SongHasUnsavedChanges = true;
        }
        private void SectionEdit(object sender, EventArgs e)
        {
            Section selected = (Section)listSections.SelectedItem;
            if (selected == null)
                return;
            Navigation.PushAsync(new SectionPage(this, CurrentSong.Sections, CurrentSong.Sections.IndexOf(selected)));
        }
        private void SectionNew(object sender, EventArgs e)
        {
            CurrentSong.Sections.Add(Section.GetNewBasicSection());
            SongHasUnsavedChanges = true;
        }
        private async void LoadNameUpdatePage(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new NameUpdatePage(this));
        }
        private async void SaveSong(object sender, EventArgs e)
        {
            IPlatformStorage storage = ParentPage.PlatformStorage;
            string fileName = CurrentSong.Name + ".gma";
            bool exists = await storage.FileExists(fileName);
            if (exists)
            {
                bool accepted = await DisplayAlert("Information", "A song is already saved with that name. Would you like to overwrite it?", "Yes", "No");
                if (!accepted)
                {
                    await DisplayAlert("Information", "The song was not saved.", "OK");
                    return;
                }
            }

            await storage.OpenFile(fileName, true);
            try
            {
                // Put song details
                //  Name
                //  Number of section
                int length = CurrentSong.NumberOfSections;
                storage.FileWriteString(CurrentSong.Name);
                storage.FileWriteInt(length);

                // For each section, put section details
                //  Name
                //  Repetitions
                //  Tempo
                //  BeatsPerMeasure
                //  BeatValue
                //  Number of notes in the sequence
                //  (The sequence)
                for (int s = 0; s < length; s++)
                {
                    // Basic properties
                    Section sec = CurrentSong.Sections[s];
                    int numberOfNotes = sec.Sequence.Count;
                    storage.FileWriteString(sec.Name);
                    storage.FileWriteInt(sec.Repetitions);
                    storage.FileWriteFloat((float)sec.Tempo);
                    storage.FileWriteInt(sec.BeatsPerMeasure);
                    storage.FileWriteInt(sec.BeatValue);
                    storage.FileWriteInt(numberOfNotes);

                    // For each note in sequence, put all non-derived properties
                    //  IsSound
                    //  NoteType
                    //  IsDotted
                    //  Tuplet
                    //  TieString
                    //  Accent
                    for (int n = 0; n < numberOfNotes; n++)
                    {
                        Note note = sec.Sequence[n];
                        storage.FileWriteBool(note.IsSound);
                        storage.FileWriteInt(note.NoteType);
                        storage.FileWriteBool(note.IsDotted);
                        storage.FileWriteInt(note.Tuplet);
                        storage.FileWriteInt(note.TieString);
                        storage.FileWriteInt(note.Accent);
                    }

                }

                await DisplayAlert("Information", "Song saved.", "OK");
                SongHasUnsavedChanges = false;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Issue", "Could not save the song - " + ex.Message, "OK");
            }
            finally
            {
                storage.CloseFile(true);
            }

        }
        private async void LoadSong(object sender, EventArgs e)
        {
            if (SongHasUnsavedChanges)
            {
                bool confirm = await DisplayAlert("Question", "The current song has unsaved changes. Are you sure you want to load another song?", "Yes", "No");
                if (!confirm)
                    return;
            }

            IPlatformStorage storage = ParentPage.PlatformStorage;
            string[] fileList = await storage.GetExistingFileList();
            string choice = await DisplayActionSheet("Load which song?", "Cancel", null, fileList);
            if (choice == "Cancel")
                return;

            await storage.OpenFile(choice, false);
            try
            {
                // Get song details
                //  Name
                //  Number of section
                SongName = storage.FileReadString();
                int length = storage.FileReadInt();
                SongList.Clear();

                // For each section, get section details
                //  Name
                //  Repetitions
                //  Tempo
                //  BeatsPerMeasure
                //  BeatValue
                //  Number of notes in the sequence
                //  (The sequence)
                for (int s = 0; s < length; s++)
                {
                    // Basic properties
                    Section sec = Section.GetNewBasicSection();
                    sec.Name = storage.FileReadString();
                    sec.Repetitions = storage.FileReadInt();
                    sec.Tempo = (double)storage.FileReadFloat();
                    sec.BeatsPerMeasure = storage.FileReadInt();
                    sec.BeatValue = storage.FileReadInt();
                    int numberOfNotes = storage.FileReadInt();
                    sec.Sequence = new List<Note>();

                    // For each note in sequence, put all non-derived properties
                    //  IsSound
                    //  NoteType
                    //  IsDotted
                    //  Tuplet
                    //  TieString
                    //  Accent
                    for (int n = 0; n < numberOfNotes; n++)
                    {
                        Note note = new Note(4, 0);
                        note.IsSound = storage.FileReadBool();
                        note.NoteType = storage.FileReadInt();
                        note.IsDotted = storage.FileReadBool();
                        note.Tuplet = storage.FileReadInt();
                        note.TieString = storage.FileReadInt();
                        note.Accent = storage.FileReadInt();

                        sec.Sequence.Add(note);
                    }
                    SongList.Add(sec);
                    CurrentSong.Sections[s].CalculateDerivedAttributes();

                }

                ParentPage.UpdateDisplay();
                SongHasUnsavedChanges = false;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Issue", "Could not load the song - " + ex.Message, "OK");
            }
            finally
            {
                storage.CloseFile(false);
            }
            
        }
        private async void NewSong(object sender, EventArgs e)
        {
            if (SongHasUnsavedChanges)
            {
                bool confirm = await DisplayAlert("Question", "The current song has unsaved changes. Are you sure you want to create a new song?", "Yes", "No");
                if (!confirm)
                    return;
            }
            
            // Update things on this page
            SongName = await ParentPage.PlatformStorage.GetNewSongName();
            SongList.Clear();
            SongList.Add(Section.GetNewBasicSection());
            ParentPage.UpdateDisplay();

            SongHasUnsavedChanges = false;
        }
        private async void Return(object sender, EventArgs e)
        {
            // Return to main page
            ParentPage.UpdateDisplay();
            ParentPage.RefreshTempo();
            await Navigation.PopAsync();
        }
    }
}