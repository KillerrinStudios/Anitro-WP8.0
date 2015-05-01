using System.Diagnostics;
using System.Windows;
using Microsoft.Phone.Scheduler;
using AnitroAPI; //using Hummingbird_API;
using System;
using System.Net.NetworkInformation;

namespace AnitroScheduler
{
    public class ScheduledAgent : ScheduledTaskAgent
    {
        /// <remarks>
        /// ScheduledAgent constructor, initializes the UnhandledException handler
        /// </remarks>
        static ScheduledAgent()
        {
            // Subscribe to the managed exception handler
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                Application.Current.UnhandledException += UnhandledException;
            });
        }

        /// Code to execute on Unhandled Exceptions
        private static void UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                Debugger.Break();
            }
        }

        /// <summary>
        /// Agent that runs a scheduled task
        /// </summary>
        /// <param name="task">
        /// The invoked task
        /// </param>
        /// <remarks>
        /// This method is called when a periodic or resource intensive task is invoked
        /// </remarks>
        protected async override void OnInvoke(ScheduledTask task)
        {
            Debug.WriteLine("Entering Background Agent");

            #region Early Exit Checks
            // If settings don't exist, chances are that their wont be any library info, so just exit now.
            if (Storage.Settings.User.userName.IsDefault() || Storage.Settings.User.authToken.IsDefault())//Storage.DoesFileExist(Storage.SETTINGS))
            {
                Debug.WriteLine("Login Info Not Found: Exiting Background Agent");
                NotifyComplete();
                return;
            }

            if (!(Windows.Phone.System.UserProfile.LockScreenManager.IsProvidedByCurrentApplication))
            {
                Debug.WriteLine("Lockscreen Not Provider: Exiting Background Agent");
                NotifyComplete();
                return;
            }

            if (!Consts.IsConnectedToInternet())
            {
                Debug.WriteLine("Network Not Found");
                NotifyComplete();
                return;
            }
            #endregion


            // Update the Lockscreen Image
            await Lockscreen_Helper.SetRandomImageFromLibrary();

            #region Updating Settings
            Debug.WriteLine("Updating the settings");
            //int ctr = 0;
            
            //while (true)
            //{
            //    // Close the background task if it takes too long to load the file.
            //    if (ctr >= 20) { NotifyComplete(); }

            //    // Check if its already loaded into memory
            //    if (!(string.IsNullOrEmpty(Consts.settings.userName)))
            //    {
            //        break;
            //    }

            //    // If its not loaded into memory, try loading it into memory
            //    if (await Storage.LoadSettingsInfo())
            //    {
            //        break;
            //    }

            //    // if not, bring up the counter...
            //    ctr++;
            //}
        
            //Consts.settings.ScheduledTaskLastRun = DateTime.Now;
            //await Storage.SaveSettingsInfo();

            Storage.Settings.ScheduledTaskLastRun.Value = DateTime.Now;
            #endregion

            // If a debugger is attached, relaunch it in 30 seconds for testing purposes.
            if (Debugger.IsAttached)
            {
                ScheduledActionService.LaunchForTest(task.Name, TimeSpan.FromSeconds(30));
            }
            
            Debug.WriteLine("Exiting Background Agent");
            NotifyComplete();
        }
    }
}