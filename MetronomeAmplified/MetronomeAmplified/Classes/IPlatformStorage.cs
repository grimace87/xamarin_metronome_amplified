using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetronomeAmplified.Classes
{
    public interface IPlatformStorage
    {
        // Key-value storage
        bool KeyExists(string key);
        bool GetBoolean(string key);
        void StoreBoolean(string key, bool value);
        float GetFloat(string key);
        void StoreFloat(string key, float value);
        int GetInt(string key);
        void StoreInt(string key, int value);

        // File storage
        Task<string> GetNewSongName();
        Task<string[]> GetExistingFileList();
        Task<bool> FileExists(string fileName);
        Task<bool> OpenFile(string fileName, bool forWriting);
        Task CloseFile(bool fileWasForWriting);
        string FileReadString();
        int FileReadInt();
        bool FileReadBool();
        float FileReadFloat();
        void FileWriteString(string val);
        void FileWriteInt(int val);
        void FileWriteBool(bool val);
        void FileWriteFloat(float val);

    }
}
