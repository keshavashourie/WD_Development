using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Web;
using System.Web.UI;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using OM = Camstar.WCF.ObjectStack;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using WebClientPortal;
using System.Runtime.Serialization;
using System.Text;

/// <summary>
/// Summary description for WIPChartWP
/// </summary>
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class WIPChartWP : MatrixWebPart
    {
        protected CWC.ChartControl RejectChart { get { return Page.FindCamstarControl("RejectChart") as CWC.ChartControl; } }
        protected CWC.ChartControl WipChart { get { return Page.FindCamstarControl("WipChart") as CWC.ChartControl; } }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!Page.IsPostBack)
            {
                RefreshAllCharts();
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            ScriptManager.RegisterStartupScript(this, this.GetType(), "initializeWIPChartWP", $"WIPChartWP.initialize();", true);
        }

        protected override IEnumerable<ScriptReference> GetScriptReferences()
        {
            yield return new ScriptReference("~/Scripts/User/WIPChartWP.js");
        }

        private bool VerifyDataTableExist(DataTable dt)
        {
            bool Valid = false;

            if (dt != null)
            {
                if (dt.Rows.Count > 0)
                {
                    Valid = true;
                }
            }

            return Valid;
        }

        protected virtual void RefreshAllCharts()
        {
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            //QueryUtil qUtil = new QueryUtil(session.CurrentUserProfile);
            QueryService qSvc = new QueryService(session.CurrentUserProfile);
            QueryService qSvc2 = new QueryService(session.CurrentUserProfile);
            //QueryParameter[] queryParameters = GetQueryParams();
            List<string> errors = new List<string>();
            string errMsg = null;

            errMsg = RefreshWipChart(GetQueryParams(new LineAssignmentItem[] { LineAssignmentItem.OPERATION, LineAssignmentItem.RESOURCE }), qSvc);
            if (!string.IsNullOrEmpty(errMsg))
                errors.Add(errMsg);


            errMsg = RefreshRejectChart(GetQueryParams(new LineAssignmentItem[] { LineAssignmentItem.OPERATION, LineAssignmentItem.WORKCENTER }), qSvc2);
            if (!string.IsNullOrEmpty(errMsg))
                errors.Add(errMsg);


            if (errors.Count > 0)
                Page.StatusBar.WriteError(errors[0]);


        }

        protected virtual QueryParameters GetQueryParams(LineAssignmentItem[] lineAssigns)
        {
            object lineAssignmentResource = Page.SessionDataContract.GetValueByName(Camstar.WebPortal.Constants.DataMemberConstants.Resource);
            object lineAssignmentOperation = Page.SessionDataContract.GetValueByName(Camstar.WebPortal.Constants.DataMemberConstants.Operation);
            object lineAssignmentWorkCenter = Page.SessionDataContract.GetValueByName(Camstar.WebPortal.Constants.DataMemberConstants.WorkCenter);

            string Resource = lineAssignmentResource != null ? lineAssignmentResource.ToString() : string.Empty;
            string Operation = lineAssignmentOperation != null ? lineAssignmentOperation.ToString() : string.Empty;
            string WorkCenter = lineAssignmentWorkCenter != null ? lineAssignmentWorkCenter.ToString() : string.Empty;


            QueryParameters queryParameters = new QueryParameters();
            queryParameters.Parameters = new QueryParameter[lineAssigns.Length];
            for (int i = 0; i < lineAssigns.Length; i++)
            {
                queryParameters.Parameters[i] = new QueryParameter();
                queryParameters.Parameters[i].Name = lineAssigns[i].ToString();
                var pValue = string.Empty;
                switch (lineAssigns[i])
                {
                    case LineAssignmentItem.RESOURCE:
                        pValue = Resource;
                        break;
                    case LineAssignmentItem.OPERATION:
                        pValue = Operation;
                        break;
                    case LineAssignmentItem.WORKCENTER:
                        pValue = WorkCenter;
                        break;
                }
                queryParameters.Parameters[i].Value = pValue;
            }
            return queryParameters;
        }

        protected virtual string RefreshWipChart(QueryParameters queryParams, QueryService qSvc)
        {
            RecordSet data = new RecordSet();
            ResultStatus status = new ResultStatus();
            string errMsg = null;

            QueryOptions oQueryOptions = new QueryOptions();
            oQueryOptions.QueryType = OM.QueryType.System;
            oQueryOptions.StartRow = 1;

            string queryName = null;
            if (queryName != null)
            {
                status = qSvc.Execute(queryName, queryParams, oQueryOptions, out data);
                if (status.IsSuccess)
                {
                    DataTable dt = data.GetAsDataTable();
                    if (VerifyDataTableExist(dt))
                    {
                        ScriptManager.RegisterStartupScript(this, this.GetType(), "wipChartNotEmpty", $"WIPChartWP.wipChartNotEmpty();", true);
                        WipChart.Data = dt;
                    }
                    else
                    {
                        ScriptManager.RegisterStartupScript(this, this.GetType(), "wipEmptyMessage", $"WIPChartWP.wipEmptyMessage();", true);
                        WipChart.ClearData();
                    }
                }
                else
                {
                    errMsg = status.ExceptionData.Description;
                }
            } else
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "wipEmptyMessage", $"WIPChartWP.wipEmptyMessage();", true);
                WipChart.ClearData();
            }

            return errMsg;
        }


        protected virtual string RefreshRejectChart(QueryParameters queryParams, QueryService qSvc)
        {
            RecordSet data = new RecordSet();
            ResultStatus status = new ResultStatus();
            string errMsg = null;

            QueryOptions oQueryOptions = new QueryOptions();
            oQueryOptions.QueryType = OM.QueryType.System;
            oQueryOptions.StartRow = 1;



            string queryName = null;
            if (queryName != null)
            {
                status = qSvc.Execute(queryName, queryParams, oQueryOptions, out data);
                if (status.IsSuccess)
                {
                    DataTable dt = data.GetAsDataTable();
                    if (VerifyDataTableExist(dt))
                    {
                        ScriptManager.RegisterStartupScript(this, this.GetType(), "rejectChartNotEmpty", $"WIPChartWP.rejectChartNotEmpty();", true);
                        RejectChart.Data = dt;
                    }
                    else
                    {
                        ScriptManager.RegisterStartupScript(this, this.GetType(), "rejectEmptyMessage", $"WIPChartWP.rejectEmptyMessage();", true);
                        RejectChart.ClearData();
                    }
                }
                else
                {
                    errMsg = status.ExceptionData.Description;
                }
            } else
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "rejectEmptyMessage", $"WIPChartWP.rejectEmptyMessage();", true);
                RejectChart.ClearData();
            }

            return errMsg;
        }


        public static bool RefreshBarChart(AjaxTransition transition)
        {

            ClientControllerState parms = null;

            using (Stream str = new MemoryStream(Encoding.UTF8.GetBytes(transition.CommandParameters)))
            {
                var ser = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(ClientControllerState));
                parms = ser.ReadObject(str) as ClientControllerState;
            }

            object lineAssignmentResource = null;
            object lineAssignmentOperation = null;
            object lineAssignmentWorkCenter = null;
            ApolloPortalService appolloSvc = new ApolloPortalService();
            var appolloSettings = new ApolloPortalService.ApolloSettings();
            ResultStatus status = appolloSvc.GetApolloSettings(out appolloSettings);
            if (status.IsSuccess)
            {
                object resource = appolloSettings.Resource;
                object operation = appolloSettings.Operation;
                object workcenter = appolloSettings.Workcenter;
                object workstation = appolloSettings.Workstation;

                //Retrieve Line Assigment Data
                lineAssignmentResource = (resource != null ? resource.ToString() : String.Empty);
                lineAssignmentOperation = (operation != null ? operation.ToString() : String.Empty);
                lineAssignmentWorkCenter = (workcenter != null ? workcenter.ToString() : String.Empty);
            }

            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            QueryService oService = new QueryService(session.CurrentUserProfile);
            QueryParameters objQueryParameters = new QueryParameters();

            RecordSet recordSet = new RecordSet();
            status = new ResultStatus();

            string queryName = null;

            objQueryParameters.Parameters = new QueryParameter[2];

            objQueryParameters.Parameters[0] = new QueryParameter();
            objQueryParameters.Parameters[0].Name = LineAssignmentItem.OPERATION.ToString();
            objQueryParameters.Parameters[0].Value = lineAssignmentOperation.ToString();

            if (parms.Type == "Reject")
            {
                objQueryParameters.Parameters[1] = new QueryParameter();
                objQueryParameters.Parameters[1].Name = LineAssignmentItem.WORKCENTER.ToString();
                objQueryParameters.Parameters[1].Value = lineAssignmentWorkCenter.ToString();
                queryName = null;
            }
            else
            {
                objQueryParameters.Parameters[1] = new QueryParameter();
                objQueryParameters.Parameters[1].Name = LineAssignmentItem.RESOURCE.ToString();
                objQueryParameters.Parameters[1].Value = lineAssignmentResource.ToString();
                queryName = null;
            }

            string txnData = null;
            if (queryName == null)
            {
                QueryOptions queryOptions = new QueryOptions()
                {
                    QueryType = WCF.ObjectStack.QueryType.System,
                    ChangeCount = 0
                };

                status = oService.Execute(queryName, objQueryParameters, queryOptions, out recordSet);

               

                if (recordSet.Rows != null)
                {
                    var chartData = new List<BarChartContext>();
                    var serializer = new JavaScriptSerializer();

                    foreach (Row row in recordSet.Rows)
                    {
                        var dataContext = new BarChartContext
                        {
                            x = row.Values[0],
                            y = row.Values[1]
                        };
                        chartData.Add(dataContext);

                    }

                    txnData = JsonConvert.SerializeObject(chartData, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                }
            }

            transition.Response = new[] { new ResponseSection(ResponseType.Command, transition.ID, txnData) };

            return true;
        }

        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as Camstar.WebPortal.Personalization.CustomAction;

            if (action != null && action.Parameters == "Refresh")
            {
                RefreshAllCharts();
            }
        }

        protected enum LineAssignmentItem
        {
            RESOURCE,
            OPERATION,
            WORKCENTER
        }

        #region Public Class
        [DataContract]
        public class ClientControllerState
        {
            [DataMember]
            public string Type { get; set; }
        }


        public class BarChartContext
        {
            public string x { get; set;}
            public string y { get; set; }
        }
        #endregion
    }
}