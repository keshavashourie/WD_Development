// Copyright Siemens 2023  
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using DT = Camstar.WebPortal.WebPortlets.DataTransfer;
using Siemens.OPCR.Diagnostics;

namespace Camstar.WebPortal.WebPortlets.DataTransfer
{
    /// <summary>
    /// Summary description for ExecutionConsole
    /// </summary>
    public class ExportExecuteConsoleWP : MatrixWebPart
    {

        private LogClient _log = null;

        private void LogMessage(LogSeverity level, string method, string message)
        {
            if (_log == null)
            {
                _log = new LogClient("Camstar");
                _log.ClassName = "ExportExecuteConsoleWP";
            }
            _log.LogMessage(level, method, message);
        }

        #region ClientScript Section

        protected override string ClientControlTypeName
        {
            get { return "Camstar.WebPortal.WebPortlets.ExecutionConsole"; }
        }

        protected override IEnumerable<ScriptReference> GetScriptReferences()
        {
            yield return new ScriptReference("~/Scripts/ClientFramework/Camstar.WebPortal.WebPortlets/ExecutionConsole.js");
        }

        protected override IEnumerable<ScriptDescriptor> GetScriptDescriptors()
        {
            var list = base.GetScriptDescriptors().ToList();
            if (list.Count > 0)
            {
                var descriptor = list[0] as ScriptControlDescriptor;
                if (descriptor != null)
                {
                    descriptor.AddProperty("ProgressWindowID", ProgressWindow.ClientID);
                    descriptor.AddProperty("StartExportBtn", StartExportBtn.ClientID);
                    descriptor.AddProperty("HiddenExportImportName", _hiddenExportImportName.ClientID);
                    descriptor.AddProperty("StartExecutionConsole", _startExecutionConsole);
                    descriptor.AddProperty("MaxRefreshAttempt", CamstarPortalSection.Settings.DefaultSettings.MaxDataTransferRefreshAttempts);
                    descriptor.AddProperty("KeepSessionAlive", CamstarPortalSection.Settings.DefaultSettings.DataTransKeepSessionAlive ? 1 : 0);
                }
            }
            return list;
        }

        #endregion

        #region Controls

        protected virtual CWC.Label ProgressWindow { get { return Page.FindCamstarControl("ProgressWindow") as CWC.Label; } }
        protected virtual CWC.TextBox FileNametxt { get { return Page.FindCamstarControl("FileNametxt") as CWC.TextBox; } }
        protected virtual CWC.Button StartExportBtn { get { return Page.FindCamstarControl("StartExportBtn") as CWC.Button; } }
        protected virtual CWC.Button DownloadBtn { get { return Page.FindCamstarControl("DownloadBtn") as CWC.Button; } }
        protected virtual CWC.CheckBox IsEncryption { get { return Page.FindCamstarControl("Data_EncryptionFile") as CWC.CheckBox; } }

        #endregion

        #region Protected Functions

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            _hiddenExportImportName = new HtmlInputHidden { ID = "ExportImportName" };
            Controls.Add(_hiddenExportImportName);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (_circularRefInstances == null)
            {
                _circularRefInstances = new Dictionary<string, string>();
            }           
            if (string.IsNullOrEmpty(_circRefError))
            {
                _circRefError = FrameworkManagerUtil.GetLabelValue("CircularRefErrorLbl") ?? string.Empty;
            }

            StartExportBtn.Click += StartExportBtnClick;

            var manExp = Page.PageflowControls["ManualExportBtr"];
            FileNametxt.Hidden = true;

            if (manExp != null)
            {
                var isManualExport = true;
                if (manExp.Data == null)
                    isManualExport = true;
                else if (manExp.Data is bool)
                    isManualExport = (bool)manExp.Data;
                else
                    isManualExport = (manExp.Data.ToString().ToLower() == "true");

                if (FileNametxt != null)
                {
                    if (string.IsNullOrEmpty(FileNametxt.Data as string))
                        FileNametxt.Data = GetPredefFileName();
                }
                if (DownloadBtn != null)
                    DownloadBtn.Hidden = !isManualExport;
            }

            StartExportBtn.Enabled = true;
        }

