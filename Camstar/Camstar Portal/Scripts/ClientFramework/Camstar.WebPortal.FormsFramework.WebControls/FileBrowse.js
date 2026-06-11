// Copyright Siemens 2023  

/// <reference name="MicrosoftAjax.js"/>
/// <reference path="../Camstar.UI/Control.js" />
/// <reference path="PickLists/PickListCommon.js" />
/// <reference path="PickLists/PickListControl.js" />
/// <reference path="PickLists/PickListPanel.js" />

Type.registerNamespace("Camstar.WebPortal.FormsFramework.WebControls");

Camstar.WebPortal.FormsFramework.WebControls.FileBrowse = function(element)
{
    Camstar.WebPortal.FormsFramework.WebControls.FileBrowse.initializeBase(this, [element]);
    this._serverType = "Camstar.WebPortal.FormsFramework.WebControls.FileBrowse, Camstar.WebPortal.FormsFramework.WebControls";
}

Camstar.WebPortal.FormsFramework.WebControls.FileBrowse.prototype =
{
    initialize: function() {
        Camstar.WebPortal.FormsFramework.WebControls.FileBrowse.callBaseMethod(this, 'initialize');
    },

    dispose: function() {
        Camstar.WebPortal.FormsFramework.WebControls.FileBrowse.callBaseMethod(this, 'dispose');
    },
    directUpdate: function (value) {
        Camstar.WebPortal.FormsFramework.WebControls.FileBrowse.callBaseMethod(this, 'directUpdate');

        if (value.PropertyKey == eval(Camstar.Ajax.DirectUpdateParameterKeys.Data)) {
            if (this._value) {
                this._value.value = value.PropertyValue;
            }
        }
        else if (value.PropertyKey == eval(Camstar.Ajax.DirectUpdateParameterKeys.DataText)) {
            if (this._editor) {
                this._editor.value = value.PropertyValue;
            }
        }
        else if (value.PropertyKey == eval(Camstar.Ajax.DirectUpdateParameterKeys.Enable)) {
            //set disable for span element
            var spanElement = this.get_element();
            spanElement.disabled = value.PropertyValue != "True";

            //set disable for main table element
            var tableMain = $(".PickListInputControl", this.get_element())[0];
            if (tableMain) {
                tableMain.disabled = value.PropertyValue != "True";
                //set disable for child table element
                tableMain.rows[0].cells[0].childNodes[0].disabled = value.PropertyValue != "True";
            }

            this._button.disabled = value.PropertyValue != "True";
            this._editor.disabled = value.PropertyValue != "True";

            //set background for container td
            this._editor.parentNode.style.backgroundColor = this._editor.parentNode.getAttribute(value.PropertyValue != "True" ? "DisabledBG" : "EnabledBG");
            if (this._panel) {
                $get(this._panel._filter.id + "_Fltc", this._panel._filter).disabled = value.PropertyValue != "True";
                this._panel._refresh.disabled = value.PropertyValue != "True";
            }
        }
        else if (value.PropertyKey == eval(Camstar.Ajax.DirectUpdateParameterKeys.Visible)) {
            if (this._editor)
            {
                var parent = this._editor.parentNode;
                while (parent.tagName != "SPAN")
                {
                    parent = parent.parentNode;
                }
                //parent.style.display = value.PropertyValue == "True" ? "" : "none";
                //this._label.style.display = value.PropertyValue == "True" ? "" : "none";
            }
        }
    },

    isFileBrowse: function () {
        return true;
    }
}

Camstar.WebPortal.FormsFramework.WebControls.FileBrowse.registerClass('Camstar.WebPortal.FormsFramework.WebControls.FileBrowse', Camstar.WebPortal.FormsFramework.WebControls.PickLists.PickListControl);

if (typeof (Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
