using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

using Windows.Storage;
using Microsoft.Phone.Storage;
using System.IO;
using System.Diagnostics;
using System.IO.IsolatedStorage;

namespace AnitroAPI
{
    public static class Storage
    {
        //public const string SETTINGS = "appSettings.hmb";
        public const string AVATARIMAGE = "avatar.jpg";

        public const string ANIMELIBRARY = "aniLibrary.hmb";
        public const string ANIMELIBRARY_CURRENTLYWATCHING = "aniLibrary_currentlyWatching.hmb";
        public const string ANIMELIBRARY_COMPLETED = "aniLibrary_completed.hmb";
        public const string ANIMELIBRARY_PLANTOWATCH = "aniLibrary_planToWatch.hmb";
        public const string ANIMELIBRARY_ONHOLD = "aniLibrary_onHold.hmb";
        public const string ANIMELIBRARY_DROPPED = "aniLibrary_dropped.hmb";
        public const string ANIMELIBRARY_FAVOURITES = "aniLibrary_favourites.hmb";

        public static bool isApplicationClosing = false;
        public static bool isSavingComplete = true;
        //public static bool settingsFileInUse = false;

        #region Storage Tools
        public static async Task<bool> WriteToStorage(string fileName, string content)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(content);

                StorageFolder folder = ApplicationData.Current.LocalFolder;
                StorageFile file = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);

                using (Stream s = await file.OpenStreamForWriteAsync())
                {
                    await s.WriteAsync(data, 0, data.Length);
                }
                Debug.WriteLine("Storage Saved: " + fileName);
                
