// Copyright Siemens 2023  

/// <reference name="MicrosoftAjax.js"/>
/// <reference path="../Camstar.UI/Control.js" />
/// <reference path="PickLists/PickListCommon.js" />
/// <reference path="PickLists/PickListControl.js" />
/// <reference path="PickLists/PickListPanel.js" />
/// <reference path="../CamstarPortal.WebControls/JQGridBaseData.js" />

Type.registerNamespace("Camstar.WebPortal.FormsFramework.WebControls");

Camstar.WebPortal.FormsFramework.WebControls.ContainerList = function(element)
{
    Camstar.WebPortal.FormsFramework.WebControls.ContainerList.initializeBase(this, [element]);
    this._pagerPicklistSelected = false;
}

Camstar.WebPortal.FormsFramework.WebControls.ContainerList.prototype =
{
    initialize: function()
    {
        Camstar.WebPortal.FormsFramework.WebControls.ContainerList.callBaseMethod(this, 'initialize');
        var pagerPickList = $(".ui-pg-input", this._element)[0];
        if (pagerPickList != undefined) {
            $clearHandlers(pagerPickList);
            $addHandlers(pagerPickList,
                        {
                            'click': function () { this._pagerPicklistSelected = true; },
                            'blur': function () { this._pagerPicklistSelected = false; }
                        }, this);
        }
    },

    dispose: function()
    {
        Camstar.WebPortal.FormsFramework.WebControls.ContainerList.callBaseMethod(this, 'dispose');
    }, 
    
    directUpdate: function(value)
    {
        Camstar.WebPortal.FormsFramework.WebControls.ContainerList.callBaseMethod(this, 'directUpdate');

        if (value.PropertyKey == eval(Camstar.Ajax.DirectUpdateParameterKeys.Data))
        {
            if (this._editor)
            {
                this._editor.value = value.PropertyValue;
            }
        }
        else if (value.PropertyKey == eval(Camstar.Ajax.DirectUpdateParameterKeys.Enable))
        {
            //set disable for span element
            var spanElement = this.get_element();
            spanElement.disabled = value.PropertyValue != "True";
            
            //set disable for main table element
            var tableMain = $(".PickListInputControl", this.get_element())[0];
            tableMain.disabled = value.PropertyValue != "True";
            tableMain.parentNode.childNodes[1].disabled = value.PropertyValue != "True";
            //set disable for child table element
            tableMain.rows[0].cells[0].childNodes[0].disabled = value.PropertyValue != "True";
            
            this._button.disabled = value.PropertyValue != "True";
            this._editor.disabled = value.PropertyValue != "True";
            
            //set background for container td
            this._editor.parentElement.style.backgroundColor = this._editor.parentElement.getAttribute(value.PropertyValue != "True" ? "DisabledBG" : "EnabledBG");

            if (this._panel)
            {
                $get(this._panel._filter.id + "_Fltc", this._panel._filter).disabled = value.PropertyValue != "True";
                this._panel._refresh.disabled = value.PropertyValue != "True";
            }
        }
        else if (value.PropertyKey == eval(Camstar.Ajax.DirectUpdateParameterKeys.Visible))
        {
            
            if (this._editor)
            {
                var parent = this._editor.parentElement;
                
                while (parent.tagName != "SPAN")
                {
                    parent = parent.parentElement;
                }
                
                parent.style.display = value.PropertyValue == "True" ? "" : "none";
                this._label.style.display = value.PropertyValue == "True" ? "" : "none";
            }
        }
    },

    open: function() {
        Camstar.WebPortal.FormsFramework.WebControls.ContainerList.callBaseMethod(this, "open");
        if (this._panel && this._panel._view) {
            var view = this._panel._view;
            var isGrid = view.control instanceof CamstarPortal.WebControls.JQGridBaseData;
            if (isGrid && view.control._responsiveWidth) {
                $(view).jqGrid("resizeGrid", 0);
            }
        }
    }
}

Camstar.WebPortal.FormsFramework.WebControls.ContainerList.registerClass('Camstar.WebPortal.FormsFramework.WebControls.ContainerList', Camstar.WebPortal.FormsFramework.WebControls.PickLists.PickListControl);

if (typeof (Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
