// Copyright 2020 Siemens AG
/**
 * Provides a set of utility methods not currently available in Siemens Web Framework.
 * @module "js/mom.utils.service"
 * @requires app
 * @requires js/eventBus
 * @requires js/logger
 * @requires lodash
 * @requires jquery
 * @requires js/uwPropertyService
 * @requires js/appCtxService
 * @requires "js/mom.breadcrumb.service"
 * @requires js/iconService
 */
/* eslint-disable valid-jsdoc */
/*global
 */
import app from "app";
import eventBus from "js/eventBus";
import logger from "js/logger";
import _ from "lodash";
import $ from "jquery";
import declUtils from "js/declUtils";
import noty from "js/jquery.noty.customized";
import _breadcrumb from "js/mom.breadcrumb.service";
import _uwPropertySvc from "js/uwPropertyService";
import _appCtxSvc from "js/appCtxService";
import _iconSvc from "js/iconService";
import _actionSvc from "js/actionService";
import _cfgSvc from "js/configurationService";
import _leavePlaceSvc from "js/leavePlace.service";
import _messagingSvc from "js/messagingService";
import awStateSvc from 'js/awStateService';
import awHttpSvc from 'js/awHttpService';
import awWindowSvc from 'js/awWindowService';
import awPromiseSvc from 'js/awPromiseService';
import momAnchorScrollSvc from 'js/mom.anchorScroll.service';
import awRootScopeSvc from 'js/awRootScopeService';
import awSceSvc from 'js/awSceService';

const exports = {};
const locationTitlesCtx = "location.titles";
let leaveConfirmationMessageDisplayed = false;

eventBus.subscribe("mom.commands.update", function (cfg) {
    exports.updateCommands(cfg);
});

/**
 * Closes all currently-displayed messages.
 */
exports.closeMessages = function() {
    noty.close();
};

exports._toggleEditing = function( enableEditing, object, editMap ) {
    if( object && editMap ) {
        Object.keys( editMap ).forEach( function( key ) {
            if( typeof _.get( object, key ) === "object" ) {
                const prop = object[ key ];
                prop.isEditable = enableEditing && Boolean( editMap[ key ] );
                if( !enableEditing ) {
                    // Reset old value
                    prop.dirty = false;
                    prop.dbValue = prop.value;
                    prop.dbValues = prop.value;
                    prop.displayValues = prop.prevDisplayValues || [ String( prop.value ) ];
                    prop.displayValueUpdated = false;
                    prop.uiValue = prop.prevDisplayValues && prop.prevDisplayValues[0] || String( prop.value );
                    prop.uiValues = prop.prevDisplayValues || [ String( prop.value ) ];
                    prop.valueUpdated = false;
                }
            }
        } );
    }
    _appCtxSvc.updateCtx( "editInProgress", enableEditing );
    return { object: object };
};

/**
 * Retrieves the full path to the specified type icon, i.e. "<assets\>/image/type<name\>48.svg".
 * @param {String} name The name of a valid type icon.
 */
exports.typeIconPath = function( name ) {
    return app.getBaseUrlPath() + "/image/type" + name + "48.svg";
};

/**
 * Retrieves the full path to the specified command icon, i.e. "<assets\>/image/cmd<name\>24.svg".
 * @param {String} name The name of a valid command icon.
 */
exports.cmdIconPath = function( name ) {
    return app.getBaseUrlPath() + "/image/cmd" + name + "24.svg";
};

/**
 * Retrieves the full SVG code of the specified indicator icon (located in <assets\>/image/indicator<name\>.svg). Useful to configure cell indicators.
 * @param {String} name The name of the indicator icon to load.
 */
exports.indicatorIcon = function( name ) {
    return awSceSvc.instance.trustAsHtml( _iconSvc.getIndicatorIcon( name ) );
};

/**
 * Returns the full path to an image file in the <assets\>/image/ folder.
 * @param {String} path The partial path (i.e. within the <assets\>/image/ folder) to the image.
 */
exports.imagePath = function( path ) {
    return app.getBaseUrlPath() + "/image/" + path;
};

/**
 * An object used to configure a message to prompt the user whether to navigate away from the current screen or not.
 * @typedef {Object} NavigationPromptConfig
 * @property {Boolean} enabled Whether the message is enabled or not (default: false).
 * @property {String} message The message to display when navigating away from the screen being edited (default: 'Do you want to navigate away from this page? Any unsaved changes will be lost.').
 * @property {String} deny The text to display in the button to cancel navigation (default: Cancel). If explicitly set to **null**, the button will be hidden.
 * @property {String} accept The text to display in the button to confirm the navigation (default: OK). If explicitly set to **null**, the button will be hidden.
 */
/**
 * Marks the properties of the specified object as editable, and sets the **editInProgress** context to **true**.
 * > **Tip** This method is also exposed as the **MomStartEditing** custom action. For more information on how to use it, see the [In-place Editing](https://gitlab.industrysoftware.automation.siemens.com/mom/mom-ui/wikis/in-place-editing) page on the MOM UI Wiki.
 * @param {Object} object The object to edit.
 * @param {Object.<String, Boolean>} editMap A dictionary containing the properties to be edited.
 * @param {Object} options A dictionary that can contain the following options:
 * * **navigationPrompt** &mdash; A [NavigationPromptConfig](#~NavigationPromptConfig) object.
 */
exports.startEditing = function( object, editMap, options ) {
    if(
        options &&
        options.navigationPrompt &&
        options.navigationPrompt.enabled
    ) {
        exports._registerLeaveHandler( options.navigationPrompt );
    }
    return exports._toggleEditing( true, object, editMap );
};

