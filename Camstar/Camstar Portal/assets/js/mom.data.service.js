// Copyright 2020 Siemens AG
/**
 * Provides a set of utility methods that can be used to manipulate data and produce ViewModelProperty objects.
 * @module "js/mom.data.service"
 * @requires app
 * @requires jquery
 * @requires lodash
 * @requires js/declUtils
 * @requires js/uwPropertyService
 * @requires js/dateTimeService
 */
/* eslint-disable valid-jsdoc */
/*global
 */
import app from 'app';
import $ from 'jquery';
import _ from 'lodash';
import uwPropertySvc from 'js/uwPropertyService';
import dateTimeSvc from 'js/dateTimeService';

'use strict';
const exports = {};

let innerReduceFunction = function( acc, curr ) {
    let currentDataType = getScalarDataType( curr );
    if( acc === currentDataType ) {
        return acc;
    }
    if( acc === 'INTEGER' && currentDataType === 'DOUBLE' || acc === 'DOUBLE' && currentDataType === 'INTEGER' ) {
        return 'DOUBLE';
    }
    return 'OBJECT';
};

let getScalarDataType = function( value ) {
    let dataType = 'OBJECT';
    if( typeof value === typeof '' ) {
        //TODO: ensure that char is really used, may be better to return always string?
        dataType = 'STRING';
    } else if( typeof value === typeof true ) {
        dataType = 'BOOLEAN';
    } else if( Object.prototype.toString.call( value ) === '[object Date]' && Boolean( Date.parse( value ) ) ) {
        //TODO: when data belongs to "DATETIME" type?
        dataType = "DATE";
    } else if( $.isNumeric( value ) ) {
        if( Number.isInteger( value ) ) {
            dataType = "INTEGER";
        } else {
            dataType = "DOUBLE";
        }
    }
    return dataType;
};

let getPropertyValues = function( propType, propVal, propDispVal ) {
    let objToReturn = {
        value: null,
        displayValue: null
    };
    switch ( propType ) {
        case 'DATE': {
            objToReturn.value = new Date( propVal ).getTime();
            objToReturn.displayValue = dateTimeSvc.formatDate( new Date( propDispVal ).getTime() );
            break;
        }
        case 'DATETIME': {
            objToReturn.value = new Date( propVal ).getTime();
            objToReturn.displayValue = dateTimeSvc.formatSessionDateTime( new Date( propDispVal ).getTime() );
            break;
        }
        case 'INTEGER':
        case 'DOUBLE': {
            objToReturn.value = new Number( propVal );

            if( _.isString( propDispVal ) ) {
                objToReturn.displayValue = propDispVal;
            } else {
                objToReturn.displayValue = new String( propDispVal );
            }

            break;
        }
        default: {
            objToReturn.value = propVal;
            objToReturn.displayValue = propDispVal;
            break;
        }
    }
    return objToReturn;
};

/**
 *  Determines the type of **value** and returns its Siemens Web Framework type.
 *  @param {*} value - The argument wich type has to be detected. It could be either a scalar or vector.
 *  @returns {String } A type identifier recognized by Siemens Web Framework.
 */
exports.getType = function( value ) {
    // These are all the types found in the Siemens Web Framework source code, some of them will be not managed...
    // "OBJECT", "STRING", "CHAR", "BOOLEAN", "DATE", <<"DATETIME">>, <<"FLOAT">>, "INTEGER", "DOUBLE"
    // "DATEARRAY", "INTEGERARRAY", "DOUBLEARRAY", "BOOLEANARRAY", "OBJECTARRAY"
    if( Array.isArray( value ) ) {
        if( value.length === 0 ) {
            throw new RangeError( "When the value is an array, it has to contain at least an item." );
        }
        return value.reduce( innerReduceFunction, getScalarDataType( value[ 0 ] ) ) + 'ARRAY';
    }
    return getScalarDataType( value );
};

/* eslint-disable complexity */
/**
 * Creates a ViewModelProperty object.
 *
 * The following fields have some *smart defaults*:
 *
 * * **propertyDisplayName** &mdash; Defaults to the **name** of the property.
 * * **uiValue** &mdash; Defaults to the **value** of the property.
 * * **type** &mdash; Inferred automatically by calling the [getType](#.getType) method.
 * * **isArray** &mdash; Set to **true** if the value is an array.
 * * **isEnabled** &mdash; Set to **true** by default.
 * * **isEditable** &mdash; Set to **true** by default.
 * * **isPropertyModifiable** &mdash; Set to **false** by default.
 * * **isRequired** &mdash; Set to **false** by default.
 *
 * > **Tip:** For more informations on what fields can be configured for ViewModelProperty objects, see the [ViewModelProperty Object Reference](https://gitlab.industrysoftware.automation.siemens.com/mom/mom-ui/wikis/ViewModelProperty-object-reference) page on the MOM UI wiki.
 * @param {String} name The name/identifier of the property.
 * @param {*} value The value of the property.
 * @param {Object} def An object containing additional ViewModelProperty fields to be added to the property.
 * @param {DeclViewModel} vm A reference to the current ViewModel (necessary only if the **dataProvider** field is specified).
 * @returns {ViewModelProperty} A valid ViewModelProperty object.
 */
