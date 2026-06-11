// Copyright Siemens 2023
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework.Utilities;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.Personalization;
using CamstarPortal.WebControls;
using Camstar.WCF.Services;
using System.Collections;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WCF.ObjectStack;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class MessageCenter : MatrixWebPart
    {

        #region Properties
        protected virtual CWC.NamedObject MessageCategoryFilterField { get { return Page.FindCamstarControl("MessageCategoryFilter") as CWC.NamedObject; } }
        protected virtual JQDataGrid SearchResultsGrid
        {
            get
            {
                return Page.FindCamstarControl("SearchResultsGrid") as JQDataGrid;
            }
        }

        protected virtual CWC.Button SearchButton {
            get
            {
                return Page.FindCamstarControl("SearchButton") as CWC.Button;
            }
        }
        #endregion


        protected override void OnLoad(System.EventArgs e)
        {
            base.OnLoad(e);

            var searchResultsWP = Page.FindIForm("MCsearchResults_WP") as WebPartBase;

            if (searchResultsWP != null)
                searchResultsWP.RenderToClient = true;

            // If the Search button has been clicked then clear grid selection
            if (Page.IsPostBack && Page.Request.Form[SearchButton.UniqueID] != null)
                SearchResultsGrid.GridContext.SelectedRowIDs?.Clear();

            // If there is no selected events clear actions
            if (!SearchResultsGrid.IsRowSelected)
                Page.ActionDispatcher.ActionStatuses = null;

            if (!Page.IsPostBack && Page.Request["SelectedSection"] != null)
            {
                int selectedSection;
                if (int.TryParse(Page.Request["SelectedSection"], out selectedSection))
                {
                    GetToDoListService service = new GetToDoListService(FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);
                    GetToDoList_Request request = new GetToDoList_Request()
                    {
                        Info = new GetToDoList_Info()
                        {
                            MessageCategoryFilter = FieldInfoUtil.RequestSelectionValue()
                        }
                    };

                    GetToDoList_Result result;
                    ResultStatus rs = service.ExecuteTransaction(request, out result);
                    if (rs.IsSuccess)
                    {
                        RecordSet selectionValues = result.Environment.MessageCategoryFilter.SelectionValues;
                        int nameColumnIndex = selectionValues.Headers.ToList().IndexOf(selectionValues.Headers.Where(header => header.Name.Equals(NameColumn)).FirstOrDefault());
                        int textColumnIndex = selectionValues.Headers.ToList().IndexOf(selectionValues.Headers.Where(header => header.Name.Equals(TextColumn)).FirstOrDefault());

                        MessageCategoryFilterField.Data = selectionValues.Rows[selectedSection].Values[nameColumnIndex];
                        MessageCategoryFilterField.Text = selectionValues.Rows[selectedSection].Values[textColumnIndex];

                        Page.Service.LoadSingleSelectionValues(SearchResultsGrid as IFieldSelection);
                    }
                }
            }
        }

        private const string NameColumn = "LabelName";
        private const string TextColumn = "UserLabelName";
    }
}
