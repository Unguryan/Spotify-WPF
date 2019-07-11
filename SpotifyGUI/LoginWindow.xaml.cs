using Spotify.Auth;
using Spotify.Enums;
using Spotify.Models;
using System.Collections.Generic;
using System.Threading;
using System.Windows;


namespace SpotifyGUI
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        AutorizationCodeAuth auth;

        Dictionary<string, string> headers = new Dictionary<string, string>();

        public Window1()
        {
            InitializeComponent();
            auth = new AutorizationCodeAuth()
            {
                ShowDialog = false,
                Scope = Scope.UserReadRecentlyPlayed | Scope.UserReadPrivate | Scope.UserReadEmail | Scope.UserReadBirthdate | Scope.UserLibraryRead | Scope.PlaylistReadPrivate,
                ClientId = SpotifyLogin.GetClient_Id,
                RedirectUri = SpotifyLogin.GetRedirect_Uri
            };

            auth.OnResponseReceivedEvent += Auth_OnResponseReceivedEvent;

        }

        private void Auth_OnResponseReceivedEvent(AutorizationCodeAuthResponse response)
        {
            if(response.Error == null)
            {
                SpotifyLogin.GrantCode = response.Code;
            }
            else
            {
                MessageBox.Show("Error!");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Autorization();
        }

        private void Autorization()
        {
            auth.StartHttpServer();
            auth.DoAuth();
            Thread.Sleep(1000);
            Token a = auth.ExchangeAuthCode(SpotifyLogin.GrantCode, SpotifyLogin.GetClient_Secret);

            headers.Add("Authorization", "Bearer " + a.AccessToken);

            if (a != null)
            {
                MessageBox.Show("Complete!");
                SpotifyLogin.AccessToken = a.AccessToken;
                SpotifyLogin.RefreshToken = a.RefreshToken;
                SpotifyWindow main = new SpotifyWindow();
                main.Show();
                Close();
            }
        }
    }


    class SpotifyLogin
    {
        public static string GetClient_Id { get; } = "3068c22f912e48c79c734c53d52b81dc";

        public static string GetClient_Secret { get; } = "c48d9c7da23b48f3bd13efac7ef855f8";

        public static string GetRedirect_Uri { get; } = "http://localhost:80";

        public static string GrantCode { get; set; }

        public static string AccessToken { get; set; }

        public static string RefreshToken { get; set; }

        public static string IdCurrentUser { get; set; }

        public static string CurrentLocation { get; set; }
    }
}
