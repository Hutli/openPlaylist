﻿using System.Collections.ObjectModel;
using System.Linq;
using OpenPlaylistServer.Models;
using OpenPlaylistServer.Services.Interfaces;

namespace OpenPlaylistServer.Services.Implementation
{
    public class PlaylistService : IPlaylistService
    {
        readonly ObservableCollectionEx<PlaylistTrack> _tracks;
        readonly ReadOnlyObservableCollection<PlaylistTrack> _roTracks;

        private IUserService _userService;

        public PlaylistService(IUserService userService){
            _tracks = new ObservableCollectionEx<PlaylistTrack>();
            _roTracks = new ReadOnlyObservableCollection<PlaylistTrack>(_tracks);
            _userService = userService;
        }

        public PlaylistTrack FindTrack(string trackUri)
        {
            return _tracks.FirstOrDefault(x => x.Uri == trackUri);
        }

        public void AddByURI(string trackUri)
        {
            //var track = SpotifyLoggedIn.Instance.TrackFromLink(trackUri).Result;
            PlaylistTrack playlistTrack = new PlaylistTrack(trackUri);
            _tracks.Add(playlistTrack);
        }

        //public void AddByRef(playlistTrack playlistTrack)
        //{
        //    _tracks.Add(playlistTrack);
        //}

        public void RemoveByTitle(string name)
        {
            if (_tracks.Any(e => e.Name.Equals(name)))
                _tracks.Remove(_tracks.First(e => e.Name.Equals(name)));
        }

        public void Remove(PlaylistTrack track)
        {
            _tracks.Remove(track);
        }


        public ReadOnlyObservableCollection<PlaylistTrack> Tracks
        {
            get {
                return _roTracks;
            }
        }


        public PlaylistTrack NextTrack()
        {
            CountAndUpdatePVotes();
            PlaylistTrack next = _tracks.OrderByDescending(x => x.TotalScore).FirstOrDefault();

            if (next == null) return null;
            next.ResetPScore();
            _tracks.Remove(next);

            return next;
        }

        private void CountAndUpdatePVotes()
        {
            foreach (var track in _tracks)
            {
                // add temp score to permanent score
                track.UpdatePScore(track.TScore);
                // reset temp score
                track.TScore = 0;
            }
        }


        public void Add(PlaylistTrack track)
        {
            _tracks.Add(track);
        }
    }
}