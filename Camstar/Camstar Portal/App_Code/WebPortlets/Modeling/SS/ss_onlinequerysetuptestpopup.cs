/* Copyright 2023 Siemens */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
//using System.Web.UI.WebControls;

using System.Data;

using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;

using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.WebPortlets;
using Camstar.WebPortal.Personalization;

using CamstarPortal.WebControls;

namespace Camstar.WebPortal.WebPortlets.Modeling
{
    public class SS_OnlineQuerySetupTestPopup : UserQueryTestPopup
    {

        protected override JQDataGrid UserQueryResultGrid { get { return Page.FindCamstarControl("UserQueryResult") as JQDataGrid; } }

        protected override void OnInit(EventArgs e)
        {
            _queryParams = Page.DataContract.GetValueByName("UserQueryParamsDM") as QueryParameters;
            _queryName = Page.DataContract.GetValueByName("UserQueryNameDM").ToString();
            _OQSParams = Page.DataContract.GetValueByName("OnlineQuerySetupParamsDM") as OnlineQuerySetupParamsChanges[];

            base.OnInit(e);

            Page.Title = "Query: " + _queryName;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            //if no queryParams execute query immediate
            if (_queryParams == null || _queryParams.Parameters.Count() == 0)
                ExecuteQuery();
        }

        protected override void AddParamControls(QueryParameters queryParams)
        {
            this.ControlAlignment = ControlAlignmentType.LabelLeftInputRight;
            BaseFieldExpression = ".";

            foreach (QueryParameter qp in queryParams.Parameters)
            {
                TextBox _paramControl = new TextBox();
                CreateField(_paramControl as Control, "");
                _paramControl.ID = "_parameter_" + qp.Name;
                _paramControl.LabelText = _OQSParams.Where(c => c.Name == qp.Name && c.DisplayText != null).Select(c => c.DisplayText.ToString()).FirstOrDefault();
                _paramControl.LabelPosition = LabelPositionType.Top;
                _paramControl.Margin = new Margin() { Right = 15 };
                _paramControlList.Add(_paramControl);
                this[1, 0] = _paramControl;
                _paramControl.TextControl.Text = _OQSParams.Where(c => c.Name == qp.Name && c.DefaultValue != null).Select(c => c.DefaultValue.ToString()).FirstOrDefault();
            }
        }

        private QueryParameters _queryParams;
        private string _queryName;
		private OnlineQuerySetupParamsChanges[] _OQSParams;
        private Dictionary<string, int?> ParameterDataTypeMap;
        LabelCache labelCache;

    }
}




