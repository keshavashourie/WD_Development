// © 2019 Siemens Product Lifecycle Management Software Inc.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework.Utilities;
using System.Data;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.FormsFramework;

using OM = Camstar.WCF.ObjectStack;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;

using CGC = Camstar.WebPortal.FormsFramework.WebGridControls;


namespace Camstar.WebPortal.WebPortlets.Shopfloor
{

    public class isCarrierShopfloor : MatrixWebPart
    {

        #region Classes


        internal class ContainerTmp
        {
            public string ContainerName;
            public string ProductName;
            public string ProductRevision;
            public string MfgOrderName;
            public string ContainerLevelName;
        }
        #endregion Classes

        #region Properties
        /// <summary>
        /// Container "#ErrorMsg.Name" is closed
        /// </summary>
        private const string LABEL_CONTAINER_CLOSED = "ContainerStatusClosed";  //  
        /// <summary>
        /// Container "#ErrorMsg.Name" is not active
        /// </summary>
        private const string LABEL_CONTAINER_INACTIVE = "ContainerStatusNotActive"; //  
        /// <summary>
        /// #ErrorMsg.Name is not assigned to Carrier #ErrorMsg.Name2.
        /// </summary>
        private const string LABEL_NOT_IN_CARRIER = "ContainerNotInCarrier";    //  
        /// <summary>
        /// #ErrorMsg.Name is not at Spec #ErrorMsg.Name2
        /// </summary>
        private const string LABEL_NOT_AT_SPEC = "ContainerNotAtThisSpec";  //  
        /// <summary>
        /// Container "#ErrorMsg.Name" is on hold
        /// </summary>
        private const string LABEL_ON_HOLD = "ContainerOnHold"; //  
        /// <summary>
        /// Container #ErrorMsg.CDOTypeName "#ErrorMsg.Name" not found
        /// </summary>
		private const string LABEL_CARRIER_NOT_FOUND = "NamedObjectNotFound";
        /// <summary>
        /// Container not found.  
        /// </summary>
        private const string LABEL_NOT_FOUND = "ContainerNotFound"; //  
        /// <summary>
        /// This carrier can only be loaded with Failed containers.  
        /// </summary>
        private const string LABEL_ONLY_FAILED = "isLoadUnloadCarrier_FailedContainersOnly"; //  
        /// <summary>
        /// This carrier can only be loaded with Good containers.
        /// </summary>
        private const string LABEL_ONLY_GOOD = "isLoadUnloadCarrier_GoodContainersOnly"; //  
        /// <summary>
        /// Container '#ErrorMsg.Name' already loaded on Carrier '#ErrorMsg.Name2'
        /// </summary>
        private const string LABEL_ALREADY_LOADED = "isLoadUnloadCarrier_ContainerAlreadyLoaded"; // 
        /// <summary>
        /// The carrier does not support tracking.
        /// </summary>
        private const string LABEL_NO_TRACKINGCONTAINER = "isErrorNoTrackingContainer"; //


        private const string IsMoveInDisabled = "IsMoveInDisabled";
        private const string IsMoveDisabled = "IsMoveDisabled";
        private const string IsUnloadAllDisabled = "IsUnloadAllDisabled";
        private const string IsSetupDisabled = "IsSetupDisabled";

        internal ContainerTmp LoadedContainer = null;

        List<MultiContainerTxnItem> AllContainers = new List<MultiContainerTxnItem>();

        protected virtual CWC.RadioButton UnloadButton { get { return Page.FindCamstarControl("ControlUnload") as CWC.RadioButton; } }
        protected virtual CWC.RadioButton LoadButton { get { return Page.FindCamstarControl("ControlLoad") as CWC.RadioButton; } }
        private bool LoadMode { get { return (bool)LoadButton.Data; } }

        protected object AllContainersDCValue { get { return Page.DataContract.GetValueByName("MultiContainer_AllContainers"); } }

        protected virtual JQDataGrid resultsGrid { get { return Page.FindCamstarControl("ES_GetAllSNContainersGrid") as JQDataGrid; } }
        protected CWC.ContainerList ContainerGridList { get { return Page.FindCamstarControl("ES_ContainerTmp") as CWC.ContainerList; } }

        protected virtual JQDataGrid ContainerGrid { get { return Page.FindCamstarControl("isLoadUnloadCarrier_Containers") as JQDataGrid; } }

