//© 2022 Copyright Siemens.
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using OM = Camstar.WCF.ObjectStack;

namespace Camstar.WebPortal.WebPortlets.Parts
{
    
    public class PartGeneral : MatrixWebPart
    {
        #region Controls

        private CWC.NamedObject resource { get { return Page.FindCamstarControl("Part_Resource") as CWC.NamedObject; } }
        private CWC.InquiryControl partqtyinuse { get { return Page.FindCamstarControl("ResourceStatusDetails_PartQtyInUse") as CWC.InquiryControl; } }
        private CWC.InquiryControl partqtyinrequest { get { return Page.FindCamstarControl("ResourceStatusDetails_PartQtyInRequest") as CWC.InquiryControl; } }
        private CWC.InquiryControl partdecommissioned { get { return Page.FindCamstarControl("PartDecommissioned") as CWC.InquiryControl; } }
        private CWC.TextBox _txtComments { get { return Page.FindCamstarControl("Shopfloor_Comments") as CWC.TextBox; } }
        private ToggleContainer _toggleComments { get { return Page.FindCamstarControl("PartComments_Toggle") as ToggleContainer; } }
        private CWC.TextBox scrapqty { get { return Page.FindCamstarControl("PartScrap_ScrapQty") as CWC.TextBox; } }

        #endregion

        #region Protected Functions

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (Page.IsFloatingFrame || Page.EventArgument == "FloatingFrameSubmitParentPostBackArgument")
            { ScriptManager.RegisterStartupScript(Page.Form, GetType(), "PopUpMaximize", "pop.GetCallerPage().pop.maximize();", true); }

            if (this.PrimaryServiceType == "PartSetup" || this.PrimaryServiceType == "PartScrap")
            {
                (Page.FindCamstarControl("Part_Resource") as CWC.NamedObject).DataChanged += new EventHandler(Resource_DataChanged);
                if (!Page.IsPostBack) { Resource_DataChanged("", e); }
                else if (this.PrimaryServiceType == "PartScrap")
                    ScriptManager.RegisterStartupScript(Page.Form, GetType(), "PopUpMaximize", "pop.GetCallerPage().pop.maximize();", true);
            }

            else if (this.PrimaryServiceType == "PartCreate")
            {
                CWC.TextBox partqty = (this.Page.FindCamstarControl("PartDetails_PartQty") as CWC.TextBox);
                if (partqty.Data == null)
                { partqty.Data = "1"; }
                ScriptManager.RegisterStartupScript(Page.Form, GetType(), "PopUpMaximize", "pop.GetCallerPage().pop.maximize();", true);
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (string.Compare(this.PrimaryServiceType, "PartScrap", true) == 0)
                scrapqty.Focus();
        }
        #endregion

