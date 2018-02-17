using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MetronomeAmplified
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HelpPage : ContentPage
    {
        // Page data
        private const double IMAGE_ASPECT = 0.585365853658536;
        private static string[] sectionNames = { "Navigating the App", "Controlling the Experience", "Crafting Your Song", "Managing Song Files" };
        private static string[][] content = {
            // Navigating the app
            new string[] {
                "The pattern of percussive beats you'll play along with are displayed here. The time signature is shown, along with the timing of each note, if you're familiar with musical notation. A song consists of one or more different sections, each with its own note pattern, and therefore can be very simple, or very complex.",
                "Playback is controlled with the buttons at the bottom. Fast-forward and rewind have a use only with songs that have multiple sections. Pausing will halt the current position in the current section, while stopping will reset the playback position to the beginning.",
                "A section of a song has a default tempo, although the tempo it is being played at can vary. Tapping the beat-per-minute count will reset to the default tempo, while the slider allows free manual adjustments.",
                "Training modes exist to alter the tempo automaticaly while you play, or set a limit to how long you'd like to play. Pressing the One-Touch Tempo Lift button will begin tempo control, and this feature can be customised through the settings screen.",
                "The timer will allow you to set a timespan before the metronome stops playing.",
                "There is more than one set of sounds that the metronome can play - they can be loaded by pressing the Tone button.",
                "The song and its sections can be fully customised by tapping the Song button.",
                "Various settings are accessible by tapping the Settings button. These settings control the Tempo Lift behaviour, as well as visual cues to coincide with the sounds that you hear."
            },
            // Controlling the experience
            new string[] {
                "Flash cues can be toggled on and off by pressing here. They are colourful flashes which follow he sounds that the metronome plays. The colour of the flashes may be changed between several pre-defined options.",
                "The One-Touch Tempo Lift feature is controlled here. It can lift the tempo continuously while you play for a set amount of time, or endlessly rise at the end of each repetition of the song until stopped (or the 300 BPM limit is reached). The amount the tempo rises in that duration, or per repetition, may be adjusted.",
                "The Session Timer lets the metronome play for a set amount of time before stopping. You can choose an amount of time that you wish to play for each time you practise.",
                "There are various progress bars to indicate how far through a song you are (based on how many sections have been played), how far through a particular section you are, or how far through a session you are if the Session Timer is running."
            },
            // Crafting your song
            new string[] {
                "When you choose to edit one of the song's sections, you will reach this screen. At the top you'll find some fundamental parameters you can play with. By default, a section will be in common time with a moderate tempo, however you can change the time signature, adjust the section's default tempo, or change the number of times the section plays before continuing. You can also give the section a unique name.",
                "Here you'll see the note pattern in the section being edited. If you tap on a note, you can set the position of the blue cursor. The cursor sets the position that will be modified using the controls below.",
                "The first two rows of controls let you place notes or rests at the position of the cursor. Once placed in position, intensiy of the sound for each note can be changed by tapping the coloured bar underneath the note in the section display. Rests are less commonly used, but they are available from the second row of controls.",
                "The third row of controls lets you modify the notes in various ways. Ties allow notes to be joined together in a way that none but the first will make a sound. Tuplets allow irregular durations of notes by fitting a different number of notes into a length of times than what would otherwise fit. Dots extend the duration of a note by 50%. Notes or rests can be removed from the section by pressing the cross.",
                "When you are finished modifying the section, press Save to keep the changes. If you've changed your mind and would like to undo all changes since you last saved the section, press cancel."
            },
            // Managing song files
            new string[] {
                "The song can be renamed by pressing here. Only letters, numbers, spaces and hyphen are allowed in the song name. This name is what will be shown to you when you have saved the song and would like to load it again at a later stage.",
                "The sections that make up the song are listed here. There is no limit to how many sections can be here, and you can scroll through the list if there are many.",
                "The controls above the list allow you to delete a section, duplicate a section at the end of the list, start editing a section, or add a new section starting with a basic pattern. Copying sections may make thing easier than adding new ones if you want to preserve tempo or the parts of a section's pattern.",
                "If you are happy with the song you have created, and may want to use it again at a later stage, save it. It will be saved under the name you have given it, with '.gma' at the end. Be aware that if you save a song with the same name as one that already exists, the older version will be replaced by the newer one.",
                "When you tap Load, you will see a list of songs you have saved. You can choose a song to load, or delete songs you no longer want."
            }
        };
        private static string[][] imagesFiles = {
            new string[] { "ss_n1.png", "ss_n1.png", "ss_n1.png", "ss_n1.png", "ss_n1.png", "ss_n1.png", "ss_n1.png", "ss_n1.png" },
            new string[] { "ss_s1.png", "ss_s1.png", "ss_s2.png", "ss_s3.png"},
            new string[] { "ss_f1.png", "ss_f2.png", "ss_f2.png", "ss_f2.png", "ss_f2.png" },
            new string[] { "ss_m1.png", "ss_m1.png", "ss_m1.png", "ss_m1.png", "ss_m2.png" }
        };
        private static double[][] pointerX = {
            new double[] { -0.15, 0.15, 0.12, 0.5, -0.15, -0.2, 0.05, 0.3 },
            new double[] { -0.13, -0.13, -0.13, -0.02 },
            new double[] { -0.13, -0.13, -0.16, -0.16, -0.2 },
            new double[] { -0.13, -0.13, -0.16, -0.16, -0.1 }
        };
        private static double[][] pointerY = {
            new double[] { 0.15, 0.9, 0.32, 0.85, 0.85, 0.02, 0.02, 0.02 },
            new double[] { 0.16, 0.39, 0.41, 0.7 },
            new double[] { 0.16, 0.2, 0.38, 0.72, 0.89 },
            new double[] { 0.11, 0.39, 0.29, 0.89, 0.12 }
        };

        // State
        private int categoryNo = 0;
        private int pageNo = 0;
        double imageWidth = 0.0;
        double imageHeight = 0.0;
        double offsetX = 0.0;
        double offsetY = 0.0;

        // Constructor
        public HelpPage()
        {
            // Don't show the navigation action bar
            NavigationPage.SetHasNavigationBar(this, false);

            // Start up page
            InitializeComponent();

            // Set measurement function
            ContentLayout.SizeChanged += ContentLayout_SizeChanged;
        }
        
        // UI responses
        private void LoadSectionNavigation(object sender, EventArgs e)
        {
            StartSection(0);
        }
        private void LoadSectionControlling(object sender, EventArgs e)
        {
            StartSection(1);
        }
        private void LoadSectionCrafting(object sender, EventArgs e)
        {
            StartSection(2);
        }
        private void LoadSectionManaging(object sender, EventArgs e)
        {
            StartSection(3);
        }
        private void BackToCategories(object sender, EventArgs e)
        {
            HelpHeaders.IsVisible = true;
            HelpContent.IsVisible = false;
        }
        private void ReturnFromHelp(object sender, EventArgs e)
        {
            Navigation.PopAsync();
        }
        private void BackPressed(object sender, EventArgs e)
        {
            if (pageNo > 0)
            {
                pageNo--;
                LoadContent();
            }
        }
        private void ForwardPressed(object sender, EventArgs e)
        {
            if (pageNo < content[categoryNo].Length - 1)
            {
                pageNo++;
                LoadContent();
            }
        }

        // Control content
        private void StartSection(int sectionNumber)
        {
            categoryNo = sectionNumber;
            pageNo = 0;
            LoadContent();
            HelpHeaders.IsVisible = false;
            HelpContent.IsVisible = true;
        }
        private void LoadContent()
        {
            // Load content
            HelpSectionHeader.Text = sectionNames[categoryNo];
            ContentImage.Source = imagesFiles[categoryNo][pageNo];
            ContentText.Text = content[categoryNo][pageNo];
            PositionArrow();

            // Set forward arrow appearance
            if (pageNo > 0)
            {
                BackArrow.IsOpaque = true;
                BackArrow.Opacity = 1.0;
            }
            else
            {
                BackArrow.IsOpaque = false;
                BackArrow.Opacity = 0.3;
            }

            // Set back arrow appearance
            if (pageNo < content[categoryNo].Length - 1)
            {
                ForwardArrow.IsOpaque = true;
                ForwardArrow.Opacity = 1.0;
            }
            else
            {
                ForwardArrow.IsOpaque = false;
                ForwardArrow.Opacity = 0.3;
            }
        }
        private void ContentLayout_SizeChanged(object sender, EventArgs e)
        {
            // Get size of area to place the image in
            double width = ContentLayout.Width;
            double height = 0.6 * ContentLayout.Height;
            if (width / height > IMAGE_ASPECT)
            {
                imageHeight = height;
                imageWidth = height * IMAGE_ASPECT;
                offsetX = 0.5 * (width - imageWidth);
                offsetY = 0.0;
            }
            else
            {
                imageWidth = width;
                imageHeight = width / IMAGE_ASPECT;
                offsetX = 0.0;
                offsetY = 0.5 * (height - imageHeight);
            }

            // Layout out main image
            AbsoluteLayout.SetLayoutFlags(ContentImage, AbsoluteLayoutFlags.None);
            AbsoluteLayout.SetLayoutBounds(ContentImage, new Rectangle(offsetX, offsetY, imageWidth, imageHeight));

            // Layout out text
            AbsoluteLayout.SetLayoutFlags(ContentText, AbsoluteLayoutFlags.All);
            AbsoluteLayout.SetLayoutBounds(ContentText, new Rectangle(0.0, 1.0, 1.0, 0.4));

            // Layout out back arrow
            AbsoluteLayout.SetLayoutFlags(BackArrow, AbsoluteLayoutFlags.PositionProportional);
            AbsoluteLayout.SetLayoutBounds(BackArrow, new Rectangle(0.0, 0.3, imageWidth / 8.0, imageWidth / 4.0));

            // Layout out forward arrow
            AbsoluteLayout.SetLayoutFlags(ForwardArrow, AbsoluteLayoutFlags.PositionProportional);
            AbsoluteLayout.SetLayoutBounds(ForwardArrow, new Rectangle(1.0, 0.3, imageWidth / 8.0, imageWidth / 4.0));

            // Layout out arrow image
            PositionArrow();
        }
        private void PositionArrow()
        {
            // Layout out arrow image
            double pointerOffsetX = pointerX[categoryNo][pageNo];
            double pointerOffsetY = pointerY[categoryNo][pageNo];
            AbsoluteLayout.SetLayoutFlags(ArrowImage, AbsoluteLayoutFlags.None);
            AbsoluteLayout.SetLayoutBounds(ArrowImage, new Rectangle(offsetX + pointerOffsetX * imageWidth, offsetY + pointerOffsetY * imageHeight, imageWidth / 4.0, imageWidth / 8.0));

        }
    }
}