/**
 * Marks the properties of the specified object as non-editable, and sets the **editInProgress** context to **false**.
 * > **Tip** This method is also exposed as the **MomCancelEditing** custom action. For more information on how to use it, see the [In-place Editing](https://gitlab.industrysoftware.automation.siemens.com/mom/mom-ui/wikis/in-place-editing) page on the MOM UI Wiki.
 * @param {Object} object An editable object.
 * @param {Object.<String, Boolean>} editMap A dictionary containing the properties not to be edited.
 */
exports.cancelEditing = function( object, editMap ) {
    exports._unregisterLeaveHandler();
    return exports._toggleEditing( false, object, editMap );
};

exports._registerLeaveHandler = function( cfg ) {
    let currentSelection = _appCtxSvc.getCtx( "selected" );
    _leavePlaceSvc.registerLeaveHandler( {
        okToLeave: function() {
            eventBus.publish( "hosting.changeSelection", {
                selected: currentSelection.uid
            } );
            let deferred = awPromiseSvc.instance.defer();
            let deny = {
                addClass: "btn btn-notify",
                text: ( cfg && cfg.deny ) || "Cancel",
                onClick: function( noty ) {
                    leaveConfirmationMessageDisplayed = false;
                    noty.close();
                    deferred.reject();
                }
            };
            let accept = {
                addClass: "btn btn-notify",
                text: ( cfg && cfg.accept ) || "OK",
                onClick: function( noty ) {
                    leaveConfirmationMessageDisplayed = false;
                    noty.close();
                    deferred.resolve();
                }
            };
            let buttons = [];
            if( !cfg || ( cfg && cfg.deny !== null ) ) {
                buttons.push( deny );
            }
            if( !cfg || ( cfg && cfg.accept !== null ) ) {
                buttons.push( accept );
            }
            let message =
                ( cfg && cfg.message ) ||
                "Do you want to navigate away from this page? Any unsaved changes will be lost.";
            if( !leaveConfirmationMessageDisplayed ) {
                leaveConfirmationMessageDisplayed = true;
                _messagingSvc.showWarning( message, buttons );
            }
            return deferred.promise;
        }
    } );
};

exports._unregisterLeaveHandler = function() {
    _leavePlaceSvc.registerLeaveHandler( null );
};

/**
 * An object used to specify an editable/non-editable property via the [startTableEdit](#.startTableEdit) method.
 * @typedef {Object} EditingConfig
 * @property {String} propertyName The name of the property to mark as editable/non-editable.
 * @property {Boolean} isPropertyModifiable Specifies whether the property is modifiable (set to **false** to mark it as non-editable).
 * @property {Boolean} editable Specifies whether the property is editable (set to **false** to mark it as non-editable).
 */
/**
 * Specifies which property of each object in **data** must be used as a unique identifier, and which properties to set as non-editable (all the other properties will be marked editable).
 * > **Note** You can use this method to edit data in a table, as described in [Siemens Web Framework Tutorial #9](https://gitlab.industrysoftware.automation.siemens.com/Apollo/samples/Tutorial-9-Demonstrate_Editing_capabilities_for_aw-Table/).
 * > **Tip** This method is also exposed as the **MomStartTableEditing** custom action.
 * @param {Array.<Object>} data An array of objects to be edited.
 * @param {String} uid  The name of the property to use as unique identifier.
 * @param {Object.<String, EditingConfig>} props A dictionary of [EditingConfig](#~EditingConfig) objects, indicating which properties are not editable.
 */
exports.startTableEditing = function( data, uid, props ) {
    return data.map( function( item ) {
        return {
            uid: item[ uid ],
            props: _.cloneDeep( props )
        };
    } );
};

/**
 *
 * @param {UwDataProvider} dataProvider A reference to the dataProvder managing the items to select/deselect.
 * @param {String[]} ids An array of identifiers of the items to select/deselect.
 * @param {Object} [options={identifier: 'uid'}] An object exposing an **identifier** property, used to indicate which property of the element will be used to identify the element univocaly.
 */
exports.select = function( dataProvider, ids, options ) {
    if( !dataProvider ) {
        return;
    }
    let opts = Object.assign( { identifier: "uid" }, options );
    let objects = dataProvider.viewModelCollection.loadedVMObjects.filter(
        function( item ) {
            return ids.includes( _.get( item, opts.identifier ) );
        }
    );
    dataProvider.selectionModel.setSelection( objects );
};

/**
 * An object used to define a new watcher through the [watch](#.watch) method.
 * @typedef {Object} WatcherConfig
 * @property {DeclViewModel} vm A reference to the viewModel on whose scope the expression will be evaluated.
 * If specified, the watcher will be deregistered automatically when the specified viewModel is destroyed.
 * @property {Object} env _(Ignored if **vm** is specified)_ An object containing properties that will be available in the execution context of the specified expression.
 * @property {String} name The name of the expression. This value will be used in the event name: `mom.<name>.onValueChanged`.
 * @property {String} expression The expression to evaluate (without surrounding curly brackets).
 * @property {String} deregisterOn _(Ignored if **vm** is specified)_ The name of an event that will de-register the watcher.
 * @property {Boolean} [collection=false] Set this to **true** if the result of the expression is an object or array.
 */
/**
 * Creates a watcher that will trigger an event when the value of the specified expression changes.
 * > **Tip:** For more information on how to use this method, see the [Detecting value changes](https://gitlab.industrysoftware.automation.siemens.com/mom/mom-ui/wikis/detecting-value-changes) on the MOM UI Wiki.
 * @param {module:"js/mom.utils.service"~WatcherConfig} cfg A configuration object used to configure the watcher.
 */
