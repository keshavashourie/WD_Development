// Copyright Siemens 2023  
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Data;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework.Utilities;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using PERS = Camstar.WebPortal.Personalization;
using CamstarPortal.WebControls;
using System.Collections;
using Camstar.WebPortal.WCFUtilities;
using System.ComponentModel.DataAnnotations;


namespace Camstar.WebPortal.WebPortlets.Modeling
{
    /// <summary>
    /// The base class for WIP Message page.
    /// </summary>
    public class ModelingWIPKeys : MatrixWebPart
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!Page.IsPostBack)
            {
                ActivateWIPMsgForm(WIPMsgFormTypes.None);
                OperationToFind.Hidden = true;
                LabelToFind.Hidden = true;
                LoadRelevantServiceTypes();
                Page.PortalContext.LocalSession["EsigPostback"] = false;
            }
            JQDataGrid operationKeysGrid = (JQDataGrid)Page.FindCamstarControl("OperationKeysGrid");
            JQDataGrid operationSubGrid = (JQDataGrid)Page.FindCamstarControl("OperationWIPMessageDetailsGrid");
            JQDataGrid allKeysGrid = (JQDataGrid)Page.FindCamstarControl("AllKeysGrid");
            JQDataGrid labelKeysGrid = (JQDataGrid)Page.FindCamstarControl("LabelKeysGrid");
            JQDataGrid labelSubGrid = (JQDataGrid)Page.FindCamstarControl("LabelWIPMessageDetailsGrid");
            CWC.TextBox stateControl = (CWC.TextBox)Page.FindCamstarControl("TreeStateControl");
            stateControl.Hidden = true;
            //we set client id to static to use it in script
            stateControl.ClientIDMode = System.Web.UI.ClientIDMode.Static;
            Tree = new TreeSimulator(operationKeysGrid, operationSubGrid, allKeysGrid, labelKeysGrid, labelSubGrid, stateControl);
            Tree.Selected += Tree_Selected;
            if ((bool)Page.PortalContext.LocalSession["EsigPostback"] && Page.PortalContext.LocalSession["EsigEventArgument"] != null)
                WebPartCustomAction(null, Page.PortalContext.LocalSession["EsigEventArgument"] as PERS.CustomActionEventArgs);
        }

        protected virtual void LoadRelevantServiceTypes()
        {
            var cdoCache = FrameworkManagerUtil.GetCDOCache(HttpContext.Current.Session);
            string displayExp, valueExp;
            var services = cdoCache.GetShopFloorServicesTable(out displayExp, out valueExp);
            if (!string.IsNullOrEmpty(displayExp))
                serviceTypes.ListDisplayExpression = displayExp;
            if (!string.IsNullOrEmpty(valueExp))
                serviceTypes.ListValueColumn = valueExp;
            serviceTypes.SetSelectionValues(new OM.RecordSet
                {
                    Headers = services.Columns.OfType<DataColumn>().Select(c => new OM.Header() { Name = c.ColumnName }).ToArray(),
                    Rows = services.Rows.OfType<DataRow>().Select(r => new OM.Row() { Values = r.ItemArray.Select(i => i.ToString()).ToArray() }).ToArray(),
                    TotalCount = services.Rows.Count
                });
        }

        protected virtual void Tree_Selected(object sender, TreeSimulator.MsgEventAgrs e)
        {
            ActivateWIPMsgForm(e.FormType);
            if (e.MsgDetail != null)
                ShowMsgDetail(e.MsgDetail);
            RenderToClient = true;
        }

        protected virtual void ShowMsgDetail(OM.WIPMsgDetailChanges msgDetail)
        {
            GeneralGroupWP.ClearValues();
            ProcessingGroupWP.ClearValues();
            NotificationGroupWP.ClearValues();

            GeneralGroupWP.DisplayValues(new OM.WIPMsgMaint { ObjectChanges = new OM.WIPMsgDefMgrChanges { WIPMsgDetailItem = msgDetail } });
            ProcessingGroupWP.DisplayValues(new OM.WIPMsgMaint { ObjectChanges = new OM.WIPMsgDefMgrChanges { WIPMsgDetailItem = msgDetail } });
            NotificationGroupWP.DisplayValues(new OM.WIPMsgMaint { ObjectChanges = new OM.WIPMsgDefMgrChanges { WIPMsgDetailItem = msgDetail } });

            ActivateWIPMsgForm(WIPMsgFormTypes.WIPMsgDetails);
        }

        protected virtual void ProcessEsigs(Personalization.CustomActionEventArgs e, ESigMaintActions action)
        {
            string command = ((int)action).ToString(CultureInfo.InvariantCulture);
            if ((Page.PortalContext as MaintenanceBehaviorContext).State == MaintenanceBehaviorContext.MaintenanceState.Edit)
                command = ((int)ESigMaintActions.Update).ToString(CultureInfo.InvariantCulture);
            var esigReq = GetEsigRequirementMaintenanceTxn(PrimaryServiceType, command, null);
            if (esigReq != null)
            {
                Page.PortalContext.LocalSession["EsigEventArgument"] = e;
                Page.PortalContext.LocalSession["EsigPostback"] = true;
                Page.OpenContainerPopupESigCapture(PrimaryServiceType, Tuple.Create(esigReq, (OM.ESigProcessTimerServiceDetail[])null));
            }
        }
        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            PERS.CustomAction action = e.Action as PERS.CustomAction;
            string parameter = action.Parameters;
            if (action != null)
            {
                if (parameter == "SaveMessage")
                {
                    if (!(bool)Page.PortalContext.LocalSession["EsigPostback"])
                        ProcessEsigs(e, ESigMaintActions.Create);
                    Page.PortalContext.LocalSession["EsigPostback"] = false;
                    OM.WIPMsgMaint message = new OM.WIPMsgMaint();
                    Page.GetInputData(message);
                    Tree.UpdateSelectedMsgDetail(message.ObjectChanges.WIPMsgDetailItem);
                    this.RenderToClient = true; //replace with js-functionality in the future
                    OM.ResultStatus status = SubmitWIPMsgChanges();
                    e.Result = status;
                    Page.StatusBar.WriteStatus(status);
                }
                else if (parameter == "DeleteMessage")
                {
                    if (!(bool)Page.PortalContext.LocalSession["EsigPostback"])
                        ProcessEsigs(e, ESigMaintActions.Delete);
                    Page.PortalContext.LocalSession["EsigPostback"] = false;
                    OM.WIPMsgTypeEnum? retEnum;
                    string categoryName;
                    Tree.DeleteSelectedItem(out retEnum, out categoryName);
                    OM.ResultStatus status = SubmitWIPMsgChanges();
                    if (status.IsSuccess)
                        ActivateWIPMsgForm(WIPMsgFormTypes.None);
                    e.Result = status;
                    Page.StatusBar.WriteStatus(status);
                    this.RenderToClient = true; //replace with js-functionality in the future

                }
                else if (parameter == "DeleteAll")
                {
                    if (!(bool)Page.PortalContext.LocalSession["EsigPostback"])
                        ProcessEsigs(e, ESigMaintActions.Delete);
                    Page.PortalContext.LocalSession["EsigPostback"] = false;
                    Tree.DeleteGroupItem();
                    OM.ResultStatus status = SubmitWIPMsgChanges();
                    if (status.IsSuccess)
                        ActivateWIPMsgForm(WIPMsgFormTypes.None);
                    e.Result = status;
                    Page.StatusBar.WriteStatus(status);
                    this.RenderToClient = true; //replace with js-functionality in the future
                }
                else if (parameter == "CopyAsNewMsg")
                {
                    //move to Msg Type page
                    IsCopyContext = true;
                    ActivateWIPMsgForm(WIPMsgFormTypes.WIPMsgType);
                }
                else if (parameter == "NewWIPMessage")
                {
                    //move to Msg Type page
                    IsCopyContext = false;
                    GeneralGroupWP.ClearValues();
                    ProcessingGroupWP.ClearValues();
                    NotificationGroupWP.ClearValues();

                    ActivateWIPMsgForm(WIPMsgFormTypes.WIPMsgType);
                }
                else if (parameter.Equals("CreateOrCopyMessage"))
                {
                    OM.ResultStatus status = CreateOrCopyMessage();
                    if (status == null)
                        return;
                    e.Result = status;
                    Page.StatusBar.WriteStatus(status);
                }
            }
        }

        protected virtual OM.ESigServiceDetail[] GetEsigRequirementMaintenanceTxn(string servName, string eSigMaintAction, PERS.DependsOnItem[] dependencies)
        {
            OM.ESigServiceDetail[] returnValue = null;
            if (!string.IsNullOrEmpty(eSigMaintAction))
            {
                WSDataCreator creator = new WSDataCreator();
                IWCFService service = creator.CreateService(servName, FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);
                Request request = WCFObject.CreateObject(servName + "_Request") as Request;
                WCFObject wcfo = new WCFObject(request);
                OM.Info info = WCFObject.CreateObject(servName + "_Info") as OM.Info;
                wcfo.SetValue("Info", info);
                wcfo.SetValue("Info.ESigRequirement", new OM.Info(true));
                wcfo.SetValue("Info.ESigDetails", new OM.ESigServiceDetail_Info { RequestValue = true });
                var txn = creator.CreateServiceData(servName);

                var parameters = WCFObject.CreateObject(string.Format("{0}_LoadESigDetails_Parameters", servName)) as Parameters;
                WCFObject wcfoParams = new WCFObject(parameters);
                wcfoParams.SetValue("ESigMaintAction", eSigMaintAction);

                Result res;
                OM.ResultStatus status = (service as IMaintenanceBase).LoadESigDetails(txn, parameters, request, out res);

                if (status.IsSuccess && res != null)
                {
                    var wcfoRes = new WCFObject(res);
                    returnValue = wcfoRes.GetValue("Value.ESigDetails") as OM.ESigServiceDetail[];
                }

            }
            return returnValue;
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            Page.UpdatePageActionsButtons(Page.ButtonsBar);
            //restore drop-down selection values, since they are not obtained via field expression
            if (serviceTypes.DropDownControl.Items.Count == 0)
                LoadRelevantServiceTypes();
        }

        //create a new one or create as a copy of existing one.
        protected virtual OM.ResultStatus CreateOrCopyMessage()//returns error status only
        {
            var validationStatus = ValidateWIPMsgTypeForm();
            if (!validationStatus.IsSuccess)
                return validationStatus;
            OM.WIPMsgMaint message = new OM.WIPMsgMaint();
            bool isMissed = false;
            CWC.DropDownList wipMsgTypeControl = WIPMsgType;
            OM.WIPMsgTypeEnum wipMsgType = OM.Enumeration<OM.WIPMsgTypeEnum, int>.ConvertToEnum((int)wipMsgTypeControl.Data, out isMissed);
            if (isMissed)
                throw new ApplicationException("WIPMsgType is not specified");
            string categoryName = null;
            switch (wipMsgType)
            {
                case OM.WIPMsgTypeEnum.OperationKeys:
                    CWC.DropDownList operationList = OperationToFind;
                    categoryName = (string)operationList.Data;
                    break;
                case OM.WIPMsgTypeEnum.LabelKeys:
                    CWC.TextBox labelText = LabelToFind;
                    categoryName = (string)labelText.Data;
                    break;

            }
            Page.GetInputData(message);
            OM.ResultStatus st = null;
            if (IsCopyContext)
            {
                OM.WIPMsgDetailChanges msgDetail = (OM.WIPMsgDetailChanges)message.ObjectChanges.WIPMsgDetailItem.Clone();
                Tree.AddMsgDetail(msgDetail, wipMsgType, categoryName);
                //save transaction
                OM.ResultStatus status = SubmitWIPMsgChanges();
                if (!status.IsSuccess)
                    st = status;
                IsCopyContext = false;
            }
            else
            {
                OM.WIPMsgDetailChanges msgDetail = (OM.WIPMsgDetailChanges)message.ObjectChanges.WIPMsgDetailItem.Clone();
                Tree.AddMsgDetail(msgDetail, wipMsgType, categoryName);
            }
            this.RenderToClient = true;
            return st;
        }

        protected virtual void ActivateWIPMsgForm(WIPMsgFormTypes type)
        {
            if (type == WIPMsgFormTypes.WIPMsgType)
                WIPMsgType.Data = 1;
            WIPMessageTypeWP.Hidden = type != WIPMsgFormTypes.WIPMsgType;
            WIPMessageDetailsWP.Hidden = type != WIPMsgFormTypes.WIPMsgDetails;
        }

        public override void RequestValues(OM.Info serviceInfo, OM.Service serviceData)
        {
            base.RequestValues(serviceInfo, serviceData);

            (serviceData as OM.WIPMsgMaint).ObjectWithWIPMsg = (Page.PortalContext as MaintenanceBehaviorContext).Current;
        }

        protected override void LoadViewState(object savedState)
        {
            base.LoadViewState(savedState);
            IsCopyContext = (bool)ViewState["IsCopyContext"];
        }

        protected override object SaveViewState()
        {
            ViewState["IsCopyContext"] = IsCopyContext;
            return base.SaveViewState();
        }

        protected virtual void SetRelevantServiceName(OM.WIPMsgMaint message)
        {
            if (message.ObjectChanges.OperationKeys != null)
                if (message.ObjectChanges.OperationKeys[0].WIPMsgDetails != null)
                    message.ObjectChanges.OperationKeys[0].WIPMsgDetails[0].RelevantServiceName = serviceTypes.DropDownControl.Text;
            if (message.ObjectChanges.AllKey != null)
                if (message.ObjectChanges.AllKey.WIPMsgDetails != null)
                    message.ObjectChanges.AllKey.WIPMsgDetails[0].RelevantServiceName = serviceTypes.DropDownControl.Text;
            if (message.ObjectChanges.LabelKeys != null)
                if (message.ObjectChanges.LabelKeys[0].WIPMsgDetails != null)
                    message.ObjectChanges.LabelKeys[0].WIPMsgDetails[0].RelevantServiceName = serviceTypes.DropDownControl.Text;
        }

        protected virtual OM.ResultStatus SubmitWIPMsgChanges()
        {
            OM.WIPMsgMaint message = new OM.WIPMsgMaint();
            this.GetInputData(message);
            SetRelevantServiceName(message);

            var tuple = ESigCaptureUtil.CollectESigServiceDetailsAll();
            OM.ESigServiceDetail[] eSigDetails = tuple == null ? null : tuple.Item1;
            var profile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
            WSDataCreator dataCreator = new WSDataCreator();
            var serv = (Camstar.WCF.Services.WIPMsgMaintService)dataCreator.CreateService(PrimaryServiceType, profile);
            OM.WIPMsgMaint_Info reqInfo = new OM.WIPMsgMaint_Info();
            OM.WIPMsgMaint firstInput = new OM.WIPMsgMaint { ObjectWithWIPMsg = (Page.PortalContext as MaintenanceBehaviorContext).Current };
            if (eSigDetails != null && eSigDetails.Length > 0)
                firstInput.ESigDetails = eSigDetails;
            firstInput.ObjectChanges = null;
            OM.WIPMsgMaint secondInput = new OM.WIPMsgMaint { ObjectChanges = message.ObjectChanges };
            serv.BeginTransaction();
            serv.Load(firstInput);

            serv.AddDataTransaction(secondInput);
            serv.ExecuteTransaction();
            OM.ResultStatus status = serv.CommitTransaction();
            if (status.IsSuccess)
            {
                string rowID = "";
                string rowDataObject = "";
                GridContext context = null;

                Tree.SaveTreeState(out rowID, out rowDataObject, out context, IsCopyContext);

                ReloadWIPMsg();
                this.RenderToClient = true;
                if (context != null && rowID != null && rowDataObject != null)
                {
                    context.ExpandRow(new ClientGridState { RowID = rowID, RowDataObject = rowDataObject });
                    context.RenderToClient = true;
                }
            }
            return status;
        }

        


        protected virtual void ReloadWIPMsg()
        {
            OM.WIPMsgMaint message = new OM.WIPMsgMaint();
            this.GetInputData(message);
            var profile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
            WSDataCreator dataCreator = new WSDataCreator();
            var serv = (WIPMsgMaintService)dataCreator.CreateService(PrimaryServiceType, profile);
            WIPMsgMaint_Request request = (WIPMsgMaint_Request)dataCreator.CreateObject(PrimaryServiceType + "_Request");
            WIPMsgMaint_Result result = (WIPMsgMaint_Result)dataCreator.CreateObject(PrimaryServiceType + "_Result");
            OM.WIPMsgMaint_Info reqInfo = new OM.WIPMsgMaint_Info()
            {
                ObjectChanges = new OM.WIPMsgDefMgrChanges_Info()
                {
                    AllKey = new OM.WIPMsgKeyChanges_Info() { WIPMsgDetails = new OM.WIPMsgDetailChanges_Info() { RequestValue = true } },
                    LabelKeys = new OM.WIPMsgLabelKeyChanges_Info() { RequestValue = true },
                    OperationKeys = new OM.WIPMsgOperationKeyChanges_Info() { RequestValue = true }
                }
            };
            request.Info = reqInfo;
            OM.WIPMsgMaint input = new OM.WIPMsgMaint { ObjectWithWIPMsg = (Page.PortalContext as MaintenanceBehaviorContext).Current };
            input.ObjectChanges = null;

            serv.BeginTransaction();
            serv.Load(input);
            OM.ResultStatus status = serv.CommitTransaction(request, out result);
            if (status.IsSuccess)
            {
                Tree.LoadData(result);
            }
        }

        protected virtual OM.ResultStatus ValidateWIPMsgTypeForm()
        {
            bool isMissed;
            OM.WIPMsgTypeEnum wipMsgType = OM.Enumeration<OM.WIPMsgTypeEnum, int>.ConvertToEnum((int)WIPMsgType.Data, out isMissed);
            if (isMissed)
                throw new ApplicationException("WIPMsgType is not specified");
            LabelCache labelCache = FrameworkManagerUtil.GetLabelCache(Page.Session);
            switch (wipMsgType)
            {
                case OM.WIPMsgTypeEnum.OperationKeys:
                    string value = (string)OperationToFind.Data;
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        OM.Label label = labelCache.GetLabelByName("NoOperationSpecified");
                        return new OM.ResultStatus(label.Value, false);
                    };
                    break;
                case OM.WIPMsgTypeEnum.LabelKeys:
                    value = (string)LabelToFind.Data;
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        OM.Label label = labelCache.GetLabelByName("NoLabelSpecified");
                        return new OM.ResultStatus(label.Value, false);
                    }
                    break;
            }
            return new OM.ResultStatus(string.Empty, true);
        }

        #region Properties

        protected virtual JQGridBase MsgMessage
        {
            get
            {
                return Page.FindCamstarControl("AllKeysGrid") as JQGridBase;
            }
        }

        protected virtual MatrixWebPart WIPMessageDetailsWP
        {
            get
            {
                var proxy = Page.CamstarControls.Single(w => w.ID == "WIPMessageDetailsWP");
                return proxy != null ? proxy.AsIForm as MatrixWebPart : null; ;
            }
        }

        protected virtual MatrixWebPart GeneralGroupWP
        {
            get
            {
                var proxy = Page.CamstarControls.Single(w => w.ID == "GeneralGroupWP");
                return proxy != null ? proxy.AsIForm as MatrixWebPart : null; ;
            }
        }

        protected virtual MatrixWebPart ProcessingGroupWP
        {
            get
            {
                var proxy = Page.CamstarControls.Single(w => w.ID == "ProcessingGroupWP");
                return proxy != null ? proxy.AsIForm as MatrixWebPart : null; ;
            }
        }

        protected virtual MatrixWebPart NotificationGroupWP
        {
            get
            {
                var proxy = Page.CamstarControls.Single(w => w.ID == "NotificationGroupWP");
                return proxy != null ? proxy.AsIForm as MatrixWebPart : null; ;
            }
        }

        protected virtual MatrixWebPart WIPMessageTypeWP
        {
            get
            {
                var proxy = Page.CamstarControls.Single(w => w.ID == "WIPMessageTypeWP");
                return proxy != null ? proxy.AsIForm as MatrixWebPart : null; ;
            }
        }

        protected virtual CWC.Button ContinueButton
        {
            get
            {
                return Page.FindCamstarControl("ContinueBtn") as CWC.Button;
            }
        }

        protected virtual CWC.DropDownList OperationToFind
        {
            get
            {
                return Page.FindCamstarControl("OperationToFind") as CWC.DropDownList;
            }
        }

        protected virtual CWC.TextBox LabelToFind
        {
            get
            {
                return (CWC.TextBox)Page.FindCamstarControl("LabelToFind");
            }
        }

        protected virtual CWC.DropDownList WIPMsgType
        {
            get
            {
                return (CWC.DropDownList)Page.FindCamstarControl("WIPMsgType");
            }
        }

        protected virtual CWC.DropDownList serviceTypes
        {
            get { return Page.FindCamstarControl("WIPMsgDetailItem_RelevantServiceType") as CWC.DropDownList; }
        }

        protected virtual TreeSimulator Tree
        {
            get;
            set;
        }

        protected virtual bool IsCopyContext
        {
            get;
            set;
        }

        #region ActionActivity

        public virtual bool IsSaveActive
        {
            get
            {
                return !WIPMessageDetailsWP.Hidden;
            }
        }

        public virtual bool IsDeleteActive
        {
            get
            {
                return !WIPMessageDetailsWP.Hidden;
            }
        }

        public virtual bool IsCopyActive
        {
            get
            {
                if (IsKeyGroupSelected || WIPMessageDetailsWP.Hidden)
                    return false;

                MatrixWebPart webPart = WIPMessageDetailsWP;
                OM.WIPMsgMaint message = new OM.WIPMsgMaint();
                Page.GetInputData(message);
                bool isCurrentMessageValid = message.ObjectChanges != null && message.ObjectChanges.WIPMsgDetailItem != null
                    && message.ObjectChanges.WIPMsgDetailItem.MsgText != null && !message.ObjectChanges.WIPMsgDetailItem.MsgText.IsEmpty;
                return isCurrentMessageValid;
            }
        }

        public virtual bool IsNewActive
        {
            get
            {
                return WIPMessageTypeWP.Hidden;
            }
        }

        public virtual bool IsDeleteAllActive
        {
            get
            {
                return IsKeyGroupSelected;
            }
        }

        protected virtual bool IsKeyGroupSelected
        {
            get
            {
                var operationKeysGrid = (JQDataGrid)Page.FindCamstarControl("OperationKeysGrid");
                var labelKeysGrid = (JQDataGrid)Page.FindCamstarControl("LabelKeysGrid");
                return operationKeysGrid != null && labelKeysGrid != null &&
                       (operationKeysGrid.GridContext.SelectedRowID != null ||
                        labelKeysGrid.GridContext.SelectedRowID != null);
            }
        }

        #endregion

        #endregion
    }
}

