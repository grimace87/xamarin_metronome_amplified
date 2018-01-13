using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.Content;

using MetronomeAmplified.Classes;
using Java.IO;
using System.IO;
using System.Threading.Tasks;

namespace MetronomeAmplified.Droid
{
    public class PlatformStorage : IPlatformStorage
    {
        // Keys and such
        private static string STORAGE_NAME = "PREFS";
        private Context ThisContext;
        private ISharedPreferences Prefs;
        private DataOutputStream OutputStream;
        private DataInputStream InputStream;

        // Constructor
        public PlatformStorage(Context context)
        {
            ThisContext = context;
            Prefs = ThisContext.GetSharedPreferences(STORAGE_NAME, FileCreationMode.Private);
        }
        
        // Implemented functions for key-value storage
        public bool KeyExists(string key)
        {
            bool hasKey = Prefs.Contains(key);
            return hasKey;
        }
        public float GetFloat(string key)
        {
            return Prefs.GetFloat(key, 0.0f);
        }
        public void StoreFloat(string key, float value)
        {
            ISharedPreferencesEditor edit = Prefs.Edit();
            edit.PutFloat(key, value);
            edit.Commit();
        }
        public int GetInt(string key)
        {
            return Prefs.GetInt(key, 0);
        }
        public void StoreInt(string key, int value)
        {
            ISharedPreferencesEditor edit = Prefs.Edit();
            edit.PutInt(key, value);
            edit.Commit();
        }
        public bool GetBoolean(string key)
        {
            return Prefs.GetBoolean(key, false);
        }
        public void StoreBoolean(string key, bool value)
        {
            ISharedPreferencesEditor edit = Prefs.Edit();
            edit.PutBoolean(key, value);
            edit.Commit();
        }

        // Implemented functions for files
        public async Task<string> GetNewSongName()
        {
            int attempt = 0;
            string testName = "";
            while (true)
            {
                attempt++;
                testName = "Song" + attempt;
                Java.IO.File file = new Java.IO.File(testName);
                    if (file.Exists())
                        continue;
                break;
            }
            return testName;
        }
        public async Task<string[]> GetExistingFileList()
        {
            Java.IO.File[] files = ThisContext.GetDir("songs", FileCreationMode.Private).ListFiles();
            int count = files.Length;
            string[] fileNames = new string[count];
            for (int i = 0; i < count; i++)
                fileNames[i] = files[i].Name;
            return fileNames;
        }
        public async Task<bool> FileExists(string fileName)
        {
            Java.IO.File file = new Java.IO.File(fileName);
            return file.Exists();
        }
        public async Task<bool> OpenFile(string fileName, bool forWriting)
        {
            Java.IO.File dir = ThisContext.GetDir("songs", FileCreationMode.Private);
            Java.IO.File file = new Java.IO.File(dir, fileName);
            if (file.Exists() == false)
                file.CreateNewFile();
            Stream stream;
            if (forWriting)
            {
                stream = ThisContext.OpenFileOutput(fileName, FileCreationMode.Private);
                OutputStream = new DataOutputStream(stream);
            }
            else
            {
                try
                {
                    stream = ThisContext.OpenFileInput(fileName);
                    InputStream = new DataInputStream(stream);
                }
                catch (Java.IO.FileNotFoundException e)
                {
                    return false;
                }
            }
            return true;
        }
        public async Task<bool> DeleteFile(string fileName)
        {
            Java.IO.File dir = ThisContext.GetDir("songs", FileCreationMode.Private);
            Java.IO.File file = new Java.IO.File(dir, fileName);
            return file.Delete();
        }
        public async Task CloseFile(bool fileWasForWriting)
        {
            if (fileWasForWriting)
                OutputStream.Close();
            else
                InputStream.Close();
        }
        public string FileReadString()
        {
            return InputStream.ReadUTF();
        }
        public int FileReadInt()
        {
            return InputStream.ReadInt();
        }
        public bool FileReadBool()
        {
            return InputStream.ReadBoolean();
        }
        public float FileReadFloat()
        {
            return InputStream.ReadFloat();
        }
        public void FileWriteString(string val)
        {
            OutputStream.WriteUTF(val);
        }
        public void FileWriteInt(int val)
        {
            OutputStream.WriteInt(val);
        }
        public void FileWriteBool(bool val)
        {
            OutputStream.WriteBoolean(val);
        }
        public void FileWriteFloat(float val)
        {
            OutputStream.WriteFloat(val);
        }
    }
}