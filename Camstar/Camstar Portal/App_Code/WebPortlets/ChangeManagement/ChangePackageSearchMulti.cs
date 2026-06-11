//
// Copyright Siemens 2023  
//
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
using Camstar.WebPortal.FormsFramework.WebGridControls;

/// <summary>
/// Summary description for ChangePackageSearchMulti
/// </summary>

namespace Camstar.WebPortal.WebPortlets.ChangeManagement
{
    public class ChangePackageSearchMulti : MatrixWebPart
    {
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

        protected virtual CWC.RevisionedObject PackageWorkflow
        {
            get { return Page.FindCamstarControl("PackageInquiry_Workflow") as CWC.RevisionedObject; }
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

        protected virtual CWC.NamedObject ChangeMgtTxn_ChangePackage
        {
            get { return Page.FindCamstarControl("ChangeMgtTxn_ChangePackage") as CWC.NamedObject; }
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
        protected virtual CWC.Button ReloadActions
        {
            get { return Page.FindCamstarControl("ReloadActions") as CWC.Button; }
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
             string.Format("PackageSearchMultiple_AddToggleSearchAttr(" + _isClearAll.ToString().ToLower() + ");"), true);

        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _isClearAll = false;
            SearchButton.Click += SearchChangePackage;
            ClearAllButton.Click += ClearAll;
            ChangePackageGrid.RowSelected += ChangePackageGrid_RowSelected;
            (PackageStatusMultiSelect.PickListPanelControl.ViewControl as JQDataGrid).PreRender += ChangePackageSearch_PreRender;
            ReloadActions.Click += ReloadActions_Click;

            if (Page.SessionVariables.GetValueByName("ReloadPage") != null)
            {
                SearchChangePackage(null, null);
                Page.SessionVariables.SetValueByName("ReloadPage", null);
            }

        }

        protected virtual void ReloadActions_Click(object sender, EventArgs e)
        {
            GridContext ctx = ChangePackageGrid.GridContext;
            if (ctx.SelectedRowIDs != null && ctx.SelectedRowIDs.Count > 0)
            {
                PackageActions.Hidden = false;
                PackageActions.RenderToClient = true;
                
                List<string> packages = (from rowId in ctx.SelectedRowIDs
                    let packageInquiryDetails = ctx.GetItem(rowId) as PackageInquiryDetail
                    where
                        packageInquiryDetails != null && !String.IsNullOrEmpty(packageInquiryDetails.PackageName.Value)
                    select packageInquiryDetails.PackageName.Value)
                    .ToList();

                if (packages.Count == 1)
                {
                    Page.DataContract.SetValueByName("ChangePackage", packages.First());
                    Page.SessionVariables.SetValueByName("ChangePackage", packages);
                }
                else
                    Page.SessionVariables.SetValueByName("ChangePackage", packages);

                GetPackageActions(packages);
            }
            else
            {
                PackageActions.Hidden = true;
                PackageActions.RenderToClient = true;
            }

        }

        protected virtual void GetPackageActions(List<string> packages)
        {
            var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            if (session != null && packages != null)
            {
                var service = new ChangeMgtTxnsService(session.CurrentUserProfile);
                var serviceData = new ChangeMgtTxns();
                serviceData.ChangePackages = new NamedObjectRef[packages.Count];
                int i = 0;
                serviceData.ActionSource = new NamedObjectRef("PackageSearchMultiple_VP");

                foreach (var p in packages)
                {
                    serviceData.ChangePackages[i++] = new NamedObjectRef { Name = p };
                }

                var request = new ChangeMgtTxns_Request()
                {
                    Info = new ChangeMgtTxns_Info()
                    {
                        ActionDetails = new ActionDetails_Info()
                        {
                            Action = new Info(true),
                            IsEnabled = new Info(true)

                        }
                    }
                };

                var result = new ChangeMgtTxns_Result();

                ResultStatus resultStatus = service.GetActions(serviceData, request, out result);

                if (resultStatus != null && resultStatus.IsSuccess)
                {
                    PackageActions.ActionStatuses = result.Value.ActionDetails;
                    PackageActions.RenderToClient = true;
                }
                else
                {
                    DisplayMessage(resultStatus);
                }

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
            PackageActions.Hidden = false;
            PackageActions.RenderToClient = true;

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
