// Copyright Siemens 2023  

/// <reference name="MicrosoftAjax.js"/>
/// <reference path="../../Camstar.UI/Control.js" />

Type.registerNamespace("Camstar.WebPortal.FormsFramework.WebControls.PickLists");

// PickList Control Common Interface
Camstar.WebPortal.FormsFramework.WebControls.PickLists.IPickListControl = function() { }
Camstar.WebPortal.FormsFramework.WebControls.PickLists.IPickListControl.Prototype = {
    setValue: function (val) { },
    clearValue: function () { },
    getValue: function () { }
}
Camstar.WebPortal.FormsFramework.WebControls.PickLists.IPickListControl.registerInterface('Camstar.WebPortal.FormsFramework.WebControls.PickLists.IPickListControl');

// PickList Panel Common Interface
Camstar.WebPortal.FormsFramework.WebControls.PickLists.IPickListPanel = function() { }
Camstar.WebPortal.FormsFramework.WebControls.PickLists.IPickListPanel.Prototype = {
    load: function() { },
    filter: function() { },
    page: function() { },
    focus: function(revert) { }
}
Camstar.WebPortal.FormsFramework.WebControls.PickLists.IPickListPanel.registerInterface('Camstar.WebPortal.FormsFramework.WebControls.PickLists.IPickListPanel');

// TreeViewControl client-side functions
var mkInstanceNameBox = "instanceNameBox";
var mkInstanceRevBox = "instanceRevisionBox";
var mkReloadButton = "reloadButton";
var mkUserFieldsDiv = "userFieldsDiv";
var mkParametricDiv = "parametricDiv";
var mkStatusMessageCtrlId = "StatusMessage";

// Displays status message either in the status frame or status control on a form 
// Pops up an alert message box if no status frame or control found.
function DisplayMessage(message, isSuccess)
{
    // Write message to the status frame
    if (parent.frames.WriteMessage != null)
    {
        parent.frames.WriteMessage(message, isSuccess);
    }
    else
    // Write message to the status control on the current form
        if (document.getElementById(mkStatusMessageCtrlId) != null)
        {
            var statusCtrl = document.getElementById(mkStatusMessageCtrlId);

            var t = getCEP_top();
            var spans = document.getElementsByTagName(t.TagSpan);
            if (spans != null && spans.length > 0)
            {
                spans[spans.length - 1].innerText = message;
                if (isSuccess)
                    spans[spans.length - 1].style.color = t.ColorBlack;
                else
                    spans[spans.length - 1].style.color = t.ColorRed;
            }
        }
        // Display message box with the text message
        else
        {
            alert(message);
        }
}

//Checks if all the reported elements are valid
function AreValidElements(valueList)
{
    var isValidList = true;
    for (var i = 0; i < valueList.length; i++)
    {
        if (isValidList == true)
        {
            if (!IsValidElement(document, valueList[i]))
            {
                isValidList = false;
            } //if
        } //if
    } //for
    return isValidList;
} //AreValidElements

if (typeof (Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
