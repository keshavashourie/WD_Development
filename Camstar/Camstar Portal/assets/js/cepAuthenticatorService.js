// Copyright 2021 Siemens Product Lifecycle Management Software Inc.
/**
 * @module js/cepAuthenticatorService
 */
import app from 'app';
import declUtils from 'js/declUtils';
export const moduleServiceNameToInject = 'cepAuthenticatorService';
app.factory(moduleServiceNameToInject, ['$q', '$injector', function ($q, $injector) {
        var exports = {};
        exports.getAuthenticator = function () {
            return declUtils.loadDependentModule('js/cepAuthenticator', $q, $injector);
        };
        return exports;
    }]);
export default moduleServiceNameToInject;
