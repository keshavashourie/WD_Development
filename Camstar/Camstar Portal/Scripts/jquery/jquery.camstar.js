// Copyright Siemens 2024

/// <reference path="jquery.min.js" />
/// <reference path="jquery-ui.min.js" />

(function () {

    if (typeof window.CustomEvent === "function") return false;

    function CustomEvent(event, params) {
        params = params || { bubbles: false, cancelable: false, detail: null };
        var evt = document.createEvent('CustomEvent');
        evt.initCustomEvent(event, params.bubbles, params.cancelable, params.detail);
        return evt;
    }

    CustomEvent.prototype = window.Event.prototype;

    window.CustomEvent = CustomEvent;
})();

$(window).on("click",
    function (e) {
        var t = getCEP_top();
        var container = $(".tabsMenuBorder", t.document);
        if (container.is(":visible") && $(e.target).attr("class") != "tabsDownArrow") {
            if (!container.is(e.target) && container.has(e.target).length === 0) {
                container.hide();
            }
        }
    }
);

$(document).ready(function ($) {
    var t = getCEP_top();
    $("footer", t.document).footer();

    $(".close-concierge-button").click(function () {
        $(".zone").toggle();
    });

    $(".navigation").navigation();
    if (window === t)
        buildTopMenu($(".navi.top-menu", t.document));


    if (Camstars.Browser.IE) {  // remove focus from the pagination buttons for the correct styles
        $('td[id$="ctl00_WebPartManager_MDL_Filter_WP_InstanceGrid_pager"]').bind("mouseup",
            function (ev) {
                ev.currentTarget.firstElementChild.blur();
            });
    }
});

function PositioningConcierge() {
    var $parentBlockDOM = getCEP_top().$(".navigation, .cs-nav-tabs");
    if (!$parentBlockDOM.length) {
        console.warn("PositioningConcierge: parentBlockDOM not found");
        return;
    }

    var propertyShadow = "0";

    if ($parentBlockDOM.css("box-shadow"))
        propertyShadow = $parentBlockDOM.css("box-shadow").split("px");

    var heigthShadow = 0/*parseInt(propertyShadow[1]) + parseInt(propertyShadow[3])*/;
    var $conciergePointer = $(".concierge-container").find(".pointer");
    var pointerHeight = $conciergePointer.height() / 2;
    var distToBottomParent = parseInt($parentBlockDOM[0].offsetTop) + parseInt($parentBlockDOM[0].offsetHeight) + parseInt(pointerHeight);
    var $conciergeContainer = $(".concierge-container").find(".ui-flyout-container");
    var conciergeContainerWidth = $conciergeContainer.width();
    var distToRightSideParent = parseInt($parentBlockDOM[0].offsetLeft) + parseInt($parentBlockDOM[0].offsetWidth) - parseInt(conciergeContainerWidth) - 19;

    $conciergeContainer
        .closest(".concierge-container")
        .removeAttr("style")
        .css({ "left": distToRightSideParent, "top": distToBottomParent });
}

(function ($) {
    var $firstLvlList;
    var $firstLvlListItems;
    var $conciergeListItem;
    var $conciergeZone;
    var conciergeOpen;
    var deactivateSecondLevelId;

    var methods = {
        init: function (options) {
            return this.each(function () {
                $firstLvlList = $("> ul", this);
                $firstLvlListItems = $("> li", $firstLvlList).not(".navigation-concierge");
                $conciergeListItem = $(".navigation-concierge");
                $conciergeZone = $(".concierge-container");
                conciergeOpen = true;
                $firstLvlList.fadeIn();
                methods.positionConcierge();
                $(window).bind('resize.navigation', methods.positionConcierge);
                $firstLvlListItems.bind('click.navigation', methods.showSubMenu);
                $conciergeListItem.click(function () {
                    methods.positionConcierge();
                    __toppage.getConcierge().toggle();
                    conciergeOpen = !conciergeOpen;
                });

            });
        },

        destroy: function () {
            return this.each(function () {
                $(window).unbind('.navigation');
            });
        },

        positionConcierge: function () {
			if ($(getCEP_top().document.body).hasClass("Camstar-theme")) {
                if ($conciergeZone.length > 0)
                    $conciergeZone.css('left', ($conciergeListItem.position().left - 264) + 'px');
            }
            else {
                PositioningConcierge();
            }
            $('.navigation-concierge').hide();
        },

        showSubMenu: function () {
            var $firstLvlListItem = $(this);
            var $secondLvlList = $firstLvlListItem.children(".navigation-sub");
            var $secondLvlListItems = $("ul > li", $secondLvlList);

            $firstLvlListItem.unbind('mouseleave').mouseleave(function () {
                deactivateSecondLevelId = setTimeout(function () {
                    $secondLvlList.slideUp(200);
                    deactivateSecondLevelId = null;
                }, 200);
            });

            $secondLvlList.unbind('mouseenter').mouseenter(function (e) {
                if (deactivateSecondLevelId) {
                    clearTimeout(deactivateSecondLevelId);
                    deactivateSecondLevelId = null;
                }
                else
                    $(this).fadeIn(0);
                e.stopPropagation();
            });
            $secondLvlListItems.each(function () {
                if ($("ul", this).length > 0)
                    $("> a", this).addClass("has-children");
            });

            var offset = $firstLvlListItem.offset();
            var top = offset.top + $firstLvlListItem.innerHeight();
            var left = offset.left + 5;

            $secondLvlList.css({ top: top + 'px' }).css({ left: left + 'px' });

            $secondLvlList.fadeIn(200);

            $secondLvlListItems.each(function () {
                $(this).unbind('mouseenter').mouseenter(function () {
                    var me1 = this;
                    var $secondLvlListItem = $(this);
                    var $thirdLvlList = $(">.navigation-sub", $secondLvlListItem);

                    if ($thirdLvlList.length > 0) {
                        $secondLvlListItem.unbind('mouseleave').mouseleave(function () {
                            if ($thirdLvlList.length > 0) {
                                me1.deactivateSubLevel = setTimeout(function () {
                                    $thirdLvlList.slideUp(200);
                                }, 200);
                            }
                        });
                        var offset2 = $secondLvlListItem.offset();
                        var top2 = offset2.top - $secondLvlListItem.parents(".navigation-sub").offset().top;
                        var left2 = $secondLvlListItem.width() + 5;
                        $thirdLvlList.css({ top: top2 + 'px' }).css({ left: left2 + 'px' });
                        if (me1.deactivateSubLevel) {
                            clearTimeout(me1.deactivateSubLevel);
                            me1.deactivateSubLevel = null;
                        }
                        $thirdLvlList.fadeIn(200);
                    }
                });

                var $elem = $(this);
                if (!$elem.attr('clickAttached')) {
                    $elem.bind('click', function (e) {
                        $secondLvlList.slideUp(200);
                        e.stopPropagation();
                    });
                    $elem.attr('clickAttached', '1');
                }
            });
        }
    };

    $.fn.navigation = function (method) {
        if (methods[method])
            return methods[method].apply(this, Array.prototype.slice.call(arguments, 1));
        else if (typeof method === 'object' || !method)
            return methods.init.apply(this, arguments);
        else
            $.error('Method ' + method + ' does not exist on jQuery.navigation');
    };

})(jQuery);

(function ($) {
    var $footer;
    var $errorContainer;
    var $detailsButton;
    var statusMessage;
    var expanded;

    var methods = {
        init: function (options) {
            $footer = this;
            $errorContainer = $(".status", this).hide();
        },

        displayMessage: function (message, type) {
            var width = getTextWidth($("<div class='message'>").html(message));
            statusMessage = message;
            $errorContainer.attr("messagetype", type).html(message);
            $errorContainer.fadeIn();
            if (width > 850) {
                $errorContainer.statusFlyOut({ contentSelector: "" });
            }
        }
    };

    $.fn.footer = function (method) {
        if (methods[method])
            return methods[method].apply(this, Array.prototype.slice.call(arguments, 1));
        else if (typeof method === 'object' || !method)
            return methods.init.apply(this, arguments);
        else
            $.error('Method ' + method + ' does not exist on jQuery.footer');
    };
})(jQuery);

(function ($) {
    var $flyout;
    var $icon;
    var $contentHTML;

    var methods = {
        init: function (options) {
            //append tooltip icon
            $('<div class="tooltip-chevron"></div>').appendTo($(this));

            $flyout = $('#textFlyout');
            $icon = $('div.tooltip-chevron', this);
            if (options.contentSelector)
                $contentHTML = $(options.contentSelector, this).html();
            else
                $contentHTML = this.text();

            //add click event to icon
            $icon.bind('click', methods.toggle);

            //append flyout div
            if ($flyout.length == 0) {
                $flyout = $('<div id=textFlyout class="text-flyout ui-jqgrid-row-yellow" style="display: none">' +
                    '<div class="pointer-top"></div>' +
                    '<div class="wrap ui-helper-clearfix">' +
                    '<div class="content"></div>' +
                    '<div class="footer"><img src="/CamstarPortal/Themes/Camstar/images/icons/icon-close-16x16.png" /></div>' +
                    '</div>' +
                    '<div class="pointer-bottom"></div>' +
                    '</div>')
                    .appendTo('body');
            }
            return this;
        },

        toggle: function () {
            if ($flyout.is(':visible')) {
                methods.hide();
            }
            else {
                methods.show();
            }
            return this;
        },

        show: function () {
            $icon.attr("expanded", "true");

            var $content = $('div.content', $flyout);
            $content.html($contentHTML);
            //adjust formats to display text based on rich text properties
            $content.adjustFonts();

            var padding = 20;
            var maxFlyoutWidth = 400;

            // Calculate the X index
            var pageWidth = $icon.parents('body').width();
            var flyoutWidth = $flyout.width();
            var pointerX = $icon.offset().left;

            var newX = 0;

            if (flyoutWidth > maxFlyoutWidth) flyoutWidth = maxFlyoutWidth;

            if (pageWidth > (pointerX + (flyoutWidth / 2) + padding)) {
                newX = pointerX - (flyoutWidth / 2);
            }
            else {
                newX = pageWidth - (flyoutWidth + padding);
            }

            // Calculate the Y index
            var pageHeight = $icon.parents('body').height();
            var flyoutHeight = getTextHeight($flyout, flyoutWidth) + 10;
            var pointerY = $icon.offset().top;

            var newY = 0;

            if (pointerY > (pageHeight / 2)) { //bottom
                newY = pointerY - (flyoutHeight + padding);
            }
            else { //top
                newY = pointerY + padding;
            }

            // Calculate proper zIndex for the flyout panel
            var zIndex = 0;
            $($icon.parents('div')).each(function () {
                if (zIndex == 0 && $(this).css('z-index') != 'auto')
                    zIndex = parseInt($(this).css('z-index'));
            });

            $flyout.css({ top: newY + 'px', left: newX + 'px', width: flyoutWidth + 'px' });
            if (zIndex > 0)
                $flyout.css('z-index', (zIndex + 1).toString());

            $flyout.fadeIn("500");
            $('.footer > img', $flyout).unbind('click');
            $('.footer > img', $flyout).click(function () { $icon.click(); return false; });

            return this;
        },

        hide: function () {
            $icon.removeAttr("expanded");
            $flyout.fadeOut("500");
            return this;
        }
    };

    $.fn.statusFlyOut = function (method) {
        if (methods[method])
            return methods[method].apply(this, Array.prototype.slice.call(arguments, 1));
        else if (typeof method === 'object' || !method)
            return methods.init.apply(this, arguments);
        else
            $.error('Method ' + method + ' does not exist on jQuery.footer');
    };
})(jQuery);

