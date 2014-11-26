﻿using System.Collections.ObjectModel;
using OpenPlaylistServer.Models;
using OpenPlaylistServer.Services.Interfaces;

namespace OpenPlaylistServer.Services.Implementation
{
    public class UserService : IUserService
    {
        readonly ObservableCollectionEx<User> _users;
        readonly ReadOnlyObservableCollection<User> _roUsers;

        public UserService()
        {
            _users = new ObservableCollectionEx<User>();
            _roUsers = new ReadOnlyObservableCollection<User>(_users);
        }

        public ReadOnlyObservableCollection<User> Users
        {
            get {
                return _roUsers;
            }
        }

        public void Add(User user)
        {
            _users.Add(user);
        }
    }
}