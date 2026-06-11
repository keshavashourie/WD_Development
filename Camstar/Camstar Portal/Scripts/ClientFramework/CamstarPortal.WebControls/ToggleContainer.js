// Copyright Siemens 2023  

/// <reference name="MicrosoftAjax.js"/>
/// <reference path="~/Scripts/ClientFramework/Camstar.UI/Control.js" />
Type.registerNamespace("Camstar.WebPortal.Personalization");

/******************* Camstar.WebPortal.Personalization.CollapsableState *******************/
if (Camstar.WebPortal.Personalization.CollapsableState == undefined) 
{
    Camstar.WebPortal.Personalization.CollapsableState = function() { };

    Camstar.WebPortal.Personalization.CollapsableState.prototype =
    {
        Collapsed: 0,
        Expanded: 1
    };
    Camstar.WebPortal.Personalization.CollapsableState.registerEnum("Camstar.WebPortal.Personalization.CollapsableState", false);    
}

/******************* Camstar.WebPortal.PortalFramework.ToggleContainer *******************/
Camstar.WebPortal.PortalFramework.ToggleContainer = function (element)
{
    Camstar.WebPortal.PortalFramework.ToggleContainer.initializeBase(this, [element]);
    this._collapsableTableId = null;
    this._collapseImageId = null;
    this._collapseImage = null;
    this._collapseTable = null;
    this._controlID = null;
    this._hidden = false;
    this._hiddenFieldId = null;
    this._hiddenField = null;
    this._labels = null;
    this._clientToggle = null;
    this._defaultStateSpecified = null;
    this._events = null;
},

