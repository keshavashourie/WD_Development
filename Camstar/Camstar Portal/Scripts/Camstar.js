// Copyright Siemens 2025 

var mkUserFieldsDiv = "userFieldsDiv";
var mkParametricDiv = "parametricDiv";
var mkValidateFunctionName = "OnDCDFieldValueChanged";
var mDisabledKeyAlert = "This key has been disabled.";
var gkBackSpaceKeyCode = 8;
var gkKeyCode = "";
var isSSO = false;
var isSam = false;
var isIplUrlSet = false;

// The control name where current scroll position are saver on post back
var	mPositionInputName = "__position";

var Camstars =
{
    Browser: 
    {
        IE:     /*@cc_on!@*/false || !!document.documentMode,
        Opera:  !!window.opera,
        WebKit: navigator.userAgent.indexOf('AppleWebKit/') > -1,
        Gecko:  navigator.userAgent.indexOf('Gecko') > -1 && navigator.userAgent.indexOf('KHTML') == -1,
        MobileSafari: !!navigator.userAgent.match(/Apple.*Mobile.*Safari/),
        FireFox: !document.all,
        IsMobile: /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent)
    },
    KeyCodes:
    {
        Backspace: 8,
        Tab: 9,
        Enter: 13
    }
};

Camstars.Object = 
{
    extend: function(destination, source)
    {
        if (arguments.length == 1)
        {
            source = this;
        }
        
        for (var property in source)
        {
            
            if (this.isFunction(source[property]) && source[property].arguments && source[property].arguments.length == 1)
            {
                destination[property] = source[property](destination);
            }
            else
            {
                destination[property] = source[property];
            }
        }
    
        return destination;
    },
    
    isString: function(object)
    {
        return typeof object == "string";
    },
    
    isElement: function(object)
    {
        return object && object.nodeType == 1;
    },
   
    isUndefined: function(object)
    {
        return typeof object == "undefined";
    },
    
    isFunction: function(object)
    {
        return typeof object == "function";
    }
};

Camstars.Event = 
{
    extend: function(destination)
    {
       destination.addEvent = function(eventName, wrapper){ Camstars.Event.addEvent(this, eventName, wrapper); };
       destination.removeEvent = function(eventName, wrapper){ Camstars.Event.removeEvent(this, eventName, wrapper); };
    },
    
    addEvent: function(element, eventName, wrapper)
    {
        if (element.addEventListener)
        {
            return element.addEventListener(eventName, wrapper, false);
        }
        else
        {
            return element.attachEvent(eventName.indexOf("on") > -1 ? eventName : "on" + eventName, wrapper);
        }
    },
    
    removeEvent: function(element, eventName, wrapper)
    {
        if (element.removeEventListener)
        {
            return element.removeEventListener(eventName, wrapper, false);
        }
        else
        {
            return element.detachEvent(eventName.indexOf("on") > -1 ? eventName : "on" + eventName, wrapper);
        }
    }
};


Camstars.Controls =
{
    getValueById: function (editorClientId)
    {
        var ctl = $find(editorClientId);
        var val = "";
        if (ctl)
        {
            if (ctl.getValue != undefined)
            {
                val = ctl.getValue();
            }
            else if (ctl._element != undefined)
            {
                if (ctl._element.value != undefined)
                    val = ctl._element.value;
                else
                {
                    //some container element. Check for file uploader
                    var elemFile = $("#" + editorClientId).find("input[type='text']");
                    if (elemFile.length) {
                        val = elemFile.val();
                        if (val.lastIndexOf("\\") != -1)
                            val = val.substring(val.lastIndexOf("\\") + 1);
                    }
                }
            }
        }
        else
        {
            // Try jquery object
            ctl = $("#" + editorClientId);
            if (ctl && ctl.length > 0) {
                if (ctl.find("input[type='file']").length > 0)
                {
                    val = ctl.find("input[type='file']").val();
                    // leave only file name
                    if (val.lastIndexOf("\\") != -1)
                        val = val.substring(val.lastIndexOf("\\") + 1);
                }
                else
                {
                    val = ctl.val();
                    if (val)
                    {
                        if (ctl.is(':checkbox'))
                        {
                            val = ctl.is(':checked');
                        }
                    }
                    else if (ctl.length == 1 && ctl[0].tagName === "SPAN")
                    {
                        //deals with wrapped elements
                        ctl = ctl.find("input");
                        if (ctl && ctl.attr("type") === "radio")
                            val = ctl[0].checked.toString();
                    }
                }
            }
        }
        return val;
    },

    setValueById: function (editorClientId, value, takeDefault)
    {
        var ctl = $find(editorClientId);
        if (ctl)
        {
            if (takeDefault && ctl.get_defaultValue && (value === undefined || value == null || value === '')) {
                value = ctl.get_defaultValue() || value;
            }

            if (ctl.directUpdate != undefined && ctl.GridID == undefined)
            {
                ctl.directUpdate({ PropertyKey: eval(Camstar.Ajax.DirectUpdateParameterKeys.Data), PropertyValue: value });
            }
            else
            {
                if (ctl.setValue != undefined)
                    ctl.setValue(value);
            }
        }
        else
        {
            ctl = $("#" + editorClientId);
            if (ctl && ctl.length > 0)
            {
                if (takeDefault && (value === undefined || value == null || value === '')) {
                    value = ctl.attr("defaultValue") || ctl.parent().attr("defaultValue") || value;
                }

                if (ctl[0].type == "checkbox")
                    ctl[0].checked = (value == "True" || value == 1);
                else
                    ctl.val(value);
            }
        }
    }
}

Camstars.Object.extend(String.prototype, {
    empty: function()
    {
        return this == "";
    }
});

Camstars.Object.extend(window, {
    getViewportHeight: function()
    {
        if (window.innerHeight != window.undefined) return window.innerHeight;
	    if (document.compatMode == "CSS1Compat") return document.documentElement.clientHeight;
	    if (document.body) return document.body.clientHeight;
	    return window.undefined;
    },
    
    getViewportWidth: function()
    {
        if (window.innerWidth!=window.undefined) return window.innerWidth;
	    if (document.compatMode=='CSS1Compat') return document.documentElement.clientWidth;
	    if (document.body) return document.body.clientWidth;
	    return window.undefined;
    },
    
    getScrollTop: function()
    {
        if (window.pageYOffset) return window.pageYOffset;
		if (document.documentElement && document.documentElement.scrollTop) return document.documentElement.scrollTop;
		if (document.body) return document.body.scrollTop;
    },
    
    getScrollLeft: function()
    {
        if (window.pageXOffset) return window.pageXOffset;
		if (document.documentElement && document.documentElement.scrollTop) return document.documentElement.scrollLeft;
		if (document.body) return document.body.scrollLeft;
    }
});

function loadPortalMenu(sso) {
    let jsonItem = sessionStorage.getItem("menuItems");
    var loginMethod = document.getElementById("LoginMethod");
    isSSO = typeof sso !== 'undefined' && sso.toLowerCase() === 'true';
    isSam = loginMethod.value === "SAM" ? true : false;
    if (!jsonItem) {
        $.ajax({
            type: "POST",
            dataType: "json",
            url: './ApolloPortalService.svc/web/GetMenuItems',
            headers: {
                'Accept': 'application/json'
            },
            contentType: "application/json;charset=UTF-8",
            async: true,
            context: document.body
        }).success(parseSuccess).fail(loadMenuFail);
    } else
        renderPrimaryMenu();
    function parseSuccess(response) {
        if (response.GetMenuItemsResult.IsSuccess) {
            var menuItems = response.menuItems;
            sessionStorage.setItem("menuItems", JSON.stringify(menuItems));
            renderPrimaryMenu();
        }
    }
    function loadMenuFail(e) {
        //window.top.location = 'default.aspx';
    }
}
function showSecondaryMenu(liElement) {
	let ulElement = liElement.parentElement;
	$('#secondary-navigation').show();			
	liElement.classList.add("open");
	ulElement.classList.add("open");
	document.getElementById("secondary-navigation").classList.add("open");
    document.getElementById("secondary-nav-menu-pin").classList.add("open");

    setupSecondaryMenuResize();
}

function setupSecondaryMenuResize() {
    $('#secondary-navigation #button-resize').on('mousedown', function (e) {


        let onMouseMove = (e) => {
            var pageX = e.pageX || e.touches[0].pageX;
            //Update the sidenav width
            var x = pageX - $('#secondary-navigation').offset().left;
            if (x >= 180 && x <= 280) {
                $('#secondary-navigation').css('width', x);
                // This is required to update the max width of links,
                // which was previously done with fixed values in CSS
                $('#secondary-navigation').find('a').each((index, element) => {
                    element.style.maxWidth = x - 44 + 'px';
                });
            }
        };

        let removeEventListeners = (mouseEvent) => {
            document.removeEventListener('mousemove', onMouseMove);
            document.removeEventListener('mouseup', removeEventListeners);
            document.removeEventListener('touchmove', onMouseMove);
            document.removeEventListener('touchend', removeEventListeners);
        }

        document.addEventListener('mousemove', onMouseMove);
        document.addEventListener('touchmove', onMouseMove);
        document.addEventListener('mouseup', removeEventListeners);
        document.addEventListener('touchend', removeEventListeners);

    });
}
function menuItem_Click(id) {
    var liElement = id.parentElement;
    var ulElement = liElement.parentElement;
    if (document.getElementById("primary-navigation-more").classList.contains("open")) {
        document.getElementById("primary-navigation-more").classList.remove("open");
        //setting tab index
        var index = Number(sessionStorage.getItem("currentIndex"));
        settingTabIndex(index);
    }
    
    if (liElement.classList.contains("open")) {
        //document.getElementById("header-title").innerText = "";
		liElement.classList.remove("open");
        closeNavigationPanel();
    }
    else {
		let elem = $(liElement);
		if (!elem.hasClass("mainnav-list"))
			elem.addClass("mainnav-list");
        let i = 0;
        //	First close any sub-menu already opened
		var elm = document.getElementsByClassName("subnav-list open");
        for (let i = elm.length-1; i >= 0; i--) {
            if (liElement.id != elm[i].id) {
                elm[i].classList.remove("open");
            }
        }
		//	Now remove open class from primary menu item
		elm = ulElement.getElementsByClassName("open");
        for (let i = elm.length-1; i >= 0; i--) {
            if (liElement.id != elm[i].id) {
                elm[i].classList.remove("open");
            }
        }

		let jsonItem = sessionStorage.getItem("menuItems");
		if (jsonItem)
		{
			var menuItems = JSON.parse(jsonItem);
			var item;
			menuItems.forEach(function (menuitem) {
				if (id.lastChild.innerHTML === menuitem.DisplayName) {
					item = menuitem;
				}
			});
			if (item)
				sessionStorage.setItem("subMenuItem", JSON.stringify(item.Children));
			renderMenu();
			showSecondaryMenu(liElement);
		} else
			$.ajax({
				type: "POST",
				dataType: "json",
				url: './ApolloPortalService.svc/web/GetMenuItems',
				headers: {
					'Accept': 'application/json'
				},
				contentType: "application/json;charset=UTF-8",
				async: true,
				context: document.body
			}).success(parseSuccess).fail(false);
		
        function parseSuccess(response) {
            if (response.GetMenuItemsResult.IsSuccess) {
                var menuItems = response.menuItems;
				sessionStorage.setItem("menuItems", JSON.stringify(menuItems));
				renderMenu();
				showSecondaryMenu(liElement);
            }
        }

		function renderMenu() {
			var jsonItem = sessionStorage.getItem("menuItems");
			var menuItems = JSON.parse(jsonItem);
			var item;
			menuItems.forEach(function (menuitem) {
				if (id.lastChild.innerHTML === menuitem.DisplayName) {
					item = menuitem;
				}
			});
			let menuId = 'secondary-nav-menu-' + item.Id;
			let exist = document.getElementById(menuId);
            var li = document.getElementById('subnav-title');
            li.innerHTML = item.DisplayName;
            var element = document.getElementById("secondary-navigation");
            doPinMenu(element.classList.contains("pinned"));
			if (exist) {
                $(exist).addClass("open");
                return;
			}
            if (item.Children.length > 0) {
				sessionStorage.setItem("subMenuItem", JSON.stringify(item.Children));
				var ol = document.createElement('ol');
				ol.setAttribute('id', menuId);
				ol.setAttribute('menuId', item.Id);
				ol.setAttribute('class', 'subnav-list open');
				for (let i = 0; i < item.Children.length; i++) {
					var subMenuItem = item.Children[i];

					var el = document.createElement('li');
					el.setAttribute("id", "secondary-nav-submenu-" + subMenuItem.Id);
					var anchor = document.createElement("a");
					anchor.setAttribute("id", "anchor-nav-submenu"  + subMenuItem.Id);
                    anchor.setAttribute("data-submenu", subMenuItem.DisplayName);
                    anchor.setAttribute("title", subMenuItem.DisplayName);
                    var index = menuItems.length + 6 + i;
                    anchor.setAttribute("tabindex", index);

                    var span = document.createElement("span");
                    span.innerHTML = subMenuItem.DisplayName;
                    anchor.appendChild(span);

                    if (subMenuItem.Children.length > 0) {
                        var a = document.createElement("a");
                        a.setAttribute("onClick", "expandMenu_Click(this)");
                        a.setAttribute("class", "button-chromeless-reverse button-icon-only");
                        //a.setAttribute("tabindex", index);
                        a.addEventListener("keydown", function (event) {
                            if (event.keyCode === 13) {
                                var indx = this.tabIndex;
                               expandMenu_Click(this,false,index);                                
                            }
                        });
                        a.setAttribute("id", "expandIcon");
                        var img = document.createElement("img");
                        img.setAttribute("src", "/CamstarPortal/assets/image/miscRightArrow16.svg");
                        img.setAttribute("style", "height:16px; width:16px; -webkit-filter: invert(100%);");
                        img.setAttribute("fill", "#464646");
                        a.appendChild(img);
                        el.appendChild(a);

                        anchor.setAttribute("onClick", "expandMenu_Click(this, true, " + index + ")");
                        anchor.addEventListener("keydown", function (event) {
                            if (event.keyCode === 13) {
                                var index = this.tabIndex;
                                expandMenu_Click(this, true, index);
                            }
                        });
                    } else {
                        anchor.setAttribute("class", "app-menu-link");
                        anchor.setAttribute("onClick", "subMenu_Click(this.id)")
                        span.setAttribute("class", "app-menu-link");
                        anchor.addEventListener("keydown", function (event) {
                            if (event.keyCode === 13) {
                                subMenu_Click(this.id);                                
                            }
                        });
                    }

					el.appendChild(anchor);
					ol.appendChild(el);		 
			  	}
                document.getElementById("secondary-navigation").appendChild(ol);
               
			}
		} 
	}
}

function subMenu_Click(id) {
    closeNavigationPanel();
    var menuID = id.length > 16 ? id.substr(id.length - 16) : '';   //  if possible we will use the ID to identify the menu item
    var itemName = menuID ? menuID : document.getElementById(id).innerText;
    var item = sessionStorage.getItem("subMenuItem");
    var jsonItem = JSON.parse(item);
    var subMenu;
    for (let i = 0; i < jsonItem.length; i++) {
        if (itemName == jsonItem[i].DisplayName)
            subMenu = jsonItem[i];
    }
    let shouldSkip = false;
    jsonItem.forEach((item) => {
        if (!shouldSkip) {
            if (menuID) {
                if (item.Id === menuID) {
                    subMenu = item;
                    shouldSkip = true;
                    return;
                }
                else {
                    item.Children.forEach((i) => {
                        if (menuID == i.Id) {
                            subMenu = i;
                            shouldSkip = true;
                            return;
                        }
                        else {
                            i.Children.forEach((itm) => {
                                if (menuID == itm.Id) {
                                    subMenu = itm;
                                    shouldSkip = true;
                                    return;
                                }
                            });
                        }
                    })
                }
            }
            else if (itemName == item.DisplayName) {
                subMenu = item;
                shouldSkip = true;
                return;
            }
            else {
                item.Children.forEach((i) => {
                    if (itemName == i.DisplayName) {
                        subMenu = i;
                        shouldSkip = true;
                        return;
                    }
                    else {
                        i.Children.forEach((itm) => {
                            if (itemName == itm.DisplayName) {
                                subMenu = itm;
                                shouldSkip = true;
                                return;
                            }                      
                        });
                    }
                })
            }
               
        }
    });
    userInteraction(subMenu);
}

