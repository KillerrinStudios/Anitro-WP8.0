using System;
using System.Collections.Generic;

using System.Linq;
using System.Net;
using System.Diagnostics;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using Microsoft.Phone.Controls;
using Microsoft.Phone.Controls.Primitives;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;

using AnitroAPI;
using System.Net.NetworkInformation;//using Microsoft.Phone.Net.NetworkInformation; //using Hummingbird_API;



namespace Hummingbird
{
    public partial class LibraryPage : PhoneApplicationPage
    {
        public LibraryObject libraryObject;

        public int[] numberSelected = new int[] { 0, 0, 0 };

        public LibraryPage()
        {
            InitializeComponent();

            List<int> numbers = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            this.selectorInt1.DataSource = new ListLoopingDataSource<int>() { Items = numbers, SelectedItem = 0 };
            this.selectorInt2.DataSource = new ListLoopingDataSource<int>() { Items = numbers, SelectedItem = 0 };
            this.selectorInt3.DataSource = new ListLoopingDataSource<int>() { Items = numbers, SelectedItem = 0 };

            this.libraryRating.ManipulationStarted += new EventHandler<ManipulationStartedEventArgs>(rating_ManipulationStarted);
            this.libraryRating.ManipulationCompleted += new EventHandler<ManipulationCompletedEventArgs>(rating_ManipulationCompleted);
            Touch.FrameReported += new TouchFrameEventHandler(Touch_FrameReported);
        }

        public bool contentLoaded = false;
        public bool favouritesLoaded = false;
        public bool isLandscape;
        public bool isFavorited;
        private bool loggedIn;

        private string metaDataErrorMessage = "There was a problem grabbing the metadata for this show. Please try again later.";

        public static bool backButtonPressed = false;
        public static bool cameFromPin = false;

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!contentLoaded)
            {
                string slug;
                string status;
                string pin;

                NavigationContext.QueryString.TryGetValue("slug", out slug);
                NavigationContext.QueryString.TryGetValue("status", out status);
                NavigationContext.QueryString.TryGetValue("pin", out pin);

                Debug.WriteLine(status);

                #region Pin Settings
                if (pin == "true")
                {
                    Debug.WriteLine("Came from pin");
                    cameFromPin = true;
                }

                ApplicationBarIconButton pinButton = (ApplicationBarIconButton)ApplicationBar.Buttons[1];
                bool pinned = false;
                foreach (ShellTile tile in ShellTile.ActiveTiles)
                {
                    if (tile.NavigationUri.OriginalString.Contains(slug))
                    {
                        pinButton.IsEnabled = false;
                        pinned = true;
                    }
                }
                if (!pinned) pinButton.IsEnabled = true;
                #endregion

                ChangeProgressBar(true);

                Task<bool> animeGet = GetAnimeInfo(slug, status);
                if (await animeGet)
                {

                    if (status != "favourites" && !favouritesLoaded)
                    {
                        await Storage.LoadAnimeLibrary("favourites");
                    }

                    Uri uri = new Uri(libraryObject.anime.cover_image, UriKind.Absolute);
                    ImageSource src = new BitmapImage(uri);
                    BackgroundImage.Source = src;

                    #region Favourites Button
                    ApplicationBarIconButton favButton = (ApplicationBarIconButton)ApplicationBar.Buttons[2];
                    if (Consts.DoesExistInLibrary(Consts.LibrarySelection.Favourites, libraryObject)) { isFavorited = true; favButton.IconUri = new Uri("/Assets/AppBar/favs.removefrom.png", UriKind.Relative); }
                    else { isFavorited = false; favButton.IconUri = new Uri("/Assets/AppBar/favs.addto.png", UriKind.Relative); }
                    #endregion

                    #region Add Genres to Offline Storage
                    if (Consts.IsConnectedToInternet())
                    {
                        Consts.LibrarySelection lS = Consts.GetLibrarySelectionFromStatus(status);
                        if (lS != Consts.LibrarySelection.None && lS != Consts.LibrarySelection.All)
                        {
                            if (libraryObject.anime.genres.Count == 0 || string.IsNullOrEmpty(libraryObject.anime.genres[0].name))
                            {
                                Anime _anime = await AnitroAPI.Hummingbird.GetAnime(slug);
                                libraryObject.anime = _anime;
                                Consts.UpdateLibrary(lS, libraryObject);
                            }
                        }
                    }
                    #endregion

                    if (!contentLoaded) { DisplayAnimeInfo(); contentLoaded = true; }
                }

                ChangeProgressBar(false);
                OnOrientationChanged(new OrientationChangedEventArgs(Orientation));
            }

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.libraryRating.ManipulationStarted -= new EventHandler<ManipulationStartedEventArgs>(rating_ManipulationStarted);
            this.libraryRating.ManipulationCompleted -= new EventHandler<ManipulationCompletedEventArgs>(rating_ManipulationCompleted);
            Touch.FrameReported -= new TouchFrameEventHandler(Touch_FrameReported);

