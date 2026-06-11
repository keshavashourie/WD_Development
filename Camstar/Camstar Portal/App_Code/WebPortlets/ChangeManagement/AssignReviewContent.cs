// Copyright Siemens 2023  
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.PortalFramework;
using DT = Camstar.WebPortal.WebPortlets.DataTransfer;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using UIAction = Camstar.WebPortal.Personalization.UIAction;
using System.Text;

namespace Camstar.WebPortal.WebPortlets.ChangeManagement
{

    /// <summary>
    /// TODO: Add a Summary description for this Camstar Web Part
    /// </summary>
    public class AssignReviewContent : MatrixWebPart
    {
        #region Controls

        protected virtual JQDataGrid ItemsSelectedGrid { get { return Page.FindCamstarControl("ItemsSelectedGrid") as JQDataGrid; } }
        protected virtual JQDataGrid ObjectTypeGrid { get; set; }
        protected virtual CWC.CheckBox HideDepControl { get { return Page.FindCamstarControl("HideDepControl") as CWC.CheckBox; } }
        protected virtual CWC.Button ContentHierarchy { get { return Page.FindCamstarControl("ContentHierarchy") as CWC.Button; } }
        protected virtual CWC.NamedObject SelectedPackage { get { return Page.FindCamstarControl("ChangePackageHeader_ChangePackage") as CWC.NamedObject; } }
        protected virtual CWC.InquiryControl ChangePackageHeader_Desc { get { return Page.FindCamstarControl("Description") as CWC.InquiryControl; } }
        protected virtual CWC.InquiryControl ChangePackageHeader_LastUpdate { get { return Page.FindCamstarControl("LastUpdated") as CWC.InquiryControl; } }
        protected virtual CWC.InquiryControl ChangePackageHeader_Owner { get { return Page.FindCamstarControl("Owner") as CWC.InquiryControl; } }
        protected virtual CWC.InquiryControl ChangePackageHeader_Targets { get { return Page.FindCamstarControl("Targets") as CWC.InquiryControl; } }
        protected virtual CWC.Button AddOrRemoveDependencies { get { return Page.FindCamstarControl("AddOrRemoveDependencies") as CWC.Button; } }
        protected virtual CWC.TextBox GeneralInstructions { get { return Page.FindCamstarControl("GeneralInstructions") as CWC.TextBox; } }
        protected virtual CWC.TextBox SpecialInstructions { get { return Page.FindCamstarControl("SpecialInstructions") as CWC.TextBox; } }
        protected virtual JQDataGrid CollaboratorsGrid { get { return Page.FindCamstarControl("CollaboratorEntriesGrid") as JQDataGrid; } }
        protected virtual JQTabContainer TabContainer { get { return Page.FindCamstarControl("PageTab") as JQTabContainer; } }

        #endregion

        protected virtual UIAction SubmitAction { get { return Page.ActionDispatcher.PageActions().FirstOrDefault(a => a is SubmitAction); } }
        protected virtual UIAction CompleteAction { get { return Page.ActionDispatcher.PageActions().FirstOrDefault(a => a is FloatPageOpenAction); } }
        protected virtual UIAction ResetAction { get { return Page.ActionDispatcher.PageActions().FirstOrDefault(a => a is CustomAction); } }
        protected virtual Boolean IsCollaborator { get { return Convert.ToBoolean(Page.SessionVariables["IsCollaborator"]); } }
        protected virtual Boolean IsPackageOwner { get { return Convert.ToBoolean(Page.SessionVariables["IsPackageOwner"]); } }
        protected virtual Boolean IsOwnerRole { get { return Convert.ToBoolean(Page.SessionVariables["IsOwnerRole"]); } }


        #region Protected Functions

        #region Webparts
        protected virtual InstanceList InstanceList
        {
            get
            {
                var proxy = Page.CamstarControls.Single(w => w.ID == "InstanceList_WP");
                return proxy != null ? proxy.AsIForm as InstanceList : null; ;
            }
        }
        protected virtual MatrixWebPart InstanceHeader
        {
            get
            {
                var proxy = Page.CamstarControls.Single(w => w.ID == "CM_InstanceHeader_WP");
                return proxy != null ? proxy.AsIForm as MatrixWebPart : null; ;
            }
        }
        #endregion

        /// <summary>
        /// TODO: Summary Description of function
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            TabContainer.SelectedIndexChanged += TabContainer_SelectedIndexChanged;
            HideDepControl.DataChanged += (sender, args) => ShowHideDependencies();
            ObjectTypeGrid = Page.FindCamstarControl("ObjectTypeGrid") as JQDataGrid;

            if (!Page.IsPostBack)
                InstanceList.ResetFilter();

            if (string.IsNullOrEmpty(_circRefError))
            {
                _circRefError = FrameworkManagerUtil.GetLabelValue("CircularRefErrorLbl") ?? string.Empty;
            }

            if (AddOrRemoveDependencies != null)
            {
                if (ViewState[DependenciesAdded] == null || (!(bool)ViewState[DependenciesAdded]))
                {
                    ReloadAddorRemoveButton(true);
                    CamstarWebControl.SetRenderToClient(AddOrRemoveDependencies);
                }
                AddOrRemoveDependencies.Click += (sender, args) =>
                {
                    if (ViewState[DependenciesAdded] == null || (!(bool)ViewState[DependenciesAdded]))
                    {
                        AddDependencies();
                        ReloadAddorRemoveButton(false);
                    }
                    else
                    {
                        DeleteDependencies();
                        ReloadAddorRemoveButton(true);
                    }
                    CamstarWebControl.SetRenderToClient(AddOrRemoveDependencies);
                };
            }
            SelectedPackage.DataChanged += SelectedPackage_DataChanged;
            var newInstances = Page.DataContract.GetValueByName("SelectedInstances") as ObjectTypeItem[];
            if (!HideDepControl.IsChecked)
                ObjectTypeGrid.Data = newInstances;
            ObjectTypeGrid.GridContext.DataChanged += GridContext_DataChanged;
            ItemsSelectedGrid.BoundContext.SnapCompleted += ItemsSelectedGrid_SnapCompleted;

