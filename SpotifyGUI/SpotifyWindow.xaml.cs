using System;
using System.Collections.Generic;
using System.Windows;
using Newtonsoft.Json;
using System.Linq;
using Spotify;
using Spotify.Models;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Data;
using System.Globalization;
using System.Data;
using System.Windows.Input;

namespace SpotifyGUI
{

    /// <summary>
    /// Логика взаимодействия для SpotifyWindow.xaml
    /// </summary>
    public partial class SpotifyWindow : Window
    {
        Dictionary<string, string> headers = new Dictionary<string, string>();
        SpotifyWebClient client = new SpotifyWebClient();
        SpotifyWebBuilder builder = new SpotifyWebBuilder();
        public SpotifyWindow()
        {
            InitializeComponent();

            headers.Add("Authorization", "Bearer " + SpotifyLogin.AccessToken);
            Tuple<ResponseInfo, string> tuple = client.Download(builder.GetPrivateProfile(), headers);
            var obj = JsonConvert.DeserializeObject<PrivateProfile>(tuple.Item2);
            NameLabel.Text += obj.DisplayName;
            SpotifyLogin.IdCurrentUser = obj.Id;
            SpotifyLogin.CurrentLocation = obj.Country;

            DGCurrent.Visibility = Visibility.Hidden;
            DGAll.Visibility = Visibility.Visible;

            MyPlayBut.IsEnabled = false;

            UpdateMyPlayist();
        }



        public ImageSource GetImage(string link)
        {
            return BitmapFrame.Create(new Uri(link));
        }

        private void AboutMeBut_Click(object sender, RoutedEventArgs e)//?
        {
            MessageBox.Show("Create By: Unguryan Alexandr\nGit: Unguryan\nTelegram:@Unguryan");
        }

        private void UpdateFeatured()
        {
            try
            {
                Tuple<ResponseInfo, string> Featured = client.Download(builder.GetFeaturedPlaylists("", SpotifyLogin.CurrentLocation), headers);

                var obj = JsonConvert.DeserializeObject<FeaturedPlaylists>(Featured.Item2);

                Feat.Content = obj.Message;
                DGAll.Columns[2].Visibility = Visibility.Visible;
                DGAll.ItemsSource = obj.Playlists.Items;

            }
            catch { }
        }

        private void UpdateRecently()
        {
            try
            {
                Tuple<ResponseInfo, string> Recently = client.Download(builder.GetUsersRecentlyPlayedTracks(25), headers);

                var obj = JsonConvert.DeserializeObject<CursorPaging<PlayHistory>>(Recently.Item2);

                IEnumerable<SimplePlaylist> list = GetPlaylist(obj);
                DGAll.Columns[2].Visibility = Visibility.Visible;
                DGAll.ItemsSource = list;
            }
            catch { }
        }

        private IEnumerable<SimplePlaylist> GetPlaylist(CursorPaging<PlayHistory> ob)
        {
            try
            {
                IEnumerable<PlayHistory> obj = ob.Items.GroupBy(x => x.Track.Name)
                                                       .Select(g => g.First())
                                                       .ToList();

                List<SimplePlaylist> list = new List<SimplePlaylist>();
                foreach (var t in obj)
                {
                    Tuple<ResponseInfo, string> tuple1;
                    if (t.Track.Album.Type == "album" || t.Context.Type == "album")
                    {
                        tuple1 = client.Download(builder.GetAlbum(t.Track.Album.Uri.Substring(14)), headers);
                        var obj1 = JsonConvert.DeserializeObject<FullAlbum>(tuple1.Item2);
                        list.Add(new SimplePlaylist() { Name = obj1.Name, Id = obj1.Id, Images = obj1.Images, Type = obj1.Type, Tracks = new PlaylistTrackCollection() { Total = obj1.Tracks.Total } });
                    }
                    else if (t.Context.Type == "playlist")
                    {
                        tuple1 = client.Download(builder.GetPlaylist(t.Track.Album.Uri.Substring(17)), headers);
                        var obj2 = JsonConvert.DeserializeObject<FullPlaylist>(tuple1.Item2);
                        list.Add(new SimplePlaylist() { Name = obj2.Name, Id = obj2.Id, Images = obj2.Images, Type = obj2.Type, Owner = obj2.Owner, Tracks = new PlaylistTrackCollection() { Total = obj2.Tracks.Total } });
                    }
                    else if (t.Context.Type == "artist")
                    {
                        tuple1 = client.Download(builder.GetArtist(t.Track.Album.Uri.Substring(15)), headers);
                        var obj3 = JsonConvert.DeserializeObject<FullArtist>(tuple1.Item2);
                        list.Add(new SimplePlaylist() { Name = obj3.Name, Id = obj3.Id, Images = obj3.Images, Type = obj3.Type });
                    }
                }

                return list.GroupBy(x => x.Name).Select(g => g.First()).ToList();
            }
            catch { return null; }
        }


