// Copyright Siemens 2023
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

namespace Camstar.WebPortal.WebPortlets.Parts
{
    public class PartRequest : MatrixWebPart, IPostBackEventHandler
    {
        private CWC.TextBox svctype { get { return Page.FindCamstarControl("ServiceType") as CWC.TextBox; } }
        private CWC.TextBox readonlyinfo { get { return Page.FindCamstarControl("ReadOnlyInfo") as CWC.TextBox; } }

        private CWC.NamedObject resource { get { return Page.FindCamstarControl("ResourceField") as CWC.NamedObject; } }
		private CWC.FlyoutDropDown resourcebutton { get { return Page.FindCamstarControl("ResourcePartsButton") as CWC.FlyoutDropDown; } }
        private CWC.Button resourcebuttonpopup { get { return Page.FindCamstarControl("ResourcePartsButtonPopup") as CWC.Button; } }
        private CWC.NamedObject joborderfield { get { return Page.FindCamstarControl("JobOrderField") as CWC.NamedObject; } }
		private CWC.FlyoutDropDown jobhistorybutton { get { return Page.FindCamstarControl("JobHistoryButton") as CWC.FlyoutDropDown; } }
        private CWC.Button jobhistorybuttonpopup { get { return Page.FindCamstarControl("JobHistoryButtonPopup") as CWC.Button; } }
        private CWC.NamedObject requestorderfield { get { return Page.FindCamstarControl("RequestOrderField") as CWC.NamedObject; } }
		private CWC.FlyoutDropDown requesthistorybutton { get { return Page.FindCamstarControl("RequestHistoryButton") as CWC.FlyoutDropDown; } }
        private CWC.CheckBox requireemail { get { return Page.FindCamstarControl("RequireAcknowledgeEmailField") as CWC.CheckBox; } }
        private CWC.DropDownList requesttypefield { get { return Page.FindCamstarControl("RequestTypeField") as CWC.DropDownList; } }

        private CWC.Button addselectedpartbutton { get { return Page.FindCamstarControl("AddSelectedPartButton") as CWC.Button; } }
        private JQDataGrid materiallistpanel { get { return Page.FindCamstarControl("MaterialListPanel") as JQDataGrid; } }
        private JQDataGrid resourcepartspanel { get { return Page.FindCamstarControl("ResourcePartsPanel") as JQDataGrid; } }
        private JQDataGrid selectionvaluepanel { get { return Page.FindCamstarControl("SelectionValuePanel") as JQDataGrid; } }
        private JQDataGrid servicedetailspanel { get { return Page.FindCamstarControl("ServiceDetailsPanel") as JQDataGrid; } }
        private JQDataGrid requestorderpanel { get { return Page.FindCamstarControl("RequestOrderMaterialPartsPanel") as JQDataGrid; } }
        private CWC.CheckBox autocompletefield { get { return Page.FindCamstarControl("AutoCompleteField") as CWC.CheckBox; } }

        public PartRequest(){}

        public void RaisePostBackEvent(string eventArgument){}

        private void SetControls()
        {
            if (this.PrimaryServiceType == "PartRequest")
            {
                if (readonlyinfo.Data != null)
                {
                    resource.ReadOnly = true;
                }
                requireemail.DefaultValue = true;

                if (requesttypefield.Text == "Return")
                {
                    materiallistpanel.Hidden = true;
                    materiallistpanel.Enabled = false;

                    resourcepartspanel.Hidden = false;
                    resourcepartspanel.Enabled = true;
                }
                else
                {
                    resourcepartspanel.Hidden = true;
                    resourcepartspanel.Enabled = false;

                    materiallistpanel.Hidden = false;
                    materiallistpanel.Enabled = true;
                }
            }
        }

