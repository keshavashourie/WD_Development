define(['exports', 'react', 'big.js'], (function (exports, react, Big) { 'use strict';

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
	    const [value, setValue] = react.useState(props.value ?? '');
	    const inputRef = react.useRef(null);
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
	    return (react.createElement("input", { ref: node => {
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
	    react.useEffect(() => {
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
	    const [state, setState] = react.useState({
	        editedValue: undefined, isFocused: false, hasError: props.hasError
	    });
	    react.useEffect(() => setState((prevState) => ({ ...prevState, editedValue: undefined })), [value]);
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
	        react.createElement("input", { ref: node => {
	                if (props.inputRef) {
	                    props.inputRef.current = node ?? undefined;
	                }
	            }, style: inputStyle, id: props.id, className: getInputClassNames(), tabIndex: tabIndex, value: getCurrentDisplayValue().toString(), type: props.inputType ? "password" : "text", maxLength: maxLengthInput === 0 ? undefined : maxLengthInput, onFocus: onEnterHandle, onKeyDown: (event) => {
	                if (event.key === "Enter") {
	                    onEnterKeyPressHandler(event);
	                }
	            }, placeholder: placeHolderInput, onBlur: onBlur, onChange: (event) => onChangeHandle(event), disabled: disabled, ...props.ariaLabel ? { "aria-label": props.ariaLabel } : {}, ...props.hasError || state.hasError ? { "aria-invalid": true } : {}, ...props.ariaRequired ? { "aria-required": true } : {}, ...props.required ? { required: true } : {}, autoComplete: props.autoComplete.replaceAll("_", "-") }) :
	        react.createElement(CustomInputMask, { ref: props.inputRef, key: value, id: props.id, style: inputStyle, className: getInputClassNames(), tabIndex: tabIndex, showAsPassowrd: props.inputType, mask: userInputMask, maxLength: maxLengthInput === 0 ? undefined : maxLengthInput, value: getCurrentDisplayValue().toString(), placeHolderChar: "_", placeHolderInput: placeHolderInput, onChangeHandler: onChangeHandle, onEnterKeyPress: (event) => { onEnterKeyPressHandler(event); }, onFocus: onEnterHandle, onBlur: onBlur, disabled: disabled, ariaLabel: props.ariaLabel, ariaInvalid: props.hasError || state.hasError, ariaRequired: ariaRequired, required: props.required, autoComplete: props.autoComplete.replaceAll("_", "-") }));
	}

	var cmdQRCode = "widgets/mendix/opcentercoretextbox/assets/829624eac6f2835e.svg";

	const BarcodeImage = ({ style, onClickHandler }) => react.createElement("img", { style: style, className: "opcore-qr-image", role: "button", src: cmdQRCode, onMouseDown: () => onClickHandler !== undefined ? onClickHandler(true) : false });

	function Alert({ id, message, className, bootstrapStyle }) {
	    return message ? react.createElement("div", { id: `${id}-error`, role: "alert", className: classNames(`alert alert-${bootstrapStyle}`, className) }, message) : null;
	}

	var isFocusFromQRImage = false;
	function OpcenterCoreTextBox(props) {
	    const { valueAttribute, showAsPassowrd, placeholder, showQrImageUserInput, inputMask, readOnlyStyle, onClickAction, onBarcodeScanAction, submitDelay, onEnterAction, tabIndex, onEnterKeyAction, onLeaveAction } = props;
	    const validationFeedback = props.valueAttribute?.validation;
	    const required = props.validationProperty === "required" || props.validationPropertyForDigit === "required";
	    const submitDelayValue = submitDelay ? submitDelay >= 0 ? submitDelay : 0 : 0;
	    const inputRef = react.useRef();
	    const [state, setState] = react.useState({
	        showInputQRImage: false,
	        isInputQRImageClick: false, isValueChangeByScan: false
	    });
	    // useEffect(() => {
	    //     checkValidators();
	    // }, []);
	    react.useEffect(() => {
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
	        react.createElement("div", null,
	            react.createElement("div", { className: "widget-opcoretextbox-holder" },
	                react.createElement(InputElement, { inputRef: inputRef, id: props.id, inputType: showAsPassowrd, placeHolderInput: placeholder?.value, inputMask: inputMask, tabIndex: tabIndex, maxLengthType: props.maxLengthType, maxLength: props.customMaxLength, ariaRequired: props.ariaRequired, ariaLabel: props.screenReaderCaption?.value, autoComplete: props.autoComplete, showQRImage: showQrImageUserInput !== undefined && showQrImageUserInput ? !!state.showInputQRImage : false, isInputQRImageClick: state.isInputQRImageClick, isValueChangedByScan: state.isValueChangeByScan, attributeType: props.selectedAttributeType, decimalMode: props.decimalMode, decimalPrecision: props.decimalPrecision, groupDigits: props.groupDigits, onEnter: onEnterHandler, onLeave: onLeaveHandler, onEnterKeyPress: onEnterKeyPressHandler, onChange: props.valueAttribute?.setValue, applyChange: props.submitWhileEditing === "whileEditing" ? applyChangeWhileEditing : undefined, value: valueAttribute ? valueAttribute.displayValue : "", disabled: props.valueAttribute?.readOnly, hasError: !!validationFeedback, required: required }),
	                showQrImageUserInput !== undefined && showQrImageUserInput && !!state.showInputQRImage ?
	                    react.createElement(BarcodeImage, { onClickHandler: onClickHandler }) : null,
	                react.createElement(Alert, { id: props.id, message: validationFeedback, bootstrapStyle: "danger", className: "mx-validation-message" }))) :
	        react.createElement("div", { className: "widget-opcoretextbox-holder" },
	            react.createElement("div", { className: "form-control-static" }, valueAttribute ? valueAttribute.displayValue : " ")));
	}

	exports.OpcenterCoreTextBox = OpcenterCoreTextBox;

}));
//# sourceMappingURL=data:application/json;charset=utf-8;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoiT3BjZW50ZXJDb3JlVGV4dEJveC5qcyIsInNvdXJjZXMiOlsiLi4vLi4vLi4vLi4vLi4vbm9kZV9tb2R1bGVzL2NsYXNzbmFtZXMvaW5kZXguanMiLCIuLi8uLi8uLi8uLi8uLi9zcmMvY29tcG9uZW50cy9JbnB1dE1hc2tFbGVtZW50LnRzeCIsIi4uLy4uLy4uLy4uLy4uL3NyYy9jb21wb25lbnRzL0lucHV0RWxlbWVudC50c3giLCIuLi8uLi8uLi8uLi8uLi9zcmMvaW1hZ2VzL2NtZFFSQ29kZS5zdmciLCIuLi8uLi8uLi8uLi8uLi9zcmMvY29tcG9uZW50cy9CYXJjb2RlSW1hZ2UudHN4IiwiLi4vLi4vLi4vLi4vLi4vc3JjL2NvbXBvbmVudHMvQWxlcnQudHN4IiwiLi4vLi4vLi4vLi4vLi4vc3JjL09wY2VudGVyQ29yZVRleHRCb3gudHN4Il0sInNvdXJjZXNDb250ZW50IjpbIi8qIVxuXHRDb3B5cmlnaHQgKGMpIDIwMTggSmVkIFdhdHNvbi5cblx0TGljZW5zZWQgdW5kZXIgdGhlIE1JVCBMaWNlbnNlIChNSVQpLCBzZWVcblx0aHR0cDovL2plZHdhdHNvbi5naXRodWIuaW8vY2xhc3NuYW1lc1xuKi9cbi8qIGdsb2JhbCBkZWZpbmUgKi9cblxuKGZ1bmN0aW9uICgpIHtcblx0J3VzZSBzdHJpY3QnO1xuXG5cdHZhciBoYXNPd24gPSB7fS5oYXNPd25Qcm9wZXJ0eTtcblxuXHRmdW5jdGlvbiBjbGFzc05hbWVzICgpIHtcblx0XHR2YXIgY2xhc3NlcyA9ICcnO1xuXG5cdFx0Zm9yICh2YXIgaSA9IDA7IGkgPCBhcmd1bWVudHMubGVuZ3RoOyBpKyspIHtcblx0XHRcdHZhciBhcmcgPSBhcmd1bWVudHNbaV07XG5cdFx0XHRpZiAoYXJnKSB7XG5cdFx0XHRcdGNsYXNzZXMgPSBhcHBlbmRDbGFzcyhjbGFzc2VzLCBwYXJzZVZhbHVlKGFyZykpO1xuXHRcdFx0fVxuXHRcdH1cblxuXHRcdHJldHVybiBjbGFzc2VzO1xuXHR9XG5cblx0ZnVuY3Rpb24gcGFyc2VWYWx1ZSAoYXJnKSB7XG5cdFx0aWYgKHR5cGVvZiBhcmcgPT09ICdzdHJpbmcnIHx8IHR5cGVvZiBhcmcgPT09ICdudW1iZXInKSB7XG5cdFx0XHRyZXR1cm4gYXJnO1xuXHRcdH1cblxuXHRcdGlmICh0eXBlb2YgYXJnICE9PSAnb2JqZWN0Jykge1xuXHRcdFx0cmV0dXJuICcnO1xuXHRcdH1cblxuXHRcdGlmIChBcnJheS5pc0FycmF5KGFyZykpIHtcblx0XHRcdHJldHVybiBjbGFzc05hbWVzLmFwcGx5KG51bGwsIGFyZyk7XG5cdFx0fVxuXG5cdFx0aWYgKGFyZy50b1N0cmluZyAhPT0gT2JqZWN0LnByb3RvdHlwZS50b1N0cmluZyAmJiAhYXJnLnRvU3RyaW5nLnRvU3RyaW5nKCkuaW5jbHVkZXMoJ1tuYXRpdmUgY29kZV0nKSkge1xuXHRcdFx0cmV0dXJuIGFyZy50b1N0cmluZygpO1xuXHRcdH1cblxuXHRcdHZhciBjbGFzc2VzID0gJyc7XG5cblx0XHRmb3IgKHZhciBrZXkgaW4gYXJnKSB7XG5cdFx0XHRpZiAoaGFzT3duLmNhbGwoYXJnLCBrZXkpICYmIGFyZ1trZXldKSB7XG5cdFx0XHRcdGNsYXNzZXMgPSBhcHBlbmRDbGFzcyhjbGFzc2VzLCBrZXkpO1xuXHRcdFx0fVxuXHRcdH1cblxuXHRcdHJldHVybiBjbGFzc2VzO1xuXHR9XG5cblx0ZnVuY3Rpb24gYXBwZW5kQ2xhc3MgKHZhbHVlLCBuZXdDbGFzcykge1xuXHRcdGlmICghbmV3Q2xhc3MpIHtcblx0XHRcdHJldHVybiB2YWx1ZTtcblx0XHR9XG5cdFxuXHRcdGlmICh2YWx1ZSkge1xuXHRcdFx0cmV0dXJuIHZhbHVlICsgJyAnICsgbmV3Q2xhc3M7XG5cdFx0fVxuXHRcblx0XHRyZXR1cm4gdmFsdWUgKyBuZXdDbGFzcztcblx0fVxuXG5cdGlmICh0eXBlb2YgbW9kdWxlICE9PSAndW5kZWZpbmVkJyAmJiBtb2R1bGUuZXhwb3J0cykge1xuXHRcdGNsYXNzTmFtZXMuZGVmYXVsdCA9IGNsYXNzTmFtZXM7XG5cdFx0bW9kdWxlLmV4cG9ydHMgPSBjbGFzc05hbWVzO1xuXHR9IGVsc2UgaWYgKHR5cGVvZiBkZWZpbmUgPT09ICdmdW5jdGlvbicgJiYgdHlwZW9mIGRlZmluZS5hbWQgPT09ICdvYmplY3QnICYmIGRlZmluZS5hbWQpIHtcblx0XHQvLyByZWdpc3RlciBhcyAnY2xhc3NuYW1lcycsIGNvbnNpc3RlbnQgd2l0aCBucG0gcGFja2FnZSBuYW1lXG5cdFx0ZGVmaW5lKCdjbGFzc25hbWVzJywgW10sIGZ1bmN0aW9uICgpIHtcblx0XHRcdHJldHVybiBjbGFzc05hbWVzO1xuXHRcdH0pO1xuXHR9IGVsc2Uge1xuXHRcdHdpbmRvdy5jbGFzc05hbWVzID0gY2xhc3NOYW1lcztcblx0fVxufSgpKTtcbiIsImltcG9ydCBSZWFjdCwgeyBjcmVhdGVFbGVtZW50LCB1c2VTdGF0ZSwgdXNlUmVmLCBDU1NQcm9wZXJ0aWVzLCBDaGFuZ2VFdmVudCB9IGZyb20gJ3JlYWN0JztcblxuZXhwb3J0IGludGVyZmFjZSBJbnB1dE1hc2tQcm9wcyB7XG4gICAgcmVmPzogUmVhY3QuTXV0YWJsZVJlZk9iamVjdDxIVE1MSW5wdXRFbGVtZW50IHwgdW5kZWZpbmVkPlxuICAgIGlkOiBzdHJpbmc7XG4gICAgY2xhc3NOYW1lPzogc3RyaW5nO1xuICAgIHN0eWxlPzogQ1NTUHJvcGVydGllcztcbiAgICBzaG93QXNQYXNzb3dyZD86IGJvb2xlYW47XG4gICAgaW5wdXRNYXNrPzogc3RyaW5nO1xuICAgIG1hc2s/OiBzdHJpbmc7XG4gICAgcGxhY2VIb2xkZXJDaGFyPzogc3RyaW5nO1xuICAgIHBsYWNlSG9sZGVySW5wdXQ/OiBzdHJpbmc7XG4gICAgdGFiSW5kZXg/OiBudW1iZXI7XG4gICAgdmFsdWU/OiBzdHJpbmc7XG4gICAgb25DaGFuZ2VIYW5kbGVyPzogKGV2ZW50OiBDaGFuZ2VFdmVudDxIVE1MSW5wdXRFbGVtZW50PiwgaW5JbnB1dE1hc2tWYWx1ZTogYm9vbGVhbiwgdmFsdWU6IHN0cmluZykgPT4gdm9pZDtcbiAgICBvbkVudGVyS2V5UHJlc3M/OiBSZWFjdC5LZXlib2FyZEV2ZW50SGFuZGxlcjxIVE1MSW5wdXRFbGVtZW50PjtcbiAgICBvbkZvY3VzOigpID0+IHZvaWQ7XG4gICAgb25CbHVyOiAoKSA9PiB2b2lkO1xuICAgIGFyaWFSZXF1aXJlZDogYm9vbGVhbjtcbiAgICBhcmlhTGFiZWw/OiBzdHJpbmdcbiAgICBhcmlhSW52YWxpZD86IGJvb2xlYW47XG4gICAgYXV0b0NvbXBsZXRlOiBzdHJpbmc7XG4gICAgbWF4TGVuZ3RoPzogbnVtYmVyO1xuICAgIGRpc2FibGVkPzogYm9vbGVhbjtcbiAgICBoYXNFcnJvcj86IGJvb2xlYW47XG4gICAgcmVxdWlyZWQ/OiBib29sZWFuO1xufVxuXG52YXIgcGxhY2VIb2xkZXIgPSAnJztcblxuY29uc3QgQ3VzdG9tSW5wdXRNYXNrID0gKHByb3BzOiBJbnB1dE1hc2tQcm9wcykgPT4ge1xuICAgIGNvbnN0IG1hc2sgPSBwcm9wcy5tYXNrID8/ICcnO1xuICAgIGNvbnN0IHBsYWNlaG9sZGVyQ2hhciA9IHByb3BzLnBsYWNlSG9sZGVyQ2hhciA/PyAnXyc7XG4gICAgcGxhY2VIb2xkZXIgPSBwcm9wcy5wbGFjZUhvbGRlcklucHV0ID8/ICcnO1xuICAgIFxuICAgIGNvbnN0IFt2YWx1ZSwgc2V0VmFsdWVdID0gdXNlU3RhdGUocHJvcHMudmFsdWUgPz8gJycpO1xuICAgIGNvbnN0IGlucHV0UmVmID0gdXNlUmVmPEhUTUxJbnB1dEVsZW1lbnQ+KG51bGwpO1xuXG4gICAgY29uc3QgbWFza1J1bGVzOiB7IFtrZXk6IHN0cmluZ106IFJlZ0V4cCB9ID0ge1xuICAgICAgICAnOSc6IC9cXGQvLFxuICAgICAgICAnWic6IC9bQS1aYS16XS8sXG4gICAgICAgICdVJzogL1tBLVpdLyxcbiAgICAgICAgJ0wnOiAvW2Etel0vLFxuICAgICAgICAnKic6IC9bQS1aYS16MC05XS9cbiAgICB9O1xuXG5cbiAgICBjb25zdCBjbGVhblZhbHVlID0gKGlucHV0OiBzdHJpbmcpID0+IHtcbiAgICAgICAgcmV0dXJuIGlucHV0LnJlcGxhY2UoL1teMC05YS16QS1aXS9nLCAnJyk7XG4gICAgfVxuXG4gICAgY29uc3QgYXBwbHlNYXNrID0gKGlucHV0OiBzdHJpbmcpID0+IHtcbiAgICAgICAgbGV0IGNsZWFuSW5wdXQgPSBjbGVhblZhbHVlKGlucHV0KTtcbiAgICAgICAgbGV0IG1hc2tlZFZhbHVlID0gJyc7XG4gICAgICAgIGxldCBpbnB1dEluZGV4ID0gMDtcblxuICAgICAgICBmb3IgKGxldCBpID0gMDsgaSA8IG1hc2subGVuZ3RoOyBpKyspIHtcbiAgICAgICAgICAgIGNvbnN0IG1hc2tDaGFyID0gbWFza1tpXTtcblxuICAgICAgICAgICAgaWYgKG1hc2tSdWxlc1ttYXNrQ2hhcl0pIHtcbiAgICAgICAgICAgICAgICBpZiAoaW5wdXRJbmRleCA8IGNsZWFuSW5wdXQubGVuZ3RoICYmIG1hc2tSdWxlc1ttYXNrQ2hhcl0udGVzdChjbGVhbklucHV0W2lucHV0SW5kZXhdKSkge1xuICAgICAgICAgICAgICAgICAgICBtYXNrZWRWYWx1ZSArPSBjbGVhbklucHV0W2lucHV0SW5kZXhdO1xuICAgICAgICAgICAgICAgICAgICBpbnB1dEluZGV4Kys7XG4gICAgICAgICAgICAgICAgfSBlbHNlIHtcbiAgICAgICAgICAgICAgICAgICAgbWFza2VkVmFsdWUgKz0gcGxhY2Vob2xkZXJDaGFyO1xuICAgICAgICAgICAgICAgIH1cbiAgICAgICAgICAgIH0gZWxzZSB7XG4gICAgICAgICAgICAgICAgbWFza2VkVmFsdWUgKz0gbWFza0NoYXI7XG4gICAgICAgICAgICAgICAgaWYgKGNsZWFuSW5wdXRbaW5wdXRJbmRleF0gPT09IG1hc2tDaGFyKSB7XG4gICAgICAgICAgICAgICAgICAgIGlucHV0SW5kZXgrKztcbiAgICAgICAgICAgICAgICB9XG4gICAgICAgICAgICB9XG4gICAgICAgIH1cblxuICAgICAgICByZXR1cm4gbWFza2VkVmFsdWU7XG4gICAgfTtcblxuXG4gICAgY29uc3QgZ2V0TmV4dEN1cnNvclBvc2l0aW9uID0gKGN1cnNvclBvc2l0aW9uOiBudW1iZXIsIG1hc2tlZFZhbHVlOiBzdHJpbmcpID0+IHtcbiAgICAgICAgZm9yIChsZXQgaSA9IGN1cnNvclBvc2l0aW9uOyBpIDwgbWFza2VkVmFsdWUubGVuZ3RoOyBpKyspIHtcbiAgICAgICAgICAgIGlmIChtYXNrZWRWYWx1ZVtpXSAhPT0gcGxhY2Vob2xkZXJDaGFyIHx8IG1hc2tSdWxlc1ttYXNrW2ldXSkge1xuICAgICAgICAgICAgICAgIHJldHVybiBpO1xuICAgICAgICAgICAgfVxuICAgICAgICB9XG4gICAgICAgIHJldHVybiBtYXNrZWRWYWx1ZS5sZW5ndGg7XG4gICAgfTtcblxuICAgIGNvbnN0IGhhbmRsZUNoYW5nZSA9IChldmVudDogUmVhY3QuQ2hhbmdlRXZlbnQ8SFRNTElucHV0RWxlbWVudD4pID0+IHtcbiAgICAgICAgY29uc3QgaW5wdXQgPSBldmVudC50YXJnZXQudmFsdWU7XG4gICAgICAgIGNvbnN0IGN1cnNvclBvc2l0aW9uID0gZXZlbnQudGFyZ2V0LnNlbGVjdGlvblN0YXJ0ID8/IDA7XG4gICAgICAgIGNvbnN0IG1hc2tlZFZhbHVlID0gYXBwbHlNYXNrKGlucHV0KTtcbiAgICAgICAgc2V0VmFsdWUobWFza2VkVmFsdWUpO1xuICAgICAgICBwcm9wcy5vbkNoYW5nZUhhbmRsZXI/LihldmVudCwgdHJ1ZSwgbWFza2VkVmFsdWUpO1xuXG4gICAgICAgIGNvbnN0IG5leHRDdXJzb3JQb3NpdGlvbiA9IGdldE5leHRDdXJzb3JQb3NpdGlvbihjdXJzb3JQb3NpdGlvbiA/PyAwLCBtYXNrZWRWYWx1ZSk7XG5cbiAgICAgICAgc2V0VGltZW91dCgoKSA9PiB7XG4gICAgICAgICAgICBpbnB1dFJlZi5jdXJyZW50Py5zZXRTZWxlY3Rpb25SYW5nZShuZXh0Q3Vyc29yUG9zaXRpb24sIG5leHRDdXJzb3JQb3NpdGlvbik7XG4gICAgICAgIH0sIDApO1xuICAgIH1cblxuXG5cbiAgICByZXR1cm4gKFxuXG4gICAgICAgIDxpbnB1dFxuICAgICAgICAgICAgcmVmPXtub2RlID0+IHtcbiAgICAgICAgICAgICAgICBpZiAocHJvcHMucmVmKSB7XG4gICAgICAgICAgICAgICAgICAgIHByb3BzLnJlZi5jdXJyZW50ID0gbm9kZSA/PyB1bmRlZmluZWQ7XG4gICAgICAgICAgICAgICAgfVxuICAgICAgICAgICAgfX1cbiAgICAgICAgICAgIHN0eWxlPXtwcm9wcy5zdHlsZX1cbiAgICAgICAgICAgIGlkPXtwcm9wcy5pZH1cbiAgICAgICAgICAgIGNsYXNzTmFtZT17cHJvcHMuY2xhc3NOYW1lfVxuICAgICAgICAgICAgdGFiSW5kZXg9e3Byb3BzLnRhYkluZGV4fVxuICAgICAgICAgICAgdHlwZT17cHJvcHMuc2hvd0FzUGFzc293cmQgPyAncGFzc3dvcmQnIDogJ3RleHQnfVxuICAgICAgICAgICAgdmFsdWU9e3ZhbHVlfVxuICAgICAgICAgICAgb25DaGFuZ2U9e2hhbmRsZUNoYW5nZX1cbiAgICAgICAgICAgIG9uS2V5RG93bj17KGUpID0+IHtcbiAgICAgICAgICAgICAgICBpZiAoZS5rZXkgPT09ICdFbnRlcicpIHtcbiAgICAgICAgICAgICAgICAgICAgcHJvcHMub25FbnRlcktleVByZXNzPy4oZSk7XG4gICAgICAgICAgICAgICAgfVxuICAgICAgICAgICAgfX1cbiAgICAgICAgICAgIG9uRm9jdXM9e3Byb3BzLm9uRm9jdXN9XG4gICAgICAgICAgICBvbkJsdXI9e3Byb3BzLm9uQmx1cn1cbiAgICAgICAgICAgIHBsYWNlaG9sZGVyPXtwbGFjZUhvbGRlcn1cbiAgICAgICAgICAgIG1heExlbmd0aD17cHJvcHMubWF4TGVuZ3RofVxuICAgICAgICAgICAgZGlzYWJsZWQ9e3Byb3BzLmRpc2FibGVkfVxuICAgICAgICAgICAgey4uLnByb3BzLmFyaWFMYWJlbCA/IHsgJ2FyaWEtbGFiZWwnOiBwcm9wcy5hcmlhTGFiZWwgfSA6IHt9fVxuICAgICAgICAgICAgey4uLnByb3BzLmhhc0Vycm9yID8geyAnYXJpYS1pbnZhbGlkJzogdHJ1ZSB9IDoge319XG4gICAgICAgICAgICB7Li4ucHJvcHMuYXJpYVJlcXVpcmVkID8geyAnYXJpYS1yZXF1aXJlZCc6IHRydWUgfSA6IHt9fVxuICAgICAgICAgICAgey4uLnByb3BzLnJlcXVpcmVkID8geyByZXF1aXJlZDogdHJ1ZSB9IDoge319XG4gICAgICAgICAgICBhdXRvQ29tcGxldGU9e3Byb3BzLmF1dG9Db21wbGV0ZS5yZXBsYWNlQWxsKCdfJywgJy0nKX1cbiAgICAgICAgLz5cbiAgICApO1xufVxuXG5leHBvcnQgZGVmYXVsdCBDdXN0b21JbnB1dE1hc2s7IiwiaW1wb3J0IHsgUmVhY3RFbGVtZW50LCBDU1NQcm9wZXJ0aWVzLCBjcmVhdGVFbGVtZW50LCBDaGFuZ2VFdmVudCwgdXNlU3RhdGUsIHVzZUVmZmVjdCwgUmVmIH0gZnJvbSBcInJlYWN0XCI7XG5pbXBvcnQgeyBEZWNpbWFsTW9kZUVudW0sIE1heExlbmd0aFR5cGVFbnVtLCBTZWxlY3RlZEF0dHJpYnV0ZVR5cGVFbnVtIH0gZnJvbSBcInR5cGluZ3MvT3BjZW50ZXJDb3JlVGV4dEJveFByb3BzXCI7XG5pbXBvcnQgY2xhc3NOYW1lcyBmcm9tIFwiY2xhc3NuYW1lc1wiO1xuaW1wb3J0IElucHV0TWFza0VsZW1lbnQgZnJvbSBcIi4vSW5wdXRNYXNrRWxlbWVudFwiO1xuaW1wb3J0IEJpZyBmcm9tIFwiYmlnLmpzXCI7XG5cbmV4cG9ydCBpbnRlcmZhY2UgSW5wdXRFbGVtZW50UHJvcHMge1xuICAgIGlkOiBzdHJpbmc7XG4gICAgcmVmPzogUmVmPEhUTUxJbnB1dEVsZW1lbnQ+O1xuICAgIGlucHV0UmVmPzogUmVhY3QuTXV0YWJsZVJlZk9iamVjdDxIVE1MSW5wdXRFbGVtZW50IHwgdW5kZWZpbmVkPlxuICAgIGNsYXNzTmFtZT86IHN0cmluZztcbiAgICBpbnB1dFR5cGU6IGJvb2xlYW47XG4gICAgc3R5bGU/OiBDU1NQcm9wZXJ0aWVzO1xuICAgIHZhbHVlPzogc3RyaW5nO1xuICAgIHNob3dRUkltYWdlOiBib29sZWFuO1xuICAgIGlzSW5wdXRRUkltYWdlQ2xpY2s/OiBib29sZWFuO1xuICAgIGlzVmFsdWVDaGFuZ2VkQnlTY2FuPzogYm9vbGVhbjtcbiAgICBpbnB1dE1hc2s/OiBzdHJpbmc7XG4gICAgcGxhY2VIb2xkZXJJbnB1dD86IHN0cmluZztcbiAgICBjbGlja2FibGU/OiBib29sZWFuO1xuICAgIHRhYkluZGV4PzogbnVtYmVyO1xuICAgIG1heExlbmd0aFR5cGU6IE1heExlbmd0aFR5cGVFbnVtO1xuICAgIGFyaWFSZXF1aXJlZDogYm9vbGVhbjtcbiAgICBhcmlhTGFiZWw/OiBzdHJpbmdcbiAgICBhdXRvQ29tcGxldGU6IHN0cmluZztcbiAgICBtYXhMZW5ndGg6IG51bWJlcjtcbiAgICBhdHRyaWJ1dGVUeXBlPzogU2VsZWN0ZWRBdHRyaWJ1dGVUeXBlRW51bTtcbiAgICBkZWNpbWFsTW9kZT86IERlY2ltYWxNb2RlRW51bTtcbiAgICBkZWNpbWFsUHJlY2lzaW9uPzogbnVtYmVyO1xuICAgIGdyb3VwRGlnaXRzPzogYm9vbGVhbjtcbiAgICBvbkVudGVyPzogKHNob3dRUkltYWdlOiBib29sZWFuKSA9PiB2b2lkO1xuICAgIG9uQ2hhbmdlPzogKHZhbHVlOiBzdHJpbmcgfCBCaWcpID0+IHZvaWQ7XG4gICAgb25MZWF2ZT86ICh2YWx1ZTogc3RyaW5nIHwgQmlnLCBjaGFuZ2VkOiBib29sZWFuLCBcbiAgICAgICAgc2hvd1FSSW1hZ2U6IGJvb2xlYW4sIGlzVmFsdWVJbnZhbGlkOiBib29sZWFuKSA9PiB2b2lkO1xuICAgIGFwcGx5Q2hhbmdlPzogKHZhbHVlOiBzdHJpbmcgfCBCaWcpID0+IHZvaWQ7XG4gICAgb25FbnRlcktleVByZXNzPzogKHZhbHVlOiBzdHJpbmcgfCBCaWcsIGNoYW5nZWQ6IGJvb2xlYW4sIGlzVmFsdWVJbnZhbGlkOiBib29sZWFuKSA9PiB2b2lkO1xuICAgIGdldFJlZj86IChub2RlOiBIVE1MRWxlbWVudCkgPT4gdm9pZDtcbiAgICBkaXNhYmxlZD86IGJvb2xlYW47XG4gICAgaGFzRXJyb3I/OiBib29sZWFuO1xuICAgIHJlcXVpcmVkPzogYm9vbGVhbjtcbn1cblxuaW50ZXJmYWNlIElucHV0RWxlbWVudFN0YXRlIHtcbiAgICBlZGl0ZWRWYWx1ZT86IHN0cmluZztcbiAgICBoYXNFcnJvcj86IGJvb2xlYW47XG4gICAgaXNGb2N1c2VkPzogYm9vbGVhbjtcbn1cblxuZXhwb3J0IGZ1bmN0aW9uIElucHV0RWxlbWVudChwcm9wczogSW5wdXRFbGVtZW50UHJvcHMpOiBSZWFjdEVsZW1lbnQge1xuICAgIGNvbnN0IHsgb25MZWF2ZSwgdmFsdWUsIHBsYWNlSG9sZGVySW5wdXQsIHRhYkluZGV4LCBhcmlhUmVxdWlyZWQsIHNob3dRUkltYWdlLCBtYXhMZW5ndGhUeXBlLCBpbnB1dE1hc2ssXG4gICAgICAgIGRpc2FibGVkLCBvbkVudGVyLCBvbkVudGVyS2V5UHJlc3N9ID0gcHJvcHM7XG5cbiAgICB1c2VFZmZlY3QoKCkgPT4ge1xuICAgICAgICBpZiAoIXByb3BzLmlzVmFsdWVDaGFuZ2VkQnlTY2FuKSB7XG4gICAgICAgICAgICAvLyBSZW1vdmUgdGhlICdvcGNvcmUtaW5wdXQtYWN0aXZlJyBjbGFzcyBmcm9tIGFsbCBpbnB1dCBlbGVtZW50c1xuICAgICAgICAgICAgZG9jdW1lbnQucXVlcnlTZWxlY3RvckFsbCgnaW5wdXQub3Bjb3JlLWlucHV0LWFjdGl2ZScpLmZvckVhY2goKGVsZW1lbnQpID0+IHtcbiAgICAgICAgICAgICAgICBlbGVtZW50LmNsYXNzTGlzdC5yZW1vdmUoJ29wY29yZS1pbnB1dC1hY3RpdmUnKTtcbiAgICAgICAgICAgIH0pO1xuICAgICAgICB9XG4gICAgfSwgW3Byb3BzLmlzSW5wdXRRUkltYWdlQ2xpY2tdKTtcbiAgICBjb25zdCBpbnB1dEFjdGl2ZUNsYXNzID0gcHJvcHMuaXNJbnB1dFFSSW1hZ2VDbGljayB8fCBwcm9wcy5pc1ZhbHVlQ2hhbmdlZEJ5U2NhbiA/IFwib3Bjb3JlLWlucHV0LWFjdGl2ZVwiIDogXCJcIjtcbiAgICBjb25zdCBpbnB1dENsYXNzID0gc2hvd1FSSW1hZ2UgPyBcIndpZGdldC1vcGNvcmV0ZXh0Ym94LXBhZGRpbmctcmlnaHRcIiA6IFwiXCI7XG4gICAgY29uc3QgY2xhc3NOYW1lc0lucCA9IGNsYXNzTmFtZXMoXCJmb3JtLWNvbnRyb2xcIiwgaW5wdXRDbGFzcyk7XG4gICAgY29uc3QgbWF4TGVuZ3RoSW5wdXQgPSBnZXRNYXhMZW5ndGgoKTtcbiAgICBjb25zdCB1c2VySW5wdXRNYXNrID0gaW5wdXRNYXNrICE9PSB1bmRlZmluZWQgJiYgaW5wdXRNYXNrLnRyaW0oKSAhPT0gJycgPyBpbnB1dE1hc2sgOiAnJ1xuXG4gICAgY29uc3QgW3N0YXRlLCBzZXRTdGF0ZV0gPSB1c2VTdGF0ZTxJbnB1dEVsZW1lbnRTdGF0ZT4oe1xuICAgICAgICBlZGl0ZWRWYWx1ZTogdW5kZWZpbmVkLCBpc0ZvY3VzZWQ6IGZhbHNlLCBoYXNFcnJvcjogcHJvcHMuaGFzRXJyb3JcbiAgICB9KTtcblxuICAgIHVzZUVmZmVjdCgoKSA9PlxuICAgICAgICBzZXRTdGF0ZSgocHJldlN0YXRlKSA9PiAoeyAuLi5wcmV2U3RhdGUsIGVkaXRlZFZhbHVlOiB1bmRlZmluZWQgfSkpXG4gICAgICAgICwgW3ZhbHVlXSk7XG5cbiAgICBmdW5jdGlvbiBnZXRDdXJyZW50VmFsdWUoKTogc3RyaW5nIHwgQmlnIHtcbiAgICAgICAgcmV0dXJuIHN0YXRlLmVkaXRlZFZhbHVlICE9PSB1bmRlZmluZWQgPyBzdGF0ZS5lZGl0ZWRWYWx1ZSA6IHZhbHVlICE9PSB1bmRlZmluZWQgPyB2YWx1ZSA6ICcnO1xuICAgIH1cblxuICAgIGZ1bmN0aW9uIGdldEN1cnJlbnREaXNwbGF5VmFsdWUoKTogc3RyaW5nIHwgQmlnIHtcbiAgICAgICAgbGV0IGN1cnJlbnRWYWx1ZSA9IGdldEN1cnJlbnRWYWx1ZSgpLnRvU3RyaW5nKCk7XG5cbiAgICAgICAgaWYgKHByb3BzLmF0dHJpYnV0ZVR5cGUgPT09IFwiZGVjaW1hbFwiKSB7XG4gICAgICAgICAgICByZXR1cm4gZ2V0Rm9ybWF0dGVkRGVjaW1hbFN0cmluZyhjdXJyZW50VmFsdWUsIHByb3BzLmFwcGx5Q2hhbmdlICE9PSB1bmRlZmluZWQsIHN0YXRlLmlzRm9jdXNlZCA/PyBmYWxzZSk7XG4gICAgICAgIH0gZWxzZSBpZiAocHJvcHMuYXR0cmlidXRlVHlwZSA9PT0gXCJpbnRlZ2VyXCIpIHtcbiAgICAgICAgICAgIHJldHVybiBnZXRGb3JtYXR0ZWRJbnRlZ2VyU3RyaW5nKGN1cnJlbnRWYWx1ZSwgc3RhdGUuaXNGb2N1c2VkID8/IGZhbHNlKTtcbiAgICAgICAgfSBlbHNlIGlmIChwcm9wcy5hdHRyaWJ1dGVUeXBlID09PSBcImxvbmdcIikge1xuICAgICAgICAgICAgcmV0dXJuIGdldEZvcm1hdHRlZExvbmdTdHJpbmcoY3VycmVudFZhbHVlLCBzdGF0ZS5pc0ZvY3VzZWQgPz8gZmFsc2UpO1xuICAgICAgICB9XG5cbiAgICAgICAgcmV0dXJuIGN1cnJlbnRWYWx1ZTtcbiAgICB9XG5cbiAgICBjb25zdCBnZXRWYWx1ZUJ5QXR0cmlidXRlVHlwZSA9ICh2YWx1ZTogc3RyaW5nKTogeyBpc1ZhbGlkOiBib29sZWFuLCBwYXJzZWRWYWx1ZT86IHN0cmluZyB8IEJpZywgcGFyc2VkVmFsdWVCaWc/OiBCaWcgfSA9PiB7XG4gICAgICAgIGlmIChwcm9wcy5hdHRyaWJ1dGVUeXBlID09PSBcImRlY2ltYWxcIikge1xuICAgICAgICAgICAgaWYgKHZhbHVlID09PSAnJykge1xuICAgICAgICAgICAgICAgIHJldHVybiB7IGlzVmFsaWQ6IHRydWUsIHBhcnNlZFZhbHVlOiAnJyB9O1xuICAgICAgICAgICAgfVxuICAgICAgICAgICAgbGV0IGRlY2ltYWxWYWx1ZTogQmlnO1xuICAgICAgICAgICAgdHJ5IHtcbiAgICAgICAgICAgICAgICBkZWNpbWFsVmFsdWUgPSBuZXcgQmlnKHZhbHVlKTtcbiAgICAgICAgICAgIH0gY2F0Y2ggKGVycm9yKSB7XG4gICAgICAgICAgICAgICAgcmV0dXJuIHsgaXNWYWxpZDogZmFsc2UsIHBhcnNlZFZhbHVlOiB2YWx1ZSB9O1xuICAgICAgICAgICAgfVxuICAgICAgICAgICAgbGV0IGRlY2ltYWxWYWx1ZVBhcnNlZDtcbiAgICAgICAgICAgIGxldCBkZWNpbWFsTW9kZSA9IHByb3BzLmRlY2ltYWxNb2RlO1xuICAgICAgICAgICAgbGV0IGRlY2ltYWxQcmVjaXNpb24gPSBwcm9wcy5kZWNpbWFsUHJlY2lzaW9uO1xuXG4gICAgICAgICAgICBpZiAoZGVjaW1hbE1vZGUgPT09IFwiZml4ZWRcIikge1xuICAgICAgICAgICAgICAgIGlmIChkZWNpbWFsUHJlY2lzaW9uICE9PSB1bmRlZmluZWQgJiYgZGVjaW1hbFByZWNpc2lvbiAhPT0gbnVsbCkge1xuICAgICAgICAgICAgICAgICAgICAvLyBGaW5kIHRoZSBudW1iZXIgb2YgZGlnaXRzIGJlZm9yZSB0aGUgZGVjaW1hbCBwb2ludFxuICAgICAgICAgICAgICAgICAgICBjb25zdCBpbnRlZ2VyUGFydCA9IHZhbHVlLnNwbGl0KFwiLlwiKVswXTtcbiAgICAgICAgICAgICAgICAgICAgY29uc3QgZGlnaXRzQmVmb3JlRGVjaW1hbCA9IHBhcnNlSW50KGludGVnZXJQYXJ0KSA8PSAwID8gaW50ZWdlclBhcnQubGVuZ3RoIC0gMSA6IGludGVnZXJQYXJ0Lmxlbmd0aDtcbiAgICAgICAgICAgICAgICAgICAgZGVjaW1hbFZhbHVlUGFyc2VkID0gZGVjaW1hbFZhbHVlLnRvUHJlY2lzaW9uKGRlY2ltYWxQcmVjaXNpb24gKyBkaWdpdHNCZWZvcmVEZWNpbWFsKTtcbiAgICAgICAgICAgICAgICB9XG4gICAgICAgICAgICB9XG4gICAgICAgICAgICBlbHNlIGlmIChkZWNpbWFsTW9kZSA9PT0gXCJhdXRvXCIpIHtcbiAgICAgICAgICAgICAgICBkZWNpbWFsVmFsdWVQYXJzZWQgPSBkZWNpbWFsVmFsdWUudG9TdHJpbmcoKTtcbiAgICAgICAgICAgIH1cbiAgICAgICAgICAgIHJldHVybiB7IGlzVmFsaWQ6IHRydWUsIHBhcnNlZFZhbHVlOiBuZXcgQmlnKGRlY2ltYWxWYWx1ZVBhcnNlZCA/PyAnJyksIHBhcnNlZFZhbHVlQmlnOiBkZWNpbWFsVmFsdWUgfTtcbiAgICAgICAgfVxuICAgICAgICBlbHNlIGlmIChwcm9wcy5hdHRyaWJ1dGVUeXBlID09PSBcImludGVnZXJcIikge1xuICAgICAgICAgICAgaWYgKHZhbHVlID09PSAnJykge1xuICAgICAgICAgICAgICAgIHJldHVybiB7IGlzVmFsaWQ6IHRydWUsIHBhcnNlZFZhbHVlOiAnJyB9O1xuICAgICAgICAgICAgfVxuICAgICAgICAgICAgY29uc3QgaW50ZWdlclJlZ2V4ID0gL14tP1xcZHsxLDEwfSQvO1xuICAgICAgICAgICAgaWYgKCFpbnRlZ2VyUmVnZXgudGVzdCh2YWx1ZSkpIHtcbiAgICAgICAgICAgICAgICByZXR1cm4geyBpc1ZhbGlkOiBmYWxzZSwgcGFyc2VkVmFsdWU6IHZhbHVlIH07XG4gICAgICAgICAgICB9XG4gICAgICAgICAgICByZXR1cm4geyBpc1ZhbGlkOiB0cnVlLCBwYXJzZWRWYWx1ZTogbmV3IEJpZyh2YWx1ZSkgfTtcbiAgICAgICAgfVxuICAgICAgICBlbHNlIGlmIChwcm9wcy5hdHRyaWJ1dGVUeXBlID09PSBcImxvbmdcIikge1xuICAgICAgICAgICAgaWYgKHZhbHVlID09PSAnJykge1xuICAgICAgICAgICAgICAgIHJldHVybiB7IGlzVmFsaWQ6IHRydWUsIHBhcnNlZFZhbHVlOiAnJyB9O1xuICAgICAgICAgICAgfVxuICAgICAgICAgICAgY29uc3QgbG9uZ1JlZ2V4ID0gL14tP1xcZHsxLDE5fSQvO1xuICAgICAgICAgICAgaWYgKCFsb25nUmVnZXgudGVzdCh2YWx1ZSkpIHtcbiAgICAgICAgICAgICAgICByZXR1cm4geyBpc1ZhbGlkOiBmYWxzZSwgcGFyc2VkVmFsdWU6IHZhbHVlIH07XG4gICAgICAgICAgICB9XG4gICAgICAgICAgICByZXR1cm4geyBpc1ZhbGlkOiB0cnVlLCBwYXJzZWRWYWx1ZTogbmV3IEJpZyh2YWx1ZSkgfTtcbiAgICAgICAgfVxuXG4gICAgICAgIHJldHVybiB7IGlzVmFsaWQ6IHRydWUsIHBhcnNlZFZhbHVlOiB2YWx1ZSB9O1xuICAgIH1cblxuICAgIGNvbnN0IGdldEZvcm1hdHRlZERlY2ltYWxTdHJpbmcgPSAodmFsdWU6IHN0cmluZyxcbiAgICAgICAgYXBwbHlDaGFuZ2VXaGlsZUVkaXRpbmc6IGJvb2xlYW4sIGlzRm9jdXNlZDogYm9vbGVhbik6IHN0cmluZyA9PiB7XG4gICAgICAgIGlmICh2YWx1ZSA9PT0gJycgfHwgc3RhdGUuaGFzRXJyb3IpIHtcbiAgICAgICAgICAgIHJldHVybiB2YWx1ZTtcbiAgICAgICAgfVxuICAgICAgICBjb25zdCBbaW50ZWdlclBhcnQsIGRlY2ltYWxQYXJ0XSA9IHZhbHVlLnNwbGl0KFwiLlwiKTtcbiAgICAgICAgbGV0IGdyb3VwRm9ybWF0dGVkSW50ZWdlclBhcnQgPSAnJztcbiAgICAgICAgbGV0IGRpZ2l0c0JlZm9yZURlY2ltYWwgPSBwYXJzZUludChpbnRlZ2VyUGFydCkgPD0gMCA/IGludGVnZXJQYXJ0Lmxlbmd0aCAtIDEgOiBpbnRlZ2VyUGFydC5sZW5ndGg7XG4gICAgICAgIGxldCBkZWNpbWFsUHJlY2lzaW9uID0gcHJvcHMuZGVjaW1hbE1vZGUgPT09IFwiZml4ZWRcIiA/IHByb3BzLmRlY2ltYWxQcmVjaXNpb24gOiBwcm9wcy5kZWNpbWFsTW9kZSA9PT0gXCJhdXRvXCIgPyBkZWNpbWFsUGFydD8ubGVuZ3RoIDogdW5kZWZpbmVkO1xuICAgICAgICBpZiAoZGVjaW1hbFByZWNpc2lvbiAgIT09IHVuZGVmaW5lZCkge1xuICAgICAgICAgICAgZGVjaW1hbFByZWNpc2lvbiA9IHBhcnNlSW50KGludGVnZXJQYXJ0KSA9PT0gMCA/IGRlY2ltYWxQcmVjaXNpb24gLSBjb3VudExlYWRpbmdaZXJvcyhkZWNpbWFsUGFydCkgOiBkZWNpbWFsUHJlY2lzaW9uO1xuICAgICAgICAgICAgaWYoaW50ZWdlclBhcnQgPT09ICctMCcgJiYgZGVjaW1hbFByZWNpc2lvbiA+IDApXG4gICAgICAgICAgICAgICAgZGVjaW1hbFByZWNpc2lvbi0tO1xuICAgICAgICB9XG4gICAgICAgIGxldCBmb3JtYXR0ZWREZWNpbWFsID0gbmV3IEJpZyhgJHtpbnRlZ2VyUGFydCA/PyAwfS4ke2RlY2ltYWxQYXJ0ID8/IDB9YCk7XG4gICAgICAgIGxldCBmb3JtYXR0ZWREZWNpbWFsU3RyaW5nID0gZm9ybWF0dGVkRGVjaW1hbC50b1ByZWNpc2lvbihkZWNpbWFsUHJlY2lzaW9uID8gXG4gICAgICAgICAgICAoZGVjaW1hbFByZWNpc2lvbiArIGRpZ2l0c0JlZm9yZURlY2ltYWwpIDogdW5kZWZpbmVkKTtcbiAgICAgICAgbGV0IFtmb3JtYXR0ZWRJbnRlZ2VyUGFydCwgZm9ybWF0dGVkRGVjaW1hbFBhcnRdID0gZm9ybWF0dGVkRGVjaW1hbFN0cmluZy5zcGxpdChcIi5cIik7XG4gICAgICAgIGZvcm1hdHRlZEludGVnZXJQYXJ0ID0gaW50ZWdlclBhcnQgPT09ICctMCcgPyAnLTAnIDogZm9ybWF0dGVkSW50ZWdlclBhcnQ7XG4gICAgICAgIGlmIChhcHBseUNoYW5nZVdoaWxlRWRpdGluZyB8fCAoIWFwcGx5Q2hhbmdlV2hpbGVFZGl0aW5nICYmICFpc0ZvY3VzZWQpKSB7XG4gICAgICAgICAgICBpZiAocHJvcHMuZ3JvdXBEaWdpdHMgJiYgZm9ybWF0dGVkSW50ZWdlclBhcnQubGVuZ3RoID4gMykge1xuICAgICAgICAgICAgICAgIGdyb3VwRm9ybWF0dGVkSW50ZWdlclBhcnQgPSBmb3JtYXR0ZWRJbnRlZ2VyUGFydC5yZXBsYWNlKC8oXFxkKSg/PShcXGR7M30pKyg/IVxcZCkpL2csIFwiJDEsXCIpO1xuICAgICAgICAgICAgICAgIHJldHVybiBmb3JtYXR0ZWREZWNpbWFsUGFydCA/IGAke2dyb3VwRm9ybWF0dGVkSW50ZWdlclBhcnR9LiR7Zm9ybWF0dGVkRGVjaW1hbFBhcnR9YCA6IGdyb3VwRm9ybWF0dGVkSW50ZWdlclBhcnQ7XG4gICAgICAgICAgICB9IGVsc2Uge1xuICAgICAgICAgICAgICAgIHJldHVybiBmb3JtYXR0ZWREZWNpbWFsUGFydCA/IGAke2Zvcm1hdHRlZEludGVnZXJQYXJ0fS4ke2Zvcm1hdHRlZERlY2ltYWxQYXJ0fWAgOiBmb3JtYXR0ZWRJbnRlZ2VyUGFydDtcbiAgICAgICAgICAgIH1cbiAgICAgICAgfSBlbHNlIGlmIChpc0ZvY3VzZWQpIHtcbiAgICAgICAgICAgIHJldHVybiBkZWNpbWFsUGFydCAhPT0gdW5kZWZpbmVkID8gYCR7Zm9ybWF0dGVkSW50ZWdlclBhcnR9LiR7ZGVjaW1hbFBhcnR9YCA6IGludGVnZXJQYXJ0O1xuICAgICAgICB9IGVsc2Uge1xuICAgICAgICAgICAgcmV0dXJuIGZvcm1hdHRlZERlY2ltYWxQYXJ0ID8gYCR7Zm9ybWF0dGVkSW50ZWdlclBhcnR9LiR7Zm9ybWF0dGVkRGVjaW1hbFBhcnR9YCA6IGZvcm1hdHRlZEludGVnZXJQYXJ0O1xuICAgICAgICB9XG4gICAgfVxuXG4gICAgY29uc3QgZ2V0Rm9ybWF0dGVkSW50ZWdlclN0cmluZyA9ICh2YWx1ZTogc3RyaW5nLCBpc0ZvY3VzZWQ6IGJvb2xlYW4pOiBzdHJpbmcgPT4ge1xuICAgICAgICBpZiAoc3RhdGUuaGFzRXJyb3IpXG4gICAgICAgICAgICByZXR1cm4gdmFsdWU7XG4gICAgICAgIGxldCBncm91cEZvcm1hdHRlZEludGVnZXJQYXJ0ID0gJyc7XG4gICAgICAgIGlmIChwcm9wcy5ncm91cERpZ2l0cyAmJiB2YWx1ZS5sZW5ndGggPiAzICYmICFpc0ZvY3VzZWQpIHtcbiAgICAgICAgICAgIGdyb3VwRm9ybWF0dGVkSW50ZWdlclBhcnQgPSB2YWx1ZS5yZXBsYWNlKC8oXFxkKSg/PShcXGR7M30pKyg/IVxcZCkpL2csIFwiJDEsXCIpO1xuICAgICAgICAgICAgcmV0dXJuIGdyb3VwRm9ybWF0dGVkSW50ZWdlclBhcnQ7XG4gICAgICAgIH0gZWxzZVxuICAgICAgICAgICAgcmV0dXJuIHZhbHVlO1xuICAgIH1cblxuICAgIGNvbnN0IGdldEZvcm1hdHRlZExvbmdTdHJpbmcgPSAodmFsdWU6IHN0cmluZywgaXNGb2N1c2VkOiBib29sZWFuKTogc3RyaW5nID0+IHtcbiAgICAgICAgaWYgKHN0YXRlLmhhc0Vycm9yKVxuICAgICAgICAgICAgcmV0dXJuIHZhbHVlO1xuICAgICAgICBsZXQgZ3JvdXBGb3JtYXR0ZWRMb25nUGFydCA9ICcnO1xuICAgICAgICBpZiAocHJvcHMuZ3JvdXBEaWdpdHMgJiYgdmFsdWUubGVuZ3RoID4gMyAmJiAhaXNGb2N1c2VkKSB7XG4gICAgICAgICAgICBncm91cEZvcm1hdHRlZExvbmdQYXJ0ID0gdmFsdWUucmVwbGFjZSgvKFxcZCkoPz0oXFxkezN9KSsoPyFcXGQpKS9nLCBcIiQxLFwiKTtcbiAgICAgICAgICAgIHJldHVybiBncm91cEZvcm1hdHRlZExvbmdQYXJ0O1xuICAgICAgICB9IGVsc2VcbiAgICAgICAgICAgIHJldHVybiB2YWx1ZTtcbiAgICB9XG5cbiAgICBmdW5jdGlvbiBjb3VudExlYWRpbmdaZXJvcyh2YWx1ZSA6IHN0cmluZyB8IEJpZyk6IG51bWJlciB7XG4gICAgICAgIGNvbnN0IHN0ciA9IHZhbHVlLnRvU3RyaW5nKCk7XG4gICAgICAgIGxldCBjb3VudCA9IDA7XG4gICAgICAgIGZvciAobGV0IGkgPSAwOyBpIDwgc3RyLmxlbmd0aDsgaSsrKSB7XG4gICAgICAgICAgICBpZiAoc3RyW2ldID09PSAnMCcpIHtcbiAgICAgICAgICAgICAgICBjb3VudCsrO1xuICAgICAgICAgICAgfSBlbHNlIHtcbiAgICAgICAgICAgICAgICBicmVhaztcbiAgICAgICAgICAgIH1cbiAgICAgICAgfVxuICAgICAgICByZXR1cm4gY291bnQ7XG4gICAgfVxuXG4gICAgZnVuY3Rpb24gZ2V0TWF4TGVuZ3RoKCk6IG51bWJlciB7XG4gICAgICAgIGlmIChtYXhMZW5ndGhUeXBlID09PSBcImRlZmF1bHRcIilcbiAgICAgICAgICAgIHJldHVybiAyMDA7XG4gICAgICAgIGlmIChtYXhMZW5ndGhUeXBlID09PSBcImN1c3RvbVwiKVxuICAgICAgICAgICAgcmV0dXJuIHByb3BzLm1heExlbmd0aDtcbiAgICAgICAgZWxzZVxuICAgICAgICAgICAgcmV0dXJuIDA7XG4gICAgfVxuXG4gICAgY29uc3Qgb25CbHVyID0gKCk6IHZvaWQgPT4ge1xuICAgICAgICBsZXQgY3VycmVudFZhbHVlID0gZ2V0Q3VycmVudFZhbHVlKCk7XG4gICAgICAgIGxldCB2YWx1ZVRvQmVTZXQgPSBnZXRWYWx1ZUJ5QXR0cmlidXRlVHlwZShjdXJyZW50VmFsdWUudG9TdHJpbmcoKSk7XG4gICAgICAgIGlmICh2YWx1ZVRvQmVTZXQuaXNWYWxpZCAmJiB2YWx1ZVRvQmVTZXQucGFyc2VkVmFsdWUgIT09IHVuZGVmaW5lZCkge1xuICAgICAgICAgICAgb25MZWF2ZT8uKHZhbHVlVG9CZVNldC5wYXJzZWRWYWx1ZSwgdmFsdWVUb0JlU2V0LnBhcnNlZFZhbHVlLnRvU3RyaW5nKCkgIT09IHZhbHVlLCBmYWxzZSwgZmFsc2UpO1xuICAgICAgICB9XG4gICAgICAgIGVsc2VcbiAgICAgICAgICAgIG9uTGVhdmU/LihjdXJyZW50VmFsdWUsIGZhbHNlLCBmYWxzZSwgdHJ1ZSk7XG5cbiAgICAgICAgaWYgKHZhbHVlVG9CZVNldC5pc1ZhbGlkKVxuICAgICAgICAgICAgc2V0U3RhdGUoe1xuICAgICAgICAgICAgICAgIGVkaXRlZFZhbHVlOiB2YWx1ZVRvQmVTZXQuaXNWYWxpZCA/IHVuZGVmaW5lZCA6IHZhbHVlVG9CZVNldC5wYXJzZWRWYWx1ZT8udG9TdHJpbmcoKSxcbiAgICAgICAgICAgICAgICBpc0ZvY3VzZWQ6IGZhbHNlLCBoYXNFcnJvcjogIXZhbHVlVG9CZVNldC5pc1ZhbGlkXG4gICAgICAgICAgICB9KTtcbiAgICB9XG5cbiAgICBjb25zdCBvbkVudGVySGFuZGxlID0gKCk6IHZvaWQgPT4ge1xuICAgICAgICBzZXRTdGF0ZSgocHJldlN0YXRlKSA9PiAoeyAuLi5wcmV2U3RhdGUsIGlzRm9jdXNlZDogdHJ1ZSB9KSk7XG4gICAgICAgIG9uRW50ZXI/Lih0cnVlKTtcbiAgICB9XG5cbiAgICBjb25zdCBvbkVudGVyS2V5UHJlc3NIYW5kbGVyID0gKGV2ZW50OiBSZWFjdC5LZXlib2FyZEV2ZW50PEhUTUxJbnB1dEVsZW1lbnQ+KTogdm9pZCA9PiB7ICBcbiAgICAgICAgZXZlbnQucHJldmVudERlZmF1bHQoKTsgIFxuICAgICAgICBsZXQgY3VycmVudFZhbHVlID0gZ2V0Q3VycmVudFZhbHVlKCk7XG4gICAgICAgIGxldCB2YWx1ZVRvQmVTZXQgPSBnZXRWYWx1ZUJ5QXR0cmlidXRlVHlwZShjdXJyZW50VmFsdWUudG9TdHJpbmcoKSk7XG4gICAgICAgIGlmICh2YWx1ZVRvQmVTZXQuaXNWYWxpZCAmJiB2YWx1ZVRvQmVTZXQucGFyc2VkVmFsdWUgIT09IHVuZGVmaW5lZCkge1xuICAgICAgICAgICAgb25FbnRlcktleVByZXNzPy4odmFsdWVUb0JlU2V0LnBhcnNlZFZhbHVlLCB2YWx1ZVRvQmVTZXQucGFyc2VkVmFsdWUudG9TdHJpbmcoKSAhPT0gdmFsdWUsIGZhbHNlKTtcbiAgICAgICAgfVxuICAgICAgICBlbHNlXG4gICAgICAgICAgICBvbkVudGVyS2V5UHJlc3M/LihjdXJyZW50VmFsdWUsIGZhbHNlLCB0cnVlKTtcblxuICAgIH1cblxuICAgIGNvbnN0IG9uQ2hhbmdlSGFuZGxlID0gKGV2ZW50OiBDaGFuZ2VFdmVudDxIVE1MSW5wdXRFbGVtZW50PiwgaXNJbnB1dE1hc2tWYWx1ZTogYm9vbGVhbiA9IGZhbHNlLCB2YWx1ZTogc3RyaW5nID0gJycpOiB2b2lkID0+IHtcbiAgICAgICAgdmFyIHZhbHVlU2hvdWxkQmVTZXQgPSBmYWxzZTtcbiAgICAgICAgdmFyIHZhbHVlVG9CZVNldCA9IGlzSW5wdXRNYXNrVmFsdWUgPyB2YWx1ZSA6IGV2ZW50LmN1cnJlbnRUYXJnZXQudmFsdWU7XG4gICAgICAgIGlmIChtYXhMZW5ndGhJbnB1dCA9PT0gMCB8fCB2YWx1ZVRvQmVTZXQubGVuZ3RoIDw9IG1heExlbmd0aElucHV0KVxuICAgICAgICAgICAgdmFsdWVTaG91bGRCZVNldCA9IHRydWU7XG5cbiAgICAgICAgaWYgKHZhbHVlU2hvdWxkQmVTZXQpIHtcbiAgICAgICAgICAgIGxldCB7IGlzVmFsaWQsIHBhcnNlZFZhbHVlIH0gPSBnZXRWYWx1ZUJ5QXR0cmlidXRlVHlwZSh2YWx1ZVRvQmVTZXQpO1xuICAgICAgICAgICAgc2V0U3RhdGUoKHByZXZTdGF0ZSkgPT4gKHsgLi4ucHJldlN0YXRlLCBlZGl0ZWRWYWx1ZTogdmFsdWVUb0JlU2V0LCBoYXNFcnJvcjogIWlzVmFsaWQgfSkpO1xuICAgICAgICAgICAgaWYgKHByb3BzLmFwcGx5Q2hhbmdlICE9PSB1bmRlZmluZWQpIHtcbiAgICAgICAgICAgICAgICBpZiAoaXNWYWxpZCAmJiBwYXJzZWRWYWx1ZSAhPT0gdW5kZWZpbmVkKSB7XG4gICAgICAgICAgICAgICAgICAgIHByb3BzLmFwcGx5Q2hhbmdlKHBhcnNlZFZhbHVlKTtcbiAgICAgICAgICAgICAgICB9XG4gICAgICAgICAgICB9XG4gICAgICAgIH1cbiAgICB9XG5cbiAgICBjb25zdCBpbnB1dFN0eWxlOiBDU1NQcm9wZXJ0aWVzID0ge1xuICAgICAgICB3aWR0aDogXCIxMDAlXCJcbiAgICB9XG5cbiAgICBmdW5jdGlvbiBnZXRJbnB1dENsYXNzTmFtZXMoKTogc3RyaW5nIHtcbiAgICAgICAgbGV0IGNsYXNzTmFtZXNSZXN1bHQgPSBwcm9wcy5pc0lucHV0UVJJbWFnZUNsaWNrIHx8IHByb3BzLmlzVmFsdWVDaGFuZ2VkQnlTY2FuID8gY2xhc3NOYW1lcyhjbGFzc05hbWVzSW5wLCBpbnB1dEFjdGl2ZUNsYXNzKSA6IGNsYXNzTmFtZXNJbnA7XG4gICAgICAgIHNldFRpbWVvdXQoKCkgPT4ge1xuICAgICAgICAgICAgcmV0dXJuIGNsYXNzTmFtZXNSZXN1bHQ7XG4gICAgICAgIH0sIDUwMCk7XG4gICAgICAgIHJldHVybiBjbGFzc05hbWVzUmVzdWx0O1xuICAgIH1cblxuICAgIHJldHVybiAoXG4gICAgICAgIHVzZXJJbnB1dE1hc2sgPT09ICcnID9cbiAgICAgICAgICAgIDxpbnB1dFxuICAgICAgICAgICAgICAgIHJlZj17bm9kZSA9PiB7XG4gICAgICAgICAgICAgICAgICAgIGlmIChwcm9wcy5pbnB1dFJlZikge1xuICAgICAgICAgICAgICAgICAgICAgICAgcHJvcHMuaW5wdXRSZWYuY3VycmVudCA9IG5vZGUgPz8gdW5kZWZpbmVkO1xuICAgICAgICAgICAgICAgICAgICB9XG4gICAgICAgICAgICAgICAgfX1cbiAgICAgICAgICAgICAgICBzdHlsZT17aW5wdXRTdHlsZX1cbiAgICAgICAgICAgICAgICBpZD17cHJvcHMuaWR9XG4gICAgICAgICAgICAgICAgY2xhc3NOYW1lPXtnZXRJbnB1dENsYXNzTmFtZXMoKX1cbiAgICAgICAgICAgICAgICB0YWJJbmRleD17dGFiSW5kZXh9XG4gICAgICAgICAgICAgICAgdmFsdWU9e2dldEN1cnJlbnREaXNwbGF5VmFsdWUoKS50b1N0cmluZygpfVxuICAgICAgICAgICAgICAgIHR5cGU9e3Byb3BzLmlucHV0VHlwZSA/IFwicGFzc3dvcmRcIiA6IFwidGV4dFwifVxuICAgICAgICAgICAgICAgIG1heExlbmd0aD17bWF4TGVuZ3RoSW5wdXQgPT09IDAgPyB1bmRlZmluZWQgOiBtYXhMZW5ndGhJbnB1dH1cbiAgICAgICAgICAgICAgICBvbkZvY3VzPXtvbkVudGVySGFuZGxlfVxuICAgICAgICAgICAgICAgIG9uS2V5RG93bj17KGV2ZW50KSA9PiB7XG4gICAgICAgICAgICAgICAgICAgIGlmIChldmVudC5rZXkgPT09IFwiRW50ZXJcIikge1xuICAgICAgICAgICAgICAgICAgICAgICAgb25FbnRlcktleVByZXNzSGFuZGxlcihldmVudCk7XG4gICAgICAgICAgICAgICAgICAgIH1cbiAgICAgICAgICAgICAgICB9fVxuICAgICAgICAgICAgICAgIHBsYWNlaG9sZGVyPXtwbGFjZUhvbGRlcklucHV0fVxuICAgICAgICAgICAgICAgIG9uQmx1cj17b25CbHVyfVxuICAgICAgICAgICAgICAgIG9uQ2hhbmdlPXsoZXZlbnQpID0+IG9uQ2hhbmdlSGFuZGxlKGV2ZW50KX1cbiAgICAgICAgICAgICAgICBkaXNhYmxlZD17ZGlzYWJsZWR9XG4gICAgICAgICAgICAgICAgey4uLnByb3BzLmFyaWFMYWJlbCA/IHsgXCJhcmlhLWxhYmVsXCI6IHByb3BzLmFyaWFMYWJlbCB9IDoge319XG4gICAgICAgICAgICAgICAgey4uLnByb3BzLmhhc0Vycm9yIHx8IHN0YXRlLmhhc0Vycm9yID8geyBcImFyaWEtaW52YWxpZFwiOiB0cnVlIH0gOiB7fX1cbiAgICAgICAgICAgICAgICB7Li4ucHJvcHMuYXJpYVJlcXVpcmVkID8geyBcImFyaWEtcmVxdWlyZWRcIjogdHJ1ZSB9IDoge319XG4gICAgICAgICAgICAgICAgey4uLnByb3BzLnJlcXVpcmVkID8geyByZXF1aXJlZDogdHJ1ZSB9IDoge319XG4gICAgICAgICAgICAgICAgYXV0b0NvbXBsZXRlPXtwcm9wcy5hdXRvQ29tcGxldGUucmVwbGFjZUFsbChcIl9cIiwgXCItXCIpfVxuICAgICAgICAgICAgLz4gOlxuICAgICAgICAgICAgPElucHV0TWFza0VsZW1lbnRcbiAgICAgICAgICAgICAgICByZWY9e3Byb3BzLmlucHV0UmVmfVxuICAgICAgICAgICAgICAgIGtleT17dmFsdWV9XG4gICAgICAgICAgICAgICAgaWQ9e3Byb3BzLmlkfVxuICAgICAgICAgICAgICAgIHN0eWxlPXtpbnB1dFN0eWxlfVxuICAgICAgICAgICAgICAgIGNsYXNzTmFtZT17Z2V0SW5wdXRDbGFzc05hbWVzKCl9XG4gICAgICAgICAgICAgICAgdGFiSW5kZXg9e3RhYkluZGV4fVxuICAgICAgICAgICAgICAgIHNob3dBc1Bhc3Nvd3JkPXtwcm9wcy5pbnB1dFR5cGV9XG4gICAgICAgICAgICAgICAgbWFzaz17dXNlcklucHV0TWFza31cbiAgICAgICAgICAgICAgICBtYXhMZW5ndGg9e21heExlbmd0aElucHV0ID09PSAwID8gdW5kZWZpbmVkIDogbWF4TGVuZ3RoSW5wdXR9XG4gICAgICAgICAgICAgICAgdmFsdWU9e2dldEN1cnJlbnREaXNwbGF5VmFsdWUoKS50b1N0cmluZygpfVxuICAgICAgICAgICAgICAgIHBsYWNlSG9sZGVyQ2hhcj1cIl9cIlxuICAgICAgICAgICAgICAgIHBsYWNlSG9sZGVySW5wdXQ9e3BsYWNlSG9sZGVySW5wdXR9XG4gICAgICAgICAgICAgICAgb25DaGFuZ2VIYW5kbGVyPXtvbkNoYW5nZUhhbmRsZX1cbiAgICAgICAgICAgICAgICBvbkVudGVyS2V5UHJlc3M9eyhldmVudCkgPT4ge29uRW50ZXJLZXlQcmVzc0hhbmRsZXIoZXZlbnQpfX1cbiAgICAgICAgICAgICAgICBvbkZvY3VzPXtvbkVudGVySGFuZGxlfVxuICAgICAgICAgICAgICAgIG9uQmx1cj17b25CbHVyfVxuICAgICAgICAgICAgICAgIGRpc2FibGVkPXtkaXNhYmxlZH1cbiAgICAgICAgICAgICAgICBhcmlhTGFiZWw9e3Byb3BzLmFyaWFMYWJlbH1cbiAgICAgICAgICAgICAgICBhcmlhSW52YWxpZD17cHJvcHMuaGFzRXJyb3IgfHwgc3RhdGUuaGFzRXJyb3J9XG4gICAgICAgICAgICAgICAgYXJpYVJlcXVpcmVkPXthcmlhUmVxdWlyZWR9XG4gICAgICAgICAgICAgICAgcmVxdWlyZWQ9e3Byb3BzLnJlcXVpcmVkfVxuICAgICAgICAgICAgICAgIGF1dG9Db21wbGV0ZT17cHJvcHMuYXV0b0NvbXBsZXRlLnJlcGxhY2VBbGwoXCJfXCIsIFwiLVwiKX1cbiAgICAgICAgICAgID48L0lucHV0TWFza0VsZW1lbnQ+XG4gICAgKTtcbn1cblxuZXhwb3J0IGRlZmF1bHQgSW5wdXRFbGVtZW50O1xuIiwiZXhwb3J0IGRlZmF1bHQgXCJ3aWRnZXRzL21lbmRpeC9vcGNlbnRlcmNvcmV0ZXh0Ym94L2Fzc2V0cy84Mjk2MjRlYWM2ZjI4MzVlLnN2Z1wiIiwiaW1wb3J0IHsgRnVuY3Rpb25Db21wb25lbnQsIGNyZWF0ZUVsZW1lbnQsIENTU1Byb3BlcnRpZXMgfSBmcm9tIFwicmVhY3RcIjtcbmltcG9ydCBjbWRRUkNvZGUgZnJvbSBcIi4uL2ltYWdlcy9jbWRRUkNvZGUuc3ZnXCI7XG5cblxuZXhwb3J0IGludGVyZmFjZSBCYXJjb2RlSW1hZ2VQcm9wcyB7XG4gICAgc3R5bGU/OiBDU1NQcm9wZXJ0aWVzXG4gICAgb25DbGlja0hhbmRsZXI/OiAoaXNRUkltYWdlQ2xpY2tlZDogYm9vbGVhbikgPT4gdm9pZDtcbn1cbmV4cG9ydCBjb25zdCBCYXJjb2RlSW1hZ2U6IEZ1bmN0aW9uQ29tcG9uZW50PEJhcmNvZGVJbWFnZVByb3BzPiA9ICh7IHN0eWxlLCBvbkNsaWNrSGFuZGxlciB9KSA9PlxuXG4gICAgPGltZ1xuICAgICAgICBzdHlsZT17c3R5bGV9XG4gICAgICAgIGNsYXNzTmFtZT1cIm9wY29yZS1xci1pbWFnZVwiXG4gICAgICAgIHJvbGU9e1wiYnV0dG9uXCJ9XG4gICAgICAgIHNyYz17Y21kUVJDb2RlfVxuICAgICAgICBvbk1vdXNlRG93bj17KCkgPT4gb25DbGlja0hhbmRsZXIgIT09IHVuZGVmaW5lZCA/IG9uQ2xpY2tIYW5kbGVyKHRydWUpIDogZmFsc2V9XG4gICAgPjwvaW1nPlxuXG4iLCJpbXBvcnQgeyBSZWFjdEVsZW1lbnQsIGNyZWF0ZUVsZW1lbnQgfSBmcm9tIFwicmVhY3RcIjtcbmltcG9ydCBjbGFzc05hbWVzIGZyb20gXCJjbGFzc25hbWVzXCI7XG5cbmV4cG9ydCBpbnRlcmZhY2UgQWxlcnRQcm9wcyB7XG4gICAgaWQ/OiBzdHJpbmc7XG4gICAgbWVzc2FnZT86IHN0cmluZztcbiAgICBjbGFzc05hbWU/OiBzdHJpbmc7XG4gICAgYm9vdHN0cmFwU3R5bGU6IFwiZGVmYXVsdFwiIHwgXCJwcmltYXJ5XCIgfCBcInN1Y2Nlc3NcIiB8IFwiaW5mb1wiIHwgXCJpbnZlcnNlXCIgfCBcIndhcm5pbmdcIiB8IFwiZGFuZ2VyXCI7XG59XG5cbmV4cG9ydCBmdW5jdGlvbiBBbGVydCh7IGlkLCBtZXNzYWdlLCBjbGFzc05hbWUsIGJvb3RzdHJhcFN0eWxlIH06IEFsZXJ0UHJvcHMpOiBSZWFjdEVsZW1lbnQgfCBudWxsIHtcbiAgICByZXR1cm4gbWVzc2FnZSA/IDxkaXYgaWQ9e2Ake2lkfS1lcnJvcmB9IHJvbGU9XCJhbGVydFwiIGNsYXNzTmFtZT17Y2xhc3NOYW1lcyhgYWxlcnQgYWxlcnQtJHtib290c3RyYXBTdHlsZX1gLCBjbGFzc05hbWUpfT57bWVzc2FnZX08L2Rpdj4gOiBudWxsO1xufVxuIiwiaW1wb3J0IHsgUmVhY3RFbGVtZW50LCBjcmVhdGVFbGVtZW50LCB1c2VFZmZlY3QsIHVzZVN0YXRlLCB1c2VSZWYgfSBmcm9tIFwicmVhY3RcIjtcbmltcG9ydCB7IE9wY2VudGVyQ29yZVRleHRCb3hDb250YWluZXJQcm9wcyB9IGZyb20gXCIuLi90eXBpbmdzL09wY2VudGVyQ29yZVRleHRCb3hQcm9wc1wiO1xuaW1wb3J0IHsgSW5wdXRFbGVtZW50IH0gZnJvbSBcIi4vY29tcG9uZW50cy9JbnB1dEVsZW1lbnRcIjtcbmltcG9ydCB7IEJhcmNvZGVJbWFnZSB9IGZyb20gXCIuL2NvbXBvbmVudHMvQmFyY29kZUltYWdlXCI7XG5pbXBvcnQgXCIuL3VpL09wY2VudGVyQ29yZVRleHRCb3guY3NzXCI7XG5pbXBvcnQgeyBBbGVydCB9IGZyb20gXCIuL2NvbXBvbmVudHMvQWxlcnRcIjtcblxuaW50ZXJmYWNlIE9wY2VudGVyQ29yZVRleHRCb3hFeHRlbmRlZFByb3BzIHtcbiAgICBzaG93SW5wdXRRUkltYWdlPzogYm9vbGVhbjtcbiAgICBpc0lucHV0UVJJbWFnZUNsaWNrPzogYm9vbGVhbjtcbiAgICBpc1ZhbHVlQ2hhbmdlQnlTY2FuPzogYm9vbGVhbjtcbn1cblxudmFyIGlzRm9jdXNGcm9tUVJJbWFnZTogYm9vbGVhbiA9IGZhbHNlO1xuXG5leHBvcnQgZnVuY3Rpb24gT3BjZW50ZXJDb3JlVGV4dEJveChwcm9wczogT3BjZW50ZXJDb3JlVGV4dEJveENvbnRhaW5lclByb3BzKTogUmVhY3RFbGVtZW50IHtcbiAgICBjb25zdCB7IHZhbHVlQXR0cmlidXRlLCBzaG93QXNQYXNzb3dyZCwgcGxhY2Vob2xkZXIsIHNob3dRckltYWdlVXNlcklucHV0LCBpbnB1dE1hc2ssIHJlYWRPbmx5U3R5bGUsXG4gICAgICAgIG9uQ2xpY2tBY3Rpb24sIG9uQmFyY29kZVNjYW5BY3Rpb24sIHN1Ym1pdERlbGF5LFxuICAgICAgICBvbkVudGVyQWN0aW9uLCB0YWJJbmRleCwgb25FbnRlcktleUFjdGlvbiwgb25MZWF2ZUFjdGlvbiB9ID0gcHJvcHM7XG5cbiAgICBjb25zdCB2YWxpZGF0aW9uRmVlZGJhY2sgPSBwcm9wcy52YWx1ZUF0dHJpYnV0ZT8udmFsaWRhdGlvbjtcbiAgICBjb25zdCByZXF1aXJlZCA9IHByb3BzLnZhbGlkYXRpb25Qcm9wZXJ0eSA9PT0gXCJyZXF1aXJlZFwiIHx8IHByb3BzLnZhbGlkYXRpb25Qcm9wZXJ0eUZvckRpZ2l0ID09PSBcInJlcXVpcmVkXCI7XG4gICAgY29uc3Qgc3VibWl0RGVsYXlWYWx1ZSA9IHN1Ym1pdERlbGF5ID8gc3VibWl0RGVsYXkgPj0gMCA/IHN1Ym1pdERlbGF5IDogMCA6IDA7XG4gICAgY29uc3QgaW5wdXRSZWYgPSB1c2VSZWY8SFRNTElucHV0RWxlbWVudD4oKTtcblxuICAgIGNvbnN0IFtzdGF0ZSwgc2V0U3RhdGVdID1cbiAgICAgICAgdXNlU3RhdGU8T3BjZW50ZXJDb3JlVGV4dEJveEV4dGVuZGVkUHJvcHM+KHtcbiAgICAgICAgICAgIHNob3dJbnB1dFFSSW1hZ2U6IGZhbHNlLFxuICAgICAgICAgICAgaXNJbnB1dFFSSW1hZ2VDbGljazogZmFsc2UsIGlzVmFsdWVDaGFuZ2VCeVNjYW46IGZhbHNlXG4gICAgICAgIH0pO1xuXG4gICAgLy8gdXNlRWZmZWN0KCgpID0+IHtcbiAgICAvLyAgICAgY2hlY2tWYWxpZGF0b3JzKCk7XG4gICAgLy8gfSwgW10pO1xuXG4gICAgdXNlRWZmZWN0KCgpID0+IHtcbiAgICAgICAgQ2hlY2tJZkF0dHJpYnV0ZVZhbHVlQ2hhbmdlZEJ5U2NhbigpO1xuICAgICAgICBjaGVja1ZhbGlkYXRvcnMoKTtcbiAgICB9LCBbdmFsdWVBdHRyaWJ1dGU/LnZhbHVlXSk7XG5cbiAgICBjb25zdCBjaGVja1ZhbGlkYXRvcnMgPSAoKSA9PiB7XG4gICAgICAgIGlmIChwcm9wcy52YWxpZGF0aW9uUHJvcGVydHkgPT09IFwicmVxdWlyZWRcIiB8fCBwcm9wcy52YWxpZGF0aW9uUHJvcGVydHlGb3JEaWdpdCA9PT0gXCJyZXF1aXJlZFwiKSB7XG4gICAgICAgICAgICBwcm9wcy52YWx1ZUF0dHJpYnV0ZT8uc2V0VmFsaWRhdG9yKHJlcXVpcmVkdmFsaWRhdG9yKTtcbiAgICAgICAgfVxuICAgICAgICBlbHNlIGlmIChwcm9wcy52YWxpZGF0aW9uUHJvcGVydHkgPT09IFwiZW1haWxcIikge1xuICAgICAgICAgICAgcHJvcHMudmFsdWVBdHRyaWJ1dGU/LnNldFZhbGlkYXRvcihlbWFpbHZhbGlkYXRvcik7XG4gICAgICAgIH1cbiAgICAgICAgZWxzZSBpZiAocHJvcHMudmFsaWRhdGlvblByb3BlcnR5ID09PSBcImN1c3RvbVwiIHx8IHByb3BzLnZhbGlkYXRpb25Qcm9wZXJ0eUZvckRpZ2l0ID09PSBcImN1c3RvbVwiKSB7XG4gICAgICAgICAgICBwcm9wcy52YWx1ZUF0dHJpYnV0ZT8uc2V0VmFsaWRhdG9yKGN1c3RvbVZhbGlkYXRvcik7XG4gICAgICAgIH1cbiAgICAgICAgZWxzZSBpZiAocHJvcHMudmFsaWRhdGlvblByb3BlcnR5Rm9yRGlnaXQgPT09IFwicG9zaXRpdmVOdW1iZXJcIikge1xuICAgICAgICAgICAgcHJvcHMudmFsdWVBdHRyaWJ1dGU/LnNldFZhbGlkYXRvcihwb3NpdGl2ZU51bWJlclZhbGlkYXRvcik7XG4gICAgICAgIH1cbiAgICAgICAgZWxzZVxuICAgICAgICAgICAgcHJvcHMudmFsdWVBdHRyaWJ1dGU/LnNldFZhbGlkYXRvcih1bmRlZmluZWQpO1xuICAgIH1cblxuICAgIGZ1bmN0aW9uIENoZWNrSWZBdHRyaWJ1dGVWYWx1ZUNoYW5nZWRCeVNjYW4oKTogdm9pZCB7XG4gICAgICAgIGlmIChzdGF0ZS5pc0lucHV0UVJJbWFnZUNsaWNrKSB7XG4gICAgICAgICAgICBpZiAob25DbGlja0FjdGlvbikge1xuICAgICAgICAgICAgICAgIHZhciBhdHRyaWJ1dGVWYWx1ZSA9IHZhbHVlQXR0cmlidXRlPy52YWx1ZSAhPT0gdW5kZWZpbmVkID8gdmFsdWVBdHRyaWJ1dGUuZGlzcGxheVZhbHVlIDogJyc7XG4gICAgICAgICAgICAgICAgaWYgKGF0dHJpYnV0ZVZhbHVlICE9PSAnJykge1xuICAgICAgICAgICAgICAgICAgICBzZXRUaW1lb3V0KCgpID0+IHtcbiAgICAgICAgICAgICAgICAgICAgICAgIG9uQmFyY29kZVNjYW5BY3Rpb25FeGVjdXRlKCk7XG4gICAgICAgICAgICAgICAgICAgIH0sIDUwMCk7XG4gICAgICAgICAgICAgICAgICAgIHNldFRpbWVvdXQoKCkgPT4ge1xuICAgICAgICAgICAgICAgICAgICAgICAgZm9jdXNOZXh0RWxlbWVudCgpO1xuICAgICAgICAgICAgICAgICAgICB9LCAxMDAwKTtcbiAgICAgICAgICAgICAgICAgICAgLy9mb2N1c05leHRFbGVtZW50KDE1MDApO1xuICAgICAgICAgICAgICAgICAgICBpZiAob25MZWF2ZUFjdGlvbiAmJiBvbkxlYXZlQWN0aW9uLmNhbkV4ZWN1dGUpIHtcbiAgICAgICAgICAgICAgICAgICAgICAgIG9uTGVhdmVBY3Rpb24uZXhlY3V0ZSgpO1xuICAgICAgICAgICAgICAgICAgICB9XG4gICAgICAgICAgICAgICAgfVxuICAgICAgICAgICAgfVxuICAgICAgICAgICAgc2V0U3RhdGUoKHByZXZTdGF0ZSkgPT4gKHsgLi4ucHJldlN0YXRlLCBpc0lucHV0UVJJbWFnZUNsaWNrOiBmYWxzZSwgaXNWYWx1ZUNoYW5nZUJ5U2NhbjogdHJ1ZSB9KSk7XG4gICAgICAgIH1cbiAgICAgICAgZWxzZSB7XG4gICAgICAgICAgICBzZXRTdGF0ZSgocHJldlN0YXRlKSA9PiAoeyAuLi5wcmV2U3RhdGUsIGlzVmFsdWVDaGFuZ2VCeVNjYW46IGZhbHNlIH0pKTtcbiAgICAgICAgfVxuICAgIH1cblxuICAgIGZ1bmN0aW9uIG9uQmFyY29kZVNjYW5BY3Rpb25FeGVjdXRlKCk6IHZvaWQge1xuICAgICAgICBpZiAob25CYXJjb2RlU2NhbkFjdGlvbiAmJiBvbkJhcmNvZGVTY2FuQWN0aW9uLmNhbkV4ZWN1dGUpIHtcbiAgICAgICAgICAgIG9uQmFyY29kZVNjYW5BY3Rpb24uZXhlY3V0ZSgpO1xuICAgICAgICB9XG4gICAgfVxuXG4gICAgY29uc3Qgb25DbGlja0hhbmRsZXIgPSAoaXNRUkltYWdlQ2xpY2tlZDogYm9vbGVhbikgPT4ge1xuICAgICAgICBpZiAob25DbGlja0FjdGlvbiAmJiBvbkNsaWNrQWN0aW9uLmNhbkV4ZWN1dGUpIHtcbiAgICAgICAgICAgIG9uQ2xpY2tBY3Rpb24uZXhlY3V0ZSgpO1xuICAgICAgICB9XG4gICAgICAgIHNldFN0YXRlKChwcmV2U3RhdGUpID0+ICh7IC4uLnByZXZTdGF0ZSwgaXNJbnB1dFFSSW1hZ2VDbGljazogaXNRUkltYWdlQ2xpY2tlZCB9KSk7XG4gICAgfVxuXG4gICAgY29uc3Qgb25FbnRlckhhbmRsZXIgPSAoc2hvd1FSSW1hZ2U6IGJvb2xlYW4pID0+IHtcbiAgICAgICAgaWYgKCFpc0ZvY3VzRnJvbVFSSW1hZ2UgJiYgb25FbnRlckFjdGlvbiAmJiBvbkVudGVyQWN0aW9uLmNhbkV4ZWN1dGUpIHtcbiAgICAgICAgICAgIG9uRW50ZXJBY3Rpb24uZXhlY3V0ZSgpO1xuICAgICAgICB9XG4gICAgICAgIGlzRm9jdXNGcm9tUVJJbWFnZSA9IGZhbHNlO1xuICAgICAgICBzZXRTdGF0ZSgocHJldlN0YXRlKSA9PiAoe1xuICAgICAgICAgICAgLi4ucHJldlN0YXRlLFxuICAgICAgICAgICAgc2hvd0lucHV0UVJJbWFnZTogc2hvd1FSSW1hZ2UsIGlzSW5wdXRRUkltYWdlQ2xpY2s6IGZhbHNlXG4gICAgICAgIH0pKTtcbiAgICB9XG5cbiAgICBjb25zdCBvbkVudGVyS2V5UHJlc3NIYW5kbGVyID0gICh2YWx1ZTogc3RyaW5nIHwgQmlnLCBpc0NoYW5nZWQ6IGJvb2xlYW4sIGlzVmFsdWVJbnZhbGlkOiBib29sZWFuID0gZmFsc2UpID0+IHtcbiAgICAgICAgaWYgKCFpc1ZhbHVlSW52YWxpZCkge1xuICAgICAgICAgICAgICAgIFxuICAgICAgICAgICAgaWYgKGlzQ2hhbmdlZCkge1xuICAgICAgICAgICAgICAgICAgICBhcHBseUNoYW5nZSh2YWx1ZSk7XG4gICAgICAgICAgICB9XG4gICAgICAgIH1cbiAgICAgICAgaWYgKG9uRW50ZXJLZXlBY3Rpb24gJiYgb25FbnRlcktleUFjdGlvbi5jYW5FeGVjdXRlKSB7XG4gICAgICAgICAgICBvbkVudGVyS2V5QWN0aW9uLmV4ZWN1dGUoKTtcbiAgICAgICAgfVxuICAgIH1cblxuICAgIGNvbnN0IG9uTGVhdmVIYW5kbGVyID0gKHZhbHVlOiBzdHJpbmcgfCBCaWcsIGlzQ2hhbmdlZDogYm9vbGVhbiwgc2hvd1FSSW1hZ2U6IGJvb2xlYW4sIGlzVmFsdWVJbnZhbGlkOiBib29sZWFuID0gZmFsc2UpID0+IHtcbiAgICAgICAgaWYgKCFpc1ZhbHVlSW52YWxpZCkge1xuICAgICAgICAgICAgaWYgKCFzdGF0ZS5pc0lucHV0UVJJbWFnZUNsaWNrICYmIG9uTGVhdmVBY3Rpb24gJiYgb25MZWF2ZUFjdGlvbi5jYW5FeGVjdXRlKSB7XG4gICAgICAgICAgICAgICAgb25MZWF2ZUFjdGlvbi5leGVjdXRlKCk7XG4gICAgICAgICAgICB9XG5cbiAgICAgICAgICAgIGlmIChpc0NoYW5nZWQpIHtcbiAgICAgICAgICAgICAgICBpZiAoKCFzdGF0ZS5pc0lucHV0UVJJbWFnZUNsaWNrKSB8fCAoc3RhdGUuaXNJbnB1dFFSSW1hZ2VDbGljayAmJiAhb25DbGlja0FjdGlvbikpXG4gICAgICAgICAgICAgICAgICAgIGFwcGx5Q2hhbmdlKHZhbHVlKTtcbiAgICAgICAgICAgIH1cblxuICAgICAgICAgICAgaWYgKHN0YXRlLmlzSW5wdXRRUkltYWdlQ2xpY2sgJiYgIW9uQ2xpY2tBY3Rpb24pIHtcbiAgICAgICAgICAgICAgICBmb2N1c0N1cnJlbnRFbGVtZW50KCk7XG4gICAgICAgICAgICB9XG4gICAgICAgICAgICBlbHNlIHtcbiAgICAgICAgICAgICAgICBzZXRTdGF0ZSgocHJldlN0YXRlKSA9PiAoeyAuLi5wcmV2U3RhdGUsIHNob3dJbnB1dFFSSW1hZ2U6IHNob3dRUkltYWdlIH0pKTtcbiAgICAgICAgICAgIH1cbiAgICAgICAgfVxuICAgICAgICBlbHNlXG4gICAgICAgICAgICBzZXRTdGF0ZSgocHJldlN0YXRlKSA9PiAoeyAuLi5wcmV2U3RhdGUsIHNob3dJbnB1dFFSSW1hZ2U6IHNob3dRUkltYWdlIH0pKTtcblxuICAgIH07XG5cbiAgICBjb25zdCBhcHBseUNoYW5nZSA9ICh2YWx1ZTogc3RyaW5nIHwgQmlnKSA9PiB7XG5cbiAgICAgICAgaWYgKCFwcm9wcy52YWx1ZUF0dHJpYnV0ZT8ucmVhZE9ubHkgJiYgcHJvcHMudmFsdWVBdHRyaWJ1dGU/LnN0YXR1cyA9PT0gXCJhdmFpbGFibGVcIikge1xuICAgICAgICAgICAgaWYgKHZhbHVlID09PSAnJyAmJiAocHJvcHMuc2VsZWN0ZWRBdHRyaWJ1dGVUeXBlID09PSBcImludGVnZXJcIlxuICAgICAgICAgICAgICAgIHx8IHByb3BzLnNlbGVjdGVkQXR0cmlidXRlVHlwZSA9PT0gXCJkZWNpbWFsXCIgfHwgcHJvcHMuc2VsZWN0ZWRBdHRyaWJ1dGVUeXBlID09PSBcImxvbmdcIikpIHtcbiAgICAgICAgICAgICAgICBwcm9wcy52YWx1ZUF0dHJpYnV0ZS5zZXRWYWx1ZSh1bmRlZmluZWQpO1xuICAgICAgICAgICAgfSBlbHNlXG4gICAgICAgICAgICAgICAgcHJvcHMudmFsdWVBdHRyaWJ1dGUuc2V0VmFsdWUodmFsdWUpO1xuICAgICAgICB9XG4gICAgfVxuXG4gICAgY29uc3QgYXBwbHlDaGFuZ2VXaGlsZUVkaXRpbmcgPSAodmFsdWU6IHN0cmluZyB8IEJpZykgPT4ge1xuICAgICAgICBzZXRUaW1lb3V0KFxuICAgICAgICAgICAgKCkgPT4gYXBwbHlDaGFuZ2UodmFsdWUpLFxuICAgICAgICAgICAgc3VibWl0RGVsYXlWYWx1ZVxuICAgICAgICApO1xuICAgIH1cblxuICAgIGNvbnN0IHJlcXVpcmVkdmFsaWRhdG9yID0gKHZhbHVlOiBzdHJpbmcgfCB1bmRlZmluZWQpOiBzdHJpbmcgfCB1bmRlZmluZWQgPT4ge1xuICAgICAgICBjb25zdCB7IHZhbGlkYXRpb25NZXNzYWdlIH0gPSBwcm9wcztcblxuICAgICAgICBpZiAodmFsaWRhdGlvbk1lc3NhZ2UgJiYgIXZhbHVlKSB7XG4gICAgICAgICAgICByZXR1cm4gdmFsaWRhdGlvbk1lc3NhZ2U7XG4gICAgICAgIH1cbiAgICB9XG5cbiAgICBjb25zdCBlbWFpbHZhbGlkYXRvciA9ICh2YWx1ZTogc3RyaW5nIHwgdW5kZWZpbmVkKTogc3RyaW5nIHwgdW5kZWZpbmVkID0+IHtcbiAgICAgICAgY29uc3QgeyB2YWxpZGF0aW9uTWVzc2FnZSB9ID0gcHJvcHM7XG5cbiAgICAgICAgaWYgKHZhbHVlICE9PSB1bmRlZmluZWQgJiYgdmFsdWUgIT09ICcnKSB7XG4gICAgICAgICAgICBsZXQgbWF0Y2hSZXN1bHQgPSB2YWx1ZS5tYXRjaCgvW0EtWmEtejAtOSEjJCUmJycqKy89P15fYHt8fX4tXSsoPzpcXC5bQS1aYS16MC05ISMkJSYnJyorLz0/Xl9ge3x9fi1dKykqQCg/OltBLVphLXowLTldKD86W0EtWmEtejAtOS1dKltBLVphLXowLTldKT9cXC4pK1tBLVphLXowLTldKD86W0EtWmEtejAtOS1dKltBLVphLXowLTldKT8vKTtcblxuICAgICAgICAgICAgaWYgKG1hdGNoUmVzdWx0ID09PSBudWxsKSB7XG4gICAgICAgICAgICAgICAgaWYgKHZhbGlkYXRpb25NZXNzYWdlKSB7XG4gICAgICAgICAgICAgICAgICAgIHJldHVybiB2YWxpZGF0aW9uTWVzc2FnZTtcbiAgICAgICAgICAgICAgICB9XG4gICAgICAgICAgICB9XG4gICAgICAgIH1cbiAgICB9XG5cbiAgICBjb25zdCBwb3NpdGl2ZU51bWJlclZhbGlkYXRvciA9ICh2YWx1ZTogc3RyaW5nIHwgdW5kZWZpbmVkKTogc3RyaW5nIHwgdW5kZWZpbmVkID0+IHtcbiAgICAgICAgY29uc3QgeyB2YWxpZGF0aW9uTWVzc2FnZSB9ID0gcHJvcHM7XG5cbiAgICAgICAgaWYgKHZhbHVlICE9PSB1bmRlZmluZWQgJiYgdmFsdWUgIT09ICcnKSB7XG4gICAgICAgICAgICBsZXQgbWF0Y2hSZXN1bHQgPSB2YWx1ZS50b1N0cmluZygpLm1hdGNoKC9eKD8hMChcXC4wKyk/JClcXGQqXFwuP1xcZCskLyk7XG5cbiAgICAgICAgICAgIGlmIChtYXRjaFJlc3VsdCA9PT0gbnVsbCkge1xuICAgICAgICAgICAgICAgIGlmICh2YWxpZGF0aW9uTWVzc2FnZSkge1xuICAgICAgICAgICAgICAgICAgICByZXR1cm4gdmFsaWRhdGlvbk1lc3NhZ2U7XG4gICAgICAgICAgICAgICAgfVxuICAgICAgICAgICAgfVxuICAgICAgICB9XG4gICAgfVxuXG4gICAgY29uc3QgY3VzdG9tVmFsaWRhdG9yID0gKHZhbHVlOiBzdHJpbmcgfCB1bmRlZmluZWQpOiBzdHJpbmcgfCB1bmRlZmluZWQgPT4ge1xuICAgICAgICBjb25zdCB7IHZhbGlkYXRpb25NZXNzYWdlLCB2YWxpZGF0aW9uRXhwcmVzc2lvbiB9ID0gcHJvcHM7XG5cbiAgICAgICAgaWYgKHZhbGlkYXRpb25FeHByZXNzaW9uPy52YWx1ZSAmJiB2YWx1ZSAhPT0gdW5kZWZpbmVkICYmIHZhbHVlICE9PSAnJykge1xuICAgICAgICAgICAgbGV0IG1hdGNoUmVzdWx0ID0gdmFsdWUudG9TdHJpbmcoKS5tYXRjaCh2YWxpZGF0aW9uRXhwcmVzc2lvbi52YWx1ZSk7XG5cbiAgICAgICAgICAgIGlmIChtYXRjaFJlc3VsdCA9PT0gbnVsbCkge1xuICAgICAgICAgICAgICAgIGlmICh2YWxpZGF0aW9uTWVzc2FnZSkge1xuICAgICAgICAgICAgICAgICAgICByZXR1cm4gdmFsaWRhdGlvbk1lc3NhZ2U7XG4gICAgICAgICAgICAgICAgfVxuICAgICAgICAgICAgfVxuICAgICAgICB9XG4gICAgfVxuXG4gICAgY29uc3QgZm9jdXNDdXJyZW50RWxlbWVudCA9ICgpID0+IHtcbiAgICAgICAgaWYgKGlucHV0UmVmLmN1cnJlbnQpIHtcbiAgICAgICAgICAgIGlucHV0UmVmLmN1cnJlbnQuZm9jdXMoKTtcbiAgICAgICAgfVxuICAgIH1cblxuICAgIGNvbnN0IGZvY3VzTmV4dEVsZW1lbnQgPSAodGltZW91dDogbnVtYmVyID0gMCkgPT4ge1xuXG4gICAgICAgIC8vIGlmIChkb2N1bWVudC5hY3RpdmVFbGVtZW50IGluc3RhbmNlb2YgSFRNTEVsZW1lbnQpIHtcbiAgICAgICAgLy8gICAgIGRvY3VtZW50LmFjdGl2ZUVsZW1lbnQuYmx1cigpO1xuICAgICAgICAvLyB9XG5cbiAgICAgICAgaWYgKGlucHV0UmVmLmN1cnJlbnQpIHtcbiAgICAgICAgICAgIGxldCBjdXJyZW50RWxlbWVudCA9IGlucHV0UmVmLmN1cnJlbnQgYXMgSFRNTEVsZW1lbnQgfCBudWxsO1xuXG4gICAgICAgICAgICAvLyBUcmF2ZXJzZSB1cCB0byB0aGUgcGFyZW50IGVsZW1lbnRcbiAgICAgICAgICAgIHdoaWxlIChjdXJyZW50RWxlbWVudCkge1xuICAgICAgICAgICAgICAgIGxldCBuZXh0RWxlbWVudCA9IGN1cnJlbnRFbGVtZW50Lm5leHRFbGVtZW50U2libGluZyBhcyBIVE1MRWxlbWVudCB8IG51bGw7XG5cbiAgICAgICAgICAgICAgICAvLyBUcmF2ZXJzZSBkb3duIHRvIGZpbmQgdGhlIG5leHQgZm9jdXNhYmxlIGVsZW1lbnRcbiAgICAgICAgICAgICAgICB3aGlsZSAobmV4dEVsZW1lbnQpIHtcbiAgICAgICAgICAgICAgICAgICAgY29uc3QgZm9jdXNhYmxlRWxlbWVudCA9IGZpbmRGb2N1c2FibGVFbGVtZW50KG5leHRFbGVtZW50KTtcbiAgICAgICAgICAgICAgICAgICAgaWYgKGZvY3VzYWJsZUVsZW1lbnQpIHtcbiAgICAgICAgICAgICAgICAgICAgICAgIC8vZm9jdXNhYmxlRWxlbWVudC5mb2N1cygpO1xuICAgICAgICAgICAgICAgICAgICAgICAgc2V0VGltZW91dCgoKSA9PiB7XG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgZm9jdXNhYmxlRWxlbWVudC5mb2N1cygpO1xuICAgICAgICAgICAgICAgICAgICAgICAgfSwgdGltZW91dCk7XG4gICAgICAgICAgICAgICAgICAgICAgICByZXR1cm47XG4gICAgICAgICAgICAgICAgICAgIH1cbiAgICAgICAgICAgICAgICAgICAgbmV4dEVsZW1lbnQgPSBuZXh0RWxlbWVudC5uZXh0RWxlbWVudFNpYmxpbmcgYXMgSFRNTEVsZW1lbnQgfCBudWxsO1xuICAgICAgICAgICAgICAgIH1cblxuICAgICAgICAgICAgICAgIGN1cnJlbnRFbGVtZW50ID0gY3VycmVudEVsZW1lbnQucGFyZW50RWxlbWVudCBhcyBIVE1MRWxlbWVudCB8IG51bGw7XG4gICAgICAgICAgICB9XG4gICAgICAgIH1cbiAgICB9O1xuXG4gICAgY29uc3QgZmluZEZvY3VzYWJsZUVsZW1lbnQgPSAoZWxlbWVudDogSFRNTEVsZW1lbnQpOiBIVE1MRWxlbWVudCB8IG51bGwgPT4ge1xuICAgICAgICBpZiAoaXNGb2N1c2FibGUoZWxlbWVudCkpIHtcbiAgICAgICAgICAgIHJldHVybiBlbGVtZW50O1xuICAgICAgICB9XG5cbiAgICAgICAgY29uc3QgY2hpbGRyZW4gPSBlbGVtZW50LmNoaWxkcmVuO1xuICAgICAgICBmb3IgKGxldCBpID0gMDsgaSA8IGNoaWxkcmVuLmxlbmd0aDsgaSsrKSB7XG4gICAgICAgICAgICBjb25zdCBmb2N1c2FibGVDaGlsZCA9IGZpbmRGb2N1c2FibGVFbGVtZW50KGNoaWxkcmVuW2ldIGFzIEhUTUxFbGVtZW50KTtcbiAgICAgICAgICAgIGlmIChmb2N1c2FibGVDaGlsZCkge1xuICAgICAgICAgICAgICAgIHJldHVybiBmb2N1c2FibGVDaGlsZDtcbiAgICAgICAgICAgIH1cbiAgICAgICAgfVxuXG4gICAgICAgIHJldHVybiBudWxsO1xuICAgIH07XG5cbiAgICBjb25zdCBpc0ZvY3VzYWJsZSA9IChlbGVtZW50OiBIVE1MRWxlbWVudCk6IGJvb2xlYW4gPT4ge1xuICAgICAgICBjb25zdCBmb2N1c2FibGVFbGVtZW50cyA9IFsnSU5QVVQnLCAnQlVUVE9OJywgJ1NFTEVDVCcsICdURVhUQVJFQScsICdBJ107XG4gICAgICAgIHJldHVybiAoXG4gICAgICAgICAgICBmb2N1c2FibGVFbGVtZW50cy5pbmNsdWRlcyhlbGVtZW50LnRhZ05hbWUpIHx8XG4gICAgICAgICAgICBlbGVtZW50LnRhYkluZGV4ID49IDBcbiAgICAgICAgKTtcbiAgICB9O1xuXG4gICAgcmV0dXJuIChcbiAgICAgICAgKCFwcm9wcy52YWx1ZUF0dHJpYnV0ZT8ucmVhZE9ubHkgJiYgcmVhZE9ubHlTdHlsZSA9PT0gXCJ0ZXh0XCIpIHx8IChyZWFkT25seVN0eWxlID09PSBcImRhdGFWaWV3XCIgfHwgcmVhZE9ubHlTdHlsZSA9PT0gXCJjb250cm9sXCIpID9cbiAgICAgICAgICAgIDxkaXY+XG4gICAgICAgICAgICAgICAgPGRpdlxuICAgICAgICAgICAgICAgICAgICBjbGFzc05hbWU9XCJ3aWRnZXQtb3Bjb3JldGV4dGJveC1ob2xkZXJcIj5cbiAgICAgICAgICAgICAgICAgICAgPElucHV0RWxlbWVudFxuICAgICAgICAgICAgICAgICAgICAgICAgaW5wdXRSZWY9e2lucHV0UmVmfVxuICAgICAgICAgICAgICAgICAgICAgICAgaWQ9e3Byb3BzLmlkfVxuICAgICAgICAgICAgICAgICAgICAgICAgaW5wdXRUeXBlPXtzaG93QXNQYXNzb3dyZH1cbiAgICAgICAgICAgICAgICAgICAgICAgIHBsYWNlSG9sZGVySW5wdXQ9e3BsYWNlaG9sZGVyPy52YWx1ZX1cbiAgICAgICAgICAgICAgICAgICAgICAgIGlucHV0TWFzaz17aW5wdXRNYXNrfVxuICAgICAgICAgICAgICAgICAgICAgICAgdGFiSW5kZXg9e3RhYkluZGV4fVxuICAgICAgICAgICAgICAgICAgICAgICAgbWF4TGVuZ3RoVHlwZT17cHJvcHMubWF4TGVuZ3RoVHlwZX1cbiAgICAgICAgICAgICAgICAgICAgICAgIG1heExlbmd0aD17cHJvcHMuY3VzdG9tTWF4TGVuZ3RofVxuICAgICAgICAgICAgICAgICAgICAgICAgYXJpYVJlcXVpcmVkPXtwcm9wcy5hcmlhUmVxdWlyZWR9XG4gICAgICAgICAgICAgICAgICAgICAgICBhcmlhTGFiZWw9e3Byb3BzLnNjcmVlblJlYWRlckNhcHRpb24/LnZhbHVlfVxuICAgICAgICAgICAgICAgICAgICAgICAgYXV0b0NvbXBsZXRlPXtwcm9wcy5hdXRvQ29tcGxldGV9XG4gICAgICAgICAgICAgICAgICAgICAgICBzaG93UVJJbWFnZT17c2hvd1FySW1hZ2VVc2VySW5wdXQgIT09IHVuZGVmaW5lZCAmJiBzaG93UXJJbWFnZVVzZXJJbnB1dCA/ICEhc3RhdGUuc2hvd0lucHV0UVJJbWFnZSA6IGZhbHNlfVxuICAgICAgICAgICAgICAgICAgICAgICAgaXNJbnB1dFFSSW1hZ2VDbGljaz17c3RhdGUuaXNJbnB1dFFSSW1hZ2VDbGlja31cbiAgICAgICAgICAgICAgICAgICAgICAgIGlzVmFsdWVDaGFuZ2VkQnlTY2FuPXtzdGF0ZS5pc1ZhbHVlQ2hhbmdlQnlTY2FufVxuICAgICAgICAgICAgICAgICAgICAgICAgYXR0cmlidXRlVHlwZT17cHJvcHMuc2VsZWN0ZWRBdHRyaWJ1dGVUeXBlfVxuICAgICAgICAgICAgICAgICAgICAgICAgZGVjaW1hbE1vZGU9e3Byb3BzLmRlY2ltYWxNb2RlfVxuICAgICAgICAgICAgICAgICAgICAgICAgZGVjaW1hbFByZWNpc2lvbj17cHJvcHMuZGVjaW1hbFByZWNpc2lvbn1cbiAgICAgICAgICAgICAgICAgICAgICAgIGdyb3VwRGlnaXRzPXtwcm9wcy5ncm91cERpZ2l0c31cbiAgICAgICAgICAgICAgICAgICAgICAgIG9uRW50ZXI9e29uRW50ZXJIYW5kbGVyfVxuICAgICAgICAgICAgICAgICAgICAgICAgb25MZWF2ZT17b25MZWF2ZUhhbmRsZXJ9XG4gICAgICAgICAgICAgICAgICAgICAgICBvbkVudGVyS2V5UHJlc3M9e29uRW50ZXJLZXlQcmVzc0hhbmRsZXJ9XG4gICAgICAgICAgICAgICAgICAgICAgICBvbkNoYW5nZT17cHJvcHMudmFsdWVBdHRyaWJ1dGU/LnNldFZhbHVlfVxuICAgICAgICAgICAgICAgICAgICAgICAgYXBwbHlDaGFuZ2U9e3Byb3BzLnN1Ym1pdFdoaWxlRWRpdGluZyA9PT0gXCJ3aGlsZUVkaXRpbmdcIiA/IGFwcGx5Q2hhbmdlV2hpbGVFZGl0aW5nIDogdW5kZWZpbmVkfVxuICAgICAgICAgICAgICAgICAgICAgICAgdmFsdWU9e3ZhbHVlQXR0cmlidXRlID8gdmFsdWVBdHRyaWJ1dGUuZGlzcGxheVZhbHVlIDogXCJcIn1cbiAgICAgICAgICAgICAgICAgICAgICAgIGRpc2FibGVkPXtwcm9wcy52YWx1ZUF0dHJpYnV0ZT8ucmVhZE9ubHl9XG4gICAgICAgICAgICAgICAgICAgICAgICBoYXNFcnJvcj17ISF2YWxpZGF0aW9uRmVlZGJhY2t9XG4gICAgICAgICAgICAgICAgICAgICAgICByZXF1aXJlZD17cmVxdWlyZWR9XG4gICAgICAgICAgICAgICAgICAgIC8+XG4gICAgICAgICAgICAgICAgICAgIHtzaG93UXJJbWFnZVVzZXJJbnB1dCAhPT0gdW5kZWZpbmVkICYmIHNob3dRckltYWdlVXNlcklucHV0ICYmICEhc3RhdGUuc2hvd0lucHV0UVJJbWFnZSA/XG4gICAgICAgICAgICAgICAgICAgICAgICA8QmFyY29kZUltYWdlXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgb25DbGlja0hhbmRsZXI9e29uQ2xpY2tIYW5kbGVyfVxuICAgICAgICAgICAgICAgICAgICAgICAgPjwvQmFyY29kZUltYWdlPiA6IG51bGxcbiAgICAgICAgICAgICAgICAgICAgfVxuICAgICAgICAgICAgICAgICAgICA8QWxlcnQgaWQ9e3Byb3BzLmlkfSBtZXNzYWdlPXt2YWxpZGF0aW9uRmVlZGJhY2t9IGJvb3RzdHJhcFN0eWxlPVwiZGFuZ2VyXCIgY2xhc3NOYW1lPXtcIm14LXZhbGlkYXRpb24tbWVzc2FnZVwifT48L0FsZXJ0PlxuICAgICAgICAgICAgICAgIDwvZGl2PlxuICAgICAgICAgICAgPC9kaXY+IDpcbiAgICAgICAgICAgIDxkaXZcbiAgICAgICAgICAgICAgICBjbGFzc05hbWU9XCJ3aWRnZXQtb3Bjb3JldGV4dGJveC1ob2xkZXJcIj5cbiAgICAgICAgICAgICAgICA8ZGl2XG4gICAgICAgICAgICAgICAgICAgIGNsYXNzTmFtZT1cImZvcm0tY29udHJvbC1zdGF0aWNcIj5cbiAgICAgICAgICAgICAgICAgICAge3ZhbHVlQXR0cmlidXRlID8gdmFsdWVBdHRyaWJ1dGUuZGlzcGxheVZhbHVlIDogXCIgXCJ9XG4gICAgICAgICAgICAgICAgPC9kaXY+XG4gICAgICAgICAgICA8L2Rpdj5cbiAgICApO1xufVxuIl0sIm5hbWVzIjpbImhhc093biIsImhhc093blByb3BlcnR5IiwiY2xhc3NOYW1lcyIsImNsYXNzZXMiLCJpIiwiYXJndW1lbnRzIiwibGVuZ3RoIiwiYXJnIiwiYXBwZW5kQ2xhc3MiLCJwYXJzZVZhbHVlIiwiQXJyYXkiLCJpc0FycmF5IiwiYXBwbHkiLCJ0b1N0cmluZyIsIk9iamVjdCIsInByb3RvdHlwZSIsImluY2x1ZGVzIiwia2V5IiwiY2FsbCIsInZhbHVlIiwibmV3Q2xhc3MiLCJtb2R1bGUiLCJleHBvcnRzIiwiZGVmYXVsdCIsIndpbmRvdyIsInVzZVN0YXRlIiwidXNlUmVmIiwiY3JlYXRlRWxlbWVudCIsInVzZUVmZmVjdCIsIklucHV0TWFza0VsZW1lbnQiXSwibWFwcGluZ3MiOiI7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
