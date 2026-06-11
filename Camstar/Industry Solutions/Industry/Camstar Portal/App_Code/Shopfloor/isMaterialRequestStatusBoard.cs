// Copyright Siemens 2025

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using System.Web.UI;
using Camstar.WCF.Services;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.FormsFramework.Utilities;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using PERS = Camstar.WebPortal.Personalization;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.WCFUtilities;
namespace Camstar.WebPortal.WebPortlets
{
    public class isMaterialRequestStatusBoard : MatrixWebPart
    {
        #region
        protected virtual CWC.TileContainer TileContainer
        {
            get { return Page.FindCamstarControl("MaterialRequestStatusBoardTileContainer") as CWC.TileContainer; }
        }
        protected virtual CWC.NamedObject MaterialQueue
        {
            get { return Page.FindCamstarControl("isMaterialRequestStatusBoard_isMaterialQueue") as CWC.NamedObject; }
        }
        protected virtual CWC.Button ButtonUpdate
        {
            get { return Page.FindCamstarControl("ButtonUpdate") as CWC.Button; }
        }
        protected MatrixWebPart RerenderWP { get { return Page.FindCamstarControl("BlankWP") as MatrixWebPart; } }
        public virtual OM.ResultStatus QueryStatus { get; protected set; }
        public virtual OM.QueryType QueryType { get; protected set; }
        #endregion

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ButtonUpdate.Style.Add("display", "none");
            ButtonUpdate.Click += ButtonUpdate_Click;
            if (!Page.IsPostBack)
            {
                OM.RecordSet rsMaterialRequestStatusBoardDetails = GetMaterialRequestStatusBoardRecords();
                TileContainer.ControlState.Tiles = new List<CWC.TileContainer.TileContext>();
                foreach (var column in TileContainer.ControlState.Columns)
                {
                    TileContainer.ControlState.Tiles.AddRange(GenerateTiles(column.Name, rsMaterialRequestStatusBoardDetails));
                }
            }
            TileContainer.TileClick += TileContainer_TileClick;

            var startupScript = string.Format("isMaterialRequestStatusBoard.connectToSignalRHub('{0}', '{1}');", TileContainer.SignalRUrl, Guid.NewGuid().ToString());

            if (!Page.IsPostBack && !Page.ClientScript.IsStartupScriptRegistered("isMaterialRequestStatusBoardStartup"))
                ScriptManager.RegisterStartupScript(this, GetType(), "isMaterialRequestStatusBoardStartup", startupScript, true);
        }

        private void ButtonUpdate_Click(object sender, EventArgs e)
        {
            OM.RecordSet rsMaterialRequestStatusBoardDetails = GetMaterialRequestStatusBoardRecords();
            TileContainer.ControlState.Tiles = new List<CWC.TileContainer.TileContext>();
            foreach (var column in TileContainer.ControlState.Columns)
            {
                TileContainer.ControlState.Tiles.AddRange(GenerateTiles(column.Name, rsMaterialRequestStatusBoardDetails));

            }
            this.WebPartManager.RenderToClient(RerenderWP);
        }

        private OM.RecordSet GetMaterialRequestStatusBoardRecords()
        {
            var fs = FrameworkManagerUtil.GetFrameworkSession();
            UserProfile profile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
            var serv = new QueryService(fs.CurrentUserProfile);
            var qparam = new OM.QueryParameters()
            {
                Parameters = new OM.QueryParameter[]
                            {
                            }
            };

            var resultMaterialRequestStatusBoard = new OM.RecordSet();
            var resultStatus = serv.Execute("isGetMaterialRequestStatusBoardDetails", qparam,
                                        new OM.QueryOptions() { QueryType = OM.QueryType.System }, out resultMaterialRequestStatusBoard);
            return resultMaterialRequestStatusBoard;

        }
        private void TileContainer_TileClick(object sender, CWC.TileContainer.TileEventArgs e)
        {
            Page.DisplayMessage(JsonConvert.SerializeObject(e), true);
        }

