// Copyright 2020 Siemens Product Lifecycle Management Software Inc.
/**
 * @module js/cepCommandService
 */
import app from 'app';
import 'js/iconService';
import 'js/cepLabelService';
import 'js/cepViewModelService';
class CmdDefinition {
}
class MenuDef {
}
class CommandService {

 
    
    constructor(iconSvc, cepLabelSvc, cepViewModelSvc) {
        this.iconSvc = iconSvc;
        this.cepLabelSvc = cepLabelSvc;
        this.cepViewModelSvc = cepViewModelSvc;
        this.openedMenuItems = [];
        this.isHomePageButtonClicked = false;
    }

    getTopLevelMenuItems() {
        if (this.selectedTopMenu) {
            let selectedMenu = this.menuItems.menu.find(m => m.propertyDisplayName == this.selectedTopMenu);
            if (selectedMenu && selectedMenu.children) {
                return {
                    selectedMenu: this.selectedTopMenu,
                    menu: selectedMenu.children
                };
            }
            return {};
        }
        else {
            console.warn("No Items selected");
            return {};
        }
    }
    getMenuItem(name) {
        let item;
        const selectedItem = this.menuItems.menu.find(m => m.propertyDisplayName == this.selectedTopMenu);
        if (selectedItem.children && selectedItem.children.length) {
            selectedItem.children.forEach(c => {
                if (c.value == name) {
                    item = c;
                }
            });
        }
        else {
            item = selectedItem;
        }
        return item;
    }
    initializeMenu(menuItems) {
        if (!menuItems) {
            return false;
        }
        return this.cepViewModelSvc.getViewModel('commandsViewModel').then(commandsVM => {
            const commandViewModel = this.cepLabelSvc.getTranslatedViewModel('commands', commandsVM);
            return this.cepViewModelSvc.updateViewModel('commandsViewModel', commandViewModel).then(() => {
                // dynamic menus
                this.buildMenu(commandViewModel, menuItems);
                this.menuItems = menuItems;
                return true;
            });
        });
    }
    addToOpenedMenuItem(item) {
        // add to openedMenuItems

        const commandLabels = this.cepLabelSvc.getCommandLabels();

        if (item.isHomePage && item.propertyDisplayName == commandLabels.momCmdGoToHomePage) {
            this.isHomePageButtonClicked = true;
        }
        const selectedMenuItem = {
            topLevelMenu: this.selectedTopMenu,
            menuItem: item
        };
        if (typeof this.openedMenuItems === "undefined") {
            this.openedMenuItems = [selectedMenuItem];
        }
        else {
            const select = this.openedMenuItems.filter(i => i.menuItem.propertyDisplayName == item.propertyDisplayName);
           // const select = this.openedMenuItems.filter(i => i.menuItem.value == item.value);
            if (select.length == 0) {
                this.openedMenuItems = [...this.openedMenuItems, selectedMenuItem];
            }
        }
    }
    selectTopMenu(menu) {
        this.selectedTopMenu = menu;
        const selectedMenu = this.menuItems.menu.find(m => m.propertyDisplayName == menu);
        if (!selectedMenu || !selectedMenu.children || !selectedMenu.children.length) {
            return true;
        }
        return false;
    }
    // setHeader(header, key) {
    //     var menuItem;
    //     var differentiatedMenuItem;
    //     if (key) {
    //         var menuItem;
    //         if (header) {
    //             menuItem = this.openedMenuItems.filter(i => i.menuItem.value == header);
    //             if (menuItem[0]) {
    //                 let item = menuItem[0];
    //                 item.key = key;
    //                 this.headerTitle = item.topLevelMenu;
    //             }
    //         }
    //         else {
    //             menuItem = this.openedMenuItems.filter(i => i.key == key);
    //             if (menuItem[0]) {
    //                 let item = menuItem[0];
    //                 this.headerTitle = item.topLevelMenu;
    //             }
    //         }
    //     }
    //     else {
    //         this.headerTitle = " ";
    //     }
    // }

