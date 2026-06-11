/*global
 define
 */

/**
 * @module js/swac/swac-boot.service
 */
define(['app', 'js/declUtils', 'js/eventBus', '@swac/base', '@swac/boot', 'js/appCtxService'],
function(app, declUtils, eventBus, SWACKit, SWACBoot) {
  'use strict';

  var dpcRoot = null;
  var bAlreadyBoot = false;

  var SWAC = new SWACKit();

  var exports = {}; 
  var _hub = null;
  var ngDefer = null;

  var actionServiceCache = { //cache dynamic loaded service for inputFunction
  };

  var _$parse = null;
  var _$timeout = null;

  app.factory('swacBootService', ['appCtxService', '$state', '$q', '$parse', '$timeout', function(appCtxSvc, $state, $q, $parse, $timeout) {
    _$parse = $parse;
    _$timeout = $timeout;
    ngDefer = $q.defer();

    /**
    * gets the service
    * @param {string} servicePath path of the service
    * @param {boolean} synchMode synch mode
    * @returns {object} the service
    */
    function getService(servicePath, synchMode) { //cache services
      var sN = servicePath.replace(/\//g, '.');
      if (sN in actionServiceCache) {
        if (typeof synchMode === 'boolean' && synchMode) {
          return actionServiceCache[sN].instance;
        } 
        return actionServiceCache[sN].promise;
      }else if (typeof synchMode === 'boolean' && synchMode) {
        return;
      }
      var currService = {
        path: servicePath,
        promise: declUtils.loadDependentModule(servicePath, $q, app.getInjector())
      };
      actionServiceCache[sN] = currService;
      return actionServiceCache[sN].promise.then(function(service) {
        currService.instance = service; //store instance for sincronous call
        return service;
      }, function(reason) {
        currService.reason = reason;
        throw reason;
      });
    }

    /**
    * should be called as soon as possible to fill the service cache
    * @param {string} servicePath path of the service
    * @returns {object} service instance
    */
    function loadService(servicePath) {
      return getService(servicePath);
    }

    /**
    * synchronous service method call
    * @param {string} servicePath path of the service
    * @param {function} method method to call
    * @param {object} params params of the function
    * @returns {function} the service call
    */
    function synchServiceCaller(servicePath, method, params) {
      var service = getService(servicePath, true);
      if (service) {
        if (typeof service[method] === 'function') {
          return service[method].apply(null, [params]);
        }
      }
    }

    /**
    *  parse expression 
    * usage:
    * parseExpression('data.name.length',{data:{name:"gigi"}}) return 4
    *  used in rejectExpression to obtain a boolean value
    * @param {function} exp expression to parse
    * @param {object} extraContext additional context
    * @returns {function} expression condition
    */
    function parseExpression(exp, extraContext) {
      var exprCondition = _$parse(exp);
      var ctx = {};
      if (typeof extraContext === 'object' && extraContext !== null) {
        Object.getOwnPropertyNames(extraContext).forEach(function(pName) {
          ctx[pName] = extraContext[pName];
        });
      }
      return exprCondition(ctx);
    }
    
    /**
    * Creates a validator
    * @param {object} conf conf object
    * @returns {function} validation function
    */ 
    function createValidatorFunction(conf) {
      if ('validatorDep' in conf) {
        loadService(conf.validatorDep);
      }
      return function(validatorEventParams) {
        if ('rejectExpression' in conf && !parseExpression(conf.rejectExpression, {
          value: validatorEventParams.data.value
        })) {
          validatorEventParams.reject = true;
        }else if ('validatorMethod' in conf && 'validatorDep' in conf) {
          //IMPORTANT: synchServiceCaller do nothing if the service is still not loaded
          synchServiceCaller(conf.validatorDep, conf.validatorMethod, validatorEventParams);
        }
      };
    }

    exports.boot = function(bootConf) {
      var _appCtxSvc = appCtxSvc;
      var swacConf = null;
      var cmpInterface = {};
      var bootReady = $q.defer();
      var inUpdate = [];
      var localCtxDpcRep = {};
      var bGoOn = false;
      var settings = {
        version: (typeof SWAC !== 'undefined' && typeof SWAC.version !== 'undefined') ? SWAC.version : '1.0.0',
        auth: 'no',
        extensions: [],
        timeout: 3000
      };

      if (bAlreadyBoot) {
        ngDefer.reject('boot already done');
        return ngDefer.promise;
      }

    /**
    * 
    * @param {string} dpcName dpc name
    * @param {object} value  value of dpc
    */
    function dpcUpdateManagement(dpcName, value) {
      var elem = swacConf.dpcCtxMap[dpcName];
      var _flags = SWAC.DPC.Permissions.Read | SWAC.DPC.Permissions.Write;  //eslint-disable-line no-bitwise
      if (value) {
        if (typeof elem.flags === 'string') {
          if (elem.flags.toLowerCase() === 'none') {
            _flags = SWAC.DPC.Permissions.None;
          }else if ((elem.flags.toLowerCase().indexOf('rw') === -1) && (elem.flags.toLowerCase().indexOf('wr') === -1)) {
            if (elem.flags.toLowerCase().indexOf('r') !== -1) {
              _flags = SWAC.DPC.Permissions.Read;
            }
            if (elem.flags.toLowerCase().indexOf('w') !== -1) {
              _flags = SWAC.DPC.Permissions.Write;
            }
          }
        }
        dpcRoot.set(dpcName, value, elem.type, _flags); // create dpc
      }

      if (dpcName && (dpcRoot.open(dpcName)) && dpcRoot.open(dpcName).onBeforeValueChanged) {
        if (elem.validator && elem.validator.method && elem.validator.deps) {
          dpcRoot.open(dpcName).onBeforeValueChanged.subscribe(createValidatorFunction({ ctx: elem.ctx, validatorMethod: elem.validator.method, validatorDep: elem.validator.deps }));
        }else if (elem.rejectExpression) {
          dpcRoot.open(dpcName).onBeforeValueChanged.subscribe(createValidatorFunction({ ctx: elem.ctx, rejectExpression: elem.rejectExpression }));
        }
      }
    }

    /**
     * dpcManagement
     */
    function dpcManagement() {
        cmpInterface.dpc = {
            structure: []
          };
        cmpInterface.dpc.bind = function(root) {
          dpcRoot = root;

          var contextRegister = function(data) {
            var dpcList = null;
            var dpcName = null;

            if (dpcRoot === null) {
              return;
            }

            if (localCtxDpcRep[data.name] && (dpcList = localCtxDpcRep[data.name]).length > 0) {
              for (var i = 0; i < dpcList.length; i++) {
                dpcName = dpcList[i];
                if (typeof data.value !== 'undefined') {
                  dpcUpdateManagement(dpcName, data.value);
                }else {
                  dpcRoot.remove(dpcName);
                }
              }
            }
          };

          dpcRoot.onValueChanged.subscribe(function(eventData) {
            if (inUpdate.indexOf(eventData.data.key) === -1) {
              if (swacConf.dpcCtxMap.hasOwnProperty(eventData.data.key)) {
                _$timeout(function() {
                  inUpdate.push(eventData.data.key);
                  _appCtxSvc.updateCtx(swacConf.dpcCtxMap[eventData.data.key].ctx, eventData.data.value);
                });
              }
            } else {
              inUpdate.splice(inUpdate.indexOf(eventData.data.key), 1);
            }
          });

          for (var dpcName in swacConf.dpcCtxMap) {
            if (swacConf.dpcCtxMap.hasOwnProperty(dpcName)) {
              dpcUpdateManagement(dpcName, _appCtxSvc.getCtx(swacConf.dpcCtxMap[dpcName].ctx));
            }
          }

          eventBus.subscribe('appCtx.update', function(data) {
            var dpcName = null;
            var dpcList = null;
            if (dpcRoot === null) {
              return;
            }

            if (localCtxDpcRep[data.name] && (dpcList = localCtxDpcRep[data.name]).length > 0) {
              for (var i = 0; i < dpcList.length; i++) {
                dpcName = dpcList[i];
                if (inUpdate.indexOf(dpcName) === -1) {
                  var node = dpcRoot.open(dpcName);
                  if (node) {
                    if (typeof node.beginSet === 'function') {
                      inUpdate.push(dpcName);
                      node.beginSet(data.value).then(
                      function(value) {
                        if (value.modified) {
                          _$timeout(function() {
                            _appCtxSvc.updateCtx(data.name, value.data);
                            inUpdate.splice(inUpdate.indexOf(dpcName), 1);
                          });
                        }else {
                          inUpdate.splice(inUpdate.indexOf(dpcName), 1);
                        }
                      },
                      function() {
                        if (typeof node.beginGet === 'function') {
                          node.beginGet().then(function(value) {
                            _$timeout(function() {
                              _appCtxSvc.updateCtx(data.name, value);
                              inUpdate.splice(inUpdate.indexOf(dpcName), 1);
                            });
                          }, function() {
                            inUpdate.splice(inUpdate.indexOf(dpcName), 1);
                          });
                        }else {
                          inUpdate.splice(inUpdate.indexOf(dpcName), 1);
                        }
                      });
                    }else {
                      inUpdate.splice(inUpdate.indexOf(dpcName), 1);
                    }
                  }else {
                    dpcUpdateManagement(dpcName, data.value);
                  }
                }else {
                  inUpdate.splice(inUpdate.indexOf(dpcName), 1);
                }
              }
            }else {
              // context created
              contextRegister(data);
            }
          });

          eventBus.subscribe('appCtx.register', function(data) {
            contextRegister(data);
          });
        };

        for (var dpcName in swacConf.dpcCtxMap) {
          if (swacConf.dpcCtxMap.hasOwnProperty(dpcName)) {
            if (!swacConf.dpcCtxMap[dpcName].dynamic) {
              cmpInterface.dpc.structure.push({
                key: dpcName,
                value: swacConf.dpcCtxMap[dpcName].value || '',
                type: swacConf.dpcCtxMap[dpcName].type || '',
                flags: swacConf.dpcCtxMap[dpcName].flags || 'rw'
              });
            }
            var ctName = swacConf.dpcCtxMap[dpcName].ctx;
            if (localCtxDpcRep[ctName]) { // context with more dpc
              var dpcArray = localCtxDpcRep[ctName];
              dpcArray.push(dpcName);
              localCtxDpcRep[ctName] = dpcArray;
            }else { //context not already inserted
              localCtxDpcRep[ctName] = [dpcName];
            }
          }
        }
    }
    /**
    * Manage Event Metod and interface mapping
    */
    function eventInterfaceManagement() {
      var serviceRequestPromises = [];
      var events = [];
      var eventSubObject = [];
      var actionObj = null;
      var reqPromise = null;

      if (typeof swacConf.contracts.eventMap === 'object') {
        events = Object.getOwnPropertyNames(swacConf.contracts.eventMap);
        events.forEach(function(evName) {
          var swacEvent = new SWAC.Eventing.Publisher(evName);
          cmpInterface[evName] = swacEvent.event;
          eventSubObject.push(eventBus.subscribe(swacConf.contracts.eventMap[evName], function(data) {
            swacEvent.notify(data, true);
          }));
        });
      }

      if (typeof swacConf.contracts.methodMap === 'object' && !Array.isArray(swacConf.contracts.methodMap)) {
        Object.getOwnPropertyNames(swacConf.contracts.methodMap).forEach(function(methodName) {

          if( typeof swacConf.contracts.methodMap[methodName] === 'object' ){
          reqPromise = declUtils.loadDependentModule(swacConf.contracts.methodMap[methodName].deps, $q, app.getInjector());
          serviceRequestPromises.push(reqPromise);
          reqPromise.then(function(ss) {
            if (ss.hasOwnProperty(swacConf.contracts.methodMap[methodName].name) &&
            typeof ss[swacConf.contracts.methodMap[methodName].name] === 'function') {
              cmpInterface[methodName] = ss[swacConf.contracts.methodMap[methodName].name];
              cmpInterface[methodName].bind(ss);
            }
          });
        }else{
            if(bootConf.viewModel && (actionObj = bootConf.viewModel.getAction(swacConf.contracts.methodMap[methodName]))){
              reqPromise = declUtils.loadDependentModule(actionObj.deps, $q, app.getInjector());
              serviceRequestPromises.push(reqPromise);
              reqPromise.then(function(ss) {
                if (ss.hasOwnProperty(actionObj.method) &&
                typeof ss[actionObj.method] === 'function') {
                  cmpInterface[methodName] = ss[actionObj.method];
                  cmpInterface[methodName].bind(ss);
                }
              });
            }
          }
        });
      }else if (Array.isArray(swacConf.contracts.methodMap)) {
        swacConf.contracts.methodMap.forEach(function(deps) {
          var reqPromise = declUtils.loadDependentModule(deps, $q, app.getInjector());
          serviceRequestPromises.push(reqPromise);
          reqPromise.then(function(ss) {
            for (var methodName in ss) {
              if (typeof ss[methodName] === 'function') {
                cmpInterface[methodName] = ss[methodName];
                cmpInterface[methodName].bind(ss);
              }
            }
          });
        });
      }

      if (typeof swacConf.contracts.interfaces === 'object') {
        cmpInterface.interfaces = {};
        Object.getOwnPropertyNames(swacConf.contracts.interfaces).forEach(function(intfName) {
          //event
          Object.getOwnPropertyNames(swacConf.contracts.interfaces[intfName].eventMap).forEach(function(evName) {
            var swacEvent = new SWAC.Eventing.Publisher(evName);

            if (!cmpInterface.interfaces[intfName]) {
              cmpInterface.interfaces[intfName] = {};
            }
            cmpInterface.interfaces[intfName][evName] = swacEvent.event;
            eventSubObject.push(eventBus.subscribe(swacConf.contracts.interfaces[intfName].eventMap[evName], function(data) {
              swacEvent.notify(data, true);
            }));
          });

         // methods
          if (typeof swacConf.contracts.interfaces[intfName].methodMap === 'object' && !Array.isArray(swacConf.contracts.interfaces[intfName].methodMap)) {
            Object.getOwnPropertyNames(swacConf.contracts.interfaces[intfName].methodMap).forEach(function(methodName) {
              var reqPromise = declUtils.loadDependentModule(swacConf.contracts.interfaces[intfName].methodMap[methodName].deps, $q, app.getInjector());
              serviceRequestPromises.push(reqPromise);
              reqPromise.then(function(ss) {
                if (ss.hasOwnProperty(swacConf.contracts.interfaces[intfName].methodMap[methodName].name) &&
                typeof ss[swacConf.contracts.interfaces[intfName].methodMap[methodName].name] === 'function') {
                  if (!cmpInterface.interfaces[intfName]) {
                    cmpInterface.interfaces[intfName] = {};
                  }
                  cmpInterface.interfaces[intfName][methodName] = ss[swacConf.contracts.interfaces[intfName].methodMap[methodName].name];
                  cmpInterface.interfaces[intfName][methodName].bind(ss);
                }
              });
            });
          }else if (Array.isArray(swacConf.contracts.interfaces[intfName].methodMap)) {
            swacConf.contracts.interfaces[intfName].methodMap.forEach(function(deps) {
              var reqPromise = declUtils.loadDependentModule(deps, $q, app.getInjector());
              serviceRequestPromises.push(reqPromise);
              reqPromise.then(function(ss) {
                for (var methodName in ss) {
                  if (typeof ss[methodName] === 'function') {
                    if (!cmpInterface.interfaces[intfName]) {
                      cmpInterface.interfaces[intfName] = {};
                    }
                    cmpInterface.interfaces[intfName][methodName] = ss[methodName];
                    cmpInterface.interfaces[intfName][methodName].bind(ss);
                  }
                }
              });
            });
          }  
        });
      }
      $q.all(serviceRequestPromises).then(function() {
        bootReady.resolve();
      });
    }

      if (!bootConf || typeof bootConf !== 'object' || typeof bootConf.viewModel === 'object') {
        if (!bootConf) { //page
          if (typeof $state.current.data === 'object') {
            swacConf = $state.current.data.swacBootInfo;
            if (!$state.current.data.swacBootID) {
              bGoOn = true;
            }
          }else {
            bGoOn = true;
          }
        }else if (bootConf && typeof bootConf.viewModel === 'object') { //view
          swacConf = bootConf.viewModel.swacBootInfo;
          if(swacConf && typeof $state.current.data.swacBootID !== 'undefined' && typeof swacConf.bootID !== 'undefined'){
            bGoOn = $state.current.data.swacBootID === swacConf.bootID;
          }
          if(!bGoOn && typeof $state.current.data.swacBootID === 'undefined'){
            bGoOn = $state.current.view === bootConf.viewModel.getPanelId();
          }
        }
        if (swacConf && bGoOn) {
          if (typeof swacConf.settings === 'object') {
            if (swacConf.settings.version) {
              settings.version = swacConf.settings.version;
            }
            if (swacConf.settings.auth) {
              settings.auth = swacConf.settings.auth
            }
            if (swacConf.settings.extensions) {
              settings.extensions = swacConf.settings.extensions;
            }
            if (swacConf.settings.timeout) {
              settings.timeout = swacConf.settings.timeout;
            }
          }
          if (typeof swacConf.dpcCtxMap === 'object') {
            dpcManagement();
          }
          if (typeof swacConf.contracts === 'object') {
            eventInterfaceManagement();
          }else {
            bootReady.resolve();
          }
        }else if(bGoOn){
          bootReady.resolve(); // empty configuration only swac service loaded
        }
      }else if (typeof bootConf.contracts === 'object') {
        cmpInterface = bootConf.contracts;
        bootReady.resolve();
      }

      bootReady.promise.then(function() {
        if (cmpInterface && !bAlreadyBoot) {
          bAlreadyBoot = true;
          SWACBoot.start(function(info) {
            _hub = typeof info.SWAC !== 'undefined' ? new info.SWAC.Hub(cmpInterface) : new SWAC.Hub(cmpInterface);       
            _hub.beginExpose().then(function(){
              ngDefer.resolve("beginExpose succesfully executed");
            }, function(reason){
              ngDefer.reject(reason);
            });
          },
          function(reason){
            ngDefer.reject(reason);
          }, settings.version, settings.auth, settings.extensions, settings.timeout);
        }else{
          ngDefer.reject('boot Start skipped');
        }
      });
      return ngDefer.promise;
    };
    return exports;
  }]);

  return {
    moduleServiceNameToInject: 'swacBootService'
  };
});
