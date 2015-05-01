using System;
using System.Net;
using System.Windows;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.NetworkInformation;

namespace AnitroAPI
{
    public static class Consts
    {
        public static string APP_NAME = "Anitro";

        public static string testAccountUsername = "killerrin";
        public static string testAccountPassword = "";

        public static bool LoggedIn = false;
        public static bool HasAccessForLockscreen;


        // Mashape Key for API Calls    | Testing Key: "JIyg90lZ0KRmT0qivz8ECXjvl0rd18lS";
        public const string MASHAPE_KEY = "TkLbJdjaFrDjcjuGrKc5XvJREP0pgnYs";

        // Enumerators
        public enum LoginError
        {
            None,
            InvalidLogin,
            ServerError,
            InfoNotEntered,
            NetworkError,
            UnknownError
        }

        public enum LibrarySelection
        {
            None,
            All,
            CurrentlyWatching,
            PlanToWatch,
            Completed,
            OnHold,
            Dropped,
            Favourites
        }

        // Anime List
        public static ObservableCollection<LibraryObject> currentlyWatching = new ObservableCollection<LibraryObject>(); //public static List<LibraryObject> currentlyWatching = new List<LibraryObject>();
        public static ObservableCollection<LibraryObject> planToWatch = new ObservableCollection<LibraryObject>();//public static List<LibraryObject> planToWatch = new List<LibraryObject>();
        public static ObservableCollection<LibraryObject> completed = new ObservableCollection<LibraryObject>();//public static List<LibraryObject> completed = new List<LibraryObject>();
        public static ObservableCollection<LibraryObject> onHold = new ObservableCollection<LibraryObject>();//public static List<LibraryObject> onHold = new List<LibraryObject>();
        public static ObservableCollection<LibraryObject> dropped = new ObservableCollection<LibraryObject>();//public static List<LibraryObject> dropped = new List<LibraryObject>();
        public static ObservableCollection<LibraryObject> favourites = new ObservableCollection<LibraryObject>();//public static List<LibraryObject> dropped = new List<LibraryObject>();
        public static ObservableCollection<ActivityFeedObject> activityFeed = new ObservableCollection<ActivityFeedObject>();

        public static bool cwLoaded;
        public static bool pTWLoaded;
        public static bool cLoaded;
        public static bool oHLoaded;
        public static bool dLoaded;
        public static bool fLoaded;
        public static bool aFLoaded;

        #region Helper Methods
        public static bool IsConnectedToInternet()
        {

            return NetworkInterface.GetIsNetworkAvailable();
        }
        #endregion

        #region Library Helper Methods
        public static void AddToFavourites(LibraryObject temp, bool save = true)
        {
            if (DoesExistInLibrary(LibrarySelection.Favourites, temp)) { return; }

            favourites.Add(temp);
            if (save) Storage.SaveAnimeLibrary("favourites");
        }

        public static void UpdateFavourites(LibraryObject temp, bool save = true)
        {
            if (!DoesExistInLibrary(LibrarySelection.Favourites, temp)) { return; }

            for (int i = 0; i < favourites.Count; i++)
            {
                if (temp.anime.slug == favourites[i].anime.slug) 
                {
                    favourites[i] = temp;
                    if (save) Storage.SaveAnimeLibrary("favourites");
                    return; 
                }
            }

        }

        public static void RemoveFromFavourites(LibraryObject temp)
        {
            if (!DoesExistInLibrary(LibrarySelection.Favourites,temp)){ return; }

            //favourites.Remove(temp);
            for (int i = 0; i < favourites.Count; i++)
            {
                if (temp.anime.slug == favourites[i].anime.slug) { favourites.RemoveAt(i); break; }
            }
            Storage.SaveAnimeLibrary("favourites");
        }
        