public enum WIPMsgFormTypes
{
    None,
    WIPMsgType,
    WIPMsgDetails
}

public class TreeSimulator
{
    public class MsgEventAgrs : EventArgs
    {
        public readonly OM.WIPMsgDetailChanges MsgDetail;
        public readonly ItemDataContext GridContext;
        public readonly OM.WIPMsgTypeEnum Type;
        public WIPMsgFormTypes FormType;

        public MsgEventAgrs(OM.WIPMsgDetailChanges msgDetail, ItemDataContext context, OM.WIPMsgTypeEnum msgType, WIPMsgFormTypes formType)
        {
            MsgDetail = msgDetail;
            GridContext = context;
            Type = msgType;
            FormType = formType;
        }
    }

    public TreeSimulator(JQDataGrid operationKeysGrid, JQDataGrid operationSubGrid, JQDataGrid allKeysGrid, JQDataGrid labelKeysGrid, JQDataGrid labelSubGrid, CWC.TextBox stateControl)
    {
        OperationKeysGrid = operationKeysGrid;
        OperationKeysSubgrid = operationSubGrid;
        AllKeysGrid = allKeysGrid;
        LabelKeysGrid = labelKeysGrid;
        LabelKeysSubgrid = labelSubGrid;
        StateControl = stateControl;
        //adds Selected event

        operationKeysGrid.RowSelected += (sender, args) => { OnGroupSelected(OM.WIPMsgTypeEnum.OperationKeys, (ItemDataContext)args.Context); return args.Response; };
        labelKeysGrid.RowSelected += (sender, args) => { OnGroupSelected(OM.WIPMsgTypeEnum.LabelKeys, (ItemDataContext)args.Context); return args.Response; };

        operationSubGrid.RowSelected += (sender, args) => { OnSelected(OM.WIPMsgTypeEnum.OperationKeys, (ItemDataContext)args.Context); return args.Response; };
        allKeysGrid.RowSelected += (sender, args) => { OnSelected(OM.WIPMsgTypeEnum.AllKeys, (ItemDataContext)args.Context); return args.Response; };
        labelSubGrid.RowSelected += (sender, args) => { OnSelected(OM.WIPMsgTypeEnum.LabelKeys, (ItemDataContext)args.Context); return args.Response; };
    }

