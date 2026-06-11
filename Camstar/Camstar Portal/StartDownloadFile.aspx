<%-- Copyright Siemens 2023   --%>
<%@ Page Language="C#" AutoEventWireup="true" CodeFile="StartDownloadFile.aspx.cs" Inherits="StartDownloadFile" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
        <script language="javascript" type="text/javascript">
        function DownloadFile()
        {
            var iframe = document.createElement("iframe");
            
            // Point the IFRAME to GenerateFile, with the
            //   desired region as a querystring argument.
            iframe.src = "DownloadFile.aspx?Name=" + Name + "&Version=" + Version + "&AttachmentsID=" + AttachmentsID;

            // This makes the IFRAME invisible to the user.
            iframe.style.visibility = "hidden";

            // Add the IFRAME to the page.  This will trigger
            //   a request to GenerateFile now.
            document.body.appendChild(iframe);
        }
        
    </script>

</head>
<body onload="DownloadFile();">
    <form id="form1" runat="server">
    <div>
    
    </div>

    </form>
</body>
</html>
