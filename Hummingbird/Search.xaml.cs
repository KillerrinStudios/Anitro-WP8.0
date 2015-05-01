using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using AnitroAPI; //using Hummingbird_API;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Net.NetworkInformation;

namespace Hummingbird
{
    public partial class Search : PhoneApplicationPage
    {

        public ObservableCollection<Anime> searchResults = new ObservableCollection<Anime>();

        public Search()
        {
            InitializeComponent();

            ///OnOrientationChanged(new OrientationChangedEventArgs(Orientation));

            listBox_Search.ItemsSource = searchResults;
        }

        protected override void OnOrientationChanged(OrientationChangedEventArgs e)
        {
            if (e.Orientation == PageOrientation.Landscape || e.Orientation == PageOrientation.LandscapeLeft || e.Orientation == PageOrientation.LandscapeRight)
            {
                listBox_Search.ItemTemplate = searchTemplateLandscape;
                listBox_Search.ItemsSource = searchResults;
            }
            else
            {
                listBox_Search.ItemTemplate = searchTemplatePortrait;
                listBox_Search.ItemsSource = searchResults;
            }

            listBox_Search.UpdateLayout();
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


        #region Events
        private void listBox_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            try
            {
                if (((ListBox)sender).SelectedItem == null) { return; }

                Anime _anime = ((ListBox)sender).SelectedItem as Anime;
                
                LibraryObject selected = new LibraryObject
                {
                    episodes_watched = "0", //public string episodes_watched { get; set; } // int
                    last_watched = "", //public string last_watched { get; set; }
                    rewatched_times = "0", //public string rewatched_times { get; set; } // int
                    notes = "", //public object notes { get; set; } //string
                    notes_present = false, //public object notes_present { get; set; } //bool
                    status = "search", //public string status { get; set; }
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

                Consts.LibrarySelection libSel = Consts.FindWhereExistsInLibrary(selected);
                if (libSel == Consts.LibrarySelection.None || libSel == Consts.LibrarySelection.Favourites) { }
                else
                {
                    selected = Consts.GetObjectInLibrary(libSel, selected.anime.slug);
                }


                string sendData = "slug=" + selected.anime.slug + "&status=" + selected.status;
                NavigationService.Navigate(new Uri("/LibraryPage.xaml?" + sendData, UriKind.Relative));

                ((ListBox)sender).SelectedItem = null;
            }
            catch (Exception) { }
        }

        private void AddToFavourites_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MenuItem menu = sender as MenuItem;
                Anime _anime = menu.DataContext as Anime;

                Consts.LibrarySelection selection = Consts.FindWhereExistsInLibrary(_anime.slug);
                LibraryObject libObj = new LibraryObject();

                switch(selection)
                {
                    case Consts.LibrarySelection.CurrentlyWatching:
                        libObj = Consts.GetObjectInLibrary(selection, _anime.slug);
                        break;
                    case Consts.LibrarySelection.PlanToWatch:
                        libObj = Consts.GetObjectInLibrary(selection, _anime.slug);
                        break;
                    case Consts.LibrarySelection.Completed:
                        libObj = Consts.GetObjectInLibrary(selection, _anime.slug);
                        break;
                    case Consts.LibrarySelection.OnHold:
                        libObj = Consts.GetObjectInLibrary(selection, _anime.slug);
                        break;
                    case Consts.LibrarySelection.Dropped:
                        libObj = Consts.GetObjectInLibrary(selection, _anime.slug);
                        break;
                    case Consts.LibrarySelection.None:
                    default:
                        libObj = new LibraryObject
                        {
                            episodes_watched = "0", //public string episodes_watched { get; set; } // int
                            last_watched = "", //public string last_watched { get; set; }
                            rewatched_times = "0", //public string rewatched_times { get; set; } // int
                            notes = "", //public object notes { get; set; } //string
                            notes_present = false, //public object notes_present { get; set; } //bool
                            status = "search", //public string status { get; set; }
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
                        break;
                }
                Consts.AddToFavourites(libObj);
            }
            catch (Exception) { }
        }

        private void PinToStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MenuItem menu = sender as MenuItem;
                Anime _anime = menu.DataContext as Anime;
                LibraryObject libraryObject = new LibraryObject
                {
                    episodes_watched = "0", //public string episodes_watched { get; set; } // int
                    last_watched = "", //public string last_watched { get; set; }
                    rewatched_times = "0", //public string rewatched_times { get; set; } // int
                    notes = "", //public object notes { get; set; } //string
                    notes_present = false, //public object notes_present { get; set; } //bool
                    status = "search", //public string status { get; set; }
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
                standardTileData.BackBackgroundImage = null; //new Uri("/Assets/Tiles/FlipCycleTileMedium.png", UriKind.Relative);

                ShellTile tiletopin = ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains("MainPage.xaml"));

                if (tiletopin == null)
                {
                    string sendData = "slug=" + libraryObject.anime.slug + "&status=" + libraryObject.status + "&pin=true";
                    ShellTile.Create(new Uri("/LibraryPage.xaml?" + sendData, UriKind.Relative), standardTileData, false);
                }
            }
            catch (Exception) { }
        }

        private async void searchBoxEnterEvent(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Debug.WriteLine("Enter Pressed");
                this.Focus();

                if (Consts.IsConnectedToInternet())
                {
                    ChangeProgressBar(true);
                    Task<List<Anime>> animeListTask = AnitroAPI.Hummingbird.SearchAnime(searchBox.Text);
                    await animeListTask;
                    List<Anime> animeList = animeListTask.Result;

                    searchResults = new ObservableCollection<Anime>();
                    foreach (Anime a in animeList)
                    {
                        searchResults.Add(a);
                    }

                    listBox_Search.ItemsSource = searchResults;

                    ChangeProgressBar(false);
                }
                else
                {
                    MessageBox.Show("There is no network to download search. Please check your network settings and try again.");
                }
            }
        }
        #endregion

        #region AppBar
        private void AppBar_Settings_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("Navigating to Settings");
            NavigationService.Navigate(new Uri("/Settings.xaml", UriKind.Relative));
            Debug.WriteLine("Leaving Search to Settings");
        }
        private void AppBar_About_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("Navigating to About");
            NavigationService.Navigate(new Uri("/About.xaml", UriKind.Relative));
            Debug.WriteLine("Leaving Search to About");
        }
        #endregion
    }
}