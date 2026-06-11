using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework.Utilities;
using System.Data;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.FormsFramework;
//using Camstar.WebPortal.Helpers.ES;
using Newtonsoft.Json;

using OM = Camstar.WCF.ObjectStack;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;

using CGC = Camstar.WebPortal.FormsFramework.WebGridControls;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{

    /// <summary>
    /// Holds data from GetMfgOrderList query
    /// Helps to convert results to JSON for client side processing
    /// When overriding the query GetMfgOrderList, create a new class derived from this with the extended properties.
    /// </summary>
    public class isMfgOrderListItem : MfgOrderListItem
    {
        //public string PriorityCodeName;
        public string Recipe;
        public string RequiredRecipe;
        public string LineAssignment;
        public string ScheduledSequence;
        public string SMTSide;
        public string psoId;
        public string OperationId;
            //  isPreactorScheduledOrdersId for easy linking to row with comment
    }

    /// <summary>
    /// Implement behavior for MfgOperation_VP page
    /// </summary>
    public class isMfgOperation : MfgOperation
    {
        protected bool SchedulingEnabled { get; set; }

        #region Page Event Handlers
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            SetSchedulingEnabled();
        }
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            ScriptManager.RegisterStartupScript(this, this.GetType(), "initializeisMfgOperation", $"isMfgOperation.initialize({GetClientLabels()},{GetDisplayDetails()});", true);
        }


        protected void SetSchedulingEnabled()
        {
            SchedulingEnabled = false;

            var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var factoryMaint = Page.Service.GetService<FactoryMaintService>();

            var serviceData = new FactoryMaint();

            if (session.SessionValues != null && !String.IsNullOrEmpty(session.SessionValues.Factory))
            {
                serviceData.ObjectToChange = new NamedObjectRef(session.SessionValues.Factory);
                var request = new FactoryMaint_Request();
                var result = new FactoryMaint_Result();
                var resultStatus = new ResultStatus();

                request.Info = new FactoryMaint_Info
                {
                    RequestValue = true,
                    ObjectChanges = new FactoryChanges_Info
                    {
                        Name = new Info(true),
                        isPreactorEnabled = new Info(true),
                        isLocalSchedulingEnabled = new Info(true)
                    }
                };

                resultStatus = factoryMaint.Load(serviceData, request, out result);
                if (resultStatus.IsSuccess && result.Value.ObjectChanges != null)
                {
                    SchedulingEnabled = (bool)result.Value.ObjectChanges.isLocalSchedulingEnabled || (bool)result.Value.ObjectChanges.isPreactorEnabled;
                }
            }
        }
        #endregion Page Event Handlers

        #region virtual methods other workspaces may want to override
        /// <summary>
        /// Override this to change the query used for getting the Mfg Order List
        /// </summary>
        /// <returns></returns>
        protected override string GetMfgOrderListQuery()
        {
            return SchedulingEnabled ? "isGetMfgOrderListSchd" : "isGetMfgOrderList";
        }

        /// <summary>
        /// Override this to set data when using a class derived from MfgOrderListItem
        /// </summary>
        /// <param name="recordSet"></param>
        protected override string GetMfgOrderListJSON(RecordSet recordSet)
        {
            var ordList = new List<isMfgOrderListItem>();
            foreach (Row row in recordSet.Rows)
            {
                var item = new isMfgOrderListItem
                {
                    // set common fields
                    ProductDisplay = row.Values[0],
                    ProductDescription = row.Values[1],
                    MfgOrderName = row.Values[2],
                    ERPOperationName = row.Values[3],
                    ERPOperationNo = row.Values[4],
                    MfgOrderQty = string.IsNullOrWhiteSpace(row.Values[5]) ? 0 : Convert.ToInt64(row.Values[5]),
                    CustomerName = row.Values[6],
                    Product = row.Values[9],
                    MfgOrder = row.Values[10],
                    Spec = row.Values[11],
                    Workflow = row.Values[12],
                    WorkflowDisplay = row.Values[13],
                    WorkflowStepName = row.Values[14],
                    ERPOperation = row.Values[15],
                    PriorityCodeName = row.Values[16],
                    RouteStepId = row.Values[17],
                    RoutStepName = row.Values[18],
                    OrderStatus = row.Values[19],
                    WorkflowStepId = row.Values[20],
                    ProductName = row.Values[21],
                    ProductRevision = row.Values[22],
                    ProcessedQty = string.IsNullOrWhiteSpace(row.Values[23]) ? 0 : Convert.ToInt64(row.Values[23]),
                    SpecName = row.Values[24],
                    SpecRevision = row.Values[25],
                    OperationId = row.Values[32],
                    //  This is the Recipe returned by the isGetRecipeFromMatrix function
                    RequiredRecipe = row.Values[26]
                };
                item.RemainingQty = item.MfgOrderQty - item.ProcessedQty;
                item.UniqueId = item.MfgOrderName + "_" + item.WorkflowStepName;

                if (SchedulingEnabled)
                {
                    item.PlannedStartDate = GetDisplayTimeString(row.Values[7]);
                    item.PlannedCompletionDate = GetDisplayTimeString(row.Values[8]);

                    item.LineAssignment = row.Values[27];
                    item.ScheduledSequence = row.Values[28];
                    item.SMTSide = row.Values[29];
                    item.psoId = row.Values[30];
                    var recipeName = row.Values[34];
                    if (!string.IsNullOrEmpty(recipeName))
                        item.Recipe = string.Format("{0}:{1}", recipeName, row.Values[35]);
                }
                else
                {
                    item.PlannedStartDate = GetLocalTimeString(row.Values[7]);
                    item.PlannedCompletionDate = GetLocalTimeString(row.Values[8]);

                    item.LineAssignment = string.Empty;
                    item.ScheduledSequence = string.Empty;
                    item.SMTSide = string.Empty;
                    item.psoId = string.Empty;
                    var recipeName = row.Values[29];
                    if (!string.IsNullOrEmpty(recipeName))
                        item.Recipe = string.Format("{0}:{1}", recipeName, row.Values[30]);
                }

                ordList.Add(item);
            }

            TileContainer.ControlState.Tiles = new List<CWC.TileContainer.TileContext>();
            var tileColName = TileContainer.ControlState.Columns[0].Name;    // assuming we have only one column here. is there a use case for more than one? and what does that do?
            foreach (isMfgOrderListItem item in ordList)
            {
                TileContainer.ControlState.Tiles.Add(CreateTile(item, tileColName));
            }

            return new JavaScriptSerializer() { MaxJsonLength = Int32.MaxValue }.Serialize(ordList);
        }

        protected string GetDisplayTimeString(string dbTimeString)
        {
            string displayTimeString = string.Empty;

            if (!string.IsNullOrWhiteSpace(dbTimeString))
            {
                DateTime time = DateTime.Parse(dbTimeString);
                displayTimeString = time.ToString(); // TODO: I think this just uses server culture to convert. should we do anything else?
            }

            return displayTimeString;
        }

        /// <summary>
        /// Override this to control labels sent to client.
        /// </summary>
        protected override void SetLabelNames()
        {
            base.SetLabelNames();

            AddLabel("Recipe", "CSICDOName_Recipe");
            AddLabel("LineAssignment", "isLblMenuLineAssignment");
            AddLabel("SMTSide", "RouteStep_isSMTSide");
            AddLabel("RequiredRecipe", "RequiredRecipe");
            AddLabel("PartiallySatisfied", "Lbl_MaterialPartiallySatisfied");
            AddLabel("Satisfied", "Lbl_IssueStatus_Satisfied");
            AddLabel("NotRequired", "Lbl_LoadedNotRequired");
            AddLabel("NotLoaded", "Lbl_MaterialNotLoaded");

        }

        /// <summary>
        /// Override this to control properties displayed in the Detials panel
        /// </summary>
        protected override void SetDetailsToDisplay()
        {
            // <label to use, property value from MfgOrderListItem to display>
            displayDetails = new Dictionary<string, string>
            {
                { "MfgOrderName", "MfgOrderName" },
                { "WorkflowDisplay", "WorkflowDisplay" },
                { "WorkflowStep", "WorkflowStepName" },
                { "Quantity", "MfgOrderQty" },
                {"ProcessedQty", "ProcessedQty" },
                {"RemainingQty", "RemainingQty" },
                { "ERPOperation", "ERPOperationName" },
                { "Sequence", "ERPOperationNo" },
                { "ProductDisplay", "ProductDisplay" },
                { "ProductDescription", "ProductDescription" },
                { "CustomerName", "CustomerName" },
                { "PlannedStartDate", "PlannedStartDate" },
                { "PlannedCompletionDate", "PlannedCompletionDate" },
                { "OrderStatus", "OrderStatus"},
                { "PriorityCode", "PriorityCodeName" },
                { "Recipe", "Recipe" },
                { "RequiredRecipe", "RequiredRecipe" },
                { "LineAssignment", "LineAssignment" },
                //{ "Sequence", "ScheduledSequence" },
                { "SMTSide", "SMTSide" },

            };
        }
        #endregion virtual methods other workspaces may want to override

        #region virtual methods other workspaces should not need to override
        #endregion virtual methods other workspaces should not need to override

        protected override IEnumerable<ScriptReference> GetScriptReferences()
        {
            return base.GetScriptReferences().Concat(GetisMyScriptReferences());   
        }

        protected IEnumerable<ScriptReference> GetisMyScriptReferences()
        {
            yield return new ScriptReference("~/Scripts/MfgOperation_VP.js");
            yield return new ScriptReference("~/Scripts/MfgOperationMaterials.js");
            yield return new ScriptReference("~/Scripts/User/Industry Solutions/isMfgOperation.js");
            yield return new ScriptReference("~/Scripts/User/Industry Solutions/isMfgOperationComments.js");
            yield return new ScriptReference("~/Scripts/User/Industry Solutions/isMfgOperationMaterials.js");
        }

    }
}