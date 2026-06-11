// Copyright Siemens 2023  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Data;
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework.Utilities;
using CWGC = Camstar.WebPortal.FormsFramework.WebGridControls;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.WebPortlets.DataTransfer;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.PortalFramework;

namespace Camstar.WebPortal.WebPortlets.ChangeManagement
{
    public class ChangePackageSearch : MatrixWebPart
    {
        public ChangePackageSearch() { }

        #region Constants

        protected const string instancesForSelectedObject = "InstancesForSelectedObject";
        protected const string changePackageSearchDetail = "ChangePackageSearchDetail";
        private const string DeploymentDetails = "DeploymentDetails";
        bool _isClearAll = false;

        #endregion

        #region Properties

        // Storage for search result
        protected virtual PackageInquiryDetail[] changePackageSearchResult
        {
            get
            {
                PackageInquiryDetail[] changePackageSearchResult = Page.DataContract.GetValueByName<PackageInquiryDetail[]>(changePackageSearchDetail);
                return changePackageSearchResult ?? new PackageInquiryDetail[0];
            }
            set
            {
                Page.DataContract.SetValueByName(changePackageSearchDetail, value);
            }
        }

        #endregion

        #region Protected Methods: Controls

        // Filters
        protected virtual CWC.TextBox PackageName
        {
            get { return Page.FindCamstarControl("PackageInquiry_PackageName") as CWC.TextBox; }
        }
        protected virtual CWGC.MultiSelectPickList PackageStepsMultiSelect
        {
            get { return Page.FindCamstarControl("PackageInquiry_PackageSteps") as CWGC.MultiSelectPickList; }
        }
        protected virtual CWGC.MultiSelectPickList PackageStatusMultiSelect
        {
            get { return Page.FindCamstarControl("PackageInquiry_PackageStatus") as CWGC.MultiSelectPickList; }
        }
        protected virtual CWC.NamedObject PackageTarget
        {
            get { return Page.FindCamstarControl("PackageInquiry_TargetSystem") as CWC.NamedObject; }
        }
        protected virtual CWC.NamedObject PackageOwner
        {
            get { return Page.FindCamstarControl("PackageInquiry_PackageOwner") as CWC.NamedObject; }
        }
        protected virtual CWC.CheckBox IsOwner
        {
            get { return Page.FindCamstarControl("PackageInquiry_IsOwner") as CWC.CheckBox; }
        }
        protected virtual CWC.CheckBox IsApprover
        {
            get { return Page.FindCamstarControl("PackageInquiry_IsApprover") as CWC.CheckBox; }
        }
        protected virtual CWC.CheckBox IsCollaborator
        {
            get { return Page.FindCamstarControl("PackageInquiry_IsCollaborator") as CWC.CheckBox; }
        }
        protected virtual CWC.CheckBox HasCollaboratorRole
        {
            get { return Page.FindCamstarControl("PackageInquiry_HasCollaboratorRole") as CWC.CheckBox; }
        }
        protected virtual CWC.CheckBox HasApproverRole
        {
            get { return Page.FindCamstarControl("PackageInquiry_HasApproverRole") as CWC.CheckBox; }
        }
        protected virtual CWC.CheckBox HasOwnerRole
        {
            get { return Page.FindCamstarControl("PackageInquiry_HasOwnerRole") as CWC.CheckBox; }
        }
        protected virtual CWC.CheckBox IsApprovalCompleted
        {
            get { return Page.FindCamstarControl("PackageInquiry_IsApprovalCompleted") as CWC.CheckBox; }
        }
        protected virtual CWC.CheckBox IsCollaborationCompleted
        {
            get { return Page.FindCamstarControl("PackageInquiry_Collaboration") as CWC.CheckBox; }
        }
        protected virtual CWC.RevisionedObject PackageWorkflow
        {
            get { return Page.FindCamstarControl("PackageInquiry_Workflow") as CWC.RevisionedObject; }
        }
        protected virtual CWC.NamedObject OwnerRole
        {
            get { return Page.FindCamstarControl("PackageInquiry_OwnerRole") as CWC.NamedObject; }
        }
        protected virtual CWC.NamedObject InstanceID
        {
            get { return Page.FindCamstarControl("InstanceID") as CWC.NamedObject; }
        }
        protected virtual CWC.NamedObject PriorityCode
        {
            get { return Page.FindCamstarControl("PackageInquiry_PackagePriorityCode") as CWC.NamedObject; }
        }
        protected virtual CWC.NamedObject PackageType
        {
            get { return Page.FindCamstarControl("PackageInquiry_PackageType") as CWC.NamedObject; }
        }

