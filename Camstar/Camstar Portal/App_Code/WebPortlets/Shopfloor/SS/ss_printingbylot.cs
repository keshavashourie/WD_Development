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
/// Summary description for SS_PrintingByLot
/// </summary>
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_PrintingByLot: scsShopfloorBase
    {
        protected CWC.TextBox _txtSelectionId { get { return Page.FindCamstarControl("PrintingByLot_SelectionId") as CWC.TextBox; } }
        protected JQDataGrid _gridContainer { get { return Page.FindCamstarControl("PrintingByLot_ContainerGrid") as JQDataGrid; } }
        protected CWC.TextBox _txtComputerName { get { return Page.FindCamstarControl("PrintingByLot_ComputerName") as CWC.TextBox; } }
        protected CWC.RevisionedObject _rdoSpec { get { return Page.FindCamstarControl("PrintingByLot_Spec") as CWC.RevisionedObject; } }
        protected SEMI.AppCode.DataEnvelopControl _envSelectedLots { get { return Page.FindCamstarControl("PrintingByLot_DataEnvelop") as SEMI.AppCode.DataEnvelopControl; } }
        protected ContainerListGrid _contContainer { get { return Page.FindCamstarControl("PrintingByLot_Container") as ContainerListGrid; } }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _txtSelectionId.DataChanged += new EventHandler(_txtSelectionId_DataChanged);
            _txtComputerName.TextControl.Text = SEMI.AppCode.UIUtility.GetComputerName(this);

            if (Page.IsPostBack)
                // Check if it is a pop up close, get the return result
                if (SEMI.AppCode.UIUtility.IsPopupClose(this))
                {
                    if (_envSelectedLots != null)
                        // manually initialize the containers list data contract since it is not triggered when we do a manual popup close
                        if (Page.DataContract.GetValueByName("PrintingByLot_DataEnvelopDM") != null)
                            _envSelectedLots.SS_ContainersList = Page.DataContract.GetValueByName("PrintingByLot_DataEnvelopDM") as string[];

                    if (_envSelectedLots.SS_ContainersList != null)
                    {
                        string[] sContainers;
                        sContainers = _envSelectedLots.SS_ContainersList;

                        if (sContainers.Length > 0)
                        {
                            _txtSelectionId.Data = sContainers[0].ToString();
                            FetchPrintingByLotDetails();
                        }
                        //nullify the containers list
                        _envSelectedLots.SS_ContainersList = null;
                    }
                }
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        void _txtSelectionId_DataChanged(object sender, EventArgs e)
        {
            FetchPrintingByLotDetails();
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        public void FetchPrintingByLotDetails()
        {
            try
            {
                var fs = FrameworkManagerUtil.GetFrameworkSession();
                PrintingByLot oServiceData = new PrintingByLot();
                PrintingByLot_Info oServiceInfo = new PrintingByLot_Info();
                PrintingByLot_Request oRequest = new PrintingByLot_Request();
                PrintingByLot_Result oResult = new PrintingByLot_Result();
                PrintingByLotService oService = new PrintingByLotService(fs.CurrentUserProfile);
                ResultStatus oResultStatus = new ResultStatus();

                oServiceData.SelectionId = _txtSelectionId.Data.ToString();
                oServiceInfo.SelectionContainer = FieldInfoUtil.RequestValue();
                oServiceInfo.Spec = FieldInfoUtil.RequestValue();

                oRequest.Info = oServiceInfo;

                oResultStatus = oService.ResolveSelectionId(oServiceData, oRequest, out oResult);
                if (oResultStatus.IsSuccess)
                {
                    if (oResult.Value.SelectionContainer != null)
                    {
                        _contContainer.Data = oResult.Value.SelectionContainer;
                        _rdoSpec.Data = oResult.Value.Spec;
                        JQDataGrid _gridContainerTemp = Page.FindCamstarControl("PrintingByLot_ContainerGrid") as JQDataGrid;
                        SEMI.AppCode.UIUtility.GetLotQuerySelection(this, "PrintingByLot", _contContainer.Data.ToString(), true, ref _gridContainerTemp, _gridContainerTemp.ID.ToString(), true);                        
                    }
                    else
                    {
                        _contContainer.ClearData();
                        _gridContainer.ClearData();
                        _txtSelectionId.ClearData();
                    }                  
                }
                else
                {
                    DisplayMessage(oResultStatus);
                }
            }
            
            catch (Exception ex)
            {
                DisplayMessage(new ResultStatus(ex.Message.ToString(), false));
            }
        } // FetchPrintingByLotDetails


        //---------------------------------------------------
        //
        //---------------------------------------------------
        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as Camstar.WebPortal.Personalization.CustomAction;

            if (action != null && action.Parameters == "Reset")
            {
                Page.ClearValues();                
                _gridContainer.ClearData();
                _txtSelectionId.Focus();
            }
        }

    }
}



