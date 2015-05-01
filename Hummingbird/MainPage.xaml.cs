using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;

using Hummingbird.Resources;
using AnitroAPI; //using Hummingbird_API;

using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Net.NetworkInformation;
using BugSense;

namespace Hummingbird
{
    public partial class MainPage : PhoneApplicationPage
    {
        public string pageName = "MainPage"; 
        public bool notSignedIn;

        // Constructor
        public MainPage()
        {
            Debug.WriteLine("Entering MainPage()");

            InitializeComponent();

            this.listBox_ActivityFeed.ItemsSource = Consts.activityFeed;
            this.listBox_CurrentlyWatching.ItemsSource = Consts.currentlyWatching;
            this.listBox_PlanToWatch.ItemsSource = Consts.planToWatch;
            this.listBox_Completed.ItemsSource = Consts.completed;
            this.listBox_OnHold.ItemsSource = Consts.onHold;
            this.listBox_Dropped.ItemsSource = Consts.dropped;
            this.listBox_Favourites.ItemsSource = Consts.favourites;

            //ChangeProgressBar(true);
            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            #region Navigation Parameters
            //#region Lockscreen
            //string lockscreenKey = "WallpaperSettings";
            //string lockscreenValue = "0";
            //bool lockscreenValueExists = NavigationContext.QueryString.TryGetValue(lockscreenKey, out lockscreenValue);

            //if (lockscreenValueExists)
            //{
            //    string sendData = "fromLockscreen=" + true;
            //    NavigationService.Navigate(new Uri("/Settings.xaml?" + sendData, UriKind.Relative));
            //}
            //#endregion
            #endregion

            // Set the ItemSources
            this.listBox_ActivityFeed.ItemsSource = Consts.activityFeed;
            this.listBox_CurrentlyWatching.ItemsSource = Consts.currentlyWatching;
            this.listBox_PlanToWatch.ItemsSource = Consts.planToWatch;
            this.listBox_Completed.ItemsSource = Consts.completed;
            this.listBox_OnHold.ItemsSource = Consts.onHold;
            this.listBox_Dropped.ItemsSource = Consts.dropped;
            this.listBox_Favourites.ItemsSource = Consts.favourites;
             
            Debug.WriteLine("Entering MainPage OnNavigatedTo");
            if (Consts.LoggedIn == false)
            {
                Login();
            }


            #region Error Logging
            this.Loaded += async (sender, args) =>
            {
                await Task.Delay(5000);
                int totalCrashes = await BugSenseHandler.Instance.GetTotalCrashesNum();
                Debug.WriteLine("TotalCrashes: {0}", totalCrashes);
                if (totalCrashes > 3)
                {
                    await BugSenseHandler.Instance.ClearTotalCrashesNum();
                    Debug.WriteLine("TotalCrashes after clear: {0}", await BugSenseHandler.Instance.GetTotalCrashesNum());
                }
                Debug.WriteLine("Last Crash Error ID: {0}", await BugSenseHandler.Instance.GetLastCrashId());

                // Enable Async reporting
                BugSenseHandler.Instance.RegisterAsyncHandlerContext();
            };
            #endregion
        }

        #region Helper Methods
        private void ChangeProgressBar(bool isEnabled)
        {
            if (isEnabled) { ApplicationProgressBar.Visibility = System.Windows.Visibility.Visible; }
            else { ApplicationProgressBar.Visibility = System.Windows.Visibility.Collapsed; } 
            ApplicationProgressBar.IsEnabled = isEnabled;
        }

        public void UpdateLibraryListOnScreen()
        {
            // Set to nothing to reset the list
            Consts.currentlyWatching = new ObservableCollection<LibraryObject>();
            Consts.planToWatch = new ObservableCollection<LibraryObject>();
            Consts.completed = new ObservableCollection<LibraryObject>();
            Consts.onHold = new ObservableCollection<LibraryObject>();
            Consts.dropped = new ObservableCollection<LibraryObject>();


            // Reset the datasource to re-add items as they finish
            this.listBox_ActivityFeed.ItemsSource = Consts.activityFeed;
            this.listBox_CurrentlyWatching.ItemsSource = Consts.currentlyWatching;
            this.listBox_PlanToWatch.ItemsSource = Consts.planToWatch;
            this.listBox_Completed.ItemsSource = Consts.completed;
            this.listBox_OnHold.ItemsSource = Consts.onHold;
            this.listBox_Dropped.ItemsSource = Consts.dropped;
            this.listBox_Favourites.ItemsSource = Consts.favourites;

            Consts.cwLoaded = false;
            Consts.pTWLoaded = false;
            Consts.cLoaded = false;
            Consts.oHLoaded = false;
            Consts.dLoaded = false;
        }
        #endregion

