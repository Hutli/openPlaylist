﻿using System.Collections.Generic;
using Newtonsoft.Json;

namespace WebAPI
{
    [JsonObject(MemberSerialization.OptOut)]
    public class Track : SpotifyObject
    {
        public Track(string id, string name, int duration, bool isExplicit, int trackNumber, string isrc, string previewURL, Album album)
            : base(id, name)
        {
            Duration = duration;
            IsExplicit = isExplicit;
            TrackNumber = trackNumber;
            ISRC = isrc;
            PreviewURL = previewURL;
            Album = album;
        }

        public string ISRC { get; private set; }

        public string PreviewURL { get; private set; }

        public int Duration { get; private set; }

        public int currentDurationStep { get; set; }

        public bool IsExplicit { get; private set; }

        public int TrackNumber { get; private set; }

        [JsonProperty]
        public Album Album { get; private set; }

        public override string URI { get { return "spotify:track:" + ID; } }

        public override string ToString() { return string.Format("{0} on {1}", Name, Album); }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(Track))
            {
                return ((Track)obj).ID == this.ID || ((Track)obj).ISRC == this.ISRC;
            }
            else
                return false;
            
        }
    }
}
