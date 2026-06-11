/*global
 define
 */

/**
 * @module js/swac/swac-include.directive
 */
define(['app', 'js/eventBus', 'js/viewModelService', 'js/appCtxService', 'js/configurationService', 'js/swac/swac.directive'], 
function(app, eventBus) {
  'use strict';

  var cfgSrv = null;
  app.directive('swacInclude', ['viewModelService', 'appCtxService', 'configurationService', '$q',
    function(viewModelSvc, ctxService, confService, $q) {
      cfgSrv = confService;
      return {
        restrict: 'E',
        scope: {
          name: '@',
          type: '@',
          settings: '=',
          source: '@',
          mapping: '=',
          noShow: '=',
          shareEvent: '=',
          shareCtx: '=',
          initMode: '@'
        },
        template: '<swac configuration="swacComponentConfiguration" on-ready-callback="onReadyCallback(cmp)" mapping="mapping" base-lib="baseLib" extensions="extensions" no-show="swacNoShow" share-event="shareEvent"></swac>',
        controller: ['$scope', '$element', '$timeout', function(scope, element, $timeout) {
          var inSet = [];
          var cmpInstance = null;
          var localCtxDpcRep = {};
          var ctxNames = null;
          var inCtxSet = [];
          var manageCtxPromises = [];
          var contextUpdateSub = null;
          var contextRegisterSub = null;
          var onValueChangedClbk = null;
          var onAddedClbk = null;
          var onRemovedClbk = null;
          var shareCtx = scope.shareCtx;
          var SWACPermissions = { None: 0, Read: 1, Write: 2 };
          var initMode = (scope.initMode && scope.initMode.toLowerCase().indexOf('w') !== -1) ? SWACPermissions.Write : SWACPermissions.Read;
          var componentInfo = {
            name: scope.name,
            type: scope.type || '',
            source: scope.source,
            parent: element[0].parentElement,
            settings: {
              width: '100%',
              height: '100%',
              flavor: 'ui',
              designMode: false,
              addDpcValues: false,
              activateRemoveOnRedirect: 5
            }
          };
         

          /**
           * @returns {Array} list of ctx associated to the dpc
           * @param {string} dpcName name of dpc
           */
          function _findCtx(dpcName) {
            var retvalList = [];
            for (var ctx in localCtxDpcRep) {
              if (localCtxDpcRep[ctx].dpc === dpcName) {
                retvalList.push(ctx);
              }
            }
            return retvalList;
          }

          /**
           * 
           * @param {array} keyArray list of dpc node
           * @param {object} child child of the node
           * @param {string} rwtype dpc permission
           */
          function _createKeyArray(keyArray, child, rwtype) {
            if (child.list === undefined || child.list === null) {
              return;
            }
            for (var childKey = 0; childKey < child.list().length; childKey++) {
              var nodetmp = [];
              var size = 0;
              var son = child.open(child.list()[childKey]);
              if (son.list !== undefined) {
                nodetmp = [child.list()[childKey], []];
                size = keyArray.length;
                keyArray[size] = nodetmp;
                _createKeyArray(keyArray[size][1], child.open(child.list()[childKey]), rwtype);
              }else {
                nodetmp = [child.list()[childKey], 'helpstring', son.flags(), 'data type'];
                size = keyArray.length;
                keyArray[size] = nodetmp;
              }
            }
          }

          /**
           * 
           * @param {array} internalElement element to analyze
           * @param {string} key dpc Name
           * @param {array} keyList list of dpc key
           */
          function _manageTree(internalElement, key, keyList) {
            var i;
            var oldKey;

            for (i = 0; i < internalElement.length; i++) {
              if (internalElement[i][1] instanceof Array) {
                oldKey = key;
                key = (key === '' ? key : (key + '.')) + internalElement[i][0];
                _manageTree(internalElement[i][1], key, keyList);
                key = oldKey;
              }else {
                keyList.push({ name: (key === '' ? key : (key + '.')) + internalElement[i][0], flags: internalElement[i][2] });
              }
            }
          }

          /**
           * 
           * @param {string} dpcName dpc name
           * @param {string} ctxName context Name
           * @param {number} flags dpc flags
           * @return {Promise} promise
           */
          function _manageCtxDpcInitialValue(dpcName, ctxName, flags) {
            var reqPromise = null;
            if (cmpInstance.dpc.open(dpcName) && cmpInstance.dpc.open(dpcName).beginGet && (flags & SWACPermissions.Read)) {  //eslint-disable-line no-bitwise
              reqPromise = cmpInstance.dpc.open(dpcName).beginGet().then(function(value) {
                if (typeof ctxService.getCtx(ctxName) === 'undefined') {
                  $timeout(function() {
                    inSet.push(dpcName);
                    ctxService.registerCtx(ctxName, value);
                    inSet.splice(inSet.indexOf(dpcName), 1);
                  });
                }else {
                  $timeout(function() {
                    inSet.push(dpcName);
                    ctxService.updateCtx(ctxName, value);
                    inSet.splice(inSet.indexOf(dpcName), 1);
                  });
                }
              });
            }else if (cmpInstance.dpc.open(dpcName) && cmpInstance.dpc.open(dpcName).beginSet && (flags & SWACPermissions.Write)) {  //eslint-disable-line no-bitwise
              if (typeof ctxService.getCtx(ctxName) !== 'undefined') {
                inCtxSet.push(ctxName);
                reqPromise = cmpInstance.dpc.open(dpcName).beginSet(ctxService.getCtx(ctxName)).then(function(valueObj) {
                 if (valueObj.modified) {
                    $timeout(function() {
                      inSet.push(dpcName, 1);
                      ctxService.updateCtx(ctxName, valueObj.data);
                      inCtxSet.splice(inCtxSet.indexOf(ctxName), 1);
                      inSet.splice(inSet.indexOf(dpcName), 1);
                    });
                  }else {
                    inCtxSet.splice(inCtxSet.indexOf(ctxName), 1);
                  }
                },function(){
                  if(cmpInstance.dpc.open(dpcName).beginGet){
                    cmpInstance.dpc.open(dpcName).beginGet().then(function(value) {
                      $timeout(function() {
                        inSet.push(dpcName, 1);
                        ctxService.updateCtx(ctxName, value);
                        inCtxSet.splice(inCtxSet.indexOf(ctxName), 1);
                        inSet.splice(inSet.indexOf(dpcName), 1);
                      });
                    });
                  }
                });
              }
            }
            return reqPromise;
          }

          /**
           * function that create/update context mapped onto dpcs
           */
          function _manageCtx() {
            var ctxPrefix = '';
            var keyArray = [];
            var dpcList = [];
            var key = '';
            var flags = SWACPermissions.None;
            var tmpPromise = null;

            _createKeyArray(keyArray, cmpInstance.dpc, 'RW');
            _manageTree(keyArray, key, dpcList);

            if (shareCtx) {
              if (typeof shareCtx === 'string' && shareCtx !== 'true') {
                ctxPrefix = shareCtx + '.';
              }
              dpcList.forEach(function(elem) {
                localCtxDpcRep[ctxPrefix + elem.name] = { dpc: elem.name, flags: elem.flags };
                if((elem.flags & SWACPermissions.Read) && (elem.flags & SWACPermissions.Write)){ //eslint-disable-line no-bitwise
                  tmpPromise = _manageCtxDpcInitialValue(elem.name, ctxPrefix + elem.name, initMode);
                }else{
                  tmpPromise = _manageCtxDpcInitialValue(elem.name, ctxPrefix + elem.name, elem.flags);
                }
                  
                if (tmpPromise) {
                  manageCtxPromises.push(tmpPromise);
                }
              });
            }
            
            if (scope.mapping && scope.mapping.dpcMapping) {
              ctxNames = scope.mapping.dpcMapping;
              Object.keys(ctxNames).forEach(function(ctxName) {
                var dpcObj = ctxNames[ctxName];
                var dpcName = '';
                var normalized_flags = SWACPermissions.None;

                if (typeof dpcObj === 'object') {
                  dpcName = dpcObj.dpc;
                }else {
                  dpcName = dpcObj;
                }
                for (var k in dpcList) {
                  if (dpcList[k].name === dpcName) {
                    flags = dpcList[k].flags;
                    normalized_flags = flags;

                    if (dpcObj.denyFlags && typeof dpcObj.denyFlags === 'string') {
                      if (dpcObj.denyFlags.toLowerCase().indexOf('r') !== -1) {
                        flags ^= SWACPermissions.Read;  //eslint-disable-line no-bitwise
                      }
                      if (dpcObj.denyFlags.toLowerCase().indexOf('w') !== -1) {
                        flags ^= SWACPermissions.Write;  //eslint-disable-line no-bitwise
                      }
                      normalized_flags = flags;
                    }

                    if ((flags & SWACPermissions.Read) && (flags & SWACPermissions.Write)) {  //eslint-disable-line no-bitwise
                      if (dpcObj.initMode && typeof dpcObj.initMode === 'string') {
                        if (dpcObj.initMode.toLowerCase().indexOf('w') !== -1) {
                          flags = SWACPermissions.Write;
                        } 
                        if (dpcObj.initMode.toLowerCase().indexOf('r') !== -1) {
                          flags = SWACPermissions.Read;
                        }   
                      }

                      if ((flags & SWACPermissions.Read) && (flags & SWACPermissions.Write)){ //eslint-disable-line no-bitwise
                        flags = initMode;
                      }
                    }
                    
                    break;
                  }
                }
    
                tmpPromise = _manageCtxDpcInitialValue(dpcName, ctxName, flags);
                if (tmpPromise) {
                  manageCtxPromises.push(tmpPromise);
                }
                localCtxDpcRep[ctxName] = { dpc: dpcName, flags: normalized_flags };
              });
            }

            $q.all(manageCtxPromises).then(function() {
              if (cmpInstance && cmpInstance.hasUI()) {
                scope.swacNoShow = (typeof scope.noShow !== 'undefined') ? scope.noShow : false;
              }
              eventBus.publish(cmpInstance.name() + '.onReady', { name: cmpInstance.name() });
            });
          }

          /**
           * function for update dpc
           * @param {object} data event param
           */
          function contextUpdateManagement(data) {
            if (typeof localCtxDpcRep[data.name] !== 'undefined' && (inSet.indexOf(localCtxDpcRep[data.name].dpc) === -1)) {
              var dpcName = localCtxDpcRep[data.name].dpc;
              var node = cmpInstance.dpc.open(dpcName);
              if ((localCtxDpcRep[data.name].flags & SWACPermissions.Write) && node && typeof node.beginSet === 'function') {  //eslint-disable-line no-bitwise
                if (inCtxSet.length > 0) {
                  var ctxList = _findCtx(localCtxDpcRep[data.name].dpc);
                  if (ctxList.length > 0) {
                    ctxList.forEach(function(ctxName) {
                      if (inCtxSet.indexOf(ctxName) > -1) {
                        inCtxSet.splice(inCtxSet.indexOf(ctxName), 1);
                      }
                    });
                  }
                }

                if (inCtxSet.indexOf(data.name) === -1) {
                  inCtxSet.push(data.name);
                }
                node.beginSet(data.value).then(function(valueObj) {
                  if (valueObj.modified) {
                    $timeout(function() {
                      inSet.push(dpcName);
                      ctxService.updateCtx(data.name, valueObj.data);
                      inCtxSet.splice(inCtxSet.indexOf(data.name), 1);
                      inSet.splice(inSet.indexOf(dpcName), 1);
                    });
                  }else {
                    inCtxSet.splice(inCtxSet.indexOf(data.name), 1);
                  }
                }, function() {
                  if(node.beginGet){
                    node.beginGet().then(function(value) {
                      $timeout(function() {
                        inSet.push(dpcName);
                        ctxService.updateCtx(data.name, value);
                        inCtxSet.splice(inCtxSet.indexOf(data.name), 1);
                        inSet.splice(inSet.indexOf(dpcName), 1);
                      });
                    });
                  }
                });
              }
            }
          }

          contextRegisterSub = eventBus.subscribe('appCtx.register', function(data) {
            contextUpdateManagement(data);
          });

          contextUpdateSub = eventBus.subscribe('appCtx.update', function(data) {
            contextUpdateManagement(data);
          });

          scope.$on('$destroy', function() {
            if (contextUpdateSub) {
              eventBus.unsubscribe(contextUpdateSub);
            }
            if (contextRegisterSub) {
              eventBus.unsubscribe(contextRegisterSub);
            }
            if (onValueChangedClbk) {
              cmpInstance.dpc.onValueChanged.unsubscribe(onValueChangedClbk);
            }
            if (onAddedClbk) {
              cmpInstance.dpc.onAdded.unsubscribe(onAddedClbk);
            }
            if (onRemovedClbk) {
              cmpInstance.dpc.onRemoved.unsubscribe(onRemovedClbk);
            }
          });

          scope.$on('componentEvents', function(event, data) {
            eventBus.publish(data.name, data.data);
          });

          scope.$on('onCreated', function(event, data) {
            eventBus.publish(data.name + '.onCreated', data);
          });

          scope.$on('onFailure', function(event, data) {
            eventBus.publish(data.name + '.onFailure', data);
          });

          scope.$on('onRemoved', function(event, data) {
            eventBus.publish(data.name + '.onRemoved', data);
          });

          scope.onReadyCallback = function(cmp) {
            cmpInstance = cmp;
            cmpInstance._internal.iframe.getIFrame().style.position = 'relative';

            if (cmpInstance.dpc && cmpInstance.dpc.onValueChanged) {
              cmpInstance.dpc.onValueChanged.subscribe(onValueChangedClbk = function(evt) {
                var ctxList = _findCtx(evt.data.key);
                if (ctxList.length > 0) {
                  ctxList.forEach(function(ctxChangedName) {
                    if ((inCtxSet.indexOf(ctxChangedName) === -1) && (localCtxDpcRep[ctxChangedName].flags & SWACPermissions.Read) &&  //eslint-disable-line no-bitwise
                      cmpInstance.dpc.open(evt.data.key) && cmpInstance.dpc.open(evt.data.key).beginGet) {
                      cmpInstance.dpc.open(evt.data.key).beginGet().then(function(value) {
                        $timeout(function() {
                          inSet.push(evt.data.key);
                          ctxService.updateCtx(ctxChangedName, value);
                          inSet.splice(inSet.indexOf(evt.data.key), 1);
                        });
                      });
                    }
                  });
                }
              });
            }

            _manageCtx();

            cmpInstance.dpc.onAdded.subscribe(onAddedClbk = function(evt) {
              if (evt.data.node === 'node') {
                return;
              }

              var dpcElem = cmpInstance.dpc.open(evt.data.key);
              if (!dpcElem) {
                return;
              }

              if (shareCtx) {
                var ctxPrefix = '';
                var flags = SWACPermissions.None;
                if (typeof shareCtx === 'string' && shareCtx !== 'true') {
                  ctxPrefix = shareCtx + '.';
                }
                if ((evt.data.flags & SWACPermissions.Write)) {  //eslint-disable-line no-bitwise
                  flags = flags || SWACPermissions.Write;
                }
                if ((evt.data.flags & SWACPermissions.Read)) {  //eslint-disable-line no-bitwise
                  flags = flags || SWACPermissions.Read;
                }
                localCtxDpcRep[ctxPrefix + evt.data.key] = { dpc: evt.data.key, flags: flags };
                
              }

              var ctxList = _findCtx(evt.data.key);
              if (ctxList.length > 0) {
                ctxList.forEach(function(ctxAddName) {
                  var dpcObj = null;
                  var denyFlags = SWACPermissions.None;
                  var initialFlag = dpcElem.flags();
                  localCtxDpcRep[ctxAddName].flags = dpcElem.flags();
                  if (ctxNames && (dpcObj = ctxNames[ctxAddName])) {
                    if (typeof dpcObj === 'object' && dpcObj.denyFlags) {
                      if (dpcObj.denyFlags.toLowerCase().indexOf('r') !== -1) {
                        denyFlags |= SWACPermissions.Read;  //eslint-disable-line no-bitwise
                      }
                      if (dpcObj.denyFlags.toLowerCase().indexOf('w') !== -1) {
                        denyFlags |= SWACPermissions.Write;  //eslint-disable-line no-bitwise
                      }
                      if ((dpcElem.flags() & SWACPermissions.Read) && (denyFlags & SWACPermissions.Read)) {  //eslint-disable-line no-bitwise
                        localCtxDpcRep[ctxAddName].flags ^= SWACPermissions.Read;  //eslint-disable-line no-bitwise
                      }
                      if ((dpcElem.flags() & SWACPermissions.Write) && (denyFlags & SWACPermissions.Write)) {  //eslint-disable-line no-bitwise
                        localCtxDpcRep[ctxAddName].flags ^= SWACPermissions.Write;  //eslint-disable-line no-bitwise
                      }
                      initialFlag = localCtxDpcRep[ctxAddName].flags;
                    }
                  }

                  if ((initialFlag & SWACPermissions.Read) && (initialFlag & SWACPermissions.Write)) {  //eslint-disable-line no-bitwise
                    initialFlag = initMode;
                  }

                  if (inCtxSet.indexOf(ctxAddName) === -1) {
                    if((initialFlag & SWACPermissions.Write) && dpcElem.beginSet && (typeof ctxService.getCtx(ctxAddName) !== 'undefined')){ //eslint-disable-line no-bitwise
                      inSet.push(evt.data.key);
                      inCtxSet.push(ctxAddName);
                      dpcElem.beginSet(ctxService.getCtx(ctxAddName)).then(function(valueObj){
                        if (valueObj.modified) {
                          $timeout(function() {
                            ctxService.updateCtx(ctxAddName, valueObj.data);
                            inSet.splice(inSet.indexOf(evt.data.key), 1);
                            inCtxSet.splice(inCtxSet.indexOf(ctxAddName), 1);
                          });
                        }else {
                          inSet.splice(inSet.indexOf(ctxAddName), 1);
                          inCtxSet.splice(inCtxSet.indexOf(ctxAddName), 1);
                        }
                      },function(){
                        if(dpcElem.beginGet){
                          dpcElem.beginGet().then(function(value) {
                            $timeout(function() {
                              ctxService.updateCtx(ctxAddName, value);
                              inSet.splice(inSet.indexOf(evt.data.key), 1);
                              inCtxSet.splice(inCtxSet.indexOf(ctxAddName), 1);
                            });
                          });
                        }
                      });
                    }
                  }
                });
              }
            });

            cmpInstance.dpc.onRemoved.subscribe(onRemovedClbk = function(evt) {
              var ctxPrefix = '';
              var ctxList = _findCtx(evt.data.key);

              if (ctxList.length > 0) {
                if (shareCtx) {
                  if ((typeof shareCtx === 'string') && (shareCtx !== 'true')) {
                    ctxPrefix = shareCtx + '.';
                  }
                }

                ctxList.forEach(function(ctxRemoveName) {
                  if (ctxRemoveName && typeof ctxService.getCtx(ctxRemoveName) !== 'undefined') {
                    $timeout(function() {
                      ctxService.unRegisterCtx(ctxRemoveName);
                      if (ctxRemoveName === (ctxPrefix + evt.data.key)) {
                        delete localCtxDpcRep[ctxPrefix + evt.data.key];
                      }
                    });
                  }
                });
              }
            });
          };

          if (scope.settings) {
            if (typeof scope.settings.width !== 'undefined') {
              componentInfo.settings.width = scope.settings.width;
            }
            if (typeof scope.settings.height !== 'undefined') {
              componentInfo.settings.height = scope.settings.height;
            }
            if (typeof scope.settings.top !== 'undefined') {
              componentInfo.settings.top = scope.settings.top;
            }
            if (typeof scope.settings.left !== 'undefined') {
              componentInfo.settings.left = scope.settings.left;
            }
            if (typeof scope.settings.flavor !== 'undefined') {
              componentInfo.settings.flavor = scope.settings.flavor;
            }
            if (typeof scope.settings.designMode !== 'undefined') {
              componentInfo.settings.designMode = scope.settings.designMode;
            }
            if (typeof scope.settings.addDpcValues !== 'undefined') {
              componentInfo.settings.addDpcValues = scope.settings.addDpcValues;
            }
            if (typeof scope.settings.activateRemoveOnRedirect !== 'undefined') {
              componentInfo.settings.activateRemoveOnRedirect = scope.settings.activateRemoveOnRedirect;
            }
          }

          cfgSrv.getCfg('afx-swac-config').then(function(data) {
            if (data.baseLib) {
              scope.baseLib = data.baseLib;
            }
            if (data.extensions) {
              scope.extensions = data.extensions;
            }
            scope.swacComponentConfiguration = componentInfo;
          });

             scope.$watch('noShow', function (val) {
            if (cmpInstance && cmpInstance.hasUI()) {
              scope.swacNoShow = val;
            }
          });

        }]
      };
    }]);
});
