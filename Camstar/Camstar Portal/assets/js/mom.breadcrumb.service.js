// Copyright 2020 Siemens AG

/**
 * **Note:** This module is for internal use only.
 * @module "js/mom.breadcrumb.service"
 * @requires app
 * @requires js/eventBus
 * @requires lodash
 * @requires "js/momc.ctx.service"
 * @ignore
 */
/*global
 */
/* eslint-disable require-jsdoc */
import app from 'app';
import eventBus from 'js/eventBus';
import _ from 'lodash';
import appCtxSvc from 'js/appCtxService';
import awStateSvc from 'js/awStateService';
import momStateParamsSvc from 'js/mom.stateParams.service';
import momCtxSvc from 'js/mom.ctx.service';

'use strict';
const exports = {};

// Update breadcrumb when location title changes or an item is selected
eventBus.subscribe( 'appCtx.update', function( data ) {
    if( data.name === 'location.titles' && data.value ) {
        updateLocationTitle( awStateSvc.instance.current.name, data.value.headerTitle );
    } else if( data.name === 'selected' ) {
        createOrUpdateSelectedCrumb();

    }
} );
eventBus.subscribe( 'appCtx.register', function( data ) {
    if( data.name === 'selected' ) {
        createOrUpdateSelectedCrumb();
    }
} );

let provider = {
    crumbs: [],
    onSelect: function( crumb ) {
        if( crumb.stateId ) {
            let state = awStateSvc.instance.get( crumb.stateId );
            let params = {};
            if( crumb.stateParams ) {
                params = crumb.stateParams;
            } else {
                if( state && state.data && state.data.params ) {
                    params = state.data.params;
                }
                Object.getOwnPropertyNames( momStateParamsSvc.instance ).forEach( function( pName ) {
                    if( typeof momStateParamsSvc.instance[ pName ] === 'string' ) {
                        params[ pName ] = params[ pName ] || momStateParamsSvc.instance[ pName ];
                    }
                } );
            }
            let reload = awStateSvc.instance.current.name === crumb.stateId;
            awStateSvc.instance.go( crumb.stateId, params, { reload: reload } );
        }
    }
};

function locationCrumb( c ) {
    let id = c.stateId || awStateSvc.instance.current.name;
    let displayName = c.title;
    if( !displayName ) {
        let titles = appCtxSvc.getCtx( 'location.titles' );
        if( titles ) {
            try {
                displayName = momCtxSvc._getTitleFromState( id, displayName );
            } catch ( e ) {
                displayName = null;
            } finally {
                displayName = displayName || titles.headerTitle;
            }
        }
    }
    let result = {
        displayName: displayName,
        showArrow: c.showArrow || false,
        stateId: id, // custom
        stateParams: c.stateParams,
        selectedCrumb: false,
        clicked: false
    };
    if( c.swacScreen ) {
        result.swacScreen = c.swacScreen;
    }
    return result;
}

function selectionCrumb( title ) {
    return {
        displayName: title,
        showArrow: false,
        selectedCrumb: true,
        clicked: false
    };
}

function updateLocationTitle( stateId, title ) {
    let locationCrumbs = provider.crumbs.filter( function( crumb ) {
        return !crumb.selectedCrumb;
    } );
    if( locationCrumbs.length > 0 ) {
        locationCrumbs[ locationCrumbs.length - 1 ].displayName = title;
    }
}

function createOrUpdateSelectedCrumb( cfg ) {
    if( provider.crumbs.length <= 0 ) {
        return;
    }
    let config = cfg || _.get( awStateSvc.instance, "current.data.breadcrumbConfig" );
    if( config ) {
        let selected = appCtxSvc.getCtx( 'selected' ) || {};
        let selectedTitle = _.get( selected, config.selectionTitleField );
        provider.crumbs[ provider.crumbs.length - 1 ].showArrow = false;
        if( config.selectionTitleField && selected !== {} && selectedTitle ) {
            if( typeof( selectedTitle === 'object' ) && selectedTitle.dbValue ) {
                selectedTitle = selectedTitle.dbValue;
            }
            exports.setBreadcrumbSelection( selectedTitle );
        } else {
            exports.unsetBreadcrumbSelection();
        }
    }
}

exports.setBreadcrumbSelection = function( title ) {
    if( provider.crumbs.length <= 0 ) {
        return;
    }
    let last = provider.crumbs[ provider.crumbs.length - 1 ];
    if( last && last.selectedCrumb ) {
        last.displayName = title;
    } else if( last && last.displayName === title ) {
        // nothing to do
    } else {
        provider.crumbs[ provider.crumbs.length - 1 ].showArrow = true;
        provider.crumbs.push( selectionCrumb( title ) );
    }
};

exports.unsetBreadcrumbSelection = function() {
    if( provider.crumbs.length <= 1 ) {
        return;
    }
    if( provider.crumbs[ provider.crumbs.length - 1 ].selectedCrumb ) {
        provider.crumbs.pop();
        provider.crumbs[ provider.crumbs.length - 1 ].showArrow = false;
    }
};

exports.reset = function() {
    provider.crumbs = [];
};

exports.provider = function() {
    return provider;
};

exports.addLocationCrumb = function( c ) {
    provider.crumbs.forEach( function( crumb ) {
        crumb.showArrow = true;
    } );
    provider.crumbs.push( locationCrumb( c ) );
};

exports.getCrumbs = function() {
    return provider.crumbs;
};

exports.select = function( crumb ) {
    return provider.onSelect( crumb );
};

exports.setCrumbs = function( crumbs, cfg ) {
    if( crumbs && crumbs.length > 0 ) {
        provider.crumbs = crumbs;
        return provider;
    }
    return exports.build( cfg );
};

exports.build = function( cfg ) {
    let config = cfg || _.get( awStateSvc.instance, "current.data.breadcrumbConfig" );
    provider.crumbs = [];
    provider.crumbs.push( locationCrumb( {
        showArrow: true
    } ) ); // current location
    while( config && config.parent ) {
        let state = awStateSvc.instance.get( config.parent );
        let title;
        if( state && 'data' in state && 'breadcrumbConfig' in state.data ) {
            config = state.data.breadcrumbConfig;
            try {
                title = momCtxSvc._getTitleFromState( awStateSvc.instance.get( state.parent ).name, title );
            } catch ( e ) {
                title = null;
            }
            title = title || awStateSvc.instance.get( state.parent ).data.headerTitle;
            provider.crumbs.push( locationCrumb( {
                title: title,
                stateId: state.name,
                showArrow: true
            } ) );
        } else {
            config = {};
        }
    }
    provider.crumbs = provider.crumbs.reverse();
    createOrUpdateSelectedCrumb( config );
    if( provider.crumbs.length > 0 ) {
        provider.crumbs[ 0 ].primaryCrumb = true;
    }
    return provider;
};

app.factory( 'momBreadcrumbService', () => exports );

export default exports;
