
/*
 * Song
 * Represents a song as a sequence of sections
 * 
 * Created 28/09/2017 by Thomas Reichert
 *
 */

using Xamarin.Forms.PlatformConfiguration;

namespace MetronomeAmplified.Classes
{
    public class Song
    {
        // Basic parameters
        public string Name;
        public SectionList Sections;
        
        // Public properties
        public int NumberOfSections { get { return Sections.Count; } }

        // Constructor for a new song with a new name
        public Song()
        {
            Sections = new SectionList
            {
                Section.GetNewBasicSection()
            };
            Sections[0].CalculateDerivedAttributes();
        }
        
        // Go to the next section
        public Section GetSection(int SectionNo)
        {
            return Sections[SectionNo];
        }
        
    }
}