        #region Public Functions
        public void Resource_DataChanged(object sender, EventArgs e)
        {
            if (this.PrimaryServiceType == "PartSetup")
            {
                if (resource.Data != null)
                {
                    CWC.DropDownList availability = (this.Page.FindCamstarControl("PartSetup_Availability") as CWC.DropDownList);
                    CWC.NamedObject resourcestatuscode = (this.Page.FindCamstarControl("PartSetup_ResourceStatusCode") as CWC.NamedObject);
                    CWC.NamedObject resourcestatusreason = (this.Page.FindCamstarControl("PartSetup_ResourceStatusReason") as CWC.NamedObject);
                    CWC.TextBox partqty = (this.Page.FindCamstarControl("PartSetup_PartQty") as CWC.TextBox);
                    CWC.NamedObject physicallocation = (this.Page.FindCamstarControl("PartSetup_PhysicalLocation") as CWC.NamedObject);
                    CWC.NamedObject physicalposition = (this.Page.FindCamstarControl("PartSetup_PhysicalPosition") as CWC.NamedObject);
                    CWC.RevisionedObject product = (this.Page.FindCamstarControl("PartSetup_Product") as CWC.RevisionedObject);
                    CWC.RevisionedObject setup = (this.Page.FindCamstarControl("PartSetup_Setup") as CWC.RevisionedObject);
                    CWC.DateChooser partexpirydate = (this.Page.FindCamstarControl("PartSetup_PartExpiryDate") as CWC.DateChooser);

                    string serviceName = this.PrimaryServiceType.ToString();

                    var fs = FrameworkManagerUtil.GetFrameworkSession();
                    UserProfile profile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
                    ResultStatus res = new ResultStatus(null, false);

                    PartSetupService Svc = new PartSetupService(fs.CurrentUserProfile);
                    PartSetup SvcData = new PartSetup();
                    PartSetup_Info SvcInfo = new PartSetup_Info();
                    PartSetup_Request ReqData = new PartSetup_Request();
                    PartSetup_Result ResData = new PartSetup_Result();

                    SvcData.Resource = new NamedObjectRef();
                    SvcData.Resource.Name = resource.Data.ToString();

                    SvcInfo.ResourceStatusDetails = new PartStatusDetails_Info();
                    SvcInfo.ResourceStatusDetails.Availability = new Info(true);
                    SvcInfo.ResourceStatusDetails.PartDecommissioned = new Info(true);
                    SvcInfo.ResourceStatusDetails.PartExpiryDate = new Info(true);
                    SvcInfo.ResourceStatusDetails.PartQty = new Info(true);
                    SvcInfo.ResourceStatusDetails.PartQtyInRequest = new Info(true);
                    SvcInfo.ResourceStatusDetails.PartQtyInUse = new Info(true);
                    SvcInfo.ResourceStatusDetails.PhysicalLocation = new Info(true);
                    SvcInfo.ResourceStatusDetails.PhysicalPosition = new Info(true);
                    SvcInfo.ResourceStatusDetails.Product = new Info(true);
                    SvcInfo.ResourceStatusDetails.Setup = new Info(true);
                    SvcInfo.ResourceStatusDetails.Status = new Info(true);
                    SvcInfo.ResourceStatusDetails.Reason = new Info(true);
                    
                    ReqData.Info = SvcInfo;

                    OM.ResultStatus Results = Svc.Load(SvcData, ReqData, out ResData);
                    if (Results.IsSuccess)
                    {
                        if (ResData.Value.ResourceStatusDetails.Availability != null)
                        {
                            if (ResData.Value.ResourceStatusDetails.Availability.Value == "Up") availability.Data = 1;
                            else availability.Data = 2;

                            availability.Text = ResData.Value.ResourceStatusDetails.Availability.Value;
                        }
                        else
                        { availability.Text = ""; }

                        if (ResData.Value.ResourceStatusDetails.PartDecommissioned != null)
                        { partdecommissioned.Data = ResData.Value.ResourceStatusDetails.PartDecommissioned.Value; }
                        else
                        { partdecommissioned.Data = null; }

                        if (ResData.Value.ResourceStatusDetails.PartExpiryDate != null)
                        { partexpirydate.Data = ResData.Value.ResourceStatusDetails.PartExpiryDate.Value; }
                        else
                        { partexpirydate.Data = null; }

                        if (ResData.Value.ResourceStatusDetails.PartQty != null)
                        { partqty.Data = ResData.Value.ResourceStatusDetails.PartQty.Value.ToString(); }
                        else
                        { partqty.Data = null; }

                        if (ResData.Value.ResourceStatusDetails.PartQtyInRequest != null)
                        { partqtyinrequest.Data = ResData.Value.ResourceStatusDetails.PartQtyInRequest.Value.ToString(); }
                        else
                        { partqtyinrequest.Data = null; }

                        if (ResData.Value.ResourceStatusDetails.PartQtyInUse != null)
                        { partqtyinuse.Data = ResData.Value.ResourceStatusDetails.PartQtyInUse.Value.ToString(); }
                        else
                        { partqtyinuse.Data = null; }

                        if (ResData.Value.ResourceStatusDetails.PhysicalLocation != null)
                        { physicallocation.Data = ResData.Value.ResourceStatusDetails.PhysicalLocation.Name; }
                        else
                        { physicallocation.Data = null; }

                        if (ResData.Value.ResourceStatusDetails.PhysicalPosition != null)
                        { physicalposition.Data = ResData.Value.ResourceStatusDetails.PhysicalPosition.Name; }
                        else
                        { physicalposition.Data = null; }

                        if (ResData.Value.ResourceStatusDetails.Product != null)
                        { product.Data = ResData.Value.ResourceStatusDetails.Product; }
                        else
                        { product.Data = null; }

                        if (ResData.Value.ResourceStatusDetails.Setup != null)
                        { setup.Data = ResData.Value.ResourceStatusDetails.Setup; }
                        else
                        { setup.Data = null; }

                        if (ResData.Value.ResourceStatusDetails.Status != null)
                        { resourcestatuscode.Data = ResData.Value.ResourceStatusDetails.Status.Name; }
                        else
                        { resourcestatuscode.Data = null; }

                        if (ResData.Value.ResourceStatusDetails.Reason != null)
                        { resourcestatusreason.Data = ResData.Value.ResourceStatusDetails.Reason.Name; }
                        else
                        { resourcestatusreason.Data = null; }
                        
                    }
                    else
                    {
                        DisplayMessage(Results);
                    }
                }
            }
            else if (this.PrimaryServiceType == "PartScrap")
            {
                if (resource.Data != null)
                {
                    CWC.TextBox availability = (this.Page.FindCamstarControl("ResourceStatusDetails_Availability") as CWC.TextBox);
                    CWC.NamedObject resourcestatuscode = (this.Page.FindCamstarControl("ResourceStatusDetails_Status") as CWC.NamedObject);
                    CWC.NamedObject resourcestatusreason = (this.Page.FindCamstarControl("ResourceStatusDetails_Reason") as CWC.NamedObject);
                    CWC.TextBox partqty = (this.Page.FindCamstarControl("ResourceStatusDetails_PartQty") as CWC.TextBox);
                    CWC.NamedObject physicallocation = (this.Page.FindCamstarControl("ResourceStatusDetails_PhysicalLocation") as CWC.NamedObject);
                    CWC.NamedObject physicalposition = (this.Page.FindCamstarControl("ResourceStatusDetails_PhysicalPosition") as CWC.NamedObject);
                    CWC.RevisionedObject product = (this.Page.FindCamstarControl("ResourceStatusDetails_Product") as CWC.RevisionedObject);
                    CWC.RevisionedObject setup = (this.Page.FindCamstarControl("ResourceStatusDetails_Setup") as CWC.RevisionedObject);

                    string serviceName = this.PrimaryServiceType.ToString();

                    var fs = FrameworkManagerUtil.GetFrameworkSession();
                    UserProfile profile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
                    ResultStatus res = new ResultStatus(null, false);

                    PartScrapService Svc = new PartScrapService(fs.CurrentUserProfile);
                    PartScrap SvcData = new PartScrap();
                    PartScrap_Info SvcInfo = new PartScrap_Info();
                    PartScrap_Request ReqData = new PartScrap_Request();
                    PartScrap_Result ResData = new PartScrap_Result();

                    SvcData.Resource = new NamedObjectRef();
                    SvcData.Resource.Name = resource.Data.ToString();

                    SvcInfo.ResourceStatusDetails = new PartStatusDetails_Info();
                    SvcInfo.ResourceStatusDetails.PartQtyInUse = new Info(true);
                    SvcInfo.ResourceStatusDetails.PartQtyInRequest = new Info(true);
                    SvcInfo.ResourceStatusDetails.PartDecommissioned = new Info(true);
                    SvcInfo.ResourceStatusDetails.Availability = new Info(true);
                    SvcInfo.ResourceStatusDetails.Status = new Info(true);
                    SvcInfo.ResourceStatusDetails.Reason = new Info(true);
                    SvcInfo.ResourceStatusDetails.Product = new Info(true);
                    SvcInfo.ResourceStatusDetails.PartQty = new Info(true);
                    SvcInfo.ResourceStatusDetails.Setup = new Info(true);
                    SvcInfo.ResourceStatusDetails.PhysicalLocation = new Info(true);
                    SvcInfo.ResourceStatusDetails.PhysicalPosition = new Info(true);

                    ReqData.Info = SvcInfo;

                    OM.ResultStatus Results = Svc.Load(SvcData, ReqData, out ResData);
                    if (Results.IsSuccess)
                    {
                        if (ResData.Value.ResourceStatusDetails.PartQtyInUse != null)
                            partqtyinuse.Data = ResData.Value.ResourceStatusDetails.PartQtyInUse.Value.ToString();
                        else
                            partqtyinuse.Data = null;

                        if (ResData.Value.ResourceStatusDetails.PartQtyInRequest != null)
                            partqtyinrequest.Data = ResData.Value.ResourceStatusDetails.PartQtyInRequest.Value.ToString();
                        else
                            partqtyinrequest.Data = null;

                        if (ResData.Value.ResourceStatusDetails.PartDecommissioned != null)
                            partdecommissioned.Data = ResData.Value.ResourceStatusDetails.PartDecommissioned.Value;
                        else
                            partdecommissioned.Data = null;

                        if (ResData.Value.ResourceStatusDetails.Availability != null)
                            availability.Data = ResData.Value.ResourceStatusDetails.Availability.Value;
                        else
                            availability.Data = null;

                        if (ResData.Value.ResourceStatusDetails.Status != null)
                            resourcestatuscode.Data = ResData.Value.ResourceStatusDetails.Status.Name;
                        else
                            resourcestatuscode.Data = null;

                        if (ResData.Value.ResourceStatusDetails.Reason != null)
                            resourcestatusreason.Data = ResData.Value.ResourceStatusDetails.Reason.Name;
                        else
                            resourcestatusreason.Data = null;

                        if (ResData.Value.ResourceStatusDetails.PartQty != null)
                            partqty.Data = ResData.Value.ResourceStatusDetails.PartQty.Value.ToString();
                        else
                            partqty.Data = null;

                        if (ResData.Value.ResourceStatusDetails.PhysicalLocation != null)
                            physicallocation.Data = ResData.Value.ResourceStatusDetails.PhysicalLocation.Name;
                        else
                            physicallocation.Data = null;

                        if (ResData.Value.ResourceStatusDetails.PhysicalPosition != null)
                            physicalposition.Data = ResData.Value.ResourceStatusDetails.PhysicalPosition.Name;
                        else
                            physicalposition.Data = null;

                        if (ResData.Value.ResourceStatusDetails.Product != null)
                            product.Data = ResData.Value.ResourceStatusDetails.Product;
                        else
                            product.Data = null;

                        if (ResData.Value.ResourceStatusDetails.Setup != null)
                            setup.Data = ResData.Value.ResourceStatusDetails.Setup;
                        else
                            setup.Data = null;
                    }
                    else
                    {
                        DisplayMessage(Results);
                    }
                }
            }
        }

