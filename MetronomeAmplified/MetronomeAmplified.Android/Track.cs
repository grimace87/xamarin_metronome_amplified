
using System;

using Android.Content;
using Android.Media;

using MetronomeAmplified.Classes;

namespace MetronomeAmplified.Droid
{
    class Track : AudioTrack
    {
        public int TrackDataLength;
        public byte[] TrackData;

        private Track(Stream streamType, int sampleRateInHz, ChannelOut channelConfig, Encoding audioFormat, int bufferSizeInBytes, AudioTrackMode mode, int sessionId, int dataLength, byte[] data)
        :
            base(streamType, sampleRateInHz, channelConfig, audioFormat, bufferSizeInBytes, mode, sessionId)
        {
            TrackDataLength = dataLength;
            TrackData = data;
        }

        public unsafe void LoadNewAudioFile(Context context, int resID)
        {
            // Stop playback first
            this.Stop();

            // Read a resource as an input stream
            System.IO.Stream input = context.Resources.OpenRawResource(resID);

            // Read header bytes (20 bytes, WAVEFORMATEX struct (18 bytes usually), 8 bytes)
            byte[] hdr1 = new byte[20];
            input.Read(hdr1, 0, 20);

            // Find size of WAVEFORMATEX structure
            int sizeOfStruct;
            fixed (byte* ptr = &hdr1[16])
            {
                sizeOfStruct = *((int*)ptr);
            }

            // Read WAVEFORMATEX
            byte[] hdrFormatEx = new byte[sizeOfStruct];
            input.Read(hdrFormatEx, 0, sizeOfStruct);

            // Find some parameters
            int samplesPerSec;
            Int16 bitsPerSample;
            fixed (byte* ptr = hdrFormatEx)
            {
                samplesPerSec = *((int*)(ptr + 4));
                bitsPerSample = *((short*)(ptr + 14));
            }

            // Return if things aren't alright
            if (samplesPerSec != 44100 || bitsPerSample != 16)
                return;

            // Read the remaining header information
            byte[] hdr2 = new byte[8];
            input.Read(hdr2, 0, 8);

            // Get the size of audio data
            int dataLength;
            fixed (byte* ptr = hdr2)
            {
                dataLength = *((int*)(ptr + 4));
            }

            // Allocate space for the audio data and read it from the file
            byte[] data = new byte[dataLength];
            input.Read(data, 0, dataLength);

            // Store data in this object
            TrackDataLength = dataLength;
            TrackData = data;

        }

        public static unsafe Track OpenTrack(Context context, int resID)
        {
            // Read a resource as an input stream
            System.IO.Stream input = context.Resources.OpenRawResource(resID);

            // Read header bytes (20 bytes, WAVEFORMATEX struct (18 bytes usually), 8 bytes)
            byte[] hdr1 = new byte[20];
            input.Read(hdr1, 0, 20);

            // Find size of WAVEFORMATEX structure
            int sizeOfStruct;
            fixed (byte* ptr = &hdr1[16])
            {
                sizeOfStruct = *((int*)ptr);
            }

            // Read WAVEFORMATEX
            byte[] hdrFormatEx = new byte[sizeOfStruct];
            input.Read(hdrFormatEx, 0, sizeOfStruct);

            // Find some parameters
            int samplesPerSec;
            Int16 bitsPerSample;
            fixed (byte* ptr = hdrFormatEx)
            {
                samplesPerSec = *((int*)(ptr + 4));
                bitsPerSample = *((short*)(ptr + 14));
            }

            // Return if things aren't alright
            if (samplesPerSec != 44100 || bitsPerSample != 16)
                return null;

            // Read the remaining header information
            byte[] hdr2 = new byte[8];
            input.Read(hdr2, 0, 8);

            // Get the size of audio data
            int dataLength;
            fixed (byte* ptr = hdr2)
            {
                dataLength = *((int*)(ptr + 4));
            }

            // Allocate space for the audio data and read it from the file
            byte[] data = new byte[dataLength];
            input.Read(data, 0, dataLength);

            // Create the audio track and start playing it
            return new Track(Stream.Music, samplesPerSec, ChannelOut.Mono, Encoding.Pcm16bit, dataLength, AudioTrackMode.Static, 0, dataLength, data);

        }

    }
}