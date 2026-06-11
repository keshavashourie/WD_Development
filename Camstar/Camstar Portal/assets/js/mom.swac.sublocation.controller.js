// Copyright 2020 Siemens AG
/**
 * Defines the controller that should be used for all MOM SWAC SubLocation states.
 *
 * @module js/mom.swac.sublocation.controller
 */
import app from 'app';
import ngModule from 'angular';
import logger from 'js/logger';
import eventBus from 'js/eventBus';
import 'js/appCtxService';
import 'js/swac/swac-include.directive';
import 'js/exist-when.directive';
import 'js/aw-base-sublocation.directive';
import 'js/aw.native.sublocation.controller';
import 'js/mom.native.sublocation.controller';
import 'js/mom.swac.compatibility.service';

'use strict';

app.controller( 'MomSwacSubLocationCtrl', [
    '$scope',
    '$state',
    '$controller',
    'momSwacCompatibilityService',
    'appCtxService',
    function( $scope, $state, $controller, momSwacCompatibilityService, appCtxService ) {

        let ctrl = this; //eslint-disable-line consistent-this, no-invalid-this

        ngModule.extend( ctrl, $controller( 'NativeSubLocationCtrl', {
            $scope: $scope
        } ) );

        return momSwacCompatibilityService._init( $state.params.screen ).then( () => {
            const cfg = momSwacCompatibilityService.config;
            $scope.screens = cfg.screens || {};
            $scope.defaultScreen = cfg.default;
            $scope.screen = $state.params.screen || $scope.defaultScreen;

            if( Object.keys( $scope.screens ).length === 0 ) {
                logger.error( 'No SWAC Screens defined in the mom-swac-screens.json configuration file.' );
                return;
            }
            if( !$scope.screen ) {
                logger.error( 'No SWAC Screen to display (no screen parameter specified and no default specified).' );
                return;
            }
            if( cfg.componentUrl ) {
                $scope.source = cfg.componentUrl;
            } else {
                $scope.source = $scope.screens[ $scope.screen ];
            }
            if( !$scope.source ) {
                logger.error( 'Unknown SWAC Screen: ' + $scope.screen );
                return;
            }
            const titles = appCtxService.getCtx( 'location.titles' );
            // Preserve titles!
            $state.current.data.browserSubTitle = titles.browserSubTitle;
            $state.current.data.headerTitle = titles.headerTitle;
            eventBus.publish( 'mom.swac.screen.loadStart', { name: $scope.screen } );
            logger.info( 'Loading SWAC Screen: ' + $scope.screen + ' -> ' + $scope.source );
        } );

    }
] );
