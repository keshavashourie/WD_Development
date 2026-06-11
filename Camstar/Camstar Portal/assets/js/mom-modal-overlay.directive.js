// Copyright 2020 Siemens AG

/*global
 define
 */
/* eslint-disable valid-jsdoc */
/**
 * A module exposing a custom element used to display a modal progress indicator.
 * @module "js/mom-modal-overlay.directive"
 * @ignore
 */
import app from 'app';
import eventBus from 'js/eventBus';

'use strict';

/**
 * A custom element used to display a modal progress indicator.
 * > **Note:** This element is already included in all location/sublocation templates and does not need to be added to views.
 * @typedef "mom-modal-overlay"
 * @implements {Element}
 * @example
 * <mom-modal-overlay
 *   start-event="modal.progress.start"
 *   end-event="modal.progress.end"
 *   content-class="mom-modal-overlay-spinner"></mom-modal-overlay>
 */
app.directive( 'momModalOverlay', [ '$timeout', //
    function( $timeout ) {
        return {
            restrict: 'E',
            scope: {
                startEvent: '@',
                endEvent: '@',
                contentClass: '@'
            },
            templateUrl: app.getBaseUrlPath() + '/html/mom-modal-overlay.directive.html',
            link: function( $scope, $element ) {

                if( $scope.contentClass.match( /spinner/ ) ) {
                    // Create canvas spinner

                    let d = 120;
                    let canvas = document.createElement( 'canvas' );
                    canvas.width = d;
                    canvas.height = d;
                    let ctx = canvas.getContext( '2d' );
                    let opacity;

                    $element[ 0 ].children[ 0 ].children[ 0 ].appendChild( canvas );

                    ctx.translate( d / 2, d / 2 );
                    ctx.rotate( Math.PI * 360 / 360 );
                    ctx.lineWidth = Math.ceil( d / 50 );
                    ctx.lineCap = 'square';
                    for( var i = 0; i <= 360; i++ ) {
                        ctx.save();

                        ctx.rotate( ( Math.PI * i / 180 ) );
                        ctx.beginPath();
                        ctx.moveTo( 0, 0 );
                        opacity = ( 360 - ( i * 0.95 ) ) / 360;
                        ctx.strokeStyle = 'rgba(255,255,255,' + opacity.toFixed( 2 ) + ')';
                        ctx.lineTo( 0, d + 30 );
                        ctx.stroke();
                        ctx.closePath();
                        ctx.restore();
                    }
                    ctx.globalCompositeOperation = 'source-out';
                    ctx.beginPath();
                    ctx.arc( 0, 0, d / 2, 2 * Math.PI, false );
                    ctx.fillStyle = '#3296b9';
                    ctx.fill();
                    ctx.globalCompositeOperation = 'destination-out';
                    ctx.beginPath();
                    ctx.arc( 0, 0, ( d / 2 ) * 0.9, 2 * Math.PI, false );
                    ctx.fill();
                }

                /**
                 * state of progress indicator
                 */
                let state = false;

                /**
                 * Ref count of progress event, only stop progress bar if ref count is zero
                 */
                let progressRefCount = 0;

                let api = {};
                $scope.showProgressIndicator = state;
                let _animationWaitTime = 500; //Expose as parameter

                api.toggleProgressState = function( show ) {
                    /**
                     * Time to wait before starting the animation
                     */
                    if( show ) {
                        api.animationWaitTimer = $timeout( function() {
                            // And then check to make sure there are operations running and
                            // the indicator is not already activated before starting it
                            if( state !== progressRefCount > 0 ) {
                                state = progressRefCount > 0;

                                $scope.$evalAsync( function() {
                                    $scope.showProgressIndicator = state;
                                } );
                            }
                        }, _animationWaitTime );
                    } else {
                        // Don't toggle showProgressIndicator if the state is already correct
                        if( state !== progressRefCount > 0 ) {
                            state = progressRefCount > 0;

                            $scope.$evalAsync( function() {
                                $scope.showProgressIndicator = state;
                                $element[ 0 ].children[ 0 ].style.cssText = '';
                                _animationWaitTime = 500;
                            } );
                        }
                    }
                };

                //Show / hide the progress indicator depending on network activity
                api.progressStartListener = eventBus.subscribe( $scope.startEvent, function( data ) {
                    progressRefCount++;
                    if( data ) {
                        if( data.style ) {
                            for( let prop in data.style ) {
                                $element[ 0 ].children[ 0 ].style[ prop ] = data.style[ prop ];
                            }
                        }
                        if( typeof data.animationWaitTime === 'number' ) {
                            _animationWaitTime = data.animationWaitTime;
                        }
                    }
                    api.toggleProgressState( true );
                } );

                api.progressStopListener = eventBus.subscribe( $scope.endEvent, function() {
                    progressRefCount--;

                    if( progressRefCount < 0 ) {
                        progressRefCount = 0;
                    }

                    if( progressRefCount === 0 ) {
                        api.toggleProgressState( false );
                    }
                } );

                $scope.$on( '$destroy', function() {
                    eventBus.unsubscribe( api.progressStartListener );
                    eventBus.unsubscribe( api.progressStopListener );

                    if( api.animationWaitTimer ) {
                        $timeout.cancel( api.animationWaitTimer );
                    }
                } );
            }
        };
    }
] );
