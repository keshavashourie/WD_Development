// Copyright Siemens 2023  
using System;
using System.Data;
using System.Linq;
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.WebControls.PickLists;
using Header = Camstar.WCF.ObjectStack.Header;

namespace Camstar.WebPortal.WebPortlets.Modeling
{
    /// <summary>
    /// Summary description for ModelingESigServicesWP
    /// </summary>
    public class ModelingESigServicesWP : MatrixWebPart
    {
        protected override void OnLoad(EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                FreezeReq.Attributes.CssStyle.Add("display","none !important");
                CurrentFreezeReq.Attributes.CssStyle.Add("display","none !important");
                LabelCache labelCache = FrameworkManagerUtil.GetLabelCache(Page.Session);
                GetAllLabelsInOneRequest(labelCache);
                _noneLabel = labelCache.GetLabelByName("ModelingESigReq_None").Value;
                _inheritLabel = labelCache.GetLabelByName("ModelingESigReq_Inherit").Value;
                _createlabel = labelCache.GetLabelByName("ModelingESigReq_CreateRequirement").Value;
                _updateLabel = labelCache.GetLabelByName("ModelingESigReq_UpdateRequirement").Value;
                _deleteLabel = labelCache.GetLabelByName("ModelingESigReq_DeleteRequirement").Value;
                _freezeLabel = labelCache.GetLabelByName("ModelingESigReq_FreezeRequirement").Value;
                CurrentCreateReq.LabelControl.Text = string.Format("{0}: {1}", _createlabel, string.Empty);
                CurrentUpdateReq.LabelControl.Text = string.Format("{0}: {1}", _updateLabel, string.Empty);
                CurrentDeleteReq.LabelControl.Text = string.Format("{0}: {1}", _deleteLabel, string.Empty);
                CurrentFreezeReq.LabelControl.Text = string.Format("{0}: {1}", _freezeLabel, string.Empty);
                RequestEsigRequirements();

            }
            base.OnLoad(e);
            ModelingESigServicesTree.HideFilter = true;
            ModelingESigServicesTree.DisplayingData += ModelingESigServicesTree_DisplayingData;
            ModelingESigServicesTree.DataChanged += ModelingESigServicesTree_DataChanged;
            SubmitBtn.Click += SubmitBtn_Click;
            if (Page.PortalContext.LocalSession["EsigRequirements"] != null)
            {
                var resultSet = Page.PortalContext.LocalSession["EsigRequirements"] as RecordSet;
                CreateReq.SetSelectionValues(resultSet);
                UpdateReq.SetSelectionValues(resultSet);
                DeleteReq.SetSelectionValues(resultSet);
                FreezeReq.SetSelectionValues(resultSet);
            }
            //handler will restore data after refreshing
            CreateReq.PickListPanelControl.PostProcessData += PickListPanelControl_PostProcessData;
            UpdateReq.PickListPanelControl.PostProcessData += PickListPanelControl_PostProcessData;
            DeleteReq.PickListPanelControl.PostProcessData += PickListPanelControl_PostProcessData;
            FreezeReq.PickListPanelControl.PostProcessData += PickListPanelControl_PostProcessData;

        }

        protected virtual void PickListPanelControl_PostProcessData(object sender, DataRequestEventArgs e)
        {
            var esigs = Page.PortalContext.LocalSession["EsigRequirements"] as RecordSet;
            if (esigs != null)
                e.Data = esigs.GetAsDataTable();
        }

