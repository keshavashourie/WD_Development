//© 2023 Siemens Product.
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;


using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using OM = Camstar.WCF.ObjectStack;


/// <summary>
/// Summary description for PartPopUp
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Parts
{

    public class ResourcePartsPopUp : MatrixWebPart
    {
        #region Controls

        private CWC.TextBox selectedresource { get { return Page.FindCamstarControl("SelectedResource") as CWC.TextBox; } }
        private JQDataGrid resourcepartspanel { get { return Page.FindCamstarControl("ResourcePartsPanel") as JQDataGrid; } }

        #endregion
        public ResourcePartsPopUp()
        {
            //
            // TODO: Add constructor logic here
            //
        }


        #region Protected Functions
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            GetSelectedResource();
            if (!Page.IsPostBack)
            {
                ScriptManager.RegisterStartupScript(this.Page.Form, this.Page.GetType(), "PopUpMaximize", "pop.GetCallerPage().pop.maximize();", true);
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            FetchGridDataMaterialList();
        }
        #endregion

        #region Public Functions
        public void FetchGridDataMaterialList()
        {

            string serviceName = this.PrimaryServiceType.ToString();

            var fs = FrameworkManagerUtil.GetFrameworkSession();
            UserProfile profile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
            ResultStatus res = new ResultStatus(null, false);

            PartTxnService Svc = new PartTxnService(fs.CurrentUserProfile);
            PartTxn SvcData = new PartTxn();
            PartTxn_Info SvcInfo = new PartTxn_Info();
            PartTxn_Request ReqData = new PartTxn_Request();
            PartTxn_Result ResData = new PartTxn_Result();

            SvcData.Resource = new NamedObjectRef();
            if (selectedresource.Data != null)
                SvcData.Resource.Name = selectedresource.Data.ToString();

            SvcInfo.ResourceParts = new ResourcePart_Info();
            SvcInfo.ResourceParts.PartName = new Info(true);
            SvcInfo.ResourceParts.PartQty = new Info(true);
            SvcInfo.ResourceParts.MaterialPart = new Info(true);

            ReqData.Info = SvcInfo;
            OM.ResultStatus Results = Svc.Load(SvcData, ReqData, out ResData);
            if (Results.IsSuccess)
            {
                resourcepartspanel.Data = ResData.Value.ResourceParts;
                resourcepartspanel.OriginalData = ResData.Value.ResourceParts;
            }
        }

        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as CustomAction;
            if (action != null)
                if (action.Parameters == "ClosePop")
                {
                    ScriptManager.RegisterStartupScript(Page.Form, GetType(), "CloseButton", "window.parent.__page._isTabContainerPage=null;window.parent.CloseFloatingFrame(true);", true);
                }
        }
        #endregion

        #region Private Functions
        private void GetSelectedResource()
        {
            if (Page.PortalContext.DataContract.GetValueByName("ResourceParts_PartRequestMain") != null)
            {
                string SelectedResource = Page.PortalContext.DataContract.GetValueByName("ResourceParts_PartRequestMain").ToString();
                selectedresource.Data = SelectedResource;
            }
            else if (Page.PortalContext.DataContract.GetValueByName("SelectedResource") != null)
            {
                string SelectedResource = Page.PortalContext.DataContract.GetValueByName("SelectedResource").ToString();
                selectedresource.Data = SelectedResource;
            }
            else if (Page.PortalContext.DataContract.GetValueByName("SelectedResource_ResourceParts") != null)
            {
                string SelectedResource = Page.PortalContext.DataContract.GetValueByName("SelectedResource_ResourceParts").ToString();
                selectedresource.Data = SelectedResource;
            }
            string isPopup = Page.PortalContext.DataContract.GetValueByName("IsPopUp_ResourceParts") as string;
            if (isPopup != "Yes")
            {
                selectedresource.Visible = false;
                if (!Page.IsResponsive)
                {
                    resourcepartspanel.BoundContext.Width = 580;
                    this.Margin.Top = 15;
                }
                else
                {
                    resourcepartspanel.BoundContext.Width = Convert.ToInt32(this.Width.Value) - 30;
                }
                this.Page.ActionDispatcher.PageActions().Where(act => act.Name == "CloseButton").FirstOrDefault().IsHidden = true;
            }
        }
        #endregion

        #region Constants

        #endregion

        #region Private Member Variables

        #endregion

    }

}
