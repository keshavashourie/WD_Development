//Copyright Siemens 2023
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.Tools.ASPXConverter;
using UIAction = Camstar.WebPortal.Personalization.UIAction;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using CWGC = Camstar.WebPortal.FormsFramework.WebGridControls;

/// <summary>
/// Summary description for DelegationSearch
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class QualitySearch : MatrixWebPart
    {
        protected virtual CWC.Button SearchButton { get { return Page.FindCamstarControl("SearchButton") as CWC.Button; } }

        protected virtual JQDataGrid SearchResults { get { return Page.FindCamstarControl("SearchResultsGrid") as JQDataGrid; } }

        #region Protected Functions
        /// <summary>
        /// OnInit - register events
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            Page.OnRequestControlSelectionValues += Page_OnRequestControlSelectionValues;
        }

        protected virtual void Page_OnRequestControlSelectionValues(object obj, FormsFramework.SelectionControlProcessingEventArgs e)
        {
            if (!Page.IsPostBack || ((Control)e.Control).ID == null || !((Control)e.Control).ID.Contains("SearchResultsGrid"))
                return;//only process if search

            (SearchResults.GridContext as DataGridContext).ClearData();
        }
        #endregion
    }
}