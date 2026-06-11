// Copyright 2020 Siemens AG

/**
 * Provides ways to manage application contexts in a state-specific ways, through a configuration file (**mom-config.json**).
 *
 * > **Tip:** For more information on how to use this module, see the [Using the MOM Context Service](https://gitlab.industrysoftware.automation.siemens.com/mom/mom-ui/wikis/using-the-mom-context-service) page on the MOM UI Kit Wiki.
 * @module "js/mom.ctx.service"
 * @requires app
 * @requires js/localeService
 * @requires js/configurationService
 * @requires js/appCtxService
 * @requires js/viewModelProcessingFactory
 * @requires js/actionService
 * @requires js/declUtils
 */
/*global
 */
/* eslint-disable require-jsdoc */
import app from 'app';
import _ from 'lodash';
import declUtils from 'js/declUtils';
import logger from 'js/logger';
import localeSvc from 'js/localeService';
import cfgSvc from 'js/configurationService';
import appCtxSvc from 'js/appCtxService';
import viewModelProc from 'js/viewModelProcessingFactory';
import actionSvc from 'js/actionService';
import awPromiseSvc from 'js/awPromiseService';
import awStateSvc from 'js/awStateService';
import awHttpSvc from 'js/awHttpService';
import awParseSvc from 'js/awParseService';

'use strict';
const exports = {};

/**
 * An object containing the configuration of this service, retrieved from the **mom-config.json** file.
 *
 * > **Tip:** For more details on the structure of this object, see the [Using the MOM Context Service](https://gitlab.industrysoftware.automation.siemens.com/mom/mom-ui/wikis/using-the-mom-context-service) page on the MOM UI Kit Wiki.
 * @typedef MomConfig
 * @property {Object} stateContexts An object containing state-specific context configurations.
 * @property {Object} initContexts A dictionary of contexts to set at startup.
 * @property {Object} clearCtxExpressions A dictionary of conditions (expressions) used to clear specific contexts, if satisfied.
 * @property {Object} titleExpressions A dictionary containing expressions to be used to populate the titles of specific states.
 * @property {Object} actions A dictionary of actions typically used to populate contexts.
 * @property {Object} data An dictionary of properties that can be accessed in expressions within the service configuration.
 * @property {Object} i18n A dictionary used to resolve translations for the specified identifiers.
 */
/**
 * The configuration of this service, loaded from the **mom-config.json** file.
 * @member {module:"js/mom.ctx.service"~MomConfig} [config={}]
 */
exports.config = {};
exports._dataContext = { ctx: appCtxSvc.ctx };
exports._lastTitle = "";
exports._sequentialQueue = new function() {
    //sequential ctx check execution ...reentrancy when configured both location and sublocation
    let promiseQueue = [];
    return {
        getQueuePromise: function() {
            let p = awPromiseSvc.instance.defer();
            let r = awPromiseSvc.instance.defer();
            promiseQueue.push( {
                p: p,
                r: r
            } );
            r.promise.then().finally( function() {
                promiseQueue = promiseQueue.filter( function( elem ) {
                    return elem.r !== r;
                } );
                if( promiseQueue.length > 0 ) {
                    promiseQueue[ 0 ].p.resolve( promiseQueue[ 0 ].r );
                }
            } );
            if( promiseQueue.length === 1 ) { //first in list ... manual trigger
                p.resolve( r );
            }
            return p.promise;
        }
    };
}();
exports._actionListCache = {}; //cache the list of hierachy action based on state name
exports._languageRepository = {};
exports._i18nPromise = null;
exports._declUtils = declUtils;

exports._timeEnd = function( start ) {
    return Date.now() - start + "ms";
};

exports._getCfg = function( config ) {
    return cfgSvc.getCfg( config.cfg ).then( function( data ) {
        return data;
    } ).catch( function() {
        return {}; // never fail
    } );
};

exports._reset = function() {
    exports.config = {};
    exports._dataContext = {};
    exports._lastTitle = "";
    exports._actionListCache = {};
    exports._languageRepository = {};
    exports._i18nPromise = null;
    exports._declUtils = declUtils;
};

