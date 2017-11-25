
/*
 * PlatformAudioPlayer
 * Perform audio functions on Android platform
 * 
 * Created 28/09/2017 by Thomas Reichert
 *
 */

using System;

using Android.Content;
using Android.Support.V4.Media.Session;
using Android.Service.Media;
using Android.Support.V4.Media;
using Android.Media;
using Android.Support.V7.App;
using Android.App;

using MetronomeAmplified.Classes;

namespace MetronomeAmplified.Droid
{
    class PlatformAudioPlayer : IPlatformAudioPlayer
    {
        private static int[] files = { Resource.Raw.hat, Resource.Raw.kick, Resource.Raw.tom, Resource.Raw.snare, Resource.Raw.wood_gentle, Resource.Raw.wood_mid, Resource.Raw.wood_high, Resource.Raw.wood_low };
        private MediaSessionCompat mediaSession;
        private Context context;
        private Notification notify;

        private static string[] file_names = { "hat.wav", "kick.wav", "tom.wav", "snare.wav", "wood_gentle.wav", "wood_mid.wav", "wood_high.wav", "wood_low.wav" };

        private Track[] tracks;
        
        public PlatformAudioPlayer(Context _context)
        {
            // Assign Activity context
            context = _context;
            tracks = new Track[] { null, null, null, null };

            // Create a new media session, and pass it to a MediaBrowserServiceCompat subclass
            mediaSession = new MediaSessionCompat(context, "Session1");
            MediaBrowserServiceCompat service = new BrowserService(mediaSession);

            // Give the media session some capabilities, including the ability to be controlled by Media Buttons
            mediaSession.SetFlags(MediaSessionCompat.FlagHandlesMediaButtons | MediaSessionCompat.FlagHandlesTransportControls);
            PlaybackStateCompat.Builder playbackState = new PlaybackStateCompat.Builder().SetActions(PlaybackStateCompat.ActionPlay | PlaybackStateCompat.ActionPlayPause);
            mediaSession.SetPlaybackState(playbackState.Build());
            NotificationCompat.Builder noti = new NotificationCompat.Builder(context);
            noti.SetContentTitle("Metronome Amplified");
            noti.SetContentText("Control");
            noti.SetSmallIcon(Resource.Drawable.TerribleMetronomeImage);
            NotificationCompat.MediaStyle mediaStyle = new NotificationCompat.MediaStyle();
            mediaStyle.SetMediaSession(mediaSession.SessionToken);
            noti.SetStyle(mediaStyle);
            notify = noti.Build();

            // Pass in a class with appropriate callbacks
            mediaSession.SetCallback(new MediaSessionCallbacks(this));

            // Say that this session is ready to play something
            mediaSession.Active = true;

        }

        public async void Init()
        {

        }

        public void PlaySound(int fileIndex)
        {
            int accent = fileIndex % 4;
            Track track = tracks[accent];
            if (track == null)
            {
                // Open a file
                tracks[accent] = Track.OpenTrack(context, files[fileIndex]);

                // Get the notification service registered to the MediaSessionCompat
                NotificationManager man = (NotificationManager)context.GetSystemService(Context.NotificationService);
                man.Notify(0, notify);

                tracks[accent].WriteAsync(tracks[accent].TrackData, 0, tracks[accent].TrackDataLength).ContinueWith((result) => { tracks[accent].Play(); });
                
            }
            else
            {
                // Track has been initialised, so rewind it and play it again
                if (track.PlayState == PlayState.Playing)
                    track.Stop();
                track.ReloadStaticData();
                track.Play();
            }
            
        }

        public void StopAudio()
        {
            for (int i = 0; i < 4; i++)
            {
                if (tracks[i] != null)
                {
                    PlayState state = tracks[i].PlayState;
                    if (state == PlayState.Playing || state == PlayState.Paused)
                    {
                        tracks[i].Flush();
                        tracks[i].Stop();
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
                if (tracks[i] == null)
                    tracks[i] = Track.OpenTrack(context, fileNo);
                else if (tracks[i].PlayState == PlayState.Playing)
                    tracks[i].Stop();
                await tracks[i].WriteAsync(tracks[i].TrackData, 0, tracks[i].TrackDataLength);
            }
            
        }

    }
}