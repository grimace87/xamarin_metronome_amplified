
using System;
using System.Collections.ObjectModel;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using MetronomeAmplified.Classes;
using System.Collections.Generic;

namespace MetronomeAmplified
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SectionPage : ContentPage
    {
        // References to the original section data
        private SongPage ParentPage;
        private SectionList ParentSections;
        private int SectionIndex;

        // A copy of the section data
        private Section WorkingSection;

        // UI element that needs to be worked with
        private BoxView CursorBox;
        private int CursorPos;

        // Strings for the drop-down pickers
        private string[] ListOfBeatPerMeasures = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12" };
        private string[] ListOfBeatValues = { "4", "8" };

        // Properties bound to UI
        public string GetSongTitle { get { return ParentPage.SongName; } set { ParentPage.SongName = value; OnPropertyChanged("GetSongTitle"); } }
        public string SectionName { get { return WorkingSection.Name; } set { WorkingSection.Name = value; OnPropertyChanged("SectionName"); } }
        public int BeatsPerMeasure { get { return WorkingSection.BeatsPerMeasure; } set { WorkingSection.BeatsPerMeasure = value; OnPropertyChanged("BeatsPerMeasure"); } }
        public int BeatValue { get { return WorkingSection.BeatValue; } set { WorkingSection.BeatValue = value; OnPropertyChanged("BeatValue"); } }
        public string[] GetBeatPerMeasureList { get { return ListOfBeatPerMeasures; } }
        public string[] GetBeatValueList { get { return ListOfBeatValues; } }

        // The descriptions of tuplet types
        private const string TUP1 = "3 (against 2)", TUP2 = "5 (against 4)", TUP3 = "6 (against 4)", TUP4 = "7 (against 4)",
            TUP5 = "7 (against 8)", TUP6 = "9 (against 8)";
        private readonly string[] TupletTypes = new string[] { TUP1, TUP2, TUP3, TUP4, TUP5, TUP6 };

        public SectionPage(SongPage parentPage, SectionList sections, int sectionIndex)
        {
            // Don't show the navigation action bar
            NavigationPage.SetHasNavigationBar(this, false);

            // Duplicate the passed song
            ParentPage = parentPage;
            ParentSections = sections;
            SectionIndex = sectionIndex;
            WorkingSection = Section.DuplicateSection(ParentSections[SectionIndex]);
            
            // Initialise the page with its XAML and such
            InitializeComponent();
            BindingContext = this;
            pickBeatsPerMeasure.SelectedIndex = 3;
            pickBeatValue.SelectedIndex = 0;
            pickBeatsPerMeasure.SelectedIndexChanged += ChangeBeatsPerMeasure;
            pickBeatValue.SelectedIndexChanged += ChangeBeatValue;
            pickBeatsPerMeasure.SelectedIndex = WorkingSection.BeatsPerMeasure - 1;
            pickBeatValue.SelectedIndex = WorkingSection.BeatValue == 4 ? 0 : 1;
            sliderTempo.Value = (WorkingSection.Tempo - 44.0) / 2.56;
            TempoSliderChanged(null, null);
            labelReps.Text = String.Format("Play {0} times", WorkingSection.Repetitions);

            // Validate the section and display it
            CursorPos = 0;
            UpdateDisplay();

        }

        // UI event handlers
        private async void ChangeSectionName(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new SectionNameUpdatePage(this));
        }
        private void ChangeBeatsPerMeasure(object sender, EventArgs e)
        {
            BeatsPerMeasure = pickBeatsPerMeasure.SelectedIndex + 1;
            UpdateDisplay();
        }
        private void ChangeBeatValue(object sender, EventArgs e)
        {
            BeatValue = pickBeatValue.SelectedIndex == 0 ? 4 : 8;
            UpdateDisplay();
        }
        private void SaveChanges(object sender, EventArgs e)
        {
            if (WorkingSection.SectionIsValid())
            {
                // Change the observable collection, which should trigger the CollectionChanged event, which should trigger the bound view to update
                ParentSections.ReplaceItem(SectionIndex, WorkingSection);

                // Return to previous page
                Navigation.PopAsync();
                ParentPage.SongHasUnsavedChanges = true;
            }
            else
                DisplayAlert("Issue", WorkingSection.InvalidReason, "OK");
        }
        private void Cancel(object sender, EventArgs e)
        {
            Navigation.PopAsync();
        }
        private void TempoSliderChanged(object sender, EventArgs e)
        {
            // Adjust playback tempo gain
            WorkingSection.Tempo = sliderTempo.Value * 2.56 + 44.0;
            labelTempo.Text = String.Format("{0:F0} BPM", WorkingSection.Tempo);
        }
        private void IncrementReps(object sender, EventArgs e)
        {
            if (WorkingSection.Repetitions < 64)
            {
                WorkingSection.Repetitions++;
                labelReps.Text = String.Format("Play {0} times", WorkingSection.Repetitions);
            }
        }
        private void DecrementReps(object sender, EventArgs e)
        {
            if (WorkingSection.Repetitions > 1)
            {
                WorkingSection.Repetitions--;
                labelReps.Text = String.Format("Play {0} times", WorkingSection.Repetitions);
            }
        }

        // Validate stuff and update the display
        public void UpdateDisplay()
        {
            WorkingSection.CalculateDerivedAttributes();
            CursorBox = WorkingSection.PopulateSongLayout(this, layoutSectionDisplay, true);
            SetCursor(CursorPos);
        }
        public void SetCursor(int noteNumber)
        {
            CursorPos = noteNumber;
            if (CursorPos > WorkingSection.Sequence.Count)
                CursorPos = WorkingSection.Sequence.Count;
            AbsoluteLayout.SetLayoutBounds(CursorBox, WorkingSection.GetCursorRectangle(noteNumber));
        }

        // Event handlers to insert and modify notes
        private void InsertSemibreve(object sender, EventArgs e)
        {
            if (CursorPos < WorkingSection.Sequence.Count)
                WorkingSection.Sequence.RemoveAt(CursorPos);
            WorkingSection.Sequence.Insert(CursorPos, new Note(1, 3));
            UpdateDisplay();
        }
        private void InsertMinim(object sender, EventArgs e)
        {
            if (CursorPos < WorkingSection.Sequence.Count)
                WorkingSection.Sequence.RemoveAt(CursorPos);
            WorkingSection.Sequence.Insert(CursorPos, new Note(2, 3));
            UpdateDisplay();
        }
        private void InsertCrotchet(object sender, EventArgs e)
        {
            if (CursorPos < WorkingSection.Sequence.Count)
                WorkingSection.Sequence.RemoveAt(CursorPos);
            WorkingSection.Sequence.Insert(CursorPos, new Note(4, 3));
            UpdateDisplay();
        }
        private void InsertQuaver(object sender, EventArgs e)
        {
            if (CursorPos < WorkingSection.Sequence.Count)
                WorkingSection.Sequence.RemoveAt(CursorPos);
            WorkingSection.Sequence.Insert(CursorPos, new Note(8, 1));
            UpdateDisplay();
        }
        private void InsertSemiquaver(object sender, EventArgs e)
        {
            if (CursorPos < WorkingSection.Sequence.Count)
                WorkingSection.Sequence.RemoveAt(CursorPos);
            WorkingSection.Sequence.Insert(CursorPos, new Note(16, 1));
            UpdateDisplay();
        }
        private void InsertDemisemiquaver(object sender, EventArgs e)
        {
            if (CursorPos < WorkingSection.Sequence.Count)
                WorkingSection.Sequence.RemoveAt(CursorPos);
            WorkingSection.Sequence.Insert(CursorPos, new Note(32, 1));
            UpdateDisplay();
        }
        private void InsertSemibreveRest(object sender, EventArgs e)
        {
            if (CursorPos < WorkingSection.Sequence.Count)
                WorkingSection.Sequence.RemoveAt(CursorPos);
            WorkingSection.Sequence.Insert(CursorPos, new Note(1, 3, false));
            UpdateDisplay();
        }
        private void InsertMinimRest(object sender, EventArgs e)
        {
            if (CursorPos < WorkingSection.Sequence.Count)
                WorkingSection.Sequence.RemoveAt(CursorPos);
            WorkingSection.Sequence.Insert(CursorPos, new Note(2, 3, false));
            UpdateDisplay();
        }
        private void InsertCrotchetRest(object sender, EventArgs e)
        {
            if (CursorPos < WorkingSection.Sequence.Count)
                WorkingSection.Sequence.RemoveAt(CursorPos);
            WorkingSection.Sequence.Insert(CursorPos, new Note(4, 3, false));
            UpdateDisplay();
        }
        private void InsertQuaverRest(object sender, EventArgs e)
        {
            if (CursorPos < WorkingSection.Sequence.Count)
                WorkingSection.Sequence.RemoveAt(CursorPos);
            WorkingSection.Sequence.Insert(CursorPos, new Note(8, 1, false));
            UpdateDisplay();
        }
        private void InsertSemiquaverRest(object sender, EventArgs e)
        {
            if (CursorPos < WorkingSection.Sequence.Count)
                WorkingSection.Sequence.RemoveAt(CursorPos);
            WorkingSection.Sequence.Insert(CursorPos, new Note(16, 1, false));
            UpdateDisplay();
        }
        private void InsertDemisemiquaverRest(object sender, EventArgs e)
        {
            if (CursorPos < WorkingSection.Sequence.Count)
                WorkingSection.Sequence.RemoveAt(CursorPos);
            WorkingSection.Sequence.Insert(CursorPos, new Note(32, 1, false));
            UpdateDisplay();
        }
        private void EmbellishTie(object sender, EventArgs e)
        {
            if (CursorPos == WorkingSection.Sequence.Count)
                return;
            if (WorkingSection.Sequence.Count < 2)
                return;
            if (WorkingSection.Sequence[CursorPos].TieString > 0)
                WorkingSection.Sequence[CursorPos].TieString = 0;
            else
            {
                if (CursorPos == 0)
                {
                    // For first note, join to next
                    if (WorkingSection.Sequence[1].TieString == 0)
                        WorkingSection.Sequence[1].TieString++;
                    WorkingSection.Sequence[0].TieString = WorkingSection.Sequence[1].TieString + 1;
                }
                else
                {
                    int prev = WorkingSection.Sequence[CursorPos - 1].TieString;
                    if (prev == 0)
                    {
                        // New tie with previous note
                        WorkingSection.Sequence[CursorPos - 1].TieString = 2;
                        WorkingSection.Sequence[CursorPos].TieString = 2;
                    }
                    else
                    {
                        // Attach to existing tie preceding this note
                        prev++;
                        int count = prev;
                        int pos = CursorPos;
                        while (count > 0)
                        {
                            WorkingSection.Sequence[pos].TieString = prev;
                            pos--;
                            count--;
                        }
                    }
                }
            }
            WorkingSection.FixTies();
            UpdateDisplay();
        }
        private async void EmbellishTuplet(object sender, EventArgs e)
        {
            if (CursorPos == WorkingSection.Sequence.Count)
                return;
            if (WorkingSection.Sequence[CursorPos].Tuplet > 0)
                WorkingSection.Sequence[CursorPos].Tuplet = 0;
            else
            {
                string tuplet = await DisplayActionSheet("Tuplet grouping", "Cancel", null, TupletTypes);
                if (tuplet == "Cancel")
                    return;
                int type;
                switch (tuplet)
                {
                    case TUP1: type = 1; break;
                    case TUP2: type = 2; break;
                    case TUP3: type = 3; break;
                    case TUP4: type = 4; break;
                    case TUP5: type = 5; break;
                    case TUP6: type = 6; break;
                    default: return;
                }
                int numberOfNotes = WorkingSection.EvaluateTupletSequence(CursorPos, type);
                if (numberOfNotes < 0)
                    await DisplayAlert("Issue", "Cannot begin that tuplet sequence from that position.", "OK");
                else
                {
                    int setPos = CursorPos;
                    while (numberOfNotes > 0)
                    {
                        WorkingSection.Sequence[setPos].Tuplet = type;
                        setPos++;
                        numberOfNotes--;
                    }
                }
            }
            WorkingSection.ClearDodgyTuplets();
            UpdateDisplay();
        }
        private void EmbellishDot(object sender, EventArgs e)
        {
            if (CursorPos == WorkingSection.Sequence.Count)
                return;
            WorkingSection.Sequence[CursorPos].IsDotted = !WorkingSection.Sequence[CursorPos].IsDotted;
            UpdateDisplay();
        }
        private void EmbellishErase(object sender, EventArgs e)
        {
            if (CursorPos == WorkingSection.Sequence.Count || WorkingSection.Sequence.Count == 1)
                return;
            WorkingSection.Sequence.RemoveAt(CursorPos);
            if (CursorPos > 0)
                CursorPos--;
            WorkingSection.FixTies();
            WorkingSection.ClearDodgyTuplets();
            UpdateDisplay();
        }

    }

}