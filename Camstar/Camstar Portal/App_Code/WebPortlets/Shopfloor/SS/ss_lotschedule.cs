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
using SEMI.AppCode;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.PortalFramework;



/// <summary>
/// Summary description 
/// </summary>
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
	public class SS_LotSchedule : scsShopfloorBase
	{
		#region Properties

		protected CWC.TextBox txtComputerName { get { return Page.FindCamstarControl("LotSchedule_ComputerNameField") as CWC.TextBox; } }
		//Buttons
		CWC.Button _btnLotSchedule_MaterialPartButton { get { return Page.FindCamstarControl("LotSchedule_MaterialPartButton") as CWC.Button; } }
		CWC.Button _btnLotSchedule_ProductButton { get { return Page.FindCamstarControl("LotSchedule_ProductButton") as CWC.Button; } }
		CWC.Button _btnHiddenUpdateFields { get { return Page.FindCamstarControl("btnHiddenUpdateFields") as CWC.Button; } }
		//Controls
		CWC.NamedObject _MfgOrder { get { return Page.FindCamstarControl("LotSchedule_MfgOrder") as CWC.NamedObject; } }
        CWC.NamedObject _Owner { get { return Page.FindCamstarControl("LotSchedule_Owner") as CWC.NamedObject; } }

		CWC.RevisionedObject _Product { get { return Page.FindCamstarControl("LotSchedule_Product") as CWC.RevisionedObject; } }
		CWC.RevisionedObject _ProcessSpec { get { return Page.FindCamstarControl("LotSchedule_ProcessSpec") as CWC.RevisionedObject; } }
		CWC.RevisionedObject _ProductBOM { get { return Page.FindCamstarControl("LotSchedule_ProductBOM") as CWC.RevisionedObject; } }
		CWC.RevisionedObject _SupplementarySpec { get { return Page.FindCamstarControl("LotSchedule_SupplementarySpec") as CWC.RevisionedObject; } }
		CWC.NamedObject _Priority { get { return Page.FindCamstarControl("LotSchedule_Priority") as CWC.NamedObject; } }
		CWC.NamedObject _SSPlanRef { get { return Page.FindCamstarControl("LotSchedule_SSPlanRef") as CWC.NamedObject; } }

		CWC.NamedSubentity _FirstWIPStep { get { return Page.FindCamstarControl("LotSchedule_FirstWIPStep") as CWC.NamedSubentity; } }

		CWC.DateChooser _ExpectedStartDate { get { return Page.FindCamstarControl("LotSchedule_ExpectedStartDate") as CWC.DateChooser; } }
		CWC.DateChooser _ExpectedEndDate { get { return Page.FindCamstarControl("LotSchedule_ExpectedEndDate") as CWC.DateChooser; } }
		CWC.TextBox _CycleTime { get { return Page.FindCamstarControl("LotSchedule_CycleTime") as CWC.TextBox; } }
		CWC.TextBox _SelectionIdField { get { return Page.FindCamstarControl("LotSchedule_SelectionIdField") as CWC.TextBox; } }
		CWC.TextBox _ScheduleQty { get { return Page.FindCamstarControl("LotSchedule_ScheduleQty") as CWC.TextBox; } }
        CWC.TextBox _Comments { get { return Page.FindCamstarControl("LotSchedule_Comments") as CWC.TextBox; } }
        CWC.TextBox _txtSSPopupKey { get { return Page.FindCamstarControl("SSPopupKey") as CWC.TextBox; } }
        CWC.TextBox _txtSelectedRowId { get { return Page.FindCamstarControl("SelectedRowId") as CWC.TextBox; } }
        CWC.TextBox _txtToFromPopupKey { get { return Page.FindCamstarControl("ToFromPopupKey") as CWC.TextBox; } }
		CWC.TextBox _txtNewLotID { get { return Page.FindCamstarControl("LotSchedule_NewLotId") as CWC.TextBox; } }
		CWC.CheckBox _chkAutoSetNewLotID { get { return Page.FindCamstarControl("LotSchedule_AutoSetNewLotId") as CWC.CheckBox; } }
        
		SEMI.AppCode.DataEnvelopControl _envSelectedLots { get { return Page.FindCamstarControl("LotSchedule_SelectedLotsList") as SEMI.AppCode.DataEnvelopControl; } }
		DropDownList _ListContainers { get { return Page.FindCamstarControl("LotSchedule_Container") as DropDownList; } }
		DropDownList _ListPartialLot { get { return Page.FindCamstarControl("LotSchedule_PartialLot") as DropDownList; } }
		JQDataGrid HandlersGrid { get { return Page.FindCamstarControl("LotSchedule_Details") as JQDataGrid; } }
        JQDataGrid _gridSSDetails { get { return Page.FindCamstarControl("LotSchedule_SSDetails") as JQDataGrid; } }
        JQDataGrid _gridSpecsSelection { get { return Page.FindCamstarControl("LotSchedule_SpecsSelection") as JQDataGrid; } }
		JQDataGrid _gridSSByStepDetails { get { return Page.FindCamstarControl("LotSchedule_SSByStepDetails") as JQDataGrid; } }
		RecordSet rsDetailGrid;
		WorkflowNavigator _FirstWIPStepWorkflow { get { return Page.FindCamstarControl("FirstWIPStepWfNavigator") as WorkflowNavigator; } }
		ToggleContainer _togglecontainerControl { get { return Page.FindCamstarControl("Control") as ToggleContainer; } }

        string  sAttrValOrg;
        string sAttrValRevOrg;
        string sAttrValMfgOrderOrg;

		#endregion

		#region DataChangeEvents

		/// <summary>
		/// This is an event that is fire when data is change in the SelectionId field is changed.
		/// </summary>
		public void SelectionIdField_DataChanged()
		{
			//Get Details grid for comparing data
			JQDataGrid DetailsGrid = Page.FindCamstarControl("LotSchedule_Details") as JQDataGrid;
			if (_SelectionIdField.Data != null && IsUniqueSingle(DetailsGrid, _SelectionIdField.TextControl.Text))
			{
				DetailsGridDataChangeEvent(_SelectionIdField.TextControl.Text);
				_SelectionIdField.ClearData();
			}
		}

        /// <summary>
        /// Set data for the Owner,priorty,Comments and schedule data fields.
        /// </summary>
        /// <param name="lot">Target lot</param>
        private void FetchLotInfo(string lot)
        {
            try
            {
                // get the session and user profile
                var fs = FrameworkManagerUtil.GetFrameworkSession();

                // Get the service type of the page
                string sServiceType = Page.PrimaryServiceType;

                // Run proper constructor. We need to be dynamic with the primary service type
                var svcType = WCFObject.CreateObjectType(sServiceType + "Service");
                //create a request object
                var oRequest = WCFObject.CreateObject(sServiceType + "_Request");
                var svcConstructor = svcType.GetConstructor(new Type[] { typeof(UserProfile) });
                var oService = svcConstructor.Invoke(new object[] { fs.CurrentUserProfile });

                //retriving data dynamically for Flexibility of the page.
                var data = CreateServiceData(sServiceType);
                var oServiceData = data as LotSchedule;

                var info = CreateServiceInfo(sServiceType);
                var oServiceInfo = info as LotSchedule_Info;
                (oRequest as Request).Info = oServiceInfo;

                if (_MfgOrder.TextEditControl.Text != "")
                {
                    oServiceData.MfgOrder = (NamedObjectRef)_MfgOrder.Data;
                }
                    oServiceData.Product = (RevisionedObjectRef)_Product.Data;
                    oServiceData.ProcessSpec = (RevisionedObjectRef)_ProcessSpec.Data;
                    oServiceData.Container = new ContainerRef(lot);
                    oServiceData.Container.Name = lot;

                //requesting Owner field
                if (_Owner.TextEditControl.Text == "")
                {
                    oServiceInfo.Owner = new OM.Info();
                    oServiceInfo.Owner.RequestSelectionValues = true;
                }
                else
                {
                    oServiceData.Owner = (NamedObjectRef)_Owner.Data;
                }

                //requesting _Comments field
                if (_Comments.TextControl.Text == "")
                {
                    oServiceInfo.ScheduleInstructions = FieldInfoUtil.RequestValue();                   
                }

                //requesting Priority field
                if (_Priority.TextEditControl.Text == "")
                {
                    oServiceInfo.Priority = FieldInfoUtil.RequestValue();
                }

                oServiceInfo.ScheduleData = FieldInfoUtil.RequestValue();

                // init the result object=
                Result oResult = new Result();

                // execute to request the value
                ResultStatus resultStatus = (oService as IShopFloorBase).GetEnvironment(oServiceData, (oRequest as Request), out oResult);

                if (resultStatus.IsSuccess)
                {
                    if (_Owner.TextEditControl.Text == "")
                    {
                        _Owner.Data = (oResult.Value as LotSchedule).Owner;
                    }

                    if (_Comments.TextControl.Text == "")
                    {
                        _Comments.Data = (oResult.Value as LotSchedule).ScheduleInstructions;
                    }

                    if (_Priority.TextEditControl.Text == "")
                    {
                        _Priority.Data = (oResult.Value as LotSchedule).Priority;
                    }                    
                }
                else
                    DisplayMessage(resultStatus);
				
               
            }
            catch (Exception Ex)
            {
                DisplayMessage(new OM.ResultStatus(Ex.TargetSite.Name + "(): " + Ex.Message, false));
            }
        }

		/// <summary>
		/// Sets the Value for the depend fields on the Lot Grid.
		/// </summary>
		/// <param name="container"></param>
		public void DetailsGridDataChangeEvent(string container)
		{

			///Existing data is not null
			rsDetailGrid = PopulateGrid(container, "LotSchedule_Details");
            
			if (rsDetailGrid != null)
			{
				_ListContainers.DropDownControl.Items.Add(rsDetailGrid.Rows[0].Values[0].ToString());
				if(_ListPartialLot.DropDownControl.Items.Count==0)
					_ListPartialLot.DropDownControl.Items.Add("");
				_ListPartialLot.DropDownControl.Items.Add(rsDetailGrid.Rows[0].Values[0].ToString());


                FetchLotInfo(container);
				if (rsDetailGrid != null)
				{
					if ((rsDetailGrid.Rows[0].Values[18].ToString() == "WAFER") || (rsDetailGrid.Rows[0].Values[18].ToString() == "WAFERSORT" && Convert.ToInt32(rsDetailGrid.Rows[0].Values[1].ToString()) == 0) || (Convert.ToBoolean(rsDetailGrid.Rows[0].Values[21]) == true))

                    {
                        if (_ScheduleQty.TextControl.Text != null && _ScheduleQty.TextControl.Text != "")
                            _ScheduleQty.TextControl.Text = (Convert.ToInt32(rsDetailGrid.Rows[0].Values[2].ToString()) + (Convert.ToInt32(_ScheduleQty.TextControl.Text))).ToString();
                        else
                            _ScheduleQty.TextControl.Text = rsDetailGrid.Rows[0].Values[2].ToString();
					}
					else
					{
                        if (_ScheduleQty.TextControl.Text != null && _ScheduleQty.TextControl.Text != "")
                        {
                            if ((float.Parse(_ScheduleQty.TextControl.Text) % 1) == 0)
                                _ScheduleQty.TextControl.Text = (Convert.ToInt32(rsDetailGrid.Rows[0].Values[1].ToString()) + (Convert.ToInt32(_ScheduleQty.TextControl.Text))).ToString();
                        }
                        else
                            _ScheduleQty.TextControl.Text = rsDetailGrid.Rows[0].Values[1].ToString();
                    }
				}
               
			}
		}

		/// <summary>
		/// Data change event for Product
		/// </summary>
        private void Product_DataChanged(object sender, EventArgs e)
		{
			if (_Product.Data != null)
			{
				try
				{
					//clear the clear message
					Page.StatusBar.ClearMessage();

                    ClearDataDependency();

					// get the session and user profile
					var fs = FrameworkManagerUtil.GetFrameworkSession();

					// Get the service type of the page
					string sServiceType = Page.PrimaryServiceType;

					// Run proper constructor. We need to be dynamic with the primary service type
					var svcType = WCFObject.CreateObjectType(sServiceType + "Service");
					//create a request object
					var oRequest = WCFObject.CreateObject(sServiceType + "_Request");
					var svcConstructor = svcType.GetConstructor(new Type[] { typeof(UserProfile) });
					var oService = svcConstructor.Invoke(new object[] { fs.CurrentUserProfile });

					//retriving data dynamically for Flexibility of the page.
					var data = CreateServiceData(sServiceType);
					var oServiceData = data as LotSchedule;

					var info = CreateServiceInfo(sServiceType);
					var oServiceInfo = info as LotSchedule_Info;
					(oRequest as Request).Info = oServiceInfo;

					//request of data to be retrived.				
					oServiceData.MfgOrder = (NamedObjectRef)_MfgOrder.Data;
					oServiceData.Product = (RevisionedObjectRef)_Product.Data;


					oServiceInfo.ProductBOM = new OM.Info();
					oServiceInfo.ProductBOM.RequestSelectionValues = true;
					oServiceInfo.ProcessSpec = new OM.Info();
					oServiceInfo.ProcessSpec.RequestSelectionValues = true;


					// init the result object=
					Result oResult = new Result();

					// execute to request the value
					ResultStatus resultStatus = (oService as IShopFloorBase).GetEnvironment(oServiceData, (oRequest as Request), out oResult);

					if (resultStatus.IsSuccess)
					{
						_ProcessSpec.SetSelectionValues((oResult.Environment as LotSchedule_Environment).ProcessSpec.SelectionValues);
						if ((oResult.Environment as LotSchedule_Environment).ProcessSpec.SelectionValues != null && (oResult.Environment as LotSchedule_Environment).ProcessSpec.SelectionValues.Rows.Length == 1)
						_ProcessSpec.Data = new RevisionedObjectRef((oResult.Environment as LotSchedule_Environment).ProcessSpec.SelectionValues.Rows[0].Values[0], (oResult.Environment as LotSchedule_Environment).ProcessSpec.SelectionValues.Rows[0].Values[1]);
							
						Page.CollectDataContractByName("LotSchedule_ProcessSpecDM");
						Page.CollectDataContractByName("LotSchedule_ProcessSpecRevDM");
						Page.CollectDataContractByName("LotSchedule_ProcessSpecIsRORDM");
						
						_ProductBOM.SetSelectionValues((oResult.Environment as LotSchedule_Environment).ProductBOM.SelectionValues);
					}
					else
						DisplayMessage(resultStatus);
				}
				catch (Exception Ex)
				{

					DisplayMessage(new OM.ResultStatus(Ex.TargetSite.Name + "(): " + Ex.Message, false));
				}
				finally
				{

				}
			}
		}

		/// <summary>
		/// Clears data for the product data change event
		/// </summary>
		private void ClearDataDependency()
		{
			_ProductBOM.ClearData();
			_ProcessSpec.ClearData();
			_Priority.ClearData();
			_FirstWIPStep.ClearData();
			_ExpectedEndDate.ClearData();
			_ExpectedStartDate.ClearData();
			_CycleTime.ClearData();
		}

		/// <summary>
		/// Loads Value for the LotScheduleForm
		/// </summary>
		public void ProcessSpec_DataChanged()
		{
            
			if (_ProcessSpec.Data != null)
			{
				try
				{
					//clear the clear message
					Page.StatusBar.ClearMessage();
                    
                    _ListContainers.ClearData();
                    _ListPartialLot.ClearData();
                    _ListContainers.DropDownControl.Items.Clear();
                    _ListPartialLot.DropDownControl.Items.Clear();
                    _ScheduleQty.ClearData();

                    if (HandlersGrid.Data != null)
                        HandlersGrid.ClearData();   
           
					// get the session and user profile
					var fs = FrameworkManagerUtil.GetFrameworkSession();

					// Get the service type of the page
					string sServiceType = Page.PrimaryServiceType;

					// Run proper constructor. We need to be dynamic with the primary service type
					var svcType = WCFObject.CreateObjectType(sServiceType + "Service");
					//create a request object
					var oRequest = WCFObject.CreateObject(sServiceType + "_Request");
					var svcConstructor = svcType.GetConstructor(new Type[] { typeof(UserProfile) });
					var oService = svcConstructor.Invoke(new object[] { fs.CurrentUserProfile });

					//retriving data dynamically for Flexibility of the page.
					var data = CreateServiceData(sServiceType);
					var oServiceData = data as LotSchedule;

					var info = CreateServiceInfo(sServiceType);
					var oServiceInfo = info as LotSchedule_Info;
					(oRequest as Request).Info = oServiceInfo;

					//request of data to be retrived.				
					oServiceData.MfgOrder = (NamedObjectRef)_MfgOrder.Data;
					oServiceData.Product = (RevisionedObjectRef)_Product.Data;
					oServiceData.ProcessSpec = (RevisionedObjectRef)_ProcessSpec.Data;

					oServiceInfo.FirstWIPStep = new OM.Info();
					oServiceInfo.FirstWIPStep.RequestSelectionValues = true;

					oServiceInfo.ExpectedStartDate = FieldInfoUtil.RequestValue();
					oServiceInfo.ExpectedEndDate = FieldInfoUtil.RequestValue();
					oServiceInfo.CycleTime = FieldInfoUtil.RequestValue();
					oServiceInfo.TestParamsDetailsSelection = new ScheduleTestParamsDetails_Info();
					oServiceInfo.TestParamsDetailsSelection.ProcessSpec = FieldInfoUtil.RequestValue();
					oServiceInfo.TestParamsDetailsSelection.ProcessType = FieldInfoUtil.RequestValue();
					oServiceInfo.TestParamsDetailsSelection.Spec = FieldInfoUtil.RequestValue();
					oServiceInfo.TestParamsDetailsSelection.TestProgramName = FieldInfoUtil.RequestValue();
					oServiceInfo.TestParamsDetailsSelection.TestProgramMajorRevision = FieldInfoUtil.RequestValue();
					oServiceInfo.TestParamsDetailsSelection.TestProgramMinorRevision = FieldInfoUtil.RequestValue();
					oServiceInfo.TestParamsDetailsSelection.TestTemperature = FieldInfoUtil.RequestValue();
					oServiceInfo.TestParamsDetailsSelection.TestCode = FieldInfoUtil.RequestValue();

					// init the result object=
					Result oResult = new Result();

					// execute to request the value
					ResultStatus resultStatus = (oService as IShopFloorBase).GetEnvironment(oServiceData, (oRequest as Request), out oResult);                   
					if (resultStatus.IsSuccess)
					{
						_FirstWIPStep.SetSelectionValues((oResult.Environment as LotSchedule_Environment).FirstWIPStep.SelectionValues);
						oServiceData.FirstWIPStep = (NamedSubentityRef)_FirstWIPStep.Data;

						oServiceData.ExpectedStartDate = (oResult.Value as LotSchedule).ExpectedStartDate.Value;
						oServiceData.ExpectedEndDate = (oResult.Value as LotSchedule).ExpectedEndDate.Value;
						oServiceData.CycleTime = (oResult.Value as LotSchedule).CycleTime.Value;

						_FirstWIPStep.Data = (oResult.Environment as LotSchedule_Environment).FirstWIPStep.SelectionValues.Rows[0].Values[0];
						_ExpectedStartDate.Data = oServiceData.ExpectedStartDate.Value;
						_ExpectedEndDate.Data = oServiceData.ExpectedEndDate.Value;
						_CycleTime.Data = oServiceData.CycleTime;
					

						if ((oResult.Value as SchedulingTxn).TestParamsDetailsSelection != null)
						{
							JQDataGrid TestParamsDetailsGrid = Page.FindCamstarControl("LotSchedule_TestParamsDetails") as JQDataGrid;
							//Binding the Data to the Grid						
							(TestParamsDetailsGrid.GridContext as BoundContext).Data = (oResult.Value as SchedulingTxn).TestParamsDetailsSelection.ToArray();
							TestParamsDetailsGrid.BoundContext.LoadData();
							//Rendering the Grid with data
							CamstarWebControl.SetRenderToClient(TestParamsDetailsGrid);
						}

					}
				}               
				catch (Exception Ex)
				{

					DisplayMessage(new OM.ResultStatus(Ex.TargetSite.Name + "(): " + Ex.Message, false));
				}
			}
		}

		/// <summary>
		/// Data change event for MfgOrder
		/// </summary>
		public void MfgOrderField_DataChanged()
		{
			if (_MfgOrder.Data != null)
			{
				try
				{
					//clear the clear message
					Page.StatusBar.ClearMessage();

					// get the session and user profile
					var fs = FrameworkManagerUtil.GetFrameworkSession();

					// Get the service type of the page
					string sServiceType = Page.PrimaryServiceType;

					// Run proper constructor. We need to be dynamic with the primary service type
					var svcType = WCFObject.CreateObjectType(sServiceType + "Service");
					//create a request object
					var oRequest = WCFObject.CreateObject(sServiceType + "_Request");
					var svcConstructor = svcType.GetConstructor(new Type[] { typeof(UserProfile) });
					var oService = svcConstructor.Invoke(new object[] { fs.CurrentUserProfile });

					//retriving data dynamically for Flexibility of the page.
					var data = CreateServiceData(sServiceType);
					var oServiceData = data as LotSchedule;

					var info = CreateServiceInfo(sServiceType);
					var oServiceInfo = info as LotSchedule_Info;
					(oRequest as Request).Info = oServiceInfo;

					//request of data to be retrived.				
					oServiceData.MfgOrder = new NamedObjectRef(_MfgOrder.Data.ToString());
					oServiceInfo.Product = FieldInfoUtil.RequestValue();
                    oServiceInfo.ProductBOM = FieldInfoUtil.RequestValue();
                    oServiceInfo.ProcessSpec = FieldInfoUtil.RequestValue();


					oServiceInfo.ProductBOM = new OM.Info();
					oServiceInfo.ProductBOM.RequestSelectionValues = true;
					oServiceInfo.ProcessSpec = new OM.Info();
					oServiceInfo.ProcessSpec.RequestSelectionValues = true;
					oServiceInfo.Priority = new OM.Info();
					oServiceInfo.Priority.RequestSelectionValues = true;


					// init the result object
					Result oResult = new Result();

					// execute to request the value
					ResultStatus resultStatus = (oService as IShopFloorBase).GetEnvironment(oServiceData, (oRequest as Request), out oResult);

					if (resultStatus.IsSuccess)
					{
						_ProcessSpec.SetSelectionValues((oResult.Environment as LotSchedule_Environment).ProcessSpec.SelectionValues);
						_ProductBOM.SetSelectionValues((oResult.Environment as LotSchedule_Environment).ProductBOM.SelectionValues);
						_Priority.SetSelectionValues((oResult.Environment as LotSchedule_Environment).Priority.SelectionValues);

						_Product.Data = (oResult.Value as LotSchedule).Product;
                        _ProcessSpec.Data = (oResult.Value as LotSchedule).ProcessSpec;
                        _ProductBOM.Data = (oResult.Value as LotSchedule).ProductBOM;
					}
					else
						DisplayMessage(resultStatus);
				}
				catch (Exception Ex)
				{

					DisplayMessage(new OM.ResultStatus(Ex.TargetSite.Name + "(): " + Ex.Message, false));
				}
				finally
				{

				}
			}
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Populates grid with target container or containers.
		/// </summary>
		/// <param name="lots">Lots to add</param>
		/// <param name="gridNameRef">Reference name to the Grid</param>
		/// <returns>RecordSet</returns>
		private RecordSet PopulateGrid(string lot, string gridNameRef)
		{
			JQDataGrid _gridContainers = Page.FindCamstarControl(gridNameRef) as JQDataGrid;
			RecordSet rset;
			rset = SEMI.AppCode.UIUtility.GetLotQuerySelection(this, "LotSchedule", lot, false, ref _gridContainers, gridNameRef);
			//SchedulingTxnDetails
			return rset;
		}

		/// <summary>
		/// Check to see if the data is unique, if not return a unique list.
		/// </summary>
		/// <param name="grid">JQDataGrid</param>
		/// <param name="lotList">ref string[]</param>
		private void GetUniqueList(JQDataGrid grid, ref string[] lotList)
		{
			int totalRows = grid.GridContext.GetTotalRows();
			bool isUnique = false;

			foreach (string lot in lotList)
			{
				for (int x = 0; x < totalRows; x++)
				{
					isUnique = !lot.Equals(grid.GridContext.GetCell(x, grid.Settings.Columns[0].Name).ToString());
					if (!isUnique)
					{
						lotList = lotList.Where(val => val != lot).ToArray();
					}
				}
			}
		}

		/// <summary>
		/// Check to see if the single lot is unique
		/// </summary>
		/// <param name="grid">JQDataGrid</param>
		/// <param name="lotList">ref string</param>
		private bool IsUniqueSingle(JQDataGrid grid, string lot)
		{
			int totalRows = grid.GridContext.GetTotalRows();
			bool isUnique = false;

			if (totalRows.Equals(0))
				isUnique = true;
			else
			{
				isUnique = true;
				for (int x = 0; x < totalRows; x++)
				{
					bool isUniqueTemp = false;
					//Compare value to check if Unique
					isUniqueTemp = lot.ToUpper().Equals(grid.GridContext.GetCell(x, grid.Settings.Columns[0].Name).ToString().ToUpper());
					if (isUniqueTemp)
						isUnique = false;
				}
			}
			return isUnique;
		}		

		/// <summary>
		/// Clear control data on the form.
		/// </summary>
		public void clearData()
		{
			JQDataGrid HandlersGrid = Page.FindCamstarControl("LotSchedule_Details") as JQDataGrid;
			int totalRows = HandlersGrid.GridContext.GetTotalRows();
			_ListContainers.ClearData();
			_ListPartialLot.ClearData();
			_ListContainers.DropDownControl.Items.Clear();
			_ListPartialLot.DropDownControl.Items.Clear();
			_ScheduleQty.ClearData();
			txtComputerName.ClearData();
			_MfgOrder.ClearData();
			_Product.ClearData();

			_ProductBOM.ClearData();
			_ProcessSpec.ClearData();
			_Priority.ClearData();
			_FirstWIPStep.ClearData();
			_ExpectedEndDate.ClearData();
			_ExpectedStartDate.ClearData();
			_CycleTime.ClearData();

			_SelectionIdField.ClearData();
			_envSelectedLots.ClearData();
			HandlersGrid.ClearData();
			_SSPlanRef.ClearData();
			JQDataGrid SSDetailsGrid = Page.FindCamstarControl("LotSchedule_SSDetails") as JQDataGrid;
			SSDetailsGrid.ClearData();
            _gridSpecsSelection.ClearData();
			_gridSSByStepDetails.ClearData();
		}

		/// <summary>
		/// Updates fields that are dependent on the Lot details grid.
		/// </summary>
		public void UpdateFields()
		{
			JQDataGrid grid = Page.FindCamstarControl("LotSchedule_Details") as JQDataGrid;
			int totalRows = HandlersGrid.GridContext.GetTotalRows();
			_ListContainers.ClearData();
			_ListPartialLot.ClearData();
			_ListContainers.DropDownControl.Items.Clear();
			_ListPartialLot.DropDownControl.Items.Clear();
			_ScheduleQty.ClearData();

			for (int x = 0; x < totalRows; x++)
			{
				_ListContainers.DropDownControl.Items.Add(grid.GridContext.GetCell(x, grid.Settings.Columns[0].Name).ToString());
				if (_ListPartialLot.DropDownControl.Items.Count == 0)
					_ListPartialLot.DropDownControl.Items.Add("");
				_ListPartialLot.DropDownControl.Items.Add(grid.GridContext.GetCell(x, grid.Settings.Columns[0].Name).ToString());

				//To parse a string, we have to make sure that there is a value
				if (rsDetailGrid != null)
				{
                    if ((rsDetailGrid.Rows[0].Values[18].ToString() == "WAFER") || (rsDetailGrid.Rows[0].Values[18].ToString() == "WAFERSORT" && Convert.ToInt32(rsDetailGrid.Rows[0].Values[1].ToString()) == 0) || (Convert.ToBoolean(rsDetailGrid.Rows[0].Values[21]) == true))

					{
                        if (_ScheduleQty.TextControl.Text != null && _ScheduleQty.TextControl.Text != "")
                            _ScheduleQty.TextControl.Text = (Convert.ToInt32(rsDetailGrid.Rows[0].Values[2].ToString()) + (Convert.ToInt32(_ScheduleQty.TextControl.Text))).ToString();
                        else
                            _ScheduleQty.TextControl.Text = rsDetailGrid.Rows[0].Values[2].ToString();
                    }
					else
					{
                        if (_ScheduleQty.TextControl.Text != null && _ScheduleQty.TextControl.Text != "")
                            _ScheduleQty.TextControl.Text = (Convert.ToInt32(rsDetailGrid.Rows[0].Values[1].ToString()) + (Convert.ToInt32(_ScheduleQty.TextControl.Text))).ToString();
                        else
                            _ScheduleQty.TextControl.Text = rsDetailGrid.Rows[0].Values[1].ToString();
                    }
				}
			}

			CamstarWebControl.SetRenderToClient(_ListContainers);
			CamstarWebControl.SetRenderToClient(_ListPartialLot);
			CamstarWebControl.SetRenderToClient(_ScheduleQty);
		}

        //-----------------------------------------
        // Get Schedule SS Specs
        //-----------------------------------------
        public virtual void GetScheduleSSSpecs(string ServiceType, bool IsSS, string ProcessSpecName, string ProcessSpecRevision)
        {
            try
            {
                // Prepare service
                var fs = FrameworkManagerUtil.GetFrameworkSession();
                ResultStatus oServiceResult = new ResultStatus(null, false);
                string sServiceType = "LotSchedule";
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
                var oServiceInfo = info as LotSchedule_Info;
                (oRequest as Request).Info = oServiceInfo;

                if (IsSS)
                {
                    (oServiceData as LotSchedule).SS = new RevisionedObjectRef();
                    (oServiceData as LotSchedule).SS.Name = ProcessSpecName;
                    if (ProcessSpecRevision != "")
                    {
                        (oServiceData as LotSchedule).SS.Revision = ProcessSpecRevision;
                        (oServiceData as LotSchedule).SS.RevisionOfRecord = false;
                    }
                    else
                    {
                        (oServiceData as LotSchedule).SS.RevisionOfRecord = true;
                    }
                }
                else
                {
                    (oServiceData as LotSchedule).ProcessSpec = new RevisionedObjectRef();
                    (oServiceData as LotSchedule).ProcessSpec.Name = ProcessSpecName;
                    if (ProcessSpecRevision != "")
                    {
                        (oServiceData as LotSchedule).ProcessSpec.Revision = ProcessSpecRevision;
                        (oServiceData as LotSchedule).ProcessSpec.RevisionOfRecord = false;
                    }
                    else
                    {
                        (oServiceData as LotSchedule).ProcessSpec.RevisionOfRecord = true;
                    }
                }

                // Request Selection Value
				if (IsSS)
				{
					oServiceInfo.SSMoveOutToSpec = FieldInfoUtil.RequestSelectionValue();
					oServiceInfo.SSMoveOutToStep = FieldInfoUtil.RequestSelectionValue();
				}
				else
				{
					oServiceInfo.SSMoveOutFromSpec = FieldInfoUtil.RequestSelectionValue();
					oServiceInfo.SSMoveOutFromStep = FieldInfoUtil.RequestSelectionValue();
				}

                // Perform the request
                Result oResult = new Result();
                ResultStatus oResultStatus = new ResultStatus();

                oResultStatus = (oService as IShopFloorBase).GetEnvironment(oServiceData, (oRequest as Request), out oResult);

                if (oResultStatus.IsSuccess)
                {
                    if (IsSS)
                    {
                        _gridSpecsSelection.ClearData();
                        _gridSpecsSelection.Data = (oResult.Environment as LotSchedule_Environment).SSMoveOutToSpec.SelectionValues.GetAsExplicitlyDataTable();
                        _gridSpecsSelection.OriginalData = (oResult.Environment as LotSchedule_Environment).SSMoveOutToSpec.SelectionValues.GetAsExplicitlyDataTable();
                    }
                    else
                    {
                        _gridSpecsSelection.ClearData();
                        _gridSpecsSelection.Data = (oResult.Environment as LotSchedule_Environment).SSMoveOutFromSpec.SelectionValues.GetAsExplicitlyDataTable();
                        _gridSpecsSelection.OriginalData = (oResult.Environment as LotSchedule_Environment).SSMoveOutFromSpec.SelectionValues.GetAsExplicitlyDataTable();
                    }
                }
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

        //-----------------------------------------
        // Popup Data Selection
        //-----------------------------------------
        public virtual void PopupDataSelection(string Key, bool EndResponse = false)
        {
            try
            {
				if (Page.DataContract.GetValueByName("LotSchedule_GridRowIdDM") != "" || Page.DataContract.GetValueByName("LotSchedule_StepGridRowIdDM") != "")
                {
                    bool bIsSS = false;
                    string sSSKey = "";
                    string sSSValue = "";
					if (Key == "MoveOutFromSpec" || Key == "MoveBackToSpec" || Key == "MoveOutFromStep" || Key == "MoveBackToStep")
                    {
                        bIsSS = false;
                        if (_ProcessSpec.Data != null)
                            sSSValue = _ProcessSpec.Data.ToString();
                    }
                    else
                    {
                        bIsSS = true;
						if (Page.EventTarget.Contains(_gridSSDetails.ID))
						{
							if (_gridSSDetails.GridContext.GetCell(Page.DataContract.GetValueByName("LotSchedule_GridRowIdDM").ToString(), "SS") != null)
								sSSValue = _gridSSDetails.GridContext.GetCell(Page.DataContract.GetValueByName("LotSchedule_GridRowIdDM").ToString(), "SS").ToString();
						}
						else if (Page.EventTarget.Contains(_gridSSByStepDetails.ID))
						{
							if (_gridSSByStepDetails.GridContext.GetCell(Page.DataContract.GetValueByName("LotSchedule_StepGridRowIdDM").ToString(), "SS") != null)
								sSSValue = _gridSSByStepDetails.GridContext.GetCell(Page.DataContract.GetValueByName("LotSchedule_StepGridRowIdDM").ToString(), "SS").ToString();
						}
                    }
                    if (sSSValue != "")
                    {
                        string[] splitsSSValue = sSSValue.Split(':');
                        if (splitsSSValue.Length > 1)
                            GetScheduleSSSpecs(Page.PrimaryServiceType, bIsSS, splitsSSValue[0], splitsSSValue[1]);
                        else
                            GetScheduleSSSpecs(Page.PrimaryServiceType, bIsSS, sSSValue, "");

                        if (_gridSpecsSelection.Data != null || _SupplementarySpec.Data != null)
                        {
                            Camstar.WebPortal.Personalization.FloatPageOpenAction objAction = new FloatPageOpenAction();

							int i;
							if (Key == "MoveOutFromStep" || Key == "MoveOutToStep" || Key == "MoveBackFromStep" || Key == "MoveBackToStep")
								i = 7;
							else
								i = 3;
							UIComponentDataContractLink[] objLinks = new UIComponentDataContractLink[i];

							if (Key == "MoveOutFromStep" || Key == "MoveBackToStep")
							{
								Page.DataContract.SetValueByName("LotSchedule_PrimaryService", this.PrimaryServiceType);
								objAction.PageName = "SS_WorkflowStepSelectionPopupVP";
								objLinks[0] = new UIComponentDataContractLink();
								objLinks[0].SourceMember = "LotSchedule_PrimaryService";
								objLinks[0].TargetMember = "Popup_PrimaryService";
								objLinks[1] = new UIComponentDataContractLink();
								objLinks[1].SourceMember = "LotSchedule_ProcessSpecDM";
								objLinks[1].TargetMember = "Popup_ProcessSpecDM";
								objLinks[2] = new UIComponentDataContractLink();
								objLinks[2].SourceMember = "LotSchedule_ProcessSpecRevDM";
								objLinks[2].TargetMember = "Popup_ProcessSpecRevDM";
								objLinks[3] = new UIComponentDataContractLink();
								objLinks[3].SourceMember = "LotSchedule_ProcessSpecIsRORDM";
								objLinks[3].TargetMember = "Popup_ProcessSpeIsRORDM";

								objLinks[4] = new UIComponentDataContractLink();
								objLinks[5] = new UIComponentDataContractLink();
								objLinks[6] = new UIComponentDataContractLink();

								if (Page.DataContract.GetValueByName("LotSchedule_IsSSWorkflow") != null)
								{
									var SelectedSSWorkflow = Page.DataContract.GetValueByName("LotSchedule_IsSSWorkflow") as RevisionedObjectRef;
									var SelectedSSStep = Page.DataContract.GetValueByName("LotSchedule_IsSSStep").ToString();
									Page.DataContract.SetValueByName("LotSchedule_SelectedSSWorkflow", SelectedSSWorkflow.Name);
									Page.DataContract.SetValueByName("LotSchedule_SelectedSSWorkflowRev", SelectedSSWorkflow.Revision);
									Page.DataContract.SetValueByName("LotSchedule_SSSelectedStep", SelectedSSStep);

									objLinks[4].SourceMember = "LotSchedule_SelectedSSWorkflow";
									objLinks[5].SourceMember = "LotSchedule_SelectedSSWorkflowRev";
									objLinks[6].SourceMember = "LotSchedule_SSSelectedStep";
								}
								else
								{
									Page.DataContract.SetValueByName("LotSchedule_SSSelectedStep", null);
									objLinks[4].SourceMember = "LotSchedule_WorkflowDM";
									objLinks[5].SourceMember = "LotSchedule_WorkflowRevDM";
									objLinks[6].SourceMember = "LotSchedule_SSSelectedStep";
								}
								objLinks[4].TargetMember = "Popup_Workflow";
								objLinks[5].TargetMember = "Popup_WorkflowRev";
								objLinks[6].TargetMember = "Popup_SSReturnedSelectedStep";

								Page.PortalContext.DataContract.SetValueByName("LotSchedule_IsSSWorkflow", null);
								Page.PortalContext.DataContract.SetValueByName("LotSchedule_IsSSStep", null);

								objAction.FrameLocation = new UIFloatingPageLocation();
								objAction.FrameLocation.Width = 510;
								objAction.FrameLocation.Height = 480;
								objAction.EndResponse = false;
							}
							
							else if (Key == "MoveOutToStep" || Key == "MoveBackFromStep")
							{
								Page.DataContract.SetValueByName("LotSchedule_PrimaryService", this.PrimaryServiceType);
								objAction.PageName = "SS_WorkflowStepSelectionPopupVP";
								objLinks[0] = new UIComponentDataContractLink();
								objLinks[0].SourceMember = "LotSchedule_PrimaryService";
								objLinks[0].TargetMember = "Popup_PrimaryService";
								objLinks[1] = new UIComponentDataContractLink();
								objLinks[1].SourceMember = "LotSchedule_SupplementarySpecDM";
								objLinks[1].TargetMember = "Popup_ProcessSpecDM";
								objLinks[2] = new UIComponentDataContractLink();
								objLinks[2].SourceMember = "LotSchedule_SupplementarySpecRevDM";
								objLinks[2].TargetMember = "Popup_ProcessSpeRevDM";
								objLinks[3] = new UIComponentDataContractLink();
								objLinks[3].SourceMember = "LotSchedule_SupplementarySpecIsRORDM";
								objLinks[3].TargetMember = "Popup_ProcessSpecIsRORDM";
								objLinks[4] = new UIComponentDataContractLink();
								objLinks[5] = new UIComponentDataContractLink();
								objLinks[6] = new UIComponentDataContractLink();

								if (Page.DataContract.GetValueByName("LotSchedule_IsSSWorkflow") != null)
								{
									var SelectedSSWorkflow = Page.DataContract.GetValueByName("LotSchedule_IsSSWorkflow") as RevisionedObjectRef;
									var SelectedSSStep = Page.DataContract.GetValueByName("LotSchedule_IsSSStep").ToString();
									Page.DataContract.SetValueByName("LotSchedule_SelectedSSWorkflow", SelectedSSWorkflow.Name);
									Page.DataContract.SetValueByName("LotSchedule_SelectedSSWorkflowRev", SelectedSSWorkflow.Revision);
									Page.DataContract.SetValueByName("LotSchedule_SSSelectedStep", SelectedSSStep);

									objLinks[4].SourceMember = "LotSchedule_SelectedSSWorkflow";
									objLinks[5].SourceMember = "LotSchedule_SelectedSSWorkflowRev";
									objLinks[6].SourceMember = "LotSchedule_SSSelectedStep";
								}
								else
								{
									Page.DataContract.SetValueByName("LotSchedule_SSSelectedStep", null);
									objLinks[4].SourceMember = "LotSchedule_SSWorkflowDM";
									objLinks[5].SourceMember = "LotSchedule_SSWorkflowRevDM";
									objLinks[6].SourceMember = "LotSchedule_SSSelectedStep";
								}								
								objLinks[4].TargetMember = "Popup_Workflow";
								objLinks[5].TargetMember = "Popup_WorkflowRev";
								objLinks[6].TargetMember = "Popup_SSReturnedSelectedStep";

								Page.PortalContext.DataContract.SetValueByName("LotSchedule_IsSSWorkflow", null);
								Page.PortalContext.DataContract.SetValueByName("LotSchedule_IsSSStep", null);								
								objAction.FrameLocation = new UIFloatingPageLocation();
								objAction.FrameLocation.Width = 510;
								objAction.FrameLocation.Height = 480;
								objAction.EndResponse = false;
							}
							else
							{
								objAction.PageName = "SS_DataSelectionValuesPopupVP";
								objLinks[0] = new UIComponentDataContractLink();
								objLinks[0].SourceMember = "LotSchedule_ToFromPopupKey";
								objLinks[0].TargetMember = "Popup_KeyDM";
								objLinks[1] = new UIComponentDataContractLink();
								objLinks[1].SourceMember = "LotSchedule_SpecsSelectionDM";
								objLinks[1].TargetMember = "Popup_SpecsSelectionDM";
								objLinks[2] = new UIComponentDataContractLink();
								objLinks[2].SourceMember = "LotSchedule_IsSSDM";
								objLinks[2].TargetMember = "Popup_IsSSDM";

								objAction.FrameLocation = new UIFloatingPageLocation();
								objAction.FrameLocation.Width = 380;
								objAction.FrameLocation.Height = 480;
								objAction.EndResponse = false;
							}
                            UIComponentDataContractReturnLink[] objReturnLinks = new UIComponentDataContractReturnLink[2];
                            objReturnLinks[0] = new UIComponentDataContractReturnLink();
                            objReturnLinks[1] = new UIComponentDataContractReturnLink();
                            if (Key == "MoveOutFromSpec")
                            {
                                objReturnLinks[0].SourceMember = "Popup_ReturnSelectedSpecDM";
                                objReturnLinks[0].TargetMember = "LotSchedule_ReturnedMoveOutFromSpec";
                                objReturnLinks[1].SourceMember = "Popup_ReturnSelectedSpecRevDM";
                                objReturnLinks[1].TargetMember = "LotSchedule_ReturnedMoveOutFromSpecRev";
                            }
                            else if (Key == "MoveOutToSpec")
                            {
                                objReturnLinks[0].SourceMember = "Popup_ReturnSelectedSpecDM";
                                objReturnLinks[0].TargetMember = "LotSchedule_ReturnedMoveOutToSpec";
                                objReturnLinks[1].SourceMember = "Popup_ReturnSelectedSpecRevDM";
                                objReturnLinks[1].TargetMember = "LotSchedule_ReturnedMoveOutToSpecRev";
                            }
                            else if (Key == "MoveBackFromSpec")
                            {
                                objReturnLinks[0].SourceMember = "Popup_ReturnSelectedSpecDM";
                                objReturnLinks[0].TargetMember = "LotSchedule_ReturnedMoveBackFromSpec";
                                objReturnLinks[1].SourceMember = "Popup_ReturnSelectedSpecRevDM";
                                objReturnLinks[1].TargetMember = "LotSchedule_ReturnedMoveBackFromSpecRev";
                            }
                            else if (Key == "MoveBackToSpec")
                            {
                                objReturnLinks[0].SourceMember = "Popup_ReturnSelectedSpecDM";
                                objReturnLinks[0].TargetMember = "LotSchedule_ReturnedMoveBackToSpec";
                                objReturnLinks[1].SourceMember = "Popup_ReturnSelectedSpecRevDM";
                                objReturnLinks[1].TargetMember = "LotSchedule_ReturnedMoveBackToSpecRev";
                            }
							else if (Key == "MoveOutFromStep")
							{
								objReturnLinks[0].SourceMember = "Popup_ReturnedSelectedStep";
								objReturnLinks[0].TargetMember = "LotSchedule_ReturnedMoveOutFromStep";
								objReturnLinks[1].SourceMember = "Popup_ReturnedSelectedWorkflow";
								objReturnLinks[1].TargetMember = "LotSchedule_ReturnedWorkflow";
							}
							else if (Key == "MoveOutToStep")
							{
								objReturnLinks[0].SourceMember = "Popup_ReturnedSelectedStep";
								objReturnLinks[0].TargetMember = "LotSchedule_ReturnedMoveOutToStep";
								objReturnLinks[1].SourceMember = "Popup_ReturnedSelectedWorkflow";
								objReturnLinks[1].TargetMember = "LotSchedule_ReturnedSSWorkflow";
							}
							else if (Key == "MoveBackFromStep")
							{
								objReturnLinks[0].SourceMember = "Popup_ReturnedSelectedStep";
								objReturnLinks[0].TargetMember = "LotSchedule_ReturnedMoveBackFromStep";
								objReturnLinks[1].SourceMember = "Popup_ReturnedSelectedWorkflow";
								objReturnLinks[1].TargetMember = "LotSchedule_ReturnedSSWorkflow";
							}
							else if (Key == "MoveBackToStep")
							{
								objReturnLinks[0].SourceMember = "Popup_ReturnedSelectedStep";
								objReturnLinks[0].TargetMember = "LotSchedule_ReturnedMoveBackToStep";
								objReturnLinks[1].SourceMember = "Popup_ReturnedSelectedWorkflow";
								objReturnLinks[1].TargetMember = "LotSchedule_ReturnedWorkflow";
							}
                            objAction.DataContractMap = new UIComponentDataContractMap();
                            objAction.DataContractMap.Links = objLinks;
                            objAction.DataContractReturnMap = new UIComponentDataContractReturnMap();
                            objAction.DataContractReturnMap.ReturnLinks = objReturnLinks;
                            //objAction.FrameLocation = new UIFloatingPageLocation();
							//objAction.FrameLocation.Width = 380;
							//objAction.FrameLocation.Height = 480;
							//objAction.EndResponse = false;
                           
							Page.ActionDispatcher.ExecuteAction(objAction);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

		#endregion

		#region Page Events

		/// <summary>
		/// On page load
		/// </summary>
		/// <param name="e"></param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			_FirstWIPStepWorkflow.StepControl.Required = false;
			_btnHiddenUpdateFields.Hidden = true;
			//ScriptManager.RegisterStartupScript(Page.Form, GetType(), "ExecuteSearch", " $('#ctl00_WebPartManager_SS_MultiLotSelectionLotListWP_Control0').click();", true);

			HandlersGrid.GridContext.RowDeleted += GridContext_RowDeleted;
            _Product.DataChanged += Product_DataChanged;
			
			//Clear Display message
			DisplayMessage(new ResultStatus("", true));

			// Get Computername
			txtComputerName.Data = SEMI.AppCode.UIUtility.GetComputerName(this);

            // Add ss popup key
            string ssKey = Page.PrimaryServiceType;
            switch (ssKey)
            {
                case "WaferLotSchedule":
                case "WaferSortLotSchedule":
                case "BackGrindLotSchedule":
                case "AssemblyLotSchedule":
                case "AssemblyCarrierLotSchedule":
                case "TestLotSchedule":
                case "FinalTestLotSchedule":
                    _txtSSPopupKey.Data = ssKey.Replace("LotSchedule", "") + "SS";
                    break;
                case "AssemblyMotherLotSchedule":
                case "AssemblySubLotSchedule":
                    _txtSSPopupKey.Data = ssKey.Replace("Schedule", "") + "SS";
                    break;
                default:
                    _txtSSPopupKey.Data = "SupplementarySpec";
                    break;
            }
			
			if (ssKey.Contains("Batch"))
			{
				_txtNewLotID.Visible = false;
				_ListPartialLot.Visible = false;
				_chkAutoSetNewLotID.Visible = false;
				_ScheduleQty.Visible = false;
				_ListContainers.Visible = false;
			}
            
            // Add To From popup key
            _txtToFromPopupKey.Data = "MoveOutToSpec";

            // Add Selected Row Id
            if (Page.DataContract.GetValueByName("LotSchedule_GridRowIdDM") != null)
                _txtSelectedRowId.Data = Page.DataContract.GetValueByName("LotSchedule_GridRowIdDM").ToString();
			else if (Page.DataContract.GetValueByName("LotSchedule_StepGridRowIdDM") != null)
				_txtSelectedRowId.Data = Page.DataContract.GetValueByName("LotSchedule_StepGridRowIdDM").ToString();
                           
			// Check if it is a pop up close, get the return result
			if (SEMI.AppCode.UIUtility.IsPopupClose(this))
			{
				if (_envSelectedLots != null)

					// manually initialize the containers list data contract since it is not triggered when we do a manual popup close
					if (Page.DataContract.GetValueByName("LotSchedule_SelectedLotsListDM") != null)
					{
						//gets the list of returned lot from the popup form
						_envSelectedLots.SS_ContainersList = Page.DataContract.GetValueByName("LotSchedule_SelectedLotsListDM") as string[];
					}

				if (_envSelectedLots.SS_ContainersList != null)
				{
					string[] sContainers;
					sContainers = _envSelectedLots.SS_ContainersList;

					//Get Details grid for comparing data
					JQDataGrid DetailsGrid = Page.FindCamstarControl("LotSchedule_Details") as JQDataGrid;

					GetUniqueList(DetailsGrid, ref sContainers);
					
					foreach (string sContainersItem in sContainers)
					{
						//populate grid.
						DetailsGridDataChangeEvent(sContainersItem);
					}
					//nullify the containers list
					_envSelectedLots.SS_ContainersList = null;
				}

                if (_txtSelectedRowId.Data != null)
                {
                    int selectedRowId = Convert.ToInt32(_txtSelectedRowId.Data.ToString());
                    string selectedSpec = "";
                    string selectedSpecRev = "";
					CWC.NamedSubentity selectedStep;
					RevisionedObjectRef selectedWorkflow;
                    string attrVal = "";
                    string attrRev = "";
                    ScheduleSSPlanDetails[] getSSPlanDetails = _gridSSDetails.Data as ScheduleSSPlanDetails[];
					ScheduleSSDetailsEx[] getSSPlanDetailsByStep = _gridSSByStepDetails.Data as ScheduleSSDetailsEx[];

                    bool bIsSSByStep = false;
					bool bSetSSTestParams = false;

                    if (Page.PortalContext.DataContract.GetValueByName<string>("LotSchedule_ReturnedSSValue") != null)
                    {
                        attrVal = Page.PortalContext.DataContract.GetValueByName<string>("LotSchedule_ReturnedSSValue").ToString();
                        attrRev = Page.PortalContext.DataContract.GetValueByName<string>("LotSchedule_ReturnedSSRevision").ToString();
						bSetSSTestParams = true;
						if (Page.PortalContext.DataContract.GetValueByName("LotSchedule_GridRowIdDM") != null)
						{
							getSSPlanDetails[selectedRowId].SS = new RevisionedObjectRef();
							getSSPlanDetails[selectedRowId].SS.Name = attrVal;
							getSSPlanDetails[selectedRowId].SS.Revision = attrRev;
						}
						else if (Page.PortalContext.DataContract.GetValueByName("LotSchedule_StepGridRowIdDM") != null)
						{
							if (getSSPlanDetailsByStep[selectedRowId].SS != null && (getSSPlanDetailsByStep[selectedRowId].SS.Name != attrVal || getSSPlanDetailsByStep[selectedRowId].SS.Revision != attrRev))
							{
								getSSPlanDetailsByStep[selectedRowId].MoveOutToStep = null;
								getSSPlanDetailsByStep[selectedRowId].strMoveOutToStep = null;
								getSSPlanDetailsByStep[selectedRowId].MoveOutToStepWorkflow = null;
								getSSPlanDetailsByStep[selectedRowId].MoveBackFromStep = null;
								getSSPlanDetailsByStep[selectedRowId].strMoveBackFromStep = null;
								getSSPlanDetailsByStep[selectedRowId].MoveBackFromStepWorkflow = null;
							}
							getSSPlanDetailsByStep[selectedRowId].SS = new RevisionedObjectRef();
							getSSPlanDetailsByStep[selectedRowId].SS.Name = attrVal;
							getSSPlanDetailsByStep[selectedRowId].SS.Revision = attrRev;
							_SupplementarySpec.Data = _gridSSByStepDetails.GridContext.GetCell(_txtSelectedRowId.Data.ToString(), "SS");
						}
                        Page.PortalContext.DataContract.SetValueByName("LotSchedule_ReturnedSSValue", null);
                        Page.PortalContext.DataContract.SetValueByName("LotSchedule_ReturnedSSRevision", null);
                    }
                    else if (Page.PortalContext.DataContract.GetValueByName<string>("LotSchedule_ReturnedMoveOutFromSpec") != null)
                    {
                        selectedSpec = Page.PortalContext.DataContract.GetValueByName<string>("LotSchedule_ReturnedMoveOutFromSpec").ToString();
                        selectedSpecRev = Page.PortalContext.DataContract.GetValueByName<string>("LotSchedule_ReturnedMoveOutFromSpecRev").ToString();
                        getSSPlanDetails[selectedRowId].MoveOutFromSpec = new RevisionedObjectRef();
                        getSSPlanDetails[selectedRowId].MoveOutFromSpec.Name = selectedSpec;
                        getSSPlanDetails[selectedRowId].MoveOutFromSpec.Revision = selectedSpecRev;
                        Page.PortalContext.DataContract.SetValueByName("LotSchedule_ReturnedMoveOutFromSpec", null);
                        Page.PortalContext.DataContract.SetValueByName("LotSchedule_ReturnedMoveOutFromSpecRev", null);
                    }
                    else if (Page.PortalContext.DataContract.GetValueByName<string>("LotSchedule_ReturnedMoveOutToSpec") != null)
                    {
                        selectedSpec = Page.PortalContext.DataContract.GetValueByName<string>("LotSchedule_ReturnedMoveOutToSpec").ToString();
                        selectedSpecRev = Page.PortalContext.DataContract.GetValueByName<string>("LotSchedule_ReturnedMoveOutToSpecRev").ToString();
                        getSSPlanDetails[selectedRowId].MoveOutToSpec = new RevisionedObjectRef();
                        getSSPlanDetails[selectedRowId].MoveOutToSpec.Name = selectedSpec;
                        getSSPlanDetails[selectedRowId].MoveOutToSpec.Revision = selectedSpecRev;
                        Page.PortalContext.DataContract.SetValueByName("LotSchedule_ReturnedMoveOutToSpec", null);
                        Page.PortalContext.DataContract.SetValueByName("LotSchedule_ReturnedMoveOutToSpecRev", null);
                    }
                    else if (Page.PortalContext.DataContract.GetValueByName<string>("LotSchedule_ReturnedMoveBackFromSpec") != null)
                    {
                        selectedSpec = Page.PortalContext.DataContract.GetValueByName<string>("LotSchedule_ReturnedMoveBackFromSpec").ToString();
                        selectedSpecRev = Page.PortalContext.DataContract.GetValueByName<string>("LotSchedule_ReturnedMoveBackFromSpecRev").ToString();
                        getSSPlanDetails[selectedRowId].MoveBackFromSpec = new RevisionedObjectRef();
                        getSSPlanDetails[selectedRowId].MoveBackFromSpec.Name = selectedSpec;
                        getSSPlanDetails[selectedRowId].MoveBackFromSpec.Revision = selectedSpecRev;
                        Page.PortalContext.DataContract.SetValueByName("LotSchedule_ReturnedMoveBackFromSpec", null);
                        Page.PortalContext.DataContract.SetValueByName("LotSchedule_ReturnedMoveBackFromSpecRev", null);
                    }
                    else if (Page.PortalContext.DataContract.GetValueByName<string>("LotSchedule_ReturnedMoveBackToSpec") != null)
                    {
                        selectedSpec = Page.PortalContext.DataContract.GetValueByName<string>("LotSchedule_ReturnedMoveBackToSpec").ToString();
                        selectedSpecRev = Page.PortalContext.DataContract.GetValueByName<string>("LotSchedule_ReturnedMoveBackToSpecRev").ToString();
                        getSSPlanDetails[selectedRowId].MoveBackToSpec = new RevisionedObjectRef();
                        getSSPlanDetails[selectedRowId].MoveBackToSpec.Name = selectedSpec;
                        getSSPlanDetails[selectedRowId].MoveBackToSpec.Revision = selectedSpecRev;
                        Page.PortalContext.DataContract.SetValueByName("LotSchedule_ReturnedMoveBackToSpec", null);
                        Page.PortalContext.DataContract.SetValueByName("LotSchedule_ReturnedMoveBackToSpecRev", null);
                    }
					else if (Page.PortalContext.DataContract.GetValueByName("LotSchedule_ReturnedMoveOutFromStep") != null)
					{
						selectedStep = Page.PortalContext.DataContract.GetValueByName("LotSchedule_ReturnedMoveOutFromStep") as Camstar.WebPortal.FormsFramework.WebControls.NamedSubentity;
						selectedWorkflow = Page.PortalContext.DataContract.GetValueByName("LotSchedule_ReturnedWorkflow") as RevisionedObjectRef;
						getSSPlanDetailsByStep[selectedRowId].MoveOutFromStep = new NamedSubentityRef();
						getSSPlanDetailsByStep[selectedRowId].MoveOutFromStep = (selectedStep as Camstar.WebPortal.FormsFramework.WebControls.NamedSubentity).Data as NamedSubentityRef;
						getSSPlanDetailsByStep[selectedRowId].MoveOutFromStep.Parent = selectedWorkflow as BaseObjectRef;
						getSSPlanDetailsByStep[selectedRowId].strMoveOutFromStep = (selectedStep.Data as NamedSubentityRef).Name;
						getSSPlanDetailsByStep[selectedRowId].MoveOutFromStepWorkflow = selectedWorkflow as RevisionedObjectRef;
						Page.PortalContext.DataContract.SetValueByName("LotSchedule_ReturnedMoveOutFromStep", null);
						Page.PortalContext.DataContract.SetValueByName("LotSchedule_ReturnedWorkflow", null);
                        bIsSSByStep = true;
					}
					else if (Page.PortalContext.DataContract.GetValueByName("LotSchedule_ReturnedMoveOutToStep") != null)
					{
						selectedStep = Page.PortalContext.DataContract.GetValueByName("LotSchedule_ReturnedMoveOutToStep") as Camstar.WebPortal.FormsFramework.WebControls.NamedSubentity;
						selectedWorkflow = Page.PortalContext.DataContract.GetValueByName("LotSchedule_ReturnedSSWorkflow") as RevisionedObjectRef;
						getSSPlanDetailsByStep[selectedRowId].MoveOutToStep = new NamedSubentityRef();
						getSSPlanDetailsByStep[selectedRowId].MoveOutToStep = (selectedStep as Camstar.WebPortal.FormsFramework.WebControls.NamedSubentity).Data as NamedSubentityRef;
						getSSPlanDetailsByStep[selectedRowId].MoveOutToStep.Parent = selectedWorkflow as BaseObjectRef;
						getSSPlanDetailsByStep[selectedRowId].strMoveOutToStep = (selectedStep.Data as NamedSubentityRef).Name;
						getSSPlanDetailsByStep[selectedRowId].MoveOutToStepWorkflow = selectedWorkflow as RevisionedObjectRef;
						Page.PortalContext.DataContract.SetValueByName("LotSchedule_ReturnedMoveOutToStep", null);
						Page.PortalContext.DataContract.SetValueByName("LotSchedule_ReturnedSSWorkflow", null);
                        bIsSSByStep = true;
					}
					else if (Page.PortalContext.DataContract.GetValueByName("LotSchedule_ReturnedMoveBackFromStep") != null)
					{
						selectedStep = Page.PortalContext.DataContract.GetValueByName("LotSchedule_ReturnedMoveBackFromStep") as Camstar.WebPortal.FormsFramework.WebControls.NamedSubentity;
						selectedWorkflow = Page.PortalContext.DataContract.GetValueByName("LotSchedule_ReturnedSSWorkflow") as RevisionedObjectRef;
						getSSPlanDetailsByStep[selectedRowId].MoveBackFromStep = new NamedSubentityRef();
						getSSPlanDetailsByStep[selectedRowId].MoveBackFromStep = (selectedStep as Camstar.WebPortal.FormsFramework.WebControls.NamedSubentity).Data as NamedSubentityRef;
						getSSPlanDetailsByStep[selectedRowId].MoveBackFromStep.Parent = selectedWorkflow as BaseObjectRef;
						getSSPlanDetailsByStep[selectedRowId].strMoveBackFromStep = (selectedStep.Data as NamedSubentityRef).Name;
						getSSPlanDetailsByStep[selectedRowId].MoveBackFromStepWorkflow = selectedWorkflow as RevisionedObjectRef;
						Page.PortalContext.DataContract.SetValueByName("LotSchedule_ReturnedMoveBackFromStep", null);
						Page.PortalContext.DataContract.SetValueByName("LotSchedule_ReturnedSSWorkflow", null);
                        bIsSSByStep = true;
					}
					else if (Page.PortalContext.DataContract.GetValueByName("LotSchedule_ReturnedMoveBackToStep") != null)
					{
						selectedStep = Page.PortalContext.DataContract.GetValueByName("LotSchedule_ReturnedMoveBackToStep") as Camstar.WebPortal.FormsFramework.WebControls.NamedSubentity;
						selectedWorkflow = Page.PortalContext.DataContract.GetValueByName("LotSchedule_ReturnedWorkflow") as RevisionedObjectRef;
						getSSPlanDetailsByStep[selectedRowId].MoveBackToStep = new NamedSubentityRef();
						getSSPlanDetailsByStep[selectedRowId].MoveBackToStep = (selectedStep as Camstar.WebPortal.FormsFramework.WebControls.NamedSubentity).Data as NamedSubentityRef;
						getSSPlanDetailsByStep[selectedRowId].MoveBackToStep.Parent = selectedWorkflow as BaseObjectRef;
						getSSPlanDetailsByStep[selectedRowId].strMoveBackToStep = (selectedStep.Data as NamedSubentityRef).Name;
						getSSPlanDetailsByStep[selectedRowId].MoveBackToStepWorkflow = selectedWorkflow as RevisionedObjectRef;
						Page.PortalContext.DataContract.SetValueByName("LotSchedule_ReturnedMoveBackToStep", null);
						Page.PortalContext.DataContract.SetValueByName("LotSchedule_ReturnedWorkflow", null);
                        bIsSSByStep = true;
					}

                    _gridSSDetails.ClearData();
                    _gridSSDetails.Data = getSSPlanDetails;
                    _gridSSDetails.OriginalData = getSSPlanDetails;
					_gridSSByStepDetails.ClearData();
					_gridSSByStepDetails.Data = getSSPlanDetailsByStep;
					_gridSSByStepDetails.OriginalData = getSSPlanDetailsByStep;
					//CamstarWebControl.SetRenderToClient(_gridSSByStepDetails);
					Page.PortalContext.DataContract.SetValueByName("LotSchedule_GridRowIdDM", null);
					Page.PortalContext.DataContract.SetValueByName("LotSchedule_StepGridRowIdDM", null);
	
					if(bSetSSTestParams)
						SetSSTestParams(attrVal, attrRev);

					if (bIsSSByStep)
                        Page.SetFocus(_gridSSByStepDetails);
                    else
                        Page.SetFocus(_gridSSDetails);
                }
			}
		}

		public void SetSSTestParams(string sSSName, string sSSRev)
		{

			// get the existing TestParams from the grid
			// check if there is any matching ProcessSpec with the input sSSName/sSSRev
			// if there is nothing matching then proceed with request else do nothing
			JQDataGrid TestParamsDetailsGrid = Page.FindCamstarControl("LotSchedule_TestParamsDetails") as JQDataGrid;
			ScheduleTestParamsDetails[] oExistingParams = (TestParamsDetailsGrid.GridContext as BoundContext).Data as ScheduleTestParamsDetails[];
			bool bGetTestParams = false;

			if (oExistingParams == null)
				bGetTestParams = true;
			else
			{
                // not empty and try to add another Supplementary Spec	
                if (oExistingParams.FirstOrDefault(p => (p.ProcessSpec.Name.ToString().ToUpper() == sSSName.ToUpper() && p.ProcessSpec.Revision == sSSRev)) == null)                
					bGetTestParams = true;
			}
			
			if(bGetTestParams)
			{
				try
				{
					// get the session and user profile
					var fs = FrameworkManagerUtil.GetFrameworkSession();

					// Get the service type of the page
					string sServiceType = Page.PrimaryServiceType;

					// Run proper constructor. We need to be dynamic with the primary service type
					var svcType = WCFObject.CreateObjectType(sServiceType + "Service");
					//create a request object
					var oRequest = WCFObject.CreateObject(sServiceType + "_Request");
					var svcConstructor = svcType.GetConstructor(new Type[] { typeof(UserProfile) });
					var oService = svcConstructor.Invoke(new object[] { fs.CurrentUserProfile });

					//retriving data dynamically for Flexibility of the page.
					var data = CreateServiceData(sServiceType);
					var oServiceData = data as LotSchedule;

					var info = CreateServiceInfo(sServiceType);
					var oServiceInfo = info as LotSchedule_Info;
					(oRequest as Request).Info = oServiceInfo;

					//request of data to be retrived.				
					oServiceData.MfgOrder = (NamedObjectRef)_MfgOrder.Data;
					oServiceData.Product = (RevisionedObjectRef)_Product.Data;
					oServiceData.ProcessSpec = (RevisionedObjectRef)_ProcessSpec.Data;
					oServiceData.SS = new RevisionedObjectRef(sSSName, sSSRev);

					oServiceInfo.scsSSTestParamsDetailsSelection = new ScheduleTestParamsDetails_Info();
					oServiceInfo.scsSSTestParamsDetailsSelection.ProcessSpec = FieldInfoUtil.RequestValue();
					oServiceInfo.scsSSTestParamsDetailsSelection.ProcessType = FieldInfoUtil.RequestValue();
					oServiceInfo.scsSSTestParamsDetailsSelection.Spec = FieldInfoUtil.RequestValue();
					oServiceInfo.scsSSTestParamsDetailsSelection.TestProgramName = FieldInfoUtil.RequestValue();
					oServiceInfo.scsSSTestParamsDetailsSelection.TestProgramMajorRevision = FieldInfoUtil.RequestValue();
					oServiceInfo.scsSSTestParamsDetailsSelection.TestProgramMinorRevision = FieldInfoUtil.RequestValue();
					oServiceInfo.scsSSTestParamsDetailsSelection.TestTemperature = FieldInfoUtil.RequestValue();
					oServiceInfo.scsSSTestParamsDetailsSelection.TestCode = FieldInfoUtil.RequestValue();
					
					// init the result object=
					Result oResult = new Result();

					// execute to request the value
					ResultStatus resultStatus = (oService as IShopFloorBase).GetEnvironment(oServiceData, (oRequest as Request), out oResult);
					if (resultStatus.IsSuccess)
					{
						if ((oResult.Value as SchedulingTxn).scsSSTestParamsDetailsSelection != null)
						{
							// get the existing TestParams from the grid.
							ScheduleTestParamsDetails[] oNewParams = (oResult.Value as SchedulingTxn).scsSSTestParamsDetailsSelection as ScheduleTestParamsDetails[];
							ScheduleTestParamsDetails[] oMergeParams = new ScheduleTestParamsDetails[oExistingParams.Length + oNewParams.Length];
							Array.Copy(oExistingParams, oMergeParams, oExistingParams.Length);
							Array.Copy(oNewParams, 0, oMergeParams, oExistingParams.Length, oNewParams.Length);

							//Binding the Data to the Grid						
							(TestParamsDetailsGrid.GridContext as BoundContext).Data = oMergeParams;
							TestParamsDetailsGrid.BoundContext.LoadData();
							//Rendering the Grid with data
							CamstarWebControl.SetRenderToClient(TestParamsDetailsGrid);
						}

					}
				}
				catch (Exception Ex)
				{
					DisplayMessage(new OM.ResultStatus(Ex.TargetSite.Name + "(): " + Ex.Message, false));
				}
			}			
		} // SetTestParams


        /// <summary>
        /// This event is fired when the submit action button is clicked.
        /// </summary>
        /// <param name="serviceData"></param>
        public override void GetInputData(Service serviceData)
		{
			try
			{
				JQDataGrid grid = Page.FindCamstarControl("LotSchedule_Details") as JQDataGrid;

				List<SchedulingTxnDetails> detailsLots = new List<SchedulingTxnDetails>();
				int totalRows = grid.GridContext.GetTotalRows();
				int totalColumns = grid.Settings.Columns.Count();
				SchedulingTxnDetails details;
				for (int x = 0; x < totalRows; x++)
				{
					details = new SchedulingTxnDetails();
					details.Container = new OM.ContainerRef();
					details.Container.Name = grid.GridContext.GetCell(x, grid.Settings.Columns[0].Name).ToString();
					detailsLots.Add(details);
				}
				(serviceData as LotSchedule).Details = detailsLots.ToArray();
				(serviceData as LotSchedule).Container = new ContainerRef(_ListContainers.DropDownControl.SelectedValue.ToString());
				(serviceData as LotSchedule).PartialLot = new ContainerRef(_ListPartialLot.DropDownControl.SelectedValue.ToString());
				if (_gridSSDetails.Data != null)
				{
					ScheduleSSPlanDetails[] getSSDetails = _gridSSDetails.Data as ScheduleSSPlanDetails[];
					ScheduleSSDetailsEx[] getSSDetailsByStep = _gridSSByStepDetails.Data as ScheduleSSDetailsEx[];

                    int iSSBySpecCount = 0;
                    int iSSByStepCount = 0;
                    if (getSSDetails != null)
                        iSSBySpecCount = getSSDetails.Length;
                    if (getSSDetailsByStep != null)
                        iSSByStepCount = getSSDetailsByStep.Length;

                    (serviceData as LotSchedule).SSDetails = new ScheduleSSDetails[iSSBySpecCount + iSSByStepCount];

					//Submit SS Details
					int index = 0;

                    for (int gridSSBySpecIndex = 0; gridSSBySpecIndex < iSSBySpecCount; gridSSBySpecIndex++, index++)
					{
						(serviceData as LotSchedule).SSDetails[index] = new ScheduleSSDetails();
						(serviceData as LotSchedule).SSDetails[index].SS = getSSDetails[gridSSBySpecIndex].SS;
						(serviceData as LotSchedule).SSDetails[index].MoveOutFromSpec = getSSDetails[gridSSBySpecIndex].MoveOutFromSpec;
						(serviceData as LotSchedule).SSDetails[index].MoveOutToSpec = getSSDetails[gridSSBySpecIndex].MoveOutToSpec;
						(serviceData as LotSchedule).SSDetails[index].MoveBackFromSpec = getSSDetails[gridSSBySpecIndex].MoveBackFromSpec;
						(serviceData as LotSchedule).SSDetails[index].MoveBackToSpec = getSSDetails[gridSSBySpecIndex].MoveBackToSpec;
						(serviceData as LotSchedule).SSDetails[index].IgnoreMainLot = getSSDetails[gridSSBySpecIndex].IgnoreMainLot;
                        (serviceData as LotSchedule).SSDetails[index].RPTBulkDelta = getSSDetails[gridSSBySpecIndex].RPTBulkDelta;
                        (serviceData as LotSchedule).SSDetails[index].RPTUnitDelta = getSSDetails[gridSSBySpecIndex].RPTUnitDelta;
                        (serviceData as LotSchedule).SSDetails[index].UseStep = false;
						if (_gridSSDetails.GridContext.GetCell(gridSSBySpecIndex, "LotId") != null)
							if (_gridSSDetails.GridContext.GetCell(gridSSBySpecIndex, "LotId").ToString() != "")
								(serviceData as LotSchedule).SSDetails[index].LotId = _gridSSDetails.GridContext.GetCell(gridSSBySpecIndex, "LotId").ToString();
					}
                    for (int gridSSByStepIndex = 0; gridSSByStepIndex < iSSByStepCount; gridSSByStepIndex++, index++)
					{
						(serviceData as LotSchedule).SSDetails[index] = new ScheduleSSDetails();
						(serviceData as LotSchedule).SSDetails[index].SS = getSSDetailsByStep[gridSSByStepIndex].SS;
						(serviceData as LotSchedule).SSDetails[index].MoveOutFromStep = getSSDetailsByStep[gridSSByStepIndex].MoveOutFromStep;
						(serviceData as LotSchedule).SSDetails[index].MoveOutToStep = getSSDetailsByStep[gridSSByStepIndex].MoveOutToStep;
						(serviceData as LotSchedule).SSDetails[index].MoveBackFromStep = getSSDetailsByStep[gridSSByStepIndex].MoveBackFromStep;
						(serviceData as LotSchedule).SSDetails[index].MoveBackToStep = getSSDetailsByStep[gridSSByStepIndex].MoveBackToStep;
						(serviceData as LotSchedule).SSDetails[index].IgnoreMainLot = getSSDetailsByStep[gridSSByStepIndex].IgnoreMainLot;
                        (serviceData as LotSchedule).SSDetails[index].RPTBulkDelta = getSSDetailsByStep[gridSSByStepIndex].RPTBulkDelta;
                        (serviceData as LotSchedule).SSDetails[index].RPTUnitDelta = getSSDetailsByStep[gridSSByStepIndex].RPTUnitDelta;
                        (serviceData as LotSchedule).SSDetails[index].UseStep = true;
						if (_gridSSByStepDetails.GridContext.GetCell(gridSSByStepIndex, "LotId") != null)
							if (_gridSSByStepDetails.GridContext.GetCell(gridSSByStepIndex, "LotId").ToString() != "")
								(serviceData as LotSchedule).SSDetails[index].LotId = _gridSSByStepDetails.GridContext.GetCell(gridSSByStepIndex, "LotId").ToString();
					}
				}
				base.GetInputData(serviceData);
			}
			catch (Exception Ex)
			{
				DisplayMessage(new OM.ResultStatus(Ex.TargetSite.Name + "(): " + Ex.Message, false));
			}
		}

		/// <summary>
		/// Delete row event for the grid
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public ResponseData GridContext_RowDeleted(object sender, JQGridEventArgs args)
		{
			var resonse = args.Response as DirectUpdateData;
			return resonse;
		}

		/// <summary>
		/// On click of Add Details
		/// </summary>
		public void AddSSPlanRefClick()
		{
			//If no data do not add.
			if (_SSPlanRef.Data != null)
			{
				try
				{
					//clear the clear message
					Page.StatusBar.ClearMessage();

					// get the session and user profile
					var fs = FrameworkManagerUtil.GetFrameworkSession();

					// Get the service type of the page
					string sServiceType = Page.PrimaryServiceType;

					// Run proper constructor. We need to be dynamic with the primary service type
					var svcType = WCFObject.CreateObjectType(sServiceType + "Service");
					//create a request object
					var oRequest = WCFObject.CreateObject(sServiceType + "_Request");
					var svcConstructor = svcType.GetConstructor(new Type[] { typeof(UserProfile) });
					var oService = svcConstructor.Invoke(new object[] { fs.CurrentUserProfile });

					//retriving data dynamically for Flexibility of the page.
					var data = CreateServiceData(sServiceType);
					var oServiceData = data as LotSchedule;

					var info = CreateServiceInfo(sServiceType);
					var oServiceInfo = info as LotSchedule_Info;

					//request of data to be retrived.	
					oServiceData.SSPlanRef = new NamedObjectRef();
					oServiceData.SSPlanRef.Name = _SSPlanRef.TextEditControl.Text;
					oServiceInfo.SSPlanRefDetails = new ScheduleSSPlanDetails_Info();
					oServiceInfo.SSPlanRefDetails.SS = FieldInfoUtil.RequestValue();
					oServiceInfo.SSPlanRefDetails.MoveOutFromSpec = FieldInfoUtil.RequestValue();
					oServiceInfo.SSPlanRefDetails.MoveOutToSpec = FieldInfoUtil.RequestValue();
					oServiceInfo.SSPlanRefDetails.MoveBackFromSpec = FieldInfoUtil.RequestValue();
					oServiceInfo.SSPlanRefDetails.MoveBackToSpec = FieldInfoUtil.RequestValue();
					oServiceInfo.SSPlanRefDetails.IgnoreMainLot = FieldInfoUtil.RequestValue();
                    oServiceInfo.SSPlanRefDetails.RPTBulkDelta = FieldInfoUtil.RequestValue();
                    oServiceInfo.SSPlanRefDetails.RPTUnitDelta = FieldInfoUtil.RequestValue();

                    (oRequest as Request).Info = oServiceInfo;
					// init the result object
					Result oResult = new Result();

					// execute to request the value
					ResultStatus resultStatus = (oService as IShopFloorBase).GetEnvironment(oServiceData, (oRequest as Request), out oResult);

					if (resultStatus.IsSuccess)
					{
						JQDataGrid SSDetailsGrid = Page.FindCamstarControl("LotSchedule_SSDetails") as JQDataGrid;

						///Existing data is not null
						ScheduleSSPlanDetails[] oExistingList = (SSDetailsGrid.GridContext as BoundContext).Data as ScheduleSSPlanDetails[];

						if (oExistingList != null)
						{
							//New data that is to be added.
							ScheduleSSPlanDetails[] aNewData = new ScheduleSSPlanDetails[(oResult.Value as SchedulingTxn).SSPlanRefDetails.ToArray().Length];

							aNewData = (oResult.Value as SchedulingTxn).SSPlanRefDetails.ToArray();

							///Check if Row is Unique
							bool isUnique = true;
							//for (int x = 0; x < oExistingList.Length; x++)
							//{
							//    if (oExistingList[x].Self.ID.Equals(aNewData[0].Self.ID))
							//    {
							//        isUnique = false;
							//    }
							//}

							//Check to see if unique
							if (isUnique)
							{
								//Merging existing data to new data.
								ScheduleSSPlanDetails[] aMergedArray = new ScheduleSSPlanDetails[oExistingList.Length + aNewData.Length];
								Array.Copy(oExistingList, aMergedArray, oExistingList.Length);
								Array.Copy(aNewData, 0, aMergedArray, oExistingList.Length, aNewData.Length);
								//Binding the Data to the Grid						
								(SSDetailsGrid.GridContext as BoundContext).Data = aMergedArray;
								SSDetailsGrid.BoundContext.LoadData();
								//Rendering the Grid with data
								CamstarWebControl.SetRenderToClient(SSDetailsGrid);
							}
						}
						else
						{
							//Binding the Data to the Grid						
							(SSDetailsGrid.GridContext as BoundContext).Data = (oResult.Value as SchedulingTxn).SSPlanRefDetails.ToArray();
							SSDetailsGrid.BoundContext.LoadData();
							//Rendering the Grid with data
							CamstarWebControl.SetRenderToClient(SSDetailsGrid);
						}
						//populate data triggered by Add button to Test Params Detail grid
						foreach (ScheduleSSPlanDetails ssItem in (oResult.Value as SchedulingTxn).SSPlanRefDetails)
						{
							string SSName1 = "";
							string SSRev1 = "";
							SSName1 = ssItem.SS.Name;
							SSRev1 = ssItem.SS.Revision;
							SetSSTestParams(SSName1, SSRev1);
						}
					}
					else
						DisplayMessage(resultStatus);
				}
				catch (Exception Ex)
				{
					DisplayMessage(new OM.ResultStatus(Ex.TargetSite.Name + "(): " + Ex.Message, false));
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
			base.WebPartCustomAction(sender, e);
			var action = e.Action as Camstar.WebPortal.Personalization.CustomAction;

			if (action != null && action.Parameters == "Reset")
			{
				clearData();
                Page.ShopfloorReset(sender, e);
				OnLoad(null);
			}
            else if (action != null && action.Parameters == "MoveOutFromSpec")
            {
                PopupDataSelection("MoveOutFromSpec");
            }
            else if (action != null && action.Parameters == "MoveOutToSpec")
            {
                PopupDataSelection("MoveOutToSpec");
            }
            else if (action != null && action.Parameters == "MoveBackFromSpec")
            {
                PopupDataSelection("MoveBackFromSpec");
            }
            else if (action != null && action.Parameters == "MoveBackToSpec")
            {
                PopupDataSelection("MoveBackToSpec");
            }
			else if (action != null && action.Parameters == "MoveOutFromStep")
			{
				PopupDataSelection("MoveOutFromStep");
			}
			else if (action != null && action.Parameters == "MoveOutToStep")
			{
				PopupDataSelection("MoveOutToStep");
			}
			else if (action != null && action.Parameters == "MoveBackFromStep")
			{
				PopupDataSelection("MoveBackFromStep");
			}
			else if (action != null && action.Parameters == "MoveBackToStep")
			{
				PopupDataSelection("MoveBackToStep");
			}
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			if (_togglecontainerControl.State == CollapsableState.Collapsed)
				_togglecontainerControl.LabelName = _togglecontainerControl.CollapsedLabelName;
			else
				_togglecontainerControl.LabelName = _togglecontainerControl.ExpandedLabelName;
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
				clearData();
			}
			//OnLoad(null);
		}

		#endregion
	}
	public class ScheduleSSDetailsEx : ScheduleSSDetails
	{
		public RevisionedObjectRef MoveOutFromStepWorkflow { get; set; }
		public RevisionedObjectRef MoveOutToStepWorkflow { get; set; }
		public RevisionedObjectRef MoveBackFromStepWorkflow { get; set; }
		public RevisionedObjectRef MoveBackToStepWorkflow { get; set; }
		public string strMoveOutFromStep { get; set; }
		public string strMoveOutToStep { get; set; }
		public string strMoveBackFromStep { get; set; }
		public string strMoveBackToStep { get; set; }
	}
}



