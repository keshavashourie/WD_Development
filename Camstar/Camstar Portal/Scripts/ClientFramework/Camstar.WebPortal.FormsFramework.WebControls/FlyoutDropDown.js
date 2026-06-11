// Copyright Siemens 2023  

/// <reference name="MicrosoftAjax.js"/>
/// <reference path="../Camstar.UI/Control.js" />
Type.registerNamespace("Camstar.WebPortal.FormsFramework.WebControls");

Camstar.WebPortal.FormsFramework.WebControls.FlyoutDropDown = function (element)
{
    Camstar.WebPortal.FormsFramework.WebControls.FlyoutDropDown.initializeBase(this, [element]);

    this._panel = null;
    this._button = null;
    this._width = 300;
    this._height = 400;   
    this._src = null;
}

Camstar.WebPortal.FormsFramework.WebControls.FlyoutDropDown.prototype =
{
    initialize: function ()
    {
        Camstar.WebPortal.FormsFramework.WebControls.FlyoutDropDown.callBaseMethod(this, 'initialize');
        var inputElement = this.get_element();

        var tempControl = inputElement;
        var top = inputElement.offsetHeight + $(inputElement).parent().offset().top + 40;

        var left = 0;
        if (inputElement.clientLeft)
            left = inputElement.clientLeft;

        while (tempControl != null) {
            left += tempControl.offsetLeft;
            // scrollLeft is initialized in IE when textbox contains long value, so PickList appeares too far on the left.
            if (tempControl.tagName.toLowerCase() != 'input')
                left -= tempControl.scrollLeft;
            tempControl = tempControl.offsetParent;
        }
        this._panel = inputElement.nextSibling;
        var leftPosition = left - this._width + inputElement.offsetWidth - 10;
        if (leftPosition < 10)
            leftPosition = 10;
        this._panel.style.left = leftPosition + "px";
        this._panel.style.top = top + "px";
        this._button = this._panel.childNodes[2];
        this._button.setAttribute('onclick', 'return false;');
        inputElement.setAttribute('onclick', 'return false;');
        $clearHandlers(inputElement);
        $addHandlers(inputElement,
                {
                    'click': this.onClickButton
                }, this);
        $clearHandlers(this._button);
        $addHandlers(this._button,
                {
                    'click': this.onClickButton
                }, this);
        $(".ui-flyout-panel > iframe").attr('src', 'Blank.htm'); 
        if (this._extensions != null)
            eval(this._extensions);
    },

    onClickButton: function ()
    {
        if(this._panel.style.display == 'none')
            this.open();
        else
            this.close();
        return false;
    },
    
    open: function ()
    {
       $(".ui-flyout-panel > iframe").attr('src', this._src);

        this._panel.style.display = '';
    },
    
    close: function ()
    {
        this._panel.style.display = 'none';
        
        $(".ui-flyout-panel > iframe").attr('src', 'Blank.htm');
    },
    
    dispose: function ()
    {
        Camstar.WebPortal.FormsFramework.WebControls.FlyoutDropDown.callBaseMethod(this, 'dispose');
    },
    
    get_width: function () { return this._width; },
    set_width: function (value) { this._width = value; },
    
    get_height: function () { return this._height; },
    set_height: function (value) { this._height = value; },
    
    get_extensions: function () { return this._extensions; },
    set_extensions: function (value) { this._extensions = value; },
    
    get_Src: function () { return this._src; },
    set_Src: function (value) { this._src = value; }
}

Camstar.WebPortal.FormsFramework.WebControls.FlyoutDropDown.registerClass('Camstar.WebPortal.FormsFramework.WebControls.FlyoutDropDown', Camstar.UI.Control);

if (typeof (Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();