exports._getHierarchyActionList = function( conf, that ) {
    //extract from conf the hierachy of actions for the current state (that)
    let currS = that;
    let cfgStates = [];
    let searS = null;
    let actionList = [];

    if( conf.stateContexts && Object.getOwnPropertyNames( conf.stateContexts ).length > 0 ) {
        cfgStates = Object.getOwnPropertyNames( conf.stateContexts );
        while( currS !== null ) {
            searS = cfgStates.filter( function( elem ) {
                return ( currS.self !== undefined && elem === currS.self.name ) || ( currS.name !== undefined && elem === currS.name );
            } );
            if( searS.length > 0 ) {
                while( searS.length > 0 ) {
                    actionList.push( conf.stateContexts[ searS[ 0 ] ] ); //found link to cfg
                    searS = cfgStates.filter( function( elem ) {
                        return elem === conf.stateContexts[ searS[ 0 ] ].parent;
                    } );
                }
                break;
            } else if( "parent" in currS ) {
                currS = awStateSvc.instance.get( currS.parent );
            } else {
                currS = null;
            }
        }
        actionList.reverse();
    }
    return actionList;
};

//create a flat list of the action
function getActions( ctxActionArray ) {
    let resultList = [];
    let elem = ctxActionArray.pop();
    while( elem ) {
        if( "nextCtx" in elem ) {
            if( Array.isArray( elem.nextCtx ) ) {
                resultList = resultList.concat( getActions( elem.nextCtx ) );
                delete elem.nextCtx;
            }
            delete elem.nextCtx;
        }
        resultList.push( elem );
        elem = ctxActionArray.pop();
    }
    return resultList;
}

function reorderActions( actionList ) {
    let resultList = [];
    let elem;
    while( actionList.length > 0 ) {
        elem = actionList.pop();
        if( "config" in elem && Array.isArray( elem.config ) ) {
            resultList = resultList.concat( getActions( elem.config ) );
        }
    }
    return resultList;
}

exports._getCacheableList = function( actionList ) {
    let orderedList = reorderActions( JSON.parse( JSON.stringify( actionList ) ) ).reverse();
    orderedList = groupPriority( orderedList ); //group by priority
    return orderedList.newArray.concat( orderedList.noPrority );
};

exports._getUpdatedCtx = function() {
    exports._dataContext.ctx = appCtxSvc.ctx;
    exports._dataContext.data = exports.config.data;
    if( !( 'params' in exports._dataContext.ctx.state ) ) { //f5 case ... no params from Angular
        exports._dataContext.ctx.state.params = exports._dataContext.stateParams;
    }
    // Update the context with the most-recent stateParams
    appCtxSvc.ctx.state.params = exports._dataContext.stateParams;
    return exports._dataContext;
};

exports._parseExpression = function( exp, extraContext ) {
    let exprCondition = awParseSvc.instance( exp );
    let ctx = exports._getUpdatedCtx();
    if( typeof extraContext === "object" && extraContext !== null ) {
        Object.getOwnPropertyNames( extraContext ).forEach( function( pName ) {
            ctx[ pName ] = extraContext[ pName ];
        } );
    }
    return exprCondition( ctx );
};

exports._formatOutput = function( data, action ) {
    if( "outputFormatExpression" in action ) {
        return exports._parseExpression( action.outputFormatExpression, {
            result: data
        } );
    }
    return data;
};

exports._callAction = function( action ) {
    exports._getUpdatedCtx();
    let localDefer = awPromiseSvc.instance.defer();
    let defvalue = {};
    let theAction;
    let priority = ( "priority" in action ) ? action.priority : 'no';
    let startTime = Date.now();
    if( "ctx" in action && "inputAction" in action ) {
        theAction = exports.config.actions[ action.inputAction ];
        if( theAction ) {
            logger.trace( "Start action(" + priority + "): " + action.inputAction );
            let cm = viewModelProc.createDeclViewModel( {
                _viewModelId: "ctxVM"
            } );
            exports._declUtils.loadDependentModule( theAction.deps, awPromiseSvc.instance, app.getInjector() ).then( function( depMod ) {
                try {
                    let result = actionSvc.executeAction( cm, theAction, exports._getUpdatedCtx(), depMod );
                    if( typeof result.then === 'function' ) {
                        result.then( function( realResult ) {
                            realResult = exports._formatOutput( realResult, action );
                            appCtxSvc.updateCtx( action.ctx, realResult );
                            logger.trace( exports._timeEnd( startTime ), "Start action(" + priority + "): " + action.inputAction );
                            localDefer.resolve( realResult );
                        }, function() {
                            logger.trace( exports._timeEnd( startTime ), "Start action(" + priority + "): " + action.inputAction );
                            localDefer.resolve( defvalue );
                        } );
                    } else {
                        result = exports._formatOutput( result, action );
                        appCtxSvc.updateCtx( action.ctx, result );
                        logger.trace( exports._timeEnd( startTime ), "Start action(" + priority + "): " + action.inputAction );
                        localDefer.resolve( result );
                    }
                } catch ( e ) {
                    logger.trace( exports._timeEnd( startTime ), "Start action(" + priority + "): " + action.inputAction );
                    localDefer.resolve( defvalue );
                }
            } ).catch( function() {
                logger.trace( exports._timeEnd( startTime ), "Start action(" + priority + "): " + action.inputAction );
                localDefer.resolve( defvalue );
            } );
        }
    } else {
        logger.trace( exports._timeEnd( startTime ), "Start action(" + priority + "): " + action.inputAction );
        localDefer.resolve( defvalue );
    }
    return localDefer.promise;
};

