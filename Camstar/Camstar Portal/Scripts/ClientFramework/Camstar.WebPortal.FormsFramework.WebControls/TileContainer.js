// Copyright Siemens 2023  

Type.registerNamespace("Camstar.WebPortal.FormsFramework.WebControls");

Camstar.WebPortal.FormsFramework.WebControls.TileContainer = function (element) {
    Camstar.WebPortal.FormsFramework.WebControls.TileContainer.initializeBase(this, [element]);

    this._divTooltip = null;
    this._hiddenStateControl = null;
    this._state = null;
    this._controlId = null;
    this._columnTileCustomStyle = {};
}

Camstar.WebPortal.FormsFramework.WebControls.TileContainer.prototype = {
    
    initialize: function () {
        Camstar.WebPortal.FormsFramework.WebControls.TileContainer.callBaseMethod(this, 'initialize');

        this._state = JSON.parse(this._hiddenStateControl.value);
        this.renderControl();
    },

    renderState: function(newState, tilesOnly) {
        this._state = newState;
        this._hiddenStateControl.value = JSON.stringify(newState);
        if (!tilesOnly)
            this.renderControl();
        else
            this.renderTiles(this._state);
    },

    getValue: function () {
        return this._state;
    },

    setValue: function (newState) {
        // keep old columns and refresh tiles.
        if (newState && !newState.hasOwnProperty("Columns")) {
            this._state.Tiles = newState.Tiles;
            this.renderState(this._state, true);
        }
        else
            this.renderState(newState);
    },

    clearValue: function () {
        
    },

    click: function(e) {
        if (e.target.className === "icon") {
            var tileDiv = $(e.target).closest(".item")[0];
            var tileColumn = tileDiv.parentNode;

            var args = {
                CustomData: tileDiv.getAttribute("data-custom"),
                Index: tileDiv.getAttribute("data-ind"),
                ColumnName: tileColumn.getAttribute("data-name")
            };
            __page.postback(this._controlId, JSON.stringify(args));
        }
    },

    renderControl: function () {
        this.renderColumns(this._state);
        this.renderTiles(this._state);
    },

    renderColumns: function(state) {
        this._element.innerHTML = "";
        if (state && state.Columns) {
            if (Array.isArray(state.Columns)) {
                for (var i = 0; i < state.Columns.length; i++) {
                    var col = state.Columns[i];
                    if (col.Visible) {
                        var node = document.createElement("div");
                        node.className = "tileColumn";
                        node.setAttribute("data-name", col.Name);
                        node.setAttribute("data-ind", i);

                        if (col.CssClass)
                            node.classList.add(col.CssClass);
                        if (col.ColumnStyle)
                            node.style.cssText = col.ColumnStyle;
                        if (col.TileStyle)
                            this._columnTileCustomStyle[col.Name] = col.TileStyle;

                        var title = document.createElement("span");
                        title.className = "title-label";
                        title.innerHTML = col.Title;

                        node.appendChild(title);
                        this._element.appendChild(node);
                    }
                }
            }
            else
                console.error("Invalid Columns value: array is expected.");
        }
    },

    renderTiles: function(state) {
        // remove previous tiles.
        $(".item", this._element).remove();

        if (state && state.Columns && state.Tiles) {
            if (Array.isArray(state.Tiles)) {
                for (var i = 0; i < state.Tiles.length; i++) {
                    // Pass in DefaultImage defined in Portal Page
                    this.addTile(state.Tiles[i], state);
                }
            }
            else
                console.error("Invalid Tiles value: array is expected.");
        }
    },

    addTile: function(tileObj, state) {
        if (tileObj.ColumnName) {
            var colDiv = this._element.querySelector('div[data-name="' + tileObj.ColumnName + '"]');
            if (colDiv) {
                var tileDiv = document.createElement("div");
                tileDiv.className = "item";
                var itemStyle = this._columnTileCustomStyle[tileObj.ColumnName];
                var tileStyle = "";
                if (itemStyle)
                    tileStyle = itemStyle;
                if (tileObj.TileStyle) {
                    if (tileStyle && tileStyle.slice(-1) !== ';')
                        tileStyle += ";";
                    tileStyle += tileObj.TileStyle;
                }

                if (tileStyle)
                    tileDiv.style.cssText = tileStyle;

                tileDiv.setAttribute("data-ind", colDiv.querySelectorAll(".item").length);
                if (tileObj.CustomData)
                    tileDiv.setAttribute("data-custom", tileObj.CustomData);

                var body = document.createElement("div");
                body.className = "textArea";

                var titleArea = document.createElement("div");
                titleArea.className = "titleArea";

                var iconLeft = document.createElement("span");
                var image = tileObj.Image;
                if (!image) {
                    var column = null;
                    state.Columns.forEach(function (f) {
                        if (f.Name === tileObj.ColumnName) {
                            column = f;
                            return true;
                        }
                    });
                    if (column && column.DefaultImage)
                        image = column.DefaultImage;
                    else
                        image = state.DefaultImage;
                }
                if (image) {
                    var site = window.location.pathname.split("/");
                    if (site.length > 1)
                        iconLeft.style.backgroundImage = "url(/" + site[1] + "/assets/image/" + image + ")";
                }
                iconLeft.className = "icon";

                var bodyTitle = document.createElement("span");
                bodyTitle.className = "title";
                bodyTitle.innerText = tileObj.Title;
                titleArea.appendChild(iconLeft);
                titleArea.appendChild(bodyTitle);

                body.appendChild(titleArea);

                if (tileObj.Text) {
                    if (Array.isArray(tileObj.Text)) {
                        for (var i = 0; i < tileObj.Text.length; i++) {
                            var textSpan = document.createElement("span");
                            textSpan.innerText = tileObj.Text[i];
                            body.appendChild(textSpan);
                        }
                    }
                    else
                        console.error("Invalid Text value: array of strings is expected.");
                }

                var iconRight = document.createElement("span");
                iconRight.className = "icon-last";

                tileDiv.appendChild(body);
                tileDiv.appendChild(iconRight);
                colDiv.appendChild(tileDiv);
            }
        }
    },

    directUpdate: function(value) {
        Camstar.WebPortal.FormsFramework.WebControls.TileContainer.callBaseMethod(this, 'directUpdate');

        if (value.PropertyKey == eval(Camstar.Ajax.DirectUpdateParameterKeys.Data)) {
            var data = value.PropertyValue;
            this.setValue(data);
        }
    },

    dispose: function () {
        if (this._element)
            this._element.removeEventListener("click", this.click);
        this._divTooltip = null;
        this._valueControl = null;
        this._hiddenStateControl = null;
        this._state = null;
        this._controlId = null;
        this._columnTileCustomStyle = null;

        Camstar.WebPortal.FormsFramework.WebControls.TileContainer.callBaseMethod(this, 'dispose');
    },

    get_hiddenStateControl: function () { return this._hiddenStateControl; },
    set_hiddenStateControl: function (value) { this._hiddenStateControl = value; },

    get_controlId: function () { return this._controlId; },
    set_controlId: function (value) { this._controlId = value; }
    
}

Camstar.WebPortal.FormsFramework.WebControls.TileContainer.registerClass('Camstar.WebPortal.FormsFramework.WebControls.TileContainer', Camstar.UI.Control);

if (typeof (Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