            ItemsSelectedGrid.GridContext.RowDeleted += (sender, args) =>
            {
                Page.PortalContext.LocalSession[CheckSelectedInstancesCount] = true;
                //revalidate confirmation message in case selected instances droped to allowed quantity
                ((DirectUpdateData)args.Response).PropertyValue = ((DirectUpdateData)args.Response).PropertyValue.Replace("PostBackRequested:false", "PostBackRequested:true");
                return args.Response;
            };
            var checkCount = Page.PortalContext.LocalSession[CheckSelectedInstancesCount];
            if (checkCount != null && (bool)checkCount)
            {
                CheckSelectionLimit();
                Page.PortalContext.LocalSession[CheckSelectedInstancesCount] = false;
            }
            var whereUsedObjects = Page.PortalContext.LocalSession[AssignReviewContentWhereUsed.WhereUsedObjects] as ObjectTypeItem[];
            if (whereUsedObjects != null && whereUsedObjects.Length > 0)
            {
                var newItems = new List<ObjectTypeItem>(ObjectTypeGrid.Data as ObjectTypeItem[]);
                //check, whether passed instances are alredy present in the grid
                foreach (var item in whereUsedObjects)
                {
                    var index = newItems.FindIndex(i => i.DisplayName == item.DisplayName);
                    if (index == -1)
                        newItems.Add(item);
                    else
                    {
                        var instances = new List<SelectedInstanceItem>(newItems[index].Instances);
                        foreach (var instance in item.Instances)
                        {
                            var instanceIndex = Array.FindIndex(newItems[index].Instances,
                                i => i.InstanceID.Equals(instance.InstanceID));
                            if (instanceIndex == -1)
                            {
                                instances.Add(instance);
                            }
                        }
                        newItems[index].Instances = instances.ToArray();
                    }
                }
                ObjectTypeGrid.Data = newItems.ToArray();
                Page.DataContract.SetValueByName("SelectedInstances", ObjectTypeGrid.Data);
                Page.PortalContext.LocalSession.Remove(AssignReviewContentWhereUsed.WhereUsedObjects);
            }
            var whereUsedBtn = Page.FindCamstarControl("WhereUsedBtn") as CWC.Button;
            if (whereUsedBtn != null || Page.DataContract.GetValueByName("WhereUsedPageName") as String != null)
                whereUsedBtn.Click += (sender, args) =>
                {
                    GetAllLabelsInOneRequest();

                    var subGrids = ItemsSelectedGrid != null ? ItemsSelectedGrid.GridContext.GetSubContexts() : null;
                    if (subGrids == null)
                    {
                        var labelCache = FrameworkManagerUtil.GetLabelCache(HttpContext.Current.Session);
                        Page.DisplayWarning(labelCache.GetLabelByName("Lbl_NumberOfWhereUsedInstancesMustBeGreater").Value);
                        return;
                    }

                    var service = new AssignChangePkgContentService(FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);
                    var data = new AssignChangePkgContent();
                    data.ChangePackage = SelectedPackage.Data as NamedObjectRef;
                    var request = new AssignChangePkgContent_Request()
                    {
                        Info = new AssignChangePkgContent_Info()
                        {
                            MaxWhereUsedInstances = new Info(true)
                        }
                    };

                    AssignChangePkgContent_Result result;
                    var rs = service.Load(data, request, out result);
                    var maxWhereUsedCount = result.Value.MaxWhereUsedInstances.Value;

                    if (maxWhereUsedCount == 0)
                    {
                        var labelCache = FrameworkManagerUtil.GetLabelCache(HttpContext.Current.Session);
                        Page.DisplayMessage(labelCache.GetLabelByName("Lbl_WhereUsedNoSettingsAssociated").Value, false);
                        return;
                    }

                    List<object> selectedItems = new List<object>();
                    var selectedItemsCount = 0;
                    foreach (var grid in subGrids.Where(s => s.GetSelectedItems(false) != null))
                    {
                        selectedItemsCount += grid.GetSelectedItems(false).Length;
                        if (selectedItemsCount > maxWhereUsedCount)
                        {
                            var labelCache = FrameworkManagerUtil.GetLabelCache(HttpContext.Current.Session);
                            Page.DisplayMessage(labelCache.GetLabelByName("Lbl_WhereUsedExceedsLimit").Value, false);
                            return;
                        }
                        else
                        {
                            selectedItems.AddRange(grid.GetSelectedItems(false).ToList());
                        }

                    }
                    if (selectedItemsCount == 0)
                    {
                        var labelCache = FrameworkManagerUtil.GetLabelCache(HttpContext.Current.Session);
                        Page.DisplayWarning(labelCache.GetLabelByName("Lbl_NumberOfWhereUsedInstancesMustBeGreater").Value);
                        return;
                    }
                    if (selectedItems != null)
                    {
                        var action = new FloatPageOpenAction()
                        {
                            ESignatureRequired = BooleanTriState.No,
                            PageName = (string)Page.DataContract.GetValueByName("WhereUsedPageName"),
                            ShowButtons = true,
                            DataContractMap = new UIComponentDataContractMap
                            {
                                Links = new[]
                                {
                                    new UIComponentDataContractLink
                                    {
                                        SourceMember = "WhereUsedObject",
                                        TargetMember = "WhereUsedObject"
                                    },
                                    new UIComponentDataContractLink
                                    {
                                        SourceMember = "ChangePackage",
                                        TargetMember = "ChangePackage"
                                    },
                                    new UIComponentDataContractLink
                                    {
                                        SourceMember = "WhereCame",
                                        TargetMember = "WhereCame"
                                    },
                                    new UIComponentDataContractLink
                                    {
                                        SourceMember = "IsChangeMgtSettingsRequired",
                                        TargetMember = "IsChangeMgtSettingsRequired"
                                    }
                                }
                            }
                        };
                        Page.DataContract.SetValueByName("WhereUsedObject", selectedItems);
                        Page.DataContract.SetValueByName("IsChangeMgtSettingsRequired", true);
                        (HttpContext.Current.CurrentHandler as IActionContainer).ActionDispatcher.ExecuteAction(action);
                    }
                };

