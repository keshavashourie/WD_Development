// Copyright Siemens 2020 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls.WebParts;
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.Personalization;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;

namespace Camstar.WebPortal.WebPortlets
{
    public class isIPLLocationObjectsWP : MatrixWebPart
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            ValorSwacWebPart.Style[HtmlTextWriterStyle.Display] = "none";
            if (!Page.IsPostBack)
            {
                LoadObjectTypesInfo();
            }

            ddlObjectType.DataChanged += DdlObjectType_DataChanged;
            var componentName = Page.DataContract.GetValueByName<string>("ComponentNameDM");
            btnGetValorItem.OnClientClick = string.Format("isMaterialRequestStatusBoard.setResource('{0}', '{1}', '{2}', '{3}')", ddlObjectType.ClientID, ddlInstance_NDO.ClientID, ValorSwacWebPart.ClientID, componentName);
        }

        private void BtnGetValorItem_Click(object sender, EventArgs e)
        {
            if (!ddlObjectType.IsEmpty && !ddlInstance_NDO.IsEmpty)
            {
                var instanceName = Uri.EscapeUriString(ddlInstance_NDO.Text);
                var assetNumber = string.Format("{0}-{1}", ddlObjectType.Data, ddlInstance_NDO.Data);
            }
        }

        private void DdlObjectType_DataChanged(object sender, EventArgs e)
        {
            LoadInstances(ddlObjectType.Data as string);
            ScriptManager.RegisterStartupScript(this, Page.GetType(), "displaySwacWP", string.Format("isMaterialRequestStatusBoard.displaySwacWP('{0}', false);", ValorSwacWebPart.ClientID), true);
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            ddlInstance_NDO.Visible = !ddlObjectType.IsEmpty;
        }

        protected virtual void LoadInstances(string CDODefId)
        {
            if (!string.IsNullOrEmpty(CDODefId))
            {
                var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
                var cdoServ = new CDOInquiryService(session.CurrentUserProfile);

                var cdo = new CDOInquiry
                {
                    IsRDO = false,
                    CDODefId = new Enumeration<MaintainableObjectEnum, string>(CDODefId)
                };
                var cdoReq = new CDOInquiry_Request
                {
                    Info = new CDOInquiry_Info { CDOFilteredInstances = new Info(false, true) }
                };
                CDOInquiry_Result cdoRes;
                var res = cdoServ.GetEnvironment(cdo, cdoReq, out cdoRes);
                if (res.IsSuccess)
                {
                    var instances = cdoRes.Environment.CDOFilteredInstances.SelectionValues;
                    if (instances != null && instances.Rows != null)
                        ddlInstance_NDO.SetSelectionValues(instances);
                    else
                        ddlInstance_NDO.ClearSelectionValues();
                }
                else
                    Page.DisplayMessage(res);
            }
            else
                ddlInstance_NDO.ClearSelectionValues();
        }

        protected virtual void LoadObjectTypesInfo()
        {
            var _cdoCache = new MaintCDOCache();
            var cdoCacheData = _cdoCache.MaintCdoData;
            if (cdoCacheData != null)
            {
                var ddlListValues = new List<CustomListValueMapItem>();
                foreach (var cdo in cdoCacheData.Where(cdo => SupportedObjectTypes.Contains(cdo.CDOName)))
                {
                    ddlListValues.Add(new CustomListValueMapItem {DisplayName = cdo.CDODisplayName, Value = cdo.CDODefID});
                }
                ddlObjectType.CustomListValues = ddlListValues.ToArray();
            }
        }

        protected virtual HashSet<string> SupportedObjectTypes
        {
            get { return new HashSet<string> {"isMaterialQueue", "isInventoryLocation"}; }
        }

        protected virtual CWC.DropDownList ddlObjectType
        {
            get { return Page.FindCamstarControl("ddlCdoType") as CWC.DropDownList; }
        }

        protected virtual CWC.DropDownList ddlInstance_NDO
        {
            get { return Page.FindCamstarControl("ddlInstance_NDO") as CWC.DropDownList; }
        }

        protected virtual CWC.Button btnGetValorItem
        {
            get { return Page.FindCamstarControl("btnGetValorItem") as CWC.Button; }
        }

        protected virtual WebPart ValorSwacWebPart
        {
            get { return Page.Manager.WebParts["SwacWP"]; }
        }
    }
}