    public event EventHandler<MsgEventAgrs> Selected;

    private readonly JQDataGrid OperationKeysGrid, OperationKeysSubgrid, AllKeysGrid, LabelKeysGrid, LabelKeysSubgrid;
    private readonly CWC.TextBox StateControl;

    #region Properties

    protected virtual IEnumerable<GridContext> OperationContexts
    {
        get
        {
            IEnumerable<GridContext> contexts = OperationKeysSubgrid.GridContext.GetSubContexts();
            return contexts ?? new ItemDataContext[] { };
        }
    }

    protected virtual IEnumerable<GridContext> GeneralContexts
    {
        get
        {
            return new GridContext[] { AllKeysGrid.GridContext };
        }
    }

    protected virtual IEnumerable<GridContext> LabelContexts
    {
        get
        {
            IEnumerable<GridContext> contexts = LabelKeysSubgrid.GridContext.GetSubContexts();
            return contexts ?? new ItemDataContext[] { };
        }
    }

    protected virtual List<GridContext> AllContexts
    {
        get
        {
            List<GridContext> allContexts = new List<GridContext>();
            IEnumerable<GridContext> addedContexts = OperationContexts;
            allContexts.AddRange(addedContexts);
            allContexts.AddRange(GeneralContexts);
            allContexts.AddRange(LabelContexts);
            return allContexts;
        }
    }

