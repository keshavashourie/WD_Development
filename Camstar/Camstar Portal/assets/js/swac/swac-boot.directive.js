/*global
 define
 */

/**
 * @module js/swac/swac-boot.directive
 */
define(['app', 'js/viewModelService', 'js/swac/swac-boot.service'],
function(app) {
  'use strict';

  app.directive('swacBoot', [function() {
    return {
      restrict: 'E',
      scope: {},
      template: '',
      controller: ['$scope', '$element', '$attrs', 'viewModelService', 'swacBootService', function(scope, element, attrs, viewModelSvc, swacBootSvc) {
        if (viewModelSvc) {
          var declViewModel = viewModelSvc.getViewModel(scope, true);
          if (declViewModel !== null) {
            swacBootSvc.boot({ viewModel: declViewModel });
          } else {
            swacBootSvc.boot();
          }
        }
      }]
    };
  }]);
});