(function ($) {
    var $taskList;
    var isResponsive = false;
    var taskListWrapper;
    var wrapperClicked = false;
    var top = 0;
    var left = 0;
    var methods = {
        init: function (options) {
            $taskList = this;

            //only initialize the iScroll panel if there are more than 6 items && when not in responsive mode
            if (options.taskCount.length > 0 && options.taskCount > 6
                && typeof (__page) !== 'undefined' && !__page.get_isResponsive()) {
                taskListWrapper = new iScroll(options.id, { hideScrollbar: true, checkDOMChanges: true, hScroll: false, hScrollbar: false, snap: 'div' });
                if (taskListWrapper) {
                    // TODO: Look to see if we can refactor
                    // classic desktop
                    if (typeof (__page) !== 'undefined' && !__page.get_isResponsive()) {

                        var headerBottom = $(".scrollable-menu > .header", $taskList).offset().top + 30;
                        var footerTop = $(".scrollable-menu > .footer", $taskList).offset().top;
                        var height = footerTop - headerBottom;
                        $(".scrollable-menu > .wrapper", $taskList).css({ height: height + 'px' });
                    }

                    setTimeout(function () { taskListWrapper.refresh(); }, 100);
                }
            }

            $.bind('touchmove.tasklist', function (e) { e.preventDefault(); }, false);
            $.bind('resize', methods.setWrapperHeight);

            $(".scrollable-menu .button-up", $taskList).bind('click.tasklist', methods.onUpClick);
            $(".scrollable-menu .button-down", $taskList).bind('click.tasklist', methods.onDownClick);

            $(window).bind('resize.tasklist', methods.setWrapperHeight);
            $(window).bind('scroll.tasklist', methods.setWrapperHeight);
            isResponsive = $(document.body).hasClass("cs-responsive");

            if (!isResponsive) {
                // TODO: Look to see if we can refactor
                //classic desktop                
                $(".task-item .document").bind('click.taskdocument', methods.onDocumentClick);
                $(".scrollable-menu .wrapper").ready(function () {

                    //attribute to get around scrollbar problem on txn tasks
                    $(".form-scrollablepanel").removeAttr("txnTask", "true");

                    if (options.scrollTo.length > 0) {
                        taskListWrapper.maxScrollY = -10000;

                        setTimeout(function () { taskListWrapper.scrollToElement('div:nth-child(' + options.scrollTo + ')', 300); }, 300);
                        setTimeout(function () { methods.setWrapperHeight.call(this); }, 100);
                    }
                });

            }
            // responsive 
            else {
                $(".scrollable-menu-container-m .document-m").bind('click.taskdocument', methods.onDocumentClick);
                var $pageEproc = $(".page-eproc-m");

                // find a row with scrollable-menu-m
                var h = $("#WebPart_ContainerStatusWP_R2_UIComponent").outerHeight();
                $("#TemplateContentDiv >.page-container >.row").each(function (i) {
                    if (i != 3 || $pageEproc.length) {
                        h += $(this).outerHeight();
                    }
                    else{
                        this.style.height = "calc( 100vh - " + h + "px )";
                    }
                });
            }

            //PagePanel functionality for transaction tasks
            var $pagePanel = $(".cs-page-panel");

            if ($(".task-details-cell").length === 0 && $(".taskmenuwp-resp").length > 0) {
                $(".taskmenuwp-resp").siblings().toggleClass("task-details-cell", $pagePanel.length > 0);
                $(".taskmenuwp-resp").siblings().toggleClass("task-details-has-custom-wp", $pagePanel.length > 0);
            }

            $(".task-details-cell").toggleClass("transaction-task", $pagePanel.length > 0);
            $(document.body).toggleClass("transaction-task", $pagePanel.length > 0);

            if ($pagePanel.length) {

                //bind to the load element of the txn page iframe
                $("> iframe", $pagePanel).on('load', function () {

                    // Handler for iframe .load() called - append class to body to hide the container status web part
                    var $body = $(this).contents().find("body");
                    $body.addClass("pagepanel-eproc");
                    $body.toggleClass("fullScreen", window.__page.get_fullScreen());

                    //this makes the transaction iframe appear after it has loaded and had the above css applied to hide
                    //the container header wp. Using css/attribute here instead of show() for IE9 rendering issues
                    $(".cs-page-panel")
                        .css({ display: 'block' })
                        .toggleClass("fullScreen", window.__page.get_fullScreen())
                        .attr("loaded", "true");
                });
            }
        },

        setWrapperHeight: function () {

            if (taskListWrapper) {

                if (typeof (__page) !== 'undefined') {
                    // classic desktop
                    if (!__page.get_isResponsive()) {
                        var headerBottom = $(".scrollable-menu > .header", $taskList).offset().top + 30;
                        var footerTop = $(".scrollable-menu > .footer", $taskList).offset().top;
                        var height = footerTop - headerBottom;
                        $(".scrollable-menu > .wrapper", $taskList).css({ height: height + 'px' });
                    }
                    // responsive
                    else {
                        $(".scrollable-menu-m .scrollable-menu-container-m", $taskList).css({ height: height + 'px' });
                    }
                }

                setTimeout(function () { taskListWrapper.refresh(); }, 100);
            }
        },

        onDownClick: function (e) {

            if (taskListWrapper.y > taskListWrapper.maxScrollY)
                taskListWrapper.scrollTo(0, 100, 200, true);
            else
                $(".scrollable-menu .button-down").attr("disabled", true);

            $(".scrollable-menu .button-up").removeAttr("disabled");
        },

        onUpClick: function (e) {

            if (taskListWrapper.y < -20)
                taskListWrapper.scrollTo(0, -100, 200, true);
            else
                $(".scrollable-menu .button-up").attr("disabled", true);

            $(".scrollable-menu .button-down").removeAttr("disabled");
        },

        onDocumentClick: function (e) {
            if (typeof (__page) !== 'undefined' && !__page.get_isResponsive()) {
                var flyoutDoc$;
                //get the position of the current selected task item         
                var offset = $(this).offset();
                top = offset.top;
                left = offset.left;
                flyoutDoc$ = $(".ui-scrollablemenu-doc-flyout");
                if (flyoutDoc$.length) {
                    flyoutDoc$[0].click();
                }

                //align the document flyout to the position of the selected task item
                top = top - 180;
                left = left + 240;
                if (top > 430) { top = 430; }

                if (typeof (__page) !== 'undefined') {
                    // classic desktop
                    if (!__page.get_isResponsive()) {
                        $(".webpart-taskmenu .ui-flyout-container").css("left", left);
                        $(".webpart-taskmenu .ui-flyout-container").css("top", top);
                    }
                }
            }
            else {
                // Open Command bar documents
                var documentsSpan = $('.container-documents', $('#ctl00_sidebar'));
                if (documentsSpan.length) {
                    documentsSpan.click();
                }
            }
        }
    };

    $.fn.tasklist = function (method) {
        if (methods[method])
            return methods[method].apply(this, Array.prototype.slice.call(arguments, 1));
        else if (typeof method === 'object' || !method)
            return methods.init.apply(this, arguments);
        else
            $.error('Method ' + method + ' does not exist on jQuery.tasklist');
    };
})(jQuery);

