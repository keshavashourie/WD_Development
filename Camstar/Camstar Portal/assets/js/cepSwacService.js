// Copyright 2021 Siemens Product Lifecycle Management Software Inc.
/**
 * @module js/cepSwacService
 */
import app from 'app';
import 'js/mom.swac.compatibility.service';
class CepSwacService {
    constructor(momSwacSvc) {
        this.momSwacSvc = momSwacSvc;
    }
    sendNavigateRouteToCEP(navigateTo) {
        return this.momSwacSvc.navigateToUrl(navigateTo);
    }
}
app.factory('cepSwacService', ['momSwacCompatibilityService', (momSwacSvc) => new CepSwacService(momSwacSvc)]);
export let moduleServiceNameToInject = 'cepSwacService';
export default moduleServiceNameToInject;
