
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

        // Use properties because these things are bound to the UI in the song page that lists sections
        public string GetName { get { return Name; } }
        public string GetTimeSignatureString { get { return String.Format("{0}/{1}", BeatsPerMeasure, BeatValue); } }
        public string GetBPMString { get { return String.Format("   {0:0} BPM", Tempo); } }
        public string GetRepsString { get { return String.Format("   x {0}", Repetitions); } }

        // The note sequence
        private List<Note> NoteSequence;
        public List<Note> Sequence { get { return NoteSequence; } set { NoteSequence = value; } }

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

        // Evaluate the section and generate derived attributes
        public void CalculateDerivedAttributes()
        {
            // Validate and calculate derived attributes
            if (SectionIsValid())
            {
                CalculateFirstDerivedAttributes();
                CalculateSecondDerivedAttributes();
            }
            else
                GenerateInvalidSequenceImages();
            
        }

        // Check if the sequence is valid and matches the time signature
        public bool SectionIsValid()
        {
            // Sum of notes must equate to the total length of the measure
            Note note;
            int i;
            int length = Sequence.Count;
            int consecutiveValue = 0;
            int transformUnit = 241920;
            int divisor;
            int consecutiveMeasureSize = transformUnit * BeatsPerMeasure / (BeatValue * 24);
            for (i = 0; i < length; i++)
            {
                note = Sequence[i];
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

        // Calculate the first set of derived attributes for each note
        private void CalculateFirstDerivedAttributes()
        {
            // Set default first attributes which won't necessarily be modified in the code that follows
            
        }

        // Calculate the second set of derived attributes
        private void CalculateSecondDerivedAttributes()
        {
            foreach (Note note in Sequence)
                note.GenerateValidSecondaries();
        }

        // Generate basic images for notes in an invalid sequence
        private void GenerateInvalidSequenceImages()
        {
            foreach (Note note in Sequence)
                note.GenerateInvalidSecondaries();
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
            int numberOfNotes = Sequence.Count;

            // Find measurements
            layoutWidth = layout.LayoutWidth;
            layoutHeight = layout.LayoutHeight;
            double extraSpace = IsEditable ? 1.0 : 0.0;
            NoteWidth = 0.25 * layoutHeight;
            BlockWidthPerNote = layoutWidth / (numberOfNotes + extraSpace);
            FirstNoteXOffset = 0.5 * (BlockWidthPerNote - NoteWidth);

            // Place an image of each note, and a box that is coloured according to the accent
            double AnchorX;
            for (int i = 0; i < numberOfNotes; i++)
            {
                Note note = Sequence[i];
                // The note image
                Image img = new Image();
                img.Aspect = Aspect.AspectFit;
                img.Source = ImageSource.FromFile(FILE_NAMES[note.ImageIndex]);
                if (DisplayPage != null)
                    img.GestureRecognizers.Add(new TapGestureRecognizer {
                        Command = new Command(index => TapImage((int)index)),
                        CommandParameter = i,
                        NumberOfTapsRequired = 1
                    });
                AnchorX = FirstNoteXOffset + BlockWidthPerNote * i;
                AbsoluteLayout.SetLayoutFlags(img, AbsoluteLayoutFlags.None);
                AbsoluteLayout.SetLayoutBounds(img, new Rectangle(AnchorX, 0.25 * layoutHeight, NoteWidth, 2.0 * NoteWidth));
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
                    AnchorX = BlockWidthPerNote * i;
                    AbsoluteLayout.SetLayoutFlags(accentBox, AbsoluteLayoutFlags.None);
                    AbsoluteLayout.SetLayoutBounds(accentBox, new Rectangle(AnchorX, 0.9 * layoutHeight, BlockWidthPerNote, 0.1 * layoutHeight));
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
                AnchorX = BlockWidthPerNote * numberOfNotes;
                AbsoluteLayout.SetLayoutFlags(img, AbsoluteLayoutFlags.None);
                AbsoluteLayout.SetLayoutBounds(img, new Rectangle(AnchorX, 0.0, BlockWidthPerNote, 0.9 * layoutHeight));
                layout.Children.Add(img);
            }

            // Add ties
            int tieCount;
            double tieWidth;
            for (int i = 0; i < numberOfNotes; i++)
            {
                tieCount = Sequence[i].TieString;
                if (tieCount == 0)
                    continue;
                Image img = new Image();
                img.Aspect = Aspect.Fill;
                img.Source = ImageSource.FromFile(FILE_NAMES[25]);
                tieWidth = tieCount - 1;
                AnchorX = FirstNoteXOffset + BlockWidthPerNote * i;
                AbsoluteLayout.SetLayoutFlags(img, AbsoluteLayoutFlags.None);
                AbsoluteLayout.SetLayoutBounds(img, new Rectangle(AnchorX, 0.8 * layoutHeight, BlockWidthPerNote * tieWidth, 0.1 * layoutHeight));
                layout.Children.Add(img);
                i += tieCount - 1;
            }

            // Add beams
            /*for (int i = 0; i < numberOfNotes; i++)
            {
                // Figure out if note is beamable
                Note note = Sequence[i];
                if (!note.FirstOfBeat || !note.NextIsJoinable) continue;
                
                // Draw the beam
                BoxView boxy = new BoxView();
                boxy.BackgroundColor = Color.Black;
                AnchorX = BlockWidthPerNote * (i + 0.5);
                AbsoluteLayout.SetLayoutFlags(boxy, AbsoluteLayoutFlags.None);
                AbsoluteLayout.SetLayoutBounds(boxy, new Rectangle(AnchorX, 0.1 * layoutHeight, BlockWidthPerNote, 0.1 * layoutHeight));
                layout.Children.Add(boxy);
            }*/

            // Add tuplets
            int tupletCount, tupletDisplayNumber;
            double tupletWidth;
            for (int i = 0; i < numberOfNotes; i++)
            {
                tupletCount = Sequence[i].Tuplet;
                if (tupletCount == 0)
                {
                    i++;
                    continue;
                }
                switch(tupletCount)
                {
                    case 1: tupletDisplayNumber = 3; break;
                    case 2: tupletDisplayNumber = 5; break;
                    case 3: tupletDisplayNumber = 6; break;
                    case 4: tupletDisplayNumber = 7; break;
                    case 5: tupletDisplayNumber = 7; break;
                    case 6: tupletDisplayNumber = 9; break;
                    default: i++; continue;
                }
                tupletCount = AttemptTupletSequence(i, tupletCount);
                if (tupletCount < 2)
                    // Faulty tuplet set - throw an error
                    Sequence[-1].Tuplet = 0;
                Image img = new Image();
                img.Aspect = Aspect.Fill;
                img.Source = ImageSource.FromFile(FILE_NAMES[26]);
                tupletWidth = BlockWidthPerNote * (tupletCount - 1) + NoteWidth;
                AnchorX = FirstNoteXOffset + BlockWidthPerNote * i;
                AbsoluteLayout.SetLayoutFlags(img, AbsoluteLayoutFlags.None);
                AbsoluteLayout.SetLayoutBounds(img, new Rectangle(AnchorX, 0.0, tupletWidth, 0.2 * layoutHeight));
                layout.Children.Add(img);
                Label label = new Label();
                label.Text = tupletDisplayNumber.ToString();
                label.HorizontalTextAlignment = TextAlignment.Center;
                AbsoluteLayout.SetLayoutFlags(label, AbsoluteLayoutFlags.None);
                AbsoluteLayout.SetLayoutBounds(label, new Rectangle(AnchorX, 0.0, tupletWidth, 0.2 * layoutHeight));
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
                AbsoluteLayout.SetLayoutBounds(box, new Rectangle(0.0, 0.0, BlockWidthPerNote, layoutHeight));
            }
            else
            {
                AbsoluteLayout.SetLayoutBounds(box, new Rectangle(0.0, 0.9 * layoutHeight, BlockWidthPerNote, 0.1 * layoutHeight));
            }
            layout.Children.Add(box);

            // Return a reference to the BoxView progress indication thingy
            return box;

        }

        // Get the rectangle to display the box below the current note being played
        public Rectangle GetProgressRectangle(int CurrentNote)
        {
            return new Rectangle(BlockWidthPerNote * CurrentNote, 0.9 * layoutHeight, BlockWidthPerNote, 0.1 * layoutHeight);
        }

        // Get the rectangle to display the cursor over the current note being played
        public Rectangle GetCursorRectangle(int CurrentNote)
        {
            return new Rectangle(BlockWidthPerNote * CurrentNote, 0.0, BlockWidthPerNote, layoutHeight);
        }

        // Function to action pressing a particular note, or an accent box, given its Id attribute
        private void TapImage(int img)
        {
            DisplayPage.SetCursor(img);
        }
        private void ChangeAccent(int note)
        {
            Sequence[note].Accent++;
            if (Sequence[note].Accent > 3)
                Sequence[note].Accent = 0;
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
            int smallestNoteDuration = Sequence[startPos].NormalisedLengthIgnoreTuplets;
            int accumulatedDuration = 0;
            int scan = startPos;
            while (scan < Sequence.Count)
            {
                Note thisNote = Sequence[scan];
                
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
