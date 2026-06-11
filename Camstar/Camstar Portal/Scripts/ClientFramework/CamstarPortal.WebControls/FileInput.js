// Copyright Siemens 2023  

/// <reference name="MicrosoftAjax.js"/>
/// <reference path="~/Scripts/ClientFramework/Camstar.UI/Control.js" />
Type.registerNamespace("Camstar.WebPortal.FormsFramework.WebControls");
Camstar.WebPortal.FormsFramework.WebControls.FileInput = function (element)
{
    Camstar.WebPortal.FormsFramework.WebControls.FileInput.initializeBase(this, [element]);

    this._inputFieldId = null;
    this._hiddenFieldId = null;
    this._autoPostBack = null;   

    this.uploadingCompleted = false;
    this.eventTarget = '';
    this.eventArgument = '';
    this.additionalInput = '';    
    this._defaultValue = null;
};
Camstar.WebPortal.FormsFramework.WebControls.FileInput.prototype =
{
    initialize: function ()
    {
        Camstar.WebPortal.FormsFramework.WebControls.FileInput.callBaseMethod(this, 'initialize');        
        //clear existing file input controls
        $("span[id='" + this.get_id() + "']").has("form").remove();
        
        var fileInp = $('#' + this._inputFieldId);
        var fileName = $('#' + this._hiddenFieldId).val();

        
        if (!fileInp.parent().is('form')) {
 
            
            var form = $("<div id='fileupload" + this.get_id() + "'><form method='POST' action='DownloadFile.aspx?uploadFile=true&CallStackKey=" + getParameterByName("CallstackKey") + "' enctype='multipart/form-data' name='fileInputForm'><label class='cs-fileupload'></label></form></div>");
            fileInp.wrap(form);

            fileInp.closest(".cs-fileupload").before("<input id='fileuploadtext" + this.get_id() + "' type='text'  onclick=onSelectFileUploadTextbox(event," + this.get_id() + ",'cs-fileuploadtext') readonly class='cs-fileuploadtext' value='" + fileName + "'></input>");
            fileInp.before("<span class='cs-fileinput-button'>Choose File</span>");
            

            $("body").append('<div id="fileInputWarning" style="display: none"></div>');

            if (this._autoPostBack)
                $("input[id='fileuploadtext" + this.get_id() + "']").prop('autopostback', true);

	    var me = this;
	    $("input[id='fileuploadtext" + me.get_id() + "']").change(function ()
	    {
    		$('#' + me._hiddenFieldId).val($(this).val());
	    });

	    $("div[id='fileupload" + me.get_id() + "']").fileupload({
                dataType: 'json',
                start: function() {
                    $("#mod").show();
                },
                fail: function () {
                    $("#mod").hide();  
                    __page.getLabel('Lbl_SizeOfFileHasOutOfLimit', function (response) {
                        if ($.isArray(response)) {
                            alert(response[0].Value);
                        }
                        else {
                            alert(response.Error);
                        }
                    });                   
                },
                done: function (e, data) {
                    $("#mod").hide();

                    var result = data.result;
                    if (result.status == 1) { // successful file upload
                        var uploadtext = $("input[id='fileuploadtext" + me.get_id() + "']");
                        uploadtext.val(result.path);
                        $("#" + fileInp.prop("id") + "_Hidden").val(result.path);
                        if(uploadtext.prop("autopostback"))
                            __doPostBack(this.Id, "FileInput_Postback");
                    }
                    else
                    {
                        //error
                        var dialog = $(".ui-dialog");
                        var pager = $(".ui-pager-control[id^=pg_]");
                        // if control exists within a grid
                        if (dialog.length > 0 && pager.length > 0)
                        {
                            dialog.data('bubbleMessage', result.message);
                            $('#light_popup_dialog_button_Close', dialog).click();

                            if (result.status == 0)
                                alert(result.message);
                        }
                        else
                        {
                            alert(result.message);
                        }
                    }
                    
                }
            });
        }
    },
    dispose: function ()
    {
        Camstar.WebPortal.FormsFramework.WebControls.FileInput.callBaseMethod(this, 'dispose');
    },

    get_hiddenFieldId: function () { return this._hiddenFieldId; },
    set_hiddenFieldId: function (value) { this._hiddenFieldId = value; },

    get_inputFieldId: function () { return this._inputFieldId; },
    set_inputFieldId: function (value) { this._inputFieldId = value; },

    get_hiddenField: function () { return this._hiddenField; },
    set_hiddenField: function (value) { this._hiddenField = value; },

    get_inputField: function () { return this._inputField; },
    set_inputField: function (value) { this._inputField = value; },
    
    get_autoPostBack: function () { return this._autoPostBack; },
    set_autoPostBack: function (value) { this._autoPostBack = value; },

    get_defaultValue: function () { return this._defaultValue; },
    set_defaultValue: function (value) { this._defaultValue = value; }

},

Camstar.WebPortal.FormsFramework.WebControls.FileInput.registerClass('Camstar.WebPortal.FormsFramework.WebControls.FileInput', Camstar.UI.Control);
if (typeof (Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
