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


/// <summary>
/// Summary description for SS_WIPDataValidValuesPopup
/// </summary>
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_WIPDataValidValuesPopup : MatrixWebPart
    {
        #region Properties

        protected CWC.ContainerList _ContainerField { get { return Page.FindCamstarControl("Container") as CWC.ContainerList; } }
        protected CWC.NamedObject _ndoProcessTypeField { get { return Page.FindCamstarControl("ProcessType") as CWC.NamedObject; } }
        protected CWC.TextBox _txtServiceNameField { get { return Page.FindCamstarControl("ServiceName") as CWC.TextBox; } }
        protected CWC.NamedObject _ndoWIPDataNameField { get { return Page.FindCamstarControl("WIPDataName") as CWC.NamedObject; } }
        protected CWC.TextBox _txtWIPDataValueField { get { return Page.FindCamstarControl("WIPDataValue") as CWC.TextBox; } }

        protected CWC.TextBox _txtMinDataValueField { get { return Page.FindCamstarControl("MinDataValue") as CWC.TextBox; } }
        protected CWC.TextBox _txtMaxDataValueField { get { return Page.FindCamstarControl("MaxDataValue") as CWC.TextBox; } }
        protected CWC.TextBox _txtLowerLimitField { get { return Page.FindCamstarControl("LowerLimit") as CWC.TextBox; } }
        protected CWC.TextBox _txtUpperLimitField { get { return Page.FindCamstarControl("UpperLimit") as CWC.TextBox; } }
        protected CWC.TextBox _txtCalFormulaExpressionField { get { return Page.FindCamstarControl("CalFormulaExpression") as CWC.TextBox; } }
        protected JQDataGrid _gridWIPDataValidValuesField { get { return Page.FindCamstarControl("WIPDataValidValues") as JQDataGrid; } }
		protected CWC.TextBox _txtKeyField { get { return Page.FindCamstarControl("Key") as CWC.TextBox; } }
		string SamplingKey;
        protected const string _sWIPDataSetupNameSessionId = "WIPData_ValidValuesPopup_WIPDataSetupName_SessionIdentifier";
        protected const string _sSamplingWIPDataServiceIdentifierSessionId = "WIPData_ValidValuesPopup_IsSamplingWIPDataService_SessionIdentifier";
		protected const string _sSamplingWIPDataContainerSessionId = "WIPData_ValidValuesPopup_Container_SessionIdentifier";
		protected const string _sSamplingWIPDataServiceNameSessionId = "WIPData_ValidValuesPopup_ServiceName_SessionIdentifier";
        protected CWC.NamedObject _ndoEquipmentField { get { return Page.FindCamstarControl("Equipment") as CWC.NamedObject; } }
        #endregion

        #region Page Events

        protected override void OnLoad(EventArgs e)
            {
                base.OnLoad(e);

				if (!Page.IsPostBack)
				{
					//_gridWIPDataValidValuesField.GridContext.RowSelected += new JQGridEventHandler(WIPDataValidValues_RowSelected); 

					Page.CollectDataContract();

					// fix for issue where the WIPDataName DataContract is always blank when the popup is called by SamplingWIPData 
					if (_ndoWIPDataNameField.IsEmpty)
						if (Page.Session[_sWIPDataSetupNameSessionId] != null)
						{
							_ndoWIPDataNameField.Data = new NamedObjectRef(Page.Session[_sWIPDataSetupNameSessionId].ToString());
							Page.Session[_sWIPDataSetupNameSessionId] = null;
						}

					// fix for issue where the SamplingKey DataContract is always blank when the popup is called by SamplingWIPData 
					if (Page.DataContract.GetValueByName<string>("WIPDataValidValues_SamplingWIPData_KeyDM") != null)
						SamplingKey = Page.DataContract.GetValueByName<string>("WIPDataValidValues_SamplingWIPData_KeyDM");
					else
						if (Page.Session[_sSamplingWIPDataServiceIdentifierSessionId] != null)
						{
							SamplingKey = Page.Session[_sSamplingWIPDataServiceIdentifierSessionId].ToString();
							Page.Session[_sSamplingWIPDataServiceIdentifierSessionId] = null;
						}

				//	 fix for issue where the Container DataContract is always blank when the popup is called by SamplingWIPData 
					if (_ContainerField.IsEmpty)
						if (Page.Session[_sSamplingWIPDataContainerSessionId] != null)
						{
							_ContainerField.Data = new ContainerRef(Page.Session[_sSamplingWIPDataContainerSessionId].ToString());
							Page.Session[_sSamplingWIPDataContainerSessionId] = null;
						}

					// fix for issue where the ServiceName DataContract is always blank when the popup is called by SamplingWIPData 
					if (_txtServiceNameField.IsEmpty)
						if (Page.Session[_sSamplingWIPDataServiceNameSessionId] != null)
						{
							_txtServiceNameField.Data = new NamedObjectRef(Page.Session[_sSamplingWIPDataServiceNameSessionId].ToString());
							Page.Session[_sSamplingWIPDataServiceNameSessionId] = null;
						}

					FetchData();
				}

            }

            public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
            {
                base.WebPartCustomAction(sender, e);
                var action = e.Action as Camstar.WebPortal.Personalization.CustomAction;

                if (action != null && action.Parameters == "OK")
                {
                    OKButton_Click();
                    Page.CloseFloatingFrameOnSubmit(new ResultStatus());
                }
            }

        #endregion

        #region Methods

            public void FetchData()
            {
                try
                {
                    // get the session and user profile
                    var fs = FrameworkManagerUtil.GetFrameworkSession();

					// Run proper constructor 					
					
					string sServiceType = SamplingKey != null ? SamplingKey : "WIPData";
					var svcType = WCFObject.CreateObjectType(sServiceType + "Service");
					var svcConstructor = svcType.GetConstructor(new Type[] { typeof(UserProfile) });
					var objSvc = svcConstructor.Invoke(new object[] { fs.CurrentUserProfile });

					//WIPDataService objSvc = new WIPDataService(fs.CurrentUserProfile);

                   // WIPData objSvcData = new WIPData();
					//SamplingWIPData SamplingobjSvcData = new SamplingWIPData();
					var objSvcData = CreateServiceData(sServiceType);

					if (SamplingKey == null)
					{
						if (!string.IsNullOrEmpty(_ContainerField.TextEditControl.Text)) { (objSvcData as WIPData).Container = new ContainerRef(_ContainerField.TextEditControl.Text); }
						if (!string.IsNullOrEmpty(_ndoProcessTypeField.TextEditControl.Text)) { (objSvcData as WIPData).ProcessType = new NamedObjectRef(_ndoProcessTypeField.TextEditControl.Text); }
						if (!string.IsNullOrEmpty(_txtServiceNameField.TextControl.Text)) { (objSvcData as WIPData).ServiceName = _txtServiceNameField.TextControl.Text; }
                        if (!string.IsNullOrEmpty(_ndoEquipmentField.TextEditControl.Text)) { (objSvcData as WIPData).Equipment = new NamedObjectRef(_ndoEquipmentField.TextEditControl.Text); }
                        (objSvcData as WIPData).WIPDataName = new NamedObjectRef(_ndoWIPDataNameField.TextEditControl.Text);
					}
					else
					{
						if (!string.IsNullOrEmpty(_ContainerField.TextEditControl.Text)) { (objSvcData as SamplingWIPData).Container = new ContainerRef(_ContainerField.TextEditControl.Text); }
						if (!string.IsNullOrEmpty(_ndoProcessTypeField.TextEditControl.Text)) { (objSvcData as SamplingWIPData).ProcessType = new NamedObjectRef(_ndoProcessTypeField.TextEditControl.Text); }
						if (!string.IsNullOrEmpty(_txtServiceNameField.TextControl.Text)) { (objSvcData as SamplingWIPData).ServiceName = _txtServiceNameField.TextControl.Text; }
						(objSvcData as SamplingWIPData).WIPDataName = new NamedObjectRef(_ndoWIPDataNameField.TextEditControl.Text);
					}

                    // prepare the service info
                   // WIPData_Info objSvcInfo = new WIPData_Info();
					var objSvcInfo = CreateServiceInfo(sServiceType) ;
					if (SamplingKey == null)
					{						
						(objSvcInfo as WIPData_Info).WIPDataValidValues = new WIPDataSetupDetailsValues_Info() { DataValue = FieldInfoUtil.RequestValue() };
						(objSvcInfo as WIPData_Info).WIPDataLowerLimit = FieldInfoUtil.RequestValue();
						(objSvcInfo as WIPData_Info).WIPDataUpperLimit = FieldInfoUtil.RequestValue();
						(objSvcInfo as WIPData_Info).WIPDataMinDataValue = FieldInfoUtil.RequestValue();
						(objSvcInfo as WIPData_Info).WIPDataMaxDataValue = FieldInfoUtil.RequestValue();
						(objSvcInfo as WIPData_Info).WIPDataCalFormulaExpression = FieldInfoUtil.RequestValue();
						(objSvcInfo as WIPData_Info).WIPDataIsCalculatedData = FieldInfoUtil.RequestValue();
					}
					else
					{
						(objSvcInfo as SamplingWIPData_Info).WIPDataValidValues = new WIPDataSetupDetailsValues_Info() { DataValue = FieldInfoUtil.RequestValue() };
						(objSvcInfo as SamplingWIPData_Info).WIPDataLowerLimit = FieldInfoUtil.RequestValue();
						(objSvcInfo as SamplingWIPData_Info).WIPDataUpperLimit = FieldInfoUtil.RequestValue();
						(objSvcInfo as SamplingWIPData_Info).WIPDataMinDataValue = FieldInfoUtil.RequestValue();
						(objSvcInfo as SamplingWIPData_Info).WIPDataMaxDataValue = FieldInfoUtil.RequestValue();
						(objSvcInfo as SamplingWIPData_Info).WIPDataCalFormulaExpression = FieldInfoUtil.RequestValue();
						(objSvcInfo as SamplingWIPData_Info).WIPDataIsCalculatedData = FieldInfoUtil.RequestValue();
					}

                    // init the result object
                    WIPData_Result objResult = new WIPData_Result();
					SamplingWIPData_Result samplingObjResult = new SamplingWIPData_Result(); 

					//SamplingWIPData_Result;
					//var objResult = WCFObject.CreateObjectType(sServiceType + "Result");
                    // execute to request the value
					ResultStatus resultStatus = new ResultStatus();
					if (SamplingKey == null)
						resultStatus = (objSvc as WIPDataService).WIPData_RequestValidValues(objSvcData as WIPData, new WIPData_Request { Info = objSvcInfo as WIPData_Info }, out objResult);	
					else
						resultStatus = (objSvc as SamplingWIPDataService).WIPData_RequestValidValues(objSvcData as SamplingWIPData, new SamplingWIPData_Request { Info = objSvcInfo as SamplingWIPData_Info }, out samplingObjResult);
					
					var exeMethod = svcType.GetType().GetMethods().FirstOrDefault(m => m.Name == "WIPData_RequestValidValues" && m.GetParameters().Count() == 3);
					
					if (SamplingKey == null)
					{
						var parms = new object[] { objSvcData, new WIPData_Request { Info = (objSvcInfo as WIPData_Info) }, objResult };
						if (exeMethod != null)
							resultStatus = exeMethod.Invoke(svcType, parms) as OM.ResultStatus;
					}
					else
					{
						var parms = new object[] { objSvcData, new SamplingWIPData_Request { Info = (objSvcInfo as SamplingWIPData_Info) }, objResult };
						if (exeMethod != null)
							resultStatus = exeMethod.Invoke(svcType, parms) as OM.ResultStatus;
					}

                    if (resultStatus.IsSuccess)
                    {
						if (SamplingKey == null)
						{
							// display the data
							if (objResult.Value.WIPDataLowerLimit != null) { _txtLowerLimitField.Data = objResult.Value.WIPDataLowerLimit.ToString(); }
							if (objResult.Value.WIPDataUpperLimit != null) { _txtUpperLimitField.Data = objResult.Value.WIPDataUpperLimit.ToString(); }
							if (objResult.Value.WIPDataMinDataValue != null) { _txtMinDataValueField.Data = objResult.Value.WIPDataMinDataValue.ToString(); }
							if (objResult.Value.WIPDataMaxDataValue != null) { _txtMaxDataValueField.Data = objResult.Value.WIPDataMaxDataValue.ToString(); }
							if (objResult.Value.WIPDataCalFormulaExpression != null) { _txtCalFormulaExpressionField.Data = objResult.Value.WIPDataCalFormulaExpression.ToString(); }

							if (objResult.Value.WIPDataValidValues != null)
							{
								List<WIPDataSetupDetailsValues> objDetails = new List<WIPDataSetupDetailsValues>();
								foreach (WIPDataSetupDetailsValues objData in objResult.Value.WIPDataValidValues)
								{
									WIPDataSetupDetailsValues objNewRow = new WIPDataSetupDetailsValues();
									objNewRow.DataValue = objData.DataValue;
									objDetails.Add(objNewRow);
								}
								if (objDetails.Count > 0)
								{
									//assign to the WIPDataSetupDetailsValues to the grid
									_gridWIPDataValidValuesField.Data = objDetails.ToArray();
									CamstarWebControl.SetRenderToClient(_gridWIPDataValidValuesField);

									//set selected row matched with the WIP data value and valid values
									if (!string.IsNullOrEmpty(_txtWIPDataValueField.TextControl.Text))
									{
										int intRowId = objDetails.ToList().FindIndex(p => p.DataValue == _txtWIPDataValueField.TextControl.Text);
										if (intRowId >= 0) { _gridWIPDataValidValuesField.Action_SelectRow(intRowId.ToString().PadLeft(6, '0'), "select"); }
									}
								}
							}

							//Disable grid selection should is calculated data
							if (objResult.Value.WIPDataIsCalculatedData == true)
							{
								_gridWIPDataValidValuesField.GridContext.RowSelectionMode = JQGridSelectionMode.Disable;
							}
						}
						else //for SampleWIPData
						{
							if (samplingObjResult.Value.WIPDataLowerLimit != null) { _txtLowerLimitField.Data = samplingObjResult.Value.WIPDataLowerLimit.ToString(); }
							if (samplingObjResult.Value.WIPDataUpperLimit != null) { _txtUpperLimitField.Data = samplingObjResult.Value.WIPDataUpperLimit.ToString(); }
							if (samplingObjResult.Value.WIPDataMinDataValue != null) { _txtMinDataValueField.Data = samplingObjResult.Value.WIPDataMinDataValue.ToString(); }
							if (samplingObjResult.Value.WIPDataMaxDataValue != null) { _txtMaxDataValueField.Data = samplingObjResult.Value.WIPDataMaxDataValue.ToString(); }
							if (samplingObjResult.Value.WIPDataCalFormulaExpression != null) { _txtCalFormulaExpressionField.Data = samplingObjResult.Value.WIPDataCalFormulaExpression.ToString(); }

							if (samplingObjResult.Value.WIPDataValidValues != null)
							{
								List<WIPDataSetupDetailsValues> objDetails = new List<WIPDataSetupDetailsValues>();
								foreach (WIPDataSetupDetailsValues objData in samplingObjResult.Value.WIPDataValidValues)
								{
									WIPDataSetupDetailsValues objNewRow = new WIPDataSetupDetailsValues();
									objNewRow.DataValue = objData.DataValue;
									objDetails.Add(objNewRow);
								}
								if (objDetails.Count > 0)
								{
									//assign to the WIPDataSetupDetailsValues to the grid
									_gridWIPDataValidValuesField.Data = objDetails.ToArray();
									CamstarWebControl.SetRenderToClient(_gridWIPDataValidValuesField);

									//set selected row matched with the WIP data value and valid values
									if (!string.IsNullOrEmpty(_txtWIPDataValueField.TextControl.Text))
									{
										int intRowId = objDetails.ToList().FindIndex(p => p.DataValue == _txtWIPDataValueField.TextControl.Text);
										if (intRowId >= 0) { _gridWIPDataValidValuesField.Action_SelectRow(intRowId.ToString().PadLeft(6, '0'), "select"); }
									}
								}
							}

							//Disable grid selection should is calculated data
							if (samplingObjResult.Value.WIPDataIsCalculatedData == true)
							{
								_gridWIPDataValidValuesField.GridContext.RowSelectionMode = JQGridSelectionMode.Disable;
							}
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

            public void OKButton_Click()
            {
                try
                {
                    if (_gridWIPDataValidValuesField.Data != null && _gridWIPDataValidValuesField.SelectedRowIDs.Length>0)
                    {
                        string[] arrSelectedRowIds = (string[])_gridWIPDataValidValuesField.SelectedRowIDs;
                        Array.Sort(arrSelectedRowIds);
                        _txtWIPDataValueField.Data = _gridWIPDataValidValuesField.GridContext.GetCell(arrSelectedRowIds[0], "DataValue");
                    }
                }
                catch (Exception Ex) //Catch errors
                {
                    DisplayMessage(new OM.ResultStatus(Ex.TargetSite.Name + "(): " + Ex.Message, false));
                }
            }

            //ResponseData WIPDataValidValues_RowSelected(object sender, JQGridEventArgs e)
            //{
            //    _gridWIPDataValidValuesField.Action_SelectRow(null, "deselect");
            //    return e.Response;
            //}

        #endregion

    }
}



