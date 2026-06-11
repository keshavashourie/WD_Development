// Copyright Siemens 2023  
using Camstar.WCF.Services;
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
using System.IO;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using OM = Camstar.WCF.ObjectStack;
using System.Text;

namespace Camstar.WebPortal.WebPortlets.DataTransfer
{

    /// <summary>
    /// The Activate Change Pkg page will call the ActivateChangePkg service -> Load method to load the file created by the deploy process.
    /// It will then upload the data from the file.
    /// Once it has the data, it will call the ActivateChangePkg service again to write the information to history.
    /// Lastly, it will display the status of the activation to the user in the activation console.
    /// </summary>
    public class ActivateChangePkg : MatrixWebPart
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
                    descriptor.AddProperty("ActivatePackageBtn", ActivatePackageBtn.ClientID);
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
        protected virtual CWC.Label ErrorHandling { get { return Page.FindCamstarControl("ErrorHandling") as CWC.Label; } }
        protected virtual CWC.Label ErrorHandlingMsg { get { return Page.FindCamstarControl("ErrorHandlingMsg") as CWC.Label; } }
        protected virtual CWC.Button ActivatePackageBtn { get { return Page.FindCamstarControl("ActivatePackageBtn") as CWC.Button; } }
        protected virtual CWC.NamedObject ActivateChangePkg_ChangePackage { get { return Page.FindCamstarControl("ActivateChangePkg_ChangePackage") as CWC.NamedObject; } }
        protected virtual CWC.DropDownList OverrideMode { get { return Page.FindCamstarControl("OverrideMode") as CWC.DropDownList; } }
        protected virtual CWC.TextBox FileNameTxt { get { return Page.FindCamstarControl("FileNameTxt") as CWC.TextBox; } }
        protected virtual CWC.TextBox ModelingImportName { get { return Page.FindCamstarControl("ModelingImportName") as CWC.TextBox; } }
        protected virtual CWC.FileInput ImportFile { get { return Page.FindCamstarControl("ImportFile") as CWC.FileInput; } }
        protected virtual WebPartBase ErrorHandlingWP { get { return Page.FindIForm("ErrorHandling_WP") as WebPartBase; } }


        #endregion

