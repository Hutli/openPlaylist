using System.Collections;
using Xamarin.Forms;

namespace OpenPlaylistApp
{
    //This is a template for how to diplay a track
    class TrackTemplate : DataTemplate
    {
        public TrackTemplate():base(typeof(CustomCell)){
            //this.SetBinding(CustomCell.SelectedProperty, "IsSelected");
            this.SetBinding(CustomCell.GrayoutProperty, "IsFiltered");
            this.SetBinding(CustomCell.ImageSourceProperty, "Album.Images.FirstOrDefaul().URL");
            this.SetBinding(CustomCell.TextProperty, "Name");
            this.SetBinding(CustomCell.DetailProperty, "Album.ToStringProp");
            this.SetBinding(CustomCell.VoteProperty, "TotalScore");
        }
    }
}
