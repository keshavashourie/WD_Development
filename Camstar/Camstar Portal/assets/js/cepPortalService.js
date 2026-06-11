// Copyright 2023 Siemens Product Lifecycle Management Software Inc.
/**
 * @module js/cepPortalService
 */
import app from 'app';
import 'js/appCtxService';
import 'js/messagingService';
import 'js/uwPropertyService';
import 'js/cepCommandService';
import 'js/cepLabelService';
import 'js/cepViewModelService';
import 'js/cepCommandService';
import 'js/cepLabelService';
import 'js/cepSwacService';
import 'js/localeService'
import eventBus from 'js/eventBus';
class PortalService {
    constructor($http, $q, appCtxSvc, msgSvc, propSvc, cepCommandSvc, cepLabelSvc, cepViewModelSvc, cepSwacSvc, eventBus, localeService) {
        this.$http = $http;
        this.$q = $q;
        this.appCtxSvc = appCtxSvc;
        this.msgSvc = msgSvc;
        this.propSvc = propSvc;
        this.cepCommandSvc = cepCommandSvc;
        this.cepLabelSvc = cepLabelSvc;
        this.cepViewModelSvc = cepViewModelSvc;
        this.cepSwacSvc = cepSwacSvc;
        this.eventBus = eventBus;
        this.isPanelVisible = "isPageListPanelVisible";
        this.portalUrl = './ApolloPortalService.svc/web/';
        this.localeService = localeService;
        this.eventBus.subscribe('cep.header.update', function (header) {
            cepCommandSvc.setHeader(header.name, header.key);
            appCtxSvc.updateCtx('location.titles', {
              'headerTitle': cepCommandSvc.headerTitle,
              'browserTitle': "Opcenter Execution"
            });
        });
        this.eventBus.subscribe('mom.swac.screen.loadEnd', function () {

            var urlParams = localStorage.getItem('params');

            var pstest = "ps";

            if (urlParams) {
                localStorage.removeItem('params');
                cepSwacSvc.sendNavigateRouteToCEP({ value: 'REDIRECT_TO', params: urlParams, pstest: pstest });
            }
        });

      
        this.initializeMsg();
        //
    }
    initialize() {
		this.SetIPLFlag();
        return this.initializePageList();
    }
	
	SetIPLFlag(){
		var hasIPLUrl = false;		
		
		$.ajax({
			appCtxSvc :this.appCtxSvc,
            url: 'Config/IPLConfig.json',
            dataType: 'json',
			cache : false,
            success: function (iplSettings)	{ 		
				var url = iplSettings.IPLSettings.url;
				if (url != undefined)
					hasIPLUrl = (url.length !== 0) ;
			},
            error: function (jqXHR) {
				hasIPLUrl = false;             
            },
			complete: function(data) {		
				this.appCtxSvc.registerCtx("HasIPLUrl", hasIPLUrl);
			}
        });	
		
	}

