<%@ Page Language="C#" debug="false" trace="false"%>
<html>
    <head>
        <title></title>
        <script runat="server">
            private void Page_Load()
            {
                string[] languages = HttpContext.Current.Request.UserLanguages;
                if (languages != null && languages.Length > 0)
                {
                    var userLang = languages[0].ToUpper().Substring(0, 2);
                    Response.AddHeader("UserLanguage", userLang);
                    Response.End();
                }
            }
        </script>
    </head>
    <body></body>
</html>