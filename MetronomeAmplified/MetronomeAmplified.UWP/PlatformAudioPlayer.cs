
/*
 * PlatformAudioPlayer
 * Perform audio functions on UWP
 * 
 * Created 28/09/2017 by Thomas Reichert
 *
 */

using System;
using Windows.Media.Audio;

using MetronomeAmplified.Classes;
using System.Threading.Tasks;
using Windows.Storage;

namespace MetronomeAmplified.UWP
{
    class PlatformAudioPlayer : IPlatformAudioPlayer
    {
        // The file name strings
        // Maby appending ms-appx:/// ?
        private static string[] files = { "hat.wav", "kick.wav", "tom.wav", "snare.wav" };

        // The audio graph, file input nodes, and device output node (no intermediate submixes required)
        private AudioGraph audioGraph;
        private AudioFileInputNode[] inputNodes;
        private AudioDeviceOutputNode outputNode;

        private bool hasInit = false;
        private bool isStarted = false;
        private bool[] soundReady = { false, false, false, false };

        // Construct the audio player object, with an Audio Graph containing no inputs and one output
        public PlatformAudioPlayer()
        {
            inputNodes = new AudioFileInputNode[] { null, null, null, null };
        }

        public async void Init()
        {
            if (hasInit)
                return;

            // Create the UWP AudioGraph object
            await InitAudioGraphAsync();
            if (audioGraph == null) return;

            // Create the OutputNode for playback
            await CreateOutputNodeAsync();
            if (outputNode == null) return;

            // Create the input nodes
            for (int i = 0; i < 4; i++)
            {
                inputNodes[i] = await CreateInputNodeAsync(files[i]);
                inputNodes[i].AddOutgoingConnection(outputNode);
                soundReady[i] = true;
            }
            hasInit = true;
        }

        public void PlaySound(int fileIndex)
        {
            if (hasInit == false)
                return;
            int index = fileIndex % 4;
            if (isStarted == false)
            {
                // Signal started
                isStarted = true;
                // Stop other sounds from playing on first playthrough
                for (int i = 0; i < 4; i++)
                {
                    if (i == index)
                        continue;
                    inputNodes[i].Seek(TimeSpan.FromMilliseconds(inputNodes[i].Duration.Milliseconds - 1));
                }
                audioGraph.Start();
            }
            if (soundReady[index])
            {
                inputNodes[index].Seek(TimeSpan.FromSeconds(0.0));
                inputNodes[index].Start();
            }
        }

        public void StopAudio()
        {
            if (hasInit == false)
                return;
            isStarted = false;
            // Stop the AudioGraph which stops all nodes
            audioGraph.Stop();
        }

        public async void LoadTracks(string[] accentFiles)
        {
            for (int i = 0; i < 4; i++)
            {
                soundReady[i] = false;
                if (inputNodes[i] != null)
                {
                    inputNodes[i].Stop();
                    inputNodes[i].Dispose();
                }
                inputNodes[i] = await CreateInputNodeAsync(accentFiles[i]);
                inputNodes[i].AddOutgoingConnection(outputNode);
                soundReady[i] = true;
            }
        }

        // Create the audio graph and its output node
        private async Task InitAudioGraphAsync()
        {
            // Create the audio graph
            AudioGraphSettings settings = new AudioGraphSettings(Windows.Media.Render.AudioRenderCategory.Media);
            settings.QuantumSizeSelectionMode = QuantumSizeSelectionMode.LowestLatency;
            CreateAudioGraphResult result = await AudioGraph.CreateAsync(settings);
            if (result.Status != AudioGraphCreationStatus.Success)
                return;
            audioGraph = result.Graph;

        }

        // Create the (device) output node
        private async Task CreateOutputNodeAsync()
        {
            if (audioGraph == null)
                return;
            CreateAudioDeviceOutputNodeResult result = await audioGraph.CreateDeviceOutputNodeAsync();
            if (result == null)
                return;
            if (result.Status != AudioDeviceNodeCreationStatus.Success)
                return;
            outputNode = result.DeviceOutputNode;
        }

        // Create a file input node
        private async Task<AudioFileInputNode> CreateInputNodeAsync(string fileName)
        {
            StorageFolder folder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            StorageFile file = await folder.GetFileAsync(fileName);
            CreateAudioFileInputNodeResult result = await audioGraph.CreateFileInputNodeAsync(file);
            if (result.Status != AudioFileNodeCreationStatus.Success)
                return null;
            return result.FileInputNode;
        }
        
    }
}
