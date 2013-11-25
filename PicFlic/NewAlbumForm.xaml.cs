using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace PicFlic
{
    public partial class NewAlbumForm : PhoneApplicationPage
    {
        App global = App.Current as App;//load present state of app
        string NewAlbumAccessType = string.Empty;
        int createNewAlbumFlag = 1;

        public NewAlbumForm()
        {
            InitializeComponent();
        }

       private void Submit_NewAlbumForm(object sender, RoutedEventArgs e)
        {
            if (NewAlbumAccessType_private.IsChecked == true) NewAlbumAccessType = "private";
            else NewAlbumAccessType = "public";
           
           string entry = string.Format("<entry xmlns='http://www.w3.org/2005/Atom' xmlns:media='http://search.yahoo.com/mrss/' xmlns:gphoto='http://schemas.google.com/photos/2007'><title type='text'>{0}</title><summary type='text'>{1}</summary><gphoto:location>Helsinki</gphoto:location><gphoto:access>{2}</gphoto:access><gphoto:timestamp></gphoto:timestamp><media:group><media:keywords>New Album</media:keywords></media:group><category scheme='http://schemas.google.com/g/2005#kind' term='http://schemas.google.com/photos/2007#album'></category></entry>", NewAlbumName.Text, NewAlbumDesc.Text, NewAlbumAccessType);
           
            WebClient wc = new WebClient();
            Uri uri = new Uri("https://picasaweb.google.com/data/feed/api/user/"+global.username, UriKind.Absolute);
            wc.Headers[HttpRequestHeader.ContentType] = "application/atom+xml";
            wc.Headers[HttpRequestHeader.ContentLength] = entry.Length.ToString();
            wc.Headers[HttpRequestHeader.Authorization] = "GoogleLogin auth=" + global.gtoken;
            wc.UploadStringCompleted += new UploadStringCompletedEventHandler(wc_UploadStringCompleted);

            wc.UploadStringAsync(uri, "POST", entry);
        }
       
     void wc_UploadStringCompleted(object sender, UploadStringCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null)
                    MessageBox.Show(e.Error.Message);
                else
                    //MessageBox.Show(e.Result);
                //fetch_galbumslist();
                MessageBox.Show("Album \"" + NewAlbumName.Text + "\" created Successfully!");
                this.NavigationService.Navigate(new Uri("/gAlbumsListPage.xaml", UriKind.Relative));
            }

            catch (Exception exc)
            {
                MessageBox.Show(exc.ToString());
            }
        }

     //protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
     //{
     //    base.OnNavigatedTo(e);
     //    this.NavigationService.Navigate(new Uri("/gAlbumsListPage.xaml", UriKind.Relative));
     //    return;
     //}

     //protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
     //{
     //    base.OnNavigatedTo(e);
     //    return;
     //}
        
    }//apppage
}//namespace