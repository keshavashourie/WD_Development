//© 2022 Siemens Product Lifecycle Management Software Inc.
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
using OM = Camstar.WCF.ObjectStack;
using System.Text;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{

    /// <summary>
    /// TODO: Add a Summary description for this Camstar Web Part
    /// </summary>
    public class scsOnlineTravelerR2Panel : MatrixWebPart
    {
        #region Controls

        protected virtual CWC.TextBox _txtSelectionId { get { return Page.FindCamstarControl("OnlineTraveler_SelectionId") as CWC.TextBox; } }

        protected virtual CWC.PagePanel LotDetails
        {
            get { return FindCamstarControl("LotDetailsPanel") as CWC.PagePanel; }
        } 

        #endregion

        #region Protected Functions

        /// <summary>
        /// TODO: Summary Description of function
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _txtSelectionId.DataChanged += delegate { LoadDependentControls(); };

            if (!Page.IsPostBack)
            {
                if (_txtSelectionId.Data != null)
                    LoadDependentControls();
            }
        }
        #endregion

        #region Public Functions

        #endregion

        #region Private Functions
        private void LoadDependentControls()
        {
            if (_txtSelectionId == null)
                throw new ApplicationException("The control is not found");

            if (_txtSelectionId.Data != null)
            {
                Page.StatusBar.ClearMessage();
                OM.ViewContainerStatus inputData = new OM.ViewContainerStatus { Container = new OM.ContainerRef(_txtSelectionId.Data.ToString()) };
                OM.ViewContainerStatus_Info info = new OM.ViewContainerStatus_Info
                {
                    Workflow = FieldInfoUtil.RequestValue(),
                    Step = FieldInfoUtil.RequestValue(),
                    NextStep = FieldInfoUtil.RequestValue(),
                    Owner = FieldInfoUtil.RequestValue(),
                    ProcessSpecObjectType = FieldInfoUtil.RequestValue(),
                    ProcessSpec = FieldInfoUtil.RequestValue(),
                    ProcessSpecRevision = FieldInfoUtil.RequestValue(),
                    Product = FieldInfoUtil.RequestValue(),
                    WIPStatus = FieldInfoUtil.RequestValue(),
                    WIPType = FieldInfoUtil.RequestValue(),
                    WIPYieldResult = FieldInfoUtil.RequestValue(),
                    Qty = FieldInfoUtil.RequestValue(),
                    Qty2 = FieldInfoUtil.RequestValue()
                };
                OM.UserProfile profile = HttpContext.Current.Session[Camstar.WebPortal.Constants.SessionConstants.UserProfile] as OM.UserProfile;
                ViewContainerStatusService serv = new ViewContainerStatusService(profile);
                ViewContainerStatus_Result result = null;
                OM.ResultStatus resultStatus = serv.ExecuteTransaction(inputData, new ViewContainerStatus_Request { Info = info }, out result);
                if (resultStatus.IsSuccess)
                {
                    var PanelNameValueData = new Dictionary<string, object>();
                    PanelNameValueData.Add("Product", getValue(result.Value.Product));
                    PanelNameValueData.Add("Workflow", getValue(result.Value.Workflow));
                    PanelNameValueData.Add("Step", getValue(result.Value.Step));
                    PanelNameValueData.Add("Qty", getValue(result.Value.Qty));
                    PanelNameValueData.Add("Qty2", getValue(result.Value.Qty2));
                    PanelNameValueData.Add("Owner", getValue(result.Value.Owner));
                    PanelNameValueData.Add("WIP Status", getValue(result.Value.WIPStatus));
                    PanelNameValueData.Add("WIP Type", getValue(result.Value.WIPType));
                    PanelNameValueData.Add("WIP Yield Result", getValue(result.Value.WIPYieldResult));
                    PanelNameValueData.Add("Process Spec Object", getValue(result.Value.ProcessSpecObjectType));
                    PanelNameValueData.Add("Process Spec", getValue(result.Value.ProcessSpec));
                    PanelNameValueData.Add("Process Spec Revision", getValue(result.Value.ProcessSpecRevision));

                    StringBuilder htmlText = new StringBuilder("<div class='content'><span class='title'>Lot Details</span><div class=content-tbl>");
                    string id = "ctl00_WebPartManager_BlankWP_OnlineTraveler_"; 
                    foreach (string key in PanelNameValueData.Keys)
                    {
                        htmlText.AppendLine(string.Format("<div class='content-row'><span class='name'>{0}</span><span id='" + id + String.Concat(key.Where(c => !Char.IsWhiteSpace(c))) + "' class='val'>{1}</span></div>", key, PanelNameValueData[key].ToString()));
                    }
                    htmlText.AppendLine("</div></div>");
                    htmlText.AppendLine(@"<input type='submit' id='online_traveler_reset_button' onclick=""$('#ctl00_WebPartManager_BlankWP_OnlineTraveler_SelectionId_ctl00').val('');"" title='Reset' class='cs-button-secondary' value='Reset' style='float: right;'>");
                    LotDetails.Controls.Clear(); 
                    LotDetails.Controls.Add(new LiteralControl(htmlText.ToString()));
                    LotDetails.Visible = true;
                    CamstarWebControl.SetRenderToClient(LotDetails);
                }
                else
                {
                    LotDetails.Controls.Clear();
                    LotDetails.Visible = false;
                    _txtSelectionId.ClearData();
                    this.DisplayMessage(resultStatus);
                }

            }
            else
            {
                LotDetails.Controls.Clear();
                LotDetails.Visible = false;
                _txtSelectionId.ClearData();
                Page.ShopfloorReset(null, null);
            }
        } // LoadDependentControls

        private string getValue(object Value)
        {
            if (Value != null)
            {
                return Value.ToString();
            }
            return "";
        }
        #endregion

        #region Constants

        #endregion

        #region Private Member Variables

        #endregion

    }

}

