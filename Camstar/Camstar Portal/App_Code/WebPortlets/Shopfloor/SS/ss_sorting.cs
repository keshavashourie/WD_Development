/* Copyright 2025 Siemens */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Web;
using System.Web.UI;

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
/// Summary description for SS_Sorting
/// </summary>
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_Sorting : MatrixWebPart
    {

        protected CWC.TextBox _txtSelectionID { get { return Page.FindCamstarControl("Sorting_SelectionId") as CWC.TextBox; } }
        protected CWC.TextBox _txtComputerName { get { return Page.FindCamstarControl("Sorting_ComputerName") as CWC.TextBox; } }
        protected CWC.NamedObject _ndoEmployee { get { return Page.FindCamstarControl("Sorting_Employee") as CWC.NamedObject; } }
        protected ContainerListGrid _contContainer { get { return Page.FindCamstarControl("Sorting_Container") as ContainerListGrid; } }
        protected JQDataGrid _gridCurrentWaferDetails { get { return Page.FindCamstarControl("Sorting_CurrentWaferDetails") as JQDataGrid; } }
        protected CWC.CheckBox _chkIsActive { get { return Page.FindCamstarControl("Sorting_IsActive") as CWC.CheckBox; } }
        private CWC.Button _btnSubmit { get { return Page.FindCamstarControl("SubmitButton") as CWC.Button; } }
        private CWC.Button _btnReset { get { return Page.FindCamstarControl("ResetButton") as CWC.Button; } }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _txtSelectionID.DataChanged += new EventHandler(_txtSelectionID_DataChanged);
            _txtComputerName.Data = SEMI.AppCode.UIUtility.GetComputerName(this);
            // additional logic where the web part is referenced into a standalone page
            if (Page.IsAJAXFloatingFrame)
            {
                // hide the submit and reset buttons
                _btnSubmit.Visible = false;
                _btnSubmit.Enabled = false;
                _btnReset.Visible = false;
                _btnReset.Enabled = false;
            }
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        void _txtSelectionID_DataChanged(object sender, EventArgs e)
        {
            if (_txtSelectionID.Data != null)
            {
                FetchSortingDetails();
            }
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        public void FetchSortingDetails()
        {
            try
            {
                bool bExecute = false;
                if (_chkIsActive != null)
                    bExecute = _chkIsActive.CheckControl.Checked;
                if (bExecute)
                {
                    var fs = FrameworkManagerUtil.GetFrameworkSession();
                    OM.Sorting oServiceData = new OM.Sorting();
                    Sorting_Info oServiceInfo = new Sorting_Info();
                    Sorting_Request oRequest = new Sorting_Request();
                    Sorting_Result oResult = new Sorting_Result();
                    SortingService oService = new SortingService(fs.CurrentUserProfile);
                    ResultStatus oResultStatus = new ResultStatus();

                    oServiceData.SelectionId = _txtSelectionID.Data.ToString();

                    oServiceInfo.Container = FieldInfoUtil.RequestValue();

                    oServiceInfo.Qty = FieldInfoUtil.RequestValue();
                    oServiceInfo.Qty2 = FieldInfoUtil.RequestValue();

                    oServiceInfo.CurrentWafersDetails = new SortingWafersDetails_Info();
                    oServiceInfo.CurrentWafersDetails.WaferScribeNumber = FieldInfoUtil.RequestValue();
                    oServiceInfo.CurrentWafersDetails.WaferNumber = FieldInfoUtil.RequestValue();
                    oServiceInfo.CurrentWafersDetails.Equipment = FieldInfoUtil.RequestValue();
                    oServiceInfo.CurrentWafersDetails.Grade = FieldInfoUtil.RequestValue();
                    oServiceInfo.CurrentWafersDetails.NDPW = FieldInfoUtil.RequestValue();
                    oServiceInfo.CurrentWafersDetails.GoodQty = FieldInfoUtil.RequestValue();
                    oServiceInfo.CurrentWafersDetails.SortNoteCode = FieldInfoUtil.RequestValue();
                    oServiceInfo.CurrentWafersDetails.ResortNoteCode = FieldInfoUtil.RequestValue();
                    oServiceInfo.CurrentWafersDetails.Comments = FieldInfoUtil.RequestValue();

                    oServiceInfo.SortingGrades = new ProductSortingGrades_Info();
                    oServiceInfo.SortingGrades.Grade = FieldInfoUtil.RequestValue();

                    oRequest.Info = oServiceInfo;

                    oResultStatus = oService.ResolveSelectionId(oServiceData, oRequest, out oResult);
                    if (oResultStatus.IsSuccess)
                    {
                        if (oResult.Value.Container != null)
                            _contContainer.Data = oResult.Value.Container;

                        if (oResult.Value.CurrentWafersDetails != null)
                        {
                            (_gridCurrentWaferDetails.GridContext as BoundContext).Data = oResult.Value.CurrentWafersDetails.ToArray();
                            _gridCurrentWaferDetails.BoundContext.LoadData();
                            CamstarWebControl.SetRenderToClient(_gridCurrentWaferDetails);
                        }
                        else
                        {
                            _gridCurrentWaferDetails.BoundContext.ClearData();
                        }
                    }
                    else
                    {
                        DisplayMessage(oResultStatus);
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayMessage(new ResultStatus(ex.Message.ToString(), false));
            }
        } // FetchTestParams


        //---------------------------------------------------
        //
        //---------------------------------------------------
        public override void GetInputData(Service serviceData)
        {
            base.GetInputData(serviceData);

            if (serviceData is OM.Sorting)
            {
                SortingWafersDetails[] oWafersDetails = (_gridCurrentWaferDetails.GridContext as BoundContext).Data as SortingWafersDetails[];               
                foreach (SortingWafersDetails oDetail in oWafersDetails)
                {
                    oDetail.ListItemIndex = null;
                    oDetail.Self = null;
                    if (oDetail.Equipment != null)oDetail.Equipment.ID = null;                  
                    oDetail.ListItemAction = ListItemAction.Add;
                }

                (serviceData as OM.Sorting).WafersDetails = oWafersDetails;               
            }
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        public void ReloadState()
        {
            if (_txtSelectionID.Data != null)
            {
                FetchSortingDetails();
            }
            else
            {
                Page.ClearValues();
                _gridCurrentWaferDetails.Clear();
                _gridCurrentWaferDetails.ClearSelectionValues();
            }
        }
    }
}