    #endregion


    public virtual void LoadData(WIPMsgMaint_Result result)
    {
        if (result != null)
        {
            AllKeysGrid.ClearData();
            OperationKeysGrid.ClearData();
            LabelKeysGrid.ClearData();

            AllKeysGrid.Data = result.Value.ObjectChanges.AllKey.WIPMsgDetails;
            OperationKeysGrid.Data = result.Value.ObjectChanges.OperationKeys;
            LabelKeysGrid.Data = result.Value.ObjectChanges.LabelKeys;

            // Set Original Data to Current Data.
            // If the submit is successfull, we need to update orginal data with the current data to prevent  secondary re-submit in the future
            OperationKeysGrid.OriginalData = WCFObject.CloneArray(OperationKeysGrid.Data, (OperationKeysGrid.GridContext as ItemDataContext).ItemType);
            LabelKeysGrid.OriginalData = WCFObject.CloneArray(LabelKeysGrid.Data, (LabelKeysGrid.GridContext as ItemDataContext).ItemType);
            AllKeysGrid.OriginalData = WCFObject.CloneArray(AllKeysGrid.Data, (AllKeysGrid.GridContext as ItemDataContext).ItemType);

            AllKeysGrid.Action_Reload("");
            OperationKeysGrid.Action_Reload("");
            LabelKeysGrid.Action_Reload("");
        }
    }

