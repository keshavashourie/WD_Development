// Copyright Siemens 2023  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Camstar.WebPortal.WebPortlets;
using CamstarPortal.WebControls;
using Camstar.WCF.Services;
using Camstar.WCF.ObjectStack;
using OM = Camstar.WCF.ObjectStack;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{

    public class CollectResourceData : MatrixWebPart
    {
        #region Properties

        protected virtual CWC.RevisionedObject DataCollectionDef
        {
            get
            {
                return Page.FindCamstarControl("DataCollectionDef") as CWC.RevisionedObject;
            }
        }

        protected virtual ShopFloorDCControl DataCollection
        {
            get
            {
                return Page.FindCamstarControl("DataCollection") as ShopFloorDCControl;
            }
        }

        #endregion

        #region Methods

        public override void DisplayValues(WCF.ObjectStack.Service serviceData)
        {

            ShopFloorDCControl dataCollection = DataCollection;
            ShopFloor data = (ShopFloor)serviceData;
            dataCollection.DisplayValues(data);
            base.DisplayValues(serviceData);

        }

        public override void RequestValues(Info serviceInfo, Service serviceData)
        {
            base.RequestValues(serviceInfo, serviceData);
            ShopFloor_Info info = (ShopFloor_Info)serviceInfo;
            Camstar.WCF.ObjectStack.CollectResourceData data = (Camstar.WCF.ObjectStack.CollectResourceData)serviceData;
            if (dataPointsRequested)
                DataCollection.RequestValues(data, info);
            info.DataCollectionDef = new OM.Info(true);

            data.DataCollectionDef = DataCollectionDef.Data as RevisionedObjectRef;
        }

        public override void GetInputData(Service serviceData)
        {
            base.GetInputData(serviceData);
            DataPointSummary[] dataPointSummary = DataCollection.GetDataPointSummary();
            if (dataPointSummary != null && dataPointSummary.Length > 0)
                ((ShopFloor)serviceData).ParametricData = dataPointSummary[0];
        }

        public override void ClearValues(Service serviceData)
        {
            base.ClearValues(serviceData);
            DataCollection.Clean();
            DataCollection.IterationCount = 1;
        }

        public virtual void SetDataCollection(object sender, EventArgs arg)
        {
            dataPointsRequested = true;
            Service.LoadServiceValues(PrimaryServiceType, "GetDataPoints");//loaded DisplayValues method is called
            dataPointsRequested = false;
        }

        #endregion

        private bool dataPointsRequested = false;
    }
}
