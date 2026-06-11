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
    public class ActivationSearch : MatrixWebPart
    {
        #region Constants

        protected const string activationSearchDetail = "ActivationSearchDetail";
        protected List<ChangePackageImportStatusEnum> DefaultSelectCPImportStatuses = new List<ChangePackageImportStatusEnum>()
        {
            ChangePackageImportStatusEnum.ActivationIncomplete,
            ChangePackageImportStatusEnum.PendingActivation
        };

        #endregion

        #region Properties

        // Storage for search result
        protected virtual ActivationInquiryDetail[] ActivationSearchResult
        {
            get
            {
                ActivationInquiryDetail[] activationSearchResult = Page.DataContract.GetValueByName<ActivationInquiryDetail[]>(activationSearchDetail);
                return activationSearchResult ?? new ActivationInquiryDetail[0];
            }
            set
            {
                Page.DataContract.SetValueByName(activationSearchDetail, value);
            }
        }

        #endregion

        #region Controls

        protected virtual CWC.TextBox PackageName
        {
            get { return Page.FindCamstarControl("ActivationInquiry_PackageName") as CWC.TextBox; }
        }

        protected virtual CWC.DropDownList PackageOwnerName
        {
            get { return Page.FindCamstarControl("ActivationInquiry_PackageOwnerName") as CWC.DropDownList; }
        }

        protected virtual CWC.DropDownList SourceSystemName
        {
            get { return Page.FindCamstarControl("ActivationInquiry_SourceSystemName") as CWC.DropDownList; }
        }

        protected virtual CWC.DateChooser FromDeploymentTimestamp
        {
            get { return Page.FindCamstarControl("ActivationInquiry_FromDeploymentTimestamp") as CWC.DateChooser; }
        }

        protected virtual CWC.DateChooser ToDeploymentTimestamp
        {
            get { return Page.FindCamstarControl("ActivationInquiry_ToDeploymentTimestamp") as CWC.DateChooser; }
        }

        protected virtual CWGC.MultiSelectPickList ActivationState
        {
            get { return Page.FindCamstarControl("ActivationInquiry_ActivationState") as CWGC.MultiSelectPickList; }
        }
        protected virtual CWGC.MultiSelectPickList PackageStatus
        {
            get { return Page.FindCamstarControl("PackageStatus") as CWGC.MultiSelectPickList; }
        }
        protected virtual CWC.NamedObject InstanceID
        {
            get { return Page.FindCamstarControl("InstanceID") as CWC.NamedObject; }
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
        protected virtual CWGC.JQDataGrid ActivationGrid
        {
            get { return Page.FindCamstarControl("ActivationGrid") as CWGC.JQDataGrid; }
        }

        protected virtual ActionsControl PackageActions
        {
            get { return Page.FindIForm("ActionsControl") as ActionsControl; }
        }


        #endregion

        #region Protected Methods: Override Events

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            ScriptManager.RegisterStartupScript(Page.Form, Page.Form.GetType(), "ActivationInquiryFunctions",
               string.Format("ActivationInquiry_AddToggleSearchAttr();"), true);
        }

        /// <summary>
        /// Bind event handlers to search and clear buttons
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            SearchButton.Click += SearchActivation;
            ClearAllButton.Click += ClearAll;
            ActivationGrid.RowSelected += ActivationGrid_RowSelected;
            (ActivationState.PickListPanelControl.ViewControl as JQDataGrid).PreRender += ActivationState_PreRender;
            (PackageStatus.PickListPanelControl.ViewControl as JQDataGrid).PreRender += PackageStatus_PreRender;

            if (Page.SessionVariables.GetValueByName("ReloadPage") != null)
            {
                SearchActivation(null, null);
                Page.SessionVariables.SetValueByName("ReloadPage", null);
            }
        }

        protected virtual ResponseData ActivationGrid_RowSelected(object sender, JQGridEventArgs args)
        {
            PackageActions.Hidden = false;
            PackageActions.RenderToClient = true;

           if (ActivationGrid.GridContext.SelectedItem != null && (ActivationGrid.GridContext.SelectedItem as ActivationInquiryDetail).ChangePackage.Name != null)
            {
                Page.SessionVariables.SetValueByName("ChangePackage", (ActivationGrid.GridContext.SelectedItem as ActivationInquiryDetail).ChangePackage.Name);
            }

            return null;
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// Call the ActivationInquiry service to retrieve the data and display in the grid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void SearchActivation(object sender, EventArgs e)
        {
            ClearGridData();
            PackageActions.Hidden = true;
            InstanceID.ClearData();
            var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            if (session != null)
            {
                var service = new ActivationInquiryService(session.CurrentUserProfile);

                //// Set up parameters for the service here
                var serviceData = new ActivationInquiry()
                {
                    PackageName = PackageName.Data as string,
                    SourceSystemName = SourceSystemName.Data as string,
                    PackageOwnerName = PackageOwnerName.Data as string,
                    FromDeploymentTimestamp = FromDeploymentTimestamp.Data != null ? new Primitive<DateTime>((DateTime)FromDeploymentTimestamp.Data) : null,
                    ToDeploymentTimestamp = ToDeploymentTimestamp.Data != null ? new Primitive<DateTime>((DateTime)ToDeploymentTimestamp.Data) : null
                };

                var wcf = new WCFUtilities.WCFObject(serviceData);
                // set activation states.
                ActivationState.CustomDataSetter(ActivationState, ActivationState.FieldExpressions, wcf);
                //set package status
                PackageStatus.CustomDataSetter(PackageStatus, PackageStatus.FieldExpressions, wcf);

                var request = new ActivationInquiry_Request()
                {
                    Info = new ActivationInquiry_Info()
                    {
                        PackageDetails = new ActivationInquiryDetail_Info { RequestValue = true }
                    }
                };

                var result = new ActivationInquiry_Result();

                ResultStatus resultStatus = service.GetPackages(serviceData, request, out result);

                if (resultStatus != null && resultStatus.IsSuccess)
                {
                    ActivationSearchResult = result.Value.PackageDetails;
                    ActivationGrid.Data = ActivationSearchResult;
                    ActivationGrid.DataBind();
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
            PackageActions.Hidden = true;
        }

        protected virtual void ClearPageData()
        {
            Page.ClearValues();
        }

        protected virtual void ClearGridData()
        {
            ActivationGrid.ClearData();
            ActivationGrid.OriginalData = null;
            ActivationGrid.GridContext.CurrentPage = 1;
        }

        #endregion

        #region Private Methods

        protected virtual void ActivationState_PreRender(object sender, EventArgs e)
        {
            var grid = (ActivationState.PickListPanelControl.ViewControl as JQDataGrid);
            if (grid != null)
            {
                if (grid.GridContext.SelectedRowIDs == null)
                    foreach (var row in DefaultSelectCPImportStatuses)
                    {
                        grid.GridContext.SelectRow(((int)row).ToString(), true);
                    }
            }
        }

        protected virtual void PackageStatus_PreRender(object sender, EventArgs e)
        {
            var grid = (PackageStatus.PickListPanelControl.ViewControl as JQDataGrid);
            if (grid != null)
            {
                if (grid.GridContext.SelectedRowIDs == null)
                    grid.GridContext.SelectRow(((int)PackageStatusEnum.Open).ToString(), true);
            }
        }

        #endregion

    }
}