        #region Startup Methods
        private async void Login()
        {
            if (App.loggedIn) { SwitchToMainLayout(); }
            else 
            {
                SwitchToLoginLayout();
                //if (Storage.DoesFileExist(Storage.SETTINGS))
                //{
                //    Debug.WriteLine("Login(): File Exists, but not loaded: Entering Infiniate Loading loop");
                //    bool fileLoaded = false;
                    
                //    for (int i = 0; i < 100; i++)
                //    {
                //        try
                //        {
                //            if (await Storage.LoadSettingsInfo()) 
                //            {
                //                if (string.IsNullOrEmpty(Consts.settings.userName) || string.IsNullOrEmpty(Consts.settings.auth_token))
                //                {
                //                    Debug.WriteLine("Login(): File found but empty: breaking out of loading loop");
                //                    fileLoaded = false;
                //                    break;
                //                }
                //                else
                //                {
                //                    Debug.WriteLine("Login(): File Found: breaking out of loading loop");
                //                    fileLoaded = true;
                //                    break;
                //                }
                //            }
                //        }
                //        catch (Exception) { }
                //    }

                //    if (fileLoaded) { SwitchToMainLayout(); }
                //    else { SwitchToLoginLayout(); }
                //}
                //else { SwitchToLoginLayout(); }
            }
        }

        private void SwitchToLoginLayout()
        {
            Debug.WriteLine("Switching to Login Layout");
            Consts.LoggedIn = false;
            notSignedIn = true;

            ApplicationBar.IsVisible = false;
            ContentPanel.Visibility = System.Windows.Visibility.Collapsed;
            LoginLayout.Visibility = System.Windows.Visibility.Visible;

            Debug.WriteLine("File not Found");
        }

        private async void SwitchToMainLayout()
        {
            Debug.WriteLine("Switching to Main Layout");
            Consts.LoggedIn = true;

            ContentPanel.Visibility = System.Windows.Visibility.Visible;
            userItem.Header = Storage.Settings.User.userName.Value;//Consts.settings.userName;

            Debug.WriteLine("\n\n\n");

            Debug.WriteLine("User Info Loaded: " + Storage.Settings.User.authToken.Value + " | " + Storage.Settings.User.userName.Value);//Consts.settings.auth_token + " | " + Consts.settings.userName);

            if (Consts.IsConnectedToInternet())
            {
                try
                {
                    Task<bool> loadStatusFeedBool = AnitroAPI.Hummingbird.GetStatusFeed(); //bool loadStatusFeedBool = await AnitroAPI.Hummingbird.GetStatusFeed(Consts.settings.userName);
                }
                catch (Exception) { }
            }
                //await loadStatusFeedBool;
            //listBox_ActivityFeed.ItemsSource = Consts.activityFeed;
            
            bool loadLibraryBool = await LoadLibrary().ConfigureAwait(false);
            //Task<bool> loadLibraryBool = LoadLibrary();

            BugSenseHandler.Instance.UserIdentifier = Storage.Settings.User.userName.Value;
        }

        private async Task<bool> LoadLibrary()
        {
            ChangeProgressBar(true);
            Debug.WriteLine("LoadLibrary(): Entering");

            
            if (notSignedIn) 
            {
                if (Consts.IsConnectedToInternet())
                {
                    AnitroAPI.Hummingbird.GenerateAllLibraries(Consts.LibrarySelection.All);
                }
                else
                {
                    MessageBox.Show("There is no network to download library data. Please check your network settings and try again.");
                }
            }//await GenerateLibrary(Consts.LibrarySelection.All); }
            else
            {
                ///
                /// If problems arise
                /// Consider awaiting GenerateLibrary
                /// 
                //Task<bool> gAL = await Hummingbird_API.Hummingbird.GetAllLibraries().ConfigureAwait(;
                AnitroAPI.Hummingbird.GetAllLibraries();
            }
            ChangeProgressBar(false);

            return true;
        }   
        #endregion