exports.watch = function( cfg ) {
    if(
        !(
            cfg &&
            cfg.name &&
            cfg.expression &&
            ( cfg.vm || ( cfg.env && cfg.deregisterOn ) )
        )
    ) {
        logger.error(
            "MOM UI - watch: Specify a valid watcher configuration object with name, expression, vm (or env, and deregisterOn) properties."
        );
        return;
    }
    let scope;
    if( cfg.env ) {
        scope = awRootScopeSvc.instance.$new( true );
        Object.keys( cfg.env ).forEach( function( key ) {
            scope[ key ] = cfg.env[ key ];
        } );
    } else {
        if( cfg.vm && cfg.vm._internal && cfg.vm._internal.origCtxNode ) {
            scope = cfg.vm._internal.origCtxNode;
        } else {
            logger.error(
                "MOM UI - watch: vm property contains an invalid viewModel reference."
            );
            return;
        }
    }
    let deregister;
    if( cfg.collection ) {
        deregister = scope.$watchCollection( cfg.expression, function(
            newVal,
            oldVal
        ) {
            eventBus.publish( "mom." + cfg.name + ".onValueChanged", {
                newVal: newVal,
                oldVal: oldVal
            } );
        } );
    } else {
        deregister = scope.$watch( cfg.expression, function( newVal, oldVal ) {
            eventBus.publish( "mom." + cfg.name + ".onValueChanged", {
                newVal: newVal,
                oldVal: oldVal
            } );
        } );
    }
    if( cfg.deregisterOn ) {
        eventBus.subscribe( cfg.deregisterOn, function() {
            deregister();
            scope.$destroy();
            logger.debug(
                "MOM UI - De-registered watcher (additional scope): ",
                cfg.name
            );
        } );
    }
    if( cfg.env ) {
        logger.debug(
            "MOM UI - Registered watcher (additional scope): ",
            cfg.name
        );
    } else if( cfg.vm ) {
        logger.debug(
            "MOM UI - Registered watcher (viewModel: " +
            cfg.vm._internal.panelId +
            "): ",
            cfg.name
        );
    }
};

/**
 * Updates the list of currently-loaded items managed by a dataProvider with the specified collection.
 * > **Tip:** This method is also exposed as the **MomUpdateDataProvider** custom action.
 * @param {UwDataProvider} dataProvider A reference to the dataProvder.
 * @param {Object[]} collection The new collection of items to be passed to the dataProvider.
 * @param {Number} total The total number of items (not only the ones that have been loaded) managed by the dataProvider.
 * @returns {Object} An object containing the collection and the total values passed as input.
 */
exports.updateDataProvider = function( dataProvider, collection, total ) {
    dataProvider.update( collection, total );
    return { collection: collection, total: total };
};

/**
 * Returns the specified input data as output. This is useful in some cases to trigger dataParser/output mapping in viewModels, or as an
 * easy way to execute an expression on some data.
 * > **Tip:** This method is also exposed as the **MomGetInputData** custom action.
 * @async
 * @param {*} input The input data to return (it should be an object to be able to retrieve its properties in action **outputData**, see the example).
 * @returns {Promise<*>}
 * @example
 *  "setValues": {
 *      "actionType": "MomGetInputData",
 *      "inputData": {
 *          "input": {
 *              "total": "{{ctx.notesByState[ctx.state.params.status] || ctx.notesByState.total}}",
 *              "status": "{{ctx.state.params.status}}"
 *          }
 *      },
 *      "outputData": {
 *          "total.uiValue": "total",
 *          "status.uiValue": "status"
 *      }
 *  }
 */
exports.getInputData = function( input ) {
    return exports.promisify( input );
};

/**
 * An object representing the configuration of an HTTP request.
 * @typedef {Object} HttpRequest
 * @property {String} method The HTTP method (e.g. 'GET', 'POST', etc).
 * @property {String} url An absolute or relative URL of the resource that is being requested.
 * @property {Object.<String>} params A map of strings or objects which will be serialized and appended as GET parameters.
 * @property {String|Object} data Some data to be sent as the request message data.
 * @property {Object.<String>} headers A map of strings representing HTTP headers to send to the server.
 * @property {Boolean} withCredentials Whether to set the withCredentials flag on the underlying XHR object.
 */
/**
 * An object representing an HTTP response.
 * @typedef {Object} HttpResponse
 * @property {module:"js/mom.utils.service"~HttpRequest} config The configuration object that was used to generate the request.
 * @property {String|Object} data The response body (if a JSON response is returned, it will be automatically transformed into the corresponding Object).
 * @property {Number} status The HTTP status code of the response.
 * @property {String} statusText The HTTP status text of the response.
 */
/**
 * Executes an HTTP request with integrated logging and optionally triggering a progress indicator.
 * > **Tip:** This method is also exposed as the **MomHttpRequest** custom action.
 * > **Important:** By default, this method does not reject the Promise in case of HTTP errors.
 * @async
 * @param {module:"js/mom.utils.service"~HttpRequest} config The configuration of the HTTP request to execute.
 * @param {Object} [options={showModalIndicator: true, rejectOnError: true}] Additional options. The following options can be specified:
 *   * **showIndicator**: If set to **true**, a progress indicator will be displayed while the request is being executed.
 *   * **showModalIndicator**: If set to **true**, a modal progress indicator will be displayed while the request is being executed.
 *   * **rejectOnError**: If set to **true**, the method will reject the Promise in case of an HTTP error.
 * @returns {Promise<HttpResponse>} A promise wrapping an [HttpResponse](#~HttpResponse) object corresponding to the response of the request.
 */
