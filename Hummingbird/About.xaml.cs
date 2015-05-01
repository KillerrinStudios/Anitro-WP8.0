using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using System.Diagnostics;

namespace Hummingbird
{
    public partial class About : PhoneApplicationPage
    {
        public About()
        {
            InitializeComponent();
        }

        private void EmailSupportButton(object sender, System.Windows.Input.GestureEventArgs e)
        {
            EmailComposeTask emailComposeTask = new EmailComposeTask();

            emailComposeTask.Subject = "support - Anitro";
            emailComposeTask.Body = "";
            emailComposeTask.To = "support@killerrin.com";
            emailComposeTask.Cc = "";
            emailComposeTask.Bcc = "";

            emailComposeTask.Show();
        }

        private void EmailFeedbackButton(object sender, System.Windows.Input.GestureEventArgs e)
        {
            EmailComposeTask emailComposeTask = new EmailComposeTask();

            emailComposeTask.Subject = "feedback - Anitro";
            emailComposeTask.Body = "";
            emailComposeTask.To = "support@killerrin.com";
            emailComposeTask.Cc = "";
            emailComposeTask.Bcc = "";

            emailComposeTask.Show();
        }

        private void AppBar_Settings_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("Navigating to Settings");
            NavigationService.Navigate(new Uri("/Settings.xaml", UriKind.Relative));
            Debug.WriteLine("Leaving MainPage to Settings");
        }

        private void AppBar_Search_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("Navigating to Search");
            NavigationService.Navigate(new Uri("/Search.xaml", UriKind.Relative));
            Debug.WriteLine("Leaving MainPage to Search");
        }
    }
}