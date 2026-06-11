// Copyright Siemens 2023  

/// <reference path="../../MicrosoftAjaxExt.js"/>
/// <reference path="../../Camstar.UI/Control.js" />
/// <reference path="PickListCommon.js" />
/// <reference path="PickListPanel.js" />

Type.registerNamespace("Camstar.WebPortal.FormsFramework.WebControls.PickLists");

Camstar.WebPortal.FormsFramework.WebControls.PickLists.PickListControl = function (element)
{
    Camstar.WebPortal.FormsFramework.WebControls.PickLists.PickListControl.initializeBase(this, [element]);

    // Components
    this._panel = null;

    // Child elements
    this._editor = null;
    this._button = null;
    this._value = null;
    this._label = null;

    // Control properties
    this._usevalue = false;
    this._readonly = false;
    this._typeahead = false;
    this._required = false;
    this._needpostback = false;
    this._extensions = null;
    this._dependencies = false;
    this._freeformentry = false;
    this._cancelPostBackKey = false;
    this._allowDeleteKey = false;
    this._deactivateID = null;
    this._isInlineControl = false;
    this._inlineDependencies = null;
    this._controlID = null;
    this._displayPanelOnly = null;
    this._onPageLoad = false;
    this._defaultValue = null;
    this._serverType = "Camstar.WebPortal.FormsFramework.WebControls.PickLists.PickListControl, Camstar.WebPortal.FormsFramework.WebControls";
},