function findChildMenu(item, itemName) {
    var childMenu = null;
    if (item) {
        item.Children.some((i) => {
            if (itemName == i.DisplayName) {
                childMenu = i;
                return true;
            } else if (i.Children) {
                i.Children.some((j) => {
                    if (itemName == j.DisplayName) {
                        childMenu = j;
                        return true;
                    } else if (!childMenu) {
                        childMenu = findChildMenu(j, itemName);
                        if (childMenu != null)
                            return true;
                    }
                });
            }        
        });
    }
    return childMenu;
}
function expandMenu_Click(elem, getPrev,index) {
    var img = getPrev ? $(elem).prev()[0].childNodes[0] : elem.childNodes[0];
    if (!(img.classList.contains("expandMenu"))) {

        img.classList.add("expandMenu");
        var jsonItem = sessionStorage.getItem("subMenuItem");
        var MenuItems = JSON.parse(jsonItem);
        var itemName = elem.parentElement.innerText;
        var childMenuItems;
        let shouldSkip = false;
		let id = "";
        MenuItems.forEach((item) => {
            if (!shouldSkip) {
                if (itemName == item.DisplayName) {
                    childMenuItems = item.Children;
                    shouldSkip = true;
					id = item.Id;
                    return;
                }
                else {
                    var childMenu = findChildMenu(item, itemName);
                    if (childMenu) {
                        childMenuItems = childMenu.Children;
                        shouldSkip = true;
                        id = childMenu.Id;
                        return;
                    }
                }
            }
        });
		elem.parentElement.setAttribute("divSubId", 'divSub'+id);
		let exist = $('#divSub'+id);
		if (exist.length) {
			exist.show();
			return;
		}
		var div = document.createElement('div');
		div.setAttribute("id", "divSub"+id);
		var ol = document.createElement('ol');
		ol.setAttribute("style", "padding-left: 4px");
        //for (let i = childMenuItems.length - 1; i >= 0; i--) {
		for (let i = 0; i < childMenuItems.length; i++) {	
			let item = childMenuItems[i];
            var el = document.createElement('li');
            el.setAttribute("id", "secondary-nav-" + elem.parentElement.innerText.replace(/\s/g, '') + '-' + item.Id);
            el.setAttribute("style", "padding-left: 8px");
            var anchor = document.createElement("a");
            anchor.setAttribute("id", "anchor-nav-"+ elem.parentElement.innerText.replace(/\s/g, '') + '-' + item.Id);
            anchor.setAttribute("title", item.DisplayName);
            anchor.setAttribute("data-submenu", item.DisplayName);
            anchor.setAttribute("tabindex", index +((i+1)*0.1));
            
            var span = document.createElement("span");
            span.innerHTML = item.DisplayName;
            anchor.appendChild(span);

            if (item.Children.length > 0) {
                var a = document.createElement("a");
                a.setAttribute("onClick", "expandMenu_Click(this)");
                a.setAttribute("class", "button-chromeless-reverse button-icon-only");
                a.setAttribute("id", "expandIcon");
                a.addEventListener("keydown", function (event) {
                    if (event.keyCode === 13) {
                        expandMenu_Click(this,false, index+((i+1)*0.1));
                    }
                });
                var img = document.createElement("img");
                img.setAttribute("src", "/CamstarPortal/assets/image/miscRightArrow16.svg");
                img.setAttribute("style", "height:16px; width:16px; -webkit-filter: invert(100%);");
                img.setAttribute("fill", "#464646");
                a.appendChild(img);
                el.appendChild(a);

                anchor.setAttribute("onClick", "expandMenu_Click(this, true)");
                anchor.addEventListener("keydown", function (event) {
                    if (event.keyCode === 13) {
                          expandMenu_Click(this, true,index +((i+1)*0.1));                     
                    }
                });
            } else {
                anchor.setAttribute("class", "app-menu-link");
                anchor.setAttribute("onClick", "subMenu_Click(this.id)");
                anchor.addEventListener("keydown", function (event) {
                    if (event.keyCode === 13) {
                        subMenu_Click(this.id);                       
                    }
                });
                span.setAttribute("class", "app-menu-link");
            }


            el.appendChild(anchor);
            ol.appendChild(el);
        }
		div.appendChild(ol);
        elem.parentElement.appendChild(div);
        
       
    }
    else {
        img.classList.remove("expandMenu")
        img.classList.add("collapseMenu");
		let id = elem.parentElement.getAttribute("divSubId");
		let exist = $('#'+id);
		if (exist.length) {
			exist.hide();
			return;
		}
    }
}


function closeNavigationPanel() {
	let panel = $('#secondary-navigation');
    if (panel.hasClass("pinned"))
        return;
	panel.removeClass('open');
	panel = $('#ctl00_primaryNavMenu');	
	panel.removeClass('open');
    document.getElementById("primary-navigation-more").classList.remove("open");
    var hidden = document.getElementsByClassName("subnav-list open");
    for (var i = hidden.length - 1; i > -1; i--) {
        hidden[i].classList.remove("open");
    }	
    hidden = document.getElementsByClassName("mainnav-list open");
    for (var i = hidden.length - 1; i > -1; i--) {
        hidden[i].classList.remove("open");
    }	
	
    var liElement = document.getElementById("nav-item-avatar")
    var ulElement = liElement.parentElement;
	liElement.classList.remove("open");
	ulElement.classList.remove("open");
	
    liElement = document.getElementById("nav-item-settings")
    ulElement = liElement.parentElement;
	liElement.classList.remove("open");
	ulElement.classList.remove("open");

    doPinMenu(false);
}

function pin_click() {
    var element = document.getElementById("secondary-navigation");
    doPinMenu(!element.classList.contains("pinned"));
}
function doPinMenu(pin) {
    let width = $('#secondary-navigation').width() + $('#secondary-navigation').position().left +10;
    if (pin) {
        document.getElementById("secondary-navigation").classList.add("pinned");
        $('#secondary-navigation.pinned ~ main').css('margin-left', width + 'px');
        $('#secondary-navigation.pinned ~ #app-header').css('margin-left', width + 'px');
    } else {
        $('#secondary-navigation.pinned ~ main').css('margin-left', '');
        $('#secondary-navigation.pinned ~ #app-header').css('margin-left', '');
        document.getElementById("secondary-navigation").classList.remove("pinned");
    }
}
function setElementDisplay(element, display) {
    if (element) {
        if (display)
            element.setAttribute("style", "display:block");
        else
            element.style.display = null;
    }
}

function more_click() {
    //	First close any sub-menu already opened
    var elm = document.getElementsByClassName("subnav-list open");
    for (let i = elm.length - 1; i >= 0; i--) {
        elm[i].classList.remove("open");
    }
    var elm = document.getElementsByClassName("mainnav-list open");
    for (let i = elm.length - 1; i >= 0; i--) {
        elm[i].classList.remove("open");
    }
    elm = $('#ctl00_primaryNavMenu');
    elm.removeClass('open');
    $('#secondary-navigation').hide();
    document.getElementById("secondary-navigation").classList.remove("open");

    let more = document.getElementById("primary-navigation-more");
    if (more.classList.contains("open")) {
        more.classList.remove("open");
        var index = Number(sessionStorage.getItem("currentIndex"));
        settingTabIndex(index);
        return;
    }
    more.classList.add("open");

    //setting tab index
    let jsonItem = sessionStorage.getItem("menuItems");
    var menuItems = JSON.parse(jsonItem);
    settingTabIndex(menuItems.length);
   
}

function settingTabIndex(tabindx) {
    var menu = $('.primaryNavMenu');
    
    //  Help
    menu[0].removeChild(document.getElementById('nav-item-help'));
    addStandardItem(menu, 'help', tabindx + 2, 'closeNavigationPanel();__page.openHelpframe("' + noHelpText + '");', helpText, 'cmdHelp16.svg');

    if (isIplUrlSet) {
        menu[0].removeChild(document.getElementById('nav-item-ipl'));
        addStandardItem(menu, 'ipl', tabindx + 3, '__page.openIPLwindow();', "IPL", 'cmdMaterialRequest24.svg');
    }

    // Mendix Apps - Add them after IPL
    if (mendixSettings && mendixSettings.urls) {
        // Mendix Core
        if (mendixSettings.urls.Core) {
            menu[0].removeChild(document.getElementById('nav-item-mx-core'));
            addStandardItem(menu, 'mx-core', tabindx + 4, '__page.openMendixWindow("Core")', "MX Core", 'cmdOpenInNewTab24.svg');
        }
        // Mendix Semiconductor
        if (mendixSettings.urls.Semiconductor) {
            menu[0].removeChild(document.getElementById('nav-item-mx-semi'));
            addStandardItem(menu, 'mx-semi', tabindx + 5, '__page.openMendixWindow("Semiconductor")', "MX Semi", 'cmdOpenInNewTab24.svg');
        }		
		// Mendix Electronics
        if (mendixSettings.urls.Electronics) {
            menu[0].removeChild(document.getElementById('nav-item-mx-elec'));
            addStandardItem(menu, 'mx-elec', tabindx + 6, '__page.openMendixWindow("Electronics")', "MX Elec", 'cmdOpenInNewTab24.svg');
        }
    }
    let chatUrl = document.getElementById("plChatUrl");
    if (isSSO && isSam) {
        if (chatUrl) {
            //plchat 
            menu[0].removeChild(document.getElementById('nav-item-plchat'));
            addStandardItem(menu, 'plchat', tabindx + 7, 'openPlchat()', plChatText, 'cmdUIApplication24.svg');
        }
        // Portal Studio
        menu[0].removeChild(document.getElementById('nav-item-alerts'));
        addStandardItem(menu, 'alerts', tabindx + 8, 'openPortalStudio(2)', portalStudioText, 'cmdUIApplication24.svg');

        //  Settings
        menu[0].removeChild(document.getElementById('nav-item-settings'));
        addStandardItem(menu, 'settings', tabindx + 9, 'settings_click()', settingsText, 'cmdSettings24.svg');
    }
    else {
        // Portal Studio
        menu[0].removeChild(document.getElementById('nav-item-alerts'));
        addStandardItem(menu, 'alerts', tabindx + 7, 'openPortalStudio(2)', portalStudioText, 'cmdUIApplication24.svg');

        //  Settings
        menu[0].removeChild(document.getElementById('nav-item-settings'));
        addStandardItem(menu, 'settings', tabindx + 8, 'settings_click()', settingsText, 'cmdSettings24.svg');
    }

    //user profile 
    menu[0].removeChild(document.getElementById('nav-item-avatar'));
    //  User Profile
    var li = document.createElement('li');
    li.setAttribute("id", "nav-item-avatar");
    var anchor = document.createElement("a");
    anchor.setAttribute("id", "nav-anchor-user");
    var tabindex =tabindx + 6;
    anchor.setAttribute("tabindex", tabindex.toString());
    anchor.setAttribute("onclick", "usr_click();");
    anchor.addEventListener("keydown", function (event) {
        if (event.keyCode === 13) {
            document.activeElement.blur();
            usr_click();
        }
    });
    var span = document.createElement("span");
    span.setAttribute("id", "nav-item-avatar-link");
    let html = '<svg id="Artwork" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24"><g><path d="M12,10.5c2.2,0,4-2.9,4-5.3A4.122,4.122,0,0,0,12,1,4.122,4.122,0,0,0,8,5.2C8,7.6,9.8,10.5,12,10.5ZM12,2a3.057,3.057,0,0,1,3,3.2c0,1.9-1.5,4.3-3,4.3S9,7.1,9,5.2A3.057,3.057,0,0,1,12,2Z" fill="#464646"></path><path d="M16.7,11.1l-.3-.1-.2.2a7.208,7.208,0,0,1-8.4,0L7.6,11l-.3.1A5.515,5.515,0,0,0,4,15.9V24H20V15.9A5.4,5.4,0,0,0,16.7,11.1ZM19,23H17V17H16v6H8V17H7v6H5V15.9a4.324,4.324,0,0,1,2.482-3.8,7.874,7.874,0,0,0,9.036,0A4.515,4.515,0,0,1,19,15.9Z" class="aw-theme-iconOutline" fill="#464646"></path></g></svg>';
    span.innerHTML = html;
    anchor.appendChild(span);
    li.appendChild(anchor);
    menu.append(li);
}


var _menuLabels = {};
var helpText = "*Help";
var portalStudioText = "*Portal Studio";
var settingsText = "*Settings";
var moreText = "More";
var backText = "Back";
var homePageText = "Home Page";
var plChatText = "Opcenter Chat";

var noHelpText = "*Online help is currently being developed and will be deployed in a future release.";
var _menuResize = null;

