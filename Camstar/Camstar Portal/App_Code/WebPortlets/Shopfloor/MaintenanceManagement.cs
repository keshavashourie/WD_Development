// © 2023 Siemens Product Lifecycle Management Software Inc.														 
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
using Newtonsoft.Json;
using Camstar.WebPortal.WCFUtilities;
using WC = CamstarPortal.WebControls;
using PERS = Camstar.WebPortal.Personalization;

/// <summary>
/// Summary description for RLWPTwoLevelStart
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class MaintenanceManagement : MatrixWebPart, IPostBackEventHandler
    {

        #region Property
        private JQDataGrid ctrStatusDetailsGrid { get { return Page.FindCamstarControl("GetMaintenanceStatuses_StatusDetails") as JQDataGrid; } }
        private JQDataGrid ctrChecklistGrid { get { return Page.FindCamstarControl("GridChecklist") as JQDataGrid; } }
        private WC.ShopFloorDCControl ctrParametricData { get { return Page.FindCamstarControl("ctrParametricData") as WC.ShopFloorDCControl; } }
        private CWC.RevisionedObject hdnDataCollection { get { return Page.FindCamstarControl("hdnDataCollection") as CWC.RevisionedObject; } }
        private CWC.NamedObject ndoResource { get { return Page.FindCamstarControl("CompleteMaintenance_Resource") as CWC.NamedObject; } }
        private CWC.RevisionedObject maintReq { get { return Page.FindCamstarControl("MaintReq") as CWC.RevisionedObject; } }
        private CWC.DateChooser nextDue { get { return Page.FindCamstarControl("StatusDetails_NextDateDue") as CWC.DateChooser; } }
        private CWC.DateChooser nextWarning { get { return Page.FindCamstarControl("StatusDetails_NextDateWarning") as CWC.DateChooser; } }
        private CWC.DateChooser nextLimit { get { return Page.FindCamstarControl("StatusDetails_NextDateLimit") as CWC.DateChooser; } }
        private CWC.TextBox nextQtyDue { get { return Page.FindCamstarControl("StatusDetails_NextThruputQtyDue") as CWC.TextBox; } }
        private CWC.TextBox nextQtyLimit { get { return Page.FindCamstarControl("StatusDetails_NextThruputQtyLimit") as CWC.TextBox; } }
        private CWC.TextBox nextQtyWarning { get { return Page.FindCamstarControl("StatusDetails_NextThruputQtyWarning") as CWC.TextBox; } }
        private CWC.TextBox thruputQty { get { return Page.FindCamstarControl("StatusDetails_ThruputQty") as CWC.TextBox; } }
        private CWC.NamedObject ndoUOM { get { return Page.FindCamstarControl("StatusDetails_UOM") as CWC.NamedObject; } }
        private CWC.CheckBox flagForceMaintenance { get { return Page.FindCamstarControl("CompleteMaintenance_ForceMaintenance") as CWC.CheckBox; } }





        #endregion Property

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (Page.EventArgument.Contains("OnRowSelected"))
            {
                if (Page.EventTarget.Contains("_StatusDetails"))
                {
                    if (ctrStatusDetailsGrid.IsRowSelected && ctrStatusDetailsGrid.GridContext.SelectedRowIDs.Count() > 0)
                    {
                        System.Data.DataRow rowSelected = ctrStatusDetailsGrid.GridContext.GetSelectedItems(false)[0] as System.Data.DataRow;
                        string maintStatus = rowSelected["MaintenanceStatus"].ToString();
                        string resource = rowSelected["ResourceName"].ToString();
                        if (resource != "" && maintStatus != "")
                            PopulateChecklistGrid(resource, maintStatus);
                    }
                }

                var varItemDataContext = (ctrChecklistGrid.GridContext as ItemDataContext);
                bool haveCheckList = varItemDataContext != null && varItemDataContext.GetTotalRows() > 0;

                if (isChecklistCompleted() && !ctrChecklistGrid.IsRowSelected)
                {
                    haveCheckList = false;
                }

                if ((Page.EventTarget.Contains("GridChecklist") || !haveCheckList) && ctrStatusDetailsGrid.IsRowSelected)
                {
                    PerfomResolveParametricData(haveCheckList);
                }
                else
                {
                    CheckStatusDetailsGrid(true);
                }
            }
            else if (Page.IsPostBack)
            {
                CheckStatusDetailsGrid(false);
            }
            ctrChecklistGrid.GridContext.RowSelected += new JQGridEventHandler(ChecklistGrid_RowSelected);
        }

        protected void CheckStatusDetailsGrid(bool clearDataCollection)
        {
            if (ctrStatusDetailsGrid.IsRowSelected && !ctrChecklistGrid.IsRowSelected)
            {
                if (ctrStatusDetailsGrid.GridContext.SelectedRowIDs.Count() > 0)
                {
                    System.Data.DataRow rowSelected = ctrStatusDetailsGrid.GridContext.GetSelectedItems(false)[0] as System.Data.DataRow;
                    string maintStatus = rowSelected["MaintenanceStatus"].ToString();
                    string resource = rowSelected["ResourceName"].ToString();
                    if (!string.IsNullOrEmpty(resource) && !string.IsNullOrEmpty(maintStatus))
                        PopulateChecklistGrid(resource, maintStatus);
                }
                if (clearDataCollection)
                {
                    hdnDataCollection.ClearData();
                    ctrParametricData.Clean();
                }
            }
            else if (!ctrStatusDetailsGrid.IsRowSelected)
            {
                ClearData();
            }
        }

        protected void ClearData()
        {
            ndoResource.ClearData();
            maintReq.ClearData();
            nextDue.ClearData();
            nextLimit.ClearData();
            nextQtyDue.ClearData();
            nextQtyLimit.ClearData();
            nextQtyWarning.ClearData();
            nextWarning.ClearData();
            ndoUOM.ClearData();
            ctrChecklistGrid.ClearData();
            ctrChecklistGrid.Data = null;
            ctrChecklistGrid.OriginalData = null;
            hdnDataCollection.ClearData();
            ctrParametricData.Clean();
            ctrStatusDetailsGrid.SelectedRowID = null;
            flagForceMaintenance.ClearData();
        }
        #region Page Event

        public MaintenanceManagement()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);



        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            Page.OnPreExecute += new EventHandler<FormProcessingEventArgs>(Page_OnPreExecute);
            Page.OnPostExecute += new EventHandler<ResultEventArgs>(Page_OnPostExecute);

        }

        void Page_OnPreExecute(object sender, FormProcessingEventArgs e)
        {
            OM.Info serviceInfo = e.Info;
            OM.Service serviceData = e.Data;
            if (ctrStatusDetailsGrid.GridContext.GetSelectedItems(false).Length == 0)
            {
                Label lab = FrameworkManagerUtil.GetLabelCache().GetLabelByName("Lbl_NoRowSelected");
                string msg = lab != null ? lab.Value : "You must select a row!";
                Page.DisplayMessage(msg, false);
                e.Result = false;
            }
        }

        void Page_OnPostExecute(object sender, ResultEventArgs e)
        {
            bool isSaveSelectedRow = !ctrStatusDetailsGrid.IsRowSelected && !string.IsNullOrEmpty(saveSelectedRow);
            if (isSaveSelectedRow)
            {
                ctrStatusDetailsGrid.GridContext.SelectRow(saveSelectedRow, true);
                PopulateChecklistGrid(saveResource, saveMaintStatus);
            }

            if (e.Status.IsSuccess)
                ClearData();

            hdnDataCollection.ClearData();
            ctrParametricData.Clean();

            if (ctrStatusDetailsGrid.GridContext.SelectedRowIDs.Count() > 0 && isSaveSelectedRow)
            {
                if (isChecklistCompleted() && !ctrChecklistGrid.IsRowSelected)
                {
                    PerfomResolveParametricData(false);
                }
            }
        }

        public void RaisePostBackEvent(string eventArgument)
        {

        }

        private string saveMaintStatus = "";
        private string saveResource = "";
        private string saveSelectedRow = "";
        public override void GetInputData(OM.Service serviceData)
        {
            base.GetInputData(serviceData);

            try
            {
                if (ctrStatusDetailsGrid.GridContext.GetSelectedItems(false).Length > 0)
                {
                    System.Data.DataRow rowSelected = ctrStatusDetailsGrid.GridContext.GetSelectedItems(false)[0] as System.Data.DataRow;
                    saveSelectedRow = ctrStatusDetailsGrid.GridContext.SelectedRowIDs[0];
                    saveMaintStatus = rowSelected["MaintenanceStatus"].ToString();
                    saveResource = rowSelected["ResourceName"].ToString();
                    if (string.IsNullOrEmpty(saveResource) && ndoResource.Data != null)
                        saveResource = ndoResource.Data.ToString();
                    CompleteMaintenance sd = (serviceData as CompleteMaintenance);
                    if (!string.IsNullOrEmpty(saveResource))
                        sd.Resource = new NamedObjectRef(saveResource);
                    if (ctrChecklistGrid.GridContext.GetSelectedCount() > 0 || !string.IsNullOrEmpty(saveMaintStatus))
                    {
                        sd.ServiceDetails = new CompleteMaintDetails[]
                                                  {
                                               new CompleteMaintDetails
                                                   {
                                                     MaintenanceStatus = new SubentityRef(saveMaintStatus),
                                                     Checklist = new MaintenanceReqChecklistChanges[ctrChecklistGrid.GridContext.GetSelectedCount()]
                                                   }
                                                  };
                    }
                    if (ctrChecklistGrid.GridContext.GetSelectedCount() > 0)
                    {
                        MaintenanceReqChecklistChanges[] check = sd.ServiceDetails[0].Checklist;
                        var CheckSelected = ctrChecklistGrid.GridContext.GetSelectedItems(false).ToList();
                        foreach (MaintenanceReqChecklistChanges it in CheckSelected)
                        {
                            int index = CheckSelected.IndexOf(it);
                            if (index >= 0 && index < check.Length)
                            {
                                check[index] = new MaintenanceReqChecklistChanges();
                                check[index].ChecklistId = it.ChecklistId != null ? it.ChecklistId.Value : null;
                                check[index].Instruction = it.Instruction != null ? it.Instruction.Value : null;
                                check[index].ListItemAction = ListItemAction.Add;
                                if (it.Comments != null)
                                    if (it.Comments.Value != null)
                                        check[index].ChecklistId = it.Comments.Value;
                            }
                        }
                    }
                    if (ctrParametricData != null)
                    {
                        DataPointSummary[] dataPointSummary = ctrParametricData.GetDataPointSummary();
                        if (dataPointSummary != null && dataPointSummary.Length > 0)
                            ((ShopFloor)serviceData).ParametricData = dataPointSummary[0];
                    }
                }
            }
            catch (Exception ex)
            {
                //ES_Utility.LogMessage(Siemens.ES.Diagnostics.LogSeverity.Error, "ES_MaintenanceManagement_GetInputData", ex.ToString());
                Page.DisplayMessage(ex.Message, false);
            }
        }

        #endregion Page Event

        #region Grid Event 

        ResponseData ChecklistGrid_RowSelected(object sender, JQGridEventArgs args)
        {

            ((DirectUpdateData)args.Response).PropertyValue = ((DirectUpdateData)args.Response).PropertyValue.Replace("PostBackRequested:false", "PostBackRequested:true");
            //PerfomResolveParametricData(true);
            return args.Response;
        }

        public void PopulateChecklistGrid(string resource, string maintStatus)
        {

            CompleteMaintenance inputData = new CompleteMaintenance
            {
                Resource = new NamedObjectRef(resource),
                ServiceDetails = new CompleteMaintDetails[]
                                                                 {
                                                                    new CompleteMaintDetails{
                                                                                              MaintenanceStatus = new SubentityRef(maintStatus)
                                                                                            }
                                                                 }


            };

            CompleteMaintenance_Info info = new CompleteMaintenance_Info
            {
                ServiceDetails = new CompleteMaintDetails_Info
                {
                    ChecklistSelection = new MaintenanceReqChecklistChanges_Info
                    {
                        ChecklistId = FieldInfoUtil.RequestValue(),
                        Instruction = FieldInfoUtil.RequestValue(),
                        Comments = FieldInfoUtil.RequestValue(),
                        Employee = FieldInfoUtil.RequestValue(),
                        TxnDate = FieldInfoUtil.RequestValue(),
                        Description = FieldInfoUtil.RequestValue(),
                        Notes = FieldInfoUtil.RequestValue()
                    }
                }
            };

            UserProfile profile = HttpContext.Current.Session[Camstar.WebPortal.Constants.SessionConstants.UserProfile] as UserProfile;
            CompleteMaintenanceService serv = new CompleteMaintenanceService(profile);
            CompleteMaintenance_Result result = null;
            ResultStatus resultStatus = serv.GetEnvironment(inputData, new CompleteMaintenance_Request { Info = info }, out result);

            ctrChecklistGrid.ClearData();

            ctrChecklistGrid.Data = result.Value.ServiceDetails[0].ChecklistSelection;
        }


        #endregion Grid Event 

        #region DataCollection Event

        public void PerfomResolveParametricData(bool haveCheckList)
        {
            //RevisionedObjectRef
            System.Data.DataRow rowSelected = ctrStatusDetailsGrid.BoundContext.GetSelectedItems(false)[0] as System.Data.DataRow;
            string maintStatus = rowSelected["MaintenanceStatus"].ToString();
            string resource = rowSelected["ResourceName"].ToString();

            if (resource != "" && maintStatus != "")
            {
                MaintenanceReqChecklistChanges[] check = null;
                if (haveCheckList)
                {
                    var CheckSelected = ctrChecklistGrid.GridContext.GetSelectedItems(false).ToList();
                    check = new MaintenanceReqChecklistChanges[CheckSelected.Count];
                    foreach (MaintenanceReqChecklistChanges it in CheckSelected)
                    {
                        check[CheckSelected.IndexOf(it)] = new MaintenanceReqChecklistChanges();
                        check[CheckSelected.IndexOf(it)].ChecklistId = it.ChecklistId != null ? it.ChecklistId.Value : null;
                        check[CheckSelected.IndexOf(it)].Instruction = it.Instruction != null ? it.Instruction.Value : null;
                        check[CheckSelected.IndexOf(it)].ListItemAction = ListItemAction.Add;
                        if (it.Comments != null)
                            if (it.Comments.Value != null)
                                check[CheckSelected.IndexOf(it)].ChecklistId = it.Comments.Value;
                    }
                }
                CompleteMaintenance inputData = new CompleteMaintenance
                {
                    Resource = new NamedObjectRef(resource),
                    ServiceDetails = new CompleteMaintDetails[]
                                             {
                                               new CompleteMaintDetails
                                                   {
                                                     MaintenanceStatus = new SubentityRef(maintStatus),
                                                     Checklist = check
                                                   }
                                             }
                };

                CompleteMaintenance_Info info = new CompleteMaintenance_Info
                {
                    ServiceDetails = new CompleteMaintDetails_Info
                    {
                        DataCollectionDef = new Info(true, true)
                    }
                };


                UserProfile profile = HttpContext.Current.Session[Camstar.WebPortal.Constants.SessionConstants.UserProfile] as UserProfile;
                CompleteMaintenanceService serv = new CompleteMaintenanceService(profile);
                CompleteMaintenance_Result result = null;
                ResultStatus resultStatus = serv.ResolveParametricData(inputData, new CompleteMaintenance_Request { Info = info }, out result);

                inputData.ServiceDetails[0].DataCollectionDef = (result.Value.ServiceDetails[0].DataCollectionDef as RevisionedObjectRef);
                hdnDataCollection.Data = (result.Value.ServiceDetails[0].DataCollectionDef as RevisionedObjectRef);

                Service.LoadServiceValues("CompleteMaintenance", "GetDataPoints");//loaded DisplayValues method is called
            }
        }


        public override void DisplayValues(Service serviceData)
        {
            ctrParametricData.DisplayValues(serviceData as ShopFloor);
            base.DisplayValues(serviceData);
            //hdnDataCollection.Data = null; 
        }

        public override void RequestValues(Info serviceInfo, Service serviceData)
        {
            base.RequestValues(serviceInfo, serviceData);


            if (serviceData is ShopFloor)
                (serviceData as ShopFloor).DataCollectionDef = hdnDataCollection.Data as RevisionedObjectRef;

            if (serviceInfo is ShopFloor_Info)
            {
                (serviceInfo as CompleteMaintenance_Info).DataCollectionDef = new Info(true);
                (serviceInfo as ShopFloor_Info).HasDataCollection = new Info(true);
            }
            ctrParametricData.Visible = true;
            ctrParametricData.RequestValues(serviceData as ShopFloor, serviceInfo as ShopFloor_Info);

        }

        public bool isChecklistCompleted()
        {
            bool complete = true;
            foreach (MaintenanceReqChecklistChanges row in ctrChecklistGrid.Data as MaintenanceReqChecklistChanges[])
            {
                if (row.Employee == null)
                {
                    complete = false;
                    break;
                }
            } // foreach
            return complete;
        } // public bool isChecklistCompleted

        #endregion DataCollection Event 


    }
}
