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
    public class MfgOrderListItem
    {
        public string ProductDisplay;
        public string ProductDescription;
        public string MfgOrderName;
        public string ERPOperationName;
        public string ERPOperationNo;
        public long MfgOrderQty;
        public string CustomerName;
        public string PlannedStartDate;
        public string PlannedCompletionDate;
        public string Product;      // InstanceId (no display)
        public string MfgOrder;     // InstanceId (no display)
        public string Spec;         // InstanceId (no display)
        public string Workflow;     // InstanceId (no display)
        public string WorkflowDisplay;
        public string WorkflowStepName;
        public string ERPOperation;
        public string PriorityCodeName;
        public string RouteStepId;
        public string RoutStepName;
        public string OrderStatus;
        public string WorkflowStepId;
        public string ProductName;
        public string ProductRevision;
        public long ProcessedQty;
        public long RemainingQty;
        public string SpecName;
        public string SpecRevision;
        public string UniqueId;
    }

    /// <summary>
    /// Implement behavior for MfgOperation_VP page
    /// </summary>
    public class MfgOperation : MatrixWebPart
    {
        private bool resettingPage = false;

        protected Dictionary<string, string> labelNames;
        protected Dictionary<string, string> labelValues;
        protected Dictionary<string, string> displayDetails;

        protected virtual CWC.TileContainer TileContainer { get { return Page.FindCamstarControl("MfgOrderList") as CWC.TileContainer; } }

        protected MatrixWebPart MfgOperationWP { get { return Page.FindCamstarControl("MfgOperationWP") as MatrixWebPart; } }

        protected CWC.NamedObject SelectedResource { get { return Page.FindCamstarControl("Resource") as CWC.NamedObject; } }
        protected CWC.NamedObject MfgOrderFilter { get { return Page.FindCamstarControl("MfgOrderFilter") as CWC.NamedObject; } }

        public MfgOperation()
        {
        }

        #region Page Event Handlers
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            SelectedResource.DataChanged += SelectedResource_DataChanged;
            MfgOrderFilter.DataChanged += MfgOrderFilter_DataChanged;
            TileContainer.TileClick += TileContainer_TileClick;
            LoadLabels();
            SetDetailsToDisplay();
            if (!Page.IsPostBack)
            {
                object lineAssignmentResource = Page.SessionDataContract.GetValueByName(Camstar.WebPortal.Constants.DataMemberConstants.Resource);
                object lineAssignmentWorkStation = Page.SessionDataContract.GetValueByName(Camstar.WebPortal.Constants.DataMemberConstants.WorkStation);

                if (lineAssignmentResource != null && !string.IsNullOrEmpty(lineAssignmentResource.ToString()))
                    SelectedResource.Data = lineAssignmentResource.ToString();
                else if (lineAssignmentWorkStation != null && !string.IsNullOrEmpty(lineAssignmentWorkStation.ToString()))
                    SelectedResource.Data = lineAssignmentWorkStation.ToString();
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            ScriptManager.RegisterStartupScript(this, this.GetType(), "initializeMfgOperation", $"mfgOperation.initialize({GetClientLabels()},{GetDisplayDetails()});", true);
        }


        protected void SelectedResource_DataChanged(object sender, EventArgs e)
        {
            if(!resettingPage)
                BuildTiles();
        }

        protected void MfgOrderFilter_DataChanged(object sender, EventArgs e)
        {
            if (!resettingPage)
                BuildTiles();
        }

        protected void TileContainer_TileClick(object sender, CWC.TileContainer.TileEventArgs e)
        {
            //Page.DisplayMessage(JsonConvert.SerializeObject(e), true);
            //TODO: why is this? what does it do?
        }

        public override void WebPartCustomAction(object sender, CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as Camstar.WebPortal.Personalization.CustomAction;
            if (action != null)
                switch (action.Parameters)
                {
                    case "Reset":
                        {
                            resettingPage = true;
                            MfgOrderFilter.ClearData();
                            SelectedResource.ClearData();
                            Page.ClearValues();
                            TileContainer.ControlState.Tiles?.Clear();
                            ScriptManager.RegisterStartupScript(this, this.GetType(), "initializeMfgOperationScript", $"mfgOperation.setOrderList(null);", true);
                            this.WebPartManager.RenderToClient(MfgOperationWP);
                            resettingPage = false;
                        }
                        break;
                }
        }
        #endregion Page Event Handlers

        #region virtual methods other workspaces may want to override
        /// <summary>
        /// Override this to change the query used for getting the Mfg Order List
        /// </summary>
        /// <returns></returns>
        protected virtual string GetMfgOrderListQuery()
        {
            return "GetMfgOrderList";
        }

        /// <summary>
        /// Override this to set data when using a class derived from MfgOrderListItem
        /// </summary>
        /// <param name="recordSet"></param>
        protected virtual string GetMfgOrderListJSON(RecordSet recordSet)
        {
            var ordList = new List<MfgOrderListItem>();
            foreach (Row row in recordSet.Rows)
            {
                var item = new MfgOrderListItem
                {
                    ProductDisplay = row.Values[0],
                    ProductDescription = row.Values[1],
                    MfgOrderName = row.Values[2],
                    ERPOperationName = row.Values[3],
                    ERPOperationNo = row.Values[4],
                    MfgOrderQty = string.IsNullOrWhiteSpace(row.Values[5]) ? 0 : Convert.ToInt32(row.Values[5]),
                    CustomerName = row.Values[6],
                    PlannedStartDate = GetLocalTimeString(row.Values[7]),
                    PlannedCompletionDate = GetLocalTimeString(row.Values[8]),
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
                    ProcessedQty = string.IsNullOrWhiteSpace(row.Values[23]) ? 0 : Convert.ToInt32(row.Values[23]),
                    SpecName = row.Values[24],
                    SpecRevision = row.Values[25]
                    
                };
                item.RemainingQty = item.MfgOrderQty - item.ProcessedQty;
                item.UniqueId = item.MfgOrderName + "_" + item.WorkflowStepName;

                ordList.Add(item);
            }

            TileContainer.ControlState.DefaultImage = "typeWorkOrderMfg48.svg";
            TileContainer.ControlState.Tiles = new List<CWC.TileContainer.TileContext>();
            var tileColName = TileContainer.ControlState.Columns[0].Name;    // assuming we have only one column here. is there a use case for more than one? and what does that do?
            foreach (MfgOrderListItem item in ordList)
            {
                TileContainer.ControlState.Tiles.Add(CreateTile(item, tileColName));
            }

            return new JavaScriptSerializer() { MaxJsonLength = Int32.MaxValue }.Serialize(ordList);
        }

        protected void AddLabel(string id, string labelName)
        {
            if (labelNames.ContainsKey(id))
                labelNames[id] = labelName;
            else
                labelNames.Add(id, labelName);
        }

        /// <summary>
        /// Override this to control labels sent to client.
        /// </summary>
        protected virtual void SetLabelNames()
        {
            labelNames = new Dictionary<string, string>
            {
                // labels for GetMfgOrderList resultset
                { "ProductDisplay", "CSICDOName_Product" },                             // Product
                { "ProductDescription", "SelVal_ProductDescription" },                  // Product Description
                { "MfgOrderName", "CSICDOName_MfgOrder" },                              // Mfg Order
                { "ERPOperationName", "MfgOrderList_ERPOperationName" },                // ERP Operation Name
                { "ERPOperationNo", "MfgOrderList_ERPOperationNo" },                    // ERP Operation Sequence
                { "MfgOrderQty", "IntegrationContainerInfo_MfgOrderQty" },              // Mfg Order Qty
                { "CustomerName", "Container_Customer" },                               // Customer
                { "PlannedStartDate", "MfgOrder_PlannedStartDate" },                    // Planned Start Date
                { "PlannedCompletionDate", "MfgOrder_PlannedCompletionDate" },          // Planned Completion Date
                { "WorkflowDisplay", "Container_Workflow" },                            // Workflow
                { "WorkflowStep", "CSICDOName_Step" },                                  // Step
                { "ERPOperation", "RouteStep_ERPOperation" },                           // ERP Operation
                { "PriorityCode", "CSICDOName_PriorityCode" },                          // Priority Code
                { "RouteStepName", "RouteStep_Name" },                                  // Route Step Name
                { "OrderStatus", "MfgOrder_OrderStatus" },                              // Order Status
                { "ProductName", "AdvSearch_ProductName" },                             // Product Name
                { "ProductRevision", "ResourceStatusDetails_ProductRev" },              // Product Revsion
                { "SpecName", "Spec_Name"},                                             // Spec Name
                { "SpecRevision", "ContainerStatusDetails_SpecRevision"},               // Spec Revision
                { "ProcessedQty", "Thruput_QtyProcessed"},                              // Qty Processed

                // alternates and others
                { "Quantity", "Web_Quantity" },                                         // Quantity
                { "WorkflowStepName", "Container_WorkflowStep" },                       // Workflow Step
                { "MfgOrderDetail", "MfgOrderDetail" },                                 // Mfg Order Detail (title for details panel)
                { "Description", "Lbl_Description"},                                    // Description
                { "Sequence", "Step_Sequence"},                                         // Sequence
                { "MaterialRequirements", "MaterialRequirementsSection" },              // Material Requirements
                { "Comments", "Lbl_Comments" },                                         // Comments
                { "Documents", "Web_Documents" },                                        // Documents
                { "NoData", "Mom_EmptyComponentText" },
                { "Save", "Lbl_Save" },
                { "RemainingQty", "Thruput_QtyRemaining" },
                { "Status", "Status" },
                { "Product", "Web_Product" },
                { "ContainerQty", "Container_Qty" },
                { "ProducingOrder", "MaterialListItem_ProducingOrder" },
                { "Slot", "HVSetupHistoryDetail_Slot" },
                { "SubSlot", "HVSetupHistoryDetail_SubSlot" },
                { "Lot", "ComponentDefectHistoryDetail_Lot" },
                { "HideNonRequirement", "LblMfgOperationMaterialRequirementHideNonRequirement" },
                { "Satisfied", "Lbl_IssueStatus_Satisfied" },
                { "NotRequired", "Lbl_LoadedNotRequired" },
                { "NotLoaded", "Lbl_MaterialNotLoaded" },
                { "Attributes", "Lbl_Attributes" },
                { "Web_Resource", "Web_Resource" },
                { "SelVal_Value", "SelVal_Value" },
                { "Lbl_Name", "Lbl_Name" }
            };
        }

        /// <summary>
        /// Override this to control properties displayed in the Detials panel
        /// </summary>
        protected virtual void SetDetailsToDisplay()
        {
            // <label to use, property value from MfgOrderListItem to display>
            displayDetails = new Dictionary<string, string>
            {
                { "MfgOrderName", "MfgOrderName"},
                { "WorkflowDisplay", "WorkflowDisplay" },
                { "WorkflowStep", "WorkflowStepName"},
                { "Quantity", "MfgOrderQty"},
                { "ERPOperation", "ERPOperationName"},
                { "Sequence", "ERPOperationNo"},
                { "ProductDisplay", "ProductDisplay"},
                { "ProductDescription", "ProductDescription"},
                { "CustomerName", "CustomerName"},
                { "PlannedStartDate", "PlannedStartDate"},
                { "PlannedCompletionDate", "PlannedCompletionDate"},
                { "OrderStatus", "OrderStatus"},
            };
        }
        #endregion virtual methods other workspaces may want to override

        #region virtual methods other workspaces should not need to override
        /// <summary>
        /// Other workspaces should not need to override this
        /// </summary>
        protected virtual void BuildTiles()
        {
            TileContainer.DefaultImage = "typeWorkOrderMfg48.svg";
            if (SelectedResource.Data != null)
            {
                FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
                QueryService oService = new QueryService(session.CurrentUserProfile);
                QueryParameters objQueryParameters = new QueryParameters();
                QueryParameter[] queryParameters = new QueryParameter[2]
                {
                    new QueryParameter("ResourceName", SelectedResource.Data.ToString()),
                    new QueryParameter("MfgOrderName", MfgOrderFilter.Data != null ? MfgOrderFilter.Data.ToString() : "")
                };
                objQueryParameters.Parameters = queryParameters;

                ResultStatus status = oService.Execute(GetMfgOrderListQuery(), objQueryParameters, new QueryOptions(), out RecordSet recordSet);

                if (status.ExceptionData != null)
                {
                    DisplayMessage(status);
                }
                else if (status.IsSuccess && recordSet.Rows != null)
                {
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "initializeMfgOperationScript", $"mfgOperation.setOrderList({GetMfgOrderListJSON(recordSet)});", true);
                }
                else if (TileContainer.ControlState.Tiles != null)
                {
                    TileContainer.ControlState.Tiles.Clear();
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "initializeMfgOperationScript", $"mfgOperation.setOrderList(null);", true);
                }
            }
            else if (TileContainer.ControlState.Tiles != null)
            {
                TileContainer.ControlState.Tiles.Clear();
                ScriptManager.RegisterStartupScript(this, this.GetType(), "initializeMfgOperationScript", $"mfgOperation.setOrderList(null);", true);
            }
            this.WebPartManager.RenderToClient(MfgOperationWP);
        }

        /// <summary>
        /// Other workspaces should not need to override this
        /// </summary>
        protected virtual void LoadLabels()
        {
            // create mapping of friendly label names to actual label names
            SetLabelNames();

            // load labels to the cache
            var labelCache = FrameworkManagerUtil.GetLabelCache(System.Web.HttpContext.Current.Session);
            LabelList labelList = new LabelList();
            foreach (KeyValuePair<string, string> item in labelNames)
            {
                labelList.Add(new OM.Label(item.Value));
            }
            labelCache.GetLabels(labelList);

            // create mapping of friendly names to lable values
            labelValues = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> item in labelNames)
            {
                var label = labelCache.GetLabelByName(item.Value);
                labelValues.Add(item.Key, label.Value);
            }
        }

        /// <summary>
        /// Creates a JSON string representing an object with label name values for passing to the client
        /// Other workspaces should not need to override this
        /// </summary>
        /// <returns></returns>
        protected virtual string GetClientLabels()
        {
            List<string> labelMap = new List<string>();
            foreach (KeyValuePair<string, string> label in labelValues)
            {
                labelMap.Add("\"" + label.Key + "\"" + ":" + "\"" + label.Value + "\"");
            }

            return "{" + String.Join(",", labelMap) + "}";
        }

        /// <summary>
        /// Creates a JSON string representing an array of objects that define properties to display in the Details panel
        /// Other workspaces should not need to override this.
        /// </summary>
        /// <returns></returns>
        protected virtual string GetDisplayDetails()
        {
            List<string> details = new List<string>();
            foreach (KeyValuePair<string, string> detail in displayDetails)
            {
                // a json obj of form { label : <label to use>, value : <order data property to use> }
                details.Add("{\"label\":\"" + detail.Key + "\", \"value\":\"" + detail.Value + "\"}");
            }

            // make an array of the objs
            return "[" + String.Join(",", details) + "]";
        }

        /// <summary>
        /// Creates a tile for the tile container based on data for an Mfg Order.
        /// Other workspaces should not have to override this.
        /// </summary>
        /// <param name="ordListItem"></param>
        /// <param name="tileColName"></param>
        /// <returns></returns>
        protected virtual CWC.TileContainer.TileContext CreateTile(MfgOrderListItem ordListItem, string tileColName)
        {
            var tile = new CWC.TileContainer.TileContext
            {
                ColumnName = tileColName,
                Title = ordListItem.MfgOrderName,
                Image = "typeWorkOrderMfg48.svg"
            };

            var strings = new List<string>();
            strings.Add($"{labelValues["WorkflowStep"]} : {ordListItem.WorkflowStepName}");
            if (string.Compare(ordListItem.ProductDisplay, ":") == 0)
                strings.Add($"{labelValues["ProductDisplay"]} : ");
            else
                strings.Add($"{labelValues["ProductDisplay"]} : {ordListItem.ProductDisplay}");
            strings.Add($"{labelValues["Quantity"]} : {ordListItem.MfgOrderQty}");
            strings.Add($"{labelValues["PlannedStartDate"]} : {ordListItem.PlannedStartDate}");
            tile.Text = strings.ToArray();
            tile.CustomData = ordListItem.UniqueId;

            return tile;
        }

        /// <summary>
        /// Converts a GMT time to a local display value
        /// Other workspaces should not need to override this.
        /// </summary>
        /// <param name="gmtTimeString"></param>
        /// <returns></returns>
        protected virtual string GetLocalTimeString(string gmtTimeString)
        {
            string localTimeString = string.Empty;

            if (!string.IsNullOrWhiteSpace(gmtTimeString))
            {
                try
                {
                    DateTime gmtTime = DateTime.Parse(gmtTimeString);
                    TimeSpan utcOffset = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session).CurrentUserProfile.UTCOffset;
                    DateTime localTime = gmtTime.Add(utcOffset);
                    localTimeString = localTime.ToString(); // TODO: I think this just uses server culture to convert. should we do anything else?
                }
                catch (Exception)
                {
                }

            }

            return localTimeString;
        }
        #endregion virtual methods other workspaces should not need to override

        protected override IEnumerable<ScriptReference> GetScriptReferences()
        {
            yield return new ScriptReference("~/Scripts/CRModules.js");
            yield return new ScriptReference("~/Scripts/CRSlideOut.js");
            yield return new ScriptReference("~/Scripts/CRSideBar.js");
            yield return new ScriptReference("~/Scripts/MfgOperationMaterials.js");
            yield return new ScriptReference("~/Scripts/MfgOperation_VP.js");
            yield return new ScriptReference("~/Scripts/MfgOperationMaterials.js");
        }
    }
}