            SubmitAction.Execution = new UIActionExecution();
            SubmitAction.Execution.Executed += ((sender, ee) => SelectedPackage.Data = Page.DataContract.GetValueByName("ChangePackage"));

            ResetAction.Execution = new UIActionExecution();
            ResetAction.Execution.Executed += ((sender, ee) => SelectedPackage.Data = Page.DataContract.GetValueByName("ChangePackage"));

            var popupCmd = Page.PortalContext.DataContract.GetValueByName<string>("CompletePopupCommand");
            Page.PortalContext.DataContract.SetValueByName("CompletePopupCommand", "");
            if (!string.IsNullOrEmpty(popupCmd))
            {
                if (SubmitAction != null)
                {
                    _isComplete = true;
                    Page.ActionDispatcher.ExecuteAction(SubmitAction);
                }
            }
        }

        private void GetAllLabelsInOneRequest()
        {
            var labelCache = FrameworkManagerUtil.GetLabelCache(HttpContext.Current.Session);
            var labels = new[]
            {
                new Label("Lbl_NumberOfWhereUsedInstancesMustBeGreater"),
                new Label("Lbl_WhereUsedNoSettingsAssociated"),
                new Label("Lbl_WhereUsedExceedsLimit"),
                new Label("Lbl_NumberOfWhereUsedInstancesMustBeGreater"),
                new Label("Lbl_AddDependenciesBtn"),
                new Label("Lbl_RemoveDependenciesBtn"), 
            };
            labelCache.GetLabels(new LabelList(labels));
        }