function renderPrimaryMenu(isResize) {
    let jsonItem = sessionStorage.getItem("menuItems");
    if (jsonItem) {
        var menuItems = JSON.parse(jsonItem);
        var menu = $('.primaryNavMenu');
        if (menu.length) {
            var labels = [{ Name: 'BackButton' }, { Name: 'HomePageLbl' }, { Name: 'Banner_Help' }, { Name: 'Banner_Studio' }, { Name: 'Banner_Settings' }, { Name: 'Lbl_NoHelpFileMessage' }, { Name: 'Lbl_MenuMore' }];
            backText = sessionStorage.getItem("BackButton");
            homePageText = sessionStorage.getItem("HomePageLbl");
            helpText = sessionStorage.getItem("Banner_Help");
            portalStudioText = sessionStorage.getItem("Banner_Studio");
            settingsText = sessionStorage.getItem("Banner_Settings");
            noHelpText = sessionStorage.getItem("Lbl_NoHelpFileMessage");
            moreText = sessionStorage.getItem("Lbl_MenuMore");
            var __page = $find("__Page");
            if (typeof __page !== 'undefined' && __page && !backText)
                __page.getLabels(labels, function (response) {
                    if ($.isArray(response)) {
                        $.each(response, function () {
                            var labelName = this.Name;
                            var labelText = this.Value;
                            _menuLabels[this.Name] = this.Value;
                            switch (labelName) {
                                case 'BackButton':
                                    backText = labelText;
                                    sessionStorage.setItem('BackButton', labelText);
                                    break;
                                case 'HomePageLbl':
                                    homePageText = labelText;
                                    sessionStorage.setItem('HomePageLbl', labelText);
                                    break;
                                case 'Banner_Help':
                                    helpText = labelText;
                                    sessionStorage.setItem('Banner_Help', labelText);
                                    break;
                                case 'Banner_Studio':
                                    portalStudioText = labelText;
                                    sessionStorage.setItem('Banner_Studio', labelText);
                                    break;
                                case 'Banner_Settings':
                                    settingsText = labelText;
                                    sessionStorage.setItem('Banner_Settings', labelText);
                                    break;
                                case 'Lbl_NoHelpFileMessage':
                                    noHelpText = labelText;
                                    sessionStorage.setItem('Lbl_NoHelpFileMessage', labelText);
                                    break;
                                case 'Lbl_MenuMore':
                                    moreText = labelText;
                                    sessionStorage.setItem('Lbl_MenuMore', labelText);
                                    break;
                                default:
                                    break;
                            }
                        });
                    }
                    finishRender();
                });
            else
                finishRender();

            function calMenuBottomHeight() {
				let baseHeight = 290; // Base height without any additional menu
				let additionalHeight = 0;

				// Check for IPL
				if (isIplUrlSet) {
					additionalHeight += 50;
				}

				// Check for Mendix apps
				if (mendixSettings && mendixSettings.urls) {
					if (mendixSettings.urls.Core) {
						additionalHeight += 50;
					}
					if (mendixSettings.urls.Semiconductor) {
						additionalHeight += 50;
					}
					if (mendixSettings.urls.Electronics) {
						additionalHeight += 50;
					}
                }
                // check for plchat
                let chatUrl = document.getElementById("plChatUrl");
                if (isSSO && isSam && chatUrl) {
                    additionalHeight += 50;
                }
				return baseHeight + additionalHeight;
			}

            function finishRender() {
                isIplUrlSet = IsIPlUrlSet();
                //let menuBottomHeight = isIplUrlSet ? 340 : 290;
				let menuBottomHeight = calMenuBottomHeight();
                let height = menu.height();
                var needMore = false;
                let top = 210;  //  Icon, Back, Home
                let more = 36;
                let bottom = menuBottomHeight;   //  Help, IPL, Portal Studio, Settings, User Profile
                let curHeight = 0;
                let availHeight = height - top - bottom - more;
                let haveMore = $('#ctl00_navItemMore').length > 0;
                let displayCount = 0;
                let curDisplayCount = 0;
                if (backText)
                    $('#span-item-app-back').text(backText);
                if (homePageText)
                    $('#span-item-app-home').text(homePageText);
                $('#primaryNavMenuMore').html("");
                if (isResize) {
                    for (var i = 0; i < menuItems.length; i++) {
                        curHeight += 60;
                        let item = menuItems[i];
                        if (item.IsHomePage) {
                            sessionStorage.setItem("homepage", JSON.stringify(item));
                            continue;
                        }
                        if (item.IsInMenu)
                            curDisplayCount++;
                        if (curHeight > availHeight) {
                            needMore = true;
                        } else
                            displayCount++;
                    }
                    if (displayCount == curDisplayCount && ((haveMore && needMore) || (!haveMore && !needMore)))
                        return;
                    else {
                        for (let i = menu[0].children.length - 1; i > 2; i--) {
                            let child = menu[0].children[i];
                            menu[0].removeChild(child);
                        }
                    }
                }
            
                curHeight = 0;
                needMore = false;
                var currentIndex = 1;
                var menuMore = $('#primaryNavMenuMore');
                for (var i = 0; i < menuItems.length; i++) {
                    let id = i + 1;
                    let item = menuItems[i];
                    if (item.IsHomePage) {
                        sessionStorage.setItem("homepage", JSON.stringify(item));
                        continue;
                    }
                    if (!needMore) {
                        item.IsInMenu = true;
                        addMenuItem(menu, item, id);
                        let newItem = $('#nav-item-' + id);
                        if (newItem)
                            curHeight += newItem.height();
                        if (curHeight > availHeight && i < menuItems.length-1) {
                            needMore = true;
                        }
                        currentIndex++;
                    } else {
                        item.IsInMenu = false;
                        addMenuItem(menuMore, item, id);
                    }
                }
                sessionStorage.setItem("menuItems", JSON.stringify(menuItems));
                sessionStorage.setItem("currentIndex", currentIndex);
                //  More
                if (needMore) {
                    
                    var li = document.createElement('li');
                    li.setAttribute("id", "ctl00_navItemMore");
                    var anchor = document.createElement("a");
                    anchor.setAttribute("id", "nav-menu-more");
                    var tabindex = currentIndex + 2;
                    anchor.setAttribute("tabindex", tabindex.toString());
                    anchor.setAttribute("onclick", "more_click()");
                    anchor.addEventListener("keydown", function (event) {
                        if (event.keyCode === 13) {
                            document.activeElement.blur();
                            more_click();
                        }
                    });
                    var span = document.createElement("span");
                    span.innerText = moreText;
                    let html = '<svg id="Layer_1" data-name="Layer 1" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16"><polyline points="5.912 3.794 10.477 7.767 5.912 11.913" stroke-miterlimit="10"></polyline></svg>';
                    anchor.innerHTML = html;
                    anchor.innerHTML = html;
                    anchor.appendChild(span);
                    li.appendChild(anchor);
                    menu.append(li);
                } else {
                    let more = document.getElementById("primary-navigation-more");
                    if (more.classList.contains("open"))
                        more.classList.remove("open");
                }

                //  Help
                addStandardItem(menu, 'help', currentIndex + 3, 'closeNavigationPanel();__page.openHelpframe("' + noHelpText + '");', helpText, 'cmdHelp16.svg');

                //  IPL button. Only display this button if SRC configures a URL for the IPL website.
                if (isIplUrlSet) {
                    addStandardItem(menu, 'ipl', currentIndex + 4, '__page.openIPLwindow();', "IPL", 'cmdMaterialRequest24.svg');
                }
                
				// Open Mendix in new browser tab button. Only display this button if the Mendix URL is configured in MXConfiguration.json.
				if (mendixSettings && mendixSettings.urls) {
					// Mendix Core
					if (mendixSettings.urls.Core) {
						addStandardItem(menu, 'mx-core', currentIndex + 5, '__page.openMendixWindow("Core")', "MX Core", 'cmdOpenInNewTab24.svg');
					}
					// Mendix Semiconductor
					if (mendixSettings.urls.Semiconductor) {
						addStandardItem(menu, 'mx-semi', currentIndex + 6, '__page.openMendixWindow("Semiconductor")', "MX Semi", 'cmdOpenInNewTab24.svg');
					}
					// Mendix Electronics
					if (mendixSettings.urls.Electronics) {
						addStandardItem(menu, 'mx-elec', currentIndex + 7, '__page.openMendixWindow("Electronics")', "MX Elec", 'cmdOpenInNewTab24.svg');
					}					
                }

                let chatUrl = document.getElementById("plChatUrl");
                if (isSSO && isSam) {
                    //plchat
                    if (chatUrl)
                        addStandardItem(menu, 'plchat', currentIndex + 8, 'openPlchat()', plChatText, 'plchat24.svg');
                    // Portal Studio
                    //let psAccess = sessionStorage.getItem("psAccess");
                    //if (psAccess)
                    addStandardItem(menu, 'alerts', currentIndex + 9, 'openPortalStudio(2)', portalStudioText, 'cmdUIApplication24.svg');

                    //  Settings
                    addStandardItem(menu, 'settings', currentIndex + 10, 'settings_click()', settingsText, 'cmdSettings24.svg');
                } else {
                    // Portal Studio
                    //let psAccess = sessionStorage.getItem("psAccess");
                    //if (psAccess)
                    addStandardItem(menu, 'alerts', currentIndex + 8, 'openPortalStudio(2)', portalStudioText, 'cmdUIApplication24.svg');

                    //  Settings
                    addStandardItem(menu, 'settings', currentIndex + 9, 'settings_click()', settingsText, 'cmdSettings24.svg');
                }              
				
                //  User Profile
                var li = document.createElement('li');
                li.setAttribute("id", "nav-item-avatar");
                var anchor = document.createElement("a");
                anchor.setAttribute("id", "nav-anchor-user");
                var tabindex = currentIndex + 6;
                anchor.setAttribute("tabindex", tabindex.toString());
                anchor.setAttribute("onclick", "usr_click();");
                anchor.addEventListener("keydown", function (event) {
                    if (event.keyCode === 13) {
                        document.activeElement.blur();
                        usr_click();
                    }
                });
                var span = document.createElement("span");
                span.setAttribute("id", "nav-item-avatar-link");
                let html = '<svg id="Artwork" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24"><g><path d="M12,10.5c2.2,0,4-2.9,4-5.3A4.122,4.122,0,0,0,12,1,4.122,4.122,0,0,0,8,5.2C8,7.6,9.8,10.5,12,10.5ZM12,2a3.057,3.057,0,0,1,3,3.2c0,1.9-1.5,4.3-3,4.3S9,7.1,9,5.2A3.057,3.057,0,0,1,12,2Z" fill="#464646"></path><path d="M16.7,11.1l-.3-.1-.2.2a7.208,7.208,0,0,1-8.4,0L7.6,11l-.3.1A5.515,5.515,0,0,0,4,15.9V24H20V15.9A5.4,5.4,0,0,0,16.7,11.1ZM19,23H17V17H16v6H8V17H7v6H5V15.9a4.324,4.324,0,0,1,2.482-3.8,7.874,7.874,0,0,0,9.036,0A4.515,4.515,0,0,1,19,15.9Z" class="aw-theme-iconOutline" fill="#464646"></path></g></svg>';
                span.innerHTML = html;
                anchor.appendChild(span);
                li.appendChild(anchor);
                menu.append(li);
            }
        }    
    }

    if (!isResize) {
        window.addEventListener('resize', function (event) {
            if (_menuResize) clearTimeout(_menuResize);
            _menuResize = setTimeout(renderPrimaryMenu(true), 250);
        }, true);        
    }
}

// The IPL menu is used in conjunction with SRC. SRC can configure a URL that points to the customer's IPL system.
// If the URL is set then this function will return true. The results will determine whether or not a button will
// be displayed below the help button.
function IsIPlUrlSet() {
    var hasIPLUrl = sessionStorage.getItem("HasIPLURL");
    if (hasIPLUrl === null) {
        $.ajax({
            url: 'Config/IPLConfiguration.json',
            dataType: 'json',
            cache: false,
            async: false,
            success: function (iplSettings) {
                var url = iplSettings.url;
                if (url != undefined)
                    hasIPLUrl = (url.length !== 0);
            },
            error: function (jqXHR) {
                hasIPLUrl = false;
            }
        });
        sessionStorage.setItem("HasIPLURL", hasIPLUrl);
    }
    return hasIPLUrl && hasIPLUrl !== "false";
}

function addStandardItem(menu, id, tabindex, onclick, text, image) {
    var li = document.createElement('li');
    li.setAttribute("id", "nav-item-" + id);
    var anchor = document.createElement("a");
    anchor.setAttribute("onclick", onclick);
    anchor.setAttribute("id", "nav-anchor-" + id);
    anchor.setAttribute("title", text);
    anchor.setAttribute("tabindex", tabindex.toString());
    anchor.addEventListener("keydown", function (event) {
    if (event.keyCode === 13) {
        document.activeElement.blur();
        if (id == 'help') {
            closeNavigationPanel();
            __page.openHelpframe(noHelpText);
        }
        else if (id == 'alerts') {
            openPortalStudio(2);
        }
        else if (id == 'settings') {
            settings_click();
        }
    }
    });
    anchor.setAttribute("data-menu", text);
    var svg = document.createElement("img");
    svg.src = "/CamstarPortal/assets/image/" + image;
    svg.setAttribute("data-name", "icon-placeholder");
    svg.setAttribute("width", "16");
    svg.setAttribute("height", "16");
    svg.setAttribute("viewBox", "0 0 16 16");
    var span = document.createElement("span");
    span.innerText = text;
    anchor.appendChild(svg);
    anchor.appendChild(span);
    li.appendChild(anchor);
    menu.append(li);
}

function addMenuItem(menu, item, id) {
    var li = document.createElement('li');
    li.setAttribute("id", "nav-item-" + id);
    var index = id + 2;
    var anchor = document.createElement("a");
    anchor.setAttribute("id", "primary-nav-menu-" + id);
    anchor.setAttribute("onclick", "menuItem_Click(this)");
    anchor.addEventListener("keydown", function (event) {
        if (event.keyCode === 13) {
            document.activeElement.blur();
            menuItem_Click(this);
            var item = sessionStorage.getItem("subMenuItem");
            var jsonItem = JSON.parse(item);
            var id = jsonItem[0].Id;
            document.getElementById("anchor-nav-submenu"+id).focus();
        }
    });
  
    anchor.setAttribute("title", item.DisplayName);
    if (item.IsInMenu) {
        anchor.setAttribute("tabindex", index.toString());
    }
    else {
        anchor.setAttribute("tabindex",(index + 1).toString());
    }
    anchor.setAttribute("data-menu", item.DisplayName);
    var svg = document.createElement("img");
    let icon = item.ApolloIcon ? item.ApolloIcon : "cmdFocusOn";
    svg.src = "/CamstarPortal/assets/image/" + icon + "24.svg";
    svg.setAttribute("id", "icon-placeholder");
    svg.setAttribute("data-name", "icon-placeholder");
    svg.setAttribute("width", "16");
    svg.setAttribute("height", "16");
    svg.setAttribute("viewBox", "0 0 16 16");
    var span = document.createElement("span");
    span.setAttribute("id", "nav-menu" + item.DisplayName.replace(" ", "_"));
    span.innerText = item.DisplayName;

    anchor.appendChild(svg);
    anchor.appendChild(span);
    li.appendChild(anchor);
  
    menu.append(li);
}

