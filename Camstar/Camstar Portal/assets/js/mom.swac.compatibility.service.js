// Copyright 2020 Siemens AG
/**
 * Register all SWAC services and retrieves all SWAC interfaces necessary to manage MOM SWAC Screens.
 *
 * More specifically, the following services will be registered via the  [init](#.init) method:
 *
 * * {@link module:"MOM.UI.Busy"|MOM.UI.Busy}
 * * {@link module:"MOM.UI.Confirmation"|MOM.UI.Confirmation}
 * * {@link module:"MOM.UI.Context"|MOM.UI.Context}
 * * {@link module:"MOM.UI.Error"|MOM.UI.Error}
 * * {@link module:"MOM.UI.Warning"|MOM.UI.Warning}
 * * {@link module:"MOM.UI.EventBus"|MOM.UI.EventBus}
 * * {@link module:"MOM.UI.I18n"|MOM.UI.I18n}
 * * {@link module:"MOM.UI.Navigation"|MOM.UI.Navigation}
 * * {@link module:"MOM.UI.Notification"|MOM.UI.Notification}
 * * {@link module:"MOM.UI.Theme"|MOM.UI.Theme}
 *
 * Additionally, the following SWAC Interfaces are used by this service:
 *
 * * {@link external:"MOM.UI.Authenticable"|MOM.UI.Authenticable}
 * * {@link external:"MOM.UI.Localizable"|MOM.UI.Localizable}
 * * {@link external:"MOM.UI.Navigable"|MOM.UI.Navigable}
 * * {@link external:"MOM.UI.Themable"|MOM.UI.Themable}
 *
 * > **Tip** For more information on how to use this service, see the following pages on the MOM UI Wiki:
 * >
 * > * [SWAC Compatibility Module](https://gitlab.industrysoftware.automation.siemens.com/mom/mom-ui/wikis/swac-compatibility-module)
 * > * [How to use the SWAC Compatibility Module](https://gitlab.industrysoftware.automation.siemens.com/mom/mom-ui/wikis/how-to-use-the-swac-compatibility-module)
 * @module "js/mom.swac.compatibility.service"
 * @requires app
 * @requires js/eventBus
 * @requires js/logger
 * @requires @swac/container
 * @requires js/mom.swac.eventBus.service
 * @requires js/configurationService
 * @requires js/appCtxService
 * @requires js/mom.utils.service
 * @requires js/mom.swac.error.service
 * @requires js/mom.swac.warning.service
 * @requires js/mom.swac.theme.service
 * @requires js/mom.swac.i18n.service
 * @requires js/mom.swac.busy.service
 * @requires js/mom.swac.navigation.service
 * @requires js/mom.swac.confirmation.service
 * @requires js/mom.swac.notification.service
 * @requires js/mom.breadcrumb.service
 */
/**
 * SWAC Interface used to authenticate a SWAC Screen with a token.
 * @external "MOM.UI.Authenticable"
 */
/**
 * Passes an authentication token to the SWAC Screen.
 * The token is retrieved from a context that is specified using one of the following properties of the **mom-swac-screen.json** configuration file:
 * * **settings.authentication.tokenContext**
 * * **settings.*componentName*.authentication.tokenContext**
 * @function external:"MOM.UI.Authenticable"#setToken
 * @param {*} token The authentication token.
 */
/**
 * Notifies that the authentication token is expired.
 * This event is automatically re-published on eventBus as **mom.ui.authenticable.*componentName*.onTokenExpiration**.
 * @event external:"MOM.UI.Authenticable"#onTokenExpiration
 *
 */
/**
 * SWAC Interface used to set and retrieve the theme of a SWAC Screen.
 * @external "MOM.UI.Themable"
 */
/**
 * Retrieves the current theme of the SWAC Screen.
 * @function external:"MOM.UI.Themable"#getTheme
 * @returns {Promise<String>} The ID of the current theme used by the SWAC Screen wrapped in a Promise.
 *
 */
/**
 * Sets the theme of the SWAC Screen
 * @function external:"MOM.UI.Themable"#setTheme
 * @param {String} theme The ID of the theme.
 * @returns {Promise} A promise fulfilled if the operation was successful.
 */
/**
 * SWAC Interface used to set and retrieve the locale of a SWAC Screen.
 * @external "MOM.UI.Localizable"
 */
/**
 * Retrieves the current locale of the SWAC Screen.
 * @function external:"MOM.UI.Localizable"#getLocale
 * @returns {Promise<String>} The ID of the current locale used by the SWAC Screen wrapped in a Promise.
 *
 */
