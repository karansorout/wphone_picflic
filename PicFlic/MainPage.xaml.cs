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
        private App global = App.Current as App;
        private IsolatedStorageSettings applicationStorage;
        
        // Constructor
        public MainPage()
        {
            InitializeComponent();
            
            applicationStorage = IsolatedStorageSettings.ApplicationSettings;
        }

        //login button press handling  
        private void p1_button_login_Click(object sender, RoutedEventArgs e)
        {
            if ((p1_radioButton_picasa.IsChecked != true) && (p1_radioButton_flickr.IsChecked != true))
            {
                //please select a target ddrive
                MessageBox.Show(AppResources.p1_user_msg_selectdrive);
            }
            else
            {
                if (p1_radioButton_flickr.IsChecked == true)
                {
                    //Flicker is under construction
                    MessageBox.Show(AppResources.p1_flickr_underconstruction_msg);
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

                            gconnect();//google authentication

                        }

                    }
                }
            }
        }
             
      // get authentication from Google
      private void gconnect()
      {
          if (global.isLoginFlag == 0)
          {
              global.username = p1_login_email.Text;
              global.password = p1_passwordBox.Password;
          }
          WebClient webClient = new WebClient();
          Uri uri = new Uri(string.Format("https://www.google.com/accounts/ClientLogin?Email={0}&Passwd={1}&service=lh2&accountType=GOOGLE", global.username, global.password));
          webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(gconnectResults);
          webClient.DownloadStringAsync(uri);
       }

      //google authentication handling
      private void gconnectResults(object sender, DownloadStringCompletedEventArgs e)
      {
          try
          {
              if (e.Result != null && e.Error == null)//auth sucess
              {
                  int idx = e.Result.IndexOf("Auth=");//capture the index of google auth token
                  if (idx != -1)
                  {
                      global.gtoken = e.Result.Substring(idx + 5);//capture the gtoken string
                  }
                  if (global.gtoken != "")//if found
                  {
                     //user logged in successfully, lets move necessary info to store
                      global.isLoginFlag = 1;//Login flag is set

                      // Saving logon details to the isolated storage.
                      if (applicationStorage.Contains("username"))
                          applicationStorage["username"] = global.username;
                      else
                          applicationStorage.Add("username", global.username);

                      if (applicationStorage.Contains("password"))
                          applicationStorage["password"] = global.password;
                      else
                          applicationStorage.Add("password", global.password);

                      if (applicationStorage.Contains("isLoginFlag"))
                          applicationStorage["isLoginFlag"] = global.isLoginFlag;
                      else
                          applicationStorage.Add("isLoginFlag", global.isLoginFlag);
                     

                     this.NavigationService.Navigate(new Uri("/gAlbumsListPage.xaml", UriKind.Relative));//navigate to picasa album list page
                    

                    
                    return;
                  }
              }
              //Authentication failed, please Check your Email and Password
              MessageBox.Show(AppResources.p1_authFailed);
          }
          catch (WebException)
          {
              //Unable to authrize from google, excetion=
              MessageBox.Show(AppResources.p1_unableToAuth + e.Error);
          }
      }
        
      //AboutUs Click handler
      private void AboutPicFlic_Click(object sender, EventArgs e)
      {
          MessageBox.Show(AppResources.p1_aboutApp);
      }

      protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
      {
          base.OnNavigatedTo(e);
          string logout = string.Empty;
          //Logout handling
          if (NavigationContext.QueryString.TryGetValue("logout", out logout))
          {
              LogoutUser();  
          }
      }

      private void LogoutUser()
      {
          global.isLoginFlag = 0;

          if (applicationStorage.Contains("isLoginFlag"))
              applicationStorage["isLoginFlag"] = global.isLoginFlag;
          else
              applicationStorage.Add("isLoginFlag", global.isLoginFlag);

          if (applicationStorage.Contains("username"))
              applicationStorage["username"] = string.Empty;
          else
              applicationStorage.Add("username", global.isLoginFlag);

          if (applicationStorage.Contains("password"))
              applicationStorage["password"] = string.Empty;
          else
              applicationStorage.Add("password", string.Empty);

          while (NavigationService.CanGoBack)//is history stack available?
              NavigationService.RemoveBackEntry();//remove history stack
          global.gtoken = string.Empty;//clear auth token
          global.username = string.Empty;
          global.password = string.Empty;
          NavigationContext.QueryString.Clear();//clear all lists
      }

      //protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
      //{
      //    base.OnNavigatedTo(e);
          
      //}

      //Pin to start / live tile handler
      private void PinToStart_Click(object sender, EventArgs e)
      {
          StandardTileData standardTileData = new StandardTileData();
          standardTileData.BackgroundImage = new Uri("/Images/background.png", UriKind.Relative);
          standardTileData.Title = "*****PicFlic*****";
          standardTileData.Count = 0;
          standardTileData.BackTitle = "Flick your picasa images";
          standardTileData.BackContent = "";
          standardTileData.BackBackgroundImage = new Uri("/Images/background2.png", UriKind.Relative);

          // Check if app is already pinned
          ShellTile tiletopin = ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains("MainPage.xaml"));
          if (tiletopin == null)
          {
              ShellTile.Create(new Uri("/MainPage.xaml", UriKind.Relative), standardTileData);//home page
          }
          else
          {
              //PicFlic is already Pinned
              MessageBox.Show(AppResources.p1_alreadyPinned);
          }
      }


    }//mainpage-application
}//namespace