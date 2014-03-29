<%@ Page Title="FoodBySearch" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="FoodBySearch.aspx.cs" Inherits="TravelFood.Food.FoodBySearch" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <!-- Search Bar -->
    <div>
        <input id="txtSearch" type="text" />
        <button id="btnSearch" type="button" onclick="SearchPlace">Search</button>
    </div>

    <!-- Result -->
    <div id="results">
    </div>

    <!-- Google Map -->
    <div id="map-canvas"></div>
</asp:Content>
