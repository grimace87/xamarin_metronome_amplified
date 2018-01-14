
/*
 * PlatformAudioPlayer
 * Perform audio functions on Android platform
 * 
 * Created 28/09/2017 by Thomas Reichert
 *
 */

using Android.Content;
using Android.Media;
using Android.App;

using MetronomeAmplified.Classes;
using Android.Support.V7.App;
using Android.Support.V4.Media.Session;
using Android.OS;

namespace MetronomeAmplified.Droid
{
    class PlatformAudioPlayer : IPlatformAudioPlayer
    {
        private static int[] files = { Resource.Raw.hat, Resource.Raw.kick, Resource.Raw.tom, Resource.Raw.snare, Resource.Raw.wood_gentle, Resource.Raw.wood_mid, Resource.Raw.wood_high, Resource.Raw.wood_low };
        private Context context;

        private static string[] file_names = { "hat.wav", "kick.wav", "tom.wav", "snare.wav", "wood_gentle.wav", "wood_mid.wav", "wood_high.wav", "wood_low.wav" };

        private Track[] tracks;

        private bool isPlaying = false;
        
        public Notification notify;
        private NotificationManager notificationManager;

        private const int NOTIFICATION_ID = 0x729bc72a;
        
        public PlatformAudioPlayer(Context _context)
        {
            // Assign Activity context and stuff
            this.context = _context;
            this.tracks = new Track[] { null, null, null, null };
            
            // Get the notification manager
            this.notificationManager = (NotificationManager)this.context.GetSystemService(Context.NotificationService);

            // Give the media session some capabilities, including the ability to be controlled by Media Buttons
            PlaybackStateCompat.Builder playbackState = new PlaybackStateCompat.Builder().SetActions(PlaybackStateCompat.ActionPlay | PlaybackStateCompat.ActionPlayPause);
            NotificationCompat.Builder noti = new NotificationCompat.Builder(this.context);
            noti.SetOngoing(true);
            noti.SetSmallIcon(Resource.Drawable.icon);
            noti.SetContentTitle("Metronome Amplified");
            noti.SetContentText("Control");

            // Make a pending intent to focus the app
            Intent intent = new Intent(this.context, typeof(MainActivity));
            PendingIntent pend = PendingIntent.GetActivity(this.context, NOTIFICATION_ID, intent, PendingIntentFlags.CancelCurrent);
            noti.SetContentIntent(pend);

            // Make an intent to stop the player
            Intent stopIntent = new Intent(this.context, typeof(ActionService));
            PendingIntent stopPending = PendingIntent.GetService(this.context, NOTIFICATION_ID, stopIntent, PendingIntentFlags.CancelCurrent);
            noti.AddAction(Resource.Drawable.stopbutton, "Stop", stopPending);

            // Set the amount of visible detail for show on the lock screen
            noti.SetVisibility(NotificationCompat.VisibilityPrivate);

            // Build the notification
            notify = noti.Build();

        }

        public void Init()
        {
            const int trackSetOffset = 0;
            for (int i = 0; i < 4; i++)
            {
                // Open a file
                this.tracks[i] = Track.OpenTrack(this.context, PlatformAudioPlayer.files[trackSetOffset + i]);

                // Write audio data
                this.tracks[i].WriteAsync(this.tracks[i].TrackData, 0, this.tracks[i].TrackDataLength);
            }
        }

        public void PlaySound(int fileIndex)
        {
            int accent = fileIndex % 4;
            Track track = this.tracks[accent];
            if (this.isPlaying == false)
            {
                this.isPlaying = true;

                this.notificationManager.Notify(NOTIFICATION_ID, notify);
            }
            
            if (track.PlayState == PlayState.Playing)
                track.Stop();
            track.ReloadStaticData();
            track.Play();
            
        }

        public void StopAudio()
        {
            this.notificationManager.Cancel(NOTIFICATION_ID);
            this.isPlaying = false;

            for (int i = 0; i < 4; i++)
            {
                if (this.tracks[i] != null)
                {
                    PlayState state = this.tracks[i].PlayState;
                    if (state == PlayState.Playing || state == PlayState.Paused)
                    {
                        this.tracks[i].Flush();
                        this.tracks[i].Stop();
                    }
                }
            }
        }

        public async void LoadTracks(string[] accentFiles)
        {
            for (int i = 0; i < 4; i++)
            {
                int fileNo = 0;
                for (int scan = 0; scan < file_names.Length; scan++)
                    if (file_names[scan] == accentFiles[i])
                    {
                        fileNo = files[scan];
                        break;
                    }
                if (fileNo == 0)
                    continue;
                if (this.tracks[i] == null)
                    this.tracks[i] = Track.OpenTrack(context, fileNo);
                else if (this.tracks[i].PlayState == PlayState.Playing)
                    this.tracks[i].LoadNewAudioFile(context, fileNo);
                await this.tracks[i].WriteAsync(this.tracks[i].TrackData, 0, this.tracks[i].TrackDataLength);
            }
            
        }

    }
}