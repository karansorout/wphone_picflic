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

namespace PicFlic
{
    public partial class gAlbumImagesPage : PhoneApplicationPage
    {
        private App global = App.Current as App;//load current state of global variables
        private BitmapImage bitmapImage;//var for holding bitmapimage
        private GestureListener gestureListener;// GestureListener from ToolKit
        double initialScale = 1d;

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
            bitmapimage.Source = bitmapImage;
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

        private void OnPinchStarted(object s, PinchStartedGestureEventArgs e)
        {

            initialScale = ((CompositeTransform)bitmapimage.RenderTransform).ScaleX;
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
        
                    
    }//apppage
}//namespace