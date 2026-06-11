<%-- Copyright Siemens 2025   --%>

<%@ Page Language="c#" CodeFile="Default.aspx.cs" AutoEventWireup="false" Inherits="Camstar.Portal.Default" %>

<!doctype html>
<html xmlns="http://www.w3.org/1999/xhtml" xml:lang="en" lang="en">

<head>

    <title>Opcenter Execution</title>
    <meta name="viewport" content="width=device-width" initial-scale="1.0" />
    <asp:PlaceHolder runat="server">
        <%= this.styleSheetString %>
    </asp:PlaceHolder>

</head>
<body>
    <%: Scripts.Render("~/bundles/login") %>

    <form id="form1" runat="server" method="post">
        <%-- <div class="login-background">--%>
        <div class="ui-login">
            <div class="siemens-logo">
                <img src="/CamstarPortal/Images/logoSiemensWhite48.svg" />
            </div>
            <div id="WebPart_StatusBar" class="webpart webpart-status" messagetype="Error">
                <div>
                    <div class="message">
                        <div class="messageType" id="divStatusMessage">
                            <span class="message-status-type">Error</span><span class="instruction"><asp:Label ID="ErrorLabel" runat="server"></asp:Label></span>
                        </div>
                    </div>
                </div>
                <input name="ctl00$WebPartManager$StatusBar$StatusMessageExists" type="hidden" id="ctl00_WebPartManager_StatusBar_StatusMessageExists" value="1" />
                <a id="ctl00_WebPartManager_StatusBar_CloseStatusButton" class="close"></a>
            </div>
            <div class="ui-login-signin">
                <div class="login-container">
                    <div class="ui-login-main" id="LoginMainBlock" runat="server">
                        <div class="login-header">
                            <div class="product-logo-container">

                                <img class="product-logo" src="/CamstarPortal/Images/desktopOcEx48.svg" role="presentation" />
                            </div>
                            <div>
                                <div class="system-name">OPCENTER EXECUTION</div>
                                <div class="product-name">Semiconductor</div>
                            </div>
                        </div>

                        <ul>
                            <li>
                                <div id="userNameAsterisk" class="teal-asterisk" runat="server">*</div>
                                <asp:Label ID="UsernameLabel" runat="server"> User Name</asp:Label>
                                <asp:Label ID="LanguageLabel" runat="server" Visible="false">Language</asp:Label></li>
                            <li>
                                <asp:TextBox ID="UsernameTextbox" runat="server" autocorrect="off" autocapitalize="none" autocomplete="off"></asp:TextBox>

                                <asp:LinkButton ID="WinAuthChangeUser" Visible="false" Enabled="false" runat="server" OnClick="WinAuthChangeUser_Click">Change User</asp:LinkButton>

                                <asp:DropDownList ID="LanguageDropDown" Visible="false" runat="server" /></li>

                            <li>
                                <div id="passwordAsterisk" class="teal-asterisk" runat="server">*</div>
                                <asp:Label ID="PasswordLabel" runat="server"> Password</asp:Label>
                                <asp:Label ID="TimeZoneLabel" runat="server" Visible="false">Time Zone</asp:Label></li>
                            <li>
                                <asp:TextBox ID="PasswordTextbox" runat="server" autocomplete="new-password" TextMode="Password"></asp:TextBox>
                                <asp:DropDownList ID="TimeZoneDropDown" Visible="false" runat="server" />

                            </li>
                            <li>
                                <div id="domainAsterisk" class="teal-asterisk" runat="server">*</div>
                                <asp:Label ID="DomainLabel" runat="server"> Domain</asp:Label></li>
                            <li>
                                <asp:DropDownList ID="DomainDropDown" runat="server"></asp:DropDownList></li>
                            <li>
                                <div style="display: flex; justify-content: space-between; align-items: center;">
                                    <asp:ImageButton ID="SettingsButton" ImageUrl="/CamstarPortal/Images/Icons/cmdSettings24.svg" Style="margin-top: 16px; margin-left: 10px; filter: brightness(0) invert(1); cursor: pointer; vertical-align: middle;" OnClick="SettingsButton_Click" runat="server" />
                                    <div style="display: flex; align-items: center; gap: 10px">
                                        <div id="SSOLogin" visible="false" runat="server" style="display: flex">
                                            <asp:Button ID="SSOLoginButton" runat="server" Text="Use Single Sign On (SSO)" OnClick="OnSSO_Click" Style="width: 180px; vertical-align: middle;" CssClass="cs-button" />
                                            <span style="color: #fff; vertical-align: middle; margin-left: 8px; margin-top: 26px;">Or</span>
                                        </div>
                                        <asp:Button ID="LoginButton" runat="server" Text="Log In" Style="vertical-align: middle;" OnClick="LoginButton_Click" CssClass="cs-button" />
                                    </div>
                                </div>
                                <asp:Button ID="SaveButton" runat="server" Text="Save" Visible="false" Style="vertical-align: middle;" OnClick="SaveButton_Click" CssClass="cs-button" />
                                <asp:Button ID="CancelButton" runat="server" Text="Cancel" Visible="false" Style="vertical-align: middle;" OnClick="CancelButton_Click" CssClass="cs-button" />
                            </li>
                        </ul>

                        <div class="ui-login-border"></div>
                    </div>
                </div>
            </div>
            <div class="version-details">
                <div class="version-number">Version 2510</div>
                <br>

                <div class="version-text">
                    This material contains trade secrets or otherwise confidential information
                    owned by Siemens Industry Software Inc. or its affiliates (collectively, “Siemens”), or its
                    licensors. Access to and use of this information is strictly limited as set forth in the Customer’s
                    applicable agreements with Siemens.
                </div>
                <div class="version-copyright">
                    Copyright &#169 
                        <asp:Label ID="Year" runat="server" />
                    Siemens
                </div>
            </div>
        </div>
        <%--   </div>--%>
    </form>




</body>
</html>
