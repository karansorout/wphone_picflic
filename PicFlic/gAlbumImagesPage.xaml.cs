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

namespace PicFlic
{
    public partial class gAlbumImagesPage : PhoneApplicationPage
    {
        //load current state of global variables
        private App global = App.Current as App;
        string img_href = string.Empty;
        //var for holding bitmapimage
        private BitmapImage bitmapImage;
        // GestureListener from ToolKit
        private GestureListener gestureListener;
        //public override long ContentLength { get; set; }

        public gAlbumImagesPage()
        {
            InitializeComponent();
            
            // Initialize GestureListener
            gestureListener = GestureService.GetGestureListener(ContentPanel);
            // Handle Dragging (to show next or previous image from Album)
            gestureListener.DragCompleted += new EventHandler<DragCompletedGestureEventArgs>(gestureListener_DragCompleted);
        }

        // Navigate to this page
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Find selected image index from parameters
            //IDictionary<string, string> parameters = this.NavigationContext.QueryString;
            if (global.galbumlist[global.selectedAlbumIndex].title != null)
            {
                //int selected_imageIndex = Convert.ToInt32(global.selectedImageIndex);
                img_href = global.galbumlist[global.selectedAlbumIndex].href;
                // Load image from Google
                LoadImage();
            }
            else
            {
                MessageBox.Show("Please select an album from previous page");
            }
        }

        // Navigate from this page
        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            // Disable GestureListener
            bitmapImage.DownloadProgress -= new EventHandler<DownloadProgressEventArgs>(bitmapImage_Results);
        }

        // Gesture - Drag is complete
        void gestureListener_DragCompleted(object sender, DragCompletedGestureEventArgs e)
        {
            // Left or Right
            if (e.HorizontalChange > 0)
            {
                //MessageBox.Show("inside L to R, decreasing idx");
                // previous image (or last if first is shown)
                global.selectedImageIndex--;
                if (global.selectedImageIndex < 0) global.selectedImageIndex = global.galbumImages.Count - 1;
            }
            else
            {
                //MessageBox.Show("inside R to L, increasing idx");
                global.selectedImageIndex++;
                //first if last is shown
                if (global.selectedImageIndex > (global.galbumImages.Count - 1)) global.selectedImageIndex = 0;
            }
            // Load image from Google
            //MessageBox.Show("about to call load image fn");
            //int selected_imageIndex = Convert.ToInt32(global.selectedImageIndex);
            LoadImage();
        }

        // Load Image from Google
        private void LoadImage()
        {
            //MessageBox.Show("inside loadimage loop & selected image index2" + selected_imageIndex);
            if (global.selectedImageIndex < 0) global.selectedImageIndex = 0;
            // Load a new image
            bitmapImage = new BitmapImage(new Uri(global.galbumImages[global.selectedImageIndex].content, UriKind.RelativeOrAbsolute));
            // Handle loading (hide Loading... animation)
            bitmapImage.DownloadProgress += new EventHandler<DownloadProgressEventArgs>(bitmapImage_Results);
            // Loaded Image is image source in XAML
            image.Source = bitmapImage;
        }

        // Image is loaded from Google
        void bitmapImage_Results(object sender, DownloadProgressEventArgs e)
        {
            //MessageBox.Show("inside bitmapImage_DownloadProgress loop");

            // Disable LoadingListener for this image
            bitmapImage.DownloadProgress -= new EventHandler<DownloadProgressEventArgs>(bitmapImage_Results);
            // Show image details in UI
            ImageInfoTextBlock.Text = String.Format("Album {0} : Image {1} of {2}.", global.galbumlist[global.selectedAlbumIndex].title, (global.selectedImageIndex + 1), global.galbumImages.Count);

        }

        
        
        
        
        //handling upload pic
        private void p4_appbar_uploadpic(object sender, EventArgs e)
        {
            //MessageBox.Show("p4_appbar_uploadpic works!");
            
            PhotoChooserTask task = new PhotoChooserTask();
            task.Completed += task_Completed;
            task.Show();
        }
 
        private void task_Completed(object sender, PhotoResult e)
        {

            if (e.TaskResult == TaskResult.OK)
            {

                MessageBox.Show(e.ChosenPhoto.Length.ToString());
                UploadPhotoNewMethod(e.ChosenPhoto);

            }

        }


        //upload image
        private void UploadPhotoNewMethod(Stream stream)
        {
            const int BLOCK_SIZE = 4096;
            img_href.Replace("entry", "feed");
            //Uri uri = new Uri("http://picasaweb.google.com/data/feed/api/user/default/albumid/default", UriKind.Absolute);
            Uri uri = new Uri(img_href, UriKind.Absolute);

            WebClient wc = new WebClient();
            string AuthToken = global.gtoken;
            wc.Headers[HttpRequestHeader.Authorization] = "GoogleLogin auth=" + AuthToken;
            wc.Headers[HttpRequestHeader.ContentLength] = stream.Length.ToString();
            wc.Headers[HttpRequestHeader.ContentType] = "image/jpeg";
            wc.AllowReadStreamBuffering = true;
            wc.AllowWriteStreamBuffering =true;
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
                    
    }//apppage
}//namespace