using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SpotifyAPI.Web; //Base Namespace
using SpotifyAPI.Web.Auth; //All Authentication-related classes
using SpotifyAPI.Web.Enums; //Enums
using SpotifyAPI.Web.Models; //Models for the JSON-responses
using Microsoft.WindowsAzure.Storage.Blob;   //For blob storage
using Microsoft.WindowsAzure.Storage.Table;  //For artistcloudtable storage
using Microsoft.Azure;     //Configuration manager
using Microsoft.WindowsAzure.Storage;   //For storage in general
using System.IO;
using Microsoft.ApplicationInsights;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace Program5
{

    public partial class _Default : Page
    {
        static ClientCredentialsAuth auth;
        private static SpotifyWebAPI ourPlayer;
        private static SearchItem item;
        private static int state;
        private const String musicURL = @"https://api.musixmatch.com/ws/1.1/matcher.lyrics.get?format=jsonp&callback=callback";
        //&q_track=Black%20Diamonds&q_artist=Therion
        private const string apiKey = "&apikey=c1efc87437d8289718e51d58de5be083";
        private static string inTrack, inArtist, inAlbum;
        private static CloudTable artistcloudtable;
        private static bool login;


        protected void Page_Load(object sender, EventArgs e)
        {
            if (!login)
            {
                //Create the auth object
                auth = new ClientCredentialsAuth()
                {
                    //Your client Id
                    ClientId = "233de2f259b54609bedb58dfe5f037d7",
                    //Your client secret UNSECURE!!
                    ClientSecret = "588ddce164f7461283f80be878c43b37",
                    //How many permissions we need?
                    Scope = Scope.UserReadPrivate,
                };
                //With this token object, we now can make calls
                Token token = auth.DoAuth();
                ourPlayer = new SpotifyWebAPI()
                {
                    TokenType = token.TokenType,
                    AccessToken = token.AccessToken,
                    UseAuth = true
                };
                login = true;
            }


            if (Label1.Text == "List of artists")
                state = 1;
            else if (Label1.Text == "List of albums")
                state = 2;
            else if (Label1.Text == "List of tracks")
                state = 3;

            
        }

        protected void searchButton_Click(object sender, EventArgs e)
        {
            lyricsBox.Text = "";
            state = 0;
            if (String.IsNullOrWhiteSpace(searchEntryBox.Text) || ourPlayer == null)
            {
                return;
            }

            try
            {
                ListBox2.Items.Clear();
                Label1.Text = "List of artists";
                item = null;
                item = ourPlayer.SearchItems(searchEntryBox.Text.ToString(), SearchType.Artist);
                for (int i = 0; i < item.Artists.Items.Count; i++)
                {
                    ListBox2.Items.Add(item.Artists.Items[i].Name.ToString());
                }

                state = 1; //This allows for the user to click on an album

                CloudStorageAccount myAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
                CloudBlobClient blobClient = myAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference("css490finalproject"); //Our blob for collecting searches
                container.CreateIfNotExists();
                CloudBlockBlob myBlob = container.GetBlockBlobReference("spotifysearchresults.txt"); //The text file for storing the searches
                string contents = "";
                try
                {
                    contents = myBlob.DownloadText() + "\n";
                }
                catch (Exception)
                {

                }

                myBlob.UploadText(contents + searchEntryBox.Text + "\n");       //This is how we add the search result to the list of search results in blob


                //This is where we add the list of artists to the artistcloudtable
                CloudTableClient tableClient = null;
                artistcloudtable = null;
                try
                {

                    tableClient = myAccount.CreateCloudTableClient();
                    artistcloudtable = tableClient.GetTableReference("Artists");
                }
                catch (Exception)
                {
                    ErrorText.Text = "Error: Problem connecting to cloud artistcloudtable.";
                    return;
                }


                try
                {
                    artistcloudtable.CreateIfNotExists();
                }
                catch (Exception)
                {
                    ErrorText.Text = "Error: Could not create artistcloudtable...";
                }

                ErrorText.Text = "Search successfully completed.";

                ArtistData artists = new ArtistData();
            }
            catch (Exception l)
            {
                ErrorText.Text = "Error: problem finding/loading artists.";
            }


        }



        protected void authenticateButton_Click(object sender, EventArgs e)
        {
            
        }

        public class ArtistData
        {
            public List<DynamicTableEntity> UserDataList;
            public Dictionary<string, EntityProperty> data;
            public DynamicTableEntity entity = null;

            public ArtistData()
            {
                UserDataList = new List<DynamicTableEntity>();
            }

            public void addAttribute(string key, string value)
            {
                data.Add(key, new EntityProperty(value));
            }

            public void addEntityToDataList()
            {
                entity.Properties = data;
                UserDataList.Add(entity);
            }

            public void createNewEntity(string partitionKey, string rowKey)
            {
                entity = new DynamicTableEntity();
                data = new Dictionary<string, EntityProperty>();
                entity.PartitionKey = partitionKey;
                entity.RowKey = rowKey;
            }
        }

        protected void ListBox2_OnClick(object sender, EventArgs e)
        {

        }
        protected void ListBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ListBox2.Items[ListBox2.SelectedIndex].Text == "") return;

            if (state == 0) //This is when there's nothing in the list box
                return;

            else if (state == 1)
            {
                item = ourPlayer.SearchItems(ListBox2.Items[ListBox2.SelectedIndex].Text, SearchType.Album);
                ErrorText.Text = "Selected Artist: " + ListBox2.Items[ListBox2.SelectedIndex].Text;
                inArtist = ListBox2.Items[ListBox2.SelectedIndex].Text;
                inArtist.Replace(' ', '%');
                Label1.Text = "List of albums";
                ListBox2.Items.Clear();

                for (int i = 0; i < item.Albums.Items.Count; i++)
                {
                    ListBox2.Items.Add(item.Albums.Items[i].Name.ToString());
                }

                state = 2;
            }
        

            else if (state == 2)
            {
            
                item = ourPlayer.SearchItems(ListBox2.Items[ListBox2.SelectedIndex].Text, SearchType.Track);
                ErrorText.Text = "Selected Album: " + ListBox2.Items[ListBox2.SelectedIndex].Text;
                inAlbum = ListBox2.Items[ListBox2.SelectedIndex].Text;
                Label1.Text = "List of tracks";
                ListBox2.Items.Clear();

                for (int i = 0; i < item.Tracks.Items.Count; i++)
                {
                    ListBox2.Items.Add(item.Tracks.Items[i].Name.ToString());
                }

                state = 3;
            }
            else if (state == 3)
            {
                bool hasLyrics = true;
                inTrack = ListBox2.Items[ListBox2.SelectedIndex].Text;
                Regex spacer = new Regex(@"\s+");
                //     Regex lyrics = new Regex(@"")
                spacer.Replace(inTrack, "%");
                //&q_track=Black%20Diamonds&q_artist=Therion
                HttpResponseMessage msg = new HttpResponseMessage();
                using (var client = new HttpClient())
                {
                    Uri target = new Uri(musicURL + "&q_track=" + inTrack + "&q_artist=" + inArtist + apiKey);
                    if (Uri.IsWellFormedUriString(target.AbsoluteUri.ToString(), UriKind.Absolute))
                    {
                        msg = client.GetAsync(target.AbsoluteUri).Result;
                    }
                    String result = String.Empty;
                    try
                    {
                        result = msg.Content.ReadAsStringAsync().Result;
                        List<String> parsed = new List<string>();
                        parsed = result.Split(',').ToList();
                        String match = String.Empty;
                        for (int i = 0; i < parsed.Count; i++)
                        {
                            if (parsed.ElementAt(i).Contains("lyrics_body"))
                            {
                                match = parsed.ElementAt(i);
                                break;
                            }
                        }
                        if (match.Equals(String.Empty))
                        {
                            ErrorText.Text = "This song has no lyrics, please try another search.";
                            state = 0; //reset.    
                            hasLyrics = false;
                        }
                        if (hasLyrics)
                        {
                            List<String> lyricParsed = new List<string>();
                            lyricParsed = match.Split('"').ToList();

                            for (int i = 0; i < lyricParsed.Count; i++)
                            {
                                if (lyricParsed.ElementAt(i).Contains(":") && i < (lyricParsed.Count - 1))
                                {
                                    lyricsBox.Text = lyricParsed.ElementAt(i + 1).ToString().Replace("******* This Lyrics is NOT " +
                                        "for Commercial use *******", "").Replace(@"\n", Environment.NewLine);
                                }
                            }
                        }
                    }
                    catch (Exception j)
                    {
                        ErrorText.Text = "Couldn't display lyrics...";
                    }
                    
                }
                //create artistcloudtable entry here.
                DynamicTableEntity searchRecord = new DynamicTableEntity();
                Dictionary<string, EntityProperty> data1 = new Dictionary<string, EntityProperty>();
                data1.Add("artist", new EntityProperty(inArtist));
                data1.Add("album", new EntityProperty(inAlbum));
                data1.Add("track", new EntityProperty(inTrack));
                data1.Add("lyrics", new EntityProperty(hasLyrics));    
                searchRecord.Properties = data1;
                searchRecord.PartitionKey = "project490partition";
                searchRecord.RowKey = Convert.ToString(DateTime.UtcNow.Ticks.ToString("d20"));
                var updater = TableOperation.InsertOrReplace(searchRecord);
                artistcloudtable.Execute(updater);
            }
        } //end of listbox2 click.
    }
}


/*
protected void getDevicesButton_Click(object sender, EventArgs e)
{
    devicesListBox.Items.Clear();
    if (ourPlayer == null)
        return;
    devices = ourPlayer.GetDevices();
    devices.Devices.ForEach(device => devicesListBox.Items.Add(device.Name));
}

protected void playButton_Click(object sender, EventArgs e)
{
    int deviceIndex = -1, mediaIndex = -1;
    try
    {
        deviceIndex = devicesListBox.SelectedIndex;
        mediaIndex = ListBox1.SelectedIndex;
    }
    catch (Exception)
    {
        return;
    }
    if (ourPlayer == null || mediaIndex == -1 || deviceIndex == -1)
    {
        return;
    }


    PlaybackContext context = ourPlayer.GetPlayback();
    if (context.Item != null && context.Device.Id != devices.Devices[deviceIndex].Id)
    {
        ErrorResponse changeDevice = ourPlayer.TransferPlayback(devices.Devices[deviceIndex].Id.ToString(),true);
    }

    ErrorResponse error = ourPlayer.ResumePlayback(devices.Devices[deviceIndex].Id.ToString(),
        item.Artists.Items[mediaIndex].Uri.ToString());
}
} */
