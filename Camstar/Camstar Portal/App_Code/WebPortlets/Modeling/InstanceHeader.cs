// Copyright Siemens 2023  
using System;
using System.Web;
using System.Web.UI;
using Camstar.WCF.Services;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.FormsFramework;
using OM = Camstar.WCF.ObjectStack;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using CWGC = Camstar.WebPortal.FormsFramework.WebGridControls;
using PERS = Camstar.WebPortal.Personalization;
using System.Collections.Generic;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using System.Data;
using System.Runtime.Remoting;
using System.Reflection;
using Camstar.WebPortal.WCFUtilities;
using System.Linq;
using System.Text;

namespace Camstar.WebPortal.WebPortlets.Modeling
{
    public class InstanceHeader : MatrixWebPart
    {

        #region Controls
        protected virtual CWC.DropDownList Status { get { return Page.FindCamstarControl("StatusRev") as CWC.DropDownList; } }
        protected virtual CWC.ImageControl LockImg { get { return Page.FindCamstarControl("LockImage") as CWC.ImageControl; } }
        protected virtual JQTabContainer DescriptionTabContainer { get { return Page.FindCamstarControl("DescriptionTabContainer") as JQTabContainer; } }
        protected virtual CWC.Label Lbl_CMInstanceStatus { get { return Page.FindCamstarControl("Lbl_CMInstanceStatus") as CWC.Label; } }
        protected virtual CWC.Label Lbl_AssociatedPackages { get { return Page.FindCamstarControl("Lbl_AssociatedPackages") as CWC.Label; } }
        protected virtual CWC.Label Lbl_InstanceLockedByCM { get { return Page.FindCamstarControl("Lbl_InstanceLockedByCM") as CWC.Label; } }
        protected virtual CWC.Label Lbl_Yes { get { return Page.FindCamstarControl("Lbl_Yes") as CWC.Label; } }
        protected virtual CWC.Label Lbl_No { get { return Page.FindCamstarControl("Lbl_No") as CWC.Label; } }
        protected virtual CWGC.MultiSelectPickList ObjectChanges_FilterTags { get { return Page.FindCamstarControl("ObjectChanges_FilterTags") as CWGC.MultiSelectPickList; } }
        protected virtual CWC.TextBox FilterTagsHidden { get { return Page.FindCamstarControl("FilterTagsHidden") as CWC.TextBox; } }
        protected virtual CWGC.JQDataGrid InstanceGrid { get { return Page.FindCamstarControl("InstanceGrid") as CWGC.JQDataGrid; } }
        #endregion

