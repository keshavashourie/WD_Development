// Copyright 2020 Siemens AG
/**
 * This SWAC Service provides a way to show confirmation messages from a SWAC Component.
 * @module "js/mom.swac.confirmation.service"
 * @name "MOM.UI.Confirmation"
 * @requires app
 * @requires @swac/container
 * @requires js/NotyModule
 */
import app from 'app';
import SWACKit from '@swac/container';
import notySvc from 'js/NotyModule';
import utils from 'js/mom.utils.service';

const exports = {};
let SWAC = new SWACKit();

/**
 * The configuration of a confirmation button displayed in a confirmation message.
 * @typedef {Object} ConfirmationButton
 * @property {String} text The text to display in the button.
 * @property {String} id The ID that will be returned when the button is pressed
 */
/**
 * Shows a confirmation message. By default, an **OK** button (id: **ok**) and a **Cancel** (id: **cancel**) button are displayed.
 * @param {String} title The title of the message _(currently not processed)_.
 * @param {String} message The message to show. it may contain HTML code.
 * @param {Object} opts Additional options and configuration. At present, only the **buttons** property
 * can be set to an array of [ConfirmationButton](#~ConfirmationButton) objects.
 *
 * The default **buttons** configuration is the following:
 * ```
 * [
 *   {
 *     text: 'OK',
 *     id: 'ok'
 *   },
 *   {
 *     text: 'Cancel',
 *     id: 'cancel'
 *   }
 * ]
 * ```
 * @returns {Promise} A Promise fulfilled with an object containing the the **id** of the button that was pressed.
 *
 * Example: `{buttonId: 'ok'}`
 */
exports.show = function( title, message, opts ) {
    let options = opts || {};
    let defer = new SWAC.Defer();
    let i;
    let totalButtons;

    if( options.buttons ) {
        totalButtons = options.buttons.length;
        for( i = 0; i < totalButtons; i++ ) {
            ( function( index ) {
                options.buttons[ index ].onClick = function( $noty ) {
                    $noty.close();
                    defer.fulfill( { buttonId: options.buttons[ index ].id } );
                };
                options.buttons[ index ].text = options.buttons[ index ].text || options.buttons[ index ].displayName;
                options.buttons[ index ].addClass = "btn btn-notify";
            } )( i );
        }
        notySvc.showWarning( message, options.buttons, options.data );
    } else {
        let buttons = [ {
                "text": "OK",
                "onClick": function( $noty ) {
                    $noty.close();
                    defer.fulfill( { buttonId: 'ok' } );
                },
                "addClass": "btn btn-notify"
            },
            {
                "text": "Cancel",
                "onClick": function( $noty ) {
                    $noty.close();
                    defer.fulfill( { buttonId: 'cancel' } );
                },
                "addClass": "btn btn-notify"
            }
        ];
        notySvc.showWarning( message, buttons, options.data );
    }

    return defer.promise;
};

/**
 * Hides all currently-displayed messages.
 */
exports.hide = function() {
    utils.closeMessages();
};

app.factory( 'momSwacConfirmationService', () => exports );

export default exports;
