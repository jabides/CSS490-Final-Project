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
        static ClientCredentialsAuth auth;
        private static SpotifyWebAPI ourPlayer;
        private static SearchItem item;
        private static AvailabeDevices devices;
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void searchButton_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(searchEntryBox.Text) || ourPlayer == null)
            {
                return;
            }
            ListBox2.Items.Clear();

            item = null;
            item = ourPlayer.SearchItems(searchEntryBox.Text.ToString(), SearchType.Artist);
            for (int i = 0; i < item.Artists.Items.Count; i++)
            {
                ListBox2.Items.Add(item.Artists.Items[i].Name.ToString());
            }

            CloudStorageAccount myAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            CloudBlobClient blobClient = myAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("css490finalproject"); //Our blob for collecting searches
            container.CreateIfNotExists();
            CloudBlockBlob myBlob = container.GetBlockBlobReference("spotifysearchresults.txt"); //The text file for storing the searches
            string contents = "";
            try
            {
                contents = myBlob.DownloadText();
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

        protected void ListBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ListBox2.SelectedValue == "") return;
            item = ourPlayer.SearchItems(ListBox2.SelectedValue, SearchType.Album);

            ListBox2.Items.Clear();

            for (int i = 0; i < item.Albums.Total; i++)
            {
                ListBox2.Items.Add(item.Albums.Items[i].Name.ToString());
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