(function ($) {

    $.fn.easyAccordion = function (options) {
        var defaults = {
            slideNum: true,
            autoStart: false,
            pauseOnHover: false,
            slideInterval: 5000
        };

        this.each(function () {
            var _$this = $(this);
            var settings = $.extend(defaults, options);
            _$this.find('dl').addClass('easy-accordion');

            $.fn.setVariables = function () {
                var __$this = $(this);
                dlWidth = __$this.width();
                if (!settings.width) {
                    var $panel = $("#scrollablepanel");
                    if ($panel.length) {
                        var panelWidth = $panel.width();
                        dlWidth = panelWidth - $('.ui-eproc-summarybutton', $panel).outerWidth() - 18;
                    }
                }
                dlHeight = __$this.height();
                dtWidth = __$this.find('dt').outerWidth(true);
                slideTotal = __$this.find('dt').length;
                if (slideTotal == 1) {
                    dtHeight = 11;
                    dtWidth = 11;
                }

                ddWidth = dlWidth - (dtWidth * slideTotal) - (__$this.find('dd').outerWidth(true) - __$this.find('dd').width());
                ddHeight = dlHeight - (__$this.find('dd').outerHeight(true) - __$this.find('dd').height());
            };

            _$this.setVariables();

            var f = 1;
            _$this.find('dt').each(function () {
                $(this).addClass('spine_' + f);
                // add active corner
                var corner = document.createElement('div');
                corner.className = 'activeCorner spine_' + f;
                $(this).append(corner);
            });

            if (_$this.find('.active').length)
                _$this.find('.active').next('dd').addClass('active');
            else
                _$this.find('dt:first').addClass('active').next('dd').addClass('active');

            $.fn.findActiveSlide = function () {
                var i = 1;
                this.find('dt').each(function () {
                    if ($(this).hasClass('active'))
                        activeID = i; // Active slide
                    else if ($(this).hasClass('no-more-active'))
                        noMoreActiveID = i; // No more active slide
                    i = i + 1;
                });
            };

            $.fn.calculateSlidePos = function () {
                var totalSlides = $(this).find('dt').length;
                if (totalSlides <= 0) return;
                // maximum 4 slides are visible.
                if (totalSlides <= 3) {
                    $(this).find('dd.active').css({ 'width': dlWidth - (totalSlides - 1) * dtWidth });
                    $(this).find('.easy-accordion > dt').eq(totalSlides - 1).addClass('last');
                }
                // if more then 4 slides - scroll-left and scroll-right buttons are appeared.
                else {
                    var scrollLeft = $(".ui-accordionmenu-scrollleft-div", this).length ? $(".ui-accordionmenu-scrollleft-div", this).width() : 0;
                    var scrollRight = $(".ui-accordionmenu-scrollright-div", this).length ? $(".ui-accordionmenu-scrollright-div", this).width() : 0;
                    $(this).find('dd.active').css({ 'width': dlWidth - 3 * dtWidth - scrollLeft - scrollRight });
                    $(this).find('.easy-accordion > dt').eq(3).addClass('last');
                }
            };
            var activeWidth = 0;

            var changeTooltipPosition = function (event) {
                var position = $('dd.active').position();
                var tooltipX = position.left;
                var tooltipY = position.top + 20;
                $('div.tooltip').css({ top: tooltipY, left: tooltipX });
            };

            var showTooltip = function (event) {
                $('div.tooltip').remove();
                $('#taskListTextFlyout').show();
                changeTooltipPosition(event);
            };

            var hideTooltip = function () {
                $('div.tooltip').remove();
            };

            var instructionWidth = 0;

            instructionWidth = $(this).find('dd.active .instruction').width();

            _$this.findActiveSlide();
            _$this.calculateSlidePos();

            //remove html characters from the actual task list instruction displayed in the control
            $('dd.active .instruction').html(function (index, currentHtmlString) {
                return currentHtmlString.replace(/&lt;[\w\W]+?&gt;/gi, '').replace(/<\/?pre>/g, '');
            });

            //Get the html contents of the instructions to use in the flyout    
            var contentHTML = $('dd.active .instruction').html();

            //remove paragraphs from the actual task list instruction
            $('dd.active .instruction').html(function (index, currentHtmlString) {
                return currentHtmlString.replace(/<\/?p>/g, '');
            });

            activeWidth = $(this).find('dd.active').width();

            //Add tooltip - ellipsis if the text is greater than the dd element that contains it 
            if (instructionWidth > activeWidth) {

                //append the expansion icon to the end of the instruction span
                $('<div class="tooltip-chevron"></div>')
                    .appendTo('dd.active');

                $('div.tooltip-chevron', 'dd.active').click(
                    function () {

                        $('#taskListTextFlyout').is(":visible") ? handleClose() : displayFlyout(contentHTML);
                    });

                //handle displaying of text flyout for task list instruction
                function displayFlyout(contentHTML) {
                    var fl = $('#taskListTextFlyout');
                    var tt = $('div.tooltip-chevron', 'dd.active');
                    $('.tooltip-chevron').attr("expanded", "true");
                    $('div.content', fl[0]).css('height', '');

                    $('div.content', fl[0]).html(contentHTML);

                    //adjust formats to display text based on rich text properties
                    $('div.content', fl[0]).adjustFonts();

                    var width = fl.width();
                    if (width > 400) {
                        width = 400;
                        fl.width(width);
                    }

                    var imgCoord = tt.offset();
                    // shift to image center
                    imgCoord.left += tt.width() / 2;
                    imgCoord.top += tt.height() / 2;

                    var parentBody = tt.parents('body');
                    var pointerShift = 18;
                    var pointerLeft = 0;
                    var horizShift = - pointerShift;            //on the right by the image

                    if ((imgCoord.left + width) > parentBody.width()) {
                        horizShift = -width + pointerShift - 2; // on the left by the image 
                        pointerLeft = width - pointerShift * 2 + 3;
                    }

                    var top = imgCoord.top + tt.height() + 10;
                    var left = imgCoord.left + horizShift + 1;

                    // Calculate proper zIndex for the flyout panel
                    var zIndex = 0;
                    $(tt.parents('div')).each(function () {
                        if (zIndex == 0 && $(this).css('z-index') != 'auto')
                            zIndex = parseInt($(this).css('z-index'));
                    });

                    fl.css({ top: top + 'px', left: left + 'px' });
                    $('div.content', fl[0]).height(fl.height() - 20);

                    if (zIndex > 0)
                        fl.css('z-index', (zIndex + 1).toString());

                    $('.pointer-top', fl[0]).css({ left: pointerLeft + 'px', width: fl.width() + 'px' })
                        .unbind('click')
                        .click(function () { tt.click(); return false; });

                    fl.fadeIn("500");
                    $('#taskListTextFlyout .footer > img').unbind('click');
                    $('#taskListTextFlyout .footer > img').click(function () { tt.click(); return false; });
                }

                function handleClose() {
                    $('.tooltip-chevron').removeAttr("expanded");
                    $('#taskListTextFlyout').fadeOut("500");
                }

                //Create textflyout structure
                var taskListTextFlyout = $('#taskListTextFlyout');
                if (taskListTextFlyout.length == 0) {
                    $('<div id=taskListTextFlyout class="text-flyout ui-jqgrid-row-yellow">' +
                        '<div class="pointer-top"></div>' +
                        '<div class="wrap ui-helper-clearfix">' +
                        '<div class="content"></div>' +
                        '<div class="footer"><img src="/CamstarPortal/Themes/Camstar/images/icons/icon-close-16x16.png" /></div>' +
                        '</div>' +
                        '<div class="pointer-bottom"></div>' +
                        '</div>')
                        .appendTo('body');
                }
                $('#taskListTextFlyout').hide();
            }
            else if ($('#taskListTextFlyout').is(":visible")) {
                $('#taskListTextFlyout').hide();
            }
        });
    };
})(jQuery);

(function ($) {
    $.widget("camstar.flyout", {
        // default options
        options: {
            clear: null,
            className: null,
            target: null,
            show: false,
            location: 'below',
            header: { text: null },
            offset: { top: 0, left: 0 }
        },

        // set up the widget
        _create: function () {
            this.flyout = $("<div class=\"flyout\">" +
                "<div class=\"wrap ui-helper-clearfix\">" +
                "<div class=\"content\"></div>" +
                "<div class=\"footer\"></div>" +
                "</div>" +
                "</div>");

            $('.content', this.flyout).append(this.element);

            if (this.options.className != null)
                this.flyout.addClass(this.options.className);

            if (this.options.header != null) {
                $("<div class=\"header\"><span><span></div>").insertBefore($(".content", this.flyout));
                $(".header span", this.flyout).html(this.options.header.text);
            }

            if (this.options.location == 'below')
                $('.wrap', this.flyout).before("<div class=\"pointer\"></div>");
            else if (this.options.location == 'above')
                $('.wrap', this.flyout).after("<div class=\"pointer\"></div>");

            $("body").append(this.flyout);
            this.options.trigger.bind("click.flyout", $.proxy(this._toggle, this));
            $("body").bind("click.flyout", $.proxy(this._hide, this));
            if (this.options.show)
                this._toggle();
        },

        _toggle: function (e) {
            if (this.flyout.is(":visible"))
                this._hide();
            else
                this._show();
        },

        _show: function () {
            var top = this.options.trigger.offset().top;
            var left = this.options.trigger.offset().left;
            var height = this.flyout.height();
            var actualLeft = left - this.options.offset.left;
            var actualTop = top - this.options.offset.top;
            if (this.options.location == 'above')
                actualTop = actualTop - height - 30;
            if (this.options.location == 'below')
                actualTop = actualTop + 30;

            if (actualLeft < 0) {
                // correct to be not cut
                actualLeft = 2;
            }
            else if (actualLeft == 0) {
                actualLeft = 4;
            }

            this.flyout.css({ top: actualTop + 'px', left: actualLeft + 'px' });
            this.flyout.fadeIn(
                {
                    duration: 500,
                    start: function () {
                        var p = $(this).position().left;
                        if (p == 2)
                            $('.pointer', this).attr('edge', 'left');
                        else
                            $('.pointer', this).removeAttr('edge');
                    }
                });
        },

        _hide: function () {
            this.flyout.fadeOut(500);
        },

        // respond to changes to options
        _setOption: function (key, value) {
            switch (key) {
                case "clear":
                    break;
            }

            $.Widget.prototype._setOption.apply(this, arguments);
            // In jQuery UI 1.9 and above, you use the _super method instead
            // this._super( "_setOption", key, value );
        },

        close: function () {
            this._hide();
        },

        // clean up any modifications your widget has made to the DOM
        destroy: function () {

            // In jQuery UI 1.8, you must invoke the destroy method from the base widget

            $.Widget.prototype.destroy.call(this);

            this.options.trigger.unbind("click.flyout", $.proxy(this._toggle, this));
            $("body").unbind("click.flyout", $.proxy(this._hide, this));
            // In jQuery UI 1.9 and above, you would define _destroy instead of destroy and not call the base method
        }
    });
}(jQuery));

