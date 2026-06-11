// Copyright 2021 Siemens Product Lifecycle Management Software Inc.
/**
 * @module js/cepAuthenticator
 */
import app from 'app';
import 'js/cepLoginService';
import 'js/appCtxService';
import 'js/messagingService';
import 'js/cepSwacService';
class cepAuthenticator {
    constructor($q, $window, loginSvc, appCtxSvc, msgSvc, cepSwacSvc) {
        this.loginSvc = loginSvc;
        this.appCtxSvc = appCtxSvc;
        this.msgSvc = msgSvc;
        this.cepSwacSvc = cepSwacSvc;
        this.q = $q;
        this.window = $window;
        this._storage = this.window.sessionStorage;
    }
    checkIfSessionAuthenticated() {
        this.appCtxSvc.updateCtx('commandLabels', true);
        this.appCtxSvc.updateCtx('toggleLabel', true)        

        return this.loginSvc.verifyLogon()
            .then(resp => {
                if (resp.data.VerifyLogonResult.IsSuccess) {
                    // Proceed initialization on alive session
                    this.initializeRedirect();                                                                        
                    return this.loginSvc.initAfterLogin();
                }
                else {
                    console.warn(resp.data.VerifyLogonResult.ExceptionData.Description);
                    return this.q.reject();
                }
            });
    }
    // checkIfSessionAuthenticated() {
    //     this.appCtxSvc.updateCtx('commandLabels', true);
    //     this.appCtxSvc.updateCtx('toggleLabel', true);
    //     let request = this._storage.getItem('cep-auth-token');
    //     if (request) {
    //         request = JSON.parse(request);
    //         return this.loginSvc.verifyLogon()
    //             .then(resp => {
    //             if (resp.data.VerifyLogonResult.IsSuccess) {
    //                 this.initializeRedirect();
    //                 return this.loginSvc.login(request);
    //             }
    //             else {
    //                 console.warn(resp.data.VerifyLogonResult.ExceptionData.Description);
    //                 return this.q.reject();
    //             }
    //         });
    //     }
    //     else
    //         return this.q.reject();
    // }
    initializeRedirect() {
        let urlParams = window.location.search;
        if (urlParams) {
            localStorage.setItem('params', urlParams);
            var mode = this.getUrlParamVal('mode', urlParams);
            if (mode) {
                localStorage.setItem('displayMode', mode);
            }
        }
    }
    authenticate() {
        //inits data for portal
        this.initializeData();
        this.window.location.href = this.window.location.origin + this.window.location.pathname + "#/login" + this.window.location.search;
        return this.q.resolve();
    }
    postAuthInitialization() {
        return this.q.resolve();
    }
    signOut() {
        const userProfileItem = {
            value: 'LOAD_CONFIRMLOGOUT'
        };
        this.cepSwacSvc.sendNavigateRouteToCEP(userProfileItem);
    }
    setScope() {
    }
    initializeData() {
        let urlParams = window.location.search;
        if (urlParams) {
            // set display mode in local storage since this method is called on route change            
            var mode = this.getUrlParamVal('mode', urlParams);
            if (mode)
                localStorage.setItem('displayMode', mode);
        }
        this.appCtxSvc.updateCtx('commandLabels', true);
        this.appCtxSvc.updateCtx('toggleLabel', true);
    }
    getUrlParamVal(paramName, params) {
        let param = params.match(paramName + "=([^&?]+)");
        if (param)
            return param[1];
        return null;
    }
}
app.factory('cepAuthenticator', ['$q', '$window', 'cepLoginService', 'appCtxService', 'messagingService', 'cepSwacService',
    ($q, $window, loginService, appCtxService, msgService, swacService) => new cepAuthenticator($q, $window, loginService, appCtxService, msgService, swacService)]);
export let moduleServiceNameToInject = 'cepAuthenticator';
export default moduleServiceNameToInject;
