
using System.Collections.ObjectModel;

namespace MetronomeAmplified.Classes
{
    public class SectionList : ObservableCollection<Section>
    {
        public void ReplaceItem(int index, Section item)
        {
            // I don't know why, but the SongPage doesn't update its list in the right way if this call isn't duplicated...
            SetItem(index, item);
            SetItem(index, item);
        }
    }
}
