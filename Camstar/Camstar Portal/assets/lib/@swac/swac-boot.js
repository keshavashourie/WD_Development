/* Version 1.6.2, Boot version 1.6.2, copyright (C) 2021, Siemens AG. All Rights Reserved. */


(function () {
  var swacBootModule = function () {


/**
 * Class that manages the boot of the component.
 *
 * @class SWACBoot
 * @constructor
 * 
 */
var SWACBoot = (function () {
  

  //////////////
  // PRIVATE  //
  //////////////

  var
      //local SWAC object
      SWACInstance = null,

      // Current bootloader phase.
      _state = 'pending',

      _bootStarted = false,

      // Timer for bootloader timeout.
      _bootTimer = -1,

      _userSettings = {
        success: null,
        failure: null,
        version: null,
        auth: null,
        attributes: null,
        timeout: null,
        extensions: null
      },

      _connectionList = [],

      _sharedWorkerConnectionQueue = [],

      _removeItemFromConnectionList = function (name) {
        var nameArray = null;
        var _name = '';

        if (name.indexOf('>_') !== -1) {
          nameArray = JSON.parse(name);
          _name = nameArray[nameArray.length - 1];
        }
        else {
          _name = name;
        }

        for (var i = _connectionList.length - 1; i >= 0; i--) {
          if (JSON.stringify(_connectionList[i].fullname).lastIndexOf(_name) !== -1) {
            _connectionList[i].port.onmessage = null;
            _connectionList[i].port = null;
            _connectionList[i] = null;
            _connectionList.splice(i, 1);
          }
        }
      },

    _checkIfWorker = function (obj) {
      if (((typeof WorkerGlobalScope !== 'undefined') && (obj instanceof WorkerGlobalScope)) ||
          ((typeof DedicatedWorkerGlobalScope !== 'undefined') && (obj instanceof DedicatedWorkerGlobalScope))) {
        return true;
      }
      else {
        return false;
      }
    },

  /**
   * Checks if the specified object is a Shared Worker
   *
   * @private
   * @method _checkIfSharedWorker
   * @param {Object} obj object to check
   * @return {Boolean} Check result
   */
  _checkIfSharedWorker = function (obj) {
    if (((typeof SharedWorkerGlobalScope !== 'undefined') && (obj instanceof SharedWorkerGlobalScope))) {
      return true;
    }
    else {
      return false;
    }
  },

  isWorker = _checkIfWorker(this),
  isSharedWorker = _checkIfSharedWorker(this),

  // First connection promise resolve function
  resolveFP = null,

  firstConnectionPromise = new Promise(function (resolve) {
    resolveFP = resolve;
  }),

  // worker instance or parent window of the iframe component is hosted in.
  _p = isSharedWorker ? null : (isWorker ? this : window.parent),

  _pP = isSharedWorker ? firstConnectionPromise : Promise.resolve(),

  ConnectionObj = function (port) {
    var swPort = port,
      clbkList = [];

    swPort.onmessage = function (e) {
      clbkList.forEach(function (clb) {
        clb(e);
      });
    };

    var _addEventListener = function (msg, handler) {
      if (msg === 'message') {
        clbkList.push(handler);
      }
    };

    var _removeEventListener = function (msg, handler) {
      if (msg === 'message') {
        var idCallB = clbkList.indexOf(handler);
        if (idCallB !== -1) {
          clbkList.splice(idCallB, 1);
        }
      }
    };

    var _detachBootMessageListener = function () {
      clbkList.splice(0, 1);
    };

    var _postMessage = function (msg) {
      swPort.postMessage(msg);
    };

    var thisConnectionObj = {
      addEventListener: _addEventListener,
      removeEventListener: _removeEventListener,
      detachBootMessageListener: _detachBootMessageListener,
      postMessage: _postMessage,
      port: swPort
    };

    return thisConnectionObj;
  },

  // Handler for incoming messages.
  _msgHandler = null,

  // Content with type, name and authentication info
  _content = null,

  // raw content received from container
  _containerInfo = {},

  _injectionQueue = [],

  _extensionsAliasMap = {},

  _configTimeout = null,

  _timeoutEnabled = (function () {
    try {
      var _enabled = window.sessionStorage.getItem('swac.timeouts.enabled');
      return (typeof JSON.parse(_enabled) === 'boolean') ? JSON.parse(_enabled) : true;
    }
    catch (e) {
      return true;
    }
  })(),

  /**
 * Clear timer and remove event listener
 *
 * @private
 * @method _clearTimerAndListener
 */
  _clearTimerAndListener = function () {
    if (_timeoutEnabled) {
      window.clearTimeout(_bootTimer);
    }
    if (isSharedWorker) {
      if (_p) {
        _p.removeEventListener('message', _msgHandler);
      }
    }
    else {
      window.removeEventListener('message', _msgHandler);
    }
  },

  /**
  * Load SWAC base
  *
  * @private
  * @method _loadBase
  * @param {string} url url for the script to load
  */
  _loadBase = function (url) {
    var baselib = '';

    if (isSharedWorker || isWorker) {
      importScripts(url);
    }
    else {
      baselib = document.createElement('script');

      baselib.innerHTML = window.atob(url.split(';base64,')[1]);

      if (document.getElementsByTagName('head')) { //head is an optional tag
        document.getElementsByTagName('head')[0].appendChild(baselib);
        document.getElementsByTagName('head')[0].removeChild(baselib);
      }
      else {
        document.getElementsByTagName('body')[0].appendChild(baselib);
        document.getElementsByTagName('body')[0].removeChild(baselib);
      }
    }
    url = '';
  },

  /**
  * Load a Javascript
  *
  * @private
  * @method _jsLoadUtil
  * @param {string} url url for the script to load
  * @param {function} success function to call when success
  * @param {function} failure function to call when fail
  */
  _jsLoadUtil = function (url, success, failure) {
    var baselib = '',
      _cleanScript,
      _success,
      _failure;

    if (isSharedWorker || isWorker) {
      try {
        importScripts(url);
        if (success) {
          success();
        }
        else {
          throw new Error('Load failure');
        }
      }
      catch (e) {
        if (failure) {
          failure('Failed to load ' + url + ' library');
        }
      }
    }
    else {
      baselib = document.createElement('script');
      _cleanScript = function () {
        baselib.removeAttribute('src');
        if (document.getElementsByTagName('head')) { //head is an optional tag
          document.getElementsByTagName('head')[0].removeChild(baselib);
        }
        else {
          document.getElementsByTagName('body')[0].removeChild(baselib);
        }
        baselib = null;
      };
      _success = function () {
        baselib.removeEventListener('load', _success);
        _cleanScript();
        if (success) {
          success();
        }
      };
      _failure = function () {
        baselib.removeEventListener('error', _failure);
        _cleanScript();
        if (failure) {
          failure('Failed to load ' + url + ' library');
        }
      };

      baselib.setAttribute('type', 'text/javascript');
      baselib.setAttribute('src', url);

      if (success || failure) {
        if (success) {
          baselib.addEventListener('load', _success);
        }
        if (failure) {
          baselib.addEventListener('error', _failure);
        }
      }
      if (document.getElementsByTagName('head')) { //head is an optional tag
        document.getElementsByTagName('head')[0].appendChild(baselib);
      }
      else {
        document.getElementsByTagName('body')[0].appendChild(baselib);
      }
    }
    url = '';
  },

  /**
  * Creates a message
  *
  * @private
  * @method _createMessage
  * @param {object} content content of the message
  * @return {object} a JSON object containing the content of the message
  */
  _createMessage = function (content) {
    return JSON.stringify({ t: 'boot', c: content });
  },

  /**
  * Check if SWAC object is valid
  *
  * @private
  * @method _checkSWAC
  * @return {boolean} SWAC object is valid
  */
  _checkSWAC = function () {
    if (window.SWAC && (typeof SWAC === 'object') && (typeof SWAC.Communication === 'object')) {
      return true;
    }
    else {
      return false;
    }
  },

  /**
  * Receives reply from container
  *
  * @private
  * @method _receiveMessage
  * @param {object} event Event object
  * @param {function} success success callback
  * @param {function} failure failure callback
  * @param {object} currPort message port
  */
  _receiveMessage = function (event, success, failure, currPort) {
    var data = {},
      succCallback,
      failedCb,
      done,
      p,
      regex = null,
      aggregator,
      extension,
      extensionCount = null,
      containerVersion = null,
      bootInstance = null;

    if (_state === 'waiting' || _state === 'ok') {
      if ((typeof (event.data) === 'string') && (event.data.length > 0)) {
        try {
          // Check for URL in response and start loading script
          data = JSON.parse(event.data);
          if (!data || !data.t || !data.c) {
            throw new Error('Incompatible message received');
          }
          else if (data.t !== 'boot') {
            throw new Error('Unknown message received: ' + data.t);
          }
        }
        catch (e) {
          return;
        }

        failedCb = function (reason) {
          _state = 'failed';
          _clearTimerAndListener();
          failure({ message: reason });
          if (isSharedWorker) {
            currPort.postMessage(_createMessage({ message: 'failed' }));
          }
          else {
            swacPostMessage(_createMessage({ message: 'failed' }), '*');
          }
        };

        if ((data.t === 'boot') && (data.c.message === 'pong')) {
          if (typeof data.c.timeouts !== 'undefined') {
            _configTimeout = data.c.timeouts;
          }
          done = function () {
            SWACInstance.isContainer = false;
            _state = 'ok';
            _content = data.c;
            _content.containerVersion = _content.containerVersion || '1.0.0';
            if (isSharedWorker) {
              currPort.postMessage(_createMessage({ message: 'ok' }));
            }
            else {
              swacPostMessage(_createMessage({ message: 'ok' }), '*');
            }
          };

          aggregator = function () {
            var _script,
              injectionQueueCount = _injectionQueue.length,
              extensionObj = {};

            if (typeof SWACInstance.Hub.prototype.Extensions !== 'undefined'
              ) {
              if (typeof SWACInstance.Hub.prototype.Extensions !== 'undefined' && Object.keys(SWACInstance.Hub.prototype.Extensions).length > 0) {
                extensionObj = SWACInstance.Hub.prototype.Extensions;
              }
            }
            else {
              // Old container version, no extensions inject
              done();
              return;
            }

            if (extensionCount !== null) {
                for (var ext in extensionObj) {
                  if (extensionObj.hasOwnProperty(ext)) {
                    if (typeof _extensionsAliasMap[ext] !== 'undefined') {
                      extensionObj[_extensionsAliasMap[ext]] = extensionObj[ext];
                      delete extensionObj[ext];
                      delete _extensionsAliasMap[ext];
                    }
                  }
                }
            }

            if
              (
              (injectionQueueCount === 0)
              || (SWACInstance._internal.Utils.checkVersion(containerVersion, '1.4.1') < 0))
            {
              done();
            } else {
              _state = 'upgrading';
              _script = _injectionQueue.pop();
              if (typeof defineExtension === 'undefined') {
                // Load defineExtension
                _jsLoadUtil(_script, function () {
                  if (typeof defineExtension !== 'undefined') {
                    defineExtension = defineExtension.bind({ SWAC: SWACInstance });
                  }
                  aggregator();
                }, failedCb);
              }
              else {
                  // Load the extension
                  if (_script === '$$unknownExtension$$') {
                    aggregator();
                  }
                  else {
                    extensionCount = Object.keys(extensionObj).length;
                    _jsLoadUtil(_script, aggregator, aggregator);
                  }
              }
            }
          };

          if (data.c.extensions) {
            for (extension in data.c.extensions) {
              if (data.c.extensions.hasOwnProperty(extension)) {
                _injectionQueue.unshift(data.c.extensions[extension]);
              }
            }
          }

          containerVersion = data.c.containerVersion || '1.0.0';

          bootInstance = (typeof SWACBoot !== 'undefined') ? SWACBoot : null;

          if (window.SWAC && (typeof window.SWAC !== 'undefined')) {
            if (_checkSWAC()) {
              SWACInstance = window.SWAC;
              SWACInstance.BootInstance = bootInstance;
              aggregator();
            }
            else {
              failedCb('SWAC is a reserved variable name');
            }
          }
          else {
            if (containerVersion === '1.5.0') {
              window.console.warn('SWAC version 1.5.0 is incompatible with current version');
              failedCb('SWAC version 1.5.0 is incompatible with current version');
              return;
            }

            regex = /^\s+|\s+$/g;

            if ((data.c.url === undefined) || (data.c.url === null) || (data.c.url.replace(regex, '') === '')) {
              failedCb('SWAC.Config.Container.URLs library is empty');
            }
            else {
              succCallback = function () {
                if ((window.SWAC && (typeof window.SWAC !== 'undefined')) || ((typeof data.c.namespace !== 'undefined') && (typeof window[data.c.namespace] !== 'undefined'))) {
                  if (_checkSWAC()) {
                    SWACInstance = window.SWAC;
                    SWACInstance.BootInstance = bootInstance;
                    aggregator();
                  }
                  else {
                    failedCb('SWAC is a reserved variable name');
                  }
                }
                else {
                  failedCb('Failed to load SWAC.Config.Container.URLs library');
                }
              };

              _state = 'upgrading';

              if (data.c.url.lastIndexOf('.js') === (data.c.url.length - 3)) {
                _jsLoadUtil(data.c.url, succCallback, failedCb);
              }
              else {
                _loadBase(data.c.url);
                succCallback();
              }

              data.c.url = '';
            }
            regex = null;
          }
        }
        else if ((data.t === 'boot') && (data.c.message === 'ok2')) {
          // Container object has been created and is ready to be accepted
          _state = 'done';
          _bootStarted = false;

          _clearTimerAndListener();
          for (p in _content) {
            if (_content.hasOwnProperty(p)) {
              _containerInfo[p] = _content[p];
            }
          }

          _content.message = 'SWAC successfully loaded';
          _content.auth = _content.authentication;
          _content.SWAC = (typeof SWACInstance === 'object') ? SWACInstance : new SWACInstance();
          if (_configTimeout) {
            _content.SWAC.Config.TimeOuts = _configTimeout;
          }
          delete _content.authentication;
          delete _content.url;
          delete _content.extensions;
          delete _content.namespace;
          if (!data.c.details) { //introduced in 1.4.0 for fullname 
            _containerInfo['details'] = {
              path: ['<root>']
            };
          }
          else {
            if (isSharedWorker) {
              currPort.fullname = data.c.details.path;
            }
          }

          //merge ok2 message info into containerInfo object
          for (p in data.c) {
            if (p !== 'message' && data.c.hasOwnProperty(p)) {
              if (!_containerInfo.hasOwnProperty(p)) {
                _containerInfo[p] = data.c[p]; //used first for fullname and designMode 18/07/2017
              }
            }
          }

          delete _content._internal;

          SWACInstance.Hub.prototype.containerVersion = _content.containerVersion;

          success(_content);
        }
        else if ((data.t === 'boot') && (data.c.message === 'peng')) {
          _clearTimerAndListener();
          failure({ message: data.c.reason });
        }
      }
    }
  },

  /**
  * Check for alternative communication channel
  *
  * @private
  * @method _checkAlternativeCommunicationChannel
  * @return {function} function for communications
  */
  _checkAlternativeCommunicationChannel = function () {
    if (_p === self) {
      if (_checkIfWorker(_p)) {
        return _p.postMessage;
      }
      else if (typeof swacNative === 'object' && typeof swacNative.postMessage === 'function') {
        return swacNative.postMessage; //chromioum convention
      }
      else if (window.external && typeof window.external.postMessage === 'function') {
        return window.external.postMessage; //IE convention
      }
    }
    return null;
  },

  // Core function for Bootstrap
  _bootstrapCore = function (port, timeout, success, failure, version, auth, attributes) {
    var otherChannel = _checkAlternativeCommunicationChannel(),
      extensionList = [],
      k,
      pingMsgObj,
      pingMsg;

    if (port === self && typeof otherChannel !== 'function' && typeof window['swacPostMessage'] !== 'function') {
      _state = 'failed';
      _containerInfo['details'] = {
        path: ['<root>']
      };
      window.setTimeout(function () {
        failure({ message: 'Component is not embedded into an iframe' });
      }, 0);
    }
    else {
      //check wrong boot phase scenario
      if (window.SWAC && (typeof SWAC === 'object') && (typeof SWAC.Container === 'object')) {
        var cmpL = SWAC.Container.get();
        if (cmpL && (Array.isArray(cmpL) && cmpL.length > 0)) {
          //mixed component with child component present
          SWAC._internal.wrongMixedBoothSequence = true;
          window.console.warn('Mixed component: wrong boot sequence detected');
        }
      }

      if (!isSharedWorker) {
        if (typeof window['swacPostMessage'] !== 'function') {
          if (isWorker) {
            window['swacPostMessage'] = function (message, targetOrigin, transfer) {
              return otherChannel(message);
            };
          }
          else if (typeof otherChannel === 'function') {
            window['swacPostMessage'] = otherChannel;
          }
          else {
            window['swacPostMessage'] = function (message, targetOrigin, transfer) {
              return port.postMessage(message, targetOrigin);
            };
          }
        }
      }

      _msgHandler = function (event) {
        _receiveMessage(event, success, failure, port);
      };

      _state = 'waiting';

      if (isSharedWorker) {
        port.addEventListener('message', _msgHandler, false);
      }
      else {
        window.addEventListener('message', _msgHandler, false);
      }

      if (_timeoutEnabled) {
        _bootTimer = window.setTimeout(function () {
          if (_state !== 'done') {
            _state = 'timedout';
            _containerInfo['details'] = {
              path: ['<root>']
            };
            _clearTimerAndListener();
            failure({ message: 'Bootload sequence timed out' });
          }
        }, timeout);
      }

      if ((arguments[7] !== null) && (typeof arguments[7] !== 'undefined')) {
        // Normalize extension list to support old versions
        for (k = 0; k < arguments[7].length; k++) {
          if (typeof arguments[7][k] === 'object') {
            extensionList.push(arguments[7][k].extension);
            _extensionsAliasMap[arguments[7][k].extension] = arguments[7][k].as;
          }
          else {
            extensionList.push(arguments[7][k]);
          }
        }
      }

      pingMsgObj = { message: 'ping', version: version, authentication: auth, attributes: attributes, extensions: extensionList };
      if (_checkSWAC()) {
        pingMsgObj.swacBaseLoaded = true;
      }
      pingMsg = _createMessage(pingMsgObj);
      if (isSharedWorker) {
        port.postMessage(pingMsg);
      }
      else {
        swacPostMessage(pingMsg, '*'); // targetOrigin is application-specific, use * here.
      }
    }
  },

  _manageConnections = function (deferHub, fakeHub) {
    var onDisconnectedCbk = null;

    deferHub.promise.then(function () {
      _sharedWorkerConnectionQueue.shift();
    }).catch(function (reason) {
      _removeItemFromConnectionList(_containerInfo.name);
      _sharedWorkerConnectionQueue.shift();
      if (JSON.stringify(_containerInfo.details.path).indexOf('<' + _containerInfo.name + '>_') !== -1) {
        _containerInfo = {};
        delete window.SWAC;
      }
    }).finally(function () {
      if (_sharedWorkerConnectionQueue.length > 0) {
        setTimeout(function () {
          /* jshint ignore:start*/
          _executeConnection(_sharedWorkerConnectionQueue[0]);
          /* jshint ignore:end */
        }, 0);
      }
    });

    fakeHub.onDisconnected.subscribe(onDisconnectedCbk = function () {
      _removeItemFromConnectionList(fakeHub.fullname);
      if (_connectionList.length === 0) {
        _connectionList.first = true;
      }
      _p = null;
      fakeHub.onDisconnected.unsubscribe(onDisconnectedCbk);
    });

  },

  _executeConnection = function (e) {
    var conObj = new ConnectionObj(e.ports[0]);

    if (!_connectionList.first && _connectionList.length === 0) {
      _p = conObj;
      if (!firstConnectionPromise.resolved) {
        resolveFP();
        firstConnectionPromise.resolved = true;
      }
      else {
        _state = 'pending';
        _bootstrapCore(conObj, _userSettings.timeout, _userSettings.success, _userSettings.failure, _userSettings.version, _userSettings.auth, _userSettings.attributes, _userSettings.extensions);
      }
    }
    else {
      //start new boot phase
      var success = function () {
        //now emulate hub
        var localHub = new SWAC.Hub(SWAC.Hub.instance._internal.original, conObj),
          _deferHub = new SWAC.Defer(),
          fullname = JSON.stringify(conObj.fullname);

        _deferHub.promise.catch(function (reason) {
          if (reason.reasonCode && (reason.reasonCode === 17015)) { // Bind function is not supported in Shared Worker
            setTimeout(function () {
              throw new SWAC.Exception(3011);
            }, 0);
          }
        });

        var fakeHub = new SWAC.WorkerHub(_deferHub, localHub, conObj, false);
        _manageConnections(_deferHub, fakeHub);

        if (typeof SWAC.Hub.instance._internal.original === 'function') {
          fakeHub.fullname = fullname;
          localHub._internal.setInnerComponent(SWAC.Hub.instance._internal.original(fakeHub));
        }

        fakeHub.services = localHub._internal.services;
        localHub._internal.setHub(fakeHub);

        fakeHub = null;

        localHub.fullname = fullname;

        localHub.beginExpose().then(function () {
          conObj.detachBootMessageListener();
          _deferHub.fulfill(localHub);
          localHub = null;
        }, function (error) {
          _deferHub.reject(error);
          localHub = null;
        });
      };

      var failure = function (reason) {
        conObj = null;
      };

      //call boot start
      _bootstrapCore(conObj, _userSettings.timeout, success, failure, _userSettings.version, _userSettings.auth, _userSettings.attributes, _userSettings.extensions);
    }
    _connectionList.push(conObj);
  },

  // Subscribe to Shared Worker connect event
  _initSharedW = function () {
    this.addEventListener('connect', function (e) {
      if (_sharedWorkerConnectionQueue.length > 0) {
        _sharedWorkerConnectionQueue.push(e);
        return;
      }

      _sharedWorkerConnectionQueue.push(e);

      _executeConnection(e);
    }, false);
  },

  // Initiates the bootstrap mechanism with pre-checks and time-out.
  _bootstrap = function (success, failure, version, auth, timeout) {
    version = version || '*';                     // default is container's current
    timeout = timeout || 1000;                    // default timeout is 1s
    failure = failure || function (event) { };    // do nothing, if no onfailure callback was specified
    auth = auth || 'no';                          // default is no authentication

    var attributes = arguments[5];
    var extensions = arguments[6];

    _userSettings.success = success;
    _userSettings.failure = failure;
    _userSettings.version = version;
    _userSettings.auth = auth;
    _userSettings.attributes = attributes;
    _userSettings.extensions = extensions;
    _userSettings.timeout = timeout;

    if (isSharedWorker || isWorker) {
      window = this;
    }

    if (_state === 'done') {
      window.setTimeout(function () {
        failure({ message: 'Boot phase already done' });
      }, 0);
      return;
    }
    else if (!_bootStarted) {
      _bootStarted = true;
    }
    else {
      window.setTimeout(function () {
        failure({ message: 'Boot phase in progress' });
      }, 0);
      return;
    }

    _pP.then(function () {
      _bootstrapCore(_p, timeout, success, failure, version, auth, attributes, extensions);
    }).catch(function () { });
  };

  firstConnectionPromise.catch(function () { });

  var _bootstrapSWAC = function (success, failure, version, auth, extensions, timeout) {
    var _timeout = timeout,
      _extensions = extensions;

    if (typeof extensions === 'number') {
      _timeout = extensions;
      _extensions = null;
    }
    _bootstrap(success, failure, version, auth, _timeout, null, _extensions);
  };


  if (isSharedWorker) {
    _initSharedW();
  }

  //////////////
  // PUBLIC   //
  //////////////

  return {
    /**
    * Initialize the SWAC component boot phase
    *
    * @method start
    * @param {function} success success callback.
    * @param {function} failure failure callback.
    * @param {string} version SWAC version used to develop the component.
    * @param {string} auth Specify if component require authentication.
    * @param {number} timeout SWACBoot.start timeout in milliseconds.
    */
    start: _bootstrapSWAC,
    
    _internal: {
      /**
      * Container informations.
      *
      * @protected
      * @property _internal.containerInfo
      * @return {object} returns an object containing informations received from the container
      */
      containerInfo: _containerInfo,
      /**
      * Check if an object is a Worker instance
      *
      * @protected
      * @method _internal.checkIfWorker
      * @return {boolean} object is a Worker
      */
      checkIfWorker: _checkIfWorker,
      /**
      * Shared Worker connection list.
      *
      * @protected
      * @property _internal.SWConList
      * @return {Array} returns the connection list
      */
      SWConList: _connectionList,
      manageConnections: _manageConnections
    }
  };

}());

/**
* Provide the version of the object.
*
* @property version
* @type String
*/
SWACBoot.version = '1.6.2';

    return SWACBoot;
  };

  try {
    var _SWACBoot = swacBootModule();

    if (_SWACBoot._internal.checkIfWorker(this)) {
      window = this;
    }

    if (typeof window.SWACBoot === 'undefined') {
      window.SWACBoot = _SWACBoot;
    }

    if ((typeof module !== 'undefined') && (typeof module.exports !== 'undefined')) {
      module.exports = window.SWACBoot;
      module.exports['default'] = module.exports;
    }
    else if ((typeof define === 'function' && (typeof define.amd !== 'undefined')) &&
        (typeof requirejs !== 'undefined') &&
        (typeof requirejs.s.contexts._.config !== 'undefined') &&
        (typeof requirejs.s.contexts._.config.paths['@swac/boot'] !== 'undefined')) {
      define(function () {
        return window.SWACBoot;
      });
    }
    else if ((typeof SystemJS !== 'undefined') && SystemJS.map.hasOwnProperty('@swac/boot')) {
      define(function () {
        return window.SWACBoot;
      });
    }
  }
  catch (e) { }

})();