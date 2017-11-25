
namespace MetronomeAmplified.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            this.InitializeComponent();
            LoadApplication(new MetronomeAmplified.App(new PlatformStorage(), new PlatformAudioPlayer()));
        }
    }
}
