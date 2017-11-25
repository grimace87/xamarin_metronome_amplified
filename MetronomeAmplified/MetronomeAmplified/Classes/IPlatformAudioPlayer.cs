
/*
 * IPlatformAudioPlayer
 * Interface implemented to perform platform-specific audio playback functions
 * 
 * Created 28/09/2017 by Thomas Reichert
 *
 */

namespace MetronomeAmplified.Classes
{
    public interface IPlatformAudioPlayer
    {
        void Init();
        void PlaySound(int fileIndex);
        void StopAudio();
        void LoadTracks(string[] accentFiles);
    }
}