function usr_click() {
    var liElement = document.getElementById("nav-item-avatar")
    var ulElement = liElement.parentElement;

    if (liElement.classList.contains("open")) {
        //document.getElementById("header-title").innerText = "";
        closeNavigationPanel();
    }
    else {
        showSecondaryMenu(liElement);
        closeNavigationPanel();
        liElement.classList.add("open");
        ulElement.classList.add("open");
        document.getElementById("secondary-navigation").classList.add("open");
        document.getElementById("secondary-nav-menu-pin").classList.add("open");
       
        if (document.contains(document.getElementById("secondary-nav-menu-settings"))) {
            document.getElementById("secondary-nav-menu-settings").remove();

        }
        document.getElementById('subnav-title').innerHTML = "";;

		let menu = $('#secondary-nav-menu-user');
		if (menu.length) {
			menu.addClass("open");			
			return;
		}
		
		var userProfileText = "*User Profile";
		var logoutText = "*Log Off";
		var settingsText = "*Settings";
		var labels = [{ Name: 'Employee_UserProfile' }, { Name: 'Banner_LogOff' }, { Name: 'Banner_Settings' }];
		__page.getLabels(labels, function (response) {
			if ($.isArray(response)) {
				$.each(response, function () {
					var labelName = this.Name;
					var labelText = this.Value;
					switch (labelName) {
						case 'Employee_UserProfile':
							userProfileText = labelText;
							break;
						case 'Banner_LogOff':
							logoutText = labelText;
							break;
						case 'Banner_Settings':
							settingsText = labelText;
							break;
						default:
							break;
					}
				});
			}
			else {
				alert(response.Error);
			}

			var div = document.createElement('div');
			div.setAttribute('id', 'div-secondary-nav-menu-userProfile');

			var ol = document.createElement('ol');
			ol.setAttribute('id', 'secondary-nav-menu-user');
			ol.setAttribute('class', 'subnav-list open');

			var ll = document.createElement('li');
			ll.setAttribute("id", "secondary-nav-submenu-user1");
			var span0 = document.createElement("span");
			span0.setAttribute("id", "span-userProfile");
			
			let html = '<svg id="Artwork" xmlns="http://www.w3.org/2000/svg" viewBox="-6 -6 36 36"><g><path d="M12,10.5c2.2,0,4-2.9,4-5.3A4.122,4.122,0,0,0,12,1,4.122,4.122,0,0,0,8,5.2C8,7.6,9.8,10.5,12,10.5ZM12,2a3.057,3.057,0,0,1,3,3.2c0,1.9-1.5,4.3-3,4.3S9,7.1,9,5.2A3.057,3.057,0,0,1,12,2Z" fill="#464646"></path><path d="M16.7,11.1l-.3-.1-.2.2a7.208,7.208,0,0,1-8.4,0L7.6,11l-.3.1A5.515,5.515,0,0,0,4,15.9V24H20V15.9A5.4,5.4,0,0,0,16.7,11.1ZM19,23H17V17H16v6H8V17H7v6H5V15.9a4.324,4.324,0,0,1,2.482-3.8,7.874,7.874,0,0,0,9.036,0A4.515,4.515,0,0,1,19,15.9Z" class="aw-theme-iconOutline" fill="#464646"></path></g></svg>';			
		    span0.innerHTML = html;
			
			//span0.appendChild(img);
			ll.appendChild(span0);
			ol.appendChild(ll);
			//userprofile
			var el = document.createElement('li');
			el.setAttribute("id", "secondary-nav-submenu-user2");

			var img = document.createElement("img");
			img.setAttribute("id", "user-userProfile");
			img.setAttribute("src", "/CamstarPortal/assets/image/cmdUser24.svg");
			img.setAttribute("class", "primary-menu-icon");
			//img.setAttribute("style", "height:16px; width:16px");
			el.appendChild(img);

			var anchor = document.createElement("a");
			anchor.setAttribute("id", "anchor-nav-submenu-user1");
			anchor.setAttribute("onClick", "userInteraction('LOAD_USERPROFILE')")
            anchor.setAttribute("class", "app-menu-link");
            anchor.setAttribute("tabindex", "1");
            anchor.addEventListener("keydown", function (event) {
                if (event.keyCode === 13) {
                    document.activeElement.blur();
                    userInteraction('LOAD_USERPROFILE');
                }
            });
			var span = document.createElement("span");
            span.innerHTML = userProfileText;
            span.setAttribute("class", "app-menu-link");
			anchor.appendChild(span);
			el.appendChild(anchor);
			ol.appendChild(el);
			//logout
			var li = document.createElement('li');
			li.setAttribute("id", "secondary-nav-submenu-user3");

			img = document.createElement("img");
			img.setAttribute("id", "user-logout");
			img.setAttribute("src", "/CamstarPortal/assets/image/cmdSignOut24.svg");
			img.setAttribute("class", "primary-menu-icon");
			//img.setAttribute("style", "height:16px; width:16px");
			li.appendChild(img);

			var anchor1 = document.createElement("a");
            anchor1.setAttribute("id", "anchor-nav-submenu-user2");
            anchor1.setAttribute("class", "app-menu-link");
            anchor1.setAttribute("onClick", "userInteraction('LOAD_CONFIRMLOGOUT')");
            anchor1.setAttribute("tabindex", "2");
            anchor1.addEventListener("keydown", function (event) {
                if (event.keyCode === 13) {
                    document.activeElement.blur();
                    userInteraction("LOAD_CONFIRMLOGOUT");
                }
            });
			var span1 = document.createElement("span");
            span1.innerHTML = logoutText;
            span1.setAttribute("class", "app-menu-link");
			anchor1.appendChild(span1);
			li.appendChild(anchor1);
			ol.appendChild(li);
			
			div.appendChild(ol);
			document.getElementById("secondary-navigation").appendChild(div);		
		});		
    }

}
function settings_click() {
    var liElement = document.getElementById("nav-item-settings");
    var ulElement = liElement.parentElement;
    //document.getElementById("header-title").innerText = "";
    if (liElement.classList.contains("open")) {
		liElement.classList.remove("open");
		ulElement.classList.remove("open");
        closeNavigationPanel();
    }
    else {
        closeNavigationPanel();
        showSecondaryMenu(liElement);
        document.getElementById('subnav-title').innerHTML = "";
        liElement.classList.add("open");
        ulElement.classList.add("open");
        document.getElementById("secondary-navigation").classList.add("open");
        document.getElementById("secondary-nav-menu-pin").classList.add("open");
        if (document.contains(document.getElementById("secondary-nav-menu-user"))) {
            document.getElementById("secondary-nav-menu-user").remove();

        }
        let nav = $('#secondary-navigation');
		let par = $('#nav-item-settings');
		let menu = $('#secondary-nav-menu-settings');
		if (menu.length) {
			menu.addClass("open");
			return;
		}
		
		var settingsText = "*Settings";
		var setLineAssignmentText = "*Set Line Assignment";
		var setFilterTagsText = "*Set Filter Tags";
		var labels = [{ Name: 'Banner_Settings' }, { Name: 'Banner_SetFilterTags' }, {Name: 'Lbl_SetLineAssignment_Title'}];
		__page.getLabels(labels, function (response) {
			if ($.isArray(response)) {
				$.each(response, function () {
					var labelName = this.Name;
					var labelText = this.Value;
					switch (labelName) {
						case 'Banner_Settings':
							settingsText = labelText;
							break;
						case 'Lbl_SetLineAssignment_Title':
							setLineAssignmentText = labelText;
							break;
						case 'Banner_SetFilterTags':
							setFilterTagsText = labelText;
							break;
						default:
							break;
					}
				});


			}
			else {
				alert(response.Error);
			}
			
			var div = document.createElement('div');
			div.setAttribute('id', 'div-secondary-nav-menu-settings');
			
			var ol = document.createElement('ol');
			ol.setAttribute('id', 'secondary-nav-menu-settings');
			ol.setAttribute('class', 'subnav-list open');

			var liElement = document.createElement('li');
			liElement.setAttribute("id", "secondary-nav-submenu-settings-header");
			liElement.innerText = settingsText;
			ol.appendChild(liElement);
			var li = document.createElement('li');
			li.setAttribute("id", "secondary-nav-submenu-settings-2");

			var anchor1 = document.createElement("a");
			anchor1.setAttribute("id", "anchor-nav-submenu-settings-lineAssignment");
			anchor1.setAttribute("onClick", "userInteraction('LOAD_LINEASSIGNMENT')")
            anchor1.setAttribute("title", setLineAssignmentText);
            anchor1.setAttribute("tabindex", "1");
            anchor1.setAttribute("class", "app-menu-link");
            anchor1.addEventListener("keydown", function (event) {
                if (event.keyCode === 13) {
                    document.activeElement.blur();
                    userInteraction("LOAD_LINEASSIGNMENT");
                }
            });

            var span1 = document.createElement("span");
            span1.innerHTML = setLineAssignmentText;
            span1.setAttribute("class", "app-menu-link");
            anchor1.appendChild(span1);
            li.appendChild(anchor1);
            ol.appendChild(li);

            var el = document.createElement('li');
            el.setAttribute("id", "secondary-nav-submenu-settings-3");

            var anchor2 = document.createElement("a");
            anchor2.setAttribute("id", "anchor-nav-submenu-settings-filterTags");
            anchor2.setAttribute("onClick", "userInteraction('LOAD_FILTERTAGS')")
            anchor2.setAttribute("title", setFilterTagsText);
            anchor2.setAttribute("class", "app-menu-link");
            anchor2.setAttribute("tabindex", "2");
            anchor2.addEventListener("keydown", function (event) {
                if (event.keyCode === 13) {
                    document.activeElement.blur();
                    userInteraction("LOAD_FILTERTAGS");
                }
            });
            
            var span = document.createElement("span");
            span.setAttribute("id", "span-nav-submenu-settings-filterTags");
            span.innerHTML = setFilterTagsText;
            span.setAttribute("class", "app-menu-link");
            anchor2.appendChild(span);
            el.appendChild(anchor2);
			ol.appendChild(el);
			div.appendChild(ol);
			
			document.getElementById("secondary-navigation").appendChild(div);			
		});		
		
    }

}
function renderSettingsMenu() {
}
function home_click() {
    var item = sessionStorage.getItem("homepage");
    if (item) {
        var jsonItem = JSON.parse(item);
        if (jsonItem) {
            var lielement = document.getElementById('header-title');
            if (lielement && typeof _menuLabels !== 'undefined' && _menuLabels != null) {
                let text = _menuLabels['HomePageLbl'];
                if (text)
                    lielement.innerText = text;
            }
            userInteraction(jsonItem);
        } else
            console.warn("No homepage configured.")
    } else
        console.warn("No homepage configured.")
}
function userInteraction(nav) {
	closeNavigationPanel();
	let isError=false;
	if (typeof nav === 'undefined')
	{
		isError=true;
	}
    var __page = $find("__Page");
    switch (nav) {
        case 'LOAD_LINEASSIGNMENT':
            var labels = [{ Name: 'Lbl_SetLineAssignment_Title' }, { Name: 'Lbl_PopupLoadingTitle' }];
            __page.getLabels(labels, function (response) {
                if ($.isArray(response)) {
                    var setLineAssignmentText;
                    var loadingLbl;
                    $.each(response, function () {
                        var labelName = this.Name;
                        var labelText = this.Value;
                        switch (labelName) {
                            case 'Lbl_SetLineAssignment_Title':
                                setLineAssignmentText = labelText;
                                break;
                            case 'Lbl_PopupLoadingTitle':
                                loadingLbl = labelText;
                                break;
                            default:
                                break;
                        }
                    });
                    pop.showAjax('./LineAssignmentPage.aspx?IsFloatingFrame=2', setLineAssignmentText, 520, 662, 0, 0, true, '', '', this, true, '', null, false, false, loadingLbl);
                }
                else {
                    alert(response.Error);
                }
            });

            break;
        case 'LOAD_FILTERTAGS':
            var labels = [{ Name: 'Banner_SetFilterTags' }, { Name: 'Lbl_PopupLoadingTitle' }];
            __page.getLabels(labels, function (response) {
                if ($.isArray(response)) {
                    var setFilterTagsText;
                    var loadingLbl;
                    $.each(response, function () {
                        var labelName = this.Name;
                        var labelText = this.Value;
                        switch (labelName) {
                            case 'Banner_SetFilterTags':
                                setFilterTagsText = labelText;
                                break;
                            case 'Lbl_PopupLoadingTitle':
                                loadingLbl = labelText;
                                break;
                            default:
                                break;
                        }
                    });
                    pop.showAjax('./ModelingDataFilterSessionValuePopup_VP.aspx?IsFloatingFrame=2', setFilterTagsText, 420, 508, 0, 0, true, '', '', this, true, '', null, false, false, loadingLbl);
                }
                else {
                    alert(response.Error);
                }
            });
            break;
        case 'LOAD_USERPROFILE':
            var labels = [{ Name: 'Lbl_UserAndSystemInfo' }, { Name: 'Lbl_PopupLoadingTitle' }];
            __page.getLabels(labels, function (response) {
                if ($.isArray(response)) {
                    var userProfileText;
                    var loadingLbl;
                    $.each(response, function () {
                        var labelName = this.Name;
                        var labelText = this.Value;
                        switch (labelName) {
                            case 'Lbl_UserAndSystemInfo':
                                userProfileText = labelText;
                                break;
                            case 'Lbl_PopupLoadingTitle':
                                loadingLbl = labelText;
                                break;
                            default:
                                break;
                        }
                    });
                    pop.showAjax('./UserProfile_VP.aspx?CallStackKey=&IsFloatingFrame=2', userProfileText, 800, 830, 0, 0, true, '', '', this, true, '', null, false, false, loadingLbl);

                }
                else {
                    alert(response.Error);
                }
            });
            break;
        case 'LOAD_CONFIRMLOGOUT':
            var executeWhenTrue = "";            
                executeWhenTrue = "KillSession(true);setTimeout( function()  {window.top.location = 'default.aspx';}, 2000 );";          
            var labels = [{ Name: 'AlertConfirmLogout' }, { Name: 'Lbl_Warning' }];

            __page.getLabels(labels, function (response) {
                if ($.isArray(response)) {
                    var confMessage;
                    var warningLbl;
                    $.each(response, function () {
                        var labelName = this.Name;
                        var labelText = this.Value;
                        switch (labelName) {
                            case 'AlertConfirmLogout':
                                confMessage = labelText;
                                break;
                            case 'Lbl_Warning':
                                warningLbl = labelText;
                                break;
                            default:
                                break;
                        }
                    });
                    JConfirmationLong(confMessage, null, executeWhenTrue, null, null, null, warningLbl);
                }
                else {
                    alert(response.Error);
                }
            });
            break;
        case 'LOAD_HELPFRAME':
            __page.getLabel('Lbl_NoHelpFileMessage', function (label) {
                var noHelpText = 'Online help is currently being developed and will be deployed in a future release.';
                if ($.isArray(label)) {
                    noHelpText = label[0].Value;
                }
                __page.openHelpframe(noHelpText);
            });

            break;
        case 'IPL_URL':
            __page.openIPLwindow();

            break;
        case 'REDIRECT_TO':
            var redirectPage = getUrlParamVal("redirectToPage", nav.params);
            var redirectWebpart = getUrlParamVal("redirectToWebpart", nav.params);
            var redirectPageflow = getUrlParamVal("redirectToPageflow", nav.params);
            if (!redirectPage) redirectPage = "Main.aspx";
            var queryString = 'ResetCallStack=true';
            if (redirectPageflow) {
                queryString += '&redirectToPageFlow=' + redirectPageflow;
            }
            if (redirectWebpart) {
                queryString += '&WebPart=' + redirectWebpart;
            }
            if (nav.pstest == "ps") {
                queryString += '&Test=true';
            }
            __toppage.openInTabId(redirectPage, queryString, null, null, null, true);
            break;
        default:
            var queryString = 'ResetCallStack=true';
            if (nav.QueryString) {
                queryString += '&' + nav.QueryString;
            }
            var isPageFlow = nav.UIVirtualPageName.indexOf('PF.') > -1;
            if (isPageFlow) {
                __toppage.openInTabId('Main.aspx', queryString + '&redirectToPageFlow=' + nav.UIVirtualPageName, nav.DisplayName, null, null, true);
            }
            else {
                var pageDisplayMode = nav.PageDisplay;
                if (pageDisplayMode === "1") {    // openInNewWindow
                    __toppage.openInNewWindow(nav.PageURL);
                }
                else {
                    if (nav.PageURL) {
                        alert("External resource must be open in a new window. Set menu item's PageDisplay property to 'InNewBrowser' mode.");
                    }
                    else {
                        __toppage.openInTabId(nav.UIVirtualPageName, queryString, nav.DisplayName, null, null, true);
                        //  Uncomment the line below to attempt to open an existing tab of the home page and comment out the line above

                        //__toppage.displayExistingTab(nav.UIVirtualPageName, queryString, nav.DisplayName, null, null, true);
                    }
                }
            }
            break;

    }
}

function openPlchat() {
    const chatUrl = document.getElementById("plChatUrl").value;
    const container = document.getElementById("plchat-element");

    // remove old iframe (if any)
    const existing = document.getElementById("plchat-iframe");
    if (existing) existing.remove();

    // create iframe
    const iframe = document.createElement("iframe");
    iframe.id = "plchat-iframe";
    iframe.style.cssText = "width:40%;height:40%;position:fixed;bottom:0px;right:90px;";
    iframe.onload = function () {
        let doc = iframe.contentDocument || iframe.contentWindow.document;

        // Ensure clean DOM
        doc.open();
        doc.write("<!DOCTYPE html><html><head></head><body></body></html>");
          doc.close();
          // 1. Create the <style> element
          let style = doc.createElement("style");

          // 2. Define the CSS rules needed for internal scrolling
          const internalIframeCss = `
            html, body {
            height: 100%;
            margin: 0;
            padding: 0;
            box-sizing: border-box;
           }

          form.plchat-main-panel,
          .sw-command-panelContent {
          height: 100%;
            display: flex;
            flex-direction: column;
          }

          .plchat-header-wrapper,
           .plchat-footer {
            flex-shrink: 0;
            }

            #plchat-body {
            flex-grow: 1;
            overflow-y: auto !important; /* Use !important to ensure override */
            overflow-x: hidden;
            min-height: 0; /* Critical for flex items that need to scroll */
            }
           `;
          // 3. Set the CSS content for the style tag
          style.textContent = internalIframeCss; // Or style.innerHTML = internalIframeCss;

          // 4. Append the style tag to the iframe's head
          doc.head.appendChild(style);



        // Add loader script
        let script = doc.createElement("script");
        script.src = `${chatUrl}/des-plchat-acc-loader.js`;
        doc.head.appendChild(script);

        // Create chat element
        let chatEl = doc.createElement("des-plchat-acc");
        chatEl.id = "plchat-control";
        chatEl.setAttribute("display-close-chat", "true");
        chatEl.setAttribute("display-plchat-title", "Opcenter Chat - Pre Release");
        chatEl.setAttribute(
            "custom-color-palette",
            '{"--plchat-header-background-color":"#0F789B","--plchat-border-color-text":"#0F789B","--plchat-border-color":"#0F789B"}'
        );

        doc.body.appendChild(chatEl);

        // Listen for close inside iframe → forward to parent
        iframe.contentWindow.addEventListener("des-plchat-close", () => {
            parent.postMessage({ type: "plchat-close" }, "*");
        });

        // Fetch accSessionId and inject into chat
        $.ajax({
            url: "Main.aspx/GetPlChatDetails",
            method: "POST",
            async: true,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (response) {
                let accSessionId;
                try {
                    if (response && response.d) {
                        let parsed = JSON.parse(response.d);
                        accSessionId = parsed.accSessionId;
                    }
                } catch (e) {
                    console.error("Failed to parse response.d:", e, response);
                }

                if (accSessionId) {
                    chatEl.setAttribute("acc-session-id", accSessionId);
                }
            }
        });
    };

  
    // Append the iframe AFTER wiring onload and set a proper src to ensure onload fires
    // use about:blank explicitly
    iframe.src = "about:blank";
    container.appendChild(iframe);

    $("#plchat-element").show();
    
}
// Parent listens for close event
window.addEventListener("message", function (event) {
    if (event.data && event.data.type === "plchat-close") {
        console.log("Chat close event received from iframe");
        $("#plchat-iframe").remove();
    }
});


