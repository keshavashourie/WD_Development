// Copyright Siemens 2023  

/// <reference name="MicrosoftAjax.js"/>
/// <reference path="~/Scripts/jquery/jquery.min.js" />
/// <reference path="~/Scripts/jquery/jquery-ui.min.js" />
/// <reference path="~/Scripts/ClientFramework/Camstar.UI/Control.js" />
/// <reference path="~/Scripts/ClientFramework/Camstar.WebPortal.PortalFramework/WebPartBase.js" />

Type.registerNamespace("Camstar.WebPortal.WebPortlets.ImportExecutionConsole");
Type.registerNamespace("Camstar.WebPortal.WebPortlets.ExportExecutionConsole");

Camstar.WebPortal.WebPortlets.ExecutionConsole = function (element)
{
    Camstar.WebPortal.WebPortlets.ExecutionConsole.initializeBase(this, [element]);
    this._TransferType = null;
    this._StartExportBtn = null;
    this._StartImportBtn = null;
    this._HiddenExportImportName = null;
    this._TimeoutId = null;
    this._StartExecutionConsole = null;
    this._MaxRefreshAttempt = null;
    this._refreshAttempt = null;
    this._successCount = null;
    this._consoleViewBox = null;
    this._ProgressWindowID = null;
    this._progressDoc = null;
    this._progressWin = null;
    this._iframeHeight = null;
    this._keepSessionAlive = 0;
    this._prepareMsg = null;
    this._timeoutErrorMsg = null;
    this._errorProcessingMsg = null;
    this._instanceCompleteMsg = null;
    this._doneMsg = null;
    this._successMsg = null;
    this._errorMsg = null;
};