(function ($) {

    $.fn.autoGrowInput = function (o) {

        o = $.extend({
            maxWidth: 1000,
            minWidth: 0,
            comfortZone: 70
        }, o);

        this.filter('input:text').each(function () {

            var minWidth = o.minWidth || $(this).width(),
                val = '',
                input = $(this),
                testSubject = $('<tester/>').css({
                    position: 'absolute',
                    top: -9999,
                    left: -9999,
                    width: 'auto',
                    fontSize: input.css('fontSize'),
                    fontFamily: input.css('fontFamily'),
                    fontWeight: input.css('fontWeight'),
                    letterSpacing: input.css('letterSpacing'),
                    whiteSpace: 'nowrap'
                }),
                check = function () {

                    if (val === (val = input.val())) { return; }

                    // Enter new content into testSubject
                    var escaped = val.replace(/&/g, '&amp;').replace(/\s/g, ' ').replace(/</g, '&lt;').replace(/>/g, '&gt;');
                    testSubject.html(escaped);

                    // Calculate new width + whether to change
                    var testerWidth = testSubject.width(),
                        newWidth = (testerWidth + o.comfortZone) >= minWidth ? testerWidth + o.comfortZone : minWidth,
                        currentWidth = input.width(),
                        isValidWidthChange = (newWidth < currentWidth && newWidth >= minWidth)
                            || (newWidth > minWidth && newWidth < o.maxWidth);

                    // Animate width
                    if (isValidWidthChange && (newWidth > currentWidth)) {
                        var columnNum = $(this).parent().attr('columnnum');
                        $('span[columnnum="' + columnNum + '"]').children('input').width(newWidth);
                    }

                };

            testSubject.insertAfter(input);

            $(this).bind('keyup keydown blur update', check);

            //call the check method here in case of page load and field already has data in it
            check();
        });

        return this;

    };

})(jQuery);

(function ($) {
    var $computation;
    var ShopFloorDCVerticalSample = '';
    var ShopFloorDCVerticalData = '';
    var ShopFloorDCVerticalDataBottom = '';
    var ShopFloorDCHorizontalDataTitle = '';
    var ShopFloorDCHorizontalData = '';
    var methods = {
        init: function (options) {
            $computation = this;
            //Desktop
            if (typeof (__page) !== 'undefined' && !__page.get_isResponsive()) {
                ShopFloorDCVerticalSample = 'ShopFloorDCVerticalSample';
                ShopFloorDCVerticalData = 'ShopFloorDCVerticalData';
                ShopFloorDCVerticalDataBottom = 'ShopFloorDCVerticalData-bottom';
                ShopFloorDCHorizontalDataTitle = 'ShopFloorDCHorizontalDataTitle';
                ShopFloorDCHorizontalData = 'ShopFloorDCHorizontalData';
            }  //Responsive
            else {
                ShopFloorDCVerticalSample = 'ShopFloorDCVerticalSampleResp';
                ShopFloorDCVerticalData = 'ShopFloorDCVerticalDataResp';
                ShopFloorDCVerticalDataBottom = 'ShopFloorDCVerticalDataResp-bottom';
                ShopFloorDCHorizontalDataTitle = 'ShopFloorDCHorizontalDataTitleResp';
                ShopFloorDCHorizontalData = 'ShopFloorDCHorizontalDataResp';

            }
            //vertical layout bindings
            $("td." + ShopFloorDCVerticalData, $computation).bind('click.cell', methods.onCellClick);
            $("td." + ShopFloorDCVerticalDataBottom, $computation).bind('click.cell', methods.onCellClick);
            $("td." + ShopFloorDCVerticalSample, $computation).bind('mouseover.header', methods.onHeaderMouseOver);
            $("td." + ShopFloorDCVerticalSample, $computation).bind('mouseout.header', methods.onHeaderMouseOut);
            $("td." + ShopFloorDCVerticalSample, $computation).bind('click.header', methods.onHeaderClick);
            $("td." + ShopFloorDCVerticalData, $computation).bind('mouseover.cell', methods.onCellMouseOver);
            $("td." + ShopFloorDCVerticalDataBottom, $computation).bind('mouseover.cell', methods.onCellMouseOver);
            $("td." + ShopFloorDCVerticalData, $computation).bind('mouseout.cell', methods.onCellMouseOut);
            $("td." + ShopFloorDCVerticalData, $computation).bind('mouseout.cell', methods.onCellMouseOut);

            //horizontal layout bindings            
            $("td." + ShopFloorDCHorizontalDataTitle, $computation).bind('click.header', methods.onHeaderClick);
            $("td." + ShopFloorDCHorizontalDataTitle, $computation).bind('mouseover.header', methods.onHeaderMouseOver);
            $("td." + ShopFloorDCHorizontalDataTitle, $computation).bind('mouseout.header', methods.onHeaderMouseOut);
            $("div." + ShopFloorDCHorizontalData, $computation).bind('click.cell', methods.onCellClick);
            $("div." + ShopFloorDCHorizontalData, $computation).bind('mouseover.cell', methods.onCellMouseOver);
            $("div." + ShopFloorDCHorizontalData, $computation).bind('mouseout.cell', methods.onCellMouseOut);

            //set selected fields on load
            $(".ParametricDataControl").ready(function () {

                var tdAttr = $('td').attr('samplenum');

                if ($(".samplesHiddenField").val() >= 0 && typeof tdAttr !== typeof undefined && tdAttr !== false) {
                    $(this).attr('select', 'true');

                    //select the current sample (all cells that match the samplenum)
                    if ($("." + ShopFloorDCVerticalSample).length > 0) {
                        $('td[samplenum=' + $(".samplesHiddenField").val() + ']').attr('select', 'true');
                    }
                    else {
                        $('td[samplenum=' + $(".samplesHiddenField").val() + '] div').attr('select', 'true');
                        $("." + ShopFloorDCHorizontalDataTitle + "[samplenum=" + $(".samplesHiddenField").val() + "]").attr('select', 'true');
                    }
                }
            });
        },

        onHeaderClick: function (e) {
            //Desktop
            if (typeof (__page) !== 'undefined' && !__page.get_isResponsive()) {
                ShopFloorDCVerticalSample = 'ShopFloorDCVerticalSample';
                ShopFloorDCVerticalData = 'ShopFloorDCVerticalData';
                ShopFloorDCVerticalDataBottom = 'ShopFloorDCVerticalData-bottom';
                ShopFloorDCHorizontalDataTitle = 'ShopFloorDCHorizontalDataTitle';
                ShopFloorDCHorizontalData = 'ShopFloorDCHorizontalData';
            }//Responsive
            else {
                ShopFloorDCVerticalSample = 'ShopFloorDCVerticalSampleResp';
                ShopFloorDCVerticalData = 'ShopFloorDCVerticalDataResp';
                ShopFloorDCVerticalDataBottom = 'ShopFloorDCVerticalDataResp-bottom';
                ShopFloorDCHorizontalDataTitle = 'ShopFloorDCHorizontalDataTitleResp';
                ShopFloorDCHorizontalData = 'ShopFloorDCHorizontalDataResp';

            }

            if ($(this).attr('samplenum') >= 0) {
                //sample selection - first clear out any other selected samples
                //then select the current sample (all cells that match the samplenum)
                if ($("." + ShopFloorDCVerticalSample).length > 0) {

                    $("td." + ShopFloorDCVerticalData).removeAttr('select');
                    $("td." + ShopFloorDCVerticalDataBottom).removeAttr('select');
                    $("." + ShopFloorDCVerticalSample).removeAttr('select');

                    $('td[samplenum=' + ($(this).attr('samplenum')) + ']').attr('select', 'true');
                }
                else {
                    $("div." + ShopFloorDCHorizontalData).removeAttr('select');
                    $("." + ShopFloorDCHorizontalDataTitle).removeAttr('select');

                    $('td[samplenum=' + ($(this).attr('samplenum')) + '] div').attr('select', 'true');
                }

                $(this).attr('select', 'true');

                //set the hidden field value
                $(".samplesHiddenField").val($(this).attr('samplenum'));

            }
        },

        onHeaderMouseOver: function (e) {
            if (typeof (__page) !== 'undefined' && !__page.get_isResponsive()) {
                ShopFloorDCVerticalSample = 'ShopFloorDCVerticalSample';
            }//Responsive
            else {
                ShopFloorDCVerticalSample = 'ShopFloorDCVerticalSampleResp';

            }

            if ($(this).attr('samplenum') >= 0) {
                $(this).attr('mouseover', 'true');

                //select the current sample (all cells that match the samplenum)
                if ($("." + ShopFloorDCVerticalSample).length > 0) {
                    $('td[samplenum=' + ($(this).attr('samplenum')) + ']').attr('mouseover', 'true');
                }
                else {
                    $('td[samplenum=' + ($(this).attr('samplenum')) + '] div').attr('mouseover', 'true');
                }
            }
        },

        onHeaderMouseOut: function (e) {
            //Desktop
            if (typeof (__page) !== 'undefined' && !__page.get_isResponsive()) {
                ShopFloorDCVerticalSample = 'ShopFloorDCVerticalSample';
            }//Responsive
            else {
                ShopFloorDCVerticalSample = 'ShopFloorDCVerticalSampleResp';
            }

            if ($(this).attr('samplenum') >= 0) {
                $(this).removeAttr('mouseover');

                if ($("." + ShopFloorDCVerticalSample).length > 0) {
                    $('td[samplenum=' + ($(this).attr('samplenum')) + ']').removeAttr('mouseover');
                }
                else {
                    $('td[samplenum=' + ($(this).attr('samplenum')) + '] div').removeAttr('mouseover');
                }
            }
        },

        onCellClick: function (e) {
            //Desktop
            if (typeof (__page) !== 'undefined' && !__page.get_isResponsive()) {
                ShopFloorDCVerticalSample = 'ShopFloorDCVerticalSample';
                ShopFloorDCVerticalData = 'ShopFloorDCVerticalData';
                ShopFloorDCVerticalDataBottom = 'ShopFloorDCVerticalData-bottom';
                ShopFloorDCHorizontalDataTitle = 'ShopFloorDCHorizontalDataTitle';
                ShopFloorDCHorizontalData = 'ShopFloorDCHorizontalData';
            }//Responsive
            else {
                ShopFloorDCVerticalSample = 'ShopFloorDCVerticalSampleResp';
                ShopFloorDCVerticalData = 'ShopFloorDCVerticalDataResp';
                ShopFloorDCVerticalDataBottom = 'ShopFloorDCVerticalDataResp-bottom';
                ShopFloorDCHorizontalDataTitle = 'ShopFloorDCHorizontalDataTitleResp';
                ShopFloorDCHorizontalData = 'ShopFloorDCHorizontalDataResp';

            }

            if ($(this).attr('samplenum') >= 0 || $(this).parent().attr('samplenum') >= 0) {
                //set the sample num on the hidden field so the calculate button knows which sample to perform the calc on
                //next clear out any other selected samples
                //then select the current sample (all cells that match the samplenum)
                if ($("." + ShopFloorDCVerticalSample).length > 0) {
                    $(".samplesHiddenField").val($(this).attr('samplenum'));

                    $("td." + ShopFloorDCVerticalData).removeAttr('select');
                    $("td." + ShopFloorDCVerticalDataBottom).removeAttr('select');
                    $("." + ShopFloorDCVerticalSample).removeAttr('select');

                    $('td[samplenum=' + ($(this).attr('samplenum')) + ']').attr('select', 'true');
                } else {
                    $(".samplesHiddenField").val($(this).parent().attr('samplenum'));

                    $("div." + ShopFloorDCHorizontalData).removeAttr('select');
                    $("." + ShopFloorDCHorizontalDataTitle).removeAttr('select');

                    $('td[samplenum=' + ($(this).parent().attr('samplenum')) + '] div').attr('select', 'true');
                    $("." + ShopFloorDCHorizontalDataTitle + "[samplenum=" + ($(this).parent().attr('samplenum')) + "]").attr('select', 'true');
                }
            }
        },

        onCellMouseOver: function (e) {
            //Desktop
            if (typeof (__page) !== 'undefined' && !__page.get_isResponsive()) {
                ShopFloorDCVerticalSample = 'ShopFloorDCVerticalSample';
                ShopFloorDCHorizontalDataTitle = 'ShopFloorDCHorizontalDataTitle';
            }//Responsive
            else {
                ShopFloorDCVerticalSample = 'ShopFloorDCVerticalSampleResp';
                ShopFloorDCHorizontalDataTitle = 'ShopFloorDCHorizontalDataTitleResp';

            }

            if ($(this).attr('samplenum') >= 0 || $(this).parent().attr('samplenum') >= 0) {
                //select the current sample (all cells that match the samplenum)
                if ($("." + ShopFloorDCVerticalSample).length > 0) {
                    $('td[samplenum=' + ($(this).attr('samplenum')) + ']').attr('mouseover', 'true');
                } else {
                    $('td[samplenum=' + ($(this).parent().attr('samplenum')) + '] div').attr('mouseover', 'true');
                    $("." + ShopFloorDCHorizontalDataTitle + "[samplenum=" + ($(this).parent().attr('samplenum')) + "]").attr('mouseover', 'true');
                }
            }
        },

        onCellMouseOut: function (e) {
            //Desktop
            if (typeof (__page) !== 'undefined' && !__page.get_isResponsive()) {
                ShopFloorDCVerticalSample = 'ShopFloorDCVerticalSample';
                ShopFloorDCHorizontalDataTitle = 'ShopFloorDCHorizontalDataTitle';
            }//Responsive
            else {
                ShopFloorDCVerticalSample = 'ShopFloorDCVerticalSampleResp';
                ShopFloorDCHorizontalDataTitle = 'ShopFloorDCHorizontalDataTitleResp';

            }

            if ($(this).attr('samplenum') >= 0 || $(this).parent().attr('samplenum') >= 0) {
                //select the current sample (all cells that match the samplenum)
                if ($("." + ShopFloorDCVerticalSample).length > 0) {
                    $('td[samplenum=' + ($(this).attr('samplenum')) + ']').removeAttr('mouseover');
                } else {
                    $('td[samplenum=' + ($(this).parent().attr('samplenum')) + '] div').removeAttr('mouseover');
                    $("." + ShopFloorDCHorizontalDataTitle + "[samplenum=" + ($(this).parent().attr('samplenum')) + "]").removeAttr('mouseover');
                }
            }
        }
    };

    $.fn.computation = function (method) {
        if (methods[method])
            return methods[method].apply(this, Array.prototype.slice.call(arguments, 1));
        else if (typeof method === 'object' || !method)
            return methods.init.apply(this, arguments);
        else
            $.error('Method ' + method + ' does not exist on jQuery.computation');
    };
})(jQuery);

