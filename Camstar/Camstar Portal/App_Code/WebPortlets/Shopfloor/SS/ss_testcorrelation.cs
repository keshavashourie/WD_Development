/* Copyright 2019 Siemens */
using System;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework.Utilities;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WCF.Services;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WCF.ObjectStack;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    /// <summary>
    /// Summary description for WIPEqpMaterialsSetup
    /// </summary>
    public class TestCorrelation : MatrixWebPart
    {
        #region Properties
        CWC.TextBox _txtComputerNameField { get { return Page.FindCamstarControl("TestCorrelation_ComputerName") as CWC.TextBox; } }
        CWC.NamedObject _ndoResourceField { get { return Page.FindCamstarControl("TestCorrelation_Resource") as CWC.NamedObject; } }
        CWC.CheckBox _chkRequireShiftCorrelation { get { return Page.FindCamstarControl("TestCorrelation_RequireShiftCorrelation") as CWC.CheckBox; } }
        
        #endregion


        #region Functions
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Get Computername
            _txtComputerNameField.TextControl.Text = SEMI.AppCode.UIUtility.GetComputerName(this);
            _ndoResourceField.DataChanged += _ndoResourceField_DataChanged;
        }

		void _ndoResourceField_DataChanged(object sender, EventArgs e)
		{
			if (_ndoResourceField.Data == null)
				Page.ShopfloorReset(sender, e as Personalization.CustomActionEventArgs);
			else
			{
				_ndoResourceField.DisableValueClearing = true;
				Page.ShopfloorReset(sender, e as Personalization.CustomActionEventArgs);
				_ndoResourceField.DisableValueClearing = false;

				Result rslt = GetTestCorrelationData(PrimaryServiceType);
				if (rslt != null)
					Page.DisplayValues(rslt.Value as OM.Service);
			}
		}

        protected Result GetTestCorrelationData(string pService)
        {
            Camstar.WCF.ObjectStack.UserProfile profile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;         
            ResultStatus res = new ResultStatus(null, false);
            var cdo = WCFObject.CreateObject(pService) as ICreator;
            var request = WCFObject.CreateObject(pService + "_Request") as ICreator;
            var reqInfo = WCFObject.CreateObject(pService + "_Info") as ICreator;
            Result result = null;
            var service = new WSDataCreator().CreateService(pService, profile);

            //set data input
            cdo.SetValue("Resource", _ndoResourceField.Data as NamedObjectRef);

            reqInfo.SetValue("CorrelationFlag", new OM.Info(true));
            reqInfo.SetValue("LastCorrelationDate", new OM.Info(true));
            reqInfo.SetValue("LastCorrelationLotName", new OM.Info(true));
            reqInfo.SetValue("RequireShiftCorrelation", new OM.Info(true));
            reqInfo.SetValue("RequireTimeCorrelation", new OM.Info(true));
            reqInfo.SetValue("RequireSetupCorrelation", new OM.Info(true));
            reqInfo.SetValue("LastCorrelationShift", new OM.Info(true));
            reqInfo.SetValue("CorrelationShiftsMissed", new OM.Info(true));
            reqInfo.SetValue("TimeCorrelationLimit", new OM.Info(true));
            reqInfo.SetValue("TimeCorrelationTimeframe", new OM.Info(true));
            reqInfo.SetValue("GoodUnits", new OM.Info(true));
            
            request.SetValue("Info", reqInfo);

            ResultStatus rslt = service.GetEnvironment(cdo as DCObject, request as Request, out result);

            
            if (rslt.IsSuccess)
                return result;
            else
                return null;
        }

        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as Camstar.WebPortal.Personalization.CustomAction;

            if (action != null && action.Parameters == "Reset")
            {
                Page.ShopfloorReset(sender, e);
            }
        }    
    }
}
        #endregion



