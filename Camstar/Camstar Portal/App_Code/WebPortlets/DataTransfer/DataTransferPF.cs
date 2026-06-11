// Copyright Siemens 2023  
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.PortalFramework;
using OM = Camstar.WCF.ObjectStack;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.Constants;


namespace Camstar.WebPortal.WebPortlets.DataTransfer
{
    public class DataTransferPF : WebPartPageBase
    {

        #region Controls

        protected virtual JQDataGrid ObjectTypeGrid { get { return FindCamstarControl("ObjectTypeGrid") as JQDataGrid; } }
        protected virtual WebPartBase InfoPanelWP { get { return FindIForm("DT_InfoPanel_WP") as WebPartBase; } }

        #endregion

        #region Protected Functions          
        protected override void CreatePageflowNavigationButtons()
        {
            base.CreatePageflowNavigationButtons();
            NavigationButtons.DeleteButton.Visible = false;
            NavigationButtons.SaveButton.Visible = false;
            NavigationButtons.NextButton.Attributes.Add("position", "rightmost");
            NavigationButtons.BackButton.Attributes.Add("position", "rightmost");
            NavigationButtons.CancelButton.Attributes.Add("position", "rightmost");
            NavigationButtons.CancelButton.Visible = true;
            NavigationButtons.SubmitButton.Visible = false;

            if (CurrentPageFlow != null)
            {
                if (CurrentPageFlow.ActiveNodeKey.Key == _TransferTypeVP)
                {                    
                    NavigationButtons.NextButton.LabelText = _labelsForTransfer["StartButton"] ?? string.Empty;
                    NavigationButtons.NextButton.Click += (sender, e) =>
                    {
                        PortalContext.DataContract.SetValueByName("DT_SummaryData", null);
                        PortalContext.DataContract.SetValueByName("DT_SelectedInstances", null);
                        if (PortalContext is MaintenanceBehaviorContext)
                            (PortalContext as MaintenanceBehaviorContext).CDOID = null;
                    };
                    NavigationButtons.CancelButton.Visible = false;
                }

                else if (CurrentPageFlow.ActiveNodeKey.Key == _ImportExecutionVP || CurrentPageFlow.ActiveNodeKey.Key== _ExportExecutionVP)
                {
                    NavigationButtons.BackButton.Visible = true;
                    NavigationButtons.CancelButton.Visible = false;
                }

                if (CurrentPageFlow.OnPersistState == null)
                {
                    CurrentPageFlow.OnPersistState += (sender, args) => { args.CancelDefault = true; };
                }
            }

            if (Transfer == null)
            {
                var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);                
                Transfer = new DataTransfer(new DataTransferInfo(session.CurrentUserProfile.Name), new DataTransferRepository(session.CurrentUserProfile), _labelsForTransfer);
            }
        }

        protected override void OnInit(EventArgs e)
        {
            if (!CurrentCallStack.IsInitialized && CurrentPageFlow != null && CurrentPageFlow.ActiveNodeKey.Key != _TransferTypeVP)
            {
                // Keep current call stack for non-first page
                CurrentCallStack.IsInitialized = true;
            }
            base.OnInit(e);
            if (_labelsForTransfer == null)
                _labelsForTransfer = new Dictionary<string,string>();
            string[] labels = { "Lbl_DT_Status", "Lbl_DT_Employee", "Lbl_DT_StartTimestamp", "Lbl_DT_ExecutionTime", "Lbl_DT_Complete", "Lbl_DT_Error", "Lbl_DT_Transferring", "Lbl_DT_Source", "Lbl_DT_MinsSecs", "Lbl_DT_ObjectTypes", "Lbl_DT_Instances", "Lbl_DT_FileName", "Lbl_DT_NotStarted", "StartButton" };            
            _labelsForTransfer = GetDataTranfserLabels(labels);
           
        }

