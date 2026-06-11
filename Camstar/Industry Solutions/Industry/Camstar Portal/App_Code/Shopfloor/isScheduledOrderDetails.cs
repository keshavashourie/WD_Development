// © 2018 Siemens Product Lifecycle Management Software Inc.
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using Camstar.WCF.Services;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.FormsFramework.Utilities;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.WCFUtilities;

/// <summary>
/// Update details of a scheduled order
/// </summary>
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class isScheduledOrderDetails : MatrixWebPart
    {
        protected virtual CWC.TextBox _comments { get { return Page.FindCamstarControl("Comments") as CWC.TextBox; } }
        protected virtual CWC.TextBox _preactorScheduledOrdersId { get { return Page.FindCamstarControl("isPreactorScheduledOrdersId") as CWC.TextBox; } }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if(!Page.IsPostBack)
                LoadComments();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);

             var action = e.Action as CustomAction;
             if (action != null)
             {
                 switch (action.Parameters)
                 {
                     case "Submit":
                         {
                             UpdateComments();
                             break;
                         }
                 }
             }
        }

        /// <summary>
        /// Write new comments to isPreactorScheduledOrders record
        /// </summary>
        protected void UpdateComments()
        {
            var fs = FrameworkManagerUtil.GetFrameworkSession();
            UserProfile profile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;

            isOrderChangeComments cdo = new isOrderChangeComments()
            {
                isComments = _comments.Data as string,
                isPreactorScheduledOrdersId = _preactorScheduledOrdersId.Data as string
            };
            isOrderChangeCommentsService changeCommentsSvc = new isOrderChangeCommentsService(profile);
            ResultStatus result = changeCommentsSvc.ExecuteTransaction(cdo);
            DisplayMessage(result);
            if (result.IsSuccess)
                Page.CloseFloatingFrame(true);
        }

        /// <summary>
        /// Load comments associated with selected Preactor Scheduled Order and show on page
        /// </summary>
        protected void LoadComments()
        {
            var fs = FrameworkManagerUtil.GetFrameworkSession();
            UserProfile profile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;

            isOrderChangeComments cdo = new isOrderChangeComments()
            {
                isPreactorScheduledOrdersId = _preactorScheduledOrdersId.Data as string
            };
            
            isOrderChangeComments_Request request = new isOrderChangeComments_Request
            {
                Info = new isOrderChangeComments_Info
                {
                    isComments = new Info(true)
                }
            };

            isOrderChangeCommentsService changeCommentsSvc = new isOrderChangeCommentsService(profile);
            isOrderChangeComments_Result result;
            ResultStatus resultStatus = changeCommentsSvc.Load(cdo, request, out result);
            if(resultStatus.IsSuccess)
            {
                _comments.Data = (result.Value.isComments == null) ? "" : result.Value.isComments.Value;
            }
            else
            {
                DisplayMessage(resultStatus);
            }
        }
    }
}