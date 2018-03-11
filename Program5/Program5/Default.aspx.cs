using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SpotifyAPI.Web; //Base Namespace
using SpotifyAPI.Web.Auth; //All Authentication-related classes
using SpotifyAPI.Web.Enums; //Enums
using SpotifyAPI.Web.Models; //Models for the JSON-responses



namespace Program5
{
    public partial class _Default : Page
    {

        static ClientCredentialsAuth auth;
        private static SpotifyWebAPI ourPlayer;
        protected void Page_Load(object sender, EventArgs e)
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

        protected void searchButtion_Click(object sender, EventArgs e)
        {
            if(String.IsNullOrWhiteSpace(searchEntryBox.Text))
            {
                return;
            }
            SearchItem item = ourPlayer.SearchItems(searchEntryBox.Text.ToString(), SearchType.Artist | SearchType.Playlist
                | SearchType.Album);
            for(int i = 0; i < item.Artists.Total; i++)
            {
                ListBox1.Items.Add(item.Artists.Items[i].Name.ToString());
            }
        }
    }
}