        public void FetchSelectionGridData()
        {
            if (this.PrimaryServiceType == "PartRequest" || this.PrimaryServiceType == "PartRequestUpdate")
            {
                string serviceName = this.PrimaryServiceType.ToString();

                var fs = FrameworkManagerUtil.GetFrameworkSession();
                UserProfile profile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
                ResultStatus res = new ResultStatus(null, false);

                PartTxnService Svc = new PartTxnService(fs.CurrentUserProfile);
                PartTxn SvcData = new PartTxn();
                PartTxn_Info SvcInfo = new PartTxn_Info();
                PartTxn_Request ReqData = new PartTxn_Request();
                PartTxn_Result ResData = new PartTxn_Result();

                SvcData.Resource = new NamedObjectRef();
                if (resource.Data != null)
                    SvcData.Resource.Name = resource.Data.ToString();
                SvcData.JobOrder = new NamedObjectRef();

                if (joborderfield.Data != null)
                    SvcData.JobOrder.Name = joborderfield.Data.ToString();

                if (requesttypefield.Text == "Request")
                {
                    SvcInfo.MaterialList = new PartTxnServiceDetail_Info();
                    SvcInfo.MaterialList.MaterialPart = new Info(true);
                    SvcInfo.MaterialList.RequiredPartQty = new Info(true);
                    SvcInfo.MaterialList.MaintenanceReq = new Info(true);
                    SvcInfo.MaterialList.PartQty = new Info(true);

                    ReqData.Info = SvcInfo;
                    OM.ResultStatus Results = Svc.Load(SvcData, ReqData, out ResData);
                    if (Results.IsSuccess)
                    {
                        materiallistpanel.Data = ResData.Value.MaterialList;
                        materiallistpanel.OriginalData = ResData.Value.MaterialList;
                    }
                }
                else if (requesttypefield.Text == "Return")
                {
                    SvcInfo.ResourceParts = new ResourcePart_Info();
                    SvcInfo.ResourceParts.PartName = new Info(true);
                    SvcInfo.ResourceParts.PartQty = new Info(true);
                    SvcInfo.ResourceParts.MaterialPart = new Info(true);

                    ReqData.Info = SvcInfo;
                    OM.ResultStatus Results = Svc.Load(SvcData, ReqData, out ResData);
                    if (Results.IsSuccess)
                    {
                        var newSvcDetails = new List<PartTxnServiceDetail>();
                        int rowCount = 0;
                        if (ResData.Value.ResourceParts != null)
                            rowCount = ResData.Value.ResourceParts.Count();

                        for (int i = 0; i < rowCount; i++)
                        {
                            newSvcDetails.Add(new PartTxnServiceDetail
                            {
                                PartName = ResData.Value.ResourceParts[i].PartName,
                                PartQty = ResData.Value.ResourceParts[i].PartQty,
                                MaterialPart = ResData.Value.ResourceParts[i].MaterialPart
                            });
                        }

                        resourcepartspanel.Data = newSvcDetails.ToArray();
                        resourcepartspanel.OriginalData = newSvcDetails.ToArray();
                    }
                }
            }
        }

        public void ForceRequestType()
        {
            if (requesttypefield.Text == "Request")
                requesttypefield.Data = 1;
            else if (requesttypefield.Text == "Return")
                requesttypefield.Data = 2;
        }

        public void JobOrderField_DataChanged(object sender, EventArgs e)
        {
            if (joborderfield.Data != null)
            {
                jobhistorybuttonpopup.Visible = true;
            }
            else
            {
                jobhistorybuttonpopup.Visible = false;
            }
        }

        public void RequestType_DataChanged(object sender, EventArgs e)
        {
            if (joborderfield.Data != null || resource.Data != null)
            {
                jobhistorybuttonpopup.Visible = true;
                resourcebuttonpopup.Visible = true;
            }
            SetControls();
            //if (this.PrimaryServiceType == "PartRequest")
            //{
            //    materiallistpanel.ClearData();
            //}
            FetchSelectionGridData();
        }

