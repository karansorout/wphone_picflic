using System;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.IO;
using Microsoft.Phone.Tasks;
using System.Windows.Media;
using System.Collections;

namespace PicFlic
{
    public partial class gAlbumImagesPage : PhoneApplicationPage
    {
        private App global = App.Current as App;//load current state of global variables
        private BitmapImage bitmapImage;//var for holding bitmapimage
        private GestureListener gestureListener;// GestureListener from ToolKit
        double initialScale = 1d;
        string img_href = string.Empty;

        public gAlbumImagesPage()
        {
            InitializeComponent();

            gestureListener = GestureService.GetGestureListener(ContentPanel);// Initialize GestureListener

            gestureListener.DragCompleted += new EventHandler<DragCompletedGestureEventArgs>(gestureListener_DragCompleted);// Handle Dragging (to show next or previous image from Album)

        }

        // Navigate to this page
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Find selected image index from parameters
            if (global.galbumlist[global.selectedAlbumIndex].title != null)
            {
                // Load image from Google
                LoadImage();
            }
            else
            {
                //Please select an album from previous page
                MessageBox.Show(AppResources.p4_selectAlbumagain);
            }
        }

        // Navigate from this page
        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
           
        }

        // Drag is complete
        void gestureListener_DragCompleted(object sender, DragCompletedGestureEventArgs e)
        {
            if (e.HorizontalChange > 0)//L to R
            {
                global.selectedImageIndex--;//decrease idx
                if (global.selectedImageIndex < 0)//if first image reached
                {
                    global.selectedImageIndex = global.galbumImages.Count - 1;//go to last image
                }
                LoadImage();
            }
            else
            {
                global.selectedImageIndex++;
                //first if last is shown
                if (global.selectedImageIndex > (global.galbumImages.Count - 1))
                {
                    global.selectedImageIndex = 0;
                }
                LoadImage();
            }
        }

        // Load Image from Google
        private void LoadImage()
        {
            if (global.selectedImageIndex >= 0)
            {
                // Load a new image
                bitmapImage = new BitmapImage(new Uri(global.galbumImages[global.selectedImageIndex].content, UriKind.RelativeOrAbsolute));
                // Handle loading (hide Loading... animation)
                bitmapImage.DownloadProgress += new EventHandler<DownloadProgressEventArgs>(bitmapImage_Results);
                // Loaded Image is image source in XAML
                bitmapimage.Source = bitmapImage;
            }
        }

        // Image is loaded from Google
        void bitmapImage_Results(object sender, DownloadProgressEventArgs e)
        {
            //MessageBox.Show("inside bitmapImage_DownloadProgress loop");

            // Disable LoadingListener for this image
            bitmapImage.DownloadProgress -= new EventHandler<DownloadProgressEventArgs>(bitmapImage_Results);
            // Show image details in UI
            ImageInfoTextBlock.Text = String.Format("Album {0} : Image {1} of {2}.", global.galbumlist[global.selectedAlbumIndex].title, (global.selectedImageIndex + 1), global.galbumImages.Count);

            //img_href = global.galbumImages[global.selectedImageIndex].href;
        }

        private void OnPinchStarted(object s, PinchStartedGestureEventArgs e)
        {

            initialScale = ((CompositeTransform)bitmapimage.RenderTransform).ScaleX;
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {

            base.OnBackKeyPress(e);
        }

        private void OnPinchDelta(object s, PinchGestureEventArgs e)
        {

            var finger1 = e.GetPosition(bitmapimage, 0);
            var finger2 = e.GetPosition(bitmapimage, 1);
            var center = new Point(
            (finger2.X + finger1.X) / 2 / bitmapimage.ActualWidth,

            (finger2.Y + finger1.Y) / 2 / bitmapimage.ActualHeight);

            bitmapimage.RenderTransformOrigin = center;

            var transform = (CompositeTransform)bitmapimage.RenderTransform;
            transform.ScaleX = initialScale * e.DistanceRatio;

            transform.ScaleY = transform.ScaleX;
        }

        private void deleteimage_Click(object sender, EventArgs e)
         {
             //DELETE PIC : Are you sure?
             MessageBox.Show(AppResources.p4_deletePic);

            string url = global.galbumImages[global.selectedImageIndex].href.Replace("feed", "media"); //google oath2.0 playground
            Uri uri = new Uri(url, UriKind.Absolute);// concat. absolute uri

            WebClient wc = new WebClient();
            string AuthToken = global.gtoken;
            wc.Headers[HttpRequestHeader.Authorization] = "GoogleLogin auth=" + AuthToken;
            wc.Headers[HttpRequestHeader.IfMatch] = "*";
            wc.Headers[HttpRequestHeader.ContentLength] = "0";

            wc.UploadStringCompleted += new UploadStringCompletedEventHandler(wc_UploadStringCompleted);

            wc.UploadStringAsync(uri, "DELETE", "");
          }

        void wc_UploadStringCompleted(object sender, UploadStringCompletedEventArgs e)
         {
            try
            {
                if (e.Error != null)
                    MessageBox.Show(e.Error.Message);
                else
                    GetImages();//refresh thumbnail list
                    //Image DELETED successfully
                    MessageBox.Show(AppResources.p4_imgDeletedSuccess);
                    
                    global.selectedImageIndex++;
                    //first if last is shown
                    if (global.selectedImageIndex > (global.galbumImages.Count - 1)) global.selectedImageIndex = 0;
      
                    LoadImage();
            }
        
        catch (Exception exc)
        {
            MessageBox.Show(exc.ToString());
        }
       }

        // Start loading images from Google
        private void GetImages()
        {
            WebClient webClient = new WebClient();
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
                    //Cannot load images from Picasa server!
                    MessageBox.Show(AppResources.p3_webexception);
                    return;
                }
                else
                {
                    // Deserialize JSON
                    IDictionary<string, object> json = (IDictionary<string, object>)SimpleJson.DeserializeObject(e.Result);
                    // get feed
                    IDictionary<string, object> feed = (IDictionary<string, object>)json["feed"];
                    // get Number of photos
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

                        // Link List
                        IList link = (IList)entry["link"];
                        // First link is album data link object
                        IDictionary<string, object> href = (IDictionary<string, object>)link[0];
                        // Get album href
                        albumImage.href = (string)href["href"];


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
                    //AlbumImagesListBox.ItemsSource = global.galbumImages;
                }
            }
            catch (WebException)
            {
                MessageBox.Show(AppResources.p3_webexception);//Cannot load images from Picasa server!
            }
            catch (KeyNotFoundException)
            {
                MessageBox.Show(AppResources.p3_KeyNotFoundException);//No images in the Album
            }
          }

        //Logout initiated
        private void Logout_Click(object sender, EventArgs e)
        {
            global.isLogoutFlag = 1;
            this.NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
        }
                    
    }//apppage
}//namespace