exports.httpRequest = function( config, options ) {
    let opts = options || {};
    // Managing each supported option to handle usage in custom actions (attributes generated from exprs are always strings)
    let showIndicator = opts.showIndicator && opts.showIndicator !== "false";
    let showModalIndicator =
        opts.showModalIndicator === undefined ||
        ( opts.showModalIndicator && opts.showModalIndicator !== "false" );
    let rejectOnError =
        opts.rejectOnError === undefined ||
        ( opts.rejectOnError && opts.rejectOnError !== "false" );
    if( showModalIndicator ) {
        eventBus.publish( "modal.progress.start" );
    } else if( showIndicator ) {
        eventBus.publish( "progress.start" );
    }
    logger.trace( "MOM UI - httpRequest:", config, options );
    return awHttpSvc.instance( config )
        .then( function( resp ) {
            logger.trace( "MOM UI - httpRequest Response:", resp );
            return resp;
        } )
        .catch( function( err ) {
            logger.error( "MOM UI - httpRequest Error:", err );
            if( rejectOnError ) {
                throw err;
            }
            return err;
        } )
        .finally( function() {
            if( showModalIndicator ) {
                eventBus.publish( "modal.progress.end" );
            } else if( showIndicator ) {
                eventBus.publish( "progress.end" );
            }
        } );
};

/**
 * Shortcut method to execute an HTTP GET request, equivalent to calling [httpRequest](#.httpRequest) with the following [HttpRequest](#~HttpRequest) properties pre-set:
 * * method: **GET**
 * * url: _url_
 *
 * > **Tip:** This method is also exposed as the **MomHttpGet** custom action.
 * @param {String} url The URL to request.
 * @param {module:"js/mom.utils.service"~HttpRequest} config See [httpRequest](#.httpRequest).
 * @param {Object} [options={showModalIndicator: true, rejectOnError: true}] See [httpRequest](#.httpRequest).
 * @returns {Promise<HttpResponse>} See [httpRequest](#.httpRequest).
 */
exports.httpGet = function( url, config, options ) {
    let cfg = config || {};
    cfg.method = "GET";
    cfg.url = url;
    return exports.httpRequest( cfg, options );
};

/**
 * Shortcut method to execute an HTTP HEAD request, equivalent to calling [httpRequest](#.httpRequest) with the following [HttpRequest](#~HttpRequest) properties pre-set:
 * * method: **HEAD**
 * * url: _url_
 *
 * > **Tip:** This method is also exposed as the **MomHttpHead** custom action.
 * @param {String} url The URL to request.
 * @param {module:"js/mom.utils.service"~HttpRequest} config See [httpRequest](#.httpRequest).
 * @param {Object} [options={showModalIndicator: true, rejectOnError: true}] See [httpRequest](#.httpRequest).
 * @returns {Promise<HttpResponse>} See [httpRequest](#.httpRequest).
 */
exports.httpHead = function( url, config, options ) {
    let cfg = config || {};
    cfg.method = "HEAD";
    cfg.url = url;
    return exports.httpRequest( cfg, options );
};

/**
 * Shortcut method to execute an HTTP DELETE request, equivalent to calling [httpRequest](#.httpRequest) with the following [HttpRequest](#~HttpRequest) properties pre-set:
 * * method: **DELETE**
 * * url: _url_
 *
 * > **Tip:** This method is also exposed as the **MomHttpDelete** custom action.
 * @param {String} url The URL to request.
 * @param {module:"js/mom.utils.service"~HttpRequest} config See [httpRequest](#.httpRequest).
 * @param {Object} [options={showModalIndicator: true, rejectOnError: true}] See [httpRequest](#.httpRequest).
 * @returns {Promise<HttpResponse>} See [httpRequest](#.httpRequest).
 */
exports.httpDelete = function( url, config, options ) {
    let cfg = config || {};
    cfg.method = "DELETE";
    cfg.url = url;
    return exports.httpRequest( cfg, options );
};

/**
 * Shortcut method to execute an HTTP POST request, equivalent to calling [httpRequest](#.httpRequest) with the following [HttpRequest](#~HttpRequest) properties pre-set:
 * * method: **POST**
 * * data: _data_
 * * url: _url_
 *
 * > **Tip:** This method is also exposed as the **MomHttpPost** custom action.
 * @param {String} url The URL to request.
 * @param {Object} data The data to send along with the request.
 * @param {module:"js/mom.utils.service"~HttpRequest} config See [httpRequest](#.httpRequest).
 * @param {Object} [options={showModalIndicator: true, rejectOnError: true}] See [httpRequest](#.httpRequest).
 * @returns {Promise<HttpResponse>} See [httpRequest](#.httpRequest).
 */
exports.httpPost = function( url, data, config, options ) {
    let cfg = config || {};
    cfg.method = "POST";
    cfg.url = url;
    cfg.data = data;
    return exports.httpRequest( cfg, options );
};

