// Copyright Siemens 2023  

var camstar = camstar || {};

camstar.sessionTimeout = function () {
    //private members     
    var sessionTimeoutCountdownId = 'sessionTimeoutCountdown',
    expiredMessageLabel,
    loginLabel,
    redirectAfter = 60, // number of seconds to wait before redirecting the user
    redirectTo = window.location.protocol + '//' + window.location.hostname + '/CamstarPortal' + '/Default.aspx', // URL to relocate the user to once they have timed out
    keepAliveUrl = 'SessionHandler.ashx?', // URL to call to keep the session alive
    running = false, // var to check if the countdown is running
    idleTime, // miniutes converted to milliseconds
    expirationTime,
    timer, // reference to the setInterval timer so it can be stopped
    timeout, // reference to the setTimeout timer so it can be stopped
    ssoTimer, //reference to setInterval SSO Timer for renewal of SSO session. Renewal must happen only till opcore session is active.
    ssoRenew = false, // to check whether sso related renewal requests must be performed
    warning,
        init = function (type, time, initialSessionTimeoutMessage, expiredMessage, sessionExpirationWarning, keepMeLoggedIn, logMeOut, logIn, ssoLogoutUrl) {
            if (ssoLogoutUrl) {
                redirectTo = ssoLogoutUrl;
            } else if (!($("body").hasClass("Horizon-theme"))) {
                redirectTo = 'Default.htm?mode=classic';
            }
            expirationTime = (time * 60000);
            idleTime = (time * 60000) - 60000; // miniutes converted to milliseconds
            // Below endpoint to check whether SSO renewal requests are required. For cases where this endpoint is not implemented flag remains false and no timer is started
            $.ajax({
                url: '/renew'
            })
            .done(function (returnData) {
                ssoRenew = returnData.result;
                if (ssoRenew === 'true') {
                    var renewInterval = returnData.data; // This value is in seconds
                    // Start sso timer for renewal.
                    ssoTimer = setInterval(function () {
                        $.ajax({
                            url: '/heartbeat'
                        })
                    }, 1000 * renewInterval);
                }
            })
            .fail(function () {
                ssoRenew = false;
            });

            if (type == "warn")
            {
                var w = 460;
                if (__page && __page.isMobilePage())
                    w = 310;
                initTimeoutWarning(initialSessionTimeoutMessage, sessionExpirationWarning, keepMeLoggedIn, logMeOut, logIn, expiredMessage, w);
                window.addEventListener("resize", function () { camstar.sessionTimeout.centerDialog() });
            }
            if (type == "alive")
            {
                keepSessionAlive();
            }       
            return;
    },
        initTimeoutWarning = function (initialSessionTimeoutMessage, sessionExpirationWarning, keepMeLoggedIn, logMeOut, logIn, expiredMessage, w) {
        //add session timeout warning and hidden time to body
        $("body").append('<div id="sessionTimeoutWarning" style="display: none" class=""></div>')
                    .append($('<input/>', { type: 'hidden', id: 'sessionTimeoutStartTime', value: $.now() }));

        warning = $("#sessionTimeoutWarning").html(initialSessionTimeoutMessage);
        expiredMessageLabel = expiredMessage;
        loginLabel = logIn;
        timeout = setTimeout(createTimer, idleTime);
       
        //set up session timeout dialg
        warning.dialog({            
            title: sessionExpirationWarning,
            autoOpen: false,	// set this to false so we can manually open it
            dialogClass: 'ui-dialog-session-timeout-warning dialog ui-draggable',
            closeOnEscape: false,
            draggable: false,
            width: w || 460,
            minHeight: 50,
            modal: true,
            resizable: false,
            beforeClose: function () { // bind to beforeclose so if the user clicks on the "X" or escape to close the dialog, it will work too
                // stop countdown
                running = false;
                if (ssoRenew) {
                    // renew sso session as well
                    $.ajax({
                        url: '/heartbeat'
                    });
                }
                // ajax call to keep the server-side session alive
                $.ajax({
                    url: keepAliveUrl + "refresh=1",
                    contentType: "html"
                });

            },
            buttons:
            [
                {                    
                    text: keepMeLoggedIn,
                    click: function () {
                        $(this).dialog('close');
                        reset(idleTime);
                    },
                    'class': 'cs-button'
                },
                {                   
                    text: logMeOut,
                    click: function () {
                        stopSSORenew();
                        redirectToStart();
                    },
                    'class': 'cs-button-secondary'
                }
            ],
            open: function () {
                // scrollbar fix for IE
                $('body').css('overflow', 'hidden');
            },
            close: function () {
                // reset overflow
                $('body').css('overflow', 'auto');
                reset(idleTime);
            }
        }); // end of dialog
        $(".ui-dialog")
            .css("z-index", 2)
            .removeClass("ui-widget")
            .removeClass("ui-widget-content");
        $(".ui-dialog-content").removeClass("ui-widget-content");        
    },
    keepSessionAlive = function () {
        setInterval(function () {
            $.ajax({
                url: keepAliveUrl + "refresh=1",
                contentType: "html"
            });
        }, idleTime);
    },
    createTimer = function () {
        var time = Number($('#sessionTimeoutStartTime').val());
        var now = $.now();
        var timediff = (now - time); //-minute 
        var expirationTimeDiff = expirationTime - timediff;
        var idleTimeDiff = idleTime - timediff;
        var timedout = idleTimeDiff < 1;

        var time = $('#sessionTimeoutStartTime').val();
        var timediff = ($.now() - time); //-minute
        var timedout = ((idleTime - timediff) < 1);

        if (!timedout) {
            //  Reset timer to the amount of time between Idle Time and last time session start was updated
            let diff = idleTime - timediff;
            timeout = setTimeout(createTimer, diff);
        } else if (expirationTimeDiff < 0)
            displayLogin();
        else {
            var counter = redirectAfter;
            running = true;

            // intialisze timer
            $('#' + sessionTimeoutCountdownId).html(redirectAfter);

            // open dialog
            warning.dialog('open');

            // create a timer that runs every second
            timer = setInterval(function () {
                counter -= 1;
                var now = $.now();
                var timediff = (now - time); //-minute
                var timedout = expirationTime - timediff;

                // if the counter is 0, redirect the user
                if (counter <= 0 || timedout < 0) {
                    stopSSORenew();
                    displayLogin();
                } else {
                    $('#' + sessionTimeoutCountdownId).html(counter);
                }

            }, 1000);

        }
    },
    reset = function(time) {
        clearInterval(timer);
        clearTimeout(timeout);

        if (time > 0)
            timeout = setTimeout(createTimer, time);
        else {
            stopSSORenew(); // stop sso renewal before logout
            KillSession();
        }
    },
    resetSessionTime = function () {
        $('#sessionTimeoutStartTime').val($.now());
    },
    redirectToStart = function () {
        reset();
        if (window.ssoSessionId) {
            submitLogoutForm(window.ssoSessionId);
        } else {
            window.top.location = redirectTo;
        }
    },
        displayLogin = function () {
            reset();
            $(warning).html(expiredMessageLabel);

            $(warning).dialog("option", "buttons", [{            
                text: loginLabel,
                click: function () {
                    if (window.ssoSessionId) {
                        submitLogoutForm(window.ssoSessionId);
                    }
                    else {
                        window.top.location = redirectTo;
                    }
                },
                'class': 'cs-button'
            }]);
            $(".ui-icon-closethick").hide();
    },
    center = function() {
        if (warning && warning.dialog("isOpen")) {
            warning.dialog("option", "position", warning.dialog("option", "position"));
        }
    },
     // Stop sso renewal before logout. Executes only when sso renewal was required
    stopSSORenew = function() { 
        if (ssoRenew) {
            clearInterval(ssoTimer);
        }
    }
    dispose = function() {
        clearInterval(timer);
        clearTimeout(timeout);
        stopSSORenew();
    };
    //public members
    return {
        init: init,
        reset: reset,
        resetSessionTime: resetSessionTime,
        centerDialog: center
    };
}();