exports._callCfg = function( action ) {
    if( "inputCfg" in action && "ctx" in action ) {
        let priority = ( "priority" in action ) ? action.priority : 'no';
        let startTime = Date.now();
        logger.trace( "Start load cfg (" + priority + "): " + action.ctx );
        return exports._getCfg( {
            ctx: action.ctx,
            cfg: action.inputCfg
        } ).then( function( result ) {
            logger.trace( exports._timeEnd( startTime ), "Start load cfg (" + priority + "): " + action.ctx );
            return appCtxSvc.updateCtx( action.ctx, exports._formatOutput( result, action ) );
        } );
    }
    return awPromiseSvc.instance.when();
};

exports._callFunction = function( action ) {
    if( "inputFunction" in action && "ctx" in action ) {
        let priority = "priority" in action ? action.priority : 'no';
        let startTime = Date.now();
        logger.trace( "Start call function(" + priority + "): " + action.inputFunction.method );
        let param = [];
        if( "params" in action.inputFunction ) {
            if( Array.isArray( action.inputFunction.params ) ) {
                param = action.inputFunction.params.map( function( elem ) {
                    if( typeof elem === "string" && elem.indexOf( '{{' ) === 0 ) {
                        elem = elem.substring( 2 );
                        if( elem.indexOf( '}}' ) === ( elem.length - 2 ) ) {
                            elem = elem.substring( 0, elem.length - 2 );
                        }
                        elem = exports._parseExpression( elem );
                    }
                    return elem;
                } );
            }
        }

        return serviceCaller( action.inputFunction.deps, action.inputFunction.method, param ).then( function( result ) {
            logger.trace( exports._timeEnd( startTime ), "Start call function(" + priority + "): " + action.inputFunction.method );
            return appCtxSvc.updateCtx( action.ctx, exports._formatOutput( result, action ) );
        } );
    }
    return awPromiseSvc.instance.when();
};

exports._callData = function( action ) {
    if( "inputData" in action && "ctx" in action ) {
        let priority = ( "priority" in action ) ? action.priority : 'no';
        let startTime = Date.now();
        logger.trace( "Start load data (" + priority + "): " + action.inputData );
        let result = exports.config.data[ action.inputData ];
        result = exports._formatOutput( result, action );
        appCtxSvc.updateCtx( action.ctx, result );
        logger.trace( exports._timeEnd( startTime ), "Start load data (" + priority + "): " + action.inputData );
    }
    return awPromiseSvc.instance.when();
};

exports._callHttpGet = function( action ) {
    let priority = ( "priority" in action ) ? action.priority : 'no';
    let startTime = Date.now();
    if( "inputHttpGetExpression" in action && "ctx" in action ) {
        logger.trace( "Start httpGET (" + priority + "): " + action.ctx );
        let query = exports._parseExpression( action.inputHttpGetExpression );
        return awHttpSvc.instance.get( query, {
            withCredentials: false
        } ).then( function( result ) {
            result = exports._formatOutput( result, action );
            appCtxSvc.updateCtx( action.ctx, result );
            logger.trace( exports._timeEnd( startTime ), "Start httpGET (" + priority + "): " + action.ctx );
        } ).catch( function() {
            logger.trace( "Start httpGET - FAILED" );
        } );
    }
    return awPromiseSvc.instance.when();
};

exports._execAction = function( action ) {
    if( "inputAction" in action ) {
        return exports._callAction( action );
    } else if( "inputCfg" in action ) {
        return exports._callCfg( action );
    } else if( "inputData" in action ) {
        return exports._callData( action );
    } else if( "inputHttpGetExpression" in action ) {
        return exports._callHttpGet( action );
    } else if( "inputFunction" in action ) {
        return exports._callFunction( action );
    }
    return awPromiseSvc.instance.when();
};

