// Copyright Siemens 2023  
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using OM = Camstar.WCF.ObjectStack;

namespace Camstar.WebPortal.WebPortlets.DataTransfer
{

    public class ImportExecuteConsoleWP: MatrixWebPart
    {

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
                    descriptor.AddProperty("StartImportBtn", StartImportBtn.ClientID);
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
        protected virtual CWC.Button StartImportBtn { get { return Page.FindCamstarControl("StartImportBtn") as CWC.Button; } }
        protected virtual CWC.CheckBox ImportIfExistsBtn { get { return Page.FindCamstarControl("ImportIfExistsBtn") as CWC.CheckBox; } }
        protected virtual CWC.DropDownList ImportAction { get { return Page.FindCamstarControl("ImportAction") as CWC.DropDownList; } }
        protected virtual WebPartBase DataTransferModeWP { get { return Page.FindIForm("DataTransferMode_WP") as WebPartBase; } }
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
            StartImportBtn.Click += StartImportBtnClick;
            FileNametxt.Hidden = true;

            if (FileNametxt != null)
            {
                if (string.IsNullOrEmpty(FileNametxt.Data as string))
                    FileNametxt.Data = GetPredefFileName();
            }
            StartImportBtn.Enabled = true;
        }

        protected virtual void StartImportBtnClick(object sender, EventArgs e)
        {
            var importItems = Page.PortalContext.DataContract.GetValueByName<ObjectTypeItem[]>("DT_SelectedInstances");
            var importName = FileNametxt.Data.ToString();
            var importSetName = Page.PortalContext.LocalSession["ImportSetName"].ToString();
            _hiddenExportImportName.Value = importName;

            var options = new object[] {ImportIfExistsBtn.IsChecked, GetImportAction()};
            var selectedItems =
                from ob in importItems
                where ob.Instances != null && ob.Instances.Length > 0
                select ob
                    into obx
                    from it in obx.Instances
                    orderby it.Order
                    select it;

            var res = Transfer.Transfer(importName, importSetName, selectedItems.ToArray(), options);

            if (res.IsSuccess)
            {
                _startExecutionConsole = true;
                FileNametxt.Data = null;
                StartImportBtn.Enabled = false;
                RenderToClient = true;
            }
            else
            {
                DisplayMessage(res);
            }
        }

        #endregion

        #region Public Functions

        public static bool GetTransferStatus(AjaxTransition transition)
        {
            var transfer = Transfer;
            var importName = transition.CommandParameters;
            var transferResponse = "";

            var resultStatus = transfer.GetTransferStatus(importName);
            if (resultStatus.IsSuccess)
            {
                transferResponse = transfer.TransferStatusResponse;
                if (transfer.Status == TransferStatus.Error)
                {
                    _portalContext.DataContract.SetValueByName("DT_ErrorData", transfer.Errors);
                }
            }
            else
            {
                transferResponse = resultStatus.ExceptionData.Description;
            }
            
            transition.Response = new ResponseSection[1];
            transition.Response[0] = new ResponseSection(ResponseType.Command, string.Empty, new CommandData(resultStatus.IsSuccess, transferResponse));
            return true;
        }

        #endregion

        #region Protected Functions

        protected virtual string GetPredefFileName()
        {
            return "Import-" + DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss");
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

        protected virtual OM.ImportActionEnum? GetImportAction()
        {
            if (ImportAction.Data != null)
            {
                switch (ImportAction.CustomListValues[int.Parse((string)ImportAction.Data)].DisplayName)
                {
                    case "Abort Process and Roll Back":
                        return OM.ImportActionEnum.Abort;
                    case "Skip":
                        return OM.ImportActionEnum.Skip;
                    case "Overwrite":
                        return OM.ImportActionEnum.Overwrite;
                }
            }
            return null;
        }
        #endregion

        private HtmlInputHidden _hiddenExportImportName;
        private bool _startExecutionConsole;
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
    }

}
