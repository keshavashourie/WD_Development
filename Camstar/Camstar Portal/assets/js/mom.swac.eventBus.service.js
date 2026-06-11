// Copyright 2020 Siemens AG
/**
 * This SWAC Service provides a way to publish and subscribe to events on the Siemens Web Framework eventBus service from a SWAC Component.
 *
 * Example:
 *
 * ```
 * // After retrieving the SWAC service, register the topics you want to subscribe to.
 * // If you are using wildcards in topic IDs, do not register for specific topics already matched by the
 * // wildcard or you will receive the same event twice.
 * eventBusSvc.register('appCtx.*');
 * eventBusSvc.register('log');
 *
 * // Configure as many subscription on the event property exposed by the service
 * // but remember to filter by topic as needed.
 * eventBusSvc.event.subscribe(function(event) {
 *   if (event.data.topic === 'appCtx.register') {
 *     console.log("New context registered", event.data.data.value);
 *   }
 * });
 *
 * // To publish event on the Siemens Web Framework Container eventBus, use the publish method as you would with
 * // the standard eventBus
 * eventBusSvc.publish('my.custom.event', {
 *   success: true
 * });
 * ```
 * @module "js/mom.swac.eventBus.service"
 * @name "MOM.UI.EventBus"
 * @requires js/eventBus
 * @requires @swac/container
 */
import eventBus from 'js/eventBus';
import SWACKit from '@swac/container';

export let SwacEventBus = function() {
    let SWAC = new SWACKit();
    let self = this; //eslint-disable-line consistent-this
    self._swacBus = new SWAC.Eventing.Publisher( 'MOM.UI.EventBus' );
    self._subscriptions = {};
    /**
     * @member {SWAC.Eventing.Event} event A SWAC Event object that can be used to subscribe to registered eventBus topics.
     *
     * To subscribe to a previously-registered eventBus event, call the **subscribe** method of this object passing a callback that takes a
     * single object as parameter. When an event is received, the actual eventBus event topic and data willbe wrapped in a SWAC Event payload,
     * so an object containing a **data** and a **type** property similar to the following will be passed to the callback:
     *
     *  ```
     *  {
     *    data: {
     *      channel: 'soajs',
     *      data: {
     *        name: 'momPrimaryNavigationPanel',
     *        value: 'sampleSwacScreens'
     *      },
     *      timestamp: "2018-11-30T14:39:44.066Z",
     *      topic: 'appCtx.register'
     *    },
     *    type: 'event'
     *  }
     *  ```
     * @static
     */
    self.event = self._swacBus.event;

    /**
     * Publishes an event on the Siemens Web Framework eventBus.
     * @method publish
     * @static
     * @param {String} topic The name (topic) of the event.
     * @param {Object} eventData The payload of the event.
     */
    self.publish = function( topic, eventData ) {
        eventBus.publish( topic, eventData );
    };

    /**
     * Registers a topic so that it will be possible to subscribe to it.
     * @method register
     * @static
     * @param {String} topic The name of a valid eventBus topic.
     */
    self.register = function( topic ) {
        if( !self._subscriptions[ topic ] ) {
            self._subscriptions[ topic ] = eventBus.subscribe( topic, function( data, envelope ) {
                self._swacBus.notify( envelope );
            } );
        }
    };

    /**
     * Unregisters a topic so that the corresponding eventBus events will no longer be received by the SWAC Component.
     * @method unregister
     * @static
     * @param {String} topic The name of a valid eventBus topic that was previously registered.
     */
    self.unregister = function( topic ) {
        if( self._subscriptions[ topic ] ) {
            eventBus.unsubscribe( self._subscriptions[ topic ] );
        }
    };
};
export default SwacEventBus;