        protected virtual TextBox HiddenContainer { get { return Page.FindCamstarControl("HiddenContainer") as TextBox; } }
        protected virtual TextBox TrackingContainer { get { return Page.FindCamstarControl("ControlContainerName") as TextBox; } }

        protected virtual NamedObject Carrier { get { return Page.FindCamstarControl("ControlCarrier") as NamedObject; } }
        protected virtual TextBox WorkflowStep { get { return Page.FindCamstarControl("ControlWorkflowStep") as TextBox; } }
        protected virtual DropDownList ContainerStatus { get { return Page.FindCamstarControl("ControlContainerStatus") as DropDownList; } }
        protected virtual DropDownList ddlSlotPosition { get { return Page.FindCamstarControl("isLoadUnloadCarrier_SlotPosition") as DropDownList; } }
        protected virtual CWC.RevisionedObject rdoSpec { get { return Page.FindCamstarControl("isLoadUnloadCarrier_Spec") as CWC.RevisionedObject; } }
        protected virtual CWC.NamedObject ndoResource { get { return Page.FindCamstarControl("isLoadUnloadCarrier_Resource") as CWC.NamedObject; } }
        protected bool FailedOnly { get { return ContainerStatus.Data != null && Convert.ToInt32(ContainerStatus.Data) == 2; } }
        protected bool GoodOnly { get { return ContainerStatus.Data != null && Convert.ToInt32(ContainerStatus.Data) == 1; } }
        protected virtual TextBox Capacity { get { return Page.FindCamstarControl("ControlCapacity") as TextBox; } }
        protected virtual TextBox ContainerName { get { return Page.FindCamstarControl("ControlContainer") as TextBox; } }
        protected CWC.TextBox IdentifierParam { get { return Page.FindCamstarControl("IdentifierParam") as CWC.TextBox; } }
        protected CWC.ContainerList Container { get { return Page.FindCamstarControl("MfgOrderReassign_Container") as CWC.ContainerList; } }

        protected CWC.CheckBox UseQueue { get { return Page.FindCamstarControl("ControlUseQueue") as CWC.CheckBox; } }
        protected CWC.CheckBox InProcess { get { return Page.FindCamstarControl("ControlInProcess") as CWC.CheckBox; } }
        protected CWC.CheckBox UsePosition { get { return Page.FindCamstarControl("isLoadUnloadCarrier_UsePosition") as CWC.CheckBox; } }
        protected CWC.CheckBox chkTransfer { get { return Page.FindCamstarControl("isLoadUnloadCarrier_TransferContainer") as CWC.CheckBox; } }
        #endregion

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
        }

        public override void PostExecute(ResultStatus status, Service serviceData)
        {
            base.PostExecute(status, serviceData);
            ContainerName.ClearData();
            HiddenContainer.ClearData();
        }

        protected override void OnLoad(System.EventArgs e)
        {
            base.OnLoad(e);

            Carrier.DataChanged += Carrier_DataChanged;
            ContainerName.DataChanged += Identifier_DataChanged;
            (resultsGrid.GridContext as CGC.QueryContext).AfterQueryExecution += resultsGrid_AfterQueryExecution;
            LoadButton.RadioControl.CheckedChanged += LoadRadio_DataChanged;
            UnloadButton.RadioControl.CheckedChanged += UnloadRadio_DataChanged;

            if (!Page.IsPostBack)
            {
                Page.SessionVariables[IsMoveInDisabled] = true;
                Page.SessionVariables[IsMoveDisabled] = true;
                Page.SessionVariables[IsUnloadAllDisabled] = true;
                Page.SessionVariables[IsSetupDisabled] = true;

                ReceiveTrackingContainer();

                ContainerGrid.BoundContext.Fields[0].Visible = false;
                ddlSlotPosition.Visible = false;
            }

            if (!Page.IsFloatingFrame)
                Page.DataContract.SetValueByName("Containers", null);

            //reload carrier when setup is executed
            if (Page.EventArgument == "FloatingFrameSubmitParentPostBackArgument")
            {
                ReloadInfoFieldsValue();
                ReloadCarrierPositionSelection();
            }

            if (Carrier.Enabled)
                ContainerName.Focus();               
        }

