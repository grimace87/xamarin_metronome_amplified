

using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.Media;
using Android.Support.V4.Media.Session;

namespace MetronomeAmplified.Droid
{
    class BrowserService : MediaBrowserServiceCompat
    {
        private MediaSessionCompat mediaSession;

        public BrowserService(MediaSessionCompat session)
        {
            mediaSession = session;
        }

        public override BrowserRoot OnGetRoot(string clientPackageName, int clientUid, Bundle rootHints)
        {
            throw new NotImplementedException();
        }

        public override void OnLoadChildren(string parentId, Result result)
        {
            throw new NotImplementedException();
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            MediaButtonReceiver.HandleIntent(mediaSession, intent);
            return base.OnStartCommand(intent, flags, startId);
        }
    }
}