        private void UpdateMyPlayist()
        {
            try
            {
                Tuple<ResponseInfo, string> My = client.Download(builder.GetUserPlaylists(SpotifyLogin.IdCurrentUser), headers);

                var obj = JsonConvert.DeserializeObject<Paging<SimplePlaylist>>(My.Item2);

                DGAll.Columns[2].Visibility = Visibility.Visible;
                DGAll.ItemsSource = obj.Items;

            }
            catch { }
        }

        private void UpdateCharts()
        {
            try
            {
                Tuple<ResponseInfo, string> tuple1 = client.Download(builder.GetCategoryPlaylists("toplists", SpotifyLogin.CurrentLocation, 14), headers);

                var obj = JsonConvert.DeserializeObject<CategoryPlaylist>(tuple1.Item2);

                obj.Playlists.Items.RemoveRange(0, 10);
                DGAll.Columns[2].Visibility = Visibility.Visible;
                DGAll.ItemsSource = obj.Playlists.Items;
            }
            catch { }
        }

        private void UpdateNewReleases()
        {
            Tuple<ResponseInfo, string> tuple1 = client.Download(builder.GetNewAlbumReleases(SpotifyLogin.CurrentLocation, 20), headers);
            Tuple<ResponseInfo, string> tuple2 = client.Download(builder.GetCategoryPlaylists("toplists", "GB", 50), headers);

            var Obj = JsonConvert.DeserializeObject<CategoryPlaylist>(tuple2.Item2);
            var obj = JsonConvert.DeserializeObject<NewAlbumReleases>(tuple1.Item2);
            List<SimplePlaylist> list = new List<SimplePlaylist>();

            Tuple<ResponseInfo, string> tuple4 = client.Download(builder.GetPlaylist(Obj.Playlists.Items[3].Id), headers);
            var obj2 = JsonConvert.DeserializeObject<FullPlaylist>(tuple4.Item2);
            list.Add(new SimplePlaylist() { Name = obj2.Name, Id = obj2.Id, Images = obj2.Images, Owner = obj2.Owner, Type = obj2.Type, Tracks = new PlaylistTrackCollection() { Total = obj2.Tracks.Total } });


            foreach (var t in obj.Albums.Items)
            {
                Tuple<ResponseInfo, string> tuple3 = client.Download(builder.GetAlbum(t.Uri.Substring(14)), headers);
                var obj1 = JsonConvert.DeserializeObject<FullAlbum>(tuple3.Item2);
                list.Add(new SimplePlaylist() { Name = obj1.Name, Id = obj1.Id, Images = obj1.Images, Type = obj1.Type, Tracks = new PlaylistTrackCollection() { Total = obj1.Tracks.Total } });
            }


            DGAll.Columns[2].Visibility = Visibility.Visible;
            DGAll.ItemsSource = list;
        }

        private void UpdateGenres()
        {

            try
            {
                Tuple<ResponseInfo, string> tuple1 = client.Download(builder.GetCategories(SpotifyLogin.CurrentLocation, "", 40), headers);
                var obj = JsonConvert.DeserializeObject<CategoryList>(tuple1.Item2);
                List<SimplePlaylist> list = new List<SimplePlaylist>();
                foreach (var t in obj.Categories.Items)
                    list.Add(new SimplePlaylist() { Name = t.Name, Id = t.Id, Images = t.Icons });

                list.RemoveAt(0);
                DGAll.Columns[2].Visibility = Visibility.Hidden;
                DGAll.ItemsSource = list;
            }
            catch { }
        }



        private void UpdateBut_Click(object sender, RoutedEventArgs e)
        {
            if (!MyPlayBut.IsEnabled)
            {
                UpdateMyPlayist();
                return;
            }
            if (!FeaturedBut.IsEnabled)
            {
                UpdateFeatured();
                return;
            }
            if (!ChartsBut.IsEnabled)
            {
                UpdateCharts();
                return;
            }
            if (!RecentlyBut.IsEnabled)
            {
                UpdateRecently();
                return;
            }
            if (!GenresBut.IsEnabled)
            {
                UpdateGenres();
                return;
            }
            if (!NewRealisesBut.IsEnabled)
            {
                UpdateNewReleases();
                return;
            }
        }


