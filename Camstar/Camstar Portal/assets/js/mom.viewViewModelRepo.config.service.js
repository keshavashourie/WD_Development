/* eslint-disable require-jsdoc */
/**
 * @module js/mom.viewViewModelRepo.config.service
  * @requires lodash
  * @requires js/logger
  * @requires js/localeService
  * @requires js/mom.utils.service
  * @requires js/eventBus
/*eslint-disable require-jsdoc*/
import app from 'app';
import _ from 'lodash';
import 'js/localeService';
import 'js/mom.utils.service';
import 'js/viewModelService';
import _momACBSrv from 'js/mom.acb.component.service';
import _viewModeService from 'js/viewMode.service';

'use strict';
var exports = {};
var _$q;
var _$templateCache;
var _appCtxSvc;
var _utilsSvc;

exports.getViewAndViewModel = function( id, baseUrl, customViews ) {
    var data = {
        id: id,
        baseUrl: baseUrl,
        ctx: _appCtxSvc.ctx
    };

    // Get the associated View and ViewModel
    var customView = customViews.find( x => x.id === id );
    if( customView === undefined ) {
        return getViewAndViewModelResources( baseUrl, id, data, undefined );
    }
    // Get Search Data
    return getSearchViewAndViewModel( baseUrl, customView.id, data );

};

function getViewAndViewModelResources( baseUrl, id, data, customView ) {
    var result = {};
    var viewUrl = baseUrl + '/html/' + id + 'View.html';
    var tplId = id;
    var vExt = '.html';
    var vmExt = '.json';
    var reqHTMLUrl = customView !== undefined ? customView.customViewUrl + customView.viewMethod : baseUrl + '/html/' + tplId + 'View' + vExt;
    var tmplHTMLPromise = null;
    tmplHTMLPromise = _$templateCache.get( reqHTMLUrl );
    if( id === "momNavigateBreadcrumb" ) {
        tmplHTMLPromise = _$q.when( {
            data: null
        } );
    } else {
        tmplHTMLPromise = tmplHTMLPromise ? _$q.when( {
            data: tmplHTMLPromise
        } ) : _utilsSvc.httpGet( reqHTMLUrl );
    }
    return tmplHTMLPromise.then( function( html ) {
        result.view = html.data;
        _$templateCache.put( viewUrl, result.view );
        _$templateCache.put( reqHTMLUrl, result.view );
        var reqJSONUrl = customView !== undefined ? customView.customViewUrl + customView.viewModelMethod : baseUrl + '/viewmodel/' + tplId + 'ViewModel' + vmExt;
        var tmplJSONPromise = _$templateCache.get( reqJSONUrl );
        tmplJSONPromise = tmplJSONPromise ? _$q.when( {
            data: tmplJSONPromise
        } ) : _utilsSvc.httpGet( reqJSONUrl );
        return tmplJSONPromise.then( function( json ) {
            if( !data.id.match( /^_/ ) ) {
                _$templateCache.put( viewUrl, result.view );
                result.viewModel = json.data;
                return result;
            }
            var view = _.template( result.view )( data );
            _$templateCache.put( viewUrl, view );
            _$templateCache.put( reqJSONUrl, json.data );
            var parkModel = typeof json.data !== 'string' ? JSON.stringify( json.data ) : json.data;
            var theParsedTemplate = _.template( parkModel )( data );
            return {
                view: view,
                viewModel: JSON.parse( theParsedTemplate )
            };
        } );
    } );
}

function getSearchViewAndViewModel( baseUrl, id, data ) {
    var result = {};
    var viewUrl = baseUrl + '/html/' + id + 'View.html';
    var viewModelUrl = baseUrl + '/viewmodel/' + id + 'ViewModel' + '.json';
    var tmplHTMLPromise = null;

    // Call method to create view and viewModel for Search (returns obj with View and ViewModel)
    // Create ViewModel first from the Column Provider and then the View with the below logic.
    //Next, we will see the keyup events. (swf filter logic)
    var currentMode = _viewModeService.getViewMode();
    var isTable = currentMode === 'TableView' || currentMode === 'TableTreeView' || currentMode === 'SummaryView' ? true : false;
    var createdDOM = !isTable ? _momACBSrv.createViewViewModelFromList() : _momACBSrv.createViewViewModelFromColumns();
    tmplHTMLPromise = _$templateCache.get( viewUrl );
    tmplHTMLPromise = _$q.when( {
        data: tmplHTMLPromise
    } );
    return tmplHTMLPromise.then( function( html ) {
        html.data = createdDOM.view;
        result.view = html.data;
        _$templateCache.put( viewUrl, result.view );
        _$templateCache.put( viewUrl, result.view );
        var tmplJSONPromise = _$templateCache.get( viewModelUrl );
        tmplJSONPromise = _$q.when( {
            data: tmplJSONPromise
        } );
        return tmplJSONPromise.then( function( json ) {
            if( json.data === undefined ) {
                json.data = createdDOM.viewModel;
            }
            if( !data.id.match( /^_/ ) ) {
                _$templateCache.put( viewUrl, result.view );
                result.viewModel = json.data;
                return result;
            }
            var view = _.template( result.view )( data );
            _$templateCache.put( viewUrl, view );
            _$templateCache.put( viewModelUrl, json.data );
            var parkModel = typeof json.data !== 'string' ? JSON.stringify( json.data ) : json.data;
            var theParsedTemplate = _.template( parkModel )( data );
            return {
                view: view,
                viewModel: JSON.parse( theParsedTemplate )
            };
        } );
    } );
}

app.factory( 'mom.viewViewModelRepo.config.service', [
    '$q', '$templateCache', 'appCtxService',
    'momUtilsService',
    function( $q, $templateCache, appCtxService,
        utilsService ) {
        _$q = $q;
        _$templateCache = $templateCache;
        _appCtxSvc = appCtxService;
        _utilsSvc = utilsService;
        return exports;
    }
] );
export let moduleServiceNameToInject = 'mom.viewViewModelRepo.config.service';
export default moduleServiceNameToInject;
