// Copyright 2020 Siemens AG
/**
 * This SWAC Service provides a way to show error messages from a SWAC Component.
 * @module "js/mom.swac.error.service"
 * @name "MOM.UI.Error"
 * @requires app
 * @requires js/messagingService
 */
import app from 'app';
import messagingSvc from 'js/messagingService';
import utils from 'js/mom.utils.service';

const exports = {};

/**
 * Shows an error message.
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
    messagingSvc.showError( message, null, null, buttons );
};

/**
 * Hides all currently-displayed messages.
 */
exports.hide = function() {
    utils.closeMessages();
};

app.factory( 'momSwacErrorService', () => exports );

export default exports;
