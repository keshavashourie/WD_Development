/*global
 define
 */

/**
 * @module js/swac/swac.directive
 */
define(['app', '@swac/container'],
function(app, SWACKit) {
  'use strict';

  var SWAC = new SWACKit();

  if (SWAC.Config.TimeOuts.Create < 10000) {
    SWAC.Config.TimeOuts.Create = 10000;
  }

  var alreadySubscribed = false;

  app.directive('swac', ['$q', function($q) {
    return {
      restrict: 'E',
      scope: {
        onReadyCallback: '&',
        onErrorCallback: '&',
        configuration: '=',
        mapping: '=',
        baseLib: '=',
        extensions: '=',
        noShow: '=',
        shareEvent: '='
      },
      template: '',
      controller: ['$scope', function(scope) {
        var cmpStarted = false;
        var cmpInstance = null;
        var clbkList = {};
        var interfacesList = {};
        var interfaceRequestPromises = [];
        var shareEvent = scope.shareEvent;

        if (!alreadySubscribed) {
          alreadySubscribed = true;

          SWAC.Container.onCreated.subscribe(function(event) {
            scope.$emit('onCreated', event.data);
          });

          SWAC.Container.onFailure.subscribe(function(event) {
            scope.$emit('onFailure', event.data);
          });

          SWAC.Container.onRemoved.subscribe(function(event) {
            scope.$emit('onRemoved', event.data);
          });
        }

        /**
         * private
         * _subscribeOnEvents function for subscribing on component event
         */
        function _subscribeOnEvents() {

          if (scope.mapping && scope.mapping.eventMapping) {
            var evtRep = scope.mapping.eventMapping;
            Object.getOwnPropertyNames(evtRep).forEach(function(evtName) {
              var tmpElem = null;
              if (cmpInstance.proxy[evtName]) {
                tmpElem = evtRep[evtName];
                if (!Array.isArray(tmpElem)) {
                  tmpElem = [tmpElem];
                }
                tmpElem.forEach(function(elem) {
                  cmpInstance.proxy[evtName].subscribe(clbkList[evtName + '_' + elem] = function(evtData) {
                    var event = {
                      name: elem, data: evtData
                    };
                    scope.$emit('componentEvents', event);
                  });
                });
              }else if (evtName.lastIndexOf('.') !== -1) {
                var pos = evtName.lastIndexOf('.');
                var infName = evtName.substring(0, pos);
                var infEvtName = evtName.substring(pos + 1);
                if (interfacesList[infName]) {
                  tmpElem = evtRep[evtName];
                  if (!Array.isArray(tmpElem)) {
                    tmpElem = [tmpElem];
                  }
                  tmpElem.forEach(function(elem) {
                    if (typeof (interfacesList[infName][infEvtName].subscribe) === 'function') {
                      interfacesList[infName][infEvtName].subscribe(clbkList[evtName + '_' + elem] = function(evtData) {
                        var event = {
                          name: elem, data: evtData
                        };
                        scope.$emit('componentEvents', event);
                      });
                    }
                  });
                }
              }
            });
          }

          if (shareEvent) {
            for (var m in cmpInstance.proxy) {
              if (typeof (cmpInstance.proxy[m].subscribe) === 'function') {
                var evtSharedName = (typeof shareEvent === 'string' && shareEvent !== 'true') ? shareEvent + '.' + m : m;
                cmpInstance.proxy[m].subscribe(clbkList['shared' + '_' + m + '_' + evtSharedName] = function(evtData) {
                  var event = {
                    name: evtSharedName, data: evtData
                  };
                  scope.$emit('componentEvents', event);
                });
              }
            }

            for (var t in interfacesList) {
              if (interfacesList.hasOwnProperty(t)) {
                for (var n in interfacesList[t]) {
                  if (typeof (interfacesList[t][n].subscribe) === 'function') {
                    var evtIntSharedName = (typeof shareEvent === 'string') ? shareEvent + '.' + t + '.' + n : t + '.' + n;
                    interfacesList[t][n].subscribe(clbkList['shared' + '_' + t + '.' + n + '_' + evtIntSharedName] = function(evtData) {
                      var event = {
                        name: evtIntSharedName, data: evtData
                      };
                      scope.$emit('componentEvents', event);
                    });
                  }
                }
              }
            }
          }

          if (typeof scope.onReadyCallback === 'function') {
            scope.onReadyCallback({ cmp: cmpInstance });
          }
        }

        /**
         * 
         * @param {Object} conf:any
         * private
         *  startComponent : start the component creation
         */
        function startComponent(conf) {
          if (scope.baseLib !== undefined) {
            SWAC.Config.Container.URLs.BaseLibrary = scope.baseLib;
          }

          if (scope.extensions !== undefined) {
            SWAC.Config.Extensions = scope.extensions;
          }

          SWAC.Container.beginCreate(conf).then(
              function() {
                cmpInstance = SWAC.Container.get({ name: conf.name });

                cmpInstance.onReady.subscribe(clbkList.onReadyclbk = function() {
                 
                  if (scope.mapping && scope.mapping.interfaces) {
                    scope.mapping.interfaces.forEach(function(infName) {
                      if (cmpInstance.interfaces.has(infName)) {
                        var reqPromise = cmpInstance.interfaces.beginGet(infName);
                        interfaceRequestPromises.push(reqPromise);
                        reqPromise.then(function(intf) {
                          interfacesList[infName] = intf;
                        });
                      }
                    });

                    $q.all(interfaceRequestPromises).then(function() {
                      _subscribeOnEvents();
                    });
                  }else{
                    _subscribeOnEvents();
                  }
                });
              },
            function(reason) {
              if (typeof scope.onErrorCallback === 'function') {
                scope.onErrorCallback({ message: reason.message });
              }
            });
        }

        scope.$watch('configuration', function(val) {
          if (!cmpStarted && typeof val === 'object') {
            cmpStarted = true;
            startComponent(val);
          }
        });

        scope.$watch('noShow', function(val) {
          if (cmpInstance && cmpInstance.hasUI()) {
            if (typeof val === 'undefined') {
              val = false;
            }
            cmpInstance.beginShow(!val);
          }
        });

        scope.$on('$destroy', function() {
          if (cmpInstance) {
            cmpInstance.onReady.unsubscribe(clbkList.onReadyclbk);
            delete clbkList.onReadyclbk;

            if (scope.mapping && scope.mapping.eventMapping) {
              var evtRep = scope.mapping.eventMapping;
              Object.getOwnPropertyNames(evtRep).forEach(function(evtName) {
                var tmpElem = null;
                if (cmpInstance.proxy[evtName]) {
                  tmpElem = evtRep[evtName];
                  if (!Array.isArray(tmpElem)) {
                    tmpElem = [tmpElem];
                }
                  for (var i = 0; i < tmpElem.length; i++) {
                    cmpInstance.proxy[evtName].unsubscribe(clbkList[evtName + '_' + tmpElem[i]]);
                    delete clbkList[evtName + '_' + tmpElem[i]];
                }
                }else if (evtName.lastIndexOf('.') !== -1) {
                  var pos = evtName.lastIndexOf('.');
                  var infName = evtName.substring(0, pos);
                  var infEvtName = evtName.substring(pos + 1);
                  if (interfacesList[infName]) {
                    tmpElem = evtRep[evtName];
                    if (!Array.isArray(tmpElem)) {
                      tmpElem = [tmpElem];
                  }
                    tmpElem.forEach(function(elem) {
                      if (typeof (interfacesList[infName][infEvtName].subscribe) === 'function') {
                        interfacesList[infName][infEvtName].unsubscribe(clbkList[evtName + '_' + elem]);
                        delete clbkList[evtName + '_' + elem];
                    }
                  });
                }
              }
});
        }

            if (shareEvent) {
              var evtSharedName = '';
              for (var m in cmpInstance.proxy) {
                if (typeof (cmpInstance.proxy[m].subscribe) === 'function') { // Event
                  evtSharedName = (typeof shareEvent === 'string') ? shareEvent + '.' + m : m;
                  cmpInstance.proxy[m].unsubscribe(clbkList['shared' + m + '_' + evtSharedName]);
                  delete clbkList['shared' + '_' + m + '_' + evtSharedName];
              }
}

              for (var t in interfacesList) {
                if (interfacesList.hasOwnProperty(t)) {
                  for (var n in interfacesList[t]) {
                    if (typeof (interfacesList[t][n].subscribe) === 'function') {
                      evtSharedName = (typeof shareEvent === 'string') ? shareEvent + '.' + t + '.' + n : t + '.' + n;
                      interfacesList[t][n].unsubscribe(clbkList['shared' + '_' + t + '.' + n + '_' + evtSharedName]);
                      delete clbkList['shared' + '_' + t + '.' + n + '_' + evtSharedName];
                  }
                }
              }
}
        }
            interfacesList = {};
            cmpInstance.beginRemove().then(
              function () { },
              function () { }
            );
        }
        });
      }]
    };
  }]);
});
