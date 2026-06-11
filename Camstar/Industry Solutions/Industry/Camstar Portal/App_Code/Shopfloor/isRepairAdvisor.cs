// © 2022 Siemens Product Lifecycle Management Software Inc.
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

using Camstar.WebPortal.FormsFramework.WebGridControls;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.Utilities;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.Utilities;
using PERS = Camstar.WebPortal.Personalization;
using System.Web.UI;
//using static Camstar.WebPortal.WebPortlets.Shopfloor.isDefect;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    /// <summary>
    /// Summary description for isRepairAdvisor
    /// </summary>
    public class isRepairAdvisor : MatrixWebPart
    {
        public isRepairAdvisor()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        #region Properties

        const string DC_DEFECT_JSON = "SelectedDefectJsonDM";
        const string DC_CONPROD_JSON = "ContainerProductIdDM";
        bool EnableRepairButton = false;

        protected virtual CWC.TextBox DefectReasonName { get { return Page.FindCamstarControl("DefectReasonName") as CWC.TextBox; } }
        protected virtual CWC.TextBox DefectReasonId { get { return Page.FindCamstarControl("DefectReasonId") as CWC.TextBox; } }
        protected virtual CWC.TextBox ContainerProductId { get { return Page.FindCamstarControl("ContainerProductId") as CWC.TextBox; } }
        protected virtual CWC.TextBox RefDes { get { return Page.FindCamstarControl("RefDes") as CWC.TextBox; } }
        protected virtual CWC.TextBox CurrentDefectsIDString { get { return Page.FindCamstarControl("CurrentDefectsIDString") as CWC.TextBox; } }
        protected virtual CWC.TextBox X { get { return Page.FindCamstarControl("X") as CWC.TextBox; } }
        protected virtual CWC.TextBox Y { get { return Page.FindCamstarControl("Y") as CWC.TextBox; } }
        protected virtual CWC.TextBox SelectedDefectJson { get { return Page.FindCamstarControl("SelectedDefectJson") as CWC.TextBox; } }
        protected virtual CWC.TextBox SelectedRepairActionNames { get { return Page.FindCamstarControl("SelectedRepairActionNames") as CWC.TextBox; } }
        protected virtual JQDataGrid RepairActionCountsGrid { get { return Page.FindCamstarControl("RepairActionCountsGrid") as JQDataGrid; } }
        protected virtual CWC.Button RepairButton { get { return Page.FindCamstarControl("RepairButton") as CWC.Button; } }
        protected virtual CWC.Button OpenRepairPopupButton { get { return Page.FindCamstarControl("OpenRepairPopup") as CWC.Button; } }
        protected virtual MatrixWebPart RepairAdvisorWP { get { return Page.FindCamstarControl("RepairAdvisorWP") as MatrixWebPart; } }
        #endregion

        protected override void OnLoad(System.EventArgs e)
        {
            base.OnLoad(e);

            //TODO: initial setting of a control's data by data contract doesn't seem to trigger a data changed.
            //      but we want to make sure the JSON field is set before setting other data from it.
            //      so doing by this test and it works OK, but would be nice to figure out 
            //      correct set of dependencies so could do it in a data changed handler or something like that.
            if (Page.DataContract.GetValueByName(DC_DEFECT_JSON) != null)
            {
                SetQueryParamsFromJsonData(Page.DataContract.GetValueByName(DC_DEFECT_JSON) as string);
            }

            RepairActionCountsGrid.RowSelected += new JQGridEventHandler(RepairActionCountsGrid_RowSelected);
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            RepairButton.Enabled = EnableRepairButton;
        }

        private ResponseData RepairActionCountsGrid_RowSelected(object sender, JQGridEventArgs args)
        {
            var rowIDs = RepairActionCountsGrid.GridContext.SelectedRowIDs;
            if (rowIDs != null)
            {
                EnableRepairButton = rowIDs.Count > 0;
            }

            return args.Response;
        }

        private void RepairActionCountsSelected()
        {
            string actionList = string.Empty;

            var rowIDs = RepairActionCountsGrid.GridContext.SelectedRowIDs;
            if (rowIDs != null)
            {
                // build list of action names each time user selects one.
                foreach (string name in rowIDs)
                {
                    if (string.IsNullOrEmpty(actionList))
                        actionList = name;
                    else
                        actionList += (";" + name);
                }
            }

            SelectedRepairActionNames.Data = actionList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public void SetQueryParamsFromJsonData(string defectJson)
        {
            // Deserialize to object array
            CurrentDefect[] defectsToRepair = CurrentDefect.DeserializeDefects((string)defectJson);

            // must be only one selected defect. use it to set query params
            var selectedDefect = defectsToRepair[0];

            if (Page.DataContract.GetValueByName(DC_CONPROD_JSON) != null)
                ContainerProductId.Data = Page.DataContract.GetValueByName(DC_CONPROD_JSON);
            DefectReasonName.Data = selectedDefect.isDefectReasonName; //obsolete, using Id
            DefectReasonId.Data = selectedDefect.isDefectReasonId;
            RefDes.Data = selectedDefect.isRefDes;
            CurrentDefectsIDString.Data = selectedDefect.isCurrentDefectsIDString;
            if (string.IsNullOrEmpty(selectedDefect.isRefDes))
            {
                X.Data = selectedDefect.isX;
                Y.Data = selectedDefect.isY;
            }
            else
            {
                X.Data = string.Empty;
                Y.Data = string.Empty;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            //TODO: update this to handle opening the Repair dialog
            base.WebPartCustomAction(sender, e);
            var action = e.Action as PERS.CustomAction;
            if (action != null)
            {
                switch (action.Parameters)
                {

                    case "repair":
                        {
                            RepairActionCountsSelected();
                            Page.CloseFloatingFrameOnSubmit(new OM.ResultStatus());
                            break;
                        }
                    case "cancel":
                        {
                            Page.CloseFloatingFrame(false);
                            break;
                        }

                }
            }
        }

        protected override IEnumerable<ScriptReference> GetScriptReferences()
        {
            yield return new ScriptReference("~/Scripts/user/Industry Solutions/isRepairAdvisor.js");
        }
    }
}