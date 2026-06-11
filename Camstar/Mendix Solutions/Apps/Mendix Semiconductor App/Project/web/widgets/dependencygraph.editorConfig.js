'use strict';

Object.defineProperty(exports, '__esModule', {
  value: true
});
function getProperties(_values, defaultProperties /* , target: Platform*/) {
  return defaultProperties;
}
function check(values) {
  var errors = [];
  if (values.breadcrumbDatasource !== null) {
    if (values.breadcrumbNodeId === "") {
      errors.push({
        property: "breadcrumbNodeId",
        message: "Property 'Node ID' is required when a breadcrumbs database is added."
      });
    }
    if (values.breadcrumbLabel === "") {
      errors.push({
        property: "breadcrumbLabel",
        message: "Property 'Label' is required when a breadcrumbs database is added."
      });
    }
  }
  return errors;
}
exports.check = check;
exports.getProperties = getProperties;
