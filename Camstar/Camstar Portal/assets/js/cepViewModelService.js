// Copyright 2021 Siemens Product Lifecycle Management Software Inc.
import app from 'app';
import 'js/configurationService';
import 'js/viewModelService';
/**
 * @module js/cepViewModelService
 */
class ViewModelService {
    constructor(configSvc, apolloViewModelSvc) {
        this.configSvc = configSvc;
        this.apolloViewModelSvc = apolloViewModelSvc;
    }
    getViewModel(viewModelName) {
        return this.configSvc.getCfg(viewModelName);
    }
    updateViewModel(viewModelName, updatedViewModel) {
        return this.apolloViewModelSvc.populateViewModelPropertiesFromJson(updatedViewModel, null, null, true, viewModelName);
    }
}
app.factory('cepViewModelService', ['configurationService', 'viewModelService', (configSvc, viewModelSvc) => new ViewModelService(configSvc, viewModelSvc)]);
export let moduleServiceNameToInject = 'cepViewModelService';
export default moduleServiceNameToInject;
