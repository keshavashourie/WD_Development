// Copyright 2020 Siemens AG
/* eslint-disable valid-jsdoc */
/**
 * This SWAC Service provides a way for a SWAC Component to navigate to another Siemens Web Framework screen.
 * @module "js/mom.swac.navigation.service"
 * @name "MOM.UI.Navigation"
 * @requires app
 */
import app from 'app';
import utils from 'js/mom.utils.service';
import awStateSvc from 'js/awStateService';
import awLocationSvc from 'js/awLocationService';
import eventBus from 'js/eventBus';
import awTimeoutSvc from 'js/awTimeoutService';
import awRootScopeSvc from 'js/awRootScopeService';
import momStateParamsSvc from 'js/mom.stateParams.service';

const exports = {};

let _momScreenName = undefined;
let _momScreenParams = undefined;

/**
 * Navigates to another state.
 * @param {String} id The ID of the state to navigate to.
 * @param {Object} [params={}] The parameters to pass to the state.
 * @param {Object} [options={}] Additional options. Currently the following properties are supported:
 * * **reload**: If set to **true**, forces the state to be reloaded even if not necessary (e.g. for a navigation to the current state).
 * * **notify**: If set to **false**, no internal events will be broadcasted when navigating to the new state.
 */
exports.navigateTo = function( id, params, options ) {
    awStateSvc.instance.go( id, params, options );

    // non UAF screens
    if( !params || id !== 'momSwacSublocation' ) {
        return;
    }

    var momScreenName = params.screen;
    var momScreenParams = params.parameters ? params.parameters : {};

    // first navigation
    if( _momScreenName === undefined ) {
        _momScreenName = momScreenName;
        _momScreenParams = momScreenParams;
        return;
    }

    _momScreenName = momScreenName;
    _momScreenParams = momScreenParams;
    exports.applyQueryString();
};

/**
 * Sets the query string to the specified value.
 * @param {String|Object.<string>|Object.<Array.<string>>} search The new query string expressed as string or object (or a key to set if **paramValue** is specified).
 * @param {String|Boolean|Array.<string>|Number} [paramValue=undefined] The value of the search parameter, if **search** is a string.
 */
exports.setQueryString = function() {
    if( _momScreenName && momStateParamsSvc.instance.screen !== _momScreenName ) {
        if( arguments.length === 2 ) {
            _momScreenParams[ arguments[ 0 ] ] = arguments[ 1 ];
        } else if( arguments.length === 1 ) {
            _momScreenParams[ arguments[ 0 ] ] = undefined;
        }
        return;
    }

    if( arguments.length === 2 ) {
        awLocationSvc.instance.search( arguments[ 0 ], arguments[ 1 ] );
    } else if( arguments.length === 1 ) {
        awLocationSvc.instance.search( arguments[ 0 ] );
    }
    eventBus.publish( 'locationChange', {
        stateName: _momScreenName,
        stateParams: awLocationSvc.instance.search()
    } );

    if( awRootScopeSvc.instance.$$phase !== '$apply' && awRootScopeSvc.instance.$$phase !== '$digest' ) {
        awRootScopeSvc.instance.$apply();
    }
};

/**
 * Returns the value of the current query string.
 * @returns {Object} The object containing the query string parameters.
 */
exports.getQueryString = function() {
    if( _momScreenName && momStateParamsSvc.instance.screen !== _momScreenName ) {
        return _momScreenParams;
    }
    return awLocationSvc.instance.search();
};

exports.applyQueryString = function() {
    if( _momScreenName && momStateParamsSvc.instance.screen !== _momScreenName ) {
        awTimeoutSvc.instance( exports.applyQueryString );
        return;
    }
    awLocationSvc.instance.search( _momScreenParams );
    eventBus.publish( 'locationChange', awLocationSvc.instance.search() );

    if( awRootScopeSvc.instance.$$phase !== '$apply' && awRootScopeSvc.instance.$$phase !== '$digest' ) {
        awRootScopeSvc.instance.$apply();
    }
};

/**
 * See the [setLocationTitles](module-_js_mom.utils.service_.html#.setLocationTitles) method.
 */
exports.setLocationTitles = function( titles ) {
    return utils.setLocationTitles( titles );
};

/**
 * See the [setBreadcrumb](module-_js_mom.utils.service_.html#.setBreadcrumb) method.
 */
exports.setBreadcrumb = function( crumbs ) {
    return utils.setBreadcrumb( crumbs );
};

/** eslint-disable-line valid-jsdoc
 * See the [setBreadcrumbSelection](module-_js_mom.utils.service_.html#.setBreadcrumbSelection) method.
 */
exports.setBreadcrumbSelection = function( title ) {
    return utils.setBreadcrumbSelection( title );
};

app.factory( 'momSwacNavigationService', () => exports );

export default exports;