    //adds new meessage to the grid or the subgrids.
    public virtual void AddMsgDetail(OM.WIPMsgDetailChanges msgDetail, OM.WIPMsgTypeEnum type, string name)
    {
        switch (type)
        {
            case OM.WIPMsgTypeEnum.OperationKeys:
                OM.WIPMsgOperationKeyChanges[] operationKeys = (OM.WIPMsgOperationKeyChanges[])OperationKeysGrid.Data == null ? new OM.WIPMsgOperationKeyChanges[0] : (OM.WIPMsgOperationKeyChanges[])OperationKeysGrid.Data;
                var operationKey = operationKeys.SingleOrDefault(op => op.Operation.Name == name);
                if (operationKey == null)
                {
                    //add a new operation key
                    operationKey = new OM.WIPMsgOperationKeyChanges { Operation = new OM.NamedObjectRef { Name = name }, WIPMsgDetails = new OM.WIPMsgDetailChanges[0] };
                    OperationKeysGrid.Data = operationKeys.Concat(new OM.WIPMsgOperationKeyChanges[] { operationKey }).ToArray();
                }
                operationKey.WIPMsgDetails = operationKey.WIPMsgDetails == null ?
                    new OM.WIPMsgDetailChanges[] { msgDetail } : operationKey.WIPMsgDetails.Concat(new OM.WIPMsgDetailChanges[] { msgDetail }).ToArray();
                OperationKeysGrid.GridContext.LoadData();
                break;
            case OM.WIPMsgTypeEnum.AllKeys:
                OM.WIPMsgDetailChanges[] msgDetails = (OM.WIPMsgDetailChanges[])AllKeysGrid.Data == null ? new OM.WIPMsgDetailChanges[0] : (OM.WIPMsgDetailChanges[])AllKeysGrid.Data;
                AllKeysGrid.Data = msgDetails.Concat(new OM.WIPMsgDetailChanges[] { msgDetail }).ToArray();
                AllKeysGrid.GridContext.LoadData();
                break;
            case OM.WIPMsgTypeEnum.LabelKeys:
                OM.WIPMsgLabelKeyChanges[] labelKeys = (OM.WIPMsgLabelKeyChanges[])LabelKeysGrid.Data == null ? new OM.WIPMsgLabelKeyChanges[0] : (OM.WIPMsgLabelKeyChanges[])LabelKeysGrid.Data;
                var labelKey = labelKeys.SingleOrDefault(lb => lb.Label.ToString() == name);
                if (labelKey == null)
                {
                    //add a new label key
                    labelKey = new OM.WIPMsgLabelKeyChanges { Label = name, WIPMsgDetails = new OM.WIPMsgDetailChanges[0] };
                    LabelKeysGrid.Data = labelKeys.Concat(new OM.WIPMsgLabelKeyChanges[] { labelKey }).ToArray();
                }
                labelKey.WIPMsgDetails = labelKey.WIPMsgDetails == null ?
                    new OM.WIPMsgDetailChanges[] { msgDetail } : labelKey.WIPMsgDetails.Concat(new OM.WIPMsgDetailChanges[] { msgDetail }).ToArray();
                LabelKeysGrid.GridContext.LoadData();
                break;
        }
        SelectAndExpandItem(msgDetail, type, name);
    }

