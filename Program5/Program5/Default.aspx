<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Program5._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="jumbotron">
        <h1>CSS 490 Program 5 by JJ Abides and Peter Stanton</h1>
        <p class="lead">Welcome to our Spotify search engine. Here you can view album and song data of a song artist.</p>
        <br />
        <p>Artist Name</p>
        <asp:TextBox ID="searchEntryBox" runat="server"></asp:TextBox>
        &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
        <asp:Label ID="ErrorText" runat="server"></asp:Label>
        <br />
        <asp:Button ID="searchButton" runat="server" OnClick="searchButton_Click" Text="Search" />
        <asp:TextBox ID="lyricsBox" runat="server" Height="72px" TextMode="MultiLine" Width="263px"></asp:TextBox>
    </div>

    <div class="row">
        <div class="col-md-4">
            <p>
    
              
                <br />
                <asp:Button ID="authenticateButton" runat="server" OnClick="authenticateButton_Click" Text="login" />
                <br />
                <asp:Label ID="Label1" runat="server"></asp:Label>
                <br />
                <asp:ListBox ID="ListBox2" runat="server" Width="800px" Height="300px" OnSelectedIndexChanged="ListBox2_SelectedIndexChanged" AutoPostBack="True"></asp:ListBox>
            </p>
        </div>
     
       
    </div>

</asp:Content>