// The method is event handler used for InterationGrid PDD control. 
// It is invoked when text box with number (int, float, dec, fixed) inside is changed. 
// The method verifies the value and if the value is out of the limits the background color is switched.
// The colors are defined in the CSS classes.
// The method is also invoked to revalidate related data points if the Is Limit Override check box is being clicked.
// Parameters:
//     string lowerLimit, upperLimit   - value limits;
//     bool   isLimitOverrideAllowed   - true if the the value can be out of the limits. 
//     int    layoutMode               - current dataPointSummary rendering mode: 
//                                      1- IterationGrid; 2 - RowColumn; 0 - validation disabled.
//     string valueType                - type of validated value
function OnDCDFieldValueChanged(lowerLimit, upperLimit, isLimitOverrideAllowed, layoutMode, valueType)
{
    var sampleRow = null;
    if(event.srcElement != null)
    {
        var inputElement = event.srcElement;
        if (valueType== null)
        {
            // The Override value checkbox was clicked.
            sampleRow = GetSampleRow(inputElement);
            if (sampleRow == null)
                return;
                
            var inputs = sampleRow.getElementsByTagName("INPUT");
                for(var j=0; j<inputs.length; j++)
                {
                    if (inputs[j].type == "text")
                    {
                        if (inputs[j].onchange != null)
                            if (inputs[j].onchange.toString().indexOf(mkValidateFunctionName)>=0)
                            {
                                // The text box onchange event should be fired.
                                inputs[j].fireEvent("onchange");
                            }
                    }
                    else if (inputs[j].type == "checkbox")
                    {
                        if (inputs[j].onclick != null)
                            if (inputs[j].onclick.toString().indexOf(mkValidateFunctionName)>=0 && inputs[j] != inputElement)
                            {
                                // The text box onchange event should be fired.
                                inputs[j].fireEvent("onclick");
                            }
                    }
                }
            inputs = sampleRow.getElementsByTagName("SELECT");
            for(var j=0; j<inputs.length; j++)
            {
                if (inputs[j].onchange != null)
                    if (inputs[j].onchange.toString().indexOf(mkValidateFunctionName)>=0 && inputs[j] != inputElement)
                    {
                        // The text box onchange event should be fired.
                        inputs[j].fireEvent("onchange");
                    }
            }
        }
        else
        {            
            var isCompositeLimitOverrideAllowed = isLimitOverrideAllowed;
            if (layoutMode == 1 || layoutMode == 2)
            {
                // Find checkbox IsOverrideAllowed for Iteration Grid           
                sampleRow = GetSampleRow(inputElement);                
                var ovrctl = GetOverrideEnableControl(sampleRow); 
                if (ovrctl != null)
                        isCompositeLimitOverrideAllowed = (ovrctl.checked && isLimitOverrideAllowed);
            }
            else
                return;

            // This is a comparison itself
            var currentValue = inputElement.value;
            if (valueType != "String" && valueType != "Boolean")
            {
                // Numeric validation
                if (valueType == "Timestamp") {
                    currentValue = Date.parse(currentValue);
                }
                else {
                    currentValue = Number.parseLocale(currentValue);
                }

                if(isNaN(currentValue) && inputElement.value != "")
                {
                    inputElement.parentElement.className = "TextMediumError";
                }
                else
                {
                    if (lowerLimit != null)
                        if (valueType == "Timestamp") {
                            lowerLimit = Date.parse(lowerLimit);
                        }
                        else {
                            lowerLimit = Number.parseLocale(lowerLimit);
                        }
                    if (upperLimit != null)
                        if (valueType == "Timestamp") {
                            upperLimit = Date.parse(upperLimit);
                        }
                        else {
                            upperLimit = Number.parseLocale(upperLimit);
                        }
                    if(((lowerLimit == null) || (lowerLimit != null && currentValue >= lowerLimit)) &&
                       ((upperLimit == null) || (upperLimit != null && currentValue <= upperLimit)) ||
                       (event.srcElement.value == ""))
                    {
                        inputElement.parentElement.className = "TextMedium";
                    }
                    else
                    {
                        inputElement.parentElement.className = isCompositeLimitOverrideAllowed ? "TextMediumWarning" : "TextMediumError";
                    }
                }
             }
             else if (valueType == "Boolean")
             {
                var boolValue = null;
                var isElementCheckBox = (inputElement.tagName == "INPUT");
                if (! isElementCheckBox )
                {
                    var selectedValue = inputElement.value;
                    if (selectedValue == "1") 
                        boolValue = "true";
                    else 
                        if (selectedValue == "0" ) 
                            boolValue = "false";
                }
                else
                {
                    boolValue = inputElement.checked ? "true" : "false";
                }
                var violation = false;
                if (boolValue!=null && lowerLimit != null && upperLimit!=null)
                {
                    lowerLimit = lowerLimit.toLocaleLowerCase();
                    upperLimit = upperLimit.toLocaleLowerCase();

                    if (lowerLimit == "1") lowerLimit = "true";
                    else if (lowerLimit == "0" ) lowerLimit = "false";

                    if (upperLimit == "1") upperLimit = "true";
                    else if (upperLimit == "0") upperLimit = "false";

                    if(lowerLimit == upperLimit)
                        violation = (boolValue != lowerLimit);
                }
                if (! violation) {
                    if (isElementCheckBox)
                        inputElement.className = "Checkbox";
                    else
                        inputElement.className = "SelectMedium";
                }
                else
                {
                    if (isElementCheckBox)
                    inputElement.className = isCompositeLimitOverrideAllowed ? "CheckboxWarning" : "CheckboxError";
                    else
                        inputElement.className = isCompositeLimitOverrideAllowed ? "SelectMediumWarning" : "SelectMediumError";
                }
             }
             else 
             {
                // String validation
                var stringValue = currentValue.toLocaleLowerCase();
                
                if (((lowerLimit == null) || (lowerLimit != null && stringValue.localeCompare(lowerLimit.toLocaleLowerCase()) >= 0)) &&
                   ((upperLimit == null) || (upperLimit != null && stringValue.localeCompare(upperLimit.toLocaleLowerCase()) <= 0)) ||
                   stringValue.length==0)
                {
                    event.srcElement.className = "TextMedium";
                }
                else
                {
                    event.srcElement.className = isCompositeLimitOverrideAllowed ? "TextMediumWarning" : "TextMediumError";
                }
             }
         }
    }
} //OnDCDFieldValueChanged

function OnDCDFieldValueChanged_Comp(el /* parent id or input element*/, lowerLimit, upperLimit, decimalScale, booleanTrue, booleanFalse, isLimitOverrideAllowed, layoutMode, valueType, inp, dpName, $dp) {
    var sampleRow = null;
    var element = (typeof el == "string") ? document.getElementById(el) : el;

    if (!element)
        return;

    if (!$(element).is(":input"))
        element = $(":input", element).get(0);

    var inputElement = element;

    if (valueType == null) {

        // The Override value checkbox was clicked.
        sampleRow = GetSampleRow(inputElement);
        if (sampleRow == null)
            return;

        var inputs = sampleRow.getElementsByTagName("INPUT");
        for (var j = 0; j < inputs.length; j++) {
            if (inputs[j].type == "text") {
                if (inputs[j].onchange != null)
                    if (inputs[j].onchange.toString().indexOf(mkValidateFunctionName) >= 0) {
                        // The text box onchange event should be fired.
                        inputs[j].fireEvent("onchange");
                    }
            }
            else if (inputs[j].type == "checkbox") {
                if (inputs[j].onclick != null)
                    if (inputs[j].onclick.toString().indexOf(mkValidateFunctionName) >= 0 && inputs[j] != inputElement) {
                        // The text box onchange event should be fired.
                        inputs[j].fireEvent("onclick");
                    }
            }
        }
        inputs = sampleRow.getElementsByTagName("SELECT");

        for (var j = 0; j < inputs.length; j++) {
            if (inputs[j].onchange != null)
                if (inputs[j].onchange.toString().indexOf(mkValidateFunctionName) >= 0 && inputs[j] != inputElement) {
                    // The text box onchange event should be fired.
                    inputs[j].fireEvent("onchange");
                }
        }
    }
    else {

        var isCompositeLimitOverrideAllowed = isLimitOverrideAllowed;
        var $inp = $(inputElement);
        var $inpContext = $inp.closest(".ParametricDataControl");

        if (!$dp) {
            $dp = $inp.closest('.DataPointItem');
            if (!$dp.length)
                $dp = $inp.closest('.DataPointItem-resp,.ShopFloorDCVerticalDataResp,.ShopFloorDCHorizontalDataResp', $inpContext);
        }

        // This is a comparison itself
        var currentValue = $inp.val();
        if (valueType != "String" && valueType != "Boolean") {
            var _currentValue = currentValue.slice();
            // Numeric validation
            if (valueType == "Timestamp") {
                currentValue = Date.parse(currentValue);
            }
            else {
                currentValue = Number.parseLocale(currentValue);
            }

            if (isNaN(currentValue) && inputElement.value != "") {
                $(inputElement.parentElement).addClass("ui-error");
            }
            else {
                if (decimalScale != null && _currentValue != "" && __page.lastFocusedControlId != "" && !__page.lastFocusedControlId.includes("sidebarFocusID")) {
                    if (valueType == 'Decimal' || valueType == 'Float' || valueType == 'Fixed') {
                        var valueParts = _currentValue.replace(",", ".").split(".");
                        var throwErr = false;
                        var errorLbl = '';
                        if (valueParts.length != 1) {
                            if (decimalScale == '0')
                            {
                                var throwErr = true;
                                errorLbl = 'Lbl_DecimalScaleValidationZero';
                            }
                            else {
                                var decPlaces = _currentValue.replace(",", ".").split(".")[1].length;
                                if (decPlaces != parseInt(decimalScale)) {
                                    throwErr = true;
                                    errorLbl = 'Lbl_DecimalScaleValidation';//Error';
                                }
                            }
                        } else if (decimalScale != '0') {
                            throwErr = true;
                            errorLbl = 'Lbl_DecimalScaleValidation';//Error';
                        }
                        
                        if (throwErr) {
                            var msg = $find('WebPart_StatusBar_UIComponent');
                            if (msg) {
                                __page.getLabel(errorLbl, function (response) {
                                    if ($.isArray(response)) {
                                        let text = response[0].Value.replace("#ErrorMsg.Name2", decimalScale).replace("#ErrorMsg.Name", dpName);
                                        msg.write(text, "Warning");
                                    }
                                    else {
                                        alert(response.Error);
                                    }
                                });
                            }
                            
                        }
                    }
                }
                $(inputElement.parentElement).removeClass("ui-error");
                if (lowerLimit != null)
                    if (valueType == "Timestamp") {
                        lowerLimit = Date.parse(lowerLimit);
                    }
                    else {
                        lowerLimit = Number.parseLocale(lowerLimit);
                    }
                    
                if (upperLimit != null)
                    
                if (valueType == "Timestamp") {
                    upperLimit = Date.parse(upperLimit);
                }
                else {
                    upperLimit = Number.parseLocale(upperLimit);
                }
                var lowLimitBroken = lowerLimit != null && currentValue < lowerLimit;
                var upLimitBroken = upperLimit != null && currentValue > upperLimit;

                var inputWarningClass = isCompositeLimitOverrideAllowed ? "ui-warning" : "ui-error";
                var lowSpanWarningClass = isCompositeLimitOverrideAllowed ? "LL-warning" : "LL-error";
                var upSpanWarningClass = isCompositeLimitOverrideAllowed ? "UL-warning" : "UL-error";
                
                if (valueType == "Timestamp" && (lowLimitBroken || upLimitBroken)) {
                    $(inputElement)[0].style.border = "1px solid #F00";
                }
                else {
                    if (valueType == "Timestamp")
                    $(inputElement)[0].style.border = "1px solid #8c8c8c";
                }
                    $(inputElement.parentElement).toggleClass(inputWarningClass, lowLimitBroken || upLimitBroken);
              

               
                $('.LL', $dp).toggleClass(lowSpanWarningClass, lowLimitBroken);
                $('.UL', $dp).toggleClass(upSpanWarningClass, upLimitBroken);
            }
        }
        else if (valueType == "Boolean") {
            var boolValue = null;
            var isElementCheckBox = $(inputElement).is(":checkbox");
            if (!isElementCheckBox) {
                boolValue = $inp.val() && $inp.val().toLocaleLowerCase();
                if (!boolValue)
                    boolValue = null;
            }
            else {
                boolValue = inputElement.checked ? "true" : "false";
            }
            var violation = false;
            var getLimit = function (lim, bTrue, bFalse) {
                var l = lim.toLocaleLowerCase();
                if (l == "1")
                    l = "true";
                else if (l == "0")
                    l = "false";
                else if (l == bTrue)
                    l = "true";
                else if (l == bFalse)
                    l = "false";
                return l;
            };

            if (boolValue != null && lowerLimit != null && upperLimit != null) {

                var booleanTrue = booleanTrue.toLocaleLowerCase();
                var booleanFalse = booleanFalse.toLocaleLowerCase();

                var lLimit = getLimit(lowerLimit, booleanTrue, booleanFalse);
                var uLimit = getLimit(upperLimit, booleanTrue, booleanFalse);

                if (lLimit == uLimit)
                    violation = (boolValue != lLimit);
            }
            // border color changed for parent SPAN
            if (isElementCheckBox)
                $inp.parent().toggleClass(isCompositeLimitOverrideAllowed ? "CheckboxWarning" : "CheckboxError", violation);
            else
                $inp.parent().toggleClass(isCompositeLimitOverrideAllowed ? "ui-warning-select" : "ui-error-select", violation);
        }
        else {
            // String validation
            currentValue = currentValue || "";
            var stringValue = currentValue.toLocaleLowerCase();

            var lowSpanWarningClass = isCompositeLimitOverrideAllowed ? "LL-warning" : "LL-error";
            var upSpanWarningClass = isCompositeLimitOverrideAllowed ? "UL-warning" : "UL-error";

            if (((lowerLimit == null) || (lowerLimit != null && stringValue.localeCompare(lowerLimit.toLocaleLowerCase()) >= 0)) &&
                ((upperLimit == null) || (upperLimit != null && stringValue.localeCompare(upperLimit.toLocaleLowerCase()) <= 0)) ||
                stringValue.length == 0) {

                $(inputElement.parentElement).removeClass(["ui-error", "ui-warning"]);
                $('.LL', $dp).removeClass(lowSpanWarningClass);
                $('.UL', $dp).removeClass(upSpanWarningClass);
            }
            else {
                var lowLimitBroken = lowerLimit != null && stringValue.localeCompare(lowerLimit.toLocaleLowerCase()) <= 0;
                var upLimitBroken = upperLimit != null && stringValue.localeCompare(upperLimit.toLocaleLowerCase()) >= 0;

                $(inputElement.parentElement).addClass(isCompositeLimitOverrideAllowed ? "ui-warning" : "ui-error");
                $('.LL', $dp).toggleClass(lowSpanWarningClass, lowLimitBroken);
                $('.UL', $dp).toggleClass(upSpanWarningClass, upLimitBroken);
            }
        }
    }
} 

// Provides initial validation of data points after post backs.
// Parameters:
//       dataPointID   - ID of dataPoint control
//       initArray     - is the array of pairs of control ID and its event. 
function InitDataPointValidation(dataPointID, initArray)
{
    for(var i=0; i<initArray.length; i+=2)
    {
        var id = dataPointID + "_"+ initArray[i];
        var eventName = initArray[i+1];
        var validatedControl = null;
        
        // Search for the control
        var parentObj = document.all[id];
        var fired = false;
        if (parentObj != null)
        {
            var inputObjs = parentObj.getElementsByTagName("input");
            for(var j=0; j<inputObjs.length; j++)
            {
                var obj = inputObjs[j];
                if (eventName == "OnChange")
                {
                    if(obj.onchange != null)
                        if (obj.onchange.toString().indexOf(mkValidateFunctionName) >= 0 )
                        {
                            obj.fireEvent(eventName);
                            fired = true;
                            break;
                        }
                }
                else if (eventName == "OnClick")
                {
                    if(obj.onclick != null)
                        if (obj.onclick.toString().indexOf(mkValidateFunctionName) >= 0 )
                        {
                            obj.fireEvent(eventName);
                            fired = true;
                            break;
                        }
                }
            }

            if (!fired)
            {
                var selectObjs = parentObj.getElementsByTagName("select");
                for(var j=0; j<selectObjs.length; j++)
                {
                    var obj = selectObjs[j];
                    if (eventName == "OnChange")
                    {
                        if(obj.onchange != null)
                            if (obj.onchange.toString().indexOf(mkValidateFunctionName) >= 0 )
                            {
                                obj.fireEvent(eventName);
                                break;
                            }
                    }
                }
            }
        }
    }
} // InitDataPointValidation

function InitDataPointValidation_Comp(dataPointID, initArray) {

    for (var i = 0; i < initArray.length; i += 2) {
        var elementId = dataPointID + "_" + initArray[i];
        var eventName = initArray[i + 1];

        // Search for the control
        var element = document.getElementById(elementId);
        if (element != null && element.childNodes[0]) {
            element = element.childNodes[0];
            var obj = element;
            var booleanControl = $('#' + elementId).find("select");

            if (booleanControl.length > 0)
                obj = booleanControl[0];

            if (eventName == "OnChange") {
                if (obj.onchange != null)
                    if (obj.onchange.toString().indexOf(mkValidateFunctionName) >= 0) {
                        obj.onchange();
                    }
            }
            else if (eventName == "OnClick") {
                if (obj.onclick != null)
                    if (obj.onclick.toString().indexOf(mkValidateFunctionName) >= 0) {
                        obj.fireEvent(eventName);
                    }
            }
        }
    }

    // Wrap main datapoint table into the div to have correct hotizontal scrolling
    var $pdc = $("#" + dataPointID + " .ParametricDataControlMainTableResp");
    if (!$(".tbl-wrapper", $pdc).length) {
        $(">table", $pdc).wrap("<div class=tbl-wrapper></div>");
    }

    // Adjust header width 
    setTimeout(function () {
        var $pdd = $("#" + dataPointID + " .ParametricDataControlMainTableResp");
        var $hdr = $(".ShopFloorDCHeaderResp", $pdd);
        var $tbl = $(".tbl-wrapper > table", $pdd);
        $hdr.width($tbl.width() - ($pdd.is('[horizontal="true"]') ? 16 : 17));
    }, 100);

} // InitDataPointValidation

// Finds the sample <tr> element in case of IterationGrid or <table> for RowColumn mode.
// Parameter:
//      fromElement - the INPUT or SELECT element that belongs to PDD control.
function GetSampleRow(fromElement)
{
    for (var parent = fromElement.parentElement; parent != null; parent = parent.parentElement)
    {
        if (parent.getAttribute("PDDSample")!=null)
            break;
    }
    return parent;
} //GetSampleRow

// Returns isOverrideLimits checkbox element of PDD control.
// Parameter:
//      sampleRow   - the <tr> or <table> container element.
function GetOverrideEnableControl(sampleRow)
{
    var inputObjs = sampleRow.getElementsByTagName("INPUT");
    for(var i=0; i<inputObjs.length; i++)
    {
        if (inputObjs[i].type == "checkbox" && inputObjs[i].id.indexOf("DataPoint_IsLimitOverride")!=-1)
            return inputObjs[i];
    }
    return null;
} //GetOverrideEnableControl

