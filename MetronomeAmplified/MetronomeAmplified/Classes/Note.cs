
/*
 * Note
 * Represents a note with all attributes that it may have
 * 
 * Created 28/09/2017 by Thomas Reichert
 *
 */

namespace MetronomeAmplified.Classes
{
    public class Note
    {
        // Primary parameters of a note
        public bool IsSound;
        public int NoteType;
        public bool IsDotted;
        public int Tuplet;
        public int TieString;
        public int Accent;

        // Derived attributes
        public int ImageIndex;
        public double NoteValue;
        public bool IsTieExtension;

        // Beam-related metadata
        public BeamabilityMetadata Metadata;

        // The note indices used for indexing the image file array:
        // 0-5:   semibreve ... demisemiquaver
        // 6-11:  dottedsemibreve ... dotteddemisemiquaver
        // 12-17: semibreverest ... demisemibreverest
        // 18-23: dottedsemibreverest ... dotteddemisemiquaverrest
        
        // Normalised note duration, ignoring tuplets or not
        public int NormalisedLengthIgnoreTuplets
        {
            get
            {
                int baseLength = 11340 * (32 / NoteType);
                if (IsDotted) baseLength = 3 * baseLength / 2;
                return baseLength;
            }
        }
        public int NormalisedLengthConsideringTuplets
        {
            get
            {
                int length = NormalisedLengthIgnoreTuplets;
                switch (Tuplet)
                {
                    case 1: length = (length * 2) / 3; break;
                    case 2: length = (length * 4) / 5; break;
                    case 3: length = (length * 4) / 6; break;
                    case 4: length = (length * 4) / 7; break;
                    case 5: length = (length * 8) / 7; break;
                    case 6: length = (length * 8) / 9; break;
                }
                return length;
            }
        }

        // Constructors for a new note
        public Note(int noteType, int accent) : this(noteType, accent, true) { }
        public Note(int noteType, int accent, bool isSound)
        {
            // Set primary attributes
            IsSound = isSound;
            IsDotted = false;
            TieString = 0;
            NoteType = noteType;
            Tuplet = 0;
            Accent = accent;

            // Create the metadata object
            Metadata = new BeamabilityMetadata();

            // Generate the most important derived attributes
            GenerateDerivedAttributes();

        }

        // Kinda like a copy constructor
        public static Note DuplicateNote(Note original)
        {
            // Copy basic parameters
            Note newNote = new Note(original.NoteType, original.Accent);
            newNote.IsSound = original.IsSound;
            newNote.IsDotted = original.IsDotted;
            newNote.TieString = original.TieString;
            newNote.Tuplet = original.Tuplet;

            // Generate the most important derived attributes
            newNote.GenerateDerivedAttributes();

            // Return
            return newNote;
        }
        
        // Generate derived attributes given that the sequence is invalid
        public void GenerateDerivedAttributes()
        {
            switch (NoteType)
            {
                case 1:
                    NoteValue = 4.0;
                    ImageIndex = 0;
                    break;
                case 2:
                    NoteValue = 2.0;
                    ImageIndex = 1;
                    break;
                case 4:
                    NoteValue = 1.0;
                    ImageIndex = 2;
                    break;
                case 8:
                    NoteValue = 0.5;
                    ImageIndex = 3;
                    break;
                case 16:
                    NoteValue = 0.25;
                    ImageIndex = 4;
                    break;
                case 32:
                    NoteValue = 0.125;
                    ImageIndex = 5;
                    break;
            }
            if (IsDotted)
            {
                ImageIndex += 6;
                NoteValue *= 1.5;
            }
            if (IsSound == false)
                ImageIndex += 12;
            if (Tuplet > 0)
            {
                switch (Tuplet)
                {
                    case 1: NoteValue *= 2.0 / 3.0; break;
                    case 2: NoteValue *= 4.0 / 5.0; break;
                    case 3: NoteValue *= 4.0 / 6.0; break;
                    case 4: NoteValue *= 4.0 / 7.0; break;
                    case 5: NoteValue *= 8.0 / 7.0; break;
                    case 6: NoteValue *= 8.0 / 9.0; break;
                }
            }
        }
        
        // Metadata relating to beams, stored within an object of a sub-class, which is invalidated if any part of the note sequence changes
        public class BeamabilityMetadata
        {
            public bool FallsOnBeat;
            public bool JoinableToNext;
            public int NoOfBeams;
        }

    }
}
