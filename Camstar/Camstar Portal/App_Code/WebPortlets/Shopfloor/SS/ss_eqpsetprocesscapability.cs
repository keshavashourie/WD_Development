/* Copyright 2019 Siemens */
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

/// <summary>
/// Summary description for SS_EqpSetProcessCapability
/// </summary>
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_EqpSetProcessCapability : MatrixWebPart
    {
        #region Properties

        // TextBoxs
        CWC.NamedObject _txtEquipmentField { get { return Page.FindCamstarControl("EqpSetProcessCapability_Resource") as CWC.NamedObject; } }
        CWC.NamedObject _txtEmployeeField { get { return Page.FindCamstarControl("EqpSetProcessCapability_Employee") as CWC.NamedObject; } }
        CWC.TextBox _txtCommentsField { get { return Page.FindCamstarControl("EqpSetProcessCapability_Comments") as CWC.TextBox; } }

        // JQDataGrids
        JQDataGrid _gridEqpProcessCapabilityFields { get { return Page.FindCamstarControl("EqpSetProcessCapability_Details") as JQDataGrid; } }

        #endregion

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }

        public void EquipmentField_DataChanged(object sender, EventArgs e)
        {
            try
            {
				//get the session and user profile
                var fs = FrameworkManagerUtil.GetFrameworkSession();

                EqpSetProcessCapabilityService oService = new EqpSetProcessCapabilityService(fs.CurrentUserProfile);
                EqpSetProcessCapability oServiceData = new EqpSetProcessCapability();
                EqpSetProcessCapability_Info oServiceInfo = new EqpSetProcessCapability_Info();
                EqpSetProcessCapability_Request oRequest = new EqpSetProcessCapability_Request();
                EqpSetProcessCapability_Result oResult = new EqpSetProcessCapability_Result();
                ResultStatus oResultStatus = new ResultStatus();
				
				//Clear the Grid before retrieve
				_gridEqpProcessCapabilityFields.ClearData();
				_gridEqpProcessCapabilityFields.OriginalData = null;
				_gridEqpProcessCapabilityFields.GridContext.CurrentPage = 1;
				
				//Clear the status message
				Page.StatusBar.ClearMessage();
				
                if (_txtEquipmentField.Data != null)
                {
                    //Prepare the request
                    oServiceData.Resource = new NamedObjectRef();
                    oServiceData.Resource.Name = _txtEquipmentField.Data.ToString();
                    oServiceInfo.DetailsSelection = new EqpProcessCapabilityDetails_Info();
                    oServiceInfo.DetailsSelection.ProcessCapability = FieldInfoUtil.RequestValue();
                    oServiceInfo.DetailsSelection.ActivationStatus = FieldInfoUtil.RequestValue();
                    oServiceInfo.DetailsSelection.LastUpdateBy = FieldInfoUtil.RequestValue();
                    oServiceInfo.DetailsSelection.Availability = FieldInfoUtil.RequestValue();

                    oRequest.Info = oServiceInfo;

                    oResultStatus = oService.GetEnvironment(oServiceData, oRequest, out oResult);

                    if (oResultStatus.IsSuccess)
                    {						
                        if (oResult.Value.DetailsSelection != null)
                        {
                            // Set the result object
                            var oResponseData = oResult.Value as EqpSetProcessCapability;

                            if (oResponseData.DetailsSelection != null)
                            {
                                foreach (EqpProcessCapabilityDetails processCapabilityList in oResponseData.DetailsSelection)
                                {
                                    //int iNewRowCount = _gridEqpProcessCapabilityFields.BoundContext.GetTotalRows();
                                    //(_gridEqpProcessCapabilityFields.GridContext as ItemDataContext).MakeAutoRowId(iNewRowCount);
                                    //string id = (_gridEqpProcessCapabilityFields.GridContext as ItemDataContext).AddNewRow(iNewRowCount.ToString());
                                    //object EqpProcessCapabilityDetailsItem = ((_gridEqpProcessCapabilityFields.GridContext as ItemDataContext).Data as Array).GetValue(iNewRowCount);
                                    //(EqpProcessCapabilityDetailsItem as EqpProcessCapabilityDetails).ProcessCapability = processCapabilityList.ProcessCapability;
                                    //(EqpProcessCapabilityDetailsItem as EqpProcessCapabilityDetails).ActivationStatus = processCapabilityList.ActivationStatus;
                                    //(EqpProcessCapabilityDetailsItem as EqpProcessCapabilityDetails).LastUpdateBy = processCapabilityList.LastUpdateBy;
                                    //(EqpProcessCapabilityDetailsItem as EqpProcessCapabilityDetails).Availability = processCapabilityList.Availability;

                                    //_gridEqpProcessCapabilityFields.GridContext.AdjustCurrentPage(id);
                                    //CamstarWebControl.SetRenderToClient(_gridEqpProcessCapabilityFields);
                                    ItemListGrid_AddNewRow(processCapabilityList.ProcessCapability, (bool) processCapabilityList.ActivationStatus, processCapabilityList.LastUpdateBy, (bool)processCapabilityList.Availability);
                                }
                            }

                            ////bind to the regular grid
                            //_gridEqpProcessCapabilityFields.Data = oResult.Value.DetailsSelection;
                            //CamstarWebControl.SetRenderToClient(_gridEqpProcessCapabilityFields);
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
               DisplayMessage(new ResultStatus("EquipmentField_DataChanged::" + ex.Message.ToString(), false));
            }
        }

        public void ItemListGrid_AddNewRow(NamedObjectRef ProcessCapability, bool ActivationStatus, NamedObjectRef LastUpdateBy, bool Availability)
        {
            try
            {
                JQDataGrid _gridDetails = _gridEqpProcessCapabilityFields;
                EqpProcessCapabilityDetails[] oNewDetail = new EqpProcessCapabilityDetails[1];
                oNewDetail[0] = new EqpProcessCapabilityDetails();
                oNewDetail[0].ProcessCapability = ProcessCapability;
                oNewDetail[0].ActivationStatus = ActivationStatus;
                oNewDetail[0].LastUpdateBy = LastUpdateBy;
                oNewDetail[0].Availability = Availability;
                EqpProcessCapabilityDetails[] oExisting = (_gridDetails.GridContext as BoundContext).Data as EqpProcessCapabilityDetails[];
                if (oExisting != null)
                {
                    bool isUnique = true;
                    for (int i = 0; i < oExisting.Length; i++)
                    {
                        if (oExisting[i].ProcessCapability.Equals(oNewDetail[0].ProcessCapability))
                        {
                            isUnique = false;
                        }
                    }
                    if (isUnique)
                    {
                        EqpProcessCapabilityDetails[] oMerged = new EqpProcessCapabilityDetails[oExisting.Length + 1];
                        Array.Copy(oExisting, oMerged, oExisting.Length);
                        Array.Copy(oNewDetail, 0, oMerged, oExisting.Length, 1);
                        (_gridDetails.GridContext as BoundContext).Data = oMerged.ToArray();
                    }
                }
                else
                {
                    (_gridDetails.GridContext as BoundContext).Data = oNewDetail.ToArray();
                }
                _gridDetails.BoundContext.LoadData();
                CamstarWebControl.SetRenderToClient(_gridDetails);
            }
            catch (Exception ex)
            { }
        }

    }
}