Camstar.WebPortal.WebPortlets.ExecutionConsole.prototype = {
    initialize: function ()
    {
        Camstar.WebPortal.WebPortlets.ExecutionConsole.callBaseMethod(this, 'initialize');

        var me = this;

        var lbls = [{ Name: 'Lbl_DT_PreparingForExportImport' }, { Name: 'Lbl_DT_TimeoutExportImport' }, { Name: 'Lbl_DT_ErrorProcessing' }, { Name: 'Lbl_DT_InstanceComplete' }, { Name: 'Lbl_DT_Done' }, { Name: 'StatusMessage_Success' }, { Name: 'StatusMessage_Error' }];
        
        if (this.get_ProgressWindowID())
        {
            if (this.get_StartExportBtn())
                _TransferType = "Export";
            if (this.get_StartImportBtn())
                _TransferType = "Import";
            if (this.get_ActivatePackageBtn())
                _TransferType = "Activation";

            var $progressSpan = $('#' + this.get_ProgressWindowID());
            if ($progressSpan.length)
            {
                var $ifr = $progressSpan.find('iframe');
                if ($ifr.length == 0)
                {
                    $('<div><iframe src="about:blank"></iframe></div>').appendTo($progressSpan);
                    $('div', $progressSpan).height(440).css('border', '1px solid black').width($progressSpan.width()-4);
                    $ifr = $progressSpan.find('iframe');
                    $ifr[0].height = $('div', $progressSpan).height() + 'px';
                    $ifr[0].width = $('div', $progressSpan).width() + 'px';
                }
                this._progressWin = $ifr[0].contentWindow;
                this._progressDoc = $ifr[0].contentDocument;

                this._iframeHeight = $('div', $progressSpan).height();
            }
        }

        if (this._StartExecutionConsole)
        {
            // Get Page Labels
            this.getLabels(lbls);
            // disable all buttons
            $('#WebPart_NavigationButtonsBar :submit').prop("disabled", true);
        }
        $('[id$="_DownloadBtn"]').prop('disabled', true).click(function ()
        {
            $('#WebPart_NavigationButtonsBar :submit').prop("disabled", false);
            me._DownloadExportedFile($('#' + me._HiddenExportImportName).val() + ".xml");
            return false;
        });
    },

    _StartRefreshConsole: function (e)
    {
        var me = this;

        if (this._progressDoc)
        {
            this._progressDoc.open();            
            var msg = this._prepareMsg.replace('{0}', _TransferType);
            this._writeLog(msg);
        }

        this._refreshAttempt = 0;
        this._successCount = 0;
        this._TimeoutId = setInterval(function ()
        {
            me._RefreshConsole();
            me._RefreshSession();
        }, 5000);
    },

    _StopRefreshConsole: function (e)
    {
        if (this._progressDoc)
        {
            this._progressDoc.write('</div>');
            this._progressDoc.close();
        }

        if (this._TimeoutId)
            clearInterval(this._TimeoutId);

        // enable navigation buttons
        $('#WebPart_NavigationButtonsBar :submit').prop("disabled", false);

        $('#' + this._StopExportBtn).prop("disabled", true);
    },

    _RefreshConsole: function (e)
    {        
        if (this._refreshAttempt > this._MaxRefreshAttempt)
        {           
            this._writeLog(this._timeoutErrorMsg);
            this._StopRefreshConsole();
        }
        else
        {
            this._refreshAttempt++;
            var exportName = $('#' + this._HiddenExportImportName);
            if (exportName && exportName.val())
            {
                var transition = new Camstar.Ajax.Transition(eval(Camstar.Ajax.RequestType.Command), this);
                transition.set_command("GetTransferStatus");
                transition.set_commandParameters(exportName.val());
                transition.set_clientCallback("_RefreshConsoleCallBack");
                var communicator = new Camstar.Ajax.Communicator(transition, this);
                communicator.syncCall();
            }
        }
    },

    _writeLog: function(s)
    {
        if (this._progressDoc)
        {
            this._progressDoc.writeln(s);
        }
    },

    _RefreshConsoleCallBack: function (response)
    {
        var me = this;
        var complete = false;
        var isSuccess = false;

        if(response.Data != null)
            isSuccess = response.Data.IsSuccess;

        if (!isSuccess)
        {
            this._writeLog(response.Data.Message + " ... <br>");
        }
        else
        {
            var logrecs = eval(response.Data.Message);
            
            me._progressDoc.close();
            me._progressDoc.open();      
            var msg = this._prepareMsg.replace('{0}', _TransferType);
            this._writeLog(msg);
            
            var complete = false;
            var error = false;
            var logErrorMessage = "";
            var i = 0;
            logrecs.some(function (l, i) {
                if (l.RecordType == 'complete')
                    complete = true;
                if (l.Status == "2") {
                    error = true;
                    if (l.Message != null)
                        logErrorMessage = l.Message + '<br>';
                    else
                        logErrorMessage = me._errorProcessingMsg + me._encode(l.ObjectName);
                    //exit out of loop
                    return;
                }

                var index = i;
                if (!complete) index = i + 1;
                if (l.RecordType == 'log')
                    me._writeLog((index) + ' - ' + l.ProcTime + ' ' + l.Status + ' ' + l.InstanceType + ' <b>' + me._encode(l.ObjectName) + '</b><br>');
                if (l.RecordType == 'idle')
                    me._writeLog(l.Status + '<br>');
            });
            
            if (me._successCount != logrecs.length)
            {
                me._successCount = logrecs.length;
                me._refreshAttempt = 0;
            }

            //complete
            if (complete)
            {
                __page.displayStatus(this._doneMsg, 'Success', this._successMsg);
                $('[id$="_DownloadBtn"]').prop('disabled', false);
                var msg = this._instanceCompleteMsg.replace('{0}', _TransferType);
                me._writeLog(msg);
                me._StopRefreshConsole();
            }
            //error
            else if (error) {
                __page.displayStatus(logErrorMessage, 'Error', this._errorMsg);
                me._writeLog(logErrorMessage);
                me._StopRefreshConsole();
            }

            // scroll window down
            var Yoffset = this._progressDoc.body.scrollHeight - this._iframeHeight;
            if (Yoffset > 0)
                this._progressWin.scrollBy(0, Yoffset);
        }
    },

    _DownloadExportedFile: function (fileName)
    {
        var iframe = $('#DownloadIframe');
        if (iframe.length < 1)
            iframe = $('<iframe id="DownloadIframe"/>');
        iframe.attr('src', "DownloadFile.aspx?viewdocfile=" + fileName);
        iframe.hide();
        iframe.appendTo(document.body);

        $('[id$="_DownloadBtn"]').prop('disabled', true);
    },

    _DownloadExportedFile1: function (e)
    {
        var exportName = $('#' + this._HiddenExportImportName);
        if (exportName && exportName.val())
        {
            var transition = new Camstar.Ajax.Transition(eval(Camstar.Ajax.RequestType.Command), this);
            transition.set_command("DownloadExport");
            transition.set_commandParameters(exportName.val());
            transition.set_clientCallback("_DownloadExportedFileCallBack");

            var communicator = new Camstar.Ajax.Communicator(transition, this);
            communicator.syncCall();
        }
    },

    _DownloadExportedFileCallBack: function (response)
    {
        if (response && response.Data)
        {
            var iframe = $('#DownloadIframe');
            if (iframe.length < 1)
                iframe = $('<iframe id="DownloadIframe"/>');
            iframe.attr('src', "DownloadFile.aspx?viewdocfile=" + response.Data.Message);
            iframe.hide();
            iframe.appendTo(document.body);
        }
    },

    _CancelExport: function (e)
    {
        var exportName = $('#' + this._HiddenExportImportName);
        if (exportName && exportName.val())
        {
            var transition = new Camstar.Ajax.Transition(eval(Camstar.Ajax.RequestType.Command), this);
            transition.set_command("CancelExport");
            transition.set_commandParameters(exportName.val());
            transition.set_clientCallback("_CancelExportCallBack");

            var communicator = new Camstar.Ajax.Communicator(transition, this);
            communicator.syncCall();
        }
    },

    _CancelExportCallBack: function (response)
    {
        if (response && response.Data)
        {
            // enable navigation buttons
            $('#WebPart_NavigationButtonsBar :submit').prop("disabled", false);
        }
    },

    _encode: function (s)
    {
        if (s.indexOf('&') != -1)
            s = s.replace('&', '&amp;');
        if (s.indexOf('<') != -1)
            s = s.replace(/</g, '&lt;');
        if (s.indexOf('>') != -1)
            s = s.replace(/\>/g, '&gt;');

        return s;
    },

    _RefreshSession: function() {
        if (this._keepSessionAlive > 0)
            parent.camstar.sessionTimeout.resetSessionTime();
    },

    dispose: function ()
    {
        this._StartExportBtn = null;
        this._HiddenExportImportName = null;
        this._TimeoutId = null;
        this._StartExecutionConsole = null;
        this._MaxRefreshAttempt = null;
        Camstar.WebPortal.WebPortlets.ExecutionConsole.callBaseMethod(this, 'dispose');
    },

    get_ProgressWindowID: function () { return this._ProgressWindowID; },
    set_ProgressWindowID: function (value) { this._ProgressWindowID = value; },

    get_StartExportBtn: function () { return this._StartExportBtn; },
    set_StartExportBtn: function (value) { this._StartExportBtn = value; },

    get_StartImportBtn: function () { return this._StartImportBtn; },
    set_StartImportBtn: function (value) { this._StartImportBtn = value; },

    get_ActivatePackageBtn: function () { return this._ActivatePackageBtn; },
    set_ActivatePackageBtn: function (value) { this._ActivatePackageBtn = value; },

    get_StopExportBtn: function () { return this._StopExportBtn; },
    set_StopExportBtn: function (value) { this._StopExportBtn = value; },

    get_HiddenExportImportName: function () { return this._HiddenExportImportName; },
    set_HiddenExportImportName: function (value) { this._HiddenExportImportName = value; },

    get_StartExecutionConsole: function () { return this._StartExecutionConsole; },
    set_StartExecutionConsole: function (value) { this._StartExecutionConsole = value; },

    get_MaxRefreshAttempt: function () { return this._MaxRefreshAttempt; },
    set_MaxRefreshAttempt: function (value) { this._MaxRefreshAttempt = value; },
    
    get_KeepSessionAlive: function () { return this._keepSessionAlive; },
    set_KeepSessionAlive: function (value) { this._keepSessionAlive = (value >= 0) ? value : 0; },

    getLabels: function (lbls) {
        var me = this;
        __page.getLabels(lbls, function (rspnse) {
            if ($.isArray(rspnse)) {                
                $.each(rspnse, function () {
                    var lblName = this.Name;
                    var lblValue = this.Value;
                    switch (lblName)
                    {
                        case 'Lbl_DT_PreparingForExportImport':
                            me._prepareMsg = lblValue;
                            break;
                        case 'Lbl_DT_TimeoutExportImport':
                            me._timeoutErrorMsg = lblValue;
                            break;
                        case 'Lbl_DT_ErrorProcessing':
                            me._errorProcessingMsg = lblValue;
                            break;
                        case 'Lbl_DT_InstanceComplete':
                            me._instanceCompleteMsg = lblValue;
                            break;
                        case 'Lbl_DT_Done':
                            me._doneMsg = lblValue;
                        case 'StatusMessage_Success':
                            me._successMsg = lblValue;
                            break;
                        case 'StatusMessage_Error':
                            me._errorMsg = lblValue;
                            break;
                        default:
                            break;
                    }
                });
                me._StartRefreshConsole();
            }
            else {
                alert(rspnse.Error);
            }
        });
    }

};

Camstar.WebPortal.WebPortlets.ExecutionConsole.registerClass('Camstar.WebPortal.WebPortlets.ExecutionConsole', Camstar.WebPortal.PortalFramework.WebPartBase);
