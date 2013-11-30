using System;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.IO.IsolatedStorage;

namespace PicFlic
{
    public partial class gAlbumsListPage : PhoneApplicationPage
    {
        private static App global = App.Current as App;
        private IsolatedStorageSettings applicationStorage;
 
        public gAlbumsListPage()
        {
            InitializeComponent();

            applicationStorage = IsolatedStorageSettings.ApplicationSettings;

            if (string.IsNullOrEmpty(global.gtoken))
            {
                //LOgin required
                gconnect();
            }
            else
            {
                //LOgin available
                fetch_galbumslist();//download list of picasa albums
            }
            
            
        }

        // get authentication from Google
        private void gconnect()
        {
            if (global.isLoginFlag == 1)
            {
                if (applicationStorage.Contains("username"))
                {
                    global.username = (string)applicationStorage["username"];
                }

                if (applicationStorage.Contains("password"))
                {
                    global.password = (string)applicationStorage["password"];
                }
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

                        fetch_galbumslist();//download list of picasa albums
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
        
                //fetch the list of albums
                public void fetch_galbumslist()
                {
                    WebClient webClient = new WebClient();
                    webClient.Headers[HttpRequestHeader.Authorization] = "GoogleLogin auth=" + global.gtoken;
                    Uri uri = new Uri(String.Format("http://picasaweb.google.com/data/feed/api/user/{0}?alt=json", global.username), UriKind.Absolute);
                    webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(galbums_data);
                    webClient.DownloadStringAsync(uri);
                }

                //handling gAlbums data
                public void galbums_data(object sender, DownloadStringCompletedEventArgs e)
                {
                    try
                    {
                        if (e.Result == null || e.Error != null)
                        {
                            //Cannot get albums data from Picasa server - null returned!
                            MessageBox.Show(AppResources.p2_AlbumList_NullReturned);
                            return;
                        }
                        else
                        {
                            // Deserialize JSON
                            IDictionary<string, object> json = (IDictionary<string, object>)SimpleJson.DeserializeObject(e.Result);
                            // Get Feed
                            IDictionary<string, object> feed = (IDictionary<string, object>)json["feed"];
                            // GET authers
                            IList author = (IList)feed["author"];
                            // get author idx=1
                            IDictionary<string, object> firstAuthor = (IDictionary<string, object>)author[0];
                            // get album counts
                            IList entries = (IList)feed["entry"];

                            global.galbumlist.Clear();//clear old entries

                            // Find album details
                            for (int i = 0; i < entries.Count; i++)
                            {
                                // Create a new Album
                                Album album = new Album();
                                // Album entry object
                                IDictionary<string, object> entry = (IDictionary<string, object>)entries[i];
                                // Published object
                                IDictionary<string, object> published = (IDictionary<string, object>)entry["published"];
                                // Get published date
                                album.published = (string)published["$t"];
                                // Get album entry
                                IDictionary<string, object> title = (IDictionary<string, object>)entry["title"];
                                // Get album title
                                album.title = (string)title["$t"];
                                // Link List
                                IList link = (IList)entry["link"];
                                // First link is album data link object
                                IDictionary<string, object> href = (IDictionary<string, object>)link[0];
                                // Get album href
                                album.href = (string)href["href"];
                                // Media group object
                                IDictionary<string, object> mediagroup = (IDictionary<string, object>)entry["media$group"];
                                // Get thumbnails
                                IList mediathumbnailList = (IList)mediagroup["media$thumbnail"];
                                // First thumbnail object for album cover
                                var mediathumbnail = (IDictionary<string, object>)mediathumbnailList[0];
                                // Get thumbnail url
                                album.thumbnail = (string)mediathumbnail["url"];
                                // Add album to albums list
                                global.galbumlist.Add(album);
                            }

                            // put albums list to display
                            AlbumsListBox.ItemsSource = global.galbumlist;
                        }
                    }
                    catch (WebException)
                    {
                        //Cannot get albums data from Picasa server - web exception occured!
                        MessageBox.Show(AppResources.p2_webException);
                    }
                    catch (KeyNotFoundException)
                    {
                        //No Albums Found or Cannot load images from Picasa Server
                        MessageBox.Show(AppResources.p2_KeyNotFoundError);
                    }
                }

                
        // Handle selection from AlbumListBox
        private void AlbumsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // If real selection is happened, go to a AlbumPage
            if (AlbumsListBox.SelectedIndex == -1) return;
            global.selectedAlbumIndex = AlbumsListBox.SelectedIndex;

            //MessageBox.Show("ready to go to next page");
            this.NavigationService.Navigate(new Uri("/gAlbumPage.xaml?SelectedIndex=" + AlbumsListBox.SelectedIndex, UriKind.Relative));
            AlbumsListBox.SelectedIndex = -1;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            NavigationService.RemoveBackEntry();//no go to login page using back button
            base.OnNavigatedTo(e);
            if (AlbumsListBox != null)
            {
                AlbumsListBox.SelectionChanged += AlbumsListBox_SelectionChanged;
            }
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (AlbumsListBox != null)
            {
                AlbumsListBox.SelectionChanged -= AlbumsListBox_SelectionChanged;
                //global.createNewAlbumFlag = 1;
            }
        }

        private void p2_NewAlbumForm(object sender, EventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/NewAlbumForm.xaml", UriKind.Relative));
        }

        //Logout initiated
        private void Logout_Click(object sender, EventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/MainPage.xaml?logout=1", UriKind.Relative));
        }

        //AboutUs Click handler
        private void AboutPicFlic_Click(object sender, EventArgs e)
        {
            MessageBox.Show("***PICFLIC - \"Flick your Pics in cloud\"***" + "\n" + "\n"
                + AppResources.p1_aboutApp + "\n" + "\n" +
                AppResources.p1_aboutApp1 + "\n" +
                AppResources.p1_aboutApp2 + "\n" +
                AppResources.p1_aboutApp3 + "\n" +
                AppResources.p1_aboutApp4 + "\n" +
                AppResources.p1_aboutApp5 + "\n" +
                AppResources.p1_aboutApp6);
        }

        //Pin to start / live tile handler
        //private void PinToStart_Click(object sender, EventArgs e)
        //{
        //    StandardTileData standardTileData = new StandardTileData();
        //    standardTileData.BackgroundImage = new Uri("/Images/background.png", UriKind.Relative);
        //    standardTileData.Title = "*****PicFlic*****";
        //    standardTileData.Count = 0;
        //    standardTileData.BackTitle = "Flick your picasa images";
        //    standardTileData.BackContent = "";
        //    standardTileData.BackBackgroundImage = new Uri("/Images/background2.png", UriKind.Relative);

        //    // Check if app is already pinned
        //    ShellTile tiletopin = ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains("gAlbumListPage.xaml"));
        //    if (tiletopin == null)
        //    {
        //        ShellTile.Create(new Uri("/MainPage.xaml", UriKind.Relative), standardTileData);//home page
        //    }
        //    else
        //    {
        //        //PicFlic is already Pinned
        //        MessageBox.Show(AppResources.p1_alreadyPinned);
        //    }
        //}

    }//app page    
}//namespace
