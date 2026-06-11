
var isCarrierOperations = (function () {
    'use strict';

    var useISScript = true;     // keep until ready to use core sidebar script 
    $(function () {
        if (Camstar.WebPortal.WebPortlets.SideBar && window.location.pathname.indexOf("CarrierOperation") > 0) {
            if (typeof Camstar.WebPortal.WebPortlets.SideBar.prototype._loadCommands === "function") {
                Camstar.WebPortal.WebPortlets.SideBar.prototype._saveLoadCommands = Camstar.WebPortal.WebPortlets.SideBar.prototype._loadCommands;

                Camstar.WebPortal.WebPortlets.SideBar.prototype._loadCommands = function () {
                    Camstar.WebPortal.WebPortlets.SideBar.prototype._saveLoadCommands.call(this);

                    //  Now call our updateCommandBar to set Reset text
                    setTimeout(updateCommandBar, 500);
                };
            } else {
                console.warn("Camstar.WebPortal.WebPortlets.SideBar.prototype._loadCommands is invalid.");
            }
        }

        setTimeout(updateCommandBar, 500);
    });

    function isInEProcedurePage() {
        return $("form").attr("action").indexOf("EProcedure") >= 0;
    }

    function updateCommandBar() {
        if (useISScript) {
            if (isInEProcedurePage()) {
                //  Find Side Bar and then find first button displayed and change class to show Submit icon
                let sideBarRight = parent.$find('ctl00_SideBarRight');
                let sideBar = $(sideBarRight._element).children().eq(0);
                let children = $(sideBar).children().eq(1);
                let div = $(children).eq(0);
                let span = $(div).children().eq(0);
                $(div).attr('title', 'Execute');
                $(span).removeClass('action-icon-clear');
                $(span).addClass('action-icon-submit');
            } else {
                let button = $('.action-redirect-button');
                if (button.length) {
                    button.children().eq(1).html('Reset');
                }
            }
        }
    }

})();

