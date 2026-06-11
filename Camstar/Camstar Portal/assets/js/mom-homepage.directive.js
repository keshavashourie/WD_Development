// Copyright 2020 Siemens AG
/*global
 define
 */
/* eslint-disable valid-jsdoc */
/**
 * A module exposing a custom element for a home page container.
 * @module "js/mom-homepage.directive"
 */
import app from 'app';
import 'js/aw-scrollpanel.directive';
import 'js/aw-row.directive';
import 'js/aw-command-bar.directive';
import 'js/appCtxService';

'use strict';

/**
 * A custom element for a home page container.
 * @typedef "mom-homepage"
 * @implements {Element}
 */
app.directive( 'momHomepage', [ 'appCtxService', '$timeout', function( appCtxService, $timeout ) {
    return {
        restrict: 'E',
        scope: {},
        transclude: true,
        link: function( scope, element ) {
            element[0].classList += ' mom-homepage';
            // Detect scrolling
            let manageScrolling = function( data ) {
                if( data.currentTarget.scrollTop > 100 && !appCtxService.ctx.momHomePageScrolledDown ) {
                    $timeout( function() {
                        appCtxService.registerCtx( 'momHomePageScrolledDown', true );
                    }, 10 );
                } else if( data.currentTarget.scrollTop < 100 && appCtxService.ctx.momHomePageScrolledDown ) {
                    $timeout( function() {
                        appCtxService.registerCtx( 'momHomePageScrolledDown', false );
                    }, 10 );
                }
            };
            let container = element.find( '.aw-layout-row > .mom-homepage-container' )[ 0 ];
            container.addEventListener( 'scroll', manageScrolling, false );
            scope.$on( '$destroy', function() {
                container.removeEventListener( 'scroll', manageScrolling );
            } );
        },
        controller: [ function() {} ],
        templateUrl: app.getBaseUrlPath() + '/html/mom-homepage.html'
    };
} ] );
