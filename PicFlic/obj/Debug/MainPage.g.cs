﻿#pragma checksum "G:\Wphone\Project\_project_dev\PicFlic\PicFlic\MainPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "0319AE491C825BBF953F450310AE2D62"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18051
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace PicFlic {
    
    
    public partial class MainPage : Microsoft.Phone.Controls.PhoneApplicationPage {
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal System.Windows.Controls.StackPanel TitlePanel;
        
        internal System.Windows.Controls.TextBlock PageTitle;
        
        internal System.Windows.Controls.Grid ContentPanel;
        
        internal System.Windows.Controls.TextBlock p1_textBlock_connect;
        
        internal System.Windows.Controls.RadioButton p1_radioButton_picasa;
        
        internal System.Windows.Controls.RadioButton p1_radioButton_flickr;
        
        internal System.Windows.Controls.TextBlock p1_textBlock_uname;
        
        internal System.Windows.Controls.TextBox p1_login_email;
        
        internal System.Windows.Controls.TextBlock p1_textBlock_pwd;
        
        internal System.Windows.Controls.PasswordBox p1_passwordBox;
        
        internal System.Windows.Controls.Button p1_button_login;
        
        internal System.Windows.Controls.TextBlock textBlock1;
        
        internal Microsoft.Phone.Shell.ApplicationBarMenuItem AboutPicFlic;
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Windows.Application.LoadComponent(this, new System.Uri("/PicFlic;component/MainPage.xaml", System.UriKind.Relative));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.TitlePanel = ((System.Windows.Controls.StackPanel)(this.FindName("TitlePanel")));
            this.PageTitle = ((System.Windows.Controls.TextBlock)(this.FindName("PageTitle")));
            this.ContentPanel = ((System.Windows.Controls.Grid)(this.FindName("ContentPanel")));
            this.p1_textBlock_connect = ((System.Windows.Controls.TextBlock)(this.FindName("p1_textBlock_connect")));
            this.p1_radioButton_picasa = ((System.Windows.Controls.RadioButton)(this.FindName("p1_radioButton_picasa")));
            this.p1_radioButton_flickr = ((System.Windows.Controls.RadioButton)(this.FindName("p1_radioButton_flickr")));
            this.p1_textBlock_uname = ((System.Windows.Controls.TextBlock)(this.FindName("p1_textBlock_uname")));
            this.p1_login_email = ((System.Windows.Controls.TextBox)(this.FindName("p1_login_email")));
            this.p1_textBlock_pwd = ((System.Windows.Controls.TextBlock)(this.FindName("p1_textBlock_pwd")));
            this.p1_passwordBox = ((System.Windows.Controls.PasswordBox)(this.FindName("p1_passwordBox")));
            this.p1_button_login = ((System.Windows.Controls.Button)(this.FindName("p1_button_login")));
            this.textBlock1 = ((System.Windows.Controls.TextBlock)(this.FindName("textBlock1")));
            this.AboutPicFlic = ((Microsoft.Phone.Shell.ApplicationBarMenuItem)(this.FindName("AboutPicFlic")));
        }
    }
}

