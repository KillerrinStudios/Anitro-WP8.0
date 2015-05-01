using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Windows.Phone.System.UserProfile;
using AnitroAPI; //using Hummingbird_API;
using System.Diagnostics;
using System.Net.NetworkInformation;

namespace Hummingbird
{
    public partial class Settings : PhoneApplicationPage
    {
        public string pageName = "Settings";
        //private MainPage mainPage;

        public Settings()
        {
            //mainPage = ((MainPage)(((System.Windows.Controls.ContentControl)(App.RootFrame)).Content));
    
            InitializeComponent();
            Debug.WriteLine("Initializing Settings");

            #region Set values to settings info
            // Set Text
            logoutUsernameText.Text = Storage.Settings.User.userName.Value;// Consts.settings.userName;
            scheduledTaskRunner.Text = "scheduled task last run: " + Storage.Settings.ScheduledTaskLastRun.Value.ToString();// Consts.settings.ScheduledTaskLastRun.ToString();

            // Set the switches
            Consts.HasAccessForLockscreen = LockScreenManager.IsProvidedByCurrentApplication;
            lockscreenSwitch.IsChecked = Consts.HasAccessForLockscreen;

            randomize_Favourites.IsChecked = Storage.Settings.Lockscreen.randomizeFavourites.Value;
            randomize_CurrentlyWatching.IsChecked = Storage.Settings.Lockscreen.randomizeCurrentlyWatching.Value;
            randomize_PlanToWatch.IsChecked = Storage.Settings.Lockscreen.randomizePlanToWatch.Value;
            randomize_Completed.IsChecked = Storage.Settings.Lockscreen.randomizeCompleted.Value;
            randomize_OnHold.IsChecked = Storage.Settings.Lockscreen.randomizeOnHold.Value;
            randomize_Dropped.IsChecked = Storage.Settings.Lockscreen.randomizeDropped.Value;

            #endregion
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            string fromLockscreen = "False";
            NavigationContext.QueryString.TryGetValue("fromLockscreen", out fromLockscreen);

            switch(fromLockscreen)
            {
                case "True":
                    NavigationService.RemoveBackEntry();
                    mainPivot.SelectedIndex = 1;
                    generalPivot.IsEnabled = false;
                    break;
                case "False":
                    break;
            }
            
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }


        private async void logoutButton_Click(object sender, RoutedEventArgs e)
        {
            CustomMessageBox messageBox = new CustomMessageBox()
            {
                Caption = "Logout?",
                Message = "Are you sure you want to log out of " + Storage.Settings.User.userName.Value,//Consts.settings.userName,
                LeftButtonContent = "yes",
                RightButtonContent = "no",
                IsFullScreen = false
            };

            messageBox.Dismissed += (s1, e1) =>
            {
                switch (e1.Result)
                {
                    case CustomMessageBoxResult.LeftButton:
                        Storage.Settings.User.userName.SetDefault();//Consts.settings.userName = "";
                        Storage.Settings.User.authToken.SetDefault();//Consts.settings.auth_token = "";
                        Consts.LoggedIn = false;
                        App.loggedIn = false;

                        //Consts.ClearLibrary(Consts.LibrarySelection.All);
                        Storage.DeleteAnimeLibrary();

                        //Consts.ClearSettingsInfo();
                        //Storage.DeleteSettingsInfo();
                        //Storage.SaveSettingsInfo();

                        IEnumerable<ShellTile> tiles = ShellTile.ActiveTiles;
                        foreach (ShellTile t in tiles)
                        {
                            try
                            {
                                t.Delete();
                            }
                            catch (Exception) { }
                        }

                        //NavigationService.Navigate(new Uri(string.Format("/MainPage.xaml?random={0}", Guid.NewGuid()), UriKind.Relative));    
                        NavigationService.GoBack();
                        break;
                    case CustomMessageBoxResult.RightButton:
                    case CustomMessageBoxResult.None:
                    default:
                        // Do nothing.
                        break;
                }
            };


            messageBox.Show();
        }

        #region Application Bar
        private void AppBar_Search_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("Navigating to Search");
            NavigationService.Navigate(new Uri("/Search.xaml", UriKind.Relative));
            Debug.WriteLine("Leaving Settings to Search");
        }

