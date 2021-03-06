using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Nancy.Hosting.Self;
using OpenPlaylistServer.Models;
using OpenPlaylistServer.Services.Interfaces;
using OpenPlaylistServer.Views;
using SpotifyDotNet;
using Track = WebAPI.Track;

namespace OpenPlaylistServer.Services.Implementation {
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : IMainWindow {
        private static readonly byte[] _appkey = Properties.Resources.spotify_appkey;
        private readonly Spotify _session = Spotify.Instance;
        private readonly IMainWindowViewModel _viewModel;

        public MainWindow(IMainWindowViewModel viewModel) {
            DataContext = viewModel;
            InitializeComponent();

            _viewModel = viewModel;

            var hostConfig = new HostConfiguration {
                UrlReservations = {
                    CreateAutomatically = true
                }
            };

            var host = new NancyHost(hostConfig, new Uri("http://localhost:5555"));
            host.Start();

            _session.TrackEnded += viewModel.TrackEnded;
        }

        private void OnLoginSuccess() {
            Dispatcher.Invoke(() => {
                LoggedInStatus.Content = "Succesfully logged in";
                PlayButton.IsEnabled = true;
                StopButton.IsEnabled = true;
            });
        }

        private void OnLogInError(LoginState loginState) {
            Dispatcher.Invoke(() => {
                LoggedInStatus.Content = "Error: " + loginState;
            });
        }

        //WPF Content
        private void PlayButton_Click(object sender, RoutedEventArgs e) {
            _viewModel.PlayButtonClicked();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e) {
            _viewModel.StopButtonClicked();
        }

        private void RemoveTrack_Click(object sender, RoutedEventArgs e) {
            var selected = PlaylistView.SelectedItem as Track;
            if(selected == null)
                return;
            _viewModel.RemoveTrack_Click(selected);
        }

        private void MoveUp_Click(object sender, RoutedEventArgs e) {
            var selected = PlaylistView.SelectedItem as Track;
            if(selected == null)
                return;
            _viewModel.MoveUp_Click(selected);
        }

        private void MoveDown_Click(object sender, RoutedEventArgs e) {
            var selected = PlaylistView.SelectedItem as Track;
            if(selected == null)
                return;
            _viewModel.MoveDown_Click(selected);
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e) {
            var loginButton = (Button)sender;
            loginButton.IsEnabled = false;
            var username = UsernameBox.Text;
            var password = PasswordBox.Password;

            Tuple<SpotifyLoggedIn, LoginState> spotifyLoggedIn;
            if(string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                spotifyLoggedIn = _session.Login("jensstaermose@hotmail.com", "hejheider", false, _appkey);
            else
                spotifyLoggedIn = _session.Login(username, password, false, _appkey);

            if(spotifyLoggedIn.Item2 == LoginState.OK)
                OnLoginSuccess();
            else
                OnLogInError(spotifyLoggedIn.Item2);
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e) {
            _viewModel.TrackEnded();
        }

        private void AddRestriction_Click(object sender, RoutedEventArgs e) {
            var restrictionName = new RestrictionUnit(TrackField.Titles, "");
            var restrictionArtist = new RestrictionUnit(TrackField.Artists, "");
            var restriction = new Restriction(new DateTime(),
                                              new DateTime(1, 1, 1, 23, 59, 59),
                                              RestrictionType.BlackList,
                                              restrictionName,
                                              restrictionArtist);
            _viewModel.AddRestriction(restriction);

            var rd = new RestrictionDialog(restriction);
            rd.ShowDialog();
        }

        private void RemoveRestriction_Click(object sender, RoutedEventArgs e) {
            var selected = RestrictionsList.SelectedItem as Restriction;
            if(selected == null)
                return;
            _viewModel.RemoveRestriction(selected);
        }

        private void Control_OnMouseDoubleClick(object sender, MouseButtonEventArgs e) {
            var selected = RestrictionsList.SelectedItem as Restriction;
            if(selected == null)
                return;
            new RestrictionDialog(selected).ShowDialog();
        }
    }
}