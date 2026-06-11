// Copyright 2020 Siemens AG

/*global
 define
 */

/**
 * Directive to display the secondary workarea.
 *
 * @module js/mom-secondary-workarea.directive
 */
import app from 'app';
import _ from 'lodash';
import eventBus from 'js/eventBus';
import 'js/aw-selection-summary.directive';
import 'js/editHandlerService';
import 'js/appCtxService';

'use strict';

app.directive( 'momSecondaryWorkarea', [ 'editHandlerService', 'appCtxService', function( editHandlerService, appCtxSvc ) {
    return {
        restrict: 'E',
        templateUrl: app.getBaseUrlPath() + '/html/mom-secondary-workarea.directive.html',
        scope: {
            viewBase: '=',
            selected: '=?' //The currently selected model objects
        },
        controller: [ '$scope', function( $scope ) {
            $scope.hasTcSessionData = !_.isUndefined( appCtxSvc.ctx.tcSessionData );
            $scope.momDetails = appCtxSvc.ctx.momDetails;
            $scope.momDisableEmptyComponent = appCtxSvc.ctx.momDisableEmptyComponent;
        } ],
        link: function( $scope, $element ) {
            let sash = $element.prev( '.aw-layout-splitter' );

            if( sash ) {
                $scope.$watch( _.debounce( function updateInvisibleClass() {
                    let width = $element.width();
                    if( width < 300 ) {
                        $element.addClass( "invisible" );
                        sash.addClass( "invisible" );
                    } else {
                        $element.removeClass( "invisible" );
                        sash.removeClass( "invisible" );
                    }
                } ), 250, {
                    leading: false,
                    trailing: true
                } );

            }

            //Set selection source to secondary workarea
            $scope.$on( 'dataProvider.selectionChangeEvent', function( event, data ) {
                data.source = 'secondaryWorkArea';

                // Add secondaryWorkArea.selectionChangeEvent
                eventBus.publish( 'secondaryWorkArea.selectionChangeEvent', {
                    selectionModel: data.selectionModel,
                    dataCtxNode: $scope,
                    dataProvider: data.dataProvider
                } );
            } );
        }
    };
} ] );
