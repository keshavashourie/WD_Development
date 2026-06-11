/* Copyright 2019 Siemens */
using System;
using System.Collections.Generic;
using System.Linq;
using Camstar.WebPortal.WebPortlets;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.Utilities;
using Camstar.WCF.Services;
using System.Web;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.WCFUtilities;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using System.Web.UI.WebControls;

/// <summary>
/// Summary description for SS_ToolPlan
/// </summary>
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_ToolPlan : MatrixWebPart
    {
        #region Properties

        // TextBoxs
        CWC.TextBox _txtSelectionIdField { get { return Page.FindCamstarControl("SelectionId") as CWC.TextBox; } }  
        CWC.TextBox _txtRecipeDescriptionField { get { return Page.FindCamstarControl("RecipeDescription") as CWC.TextBox; } }
        CWC.TextBox _txtToolPlanNameField { get { return Page.FindCamstarControl("EquipmentSetup_ToolPlanName") as CWC.TextBox; } }
        CWC.TextBox _txtToolPlanDesField { get { return Page.FindCamstarControl("EquipmentSetup_ToolPlanDescription") as CWC.TextBox; } }
        // NamedObject
        CWC.NamedObject _ndoEquipmentField { get { return Page.FindCamstarControl("Equipment") as CWC.NamedObject; } }
        CWC.NamedObject _ndoRequiredToolPlanField { get { return Page.FindCamstarControl("RequiredToolPlan") as CWC.NamedObject; } }
        CWC.NamedObject _ndoProcessTypeField { get { return Page.FindCamstarControl("EquipmentSetup_ToolPlanProcessType") as CWC.NamedObject; } }
        // ContainerList
        CWC.ContainerList _ContainerField { get { return Page.FindCamstarControl("Container") as CWC.ContainerList; } }
        CWC.ContainerList _ctlContainerField { get { return Page.FindCamstarControl("EquipmentSetup_ToolPlanLot") as CWC.ContainerList; } }
        // JQDataGrids
        JQDataGrid _gridToolPlanDetailsFields { get { return Page.FindCamstarControl("EquipmentSetup_ToolPlanDetails") as JQDataGrid; } } 
        CWC.NamedObject _ndoProcessType2Field { get { return Page.FindCamstarControl("ProcessType") as CWC.NamedObject; } }        

        #endregion
        
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            FetchRequiredToolPlan();
            FetchData();
        }

        private void FetchRequiredToolPlan()
        {
            try
            {
                // get the session and user profile
                var fs = FrameworkManagerUtil.GetFrameworkSession();

                WIPMainService oService = new WIPMainService(fs.CurrentUserProfile);
                WIPMain oServiceData = new WIPMain();
                WIPMain_Info oServiceInfo = new WIPMain_Info();
                ResultStatus oResultStatus = new ResultStatus();

                if (_txtSelectionIdField.TextControl.Text != null)
                {
                    if (_ContainerField.Data != null)
                    {
                        oServiceData.Container = new ContainerRef();
                        oServiceData.Container.Name = _ContainerField.Data.ToString();
                    }
                    else
                    {
                        oServiceData.SelectionId = _txtSelectionIdField.Data == null ? null : _txtSelectionIdField.Data.ToString(); 
                    }
                }

                if (_ndoEquipmentField.TextEditControl.Text != null)
                {
                    oServiceData.Equipment = new NamedObjectRef();
                    oServiceData.Equipment.Name = _ndoEquipmentField.TextEditControl.Text;
                }

                if (_ndoProcessType2Field.TextEditControl.Text != null)
                {
                    oServiceData.ProcessType = new NamedObjectRef();
                    oServiceData.ProcessType.Name = _ndoProcessType2Field.TextEditControl.Text;
                }
								
                oServiceInfo.RequiredToolPlan = FieldInfoUtil.RequestValue();

                //Set the request
                WIPMain_Request oServiceRequest = new WIPMain_Request();
                oServiceRequest.Info = oServiceInfo;

                // Set the status
                WIPMain_Result oServiceResult = new WIPMain_Result();

                // execute the request selection values
                ResultStatus objRS = oService.ResolveSelectionId(oServiceData, oServiceRequest, out oServiceResult);
                if (objRS.IsSuccess)
                {
                    if (oServiceInfo.RequiredToolPlan != null)
                    {
                        _ndoRequiredToolPlanField.Data = oServiceResult.Value.RequiredToolPlan;
                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private void FetchData()
        {
            try
            {
                // get the session and user profile
                var fs = FrameworkManagerUtil.GetFrameworkSession();

                string ServiceTypeRequired = "EquipmentSetup";
                ResultStatus oServinceResult = new ResultStatus(null, false);
                var oServiceData = WCFObject.CreateObject(ServiceTypeRequired) as ICreator;
                var oServiceRequest = WCFObject.CreateObject(ServiceTypeRequired + "_Request") as ICreator;
                var oServiceInfo = WCFObject.CreateObject(ServiceTypeRequired + "_Info") as ICreator;
                Result oResponseData = null;
                var oService = new WSDataCreator().CreateService(ServiceTypeRequired, fs.CurrentUserProfile);
                bool bRequestToolPlan = false;

                if (_ndoRequiredToolPlanField.Data != null)
                {
                    _gridToolPlanDetailsFields.ClearData();
                    CamstarWebControl.SetRenderToClient(_gridToolPlanDetailsFields);

                    oServiceData.SetValue("Resource", _ndoEquipmentField.Data as NamedObjectRef);

                    if (_ctlContainerField != null)
                    {
                        if (_ctlContainerField.Data != null)
                        {
                            oServiceData.SetValue("ToolPlanLot", _ctlContainerField.Data as ContainerRef);
                            bRequestToolPlan = true;
                        }
                    }

                    oServiceData.SetValue("ToolPlan", _ndoRequiredToolPlanField.Data as NamedObjectRef);

                    if (bRequestToolPlan)
                    {
                        if (_ndoProcessTypeField.Data != null)
                        { oServiceData.SetValue("ToolPlanProcessType", _ndoProcessTypeField.Data as NamedObjectRef); }

                        oServiceInfo.SetValue("ToolPlanName", new Camstar.WCF.ObjectStack.Info(true));
                        oServiceInfo.SetValue("ToolPlanDescription", new Camstar.WCF.ObjectStack.Info(true));
                        oServiceInfo.SetValue("ToolPlanDetails", new Camstar.WCF.ObjectStack.EquipmentSetupToolPlanItem_Info());
                        oServiceInfo.SetValue("ToolPlanDetails.ItemName", new Camstar.WCF.ObjectStack.Info(true));
                        oServiceInfo.SetValue("ToolPlanDetails.ItemComments", new Camstar.WCF.ObjectStack.Info(true));
                        oServiceInfo.SetValue("ToolPlanDetails.Detail", new Camstar.WCF.ObjectStack.Info(true));
                        oServiceInfo.SetValue("ToolPlanDetails.DisplayName", new Camstar.WCF.ObjectStack.Info(true));
                        oServiceInfo.SetValue("ToolPlanDetails.ToolFamilyQty", new Camstar.WCF.ObjectStack.Info(true));
                    }
                    oServiceRequest.SetValue("Info", oServiceInfo);

                    // Request the data
                    ResultStatus oResultStatus = oService.GetEnvironment(oServiceData as DCObject, oServiceRequest as Request, out oResponseData);
                    if (oResultStatus.IsSuccess)
                    {

                        if (bRequestToolPlan)
                        {
                            _txtToolPlanNameField.Data = (oResponseData as ICreator).GetValue("Value.ToolPlanName");
                            _txtToolPlanDesField.Data = (oResponseData as ICreator).GetValue("Value.ToolPlanDescription");

                            EquipmentSetupToolPlanItem[] objToolPlanDetails = ((oResponseData as ICreator).GetValue("Value.ToolPlanDetails") as EquipmentSetupToolPlanItem[]);

                            // bind the results to the grid for ToolPlan
                            if (objToolPlanDetails != null)
                            {
                                (_gridToolPlanDetailsFields.GridContext as BoundContext).Data = objToolPlanDetails.ToArray();
                                _gridToolPlanDetailsFields.BoundContext.LoadData();
                            }
                            else
                            {
                                _gridToolPlanDetailsFields.ClearData();
                            }
                            CamstarWebControl.SetRenderToClient(_gridToolPlanDetailsFields);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
    }
}