        #region Protected Methods
        protected override void OnPreLoad(object sender, EventArgs e)
        {
            base.OnPreLoad(sender, e);
            if (!PortalContext.IsRDO)
            {
                Status.FieldExpressions = null;
            }
            if (!PortalContext.IsFrozen)
            {
                LockImg.Visible = false;
            }
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!PortalContext.IsFrozen && Page.IsFloatingFrame)
            {
                Status.Visible = false;
            }
            // if CDO caption length is more than 31, then the Lbl_ItemNameLabel value will be wrapped and the tooltip will show up
            if (PortalContext.CDODisplayName.Length > 31)
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "labelWrapper", "labelWrapper();", true);
            }

            if (HasChangePackages())
            {
                DescriptionTabContainer.SelectedIndexChanged += DescriptionTabContainer_SelectedIndexChanged;
                DescriptionTabContainer.LoadAllTabs = false;                
                SetupChangeMgtTab(true);
            }
            else
                DescriptionTabContainer.LoadAllTabs = true;

            HttpContext.Current.Session["RenderFilterTags"] = null;
            if (!Page.IsPostBack)
            {
                var WarningTextLbl = Page.FindCamstarControl("WarningTextLbl") as CWC.Label;
                if (WarningTextLbl != null && PortalContext.MaintService == "FilterTagMaint")
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(FrameworkManagerUtil.GetLabelValue("WarningDeleteFilterTagLbl") ?? string.Empty);
                    sb.Append(FrameworkManagerUtil.GetLabelValue("WarningDeleteFilterTagLbl2") ?? string.Empty);
                    sb.Append(FrameworkManagerUtil.GetLabelValue("WarningDeleteFilterTagLbl3") ?? string.Empty);
                    WarningTextLbl.LabelText = sb.ToString() ?? string.Empty;
                }

                if (WarningTextLbl != null)
                {
                    ObjectChanges_FilterTags.ReadOnly = true;
                }              
            }
            if (ObjectChanges_FilterTags != null)
            {
                ObjectChanges_FilterTags.PreRender += ObjectChanges_FilterTags_PreRender;
            }
            if (InstanceGrid != null)
            {
                InstanceGrid.RowSelected += InstanceGrid_RowSelected;
            }            

            ScriptManager.RegisterStartupScript(this, this.GetType(), "resetIsDirty", "resetIsDirty();", true);
        }

        public override void ClearValues()
        {
            string tags = string.Empty;
            string existingTags = FilterTagsHidden.Data as string ?? string.Empty;
            if (HttpContext.Current.Session["RenderFilterTags"] != null)
                tags = FilterTagsHidden.Data as string ?? string.Empty;
            base.ClearValues();
            // reset is set to false only on initial load or reset action executed
            if (!ResetExecuted)
            {
                ResetExecuted = true;
            }
            if (HttpContext.Current.Session["RenderFilterTags"] != null && !string.IsNullOrEmpty(tags))
            {
                FilterTagsHidden.Data = tags;
                RenderFilterTags();
                HttpContext.Current.Session["RenderFilterTags"] = null;
            }
        }

        protected virtual void ObjectChanges_FilterTags_PreRender(object sender, EventArgs e)
        {
            RenderFilterTags();
        }

        public void RenderFilterTags()
        {
            if (FilterTagsHidden != null)
            {
                var grid = ObjectChanges_FilterTags.PickListPanelControl.ViewControl as JQDataGrid;
                if (grid != null)
                {
                    if (grid.GridContext.SelectedRowIDs != null
                        && SelectedFilterTags != null
                        && !ResetExecuted) // add this condition to ensure grid resets only when needed
                    {
                        SelectedFilterTags = grid.GridContext.SelectedRowIDs.ToArray();
                    }
                    else
                    {
                        var assignedTags = FilterTagsHidden.Data as string ?? string.Empty;
                        if (!string.IsNullOrEmpty(assignedTags))
                        {
                            GetAllFilterTags();
                            var assignedTagsAry = assignedTags.Split(',');
                            foreach (var assignedTag in assignedTagsAry)
                            {
                                grid.GridContext.SelectRow(assignedTag, true);
                            }
                            SelectedFilterTags = grid.GridContext.SelectedRowIDs.ToArray();
                        }
                        // set ResetExecuted back to false
                        ResetExecuted = false;
                        HttpContext.Current.Session["RenderFilterTags"] = true;
                    }
                }
            }
        }

        protected virtual ResponseData InstanceGrid_RowSelected(object sender, JQGridEventArgs args)
        {
            SelectedFilterTags = null;
            return null;
        }
        
        protected virtual void GetAllFilterTags()
        {
            var svc = new FilterTagInquiryService(FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);
            var request = new FilterTagInquiry_Request
            {
                Info = new WCF.ObjectStack.FilterTagInquiry_Info
                {
                    FilterTag = new WCF.ObjectStack.FilterTag_Info
                    {
                        RequestSelectionValues = true
                    }
                }
            };
            FilterTagInquiry_Result res;
            var resultStatus = svc.GetEnvironment(new WCF.ObjectStack.FilterTagInquiry(), request, out res);
            if (resultStatus.IsSuccess)
            {
                if (res != null && res.Environment != null && res.Environment.FilterTag != null)
                {
                    ObjectChanges_FilterTags.SetSelectionValues(res.Environment.FilterTag.SelectionValues);
                }
            }
        }

        #endregion

        #region Public Methods
        public override void GetInputData(OM.Service serviceData)
        {
            base.GetInputData(serviceData);
            // Update assigned filter tags on all instances but the employee maint service
            var emplMaint = serviceData as OM.EmployeeMaint;
            if (emplMaint == null)
            {
                if (ObjectChanges_FilterTags != null)
                {
                    var selectedRowIds = ObjectChanges_FilterTags.SelectedRowIDs as string[];
                    string filterTags = string.Empty;
                    if (selectedRowIds != null)
                    {
                        filterTags = string.Join(",", selectedRowIds);
                        var data = new WCFObject(serviceData);
                        if (data != null)
                        {
                            data.SetValue("ObjectChanges.FilterTags", filterTags);
                        }
                    }
                    FilterTagsHidden.Data = filterTags;
                }
            }
        }

        public override void PostExecute(OM.ResultStatus status, OM.Service serviceData)
        {
            base.PostExecute(status, serviceData);
            if (HasChangePackages())
            {
                if (status != null && status.IsSuccess)
                    SetupChangeMgtTab(true);
            }

            if (status.IsSuccess)
            {
                object service;
                object objectChanges;
                var objectChangesName = string.Empty;
                                
                if (!PortalContext.IsRDO)
                {
                    service = (OM.NamedDataObjectMaint)serviceData;
                    objectChanges = ((OM.NamedDataObjectMaint)service).ObjectChanges;
                    objectChangesName = objectChanges != null ? ((OM.NamedDataObjectChanges)objectChanges).Name != null ? ((OM.NamedDataObjectChanges)objectChanges).Name.Value : string.Empty : string.Empty;
                }
                else
                {
                    service = (OM.RevisionedObjectMaint)serviceData;
                    objectChanges = ((OM.RevisionedObjectMaint)service).ObjectChanges;
                    objectChangesName = objectChanges != null ? ((OM.RevisionedObjectChanges)objectChanges).Name != null ? ((OM.RevisionedObjectChanges)objectChanges).Name.Value : string.Empty : string.Empty;
                }
                if (objectChanges != null)
                {
                    var type = objectChanges.GetType();
                    var typeName = type.Name ?? string.Empty;
                    if (typeName == "FilterTagChanges" && objectChangesName != null)
                    {
                        var filterTags = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile.FilterTags;
                        var newInstanceId = GetFilterNewFilterTagInstanceID(objectChangesName);
                        if (string.IsNullOrEmpty(filterTags) && !string.IsNullOrEmpty(newInstanceId))
                        {
                            FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile.FilterTags = newInstanceId;
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(newInstanceId) && !filterTags.Contains(newInstanceId))
                            {
                                filterTags += "," + newInstanceId;
                                FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile.FilterTags = filterTags;
                            }
                        }
                    }
                }

                var WarningTextLbl = Page.FindCamstarControl("WarningTextLbl") as CWC.Label;
                if (service.GetType().Name == "FilterTagMaint")
                {
                    Page.DataContract.SetValueByName("FilterTagsDM", null);
                }

                SelectedFilterTags = null;
                GetAllFilterTags();               
            }
        }
        #endregion

        #region Private Methods
        protected virtual void DescriptionTabContainer_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedTab = DescriptionTabContainer.SelectedItem;
            if (selectedTab.Name == "ChangeMgtTab")
                SetupChangeMgtTab(false);
        }

        /// <summary>
        /// Displays Change Mgt tab info.
        /// </summary>
        /// <param name="force">Forses Change Mgt tab info reloading.</param>
        protected virtual void SetupChangeMgtTab(bool force)
        {
            bool loadChangePkgTabInfo = false;
            if (PortalContext.State == MaintenanceBehaviorContext.MaintenanceState.Edit)
            {
                var currentInstanceId = PortalContext.Current.ID;
                if (!force)
                {
                    if (InstanceLockedChgPkg == null || AssociatedPackagesChgPkg == null || SelectedInstanceChgPkg != currentInstanceId)
                        loadChangePkgTabInfo = true;
                }
                else
                    loadChangePkgTabInfo = true;

                if (loadChangePkgTabInfo)
                {
                    CleanUpChangeMgtTabInfo();
                    if (PortalContext.Current != null && !string.IsNullOrEmpty(currentInstanceId))
                        LoadChangeMgtTabInfo(PortalContext.Current.ID);
                }
            }
            bool isLocked = InstanceLockedChgPkg != null && InstanceLockedChgPkg.Value;
            string associatedPkgs = AssociatedPackagesChgPkg ?? "0";

            const string cmInfoHtmlTemplate = "<div class='leftDiv'><div><b>{0}</b></div><div>{1} <b>{5}</b></div><div>{2} <b><font color='{3}'>{4}</font></b></div></div><div class='rightDiv'>&nbsp;</div>";
            const string greenColor = "#008000";
            const string redColor = "#800000";

            string isLockedStr = Lbl_No.Text;
            string isLockedColor = greenColor;

            if (isLocked)
            {
                isLockedStr = Lbl_Yes.Text;
                isLockedColor = redColor;
            }

            var cmInfo = Page.FindCamstarControl("CMInfoField") as CWC.Label;
            if (cmInfo != null)
            {
                cmInfo.Text = string.Format(cmInfoHtmlTemplate, Lbl_CMInstanceStatus.Text, Lbl_AssociatedPackages.Text, Lbl_InstanceLockedByCM.Text, isLockedColor, isLockedStr, associatedPkgs);
                CamstarWebControl.SetRenderToClient(cmInfo);
            }
        }

        /// <summary>
        /// Loads info for Change Mgt tab.
        /// </summary>
        /// <param name="modelingInstanceId">Selected modeling instance ID.</param>
        protected virtual void LoadChangeMgtTabInfo(string modelingInstanceId)
        {
            var sesn = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            if (sesn != null)
            {
                var service = new ChangePackageModelingInquiryService(sesn.CurrentUserProfile);
                var serviceData = new OM.ChangePackageModelingInquiry { ObjectInstanceId = modelingInstanceId };

                var request = new ChangePackageModelingInquiry_Request
                {
                    Info = new OM.ChangePackageModelingInquiry_Info
                    {
                        IsLockInstance = new OM.Info(true),
                        AssociatedPackagesCount = new OM.Info(true)
                    }
                };
                ChangePackageModelingInquiry_Result result;
                var resultStatus = service.ExecuteTransaction(serviceData, request, out result);

                if (resultStatus.IsSuccess)
                {
                    if (result.Value != null)
                    {
                        InstanceLockedChgPkg = (bool)result.Value.IsLockInstance;
                        AssociatedPackagesChgPkg = result.Value.AssociatedPackagesCount.ToString();
                        SelectedInstanceChgPkg = modelingInstanceId;
                    }
                }
                else
                    DisplayMessage(resultStatus);
            }
        }        

        /// <summary>
        /// Gets the newly created Filter Tag to set for assigning to session filter tags
        /// </summary>
        /// <param name="filterTagName"></param>
        /// <returns></returns>
        protected virtual string GetFilterNewFilterTagInstanceID(string filterTagName)
        {
            string instanceId = string.Empty;
            var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = new FilterTagInquiryService(session.CurrentUserProfile);
            var serviceData = new OM.FilterTagInquiry();

            var request = new FilterTagInquiry_Request
            {
                Info = new OM.FilterTagInquiry_Info()
                {
                    FilterTag = new OM.FilterTag_Info
                    {
                        InstanceID = new OM.Info(true),
                        Name = new OM.Info(true)
                    }
                }
            };

            FilterTagInquiry_Result result;

            var resultStatus = service.GetFilterTags(serviceData, request, out result);
            if (resultStatus.IsSuccess)
            {
                if (result != null && result.Value != null && result.Value.FilterTag != null)
                {
                    foreach (var filterTag in result.Value.FilterTag)
                    {
                        if (filterTag.InstanceID.Name == filterTagName)
                        {
                            instanceId = filterTag.InstanceID.ID;
                            break;
                        }
                    }
                }
            }
            return instanceId;
        }

        /// <summary>
        /// Cleans up Change Mgt tab info.
        /// </summary>
        protected virtual void CleanUpChangeMgtTabInfo()
        {
            SelectedInstanceChgPkg = null;
            AssociatedPackagesChgPkg = null;
            InstanceLockedChgPkg = null;
        }
        #endregion

        #region Private Members
        protected virtual MaintenanceBehaviorContext PortalContext
        {
            get { return Page.PortalContext as MaintenanceBehaviorContext; }
        }

        /// <summary>
        /// Checks whether signed in user has packages or not.
        /// </summary>
        /// <returns></returns>
        protected virtual bool HasChangePackages()
        {
            var res = Page.SessionDataContract.GetValueByName(DataMemberConstants.HasCMPackage);
            return res != null && Boolean.Parse(res.ToString());
        }

        protected virtual bool? InstanceLockedChgPkg
        {
            get
            {
                if (ViewState["InstanceLocked_chgPkg"] != null)
                    return Convert.ToBoolean(ViewState["InstanceLocked_chgPkg"]);
                return null;
            }
            set { ViewState["InstanceLocked_chgPkg"] = value; }
        }

        protected virtual string AssociatedPackagesChgPkg
        {
            get
            {
                if (ViewState["AssociatedPackages_chgPkg"] != null)
                    return ViewState["AssociatedPackages_chgPkg"].ToString();
                return null;
            }
            set { ViewState["AssociatedPackages_chgPkg"] = value; }
        }

        /// <summary>
        /// Change management tab info is loaded for this modeling instance ID now.
        /// </summary>
        protected virtual string SelectedInstanceChgPkg
        {
            get
            {
                if (ViewState["SelectedInstanceChgPkg"] != null)
                    return ViewState["SelectedInstanceChgPkg"].ToString();
                return null;
            }
            set { ViewState["SelectedInstanceChgPkg"] = value; }
        }
        
        /// <summary>
        /// Data contract member for selected filter tags (Instance ID)
        /// </summary>
        protected virtual string[] SelectedFilterTags
        {
            get
            {
                return Page.DataContract.GetValueByName<string[]>("SelectedFilterTagsDM");
            }
            set
            {
                Page.DataContract.SetValueByName("SelectedFilterTagsDM", value);

            }
        }      
        /// <summary>
        /// Data Contract that checks if FilterTags control needs to be cleared
        /// </summary>
        protected virtual bool ResetExecuted
        {
            get
            {
                return Page.DataContract.GetValueByName<bool>("ResetExecuted");
            }
            set
            {
                Page.DataContract.SetValueByName("ResetExecuted", value);
            }
        }

        protected virtual bool FilterTagsRendered
        {
            get
            {
                return Page.DataContract.GetValueByName<bool>("FilterTagsRendered");
            }
            set
            {
                Page.DataContract.SetValueByName("FilterTagsRendered", value);
            }
        }
        #endregion
    }
}