exports._buildCaller = function( actionList ) {
    let localDefer = awPromiseSvc.instance.defer();
    let fcore = function() {
        localDefer.resolve();
        return localDefer.promise;
    };
    let orderedList = actionList;

    if( orderedList.length > 0 ) {
        let allP = [];
        let next = localDefer.promise;
        orderedList.forEach( function( elem ) {
            next = next.then( function() {
                if( Array.isArray( elem ) ) {
                    return awPromiseSvc.instance.all( elem.map( function( action ) { //parallel executions
                        if( !( "condition" in action ) || ( exports._parseExpression( action.condition ) ) ) { // conditions
                            return exports._execAction( action );
                        }
                        return awPromiseSvc.instance.when();

                    } ) );
                }
                if( elem && ( !( "condition" in elem ) || ( exports._parseExpression( elem.condition ) ) ) ) { // conditions
                    return exports._execAction( elem );
                }

            } );
            allP.push( next );
        } );
        fcore = function() {
            localDefer.resolve(); //trigger Start Call
            return awPromiseSvc.instance.all( allP );
        };
    }

    return function() {
        return fcore();
    };
};

function groupPriority( orderedList ) {
    let retObj = {
        newArray: [],
        noPrority: []
    };
    orderedList.forEach( function( elem ) {
        if( "priority" in elem && typeof elem.priority === "number" ) {
            if( !Array.isArray( this.newArray[ elem.priority ] ) ) {
                this.newArray[ elem.priority ] = [];
            }
            this.newArray[ elem.priority ].push( elem );
        } else {
            this.noPrority.push( elem );
        }
    }, retObj );
    return retObj;
}
let actionServiceCache = { //cache dynamic loaded service for inputFunction
};

