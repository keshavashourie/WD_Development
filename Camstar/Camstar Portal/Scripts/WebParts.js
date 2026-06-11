// Copyright Siemens 2023  

// JScript File
function WebPartBase_SyncWebPartTitle(webPartId)
{
    if(__wpm.IsDragDropEnabled())
    {
        var webPartElement = document.getElementById("WebPart_" + webPartId);
        var webPartTitleElement = document.getElementById("WebPartTitle_" + webPartId);
        if (webPartElement != null && webPartTitleElement != null && webPartElement.__webPart != null)
        {
            webPartElement.__webPart.webPatTitleElement = webPartTitleElement;
            webPartTitleElement.style.cursor = "move";

            $addHandlers(webPartTitleElement, {"mousedown": WebPart_OnMouseDown });
        }
    }
} // WebPartBase_SyncWebPartTitle

function WebPartBase_OnActivate(sender)
{
    // mark current web part as selected
    if(sender != null)
    {
        if(sender.firstChild.children.length > 0)
        {
            var title = sender.firstChild;
            if(title == null)
                return;
            title = title.children[1];
            if(title == null)
                return;
            title = title.children[1];
            if(title == null)
                return;
            title = title.firstChild;
            if(title == null)
                return;
            var headerText = title.firstChild.firstChild.firstChild.firstChild.firstChild;
            var header = title.firstChild;
            title.className = sender["selectedTitleClass"];
            headerText.className = sender["selectedHeaderTextClass"];
            header.className = sender["selectedHeaderClass"];
            var prevSelWebPart = WebPartBase_GetSelected();   
            if(prevSelWebPart && prevSelWebPart != sender)
            {
                title = prevSelWebPart.firstChild.children[1].children[1].firstChild;
                headerText = title.firstChild.firstChild.firstChild.firstChild.firstChild;
                header = title.firstChild
                header.className = sender["regularHeaderClass"];
                title.className = sender["regularTitleClass"];
                headerText.className = sender["regularHeaderTextClass"];
            }
            
            WebPartBase_SetSelected(sender);
            SwitchStatusBar(sender);
        }
    }
} // WebPartBase_OnActivate

function SwitchStatusBar(webPart)
{
    if(typeof(ButtonsBarControlParams) != 'undefined')
    {
        var buttonsBar = $get(ButtonsBarControlParams[0]);
        if(buttonsBar != null)
        {
            var dynamicPanelFlag = webPart.children[0].children[3].children[1].children[0].id;
            var thereIsVisiblePanel = false;
            for(var i = 0; i < buttonsBar.children[0].children.length; i ++)
            {
                var ownerWebPartFlag = buttonsBar.children[0].children[i]["ownerWebPartId"];
                if(ownerWebPartFlag != null)
                {
                    // executes when current html control is dynamic panel
                    thereIsVisiblePanel = (dynamicPanelFlag == ownerWebPartFlag) ? true : thereIsVisiblePanel;
                    buttonsBar.children[0].children[i].style.display = (dynamicPanelFlag == ownerWebPartFlag) ? "inline" : "none";
                }
                else
                {
                    // executes when current html control is static panel
                    thereIsVisiblePanel = true;
                }
            }
            
            // store prev display value
            var prevDisplay = buttonsBar.currentStyle.display;
            
            // change (if there is need) buttons bar display mode
            buttonsBar.style.display = thereIsVisiblePanel ? "block" : "none";
            
            // the following trick is needed to support correct clicking
            if(prevDisplay != buttonsBar.currentStyle.display && event != null)
                if(event.srcElement["webPartButtonCellFlag"] == "true")
                    event.srcElement.children[0].click();
        }
    }
} // SwitchStatusBar

function WebPartBase_SetSelected(webPart)
{
    var hiddenField = $get("selectedWebPart");
    if(hiddenField)
        hiddenField.value = webPart.id;
} // WebPartBase_SetSelected

function WebPartBase_GetSelected()
{
    var hiddenField = $get("selectedWebPart");
    if (hiddenField && hiddenField.value != "" && hiddenField.value != null)
    {
        return $get(hiddenField.value);
    }
} // WebPartBase_GetSelected

