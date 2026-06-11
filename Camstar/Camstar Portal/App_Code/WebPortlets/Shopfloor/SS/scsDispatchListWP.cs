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
using Camstar.WCF.ObjectStack;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using WebClientPortal;
using System.IO;
using System.Text;
using System.Runtime.Serialization;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{

    public class scsDispatchListItem
    {
        public string Container;
        public string Qty;
        public string Qty2;
        public string Step;
        public string Spec;
        public string Operation;
        public string Product;
        public string WIPStatus;
        public string State;
        public string PriorityCode;
    }


    /// <summary>
    /// TODO: Add a Summary description for this Camstar Web Part
    /// </summary>
    public class scsDispatchListWP : MatrixWebPart
    {
        #region Controls

        protected virtual CWC.TileContainer DispatchListTile { get { return Page.FindCamstarControl("DispatchListTile") as CWC.TileContainer; } }

        protected MatrixWebPart DispatchListWP { get { return Page.FindCamstarControl("scsDispatchListWP") as MatrixWebPart; } }

        #endregion

        #region Protected Functions
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            ScriptManager.RegisterStartupScript(this, this.GetType(), "initialize", $"scsDispatchListWP.initialize({GetClientLabels()});", true);

            string lineAssigFactory = (Page.SessionDataContract.GetValueByName(Camstar.WebPortal.Constants.DataMemberConstants.Factory) != null ?
                                       Page.SessionDataContract.GetValueByName(Camstar.WebPortal.Constants.DataMemberConstants.Factory).ToString() : String.Empty);
            string lineAssigWorkCenter = (Page.SessionDataContract.GetValueByName(Camstar.WebPortal.Constants.DataMemberConstants.WorkCenter) != null ?
                                          Page.SessionDataContract.GetValueByName(Camstar.WebPortal.Constants.DataMemberConstants.WorkCenter).ToString() : String.Empty);
            string lineAssigOperation = (Page.SessionDataContract.GetValueByName(Camstar.WebPortal.Constants.DataMemberConstants.Operation) != null ?
                                           Page.SessionDataContract.GetValueByName(Camstar.WebPortal.Constants.DataMemberConstants.Operation).ToString() : String.Empty);
            string lineAssigResource = (Page.SessionDataContract.GetValueByName(Camstar.WebPortal.Constants.DataMemberConstants.Resource) != null ?
                                       Page.SessionDataContract.GetValueByName(Camstar.WebPortal.Constants.DataMemberConstants.Resource).ToString() : String.Empty);
            string lineAssigSpec = (Page.SessionDataContract.GetValueByName(Camstar.WebPortal.Constants.DataMemberConstants.Spec) != null ?
                                       Page.SessionDataContract.GetValueByName(Camstar.WebPortal.Constants.DataMemberConstants.Spec).ToString() : String.Empty);
        }

        /// <summary>
        /// TODO: Summary Description of function
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            LoadLabels();
            if (!Page.IsPostBack)
                BuildTiles(DispatchListTile);
        }

        protected override IEnumerable<ScriptReference> GetScriptReferences()
        {
            yield return new ScriptReference("~/Scripts/User/SCS/scsDispatchListWP.js");
        }

        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as Camstar.WebPortal.Personalization.CustomAction;

            if (action != null && action.Parameters == "Refresh")
            {
                BuildTiles(DispatchListTile);
            }
        }

        protected static string getWIPState(string wipState)
        {
            if (wipState == "1")
                return "Track In";
            else if (wipState == "2")
                return "Track Out";
            else if (wipState == "3")
                return "Move Out";
            else
                return "Move In";
        }

        /// <summary>
        /// Override this to set data when using a class derived from scsDispatchListItem
        /// </summary>
        /// <param name="recordSet"></param>
        protected string GetDispatchListItemListJSON(RecordSet recordSet)
        {
            var ordList = new List<scsDispatchListItem>();
            foreach (Row row in recordSet.Rows)
            {
                var item = new scsDispatchListItem
                {
                    Container = row.Values[0],
                    Qty = row.Values[1],
                    Qty2 = row.Values[2],
                    Product = row.Values[3],
                    Step = row.Values[4],
                    Spec = row.Values[5],
                    Operation = row.Values[6],
                    State = getWIPState(row.Values[7]),
                    WIPStatus = row.Values[8],
                    PriorityCode = row.Values[9]
                };

                ordList.Add(item);
            }

            DispatchListTile.ControlState.Tiles = new List<CWC.TileContainer.TileContext>();

            foreach (scsDispatchListItem item in ordList)
            {
                DispatchListTile.ControlState.Tiles.Add(CreateTile(item, "Lot"));
            }

            var serializer = new JavaScriptSerializer();
            return serializer.Serialize(ordList);
        }

        /// <summary>
        /// Override this to control labels sent to client.
        /// </summary>
        protected void SetLabelNames()
        {
            labelNames = new Dictionary<string, string>
            {
                { "ProductDisplay", "CSICDOName_Product" },                             // Product
                { "WorkflowStep", "CSICDOName_Step" },                                  // Step
                { "Quantity", "Web_Quantity" },                                           // Quantity
                { "Qty", "SelVal_Qty" },
                { "Qty2", "SelVal_Qty2" },
                { "SimpleWIPMain", "Lbl_SimpleWIPMain" }
            };
        }
        /// <summary>
        /// Other workspaces should not need to override this
        /// </summary>
        protected void LoadLabels()
        {
            // create mapping of friendly label names to actual label names
            SetLabelNames();

            // load labels to the cache
            var labelCache = FrameworkManagerUtil.GetLabelCache(System.Web.HttpContext.Current.Session);
            LabelList labelList = new LabelList();
            foreach (KeyValuePair<string, string> item in labelNames)
            {
                labelList.Add(new Label(item.Value));
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
        protected string GetClientLabels()
        {
            List<string> labelMap = new List<string>();
            foreach (KeyValuePair<string, string> label in labelValues)
            {
                labelMap.Add("\"" + label.Key + "\"" + ":" + "\"" + label.Value + "\"");
            }

            return "{" + String.Join(",", labelMap) + "}";
        }

        /// <summary>
        /// Other workspaces should not need to override this
        /// </summary>
        protected void BuildTiles(CWC.TileContainer tileContainer)
        {

            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            QueryService oService = new QueryService(session.CurrentUserProfile);
            QueryParameters queryParam = new QueryParameters();

            RecordSet recordSet = new RecordSet();
            ResultStatus status = new ResultStatus();

            object lineAssignmentResource = Page.SessionDataContract.GetValueByName(Camstar.WebPortal.Constants.DataMemberConstants.Resource);
            object lineAssignmentOperation = Page.SessionDataContract.GetValueByName(Camstar.WebPortal.Constants.DataMemberConstants.Operation);
            object lineAssignmentWorkCenter = Page.SessionDataContract.GetValueByName(Camstar.WebPortal.Constants.DataMemberConstants.WorkCenter);
            object lineAssignmentSpec = Page.SessionDataContract.GetValueByName(Camstar.WebPortal.Constants.DataMemberConstants.Spec);

            string workCenter = "";
            string operationName = "";
            string specFilter = "";
            if (lineAssignmentWorkCenter != null)
                workCenter = lineAssignmentWorkCenter.ToString();

            if (lineAssignmentOperation != null)
                operationName = lineAssignmentOperation.ToString();

            if (lineAssignmentSpec != null)
                specFilter = lineAssignmentSpec.ToString();

            if (lineAssignmentResource != null && lineAssignmentResource.ToString() != "")
            {

                string resourceName = lineAssignmentResource.ToString();
                queryParam = new QueryParameters();
                queryParam.Parameters = new QueryParameter[4];
                queryParam.Parameters[0] = new QueryParameter();
                queryParam.Parameters[0].Name = "RESOURCE";
                queryParam.Parameters[0].Value = resourceName;
                queryParam.Parameters[1] = new QueryParameter();
                queryParam.Parameters[1].Name = "CONTAINERNAME";
                queryParam.Parameters[1].Value = "";
                queryParam.Parameters[2] = new QueryParameter();
                queryParam.Parameters[2].Name = "STATE";
                queryParam.Parameters[2].Value = "";
                queryParam.Parameters[3] = new QueryParameter();
                queryParam.Parameters[3].Name = "NUMOFRECORD";
                queryParam.Parameters[3].Value = numOfRecord.ToString();

                QueryOptions queryOptions = new QueryOptions()
                {
                    QueryType = WCF.ObjectStack.QueryType.System,
                    ChangeCount = 0
                };

                status = oService.Execute("scsGetDashboardDispatchListByResource", queryParam, queryOptions, out recordSet);

                if (status.ExceptionData != null)
                {
                    DisplayMessage(status);
                }
                else if (status.IsSuccess && recordSet.Rows != null)
                {
                    GetDispatchListItemListJSON(recordSet);
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "dispatchTileNotEmpty", $"scsDispatchListWP.hideEmptyMessageWithLoad({recordSet.Rows[0].Values[10]}, true);", true);

                }
                else if (tileContainer.ControlState.Tiles != null)
                {
                    tileContainer.ControlState.Tiles.Clear();
                    if (lineAssignmentOperation == null)
                        ScriptManager.RegisterStartupScript(this, this.GetType(), "displayEmptyMessage", $"scsDispatchListWP.displayEmptyMessageWithLoad(true);", true);
                }

            }
            else if (specFilter != "" || workCenter != "" || operationName != "")
            {
                //Spec having higher Priority than WorkCenter & Operation
                queryParam = new QueryParameters();
                queryParam.Parameters = new QueryParameter[6];
                queryParam.Parameters[0] = new QueryParameter();
                queryParam.Parameters[0].Name = "WORKCENTER";
                queryParam.Parameters[0].Value = !String.IsNullOrEmpty(specFilter) ? "" : workCenter;
                queryParam.Parameters[1] = new QueryParameter();
                queryParam.Parameters[1].Name = "OPERATION";
                queryParam.Parameters[1].Value = !String.IsNullOrEmpty(specFilter) ? "" : operationName;
                queryParam.Parameters[2] = new QueryParameter();
                queryParam.Parameters[2].Name = "CONTAINERNAME";
                queryParam.Parameters[2].Value = "";
                queryParam.Parameters[3] = new QueryParameter();
                queryParam.Parameters[3].Name = "STATE";
                queryParam.Parameters[3].Value = "";
                queryParam.Parameters[4] = new QueryParameter();
                queryParam.Parameters[4].Name = "NUMOFRECORD";
                queryParam.Parameters[4].Value = numOfRecord.ToString();
                queryParam.Parameters[5] = new QueryParameter();
                queryParam.Parameters[5].Name = "SPEC";
                queryParam.Parameters[5].Value = specFilter;


                QueryOptions queryOptions = new QueryOptions()
                {
                    QueryType = WCF.ObjectStack.QueryType.System,
                    ChangeCount = 0
                };

                status = oService.Execute("scsGetDashboardDispatchListByOperation", queryParam, queryOptions, out recordSet);

                if (status.ExceptionData != null)
                {
                    DisplayMessage(status);
                }
                else if (status.IsSuccess && recordSet.Rows != null)
                {
                    GetDispatchListItemListJSON(recordSet);
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "dispatchTileNotEmpty", $"scsDispatchListWP.hideEmptyMessageWithLoad({recordSet.Rows[0].Values[10]},true);", true);
                }
                else if (tileContainer.ControlState.Tiles != null)
                {
                    tileContainer.ControlState.Tiles.Clear();
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "displayEmptyMessage", $"scsDispatchListWP.displayEmptyMessageWithLoad(true);", true);
                }
            }

            this.WebPartManager.RenderToClient(DispatchListWP);
        }

        /// <summary>
        /// Creates a tile for the tile container based on data for an Mfg Order.
        /// Other workspaces should not have to override this.
        /// </summary>
        /// <param name="ordListItem"></param>
        /// <param name="tileColName"></param>
        /// <returns></returns>
        protected CWC.TileContainer.TileContext CreateTile(scsDispatchListItem ordListItem, string tileColName)
        {
            var tile = new CWC.TileContainer.TileContext
            {
                ColumnName = tileColName,
                Title = ordListItem.Container,
            };

            var strings = new List<string>();
            strings.Add($"{ordListItem.Qty}");
            strings.Add($"{ordListItem.Qty2}");
            strings.Add($"{labelValues["ProductDisplay"]}: {ordListItem.Product}");
            strings.Add($"{labelValues["WorkflowStep"]}: {ordListItem.Step}");
            strings.Add($"{ordListItem.State}");
            strings.Add($"{ordListItem.PriorityCode}");
            strings.Add($"{ordListItem.WIPStatus}");
            tile.Text = strings.ToArray();
            tile.CustomData = ordListItem.Container;

            return tile;
        }
        #endregion

        #region Public Functions
        public static bool RefreshDispatchList(AjaxTransition transition)
        {
            object lineAssignmentResource = null;
            object lineAssignmentOperation = null;
            object lineAssignmentWorkCenter = null;
            object lineAssignmentSpec = null;

            ApolloPortalService appolloSvc = new ApolloPortalService();
            var appolloSettings = new ApolloPortalService.ApolloSettings();
            ResultStatus status = appolloSvc.GetApolloSettings(out appolloSettings);
            if (status.IsSuccess)
            {
                object resource = appolloSettings.Resource;
                object operation = appolloSettings.Operation;
                object workcenter = appolloSettings.Workcenter;
                object workstation = appolloSettings.Workstation;
                object spec = appolloSettings.Spec;

                //Retrieve Line Assigment Data
                lineAssignmentResource = (resource != null ? resource.ToString() : String.Empty);
                lineAssignmentOperation = (operation != null ? operation.ToString() : String.Empty);
                lineAssignmentWorkCenter = (workcenter != null ? workcenter.ToString() : String.Empty);
                lineAssignmentSpec = (spec != null ? spec.ToString() : String.Empty);
            }

            ClientControllerState parms = null;
            using (Stream str = new MemoryStream(Encoding.UTF8.GetBytes(transition.CommandParameters)))
            {
                var ser = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(ClientControllerState));
                parms = ser.ReadObject(str) as ClientControllerState;
            }

            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            QueryService oService = new QueryService(session.CurrentUserProfile);
            QueryParameters queryParam = new QueryParameters();

            RecordSet recordSet = new RecordSet();
            status = new ResultStatus();

            string workCenter = "";
            string operationName = "";
            string specFilter = "";
            if (lineAssignmentWorkCenter != null)
            {
                workCenter = lineAssignmentWorkCenter.ToString();
            }
            if (lineAssignmentOperation != null)
            {
                operationName = lineAssignmentOperation.ToString();
            }
            if (lineAssignmentSpec != null)
            {
                specFilter = lineAssignmentSpec.ToString();
            }

            if (lineAssignmentResource != null && lineAssignmentResource.ToString() != "")
            {

                string resourceName = lineAssignmentResource.ToString();
                queryParam = new QueryParameters();
                queryParam.Parameters = new QueryParameter[4];
                queryParam.Parameters[0] = new QueryParameter();
                queryParam.Parameters[0].Name = "RESOURCE";
                queryParam.Parameters[0].Value = resourceName;
                queryParam.Parameters[1] = new QueryParameter();
                queryParam.Parameters[1].Name = "CONTAINERNAME";
                queryParam.Parameters[1].Value = parms.TextSearch;
                queryParam.Parameters[2] = new QueryParameter();
                queryParam.Parameters[2].Name = "STATE";
                queryParam.Parameters[2].Value = parms.State;
                queryParam.Parameters[3] = new QueryParameter();
                queryParam.Parameters[3].Name = "NUMOFRECORD";
                queryParam.Parameters[3].Value = numOfRecord.ToString();

                QueryOptions queryOptions = new QueryOptions()
                {
                    QueryType = WCF.ObjectStack.QueryType.System,
                    ChangeCount = 0
                };

                status = oService.Execute("scsGetDashboardDispatchListByResource", queryParam, queryOptions, out recordSet);
            }
            else if (specFilter != "" || workCenter != "" || operationName != "")
            {
                //Spec having higher Priority than WorkCenter & Operation
                queryParam = new QueryParameters();
                queryParam.Parameters = new QueryParameter[6];
                queryParam.Parameters[0] = new QueryParameter();
                queryParam.Parameters[0].Name = "WORKCENTER";
                queryParam.Parameters[0].Value = !String.IsNullOrEmpty(specFilter) ? "" : workCenter;
                queryParam.Parameters[1] = new QueryParameter();
                queryParam.Parameters[1].Name = "OPERATION";
                queryParam.Parameters[1].Value = !String.IsNullOrEmpty(specFilter) ? "" : operationName;
                queryParam.Parameters[2] = new QueryParameter();
                queryParam.Parameters[2].Name = "CONTAINERNAME";
                queryParam.Parameters[2].Value = parms.TextSearch;
                queryParam.Parameters[3] = new QueryParameter();
                queryParam.Parameters[3].Name = "STATE";
                queryParam.Parameters[3].Value = parms.State;
                queryParam.Parameters[4] = new QueryParameter();
                queryParam.Parameters[4].Name = "NUMOFRECORD";
                queryParam.Parameters[4].Value = numOfRecord.ToString();
                queryParam.Parameters[5] = new QueryParameter();
                queryParam.Parameters[5].Name = "SPEC";
                queryParam.Parameters[5].Value = specFilter;


                QueryOptions queryOptions = new QueryOptions()
                {
                    QueryType = WCF.ObjectStack.QueryType.System,
                    ChangeCount = 0
                };

                status = oService.Execute("scsGetDashboardDispatchListByOperation", queryParam, queryOptions, out recordSet);
            }

            string txnData = null;
            string totalLot = "0";

            if (recordSet.Rows != null)
            {
                var tiles = new List<CWC.TileContainer.TileContext>();
                var serializer = new JavaScriptSerializer();
                var labels = serializer.Deserialize<Dictionary<string, string>>(parms.Labels);
                foreach (Row row in recordSet.Rows)
                {
                    var item = new scsDispatchListItem
                    {
                        Container = row.Values[0],
                        Qty = row.Values[1],
                        Qty2 = row.Values[2],
                        Product = row.Values[3],
                        Step = row.Values[4],
                        Spec = row.Values[5],
                        Operation = row.Values[6],
                        State = getWIPState(row.Values[7]),
                        WIPStatus = row.Values[8],
                        PriorityCode = row.Values[9]
                    };

                    var tile = new CWC.TileContainer.TileContext
                    {
                        ColumnName = "Lot",
                        Title = item.Container,
                    };

                    var strings = new List<string>();
                    strings.Add($"{item.Qty}");
                    strings.Add($"{item.Qty2}");
                    strings.Add($"{labels["ProductDisplay"]}: {item.Product}");
                    strings.Add($"{labels["WorkflowStep"]}: {item.Step}");
                    strings.Add($"{item.State}");
                    strings.Add($"{item.PriorityCode}");
                    strings.Add($"{item.WIPStatus}");

                    tile.Text = strings.ToArray();
                    tile.CustomData = item.Container;
                    tiles.Add(tile);

                    totalLot = row.Values[10];
                }

                var tileContext = new CWC.TileContainer.TilesContext
                {
                    Tiles = tiles,
                    CustomData = totalLot
                };

                txnData = JsonConvert.SerializeObject(tileContext, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            }

            transition.Response = new[] { new ResponseSection(ResponseType.Command, transition.ID, txnData) };

            return true;
        }

        #endregion

        #region Private Functions

        #endregion

        #region Constants

        #endregion

        #region Private Member Variables
        private Dictionary<string, string> labelNames;
        private Dictionary<string, string> labelValues;
        private static int numOfRecord = 9;
        #endregion


        #region Public Class
        [DataContract]
        public class ClientControllerState
        {
            [DataMember]
            public string Labels { get; set; }
            [DataMember]
            public string TextSearch { get; set; }
            [DataMember]
            public string State { get; set; }
        }
        #endregion
    }

}

