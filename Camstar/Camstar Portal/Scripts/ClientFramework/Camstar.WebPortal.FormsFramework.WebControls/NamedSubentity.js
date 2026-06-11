// Copyright Siemens 2023  

/// <reference name="MicrosoftAjax.js"/>
/// <reference path="../Camstar.UI/Control.js" />
/// <reference path="PickLists/PickListCommon.js" />
/// <reference path="PickLists/PickListControl.js" />
/// <reference path="PickLists/PickListPanel.js" />

Type.registerNamespace("Camstar.WebPortal.FormsFramework.WebControls");

Camstar.WebPortal.FormsFramework.WebControls.NamedSubentity = function (element)
{
    Camstar.WebPortal.FormsFramework.WebControls.NamedSubentity.initializeBase(this, [element]);
    this._parentDataID = null;
    this._showParent = true;
},

Camstar.WebPortal.FormsFramework.WebControls.NamedSubentity.prototype =
{
    initialize: function ()
    {
        Camstar.WebPortal.FormsFramework.WebControls.NamedSubentity.callBaseMethod(this, 'initialize');

        this._parentText = null;
    },

    dispose: function ()
    {
        Camstar.WebPortal.FormsFramework.WebControls.NamedSubentity.callBaseMethod(this, 'dispose');
    },

    setValue: function (val)
    {
        Camstar.WebPortal.FormsFramework.WebControls.NamedSubentity.callBaseMethod(this, 'setValue', [val]);
        this._parentText = val.parent;
        if (!this._parentText && val.tag)
            this._parentText = val.tag;
        if (this._parentDataID)
        {
            var parentHidden = $("#" + this._parentDataID);
            if (parentHidden.length > 0)
                parentHidden.val(this._parentText);
        }

        var $ed = $(this._editor);
        if ($ed.length)
        {
            (this._parentText !== "" && this._showParent) ? $ed.val(this._parentText + ':' + val.text) : $ed.val(val.text);
        }

    },

    getValue: function ()
    {
        var value = Camstar.WebPortal.FormsFramework.WebControls.NamedSubentity.callBaseMethod(this, 'getValue');
        var separator = ':';
        if (value && this._parentText)
            return value.search(separator) === -1 ? this._parentText + separator + value : value;
        return value;
    },
    
    get_parentDataID: function () { return this._parentDataID; },
    set_parentDataID: function(value)
    {
         this._parentDataID = value;
    },
    get_showParent: function () { return this._showParent; },
    set_showParent: function (value)
    {
        this._showParent = value;
    }
},

Camstar.WebPortal.FormsFramework.WebControls.NamedSubentity.registerClass('Camstar.WebPortal.FormsFramework.WebControls.NamedSubentity', Camstar.WebPortal.FormsFramework.WebControls.NamedObject);

if (typeof (Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
