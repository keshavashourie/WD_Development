// Copyright Siemens 2023  

/// <reference name="MicrosoftAjax.js"/>
/// <reference path="../Camstar.UI/Control.js" />
/// <reference path="PickLists/PickListCommon.js" />
/// <reference path="PickLists/PickListControl.js" />
/// <reference path="PickLists/PickListPanel.js" />

Type.registerNamespace("Camstar.WebPortal.Personalization");

/******************* Camstar.WebPortal.Personalization.CollapsableState *******************/
if (Camstar.WebPortal.Personalization.RDOFormatType == undefined) {
    Camstar.WebPortal.Personalization.RDOFormatType = function () { };

    Camstar.WebPortal.Personalization.RDOFormatType.prototype =
    {
        Standard: 0,
        Revision: 1,
        ROR: 2
    };
    Camstar.WebPortal.Personalization.RDOFormatType.registerEnum("Camstar.WebPortal.Personalization.RDOFormatType", false);
}

Type.registerNamespace("Camstar.WebPortal.FormsFramework.WebControls");

Camstar.WebPortal.FormsFramework.WebControls.RevisionedObject = function(element)
{
    Camstar.WebPortal.FormsFramework.WebControls.RevisionedObject.initializeBase(this, [element]);

    this._baseId = element.id;
    this._formatType = null;
    this._div = null;
    this._hidden = null;    
    this._onFocusFired = 0;

}