        private void AppBar_About_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("Navigating to About");
            NavigationService.Navigate(new Uri("/About.xaml", UriKind.Relative));
            Debug.WriteLine("Leaving Settings to About");
        }
        #endregion

        #region Lockscreen
        private async void lockscreenSwitch_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                Consts.HasAccessForLockscreen = LockScreenManager.IsProvidedByCurrentApplication;
                if (!Consts.HasAccessForLockscreen)
                {
                    // If you're not the provider, this call will prompt the user for permission.
                    // Calling RequestAccessAsync from a background agent is not allowed.
                    var accessRequested = await LockScreenManager.RequestAccessAsync();

                    Consts.HasAccessForLockscreen = (accessRequested == LockScreenRequestResult.Granted);
                    if (Consts.HasAccessForLockscreen)
                    {
                        lockscreenUpdateButton_Clicked(sender, e);
                        //lockscreenSwitch.IsChecked = Consts.HasAccessForLockscreen;
                        //lockscreenSwitch.Opacity = 0.75;
                    }
                }
            }
            catch (Exception) { }
        }

        private void lockscreenSwitch_UnChecked(object sender, RoutedEventArgs e)
        {
            CustomMessageBox messageBox = new CustomMessageBox()
            {
                Caption = "Disable lockscreen?",
                Message = "To disable lock screen you need to change your lockscreen provider, would you like to go to that settings screen now?",
                LeftButtonContent = "yes",
                RightButtonContent = "no",
                IsFullScreen = false
            };

            messageBox.Dismissed += (s1, e1) =>
            {
                switch (e1.Result)
                {
                    case CustomMessageBoxResult.LeftButton:
                        lockscreenButton_Clicked(sender, e);
                        break;
                    case CustomMessageBoxResult.RightButton:
                        //lockscreenSwitch.IsChecked = true;
                        // Do nothing.
                        break;
                    case CustomMessageBoxResult.None:
                        //lockscreenSwitch.IsChecked = true;
                        // Do nothing.
                        break;
                    default:
                        break;
                }
            };

            messageBox.Show();
        }

        private async void lockscreenButton_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                var op = await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings-lock:"));
            }
            catch(Exception)
            {

            }
        }

        private async void lockscreenUpdateButton_Clicked(object sender, RoutedEventArgs e)
        {
            if (Consts.IsConnectedToInternet())
            {
                if (!Consts.HasAccessForLockscreen) { return; }
                else
                {
                    //Lockscreen_Helper.DeleteLockscreenImage();
                    await Lockscreen_Helper.SetRandomImageFromLibrary();
                    MessageBox.Show("Locksceen has been updated");
                }
            }
        }

        #region Randomize
        #region UnChecked
        private void randomize_Dropped_UnChecked(object sender, RoutedEventArgs e)
        {
            Storage.Settings.Lockscreen.randomizeDropped.Value = false;
        }

        private void randomize_OnHold_UnChecked(object sender, RoutedEventArgs e)
        {
            Storage.Settings.Lockscreen.randomizeOnHold.Value = false;

        }

        private void randomize_Completed_UnChecked(object sender, RoutedEventArgs e)
        {
            Storage.Settings.Lockscreen.randomizeCompleted.Value = false;

        }

        private void randomize_PlanToWatch_UnChecked(object sender, RoutedEventArgs e)
        {
            Storage.Settings.Lockscreen.randomizePlanToWatch.Value = false;

        }

        private void randomize_CurrentlyWatching_UnChecked(object sender, RoutedEventArgs e)
        {
            Storage.Settings.Lockscreen.randomizeCurrentlyWatching.Value = false;

        }

        private void randomize_Favourites_UnChecked(object sender, RoutedEventArgs e)
        {
            Storage.Settings.Lockscreen.randomizeFavourites.Value = false;
        }
        #endregion
        #region Checked
        private void randomize_Favourites_Checked(object sender, RoutedEventArgs e)
        {
            Storage.Settings.Lockscreen.randomizeFavourites.Value = true;
        }

        private void randomize_CurrentlyWatching_Checked(object sender, RoutedEventArgs e)
        {
            Storage.Settings.Lockscreen.randomizeCurrentlyWatching.Value = true;
        }

        private void randomize_PlanToWatch_Checked(object sender, RoutedEventArgs e)
        {
            Storage.Settings.Lockscreen.randomizePlanToWatch.Value = true;
        }

        private void randomize_Completed_Checked(object sender, RoutedEventArgs e)
        {
            Storage.Settings.Lockscreen.randomizeCompleted.Value = true;
        }

        private void randomize_OnHold_Checked(object sender, RoutedEventArgs e)
        {
            Storage.Settings.Lockscreen.randomizeOnHold.Value = true;
        }

        private void randomize_Dropped_Checked(object sender, RoutedEventArgs e)
        {
            Storage.Settings.Lockscreen.randomizeDropped.Value = true;
        }
        #endregion
        #endregion
        #endregion
    }

}