/**
 * Shortcut method to execute an HTTP PUT request, equivalent to calling [httpRequest](#.httpRequest) with the following [HttpRequest](#~HttpRequest) properties pre-set:
 * * method: **PUT**
 * * data: _data_
 * * url: _url_
 *
 * > **Tip:** This method is also exposed as the **MomHttpPut** custom action.
 * @param {String} url The URL to request.
 * @param {Object} data The data to send along with the request.
 * @param {module:"js/mom.utils.service"~HttpRequest} config See [httpRequest](#.httpRequest).
 * @param {Object} [options={showModalIndicator: true, rejectOnError: true}] See [httpRequest](#.httpRequest).
 * @returns {Promise<HttpResponse>} See [httpRequest](#.httpRequest).
 */
exports.httpPut = function( url, data, config, options ) {
    let cfg = config || {};
    cfg.method = "PUT";
    cfg.url = url;
    cfg.data = data;
    return exports.httpRequest( cfg, options );
};

/**
 * Shortcut method to execute an HTTP PATCH request, equivalent to calling [httpRequest](#.httpRequest) with the following [HttpRequest](#~HttpRequest) properties pre-set:
 * * method: **PATCH**
 * * data: _data_
 * * url: _url_
 *
 * > **Tip:** This method is also exposed as the **MomHttpPatch** custom action.
 * @param {String} url The URL to request.
 * @param {Object} data The data to send along with the request.
 * @param {module:"js/mom.utils.service"~HttpRequest} config See [httpRequest](#.httpRequest).
 * @param {Object} [options={showModalIndicator: true, rejectOnError: true}] See [httpRequest](#.httpRequest).
 * @returns {Promise<HttpResponse>} See [httpRequest](#.httpRequest).
 */
exports.httpPatch = function( url, data, config, options ) {
    let cfg = config || {};
    cfg.method = "PATCH";
    cfg.url = url;
    cfg.data = data;
    return exports.httpRequest( cfg, options );
};

/**
 * Sets the title of the current Location, displayed in the application header.
 * @param {String} title The Location title to set.
 */
exports.setHeaderTitle = function( title ) {
    let titles = _appCtxSvc.getCtx( locationTitlesCtx );
    titles.headerTitle = title;
    _appCtxSvc.updateCtx( locationTitlesCtx, titles );
};

/**
 * Sets the header title, browser title, and browser subtitle.
 * @param {String} titles An object with the following properties, used to set the respective titles:
 * * browserSubTitle
 * * browserTitle
 * * headerTitle
 *
 * **Note** Existing titles will be used if not specified.
 */
exports.setLocationTitles = function( titles ) {
    let currentTitles = _appCtxSvc.getCtx( locationTitlesCtx );
    currentTitles = Object.assign( currentTitles, titles );
    _appCtxSvc.updateCtx( locationTitlesCtx, currentTitles );
};

/**
 * Navigates to the home page state that was configured for the current site _or_ to the location specified in the
 * **momDefaultPage** context.
 *
 * The value of **momDefaultPage** can be:
 *
 * * An absolute URL starting with **http:**
 * * A path name starting with <b>/</b>
 * * An url fragment starting with **#**
 */
