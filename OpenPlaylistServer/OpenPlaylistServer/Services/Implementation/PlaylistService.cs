﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json.Linq;
using OpenPlaylistServer.Services.Interfaces;
using WebAPI;

namespace OpenPlaylistServer.Services.Implementation {
    public class PlaylistService : IPlaylistService {
        private readonly IHistoryService _historyService;
        private readonly IUserService _userService;

        public PlaylistService(IUserService userService, IHistoryService historyService) {
            Tracks = new ObservableCollection<Track>();
            _userService = userService;
            _historyService = historyService;
        }

        public Track FindTrack(string trackUri) {
            return Tracks.FirstOrDefault(x => x.Uri == trackUri);
        }

        public ObservableCollection<Track> Tracks {
            get;
            private set;
        }

        public int CalcTScore(Track track) {
            return _userService.Users.Where(u => u.Vote != null).Count(u => Equals(u.Vote.Track, track));
        }

        public Track NextTrack() {
            CountAndUpdatePVotes();
            var next = Tracks.OrderByDescending(x => x.TotalScore).FirstOrDefault();
            if(next == null || (_historyService.GetLastTrack() != null && _historyService.GetLastTrack().Equals(next))) {
                // if last track is equal to next track, find another relevant track instead
                next = SmartFindTrack();
                Console.WriteLine("smart find called. Found " + next);
                if(next == null) {
                    // smartfind could not find next track, so we play a default (debug track)
                    return WebAPIMethods.GetTrack("spotify:track:4vqEoOF7BBERkmvbrhgBN8");
                }
            }

            next.PScore = 0;
            ResetVotes(next);
            Remove(next);

            return next;
        }

        public void Remove(Track track) {
            Tracks.Remove(track);
        }

        public void Add(Track track) {
            Tracks.Add(track);
        }

        private void ResetVotes(Track track) {
            var users = _userService.Users.Where(u => u.Vote != null && Equals(u.Vote.Track, track));
            foreach(var user in users)
                user.Vote = null;

            var tScore = CalcTScore(track);
            track.TScore = tScore;
        }

        private void CountAndUpdatePVotes() {
            foreach(var track in Tracks) {
                track.TScore = CalcTScore(track);
                track.PScore += track.TScore; // add temp score to permanent score
            }
        }

        private Track SmartFindTrack() {
            var lastTracks = _historyService.GetLastNTracks(9);
            var relArtists = new List<string>();
            foreach(var artist in lastTracks.SelectMany(track => track.Album.Artists)) {
                var str =
                    Request.Get(String.Format("https://api.spotify.com/v1/artists/{0}/related-artists", artist.Id))
                           .Result;
                var jobject = JObject.Parse(str);
                foreach(var jartist in jobject["artists"])
                    relArtists.Add(jartist["id"].ToString());
            }
            var groupedArtists = relArtists.GroupBy(x => x);
            IGrouping<string, string> mostAppearing = null;
            foreach(var group in groupedArtists) // find id which appears most
            {
                if(mostAppearing == null || mostAppearing.Count() < group.Count())
                    mostAppearing = group;
            }
            if(mostAppearing == null)
                return null;
            var topTracks =
                Request.Get(String.Format("https://api.spotify.com/v1/artists/{0}/top-tracks?country=DK",
                                          mostAppearing.Key)).Result;
            var jTopTracks = JObject.Parse(topTracks);

            var uris = jTopTracks["tracks"].Select(trackToken => (string)trackToken["uri"]);
            var uri = uris.FirstOrDefault(t => !lastTracks.Select(x => x.Uri).Contains(t));
            return WebAPIMethods.GetTrack(uri);
        }
    }
}