        /// <summary>
        /// Verify that the user has put some items in the grid on the selection page,
        /// otherwise there will be nothing to export
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void NextButton_Click(object sender, NavigationEventArgs e)
        {
            if (CurrentPageFlow.ActiveNodeKey.Key == _TransferTypeVP)
            {

                var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
                var transferType = TransferType.NotSet;
                if ((bool)((RadioButton)FindCamstarControl("ManualImportBtr")).Data) transferType = TransferType.Import;
                if ((bool)((RadioButton)FindCamstarControl("ManualExportBtr")).Data) transferType = TransferType.Export;
                if ((bool)((RadioButton)FindCamstarControl("AutoBtr")).Data) transferType = TransferType.EndtoEnd;
                Transfer.SetTransferType(transferType);
            }
            if ((CurrentPageFlow.ActiveNodeKey.Key == _SelectionVP || CurrentPageFlow.ActiveNodeKey.Key == _ReferencesVP) && ObjectTypeGrid != null)
            {
                var data = ObjectTypeGrid.Data as ObjectTypeItem[];

                if (ObjectTypeItem.GetInstancesCount(data, false) == 0)
                {
                    var statusItem = new RequiredFieldStatusItem(ObjectTypeGrid.Caption, ObjectTypeGrid.PageName)
                    {
                        ID = ObjectTypeGrid.ID,
                        ClientID = ObjectTypeGrid.ClientID,
                        IsGridType = true
                    };
                    DisplayMessage(new ValidationStatus { statusItem });
                    return;
                }
            }
            base.NextButton_Click(sender, e);
        }

        protected virtual ResponseData targetGrid_RowSelected(object sender, JQGridEventArgs args)
        {
            var grid = FindCamstarControl("TargetImpactGrid") as JQDataGrid;
            if (grid != null)
                grid.Data = GetImpacts(args.Context.SelectedRowID);
            return null;
        }


        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            var finishButton = FindCamstarControl("FinishBtn") as Button;
            if (finishButton != null)
            {
                finishButton.Attributes.Add("position", "rightmost");
            }
            if (CurrentPageFlow.ActiveNodeKey.Key == _TransferTypeVP)
            {
                ((RadioButton)FindCamstarControl("ManualImportBtr")).RadioControl.CheckedChanged += ClearPageFlowValues;
                ((RadioButton)FindCamstarControl("ManualExportBtr")).RadioControl.CheckedChanged += ClearPageFlowValues;
            }
            // load stub data to Targets grid
            if (CurrentPageFlow.ActiveNodeKey.Key == "DT_TargetImpact_VP")
            {
                JQDataGrid targetGrid = PageflowControls.FindControl("TargetGrid") as JQDataGrid;
                if (targetGrid != null && targetGrid.Data != null)
                    {
                        var grid = FindCamstarControl("AvailableTargetGrid") as JQDataGrid;
                        if (grid != null)
                        {
                            grid.Data = (targetGrid.Data as TargetSystem[]).Where(t => t.IsSelected).ToArray();
                            grid.RowSelected += targetGrid_RowSelected;
                        }
                    }
                }
            else if (CurrentPageFlow.ActiveNodeKey.Key == _ImportExecutionVP || CurrentPageFlow.ActiveNodeKey.Key == _ExportExecutionVP)
            {
                SaveSummary();
            }

            if (CurrentPageFlow.ActiveNodeKey.Key == _SelectionVP)
            {
                var instanceName = FindCamstarControl("ObjectList") as DropDownList;
                if (instanceName != null)
                {
                    instanceName.Focus();
                }
            }

