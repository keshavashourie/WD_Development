var Component = function () {

    return {
        interfaces: {
            'MOM.UI.Navigable': {
                navigateTo: function (nav) {
                    var __page = $find("__Page");
                    switch (nav.value) {
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
                            var executeWhenTrue = "KillSession(true);setTimeout( function()  {window.top.location = 'default.htm';}, 2000 );";
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
                            if (nav.queryString) {
                                queryString += '&' + nav.queryString;
                            }
                            var isPageFlow = nav.value.indexOf('PF.') > -1;
                            if (isPageFlow) {
                                __toppage.openInTabId('Main.aspx', queryString + '&redirectToPageFlow=' + nav.value, nav.uiValue, null, null, true);
                            }
                            else {
                                var pageDisplayMode = nav._data.PageDisplay;
                                if (pageDisplayMode === "1") {    // openInNewWindow
                                    __toppage.openInNewWindow(nav._data.PageURL);
                                }
                                else {
                                    if (nav._data.PageURL) {
                                        alert("External resource must be open in a new window. Set menu item's PageDisplay property to 'InNewBrowser' mode.");
                                    }
                                    else {
                                        __toppage.openInTabId(nav.value, queryString, nav.uiValue, null, null, true);
                                    }
                                }
                            }
                            break;

                    }
                }
            }
        }
    };
    return interface;
}();

SWACBoot.start(
    function (containerInfo) {

        // Increase Internal timeout to 5 sec beasause of slow IE
        SWAC.Config.TimeOuts.Internal = 5000;

        Component.Hub = new SWAC.Hub(Component);
        Component.Hub.beginExpose().then(
            function () {
            },
            function (reason) {
                console.error(reason);
            }
        );
    },

    function (evt) {
        console.log('Boot error:', evt);
    }, '1.6.2', 'no', 5000
);

function setApolloHeader(title, key) {
    if (Component && Component.Hub && Component.Hub.services) {
        Component.Hub.services.beginGet("MOM.UI.EventBus").then(function (svc) {
            svc.publish('cep.header.update', {
                name: title,
                key: key
            });
        },
            // On reject
            function (reason) {
                console.error(reason);
            }
        );
    }
    else {
        // swac is not loaded yet ---- ignore
        console.info("SWAC is not initialized yet. Set apollo header ignored.", title);
    }
}

function setApolloLineAssignement(workcenter, operation, spec, resource, workstation) {
    Component.Hub.services.beginGet("MOM.UI.EventBus").then(
        function (svc) {
            svc.publish('cep.line.assignment.update', {
                workcenter: workcenter,
                operation: operation,
                spec: spec,
                resource: resource,
                workstation: workstation
            });
        },
        // On reject
        function (reason) {
            console.error(reason);
        });
}

function _show(momUiSvc, messageType, messageText) {
    return Component.Hub.services.beginGet("MOM.UI." + momUiSvc).then(
        function (svc) {
            svc.show(messageType, messageText, {});
        },
        function (reason) {
            console.log("Error with MOM service " + momUiSvc, reason);
        }
    );
}
function _initConfigApolloMsg(messageType, messageText, fadeOut) {
    if (Component && Component.Hub && Component.Hub.services) {
        Component.Hub.services.beginGet("MOM.UI.EventBus").then(function (svc) {
            svc.publish('cep.message.init', {
                messageType: messageType,
                messageText: messageText,
                fadeOut: fadeOut
            });
        },
            function (reason) {
                console.error(reason);
            }
        );
    }
    else {
        console.info("SWAC is not initialized yet. Set apollo message ignored.", title);
    }
}


function showApolloError(msg, fadeOut) {
    _initConfigApolloMsg("ERROR", msg, fadeOut);
    return _show("Error", "Error", msg);
}

function showApolloInfo(msg, fadeOut) {
    _initConfigApolloMsg("INFO", msg, fadeOut);
    return _show("Notification", "Status", msg);
}

function showApolloWarning(msg) {
    return _show("Warning", "Warning", msg);
}

function getUrlParamVal(paramName, params) {
    var param = params.match(paramName + "=([^&?]+)");
    if (param)
        return param[1];
    return null;
}