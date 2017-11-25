using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

using MetronomeAmplified.Classes;

namespace MetronomeAmplified
{
    public partial class App : Application
    {
        public App(IPlatformStorage platformStorage, IPlatformAudioPlayer audioPlayer)
        {
            InitializeComponent();
            MainPage = new NavigationPage(new MetronomeAmplified.MainPage(platformStorage, audioPlayer));
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