exports.vmProp = function( name, value, def, vm ) {
    let definition = def || {};
    let propName = name;
    let propDisplayName = definition.propertyDisplayName || definition.displayName || definition.name || name;
    let val = value || definition.dbValue || definition.value;
    let displayVal = definition.uiValue || definition.displayValue || new String( val );
    let modifiable = definition.isPropertyModifiable || definition.isModifiable;
    let propType = definition.type || exports.getType( value );
    let values = getPropertyValues( propType, val, displayVal );

    if( definition.isArray === true ) {
        values.value = _.isArray( values.value ) ? values.value : [ values.value ];
    }

    values.displayValue = _.isArray( values.displayValue ) ? values.displayValue : [ values.displayValue ];

    let prop = uwPropertySvc.createViewModelProperty( propName, propDisplayName, propType, values.value, values.displayValue );

    if( propType === 'DATE' ) {
        prop.dateApi = prop.dateApi || {};
        prop.dateApi.isDateEnabled = true;
        prop.dateApi.isTimeEnabled = false;
    } else if( propType === 'DATETIME' ) {
        prop.dateApi = prop.dateApi || {};
        prop.dateApi.isDateEnabled = true;
        prop.dateApi.isTimeEnabled = true;
    }

    uwPropertySvc.setHasLov( prop, definition.hasLOV === true );
    uwPropertySvc.setIsArray( prop, val instanceof Array || definition.isArray === true );
    uwPropertySvc.setIsRequired( prop, definition.isRequired === true );

    if( definition.renderingHint ) {
        uwPropertySvc.setRenderingHint( prop, definition.renderingHint );
    }

    let isEnabled = _.isUndefined( definition.isEnabled ) ? true : !( definition.isEnabled === false );
    uwPropertySvc.setIsEnabled( prop, isEnabled );

    let maxLength = _.isUndefined( definition.maxLength ) ? 0 : definition.maxLength;
    uwPropertySvc.setLength( prop, maxLength );

    let isEditable = _.isUndefined( definition.isEditable ) ? false : definition.isEditable;
    uwPropertySvc.setIsEditable( prop, isEditable );

    let isModifiable = _.isUndefined( modifiable ) ? true : modifiable;
    uwPropertySvc.setIsPropertyModifiable( prop, isModifiable );

    if( definition.dataProvider ) {
        prop.dataProvider = definition.dataProvider;
        uwPropertySvc.setHasLov( prop, true );
        prop.emptyLOVEntry = _.isUndefined( definition.emptyLOVEntry ) ? true : definition.emptyLOVEntry;
        prop.isSelectOnly = definition.isSelectOnly;
        prop.getViewModel = function() {
            return vm;
        };
    }

    // Set all properties specified via the definition (overriding defaults)
    Object.keys( definition ).forEach( function( field ) {
        prop[ field ] = definition[ field ];
    } );

    return prop;
};

/**
 * Converts an object **obj** into a dictionary of ViewModelProperty objects by applying the property definition object **def** to each
 *  value, and automatically setting each property name to the corresponding object key.
 * @param {Object} obj The object to convert.
 * @param {Object} def An object containing additional ViewModelProperty fields to be added to each property.
 * @param {DeclViewModel} vm A reference to the current ViewModel (necessary only if the **dataProvider** field is specified).
 * @returns {Object.<string, ViewModelProperty>} A dictionary of ViewModelProperty objects.
 */
exports.vmPropObj = function( obj, def, vm ) {
    let result = {};
    Object.keys( obj ).forEach( function( key ) {
        result[ key ] = exports.vmProp( key, obj[ key ], def || {}, vm );
    } );
    return result;
};

/**
 * Sets the edit state of ViewModelProperty. If the property is editable and editable in view model then the
 * **isEditable** flag is set to true which shows the properties as editable.
 *
 * @param {ViewModelProperty} vmProp A ViewModelProperty object that will be updated.
 * @param {Boolean} editable Sets edit state of ViewModelProperty.
 * @param {Boolean} override TRUE if the editing state should be updated an announced even if not currently
 *            different than the desired state.
 */
exports.setEditState = function( vmProp, editable, override ) {
    return uwPropertySvc.setEditState( vmProp, editable, override );
};

/**
 * Resets the value of a property to its original value.
 *
 * @param {ViewModelProperty} vmProp A ViewModelProperty object that will be updated.
 */
exports.resetUpdates = function( vmProp ) {
    return uwPropertySvc.resetUpdates( vmProp );
};

/**
 * Set the **error** field of a ViewModelProperty object and notifies changes.
 *
 * @param {ViewModelProperty} vmProp A ViewModelProperty object that will be updated.
 * @param {String} error - The message that should be displayed when some aspect of the property's value is not
 *            correct. This value must be 'null' or an empty string to not have the error be displayed.
 */
exports.setError = function( vmProp, error ) {
    return uwPropertySvc.setError( vmProp, error );
};

/**
 * Set the value of a ViewModelProperty object along with all related fields, and notifies changes.
 *
 * @param {ViewModelProperty} vmProp A ViewModelProperty object that will be updated.
 * @param {*} value The new value of the property.
 * @returns {ViewModelProperty} The input ViewModelProperty object with the new value set.
 */
exports.setValue = function( vmProp, value ) {
    return uwPropertySvc.setValue( vmProp, value );
};

/**
 * Iterates over the keys of the **values** object and sets each value as te new value of the ViewModelProperty object stored
 * at the corresponding key of the **obj** object.
 * @param {Object.<string, ViewModelProperty>} obj A dictionary of ViewModelProperty objects.
 * @param {Object.<string, *>} values A dictionary of values to set for **obj** properties.
 * @returns {Object.<string, ViewModelProperty>} The input dictionary of ViewModelProperty objects with the new values set.
 */
exports.setObjectValues = function( obj, values ) {
    Object.keys( values ).forEach( function( key ) {
        uwPropertySvc.setValue( obj[ key ], values[ key ] );
    } );
    return obj;
};

app.factory( 'momDataService', () => exports );

export default exports;
