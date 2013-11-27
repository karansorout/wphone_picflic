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
        private App global = App.Current as App;
           
        public gAlbumsListPage()
        {
            InitializeComponent();
            var gtoken = PhoneApplicationService.Current.State["gtoken"];
            var username = PhoneApplicationService.Current.State["username"];
            global.gtoken = gtoken.ToString();
            global.username = username.ToString();
            
            fetch_galbumslist();//download list of picasa albums
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
            global.isLogoutFlag = 1;
            this.NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
        }

    }//app page    
}//namespace
