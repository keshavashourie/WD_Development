// Copyright 2020 Siemens AG
/**
 * Defines some polyfills for common methods that may not be availabe on certain browsers.
 *
 * **Note:** The methods exposed by this service are called automatically when the application starts.
 * @module "js/mom.utils"
 * @requires lodash
 */
/* eslint-disable no-extend-native */
/*global
*/
import _ from 'lodash';

/**
 * Defines {@link https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Object/assign|Object.assign()}, if not implemented by the current browser.
 */
export let defineObjectAssign = function() {
    if( !Object.prototype.assign ) {
        // This API isn't supported by IE11
        Object.defineProperty( Object, 'assign', {
            value: function() {
                let result = arguments[ 0 ];
                for( let i = 1; i < arguments.length; i++ ) {
                    result = _.assign( result, arguments[ i ] );
                }
                return result;
            }
        } );
    }
};

/**
 * Defines {@link https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Object/entries|Object.entries()}, if not implemented by the current browser.
 */
export let defineObjectEntries = function() {
    if( !Object.prototype.entries ) {
        // This API isn't supported by IE11
        Object.defineProperty( Object, 'entries', {
            value: function( obj ) {
                return _.entries( obj );
            }
        } );
    }
};

/**
 * Defines {@link https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/String/endsWith|String.prototype.endsWith()}, if not implemented by the current browser.
 */
export let defineStringEndsWith = function() {
    if( !String.prototype.endsWith ) {
        // This API isn't supported by IE11
        Object.defineProperty( String.prototype, 'endsWith', {
            value: function( target, position ) {
                return _.endsWith( this, target, position );
            }
        } );
    }
};

/**
 * Defines {@link https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/String/padEnds|String.prototype.padEnd()}, if not implemented by the current browser.
 */
export let defineStringPadEnd = function() {
    if( !String.prototype.padEnd ) {
        // This API isn't supported by IE11
        Object.defineProperty( String.prototype, 'padEnd', {
            value: function( length, chars ) {
                return _.padEnd( this, length, chars );
            }
        } );
    }
};

/**
 * Defines {@link https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/String/padStart|String.prototype.padStart()}, if not implemented by the current browser.
 */
export let defineStringPadStart = function() {
    if( !String.prototype.padStart ) {
        // This API isn't supported by IE11
        Object.defineProperty( String.prototype, 'padStart', {
            value: function( length, chars ) {
                return _.padStart( this, length, chars );
            }
        } );
    }
};

/**
 * Defines {@link https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/String/repeat|String.prototype.repeat()}, if not implemented by the current browser.
 */
export let defineStringRepeat = function() {
    if( !String.prototype.repeat ) {
        // This API isn't supported by IE11
        Object.defineProperty( String.prototype, 'repeat', {
            value: function( count ) {
                return _.repeat( this, count );
            }
        } );
    }
};

/**
 * Defines {@link https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/String/trimEnd|String.prototype.trimEnd()}, if not implemented by the current browser.
 */
export let defineStringTrimEnd = function() {
    if( !String.prototype.trimEnd ) {
        // This API isn't supported by IE11
        Object.defineProperty( String.prototype, 'trimEnd', {
            value: function() {
                return _.trimEnd( this );
            }
        } );
    }
};

/**
 * Defines {@link https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/String/trimStart|String.prototype.trimStart()}, if not implemented by the current browser.
 */
export let defineStringTrimStart = function() {
    if( !String.prototype.trimStart ) {
        // This API isn't supported by IE11
        Object.defineProperty( String.prototype, 'trimStart', {
            value: function() {
                return _.trimStart( this );
            }
        } );
    }
};

defineObjectAssign();
defineObjectEntries();

defineStringEndsWith();
defineStringPadEnd();
defineStringPadStart();
defineStringRepeat();
defineStringTrimEnd();
defineStringTrimStart();

const exports = {
    defineObjectAssign,
    defineObjectEntries,
    defineStringEndsWith,
    defineStringPadEnd,
    defineStringPadStart,
    defineStringRepeat,
    defineStringTrimEnd,
    defineStringTrimStart
};

export default exports;
