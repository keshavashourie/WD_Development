// Copyright Siemens 2023 

/// <reference path="../MicrosoftAjaxExt.js"/>
/// <reference path="../Camstar.UI/Control.js" />
/// <reference path="PickLists/PickListCommon.js" />
/// <reference path="PickLists/PickListControl.js" />
/// <reference path="PickLists/PickListPanel.js" />
/// <reference path="RevisionedObject.js" />

Type.registerNamespace("Camstar.WebPortal.FormsFramework.WebControls");

Camstar.WebPortal.FormsFramework.WebControls.WorkflowNavigator = function (element)
{
    if (element)
    {
        Camstar.WebPortal.FormsFramework.WebControls.WorkflowNavigator.initializeBase(this, [element]);
        this.$toStepControl = null;
        this._toStepControlID = null;
        
        this._toStackControl = null;
    }
}

Camstar.WebPortal.FormsFramework.WebControls.WorkflowNavigator.prototype =
{
    initialize: function ()
    {
        Camstar.WebPortal.FormsFramework.WebControls.WorkflowNavigator.callBaseMethod(this, 'initialize');
    },

    dispose: function () {
        var ctrls = $(this.get_element()).data("controls") || [];

        if (this.get_toStepControlID()) 
            ctrls.push(this.get_toStepControlID()+ "_UIComponent");

        if (this.get_toStackControl())
            ctrls.push(this.get_toStackControl().id + "_UIComponent");

        $(this.get_element()).data("controls", ctrls);

        Camstar.WebPortal.FormsFramework.WebControls.RevisionedObject.callBaseMethod(this, 'dispose');
    },

    //Set values into the name, revision and step fields, check ror field if necessary
    setValue: function (value)
    {
        var tag = decodeURIComponent(value.tag);
        this.$toStepControl = $("#" + this._toStepControlID);
        var NameRevRORStep = eval('(' + tag + ')');

        if (typeof NameRevRORStep.Info == "undefined")
        {
            // it's not a step - selection ignored
            return false;
        }

        //Set value to the Name field
        this._$getControl("Edit").val(NameRevRORStep.Name);
        this._$getControl("Hidden1").val(NameRevRORStep.Name);
        //Clear revision control
        this._$getControl("Rev").val(NameRevRORStep.Revision);
        //Set value to the Step field
        this.$toStepControl.val(NameRevRORStep.Info);

        //Presence of revision value determines whether ror is selected
        NameRevRORStep.Ror = NameRevRORStep.Revision ? false : true;
        var rorChk = this._$getControl("IsRevisionCheckBox");
        if (NameRevRORStep.Ror == true)
        {
            if (!rorChk[0].checked)
                rorChk.click();
        } //if
        else
        {
            if (rorChk[0].checked)
                rorChk.click();
        } //else

        NameRevRORStep.Ror = rorChk[0].checked;

        //this will close the picklist panel:
        Camstar.WebPortal.FormsFramework.WebControls.WorkflowNavigator.callBaseMethod(this, 'close');
        this.setRDOHiddenFieldValue(NameRevRORStep);

        $(this._toStackControl).val('');
        if (typeof value.parentTags != "undefined")
        {
            this.setStackHiddenFieldValue(value.parentTags);
        }
    }, //SetRDOValue

    //Set`s value into the RDO control`s hidden field, fires DataChanged event in case value differs from the previous
    setRDOHiddenFieldValue: function (newValue)
    {
        var prevValue = this._$getControl("Hidden");
        if (newValue.Name != prevValue.Name
            || newValue.Revision != prevValue.Revision)
        {
            this._$getControl("Hidden").val(newValue);
            if (this._cancelPostBackKey)
                // cancel postback.
                this.onChange();
            else
                // do postback.
                this._$getControl("Hidden").change();
            return true;
        } //if
    }, //SetRDOHiddenFieldValue
    
    setStackHiddenFieldValue: function (parentTags) 
    {
        if (parentTags.length > 0) {
            var decodedTags = [];
            $.each(parentTags, function (index, value)
            {
                decodedTags.push(eval('(' + decodeURIComponent(value) + ')'));
            });

            var strValue = JSON.stringify(decodedTags);
            $(this._toStackControl).val(encodeURIComponent(strValue));
        }
    },

    clearValue: function () {
        this._$getControl("Edit").val('');
        this._$getControl("Hidden").val('');
        this._$getControl("Rev").val('');
        $(this._toStackControl).val('');
        $("#" + this._toStepControlID).val('');
    },

    get_toStepControlID: function () { return this._toStepControlID; },
    set_toStepControlID: function (value) { this._toStepControlID = value.id + "_Edit"; },
    
    get_toStackControl: function () { return this._toStackControl; },
    set_toStackControl: function (value) { this._toStackControl = value; }
}

Camstar.WebPortal.FormsFramework.WebControls.WorkflowNavigator.registerClass('Camstar.WebPortal.FormsFramework.WebControls.WorkflowNavigator', Camstar.WebPortal.FormsFramework.WebControls.RevisionedObject);

if (typeof (Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
