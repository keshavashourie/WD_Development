// Copyright 2020 Siemens AG

/*global
 define
 */
/* eslint-disable valid-jsdoc */
/**
 * A module exposing a custom attribute to automatically update a context whenever the value of an expression changes.
 * @module "js/mom-update-ctx-on-change.directive"
 * @requires app
 * @requires "js/appCtxService"
 */
import app from 'app';
import logger from 'js/logger';
import 'js/appCtxService';

'use strict';

/**
 * This custom attribute can be used to automatically update a context according to the value of an expression.
 * The expression can contain any scope variable accessible within the element where this attribute is present.
 * @typedef "mom-update-ctx-on-change"
 * @implements {Attribute}
 * @property {String} mom-update-ctx-on-change A colon-separated string identifying the context to update and the expression to evaluate.
 * @example
 * <caption>The following code snippet shows how to update the <strong>validConfigForm</strong> context whenever the value of the specified expression changes.</caption>
 * <aw-panel-body mom-update-ctx-on-change="validConfigForm: awPanelBody.$dirty && awPanelBody.$valid && conditions.configFormIsValid">
 *   // ...
 * </aw-panel-body>"
 */
app.directive( 'momUpdateCtxOnChange', [ 'appCtxService', function( appCtxService ) {
    return {
        restrict: 'A',
        scope: true,
        link: function( scope, element, attr ) {
            let parts = attr.momUpdateCtxOnChange.split( ':' );
            if( parts.length < 2 ) {
                logger.error( 'MOM UI Kit - mom-update-ctx-change - Invalid context/value pair:' + attr.momUpdateCtxOnChange );
                return;
            }
            let context = parts[ 0 ].trim();
            let expression = parts.slice( 1, parts.length ).join( ':' ).trim();
            scope.$watch( expression, function( newVal ) {
                appCtxService.updateCtx( context, newVal );
            } );
        }
    };
} ] );