        public static bool DoesExistInLibrary(LibrarySelection selection, LibraryObject temp)
        {
            switch (selection)
            {
                case LibrarySelection.CurrentlyWatching:
                    foreach (LibraryObject lO in currentlyWatching)
                    {
                        if (temp.anime.slug == lO.anime.slug) { return true; }
                    }
                    break;
                case LibrarySelection.PlanToWatch:
                    foreach (LibraryObject lO in planToWatch)
                    {
                        if (temp.anime.slug == lO.anime.slug) { return true; }
                    }
                    break;
                case LibrarySelection.Completed:
                    foreach (LibraryObject lO in completed)
                    {
                        if (temp.anime.slug == lO.anime.slug) { return true; }
                    }
                    break;
                case LibrarySelection.OnHold:
                    foreach (LibraryObject lO in onHold)
                    {
                        if (temp.anime.slug == lO.anime.slug) { return true; }
                    }
                    break;
                case LibrarySelection.Dropped:
                    foreach (LibraryObject lO in dropped)
                    {
                        if (temp.anime.slug == lO.anime.slug) { return true; }
                    }
                    break;
                case LibrarySelection.Favourites:
                    foreach (LibraryObject lO in favourites)
                    {
                        if (temp.anime.slug == lO.anime.slug) { return true; }
                    }
                    break;
                case LibrarySelection.All:
                    foreach (LibraryObject lO in currentlyWatching)
                    {
                        if (temp.anime.slug == lO.anime.slug) { return true; }
                    }
                    foreach (LibraryObject lO in planToWatch)
                    {
                        if (temp.anime.slug == lO.anime.slug) { return true; }
                    }
                    foreach (LibraryObject lO in completed)
                    {
                        if (temp.anime.slug == lO.anime.slug) { return true; }
                    }
                    foreach (LibraryObject lO in onHold)
                    {
                        if (temp.anime.slug == lO.anime.slug) { return true; }
                    }
                    foreach (LibraryObject lO in dropped)
                    {
                        if (temp.anime.slug == lO.anime.slug) { return true; }
                    }
                    foreach (LibraryObject lO in favourites)
                    {
                        if (temp.anime.slug == lO.anime.slug) { return true; }
                    }
                    break;
                case LibrarySelection.None:
                    break;
                default: break;
            }

            return false;
        }
        public static bool DoesExistInLibrary(LibrarySelection selection, string slug)
        {
            switch (selection)
            {
                case LibrarySelection.CurrentlyWatching:
                    foreach (LibraryObject lO in currentlyWatching)
                    {
                        if (slug == lO.anime.slug) { return true; }
                    }
                    break;
                case LibrarySelection.PlanToWatch:
                    foreach (LibraryObject lO in planToWatch)
                    {
                        if (slug == lO.anime.slug) { return true; }
                    }
                    break;
                case LibrarySelection.Completed:
                    foreach (LibraryObject lO in completed)
                    {
                        if (slug == lO.anime.slug) { return true; }
                    }
                    break;
                case LibrarySelection.OnHold:
                    foreach (LibraryObject lO in onHold)
                    {
                        if (slug == lO.anime.slug) { return true; }
                    }
                    break;
                case LibrarySelection.Dropped:
                    foreach (LibraryObject lO in dropped)
                    {
                        if (slug == lO.anime.slug) { return true; }
                    }
                    break;
                case LibrarySelection.Favourites:
                    foreach (LibraryObject lO in favourites)
                    {
                        if (slug == lO.anime.slug) { return true; }
                    }
                    break;
                case LibrarySelection.All:
                    foreach (LibraryObject lO in currentlyWatching)
                    {
                        if (slug == lO.anime.slug) { return true; }
                    }
                    foreach (LibraryObject lO in planToWatch)
                    {
                        if (slug == lO.anime.slug) { return true; }
                    }
                    foreach (LibraryObject lO in completed)
                    {
                        if (slug == lO.anime.slug) { return true; }
                    }
                    foreach (LibraryObject lO in onHold)
                    {
                        if (slug == lO.anime.slug) { return true; }
                    }
                    foreach (LibraryObject lO in dropped)
                    {
                        if (slug == lO.anime.slug) { return true; }
                    }
                    foreach (LibraryObject lO in favourites)
                    {
                        if (slug == lO.anime.slug) { return true; }
                    }
                    break;
                case LibrarySelection.None:
                    break;
                default: break;
            }

            return false;
        }

