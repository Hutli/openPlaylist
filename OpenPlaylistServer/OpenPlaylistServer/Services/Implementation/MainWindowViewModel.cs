﻿using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Net.Mime;
using System.Windows;
using System.Windows.Threading;
using System;
using OpenPlaylistServer.Collections;
using OpenPlaylistServer.Models;
using OpenPlaylistServer.Services.Interfaces;
using WebAPI;
using System.Collections.ObjectModel;

namespace OpenPlaylistServer.Services.Implementation
{
    class MainWindowViewModel : IMainWindowViewModel
    {
        private IPlaylistService _playlistService;
        private IUserService _userService;
        private IPlaybackService _playbackService;
        private IHistoryService _historyService;
        private IRestrictionService _restrictionService;

        public MainWindowViewModel(IPlaylistService playlistService, IUserService userService, IPlaybackService playbackService, IHistoryService historyService, IRestrictionService restrictionService)
        {
            _playlistService = playlistService;
            _userService = userService;
            _playbackService = playbackService;
            _historyService = historyService;
            _restrictionService = restrictionService;
        }

        public ObservableCollection<Track> Tracks
        {
            get
            {
                return _playlistService.Tracks;
            }
        }

        public ObservableCollection<Track> History
        {
            get
            {
                return _historyService.Tracks;
            }
        }

        public ObservableCollection<Restriction> Restrictions
        {
            get
            {
                return _restrictionService.Restrictions;
            }
        }

        public void AddRestriction(Restriction restriction)
        {
            _restrictionService.AddRestriction(restriction);
        }

        public ObservableCollection<User> Users
        {
            get
            {
                return _userService.Users;
            }
        }

        public void TrackEnded()
        {
            Console.WriteLine("Track ended called");
            // TrackEnded is called from libspotify running in a different thread than the UI thread.
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    var currentPlaying = _playbackService.GetCurrentPlaying();
                    if (currentPlaying != null)
                    {
                        _historyService.Add(currentPlaying);
                    }

                    Track next = _playlistService.NextTrack();
                    if (next != null)
                    {
                        _playbackService.Play(next);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }


            });
            
        }

        public void PlayButtonClicked()
        {
            TrackEnded();
        }

        public void StopButtonClicked()
        {
            _playbackService.Stop();
        }

        public void RemoveTrack_Click(Track track){
            Tracks.Remove(track);
        }

    }
}
