"use strict";
/*
 * ATTENTION: An "eval-source-map" devtool has been used.
 * This devtool is neither made for production nor for readable output files.
 * It uses "eval()" calls to create a separate source file with attached SourceMaps in the browser devtools.
 * If you are trying to read the output file, select a different devtool (https://webpack.js.org/configuration/devtool/)
 * or disable the default devtool with "devtool: false".
 * If you are looking for production-ready output files, see mode: "production" (https://webpack.js.org/configuration/mode/).
 */
(self["webpackChunkafx_cep"] = self["webpackChunkafx_cep"] || []).push([[61],{

/***/ "./out/site/afx-cep/assets/js/aw.checkAuthentication.controller.js":
/*!*************************************************************************!*\
  !*** ./out/site/afx-cep/assets/js/aw.checkAuthentication.controller.js ***!
  \*************************************************************************/
/***/ ((__unused_webpack_module, __webpack_exports__, __webpack_require__) => {

eval("__webpack_require__.r(__webpack_exports__);\n/* harmony import */ var app__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! app */ \"./out/site/afx-cep/assets/js/adapters/angularjs/appWrapper.js\");\n/* harmony import */ var js_sessionManager_service__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! js/sessionManager.service */ \"./out/site/afx-cep/assets/js/sessionManager.service.js\");\n// Copyright (c) 2020 Siemens\n\n/**\n * This module contains a controller that handles checking authentication\n *\n * @module js/aw.checkAuthentication.controller\n * @class aw.checkAuthentication.controller\n * @memberOf angular_module\n */\n\n\napp__WEBPACK_IMPORTED_MODULE_0__[\"default\"].controller('CheckAuthentication', ['$q', '$scope', '$injector', 'authenticator', 'sessionManagerService', function ($q, $scope, $injector, authenticator, sessionMgr) {\n  if (authenticator) {\n    authenticator.setScope($scope, $injector);\n    sessionMgr.resetPipeLine();\n    authenticator.authenticate($q).then(function () {\n      sessionMgr.authenticationSuccessful();\n    });\n  }\n}]);//# sourceURL=[module]\n//# sourceMappingURL=data:application/json;charset=utf-8;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoiLi9vdXQvc2l0ZS9hZngtY2VwL2Fzc2V0cy9qcy9hdy5jaGVja0F1dGhlbnRpY2F0aW9uLmNvbnRyb2xsZXIuanMuanMiLCJtYXBwaW5ncyI6Ijs7O0FBQUE7O0FBRUE7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBRUE7QUFHQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBIiwic291cmNlcyI6WyJ3ZWJwYWNrOi8vYWZ4LWNlcC8uL291dC9zaXRlL2FmeC1jZXAvYXNzZXRzL2pzL2F3LmNoZWNrQXV0aGVudGljYXRpb24uY29udHJvbGxlci5qcz9iM2JiIl0sInNvdXJjZXNDb250ZW50IjpbIi8vIENvcHlyaWdodCAoYykgMjAyMCBTaWVtZW5zXG5cbi8qKlxuICogVGhpcyBtb2R1bGUgY29udGFpbnMgYSBjb250cm9sbGVyIHRoYXQgaGFuZGxlcyBjaGVja2luZyBhdXRoZW50aWNhdGlvblxuICpcbiAqIEBtb2R1bGUganMvYXcuY2hlY2tBdXRoZW50aWNhdGlvbi5jb250cm9sbGVyXG4gKiBAY2xhc3MgYXcuY2hlY2tBdXRoZW50aWNhdGlvbi5jb250cm9sbGVyXG4gKiBAbWVtYmVyT2YgYW5ndWxhcl9tb2R1bGVcbiAqL1xuaW1wb3J0IGFwcCBmcm9tICdhcHAnO1xuaW1wb3J0ICdqcy9zZXNzaW9uTWFuYWdlci5zZXJ2aWNlJztcblxuYXBwLmNvbnRyb2xsZXIoICdDaGVja0F1dGhlbnRpY2F0aW9uJywgW1xuICAgICckcScsICckc2NvcGUnLCAnJGluamVjdG9yJywgJ2F1dGhlbnRpY2F0b3InLCAnc2Vzc2lvbk1hbmFnZXJTZXJ2aWNlJyxcbiAgICBmdW5jdGlvbiggJHEsICRzY29wZSwgJGluamVjdG9yLCBhdXRoZW50aWNhdG9yLCBzZXNzaW9uTWdyICkge1xuICAgICAgICBpZiggYXV0aGVudGljYXRvciApIHtcbiAgICAgICAgICAgIGF1dGhlbnRpY2F0b3Iuc2V0U2NvcGUoICRzY29wZSwgJGluamVjdG9yICk7XG4gICAgICAgICAgICBzZXNzaW9uTWdyLnJlc2V0UGlwZUxpbmUoKTtcbiAgICAgICAgICAgIGF1dGhlbnRpY2F0b3IuYXV0aGVudGljYXRlKCAkcSApLnRoZW4oIGZ1bmN0aW9uKCkge1xuICAgICAgICAgICAgICAgIHNlc3Npb25NZ3IuYXV0aGVudGljYXRpb25TdWNjZXNzZnVsKCk7XG4gICAgICAgICAgICB9ICk7XG4gICAgICAgIH1cbiAgICB9XG5dICk7XG4iXSwibmFtZXMiOltdLCJzb3VyY2VSb290IjoiIn0=\n//# sourceURL=webpack-internal:///./out/site/afx-cep/assets/js/aw.checkAuthentication.controller.js\n");

/***/ })

}]);