// Copyright 2020 Siemens AG
/**
 * This SWAC Service provides a way to set and retrieve the current utils from a SWAC Component
 * @module "js/mom.swac.utils.service"
 * @name "MOM.UI.Theme"
 * @requires app
 * @requires js/utils.service
 */
import app from 'app';
import momUtilsSvc from 'js/mom.utils.service';

const exports = {};

/**
 * Sets the title of the current Location, displayed in the application header.
 * @param {String} title The Location title to set.
 */
 exports.setHeaderTitle = function( title ) {
    momUtilsSvc.setHeaderTitle(title);
};


/**
 * Sets the header title, browser title, and browser subtitle.
 * @param {String} titles An object with the following properties, used to set the respective titles:
 * * browserSubTitle
 * * browserTitle
 * * headerTitle
 *
 * **Note** Existing titles will be used if not specified.
 */
 exports.setLocationTitles = function( titles ) {
     momUtilsSvc.setLocationTitles(titles);
};

/**
 * An object used to manage the navigation breadcrumb.
 * @typedef BreadcrumbProvider
 * @property {Crumb[]} crumbs An array of [Crumb](#~Crumb) objects representing the current navigation breadcrumb.
 * @property {Function} onSelect A function that will be executed to perform the navigation when a crumb is clicked. It takes a single [Crumb](#~Crumb) parameter.
 */
/**
 * An object used to represent a single crumb used in the navigation breadcrumb.
 * @typedef Crumb
 * @property {String} title The title of the crumb.
 * @property {String} stateId The ID of the state associated to the crumb.
 * @property {Object} [params={}] The parameters to pass to the crumb state when a navigation is performed (i.e. the user clicks the crumb).
 * @property {Boolean} [swacScreen=false] Whether the crumb is used to navigate to a SWAC Screen or not.
 */
/**
 * Overrides the default breadcrumb configuration and recreates a breadcrumb containing the specified crumbs.
 * > **Note:** Use this method only if you need to override the default breadcrumb. By default, the navigation breadcrumb is built automatically based on your **states.json** configuration. For more information, see [Configuring the navigation breadcrumb](https://gitlab.industrysoftware.automation.siemens.com/mom/mom-ui/wikis/configuring-the-navigation-breadcrumb) on the MOM UI Wiki.
 * @param {Crumb[]} crumbs An array of [Crumb](#~Crumb) objects used to configure the breadcrumb.
 * @returns {BreadcrumbProvider} The [BreadcrumbProvider](#~BreadcrumbProvider) object used to manage the breadcrumb.
 */
 exports.setBreadcrumb = function( crumbs ) {
     return momUtilsSvc.setBreadcrumb(crumbs);
};

/**
 * Adds the specified title as selected item in the navigation breadcrumb.
 * > **Note:** Use this method only if you need to override the default breadcrumb. By default, the navigation breadcrumb is built automatically based on your **states.json** configuration. For more information, see [Configuring the navigation breadcrumb](https://gitlab.industrysoftware.automation.siemens.com/mom/mom-ui/wikis/configuring-the-navigation-breadcrumb) on the MOM UI Wiki.
 * @param {String} title The string to display as the last item of the breadcrumb (typically used to indicate an item selection).
 * @returns {BreadcrumbProvider} The [BreadcrumbProvider](#~BreadcrumbProvider)  object used to manage the breadcrumb.
 */
exports.setBreadcrumbSelection = function( title ) {
    return momUtilsSvc.setBreadcrumbSelection(title);
};


app.factory( 'momSwacUtilsService', () => exports );

export default exports;
