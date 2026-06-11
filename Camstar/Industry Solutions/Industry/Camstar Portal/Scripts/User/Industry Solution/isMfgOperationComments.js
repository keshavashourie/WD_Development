// Copyright Siemens 2022

(function () {
    'use strict';

    var api = CR.GetNamespace('CR.isMfgOperationComments');
    api.closeComments = closeComments;
    api.setLabels = setLabels;
    api.showComments = showComments;
    api.getMfgOperationComments = getMfgOperationComments;
    api.setMfgOperationComments = setMfgOperationComments;

    var labels = [];
    var order = {};
    var resource = '';
    var commentsChangedCallback;

    var IDS = {
        slideOut: 'ismfg-operation-comments',
        commentsLabel: '#iscomments',
        cancel: '#ismfg-operation-comments-cancel',
        save: '#ismfg-operation-comments-save',
        commentsInput: '#ismfg-operation-comments-input'
    };
    Object.freeze(IDS);

    var TEMPLATE = {
        slideOut:
            "<div id='ismfg-operation-comments-slideout' class='ismfgoperation-slideout' style='width: 365px'>\
                <div class='cs-textarea'>\
                    <span id='iscomments' class='cs-textbox cs-textarea block' > \
						<span class='cs-label' >Comment</span>\
					</span>\
					<div class='ismfgoperation-comment es-slide-out-content'>\
                    <textarea id='ismfg-operation-comments-input' height='400px' rows='5' cols='30'></textarea>\
					<br>\
					</div>\
                </div>\
                <div class='ismfgoperation-comments-buttonbar'>\
                    <div class='iszone-static' style='text-align:right'>\
                        <input type='button' class='cs-button-secondary' value='Cancel' id='ismfg-operation-comments-cancel'/>\
                        <input type='button' class='cs-button' value='Save' id='ismfg-operation-comments-save'/>\
                    </div>\
                </div>\
            </div>"
    };

    function closeComments() {
        CR.SlideOut.close(IDS.slideOut);
        $(IDS.save).hide();
    }

    function setLabels(data) {
        labels = data;
    }
    function getMfgOperationComments(successCallback, failCallback, selectedOrder, selectedResource, labels) {
        successCallback = successCallback;
        failCallback = failCallback;

        order = selectedOrder;
        resource = selectedResource;

        $(IDS.commentsLabel).text(labels["Comments"]);
        $(IDS.cancel).val(labels["Cancel"]);
        $(IDS.save).val(labels["Save"]);
        $(IDS.save).show();

        if (CR.SlideOut.isOpen(IDS.slideOut)) {
            closeComments();
        } else {
            let getCommentParams = {
                resourceName: resource,
                workflowStepId: order.WorkflowStepId,
                mfgOrderId: order.MfgOrder,
                preactorScheduledOrdersId: order.psoId
            };

            $.ajax({
                type: "POST",
                dataType: "json",
                url: './MfgOperationService.svc/web/GetMfgOperationComments',
                headers: {
                    'Accept': 'application/json'
                },
                contentType: "application/json;charset=UTF-8",
                async: true,
                data: JSON.stringify(getCommentParams),
                context: document.body
            })
                .success(successCallback)
                .fail(failCallback);
        }
    }

    function showComments(selectedOrder, selectedResource, labels, onCommentsChanged) {

        commentsChangedCallback = onCommentsChanged;

        $(IDS.commentsLabel).text(labels["Comments"]);
        $(IDS.cancel).val(labels["Cancel"]);
        $(IDS.save).val(labels["Save"]);
        $(IDS.save).show();

        if (CR.SlideOut.isOpen(IDS.slideOut)) {
            closeComments();
        } else {
            getMfgOperationComments(openComments, null, selectedOrder, selectedResource, labels);
        }

        function openComments(data) {
            if (data.Success) {
                var $form = $(TEMPLATE.slideOut);
                var $input = $form.find(IDS.commentsInput);
                $input.val(data.Data);
                var $savebutton = $form.find(IDS.save);
                $savebutton.click(saveComments);
                var $cancelButton = $form.find(IDS.cancel);
                $cancelButton.click(closeComments);
                CR.SlideOut.open(
                    $('#TemplateContentDiv'),
                    IDS.slideOut,
                    labels["Comments"],
                    //TEMPLATE.slideOut,
                    $form,
                    '365px',
                    'right',
                    null,
                    null,
                    true,
                    $('#TemplateContentDiv'));
            }
        }
    }

    // click handler for save button
    function saveComments() {
        var comments = $(IDS.commentsInput).val();
        setMfgOperationComments(successHandler, null, order.MfgOrder, order.WorkflowStepId, resource, order.psoId, comments);

        function successHandler(result) {
            if (result.Success && commentsChangedCallback) {
                __page.displayStatus(result.Message, "Success");
                result.Data = comments;
                commentsChangedCallback(result);
                closeComments();
            }
        }
    }

    // handler for save comment
    function setMfgOperationComments(successCallback, failCallback, mfgOrderId, workflowStepId, resourceName, psoId, comments) {
        successCallback = successCallback;
        failCallback = failCallback;

        let setCommentParams = {
            mfgOrderId: mfgOrderId,
            workflowStepId: workflowStepId,
            resourceName: resourceName,
            preactorScheduledOrdersId: psoId,
            comments: comments
        };
        console.log(setCommentParams)

        $.ajax({
            type: "POST",
            dataType: "json",
            url: './MfgOperationService.svc/web/SetMfgOperationComments',
            headers: {
                'Accept': 'application/json'
            },
            contentType: "application/json;charset=UTF-8",
            async: true,
            data: JSON.stringify(setCommentParams),
            context: document.body
        })
            .success(successCallback)
            .fail(failCallback);
    }

})();