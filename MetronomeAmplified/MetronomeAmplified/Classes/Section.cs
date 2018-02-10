
/*
 * Section
 * Represents a song section as a sequence of notes
 * 
 * Created 28/09/2017 by Thomas Reichert
 *
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MetronomeAmplified.Classes
{
    public class Section
    {
        // The file names of the note images
        private static string[] FILE_NAMES = {
            "semibreve.png", "minim.png", "crotchet.png", "quaver.png", "semiquaver.png", "demisemiquaver.png",
            "dottedsemibreve.png", "dottedminim.png", "dottedcrotchet.png", "dottedquaver.png", "dottedsemiquaver.png", "dotteddemisemiquaver.png",
            "semibreverest.png", "minimrest.png", "crotchetrest.png", "quaverrest.png", "semiquaverrest.png", "demisemiquaverrest.png",
            "dottedsemibreverest.png", "dottedminimrest.png", "dottedcrotchetrest.png", "dottedquaverrest.png", "dottedsemiquaverrest.png", "dotteddemisemiquaverrest.png",
            "blanknote.png", "tie.png", "tuplet.png"
        };

        // The page in which the display is
        private SectionPage DisplayPage;

        // Section attributes
        public string Name;
        public int Repetitions;
        public double Tempo;
        public int BeatsPerMeasure;
        public int BeatValue;

        // Whether or not the note metadata of the entire sequence needs to be re-calculated
        public bool MetadataIsDirty = true;

        // Use properties because these things are bound to the UI in the song page that lists sections
        public string GetName { get { return Name; } }
        public string GetTimeSignatureString { get { return String.Format("{0}/{1}", BeatsPerMeasure, BeatValue); } }
        public string GetBPMString { get { return String.Format("   {0:0} BPM", Tempo); } }
        public string GetRepsString { get { return String.Format("   x {0}", Repetitions); } }

        // The note sequence
        private List<Note> NoteSequence;
        public List<Note> Sequence { get { return NoteSequence; } set { MetadataIsDirty = true; NoteSequence = value; } }

        // Derived drawing parameters
        private double layoutWidth;
        private double layoutHeight;
        private double FirstNoteXOffset;
        private double BlockWidthPerNote;
        private double NoteWidth;

        // Reason string for the sequence being invalid
        public string InvalidReason;

        // Basic constructor
        private Section()
        {
            NoteSequence = new List<Note>();
        }

        // Kinda like a copy constructor
        public static Section DuplicateSection(Section original)
        {
            Section newSection = new Section();
            newSection.Repetitions = original.Repetitions;
            newSection.Tempo = original.Tempo;
            newSection.Name = original.Name;
            newSection.BeatsPerMeasure = original.BeatsPerMeasure;
            newSection.BeatValue = original.BeatValue;
            foreach (Note note in original.NoteSequence)
                newSection.NoteSequence.Add(Note.DuplicateNote(note));
            return newSection;
        }

        // Return a standard section
        public static Section GetNewBasicSection()
        {
            Section section = new Section();
            section.Repetitions = 1;
            section.Tempo = 108.0;
            section.Name = "Unnamed Section";
            section.BeatsPerMeasure = 4;
            section.BeatValue = 4;
            section.NoteSequence.Add(new Note(4, 1));
            section.NoteSequence.Add(new Note(8, 0));
            section.NoteSequence.Add(new Note(8, 0));
            section.NoteSequence.Add(new Note(4, 3));
            section.NoteSequence.Add(new Note(4, 0));
            return section;
        }

        // Calculate the derived attributes
        public void CalculateDerivedAttributes()
        {
            foreach (Note note in NoteSequence)
                note.GenerateDerivedAttributes();
        }

        // Check if the sequence is valid and matches the time signature
        public bool SectionIsValid()
        {
            // Sum of notes must equate to the total length of the measure
            Note note;
            int i;
            int length = NoteSequence.Count;
            int consecutiveValue = 0;
            int transformUnit = 241920;
            int divisor;
            int consecutiveMeasureSize = transformUnit * BeatsPerMeasure / (BeatValue * 24);
            for (i = 0; i < length; i++)
            {
                note = NoteSequence[i];
                // Add value
                divisor = note.NoteType * 24;
                if (note.IsDotted) divisor = divisor * 2 / 3;
                if (note.Tuplet > 0)
                {
                    switch (note.Tuplet)
                    {
                        case 1: divisor = divisor * 3 / 2; break;
                        case 2: divisor = divisor * 5 / 4; break;
                        case 3: divisor = divisor * 6 / 4; break;
                        case 4: divisor = divisor * 7 / 4; break;
                        case 5: divisor = divisor * 7 / 8; break;
                        case 6: divisor = divisor * 9 / 8; break;
                        default:
                            throw new NotImplementedException(String.Format("Invalid tuplet value on note {0} while processing note sequence.", i + 1));
                    }
                }
                if (transformUnit % divisor != 0)
                    throw new NotImplementedException(String.Format("I guess my code doesn't work right ({0} into {1}).", transformUnit, divisor));
                consecutiveValue += transformUnit / divisor;
            }

            // Error if count didn't match up with the time signature
            if (consecutiveValue != consecutiveMeasureSize)
            {
                if (consecutiveValue > consecutiveMeasureSize)
                    InvalidReason = "Total length of notes appears to be too long for the time signature.";
                else
                    InvalidReason = "Total length of notes appears to be too short for the time signature.";
                return false;
            }

            return true;
        }
        
        // Function to populate a provided AbsoluteLayout with note images from this section, and with some sort of position indicator
        public BoxView PopulateSongLayout(SectionPage page, SectionLayout layout, bool IsEditable)
        {
            // Make sure that the layout has been measured
            if (!layout.SizeIsValid)
                return null;
            
            // Remember the page calling this so that it can be notified
            DisplayPage = page;

            // Clear the layout
            layout.Children.Clear();

            // Figure out what to show
            int numberOfNotes = NoteSequence.Count;

            // Find measurements
            layoutWidth = layout.LayoutWidth;
            layoutHeight = layout.LayoutHeight;
            int extraSpaces = IsEditable ? 2 : 1;
            NoteWidth = 0.25 * layoutHeight;
            BlockWidthPerNote = layoutWidth / (numberOfNotes + extraSpaces);
            FirstNoteXOffset = 0.5 * (BlockWidthPerNote - NoteWidth);

            // Calculate section metadata if need be
            if (MetadataIsDirty)
                CalculateNoteMetadata();

            // Display the time signature beat count
            double AnchorX;
            AnchorX = FirstNoteXOffset;
            Label sigCount = new Label();
            sigCount.FontSize = 20;
            sigCount.Text = BeatsPerMeasure.ToString();
            sigCount.HorizontalTextAlignment = TextAlignment.Center;
            sigCount.VerticalTextAlignment = TextAlignment.Center;
            AbsoluteLayout.SetLayoutFlags(sigCount, AbsoluteLayoutFlags.None);
            AbsoluteLayout.SetLayoutBounds(sigCount, new Rectangle(AnchorX, 0.1 * layoutHeight, NoteWidth, 0.4 * layoutHeight));
            layout.Children.Add(sigCount);

            // Display the time signature beat count
            Label sigValue = new Label();
            sigValue.FontSize = 20;
            sigValue.Text = BeatValue.ToString();
            sigValue.HorizontalTextAlignment = TextAlignment.Center;
            sigValue.VerticalTextAlignment = TextAlignment.Center;
            AbsoluteLayout.SetLayoutFlags(sigValue, AbsoluteLayoutFlags.None);
            AbsoluteLayout.SetLayoutBounds(sigValue, new Rectangle(AnchorX, 0.5 * layoutHeight, NoteWidth, 0.4 * layoutHeight));
            layout.Children.Add(sigValue);

            // Place an image of each note, and a box that is coloured according to the accent
            for (int i = 0; i < numberOfNotes; i++)
            {
                Note note = NoteSequence[i];
                // The note image
                Image img = new Image();
                img.Aspect = Aspect.AspectFit;
                if ((note.Metadata.NoOfBeams > 0) && (note.Metadata.JoinableToPrevious || note.Metadata.JoinableToNext))
                {
                    if (note.IsDotted) img.Source = ImageSource.FromFile(FILE_NAMES[8]);
                    else img.Source = ImageSource.FromFile(FILE_NAMES[2]);
                }
                else img.Source = ImageSource.FromFile(FILE_NAMES[note.ImageIndex]);
                if (DisplayPage != null)
                    img.GestureRecognizers.Add(new TapGestureRecognizer {
                        Command = new Command(index => TapImage((int)index)),
                        CommandParameter = i,
                        NumberOfTapsRequired = 1
                    });
                AnchorX = FirstNoteXOffset + BlockWidthPerNote * (i + 1);
                AbsoluteLayout.SetLayoutFlags(img, AbsoluteLayoutFlags.None);
                AbsoluteLayout.SetLayoutBounds(img, new Rectangle(AnchorX, 0.25 * layoutHeight, NoteWidth, 1.5 * NoteWidth));
                layout.Children.Add(img);
                // The accent box
                if (IsEditable)
                {
                    BoxView accentBox = new BoxView();
                    Color color;
                    switch (note.Accent)
                    {
                        case 0: color = Color.FromRgb(0.0, 1.0, 0.0); break;
                        case 1: color = Color.FromRgb(1.0, 1.0, 0.0); break;
                        case 2: color = Color.FromRgb(1.0, 0.5, 0.0); break;
                        case 3: color = Color.FromRgb(1.0, 0.0, 0.0); break;
                        default: color = Color.FromRgb(0.0, 0.0, 0.0); break;
                    }
                    accentBox.BackgroundColor = color;
                    accentBox.Opacity = 0.4;
                    if (DisplayPage != null)
                        accentBox.GestureRecognizers.Add(new TapGestureRecognizer
                        {
                            Command = new Command(index => ChangeAccent((int)index)),
                            CommandParameter = i,
                            NumberOfTapsRequired = 1
                        });
                    AnchorX = BlockWidthPerNote * (i + 1);
                    AbsoluteLayout.SetLayoutFlags(accentBox, AbsoluteLayoutFlags.None);
                    AbsoluteLayout.SetLayoutBounds(accentBox, new Rectangle(AnchorX, 0.8 * layoutHeight, BlockWidthPerNote, 0.2 * layoutHeight));
                    layout.Children.Add(accentBox);
                }
            }

            // For editable displays, add an extra image at the end where a note can be added at the end of the sequence
            if (IsEditable)
            {
                Image img = new Image();
                img.Aspect = Aspect.Fill;
                img.Source = ImageSource.FromFile(FILE_NAMES[24]);
                if (DisplayPage != null)
                    img.GestureRecognizers.Add(new TapGestureRecognizer
                    {
                        Command = new Command(index => TapImage((int)index)),
                        CommandParameter = numberOfNotes,
                        NumberOfTapsRequired = 1
                    });
                AnchorX = BlockWidthPerNote * (numberOfNotes + 1);
                AbsoluteLayout.SetLayoutFlags(img, AbsoluteLayoutFlags.None);
                AbsoluteLayout.SetLayoutBounds(img, new Rectangle(AnchorX, 0.0, BlockWidthPerNote, 0.9 * layoutHeight));
                layout.Children.Add(img);
            }

            // Add ties
            int tieCount;
            double tieWidth;
            for (int i = 0; i < numberOfNotes; i++)
            {
                tieCount = NoteSequence[i].TieString;
                if (tieCount == 0)
                    continue;
                Image img = new Image();
                img.Aspect = Aspect.Fill;
                img.Source = ImageSource.FromFile(FILE_NAMES[25]);
                tieWidth = tieCount - 1;
                AnchorX = FirstNoteXOffset + BlockWidthPerNote * (i + 1);
                AbsoluteLayout.SetLayoutFlags(img, AbsoluteLayoutFlags.None);
                AbsoluteLayout.SetLayoutBounds(img, new Rectangle(AnchorX, 0.65 * layoutHeight, BlockWidthPerNote * tieWidth, 0.1 * layoutHeight));
                layout.Children.Add(img);
                i += tieCount - 1;
            }

            // Add beams
            double beamWidth;
            for (int beamLevel = 1; beamLevel <= 4; beamLevel++)
            {
                for (int i = 0; i < numberOfNotes; i++)
                {
                    // Figure out if note is beamable at this level
                    if (NoteSequence[i].Metadata.NoOfBeams < beamLevel) continue;

                    // Check that this note will be connected to surrounding notes at all
                    if ((NoteSequence[i].Metadata.JoinableToPrevious | NoteSequence[i].Metadata.JoinableToNext) == false) continue;

                    // Check how many notes to beam
                    int noToBeam = 1;
                    while (i + noToBeam < numberOfNotes)
                    {
                        if (NoteSequence[i + noToBeam].Metadata.FallsOnBeat) break;
                        if (NoteSequence[i + noToBeam].Metadata.NoOfBeams < beamLevel) break;
                        noToBeam++;
                    }

                    // Draw the beam
                    BoxView boxy = new BoxView();
                    boxy.BackgroundColor = Color.Black;
                    AnchorX = BlockWidthPerNote * (i + 0.5 + 1);
                    beamWidth = noToBeam == 1 ? NoteWidth * 0.5 : (noToBeam - 1) * BlockWidthPerNote;
                    if (noToBeam == 1)
                    {
                        if (i + 1 == numberOfNotes) AnchorX -= NoteWidth * 0.5;
                        else if (NoteSequence[i].Metadata.JoinableToNext == false) AnchorX -= NoteWidth * 0.5;
                    }
                    AbsoluteLayout.SetLayoutFlags(boxy, AbsoluteLayoutFlags.None);
                    AbsoluteLayout.SetLayoutBounds(boxy, new Rectangle(AnchorX, (0.2 + 0.05 * beamLevel) * layoutHeight, beamWidth, 0.025 * layoutHeight));
                    layout.Children.Add(boxy);
                    i += noToBeam - 1;
                }
            }

            // Add tuplets
            int tupletCount, tupletDisplayNumber;
            double tupletWidth;
            for (int i = 0; i < numberOfNotes; i++)
            {
                tupletCount = NoteSequence[i].Tuplet;
                if (tupletCount == 0) continue;
                switch(tupletCount)
                {
                    case 1: tupletDisplayNumber = 3; break;
                    case 2: tupletDisplayNumber = 5; break;
                    case 3: tupletDisplayNumber = 6; break;
                    case 4: tupletDisplayNumber = 7; break;
                    case 5: tupletDisplayNumber = 7; break;
                    case 6: tupletDisplayNumber = 9; break;
                    default: continue;
                }
                tupletCount = AttemptTupletSequence(i, tupletCount);
                if (tupletCount < 2)
                    // Faulty tuplet set - throw an error
                    NoteSequence[-1].Tuplet = 0;
                Image img = new Image();
                img.Aspect = Aspect.Fill;
                img.Source = ImageSource.FromFile(FILE_NAMES[26]);
                tupletWidth = BlockWidthPerNote * (tupletCount - 1) + NoteWidth;
                AnchorX = FirstNoteXOffset + BlockWidthPerNote * (i + 1);
                AbsoluteLayout.SetLayoutFlags(img, AbsoluteLayoutFlags.None);
                AbsoluteLayout.SetLayoutBounds(img, new Rectangle(AnchorX, 0.1 * layoutHeight, tupletWidth, 0.2 * layoutHeight));
                layout.Children.Add(img);
                Label label = new Label();
                label.Text = tupletDisplayNumber.ToString();
                label.HorizontalTextAlignment = TextAlignment.Center;
                AbsoluteLayout.SetLayoutFlags(label, AbsoluteLayoutFlags.None);
                AbsoluteLayout.SetLayoutBounds(label, new Rectangle(AnchorX, 0.05 * layoutHeight, tupletWidth, 0.2 * layoutHeight));
                layout.Children.Add(label);
                i += tupletCount - 1;
            }

            // Add the section progress indicator thingy, or the cursor selector thingy
            BoxView box = new BoxView();
            box.BackgroundColor = Color.CornflowerBlue;
            AbsoluteLayout.SetLayoutFlags(box, AbsoluteLayoutFlags.None);
            if (IsEditable)
            {
                box.Opacity = 0.4;
                AbsoluteLayout.SetLayoutBounds(box, new Rectangle(0.0, 0.0, BlockWidthPerNote, 0.8 * layoutHeight));
            }
            else
            {
                AbsoluteLayout.SetLayoutBounds(box, new Rectangle(0.0, 0.9 * layoutHeight, BlockWidthPerNote, 0.1 * layoutHeight));
            }
            layout.Children.Add(box);

            // Return a reference to the BoxView progress indication thingy
            return box;

        }

        // Calculate properties of the notes used to make beams
        private void CalculateNoteMetadata()
        {
            // Take this chance to verify tuplet integrity
            int note;
            for (note = 0; note < NoteSequence.Count; note++)
            {
                int tuplet = NoteSequence[note].Tuplet;
                if (tuplet == 0) continue;
                int sequenceLength = AttemptTupletSequence(note, tuplet);
                if (sequenceLength < 2)
                    NoteSequence[note].Tuplet = 0;
                else
                {
                    bool faultySequence = false;
                    for (int scan = note; scan < note + sequenceLength; scan++)
                    {
                        if (NoteSequence[scan].Tuplet != tuplet)
                        {
                            faultySequence = true;
                            break;
                        }
                    }
                    if (faultySequence)
                    {
                        while (note < NoteSequence.Count)
                        {
                            if (NoteSequence[note].Tuplet == tuplet)
                            {
                                NoteSequence[note].Tuplet = 0;
                                note++;
                            }
                            else
                                break;
                        }
                        continue;
                    }
                    note += sequenceLength - 1;
                }
            }

            // Find out which notes are silenced by ties
            note = 0;
            while (note < NoteSequence.Count)
            {
                NoteSequence[note].IsTieExtension = false;
                int tieString = NoteSequence[note].TieString;
                if (tieString > 0)
                {
                    int scan;
                    for (scan = note + 1; scan < note + tieString; scan++)
                        NoteSequence[scan].IsTieExtension = true;
                    note = scan;
                }
                else
                    note++;
            }

            // Find out how long each beat is in normalised units, and find whether each note falls on one of those beats
            int beatLength = (BeatValue == 8) && (BeatsPerMeasure % 3 == 0) ? 136080 : 90720;
            int accumulatedLength = 0;
            for (note = 0; note < NoteSequence.Count; note++)
            {
                NoteSequence[note].Metadata.FallsOnBeat = (accumulatedLength % beatLength) == 0;
                accumulatedLength += NoteSequence[note].NormalisedLengthConsideringTuplets;
            }

            // Find out how many beams each note potentially could have
            for (note = 0; note < NoteSequence.Count; note++)
            {
                Note thisNote = NoteSequence[note];
                if (thisNote.IsSound == false)
                {
                    thisNote.Metadata.NoOfBeams = 0;
                    continue;
                }
                switch (thisNote.NoteType)
                {
                    case 1: thisNote.Metadata.NoOfBeams = 0; break;
                    case 2: thisNote.Metadata.NoOfBeams = 0; break;
                    case 4: thisNote.Metadata.NoOfBeams = 0; break;
                    case 8: thisNote.Metadata.NoOfBeams = 1; break;
                    case 16: thisNote.Metadata.NoOfBeams = 2; break;
                    case 32: thisNote.Metadata.NoOfBeams = 3; break;
                    default: thisNote.Metadata.NoOfBeams = 0; break;
                }
            }

            // Find out if each note can be connected to the next and previous
            for (note = 0; note < NoteSequence.Count; note++)
            {
                Note thisNote = NoteSequence[note];
                thisNote.Metadata.JoinableToPrevious = true;
                if (note == 0) thisNote.Metadata.JoinableToPrevious = false;
                else if (thisNote.Metadata.FallsOnBeat) thisNote.Metadata.JoinableToPrevious = false;
                else if (NoteSequence[note - 1].Metadata.NoOfBeams == 0) thisNote.Metadata.JoinableToPrevious = false;
                thisNote.Metadata.JoinableToNext = false;
                if (thisNote.Metadata.NoOfBeams > 0)
                    if (note < NoteSequence.Count - 1)
                        if (NoteSequence[note + 1].Metadata.FallsOnBeat == false)
                            if (NoteSequence[note + 1].Metadata.NoOfBeams > 0)
                                thisNote.Metadata.JoinableToNext = true;
                
            }
        }

        // Get the rectangle to display the box below the current note being played (add 1 to CurrentNote to give space for time signature)
        public Rectangle GetProgressRectangle(int CurrentNote)
        {
            return new Rectangle(BlockWidthPerNote * (CurrentNote + 1), 0.9 * layoutHeight, BlockWidthPerNote, 0.1 * layoutHeight);
        }

        // Get the rectangle to display the cursor over the current note being played
        public Rectangle GetCursorRectangle(int CurrentNote)
        {
            return new Rectangle(BlockWidthPerNote * (CurrentNote + 1), 0.0, BlockWidthPerNote, 0.8 * layoutHeight);
        }

        // Function to action pressing a particular note, or an accent box, given its Id attribute
        private void TapImage(int img)
        {
            DisplayPage.SetCursor(img);
        }
        private void ChangeAccent(int note)
        {
            NoteSequence[note].Accent++;
            if (NoteSequence[note].Accent > 3)
                NoteSequence[note].Accent = 0;
            DisplayPage.UpdateDisplay();
        }

        // Check for the validity of a tuplet sequence beginning at a certain position in this sequence
        public int AttemptTupletSequence(int startPos, int tupletTypeIndex)
        {
            // Find out how many notes (of the smallest type) are tied together by this tuplet, starting from this position, or return -1 if it cannot be formed
            int notesToCount;
            switch(tupletTypeIndex)
            {
                case 1: notesToCount = 3; break;
                case 2: notesToCount = 5; break;
                case 3: notesToCount = 6; break;
                case 4: notesToCount = 7; break;
                case 5: notesToCount = 7; break;
                case 6: notesToCount = 9; break;
                default: return -1;
            }

            // Scan from this position and count notes
            int smallestNoteDuration = NoteSequence[startPos].NormalisedLengthIgnoreTuplets;
            int accumulatedDuration = 0;
            int scan = startPos;
            while (scan < NoteSequence.Count)
            {
                Note thisNote = NoteSequence[scan];
                
                // Add value to the accumulated duration, and check if this is a new smallest note
                accumulatedDuration += thisNote.NormalisedLengthIgnoreTuplets;
                if (thisNote.NormalisedLengthIgnoreTuplets < smallestNoteDuration)
                    smallestNoteDuration = thisNote.NormalisedLengthIgnoreTuplets;

                // Shift attention to next note, and break out if the target is now reached or surpassed
                scan++;
                if (accumulatedDuration >= notesToCount * smallestNoteDuration)
                    break;
            }

            // Return the number of notes used to form the required tuplet, or -1 if it couldn't be done
            return accumulatedDuration == notesToCount * smallestNoteDuration ? scan - startPos : -1;
        }
        
    }
}
