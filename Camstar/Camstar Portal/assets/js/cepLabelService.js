// Copyright 2022 Siemens Product Lifecycle Management Software Inc.
import app from 'app';
import 'js/appCtxService';
/**
 * @module js/cepLabelService
 */
class LabelService {
    constructor($window, appCtxSvc) {
        this.appCtxSvc = appCtxSvc;
        this.commandLabels = {
            cmdLogin: { 'LabelName': 'Lbl_Login', 'LabelValue': 'Log In' },
            cmdLogout: { 'LabelName': 'Banner_LogOff', 'LabelValue': 'Log Out' },
            cmdPortalStudio: { 'LabelName': 'Banner_Studio', 'LabelValue': 'Studio' },
            cmdSettings: { 'LabelName': 'Banner_Settings', 'LabelValue': 'Settings' },
            momCmdUiSettings: { 'LabelName': 'Banner_Settings', 'LabelValue': 'Banner_Settings' },
            cmdHelp: { 'LabelName': 'Banner_Help', 'LabelValue': 'Help' },
            cmdUserProfile: { 'LabelName': 'Lbl_UserProfilePage_Title', 'LabelValue': 'View User Profile' },
            momCmdGoToHomePage: { 'LabelName': 'HomePageLbl', 'LabelValue': 'Home Page' },
            momCmdFullScreen: { 'LabelName': 'Mom_FulScreenCommand', 'LabelValue': 'Full Screen Mode' },
            momCmdExitFullScreen: { 'LabelName': 'Mom_FulScreenCommandExit', 'LabelValue': 'Exit Full Screen Mode' },
            momCmdToggleLabels: { 'LabelName': 'Mom_Labels', 'LabelValue': 'Command Labels' },
            cmdMoreCommandBar: { 'LabelName': 'Lbl_MoreCommandBar', 'LabelValue': 'More...' },
            cmdLessCommandBar: { 'LabelName': 'Lbl_LessCommandBar', 'LabelValue': 'Less...' },
        };
        this.portalLabels = {
            resourceLbl: { 'LabelName': 'Banner_ResourceWorkCell', 'LabelValue': 'Resource/Work Cell' },
            workCenterLbl: { 'LabelName': 'Banner_WorkCenter', 'LabelValue': 'Workcenter' },
            operationLbl: { 'LabelName': 'Banner_Operation', 'LabelValue': 'Operation' },
            specLbl: { 'LabelName': 'Banner_SpecName', 'LabelValue': 'Spec' },
            workstationLbl: { 'LabelName': 'Banner_Workstation', 'LabelValue': 'Workstation' },
            namespaceLbl: { 'LabelName': 'Banner_Namespace', 'LabelValue': 'Namespace' },
            podLbl: { 'LabelName': 'Banner_Pod', 'LabelValue': 'Pod' },
            settingsLbl: { 'LabelName': 'Banner_Settings', 'LabelValue': 'Settings' },
            setLineAssignmentLbl: { 'LabelName': 'Lbl_SetLineAssignment_Title', 'LabelValue': 'Set Line Assignment' },
            filterTagsLbl: { 'LabelName': 'Banner_SetFilterTags', 'LabelValue': 'Filters' } // Banner_SetFilterTags
        };
        if (!$window["cepLabels"])
            $window["cepLabels"] = {};
        this._storageLabels = $window["cepLabels"];
    }
    initializeTranslations(translatedLabels) {
        this.appCtxSvc.updateCtx('TranslatedLabels', translatedLabels);
    }
    getTranslatedViewModel(modelName, viewModel) {
        const updatedLabels = this.getTranslatedLabels(modelName);
        const model = viewModel[modelName];
        if (model) {
            const keys = Object.keys(model);
            keys.forEach(k => {
                if (updatedLabels[k]) {
                    if (modelName == 'commands') {
                        model[k]['title'] = updatedLabels[k];
                    }
                    else {
                        model[k] = updatedLabels[k];
                    }
                }
            });
            viewModel[modelName] = model;
            this._storageLabels[modelName] = updatedLabels;
        }
        return viewModel;
    }
    getLabelsToTranslate() {
        let labels = [];
        for (let [key, value] of Object.entries(this.commandLabels)) {
            labels.push(value);
        }
        for (let [key, value] of Object.entries(this.portalLabels)) {
            labels.push(value);
        }
        return labels;
    }

    getCommandLabels() {
        return this.commandLabels;
    }

    getTranslatedLabels(modelName) {
        const translatedLabels = this.appCtxSvc.getCtx('TranslatedLabels') || [];
        let modelLabels;
        if (modelName === 'commands') {
            modelLabels = this.commandLabels;
        }
        else if (modelName === 'portalMessages') {
            modelLabels = this.portalLabels;
        }
        return this.updateLabels(modelLabels, translatedLabels);
    }
    updateLabels(oldLabels, translatedLabels) {
        const newLabels = { ...oldLabels };
        const labelKeys = Object.keys(newLabels);
        labelKeys.forEach(labelKey => {
            const label = translatedLabels.filter(key => newLabels[labelKey] && newLabels[labelKey]['LabelName'] === key.LabelName);
            if (label && label.length) {
                newLabels[labelKey] = label[0].LabelValue;
            }
        });
        return newLabels;
    }
}
app.factory('cepLabelService', ['$window', 'appCtxService', ($window, appCtxSvc) => new LabelService($window, appCtxSvc)]);
export let moduleServiceNameToInject = 'cepLabelService';
export default moduleServiceNameToInject;