        protected virtual void SubmitBtn_Click(object sender, EventArgs e)
        {
            int cdoId;
            if (!int.TryParse((string)ModelingESigServicesTree.Data, out cdoId))
                return;
            var service = new ModelingESigReqMaintService(FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);
            var data = new ModelingESigReqMaint() {ObjectChanges = new ModelingESigReqChanges() { Name = (string)ModelingESigServicesTree.Data } };
            var request = new ModelingESigReqMaint_Request();

            var createReq = (CreateReq.Data as NamedObjectRef).Name;
            data.ObjectChanges.CreateReqClearInherit = createReq.Equals(_noneLabel);
            data.ObjectChanges.CreateRequirement = (createReq.Equals(_noneLabel) || createReq.Equals(_inheritLabel)) ? new NamedObjectRef("") : CreateReq.Data as NamedObjectRef;
            var updateReq = (UpdateReq.Data as NamedObjectRef).Name;
            data.ObjectChanges.UpdateReqClearInherit = updateReq.Equals(_noneLabel);
            data.ObjectChanges.UpdateRequirement = (updateReq.Equals(_noneLabel) || updateReq.Equals(_inheritLabel)) ? new NamedObjectRef("") : UpdateReq.Data as NamedObjectRef;
            var deleteReq = (DeleteReq.Data as NamedObjectRef).Name;
            data.ObjectChanges.DeleteReqClearInherit = deleteReq.Equals(_noneLabel);
            data.ObjectChanges.DeleteRequirement = (deleteReq.Equals(_noneLabel) || deleteReq.Equals(_inheritLabel)) ? new NamedObjectRef("") : DeleteReq.Data as NamedObjectRef;
            if (IsRdo(cdoId))
            {
                var freezeReq = (FreezeReq.Data as NamedObjectRef).Name;
                data.ObjectChanges.FreezeReqClearInherit = freezeReq.Equals(_noneLabel);
                data.ObjectChanges.FreezeRequirement = (freezeReq.Equals(_noneLabel) || freezeReq.Equals(_inheritLabel))
                    ? new NamedObjectRef()
                    : FreezeReq.Data as NamedObjectRef;
            }
            ModelingESigReqMaint_Result result;
            service.BeginTransaction();
            service.Process(new ModelingESigReqMaint() {ServiceCDOType = cdoId});
            service.ExecuteTransaction(data, request, out result);
            ResultStatus res = service.CommitTransaction();       
            //Page.RenderToClient = true;
            Page.ClearValues();
            ModelingESigServicesTree.Data = null;
            SelectedCdoName.Data = null;
            FreezeReq.Attributes.CssStyle.Add("display", "none !important");
            CurrentFreezeReq.Attributes.CssStyle.Add("display", "none !important");
            CurrentCreateReq.LabelControl.Text = string.Format("{0}: {1}", _createlabel, string.Empty);
            CurrentUpdateReq.LabelControl.Text = string.Format("{0}: {1}", _updateLabel, string.Empty);
            CurrentDeleteReq.LabelControl.Text = string.Format("{0}: {1}", _deleteLabel, string.Empty);
            CurrentFreezeReq.LabelControl.Text = string.Format("{0}: {1}", _freezeLabel, string.Empty);
            Page.DisplayMessage(res);
        }

        protected virtual void ModelingESigServicesTree_DataChanged(object sender, EventArgs e)
        {
            int cdoId;
            if(!int.TryParse((string)ModelingESigServicesTree.Data,out cdoId))
                return;
            if (SelectedCdoName != null)
            {
                var Cdos = (Page.PortalContext.LocalSession["CDOs"] as DataTable).Rows.OfType<DataRow>().ToList();
                var cdo = Cdos.First(c => c["MaintenanceTypeID"].Equals(cdoId));
                SelectedCdoName.Data = cdo["MaintenanceDisplayName"];
            }
            if (!IsRdo(cdoId))
            {
                FreezeReq.Attributes.CssStyle.Add("display", "none !important");
                CurrentFreezeReq.Attributes.CssStyle.Add("display", "none !important");
            }
            else
            {
                FreezeReq.Attributes.CssStyle.Remove("display");
                CurrentFreezeReq.Attributes.CssStyle.Remove("display");
            }


            var service = new ModelingESigReqMaintService(FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);
            var data = new ModelingESigReqMaint() {ServiceCDOType = cdoId};
            var request = new ModelingESigReqMaint_Request
            {
                Info = new ModelingESigReqMaint_Info() {ObjectChanges = new ModelingESigReqChanges_Info()
                {
                    ResolvedCreateReq = new Info(true),
                    ResolvedDeleteReq = new Info(true),
                    ResolvedUpdateReq = new Info(true),
                    ResolvedFreezeReq = FreezeReq.Hidden ? null : new Info(true),
                    CreateResolvedAt = new Info(true),
                    UpdateResolvedAt = new Info(true),
                    DeleteResolvedAt = new Info(true),
                    FreezeResolvedAt = FreezeReq.Hidden ? null : new Info(true),
                    CreateReqClearInherit = new Info(true),
                    UpdateReqClearInherit = new Info(true),
                    DeleteReqClearInherit = new Info(true),
                    FreezeReqClearInherit = FreezeReq.Hidden ? null : new Info(true),
                    CreateRequirement = new Info(true),
                    UpdateRequirement = new Info(true),
                    DeleteRequirement = new Info(true),
                    FreezeRequirement = FreezeReq.Hidden ? null : new Info(true),
                    ChangeHistory = new ChangeHistoryChanges_Info() {LastChangeDate = new Info(true), User = new Info(true)}
                }}
            };
            ModelingESigReqMaint_Result result;
            ResultStatus res = service.LoadValues(data, request, out result);
            if (!res.IsSuccess)
            {
                DisplayMessage(res);
                return;
            }
            var requestedChanges = result.Value.ObjectChanges;
            if (requestedChanges == null)
                return;
            CurrentCreateReq.Data = requestedChanges.ResolvedCreateReq;
            CurrentUpdateReq.Data = requestedChanges.ResolvedUpdateReq;
            CurrentDeleteReq.Data = requestedChanges.ResolvedDeleteReq;
            CurrentCreateReq.LabelControl.Text = string.Format("{0}: {1}", _createlabel, requestedChanges.CreateResolvedAt == null ? string.Empty : requestedChanges.CreateResolvedAt.Value);
            CurrentUpdateReq.LabelControl.Text = string.Format("{0}: {1}", _updateLabel, requestedChanges.UpdateResolvedAt == null ? string.Empty : requestedChanges.UpdateResolvedAt.Value);
            CurrentDeleteReq.LabelControl.Text = string.Format("{0}: {1}", _deleteLabel, requestedChanges.DeleteResolvedAt == null ? string.Empty : requestedChanges.DeleteResolvedAt.Value);
            CreateReq.Data = requestedChanges.CreateRequirement ?? (requestedChanges.CreateReqClearInherit.Value
                ? new NamedObjectRef(_noneLabel)
                : new NamedObjectRef(_inheritLabel));
            UpdateReq.Data = requestedChanges.UpdateRequirement ?? (requestedChanges.UpdateReqClearInherit.Value
                ? new NamedObjectRef(_noneLabel)
                : new NamedObjectRef(_inheritLabel));
            DeleteReq.Data = requestedChanges.DeleteRequirement ?? (requestedChanges.DeleteReqClearInherit.Value
                ? new NamedObjectRef(_noneLabel)
                : new NamedObjectRef(_inheritLabel));
            if (!FreezeReq.Hidden && !CurrentFreezeReq.Hidden)
            {
                CurrentFreezeReq.LabelControl.Text = string.Format("{0}: {1}", _freezeLabel, requestedChanges.FreezeResolvedAt == null ? string.Empty : requestedChanges.FreezeResolvedAt.Value);
                CurrentFreezeReq.Data = requestedChanges.ResolvedFreezeReq;
                FreezeReq.Data = requestedChanges.FreezeRequirement ?? (requestedChanges.FreezeReqClearInherit.Value
                    ? new NamedObjectRef(_noneLabel)
                    : new NamedObjectRef(_inheritLabel));
            }
            UserField.Data = requestedChanges.ChangeHistory.User;
            if (requestedChanges.ChangeHistory.LastChangeDate != null)
                ChangedDateField.Data = requestedChanges.ChangeHistory.LastChangeDate.Value;
            else
                ChangedDateField.Data = null;
        }

