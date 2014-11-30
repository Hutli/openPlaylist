﻿using Newtonsoft.Json;
using OpenPlaylistApp.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using WebAPI;
using Xamarin.Forms;

namespace OpenPlaylistApp.ViewModels
{
    public class SearchViewModel : BaseViewModel
    {

        private ObservableCollection<Track> _results = new ObservableCollection<Track>();

        public ObservableCollection<Track> Results
        {
            get { return _results; }
            set { _results = value; OnPropertyChanged("Results"); }
        }

        private Track _selectedItem;
        /// <summary>
        /// Gets or sets the selected feed item
        /// </summary>
        public Track SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                OnPropertyChanged("SelectedItem");
            }
        }

        public SearchViewModel()
        {
            
        }


        async public void GetResults(string searchStr){
            if (IsBusy)
                return;

            IsBusy = true;

            try{
                Session session = Session.Instance();
                ObservableCollection<Track> returnValue = new ObservableCollection<Track>();
                Results.Clear();
                var json = await session.Search(App.User.Venue, searchStr);
                returnValue = (ObservableCollection<Track>)JsonConvert.DeserializeObject(json, typeof(ObservableCollection<Track>));
            
                if (returnValue != null)
                {
                    foreach (Track t in returnValue)
                    {
                        Results.Add(t);
                    }
                }
                
            }
            catch (Exception ex)
            {
                App.GetMainPage().DisplayAlert("Error", ex.Message, "OK", "Cancel");
            }
            IsBusy = false;
        }
    }
}

