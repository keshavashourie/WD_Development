function navigateToLanguageFolder(userLang)
{
	var defaultLang = "EN";	// EN language is default.
	var currentUrl = window.location.href;	
	var page = window.location.pathname.split("/").pop(); // requested page name.
	
	var defaultUrl = currentUrl;//currentUrl.replace(page, defaultLang + "/" + page);
	var xhttp;
	if (window.XMLHttpRequest)
		xhttp = new XMLHttpRequest();
	else
		xhttp = new ActiveXObject("Microsoft.XMLHTTP");
	
	if (!userLang)
	{		
		try
		{
			xhttp.open('HEAD', "../UserLanguage.aspx", false);
			xhttp.send(null);
			userLang = xhttp.getResponseHeader("UserLanguage");
		}
		catch(e) {}
		if (!userLang)
		{
			var langCode = navigator.languages ? navigator.languages[0] : (navigator.language || navigator.userLanguage);
			userLang = langCode.toUpperCase().substr(0, 2);	// browser's language: EN, DE, ZH, etc.
		}			
	}
	if (userLang == "DE" || userLang == "ZH") // temp fix for Management Studio.
	//if (userLang != defaultLang)
	{
		var langSpecificUrl = currentUrl.replace(page, userLang + "/" + page);
		xhttp.onreadystatechange = function() {
			if (xhttp.readyState == XMLHttpRequest.DONE)
			{
				var isChrome = !!window.chrome && !!window.chrome.webstore;
				if (xhttp.status == 200)
					window.location.replace ? window.location.replace(langSpecificUrl) : window.location = langSpecificUrl;
				else if (xhttp.status == 0 && isChrome) // for Chrome, Management Studio.
					window.location.replace ? window.location.replace(langSpecificUrl) : window.location = langSpecificUrl;
				//else
					// if user language folder doesn't exist - navigate to default language folder.   
				//	window.location.replace ? window.location.replace(defaultUrl) : window.location = defaultUrl;
			}
		}
		try
		{
			var urlToCheck = langSpecificUrl;			
			var urlParts = langSpecificUrl.split('#');
			if (urlParts.length > 1)
			{
				var urlFirstPart = urlParts[0];
				var urlSecondPart = urlParts[1];
				var urlQueries = urlSecondPart.split('='); // remove query part. cshid=ModelingObjects/Defining_Bills_of_Process_P.htm
				if (urlQueries.length > 1)
					urlSecondPart = urlQueries[1];				
				urlToCheck = urlToCheck.substr(0, urlFirstPart.lastIndexOf("/") + 1) + "Content/" + urlSecondPart;		
			}
			xhttp.open("HEAD", urlToCheck, true); // check whether user language folder exists or not.
			xhttp.send();
		}
		catch(e)
		{
			window.location.replace ? window.location.replace(langSpecificUrl) : window.location = langSpecificUrl;
		}
	}
	//else
	//	window.location.replace ? window.location.replace(defaultUrl) : window.location = defaultUrl;
}