// Copyright 2020 Siemens AG
/**
 * **Note:** This module is for internal use only.
 * @module "js/mom.init.service"
 * @requires app
 * @requires logger
 * @requires eventBus
 * @requires appCtxService
 * @requires configurationService
 * @ignore
 */
/* eslint-disable valid-jsdoc */
import app from "app";
import eventBus from "js/eventBus";
import logger from "js/logger";
import appCtxSvc from "js/appCtxService";
import cfgSvc from "js/configurationService";
import localeSvc from "js/localeService";

const exports = {};

exports.init = function() {
    logger.trace( "MOM UI - Environment initialization start" );
    eventBus.publish( "mom.init" );

    // Register contexts
    appCtxSvc.registerCtx( "momBaseUrl", app.getBaseUrlPath() );
    appCtxSvc.registerCtx( "momChangeThemeDisabled", true );
    appCtxSvc.registerCtx( "currentYear", new Date().getFullYear() );

    //Set ctx for width config
    eventBus.subscribe( 'awsidenav.openClose', ( data ) => {
        if( appCtxSvc.ctx.configuredWidth === undefined ) {
            if( data !== undefined && data.config !== undefined && data.config.width !== undefined ) {
                appCtxSvc.registerCtx( "configuredWidth", data.config.width === "WIDE" ? 280 : 180 );
            } else {
                appCtxSvc.registerCtx( "configuredWidth", 180 );
            }
        }
    } );

    // Override to reset back button
    localeSvc
        .getLocalizedText( "BaseMessages", "BACK_BUTTON_TITLE" )
        .then( ( s ) => {
            appCtxSvc.registerCtx( "previousLocationDisplayName ", s );
            eventBus.subscribe( "appCtx.*", ( data ) => {
                if( data.name === "previousLocationDisplayName " ) {
                    // Reset without notifying
                    appCtxSvc.ctx[ "previousLocationDisplayName " ] = s;
                }
            } );
        } );

    eventBus.subscribe( "primaryWorkArea.selectionChangeEvent", function( data ) {
        appCtxSvc.updateCtx(
            "momPrimarySelection",
            data.dataProvider.selectedObjects
        );
        appCtxSvc.updateCtx( "momSecondarySelection", [] );
    } );

    eventBus.subscribe( "secondaryWorkArea.selectionChangeEvent", function(
        data
    ) {
        appCtxSvc.updateCtx(
            "momSecondarySelection",
            data.dataProvider.selectedObjects
        );
    } );

    eventBus.subscribe( "appCtx.register", function( context ) {
        if( context.name === "ViewModeContext" ) {
            if( context.value && context.value.supportedViewModes ) {
                appCtxSvc.registerCtx(
                    "momSupportedViewModesList",
                    Object.keys( context.value.supportedViewModes )
                );
            }
        }
    } );

    // Disable right-click on aw-tile-canvas
    eventBus.subscribe( "*.contentLoaded", () => {
        const tiles = document.querySelectorAll( "aw-tile > .aw-tile-tileContainer" );
        if( tiles.length > 0 ) {
            tiles.unbind( "contextmenu" );
        }
    } );

    const body = document.getElementsByTagName( "BODY" )[ 0 ];
    // Add version info to body tag

    const promises = [];
    promises.push(
        cfgSvc.getCfg( "versionConstants" ).then( ( cfg ) => {
            body.dataset.afxVersion = cfg.afx.version;
        } )
    );
    promises.push(
        cfgSvc.getCfg( "OSSAttributionInfo" ).then( ( cfg ) => {
            if( cfg[ "mom-ui" ] ) {
                body.dataset.momUiVersion = cfg[ "mom-ui" ].version;
            }
        } )
    );
    promises.push(
        cfgSvc.getCfg( 'momAcbConfiguration' ).then( ( cfg ) => {
            appCtxSvc.updateCtx( 'momAcbConfiguration', cfg );
        } )
    );
    return Promise.all( promises ).finally( () => {
        logger.trace( "MOM UI - Environment initialization end" );
    } );
};

app.factory( "momInitService", () => exports );
window.MOMUI = exports;
export default exports;