    public virtual void SelectAndExpandItem(OM.WIPMsgDetailChanges msgDetail, OM.WIPMsgTypeEnum type, string name)
    {
        switch (type)
        {
            case OM.WIPMsgTypeEnum.OperationKeys:
                //looking for msgDetail in OperationKey grid
                ItemDataContext context = (ItemDataContext)OperationKeysGrid.GridContext;
                OM.WIPMsgOperationKeyChanges[] operationKeys = (OM.WIPMsgOperationKeyChanges[])context.Data;
                int operationIndex = operationKeys.ToList().FindIndex(op => op.Operation.Name == name && op.WIPMsgDetails != null && op.WIPMsgDetails.Contains(msgDetail));
                if (operationIndex > -1)
                {
                    string rowID = context.GetRowId(operationIndex);
                    //expand subgrid
                    context.ExpandRow(new ClientGridState { RowID = rowID, RowDataObject = OperationKeysSubgrid.ContextID });
                    //select a newly added row
                    SelectWIPMsgDetailItem((ItemDataContext)context.GetSubgridRowContext(rowID), msgDetail);
                    context.RenderToClient = true;
                }
                break;

            case OM.WIPMsgTypeEnum.AllKeys:
                //looking for msgDetail in AllKey grid
                bool selected = SelectWIPMsgDetailItem((ItemDataContext)AllKeysGrid.GridContext, msgDetail);
                AllKeysGrid.GridContext.RenderToClient = true;
                //if (selected) return;
                break;
            case OM.WIPMsgTypeEnum.LabelKeys:
                //looking for msgDetail in LabelKey grid
                context = (ItemDataContext)LabelKeysGrid.GridContext;
                OM.WIPMsgLabelKeyChanges[] labelKeys = (OM.WIPMsgLabelKeyChanges[])context.Data;
                int labelIndex = labelKeys.ToList().FindIndex(lb => lb.Label.ToString() == name && lb.WIPMsgDetails != null && lb.WIPMsgDetails.Contains(msgDetail));
                if (labelIndex > -1)
                {
                    string rowID = context.GetRowId(labelIndex);
                    //expand subgrid
                    context.ExpandRow(new ClientGridState { RowID = rowID, RowDataObject = LabelKeysSubgrid.ContextID });
                    //select a newly added row
                    SelectWIPMsgDetailItem((ItemDataContext)context.GetSubgridRowContext(rowID), msgDetail);
                    context.RenderToClient = true;
                    return;
                }
                break;
        }
        return;
    }