            var script = string.Format("ExportImportCheck();");
            ScriptManager.RegisterStartupScript(Page.Form, GetType(), "TypeSelector", script, true);
        }

        protected virtual void ClearPageFlowValues(object sender, EventArgs e)
        {
            PortalContext.DataContract.SetValueByName("DT_SelectedInstances", null);
            PageflowControls.Clear();
        }

        protected override void LoadPageflowData()
        {
            base.LoadPageflowData();
            var activityName = CurrentPageFlow.ActiveNodeKey.Key;

            if (activityName == "DT_Target_VP")
            {
                var grid = FindCamstarControl("TargetGrid") as JQDataGrid;
                if (grid != null)
                {
                    var d = PageflowControls.GetDataByFieldExpression("TargetGridData");
                    if (d == null)
                        grid.Data = GetTargets();
                    else
                        grid.Data = d;
                }
            }
            if (activityName == _TransferTypeVP)
            {
               WorkflowProgressBar.ResetHistory();
               if (PageflowControls.Count > 0)
                   PageflowControls.Clear();
            }
        }

        protected override void SavePageflowData()
        {
            base.SavePageflowData();
            var grid = FindCamstarControl("TargetGrid") as JQDataGrid;
            if (grid != null)
            {
                PageflowControls.Add("TargetGridData", grid.Data);
            }

            var selectAll = FindCamstarControl("SelectAll_Btr") as RadioButton;
            if (selectAll != null && (bool)selectAll.Data == true)
            {
                // Load all instances
                if (PortalContext != null && PortalContext.LocalSession["ObjectList"] != null)
                {
                    var allInstances = new List<SelectedInstanceItem>();
                    var exportObject = PortalContext.LocalSession["ObjectList"] as Dictionary<string, CDOData>;
                    Array.ForEach(exportObject.Take(100).ToArray(), eo =>
                    {
                        var inst = GetInstances(eo.Value.CDODefID);
                        if (inst != null)
                            allInstances.AddRange(inst);
                    });

                    PortalContext.DataContract.SetValueByName("DT_SelectedInstances", allInstances.ToArray());
                }
            }
            else if (CurrentPageFlow.ActiveNodeKey.Key == _ImportExecutionVP || CurrentPageFlow.ActiveNodeKey.Key == _ExportExecutionVP)
            {
                SaveSummary();
            }
        }

       protected virtual void SaveSummary()
        {
            var dataTransferType = InfoPanelWP.Page.FindCamstarControl("DT_TypeTxt") as CWC.TextBox;
            var sourceSystem = InfoPanelWP.Page.FindCamstarControl("SourceSystemTxt") as CWC.TextBox;
            var targetSystem = InfoPanelWP.Page.FindCamstarControl("TargetSystemTxt") as CWC.TextBox;
            var transfer = Transfer;

            var summary = new List<SummaryItem>
                {
                    new SummaryItem("Status", dataTransferType.LabelControl.Text, dataTransferType.Data as string),
                    new SummaryItem("Status", sourceSystem.Text, sourceSystem.Data as string),
                    new SummaryItem("Status", targetSystem.Text, (targetSystem.Data as string).Replace('\n', ',').TrimEnd(','))
                };
            if (transfer != null)
            {
                summary.AddRange(transfer.Summary);
            }
            // Save summary data
            PortalContext.DataContract.SetValueByName("DT_SummaryData", summary.ToArray());
        }

        protected virtual SelectedInstanceItem[] GetInstances(string cdoDefId)
        {
            var cdoInq = new OM.CDOInquiry() { CDODefId = new OM.Enumeration<OM.MaintainableObjectEnum, string>(cdoDefId) };
            var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            if (session != null)
            {
                var srv = new Camstar.WCF.Services.CDOInquiryService(session.CurrentUserProfile);
                var req = new Camstar.WCF.Services.CDOInquiry_Request() { Info = new OM.CDOInquiry_Info() { CDOInstances = new OM.Info(false, true) } };
                var res = new Camstar.WCF.Services.CDOInquiry_Result();
                var stat = srv.GetEnvironment(cdoInq, req, out res);
                if (stat.IsSuccess && res.Environment != null && res.Environment.CDOInstances != null && res.Environment.CDOInstances.SelectionValues != null && res.Environment.CDOInstances.SelectionValues.Rows != null)
                {
                    var hdr = res.Environment.CDOInstances.SelectionValues.Headers.First(h => h.Name == "InstanceId");
                    var id_index = Array.IndexOf(res.Environment.CDOInstances.SelectionValues.Headers, hdr);
                    var inst =
                        from r in res.Environment.CDOInstances.SelectionValues.Rows
                        select new SelectedInstanceItem() { CDOTypeID = cdoDefId, InstanceID = r.Values[id_index] };

                    return inst.ToArray();
                }
            }
            return null;
        }

        protected virtual TargetSystem[] GetTargets()
        {
            return new TargetSystem[]
            {
                new TargetSystem { SystemName="Sys1", Location="Minsk",     MDBVersion="1.0", ServerType="Production",  Administrator="CamstarAdmin", CamstarVersion="4.5.1", ChannelAdapter="4.5"},
                new TargetSystem { SystemName="Sys2", Location="Charlotte", MDBVersion="2.0", ServerType="Development", Administrator="CamstarAdmin", CamstarVersion="5.2.1", ChannelAdapter="5.0"}
            };
        }

        protected virtual TargetImpactInstance[] GetImpacts(string sysname)
        {
            var inst = PortalContext.DataContract.GetValueByName<SelectedInstanceItem[]>("DT_SelectedInstances");
            if (inst != null)
            {
                return
                    (from c in inst
                     select new TargetImpactInstance { ObjectName = c.CDOTypeName, Name = c.Name, Impact = ImpactEnum.Update, OldValue = "f1", NewValue = "f2", ChangedField = "FieldOne", Description = c.Description })
                     .ToArray();
            }
            return new TargetImpactInstance[] { };
        }

        #endregion        

        #region Private Functions
        protected virtual IDictionary<string, string> GetDataTranfserLabels(string[] labelNames)
        {
            if (_labelsForTransfer == null)
                _labelsForTransfer = new Dictionary<string, string>();
            var labelCache = FrameworkManagerUtil.GetLabelCache(HttpContext.Current.Session);

            foreach (string labelName in labelNames)
            {
                if (!_labelsForTransfer.ContainsKey(labelName))
                {                    
                    if (labelCache != null)
                    {
                        bool useDefaultLanguage = false;
                        if (HttpContext.Current.Session[SessionConstants.UseDefaultLanguage] != null)
                            useDefaultLanguage = (bool)HttpContext.Current.Session[SessionConstants.UseDefaultLanguage];
                        _labelsForTransfer.Add(labelName, labelCache.GetLabelValueByContext(labelName, useDefaultLanguage));
                    }
                }
            }
            return _labelsForTransfer;
        }
        #endregion

        #region Constants

        private const string _TransferTypeVP = "DT_TransferType_VP";
        private const string _ImportExecutionVP = "DT_ImportExecution_VP";
        private const string _ExportExecutionVP = "DT_ExportExecution_VP";
        private const string _SelectionVP = "DT_Selection_VP";
        private const string _ReferencesVP = "DT_References_VP";

        #endregion

        #region Protected Member Variables
        protected virtual DataTransfer Transfer
        {
            get { return (DataTransfer)PortalContext.LocalSession["Transfer"]; }
            set { PortalContext.LocalSession["Transfer"] = value; }
        }        
        #endregion        

        #region Private  Members
        protected virtual IDictionary<string, string> _labelsForTransfer
        {
            get
            {
                if (Session["_labelsForTransfer"] != null)
                    return (IDictionary<string,string>)Session["_labelsForTransfer"];
                else
                    return null;
            }
            set
            {
                if (Session["_labelsForTransfer"] != null)
                    Session["_labelsForTransfer"] = value;
                else
                    Session.Add("_labelsForTransfer", value);
            }
        }
        #endregion
    }

    public class TargetSystem
    {
        public virtual bool IsSelected { get; set; }
        public virtual string SystemName { get; set; }
        public virtual string Location { get; set; }
        public virtual string MDBVersion { get; set; }
        public virtual string ServerType { get; set; }
        public virtual string CamstarVersion { get; set; }
        public virtual string Administrator { get; set; }
        public virtual string ChannelAdapter { get; set; }
    }

    public class TargetImpactInstance
    {
        public virtual string ObjectName { get; set; }
        public virtual ImpactEnum Impact { get; set; }
        public virtual string Name { get; set; }
        public virtual string Description { get; set; }
        public virtual string ChangedField { get; set; }
        public virtual string OldValue { get; set; }
        public virtual string NewValue { get; set; }
    }

    public enum ImpactEnum { Update, New }

}

