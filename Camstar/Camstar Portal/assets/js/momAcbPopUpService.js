// Copyright (c) 2019 Siemens

/**
 * @module js/momAcbPopUpService
 */
import app from 'app';
import appCtxSvc from 'js/appCtxService';
import popupSvc from 'js/popupService';
import eventBus from 'js/eventBus';

import ngUtils from 'js/ngUtils';
import _ from 'lodash';

var exports = {};

/**
 * Open none dragable popup
 */
export let displayPopup = function() {
    popupSvc.showPopup( 'popupOld' );
};
/**
 * Open dragable popup
 */
export let displayDragablePopup = function() {
    popupSvc.showPopup( 'popupDragableOld' );
};

/**
 * Open no modal popup
 */
export let displayNoModalPopup = function() {
    popupSvc.showPopup( 'popupNoModalOld' );
};

/**
 */
export let effectivityLinkClicked = function( data ) {
    data.currentEffectivity.value = true;
};

/**
 */
export let closePopup = function() {
    eventBus.publish( 'awPopupWidget.close' );
};

export let closePopupWindow = function() {
    eventBus.publish( 'awPopup.close' );
};

export let Remove = function( elementId ) {
    ngUtils.destroyNgElement( document.getElementById( elementId ) );
};

/**
 * None draggable popup method to update the label in context.
 */
export let updateText = function( data ) {
    appCtxSvc.ctx.updatedtext = data.newtext.dbValue;
    eventBus.publish( 'aw.complete' );
};
export let updateTextandDate = function( data ) {
    appCtxSvc.ctx.updatedtext = data.newtext.dbValue;
    appCtxSvc.ctx.updatedDateTime = data.dateTimeDetails.uiValue;
    eventBus.publish( 'aw.complete' );
};

/**
 *  Draggable popup method to update the label in context.
 */
export let updateTextDragable = function( data ) {
    appCtxSvc.ctx.updatedtext = data.newtext.dbValue;
    eventBus.publish( 'aw.complete' );
};

/**
 * Update lable by user provided value
 */
export let displayUpdatedText = function( data ) {
    data.dragInfo.uiValue = appCtxSvc.ctx.updatedtext;
};
export let displayUpdatedTextandDate = function( data ) {
    data.dragInfo.uiValue = appCtxSvc.ctx.updatedtext;
    data.dateTimeDetails.uiValue = appCtxSvc.ctx.updatedDateTime;
};

export let displayPromiseValue = function( data ) {
    let eventData = data.eventData;
    data.log.uiValue = 'popupId: ' + eventData.popupId + ' | popupElement: ' + eventData.popupElement.nodeName;
};

export let processPromiseValue = function( data ) {
    return 'popupId: ' + data.id + ' | popupElement: ' + data.panelEl.nodeName;
};

export let show2 = function( params ) {
    return popupSvc.show( params );
};

export let displayUpdatedTextofList = function( data ) {
    data.box2.uiValue = data.listBox1.dbValue;
};

// avoid intense invoke
let throttledShow = null;
export let showPopup = function( params ) {
    if( !throttledShow ) {
        throttledShow = _.throttle( popupSvc.show, 200 );
    }
    return throttledShow( params );
};

// avoid intense invoke
let throttledShowBalloon = null;
export let showBalloon = function( params ) {
    if( !throttledShowBalloon ) {
        throttledShowBalloon = _.throttle( popupSvc.show, 200 );
    }

    return throttledShowBalloon( params );
};

const getFn = function( eventId, throttle = false ) {
    let fn = popupElement => { eventBus.publish( eventId, { popupId: popupElement.id } ); };
    if( throttle ) { fn = _.throttle( fn, 1000 ); }
    return fn;
};
const hooksMap = {
    open: getFn( 'showcase.openPopup' ),
    close: getFn( 'showcase.closePopup' ),
    // `update` was called too intensive, apply throttle to ease automation test.
    update: getFn( 'showcase.updatePopup', true )
};
export let getHookFunction = function( key ) {
    return hooksMap[ key ];
};

export let invokeMockSOA = function() {
    // eslint-disable-next-line no-console
    console.log( 'CALLED - invokeMockSOA' );
};

export default exports = {
    displayPopup,
    displayDragablePopup,
    displayNoModalPopup,
    effectivityLinkClicked,
    closePopup,
    closePopupWindow,
    Remove,
    updateText,
    updateTextandDate,
    updateTextDragable,
    displayUpdatedText,
    displayUpdatedTextandDate,
    displayPromiseValue,
    processPromiseValue,
    displayUpdatedTextofList,
    showPopup,
    show2,
    getHookFunction,
    invokeMockSOA,
    showBalloon
};
/**
 * @memberof NgServices
 * @member createChangeService
 */
app.factory( 'momAcbPopUpService', () => exports );
