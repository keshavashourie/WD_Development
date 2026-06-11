using System;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WebPortal.Utilities;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WCF.ObjectStack;
using System.Web;


namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
   
    public class ResourceSetup : MatrixWebPart
    {
        protected CWC.NamedObject _Resource { get { return Page.FindCamstarControl("Resource") as CWC.NamedObject; } }
        protected JQDataGrid _ResourceSetup_Tools { get { return Page.FindCamstarControl("ResourceSetup_Tools") as JQDataGrid; } }
        protected CWC.CheckBox _UpdateTools { get { return Page.FindCamstarControl("ResourceSetup_UpdateTools") as CWC.CheckBox; } }


        //-----------------------------------------
        //
        //-----------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (Page.IsPostBack)
                _Resource.DataChanged += delegate { LoadDependentControls(); };

        }

        //-----------------------------------------
        //
        //-----------------------------------------
        private void LoadDependentControls()
        {

            if (_Resource == null)
                throw new ApplicationException("The control is not found");

            OM.ResourceSetup inputData = new OM.ResourceSetup { Resource = _Resource.Data as NamedObjectRef };
            ResourceStatusDetails_Info statusDetailsInfo = new ResourceStatusDetails_Info
            {
                Tools = FieldInfoUtil.RequestValue(),

            };
            ResourceSetup_Info info = new ResourceSetup_Info
            {
                ResourceStatusDetails = statusDetailsInfo
            };

            UserProfile profile = HttpContext.Current.Session[Camstar.WebPortal.Constants.SessionConstants.UserProfile] as UserProfile;
            ResourceSetupService serv = new ResourceSetupService(profile);
            ResourceSetup_Result result = null;
            ResultStatus resultStatus = serv.GetEnvironment(inputData, new ResourceSetup_Request { Info = info }, out result);
            if (resultStatus.IsSuccess)
            {
                if (result.Value.ResourceStatusDetails != null)
                {
                    ResourceStatusDetails resourceStatus = result.Value.ResourceStatusDetails;

                    // tools
                    if (resourceStatus.Tools != null)
                    {

                        NamedObjectRef[] tools = new NamedObjectRef[resourceStatus.Tools.Length];
                        int intToolsIndex = 0;

                        foreach (NamedObjectRef tool in resourceStatus.Tools)
                        {
                            tools[intToolsIndex] = new NamedObjectRef();
                            tools[intToolsIndex].Name = tool.Name;
                            intToolsIndex++;
                        }

                        (_ResourceSetup_Tools.GridContext as ItemDataContext).Data = tools;
                        _ResourceSetup_Tools.BoundContext.LoadData();
                        //_UpdateTools.CheckControl.Checked = true;
                    }
                    else
                    {
                        _ResourceSetup_Tools.OriginalData = null;
                        _ResourceSetup_Tools.ClearData();
                        //_UpdateTools.CheckControl.Checked = false;
                    }

                }
            }


        } // LoadDependentControls


    }
}