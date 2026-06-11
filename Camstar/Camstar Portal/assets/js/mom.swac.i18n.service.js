// Copyright 2020 Siemens AG
/**
 * This SWAC Service provides a way to set and retrieve the current locale from a SWAC Component
 * @module "js/mom.swac.i18n.service"
 * @name "MOM.UI.I18n"
 * @requires app
 * @requires js/localeService
 */
import app from 'app';
import localeSvc from 'js/localeService';

const exports = {};

/**
 * Sets the current locale.
 * @param {String} code A valid locale code (e.g. en, en_US, fr_FR, zh_CN, etc.).
 */
exports.set = function( code ) {
    localeSvc.setLocale( code );
};

/**
 * Retrieves the current locale code.
 * @returns {String} The current locale code.
 */
exports.get = function() {
    return localeSvc.getLocale();
};

app.factory( 'momSwacI18nService', () => exports );

export default exports;