/**
 * Sets the locale of the SWAC Screen
 * @function external:"MOM.UI.Localizable"#setLocale
 * @param {String} locale The ID of the locale, e.g. **en_US**.
 * @returns {Promise} A promise fulfilled if the operation was successful.
 */
/**
 * SWAC Interface used to set and retrieve the location (SWAC Component URL) of a SWAC Screen.
 * @external "MOM.UI.Navigable"
 */
/**
 * Retrieves the absolute URL of the SWAC Screen.
 * @function external:"MOM.UI.Navigable"#getLocation
 * @returns {Promise<String>} The absolute URL of the SWAC Screen wrapped in a Promise.
 *
 */
/**
 * Sets the URL of the SWAC Screen, forcing a new SWAC Screen to be loaded.
 * @function external:"MOM.UI.Navigable"#navigateTo
 * @param {String} screen The full absolute URL of a valid SWAC Screen to display.
 * @returns {Promise} A promise fulfilled if the operation was successful.
 */
import app from 'app';
import eventBus from 'js/eventBus';
import logger from 'js/logger';
import SWACKit from '@swac/container';
import SwacEventBus from 'js/mom.swac.eventBus.service';
import browserUtils from 'js/browserUtils';
import cfgSvc from 'js/configurationService';
import appCtxSvc from 'js/appCtxService';
import utils from 'js/mom.utils.service';
import swacErrorSvc from 'js/mom.swac.error.service';
import swacWarningSvc from 'js/mom.swac.warning.service';
import swacThemeSvc from 'js/mom.swac.theme.service';
import swacI18nSvc from 'js/mom.swac.i18n.service';
import swacBusySvc from 'js/mom.swac.busy.service';
import swacNavigationSvc from 'js/mom.swac.navigation.service';
import swacNotificationSvc from 'js/mom.swac.notification.service';
import swacConfirmationSvc from 'js/mom.swac.confirmation.service';
import breadcrumb from 'js/mom.breadcrumb.service';
import awLocationSvc from 'js/awLocationService';
import awNarrowModeSvc from 'js/aw.narrowMode.service';

const exports = {
    component: null, // the current exports.SWAC Screen Component
    initialized: false,
    componentData: {},
    SWAC: new SWACKit()
};

const momSwacScreens = browserUtils.getUrlAttributes().momSwacScreens;

const getComponentIframe = () => {
    const obj = exports.component._internal.iframe;
    // From SWAC v1.6.0+ it is necessary to use the getIframe method.
    return obj.getIFrame && obj.getIFrame() || obj;
};

exports._withConfig = ( callback ) => {
    return cfgSvc.getCfg( momSwacScreens || 'mom-swac-screens' ).then( ( cfg ) => {
        exports.config = cfg;
        return callback();
    } );
};

exports._init = function( screen ) {
    if( !exports.initialized ) {
        // Modify breadcrumb behavior to manage SWAC Screen navigation
        const bp = breadcrumb.provider();
        const onSelect = bp.onSelect;
        bp.onSelect = function( crumb ) {
            if( crumb.swacScreen ) {
                return exports.navigateToState( crumb.id || crumb.stateId );
            }
            return onSelect( crumb );
        };
        return exports._withConfig( () => {
            exports._registerServices();
            exports._initializeComponent( screen );
            exports.initialized = true;
        } );
    }
    return Promise.resolve();
};

exports._registerServices = function() { //eslint-disable-line valid-jsdoc, require-jsdoc
    exports.SWAC.Services.register( 'MOM.UI.Error', swacErrorSvc );
    exports.SWAC.Services.register( 'MOM.UI.Warning', swacWarningSvc );
    exports.SWAC.Services.register( 'MOM.UI.Theme', swacThemeSvc );
    exports.SWAC.Services.register( 'MOM.UI.I18n', swacI18nSvc );
    exports.SWAC.Services.register( 'MOM.UI.Busy', swacBusySvc );
    exports.SWAC.Services.register( 'MOM.UI.Navigation', swacNavigationSvc );
    exports.SWAC.Services.register( 'MOM.UI.Confirmation', swacConfirmationSvc );
    exports.SWAC.Services.register( 'MOM.UI.Context', appCtxSvc );
    exports.SWAC.Services.register( 'MOM.UI.EventBus', new SwacEventBus() );
    exports.SWAC.Services.register( 'MOM.UI.Notification', swacNotificationSvc );
};

exports._registerComponent = function( event ) {
    let name = event.data.name;
    exports.component = exports.SWAC.Container.get( { name: name } );
};

