/* Copyright 2024 Siemens */
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
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.PortalFramework;

/// <summary>
/// Summary description for SS_LotSplit
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_LotSplit : scsShopfloorBase
    {
        protected CWC.TextBox _txtSelectionId { get { return Page.FindCamstarControl("LotSplit_SelectionId") as CWC.TextBox; } }
        protected CWC.TextBox _txtComputerName { get { return Page.FindCamstarControl("LotSplit_ComputerName") as CWC.TextBox; } }                
        protected CWC.TextBox _txtServiceName { get { return Page.FindCamstarControl("LotSplit_ServiceName") as CWC.TextBox; } }
        protected CWC.TextBox _txtComment { get { return Page.FindCamstarControl("Shopfloor_Comments") as CWC.TextBox; } }

        protected CWC.TextBox _txtMaxStandbyQty { get { return Page.FindCamstarControl("LotSplit_MaxStandbyQty") as CWC.TextBox; } }
        protected CWC.TextBox _txtMaxQtyToProcess { get { return Page.FindCamstarControl("LotSplit_MaxQtyToProcess") as CWC.TextBox; } }
        protected CWC.TextBox _txtMaxInProcessQty { get { return Page.FindCamstarControl("LotSplit_MaxInProcessQty") as CWC.TextBox; } }
        protected CWC.TextBox _txtMaxProcessedQty { get { return Page.FindCamstarControl("LotSplit_MaxProcessedQty") as CWC.TextBox; } }        
        
        protected CWC.TextBox _txtContainerToDelete { get { return Page.FindCamstarControl("LotSplit_ContainerToDelete") as CWC.TextBox; } }
        protected CWC.TextBox _txtWaferSetCount { get { return Page.FindCamstarControl("LotSplit_WaferSetCount") as CWC.TextBox; } }

        protected CWC.NamedObject _ndoProcessType { get { return Page.FindCamstarControl("LotSplit_ProcessType") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoEquipment { get { return Page.FindCamstarControl("LotSplit_Equipment") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoEmployee { get { return Page.FindCamstarControl("LotSplit_Employee") as CWC.NamedObject; } }        

        protected CWC.RevisionedObject _rdoFutureCombineSpec { get { return Page.FindCamstarControl("LotSplit_FutureCombineSpec") as CWC.RevisionedObject; } }
        
        protected CWC.CheckBox _chkCreateNewSchedule { get { return Page.FindCamstarControl("LotSplit_CreateNewSchedule") as CWC.CheckBox; } }
        protected JQDataGrid _gridContainer { get { return Page.FindCamstarControl("LotSplit_ContainerGrid") as JQDataGrid; } }

        protected JQDataGrid _gridDetails { get { return Page.FindCamstarControl("LotSplit_Details") as JQDataGrid; } }
        protected JQDataGrid _gridWafers { get { return Page.FindCamstarControl("LotSplit_Wafers") as JQDataGrid; } }
        
        protected const int _kContainersPerBatch = 20;        
        protected const string _kWafersViewStateKey = "LotSplit_Wafers_ViewStateVariableKey";

        protected SEMI.AppCode.DataEnvelopControl _envSelectedLots { get { return Page.FindCamstarControl("LotSplit_DataEnvelop") as SEMI.AppCode.DataEnvelopControl; } }

        private ToggleContainer _toggleComments { get { return Page.FindCamstarControl("CommentToggle") as ToggleContainer; } }


        //---------------------------------------------------
        //
        //---------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            _txtSelectionId.DataChanged += new EventHandler(_txtSelectionId_DataChanged);            
            _ndoEquipment.DataChanged += new EventHandler(_ndoEquipment_DataChanged);
            _ndoProcessType.DataChanged += new EventHandler(_ndoProcessType_DataChanged);
            _txtComputerName.TextControl.Text = SEMI.AppCode.UIUtility.GetComputerName(this);
            _txtServiceName.Data = Page.PrimaryServiceType.ToString();

			if (Page.IsPostBack)
			{    // Check if it is a pop up close, get the return result
				if (SEMI.AppCode.UIUtility.IsPopupClose(this))
				{
					if (_envSelectedLots != null)
						// manually initialize the containers list data contract since it is not triggered when we do a manual popup close
						if (Page.DataContract.GetValueByName("LotSplit_DataEnvelopDM") != null)
							_envSelectedLots.SS_ContainersList = Page.DataContract.GetValueByName("LotSplit_DataEnvelopDM") as string[];

					if (_envSelectedLots.SS_ContainersList != null)
					{
						string[] sContainers;
						sContainers = _envSelectedLots.SS_ContainersList;

						if (sContainers.Length > 0)
						{
							_txtSelectionId.TextControl.Text = sContainers[0];
							_txtSelectionId_DataChanged(null, null);
						}

						//nullify the containers list
						_envSelectedLots.SS_ContainersList = null;
					}

					var KeyDM = Page.PortalContext.DataContract.GetValueByName<string>("LotSplit_PopupKeyDM");
					var GridRowIdDM = Page.PortalContext.DataContract.GetValueByName<string>("LotSplit_SelectedRowIdDM");
					var ValueDM = Page.PortalContext.DataContract.GetValueByName("LotSplit_ReturnValueDM");
					var RevisionDM = Page.PortalContext.DataContract.GetValueByName("LotSplit_ReturnRevisionDM");

					if (!string.IsNullOrEmpty(GridRowIdDM))
					{
						if (ValueDM != null && KeyDM != null)
						{
							string sCellValue = ValueDM.ToString();
							if (RevisionDM != null)
								sCellValue = ValueDM + ":" + RevisionDM;

							string sCellToUpdate = "";
							switch (KeyDM)
							{
								case "MfgOrderAvailable":
									sCellToUpdate = "SplitToMfgOrder";
									break;
								case "ProductAvailable":
									sCellToUpdate = "SplitToProduct";
									break;
							}
							(_gridDetails.GridContext as ItemDataContext).SetCell(GridRowIdDM, sCellToUpdate, ValueDM);

						}
					}

					// clear the data contracts as the mess with the data loading of grid values
					Page.PortalContext.DataContract.SetValueByName("LotSplit_PopupKeyDM", null);
					Page.PortalContext.DataContract.SetValueByName("LotSplit_SelectedRowIdDM", null);
					Page.PortalContext.DataContract.SetValueByName("LotSplit_ReturnValueDM", null);
					Page.PortalContext.DataContract.SetValueByName("LotSplit_ReturnRevisionDM", null);
				}
			}
			else
			{
				string sContainer = Page.DataContract.GetValueByName("SelectedContainerNameDM") is ContainerRef ? (Page.DataContract.GetValueByName("SelectedContainerNameDM") as ContainerRef).Name : Page.DataContract.GetValueByName("SelectedContainerNameDM") as string;

                if (!string.IsNullOrEmpty(sContainer))
				{
					_txtSelectionId.TextControl.Text = sContainer;
					_txtSelectionId_DataChanged(null, null);
				}

                //get containers from Container Search screen
                if (Page.Session["selectedContainers"] != null)
                {
                    string[] sContainersList = Page.Session["selectedContainers"] as string[];
                    foreach (string container in sContainersList)
                    {
                        _txtSelectionId.TextControl.Text = container;
                        _txtSelectionId_DataChanged(null, null);
                    }
                }
			}
        } // OnLoad

        //---------------------------------------------------
        //
        //---------------------------------------------------
        public override void WebPartCustomAction(object sender, CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as Camstar.WebPortal.Personalization.CustomAction;

            if (action != null && action.Parameters == "Reset")
            {
                CustomReset();
            }
            else if (action != null && action.Parameters == "Submit")
            {
                e.Result = CustomSubmit();
				if (e.Result.IsSuccess)
				{
					CustomReset();
                    // COMMENT THE LINE IMPLEMENTATION FOR BUG 130061
					//Page.CloseFloatingFrameOnSubmit(e.Result);
				}
            }
            else if (action != null && action.Parameters == "RowCopy")
            {
                WafersGrid_RowCopy();
            }
        } // WebPartCustomAction 


        //---------------------------------------------------
        //
        //---------------------------------------------------
        public void WafersGrid_RowCopy()
        {
              // Here is action on click
            var parts = Page.EventArgument.Split(':');
            var rowIndex = parts[3];

            // check the row of buttob clicked is selected
            //string strRowId = (_gridWafers.GridContext as ItemDataContext).SelectedRowIDs != null ? (_gridWafers.GridContext as ItemDataContext).SelectedRowIDs.FirstOrDefault(p => p.ToString() == rowIndex.ToString()) : null;
            if (!string.IsNullOrEmpty(rowIndex.ToString()))
            {
                SS_LotSplit_Wafer[] oGridWafers = (_gridWafers.GridContext as BoundContext).Data as SS_LotSplit_Wafer[];
                List<SS_LotSplit_Wafer> oNewWafersList = new List<SS_LotSplit_Wafer>();
                int iRowIndex = 0;
                if (oGridWafers != null)
                {
                    foreach (SS_LotSplit_Wafer oWafer in oGridWafers)
                    {
                        SS_LotSplit_Wafer oWaferClone = new SS_LotSplit_Wafer();
                        oWaferClone = oWafer;
                        oNewWafersList.Add(oWaferClone);

                        if (iRowIndex == int.Parse(rowIndex))
                        {
                            SS_LotSplit_Wafer oWaferClone2 = new SS_LotSplit_Wafer();
                            oWaferClone2.FromContainer = oWafer.FromContainer;
                            oWaferClone2.WaferScribeNumber = oWafer.WaferScribeNumber.ToString();
                            oWaferClone2.ToWaferScribeNumber = oWafer.ToWaferScribeNumber;
                            oWaferClone2.NDPW = oWafer.NDPW;
                            oWaferClone2.GoodQty = oWafer.GoodQty;
                            oWaferClone2.MaxNDPW = oWafer.MaxNDPW;
                            oWaferClone2.MaxGoodQty = oWafer.MaxGoodQty;
                            oWaferClone2.LotWaferItemId = oWafer.LotWaferItemId;
                            oWaferClone2.WaferNumber = oWafer.WaferNumber;
                            oWaferClone2.ContainerRowId = oWafer.ContainerRowId;            
                            oNewWafersList.Add(oWaferClone2);
                        }
                        iRowIndex++;
                    }

                    // bind to the grid
                    (_gridWafers.GridContext as BoundContext).Data = oNewWafersList.ToArray();
                    _gridWafers.BoundContext.LoadData();
                    CamstarWebControl.SetRenderToClient(_gridWafers);
                }
            }
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        private void CustomReset()
        {
            Page.ClearValues();
            _gridContainer.ClearData();
            _gridDetails.ClearData();
            _gridWafers.ClearData();
            _txtWaferSetCount.Data = "0";
            _toggleComments.Reset();

            _ndoEquipment.DropDownControl.Items.Clear();
            _ndoProcessType.DropDownControl.Items.Clear();            
            _ndoEquipment.DropDownControl.ClearSelection();
            _ndoProcessType.DropDownControl.ClearSelection();

            _txtSelectionId.Focus();

        } // CustomReset

        //---------------------------------------------------
        //
        //---------------------------------------------------
        private ResultStatus CustomSubmit()
        {
            // Prepare service
            var fs = FrameworkManagerUtil.GetFrameworkSession();
            ResultStatus oServiceResult = new ResultStatus(null, false);
            string sServiceType = "LotSplit";
            sServiceType = Page.PrimaryServiceType.ToString();

            // Run proper constructor. We need to be dynamic with the primary service type
            var svcType = WCFObject.CreateObjectType(sServiceType + "Service");
            // create a request object
            var oRequest = WCFObject.CreateObject(sServiceType + "_Request");
            var svcConstructor = svcType.GetConstructor(new Type[] { typeof(UserProfile) });
            var oService = svcConstructor.Invoke(new object[] { fs.CurrentUserProfile });

            // retriving data dynamically for Flexibility of the page.
            var oServiceData = CreateServiceData(sServiceType);

            if (_txtSelectionId.Data != null)
                (oServiceData as LotSplit).Container = new ContainerRef(_txtSelectionId.Data.ToString());
            
            if (_chkCreateNewSchedule.CheckControl.Checked)
                (oServiceData as LotSplit).CreateNewSchedule = true;
            else
                (oServiceData as LotSplit).CreateNewSchedule = false;

            if (_rdoFutureCombineSpec.Data != null)
                (oServiceData as LotSplit).FutureCombineSpec = _rdoFutureCombineSpec.Data as RevisionedObjectRef;

            if (_ndoEquipment.DropDownControl.SelectedValue != "")
                (oServiceData as LotSplit).Equipment = new NamedObjectRef(_ndoEquipment.DropDownControl.SelectedValue);

            if (_ndoProcessType.DropDownControl.SelectedValue != "")
                (oServiceData as LotSplit).ProcessType = new NamedObjectRef(_ndoProcessType.DropDownControl.SelectedValue);

            (oServiceData as LotSplit).ComputerName = _txtComputerName.Data != null ? _txtComputerName.Data.ToString() : null;
            (oServiceData as LotSplit).Employee = _ndoEmployee.Data != null ? new NamedObjectRef(_ndoEmployee.Data.ToString()) : null;
            (oServiceData as LotSplit).Comments = _txtComment.Data != null ? _txtComment.Data.ToString() : null;

            Hashtable htToContainers = new Hashtable();
            Hashtable htToContainerQty = new Hashtable();

            SplitLotDetails[] oDetails = (_gridDetails.GridContext as BoundContext).Data as SplitLotDetails[];
            if (oDetails != null)
            {                
                int iRowIndex = 0;
                foreach (SplitLotDetails oDetail in oDetails)
                {
                    if (oDetail.ToContainerName != null && !String.IsNullOrEmpty(oDetail.ToContainerName.ToString()))
                        htToContainers.Add(iRowIndex.ToString().PadLeft(6, '0'), oDetail.ToContainerName.ToString());
                    else
                        htToContainers.Add(iRowIndex.ToString().PadLeft(6, '0'), "");
                    
                    htToContainerQty.Add(iRowIndex.ToString().PadLeft(6, '0'), 0);
                    iRowIndex++;
                }
                            
                (oServiceData as LotSplit).Details = oDetails.ToArray();
            }

            if (Page.PrimaryServiceType == "LotSplitByWafers")
            {
                List<SplitLotWafers> oWafers = new List<SplitLotWafers>();

                if (_gridWafers.GridContext.GetSelectedCount() > 0)
                {
                    foreach (SS_LotSplit_Wafer oItem in (_gridWafers.GridContext as BoundContext).GetSelectedItems(false))
                    {
                        SplitLotWafers oWafer = new SplitLotWafers();
                        if (htToContainers != null)
                            // get the container name based on the rowId identifier
                            if (htToContainers.ContainsKey(oItem.ContainerRowId))
                            {
                                if (htToContainers[oItem.ContainerRowId].ToString() != "")
                                    oWafer.ToContainerName = htToContainers[oItem.ContainerRowId].ToString();
                                
                                oWafer.ListItemAction = ListItemAction.Add;
                                oWafer.LotWafersItem = new SubentityRef();
                                oWafer.LotWafersItem.ID = oItem.LotWaferItemId;
                                oWafer.WaferScribeNumber = oItem.WaferScribeNumber;
                                oWafer.ToWaferScribeNumber = oItem.ToWaferScribeNumber;
                                oWafer.NDPW = int.Parse(oItem.NDPW);
                                oWafer.GoodQty = int.Parse(oItem.GoodQty);
                                oWafer.WaferNumber = oItem.WaferNumber;
                                oWafer.scsToWaferSetID = oItem.ContainerRowId;
                                oWafers.Add(oWafer);

                                int iToContainerQty = 0;
                                if (oWafer.GoodQty != null)
                                    if (oWafer.GoodQty.ToString() != "" && int.Parse(oWafer.GoodQty.ToString()) > 0)
                                        iToContainerQty = int.Parse(oWafer.GoodQty.ToString());
                                    else
                                        iToContainerQty = int.Parse(oWafer.NDPW.ToString());

                                htToContainerQty[oItem.ContainerRowId] = int.Parse(htToContainerQty[oItem.ContainerRowId].ToString()) + iToContainerQty;
                            } //  if (htToContainers.ContainsKey(oItem.ContainerRowId))
                    } // foreach (SS_LotSplit_Wafer oItem in (_gridWafers.GridContext as BoundContext).GetSelectedItems(false))
                } // if (_gridWafers.GridContext.GetSelectedCount() > 0)

                if (oDetails != null)
                {
                    //List<int> lstDetailsToNullify = new List<int>();
                    int iRowIndex = 0;
                    foreach (SplitLotDetails oDetail in oDetails)
                    {
                        string sRowId = iRowIndex.ToString().PadLeft(6, '0');
                        //if (oDetail.ToContainerName == null || string.IsNullOrEmpty(oDetail.ToContainerName.ToString()))
                        //{                            
                        //    //lstDetailsToNullify.Add(iRowIndex);
                        //}

                        oDetail.scsToWaferSetID = sRowId;
                        oDetail.StandbyQty = int.Parse(htToContainerQty[sRowId].ToString());
                        iRowIndex++;
                    }

                    //foreach (int iNullify in lstDetailsToNullify)
                    //    (oServiceData as LotSplit).Details[iNullify] = null;

                    if (oWafers != null)
                        if (oWafers.Count > 0)
                            (oServiceData as LotSplit).Wafers = oWafers.ToArray();
                }
            } // if (Page.PrimaryServiceType == "LotSplitByWafers")

            // init the result object
            Result oResult = new Result();
            ResultStatus oResultStatus = new ResultStatus();
            oResultStatus = (oService as IShopFloorBase).ExecuteTransaction((oServiceData as DCObject));
            return oResultStatus;
            
        } // CustomSubmit

        //---------------------------------------------------
        //
        //---------------------------------------------------
        void _ndoProcessType_DataChanged(object sender, EventArgs e)
        {
            // clear the equipment selection dropdown
            _ndoEquipment.DropDownControl.Items.Clear();
            if (_ndoProcessType.DropDownControl.SelectedValue != "" && _txtSelectionId.Data != null)            
                FetchData("ProcessType");               
            
        } // _ndoProcessType_DataChanged

        //---------------------------------------------------
        //
        //---------------------------------------------------
        void _ndoEquipment_DataChanged(object sender, EventArgs e)
        {
            if (_ndoEquipment.DropDownControl.SelectedValue != "" && _txtSelectionId.Data != null)                
                FetchData("Equipment");           
        } // _ndoEquipment_DataChanged

        //---------------------------------------------------
        //
        //---------------------------------------------------
        void _txtSelectionId_DataChanged(object sender, EventArgs e)
        {
           if (_txtSelectionId.Data != null)
               if (_txtSelectionId.Data.ToString() != "")
                    FetchData("SelectionId");            
        } // _txtSelectionId_DataChanged

        //---------------------------------------------------
        //
        //---------------------------------------------------
        private void FetchData(string sEventType)
        {
            // Prepare service
            var fs = FrameworkManagerUtil.GetFrameworkSession();
            ResultStatus oServiceResult = new ResultStatus(null, false);
            string sServiceType = "LotSplit";
            sServiceType = Page.PrimaryServiceType.ToString();

            // Run proper constructor. We need to be dynamic with the primary service type
            var svcType = WCFObject.CreateObjectType(sServiceType + "Service");
            //create a request object
            var oRequest = WCFObject.CreateObject(sServiceType + "_Request");
            var svcConstructor = svcType.GetConstructor(new Type[] { typeof(UserProfile) });
            var oService = svcConstructor.Invoke(new object[] { fs.CurrentUserProfile });

            //retriving data dynamically for Flexibility of the page.
            var oServiceData = CreateServiceData(sServiceType);

            var info = CreateServiceInfo(sServiceType);
            var oServiceInfo = info as LotSplit_Info;
            (oRequest as Request).Info = oServiceInfo;

            bool bExecuteResolveSelectionId = false;
            switch (sEventType)
            {
                case "SelectionId":                    
                    bExecuteResolveSelectionId = true;
                    (oServiceData as LotSplit).SelectionId = _txtSelectionId.Data.ToString();
                    oServiceInfo.SelectionContainer = FieldInfoUtil.RequestValue();
                    oServiceInfo.ProcessTypeSelection =  FieldInfoUtil.RequestValue();
                    
                    if (sServiceType == "LotSplitByWafers")
                    {
                        oServiceInfo.LotWafers = new LotWafers_Info();
                        oServiceInfo.LotWafers.WaferScribeNumber = FieldInfoUtil.RequestValue();
                        oServiceInfo.LotWafers.NDPW = FieldInfoUtil.RequestValue();
                        oServiceInfo.LotWafers.GoodQty = FieldInfoUtil.RequestValue();
                        oServiceInfo.LotWafers.WaferNumber = FieldInfoUtil.RequestValue();
                    }
                    break;
                case "ProcessType":
                    (oServiceData as LotSplit).Container = new ContainerRef(_txtSelectionId.Data.ToString());
                    (oServiceData as LotSplit).ProcessType = new NamedObjectRef(_ndoProcessType.DropDownControl.SelectedValue);
                    oServiceInfo.EquipmentSelection = FieldInfoUtil.RequestValue();
                    break;

                case "Equipment":
                    (oServiceData as LotSplit).Container = new ContainerRef(_txtSelectionId.Data.ToString());
                    (oServiceData as LotSplit).ProcessType = new NamedObjectRef(_ndoProcessType.DropDownControl.SelectedValue);
                    (oServiceData as LotSplit).Equipment = new NamedObjectRef(_ndoEquipment.DropDownControl.SelectedValue);
                    break;
            }

            oServiceInfo.MaxStandbyQty = FieldInfoUtil.RequestValue();
            oServiceInfo.MaxQtyToProcess = FieldInfoUtil.RequestValue();
            oServiceInfo.MaxInProcessQty = FieldInfoUtil.RequestValue();
            oServiceInfo.MaxProcessedQty = FieldInfoUtil.RequestValue();

            oServiceInfo.FutureCombineSpec = FieldInfoUtil.RequestSelectionValue();
          
            // init the result object
            Result oResult = new Result();
            ResultStatus oResultStatus = new ResultStatus();

            // execute to request the value or resolveSelectionId
            if (bExecuteResolveSelectionId)
                oResultStatus = (oService as IShopFloorBase).ResolveSelectionId((oServiceData as DCObject), (oRequest as Request), out oResult);
            else
                oResultStatus = (oService as IShopFloorBase).GetEnvironment(oServiceData, (oRequest as Request), out oResult);

            if (oResultStatus.IsSuccess)
            {
                _txtMaxStandbyQty.Data = (oResult.Value as LotSplit).MaxStandbyQty.ToString();
                _txtMaxQtyToProcess.Data = (oResult.Value as LotSplit).MaxQtyToProcess.ToString();
                _txtMaxInProcessQty.Data = (oResult.Value as LotSplit).MaxInProcessQty.ToString();
                _txtMaxProcessedQty.Data = (oResult.Value as LotSplit).MaxProcessedQty.ToString();

                if (sEventType == "SelectionId")
                {
                    string sResolvedContainer = (oResult.Value as LotSplit).SelectionContainer.Name;
                    _txtSelectionId.Data = sResolvedContainer;

                    if ((oResult.Value as LotSplit).ProcessTypeSelection != null)
                    {
                        // add the processTypes to the ProcessType dropdown
                        foreach (NamedObjectRef oProcessType in (oResult.Value as LotSplit).ProcessTypeSelection)
                            _ndoProcessType.DropDownControl.Items.Add(oProcessType.Name);
                    }                    

                    if (sServiceType == "LotSplitByWafers")
                    {                        
                        ViewState[_kWafersViewStateKey] = (oResult.Value as LotSplit).LotWafers;
                    }

                    JQDataGrid _gridContainerTemp = Page.FindCamstarControl("LotSplit_ContainerGrid") as JQDataGrid;
                    SEMI.AppCode.UIUtility.GetLotQuerySelection(this, sServiceType, _txtSelectionId.Data.ToString(), true, ref _gridContainerTemp, _gridContainerTemp.ID.ToString(), true);
                    _gridDetails.ClearData();
                    _gridWafers.ClearData();

                    if ((oResult.Environment as LotSplit_Environment).FutureCombineSpec.SelectionValues != null)
                        _rdoFutureCombineSpec.SetSelectionValues((oResult.Environment as LotSplit_Environment).FutureCombineSpec.SelectionValues);

                    if (_ndoProcessType.DropDownControl.SelectedValue != "")
                        _ndoProcessType_DataChanged(null, null);
                }                
                else
                {
                    if (sEventType == "ProcessType")
                    {
                        if ((oResult.Value as LotSplit).EquipmentSelection != null)
                        {
                            // for each equipment, add to the equipment selection                        
                            foreach (NamedObjectRef oEquipment in (oResult.Value as LotSplit).EquipmentSelection)
                                _ndoEquipment.DropDownControl.Items.Add(oEquipment.Name);
                        }

                        // call the equipment data changed event 
                        if (_ndoEquipment.DropDownControl.SelectedValue != "")
                            _ndoEquipment_DataChanged(null, null);
                    }
                }
            }
            else
                DisplayMessage(oResultStatus);
        } // FetchData

        //---------------------------------------------------
        //
        //---------------------------------------------------
        public void UpdateWaferGrid()
        {
            if (Page.PrimaryServiceType == "LotSplitByWafers")
            {
                if (_txtSelectionId.Data != null)
                {
                    SplitLotDetails[] oDetails = (_gridDetails.GridContext as BoundContext).Data as SplitLotDetails[];
                    int iWaferSetCount = 0;

                    if (_txtContainerToDelete.Data != null)
                    {
                        string sContainerRowId = _txtContainerToDelete.Data.ToString();
                        RemoveWaferSet(sContainerRowId);
                        _txtContainerToDelete.ClearData();
                    }

                    //check if the number of rows match the number of wafer sets in the hashtable            
                    iWaferSetCount = int.Parse(_txtWaferSetCount.Data.ToString());
                    if (oDetails != null)
                    {
                        if (oDetails.Length > iWaferSetCount)
                            AddWaferSet((oDetails.Length - 1).ToString().PadLeft(6, '0'));
                    }
                }
            }
        } // UpdateWaferGrid

        //---------------------------------------------------
        //
        //---------------------------------------------------
        private void AddWaferSet(string sRowId)
        {
            LotWafers[] oWafers = ViewState[_kWafersViewStateKey] as LotWafers[];

            SS_LotSplit_Wafer[] oGridWafers = (_gridWafers.GridContext as BoundContext).Data as SS_LotSplit_Wafer[];
            List<SS_LotSplit_Wafer> oNewWafersList = new List<SS_LotSplit_Wafer>();

            if (oGridWafers != null)
            {
                foreach (SS_LotSplit_Wafer oWafer in oGridWafers)
                {
                    SS_LotSplit_Wafer oWaferClone = new SS_LotSplit_Wafer();
                    oWaferClone = oWafer;
                    oNewWafersList.Add(oWaferClone);
                }
            }

            if (oWafers != null)
            {
                foreach (LotWafers oLotWafer in oWafers)
                {
                    SS_LotSplit_Wafer oNewWafer = new SS_LotSplit_Wafer();
                    oNewWafer.FromContainer = _txtSelectionId.Data.ToString();
                    oNewWafer.WaferScribeNumber = oLotWafer.WaferScribeNumber.ToString();
                    oNewWafer.ToWaferScribeNumber = "";
                    oNewWafer.NDPW = oLotWafer.NDPW != null ? oLotWafer.NDPW.ToString() : "";
                    oNewWafer.GoodQty = oLotWafer.GoodQty != null ? oLotWafer.GoodQty.ToString() : "";
                    oNewWafer.MaxNDPW = oLotWafer.NDPW != null ? oLotWafer.NDPW.ToString() : "";
                    oNewWafer.MaxGoodQty = oLotWafer.GoodQty != null ? oLotWafer.GoodQty.ToString() : "";
                    oNewWafer.LotWaferItemId = oLotWafer.Self.ID;
                    oNewWafer.WaferNumber = oLotWafer.WaferNumber.ToString();
                    oNewWafer.ContainerRowId = sRowId;
                    oNewWafersList.Add(oNewWafer);
                }

                // update the waferset count
                int iWaferSetCount = 0;
                iWaferSetCount = int.Parse(_txtWaferSetCount.Data.ToString());
                iWaferSetCount++;
                _txtWaferSetCount.Data = iWaferSetCount.ToString();
            }
            // bind to the grid
            (_gridWafers.GridContext as BoundContext).Data = oNewWafersList.ToArray();
            _gridWafers.BoundContext.LoadData();
            CamstarWebControl.SetRenderToClient(_gridWafers);            
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        private void RemoveWaferSet(string sRowId)
        {
            SS_LotSplit_Wafer[] oGridWafers = (_gridWafers.GridContext as BoundContext).Data as SS_LotSplit_Wafer[];
            List<SS_LotSplit_Wafer> oNewWafersList = new List<SS_LotSplit_Wafer>();

            if (oGridWafers != null)
            {
                foreach (SS_LotSplit_Wafer oWafer in oGridWafers)
                {
                    SS_LotSplit_Wafer oWaferClone = new SS_LotSplit_Wafer();

                    if (int.Parse(oWafer.ContainerRowId) < int.Parse(sRowId))
                    {
                        oWaferClone = oWafer;
                        oNewWafersList.Add(oWaferClone);
                    }
                    else
                    {
                        if (int.Parse(oWafer.ContainerRowId) > int.Parse(sRowId))
                        {
                            oWaferClone = oWafer;
                            oWaferClone.ContainerRowId = (int.Parse(oWafer.ContainerRowId) - 1).ToString().PadLeft(6, '0');
                            oNewWafersList.Add(oWaferClone);
                        }
                    }
                }

                // update the waferset count
                int iWaferSetCount = 0;
                iWaferSetCount = int.Parse(_txtWaferSetCount.Data.ToString());
                iWaferSetCount--;
                _txtWaferSetCount.Data = iWaferSetCount.ToString();

                // bind to the grid
                (_gridWafers.GridContext as BoundContext).Data = oNewWafersList.ToArray();
                _gridWafers.BoundContext.LoadData();
                CamstarWebControl.SetRenderToClient(_gridWafers);
            }
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        private class SS_LotSplit_Wafer
        {
            private string sWaferScribeNumber;
            private string sToWaferScribeNumber;
            private string sNDPW;
            private string sGoodQty;
            private string sMaxNDPW;
            private string sMaxGoodQty;
            private string sFromContainer;
            private string sLotWaferItemId;
            private string sContainerRowId;
            private string sWaferNumber;
            private bool bIsCopy;

            // constructor
            public SS_LotSplit_Wafer()
            {
                bIsCopy = false;
            }

            public string WaferScribeNumber
            {
                get { return sWaferScribeNumber; }
                set { sWaferScribeNumber = value; }
            }

            public string ToWaferScribeNumber
            {
                get { return sToWaferScribeNumber; }
                set { sToWaferScribeNumber = value; }
            }

            public string NDPW
            {
                get { return sNDPW; }
                set { sNDPW = value; }
            }

            public string GoodQty
            {
                get { return sGoodQty; }
                set { sGoodQty = value; }
            }

            public string MaxNDPW
            {
                get { return sMaxNDPW; }
                set { sMaxNDPW = value; }
            }

            public string MaxGoodQty
            {
                get { return sMaxGoodQty; }
                set { sMaxGoodQty = value; }
            }

            public string FromContainer
            {
                get { return sFromContainer; }
                set { sFromContainer = value; }
            }

            public string LotWaferItemId
            {
                get { return sLotWaferItemId; }
                set { sLotWaferItemId = value; }
            }

            public string WaferNumber
            {
                get { return sWaferNumber; }
                set { sWaferNumber = value; }
            }

            public bool IsCopy
            {
                get { return bIsCopy; }
                set { bIsCopy = value; }
            }

            public string ContainerRowId
            {
                get { return sContainerRowId; }
                set { sContainerRowId = value; }
            }
        } // SS_LotCombine_LotItem
    }
}



