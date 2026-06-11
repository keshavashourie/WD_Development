// Copyright Siemens 2025

$(function ($) {
    $(".ui-login").login();
    $("#UsernameTextbox").focus();
});

function bindCloseEventToStatusBar($statusBar) {
    $statusBar.on("click", ".close", function () {
        $statusBar.hide();
    });
}

function checkTimeZone() {
    const localStorageKey = "TimeZone";
    const $timeZoneDropDown = $("#TimeZoneDropDown")[0];
    let timeZoneCodeDefault = getValue(localStorageKey);

    const currentTimeZone = Intl.DateTimeFormat().resolvedOptions().timeZone;

    if (Array.isArray(mapClientToTimeZoneCode) && typeof timeZoneCodeDefault === "undefined") {
        mapClientToTimeZoneCode.forEach((item) => {
            if (Array.isArray(item.longTimezones) && item.longTimezones.indexOf(currentTimeZone) != -1 && typeof item["timeZoneCode"] !== "undefined") {
                timeZoneCodeDefault = item["timeZoneCode"];
                return;
            }
        });

        if (typeof timeZoneCodeDefault === "undefined") {
            const currentUTCOffset = new Date().getTimezoneOffset();
            const formattedOffset = currentUTCOffset > 0 ? parseInt("-" + currentUTCOffset) : currentUTCOffset === 0 ? currentUTCOffset : Math.abs(currentUTCOffset);
            mapClientToTimeZoneCode.forEach((item) => {
                if (typeof formattedOffset !== "undefined" && item["utcOffset"] === formattedOffset) {
                    timeZoneCodeDefault = item["timeZoneCode"];
                    return;
                }
            });

            if (typeof timeZoneCodeDefault === "undefined") {
                timeZoneCodeDefault = "UTC";
            }
        };
    }

    for (let i = 0; i < $timeZoneDropDown.options.length; i++) {
        if ($timeZoneDropDown.options[i].value === timeZoneCodeDefault) {
            $timeZoneDropDown.selectedIndex = i;
            break;
        }
    }

    if ($timeZoneDropDown.selectedIndex == -1)
        $timeZoneDropDown.selectedIndex = 0;
}

function saveLocalStorageTimeZone(timeZoneCode) {
    const localStorageKey = "TimeZone";
    setValue(localStorageKey, timeZoneCode);
    var $statusBar = $(".webpart-status");
    bindCloseEventToStatusBar($statusBar);

    var $message = $(".message");
    $statusBar.attr('messageType', 'Success');

    $message.html('<div class=\"messageType\" id=\"divStatusMessage\"><span class=\"message-status-type\">Success</span><span class=\"instruction noty_text\" id=\"SpanStatusMessage\">Login options saved</span></div>');
    $statusBar.fadeIn(1000).delay(6000).fadeOut(1000);

}

function setValue(key, value) {
    try {
        localStorage.setItem(key, JSON.stringify(value));
    } catch (e) {
        console.error(e);
    }
}


function getValue(key) {
    try {
        const value = localStorage.getItem(key);
        return value ? JSON.parse(value) : undefined;
    } catch (e) {
        return undefined;
    }
}