        protected virtual void StartExportBtnClick(object sender, EventArgs e)
        {
            const string ProcedureName = "StartExportBtnClick";
            var selectedObjItems = Page.PortalContext.DataContract.GetValueByName<ObjectTypeItem[]>("DT_SelectedInstances");
            var selectedItems =
                from ob in selectedObjItems
                where ob.Instances != null && ob.Instances.Length > 0
                select ob
                into obx
                from it in obx.Instances
                select it;

            // Order selected instances
            var transfer = new DT.DataTransfer
            (
                new DT.DataTransferInfo(FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile.Name),
                new DT.DataTransferRepository(FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile),
                new Dictionary<string, string>()
            );

            var includeRefs = selectedItems.Any(x => x.IsRef);
            var orderedItems = transfer.OrderSelectedInstances(selectedItems.ToArray(), includeRefs);
            if (orderedItems != null && orderedItems.Count() > 0)
            {
                selectedItems = orderedItems.ToArray();
            }

            if (_circularRefInstances != null)
            {
                StringBuilder sb = new StringBuilder();
                if (_circularRefInstances.Count > 0)
                {
                    var grpedInstances = _circularRefInstances
                                            .GroupBy(x => x.Value)
                                            .ToDictionary(y => y.Key, y => y.Select(x => x.Key).ToArray());

                    foreach (var pair in grpedInstances)
                    {
                        var i = 0;
                        foreach (var instance in pair.Value)
                        {
                            if (i > 0)
                            {
                                sb.Append(",");
                            }
                            else
                            {
                                sb.Append(pair.Key + ":");
                            }
                            sb.Append(instance);
                            i++;
                        }
                        sb.Append(";");
                    }
                    _circRefError += sb.ToString();
                    Page.DisplayWarning(_circRefError);
                }
            }            
            
            var exportFileName = FileNametxt.Data.ToString();
            LogMessage(LogSeverity.Information, ProcedureName, $"Start export for FileName: {exportFileName} for {selectedItems.Count()} instances");
            var res = Transfer.Transfer(exportFileName, "", selectedItems.ToArray(), null, (bool)IsEncryption.Data);
            LogMessage(LogSeverity.Information, ProcedureName, $"Completed export - Status: {res.IsSuccess}");

            _hiddenExportImportName.Value = exportFileName;
            if (res.IsSuccess)
            {
                _startExecutionConsole = true;
                FileNametxt.Data = null;
                StartExportBtn.Enabled = false;
                RenderToClient = true;
            }
            else
            {
                DisplayMessage(res);
            }
        }
        #endregion

        #region Static Functions

        public static bool GetTransferStatus(AjaxTransition transition)
        {
            var transfer = Transfer;
            var exportName = transition.CommandParameters;
            var transferResponse = "";

            var resultStatus = transfer.GetTransferStatus(exportName);
            if (resultStatus.IsSuccess)
            {
                transferResponse = transfer.TransferStatusResponse;
            }
            else
            {
                transferResponse = resultStatus.ExceptionData.Description;
            }

            transition.Response = new ResponseSection[1];
            transition.Response[0] = new ResponseSection(ResponseType.Command, string.Empty, new CommandData(resultStatus.IsSuccess, transferResponse));

            return true;
        }

        public static bool DownloadExport(AjaxTransition transition)
        {
            var transfer = Transfer;
            var resultStatus = transfer.GetExportFile(transition.CommandParameters);
            if (resultStatus.IsSuccess)
            {
                transition.Response = new ResponseSection[1];
                transition.Response[0] = new ResponseSection(ResponseType.Command, string.Empty,
                    new CommandData(true, transfer.TransferStatusResponse));
            }

            return resultStatus.IsSuccess;
        }

        #endregion

        #region Public Functions

        #endregion

        #region Private Functions

        protected virtual string GetPredefFileName() // pass to ExportImportName
        {
            var dt = DateTime.Now;
            return "Export-" + dt.ToString("yyyy-MM-dd-hh-mm-ss");
        }

        protected virtual void ControlVisibility(string[] controlIds, bool isVisible)
        {
            foreach (var cs in controlIds)
            {
                var c = Page.FindCamstarControl(cs) as Control;
                if (c != null)
                    c.Visible = isVisible;
            }
        }

        #endregion

        #region Constants

        #endregion

        #region Private Member Variables

        private bool _startExecutionConsole;
        private HtmlInputHidden _hiddenExportImportName;
        protected static PortalContextBase _portalContext
        {
            get
            {
                PortalContextBase cont = null;
                if (HttpContext.Current != null && HttpContext.Current.Request != null)
                {
                    var callStackId = HttpContext.Current.Request.QueryString[Constants.QueryStringConstants.CallStackKey];
                    if (callStackId != null)
                        cont = new CallStack(callStackId).Context;
                }
                return cont;
            }
        }
        protected static DataTransfer Transfer
        {
            get { return (DataTransfer)_portalContext.LocalSession["Transfer"]; }
        }

        private Dictionary<string, string> _circularRefInstances;
        private string _circRefError;
        #endregion

    }
}