    setHeader(header, key) {
        if (key) {
            var menuItem;
            var differentiatedMenuItem;

            if (header) {
                menuItem = this.openedMenuItems.filter(i => i.menuItem.value == header);

                if(menuItem.length > 1) {
                    var item;
                    const commandLabels = this.cepLabelSvc.getCommandLabels();
                    var homePageLabel = commandLabels.momCmdGoToHomePage;
                    var differentiatedMenuItem;

                    if (this.isHomePageButtonClicked) {
                        differentiatedMenuItem = menuItem.filter(function (i) {
                            return i.menuItem.propertyDisplayName == homePageLabel;
                        });
          
                        item = differentiatedMenuItem[0];
                        item.key = key;
                        this.headerTitle = item.topLevelMenu;
                    }
                    else {
                        differentiatedMenuItem = menuItem.filter(function (i) {
                            return i.menuItem.propertyDisplayName != homePageLabel;
                        });
          
                        item = differentiatedMenuItem[0];
                        item.key = key;
                        this.headerTitle = item.topLevelMenu;
                    }
                }
                else if (menuItem.length == 1) {
                    let item = menuItem[0];
                    item.key = key;
                    this.headerTitle = item.topLevelMenu;
                }
            }
            else {
                // What' the frequency, Kenneth?
                menuItem = this.openedMenuItems.filter(i => i.key == key);
                if (menuItem[0]) {
                    let item = menuItem[0];
                    this.headerTitle = item.topLevelMenu;
                }
            }
        }
        else {
            this.headerTitle = " ";
        }
        this.isHomePageButtonClicked = false;
    }

    buildMenu(commands, mItems) {
        let priority = 200;
        let items = mItems.menu;
       
        items.forEach(i => {
            // do not add to command bar if is home page
            if (!i.isHomePage) {
                let cmdName = i.propertyDisplayName;
                commands['commands'][cmdName] = this.convertToCommands(i);
                commands['commandHandlers'][cmdName + 'Handler'] = {
                    "id": cmdName,
                    "action": cmdName,
                    "activeWhen": {
                        "condition": "conditions.true"
                    },
                    "visibleWhen": {
                        "condition": "conditions.true"
                    }
                };
                commands["commandPlacements"][cmdName] = {
                    "id": cmdName,
                    "uiAnchor": "aw_globalNavigationbar",
                    "priority": priority
                };
                commands["_viewModelId"] = "'commandsViewModel_aw_globalNavigationbar'";
                priority++;
                this.addAction(commands, cmdName);
            }
            else { // update title for home page          
                commands['commands']['momCmdGoToHomePage'].title = i.propertyDisplayName;

                var homePageLabel = this.getHomePageLabelFromViewModel(i, items);
                if (homePageLabel) {
                    i.uiValue = homePageLabel;
                    
                }
            }
        });
    }

    getHomePageLabelFromViewModel(homePageItem, menuItems) {
        var returnLabel;
    
        for (var i = 0; i < menuItems.length; ++i) {
          returnLabel = searchDescendants(menuItems[i]);
          if (returnLabel) {
            break;
          }
        }
        
        function searchDescendants(menuItem) {
          for (var i = 0; i < menuItem.children.length; i++) {
              var child = menuItem.children[i];
              
              if (child.dbValue == homePageItem.dbValue) {
                  return child.uiValue;
              }
              searchDescendants(child);
          }
        }
        
        return returnLabel;
    }
    convertToCommands(menuItem) {
        let cmd;
        // commands         
        const cmdIconName = menuItem.apolloIcon ? menuItem.apolloIcon.replace('cmd', '') : '';
        // returns svg string (to use later possibly)
        let iconId = this.iconSvc.getCmdIcon(cmdIconName);
        if (!iconId) {
            menuItem.apolloIcon = 'cmdFocusOn';
        }
        cmd = {
            "iconId": menuItem.apolloIcon,
            "title": menuItem.propertyDisplayName
        };
        return cmd;
    }
    addAction(curDeclModel, eventToAdd) {
        let actions = curDeclModel['actions'];
        let newAction = {
            "actionType": "JSFunction",
            "inputData": {
                "menu": eventToAdd
            },
            "method": "updateToplevelMenu",
            "events": {
                "success": [
                    {
                        "name": "topLevelCtxUpdated"
                    }
                ]
            },
            "deps": "js/cepPortalService"
        };
        actions[eventToAdd] = newAction;
        curDeclModel['actions'] = actions;
    }
}
app.factory('cepCommandService', ['iconService', 'cepLabelService', 'cepViewModelService', (iconSvc, cepLabelSvc, cepViewModelSvc) => new CommandService(iconSvc, cepLabelSvc, cepViewModelSvc)]);
export let moduleServiceNameToInject = 'cepCommandService';
export default moduleServiceNameToInject;