    /// <summary>
    /// Forces js-click on client side
    /// </summary>
    /// <param name="context"></param>
    /// <param name="msgDetail"></param>
    /// <returns></returns>
    protected virtual bool SelectWIPMsgDetailItem(ItemDataContext context, OM.WIPMsgDetailChanges msgDetail)
    {
        Array array = (Array)context.Data;
        int count = -1;
        bool success = false;
        foreach (var item in array)
        {
            count++;
            if (item == msgDetail)
            {
                success = true;
                break;
            }
        }
        if (success)
        {
            string rowID = context.GetRowId(count);
            //that controls will force to reselect the row on the client side
            UpdateTreeState(context.ContextID, rowID);
        }
        return success;
    }

    /// <summary>
    /// Update controls that saves information about the selected row. It can forces row selection on the client, if the row is not selected.
    /// </summary>
    /// <param name="activeContextID"></param>
    /// <param name="selectedRowID"></param>
    public virtual void UpdateTreeState(string activeContextID, string selectedRowID)
    {
        if (selectedRowID == null)
            StateControl.Data = null;
        else
            StateControl.Data = string.Format("{0}${1}", activeContextID, selectedRowID);
    }

    public virtual void SaveTreeState(out string rowID, out string rowDataObject, out GridContext context, bool isCopyContext)
    {
        rowID = null;
        rowDataObject = null;
        context = null;

        if (isCopyContext)
        {
            rowDataObject = StateControl.Data.ToString().Split('$')[0];
            rowID = rowDataObject.Substring(rowDataObject.Length - 8);
            if (rowDataObject.Contains("Operation"))
                context = OperationKeysGrid.GridContext;
            else if (rowDataObject.Contains("Label"))
                context = LabelKeysGrid.GridContext;
        }
        else
        {
            foreach (var cnt in AllContexts)
            {
                if (cnt.SelectedRowID != null)
                {
                    UpdateTreeState(cnt.ContextID, cnt.SelectedRowID);
                    if (cnt.ContextID.Contains("OperationKeys"))
                    {
                        int index = ((OM.WIPMsgOperationKeyChanges[])OperationKeysGrid.Data).ToList().FindIndex(op => op.Operation != null && op.Operation.Name == cnt.ParentRowID);
                        if (index > -1)
                        {
                            context = OperationKeysGrid.GridContext;
                            rowID = context.GetRowId(index);
                            rowDataObject = OperationKeysSubgrid.ContextID;
                        }
                    }
                    else if (cnt.ContextID.Contains("LabelKeys"))
                    {
                        int index =
                            ((OM.WIPMsgLabelKeyChanges[])LabelKeysGrid.Data).ToList().FindIndex(lb => lb.Label != null && (string)lb.Label == cnt.ParentRowID);
                        if (index > -1)
                        {
                            context = LabelKeysGrid.GridContext;
                            rowID = context.GetRowId(index);
                            rowDataObject = LabelKeysSubgrid.ContextID;
                        }
                    }
                    break;
                }
            }
        }
    }

