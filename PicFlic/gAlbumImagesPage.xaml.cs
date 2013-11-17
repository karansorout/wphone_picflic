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

namespace PicFlic
{
    public partial class gAlbumImagesPage : PhoneApplicationPage
    {
        //load current state
        private App global = App.Current as App;
        

        // BitmapImage for loading Images from Google
        private BitmapImage bitmapImage;

        // GestureListener from ToolKit
        private GestureListener gestureListener;

        // Handle loading... animation in this page 
        public bool ShowProgress
        {
            get { return (bool)GetValue(ShowProgressProperty); }
            set { SetValue(ShowProgressProperty, value); }
        }

        public static readonly DependencyProperty ShowProgressProperty =
            DependencyProperty.Register("ShowProgress", typeof(bool), typeof(gAlbumImagesPage), new PropertyMetadata(false));
 
        public gAlbumImagesPage()
        {
            InitializeComponent();
              //MessageBox.Show("album name=" + global.galbumlist[global.selectedAlbumIndex].title + "///selected image index" + selected_imageIndex);
              //LoadImage(selected_imageIndex);
            
            // Initialize GestureListener
            gestureListener = GestureService.GetGestureListener(ContentPanel);
            // Handle Dragging (to show next or previous image from Album)
            gestureListener.DragCompleted += new EventHandler<DragCompletedGestureEventArgs>(gestureListener_DragCompleted);
        }

        // Gesture - Drag is complete
        void gestureListener_DragCompleted(object sender, DragCompletedGestureEventArgs e)
        {
            // Left or Right
            if (e.HorizontalChange > 0 || e.VerticalChange > 0)
            {
                // previous image (or last if first is shown)
                global.selectedImageIndex--;
                if (global.selectedImageIndex < 0) global.selectedImageIndex = global.galbumImages.Count - 1;
            }
            else
            {
                // next image (or first if last is shown)
                global.selectedImageIndex++;
                if (global.selectedImageIndex > (global.galbumImages.Count - 1)) global.selectedImageIndex = 0;
            }
            // Load image from Google
            int selected_imageIndex = Convert.ToInt32(global.selectedImageIndex);
            LoadImage(selected_imageIndex);
        }

        // Navigate to this page
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Find selected image index from parameters
            //IDictionary<string, string> parameters = this.NavigationContext.QueryString;
            if (global.galbumlist[global.selectedAlbumIndex].title != null)
            {
                int selected_imageIndex = Convert.ToInt32(global.selectedImageIndex);
                // Load image from Google
                LoadImage(selected_imageIndex);
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

        // Load Image from Google
        private void LoadImage(int selected_imageIndex)
        {
            //MessageBox.Show("inside loadimage loop & selected image index2" + selected_imageIndex);
            // Load a new image
            bitmapImage = new BitmapImage(new Uri(global.galbumImages[selected_imageIndex].content, UriKind.RelativeOrAbsolute));
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
            ImageInfoTextBlock.Text = String.Format("Album {0} : Image {1} of {2}.",
                global.selectedAlbumName,
                (global.selectedImageIndex + 1),
                global.galbumImages.Count);
            
        }

    }//apppage
}//namespace