        public void ResourceField_DataChanged(object sender, EventArgs e)
        {
            if (resource.Data != null)
            {
                resourcebuttonpopup.Visible = true;
                var fs = FrameworkManagerUtil.GetFrameworkSession();
                UserProfile profile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
                ResultStatus res = new ResultStatus(null, false);

                PartTxnService Svc = new PartTxnService(fs.CurrentUserProfile);
                PartTxn SvcData = new PartTxn();
                PartTxn_Info SvcInfo = new PartTxn_Info();
                PartTxn_Request ReqData = new PartTxn_Request();
                PartTxn_Result ResData = new PartTxn_Result();

                SvcData.Resource = new NamedObjectRef();
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

                materiallistpanel.ClearData();
                resourcepartspanel.ClearData();
                FetchSelectionGridData();
            }
            else
            {
                resourcebuttonpopup.Visible = false;
            }
        }

        public override void GetInputData(OM.Service serviceData)
        {
            base.GetInputData(serviceData);
            if ((this.PrimaryServiceType == "PartRequest" || this.PrimaryServiceType == "PartRequestUpdate") && requesttypefield.Text == "Request")
            {
                int gridCount = materiallistpanel.GridContext.GetTotalRows();
                int rowCount = 0;

                DataTable getDataTable;
                (materiallistpanel.GridContext as BoundContext).GenerateFullExcelData(gridCount, out getDataTable);

                if (gridCount > 0)
                {
                    for (int i = 0; i < gridCount; i++)
                    {
                        if (getDataTable.Rows[i].Field<String>("MaterialPart") != null && getDataTable.Rows[i].Field<String>("RequestPartQty") != null)
                            rowCount = rowCount + 1;
                    }
                }

                if (rowCount > 0)
                {
                    if (serviceData is OM.PartRequest || serviceData is OM.PartRequestUpdate)
                    {
                        (serviceData as OM.PartTxn).ServiceDetails = new PartTxnServiceDetail[rowCount];
                        int x = 0;
                        for (int i = 0; i < gridCount; i++)
                        {
                            if (getDataTable.Rows[i].Field<String>("MaterialPart") != null && getDataTable.Rows[i].Field<String>("RequestPartQty") != null)
                            {
                                string materialpart = getDataTable.Rows[i].Field<String>("MaterialPart").ToString();
                                string requestpartqty = "0";
                                string[] splitmaterialpart = materialpart.Split(':');
                                if (getDataTable.Rows[i].Field<String>("RequestPartQty") != null)
                                {
                                    requestpartqty = getDataTable.Rows[i].Field<String>("RequestPartQty").ToString();
                                }
                                (serviceData as OM.PartTxn).ServiceDetails[x] = new PartTxnServiceDetail();
                                (serviceData as OM.PartTxn).ServiceDetails[x].MaterialPart = new RevisionedObjectRef { Name = splitmaterialpart[0], Revision = splitmaterialpart[1] };
                                (serviceData as OM.PartTxn).ServiceDetails[x].RequestPartQty = Convert.ToInt32(requestpartqty);
                                x = x + 1;
                            }
                        }
                    }
                }
            }
            else if (this.PrimaryServiceType == "PartRequest" && requesttypefield.Text == "Return")
            {
                int gridCount = resourcepartspanel.GridContext.GetTotalRows();
                int rowCount = 0;

                DataTable getDataTable;
                (resourcepartspanel.GridContext as BoundContext).GenerateFullExcelData(gridCount, out getDataTable);

                if (gridCount > 0)
                {
                    for (int i = 0; i < gridCount; i++)
                    {
                        if (getDataTable.Rows[i].Field<String>("MaterialPart") != null && getDataTable.Rows[i].Field<String>("RequestPartQty") != null)
                            rowCount = rowCount + 1;
                    }
                }

                if (rowCount > 0)
                {
                    if (serviceData is OM.PartRequest || serviceData is OM.PartRequestUpdate)
                    {
                        (serviceData as OM.PartTxn).ServiceDetails = new PartTxnServiceDetail[rowCount];
                        int x = 0;
                        for (int i = 0; i < gridCount; i++)
                        {
                            if (getDataTable.Rows[i].Field<String>("MaterialPart") != null && getDataTable.Rows[i].Field<String>("RequestPartQty") != null)
                            {
                                string partname = getDataTable.Rows[i].Field<String>("PartName").ToString();
                                string partqty = getDataTable.Rows[i].Field<String>("RequestPartQty").ToString();
                                string materialpart = getDataTable.Rows[i].Field<String>("MaterialPart").ToString();
                                string[] splitmaterialpart = materialpart.Split(':');

                                (serviceData as OM.PartTxn).ServiceDetails[x] = new PartTxnServiceDetail();
                                (serviceData as OM.PartTxn).ServiceDetails[x].PartName = partname;
                                (serviceData as OM.PartTxn).ServiceDetails[x].PartQty = Convert.ToInt32(partqty);
                                (serviceData as OM.PartTxn).ServiceDetails[x].MaterialPart = new RevisionedObjectRef { Name = splitmaterialpart[0], Revision = splitmaterialpart[1] };
                                x = x + 1;
                            }
                        }
                    }
                }
            }
        }