        public static int IndexInLibrary(LibrarySelection selection, LibraryObject temp)
        {
            if (!DoesExistInLibrary(selection, temp)) { return -1; }

            switch (selection)
            {
                case LibrarySelection.CurrentlyWatching:
                    for (int i = 0; i < currentlyWatching.Count; i++)
                    {
                        if (temp.anime.slug == currentlyWatching[i].anime.slug) { return i; }
                    }
                    break;
                case LibrarySelection.PlanToWatch:
                    for (int i = 0; i < planToWatch.Count; i++)
                    {
                        if (temp.anime.slug == planToWatch[i].anime.slug) { return i; }
                    }
                    break;
                case LibrarySelection.Completed:
                    for (int i = 0; i < completed.Count; i++)
                    {
                        if (temp.anime.slug == completed[i].anime.slug) { return i; }
                    }
                    break;
                case LibrarySelection.OnHold:
                    for (int i = 0; i < onHold.Count; i++)
                    {
                        if (temp.anime.slug == onHold[i].anime.slug) { return i; }
                    }
                    break;
                case LibrarySelection.Dropped:
                    for (int i = 0; i < dropped.Count; i++)
                    {
                        if (temp.anime.slug == dropped[i].anime.slug) { return i; }
                    }
                    break;
                case LibrarySelection.Favourites:
                    for (int i = 0; i < favourites.Count; i++)
                    {
                        if (temp.anime.slug == favourites[i].anime.slug) { return i; }
                    }
                    break;
                default: break;
            }

            return -1;
        }

        public static LibrarySelection FindWhereExistsInLibrary(LibraryObject temp)
        {
            string slug = temp.anime.slug;
            foreach (LibraryObject lO in Consts.currentlyWatching)
            {
                if (lO.anime.slug == slug) { return LibrarySelection.CurrentlyWatching; }
            }
            foreach (LibraryObject lO in Consts.planToWatch)
            {
                if (lO.anime.slug == slug) { return LibrarySelection.PlanToWatch; }
            }
            foreach (LibraryObject lO in Consts.completed)
            {
                if (lO.anime.slug == slug) { return LibrarySelection.Completed; }
            }
            foreach (LibraryObject lO in Consts.onHold)
            {
                if (lO.anime.slug == slug) { return LibrarySelection.OnHold; }
            }
            foreach (LibraryObject lO in Consts.dropped)
            {
                if (lO.anime.slug == slug) { return LibrarySelection.Dropped; }
            }
            foreach (LibraryObject lO in Consts.favourites)
            {
                if (lO.anime.slug == slug) { return LibrarySelection.Favourites; }
            }
            return LibrarySelection.None;
        }
        public static LibrarySelection FindWhereExistsInLibrary(string slug)
        {
            foreach (LibraryObject lO in Consts.currentlyWatching)
            {
                if (lO.anime.slug == slug) { return LibrarySelection.CurrentlyWatching; }
            }
            foreach (LibraryObject lO in Consts.planToWatch)
            {
                if (lO.anime.slug == slug) { return LibrarySelection.PlanToWatch; }
            }
            foreach (LibraryObject lO in Consts.completed)
            {
                if (lO.anime.slug == slug) { return LibrarySelection.Completed; }
            }
            foreach (LibraryObject lO in Consts.onHold)
            {
                if (lO.anime.slug == slug) { return LibrarySelection.OnHold; }
            }
            foreach (LibraryObject lO in Consts.dropped)
            {
                if (lO.anime.slug == slug) { return LibrarySelection.Dropped; }
            }
            foreach (LibraryObject lO in Consts.favourites)
            {
                if (lO.anime.slug == slug) { return LibrarySelection.Favourites; }
            }
            return LibrarySelection.None;
        }

