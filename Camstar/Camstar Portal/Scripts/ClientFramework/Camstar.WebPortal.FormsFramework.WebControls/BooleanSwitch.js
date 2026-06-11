// Copyright Siemens 2023  

/// <reference path="../MicrosoftAjaxExt.js"/>
/// <reference path="../Camstar.UI/Control.js" />
/// <reference path="../../jquery/jquery.min.js" />
/// <reference path="../../jquery/jquery-ui.min.js" />

Type.registerNamespace("Camstar.WebPortal.FormsFramework.WebControls");
Type.registerNamespace("Camstar.WebPortal.Personalization");

Camstar.WebPortal.Personalization.BooleanSwitchDisplayModeType = function () { };

Camstar.WebPortal.Personalization.BooleanSwitchDisplayModeType.prototype =
    {
        RadioButton: 0,
        Slide: 1
    };

Camstar.WebPortal.Personalization.BooleanSwitchDisplayModeType.registerEnum("Camstar.WebPortal.Personalization.BooleanSwitchDisplayModeType", false);

Camstar.WebPortal.FormsFramework.WebControls.BooleanSwitch = function (element) {
    Camstar.WebPortal.FormsFramework.WebControls.BooleanSwitch.initializeBase(this, [element]);

    this._wrapper = null;
    this._radioList = null;
    this._state = null;
    this._needPostBack = null;
    this._readOnly = null;
    this._defaultValue = null;
    this._displayMode = Camstar.WebPortal.Personalization.BooleanSwitchDisplayModeType.RadioButton;
    this._toggleClickCallback = null; // allow for client side hook into click handler
};

Camstar.WebPortal.FormsFramework.WebControls.BooleanSwitch.prototype =
    {
        initialize: function () {
            Camstar.WebPortal.FormsFramework.WebControls.BooleanSwitch.callBaseMethod(this, 'initialize');

            if (this._displayMode == Camstar.WebPortal.Personalization.BooleanSwitchDisplayModeType.Slide) {
                var wrapper = this.get_wrapper();
                if (wrapper) {
                    if (this._readOnly && !$(wrapper).hasClass('disabled'))
                        $(wrapper).addClass('disabled');

                    var radioButtonList = $('input[type="radio"]', wrapper);
                    if (radioButtonList.length > 1) {
                        this._toggleDelegate = Function.createDelegate(this, this._onToggle);
                        $clearHandlers(wrapper);
                        $addHandlers(wrapper,
                            {
                                'click': this._toggleDelegate
                            }, this);

                        this._setInitialState();
                    }
                }
            }
        },

        _onToggle: function () {
            if (this._readOnly)
                return;

            if (this._displayMode == Camstar.WebPortal.Personalization.BooleanSwitchDisplayModeType.Slide) {
                var $wrapper = $(this.get_wrapper());
                var $element = $(this._element);

                if ($wrapper) {
                    var radioButtonList = $('input[type="radio"]', $wrapper);
                    if (radioButtonList.length > 1) {
                        var yesRadio = $(radioButtonList[0]);
                        var noRadio = $(radioButtonList[1]);

                        if (yesRadio.is(':checked')) {
                            noRadio.prop("checked", true);
                            yesRadio.prop("checked", false);
                        }
                        else {
                            yesRadio.prop("checked", true);
                            noRadio.prop("checked", false);
                        }

                        this._state = yesRadio.is(':checked');
                        $wrapper.toggleClass('switch-off switch-on');
                        $element.toggleClass('switch-off switch-on');

                        $(this).trigger('stateChanged', [this._state]);

                        if (this._needPostBack) {
                            if (this._state)
                                __page.postback(yesRadio.attr('id'), '');
                            else
                                __page.postback(noRadio.attr('id'), '');
                        }
                    }
                }
            }
            if (this._toggleClickCallback)
                this._toggleClickCallback();
        },

        _setInitialState: function () {
            var $wrapper = $(this.get_wrapper());
            var $element = $(this._element);

            if ($wrapper) {
                var radioButtonList = $('input[type="radio"]', $wrapper);
                if (radioButtonList.length > 1) {
                    var yesRadio = $(radioButtonList[0]);
                    this._state = yesRadio.is(':checked');
                    if (yesRadio.is(':checked')) {
                        if (!$wrapper.hasClass('switch-on')) {
                            $wrapper.removeClass('switch-off').addClass('switch-on');
                            $element.removeClass('switch-off').addClass('switch-on');
                        }
                    }
                    else {
                        if (!$wrapper.hasClass('switch-off')) {
                            $wrapper.removeClass('switch-on').addClass('switch-off');
                            $element.removeClass('switch-on').addClass('switch-off');
                        }
                    }
                }
            }
        },

        setState: function (value) {
            if (this._displayMode == Camstar.WebPortal.Personalization.BooleanSwitchDisplayModeType.Slide) {
                if (this._state != value)
                    this._onToggle();
            }
        },
        getState: function () { return this._state; },

        dispose: function () {
            this._wrapper = null;
            this._radioList = null;
            this._state = null;
            this._displayMode = null;
            this._readOnly = null;
            Camstar.WebPortal.FormsFramework.WebControls.BooleanSwitch.callBaseMethod(this, 'dispose');
        },

        directUpdate: function (value) {
            Camstar.WebPortal.FormsFramework.WebControls.BooleanSwitch.callBaseMethod(this, 'directUpdate');

            if (value.PropertyKey == eval(Camstar.Ajax.DirectUpdateParameterKeys.Data)) {
                this.setState(value.PropertyValue);
            }
        },

        get_displayMode: function () { return this._displayMode; },
        set_displayMode: function (value) { this._displayMode = value; },

        get_needPostBack: function () { return this._needPostBack; },
        set_needPostBack: function (value) { this._needPostBack = value; },

        get_readOnly: function () { return this._readOnly; },
        set_readOnly: function (value) { this._readOnly = value; },

        get_wrapper: function () { return this._wrapper; },
        set_wrapper: function (value) { this._wrapper = value; },

        get_radioList: function () { return this._radioList; },
        set_radioList: function (value) { this._radioList = value; },

        get_defaultValue: function () { return this._defaultValue; },
        set_defaultValue: function (value) { this._defaultValue = value; }
    };

Camstar.WebPortal.FormsFramework.WebControls.BooleanSwitch.registerClass('Camstar.WebPortal.FormsFramework.WebControls.BooleanSwitch', Camstar.UI.Control);

if (typeof (Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