        protected virtual bool IsRdo(int cdoId)
        {
            var Cdos = (Page.PortalContext.LocalSession["CDOs"] as DataTable).Rows.OfType<DataRow>().ToList();
            var cdo = Cdos.First(c => c["MaintenanceTypeID"].Equals(cdoId));
            var topParent = cdo;
            while (true)
            {
                var parent = Cdos.FirstOrDefault(c => c["MaintenanceTypeID"].Equals(topParent["ParentCDODefId"]));
                if (parent != null)
                {
                    topParent = parent;
                    continue;
                }
                break;
            }
            return (int)topParent["MaintenanceTypeID"] == 3460;
        }

        protected virtual void RequestEsigRequirements()
        {
            var service = new ESigRequirementMaintService(FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);
            var cdo = new ESigRequirementMaint();
            var request = new ESigRequirementMaint_Request
            {
                Info = new ESigRequirementMaint_Info() { ObjectToChange = new Info(false, true) }
            };
            ESigRequirementMaint_Result result;
            ResultStatus res = service.GetEnvironment(cdo, request, out result);
            if (!res.IsSuccess)
            {
                DisplayMessage(res);
                return;
            }
            var selVals = result.Environment.ObjectToChange.SelectionValues;
            var nameIndex = Array.IndexOf(selVals.Headers, selVals.Headers.First(h=>h.Name.Equals("Name")));
            var instanceIdIndex = Array.IndexOf(selVals.Headers, selVals.Headers.First(h => h.Name.Equals("InstanceId")));
            var totalCount = selVals.Rows == null ? 2 : selVals.Rows.Length + 2;
            var rows = new Row[totalCount];
            rows[0]=new Row() {Values = new [] {_noneLabel,string.Empty}};
            rows[1] = new Row() { Values = new[] { _inheritLabel, string.Empty } };
            for (int i = 2; i < totalCount; i++)
            {
                rows[i] = new Row() { Values = new[]
                {
                    selVals.Rows[i-2].Values[nameIndex],
                    selVals.Rows[i-2].Values[instanceIdIndex]
                } };
            }
            var resultSet = new RecordSet()
            {
                TotalCount = result.Environment.ObjectToChange.SelectionValues.TotalCount + 2,
                Headers = new Header[] {new Header() {Name = "Name"}, new Header() {Name = "InstanceId"}},
                Rows = rows
            };
            Page.PortalContext.LocalSession["EsigRequirements"] = resultSet;
            CreateReq.ListDisplayExpression = CreateReq.ListValueColumn =
                UpdateReq.ListDisplayExpression = UpdateReq.ListValueColumn =
                    DeleteReq.ListDisplayExpression = UpdateReq.ListValueColumn =
                        FreezeReq.ListDisplayExpression = FreezeReq.ListValueColumn = "Name";
        }

