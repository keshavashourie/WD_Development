//© 2022 Siemens Product Lifecycle Management Software Inc.
using System;
using System.Data;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WCF.ObjectStack;
using System.Web.Script.Serialization;
using Newtonsoft.Json;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{

    public class DispatchListItem
    {
        public string Container;
        public string Qty;
        public string Step;
        public string Operation;
        public string Product;
        public string InProcess;
        public string Status;
        public string IsOnHold;
    }


    /// <summary>
    /// TODO: Add a Summary description for this Camstar Web Part
    /// </summary>
    public class DispatchListWP : MatrixWebPart
    {
        #region Controls

        protected virtual CWC.TileContainer DispatchListTile { get { return Page.FindCamstarControl("DispatchListTile") as CWC.TileContainer; } }

        protected MatrixWebPart dispatchListWP { get { return Page.FindCamstarControl("DispatchListWP") as MatrixWebPart; } }

        #endregion

        #region Protected Functions
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            ScriptManager.RegisterStartupScript(this, this.GetType(), "initialize", $"DispatchListWP.initialize({GetClientLabels()});", true);
        }

        /// <summary>
        /// TODO: Summary Description of function
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            LoadLabels();
            BuildTiles(DispatchListTile);
        }

        protected override IEnumerable<ScriptReference> GetScriptReferences()
        {
            yield return new ScriptReference("~/Scripts/User/DispatchListWP.js");
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
                return "In Process";
            else
                return "In Queue";
        }

        /// <summary>
        /// Override this to set data when using a class derived from DispatchListItem
        /// </summary>
        /// <param name="recordSet"></param>
        protected string GetDispatchListItemListJSON(RecordSet recordSet)
        {
            var ordList = new List<DispatchListItem>();
            foreach (Row row in recordSet.Rows)
            {
                var item = new DispatchListItem
                {
                    Container = row.Values[1],
                    Qty = row.Values[10],
                    Product = row.Values[7],
                    Step = row.Values[5],
                    Operation = row.Values[12],
                    InProcess = getWIPState("1"),
                    Status = row.Values[11],
                    IsOnHold = row.Values[17]
                };

                ordList.Add(item);
                if (ordList.Count > 8)
                    break;
            }

            DispatchListTile.ControlState.Tiles = new List<CWC.TileContainer.TileContext>();

            foreach (DispatchListItem item in ordList)
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
            QueryParameters objQueryParameters = new QueryParameters();

            RecordSet recordSet = new RecordSet();
            ResultStatus status = new ResultStatus();

            objQueryParameters.Parameters = new QueryParameter[4];

            objQueryParameters.Parameters[0] = new QueryParameter();
            objQueryParameters.Parameters[0].Name = "Operation";
            objQueryParameters.Parameters[0].Value = "";

            objQueryParameters.Parameters[1] = new QueryParameter();
            objQueryParameters.Parameters[1].Name = "WorkCenter";
            objQueryParameters.Parameters[1].Value = "";

            objQueryParameters.Parameters[2] = new QueryParameter();
            objQueryParameters.Parameters[2].Name = "Factory";
            objQueryParameters.Parameters[2].Value = "";

            objQueryParameters.Parameters[3] = new QueryParameter();
            objQueryParameters.Parameters[3].Name = "NameFilter";
            objQueryParameters.Parameters[3].Value = "%";

            QueryOptions queryOptions = new QueryOptions()
            {
                QueryType = WCF.ObjectStack.QueryType.System,
                ChangeCount = 0
            };

            status = oService.Execute("DefaultDispatch_ContainerTxn", objQueryParameters, queryOptions, out recordSet);

            if (status.ExceptionData != null)
            {
                DisplayMessage(status);
            }
            else if (status.IsSuccess && recordSet.Rows != null)
            {
                GetDispatchListItemListJSON(recordSet);
                ScriptManager.RegisterStartupScript(this, this.GetType(), "dispatchTileNotEmpty", $"DispatchListWP.hideEmptyMessage({recordSet.Rows.Length});", true);
            }
            else if (tileContainer.ControlState.Tiles != null)
            {
                tileContainer.ControlState.Tiles.Clear();
                ScriptManager.RegisterStartupScript(this, this.GetType(), "displayEmptyMessage", $"DispatchListWP.displayEmptyMessage();", true);
            }

            this.WebPartManager.RenderToClient(dispatchListWP);
        }

        /// <summary>
        /// Creates a tile for the tile container based on data for an Mfg Order.
        /// Other workspaces should not have to override this.
        /// </summary>
        /// <param name="ordListItem"></param>
        /// <param name="tileColName"></param>
        /// <returns></returns>
        protected CWC.TileContainer.TileContext CreateTile(DispatchListItem ordListItem, string tileColName)
        {
            var tile = new CWC.TileContainer.TileContext
            {
                ColumnName = tileColName,
                Title = ordListItem.Container,
            };

            var strings = new List<string>();
            strings.Add($"{ordListItem.Qty}");
            strings.Add($"{labelValues["ProductDisplay"]}: {ordListItem.Product}");
            strings.Add($"{labelValues["WorkflowStep"]}: {ordListItem.Step}");
            strings.Add($"{ordListItem.InProcess}");
            strings.Add($"{ordListItem.Status}");
            strings.Add($"{ordListItem.IsOnHold}");
            tile.Text = strings.ToArray();
            tile.CustomData = ordListItem.Container;

            return tile;
        }
        #endregion

        #region Public Functions
        public static bool RefreshDispatchList(AjaxTransition transition)
        {
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            QueryService oService = new QueryService(session.CurrentUserProfile);
            QueryParameters objQueryParameters = new QueryParameters();

            RecordSet recordSet = new RecordSet();
            ResultStatus status = new ResultStatus();

            objQueryParameters.Parameters = new QueryParameter[4];

            objQueryParameters.Parameters[0] = new QueryParameter();
            objQueryParameters.Parameters[0].Name = "Operation";
            objQueryParameters.Parameters[0].Value = "";

            objQueryParameters.Parameters[1] = new QueryParameter();
            objQueryParameters.Parameters[1].Name = "WorkCenter";
            objQueryParameters.Parameters[1].Value = "";

            objQueryParameters.Parameters[2] = new QueryParameter();
            objQueryParameters.Parameters[2].Name = "Factory";
            objQueryParameters.Parameters[2].Value = "";

            objQueryParameters.Parameters[3] = new QueryParameter();
            objQueryParameters.Parameters[3].Name = "NameFilter";
            objQueryParameters.Parameters[3].Value = "%";

            QueryOptions queryOptions = new QueryOptions()
            {
                QueryType = WCF.ObjectStack.QueryType.System,
                ChangeCount = 0
            };

                status = oService.Execute("DefaultDispatch_ContainerTxn", objQueryParameters, queryOptions, out recordSet);
            

            string txnData = null;

            if (recordSet.Rows != null)
            {
                var tiles = new List<CWC.TileContainer.TileContext>();
                var serializer = new JavaScriptSerializer();
                var labels = serializer.Deserialize<Dictionary<string, string>>(transition.CommandParameters);
                foreach (Row row in recordSet.Rows)
                {
                    var item = new DispatchListItem
                    {
                        Container = row.Values[1],
                        Qty = row.Values[10],
                        Product = row.Values[7],
                        Step = row.Values[5],
                        Operation = row.Values[12],
                        InProcess = getWIPState("1"),
                        Status = row.Values[11],
                        IsOnHold = row.Values[17]
                    };

                    var tile = new CWC.TileContainer.TileContext
                    {
                        ColumnName = "Lot",
                        Title = item.Container,
                    };

                    var strings = new List<string>();
                    strings.Add($"{item.Qty}");
                    strings.Add($"{labels["ProductDisplay"]}: {item.Product}");
                    strings.Add($"{labels["WorkflowStep"]}: {item.Step}");
                    strings.Add($"{item.InProcess}");
                    strings.Add($"{item.Status}");

                    tile.Text = strings.ToArray();
                    tile.CustomData = item.Container;
                    tiles.Add(tile);

                    if (tiles.Count > 8)
                        break;
                }

                var tileContext = new CWC.TileContainer.TilesContext
                {
                    Tiles = tiles,
                    CustomData = recordSet.Rows.Length.ToString()
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
        #endregion

    }

}