$.fn.hasOverflow = function () {
    var $this = $(this);
    var $children = $this.find('*');
    var len = $children.length;

    if (len) {
        var maxWidth = 0;
        var maxHeight = 0;
        $children.map(function () {
            maxWidth = Math.max(maxWidth, $(this).outerWidth(true));
            maxHeight = Math.max(maxHeight, $(this).outerHeight(true));
        });

        return maxWidth > $this.width() || maxHeight > $this.height();
    }

    return false;
};

$.fn.hasOverflow = function () {
    var $this = $(this);
    var $children = $this.find('*');
    var len = $children.length;

    if (len) {
        var maxWidth = 0;
        var maxHeight = 0;
        $children.map(function () {
            maxWidth = Math.max(maxWidth, $(this).outerWidth(true));
            maxHeight = Math.max(maxHeight, $(this).outerHeight(true));
        });

        return maxWidth > $this.width() || maxHeight > $this.height();
    }

    return false;
};

(function ($) {
    var $instructionText;
    var $scrollPane;
    var isResponsive = false;
    var methods = {
        init: function (options) {
            $instructionText = this;
            var instruction = $(".instruction", this);
            var isExpanded = instruction.attr("defaultstate") == "Expanded" ? 1 : 0;
            var isExpandFull = instruction.attr("expandfull") == "True" ? 1 : 0;
            if (isExpanded) {
                instruction.addClass('instruction-show');
            }

            $(".top", $instructionText).append("<div id='icon' class='expander'/> ");
            //Collapse Expand
            // Desktop mode
            isResponsive = $(document.body).hasClass("cs-responsive");
            if (!isResponsive) {
                $(".instruction-text .expander", $instructionText).bind('click.div', methods.onExpanderClick);
                $(".instruction-text .expander", $instructionText).bind('click.div', methods.adjustText);
            }
            else if (isResponsive) {
                $(".instruction-text .title", $instructionText).off('click.div');
                $(".instruction-text .title", $instructionText).on('click.div', methods.onExpanderClick);
                //$('.caret-right', $instructionText).bind('click.div', methods.onExpanderClick);
            }
            $(".instructiontext-hidden", $instructionText).bind('change', methods.adjustText).triggerHandler('change');

            //Scrolling
            if (!isExpandFull) { //ExpandFull is disabled

                instruction.addClass('instruction-fixed');

                if (instruction.hasOverflow()) { //hasOverflow, enable scrolling
                    $scrollPane = $(".scroll-pane", this);
                    $(".instruction-text .expander", $instructionText).bind('click.div', methods.reinitScrollPanel);
                    if (isExpanded) {
                        methods.reinitScrollPanel();
                    }
                }
            }
        },
        onExpanderClick: function (e) {
            //check the state of the control and change css as necessary depending on the state

            if ($(".instruction-text[collapsed]").length > 0) {
                $(".instruction-text[collapsed]").removeAttr("collapsed");

                //set hidden field value that tracks expanded/collapsed state for the viewstate
                $(".instruction-text-state > input").val(1);
                if ($('.instruction-text .instruction .jspPane').length == 0)
                    $('.instruction-text .instruction').addClass('instruction-show');
            } else {
                $(".instruction-text").attr("collapsed", "true");

                //set hidden field value that tracks expanded/collapsed state for the viewstate
                $(".instruction-text-state > input").val(0);
                if ($('.instruction-text .instruction .jspPane').length == 0)
                    $('.instruction-text .instruction').removeClass('instruction-show');
            }

            // toggle expand caret
            if (isResponsive) {
                var caretDivDown = $('.caret-down');
                var caretDivRight = $('.caret-right');
                if (typeof (caretDivDown) !== 'undefined' && caretDivDown.length) {
                    caretDivDown
                        .removeClass('caret-down')
                        .addClass('caret-right');
                }
                else if (typeof (caretDivRight) !== 'undefined' && caretDivRight.length) {
                    caretDivRight
                        .removeClass('caret-right')
                        .addClass('caret-down');
                }
            }
        },

        adjustText: function () {

            $(".instruction-text .expander", $instructionText).adjustFonts();

        },

        reinitScrollPanel: function () {

            //this reinits the size of the scroll panel in the case of the instruction control expanding 
            var api = $scrollPane.data('jsp');
            if (api != null) {
                api.reinitialise();
            }

            if ($(".instruction-text[collapsed]").length > 0) {
                $(".instruction-text .jspVerticalBar").attr("collapsed", "true");
            }
            else {
                $(".instruction-text .jspVerticalBar").removeAttr("collapsed");
                var width = $(".instruction-text .jspPane").width() - 15;
                $(".instruction-text .jspPane").width(width);
            }

        }
    };

    $.fn.instructionText = function (method) {
        if (methods[method])
            return methods[method].apply(this, Array.prototype.slice.call(arguments, 1));
        else if (typeof method === 'object' || !method)
            return methods.init.apply(this, arguments);
        else
            $.error('Method ' + method + ' does not exist on jQuery.instructionText');
    };
})(jQuery);

