// Copyright 2020 Siemens AG

/* global define */

/**
 * Defines the controller that should be used for all MOM SubLocation states.
 *
 * @module js/mom.native.sublocation.controller
 */
import app from 'app';
import ngModule from 'angular';
import _ from 'lodash';
import eventBus from 'js/eventBus';
import 'js/logger';
import 'js/aw-include.directive';
import 'js/aw-layout-slot.directive';
import 'js/aw.native.sublocation.controller';
import 'js/mom-secondary-workarea.directive';
import 'js/appCtxService';

'use strict';

app.controller( 'MomNativeSubLocationCtrl', [
    '$scope',
    '$q',
    '$state',
    '$controller',
    'appCtxService',
    function( $scope, $q, $state, $controller, appCtxSvc ) {

        let ctrl = this; //eslint-disable-line consistent-this, no-invalid-this

        ngModule.extend( ctrl, $controller( 'NativeSubLocationCtrl', {
            $scope: $scope
        } ) );

        let ctxSubscription = eventBus.subscribe( "appCtx.*", function( data ) {
            if( data.name === 'ViewModeContext' ) {
                // Make ViewMode semi-persistent when navigating/reloading the same state.
                if( data.value && data.value.ViewModeContext && data.value.ViewModeContext !== 'None' ) {
                    appCtxSvc.registerCtx( 'preferences.AW_SubLocation_Generic_ViewMode', [ data.value.ViewModeContext ] );
                }
            }
        } );

        $scope.$on( '$destroy', function() {
            eventBus.unsubscribe( ctxSubscription );
        } );

        // REFACTORING - COPIED from default.location
        if( $state.current.templateUrl.match( /\/mom.details.sublocation.html$/ ) ) {
            appCtxSvc.registerCtx( 'momDetails', true );
        } else {
            appCtxSvc.registerCtx( 'momDetails', false );
        }

        if( $scope.breadcrumbConfig ) {
            $scope.breadcrumbConfig.vm = $scope.breadcrumbConfig.vm || 'momNavigateBreadcrumb';
            $scope.breadcrumbConfig.crumbDataProvider = $scope.breadcrumbConfig.crumbDataProvider || 'momBreadcrumbDataProvider';
        }
    }
] );
