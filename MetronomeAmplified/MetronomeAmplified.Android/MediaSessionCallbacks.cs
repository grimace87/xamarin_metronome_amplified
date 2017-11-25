
using Android.Support.V4.Media.Session;

using MetronomeAmplified.Classes;

namespace MetronomeAmplified.Droid
{
    class MediaSessionCallbacks : MediaSessionCompat.Callback
    {
        // Construct with a reference to the player
        private PlatformAudioPlayer player;
        public MediaSessionCallbacks(PlatformAudioPlayer _player)
        {
            player = _player;
        }

        // Implement callbacks for specified actions (Play and PlayPause)
        public override void OnPlay()
        {
            player.PlaySound(0);
            base.OnPlay();
        }
        public override void OnPause()
        {
            player.StopAudio();
            base.OnPause();
        }

    }
}