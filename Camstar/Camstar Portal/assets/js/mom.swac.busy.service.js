// Copyright 2020 Siemens AG
/**
 * This SWAC Service provides a way to show and hide a modal busy indicator from a SWAC Component.
 * @module "js/mom.swac.busy.service"
 * @name "MOM.UI.Busy"
 * @requires app
 * @requires js/eventBus
 */
import app from 'app';
import eventBus from 'js/eventBus';

'use strict';
const exports = {};

/**
 * Shows a modal busy indicator.
 */
exports.show = function() {
    eventBus.publish( 'modal.progress.start' );
};

/**
 * Hides a modal busy indicator.
 */
exports.hide = function() {
    eventBus.publish( 'modal.progress.end' );
};

app.factory( 'momSwacBusyService', () => exports );

export default exports;
