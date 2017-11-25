
/*
 * PlaybackObject
 * A class to encapsulate the state of playback
 * 
 * Created 28/09/2017 by Thomas Reichert
 *
 */

namespace MetronomeAmplified.Classes
{
    public class PlaybackObject
    {
        // Enum for the files, used by the platform-specific IPlatformAudioPlayer interface implementations
        public enum AudioFiles { DRUMS_HAT, DRUMS_KICK, DRUMS_TOM, DRUMS_SNARE };

        // Tempo-lifting type enum
        public enum TempoLiftType { LiftGradual, LiftPerRep };

        // Basic playback parameters
        public bool IsPlaying;
        public double TempoGain;
        public int CurrentSection;
        public int CurrentRep;
        public int CurrentNote;

        // Session (timer) parameters
        public bool IsSession;
        public double SessionTime;
        public double SessionLimit;

        // Tempo lifting parameters
        public bool IsLifting;
        public TempoLiftType LiftType;
        private double LiftTempoStart;
        private double LiftTempoTarget;
        private double LiftRunningTempo;
        private double LiftTime;
        private double LiftGainPerRep;

        // Constructor setting basic stuff
        public PlaybackObject()
        {
            IsPlaying = false;
            IsSession = false;
            IsLifting = false;

            TempoGain = 1.0;
        }

        // Start lifting tempo
        public void StartGradualLift(double startTempo, double fractionGain, double secondsDuration)
        {
            if (startTempo > 299.5)
                return;
            IsLifting = true;
            LiftTempoStart = startTempo;
            LiftRunningTempo = 0.0;
            LiftType = TempoLiftType.LiftGradual;
            LiftTime = secondsDuration;
            LiftTempoTarget = startTempo * (1.0 + fractionGain);
            if (LiftTempoTarget > 300.0)
            {
                LiftTime *= (300.0 - LiftTempoStart) / (LiftTempoTarget - LiftTempoStart);
                LiftTempoTarget = 300.0;
            }
        }
        public void StartIncremetalLift(double startTempo, double fractionGain)
        {
            IsLifting = true;
            LiftTempoStart = startTempo;
            LiftRunningTempo = startTempo;
            LiftType = TempoLiftType.LiftPerRep;
            LiftGainPerRep = fractionGain;
        }

        // Lift tempo
        public double RunGradualLift(double millisecondTimeStep)
        {
            LiftRunningTempo += 0.001 * millisecondTimeStep;
            if (LiftRunningTempo > LiftTime)
                IsLifting = false;
            return LiftTempoStart + (LiftRunningTempo / LiftTime) * (LiftTempoTarget - LiftTempoStart);
        }
        public double RunLiftAfterRep()
        {
            LiftRunningTempo += LiftTempoStart * LiftGainPerRep;
            if (LiftRunningTempo > 300.0)
            {
                LiftRunningTempo = 300.0;
                IsLifting = false;
            }
            return LiftRunningTempo;
        }

        // Start a session
        public void StartSession(double duration)
        {
            if (duration < 1) return;
            IsSession = true;
            SessionTime = 0.0;
            SessionLimit = duration;
        }
        public bool RunSession(double millisecondInterval)
        {
            SessionTime += 0.001 * millisecondInterval;
            if (SessionTime > SessionLimit)
            {
                IsSession = false;
                IsPlaying = false;
            }
            return IsPlaying;
        }

    }
}
