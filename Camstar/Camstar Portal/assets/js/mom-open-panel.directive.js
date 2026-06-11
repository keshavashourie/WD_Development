// Copyright 2020 Siemens AG

/*global
 define
 */
/* eslint-disable valid-jsdoc */
/**
 * A module exposing a simple custom attribute to open a side panel.
 * @module "js/mom-open-panel.directive"
 * @requires app
 * @requires "js/commandPanel.service"
 */
import app from 'app';
import 'js/commandPanel.service';

'use strict';

/**
 * A simple custom attribute that can be used to open a side panel.
 * @typedef "mom-open-panel"
 * @implements {Attribute}
 * @property {String} command The name of the View to load in the specified panel.
 * @property {String} location The command bar anchor where the panel will be opened. Set it to **aw_navigation** (left panel) or **aw_toolsAndInfo** (right panel).
 * @example
 * <caption>The following code snippet shows how to load the <strong>cmdAddNoteView</strong> View in the right side panel:</caption>
 * <a mom-open-panel command="cmdAddNote" location="aw_toolsAndInfo">
 *   <aw-i18n>create a new note</aw-i18n>
 * </a>
 */
app.directive( 'momOpenPanel', [ 'commandPanelService', function( cmdPanel ) {
    return {
        restrict: 'A',
        scope: {
            command: '@',
            location: '@'
        },
        link: function( scope, element, attr ) {
            element.on( 'click', function() {
                cmdPanel.activateCommandPanel( attr.command, attr.location );
            } );
        }
    };
} ] );