// This adjust the fonts that are generated by infragistics use of deprecated font tags in their rich text editor on the TaskList modeling form.
// CSS styles in the portal override these font tags, so this sets the intended values on the elements themselves
(function ($) {

    $.fn.adjustFonts = function () {

        $("font").each(function (index) {
            var fontSize = 0;

            if ($(this).attr("size")) {
                fontSize = ((Math.round(parseInt($(this).attr("size")) * 12 * 0.6)) - 10);
                if (fontSize < 10) { fontSize = 10; }

                $(this).css("font-size", fontSize.toString() + 'px');
                $(this).find('*').css("font-size", fontSize + 'px');
            }
        });

        $("em").each(function (index) {
            $(this).css("font-style", "italic");
            $(this).find('*').css("font-style", "italic");
        });

        $("strong").each(function (index) {
            $(this).css("font-weight", "bold");
            $(this).find('*').css("font-weight", "bold");
        });

        $("h1").each(function (index) {
            $(this).css("font-size", '25px');
            $(this).find('*').css("font-size", '25px');
            $(this).css("font-weight", "bold");
            $(this).find('*').css("font-weight", "bold");
        });

        $("h2").each(function (index) {
            $(this).css("font-size", '20px');
            $(this).find('*').css("font-size", '20px');
            $(this).css("font-weight", "bold");
            $(this).find('*').css("font-weight", "bold");
        });

        $("h3").each(function (index) {
            $(this).css("font-size", '16px');
            $(this).find('*').css("font-size", '16px');
            $(this).css("font-weight", "bold");
            $(this).find('*').css("font-weight", "bold");
        });

        $("h4").each(function (index) {
            $(this).css("font-size", '12px');
            $(this).find('*').css("font-size", '12px');
            $(this).css("font-weight", "bold");
            $(this).find('*').css("font-weight", "bold");
        });

        $("h5").each(function (index) {
            $(this).css("font-size", '10px');
            $(this).find('*').css("font-size", '10px');
            $(this).css("font-weight", "bold");
            $(this).find('*').css("font-weight", "bold");
        });

        $("h6").each(function (index) {
            $(this).css("font-size", '8px');
            $(this).find('*').css("font-size", '8px');
            $(this).css("font-weight", "bold");
            $(this).find('*').css("font-weight", "bold");
        });

    };
})(jQuery);

(function ($) {
    var $ribbonContainer;

    var methods = {
        init: function (options) {
            $ribbonContainer = this;

            //bind events to the click action of the header
            $(".ribbon-toggle-div", $ribbonContainer).bind('click.div', methods.onHeaderClick);

        },

        onHeaderClick: function (e) {

            //check the state of the control and change css as necessary depending on the state
            if ($(".ribbon-container[collapsed]", $ribbonContainer).length > 0) {
                $(".ribbon-container[collapsed]", $ribbonContainer).show();
                $(".ribbon-container,.ribbon-toggle-div").removeAttr("collapsed");
                $(".ribbon-toggle-div > .ribboncontainer-expandedlabel").show();
                $(".ribbon-toggle-div > .ribboncontainer-collapsedlabel").hide();

                //set hidden field value that tracks expanded/collapsed state for the viewstate
                $(".ribbon-container > input").val(1);

                //customized scrollbar re-init
                var api = $(".ribbon-container .content").data('jsp');
                api.reinitialise();
                this.parentNode.control.expand();
            }
            else {
                $(".ribbon-container", $ribbonContainer).hide();
                $(".ribbon-container,.ribbon-toggle-div").attr("collapsed", "true");
                $(".ribbon-toggle-div > .ribboncontainer-collapsedlabel").show();
                $(".ribbon-toggle-div > .ribboncontainer-expandedlabel").hide();

                //set hidden field value that tracks expanded/collapsed state for the viewstate
                $(".ribbon-container > input").val(0);
                this.parentNode.control.collapse();
            }
        }
    };

    $.fn.ribbonContainer = function (method) {
        if (methods[method])
            return methods[method].apply(this, Array.prototype.slice.call(arguments, 1));
        else if (typeof method === 'object' || !method)
            return methods.init.apply(this, arguments);
        else
            $.error('Method ' + method + ' does not exist on jQuery.ribbonContainer');
    };
})(jQuery);

(function ($) {

    var selectedButtonId = '';
    var selectedButtonText = '';

    var methods = {
        init: function (options) {

            $(".body-opview-scan .webpart-actions .action > input").bind('click.opviewscanactions', methods.onDownClick);

            if (options.clearSelected.length > 0 && options.clearSelected == 'True') {
                selectedButtonId = '';
                selectedButtonText = '';
            }

            $(".webpart-containerstatus .cs-picklist input:text").change(function () {
                selectedButtonId = '';
                selectedButtonText = '';
            });

            //bind to the load element of the txn page iframe
            $(".body-opview-panel > iframe").on('load', function () {

                // Handler for iframe .load() called - append class to body to hide the container status web part
                var $body = $(this).contents().find("body");
                $body.addClass("pagepanel-eproc");
                $body.find(".webpart-containerstatus").hide();

                //this makes the transaction iframe appear after it has loaded and had the above css applied to hide
                //the container header wp. Using css/attribute here instead of show() for IE9 rendering issues
                $(".body-opview-panel").attr("loaded", "true");
                $(".body-opview-panel").css({ display: 'block' });

                //set button color so the user can tell which button/page is selected
                if (selectedButtonId != '') {
                    $("#" + selectedButtonId, ".webpart-actions").addClass("cs-grd-06");
                }

                //set label name title of txn area to the button text
                if (selectedButtonText != '') {
                    $(".ui-webpart-opviewscan-txnname .transaction-name-label .txn-name").html(selectedButtonText);
                }

                $(".opviewactions-span").click();
                //focus iframe child elements as soon as container status web part is hidden to exclude its childs
                var page = this.contentWindow.__page;
                if (page)
                    page.setFocusOnLoad();
            });
        },

        onDownClick: function (e) {
            selectedButtonId = $(this).attr('id');
            selectedButtonText = $(this).val();
        }

    };

    $.fn.opviewscanactions = function (method) {
        if (methods[method])
            return methods[method].apply(this, Array.prototype.slice.call(arguments, 1));
        else if (typeof method === 'object' || !method)
            return methods.init.apply(this, arguments);
        else
            $.error('Method ' + method + ' does not exist on jQuery.opviewscanactions');
    };
})(jQuery);

(function ($) {

    var methods = {
        init: function (options) {
            var itemtext = '';
            if (options.itemstext.length > 0)
                itemtext = options.itemstext;

            //For EProc: bind to the load element of the view doc iframe to display the total number of items after the iframe renders
            var taskFlyoutContainer = typeof (__page) !== 'undefined' && !__page.get_isResponsive()
                ? $("div#ctl00_WebPartManager_TaskMenuWP_FlyoutViewDocsPanel.ui-flyout-panel iframe")
                : $("div#ctl00_WebPartManager_EProcDCWP_FlyoutViewDocsContainer.ui-flyout-container iframe");

            taskFlyoutContainer.on('load', function () {
                if ($(this).is(":visible")) {

                    var flyoutitems = typeof (__page) !== 'undefined' && !__page.get_isResponsive()
                        ? $('div#ctl00_WebPartManager_EProcDCWP_FlyoutViewDocsContainer.ui-flyout-container')
                        : $('#ctl00_WebPartManager_TaskMenuWP_FlyoutViewDocsContainer.ui-flyout-container');

                    flyoutitems.find(".flyoutdropdown-itemnum").remove();
                    var $body = $(".ui-flyout-panel > iframe").contents().find("body");

                    // doc flyouts for classic desktop                  
                    if (typeof (__page) !== 'undefined' && !__page.get_isResponsive()) {
                        //find all instances of the document class in the iframe after it finishes loading to get the count
                        var docCount = $("span#ctl00_WebPartManager_ViewDocWP_ViewDocControl", $body).find('.document').length;

                        $('<div id=flyout_numbertext class="flyoutdropdown-itemnum">' + docCount
                            + ' ' + itemtext
                            + '</div>')
                            .appendTo('div#ctl00_WebPartManager_TaskMenuWP_FlyoutViewDocsContainer.ui-flyout-container');
                    }
                    else if (typeof (__page) !== 'undefined' && __page.get_isResponsive()) {
                        // TODO: Refactor
                        // set header property
                        var $hdr = $('div[id$="FlyoutViewDocsHeader"]');
                        var labels = [{ Name: 'DocumentSet_DocumentEntries' }];
                        __page.getLabels(labels, function (response) {
                            if ($.isArray(response)) {
                                var docLabel = response.find(function (l) {
                                    return l.Name === 'DocumentSet_DocumentEntries';
                                });
                                if (docLabel && docLabel.Value) {
                                    $hdr.text(docLabel.Value);
                                }
                                else {
                                    $hdr.text('Documents');
                                }
                            }
                        });
                        // update flyout container class positioning
                        var top = 0;
                        var left = $('.column2-m[selected="true"]').offset().left; // width of scroll panel padding

                        $('#ctl00_WebPartManager_EProcDCWP_FlyoutViewDocsContainer.ui-flyout-container')
                            .addClass('ui-flyout-container-m')
                            .css('top', top + 'px')
                            .css('left', left + 'px')
                            .css('margin', '0');

                        var $taskmenuWP = $('.webpart-taskmenu');
                        $('.matrix', $taskmenuWP).addClass('task-menu-flyout-m');
                    }
                }
            });
        }
    };

    $.fn.viewdocflyout = function (method) {
        if (methods[method])
            return methods[method].apply(this, Array.prototype.slice.call(arguments, 1));
        else if (typeof method === 'object' || !method)
            return methods.init.apply(this, arguments);
        else
            $.error('Method ' + method + ' does not exist on jQuery.viewdocflyout');
    };
})(jQuery);

