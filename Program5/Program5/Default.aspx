﻿<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Program5._Default" %>

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
    </div>

    <div class="row">
        <div class="col-md-4">
            <p>
    
                <asp:ListBox ID="ListBox1" runat="server" Width="669px"></asp:ListBox>
                <br />
                <asp:Button ID="authenticateButton" runat="server" OnClick="authenticateButton_Click" Text="login" />
                <asp:Button ID="getDevicesButton" runat="server" OnClick="getDevicesButton_Click" Text="Load devices" />
                <asp:Button ID="playButton" runat="server" OnClick="playButton_Click" Text="Play on device" />
            </p>
        </div>
        <div class="col-md-4">
            <p>
                
                <asp:ListBox ID="devicesListBox" runat="server" Height="57px" Width="717px"></asp:ListBox>
            </p>
        </div>
        <div class="col-md-4">
          
        </div>
    </div>

</asp:Content>
