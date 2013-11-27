using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.IO.IsolatedStorage;

namespace PicFlic
{
    public partial class App : Application
    {
        //global variables
        //p1 vars
        public string gtoken { get; set; }//app google auth token
        public string username { get; set; }
        public string password { get; set; }
        //p2 vars
        public AlbumsList galbumlist = new AlbumsList();
        public int selectedAlbumIndex;
        public int isLoginFlag;
        //public int createNewAlbumFlag;
        //public string selectedAlbumName;
        //p3 vars
        public AlbumImages galbumImages = new AlbumImages();
        public int selectedImageIndex;
        Uri nUri;
        private IsolatedStorageSettings applicationStorage;
        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public PhoneApplicationFrame RootFrame { get; private set; }

        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App()
        {
            // Global handler for uncaught exceptions. 
            UnhandledException += Application_UnhandledException;
            //MessageBox.Show("Something went wrong, please restart the application");

            // Standard Silverlight initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();

            // Show graphics profiling information while debugging.
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Display the current frame rate counters.
                Application.Current.Host.Settings.EnableFrameRateCounter = true;

                // Show the areas of the app that are being redrawn in each frame.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode, 
                // which shows areas of a page that are handed off to GPU with a colored overlay.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;

                // Disable the application idle detection by setting the UserIdleDetectionMode property of the
                // application's PhoneApplicationService object to Disabled.
                // Caution:- Use this under debug mode only. Application that disables user idle detection will continue to run
                // and consume battery power when the user is not using the phone.
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            }

        }

        

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
            
            applicationStorage = IsolatedStorageSettings.ApplicationSettings;

            if (applicationStorage.Contains("isLoginFlag"))
            {
                isLoginFlag = (int)applicationStorage["isLoginFlag"];
            }

            if (isLoginFlag == 1)//if logout button was not pressed
            {
                //Albums Page
                nUri = new Uri("/gAlbumsListPage.xaml", UriKind.Relative);
                ((App)Application.Current).RootFrame.Navigate(nUri);
                
            }
            else
            {
                //Login Page
                nUri = new Uri("/MainPage.xaml", UriKind.Relative);
                ((App)Application.Current).RootFrame.Navigate(nUri);
            }
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            if (e.IsApplicationInstancePreserved)
            {
                //do nothing, app is already activated
            }
            else
            {

                var tombstore = new TransientDataStorage();
                
                isLoginFlag = tombstore.Restore<int>("t_isLoginFlag");
                username = tombstore.Restore<string>("t_username");
                password = tombstore.Restore<string>("t_password");
                gtoken = tombstore.Restore<string>("t_gtoken");
                galbumlist = tombstore.Restore<AlbumsList>("t_galbumlist");
                selectedAlbumIndex = tombstore.Restore<int>("t_selectedAlbumIndex");
                selectedImageIndex = tombstore.Restore<int>("t_selectedImageIndex");
                galbumImages = tombstore.Restore<AlbumImages>("t_galbumImages");
            }
         }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            var tombstore = new TransientDataStorage();

            tombstore.Backup("t_isLoginFlag", isLoginFlag);
            tombstore.Backup("t_username", username);
            tombstore.Backup("t_password", password);
            tombstore.Backup("t_gtoken", gtoken);
            tombstore.Backup("t_galbumlist", galbumlist);
            tombstore.Backup("t_selectedAlbumIndex", selectedAlbumIndex);
            tombstore.Backup("t_galbumImages", galbumImages);
            tombstore.Backup("t_selectedImageIndex", selectedImageIndex);
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {

        }

        // Code to execute if a navigation fails
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        #region Phone application initialization

        // Avoid double-initialization
        private bool phoneApplicationInitialized = false;

        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            RootFrame = new PhoneApplicationFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        #endregion
    }
}