        private void DGFeat_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SimplePlaylist temp = (SimplePlaylist)DGAll.SelectedItem;
            if (temp != null)
            {
                if (GenresBut.IsEnabled == false && BackBut.Visibility == Visibility.Hidden)
                {
                    Tuple<ResponseInfo, string> tuple = client.Download(builder.GetCategoryPlaylists(temp.Id), headers);

                    var obj = JsonConvert.DeserializeObject<CategoryPlaylist>(tuple.Item2);

                    DGAll.Columns[2].Visibility = Visibility.Visible;
                    DGAll.ItemsSource = obj.Playlists.Items;
                    Visible(BackButVis: Visibility.Visible, ChartBut: ChartsBut.IsEnabled, FeatBut: FeaturedBut.IsEnabled, GenreBut: GenresBut.IsEnabled, MyPBut: MyPlayBut.IsEnabled,
                        NewRealBut: NewRealisesBut.IsEnabled, RecBut: RecentlyBut.IsEnabled);
                }
                if (temp.Type == "playlist")
                {
                    Tuple<ResponseInfo, string> tuple = client.Download(builder.GetPlaylistTracks(temp.Owner.Id, temp.Id), headers);

                    var obj = JsonConvert.DeserializeObject<Paging<PlaylistTrack>>(tuple.Item2);

                    DGCurrent.ItemsSource = obj.Items;
                    Visible(DGAllVis:Visibility.Hidden, DGCurrentVis: Visibility.Visible, BackButVis: Visibility.Visible,
                        ChartBut: ChartsBut.IsEnabled, FeatBut: FeaturedBut.IsEnabled, GenreBut: GenresBut.IsEnabled, MyPBut: MyPlayBut.IsEnabled,
                        NewRealBut: NewRealisesBut.IsEnabled, RecBut : RecentlyBut.IsEnabled,
                         uriImage: temp.UrlImage, NamePlaylistLabel: temp.Name, TotalPlayListLabel: temp.Tracks.Total.ToString(), FeatText: Feat.Content.ToString() );
                }
                else if (temp.Type == "album")
                {
                    Tuple<ResponseInfo, string> tuple = client.Download(builder.GetAlbumTracks(temp.Id), headers);

                    var obj = JsonConvert.DeserializeObject<Paging<FullTrack>>(tuple.Item2);

                    List<PlaylistTrack> list = new List<PlaylistTrack>();
                    foreach (var t in obj.Items)
                    {
                        list.Add(new PlaylistTrack() { Track = t });
                    }
                    DGCurrent.ItemsSource = list;
                    Visible(DGAllVis: Visibility.Hidden, DGCurrentVis: Visibility.Visible, BackButVis: Visibility.Visible,
                        ChartBut: ChartsBut.IsEnabled, FeatBut: FeaturedBut.IsEnabled, GenreBut: GenresBut.IsEnabled, MyPBut: MyPlayBut.IsEnabled,
                        NewRealBut: NewRealisesBut.IsEnabled, RecBut: RecentlyBut.IsEnabled,
                          uriImage: temp.UrlImage, NamePlaylistLabel: temp.Name, TotalPlayListLabel: temp.Tracks.Total.ToString(), FeatText: Feat.Content.ToString());
                }
                else if (temp.Type == "artist")
                {
                    Tuple<ResponseInfo, string> tuple = client.Download(builder.GetArtistsTopTracks(temp.Id, "US"), headers);

                    var obj = JsonConvert.DeserializeObject<SeveralTracks>(tuple.Item2);

                    List<PlaylistTrack> list = new List<PlaylistTrack>();
                    foreach (var t in obj.Tracks)
                    {
                        list.Add(new PlaylistTrack() { Track = t });
                    }
                    DGCurrent.ItemsSource = list;
                    Visible(DGAllVis: Visibility.Hidden, DGCurrentVis: Visibility.Visible, BackButVis: Visibility.Visible,
                        ChartBut: ChartsBut.IsEnabled, FeatBut: FeaturedBut.IsEnabled, GenreBut: GenresBut.IsEnabled, MyPBut: MyPlayBut.IsEnabled,
                        NewRealBut: NewRealisesBut.IsEnabled, RecBut: RecentlyBut.IsEnabled,
                         uriImage: temp.UrlImage, NamePlaylistLabel: temp.Name, TotalPlayListLabel: temp.Tracks.Total.ToString(), FeatText: Feat.Content.ToString());
                }
            }
        }

        private void MyPlayListBut_Click(object sender, RoutedEventArgs e)
        {
            Visible(MyPBut: false);
            UpdateMyPlayist();
        }

