// Copyright Siemens 2023  

/// <reference name="MicrosoftAjax.js"/>

window.getViewportHeight = function()
{
    if (window.innerHeight != window.undefined) return window.innerHeight;
    if (document.compatMode == "CSS1Compat") return document.documentElement.clientHeight;
    if (document.body) return document.body.clientHeight;
    return window.undefined;
}
    
window.getViewportWidth = function()
{
    if (window.innerWidth!=window.undefined) return window.innerWidth;
    if (document.compatMode=='CSS1Compat') return document.documentElement.clientWidth;
    if (document.body) return document.body.clientWidth;
    return window.undefined;
}
    
window.getScrollTop = function()
{
    if (window.pageYOffset) return window.pageYOffset;
	if (document.documentElement && document.documentElement.scrollTop) return document.documentElement.scrollTop;
	if (document.body) return document.body.scrollTop;
}
    
window.getScrollLeft = function()
{
    if (window.pageXOffset) return window.pageXOffset;
	if (document.documentElement && document.documentElement.scrollTop) return document.documentElement.scrollLeft;
	if (document.body) return document.body.scrollLeft;
}
