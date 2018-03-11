<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Program5._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="jumbotron">
        <h1>CSS 490 Program 5 by JJ Abides and Peter Stanton</h1>
        <p class="lead">Welcome to our song artist viewer. Here you can input a song artist name and view their album and song data.</p>
        <br />

        <p>Artist Name</p>
        <asp:TextBox ID="TextBox1" runat="server"></asp:TextBox>

        <br />
        <asp:Button ID="Button1" runat="server" OnClick="Button1_Click" Text="Enter" />

        <br />
        <br />
        <p>Enter album</p>
        <asp:TextBox ID="TextBox2" runat="server"></asp:TextBox>
        <br />

        <asp:Button ID="Button2" runat="server" Text="Query" />

    </div>

    <div class="row">
        <div class="col-md-4">
            <asp:Label ID="ResultsTitle" Enabled="false" Font-Size="Large" runat="server">Results:</asp:Label>
            <br />

            <asp:Label ID="Results" runat="server"></asp:Label>
          
        </div>
    </div>

</asp:Content>
