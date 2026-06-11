/* Copyright 2019 Siemens */
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
using SEMI.AppCode;

/// <summary>
/// Summary description for SS_LotReceiving
/// </summary>
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
	public class SS_LotReceiving : scsShopfloorBase
	{
		#region Properties

		protected CWC.TextBox _txtSelectionId { get { return Page.FindCamstarControl("LotReceiving_SelectionIdField") as CWC.TextBox; } }
		protected JQDataGrid _gridContainers { get { return Page.FindCamstarControl("LotGrid") as JQDataGrid; } }
        protected JQDataGrid _gridSvcAttributes { get { return Page.FindCamstarControl("LotReceiving_ServiceAttrsDetails") as JQDataGrid; } }
        protected JQDataGrid _gridValidValues { get { return Page.FindCamstarControl("ValidValuesGrid") as JQDataGrid; } }
		protected CWC.TextBox txtComputerName { get { return Page.FindCamstarControl("LotReceiving_ComputerNameField") as CWC.TextBox; } }
		private SEMI.AppCode.DataEnvelopControl _envSelectedLots { get { return Page.FindCamstarControl("LotReceiving_SelectedLotsList") as SEMI.AppCode.DataEnvelopControl; } }
		private CWC.NamedObject noLotReceiving { get { return Page.FindCamstarControl("LotReceiving_ReceivingLocation") as CWC.NamedObject; } }
        private CWC.RevisionedObject _rdoToWorkflow { get { return Page.FindCamstarControl("LotReceiving_ToWorkflow") as CWC.RevisionedObject; } }
        private CWC.NamedSubentity _subToStep { get { return Page.FindCamstarControl("LotReceiving_ToStep") as CWC.NamedSubentity; } }
		public CWC.ContainerList _ContainerTemp { get { return Page.FindCamstarControl("LotReceiving_ContainerTemp") as CWC.ContainerList; } }
        ContainerRef containerRef;

		#endregion

		#region DataChangeEvents
		
		/// <summary>
        /// This is an event that is fired when data is changed in the Selection Id field.
		/// </summary>
		public void SelectionIdField_DataChanged()
		{
			try
			{
				if (_txtSelectionId.Data != null)
				{
                    Page.StatusBar.ClearMessage();
                    FetchData("SelectionId");
					CamstarWebControl.SetRenderToClient(_gridContainers);
				}
			}
			catch (Exception Ex)
			{
				DisplayMessage(new OM.ResultStatus(Ex.TargetSite.Name + "(): " + Ex.Message, false));
			}
			finally
			{
				_txtSelectionId.ClearData();
				_txtSelectionId.Focus();
			}
		}

        /// <summary>
        /// This is an event that is fired when data is changed in the Receiving Location field.
        /// </summary>
        public void ReceivingLocationField_DataChanged()
        {
            try
            {
                _gridSvcAttributes.ClearData();
                if (noLotReceiving.Data != null)
                    FetchData("ReceivingLocation");
            }
            catch (Exception Ex)
            {
                DisplayMessage(new OM.ResultStatus(Ex.TargetSite.Name + "(): " + Ex.Message, false));
            }
        }

        /// <summary>
        /// This is an event that is fired when data is changed in the To Workflow field.
        /// </summary>
        public void ToWorkflowField_DataChanged()
        {
            try
            {
                _subToStep.ClearData();
                _gridSvcAttributes.ClearData();
                _rdoToWorkflow.LoadOrClearDependentValues();
            }
            catch (Exception Ex)
            {
                DisplayMessage(new OM.ResultStatus(Ex.TargetSite.Name + "(): " + Ex.Message, false));
            }
        }

        /// <summary>
        /// This is an event that is fired when data is changed in the To Step field.
        /// </summary>
        public void ToStepField_DataChanged()
        {
            try
            {
                _gridSvcAttributes.ClearData();
                if (_subToStep.Data != null)
                    FetchData("ToStep");
            }
            catch (Exception Ex)
            {
                DisplayMessage(new OM.ResultStatus(Ex.TargetSite.Name + "(): " + Ex.Message, false));
            }
        }
		
		#endregion

		#region Private Methods
		
		/// <summary>
		/// Set data for the lot grid
		/// </summary>
		/// <param name="sContainersItem">Container to insert</param>
		/// <param name="_gridContainersTemp">grid host</param>
		private void SetGridData(string sContainersItem, JQDataGrid _gridContainersTemp)
		{
			// check if lot not exist in the grid else add to the grid
			string strSelectedGridId = (_gridContainersTemp.GridContext as BoundContext).GetRowIdByCellValue("Lot", sContainersItem);

			if (string.IsNullOrEmpty(strSelectedGridId))
			{
				//setting them to the grid
				SEMI.AppCode.UIUtility.GetLotQuerySelection(this, this.PrimaryServiceType.ToString(), sContainersItem, false, ref _gridContainersTemp, "LotGrid", true);//_gridContainersTemp.LabelName for lotGrid string               
			}
		}

        /// <summary>
        /// Fetch data based on the event name
        /// </summary>
        /// <param name="lot">string event name</param>
        private void FetchData(string EventName)
        {
            bool bIsFirstLot = (_gridContainers.TotalRowCount == 0);

            // Prepare service
            var fs = FrameworkManagerUtil.GetFrameworkSession();
            ResultStatus oServiceResult = new ResultStatus(null, false);
            string sServiceType = "LotReceiving";
            if (Page.PrimaryServiceType != null)
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
            var oServiceInfo = info as LotReceiving_Info;
            (oRequest as Request).Info = oServiceInfo;
            
            if (EventName == "SelectionId")
            {
                (oServiceData as LotReceiving).SelectionId = _txtSelectionId.Data.ToString();
                oServiceInfo.Containers = FieldInfoUtil.RequestValue();
                if (bIsFirstLot)
                {
                    oServiceInfo.ToWorkflow = FieldInfoUtil.RequestValue();
                    oServiceInfo.SelectionContainer = FieldInfoUtil.RequestValue();
                }
            }
            else if ((EventName == "ReceivingLocation") || (EventName == "ToStep"))
            {
                (oServiceData as LotReceiving).Container = new ContainerRef(_gridContainers.GridContext.GetCell(0, "Lot").ToString());
                if (EventName == "ReceivingLocation")
                {
                    (oServiceData as LotReceiving).ReceivingLocation = new NamedObjectRef(noLotReceiving.Data.ToString());
                }
                else
                {
                    (oServiceData as LotReceiving).ToWorkflow = _rdoToWorkflow.Data as RevisionedObjectRef;
                    (oServiceData as LotReceiving).ToStep = new NamedSubentityRef(_subToStep.Data.ToString());
                }
                oServiceInfo.ServiceAttrsDetailsSelection = new ServiceAttrsDetails_Info();
                oServiceInfo.ServiceAttrsDetailsSelection.Attribute = FieldInfoUtil.RequestValue();
                oServiceInfo.ServiceAttrsDetailsSelection.AttributeName = FieldInfoUtil.RequestValue();
                oServiceInfo.ServiceAttrsDetailsSelection.AlternateName1 = FieldInfoUtil.RequestValue();
                oServiceInfo.ServiceAttrsDetailsSelection.AlternateName2 = FieldInfoUtil.RequestValue();
                oServiceInfo.ServiceAttrsDetailsSelection.AttributeValue = FieldInfoUtil.RequestValue();
                oServiceInfo.ServiceAttrsDetailsSelection.AttributeRevision = FieldInfoUtil.RequestValue();
                oServiceInfo.ServiceAttrsDetailsSelection.AccessLevel = FieldInfoUtil.RequestValue();
                oServiceInfo.ServiceAttrsDetailsSelection.FieldType = FieldInfoUtil.RequestValue();
                oServiceInfo.ServiceAttrsDetailsSelection.IsRequired = FieldInfoUtil.RequestValue();
                oServiceInfo.ServiceAttrsDetailsSelection.ServiceAttrsSetupName = FieldInfoUtil.RequestValue();
                oServiceInfo.ServiceAttrsDetailsSelection.ObjectTypeName = FieldInfoUtil.RequestValue();
                oServiceInfo.ServiceAttrsDetailsSelection.ValidValues = new AttributeValidValuesChanges_Info();
                oServiceInfo.ServiceAttrsDetailsSelection.ValidValues.AttributeValue = FieldInfoUtil.RequestValue();
                oServiceInfo.ServiceAttrsDetailsSelection.ValidValues.AttributeRevision = FieldInfoUtil.RequestValue();
            }
            // init the result object
            Result oResult = new Result();
            ResultStatus oResultStatus = new ResultStatus();

            if (EventName == "SelectionId")
                oResultStatus = (oService as IShopFloorBase).ResolveSelectionId((oServiceData as DCObject), (oRequest as Request), out oResult);
            else
                oResultStatus = (oService as IShopFloorBase).GetEnvironment(oServiceData, (oRequest as Request), out oResult);

            if (oResultStatus.IsSuccess)
            {
                //Display the data
                if (EventName == "SelectionId")
                {
                    SetGridData(_txtSelectionId.Data.ToString(), _gridContainers);
                    if (bIsFirstLot)
                    {
                        if ((oResult.Value as LotReceiving).ToWorkflow != null)
                        {
                            _rdoToWorkflow.Data = (oResult.Value as LotReceiving).ToWorkflow;
                            ToWorkflowField_DataChanged();
                        }

                        //Fetch receiving location
                        (oServiceData as LotReceiving).Container = (oResult.Value as LotReceiving).SelectionContainer;
                        _ContainerTemp.Data = (oResult.Value as LotReceiving).SelectionContainer;
                        CamstarWebControl.SetRenderToClient(_ContainerTemp);
                        oServiceInfo.ReceivingLocation = FieldInfoUtil.RequestSelectionValue();
                        // init the result object
                        Result oReceivingLocationResult = new Result();
                        ResultStatus oReceivingLocationResultStatus = new ResultStatus();

                        oReceivingLocationResultStatus = (oService as IShopFloorBase).GetEnvironment(oServiceData, (oRequest as Request), out oReceivingLocationResult);

                        if (oReceivingLocationResultStatus.IsSuccess)
                        {
                            CWC.NamedObject _ndoTempReceivingLocation = noLotReceiving;
                            if ((oReceivingLocationResult.Value as LotReceiving).ReceivingLocation != null)
                            {
                                noLotReceiving.Data = (oReceivingLocationResult.Value as LotReceiving).ReceivingLocation;
                                ReceivingLocationField_DataChanged();
                            }
                        }
                        else
                        {
                            DisplayMessage(oReceivingLocationResultStatus);
                        }
                    }
                }
                else if (EventName == "ReceivingLocation" || EventName == "ToStep")
                {
                    if ((oResult.Value as LotReceiving).ServiceAttrsDetailsSelection != null)
                    {
                        //Bind result to the grid
                        _gridSvcAttributes.Data = (oResult.Value as LotReceiving).ServiceAttrsDetailsSelection.ToArray();
                        _gridSvcAttributes.OriginalData = (oResult.Value as LotReceiving).ServiceAttrsDetailsSelection.ToArray();

                        //Create Valid Values table
                        DataTable validValuesDT = new DataTable();
                        int countSvcAttr = (oResult.Value as LotReceiving).ServiceAttrsDetailsSelection.Count();
                        validValuesDT.Columns.Add("Attribute", typeof(String));
                        validValuesDT.Columns.Add("AttributeValue", typeof(String));
                        validValuesDT.Columns.Add("AttributeRevision", typeof(String));
                        for (int i = 0; i < countSvcAttr; i++)
                        {
                            if (((oResult.Value as LotReceiving).ServiceAttrsDetailsSelection.GetValue(i) as ServiceAttrsDetails).ValidValues != null)
                            {
                                int countValidValues = ((oResult.Value as LotReceiving).ServiceAttrsDetailsSelection.GetValue(i) as ServiceAttrsDetails).ValidValues.Count();
                                for (int x = 0; x < countValidValues; x++)
                                {
                                    DataRow dtRow = validValuesDT.NewRow();
                                    dtRow.SetField("Attribute", ((oResult.Value as LotReceiving).ServiceAttrsDetailsSelection.GetValue(i) as ServiceAttrsDetails).Attribute.Name);
                                    dtRow.SetField("AttributeValue", (((oResult.Value as LotReceiving).ServiceAttrsDetailsSelection.GetValue(i) as ServiceAttrsDetails).ValidValues.GetValue(x) as AttributeValidValuesChanges).AttributeValue);
                                    dtRow.SetField("AttributeRevision", (((oResult.Value as LotReceiving).ServiceAttrsDetailsSelection.GetValue(i) as ServiceAttrsDetails).ValidValues.GetValue(x) as AttributeValidValuesChanges).AttributeRevision);
                                    validValuesDT.Rows.Add(dtRow);
                                }
                            }
                        }
                        _gridValidValues.ClearData();
                        _gridValidValues.Data = validValuesDT;
                        _gridValidValues.OriginalData = validValuesDT;
                    }
                }
            }
            else
            {
                DisplayMessage(oResultStatus);
            }
        }

		#endregion

		#region Page Events
		
		/// <summary>
		/// This event is fired when the submit action button is clicked.
		/// </summary>
		/// <param name="serviceData"></param>
		public override void GetInputData(Service serviceData)
		{
			base.GetInputData(serviceData);
            //Containers
            if (_gridContainers.Data != null)
            {
                (serviceData as LotReceiving).Containers = new ContainerRef[_gridContainers.TotalRowCount];
                for (int i = 0; i < _gridContainers.TotalRowCount; i++)
                {
                    (serviceData as LotReceiving).Containers[i] = new ContainerRef();
                    (serviceData as LotReceiving).Containers[i].Name = _gridContainers.GridContext.GetCell(i.ToString().PadLeft(6, '0'), "Lot").ToString();
                }
            }

            //ServiceAttributes
            if (_gridSvcAttributes.Data != null)
            {
                ServiceAttrsDetails[] getServiceAttrsDetails = _gridSvcAttributes.Data as ServiceAttrsDetails[];
                if (serviceData is LotReceiving)
                {
                    (serviceData as LotReceiving).ServiceAttrsDetails = new ServiceAttrsDetails[getServiceAttrsDetails.Count()];
                    for (int i = 0; i < getServiceAttrsDetails.Count(); i++)
                    {
                        (serviceData as LotReceiving).ServiceAttrsDetails[i] = new ServiceAttrsDetails();
                        (serviceData as LotReceiving).ServiceAttrsDetails[i].Attribute = new NamedObjectRef();
                        (serviceData as LotReceiving).ServiceAttrsDetails[i].Attribute.Name = getServiceAttrsDetails[i].Attribute.Name;
                        (serviceData as LotReceiving).ServiceAttrsDetails[i].FieldType = getServiceAttrsDetails[i].FieldType;
                        (serviceData as LotReceiving).ServiceAttrsDetails[i].ServiceAttrsSetupName = getServiceAttrsDetails[i].ServiceAttrsSetupName;
                        (serviceData as LotReceiving).ServiceAttrsDetails[i].AttributeValue = getServiceAttrsDetails[i].AttributeValue;
                        (serviceData as LotReceiving).ServiceAttrsDetails[i].AttributeRevision = getServiceAttrsDetails[i].AttributeRevision;
                    }
                }
            }
		}
		
		/// <summary>
		/// On page load
		/// </summary>
		/// <param name="e"></param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			//_txtSelectionId.DataChanged += new EventHandler(_txtSelectionId_DataChanged);
			txtComputerName.TextControl.Text = SEMI.AppCode.UIUtility.GetComputerName(this);

            if (Page.IsPostBack)
            {
                // Check if it is a pop up close, get the return result
                if (SEMI.AppCode.UIUtility.IsPopupClose(this))
                {
                    if (Page.PortalContext.DataContract.GetValueByName<string>("LotReceiving_ReturnedValue") != null)
                    {
                        var attrVal = Page.PortalContext.DataContract.GetValueByName<string>("LotReceiving_ReturnedValue");
                        var attrRev = Page.PortalContext.DataContract.GetValueByName<string>("LotReceiving_ReturnedRevision");
                        string selectedAttr = Page.PortalContext.DataContract.GetValueByName("LotReceiving_SelectedAttribute").ToString();
                        if (!string.IsNullOrEmpty(selectedAttr))
                        {
                            ServiceAttrsDetails[] getServiceAttrsDetails = _gridSvcAttributes.Data as ServiceAttrsDetails[];
                            for (int i = 0; i < getServiceAttrsDetails.Count(); i++)
                            {
                                if (getServiceAttrsDetails[i].Attribute.Name == selectedAttr)
                                {
                                    getServiceAttrsDetails[i].AttributeValue = attrVal;
                                    getServiceAttrsDetails[i].AttributeRevision = attrRev;
                                }
                            }
                            _gridSvcAttributes.ClearData();
                            _gridSvcAttributes.Data = getServiceAttrsDetails;
                            _gridSvcAttributes.OriginalData = getServiceAttrsDetails;
                        }
                    }

                    if (_envSelectedLots != null)
                    {
                        // manually initialize the containers list data contract since it is not triggered when we do a manual popup close
                        if (Page.DataContract.GetValueByName("LotReceiving_SelectedLotsListDM") != null)
                        {
                            //gets the list of returned lot from the popup form
                            _envSelectedLots.SS_ContainersList = Page.DataContract.GetValueByName("LotReceiving_SelectedLotsListDM") as string[];
                        }
                    }
                    if (_envSelectedLots.SS_ContainersList != null)
                    {
                        string[] sContainers;
                        sContainers = _envSelectedLots.SS_ContainersList;

                        JQDataGrid _gridContainersTemp = Page.FindCamstarControl("LotGrid") as JQDataGrid;
                        foreach (string sContainersItem in sContainers)
                        {
                            _txtSelectionId.Data = sContainersItem;
                            SelectionIdField_DataChanged();
                        }
                        CamstarWebControl.SetRenderToClient(_gridContainersTemp);
                        //nullify the containers list
                        _envSelectedLots.SS_ContainersList = null;
                    }
                }
            }
		}

		/// <summary>
		/// Resets the page value to default
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
		{
			var action = e.Action as Camstar.WebPortal.Personalization.CustomAction;

			if (action != null && action.Parameters == "Reset")
			{
                Page.ClearValues();
                Page.ShopfloorReset(sender, e);
                _gridContainers.ClearData();
                _gridSvcAttributes.ClearData();
                _gridValidValues.ClearData();
                _txtSelectionId.Focus();
			}
			base.WebPartCustomAction(sender, e);
		}

		/// <summary>
		/// Excutes after the submit button is clicked
		/// </summary>
		/// <param name="status"></param>
		/// <param name="serviceData"></param>
		public override void PostExecute(OM.ResultStatus status, OM.Service serviceData)
		{
			base.PostExecute(status, serviceData);
			if (status.IsSuccess)
			{
				Page.ClearValues();
				_gridContainers.ClearData();
                _gridSvcAttributes.ClearData();
                _gridValidValues.ClearData();
				_txtSelectionId.Focus();
			}
		}
		#endregion
	}
}