        public override void ClearValues(Service serviceData)
        {
            string _saveResource = string.Empty;
            string _saveOrder = string.Empty;

            if (resource.ReadOnly)
            {
                if (string.IsNullOrEmpty(_saveResource) && resource.Data != null)
                    _saveResource = resource.Data.ToString();
                if (string.IsNullOrEmpty(_saveOrder) && joborderfield.Data != null)
                    _saveOrder = joborderfield.Data.ToString();
            }
            base.ClearValues(serviceData);

            if (!string.IsNullOrEmpty(_saveResource))
                resource.Data = _saveResource;
            if (!string.IsNullOrEmpty(_saveOrder))
                joborderfield.Data = _saveOrder;
        }

        //protected override void OnPreRender(EventArgs e)
        //{
        //    base.OnPreRender(e);
        //    this.Page.PrimaryServiceType = svctype.Data.ToString();
        //    if (!Page.IsPostBack)
        //    {
        //        var startupScript1 = string.Format("ES.Utility.showFullScreenPopup();");
        //        if (!Page.ClientScript.IsStartupScriptRegistered("initializePage"))
        //        {
        //            ScriptManager.RegisterStartupScript(this, this.GetType(), "initializePage", startupScript1, true);
        //        }
        //    }
        //}

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!Page.IsPostBack)
            {
                resourcebuttonpopup.Visible = false;
                jobhistorybuttonpopup.Visible = false;
            }

            if (!Page.IsPostBack && Page.IsAJAXFloatingFrame)
                ScriptManager.RegisterStartupScript(this.Page.Form, this.Page.GetType(), "PopUpMaximize", "pop.GetCallerPage().pop.maximize();", true);

            if (readonlyinfo.Data != null)
            {
                if (!readonlyinfo.Data.ToString().Equals("True"))
                {
                    resource.DataChanged += new EventHandler(ResourceField_DataChanged);
                    joborderfield.DataChanged += new EventHandler(JobOrderField_DataChanged);
                }
                else if (readonlyinfo.Data.ToString().Equals("True"))
                {
                    joborderfield.ReadOnly = true;
                }
            }
            else if (readonlyinfo.Data == null)
            {
                resource.DataChanged += new EventHandler(ResourceField_DataChanged);
                joborderfield.DataChanged += new EventHandler(JobOrderField_DataChanged);
            }

            requesttypefield.DataChanged += new EventHandler(RequestType_DataChanged);

            CamstarWebControl.SetRenderToClient(resourcebutton);
            CamstarWebControl.SetRenderToClient(jobhistorybutton);
            CamstarWebControl.SetRenderToClient(requesthistorybutton);

            ForceRequestType();
            SetControls();
        }
    }
}