        protected void ReceiveTrackingContainer()
        {
            var trackingContainer = Page.DataContract.DataMembers.FirstOrDefault(m => m.Name == "EProcHiddenTaskContainerDM");

            if (trackingContainer != null && trackingContainer.Value != null)
            {
                var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
                string queryString = String.Format(@"select rname
                                                      from Container c
                                                      cross apply(select ResourceId as rid from ProductionStatus where isTrackingContainerId = c.ContainerId) ca1
                                                      cross apply(select CarrierId as cid from CurrentStatus where CarrierId = rid) ca2
                                                      cross apply(select ResourceName as rname from ResourceDef where ResourceId = cid) ca3
                                                      where ContainerName = '{0}'", trackingContainer.Value);
                var qs = new QueryService(session.CurrentUserProfile);
                QueryOptions qr = new QueryOptions();
                RecordSet rs;
                var res = qs.ExecuteAdHoc(queryString, qr, out rs);
                if (res.IsSuccess)
                {
                    if ((rs as RecordSet).Rows != null && (rs as RecordSet).Rows.Count() > 0)
                        Carrier.Data = rs.Rows[0].Values[0];
                }

                Carrier.Enabled = false;
            }
        }

        public static bool GetColumnBoolean(string colVal)
        {
            if (!string.IsNullOrEmpty(colVal))
            {
                if (String.Equals(colVal, "false", StringComparison.OrdinalIgnoreCase) || String.Equals(colVal, "true", StringComparison.OrdinalIgnoreCase))
                {
                    return Convert.ToBoolean(colVal);
                }
                else
                {
                    return String.Equals(colVal, "0") ? false : String.Equals(colVal, "1") ? true : false;
                }
            }
            else
                return false;
        }

        bool _inExecute = false;
        bool resultsGrid_AfterQueryExecution(QueryState queryState)
        {
            string message = null;
            bool isError = true;
            try
            {
                _inExecute = true;
                if (ContainerName.Data != null && ContainerGridList.Data == null)
                {
                    FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
                    var service = new WCF.Services.isLoadUnloadCarrierService(session.CurrentUserProfile);
                    isLoadUnloadCarrier cdo = new isLoadUnloadCarrier();
                    cdo.isCarrier = new NamedObjectRef(Carrier.Data.ToString());
                    isLoadUnloadCarrier_Request request = new isLoadUnloadCarrier_Request();
                    isLoadUnloadCarrier_Result result = new isLoadUnloadCarrier_Result();

                    var labelCache = FrameworkManagerUtil.GetLabelCache(System.Web.HttpContext.Current.Session);
                    int intTotalContainers = ContainerGrid.GridContext.GetTotalRows();

                    if (queryState.returnedRecords != null && queryState.returnedRecords.TotalCount == 1)
                    {
                        OM.isLoadUnloadCarrier svc = new isLoadUnloadCarrier();

                        string name = queryState.returnedRecords.Rows[0].Values[0];
                        string parent = queryState.returnedRecords.Rows[0].Values[3];
                        string trackingcontainer = TrackingContainer.Data != null ? TrackingContainer.Data.ToString() : null;
                        //if (!string.IsNullOrEmpty(parent) && string.Compare(parent, trackingcontainer, true) != 0)
                        //    name = parent;
                        int status = Convert.ToInt32(queryState.returnedRecords.Rows[0].Values[17]);
                        string workflowStepId = queryState.returnedRecords.Rows[0].Values[20];
                        string workflowStepName = queryState.returnedRecords.Rows[0].Values[21];
                        bool isFailed = GetColumnBoolean(queryState.returnedRecords.Rows[0].Values[26]);
                        bool onHold = !string.IsNullOrEmpty(queryState.returnedRecords.Rows[0].Values[28]) && Convert.ToInt32(queryState.returnedRecords.Rows[0].Values[28]) > 0;
                        string carrierId = queryState.returnedRecords.Rows[0].Values[29];
                        bool isCarrier = GetColumnBoolean(queryState.returnedRecords.Rows[0].Values[30]);
                        string carrierName = queryState.returnedRecords.Rows[0].Values[31];
                        if (isCarrier)
                            message = labelCache.GetLabelByName(LABEL_NOT_FOUND).Value;
                        else if (LoadMode)
                        {
                            //validations
                            if (trackingcontainer == null)
                            {
                                message = labelCache.GetLabelByName(LABEL_NO_TRACKINGCONTAINER).Value;
                            }
                            else if (carrierName != "" && string.Compare(carrierName, Carrier.Data.ToString(), true) != 0 && !(bool)chkTransfer.Data)
                            {
                                message = labelCache.GetLabelByName(LABEL_ALREADY_LOADED).Value;
                                message = message.Replace("#ErrorMsg.Name2", carrierName).Replace("#ErrorMsg.Name", name);
                            }
                            else if (carrierName != "" && string.Compare(carrierName, Carrier.Data.ToString(), true) == 0 && (bool)chkTransfer.Data)
                            {
                                message = labelCache.GetLabelByName(LABEL_ALREADY_LOADED).Value;
                                message = message.Replace("#ErrorMsg.Name2", carrierName).Replace("#ErrorMsg.Name", name);
                            }
                            else if (GoodOnly && isFailed)
                            {
                                message = labelCache.GetLabelByName(LABEL_ONLY_GOOD).Value;
                            }
                            else if (FailedOnly && !isFailed)
                            {
                                message = labelCache.GetLabelByName(LABEL_ONLY_FAILED).Value;
                            }
                            else if (onHold)
                            {
                                message = labelCache.GetLabelByName(LABEL_ON_HOLD).Value;
                                message = message.Replace("#ErrorMsg.Name", name);
                            }
                            else if (!(status == 1 || status == 3))
                            {
                                message = labelCache.GetLabelByName(LABEL_CONTAINER_INACTIVE).Value;
                                message = message.Replace("#ErrorMsg.Name", name);
                            }
                            else
                            {
                                ContainerGridList.Data = new ContainerRef(name);
                                LoadedContainer = new Shopfloor.isCarrierShopfloor.ContainerTmp();
                                LoadedContainer.ContainerName = name;
                                LoadedContainer.ProductName = queryState.returnedRecords.Rows[0].Values[12];
                                LoadedContainer.ProductRevision = queryState.returnedRecords.Rows[0].Values[13];
                                LoadedContainer.MfgOrderName = queryState.returnedRecords.Rows[0].Values[7];
                                LoadedContainer.ContainerLevelName = queryState.returnedRecords.Rows[0].Values[15];

                                cdo.ContainerList = new ContainerRef[1];
                                cdo.ContainerList[0] = new ContainerRef(name);
                                cdo.isCarrierLoadType = isCarrierLoadTypeEnum.Load;
                                if (UsePosition.CheckControl.Checked)
                                    cdo.SlotPosition = Convert.ToInt32(ddlSlotPosition.Data);
                                cdo.TransferContainer = (bool)chkTransfer.Data;
                                ResultStatus resultStatus = service.ExecuteTransaction(cdo);
                                if (resultStatus.IsSuccess)
                                {
                                    //reset read-only field values based on the first loaded container
                                    if (ContainerGrid.Data == null)
                                    {
                                        WorkflowStep.Data = workflowStepName;
                                    }

                                    ReloadCarrierPositionSelection();

                                    message = resultStatus.Message;
                                    isError = false;
                                    /*
                                    try
                                    {                                      
                                        (ContainerGrid.GridContext as QueryContext).ItemType = LoadedContainer.GetType();
                                        (ContainerGrid.GridContext as QueryContext).MakeAutoRowId(0);

                                        string id = AddNewRow(ContainerGrid);
                                        ContainerTmp[] arr = ((ContainerGrid.GridContext as QueryContext).Data as Array).Cast<ContainerTmp>().ToArray();
                                        if (arr != null)
                                        {
                                            object obj = arr.GetValue(arr.Length - 1);// GetValue(int.Parse(id));
                                            (obj as ContainerTmp).ContainerName = LoadedContainer.ContainerName;
                                            (obj as ContainerTmp).MfgOrderName = LoadedContainer.MfgOrderName;
                                            (obj as ContainerTmp).ProductName = LoadedContainer.ProductName;
                                            (obj as ContainerTmp).ProductRevision = LoadedContainer.ProductRevision;
                                            (obj as ContainerTmp).ContainerLevelName = LoadedContainer.ContainerLevelName;
                                        }
                                    }
                                    catch (Exception ex2)
                                    {
                                        message = ex2.Message;
                                    }      
                                    */
                                }
                                else
                                    message = resultStatus.ToString();
                            }
                        }
                        else
                        {
                            bool found = false;
                            // collect the containers
                            for (int x = 0; x < intTotalContainers; x++)
                            {
                                string strRowId = ContainerGrid.GridContext.GetRowId(x);
                                ContainerGrid.GridContext.SelectRow(strRowId, true);
                                object cell = ContainerGrid.GridContext.GetCell(strRowId, "ContainerName");
                                string containerName = cell != null ? cell.ToString() : string.Empty;
                                if (string.Compare(containerName, name, true) == 0)
                                {
                                    ClientGridState state = new ClientGridState();
                                    state.RowID = strRowId;

                                    cdo.ContainerList = new ContainerRef[1];
                                    cdo.ContainerList[0] = new ContainerRef(name);
                                    cdo.isCarrierLoadType = isCarrierLoadTypeEnum.Remove;
                                    ResultStatus resultStatus = service.ExecuteTransaction(cdo);
                                    if (resultStatus.IsSuccess)
                                    {
                                        message = resultStatus.Message;
                                        isError = false;
                                        ContainerGrid.GridContext.Delete(state);
                                        //reset page when there is no more container in the carrier
                                        if (intTotalContainers == 1)
                                        {
                                            Page.ClearValues();
                                        }
                                    }
                                    else
                                        message = resultStatus.ToString();
                                    found = true;
                                    break;
                                }
                            }
                            if (!found)
                                message = labelCache.GetLabelByName(LABEL_NOT_IN_CARRIER).Value.Replace("#ErrorMsg.Name2", Carrier.Data.ToString()).Replace("#ErrorMsg.Name", name);
                        }
                    }
                    else
                        message = labelCache.GetLabelByName(LABEL_NOT_FOUND).Value;
                }
            }
            catch (Exception ex) //Catch errors
            {
                message = ex.Message;
            }
            if (isError)
            {
                Page.StatusBar.WriteError(message);
                FocusContainer(false);
            }
            else
            {
                Page.StatusBar.WriteSuccess(message);
                ClientGridState state = new ClientGridState();
                state.ActionID = "refr";
                state.Action = "Reload";
                (ContainerGrid.BoundContext as QueryContext).LoadData();
                CamstarWebControl.SetRenderToClient(ContainerGrid);

                //reload container grid
                ContainerGrid.GridContext.Reload(state);

                FocusContainer(true);
            }
            return true;
        }

        void Identifier_DataChanged(object sender, EventArgs e)
        {
            Page.StatusBar.ClearMessage();
            //if (!_inExecute)
            {
                ContainerGridList.ClearData();

                if (ContainerName.Data != null)
                {
                    IdentifierParam.Data = ContainerName.Data.ToString().Replace(",", "','");
                    Page.DataContract.SetValueByName("IdentifierDM", IdentifierParam.Data);

                    resultsGrid.ClearData();
                    resultsGrid.BoundContext.Reload(new ClientGridState());

                    //relaod carrier in case the first container loaded is not in the same step as the tracking container
                    if (ContainerGrid.TotalRowCount == 1)
                    {
                        ReloadInfoFieldsValue();
                    }
                }
            }
        }

        void Carrier_DataChanged(object sender, EventArgs e)
        {
            Page.StatusBar.ClearMessage();
            ClientGridState state = new ClientGridState();
            state.ActionID = "refr";
            state.Action = "Reload";
            (ContainerGrid.BoundContext as QueryContext).LoadData();
            CamstarWebControl.SetRenderToClient(ContainerGrid);

            //reload container grid
            ContainerGrid.GridContext.Reload(state);

            ReloadInfoFieldsValue();
            ReloadCarrierPositionSelection();

            if (UsePosition.CheckControl.Checked)
            {
                ddlSlotPosition.Visible = true;
                ContainerGrid.BoundContext.Fields[0].Visible = true;
            }
            else
            {
                ddlSlotPosition.Visible = false;
                ContainerGrid.BoundContext.Fields[0].Visible = false;
            }
        }

        void LoadRadio_DataChanged(object sender, EventArgs e)
        {
            if (LoadButton.RadioControl.Checked && !UnloadButton.RadioControl.Checked)
            {
                chkTransfer.Visible = true;
                if (UsePosition.CheckControl.Checked)
                {
                    ddlSlotPosition.Visible = true;
                    ReloadCarrierPositionSelection();
                }
            }
        }

        void UnloadRadio_DataChanged(object sender, EventArgs e)
        {
            if (!LoadButton.RadioControl.Checked && UnloadButton.RadioControl.Checked)
            {
                chkTransfer.ClearData();
                chkTransfer.Visible = false;
                ddlSlotPosition.ClearData();
                ddlSlotPosition.Visible = false;
            }
        }

        public override void ClearValues(Service serviceData)
        {
            AllContainers.Clear();

            //base.ClearValues(serviceData);
        }

        protected Array CloneArray(object value, Type type, int emptyItemsCount)
        {
            Array array = null;
            int originalLength = ((value != null) && (value is Array)) ? (value as Array).Length : 0;
            if (type == null)
            {
                type = typeof(object);
            }
            array = Array.CreateInstance(type, (int)(originalLength + emptyItemsCount));
            for (int i = 0; i < array.Length; i++)
            {
                if (i < originalLength)
                {
                    array.SetValue(WCFObject.Clone((value as Array).GetValue(i)), i);
                }
                else
                {
                    array.SetValue(WCFObject.CreateObject(type), i);
                }
            }
            return array;
        }
        protected string AddNewRow(JQDataGrid grid)
        {
            QueryContext item = (grid.GridContext as QueryContext);
            item.Data = CloneArray(item.Data, item.ItemType, 1);
            return (item.GetTotalRows() - 1).ToString();
        }

        bool _preventClear = false;
        protected void FocusContainer(bool clearContainer = false)
        {
            _preventClear = true;
            if (clearContainer)
                ContainerName.TextControl.Text = "";
            var script = string.Format("$('#ctl00_WebPartManager_ContainersWP_ControlContainer_ctl00').val('');");
            //Page.ClientScript.RegisterStartupScript(this.GetType(), "clearContainer", script, true);
            Page.SetFocus(ContainerName.ClientID);
        }

        /// <summary>
        /// Get containers from data contract
        /// </summary>
        /// <returns></returns>
        protected virtual string[] GetCallerContainers()
        {
            string[] containers = null;

            //Using for Quality
            var activeTab = Page.DataContract.GetValueByName("SelectedTabsForAction");

            var callerContainers = ((activeTab == null) || (activeTab.Equals("Affected Material")))
                ? Page.DataContract.GetValueByName<Array>("Containers")
                : Page.DataContract.GetValueByName<Array>("DispositionContainers");

            if (callerContainers != null)
            {
                if (callerContainers.GetType() == typeof(EventLotDetail[]))
                    containers = callerContainers.OfType<EventLotDetail>().Where(n => n.IsContainer.Value).Select(n => n.Lot.Value).Distinct().ToArray();
                else
                    containers = callerContainers.OfType<string>().ToArray();
            }

            return containers;
        }

        public override void GetInputData(Service serviceData)
        {
            try
            {
                base.GetInputData(serviceData);

                OM.isLoadUnloadCarrier svcData = serviceData as OM.isLoadUnloadCarrier;

                int intTotalContainers = ContainerGrid.GridContext.GetTotalRows();
                svcData.ContainerList = new ContainerRef[intTotalContainers];

                // collect the containers
                for (int x = 0; x < intTotalContainers; x++)
                {
                    string strRowId = ContainerGrid.GridContext.GetRowId(x);
                    ContainerGrid.GridContext.SelectRow(strRowId, true);
                    svcData.ContainerList[x] = new ContainerRef();
                    svcData.ContainerList[x].Name = ContainerGrid.GridContext.GetCell(strRowId, "Name").ToString();
                }

            }
            catch { }
        }

        public override void WebPartCustomAction(object sender, CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as Camstar.WebPortal.Personalization.CustomAction;
            if (action != null)
                switch (action.Parameters)
                {
                    case "Reset":
                        {
                            Page.ClearValues();
                        }
                        break;
                    case "UnloadAll":
                        {
                            e.Result = UnloadAll();
                            if (e.Result.IsSuccess)
                                Page.ClearValues();
                        }
                        break;
                    case "MoveIn":
                        {
                            e.Result = MoveIn();
                            if (e.Result.IsSuccess)
                                Page.ClearValues();
                        }
                        break;
                    case "Move":
                        {
                            e.Result = Move();
                            if (e.Result.IsSuccess)
                                Page.ClearValues();
                        }
                        break;
                }
            FocusContainer(true);
        }

        protected ResultStatus UnloadAll()
        {
            ResultStatus resultStatus;
            string errLabel = null;
            try
            {
                FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
                var service = new WCF.Services.isLoadUnloadCarrierService(session.CurrentUserProfile);
                isLoadUnloadCarrier cdo = new isLoadUnloadCarrier();
                cdo.isCarrier = new NamedObjectRef(Carrier.Data.ToString());
                cdo.UnloadAll = true;
                cdo.isCarrierLoadType = isCarrierLoadTypeEnum.Remove;
                resultStatus = service.ExecuteTransaction(cdo);
                return resultStatus;
            }
            catch (Exception ex)
            {
                errLabel = ex.Message;
                resultStatus = new ResultStatus();
                resultStatus.Message = errLabel;
                return resultStatus;
            }
        }

        protected ResultStatus MoveIn()
        {
            ResultStatus resultStatus;
            string errLabel = null;
            try
            {
                FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
                var service = new WCF.Services.MoveInService(session.CurrentUserProfile);
                MoveIn cdo = new MoveIn();
                cdo.Container = new ContainerRef(TrackingContainer.Data.ToString());
                if ((ndoResource.Data != null) && !string.IsNullOrWhiteSpace(ndoResource.Data.ToString()))
                    cdo.Resource = new NamedObjectRef(ndoResource.Data.ToString());
                resultStatus = service.ExecuteTransaction(cdo);
                return resultStatus;
            }
            catch (Exception ex)
            {
                errLabel = ex.Message;
                resultStatus = new ResultStatus();
                resultStatus.Message = errLabel;
                return resultStatus;
            }
        }

        protected ResultStatus Move()
        {
            ResultStatus resultStatus;
            string errLabel = null;
            try
            {
                FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
                var service = new WCF.Services.MoveStdService(session.CurrentUserProfile);
                MoveStd cdo = new MoveStd();
                cdo.Container = new ContainerRef(TrackingContainer.Data.ToString());
                if ((ndoResource.Data != null) && !string.IsNullOrWhiteSpace(ndoResource.Data.ToString()))
                    cdo.Resource = new NamedObjectRef(ndoResource.Data.ToString());
                resultStatus = service.ExecuteTransaction(cdo);
                return resultStatus;
            }
            catch (Exception ex)
            {
                errLabel = ex.Message;
                resultStatus = new ResultStatus();
                resultStatus.Message = errLabel;
                return resultStatus;
            }
        }
        /// <summary>
        /// Clear all data associated with container(s) user has entered
        /// </summary>
        protected void ClearData(bool ClearCarrier = false)
        {
            try
            {
                if (ClearCarrier)
                    Carrier.ClearData();
                ContainerName.ClearData();
                ContainerStatus.ClearData();
                WorkflowStep.ClearData();
                Capacity.ClearData();
                TrackingContainer.ClearData();
                rdoSpec.ClearData();
                ndoResource.ClearData();
                UseQueue.ClearData();
                InProcess.ClearData();
                resultsGrid.ClearData();
                ContainerGridList.ClearData();
                HiddenContainer.ClearData();
                ContainerGrid.ClearData();
                ddlSlotPosition.ClearData();
            }
            catch { }
        }

        protected override IEnumerable<ScriptReference> GetScriptReferences()
        {
            yield return new ScriptReference("~/Scripts/MfgOperation_VP.js");
            yield return new ScriptReference("~/Scripts/User/Industry Solutions/isMfgOperation.js");
        }

        protected void ReloadInfoFieldsValue()
        {
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = new WCF.Services.isLoadUnloadCarrierService(session.CurrentUserProfile);
            isLoadUnloadCarrier cdo = new isLoadUnloadCarrier();
            isLoadUnloadCarrier_Info SvcInfo = new isLoadUnloadCarrier_Info();
            isLoadUnloadCarrier_Request request = new isLoadUnloadCarrier_Request();
            isLoadUnloadCarrier_Result result = new isLoadUnloadCarrier_Result();

            if (Carrier.Data != null)
                cdo.isCarrier = new NamedObjectRef(Carrier.Data.ToString());

            SvcInfo.Capacity = new Info(true);
            SvcInfo.ContainerStatus = new Info(true);
            SvcInfo.ContainerName = new Info(true);
            SvcInfo.WorkflowStep = new Info(true);
            SvcInfo.Spec = new Info(true);
            SvcInfo.Resource = new Info(true);
            SvcInfo.UseQueue = new Info(true);
            SvcInfo.InProcess = new Info(true);
            SvcInfo.UsePosition = new Info(true);

            request.Info = SvcInfo;
            ResultStatus resultStatus = service.GetEnvironment(cdo, request, out result);
            if (resultStatus.IsSuccess)
            {
                if (result.Value.Capacity != null)
                    Capacity.Data = result.Value.Capacity.Value;
                else
                    Capacity.ClearData();

                if (result.Value.ContainerStatus != null)
                    ContainerStatus.Data = result.Value.ContainerStatus.Value;
                else
                    ContainerStatus.ClearData();

                if (result.Value.ContainerName != null)
                    TrackingContainer.Data = result.Value.ContainerName.Value;
                else
                    TrackingContainer.ClearData();

                if (result.Value.Spec != null)
                    rdoSpec.Data = result.Value.Spec;
                else
                    rdoSpec.ClearData();

                if (result.Value.Resource != null)
                    ndoResource.Data = result.Value.Resource;
                else
                    ndoResource.ClearData();

                if (result.Value.WorkflowStep != null && ContainerGrid.TotalRowCount > 0)
                    WorkflowStep.Data = result.Value.WorkflowStep;
                else
                    WorkflowStep.ClearData();

                if (result.Value.UseQueue != null && result.Value.UseQueue.Value == true)
                    UseQueue.CheckControl.Checked = true;
                else
                    UseQueue.ClearData();

                if (result.Value.InProcess != null && result.Value.InProcess.Value == true)
                    InProcess.CheckControl.Checked = true;
                else
                    InProcess.ClearData();

                if (result.Value.UsePosition != null && result.Value.UsePosition.Value == true)
                    UsePosition.CheckControl.Checked = true;
                else
                    UsePosition.ClearData();
            }
            else
            {				
				var labelCache = FrameworkManagerUtil.GetLabelCache(System.Web.HttpContext.Current.Session);
				var carrier =  labelCache.GetLabelByName("CSICDOName_Carrier").Value;
				var message = labelCache.GetLabelByName(LABEL_CARRIER_NOT_FOUND).Value;
				message = message.Replace("#ErrorMsg.Name", Carrier.Data.ToString()).Replace("#ErrorMsg.CDOTypeName", carrier);

                ClearData();
				Carrier.Data = null;
				Page.StatusBar.WriteError(message);
            }
            //reload session variables
            bool useQueue = UseQueue.Data != null && (bool)UseQueue.Data;
            bool inProcess = InProcess.Data != null && (bool)InProcess.Data;
            bool MoveInDisabled = Carrier.Data == null || ContainerGrid.TotalRowCount == 0 || !useQueue || inProcess;
            bool MoveDisabled = Carrier.Data == null || ContainerGrid.TotalRowCount == 0 || (useQueue && !inProcess);
            Page.SessionVariables[IsMoveInDisabled] = MoveInDisabled;
            Page.SessionVariables[IsMoveDisabled] = MoveDisabled;
            Page.SessionVariables[IsUnloadAllDisabled] = Carrier.Data == null || ContainerGrid.TotalRowCount == 0;
            Page.SessionVariables[IsSetupDisabled] = Carrier.Data == null || ContainerGrid.TotalRowCount > 0 || TrackingContainer.Data == null;

            ndoResource.Enabled = false;
            if (!MoveInDisabled || !MoveDisabled)
            {
                ndoResource.Enabled = true;
            }
        }

        protected void ReloadCarrierPositionSelection()
        {
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = new WCF.Services.isLoadUnloadCarrierService(session.CurrentUserProfile);
            isLoadUnloadCarrier cdo = new isLoadUnloadCarrier();
            isLoadUnloadCarrier_Info SvcInfo = new isLoadUnloadCarrier_Info();
            isLoadUnloadCarrier_Request request = new isLoadUnloadCarrier_Request();
            isLoadUnloadCarrier_Result result = new isLoadUnloadCarrier_Result();

            if (Carrier.Data != null)
            {
                cdo.isCarrier = new NamedObjectRef(Carrier.Data.ToString());

                SvcInfo.SlotPosition = new Info();
                SvcInfo.SlotPosition.RequestSelectionValues = true;

                request.Info = SvcInfo;
                ResultStatus resultStatus = service.GetEnvironment(cdo, request, out result);
                if (resultStatus.IsSuccess && result.Environment.SlotPosition.SelectionValues != null)
                {
                    ddlSlotPosition.SetSelectionValues(result.Environment.SlotPosition.SelectionValues);
                    if (result.Environment.SlotPosition.SelectionValues != null)
                        ddlSlotPosition.Data = result.Environment.SlotPosition.SelectionValues.Rows[0].Values[0];
                }
                else
                {
                    ddlSlotPosition.ClearData();
                }
            }
            else
                ddlSlotPosition.ClearData();
        }
    }
}