        #region LoginLayout Controlls
        #region Username
        private void UsernameTB_GotFocus(object sender, RoutedEventArgs e)
        {
            if (UsernameTB.Text == "username     ") // 5 Spaces to ensure that usernames dont reset upon leaving
            {
                UsernameTB.Text = "";
                //SolidColorBrush Brush1 = new SolidColorBrush();
                //Brush1.Color = Colors.Magenta;
                //UsernameTB.Foreground = Brush1;
            }
        }
        private void UsernameTB_LostFocus(object sender, RoutedEventArgs e)
        {
            if (UsernameTB.Text == String.Empty)
            {
                UsernameTB.Text = "username     "; // 5 Spaces to ensure that usernames dont reset upon leaving
                //SolidColorBrush Brush2 = new SolidColorBrush();
                //Brush2.Color = Colors.Blue;
                //UsernameTB.Foreground = Brush2;
            }
        }
        #endregion
        #region Password
        private void PasswordTB_GotFocus(object sender, RoutedEventArgs e)
        {
            if (PasswordTB.Text == "password     ") // 5 Spaces to ensure that Passwords dont reset upon leaving
            {
                PasswordTB.Text = "";
                //SolidColorBrush Brush1 = new SolidColorBrush();
                //Brush1.Color = Colors.Magenta;
                //UsernameTB.Foreground = Brush1;
            }
        }
        private void PasswordTB_LostFocus(object sender, RoutedEventArgs e)
        {
            if (PasswordTB.Text == String.Empty)
            {
                PasswordTB.Text = "password     "; // 5 Spaces to ensure that passwords dont reset upon leaving
                //SolidColorBrush Brush2 = new SolidColorBrush();
                //Brush2.Color = Colors.Blue;
                //UsernameTB.Foreground = Brush2;
            }
        }
        #endregion

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeProgressBar(true);

            Debug.WriteLine("Clicking Login Button");

            if (Consts.IsConnectedToInternet())
            {
                Consts.LoginError loginError;
                if (UsernameTB.Text == "username     " || PasswordTB.Text == "password     ") { loginError = Consts.LoginError.InfoNotEntered; }
                else
                {
                    try
                    {
                        loginError = await AnitroAPI.Hummingbird.PostLogin(UsernameTB.Text, PasswordTB.Text);
                        Debug.WriteLine("Getting Login Result");
                    }
                    catch (Exception) { loginError = Consts.LoginError.UnknownError; }
                }

                switch (loginError)
                {
                    case Consts.LoginError.None:
                        Debug.WriteLine(Storage.Settings.User.authToken.Value + " | " + Storage.Settings.User.userName.Value);
                        //Debug.WriteLine(Consts.settings.auth_token + " | " + Consts.settings.userName);
                        //loginErrors.Text = Consts.auth_token + " | "+Consts.userName;
                        //await Storage.SaveSettingsInfo();//Storage.SaveUserInfo();

                        ApplicationBar.IsVisible = true;
                        LoginLayout.Visibility = System.Windows.Visibility.Collapsed;

                        SwitchToMainLayout();

                        // Reset the textboxes
                        UsernameTB.Text = ""; //UsernameTB.Text = "username     ";
                        PasswordTB.Text = ""; //PasswordTB.Text = "password     ";

                        Debug.WriteLine("Success");
                        loginErrors.Text = "Success";

                        break;
                    case Consts.LoginError.InvalidLogin:
                        Debug.WriteLine("Invalid Login Credidentials");
                        loginErrors.Text = "Invalid Login Credidentials";
                        break;
                    case Consts.LoginError.ServerError:
                        Debug.WriteLine("Error connecting to hummingbird.me");
                        loginErrors.Text = "Error connecting to hummingbird.me";
                        break;
                    case Consts.LoginError.InfoNotEntered:
                        Debug.WriteLine("Credentials not entered");
                        loginErrors.Text = "Please type in your username/password";
                        break;
                    case Consts.LoginError.NetworkError:
                        Debug.WriteLine("Network Error");
                        loginErrors.Text = "Error Connecting to internet";
                        break;
                    case Consts.LoginError.UnknownError:
                    default:
                        Debug.WriteLine("An Unknown Error has Occured");
                        loginErrors.Text = "An Unknown Error has Occured";
                        break;
                }
            }
            else
            {
                Debug.WriteLine("Network Error");
                loginErrors.Text = "Error Connecting to internet";
            }

            ChangeProgressBar(false);
            Debug.WriteLine("Login Result Posted\n\n");
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            WebBrowserTask webBrowserTask = new WebBrowserTask();
            webBrowserTask.Uri = new Uri("http://hummingbird.me/users/sign_up", UriKind.Absolute);
            webBrowserTask.Show();
        }
        #endregion