exports._dispatchEvent = function( event ) {
    getComponentIframe().dispatchEvent( event );
};

exports._registerMouseEvents = function() {
    exports.component.onMouseEvents.subscribe( exports._dispatchEvent, [ 'click', 'doubleclick', 'mousedown', 'mouseup' ] );
};

exports._unregisterMouseEvents = function() {
    exports.component.onMouseEvents.unsubscribe( exports._dispatchEvent );
};

exports.config = {};

exports._cleanup = function() {
    exports._unregisterMouseEvents();
    exports._unregisterTokenExpirationEvent();
    exports.component = null;
    exports.componentData = {};
};

exports._showComponent = function( screen ) {
    logger.info( 'Component ready: ' + exports.component.name() );
    // Set styles:
    let leftMargin;
    if(awNarrowModeSvc.isNarrowMode()) {
        leftMargin = 8;
    } else {
        leftMargin = 64;
    }
    getComponentIframe().style.margin = `56px 8px 0px ${leftMargin}px`;
    getComponentIframe().style.zIndex = 100;
    exports.SWAC.Container.onRemoved.subscribe( exports._cleanup );
    exports._registerMouseEvents();
    const promises = [];
    promises.push( exports._setTheme() );
    promises.push( exports._setLocale() );
    promises.push( exports._initializeAuthentication() );
    return Promise.all( promises ).then( () => {
        return exports.component.beginShow( true ).then( () => {
            eventBus.publish( 'mom.swac.screen.loadEnd', { name: exports.component.name() } );
            return exports._initializeSwacState( screen );
        } );
    } );
};