        protected virtual CWC.DropDownList ContainsObject
        {
            get { return Page.FindCamstarControl("PackageInquiry_SelectedObjectType") as CWC.DropDownList; }
        }
        protected virtual CWC.DropDownList ContainsInstance
        {
            get { return Page.FindCamstarControl("PackageInquiry_SelectedInstance") as CWC.DropDownList; }
        }

        // Buttons
        protected virtual CWC.Button SearchButton
        {
            get { return Page.FindCamstarControl("SearchButton") as CWC.Button; }
        }
        protected virtual CWC.Button ClearAllButton
        {
            get { return Page.FindCamstarControl("ClearAllButton") as CWC.Button; }
        }

        // JQDataGrids
        protected virtual CWGC.JQDataGrid ChangePackageGrid
        {
            get { return Page.FindCamstarControl("ChangePackageGrid") as CWGC.JQDataGrid; }
        }

        // ActionsControl
        protected virtual ActionsControl PackageActions
        {
            get { return Page.FindIForm("ActionsControl") as ActionsControl; }
        }

        #endregion

        #region Protected Methods: Override Events

        /// <summary>
        /// Bind event handlers to search and clear buttons
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            ScriptManager.RegisterStartupScript(Page.Form, Page.Form.GetType(), "PackageInquiryFunctions",
               string.Format("PackageInquiry_AddToggleSearchAttr(" + _isClearAll.ToString().ToLower() + ");"), true);
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);            
            _isClearAll = false;
            SearchButton.Click += SearchChangePackage;
            ClearAllButton.Click += ClearAll;
            ChangePackageGrid.RowSelected += ChangePackageGrid_RowSelected;
            (PackageStatusMultiSelect.PickListPanelControl.ViewControl as JQDataGrid).PreRender += ChangePackageSearch_PreRender;

