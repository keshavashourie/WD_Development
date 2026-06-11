using System;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.PortalFramework;

/// <summary>
/// Summary description for ProductionEventResolution
/// </summary>

namespace Camstar.WebPortal.WebPortlets
{
    public class ProductionEventResolution : MatrixWebPart
    {
        #region Properties
        protected virtual JQTabContainer TabContainer
        {
            get { return Page.FindCamstarControl("Tabs") as JQTabContainer; }
        }

        protected virtual FormsFramework.WebControls.DropDownList QualityObjectDetail_Status
        {
            get { return Page.FindCamstarControl("QualityObjectDetail_Status") as FormsFramework.WebControls.DropDownList; }
        }

        protected virtual FormsFramework.WebControls.NamedObject UpdateEventData_QualityResolutionCode
        {
            get { return Page.FindCamstarControl("UpdateEventData_QualityResolutionCode") as FormsFramework.WebControls.NamedObject; }
        }

        protected virtual FormsFramework.WebControls.TextBox UpdateEventData_CloseDescription
        {
            get { return Page.FindCamstarControl("UpdateEventData_CloseDescription") as FormsFramework.WebControls.TextBox; }
        }

        protected virtual FormsFramework.WebControls.NamedObject QualityObjectDetail_CmpltRoutingResolutionCode
        {
            get { return Page.FindCamstarControl("QualityObjectDetail_CmpltRoutingResolutionCode") as FormsFramework.WebControls.NamedObject; }
        }

        protected virtual FormsFramework.WebControls.TextBox QualityObjectDetail_CmpltRoutingCloseDescription
        {
            get { return Page.FindCamstarControl("QualityObjectDetail_CmpltRoutingCloseDescription") as FormsFramework.WebControls.TextBox; }
        }
        #endregion

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (TabContainer.SelectedItem.Name == "Resolution")
                LoadEventResolution();
        }

        protected virtual void LoadEventResolution()
        {
            var status = (QualityStatusEnum)QualityObjectDetail_Status.Data;
            if (status == QualityStatusEnum.InReview)
            {
                UpdateEventData_QualityResolutionCode.Visible = false;
                UpdateEventData_CloseDescription.Visible = false;
                QualityObjectDetail_CmpltRoutingResolutionCode.Visible = true;
                QualityObjectDetail_CmpltRoutingCloseDescription.Visible = true;
            }
        }
    }
}