                return true;
            }
            catch (Exception) { return false; }

        }

        public static async Task<string> ReadFileFromStorage(string fileName)
        {
            byte[] data;

            StorageFolder folder = ApplicationData.Current.LocalFolder;

            StorageFile file = await folder.GetFileAsync(fileName);
            using (Stream s = await file.OpenStreamForReadAsync())
            {
                data = new byte[s.Length];
                await s.ReadAsync(data, 0, (int)s.Length);
            }
            Debug.WriteLine(fileName + " Loaded");
            return Encoding.UTF8.GetString(data, 0, data.Length);
        }

        public static async Task<bool> SaveFileFromServer(Uri serverURI, string fileName)
        {
            try
            {
                using (IsolatedStorageFile storageFolder = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (IsolatedStorageFileStream stream = storageFolder.CreateFile(fileName))
                    {
                        Debug.WriteLine("Opening Client");
                        System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();

                        Debug.WriteLine("Grabbing File");
                        byte[] result = await client.GetByteArrayAsync(serverURI);
                        Storage.isSavingComplete = false;

                        Debug.WriteLine("Writing File");

                        await stream.WriteAsync(result, 0, result.Length);
                        Storage.isSavingComplete = true;

                        Debug.WriteLine("File Written");
                    }
                }
                return true;
            }
            catch (Exception) { return false; }
        }

        public static bool DoesFileExist(string fileName)
        {
            using (IsolatedStorageFile storageFolder = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (storageFolder.FileExists(fileName))
                    return true;
                else
                    return false;
            }
        }     

        public static bool DoesDirectoryExist(string folderPath)
        {
            using (IsolatedStorageFile storageFolder = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (storageFolder.DirectoryExists(folderPath))
                    return true;
                else
                    return false;
            }
        }


        public static bool DeleteDirectory(string folderPath)
        {
            if (!DoesDirectoryExist(folderPath))
                return true;

            using (IsolatedStorageFile storageFolder = IsolatedStorageFile.GetUserStoreForApplication())
            {
                storageFolder.DeleteDirectory(folderPath);
                return true;
            }
        }

        public static bool DeleteFile(string fileName)
        {
            if (!DoesFileExist(fileName))
                return true;

            using (IsolatedStorageFile storageFolder = IsolatedStorageFile.GetUserStoreForApplication())
            {
                storageFolder.DeleteFile(fileName);
                return true;
            }
        }

        public static bool DeleteAllFilesInDirectory(string folderPath)
        {
            try
            {
                using (IsolatedStorageFile storageFolder = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (!DoesDirectoryExist(folderPath))
                    {
                        storageFolder.CreateDirectory(folderPath);
                        return true;
                    }

                    string[] files = storageFolder.GetFileNames(folderPath);

                    foreach (string file in files)
                    {
                        storageFolder.DeleteFile(folderPath + file);
                    }

                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }

        }
        #endregion

        #region AnimeLibrary
        public async static Task<bool> LoadAnimeLibrary(string library)
        {
            //Task<string> infoRaw = ReadFileFromStorage(ANIMELIBRARY);
            try
            {
                switch (library)
                {
                    case "":
                        if (!DoesFileExist(ANIMELIBRARY)) return false;
                        break;
                    case "currently-watching":
                        if (!DoesFileExist(ANIMELIBRARY_CURRENTLYWATCHING)) return false;
                        break;
                    case "completed":
                        if (!DoesFileExist(ANIMELIBRARY_COMPLETED)) return false;
                        break;
                    case "plan-to-watch":
                        if (!DoesFileExist(ANIMELIBRARY_PLANTOWATCH)) return false;
                        break;
                    case "on-hold":
                        if (!DoesFileExist(ANIMELIBRARY_ONHOLD)) return false;
                        break;
                    case "dropped":
                        if (!DoesFileExist(ANIMELIBRARY_DROPPED)) return false;
                        break;
                    case "favourites":
                        if (!DoesFileExist(ANIMELIBRARY_FAVOURITES)) return false;
                        break;
                }

                Task<string> infoRaw;
                if (library == "currently-watching") infoRaw = ReadFileFromStorage(ANIMELIBRARY_CURRENTLYWATCHING);// infoRaw.Result;
                else if (library == "completed") infoRaw = ReadFileFromStorage(ANIMELIBRARY_COMPLETED);// infoRaw.Result;
                else if (library == "plan-to-watch") infoRaw = ReadFileFromStorage(ANIMELIBRARY_PLANTOWATCH);// infoRaw.Result;
                else if (library == "on-hold") infoRaw = ReadFileFromStorage(ANIMELIBRARY_ONHOLD);// infoRaw.Result;
                else if (library == "dropped") infoRaw = ReadFileFromStorage(ANIMELIBRARY_DROPPED);// infoRaw.Result;
                else if (library == "favourites") infoRaw = ReadFileFromStorage(ANIMELIBRARY_FAVOURITES);// infoRaw.Result;
                else infoRaw = ReadFileFromStorage(ANIMELIBRARY);
                
                await infoRaw;
                string info = infoRaw.Result;

                //Debug.WriteLine(info);
                Consts.ParseLibraries(info, library);

                return true;
            }
            catch (Exception e)
            {
                throw e;
            }

        }

        public async static Task<bool> SaveAnimeLibrary(string library)
        {
            Debug.WriteLine("SaveAnimeLibrary(): Entering");
            if (isApplicationClosing) return false;

            isSavingComplete = false;

            switch (library)
            {
                case "currently-watching":
                    while (!Consts.cwLoaded) { }
                    break;
                case "completed":
                    while (!Consts.cLoaded) { }
                    break;
                case "plan-to-watch":
                    while (!Consts.pTWLoaded) { }
                    break;
                case "on-hold":
                    while (!Consts.oHLoaded) { Debug.WriteLine("I'M STUCK"); }
                    break;
                case "dropped":
                    while (!Consts.dLoaded) { }
                    break;
                case "favourites":
                    //while (!Consts.fLoaded) { }
                    break;
                case "":
                default:
                    while (!Consts.IsLibraryLoaded(false)) { }
                    break;
            } // Wait for libraries to be loaded first

            Debug.WriteLine("Saving Library: "+ library);

            string saveableAnimeLibrary;// = Consts.GetParseableLibraries(library);

            switch (library)
            {
                case "":
                    saveableAnimeLibrary = Consts.GetParseableLibraries(library);
                    await WriteToStorage(ANIMELIBRARY, saveableAnimeLibrary);
                    break;
                case "currently-watching":
                    saveableAnimeLibrary = Consts.GetParseableLibraries(library);
                    await WriteToStorage(ANIMELIBRARY_CURRENTLYWATCHING, saveableAnimeLibrary);
                    break;
                case "completed":
                    saveableAnimeLibrary = Consts.GetParseableLibraries(library);
                    await WriteToStorage(ANIMELIBRARY_COMPLETED, saveableAnimeLibrary);
                    break;
                case "plan-to-watch":
                    saveableAnimeLibrary = Consts.GetParseableLibraries(library);
                    await WriteToStorage(ANIMELIBRARY_PLANTOWATCH, saveableAnimeLibrary);
                    break;
                case "on-hold":
                    saveableAnimeLibrary = Consts.GetParseableLibraries(library);
                    await WriteToStorage(ANIMELIBRARY_ONHOLD, saveableAnimeLibrary);
                    break;
                case "dropped":
                    saveableAnimeLibrary = Consts.GetParseableLibraries(library);
                    await WriteToStorage(ANIMELIBRARY_DROPPED, saveableAnimeLibrary);
                    break;
                case "favourites":
                    saveableAnimeLibrary = Consts.GetParseableLibraries(library);
                    await WriteToStorage(ANIMELIBRARY_FAVOURITES, saveableAnimeLibrary);
                    break;
                case "all":
                    SaveAnimeLibrary("currently-watching");
                    SaveAnimeLibrary("completed");
                    SaveAnimeLibrary("plan-to-watch");
                    SaveAnimeLibrary("on-hold");
                    SaveAnimeLibrary("dropped");
                    SaveAnimeLibrary("favourites");
                    break;
                default:
                    SaveAnimeLibrary("all");
                    break;
            }

            isSavingComplete = true;
            return true;
        }

        public static bool DeleteAnimeLibrary()
        {
            //DeleteFile(ANIMELIBRARY);
            DeleteFile(ANIMELIBRARY_CURRENTLYWATCHING);
            DeleteFile(ANIMELIBRARY_COMPLETED);
            DeleteFile(ANIMELIBRARY_PLANTOWATCH);
            DeleteFile(ANIMELIBRARY_ONHOLD);
            DeleteFile(ANIMELIBRARY_DROPPED);
            DeleteFile(ANIMELIBRARY_FAVOURITES);
            return (true);
        }
        #endregion

        #region Settings
        //public async static Task<bool> LoadSettingsInfo(bool createNewSettingsFileIfFailed = false)
        //{
        //    if (!DoesFileExist(SETTINGS)) 
        //    {
        //        if (createNewSettingsFileIfFailed) { await SaveSettingsInfo(); }
        //        return false; 
        //    }
        //    if (settingsFileInUse) return false;
        //    settingsFileInUse = true;

        //    string info;
        //    try
        //    {
        //        Task<string> infoRaw = ReadFileFromStorage(SETTINGS);
        //        await infoRaw;
        //        info = infoRaw.Result;
        //    }
        //    catch (Exception) { return false; }

        //    Consts.ParseSettings(info);
        //    Consts.HasAccessForLockscreen = Windows.Phone.System.UserProfile.LockScreenManager.IsProvidedByCurrentApplication;

        //    settingsFileInUse = false;
        //    return true;
        //}

        //public async static Task<bool> SaveSettingsInfo()
        //{
        //    if (isApplicationClosing) return false;
        //    if (settingsFileInUse) return false;

        //    isSavingComplete = false;
        //    settingsFileInUse = true;

        //    string settingsInfo = Consts.GetParseableSettings();
        //    await WriteToStorage(SETTINGS, settingsInfo);

        //    settingsFileInUse = false;
        //    isSavingComplete = true;
        //    return true;
        //}

        //public static bool DeleteSettingsInfo(bool createNewFile = false)
        //{
        //    DeleteFile(SETTINGS);

        //    if (createNewFile)
        //    {
        //        Consts.ClearSettingsInfo();
        //        SaveSettingsInfo();
        //    }
        //    return true;
        //}

        public static class Settings
        {
            public static class User
            {
                public static readonly IsolatedStorageProperty<string> userName = new IsolatedStorageProperty<string>("username", "");
                public static readonly IsolatedStorageProperty<string> authToken = new IsolatedStorageProperty<string>("auth token", "");
                public static readonly IsolatedStorageProperty<string> userAvatar = new IsolatedStorageProperty<string>("user avatar", "");
            }

            public static class Lockscreen
            {
                public static readonly IsolatedStorageProperty<bool> randomizeCurrentlyWatching = new IsolatedStorageProperty<bool>("lockscreen randomize Currently Watching", true);
                public static readonly IsolatedStorageProperty<bool> randomizePlanToWatch = new IsolatedStorageProperty<bool>("lockscreen randomize Plan To Watch", true);
                public static readonly IsolatedStorageProperty<bool> randomizeCompleted = new IsolatedStorageProperty<bool>("lockscreen randomize Completed", true);
                public static readonly IsolatedStorageProperty<bool> randomizeOnHold = new IsolatedStorageProperty<bool>("lockscreen randomize OnHold", true);
                public static readonly IsolatedStorageProperty<bool> randomizeDropped = new IsolatedStorageProperty<bool>("lockscreen randomize Dropped", true);
                public static readonly IsolatedStorageProperty<bool> randomizeFavourites = new IsolatedStorageProperty<bool>("lockscreen randomize Favourites", true);
            }

            public static readonly IsolatedStorageProperty<double> autoGenerateLibraryAfterXDays = new IsolatedStorageProperty<double>("libraryAutoRegenTime", 1.0);

            public static readonly IsolatedStorageProperty<DateTime> ScheduledTaskLastRun = new IsolatedStorageProperty<DateTime>("ScheduledTaskLastRun", new DateTime());
            public static readonly IsolatedStorageProperty<DateTime> libraryLastPulled = new IsolatedStorageProperty<DateTime>("libraryLastPulled", new DateTime());
            
        }
        #endregion
    }


    #region Isolated Storage Settings
    /// <summary>
    /// Helper class is needed because IsolatedStorageProperty is generic and 
    /// can not provide singleton model for static content
    /// </summary>
    internal static class IsolatedStoragePropertyHelper
    {
        /// <summary>
        /// We must use this object to lock saving settings
        /// </summary>
        public static readonly object ThreadLocker = new object();

        public static readonly IsolatedStorageSettings Store = IsolatedStorageSettings.ApplicationSettings;
    }

    /// <summary>
    /// This is wrapper class for storing one setting
    /// Object of this type must be single
    /// </summary>
    /// <typeparam name="T">Any serializable type</typeparam>
    public class IsolatedStorageProperty<T>
    {
        private readonly object _defaultValue;
        private readonly string _name;
        private readonly object _syncObject = new object();

        public IsolatedStorageProperty(string name, T defaultValue = default(T))
        {
            _name = name;
            _defaultValue = defaultValue;
        }

        /// <summary>
        /// Determines if setting exists in the storage
        /// </summary>
        public bool Exists
        {
            get { return IsolatedStoragePropertyHelper.Store.Contains(_name); }
        }

        /// <summary>
        /// Use this property to access the actual setting value
        /// </summary>
        public T Value
        {
            get
            {
                //If property does not exist - initializing it using default value
                if (!Exists)
                {
                    //Initializing only once
                    lock (_syncObject)
                    {
                        if (!Exists) SetDefault();
                    }
                }

                return (T)IsolatedStoragePropertyHelper.Store[_name];
            }
            set
            {
                IsolatedStoragePropertyHelper.Store[_name] = value;
                Save();
            }
        }

        private static void Save()
        {
            lock (IsolatedStoragePropertyHelper.ThreadLocker)
            {
                IsolatedStoragePropertyHelper.Store.Save();
            }
        }

        public bool IsDefault()
        {
            return (Value.ToString() == _defaultValue.ToString());
        }

        public void SetDefault()
        {
            Value = (T)_defaultValue;
        }
    }
    #endregion
}
