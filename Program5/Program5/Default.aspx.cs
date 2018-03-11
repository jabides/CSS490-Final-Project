using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SpotifyAPI.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.Azure;
using System.IO;

namespace Program5
{
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            //Take the input and place it into a request
            string input = TextBox1.Text;

            if (input.Length == 0) return; //If the input is empty, don't do anything

            ResultsTitle.Enabled = true;

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("css490finalproject");


            if (table.Exists()) //How do we update the table without deleting it?
            {
                table.Delete();
            }

            try
            {
                table.Create();
            }
            catch (Exception k)
            {
                ResultsTitle.Text = "The previous table is currently being deleted. Please wait a few seconds.";
                return;
            }



            
            

            //Here we need to store each name into the table


        }
    }

    public class UserData
    {
        public List<DynamicTableEntity> UserDataList;
        public Dictionary<string, EntityProperty> data;
        public DynamicTableEntity entity = null;

        public UserData()
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