// 
var isMfgOperation = (function () {
    'use strict';

    var labels = {};                // labels for use on the page
    var displayDetails = [];        // array of objs defining properites to display in Mfg Order Detail panel. 

    var selectedOrder = null;
    var _sideBarInitialized = false;

    var SELECTORS = {
        RESOURCE_NAME: '#ctl00_WebPartManager_ResourceHeader_WP_Resource_Edit'
    };


    var ismfgOperationInterface = {
        initialize: initialize,
        showComments: showComments
    };
    return ismfgOperationInterface;

    function mfgOrderTileClicked(order) {
        if (!order)
            return;

        if (order.requiredRecipeLoaded) {
            return;
        }

        let getRecipeParams = {
            resourceName: $('span#ctl00_WebPartManager_ResourceHeader_WP_Resource input[type="text"]').val(),
            productName: order.ProductName,
            productRev: order.ProductRevision,
            specName: order.SpecName,
            specRev: order.SpecRevision,
            mfgOrderName: order.MfgOrderName
        };

        $.ajax({
            type: "POST",
            dataType: "json",
            url: './MfgOperationService.svc/web/GetRequiredRecipe',
            headers: {
                'Accept': 'application/json'
            },
            // Must set content-type this way to avoid jQuery bug with sending JSON containing "??"
            // https://forum.jquery.com/topic/special-characters-issue-find-random-strings-like-jquery20206329934545792639-1415046914457-in-data
            contentType: "application/json;charset=UTF-8",
            async: true,
            data: JSON.stringify(getRecipeParams),
            context: document.body
        })
            .success(getRecipeSuccess)
            .fail(getRecipeFail);

        //
        function getRecipeFail(response) {
            // TODO -notify user?
            console.error(response.responseText);
        }

        //
        function getRecipeSuccess(response) {
            if (response.GetRequiredRecipeResult.IsSuccess) {
                order.requiredRecipeLoaded = true;
                if (response.requiredRecipeName) {
                    order.RequiredRecipe = response.requiredRecipeName;
                }

                mfgOperation.rebuildDetails(order);
            }

            // Any notification if call fails?
        }
    }

    function VerifyHVTraceEnable(resourceName) {
        var result = false;

        if (resourceName) {
            let request =
            {
                resourceName: resourceName
            };

            $.ajax({
                type: "POST",
                dataType: "json",
                url: './MfgOperationService.svc/web/ValidateIsHVTraceabilityenable',
                headers: {
                    'Accept': 'application/json'
                },
                contentType: "application/json;charset=UTF-8",
                async: false,
                data: JSON.stringify(request),
                context: document.body,
                success: function (data) {
                    result = data;
                }
            })
        }

        return result;
    }

    function initialize(labelObj, displayDetailList) {
        mfgOperation.addOrderTileClickHandler(mfgOrderTileClicked);
        // Have to wait until DOM is settled - this will call functions defined in other .js files (they may not be loaded yet)
        $(function () {
            labels = labelObj;
            displayDetails = displayDetailList;
            selectedOrder = null;

            //  Set Labels to other objects 
            CR.isMfgOperationComments.setLabels(labels);
            CR.isMfgOperationMaterials.setLabels(labels);
            mfgOperation.addOrderTileClickHandler(mfgOrderSelected);
            mfgOperation.resizeHandler = doResize;

            setTimeout(function () {
                buildCommandBar();
                showNoData();
            }, 100);
        });
        $(SELECTORS.HIDE_NON_REQUIREMENT).on('change', toggleHideNonRequirement);
    }

    function mfgOrderSelected(order) {
        selectedOrder = order;
        CR.SlideOut.closeAll();

        if (order) {
            CR.isMfgOperationComments.getMfgOperationComments(onCommentsChanged, null, order, $(SELECTORS.RESOURCE_NAME).val(), labels);

            // When HV Tracaebility is disabled Material Requirement grid will consider Material Queue Data

            if (!VerifyHVTraceEnable($(SELECTORS.RESOURCE_NAME).val())) {
                CR.isMfgOperationMaterials.loadMaterialRequirements($(SELECTORS.RESOURCE_NAME).val(), order, isHideNonRequirementChecked());
            }
            else {
                CR.MfgOperationMaterials.loadMaterialRequirements($(SELECTORS.RESOURCE_NAME).val(), order, isHideNonRequirementChecked());
            }
        }
        showNoData();
    }

    function showNoData() {
        let show = mfgOperation.haveData() > 0;
        CR.SideBar.showButton('buttonComments', show);
        CR.SideBar.showButton('buttonDocuments', show);
    }

    function buildCommandBar() {
        var visible = mfgOperation.haveData();
        if (!_sideBarInitialized) {
            _sideBarInitialized = true;
            CR.SideBar.beginCustomize();
            CR.SideBar.addMenuItem('buttonComments', labels['Comments'], null, 'cr-button-comments', showComments, false, visible, 0).setBadge();
            CR.SideBar.addMenuItem('buttonDocuments', labels['Documents'], null, 'cr-button-documents', mfgOperation.showDocuments, false, visible, 0);
            CR.SideBar.addMenuItem('buttonAttributes', labels['Attributes'], null, 'cr-button-attributes', mfgOperation.showAttributes, false, true /*selectedOrder*/, 0);
            CR.SideBar.endCustomize();
        }
    }

    function showComments() {
        CR.SlideOut.closeAllRightSlideOut('ismfg-operation-comments');
        CR.isMfgOperationComments.showComments(selectedOrder, $(SELECTORS.RESOURCE_NAME).val(), labels, onCommentsChanged);

    }

    function toggleHideNonRequirement() {

        let hideNonRequirement = isHideNonRequirementChecked();
        mfgOperation.setCheckBoxChecked(hideNonRequirement);
        CR.isMfgOperationMaterials.loadMaterialRequirements($(SELECTORS.RESOURCE_NAME).val(), selectedOrder, hideNonRequirement);
    }

    function isHideNonRequirementChecked() {
        var $checkboxPanel = $(SELECTORS.HIDE_NON_REQUIREMENT);
        var $checkBox = $checkboxPanel.find('input[type=checkbox]');

        return !!($checkBox.closest('input').prop('checked'));
    }

    function onCommentsChanged(comments) {
        if (comments.Data) {
            CR.SideBar.updateBadge('buttonComments', 0, 'blue', true);
        } else {
            CR.SideBar.removeBadge('buttonComments');
        }
    }

    function doResize() {
        var height = $('#TemplateContentDiv').height() - 32;
        CR.SlideOut.resizeAll();
    }

})();