        public static LibraryObject GetObjectInLibrary(LibrarySelection selection, string slug)
        {
            switch (selection)
            {
                case LibrarySelection.CurrentlyWatching:
                    foreach (LibraryObject lO in currentlyWatching)
                    {
                        if (slug == lO.anime.slug) { return lO; }
                    }
                    break;
                case LibrarySelection.PlanToWatch:
                    foreach (LibraryObject lO in planToWatch)
                    {
                        if (slug == lO.anime.slug) { return lO; }
                    }
                    break;
                case LibrarySelection.Completed:
                    foreach (LibraryObject lO in completed)
                    {
                        if (slug == lO.anime.slug) { return lO; }
                    }
                    break;
                case LibrarySelection.OnHold:
                    foreach (LibraryObject lO in onHold)
                    {
                        if (slug == lO.anime.slug) { return lO; }
                    }
                    break;
                case LibrarySelection.Dropped:
                    foreach (LibraryObject lO in dropped)
                    {
                        if (slug == lO.anime.slug) { return lO; }
                    }
                    break;
                case LibrarySelection.Favourites:
                    foreach (LibraryObject lO in favourites)
                    {
                        if (slug == lO.anime.slug) { return lO; }
                    }
                    break;
                case LibrarySelection.All:
                    foreach (LibraryObject lO in currentlyWatching)
                    {
                        if (slug == lO.anime.slug) { return lO; }
                    }
                    foreach (LibraryObject lO in planToWatch)
                    {
                        if (slug == lO.anime.slug) { return lO; }
                    }
                    foreach (LibraryObject lO in completed)
                    {
                        if (slug == lO.anime.slug) { return lO; }
                    }
                    foreach (LibraryObject lO in onHold)
                    {
                        if (slug == lO.anime.slug) { return lO; }
                    }
                    foreach (LibraryObject lO in dropped)
                    {
                        if (slug == lO.anime.slug) { return lO; }
                    }
                    foreach (LibraryObject lO in favourites)
                    {
                        if (slug == lO.anime.slug) { return lO; }
                    }
                    break;

                case LibrarySelection.None:
                    break;
                default: break;
            }
            return new LibraryObject();
        }

        public static bool AddToLibrary(LibrarySelection selection, LibraryObject temp, bool insertAtZero = true, bool save = true)
        {
            Debug.WriteLine("AddToLibrary(): Entering");
            if (!DoesExistInLibrary(selection, temp))
            {
                switch (selection)
                {
                    case LibrarySelection.CurrentlyWatching:
                        if (insertAtZero) currentlyWatching.Insert(0, temp);
                        else currentlyWatching.Add(temp);

                        if (save) Storage.SaveAnimeLibrary("currently-watching");
                        break;
                    case LibrarySelection.PlanToWatch:
                        if (insertAtZero) planToWatch.Insert(0, temp);
                        else planToWatch.Add(temp);

                        if (save) Storage.SaveAnimeLibrary("plan-to-watch");
                        break;
                    case LibrarySelection.Completed:
                        if (insertAtZero) completed.Insert(0, temp);
                        else completed.Add(temp);

                        if (save) Storage.SaveAnimeLibrary("completed");
                        break;
                    case LibrarySelection.OnHold:
                        if (insertAtZero) onHold.Insert(0, temp);
                        else onHold.Add(temp);

                        if (save) Storage.SaveAnimeLibrary("on-hold");
                        break;
                    case LibrarySelection.Dropped:
                        if (insertAtZero) dropped.Insert(0, temp);
                        else dropped.Add(temp);

                        if (save) Storage.SaveAnimeLibrary("dropped");
                        break;
                    case LibrarySelection.Favourites:
                        AddToFavourites(temp, save);
                        break;
                    case LibrarySelection.None: Debug.WriteLine("AddToLibrary(): Exiting"); return false;
                    default: Debug.WriteLine("AddToLibrary(): Exiting"); return false;
                }

                UpdateFavourites(temp, save);

                Debug.WriteLine("AddToLibrary(): Exiting");
                return true;
            }
            else
            {
                UpdateLibrary(selection, temp, save);
                return true;            
            }
        }

        public static bool AddGenresToLibraryObject(LibrarySelection selection, LibraryObject temp, List<Genre> _genres, bool save = true)
        {
            if (!DoesExistInLibrary(selection, temp)) { return false; }
            int index = IndexInLibrary(selection, temp);

            switch (selection)
            {
                case LibrarySelection.CurrentlyWatching:
                    currentlyWatching[index].anime.genres = _genres;
                    break;
                case LibrarySelection.PlanToWatch:
                    planToWatch[index].anime.genres = _genres;
                    break;
                case LibrarySelection.Completed:
                    completed[index].anime.genres = _genres;
                    break;
                case LibrarySelection.OnHold:
                    onHold[index].anime.genres = _genres;
                    break;
                case LibrarySelection.Dropped:
                    dropped[index].anime.genres = _genres;
                    break;
                case LibrarySelection.Favourites:
                    favourites[index].anime.genres = _genres;
                    break;
                default: break;
            }

            return true;
        }