            if (Page.SessionVariables.GetValueByName("ReloadPage") != null)
            {
                SearchChangePackage(null, null);
                Page.SessionVariables.SetValueByName("ReloadPage",null);
            }
            var packageName = Page.Request.Params["PackageName"];
            if(packageName != null)
            {
                this.ClearAll(null, null);
                this.PackageName.Data = packageName;                
                this.PerformSearch();
            }
        }

        protected virtual void ChangePackageSearch_PreRender(object sender, EventArgs e)
        {
            var grid = (PackageStatusMultiSelect.PickListPanelControl.ViewControl as JQDataGrid);
            if (grid != null)
            {
                if (grid.GridContext.SelectedRowIDs == null)
                    foreach (var row in DefaultSelectPackageStatuses)
                    {
                        grid.GridContext.SelectRow(row.ToString(), true);
                    }
            }
        }

        protected virtual ResponseData ChangePackageGrid_RowSelected(object sender, JQGridEventArgs args)
        {
            PackageActions.Hidden = true;
            PackageActions.RenderToClient = true;

            if (ChangePackageGrid.GridContext.SelectedItem != null && (ChangePackageGrid.GridContext.SelectedItem as PackageInquiryDetail).ChangePackage.Name != null)
            {
                PackageActions.Hidden = false;
                Page.SessionVariables.SetValueByName("ChangePackage", (ChangePackageGrid.GridContext.SelectedItem as PackageInquiryDetail).ChangePackage.Name);
            }

            return null;
        }

        #endregion

        #region Protected Methods
        /// <summary>
        /// Call the PackageInquiry service to retrieve the data and display in the grid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void SearchChangePackage(object sender, EventArgs e)
        {
            ClearGridData();
            PackageActions.Hidden = true;
            InstanceID.ClearData();
            PerformSearch();
        }

        protected virtual void PerformSearch()
        {
            var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            if (session != null)
            {
                var service = new PackageInquiryService(session.CurrentUserProfile);

                //// Set up parameters for the service here
                var serviceData = new PackageInquiry()
                {
                    PackageName = PackageName.Data as string,
                    TargetSystem = PackageTarget.Data as NamedObjectRef,
                    PackageOwner = PackageOwner.Data as NamedObjectRef,
                    PackagePriorityCode = PriorityCode.Data as NamedObjectRef,
                    PackageType = PackageType.Data as NamedObjectRef,
                    SelectedInstance = ContainsInstance.Data as string,
                    HasApproverRole = new Primitive<bool>((bool)HasApproverRole.Data),
                    HasOwnerRole = new Primitive<bool>((bool)HasOwnerRole.Data),
                    HasCollaboratorRole = new Primitive<bool>((bool)HasCollaboratorRole.Data),
                    IsApprovalCompleted = new Primitive<bool>((bool)IsApprovalCompleted.Data),
                    IsApprover = new Primitive<bool>((bool)IsApprover.Data),
                    IsOwner = new Primitive<bool>((bool)IsOwner.Data),
                    IsCollaborator = new Primitive<bool>((bool)IsCollaborator.Data),
                    IsCollaborationCompleted = new Primitive<bool>((bool)IsCollaborationCompleted.Data),
                    PackageWorkflow = PackageWorkflow.Data as RevisionedObjectRef
                };

                var wcf = new WCFUtilities.WCFObject(serviceData);
                // set package states.
                PackageStepsMultiSelect.CustomDataSetter(PackageStepsMultiSelect, PackageStepsMultiSelect.FieldExpressions, wcf);
                PackageStatusMultiSelect.CustomDataSetter(PackageStatusMultiSelect, PackageStatusMultiSelect.FieldExpressions, wcf);

                if (ContainsObject.Data != null)
                {
                    serviceData.SelectedObjectType = (int)ContainsObject.Data;
                }

                var request = new PackageInquiry_Request()
                {
                    Info = new PackageInquiry_Info()
                    {
                        PackageDetails = new PackageInquiryDetail_Info { RequestValue = true }
                    }
                };

                var result = new PackageInquiry_Result();

                ResultStatus resultStatus = service.GetPackages(serviceData, request, out result);

                if (resultStatus != null && resultStatus.IsSuccess)
                {
                    changePackageSearchResult = result.Value.PackageDetails;
                    ChangePackageGrid.Data = changePackageSearchResult;
                    ChangePackageGrid.DataBind();
                    UpdateDataTypes();
                }
                else
                {
                    DisplayMessage(resultStatus);
                }
            }
        }

        protected virtual void UpdateDataTypes()
        {
            var rows = ChangePackageGrid.Data as PackageInquiryDetail[];
            int selectionDataIndex = 0;
            if (rows != null)
            {
                var boundParametersContext = ChangePackageGrid.GridContext as BoundContext;
                if (boundParametersContext != null)
                {
                    if (boundParametersContext.UnboundData == null)
                        boundParametersContext.UnboundData = new Dictionary<UnboundKey, object>();
                    else
                        boundParametersContext.UnboundData.Clear();
                    for (var i = 0; i < rows.Length; ++i)
                    {
                        var rowId = boundParametersContext.MakeAutoRowId(i);
                        var key = new UnboundKey() { Row = rowId, Column = DeploymentDetails };
                        if (rows[i].TargetDeployments != null)
                        {
                            boundParametersContext.UnboundData.Add(key, CreateStatusFromTargetDeployments(rows[i].TargetDeployments));
                            ++selectionDataIndex;
                        }
                    }
                }
            }
        }

        protected virtual void ClearAll(object sender, EventArgs e)
        {
            ClearPageData();
            ClearGridData();
            PackageActions.Hidden = true;
            _isClearAll = true;
        }

        protected virtual void ClearPageData()
        {
            Page.ClearValues();
        }

        protected virtual void ClearGridData()
        {
            ChangePackageGrid.ClearData();
            ChangePackageGrid.OriginalData = null;
            ChangePackageGrid.GridContext.CurrentPage = 1;
        }

        #endregion

        #region Private Functions
        protected virtual string CreateStatusFromTargetDeployments(TargetDeploymentDetail[] targetDeployments)
        {
            bool errorInDeployFound = false;
            bool incompleteDeployFound = false;
            bool completedDeployFound = false;
            string returnStatus;

            foreach (TargetDeploymentDetail targetDeployDetail in targetDeployments)
            {
                if (targetDeployDetail.Status == TargetDeliveryStatusEnum.Error)
                {
                    errorInDeployFound = true;
                }
                else if (targetDeployDetail.Status == TargetDeliveryStatusEnum.Pending || targetDeployDetail.Status == TargetDeliveryStatusEnum.Reprocess
                    || targetDeployDetail.Status == TargetDeliveryStatusEnum.Locked)
                {
                    incompleteDeployFound = true;
                }
                else if (targetDeployDetail.Status == TargetDeliveryStatusEnum.Completed)
                {
                    completedDeployFound = true;
                }
            }

            if (errorInDeployFound == true)
            {
                returnStatus = FrameworkManagerUtil.GetLabelValue("Lbl_Failed");
            }
            else if (errorInDeployFound == false && incompleteDeployFound == true)
            {
                returnStatus = FrameworkManagerUtil.GetLabelValue("Lbl_InProgress");
            }
            else if (errorInDeployFound == false && incompleteDeployFound == false && completedDeployFound == true)
            {
                returnStatus = FrameworkManagerUtil.GetLabelValue("Lbl_Complete");
            }
            else
            {
                returnStatus = string.Empty;
            }

            return returnStatus;
        }
        #endregion

        private List<PackageStatusEnum> DefaultSelectPackageStatuses = new List<PackageStatusEnum>()
        {
            PackageStatusEnum.Open,
            PackageStatusEnum.Deployed,
            PackageStatusEnum.Rejected
        };

    }
}
