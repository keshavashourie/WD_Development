// Copyright 2020 Siemens AG
/**
 * This SWAC Service provides a way to show warning messages from a SWAC Component.
 * @module "js/mom.swac.warning.service"
 * @name "MOM.UI.Warning"
 * @requires app
 * @requires js/messagingService
 */
import app from 'app';
import messagingSvc from 'js/messagingService';
import momUtilsSvc from 'js/mom.utils.service';

const exports = {};

/**
 * Shows an warning message.
 * @param {String} title The title of the message _(currently not processed)_.
 * @param {String} message The message to show. it may contain HTML code.
 * @param {Object} opts Additional options and configuration. At present, only the **buttons** property
 * can be set to an array of [ConfirmationButton](module-_MOM.UI.Confirmation_.html#~ConfirmationButton) objects.
 */

exports.show = function( title, message, opts ) {
    const options = opts || {};
    options.buttons = options.buttons || [];
    const buttons = options.buttons.map( btn => {
        return {
            id: btn.id,
            text: btn.text,
            addClass: "btn btn-notify",
            onClick: ( $noty ) => {
                $noty.close();
                return Promise.resolve( { buttonId: btn.id } );
            }
        };
    } );
    messagingSvc.showWarning( message, buttons );
};

/**
 * Hides all currently-displayed messages.
 */
exports.hide = function() {
    momUtilsSvc.closeMessages();
};

app.factory( 'momSwacWarningService', () => exports );

export default exports;
