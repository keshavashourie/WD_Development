// Copyright Siemens 2023
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Web;

using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.Personalization;


namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class LabelsPopup : MatrixWebPart
    {
        #region Property

        private JQDataGrid LabelsResultGrid { get { return Page.FindCamstarControl("LabelsResultGrid") as JQDataGrid; } }
        private CWC.DropDownList CategoryFilter { get { return Page.FindCamstarControl("CategoryFilter") as CWC.DropDownList; } }
        private CWC.TextBox LabelNameFilter { get { return Page.FindCamstarControl("LabelNameFilter") as CWC.TextBox; } }
        private CWC.TextBox LabelTextFilter { get { return Page.FindCamstarControl("LabelTextFilter") as CWC.TextBox; } }

        #endregion // property

        #region PageEvent

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!Page.IsPostBack)
                GetLabelCategory();
        }
        
        #endregion // Page Event
        
        #region Custom Procedure

        protected void GetLabelCategory()
        {
            var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            if (session != null)
            {
                var service = new DictionaryServiceService(session.CurrentUserProfile);
                var serviceData = new DictionaryService();

                var request = new DictionaryService_Request()
                {
                    Info = new DictionaryService_Info()
                    {
                        LabelCategoryDetails = new LabelCategoryDetails_Info()
                        {
                            RequestSelectionValues = true
                        }
                    }
                };

                var result = new DictionaryService_Result();
                var status = service.GetEnvironment(serviceData, request, out result);

                var details = new List<QueryData>();
                if (result != null && status.IsSuccess)
                {
                    if (result.Environment.LabelCategoryDetails != null && result.Environment.LabelCategoryDetails.SelectionValues != null)
                    {
                        OM.Header[] headers = result.Environment.LabelCategoryDetails.SelectionValues.Headers;
                        var CategoryNameIndex = Array.FindIndex(headers, h => h.Name.Equals("CategoryName"));
                        OM.Row[] sortedResult = result.Environment.LabelCategoryDetails.SelectionValues.Rows.OrderBy(row => row.Values[CategoryNameIndex]).ToArray();
                        result.Environment.LabelCategoryDetails.SelectionValues.Rows = sortedResult;

                        CategoryFilter.SetSelectionValues(result.Environment.LabelCategoryDetails.SelectionValues);
                    }
                }
            }
        }

        public void RowSelected()
        {
            if (LabelsResultGrid.SelectedItem != null)
                Page.DataContract.SetValueByName("SelectedLabelDM", LabelsResultGrid.SelectedItem);
        }

        public void SearchLabels()
        {
            var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            if (session != null)
            {
                string EmployeeName = session.CurrentUserProfile.Name;
                string filterSelectionData = CategoryFilter.SelectionData == null ? "" : CategoryFilter.SelectionData.ToString();
                int filterCategoryId = int.TryParse(filterSelectionData, out int categoryId) ? categoryId : 0;

                string filterCategory = CategoryFilter.SelectionData == null ? "" : $"and C.CategoryID = '{filterCategoryId}'";
                string labelText = LabelTextFilter.Data?.ToString() ?? "";
                string labelName = LabelNameFilter.Data?.ToString() ?? "";

                string queryText = "select T.LabelID, T.Name, T.LabelValue, T.Value, T.Category, DL.LabelValue as DictionaryValue " +
                    "from ( select L.LabelID, L.Name as Name, L.LabelValue, L.LabelValue as Value, C.Name as Category  " +
                    "from Labels L join LabelCategory C on C.CategoryID = L.CategoryID " +
                    $"where L.LabelValue like '%{labelText}%' and L.Name like '%{labelName}%' {filterCategory}" +
                    $"union select U.LabelID, U.UserLabelName as Name, U.LabelValue, U.LabelValue as Value, 'UserLabel' as Category " +
                    $"from UserLabel U where U.LabelValue like '%{labelText}%' and U.UserLabelName like '%{labelName}%')" +
                    $"T join Employee E on E.EmployeeName = '{EmployeeName}' " +
                    "left join DictionaryLabel DL on DL.LabelID = T.LabelID " +
                    "and DL.DictionaryId = E.LanguageDictionaryId order by Category, Name";

                var recordSet = new RecordSet();

                var service = new QueryService(session.CurrentUserProfile);
                var options = new QueryOptions();
                var resultStatus = service.ExecuteAdHoc(queryText, options, out recordSet);

                if (resultStatus.IsSuccess)
                {
                    var headers = recordSet.Headers;
                    var selectionValues = recordSet.Rows;
                    int LabelIdIndex = 0;
                    int LabelNameIndex = 1;
                    int LabelValueIndex = 2;
                    int LabelCategoryIndex = 4;
                    int DefaultValueIndex = 3;

                    var labelList = new List<DictionaryLabel>();
                    if (selectionValues != null)
                    {
                        labelList.AddRange(
                            selectionValues.Select(
                                v => new DictionaryLabel()
                                {
                                    ID = Int32.Parse(v.Values[LabelIdIndex]),
                                    Name = v.Values[LabelNameIndex],
                                    Value = v.Values[LabelValueIndex],
                                    Category = v.Values[LabelCategoryIndex],
                                    DefaultValue = v.Values[DefaultValueIndex]
                                }

                            )
                        );
                    }

                    LabelsResultGrid.Data = labelList.ToArray();
                }
            }
        }

        #endregion // Custom Procedure

    }
}