        public void PartCreateReset()
        {
            (this.FindCamstarControl("PartDetails_Name") as CWC.TextBox).Data = null;
            (this.FindCamstarControl("PartDetails_Product") as CWC.RevisionedObject).Data = (this.FindCamstarControl("Hidden_Product") as CWC.RevisionedObject).Data;
            (this.FindCamstarControl("PartDetails_ResourceFamily") as CWC.NamedObject).Data = (this.FindCamstarControl("Hidden_ResourceFamily") as CWC.NamedObject).Data;
            (this.FindCamstarControl("PartDetails_PartQty") as CWC.TextBox).Data = "1";
            (this.FindCamstarControl("PartDetails_PartQty") as CWC.TextBox).Data = (this.FindCamstarControl("Hidden_PartQty") as CWC.TextBox).Data;
            (this.FindCamstarControl("PartDetails_Factory") as CWC.NamedObject).Data = (this.FindCamstarControl("Hidden_Factory") as CWC.NamedObject).Data;
            (this.FindCamstarControl("PartDetails_Vendor") as CWC.NamedObject).Data = (this.FindCamstarControl("Hidden_Vendor") as CWC.NamedObject).Data;
            (this.FindCamstarControl("PartDetails_VendorModel") as CWC.TextBox).Data = (this.FindCamstarControl("Hidden_VendorModel") as CWC.TextBox).Data;
            (this.FindCamstarControl("PartDetails_VendorSerialNumber") as CWC.TextBox).Data = (this.FindCamstarControl("Hidden_VendorSerialNumber") as CWC.TextBox).Data;
            (this.FindCamstarControl("PartDetails_PartExpiryDate") as CWC.DateChooser).Data = (this.FindCamstarControl("Hidden_PartExpiryDate") as CWC.DateChooser).Data;
            (this.FindCamstarControl("PartDetails_PhysicalLocation") as CWC.NamedObject).Data = (this.FindCamstarControl("Hidden_PhysicalLocation") as CWC.NamedObject).Data;
            (this.FindCamstarControl("PartDetails_PhysicalPosition") as CWC.NamedObject).Data = (this.FindCamstarControl("Hidden_PhysicalPosition") as CWC.NamedObject).Data;
        }

        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as CustomAction;
            if (action != null)
            {
                if (action.Parameters == "ResetFields")
                {
                    if (this.PrimaryServiceType == "PartSetup")
                        Resource_DataChanged("", e);
                    else if (this.PrimaryServiceType == "PartScrap")
                        scrapqty.Data = null;
                    else if (this.PrimaryServiceType == "PartCreate")
                        PartCreateReset();

                    _txtComments.ClearData();
                    _toggleComments.Reset();
                }
            }
        }

        #endregion

        #region Private Functions

        #endregion

        #region Constants

        #endregion

        #region Private Member Variables

        #endregion

    }

}

