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
using Camstar.WCF.ObjectStack;
using System.Text;

namespace Camstar.WebPortal.WebPortlets.DataTransfer
{

    /// <summary>
    /// TODO: Add a Summary description for this Camstar Web Part
    /// </summary>
    public class OverrideActivateChangePkg : MatrixWebPart
    {
        #region Controls
        protected virtual CWC.TextBox OverrideCommentsTxt { get { return Page.FindCamstarControl("OverrideCommentsTxt") as CWC.TextBox; } }
        protected virtual CWC.Label OverrideCommentsLbl { get { return Page.FindCamstarControl("OverrideCommentsLbl") as CWC.Label; } }
        #endregion

        #region Protected Override Functions

        /// <summary>
        /// TODO: Summary Description of function
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);


            if (OverrideCommentsLbl != null)
            {
                var svc = new ActivationInquiryService(FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);
                var pkgName = Page.PortalContext.DataContract.GetValueByName<string>(_changePkg);
                var svcDet = new ActivationInquiry()
                {
                    PackageName = pkgName
                };
                var req = new ActivationInquiry_Request
                {
                    Info = new ActivationInquiry_Info
                    {
                        PackageDetails = new ActivationInquiryDetail_Info
                        {
                            ChangePackage = new ChangePackage_Info
                            {
                                PreReqChangePkgs = new Info(true)
                            }
                        }
                    }
                };
                var res = new ActivationInquiry_Result();
                var result = svc.GetPackages(svcDet, req, out res);
                if (result.IsSuccess)
                {
                    if (res.Value != null && res.Value.PackageDetails != null && res.Value.PackageDetails.Length > 0)
                    {
                        StringBuilder sb = new StringBuilder();
                        var pkg = res.Value.PackageDetails[0];
                        if (pkg.ChangePackage.PreReqChangePkgs.Length > 3)
                        {
                            for (var i = 0; i < 3; i++)
                            {
                                if (i < 2)
                                {
                                    sb.Append(pkg.ChangePackage.PreReqChangePkgs[i].Value + ",");
                                }
                                else
                                {
                                    sb.Append(pkg.ChangePackage.PreReqChangePkgs[i].Value + "..");
                                }
                            }
                        }
                        else if (pkg.ChangePackage.PreReqChangePkgs.Length == 3)
                        {
                            for (var i = 0; i < 3; i++)
                            {
                                if (i < 2)
                                {
                                    sb.Append(pkg.ChangePackage.PreReqChangePkgs[i].Value + ",");
                                }
                                else
                                {
                                    sb.Append(pkg.ChangePackage.PreReqChangePkgs[i].Value);
                                }
                            }
                        }
                        else
                        {
                            for (var i = 0; i < pkg.ChangePackage.PreReqChangePkgs.Length; i++)
                            {
                                if (i < pkg.ChangePackage.PreReqChangePkgs.Length - 1)
                                {
                                    sb.Append(pkg.ChangePackage.PreReqChangePkgs[i].Value + ",");
                                }
                                else
                                {
                                    sb.Append(pkg.ChangePackage.PreReqChangePkgs[i].Value);
                                }
                            }
                        }
                        var overrideLblText = string.Format(OverrideCommentsLbl.Text, sb.ToString());
                        OverrideCommentsLbl.Text = overrideLblText;
                    }
                }
            }
        }
        #endregion

        #region Public Override Functions                  
        public override void WebPartCustomAction(object sender, CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            if (e.Action.CommandName == "Override")
            {
                if (OverrideCommentsTxt != null)
                {
                    Page.PortalContext.DataContract.SetValueByName(_overrideExecuted, true);
                    Page.PortalContext.DataContract.SetValueByName(_overrideComments, OverrideCommentsTxt.Data != null ? OverrideCommentsTxt.Data.ToString() : string.Empty);
                }
                ScriptManager.RegisterStartupScript(Page.Form, GetType(), "OverridePreReq", "window.parent.CloseFloatingFrame(true, true);", true);
            }
            else if (e.Action.CommandName == "Cancel")
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("if (typeof(__page) !== 'undefined') {");
                sb.AppendLine("setTimeout(function(){ window.parent.CloseFloatingFrame(false); }, 1);");
                sb.AppendLine("__page.closeTab('');");
                sb.AppendLine("}");
                ScriptManager.RegisterStartupScript(Page.Form, GetType(), "CancelOverride", sb.ToString(), true);
            }
        }
        #endregion

        #region Private Functions

        #endregion

        #region Constants
        private const string _overrideComments = "OverrideComments";
        private const string _overrideExecuted = "OverrideExecuted";
        private const string _changePkg = "ChangePkg";
        #endregion

        #region Private Member Variables

        #endregion

    }

}

