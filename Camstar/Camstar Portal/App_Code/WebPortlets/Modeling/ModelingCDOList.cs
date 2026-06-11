// Copyright Siemens 2023  
using System;
using System.Web;
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.PortalFramework;

namespace Camstar.WebPortal.WebPortlets.Modeling
{
    /// <summary>
    /// Summary description for ModelingCDOList
    /// </summary>
    public class ModelingCDOList : MatrixWebPart
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!Page.IsPostBack)
                // check for packages each time Modeling page is opened.
				Page.SessionDataContract.SetValueByName(DataMemberConstants.HasCMPackage, null);
            
            CDOList.CMSetDefaultClick += CDOList_CMSetDefaultClick;
            CDOList.DisplayChangePkgSection = CheckChgPackageLicense();
        }

        protected virtual void CDOList_CMSetDefaultClick(object sender, EventArgs e)
        {
            var action = CMSetDefaultBtn.DefaultAction;
            Page.ActionDispatcher.ExecuteAction(action);
        }

        /// <summary>
        /// Checks if a customer is licensed for MPCM or not. Temp solution for now.
        /// </summary>
        /// <returns></returns>
        protected virtual bool CheckChgPackageLicense()
        {
            var hasCMPackage = Page.SessionDataContract.GetValueByName(DataMemberConstants.HasCMPackage);
            if (hasCMPackage != null)
                return Convert.ToBoolean(hasCMPackage);

            hasCMPackage = false;
            var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            if (session != null)
            {
                var service = new ChangePackageModelingInquiryService(session.CurrentUserProfile);
                var serviceData = new ChangePackageModelingInquiry();
                var request = new ChangePackageModelingInquiry_Request()
                {
                    Info = new ChangePackageModelingInquiry_Info()
                    {
                        HasChangePackage = new Info(true)
                    }
                };
                ChangePackageModelingInquiry_Result result;
                var resultStatus = service.ExecuteTransaction(serviceData, request, out result);

                if (resultStatus.IsSuccess)
                {
                    if (result.Value != null && result.Value.HasChangePackage != null)
                        hasCMPackage = (bool)result.Value.HasChangePackage;
                }
                //NOTE: Intentionally not displaying any error message here in the case of a normal Modeling user not have the license or role for this service
                
            }
            Page.SessionDataContract.SetValueByName(DataMemberConstants.HasCMPackage, hasCMPackage);
            return (bool)hasCMPackage;
        }

        protected virtual Button CMSetDefaultBtn
        {
            get { return Page.FindCamstarControl("CMSetDefaultBtn") as Button; }
        }

        protected virtual CamstarPortal.WebControls.ModelingObjectList CDOList
        {
            get { return Page.FindCamstarControl("CDOList") as CamstarPortal.WebControls.ModelingObjectList; }
        }
    }
}
