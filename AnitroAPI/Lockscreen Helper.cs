using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Navigation;
using Windows.Phone.System.UserProfile;

namespace AnitroAPI
{
    public class Lockscreen_Helper
    {
        private const string BackgroundRoot = ""; 
        private const string LOCKSCREEN_IMAGE = "lockscreen.jpg";
        private static int count = -1;
        private static Random random = new Random();

        public static bool DeleteLockscreenImage()
        {
            Storage.DeleteFile(LOCKSCREEN_IMAGE);
            return true;
        }

        public static async Task SetRandomImageFromLibrary()
        {
            if (!LockScreenManager.IsProvidedByCurrentApplication) { return; }
            if ((!Storage.Settings.Lockscreen.randomizeCurrentlyWatching.Value &&
                 !Storage.Settings.Lockscreen.randomizePlanToWatch.Value &&
                 !Storage.Settings.Lockscreen.randomizeCompleted.Value &&
                 !Storage.Settings.Lockscreen.randomizeOnHold.Value &&
                 !Storage.Settings.Lockscreen.randomizeDropped.Value &&
                 !Storage.Settings.Lockscreen.randomizeFavourites.Value)) { Debug.WriteLine("All Libraries false, exiting early."); return; }
            if (count >= 10) { return; }
            count++;

            //===========================================================================================================\\

            if (!Consts.IsLibraryLoaded(false)) 
            {
                if (Storage.Settings.Lockscreen.randomizeCurrentlyWatching.Value ||
                    Storage.Settings.Lockscreen.randomizePlanToWatch.Value ||
                    Storage.Settings.Lockscreen.randomizeCompleted.Value ||
                    Storage.Settings.Lockscreen.randomizeOnHold.Value ||
                    Storage.Settings.Lockscreen.randomizeDropped.Value)
                {
                    await Storage.LoadAnimeLibrary("");
                }
            }

            List<LibraryObject> tempList = new List<LibraryObject>();
            if (Storage.Settings.Lockscreen.randomizeCurrentlyWatching.Value)
            {
                foreach (LibraryObject lO in Consts.currentlyWatching)
                {
                    tempList.Add(lO);
                }
            }
            if (Storage.Settings.Lockscreen.randomizePlanToWatch.Value)
            {
                foreach (LibraryObject lO in Consts.planToWatch)
                {
                    tempList.Add(lO);
                }
            }
            if (Storage.Settings.Lockscreen.randomizeCompleted.Value)
            {
                foreach (LibraryObject lO in Consts.completed)
                {
                    tempList.Add(lO);
                }
            }
            if (Storage.Settings.Lockscreen.randomizeOnHold.Value)
            {
                foreach (LibraryObject lO in Consts.onHold)
                {
                    tempList.Add(lO);
                }
            }
            if (Storage.Settings.Lockscreen.randomizeDropped.Value)
            {
                foreach (LibraryObject lO in Consts.dropped)
                {
                    tempList.Add(lO);
                }
            }
            if (Storage.Settings.Lockscreen.randomizeFavourites.Value)
            {
                if (!Consts.fLoaded) { await Storage.LoadAnimeLibrary("favourites"); }

                foreach (LibraryObject lO in Consts.favourites)
                {
                    tempList.Add(lO);
                }
            }

            #region oldCode
            //switch (random.Next(2)) //Old code: 6
            //{
            //    case 0:
            //        //if (!Consts.IsLibraryLoaded(false))
            //        //{
            //        //    await Storage.LoadAnimeLibrary("");
            //        //}

            //        switch (random.Next(5))
            //        {
            //            //case 0:
            //            //    if (Storage.Settings.Lockscreen.randomizeCurrentlyWatching.Value)
            //            //    {
            //            //        tempList = Consts.currentlyWatching.ToList();
            //            //    }
            //            //    break;
            //            //case 1:
            //            //    if (Storage.Settings.Lockscreen.randomizePlanToWatch.Value)
            //            //    {
            //            //        tempList = Consts.currentlyWatching.ToList();
            //            //    }
            //            //    break;
            //            //case 2:
            //            //    if (Storage.Settings.Lockscreen.randomizeCompleted.Value)
            //            //    {
            //            //        tempList = Consts.currentlyWatching.ToList();
            //            //    }
            //            //    break;
            //            //case 3:
            //            //    if (Storage.Settings.Lockscreen.randomizeOnHold.Value)
            //            //    {
            //            //        tempList = Consts.currentlyWatching.ToList();
            //            //    }
            //            //    break;
            //            //case 4:
            //            //    if (Storage.Settings.Lockscreen.randomizeDropped.Value)
            //            //    {
            //            //        tempList = Consts.currentlyWatching.ToList();
            //            //    }
            //            //    break;
            //        }
            //        break;
            //    #region oldcode
            //    //    if (Storage.Settings.Lockscreen.randomizeCurrentlyWatching.Value)
            //    //    {
            //    //        if (!Consts.cwLoaded)
            //    //        {
            //    //            await Storage.LoadAnimeLibrary("currently-watching");
            //    //        }
            //    //        tempList = Consts.currentlyWatching.ToList();
            //    //    }
            //    //    break;
            //    //case 1:
            //    //    if (Storage.Settings.Lockscreen.randomizePlanToWatch.Value)
            //    //    {
            //    //        if (!Consts.pTWLoaded)
            //    //        {
            //    //            await Storage.LoadAnimeLibrary("plan-to-watch");
            //    //        }
            //    //        tempList = Consts.planToWatch.ToList();
            //    //    }
            //    //    break;
            //    //case 2:
            //    //    if (Storage.Settings.Lockscreen.randomizeCompleted.Value)
            //    //    {
            //    //        if (!Consts.cLoaded)
            //    //        {
            //    //            await Storage.LoadAnimeLibrary("completed");
            //    //        }
            //    //        tempList = Consts.completed.ToList();
            //    //    }
            //    //    break;
            //    //case 3:
            //    //    if (Storage.Settings.Lockscreen.randomizeOnHold.Value)
            //    //    {
            //    //        if (!Consts.oHLoaded)
            //    //        {
            //    //            await Storage.LoadAnimeLibrary("on-hold");
            //    //        }
            //    //        tempList = Consts.onHold.ToList();
            //    //    }
            //    //    break;
            //    //case 4:
            //    //    if (Storage.Settings.Lockscreen.randomizeDropped.Value)
            //    //    {
            //    //        if (!Consts.dLoaded)
            //    //        {
            //    //            await Storage.LoadAnimeLibrary("dropped");
            //    //        }
            //    //        tempList = Consts.dropped.ToList();
            //    //    }
            //    //    break;
            //    #endregion
            //    case 1:
            //        //if (Storage.Settings.Lockscreen.randomizeFavourites.Value)
            //        //{
            //        //    //if (!Consts.fLoaded)
            //        //    //{
            //        //    //    await Storage.LoadAnimeLibrary("favourites");
            //        //    //}
            //        //    tempList = Consts.favourites.ToList();
            //        //}
            //        break;
            //}
            #endregion

            if (tempList == null || tempList.Count == 0) { Debug.WriteLine("List is empty. Returning."); return; }
            else
            {
                LibraryObject tempLO = tempList[random.Next(tempList.Count)];

                Debug.WriteLine("Anime Selected: "+tempLO.anime.slug);
                await SetImage(tempLO.anime.cover_image_uri);
            }
        }

