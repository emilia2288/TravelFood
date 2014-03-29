<%@ Page Title="FoodByFriends" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="FoodByFriends.aspx.cs" Inherits="TravelFood.Food.FoodByFriends" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <!-- Search Bar -->
    <div>
        <input id="txtSearch" type="text" />
        <asp:Button ID="btnSearch" runat="server" Text="Search" />
    </div>

    <!-- Result -->
    <div id="results">
    </div>

    <!-- Google Map -->
    <div id="map-canvas"></div>
</asp:Content>
