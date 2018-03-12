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
using System.IO;
using Microsoft.ApplicationInsights;

namespace Program5
{
    
    public partial class _Default : Page
    {
        static ClientCredentialsAuth auth;
        private static SpotifyWebAPI ourPlayer;
        private static SearchItem item;
        private static AvailabeDevices devices;
        private static int state;
        

        protected void Page_Load(object sender, EventArgs e)
        {
            
        }

        protected void searchButton_Click(object sender, EventArgs e)
        {
            state = 0;
            if (String.IsNullOrWhiteSpace(searchEntryBox.Text) || ourPlayer == null)
            {
                return;
            }
            ListBox2.Items.Clear();
            Label1.Text = "List of artists";
            item = null;
            item = ourPlayer.SearchItems(searchEntryBox.Text.ToString(), SearchType.Artist);
            for (int i = 0; i < item.Artists.Items.Count; i++)
            {
                ListBox2.Items.Add(item.Artists.Items[i].Name.ToString());
            }

            if (ListBox2.Items.Count > 0) state = 1; //This allows for the user to click on an album

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
            catch (Exception g)
            {

            }

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


            try
            {
                table.CreateIfNotExists();
            }
            catch (Exception l)
            {
                ErrorText.Text = "Error: Could not create table...";
            }

            ErrorText.Text = "Search successfully completed.";

            ArtistData artists = new ArtistData();


        }

        

        protected void authenticateButton_Click(object sender, EventArgs e)
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
                Label1.Text = "List of Albums";
                ListBox2.Items.Clear();

                for (int i = 0; i < item.Albums.Items.Count; i++)
                {
                    ListBox2.Items.Add(item.Albums.Items[i].Name.ToString());
                }

                if (ListBox2.Items.Count > 0) state = 2;
                else state = 0;
            }

            else if (state == 2)
            {
                item = ourPlayer.SearchItems(ListBox2.Items[ListBox2.SelectedIndex].Text, SearchType.Track);
                ErrorText.Text = "Selected Album: " + ListBox2.Items[ListBox2.SelectedIndex].Text;
                Label1.Text = "List of Tracks";
                ListBox2.Items.Clear();

                for (int i = 0; i < item.Tracks.Items.Count; i++)
                {
                    ListBox2.Items.Add(item.Tracks.Items[i].Name.ToString());
                }

                state = 0;
            }
        }
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
