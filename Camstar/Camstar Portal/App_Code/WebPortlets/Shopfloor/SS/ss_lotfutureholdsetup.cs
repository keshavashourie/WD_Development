/* Copyright 2020 Siemens */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Web;
using System.Reflection;
using System.Reflection.Emit;
using System.ComponentModel;

using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework.Utilities;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using PERS = Camstar.WebPortal.Personalization;
using CamstarPortal.WebControls;
using Camstar.WCF.Services;
using System.Collections;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.Constants;


namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    /// <summary>
    /// Summary description for LotFutureHoldSetupTxn
    /// </summary>
    public class LotFutureHoldSetup : scsShopfloorBase
    {
        #region Properties 
        // Hidden objects
        CWC.TextBox _txtComputerNameField { get { return Page.FindCamstarControl("LotFutureHoldSetup_ComputerName") as CWC.TextBox; } }
        JQDataGrid _SelectedContainersGrid { get { return Page.FindCamstarControl("LotFutureHoldSetup_Containers") as JQDataGrid; } }
        JQDataGrid _DetailsGrid { get { return Page.FindCamstarControl("LotFutureHoldSetup_Details") as JQDataGrid; } }
        CWC.CheckBox _chkUpdateOnlyField { get { return Page.FindCamstarControl("LotFutureHoldSetup_UpdateOnly") as CWC.CheckBox; } }
        CWC.TextBox _selectionIdField { get { return Page.FindCamstarControl("SelectionIdField") as CWC.TextBox; } }
        CWC.NamedObject _EmployeeField { get { return Page.FindCamstarControl("LotFutureHoldSetup_Employee") as CWC.NamedObject; } }
        
        SEMI.AppCode.DataEnvelopControl _envSelectedLots { get { return Page.FindCamstarControl("SelectedLotsList") as SEMI.AppCode.DataEnvelopControl; } }

        #endregion

        #region Constants

        #endregion

        #region Functions

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!Page.IsPostBack)
            {
                // Get Computer name
                _txtComputerNameField.Data = SEMI.AppCode.UIUtility.GetComputerName(this);
            }
            else
            {
                //if the page has been submitted and cleared, get the computer name again.
                if (_txtComputerNameField.Data != null)
                    _txtComputerNameField.Data = SEMI.AppCode.UIUtility.GetComputerName(this);

                _selectionIdField.DataChanged += new EventHandler(_selectionIdField_DataChanged);
                // Check if it is a pop up close, get the return result
                if (SEMI.AppCode.UIUtility.IsPopupClose(this) && (Page.PortalContext.DataContract.GetValueByName("Popup_SelectedStackDM") == null))
                    PopulateContainersList();

                if (Page.EventArgument == "FloatingFrameSubmitParentPostBackArgument" && (Page.PortalContext.DataContract.GetValueByName("Popup_SelectedStackDM") != null))
                {
                    var scsWIPStepWorkflow = Page.PortalContext.DataContract.GetValueByName<RevisionedObjectRef>("Popup_SelectedWorkflowDM");
                    var scsWIPStep = Page.PortalContext.DataContract.GetValueByName<NamedSubentityRef>("Popup_SelectedStepDM");
                    var scsWorkflowStack = Page.PortalContext.DataContract.GetValueByName<NamedSubentityRef[]>("Popup_SelectedStackDM");
                    var rowid = Page.PortalContext.DataContract.GetValueByName<string>("LotFutureHoldSetup_Details_SelectedRowIdDM");
                    if (!string.IsNullOrEmpty(rowid))
                    {
                        int iRowId = int.Parse(rowid);
                        var data = _DetailsGrid.Data as OM.LotFutureHoldSetupDetails[];
                        data[iRowId].scsWIPStepWorkflow = scsWIPStepWorkflow;
                        data[iRowId].scsWIPStep = scsWIPStep;
                        data[iRowId].scsWorkflowStack = scsWorkflowStack;
                        if (data[iRowId].scsWorkflowStack == null || data[iRowId].scsWorkflowStack.Length == 0)
                        {
                            data[iRowId].scsWorkflowStackKey = null;
                            data[iRowId].scsWIPStepMainWorkflowName = data[iRowId].scsWIPStepWorkflow != null ? data[iRowId].scsWIPStepWorkflow.Name : "";
                        }
                        else
                            data[iRowId].scsWIPStepMainWorkflowName = data[iRowId].scsWorkflowStack.Last().Parent.ToString();

                    }

                    // clear the data contracts as the mess with the data loading of grid values
                    Page.PortalContext.DataContract.SetValueByName("Popup_SelectedWorkflowDM", null);
                    Page.PortalContext.DataContract.SetValueByName("Popup_SelectedStepDM", null);
                    Page.PortalContext.DataContract.SetValueByName("Popup_SelectedStackDM", null);
                    Page.PortalContext.DataContract.SetValueByName("LotFutureHoldSetup_Details_SelectedRowIdDM", null);
                }
            }
        }

        private void PopulateContainersList()
        {
            //if the containers list is not empty, process each one
            if (_envSelectedLots.SS_ContainersList != null)
            {
                string[] sContainers;
                sContainers = _envSelectedLots.SS_ContainersList;
                foreach (string sContainersItem in sContainers)
                {
                    _selectionIdField.TextControl.Text = sContainersItem;
                    _selectionIdField_DataChanged(null, null);
                }

                //nullify the containers list
                _envSelectedLots.SS_ContainersList = null;
            }
        }

        public override void GetInputData(Service serviceData)
        {
            // Manually build the transaction by populating all the data
            ((OM.LotFutureHoldSetup)serviceData).ComputerName = _txtComputerNameField.TextControl.Text;
            ((OM.LotFutureHoldSetup)serviceData).UpdateOnly = _chkUpdateOnlyField.CheckControl.Checked;
            ((OM.LotFutureHoldSetup)serviceData).Employee = (OM.NamedObjectRef)_EmployeeField.Data;
            if (_SelectedContainersGrid.GridContext.GetTotalRows() > 0)
            {
                ((OM.LotFutureHoldSetup)serviceData).Containers = new ContainerRef[_SelectedContainersGrid.GridContext.GetTotalRows()];
                int i = 0;

                while (i < _SelectedContainersGrid.GridContext.GetTotalRows())
                {
                    string sRowId = i.ToString().PadLeft(6, '0');

                    ContainerRef container = new ContainerRef(_SelectedContainersGrid.GridContext.GetCell(sRowId, _SelectedContainersGrid.Settings.Columns[0].Name).ToString());
                    ((OM.LotFutureHoldSetup)serviceData).Containers.SetValue(container, i++);                    
                }
            }
            if (_DetailsGrid.GridContext.GetTotalRows() > 0)
            {
                OM.LotFutureHoldSetupDetails[] details = new LotFutureHoldSetupDetails[_DetailsGrid.GridContext.GetTotalRows()];
                int i = 0;
                foreach (OM.LotFutureHoldSetupDetails detail in ((OM.LotFutureHoldSetupDetails[])_DetailsGrid.Data))
                {
                    OM.LotFutureHoldSetupDetails newDetail = new LotFutureHoldSetupDetails();
                    newDetail.Comments = detail.Comments;
                    newDetail.EmailGroup = detail.EmailGroup;
                    newDetail.ExpectedHoldDays = detail.ExpectedHoldDays;
                    newDetail.HoldLocation = detail.HoldLocation;
                    newDetail.HoldReason = detail.HoldReason;
                    newDetail.IncludeChildLots = detail.IncludeChildLots;
                    newDetail.Spec = detail.Spec;
                    newDetail.scsWIPStep = detail.scsWIPStep;
                    newDetail.scsWIPStepMainWorkflowName = detail.scsWIPStepMainWorkflowName;
                    newDetail.scsWIPStepWorkflow = detail.scsWIPStepWorkflow;
                    newDetail.scsWorkflowStack = detail.scsWorkflowStack;
                    newDetail.scsWorkflowStackKey = detail.scsWorkflowStackKey;
                    
                    details.SetValue(newDetail, i);
                    i++;
                }
                ((OM.LotFutureHoldSetup)serviceData).Details = details;
            }
        }

        void _selectionIdField_DataChanged(object sender, EventArgs e)
        {
            try
            {
                if (_selectionIdField.TextControl.Text != null)
                {
                    Page.StatusBar.ClearMessage();
                    // get the session and user profile
                    var fs = FrameworkManagerUtil.GetFrameworkSession();

                    // init the service, service data and service info objects
                    LotFutureHoldSetupService objSvc = new LotFutureHoldSetupService(fs.CurrentUserProfile);
                    OM.LotFutureHoldSetup objSvcData = new OM.LotFutureHoldSetup { SelectionId = (string)_selectionIdField.Data };
                    LotFutureHoldSetup_Info objSvcInfo = new LotFutureHoldSetup_Info { Containers = FieldInfoUtil.RequestValue() };
                    LotFutureHoldSetupDetails_Info objSvcDetails = new LotFutureHoldSetupDetails_Info();
                    objSvcDetails.Spec = FieldInfoUtil.RequestValue();
                    objSvcDetails.scsWIPStep = FieldInfoUtil.RequestValue();
                    objSvcDetails.scsWIPStepWorkflow = FieldInfoUtil.RequestValue();
                    objSvcDetails.scsWorkflowStack = FieldInfoUtil.RequestValue();
                    objSvcDetails.scsWorkflowStackKey = FieldInfoUtil.RequestValue();
                    objSvcDetails.scsWIPStepMainWorkflowName = FieldInfoUtil.RequestValue();
                    objSvcDetails.HoldLocation = FieldInfoUtil.RequestValue();
                    objSvcDetails.HoldReason = FieldInfoUtil.RequestValue();
                    objSvcDetails.IncludeChildLots = FieldInfoUtil.RequestValue();
                    objSvcDetails.EmailGroup = FieldInfoUtil.RequestValue();
                    objSvcDetails.ExpectedHoldDays = FieldInfoUtil.RequestValue();
                    objSvcDetails.Comments = FieldInfoUtil.RequestValue();

                    objSvcInfo.CurrentDetails = objSvcDetails;
                    // init the result object
                    LotFutureHoldSetup_Result objResult = new LotFutureHoldSetup_Result();

                    // execute to request the value
                    ResultStatus resultStatus = objSvc.ResolveSelectionId(objSvcData, new LotFutureHoldSetup_Request { Info = objSvcInfo }, out objResult);

                    if (resultStatus.IsSuccess)
                    {
                        JQDataGrid _gridContainers = _SelectedContainersGrid;
                        if(objResult.Value.Containers != null && objResult.Value.Containers.Length > 0)
                            foreach (OM.ContainerRef oContainer in objResult.Value.Containers)
                            {
                                // check if lot not exist in the grid else add to the grid
                                string strSelectedGridId = (_gridContainers.GridContext as BoundContext).GetRowIdByCellValue("Name", oContainer.Name.ToString());

                                if (string.IsNullOrEmpty(strSelectedGridId))
                                {
                                    SEMI.AppCode.UIUtility.GetLotQuerySelection(this, "LotFutureHoldSetup", oContainer.Name.ToString(), false, ref _gridContainers, "LotFutureHoldSetup_Containers", true);                                
                                }
                            }

                        // Populate the details grid
                        if (objResult.Value.CurrentDetails != null && objResult.Value.CurrentDetails.Length > 0)
                            foreach (LotFutureHoldSetupDetails detail in objResult.Value.CurrentDetails)
                            {
                                bool bIsNewRecord = true;
                                LotFutureHoldSetupDetails[] currentgridlist = _DetailsGrid.Data != null ? _DetailsGrid.Data as LotFutureHoldSetupDetails[] : null ;
                                if (currentgridlist != null)
                                    bIsNewRecord = currentgridlist.FirstOrDefault(x => (x.scsWIPStep == detail.scsWIPStep && x.scsWIPStepWorkflow == detail.scsWIPStepWorkflow && x.scsWorkflowStackKey == detail.scsWorkflowStackKey && x.Spec==null&&detail.Spec==null) || x.Spec == detail.Spec &&x.scsWIPStep==null &&detail.scsWIPStep==null)==null;
                                
                                if (bIsNewRecord)
                                {
                                    _DetailsGrid.Action_AddEmptyRow();
                                    ((OM.LotFutureHoldSetupDetails[])_DetailsGrid.Data).SetValue(detail, _DetailsGrid.GridContext.GetTotalRows() - 1);
                                }
                            }
                    }
                    else
                        DisplayMessage(resultStatus);
                }
            }
            catch (Exception Ex)
            {
                DisplayMessage(new OM.ResultStatus(Ex.TargetSite.Name + "(): " + Ex.Message, false));
            }
            finally
            {
                _selectionIdField.TextControl.Text = "";
                _selectionIdField.Focus();
            }
        }

        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as Camstar.WebPortal.Personalization.CustomAction;
            if (action != null && action.Parameters == "Clear")
            {
                Page.ClearValues();
            }
        }

        #endregion
    }
}