        protected List<CWC.TileContainer.TileContext> GenerateTiles(string columnName, OM.RecordSet rsMaterialRequestStatusBoardDetails)
        {
            var tiles = new List<CWC.TileContainer.TileContext>();
            int requestedState;
            switch (columnName)
            {
                case "Column1":   // Requested
                    {
                        requestedState = 0;
                        break;
                    }
                case "Column2":  // Waiting for Pickup
                    {
                        requestedState = 2;
                        break;
                    }
                case "Column3":  // In Progress
                    {
                        requestedState = 3;
                        break;
                    }
                case "Column4":  //Pick Up Complete
                    {
                        requestedState = 4;
                        break;
                    }
                default:
                    {
                        requestedState = 0;
                        break;
                    }
            }

            LabelCache labelCache = LabelCache.GetRuntimeCacheInstance();
            var Qtyreq = labelCache.GetLabelByName("isMaterialRequestStatusBoard_Qty_Requested");
            var DateReq = labelCache.GetLabelByName("isMaterialRequestStatusBoard_Date_Requested");
            var MfgOrderLabel = labelCache.GetLabelByName("Lbl_MfgOrder_Title");
            if (rsMaterialRequestStatusBoardDetails.Rows != null)
            {
                DataTable table = rsMaterialRequestStatusBoardDetails.GetAsDataTable();
                foreach(DataRow row in table.Rows)
                {
                    int state = int.Parse(GetDataRowColumnValue(row, "isMaterialRequestStateId", "0"));
                    if (state == requestedState)
                    {
                        string matQueueName = GetDataRowColumnValue(row, "isMaterialQueueName");
                        string inventoryLocation = GetDataRowColumnValue(row, "isInventoryLocationName");
                        string mfgOrder = GetDataRowColumnValue(row, "MfgOrderName");
                        string resourceName = GetDataRowColumnValue(row, "ResourceName");
                        if (string.IsNullOrEmpty(resourceName))
                            resourceName = GetDataRowColumnValue(row, "MaterialQueueResource");
                        string name = inventoryLocation;
                        if (!string.IsNullOrEmpty(matQueueName))
                        {
                            if (!string.IsNullOrEmpty(resourceName) && string.Compare(resourceName, matQueueName, true) != 0)
                                name = $"{matQueueName} ({resourceName})";
                            else
                                name = matQueueName;
                        }
                        else if (string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(resourceName))
                            name = resourceName;
                        if (!string.IsNullOrEmpty(name))
                        {
                            var tile = new CWC.TileContainer.TileContext
                            {
                                ColumnName = columnName,
                                Title = string.Format("{0}", name)
                            };
                            var strings = new List<string>();
                            string date = GetDataRowColumnValue(row, "RequestDateGMT");
                            DateTime requestedDate = DateTime.Parse(date).ToLocalTime();
                            string product = string.Format("{0}:{1}", GetDataRowColumnValue(row, "ProductName"), GetDataRowColumnValue(row, "ProductRevision"));
                            strings.Add(product);
                            strings.Add(GetDataRowColumnValue(row, "Description"));
                            strings.Add(string.Format(Qtyreq.Value.Trim() + "  {0} {1}", GetDataRowColumnValue(row, "QtyRequested"), GetDataRowColumnValue(row, "UOMName")));
                            strings.Add(string.Format(DateReq.Value.Trim() + " {0:d}", requestedDate.ToString("d")));
                            //if (!string.IsNullOrEmpty(mfgOrder))
                                strings.Add(string.Format(MfgOrderLabel.Value.Trim() + ": {0}", mfgOrder));
                            tile.Text = strings.ToArray();
                            tile.CustomData = date;
                            tiles.Add(tile);
                        }
                    }
                }
            }
            tiles.Sort((x, y) => string.Compare(x.CustomData, y.CustomData, StringComparison.CurrentCulture));
            return tiles;
        }
       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static string GetDataRowColumnValue(DataRow dr, string fieldName, string defaultValue = null)
        {
            string colVal = defaultValue;
            if (dr.Table != null && dr.Table.Columns.Contains(fieldName) && !dr.IsNull(fieldName))
                colVal = dr[fieldName].ToString();
            return colVal;
        }

        protected override IEnumerable<ScriptReference> GetScriptReferences()
        {
            yield return new ScriptReference("~/Scripts/User/Industry Solutions/materialrequeststatusboard.js");
        }

    }
}
