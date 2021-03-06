using Newtonsoft.Json;

namespace WebAPI {
    [JsonObject(MemberSerialization.OptOut)]
    public class Artist : SpotifyObject {
        public Artist(string id, string name) : base(id, name) {}

        public string Href {
            get {return "https://api.spotify.com/v1/artists/" + Id;}
        }

        public override string Uri {
            get {return "spotify:artist:" + Id;}
        }

        public string ToStringProp {
            get {return ToString();}
        }

        public override string ToString() {
            return Name;
        }
    }
}