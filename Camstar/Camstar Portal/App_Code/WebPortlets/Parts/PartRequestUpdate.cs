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

/// <summary>
/// Summary description for PartRequestUpdate
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Parts
{
    public class PartRequestUpdate : MatrixWebPart, IPostBackEventHandler
    {
        private CWC.TextBox svctype { get { return Page.FindCamstarControl("ServiceType") as CWC.TextBox; } }
        private CWC.TextBox readonlyinfo { get { return Page.FindCamstarControl("ReadOnlyInfo") as CWC.TextBox; } }

        private CWC.NamedObject resource { get { return Page.FindCamstarControl("ResourceField") as CWC.NamedObject; } }
        private CWC.Button resourcebutton { get { return Page.FindCamstarControl("ResourcePartsButton") as CWC.Button; } }
        private CWC.NamedObject joborderfield { get { return Page.FindCamstarControl("JobOrderField") as CWC.NamedObject; } }
        private CWC.Button jobhistorybutton { get { return Page.FindCamstarControl("JobHistoryButton") as CWC.Button; } }
        private CWC.NamedObject requestorderfield { get { return Page.FindCamstarControl("RequestOrderField") as CWC.NamedObject; } }
        private CWC.Button requesthistorybutton { get { return Page.FindCamstarControl("RequestHistoryButton") as CWC.Button; } }
        private CWC.CheckBox requireemail { get { return Page.FindCamstarControl("RequireAcknowledgeEmailField") as CWC.CheckBox; } }
        private CWC.DropDownList requesttypefield { get { return Page.FindCamstarControl("RequestTypeField") as CWC.DropDownList; } }

        private CWC.Button addselectedpartbutton { get { return Page.FindCamstarControl("AddSelectedPartButton") as CWC.Button; } }
        private JQDataGrid materiallistpanel { get { return Page.FindCamstarControl("MaterialListPanel") as JQDataGrid; } }
        private JQDataGrid resourcepartspanel { get { return Page.FindCamstarControl("ResourcePartsPanel") as JQDataGrid; } }
        private JQDataGrid hiddenresourcepartspanel { get { return Page.FindCamstarControl("HiddenResourcePartsPanel") as JQDataGrid; } }
        private JQDataGrid selectionvaluepanel { get { return Page.FindCamstarControl("SelectionValuePanel") as JQDataGrid; } }
        private JQDataGrid servicedetailspanel { get { return Page.FindCamstarControl("ServiceDetailsPanel") as JQDataGrid; } }
        private JQDataGrid requestorderpanel { get { return Page.FindCamstarControl("RequestOrderMaterialPartsPanel") as JQDataGrid; } }
        private CWC.CheckBox autocompletefield { get { return Page.FindCamstarControl("AutoCompleteField") as CWC.CheckBox; } }
        private CWC.TextBox _txtComments { get { return Page.FindCamstarControl("Shopfloor_Comments") as CWC.TextBox; } }
        private Camstar.WebPortal.PortalFramework.ToggleContainer _togComments { get { return Page.FindCamstarControl("PartRequestComments_Toggle") as Camstar.WebPortal.PortalFramework.ToggleContainer; } }

        private DataTable myresourcepartspanel = new DataTable();

        public PartRequestUpdate()
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
            if (this.PrimaryServiceType == "PartRequestUpdate")
            {
                if (readonlyinfo.Data != null)
                {
                    resource.ReadOnly = true;
                    joborderfield.ReadOnly = true;
                    requestorderfield.ReadOnly = true;
                    requesttypefield.ReadOnly = true;
                }

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

        public void SetGridButtons()
        {
            if (requesttypefield.Data == null || requesttypefield.Text == "Request")
            {
                materiallistpanel.Settings.NavigatorActions[0].Enable = false;
                materiallistpanel.Settings.NavigatorActions[0].Visible = false;
                materiallistpanel.Settings.NavigatorActions[2].Enable = false;
                materiallistpanel.Settings.NavigatorActions[2].Visible = false;
                resourcepartspanel.Settings.NavigatorActions[0].Enable = false;
                resourcepartspanel.Settings.NavigatorActions[0].Visible = false;
                resourcepartspanel.Settings.NavigatorActions[2].Enable = false;
                resourcepartspanel.Settings.NavigatorActions[2].Visible = false;
            }
            else
            {
                materiallistpanel.Settings.NavigatorActions[0].Enable = true;
                materiallistpanel.Settings.NavigatorActions[0].Visible = true;
                materiallistpanel.Settings.NavigatorActions[2].Enable = true;
                materiallistpanel.Settings.NavigatorActions[2].Visible = true;
                resourcepartspanel.Settings.NavigatorActions[0].Enable = true;
                resourcepartspanel.Settings.NavigatorActions[0].Visible = true;
                resourcepartspanel.Settings.NavigatorActions[2].Enable = true;
                resourcepartspanel.Settings.NavigatorActions[2].Visible = true;
            }
        }

        public void UpdateGridDataWithSelection()
        {
            if (resource.Data != null && requesttypefield.Data != null)
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
                cdo.SetValue("RequestType", requesttypefield.Data as DropDownList);

                // Set Request Value
                if (this.PrimaryServiceType == "PartRequestUpdate")
                {
                    if (requesttypefield.Data.ToString() == "1")
                    {
                        reqInfo.SetValue("RequestOrderMaterialParts", new OM.PartRequestOrderMaterialPart_Info());
                        reqInfo.SetValue("RequestOrderMaterialParts.MaterialPart", new OM.Info(true));
                        reqInfo.SetValue("RequestOrderMaterialParts.RequestPartQty", new OM.Info(true));
                        reqInfo.SetValue("RequestOrderMaterialParts.AssignPartQty", new OM.Info(true));
                    }
                    else if (requesttypefield.Data.ToString() == "2")
                    {
                        reqInfo.SetValue("RequestOrderParts", new OM.PartRequestOrderPart_Info());
                        reqInfo.SetValue("RequestOrderParts.MaterialPart", new OM.Info(true));
                        reqInfo.SetValue("RequestOrderParts.PartName", new OM.Info(true));
                        reqInfo.SetValue("RequestOrderParts.PartQty", new OM.Info(true));
                    }
                    request.SetValue("Info", reqInfo);

                    // Execute Request 
                    ResultStatus rslt = service.GetEnvironment(cdo as DCObject, request as Request, out result);

                    // Result
                    if (rslt.IsSuccess && requesttypefield.Data.ToString() == "1")
                    {
                        PartRequestOrderMaterialPart[] collectedData = ((result as ICreator).GetValue("Value.RequestOrderMaterialParts") as PartRequestOrderMaterialPart[]);
                        
                        int gridCount = materiallistpanel.GridContext.GetTotalRows();
                        DataTable getDataTable;
                        (materiallistpanel.GridContext as BoundContext).GenerateFullExcelData(gridCount, out getDataTable);
                        
                        var newSvcDetails = new List<PartTxnServiceDetail>();
                        int rowCount = 0;
                        if (collectedData != null)
                            rowCount = collectedData.Count();
                        
                        var selectedMatParts = new List<String>();
                        var requestPartQty = new List<String>();
                        var assignPartQty = new List<String>();
                        for (int i = 0; i < rowCount; i++)
                        {
                            selectedMatParts.Add(collectedData[i].MaterialPart.ToString());
                            requestPartQty.Add(collectedData[i].RequestPartQty.ToString());
                            assignPartQty.Add(collectedData[i].AssignPartQty.ToString());
                            for (int x = 0; x < gridCount; x++)
                            {
                                if (getDataTable.Rows[x].Field<String>("MaterialPart").ToString() == collectedData[i].MaterialPart.ToString())
                                {
                                    getDataTable.Rows[x].SetField<String>("RequestPartQty", collectedData[i].RequestPartQty.ToString());
                                    getDataTable.Rows[x].SetField<String>("AssignPartQty", collectedData[i].AssignPartQty.ToString());
                                }
                            }
                        }

                        for (int i = 0; i < gridCount; i++)
                        {
                            string materialpart = getDataTable.Rows[i].Field<String>("MaterialPart").ToString();
                            string[] splitmaterialpart = materialpart.Split(':');
                            if ((getDataTable.Rows[i].Field<String>("RequestPartQty") == null) && (getDataTable.Rows[i].Field<String>("MaintenanceReq") == null))
                            {
                                newSvcDetails.Add(new PartTxnServiceDetail
                                {
                                    MaterialPart = new RevisionedObjectRef { Name = splitmaterialpart[0], Revision = splitmaterialpart[1] },
                                    RequiredPartQty = Convert.ToInt32(getDataTable.Rows[i].Field<String>("RequiredPartQty").ToString()),
                                    PartQty = Convert.ToInt32(getDataTable.Rows[i].Field<String>("PartQty").ToString())
                                });
                            }
                            else
                            {
                                newSvcDetails.Add(new PartTxnServiceDetail
                                {
                                    MaterialPart = new RevisionedObjectRef { Name = splitmaterialpart[0], Revision = splitmaterialpart[1] },
                                    RequiredPartQty = Convert.ToInt32(getDataTable.Rows[i].Field<String>("RequiredPartQty").ToString()),
                                    PartQty = Convert.ToInt32(getDataTable.Rows[i].Field<String>("PartQty").ToString()),
                                    RequestPartQty = Convert.ToInt32(getDataTable.Rows[i].Field<String>("RequestPartQty").ToString()),
                                    AssignPartQty = Convert.ToInt32(getDataTable.Rows[i].Field<String>("AssignPartQty").ToString())
                                });
                            }
                        }

                        materiallistpanel.Data = newSvcDetails.ToArray();
                        materiallistpanel.OriginalData = newSvcDetails.ToArray();
                        materiallistpanel.GridContext.SelectedRowIDs = selectedMatParts;

                        (materiallistpanel.GridContext as BoundContext).Settings.Columns[0].IsRowID = false;
                    }
                    else if (rslt.IsSuccess && requesttypefield.Data.ToString() == "2")
                    {
                        PartRequestOrderPart[] collectedData = ((result as ICreator).GetValue("Value.RequestOrderParts") as PartRequestOrderPart[]);

                        bool matched = false;
                        int gridCount = hiddenresourcepartspanel.GridContext.GetTotalRows();
                        DataTable getDataTable;
                        (hiddenresourcepartspanel.GridContext as BoundContext).GenerateFullExcelData(gridCount, out getDataTable);

                        var newSvcDetails = new List<PartTxnServiceDetail>();
                        int rowCount = 0;
                        if (collectedData != null)
                            rowCount = collectedData.Count();

                        for (int i = 0; i < rowCount; i++)
                        {
                            int currentqty = 0;
                            for (int x = 0; x < gridCount; x++)
                            {
                                if (getDataTable.Rows[x].Field<String>("PartName").ToString() == collectedData[i].PartName.ToString())
                                {
                                    matched = true;
                                    currentqty = Convert.ToInt32(getDataTable.Rows[x].Field<String>("PartQty").ToString());
                                    break;
                                }
                            }

                            newSvcDetails.Add(new PartTxnServiceDetail
                            {
                                Part = new NamedObjectRef { Name = collectedData[i].PartName.ToString() },
                                PartQty = currentqty,
                                MaterialPart = collectedData[i].MaterialPart,
                                RequestPartQty = collectedData[i].PartQty
                            });
                        }

                        resourcepartspanel.ClearData();
                        resourcepartspanel.Data = newSvcDetails.ToArray();
                        resourcepartspanel.OriginalData = newSvcDetails.ToArray();
                    }
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

                PartRequestUpdateService Svc = new PartRequestUpdateService(fs.CurrentUserProfile);
                Camstar.WCF.ObjectStack.PartRequestUpdate SvcData = new Camstar.WCF.ObjectStack.PartRequestUpdate();
                PartRequestUpdate_Info SvcInfo = new PartRequestUpdate_Info();
                PartRequestUpdate_Request ReqData = new PartRequestUpdate_Request();
                PartRequestUpdate_Result ResData = new PartRequestUpdate_Result();

                SvcData.Resource = new NamedObjectRef();
                if (resource.Data != null)
                    SvcData.Resource.Name = resource.Data.ToString();

                SvcData.JobOrder = new NamedObjectRef();
                if (joborderfield.Data != null)
                    SvcData.JobOrder.Name = joborderfield.Data.ToString();

                SvcData.RequestOrder = new NamedObjectRef();
                if (requestorderfield.Data != null)
                    SvcData.RequestOrder.Name = requestorderfield.Data.ToString();

                if (resource.Data != null && requestorderfield.Data != null)
                {
                    SvcInfo.RequestType = new Info(true);
                    SvcInfo.RequestStatus = new Info(true);
                    SvcInfo.MaterialList = new PartTxnServiceDetail_Info();
                    SvcInfo.MaterialList.MaterialPart = new Info(true);
                    SvcInfo.MaterialList.RequiredPartQty = new Info(true);
                    SvcInfo.MaterialList.MaintenanceReq = new Info(true);
                    SvcInfo.MaterialList.PartQty = new Info(true);
                    SvcInfo.ResourceParts = new ResourcePart_Info();
                    SvcInfo.ResourceParts.PartName = new Info(true);
                    SvcInfo.ResourceParts.PartQty = new Info(true);
                    SvcInfo.ResourceParts.MaterialPart = new Info(true);

                    ReqData.Info = SvcInfo;
                    OM.ResultStatus Results = Svc.Load(SvcData, ReqData, out ResData);
                    if (Results.IsSuccess)
                        requesttypefield.Data = ResData.Value.RequestType.Value;

                    if (Results.IsSuccess && ResData.Value.RequestType.Value == 1)
                    {
                        materiallistpanel.ClearData();
                        materiallistpanel.Data = ResData.Value.MaterialList;
                        materiallistpanel.OriginalData = ResData.Value.MaterialList;
                    }
                    else if (Results.IsSuccess && requesttypefield.Text == "Return")
                    {
                        hiddenresourcepartspanel.ClearData();
                        hiddenresourcepartspanel.Data = ResData.Value.ResourceParts;
                        hiddenresourcepartspanel.OriginalData = ResData.Value.ResourceParts;

                        int resourcepartssavedCount = hiddenresourcepartspanel.GridContext.GetTotalRows();
                        (hiddenresourcepartspanel.GridContext as BoundContext).GenerateFullExcelData(resourcepartssavedCount, out myresourcepartspanel);
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

        public void ResourceField_DataChanged(object sender, EventArgs e)
        {
            if (this.PrimaryServiceType == "PartRequestUpdate")
            {
                var fs = FrameworkManagerUtil.GetFrameworkSession();
                UserProfile profile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
                ResultStatus res = new ResultStatus(null, false);

                PartRequestUpdateService Svc = new PartRequestUpdateService(fs.CurrentUserProfile);
                Camstar.WCF.ObjectStack.PartRequestUpdate SvcData = new Camstar.WCF.ObjectStack.PartRequestUpdate();
                PartRequestUpdate_Info SvcInfo = new PartRequestUpdate_Info();
                PartRequestUpdate_Request ReqData = new PartRequestUpdate_Request();
                PartRequestUpdate_Result ResData = new PartRequestUpdate_Result();

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
                    materiallistpanel.ClearData();
                    resourcepartspanel.ClearData();
                }
            }
        }

        public void JobOrderField_DataChanged(object sender, EventArgs e)
        {
            var fs = FrameworkManagerUtil.GetFrameworkSession();
            UserProfile profile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
            ResultStatus res = new ResultStatus(null, false);

            PartRequestUpdateService Svc = new PartRequestUpdateService(fs.CurrentUserProfile);
            Camstar.WCF.ObjectStack.PartRequestUpdate SvcData = new Camstar.WCF.ObjectStack.PartRequestUpdate();
            PartRequestUpdate_Info SvcInfo = new PartRequestUpdate_Info();
            PartRequestUpdate_Request ReqData = new PartRequestUpdate_Request();
            PartRequestUpdate_Result ResData = new PartRequestUpdate_Result();

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
            requesttypefield.Data = null;
            materiallistpanel.ClearData();
            resourcepartspanel.ClearData();
            FetchSelectionGridData();
            UpdateGridDataWithSelection();
            SetControls();
            SetGridButtons();
        }

        protected void ResourcePartsPanel_GetRowSnapItem(object item, IEnumerable<DataColumn> dataColumns, DataRow row)
        {
            if (item is OM.PartTxnServiceDetail)
            {
                if (row["Part"] != System.DBNull.Value)
                {
                    int savedCount = hiddenresourcepartspanel.GridContext.GetTotalRows();
                    for (int i = 0; i < savedCount; i++)
                    {
                        if (row["Part"].ToString() == myresourcepartspanel.Rows[i].Field<String>("PartName").ToString())
                        {
                            row["Part_Qty"] = myresourcepartspanel.Rows[i].Field<String>("PartQty");
                            row["Mat_Part"] = myresourcepartspanel.Rows[i].Field<String>("MaterialPart");
                        }
                    }
                }
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
                        if (getDataTable.Rows[i].Field<String>("RequestPartQty") != null)
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
                            if (getDataTable.Rows[i].Field<String>("RequestPartQty") != null)
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
            else if (this.PrimaryServiceType == "PartRequestUpdate" && requesttypefield.Text == "Return")
            {
                int gridCount = resourcepartspanel.GridContext.GetTotalRows();
                DataTable getDataTable;
                (resourcepartspanel.GridContext as BoundContext).GenerateFullExcelData(gridCount, out getDataTable);
                if (gridCount > 0)
                {
                    if (serviceData is OM.PartRequest || serviceData is OM.PartRequestUpdate)
                    {
                        (serviceData as OM.PartTxn).ServiceDetails = new PartTxnServiceDetail[gridCount];

                        if (requesttypefield.Text == "Return")
                        {
                            for (int i = 0; i < gridCount; i++)
                            {
                                string partname = resourcepartspanel.GridContext.GetCell(i, "Part").ToString();
                                string partqty = resourcepartspanel.GridContext.GetCell(i, "PartQty_Return").ToString();
                                string materialpart = resourcepartspanel.GridContext.GetCell(i, "Mat_Part").ToString();
                                string[] splitmaterialpart = materialpart.Split(':');

                                if (partqty == "")
                                {
                                    partqty = "0";
                                }

                                if (partname != "")
                                {
                                    (serviceData as OM.PartTxn).ServiceDetails[i] = new PartTxnServiceDetail();
                                    (serviceData as OM.PartTxn).ServiceDetails[i].PartName = partname;
                                    (serviceData as OM.PartTxn).ServiceDetails[i].PartQty = Convert.ToInt32(partqty);
                                    (serviceData as OM.PartTxn).ServiceDetails[i].MaterialPart = new RevisionedObjectRef { Name = splitmaterialpart[0], Revision = splitmaterialpart[1] };
                                }
                            }
                        }
                    }
                }
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            this.Page.PrimaryServiceType = svctype.Data.ToString();
        
            if (!Page.IsPostBack)
            {
                ScriptManager.RegisterStartupScript(this.Page.Form, this.Page.GetType(), "PopUpMaximize", "pop.GetCallerPage().pop.maximize();", true);
            }
        }
        
        //protected override IEnumerable<ScriptReference> GetScriptReferences()
        //{
        //    yield return new ScriptReference("~/Scripts/user/Namespace.js");
        //    yield return new ScriptReference("~/Scripts/user/Utility.js");
        //}

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            (resourcepartspanel.GridContext as ItemDataContext).GetRowSnapItem += ResourcePartsPanel_GetRowSnapItem;

            if (readonlyinfo.Data != null)
            {
                if (readonlyinfo.Data.ToString() != "True")
                {
                    //resource.DataChanged += new EventHandler(ResourceField_DataChanged);
                }
                else if (readonlyinfo.Data.ToString() == "True")
                {
                    requestorderfield.DataChanged += new EventHandler(RequestOrderField_DataChanged);
                    if (!Page.IsPostBack)
                    {
                        JobOrderField_DataChanged("", e);
                    }
                }
            }
            else if (readonlyinfo.Data == null)
            {
                resource.DataChanged += new EventHandler(ResourceField_DataChanged);
                joborderfield.DataChanged += new EventHandler(JobOrderField_DataChanged);
                requestorderfield.DataChanged += new EventHandler(RequestOrderField_DataChanged);
            }


            if (!Page.IsPostBack && resource.Data != null)
            {
                FetchSelectionGridData();
                UpdateGridDataWithSelection();
            }
            this.Page.RenderToClient = true;
            Page.Service.LoadData();

            ForceRequestType();
            SetControls();
            SetGridButtons();

            //(FindControl("MaterialPart_InlineEditorControl") as CWC.RevisionedObject).RDOFormat = RDOFormatType.Revision;

            int resourcepartssavedCount = hiddenresourcepartspanel.GridContext.GetTotalRows();
            (hiddenresourcepartspanel.GridContext as BoundContext).GenerateFullExcelData(resourcepartssavedCount, out myresourcepartspanel);

            if (!Page.IsPostBack && Page.IsAJAXFloatingFrame)
                ScriptManager.RegisterStartupScript(this.Page.Form, this.Page.GetType(), "PopUpMaximize", "pop.GetCallerPage().pop.maximize();", true);
        }

        //---------------------------------
        // Web part custom action
        //---------------------------------
        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e) //Custom action settings
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as CustomAction;
            if (action != null)
            {
                if (action.Parameters == "ResetFields")
                {
                    if (readonlyinfo.Data != null)
                    {
                        RequestOrderField_DataChanged(sender, e);  
                        _txtComments.ClearData();
                        _togComments.Reset();
                    }                
                    else
                        Page.ShopfloorReset(sender, e);
                }
            }
        }
    }
}