        #region Protected Override Methods

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            _hiddenModelingImportFile = new HtmlInputHidden { ID = "ModelingImportFile" };
            _hiddenExportImportName = new HtmlInputHidden { ID = "ExportImportName" };
            Controls.Add(_hiddenModelingImportFile);
            Controls.Add(_hiddenExportImportName);
        }

        protected override void OnPreLoad(object sender, EventArgs e)
        {
            base.OnPreLoad(sender, e);
            if (Transfer == null)
            {
                var labelsForTranfer = new Dictionary<string, string>();
                var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
                Transfer = new DataTransfer(new DataTransferInfo(session.CurrentUserProfile.Name), new DataTransferRepository(session.CurrentUserProfile), labelsForTranfer);
                Transfer.SetTransferType(TransferType.Import);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ActivatePackageBtn.Click += ActivatePackageBtnClick;
            FileNameTxt.Hidden = true;
            ModelingImportName.Hidden = true;

            if (ActivateChangePkg_ChangePackage.Data == null)
            {
                ProgressWindow.Visible = false;
                ActivatePackageBtn.Visible = false;
                ErrorHandlingWP.Hidden = true;
            }
            else
            {
                ProgressWindow.Visible = true;
                ActivatePackageBtn.Visible = true;
                ErrorHandlingWP.Hidden = false;
            }

            if (FileNameTxt != null)
            {
                if (string.IsNullOrEmpty(FileNameTxt.Data as string))
                    FileNameTxt.Data = GetPredefFileName();
            }
            ActivatePackageBtn.Enabled = true;

            //Get the Import file name.
            if (ActivateChangePkg_ChangePackage.Data != null)
                if (_hiddenModelingImportFile.Value == "" || !_hiddenModelingImportFile.Value.Contains(ActivateChangePkg_ChangePackage.Data.ToString()))
                {
                    //call the load method to get the ModelingImportFile name.
                    var service = new ActivateChangePkgService(FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);
                    var data = new OM.ActivateChangePkg()
                    {
                        ChangePackage = (OM.NamedObjectRef)ActivateChangePkg_ChangePackage.Data,
                    };

                    var request = new ActivateChangePkg_Request()
                    {
                        Info = new OM.ActivateChangePkg_Info()
                        {
                            ModelingImportFile = new OM.Info(true)
                        }
                    };

                    var result = new ActivateChangePkg_Result();
                    OM.ResultStatus rs = service.Load(data, request, out result);
                    if (rs.IsSuccess && result.Value != null && result.Value.ModelingImportFile != null)
                    {
                        ImportFile.Data = result.Value.ModelingImportFile.ToString();
                        _hiddenModelingImportFile.Value = result.Value.ModelingImportFile.ToString();
                    }
                    else
                    {
                        DisplayMessage(rs);
                    }
                }

            if (!Page.IsPostBack)
            {
                var service = new ActivateChangePkgService(FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);
                var data = new OM.ActivateChangePkg()
                {
                    ChangePackage = (OM.NamedObjectRef)ActivateChangePkg_ChangePackage.Data
                };

                ActivateChangePkg_Request request = new ActivateChangePkg_Request
                {
                    Info = new OM.ActivateChangePkg_Info
                    {
                        PreReqsActivatedFlag = new OM.Info(true)
                    }
                };
                var res = new ActivateChangePkg_Result();

                var result = service.CheckIfPreReqPkgsActivated(data, request, out res);
                if (result.IsSuccess)
                {
                    if (res.Value != null)
                    {
                        _portalContext.DataContract.SetValueByName(_changePkg, ActivateChangePkg_ChangePackage.Data.ToString());
                        var preReqsCompleted = res.Value.PreReqsActivatedFlag.Value;
                        if (!preReqsCompleted)
                        {
                            var target = new FloatingPageCallStackMethod(_overridePage)
                            {
                                Width = 530,
                                Height = 310,
                                Context = _portalContext,
                                Title = _overrideConfirmationTitle
                            };

                            var stack = new CallStack(Page);
                            var popupScript = "parent." + target.GetPopupScript("Call=true", stack);
                            ScriptManager.RegisterStartupScript(Page.Form, GetType(), "OverridePopup", popupScript, true);
                        }
                    }
                }
            }
        }
        #endregion

        #region Public Methods

        public static bool GetTransferStatus(AjaxTransition transition)
        {
            var transfer = Transfer;
            var importName = transition.CommandParameters;
            var resultStatus = transfer.GetTransferStatus(importName);

            if (resultStatus.IsSuccess)
            {
                transition.Response = new ResponseSection[1];
                transition.Response[0] = new ResponseSection(ResponseType.Command, string.Empty, new CommandData(true, transfer.TransferStatusResponse));
                if (transfer.Status == TransferStatus.Error)
                {
                    _portalContext.DataContract.SetValueByName("DT_ErrorData", transfer.Errors);
                    _portalContext.DataContract.SetValueByName("DT_SelectedInstances", null);
                }

                if (transfer.Status == TransferStatus.Complete)
                {
                    _portalContext.DataContract.SetValueByName("DT_SelectedInstances", null);
                }
                return true;
            }
            return false;
        }
        #endregion

        #region Protected Virtual Methods

        protected virtual void ActivatePackageBtnClick(object sender, EventArgs e)
        {
            try
            {
                //need to get the selected items from the ModelingImportFile
                PreLoadImport();

                var selectedItems = Page.PortalContext.DataContract.GetValueByName<SelectedInstanceItem[]>("DT_SelectedInstances") ??
                        new SelectedInstanceItem[0];
                var importName = FileNameTxt.Data.ToString();
                var importSetName = Page.PortalContext.LocalSession["ImportSetName"] != null ?
                    Page.PortalContext.LocalSession["ImportSetName"].ToString()
                    : string.Empty;
                _hiddenExportImportName.Value = importName;

                var options = new object[] { true, GetOverrideMode(), ActivateChangePkg_ChangePackage.Data };
                var res = Transfer.Transfer(importName, importSetName, selectedItems, options);

                if (res.IsSuccess)
                {
                    _startExecutionConsole = true;
                    RenderToClient = true;
                }
                else
                {
                    DisplayMessage(res);
                }

                //call to activate change package service needs to take place after you request the import, but before you display the import results in the activation console.
                var service = new ActivateChangePkgService(FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);                
                var overrideComments = _portalContext.DataContract.GetValueByName<string>(_overrideComments) ?? string.Empty;
                var overrideExecuted = _portalContext.DataContract.GetValueByName(_overrideExecuted) ?? false;
                var data = new OM.ActivateChangePkg()
                {
                    ChangePackage = (OM.NamedObjectRef)ActivateChangePkg_ChangePackage.Data,
                    ExportImportName = FileNameTxt.Data.ToString(),
                    ImportAction = int.Parse((string)OverrideMode.Data),
                    ModelingImportFile = _hiddenModelingImportFile.Value,
                    OverrideComments = overrideComments,
                    OverrideExecuted = overrideExecuted != null ? (bool)overrideExecuted : false
                };

                var result = new ActivateChangePkg_Result();
                service.ExecuteTransaction(data);

                _startExecutionConsole = true;
                FileNameTxt.Data = null;
                ActivatePackageBtn.Enabled = false;
                RenderToClient = true;
            }
            catch (Exception ex)
            {
                DisplayMessage(new OM.ResultStatus(ex.Message, false));
            }
        }

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

        protected virtual void PreLoadImport()
        {
            if (Page.PortalContext != null)
            {
                var filePath = _hiddenModelingImportFile.Value;
                if (filePath.Length > 0)
                {
                    var importFile = new ImportFile(filePath);
                    var importData = importFile.GetImportData();
                    var selectedInstances = new List<SelectedInstanceItem>();

                    if (importData != null)
                    {
                        selectedInstances.AddRange(importData.ImportContents.Select(
                                io =>
                                new SelectedInstanceItem()
                                {
                                    CDOTypeID = io.ObjectType.ToString(),
                                    CDOTypeName = (string)io.ObjectTypeName,
                                    Name = (string)io.ObjectName,
                                    InstanceID = (string)io.ObjectInstanceId,
                                    Revision = (string)io.Revision,
                                    DisplayedName = string.IsNullOrEmpty((string)io.Revision) ? (string)io.ObjectName : $"{io.ObjectName}{CamstarPortalSection.GetRevisionDelimiter()} {io.Revision}"
                                }
                            ));

                        SelectedInstances = selectedInstances.ToArray();
                        Page.PortalContext.LocalSession["ImportSetName"] = importFile.SetName;
                    }
                }
            }
        }

        protected virtual OM.ImportActionEnum? GetOverrideMode()
        {
            if (OverrideMode.Data != null)
            {
                switch (OverrideMode.Text)
                {
                    case "Abort Process and Roll Back":
                        return OM.ImportActionEnum.Abort;
                    case "Overwrite Instance":
                        return OM.ImportActionEnum.Overwrite;
                    case "Skip Instance":
                        return OM.ImportActionEnum.Skip;
                }
            }
            return null;
        }
        #endregion

        #region Constants
        private const string _overridePage = "OverrideConfirmation_VP.aspx";
        private const string _overrideComments = "OverrideComments";
        private const string _changePkg = "ChangePkg";
        private const string _overrideExecuted = "OverrideExecuted";
        #endregion

        #region Private Member Variables

        private HtmlInputHidden _hiddenModelingImportFile;
        private HtmlInputHidden _hiddenExportImportName;
        private bool _startExecutionConsole;
        private string _overrideConfirmationTitle = FrameworkManagerUtil.GetLabelValue("OverrideConfirmationLbl") ?? string.Empty;
        #endregion

        #region Protected Member Variables
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
            set { _portalContext.LocalSession["Transfer"] = value; }
        }

        protected virtual SelectedInstanceItem[] SelectedInstances
        {
            get
            {
                return Page.PortalContext.DataContract.GetValueByName<SelectedInstanceItem[]>("DT_SelectedInstances");
            }
            set
            {
                Page.PortalContext.DataContract.SetValueByName("DT_SelectedInstances", value);
            }
        }
        #endregion

    }

}