$(function () {
    var ssoLogoutUrl = null;
    var timeoutTime = $("#SessionTimeoutTime").val();
    var timeoutType = $("#SessionTimeoutType").val();

    $.ajax({
        url: 'Main.aspx/GetSSOSessionDetails',
        type: 'POST',
        async: false,
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        success: function (response) {
            if (!(response.d == null || response.d == undefined)) {
                var SSOSessionId = JSON.parse(JSON.parse(JSON.stringify(response.d))).SSOSessionId;
                ssoLogoutUrl = window.location.protocol + '//' + window.location.hostname + '/IDPAgent/api/v1/sso-service-provider/logout-request?state=' + SSOSessionId;
                window.ssoSessionId = SSOSessionId;
            }
            else {
                ssoLogoutUrl = window.location.protocol + '//' + window.location.hostname + '/CamstarPortal' + '/Default.aspx';
            }
        }
    });
            
    // Get Labels for page
    var labels = [{ Name: 'Lbl_SessionAboutToExpire' }, { Name: 'Lbl_ExpiredMessage' }, { Name: 'Lbl_SessionExpirationWarning' }, { Name: 'Lbl_KeepMeLoggedIn' }, { Name: 'Lbl_LogMeOut' }, { Name: 'Lbl_Login' }];
    var initialSessionTimeoutMessage, expiredMessage, sessionExpirationWarning, keepMeLoggedIn, logMeOut, logIn;

    function fetchAndInitSessionTimeout() {
        var labels = [{ Name: 'Lbl_SessionAboutToExpire' }, { Name: 'Lbl_ExpiredMessage' }, { Name: 'Lbl_SessionExpirationWarning' }, { Name: 'Lbl_KeepMeLoggedIn' }, { Name: 'Lbl_LogMeOut' }, { Name: 'Lbl_Login' }];
        if (typeof __page !== 'undefined') {
            __page.getLabels(labels, function (response) {
                if ($.isArray(response)) {
                    $.each(response, function () {
                        var labelName = this.Name;
                        var labelText = this.Value;
                        switch (labelName) {
                            case 'Lbl_SessionAboutToExpire':
                                initialSessionTimeoutMessage = labelText;
                                break;
                            case 'Lbl_ExpiredMessage':
                                expiredMessage = labelText;
                                break;
                            case 'Lbl_SessionExpirationWarning':
                                sessionExpirationWarning = labelText;
                                break;
                            case 'Lbl_KeepMeLoggedIn':
                                keepMeLoggedIn = labelText;
                                break;
                            case 'Lbl_LogMeOut':
                                logMeOut = labelText;
                                break;
                            case 'Lbl_Login':
                                logIn = labelText;
                                break;
                            default:
                                break;
                        }
                    });
                    camstar.sessionTimeout.init(timeoutType, timeoutTime, initialSessionTimeoutMessage, expiredMessage, sessionExpirationWarning, keepMeLoggedIn, logMeOut, logIn, ssoLogoutUrl);
                }
                else {
                    alert(response.Error);
                }
            });
        } else {
            console.warn("__page object is undefined. Session timeout might initialize without proper labels.");
            camstar.sessionTimeout.init(timeoutType, timeoutTime, initialSessionTimeoutMessage, expiredMessage, sessionExpirationWarning, keepMeLoggedIn, logMeOut, logIn, ssoLogoutUrl);
        }
    }
    fetchAndInitSessionTimeout();
});

function submitLogoutForm(ssoSessionId) {
    // Create a hidden form
    var form = document.createElement('form');
    form.method = 'POST';
    form.action = window.location.protocol + '//' + window.location.hostname + '/IDPAgent/api/v1/sso-service-provider/logout-request';
    form.style.display = 'none';

    // Create hidden input for state parameter
    var stateInput = document.createElement('input');
    stateInput.type = 'hidden';
    stateInput.name = 'state';
    stateInput.value = ssoSessionId;
    form.appendChild(stateInput);

    // Add form to document and submit
    document.body.appendChild(form);
    form.submit();
}
