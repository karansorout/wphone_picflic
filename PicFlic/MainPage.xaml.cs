using System;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using Microsoft.Phone.Shell;

namespace PicFlic
{
    public partial class MainPage : PhoneApplicationPage
    {
        //private App global = App.Current as App;
        private string gtoken = string.Empty;
        private string username = string.Empty;
        private string password = string.Empty;
      
        // Constructor
        public MainPage()
        {
            InitializeComponent();
        }

        //login button press handling  
        private void p1_button_login_Click(object sender, RoutedEventArgs e)
        {
            if ((p1_radioButton_picasa.IsChecked != true) && (p1_radioButton_flickr.IsChecked != true))
            {
                MessageBox.Show("Please select the drive to be connected");
            }
            else
            {
                if (p1_radioButton_flickr.IsChecked == true)
                {
                    MessageBox.Show("Connection with Flickr is under construction...");
                }
                else
                {
                    //empty username or pwd field check
                    if (String.IsNullOrWhiteSpace(p1_login_email.Text) || String.IsNullOrWhiteSpace(p1_passwordBox.Password))
                    {
                        //MessageBox.Show("Either 'Username' or 'Password' is empty");
                        MessageBox.Show(AppResources.p1_user_msg_emptyfields);
                    }
                    else
                    {
                        //valid id or pwd check
                        bool bInvalid_id = Regex.IsMatch(p1_login_email.Text,
                          @"^(?("")("".+?""@)|(([0-9a-zA-Z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-zA-Z])@))" +
                          @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,6}))$");
                        if (bInvalid_id != true)
                        {
                            MessageBox.Show(AppResources.p1_user_msg_invalidemail);
                        }
                        else
                        {
                            //all check passed, go for authentication
                            username = p1_login_email.Text;
                            password = p1_passwordBox.Password;

                            //MessageBox.Show("1.just before calling gconnect function");
                            gconnect();

                        }

                    }
                }
            }
        }
             
      // get authentication from Google
      private void gconnect()
      {
          //MessageBox.Show("2. before welclient connection");
          WebClient webClient = new WebClient();
          //MessageBox.Show("credentials about to be submitted="+username+"/password="+password);
          Uri uri = new Uri(string.Format("https://www.google.com/accounts/ClientLogin?Email={0}&Passwd={1}&service=lh2&accountType=GOOGLE", username, password));
          webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(gconnectResults);
          webClient.DownloadStringAsync(uri);
       }

      //google authentication handling
      private void gconnectResults(object sender, DownloadStringCompletedEventArgs e)
      {
          try
          {
              
              if (e.Result != null && e.Error == null)
              {
                  int index = e.Result.IndexOf("Auth=");
                  if (index != -1)
                  {
                      gtoken = e.Result.Substring(index + 5);
                  }
                  if (gtoken != "")
                  {
                    PhoneApplicationService.Current.State["gtoken"] = gtoken;
                    PhoneApplicationService.Current.State["username"] = username;

                    this.NavigationService.Navigate(new Uri("/gAlbumsListPage.xaml", UriKind.Relative));
                    return;
                  }
              }
              MessageBox.Show("Authentication failed, please cCheck your Email and Password");
          }
          catch (WebException)
          {
              MessageBox.Show("Unable to authrize from google, excetion="+e.Error);
          }
      }
    }//mainpage-application
}//namespace