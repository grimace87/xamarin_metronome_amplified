using System;

using Android.App;
using Android.Content;
using Android.OS;

namespace MetronomeAmplified.Droid
{
    class ActionService : Service
    {
        Context UsingContext;
        PlatformAudioPlayer Player;
        ActionBinder binder;

        public ActionService(Context context, PlatformAudioPlayer player)
        {
            // Store things
            this.UsingContext = context;
            this.Player = player;
            this.binder = new ActionBinder(this);

        }

        public override void OnCreate()
        {
            base.OnCreate();
            
        }

        public override IBinder OnBind(Intent intent)
        {
            throw new NotImplementedException();
        }
        
        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            this.Player.StopAudio();
            return StartCommandResult.Sticky;
        }

        public class ActionBinder : Binder
        {
            ActionService Service;
            public ActionBinder(ActionService service)
            {
                this.Service = service;
            }

            ActionService GetService()
            {
                return this.Service;
            }
        }
        
    }
}