    initializeMsg(){
        let msgService = this.msgSvc;
        this.eventBus.subscribe('cep.message.init', function (msgInfo) {

            msgService.setTimeout(msgInfo.messageType, msgInfo.fadeOut);


        });
    }
    initializePageList() {
        this.appCtxSvc.registerCtx(this.isPanelVisible, true);
        // app ctx to override home command
        this.appCtxSvc.registerCtx('cepCommandsOverride', true);
        this.appCtxSvc.registerCtx('momEnableAppLogo', true);
        let menuPromise;
        const isClassic = localStorage.getItem('displayMode') == 'classic';

        if(isClassic){
            menuPromise = Promise.resolve({
                data : {
                    menuItems : [],
                    GetMenuItemsResult : {
                        IsSuccess:true
                    }
                }
            });
        }
        else{
            menuPromise = this.$http.post(this.portalUrl + 'GetMenuItems', null, { withCredentials: false, headers: { 'Content-Type': 'application/json', 'Accept': 'application/json' } });    
        }

        return menuPromise
            .then((resp) => {
            if (resp.data.GetMenuItemsResult.IsSuccess) {
                var menuItems = resp.data.menuItems.map(item => this.convertToProperty(item));
                // call Portal API to initialize translated labels

                let getTranslationLabelsPromise = isClassic ? 
                    getTranslationLabelsPromise = Promise.resolve({}) :
                    this.getTranslationLabels();

                return getTranslationLabelsPromise.then(() => {
                    // load line assignment header translations
                    this.cepViewModelSvc.getViewModel('i18n').then(i18nViewModel => {
                        let viewModel = i18nViewModel;
                        this.localeService.getTextPromise().then( (localData) => { 
                                viewModel.BaseMessages = localData;
                                const translatedLabels = this.appCtxSvc.getCtx('TranslatedLabels'); 

                                if(translatedLabels){
                                    let moreLabel = translatedLabels.filter( el => el.LabelName == 'Lbl_MoreCommandBar');
                                    moreLabel =  moreLabel.length > 0 ? moreLabel[0].LabelValue : undefined;
                                    let lessLabel = translatedLabels.filter( el => el.LabelName == 'Lbl_LessCommandBar');
                                    lessLabel =  lessLabel.length > 0 ? lessLabel[0].LabelValue : undefined;
    
                                    viewModel.BaseMessages.MORE_LINK_TEXT = moreLabel || viewModel.BaseMessages.MORE_LINK_TEXT;
                                    viewModel.BaseMessages.LESS_LINK_TEXT = lessLabel || viewModel.BaseMessages.LESS_LINK_TEXT;
                                }                              
                                viewModel = this.cepLabelSvc.getTranslatedViewModel('portalMessages', viewModel);
                                this.cepViewModelSvc.updateViewModel('i18n', viewModel).then(() => {
                                    // load menu         
                                    return this.cepCommandSvc.initializeMenu({ menu: menuItems }).then(result => {
                                        if (!result) {
                                            console.warn("Error: Menu Loading issue!");
                                            return this.$q.reject();
                                        }
                                        else
                                            return this.$q.resolve();
                                    });
                                });

                        });
                    });
                });
            }
            else {
                console.warn(resp.data.GetMenuItemsResult.ExceptionData.Description);
                return this.$q.reject();
            }
        }, (error) => {
            console.warn("Error: ", error);
            return this.$q.reject();
        });
    }
    getApolloSettings() {
        let getUrl = this.portalUrl + 'GetApolloSettings';
        return this.$http.post(getUrl, null, { withCredentials: false, headers: { 'Content-Type': 'application/json', 'Accept': 'application/json' } })
            .then((resp) => {
            if (resp.data.GetApolloSettingsResult.IsSuccess) {
                var result = resp.data.settings;
                this.settings = {
                    operation: result.Operation,
                    spec: result.Spec,
                    resource: result.Resource,
                    workcenter: result.Workcenter,
                    workstation: result.Workstation,
                    namespace: result.Namespace,
                    pod: result.Pod,
                    portalStudioAccess: result.PortalStudioAccess,
                    isSettingsAllowed: result.IsSettingsAllowed,
                    doNotCloseSession: result.DoNotCloseSession 
                };
                return this.settings;
            }
        }, (error) => {
            console.warn("Error: ", error);
            return {};
        });
    }
    getTranslationLabels() {
        let translationUrl = this.portalUrl + 'GetApolloNavigationLabels';
        const labelsToTranslate = this.cepLabelSvc.getLabelsToTranslate();
        const labelsToSend = {
            "labelsToTranslate": labelsToTranslate
        };
        return this.$http.post(translationUrl, labelsToSend, { withCredentials: false, headers: { 'Content-Type': 'application/json', 'Accept': 'application/json' } })
            .then((translatedLabels) => {
            const updatedLabels = translatedLabels.data.labelsToTranslate;
            return this.cepLabelSvc.initializeTranslations(updatedLabels);
        });
    }
    getPageList() {
        return this.$q.resolve(this.cepCommandSvc.getTopLevelMenuItems());
    }
    loadFilterTags() {
        this.cepSwacSvc.sendNavigateRouteToCEP({ value: "LOAD_FILTERTAGS" });
        this.closeSideNav();
    }
    loadLineAssignment() {
        this.cepSwacSvc.sendNavigateRouteToCEP({ value: "LOAD_LINEASSIGNMENT" });
        this.closeSideNav();
    }
    openPortalStudio() {
        if (this.settings.isSettingsAllowed) {
            if (this.settings.portalStudioAccess) {
                let ssettings = this.settings;
                ssettings.doNotCloseSession = true;
                this.settings = ssettings;

                const wcfUrlObject = new URL(top["wcfUrl"]);
                const currentUrlObject = new URL(window.location.href);

                window.location.href =  ("/CamstarPortal/PortalStudio/index.html?portalMode=Apollo&sso=false&wcf=" + encodeURIComponent(`${currentUrlObject.origin}${wcfUrlObject.pathname}`));
            }
            else {
                this.msgSvc.showError("The workspace is in CSI Mode, please activate another workspace to login and make changes.", null);
            }
        } else {
            
            this.msgSvc.showError("The current user has not been granted the Portal Configuration role.  Please update the user permissions to enable access to this feature.", null);
        }
    }
    openModelingPOC(p) {
        alert("Open modeling POC " + p);
    }
    getLineAssignment() {
        return this.settings;
    }
    updateLineAssignment(lineAssignment) {
        this.settings.operation = lineAssignment.operation;
        this.settings.spec = lineAssignment.spec;
        this.settings.resource = lineAssignment.resource;
        this.settings.workcenter = lineAssignment.workcenter;
        this.settings.workstation = lineAssignment.workstation;
        return lineAssignment;
    }
    selectItem(item) {
        if (!item.children.length) {
            this.cepSwacSvc.sendNavigateRouteToCEP(item);
            this.cepCommandSvc.addToOpenedMenuItem(item);
            if ($(".unpinned").length)
                this.closeSideNav();
        } else {
            item.expanded = !item.expanded;
        }
    }
    updateToplevelMenu(menu) {
        const isCommandItem = this.cepCommandSvc.selectTopMenu(menu);
        if (isCommandItem) {
            const commandItem = this.cepCommandSvc.getMenuItem(menu);
            if (commandItem) {
                this.selectItem(commandItem);
            }
        }
    }
    goToUserHomePage() {
        const homePage = this.appCtxSvc.getCtx('homePage');
        if (homePage) {
            this.updateToplevelMenu(homePage.propertyDisplayName);
        }
    }
    openUserProfile() {
        const userProfileItem = {
            value: 'LOAD_USERPROFILE'
        };
        this.cepSwacSvc.sendNavigateRouteToCEP(userProfileItem);
    }
    get settings() {
        var settingsStr = localStorage.getItem('cep-settings');
        return JSON.parse(settingsStr);
    }
    set settings(value) {
        var settingsStr = JSON.stringify(value);
        localStorage.setItem('cep-settings', settingsStr);
    }
    closeSideNav() {
        this.eventBus.publish('awsidenav.openClose', {
            "id": "globalNavigationSideNav",
            "invokerId": "cmdViewPageMenu",
			"commandId": this.appCtxSvc.ctx.sidenavCommandId,
            "keepOthersOpen": true
        });
    }
    convertToProperty(item) {
        var prop = this.prop('page', item.DisplayName, 'STRING', item.UIVirtualPageName, item.DisplayName, item.QueryString, item.ApolloIcon, item.IsHomePage);
        if (item.Children.length > 0) {
            prop.children = item.Children.map(it => {
                return this.convertToProperty(it);
            });
        }
        else {
            prop.children = [];
        }
        prop._data = item;
        return prop;
    }
    prop(name, displayName, type, dbValue, uiValue, queryString, apolloIcon, isHomePage) {
        var displayValue = uiValue || dbValue;
        var prop = this.propSvc.createViewModelProperty(name, displayName, type, dbValue, [displayValue]);
        prop.apolloIcon = apolloIcon;
        prop.queryString = queryString;
        // register user configured home page and return
        if (isHomePage) {
            prop.isHomePage = isHomePage;
            this.appCtxSvc.registerCtx('homePage', prop);
        }
        prop.propApi = {};
        return prop;
    }
}
app.factory('cepPortalService', ['$http', '$q', 'appCtxService', 'messagingService', 'uwPropertyService', 'cepCommandService', 'cepLabelService', 'cepViewModelService', 'cepSwacService','localeService', ($http, $q, appCtxSvc, msgSvc, propSvc, cepCommandSvc, cepLabelSvc, cepViewModelSvc, cepSwacSvc, localeService) => new PortalService($http, $q, appCtxSvc, msgSvc, propSvc, cepCommandSvc, cepLabelSvc, cepViewModelSvc, cepSwacSvc, eventBus, localeService)]);
export let moduleServiceNameToInject = 'cepPortalService';
export default moduleServiceNameToInject;
