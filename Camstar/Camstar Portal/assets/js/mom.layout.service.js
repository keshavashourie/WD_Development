// Copyright 2020 Siemens AG
/**
 * @module "js/mom.layout.service"
 */
import app from 'app';
import _ from 'lodash';
import appCtxSvc from 'js/appCtxService';
import cfgSvc from 'js/configurationService';
import conditionSvc from 'js/conditionService';
import utils from 'js/mom.utils.service';
import awRootScopeSvc from 'js/awRootScopeService';
import awStateSvc from 'js/awStateService';

const exports = {};

let initialized = false;

exports.init = function() {
    if( !initialized ) {
        exports._evaluateHeaderContributions();
        initialized = true;
    }
};

exports.setMomPageEntity = function() {
    appCtxSvc.registerCtx( 'momPageEntity', ( awStateSvc.instance.current.data && awStateSvc.instance.current.data.momPageEntity || appCtxSvc.getCtx( 'momPageEntity' ) ) );
};

exports.setHeaderViewModel = function( value ) {
    let momPageEntity = value || {};
    let headerViewModel;
    if( momPageEntity.type ) {
        headerViewModel = {
            type: momPageEntity.type,
            props: {},
            typeIconURL: utils.typeIconPath( momPageEntity.type )
        };
    }
    return { headerViewModel: headerViewModel };
};

exports.toggleCommandLabels = function( value ) {
    let mainView = document.getElementById( 'main-view' );
    if( mainView ) {
        if( value ) {
            mainView.classList.add( 'aw-commands-showIconLabel' );
        } else {
            mainView.classList.remove( 'aw-commands-showIconLabel' );
        }
    }
};

exports._evaluateHeaderContributions = function() {
    return cfgSvc.getCfg( 'headerContributions' ).then(
        function( contributedHeaders ) {
            contributedHeaders = _.sortBy( contributedHeaders, [ function( o ) { return o.priority; } ] );
            contributedHeaders.reverse();

            awRootScopeSvc.instance.$watch( function _watchHeaderContributionVisibility() {
                for( let indx in contributedHeaders ) {
                    if( contributedHeaders[ indx ].visibleWhen ) {
                        let condition = conditionSvc.evaluateCondition( {
                            ctx: appCtxSvc.ctx
                        }, contributedHeaders[ indx ].visibleWhen );

                        if( condition ) {
                            return contributedHeaders[ indx ];
                        }
                    }
                }
                return null;
            }, function( activeHeader ) {
                if( activeHeader ) {
                    awRootScopeSvc.instance.$evalAsync( function() {
                        appCtxSvc.registerCtx( 'momHeaderContributedView', activeHeader.view );
                    } );
                } else {
                    awRootScopeSvc.instance.$evalAsync( function() {
                        appCtxSvc.registerCtx( 'momHeaderContributedView', null );
                    } );
                }
            } );
        } );
};

app.factory( 'momLayoutService', () => exports );

export default exports;
