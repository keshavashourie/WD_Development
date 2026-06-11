// Copyright Siemens 2023  
using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.PortalFramework;

using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework.Utilities;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using CWGC = Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WCF.ObjectStack;

namespace Camstar.WebPortal.WebPortlets.ChangeManagement
{

    /// <summary>
    /// TODO: Add a Summary description for this Camstar Web Part
    /// </summary>
    public class ChangePackageSearchPopup : MatrixWebPart
    {
        #region Controls        
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
        protected virtual CWC.NamedObject PriorityCode
        {
            get { return Page.FindCamstarControl("PackageInquiry_PackagePriorityCode") as CWC.NamedObject; }
        }
        protected virtual CWC.NamedObject PackageType
        {
            get { return Page.FindCamstarControl("PackageInquiry_PackageType") as CWC.NamedObject; }
        }
        protected virtual CWC.DropDownList SelectedObjectType
        {
            get { return Page.FindCamstarControl("PackageInquiry_SelectedObjectType") as CWC.DropDownList; }
        }
        protected virtual CWC.DropDownList ContainsInstance
        {
            get { return Page.FindCamstarControl("PackageInquiry_SelectedInstance") as CWC.DropDownList; }
        }
        protected virtual CWC.Button SearchButton
        {
            get { return Page.FindCamstarControl("SearchButton") as CWC.Button; }
        }
        protected virtual CWC.Button ClearAllButton
        {
            get { return Page.FindCamstarControl("ClearAllButton") as CWC.Button; }
        }
        protected virtual CWGC.JQDataGrid ChangePackageGrid
        {
            get { return Page.FindCamstarControl("ChangePackageGrid") as CWGC.JQDataGrid; }
        }
        #endregion

        #region Protected Override Methods

        /// <summary>
        /// TODO: Summary Description of function
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            var preReqChangePkgsReturnDM = Page.DataContract.GetValueByName<PackageInquiryDetail[]>(PreReqChangePkgsReturnDM);

            SearchButton.Click += SearchChangePackage;
            ClearAllButton.Click += ClearAll;
            (PackageStatusMultiSelect.PickListPanelControl.ViewControl as JQDataGrid).PreRender += ChangePackageSearch_PreRender;                              

            if (Page.SessionVariables.GetValueByName("ReloadPage") != null)
            {
                SearchChangePackage(null, null);
                Page.SessionVariables.SetValueByName("ReloadPage", null);
            }
        }

        #endregion

        #region Protected Virtual Methods
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

        protected virtual void SearchChangePackage(object sender, EventArgs e)
        {
            ClearGridData();

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
                    PackageWorkflow = PackageWorkflow.Data as RevisionedObjectRef,
                    SelectedInstance = ContainsInstance.Data as string                  
                };

                var wcf = new WCFUtilities.WCFObject(serviceData);
                // set package states.
                PackageStepsMultiSelect.CustomDataSetter(PackageStepsMultiSelect, PackageStepsMultiSelect.FieldExpressions, wcf);
                PackageStatusMultiSelect.CustomDataSetter(PackageStatusMultiSelect, PackageStatusMultiSelect.FieldExpressions, wcf);

                if (SelectedObjectType.Data != null)
                {
                    serviceData.SelectedObjectType = (int)SelectedObjectType.Data;
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
                    var changePackageSearchResultList = new List<PackageInquiryDetail>(result.Value.PackageDetails);
                    var selectedChangePackage = Page.DataContract.GetValueByName<NamedObjectRef>("SelectedPackageName");

                    if (selectedChangePackage != null)
                    {
                        var currentPackage = changePackageSearchResultList.Where(x => x.ChangePackage.Name == selectedChangePackage.Name);
                        if(currentPackage != null && currentPackage.Count() > 0) {
                            changePackageSearchResultList.Remove(currentPackage.First());
                        }
                    }

                    ChangePackageGrid.Data = changePackageSearchResultList.ToArray();
                    ChangePackageGrid.DataBind();
                }
                else
                {
                    DisplayMessage(resultStatus);
                }
            }
        }

        protected virtual void ClearAll(object sender, EventArgs e)
        {
            ClearPageData();
            ClearGridData();
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

        #region Public Override Methods
        public override void WebPartCustomAction(object sender, CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var resp = Page.DataContract.GetValueByName("__Page_PopupData") as PopupData;
            resp.SubID = "PackageName";
            if (e.Action.CommandName == "Add")
            {
                var selRows = ChangePackageGrid.GridContext.SelectedRowIDs;
                var preReqChangePkgs = ((Page.DataContract.GetValueByName("__Page_PopupData") as PopupData).Caller as JQDataGrid).Data as PackageInquiryDetail[];
                var newList = preReqChangePkgs.ToList();
                var data = ChangePackageGrid.Data as PackageInquiryDetail[];                
                if (selRows != null)
                {
                    foreach (var selRow in selRows)
                    {
                        var doesExist = newList.Exists(i => i.ChangePackage.Name.Value == selRow);
                        if (!doesExist)
                        {
                            var curRow = data.Where(x => x.ChangePackage.Name.Value == selRow);
                            if (curRow != null && curRow.Count() > 0)
                            {
                                newList.Add(curRow.First());
                            }
                        }
                    }                    
                    resp.ReturnedData = newList.ToArray();                    
                }
                else
                {
                    resp.ReturnedData = new PackageInquiryDetail[0];
                }
                Page.CloseFloatingFrame(true);
            }  
            else
            {
                resp.ReturnedData = new PackageInquiryDetail[0];
                Page.CloseFloatingFrame(true);
            }          
        }

        #endregion
        
        #region Public Functions                
        #endregion

        #region Private Functions       

        #endregion

        #region Constants
        private const string DeploymentDetails = "DeploymentDetails";
        private const string PreReqChangePkgsReturnDM = "PreReqChangePkgsReturnDM";
        #endregion

        #region Private Member Variables

        protected List<PackageStatusEnum> DefaultSelectPackageStatuses = new List<PackageStatusEnum>()
        {
            PackageStatusEnum.Open,
            PackageStatusEnum.Deployed,
            PackageStatusEnum.Rejected
        };
        #endregion

    }
}


