﻿using System;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using WebAPI;
using Xamarin.Forms;

namespace OpenPlaylistApp.Models
{
    public class Session
    {
        public static Session session;

        private Session()
        {
        }

        public static Session Instance()
        {
            return session ?? (session = new Session());
        }

		public static Uri MakeUri(string endpoint){
		    if (App.User == null || App.User.Venue == null)
		    {
		        return null;
		    }
			UriBuilder uriBuilder = new UriBuilder("http", App.User.Venue.IP, 5555, endpoint);
			return uriBuilder.Uri;
		}

		public static async Task<String> MakeRequest(Uri request, string errorMessageTitle, string errorMessage, TimeSpan timeout, bool loadIndicator)
        {
			if(loadIndicator) App.Home.IsBusy = true;
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = timeout;
                    //Else Windows Phone will cache and not make new request to the server
                    client.DefaultRequestHeaders.IfModifiedSince = DateTimeOffset.Now; 
                    
                    using (HttpResponseMessage response = await client.GetAsync(request))
                    {
                        App.Home.IsBusy = false;
                        if (response.IsSuccessStatusCode)
                        {
                            using (HttpContent content = response.Content)
                            {
                                var str = await content.ReadAsStringAsync();
								if(loadIndicator) App.Home.IsBusy = false;
                                return str;
                            }
                        }
                        else
                        {
							if(loadIndicator) App.Home.IsBusy = false;
                            return null;
                        }
                    }
                }
            }
            catch (Exception e)
            {
				if(loadIndicator) App.Home.IsBusy = false;
                App.GetMainPage().DisplayAlert(errorMessageTitle, errorMessage, "Ok", "Cancel");
                return null;
            }
        }

        public async Task<string> CheckIn(Venue venue, User user)
        {
            UriBuilder uriBuilder = new UriBuilder("http", venue.IP, 5555, "checkin/" + user.Id);
            var str = await MakeRequest(uriBuilder.Uri, "Venue not online", "The selected venue is not online. Try another one.", new TimeSpan(0,0,3), true);
            return str;
        }

        public async Task<string> GetVenues()
        {
            UriBuilder uriBuilder = new UriBuilder("http", "op.zz.vc");
            //App.Home.IsBusy = true;
            //var str = await MakeRequest(uriBuilder.Uri, "Venue list error",
            //    "Could not get list of venues. Contact your network administrator.", new TimeSpan(0,0,10));
            //App.Home.IsBusy = true;
            //return str;
            using (HttpClient client = new HttpClient())
            using (HttpResponseMessage response = await client.GetAsync("http://op.zz.vc/"))
            using (HttpContent content = response.Content)
            {
                var str = await content.ReadAsStringAsync();
                return str;
            }
        }

        public async Task<string> GetPlaylist(Venue venue)
        {
            UriBuilder uriBuilder = new UriBuilder("http", venue.IP, 5555, "playlist");

            return await MakeRequest(uriBuilder.Uri, "Playlist error", "Could not get playlist", new TimeSpan(0,0,10), false);

            //using (HttpClient client = new HttpClient())
            //using (HttpResponseMessage response = await client.GetAsync(uriBuilder.Uri))
            //using (HttpContent content = response.Content)
            //{
            //    var str = await content.ReadAsStringAsync();
            //    App.Home.IsBusy = false;
            //    return str;
            //}
        }

        public async Task<string> GetNowPlaying(Venue venue)
        {
            UriBuilder uriBuilder = new UriBuilder("http", venue.IP, 5555, "nowplaying");

            var str =
                await MakeRequest(uriBuilder.Uri, "Nowplaying error", "Could not get nowplaying", new TimeSpan(0, 0, 10), false);

            return str;

            //using (HttpClient client = new HttpClient())
            //{
            //    client.DefaultRequestHeaders.IfModifiedSince = DateTimeOffset.Now; //Else Windows Phone will cache and not make new request to the server
            //    using (HttpResponseMessage response = await client.GetAsync(uriBuilder.Uri))
            //    using (HttpContent content = response.Content)
            //    {
            //        return await content.ReadAsStringAsync();
            //    }
            //}
        }

        public async void ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (!(e.SelectedItem is Track)) return;
            Track track = (Track)e.SelectedItem;
            Session session = Session.Instance();

            try
            {
                session.SendVote(App.User.Venue, track, App.User); //TODO vi bruger ikke variablen
                App.User.Vote = track;
            }
            catch (Exception ex)
            {
                App.GetMainPage().DisplayAlert("Error", ex.Message, "OK", "Cancel");
            }
        }

        public async void SendVote(Venue venue, Track track, User user)
        {
            UriBuilder uriBuilder = new UriBuilder("http", venue.IP, 5555, "vote/" + track.URI + "/" + user.Id);
            await MakeRequest(uriBuilder.Uri, "Vote failed", "Could not vote on selected track", new TimeSpan(0,0,3),true);

            //using (HttpClient client = new HttpClient())
            //{
            //    {
            //        client.DefaultRequestHeaders.IfModifiedSince = DateTimeOffset.Now;
            //        //Else Windows Phone will cache and not make new request to the server

            //        using (HttpResponseMessage response = await client.GetAsync(uriBuilder.Uri))
            //        using (HttpContent content = response.Content)
            //        {
            //            /*
            //            var str = await content.ReadAsStringAsync();
                        
            //            if (str != "Success")
            //            {
            //                throw new Exception("Vote failede");
            //            }*/
            //            App.Home.IsBusy = false;
            //        }
            //    }
            //}
        }

        public async Task<string> Search(Venue venue, string searchStr)
        {
            UriBuilder uriBuilder = new UriBuilder("http", venue.IP, 5555, "search/" + searchStr);
            App.Home.IsBusy = true;
            var str = await MakeRequest(uriBuilder.Uri, "Search error", "Could not search", new TimeSpan(0, 0, 40),true);
            App.Home.IsBusy = false;
            return str;

            //using (HttpClient client = new HttpClient())
            //using (HttpResponseMessage response = await client.GetAsync(uriBuilder.Uri))
            //using (HttpContent content = response.Content)
            //{
            //    var str = await content.ReadAsStringAsync();
            //    App.Home.browsePage.IsBusy = false;
            //    return str;
            //}
        }
    }
}
