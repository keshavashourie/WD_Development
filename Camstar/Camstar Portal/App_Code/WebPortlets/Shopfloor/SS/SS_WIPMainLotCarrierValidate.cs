/* Copyright 2019 Siemens */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.WCFUtilities;
using PERS = Camstar.WebPortal.Personalization;

/// <summary>
/// Summary description for SS_WIPMain
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_WIPMainLotCarrierValidate : SS_WIPMainFramework
    {
        protected CWC.TextBox _txtWIPMainTxn { get { return Page.FindCamstarControl("WIPMainTxn") as CWC.TextBox; } }
        protected CWC.TextBox _txtContainerName { get { return Page.FindCamstarControl("ValidateCarrier_Container") as CWC.TextBox; } }
        protected CWC.NamedObject _ndoCarrierName { get { return Page.FindCamstarControl("ValidateCarrier_CarrierName") as CWC.NamedObject; } }
        protected JQDataGrid _gridCarriers { get { return Page.FindCamstarControl("ValidateCarrier_CarriersToValidate") as JQDataGrid ; } }
        protected bool bIsPopup { get { return Page.IsAJAXFloatingFrame; } }
        protected CWC.Button _btnSubmit { get { return Page.FindCamstarControl("WIPBinning_SubmitButton") as CWC.Button; } }
        protected CWC.Button _btnReset { get { return Page.FindCamstarControl("WIPBinning_ResetButton") as CWC.Button; } }
               
        protected override void OnLoad(EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                if (bIsPopup)
                {
                    _btnSubmit.Visible = false;
                    _btnReset.Visible = false;
                    _btnSubmit.Enabled = false;
                    _btnSubmit.Enabled = false;
                }
            }

            if (!bIsPopup)
            {
                string sWIPFlag = GetWIPFlag();
                switch (sWIPFlag)
                {
                    case "5":
                        _txtWIPMainTxn.Data = "Move In";
                        break;

                    case "1":
                        _txtWIPMainTxn.Data = "Track In";
                        break;

                    case "2":
                        _txtWIPMainTxn.Data = "Track Out";
                        break;

                    case "4":
                        _txtWIPMainTxn.Data = "Move Out";
                        break;

                    default: sWIPFlag = ""; break;
                }
            }
        }

        public void CarrierName_DataChanged()
        {
            if (!string.IsNullOrEmpty(_ndoCarrierName.Text))
            {
                string newName = _ndoCarrierName.Text;
                NamedObjectRef[] curRows = (_gridCarriers.GridContext as BoundContext).Data as NamedObjectRef[];

                if (curRows.IsNullOrEmpty() || (!curRows.IsNullOrEmpty()&&!curRows.Any(x => x.Name.Equals(newName))))
                {
                   List<NamedObjectRef> currowslist= new List<NamedObjectRef>();
                   if (!curRows.IsNullOrEmpty())
                   currowslist = curRows.ToList();

                    currowslist.Add( new NamedObjectRef(newName));

                    _gridCarriers.ClearData();
                    (_gridCarriers.GridContext as BoundContext).Data = currowslist.ToArray();
                    _gridCarriers.BoundContext.LoadData();
                    CamstarWebControl.SetRenderToClient(_gridCarriers);
                    _ndoCarrierName.Data = "";
                }
            }        
        }

        public void ContainerName_DataChanged()
        {
            _gridCarriers.ClearData();
        }

        #region Page Events

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
                _gridCarriers.ClearData();
                _ndoCarrierName.Data = "";
            }
            base.WebPartCustomAction(sender, e);
        }

        public override void GetInputData(Service serviceData)
        {
            if (serviceData is ValidateCarrier)
            {
                base.GetInputData(serviceData);

                string sWIPFlag = "";
                if (bIsPopup)
                {
                    string sWIPTxn = _txtWIPMainTxn.Data.ToString();
                    switch (sWIPTxn.ToUpper())
                    {
                        case "MOVE IN":
                            sWIPFlag = "5";
                            break;
                        case "MOVE OUT":
                            sWIPFlag = "4";
                            break;
                        case "TRACK IN":
                            sWIPFlag = "1";
                            break;
                        case "TRACK OUT":
                            sWIPFlag = "2";
                            break;
                    }
                }
                else
                {
                    sWIPFlag = GetWIPFlag();
                }
                
                int TotalCarriers = _gridCarriers.BoundContext.GetTotalRows();
                OM.ValidateCarrier svcData = serviceData as OM.ValidateCarrier;
                //commented line below to fix submitting blank data for the grid
                //svcData.CarriersToValidate = new NamedObjectRef[TotalCarriers];				

                if (!_txtContainerName.IsEmpty)
                {
                    (serviceData as ValidateCarrier).Container = new ContainerRef(_txtContainerName.Data.ToString());
                    (serviceData as ValidateCarrier).WIPMainTxn = new Enumeration<WIPMainTxnTypeEnum, int>(Convert.ToInt32(sWIPFlag));
                }
            }
        }

        public override void PostExecute(ResultStatus status, Service serviceData)
        {
            if (serviceData is ValidateCarrier)
            {
                //base.PostExecute(status, serviceData);
                if (status.IsSuccess)
                {
                    _gridCarriers.ClearData();
                    _ndoCarrierName.Data = "";
                }
                //base.PostExecute(status, serviceData);
            }
        }


        #endregion
    }
}



