using System;
using System.Web;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Web.UI;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.WebPortlets;
using Camstar.WCF.Services;
using OM = Camstar.WCF.ObjectStack;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;


namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class HVResourceSetup : MatrixWebPart
    {
        protected CWC.NamedObject HiddenSelectedResource { get { return Page.FindCamstarControl("HiddenSelectedResource") as CWC.NamedObject; } }
        protected CWC.NamedObject ResourceDropDown { get { return Page.FindCamstarControl("Resource") as CWC.NamedObject; } }
        protected JQDataGrid ServiceDetailsGrid { get { return Page.FindCamstarControl("ServiceDetails") as JQDataGrid; } }
        protected MatrixWebPart detailsWP { get { return Page.FindCamstarControl("HVResourceSetupWP") as MatrixWebPart; } }

        public HVResourceSetup()
        {
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            HiddenSelectedResource.DataChanged += HiddenSelectedResource_DataChanged;
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            //if (!Page.ClientScript.IsStartupScriptRegistered("ShowResourceSearchPanel"))
            //{
            //    ScriptManager.RegisterStartupScript(Page.Form, Page.Form.GetType(), "ShowResourceSearchPanel", "ShowResourceSearchPanel()", true);
            //}
        }

        protected virtual void HiddenSelectedResource_DataChanged(object sender, EventArgs e)
        {
            if(ValidateResource())
                LoadSetupDetailsGrid();
        }

        protected virtual bool ValidateResource() {
            bool valid = true;

            if (HiddenSelectedResource.Data != null)
            {

                var svcParams = new OM.ResourceMaint()
                {
                    ObjectToChange = new OM.NamedObjectRef()
                    {
                        Name = HiddenSelectedResource.Data.ToString()

                    }
                };

                var request = new ResourceMaint_Request()
                {
                    Info = new OM.ResourceMaint_Info
                    {
                        RequestValue = true,
                        ObjectToChange = new OM.Info(true),
                        ObjectChanges = new OM.ResourceChanges_Info()
                        {
                            RequestValue = true,
                            UseHVTraceability = new OM.Info(true)
                        }
                    }
                };

                var result = new ResourceMaint_Result();
                FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
                var service = new ResourceMaintService(session.CurrentUserProfile);
                OM.ResultStatus resultStatus = service.Load(svcParams, request, out result);
                if (resultStatus.IsSuccess)
                {
                    if (result.Value.ObjectChanges.UseHVTraceability != true)
                    {
                        string message = "Resource is not configured to use High Volume Traceability.";
                        string labelValue;

                        LabelCache labelCache = FrameworkManagerUtil.GetLabelCache(Page.Session);
                        if (labelCache != null)
                        {
                            labelValue = labelCache.GetLabelByName("HVResourceSetup_ResourceNotConfigured").Value;
                            message = labelValue.Replace("#ErrorMsg.Name", HiddenSelectedResource.Data.ToString());
                        }

                        ResourceDropDown.Data = null;
                        HiddenSelectedResource.Data = null;
                        Page.StatusBar.WriteError(message);
                    }
                }
                else
                {
                    ResourceDropDown.Data = null;
                    HiddenSelectedResource.Data = null;
                    Page.StatusBar.WriteError(resultStatus.ToString());
                }
            }

            return valid;
        }

        protected virtual void LoadSetupDetailsGrid()
        {
            try
            {
                if (HiddenSelectedResource.Data != null)
                {
                    var currentUserProfile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
                    QueryService service = new QueryService(currentUserProfile);
                    OM.QueryOptions options = new OM.QueryOptions();
                    OM.RecordSet data = new OM.RecordSet();

                    OM.QueryParameters queryParamsObj = new OM.QueryParameters();
                    OM.QueryParameter[] queryParams = new OM.QueryParameter[1]
                    {
                    new OM.QueryParameter("ResourceName", HiddenSelectedResource.Data.ToString())
                    };
                    queryParamsObj.Parameters = queryParams;

                    string query = "GetCurrentHVResourceSetupDetails";
                    OM.ResultStatus resStatus = service.Execute(query, queryParamsObj, options, out data);
                    if (resStatus.IsSuccess)
                    {
                        DataTable dt = data.GetAsDataTable();
                        List<OM.HVResourceSetupDetail> details = new List<OM.HVResourceSetupDetail>();
                        for (int row = 0; row < dt.Rows.Count; row++)
                        {
                            var detail = new OM.HVResourceSetupDetail();
                            detail.CompId = dt.Rows[row].Field<string>("CompId");
                            detail.CompName = dt.Rows[row].Field<string>("CompName");
                            detail.LotNo = dt.Rows[row].Field<string>("LotNo");
                            if (!string.IsNullOrWhiteSpace(dt.Rows[row].Field<string>("Slot")))
                                detail.Slot = Convert.ToInt32(dt.Rows[row].Field<string>("Slot"));
                            if(!string.IsNullOrWhiteSpace(dt.Rows[row].Field<string>("SubSlot")))
                                detail.SubSlot = Convert.ToInt32(dt.Rows[row].Field<string>("SubSlot"));
                            details.Add(detail);
                        }

                        // works
                        ServiceDetailsGrid.ClearData();
                        ServiceDetailsGrid.Data = details.ToArray();
                    }

                }
            }
            catch (Exception ex)
            {
                Page.DisplayMessage(ex.InnerException != null ? ex.InnerException.Message : ex.Message, false);
            }
        }

    }
}