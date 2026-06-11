/* Copyright 2023 Siemens */
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using OM = Camstar.WCF.ObjectStack;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.WCFUtilities;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    /// <summary>
    /// TODO: Add a Summary description for this Camstar Web Part
    /// </summary>
    public class scsDBContainerClosure : MatrixWebPart
    {

        #region Constants

        #endregion

        #region Private Member Variables
        private List<ContainerClosure> containerClosureDetails;
        private List<string> containerWithParent;
        private int counter = 0;
        private string tmpParentContainerName = String.Empty;
        private string[] selectedRowIndex;
        #endregion

        #region Controls

        // Control properties
        // TextBox
        protected CWC.TextBox _txtSelectionId { get { return Page.FindCamstarControl("scsDBContainerClosure_SelectionId") as CWC.TextBox; } }
        protected CWC.TextBox _txtScanField { get { return Page.FindCamstarControl("CheckboxSelectionField") as CWC.TextBox; } }
		protected CWC.TextBox _txtComputerName { get { return Page.FindCamstarControl("scsDBContainerClosure_ComputerName") as CWC.TextBox; } }

        // NamedObject
        protected CWC.NamedObject _terminateReason { get { return Page.FindCamstarControl("scsDBContainerClosure_TerminateReason") as CWC.NamedObject; } }
        protected CWC.NamedObject _terminateAccount { get { return Page.FindCamstarControl("scsDBContainerClosure_TerminateAccount") as CWC.NamedObject; } }

        // JQGrid
        protected JQDataGrid _details { get { return Page.FindCamstarControl("scsDBContainerClosure_Details") as JQDataGrid; } }

        #endregion

        #region Protected Functions

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _txtSelectionId.DataChanged += new EventHandler(SelectionId_DataChanged);
            _txtScanField.DataChanged += new EventHandler(ScanField_DataChanged);
			_txtComputerName.TextControl.Text = SEMI.AppCode.UIUtility.GetComputerName(this);
        }

        #endregion

        #region Public Functions

        public void SelectionId_DataChanged(object sender, EventArgs e)
        {
            try
            {
                if (_txtSelectionId.Data != null)
                {
                    FetchData();
                }
            }
            catch (Exception Ex)
            {
                DisplayMessage(new OM.ResultStatus(Ex.TargetSite.Name + "(): " + Ex.Message, false));
            }
            finally
            {
                _txtSelectionId.Focus();
            }
        }

        public void ScanField_DataChanged(object sender, EventArgs e)
        {
            try
            {
                if (_txtScanField.Data != null)
                {
                    SelectGrid();
                }
            }
            catch (Exception Ex)
            {
                DisplayMessage(new OM.ResultStatus(Ex.TargetSite.Name + "(): " + Ex.Message, false));
            }
            finally
            {
                _txtScanField.Focus();
            }
        }

        public override void GetInputData(Service serviceData)
        {
            base.GetInputData(serviceData);

            if (PrimaryServiceType.Equals("scsDBContainerClosure"))
            {
                tmpParentContainerName = String.Empty;
                if (_details != null && _details.SelectedRowCount > 0)
                {
                    containerClosureDetails = new List<ContainerClosure>();
                    selectedRowIndex = (string[])_details.SelectedRowIDs;

                    foreach (string SelectedChildIndex in selectedRowIndex)
                    {
                        string ParentName = String.Empty;
                        if (_details.GridContext.GetCell(SelectedChildIndex, "ParentContainerName") != null)
                            ParentName = _details.GridContext.GetCell(SelectedChildIndex, "ParentContainerName").ToString();

                        containerClosureDetails.Add(new ContainerClosure(
                           _details.GridContext.GetCell(SelectedChildIndex, "ContainerName").ToString(), ParentName));
                    }

                    containerWithParent = new List<string>();
                    foreach (ContainerClosure child in containerClosureDetails)
                    {

                        if (!string.IsNullOrEmpty(child.ContainerParentName))
                        {
                            if (!tmpParentContainerName.Equals(child.ContainerParentName))
                            {
                                tmpParentContainerName = child.ContainerParentName;
                                var selectedParentContainer = containerClosureDetails
                                    .Where(x => x.ContainerParentName.Equals(tmpParentContainerName))
                                    .Select(x => x.ContainerName).ToArray();

                                containerWithParent.Add(String.Join("|", selectedParentContainer));
                            }
                        }
                    }

                    var ContainerArray = containerClosureDetails.Select(x => x.ContainerName).ToArray();
                    var ContainerParentArray = containerClosureDetails.GroupBy(x => x.ContainerParentName).Select(x => x.Key).Where(x => !string.IsNullOrEmpty(x)).ToArray();

                    if (ContainerArray != null)
                        (serviceData as Camstar.WCF.ObjectStack.scsDBContainerClosure).ContainerNamesStr = String.Join("|", ContainerArray);

                    if (ContainerParentArray.Count() > 0)
                    {
                        //string containerLongString = String.Join("|", ContainerParentArray);
                        (serviceData as Camstar.WCF.ObjectStack.scsDBContainerClosure).ContainerParentStr = String.Join("|", ContainerParentArray);
                    }

                    var _containerWithParent = (from s in containerWithParent select new Primitive<string>(s)).ToArray();
                    if (_containerWithParent != null)
                        (serviceData as Camstar.WCF.ObjectStack.scsDBContainerClosure).ContainerNamesDisaStr = _containerWithParent;
                }
            }
        }

        #endregion

        #region Private Functions

        private void FetchData()
        {
            try
            {
                string sServiceType = Page.PrimaryServiceType;
                // get the session and user profile
                var fs = FrameworkManagerUtil.GetFrameworkSession();

                scsDBContainerClosureService oService = new scsDBContainerClosureService(fs.CurrentUserProfile);
                OM.scsDBContainerClosure oServiceData = new OM.scsDBContainerClosure();
                scsDBContainerClosure_Info oServiceInfo = new scsDBContainerClosure_Info();
                ResultStatus oResultStatus = new ResultStatus();


                if (!string.IsNullOrEmpty(_txtSelectionId.TextControl.Text))
                {
                    oServiceData.SelectionId = _txtSelectionId.Data.ToString();

                    // Requesting for containers
                    oServiceInfo.Containers = FieldInfoUtil.RequestValue();

                    scsDBContainerClosure_Request oServiceRequest = new scsDBContainerClosure_Request();
                    oServiceRequest.Info = oServiceInfo;

                    scsDBContainerClosure_Result oServiceResult = new scsDBContainerClosure_Result();

                    // execute the request selection values
                    ResultStatus resultStatus = oService.ResolveSelectionId(oServiceData, oServiceRequest, out oServiceResult);

                    if (resultStatus.IsSuccess)
                    {

                        foreach (OM.ContainerRef container in oServiceResult.Value.Containers)
                        {
                            OM.scsDBContainerClosure CustomServiceData = new OM.scsDBContainerClosure();
                            CustomServiceData.Container = container;
                            scsDBContainerClosure_Result CustomServiceResult = new scsDBContainerClosure_Result();
                            scsDBContainerClosure_Info CustomServiceInfo = new scsDBContainerClosure_Info();
                            CustomServiceInfo.DetailsTemp = new scsDBContainerClosureDetails_Info();
                            //CustomServiceInfo.DetailsTemp.Container = FieldInfoUtil.RequestValue();
                            CustomServiceInfo.DetailsTemp.ContainerName = FieldInfoUtil.RequestValue();
                            //CustomServiceInfo.DetailsTemp.ParentContainer = FieldInfoUtil.RequestValue();
                            CustomServiceInfo.DetailsTemp.ParentContainerName = FieldInfoUtil.RequestValue();
                            CustomServiceInfo.DetailsTemp.ParentNameDisplay = FieldInfoUtil.RequestValue();
                            CustomServiceInfo.DetailsTemp.ParentQualityStatusReason = FieldInfoUtil.RequestValue();
                            CustomServiceInfo.DetailsTemp.QualityStatusReason = FieldInfoUtil.RequestValue();
                            CustomServiceInfo.DetailsTemp.WorkflowStep = FieldInfoUtil.RequestValue();
                            CustomServiceInfo.DetailsTemp.WorkOrder = FieldInfoUtil.RequestValue();
                            CustomServiceInfo.DetailsTemp.PendingLotState = FieldInfoUtil.RequestValue();
                            CustomServiceInfo.DetailsTemp.Equipment = FieldInfoUtil.RequestValue();

                            scsDBContainerClosure_Request CustomServiceRequest = new scsDBContainerClosure_Request();
                            CustomServiceRequest.Info = CustomServiceInfo;

                            ResultStatus CustomizedResult = oService.GetContainerDetails(CustomServiceData, CustomServiceRequest, out CustomServiceResult);

                            if (CustomizedResult.IsSuccess)
                            {
                                if (_details.Data == null)
                                {
                                    _details.Data = CustomServiceResult.Value.DetailsTemp;
                                    CamstarWebControl.SetRenderToClient(_details);
                                }
                                else
                                {
                                    scsDBContainerClosureDetails[] existingDetails = _details.Data as scsDBContainerClosureDetails[];
                                    List<scsDBContainerClosureDetails> existingDetailsList = existingDetails.ToList();
                                    List<scsDBContainerClosureDetails> newDetailsList = CustomServiceResult.Value.DetailsTemp.ToList();
                                    var dict = existingDetailsList.ToDictionary(p => p.ContainerName);
                                    foreach (var detail in newDetailsList)
                                    {
                                        if(!existingDetailsList.Contains(detail))
                                            dict[detail.ContainerName] = detail;
                                    }
                                    //Sort according to the parent container name
                                    List<scsDBContainerClosureDetails> hasParent = dict.Values.Where(m => m.ParentContainerName != null).OrderBy(n => n.ParentContainerName.Value).ToList();
                                    List<scsDBContainerClosureDetails> noParent = dict.Values.Where(m => m.ParentContainerName == null).ToList();
                                    List<scsDBContainerClosureDetails> merged = hasParent.Concat(noParent).ToList();
                                    _details.Data = merged.ToArray();
                                    CamstarWebControl.SetRenderToClient(_details);
                                }
                            }
                            else
                            { Page.DisplayMessage(CustomizedResult); }
                        }
                        _txtSelectionId.TextControl.Text = "";

                    }
                    else
                    {
                        OM.scsDBContainerClosure CustomServiceData = new OM.scsDBContainerClosure();
                        CustomServiceData.SelectionId = _txtSelectionId.Data.ToString();
                        scsDBContainerClosure_Result CustomServiceResult = new scsDBContainerClosure_Result();
                        scsDBContainerClosure_Info CustomServiceInfo = new scsDBContainerClosure_Info();
                        CustomServiceInfo.DetailsTemp = new scsDBContainerClosureDetails_Info();
                        //CustomServiceInfo.DetailsTemp.Container = FieldInfoUtil.RequestValue();
                        CustomServiceInfo.DetailsTemp.ContainerName = FieldInfoUtil.RequestValue();
                        //CustomServiceInfo.DetailsTemp.ParentContainer = FieldInfoUtil.RequestValue();
                        CustomServiceInfo.DetailsTemp.ParentContainerName = FieldInfoUtil.RequestValue();
                        CustomServiceInfo.DetailsTemp.ParentNameDisplay = FieldInfoUtil.RequestValue();
                        CustomServiceInfo.DetailsTemp.ParentQualityStatusReason = FieldInfoUtil.RequestValue();
                        CustomServiceInfo.DetailsTemp.QualityStatusReason = FieldInfoUtil.RequestValue();
                        CustomServiceInfo.DetailsTemp.WorkflowStep = FieldInfoUtil.RequestValue();
                        CustomServiceInfo.DetailsTemp.WorkOrder = FieldInfoUtil.RequestValue();
                        CustomServiceInfo.DetailsTemp.PendingLotState = FieldInfoUtil.RequestValue();
                        CustomServiceInfo.DetailsTemp.Equipment = FieldInfoUtil.RequestValue();

                        scsDBContainerClosure_Request CustomServiceRequest = new scsDBContainerClosure_Request();
                        CustomServiceRequest.Info = CustomServiceInfo;

                        ResultStatus CustomizedResult = oService.ResolveSelectionIdByWorkOrder(CustomServiceData, CustomServiceRequest, out CustomServiceResult);

                        if (CustomizedResult.IsSuccess)
                        {
                            if (_details.Data == null)
                            {
                                if (CustomServiceResult.Value.DetailsTemp != null)
                                {
                                    _details.Data = _details.Data = CustomServiceResult.Value.DetailsTemp;
                                    CamstarWebControl.SetRenderToClient(_details);
                                }
                                else
                                {
                                    Page.DisplayMessage(resultStatus);
                                }
                            }
                            else
                            {
                                if (CustomServiceResult.Value.DetailsTemp != null)
                                {
                                    scsDBContainerClosureDetails[] existingDetails = _details.Data as scsDBContainerClosureDetails[];
                                    List<scsDBContainerClosureDetails> existingDetailsList = existingDetails.ToList();
                                    List<scsDBContainerClosureDetails> newDetailsList = CustomServiceResult.Value.DetailsTemp.ToList();
                                    var dict = existingDetailsList.ToDictionary(p => p.ContainerName);
                                    foreach (var detail in newDetailsList)
                                    {
                                        if (!existingDetailsList.Contains(detail))
                                            dict[detail.ContainerName] = detail;
                                    }
                                    //Sort according to the parent container name
                                    List<scsDBContainerClosureDetails> hasParent = dict.Values.Where(m => m.ParentContainerName != null).OrderBy(n => n.ParentContainerName.Value).ToList();
                                    List<scsDBContainerClosureDetails> noParent = dict.Values.Where(m => m.ParentContainerName == null).ToList();
                                    List<scsDBContainerClosureDetails> merged = hasParent.Concat(noParent).ToList();
                                    _details.Data = merged.ToArray();
                                    CamstarWebControl.SetRenderToClient(_details);
                                }
                                else
                                {
                                    Page.DisplayMessage(resultStatus);
                                }
                            }
                            _txtSelectionId.TextControl.Text = "";
                        }
                        else
                        {
                            Page.DisplayMessage(CustomizedResult);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Page.DisplayMessage(ex.Message.ToString(), false);
            }
        }

        private void SelectGrid()
        {
            try
            {
                if (!string.IsNullOrEmpty(_txtScanField.TextControl.Text) && _details.Data != null)
                {
                    string scannedText = _txtScanField.TextControl.Text;
                    int lastIndex = 0;
                    List<string> rowIds = new List<string>();

                    if (_details.GridContext.SelectedRowIDs != null)
                    {
                        rowIds = _details.GridContext.SelectedRowIDs;
                    }

                    scsDBContainerClosureDetails[] existingDetails = _details.Data as scsDBContainerClosureDetails[];

                    foreach (scsDBContainerClosureDetails detail in existingDetails)
                    {
                        string workOrderName = detail.WorkOrder != null ? detail.WorkOrder.Name : "";
                        if (detail.ParentContainerName == scannedText || workOrderName == scannedText)
                        {
                            //string rowId = _details.GridContext.GetRowId(detail.ListItemIndex.Value);
                            string rowId = lastIndex.ToString("D6");
                            var foundId = rowIds.FirstOrDefault(x => x.Contains(rowId));
                            if (foundId == null)
                                rowIds.Add(rowId);
                        }
                        lastIndex++;
                    }
                    _details.GridContext.SelectedRowIDs = rowIds;
                    CamstarWebControl.SetRenderToClient(_details);
                    _txtScanField.TextControl.Text = "";
                }
            }
            catch (Exception ex)
            {
                Page.DisplayMessage(ex.Message.ToString(), false);
            }
        }

        #endregion

    }

    class ContainerClosure
    {
        public string ContainerName { get; private set; } = String.Empty;
        public string ContainerParentName { get; private set; } = String.Empty;

        //Empty Constructor
        public ContainerClosure() { }

        // Constructor with parameters
        public ContainerClosure(string _containerName, string _containerParentName)
        {
            ContainerName = _containerName;
            ContainerParentName = _containerParentName;
        }

    }
}

