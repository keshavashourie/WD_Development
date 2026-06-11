// Copyright Siemens 2023 

/// <reference path="../MicrosoftAjaxExt.js"/>
/// <reference path="../Camstar.UI/Control.js" />

Type.registerNamespace("Camstar.WebPortal.FormsFramework.WebControls");
Type.registerNamespace("Camstar.WebPortal.Personalization");

Camstar.WebPortal.FormsFramework.WebControls.Breadcrumb = function (element) {
    Camstar.WebPortal.FormsFramework.WebControls.Breadcrumb.initializeBase(this, [element]);

    this._commands = [];
    this._control = null;
    this._serverType = null;
    this._storageKey = "BreadcrumbCommands";
    this._storage = window.sessionStorage;
    this._storageIsCleared = false;
};

Camstar.WebPortal.FormsFramework.WebControls.Breadcrumb.prototype = {
    initialize: function () {
        Camstar.WebPortal.FormsFramework.WebControls.Breadcrumb.callBaseMethod(this, 'initialize');
        var me = this;

        var commands = JSON.parse(this._storage.getItem(this._storageKey));
        if (commands) {
            this._commands = this._deserializeData(commands);
        }
        me._buildControl();

        $(window).resize(function () {
            var $element = $(me.get_element());
            var $ul = $element.find("ul");

            me._closeCompactMenu();

            var total_visible_width = 0;
            $ul.find("li:visible").each(function () { total_visible_width += $(this).outerWidth(true) });

            var $li_separator = $($ul.find("li.separator:hidden")[$ul.find("li.separator:hidden").length - 1]);
            var $li_item = $li_separator.next();
            var last_hidden_item_width = $li_separator.outerWidth(true) + $li_item.outerWidth(true);

            var ul_width = $ul.width();

            if (Math.round(ul_width) < Math.round(total_visible_width)) {
                me._hideBreadcrumbItem();
            }
            else if (Math.round(ul_width) > Math.round(total_visible_width + last_hidden_item_width)) {
                $li_separator.show();
                $li_item.show();
                $li_item.next().removeClass("compact-mode");
                $li_item.next().off('click');

                if ($ul.find("li.separator:hidden").length > 0)
                    $li_separator.addClass("compact-mode");
            }
        });
    },

    _buildControl: function () {

        var $element = $(this.get_element());
        $element.empty();

        $element.append(this._createList());
    },

    _createList: function () {
        var me = this;
        var list = document.createElement('ul');
        $(this._commands).each(function (index) {
            var command = this;

            if (index !== 0) {
                var separator = document.createElement('li');
                $(separator).addClass('separator');
                $(list).append(separator);
            }

            var item = document.createElement('li');
            $(item).html(this.title);
            $(item).attr('tabindex', '0');      // Add tabindex to make it focusable
            $(item).click(function () {
                var args = {};
                me.executeCommand(command, args, me);
            });
            $(list).append(item);
        });

        return list;
    },

    _hideBreadcrumbItem: function () {
        var $element = $(this.get_element());
        var $ul = $element.find("ul");

        var $li_separator = $($ul.find("li.separator:visible")[0]);
        var $li_item = $li_separator.next();
        var $li_next_separator = $li_item.next();

        $li_separator.hide();
        $li_item.hide();

        $li_separator.removeClass("compact-mode");
        $li_next_separator.addClass("compact-mode");

        var me = this;
        $li_item.next().on('click', function () { me._buildCompactMenu(); });
    },

    _buildCompactMenu: function () {
        var me = this;
        var $element = $(this.get_element());
        var $ul = $element.find("ul");
        var list = document.createElement('ul');
        $(list).addClass("cs-breadcrumb-compact-menu");

        $ul.find("li:hidden:nth-child(odd)").each(function () {
            var item = document.createElement('li');
            $(item).text($(this).text());
            $(item).click(function () {
                me._closeCompactMenu();

                var index = $(this).parent().find("li").index(this);
                var args = {};
                me.executeCommand(me._commands[index + 1], args, me);

            });
            list.appendChild(item);
        });

        $("body").prepend(list);
        $(list).position({
            my: "left top+10",
            at: "left bottom",
            of: $ul.find("li.separator.compact-mode"),
            collision: "fit"
        })
    },

    _arrayObjectIndexOf: function (myArray, searchTerm, property) {
        for (var i = 0, len = myArray.length; i < len; i++) {
            if (myArray[i][property] === searchTerm) return i;
        }
        return -1;
    },

    _showBreadcrumbItem: function () {
        var $element = $(this.get_element());
        var $ul = $element.find("ul");

        var $li_separator = $($ul.find("li.separator:hidden")[$ul.find("li.separator:hidden").length - 1]);
        var $li_item = $li_separator.next();

        $li_separator.show();
        $li_item.show();
    },

    _closeCompactMenu: function () {
        $("ul.cs-breadcrumb-compact-menu").remove();
    },

    _serializeData: function (commands) {
        var json = JSON.stringify(commands, function (key, value) {

            if (typeof value === 'function') {
                return value.toString();
            } else {
                return value;
            }
        });
        return json;
    },

    _deserializeData: function (json) {
        if (json) {
            json.forEach(function (command) {
                if (command.execute) {
                    command.execute = eval('(' + command.execute + ')');
                }
            });
        };
        return json;
    },

    clearStorage: function () {
        this._storage.removeItem(this._storageKey);
    },

    dispose: function () {
        Camstar.WebPortal.FormsFramework.WebControls.Breadcrumb.callBaseMethod(this, 'dispose');
    },

    executeCommand: function (command, commandArgs, context) {

        var index = this._arrayObjectIndexOf(this._commands, command.id, 'id');

        if (index < 0) { index = 0; }

        this._commands.length = index;
        this._storage.setItem(this._storageKey, this._serializeData(this._commands));
        command.execute(commandArgs, context, command.value);
    },


    addCommand: function (command) {
        command.id = this._commands.length;
        this._commands.push(command);
        this._storage.setItem(this._storageKey, this._serializeData(this._commands));
        this._buildControl();
    },
    removeCommand: function (command) {
        this._commands.pop(command);
        this._storage.setItem(this._storageKey, this._serializeData(this._commands));
        this._buildControl();
    }
};

var Command = function (title, execute, value) {
    this.id = 0;
    this.title = title;
    this.execute = execute;
    this.value = value;
};

var OpenPageCommand = function (title, page) {
    return new Command(title,
        function (commandArgs) {
            __page.openInNewTab(this.value, commandArgs, name, null, null);

        }, page);
};

var JavascriptCommand = function (title, func, value) {
    return new Command(title, func, value);
};


Camstar.WebPortal.FormsFramework.WebControls.Breadcrumb.registerClass('Camstar.WebPortal.FormsFramework.WebControls.Breadcrumb', Camstar.UI.Control);

if (typeof (Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
