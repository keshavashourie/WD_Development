/* Copyright 2019 Siemens */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Web;
using System.Web.UI;
using Camstar.WebPortal.Personalization;
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.Personalization;

/// <summary>
/// Summary description for SS_ss_TrackLabelGenerateMain
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_TrackLabelGenerate : MatrixWebPart
    {
        protected CWC.TextBox _txtSelectionId { get { return Page.FindCamstarControl("ss_TrackLabelGenerate_SelectionId") as CWC.TextBox; } }
        protected CWC.TextBox _txtComputerName { get { return Page.FindCamstarControl("ss_TrackLabelGenerate_ComputerName") as CWC.TextBox; } }
        protected CWC.TextBox _txtIsItemProcessing { get { return Page.FindCamstarControl("IsItemProcessing") as CWC.TextBox; } }
        protected CWC.TextBox _txtFirstLot { get { return Page.FindCamstarControl("FirstLot") as CWC.TextBox; } }

        protected CWC.NamedObject _ndoEmployee { get { return Page.FindCamstarControl("ss_TrackLabelGenerate_Employee") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoLabelPlan { get { return Page.FindCamstarControl("ss_TrackLabelGenerate_ss_LabelPlan") as CWC.NamedObject; } }

        protected CWC.CheckBox _chkQtyBased { get { return Page.FindCamstarControl("QtyBased") as CWC.CheckBox; } }
        protected CWC.CheckBox _chkItemBased { get { return Page.FindCamstarControl("ItemBased") as CWC.CheckBox; } }

        protected JQDataGrid _gridContainer { get { return Page.FindCamstarControl("ss_TrackLabelGenerate_ContainerGrid") as JQDataGrid; } }
        protected JQDataGrid _gridLabelPlanDetails { get { return Page.FindCamstarControl("ss_TrackLabelGenerate_ss_LabelPlanDetails") as JQDataGrid; } }
        
        //protected const string _kProcessTypeViewStateKey = "ss_TrackLabelGenerate_ProcessTypeSelection_ViewStateVariableKey";
        //protected const int _kContainersPerBatch = 20;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            _txtSelectionId.DataChanged += new EventHandler(_txtSelectionId_DataChanged);
            _ndoLabelPlan.DataChanged += new EventHandler(_ndoLabelPlan_DataChanged);
            _txtComputerName.TextControl.Text = SEMI.AppCode.UIUtility.GetComputerName(this);
            _gridLabelPlanDetails.GridContext.GridReloading += new JQGridEventHandler(LabelPlanDetailsGrid_RowReloading);

            var TrackLabelGenerateGrid = (Page.FindCamstarControl("ss_TrackLabelGenerate_ContainerGrid")) as JQDataGrid;

            if (IsResponsive)
            {
                if (TrackLabelGenerateGrid.Settings.Automation == null)
                    TrackLabelGenerateGrid.Settings.Automation = new GridAutomation();

                TrackLabelGenerateGrid.Settings.Automation.ShrinkColumnWidthToFit = false;
            }
        }

        //---------------------------------------------------
        // SelectionId Data Changed
        //---------------------------------------------------
        void _txtSelectionId_DataChanged(object sender, EventArgs e)
        {
            if (_txtSelectionId.Data != null)
            {
                Page.StatusBar.ClearMessage();
                string sContainer = "";

                if (_txtSelectionId.Data != null)
                    sContainer = _txtSelectionId.Data.ToString();

                if (!string.IsNullOrEmpty(sContainer))
                    FetchData(sContainer);

                _txtSelectionId.TextControl.Text = "";
                _txtSelectionId.Focus();
            }
        } // _txtSelectionId_DataChanged

        //---------------------------------------------------
        // LabelPlan Data Changed
        //---------------------------------------------------
        void _ndoLabelPlan_DataChanged(object sender, EventArgs e)
        {
            if (_ndoLabelPlan.Data != null)
                FetchLabelPlan();
        } // _ndoLabelPlan_DataChange

        //-----------------------------------------------------------------------------
        // Refresh the label plan details grid when grid's Reload button is pressed
        //-----------------------------------------------------------------------------
        protected virtual ResponseData LabelPlanDetailsGrid_RowReloading(object sender, JQGridEventArgs args)
        {
            if (_ndoLabelPlan.Data != null)
                FetchLabelPlan();
            return null;
        }

        //---------------------------------------------------
        // Set Controls Function
        //---------------------------------------------------
        private void SetControls()
        {
            if (_txtIsItemProcessing.Data != null)
            {
                if (_txtIsItemProcessing.Data.ToString().Equals("True"))
                {
                    _chkItemBased.Data = false;
                    _chkItemBased.ReadOnly = true;
                    _chkQtyBased.Data = false;
                    _chkQtyBased.ReadOnly = false;
                }
                else
                {
                    _chkQtyBased.Data = false;
                    _chkQtyBased.ReadOnly = true;
                    _chkItemBased.Data = false;
                    _chkItemBased.ReadOnly = false;
                }
            }
            else
            {
                _chkQtyBased.ReadOnly = false;
                _chkItemBased.ReadOnly = false;
            }
        } // _txtSelectionId_DataChanged

        //---------------------------------------------------
        // Fetch Data
        //---------------------------------------------------
        private void FetchData(string sContainerName = "")
        {
            //Initialize Service & Objects
            UserProfile profile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
            ss_TrackLabelGenerateService Svc = new ss_TrackLabelGenerateService(profile);
            ss_TrackLabelGenerate SvcData = new ss_TrackLabelGenerate();
            ss_TrackLabelGenerate_Info SvcInfo = new ss_TrackLabelGenerate_Info();
            ss_TrackLabelGenerate_Request ReqData = new ss_TrackLabelGenerate_Request();
            ss_TrackLabelGenerate_Result ResData = new ss_TrackLabelGenerate_Result();

            SvcData.SelectionId = sContainerName;
            SvcData.FirstLot = new ContainerRef();

            if ((_gridContainer.GridContext as BoundContext).GetTotalRows() > 0)
                SvcData.FirstLot.Name = _txtFirstLot.Data.ToString();
            else
                SvcData.FirstLot.Name = sContainerName;

            SvcInfo.IsItemProcessing = new Info();
            SvcInfo.IsItemProcessing.RequestValue = true;
            ReqData.Info = SvcInfo;

            //Execute Request 
            ResultStatus Results = Svc.VerifyLot(SvcData, ReqData, out ResData);

            //Result
            if (Results.IsSuccess)
            {
                string sServiceType = "ss_TrackLabelGenerate";
                
                // check if the input container already exists in the grid
                bool bNonExistingContainer = true;

                if ((_gridContainer.GridContext as BoundContext).GetTotalRows() > 0)
                {
                    string strSelectedGridId = "";

                    if (Page.IsPostBack)
                        strSelectedGridId = (_gridContainer.GridContext as BoundContext).GetRowIdByCellValue("Lot", sContainerName);

                    if (string.IsNullOrEmpty(strSelectedGridId))
                        bNonExistingContainer = true;
                    else
                        bNonExistingContainer = false;
                }
                else
                {
                    _txtIsItemProcessing.Data = ResData.Value.IsItemProcessing.ToString();
                    _txtFirstLot.Data = sContainerName;
                    SetControls();
                }

                if (bNonExistingContainer)
                {
                    JQDataGrid _gridContainerTemp = Page.FindCamstarControl("ss_TrackLabelGenerate_ContainerGrid") as JQDataGrid;
                    SEMI.AppCode.UIUtility.GetLotQuerySelection(this, sServiceType, sContainerName, false, ref _gridContainerTemp, _gridContainerTemp.ID.ToString(), true);
                }
            } //Results.IsSuccess
            else
            {
                Page.DisplayMessage(Results);
            }
        } // FetchData

        //---------------------------------------------------
        // Fetch Label Plan
        //---------------------------------------------------
        private void FetchLabelPlan()
        {
            //Initialize Service & Objects
            UserProfile profile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
            ss_LabelPlanMaintService Svc = new ss_LabelPlanMaintService(profile);
            ss_LabelPlanMaint SvcData = new ss_LabelPlanMaint();
            ss_LabelPlanMaint_Info SvcInfo = new ss_LabelPlanMaint_Info();
            ss_LabelPlanMaint_Request ReqData = new ss_LabelPlanMaint_Request();
            ss_LabelPlanMaint_Result ResData = new ss_LabelPlanMaint_Result();

            //Set Input Data
            SvcData.ObjectToChange = new NamedObjectRef();
            if (_ndoLabelPlan.Data != null)
                SvcData.ObjectToChange.Name = _ndoLabelPlan.Data.ToString();

            SvcInfo.ObjectChanges = new ss_LabelPlanChanges_Info();
            SvcInfo.ObjectChanges.RequestValue = true;
            ReqData.Info = SvcInfo;

            //Execute Request
            ResultStatus Results = Svc.Load(SvcData, ReqData, out ResData);
            if (Results.IsSuccess)
            {
                if (ResData.Value.ObjectChanges != null)
                    _gridLabelPlanDetails.Data = ResData.Value.ObjectChanges.ss_LabelPlanDetails.ToArray();
            }

        } // FetchLabelPlan

        //---------------------------------------------------
        // Web Part Custom Action
        //---------------------------------------------------
        public override void WebPartCustomAction(object sender, CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as Camstar.WebPortal.Personalization.CustomAction;

            if (action != null && action.Parameters == "Reset")
            {
                Page.ShopfloorReset(sender, e);
                _chkItemBased.ReadOnly = false;
                _chkQtyBased.ReadOnly = false;
            }
            else if (action != null && action.Parameters == "CustomSubmit")
            {
                e.Result = CustomSubmit();
                if (e.Result.IsSuccess)
                    Page.ShopfloorReset(sender, e);
            }
        } // WebPartCustomAction

        //---------------------------------------------------
        // Track Label Generate Submit Txn
        //---------------------------------------------------
        public ResultStatus CustomSubmit()
        {
            // Prepare service
            var fs = FrameworkManagerUtil.GetFrameworkSession();
            ResultStatus oServiceResult = new ResultStatus(null, false);
            string sServiceType = "ss_TrackLabelGenerate";
            
            // Run proper constructor. We need to be dynamic with the primary service type
            var svcType = WCFObject.CreateObjectType(sServiceType + "Service");
            // create a request object
            var oRequest = WCFObject.CreateObject(sServiceType + "_Request");
            var svcConstructor = svcType.GetConstructor(new Type[] { typeof(UserProfile) });
            var oService = svcConstructor.Invoke(new object[] { fs.CurrentUserProfile });

            // retriving data dynamically for Flexibility of the page.
            var oServiceData = CreateServiceData(sServiceType);

            (oServiceData as ss_TrackLabelGenerate).ComputerName = _txtComputerName.Data != null ? _txtComputerName.Data.ToString() : null;
            (oServiceData as ss_TrackLabelGenerate).Employee = _ndoEmployee.Data != null ? new NamedObjectRef(_ndoEmployee.Data.ToString()) : null;

            // Get Container Grid Data
            DataTable getDataTable;
            int gridCount = _gridContainer.GridContext.GetTotalRows();
            (_gridContainer.GridContext as BoundContext).GenerateFullExcelData(gridCount, out getDataTable);
            if (gridCount > 0)
            {
                (oServiceData as ss_TrackLabelGenerate).ss_LabelSourceInput = new ss_LabelSourceInput[gridCount];
                for (int i = 0; i < gridCount; i++)
                {
                    (oServiceData as ss_TrackLabelGenerate).ss_LabelSourceInput[i] = new ss_LabelSourceInput();
                    (oServiceData as ss_TrackLabelGenerate).ss_LabelSourceInput[i].SourceID = getDataTable.Rows[i].Field<String>("Lot").ToString();
                    (oServiceData as ss_TrackLabelGenerate).ss_LabelSourceInput[i].SourceQty = new Primitive<double>();
                    if (_chkQtyBased.IsChecked)
                        (oServiceData as ss_TrackLabelGenerate).ss_LabelSourceInput[i].SourceQty.Value = Convert.ToDouble(getDataTable.Rows[i].Field<String>("Qty").ToString());
                    else if(_chkItemBased.IsChecked)
                        (oServiceData as ss_TrackLabelGenerate).ss_LabelSourceInput[i].SourceQty.Value = Convert.ToDouble(getDataTable.Rows[i].Field<String>("Qty2").ToString());
                    else if (_txtIsItemProcessing.Data.ToString().Equals("True"))
                        (oServiceData as ss_TrackLabelGenerate).ss_LabelSourceInput[i].SourceQty.Value = Convert.ToDouble(getDataTable.Rows[i].Field<String>("Qty2").ToString());
                    else
                        (oServiceData as ss_TrackLabelGenerate).ss_LabelSourceInput[i].SourceQty.Value = Convert.ToDouble(getDataTable.Rows[i].Field<String>("Qty").ToString());
                }
            }

            (oServiceData as ss_TrackLabelGenerate).ss_LabelPlan = _ndoLabelPlan.Data != null ? new NamedObjectRef(_ndoLabelPlan.Data.ToString()) : null;

            // init the result object
            Result oResult = new Result();
            ResultStatus oResultStatus = new ResultStatus();
            oResultStatus = (oService as IShopFloorBase).ExecuteTransaction((oServiceData as DCObject));
            return oResultStatus;

        } // CustomSubmit
    }
}