        protected virtual void ModelingESigServicesTree_DisplayingData(object sender, DataRequestEventArgs e)
        {
            Page.PortalContext.LocalSession["CDOs"] = _cdoDAtaSource = e.OriginalData as DataTable;
            if (_cdoDAtaSource != null && _cdoDAtaSource.Rows.Count > 0)
            {
                var item = new tree_row("Root", "1120", "", "", false);
                CollectChildren(item, "1120");
                e.Data = item.children;
            }
            e.ViewMode = "tree";
        }

        protected virtual void CollectChildren(tree_row parent, string type)
        {
            type = type ?? "";
            foreach (var o in _cdoDAtaSource.Rows)
            {
                var row = o as DataRow;
                if (row != null)
                {
                    var rowType = row["ParentCDODefId"].ToString();
                    if (rowType == type)
                    {
                        var item = parent.Children.FirstOrDefault(ch => ch.text == row["MaintenanceTypeID"].ToString());
                        if (item == null) // Add new Group
                        {
                            item = new tree_row(row["MaintenanceDisplayName"].ToString(), row["MaintenanceTypeID"].ToString(), "", "", true);
                            parent.AddChild(item);
                        }
                        CollectChildren(item, row["MaintenanceTypeID"].ToString());
                    }
                }
            }
        }
        
        private void GetAllLabelsInOneRequest(LabelCache labelCache)
        {
            var labels = new[]{
                new WCF.ObjectStack.Label("ModelingESigReq_None"),
                new WCF.ObjectStack.Label("ModelingESigReq_Inherit"),
                new WCF.ObjectStack.Label("ModelingESigReq_CreateRequirement"),
                new WCF.ObjectStack.Label("ModelingESigReq_UpdateRequirement"),
                new WCF.ObjectStack.Label("ModelingESigReq_DeleteRequirement"),
                new WCF.ObjectStack.Label("ModelingESigReq_FreezeRequirement")
            };

            labelCache.GetLabels(new LabelList(labels));
        }
        #region Properties
        protected virtual DropDownList ModelingESigServicesTree
        {
            get{ return Page.FindCamstarControl("ModelingESigServicesTree") as DropDownList; }
        }
        protected virtual NamedObject CreateReq
        {
            get { return Page.FindCamstarControl("CreateReq") as NamedObject; }
        }
        protected virtual NamedObject CurrentCreateReq
        {
            get { return Page.FindCamstarControl("CurrentCreateReq") as NamedObject; }
        }
        protected virtual NamedObject FreezeReq
        {
            get { return Page.FindCamstarControl("FreezeReq") as NamedObject; }
        }
        protected virtual NamedObject CurrentFreezeReq
        {
            get { return Page.FindCamstarControl("CurrentFreezeReq") as NamedObject; }
        }
        protected virtual NamedObject UpdateReq
        {
            get { return Page.FindCamstarControl("UpdateReq") as NamedObject; }
        }
        protected virtual NamedObject CurrentUpdateReq
        {
            get { return Page.FindCamstarControl("CurrentUpdateReq") as NamedObject; }
        }
        protected virtual NamedObject DeleteReq
        {
            get { return Page.FindCamstarControl("DeleteReq") as NamedObject; }
        }
        protected virtual NamedObject CurrentDeleteReq
        {
            get { return Page.FindCamstarControl("CurrentDeleteReq") as NamedObject; }
        }
        protected virtual NamedObject UserField
        {
            get { return Page.FindCamstarControl("UserField") as NamedObject; }
        }
        protected virtual TextBox SelectedCdoName
        {
            get { return Page.FindCamstarControl("SelectedCdoName") as TextBox; }
        }
        protected virtual DateChooser ChangedDateField
        {
            get { return Page.FindCamstarControl("ChangedDateField") as DateChooser; }
        }
        protected virtual Button SubmitBtn
        {
            get { return Page.FindCamstarControl("SubmitBtn") as Button; }
        }
        private DataTable _cdoDAtaSource;
        private static string _noneLabel;
        private static string _inheritLabel;
        private static string _createlabel;
        private static string _updateLabel;
        private static string _deleteLabel;
        private static string _freezeLabel;

        #endregion

    }
}
