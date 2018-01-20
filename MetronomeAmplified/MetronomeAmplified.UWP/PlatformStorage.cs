
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Windows.Storage;
using Windows.Storage.Streams;

using MetronomeAmplified.Classes;

namespace MetronomeAmplified.UWP
{
    class PlatformStorage : IPlatformStorage
    {
        // Keys and such
        private static string STORAGE_NAME = "Settings";
        ApplicationDataContainer appData;

        // File storage stuff
        IOutputStream outputStream;
        IInputStream inputStream;
        DataWriter dataWriter;
        DataReader dataReader;

        // Constructor
        public PlatformStorage()
        {
            // Key-value storage objects
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.CreateContainer(STORAGE_NAME, ApplicationDataCreateDisposition.Always);
            appData = localSettings.Containers[STORAGE_NAME];

        }

        // Implemented methods
        public bool KeyExists(string key)
        {
            object set = appData.Values[key];
            return set != null;
        }
        public float GetFloat(string key)
        {
            return (float)appData.Values[key];
        }
        public void StoreFloat(string key, float value)
        {
            appData.Values[key] = value;
        }
        public int GetInt(string key)
        {
            return (int)appData.Values[key];
        }
        public void StoreInt(string key, int value)
        {
            appData.Values[key] = value;
        }
        public bool GetBoolean(string key)
        {
            return (bool)appData.Values[key];
        }
        public void StoreBoolean(string key, bool value)
        {
            appData.Values[key] = value;
        }

        // File-related stuff
        public async Task<string> GetNewSongName()
        {
            int attempt = 0;
            string testName = "";
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            while (true)
            {
                attempt++;
                testName = "Song" + attempt;
                IStorageItem file = await folder.TryGetItemAsync(testName);
                if (file != null)
                    continue;
                break;
            }
            return testName;
        }
        public async Task<string[]> GetExistingFileList()
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            IReadOnlyList<StorageFile> files = await folder.GetFilesAsync();
            int count = files.Count;
            string[] fileNames = new string[count];
            for (int i = 0; i < count; i++)
                fileNames[i] = files[i].Name;
            return fileNames;
        }
        public async Task<bool> FileExists(string fileName)
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            IStorageItem file = await folder.TryGetItemAsync(fileName);
            return file != null;
        }
        public async Task<bool> OpenFile(string fileName, bool forWriting)
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            IStorageItem f = await folder.TryGetItemAsync(fileName);
            if (f == null)
                f = await folder.CreateFileAsync(fileName);
            StorageFile file = (StorageFile)f;
            if (forWriting)
            {
                IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite);
                outputStream = stream.GetOutputStreamAt(0);
                dataWriter = new DataWriter(outputStream);
                dataWriter.UnicodeEncoding = UnicodeEncoding.Utf8;
                dataWriter.ByteOrder = ByteOrder.LittleEndian;
            }
            else
            {
                IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read);
                ulong len = stream.Size;
                inputStream = stream.GetInputStreamAt(0);
                dataReader = new DataReader(inputStream);
                await dataReader.LoadAsync((uint)len);
                dataReader.UnicodeEncoding = UnicodeEncoding.Utf8;
                dataReader.ByteOrder = ByteOrder.LittleEndian;
                dataReader.InputStreamOptions = InputStreamOptions.ReadAhead;
            }
            return true;
        }
        public async Task<bool> DeleteFile(string fileName)
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            IStorageItem f = await folder.TryGetItemAsync(fileName);
            if (f != null)
            {
                await f.DeleteAsync(StorageDeleteOption.PermanentDelete);
                return true;
            }
            return false;
        }
        public async Task CloseFile(bool fileWasForWriting)
        {
            if (fileWasForWriting)
            {
                await dataWriter.StoreAsync();
                await dataWriter.FlushAsync();
                dataWriter.Dispose();
            }
            else
            {
                dataReader.Dispose();
            }
        }
        public string FileReadString()
        {
            uint len = dataReader.ReadUInt32();
            return dataReader.ReadString(len);
        }
        public int FileReadInt()
        {
            return dataReader.ReadInt32();
        }
        public bool FileReadBool()
        {
            return dataReader.ReadBoolean();
        }
        public float FileReadFloat()
        {
            return dataReader.ReadSingle();
        }
        public void FileWriteString(string val)
        {
            dataWriter.WriteUInt32((uint)val.Length);
            dataWriter.WriteString(val);
        }
        public void FileWriteInt(int val)
        {
            dataWriter.WriteInt32(val);
        }
        public void FileWriteBool(bool val)
        {
            dataWriter.WriteBoolean(val);
        }
        public void FileWriteFloat(float val)
        {
            dataWriter.WriteSingle(val);
        }

    }
}
