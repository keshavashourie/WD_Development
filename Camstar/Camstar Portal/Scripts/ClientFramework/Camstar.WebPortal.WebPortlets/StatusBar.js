// Copyright Siemens 2024  

/// <reference path="../MicrosoftAjaxExt.js"/>
/// <reference path="../Camstar.UI/Control.js" />
/// <reference path="../Camstar.WebPortal.PortalFramework/WebPartBase.js" />
/// <reference path="../CamstarPortal.WebControls/HeaderMobileControl.js" />
Type.registerNamespace("Camstar.WebPortal.WebPortlets");

Camstar.WebPortal.WebPortlets.StatusBar = function (element) {
    Camstar.WebPortal.WebPortlets.StatusBar.initializeBase(this, [element]);
    this._errorMessageFadeOutTime = 30000;
    this._infoMessageFadeOutTime = 6000;
    this._warningMessageFadeOutTime = 15000;
    this._playSound = false;
    this._warningSound = null;
    this._successSound = null;
    this._errorSound = null;
	this._message = null;
	this._messageText = null;
	this._messageType = null;	
},


Camstar.WebPortal.WebPortlets.StatusBar.prototype = {
    initialize: function () {
        Camstar.WebPortal.WebPortlets.StatusBar.callBaseMethod(this, 'initialize');
        // Add custom initialization here
    },
    dispose: function () {
        this._playSound = false;
        this._warningSound = null;
        this._successSound = null;
        this._errorSound = null;

		var $mainElement = $(".webpart-status");
		if ($mainElement.css("display") != "none")
			$mainElement.hide();	
			/*
            This would hide any status message displayed in a parent parent
        var $parentStatus = $(".webpart-status", parent.document.body);
		if ($parentStatus.css("display") != "none")
			$parentStatus.hide();
            */     
        Camstar.WebPortal.WebPortlets.StatusBar.callBaseMethod(this, 'dispose');
    },

    get_isStatic: function () {
        return true;
    },

    // Opens status bar
    open: function (useEffect) {
        // get new message
        var $mainElement = $(".webpart-status");

        if ($(".close", $mainElement).length > 0) {
            $(".close", $mainElement).bind("click", function (e) {
                $mainElement.clearQueue().stop(true, true);
                $mainElement.hide();
            });
        }
        else {
            $(".close").bind("click", function (e) {
                $mainElement.clearQueue().stop(true, true);
                $mainElement.hide();
            });
        }

        var messageType = $("div[messagetype]").attr("messagetype");
        //TODO: handle unordered list in warning messages
        var message, messageText;

        let messageElement = $(".message", $mainElement);
        if (messageElement.length > 0) {
            message = messageElement.html();
            let txt = $('#SpanStatusMessage', $mainElement);
            if (txt.length)
                messageText = txt.text();
            else
                messageText = messageElement.text();
        }
        else {
            // get first child div text
            var msgParts = $mainElement.find('ul >li');
            if (msgParts.length > 0) {
                message = '<span class="messageType">' + $(msgParts[0]).text() + '</span>' + '<span>' + $(msgParts[1]).text() + '</span>';
                messageText = $(msgParts[1]).text();
            }
            else {
                message = $mainElement.children('div').text();
                messageText = message;
            }
        }

		this._message = message;
		this._messageText = messageText;
		this._messageType = messageType;
		
		//let parentStatus = $(parent.document).find('#WebPart_StatusBar');
		var page = top.__page || window.__page || parent.__page || getCEP_top().__page;
        var $parentStatus = $(".webpart-status", parent.document.body);
        if (!($parentStatus.length && messageType === "Error" && (document.body.classList.contains('body-float') || $mainElement.closest("body.commandbar-panel").length))) {   //  Do not show error in slideout window
            page.displayMessage(message, messageType);
        } else {
            //      This will hide the message in a "slideout" windo
            //$mainElement.clearQueue().stop(true, true);
            //$mainElement.hide();
        }
		var displaySuccessPopup = true;
		let timeout = this._infoMessageFadeOutTime;
		if (messageType === "Error")
			timeout = this._errorMessageFadeOutTime;
		else if (messageType === "Warning")
            timeout = this._warningMessageFadeOutTime;
        let realTimeout = timeout;
        if (realTimeout < 1000)
            realTimeout *= 1000;
        localStorage.setItem("messageText", messageText);
        localStorage.setItem("messageType", messageType);
		$('#ctl00_WebPartManager_StatusBar_CloseStatusButton').show();
		if ((messageType == "Success" && displaySuccessPopup) || messageType === "Error" || messageType === "Warning") {
			let displayed = false;

            if (window.__page && window.__page._virtualPageName === "SignApproval_VP" && messageType === "Success") {
                var $parentElement = $(parent.document).find('.cs-tab-pages');
                var $iframe = $parentElement.children().eq(-2).find('iframe').first();
                var iframeWindow = $iframe[0].contentWindow;
                $parentStatus = iframeWindow.jQuery('.webpart-status').fadeIn(50).delay(realTimeout).fadeOut(1000);
                $parentStatus.on("click", '.close', function (e) {
                    $parentStatus.hide();
                });
            }
            
			// Duplicate message popup on parent window in case of closing slideout panel
            if (((messageType == "Success" && displaySuccessPopup) || messageType === "Warning" || messageType === "Error") && (document.body.classList.contains('body-float') || $mainElement.closest("body.commandbar-panel").length || getCEP_top().__page.isMobilePage())) {
                if ($parentStatus.length && (messageType == "Success" || messageType === "Error" || $parentStatus.css("display") != "none")) {
                    $parentStatus.attr('messagetype', messageType);
					$parentStatus.html($mainElement.html());
					if ($parentStatus.css("display") != "none") {
						$parentStatus.clearQueue().stop(true, true).show().delay(realTimeout).fadeOut(1000);
					} else
                        $parentStatus.clearQueue().stop(true, true).show().delay(realTimeout).fadeOut(1000);
					$parentStatus.children(".close").bind("click", function (e) {
                        $parentStatus.clearQueue().stop(true, true);
						$parentStatus.hide();
					});
					$parentStatus.find('.message').click(function () {
						$parentStatus.stop(true, true).show();
					});
					displayed = true;
				}
			} else if ($parentStatus.length)
			{
				$parentStatus.attr('messagetype', messageType);
				$parentStatus.html($mainElement.html());
				$parentStatus.children(".close").bind("click", function (e) {
                    $parentStatus.clearQueue().stop(true, true);
					$parentStatus.hide();
				});
				$parentStatus.find('.message').click(function () {
					$parentStatus.stop(true, true).show();
				});
				if ($parentStatus.css("display") != "none")
					$parentStatus.css('z-index', '9999');
			}
			if (!displayed) {
				if ($mainElement.css("display") != "none") {
					$mainElement.clearQueue().stop(true, true).show().delay(realTimeout).fadeOut(1000);
				} else
                    $mainElement.clearQueue().stop(true, true).show().delay(realTimeout).fadeOut(1000);
			}
		}
		else {
			if (useEffect == true) {
				$mainElement.fadeIn(50);
			}
			else {
				$mainElement.show();
			}
			if (document.documentElement.scrollTop > 0) {
				if (($mainElement.offset().top - $mainElement.scrollTop()) < document.documentElement.scrollTop && $mainElement.length === 1)
					$mainElement.scrollTop($mainElement.top);
			}
		}
		//  If user clicks on message, stop the fade - user must then close the message
		$mainElement.find('.message').click(function ()
		{
			$mainElement.stop(true, true).show();
		});

        if (getCEP_top().__page && getCEP_top().__page.isMobilePage()) {
            var headerControl = getCEP_top().$find("NavbarHeader");
            $(".scrollable-panel").scrollTop(0);
            if (headerControl)
                headerControl.addAlert(message, messageType);
        }
            
				
        this.playSound(messageType);
        return false;
    },

    isDisplayed: function () {
        var $mainElement = $(".webpart-status");
        return $mainElement.length && $mainElement.style("display") === "block";
    },

    // Closes status bar
    close: function (useEffect) {
        var mainElem = this.get_element();
        if (useEffect == true)
            $(mainElem).fadeOut(1000);
        else
            $(mainElem).hide();
        return false;
    },

    write: function (message, msgType, caption) {
        var $mainElement = $(".webpart-status");
        var messageControl = $(".message", $mainElement);
        var title = (typeof caption === "undefined") ? msgType.toUpperCase() : caption;

        messageControl.empty();
        messageControl.append('<span class=\"message-status-type\">' + title + '</span><span class=\"instruction noty_text\" id=\"SpanStatusMessage\">' + message + '</span>');
        $("div[messagetype]").attr("messagetype", msgType);
        this.open(true);
    },

    processStatusData: function (sectionData) {
        var msgText = '';
        if (sectionData.Message) {
            msgText += sectionData.Message;
        }
        else {
            for (var i = 0; i < sectionData.ValidationItems.length; i++) {
                msgText += sectionData.ValidationItems[i].Message + "\n";
            }
        }

        this.write(msgText, "Error");
    },

    clear: function () {
        var page = top.__page || window.__page || parent.__page || getCEP_top().__page;
        page.displayMessage('', '');
        $(".webpart-status").hide();
    },

    playSound: function (messageType) {
        if (this._playSound) {
            var path;
            if (messageType == "Success")
                path = this._successSound;
            else if (messageType == "Warning")
                path = this._warningSound;
            else
                path = this._errorSound;
            if (path) {
                var $mainElement = $(".webpart-status");
                $('audio', $mainElement).attr('autoplay', 'autoplay');
                $('audio', $mainElement).attr('src', path);
            }
        }
    },

    get_errorMessageFadeOutTime: function () { return this._errorMessageFadeOutTime; },
    set_errorMessageFadeOutTime: function (value) { if (value > 0) this._errorMessageFadeOutTime = value; },
    get_infoMessageFadeOutTime: function () { return this._infoMessageFadeOutTime; },
    set_infoMessageFadeOutTime: function (value) { if (value > 0) this._infoMessageFadeOutTime = value; },
    get_warningMessageFadeOutTime: function () { return this._warningMessageFadeOutTime; },
    set_warningMessageFadeOutTime: function (value) { if (value > 0) this._warningMessageFadeOutTime = value; },



    get_playSound: function () { return this._playSound; },
    set_playSound: function (value) { this._playSound = value; },
    get_warningSound: function () { return this._warningSound; },
    set_warningSound: function (value) { this._warningSound = value; },
    get_successSound: function () { return this._successSound; },
    set_successSound: function (value) { this._successSound = value; },
    get_errorSound: function () { return this._errorSound; },
    set_errorSound: function (value) { this._errorSound = value; }
},
Camstar.WebPortal.WebPortlets.StatusBar.registerClass('Camstar.WebPortal.WebPortlets.StatusBar', Camstar.WebPortal.PortalFramework.WebPartBase);

if (typeof (Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
