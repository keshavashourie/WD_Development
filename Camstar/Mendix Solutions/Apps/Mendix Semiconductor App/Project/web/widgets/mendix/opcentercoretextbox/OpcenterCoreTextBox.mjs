import { useState, useRef, createElement, useEffect } from 'react';
import Big from 'big.js';

function getDefaultExportFromCjs (x) {
	return x && x.__esModule && Object.prototype.hasOwnProperty.call(x, 'default') ? x['default'] : x;
}

var classnames = {exports: {}};

/*!
	Copyright (c) 2018 Jed Watson.
	Licensed under the MIT License (MIT), see
	http://jedwatson.github.io/classnames
*/

var hasRequiredClassnames;

function requireClassnames () {
	if (hasRequiredClassnames) return classnames.exports;
	hasRequiredClassnames = 1;
	(function (module) {
		/* global define */

		(function () {

		  var hasOwn = {}.hasOwnProperty;
		  function classNames() {
		    var classes = '';
		    for (var i = 0; i < arguments.length; i++) {
		      var arg = arguments[i];
		      if (arg) {
		        classes = appendClass(classes, parseValue(arg));
		      }
		    }
		    return classes;
		  }
		  function parseValue(arg) {
		    if (typeof arg === 'string' || typeof arg === 'number') {
		      return arg;
		    }
		    if (typeof arg !== 'object') {
		      return '';
		    }
		    if (Array.isArray(arg)) {
		      return classNames.apply(null, arg);
		    }
		    if (arg.toString !== Object.prototype.toString && !arg.toString.toString().includes('[native code]')) {
		      return arg.toString();
		    }
		    var classes = '';
		    for (var key in arg) {
		      if (hasOwn.call(arg, key) && arg[key]) {
		        classes = appendClass(classes, key);
		      }
		    }
		    return classes;
		  }
		  function appendClass(value, newClass) {
		    if (!newClass) {
		      return value;
		    }
		    if (value) {
		      return value + ' ' + newClass;
		    }
		    return value + newClass;
		  }
		  if (module.exports) {
		    classNames.default = classNames;
		    module.exports = classNames;
		  } else {
		    window.classNames = classNames;
		  }
		})(); 
	} (classnames));
	return classnames.exports;
}

var classnamesExports = requireClassnames();
var classNames = /*@__PURE__*/getDefaultExportFromCjs(classnamesExports);

var placeHolder = '';
const CustomInputMask = (props) => {
    const mask = props.mask ?? '';
    const placeholderChar = props.placeHolderChar ?? '_';
    placeHolder = props.placeHolderInput ?? '';
    const [value, setValue] = useState(props.value ?? '');
    const inputRef = useRef(null);
    const maskRules = {
        '9': /\d/,
        'Z': /[A-Za-z]/,
        'U': /[A-Z]/,
        'L': /[a-z]/,
        '*': /[A-Za-z0-9]/
    };
    const cleanValue = (input) => {
        return input.replace(/[^0-9a-zA-Z]/g, '');
    };
    const applyMask = (input) => {
        let cleanInput = cleanValue(input);
        let maskedValue = '';
        let inputIndex = 0;
        for (let i = 0; i < mask.length; i++) {
            const maskChar = mask[i];
            if (maskRules[maskChar]) {
                if (inputIndex < cleanInput.length && maskRules[maskChar].test(cleanInput[inputIndex])) {
                    maskedValue += cleanInput[inputIndex];
                    inputIndex++;
                }
                else {
                    maskedValue += placeholderChar;
                }
            }
            else {
                maskedValue += maskChar;
                if (cleanInput[inputIndex] === maskChar) {
                    inputIndex++;
                }
            }
        }
        return maskedValue;
    };
    const getNextCursorPosition = (cursorPosition, maskedValue) => {
        for (let i = cursorPosition; i < maskedValue.length; i++) {
            if (maskedValue[i] !== placeholderChar || maskRules[mask[i]]) {
                return i;
            }
        }
        return maskedValue.length;
    };
    const handleChange = (event) => {
        const input = event.target.value;
        const cursorPosition = event.target.selectionStart ?? 0;
        const maskedValue = applyMask(input);
        setValue(maskedValue);
        props.onChangeHandler?.(event, true, maskedValue);
        const nextCursorPosition = getNextCursorPosition(cursorPosition ?? 0, maskedValue);
        setTimeout(() => {
            inputRef.current?.setSelectionRange(nextCursorPosition, nextCursorPosition);
        }, 0);
    };
    return (createElement("input", { ref: node => {
            if (props.ref) {
                props.ref.current = node ?? undefined;
            }
        }, style: props.style, id: props.id, className: props.className, tabIndex: props.tabIndex, type: props.showAsPassowrd ? 'password' : 'text', value: value, onChange: handleChange, onKeyDown: (e) => {
            if (e.key === 'Enter') {
                props.onEnterKeyPress?.(e);
            }
        }, onFocus: props.onFocus, onBlur: props.onBlur, placeholder: placeHolder, maxLength: props.maxLength, disabled: props.disabled, ...props.ariaLabel ? { 'aria-label': props.ariaLabel } : {}, ...props.hasError ? { 'aria-invalid': true } : {}, ...props.ariaRequired ? { 'aria-required': true } : {}, ...props.required ? { required: true } : {}, autoComplete: props.autoComplete.replaceAll('_', '-') }));
};

function InputElement(props) {
    const { onLeave, value, placeHolderInput, tabIndex, ariaRequired, showQRImage, maxLengthType, inputMask, disabled, onEnter, onEnterKeyPress } = props;
    useEffect(() => {
        if (!props.isValueChangedByScan) {
            // Remove the 'opcore-input-active' class from all input elements
            document.querySelectorAll('input.opcore-input-active').forEach((element) => {
                element.classList.remove('opcore-input-active');
            });
        }
    }, [props.isInputQRImageClick]);
    const inputActiveClass = props.isInputQRImageClick || props.isValueChangedByScan ? "opcore-input-active" : "";
    const inputClass = showQRImage ? "widget-opcoretextbox-padding-right" : "";
    const classNamesInp = classNames("form-control", inputClass);
    const maxLengthInput = getMaxLength();
    const userInputMask = inputMask !== undefined && inputMask.trim() !== '' ? inputMask : '';
    const [state, setState] = useState({
        editedValue: undefined, isFocused: false, hasError: props.hasError
    });
    useEffect(() => setState((prevState) => ({ ...prevState, editedValue: undefined })), [value]);
    function getCurrentValue() {
        return state.editedValue !== undefined ? state.editedValue : value !== undefined ? value : '';
    }
    function getCurrentDisplayValue() {
        let currentValue = getCurrentValue().toString();
        if (props.attributeType === "decimal") {
            return getFormattedDecimalString(currentValue, props.applyChange !== undefined, state.isFocused ?? false);
        }
        else if (props.attributeType === "integer") {
            return getFormattedIntegerString(currentValue, state.isFocused ?? false);
        }
        else if (props.attributeType === "long") {
            return getFormattedLongString(currentValue, state.isFocused ?? false);
        }
        return currentValue;
    }
    const getValueByAttributeType = (value) => {
        if (props.attributeType === "decimal") {
            if (value === '') {
                return { isValid: true, parsedValue: '' };
            }
            let decimalValue;
            try {
                decimalValue = new Big(value);
            }
            catch (error) {
                return { isValid: false, parsedValue: value };
            }
            let decimalValueParsed;
            let decimalMode = props.decimalMode;
            let decimalPrecision = props.decimalPrecision;
            if (decimalMode === "fixed") {
                if (decimalPrecision !== undefined && decimalPrecision !== null) {
                    // Find the number of digits before the decimal point
                    const integerPart = value.split(".")[0];
                    const digitsBeforeDecimal = parseInt(integerPart) <= 0 ? integerPart.length - 1 : integerPart.length;
                    decimalValueParsed = decimalValue.toPrecision(decimalPrecision + digitsBeforeDecimal);
                }
            }
            else if (decimalMode === "auto") {
                decimalValueParsed = decimalValue.toString();
            }
            return { isValid: true, parsedValue: new Big(decimalValueParsed ?? ''), parsedValueBig: decimalValue };
        }
        else if (props.attributeType === "integer") {
            if (value === '') {
                return { isValid: true, parsedValue: '' };
            }
            const integerRegex = /^-?\d{1,10}$/;
            if (!integerRegex.test(value)) {
                return { isValid: false, parsedValue: value };
            }
            return { isValid: true, parsedValue: new Big(value) };
        }
        else if (props.attributeType === "long") {
            if (value === '') {
                return { isValid: true, parsedValue: '' };
            }
            const longRegex = /^-?\d{1,19}$/;
            if (!longRegex.test(value)) {
                return { isValid: false, parsedValue: value };
            }
            return { isValid: true, parsedValue: new Big(value) };
        }
        return { isValid: true, parsedValue: value };
    };
    const getFormattedDecimalString = (value, applyChangeWhileEditing, isFocused) => {
        if (value === '' || state.hasError) {
            return value;
        }
        const [integerPart, decimalPart] = value.split(".");
        let groupFormattedIntegerPart = '';
        let digitsBeforeDecimal = parseInt(integerPart) <= 0 ? integerPart.length - 1 : integerPart.length;
        let decimalPrecision = props.decimalMode === "fixed" ? props.decimalPrecision : props.decimalMode === "auto" ? decimalPart?.length : undefined;
        if (decimalPrecision !== undefined) {
            decimalPrecision = parseInt(integerPart) === 0 ? decimalPrecision - countLeadingZeros(decimalPart) : decimalPrecision;
            if (integerPart === '-0' && decimalPrecision > 0)
                decimalPrecision--;
        }
        let formattedDecimal = new Big(`${integerPart ?? 0}.${decimalPart ?? 0}`);
        let formattedDecimalString = formattedDecimal.toPrecision(decimalPrecision ?
            (decimalPrecision + digitsBeforeDecimal) : undefined);
        let [formattedIntegerPart, formattedDecimalPart] = formattedDecimalString.split(".");
        formattedIntegerPart = integerPart === '-0' ? '-0' : formattedIntegerPart;
        if (applyChangeWhileEditing || (!applyChangeWhileEditing && !isFocused)) {
            if (props.groupDigits && formattedIntegerPart.length > 3) {
                groupFormattedIntegerPart = formattedIntegerPart.replace(/(\d)(?=(\d{3})+(?!\d))/g, "$1,");
                return formattedDecimalPart ? `${groupFormattedIntegerPart}.${formattedDecimalPart}` : groupFormattedIntegerPart;
            }
            else {
                return formattedDecimalPart ? `${formattedIntegerPart}.${formattedDecimalPart}` : formattedIntegerPart;
            }
        }
        else if (isFocused) {
            return decimalPart !== undefined ? `${formattedIntegerPart}.${decimalPart}` : integerPart;
        }
        else {
            return formattedDecimalPart ? `${formattedIntegerPart}.${formattedDecimalPart}` : formattedIntegerPart;
        }
    };
    const getFormattedIntegerString = (value, isFocused) => {
        if (state.hasError)
            return value;
        let groupFormattedIntegerPart = '';
        if (props.groupDigits && value.length > 3 && !isFocused) {
            groupFormattedIntegerPart = value.replace(/(\d)(?=(\d{3})+(?!\d))/g, "$1,");
            return groupFormattedIntegerPart;
        }
        else
            return value;
    };
    const getFormattedLongString = (value, isFocused) => {
        if (state.hasError)
            return value;
        let groupFormattedLongPart = '';
        if (props.groupDigits && value.length > 3 && !isFocused) {
            groupFormattedLongPart = value.replace(/(\d)(?=(\d{3})+(?!\d))/g, "$1,");
            return groupFormattedLongPart;
        }
        else
            return value;
    };
    function countLeadingZeros(value) {
        const str = value.toString();
        let count = 0;
        for (let i = 0; i < str.length; i++) {
            if (str[i] === '0') {
                count++;
            }
            else {
                break;
            }
        }
        return count;
    }
    function getMaxLength() {
        if (maxLengthType === "default")
            return 200;
        if (maxLengthType === "custom")
            return props.maxLength;
        else
            return 0;
    }
    const onBlur = () => {
        let currentValue = getCurrentValue();
        let valueToBeSet = getValueByAttributeType(currentValue.toString());
        if (valueToBeSet.isValid && valueToBeSet.parsedValue !== undefined) {
            onLeave?.(valueToBeSet.parsedValue, valueToBeSet.parsedValue.toString() !== value, false, false);
        }
        else
            onLeave?.(currentValue, false, false, true);
        if (valueToBeSet.isValid)
            setState({
                editedValue: valueToBeSet.isValid ? undefined : valueToBeSet.parsedValue?.toString(),
                isFocused: false, hasError: !valueToBeSet.isValid
            });
    };
    const onEnterHandle = () => {
        setState((prevState) => ({ ...prevState, isFocused: true }));
        onEnter?.(true);
    };
    const onEnterKeyPressHandler = (event) => {
        event.preventDefault();
        let currentValue = getCurrentValue();
        let valueToBeSet = getValueByAttributeType(currentValue.toString());
        if (valueToBeSet.isValid && valueToBeSet.parsedValue !== undefined) {
            onEnterKeyPress?.(valueToBeSet.parsedValue, valueToBeSet.parsedValue.toString() !== value, false);
        }
        else
            onEnterKeyPress?.(currentValue, false, true);
    };
    const onChangeHandle = (event, isInputMaskValue = false, value = '') => {
        var valueShouldBeSet = false;
        var valueToBeSet = isInputMaskValue ? value : event.currentTarget.value;
        if (maxLengthInput === 0 || valueToBeSet.length <= maxLengthInput)
            valueShouldBeSet = true;
        if (valueShouldBeSet) {
            let { isValid, parsedValue } = getValueByAttributeType(valueToBeSet);
            setState((prevState) => ({ ...prevState, editedValue: valueToBeSet, hasError: !isValid }));
            if (props.applyChange !== undefined) {
                if (isValid && parsedValue !== undefined) {
                    props.applyChange(parsedValue);
                }
            }
        }
    };
    const inputStyle = {
        width: "100%"
    };
    function getInputClassNames() {
        let classNamesResult = props.isInputQRImageClick || props.isValueChangedByScan ? classNames(classNamesInp, inputActiveClass) : classNamesInp;
        setTimeout(() => {
            return classNamesResult;
        }, 500);
        return classNamesResult;
    }
    return (userInputMask === '' ?
        createElement("input", { ref: node => {
                if (props.inputRef) {
                    props.inputRef.current = node ?? undefined;
                }
            }, style: inputStyle, id: props.id, className: getInputClassNames(), tabIndex: tabIndex, value: getCurrentDisplayValue().toString(), type: props.inputType ? "password" : "text", maxLength: maxLengthInput === 0 ? undefined : maxLengthInput, onFocus: onEnterHandle, onKeyDown: (event) => {
                if (event.key === "Enter") {
                    onEnterKeyPressHandler(event);
                }
            }, placeholder: placeHolderInput, onBlur: onBlur, onChange: (event) => onChangeHandle(event), disabled: disabled, ...props.ariaLabel ? { "aria-label": props.ariaLabel } : {}, ...props.hasError || state.hasError ? { "aria-invalid": true } : {}, ...props.ariaRequired ? { "aria-required": true } : {}, ...props.required ? { required: true } : {}, autoComplete: props.autoComplete.replaceAll("_", "-") }) :
        createElement(CustomInputMask, { ref: props.inputRef, key: value, id: props.id, style: inputStyle, className: getInputClassNames(), tabIndex: tabIndex, showAsPassowrd: props.inputType, mask: userInputMask, maxLength: maxLengthInput === 0 ? undefined : maxLengthInput, value: getCurrentDisplayValue().toString(), placeHolderChar: "_", placeHolderInput: placeHolderInput, onChangeHandler: onChangeHandle, onEnterKeyPress: (event) => { onEnterKeyPressHandler(event); }, onFocus: onEnterHandle, onBlur: onBlur, disabled: disabled, ariaLabel: props.ariaLabel, ariaInvalid: props.hasError || state.hasError, ariaRequired: ariaRequired, required: props.required, autoComplete: props.autoComplete.replaceAll("_", "-") }));
}

var cmdQRCode = "widgets/mendix/opcentercoretextbox/assets/829624eac6f2835e.svg";

const BarcodeImage = ({ style, onClickHandler }) => createElement("img", { style: style, className: "opcore-qr-image", role: "button", src: cmdQRCode, onMouseDown: () => onClickHandler !== undefined ? onClickHandler(true) : false });

function Alert({ id, message, className, bootstrapStyle }) {
    return message ? createElement("div", { id: `${id}-error`, role: "alert", className: classNames(`alert alert-${bootstrapStyle}`, className) }, message) : null;
}