           // while (!safeToBackOut) { }
            if (backButtonPressed)
            {
                if (loggedIn) 
                {
                    if (cameFromPin)
                    {
                        Debug.WriteLine("Its not safe to close yet");
                        //System.Threading.Thread.Sleep(TimeSpan.FromSeconds(2.0));
                    }

                    System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(f =>
                    {
                        Task<bool> libraryUpdate = UpdateLibrary();
                        libraryUpdate.Wait();
                    }));
                    //bool b = (UpdateLibrary()).Result; 
                }
            }

            base.OnNavigatedFrom(e);
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            backButtonPressed = true;
            base.OnBackKeyPress(e);
        }

        protected override void OnOrientationChanged(OrientationChangedEventArgs e)
        {
            if (contentLoaded)
            {
                switch (e.Orientation)
                {
                    case PageOrientation.Landscape:
                    case PageOrientation.LandscapeLeft:
                    case PageOrientation.LandscapeRight:
                        Debug.WriteLine("Page Orientation is now Landscape");
                        isLandscape = true;

                        pagePanorama.Width = 645;
                        pagePanorama.UpdateLayout();

                        libraryPanoramaItem.Width = 605;
                        libraryPanoramaItem.UpdateLayout();

                        animePanoramaItem.Width = 605;
                        animePanoramaItem.UpdateLayout();

                        animeSynopsisScrollViewer.Height = 223;
                        animeSynopsisScrollViewer.UpdateLayout();
                        
                        libraryScrollViewer.Height = 300;
                        libraryScrollViewer.UpdateLayout();
                        break;
                    case PageOrientation.PortraitDown:
                    case PageOrientation.PortraitUp:
                    case PageOrientation.Portrait:
                        Debug.WriteLine("Page Orientation is now Portrait");
                        isLandscape = false;

                        pagePanorama.Width = 470;
                        pagePanorama.UpdateLayout();

                        libraryPanoramaItem.Width = 430;
                        libraryPanoramaItem.UpdateLayout();

                        animePanoramaItem.Width = 430;
                        animePanoramaItem.UpdateLayout();

                        if (animeGenres.ActualHeight >= 60) animeSynopsisScrollViewer.Height = 390;
                        else if (animeGenres.ActualHeight >= 30.0) animeSynopsisScrollViewer.Height = 420;
                        else animeSynopsisScrollViewer.Height = 450; //440//410//480
                        animeSynopsisScrollViewer.UpdateLayout();

                        libraryScrollViewer.Height = 520;
                        libraryScrollViewer.UpdateLayout();
                        break;
                }

                //DisplayAnimeInfo();
            }

            //Debug.WriteLine(ActualHeight);
            //Debug.WriteLine(ActualWidth);

            base.OnOrientationChanged(e);
        }

        #region Helper Methods
        private void ChangeProgressBar(bool isEnabled)
        {
            if (isEnabled) { ApplicationProgressBar.Visibility = System.Windows.Visibility.Visible; }
            else { ApplicationProgressBar.Visibility = System.Windows.Visibility.Collapsed; }
            ApplicationProgressBar.IsEnabled = isEnabled;
        }
        #endregion

        public async Task<bool> GetAnimeInfo(string slug, string status)
        {
            Debug.WriteLine("GetAnimeinfo(): Entering");
            Debug.WriteLine(slug + "|" + status);

            #region Not Logged In
            if (string.IsNullOrEmpty(Storage.Settings.User.userName.Value) || string.IsNullOrEmpty(Storage.Settings.User.authToken.Value))
            {
                Debug.WriteLine("GetAnimeInfo(): Not Logged In");

                loggedIn = false;

                Anime _anime;

                if (Consts.IsConnectedToInternet())
                {
                    Debug.WriteLine("GetAnimeInfo(): Connected to the internet, grabbing data from internet");
                    Task<Anime> animeTask = AnitroAPI.Hummingbird.GetAnime(slug);
                    await animeTask;
                    _anime = animeTask.Result;
                }
                else
                {
                    Debug.WriteLine("GetAnimeInfo(): Not connected to the internet, setting to blank");
                    MessageBox.Show("Because you are not logged in, you will need to connect to the internet to grab title information.");

                    Application.Current.Terminate();
                    _anime = new Anime
                    {
                        slug = "",
                        status = "",
                        url = "",
                        title = "",
                        alternate_title = "",
                        episode_count = "",
                        cover_image = "",
                        cover_image_uri = new Uri("", UriKind.Absolute),
                        synopsis = "",
                        show_type = "",
                        genres = new List<Genre>
                                {
                                    new Genre { name = "" }
                                }
                    };

                    return false;
                }

                LibraryObject libObj = new LibraryObject
                {
                    episodes_watched = "0", //public string episodes_watched { get; set; } // int
                    last_watched = "", //public string last_watched { get; set; }
                    rewatched_times = "0", //public string rewatched_times { get; set; } // int
                    notes = "", //public object notes { get; set; } //string
                    notes_present = false, //public object notes_present { get; set; } //bool
                    status = "", //public string status { get; set; }
                    id = "", //public string id { get; set; }
                    @private = false, //public bool @private { get; set; }
                    rewatching = false, //public object rewatching { get; set; } //bool
                    anime = _anime, //public Anime anime { get; set; }
                    rating = new AnitroAPI.Rating //public Rating rating { get; set; }
                    {
                        type = "", //public string type { get; set; }
                        value = "0.0", //public string value { get; set; }
                        valueAsDouble = 0.0, //public double valueAsDouble { get; set; }
                    },
                };

                libraryObject = libObj;

                Debug.WriteLine("GetAnimeInfo(): Exiting Successful");
                return true;
            }
            #endregion
            #region Logged In
            else
            {
                loggedIn = true;

                #region Library Loaded
                if (Consts.IsLibraryLoaded())
                {
                    bool newestInfoPulled = false;

                    //libraryObject = Consts.GetObjectInLibrary(Consts.GetLibrarySelectionFromStatus(status), slug);
                    switch (status)
                    {
                        case "currently-watching":
                            foreach (LibraryObject lO in Consts.currentlyWatching)
                            {
                                if (lO.anime.slug == slug) { libraryObject = lO; break; }
                            }
                            break;
                        case "plan-to-watch":
                            foreach (LibraryObject lO in Consts.planToWatch)
                            {
                                if (lO.anime.slug == slug) { libraryObject = lO; break; }
                            }
                            break;
                        case "completed":
                            foreach (LibraryObject lO in Consts.completed)
                            {
                                if (lO.anime.slug == slug) { libraryObject = lO; break; }
                            }
                            break;
                        case "on-hold":
                            foreach (LibraryObject lO in Consts.onHold)
                            {
                                if (lO.anime.slug == slug) { libraryObject = lO; break; }
                            }
                            break;
                        case "dropped":
                            foreach (LibraryObject lO in Consts.dropped)
                            {
                                if (lO.anime.slug == slug) { libraryObject = lO; break; }
                            }
                            break;
                        case "favourites":
                            foreach (LibraryObject lO in Consts.favourites)
                            {
                                if (lO.anime.slug == slug) { libraryObject = lO; break; }
                            }
                            break;
                        case "uriAssociation":
                            if (Consts.DoesExistInLibrary(Consts.LibrarySelection.All, slug))
                            {
                                libraryObject = Consts.GetObjectInLibrary(Consts.LibrarySelection.All, slug);
                                return true;
                            }
                            else
                            {
                                return await GetAnimeInfo(slug, "");
                            }
                        case "search":
                        case "":
                        default:
                            Anime _anime;

                            if (Consts.IsConnectedToInternet())
                            {
                                Task<Anime> animeTask = AnitroAPI.Hummingbird.GetAnime(slug);
                                await animeTask;
                                _anime = animeTask.Result;
                            }
                            else
                            {
                                MessageBox.Show(metaDataErrorMessage);
                                NavigationService.GoBack();
                                _anime = new Anime
                                {
                                    slug = "",
                                    status = "",
                                    url = "",
                                    title = "",
                                    alternate_title = "",
                                    episode_count = "",
                                    cover_image = "",
                                    cover_image_uri = new Uri("", UriKind.Absolute),
                                    synopsis = "",
                                    show_type = "",
                                    genres = new List<Genre>
                                {
                                    new Genre { name = "" }
                                }
                                };
                            }

                            LibraryObject libObj = new LibraryObject
                            {
                                episodes_watched = "0", //public string episodes_watched { get; set; } // int
                                last_watched = "", //public string last_watched { get; set; }
                                rewatched_times = "0", //public string rewatched_times { get; set; } // int
                                notes = "", //public object notes { get; set; } //string
                                notes_present = false, //public object notes_present { get; set; } //bool
                                status = "", //public string status { get; set; }
                                id = "", //public string id { get; set; }
                                @private = false, //public bool @private { get; set; }
                                rewatching = false, //public object rewatching { get; set; } //bool
                                anime = _anime, //public Anime anime { get; set; }
                                rating = new AnitroAPI.Rating //public Rating rating { get; set; }
                                {
                                    type = "", //public string type { get; set; }
                                    value = "0.0", //public string value { get; set; }
                                    valueAsDouble = 0.0, //public double valueAsDouble { get; set; }
                                },
                            };

                            libraryObject = libObj;
                            newestInfoPulled = true;

                            Debug.WriteLine("GetAnimeInfo(): Exiting Successsful");
                            favouritesLoaded = true;
                            return true;
                    }

                    if (!newestInfoPulled)
                    {
                        Debug.WriteLine("GetAnimeInfo(): Exiting Successful");
                        favouritesLoaded = true;
                        return true;
                    }
                }
                #endregion
                #region Library Not Loaded
                else
                {
                    bool internetDown = Consts.IsConnectedToInternet();

                    if (status == "search" || status == "")
                    {
                        Anime _anime;
                        if (Consts.IsConnectedToInternet())
                        {
                            Task<Anime> animeTask = AnitroAPI.Hummingbird.GetAnime(slug);
                            await animeTask;
                            _anime = animeTask.Result;
                        }
                        else
                        {
                            MessageBox.Show(metaDataErrorMessage);
                            NavigationService.GoBack();

                            _anime = new Anime
                            {
                                slug = "",
                                status = "",
                                url = "",
                                title = "",
                                alternate_title = "",
                                episode_count = "",
                                cover_image = "",
                                cover_image_uri = new Uri("", UriKind.Absolute),
                                synopsis = "",
                                show_type = "",
                                genres = new List<Genre>
                                {
                                    new Genre { name = "" }
                                }
                            };
                        }

                        LibraryObject libObj = new LibraryObject
                        {
                            episodes_watched = "0", //public string episodes_watched { get; set; } // int
                            last_watched = "", //public string last_watched { get; set; }
                            rewatched_times = "0", //public string rewatched_times { get; set; } // int
                            notes = "", //public object notes { get; set; } //string
                            notes_present = false, //public object notes_present { get; set; } //bool
                            status = "", //public string status { get; set; }
                            id = "", //public string id { get; set; }
                            @private = false, //public bool @private { get; set; }
                            rewatching = false, //public object rewatching { get; set; } //bool
                            anime = _anime, //public Anime anime { get; set; }
                            rating = new AnitroAPI.Rating //public Rating rating { get; set; }
                            {
                                type = "", //public string type { get; set; }
                                value = "0.0", //public string value { get; set; }
                                valueAsDouble = 0.0, //public double valueAsDouble { get; set; }
                            },
                        };

                        libraryObject = libObj;

                        Debug.WriteLine("GetAnimeInfo(): Exiting Successful");
                        return true;
                    }
                    else if (status == "uriAssociation")
                    {
                        await Storage.LoadAnimeLibrary("");
                        await Storage.LoadAnimeLibrary("favourites");
                        favouritesLoaded = true;

                        if (Consts.DoesExistInLibrary(Consts.LibrarySelection.All, slug))
                        {
                            libraryObject = Consts.GetObjectInLibrary(Consts.LibrarySelection.All, slug);
                            return true;
                        }
                        else
                        {
                            return await GetAnimeInfo(slug, "");
                        }
                    }
                    else
                    {
                        switch (status)
                        {
                            case "currently-watching":
                                await Storage.LoadAnimeLibrary(""); //"currently-watching");
                                break;
                            case "plan-to-watch":
                                await Storage.LoadAnimeLibrary(""); //"plan-to-watch");
                                break;
                            case "completed":
                                await Storage.LoadAnimeLibrary(""); //"completed");
                                break;
                            case "on-hold":
                                await Storage.LoadAnimeLibrary(""); //"on-hold");
                                break;
                            case "dropped":
                                await Storage.LoadAnimeLibrary(""); //"dropped");
                                break;
                            case "favourites":
                                await Storage.LoadAnimeLibrary("favourites");
                                favouritesLoaded = true;
                                break;
                        }

                        libraryObject = Consts.GetObjectInLibrary(Consts.GetLibrarySelectionFromStatus(status), slug);

                        Debug.WriteLine("GetAnimeInfo(): Exiting Successful");
                        return true;
                    }
                }
            }
            #endregion
            #endregion

            Debug.WriteLine("GetAnimeInfo(): Exiting Failed");
            return false;
        }

        public async Task<bool> UpdateLibrary()
        {
            if (!contentLoaded) { return false; }
            if (!loggedIn) { return false; }
            Storage.isSavingComplete = false;

            Debug.WriteLine("UpdateLibrary(): Entering");
            if (libraryObject.status == "" || libraryObject.status == "favourites" || libraryObject.status == "search" || libraryObject.status == "none") { Debug.WriteLine("UpdateLibrary(): Can't update library"); }
            else
            {
                try
                {
                    if (Consts.IsConnectedToInternet())
                    {
                        Debug.WriteLine("UpdateLibrary(): updating library ...");

                        if (await AnitroAPI.Hummingbird.PostLibraryUpdate(libraryObject))
                        {
                            Consts.UpdateLibrary(Consts.GetLibrarySelectionFromStatus(libraryObject), libraryObject);
                            Debug.WriteLine("UpdateLibrary(): update successful");
                            return true;
                        }
                        else { Debug.WriteLine("UpdateLibrary(): update failed"); }
                    }
                }
                catch (Exception) { Debug.WriteLine("UpdateLibrary(): update failed");  }
            }

            //safeToBackOut = true;

            Storage.isSavingComplete = true;

            Debug.WriteLine("UpdateLibrary(): Exiting");
            return false;
        }

        public void DisplayAnimeInfo()
        {
            Debug.WriteLine("DisplayAnimeInfo");

            animePanoramaItem.Header = libraryObject.anime.title;
            animeSecondaryHeader.Text = libraryObject.anime.alternate_title;

            string genreInfo = "";
            for (int i = 0; i < libraryObject.anime.genres.Count; i++)
            {
                try
                {
                    genreInfo += libraryObject.anime.genres[i].name;
                    genreInfo += ", ";
                }
                catch (Exception) { }
            }

            if (genreInfo[genreInfo.Length - 2] == ',') genreInfo = genreInfo.Substring(0, genreInfo.Length - 2);
            animeGenres.Text = genreInfo;
            animeGenres.TextWrapping = TextWrapping.Wrap;
            animeGenresBar.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;

            #region Stats Bar
            if (libraryObject.anime.episode_count == "0") { animeEpisodeCount.Text = "Episodes: " + "?"; }
            else { animeEpisodeCount.Text = "Episodes: " + libraryObject.anime.episode_count; }
            animeEpisodeCount.TextWrapping = TextWrapping.Wrap;

            animeStatus.Text = libraryObject.anime.status;
            animeStatus.TextWrapping = TextWrapping.Wrap;

            animeShowType.Text = libraryObject.anime.show_type;
            animeShowType.TextWrapping = TextWrapping.Wrap;

            animeStatsBar.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            #endregion

            animeSynopsis.Text = libraryObject.anime.synopsis;
            animeSynopsis.TextWrapping = TextWrapping.Wrap;


            /// 
            /// Library Set
            ///

            if (!(string.IsNullOrEmpty(libraryObject.last_watched) || string.IsNullOrWhiteSpace(libraryObject.last_watched)))
            {
                string animeLastWatched = libraryObject.last_watched.Substring(0, libraryObject.last_watched.Length - 1);
                string[] last_watchedSplit = animeLastWatched.Split('T');

                libraryLastWatched.Text = "last watched: " + last_watchedSplit[0] + " at " + last_watchedSplit[1];
            }
            else
            {
                libraryLastWatched.Text = "last watched: never";
            }

            libraryRating.Value = libraryObject.rating.valueAsDouble;

            switch (libraryObject.status)
            {
                case "":
                    LibraryPicker.SelectedIndex = 0;
                    break;
                case "favourites":
                    LibraryPicker.SelectedIndex = 0;
                    break;
                case "currently-watching":
                    LibraryPicker.SelectedIndex = 1;
                    break;
                case "plan-to-watch":
                    LibraryPicker.SelectedIndex = 2;
                    break;
                case "completed":
                    LibraryPicker.SelectedIndex = 3;
                    break;
                case "on-hold":
                    LibraryPicker.SelectedIndex = 4;
                    break;
                case "dropped":
                    LibraryPicker.SelectedIndex = 5;
                    break;
                default:
                    LibraryPicker.SelectedIndex = 0;
                    break;
            }

            int wCount = Convert.ToInt32(libraryObject.episodes_watched);
            int epCount = Convert.ToInt32(libraryObject.anime.episode_count);
            if (epCount == 0) { libraryEpisodesWatched.Text = wCount + "/" + "?"; }
            else { libraryEpisodesWatched.Text = wCount + "/" + epCount; }

            int rewatchedTimes = Convert.ToInt32(libraryObject.rewatched_times);
            libraryRewatchedTimes.Text = libraryObject.rewatched_times.ToString();

            libraryPrivate.IsChecked = libraryObject.@private;

            if (libraryObject.notes == null) { libraryObject.notes = ""; }
            notesTextBox.Text = libraryObject.notes.ToString();

            contentLoaded = true;
        }

        #region AppBar
        private void AppBar_Search_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("Navigating to Search");
            NavigationService.Navigate(new Uri("/Search.xaml", UriKind.Relative));
            Debug.WriteLine("Leaving LibraryPage to Search");
        }

        private void AppBar_Settings_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("Navigating to Settings");
            NavigationService.Navigate(new Uri("/Settings.xaml", UriKind.Relative));
            Debug.WriteLine("Leaving LibraryPage to Settings");
        }

        private void AppBar_About_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("Navigating to About");
            NavigationService.Navigate(new Uri("/About.xaml", UriKind.Relative));
            Debug.WriteLine("Leaving LibraryPage to About");
        }

        private void AppBar_favorite_Click(object sender, EventArgs e)
        {
            ApplicationBarIconButton favButton = (ApplicationBarIconButton)ApplicationBar.Buttons[2];
            if (isFavorited)
            {
                isFavorited = false;
                favButton.IconUri = new Uri("/Assets/AppBar/favs.addto.png", UriKind.Relative);
                Consts.RemoveFromFavourites(libraryObject);
            }
            else
            {
                isFavorited = true;
                favButton.IconUri = new Uri("/Assets/AppBar/favs.removefrom.png", UriKind.Relative);
                Consts.AddToFavourites(libraryObject);
            }
        }

        private void AppBar_IE_Click(object sender, EventArgs e)
        {
            if (libraryObject != null)
            {
                WebBrowserTask webBrowserTask = new WebBrowserTask();
                //Debug.WriteLine(libraryObject.anime.url);
                webBrowserTask.Uri = new Uri(libraryObject.anime.url, UriKind.Absolute);
                webBrowserTask.Show();
            }
        }

        private void AppBar_PinToStart_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (ShellTile tile in ShellTile.ActiveTiles)
                {
                    //Debug.WriteLine(tile.NavigationUri.OriginalString);
                    if (tile.NavigationUri.OriginalString.Contains(libraryObject.anime.slug)) { return; }
                }

                // Make the Tile
                StandardTileData standardTileData = new StandardTileData();

                standardTileData.BackgroundImage = new Uri(libraryObject.anime.cover_image, UriKind.Absolute);

                standardTileData.Title = libraryObject.anime.title;
                standardTileData.Count = 0;
                standardTileData.BackTitle = "";
                standardTileData.BackContent = "";
                standardTileData.BackBackgroundImage = null;

                ShellTile tiletopin = ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains("MainPage.xaml"));

                if (tiletopin == null)
                {
                    ApplicationBarIconButton button = (ApplicationBarIconButton)ApplicationBar.Buttons[2];
                    button.IsEnabled = false;

                    string sendData = "slug=" + libraryObject.anime.slug + "&status=" + libraryObject.status + "&pin=true";
                    ShellTile.Create(new Uri("/LibraryPage.xaml?" + sendData, UriKind.Relative), standardTileData);
                }
            }
            catch (Exception) { }
        }

        private async void Set_As_Lockscreen_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("Set_As_Lockscreen_Click(): Entering");
            try
            {
                if (!contentLoaded) { return; }
                if (Windows.Phone.System.UserProfile.LockScreenManager.IsProvidedByCurrentApplication)
                {
                    Debug.WriteLine("Awaiting Lockscreen_Helper.SetImage()");
                    await Lockscreen_Helper.SetImage(new Uri(libraryObject.anime.cover_image, UriKind.Absolute));
                }
            }
            catch (Exception) { Debug.WriteLine("Set_As_Locksceen_Click(): Failed"); }
            Debug.WriteLine("Set_As_Lockscreen_Click(): Exiting");
        }
        #endregion

        #region UI Events
        #region Library
        private void libraryPrivateChecked(object sender, RoutedEventArgs e)
        {
            libraryObject.@private = true;
            UpdateLibrary();
        }

        private void libraryPrivateUnchecked(object sender, RoutedEventArgs e)
        {
            libraryObject.@private = false;
            //UpdateLibrary();
        }

        private void libraryRewatchingChecked(object sender, RoutedEventArgs e)
        {
            libraryObject.rewatching = true;
            //UpdateLibrary();
        }

        private void libraryRewatchingUnchecked(object sender, RoutedEventArgs e)
        {
            libraryObject.rewatching = false;
            //UpdateLibrary();
        }

        private void incrementEpisodeWatchedBy1(object sender, RoutedEventArgs e)
        {
            int epCount = Convert.ToInt32(libraryObject.anime.episode_count);
            int wCount = Convert.ToInt32(libraryObject.episodes_watched);

            bool skipLibChange = false;

            if (epCount == 0) { skipLibChange = true; }
            if (wCount >= epCount && skipLibChange == false) 
            {
                LibraryPicker.SelectedIndex = 3;
                return;
            }

            libraryObject.episodes_watched = Convert.ToString(++wCount);
            if (epCount == 0) { libraryEpisodesWatched.Text = wCount + "/" + "?"; }
            else { libraryEpisodesWatched.Text = wCount + "/" + epCount; }
            //UpdateLibrary();

            if (wCount >= epCount && skipLibChange == false) { LibraryPicker.SelectedIndex = 3; }
        }

        private void decrimentEpisodeWatchedBy1(object sender, RoutedEventArgs e)
        {
            int epCount = Convert.ToInt32(libraryObject.anime.episode_count);
            int wCount = Convert.ToInt32(libraryObject.episodes_watched);

            if (wCount <= 0) { return; }
            if (wCount >= epCount) { LibraryPicker.SelectedIndex = 1; }


            libraryObject.episodes_watched = Convert.ToString(--wCount);
            if (epCount == 0) { libraryEpisodesWatched.Text = wCount + "/" + "?"; }
            else { libraryEpisodesWatched.Text = wCount + "/" + epCount; }
            //UpdateLibrary();
        }

        private void incrementRewatchedTimesBy1(object sender, RoutedEventArgs e)
        {
            int rewatchedTimes = Convert.ToInt32(libraryObject.rewatched_times);

            libraryObject.rewatched_times = Convert.ToString(++rewatchedTimes);
            libraryRewatchedTimes.Text = libraryObject.rewatched_times.ToString();
            //UpdateLibrary();
        }

        private void decrimentRewatchedTimesBy1(object sender, RoutedEventArgs e)
        {
            int rewatchedTimes = Convert.ToInt32(libraryObject.rewatched_times);

            if (rewatchedTimes <= 0) { return; }
            libraryObject.rewatched_times = Convert.ToString(--rewatchedTimes);
            libraryRewatchedTimes.Text = libraryObject.rewatched_times.ToString();
            //UpdateLibrary();
        }

        private void decrimentRatingByHalf(object sender, RoutedEventArgs e)
        {
            libraryRating.Value -= 0.5;

            libraryObject.rating.valueAsDouble = libraryRating.Value;
            libraryObject.rating.value = Convert.ToString(libraryRating.Value);
        }

        private void incrementRatingByHalf(object sender, RoutedEventArgs e)
        {
            libraryRating.Value += 0.5;

            libraryObject.rating.valueAsDouble = libraryRating.Value;
            libraryObject.rating.value = Convert.ToString(libraryRating.Value);
        }

        private void ratingValueChanged(object sender, EventArgs e)
        {
            if (libraryRating.Value > 5 || libraryRating.Value < 0) return;

            Debug.WriteLine("Rating Changed");
            libraryObject.rating.valueAsDouble = libraryRating.Value;
            libraryObject.rating.value = Convert.ToString(libraryRating.Value);
            //UpdateLibrary();
        }

        private void notesLostFocus(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Notes Lost Focus");
            
            libraryObject.notes = notesTextBox.Text;
            if (notesTextBox.Text == "") { libraryObject.notes_present = false; }
            else { libraryObject.notes_present = true; }

            //UpdateLibrary();
        }
        #endregion

        #region Custom Events
        void Touch_FrameReported(object sender, TouchFrameEventArgs e)
        {
            if (pagePanorama.SelectedIndex == 1)
            {
                if (e.GetPrimaryTouchPoint(this.libraryRating).Action == TouchAction.Up)
                {
                    libraryPanoramaItem.IsHitTestVisible = true;
                    //pagePanorama.IsLocked = false;
                }
            }
        }
        void rating_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            if (pagePanorama.SelectedIndex == 1)
            {
                pagePanorama.IsLocked = true;
            }
        }
        void rating_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            if (pagePanorama.SelectedIndex == 1)
            {
                pagePanorama.IsLocked = false;

                Debug.WriteLine("Rating Changed Manipulation");
                libraryObject.rating.valueAsDouble = libraryRating.Value;
                libraryObject.rating.value = Convert.ToString(libraryRating.Value);
                //UpdateLibrary();
            }
        }
        private void notesEnterEvent(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Debug.WriteLine("Enter Pressed");
                this.Focus();
            }
        }
        #endregion

        private void LibraryPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine("LibraryPicker_SelectionChanged(): Entering");

            if (!contentLoaded) { return; }
            if (!loggedIn) { return; }
            if (LibraryPicker.SelectedItem == null) { return; }
            
            try
            {
                switch (LibraryPicker.SelectedIndex)
                {
                    case 0: //<sys:String>None</sys:String>
                        //libraryObject.status = "";
                        break;
                    case 1: //<sys:String>Currently Watching</sys:String>
                        libraryObject.status = "currently-watching";
                        break;
                    case 2: //<sys:String>Plan To Watch</sys:String>
                        libraryObject.status = "plan-to-watch";
                        break;
                    case 3: //<sys:String>Completed</sys:String>
                        libraryObject.status = "completed";
                        break;
                    case 4: //<sys:String>On Hold</sys:String>
                        libraryObject.status = "on-hold";
                        break;
                    case 5: //<sys:String>Dropped</sys:String>
                        libraryObject.status = "dropped";
                        break;
                    default:
                        break;
                }

                Debug.WriteLine("LibraryPicker_SelectionChanged(): Status set");

                if (Consts.IsConnectedToInternet())
                {
                    if (LibraryPicker.SelectedIndex != 0)
                    {
                        Consts.SwitchLibraries(Consts.GetLibrarySelectionFromStatus(libraryObject), libraryObject);
                        AnitroAPI.Hummingbird.PostLibraryUpdate(libraryObject);
                        //UpdateLibrary();
                    }
                    else if (LibraryPicker.SelectedIndex == 0)
                    {
                        Debug.WriteLine("Removing " + libraryObject.anime.slug + " from Library");
                        AnitroAPI.Hummingbird.PostLibraryRemove(libraryObject.anime.slug);

                        Consts.RemoveFromLibrary(Consts.GetLibrarySelectionFromStatus(libraryObject), libraryObject);
                        libraryObject.status = "";
                    }
                }
            }
            catch (Exception) { }
        }
        #endregion
    }

    #region Looping Datasource
    // abstract the reusable code in a base class
    // this will allow us to concentrate on the specifics when implementing deriving looping data source classes
    public abstract class LoopingDataSourceBase : ILoopingSelectorDataSource
    {
        private object selectedItem;

        #region ILoopingSelectorDataSource Members

        public abstract object GetNext(object relativeTo);

        public abstract object GetPrevious(object relativeTo);

        public object SelectedItem
        {
            get
            {
                return this.selectedItem;
            }
            set
            {
                // this will use the Equals method if it is overridden for the data source item class
                if (!object.Equals(this.selectedItem, value))
                {
                    // save the previously selected item so that we can use it 
                    // to construct the event arguments for the SelectionChanged event
                    object previousSelectedItem = this.selectedItem;
                    this.selectedItem = value;
                    // fire the SelectionChanged event
                    this.OnSelectionChanged(previousSelectedItem, this.selectedItem);
                }
            }
        }

        public event EventHandler<SelectionChangedEventArgs> SelectionChanged;

        protected virtual void OnSelectionChanged(object oldSelectedItem, object newSelectedItem)
        {
            EventHandler<SelectionChangedEventArgs> handler = this.SelectionChanged;
            if (handler != null)
            {
                handler(this, new SelectionChangedEventArgs(new object[] { oldSelectedItem }, new object[] { newSelectedItem }));
            }
        }

        #endregion
    }

    public class ListLoopingDataSource<T> : LoopingDataSourceBase
    {
        private LinkedList<T> linkedList;
        private List<LinkedListNode<T>> sortedList;
        private IComparer<T> comparer;
        private NodeComparer nodeComparer;

        public ListLoopingDataSource()
        {
        }

        public IEnumerable<T> Items
        {
            get
            {
                return this.linkedList;
            }
            set
            {
                this.SetItemCollection(value);
            }
        }

        private void SetItemCollection(IEnumerable<T> collection)
        {
            this.linkedList = new LinkedList<T>(collection);

            this.sortedList = new List<LinkedListNode<T>>(this.linkedList.Count);
            // initialize the linked list with items from the collections
            LinkedListNode<T> currentNode = this.linkedList.First;
            while (currentNode != null)
            {
                this.sortedList.Add(currentNode);
                currentNode = currentNode.Next;
            }

            IComparer<T> comparer = this.comparer;
            if (comparer == null)
            {
                // if no comparer is set use the default one if available
                if (typeof(IComparable<T>).IsAssignableFrom(typeof(T)))
                {
                    comparer = Comparer<T>.Default;
                }
                else
                {
                    throw new InvalidOperationException("There is no default comparer for this type of item. You must set one.");
                }
            }

            this.nodeComparer = new NodeComparer(comparer);
            this.sortedList.Sort(this.nodeComparer);
        }

        public IComparer<T> Comparer
        {
            get
            {
                return this.comparer;
            }
            set
            {
                this.comparer = value;
            }
        }

        public override object GetNext(object relativeTo)
        {
            // find the index of the node using binary search in the sorted list
            int index = this.sortedList.BinarySearch(new LinkedListNode<T>((T)relativeTo), this.nodeComparer);
            if (index < 0)
            {
                return default(T);
            }

            // get the actual node from the linked list using the index
            LinkedListNode<T> node = this.sortedList[index].Next;
            if (node == null)
            {
                // if there is no next node get the first one
                node = this.linkedList.First;
            }
            return node.Value;
        }

        public override object GetPrevious(object relativeTo)
        {
            int index = this.sortedList.BinarySearch(new LinkedListNode<T>((T)relativeTo), this.nodeComparer);
            if (index < 0)
            {
                return default(T);
            }
            LinkedListNode<T> node = this.sortedList[index].Previous;
            if (node == null)
            {
                // if there is no previous node get the last one
                node = this.linkedList.Last;
            }
            return node.Value;
        }

        private class NodeComparer : IComparer<LinkedListNode<T>>
        {
            private IComparer<T> comparer;

            public NodeComparer(IComparer<T> comparer)
            {
                this.comparer = comparer;
            }

            #region IComparer<LinkedListNode<T>> Members

            public int Compare(LinkedListNode<T> x, LinkedListNode<T> y)
            {
                return this.comparer.Compare(x.Value, y.Value);
            }

            #endregion
        }

    }
    #endregion
}