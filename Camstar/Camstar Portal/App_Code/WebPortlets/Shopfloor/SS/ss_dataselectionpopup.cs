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

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    /// <summary>
    /// Summary description for SS_DataSelectionPopup
    /// </summary>
    public class DataSelectionPopup : MatrixWebPart
    {

        JQDataGrid _gridWIPDataValidValuesField { get { return Page.FindCamstarControl("WIPDataValidValues") as JQDataGrid; } }
        JQDataGrid _gridSpecsSelectionField { get { return Page.FindCamstarControl("SpecsSelection") as JQDataGrid; } }
        CWC.TextBox _txtKeyField { get { return Page.FindCamstarControl("Key") as CWC.TextBox; } }
        CWC.TextBox _txtCaptionField { get { return Page.FindCamstarControl("Caption") as CWC.TextBox; } }
        CWC.TextBox _txtValueField { get { return Page.FindCamstarControl("Value") as CWC.TextBox; } }
        SEMI.AppCode.DataEnvelopControl _envWIPDataValidValuesList { get { return Page.FindCamstarControl("WIPDataValidValuesList") as SEMI.AppCode.DataEnvelopControl; } }


        #region Constant
        const string const_sKey_WafersDetails = "WafersDetails";
        const string const_sKey_QtyToProcess = "QtyToProcess";
        const string const_sColumn_DataValue = "DataValue";

        const string const_sDCM_Popup_SelectedWafersDetailsDM = "Popup_SelectedWafersDetailsDM";
        const string const_sDCM_Popup_ValueDM = "Popup_ValueDM";
        const string const_sDCM_Popup_ReturnSelectedWafersDM = "Popup_ReturnSelectedWafersDM";
        const string const_sDCM_Popup_ReturnSelectedValueDM = "Popup_ReturnSelectedValueDM";
        const string const_sDCM_Popup_ReturnSelectedSpecDM = "Popup_ReturnSelectedSpecDM";
        const string const_sDCM_Popup_ReturnSelectedSpecRevDM = "Popup_ReturnSelectedSpecRevDM";

        #endregion


        //---------------------------------------
        //
        //---------------------------------------
        protected override void OnLoad(EventArgs e)
        {                       
            base.OnLoad(e);

            string ssKey = null;
            if (Page.PortalContext.DataContract.GetValueByName<string>("Popup_IsSSDM") != null)
                ssKey = Page.PortalContext.DataContract.GetValueByName<string>("Popup_IsSSDM").ToString();
            switch (ssKey)
            {
                case "true":
                    _gridWIPDataValidValuesField.Visible = false;
                    _gridSpecsSelectionField.Visible = true;
                    CollectSelectedSpec();
                    break;
                default:
                    _gridWIPDataValidValuesField.Visible = true;
                    _gridSpecsSelectionField.Visible = false;
                    Page.CollectDataContract();

                    if (!Page.IsPostBack)
                        LoadWIPDataValidValues();
                    else
                        CollectSelectedValues();
                    break;
            }
            
        } // OnLoad


        //-----------------------------------------
        // Collect selected values from the grid
        //-----------------------------------------
        private void LoadWIPDataValidValues()
        {
            try
            {
                //------------------------------------------------------------
                // Set the column caption based on passed parameter
                //------------------------------------------------------------
                JQFieldCollection objFieldCollection = _gridWIPDataValidValuesField.BoundContext.Fields;
                foreach (JQField objField in objFieldCollection)
                {
                    switch (objField.ID)
                    {
                        case "DataValue":
                            objField.LabelText = _txtCaptionField.Data != null ? _txtCaptionField.Data.ToString() : "DataValue";
                            break;
                    }
                }

                if (_envWIPDataValidValuesList.SS_WIPDataValidValuesList != null)
                {
                    //-----------------------------------------
                    // Populate WIP Data valid values
                    //-----------------------------------------
                    string[] sWIPDataValidValues = _envWIPDataValidValuesList.SS_WIPDataValidValuesList;
                    WIPDataSetupDetailsValues[] oWIPData = new WIPDataSetupDetailsValues[sWIPDataValidValues.Count()];
                    int iCtr = 0;
                    foreach (string sValue in sWIPDataValidValues)
                    {
                        oWIPData[iCtr] = new WIPDataSetupDetailsValues();
                        oWIPData[iCtr].DataValue = sValue;
                        iCtr++;
                    }

                    // Set and bind datagrid values
                    (_gridWIPDataValidValuesField.GridContext as BoundContext).Data = oWIPData.ToArray();
                    CamstarWebControl.SetRenderToClient(_gridWIPDataValidValuesField);


                    //-----------------------------------------
                    // Mark all values that are selected
                    //-----------------------------------------
                    WIPLotTxnWafersDetails[] oSelectedWafers = null;
                    string sSelectedQty = null;
                    string sKey = _txtKeyField.Data != null ? _txtKeyField.Data.ToString() : "";

                    if (sKey == const_sKey_WafersDetails)
                    {
                        oSelectedWafers = Page.DataContract.GetValueByName(const_sDCM_Popup_SelectedWafersDetailsDM) as WIPLotTxnWafersDetails[];

                        // set the Datacontract to null
                        Page.DataContract.SetValueByName(const_sDCM_Popup_SelectedWafersDetailsDM, null);
                    }
                    

                    int iTotalSelectedValues = 0;
                    if (oSelectedWafers != null)
                        iTotalSelectedValues = oSelectedWafers.Count();
                    else if (_txtValueField.Data != null)
                        iTotalSelectedValues = 1;


                    if (iTotalSelectedValues != 0)
                    {
                        int iSelectedCtr = 0;
                        for (int iRowIndex = 0; iRowIndex < sWIPDataValidValues.Count(); iRowIndex++)
                        {
                            string sSelectedValue = "";
                            if (sKey == const_sKey_WafersDetails)
                                sSelectedValue = oSelectedWafers[iSelectedCtr].WaferScribeNumber.Value.ToString();
                            else if (sKey == const_sKey_QtyToProcess)
                                sSelectedValue = _txtValueField.Data.ToString();

                            // Compare the selected value against the list displayed, mark as selected if found.
                            if (sWIPDataValidValues[iRowIndex] == sSelectedValue)
                            {
                                _gridWIPDataValidValuesField.Action_SelectRow(iRowIndex.ToString().PadLeft(6, '0'), "select");
                                if (iSelectedCtr < iTotalSelectedValues - 1)
                                    iSelectedCtr++;
                            }//end if
                        }//end for loop
                    }//end if
                
                    // Nullify
                    _envWIPDataValidValuesList.SS_WIPDataValidValuesList = null;
                }
            }
            catch (Exception ex) //Catch errors
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

        //-----------------------------------------
        // Collect selected values from the grid
        //-----------------------------------------
        private void CollectSelectedValues()
        {
            try
            {
                WIPLotTxnWafersDetails[] oWafers = null;
                string sValue = "";

                if (_gridWIPDataValidValuesField.SelectedRowIDs != null)
                {
                    int iCtr = _gridWIPDataValidValuesField.GridContext.SelectedRowIDs.Count;
                    string sKey = _txtKeyField.Data != null ? _txtKeyField.Data.ToString() : "";

                    if (sKey == const_sKey_WafersDetails)
                        oWafers = new WIPLotTxnWafersDetails[iCtr];

                    string[] sSelectedRowIds = _gridWIPDataValidValuesField.GridContext.SelectedRowIDs.ToArray();
                    string sSelectedValue = "";

                    bool bIsFirstData = true;
                    Array.Sort<string>(sSelectedRowIds);

                    for (int i = 0; i < iCtr; i++)
                    {
                        sSelectedValue = ((_gridWIPDataValidValuesField.GridContext as ItemDataContext).GetCell(sSelectedRowIds[i], const_sColumn_DataValue).ToString());

                        if (sKey == const_sKey_WafersDetails)
                        {
                            oWafers[i] = new WIPLotTxnWafersDetails();
                            oWafers[i].WaferScribeNumber = sSelectedValue;
                        }
                        else if (sKey == const_sKey_QtyToProcess)
                        {
                            if (bIsFirstData)
                            {
                                bIsFirstData = false;
                                sValue = sSelectedValue;
                            }
                        }
                    }
                    
                    // Set datacontract value
                    if (sKey == const_sKey_WafersDetails)
                        Page.PortalContext.DataContract.SetValueByName(const_sDCM_Popup_ReturnSelectedWafersDM, oWafers);
                    else if (sKey == const_sKey_QtyToProcess)
                        Page.PortalContext.DataContract.SetValueByName(const_sDCM_Popup_ReturnSelectedValueDM, sValue);
                }
            }
            catch (Exception ex) //Catch errors
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }


        //-----------------------------------------
        // Collect selected spec from the grid
        //-----------------------------------------
        private void CollectSelectedSpec()
        {
            try
            {
                // Set data contract
                if (_gridSpecsSelectionField.SelectedRowID != null)
                {
                    string sSelectedSpec = _gridSpecsSelectionField.GridContext.GetCell(_gridSpecsSelectionField.SelectedRowID, "Name").ToString();
                    Page.PortalContext.DataContract.SetValueByName(const_sDCM_Popup_ReturnSelectedSpecDM, sSelectedSpec);

                    string sSelectedSpecRev = _gridSpecsSelectionField.GridContext.GetCell(_gridSpecsSelectionField.SelectedRowID, "Revision").ToString();
                    Page.PortalContext.DataContract.SetValueByName(const_sDCM_Popup_ReturnSelectedSpecRevDM, sSelectedSpecRev);
                }
            }
            catch (Exception ex) //Catch errors
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }
    }

}