        public static async Task SetImage(Uri uri)
        {
            //First Delete Old image
            DeleteLockscreenImage();

            Debug.WriteLine("Image Path: " + uri.OriginalString);

            string fileName = uri.Segments[uri.Segments.Length - 1];
            string imageName = BackgroundRoot + fileName;

            using (IsolatedStorageFile storageFolder = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream stream = storageFolder.CreateFile(LOCKSCREEN_IMAGE))
                {
                    Debug.WriteLine("Opening Client");
                    HttpClient client = new HttpClient();

                    Debug.WriteLine("Grabbing File");
                    byte[] hummingbirdResult = await client.GetByteArrayAsync(uri);
                    Storage.isSavingComplete = false;

                    Debug.WriteLine("Writing File");

                    await stream.WriteAsync(hummingbirdResult, 0, hummingbirdResult.Length);
                    Storage.isSavingComplete = true;

                    Debug.WriteLine("File Written");
                }
            }

            await SetLockScreen();
        }

        public static async Task SetLockScreen()
        {
            bool hasAccessForLockScreen = LockScreenManager.IsProvidedByCurrentApplication;

            if (!hasAccessForLockScreen)
            {
                var accessRequested = await LockScreenManager.RequestAccessAsync();
                hasAccessForLockScreen = (accessRequested == LockScreenRequestResult.Granted);

                Consts.HasAccessForLockscreen = hasAccessForLockScreen;
            }

            if (hasAccessForLockScreen)
            {
                bool isAppResource = true;
                string filePathOfTheImage = "Assets/defaultLockscreenBackground.png";
                var schema = isAppResource ? "ms-appx:///" : "ms-appdata:///Local/";
                var uri = new Uri(schema + filePathOfTheImage, UriKind.Absolute);
                LockScreen.SetImageUri(uri);
                Thread.Sleep(TimeSpan.FromSeconds(2));

                Uri imgUri = new Uri("ms-appdata:///local/" + LOCKSCREEN_IMAGE, UriKind.Absolute);

                LockScreen.SetImageUri(imgUri);
                for (int i = 0; i < 3; i++)
                {
                    LockScreen.SetImageUri(imgUri);
                }

                Debug.WriteLine("Lockscreen Image Set");
            }
        }
    }
}
