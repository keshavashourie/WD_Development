/* Copyright 2019 Siemens */
using System;
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
    public class SS_LotStart : MatrixWebPart
    {
        protected CWC.TextBox _txtContainerName { get { return Page.FindCamstarControl("LotStart_ContainerName") as CWC.TextBox; } }
        protected CWC.RevisionedObject _rdoProduct { get { return Page.FindCamstarControl("LotStart_Product") as CWC.RevisionedObject; } }
        protected CWC.NamedObject _ndoStartReason { get { return Page.FindCamstarControl("LotStart_StartReason") as CWC.NamedObject; } }
        protected CWC.RevisionedObject _rdoWorkflow { get { return Page.FindCamstarControl("LotStart_Workflow") as CWC.RevisionedObject; } }
        protected CWC.NamedSubentity _ndsWorkflowStep { get { return Page.FindCamstarControl("LotStart_WorkflowStep") as CWC.NamedSubentity; } }
        protected CWC.NamedObject _ndoFactory { get { return Page.FindCamstarControl("LotStart_Factory") as CWC.NamedObject; } }
        protected CWC.TextBox _txtQty2 { get { return Page.FindCamstarControl("LotStart_Qty2") as CWC.TextBox; } }
        protected CWC.TextBox _txtQty { get { return Page.FindCamstarControl("LotStart_Qty") as CWC.TextBox; } }
        protected JQDataGrid _gridSvcAttribute { get { return Page.FindCamstarControl("LotStart_ServiceAttrsDetails") as JQDataGrid; } }
        protected JQDataGrid _gridWafers { get { return Page.FindCamstarControl("LotStart_Wafers") as JQDataGrid; } }
        protected JQDataGrid _gridWaferRunNumbers { get { return Page.FindCamstarControl("LotStart_WaferRunNumbers") as JQDataGrid; } }
        protected CWC.DateChooser _dateExpectedStartdate { get { return Page.FindCamstarControl("LotStart_ExpectedStartDate") as CWC.DateChooser; } }
        protected JQDataGrid _gridValidValues { get { return Page.FindCamstarControl("ValidValuesGrid") as JQDataGrid; } }
        protected CWC.TextBox _txtComputerName { get { return Page.FindCamstarControl("LotStart_ComputerName") as CWC.TextBox; } }
        
        //---------------------------------------------------
        // Fetch service attr details and bind it to the grid
        //---------------------------------------------------
        protected void GetServiceAttrsDetails()
        {
            //Initialize the Service Data
            UserProfile profile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
            string ServiceType = this.Page.PrimaryServiceType;
            OM.Service serviceData = CreateServiceData(ServiceType);
            var Svc = new WSDataCreator().CreateService(ServiceType, profile);
            var SvcData = WCFObject.CreateObject(ServiceType) as ICreator;
            var SvcInfo = WCFObject.CreateObject(ServiceType + "_Info") as ICreator;
            var ReqData = WCFObject.CreateObject(ServiceType + "_Request") as ICreator;
            var ResData = WCFObject.CreateObject(ServiceType + "_Result") as ICreator;
            Result result = null;

            GetInputData(SvcData as OM.Service);
            //Manually collect the workflow step
            (SvcData as LotStart).WorkflowStep = new NamedSubentityRef();
            (SvcData as LotStart).WorkflowStep.Name = (_ndsWorkflowStep.Data as NamedSubentityRef).Name;
            (SvcData as LotStart).WorkflowStep.Parent = null;
            (SvcData as LotStart).WorkflowStep.CDOTypeName = null;

            SvcInfo.SetValue("ServiceAttrsDetailsSelection", new ServiceAttrsDetails_Info());
            SvcInfo.SetValue("ServiceAttrsDetailsSelection.Attribute", new Info(true));
            SvcInfo.SetValue("ServiceAttrsDetailsSelection.AttributeName", new Info(true));
            SvcInfo.SetValue("ServiceAttrsDetailsSelection.AlternateName1", new Info(true));
            SvcInfo.SetValue("ServiceAttrsDetailsSelection.AlternateName2", new Info(true));
            SvcInfo.SetValue("ServiceAttrsDetailsSelection.AttributeValue", new Info(true));
            SvcInfo.SetValue("ServiceAttrsDetailsSelection.AttributeRevision", new Info(true));
            SvcInfo.SetValue("ServiceAttrsDetailsSelection.AccessLevel", new Info(true));
            SvcInfo.SetValue("ServiceAttrsDetailsSelection.FieldType", new Info(true));
            SvcInfo.SetValue("ServiceAttrsDetailsSelection.IsRequired", new Info(true));
            SvcInfo.SetValue("ServiceAttrsDetailsSelection.ServiceAttrsSetupName", new Info(true));
            SvcInfo.SetValue("ServiceAttrsDetailsSelection.ObjectTypeName", new Info(true));
            SvcInfo.SetValue("ServiceAttrsDetailsSelection.ValidValues", new AttributeValidValuesChanges_Info());
            SvcInfo.SetValue("ServiceAttrsDetailsSelection.ValidValues.AttributeValue", new Info(true));
            SvcInfo.SetValue("ServiceAttrsDetailsSelection.ValidValues.AttributeRevision", new Info(true));
            ReqData.SetValue("Info", SvcInfo);
            
            //Submit the transaction
            ResultStatus Results = Svc.GetEnvironment(SvcData as DCObject, ReqData as Request, out result);
            _gridSvcAttribute.ClearData();
            if (Results.IsSuccess && (result.Value as LotStart).ServiceAttrsDetailsSelection != null)
            {
                //Bind result to the grid
                _gridSvcAttribute.Data = (result.Value as LotStart).ServiceAttrsDetailsSelection.ToArray();
                _gridSvcAttribute.OriginalData = (result.Value as LotStart).ServiceAttrsDetailsSelection.ToArray();

                //Create Valid Values table
                DataTable validValuesDT = new DataTable();
                int countSvcAttr = (result.Value as LotStart).ServiceAttrsDetailsSelection.Count();
                validValuesDT.Columns.Add("Attribute", typeof(String));
                validValuesDT.Columns.Add("AttributeValue", typeof(String));
                validValuesDT.Columns.Add("AttributeRevision", typeof(String));
                for (int i = 0; i < countSvcAttr; i++)
                {
                    if (((result.Value as LotStart).ServiceAttrsDetailsSelection.GetValue(i) as ServiceAttrsDetails).ValidValues != null)
                    {
                        int countValidValues = ((result.Value as LotStart).ServiceAttrsDetailsSelection.GetValue(i) as ServiceAttrsDetails).ValidValues.Count();
                        for (int x = 0; x < countValidValues; x++)
                        {
                            DataRow dtRow = validValuesDT.NewRow();
                            dtRow.SetField("Attribute", ((result.Value as LotStart).ServiceAttrsDetailsSelection.GetValue(i) as ServiceAttrsDetails).Attribute.Name);
                            dtRow.SetField("AttributeValue", (((result.Value as LotStart).ServiceAttrsDetailsSelection.GetValue(i) as ServiceAttrsDetails).ValidValues.GetValue(x) as AttributeValidValuesChanges).AttributeValue);
                            dtRow.SetField("AttributeRevision", (((result.Value as LotStart).ServiceAttrsDetailsSelection.GetValue(i) as ServiceAttrsDetails).ValidValues.GetValue(x) as AttributeValidValuesChanges).AttributeRevision);
                            validValuesDT.Rows.Add(dtRow);
                        }
                    }
                }
                _gridValidValues.ClearData();
                _gridValidValues.Data = validValuesDT;
                _gridValidValues.OriginalData = validValuesDT;
            }
        }

        //---------------------------------------------------
        // Wafers Auto Generate button function
        //---------------------------------------------------
        public void WafersAutoGenerate()
        {
            var newWafersDetails = new List<ModifyWafersDetails>();
            int iQty2 = 0;
            int iQty = 0;
            int iNDPW = 0;
            if (_txtQty2.Data.ToString() != "0" || _txtQty2.Data != null)
                iQty2 = Convert.ToInt32(_txtQty2.Data.ToString());
            if (_txtQty.Data.ToString() != "0" || _txtQty.Data != null)
                iQty = Convert.ToInt32(_txtQty.Data.ToString());
            if (iQty2 > 0)
            {
                int gridNDPW = 0;
                string gridWaferScribeNumber = "";
                _gridWafers.ClearData();
                iNDPW = iQty / iQty2;
                for (int i = 1; i <= iQty2; i++)
                {
                    if (i < iQty2)
                    {
                        gridNDPW = iNDPW;
                        iQty = iQty - iNDPW;
                    }
                    else
                        gridNDPW = iQty;
                    if (_txtContainerName.Data != null)
                        gridWaferScribeNumber = _txtContainerName.Data.ToString() + "-" + string.Format("{0:00}", i);
                    else
                        gridWaferScribeNumber = string.Format("{0:00}", i);

                    newWafersDetails.Add(new ModifyWafersDetails
                    {
                        NDPW = gridNDPW,
                        GoodQty = 0,
                        RequireDataCollection = true,
                        RequireTracking = true,
                        WaferScribeNumber = gridWaferScribeNumber,
                        WaferNumber = string.Format("{0:00}", i)
                    });
                    _gridWafers.Data = newWafersDetails.ToArray();
                    _gridWafers.OriginalData = newWafersDetails.ToArray();
                }
            }
        }

        //---------------------------------------------------
        // Get Factory from session data contract
        //---------------------------------------------------
        protected void assignFactory()
        {
            if (Page.SessionDataContract.GetValueByName(Camstar.WebPortal.Constants.DataMemberConstants.Factory) != null)
                _ndoFactory.Data = Page.SessionDataContract.GetValueByName(Camstar.WebPortal.Constants.DataMemberConstants.Factory).ToString();
        }

        //---------------------------------------------------
        // Product field data changed event
        //---------------------------------------------------
        protected void ProductField_DataChanged(object sender, EventArgs e)
        {
            if (_ndsWorkflowStep.Data != null && _ndoStartReason.Data != null)
                GetServiceAttrsDetails();
            _dateExpectedStartdate.Data = System.DateTime.Today.ToString("M/d/yyyy");
        }

        //---------------------------------------------------
        // Start Reason field data changed event
        //---------------------------------------------------
        protected void StartReasonField_DataChanged(object sender, EventArgs e)
        {
            if (_ndsWorkflowStep.Data != null && _ndoStartReason.Data != null)
                GetServiceAttrsDetails();
        }

        //---------------------------------------------------
        // Workflow Step field data changed event
        //---------------------------------------------------
        protected void WorkflowStepField_DataChanged(object sender, EventArgs e)
        {
            if (_ndsWorkflowStep.Data != null && _ndoStartReason.Data != null)
                GetServiceAttrsDetails();
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
                    case "AutoGenerate":
                        {
                            WafersAutoGenerate();
                            break;
                        }
                    case "ClearRunNumbers":
                        {
                            _gridWaferRunNumbers.ClearData();
                            break;
                        }
                    case "ClearWafers":
                        {
                            _gridWafers.ClearData();
                            break;
                        }
                    case "Reset":
                        {
                            Page.ShopfloorReset(sender, e);
                            assignFactory();
                            break;
                        }
                }
            }
        } // WebPartCustomAction(object sender, CustomActionEventArgs e)

        //---------------------------------------------------------
        // Get Input Data override function
        //---------------------------------------------------------
        public override void GetInputData(Service serviceData)
        {
            base.GetInputData(serviceData);
            //ServiceAttributes
            if (_gridSvcAttribute.Data != null)
            {
                ServiceAttrsDetails[] getServiceAttrsDetails = _gridSvcAttribute.Data as ServiceAttrsDetails[];
                if (serviceData is LotStart)
                {
                    (serviceData as LotStart).ServiceAttrsDetails = new ServiceAttrsDetails[getServiceAttrsDetails.Count()];
                    for (int i = 0; i < getServiceAttrsDetails.Count(); i++)
                    {
                        (serviceData as LotStart).ServiceAttrsDetails[i] = new ServiceAttrsDetails();
                        (serviceData as LotStart).ServiceAttrsDetails[i].Attribute = new NamedObjectRef();
                        (serviceData as LotStart).ServiceAttrsDetails[i].Attribute.Name = getServiceAttrsDetails[i].Attribute.Name;
                        (serviceData as LotStart).ServiceAttrsDetails[i].FieldType = getServiceAttrsDetails[i].FieldType;
                        (serviceData as LotStart).ServiceAttrsDetails[i].ServiceAttrsSetupName = getServiceAttrsDetails[i].ServiceAttrsSetupName;
                        (serviceData as LotStart).ServiceAttrsDetails[i].AttributeValue = getServiceAttrsDetails[i].AttributeValue;
                        (serviceData as LotStart).ServiceAttrsDetails[i].AttributeRevision = getServiceAttrsDetails[i].AttributeRevision;
                    }
                }
            }
            if (_gridWafers.Data != null)
            {
                ModifyWafersDetails[] getModifyWafersDetails = _gridWafers.Data as ModifyWafersDetails[];
                if (serviceData is LotStart)
                {
                    (serviceData as LotStart).Wafers = new ModifyWafersDetails[getModifyWafersDetails.Count()];
                    for (int i = 0; i < getModifyWafersDetails.Count(); i++)
                    {
                        (serviceData as LotStart).Wafers[i] = new ModifyWafersDetails();
                        (serviceData as LotStart).Wafers[i].WaferScribeNumber = getModifyWafersDetails[i].WaferScribeNumber;
                        (serviceData as LotStart).Wafers[i].WaferNumber = getModifyWafersDetails[i].WaferNumber;
                        (serviceData as LotStart).Wafers[i].RequireTracking = getModifyWafersDetails[i].RequireTracking;
                        (serviceData as LotStart).Wafers[i].RequireDataCollection = getModifyWafersDetails[i].RequireDataCollection;
                        (serviceData as LotStart).Wafers[i].NDPW = getModifyWafersDetails[i].NDPW;
                        (serviceData as LotStart).Wafers[i].GoodQty = getModifyWafersDetails[i].GoodQty;
                        (serviceData as LotStart).Wafers[i].VendorLotNumber = getModifyWafersDetails[i].VendorLotNumber;
                        (serviceData as LotStart).Wafers[i].VendorName = getModifyWafersDetails[i].VendorName;
                    }
                }
            }
            if (_gridWaferRunNumbers.Data != null)
            {
                ModifyWaferRunNumbersDetails[] getWaferRunNumbersDetails = _gridWaferRunNumbers.Data as ModifyWaferRunNumbersDetails[];
                if (serviceData is LotStart)
                {
                    (serviceData as LotStart).WaferRunNumbers = new ModifyWaferRunNumbersDetails[getWaferRunNumbersDetails.Count()];
                    for (int i = 0; i < getWaferRunNumbersDetails.Count(); i++)
                    {
                        (serviceData as LotStart).WaferRunNumbers[i] = new ModifyWaferRunNumbersDetails();
                        (serviceData as LotStart).WaferRunNumbers[i].WaferRunNumber = getWaferRunNumbersDetails[i].WaferRunNumber;
                        (serviceData as LotStart).WaferRunNumbers[i].ReferenceAttributeNumber = getWaferRunNumbersDetails[i].ReferenceAttributeNumber;
                    }
                }
            }
        }

        //---------------------------------------------------
        // Override OnLoad event
        //---------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);
                //Page.DistributeDataContract();
                //Page.CollectDataContract();
                _rdoProduct.DataChanged += new EventHandler(ProductField_DataChanged);
                _ndoStartReason.DataChanged += new EventHandler(StartReasonField_DataChanged);
                _ndsWorkflowStep.DataChanged += new EventHandler(WorkflowStepField_DataChanged);
                if (Page.EventArgument == "FloatingFrameSubmitParentPostBackArgument")
                {
                    var attrVal = Page.PortalContext.DataContract.GetValueByName<string>("LotStart_ReturnedValue");
                    var attrRev = Page.PortalContext.DataContract.GetValueByName<string>("LotStart_ReturnedRevision");
                    string selectedAttr = Page.PortalContext.DataContract.GetValueByName("LotStart_SelectedAttribute").ToString();
                    if (!string.IsNullOrEmpty(selectedAttr))
                    {
                        ServiceAttrsDetails[] getServiceAttrsDetails = _gridSvcAttribute.Data as ServiceAttrsDetails[];
                        for (int i = 0; i < getServiceAttrsDetails.Count(); i++)
                        {
                            if (getServiceAttrsDetails[i].Attribute.Name == selectedAttr)
                            {
                                getServiceAttrsDetails[i].AttributeValue = attrVal;
                                getServiceAttrsDetails[i].AttributeRevision = attrRev;
                            }
                        }
                        _gridSvcAttribute.ClearData();
                        _gridSvcAttribute.Data = getServiceAttrsDetails;
                        _gridSvcAttribute.OriginalData = getServiceAttrsDetails;
                    }
                }
                if (!Page.IsPostBack)
                {
                    _txtComputerName.Data = SEMI.AppCode.UIUtility.GetComputerName(this);
                    assignFactory();
                }
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

        //---------------------------------------------------
        // Override Post Execute Event
        //---------------------------------------------------
        public override void PostExecute(ResultStatus status, Service serviceData)
        {
            base.PostExecute(status, serviceData);
            if (status.IsSuccess)
            {
                _txtContainerName.ClearData();
                _gridWafers.ClearData();
            }
        }
    }
}



