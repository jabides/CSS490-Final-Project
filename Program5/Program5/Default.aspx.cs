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
using Microsoft.WindowsAzure.Storage.Table;  //For table storage
using Microsoft.Azure;     //Configuration manager
using Microsoft.WindowsAzure.Storage;   //For storage in general

namespace Program5
{
    public partial class _Default : Page
    {
        static AutorizationCodeAuth auth1;
        private static SpotifyWebAPI ourPlayer;
        private static SearchItem item;
        private static AvailabeDevices devices;
        protected void Page_Load(object sender, EventArgs e)
        {
           
        }

        protected void searchButton_Click(object sender, EventArgs e)
        {
            if(String.IsNullOrWhiteSpace(searchEntryBox.Text) || ourPlayer == null)
            {
                return;
            }
            ListBox1.Items.Clear();
            

            item = ourPlayer.SearchItems(searchEntryBox.Text.ToString(), SearchType.Artist);
            for(int i = 0; i < item.Artists.Total; i++)
            {
                ListBox1.Items.Add(item.Artists.Items[i].Name.ToString());
            }

            CloudStorageAccount myAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            CloudBlobClient blobClient = myAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("css490finalproject"); //Our blob for collecting searches
            container.CreateIfNotExists();
            CloudBlockBlob myBlob = container.GetBlockBlobReference("spotifysearchresults.txt"); //The text file for storing the searches

            string contents = myBlob.DownloadText();
            myBlob.UploadText(contents + searchEntryBox.Text + "\n");       //This is how we add the search result to the list of search results in blob


            //This is where we add the list of artists to the table
            CloudTableClient tableClient = null;
            CloudTable table = null;
            try
            {
                
                tableClient = myAccount.CreateCloudTableClient();
                table = tableClient.GetTableReference("Artists");
            }
            catch (Exception l)
            {
                ErrorText.Text = "Error: Problem connecting to cloud table.";
                return;
            }

            if (table.Exists()) //How do we update the table without deleting?
                table.Delete();

            try
            {
                table.Create();
            }
            catch (Exception k)
            {
                ErrorText.Text = "The previous table is currently being deleted. Please wait a few seconds.";
                return;
            }

            ErrorText.Text = "Search successfully completed.";

            ArtistData artists = new ArtistData();
            for (int i = 0; i < ListBox1.Items.Count; i++)
            {
                //This is where we add artists one by one
            }
            

        }

        protected void authenticateButton_Click(object sender, EventArgs e)
        {
            //Create the auth object
            auth1 = new AutorizationCodeAuth()
            {
                //Your client Id
                ClientId = "233de2f259b54609bedb58dfe5f037d7",
                //Set this to localhost if you want to use the built-in HTTP Server
                RedirectUri = "http://localhost",
                //How many permissions we need?
                Scope = Scope.UserReadPlaybackState | Scope.Streaming | Scope.UserModifyPlaybackState
                | Scope.UserReadPrivate
            };
            //This will be called, if the user cancled/accept the auth-request
            auth1.OnResponseReceivedEvent += auth1_OnResponseReceivedEvent;
            //a local HTTP Server will be started (Needed for the response)
            auth1.StartHttpServer();
            //This will open the spotify auth-page. The user can decline/accept the request
            auth1.DoAuth();       
            Thread.Sleep(60000);
            auth1.StopHttpServer();
            Console.WriteLine("Too long, didnt respond, exiting now...");
        }
        private static void auth1_OnResponseReceivedEvent(AutorizationCodeAuthResponse response)
        {

            //NEVER DO THIS! You would need to provide the ClientSecret.
            //You would need to do it e.g via a PHP-Script.
            Token token = auth1.ExchangeAuthCode(response.Code, "588ddce164f7461283f80be878c43b37");

            ourPlayer = new SpotifyWebAPI()
            {
                TokenType = token.TokenType,
                AccessToken = token.AccessToken
            };

            //With the token object, you can now make API calls
            //Stop the HTTP Server, done.
            auth1.StopHttpServer();          
        }

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
}