$.widget("cs.scrollableTabs", $.ui.tabs, {
    _init: function () {
        var me = this;

        // add header
        this.element
            .append("<div class=cs-nav-tabs />")
            .append("<div class=cs-tab-pages id=\"main-tab-pages\" />");
        $(".cs-tab-pages", this.element).before($("> .ui-tabs-streak", this.element).detach());

        if (this.options.overflow == "scroll") {
            $(".cs-nav-tabs", this.element).append("<div class='tabsDownMenu secondlevelTabs' />");
        }

        $(".cs-nav-tabs", this.element).append("<div class=cs-nav-wrapper />");
        var __$ul = $("> ul", this.element);
        if (this.options.conciergeLvl && this.options.$conciergeZoneApollo.length) {
            $(".cs-nav-tabs", this.element).append("<div class='conciergeIcon secondlevelTabs'><img src='Themes/Horizon/images/navigation/icon-concierge-24.svg' class = 'navigation-concierge'></div>");
            var $firstLvlTableConcierge = $("#ConciergeZone_Content", this.options.$conciergeZoneApollo).children().eq(0);

            if (!$firstLvlTableConcierge.hasClass("zone")) {
                $firstLvlTableConcierge.addClass("zone");
                $firstLvlTableConcierge.css("display", "none");
            }
        }

        this._on({
            "click .conciergeIcon": function (ev) {
                this._trigger("clickConcierge");
                if (!$(getCEP_top().document.body).hasClass("Camstar-theme"))
                    PositioningConcierge();
            }
        });

        this._on({
            "mousedown .conciergeIcon": function (ev) {
                $(ev.currentTarget).addClass('pressed-concierge');
            }
        });

        this._on({
            "mouseup .conciergeIcon": function (ev) {
                $(ev.currentTarget).removeClass('pressed-concierge');
            }
        });


        if (!__$ul.length)
            __$ul = $("> #tabs-strip > ul", this.element);

        __$ul.detach().appendTo($(".cs-nav-wrapper", this.element));

        // load tabs-panel into the cs-tab-pages
        var $pages = $(".cs-tab-pages", this.element);
        $("> div.ui-tabs-panel", this.element).each(function (i, el) {
            $pages.append($(el).detach());
        });

        if (this.options.hideIfEmpty === true)
            $(".cs-nav-wrapper", this.element).css("visibility", "hidden");

        if (this.options.overflow === "scroll") {
            $(".tabsDownMenu", this.element)
                .append('<div class=tabsDownArrow /><div class="hiddenMenu tabsMenuBorder"><div class=tabsMenu></div>');

            // Initially hidden
            $(".tabsDownMenu", this.element).addClass("hidden");

            $('.tabsDownArrow', this.element).click(function () {
                var $mBorder = $(".tabsMenuBorder", me.element);
                var hasItems = me.tablist.children().length > 0;
                $mBorder.toggleClass("hiddenMenu", !hasItems);
                if (!$mBorder.is(":hidden")) {
                    $mBorder.hide();
                    return;
                }

                if (hasItems) {
                    $mBorder.show();
                    me._buildTabsMenu();
                }
            });

            //in Horizon theme tabsDown menu should be located on the right
            if (document.body.classList.contains("Horizon-theme")) {
                $('.tabsDownMenu', this.element).insertAfter($(".cs-nav-wrapper", this.element));
                $(".tabsDownArrow").append("More...");
            }

            $(".cs-nav-wrapper", this.element).after('<div class="tabsLeftRightArrows secondlevelTabs">' +
                '<span class="tabArrowWrapper"><span class="right" /></span>' +
                '<span class="tabArrowWrapper"><span class="left"  /></span>' +
                '</div>');

            $(".tabsLeftRightArrows span.tabArrowWrapper", this.element).click(function (e) {
                me._scrollHorizontally(e);
            });
            this.element.addClass("scroll-mode");
        }

        else if (this.options.overflow === "wrap") {
            this.element.addClass("wrap-mode");
        }

        this._on({
            "click a.ui-tabs-close": function (ev) {
                var tab = $(ev.currentTarget).closest("li");
                var panel = this._getPanelForTab(tab);
                var evdata = {
                    tab: tab,
                    panel: panel
                };
                evdata.panel = this._getPanelForTab(evdata.tab);

                if (this._trigger("beforeTabClose", ev, evdata) === true) {
                    this.closeTab(ev, tab, panel);
                }
            }
        });

        // Add check on window resize
        this._on(window, {
            "resize": function () {
                this.checkWidthOverflow();
                if (!$(getCEP_top().document.body).hasClass("Camstar-theme"))
                    PositioningConcierge();
            }
        });


        if (document.body.classList.contains("Horizon-theme") && !document.body.classList.contains("mobile")) {
            if ($(".tabs-navigation").attr("id") == "tabContainerControl") {
                $(".cs-tab-pages", this.element).append("<div class='empty-state' id='emp-state' />");
                $(".empty-state", this.element).append("<canvas id='empty-state-img' />");
                var emptyStateImg = document.getElementById("empty-state-img"),
                    ctx = emptyStateImg.getContext('2d'),
                    image = new Image(192, 192);
                image.src = 'Themes/Horizon/images/icons/typeComputer48.svg';
                image.onload = function () {
                    ctx.drawImage(image,
                        0,
                        0,
                        emptyStateImg.width,
                        emptyStateImg.height);
                }
                $(".empty-state", this.element).append("<div class='empty-state-txt'>No Data To Display.</>");
                $(".empty-state", this.element).append("<div class='empty-state-txt-txt2'>Select a page from the menu on the left.</>");
            }
        }

        return this._super();
    },

    _destroy: function () {
        this._off(window, "resize");
        this._off("click");
        this._super();
    },

    // close tab with validation
    closeTab: function (ev, tab, panel) {
        var $iframeWindow = $(">iframe", panel);

        let $popupFrame = $iframeWindow.contents().find('div.floating-frame iframe');
        let swacPopupIsOpen = $popupFrame.is(':visible') && $popupFrame.attr('src').toLowerCase().includes('swaccontainerpopupvp.aspx');

        // SWAC popup open?  We don't control what's in it - assume they may have unsaved changes
        if ($(tab).is(".is-dirty") || swacPopupIsOpen) {
            // Verify the panel is able to close
            if ($iframeWindow.length && $iframeWindow[0].contentWindow) {
                var page = $iframeWindow[0].contentWindow.__page;
                if (page) {
                    let tabTitle = null;
                    if (swacPopupIsOpen) {
                        // use SWAC popup title for warning, otherwise let the page figure out the tab title
                        tabTitle = $iframeWindow.contents().find('div.floating-frame').attr('frame-title');
                    }

                    var _tabsContext = this;
                    return page.isAllowClose(tab, tabTitle, function () {
                        _tabsContext.remove(null, tab, panel);
                    });
                }
            }
        }
        else
            this.remove(null, tab, panel);
    },


    remove: function (ev, $tab, $panel) {
        if (!$panel) {
            // Find panel for the tab
            $panel = $(".cs-tab-pages [aria-labelledby=\"" + $tab.attr("aria-labelledby") + "\"]", this.element);
        }

        // first remove panel
        var evdata = {
            tab: $tab,
            panel: $panel,
            panelId: null,
            panelSrc: null
        };
        var $iframe = $('>iframe', $panel);
        if ($iframe.length) {
            evdata.panelId = $iframe.prop("id");
            evdata.panelSrc = $iframe.prop('src');
        }

        $panel.remove();
        $tab.remove();
        if (this.tabs.length == 1)
            this._trigger("tabRemoving", ev, evdata);
        this.refresh();
        this.checkWidthOverflow();
        this._trigger("tabRemoved", ev, evdata);

        if (document.body.classList.contains("Horizon-theme"))
            this.updateHorizonMenu();
    },

    _getChildTabs: function ($li) {
        var id = $li.attr('aria-controls');
        var $divContent = $('div[id="' + id + '"]', $li.parent().parent());
        var $ifr = $divContent.find('iframe');
        if ($ifr.length)
            return $ifr.contents().find(".ui-page-tab").find('li');
        else
            return [];
    },

    add: function (tabPrm) {
        // tabPrm = {caption, url, callStackKey}        
        var $tab = this._createNewTab(tabPrm);

        $('ul', this.element).append($tab);

        // Create div container for the content
        var $iframeDiv = $('<div />');
        $iframeDiv.prop("id", $tab.attr('contentId'));
        $(".cs-tab-pages", this.element).append($iframeDiv);
        this.refresh();
        this._activate($tab.index());
        var cc = this._createContentContainer(tabPrm);
        cc.attr("aria-controls", $tab.attr("aria-controls"));

        $iframeDiv.append(cc);

        this.checkWidthOverflow();

        if (document.body.classList.contains("Horizon-theme"))
            this.updateHorizonMenu();

        return $tab;
    },

    checkWidthOverflow: function () {
        var $wrapper = $(".cs-nav-wrapper", this.element);
        if (this.options.overflow == "scroll") {
            var allowWidth = $wrapper.width();
            // calculate total width of ul>li elements
            //       $("> ul", $wrapper).width()   ----  DOESN'T work in IE11
            var ulWidth = 0;
            $(">ul>li", $wrapper).each(function () {
                ulWidth += $(this).outerWidth(true);
            });
            var isScroll = ulWidth > allowWidth;
            $wrapper.parent().toggleClass("scrolling", isScroll);
            if (!isScroll)
                this._scrollHorizontally(null, true);
        }

        $(".tabsDownMenu", this.element).toggleClass("hidden", this.tabs.length == 0);
        if (this.options.hideIfEmpty === true)
            $wrapper.css("visibility", this.tabs.length == 0 ? "hidden" : "visible");
        return isScroll;
    },

    getPanel: function ($tab, returnBody) {
        var $div = this._getPanelForTab($tab);
        if (returnBody === true && $div.length) {
            return $($div[0].firstChild.contentDocument.body);
        }
        return $div;
    },

    // Get tab by id or callStackKey or index
    getTab: function (searchPrm) {
        if (searchPrm) {
            if (searchPrm.id) {
                return $('>.cs-nav-tabs >.cs-nav-wrapper > ul > li[aria-controls="' + searchPrm.id + '"]', this.element);
            }
            else if (searchPrm.callStackKey) {
                return $('>.cs-nav-tabs >.cs-nav-wrapper > ul > li[aria-callstack="' + searchPrm.callStackKey + '"]', this.element);
            }
            else if (searchPrm.index) {
                if (index < this.tabs.length)
                    return this.tabs[index];
            }
        }
        return null;
    },

    setCaption: function ($t, text) {

        if (!$t)
            return;

        if (!text) {
            // caption from tab
            text = $(".tab-caption-text", $t).text();

            var $body = this.getPanel($t, true);
            if ($body.length) {
                $(".pageTitleMobile .pageTitle", $body).text(text);
            }
        }

        if ($t.length && text != null && !$t.hasClass("lockcaption")) {
            $(".tab-caption-text", $t).text(text);
        }
    },

    lockCaption: function ($t, isLocked) {

        if ($t && $t.length) {
            $t.toggleClass("lockcaption", isLocked);
        }
    },

    getTabProperties: function ($t) {
        if (this.tabs && this.tabs.length) {
            if ($t && $t.length) {
                var tabPrm = {
                    caption: "",
                    url: "",
                    callStackKey: $t.attr("aria-callstack"),
                    panel: this._getPanelForTab($t)
                };
                return tabPrm;
            }
        }
        return null;
    },

    getActiveTab: function () {
        return this.active;
    },

    _getDirtyTabs: function () {
        var flt = this.tabs.filter(function () {
            return $(this).hasClass("is-dirty");
        });
        return flt;
    },

    isDirty: function (ctlId) {
        var $tab;
        var findTabs = this.tabs.filter(function () {
            return $(this).attr("aria-controls") === ctlId;
        });
        if (findTabs.length)
            $tab = $(findTabs.get(0));

        if ($tab) {
            return $tab.hasClass("is-dirty");
        }
        return false;
    },

    setDirty: function (dirty, ctlId) {
        var $tab = this.active;
        if (ctlId) {
            var findTabs = this.tabs.filter(function () {
                return $(this).attr("aria-controls") === ctlId;
            });
            if (findTabs.length)
                $tab = $(findTabs.get(0));
        }
        var toggleIsDitry = true;
        var currentTabId = ctlId;
        if (currentTabId !== 'undefined') {
            var currentTab = $("li.ui-state-active", this.tablist);
            if (currentTab.length) {
                currentTabId = currentTab[0].getAttribute("aria-controls");
            }
        }
        var iframe = $("div #" + currentTabId + " > iframe");
        if (iframe.length) {
            var exportLeftPanel = $(".export-import-left-panel", iframe[0].contentWindow.document);
            toggleIsDitry = !exportLeftPanel.length;
        }
        if ($tab.length && toggleIsDitry) {
            $tab.toggleClass("is-dirty", dirty);
        }
        return this._getDirtyTabs().length;
    },

    _createNewTab: function (tabPrm) {
        var tabId = $(this.element).prop("id") + "_" + tabPrm.callStackKey;
        var $li = $("<li><a class=cs-tab-caption-link><span class=tab-caption-text /></a><a class=ui-tabs-close /></li>");
        $li.attr('contentId', tabId);
        $li.attr("aria-callstack", tabPrm.callStackKey);
        $("a.cs-tab-caption-link", $li).prop("href", '#' + tabId);
        $("span.tab-caption-text", $li).text(tabPrm.caption);
        $("a.ui-tabs-close", $li).prop("id", tabId + "_close").prop("href", "javascript:void(0);");
        $("a.ui-tabs-close", $li).off("keydown").on("keydown", (event) => {
            if (event.key === "Enter") {
                event.preventDefault();
                event.target.click();
            }
        });
        // add Dirty mark
        if (this.options.dirtyMark === true) {
            $li.children().first().append('<span class=dirty-mark>*</span>');
        }

        return $li;
    },

    _createContentContainer: function (tabPrm) {
        var $ifr = $("<iframe></iframe>");
        $ifr.prop("frameborder", "0")
            .prop("scrolling", "no")
            .prop("id", "tabfr_" + tabPrm.callStackKey)
            .prop("src", tabPrm.url)
            .addClass("cs-tabframe")
            .attr("helpurl", "");

        return $ifr;
    },

    _buildTabsMenu: function () {
        var me = this;
        var $tmenu = $('.tabsMenu', this.element);
        $tmenu.empty();
        this.tabs.each(function (i) {
            var $tc = $("<div class=tabsElement tab-index=" + i + "><span></span><a/></div>");
            $(">a", $tc).text($(".tab-caption-text", this).text());
            $tc.attr("aria-labelledby", $(this).attr("aria-labelledby"));

            //In Horizon theme, drop down menu will contain only tabs not fit to tab control visible area
            if (document.body.classList.contains("Horizon-theme")) {
                if ($(this).is(":hidden"))
                    $tmenu.prepend($tc);
                $tc.toggleClass("selectedTabItem", true);
            } else {
                $tc.toggleClass("selectedTabItem", $(this).hasClass("ui-state-active"));
                $tmenu.append($tc);
            }
        });

        if (document.body.classList.contains("Horizon-theme")) {
            $("div.tabsElement a", $tmenu).click(function (ev) {
                var $te = $(ev.target);
                if (!$te.is("[tab-index]"))
                    $te = $te.closest("[tab-index]");
                var index = $te.attr("tab-index");

                var $clickedTab = $(me.tabs[index]);
                $clickedTab.show();

                var $wrapper = $(".cs-nav-wrapper", me.element);
                var clickedElementInDOM = $(">ul>li", $wrapper)[index];

                $(clickedElementInDOM).appendTo($(">ul", $wrapper));

                me.refresh();
                me.updateHorizonMenu();
                me._activate($clickedTab.index());
                me._scrollHorizontally();
                me._closeTabMenu($tmenu);
                ev.stopPropagation();
            }
            );

            me._on($tmenu,
                {
                    "click div.tabsElement span": function (ev) {
                        var $te = $(ev.target);
                        if (!$te.is("[tab-index]"))
                            $te = $te.closest("[tab-index]");
                        var index = $te.attr("tab-index");

                        var $tab = $(this.tabs[index]);
                        var $panel = this._getPanelForTab($tab);
                        var evdata = { tab: $tab, panel: $panel };

                        if (me._trigger("beforeTabClose", ev, evdata) === true) {
                            me.closeTab(ev, $tab, $panel);
                            me._buildTabsMenu();
                        }
                    }
                });
        } else {

            me._on($tmenu, {
                "click div.tabsElement": function (ev) {
                    var $te = $(ev.target);
                    if (!$te.is("[tab-index]"))
                        $te = $te.closest("[tab-index]");
                    var index = $te.attr("tab-index");
                    this._activate(index);
                    this._scrollHorizontally();
                    this._closeTabMenu($tmenu);
                    ev.stopPropagation();
                }
            });

            $tmenu.append('<div id="closeAllTabs" class="tabsElement closeAllTabs"><a>Close All Tabs</a></div>');
            this._on($tmenu, {
                "click .closeAllTabs": function () {
                    var cnt = this.tabs.length;
                    for (var i = cnt - 1; i >= 0; i--) {
                        var ev = null;
                        var $t = $(this.tabs[i]);
                        var $panel = $(".cs-tab-pages [aria-labelledby=\"" + $t.attr("aria-labelledby") + "\"]", this.element);
                        me.remove(ev, $t, $panel);
                        this._closeTabMenu($tmenu);
                    }
                    this._scrollHorizontally(null, true);
                }
            });
        }
    },

    _closeTabMenu: function ($tmenu) {
        this._off($tmenu, "click");
        $tmenu.empty();
        $(".tabsMenuBorder", this.element).hide();
    },

    _scrollHorizontally: function (ev, reset) {
        var clickRight = true;
        if (ev && $(ev.target).hasClass("left"))
            clickRight = false;

        var $ul = $("ul.ui-tabs-nav", this.element);
        var container = $ul.parent().get(0);

        if (reset === true) {
            container.scrollLeft = 0;
            return;
        }

        var delta = $(container).width() / 5;           // Default scroll value is 20% of parent width
        if (!clickRight)
            delta = -delta;
        container.scrollLeft += delta;
    },

    updateHorizonMenu: function () {
        var availableWidth = this.getAvailableTabControlWidth();

        var totalTabsWidth = this.getTotalTabsWidth(true);

        var isScroll = totalTabsWidth > availableWidth;

        var $wrapper = $(".cs-nav-wrapper", this.element);
        $wrapper.parent().toggleClass("scrolling", isScroll);

        this.tabs.each(function () {
            $(this).show();
        });

        if (!isScroll) {
            this._scrollHorizontally(null, true);
        } else
            this.fitLastTabToScreen();
    },

    getAvailableTabControlWidth: function () {
        var $wrapper = $(".cs-nav-wrapper", this.element);
        return $wrapper.width();
    },

    getTotalTabsWidth: function (includeHidden) {
        var totalTabsWidth = 0;
        this.tabs.each(function () {
            if ($(this).is(":visible") || includeHidden)
                totalTabsWidth += $(this).outerWidth(true);
        });

        return totalTabsWidth;
    },

    fitLastTabToScreen: function () {
        //iterate backward and hide previous tabs to fit last tab
        var availableWidth = this.getAvailableTabControlWidth();
        var cnt = this.tabs.length;
        for (var i = cnt - 2; i >= 0; i--) {
            var $t = $(this.tabs[i]);
            $t.hide();

            var $wrapper = $(".cs-nav-wrapper", this.element);
            $t.appendTo($(">ul", $wrapper));

            var totalTabsWidth = this.getTotalTabsWidth(false);
            if (availableWidth > totalTabsWidth) {
                break;
            }
        }

        this.refresh();
    }
});