function DataCollectionResponsiveInit(id) {
    var $dc = $('#' + id + ' .grid-layout');
    var IE = Camstars.Browser.IE;

    if (IE) {
        $dc.css({
            "-ms-grid-rows": "repeat(" + $dc.attr("rows") + ", 1fr)",
            "-ms-grid-columns": "repeat(" + $dc.attr("columns") + ", 1fr)"
        });
    }
    else {
        $dc.css({
            "grid-template-rows": "repeat(" + $dc.attr("rows") + ", 1fr)",
            "grid-template-columns": "repeat(" + $dc.attr("columns") + ", 1fr)"
        });
    }

    var setLimits = function ($s, $labelAndInput) {
        var upLimit = $s.attr("up-limit");
        if (upLimit) {
            $(".Label", $labelAndInput).append("<span class=UL></span>");
            $(".Label .UL", $labelAndInput).text(upLimit);
        }

        var lowLimit = $s.attr("low-limit");
        if (lowLimit) {
            $(".control-container", $labelAndInput).append("<div class=LL-row><span class=LL></span></div>");
            $(".LL", $labelAndInput).text(lowLimit);
        }

        // Add value limit validation OnDCDFieldValueChanged_Comp
        var $c = $(".control-container :input", $labelAndInput);
        if ($c.length) {
            var evName;
            if ($c.is(":text") || $c.is("select")) {
                if ($c.attr("onchange"))
                    $c.attr("onchange", null);
                evName = "change";
            }
            else if ($c.is(":checkbox")) {
                evName = "click";
            }

            $c.off(evName).on(evName, function () {
                limitValidation(this);
            });
        }
    };

    $(".DataPointItem-resp:not([style])", $dc).each(function () {
        var $d = $(this);
        var r = parseInt($d.attr("row"));
        var c = parseInt($d.attr("col"));

        if (!IE)
            $d.css("grid-area", r.toString() + " / " + c.toString() + " / " + (r + 1).toString() + " / " + (c + 1).toString());
        else {
            $d.css({
                "-ms-grid-row": r.toString(),
                "-ms-grid-row-span": "1",
                "-ms-grid-column": c.toString(),
                "-ms-grid-column-span": "1"
            });
        }

        // Add label
        var $l = $("<div class='LabelAndInput active' />");
        $l.append("<div class=Label><span class=label-text /></div>");
        $(".label-text", $l).text($d.attr("label") + $d.attr("uom"));

        // Move data control into the control-container
        $l.append("<div class=control-container />");

        var $wrapper = $("[dataPointName]", $d),
            $placeholder = $('<span style="display: none;" />')
                .insertAfter($wrapper);

        $(".control-container", $l).append($wrapper.detach());

        if ($d.attr("valueType") != "Object") {
            setLimits($d, $l);
        }
        $l.insertBefore($placeholder);
        $placeholder.remove();
    });

    var $computation = $(".computation-responsive > input").parent();
    if ($computation.length) {
        $computation.append("<div class=expression />")
            .append("<div class=equal-sign>=</div>")
            .append("<div class=result-area />");

        var $resultArea = $(".result-area", $computation);
        $resultArea.append("<div class=result><div class=LabelAndInput><div class=Label><span class=label-text /></div></div></div>");
        $(".LabelAndInput", $resultArea).append("<div class=control-container />");
        $(".LabelAndInput .control-container", $resultArea).append("<div class=control-result />");
        $resultArea.append("<div class=calculation /></div>");
        $('.calculation', $resultArea).append($(":submit", $computation).detach());

        $(".expression", $computation).text($computation.attr("expression"));
        $(".result .control-result", $resultArea).text($computation.attr("result")).prop("title", $computation.attr("result"));
        $(".label-text", $resultArea).text($computation.attr("resultName"));

        setLimits($computation, $(".LabelAndInput", $resultArea));

        var v = $computation.attr("violations");
        if (v) {
            var vtype = (v.indexOf("error") != -1) ? "error" : "warning";
            $computation.addClass("ui-" + vtype);

            if (v.indexOf(":up") != -1)
                $(".UL", $resultArea).addClass("UL-" + vtype);

            if (v.indexOf(":low") != -1)
                $(".LL", $resultArea).addClass("LL-" + vtype);
        }
    }

    // Initial data validation
    $(".DataPointItem-resp", $dc).each(function () {
        limitValidation($(":input", this).get(0));
    });

}

function limitValidation(el) {
    var $dp = $(el).closest('.DataPointItem-resp');
    var boolLimits = $dp.attr("boolLimits").split('|');

    OnDCDFieldValueChanged_Comp(el,
        $dp.attr("low-limit") || null, $dp.attr("up-limit") || null, $dp.attr("scale") || null,
        boolLimits[0] || "True", boolLimits[1] || "False",
        $dp.attr("limitOvr") === "true", 1, $dp.attr("valueType"), el, $dp.attr("dataPointName"), $dp);
}

// It is part of creating file upload control
// Hides button from inputLevel1 file control so that user could see our image
function AdjustFileInputWidth(ctrlId, fileCtrlSuffix, childTblLevel3Id)
{
    var divLevel1 = document.getElementById(ctrlId + "_DivLevel1");
    var inputLevel1 = document.getElementById(ctrlId + "_" + fileCtrlSuffix);
    var divLevel2 = document.getElementById(ctrlId + "_DivLevel2");
    var inputLevel3Empty = document.getElementById(ctrlId + "_" + fileCtrlSuffix + "_Level3");

    if (divLevel1 == null || inputLevel1 == null || divLevel2 == null || inputLevel3Empty == null)
        return false;

    var percStart = Math.round((inputLevel1.offsetWidth - inputLevel3Empty.offsetWidth) / inputLevel1.offsetWidth * 100 - 1);
    var percEnd = percStart + 1;

    var strFilter = "alpha(style=1, opacity=100, finishOpacity=0, startX=" + percStart.toString() + ", startY=0, finishX=" + percEnd.toString() + ", finishY=0)";
    inputLevel1.style.filter = strFilter;

    divLevel1.style.height = inputLevel3Empty.offsetHeight + 1;
    divLevel1.style.width = inputLevel1.offsetWidth - inputLevel3Empty.offsetWidth + inputLevel3Empty.offsetHeight + 4;
    divLevel2.style.width = inputLevel1.offsetWidth - inputLevel3Empty.offsetWidth;

    if (childTblLevel3Id != "")
    {
        var childTblLevel3 = document.getElementById(childTblLevel3Id);
        if (childTblLevel3 == null)
            return false;
        childTblLevel3.style.width = inputLevel1.offsetWidth + 3;
    }

}// AdjustFileInputWidth

// Fix focus when scrolling
function OnScrollParametricData(parametricDiv)
{
	SaveScrollingPosition();
}

// Saves current window scroll position
function SaveScrollingPosition()
{
    if( document.forms[0] != null )
    {
        var positionInput = document.forms[0].elements[mPositionInputName];
        if( positionInput != null )
        {
            positionInput.value = document.body.scrollLeft + "," + document.body.scrollTop;
            var userFieldsDiv = document.all[mkUserFieldsDiv]
            if (userFieldsDiv!=null)
                positionInput.value += ("," + userFieldsDiv.scrollLeft+ "," + userFieldsDiv.scrollTop);
            else
                positionInput.value += ",0,0";
            var parametricDiv = document.all[mkParametricDiv]
            if (parametricDiv!=null)
                positionInput.value += ("," + parametricDiv.scrollLeft+ "," + parametricDiv.scrollTop);
            else
                positionInput.value += ",0,0";
        }
    }
} // SaveScrollingPosition

// Restores window scroll position after post-back
// Returns true if scroll position is not at (0,0) and false otherwise
function RestoreScrollingPosition()
{
    var hasScrolling = false;
    if( document.forms[0] != null )
    {
        var positionInput = document.forms[0].elements[mPositionInputName];
        if( positionInput != null )
        {
            if( positionInput.value.length > 0 )
            {
                var	position = positionInput.value.split(',')
                if( position.length == 6 )
                {
                    var	myPageX = position[0];
                    var	myPageY = position[1];
                    window.scrollTo(myPageX,myPageY);
                    if( myPageX > 0 || myPageY > 0 ) hasScrolling = true;

                    var userFieldsDiv = document.all[mkUserFieldsDiv]
                    if (userFieldsDiv!=null)
                    {
                        userFieldsDiv.scrollTop = position[3];
                        userFieldsDiv.scrollLeft = position[2];
                    }
                    var parametricDiv = document.all[mkParametricDiv]
                    if (parametricDiv!=null)
                    {
                        parametricDiv.scrollTop = position[5];
                        parametricDiv.scrollLeft = position[4];
                    }
                }
            }
        }
    }
    return hasScrolling;
} // RestoreScrollingPosition

function StartDownloadFile(Name, Version, AttachmentsID) {

    var iframe = $('#DownloadIframe');
    if (iframe.length < 1) {
        iframe = $('<iframe id="DownloadIframe"/>');
        iframe.hide();
        iframe.appendTo(document.body);
    }
    iframe.attr('src', "DownloadFile.aspx?Name=" + Name + "&Version=" + Version + "&AttachmentsID=" + AttachmentsID + "&refreshPrm=" + (new Date()).getTime());
}


function PrintPDF(filePath) {
    var wnd = window.open('http://localhost/CamstarPortal' + filePath);
    wnd.print();
}

function DownloadFile(Name) {
    var callStackKey = __page.get_CallStackKey();
    var iframe = $('#DownloadIframe');
    if (iframe.length < 1)
        iframe = $('<iframe id="DownloadIframe"/>');

    iframe.attr('src', "DownloadFile.aspx?cskey="+callStackKey+"&retrieveviewdocfile=" + Name);
    iframe.hide();
    iframe.appendTo(document.body);
}

function DownloadMedwatch()
{
    var iframe = document.createElement("iframe");
    
    // Point the IFRAME to GenerateFile, with the
    //   desired region as a querystring argument.
    iframe.src = "ExportToPDF.aspx";

    // This makes the IFRAME invisible to the user.
    iframe.style.visibility = "hidden";

    // Add the IFRAME to the page.  This will trigger
    //   a request to GenerateFile now.
    document.body.appendChild(iframe);
}

function DoResetDataPostback(clearData, data, controlID, eventTarget)
{
    var eventArgument = clearData + ":" + data + ":" + controlID;
    __page.postback(eventTarget, eventArgument);

    $get('__EVENTARGUMENT').value = '';
} //DoResetDataPostback

function StarAndStopDrag()
{
    $('.WebPartCatalogItem').draggable(
        {
            start: function(event, ui)
            {
                $('.WebPartCatalogItem').css({ 'position': 'static' });
                ui.helper[0].style.position = "absolute";
            },
            stop: function(event, ui)
            {
                ui.helper[0].style.position = "static";
                $('.WebPartCatalogItem').draggable({ revert: true });
                $('.WebPartCatalogItem').css({ 'position': 'static' });
            }
        });
}

// Function to cancel the list of keys pressed by the user
function CancelKeyPress(e) 
{
    var disabledKey = false;

    if (!e) e = event;
    var isPossibleTextElement = jQuery.grep(['DIV', 'INPUT', 'TEXTAREA'], function (a) { return a == (!e.target ? event.srcElement.tagName : e.target.tagName); }).length > 0;
    var targetElement = (!e.target ? event.srcElement : e.target);

    gkKeyCode = e.keyCode;
    var t = getCEP_top();
    for (var i = 0; i < t.CancelKeyPressList.length; i++) 
    {
        var splitItem = t.CancelKeyPressList[i].split('+');
        if (splitItem.length == 1) 
        {
        	if (gkKeyCode == t.CancelKeyPressList[i]) 
            {
                // Check to see if it is the backspace
            	if ((t.CancelKeyPressList[i] == gkBackSpaceKeyCode) && isPossibleTextElement) 
                {
                    // Do not cancel the backspace if the user 
                    // is in a text box, textarea, text editor area or password element.
                    if ((!IsTextElement(targetElement)) && (!IsTextAreaElement(targetElement))
                        && (!IsFileElement(targetElement)) && (!IsPasswordElement(targetElement))
                        && (!IsURLElement(targetElement)))
                    {
                        disabledKey = true;
                    } // if
                    else 
                    {
                        // Do not cancel the backspace if the user 
                        // is in a text box, textarea or password element
                        // and it`s ReadOnly property is not set to true.
                        if (IsReadOnly(targetElement)) 
                        {
                            disabledKey = true;
                        }
                    }
                }
                else 
                {
                    disabledKey = true;
                } // if else
            } // if
        }
        else if (splitItem.length == 2) 
        {
            // The alt key
            if (splitItem[0] == gkAltKeyCode) 
            {
            	if (event.altKey) 
                {
                	if (gkKeyCode == splitItem[1]) 
                    {
                        disabledKey = true;
                    } // if
                } // if
            } // if
        } // if else

        // Inform the user of the key being disabled
        if (disabledKey) 
        {
        	gkKeyCode = 0;
        	if (window.event) event.returnValue = false; //IE
        	else e.preventDefault();//FF

            if (t.PortalLblDisabledKey != null)
                alert(t.PortalLblDisabledKey);
            else
                alert(mDisabledKeyAlert);
            return false;
        } // if
    } // for
} // CancelKeyPress

// Determines if we are currently on a text element.
function IsTextElement(el) 
{
    if (el.tagName == "INPUT" && el.type.toLowerCase() == "text") {
        return true;
    }

    return false;
} //IsTextElement()

// Determines if we are currently on a text area element (multi-line text box).
function IsTextAreaElement(el) 
{
    var retVal;
    var srcElementTagName = el.tagName.toLowerCase();
    if (srcElementTagName == "textarea")
        retVal = true;
    else 
    {
        // Spectial test for WebHtmlEditor control
        if (srcElementTagName == "div" && el.getAttribute("contentEditable"))
            retVal = true;
        else
            retVal = false;
    }
    return retVal;
}  //IsTextAreaElement()

// Determines if we are currently on a file element.
function IsFileElement(el) 
{
    if (el.tagName == "INPUT" && el.type.toLowerCase() == "file") {
        return true;
    }

    return false;
}  //IsFileElement()

// Determines if we are currently on a password element.
function IsPasswordElement(el) 
{
    if (el.tagName == "INPUT" && el.type.toLowerCase() == "password") {
        return true;
    }

    return false;
}  //IsPasswordElement()

// Determines if we are currently on a URL element.
function IsURLElement(el) {
    if (el.tagName == "INPUT" && el.type.toLowerCase() == "url") {
        return true;
    }

    return false;
}

//Determines if the current element`s readOnly property is set to true
function IsReadOnly(el) 
{    
    var retVal = false;
    var TrueReadOnly = true;
    var srcElementReadOnly = el.readOnly;
    if (srcElementReadOnly == TrueReadOnly)
        retVal = true;
    return retVal;
} //IsReadOnly

function OpenDataCollectionExportWindow(headerText, titleText, CSVText, excelText)
{
    if (document.getElementById("dialog-form") == null)
    {
        var div = document.createElement("div");
        div.id = "dialog-form";
        div.title = headerText;

        document.body.appendChild(div);

        var divTitle = document.createElement("div");
        divTitle.innerHTML = titleText;
        div.appendChild(divTitle);

        div.appendChild(document.createElement("br"));
        div.appendChild(document.createElement("br"));

        var fieldSet = document.createElement("fieldset");
        div.appendChild(fieldSet);

        var input = document.createElement("input");
        input.type = "radio"
        input.name = "Export"
        input.value = "2";
        input.id = "ExportToCSV";
        input.checked = true;
        fieldSet.appendChild(input);

        var label = document.createElement("label");
        label.innerHTML = CSVText;
        fieldSet.appendChild(label);

        label = document.createElement("label");
        label.innerHTML = "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;";
        fieldSet.appendChild(label);

        input = document.createElement("input");
        input.type = "radio"
        input.name = "Export"
        input.value = "1";
        input.id = "ExportToXML";
        fieldSet.appendChild(input);

        label = document.createElement("label");
        label.innerHTML = excelText;
        fieldSet.appendChild(label);
    }
    else
    {
        if (document.getElementById("ExportToCSV") != null)
        {
            document.getElementById("ExportToCSV").checked = true;
        }
    }

    $("#dialog-form").dialog({
        autoOpen: false,
        height: 200,
        width: 400,
        modal: true,
        buttons: {
            "Export": function ()
            {
                var eventArgument;
                if ($("#ExportToXML")[0].checked)
                {
                    eventArgument = "ExportDataCollection:0";
                }
                else if ($("#ExportToCSV")[0].checked)
                {
                    eventArgument = "ExportDataCollection:1";
                }

                var eventTarget = "__Page";
                __page.postback(eventTarget, eventArgument);

                $get('__EVENTARGUMENT').value = '';
                $(this).dialog("close");
            },
            Cancel: function ()
            {
                $("#dialog-form").dialog("close");
            }
        }
    });

    $("#dialog-form").dialog("open");
}