(function ($) {
    var isOptionsVisible = false;
    $.fn.login = function (options) {
        var $optionLink = $(".ui-login-options");
        var $optionContainer = $(".ui-login-optioncontainer");
        var $usernameTextbox = $("#UsernameTextbox");
        var $passwordTextbox = $("#PasswordTextbox");
        var $loginButton = $("#LoginButton");
        var $errorContainer = $(".ui-login-error");
        var $errorLabel = $("#ErrorLabel");
        var $statusBar = $(".webpart-status");
        var $userNameLabel = $("#UsernameLabel");
        var $passwordLabel = $("#PasswordLabel");
        var $usernameAsterisk = $("#userNameAsterisk");
        var $passwordAsterisk = $("#passwordAsterisk");

        var isOptionsVisible = false;

        function toggleLoginButton() {
            if (!$usernameTextbox.length || !$passwordTextbox.length || !$loginButton.length) return;

            var usernameVal = $usernameTextbox.val() || "";
            var passwordVal = $passwordTextbox.val() || "";

            $loginButton.prop("disabled", usernameVal.trim() === "" || passwordVal.trim() === "");
        }

        $usernameTextbox.on("input", toggleLoginButton);
        $passwordTextbox.on("input", toggleLoginButton);

        toggleLoginButton();

        if ($errorLabel.length && $errorLabel.html().length > 0) {
     
            $errorContainer.slideDown();
            $statusBar.fadeIn(1000).delay(30000).fadeOut(1000);

            bindCloseEventToStatusBar($statusBar);
        }


        $optionLink.click(function () {
            $optionContainer.slideToggle();
            isOptionsVisible = !isOptionsVisible;
            $optionLink.toggleClass('option-link-contract').toggleClass('option-link-expand');   
        });

        $loginButton.click(function (e) {
            var username = $usernameTextbox.val();
            var password = $passwordTextbox.val();
            var isValid = true;
            var message = "";

            if (username.length == 0 && password.length == 0) {
                $usernameTextbox.removeClass("ui-state-active");
                $usernameTextbox.css({ "border": "1px solid #FFADAD", "background": "#DC00001A url(/CamstarPortal/Images/indicatorWarningTriangleRed16.svg) right 8px center / 15px no-repeat " });
                $passwordTextbox.removeClass("ui-state-active");
                $passwordTextbox.css({ "border": "1px solid #FFADAD", "background": "#DC00001A url(/CamstarPortal/Images/indicatorWarningTriangleRed16.svg) right 8px center / 15px no-repeat " });
                message = "User Name required";

                $userNameLabel.text("User Name (" + message + ")");
                $userNameLabel.css({ "color": "#FFADAD" });
                $usernameAsterisk.css({ "color": "#FFADAD" });
                $passwordLabel.text("Password (Password  required)");
                $passwordLabel.css({ "color": "#FFADAD" });
                $passwordAsterisk.css({ "color": "#FFADAD" });


                isValid = false;
            }

            if (username.length == 0 && password.length > 0 && isValid) {
                $usernameTextbox.removeClass("ui-state-active");
                $usernameTextbox.css({ "border": "1px solid #FFADAD", "background": "#DC00001A url(/CamstarPortal/Images/indicatorWarningTriangleRed16.svg) right 8px center / 15px no-repeat " });
                $passwordTextbox.addClass("ui-state-active");
                $passwordTextbox.removeAttr("style");
                message = "User Name required";
                $userNameLabel.text("User Name (" + message + ")");
                $userNameLabel.css({ "color": "#FFADAD" })
                $usernameAsterisk.css({ "color": "#FFADAD" });
                $passwordLabel.text("Password");
                $passwordLabel.css({ "color": "#fff" });
                $passwordAsterisk.css({ "color": "#aae6f5"});
                isValid = false;
            }

            if (password.length == 0 && username.length > 0 && isValid) {
                $usernameTextbox.addClass("ui-state-active");
                $usernameTextbox.removeAttr("style");
                $passwordTextbox.removeClass("ui-state-active");
                $passwordTextbox.css({ "border": "1px solid #FFADAD", "background": "#DC00001A url(/CamstarPortal/Images/indicatorWarningTriangleRed16.svg) right 8px center / 15px no-repeat " });
                message = "Password required.";
                $passwordLabel.text("Password (" + message + ")");
                $passwordLabel.css({ "color": "#FFADAD" });
                $userNameLabel.text("Username");
                $userNameLabel.css({ "color": "#fff" });
                $usernameAsterisk.css({ "color": "#aae6f5" });
                isValid = false;
            }

            if (isValid) {
                $usernameTextbox.addClass("ui-state-active");
                $passwordTextbox.addClass("ui-state-active");
                $usernameTextbox.removeAttr("style");
                $passwordTextbox.removeAttr("style");
                $userNameLabel.text("User Name");
                $passwordLabel.text("Password");
                $userNameLabel.css({ "color": "#fff" })
                $passwordLabel.css({ "color": "#fff" });
                $usernameAsterisk.css({ "color": "#aae6f5" });
                $passwordAsterisk.css({ "color": "#aae6f5" });
                //disable the button to prevent multiple clicks and multiple active user sessions being created
                setTimeout(function () {
                    $loginButton.attr('disabled', 'disabled');
                }, 100);

            }
            else {
                e.preventDefault();
                $errorLabel.html(message);
                /*$statusBar.show();*/
                bindCloseEventToStatusBar($statusBar);
                $errorContainer.slideDown();

                //if validation failed (eg: emtpy username, pw, etc..) then re-enable the button
                $loginButton.removeAttr('disabled');
            }
        });
    };

})(jQuery);
