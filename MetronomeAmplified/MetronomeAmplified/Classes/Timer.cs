
/*
 * Timer
 * Timer class, using Task.Delay to run the timer method asynchronously
 * Based on dlxeon's answer to a question on StackOverflow,
 * https://stackoverflow.com/questions/32050052/timer-doesnt-contain-in-system-threading-at-xamarin-forms
 * 
 * Alleviates the problems that the XamarinForms Device timer is crap, and the third-party AdvancedTimer NuGet package
 * does not seem to provide an implementation for UWP projects.
 * 
 * Created 25/09/2017 by Thomas Reichert
 *
 */

using System;
using System.Threading;
using System.Threading.Tasks;

namespace MetronomeAmplified.Classes
{
    class Timer
    {
        // Callback function delegate
        public delegate void CallbackFunc(object sender, EventArgs e);
        CallbackFunc Callback;

        // Holdings of the timer duration and whether this thing is running or not
        public int Duration;
        private bool Running;

        // Constructor
        public Timer(CallbackFunc callback, int timerDuration)
        {
            Callback = callback;
            Duration = timerDuration;
            Running = false;
        }
        public async void Run(bool runImmediatelyWhenStarted)
        {
            Int32 LastUptime, TimeToWait;
            Running = true;
            if (!runImmediatelyWhenStarted)
                await Task.Delay(Duration);
            while (Running)
            {
                LastUptime = System.Environment.TickCount;
                Callback(null, null);
                TimeToWait = Duration - (System.Environment.TickCount - LastUptime);
                if (TimeToWait < 0)
                    TimeToWait = 0;
                await Task.Delay(TimeToWait);
            }
        }
        public void Stop()
        {
            Running = false;
        }
    }
}
