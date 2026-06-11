// Copyright Siemens 2023  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.Personalization;
using Control = System.Web.UI.Control;

namespace Camstar.WebPortal.WebPortlets.Modeling
{
    public class UserQueryTestPopup : MatrixWebPart
    {

        protected virtual JQDataGrid UserQueryResultGrid { get { return Page.FindCamstarControl("AdHocQueryGrid") as JQDataGrid; } }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            _queryParams = Page.DataContract.GetValueByName("UserQueryParamsDM") as QueryParameters;
            _queryName = Page.DataContract.GetValueByName("UserQueryNameDM").ToString();
            var label = Page.DataContract.GetValueByName("label") as string;
            bool.TryParse(Page.DataContract.GetValueByName("isSPCQuery")?.ToString(), out _isSPCQuery);

            if (label != null)
               Page.Title = label;
            else
              Page.Title = "User Query: " + _queryName;

            if (_queryParams != null && _queryParams.Parameters.Count() > 0)
                AddParamControls(_queryParams);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            //if no queryParams execute query immediate
            if (_queryParams == null || _queryParams.Parameters.Count() == 0)
                (UserQueryResultGrid.Settings as GridDataSettingsAdHocQuery).QueryText = Page.DataContract.GetValueByName("QueryTxt").ToString();

        }

        public virtual void TestUserQuery()
        {
            if (UserQueryResultGrid.Data != null)
                UserQueryResultGrid.ClearData();

            Page.StatusBar.ClearMessage();

            ExecuteQuery();
        }

        protected virtual void ExecuteQuery()
        {
            if (_paramControlList.Where(c => c.Data == null).Count() > 0)
            {
                if (labelCache == null)
                    labelCache = FrameworkManagerUtil.GetLabelCache(Page.Session);
                DisplayMessage(new ResultStatus(labelCache.GetLabelByName("UserQueryMaintInvalidParameters").Value, false));

                return;
            }

            var qService = (Page as IForm).Service.GetService<QueryService>();
            RecordSet myDataSet;
            ResultStatus res;

            if (_queryParams != null && _queryParams.Parameters.Count() > 0)
                foreach (QueryParameter qp in _queryParams.Parameters)
                    qp.Value = _paramControlList.Where(c => c.ID == "_parameter_" + qp.Name).Select(c => c.Data.ToString()).FirstOrDefault();

            QueryOptions _queryOptions = new QueryOptions()
            {
                QueryType = Camstar.WCF.ObjectStack.QueryType.User,
                ChangeCount = 0
            };
            var queryText = Page.DataContract.GetValueByName("QueryTxt").ToString();
            ParameterDataTypeMap = Page.DataContract.GetValueByName<Dictionary<string, int?>>("ParameterTypeMap");

            StringBuilder queryStringBuilder = new StringBuilder(queryText);
            if (_queryParams != null && _queryParams.Parameters.Count() > 0)
                queryStringBuilder = new StringBuilder(ReplaceQueryParamsWithValues(_queryParams.Parameters, queryText));

            if(_isSPCQuery)
                res = qService.ExecuteAdHoc(queryStringBuilder.ToString(), _queryOptions, out myDataSet);
            else
                res = qService.Execute(_queryName, _queryParams, _queryOptions, out myDataSet);

            if (res.IsSuccess)
            {
                if ((myDataSet as RecordSet).Rows == null || (myDataSet as RecordSet).Rows.Count() == 0)
                {
                    if (labelCache == null)
                        labelCache = FrameworkManagerUtil.GetLabelCache(Page.Session);
                    DisplayMessage(new ResultStatus(labelCache.GetLabelByName("Lbl_NoDataToDisplay").Value, false));
                }

                (UserQueryResultGrid.Settings as GridDataSettingsAdHocQuery).QueryText = queryStringBuilder.ToString();

            }
            else
                DisplayMessage(res);
        }

        private string ReplaceQueryParamsWithValues(QueryParameter[] queryParams, string queryText)
        {
            foreach (QueryParameter q in queryParams)
            {
                int pos = GetNextParameterIndex(queryText, "?" + q.Name, 0);
                while (pos != -1)
                {
                    if (queryText.Length > (pos + q.Name.Length + 1))
                    {
                        if (!isAlphaNumeric(queryText.Substring(pos + q.Name.Length + 1, 1)))
                        {
                            string before = queryText.Substring(0, pos);

                            var splitInput = before.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
                            var input = splitInput.Last().Trim();
                            string after = queryText.Substring(pos + q.Name.Length + 1);
                            if (string.Equals(input, "as", StringComparison.CurrentCultureIgnoreCase) && _isSPCQuery)
                            {
                                queryText = before + q.Value?.ToString() + after;
                            }
                            else
                            {
                                queryText = before + FormatQueryParameter(q) + after;
                            }
                        }
                    }
                    else // parameter is at end of query;
                    {
                        string before = queryText.Substring(0, pos);
                        var splitInput = before.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
                        var input = splitInput.Last().Trim();
                        string after = queryText.Substring(pos + q.Name.Length + 1);
                        if (string.Equals(input, "as", StringComparison.CurrentCultureIgnoreCase) && _isSPCQuery)
                        {
                            queryText = before + q.Value?.ToString();
                        }
                        else
                        {
                            queryText = before + FormatQueryParameter(q);
                        }
                    }

                    pos = GetNextParameterIndex(queryText, "?" + q.Name, pos);
                }
            }

            return queryText;
        }

        private string FormatQueryParameter(QueryParameter param)
        {
            int? datatype = ParameterDataTypeMap[param.Name];
            string value = param.Value;

            if (datatype == 4 || datatype == 5 || datatype == 6)
            {
                value = "'" + value + "'";
                int byteCount = Encoding.UTF8.GetByteCount(value);
                if (byteCount != value.Length)
                    value = "N" + value;
            }

            return value;
        }

        private bool isAlphaNumeric(string str)
        {
            return str.All(char.IsLetterOrDigit);
        }

        private int GetNextParameterIndex(string queryText, string param, int startIndex)
        {
            for (int i = startIndex; i < queryText.Length; i++)
            {
                if ((i + param.Length) < queryText.Length)
                {
                    string var = queryText.Substring(i, param.Length);
                    if (param.Equals(var))
                    {
                        var = queryText.Substring(i + param.Length, 1);
                        if (!isAlphaNumeric(var))
                            return i;
                    }
                }
                else if ((i + param.Length) == queryText.Length)
                {
                    string var = queryText.Substring(i, param.Length);
                    if (param.Equals(var))
                        return i;
                }
            }

            return -1;
        }

        protected virtual void AddParamControls(QueryParameters queryParams)
        {
            this.ControlAlignment = ControlAlignmentType.LabelLeftInputRight;
            BaseFieldExpression = ".";

            foreach (QueryParameter qp in queryParams.Parameters)
            {
                TextBox _paramControl = new TextBox();
                CreateField(_paramControl as Control, "");
                _paramControl.ID = "_parameter_" + qp.Name;
                _paramControl.LabelText = qp.Name;
                _paramControl.Data = qp.Value;
                _paramControl.LabelPosition = LabelPositionType.Top;
                _paramControl.Margin = new Margin() { Right = 15 };
                _paramControlList.Add(_paramControl);
                this[1, 0] = _paramControl;

            }
        }

        protected List<TextBox> _paramControlList = new List<TextBox>();
        private QueryParameters _queryParams;
        private string _queryName;
        LabelCache labelCache;
        private Dictionary<string, int?> ParameterDataTypeMap;
        private bool _isSPCQuery = false;

    }
}