        protected virtual void TabContainer_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (TabContainer.SelectedIndex == 1)
            {
                (SubmitAction.Control as CWC.Button).Visible = false;
                (ResetAction.Control as CWC.Button).Visible = false;
                (CompleteAction.Control as CWC.Button).Visible = false;
            }
            else
            {
                (SubmitAction.Control as CWC.Button).Visible = true;
                (ResetAction.Control as CWC.Button).Visible = true;
                (CompleteAction.Control as CWC.Button).Visible = true;
            }
        }

        public override void ClearValues(Service serviceData)
        {
            base.ClearValues(serviceData);
        }
        ResponseData GridContext_DataChanged(object sender, JQGridEventArgs args)
        {
            var childCont = sender as GridContext;

            if (childCont != null && childCont.ParentGridContext != null)
            {
                var cont = childCont.ParentGridContext as ItemDataContext;
                var selectedInstances = cont.Data as ObjectTypeItem[];
                if (selectedInstances != null)
                {
                    var oldCount = selectedInstances.Length;
                    // Cleanup empty Object Type
                    var items =
                        from d in selectedInstances
                        where d.Instances != null && d.Instances.Length > 0
                        select d;

                    var objectTypeItems = items as ObjectTypeItem[] ?? items.ToArray();
                    if (objectTypeItems.Count() < oldCount)
                    {
                        cont.Data = objectTypeItems;
                        Page.DataContract.SetValueByName("SelectedInstances", objectTypeItems);
                        if (cont.ExpandedRowIDs != null)
                        {
                            cont.ExpandedRowIDs.Clear();
                            cont.ExpandedRowID = null;
                        }
                    }
                }
            }

            return null;
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            InstanceList.Hidden = SelectedPackage.IsEmpty;

            var noCollaborators = (CollaboratorsGrid.Data as Array) == null || (CollaboratorsGrid.Data as Array).Length == 0;

            if ((IsPackageOwner || IsOwnerRole) && noCollaborators)
            {
                TabContainer.Tabs[1].Visible = false;
            }
            if (!IsCollaborator)
            {
                if ((IsPackageOwner || IsOwnerRole) && !noCollaborators)
                {
                    GeneralInstructions.Visible = false;
                    SpecialInstructions.Visible = false;
                }
                if (!IsPackageOwner && !IsOwnerRole)
                {
                    TabContainer.Tabs[1].Visible = false;
                }
                (CompleteAction.Control as CWC.Button).Visible = false;
            }
        }

        protected virtual void SelectedPackage_DataChanged(object sender, EventArgs e)
        {
            Page.DataContract.SetValueByName("SelectedInstances", null);
            if (SelectedPackage.Data == null)
                return;

            var service = new AssignChangePkgContentService(FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);
            var data = new AssignChangePkgContent();
            data.ChangePackage = SelectedPackage.Data as NamedObjectRef;
            var request = new AssignChangePkgContent_Request()
            {
                Info = new AssignChangePkgContent_Info()
                {

                    ChangePackageHeader = new ChangePackageHeader_Info()
                    {
                        RequestValue = true
                    },
                    ServiceDetail = new AssignChangePkgContentDtl_Info()
                    {
                        Instances = new CPModelingInstanceDtl_Info() { RequestValue = true }
                    },
                    CollaboratorDetails = new CollaboratorDetails_Info()
                    {
                        RequestValue = true
                    },
                    CollaboratorEntry = new CollaboratorEntryDetails_Info()
                    {
                        RequestValue = true
                    },
                    IsCollaborator = new Info(true),
                    IsPackageOwner = new Info(true),
                    IsOwnerRole = new Info(true)
                }
            };

            AssignChangePkgContent_Result result;
            var rs = service.Load(data, request, out result);
            if (rs.IsSuccess && result.Value.ServiceDetail != null)
            {
                var instances = result.Value.ServiceDetail.Instances;
                if (instances != null)
                {
                    var badInstances = instances.Where<CPModelingInstanceDtl>(x => x.IsBadReference != null && x.IsBadReference.Value).ToList();

                    if (badInstances.Any())
                    {
                        Page.DisplayMessage(badInstances.First().ErrorMessage.Value, false);
                    }

                    var otList =
                        from inst in instances
                        group inst by new { TypeName = inst.ObjectTypeName.Value, TypeValue = inst.ObjectTypeValue.Value }
                            into g
                        orderby g.Key.TypeValue
                        select
                            new ObjectTypeItem()
                            {
                                Name = g.Key.TypeName,
                                DisplayName = g.Key.TypeValue,
                                Instances =
                                    (from n in instances
                                     where n.ObjectTypeValue.Value == g.Key.TypeValue
                                     select new SelectedInstanceItem
                                     {
                                         InstanceID = n.ObjectInstanceId.Value,
                                         CDOTypeID = n.ObjectType.Value.ToString(),
                                         CDOTypeName = n.ObjectTypeName.Value,
                                         CDOTypeValue = n.ObjectTypeValue.Value,
                                         Name = n.ObjectName == null ? null : n.ObjectName.Value,
                                         DisplayedName = n.DisplayedName == null ? (n.IsBadReference.Value && n.ObjectName != null ? n.ObjectName.Value : null) : n.DisplayedName.Value,
                                         Revision = n.Revision == null ? null : n.Revision.Value,
                                         IsRef = n.IsReference.Value,
                                         Order = n.Sequence == null ? 0 : n.Sequence.Value,
                                         Description = n.Description == null ? null : n.Description.Value,
                                         LastEditedBy = n.LastUpdatedBy == null ? null : n.LastUpdatedBy.Name,
                                         LastEditedTime = n.LastUpdatedDate == null ? (DateTime?)null : n.LastUpdatedDate.Value,
                                         AddedToPackageGMT = n.AddedToPackageGMT == null ? (DateTime?)null : n.AddedToPackageGMT.Value
                                     }
                                        ).OrderBy(f => f.Name).ToArray()
                            };
                    ObjectTypeGrid.Data = otList.OrderBy(v => v.DisplayName).ToArray();
                    Page.DataContract.SetValueByName("SelectedInstances", ObjectTypeGrid.Data);

                    CheckReference();
                }
            }

            if (rs.IsSuccess && result.Value.ChangePackageHeader != null)
            {
                ChangePackageHeader_Desc.Data = result.Value.ChangePackageHeader.Description;
                ChangePackageHeader_LastUpdate.Data = result.Value.ChangePackageHeader.LastUpdatedDate;
                ChangePackageHeader_Owner.Data = result.Value.ChangePackageHeader.OwnerName;
                ChangePackageHeader_Targets.Data = result.Value.ChangePackageHeader.TargetSystemName;
                if (result.Value.CollaboratorDetails != null)
                {
                    GeneralInstructions.Data = result.Value.CollaboratorDetails.GeneralInstructions;
                    SetupGridData(result.Value.CollaboratorDetails.CollaboratorEntries);
                    Page.SessionVariables["MessageToCollaborator"] = result.Value.CollaboratorDetails.EMailMessageToCollaborator;
                    Page.SessionVariables["MessageToOwner"] = result.Value.CollaboratorDetails.EMailMessageToOwner;
                }
                Page.SessionVariables["IsCollaborator"] = result.Value.IsCollaborator.Value;
                Page.SessionVariables["IsPackageOwner"] = result.Value.IsPackageOwner.Value;
                Page.SessionVariables["IsOwnerRole"] = result.Value.IsOwnerRole.Value;
            }
        }

        protected virtual void SetupGridData(CollaboratorEntryDetails[] collaboratorEntries)
        {
            if (collaboratorEntries != null)
            {
                CollaboratorsGrid.Data = collaboratorEntries;
                var entryesToComplete = collaboratorEntries.Where(c => (bool)c.IsAbleToComplete).ToArray();
                Page.PortalContext.DataContract.SetValueByName("CollaboratorEntries", entryesToComplete);
                var defaultSelectedItem = collaboratorEntries.FirstOrDefault(c => (bool)c.IsDefaultToComplete);
                if (defaultSelectedItem != null)
                {
                    var selectedRowId = Array.IndexOf(collaboratorEntries, defaultSelectedItem);
                    var popupDefaultRowId = Array.IndexOf(entryesToComplete, defaultSelectedItem);
                    var rowId = CollaboratorsGrid.BoundContext.MakeAutoRowId(selectedRowId);
                    var popupRowId = CollaboratorsGrid.BoundContext.MakeAutoRowId(popupDefaultRowId);
                    CollaboratorsGrid.GridContext.SelectRow(rowId, true);
                    SpecialInstructions.Data = defaultSelectedItem.SpecialInstructions;
                    Page.SessionVariables["SelectedRowID"] = popupRowId;

                }
            }
        }

        public override void GetInputData(Service serviceData)
        {
            base.GetInputData(serviceData);

            var srv = serviceData as AssignChangePkgContent;

            var gridData = HideDepControl.IsChecked ? Page.PortalContext.LocalSession["DataWithDependencies"] as ObjectTypeItem[] : ObjectTypeGrid.Data as ObjectTypeItem[];
            if (srv != null && gridData != null)
            {
                var instances = new List<SelectedInstanceItem>();
                foreach (var d in gridData)
                {
                    if (d.Instances.Any())
                        instances.AddRange(d.Instances);
                }

                // Check order if not references not added                
                var transfer = new DT.DataTransfer
                    (
                        new DT.DataTransferInfo(FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile.Name),
                        new DT.DataTransferRepository(FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile),
                        new Dictionary<string, string>()
                    );                                
                try
                {
                    if (!instances.Any(x => x.IsRef))
                    {
                        var orderedItems = transfer.OrderSelectedInstances(instances.ToArray());
                        if (_circularRefInstances != null)
                        {
                            StringBuilder sb = new StringBuilder();
                            if (_circularRefInstances.Count > 0)
                            {
                                var grpedInstances = _circularRefInstances
                                                        .GroupBy(x => x.Value)
                                                        .ToDictionary(y => y.Key, y => y.Select(x => x.Key).ToArray());

                                foreach (var pair in grpedInstances)
                                {
                                    var i = 0;
                                    foreach (var instance in pair.Value)
                                    {
                                        if (i > 0)
                                        {
                                            sb.Append(",");
                                        }
                                        else
                                        {
                                            sb.Append(pair.Key + ":");
                                        }
                                        sb.Append(instance);
                                        i++;
                                    }
                                    sb.Append(";");
                                }
                                _circRefError += sb.ToString();
                                Page.DisplayWarning(_circRefError);
                            }
                        }
                        instances = orderedItems != null ? orderedItems.ToList() : instances;
                    }
                }
                catch (Exception ex)
                {
                    Page.DisplayMessage(new ResultStatus(ex.Message, false));
                    return;
                }
                
                var modInstanceList =
                    from i in instances
                    select new CPModelingInstanceDtl
                    {
                        AddedToPackageGMT = i.AddedToPackageGMT,
                        ListItemAction = ListItemAction.Add,
                        ObjectInstanceId = i.InstanceID,
                        ObjectType = int.Parse(i.CDOTypeID),
                        ObjectTypeName = new Enumeration<MaintainableObjectEnum, string>(i.CDOTypeName),
                        ObjectName = i.Name,
                        Revision = i.Revision,
                        IsReference = i.IsRef,
                        Sequence = i.Order,
                        DisplayedName = i.DisplayedName
                    };

                srv.ChangePackage = SelectedPackage.Data as NamedObjectRef;
                srv.ServiceDetail = new AssignChangePkgContentDtl
                {
                    FieldAction = Camstar.WCF.ObjectStack.Action.Create,
                    Instances = modInstanceList.ToArray()
                };
            }

            if (_isComplete && srv != null)
            {
                if (srv.ChangePackage == null)
                    srv.ChangePackage = SelectedPackage.Data as NamedObjectRef;
                var comment = Page.PortalContext.DataContract.GetValueByName<string>("CompleteComent");
                Page.PortalContext.DataContract.SetValueByName("CompleteComent", null);
                var collaboratorTask = Page.DataContract.GetValueByName("CollaboratorTask") as CollaboratorEntryDetails;
                var messageToCollaborator = Page.SessionVariables["MessageToCollaborator"] as NamedObjectRef;
                var messageToOwner = Page.SessionVariables["MessageToOwner"] as NamedObjectRef;
                if (collaboratorTask != null)
                {
                    srv.CollaboratorEntry = new CollaboratorEntryDetails()
                    {
                        SheetLevel = collaboratorTask.SheetLevel,
                        Role = collaboratorTask.Role,
                        Collaborator = collaboratorTask.Collaborator,
                        FieldAction = Camstar.WCF.ObjectStack.Action.Create,
                        CollaboratorComments = comment,
                        Status = new Enumeration<CollaborateStatusEnum, int>(3),
                        CollaboratorEntry = collaboratorTask.CollaboratorEntry
                    };
                }
                if (srv.CollaboratorDetails != null)
                {
                    srv.CollaboratorDetails.EMailMessageToCollaborator = messageToCollaborator;
                    srv.CollaboratorDetails.EMailMessageToOwner = messageToOwner;
                }
            }
        }
        #endregion

        #region Public Functions

        public virtual void AddInstances()
        {
            var data = ObjectTypeGrid.Data != null ? new List<ObjectTypeItem>(ObjectTypeGrid.Data as ObjectTypeItem[]) : new List<ObjectTypeItem>();

            var instanceGrid = Page.FindCamstarControl("InstanceListGrid") as JQDataGrid;
            var selectedItems = instanceGrid != null ? instanceGrid.GridContext.GetSelectedItems(false) : null;
            var instanceNameCtl = Page.FindCamstarControl("ObjectsList") as CWC.DropDownList;

            if (selectedItems != null && instanceNameCtl != null)
            {
                string cdoName;
                int cdoId;
                string cdoDisplayName;
                var cdoIdStr = instanceNameCtl.Data as string;

                GetCurrentCDOInfo(cdoIdStr, out cdoId, out cdoName, out cdoDisplayName);

                var objTypeItem = data.FirstOrDefault(s => s.DisplayName == cdoDisplayName);
                if (objTypeItem == null)
                {
                    objTypeItem = new ObjectTypeItem()
                    {
                        Name = cdoName,
                        CDOID = cdoId,
                        DisplayName = cdoDisplayName,
                        Instances = new SelectedInstanceItem[] { }
                    };
                    data.Add(objTypeItem);
                    objTypeItem = data.First(s => s.DisplayName == cdoDisplayName);
                }

                var instances =
                    (from si in selectedItems
                     select new SelectedInstanceItem(si as DataRow) { CDOTypeName = cdoName, CDOTypeID = cdoIdStr }).ToArray();

                foreach (var item in instances)
                    if (!objTypeItem.Instances.Any(ins => ins.InstanceID == item.InstanceID) || objTypeItem.Instances.FirstOrDefault(ins => ins.InstanceID == item.InstanceID).IsSave) item.IsSave = true;

                // Clear selecting in the instance list
                instanceGrid.GridContext.GetSelectedItems(true);

                var comparer = new InstanceComparer<SelectedInstanceItem>((i1, i2) => i1.InstanceID == i2.InstanceID, i => i.InstanceID.GetHashCode());
                objTypeItem.Instances = instances.Union(objTypeItem.Instances, comparer).OrderBy(it => it.Name).ToArray();

                ObjectTypeGrid.Data = data.OrderBy(n => n.DisplayName).ToArray();
                Page.DataContract.SetValueByName("SelectedInstances", ObjectTypeGrid.Data);
                (instanceGrid.NamingContainer as WebPartBase).RenderToClient = true;
            }

            CheckSelectionLimit();
        }

        protected virtual void AddDependencies()
        {
            if (Page.DataContract.GetValueByName<ObjectTypeItem[]>("SelectedInstances") == null)
                return;
            if (ObjectTypeGrid.Data != null)
            {
                var allSelected =
                    from o in ObjectTypeGrid.Data as ObjectTypeItem[]
                    where o.Instances != null
                    from it in o.Instances
                    select it;

                var index = 1;
                _excludedObjects = GetExcludedObjects().ToList();
                if (Page.DataContract.GetValueByName<ObjectTypeItem[]>("SelectedInstances") != null)
                {
                    var itemGroups = new List<ObjectTypeItem>(Page.DataContract.GetValueByName<ObjectTypeItem[]>("SelectedInstances"));
                    try
                    {
                        var refs = GetReferences(allSelected.ToArray(), false);

                        if (_circularRefInstances != null)
                        {
                            StringBuilder sb = new StringBuilder();
                            if (_circularRefInstances.Count > 0)
                            {
                                var grpedInstances = _circularRefInstances
                                                        .GroupBy(x => x.Value)
                                                        .ToDictionary(y => y.Key, y => y.Select(x => x.Key).ToArray());

                                foreach (var pair in grpedInstances)
                                {
                                    var i = 0;
                                    foreach (var instance in pair.Value)
                                    {
                                        if (i > 0)
                                        {
                                            sb.Append(",");
                                        }
                                        else
                                        {
                                            sb.Append(pair.Key + ":");
                                        }
                                        sb.Append(instance);
                                        i++;
                                    }
                                    sb.Append(";");
                                }
                                _circRefError += sb.ToString();
                                Page.DisplayWarning(_circRefError);
                            }
                        }

                        if (_excludedObjects != null) //remove excluded refs
                        {
                            foreach (var _excludedObject in _excludedObjects)
                            {
                                refs = refs.Where(key => key.CDOTypeValue != _excludedObject);
                            }
                        }

                        if (refs != null)
                        {
                            var orderedRefs = refs.OrderByDescending(r => r.Order);
                            foreach (var rf in orderedRefs)
                            {
                                rf.Order = index++;
                                rf.IsSave = true;

                                var itemGroup = itemGroups.FirstOrDefault(g => g.Name == rf.CDOTypeName);
                                if (itemGroup == null)
                                {
                                    itemGroup = new ObjectTypeItem()
                                    {
                                        Name = rf.CDOTypeName,
                                        DisplayName = rf.CDOTypeValue,
                                        CDOID = int.Parse(rf.CDOTypeID),
                                        Instances = new SelectedInstanceItem[] { rf }
                                    };
                                    itemGroups.Add(itemGroup);
                                }
                                else
                                {
                                    var instances = itemGroup.Instances.ToList();
                                    var instance = instances.FirstOrDefault(i => i.Name == rf.Name && i.Revision == rf.Revision);
                                    if (instance != null)
                                        instance.Order = Math.Max(instance.Order, rf.Order);
                                    else
                                        instances.Add(rf);

                                    itemGroup.Instances = instances.ToArray();
                                }
                            }
                        }

                        ObjectTypeGrid.Data = itemGroups.OrderBy(c => c.DisplayName).ToArray();
                        Page.PortalContext.DataContract.SetValueByName("SelectedInstances", ObjectTypeGrid.Data);
                    }
                    catch (Exception ex)
                    {
                        DisplayMessage(new ResultStatus(ex.Message, false));
                    }
                }                    

            }
            CheckSelectionLimit();
        }

        protected virtual void DeleteDependencies()
        {
            var data = ObjectTypeGrid.Data as ObjectTypeItem[];
            if (data != null)
            {
                var newdata =
                    from d in data
                    let inst = d.Instances.Where(n => !n.IsRef)
                    where inst.Any()
                    select new ObjectTypeItem()
                    {
                        Name = d.Name,
                        DisplayName = d.DisplayName,
                        CDOID = d.CDOID,
                        Instances = inst.ToArray()
                    };

                ObjectTypeGrid.Data = newdata.ToArray();
                Page.PortalContext.DataContract.SetValueByName("SelectedInstances", ObjectTypeGrid.Data);
                CheckSelectionLimit();
            }
        }

        public virtual void ShowHideDependencies()
        {
            if (HideDepControl.IsChecked)
            {
                var data = ObjectTypeGrid.Data as ObjectTypeItem[];
                if (data != null)
                {
                    var newdata =
                        from d in data
                        let inst = d.Instances.Where(n => !n.IsRef)
                        where inst.Any()
                        select new ObjectTypeItem()
                        {
                            Name = d.Name,
                            DisplayName = d.DisplayName,
                            CDOID = d.CDOID,
                            Instances = inst.ToArray()
                        };
                    if ((ObjectTypeGrid.Data as ObjectTypeItem[]).Count() != newdata.ToArray().Count() || Page.PortalContext.LocalSession == null)
                    {
                        Page.PortalContext.LocalSession["DataWithDependencies"] = ObjectTypeGrid.Data;
                    }                    
                    ObjectTypeGrid.Data = newdata.ToArray();
                }
            }
            else
            {
                ObjectTypeGrid.Data = Page.PortalContext.LocalSession["DataWithDependencies"];
            }
        }

        protected virtual IEnumerable<SelectedInstanceItem> GetReferences(SelectedInstanceItem[] selectedItems, bool recursion)
        {
            if (selectedItems != null && selectedItems.Length > 0)
            {
                _refs = new List<SelectedInstanceItem>(selectedItems);

                var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
                var cdoServ = new CDOInquiryService(session.CurrentUserProfile);
                var items = selectedItems;

                while (items.Length > 0)
                {
                    //loop thru items and get references
                    foreach (var item in items)
                    {
                        var reference = _refs.FirstOrDefault(x => x.Name == item.Name);
                        var order = reference != null ? reference.Order + 1 : 1;
                        var cdo = new CDOInquiry()
                        {
                            SelectedInstances = new BaseObjectRef[] { new BaseObjectRef(item.InstanceID) },
                            Recursive = recursion
                        };
                        var req = new CDOInquiry_Request
                        {
                            Info = new CDOInquiry_Info
                            {
                                ObjectReferencesList = new ObjectReferencesInfo_Info { RequestValue = true }
                            }
                        };

                        CDOInquiry_Result res;
                        var status = cdoServ.GetReferences(cdo, req, out res);
                        if (status.IsSuccess && res.Value != null && res.Value.ObjectReferencesList != null)
                        {
                            Array.ForEach(res.Value.ObjectReferencesList, obj =>
                            {
                                if (obj.ObjectFields != null)
                                    Array.ForEach(obj.ObjectFields,
                                        field => AddFieldItemToReferences(field, order, item.InstanceID));
                            });
                        }
                        item.ShouldOpen = false;
                    }
                    //set the items list to items that still need to be explored
                    items = _refs.Where(r => r.ShouldOpen).ToArray();
                }
                return _refs;
            }

            return null;
        }

        public virtual void ItemsSelectedGrid_SnapCompleted(DataTable table)
        {
            foreach (DataRow row in table.Rows)
            {
                row["IsModelingObjectExist"] = 0;
                if (new MaintCDOCache().MaintCdoData.Any(x => x.CDODefID == row["CDOTypeID"] as string))
                    row["IsModelingObjectExist"] = 1;

            }
        }

        #endregion

        #region Private Functions

        protected virtual void AddFieldItemToReferences(ObjectField r, int order, string parent)
        {
            if (r is SubentityField)
            {
                var subentity = r as SubentityField;
                if (subentity.Instances != null)
                    Array.ForEach(subentity.Instances, item =>
                    {
                        AddSubentityInstance(item, order, parent);
                    });
            }
            else if (r is ReferenceField)
            {
                var reference = r as ReferenceField;
                if (reference.References != null)
                    Array.ForEach(reference.References, rf =>
                    {
                        if (rf.ObjectFields != null)
                            Array.ForEach(rf.ObjectFields, item => AddFieldItemToReferences(item, order, (string)item.FieldName));
                        //  Do not add a Reference to any object linked to HistoryMainline
                        //  PR#9949691: Change Management: add references does not work if a Resource is included
                        if (string.Compare(rf.ObjectTypeName.Value, "HistoryMainline", true) != 0)
                            AddToReferences(new SelectedInstanceItem(rf) { ShouldOpen = true, Order = order, ReferenceParent = parent });
                    });
            }
        }
        protected virtual void AddToReferences(SelectedInstanceItem it, int depthCount = 0)
        {
            if (_excludedObjects == null || !_excludedObjects.Contains(it.CDOTypeName))
            {
                var existingItem = _refs.FirstOrDefault(x => x.InstanceID == it.InstanceID);
                if (existingItem != null)
                {
                    existingItem.ReferenceParent = it.ReferenceParent;
                    RecursivelyIncrementOrder(existingItem, it.Order++, depthCount);
                }
                else
                    _refs.Insert(0, it);
            }
        }
        protected virtual void RecursivelyIncrementOrder(SelectedInstanceItem item, int order, int depthCount)
        {
            depthCount++;
            var circRefInst = _refs.FirstOrDefault(x => x.InstanceID == item.ReferenceParent);
            if (depthCount > _maxDepthCount)
            {
                if (_circularRefInstances == null)
                {
                    _circularRefInstances = new Dictionary<string, string>();
                }
                if (circRefInst != null)
                {
                    if (!_circularRefInstances.ContainsKey(circRefInst.Name))
                    {
                        _circularRefInstances.Add(item.Name, item.CDOTypeName);
                    }
                }
            }
            item.Order = order;
            var childItems = _refs.Where(x => x.ReferenceParent == item.InstanceID);
            foreach (var child in childItems)
            {
                if (_circularRefInstances != null)
                {
                    if (circRefInst != null)
                    {
                        if (_circularRefInstances.ContainsKey(circRefInst.Name))
                        {
                            break;
                        }
                    }
                }
                RecursivelyIncrementOrder(child, order + 1, depthCount);
            }
        }

        protected virtual void AddReferenceInstance(SelectedInstanceItem it)
        {
            var existingItem = _refs.FirstOrDefault(x => x.InstanceID == it.InstanceID);
            if (existingItem != null)
            {
                existingItem.Order += it.Order;
            }
            else
            {
                _refs.Insert(0, it);
            }
        }

        protected virtual void AddSubentityInstance(SubentityInstance sr, int order, string parent)
        {
            if (sr.ObjectFields != null)
                Array.ForEach(sr.ObjectFields, item =>
                {                   
                    AddFieldItemToReferences(item, order, parent);
                });
        }

        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as CustomAction;
            if (action != null)
            {
                switch (action.Parameters)
                {
                    case "Clear":
                        {
                            Page.ClearValues();
                            Page.PortalContext.LocalSession["DataWithDependencies"] = null;
                            InstanceList.ResetFilter();
                            CheckReference();

                            break;
                        }
                }
            }
        }

        protected virtual void CheckReference()
        {
            bool isAdded = true;
            foreach (var item in ObjectTypeGrid.Data as ObjectTypeItem[])
                if (item.Instances.Any(it => it.IsRef == true))
                {
                    isAdded = false;
                    break;
                }

            ReloadAddorRemoveButton(isAdded);
            CamstarWebControl.SetRenderToClient(AddOrRemoveDependencies);
        }

        protected virtual void CheckSelectionLimit()
        {
            var instances = Page.DataContract.GetValueByName("SelectedInstances") as ObjectTypeItem[];
            if (instances == null)
                return;
            var selectedInstanceCount = ObjectTypeItem.GetInstancesCount(instances, false);
            if (SubmitAction == null)
                return;
            var submitBtn = SubmitAction.Control as CWC.Button;
            if (submitBtn == null)
                return;
            if (selectedInstanceCount <= maxAllowedInstanceCount)
            {
                if (ViewState[SubmitBtnOnClick] != null)
                {
                    submitBtn.Attributes["onclick"] = (string)ViewState[SubmitBtnOnClick];
                    CamstarWebControl.SetRenderToClient(submitBtn);
                }
                return;
            }
            var labelCache = FrameworkManagerUtil.GetLabelCache(HttpContext.Current.Session);
            if (ViewState[SubmitBtnOnClick] == null)
                ViewState[SubmitBtnOnClick] = submitBtn.Attributes["onclick"];
            submitBtn.Confirmation = new Confirmation { OK_LabelText = "OK", Cancel_LabelText = "Cancel", Message_LabelText = notification, Title_LabelText = "Warning" };
            CamstarWebControl.SetRenderToClient(submitBtn);
        }

        protected virtual void GetCurrentCDOInfo(string cdoIdStr, out int CDOId, out string CDOName, out string CDODisplayName)
        {
            CDOId = int.Parse(cdoIdStr);
            var exportCdo = Page.PortalContext.LocalSession["ObjectsList"] as Dictionary<string, CDOData>;
            if (exportCdo != null)
            {
                var d = exportCdo.FirstOrDefault(s => s.Value.CDODefID == cdoIdStr);
                CDOName = d.Value.CDOName;
                CDODisplayName = d.Value.CDODisplayName;
                return;
            }
            CDOName = null;
            CDODisplayName = null;
        }

        protected virtual void ReloadAddorRemoveButton(bool isDependenciesAdded)
        {
            var labelCache = FrameworkManagerUtil.GetLabelCache(HttpContext.Current.Session);

            var buttonLabel = (isDependenciesAdded) ? "Lbl_AddDependenciesBtn" : "Lbl_RemoveDependenciesBtn";
            AddOrRemoveDependencies.LabelText = labelCache.GetLabelByName(buttonLabel).Value;
            ViewState[DependenciesAdded] = !isDependenciesAdded;
            HideDepControl.Hidden = isDependenciesAdded;
            if (ContentHierarchy != null)
            {
                ContentHierarchy.Margin.Left = isDependenciesAdded ? 235 : 135;                     
                CamstarWebControl.SetRenderToClient(ContentHierarchy);
            }
        }

        protected virtual IEnumerable<String> GetExcludedObjects()
        {
            var excludedObjects = new string[0];
            var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = Page.Service.GetService<FactoryMaintService>();

            if (session.SessionValues != null && !String.IsNullOrEmpty(session.SessionValues.Factory))
            {
                var serviceData = new FactoryMaint();
                serviceData.ObjectToChange = new NamedObjectRef(session.SessionValues.Factory);
                var request = new FactoryMaint_Request();
                var result = new FactoryMaint_Result();
                var resultStatus = new ResultStatus();

                request.Info = new FactoryMaint_Info
                {
                    RequestValue = true,
                    ObjectChanges = new FactoryChanges_Info
                    {
                        Name = new Info(true),
                        ModelingObjsToExclude = new ModelingObjsToExcludeChanges_Info
                        {
                            ModelingCDOTypeId = new Info(true),
                            DisplayName = new Info(true)
                        }
                    }
                };

                resultStatus = service.Load(serviceData, request, out result);
                if (resultStatus.IsSuccess && result.Value.ObjectChanges.ModelingObjsToExclude != null)
                {
                    excludedObjects = result.Value.ObjectChanges.ModelingObjsToExclude
                        .Where(objExclude => objExclude.ModelingCDOTypeId != null)
                        .Select(objExclude => objExclude.DisplayName.ToString())
                        .ToArray();
                }

            }
            return excludedObjects;
        }
        #endregion

        #region Constants
        private const int _maxDepthCount = 5;
        #endregion

        #region Private Member Variables
        private List<SelectedInstanceItem> _refs;
        private const string SubmitBtnOnClick = "SubmitBtnOnClick";
        private const string CheckSelectedInstancesCount = "CheckSelectedInstancesCount";
        private const int maxAllowedInstanceCount = 1000;
        private readonly string notification = string.Format(@"The package has been assigned over {0} instances. Please be aware that system performance may be impacted.", maxAllowedInstanceCount);
        private const string DependenciesAdded = "AssignReviewContent.DependenciesAdded";
        private bool _isComplete = false;
        private List<String> _excludedObjects;
        private Dictionary<string,string> _circularRefInstances;
        private string _circRefError;
        #endregion
    }
}
