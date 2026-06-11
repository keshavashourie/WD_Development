/* Copyright 2019 Siemens */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Camstar.WebPortal.FormsFramework;
using System.ComponentModel;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WCF.ObjectStack;
using PERS = Camstar.WebPortal.Personalization;

/// <summary>
/// Summary description for DataEnvelop
/// </summary>

namespace SEMI.AppCode
{
    public class DataEnvelopControl : FieldControl, ICustomControl
    {
        private TextBox __txtInitDataName;

        //-----------------------------------------
        //
        //-----------------------------------------
        public DataEnvelopControl()
        {
            
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        public override void ClearData()
        {

        }

        //-----------------------------------------
        //
        //-----------------------------------------        
        [WebProperty()]
        public PERS.CustomControlProperty[] CustomControlProperties { set; get; }

        public string[] GetProperties()
        {
            return null;
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        protected override void CreateChildControls(System.Web.UI.ControlCollection controls)
        {
            __txtInitDataName = new TextBox();
            controls.Add(__txtInitDataName);
        }

        //-----------------------------------------
        //
        //-----------------------------------------        
        protected override void OnPreRender(EventArgs e)
        {            
            base.OnPreRender(e);
        }

        //-----------------------------------------
        //
        //-----------------------------------------   
        public DataPacket SS_DataPacket
        {
            get { return ViewState["SEMI.AppCode.DataEnvelopControl__DataPacket"] as DataPacket; }
            set { ViewState["SEMI.AppCode.DataEnvelopControl__DataPacket"] = value; }
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        public string[] SS_ContainersList
        {
            get { return ViewState["SEMI.AppCode.DataEnvelopControl__ContainersList"] as string[]; }
            set { ViewState["SEMI.AppCode.DataEnvelopControl__ContainersList"] = value; }
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        public string[] SS_WafersList
        {
            get { return ViewState["SEMI.AppCode.DataEnvelopControl__WafersList"] as string[]; }
            set { ViewState["SEMI.AppCode.DataEnvelopControl__WafersList"] = value; }
        }


        //-----------------------------------------
        //
        //-----------------------------------------
        public string[] SS_WIPDataValidValuesList
        {
            get { return ViewState["SEMI.AppCode.DataEnvelopControl__WIPDataValidValuesList"] as string[]; }
            set { ViewState["SEMI.AppCode.DataEnvelopControl__WIPDataValidValuesList"] = value; }
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        public BonusLotDetails[] SS_BonusLotDetails
        {
            get { return ViewState["SEMI.AppCode.DataEnvelopControl__BonusLotDetails"] as BonusLotDetails[]; }
            set { ViewState["SEMI.AppCode.DataEnvelopControl__BonusLotDetails"] = value; }
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        public RejectLotDetails[] SS_RejectLotDetails
        {
            get { return ViewState["SEMI.AppCode.DataEnvelopControl__RejectLotDetails"] as RejectLotDetails[]; }
            set { ViewState["SEMI.AppCode.DataEnvelopControl__RejectLotDetails"] = value; }
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        public WaferMapDetails[] SS_WaferMapDetails
        {
            get { return ViewState["SEMI.AppCode.DataEnvelopControl__WaferMapDetails"] as WaferMapDetails[]; }
            set { ViewState["SEMI.AppCode.DataEnvelopControl__WaferMapDetails"] = value; }
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        public Hashtable SS_Hashtable
        {
            get { return ViewState["SEMI.AppCode.DataEnvelopControl__Hashtable"] as Hashtable; }
            set { ViewState["SEMI.AppCode.DataEnvelopControl__Hashtable"] = value; }
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        public ConsumeMaterialsDetails[] SS_ConsumeMaterialsDetails
        {
            get { return ViewState["SEMI.AppCode.DataEnvelopControl__ConsumeMaterialsDetails"] as ConsumeMaterialsDetails[]; }
            set { ViewState["SEMI.AppCode.DataEnvelopControl__ConsumeMaterialsDetails"] = value; }
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        public ConsumeMaterialsDetailsWafers[] SS_ConsumeMaterialsDetailsWafers
        {
            get { return ViewState["SEMI.AppCode.DataEnvelopControl__ConsumeMaterialsDetailsWafers"] as ConsumeMaterialsDetailsWafers[]; }
            set { ViewState["SEMI.AppCode.DataEnvelopControl__ConsumeMaterialsDetailsWafers"] = value; }
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        public CombineLotWafers[] SS_CombineLotWafers
        {
            get { return ViewState["SEMI.AppCode.DataEnvelopControl__CombineLotWafers"] as CombineLotWafers[]; }
            set { ViewState["SEMI.AppCode.DataEnvelopControl__CombineLotWafers"] = value; }
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        public LotReleaseDetails[] SS_LotReleaseDetails
        {
            get { return ViewState["SEMI.AppCode.DataEnvelopControl__LotReleaseDetails"] as LotReleaseDetails[]; }
            set { ViewState["SEMI.AppCode.DataEnvelopControl__LotReleaseDetails"] = value; }
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        public LotHoldLocations[] SS_LotHoldLocations
        {
            get { return ViewState["SEMI.AppCode.DataEnvelopControl__LotHoldLocations"] as LotHoldLocations[]; }
            set { ViewState["SEMI.AppCode.DataEnvelopControl__LotHoldLocations"] = value; }
        }
    }
}




