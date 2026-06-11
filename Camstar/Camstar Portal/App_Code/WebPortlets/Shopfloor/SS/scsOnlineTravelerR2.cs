//© 2022 Siemens Product Lifecycle Management Software Inc.
using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.PortalFramework;

using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework.Utilities;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using OM = Camstar.WCF.ObjectStack;
using CWGC = Camstar.WebPortal.FormsFramework.WebGridControls;
using PERS = Camstar.WebPortal.Personalization;
using System.Text;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{

    /// <summary>
    /// TODO: Add a Summary description for this Camstar Web Part
    /// </summary>
    public class scsOnlineTravelerR2 : MatrixWebPart
    {
        #region Controls

        protected virtual CWGC.JQDataGrid MainLineGrid
        {
            get { return FindCamstarControl("OnlineTraveler_Grid") as CWGC.JQDataGrid; }
        } 

        protected virtual CWGC.JQDataGrid FilterGrid
        {
            get { return FindCamstarControl("OnlineTraveler_FilterGrid") as CWGC.JQDataGrid; }
        } 

        protected virtual CWGC.JQDataGrid DetailGrid
        {
            get { return FindCamstarControl("OnlineTravelerDetails_Grid") as CWGC.JQDataGrid; }
        } 

        protected virtual CWC.Breadcrumb Breadcrumb
        {
            get { return Page.FindCamstarControl("BreadCrumb") as CWC.Breadcrumb; }
        }

        #endregion

        #region Protected Functions
        protected string FilterGridRecordsetKey
        {
            get
            {
                return "OnlineTraveler_FilterGridData";
            }
        }

        protected string FilterGridFieldsKey
        {
            get
            {
                return "OnlineTraveler_FilterGridField";
            }
        }

        private string CurrentDataGridIndexKey
        {
            get
            {
                return "OnlineTraveler_FilterGridField.CurrentDataGridIndex";
            }
        }// VisibleTabsIndexKe

        /// <summary>
        /// TODO: Summary Description of function
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!Page.IsPostBack)
            {
                SEMI.AppCode.UIUtility.MaximizePopUp(this);                
            }

            MainLineGrid.RowSelected += MainLineGrid_RowSelected;

            if ((Page.Request.Form["__EVENTTARGET"] != null) && (Page.Request.Form["__EVENTTARGET"] == Breadcrumb.ClientID))
            {
                int eventArgument;
                if (Int32.TryParse(Page.Request.Form["__EVENTARGUMENT"], out eventArgument))
                {
                    Page.PortalContext.LocalSession[FilterGridRecordsetKey] = null;
                    FilterGrid.ClearData();
                    MainLineGrid.BoundContext.SelectedItem = null;
                    MainLineGrid.BoundContext.SelectedRowID = null;
                    MainLineGrid.BoundContext.SelectedRowIDs = null;
                    MainLineGrid.BoundContext.SelectedRowIndex = null;
                }
            }
        }

        protected virtual ResponseData MainLineGrid_RowSelected(object sender, CWGC.JQGridEventArgs args)
        {
            LoadSelectedRowToFilterGridFrom(MainLineGrid);
            return null;
        }

        protected override void OnPreRender(EventArgs e)
        {
            InitFilterGrid();
            RefreshDataGridsVisibility();

            SetMaxBreadcrumbLevelToClientSide();
            SetShouldClearBreadcrumbCommandsToClientSide();

            string startupScript = "setTimeout(function(){ OnlineTravelerR2_BreadcrumbInit('" + Breadcrumb.ClientID + "', '" + this.GetType().AssemblyQualifiedName + "')}, 300);";
            ScriptManager.RegisterStartupScript(this, this.GetType(), "OnlineTravelerR2_BreadcrumbInit", startupScript, true);
            ScriptManager.RegisterStartupScript(this, this.GetType(), "OnlineTravelerR2_AddSlideoutToogler", string.Format("OnlineTravelerR2_AddSlideoutToogler('{0}');", this.MainLineGrid.ClientID), true);
            base.OnPreRender(e);
        }

        #endregion

        #region Public Functions

        #endregion

        #region Private Functions
        
        private void SetMaxBreadcrumbLevelToClientSide()
        {
            string script = $"var MaxBreadcrumbLevel_{Page.CallStackKey} = 3;";

            ScriptManager.RegisterStartupScript(this, GetType(), "MaxBreadcrumbLevel", script, true);
        }

        private void SetShouldClearBreadcrumbCommandsToClientSide()
        {
            string script;

            if (MainLineGrid.Visible /* Mainline grid is current */)
            {
                script = $"var shouldClearBreadcrumbCommands_{Page.CallStackKey} = true;";
            }
            else
            {
                script = $"var shouldClearBreadcrumbCommands_{Page.CallStackKey} = false;";
            }
            ScriptManager.RegisterStartupScript(this, GetType(), "shouldClearBreadcrumbCommands", script, true);
        }

        private void InitFilterGrid()
        {
            FilterGrid.LabelPosition = PERS.LabelPositionType.Hidden;
            FilterGrid.LabelText = String.Empty;
            FilterGrid.CssClass = "FilterGrid";
            FilterGrid.BoundContext.VisibleRows = 1;
            FilterGrid.Settings.VisibleRows = 1;
            FilterGrid.Settings.ParentGrid = null;
            FilterGrid.Settings.Navigator = PERS.JQGridNavigatorMode.Disabled;
            FilterGrid.Settings.Pager.Mode = PERS.GridPagerModes.AlwaysHidden;
            FilterGrid.Settings.Pager.DisplayTotalRecords = false;
            FilterGrid.Settings.Pager.ShowButtons = false;

            ReloadFilterGridData();
            RefreshFilterGridVisibility();
        }

        private void RefreshDataGridsVisibility()
        {
            if (FilterGrid.Visible)
            {
                MainLineGrid.Visible = false;
                DetailGrid.Visible = true;
            }
            else
            {
                MainLineGrid.Visible = true;
                DetailGrid.Visible = false;
            }
            CamstarWebControl.SetRenderToClient(MainLineGrid);
            CamstarWebControl.SetRenderToClient(DetailGrid);
        }

        private void RefreshFilterGridVisibility()
        {
            if ((FilterGrid.BoundContext.Data != null) && (MainLineGrid.SelectedItem != null))
            {
                FilterGrid.Visible = true;
                DetailGrid.Visible = true;
            }
            else
            {
                DetailGrid.Visible = false;
                FilterGrid.Visible = false;
            }
            CamstarWebControl.SetRenderToClient(FilterGrid);
            CamstarWebControl.SetRenderToClient(DetailGrid);
        }

        private void ReloadFilterGridData()
        {
            var filterGridContext = FilterGrid.BoundContext as SelValGridContext;

            OM.RecordSet rs = Page.PortalContext.LocalSession[FilterGridRecordsetKey] as OM.RecordSet;
            if (rs != null)
            {
                filterGridContext.SetSelectionValues(rs);
                filterGridContext.Fields = Page.PortalContext.LocalSession[FilterGridFieldsKey] as JQFieldCollection;
            }
        }

        private void LoadSelectedRowToFilterGridFrom(CWGC.JQDataGrid srcGrid)
        {
            string gridId = srcGrid.ID;
            Page.PortalContext.LocalSession[FilterGridRecordsetKey] = GetRecordsetFromSelectedRow(srcGrid);
            Page.PortalContext.LocalSession[FilterGridFieldsKey] = GetFieldsFrom(srcGrid);
        }

        private OM.RecordSet GetRecordsetFromSelectedRow(CWGC.JQDataGrid srcGrid)
        {
            BoundContext srcContext = srcGrid.BoundContext;
            var srcData = srcContext.Data as DataTable;
            if (srcContext.SelectedRowID == null)
                return null;
            int selectedRowIndex = Int32.Parse(srcContext.SelectedRowID);

            int visibleRows = srcGrid.Settings.VisibleRows ?? 0;
            int selectedRowIndexOnCurrentPage = selectedRowIndex % visibleRows;

            object[] selectedRow = (srcData.Rows[selectedRowIndexOnCurrentPage] as DataRow).ItemArray.Clone() as object[];

            return ConvertSelectedRowToRecordSet(selectedRow, srcData.Columns);
        }

        private OM.RecordSet ConvertSelectedRowToRecordSet(object[] tableData, DataColumnCollection tableHeaders)
        {
            OM.RecordSet rs = new OM.RecordSet();

            OM.Header[] headers = tableHeaders.OfType<DataColumn>().Select(h => new OM.Header
            {
                Name = h.ColumnName,
                Label = new OM.Label(h.ColumnName),
                TypeCode = h.DataType == typeof(DateTime) ? TypeCode.DateTime : TypeCode.String
            }).ToArray();

            rs.Headers = headers;
            rs.Rows = new OM.Row[1];

            OM.Row row = new OM.Row();
            row.Values = tableData.Select(
                    o => o is DateTime ? ((DateTime)o).ToString(SortableDateTimePatternMillisec) : o.ToString()
            ).ToArray();

            rs.Rows[0] = row;
            return rs;
        }

        private JQFieldCollection GetFieldsFrom(JQDataGrid grid)
        {
            JQFieldCollection fields = new JQFieldCollection();
            foreach (var field in grid.BoundContext.Fields)
            {
                JQField newField = new JQField();
                newField.Caption = field.Caption;
                newField.CellStyle = field.CellStyle;
                newField.ContentHidden = field.ContentHidden;
                newField.BindPath = field.BindPath;
                newField.DataType = field.DataType;
                newField.HeaderStyle = field.HeaderStyle;
                newField.LabelName = field.LabelName;
                newField.LabelText = field.LabelText;
                newField.ID = field.ID;
                newField.Visible = field.Visible;
                if (field.BindPath.ToString() == "SEQ")
                    newField.Visible = false;
                newField.Width = field.Width;
                fields.Add(newField);
            };
            return fields;
        }
        #endregion

        #region Constants
        protected readonly string SortableDateTimePatternMillisec = System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat.SortableDateTimePattern + "'.'fff";
        #endregion

        #region Private Member Variables

        #endregion

    }

}

