/* Copyright 2023 Siemens */
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

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
/// Summary description for PartRequestComplete Txn
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Parts
{
    public class PartRequestComplete : MatrixWebPart, IPostBackEventHandler
    {
        private CWC.TextBox svctype { get { return Page.FindCamstarControl("ServiceType") as CWC.TextBox; } }
        private CWC.TextBox readonlyinfo { get { return Page.FindCamstarControl("ReadOnlyInfo") as CWC.TextBox; } }

        private CWC.NamedObject resource { get { return Page.FindCamstarControl("ResourceField") as CWC.NamedObject; } }
		private CWC.FlyoutDropDown resourcebutton { get { return Page.FindCamstarControl("ResourcePartsButton") as CWC.FlyoutDropDown; } }
        private CWC.NamedObject joborderfield { get { return Page.FindCamstarControl("JobOrderField") as CWC.NamedObject; } }
		private CWC.FlyoutDropDown jobhistorybutton { get { return Page.FindCamstarControl("JobHistoryButton") as CWC.FlyoutDropDown; } }
        private CWC.NamedObject requestorderfield { get { return Page.FindCamstarControl("RequestOrderField") as CWC.NamedObject; } }
		private CWC.FlyoutDropDown requesthistorybutton { get { return Page.FindCamstarControl("RequestHistoryButton") as CWC.FlyoutDropDown; } }
        private CWC.DropDownList requesttypefield { get { return Page.FindCamstarControl("RequestTypeField") as CWC.DropDownList; } }

        private JQDataGrid servicedetailspanel { get { return Page.FindCamstarControl("ServiceDetailsPanel") as JQDataGrid; } }
        private JQDataGrid requestorderpanel { get { return Page.FindCamstarControl("RequestOrderMaterialPartsPanel") as JQDataGrid; } }
        
        public PartRequestComplete()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        public void RaisePostBackEvent(string eventArgument)
        {

        }

        private void SetControls()
        {

            if (this.PrimaryServiceType == "PartRequestComplete")
            {
                if (readonlyinfo.Data != null)
                {
                    resource.ReadOnly = true;
                    joborderfield.ReadOnly = true;
                    requestorderfield.ReadOnly = true;
                }
                requesttypefield.ReadOnly = true;
                
                //for (int i = 0; i < (requestorderpanel.GridContext as BoundContext).Fields.Count; i++)
                //{
                //    if ((requestorderpanel.GridContext as BoundContext).Fields[i].ID == "_extender_")
                //        (requestorderpanel.GridContext as BoundContext).Fields[i].Width = 590;
                //}

                //for (int i = 0; i < (servicedetailspanel.GridContext as BoundContext).Fields.Count; i++)
                //{
                //    if ((servicedetailspanel.GridContext as BoundContext).Fields[i].ID == "RequestPartQty")
                //        (servicedetailspanel.GridContext as BoundContext).Fields[i].Visible = false;
                //    else if ((servicedetailspanel.GridContext as BoundContext).Fields[i].ID == "PartToReplaceName")
                //        (servicedetailspanel.GridContext as BoundContext).Fields[i].Visible = false;
                //    else if ((servicedetailspanel.GridContext as BoundContext).Fields[i].ID == "PartToReplaceQty")
                //        (servicedetailspanel.GridContext as BoundContext).Fields[i].Visible = false;
                //    else if ((servicedetailspanel.GridContext as BoundContext).Fields[i].ID == "ScrapQty")
                //        (servicedetailspanel.GridContext as BoundContext).Fields[i].Visible = false;
                //    else if ((servicedetailspanel.GridContext as BoundContext).Fields[i].ID == "PhysicalLocation")
                //        (servicedetailspanel.GridContext as BoundContext).Fields[i].Visible = false;
                //    else if ((servicedetailspanel.GridContext as BoundContext).Fields[i].ID == "PhysicalPosition")
                //        (servicedetailspanel.GridContext as BoundContext).Fields[i].Visible = false;
                //    else if ((servicedetailspanel.GridContext as BoundContext).Fields[i].ID == "_extender_")
                //        (servicedetailspanel.GridContext as BoundContext).Fields[i].Width = 480;

                //    (servicedetailspanel.GridContext as BoundContext).Fields[i].Editable = false;
                //}
            }
        }

        public void FetchGridData()
        {
            string serviceName = this.PrimaryServiceType.ToString();

            UserProfile profile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
            ResultStatus res = new ResultStatus(null, false);
            var cdo = WCFObject.CreateObject(serviceName) as ICreator;
            var request = WCFObject.CreateObject(serviceName + "_Request") as ICreator;
            var reqInfo = WCFObject.CreateObject(serviceName + "_Info") as ICreator;
            Result result = null;
            var service = new WSDataCreator().CreateService(serviceName, profile);

            // Set Data Input
            cdo.SetValue("Resource", resource.Data as NamedObjectRef);
            cdo.SetValue("RequestOrder", requestorderfield.Data as NamedObjectRef);
            cdo.SetValue("JobOrder", joborderfield.Data as DropDownList);

            // Set Request Value
            reqInfo.SetValue("RequestType", new OM.Info(true));
            reqInfo.SetValue("RequestOrderParts", new OM.PartRequestOrderPart_Info());
            reqInfo.SetValue("RequestOrderParts.MaterialPart", new OM.Info(true));
            reqInfo.SetValue("RequestOrderParts.PartName", new OM.Info(true));
            reqInfo.SetValue("RequestOrderParts.PartQty", new OM.Info(true));
            //reqInfo.SetValue("RequestOrderParts.PhysicalLocation", new OM.Info(true));
            //reqInfo.SetValue("RequestOrderParts.PhysicalPosition", new OM.Info(true));

            reqInfo.SetValue("RequestOrderMaterialParts", new OM.PartRequestOrderMaterialPart_Info());
            reqInfo.SetValue("RequestOrderMaterialParts.MaterialPart", new OM.Info(true));
            reqInfo.SetValue("RequestOrderMaterialParts.RequestPartQty", new OM.Info(true));
            reqInfo.SetValue("RequestOrderMaterialParts.AssignPartQty", new OM.Info(true));

            request.SetValue("Info", reqInfo);

            // Execute Request 
            ResultStatus rslt = service.GetEnvironment(cdo as DCObject, request as Request, out result);

            // Result
            if (rslt.IsSuccess)
            {
                requesttypefield.Data = (result as ICreator).GetValue("Value.RequestType");
                PartRequestOrderPart[] collectedData = ((result as ICreator).GetValue("Value.RequestOrderParts") as PartRequestOrderPart[]);

                var newSvcDetails = new List<PartTxnServiceDetail>();
                int rowCount = 0;

                if (collectedData != null)
                    rowCount = collectedData.Count();

                for (int i = 0; i < rowCount; i++)
                {
                    newSvcDetails.Add(new PartTxnServiceDetail
                    {
                        MaterialPart = collectedData[i].MaterialPart,
                        PartName = collectedData[i].PartName,
                        PartQty = collectedData[i].PartQty,
                        //PhysicalLocation = collectedData[i].PhysicalLocation,
                        //PhysicalPosition = collectedData[i].PhysicalPosition
                    });
                }

                servicedetailspanel.ClearData();
                servicedetailspanel.Data = newSvcDetails.ToArray();
                servicedetailspanel.OriginalData = newSvcDetails.ToArray();

                requestorderpanel.ClearData();
                requestorderpanel.Data = ((result as ICreator).GetValue("Value.RequestOrderMaterialParts"));
                requestorderpanel.OriginalData = ((result as ICreator).GetValue("Value.RequestOrderMaterialParts"));
            }
        }

        public void ForceRequestType()
        {
            if (requesttypefield.Text == "Request")
                requesttypefield.Data = 1;
            else if (requesttypefield.Text == "Return")
                requesttypefield.Data = 2;
        }

        public void RequestType_DataChanged(object sender, EventArgs e)
        {
            //SetControls();
        }

        public void ResourceField_DataChanged(object sender, EventArgs e)
        {
            var fs = FrameworkManagerUtil.GetFrameworkSession();
            UserProfile profile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
            ResultStatus res = new ResultStatus(null, false);

            PartRequestCompleteService Svc = new PartRequestCompleteService(fs.CurrentUserProfile);
            OM.PartRequestComplete SvcData = new OM.PartRequestComplete();
            PartRequestComplete_Info SvcInfo = new PartRequestComplete_Info();
            PartRequestComplete_Request ReqData = new PartRequestComplete_Request();
            PartRequestComplete_Result ResData = new PartRequestComplete_Result();

            SvcData.Resource = new NamedObjectRef();
            if (resource.Data != null)
            {
                SvcData.Resource.Name = resource.Data.ToString();

                SvcInfo.JobOrder = new Info();
                SvcInfo.JobOrder.RequestSelectionValues = true;

                ReqData.Info = SvcInfo;
                OM.ResultStatus Results = Svc.GetEnvironment(SvcData, ReqData, out ResData);
                if (Results.IsSuccess & ResData.Environment != null)
                {
                    if (ResData.Environment.JobOrder.SelectionValues.Rows != null)
                        joborderfield.Data = ResData.Environment.JobOrder.SelectionValues.Rows[0].Values[0];
                }
                JobOrderField_DataChanged(sender, e);
            }
            else
            {
                joborderfield.Data = null;
                requestorderfield.Data = null;
                requesttypefield.Data = null;
                requestorderpanel.ClearData();
                servicedetailspanel.ClearData();
            }
        }

        public void JobOrderField_DataChanged(object sender, EventArgs e)
        {
            var fs = FrameworkManagerUtil.GetFrameworkSession();
            UserProfile profile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
            ResultStatus res = new ResultStatus(null, false);

            PartRequestCompleteService Svc = new PartRequestCompleteService(fs.CurrentUserProfile);
            OM.PartRequestComplete SvcData = new OM.PartRequestComplete();
            PartRequestComplete_Info SvcInfo = new PartRequestComplete_Info();
            PartRequestComplete_Request ReqData = new PartRequestComplete_Request();
            PartRequestComplete_Result ResData = new PartRequestComplete_Result();

            SvcData.Resource = new NamedObjectRef();
            if (resource.Data != null)
                SvcData.Resource.Name = resource.Data.ToString();
            SvcData.JobOrder = new NamedObjectRef();
            if (joborderfield.Data != null)
                SvcData.JobOrder.Name = joborderfield.Data.ToString();

            if (resource.Data != null)
            {
                SvcInfo.RequestOrder = new Info();
                SvcInfo.RequestOrder.RequestSelectionValues = true;

                SvcInfo.RequestSelectionValuesInfo = new SelectionValuesInfo();
                SvcInfo.RequestSelectionValuesInfo.Parameters = new QueryParameter[]
                {
                    new QueryParameter {Name="NameFilter", Value="%" },
                };

                SvcInfo.RequestSelectionValuesInfo.Options = new QueryOptions();
                SvcInfo.RequestSelectionValuesInfo.Options.StartRow = 1;
                SvcInfo.RequestSelectionValuesInfo.Options.RowSetSize = 50;

                ReqData.Info = SvcInfo;

                OM.ResultStatus Results = Svc.GetEnvironment(SvcData, ReqData, out ResData);
                if (Results.IsSuccess & ResData.Environment != null)
                {
                    if (ResData.Environment.RequestOrder.SelectionValues.Rows != null)
                        requestorderfield.Data = ResData.Environment.RequestOrder.SelectionValues.Rows[0].Values[0];
                }
            }
        }

        protected void RequestOrderField_DataChanged(object sender, EventArgs e)
        {
            FetchGridData();
            SetControls();
        }

        private PartTxnServiceDetail[] GetServiceDetailsToSubmit()
        {
            return servicedetailspanel.Data != null ? (servicedetailspanel.Data as Array).Cast<PartTxnServiceDetail>().ToArray() : new PartTxnServiceDetail[] { };
        }

        public override void GetInputData(OM.Service serviceData)
        {
            base.GetInputData(serviceData);
            if (this.PrimaryServiceType == "PartRequestComplete")
            {
                PartTxnServiceDetail[] getGrid = GetServiceDetailsToSubmit();
                if (getGrid.Count() > 0)
                {
                    if (serviceData is OM.PartRequestComplete)
                    {
                        (serviceData as OM.PartTxn).ServiceDetails = new PartTxnServiceDetail[getGrid.Count()];
                        for (int i = 0; i < getGrid.Count(); i++)
                        {
                            (serviceData as OM.PartTxn).ServiceDetails[i] = new PartTxnServiceDetail();
                            (serviceData as OM.PartTxn).ServiceDetails[i].PartName = getGrid[i].PartName;
                            (serviceData as OM.PartTxn).ServiceDetails[i].PartQty = getGrid[i].PartQty;
                            (serviceData as OM.PartTxn).ServiceDetails[i].MaterialPart = getGrid[i].MaterialPart;
                            (serviceData as OM.PartTxn).ServiceDetails[i].RequestPartQty = getGrid[i].PartQty;
                        }
                    }
                }
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            this.Page.PrimaryServiceType = svctype.Data.ToString();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (readonlyinfo.Data == null)
            {
                resource.DataChanged += new EventHandler(ResourceField_DataChanged);
                joborderfield.DataChanged += new EventHandler(JobOrderField_DataChanged);
                requestorderfield.DataChanged += new EventHandler(RequestOrderField_DataChanged);
                requesttypefield.DataChanged += new EventHandler(RequestType_DataChanged);
            }

            if (!this.Page.IsPostBack && resource.Data != null)
            {
                FetchGridData();
            }


			CamstarWebControl.SetRenderToClient(resourcebutton);
			CamstarWebControl.SetRenderToClient(jobhistorybutton);
			CamstarWebControl.SetRenderToClient(requesthistorybutton);
			
            SetControls();
            ForceRequestType();

            if (!Page.IsPostBack && Page.IsAJAXFloatingFrame)
                ScriptManager.RegisterStartupScript(this.Page.Form, this.Page.GetType(), "PopUpMaximize", "pop.GetCallerPage().pop.maximize();", true);
        }
    }
}