// handles keypress
function SmartScanResolver(e) {
    if (typeof (smartScanProc) != "undefined") {
        return smartScanProc.ProcessKeyPress(e.charCode != null && e.charCode != 0 ? e.charCode : e.keyCode);
    }
    return true;
}

// handles keydown
function SmartScanTabResolver(e) {
    if (typeof (smartScanProc) != "undefined" && e.keyCode == 9) {
        return smartScanProc.ProcessKeyPress(e.keyCode);
    }
    return true;
}

function SmartScanTemplate(_controlId, _prefix, typeName) {
    this.controlId = _controlId;
    this.prefix = _prefix;
    this.typeName = typeName;
    this.zIndex = 0;

    this.Setup = function () {
        this.zIndex = GetTabIndex(_controlId);
    };

    this.GetClearValue = function (inputText) {
        // Remove preamble and prefix
        if (inputText.indexOf(this.prefix) == 1)
            return inputText.substring(this.prefix.length + 1, inputText.length);
        else
            return null;
    };

    this.IsMatched = function (inputText) {
        // The first char is preamble
        if (inputText.indexOf(this.prefix) == 1)
            return true;
        else
            return false;
    };

    this.IsAvailable = function () {
        var element = this.GetInputElement();
        return element && element.checkVisibility() && !element.disabled;
    }
    this.GetInputElement = function () {
        var inp = document.getElementById(this.controlId);
        if (inp) {
            if (!inp.disabled && inp.style.display != "none" && !inp.readOnly) {
                return inp;
            }
        }
        return null;
    };

    this.PopulateValue = function (inputText) {
        document.getElementById(this.controlId).value = inputText;
        $("#" + this.controlId).change();
        $("#" + this.controlId).trigger('smartScanningChange');
    };     //PopulateValue

    this.GetValue = function () {
        var element = this.GetInputElement();
        var val = element ? element.value : "";
        return val;
    };
    // set a specified value without requiring multiple DOM traversals to find the element.
    this.SetValue = function (inputText) {
        var element = this.GetInputElement();
        if (element) {
            element.value = inputText;
            $(element).change().trigger('smartScanningChange');
        }
    };
}