var isFocusFromQRImage = false;
function OpcenterCoreTextBox(props) {
    const { valueAttribute, showAsPassowrd, placeholder, showQrImageUserInput, inputMask, readOnlyStyle, onClickAction, onBarcodeScanAction, submitDelay, onEnterAction, tabIndex, onEnterKeyAction, onLeaveAction } = props;
    const validationFeedback = props.valueAttribute?.validation;
    const required = props.validationProperty === "required" || props.validationPropertyForDigit === "required";
    const submitDelayValue = submitDelay ? submitDelay >= 0 ? submitDelay : 0 : 0;
    const inputRef = useRef();
    const [state, setState] = useState({
        showInputQRImage: false,
        isInputQRImageClick: false, isValueChangeByScan: false
    });
    // useEffect(() => {
    //     checkValidators();
    // }, []);
    useEffect(() => {
        CheckIfAttributeValueChangedByScan();
        checkValidators();
    }, [valueAttribute?.value]);
    const checkValidators = () => {
        if (props.validationProperty === "required" || props.validationPropertyForDigit === "required") {
            props.valueAttribute?.setValidator(requiredvalidator);
        }
        else if (props.validationProperty === "email") {
            props.valueAttribute?.setValidator(emailvalidator);
        }
        else if (props.validationProperty === "custom" || props.validationPropertyForDigit === "custom") {
            props.valueAttribute?.setValidator(customValidator);
        }
        else if (props.validationPropertyForDigit === "positiveNumber") {
            props.valueAttribute?.setValidator(positiveNumberValidator);
        }
        else
            props.valueAttribute?.setValidator(undefined);
    };
    function CheckIfAttributeValueChangedByScan() {
        if (state.isInputQRImageClick) {
            if (onClickAction) {
                var attributeValue = valueAttribute?.value !== undefined ? valueAttribute.displayValue : '';
                if (attributeValue !== '') {
                    setTimeout(() => {
                        onBarcodeScanActionExecute();
                    }, 500);
                    setTimeout(() => {
                        focusNextElement();
                    }, 1000);
                    //focusNextElement(1500);
                    if (onLeaveAction && onLeaveAction.canExecute) {
                        onLeaveAction.execute();
                    }
                }
            }
            setState((prevState) => ({ ...prevState, isInputQRImageClick: false, isValueChangeByScan: true }));
        }
        else {
            setState((prevState) => ({ ...prevState, isValueChangeByScan: false }));
        }
    }
    function onBarcodeScanActionExecute() {
        if (onBarcodeScanAction && onBarcodeScanAction.canExecute) {
            onBarcodeScanAction.execute();
        }
    }
    const onClickHandler = (isQRImageClicked) => {
        if (onClickAction && onClickAction.canExecute) {
            onClickAction.execute();
        }
        setState((prevState) => ({ ...prevState, isInputQRImageClick: isQRImageClicked }));
    };
    const onEnterHandler = (showQRImage) => {
        if (!isFocusFromQRImage && onEnterAction && onEnterAction.canExecute) {
            onEnterAction.execute();
        }
        isFocusFromQRImage = false;
        setState((prevState) => ({
            ...prevState,
            showInputQRImage: showQRImage, isInputQRImageClick: false
        }));
    };
    const onEnterKeyPressHandler = (value, isChanged, isValueInvalid = false) => {
        if (!isValueInvalid) {
            if (isChanged) {
                applyChange(value);
            }
        }
        if (onEnterKeyAction && onEnterKeyAction.canExecute) {
            onEnterKeyAction.execute();
        }
    };
    const onLeaveHandler = (value, isChanged, showQRImage, isValueInvalid = false) => {
        if (!isValueInvalid) {
            if (!state.isInputQRImageClick && onLeaveAction && onLeaveAction.canExecute) {
                onLeaveAction.execute();
            }
            if (isChanged) {
                if ((!state.isInputQRImageClick) || (state.isInputQRImageClick && !onClickAction))
                    applyChange(value);
            }
            if (state.isInputQRImageClick && !onClickAction) {
                focusCurrentElement();
            }
            else {
                setState((prevState) => ({ ...prevState, showInputQRImage: showQRImage }));
            }
        }
        else
            setState((prevState) => ({ ...prevState, showInputQRImage: showQRImage }));
    };
    const applyChange = (value) => {
        if (!props.valueAttribute?.readOnly && props.valueAttribute?.status === "available") {
            if (value === '' && (props.selectedAttributeType === "integer"
                || props.selectedAttributeType === "decimal" || props.selectedAttributeType === "long")) {
                props.valueAttribute.setValue(undefined);
            }
            else
                props.valueAttribute.setValue(value);
        }
    };
    const applyChangeWhileEditing = (value) => {
        setTimeout(() => applyChange(value), submitDelayValue);
    };
    const requiredvalidator = (value) => {
        const { validationMessage } = props;
        if (validationMessage && !value) {
            return validationMessage;
        }
    };
    const emailvalidator = (value) => {
        const { validationMessage } = props;
        if (value !== undefined && value !== '') {
            let matchResult = value.match(/[A-Za-z0-9!#$%&''*+/=?^_`{|}~-]+(?:\.[A-Za-z0-9!#$%&''*+/=?^_`{|}~-]+)*@(?:[A-Za-z0-9](?:[A-Za-z0-9-]*[A-Za-z0-9])?\.)+[A-Za-z0-9](?:[A-Za-z0-9-]*[A-Za-z0-9])?/);
            if (matchResult === null) {
                if (validationMessage) {
                    return validationMessage;
                }
            }
        }
    };
    const positiveNumberValidator = (value) => {
        const { validationMessage } = props;
        if (value !== undefined && value !== '') {
            let matchResult = value.toString().match(/^(?!0(\.0+)?$)\d*\.?\d+$/);
            if (matchResult === null) {
                if (validationMessage) {
                    return validationMessage;
                }
            }
        }
    };
    const customValidator = (value) => {
        const { validationMessage, validationExpression } = props;
        if (validationExpression?.value && value !== undefined && value !== '') {
            let matchResult = value.toString().match(validationExpression.value);
            if (matchResult === null) {
                if (validationMessage) {
                    return validationMessage;
                }
            }
        }
    };
    const focusCurrentElement = () => {
        if (inputRef.current) {
            inputRef.current.focus();
        }
    };
    const focusNextElement = (timeout = 0) => {
        // if (document.activeElement instanceof HTMLElement) {
        //     document.activeElement.blur();
        // }
        if (inputRef.current) {
            let currentElement = inputRef.current;
            // Traverse up to the parent element
            while (currentElement) {
                let nextElement = currentElement.nextElementSibling;
                // Traverse down to find the next focusable element
                while (nextElement) {
                    const focusableElement = findFocusableElement(nextElement);
                    if (focusableElement) {
                        //focusableElement.focus();
                        setTimeout(() => {
                            focusableElement.focus();
                        }, timeout);
                        return;
                    }
                    nextElement = nextElement.nextElementSibling;
                }
                currentElement = currentElement.parentElement;
            }
        }
    };
    const findFocusableElement = (element) => {
        if (isFocusable(element)) {
            return element;
        }
        const children = element.children;
        for (let i = 0; i < children.length; i++) {
            const focusableChild = findFocusableElement(children[i]);
            if (focusableChild) {
                return focusableChild;
            }
        }
        return null;
    };
    const isFocusable = (element) => {
        const focusableElements = ['INPUT', 'BUTTON', 'SELECT', 'TEXTAREA', 'A'];
        return (focusableElements.includes(element.tagName) ||
            element.tabIndex >= 0);
    };
    return ((!props.valueAttribute?.readOnly && readOnlyStyle === "text") || (readOnlyStyle === "dataView" || readOnlyStyle === "control") ?
        createElement("div", null,
            createElement("div", { className: "widget-opcoretextbox-holder" },
                createElement(InputElement, { inputRef: inputRef, id: props.id, inputType: showAsPassowrd, placeHolderInput: placeholder?.value, inputMask: inputMask, tabIndex: tabIndex, maxLengthType: props.maxLengthType, maxLength: props.customMaxLength, ariaRequired: props.ariaRequired, ariaLabel: props.screenReaderCaption?.value, autoComplete: props.autoComplete, showQRImage: showQrImageUserInput !== undefined && showQrImageUserInput ? !!state.showInputQRImage : false, isInputQRImageClick: state.isInputQRImageClick, isValueChangedByScan: state.isValueChangeByScan, attributeType: props.selectedAttributeType, decimalMode: props.decimalMode, decimalPrecision: props.decimalPrecision, groupDigits: props.groupDigits, onEnter: onEnterHandler, onLeave: onLeaveHandler, onEnterKeyPress: onEnterKeyPressHandler, onChange: props.valueAttribute?.setValue, applyChange: props.submitWhileEditing === "whileEditing" ? applyChangeWhileEditing : undefined, value: valueAttribute ? valueAttribute.displayValue : "", disabled: props.valueAttribute?.readOnly, hasError: !!validationFeedback, required: required }),
                showQrImageUserInput !== undefined && showQrImageUserInput && !!state.showInputQRImage ?
                    createElement(BarcodeImage, { onClickHandler: onClickHandler }) : null,
                createElement(Alert, { id: props.id, message: validationFeedback, bootstrapStyle: "danger", className: "mx-validation-message" }))) :
        createElement("div", { className: "widget-opcoretextbox-holder" },
            createElement("div", { className: "form-control-static" }, valueAttribute ? valueAttribute.displayValue : " ")));
}

export { OpcenterCoreTextBox };
//# sourceMappingURL=data:application/json;charset=utf-8;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoiT3BjZW50ZXJDb3JlVGV4dEJveC5tanMiLCJzb3VyY2VzIjpbIi4uLy4uLy4uLy4uLy4uL25vZGVfbW9kdWxlcy9jbGFzc25hbWVzL2luZGV4LmpzIiwiLi4vLi4vLi4vLi4vLi4vc3JjL2NvbXBvbmVudHMvSW5wdXRNYXNrRWxlbWVudC50c3giLCIuLi8uLi8uLi8uLi8uLi9zcmMvY29tcG9uZW50cy9JbnB1dEVsZW1lbnQudHN4IiwiLi4vLi4vLi4vLi4vLi4vc3JjL2ltYWdlcy9jbWRRUkNvZGUuc3ZnIiwiLi4vLi4vLi4vLi4vLi4vc3JjL2NvbXBvbmVudHMvQmFyY29kZUltYWdlLnRzeCIsIi4uLy4uLy4uLy4uLy4uL3NyYy9jb21wb25lbnRzL0FsZXJ0LnRzeCIsIi4uLy4uLy4uLy4uLy4uL3NyYy9PcGNlbnRlckNvcmVUZXh0Qm94LnRzeCJdLCJzb3VyY2VzQ29udGVudCI6WyIvKiFcblx0Q29weXJpZ2h0IChjKSAyMDE4IEplZCBXYXRzb24uXG5cdExpY2Vuc2VkIHVuZGVyIHRoZSBNSVQgTGljZW5zZSAoTUlUKSwgc2VlXG5cdGh0dHA6Ly9qZWR3YXRzb24uZ2l0aHViLmlvL2NsYXNzbmFtZXNcbiovXG4vKiBnbG9iYWwgZGVmaW5lICovXG5cbihmdW5jdGlvbiAoKSB7XG5cdCd1c2Ugc3RyaWN0JztcblxuXHR2YXIgaGFzT3duID0ge30uaGFzT3duUHJvcGVydHk7XG5cblx0ZnVuY3Rpb24gY2xhc3NOYW1lcyAoKSB7XG5cdFx0dmFyIGNsYXNzZXMgPSAnJztcblxuXHRcdGZvciAodmFyIGkgPSAwOyBpIDwgYXJndW1lbnRzLmxlbmd0aDsgaSsrKSB7XG5cdFx0XHR2YXIgYXJnID0gYXJndW1lbnRzW2ldO1xuXHRcdFx0aWYgKGFyZykge1xuXHRcdFx0XHRjbGFzc2VzID0gYXBwZW5kQ2xhc3MoY2xhc3NlcywgcGFyc2VWYWx1ZShhcmcpKTtcblx0XHRcdH1cblx0XHR9XG5cblx0XHRyZXR1cm4gY2xhc3Nlcztcblx0fVxuXG5cdGZ1bmN0aW9uIHBhcnNlVmFsdWUgKGFyZykge1xuXHRcdGlmICh0eXBlb2YgYXJnID09PSAnc3RyaW5nJyB8fCB0eXBlb2YgYXJnID09PSAnbnVtYmVyJykge1xuXHRcdFx0cmV0dXJuIGFyZztcblx0XHR9XG5cblx0XHRpZiAodHlwZW9mIGFyZyAhPT0gJ29iamVjdCcpIHtcblx0XHRcdHJldHVybiAnJztcblx0XHR9XG5cblx0XHRpZiAoQXJyYXkuaXNBcnJheShhcmcpKSB7XG5cdFx0XHRyZXR1cm4gY2xhc3NOYW1lcy5hcHBseShudWxsLCBhcmcpO1xuXHRcdH1cblxuXHRcdGlmIChhcmcudG9TdHJpbmcgIT09IE9iamVjdC5wcm90b3R5cGUudG9TdHJpbmcgJiYgIWFyZy50b1N0cmluZy50b1N0cmluZygpLmluY2x1ZGVzKCdbbmF0aXZlIGNvZGVdJykpIHtcblx0XHRcdHJldHVybiBhcmcudG9TdHJpbmcoKTtcblx0XHR9XG5cblx0XHR2YXIgY2xhc3NlcyA9ICcnO1xuXG5cdFx0Zm9yICh2YXIga2V5IGluIGFyZykge1xuXHRcdFx0aWYgKGhhc093bi5jYWxsKGFyZywga2V5KSAmJiBhcmdba2V5XSkge1xuXHRcdFx0XHRjbGFzc2VzID0gYXBwZW5kQ2xhc3MoY2xhc3Nlcywga2V5KTtcblx0XHRcdH1cblx0XHR9XG5cblx0XHRyZXR1cm4gY2xhc3Nlcztcblx0fVxuXG5cdGZ1bmN0aW9uIGFwcGVuZENsYXNzICh2YWx1ZSwgbmV3Q2xhc3MpIHtcblx0XHRpZiAoIW5ld0NsYXNzKSB7XG5cdFx0XHRyZXR1cm4gdmFsdWU7XG5cdFx0fVxuXHRcblx0XHRpZiAodmFsdWUpIHtcblx0XHRcdHJldHVybiB2YWx1ZSArICcgJyArIG5ld0NsYXNzO1xuXHRcdH1cblx0XG5cdFx0cmV0dXJuIHZhbHVlICsgbmV3Q2xhc3M7XG5cdH1cblxuXHRpZiAodHlwZW9mIG1vZHVsZSAhPT0gJ3VuZGVmaW5lZCcgJiYgbW9kdWxlLmV4cG9ydHMpIHtcblx0XHRjbGFzc05hbWVzLmRlZmF1bHQgPSBjbGFzc05hbWVzO1xuXHRcdG1vZHVsZS5leHBvcnRzID0gY2xhc3NOYW1lcztcblx0fSBlbHNlIGlmICh0eXBlb2YgZGVmaW5lID09PSAnZnVuY3Rpb24nICYmIHR5cGVvZiBkZWZpbmUuYW1kID09PSAnb2JqZWN0JyAmJiBkZWZpbmUuYW1kKSB7XG5cdFx0Ly8gcmVnaXN0ZXIgYXMgJ2NsYXNzbmFtZXMnLCBjb25zaXN0ZW50IHdpdGggbnBtIHBhY2thZ2UgbmFtZVxuXHRcdGRlZmluZSgnY2xhc3NuYW1lcycsIFtdLCBmdW5jdGlvbiAoKSB7XG5cdFx0XHRyZXR1cm4gY2xhc3NOYW1lcztcblx0XHR9KTtcblx0fSBlbHNlIHtcblx0XHR3aW5kb3cuY2xhc3NOYW1lcyA9IGNsYXNzTmFtZXM7XG5cdH1cbn0oKSk7XG4iLCJpbXBvcnQgUmVhY3QsIHsgY3JlYXRlRWxlbWVudCwgdXNlU3RhdGUsIHVzZVJlZiwgQ1NTUHJvcGVydGllcywgQ2hhbmdlRXZlbnQgfSBmcm9tICdyZWFjdCc7XG5cbmV4cG9ydCBpbnRlcmZhY2UgSW5wdXRNYXNrUHJvcHMge1xuICAgIHJlZj86IFJlYWN0Lk11dGFibGVSZWZPYmplY3Q8SFRNTElucHV0RWxlbWVudCB8IHVuZGVmaW5lZD5cbiAgICBpZDogc3RyaW5nO1xuICAgIGNsYXNzTmFtZT86IHN0cmluZztcbiAgICBzdHlsZT86IENTU1Byb3BlcnRpZXM7XG4gICAgc2hvd0FzUGFzc293cmQ/OiBib29sZWFuO1xuICAgIGlucHV0TWFzaz86IHN0cmluZztcbiAgICBtYXNrPzogc3RyaW5nO1xuICAgIHBsYWNlSG9sZGVyQ2hhcj86IHN0cmluZztcbiAgICBwbGFjZUhvbGRlcklucHV0Pzogc3RyaW5nO1xuICAgIHRhYkluZGV4PzogbnVtYmVyO1xuICAgIHZhbHVlPzogc3RyaW5nO1xuICAgIG9uQ2hhbmdlSGFuZGxlcj86IChldmVudDogQ2hhbmdlRXZlbnQ8SFRNTElucHV0RWxlbWVudD4sIGluSW5wdXRNYXNrVmFsdWU6IGJvb2xlYW4sIHZhbHVlOiBzdHJpbmcpID0+IHZvaWQ7XG4gICAgb25FbnRlcktleVByZXNzPzogUmVhY3QuS2V5Ym9hcmRFdmVudEhhbmRsZXI8SFRNTElucHV0RWxlbWVudD47XG4gICAgb25Gb2N1czooKSA9PiB2b2lkO1xuICAgIG9uQmx1cjogKCkgPT4gdm9pZDtcbiAgICBhcmlhUmVxdWlyZWQ6IGJvb2xlYW47XG4gICAgYXJpYUxhYmVsPzogc3RyaW5nXG4gICAgYXJpYUludmFsaWQ/OiBib29sZWFuO1xuICAgIGF1dG9Db21wbGV0ZTogc3RyaW5nO1xuICAgIG1heExlbmd0aD86IG51bWJlcjtcbiAgICBkaXNhYmxlZD86IGJvb2xlYW47XG4gICAgaGFzRXJyb3I/OiBib29sZWFuO1xuICAgIHJlcXVpcmVkPzogYm9vbGVhbjtcbn1cblxudmFyIHBsYWNlSG9sZGVyID0gJyc7XG5cbmNvbnN0IEN1c3RvbUlucHV0TWFzayA9IChwcm9wczogSW5wdXRNYXNrUHJvcHMpID0+IHtcbiAgICBjb25zdCBtYXNrID0gcHJvcHMubWFzayA/PyAnJztcbiAgICBjb25zdCBwbGFjZWhvbGRlckNoYXIgPSBwcm9wcy5wbGFjZUhvbGRlckNoYXIgPz8gJ18nO1xuICAgIHBsYWNlSG9sZGVyID0gcHJvcHMucGxhY2VIb2xkZXJJbnB1dCA/PyAnJztcbiAgICBcbiAgICBjb25zdCBbdmFsdWUsIHNldFZhbHVlXSA9IHVzZVN0YXRlKHByb3BzLnZhbHVlID8/ICcnKTtcbiAgICBjb25zdCBpbnB1dFJlZiA9IHVzZVJlZjxIVE1MSW5wdXRFbGVtZW50PihudWxsKTtcblxuICAgIGNvbnN0IG1hc2tSdWxlczogeyBba2V5OiBzdHJpbmddOiBSZWdFeHAgfSA9IHtcbiAgICAgICAgJzknOiAvXFxkLyxcbiAgICAgICAgJ1onOiAvW0EtWmEtel0vLFxuICAgICAgICAnVSc6IC9bQS1aXS8sXG4gICAgICAgICdMJzogL1thLXpdLyxcbiAgICAgICAgJyonOiAvW0EtWmEtejAtOV0vXG4gICAgfTtcblxuXG4gICAgY29uc3QgY2xlYW5WYWx1ZSA9IChpbnB1dDogc3RyaW5nKSA9PiB7XG4gICAgICAgIHJldHVybiBpbnB1dC5yZXBsYWNlKC9bXjAtOWEtekEtWl0vZywgJycpO1xuICAgIH1cblxuICAgIGNvbnN0IGFwcGx5TWFzayA9IChpbnB1dDogc3RyaW5nKSA9PiB7XG4gICAgICAgIGxldCBjbGVhbklucHV0ID0gY2xlYW5WYWx1ZShpbnB1dCk7XG4gICAgICAgIGxldCBtYXNrZWRWYWx1ZSA9ICcnO1xuICAgICAgICBsZXQgaW5wdXRJbmRleCA9IDA7XG5cbiAgICAgICAgZm9yIChsZXQgaSA9IDA7IGkgPCBtYXNrLmxlbmd0aDsgaSsrKSB7XG4gICAgICAgICAgICBjb25zdCBtYXNrQ2hhciA9IG1hc2tbaV07XG5cbiAgICAgICAgICAgIGlmIChtYXNrUnVsZXNbbWFza0NoYXJdKSB7XG4gICAgICAgICAgICAgICAgaWYgKGlucHV0SW5kZXggPCBjbGVhbklucHV0Lmxlbmd0aCAmJiBtYXNrUnVsZXNbbWFza0NoYXJdLnRlc3QoY2xlYW5JbnB1dFtpbnB1dEluZGV4XSkpIHtcbiAgICAgICAgICAgICAgICAgICAgbWFza2VkVmFsdWUgKz0gY2xlYW5JbnB1dFtpbnB1dEluZGV4XTtcbiAgICAgICAgICAgICAgICAgICAgaW5wdXRJbmRleCsrO1xuICAgICAgICAgICAgICAgIH0gZWxzZSB7XG4gICAgICAgICAgICAgICAgICAgIG1hc2tlZFZhbHVlICs9IHBsYWNlaG9sZGVyQ2hhcjtcbiAgICAgICAgICAgICAgICB9XG4gICAgICAgICAgICB9IGVsc2Uge1xuICAgICAgICAgICAgICAgIG1hc2tlZFZhbHVlICs9IG1hc2tDaGFyO1xuICAgICAgICAgICAgICAgIGlmIChjbGVhbklucHV0W2lucHV0SW5kZXhdID09PSBtYXNrQ2hhcikge1xuICAgICAgICAgICAgICAgICAgICBpbnB1dEluZGV4Kys7XG4gICAgICAgICAgICAgICAgfVxuICAgICAgICAgICAgfVxuICAgICAgICB9XG5cbiAgICAgICAgcmV0dXJuIG1hc2tlZFZhbHVlO1xuICAgIH07XG5cblxuICAgIGNvbnN0IGdldE5leHRDdXJzb3JQb3NpdGlvbiA9IChjdXJzb3JQb3NpdGlvbjogbnVtYmVyLCBtYXNrZWRWYWx1ZTogc3RyaW5nKSA9PiB7XG4gICAgICAgIGZvciAobGV0IGkgPSBjdXJzb3JQb3NpdGlvbjsgaSA8IG1hc2tlZFZhbHVlLmxlbmd0aDsgaSsrKSB7XG4gICAgICAgICAgICBpZiAobWFza2VkVmFsdWVbaV0gIT09IHBsYWNlaG9sZGVyQ2hhciB8fCBtYXNrUnVsZXNbbWFza1tpXV0pIHtcbiAgICAgICAgICAgICAgICByZXR1cm4gaTtcbiAgICAgICAgICAgIH1cbiAgICAgICAgfVxuICAgICAgICByZXR1cm4gbWFza2VkVmFsdWUubGVuZ3RoO1xuICAgIH07XG5cbiAgICBjb25zdCBoYW5kbGVDaGFuZ2UgPSAoZXZlbnQ6IFJlYWN0LkNoYW5nZUV2ZW50PEhUTUxJbnB1dEVsZW1lbnQ+KSA9PiB7XG4gICAgICAgIGNvbnN0IGlucHV0ID0gZXZlbnQudGFyZ2V0LnZhbHVlO1xuICAgICAgICBjb25zdCBjdXJzb3JQb3NpdGlvbiA9IGV2ZW50LnRhcmdldC5zZWxlY3Rpb25TdGFydCA/PyAwO1xuICAgICAgICBjb25zdCBtYXNrZWRWYWx1ZSA9IGFwcGx5TWFzayhpbnB1dCk7XG4gICAgICAgIHNldFZhbHVlKG1hc2tlZFZhbHVlKTtcbiAgICAgICAgcHJvcHMub25DaGFuZ2VIYW5kbGVyPy4oZXZlbnQsIHRydWUsIG1hc2tlZFZhbHVlKTtcblxuICAgICAgICBjb25zdCBuZXh0Q3Vyc29yUG9zaXRpb24gPSBnZXROZXh0Q3Vyc29yUG9zaXRpb24oY3Vyc29yUG9zaXRpb24gPz8gMCwgbWFza2VkVmFsdWUpO1xuXG4gICAgICAgIHNldFRpbWVvdXQoKCkgPT4ge1xuICAgICAgICAgICAgaW5wdXRSZWYuY3VycmVudD8uc2V0U2VsZWN0aW9uUmFuZ2UobmV4dEN1cnNvclBvc2l0aW9uLCBuZXh0Q3Vyc29yUG9zaXRpb24pO1xuICAgICAgICB9LCAwKTtcbiAgICB9XG5cblxuXG4gICAgcmV0dXJuIChcblxuICAgICAgICA8aW5wdXRcbiAgICAgICAgICAgIHJlZj17bm9kZSA9PiB7XG4gICAgICAgICAgICAgICAgaWYgKHByb3BzLnJlZikge1xuICAgICAgICAgICAgICAgICAgICBwcm9wcy5yZWYuY3VycmVudCA9IG5vZGUgPz8gdW5kZWZpbmVkO1xuICAgICAgICAgICAgICAgIH1cbiAgICAgICAgICAgIH19XG4gICAgICAgICAgICBzdHlsZT17cHJvcHMuc3R5bGV9XG4gICAgICAgICAgICBpZD17cHJvcHMuaWR9XG4gICAgICAgICAgICBjbGFzc05hbWU9e3Byb3BzLmNsYXNzTmFtZX1cbiAgICAgICAgICAgIHRhYkluZGV4PXtwcm9wcy50YWJJbmRleH1cbiAgICAgICAgICAgIHR5cGU9e3Byb3BzLnNob3dBc1Bhc3Nvd3JkID8gJ3Bhc3N3b3JkJyA6ICd0ZXh0J31cbiAgICAgICAgICAgIHZhbHVlPXt2YWx1ZX1cbiAgICAgICAgICAgIG9uQ2hhbmdlPXtoYW5kbGVDaGFuZ2V9XG4gICAgICAgICAgICBvbktleURvd249eyhlKSA9PiB7XG4gICAgICAgICAgICAgICAgaWYgKGUua2V5ID09PSAnRW50ZXInKSB7XG4gICAgICAgICAgICAgICAgICAgIHByb3BzLm9uRW50ZXJLZXlQcmVzcz8uKGUpO1xuICAgICAgICAgICAgICAgIH1cbiAgICAgICAgICAgIH19XG4gICAgICAgICAgICBvbkZvY3VzPXtwcm9wcy5vbkZvY3VzfVxuICAgICAgICAgICAgb25CbHVyPXtwcm9wcy5vbkJsdXJ9XG4gICAgICAgICAgICBwbGFjZWhvbGRlcj17cGxhY2VIb2xkZXJ9XG4gICAgICAgICAgICBtYXhMZW5ndGg9e3Byb3BzLm1heExlbmd0aH1cbiAgICAgICAgICAgIGRpc2FibGVkPXtwcm9wcy5kaXNhYmxlZH1cbiAgICAgICAgICAgIHsuLi5wcm9wcy5hcmlhTGFiZWwgPyB7ICdhcmlhLWxhYmVsJzogcHJvcHMuYXJpYUxhYmVsIH0gOiB7fX1cbiAgICAgICAgICAgIHsuLi5wcm9wcy5oYXNFcnJvciA/IHsgJ2FyaWEtaW52YWxpZCc6IHRydWUgfSA6IHt9fVxuICAgICAgICAgICAgey4uLnByb3BzLmFyaWFSZXF1aXJlZCA/IHsgJ2FyaWEtcmVxdWlyZWQnOiB0cnVlIH0gOiB7fX1cbiAgICAgICAgICAgIHsuLi5wcm9wcy5yZXF1aXJlZCA/IHsgcmVxdWlyZWQ6IHRydWUgfSA6IHt9fVxuICAgICAgICAgICAgYXV0b0NvbXBsZXRlPXtwcm9wcy5hdXRvQ29tcGxldGUucmVwbGFjZUFsbCgnXycsICctJyl9XG4gICAgICAgIC8+XG4gICAgKTtcbn1cblxuZXhwb3J0IGRlZmF1bHQgQ3VzdG9tSW5wdXRNYXNrOyIsImltcG9ydCB7IFJlYWN0RWxlbWVudCwgQ1NTUHJvcGVydGllcywgY3JlYXRlRWxlbWVudCwgQ2hhbmdlRXZlbnQsIHVzZVN0YXRlLCB1c2VFZmZlY3QsIFJlZiB9IGZyb20gXCJyZWFjdFwiO1xuaW1wb3J0IHsgRGVjaW1hbE1vZGVFbnVtLCBNYXhMZW5ndGhUeXBlRW51bSwgU2VsZWN0ZWRBdHRyaWJ1dGVUeXBlRW51bSB9IGZyb20gXCJ0eXBpbmdzL09wY2VudGVyQ29yZVRleHRCb3hQcm9wc1wiO1xuaW1wb3J0IGNsYXNzTmFtZXMgZnJvbSBcImNsYXNzbmFtZXNcIjtcbmltcG9ydCBJbnB1dE1hc2tFbGVtZW50IGZyb20gXCIuL0lucHV0TWFza0VsZW1lbnRcIjtcbmltcG9ydCBCaWcgZnJvbSBcImJpZy5qc1wiO1xuXG5leHBvcnQgaW50ZXJmYWNlIElucHV0RWxlbWVudFByb3BzIHtcbiAgICBpZDogc3RyaW5nO1xuICAgIHJlZj86IFJlZjxIVE1MSW5wdXRFbGVtZW50PjtcbiAgICBpbnB1dFJlZj86IFJlYWN0Lk11dGFibGVSZWZPYmplY3Q8SFRNTElucHV0RWxlbWVudCB8IHVuZGVmaW5lZD5cbiAgICBjbGFzc05hbWU/OiBzdHJpbmc7XG4gICAgaW5wdXRUeXBlOiBib29sZWFuO1xuICAgIHN0eWxlPzogQ1NTUHJvcGVydGllcztcbiAgICB2YWx1ZT86IHN0cmluZztcbiAgICBzaG93UVJJbWFnZTogYm9vbGVhbjtcbiAgICBpc0lucHV0UVJJbWFnZUNsaWNrPzogYm9vbGVhbjtcbiAgICBpc1ZhbHVlQ2hhbmdlZEJ5U2Nhbj86IGJvb2xlYW47XG4gICAgaW5wdXRNYXNrPzogc3RyaW5nO1xuICAgIHBsYWNlSG9sZGVySW5wdXQ/OiBzdHJpbmc7XG4gICAgY2xpY2thYmxlPzogYm9vbGVhbjtcbiAgICB0YWJJbmRleD86IG51bWJlcjtcbiAgICBtYXhMZW5ndGhUeXBlOiBNYXhMZW5ndGhUeXBlRW51bTtcbiAgICBhcmlhUmVxdWlyZWQ6IGJvb2xlYW47XG4gICAgYXJpYUxhYmVsPzogc3RyaW5nXG4gICAgYXV0b0NvbXBsZXRlOiBzdHJpbmc7XG4gICAgbWF4TGVuZ3RoOiBudW1iZXI7XG4gICAgYXR0cmlidXRlVHlwZT86IFNlbGVjdGVkQXR0cmlidXRlVHlwZUVudW07XG4gICAgZGVjaW1hbE1vZGU/OiBEZWNpbWFsTW9kZUVudW07XG4gICAgZGVjaW1hbFByZWNpc2lvbj86IG51bWJlcjtcbiAgICBncm91cERpZ2l0cz86IGJvb2xlYW47XG4gICAgb25FbnRlcj86IChzaG93UVJJbWFnZTogYm9vbGVhbikgPT4gdm9pZDtcbiAgICBvbkNoYW5nZT86ICh2YWx1ZTogc3RyaW5nIHwgQmlnKSA9PiB2b2lkO1xuICAgIG9uTGVhdmU/OiAodmFsdWU6IHN0cmluZyB8IEJpZywgY2hhbmdlZDogYm9vbGVhbiwgXG4gICAgICAgIHNob3dRUkltYWdlOiBib29sZWFuLCBpc1ZhbHVlSW52YWxpZDogYm9vbGVhbikgPT4gdm9pZDtcbiAgICBhcHBseUNoYW5nZT86ICh2YWx1ZTogc3RyaW5nIHwgQmlnKSA9PiB2b2lkO1xuICAgIG9uRW50ZXJLZXlQcmVzcz86ICh2YWx1ZTogc3RyaW5nIHwgQmlnLCBjaGFuZ2VkOiBib29sZWFuLCBpc1ZhbHVlSW52YWxpZDogYm9vbGVhbikgPT4gdm9pZDtcbiAgICBnZXRSZWY/OiAobm9kZTogSFRNTEVsZW1lbnQpID0+IHZvaWQ7XG4gICAgZGlzYWJsZWQ/OiBib29sZWFuO1xuICAgIGhhc0Vycm9yPzogYm9vbGVhbjtcbiAgICByZXF1aXJlZD86IGJvb2xlYW47XG59XG5cbmludGVyZmFjZSBJbnB1dEVsZW1lbnRTdGF0ZSB7XG4gICAgZWRpdGVkVmFsdWU/OiBzdHJpbmc7XG4gICAgaGFzRXJyb3I/OiBib29sZWFuO1xuICAgIGlzRm9jdXNlZD86IGJvb2xlYW47XG59XG5cbmV4cG9ydCBmdW5jdGlvbiBJbnB1dEVsZW1lbnQocHJvcHM6IElucHV0RWxlbWVudFByb3BzKTogUmVhY3RFbGVtZW50IHtcbiAgICBjb25zdCB7IG9uTGVhdmUsIHZhbHVlLCBwbGFjZUhvbGRlcklucHV0LCB0YWJJbmRleCwgYXJpYVJlcXVpcmVkLCBzaG93UVJJbWFnZSwgbWF4TGVuZ3RoVHlwZSwgaW5wdXRNYXNrLFxuICAgICAgICBkaXNhYmxlZCwgb25FbnRlciwgb25FbnRlcktleVByZXNzfSA9IHByb3BzO1xuXG4gICAgdXNlRWZmZWN0KCgpID0+IHtcbiAgICAgICAgaWYgKCFwcm9wcy5pc1ZhbHVlQ2hhbmdlZEJ5U2Nhbikge1xuICAgICAgICAgICAgLy8gUmVtb3ZlIHRoZSAnb3Bjb3JlLWlucHV0LWFjdGl2ZScgY2xhc3MgZnJvbSBhbGwgaW5wdXQgZWxlbWVudHNcbiAgICAgICAgICAgIGRvY3VtZW50LnF1ZXJ5U2VsZWN0b3JBbGwoJ2lucHV0Lm9wY29yZS1pbnB1dC1hY3RpdmUnKS5mb3JFYWNoKChlbGVtZW50KSA9PiB7XG4gICAgICAgICAgICAgICAgZWxlbWVudC5jbGFzc0xpc3QucmVtb3ZlKCdvcGNvcmUtaW5wdXQtYWN0aXZlJyk7XG4gICAgICAgICAgICB9KTtcbiAgICAgICAgfVxuICAgIH0sIFtwcm9wcy5pc0lucHV0UVJJbWFnZUNsaWNrXSk7XG4gICAgY29uc3QgaW5wdXRBY3RpdmVDbGFzcyA9IHByb3BzLmlzSW5wdXRRUkltYWdlQ2xpY2sgfHwgcHJvcHMuaXNWYWx1ZUNoYW5nZWRCeVNjYW4gPyBcIm9wY29yZS1pbnB1dC1hY3RpdmVcIiA6IFwiXCI7XG4gICAgY29uc3QgaW5wdXRDbGFzcyA9IHNob3dRUkltYWdlID8gXCJ3aWRnZXQtb3Bjb3JldGV4dGJveC1wYWRkaW5nLXJpZ2h0XCIgOiBcIlwiO1xuICAgIGNvbnN0IGNsYXNzTmFtZXNJbnAgPSBjbGFzc05hbWVzKFwiZm9ybS1jb250cm9sXCIsIGlucHV0Q2xhc3MpO1xuICAgIGNvbnN0IG1heExlbmd0aElucHV0ID0gZ2V0TWF4TGVuZ3RoKCk7XG4gICAgY29uc3QgdXNlcklucHV0TWFzayA9IGlucHV0TWFzayAhPT0gdW5kZWZpbmVkICYmIGlucHV0TWFzay50cmltKCkgIT09ICcnID8gaW5wdXRNYXNrIDogJydcblxuICAgIGNvbnN0IFtzdGF0ZSwgc2V0U3RhdGVdID0gdXNlU3RhdGU8SW5wdXRFbGVtZW50U3RhdGU+KHtcbiAgICAgICAgZWRpdGVkVmFsdWU6IHVuZGVmaW5lZCwgaXNGb2N1c2VkOiBmYWxzZSwgaGFzRXJyb3I6IHByb3BzLmhhc0Vycm9yXG4gICAgfSk7XG5cbiAgICB1c2VFZmZlY3QoKCkgPT5cbiAgICAgICAgc2V0U3RhdGUoKHByZXZTdGF0ZSkgPT4gKHsgLi4ucHJldlN0YXRlLCBlZGl0ZWRWYWx1ZTogdW5kZWZpbmVkIH0pKVxuICAgICAgICAsIFt2YWx1ZV0pO1xuXG4gICAgZnVuY3Rpb24gZ2V0Q3VycmVudFZhbHVlKCk6IHN0cmluZyB8IEJpZyB7XG4gICAgICAgIHJldHVybiBzdGF0ZS5lZGl0ZWRWYWx1ZSAhPT0gdW5kZWZpbmVkID8gc3RhdGUuZWRpdGVkVmFsdWUgOiB2YWx1ZSAhPT0gdW5kZWZpbmVkID8gdmFsdWUgOiAnJztcbiAgICB9XG5cbiAgICBmdW5jdGlvbiBnZXRDdXJyZW50RGlzcGxheVZhbHVlKCk6IHN0cmluZyB8IEJpZyB7XG4gICAgICAgIGxldCBjdXJyZW50VmFsdWUgPSBnZXRDdXJyZW50VmFsdWUoKS50b1N0cmluZygpO1xuXG4gICAgICAgIGlmIChwcm9wcy5hdHRyaWJ1dGVUeXBlID09PSBcImRlY2ltYWxcIikge1xuICAgICAgICAgICAgcmV0dXJuIGdldEZvcm1hdHRlZERlY2ltYWxTdHJpbmcoY3VycmVudFZhbHVlLCBwcm9wcy5hcHBseUNoYW5nZSAhPT0gdW5kZWZpbmVkLCBzdGF0ZS5pc0ZvY3VzZWQgPz8gZmFsc2UpO1xuICAgICAgICB9IGVsc2UgaWYgKHByb3BzLmF0dHJpYnV0ZVR5cGUgPT09IFwiaW50ZWdlclwiKSB7XG4gICAgICAgICAgICByZXR1cm4gZ2V0Rm9ybWF0dGVkSW50ZWdlclN0cmluZyhjdXJyZW50VmFsdWUsIHN0YXRlLmlzRm9jdXNlZCA/PyBmYWxzZSk7XG4gICAgICAgIH0gZWxzZSBpZiAocHJvcHMuYXR0cmlidXRlVHlwZSA9PT0gXCJsb25nXCIpIHtcbiAgICAgICAgICAgIHJldHVybiBnZXRGb3JtYXR0ZWRMb25nU3RyaW5nKGN1cnJlbnRWYWx1ZSwgc3RhdGUuaXNGb2N1c2VkID8/IGZhbHNlKTtcbiAgICAgICAgfVxuXG4gICAgICAgIHJldHVybiBjdXJyZW50VmFsdWU7XG4gICAgfVxuXG4gICAgY29uc3QgZ2V0VmFsdWVCeUF0dHJpYnV0ZVR5cGUgPSAodmFsdWU6IHN0cmluZyk6IHsgaXNWYWxpZDogYm9vbGVhbiwgcGFyc2VkVmFsdWU/OiBzdHJpbmcgfCBCaWcsIHBhcnNlZFZhbHVlQmlnPzogQmlnIH0gPT4ge1xuICAgICAgICBpZiAocHJvcHMuYXR0cmlidXRlVHlwZSA9PT0gXCJkZWNpbWFsXCIpIHtcbiAgICAgICAgICAgIGlmICh2YWx1ZSA9PT0gJycpIHtcbiAgICAgICAgICAgICAgICByZXR1cm4geyBpc1ZhbGlkOiB0cnVlLCBwYXJzZWRWYWx1ZTogJycgfTtcbiAgICAgICAgICAgIH1cbiAgICAgICAgICAgIGxldCBkZWNpbWFsVmFsdWU6IEJpZztcbiAgICAgICAgICAgIHRyeSB7XG4gICAgICAgICAgICAgICAgZGVjaW1hbFZhbHVlID0gbmV3IEJpZyh2YWx1ZSk7XG4gICAgICAgICAgICB9IGNhdGNoIChlcnJvcikge1xuICAgICAgICAgICAgICAgIHJldHVybiB7IGlzVmFsaWQ6IGZhbHNlLCBwYXJzZWRWYWx1ZTogdmFsdWUgfTtcbiAgICAgICAgICAgIH1cbiAgICAgICAgICAgIGxldCBkZWNpbWFsVmFsdWVQYXJzZWQ7XG4gICAgICAgICAgICBsZXQgZGVjaW1hbE1vZGUgPSBwcm9wcy5kZWNpbWFsTW9kZTtcbiAgICAgICAgICAgIGxldCBkZWNpbWFsUHJlY2lzaW9uID0gcHJvcHMuZGVjaW1hbFByZWNpc2lvbjtcblxuICAgICAgICAgICAgaWYgKGRlY2ltYWxNb2RlID09PSBcImZpeGVkXCIpIHtcbiAgICAgICAgICAgICAgICBpZiAoZGVjaW1hbFByZWNpc2lvbiAhPT0gdW5kZWZpbmVkICYmIGRlY2ltYWxQcmVjaXNpb24gIT09IG51bGwpIHtcbiAgICAgICAgICAgICAgICAgICAgLy8gRmluZCB0aGUgbnVtYmVyIG9mIGRpZ2l0cyBiZWZvcmUgdGhlIGRlY2ltYWwgcG9pbnRcbiAgICAgICAgICAgICAgICAgICAgY29uc3QgaW50ZWdlclBhcnQgPSB2YWx1ZS5zcGxpdChcIi5cIilbMF07XG4gICAgICAgICAgICAgICAgICAgIGNvbnN0IGRpZ2l0c0JlZm9yZURlY2ltYWwgPSBwYXJzZUludChpbnRlZ2VyUGFydCkgPD0gMCA/IGludGVnZXJQYXJ0Lmxlbmd0aCAtIDEgOiBpbnRlZ2VyUGFydC5sZW5ndGg7XG4gICAgICAgICAgICAgICAgICAgIGRlY2ltYWxWYWx1ZVBhcnNlZCA9IGRlY2ltYWxWYWx1ZS50b1ByZWNpc2lvbihkZWNpbWFsUHJlY2lzaW9uICsgZGlnaXRzQmVmb3JlRGVjaW1hbCk7XG4gICAgICAgICAgICAgICAgfVxuICAgICAgICAgICAgfVxuICAgICAgICAgICAgZWxzZSBpZiAoZGVjaW1hbE1vZGUgPT09IFwiYXV0b1wiKSB7XG4gICAgICAgICAgICAgICAgZGVjaW1hbFZhbHVlUGFyc2VkID0gZGVjaW1hbFZhbHVlLnRvU3RyaW5nKCk7XG4gICAgICAgICAgICB9XG4gICAgICAgICAgICByZXR1cm4geyBpc1ZhbGlkOiB0cnVlLCBwYXJzZWRWYWx1ZTogbmV3IEJpZyhkZWNpbWFsVmFsdWVQYXJzZWQgPz8gJycpLCBwYXJzZWRWYWx1ZUJpZzogZGVjaW1hbFZhbHVlIH07XG4gICAgICAgIH1cbiAgICAgICAgZWxzZSBpZiAocHJvcHMuYXR0cmlidXRlVHlwZSA9PT0gXCJpbnRlZ2VyXCIpIHtcbiAgICAgICAgICAgIGlmICh2YWx1ZSA9PT0gJycpIHtcbiAgICAgICAgICAgICAgICByZXR1cm4geyBpc1ZhbGlkOiB0cnVlLCBwYXJzZWRWYWx1ZTogJycgfTtcbiAgICAgICAgICAgIH1cbiAgICAgICAgICAgIGNvbnN0IGludGVnZXJSZWdleCA9IC9eLT9cXGR7MSwxMH0kLztcbiAgICAgICAgICAgIGlmICghaW50ZWdlclJlZ2V4LnRlc3QodmFsdWUpKSB7XG4gICAgICAgICAgICAgICAgcmV0dXJuIHsgaXNWYWxpZDogZmFsc2UsIHBhcnNlZFZhbHVlOiB2YWx1ZSB9O1xuICAgICAgICAgICAgfVxuICAgICAgICAgICAgcmV0dXJuIHsgaXNWYWxpZDogdHJ1ZSwgcGFyc2VkVmFsdWU6IG5ldyBCaWcodmFsdWUpIH07XG4gICAgICAgIH1cbiAgICAgICAgZWxzZSBpZiAocHJvcHMuYXR0cmlidXRlVHlwZSA9PT0gXCJsb25nXCIpIHtcbiAgICAgICAgICAgIGlmICh2YWx1ZSA9PT0gJycpIHtcbiAgICAgICAgICAgICAgICByZXR1cm4geyBpc1ZhbGlkOiB0cnVlLCBwYXJzZWRWYWx1ZTogJycgfTtcbiAgICAgICAgICAgIH1cbiAgICAgICAgICAgIGNvbnN0IGxvbmdSZWdleCA9IC9eLT9cXGR7MSwxOX0kLztcbiAgICAgICAgICAgIGlmICghbG9uZ1JlZ2V4LnRlc3QodmFsdWUpKSB7XG4gICAgICAgICAgICAgICAgcmV0dXJuIHsgaXNWYWxpZDogZmFsc2UsIHBhcnNlZFZhbHVlOiB2YWx1ZSB9O1xuICAgICAgICAgICAgfVxuICAgICAgICAgICAgcmV0dXJuIHsgaXNWYWxpZDogdHJ1ZSwgcGFyc2VkVmFsdWU6IG5ldyBCaWcodmFsdWUpIH07XG4gICAgICAgIH1cblxuICAgICAgICByZXR1cm4geyBpc1ZhbGlkOiB0cnVlLCBwYXJzZWRWYWx1ZTogdmFsdWUgfTtcbiAgICB9XG5cbiAgICBjb25zdCBnZXRGb3JtYXR0ZWREZWNpbWFsU3RyaW5nID0gKHZhbHVlOiBzdHJpbmcsXG4gICAgICAgIGFwcGx5Q2hhbmdlV2hpbGVFZGl0aW5nOiBib29sZWFuLCBpc0ZvY3VzZWQ6IGJvb2xlYW4pOiBzdHJpbmcgPT4ge1xuICAgICAgICBpZiAodmFsdWUgPT09ICcnIHx8IHN0YXRlLmhhc0Vycm9yKSB7XG4gICAgICAgICAgICByZXR1cm4gdmFsdWU7XG4gICAgICAgIH1cbiAgICAgICAgY29uc3QgW2ludGVnZXJQYXJ0LCBkZWNpbWFsUGFydF0gPSB2YWx1ZS5zcGxpdChcIi5cIik7XG4gICAgICAgIGxldCBncm91cEZvcm1hdHRlZEludGVnZXJQYXJ0ID0gJyc7XG4gICAgICAgIGxldCBkaWdpdHNCZWZvcmVEZWNpbWFsID0gcGFyc2VJbnQoaW50ZWdlclBhcnQpIDw9IDAgPyBpbnRlZ2VyUGFydC5sZW5ndGggLSAxIDogaW50ZWdlclBhcnQubGVuZ3RoO1xuICAgICAgICBsZXQgZGVjaW1hbFByZWNpc2lvbiA9IHByb3BzLmRlY2ltYWxNb2RlID09PSBcImZpeGVkXCIgPyBwcm9wcy5kZWNpbWFsUHJlY2lzaW9uIDogcHJvcHMuZGVjaW1hbE1vZGUgPT09IFwiYXV0b1wiID8gZGVjaW1hbFBhcnQ/Lmxlbmd0aCA6IHVuZGVmaW5lZDtcbiAgICAgICAgaWYgKGRlY2ltYWxQcmVjaXNpb24gICE9PSB1bmRlZmluZWQpIHtcbiAgICAgICAgICAgIGRlY2ltYWxQcmVjaXNpb24gPSBwYXJzZUludChpbnRlZ2VyUGFydCkgPT09IDAgPyBkZWNpbWFsUHJlY2lzaW9uIC0gY291bnRMZWFkaW5nWmVyb3MoZGVjaW1hbFBhcnQpIDogZGVjaW1hbFByZWNpc2lvbjtcbiAgICAgICAgICAgIGlmKGludGVnZXJQYXJ0ID09PSAnLTAnICYmIGRlY2ltYWxQcmVjaXNpb24gPiAwKVxuICAgICAgICAgICAgICAgIGRlY2ltYWxQcmVjaXNpb24tLTtcbiAgICAgICAgfVxuICAgICAgICBsZXQgZm9ybWF0dGVkRGVjaW1hbCA9IG5ldyBCaWcoYCR7aW50ZWdlclBhcnQgPz8gMH0uJHtkZWNpbWFsUGFydCA/PyAwfWApO1xuICAgICAgICBsZXQgZm9ybWF0dGVkRGVjaW1hbFN0cmluZyA9IGZvcm1hdHRlZERlY2ltYWwudG9QcmVjaXNpb24oZGVjaW1hbFByZWNpc2lvbiA/IFxuICAgICAgICAgICAgKGRlY2ltYWxQcmVjaXNpb24gKyBkaWdpdHNCZWZvcmVEZWNpbWFsKSA6IHVuZGVmaW5lZCk7XG4gICAgICAgIGxldCBbZm9ybWF0dGVkSW50ZWdlclBhcnQsIGZvcm1hdHRlZERlY2ltYWxQYXJ0XSA9IGZvcm1hdHRlZERlY2ltYWxTdHJpbmcuc3BsaXQoXCIuXCIpO1xuICAgICAgICBmb3JtYXR0ZWRJbnRlZ2VyUGFydCA9IGludGVnZXJQYXJ0ID09PSAnLTAnID8gJy0wJyA6IGZvcm1hdHRlZEludGVnZXJQYXJ0O1xuICAgICAgICBpZiAoYXBwbHlDaGFuZ2VXaGlsZUVkaXRpbmcgfHwgKCFhcHBseUNoYW5nZVdoaWxlRWRpdGluZyAmJiAhaXNGb2N1c2VkKSkge1xuICAgICAgICAgICAgaWYgKHByb3BzLmdyb3VwRGlnaXRzICYmIGZvcm1hdHRlZEludGVnZXJQYXJ0Lmxlbmd0aCA+IDMpIHtcbiAgICAgICAgICAgICAgICBncm91cEZvcm1hdHRlZEludGVnZXJQYXJ0ID0gZm9ybWF0dGVkSW50ZWdlclBhcnQucmVwbGFjZSgvKFxcZCkoPz0oXFxkezN9KSsoPyFcXGQpKS9nLCBcIiQxLFwiKTtcbiAgICAgICAgICAgICAgICByZXR1cm4gZm9ybWF0dGVkRGVjaW1hbFBhcnQgPyBgJHtncm91cEZvcm1hdHRlZEludGVnZXJQYXJ0fS4ke2Zvcm1hdHRlZERlY2ltYWxQYXJ0fWAgOiBncm91cEZvcm1hdHRlZEludGVnZXJQYXJ0O1xuICAgICAgICAgICAgfSBlbHNlIHtcbiAgICAgICAgICAgICAgICByZXR1cm4gZm9ybWF0dGVkRGVjaW1hbFBhcnQgPyBgJHtmb3JtYXR0ZWRJbnRlZ2VyUGFydH0uJHtmb3JtYXR0ZWREZWNpbWFsUGFydH1gIDogZm9ybWF0dGVkSW50ZWdlclBhcnQ7XG4gICAgICAgICAgICB9XG4gICAgICAgIH0gZWxzZSBpZiAoaXNGb2N1c2VkKSB7XG4gICAgICAgICAgICByZXR1cm4gZGVjaW1hbFBhcnQgIT09IHVuZGVmaW5lZCA/IGAke2Zvcm1hdHRlZEludGVnZXJQYXJ0fS4ke2RlY2ltYWxQYXJ0fWAgOiBpbnRlZ2VyUGFydDtcbiAgICAgICAgfSBlbHNlIHtcbiAgICAgICAgICAgIHJldHVybiBmb3JtYXR0ZWREZWNpbWFsUGFydCA/IGAke2Zvcm1hdHRlZEludGVnZXJQYXJ0fS4ke2Zvcm1hdHRlZERlY2ltYWxQYXJ0fWAgOiBmb3JtYXR0ZWRJbnRlZ2VyUGFydDtcbiAgICAgICAgfVxuICAgIH1cblxuICAgIGNvbnN0IGdldEZvcm1hdHRlZEludGVnZXJTdHJpbmcgPSAodmFsdWU6IHN0cmluZywgaXNGb2N1c2VkOiBib29sZWFuKTogc3RyaW5nID0+IHtcbiAgICAgICAgaWYgKHN0YXRlLmhhc0Vycm9yKVxuICAgICAgICAgICAgcmV0dXJuIHZhbHVlO1xuICAgICAgICBsZXQgZ3JvdXBGb3JtYXR0ZWRJbnRlZ2VyUGFydCA9ICcnO1xuICAgICAgICBpZiAocHJvcHMuZ3JvdXBEaWdpdHMgJiYgdmFsdWUubGVuZ3RoID4gMyAmJiAhaXNGb2N1c2VkKSB7XG4gICAgICAgICAgICBncm91cEZvcm1hdHRlZEludGVnZXJQYXJ0ID0gdmFsdWUucmVwbGFjZSgvKFxcZCkoPz0oXFxkezN9KSsoPyFcXGQpKS9nLCBcIiQxLFwiKTtcbiAgICAgICAgICAgIHJldHVybiBncm91cEZvcm1hdHRlZEludGVnZXJQYXJ0O1xuICAgICAgICB9IGVsc2VcbiAgICAgICAgICAgIHJldHVybiB2YWx1ZTtcbiAgICB9XG5cbiAgICBjb25zdCBnZXRGb3JtYXR0ZWRMb25nU3RyaW5nID0gKHZhbHVlOiBzdHJpbmcsIGlzRm9jdXNlZDogYm9vbGVhbik6IHN0cmluZyA9PiB7XG4gICAgICAgIGlmIChzdGF0ZS5oYXNFcnJvcilcbiAgICAgICAgICAgIHJldHVybiB2YWx1ZTtcbiAgICAgICAgbGV0IGdyb3VwRm9ybWF0dGVkTG9uZ1BhcnQgPSAnJztcbiAgICAgICAgaWYgKHByb3BzLmdyb3VwRGlnaXRzICYmIHZhbHVlLmxlbmd0aCA+IDMgJiYgIWlzRm9jdXNlZCkge1xuICAgICAgICAgICAgZ3JvdXBGb3JtYXR0ZWRMb25nUGFydCA9IHZhbHVlLnJlcGxhY2UoLyhcXGQpKD89KFxcZHszfSkrKD8hXFxkKSkvZywgXCIkMSxcIik7XG4gICAgICAgICAgICByZXR1cm4gZ3JvdXBGb3JtYXR0ZWRMb25nUGFydDtcbiAgICAgICAgfSBlbHNlXG4gICAgICAgICAgICByZXR1cm4gdmFsdWU7XG4gICAgfVxuXG4gICAgZnVuY3Rpb24gY291bnRMZWFkaW5nWmVyb3ModmFsdWUgOiBzdHJpbmcgfCBCaWcpOiBudW1iZXIge1xuICAgICAgICBjb25zdCBzdHIgPSB2YWx1ZS50b1N0cmluZygpO1xuICAgICAgICBsZXQgY291bnQgPSAwO1xuICAgICAgICBmb3IgKGxldCBpID0gMDsgaSA8IHN0ci5sZW5ndGg7IGkrKykge1xuICAgICAgICAgICAgaWYgKHN0cltpXSA9PT0gJzAnKSB7XG4gICAgICAgICAgICAgICAgY291bnQrKztcbiAgICAgICAgICAgIH0gZWxzZSB7XG4gICAgICAgICAgICAgICAgYnJlYWs7XG4gICAgICAgICAgICB9XG4gICAgICAgIH1cbiAgICAgICAgcmV0dXJuIGNvdW50O1xuICAgIH1cblxuICAgIGZ1bmN0aW9uIGdldE1heExlbmd0aCgpOiBudW1iZXIge1xuICAgICAgICBpZiAobWF4TGVuZ3RoVHlwZSA9PT0gXCJkZWZhdWx0XCIpXG4gICAgICAgICAgICByZXR1cm4gMjAwO1xuICAgICAgICBpZiAobWF4TGVuZ3RoVHlwZSA9PT0gXCJjdXN0b21cIilcbiAgICAgICAgICAgIHJldHVybiBwcm9wcy5tYXhMZW5ndGg7XG4gICAgICAgIGVsc2VcbiAgICAgICAgICAgIHJldHVybiAwO1xuICAgIH1cblxuICAgIGNvbnN0IG9uQmx1ciA9ICgpOiB2b2lkID0+IHtcbiAgICAgICAgbGV0IGN1cnJlbnRWYWx1ZSA9IGdldEN1cnJlbnRWYWx1ZSgpO1xuICAgICAgICBsZXQgdmFsdWVUb0JlU2V0ID0gZ2V0VmFsdWVCeUF0dHJpYnV0ZVR5cGUoY3VycmVudFZhbHVlLnRvU3RyaW5nKCkpO1xuICAgICAgICBpZiAodmFsdWVUb0JlU2V0LmlzVmFsaWQgJiYgdmFsdWVUb0JlU2V0LnBhcnNlZFZhbHVlICE9PSB1bmRlZmluZWQpIHtcbiAgICAgICAgICAgIG9uTGVhdmU/Lih2YWx1ZVRvQmVTZXQucGFyc2VkVmFsdWUsIHZhbHVlVG9CZVNldC5wYXJzZWRWYWx1ZS50b1N0cmluZygpICE9PSB2YWx1ZSwgZmFsc2UsIGZhbHNlKTtcbiAgICAgICAgfVxuICAgICAgICBlbHNlXG4gICAgICAgICAgICBvbkxlYXZlPy4oY3VycmVudFZhbHVlLCBmYWxzZSwgZmFsc2UsIHRydWUpO1xuXG4gICAgICAgIGlmICh2YWx1ZVRvQmVTZXQuaXNWYWxpZClcbiAgICAgICAgICAgIHNldFN0YXRlKHtcbiAgICAgICAgICAgICAgICBlZGl0ZWRWYWx1ZTogdmFsdWVUb0JlU2V0LmlzVmFsaWQgPyB1bmRlZmluZWQgOiB2YWx1ZVRvQmVTZXQucGFyc2VkVmFsdWU/LnRvU3RyaW5nKCksXG4gICAgICAgICAgICAgICAgaXNGb2N1c2VkOiBmYWxzZSwgaGFzRXJyb3I6ICF2YWx1ZVRvQmVTZXQuaXNWYWxpZFxuICAgICAgICAgICAgfSk7XG4gICAgfVxuXG4gICAgY29uc3Qgb25FbnRlckhhbmRsZSA9ICgpOiB2b2lkID0+IHtcbiAgICAgICAgc2V0U3RhdGUoKHByZXZTdGF0ZSkgPT4gKHsgLi4ucHJldlN0YXRlLCBpc0ZvY3VzZWQ6IHRydWUgfSkpO1xuICAgICAgICBvbkVudGVyPy4odHJ1ZSk7XG4gICAgfVxuXG4gICAgY29uc3Qgb25FbnRlcktleVByZXNzSGFuZGxlciA9IChldmVudDogUmVhY3QuS2V5Ym9hcmRFdmVudDxIVE1MSW5wdXRFbGVtZW50Pik6IHZvaWQgPT4geyAgXG4gICAgICAgIGV2ZW50LnByZXZlbnREZWZhdWx0KCk7ICBcbiAgICAgICAgbGV0IGN1cnJlbnRWYWx1ZSA9IGdldEN1cnJlbnRWYWx1ZSgpO1xuICAgICAgICBsZXQgdmFsdWVUb0JlU2V0ID0gZ2V0VmFsdWVCeUF0dHJpYnV0ZVR5cGUoY3VycmVudFZhbHVlLnRvU3RyaW5nKCkpO1xuICAgICAgICBpZiAodmFsdWVUb0JlU2V0LmlzVmFsaWQgJiYgdmFsdWVUb0JlU2V0LnBhcnNlZFZhbHVlICE9PSB1bmRlZmluZWQpIHtcbiAgICAgICAgICAgIG9uRW50ZXJLZXlQcmVzcz8uKHZhbHVlVG9CZVNldC5wYXJzZWRWYWx1ZSwgdmFsdWVUb0JlU2V0LnBhcnNlZFZhbHVlLnRvU3RyaW5nKCkgIT09IHZhbHVlLCBmYWxzZSk7XG4gICAgICAgIH1cbiAgICAgICAgZWxzZVxuICAgICAgICAgICAgb25FbnRlcktleVByZXNzPy4oY3VycmVudFZhbHVlLCBmYWxzZSwgdHJ1ZSk7XG5cbiAgICB9XG5cbiAgICBjb25zdCBvbkNoYW5nZUhhbmRsZSA9IChldmVudDogQ2hhbmdlRXZlbnQ8SFRNTElucHV0RWxlbWVudD4sIGlzSW5wdXRNYXNrVmFsdWU6IGJvb2xlYW4gPSBmYWxzZSwgdmFsdWU6IHN0cmluZyA9ICcnKTogdm9pZCA9PiB7XG4gICAgICAgIHZhciB2YWx1ZVNob3VsZEJlU2V0ID0gZmFsc2U7XG4gICAgICAgIHZhciB2YWx1ZVRvQmVTZXQgPSBpc0lucHV0TWFza1ZhbHVlID8gdmFsdWUgOiBldmVudC5jdXJyZW50VGFyZ2V0LnZhbHVlO1xuICAgICAgICBpZiAobWF4TGVuZ3RoSW5wdXQgPT09IDAgfHwgdmFsdWVUb0JlU2V0Lmxlbmd0aCA8PSBtYXhMZW5ndGhJbnB1dClcbiAgICAgICAgICAgIHZhbHVlU2hvdWxkQmVTZXQgPSB0cnVlO1xuXG4gICAgICAgIGlmICh2YWx1ZVNob3VsZEJlU2V0KSB7XG4gICAgICAgICAgICBsZXQgeyBpc1ZhbGlkLCBwYXJzZWRWYWx1ZSB9ID0gZ2V0VmFsdWVCeUF0dHJpYnV0ZVR5cGUodmFsdWVUb0JlU2V0KTtcbiAgICAgICAgICAgIHNldFN0YXRlKChwcmV2U3RhdGUpID0+ICh7IC4uLnByZXZTdGF0ZSwgZWRpdGVkVmFsdWU6IHZhbHVlVG9CZVNldCwgaGFzRXJyb3I6ICFpc1ZhbGlkIH0pKTtcbiAgICAgICAgICAgIGlmIChwcm9wcy5hcHBseUNoYW5nZSAhPT0gdW5kZWZpbmVkKSB7XG4gICAgICAgICAgICAgICAgaWYgKGlzVmFsaWQgJiYgcGFyc2VkVmFsdWUgIT09IHVuZGVmaW5lZCkge1xuICAgICAgICAgICAgICAgICAgICBwcm9wcy5hcHBseUNoYW5nZShwYXJzZWRWYWx1ZSk7XG4gICAgICAgICAgICAgICAgfVxuICAgICAgICAgICAgfVxuICAgICAgICB9XG4gICAgfVxuXG4gICAgY29uc3QgaW5wdXRTdHlsZTogQ1NTUHJvcGVydGllcyA9IHtcbiAgICAgICAgd2lkdGg6IFwiMTAwJVwiXG4gICAgfVxuXG4gICAgZnVuY3Rpb24gZ2V0SW5wdXRDbGFzc05hbWVzKCk6IHN0cmluZyB7XG4gICAgICAgIGxldCBjbGFzc05hbWVzUmVzdWx0ID0gcHJvcHMuaXNJbnB1dFFSSW1hZ2VDbGljayB8fCBwcm9wcy5pc1ZhbHVlQ2hhbmdlZEJ5U2NhbiA/IGNsYXNzTmFtZXMoY2xhc3NOYW1lc0lucCwgaW5wdXRBY3RpdmVDbGFzcykgOiBjbGFzc05hbWVzSW5wO1xuICAgICAgICBzZXRUaW1lb3V0KCgpID0+IHtcbiAgICAgICAgICAgIHJldHVybiBjbGFzc05hbWVzUmVzdWx0O1xuICAgICAgICB9LCA1MDApO1xuICAgICAgICByZXR1cm4gY2xhc3NOYW1lc1Jlc3VsdDtcbiAgICB9XG5cbiAgICByZXR1cm4gKFxuICAgICAgICB1c2VySW5wdXRNYXNrID09PSAnJyA/XG4gICAgICAgICAgICA8aW5wdXRcbiAgICAgICAgICAgICAgICByZWY9e25vZGUgPT4ge1xuICAgICAgICAgICAgICAgICAgICBpZiAocHJvcHMuaW5wdXRSZWYpIHtcbiAgICAgICAgICAgICAgICAgICAgICAgIHByb3BzLmlucHV0UmVmLmN1cnJlbnQgPSBub2RlID8/IHVuZGVmaW5lZDtcbiAgICAgICAgICAgICAgICAgICAgfVxuICAgICAgICAgICAgICAgIH19XG4gICAgICAgICAgICAgICAgc3R5bGU9e2lucHV0U3R5bGV9XG4gICAgICAgICAgICAgICAgaWQ9e3Byb3BzLmlkfVxuICAgICAgICAgICAgICAgIGNsYXNzTmFtZT17Z2V0SW5wdXRDbGFzc05hbWVzKCl9XG4gICAgICAgICAgICAgICAgdGFiSW5kZXg9e3RhYkluZGV4fVxuICAgICAgICAgICAgICAgIHZhbHVlPXtnZXRDdXJyZW50RGlzcGxheVZhbHVlKCkudG9TdHJpbmcoKX1cbiAgICAgICAgICAgICAgICB0eXBlPXtwcm9wcy5pbnB1dFR5cGUgPyBcInBhc3N3b3JkXCIgOiBcInRleHRcIn1cbiAgICAgICAgICAgICAgICBtYXhMZW5ndGg9e21heExlbmd0aElucHV0ID09PSAwID8gdW5kZWZpbmVkIDogbWF4TGVuZ3RoSW5wdXR9XG4gICAgICAgICAgICAgICAgb25Gb2N1cz17b25FbnRlckhhbmRsZX1cbiAgICAgICAgICAgICAgICBvbktleURvd249eyhldmVudCkgPT4ge1xuICAgICAgICAgICAgICAgICAgICBpZiAoZXZlbnQua2V5ID09PSBcIkVudGVyXCIpIHtcbiAgICAgICAgICAgICAgICAgICAgICAgIG9uRW50ZXJLZXlQcmVzc0hhbmRsZXIoZXZlbnQpO1xuICAgICAgICAgICAgICAgICAgICB9XG4gICAgICAgICAgICAgICAgfX1cbiAgICAgICAgICAgICAgICBwbGFjZWhvbGRlcj17cGxhY2VIb2xkZXJJbnB1dH1cbiAgICAgICAgICAgICAgICBvbkJsdXI9e29uQmx1cn1cbiAgICAgICAgICAgICAgICBvbkNoYW5nZT17KGV2ZW50KSA9PiBvbkNoYW5nZUhhbmRsZShldmVudCl9XG4gICAgICAgICAgICAgICAgZGlzYWJsZWQ9e2Rpc2FibGVkfVxuICAgICAgICAgICAgICAgIHsuLi5wcm9wcy5hcmlhTGFiZWwgPyB7IFwiYXJpYS1sYWJlbFwiOiBwcm9wcy5hcmlhTGFiZWwgfSA6IHt9fVxuICAgICAgICAgICAgICAgIHsuLi5wcm9wcy5oYXNFcnJvciB8fCBzdGF0ZS5oYXNFcnJvciA/IHsgXCJhcmlhLWludmFsaWRcIjogdHJ1ZSB9IDoge319XG4gICAgICAgICAgICAgICAgey4uLnByb3BzLmFyaWFSZXF1aXJlZCA/IHsgXCJhcmlhLXJlcXVpcmVkXCI6IHRydWUgfSA6IHt9fVxuICAgICAgICAgICAgICAgIHsuLi5wcm9wcy5yZXF1aXJlZCA/IHsgcmVxdWlyZWQ6IHRydWUgfSA6IHt9fVxuICAgICAgICAgICAgICAgIGF1dG9Db21wbGV0ZT17cHJvcHMuYXV0b0NvbXBsZXRlLnJlcGxhY2VBbGwoXCJfXCIsIFwiLVwiKX1cbiAgICAgICAgICAgIC8+IDpcbiAgICAgICAgICAgIDxJbnB1dE1hc2tFbGVtZW50XG4gICAgICAgICAgICAgICAgcmVmPXtwcm9wcy5pbnB1dFJlZn1cbiAgICAgICAgICAgICAgICBrZXk9e3ZhbHVlfVxuICAgICAgICAgICAgICAgIGlkPXtwcm9wcy5pZH1cbiAgICAgICAgICAgICAgICBzdHlsZT17aW5wdXRTdHlsZX1cbiAgICAgICAgICAgICAgICBjbGFzc05hbWU9e2dldElucHV0Q2xhc3NOYW1lcygpfVxuICAgICAgICAgICAgICAgIHRhYkluZGV4PXt0YWJJbmRleH1cbiAgICAgICAgICAgICAgICBzaG93QXNQYXNzb3dyZD17cHJvcHMuaW5wdXRUeXBlfVxuICAgICAgICAgICAgICAgIG1hc2s9e3VzZXJJbnB1dE1hc2t9XG4gICAgICAgICAgICAgICAgbWF4TGVuZ3RoPXttYXhMZW5ndGhJbnB1dCA9PT0gMCA/IHVuZGVmaW5lZCA6IG1heExlbmd0aElucHV0fVxuICAgICAgICAgICAgICAgIHZhbHVlPXtnZXRDdXJyZW50RGlzcGxheVZhbHVlKCkudG9TdHJpbmcoKX1cbiAgICAgICAgICAgICAgICBwbGFjZUhvbGRlckNoYXI9XCJfXCJcbiAgICAgICAgICAgICAgICBwbGFjZUhvbGRlcklucHV0PXtwbGFjZUhvbGRlcklucHV0fVxuICAgICAgICAgICAgICAgIG9uQ2hhbmdlSGFuZGxlcj17b25DaGFuZ2VIYW5kbGV9XG4gICAgICAgICAgICAgICAgb25FbnRlcktleVByZXNzPXsoZXZlbnQpID0+IHtvbkVudGVyS2V5UHJlc3NIYW5kbGVyKGV2ZW50KX19XG4gICAgICAgICAgICAgICAgb25Gb2N1cz17b25FbnRlckhhbmRsZX1cbiAgICAgICAgICAgICAgICBvbkJsdXI9e29uQmx1cn1cbiAgICAgICAgICAgICAgICBkaXNhYmxlZD17ZGlzYWJsZWR9XG4gICAgICAgICAgICAgICAgYXJpYUxhYmVsPXtwcm9wcy5hcmlhTGFiZWx9XG4gICAgICAgICAgICAgICAgYXJpYUludmFsaWQ9e3Byb3BzLmhhc0Vycm9yIHx8IHN0YXRlLmhhc0Vycm9yfVxuICAgICAgICAgICAgICAgIGFyaWFSZXF1aXJlZD17YXJpYVJlcXVpcmVkfVxuICAgICAgICAgICAgICAgIHJlcXVpcmVkPXtwcm9wcy5yZXF1aXJlZH1cbiAgICAgICAgICAgICAgICBhdXRvQ29tcGxldGU9e3Byb3BzLmF1dG9Db21wbGV0ZS5yZXBsYWNlQWxsKFwiX1wiLCBcIi1cIil9XG4gICAgICAgICAgICA+PC9JbnB1dE1hc2tFbGVtZW50PlxuICAgICk7XG59XG5cbmV4cG9ydCBkZWZhdWx0IElucHV0RWxlbWVudDtcbiIsImV4cG9ydCBkZWZhdWx0IFwid2lkZ2V0cy9tZW5kaXgvb3BjZW50ZXJjb3JldGV4dGJveC9hc3NldHMvODI5NjI0ZWFjNmYyODM1ZS5zdmdcIiIsImltcG9ydCB7IEZ1bmN0aW9uQ29tcG9uZW50LCBjcmVhdGVFbGVtZW50LCBDU1NQcm9wZXJ0aWVzIH0gZnJvbSBcInJlYWN0XCI7XG5pbXBvcnQgY21kUVJDb2RlIGZyb20gXCIuLi9pbWFnZXMvY21kUVJDb2RlLnN2Z1wiO1xuXG5cbmV4cG9ydCBpbnRlcmZhY2UgQmFyY29kZUltYWdlUHJvcHMge1xuICAgIHN0eWxlPzogQ1NTUHJvcGVydGllc1xuICAgIG9uQ2xpY2tIYW5kbGVyPzogKGlzUVJJbWFnZUNsaWNrZWQ6IGJvb2xlYW4pID0+IHZvaWQ7XG59XG5leHBvcnQgY29uc3QgQmFyY29kZUltYWdlOiBGdW5jdGlvbkNvbXBvbmVudDxCYXJjb2RlSW1hZ2VQcm9wcz4gPSAoeyBzdHlsZSwgb25DbGlja0hhbmRsZXIgfSkgPT5cblxuICAgIDxpbWdcbiAgICAgICAgc3R5bGU9e3N0eWxlfVxuICAgICAgICBjbGFzc05hbWU9XCJvcGNvcmUtcXItaW1hZ2VcIlxuICAgICAgICByb2xlPXtcImJ1dHRvblwifVxuICAgICAgICBzcmM9e2NtZFFSQ29kZX1cbiAgICAgICAgb25Nb3VzZURvd249eygpID0+IG9uQ2xpY2tIYW5kbGVyICE9PSB1bmRlZmluZWQgPyBvbkNsaWNrSGFuZGxlcih0cnVlKSA6IGZhbHNlfVxuICAgID48L2ltZz5cblxuIiwiaW1wb3J0IHsgUmVhY3RFbGVtZW50LCBjcmVhdGVFbGVtZW50IH0gZnJvbSBcInJlYWN0XCI7XG5pbXBvcnQgY2xhc3NOYW1lcyBmcm9tIFwiY2xhc3NuYW1lc1wiO1xuXG5leHBvcnQgaW50ZXJmYWNlIEFsZXJ0UHJvcHMge1xuICAgIGlkPzogc3RyaW5nO1xuICAgIG1lc3NhZ2U/OiBzdHJpbmc7XG4gICAgY2xhc3NOYW1lPzogc3RyaW5nO1xuICAgIGJvb3RzdHJhcFN0eWxlOiBcImRlZmF1bHRcIiB8IFwicHJpbWFyeVwiIHwgXCJzdWNjZXNzXCIgfCBcImluZm9cIiB8IFwiaW52ZXJzZVwiIHwgXCJ3YXJuaW5nXCIgfCBcImRhbmdlclwiO1xufVxuXG5leHBvcnQgZnVuY3Rpb24gQWxlcnQoeyBpZCwgbWVzc2FnZSwgY2xhc3NOYW1lLCBib290c3RyYXBTdHlsZSB9OiBBbGVydFByb3BzKTogUmVhY3RFbGVtZW50IHwgbnVsbCB7XG4gICAgcmV0dXJuIG1lc3NhZ2UgPyA8ZGl2IGlkPXtgJHtpZH0tZXJyb3JgfSByb2xlPVwiYWxlcnRcIiBjbGFzc05hbWU9e2NsYXNzTmFtZXMoYGFsZXJ0IGFsZXJ0LSR7Ym9vdHN0cmFwU3R5bGV9YCwgY2xhc3NOYW1lKX0+e21lc3NhZ2V9PC9kaXY+IDogbnVsbDtcbn1cbiIsImltcG9ydCB7IFJlYWN0RWxlbWVudCwgY3JlYXRlRWxlbWVudCwgdXNlRWZmZWN0LCB1c2VTdGF0ZSwgdXNlUmVmIH0gZnJvbSBcInJlYWN0XCI7XG5pbXBvcnQgeyBPcGNlbnRlckNvcmVUZXh0Qm94Q29udGFpbmVyUHJvcHMgfSBmcm9tIFwiLi4vdHlwaW5ncy9PcGNlbnRlckNvcmVUZXh0Qm94UHJvcHNcIjtcbmltcG9ydCB7IElucHV0RWxlbWVudCB9IGZyb20gXCIuL2NvbXBvbmVudHMvSW5wdXRFbGVtZW50XCI7XG5pbXBvcnQgeyBCYXJjb2RlSW1hZ2UgfSBmcm9tIFwiLi9jb21wb25lbnRzL0JhcmNvZGVJbWFnZVwiO1xuaW1wb3J0IFwiLi91aS9PcGNlbnRlckNvcmVUZXh0Qm94LmNzc1wiO1xuaW1wb3J0IHsgQWxlcnQgfSBmcm9tIFwiLi9jb21wb25lbnRzL0FsZXJ0XCI7XG5cbmludGVyZmFjZSBPcGNlbnRlckNvcmVUZXh0Qm94RXh0ZW5kZWRQcm9wcyB7XG4gICAgc2hvd0lucHV0UVJJbWFnZT86IGJvb2xlYW47XG4gICAgaXNJbnB1dFFSSW1hZ2VDbGljaz86IGJvb2xlYW47XG4gICAgaXNWYWx1ZUNoYW5nZUJ5U2Nhbj86IGJvb2xlYW47XG59XG5cbnZhciBpc0ZvY3VzRnJvbVFSSW1hZ2U6IGJvb2xlYW4gPSBmYWxzZTtcblxuZXhwb3J0IGZ1bmN0aW9uIE9wY2VudGVyQ29yZVRleHRCb3gocHJvcHM6IE9wY2VudGVyQ29yZVRleHRCb3hDb250YWluZXJQcm9wcyk6IFJlYWN0RWxlbWVudCB7XG4gICAgY29uc3QgeyB2YWx1ZUF0dHJpYnV0ZSwgc2hvd0FzUGFzc293cmQsIHBsYWNlaG9sZGVyLCBzaG93UXJJbWFnZVVzZXJJbnB1dCwgaW5wdXRNYXNrLCByZWFkT25seVN0eWxlLFxuICAgICAgICBvbkNsaWNrQWN0aW9uLCBvbkJhcmNvZGVTY2FuQWN0aW9uLCBzdWJtaXREZWxheSxcbiAgICAgICAgb25FbnRlckFjdGlvbiwgdGFiSW5kZXgsIG9uRW50ZXJLZXlBY3Rpb24sIG9uTGVhdmVBY3Rpb24gfSA9IHByb3BzO1xuXG4gICAgY29uc3QgdmFsaWRhdGlvbkZlZWRiYWNrID0gcHJvcHMudmFsdWVBdHRyaWJ1dGU/LnZhbGlkYXRpb247XG4gICAgY29uc3QgcmVxdWlyZWQgPSBwcm9wcy52YWxpZGF0aW9uUHJvcGVydHkgPT09IFwicmVxdWlyZWRcIiB8fCBwcm9wcy52YWxpZGF0aW9uUHJvcGVydHlGb3JEaWdpdCA9PT0gXCJyZXF1aXJlZFwiO1xuICAgIGNvbnN0IHN1Ym1pdERlbGF5VmFsdWUgPSBzdWJtaXREZWxheSA/IHN1Ym1pdERlbGF5ID49IDAgPyBzdWJtaXREZWxheSA6IDAgOiAwO1xuICAgIGNvbnN0IGlucHV0UmVmID0gdXNlUmVmPEhUTUxJbnB1dEVsZW1lbnQ+KCk7XG5cbiAgICBjb25zdCBbc3RhdGUsIHNldFN0YXRlXSA9XG4gICAgICAgIHVzZVN0YXRlPE9wY2VudGVyQ29yZVRleHRCb3hFeHRlbmRlZFByb3BzPih7XG4gICAgICAgICAgICBzaG93SW5wdXRRUkltYWdlOiBmYWxzZSxcbiAgICAgICAgICAgIGlzSW5wdXRRUkltYWdlQ2xpY2s6IGZhbHNlLCBpc1ZhbHVlQ2hhbmdlQnlTY2FuOiBmYWxzZVxuICAgICAgICB9KTtcblxuICAgIC8vIHVzZUVmZmVjdCgoKSA9PiB7XG4gICAgLy8gICAgIGNoZWNrVmFsaWRhdG9ycygpO1xuICAgIC8vIH0sIFtdKTtcblxuICAgIHVzZUVmZmVjdCgoKSA9PiB7XG4gICAgICAgIENoZWNrSWZBdHRyaWJ1dGVWYWx1ZUNoYW5nZWRCeVNjYW4oKTtcbiAgICAgICAgY2hlY2tWYWxpZGF0b3JzKCk7XG4gICAgfSwgW3ZhbHVlQXR0cmlidXRlPy52YWx1ZV0pO1xuXG4gICAgY29uc3QgY2hlY2tWYWxpZGF0b3JzID0gKCkgPT4ge1xuICAgICAgICBpZiAocHJvcHMudmFsaWRhdGlvblByb3BlcnR5ID09PSBcInJlcXVpcmVkXCIgfHwgcHJvcHMudmFsaWRhdGlvblByb3BlcnR5Rm9yRGlnaXQgPT09IFwicmVxdWlyZWRcIikge1xuICAgICAgICAgICAgcHJvcHMudmFsdWVBdHRyaWJ1dGU/LnNldFZhbGlkYXRvcihyZXF1aXJlZHZhbGlkYXRvcik7XG4gICAgICAgIH1cbiAgICAgICAgZWxzZSBpZiAocHJvcHMudmFsaWRhdGlvblByb3BlcnR5ID09PSBcImVtYWlsXCIpIHtcbiAgICAgICAgICAgIHByb3BzLnZhbHVlQXR0cmlidXRlPy5zZXRWYWxpZGF0b3IoZW1haWx2YWxpZGF0b3IpO1xuICAgICAgICB9XG4gICAgICAgIGVsc2UgaWYgKHByb3BzLnZhbGlkYXRpb25Qcm9wZXJ0eSA9PT0gXCJjdXN0b21cIiB8fCBwcm9wcy52YWxpZGF0aW9uUHJvcGVydHlGb3JEaWdpdCA9PT0gXCJjdXN0b21cIikge1xuICAgICAgICAgICAgcHJvcHMudmFsdWVBdHRyaWJ1dGU/LnNldFZhbGlkYXRvcihjdXN0b21WYWxpZGF0b3IpO1xuICAgICAgICB9XG4gICAgICAgIGVsc2UgaWYgKHByb3BzLnZhbGlkYXRpb25Qcm9wZXJ0eUZvckRpZ2l0ID09PSBcInBvc2l0aXZlTnVtYmVyXCIpIHtcbiAgICAgICAgICAgIHByb3BzLnZhbHVlQXR0cmlidXRlPy5zZXRWYWxpZGF0b3IocG9zaXRpdmVOdW1iZXJWYWxpZGF0b3IpO1xuICAgICAgICB9XG4gICAgICAgIGVsc2VcbiAgICAgICAgICAgIHByb3BzLnZhbHVlQXR0cmlidXRlPy5zZXRWYWxpZGF0b3IodW5kZWZpbmVkKTtcbiAgICB9XG5cbiAgICBmdW5jdGlvbiBDaGVja0lmQXR0cmlidXRlVmFsdWVDaGFuZ2VkQnlTY2FuKCk6IHZvaWQge1xuICAgICAgICBpZiAoc3RhdGUuaXNJbnB1dFFSSW1hZ2VDbGljaykge1xuICAgICAgICAgICAgaWYgKG9uQ2xpY2tBY3Rpb24pIHtcbiAgICAgICAgICAgICAgICB2YXIgYXR0cmlidXRlVmFsdWUgPSB2YWx1ZUF0dHJpYnV0ZT8udmFsdWUgIT09IHVuZGVmaW5lZCA/IHZhbHVlQXR0cmlidXRlLmRpc3BsYXlWYWx1ZSA6ICcnO1xuICAgICAgICAgICAgICAgIGlmIChhdHRyaWJ1dGVWYWx1ZSAhPT0gJycpIHtcbiAgICAgICAgICAgICAgICAgICAgc2V0VGltZW91dCgoKSA9PiB7XG4gICAgICAgICAgICAgICAgICAgICAgICBvbkJhcmNvZGVTY2FuQWN0aW9uRXhlY3V0ZSgpO1xuICAgICAgICAgICAgICAgICAgICB9LCA1MDApO1xuICAgICAgICAgICAgICAgICAgICBzZXRUaW1lb3V0KCgpID0+IHtcbiAgICAgICAgICAgICAgICAgICAgICAgIGZvY3VzTmV4dEVsZW1lbnQoKTtcbiAgICAgICAgICAgICAgICAgICAgfSwgMTAwMCk7XG4gICAgICAgICAgICAgICAgICAgIC8vZm9jdXNOZXh0RWxlbWVudCgxNTAwKTtcbiAgICAgICAgICAgICAgICAgICAgaWYgKG9uTGVhdmVBY3Rpb24gJiYgb25MZWF2ZUFjdGlvbi5jYW5FeGVjdXRlKSB7XG4gICAgICAgICAgICAgICAgICAgICAgICBvbkxlYXZlQWN0aW9uLmV4ZWN1dGUoKTtcbiAgICAgICAgICAgICAgICAgICAgfVxuICAgICAgICAgICAgICAgIH1cbiAgICAgICAgICAgIH1cbiAgICAgICAgICAgIHNldFN0YXRlKChwcmV2U3RhdGUpID0+ICh7IC4uLnByZXZTdGF0ZSwgaXNJbnB1dFFSSW1hZ2VDbGljazogZmFsc2UsIGlzVmFsdWVDaGFuZ2VCeVNjYW46IHRydWUgfSkpO1xuICAgICAgICB9XG4gICAgICAgIGVsc2Uge1xuICAgICAgICAgICAgc2V0U3RhdGUoKHByZXZTdGF0ZSkgPT4gKHsgLi4ucHJldlN0YXRlLCBpc1ZhbHVlQ2hhbmdlQnlTY2FuOiBmYWxzZSB9KSk7XG4gICAgICAgIH1cbiAgICB9XG5cbiAgICBmdW5jdGlvbiBvbkJhcmNvZGVTY2FuQWN0aW9uRXhlY3V0ZSgpOiB2b2lkIHtcbiAgICAgICAgaWYgKG9uQmFyY29kZVNjYW5BY3Rpb24gJiYgb25CYXJjb2RlU2NhbkFjdGlvbi5jYW5FeGVjdXRlKSB7XG4gICAgICAgICAgICBvbkJhcmNvZGVTY2FuQWN0aW9uLmV4ZWN1dGUoKTtcbiAgICAgICAgfVxuICAgIH1cblxuICAgIGNvbnN0IG9uQ2xpY2tIYW5kbGVyID0gKGlzUVJJbWFnZUNsaWNrZWQ6IGJvb2xlYW4pID0+IHtcbiAgICAgICAgaWYgKG9uQ2xpY2tBY3Rpb24gJiYgb25DbGlja0FjdGlvbi5jYW5FeGVjdXRlKSB7XG4gICAgICAgICAgICBvbkNsaWNrQWN0aW9uLmV4ZWN1dGUoKTtcbiAgICAgICAgfVxuICAgICAgICBzZXRTdGF0ZSgocHJldlN0YXRlKSA9PiAoeyAuLi5wcmV2U3RhdGUsIGlzSW5wdXRRUkltYWdlQ2xpY2s6IGlzUVJJbWFnZUNsaWNrZWQgfSkpO1xuICAgIH1cblxuICAgIGNvbnN0IG9uRW50ZXJIYW5kbGVyID0gKHNob3dRUkltYWdlOiBib29sZWFuKSA9PiB7XG4gICAgICAgIGlmICghaXNGb2N1c0Zyb21RUkltYWdlICYmIG9uRW50ZXJBY3Rpb24gJiYgb25FbnRlckFjdGlvbi5jYW5FeGVjdXRlKSB7XG4gICAgICAgICAgICBvbkVudGVyQWN0aW9uLmV4ZWN1dGUoKTtcbiAgICAgICAgfVxuICAgICAgICBpc0ZvY3VzRnJvbVFSSW1hZ2UgPSBmYWxzZTtcbiAgICAgICAgc2V0U3RhdGUoKHByZXZTdGF0ZSkgPT4gKHtcbiAgICAgICAgICAgIC4uLnByZXZTdGF0ZSxcbiAgICAgICAgICAgIHNob3dJbnB1dFFSSW1hZ2U6IHNob3dRUkltYWdlLCBpc0lucHV0UVJJbWFnZUNsaWNrOiBmYWxzZVxuICAgICAgICB9KSk7XG4gICAgfVxuXG4gICAgY29uc3Qgb25FbnRlcktleVByZXNzSGFuZGxlciA9ICAodmFsdWU6IHN0cmluZyB8IEJpZywgaXNDaGFuZ2VkOiBib29sZWFuLCBpc1ZhbHVlSW52YWxpZDogYm9vbGVhbiA9IGZhbHNlKSA9PiB7XG4gICAgICAgIGlmICghaXNWYWx1ZUludmFsaWQpIHtcbiAgICAgICAgICAgICAgICBcbiAgICAgICAgICAgIGlmIChpc0NoYW5nZWQpIHtcbiAgICAgICAgICAgICAgICAgICAgYXBwbHlDaGFuZ2UodmFsdWUpO1xuICAgICAgICAgICAgfVxuICAgICAgICB9XG4gICAgICAgIGlmIChvbkVudGVyS2V5QWN0aW9uICYmIG9uRW50ZXJLZXlBY3Rpb24uY2FuRXhlY3V0ZSkge1xuICAgICAgICAgICAgb25FbnRlcktleUFjdGlvbi5leGVjdXRlKCk7XG4gICAgICAgIH1cbiAgICB9XG5cbiAgICBjb25zdCBvbkxlYXZlSGFuZGxlciA9ICh2YWx1ZTogc3RyaW5nIHwgQmlnLCBpc0NoYW5nZWQ6IGJvb2xlYW4sIHNob3dRUkltYWdlOiBib29sZWFuLCBpc1ZhbHVlSW52YWxpZDogYm9vbGVhbiA9IGZhbHNlKSA9PiB7XG4gICAgICAgIGlmICghaXNWYWx1ZUludmFsaWQpIHtcbiAgICAgICAgICAgIGlmICghc3RhdGUuaXNJbnB1dFFSSW1hZ2VDbGljayAmJiBvbkxlYXZlQWN0aW9uICYmIG9uTGVhdmVBY3Rpb24uY2FuRXhlY3V0ZSkge1xuICAgICAgICAgICAgICAgIG9uTGVhdmVBY3Rpb24uZXhlY3V0ZSgpO1xuICAgICAgICAgICAgfVxuXG4gICAgICAgICAgICBpZiAoaXNDaGFuZ2VkKSB7XG4gICAgICAgICAgICAgICAgaWYgKCghc3RhdGUuaXNJbnB1dFFSSW1hZ2VDbGljaykgfHwgKHN0YXRlLmlzSW5wdXRRUkltYWdlQ2xpY2sgJiYgIW9uQ2xpY2tBY3Rpb24pKVxuICAgICAgICAgICAgICAgICAgICBhcHBseUNoYW5nZSh2YWx1ZSk7XG4gICAgICAgICAgICB9XG5cbiAgICAgICAgICAgIGlmIChzdGF0ZS5pc0lucHV0UVJJbWFnZUNsaWNrICYmICFvbkNsaWNrQWN0aW9uKSB7XG4gICAgICAgICAgICAgICAgZm9jdXNDdXJyZW50RWxlbWVudCgpO1xuICAgICAgICAgICAgfVxuICAgICAgICAgICAgZWxzZSB7XG4gICAgICAgICAgICAgICAgc2V0U3RhdGUoKHByZXZTdGF0ZSkgPT4gKHsgLi4ucHJldlN0YXRlLCBzaG93SW5wdXRRUkltYWdlOiBzaG93UVJJbWFnZSB9KSk7XG4gICAgICAgICAgICB9XG4gICAgICAgIH1cbiAgICAgICAgZWxzZVxuICAgICAgICAgICAgc2V0U3RhdGUoKHByZXZTdGF0ZSkgPT4gKHsgLi4ucHJldlN0YXRlLCBzaG93SW5wdXRRUkltYWdlOiBzaG93UVJJbWFnZSB9KSk7XG5cbiAgICB9O1xuXG4gICAgY29uc3QgYXBwbHlDaGFuZ2UgPSAodmFsdWU6IHN0cmluZyB8IEJpZykgPT4ge1xuXG4gICAgICAgIGlmICghcHJvcHMudmFsdWVBdHRyaWJ1dGU/LnJlYWRPbmx5ICYmIHByb3BzLnZhbHVlQXR0cmlidXRlPy5zdGF0dXMgPT09IFwiYXZhaWxhYmxlXCIpIHtcbiAgICAgICAgICAgIGlmICh2YWx1ZSA9PT0gJycgJiYgKHByb3BzLnNlbGVjdGVkQXR0cmlidXRlVHlwZSA9PT0gXCJpbnRlZ2VyXCJcbiAgICAgICAgICAgICAgICB8fCBwcm9wcy5zZWxlY3RlZEF0dHJpYnV0ZVR5cGUgPT09IFwiZGVjaW1hbFwiIHx8IHByb3BzLnNlbGVjdGVkQXR0cmlidXRlVHlwZSA9PT0gXCJsb25nXCIpKSB7XG4gICAgICAgICAgICAgICAgcHJvcHMudmFsdWVBdHRyaWJ1dGUuc2V0VmFsdWUodW5kZWZpbmVkKTtcbiAgICAgICAgICAgIH0gZWxzZVxuICAgICAgICAgICAgICAgIHByb3BzLnZhbHVlQXR0cmlidXRlLnNldFZhbHVlKHZhbHVlKTtcbiAgICAgICAgfVxuICAgIH1cblxuICAgIGNvbnN0IGFwcGx5Q2hhbmdlV2hpbGVFZGl0aW5nID0gKHZhbHVlOiBzdHJpbmcgfCBCaWcpID0+IHtcbiAgICAgICAgc2V0VGltZW91dChcbiAgICAgICAgICAgICgpID0+IGFwcGx5Q2hhbmdlKHZhbHVlKSxcbiAgICAgICAgICAgIHN1Ym1pdERlbGF5VmFsdWVcbiAgICAgICAgKTtcbiAgICB9XG5cbiAgICBjb25zdCByZXF1aXJlZHZhbGlkYXRvciA9ICh2YWx1ZTogc3RyaW5nIHwgdW5kZWZpbmVkKTogc3RyaW5nIHwgdW5kZWZpbmVkID0+IHtcbiAgICAgICAgY29uc3QgeyB2YWxpZGF0aW9uTWVzc2FnZSB9ID0gcHJvcHM7XG5cbiAgICAgICAgaWYgKHZhbGlkYXRpb25NZXNzYWdlICYmICF2YWx1ZSkge1xuICAgICAgICAgICAgcmV0dXJuIHZhbGlkYXRpb25NZXNzYWdlO1xuICAgICAgICB9XG4gICAgfVxuXG4gICAgY29uc3QgZW1haWx2YWxpZGF0b3IgPSAodmFsdWU6IHN0cmluZyB8IHVuZGVmaW5lZCk6IHN0cmluZyB8IHVuZGVmaW5lZCA9PiB7XG4gICAgICAgIGNvbnN0IHsgdmFsaWRhdGlvbk1lc3NhZ2UgfSA9IHByb3BzO1xuXG4gICAgICAgIGlmICh2YWx1ZSAhPT0gdW5kZWZpbmVkICYmIHZhbHVlICE9PSAnJykge1xuICAgICAgICAgICAgbGV0IG1hdGNoUmVzdWx0ID0gdmFsdWUubWF0Y2goL1tBLVphLXowLTkhIyQlJicnKisvPT9eX2B7fH1+LV0rKD86XFwuW0EtWmEtejAtOSEjJCUmJycqKy89P15fYHt8fX4tXSspKkAoPzpbQS1aYS16MC05XSg/OltBLVphLXowLTktXSpbQS1aYS16MC05XSk/XFwuKStbQS1aYS16MC05XSg/OltBLVphLXowLTktXSpbQS1aYS16MC05XSk/Lyk7XG5cbiAgICAgICAgICAgIGlmIChtYXRjaFJlc3VsdCA9PT0gbnVsbCkge1xuICAgICAgICAgICAgICAgIGlmICh2YWxpZGF0aW9uTWVzc2FnZSkge1xuICAgICAgICAgICAgICAgICAgICByZXR1cm4gdmFsaWRhdGlvbk1lc3NhZ2U7XG4gICAgICAgICAgICAgICAgfVxuICAgICAgICAgICAgfVxuICAgICAgICB9XG4gICAgfVxuXG4gICAgY29uc3QgcG9zaXRpdmVOdW1iZXJWYWxpZGF0b3IgPSAodmFsdWU6IHN0cmluZyB8IHVuZGVmaW5lZCk6IHN0cmluZyB8IHVuZGVmaW5lZCA9PiB7XG4gICAgICAgIGNvbnN0IHsgdmFsaWRhdGlvbk1lc3NhZ2UgfSA9IHByb3BzO1xuXG4gICAgICAgIGlmICh2YWx1ZSAhPT0gdW5kZWZpbmVkICYmIHZhbHVlICE9PSAnJykge1xuICAgICAgICAgICAgbGV0IG1hdGNoUmVzdWx0ID0gdmFsdWUudG9TdHJpbmcoKS5tYXRjaCgvXig/ITAoXFwuMCspPyQpXFxkKlxcLj9cXGQrJC8pO1xuXG4gICAgICAgICAgICBpZiAobWF0Y2hSZXN1bHQgPT09IG51bGwpIHtcbiAgICAgICAgICAgICAgICBpZiAodmFsaWRhdGlvbk1lc3NhZ2UpIHtcbiAgICAgICAgICAgICAgICAgICAgcmV0dXJuIHZhbGlkYXRpb25NZXNzYWdlO1xuICAgICAgICAgICAgICAgIH1cbiAgICAgICAgICAgIH1cbiAgICAgICAgfVxuICAgIH1cblxuICAgIGNvbnN0IGN1c3RvbVZhbGlkYXRvciA9ICh2YWx1ZTogc3RyaW5nIHwgdW5kZWZpbmVkKTogc3RyaW5nIHwgdW5kZWZpbmVkID0+IHtcbiAgICAgICAgY29uc3QgeyB2YWxpZGF0aW9uTWVzc2FnZSwgdmFsaWRhdGlvbkV4cHJlc3Npb24gfSA9IHByb3BzO1xuXG4gICAgICAgIGlmICh2YWxpZGF0aW9uRXhwcmVzc2lvbj8udmFsdWUgJiYgdmFsdWUgIT09IHVuZGVmaW5lZCAmJiB2YWx1ZSAhPT0gJycpIHtcbiAgICAgICAgICAgIGxldCBtYXRjaFJlc3VsdCA9IHZhbHVlLnRvU3RyaW5nKCkubWF0Y2godmFsaWRhdGlvbkV4cHJlc3Npb24udmFsdWUpO1xuXG4gICAgICAgICAgICBpZiAobWF0Y2hSZXN1bHQgPT09IG51bGwpIHtcbiAgICAgICAgICAgICAgICBpZiAodmFsaWRhdGlvbk1lc3NhZ2UpIHtcbiAgICAgICAgICAgICAgICAgICAgcmV0dXJuIHZhbGlkYXRpb25NZXNzYWdlO1xuICAgICAgICAgICAgICAgIH1cbiAgICAgICAgICAgIH1cbiAgICAgICAgfVxuICAgIH1cblxuICAgIGNvbnN0IGZvY3VzQ3VycmVudEVsZW1lbnQgPSAoKSA9PiB7XG4gICAgICAgIGlmIChpbnB1dFJlZi5jdXJyZW50KSB7XG4gICAgICAgICAgICBpbnB1dFJlZi5jdXJyZW50LmZvY3VzKCk7XG4gICAgICAgIH1cbiAgICB9XG5cbiAgICBjb25zdCBmb2N1c05leHRFbGVtZW50ID0gKHRpbWVvdXQ6IG51bWJlciA9IDApID0+IHtcblxuICAgICAgICAvLyBpZiAoZG9jdW1lbnQuYWN0aXZlRWxlbWVudCBpbnN0YW5jZW9mIEhUTUxFbGVtZW50KSB7XG4gICAgICAgIC8vICAgICBkb2N1bWVudC5hY3RpdmVFbGVtZW50LmJsdXIoKTtcbiAgICAgICAgLy8gfVxuXG4gICAgICAgIGlmIChpbnB1dFJlZi5jdXJyZW50KSB7XG4gICAgICAgICAgICBsZXQgY3VycmVudEVsZW1lbnQgPSBpbnB1dFJlZi5jdXJyZW50IGFzIEhUTUxFbGVtZW50IHwgbnVsbDtcblxuICAgICAgICAgICAgLy8gVHJhdmVyc2UgdXAgdG8gdGhlIHBhcmVudCBlbGVtZW50XG4gICAgICAgICAgICB3aGlsZSAoY3VycmVudEVsZW1lbnQpIHtcbiAgICAgICAgICAgICAgICBsZXQgbmV4dEVsZW1lbnQgPSBjdXJyZW50RWxlbWVudC5uZXh0RWxlbWVudFNpYmxpbmcgYXMgSFRNTEVsZW1lbnQgfCBudWxsO1xuXG4gICAgICAgICAgICAgICAgLy8gVHJhdmVyc2UgZG93biB0byBmaW5kIHRoZSBuZXh0IGZvY3VzYWJsZSBlbGVtZW50XG4gICAgICAgICAgICAgICAgd2hpbGUgKG5leHRFbGVtZW50KSB7XG4gICAgICAgICAgICAgICAgICAgIGNvbnN0IGZvY3VzYWJsZUVsZW1lbnQgPSBmaW5kRm9jdXNhYmxlRWxlbWVudChuZXh0RWxlbWVudCk7XG4gICAgICAgICAgICAgICAgICAgIGlmIChmb2N1c2FibGVFbGVtZW50KSB7XG4gICAgICAgICAgICAgICAgICAgICAgICAvL2ZvY3VzYWJsZUVsZW1lbnQuZm9jdXMoKTtcbiAgICAgICAgICAgICAgICAgICAgICAgIHNldFRpbWVvdXQoKCkgPT4ge1xuICAgICAgICAgICAgICAgICAgICAgICAgICAgIGZvY3VzYWJsZUVsZW1lbnQuZm9jdXMoKTtcbiAgICAgICAgICAgICAgICAgICAgICAgIH0sIHRpbWVvdXQpO1xuICAgICAgICAgICAgICAgICAgICAgICAgcmV0dXJuO1xuICAgICAgICAgICAgICAgICAgICB9XG4gICAgICAgICAgICAgICAgICAgIG5leHRFbGVtZW50ID0gbmV4dEVsZW1lbnQubmV4dEVsZW1lbnRTaWJsaW5nIGFzIEhUTUxFbGVtZW50IHwgbnVsbDtcbiAgICAgICAgICAgICAgICB9XG5cbiAgICAgICAgICAgICAgICBjdXJyZW50RWxlbWVudCA9IGN1cnJlbnRFbGVtZW50LnBhcmVudEVsZW1lbnQgYXMgSFRNTEVsZW1lbnQgfCBudWxsO1xuICAgICAgICAgICAgfVxuICAgICAgICB9XG4gICAgfTtcblxuICAgIGNvbnN0IGZpbmRGb2N1c2FibGVFbGVtZW50ID0gKGVsZW1lbnQ6IEhUTUxFbGVtZW50KTogSFRNTEVsZW1lbnQgfCBudWxsID0+IHtcbiAgICAgICAgaWYgKGlzRm9jdXNhYmxlKGVsZW1lbnQpKSB7XG4gICAgICAgICAgICByZXR1cm4gZWxlbWVudDtcbiAgICAgICAgfVxuXG4gICAgICAgIGNvbnN0IGNoaWxkcmVuID0gZWxlbWVudC5jaGlsZHJlbjtcbiAgICAgICAgZm9yIChsZXQgaSA9IDA7IGkgPCBjaGlsZHJlbi5sZW5ndGg7IGkrKykge1xuICAgICAgICAgICAgY29uc3QgZm9jdXNhYmxlQ2hpbGQgPSBmaW5kRm9jdXNhYmxlRWxlbWVudChjaGlsZHJlbltpXSBhcyBIVE1MRWxlbWVudCk7XG4gICAgICAgICAgICBpZiAoZm9jdXNhYmxlQ2hpbGQpIHtcbiAgICAgICAgICAgICAgICByZXR1cm4gZm9jdXNhYmxlQ2hpbGQ7XG4gICAgICAgICAgICB9XG4gICAgICAgIH1cblxuICAgICAgICByZXR1cm4gbnVsbDtcbiAgICB9O1xuXG4gICAgY29uc3QgaXNGb2N1c2FibGUgPSAoZWxlbWVudDogSFRNTEVsZW1lbnQpOiBib29sZWFuID0+IHtcbiAgICAgICAgY29uc3QgZm9jdXNhYmxlRWxlbWVudHMgPSBbJ0lOUFVUJywgJ0JVVFRPTicsICdTRUxFQ1QnLCAnVEVYVEFSRUEnLCAnQSddO1xuICAgICAgICByZXR1cm4gKFxuICAgICAgICAgICAgZm9jdXNhYmxlRWxlbWVudHMuaW5jbHVkZXMoZWxlbWVudC50YWdOYW1lKSB8fFxuICAgICAgICAgICAgZWxlbWVudC50YWJJbmRleCA+PSAwXG4gICAgICAgICk7XG4gICAgfTtcblxuICAgIHJldHVybiAoXG4gICAgICAgICghcHJvcHMudmFsdWVBdHRyaWJ1dGU/LnJlYWRPbmx5ICYmIHJlYWRPbmx5U3R5bGUgPT09IFwidGV4dFwiKSB8fCAocmVhZE9ubHlTdHlsZSA9PT0gXCJkYXRhVmlld1wiIHx8IHJlYWRPbmx5U3R5bGUgPT09IFwiY29udHJvbFwiKSA/XG4gICAgICAgICAgICA8ZGl2PlxuICAgICAgICAgICAgICAgIDxkaXZcbiAgICAgICAgICAgICAgICAgICAgY2xhc3NOYW1lPVwid2lkZ2V0LW9wY29yZXRleHRib3gtaG9sZGVyXCI+XG4gICAgICAgICAgICAgICAgICAgIDxJbnB1dEVsZW1lbnRcbiAgICAgICAgICAgICAgICAgICAgICAgIGlucHV0UmVmPXtpbnB1dFJlZn1cbiAgICAgICAgICAgICAgICAgICAgICAgIGlkPXtwcm9wcy5pZH1cbiAgICAgICAgICAgICAgICAgICAgICAgIGlucHV0VHlwZT17c2hvd0FzUGFzc293cmR9XG4gICAgICAgICAgICAgICAgICAgICAgICBwbGFjZUhvbGRlcklucHV0PXtwbGFjZWhvbGRlcj8udmFsdWV9XG4gICAgICAgICAgICAgICAgICAgICAgICBpbnB1dE1hc2s9e2lucHV0TWFza31cbiAgICAgICAgICAgICAgICAgICAgICAgIHRhYkluZGV4PXt0YWJJbmRleH1cbiAgICAgICAgICAgICAgICAgICAgICAgIG1heExlbmd0aFR5cGU9e3Byb3BzLm1heExlbmd0aFR5cGV9XG4gICAgICAgICAgICAgICAgICAgICAgICBtYXhMZW5ndGg9e3Byb3BzLmN1c3RvbU1heExlbmd0aH1cbiAgICAgICAgICAgICAgICAgICAgICAgIGFyaWFSZXF1aXJlZD17cHJvcHMuYXJpYVJlcXVpcmVkfVxuICAgICAgICAgICAgICAgICAgICAgICAgYXJpYUxhYmVsPXtwcm9wcy5zY3JlZW5SZWFkZXJDYXB0aW9uPy52YWx1ZX1cbiAgICAgICAgICAgICAgICAgICAgICAgIGF1dG9Db21wbGV0ZT17cHJvcHMuYXV0b0NvbXBsZXRlfVxuICAgICAgICAgICAgICAgICAgICAgICAgc2hvd1FSSW1hZ2U9e3Nob3dRckltYWdlVXNlcklucHV0ICE9PSB1bmRlZmluZWQgJiYgc2hvd1FySW1hZ2VVc2VySW5wdXQgPyAhIXN0YXRlLnNob3dJbnB1dFFSSW1hZ2UgOiBmYWxzZX1cbiAgICAgICAgICAgICAgICAgICAgICAgIGlzSW5wdXRRUkltYWdlQ2xpY2s9e3N0YXRlLmlzSW5wdXRRUkltYWdlQ2xpY2t9XG4gICAgICAgICAgICAgICAgICAgICAgICBpc1ZhbHVlQ2hhbmdlZEJ5U2Nhbj17c3RhdGUuaXNWYWx1ZUNoYW5nZUJ5U2Nhbn1cbiAgICAgICAgICAgICAgICAgICAgICAgIGF0dHJpYnV0ZVR5cGU9e3Byb3BzLnNlbGVjdGVkQXR0cmlidXRlVHlwZX1cbiAgICAgICAgICAgICAgICAgICAgICAgIGRlY2ltYWxNb2RlPXtwcm9wcy5kZWNpbWFsTW9kZX1cbiAgICAgICAgICAgICAgICAgICAgICAgIGRlY2ltYWxQcmVjaXNpb249e3Byb3BzLmRlY2ltYWxQcmVjaXNpb259XG4gICAgICAgICAgICAgICAgICAgICAgICBncm91cERpZ2l0cz17cHJvcHMuZ3JvdXBEaWdpdHN9XG4gICAgICAgICAgICAgICAgICAgICAgICBvbkVudGVyPXtvbkVudGVySGFuZGxlcn1cbiAgICAgICAgICAgICAgICAgICAgICAgIG9uTGVhdmU9e29uTGVhdmVIYW5kbGVyfVxuICAgICAgICAgICAgICAgICAgICAgICAgb25FbnRlcktleVByZXNzPXtvbkVudGVyS2V5UHJlc3NIYW5kbGVyfVxuICAgICAgICAgICAgICAgICAgICAgICAgb25DaGFuZ2U9e3Byb3BzLnZhbHVlQXR0cmlidXRlPy5zZXRWYWx1ZX1cbiAgICAgICAgICAgICAgICAgICAgICAgIGFwcGx5Q2hhbmdlPXtwcm9wcy5zdWJtaXRXaGlsZUVkaXRpbmcgPT09IFwid2hpbGVFZGl0aW5nXCIgPyBhcHBseUNoYW5nZVdoaWxlRWRpdGluZyA6IHVuZGVmaW5lZH1cbiAgICAgICAgICAgICAgICAgICAgICAgIHZhbHVlPXt2YWx1ZUF0dHJpYnV0ZSA/IHZhbHVlQXR0cmlidXRlLmRpc3BsYXlWYWx1ZSA6IFwiXCJ9XG4gICAgICAgICAgICAgICAgICAgICAgICBkaXNhYmxlZD17cHJvcHMudmFsdWVBdHRyaWJ1dGU/LnJlYWRPbmx5fVxuICAgICAgICAgICAgICAgICAgICAgICAgaGFzRXJyb3I9eyEhdmFsaWRhdGlvbkZlZWRiYWNrfVxuICAgICAgICAgICAgICAgICAgICAgICAgcmVxdWlyZWQ9e3JlcXVpcmVkfVxuICAgICAgICAgICAgICAgICAgICAvPlxuICAgICAgICAgICAgICAgICAgICB7c2hvd1FySW1hZ2VVc2VySW5wdXQgIT09IHVuZGVmaW5lZCAmJiBzaG93UXJJbWFnZVVzZXJJbnB1dCAmJiAhIXN0YXRlLnNob3dJbnB1dFFSSW1hZ2UgP1xuICAgICAgICAgICAgICAgICAgICAgICAgPEJhcmNvZGVJbWFnZVxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIG9uQ2xpY2tIYW5kbGVyPXtvbkNsaWNrSGFuZGxlcn1cbiAgICAgICAgICAgICAgICAgICAgICAgID48L0JhcmNvZGVJbWFnZT4gOiBudWxsXG4gICAgICAgICAgICAgICAgICAgIH1cbiAgICAgICAgICAgICAgICAgICAgPEFsZXJ0IGlkPXtwcm9wcy5pZH0gbWVzc2FnZT17dmFsaWRhdGlvbkZlZWRiYWNrfSBib290c3RyYXBTdHlsZT1cImRhbmdlclwiIGNsYXNzTmFtZT17XCJteC12YWxpZGF0aW9uLW1lc3NhZ2VcIn0+PC9BbGVydD5cbiAgICAgICAgICAgICAgICA8L2Rpdj5cbiAgICAgICAgICAgIDwvZGl2PiA6XG4gICAgICAgICAgICA8ZGl2XG4gICAgICAgICAgICAgICAgY2xhc3NOYW1lPVwid2lkZ2V0LW9wY29yZXRleHRib3gtaG9sZGVyXCI+XG4gICAgICAgICAgICAgICAgPGRpdlxuICAgICAgICAgICAgICAgICAgICBjbGFzc05hbWU9XCJmb3JtLWNvbnRyb2wtc3RhdGljXCI+XG4gICAgICAgICAgICAgICAgICAgIHt2YWx1ZUF0dHJpYnV0ZSA/IHZhbHVlQXR0cmlidXRlLmRpc3BsYXlWYWx1ZSA6IFwiIFwifVxuICAgICAgICAgICAgICAgIDwvZGl2PlxuICAgICAgICAgICAgPC9kaXY+XG4gICAgKTtcbn1cbiJdLCJuYW1lcyI6WyJoYXNPd24iLCJoYXNPd25Qcm9wZXJ0eSIsImNsYXNzTmFtZXMiLCJjbGFzc2VzIiwiaSIsImFyZ3VtZW50cyIsImxlbmd0aCIsImFyZyIsImFwcGVuZENsYXNzIiwicGFyc2VWYWx1ZSIsIkFycmF5IiwiaXNBcnJheSIsImFwcGx5IiwidG9TdHJpbmciLCJPYmplY3QiLCJwcm90b3R5cGUiLCJpbmNsdWRlcyIsImtleSIsImNhbGwiLCJ2YWx1ZSIsIm5ld0NsYXNzIiwibW9kdWxlIiwiZXhwb3J0cyIsImRlZmF1bHQiLCJ3aW5kb3ciLCJJbnB1dE1hc2tFbGVtZW50Il0sIm1hcHBpbmdzIjoiOzs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7QUFLQTs7QUFFQyxFQUFBLENBQVksWUFBQTs7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