function getService( servicePath ) {
    let sN = servicePath.replace( /\//g, '.' );
    if( sN in actionServiceCache ) {
        return actionServiceCache[ sN ].promise;
    }
    actionServiceCache[ sN ] = {
        path: servicePath,
        promise: exports._declUtils.loadDependentModule( servicePath, awPromiseSvc.instance, app.getInjector() )
    };
    return actionServiceCache[ sN ].promise.then( function( service ) {
        return service;
    }, function( reason ) {
        throw reason;
    } );

}

function serviceCaller( servicePath, method, params ) {
    let retDefer = awPromiseSvc.instance.defer();
    getService( servicePath ).then( function( service ) {
        let result = null;
        if( typeof service[ method ] === 'function' ) {
            result = service[ method ].apply( null, params );
            if( result && typeof result.then === 'function' ) {
                result.then( function() {
                    retDefer.resolve( result );
                } );
            } else {
                retDefer.resolve( result );
            }
        }
    }, function( reason ) {
        retDefer.reject( reason );
    } );
    return retDefer.promise;
}

exports._checkCtx = function( that ) {
    let startTime = Date.now();
    logger.trace( "Add CTX - Start" );
    let currSName;
    if( that.self !== undefined ) {
        currSName = that.self.name;
    } else {
        if( that.name !== undefined ) {
            currSName = that.name;
        } else {
            currSName = 'unknown';
        }
    }
    let localDefer = awPromiseSvc.instance.defer();
    let actionList = null;
    //cache action list build
    if( currSName in exports._actionListCache ) {
        actionList = JSON.parse( JSON.stringify( exports._actionListCache[ currSName ] ) ); //use cache
    } else {
        actionList = exports._getHierarchyActionList( exports.config, that );
        actionList = exports._getCacheableList( actionList );
        exports._actionListCache[ currSName ] = JSON.parse( JSON.stringify( actionList ) );

    }
    exports._buildCaller( actionList )().then( function() { //apply condition and execute
        localDefer.resolve();
    } );
    return localDefer.promise.then( function() {
        logger.trace( "Total time:", exports._timeEnd( startTime ) );
    } );
};

exports._clearCtx = function() {
    if( "clearCtxExpressions" in exports.config ) {
        let logTxt = "";
        Object.getOwnPropertyNames( exports.config.clearCtxExpressions ).forEach( function( ctxName ) {
            let ctxValue = appCtxSvc.getCtx( ctxName );
            if( ctxValue !== null && ctxValue !== undefined ) {
                if( exports._parseExpression( exports.config.clearCtxExpressions[ ctxName ] ) ) {
                    if( logTxt.length === 0 ) {
                        logTxt = "Clear CTX: [ ";
                    }
                    logTxt = logTxt + ctxName + ' ';
                    appCtxSvc.unRegisterCtx( ctxName );
                }
            }
        } );
        if( logTxt.length !== 0 ) {
            logger.trace( logTxt + "]" );
        }
    }
};

exports._setTitle = function( titleExpr, that ) {
    let title = titleExpr ? exports._parseExpression( titleExpr ) : exports._lastTitle;
    exports._lastTitle = title;
    let titles = appCtxSvc.getCtx( 'location.titles' ) || {};
    titles.headerTitle = title;
    appCtxSvc.updateCtx( 'location.titles', titles );
    if( that.data ) {
        that.data.headerTitle = title;
    }
};

exports._localResolver = function( checkMandatory, addTitle ) {
    return [ '$stateParams', function( $stateParams ) {
        let localDefer = awPromiseSvc.instance.defer();
        let that = this; // eslint-disable-line no-invalid-this, consistent-this
        exports._i18nPromise.finally( function() {
            exports._sequentialQueue.getQueuePromise().then( function( done ) {
                let sParams = $stateParams;
                exports._dataContext.stateParams = sParams;
                exports._clearCtx(); //first clear unused
                if( checkMandatory ) { //recreate context only when involved configured state
                    return exports._checkCtx( that ).then( function() {
                        done.resolve();
                        exports._setTitle( addTitle, that );
                        localDefer.resolve();
                    } );
                }
                exports._setTitle( addTitle, that );
                done.resolve();
                localDefer.resolve();
            } );
        } );
        return localDefer.promise;
    } ];
};

exports._getTitleFromState = function( sName, title ) {
    if( typeof exports.config !== 'object' ) {
        return title;
    }
    if( exports.config && 'titleExpressions' in exports.config && sName in exports.config.titleExpressions ) {
        return exports._parseExpression( exports.config.titleExpressions[ sName ] );
    }
    let allS = awStateSvc.instance.get();
    let theS = allS.filter( function( s ) {
        return s.name === sName;
    } );
    if( theS.length === 1 ) {
        theS = theS[ 0 ].parent;
        while( theS ) {
            if( exports.config && 'titleExpressions' in exports.config && theS in exports.config.titleExpressions ) {
                return exports._parseExpression( exports.config.titleExpressions[ theS ] );
            }
            theS = awStateSvc.instance.get( theS );
            theS = theS.parent || null;
        }
    }
    return title;
};

/**
 * Initializes the service. This method must be called once, as early as possible (typically within the **checkIfSessionAuthenticated** method of your Authenticator module).
 * @returns {Promise.<MomConfig>} The configuration object loaded from the **mom-config.json** file.
 */
exports.init = function() {
    let languagesPromises = [];
    return cfgSvc.getCfg( 'mom-config' ).then( function( conf ) {
        exports.config = conf;
        // Set initial contexts
        if( conf.initContexts ) {
            Object.getOwnPropertyNames( conf.initContexts ).forEach( function( ctx ) {
                appCtxSvc.registerCtx( ctx, conf.initContexts[ ctx ] );
            } );
        }
        let stateList = awStateSvc.instance.get();
        stateList.forEach( function( s ) {
            let state = awStateSvc.instance.get( s.name );
            let configured = ( "stateContexts" in conf && s.name in conf.stateContexts );
            let titleConf = ( ( "titleExpressions" in conf && s.name in conf.titleExpressions ) ? conf.titleExpressions[ s.name ] : null );
            if( state ) {
                state.resolve.momCtx = exports._localResolver( configured, titleConf );
            }
        } );
        if( conf.i18n ) {
            //collect files name:
            let languagesFileList = {};
            for( let tradId in conf.i18n ) {
                if( Array.isArray( conf.i18n[ tradId ] ) ) {
                    conf.i18n[ tradId ].forEach( function( lFiles ) {
                        languagesFileList[ lFiles ] = true; //
                    } );
                }
            }
            Object.getOwnPropertyNames( languagesFileList ).forEach( function( fileName ) {
                languagesPromises.push( localeSvc.getTextPromise( fileName ).then( function( data ) {
                    exports._languageRepository[ fileName ] = data;
                } ) );
            } );
            exports._i18nPromise = awPromiseSvc.instance.all( languagesPromises ).then( function() {
                exports._languageRepository.i18n = {};
                for( let tradId in conf.i18n ) {
                    if( Array.isArray( conf.i18n[ tradId ] ) ) {
                        let file = conf.i18n[ tradId ][ 0 ];
                        if( file in exports._languageRepository && tradId in exports._languageRepository[ file ] ) {
                            exports._languageRepository.i18n[ tradId ] = exports._languageRepository[ file ][ tradId ];
                        }
                    }
                }
                exports._dataContext.i18n = exports._languageRepository.i18n;
            } );
        } else {
            exports._i18nPromise = awPromiseSvc.instance.when();
        }
        return exports.config;
    } );
};

app.factory( 'momCtxService', () => exports );

export default exports;