Camstar.WebPortal.FormsFramework.WebControls.PickLists.PickListControl.prototype =
{
    initialize: function ()
    {
        Camstar.WebPortal.FormsFramework.WebControls.PickLists.PickListControl.callBaseMethod(this, 'initialize');
        var me = this;
        var isMobile = Camstars.Browser.IsMobile;
        if (!this.get_readOnly())
        {
            if (this._editor)
            {
                $(this._editor)
                    .off()
                    .on('blur', function (e) { return me.onBlur(e); })
                    .on('change', function (e) { return me.onChange(e); });

                if (!this._freeformentry)
                {
                    $(this._editor).on('focus', function (e)
                    {
                        return me.onEditFocus(e);
                    });
                    //Fix for BUG 71019
                    $(this._editor).on('drop', function (e) {
                        e.preventDefault();
                        return;
                    });
                    //Fix for BUG 69786
                    $(this._editor).on('keydown', function (e) {
                        e.preventDefault();
                        return;
                    });
                }
                else
                {
                    $(this._editor).on('focus', function (e){
                        // set attribute to highlite the whole control
                        $(this).parent().attr('state', 'select');
                        $(me._editor).select();
                        return true;
                    });
                    $(this._editor).on('blur', function (e){
                        $(this).parent().removeAttr('state');
                        return true;
                    });
                    if (this.get_useValue()) {
                        $(this._editor).on('keydown', function (e) {
                            return me.onEditKeyDown(e);
                        });
                    }
                    if (this.get_hl_required()) {
                        $(this._editor).on('keyup', function (e) {
                            me._setRequiredState(me.get_editor());
                        });
                    }
                }

                if (this._isInlineControl && this._required)
                    $(this._editor).attr('required', 'required');
            }

            if (this._button)
            {
                $(this._button).off()
                    .on('click', function (e) { return me.onClickButton(e); })
                    .on('keydown', function (e) { return me.onKeyDown(e); })
                    .on('mouseout', function (e) { return $(me._panel._element).is(":visible") && me.onTimeOutDeactivate(e); });
                if (isMobile)
                    $(this._button).off("mouseout");

                this.setToolTip();
            }

            if (this._panel)
            {
                if (!isMobile) {
                    $(this._panel._element)
                    .off()
                    .on('mouseout', function (e) { return me.onTimeOutDeactivate(e); })
                    .on('mouseover', function (e) { return me.onCancelDeactivate(e); });
                }

                if (this._panel._viewId)
                {
                    var viewGridPgSelect = $("#" + this._panel._viewId + "_pager_center select[class='ui-pg-input']");
                    if (viewGridPgSelect.length != 0)
                    {
                        viewGridPgSelect.on('focus', function (e) { return me.onViewGridPgSelectFocus(e); })
                            .on('change', function (e) { return me.onViewGridPgSelectChange(e); });
                    }
                }
                this._panel.setup(this);
            }

            if (this._displayPanelOnly && !this.get_Hidden())
            {
                $(this._editor).css('display', 'none');
                $(this._button).css('display', 'none');
                $(this._panel._element).off();
                $(this._panel._element).addClass('standAlone');
                $(function ()
                {
                    var elements = $(me._element).children();
                    $.each(elements, function (i, val)
                    {
                        if (!$(val).attr('id') || $(val).attr('id') != $(me._label).attr('id'))
                        {
                            $(val).css('display', 'none');
                        }
                    });
                    var panel = $(me._panel._element).detach();
                    panel.appendTo($(me._element));
                    me.open();
                });
            }
        }
        else
        {
            if (this._panel)
            {
                this._panel.setup(this);
            }
            this.setToolTip();
        }
        if (this._extensions)
        {
            eval(this._extensions);
        }
        this._setRequiredState(this.get_editor());
    },

    dispose: function ()
    {
        if (this._editor)
            $(this._editor).off();
        if (this._button)
            $(this._button).off();

        Camstar.WebPortal.FormsFramework.WebControls.PickLists.PickListControl.callBaseMethod(this, 'dispose');
    },

    // --- IPickListControl Section ---
    setValue: function (val, notClose)
    {
        if (notClose == undefined)
            notClose = false;

        if (!notClose)
            this.close();

        if (val != null)
        {
            if (val.text == undefined)
            {
                // if val is not a structure
                val = { text: val, key: '', tag: null };
            }
            if (this._editor.value != val.text || this._value.value != val.key)
            {
                //check for file browse if so use key instead of text
                if (this.get_serverType().indexOf("FileBrowse") >= 0) {
                    this._editor.value = val.key;
                } else {
                this._editor.value = val.text;
                }
                this._value.value = val.key;
                if (this._cancelPostBackKey)
                {
                    // cancel postback.
                    this.onChange();
                }
                else
                {
                    // do postback.
                    if (this.get_needPostBack())
                        $(this._editor).trigger("change");
                    else
                        this.onChange();
                }
            }
        }

        if (this._typeAhead && this._panel && this._panel._filter)
            this._panel._filter.value = this._editor.value;

        this._setRequiredState(this.get_editor());
    },

    clearValue: function ()
    {
        var val = this.getValue();
        this._editor.value = '';
        this._value.value = '';
        if (this._panel)
        {
            if (this._panel._filterTextBox)
                this._panel._filterTextBox.value = "";
        }
        if (val != null && val != "")
        {
            if (this.get_needPostBack())
                $(this._editor).trigger("change");
            else
                this.onChange();
        }

        this._setRequiredState(this.get_editor());

    },

    getValue: function ()
    {
        return this._editor.value;
    },
    // --- End of IPickListControl

    setToolTip: function ()
    {
        this._editor.parentNode.title = this._editor.scrollWidth > this._editor.offsetWidth ? this._editor.value : "";
    },

    onEditFocus: function ()
    {
        if (this._button)
        {
                var me = this;
                setTimeout(function() { me._button.focus(); }, 0);

        }
    },

    onEditKeyDown: function (e)
    {
    },

    // virtual
    onEditorChange: function (e)
    {
    },

    onBlur: function (e)
    {
        var inputControl = e.target;
        inputControl.parentNode.title = inputControl.scrollWidth > inputControl.offsetWidth ? inputControl.value : "";
    },

    onChange: function (e)
    {
        this.reloadDependent();
        if (this._isInlineControl)
        {
            this.reloadDependentInlineControls();
        }
        if (typeof e == 'undefined')
        {
            // force change handler call for emulated event 
            $(this._editor).trigger('change');
        }
        this.onEditorChange(e);
        $(this).trigger('changed');
        this._setRequiredState(this.get_editor());
    },

    onTimeOutDeactivate: function (e)
    {
        if (this._panel && this._panel._element && $(this._panel._element).find(".header").length > 0)
        {
            var currentElement = this;
            e.target = $(this._panel._element).find(".header")[0];
            this._deactivateID = setTimeout(function () { currentElement.onDeactivate(e, true); }, 500);
        }
    },

    onCancelDeactivate: function (e)
    {
        clearTimeout(this._deactivateID);
    },

    onDeactivate: function (e, skipTimeout)
    {
        if (this._pagerPicklistSelected)
            return;
        var ev = this;
        var timeout = 200;
        if (skipTimeout) // prevents double timeout waiting. True when called from the onTimeOutDeactivate.
            timeout = 0;
        /*	NOTE: This functionality appears to work for picklists in IE as well.  Original code remains in tact since there are many additional checks that may apply to alternate scenarios.*/
        if (!window.event)
        {
            if (e)
            {
                if (!e.target || $(e.target).parents(".cs-picklist-panel").length == 0)
                    return;

                var parentContainer = $(e.target).parents(".cs-picklist-panel")[0];
                
                if (e.clientY + 1 + $(document).scrollTop() <= $(parentContainer).offset().top ||
                    e.clientY - 1 + $(document).scrollTop() >= ($(parentContainer).offset().top + $(parentContainer).height()) ||
                        e.clientX + 1 + $(document).scrollLeft() <= $(parentContainer).offset().left ||
                            e.clientX - 1 + $(document).scrollLeft() >= ($(parentContainer).offset().left + $(parentContainer).width()))
                {
                    //mouse is now outside parent coods, start timeout; 1-+ used to shrink client vars to avoid border triggers 
                    //setTimeout("$(document).find('.PickListPanel').css('display', 'none');", timeout);
                    if (this._element)
                    {
                        setTimeout('$find("' + this._element.id + '").deactivate("' + this._element.id + '");', timeout);
                    }
                }
            }
            return;
        }

        var elem = event.toElement;

        if (this._iframe != null)
        {
            if (this._iframe.id == event.srcElement.id)
            {
                if (elem.attributes['gn'] != null && elem.attributes['gn'].value == this.viewCtrlId)
                {
                    $(elem).on("deactivate", function () { return ev.OnDeactivate(); });
                    this.InitTabs(elem);
                    if (elem.tabIndex != -1)
                        elem.focus();
                    elem.tabIndex = -1;
                }
                return;
            }
        }

        // Ingore closing for text flyout
        if (elem && ($(elem).is('#textflyout') || $(elem).parents('#textflyout').length > 0))
        {
            return;
        }
        setTimeout('$find("' + this._element.id + '").deactivate();', timeout);
    },

    onViewGridPgSelectFocus: function (e)
    {
        if (this._panel)
            $clearHandlers(this._panel._element);
    },

    onViewGridPgSelectChange: function (e)
    {
        var me = this;
        if (this._panel)
        {
            $(this._panel._element).off()
                .on('mouseover', function (e) { return me.onViewGridPgSelectMouseOver(e); });

            if (this._panel._viewId)
            {
                var viewGridPgSelect = $("#" + this._panel._viewId + "_pager_center select[class='ui-pg-input']");
                if (viewGridPgSelect.length != 0)
                    viewGridPgSelect[0].blur();
            }
        }
    },

    onViewGridPgSelectMouseOver: function (e)
    {
        var me = this;
        if (this._panel)
        {
            $(this._panel._element).off()
                .on('mouseover', function (e) { return me.onCancelDeactivate(e); })
                .on('mouseout', function (e) { return me.onDeactivate(e); });

        }
    },

    DataChanged: function (method)
    {
        $(this._editor).on('change', method);
    },

    DataChanging: function (method)
    {
        $(this._editor).on('keyup', method);
    },

    Focus: function ()
    {
        this._editor.focus();
    },

    SetAttribute: function (name, val)
    {
        this._editor.setAttribute(name, val);
    },

    SetStyle: function (name, val)
    {
        this._editor.style[name] = val;
    },

    mouseout: function (elemId)
    {
        this.deactivate(elemId);
    },

    deactivate: function (elemId)
    { 
        if (this._panel.get_visible())
            this.close();
    },

    onClickButton: function (e, enterKeyPressed)
    {
        var isRealMouseClick = true;
        if (e.target != null)
        {
            var rect = e.target.getClientRects();
            if (rect.length > 0)
            {
                var r = rect[0];
                if ((e.clientX >= r.left && e.clientX < r.right) &&
                    (e.clientY >= r.top && e.clientY < r.bottom))
                {
                    // it's button
                }
                else
                {
                    isRealMouseClick = false || (typeof enterKeyPressed != 'undefined');
                }
            }
            if (!$(this._editor).is(':disabled')) {

                $(this._editor).removeAttr('state');
                $(this).parent().attr('state', 'select');
                $(this._editor).select();
            }
        }

        if (!isRealMouseClick)
        {
            return false;
        }

        if (!this._readonly && this._panel != null)
        {
            //this._panel.changePageNumber();
            this._panel.createLinkPages();

            /* TODO typeahead
            if (this._typeAhead)
            {
            this._editor.attachEvent('ondeactivate', function()
            {
            if (div.style.display == '' && !(ev.tabbing || ev.typing))
            ev.Close();
            });
            var filter = document.getElementById(this.filterCtrlId);
            filter.style.display = 'none';
            }
            */

            if (!this._panel.get_visible() && (typeof (__page) == 'undefined' || !__page._lock))
                this.open();
            else
                this.close();
        }

        return false;
    },

    onKeyDown: function (e)
    {
        var res = false;
        // Del
        if (this._allowDeleteKey && (e.keyCode == 8 || e.keyCode == 46))
            this.setValue({ key: '', text: '', tag: '' });
        else if (e.keyCode == 13)
            this.onClickButton(e, true);
        else
            res = true;

        this._setRequiredState(this.get_editor());

        return res;
    },


    setPanelPosition: function () {

        $(this._panel._element).height("auto");

        var $main = $(this._editor).closest("div.litepopup-webpart");
        if (!$main.length) {
            if ($("#WebPart_ProcessSpecDetailsLPWP").length) //scs: Bug 51778:Couldn't input value into search area.
                $main = this.getScrollableElement();
            else
                $main = $('body >form');
        }

        $main.append(this._panel._element);
        this._panel.show();

        var panelHeight = $(this._panel._element).outerHeight(true);

        var widthOfWrapperElement = $(this._element).width();
        var widthOfWrapperPickList = $(this._editor).parent().width();
        var $editorContainer = $(this._editor).parent();

        var pos = $editorContainer.get(0).getBoundingClientRect();    // position in contaner
        var topBelowCorrection = 6;

        var isMobileDevice = $('body').hasClass("mobile-device");
        var marginLeftPanel = isMobileDevice ? 10 : 6;

        var panelPos = { top: pos.bottom - topBelowCorrection, left: pos.left + marginLeftPanel };

        // Check if the panel is out of view
        var $viewPort = $main;
        var scrollCont = {top: 0, left:0};
        if (!$viewPort.length) {
            $viewPort = $editorContainer.closest(".form-scrollablepanel")
        }
        else {
            scrollCont.top = $viewPort.scrollTop();
            scrollCont.left = $viewPort.scrollLeft();
        }

        if ($viewPort.length) {
            var vpRect = $viewPort.get(0).getBoundingClientRect();
            panelPos.top = panelPos.top - vpRect.top + scrollCont.top;
            panelPos.left = panelPos.left - vpRect.left + scrollCont.left;

            if (( pos.bottom  - topBelowCorrection + panelHeight) > vpRect.bottom) {
                // Show above
                var topAbove = pos.top - vpRect.top + scrollCont.top - 3 - panelHeight;
                if (topAbove > 0) {
                    panelPos.top = topAbove;
                }
                else {
                    // Move on top if panelPos.top is negative
                    panelPos.top = 0;

                    // Shrink control if needed
                    var currentPanelHeight = $(this._panel._element).height();
                    var panelMarginTop = parseInt($(this._panel._element).css("margin-top"));
                    panelHeight = vpRect.height - panelMarginTop * 2;
                    if (panelHeight < currentPanelHeight) {
                        $(this._panel._element).css("overflow", "auto").height(panelHeight);
                    }
                    else {
                        $(this._panel._element).css("overflow", "visible");
                    }
                }
            }
        }

        this._panel.setPosition(panelPos.top, panelPos.left, false);
    },

    open: function ()
    {
        if (this._button)
            if (this._button.disabled === true)
                return;

        if (!this._displayPanelOnly)
        {
            this.setPanelPosition();
            this._panel.show();
            this._panel.adjust();
        }

        // TODO this.ShowIframe();

        if (this._typeAhead && !this.typing)
            this.FilterClick();
        this._panel.setFocus();

        if (this.get_isInlineControl())
        {
            this._panel.set_loaded(false);
        }
        this._panel.requestData();

        //adds MoreInfo button
        if (this._panel._view)
        {
            $(this._panel._view).addClass("force-resize");
            var grid = this._panel._view.control;
            if (grid && typeof grid.fullWidth === "function" )
                grid.fullWidth();
            var visibleRows = $("tr[role=row][id].jqgrow", $(this._panel._element));
            $('td[role=gridcell]:visible', visibleRows).each($.jgrid.cutOverflowText);
        }

        if (!this._displayPanelOnly)
        {
            var parent = this._panel._element.parentNode;
            while (parent != null && parent.id != mkUserFieldsDiv && parent.id != mkParametricDiv)
                parent = parent.parentNode;
            if (parent == null)
            {
                var divMain = this.getScrollableElement();
                var bottomPos = this._panel._element.offsetTop;
                var shadow = this._panel.get_shadow();
                if (shadow)
                    bottomPos += shadow.offsetHeight;

                var tempControl = this._panel._element.offsetParent;
                bottomPos -= tempControl.scrollTop;
                if (bottomPos > divMain.offsetHeight)
                    this._panel._element.focus();
            }
        }
        else
        {
            $(this._panel._element).css('position', 'relative');
            $(this._panel._element).css('z-index', '1');
            $(this._panel._element).show();
            this._panel.adjustStandAlone();
        }
    },

    close: function ()
    {
        if (!this._displayPanelOnly)
        {
            if (this._panel)
            {
                var that = this;
                this._panel.hide(true, function()
                {
                try
                {
                    that._element.appendChild(that._panel._element);
                }
                catch (e)
                {

                }

                // hide tooltips in flyout
                $('#textflyout').hide();
                $.jgrid.closeFlyout();

                setTimeout(function () {
                    if (!$(that._editor).is(':disabled'))
                        $(that._editor).focus();
                }, 100);
            });
            }
        }
    },

    _sendRequest: function (callParameters, serverAction, callBackMethod, returnVal)
    {
        var transition = new Camstar.Ajax.Transition(eval(Camstar.Ajax.RequestType.Command), this);
        transition.set_command(serverAction);

        var callParamsString = Sys.Serialization.JavaScriptSerializer.serialize(callParameters);
        transition.set_commandParameters(callParamsString);
        transition.set_clientCallback(callBackMethod);
        var communicator = new Camstar.Ajax.Communicator(transition, this);
        communicator.syncCall();
    },

    RefreshDependencies: function (responseSection)
    {
        __page.hideModal();
    },

    reloadDependent: function ()
    {
        if (this._dependencies != null)
        {
            for (var i = 0; i < this._dependencies.length; i++)
            {
                var ctrl = $find(this._dependencies[i]);
                if (ctrl != null)
                {
                    var panel = null;
                    if (Camstar.WebPortal.FormsFramework.WebControls.PickLists.IPickListControl.isImplementedBy(ctrl))
                    {
                        ctrl.clearValue();
                        panel = ctrl._panel;
                    }
                    if (Camstar.WebPortal.FormsFramework.WebControls.PickLists.IPickListPanel.isImplementedBy(ctrl))
                        panel = ctrl;

                    if (panel)
                    {
                        if (panel.get_visible())
                            panel.load(true);
                        else
                            panel.set_loaded(false);
                    }
                }
            }
        }
    },

    reloadDependentInlineControls: function ()
    {
        if (this._inlineDependencies != null)
        {
            for (var i = 0; i < this._inlineDependencies.length; i++)
            {
                var ctrl = $find(this._inlineDependencies[i]);
                if (ctrl != null)
                {
                    var panel = null;
                    if (Camstar.WebPortal.FormsFramework.WebControls.PickLists.IPickListControl.isImplementedBy(ctrl))
                    {
                        ctrl.clearValue();
                        panel = ctrl._panel;
                    }
                    if (Camstar.WebPortal.FormsFramework.WebControls.PickLists.IPickListPanel.isImplementedBy(ctrl))
                        panel = ctrl;

                    if (panel)
                    {
                        var parameters =
                                {
                                    "DataValue": this.getValue(),
                                    "ControlID": this._element.id,
                                    "CurrentCallStackKey": __page.getCurrentCallStackKey()
                                };

                        this._sendRequest(parameters, "SetDataToPickListControl", "RefreshDependencies", false);
                        panel._filterCompleted = false;
                        panel.load(true);
                        panel.set_loaded(false);
                    }
                }
                else
                {
                    alert("Dependent control '" + this._dependencies[i] + "' not found on the form.");
                }
            }
            // Set focus to the current control
            if (!$(this._editor).is(':disabled'))
                $(this._editor).focus();
        }
    },

    _setRequiredState: function (inpCtl)
    {
        var hl_css = this.get_hl_required();
        if (hl_css && this.get_required())
        {
            if (inpCtl.value.length > 0)
                $(inpCtl).parent().removeClass(hl_css);
            else
                $(inpCtl).parent().addClass(hl_css);
        }
    },
    
    getScrollableElement: function()
    {
        var scrollableElement = __page.getScrollableElement();
        
        // if pick list on the light popup.
        if ($(this._editor).closest('.ui-dialog').is(':visible'))
        {
            scrollableElement = $(this._editor).parents('.ui-dialog-content');
            scrollableElement.css('position', 'relative');
        }

        if (scrollableElement.length == 0)
        {
            // if it is not framing - take a whole window
            scrollableElement = $('body');
        }

        return scrollableElement;
    },

    get_serverType: function () { return this._serverType; },

    get_controlId: function () { return this._controlID; },

    get_Data: function () { return this.getValue(); },
    set_Data: function (value) { this.setValue(value, false); },

    get_Hidden: function () { return this._element.style.display == 'none'; },
    set_Hidden: function (value)
    {
        if (value == true)
        {
            this._label.style.display = 'none';
            this._element.style.display = 'none';
        }
        else
        {
            this._label.style.display = '';
            this._element.style.display = '';
        }
    },

    get_Disabled: function () { return this._element.disabled; },
    set_Disabled: function (value)
    {
        this._element.disabled = value;
        this._editor.disabled = value;
        this._button.disabled = value;
        if (value == true)
        {
           $(this._editor).parent().attr("disabled", "disabled");
        }
        else
        {
           $(this._editor).parent().removeAttr("disabled");
        }
    },

    get_ReadOnly: function () { return this._editor.readonly; },
    set_ReadOnly: function (value)
    {
        this._editor.readOnly = value;
        this._button.disabled = value;
        if (value == true)
        {
            $(this._editor).parent().attr("readonly", "readonly");
            $(this._button).hide();
        }
        else
        {
            $(this._editor).parent().removeAttr("readonly");
            $(this._button).show();
        }
    },

    get_IsEmpty: function () { return !this.getValue(); },

    get_editor: function () { return this._editor; },
    set_editor: function (value) { this._editor = value; },

    get_label: function () { return this._label; },
    set_label: function (value) { this._label = value; },

    get_button: function () { return this._button; },
    set_button: function (value) { this._button = value; },

    get_value: function () { return this._value; },
    set_value: function (value) { this._value = value; },

    get_panel: function () { return this._panel; },
    set_panel: function (value) { this._panel = value; },

    get_useValue: function () { return this._usevalue; },
    set_useValue: function (value) { this._usevalue = value; },

    get_readOnly: function () { return this._readonly; },
    set_readOnly: function (value) { this._readonly = value; },

    get_typeAhead: function () { return this._typeahead; },
    set_typeAhead: function (value) { this._typeahead = value; },

    get_needPostBack: function () { return this._needpostback; },
    set_needPostBack: function (value) { this._needpostback = value; },

    get_extensions: function () { return this._extensions; },
    set_extensions: function (value) { this._extensions = value; },

    get_dependencies: function () { return this._dependencies; },
    set_dependencies: function (value) { this._dependencies = value; },

    get_freeFormEntry: function () { return this._freeformentry; },
    set_freeFormEntry: function (value) { this._freeformentry = value; },

    get_cancelPostBackKey: function () { return this._cancelPostBackKey; },
    set_cancelPostBackKey: function (value) { this._cancelPostBackKey = value; },

    get_allowDeleteKey: function () { return this._allowDeleteKey; },
    set_allowDeleteKey: function (value) { this._allowDeleteKey = value; },

    get_isInlineControl: function () { return this._isInlineControl; },
    set_isInlineControl: function (value) { this._isInlineControl = value; },

    get_inlineDependencies: function () { return this._inlineDependencies; },
    set_inlineDependencies: function (value) { this._inlineDependencies = value; },

    get_required: function () { return this._required; },
    set_required: function (value) { this._required = value; },

    get_displayPanelOnly: function () { return this._displayPanelOnly; },
    set_displayPanelOnly: function (value) { this._displayPanelOnly = value; },

    get_onPageLoad: function () { return this._onPageLoad; },
    set_onPageLoad: function (value) { this._onPageLoad = value; },

    get_defaultValue: function () { return this._defaultValue; },
    set_defaultValue: function (value) { this._defaultValue = value; }
}

Camstar.WebPortal.FormsFramework.WebControls.PickLists.PickListControl.registerClass('Camstar.WebPortal.FormsFramework.WebControls.PickLists.PickListControl', Camstar.UI.Control, Camstar.WebPortal.FormsFramework.WebControls.PickLists.IPickListControl);

if (typeof (Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
