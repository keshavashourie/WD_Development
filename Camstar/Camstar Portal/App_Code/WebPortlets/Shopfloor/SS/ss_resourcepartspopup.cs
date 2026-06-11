/* Copyright 2019 Siemens */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Web;
using System.Web.UI;
using Camstar.WCF.Services;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.Personalization;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_ResourcePartsPopUp : MatrixWebPart
    {
        //Control declaration
        private CWC.TextBox _txtSelectedResource { get { return Page.FindCamstarControl("SelectedResource") as CWC.TextBox; } }
        private JQDataGrid _gridResourcePartsPanel { get { return Page.FindCamstarControl("ResourcePartsPanel") as JQDataGrid; } }

        //------------------------
        // Fetch Grid Data
        //------------------------
        public void FetchGridDataMaterialList()
        {
            //Initialize Service & Objects
            UserProfile profile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
            PartTxnService Svc = new PartTxnService(profile);
            PartTxn SvcData = new PartTxn();
            PartTxn_Info SvcInfo = new PartTxn_Info();
            PartTxn_Request ReqData = new PartTxn_Request();
            PartTxn_Result ResData = new PartTxn_Result();

            //Set Input Data
            SvcData.Resource = new NamedObjectRef();
            if (_txtSelectedResource.Data != null)
                SvcData.Resource.Name = _txtSelectedResource.Data.ToString();

            //Set Request Value
            SvcInfo.ResourceParts = new ResourcePart_Info();
            SvcInfo.ResourceParts.PartName = new Info(true);
            SvcInfo.ResourceParts.PartQty = new Info(true);
            SvcInfo.ResourceParts.MaterialPart = new Info(true);
            ReqData.Info = SvcInfo;

            //Execute Request
            ResultStatus Results = Svc.Load(SvcData, ReqData, out ResData);
            if (Results.IsSuccess)
            {
                _gridResourcePartsPanel.Data = ResData.Value.ResourceParts;
                _gridResourcePartsPanel.OriginalData = ResData.Value.ResourceParts;
            }
        }

        //------------------------
        // Get Selected Resource
        //------------------------
        private void GetSelectedResource()
        {
            if (Page.PortalContext.DataContract.GetValueByName("ResourceParts_PartRequestMain") != null)
            {
                string SelectedResource = Page.PortalContext.DataContract.GetValueByName("ResourceParts_PartRequestMain").ToString();
                _txtSelectedResource.Data = SelectedResource;
            }
            else if (Page.PortalContext.DataContract.GetValueByName("SelectedResource_ResourceParts") != null)
            {
                string SelectedResource = Page.PortalContext.DataContract.GetValueByName("SelectedResource_ResourceParts").ToString();
                _txtSelectedResource.Data = SelectedResource;
            }

            if (Page.PortalContext.DataContract.GetValueByName("IsPopUp_ResourceParts") == null)
            {
                _gridResourcePartsPanel.BoundContext.Width = 580;
                this.Page.ActionDispatcher.PageActions().Where(act => act.Name == "CloseButton").FirstOrDefault().IsHidden = true;
            }
        }

        //------------------------
        // WebPartCustomAction handler
        //------------------------
        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as CustomAction;
            if (action != null)
            {
                if (action.Parameters == "ClosePop")
                {
                    ScriptManager.RegisterStartupScript(Page.Form, GetType(), "CloseButton", "window.parent.__page._isTabContainerPage=null;window.parent.CloseFloatingFrame(true);", true);
                }
            }
        }

        //------------------------
        // OnLoad event
        //------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            GetSelectedResource();
        }

        //------------------------
        // OnPreRender event
        //------------------------
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            FetchGridDataMaterialList();
        }
    }
}



