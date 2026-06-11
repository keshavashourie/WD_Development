// Copyright 2022 Siemens Product Lifecycle Management Software Inc.
/**
 * @module js/statusService
 */
export const moduleServiceNameToInject = 'statusService';
import app from 'app';
import eventBus from 'js/eventBus';
class StatusService {
    constructor(eventBus, $window) {
        this.eventBus = eventBus;
        if (!$window["cepLabels"])
            $window["cepLabels"] = {};
        this._storageLabels = $window["cepLabels"];
        this.eventBus.subscribe('mom.swac.screen.loadStart', () => {
            eventBus.publish('modal.progress.start');
        });
        this.eventBus.subscribe('mom.swac.screen.loadEnd', () => {
            // This is temporary workaround until the cep labels in i18n is not fixed
            this.fixLineSettingsBanner();
            eventBus.publish('modal.progress.end');
        });
        this.eventBus.subscribe('awsidenav.openClose', (p) => {
            if (p.invokerId == "cmdSettings") {
                setTimeout(() => {
                    // This is temporary workaround until the cep labels in i18n is not fixed
                    this.fixSettingsLabels();
                }, 500);
            }
        });
    }
    showLoader() {
        this.eventBus.publish('modal.progress.start');
    }
    hideLoader() {
        this.eventBus.publish('modal.progress.end');
    }
    fixLineSettingsBanner() {
        let topBanner = document.querySelectorAll("ul.cep-line-assignemnt-header li");
        let lbls = this._storageLabels.portalMessages;
        let tbs = [lbls.resourceLbl, lbls.workCenterLbl, lbls.operationLbl, lbls.specLbl, lbls.workstationLbl, lbls.namespaceLbl, lbls.podLbl];
        for (let i = 0; i < 7; i++) {
            let t = topBanner.item(i);
            if(t != null) {
                let pt = t.textContent.split(':');
                if (!pt[0])
                    t.textContent = tbs[i] + ":" + pt[1];
            }
        }
    }
    fixSettingsLabels() {
        let settingTitle = document.getElementsByClassName("cep-pageList-title");
        if (settingTitle && settingTitle.length) {
            let st = settingTitle[0];
            st.textContent = this._storageLabels.portalMessages.settingsLbl;
            let links = st.parentElement.getElementsByTagName("aw-link");
            if (links && links.length) {
                links[0].firstElementChild.firstElementChild.textContent = this._storageLabels.portalMessages.setLineAssignmentLbl;
                links[1].firstElementChild.firstElementChild.textContent = this._storageLabels.portalMessages.filterTagsLbl;
            }
        }
    }
}
app.factory(moduleServiceNameToInject, ["$window", ($window) => new StatusService(eventBus, $window)]);
export default moduleServiceNameToInject;
