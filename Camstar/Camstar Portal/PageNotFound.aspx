<%-- Copyright Siemens 2023   --%>
<%@ Page Language="C#" AutoEventWireup="true" CodeFile="PageNotFound.aspx.cs" Inherits="PageNotFound" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Page Not Found</title>
        <asp:PlaceHolder runat="server">
            <%= this.styleSheetString %>
        </asp:PlaceHolder>
</head>

<body class="ui-customerror">
    <script type="text/javascript" src="scripts/jquery/jquery.min.js"></script>
    <script type="text/javascript" src="scripts/jquery/jquery-ui.min.js"></script>
    <form id="form1" runat="server">
        <div class="header">
            <img src="Images/StatusBar/ErrorXIcon.png" style="float: left; margin-right: 10px;" />
            <span id="cs-title" class="title">404, Requested Page Not Found!</span><br />
        </div>
    </form>
</body>
</html>
