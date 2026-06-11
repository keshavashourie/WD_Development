<%-- Copyright Siemens 2023   --%>
<%@ Page Language="C#" AutoEventWireup="true" CodeFile="CamstarErrorPage.aspx.cs" Inherits="ErrorPage" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Error Page</title>
        <asp:PlaceHolder runat="server">
            <%= this.styleSheetString %>
        </asp:PlaceHolder>
</head>

<body class="ui-customerror">
    <script type="text/javascript" src="scripts/jquery/jquery.min.js"></script>
    <script type="text/javascript" src="scripts/jquery/jquery-ui.min.js"></script>
    <script src="Scripts/CustomError.js"></script>
    <form id="form1" runat="server">
        <div class="header">
            <img src="Images/StatusBar/ErrorXIcon.png" style="float: left; margin-right: 10px;" />
            <span id="cs-title" class="title"><asp:Label ID="lblTitle" runat="server" Text="Label"></asp:Label></span><br />
            <asp:Label ID="lblDescription" runat="server" Text="Label"></asp:Label>
        </div>
        <div class="details-header">
            <span><asp:Label ID="lblDetails" runat="server" Text="Label"></asp:Label></span>
            <span id="arrow" class="arrow" />
            
        </div>
        <div class="details">
            <h2><asp:Label ID="lblError" CssClass="detail-error" runat="server" Text="Label"></asp:Label></h2>
            <span class="detail-label">Stack Trace:</span><br />
            <div class="stacktrace">
                <asp:Label ID="lblErrorDetail" runat="server" Text="Label"></asp:Label>
            </div>
            
        </div>
    </form>

</body>
</html>
