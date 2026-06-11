// Copyright Siemens 2023  

Sys.Application.add_load(function (sender, args)
{
    if (!Sys.WebForms.PageRequestManager.getInstance().get_isInAsyncPostBack())
    {
        Sys.WebForms.PageRequestManager.getInstance().add_initializeRequest(function(sender, args) {
		    $get("mod").style.display="block";
		    $get("modImage").style.display="block";
        });
        Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(function(sender, args) {
            $get("mod").style.display="none";
	        $get("modImage").style.display="none";
        });
    }
});
