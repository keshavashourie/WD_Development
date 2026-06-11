// Copyright Siemens 2023

// jQuery Alert Dialogs Plugin
//
// Usage:
//      jAlert( message, [title, callback] )
//      jConfirm( message, [title, callback] )
//      jPrompt( message, [value, title, callback] )

(function($) {

    $.alerts = {

        // These properties can be read/written by accessing $.alerts.propertyName from your scripts at any time

        verticalOffset: -75,                // vertical offset of the dialog from center screen, in pixels
        horizontalOffset: 0,                // horizontal offset of the dialog from center screen, in pixels/
        repositionOnResize: true,           // re-centers the dialog on window resize
        overlayOpacity: .01,                // transparency level of overlay
        overlayColor: '#FFF',               // base color of overlay
        draggable: true,                    // make the dialogs draggable (requires UI Draggables plugin)
        okButton: '&nbsp;Yes&nbsp;',        // text for the OK button
        cancelButton: '&nbsp;No&nbsp;',     // text for the Cancel button
        alertOkButton: '&nbsp;OK&nbsp;',    // text for the OK button for alerts
        dialogClass: "dialog",              // if specified, this class will be applied to all dialogs
        _title: "Message",
        _alertType: 'Informational',
        _modal:null,

        // Public methods

        alert: function(message, title, callback) {
            if( title == null ) title = $.alerts._title;
            $.alerts._show(title, message, null, 'alert', function(result) {
                if( callback ) callback(result);
            });
        },

        confirm: function(message, title, callback, alertType) 
        {
            if (title == null) 
                title = $.alerts._title;

            if (alertType != null)
                $.alerts._alertType = alertType;
            
            $.alerts._show(title, message, null, 'confirm', function(result) {
                    if( callback ) callback(result);
                });
        },

        prompt: function(message, value, title, callback) {
        if (title == null) title = $.alerts._title;
            $.alerts._show(title, message, value, 'prompt', function(result) {
                if( callback ) callback(result);
            });
        },

        // Private methods

        _show: function(title, msg, value, type, callback)
        {

            $.alerts._hide();
            $.alerts._overlay('show');
            $.alerts._modal = new CamstarPortal.WebControls.Modal();
            $.alerts._modal.show();

            var okBtnCaption = $.alerts.alertOkButton;

            if (typeof title != "string") {
                // This is an array [title, ok_button_caption]
                okBtnCaption = title[1];
                title = title[0];
            }

            $("BODY").append(
              '<div id="popup_container" type="' + $.alerts._alertType + '">' +
                '<div id="popup_title_container">' +
                    '<span id="popup_title"></span>' +
                '</div>' +
                '<div id="popup_content">' +
                  '<div id="popup_message"></div>' +
                '</div>' +
              '</div>');

            var $container = $("#popup_container");
            if ($.alerts.dialogClass) $container.addClass($.alerts.dialogClass);

            var pos = 'fixed'; 

            $container.css({
                position: pos,
                zIndex: 99999,
                margin: 0
            });

            $("#popup_title", $container).text(title);
            $("#popup_content", $container).addClass("cs-" + type);
            var $msgDiv = $("#popup_message", $container);
            if (typeof msg == "string") {
                $msgDiv.text(msg);
                $container.removeClass("html-content");
                $container.css("min-width", $container.outerWidth());
            }
            else {
                $container.addClass("html-content");
                var $msgFrame = $("<iframe id=\"generalMessageIFrame\" />").prop("src", "Blank.htm")
                    .prop("scrolling", "auto")
                    .prop("seamless", "seamless")
                    .css("display", "block")
                    .css("overflow", "auto");

                $msgDiv.append($msgFrame);
                $msgFrame.load(function () {
                    var $b = $("body", this.contentDocument);
                    $b.append(msg);

                    var $title = $("#popup_title_container", $container);
                    $("#popup_content", $container).height("calc( 100% - " + $title.outerHeight(true) + "px )");
                    var $pmsg = $("#popup_content #popup_message", $container);

                    var hc = $("#popup_panel", $container).outerHeight(true);
                    if (!$(document.body).hasClass("mobile-device")) {
                        hc += parseFloat($pmsg.css("padding-top"));
                        hc += parseFloat($pmsg.css("padding-bottom"));
                    }
                    $pmsg.css("height", "calc( 100% - " + hc + "px )");

                    $(this).width("100%").height("100%");

                    if ($("body", document).hasClass("ie")) {
                        $(this.contentDocument.documentElement).css("height", "100%");
                        $b.css("height", "100%").css("overflow", "auto");
                    }
                });
            }

            $.alerts._reposition();
            $.alerts._maintainPosition(true);

            switch (type)
            {
                case 'alert':
                    $msgDiv.after('<div id="popup_panel"><input class=cs-button type="button" value="' + okBtnCaption + '" id="popup_ok" /></div>');
                    $("#popup_ok", $container).click(function () {
                        $msgDiv.remove();
                        $.alerts._hide();
                        $.alerts._modal.hide();
                        if (callback) callback(true);
                    });
                    $("#popup_ok").focus().keypress( function(e) {
                        if( e.keyCode == 13 || e.keyCode == 27 ) $("#popup_ok").trigger('click');
                    });
                break;
                case 'confirm':
                    $msgDiv.after('<div id="popup_panel"><input class=cs-button-secondary type="button" value="' + $.alerts.cancelButton + '" id="popup_cancel" /><input class=cs-button type="button" value="' + $.alerts.okButton + '" id="popup_ok" /></div>');
                    $("#popup_ok").click( function() {
                        $.alerts._hide();
                        $.alerts._modal.hide();
                        if( callback ) callback(true);
                    });
                    $("#popup_cancel").click( function() {
                        $.alerts._hide();
                        $.alerts._modal.hide();
                        if( callback ) callback(false);
                    });
                    $("#popup_ok").focus();
                    $("#popup_ok, #popup_cancel").keypress( function(e) {
                        if( e.keyCode == 13 ) $("#popup_ok").trigger('click');
                        if( e.keyCode == 27 ) $("#popup_cancel").trigger('click');
                    });
                break;
                case 'prompt':
                    $msgDiv.append('<br /><input type="text" size="30" id="popup_prompt" />').after('<div id="popup_panel"><input class=cs-button type="button" value="' + $.alerts.okButton + '" id="popup_ok" /><input type="button" value="' + $.alerts.cancelButton + '" id="popup_cancel" /></div>');
                    $("#popup_prompt").width( $("#popup_message").width() );
                    $("#popup_ok").click( function() {
                        var val = $("#popup_prompt").val();
                        $.alerts._hide();
                        $.alerts._modal.hide();
                        if( callback ) callback( val );
                    });
                    $("#popup_cancel").click( function() {
                        $.alerts._hide();
                        $.alerts._modal.hide();
                        if( callback ) callback( null );
                    });
                    $("#popup_prompt, #popup_ok, #popup_cancel").keypress( function(e) {
                        if( e.keyCode == 13 ) $("#popup_ok").trigger('click');
                        if( e.keyCode == 27 ) $("#popup_cancel").trigger('click');
                    });
                    if( value ) $("#popup_prompt").val(value);
                    $("#popup_prompt").focus().select();
                break;
            }//switch(type)

            // Make draggable
            if ($.alerts.draggable)
            {
                try
                {
                    $("#popup_container").draggable({ handle: $("#popup_title_container") });
                    $("#popup_title").css({ cursor: 'move' });
                }//try
                catch(e) { /* requires jQuery UI draggables */ }
            }//if
        }//show
        ,

        _hide: function()
        {
            $("#popup_container").remove();
            $.alerts._overlay('hide');
            $.alerts._maintainPosition(false);
        }//_hide
        ,

        _overlay: function(status)
        {
            switch (status)
            {
                case 'show':
                    $.alerts._overlay('hide');
                    $("BODY").append('<div id="popup_overlay"></div>');
                    $("#popup_overlay").css({
                        position: 'absolute',
                        zIndex: 99998,
                        top: '0px',
                        left: '0px',
                        width: '100%',
                        height: $(document).height(),
                        background: $.alerts.overlayColor,
                        opacity: $.alerts.overlayOpacity
                    });
                break;
                case 'hide':
                    $("#popup_overlay").remove();
                break;
            }//switch
        } //_overlay
        ,

        _reposition: function()
        {
            var $container = $("#popup_container");
            var top = (($(window).height() / 2) - ($container.outerHeight() / 2)) + $.alerts.verticalOffset;
            var left = (($(window).width() / 2) - ($container.outerWidth() / 2)) + $.alerts.horizontalOffset;
            if( top < 0 ) top = 0;
            if (left < 0) left = 0;

            $container.css({
                top: top + 'px',
                left: left + 'px'
            });

            $("#popup_overlay").height($(document).height());            
        }//_reposition
        ,

        _maintainPosition: function(status)
        {
            if( $.alerts.repositionOnResize ) {
                switch (status)
                {
                    case true:
                        $(window).bind('resize', $.alerts._reposition);
                    break;
                    case false:
                        $(window).unbind('resize', $.alerts._reposition);
                    break;
                }//switch
            }//if
        }//_maintainPosition
    }

    // Shortuct functions
    jAlert = function(message, title, callback) {
        $.alerts.alert(message, title, callback);
    }

    jConfirm = function(message, title, callback, alertType) {
        $.alerts.confirm(message, title, callback, alertType);
    };

    jPrompt = function(message, value, title, callback) {
        $.alerts.prompt(message, value, title, callback);
    };

})(jQuery);

window.nativeAlert = window.alert;
window.alert = jAlert;