function SmartScanProcessor(_delimiters, _preambleChar, _defaultErrorLabel, smartScanRuleName, smartScanPatterns) {
    this.defaultErrorLabel = _defaultErrorLabel;
    this.smartScanRuleName = smartScanRuleName;
    this.smartScanPatterns = smartScanPatterns;
    this.isTypeScanner = false;
    this.delimiters = _delimiters;
    this.preambleChar = _preambleChar;
    this.templates = new Array();
    this.templatesCount = 0;
    this.inputBuffer = "";
    this.inputStarted = false;
    this.isSetupDone = false;

    this.AddTemplate = function (_controlId, _prefix, typeName) {
        var add = true;
        for (var j = 0; j < this.templatesCount; j++) {
            if (this.templates[j].controlId === _controlId) {
                add = false;
                break;
            }
        }
        if (add) {
            var templ = new SmartScanTemplate(_controlId, _prefix, typeName);
            this.templates[this.templatesCount++] = templ;
        }
    };

    this.Setup = function () {
        for (var i = 0; i < this.templatesCount; i++) {
            this.templates[i].Setup();
        }

        // sort templates
        var tempTemplate = null;
        for (var j = 0; j < this.templatesCount; j++) {
            for (var k = 0; k < this.templatesCount - j - 1; k++) {
                if (this.templates[k].Index > this.templates[k + 1].Index) {
                    tempTemplate = this.templates[k];
                    this.templates[k] = this.templates[k + 1];
                    this.templates[k + 1] = tempTemplate;
                }
            }
        }
    };

    this.GetMatchedTemplates = function () {
        if (!this.isSetupDone) {
            this.Setup();
            this.isSetupDone = true;
        }

        var count = 0;
        var tmplMatched = new Array();
        for (var i = 0; i < this.templates.length; i++) {
            var template = this.templates[i];
            if (template.IsMatched(this.inputBuffer)) {
                tmplMatched[count++] = template;
            }
        }
        return tmplMatched;
    };

    this.FindTargetTemplate = function (_matchedTemplates) {
        var template = null;
        if (_matchedTemplates.length == 1) {
            if (_matchedTemplates[0].GetInputElement()) {
                template = _matchedTemplates[0];
            }
        }
        else {
            var index = GetTabIndex(null); // Get tabindex of focused element
            var activeCtrl = document.activeElement;
            if (activeCtrl.type != null) {
                if (activeCtrl.style.zIndex == 0 && activeCtrl.type != "text") {
                    var ctrl = GetParentElement(null);
                    if (!activeCtrl.nextSibling) {
                        if (activeCtrl.previousSibling) {
                            var tmpCtrl = activeCtrl.previousSibling;
                            if (tmpCtrl.id != null) {
                                var tempIndex = GetTabIndex(null);
                                if (tempIndex <= index) {
                                    var nextCtrl = ctrl.nextSibling;
                                    while (nextCtrl) {
                                        if (nextCtrl.style && nextCtrl.style.zIndex != "") {
                                            index = nextCtrl.style.zIndex;
                                            break;
                                        }
                                        nextCtrl = nextCtrl.nextSibling;

                                    }
                                }
                            }
                        }
                    }
                }
            }

            var isFindTemplate = false;
            for (var i = 0; i < _matchedTemplates.length; i++) {
                if (_matchedTemplates[i].zIndex >= index) {
                    if (!_matchedTemplates[i].GetInputElement()) {
                        continue;
                    }
                    template = _matchedTemplates[i];
                    isFindTemplate = true;
                    break;
                }

            }

            if (template == null) {
                if (_matchedTemplates[0].GetInputElement()) {
                    template = _matchedTemplates[0];
                }
            }
        }
        return template;
    };

    this.Resolve = function () {
        var template = null;
        var _matchedTemplates = this.GetMatchedTemplates();
        if (_matchedTemplates.length > 0) {
            template = this.FindTargetTemplate(_matchedTemplates);
        }
        else {
            setTimeout("alert('" + this.defaultErrorLabel + "')", 100);
        }

        if (template) {
            if (template.GetInputElement()) {
                var valueText = template.GetClearValue(this.inputBuffer);
                template.PopulateValue(valueText);
            }
        }
    };

    this.ProcessKeyPress = function (keyCode) {
        var c = String.fromCharCode(keyCode);

        if (c != null && c != "") {
            if (c == this.preambleChar) {
                // begin input capture of scanned barcode.
                this.isTypeScanner = true;

                this.inputStarted = true;
                this.inputBuffer = c;
                //If it is necessary to see the typing string uncomment the following code
                //window.status = this.inputBuffer;

                return false;
            }
            else if (String.fromCharCode(keyCode) == this.delimiters) {
                if (this.isTypeScanner) {
                    // end input capture and process barcode.
                    if (this.smartScanPatterns) { 
                        this.applySmartScanRule();
                        return false;
                    } else {
                        // process with configured smart tags in templates
                        //Parse the input buffer then resolve each value
                        var mas = [];
                        for (var i = 0; i < this.templates.length; i++) {
                            var n = this.inputBuffer.indexOf(this.templates[i].prefix);
                            if (n > 0) mas.push(n);
                        }

                        if (mas.length > 1) {
                            var fullstring = this.inputBuffer;
                            mas.push(fullstring.length)
                            mas.sort(function (a, b) {
                                return a - b;
                            });
                            for (var i = 1; i < mas.length; i++) {
                                //Process each value skipping the preamble
                                this.inputBuffer = this.preambleChar + fullstring.substring(mas[i - 1], mas[i]);
                                this.Resolve();
                            }
                        } else this.Resolve();

                        //If it is necessary to see the typing string uncomment the following code
                        //window.status = this.inputBuffer;
                        this.isTypeScanner = false;
                        this.inputBuffer = "";
                        if (keyCode == 9)
                            return true;
                        else
                            return false;
                    }
                }
                return true;
            }
            else {
                if (this.inputStarted) {
                    this.inputBuffer += c;
                    //If it is necessary to see the typing string uncomment the following code
                    //window.status = this.inputBuffer;
                    if (this.isTypeScanner) {
                        return false;
                    }
                }
                else
                    return true;
            }
        }
    };

    this.getContainerOrIdentifier = function(response) {
        var result = this.getSmartScanType(response, "Container");
        if (!result)
            result = this.getSmartScanType(response, "ES_Identifier");
        if (!result)
            result = this.getSmartScanType(response, "Identifier");
        return result;
    }

    this.getSmartScanType = function(response, typeName) {
        var result = "";
        if (response.ParseBarcodeResult.IsSuccess && response.bcValues.length > 0) {
            response.bcValues.forEach(function (bcValue) {
                if (bcValue.Key === typeName)
                    result = bcValue.Value;
            });
        }
        return result;
    }

    this.checkSmartScanRule = function (scannedBarcode, callback) {
        if (scannedBarcode && scannedBarcode.length > 0) {
            // captured barcode includes preamble as first char, remove it for rule based processing
            var preamble = scannedBarcode[0];
            if (this.preambleChar == preamble) {
                var barcode = scannedBarcode.substring(1);
                var barcode1 = barcode.replace(//g, "\\030");
                var barcode2 = barcode1.replace(//g, "\\029").replace(//g, "");
                var data = '{ "barcode": "' + barcode2 + '", "patterns": ' + this.smartScanPatterns + ' }';
                // at this point any char that needs escaping will have a single backslash(barcode or patterns).
                // to communicate via ajax with the parser assembly, we need to escape that backslash.
                data = data.replace(/\\/g, '\\\\');

                $.ajax({
                    type: "POST",
                    dataType: "json",
                    url: './SmartScanService.svc/web/ParseBarcode',
                    headers: {
                        'Accept': 'application/json'
                    },
                    // Must set content-type this way to avoid jQuery bug with sending JSON containing "??"
                    // https://forum.jquery.com/topic/special-characters-issue-find-random-strings-like-jquery20206329934545792639-1415046914457-in-data
                    contentType: "application/json;charset=UTF-8",
                    async: true,
                    data: data,
                    context: document.body
                }).success(callback).fail(this.parseFail);
            } else
                callback(null);
        } else
            callback(null);
    }
    this.parseFail = function(response) {
        if (response) {
            // the response should have meaningful error info in it. dump the details to the console and show a user friendly message to look there.
            showErrorLabel('SmartScanServiceError', response.responseText);
        } else {
            // shouldn't happen but just in case
            showErrorLabel('UTILITY_ERR_UNKNOWN_ERROR');
        }
        this.finishScan();
    }

    // shows a message in an alert box for the specified label
    function showErrorLabel(labelName, consoleMessage) {
        this.getLabel(labelName, function (result) {
            this.showError(result.labelValue, consoleMessage);
        });
    }

    // get a single label value
    this.getLabel = function(labelName, callback) {
        this.getLabels([labelName], function (result) {
            if (callback) {
                result.labelValue = result.error ? labelName : result.labels[labelName];
                callback(result);
            }
        });
    }

    // gets multiple label values
    this.getLabels = function(labelNames, callback) {
        var requestedLabels = [];
        var request;

        labelNames.forEach(function (labelName) {
            requestedLabels.push({ Name: labelName });
        });

        __page.getLabels(requestedLabels, function (response) {
            var result = {
                labels: {},
                error: ''
            };

            if (Array.isArray(response)) {
                response.forEach(function (label) {
                    result.labels[label.Name] = label.Value ? label.Value : label.DefaultValue;
                });
            } else {
                result.error = response.Error;
                console.error('Error getting labels for: ' + JSON.stringify(labelNames) + '. Message: ' + response.Error);
            }

            if (callback) {
                callback(result);
            }
        });
    }
    // shows a message in an alert box and logs it to the console
    this.showError = function(popupMessage, consoleMessage) {
        popupMessage = popupMessage || smartProc.defaultErrorLabel;
        setTimeout("alert('" + popupMessage + "')", 100);

        consoleMessage = consoleMessage || popupMessage;
        console.error(consoleMessage);
    }

    this.endScan = function() {
        this.isTypeScanner = false;
        this.inputBuffer = "";
        if (this.delimiters === '\t') {
            //TODO: non-rule based smart scan returns true to allow default behavior of tab key.
            //      rule based cannot do that since it needs an async call.
            //      may have to do something here like trigger a tab key press, but on what element?.
            //      could we get from focus? wait and see what problems arise(if any).
        }
    }

    // parses the captured barcode using the active smart scan rule
    this.applySmartScanRule = function (callback) {
        var successCallback = typeof callback !== 'undefined' ? callback : parseSuccess;
        var smartProc = this; // remove 'this' confusion inside helper functions.

        // captured barcode includes preamble as first char, remove it for rule based processing        
        var barcode = this.inputBuffer.substring(1);
        var barcode1 = barcode.replace(//g, "\\030");
        var barcode2 = barcode1.replace(//g, "\\029").replace(//g, "");
        var data = '{ "barcode": "' + barcode2 + '", "patterns": ' + this.smartScanPatterns + ' }';
        // at this point any char that needs escaping will have a single backslash(barcode or patterns).
        // to communicate via ajax with the parser assembly, we need to escape that backslash.
        data = data.replace(/\\/g, '\\\\');

        $.ajax({
            type: "POST",
            dataType: "json",
            url: './SmartScanService.svc/web/ParseBarcode',
            headers: {
                'Accept': 'application/json'
            },
            // Must set content-type this way to avoid jQuery bug with sending JSON containing "??"
            // https://forum.jquery.com/topic/special-characters-issue-find-random-strings-like-jquery20206329934545792639-1415046914457-in-data
            contentType: "application/json;charset=UTF-8",
            async: true,
            data: data,
            context: document.body
        }).success(successCallback).fail(this.parseFail);

        /*function parseFail(response) {
            if (response) {
                // the response should have meaningful error info in it. dump the details to the console and show a user friendly message to look there.
                showErrorLabel('SmartScanServiceError', response.responseText);
            } else {
                // shouldn't happen but just in case
                showErrorLabel('UTILITY_ERR_UNKNOWN_ERROR');
            }
            finishScan();
        }*/

        function parseSuccess(response) {
            var errorMessage;
            var anyDataSet = false;
            var action = null;

            if (response.ParseBarcodeResult.IsSuccess) {
                if (response.bcValues.length > 0) {
                    // iterate extracted values and set to all controls with matching data types.
                    console.log('Parse Results: ' + JSON.stringify(response.bcValues));
                    response.bcValues.forEach(function (bcValue) {
                        //console.log('Result Key: '+ bcValue.Key + ', Value: ' + bcValue.Value);
                        var matchingTemplates = getMatchingTemplates(bcValue.Key);
                        anyDataSet = anyDataSet || matchingTemplates.length > 0;
                        matchingTemplates.forEach(function (template) {
                            if (bcValue.Key === 'Action') {
                                // can only handle one action at a time. get first and ignore others.
                                if (action === null) {
                                    action = bcValue.Value.split("\t");
                                }
                            } else if (template.IsAvailable()) {
                                var existVal = template.GetValue();
                                if (existVal && existVal.length > 0 && matchingTemplates.length > 1) {
                                    console.log('Control Id: ' + template.controlId +' already has Value(' + existVal + ') - check for other controls with same key: ' + bcValue.Key);
                                } else {
                                    console.log('Setting Value(' + bcValue.Value + ') to control with id: ' + template.controlId);
                                    template.SetValue(bcValue.Value);
                                }
                            }
                        });
                    });

                    if (action) {
                        var $btn;
                        const actionId = action[0];
                        //const actionParam = action[1];
                        const isCommandBar = action[2] === 'True';

                        // javascript function?
                        if (typeof (CR.SmartScan[actionId]) === 'function') {
                            CR.SmartScan[actionId].apply(this, response.bcValues);
                        } else if (isCommandBar) {
                            // for command bar items find by caption text.
                            $btn = $('.caption-text:contains(' + actionId + ')');
                        } else {
                            // first try to find by css selector. handle with try/catch in case of invalid selector like if actionId is "Move...".
                            try {
                                $btn = $(actionId);
                            } catch (ex) {
                                console.log('"' + actionId + '" is not a valid CSS selector. Searching for matching label...');
                                $btn = $([]);
                            }

                            // try matching on title of input element
                            if ($btn.length === 0) {
                                $btn = $('input[title=\"' + actionId + '\"]');
                            }

                            // try matching title on a span or other element that is a parent of the input element
                            if ($btn.length === 0) {
                                $btn = $('[title=\"' + actionId + '\"] > input');
                            }

                            // try matching the defaultlabel attribute of an element
                            if ($btn.length === 0) {
                                $btn = $('[defaultlabel=\"' + actionId + '\"]');
                            }
                        }

                        if ($btn) {
                            // if we found more than one, try to eliminate all not visible
                            if ($btn.length > 1) {
                                $btn = $btn.filter(':visible');
                            }

                            if ($btn.length === 1) {
                                console.log("Found the Action element with Action Identifier: " + actionId);

                                //check for onclick handler. 
                                var ev = $._data($btn, 'events');

                                if ((ev && ev.click) || ($btn.attr("onClick") !== undefined)) {
                                    $btn.click();
                                } else {
                                    // sidebar buttons are handled by the sidebar control. there are no click handlers on the menu items.
                                    console.log("OnClick event is not defined on Action element. Triggering click in case of higher level handler.");
                                    $btn.click();
                                }
                            } else if ($btn.length === 0) {
                                console.log("Action element not found:" + actionId);
                            } else if ($btn.length > 1) {
                                console.log("More than one Action element found: " + actionId + "; Number elements found:" + $btn.length);
                            }
                        }
                    } else if (!anyDataSet) {
                        console.warn("Template Names: " + getAllTemplateNames());
                        // data was extracted from the barcode, but didn't match any configured fields.
                        getLabel('SmartScanDataNoMatchFields', function (result) {
                            showError(result.labelValue.replace('#ErrorMsg.Name', smartProc.smartScanRuleName));
                        });
                    }

                } else {
                    console.warn("Template Names: " + getAllTemplateNames());
                    // no parser errors, but no data extracted from barcode.
                    getLabel('SmartScanNoDataFound', function (result) {
                        showError(result.labelValue.replace('#ErrorMsg.Name', smartProc.smartScanRuleName));
                    });
                }
            } else if (response.localizedErrMsg) {
                showError(response.localizedErrMsg);
            } else if (response.ParseBarcodeResult.ExceptionData) {
                showError(response.ParseBarcodeResult.ExceptionData.Description);
            } else {
                // shouldn't happen but just in case
                showErrorLabel('UTILITY_ERR_UNKNOWN_ERROR');
            }
            finishScan();
        }

        // find the templates whose typeName matches a smart scan pattern type.
        function getMatchingTemplates(type) {
            var matchingTemplates = [];
            smartProc.templates.forEach(function (template) {
                if (template.typeName === type)
                    matchingTemplates.push(template);
            });
            return matchingTemplates;
        }

        function getAllTemplateNames() {
            var names = "";
            smartProc.templates.forEach(function (template) {
                if (names === "")
                    names = template.typeName;
                else
                    names += "," + template.typeName;
            });
            return names;
        }
        // since rule based smart scanning is async, delay final cleanup until it is all done.
        function finishScan() {
            smartProc.isTypeScanner = false;
            smartProc.inputBuffer = "";
            if (smartProc.delimiters === '\t') {
                //TODO: non-rule based smart scan returns true to allow default behavior of tab key.
                //      rule based cannot do that since it needs an async call.
                //      may have to do something here like trigger a tab key press, but on what element?.
                //      could we get from focus? wait and see what problems arise(if any).
            }
        }

        // shows a message in an alert box and logs it to the console
        function showError(popupMessage, consoleMessage) {
            popupMessage = popupMessage || smartProc.defaultErrorLabel;
            setTimeout("alert('" + popupMessage + "')", 100);

            consoleMessage = consoleMessage || popupMessage;
            console.error(consoleMessage);
        }

        // shows a message in an alert box for the specified label
        function showErrorLabel(labelName, consoleMessage) {
            getLabel(labelName, function (result) {
                showError(result.labelValue, consoleMessage);
            });
        }

        // get a single label value
        function getLabel(labelName, callback) {
            getLabels([labelName], function (result) {
                if (callback) {
                    result.labelValue = result.error ? labelName : result.labels[labelName];
                    callback(result);
                }
            });
        }

        // gets multiple label values
        function getLabels(labelNames, callback) {
            var requestedLabels = [];
            var request;

            labelNames.forEach(function (labelName) {
                requestedLabels.push({ Name: labelName });
            });

            __page.getLabels(requestedLabels, function (response) {
                var result = {
                    labels: {},
                    error: ''
                };

                if (Array.isArray(response)) {
                    response.forEach(function (label) {
                        result.labels[label.Name] = label.Value ? label.Value : label.DefaultValue;
                    });
                } else {
                    result.error = response.Error;
                    console.error('Error getting labels for: ' + JSON.stringify(labelNames) + '. Message: ' + response.Error);
                }

                if (callback) {
                    callback(result);
                }
            });    
        }
    };
}

function GetParentElement(ctrlId) {
    var currTabindex = 0;
    var theControl = ctrlId ? document.getElementById(ctrlId) : document.activeElement;
    if (theControl) {
        currTabindex = theControl.style.zIndex;
        while (!currTabindex && theControl.parentElement) {
            currTabindex = theControl.parentElement.style.zIndex;
            theControl = theControl.parentElement;
        }
    }

    return theControl;
}

function GetTabIndex(ctrlId) {
    var currTabindex = 0;
    var theControl = ctrlId ? document.getElementById(ctrlId) : document.activeElement;
    if (theControl) {
        currTabindex = theControl.style.zIndex;
        while (!currTabindex && theControl.parentElement) {
            currTabindex = theControl.parentElement.style.zIndex;
            theControl = theControl.parentElement;
        }
    }
    return currTabindex;
}

function SetupSmartScanningEvents() {
    if (document.body.attachEvent) {
        document.body.attachEvent("onkeypress", SmartScanResolver);
        document.body.attachEvent("onkeydown", SmartScanTabResolver);
    }
    else {
        document.onkeypress = SmartScanResolver;
        document.onkeydown = SmartScanTabResolver;
    }
}

function getTextWidth(el){
    var s = $('<span >'+ $(el).html() +'</span>');
    s.css({
       position : 'absolute',
       left : -9999,
       top : -9999,
       // ensure that the span has same font properties as the element
       'font-family' : el.css('font-family'),
       'font-size' : el.css('font-size'),
       'font-weight' : el.css('font-weight'),
       'font-style' : el.css('font-style')
    });
    $('body').append(s);
    var result = s.width();
    //remove the newly created span
    s.remove();
    return result;
}

function setDirtyFlag() {
    __page.setDirty();
}

function ConfirmTabClosing($li, callback)
{
    var caption = $('span:first', $li).text();
    var labels = [{ Name: 'Lbl_UnsavedChangesOnVirtualPage' }, { Name: 'Lbl_Warning' }];
    __page.getLabels(labels, function (response) {
        if ($.isArray(response)) {
            var value;
            var warningLbl;
            $.each(response, function () {
                var labelName = this.Name;
                var labelText = this.Value;
                switch (labelName)
                {
                    case 'Lbl_UnsavedChangesOnVirtualPage':
                        value = labelText.replace('{0}', caption);
                        break;
                    case 'Lbl_Warning':
                        warningLbl = labelText;
                        break;
                    default:
                        break;
                }
            });            
            JConfirmationLong(value, null, callback, null, null, null, warningLbl);
        }
        else {
            alert(response.Error);
        }
    });    
}

function removeTab($li) {
    if (typeof $li == 'string')
        $li = $('li[aria-controls="' + $li + '"]');

    if ($li.length)
        $li.closest('.ui-page-tab').scrollableTabs("remove", null, $li);
}

function GetChildTabs($li)
{
    var id = $li.attr('aria-controls');
    var $divContent = $('div[id="' + id + '"]', $li.parent().parent());
    var $ifr = $divContent.find('iframe');
    if ($ifr.length)
        return $ifr.contents().find(".ui-page-tab").find('li');
    else
        return [];
}

function getIframe(src, win)
{
    if (win === undefined)
        win = top;

    if (win.document.URL == src)
    {
        return win;
    }

    for (var i = 0; i < win.frames.length; i++)
    {
        if (win.frames[i].document.URL == src)
        {
            return win.frames[i];
        }
        else
        {
            var w = getIframe(src, win.frames[i]);
            if (w != null)
                return w;
        }
    }
    return null;
}

function removeIframe($iframe)
{
    if ($iframe.length > 0)
    {
        $iframe[0].src = "about:blank";
        if ($iframe[0].contentWindow)
        {
            $iframe[0].contentWindow.document.write("");
            $iframe[0].contentWindow.close();
        }
        else if ($iframe[0].contentDocument)
            $iframe[0].contentDocument.write("");
        $iframe.remove();
    }
}

function getTextHeight(el, width)
{
    var s = $('<span >' + $(el).html() + '</span>');
    s.css({
        position: 'absolute',
        left: -9999,
        top: -9999,
        width: width,
        // ensure that the span has same font properties as the element
        'font-family': el.css('font-family'),
        'font-size': el.css('font-size'),
        'font-weight': el.css('font-weight'),
        'font-style': el.css('font-style')
    });
    $('body').append(s);
    var result = s.height();
    //remove the newly created span
    s.remove();
    return result;
}

function SerializeObject(obj, indentValue) 
{
    var hexDigits = "0123456789ABCDEF";
    function ToHex(d) 
    {
        return hexDigits[d >> 8] + hexDigits[d & 0x0F];
    }
    function Escape(string) 
    {
        return string.replace(/[\x00-\x1F'\\]/g,
        function (x) 
        {
            if (x == "'" || x == "\\") return "\\" + x;
            return "\\x" + ToHex(String.charCodeAt(x, 0));
        })
    }
    var indent;
    if (indentValue == null) 
    {
        indentValue = "";
        indent = ""; // or " "
    }
    else 
    {
        indent = "\n";
    }
    return GetObject(obj, indent).replace(/,$/, "");
    function GetObject(obj, indent)
    {
        if (typeof obj == 'string') 
        {
            return "'" + Escape(obj) + "',";
        }
        if (obj instanceof Array) 
        {
            result = indent + "[";
            for (var i = 0; i < obj.length; i++) 
            {
                result += indent + indentValue + GetObject(obj[i], indent + indentValue);
            }
            result += indent + "],";
            return result;
        }
        var result = "";
        if (typeof obj == 'object') 
        {
            result += indent + "{";
            for (var property in obj) 
            {
                result += indent + indentValue + "'" +
            Escape(property) + "' : " +
            GetObject(obj[property], indent + indentValue);
            }
            result += indent + "},";
        }
        else 
        {
            result += obj + ",";
        }
        return result.replace(/,(\n?\s*)([\]}])/g, "$1$2");
    }
}

function CleanupSession(stackKey)
{
    if (stackKey)
    {
        var loc = window.location;
        var appPath = loc.pathname.substr(0, loc.pathname.indexOf('/', 1));
        var url = loc.protocol + "//" + loc.host + appPath + "/SessionHandler.ashx" + "?keyId=" + stackKey;
        //to prevent ajax request caching in IE you need to have unique request every time
        if (Camstars.Browser.IE)
            url += "&date=" + new Date().getTime();
        // Send ajax request to kill the session
        $.ajax({ url: url })
            .done(function () {
                // Correctly closed
            }).fail(function () {
                console.error("CleanupSessionKeys fails", textStatus);
            });
    }
}

var donotclosesession = false;

function KillSession()
{
    sessionStorage.setItem("menuItems", "");
    sessionStorage.setItem("BackButton", "");
    let settingsStr = sessionStorage.getItem('cep-settings');
    let settings = settingsStr ? JSON.parse(settingsStr) : null;
    if (!donotclosesession && (!settings || !settings.doNotCloseSession)) {
        var loc = window.location;
        var appPath = loc.pathname.substr(0, loc.pathname.indexOf('/', 1));
        var url = loc.protocol + "//" + loc.host + appPath + "/SessionHandler.ashx?kill="+loc.pathname;

        $.ajax({ url: url })
            .done(function () {
                // Correctly closed
            }).fail(function () {
                console.error("CleanupSessionKeys fails", textStatus);
            });
    }
    else {
        donotclosesession = false;
        if (settings) {
            settings.doNotCloseSession = false;
            let settingsStr = JSON.stringify(settings);
            sessionStorage.setItem('cep-settings', settingsStr);
        }
    }
    return false;
}

function getParameterByName(name, url)
{
    if (typeof (url) == 'undefined')
        url = window.location.href;
    name = name.replace(/[\[]/, "\\\[").replace(/[\]]/, "\\\]");
    var regexS = "[\\?&]" + name + "=([^&#]*)";
    var regex = new RegExp(regexS);
    var results = regex.exec(url);
    if (results == null)
        return "";
    else
        return decodeURIComponent(results[1].replace(/\+/g, " "));
}

function onSelectFileUploadTextbox(e, control) {
    
    var fileUploadId = "#" + control.control._inputFieldId;
    $(fileUploadId).click();
    
}

function setParameterByName(name, newVal, url)
{
    var retVal = "";
    if (url == null || url == '')
    {
        if (newVal)
            retVal = "?" + name + "=" + newVal;
    }
    else if (!newVal)
        retVal = url;
    else
    {
        var param = getParameterByName(name, url);
        if (param)
        {
            var segments = url.split("?");
            var pageName = "";
            var query;
            if (segments.length == 2)
            {
                pageName = segments[0];
                query = segments[1];
            }
            else
            {
                query = segments[0];
            }
            var oldParam = name + "=" + param;
            var newParam = name + "=" + newVal;
            query = query.replace(oldParam, newParam);
            retVal = pageName + "?" + query;
        }
        else
            retVal = url + "&" + name + "=" + newVal;
    }
    return retVal;
}

function isVerticalScrollDisplayed()
{
    return $get("scrollablepanel").scrollHeight > $('#scrollablepanel').height();
}

function buildTopMenu($el) {
    var speed = 300;
    if ($el.length) {
		$el.find("li.opened").has("ul").children("ul").addClass("collapse in");
        $el.find("li").not(".opened").has("ul").children("ul").addClass("collapse");
        $el.find("li[onclick]").on("click", function(e) {
            $('.opened').removeClass("opened").children("ul.in").removeClass("in").hide(speed);
            $el.parent().collapse("hide");            
	    });

        $el.find("li").has("ul").children("a").on("click", function(e) {
		    e.preventDefault();
		    var li = $(this).parent("li");
			if (li.hasClass("opened")) {
			    li.removeClass("opened").children("ul.in").removeClass("in").hide(speed);
			}
			else {
			    li.addClass("opened").children("ul").addClass("in").show(speed);
			    li.siblings().removeClass("opened").children("ul.in").removeClass("in").hide(speed);
			}
		});		
	}
}

function onLoadTabMasterPage()
{
    if (Camstars.Browser.IE)
        window.onunload = function (evt) { KillSession(); }
    else
        window.onbeforeunload = function (evt) { KillSession(); }
}

function getCEP_top() {
    var t = window;
    for (var i = 0; i < 10; i++) { // max of 10 iterations
        if (/Main.aspx/i.test(t.location.pathname) && ! /redirectToPageFlow/i.test(t.location.search)) {
            return t;
        }
        else {
            t = t.parent;
        }            
    }
    return t;
}

function getRevisionDelimiter() {
    return top.revDelimiter || ":";
}


function top_resize() {
    $(getCEP_top()).trigger("resize", ["force-top-resize"]);
}


function openPortalStudio(primaryVersion) {
    donotclosesession = true;
    $.ajax({
        type: "POST",
        dataType: "json",
        url: './ApolloPortalService.svc/web/GetApolloSettings',
        headers: {
            'Accept': 'application/json'
        },
        contentType: "application/json;charset=UTF-8",
        async: true,
        context: document.body
    }).success(parseSuccess).fail(false);
    function parseSuccess(response) {
        if (response.GetApolloSettingsResult.IsSuccess) {

            var isSettingsAllowed = response.settings.IsSettingsAllowed;
            var isPortalStudioAccess = response.settings.PortalStudioAccess;
            if (isSettingsAllowed) {
                if (isPortalStudioAccess) {
                    if (isSSO) {
                        location.href = "PortalStudio/index.html?portalMode=Apollo&sso=true&portal=default.aspx&wcf=" + escape(top.wcfUrl);
                    }
                    else {
                        location.href = "PortalStudio/index.html?portalMode=Classic&portal=default.aspx&wcf=" + escape(top.wcfUrl);
                    }

                }
            }
            else {
                setTimeout("alert('" + "The current user has not been granted the Portal Configuration role.  Please update the user permissions to enable access to this feature." + "')", 100);
            }
        }
    }

    return false;
}

function onChangeTextBox(name) {
    if (!Camstars.Browser.IE) {
        window.__IgnoreNextPostback = window.__IgnoreNextPostback || {};
        if (window.__IgnoreNextPostback[name] === true) {
            delete window.__IgnoreNextPostback[name];
            return false;
        }

        if (event && event.keyCode === 13)
            window.__IgnoreNextPostback[name] = true;
    }
    setTimeout(function () { __doPostBack(name, ''); }, 0)
}
