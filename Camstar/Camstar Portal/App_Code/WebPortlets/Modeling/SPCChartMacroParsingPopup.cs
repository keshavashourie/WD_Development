// Copyright Siemens 2023  
using System;
using System.Collections.Generic;
using System.Linq;
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.SPCChart2;

namespace Camstar.WebPortal.WebPortlets.Modeling
{
    public class SPCChartMacroParsingPopup : MatrixWebPart
    {
        #region properties
        protected virtual Button SubmitMacroParsing { get { return Page.FindCamstarControl("SubmitMacroParsing") as Button; } }
        protected virtual TextBox SPCMacroQuery { get { return Page.FindCamstarControl("SPCMacroQuery") as TextBox; } }
        protected virtual TextBox SPCQueryName { get { return Page.FindCamstarControl("SPCQueryName") as TextBox; } }
        protected virtual JQDataGrid SPCQueryParamGrid { get { return Page.FindCamstarControl("SPCQueryParam") as JQDataGrid; } }
        protected virtual JQDataGrid StatitComandsGrid { get { return Page.FindCamstarControl("StatitComands") as JQDataGrid; } }
        #endregion

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            SubmitMacroParsing.Click += SubmitMacroParsing_Click;

            if (!Page.IsPostBack)
            {
                FillData();
            }
        }

        #region private functions
        private DataTypeEnum GetTypeOfParametersValue(string value)
        {
            if (value == null) return DataTypeEnum.String;
            int intValue;
            bool boolValue;
            decimal decimalValue;
            if (Int32.TryParse(value, out intValue))
                return DataTypeEnum.Integer;
            else if (Boolean.TryParse(value, out boolValue))
                return DataTypeEnum.Boolean;
            else if (Decimal.TryParse(value, out decimalValue))
                return DataTypeEnum.Decimal;
            else
                return DataTypeEnum.String;
        }

        private void FillData()
        {
            var queryParamsDict = Page.DataContract.GetValueByName("QueryParams") as Dictionary<string, string>;
            var statitCommandDict = Page.DataContract.GetValueByName("StatitCommands") as Dictionary<string, string>;
            var query = Page.DataContract.GetValueByName("MacroQuery")?.ToString();
            var queryName = Page.DataContract.GetValueByName("QueryName")?.ToString();

            SPCMacroQuery.Data = query;
            SPCQueryName.Data = queryName;

            var queryParameters = queryParamsDict?.Select(y => new OnlineQuerySetupParamsChanges
            {
                Name = y.Key,
                DefaultValue = y.Value,
                DataType = GetTypeOfParametersValue(y.Value)
            }).ToArray();

            (SPCQueryParamGrid.GridContext as BoundContext).Data = queryParameters;
            SPCQueryParamGrid.GridContext.LoadData();
            CamstarWebControl.SetRenderToClient(SPCQueryParamGrid);

            var statitCommands = statitCommandDict?.Select(y => new SPCChartParamsChanges
            {
                ParamName = y.Key,
                ParamValue = y.Value,
                DisplayName = SPCHelper.InlineCommandToLegacy(y.Key)
            }).ToArray();

            (StatitComandsGrid.GridContext as BoundContext).Data = statitCommands;
            StatitComandsGrid.GridContext.LoadData();
            CamstarWebControl.SetRenderToClient(StatitComandsGrid);
        }

        private void SubmitMacroParsing_Click(object sender, EventArgs e)
        {
            var queryName = SPCQueryName.Data?.ToString();
            var service = new SPCQueryMaintService(FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);
            service.BeginTransaction();
            service.New();
            var query = new SPCQueryMaint()
            {
                ObjectChanges = new SPCQueryChanges
                {
                    Name = queryName,
                    QueryText = SPCMacroQuery.Data?.ToString(),
                    UserQueryParameters = SPCQueryParamGrid.Data as OnlineQuerySetupParamsChanges[]
                }
            };

            service.ExecuteTransaction(query);

            var res = service.CommitTransaction();

            if (res.IsSuccess)
            {
                Page.DisplayMessage(res.Message, res.IsSuccess);
                Page.CurrentCallStack.Parent.Context.LocalSession["QueryName"] = queryName;
                Page.CurrentCallStack.Parent.Context.LocalSession["StatitCommands"] = StatitComandsGrid.Data as SPCChartParamsChanges[];
                Page.CloseFloatingFrame(true);
            }
            else
            {
                Page.DisplayMessage(res.ExceptionData.Description, res.IsSuccess);
            }
        }
        #endregion
    }  
}