    public virtual void UpdateSelectedMsgDetail(OM.WIPMsgDetailChanges message)
    {
        ItemDataContext currContext = (ItemDataContext)AllContexts.FirstOrDefault(c => c.SelectedRowID != null);
        OM.WIPMsgDetailChanges[] data = (OM.WIPMsgDetailChanges[])currContext.Data;
        string rowID = currContext.SelectedRowID;
        if (rowID == null)
            throw new ApplicationException("An item should be selected");
        int id = int.Parse(rowID);
        message.ListItemIndex = data[id].ListItemIndex;
        data[id] = message;
    }

    public virtual void DeleteGroupItem()
    {
        if (OperationKeysGrid.GridContext.SelectedRowID != null)
        {
            OperationKeysGrid.GridContext.Delete(new ClientGridState { RowID = OperationKeysGrid.GridContext.SelectedRowID });
            OperationKeysGrid.GridContext.RenderToClient = true;
        }
        if (LabelKeysGrid.GridContext.SelectedRowID != null)
        {
            LabelKeysGrid.GridContext.Delete(new ClientGridState { RowID = LabelKeysGrid.GridContext.SelectedRowID });
            LabelKeysGrid.GridContext.RenderToClient = true;
        }
    }

    public virtual OM.WIPMsgDetailChanges DeleteSelectedItem(out OM.WIPMsgTypeEnum? type, out string categoryName)
    {
        Dictionary<OM.WIPMsgTypeEnum, IEnumerable<GridContext>> contextDictionary = new Dictionary<OM.WIPMsgTypeEnum, IEnumerable<GridContext>>()
        {
            {OM.WIPMsgTypeEnum.OperationKeys, OperationContexts},
            {OM.WIPMsgTypeEnum.AllKeys, GeneralContexts},
            {OM.WIPMsgTypeEnum.LabelKeys, LabelContexts}
        };
        type = null;
        categoryName = null;
        foreach (var pair in contextDictionary)
        {
            foreach (var context in pair.Value)
            {
                if (context.SelectedRowID != null)
                {
                    context.Delete(new ClientGridState { RowID = context.SelectedRowID });
                    context.RenderToClient = true;
                    type = pair.Key;
                    categoryName = context.ParentRowID;

                    if (context.IsEmpty && context.ParentGridContext != null)
                    {
                        context.ParentGridContext.Delete(new ClientGridState { RowID = categoryName });
                        context.ParentGridContext.RenderToClient = true;
                    }
                }
            }
        }
        return null;
    }


    protected virtual void OnGroupSelected(OM.WIPMsgTypeEnum type, ItemDataContext context)
    {
        //don't trigger the event, if the row is not selected anymore
        if (context.SelectedRowID == null)
            return;
        DeselectedOtherContexts(context);

        UpdateTreeState(context.ContextID, context.SelectedRowID);

        if (Selected != null)
            Selected(this, new MsgEventAgrs(null, null, type, WIPMsgFormTypes.None));
    }

    protected virtual void OnSelected(OM.WIPMsgTypeEnum type, ItemDataContext context)
    {
        //don't trigger the event, if the row is not selected anymore
        if (context.SelectedRowID == null)
            return;
        DeselectedOtherContexts(context);
        if (Selected != null)
        {
            OM.WIPMsgDetailChanges msgChanges = (OM.WIPMsgDetailChanges)context.GetItem(context.SelectedRowID);
            Selected(this, new MsgEventAgrs(msgChanges, context, type, WIPMsgFormTypes.WIPMsgDetails));
        }
        //UpdateTreeState(context.ContextID, context.SelectedRowID);
    }

    protected virtual void DeselectedOtherContexts(ItemDataContext context)
    {
        var allContexts = AllContexts;
        allContexts.ForEach(currCont => { if (currCont != context) { if (currCont.SelectedRowID != null) currCont.SelectRow(null, false); } });

        if (OperationKeysGrid.GridContext.SelectedRowID != null && context != OperationKeysGrid.GridContext)
        {
            OperationKeysGrid.GridContext.SelectRow(null, false);
            OperationKeysGrid.GridContext.RenderToClient = true;
        }

        if (LabelKeysGrid.GridContext.SelectedRowID != null && context != LabelKeysGrid.GridContext)
        {
            LabelKeysGrid.GridContext.SelectRow(null, false);
            LabelKeysGrid.GridContext.RenderToClient = true;
        }
    }
}
