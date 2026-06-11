/* Copyright 2019 Siemens */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Collections;

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
using SEMI.AppCode;

/// <summary>
/// Summary description for SS_WIPMainFramework_Infobar
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_WIPMainFramework_Info : SS_WIPMainFramework
    {       
        protected override void OnLoad(EventArgs e)
        {
            
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        public void PopupWIPMessages()
        {
            // get the session and user profile
            var fs = FrameworkManagerUtil.GetFrameworkSession();

            AssemblyMotherLotWIPMainService oService = new AssemblyMotherLotWIPMainService(fs.CurrentUserProfile);
            AssemblyMotherLotWIPMain oServiceData = new AssemblyMotherLotWIPMain();
            AssemblyMotherLotWIPMain_Info oServiceInfo = new AssemblyMotherLotWIPMain_Info();
            AssemblyMotherLotWIPMain_Request oRequest = new AssemblyMotherLotWIPMain_Request();
            AssemblyMotherLotWIPMain_Result oResult = new AssemblyMotherLotWIPMain_Result();

            oServiceData.SelectionId = _txtInfoSelectedContainerId.Data.ToString();
            if (GetWIPFlag() != "")
                oServiceData.WIPFlag = int.Parse(GetWIPFlag());

            oServiceInfo.WIPInstruction = FieldInfoUtil.RequestValue();

            oRequest = new AssemblyMotherLotWIPMain_Request();
            oRequest.Info = oServiceInfo;

            ResultStatus oResultStatus = new ResultStatus();               
            oResultStatus = oService.ResolveSelectionId(oServiceData, oRequest, out oResult);

            if (oResultStatus.IsSuccess)
            {
                if (oResult.Value != null)
                    if (oResult.Value.WIPInstruction != null)
                    {
                        SEMI.AppCode.DataPacket oData = new DataPacket();
                        oData.IsAlertMessageAvailable = true;
                        string[] sMessages = new string[1] { oResult.Value.WIPInstruction.ToString() };
                        oData.AlertMessages = sMessages;

                        _envWIPMainInfoEnvelop.SS_DataPacket = oData;

                        base.PopupWIPMessages();
                    }
            }
        } //


        //-----------------------------------------
        //
        //-----------------------------------------
        public void PopupLastSPCChart()
        {
            SPCTxnData[] oSPCTxnData = GetSPCTxnDataViewState();
            if (oSPCTxnData != null)
                DisplaySPCChart(oSPCTxnData, new ResultStatus("", true));
        }
       
    }
}



