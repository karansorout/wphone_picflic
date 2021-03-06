﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using Microsoft.Phone.Shell;

namespace PicFlic
{

    public class AlbumsList : ObservableCollection<Album> {
    }

   public class Album {
        public string title { get; set; }
        public string published { get; set; }
        public string href { get; set; }
        public string thumbnail { get; set; }
    }

    public class AlbumImages : ObservableCollection<AlbumImage> {
   }

    public class AlbumImage {
        public string title { get; set; }
        public string content { get; set; }
        public string href { get; set; }
        public string width { get; set; }
        public string height { get; set; }
        public string size { get; set; }
        public string thumbnail { get; set; }
    }

    // Tombstoning classes
    public class TransientDataStorage : IDataStorage
    {

        public bool Backup(string token, object value)
        {
            if (null == value)
                return false;

            var tombstore = PhoneApplicationService.Current.State;
            if (tombstore.ContainsKey(token))
                tombstore[token] = value;
            else
                tombstore.Add(token, value);

            return true;
        }

        public T Restore<T>(string token)
        {
            var tombstore = PhoneApplicationService.Current.State;
            if (!tombstore.ContainsKey(token))
                return default(T);

            return (T)tombstore[token];
        }
    }


    public interface IDataStorage
    {
        bool Backup(string token, object value);
        T Restore<T>(string token);
    }

}
