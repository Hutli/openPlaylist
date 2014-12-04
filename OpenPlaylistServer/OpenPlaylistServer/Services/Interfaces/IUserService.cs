﻿using System.Collections.ObjectModel;
using OpenPlaylistServer.Collections;
using OpenPlaylistServer.Models;
using WebAPI;

namespace OpenPlaylistServer.Services.Interfaces
{
    public interface IUserService
    {
        ObservableCollection<User> Users { get; }

        void Add(User user);
    }
}