Camstar.WebPortal.FormsFramework.WebControls.RevisionedObject.prototype =
{
    initialize: function ()
    {
        Camstar.WebPortal.FormsFramework.WebControls.RevisionedObject.callBaseMethod(this, 'initialize');

        var $nameInput = this._$getControl("Edit");
        var $revisionInput = this._$getControl("Rev");
        var $isRORCheckBox = this._$getControl("IsRevisionCheckBox");

        var me = this;

        if ($isRORCheckBox.length)
        {
            $isRORCheckBox.after("<label for='" + $isRORCheckBox.attr("id") + "'></label>");
        };

        if ($nameInput.length && $revisionInput.length)
        {
            $nameInput.prop('disabled',!this._freeformentry);
            $revisionInput.prop('disabled', !this._freeformentry);
            if (!$isRORCheckBox.prop('disabled')) // keep disabled attribute when RO is readonly.
                $isRORCheckBox.prop('disabled', !this._freeformentry);

            if (this._freeformentry)
            {
                $nameInput
                    .bind('blur', function(e) { return me._onBlurName(e); })
                    .bind('focus', function (e) { return me._onFocusName(e); })
                    .bind('mousedown', function (e) { e.stopImmediatePropagation(); return true; });

                $revisionInput.bind('mousedown', function (e) { e.stopImmediatePropagation();  return true; });
            }
            else
            {
                $nameInput
                    .bind('blur', function (e) { return me._onBlurName(e); })
                    .bind('focus', function (e) {return me.onEditFocus(e); });
            }

            $nameInput.on('smartScanningChange', function () {
                var nameInput = me._$getControl("Edit");
                var actualInputHidden = me._$getControl('Hidden1');
                if (!actualInputHidden.val())
                    actualInputHidden.val(nameInput.val());
            });

            $isRORCheckBox.on('focus', function (x) {
                $isRORCheckBox.parent().addClass('checkbox-focused');
            });

            $isRORCheckBox.on('blur', function (x) {
                $isRORCheckBox.parent().removeClass('checkbox-focused');
            });

            $isRORCheckBox.change(function(e)
            {
                me._setControls(this.checked);
                me._setRequiredStateForRevision($revisionInput);
            });

            if ($isRORCheckBox.length && (this._formatType == eval(Camstar.WebPortal.Personalization.RDOFormatType.ROR)))
            {
                this._setControls($isRORCheckBox[0].checked);
            };
        }
        if (this._formatType == eval(Camstar.WebPortal.Personalization.RDOFormatType.ROR))
        {
            $isRORCheckBox[0].checked = true;
            $isRORCheckBox[0].disabled = true;
            $revisionInput[0].disabled = true;
        }
        if (this._formatType == eval(Camstar.WebPortal.Personalization.RDOFormatType.Revision))
        {
            $isRORCheckBox.hide();
            var $isRORCheckBoxLabel = $("label[for='" + $isRORCheckBox.attr("id") + "']");
            $isRORCheckBoxLabel.hide();
        }

        if (this.get_hl_required() && $revisionInput.length && !$revisionInput.prop('disabled'))
        {
            $revisionInput.bind('keyup', function (e)
            {
                me._setRequiredStateForRevision($revisionInput);
            });
        }
        this._setRequiredStateForRevision($revisionInput);

        if ($isRORCheckBox.prop('checked'))
        {
            $isRORCheckBox.parent().attr("checked", "checked");
        }
        else
        {
            $isRORCheckBox.parent().removeAttr("checked");
        }
    },

    _getControl: function(suffix)
    {
        return $get(this._baseId + "_" + suffix);
    },

    _$getControl: function (suffix)
    {
        return $('#' + this._baseId + "_" + suffix);
    },

    _setControls: function (isRORchecked)
    {
        var $revision = this._$getControl("Rev");
        if (isRORchecked)
        {
            $revision.prop("disabled", true)
                .removeClass("cs-revision-edit")
                .addClass("cs-revision-edit-disabled")
                .val('');
            this._setRequiredStateForRevision($revision);

            this._$getControl("IsRevisionCheckBox").parent().attr("checked", "checked");
        }
        else
        {
            $revision.prop("disabled", false)
                .removeClass("cs-revision-edit-disabled")
                .addClass("cs-revision-edit");

            if (!this._freeformentry) // <-- strange code
                $revision.prop("disabled", false);

            this._$getControl("IsRevisionCheckBox").parent().removeAttr("checked");
        }
    },

    dispose: function ()
    {
        Camstar.WebPortal.FormsFramework.WebControls.RevisionedObject.callBaseMethod(this, 'dispose');
    },
    
    directUpdate: function(value)
    {
        Camstar.WebPortal.FormsFramework.WebControls.RevisionedObject.callBaseMethod(this, 'directUpdate', [value]);
        
        if (value.PropertyKey == eval(Camstar.Ajax.DirectUpdateParameterKeys.Data))
        {
            var val = value.PropertyValue;
            var NameRevRor = this.parseStringVal(val);
            this.setRDOValue(NameRevRor);
        }
        else if (value.PropertyKey == eval(Camstar.Ajax.DirectUpdateParameterKeys.Enable))
        {
            var isDisabled = value.PropertyValue != "True";
            this._$getControl("Edit").parent().prop('disabled', isDisabled);
            this._$getControl("Edit").prop('disabled', isDisabled);
            this._$getControl("Rev").prop('disabled', isDisabled);
            this._$getControl("IsRevisionCheckBox").prop('disabled', isDisabled);
        }
        else if (value.PropertyKey == eval(Camstar.Ajax.DirectUpdateParameterKeys.Visible)) {

            if (this._editor) {
                var parent = this._editor.parentNode;

                while (parent.tagName != "SPAN") {
                    parent = parent.parentNode;
                }

                parent.style.display = value.PropertyValue == "True" ? "" : "none";
                if (this._label)
                    this._label.style.display = value.PropertyValue == "True" ? "" : "none";
            }
        }
    },

    _onFocusName: function (e)
    {
        this._onFocusFired = 1;
    },

    _onBlurName: function (e)
    {
        var $nameInput = this._$getControl("Edit");
        var $revInput = this._$getControl("Rev");
        var $actualInputHidden = this._$getControl('Hidden1');
         var $hidden = this._$getControl('Hidden');
        var $useRor = this._$getControl("IsRevisionCheckBox");
        if (this._onFocusFired == 1)
        {
            var needFireDataChanged = $actualInputHidden.val() != $nameInput.val();
            $actualInputHidden.val($nameInput.val());
            this._onFocusFired = 0;
            if (needFireDataChanged)
            {
                var NameRevROR = this.createNameRevRor($nameInput.val(), $revInput.val(), $useRor[0].checked);
                var strValue = JSON.stringify(NameRevROR);
                $hidden.val(encodeURIComponent(strValue));
                $hidden.change();
            }
        }

        var tooltip = $nameInput.val();
        if ($revInput.val().length > 0)
        {
            tooltip += " " + getRevisionDelimiter() + " " + $revInput.val(); 
        }
        if ($nameInput.val() == $actualInputHidden.val()) // if nameInputControl.value ends with '...'
            tooltip = '';

        $nameInput.parent().prop("title", tooltip);

        if ($nameInput.val().length == 0)
        {
            $revInput.val("");
            if ($useRor[0].checked && this._formatType != eval(Camstar.WebPortal.Personalization.RDOFormatType.ROR))
                $useRor[0].checked = false;
        }
        if ($useRor.prop('checked'))
        {
            $useRor.parent().attr('checked', 'checked');
        }
        else
        {
            $useRor.parent().removeAttr('checked');
        }
        return true;
    },

    // --- IPickListControl Section ---
    setValue: function (val)
    {
        var value = '';
        var key = null;
        if (typeof(val) == "string")
        {
            value = this.parseStringVal(val);
        } 
        else
        {
            if (val != null)
            {
                if (val.tag != null)
                {
                    var encodedValue = val.tag;
                    value = eval('(' + decodeURIComponent(encodedValue) + ')');
                    key = val.key;
                }
                else
                {
                    value = val.text;
                }
            }    
        }
        if (key != null && key != "")
            this.setRDOValueWithDataKey(value, key);
        else
            this.setRDOValue(value);

        Camstar.WebPortal.FormsFramework.WebControls.NamedObject.callBaseMethod(this, 'setValue');

        this.onChange();
    },

    setRDOValueWithDataKey: function (value, key)
    {
        if (this._value != null)
        {
            if (this._value.value != key)
            {
                this._value.value = key;
            }
        }

        if (!this.setRDOValue(value) && this._needpostback)
        {
            if (this._cancelPostBackKey)
            {
                // cancel postback.
                this.onChange();
            }
            else
            {
                // do postback.
                this._$getControl("Hidden").change();
            }
        }
    },

    //Set value into the name and revision fields, check ror field if necessary
    setRDOValue: function (value)
    {
        var NameRevROR = value;
        if (typeof(value) == 'undefined')
            NameRevROR = this.createNameRevRor('', '', false);
        else if (typeof(value) == "string")
            NameRevROR = this.parseStringVal(value);
        
        var $nameInput = this._$getControl("Edit");
        var $revInput = this._$getControl("Rev");
        var $actualHidden = this._$getControl("Hidden1");

        //Set value to the Name field
        $nameInput.val(NameRevROR.Name);
        $actualHidden.val(NameRevROR.Name);
        $revInput.val( NameRevROR.Revision);

        var useROR = $revInput.val() == "";
        if (NameRevROR.Name == "")
            useROR = false;

        //check checkbox when 1) no particular revision is selected and  2) active revision is selected from the picklist
        this._$getControl("IsRevisionCheckBox").prop('checked', useROR || NameRevROR.Ror);
        this._setControls(useROR);

        this._setRequiredState($nameInput[0]);
        this._setRequiredStateForRevision($revInput);

        return this.setRDOHiddenFieldValue(NameRevROR);  
    },

    clearValue: function () {
        this.setRDOValue();
    },

    getValue: function ()
    {
        if (this._onFocusFired == 1)
            this._onBlurName();
        var nameInput = this.getNameInputValue();
        var revInput = this._getControl("Rev");
        // Return serialized readable string
        if (nameInput == '')
            return '';
        else if (revInput == null || revInput.value == '')
            return nameInput;
        else
            return nameInput + getRevisionDelimiter() + revInput.value;
    },

    //Set`s value into the RDO control`s hidden field, fires DataChanged event in case value differs from the previous
    setRDOHiddenFieldValue: function (newValue)
    {
        var $hidden = this._$getControl("Hidden");
        var encodedValue = $hidden.val();
        var prevValue;
        if (encodedValue)
            prevValue = eval('(' + decodeURIComponent(encodedValue) + ')');
        else
            prevValue = this.createNameRevRor('', '', false);

        if (prevValue.useROR == null)
            prevValue.useROR = false;
        if (newValue.Name != prevValue.Name || newValue.Revision != prevValue.Revision)
        {
            var strValue = JSON.stringify(newValue);
            $hidden.val(encodeURIComponent(strValue));
            if (this._cancelPostBackKey)
            {
                // cancel postback.
                this.onChange();
            }
            else
            {
                // do postback.
                $hidden.change();
            }
            return true;
        }

        return false;
    }, 

    //Handles div`s ondeactivate event.
    //Checks if focus is not on the div or it`s child controls.
    //Gathers RDO control`s data and checks if it was changed, fires DataChanged event in this case.
    fireDataChanged: function ()
    {
        if (this.isRDODataChangeFinished())
        {
            if ( $(this._element).is(':visible'))
            {
                var rdoValueStruct = this.createNameRevRor(this._$getControl("Edit").val(), this._$getControl("Rev").val(),this._$getControl("IsRevisionCheckBox").prop('checked'));
                this.setRDOHiddenFieldValue(rdoValueStruct);
            } //if
        } //if
    }, //FireDataChanged

    //Checks if any of the child controls of the ambient div is in focus
    isRDODataChangeFinished: function ()
    {
        if (document != null)
        {
            if (document.activeElement != null)
            {
                var active = document.activeElement;
                if (IsElementChildOf(active, this._div))
                {
                    return false;
                } //if
                else { return true; } //else
            } //if
            else { return true; } //else
        } //if
        else { return true; } //else
    }, //IsRDODataChangeFinished

    getNameInputValue: function ()
    {
        var actualInputHiddenCtl = this._getControl('Hidden1');
        if (actualInputHiddenCtl == null)
            return "";

        if (actualInputHiddenCtl.value.length)
            return actualInputHiddenCtl.value;
        else
            return this._getControl("Edit").value;
    },
    
    createNameRevRor: function(name, rev, ror)
    {
        var NameRevROR = {Name: name, Revision: rev, Ror: ror};
        if (!NameRevROR.Name)
            NameRevROR.Ror = false;
        return NameRevROR;
    },
    
    parseStringVal: function(val)
    {
        var idx = val.indexOf(getRevisionDelimiter());
        var name = val;
        var rev = "";
        if (idx >= 0) {
            name = val.substring(0, idx);
            rev = val.substring(idx+1);
        }
        var NameRevRor = this.createNameRevRor(name, rev, rev.length === 0);

        if (!NameRevRor.Name)
            NameRevRor.Ror = false;
        return NameRevRor;
    },

    _setRequiredStateForRevision: function ($rev)
    {
        var hl_css = this.get_hl_required();
        if (hl_css && this.get_required())
        {
            if ($rev.val().length > 0 || $rev.prop('disabled'))
                $rev.removeClass(hl_css);
            else
                $rev.addClass(hl_css);
        }
    },

    get_div: function () { return this._div; },
    set_div: function (value) { this._div = value; },

    get_hidden: function () { return this._hidden; },
    set_hidden: function (value) { this._hidden = value; },

    get_actualInputHiddenControl: function () { return this._actualInputHiddenControl; },
    set_actualInputHiddenControl: function (value) { this._actualInputHiddenControl = value; },

    get_formatType: function () { return this._formatType; },
    set_formatType: function (value) { this._formatType = value; },

    get_isRORCheckBox: function () { return this._getControl("IsRevisionCheckBox"); },
    set_isRORCheckBox: function (value) { }
}

Camstar.WebPortal.FormsFramework.WebControls.RevisionedObject.registerClass('Camstar.WebPortal.FormsFramework.WebControls.RevisionedObject', Camstar.WebPortal.FormsFramework.WebControls.NamedObject);

if (typeof (Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
