
/*
 * SettingsObject
 * A class to encapsulate the settings needing to be stored in the Metronome application
 * 
 * Created 26/09/2017 by Thomas Reichert
 *
 */

using Xamarin.Forms;

namespace MetronomeAmplified.Classes
{
    public class SettingsObject
    {
        // Settings parameters
        public bool FlashCues;
        public Color FlashColor;
        public float OneTapGain;
        public float OneTapDuration;
        public PlaybackObject.TempoLiftType OneTapType;

        // Storage keys
        public static string KEY_CUES = "Cues";
        public static string KEY_COLOR_R = "R";
        public static string KEY_COLOR_G = "G";
        public static string KEY_COLOR_B = "B";
        public static string KEY_BPM_GAIN = "Gain";
        public static string KEY_BPM_DURATION = "Duration";
        public static string KEY_LIFT_TYPE = "LiftType";

        // Read platform-specific storage, or set default values if unable
        public SettingsObject(IPlatformStorage platformStorage)
        {
            if (platformStorage != null && platformStorage.KeyExists(KEY_CUES))
            {
                FlashCues = platformStorage.GetBoolean(KEY_CUES);
                FlashColor = new Color(
                    platformStorage.GetFloat(KEY_COLOR_R),
                    platformStorage.GetFloat(KEY_COLOR_G),
                    platformStorage.GetFloat(KEY_COLOR_B)
                );
                OneTapGain = platformStorage.GetFloat(KEY_BPM_GAIN);
                OneTapDuration = platformStorage.GetFloat(KEY_BPM_DURATION);
                OneTapType = (PlaybackObject.TempoLiftType)platformStorage.GetInt(KEY_LIFT_TYPE);
            }
            else {
                FlashCues = false;
                FlashColor = new Color(0.0, 0.8, 0.8);
                OneTapGain = 0.2f;
                OneTapDuration = 180.0f;
                OneTapType = 0;
            }
        }

    }
}