function ViewButtonClick(id, row)
{
    var viewButton = $get(id);
    var prevHref = viewButton.href;
    var newHref;
    var lastIndex = viewButton.href.lastIndexOf("\'\'");
    newHref = prevHref.substring(0, lastIndex + 1) + row.toString() + prevHref.substring(lastIndex + 1);
    viewButton.href = newHref;
    viewButton.click();
    viewButton.href = prevHref;
} //ViewButtonClick

function GetDataChooserData(dataChooserID) {
    var dataChooserControl = document.getElementById(dataChooserID);
    var i = 0;
    var cnt = dataChooserControl.all.length;
    for (i = 0; i < cnt; i++) {
        if (dataChooserControl.all[i].className == "DateChooserControl") {
            return dataChooserControl.all[i].value;
        }
    }
    return null;
} //GetDataChooserData

function AlignConciergeVertically(tabContainer) {
    if (tabContainer) {
        var conciergeContainer = $(".concierge-container");
        if (conciergeContainer.length > 0) {
            var tabContainerTop = tabContainer.offsetTop;
            var conciergeTop = conciergeContainer.position().top;
            if (tabContainerTop > conciergeTop) {
                conciergeContainer.css('top', tabContainerTop);
            }
        }
    }
}


function ButtonsBarEmpty()
{
    var empty = true;
    var wp = $get("WebPart_NavigationButtonsBar");
    var left = $("> .left", wp);
    var i;
    for (i = 0; i < left.length; i++)
    {
        if (left[i].children.length != 0)
            empty = false;
    }
    var right = $("> .right", wp);
    for (i = 0; i < right.length; i++)
    {
        if (right[i].children.length != 0)
            empty = false;
    }

    wp = $get("WebPart_ButtonsBar");
    left = $("> .left", wp);
    for (i = 0; i < left.length; i++)
    {
        if (left[i].children.length != 0)
            empty = false;
    }
    right = $("> .right", wp);
    for (i = 0; i < right.length; i++)
    {
        if (right[i].children.length != 0)
            empty = false;
    }
    return empty;
}

function containerStatusM_adjustment() {

    var $wp = $(".webpart.webpart-containerstatus-m");
    if (!$wp.length)
        return;

    var $tmr = $(".container-timer", $wp);
    $wp.toggleClass("has-timer", $tmr.length > 0);
    $tmr.parent().removeClass("empty");
    $(">input", $tmr).css("display", "").css("margin-top", "");

    if (!$wp.hasClass("rebuilt")) {
        // rearrange cells into flat structure
        $wp.append("<div class=grid-matrix />");
        var $gm = $(".grid-matrix", $wp);

        $("> .matrix >.row >.col", $wp).each(function () {
            var $c = $(this).detach();
            if ($c.hasClass("empty"))
                $c.remove();
            else {
                $c.removeClass("col").removeClass("cell-m").addClass("cont-status-cell");
                $gm.append($c);
            }
        });

        $wp.addClass("rebuilt");
        $(">.matrix >.row", $wp).remove();
        $(">.matrix", $wp).remove();
    }
}

function resourceStatus_adjustment() {

    var $wp = $(".webpart.ui-webpart-resource-status");
    if (!$wp.length)
        return;

    var w = $(".cell-group", $wp).outerWidth(true) + $(".cell-resource", $wp).outerWidth(true) + 4 /*padding-left*/;
    if (w < 100) {
        // IE problem : styles are not loaded yet -- run again in 1 sec. Make <=10 attempts to not looping idefinitely
        var cnt = $wp.attr("load-counter");
        if (typeof cnt != "number")
            cnt = 0;

        if (cnt < 10) {
            $wp.attr("load-counter", ++cnt);
            setTimeout(function () { resourceStatus_adjustment($wp); }, 1000);
            return;
        }
        else {
            // no more attempt 
            w = 858;
        }
    }
    var wpx = w + "px";

    // set width
    $(".cell-m[key=item_r0_c2]", $wp).width("calc( (100% - " + wpx + ") / 2)");
    $(".cell-m[key=item_r0_c3]", $wp).width("calc( (100% - " + wpx + ") / 2)");
    $wp.removeAttr("load-counter");
}
