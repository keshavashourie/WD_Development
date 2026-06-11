
using System;
using System.Linq;
using System.Web;

using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;

using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.Utilities;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.SPCChart2;
/// <summary>
/// Summary description for SPCChartModeling
/// </summar
namespace Camstar.WebPortal.WebPortlets.Modeling
{
    public class SPCChartModeling : MatrixWebPart
    {
        #region Properties
        private NamedObject SPCChartType { get { return Page.FindCamstarControl("SPCChartType") as CWC.NamedObject; } }

        private NamedObject SPCQuery { get { return Page.FindCamstarControl("ObjectChanges_SPCQuery") as CWC.NamedObject; } }

        private NamedObject SPCConnection { get { return Page.FindCamstarControl("ObjectChanges_SPCConnection") as CWC.NamedObject; } }
        private JQDataGrid SPCChartTypeParams { get { return Page.FindCamstarControl("Details_Params") as JQDataGrid; } }
        private JQDataGrid SPCChartTypeVisualParams { get { return Page.FindCamstarControl("ObjectChanges_SPCChartVisualParams") as JQDataGrid; } }
        private JQDataGrid SPCQueryParams { get { return Page.FindCamstarControl("SPCQueryParams") as JQDataGrid; } }
        private ToggleContainer Params { get { return Page.FindCamstarControl("ParametersGroupToggle") as ToggleContainer; } }
        private ToggleContainer ParamsSPC { get { return Page.FindCamstarControl("SPCParametersGroupToggle") as ToggleContainer; } }

        private NamedObject SPCRules { get { return Page.FindCamstarControl("SPCRules") as NamedObject; } }

        private string MacroQuery { get; set; }
        private Dictionary<string, string> QueryParams { get; set; }

        private Camstar.WCF.ObjectStack.UserProfile UserProfile
        {
            get
            {
                return HttpContext.Current.Session[Camstar.WebPortal.Constants.SessionConstants.UserProfile] as Camstar.WCF.ObjectStack.UserProfile;
            }
        }

        protected virtual TextBox ChartVariable
        { get { return Page.FindCamstarControl("ChartVariable") as TextBox; } }

        protected virtual TextBox ChartType
        { get { return Page.FindCamstarControl("ChartType") as TextBox; } }

        protected virtual FileBrowse ChartMacroField
        { get { return Page.FindCamstarControl("ChartMacro") as FileBrowse; } }

        protected virtual FileBrowse ChartProperties
        { get { return Page.FindCamstarControl("ChartProperties") as FileBrowse; } }

        protected virtual TextBox ChartHeight
        { get { return Page.FindCamstarControl("ChartHeight") as TextBox; } }
        protected virtual TextBox ChartWidth
        { get { return Page.FindCamstarControl("ChartWidth") as TextBox; } }


        protected virtual CheckBox IsNewSPC
        { get { return Page.FindCamstarControl("ObjectChanges_IsNewSPC") as CheckBox; } }

        protected virtual DropDownList LegendLocation { get { return Page.FindCamstarControl("LegendLocation") as DropDownList; } }
        protected virtual DropDownList SPCChartSaveOptions { get { return Page.FindCamstarControl("SPCChartSaveOptions") as CWC.DropDownList; } }

        private Dictionary<string, string> resultStatitCommands;


        #endregion
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            IsNewSPC.DataChanged += IsNewSPC_DataChanged;
            SPCChartType.DataChanged += new EventHandler(DetailsSPCChart_DataChanged);
            SPCQuery.DataChanged += new EventHandler(SPCQuery_DataChanged);
            ChartMacroField.DataChanged += SPCChartModeling_DataChanged;

            if (Page.EventArgument == "FloatingFrameSubmitParentPostBackArgument" && Page.DataContract.GetValueByName("QueryName") != null)
            {
                SPCQuery.Data = Page.PortalContext.LocalSession["QueryName"].ToString();
                var statitCommands = Page.PortalContext.LocalSession["StatitCommands"] as SPCChartParamsChanges[];
                UpdateSPCChartParamsFromPopup(statitCommands);
            }
        }

