/* Copyright 2022 Siemens */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Web;
using System.Web.UI;

using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.PortalFramework;
using SEMI.AppCode;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class scsDBLotStartSimple : MatrixWebPart
    {
        protected CWC.RevisionedObject _rdoProduct { get { return Page.FindCamstarControl("scsDBLotStartSimple_Product") as CWC.RevisionedObject; } }
        protected CWC.DateChooser _dateExpectedStartdate { get { return Page.FindCamstarControl("scsDBLotStartSimple_ExpectedStartDate") as CWC.DateChooser; } }
        protected CWC.NamedSubentity _ndsWorkflowStep { get { return Page.FindCamstarControl("scsDBLotStartSimple_WorkflowStep") as CWC.NamedSubentity; } }
        protected CWC.NamedObject _ndoStartReason { get { return Page.FindCamstarControl("scsDBLotStartSimple_StartReason") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoFactory { get { return Page.FindCamstarControl("scsDBLotStartSimple_Factory") as CWC.NamedObject; } }
        protected virtual CWC.CheckBox GenerateNameChild { get { return Page.FindCamstarControl("Details_AutoNumberChild") as CWC.CheckBox; } }
        protected virtual CWC.CheckBox GenerateIndividualContainerName { get { return Page.FindCamstarControl("Details_AutoNumberIndividualContainer") as CWC.CheckBox; } }
        protected virtual CWC.NamedObject ParentNumberingRule { get { return Page.FindCamstarControl("scsDBLotStartSimple_scsAutoNumberRule") as CWC.NamedObject; } }
        protected virtual CWC.NamedObject ChildNumberingRule { get { return Page.FindCamstarControl("scsDBLotStartSimple_scsChildAutoNumberRule") as CWC.NamedObject; } }
        protected virtual CWC.Button AddButton { get { return Page.FindCamstarControl("ChildDetails_Add") as CWC.Button; } }
        protected virtual CWC.TextBox ParentContainerName { get { return Page.FindCamstarControl("scsDBLotStartSimple_ContainerName") as CWC.TextBox; } }
        protected virtual CWC.NamedObject ChildLevel { get { return Page.FindCamstarControl("ChildDetails_Level") as CWC.NamedObject; } }
        protected virtual CWC.NamedObject Carrier { get { return Page.FindCamstarControl("scsDBLotStartSimple_Carrier") as CWC.NamedObject; } }
        protected virtual CWC.TextBox ChildCount { get { return Page.FindCamstarControl("ChildDetails_Count") as CWC.TextBox; } }
        protected virtual JQDataGrid ChildContainer { get { return Page.FindCamstarControl("scsChildDetailsGrid") as JQDataGrid; } }
        protected virtual CWC.NamedObject ParentLevel { get { return Page.FindCamstarControl("scsDBLotStartSimple_Level") as CWC.NamedObject; } }
        protected virtual JQDataGrid MfgOrderGrid { get { return Page.FindCamstarControl("scsWorkOrdDispatch_WorkOrder") as JQDataGrid; } }
        protected virtual CWC.CheckBox SkipParentCreation { get { return Page.FindCamstarControl("scsDBLotStartSimple_scsIsIndividualContainer") as CWC.CheckBox; } }
        protected virtual CWC.TitleControl ChildContainerInfoTitle { get { return Page.FindCamstarControl("ChildLot_ContainerInfo") as CWC.TitleControl; } }
		protected virtual CWC.TextBox _txtComputerName { get { return Page.FindCamstarControl("scsDBLotStartSimple_ComputerName") as CWC.TextBox; } }

        private const string _SequenceNumPlaceholder = "seq_num";

        //---------------------------------------------------
        //
        //---------------------------------------------------
        protected virtual string NoParentNumberingRuleDefinedMessage
        {
            get
            {
                var labelCache = FrameworkManagerUtil.GetLabelCache(HttpContext.Current.Session);
                return labelCache.GetLabelByName("scsWOD_NoNumberingRuleParent").Value;
            }
        }

        protected virtual string NoChildNumberingRuleDefinedMessage
        {
            get
            {
                var labelCache = FrameworkManagerUtil.GetLabelCache(HttpContext.Current.Session);
                return labelCache.GetLabelByName("scsWOD_NoNumberingRuleChild").Value;
            }
        }

        protected virtual string NoContainerLevelMessage
        {
            get
            {
                var labelCache = FrameworkManagerUtil.GetLabelCache(HttpContext.Current.Session);
                return labelCache.GetLabelByName("scsWOD_NoLevel").Value;
            }
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        protected virtual string NoChildInGridMessage
        {
            get
            {
                var labelCache = FrameworkManagerUtil.GetLabelCache(HttpContext.Current.Session);
                return labelCache.GetLabelByName("scsDBLotStartSimple_NoChildInGrid").Value;
            }
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        protected virtual string NoContainerInGridMessage
        {
            get
            {
                var labelCache = FrameworkManagerUtil.GetLabelCache(HttpContext.Current.Session);
                return labelCache.GetLabelByName("DBStart_EmptyContainerList").Value;
            }
        }

        //---------------------------------------------------
        // Label Text: Child Count
        //---------------------------------------------------
        protected virtual string ChildCountLbl
        {
            get
            {
                var labelCache = FrameworkManagerUtil.GetLabelCache(HttpContext.Current.Session);
                return labelCache.GetLabelByName("Container_ChildCount").Value;
            }
        }

        //---------------------------------------------------
        // Label Text: Child Containers
        //---------------------------------------------------
        protected virtual string ChildContainersGridLbl
        {
            get
            {
                var labelCache = FrameworkManagerUtil.GetLabelCache(HttpContext.Current.Session);
                return labelCache.GetLabelByName("Container_ChildContainers").Value;
            }
        }

        //---------------------------------------------------
        // Label Text: Container Count
        //---------------------------------------------------
        protected virtual string ContainerCountLbl
        {
            get
            {
                var labelCache = FrameworkManagerUtil.GetLabelCache(HttpContext.Current.Session);
                return labelCache.GetLabelByName("DBStartContainerCountLbl").Value;
            }
        }

        //---------------------------------------------------
        // Label Text: Containers
        //---------------------------------------------------
        protected virtual string ContainersGridLbl
        {
            get
            {
                var labelCache = FrameworkManagerUtil.GetLabelCache(HttpContext.Current.Session);
                return labelCache.GetLabelByName("DBStartContainersGridLbl").Value;
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            ScriptManager.RegisterStartupScript(this, this.GetType(), "scsDBLotStartSimple_CustomDisplay", "if(document.getElementById('ctl00_WebPartManager_ChildLotWP_scsDBLotStartSimple_scsIsIndividualContainer_ctl00')){if(document.getElementById('ctl00_WebPartManager_ChildLotWP_scsDBLotStartSimple_scsIsIndividualContainer_ctl00').checked){document.getElementsByClassName('child-level')[0].style.display = 'none';document.getElementsByClassName('empty-cell')[0].style.display = 'inline-block';}else{document.getElementsByClassName('child-level')[0].style.display = 'inline-block';document.getElementsByClassName('empty-cell')[0].style.display = 'none';}}", true);
        }

        protected override void OnPreLoad(object sender, EventArgs e)
        {
            base.OnPreLoad(sender, e);
            if (SkipParentCreation.IsChecked)
            {
                ChildCount.LabelText = ContainerCountLbl;
                ChildLevel.Visible = false;
                ChildContainer.LabelText = ContainersGridLbl;
                ChildContainerInfoTitle.Visible = false;
                Carrier.Enabled = false;
                GenerateNameChild.Visible = false;
                GenerateIndividualContainerName.Visible = true;
            }
            else
            {
                ChildCount.LabelText = ChildCountLbl;
                ChildLevel.Visible = true;
                ChildContainer.LabelText = ChildContainersGridLbl;
                ChildContainerInfoTitle.Visible = true;
                Carrier.Enabled = true;
                GenerateNameChild.Visible = true;
                GenerateIndividualContainerName.Visible = false;
            }
            ChildContainer.ApplyFieldPersonalization();
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);
				_txtComputerName.TextControl.Text = SEMI.AppCode.UIUtility.GetComputerName(this);
                _rdoProduct.DataChanged += new EventHandler(ProductField_DataChanged);
                Page.OnClearValues += Page_OnClearValues;

                if (Page.EventArgument == "OnRowSelected:WebPart_WorkOrderWP~scsWorkOrdDispatch_WorkOrder")
                    WorkOrderGrid_RowClick();

                //ParentNumberingRule.DataChanged += ParentNumberingRule_DataChanged;
                //GenerateNameChild.CheckControl.CheckedChanged += GenerateNameChildCheckControl_CheckedChanged;

                if (SkipParentCreation.IsChecked)
                {
                    GenerateNameChild.IsChecked = GenerateIndividualContainerName.IsChecked;
                }

                if (GenerateNameChild.IsChecked)
                {
                    (ChildContainer.GridContext as BoundContext).Fields["ContainerName"].Editable = false;
                }
                else
                {
                    (ChildContainer.GridContext as BoundContext).Fields["ContainerName"].Editable = true;
                }
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }
        private void Page_OnClearValues(object sender, FormsFramework.ServiceDataEventArgs e)
        {
            assignFactory();
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        protected virtual void ClearChildContainersGrid(string warningMessage)
        {
            if (warningMessage != null)
                Page.DisplayWarning(warningMessage);

            ChildContainer.Data = null;
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        protected void ProductField_DataChanged(object sender, EventArgs e)
        {
            if (_ndsWorkflowStep.Data != null && _ndoStartReason.Data != null)
                _dateExpectedStartdate.Data = System.DateTime.Today.ToString("M/d/yyyy");
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as Camstar.WebPortal.Personalization.CustomAction;
            if (action != null)
            {
                switch (action.Parameters)
                {
                    case "Reset":
                        {
                            SkipParentCreation.IsChecked = false;
                            ChildCount.LabelText = ChildCountLbl;
                            ChildLevel.Visible = true;
                            ChildContainer.LabelText = ChildContainersGridLbl;
                            ChildContainerInfoTitle.Visible = true;
                            Carrier.Enabled = true;
                            GenerateNameChild.Visible = true;
                            GenerateIndividualContainerName.Visible = false;
                            ChildContainer.ApplyFieldPersonalization();
                            Page.ShopfloorReset(sender, e);
                            assignFactory();
                            break;
                        }
                }
            }
        } // WebPartCustomAction(object sender, CustomActionEventArgs e)

        //---------------------------------------------------
        //
        //---------------------------------------------------
        public void WorkOrderGrid_RowClick()
        {
            string sRowID = MfgOrderGrid.SelectedRowID;
            if (sRowID != null)
            {
                RecordSet oData = MfgOrderGrid.Data as RecordSet;
                string sQty = MfgOrderGrid.GridContext.GetCell(sRowID, "Qty").ToString();
                string sInProcessQty = MfgOrderGrid.GridContext.GetCell(sRowID, "InProcessQty").ToString();
                if (sQty != null && sInProcessQty != null)
                {
                    try { ChildCount.Data = (int.Parse(sQty) - int.Parse(sInProcessQty)).ToString(); }
                    catch { }
                }
            }
            //else
            //{
            //    Page.ClearValues();
            //    assignFactory();
            //}
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        protected void assignFactory()
        {
            if (Page.SessionDataContract.GetValueByName(Camstar.WebPortal.Constants.DataMemberConstants.Factory) != null)
                _ndoFactory.Data = Page.SessionDataContract.GetValueByName(Camstar.WebPortal.Constants.DataMemberConstants.Factory).ToString();
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        public virtual void AddChildContainers()
        {
            Page.StatusBar.ClearMessage();

            if (ParentLevel.Data == null)
            {
                Page.DisplayWarning(NoContainerLevelMessage);
                return;
            }
            if (ParentContainerName.Data == null && ParentNumberingRule.Data == null)
            {
                Page.DisplayWarning(NoParentNumberingRuleDefinedMessage);
                return;
            }
            if (GenerateNameChild.IsChecked && ChildNumberingRule.Data == null)
            {
                Page.DisplayWarning(NoChildNumberingRuleDefinedMessage);
                return;
            }

            //Child Details
            CWC.TextBox _Qty = Page.FindCamstarControl("ChildDetails_Qty") as CWC.TextBox;
            CWC.NamedObject _UOM = Page.FindCamstarControl("ChildDetails_UOM") as CWC.NamedObject;
            CWC.TextBox _Qty2 = Page.FindCamstarControl("ChildDetails_Qty2") as CWC.TextBox;
            CWC.NamedObject _UOM2 = Page.FindCamstarControl("ChildDetails_UOM2") as CWC.NamedObject;
            CWC.TextBox _Count = Page.FindCamstarControl("ChildDetails_Count") as CWC.TextBox;

            try
            {
                // get the existing Containers
                //List<StartDetails> newChildContainers = GetCurrentChildContainers().ToList();
                //int startPosition = newChildContainers.Count>0?newChildContainers.Count:1;
                List<StartDetails> newChildContainers = new List<StartDetails>();
                int startPosition = 1;
                int endPosition = startPosition + int.Parse(_Count.Data.ToString());
                int totalCountLength = endPosition.ToString().Length;
                string paddingNo = "D" + totalCountLength.ToString();

                string sParentNumRuleFormat = "";
                string sChildNumRuleFormat = "";

                if (ParentNumberingRule.Data != null)
                    sParentNumRuleFormat = GetNumRuleFormat(ParentNumberingRule.Data.ToString());

                if (ChildNumberingRule.Data != null)
                    sChildNumRuleFormat = GetNumRuleFormat(ChildNumberingRule.Data.ToString());

                StartDetails c;//New Child Container                 
                for (int i = startPosition; i < endPosition; i++)
                {
                    string sContainerName = "";

                    if (GenerateNameChild.IsChecked)
                        sContainerName = sChildNumRuleFormat;
                    else
                    {
                        if (ParentContainerName.Data != null)
                            sContainerName = ParentContainerName.Data.ToString() + " - " + i.ToString(paddingNo);
                        else
                            sContainerName = sParentNumRuleFormat + " - " + i.ToString(paddingNo);
                    }

                    // initialize the new record
                    c = new StartDetails()
                    {
                        ContainerName = sContainerName
                    };

                    if (ChildLevel.Data != null)
                        c.Level = new NamedObjectRef(ChildLevel.Data.ToString());
                    else
                    {
                        if (ParentLevel.Data != null)
                            c.Level = new NamedObjectRef(ParentLevel.Data.ToString());
                    }

                    if (_Qty.Data != null)
                        c.Qty = (double)_Qty.Data;

                    if (_UOM.Data != null)
                        c.UOM = new NamedObjectRef(_UOM.Data.ToString());

                    if (_Qty2.Data != null)
                        c.Qty2 = (double)_Qty2.Data;

                    if (_UOM2.Data != null)
                        c.UOM2 = new NamedObjectRef(_UOM2.Data.ToString());

                    //add new child containers
                    newChildContainers.Add(c);
                }

                //Update Child Containers
                ChildContainer.Data = newChildContainers.ToArray();
                //NOTE: above call does not seem to populate whatever datasource is needed for the data to be persisted, submitted.  This should be reviewed
                ChildContainer.OriginalData = newChildContainers.ToArray();
            }
            catch (Exception ex)
            {
                DisplayMessage(new ResultStatus(ex.Message, false));
            }

        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        protected virtual string GetNumRuleFormat(string numberingRule)
        {
            string prefix = string.Empty;
            string suffix = string.Empty;

            if (numberingRule != null)
            {
                var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
                if (session != null)
                {
                    var service = new NumberingRuleMaintService(session.CurrentUserProfile);
                    var serviceData = new NumberingRuleMaint
                    {
                        ObjectToChange = new NamedObjectRef(numberingRule, "NumberingRule")
                    };

                    var request = new NumberingRuleMaint_Request
                    {
                        Info = new NumberingRuleMaint_Info
                        {
                            ObjectToChange = new Info(true),
                            ObjectChanges = new NumberingRuleChanges_Info
                            {
                                Prefix = new Info(true),
                                Suffix = new Info(true)
                            }
                        }
                    };

                    var result = new NumberingRuleMaint_Result();
                    ResultStatus status = service.Load(serviceData, request, out result);
                    if (status != null && status.IsSuccess)
                    {
                        prefix = result.Value.ObjectChanges.Prefix != null ? Convert.ToString(result.Value.ObjectChanges.Prefix.Value) : string.Empty;
                        suffix = result.Value.ObjectChanges.Suffix != null ? Convert.ToString(result.Value.ObjectChanges.Suffix.Value) : string.Empty;

                        if (!string.IsNullOrEmpty(prefix) && prefix.Substring(0, 1) == "\"" && prefix.Substring(prefix.Length - 1, 1) == "\"")
                            prefix = prefix.Substring(1, prefix.Length - 2);

                        if (!string.IsNullOrEmpty(suffix) && suffix.Substring(0, 1) == "\"" && suffix.Substring(suffix.Length - 1, 1) == "\"")
                            suffix = suffix.Substring(1, suffix.Length - 2);
                    }

                }
            }
            else
                return string.Empty;

            return prefix + _SequenceNumPlaceholder + suffix;
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        protected virtual StartDetails[] GetCurrentChildContainers()
        {
            if (ChildContainer.Data != null)
            {
                StartDetails[] details = (ChildContainer.Data as Array).Cast<StartDetails>().ToArray();
                return details;
            }
            else
            {
                StartDetails[] emptyDetails = new StartDetails[0];
                return emptyDetails;
            }
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        public override void GetInputData(Service serviceData)
        {
            try
            {
                base.GetInputData(serviceData);

                CWC.TextBox _ChildQty = Page.FindCamstarControl("ChildDetails_Qty") as CWC.TextBox;
                CWC.NamedObject _ChildUOM = Page.FindCamstarControl("ChildDetails_UOM") as CWC.NamedObject;
                CWC.TextBox _ChildQty2 = Page.FindCamstarControl("ChildDetails_Qty2") as CWC.TextBox;
                CWC.NamedObject _ChildUOM2 = Page.FindCamstarControl("ChildDetails_UOM2") as CWC.NamedObject;
                CWC.TextBox _ChildCount = Page.FindCamstarControl("ChildDetails_Count") as CWC.TextBox;

                (serviceData as OM.scsDBLotStartSimple).scsDefaultChildLevel = ChildLevel.Data as NamedObjectRef;
                (serviceData as OM.scsDBLotStartSimple).Carrier = Carrier.Data as NamedObjectRef;
                (serviceData as OM.scsDBLotStartSimple).Factory = _ndoFactory.Data as NamedObjectRef;

                if (_ChildCount.Data != null)
                    (serviceData as OM.scsDBLotStartSimple).scsChildCount = int.Parse(_ChildCount.Data.ToString());

                if (_ChildQty.Data != null)
                    (serviceData as OM.scsDBLotStartSimple).scsDefaultChildQty = (double)_ChildQty.Data;

                if (_ChildUOM.Data != null)
                    (serviceData as OM.scsDBLotStartSimple).scsDefaultChildUOM = new NamedObjectRef(_ChildUOM.Data.ToString());

                if (_ChildQty2.Data != null)
                    (serviceData as OM.scsDBLotStartSimple).scsDefaultChildQty2 = (double)_ChildQty2.Data;

                if (_ChildUOM2.Data != null)
                    (serviceData as OM.scsDBLotStartSimple).scsDefaultChildUOM2 = new NamedObjectRef(_ChildUOM2.Data.ToString());

                if (ParentContainerName.Data == null)
                    (serviceData as OM.scsDBLotStartSimple).scsAutoNumber = true;

                (serviceData as OM.scsDBLotStartSimple).scsChildLots = null;

                if (ChildContainer != null && ChildContainer.TotalRowCount > 0)
                {
                    StartDetails[] childData = ChildContainer.Data as StartDetails[];

                    string[] childNames = ((IEnumerable)childData).Cast<StartDetails>()
                                .Select(x => x.ContainerName.ToString())
                                .ToArray();

                    string sNames = String.Join("|", childNames);
                    (serviceData as OM.scsDBLotStartSimple).ContainerNamesStr = String.Join("|", sNames);
                    (serviceData as OM.scsDBLotStartSimple).scsChildCount = ChildContainer.TotalRowCount;
                }

                if (SkipParentCreation.IsChecked)
                {
                    (serviceData as OM.scsDBLotStartSimple).ContainerName = null;
                }
            }
            catch (Exception ex)
            { DisplayMessage(new ResultStatus(ex.Message, false)); }
        }


        //---------------------------------------------------
        //
        //---------------------------------------------------
        public override bool PreExecute(Info serviceInfo, Service serviceData)
        {
            bool result = base.PreExecute(serviceInfo, serviceData);

            // Make sure that Numbering Rules are defined if they are being used
            if (ChildContainer != null && ChildContainer.TotalRowCount <= 0)
            {
                result = false;
                if (!SkipParentCreation.IsChecked)
                {
                    Page.DisplayWarning(NoChildInGridMessage);
                }
                else
                {
                    Page.DisplayWarning(NoContainerInGridMessage);
                }
            }

            // request for the MfgOrder SelectionValues to get the grid to refresh
            (serviceInfo as scsDBLotStartSimple_Info).MfgOrder = new Info(false, true);

            return result;
        }


        //---------------------------------------------------
        //
        //---------------------------------------------------
        public override void PostExecute(ResultStatus status, Service serviceData)
        {
            base.PostExecute(status, serviceData);
            if (status != null && status.IsSuccess)
            {
                //for 'lot bulk start' no using MfgOrderGrid
                if (MfgOrderGrid != null)
                    MfgOrderGrid.SelectedRowID = null;

                ChildCount.LabelText = ChildCountLbl;
                ChildLevel.Visible = true;
                ChildContainer.LabelText = ChildContainersGridLbl;
                ChildContainerInfoTitle.Visible = true;
                Carrier.Enabled = true;
                GenerateNameChild.Visible = true;
                GenerateIndividualContainerName.Visible = false;
            }
        }

    }

}