exports._initializeComponent = function( screen ) { //eslint-disable-line valid-jsdoc, require-jsdoc
    exports.SWAC.Container.onCreated.subscribe( function( event ) {
        logger.info( 'Component created: ' + event.data.name );
        exports._registerComponent( event );
        exports.component.onReady.subscribe( function() {
            exports._showComponent( screen );
        } );
    } );
    // Adjust component theme when container theme changes
    eventBus.subscribe( 'ThemeChangeEvent', function() {
        exports._setTheme( exports.component );
    } );
    // Adjust SWAC Component width when side panel is pinned
    const adjustWidth = () => {
        let offset;
        if(awNarrowModeSvc.isNarrowMode()){
            offset = 8;
        } else{
        const pinned = appCtxSvc.ctx.awSidenavConfig &&
            appCtxSvc.ctx.awSidenavConfig.globalSidenavContext &&
            appCtxSvc.ctx.awSidenavConfig.globalSidenavContext.globalNavigationSideNav &&
            appCtxSvc.ctx.awSidenavConfig.globalSidenavContext.globalNavigationSideNav.pinned;

        const open = appCtxSvc.ctx.awSidenavConfig &&
            appCtxSvc.ctx.awSidenavConfig.globalSidenavContext &&
            appCtxSvc.ctx.awSidenavConfig.globalSidenavContext.globalNavigationSideNav &&
            appCtxSvc.ctx.awSidenavConfig.globalSidenavContext.globalNavigationSideNav.open;
        if( open && pinned ) {
            let previousSideNavWidth = previousSideNavWidth === undefined && appCtxSvc.ctx.previousSideNavWidth > 0 ?
            appCtxSvc.ctx.previousSideNavWidth : appCtxSvc.ctx.configuredWidth;
            offset = 64 + previousSideNavWidth;
        } else {
            offset = 64;
        }
}
        getComponentIframe().style.marginLeft = `${offset}px`;
        getComponentIframe().style.width = `calc(100% - ${8 + offset}px)`;

        if( appCtxSvc.ctx.fullscreen ) {
            getComponentIframe().style.marginTop = '0';
            getComponentIframe().style.height = '100%';
        } else {
            getComponentIframe().style.marginTop = '56px';
            getComponentIframe().style.height = 'calc(100% - 56px)';
        }
    };
    eventBus.subscribe( 'narrowModeChangeEvent', ( data ) => {
        adjustWidth();
    } );
    if( !exports.initialized ) {
        window.addEventListener( "resize", adjustWidth );
    }
    eventBus.subscribe( 'appCtx.*', ( data ) => {
        if( ( data.name === "awSidenavConfig" || data.name === "fullscreen") && exports.component ) {
            adjustWidth();
        }
    } );

    //Adjust the width based on config when the SWAC component loaded.
     eventBus.subscribe( 'mom.swac.screen.loadEnd', () => {
         if (exports.component) {
             adjustWidth();
         }
    } );

    // Check when user panel is open as well
    // eventBus.subscribe( 'avatar.contentLoaded', () => {
    //     adjustWidth();
    // } );
    // eventBus.subscribe( 'momPrimaryNavigationPanel.contentLoaded', () => {
    //     adjustWidth();
    // } );

    //side nav open and close
    eventBus.subscribe( 'awsidenav.resizeEnded', () => {
        appCtxSvc.registerCtx( "previousSideNavWidth", document.getElementById( 'globalNavigationSideNav' ).clientWidth );
        adjustWidth();
    } );

    if( exports.config.componentUrl ) {
        // Programmatically create a persistent SWAC component.
        let config;
        try {
            appCtxSvc.registerCtx( 'momSingleSwacScreen', true );
            config = {
                name: exports.config.default,
                source: exports.config.componentUrl,
                settings: {
                    width: awNarrowModeSvc.isNarrowMode() ? 'calc(100% - 16px)' : 'calc(100% - 8px - 64px)',
                    height: 'calc(100% - 56px)',
                    left: 0,
                    top: 0,
                    flavor: 'ui',
                    designMode: false,
                    addDpcValues: false,
                    activateRemoveOnRedirect: 5
                }
            };
        } catch ( err ) {
            throw new Error( 'Incorrect subLocation DOM structure, aborting SWAC Screen creation' );
        }
        // Show/hide component when entering in SWAC SubLocation
        eventBus.subscribe( '$locationChangeStart', ( data ) => {
            if( exports.component.beginShow ) {
                const position = exports.component.getPosition();
                if( data.newUrl.match( /#\/screen/ ) ) {
                    if( position.height === '0px' ) {
                        exports.component.beginShow( true );
                        // Visibility event is published when the controller is loaded.
                    }
                } else {
                    if( position.height !== '0px' ) {
                        exports._navigateToInternalComponentState( exports.config.hidden || 'hidden' );
                        exports.component.beginShow( false );
                    }
                }
            }
        } );
        exports.SWAC.Container.onFailure.subscribe( function( event ) {
            eventBus.publish( 'mom.swac.screen.loadFailed', { name: event.data.name, reason: event.data.details.reason } );
        } );
        exports.SWAC.Container.beginCreate( config );
    }
};

exports._logError = ( error ) => logger.warn( error );

exports._withInterface = ( reqInterface, callback ) => {
    return new Promise( resolve => {
        if( ( exports.component.interfaces.has( reqInterface ) ) ) {
            return exports.component.interfaces.beginGet( reqInterface )
                .then( interf => resolve( callback( interf ) ), resolve( exports._logError ) );
        }
        return resolve();
    } );
};

exports._setTheme = () => { //eslint-disable-line valid-jsdoc, require-jsdoc
    return exports._withInterface( 'MOM.UI.Themable', ( interf ) => {
        return interf.getTheme().then( theme => interf.setTheme( theme ) );
    } );
};

exports._setLocale = function() { //eslint-disable-line valid-jsdoc, require-jsdoc
    return exports._withInterface( 'MOM.UI.Localizable', ( interf ) => {
        return interf.getLocale().then( locale => interf.setLocale( locale ) );
    } );
};

exports._initializeSwacState = function( screen ) { //eslint-disable-line valid-jsdoc, require-jsdoc
    return new Promise( resolve => {
        let stateParams = awLocationSvc.instance.search();
        if( exports.config.componentUrl ) {
            return exports._navigateToInternalComponentState( screen, stateParams );
        }
        return resolve();
    } );
};

exports._initializeAuthentication = function() {
    let settings = exports.config.settings ? exports.config.settings[ exports.component.name() ] : exports.config;
    let auth = exports.config.authentication || settings && settings.authentication && settings.authentication;
    return exports._withInterface( 'MOM.UI.Authenticable', ( interf ) => {
        exports.componentData.tokenContext = auth.tokenContext;
        exports.componentData.authenticableInterface = interf;
        let tokenContext = exports.componentData.tokenContext;
        if( tokenContext ) {
            let token = appCtxSvc.getCtx( tokenContext );
            interf.setToken( token );
            exports._registerTokenExpirationEvent();
        }
        return Promise.resolve();
    } );
};

eventBus.subscribe( 'appCtx.*', function( data ) {
    if( exports.componentData.tokenContext && exports.componentData.authenticableInterface && data.name === exports.componentData.tokenContext ) {
        exports.componentData.authenticableInterface.setToken( data.value );
    }
} );

exports._publishTokenExpirationEvent = function( event ) {
    eventBus.publish( 'mom.ui.authenticable.' + exports.component.name() + '.onTokenExpiration', event );
};

exports._registerTokenExpirationEvent = function() {
    if( exports.componentData.authenticableInterface ) {
        exports.componentData.authenticableInterface.onTokenExpiration.subscribe( exports._publishTokenExpirationEvent );
    }
};

exports._unregisterTokenExpirationEvent = function() {
    if( exports.componentData.authenticableInterface ) {
        exports.componentData.authenticableInterface.onTokenExpiration.unsubscribe( exports._publishTokenExpirationEvent );
    }
};

exports._navigateToInternalComponentState = ( state, stateParams ) => {
    return exports._withInterface( 'MOM.UI.Navigable', ( interf ) => {
        interf.navigateToState( state, stateParams );
        return Promise.resolve( state, stateParams );
    } );
};

/**
 * Navigates to a SWAC Screen defined in the **mom-swac-screen.json** file specifying its ID.
 *
 * Note that:
 *
 * * If the specified screen is part of the currently-loaded SWAC Screen component, the navigation will be delegated to the current component.
 * * If the specified screen belongs to another SWAC Screen component, the new component will be loaded.
 *
 * A screen is considered to be part of the current component if its URL (up to the fragment) matches the URL of the current component.
 *
 * @param {String} screen The ID of the SWAC Screen to navigate to.
 * @return {Promise} A resolved promise containing the name of the SWAC Screen.
 */
exports.navigateTo = ( screen ) => {
    return exports._withConfig( () => {
        let currUrl = exports.config.screens[ exports.component.name() ];
        let newUrl = exports.config.screens[ screen ];
        let navigateToComponent = function( notify ) {
            utils.navigateTo( 'momSwacSublocation', { screen: screen }, { notify: notify, reload: notify } );
        };
        if( screen === exports.config.hidden ) {
            // Navigating to hidden internal state
            return exports.navigateToUrl( newUrl );
        }
        if( newUrl && currUrl.match( newUrl.replace( /#.+$/, '' ) ) ) {
            // Update container fragment
            navigateToComponent( false );
            // Navigate internally
            exports.navigateToUrl( newUrl );
        } else {
            navigateToComponent( true );
        }
        return Promise.resolve( screen );
    } );
};

/**
 * Navigates to a SWAC Screen accessible at the specified **url**.
 * @param {String} url The URL of the SWAC Screen to navigate to.
 * @return {Promise} A resolved promise containing the URL of the SWAC Screen.
 */
exports.navigateToUrl = ( url ) => {
    return exports._withInterface( 'MOM.UI.Navigable', ( interf ) => {
        interf.navigateTo( url );
        return Promise.resolve( url );
    } );
};

/**
 * Navigates to a SWAC Screen accessible at the specified **state**.
 *
 * > **Note:** This method should only be used when there is no need to specify an URL for a new SWAC component when performing internal navigation within the same component.
 * @param {String} state The state name of the SWAC Screen to navigate to.
 * @return {Promise} A resolved promise containing the name of the SWAC Screen state.
 */
exports.navigateToState = function( state ) {
    return exports._withConfig( () => {
        let newState = exports.config.screens[ state ];
        let navigateToComponent = function navigateToComponent( notify ) {
            utils.navigateTo( 'momSwacSublocation', {
                screen: state
            }, {
                notify: notify,
                reload: false
            } );
        };
        if( exports.component ) {
            navigateToComponent( exports.config.componentUrl );
            exports._navigateToInternalComponentState( newState );
        } else {
            navigateToComponent( true );
        }
        return Promise.resolve( screen );
    } );
};

/**
 * @module "js/appCtxService"
 * @name "MOM.UI.Context"
 * @description This SWAC Service provides a way to interact with AFX context from a SWAC Component.
 */
/**
 * @method registerCtx
 * @description Register application context variable
 * @param {String} name - The name of context variable
 * @param {Object} value - The value of context variable
 * @static
 */
/**
 * @method unRegisterCtx
 * @description Unregister application context variable
 * @param {String} name - The name of context variable
 * @static
 */
/**
 * @method updateCtx
 *  @description Update application context and Announce app context update by publishing an event
 * 'appCtx.update' with eventData as {"name": ctxVariableName, "value": ctxVariableValue}
 * @param {String} name - The name of context variable
 * @param {Object} value - The value of context variable
 * @static
 */
/**
 * @method getCtx
 * @description Get application context variable value
 * @param {String} path - Path to the context
 * @returns {Object} Value (if any) at the indicated context path location.
 * @static
 */

app.factory( 'momSwacCompatibilityService', () => exports );

export default exports;