Camstar.WebPortal.PortalFramework.ToggleContainer.prototype =
{
    initialize: function ()
    {
        Camstar.WebPortal.PortalFramework.ToggleContainer.callBaseMethod(this, 'initialize');

        this._toggleDelegate = Function.createDelegate(this, this._onToggleImage);

        if (this.get_collapseImageId())
            this._collapseImage = $get(this.get_collapseImageId()).parentNode;

        if (this._collapseImage)
        {
            $addHandlers(this._collapseImage, { 'click': this._toggleDelegate }, this);
            $addHandlers(this._collapseImage, { 'keypress': this._toggleDelegate }, this);
        }

        this._collapseTable = $get(this.get_collapsableTableId());

        var me = this;
        $(function () {
            me.renderExpandAllButton();
        });
    },


    get_events : function() {
        if(!this._events) 
        {
            this._events = new Sys.EventHandlerList();
        }

        return this._events;
    },

    add_onExpanded: function (handler)
    {
        // Add handler to list.
        this.get_events().addHandler('onExpanded', handler);
    },
    
    remove_onExpanded: function (handler)
    {
        // Remove handler from list.
        this.get_events().removeHandler('onExpanded', handler);
    },
                
    _raiseEvent: function (eventName, eventArgs)
    {
        // Get handler for event.
        var handler = this.get_events().getHandler(eventName);      
                                                                         
        if (handler) {                                           
            if (!eventArgs) {                                    
                eventArgs = Sys.EventArgs.Empty;                 
            }                          

            // Fire event.                          
            handler(this, eventArgs);                               
        }      
    },

    dispose: function ()
    {
        this._collapsableTableId = null;
        this._collapseImageId = null;
        this._collapseImage = null;
        this._collapseTable = null;
        this._controlID = null;
        this._clientToggle = null;
        this._defaultStateSpecified = null;

        Camstar.WebPortal.PortalFramework.ToggleContainer.callBaseMethod(this, 'dispose');
    },

    _onToggleImage: function (e)
    {
        if (e.type == "keypress" && e.charCode !== 32)
            return false;

        if (this._state == eval(Camstar.WebPortal.Personalization.CollapsableState.Collapsed))
        {
            this.expand();
        }
        else
        {
            this.collapse();
        }
    },

    collapse: function ()
    {
        if (!this._clientToggle)
        {
            this._state = eval(Camstar.WebPortal.Personalization.CollapsableState.Collapsed);
            $get(this.get_hiddenFieldId()).value = eval(Camstar.WebPortal.Personalization.CollapsableState.Collapsed);

            __page.postback(this.get_controlID(), this._state, GetAdditionalInput(this));
            __page._additionalInput = '';
        }
        else
            this.clientCollapse();
    },

    expand: function ()
    {
        if (!this._clientToggle)
        {
            this._state = eval(Camstar.WebPortal.Personalization.CollapsableState.Expanded);
            $get(this.get_hiddenFieldId()).value = eval(Camstar.WebPortal.Personalization.CollapsableState.Expanded);

            __page.postback(this.get_controlID(), this._state, GetAdditionalInput(this));
            __page._additionalInput = '';
        }
        else
            this.clientExpand();
    },

    // Toggle section visibility just in script. Used in TwoLevel start and for the dialog windows that don't support postbacks inside
    toggle: function ()
    {
        if (this._state == eval(Camstar.WebPortal.Personalization.CollapsableState.Collapsed))
            this.clientExpand();
        else
            this.clientCollapse();
    },

    clientCollapse: function()
    {
        var cont = $(this._collapseImage).parent().find('.content');
        var $img = $('>img', this._collapseImage);
        if ($img.length)
        {
            if (this._state == eval(Camstar.WebPortal.Personalization.CollapsableState.Expanded))
            {
                this._state = eval(Camstar.WebPortal.Personalization.CollapsableState.Collapsed);
                $get(this.get_hiddenFieldId()).value = eval(Camstar.WebPortal.Personalization.CollapsableState.Collapsed);
                cont.hide();
                cont.trigger("toggle", "client_collapsed");
                $img.prop('src', $img.attr('plus'));
                if (this._labels[0])
                    $img[0].nextSibling.data = this._labels[0];
            }
        }
    },

    clientExpand: function ()
    {
        var cont = $(this._collapseImage).parent().find('.content');
        var $img = $('>img', this._collapseImage);
        if ($img.length)
        {
            if (this._state == eval(Camstar.WebPortal.Personalization.CollapsableState.Collapsed))
            {
                this._state = eval(Camstar.WebPortal.Personalization.CollapsableState.Expanded);
                $get(this.get_hiddenFieldId()).value = eval(Camstar.WebPortal.Personalization.CollapsableState.Expanded);
                cont.show();
                cont.trigger("toggle", "client_expanded");
                $img.prop('src', $img.attr('minus'));
                if (this._labels[1])
                    $img[0].nextSibling.data = this._labels[1];

                this._raiseEvent('onExpanded');
            }
        }
    },

    // if there are at least 2 modeling toggle containers - add Expand All button to the 1st one.
    renderExpandAllButton: function()
    {
        var $toggleContainer = $(this._element);
        var $parentWebPart = $toggleContainer.parents('.matrix');
        if ($parentWebPart.length)
        {
            var modelingToggleControls = $('span[displayMode="modeling"]', $parentWebPart);
            // there are at least 2 modeling toggle containers. 
            if (modelingToggleControls.length > 1)
            {
                if ($toggleContainer.attr('id') == $(modelingToggleControls[0]).attr('id'))
                {
                    if (!this._defaultStateSpecified)
                        this.clientExpand(); // expand the 1st modeling section by default.

                    var $toggleHeader = $('div.header', $toggleContainer);
                    var $expandAllButton = $('span.expandAllButton', $toggleHeader);
                    if ($toggleHeader.length && $expandAllButton.length == 0) // if the button has not been added yet.
                    {
                        var expandAllTxt = this._labels[3];
                        var collapseAllTxt = this._labels[4];

                        $expandAllButton = $("<span class='expandAllButton collapsed'>" + expandAllTxt + "</span>");
                        $expandAllButton.click(function (e)
                        {
                            var isExpandAll = $(this).hasClass('collapsed');
                            $.each(modelingToggleControls, function () {
                                var toggleContainerObj = $find($(this).attr('id'));
                                if (toggleContainerObj)
                                {
                                    if (isExpandAll)
                                        toggleContainerObj.clientExpand();
                                    else
                                        toggleContainerObj.clientCollapse();
                                }
                            });
                            $(this).toggleClass('collapsed');
                            if (isExpandAll)
                                $(this).html(collapseAllTxt);
                            else
                                $(this).html(expandAllTxt);

                            e.cancelBubble = true; //IE
                            if (e.stopPropagation)
                                e.stopPropagation(); //other browsers
                        });

                        $toggleHeader.append($expandAllButton);
                    }
                }
            }
        }
    },

    get_collapsableTableId: function () { return this._collapsableTableId; },
    set_collapsableTableId: function (value) { this._collapsableTableId = value; },

    get_collapseImageId: function () { return this._collapseImageId; },
    set_collapseImageId: function (value) { this._collapseImageId = value; },

    get_hiddenFieldId: function () { return this._hiddenFieldId; },
    set_hiddenFieldId: function (value) { this._hiddenFieldId = value; },

    get_state: function () { return this._state; },
    set_state: function (value) { this._state = value; },

    get_controlID: function () { return this._controlID; },
    set_controlID: function (value) { this._controlID = value; },

    get_labels: function () { return this._labels; },
    set_labels: function (value) { this._labels = value; },

    get_clientToggle: function () { return this._clientToggle; },
    set_clientToggle: function (value) { this._clientToggle = value; },

    get_defaultStateSpecified: function () { return this._defaultStateSpecified; },
    set_defaultStateSpecified: function (value) { this._defaultStateSpecified = value; }
},

function GetAdditionalInput(element)
{
    var additionalInput = null;
    if (element.name)
    {
        if (element.tagName === 'INPUT')
        {
            var type = element.type;
            if (type === 'submit')
            {
                additionalInput = element.name + '=' + encodeURIComponent(element.value);
            }
        }
        else if ((element.tagName === 'BUTTON') && (element.name.length !== 0) && (element.type === 'submit'))
        {
            additionalInput = element.name + '=' + encodeURIComponent(element.value);
        }
    }
    return additionalInput;
},

// Optional descriptor for JSON serialization.
Camstar.WebPortal.PortalFramework.ToggleContainer.descriptor =
{
    properties:
    [
        { name: 'collapsableTableId', type: String },
        { name: 'collapseImageId', type: String },
        { name: 'hiddenFieldId', type: String },
        { name: 'state', type: Camstar.WebPortal.Personalization.CollapsableState },
        { name: 'controlID', type: String },
        { name: 'labels', type: Array }

    ]
},

Camstar.WebPortal.PortalFramework.ToggleContainer.registerClass('Camstar.WebPortal.PortalFramework.ToggleContainer', Camstar.UI.Control);

if (typeof(Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
