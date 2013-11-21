using System;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Phone.Tasks;
using System.IO;

namespace PicFlic
{
    public partial class gAlbumPage : PhoneApplicationPage
    {
        App global = App.Current as App;//load present state of app
        string img_href = string.Empty;//image upload url initialization
        //PhotoChooserTask photoChooserTask;//object for photo choser task
        
        public gAlbumPage()
        {
            InitializeComponent();
            
            //set page name, indicating current location
            p3_albumimagespage_name.Text = String.Format("PicFlic>Albums>" + global.galbumlist[global.selectedAlbumIndex].title);

            //int selectedAlbumIndex = Convert.ToInt32(global.selectedAlbumIndex);
            //string href_new = global.galbumlist[global.selectedAlbumIndex].href;
            
        }

        // Navigate to this page
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Phone backbutton press
            if (e.NavigationMode == System.Windows.Navigation.NavigationMode.Back)
            {
                //use existing images list, no fresh download
                AlbumImagesListBox.ItemsSource = global.galbumImages;
                AlbumImagesListBox.SelectedIndex = -1;
                return;
            }

            // forward navigation from login page
            IDictionary<string, string> parameters = this.NavigationContext.QueryString;
            if (global.selectedAlbumIndex >= 0)
            {
                //Get all images & properties of a google album
                GetImages();
        
            }
        }


        // Start loading images from Google
        private void GetImages()
        {
            WebClient webClient = new WebClient();
            //int selected_galbumIndex = Convert.ToInt32(global.selectedAlbumIndex);
            string auth = "GoogleLogin auth=" + global.gtoken;
            webClient.Headers[HttpRequestHeader.Authorization] = auth;
            Uri uri = new Uri(global.galbumlist[global.selectedAlbumIndex].href, UriKind.Absolute);
            webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(ImagesDownloaded);
            webClient.DownloadStringAsync(uri);
        }

        public void ImagesDownloaded(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {
                if (e.Result == null || e.Error != null)
                {
                    MessageBox.Show("Cannot load images from Picasa server!");
                    return;
                }
                else
                {
                    // Deserialize JSON string to dynamic object
                    IDictionary<string, object> json = (IDictionary<string, object>)SimpleJson.DeserializeObject(e.Result);
                    // Feed object
                    IDictionary<string, object> feed = (IDictionary<string, object>)json["feed"];
                    // Number of photos object
                    IDictionary<string, object> numberOfPhotos = (IDictionary<string, object>)feed["gphoto$numphotos"];
                    // Entries List
                    var entries = (IList)feed["entry"];
                    // clear previous images from albumImages
                    
                    global.galbumImages.Clear();
                    // Find image details from entries
                    for (int i = 0; i < entries.Count; i++)
                    {
                        // Create a new albumImage
                        AlbumImage albumImage = new AlbumImage();
                        // Image entry object
                        IDictionary<string, object> entry = (IDictionary<string, object>)entries[i];
                        // Image title object
                        IDictionary<string, object> title = (IDictionary<string, object>)entry["title"];
                        // Get album title
                        albumImage.title = (string)title["$t"];
                        // Album content object
                        IDictionary<string, object> content = (IDictionary<string, object>)entry["content"];
                        // Get image src url
                        albumImage.content = (string)content["src"];
                        // Image width object
                        IDictionary<string, object> width = (IDictionary<string, object>)entry["gphoto$width"];
                        // Get image width
                        albumImage.width = (string)width["$t"];
                        // Image height object
                        IDictionary<string, object> height = (IDictionary<string, object>)entry["gphoto$height"];
                        // Get image height
                        albumImage.height = (string)height["$t"];
                        // Image size object
                        IDictionary<string, object> size = (IDictionary<string, object>)entry["gphoto$size"];
                        // Get image size 
                        albumImage.size = (string)size["$t"];
                        // Image media group List
                        IDictionary<string, object> mediaGroup = (IDictionary<string, object>)entry["media$group"];
                        IList mediaThumbnailList = (IList)mediaGroup["media$thumbnail"];
                        // First thumbnail object
                        IDictionary<string, object> mediathumbnail = (IDictionary<string, object>)mediaThumbnailList[0];
                        // Get thumnail url
                        albumImage.thumbnail = (string)mediathumbnail["url"];
                        // Add albumImage to albumImages Collection
                        global.galbumImages.Add(albumImage);
                    }
                    // Add albumImages to AlbumImagesListBox
                    AlbumImagesListBox.ItemsSource = global.galbumImages;
                }
            }
            catch (WebException)
            {
                MessageBox.Show("Cannot load images from Picasa server!");
            }
            catch (KeyNotFoundException)
            {
                MessageBox.Show("Cannot load images from Picasa Server - JSON parsing error happened!");
            }
        }

        //handling upload pic
        private void p4_appbar_uploadpic(object sender, EventArgs e)
        {
            //MessageBox.Show("p4_appbar_uploadpic works!");

            PhotoChooserTask task = new PhotoChooserTask();
            task.ShowCamera = true;
            task.Completed += task_Completed;
            task.Show();
        }

        private void task_Completed(object sender, PhotoResult e)
        {

            if (e.TaskResult == TaskResult.OK)
            {

                MessageBox.Show(e.ChosenPhoto.Length.ToString());
                UploadPhoto(e.ChosenPhoto);

            }

        }


        //upload image
        private void UploadPhoto(Stream stream)
        {
            const int BLOCK_SIZE = 4096;
            //img_href.Replace("entry", "feed");
            //Uri uri = new Uri("http://picasaweb.google.com/data/feed/api/user/default/albumid/default", UriKind.Absolute);
            Uri uri = new Uri(img_href, UriKind.Absolute);

            WebClient wc = new WebClient();
            string AuthToken = global.gtoken;
            wc.Headers[HttpRequestHeader.Authorization] = "GoogleLogin auth=" + AuthToken;
            wc.Headers[HttpRequestHeader.ContentLength] = stream.Length.ToString();
            wc.Headers[HttpRequestHeader.ContentType] = "image/jpeg";
            wc.AllowReadStreamBuffering = true;
            wc.AllowWriteStreamBuffering = true;
            // what to do when write stream is open
            wc.OpenWriteCompleted += (s, args) =>
            {
                using (BinaryReader br = new BinaryReader(stream))
                {
                    using (BinaryWriter bw = new BinaryWriter(args.Result))
                    {
                        long bCount = 0;
                        long fileSize = stream.Length;
                        byte[] bytes = new byte[BLOCK_SIZE];
                        do
                        {
                            bytes = br.ReadBytes(BLOCK_SIZE);
                            bCount += bytes.Length;
                            bw.Write(bytes);
                        }
                        while (bCount < fileSize);
                    }
                }
            };

            // what to do when writing is complete
            wc.WriteStreamClosed += (s, args) =>
            {
                MessageBox.Show("Upload Complete");
            };

            // Write to the WebClient
            wc.OpenWriteAsync(uri, "POST");
        }
        

        //dummy handling of selection change
        private void AlbumImagesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //MessageBox.Show("Inside Albumimageslistbox_selectionchanged loop on 'Album Images' Page");
            //if (AlbumImagesListBox.SelectedIndex == -1) return;
            global.selectedImageIndex = AlbumImagesListBox.SelectedIndex;
            this.NavigationService.Navigate(new Uri("/gAlbumImagesPage.xaml?SelectedImageIndex=" + global.selectedImageIndex, UriKind.Relative));
        }
    
    }//apppage
}//namespace