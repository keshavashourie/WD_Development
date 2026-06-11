// Copyright 2020 Siemens AG
/**
 * This SWAC Service provides a way to set and retrieve the current theme from a SWAC Component
 * @module "js/mom.swac.theme.service"
 * @name "MOM.UI.Theme"
 * @requires app
 * @requires js/theme.service
 */
import app from 'app';
import themeSvc from 'js/theme.service';

const exports = {};

/**
 * Sets the current theme.
 * @param {String} theme The theme to apply. It can be set to **ui-lightTheme* or **ui-darkTheme**.
 */
exports.set = function( theme ) {
    if( theme.match( /dark/ ) ) {
        themeSvc.setTheme( 'ui-darkTheme' );
    } else {
        themeSvc.setTheme( 'ui-LightTheme' );
    }
};

/**
 * Retrieves the ID of the current theme.
 * @returns {String} The ID of the current theme (**ui-lightTheme** or **ui-darkTheme**).
 */
exports.get = function() {
    return themeSvc.getTheme();
};

app.factory( 'momSwacThemeService', () => exports );

export default exports;