        #region Application Bar
        private void AppBar_Search_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("Navigating to Search");
            NavigationService.Navigate(new Uri("/Search.xaml", UriKind.Relative));
            Debug.WriteLine("Leaving MainPage to Search");
        }
        private void AppBar_Refresh_Click(object sender, EventArgs e)
        {
            if (Consts.IsLibraryLoaded())
            {
                if (Consts.IsConnectedToInternet()) { }
                UpdateLibraryListOnScreen();

                AnitroAPI.Hummingbird.GenerateAllLibraries(Consts.LibrarySelection.All);
            }
        }
        private void AppBar_Settings_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("Navigating to Settings");
            NavigationService.Navigate(new Uri("/Settings.xaml", UriKind.Relative));
            Debug.WriteLine("Leaving MainPage to Settings");
        }
        private void AppBar_About_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("Navigating to About");
            NavigationService.Navigate(new Uri("/About.xaml", UriKind.Relative));
            Debug.WriteLine("Leaving MainPage to About");
        }
        #endregion

        #region Library Listbox
        #region ContextMenu
        private void PinToStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MenuItem menu = sender as MenuItem;
                LibraryObject libraryObject = menu.DataContext as LibraryObject;

                foreach (ShellTile tile in ShellTile.ActiveTiles)
                {
                    //Debug.WriteLine(tile.NavigationUri.OriginalString);
                    if (tile.NavigationUri.OriginalString.Contains(libraryObject.anime.slug)) { return; }
                }

                // Make the Tile

                //FlipTileData oFliptile = new FlipTileData();
                //oFliptile.Title = libraryObject.anime.title;
                //oFliptile.Count = 0;
                //oFliptile.BackTitle = libraryObject.anime.title;// Consts.APP_NAME;

                //oFliptile.BackContent = "";
                //oFliptile.WideBackContent = "";

                //oFliptile.SmallBackgroundImage = new Uri(libraryObject.anime.cover_image, UriKind.Absolute);
                //oFliptile.BackgroundImage = new Uri(libraryObject.anime.cover_image, UriKind.Absolute);
                //oFliptile.WideBackgroundImage = new Uri(libraryObject.anime.cover_image, UriKind.Absolute);

                //oFliptile.BackBackgroundImage = new Uri("/Assets/Tiles/FlipCycleTileMedium.png", UriKind.Relative);
                //oFliptile.WideBackBackgroundImage = new Uri("/Assets/Tiles/FlipCycleTileLarge.png", UriKind.Relative);

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

        private void AddToFavourites_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MenuItem menu = sender as MenuItem;
                LibraryObject libraryObject = menu.DataContext as LibraryObject;

                Consts.AddToFavourites(libraryObject);
            }
            catch (Exception) { }
        }

        private void RemoveFromFavourites_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MenuItem menu = sender as MenuItem;
                LibraryObject libraryObject = menu.DataContext as LibraryObject;

                Consts.RemoveFromFavourites(libraryObject);
            }
            catch (Exception) { }
        }

        #endregion

        private void library_listBox_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            try
            {
                if (((ListBox)sender).SelectedItem == null) { return; }
                LibraryObject selected = ((ListBox)sender).SelectedItem as LibraryObject;

                string sendData = "slug=" + selected.anime.slug + "&status=" + selected.status;
                NavigationService.Navigate(new Uri("/LibraryPage.xaml?" + sendData, UriKind.Relative));

                ((ListBox)sender).SelectedItem = null;
            }
            catch (Exception) { }
        }
        #endregion

        #region UserControlls
        private async void statusPostEnterCheck(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Debug.WriteLine("Enter Pressed");
                this.Focus();

                ChangeProgressBar(true);
                if (Consts.IsConnectedToInternet())
                {
                    try
                    {
                        await AnitroAPI.Hummingbird.PostStatusUpdate(updateStatusTextBox.Text);
                        listBox_ActivityFeed.ItemsSource = Consts.activityFeed;
                    }
                    catch (Exception) { }
                }
                ChangeProgressBar(false);
            }
        }

        private void user_activityFeed_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            //Throw exception to test crash logging
            //String a = null;
            //a.ToString(); 

            try
            {
                if (((ListBox)sender).SelectedItem == null) { return; }
                ActivityFeedObject selected = ((ListBox)sender).SelectedItem as ActivityFeedObject;

                if (!string.IsNullOrEmpty(selected.slug) && selected.header != Storage.Settings.User.userName.Value)//Consts.settings.userName)
                {
                    Consts.LibrarySelection lS = Consts.FindWhereExistsInLibrary(selected.slug);
                    if (lS != Consts.LibrarySelection.None)
                    {
                        string status = Consts.GetStatusFromLibrarySelection(lS);

                        string sendData = "slug=" + selected.slug + "&status=" + status;
                        NavigationService.Navigate(new Uri("/LibraryPage.xaml?" + sendData, UriKind.Relative));
                    }
                }

                ((ListBox)sender).SelectedItem = null;
            }
            catch (Exception) { }
        }
        #endregion
    }
}