        public static bool UpdateLibrary(LibrarySelection selection, LibraryObject temp, bool save = true)
        {
            if (DoesExistInLibrary(selection, temp))
            {
                switch (selection)
                {
                    case LibrarySelection.CurrentlyWatching:
                        for (int i = 0; i < currentlyWatching.Count; i++)
                        {
                            if (temp.anime.slug == currentlyWatching[i].anime.slug)
                            {
                                currentlyWatching[i] = temp;
                                if (save) Storage.SaveAnimeLibrary(""); //"currently-watching");

                                UpdateFavourites(temp, save);
                                return true;
                            }
                        }
                        break;
                    case LibrarySelection.PlanToWatch:
                        for (int i = 0; i < planToWatch.Count; i++)
                        {
                            if (temp.anime.slug == planToWatch[i].anime.slug)
                            {
                                planToWatch[i] = temp;
                                if (save) Storage.SaveAnimeLibrary(""); //"plan-to-watch");

                                UpdateFavourites(temp, save);
                                return true;
                            }
                        }
                        break;
                    case LibrarySelection.Completed:
                        for (int i = 0; i < completed.Count; i++)
                        {
                            if (temp.anime.slug == completed[i].anime.slug)
                            {
                                completed[i] = temp;
                                if (save) Storage.SaveAnimeLibrary(""); //"completed");

                                UpdateFavourites(temp, save);
                                return true;
                            }
                        }
                        break;
                    case LibrarySelection.OnHold:
                        for (int i = 0; i < onHold.Count; i++)
                        {
                            if (temp.anime.slug == onHold[i].anime.slug)
                            {
                                onHold[i] = temp;
                                if (save) Storage.SaveAnimeLibrary(""); //"on-hold");

                                UpdateFavourites(temp, save);
                                return true;
                            }
                        }
                        break;
                    case LibrarySelection.Dropped:
                        for (int i = 0; i < dropped.Count; i++)
                        {
                            if (temp.anime.slug == dropped[i].anime.slug)
                            {
                                dropped[i] = temp;
                                if (save) Storage.SaveAnimeLibrary(""); //"dropped");

                                UpdateFavourites(temp, save);
                                return true;
                            }
                        }
                        break;
                    case LibrarySelection.Favourites:
                        UpdateFavourites(temp, save);
                        return true;
                    case LibrarySelection.None: break;
                    default: break;
                }
            }
            return false;
        }

        public static bool RemoveFromLibrary(LibrarySelection selection, LibraryObject temp)
        {
            if (!DoesExistInLibrary(selection, temp)) { return false; }

            switch (selection)
            {
                case LibrarySelection.CurrentlyWatching:
                    for (int i = 0; i < currentlyWatching.Count; i++)
                    {
                        if (temp.anime.slug == currentlyWatching[i].anime.slug) { currentlyWatching.RemoveAt(i); break; }
                    }
                    Storage.SaveAnimeLibrary(""); //"currently-watching");
                    UpdateFavourites(temp);
                    return true;
                    break;
                case LibrarySelection.PlanToWatch:
                    for (int i = 0; i < planToWatch.Count; i++)
                    {
                        if (temp.anime.slug == planToWatch[i].anime.slug) { planToWatch.RemoveAt(i); break; }
                    }
                    Storage.SaveAnimeLibrary(""); //"plan-to-watch");
                    UpdateFavourites(temp);
                    return true;
                    break;
                case LibrarySelection.Completed:
                    for (int i = 0; i < completed.Count; i++)
                    {
                        if (temp.anime.slug == completed[i].anime.slug) { completed.RemoveAt(i); break; }
                    }
                    Storage.SaveAnimeLibrary(""); //"completed");
                    UpdateFavourites(temp);
                    return true;
                    break;
                case LibrarySelection.OnHold:
                    for (int i = 0; i < onHold.Count; i++)
                    {
                        if (temp.anime.slug == onHold[i].anime.slug) { onHold.RemoveAt(i); break; }
                    }
                    Storage.SaveAnimeLibrary(""); //"on-hold");
                    UpdateFavourites(temp);
                    return true;
                    break;
                case LibrarySelection.Dropped:
                    for (int i = 0; i < dropped.Count; i++)
                    {
                        if (temp.anime.slug == dropped[i].anime.slug) { dropped.RemoveAt(i); break; }
                    }
                    UpdateFavourites(temp);
                    Storage.SaveAnimeLibrary(""); //"dropped");
                    return true;
                    break;
                case LibrarySelection.Favourites:
                    Consts.RemoveFromFavourites(temp);
                    return true;
                    break;
                default:
                    break;
            }
            
            return false;
        }