        private void SPCChartModeling_DataChanged(object sender, EventArgs e)
        {
            if (ChartMacroField.Data != null)
            {
                string yesFunction = string.Format(@"ConfirmMacroFileParsinfForSPC('{0}')", IsNewSPC.ClientID);
                string noFunction = string.Format(@"CancelMacroFileParsinfForSPC('{0}', '{1}')", IsNewSPC.ClientID, ChartMacroField.ClientID);
                var labelCache = FrameworkManagerUtil.GetLabelCache(HttpContext.Current.Session);
                var messageLable = labelCache.GetLabelByName("Lbl_SPCMacroParsingConfirmationMessage");
                IsNewSPC.Attributes["onclick"] = ControlUtil.MakeConfirmation(
                        new Confirmation
                        {
                            OK_LabelName = "Web_Yes",
                            Cancel_LabelName = "Web_No",
                            Message_LabelText = String.Format(messageLable.Value, GetMacroFilesName(ChartMacroField.Data.ToString())),
                            Title_LabelName = "StatusMessage_Warning"
                        },
                        labelCache,
                        yesFunction,
                        noFunction);
                CamstarWebControl.SetRenderToClient(IsNewSPC);
            }
            else
            {
                IsNewSPC.Attributes.Remove("onclick");
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            ClearsControlsData();
        }
        private void InitSPCConnectionSelVal()
        {
            var result = new ResultStatus("", true);
            var serv = new SPCConnectionMaintService(UserProfile);
            var maint = new SPCConnectionMaint();
            SPCConnectionMaint_Request req = new SPCConnectionMaint_Request();
            req.Info = new SPCConnectionMaint_Info()
            {
                ObjectListInquiry = new Info()
                {

                    RequestSelectionValues = true
                }
            };

            SPCConnectionMaint_Result res;
            result = serv.GetEnvironment(maint, req, out res);
            if (result.IsSuccess && res.Environment != null && res.Environment.ObjectListInquiry != null && res.Environment.ObjectListInquiry.SelectionValues != null && res.Environment.ObjectListInquiry.SelectionValues.Rows != null)
            {
                var util = new WCFUtilities.SelectionValuesExUtil(res.Environment.ObjectListInquiry.SelectionValues);
                Row[] pageRow = res.Environment.ObjectListInquiry.SelectionValues.Rows;
                if (pageRow.Count() == 1)
                {
                    SPCConnection.Data = pageRow[0].Values.FirstOrDefault();
                }
            }
        }

        private void InitSPCChartTypeSelVal(string value)
        {
            var result = new ResultStatus("", true);
            var serv = new SPCChartTypeMaintService(UserProfile);
            var maint = new SPCChartTypeMaint();
            var req = new SPCChartTypeMaint_Request();

            req.Info = new SPCChartTypeMaint_Info()
            {
                ObjectListInquiry = new Info()
                {

                    RequestSelectionValues = true
                }
            };

            SPCChartTypeMaint_Result res;
            result = serv.GetEnvironment(maint, req, out res);
            if (result.IsSuccess && res.Environment != null && res.Environment.ObjectListInquiry != null && res.Environment.ObjectListInquiry.SelectionValues != null && res.Environment.ObjectListInquiry.SelectionValues.Rows != null)
            {
                var util = new WCFUtilities.SelectionValuesExUtil(res.Environment.ObjectListInquiry.SelectionValues);
                Row[] pageRow = res.Environment.ObjectListInquiry.SelectionValues.Rows;

                SPCChartType.Data = pageRow?.FirstOrDefault(x => x.Values?.FirstOrDefault() == value)?.Values?.FirstOrDefault();

            }
        }

        private void ChangeControlState(FieldControl control, bool state, bool required)
        {
            control.Visible = state;
            control.Data = state ? control.Data : null;
            control.Required = required;
        }

        protected virtual void IsNewSPC_DataChanged(object sender, EventArgs e)
        {

            if (!(bool)IsNewSPC.Data)
            {
                ClearsControlsData();
                return;
            }

            if (ChartType.Data != null)
            {
                var value = SPCHelper.LegacyChartTypeToInline(ChartType.Data.ToString());
                if (value != null)
                {
                    InitSPCChartTypeSelVal(value);
                }

            }

            if (SPCConnection.Data == null)
            {
                InitSPCConnectionSelVal();
            }

            (SPCQueryParams.GridContext as BoundContext).Data = GetResultQueryParams();
            SPCQueryParams.GridContext.LoadData();

            CamstarWebControl.SetRenderToClient(SPCQueryParams);

            if (ChartMacroField.Data != null)
            {
                ReadMacroFile();

                string macroFilePath = ChartMacroField.Data.ToString();

                var userParameters = GetUserParams();

                if (userParameters.Count > 0 && QueryParams != null)
                {
                    foreach (var parameters in userParameters)
                    {
                        if (QueryParams.ContainsKey(parameters.Key))
                            QueryParams[parameters.Key] = parameters.Value;
                    }
                }

                ClearsControlsData();
                IsNewSPC.Attributes.Remove("onclick");
                OpenSPCMacroParsingPopup(macroFilePath);
            }

            ClearsControlsData();

        }
        private void StatitCommandsParser(string fileString)
        {
            foreach (var statitChartType in SPCHelper.ChartTypes.Keys)
            {
                var elementIndex = fileString.IndexOf(statitChartType, StringComparison.CurrentCultureIgnoreCase);

                if (elementIndex > 0)
                {
                    if (!SPCHelper.StatitToInlineChartTypes.TryGetValue(statitChartType, out string value))
                    {
                        return;

                    }
                    else
                    {
                        resultStatitCommands = new Dictionary<string, string>();
                        resultStatitCommands.Add("ControlChartType", value);

                        var statitCommands = fileString.Substring(elementIndex).Substring(0).Replace("\r\n", " ");

                        ParseStatitCommands(statitCommands);
                    }

                }
            }
        }
        private void ParseStatitCommands(string statitCommands)
        {
            foreach (var command in SPCHelper.ExistStatitCommands)
            {
                var value = Regex.Match(statitCommands, String.Format("{0}(.+?){1}", command.Key, " "), RegexOptions.Singleline | RegexOptions.IgnoreCase).Groups[1].Value;
                if (string.Equals(command.Key, "by", StringComparison.CurrentCultureIgnoreCase))
                {
                    resultStatitCommands.Add(command.Value, value);
                }
                else if (!string.IsNullOrEmpty(value))
                {
                    var paramValue = value.Replace("=", String.Empty);
                    if (SPCHelper.ExistStatitCommands.TryGetValue(command.Key, out string res))
                    {
                        resultStatitCommands.Add(res, paramValue);
                        var removeAddedParameter = "/" + command.Key + "=" + paramValue;
                        statitCommands = statitCommands.Replace(removeAddedParameter, "");
                    }
                }
            }
        }
        private void ReadMacroFile()
        {
            try
            {

                using (StreamReader sr = new StreamReader(ChartMacroField.Data.ToString()))
                {
                    var fileString = sr.ReadToEnd();
                    QueryParser(fileString);
                    StatitCommandsParser(fileString);
                }

            }
            catch (Exception ex)
            {
                Page.DisplayMessage(ex.Message, false);
            }
        }
        private void QueryParser(string fileString)
        {

            var fileArray = fileString.Split('\n');
            string commentExceptString = null;

            for (int i = 0; i < fileArray.Length; i++)
            {
                var elementIndex = fileArray[i].IndexOf("##");
                if (elementIndex != -1)
                {
                    fileArray[i] = String.Empty;
                }
                commentExceptString += fileArray[i];
            }

            var queries = Regex.Matches(commentExceptString, @"Beginsql(.+?)Endsql", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            if (queries == null)
            {
                return;
            }

            string query = null;
            if (queries.Count > 1)
            {

                var labelCache = FrameworkManagerUtil.GetLabelCache(HttpContext.Current.Session);
                var messageLable = labelCache.GetLabelByName("Lbl_SPCWarningConversionMessage");

                Page.DisplayWarning(messageLable.Value);

                foreach (Match partOfQuery in queries)
                {

                    query += partOfQuery.Groups[1].Value + '\n';

                }

            }
            else
            {
                query = queries[0].Groups[1].Value;

            }

            query = Regex.Replace(query, @"TxnID", "DataID", RegexOptions.IgnoreCase);
            query = Regex.Replace(query, @"{%Metric%}", "Thickness", RegexOptions.IgnoreCase);
            query = Regex.Replace(query, @"'Thickness'", "{%Metric%}", RegexOptions.IgnoreCase);

            if (ChartVariable.Data != null)
            {
                query = Regex.Replace(query, @"{%VarToChart%}|'{%VarToChart%}'", ChartVariable.Data.ToString(), RegexOptions.IgnoreCase);

            }
            else
            {
                query = Regex.Replace(query, @"{%VarToChart%}|'{%VarToChart%}'", "Thickness", RegexOptions.IgnoreCase);
            }

            var matches = Regex.Matches(query, @"{%(.+?)%}|'{%(.+?)%}'");

            var resultQuery = new StringBuilder(query);

            QueryParams = new Dictionary<string, string>();

            for (int i = 0; i < matches.Count; i++)
            {
                var paramName = Regex.Replace(matches[i].Value, @"%|{|}|'", "");

                if (!QueryParams.ContainsKey(paramName))
                {
                    QueryParams.Add(paramName, "");
                    string val = "?" + paramName;

                    resultQuery.Replace(matches[i].Value, val);
                }
            }


            var matchQuotes = Regex.Matches(resultQuery?.ToString(), @"'(.+?)'");

            for (int i = 0; i < matchQuotes.Count; i++)
            {
                var val = Regex.Replace(matchQuotes[i].Value, @"'", "");

                resultQuery.Replace(matchQuotes[i].Value, val);
            }


            MacroQuery = resultQuery?.ToString().TrimStart();

        }

        private void ClearsControlsData()
        {
            var isNewSPC = (bool)IsNewSPC.Data;

            ChangeControlState(ChartType, !isNewSPC, !isNewSPC);
            ChangeControlState(ChartMacroField, !isNewSPC, !isNewSPC);
            ChangeControlState(ChartProperties, !isNewSPC, false);
            ChangeControlState(SPCChartType, isNewSPC, isNewSPC);
            ChangeControlState(SPCQuery, isNewSPC, isNewSPC);
            ChangeControlState(SPCConnection, isNewSPC, isNewSPC);
            ChangeControlState(SPCRules, isNewSPC, false);
            ChangeControlState(LegendLocation, isNewSPC, isNewSPC);
            ChangeControlState(SPCChartSaveOptions, isNewSPC, false);

            SPCChartSaveOptions.Visible = isNewSPC;
            SPCChartTypeParams.Visible = isNewSPC;
            SPCQueryParams.Visible = isNewSPC;
            if (SPCChartTypeVisualParams != null)
            {
                SPCChartTypeVisualParams.Visible = isNewSPC;
            }
            Params.Visible = !isNewSPC;
            ParamsSPC.Visible = isNewSPC;

            if (isNewSPC)
            {

                Page.FindCamstarControls<TextBox>().ForEach(UserParm =>
                {
                    if (UserParm.ID.StartsWith("UserParm"))
                    {
                        UserParm.ClearData();
                    }
                });

            }
            else
            {
                SPCChartTypeParams.ClearData();
                SPCQueryParams.ClearData();
                SPCChartTypeVisualParams?.ClearData();
            }
            CamstarWebControl.SetRenderToClient(Params);
            CamstarWebControl.SetRenderToClient(ParamsSPC);
            if (SPCChartTypeVisualParams != null)
            {
                CamstarWebControl.SetRenderToClient(SPCChartTypeVisualParams);
            }
        }
        private SPCChartQueryParamsChanges[] GetResultQueryParams()
        {
            var userParams = GetUserParams();

            var spcChartQueryParams = (SPCQueryParams.GridContext as BoundContext).Data as SPCChartQueryParamsChanges[];

            if (spcChartQueryParams == null || userParams == null || userParams.Count == 0)
            {
                return spcChartQueryParams;
            }

            var removeList = new HashSet<string>();
            var resultParams = new List<SPCChartQueryParamsChanges>(spcChartQueryParams);

            foreach (var item in resultParams)
            {
                userParams.TryGetValue(item.ParamName?.ToString(), out var newvalue);

                if (newvalue != null)
                {
                    item.ParamValue = newvalue;
                    removeList.Add(item.ParamName?.ToString());
                }

            }

            resultParams.AddRange(userParams.Where(x => !removeList.Contains(x.Key)).Select(x => new SPCChartQueryParamsChanges()
            {
                DataType = DataTypeEnum.String,
                ParamName = x.Key,
                ParamValue = x.Value

            }));

            return resultParams?.ToArray();
        }
        private Dictionary<string, string> GetUserParams()
        {
            var userParams = Page.FindCamstarControls<TextBox>()
            .Where(up => up.ID.StartsWith("UserParm") && up.Data != null)
            .Select(x => x.Data as string)
            .Select(y => y.Split('='))
            .Select(m => new { Key = m.First().Trim(), Value = m.Last().Trim() })
            .GroupBy(x => x.Key)
            .ToDictionary(x => x.Key, y => y.First().Value);

            return userParams;
        }



        protected void SPCQuery_DataChanged(object sender, EventArgs e)
        {

            if (SPCQuery.Data == null)
            {
                SPCQueryParams.ClearData();
                return;
            }

            var uQMS = new UserQueryMaintService(UserProfile);

            var uQueryMaint = new UserQueryMaint
            {
                ObjectToChange = new NamedObjectRef(SPCQuery.Data.ToString()),
                ObjectChanges = new UserQueryChanges
                {
                    Name = SPCQuery.Data.ToString()
                }
            };


            var request = new UserQueryMaint_Request
            {
                Info = new UserQueryMaint_Info
                {
                    ObjectChanges = new UserQueryChanges_Info
                    {
                        UserQueryParameters = new UserQueryParameterChanges_Info
                        {
                            DataType = FieldInfoUtil.RequestValue(),
                            DefaultValue = FieldInfoUtil.RequestValue(),
                            Name = FieldInfoUtil.RequestValue(),
                            DynamicValue = FieldInfoUtil.RequestValue()

                        }
                    }
                }
            };

            ResultStatus oRS = uQMS.Load(uQueryMaint, request, out var oResult);

            UserQueryParameterChanges[] uQueryParams = oResult?.Value?.ObjectChanges?.UserQueryParameters;
            if (oRS.IsSuccess && uQueryParams != null)
            {

                var spcChartQueryParams = uQueryParams
                .Select(p => new SPCChartQueryParamsChanges
                {
                    DataType = p.DataType,
                    ParamValue = p.DefaultValue,
                    ParamName = p.Name,
                    DynamicValue = p.DynamicValue
                })
                .ToArray();

                (SPCQueryParams.GridContext as BoundContext).Data = spcChartQueryParams;
                SPCQueryParams.GridContext.LoadData();

                CamstarWebControl.SetRenderToClient(SPCQueryParams);
            }


        }

        protected void DetailsSPCChart_DataChanged(object sender, EventArgs e)
        {

            if (SPCChartType.Data == null)
            {
                SPCChartTypeParams.ClearData();
                SPCChartTypeVisualParams?.ClearData();
                return;
            }
            var oSvc = new SPCChartTypeMaintService(UserProfile);

            var oChartTypeChanges = new SPCChartTypeChanges
            {
                Name = SPCChartType.Data.ToString()
            };


            var oChartTypeChangesInfo = new SPCChartTypeChanges_Info
            {
                Params = new SPCChartTypeParamsChanges_Info
                {
                    ParamName = FieldInfoUtil.RequestValue(),
                    DefaultValue = FieldInfoUtil.RequestValue(),
                    IsDynamic = FieldInfoUtil.RequestValue()
                },
                VisualParams = new SPCChartVisualParamsChanges_Info
                {
                    ParamName = FieldInfoUtil.RequestValue(),
                    ParamValue = FieldInfoUtil.RequestValue(),
                    DisplayName = FieldInfoUtil.RequestValue()
                }
            };

            var oChartType = new SPCChartTypeMaint
            {
                ObjectToChange = new NamedObjectRef(SPCChartType.Data.ToString()),
                ObjectChanges = oChartTypeChanges
            };

            var oChartTypeInfo = new SPCChartTypeMaint_Info
            {
                ObjectChanges = oChartTypeChangesInfo
            };

            SPCChartTypeMaint_Result oResult = new SPCChartTypeMaint_Result();
            ResultStatus oRS = oSvc.Load(oChartType, new SPCChartTypeMaint_Request { Info = oChartTypeInfo }, out oResult);

            var oChartTypeParams = oResult?.Value?.ObjectChanges?.Params;
            var oChartTypeVisualParams = oResult?.Value?.ObjectChanges?.VisualParams;

            if (oRS.IsSuccess)
            {
                if (oChartTypeParams != null)
                {
                    var oChartDefParams = oChartTypeParams.Select(y => new SPCChartParamsChanges
                    {
                        ParamName = y.ParamName,
                        ParamValue = y.DefaultValue,
                        IsDynamic = y.IsDynamic

                    }).ToArray();

                    (SPCChartTypeParams.GridContext as BoundContext).Data = oChartDefParams;
                    SPCChartTypeParams.GridContext.LoadData();
                }
                else
                {
                    SPCChartTypeParams.ClearData();
                }

                CamstarWebControl.SetRenderToClient(SPCChartTypeParams);

                if (SPCChartTypeVisualParams != null)
                {
                    if (oChartTypeVisualParams != null)
                    {
                        var visualParamsData = oChartTypeVisualParams.Select(v => new SPCChartVisualParamsChanges
                        {
                            ParamName = v.ParamName,
                            ParamValue = v.ParamValue,
                            DisplayName = v.DisplayName
                        }).ToArray();

                        (SPCChartTypeVisualParams.GridContext as BoundContext).Data = visualParamsData;
                        SPCChartTypeVisualParams.GridContext.LoadData();
                    }
                    else
                    {
                        SPCChartTypeVisualParams.ClearData();
                    }

                    CamstarWebControl.SetRenderToClient(SPCChartTypeVisualParams);
                }
            }
        }

        protected void OpenSPCMacroParsingPopup(string path)
        {
            FloatPageOpenAction floatAction = new FloatPageOpenAction();
            floatAction.PageName = "SPCChartMacroParsingPopup_VP";
            floatAction.ShowButtons = true;
            floatAction.EndResponse = false;

            string queryName = GetMacroFilesName(path);

            Page.DataContract.SetValueByName("StatitCommands", resultStatitCommands);
            Page.DataContract.SetValueByName("QueryParams", QueryParams);
            Page.DataContract.SetValueByName("MacroQuery", MacroQuery);
            Page.DataContract.SetValueByName("QueryName", queryName);

            Page.ActionDispatcher.ExecuteAction(floatAction);
        }

        protected string GetMacroFilesName(string path)
        {
            return path.Substring(path.LastIndexOf('\\') + 1, (path.Length - path.LastIndexOf('\\') - 5));
        }

        protected void UpdateSPCChartParamsFromPopup(SPCChartParamsChanges[] commands)
        {
            if (commands.Length == 0) return;

            var chartType = commands.Where(c => c.ParamName == "ControlChartType").FirstOrDefault();

            if (chartType != null)
                SPCChartType.Data = SPCHelper.ControlChartTypeToInlineType(chartType.ParamValue.ToString());
            var gridData = SPCChartTypeParams.Data as SPCChartParamsChanges[];
            foreach (var command in commands)
            {
                var param = gridData.Where(p => p.ParamName == command.ParamName).FirstOrDefault();
                if (param != null)
                    param.ParamValue = command.ParamValue;
                else
                {
                    Array.Resize(ref gridData, gridData.Length + 1);
                    gridData[gridData.Length - 1] = command;
                }

            }
            (SPCChartTypeParams.GridContext as BoundContext).Data = gridData;
            SPCChartTypeParams.GridContext.LoadData();
            CamstarWebControl.SetRenderToClient(SPCChartTypeParams);
        }
    }
}