exports.navigateToHomePage = function() {
    let homepage =
        _appCtxSvc.getCtx( "momDefaultPage" ) ||
        _appCtxSvc.getCtx( "workspace.defaultPage" );
    if( homepage.match( /^(#|https?:|\/)/ ) ) {
        exports.redirect( homepage );
    } else {
        awStateSvc.instance.go( _appCtxSvc.getCtx( "workspace.defaultPage" ) );
    }
};

/**
 * Navigates to the previous page in the browser history.
 */
exports.navigateToPreviousPage = function() {
    awWindowSvc.instance.history.back();
};

/**
 * Navigates to another state.
 * @param {String} state The ID of the state to navigate to.
 * @param {Object} [params={}] The parameters to pass to the state.
 * @param {Object} [options={}] Additional options. Currently the following properties are supported:
 * * **reload**: If set to **true**, forces the state to be reloaded even if not necessary (e.g. for a navigation to the current state).
 * * **notify**: If set to **false**, no internal events will be broadcasted when navigating to the new state.
 */
exports.navigateTo = function( state, params, options ) {
    eventBus.publish( 'mom.navigation.start', {
        state: state,
        params: params,
        options: options
    } );
    awStateSvc.instance.go( state, params, options );
};

/**
 * Rebuilds the navigation breadcrumb.
 * > **Note:** Calling this method is typically not necessary, as the breadcrumb is built automatically based on your **states.json** configuration. For more information, see [Configuring the navigation breadcrumb](https://gitlab.industrysoftware.automation.siemens.com/mom/mom-ui/wikis/configuring-the-navigation-breadcrumb) on the MOM UI Wiki.
 */
exports.breadcrumb = function() {
    return _breadcrumb.build();
};

/**
 * An object used to manage the navigation breadcrumb.
 * @typedef BreadcrumbProvider
 * @property {Crumb[]} crumbs An array of [Crumb](#~Crumb) objects representing the current navigation breadcrumb.
 * @property {Function} onSelect A function that will be executed to perform the navigation when a crumb is clicked. It takes a single [Crumb](#~Crumb) parameter.
 */
/**
 * An object used to represent a single crumb used in the navigation breadcrumb.
 * @typedef Crumb
 * @property {String} title The title of the crumb.
 * @property {String} stateId The ID of the state associated to the crumb.
 * @property {Object} [params={}] The parameters to pass to the crumb state when a navigation is performed (i.e. the user clicks the crumb).
 * @property {Boolean} [swacScreen=false] Whether the crumb is used to navigate to a SWAC Screen or not.
 */
/**
 * Overrides the default breadcrumb configuration and recreates a breadcrumb containing the specified crumbs.
 * > **Note:** Use this method only if you need to override the default breadcrumb. By default, the navigation breadcrumb is built automatically based on your **states.json** configuration. For more information, see [Configuring the navigation breadcrumb](https://gitlab.industrysoftware.automation.siemens.com/mom/mom-ui/wikis/configuring-the-navigation-breadcrumb) on the MOM UI Wiki.
 * @param {Crumb[]} crumbs An array of [Crumb](#~Crumb) objects used to configure the breadcrumb.
 * @returns {BreadcrumbProvider} The [BreadcrumbProvider](#~BreadcrumbProvider) object used to manage the breadcrumb.
 */
exports.setBreadcrumb = function( crumbs ) {
    _breadcrumb.reset();
    crumbs.forEach( function( crumb ) {
        _breadcrumb.addLocationCrumb( {
            title: crumb.title,
            stateId: crumb.id || crumb.stateId,
            stateParams: crumb.params,
            swacScreen: crumb.swacScreen
        } );
    } );
    let updatedCrumbs = _breadcrumb.provider().crumbs;
    if( updatedCrumbs && updatedCrumbs.length > 0 ) {
        updatedCrumbs[ 0 ].primaryCrumb = true;
    }
    eventBus.publish( "momNavigateBreadcrumb.reset", { crumbs: updatedCrumbs } );
    return _breadcrumb.provider();
};

/**
 * Adds the specified title as selected item in the navigation breadcrumb.
 * > **Note:** Use this method only if you need to override the default breadcrumb. By default, the navigation breadcrumb is built automatically based on your **states.json** configuration. For more information, see [Configuring the navigation breadcrumb](https://gitlab.industrysoftware.automation.siemens.com/mom/mom-ui/wikis/configuring-the-navigation-breadcrumb) on the MOM UI Wiki.
 * @param {String} title The string to display as the last item of the breadcrumb (typically used to indicate an item selection).
 * @returns {BreadcrumbProvider} The [BreadcrumbProvider](#~BreadcrumbProvider)  object used to manage the breadcrumb.
 */
exports.setBreadcrumbSelection = function( title ) {
    _breadcrumb.setBreadcrumbSelection( title );
    return _breadcrumb.provider();
};

/**
 * Creates a ViewModelProperty object that can be used in most of Siemens Web Framework input custom elements.
 * @param {String} name The identifier of the property.
 * @param {String} displayName The label of the property.
 * @param {String} type The type of the property. It can be one of the following:
 * * CHAR
 * * DATE
 * * DOUBLE
 * * FLOAT
 * * INTEGER
 * * BOOLEAN
 * * SHORT
 * * STRING
 * @param {*} [dbValue=undefined] The value of the property.
 * @param {String} [uiValue=String(dbValue)] The display value of the property.
 * > **IMPORTANT:** This value _must_ be a String, otherwise it will not be displayed.
 * @returns {ViewModelProperty} A valid ViewModelProperty object.
 */
exports.prop = function( name, displayName, type, dbValue, uiValue ) {
    let displayValue = uiValue || String( dbValue );
    let prop = _uwPropertySvc.createViewModelProperty(
        name,
        displayName,
        type,
        dbValue,
        [ displayValue ]
    );
    prop.propApi = {};
    return prop;
};

/**
 * Converts an object with properties set to simple values (String, Boolean, Number or Date) into an object containing the same property values but wrapped in ViewModelProperty objects.
 * Note that:
 * * The internal type of the ViewModelProperty objects is determined automatically based on the JavaScript type of the original value.
 * * The resulting ViewModelProperty objects will have a label set to the same value as their identifier.
 * * The resulting ViewModelProperty objects will have a uiValue set to their dbValue converted to a String.
 * @param {Object<String|Boolean|Number|Date>} obj An object containing the properties that need to be converted to ViewModelProperty objects.
 * @return {Object<ViewModelProperty>} An object whose properties are valid ViewModelProperty objects.
 */
exports.propsObj = function( obj ) {
    let result = {};
    for( let prop in obj ) {
        if( obj.hasOwnProperty( prop ) ) {
            let type;
            let value = obj[ prop ];
            // 'CHAR', 'DATE', 'DOUBLE', 'FLOAT', 'INTEGER', 'BOOLEAN', 'SHORT', 'STRING', 'OBJECT'
            switch ( typeof obj[ prop ] ) {
                case "string":
                    type = "STRING";
                    break;
                case "boolean":
                    type = "BOOLEAN";
                    break;
                case "number":
                    if( obj[ prop ] % 1 === 0 ) {
                        type = "INTEGER";
                    } else {
                        type = "DOUBLE";
                    }
                    break;
                case "object":
                    if( obj[ prop ] instanceof Date ) {
                        type = "DATE";
                        value = obj[ prop ].getTime();
                    } else {
                        // Arrays are not managed
                        type = "OBJECT";
                    }
                    break;
                default:
                    break;
            }
            result[ prop ] = exports.prop( prop, prop, type, value );
        }
    }
    return result;
};

/**
 * Specifies the visibility of the command that displays command labels.
 * @param {Boolean} value Specifies whether the Toggle Command Labels command is disabled or not.
 */
exports.setToggleCommandLabelsCommands = function( value ) {
    _appCtxSvc.updateCtx( "disableToggleCommandLabels", !value );
};

/**
 * Specifies whether command labels are displayed or not in the main SubLocation location command bars.
 * @param {Boolean} value Whether command labels are displayed or not.
 */
exports.setCommandLabels = function( value ) {
    _appCtxSvc.updateCtx( "commandLabels", value ); // Classic
    if( value ) {
        // UX Refresh
        _appCtxSvc.registerCtx( "toggleLabel", true );
        $( ".locationPanel" ).addClass( "aw-commands-showIconLabel" );
    } else {
        _appCtxSvc.updateCtx( "toggleLabel", false );
        $( ".locationPanel" ).removeClass( "aw-commands-showIconLabel" );
    }
};

/**
 * Toggles whether command labels are displayed or not in the main SubLocation location command bars.
 */
exports.toggleCommandLabels = function() {
    exports.setCommandLabels( !_appCtxSvc.getCtx( "commandLabels" ) );
};

/**
 * Wraps the specified value into a Promise.
 * @param {*} value The value to transform into a Promise.
 * @returns {Promise<*>} The Promise containing the value specified as input.
 */
exports.promisify = function( value ) {
    return awPromiseSvc.instance( function( resolve ) {
        resolve( value );
    } );
};

/**
 * Redirects to screen corresponding to the specified URL fragment.
 * @param {String} url The URL fragment to redirect to.
 */
exports.redirect = function( url ) {
    awWindowSvc.instance.location.href = url;
};

/**
 * An object used to configure the global navigation toolbar panel.
 * @typedef GlobalNavigationToolbar
 * @property {Boolean} showPanel Whether to display the panel open (true) or not (false).
 * @property {String} viewName The name of the view/viewModel to load in the navigation panel.
 * @property {Boolean} pinned Whether the navigation panel is pinned (true) or not (false).
 */
/**
 * Activates the primary navigation panel to display a list of navigation links.
 *
 * > **Tip:** This method is also exposed as the **MomActivatePrimaryNavigationPanel** custom action.
 *
 * If you require more customization, you can provide a [GlobalNavigationToolbar](#~GlobalNavigationToolbar) object
 * as **cfg**. This object will be stored in the **globalNavigationToolbar** context.
 *
 * For the simplest usage, the **cfg** object hould only contain a **panel** property set to a unique identifier for the navigation panel, that will be stored in the **momPrimaryNavigationPanel** context. For more information, see the [Configuring primary navigation links](https://gitlab.industrysoftware.automation.siemens.com/mom/mom-ui/wikis/configuring-primary-navigation-links) on the MOM UI Wiki.
 * @param {Object} cfg The navigation panel configuration.
 * @deprecated This method is deprecated as of v0.41.0, and it no longer works with the current version of Siemens Web Framework. Use the awsidenav.openClose event instead.
 *
 */
exports.activatePrimaryNavigationPanel = function( cfg ) {
    let toolbarPanelContext = {
        showPanel: cfg.showPanel !== false ? "true" : "false",
        viewName: cfg.viewName || "momPrimaryNavigationPanel",
        pinned: cfg.pinned
    };
    _appCtxSvc.registerCtx( "momPrimaryNavigationPanel", cfg.panel );
    _appCtxSvc.registerCtx( "globalNavigationToolbar", toolbarPanelContext );
};

/**
 * An object used to configure a composite action.
 * @typedef {Object} CompositeActionConfig
 * @property {DeclViewModel} vm A reference to the viewModel of the action(s) to execute (only necessary for the first action executed).
 * @property {String} action The ID of an action to execute (configure either **action** or **actions**).
 * @property {String[]} actions An array containing the IDs of the actions to execute in parallel (configure either **action** or **actions**).
 * @property {CompositeActionConfig} success A [CompositeActionConfig](#~CompositeActionConfig) object determining the action(s) to execute on success, and optional actions to execute
 * in case of success or failure.
 * @property {CompositeActionConfig} failure A [CompositeActionConfig](#~CompositeActionConfig) object determining the action(s) to execute on failure, and optional actions to execute
 * in case of success or failure.
 *
 */
/**
 * Executes a sequence of actions asynchronously.
 * > **Tip** This method is also exposed as the **MomCompositeAction** custom action.
 * > For more information on how to use it, see the [Configuring composite actions](https://gitlab.industrysoftware.automation.siemens.com/mom/mom-ui/wikis/Configuring-composite-actions) page on the MOM UI Wiki.
 * @param {CompositeActionConfig} cfg A [CompositeActionConfig](#~CompositeActionConfig) object determining the action(s) to execute, and optional actions to execute
 * in case of success or failure.
 * @returns {Promise<*>} An object containing the **value** and **values** properties, corresponding to the result(s) of the action(s) executed.
 */
exports.compositeAction = function( cfg ) {
    let localData = {
        data: cfg.vm,
        ctx: _appCtxSvc.ctx,
        result: cfg.result
    };
    let createCfg = function( key, cfg, results ) {
        return {
            vm: cfg.vm,
            actions: cfg[ key ].actions || [ cfg[ key ].action ],
            success: cfg[ key ].success,
            failure: cfg[ key ].failure,
            result: results.length === 1 ? results[ 0 ] : results
            //value: results[ 0 ],
            //values: results
        };
    };
    let promises = [];
    cfg.actions = cfg.actions || [ cfg.action ];
    cfg.actions.forEach( function( actionId ) {
        logger.info( "MOM Composite Action - calling: ", actionId );
        let action = cfg.vm._internal.actions[ actionId ];
        let depModuleObj = null;
        if( action.deps ) {
            promises.push(
                declUtils
                .loadDependentModule( action.deps, awPromiseSvc.instance, app.getInjector() )
                .then( function( depModuleObj ) {
                    return _actionSvc.executeAction(
                        cfg.vm,
                        action,
                        localData,
                        depModuleObj
                    );
                } )
            );
        } else {
            promises.push(
                _actionSvc.executeAction(
                    cfg.vm,
                    action,
                    localData,
                    depModuleObj
                )
            );
        }
    } );
    return awPromiseSvc.instance
        .all( promises )
        .then( function( results ) {
            if( cfg.success ) {
                if(!cfg.vm.isDestroyed()) {
                let successCfg = createCfg( "success", cfg, results );
                logger.info( "MOM Composite Action - calling success action" );
                return exports.compositeAction( successCfg );
                }
            }
            logger.info( "MOM Composite Action: success" );
            //return { values: results, value: results[ 0 ] };
            return results.length === 1 ? results[ 0 ] : results;
        } )
        .catch( function( error ) {
            let results = error.constructor === Array ? error : [ error ];
            if( cfg.failure ) {
                if(!cfg.vm.isDestroyed()) {
                let failureCfg = createCfg( "failure", cfg, results );
                logger.info( "MOM Composite Action - calling failure action" );
                return exports.compositeAction( failureCfg );
                }
            }
            logger.info( "MOM Composite Action: failure" );
            //throw { values: results, value: results[ 0 ] };
            throw results.length === 1 ? results[ 0 ] : results;
        } );
};

/**
 * Scrolls to the specify ID or name on the current view.
 * @param {String} id The ID to scroll to.
 */
exports.scrollTo = function( id ) {
    let el = document.getElementById( id );
    el =
        el ||
        ( document.getElementsByName( id ).length !== 0 &&
            document.getElementsByName( id )[ 0 ] );
    if( !el ) {
        logger.warn(
            'MOM UI - mom.utils.service#scrollTo: Unable to find element with ID or name "' +
            id +
            '"'
        );
        return;
    }
    if( el.scrollIntoView ) {
        el.scrollIntoView( { behavior: "smooth", block: "start" } );
    } else {
        momAnchorScrollSvc.instance( id );
    }
};

/**
 * Retrieves the specified Siemens Web Framework configuration file.
 * > **Tip** This method is also exposed as the **MomGetCfg** custom action.
 * @param {String} file The name of the JSON configuration file to retrieve, without extension.
 * @returns {Promise<*>} The contents of the configuration file.
 */
exports.getCfg = function( file ) {
    return _cfgSvc.getCfg( file );
};

/**
 * Updates the contents of the specified configuration file called **name** with the ones provided as **cfg** parameter,
 * and triggers the appropriate configuration update event.
 * @param {String} name The name of the configuration file to update (without extension).
 * @param {Object} cfg An object that will be merged with the existing configuration.
 */
exports.updateConfiguration = function( name, cfg ) {
    _cfgSvc.getCfg( name ).then( function( cVM ) {
        _.merge( cVM, cfg );
        eventBus.publish( "configurationChange." + name );
    } );
};

/**
 * Updates the contents of the **commandsViewModel** configuration files with the ones provided as **cfg** parameter,
 * and triggers an application-wide command update.
 * @param {Object} cfg An object compatible with the **commandsViewModel** format.
 */
exports.updateCommands = function( cfg ) {
    return exports.updateConfiguration( "commandsViewModel", cfg );
};

exports._userActivityMonitoring = {
    callback: function() {
        eventBus.publish( "mom.user.activity" );
    },
    listener: null,
    events: ""
};
/**
 * Defines additional settings to fine-tune the behavior of the [startMonitoringUserActivity](#.startMonitoringUserActivity) method.
 * @typedef DebounceConfig
 * @property {Boolean} leading The **mom.user.activity** event will be published on the leading edge of the timeout.
 * @property {Number} maxWait The maximum time the **mom.user.activity** event is allowed to be delayed before it is published.
 * @property {Boolean} trailing The **mom.user.activity** event will be published on the trailing edge of the timeout.
 */
/**
 * Starts monitoring user activity by checking mouse/keyboard events.
 * After a specified number of milliseconds, the **mom.user.activity** event is published if the user is considered active.
 * @param {Array<String>} [events=[ 'click', 'keydown', 'keyup', 'mousemove' ]] The events used to monitor user activity.
 * @param {Number} [wait=300000] The number of milliseconds to wait before firing the **mom.user.activity** event.
 * @param {Object} [options={leading: true, maxWait: 900000, trailing: false}] A [DebounceConfig](#~DebounceConfig) object containing additional options.
 */
exports.startMonitoringUserActivity = function( events, wait, options ) {
    let waitValue = wait || 300000; // 5 minutes
    let eventsValue = events || [ "click", "keydown", "keyup", "mousemove" ];
    let optionsValue = options || {
        leading: true,
        maxWait: 900000,
        trailing: false
    };
    exports._userActivityMonitoring.events = eventsValue.join( " " );
    if( exports._userActivityMonitoring.events ) {
        exports._userActivityMonitoring.listener = _.debounce(
            exports._userActivityMonitoring.callback,
            waitValue,
            optionsValue
        );
        $( document ).on(
            ( exports._userActivityMonitoring.events,
                exports._userActivityMonitoring.listener )
        );
    }
};

/**
 * Stops monitoring user activity (if previously started using the [startMonitoringUserActivity](#.startMonitoringUserActivity) method).
 */
exports.stopMonitoringUserActivity = function() {
    if( exports._userActivityMonitoring.events ) {
        $( document ).off(
            exports._userActivityMonitoring.events,
            exports._userActivityMonitoring.listener
        );
        exports._userActivityMonitoring.listener = null;
        exports._userActivityMonitoring.events = "";
    }
};

app.factory( "momUtilsService", () => exports );

export default exports;
