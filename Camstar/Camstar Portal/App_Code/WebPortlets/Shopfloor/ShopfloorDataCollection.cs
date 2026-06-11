// Copyright Siemens 2025 
using System;
using System.Linq;

using CamstarPortal.WebControls;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.Personalization;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework;
using System.Web;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
namespace Camstar.WebPortal.WebPortlets
{
    /// <summary>
    /// Summary description for ParametricDataControl
    /// </summary>
    public class ShopfloorDataCollection : MatrixWebPart
    {

        protected virtual ContainerListGrid CurrentContainer
        {
            get
            {
                return Page.FindCamstarControl("HiddenSelectedContainer") as ContainerListGrid;
            }
        }

        protected virtual string LastContainerName
        {
            get { return ViewState["LastContainerName"] as string; }
            set { ViewState["LastContainerName"] = value; }
        }

        public ShopfloorDataCollection()
        {
            Title = "Parametric Data";
        }
        public override void DisplayValues(Service serviceData)
        {
            var data = serviceData as ShopFloor;
            if (serviceData is ContainerTxn && Page.ProcessingContext[ProcessingFlagType.RequestOnSubmit] != true)
            {
                if (Container != null)
                    (data as ContainerTxn).Container = Container.Data as ContainerRef;
                else if (HiddenSelectedContainerTextBox != null)
                    (data as ContainerTxn).Container = new ContainerRef(HiddenSelectedContainerTextBox.Data as string);
                else if (HiddenSelectedContainerContainerList != null)
                    (data as ContainerTxn).Container = HiddenSelectedContainerContainerList.Data as ContainerRef;
                else if (ContainerName != null)
                    (data as ContainerTxn).Container = new ContainerRef(ContainerName.Data as string);
            }
            var dataPoints = data.ParametricData as DataPointSummary;
            if (dataPoints != null && dataPoints.DataPointDetails != null || data.WebPart != null)
                ParamDataControl.DisplayValues(data);
            
            if (_ContainerChanged)
                CollectionDef.Visible = (data.HasDataCollection != null && data.HasDataCollection.Value);
            //make field readonly if only one DataCollectionDef is present
            CollectionDef.ReadOnly = data.DataCollectionDef != null;
            base.DisplayValues(serviceData);
        }

        public override void RequestValues(Info serviceInfo, Service serviceData)
        {                
            if (serviceData is ShopFloor)
            {
                if (Container != null)
                {
                    if (serviceData is ContainerTxn && !Container.IsEmpty)
                        (serviceData as ContainerTxn).Container = Container.Data as ContainerRef;
                }
                else if (HiddenSelectedContainerTextBox != null)
                {
                    if (serviceData is ContainerTxn && !HiddenSelectedContainerTextBox.IsEmpty)
                        (serviceData as ContainerTxn).Container = new ContainerRef(HiddenSelectedContainerTextBox.Data as string);
                }
                else if (HiddenSelectedContainerContainerList != null)
                {
                    if (serviceData is ContainerTxn && !HiddenSelectedContainerContainerList.IsEmpty)
                        (serviceData as ContainerTxn).Container = HiddenSelectedContainerContainerList.Data as ContainerRef;
                }
                else if (ContainerName != null)
                {
                    if (serviceData is ContainerTxn && !ContainerName.IsEmpty)
                        (serviceData as ContainerTxn).Container = new ContainerRef(ContainerName.Data as string);
                }

                if (serviceData is ContainerTxn && !(CollectionDef.Data as RevisionedObjectRef).IsNullOrEmpty())
                    (serviceData as ShopFloor).DataCollectionDef = CollectionDef.Data as RevisionedObjectRef;
                (serviceData as ShopFloor).Factory = new NamedObjectRef(Page.SessionDataContract.GetValueByName("Factory") as string);
            }

            if (Page.ProcessingContext.Status != FormsFramework.ProcessingStatusType.SubmitTransaction)
            {
                if (serviceInfo is ShopFloor_Info)
                    (serviceInfo as ShopFloor_Info).HasDataCollection = new Info(true);

                // ComputationDetails are needed for EProcedure only.
                ParamDataControl.RequestValuesWithoutComputation(serviceData as ShopFloor, serviceInfo as ShopFloor_Info);
            }
        }

