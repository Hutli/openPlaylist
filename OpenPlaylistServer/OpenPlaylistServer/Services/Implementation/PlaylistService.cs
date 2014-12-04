﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OpenPlaylistServer.Collections;
using OpenPlaylistServer.Models;
using OpenPlaylistServer.Services.Interfaces;
using WebAPI;
using System.Collections.ObjectModel;

namespace OpenPlaylistServer.Services.Implementation
{
    public class PlaylistService : IPlaylistService
    {
        readonly ObservableCollection<Track> _tracks;

        private readonly IUserService _userService;
        private readonly IHistoryService _historyService;

        public PlaylistService(IUserService userService, IHistoryService historyService){
            _tracks = new ObservableCollection<Track>();
            _userService = userService;
            _historyService = historyService;
        }

        public Track FindTrack(string trackUri)
        {
            return _tracks.FirstOrDefault(x => x.URI == trackUri);
        }

        public ObservableCollection<Track> Tracks
        {
            get {
                return _tracks;
            }
        }

        public int CalcTScore(Track track)
        {
            return _userService.Users.Count(u => Equals(u.Vote, track));
        }

        private void ResetVotes(Track track)
        {
            var users = _userService.Users.Where(u => Equals(u.Vote, track));
            foreach (var user in users)
            {
                user.Vote = null;
            }

            var tScore = CalcTScore(track);
            track.TScore = tScore;
        }

        public Track NextTrack()
        {
            CountAndUpdatePVotes();
            Track next = _tracks.OrderByDescending(x => x.TotalScore).FirstOrDefault();;
            if(_historyService.GetLastTrack() != null &&_historyService.GetLastTrack().Equals(next))
            { // if last track is equal to next track, find another relevant track instead
                next = SmartFindTrack().Result;
            }

            if (next == null) return null;
            next.PScore = 0;
            ResetVotes(next);

            return next;
        }

        private void CountAndUpdatePVotes()
        {
            foreach (var track in _tracks)
            {
                var tScore = CalcTScore(track);
                track.TScore = tScore;
                // add temp score to permanent score
                track.PScore += track.TScore;
            }
        }

        public void Add(Track track)
        {
            _tracks.Add(track);
        }

        private async Task<Track> SmartFindTrack()
        {
            var tracks = _historyService.GetLastNTracks(10);
            var relArtists = new List<string>();
            foreach (var track in tracks) //find every related artists id for the last 10 songs
            {
                foreach (var artist in track.Album.Artists)
                {
                    var str = await Request.Get(String.Format("https://api.spotify.com/v1/artists/{0}/related-artists", artist.Id));
                    var jobject = JObject.Parse(str);
                    foreach (var jartist in jobject["artists"])
                    {
                        relArtists.Add(jartist["id"].ToString());
                    }
                }
            }
            var groupedArtists = relArtists.GroupBy(x => x);
            IGrouping<string, string> mostAppearing = null;
            foreach (var group in groupedArtists) // find id which appears most
            {
                if (mostAppearing == null || mostAppearing.Count() < group.Count())
                {
                    mostAppearing = group;
                }
            }
            if (mostAppearing == null) return null;
            string topTracks = Request.Get(String.Format("https://api.spotify.com/v1/artists/{0}/top-tracks?country=DK", mostAppearing.Key)).Result;
            var jTopTracks = JObject.Parse(topTracks);
            List<string> uris = new List<string>();
            foreach (var jTopTrack in jTopTracks["tracks"])
            {
                uris.Add((string)jTopTrack["uri"]);
            }
            var lastTracks = _historyService.GetLastNTracks(9);
            var topTrack = new Track();
            foreach (var uri in uris)
            {
                var tmp = WebAPIMethods.GetTrack(uri).Result;
                if (!lastTracks.Contains(tmp))
                    topTrack = tmp;
            }
            
            return topTrack;

            //var jToken = jTopTracks["tracks"].First();
            //if (jToken == null) return null;
            //var uri = (string) jToken["uri"];
            //var topTrack = WebAPIMethods.GetTrack(uri).Result;
            //return topTrack;
        }
    }
}
