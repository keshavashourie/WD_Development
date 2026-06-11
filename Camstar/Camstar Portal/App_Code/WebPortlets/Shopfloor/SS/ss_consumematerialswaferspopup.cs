/* Copyright 2019 Siemens */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Camstar.WCF.Services;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.Personalization;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using CGC = Camstar.WebPortal.FormsFramework.WebGridControls;
using OM = Camstar.WCF.ObjectStack;
using SEMI.AppCode;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_ConsumeMaterialsWafersPopup : MatrixWebPart
    {
        protected CWC.TextBox _txtPrimaryServiceType { get { return Page.FindCamstarControl("PrimarySvcType") as CWC.TextBox; } }
        protected CWC.TextBox _txtSelectedRowId { get { return Page.FindCamstarControl("SelectedRowId") as CWC.TextBox; } }
        protected JQDataGrid _gridWafer { get { return Page.FindCamstarControl("ServiceDetails_Wafers") as JQDataGrid; } }
        protected DataEnvelopControl _envWaferMapDetails { get { return Page.FindCamstarControl("WaferMapDetails") as DataEnvelopControl; } }
        protected DataEnvelopControl _envDataCollection { get { return Page.FindCamstarControl("EnvDataCollection") as DataEnvelopControl; } }
        Hashtable htWaferMapDetails = new Hashtable();

        //-----------------------------------------
        // On Wafer Map Details
        //-----------------------------------------
        public virtual void PopupWaferMapDetails(bool EndResponse = false)
        {
            try
            {
                if (Page.DataContract.GetValueByName("CMWafersPopup_GridRowId_DM").ToString() != "" && Convert.ToUInt32(Page.DataContract.GetValueByName("CMWafersPopup_Quantity_DM").ToString()) >= 1)
                {
                    _envDataCollection.SS_ConsumeMaterialsDetailsWafers = _gridWafer.Data as ConsumeMaterialsDetailsWafers[];
                    _txtSelectedRowId.Data = Page.DataContract.GetValueByName("CMWafersPopup_GridRowId_DM").ToString();
                    if (_envDataCollection.SS_ConsumeMaterialsDetailsWafers.Count() > Convert.ToInt32(_txtSelectedRowId.Data.ToString()))
                    {
                        if (_envDataCollection.SS_ConsumeMaterialsDetailsWafers[Convert.ToInt32(_txtSelectedRowId.Data.ToString())].WaferMapDetails != null)
                        {
                            _envWaferMapDetails.SS_WaferMapDetails = _envDataCollection.SS_ConsumeMaterialsDetailsWafers[Convert.ToInt32(_txtSelectedRowId.Data.ToString())].WaferMapDetails;
                            Page.DataContract.SetValueByName("CMWafersPopup_WaferMapDetails_DM", _envDataCollection.SS_ConsumeMaterialsDetailsWafers[Convert.ToInt32(_txtSelectedRowId.Data.ToString())].WaferMapDetails);
                        }
                        else
                        {
                            _envWaferMapDetails.SS_WaferMapDetails = null;
                            Page.DataContract.SetValueByName("CMWafersPopup_WaferMapDetails_DM", null);
                        }
                    }

                    Camstar.WebPortal.Personalization.FloatPageOpenAction objAction = new FloatPageOpenAction();
                    objAction.PageName = "SS_WaferMapDetailsPopupVP";

                    UIComponentDataContractLink[] objLinks = new UIComponentDataContractLink[4];
                    objLinks[0] = new UIComponentDataContractLink();
                    objLinks[0].SourceMember = "CMWafersPopup_PrimaryServiceType_DM";
                    objLinks[0].TargetMember = "Popup_PrimarySvcType_DM";
                    objLinks[1] = new UIComponentDataContractLink();
                    objLinks[1].SourceMember = "CMWafersPopup_Quantity_DM";
                    objLinks[1].TargetMember = "Popup_Quantity_DM";
                    objLinks[2] = new UIComponentDataContractLink();
                    objLinks[2].SourceMember = "CMWafersPopup_WaferMapDetails_DM";
                    objLinks[2].TargetMember = "Popup_WaferMapDetails_DM";
                    objLinks[3] = new UIComponentDataContractLink();
                    objLinks[3].SourceMember = "CMWafersPopup_GridRowId_DM";
                    objLinks[3].TargetMember = "Popup_SelectedItem_DM";

                    UIComponentDataContractReturnLink[] objReturnLinks = new UIComponentDataContractReturnLink[2];
                    objReturnLinks[0] = new UIComponentDataContractReturnLink();
                    objReturnLinks[0].SourceMember = "Popup_WaferMapDetails_DM";
                    objReturnLinks[0].TargetMember = "CMWafersPopup_WaferMapDetails_DM";
                    objReturnLinks[1] = new UIComponentDataContractReturnLink();
                    objReturnLinks[1].SourceMember = "Popup_SelectedItem_DM";
                    objReturnLinks[1].TargetMember = "CMWafersPopup_GridRowId_DM";
                    objAction.DataContractMap = new UIComponentDataContractMap();
                    objAction.DataContractMap.Links = objLinks;
                    objAction.DataContractReturnMap = new UIComponentDataContractReturnMap();
                    objAction.DataContractReturnMap.ReturnLinks = objReturnLinks;
                    objAction.FrameLocation = new UIFloatingPageLocation();
                    objAction.FrameLocation.Width = 850;
                    objAction.FrameLocation.Height = 600;
                    objAction.EndResponse = false;

                    this.Page.ActionDispatcher.ExecuteAction(objAction);
                }
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

        //-----------------------------------------
        // On Popup closed event
        //-----------------------------------------
        public void ClosePopup()
        {
            try
            {
                _envDataCollection.SS_ConsumeMaterialsDetailsWafers = _gridWafer.Data as ConsumeMaterialsDetailsWafers[];
                Page.CollectDataContractByName("CMWafersPopup_ConsumeMaterialsWafers_DM");
                Page.DistributeDataContract();
                Page.CloseFloatingFrameOnSubmit(new ResultStatus());
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

        //-----------------------------------------
        // Copy button function
        //-----------------------------------------
        public void CopyButton()
        {
            try
            {
                if (_gridWafer.SelectedRowID != null)
                {
                    int newIndex = Convert.ToInt32(_gridWafer.SelectedRowID.ToString()) + 1;
                    var newDetails = new List<ConsumeMaterialsDetailsWafers>();
                    if (_gridWafer.TotalRowCount > 0)
                    {
                        newDetails.AddRange(_gridWafer.Data as ConsumeMaterialsDetailsWafers[]);
                    }
                    newDetails.Insert(newIndex, new ConsumeMaterialsDetailsWafers
                    {
                        LotWafersItem = newDetails[newIndex - 1].LotWafersItem,
                        WaferScribeNumber = newDetails[newIndex - 1].WaferScribeNumber,
                        QtyConsumed = newDetails[newIndex - 1].QtyConsumed,
                        QtyRequired = newDetails[newIndex - 1].QtyRequired,
                        FromWaferScribeNumber = "",
                        QtyToConsume = 0
                    });
                    _gridWafer.ClearData();
                    _gridWafer.Data = newDetails.ToArray();
                    _gridWafer.OriginalData = newDetails.ToArray();
                }
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

        //---------------------------------------------------
        // Web part custom action
        //---------------------------------------------------
        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as CustomAction;
            if (action != null)
            {
                switch (action.Parameters)
                {
                    case "Submit":
                        {
                            ClosePopup();
                            break;
                        }
                    case "WaferMapDetails":
                        {
                            PopupWaferMapDetails();
                            break;
                        }
                    case "Copy":
                        {
                            CopyButton();
                            break;
                        }
                }
            }
        }

        //-----------------------------------------
        // On Popup closed event
        //-----------------------------------------
        public void OnPopupClose()
        {
            try
            {
                Page.CollectDataContract();
                if (Page.DataContract.GetValueByName("CMWafersPopup_WaferMapDetails_DM") != null)
                {
                    //Bind the returned data to the data envelope control & grid
                    _envDataCollection.SS_ConsumeMaterialsDetailsWafers[Convert.ToInt32(_txtSelectedRowId.Data.ToString())].WaferMapDetails = Page.DataContract.GetValueByName("CMWafersPopup_WaferMapDetails_DM") as WaferMapDetails[];
                    _gridWafer.ClearData();
                    _gridWafer.Data = _envDataCollection.SS_ConsumeMaterialsDetailsWafers;
                    _gridWafer.OriginalData = _envDataCollection.SS_ConsumeMaterialsDetailsWafers;
                }
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

        //-----------------------------------------
        // On Load event
        //-----------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);
                Page.CollectDataContract();
                if (!Page.IsPostBack)
                {
                    _txtPrimaryServiceType.Data = Page.PrimaryServiceType.ToString();
                    if (Page.DataContract.GetValueByName("CMWafersPopup_ConsumeMaterialsWafers_DM") != null)
                    {
                        //Bind data contract to the data grid
                        _gridWafer.ClearData();
                        _gridWafer.Data = Page.DataContract.GetValueByName("CMWafersPopup_ConsumeMaterialsWafers_DM") as ConsumeMaterialsDetailsWafers[];
                        _gridWafer.OriginalData = Page.DataContract.GetValueByName("CMWafersPopup_ConsumeMaterialsWafers_DM") as ConsumeMaterialsDetailsWafers[];
                        //Bind data contract to the envelope control
                        _envDataCollection.SS_ConsumeMaterialsDetailsWafers = Page.DataContract.GetValueByName("CMWafersPopup_ConsumeMaterialsWafers_DM") as ConsumeMaterialsDetailsWafers[];
                    }
                }
                if (SEMI.AppCode.UIUtility.IsPopupClose(this))
                {
                    OnPopupClose();
                }
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }
    }
}