        private void FeaturedBut_Click(object sender, RoutedEventArgs e)
        {
            Visible(Feat.Content.ToString(),FeatBut: false);
            UpdateFeatured();
        }

        private void RecentlyBut_Click(object sender, RoutedEventArgs e)
        {
            Visible(RecBut: false);
            UpdateRecently();
        }

        private void ChartsBut_Click(object sender, RoutedEventArgs e)
        {
            Visible(ChartBut: false);
            UpdateCharts();
        }

        private void GenreBut_Click(object sender, RoutedEventArgs e)
        {
            Visible(GenreBut: false);
            UpdateGenres();
        }

        private void NewRealisesBut_Click(object sender, RoutedEventArgs e)
        {
            Visible(NewRealBut: false);
            UpdateNewReleases();
        }



        private void DGAll_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
        }

        private void BackBut_Click(object sender, RoutedEventArgs e)
        {
            Visible(ChartBut: ChartsBut.IsEnabled, FeatBut: FeaturedBut.IsEnabled, GenreBut: GenresBut.IsEnabled, MyPBut: MyPlayBut.IsEnabled,
                        NewRealBut: NewRealisesBut.IsEnabled, RecBut: RecentlyBut.IsEnabled);
            UpdateBut_Click(sender, e);
        }

        private void SearchBut_Click(object sender, RoutedEventArgs e)
        {
            try { 
                Tuple<ResponseInfo, string> tuple = client.Download(builder.SearchItems(TBSearch.Text, Spotify.Enums.SearchType.Track, 30), headers);
                var obj = JsonConvert.DeserializeObject<SearchItem>(tuple.Item2);
                List<PlaylistTrack> list = new List<PlaylistTrack>();
                foreach (var t in obj.Tracks.Items)
                {
                    list.Add(new PlaylistTrack() { Track = t });
                }
                foreach (var g in list)
                {
                    if (g.Track.Artists != null)
                        g.Track.Name += " - " + g.Track.Artists.FirstOrDefault().Name;
                }
                Visible(DGAllVis: Visibility.Hidden, DGCurrentVis: Visibility.Visible, MyPBut: true);
                DGCurrent.ItemsSource = list;
            }
            catch
            {

            }
        }


        private void Visible(string FeatText = "", Visibility DGAllVis = Visibility.Visible, Visibility DGCurrentVis = Visibility.Hidden,
            Visibility BackButVis = Visibility.Hidden,  bool MyPBut = true, bool FeatBut = true, bool ChartBut = true,
            bool RecBut = true, bool GenreBut = true, bool NewRealBut = true, string uriImage = "", string NamePlaylistLabel = "", string TotalPlayListLabel = "")
        {
            Feat.Content = FeatText;
            DGAll.Visibility = DGAllVis;
            DGCurrent.Visibility = DGCurrentVis;
            BackBut.Visibility = BackButVis;
            MyPlayBut.IsEnabled = MyPBut;
            FeaturedBut.IsEnabled = FeatBut;
            ChartsBut.IsEnabled = ChartBut;
            RecentlyBut.IsEnabled = RecBut;
            GenresBut.IsEnabled = GenreBut;
            NewRealisesBut.IsEnabled = NewRealBut;
            if(uriImage != null && uriImage != "") {
                ImagePlaylist.Visibility = Visibility.Visible;
                ImagePlaylist.Source = new BitmapImage(new Uri(uriImage));
            }
            else
            {
                ImagePlaylist.Visibility = Visibility.Hidden;
            }
            if(NamePlaylistLabel != "") {
                NamePlayListLabel.Visibility = Visibility.Visible;
                NamePlayListLabel.Text = NamePlaylistLabel;
            }
            else
            {
                NamePlayListLabel.Visibility = Visibility.Hidden;
            }
            if(NamePlaylistLabel != "") {
                TotalPlaylistLabel.Visibility = Visibility.Visible;
                TotalPlaylistLabel.Text = "Total: " + TotalPlayListLabel;
            }
            else
            {
                TotalPlaylistLabel.Visibility = Visibility.Hidden;
            }

            TBSearch.Text = "";
        }

        private void TBSearch_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                SearchBut_Click(sender, e);
            }
        }


        private void SearchAsUrlBut_Click(object sender, RoutedEventArgs e)
        {
            if (TBSearch.Text.StartsWith("https://open.spotify.com/playlist/"))//24
            {
                Tuple<ResponseInfo, string> tuplePlay = client.Download(builder.GetPlaylist(TBSearch.Text.Substring(34)), headers);
                var objPlay = JsonConvert.DeserializeObject<FullPlaylist>(tuplePlay.Item2);

                Tuple<ResponseInfo, string> tuple = client.Download(builder.GetPlaylistTracks(objPlay.Owner.Id, objPlay.Id), headers);

                var obj = JsonConvert.DeserializeObject<Paging<PlaylistTrack>>(tuple.Item2);

                DGCurrent.ItemsSource = obj.Items;
                Visible(DGAllVis: Visibility.Hidden, DGCurrentVis: Visibility.Visible, BackButVis: Visibility.Hidden,
                    ChartBut: ChartsBut.IsEnabled, FeatBut: FeaturedBut.IsEnabled, GenreBut: GenresBut.IsEnabled, MyPBut: MyPlayBut.IsEnabled,
                    NewRealBut: NewRealisesBut.IsEnabled, RecBut: RecentlyBut.IsEnabled,
                     uriImage: objPlay.UrlImage, NamePlaylistLabel: objPlay.Name, TotalPlayListLabel: objPlay.Tracks.Total.ToString(), FeatText: Feat.Content.ToString());
                return;
            }
            if (TBSearch.Text.StartsWith("https://open.spotify.com/show/"))
            {
                MessageBox.Show("Shows are not available.");
                return;
            }
            if (TBSearch.Text.StartsWith("https://open.spotify.com/album/"))
            {

                Tuple<ResponseInfo, string> tuplePlay = client.Download(builder.GetAlbum(TBSearch.Text.Substring(31)), headers);
                var objPlay = JsonConvert.DeserializeObject<FullPlaylist>(tuplePlay.Item2);
                Tuple<ResponseInfo, string> tuple = client.Download(builder.GetAlbumTracks(TBSearch.Text.Substring(31)), headers);

                var obj = JsonConvert.DeserializeObject<Paging<FullTrack>>(tuple.Item2);

                List<PlaylistTrack> list = new List<PlaylistTrack>();
                foreach (var t in obj.Items)
                {
                    list.Add(new PlaylistTrack() { Track = t });
                }
                
                DGCurrent.ItemsSource = list;
                Visible(DGAllVis: Visibility.Hidden, DGCurrentVis: Visibility.Visible, BackButVis: Visibility.Hidden,
                    ChartBut: ChartsBut.IsEnabled, FeatBut: FeaturedBut.IsEnabled, GenreBut: GenresBut.IsEnabled, MyPBut: MyPlayBut.IsEnabled,
                    NewRealBut: NewRealisesBut.IsEnabled, RecBut: RecentlyBut.IsEnabled,
                      uriImage: objPlay.UrlImage, NamePlaylistLabel: objPlay.Name, TotalPlayListLabel: objPlay.Tracks.Total.ToString(), FeatText: Feat.Content.ToString());
                return;
            }
            if (TBSearch.Text.StartsWith("https://open.spotify.com/artist/"))
            {
                Tuple<ResponseInfo, string> tuplePlay = client.Download(builder.GetArtist(TBSearch.Text.Substring(32)), headers);
                var objPlay = JsonConvert.DeserializeObject<FullArtist>(tuplePlay.Item2);

                Tuple<ResponseInfo, string> tuple = client.Download(builder.GetArtistsTopTracks(TBSearch.Text.Substring(32), "US"), headers);

                var obj = JsonConvert.DeserializeObject<SeveralTracks>(tuple.Item2);

                List<PlaylistTrack> list = new List<PlaylistTrack>();
                foreach (var t in obj.Tracks)
                {
                    list.Add(new PlaylistTrack() { Track = t });
                }
                DGCurrent.ItemsSource = list;
                Visible(DGAllVis: Visibility.Hidden, DGCurrentVis: Visibility.Visible, BackButVis: Visibility.Hidden,
                    ChartBut: ChartsBut.IsEnabled, FeatBut: FeaturedBut.IsEnabled, GenreBut: GenresBut.IsEnabled, MyPBut: MyPlayBut.IsEnabled,
                    NewRealBut: NewRealisesBut.IsEnabled, RecBut: RecentlyBut.IsEnabled,
                     uriImage: objPlay.UrlImage, NamePlaylistLabel: objPlay.Name, TotalPlayListLabel: obj.Tracks.Count.ToString(), FeatText: Feat.Content.ToString());
                return;
            }
        }
    }

    [ValueConversion(typeof(string), typeof(BitmapImage))]
    public class ImageConverter : IValueConverter
    {
        public object Convert(
            object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new BitmapImage(new Uri(value.ToString()));
        }

        public object ConvertBack(
            object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }


    }
}