        public override void ClearValues(Service serviceData)
        {
            base.ClearValues(serviceData);
            if (serviceData is ShopFloor)
            {
                ParamDataControl.Clear();
                ParamDataControl.IterationCount = 1;
                CollectionDef.Visible = false;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (DCWebPart != null)
            {
                if (Container != null)
                    Container.DataChanged += _Container_DataChanged;
                else if (HiddenSelectedContainerTextBox != null)
                    HiddenSelectedContainerTextBox.DataChanged += _HiddenSelectedContainerTextBox_DataChanged;
                else if (HiddenSelectedContainerContainerList != null)
                    HiddenSelectedContainerContainerList.DataChanged += _HiddenSelectedContainerContainerList_DataChanged;
                else if (ContainerName != null)
                {
                    if (!Page.IsPostBack)
                    {
                        _ContainerName_DataChanged();
                    }
                }
            }

            if (CollectionDef != null)
            {
                //Collection Def control should be required on CollectData page only.
                CollectionDef.DataChanged += _CollectionDef_DataChanged;
                CollectionDef.Required = Page.PrimaryServiceType == typeof(CollectData).Name;
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (Page.IsPostBack)
            {
                var selContainerName = Page.DataContract.GetValueByName<ContainerRef>("SelectedContainerNameDM");
                if (selContainerName != null && !selContainerName.IsEmpty && string.Compare(selContainerName.Name, LastContainerName, true) != 0)
                {
                    if (CurrentContainer != null)
                    {
                        CurrentContainer.Data = selContainerName;
                        LastContainerName = selContainerName.Name;
                        if (Container != null)
                            _Container_DataChanged(this, e);
                        else if (HiddenSelectedContainerTextBox != null)
                            _HiddenSelectedContainerTextBox_DataChanged(this, e);
                        else if (HiddenSelectedContainerContainerList != null)
                            _HiddenSelectedContainerContainerList_DataChanged(this, e);
                        else if (ContainerName != null)
                            _ContainerName_DataChanged();
                    }
                }

                else if (CurrentContainer != null && CurrentContainer.Data != null)
                {
                    LastContainerName = CurrentContainer.Data.ToString();
                    if (selContainerName != null && string.Compare(LastContainerName, selContainerName.Name, true) != 0)
                        Page.DataContract.SetValueByName("SelectedContainerNameDM", new ContainerRef(LastContainerName));
                }
            }
        }

        public virtual void _ContainerName_DataChanged()
        {
            _ContainerChanged = true;
            CollectionDef.ClearData();
            ParamDataControl.Clear();
            if (!ContainerName.IsEmpty)
                Service.LoadServiceValues(PrimaryServiceType, "GetDataPoints");
        }

        protected virtual void _HiddenSelectedContainerContainerList_DataChanged(object sender, EventArgs e)
        {
            _ContainerChanged = true;
            CollectionDef.ClearData();
            ParamDataControl.Clear();
            if (!HiddenSelectedContainerContainerList.IsEmpty)
                Service.LoadServiceValues(PrimaryServiceType, "GetDataPoints");
        }

        protected virtual void _HiddenSelectedContainerTextBox_DataChanged(object sender, EventArgs e)
        {
            _ContainerChanged = true;
            CollectionDef.ClearData();
            ParamDataControl.Clear();
            if (!HiddenSelectedContainerTextBox.IsEmpty)
                Service.LoadServiceValues(PrimaryServiceType, "GetDataPoints");
        }

        protected virtual void _Container_DataChanged(object sender, EventArgs e)
        {
            _ContainerChanged = true;
            CollectionDef.ClearData();
            ParamDataControl.Clear();
            if (!Container.IsEmpty)
                Service.LoadServiceValues(PrimaryServiceType, "GetDataPoints");
        }

        protected virtual void _CollectionDef_DataChanged(object sender, EventArgs e)
        {
            if (!_ContainerChanged)
            {
                if (!CollectionDef.IsEmpty)
                    Service.LoadServiceValues(PrimaryServiceType, "GetDataPoints");
                else
                    ParamDataControl.Clear();
            }
        }

        public override ValidationStatus ValidateInputData(Service serviceData)
        {
            ValidationStatus status =  base.ValidateInputData(serviceData);
            status.Add(ParamDataControl.Validate());

            return status;
        }

        public override void GetInputData(Service serviceData)
        {
            base.GetInputData(serviceData);
            if(ParamDataControl != null)
            {
                DataPointSummary[] dataPointSummary = ParamDataControl.GetDataPointSummary();
                if (dataPointSummary != null && dataPointSummary.Length > 0)
                    ((ShopFloor)serviceData).ParametricData = dataPointSummary[0];
            }
        }

        private bool _ContainerChanged;

        protected virtual WebPartBase DCWebPart
        {
            get { return Page.FindIForm("ParametricDataWP") as WebPartBase; }
        }

        protected virtual CWC.RevisionedObject CollectionDef
        {
            get { return Page.FindCamstarControl("DCCollectionDef") as CWC.RevisionedObject; }
        }

        protected virtual ShopFloorDCControl ParamDataControl
        {
            get { return Page.FindCamstarControl("ParamDataField") as ShopFloorDCControl; }
        }

        protected virtual CWC.ContainerList Container
        {
            get { return Page.FindCamstarControls<CWC.ContainerList>().FirstOrDefault(c => c.ID == "ContainerStatus_ContainerName"); }
        }

        protected virtual CWC.TextBox HiddenSelectedContainerTextBox
        {
            get { return Page.FindCamstarControl("HiddenSelectedContainer") as CWC.TextBox; }
        }

        protected virtual CWC.ContainerList HiddenSelectedContainerContainerList
        {
            get { return Page.FindCamstarControl("HiddenSelectedContainer") as CWC.ContainerList; }
        }

        protected virtual CWC.TextBox ContainerName
        {
            get { return Page.FindCamstarControl("TopContainerName") as CWC.TextBox; }
        }
    }
}