        public static bool SwitchLibraries(LibrarySelection switchToLibrary, LibraryObject temp)
        {
            LibrarySelection whereInLibrary = FindWhereExistsInLibrary(temp);
            if (whereInLibrary == LibrarySelection.None || whereInLibrary == LibrarySelection.Favourites || temp.status == "search")  // Check if its currently in any of the libraries
            {
                LibrarySelection newSelection = GetLibrarySelectionFromStatus(temp);
                if (newSelection != LibrarySelection.None && newSelection != LibrarySelection.Favourites && temp.status != "search") { AddToLibrary(newSelection, temp); } // Check if the new value is able to be added to the library
                return false; 
            }
            if (DoesExistInLibrary(switchToLibrary, temp)) { return true; }

            RemoveFromLibrary(whereInLibrary, temp);
            AddToLibrary(switchToLibrary,temp);

            UpdateFavourites(temp);

            return true;
        }

        public static LibrarySelection GetLibrarySelectionFromStatus(LibraryObject lO)
        {
            switch(lO.status)
            {
                case "currently-watching":
                    return LibrarySelection.CurrentlyWatching;
                case "plan-to-watch":
                    return LibrarySelection.PlanToWatch;
                case "completed":
                    return LibrarySelection.Completed;
                case "on-hold":
                    return LibrarySelection.OnHold;
                case "dropped":
                    return LibrarySelection.Dropped;
                case "favourites":
                    return LibrarySelection.Favourites;
                case "all":
                    return LibrarySelection.All;
            }
            return LibrarySelection.None;
        }
        public static LibrarySelection GetLibrarySelectionFromStatus(string status)
        {
            switch (status)
            {
                case "currently-watching":
                    return LibrarySelection.CurrentlyWatching;
                case "plan-to-watch":
                    return LibrarySelection.PlanToWatch;
                case "completed":
                    return LibrarySelection.Completed;
                case "on-hold":
                    return LibrarySelection.OnHold;
                case "dropped":
                    return LibrarySelection.Dropped;
                case "favourites":
                    return LibrarySelection.Favourites;
                case "all":
                    return LibrarySelection.All;
            }
            return LibrarySelection.None;
        }
        public static string GetStatusFromLibrarySelection(LibrarySelection lS)
        {
            switch (lS)
            {
                case LibrarySelection.CurrentlyWatching:
                    return "currently-watching";
                case LibrarySelection.PlanToWatch:
                    return "plan-to-watch";
                case LibrarySelection.Completed:
                    return "completed";
                case LibrarySelection.OnHold:
                    return "on-hold";
                case LibrarySelection.Dropped:
                    return "dropped";
                case LibrarySelection.Favourites:
                case LibrarySelection.None:
                default:
                    return "";
            }
        }

        public static void ClearLibrary(LibrarySelection selection)
        {
            switch (selection)
            {
                case LibrarySelection.CurrentlyWatching:
                    currentlyWatching = new ObservableCollection<LibraryObject>();
                    break;
                case LibrarySelection.PlanToWatch:
                    planToWatch = new ObservableCollection<LibraryObject>();
                    break;
                case LibrarySelection.Completed:
                    completed = new ObservableCollection<LibraryObject>();
                    break;
                case LibrarySelection.OnHold:
                    onHold = new ObservableCollection<LibraryObject>();
                    break;
                case LibrarySelection.Dropped:
                    dropped = new ObservableCollection<LibraryObject>();
                    break;
                case LibrarySelection.Favourites:
                    favourites = new ObservableCollection<LibraryObject>();
                    break;
                case LibrarySelection.All:
                    ClearLibrary(LibrarySelection.CurrentlyWatching);
                    ClearLibrary(LibrarySelection.PlanToWatch);
                    ClearLibrary(LibrarySelection.Completed);
                    ClearLibrary(LibrarySelection.OnHold);
                    ClearLibrary(LibrarySelection.Dropped);
                    ClearLibrary(LibrarySelection.Favourites);
                    break;
            }
        }
        #endregion

        #region Libraries Parsing
        public static bool IsLibraryLoaded(bool includeFavourites=true)
        {
            if (cwLoaded && pTWLoaded && cLoaded && oHLoaded && dLoaded)
            {
                if (includeFavourites)
                {
                    if (fLoaded) { return true; }
                    else { return false; }
                }
                else { return true; }
            }
            else { return false; }
        }

