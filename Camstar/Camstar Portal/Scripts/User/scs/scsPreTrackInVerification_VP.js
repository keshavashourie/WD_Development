// Copyright Siemens 2021  

var scsPreTrackInVerification = (function () {
    'use strict';

    var EquipmentList = [];          // represents list of orders found for selected resource. properties match code behind MfgOrderListItem class
    var labels = {};                // labels for use on the page
    var displayDetails = [];        // array of objs defining properites to display in Mfg Order Detail panel. 
    var orderClickHandlers = [];    // any handlers in here will get called when a tile is clicked.

    var TEMPLATE = {
        contentRow: '<div class="content-row">\
                        <span class="content-name">_CONTENT_ROW_NAME_</span>\
                        <span class="content-value">_CONTENT_ROW_VALUE_</span>\
                        <span class=".icon-up"></span>\
                        <span class=".icon-down"></span>\
                    </div>'
    };

    var SELECTOR = {
        TILE_LIST: '#ctl00_WebPartManager_EquipmentTileWP_EquipmentList .item',
        //DETAIL_HEADER: '#mfgoperation-order-detail-header',
        //DETAIL_HEADER_TITLE: '#mfgoperation-order-detail-header > .title',
        //DETAIL_CONTENT: '#mfgoperation-order-detail-content',
        //DETAIL_CONTENT_TABLE: '#mfgoperation-order-detail-content > .content-tbl',
        //DETAIL_CONTAINER: "#mfgOperation-details-container",

        //MAIN_DATA: "#mfgoperation-main",
        //MATERIALS: "#mfgoperation-material",
        //MATERIALS_CONTENT: "#mfgoperation-material-content",
        //MATERIALs_HEADER: "#mfgoperation-material-header"

    };

    // interface
    var scsPreTrackInVerificationInterface = {
        addOrderTileClickHandler: addOrderTileClickHandler,
        haveData: haveData,
        initialize: initialize,
        //setDisplayDetails: setDisplayDetails,
        setOrderList: setOrderList,
        
        resizeHandler: null
    }
    return scsPreTrackInVerificationInterface;

    // Sets the properties to display in the Mfg Order Details panel
    // set extend = true to add properites and false to show only the specified properties.
    // details is an array of objects of the form { label : <labelName>, value : <orderPropName> }
    // - label is the name of the property in the "labels" obj to use as the property name.
    // - value is the name of the property on the selected order to use as display value. 
    //         names correspond to property names of the code behind class MfgOrderListItem or of a class derived from it.
    // }
    //function setDisplayDetails(extend, details) {
    //    if (extend)
    //        displayDetails = displayDetails.concat(details);
    //    else
    //        displayDetails = details;
    //}

    // handler is a callback function
    // this will be called after the standard tile click handler executes to update the details.
    // the object corresponding to the clicked tile is passed as an argument to the callback.
    function addOrderTileClickHandler(handler) {
        orderClickHandlers.push(handler);
    }

    function initialize(labelObj, displayDetailList) {
        labels = labelObj;
        displayDetails = displayDetailList;
        orderClickHandlers = [];
        setupResizeHandler();

        // set title to the details panel
        $(SELECTOR.DETAIL_HEADER_TITLE).text(labels.MfgOrderDetail);

        //updateDisplay();
		$(function(){
			var elem = $(".ui-webpart-resource-status div:nth-child(3)");
			elem.attr("key","item_r0_c0");
			$(".ui-webpart-resource-status div:nth-child(5)").attr("key","item_r0_c0");
			$(".ui-webpart-resource-status div:nth-child(7)").attr("key","item_r0_c0");
			
			if($("#ctl00_WebPartManager_MaskWP_scsMaskRequirement .toggle-container .header .icon-mask").length == 0)
				$("#ctl00_WebPartManager_MaskWP_scsMaskRequirement .toggle-container .header img").after('<span class="icon-mask"></span>');
			if($("#ctl00_WebPartManager_MaterialWP_scsMaterialRequirement .toggle-container .header .icon-material").length == 0)
			    $("#ctl00_WebPartManager_MaterialWP_scsMaterialRequirement .toggle-container .header img").after('<span class="icon-material"></span>');
			if ($("#ctl00_WebPartManager_ToolWP_scsToolRequirement .toggle-container .header .icon-tool").length == 0)
			    $("#ctl00_WebPartManager_ToolWP_scsToolRequirement .toggle-container .header img").after('<span class="icon-tool"></span>');
			if ($("#ctl00_WebPartManager_ToolFamilyWP_scsToolFamilyRequirement .toggle-container .header .icon-toolfamily").length == 0)
			    $("#ctl00_WebPartManager_ToolFamilyWP_scsToolFamilyRequirement .toggle-container .header img").after('<span class="icon-toolfamily"></span>');
		});
    }

    //function updateDisplay() {
    //    if (haveData()) {
    //        $(SELECTOR.DETAIL_CONTAINER).show();
    //        $(SELECTOR.MAIN_DATA).show();
    //    }
    //    else {
    //        $(SELECTOR.DETAIL_CONTAINER).hide();
    //        $(SELECTOR.MAIN_DATA).hide();
    //    }
    //}

    function setupResizeHandler() {
        // portal triggers this event when the containing panel is resized.  Use that to resize our table and Active PCB
        $(document).on('panelResized', function (e, bodyHeight) {
            resize();
        });

        var rszTime = null;
        $(window).bind('resize', function () {
            if (rszTime) clearTimeout(rszTime);
            rszTime = setTimeout(resize, 250);
        }).trigger('resize');

    }

    function resize() {
        var height = $('#TemplateContentDiv').height() - 32;
        let col = $(".equipmentList-tile-col");
        col.height(height);
        //col = $(".mfgOperation-data-col");
        //col.height(height);
        //col = $(".mfgOperation-details-col");
        //col.height(height);
        //col = $(SELECTOR.DETAIL_CONTAINER);
        //col.height(height-32);
        //col = $(SELECTOR.DETAIL_CONTENT);
        //let hdr = $(SELECTOR.DETAIL_HEADER);
        //col.height(height - 32 - hdr.height());

        //col = $(SELECTOR.MAIN_DATA);
        //if (col.length) {
        //    col.height(height);
        //    let headerHeight = $(SELECTOR.MATERIALs_HEADER).height();
        //    col = $(SELECTOR.MATERIALS_CONTENT);
        //    col.height(height - headerHeight);
        //}
        if (scsPreTrackInVerificationInterface.resizeHandler)
            scsPreTrackInVerificationInterface.resizeHandler();
    }

    function haveData() {
        return EquipmentList && EquipmentList.length;
    }

    var _lastSelectedTile = null;

    // Do any necessary updates to the Mfg Order tiles after they get rendered by core processing.
    function setOrderList(orderList) {
        EquipmentList = orderList;
        //updateDisplay();
        _lastSelectedTile = null;

        // update tiles in timeout to give client processing time to add them to page.
        setTimeout(updateTiles, 200);

        function updateTiles() {
            var $firstTile;
            var index = 0;

            $(SELECTOR.TILE_LIST).each(function () {
                var $tile = $(this);
                var tileId = $tile.attr('data-custom');
                $tile.click(function () { clickTile(tileId); });
                $tile.css('cursor', 'pointer');

                if (!$firstTile)
                    $firstTile = $tile;
				
				if($("#ctl00_WebPartManager_BlankWP0_HiddenSelectedEquipment_ctl00")[0].value == tileId){
					$firstTile = $tile;
				}
				
				if (EquipmentList != undefined){
				    if (EquipmentList[index++].CurrentAvailability == "2") {
				        $tile.find('.textArea').append('<img src="./assets/image/indicatorRedCircle16.svg" style="width:22px;">');
				    }
				    else {
				        $tile.find('.textArea').append('<img src="./assets/image/indicatorGreenSquare16.svg" style="width:22px;">');
				    }
                }
            });

           

            $('.icon-last').hide();

            if ($firstTile)
                $firstTile.trigger('click');
            else {
                // call any registered handlers
                orderClickHandlers.forEach(function (handler) {
                    handler(null);
                });
            }
        }
    }


    // handler for clicking a tile
    function clickTile(id) {
        // get selected order
        var order;
        EquipmentList.some(function (ord) {
            if (ord.UniqueId === id) {
                order = ord;
                return true;
            }
        });

        let tile = $('[data-custom="' + id + '"]');
        let prevMfgOrderId = _lastSelectedTile ? _lastSelectedTile.attr("data-custom") : null;
        if (prevMfgOrderId && prevMfgOrderId === id)
            return;

        if (_lastSelectedTile && _lastSelectedTile != tile)
            _lastSelectedTile.removeClass("tile-selected");
        if (tile) {
            tile.addClass("tile-selected");
            _lastSelectedTile = tile;
        }

		$("#ctl00_WebPartManager_BlankWP0_HiddenSelectedEquipment_ctl00")[0].value = id;
		$("#ctl00_WebPartManager_BlankWP0_btnFetchReqDetails")[0].click();

        // call any registered handlers
        orderClickHandlers.forEach(function (handler) {
            handler(order);
        });
    }

})();
