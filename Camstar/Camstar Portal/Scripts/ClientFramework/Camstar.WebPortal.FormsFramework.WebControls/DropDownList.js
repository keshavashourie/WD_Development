// Copyright Siemens 2023  

/// <reference path="../MicrosoftAjaxExt.js"/>
/// <reference path="../Camstar.UI/Control.js" />
/// <reference path="PickLists/PickListCommon.js" />
/// <reference path="PickLists/PickListControl.js" />
/// <reference path="PickLists/PickListPanel.js" />

Type.registerNamespace("Camstar.WebPortal.FormsFramework.WebControls");
Type.registerNamespace("Camstar.WebPortal.FormsFramework.WebControls.PickLists");

Camstar.WebPortal.FormsFramework.WebControls.DropDownList = function(element)
{
    Camstar.WebPortal.FormsFramework.WebControls.DropDownList.initializeBase(this, [element]);
}

Camstar.WebPortal.FormsFramework.WebControls.DropDownList.prototype =
{
    initialize: function ()
    {
        Camstar.WebPortal.FormsFramework.WebControls.DropDownList.callBaseMethod(this, 'initialize');
        if (this.get_useValue())
        {
            $(this._editor).val(this.resolveEnumText(this._value.value));
            this._setRequiredState(this.get_editor());
        }
    },

    dispose: function ()
    {
        Camstar.WebPortal.FormsFramework.WebControls.DropDownList.callBaseMethod(this, 'dispose');
    },

    onEditorChange: function (e)
    {
        if (this.get_useValue())
            this._value.value = this.resolveTextToEnum(this._editor.value);
    },

    resolveEnumText: function (val)
    {
        var textval = '';
        if (val && parseInt(val) != NaN)
        {
            if (this._panel)
            {
                // Try to load tree nodes if they aren't be loaded
                var initData = this._panel.get_initialData();
                if (initData)
                {
                    textval = this.findTextByKey(initData.rows, val);
                }
            }
        }
        return textval;
    },

    resolveTextToEnum: function (text)
    {
        var keyval = '';
        if (this._panel)
        {
            // Try to load tree nodes if they aren't be loaded
            var initData = this._panel.get_initialData();
            if (initData)
            {
                keyval = this.findKeyByText(initData.rows, text);
            }
        }
        return keyval;
    },

    // includes tree structure.
    findKeyByText: function (rows, text) {
        var me = this;
        var returnValue = "";
        if (rows) {
            $.each(rows, function (index, value) {
                if (value.text == text || $.trim(value.text) == text) {
                    returnValue = value.li_attr.key;
                    return false;
                }
                else if (value.children)
                    returnValue = me.findKeyByText(value.children, text);
                if (returnValue)
                    return false;
            });
        }
        return returnValue;
    },

    // includes tree structure.
    findTextByKey: function (rows, key) {
        var me = this;
        var returnValue = "";
        if (rows) {
            $.each(rows, function (index, value) {
                if (value.li_attr.key == key) {
                    returnValue = value.text;
                    return false;
                }
                else if (value.children)
                    returnValue = me.findTextByKey(value.children, key);
                if (returnValue)
                    return false;
            });
        }
        return returnValue;
    },

    directUpdate: function (value)
    {
        Camstar.WebPortal.FormsFramework.WebControls.DropDownList.callBaseMethod(this, 'directUpdate');

        if (value.PropertyKey == eval(Camstar.Ajax.DirectUpdateParameterKeys.Data))
        {
            if (this._value)
            {
                $(this._value).val(value.PropertyValue);
                $(this._editor).val(this.resolveEnumText(value.PropertyValue));
                this.onChange();
            }
        }
        else if (value.PropertyKey == eval(Camstar.Ajax.DirectUpdateParameterKeys.DataText))
        {
            if (this._editor)
            {
                this._editor.value = value.PropertyValue;
            }
            if (this._value)
            {
                this._value.value = this.resolveTextToEnum(value.PropertyValue);
            }
            this._setRequiredState(this.get_editor());
        }
        else if (value.PropertyKey == eval(Camstar.Ajax.DirectUpdateParameterKeys.Enable))
        {
            //set disable for span element  
            var spanElement = this.get_element();
            spanElement.disabled = value.PropertyValue != "True";

            //set disable for main table element
            var tableMain = $(".PickListInputControl", this.get_element())[0];
            if (tableMain)
            {
                tableMain.disabled = value.PropertyValue != "True";
                //set disable for child table element
                tableMain.rows[0].cells[0].childNodes[0].disabled = value.PropertyValue != "True";
            }

            this._button.disabled = value.PropertyValue != "True";
            this._editor.disabled = value.PropertyValue != "True";

            //set background for container td
            this._editor.parentNode.style.backgroundColor = this._editor.parentNode.getAttribute(value.PropertyValue != "True" ? "DisabledBG" : "EnabledBG");
            if (this._panel)
            {
                if (this._panel._filter )
                    $get(this._panel._filter.id + "_Fltc", this._panel._filter).disabled = value.PropertyValue != "True";
                if (this._panel._refresh)
                    this._panel._refresh.disabled = value.PropertyValue != "True";
            }
        }
        else if (value.PropertyKey == eval(Camstar.Ajax.DirectUpdateParameterKeys.Visible))
        {
            if (this._editor)
            {
                var parent = this._editor.parentNode;

                while (parent.tagName != "SPAN")
                {
                    parent = parent.parentNode;
                }

                parent.style.display = value.PropertyValue == "True" ? "" : "none";
                this._label.style.display = value.PropertyValue == "True" ? "" : "none";
            }
        }
    },

    // overriden
    getValue: function ()
    {
        if (this.get_useValue())
        {
            if (this.get_useValue() && this._editor && this._editor.value && (this._value.value == null || this._value.value == ''))
            {
                this._value.value = this.resolveTextToEnum(this._editor.value);
            }
            return this._value.value;
        }
        else
        {
            return this._editor.value;
        }
    },


    isDropDown: function ()
    {
        return true; 
    }
}

Camstar.WebPortal.FormsFramework.WebControls.DropDownList.registerClass('Camstar.WebPortal.FormsFramework.WebControls.DropDownList', Camstar.WebPortal.FormsFramework.WebControls.PickLists.PickListControl);

if (typeof (Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