        public static string GetParseableLibraries(string libraryString)
        {
            string parsableLibraries = string.Empty;
            char jsonObjectSplitter = '␟';//'¡'; //'~';
            char animeListSplitter = '|';


            switch (libraryString)
            {
                case "":
                    List<LibraryObject> library = new List<LibraryObject> { };
                    foreach (LibraryObject lO in currentlyWatching)
                    {
                        library.Add(lO);
                    }
                    foreach (LibraryObject lO in planToWatch)
                    {
                        library.Add(lO);
                    }
                    foreach (LibraryObject lO in completed)
                    {
                        library.Add(lO);
                    }
                    foreach (LibraryObject lO in onHold)
                    {
                        library.Add(lO);
                    }
                    foreach (LibraryObject lO in dropped)
                    {
                        library.Add(lO);
                    }
                    parsableLibraries += JsonConvert.SerializeObject(library);
                    break;
                case "currently-watching":
                    if (currentlyWatching.Count == 0) { parsableLibraries += "nullList"; }
                    else
                    {
                        foreach (LibraryObject lO in currentlyWatching)
                        {
                            parsableLibraries += JsonConvert.SerializeObject(lO);
                            parsableLibraries += jsonObjectSplitter;
                        }
                    }
                    break;
                case "completed":
                    if (completed.Count == 0) { parsableLibraries += "nullList"; }
                    else
                    {
                        foreach (LibraryObject lO in completed)
                        {
                            parsableLibraries += JsonConvert.SerializeObject(lO);
                            parsableLibraries += jsonObjectSplitter;
                        }
                    }
                    break;
                case "plan-to-watch":
                    if (planToWatch.Count == 0) { parsableLibraries += "nullList"; }
                    else
                    {
                        foreach (LibraryObject lO in planToWatch)
                        {
                            parsableLibraries += JsonConvert.SerializeObject(lO);
                            parsableLibraries += jsonObjectSplitter;
                        }
                    }
                    break;
                case "on-hold":
                    if (onHold.Count == 0) { parsableLibraries += "nullList"; }
                    else
                    {
                        foreach (LibraryObject lO in onHold)
                        {
                            parsableLibraries += JsonConvert.SerializeObject(lO);
                            parsableLibraries += jsonObjectSplitter;
                        }
                    }
                    break;
                case "dropped":
                    if (dropped.Count == 0) { parsableLibraries += "nullList"; }
                    else
                    {
                        foreach (LibraryObject lO in dropped)
                        {
                            parsableLibraries += JsonConvert.SerializeObject(lO);
                            parsableLibraries += jsonObjectSplitter;
                        }
                    }
                    break;
                case "favourites":
                    if (favourites.Count == 0) { parsableLibraries += "nullList"; }
                    else
                    {
                        foreach (LibraryObject lO in favourites)
                        {
                            parsableLibraries += JsonConvert.SerializeObject(lO);
                            parsableLibraries += jsonObjectSplitter;
                        }
                    }
                    break;
                default:
                    break;
            }
            return parsableLibraries;
        }
        public static void ParseLibraries(string animeLibraryString, string libraryString)
        {
            Debug.WriteLine("ParseLibraries("+libraryString+"): Entering");
            char jsonObjectSplitter = '␟';// '¡';// '~';
            char animeListSplitter = '|';

            //string output = JsonConvert.SerializeObject(library[0]);
            //Debug.WriteLine(output);
            //LibraryObject tempt = JsonConvert.DeserializeObject<LibraryObject>(output);
            //Console.WriteLine(tempt.anime.slug);


            if (libraryString == "")
            {
                if (!String.IsNullOrWhiteSpace(animeLibraryString))
                {
                    //Debug.WriteLine(animeLibraryString);
                    string response = "{\"library\":" + animeLibraryString + "}";
                    JObject jObject = JObject.Parse(response);
                    LibraryList libraryList = JsonConvert.DeserializeObject<LibraryList>(jObject.ToString());

                    foreach (LibraryObject temp in libraryList.library)
                    {
                        switch (temp.status)
                        {
                            case "currently-watching":
                                currentlyWatching.Add(temp);
                                break;
                            case "completed":
                                completed.Add(temp);
                                break;
                            case "plan-to-watch":
                                planToWatch.Add(temp);
                                break;
                            case "on-hold":
                                onHold.Add(temp);
                                break;
                            case "dropped":
                                dropped.Add(temp);
                                break;
                        }
                    }

                    cwLoaded = true;
                    cLoaded = true;
                    pTWLoaded = true;
                    oHLoaded = true;
                    dLoaded = true;

                }
            }
            else
            {
                string[] librariesSplit = animeLibraryString.Split(jsonObjectSplitter);

                if (animeLibraryString.Contains("nullList"))
                {
                    Debug.WriteLine("nullList", "ParseLibrary()");
                    switch (libraryString)
                    {
                        case "currently-watching":
                            //currentlyWatching = new List<LibraryObject>();
                            cwLoaded = true;
                            break;
                        case "completed":
                            //completed = new List<LibraryObject>();
                            cLoaded = true;
                            break;
                        case "plan-to-watch":
                            //planToWatch = new List<LibraryObject>();
                            pTWLoaded = true;
                            break;
                        case "on-hold":
                            //onHold = new List<LibraryObject>();
                            oHLoaded = true;
                            break;
                        case "dropped":
                            //dropped = new List<LibraryObject>();
                            dLoaded = true;
                            break;
                        case "favourites":
                            //favourites = new List<LibraryObject>();
                            fLoaded = true;
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    Debug.WriteLine("Parsing Library: " + libraryString);
                    foreach (string o in librariesSplit)
                    {
                        if (!String.IsNullOrWhiteSpace(o))
                        {
                            //Debug.WriteLine("Parsing Json", "ParseLibrary()");
                            JObject jObject = JObject.Parse(o); // This would be the string you defined above

                            //Console.WriteLine(o.ToString());
                            //LibraryObject tempAnimeObject = JsonConvert.DeserializeObject<LibraryObject>(o.ToString());

                            //Debug.WriteLine(jObject.ToString());

                            LibraryObject temp = JsonConvert.DeserializeObject<LibraryObject>(jObject.ToString()); //o
                            Debug.WriteLine("Parsed Json: " + temp.anime.slug, "ParseLibrary()");


                            switch (libraryString)
                            {
                                case "currently-watching":
                                    currentlyWatching.Add(temp);
                                    break;
                                case "completed":
                                    completed.Add(temp);
                                    break;
                                case "plan-to-watch":
                                    planToWatch.Add(temp);
                                    break;
                                case "on-hold":
                                    onHold.Add(temp);
                                    break;
                                case "dropped":
                                    dropped.Add(temp);
                                    break;
                                case "favourites":
                                    bool itemAdded = false;
                                    foreach (LibraryObject lO in currentlyWatching)
                                    {
                                        if (temp.anime.slug == lO.anime.slug) { favourites.Add(lO); itemAdded = true; break; }
                                    }
                                    foreach (LibraryObject lO in planToWatch)
                                    {
                                        if (temp.anime.slug == lO.anime.slug) { favourites.Add(lO); itemAdded = true; break; }
                                    }
                                    foreach (LibraryObject lO in completed)
                                    {
                                        if (temp.anime.slug == lO.anime.slug) { favourites.Add(lO); itemAdded = true; break; }
                                    }
                                    foreach (LibraryObject lO in onHold)
                                    {
                                        if (temp.anime.slug == lO.anime.slug) { favourites.Add(lO); itemAdded = true; break; }
                                    }
                                    foreach (LibraryObject lO in dropped)
                                    {
                                        if (temp.anime.slug == lO.anime.slug) { favourites.Add(lO); itemAdded = true; break; }
                                    }

                                    if (!itemAdded) { favourites.Add(temp); }
                                    break;
                                default:
                                    break;
                            }
                        }
                    }

                    switch (libraryString)
                    {
                        case "currently-watching":
                            cwLoaded = true;
                            break;
                        case "completed":
                            cLoaded = true;
                            break;
                        case "plan-to-watch":
                            pTWLoaded = true;
                            break;
                        case "on-hold":
                            oHLoaded = true;
                            break;
                        case "dropped":
                            dLoaded = true;
                            break;
                        case "favourites":
                            fLoaded = true;
                            break;
                        default:
                            break;
                    }
                }
            }

            Debug.WriteLine("ParseLibraries(" + libraryString + "): Exiting");
        }                
        #endregion

    }
}
