// Copyright Siemens 2025
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.UI;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using CWCGrid = Camstar.WebPortal.FormsFramework.WebGridControls;
using OM = Camstar.WCF.ObjectStack;
using PERS = Camstar.WebPortal.Personalization;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SPCAnnotation : MatrixWebPart
    {
        public virtual PERS.SPCChartData SPCChartParams { get; set; }

        private CWC.TextBox DataPointIDField;
        private CWC.CheckBox ExcludeDataPointField;
        private CWC.CheckBox IsCustomDataSource;
        private CWCGrid.JQDataGrid ExternalDataAnnotationGrid;
        private CWC.TextBox AddAnnotationTextField;

        private DataTable _externalAnnotationTable;

        protected override void OnLoad(EventArgs e)
        {
            DataPointIDField = Page.FindCamstarControl("DataPointID") as CWC.TextBox;
            ExcludeDataPointField = Page.FindCamstarControl("ExcludeDataPoint") as CWC.CheckBox;
            IsCustomDataSource = Page.FindCamstarControl("IsCustomDataSource") as CWC.CheckBox;
            ExternalDataAnnotationGrid = Page.FindCamstarControl("ExternalDataAnnotation") as CWCGrid.JQDataGrid;
            AddAnnotationTextField = Page.FindCamstarControl("AddAnnotationText") as CWC.TextBox;

            SetAnnotationInputEnabled(false);

            base.OnLoad(e);

            if (ExternalDataAnnotationGrid != null)
                ExternalDataAnnotationGrid.RowSelected += ExternalDataAnnotationGrid_RowSelected;

            if (Page is IActionContainer spcPage)
                SPCChartParams = spcPage.ActionDispatcher.DataContract.GetValueByName<PERS.SPCChartData>("SPCChartParams");

            ResolveCurrentSelection();
            PopulateExternalDataGrid();
            LoadAnnotation();             // always reflect DB state
            UpdateAnnotationInputState();
            ForceGridRepaint();
        }

        protected override object SaveViewState() =>
            new Pair { First = base.SaveViewState(), Second = SPCChartParams };

        protected override void LoadViewState(object savedState)
        {
            var pair = savedState as Pair;
            SPCChartParams = pair.Second as PERS.SPCChartData;
            base.LoadViewState(pair.First);
        }

        public override void WebPartCustomAction(object sender, PERS.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);

            var annotationText = GetAddAnnotationTextValue();
            var lblRequired = FrameworkManagerUtil.GetLabelCache().GetLabelByName("Lbl_AnnotationTextRequired");
            if (string.IsNullOrWhiteSpace(annotationText))
            {
                e.Result = new OM.ResultStatus(lblRequired?.Value ?? "Annotation text is required.", false);
                return;
            }

            var (selectedDataId, selectedExternalTable, _) = ResolveCurrentSelection();
            if (string.IsNullOrWhiteSpace(selectedDataId))
            {
                e.Result = new OM.ResultStatus("Please select a row.", false);
                return;
            }

            // read the posted state each submit
            var exclude = ExcludeDataPointField.IsChecked;
            var isCustom = IsCustomDataSource?.IsChecked == true;

            var page = Page as IActionContainer;
            string chartDefId = page?.ActionDispatcher.DataContract.GetValueByName("ChartNameDM")?.ToString();

            string externalTable = isCustom ? selectedExternalTable : null;
            string externalDataId = isCustom ? selectedDataId : null;

            OM.RecordSet annotationList;
            var status = _annotation.SaveAnnotation(
                dataPointID: selectedDataId,
                chartDefId: chartDefId,
                externalTable: externalTable,
                externalDataId: externalDataId,
                annotation: annotationText,
                exclude: exclude,
                annotationList: out annotationList);

            var lblOk = FrameworkManagerUtil.GetLabelCache().GetLabelByName("Lbl_AnnonationExclusionSuccessful");

            if (status.IsSuccess)
            {
                ClearAddAnnotationText();

                AddDataPointToLocalSession(selectedDataId);

                PopulateExternalDataGrid();
                LoadAnnotationTextsIfNeeded();
                RebindAndRefreshGrid();

                ScriptManager.RegisterStartupScript(Page.Form, Page.Form.GetType(), "setNotifyParent",
                    "if(window['__page']) window['__page'].set_notifyParentOnClose(true);", true);

                // reset checkbox after submit
                if (ExcludeDataPointField != null)
                {
                    ExcludeDataPointField.Data = false;
                    ExcludeDataPointField.IsChecked = false;
                }

                RenderToClient = true;
            }

            status.Message = status.IsSuccess ? (lblOk?.Value ?? "Annotation saved.") : status.Message;
            e.Result = status;
        }

        public virtual void LoadAnnotation()
        {
            var page = Page as IActionContainer;
            var chartDefId = page?.ActionDispatcher.DataContract.GetValueByName("ChartNameDM")?.ToString();
            var isCustom = IsCustomDataSource?.IsChecked == true;

            var (selectedDataId, selectedExternalTable, _) = ResolveCurrentSelection();

            string dataPointId = isCustom ? null : (DataPointIDField?.Data as string ?? selectedDataId);
            string externalTable = isCustom ? selectedExternalTable : null;
            string externalDataId = isCustom ? selectedDataId : null;

            if (string.IsNullOrWhiteSpace(dataPointId) && string.IsNullOrWhiteSpace(externalTable))
                return;

            OM.RecordSet rs;
            var status = _annotation.LoadAnnotations(dataPointId, chartDefId, externalTable, externalDataId, out rs);
            if (!status.IsSuccess)
                DisplayMessage(status);

            UpdateAnnotationInputState();
        }

        protected virtual void LoadAnnotationTextsIfNeeded()
        {
            if (_externalAnnotationTable == null) return;

            var isCustom = IsCustomDataSource?.IsChecked == true;

            for (int i = 0; i < _externalAnnotationTable.Rows.Count; i++)
            {
                var row = _externalAnnotationTable.Rows[i];
                var dataId = row["DataID"]?.ToString();
                var externalTable = isCustom && row.Table.Columns.Contains("ExternalTableName")
                    ? row["ExternalTableName"]?.ToString()
                    : null;

                row["SPCAnnotation"] = LoadAnnotationTextForDataPoint(dataId, externalTable);
            }
        }

        protected virtual string LoadAnnotationTextForDataPoint(string dataId, string externalTable)
        {
            if (string.IsNullOrWhiteSpace(dataId)) return string.Empty;

            var page = Page as IActionContainer;
            var chartDefId = page?.ActionDispatcher.DataContract.GetValueByName("ChartNameDM")?.ToString();
            var isCustom = IsCustomDataSource?.IsChecked == true;

            string normalizedDataId = dataId;
            string normalizedExternalTable = externalTable;

            if (isCustom && (string.IsNullOrWhiteSpace(normalizedExternalTable) || string.IsNullOrWhiteSpace(normalizedDataId)))
                return string.Empty;

            string dataPointId = isCustom ? null : normalizedDataId;
            string extTable = isCustom ? normalizedExternalTable : null;
            string extDataId = isCustom ? normalizedDataId : null;

            OM.RecordSet rs;
            var status = _annotation.LoadAnnotations(dataPointId, chartDefId, extTable, extDataId, out rs);

            if (!status.IsSuccess || rs?.Rows == null || rs.Rows.Length == 0)
                return string.Empty;

            return string.Join("\n",
                rs.Rows.Select(r => r.Values.Length > 1 ? r.Values[1] : string.Empty)
                       .Where(v => !string.IsNullOrWhiteSpace(v)));
        }

        protected virtual void PopulateExternalDataGrid()
        {
            if (ExternalDataAnnotationGrid == null) return;

            var page = Page as IActionContainer;
            if (page == null) return;

            var anchorId = page.ActionDispatcher.DataContract.GetValueByName("DataPointIDDM") as string;

            Camstar.WebPortal.FormsFramework.SPC.SPCChart.ExternalAnnotationContext externalContext =
                page.ActionDispatcher.DataContract.GetValueByName("ExternalAnnotationContextDM")
                as Camstar.WebPortal.FormsFramework.SPC.SPCChart.ExternalAnnotationContext;

            if (externalContext == null && !string.IsNullOrWhiteSpace(anchorId))
            {
                var spcChartObj = page.ActionDispatcher.DataContract.GetValueByName("SPCChart")
                                   as Camstar.WebPortal.FormsFramework.SPC.SPCChart;

                if (spcChartObj != null)
                {
                    externalContext = spcChartObj.GetExternalAnnotationContext(anchorId);
                    page.ActionDispatcher.DataContract.SetValueByName("ExternalAnnotationContextDM", externalContext);
                }
            }

            var dataTable = new DataTable();
            dataTable.Columns.Add("RowID", typeof(string));
            dataTable.Columns.Add("DataID", typeof(string));
            dataTable.Columns.Add("ExternalTableName", typeof(string));
            dataTable.Columns.Add("DataValue", typeof(string));
            dataTable.Columns.Add("SPCAnnotation", typeof(string));
            dataTable.PrimaryKey = new[] { dataTable.Columns["DataID"] };

            if (externalContext?.Rows != null)
            {
                var isCustom = IsCustomDataSource?.IsChecked == true;
                for (int i = 0; i < externalContext.Rows.Count; i++)
                {
                    var r = externalContext.Rows[i];
                    var dataId = r.DataID;

                    if (!string.IsNullOrWhiteSpace(dataId) && dataTable.Rows.Contains(dataId))
                        continue;

                    var nr = dataTable.NewRow();
                    nr["RowID"] = i.ToString();
                    nr["DataID"] = dataId;
                    if (isCustom)
                        nr["ExternalTableName"] = r.ExternalTableName;
                    nr["DataValue"] = r.DataValue;
                    nr["SPCAnnotation"] = r.SPCAnnotation;
                    dataTable.Rows.Add(nr);
                }
            }

            _externalAnnotationTable = dataTable;
            LoadAnnotationTextsIfNeeded();

            if (ExternalDataAnnotationGrid.GridContext is BoundContext bc)
            {
                bc.Data = dataTable;
                bc.SelectedRowIDs?.Clear();
                bc.SelectedRowID = null;

                ExternalDataAnnotationGrid.GridContext.LoadData();

                if (DataPointIDField != null && (dataTable.Rows.Count == 0 || externalContext == null))
                    DataPointIDField.Data = null;

                UpdateAnnotationInputState();
            }
            else if (dataTable.Rows.Count == 0 && DataPointIDField != null)
            {
                DataPointIDField.Data = null;
                UpdateAnnotationInputState();
            }

            CamstarWebControl.SetRenderToClient(ExternalDataAnnotationGrid);
        }

        protected virtual ResponseData ExternalDataAnnotationGrid_RowSelected(object sender, JQGridEventArgs args)
        {
            var grid = sender as JQDataGrid ?? ExternalDataAnnotationGrid;
            if (grid?.GridContext is BoundContext context)
            {
                var row = context.SelectedItem as DataRow
                          ?? ResolveRowByClientId(context.SelectedRowID)
                          ?? ResolveRowByClientId(context.SelectedRowIDs?.LastOrDefault());

                if (row != null && DataPointIDField != null)
                {
                    DataPointIDField.Data = row["DataID"]?.ToString();
                    UpdateAnnotationInputState();
                }
                else if (row == null && DataPointIDField != null)
                {
                    DataPointIDField.Data = null;
                    UpdateAnnotationInputState();
                }
            }

            return null;
        }

        private void RebindAndRefreshGrid()
        {
            if (ExternalDataAnnotationGrid?.GridContext is BoundContext bc)
            {
                bc.Data = _externalAnnotationTable;
                bc.RowsPerPage = _externalAnnotationTable?.Rows.Count > 0 ? _externalAnnotationTable.Rows.Count : bc.RowsPerPage;
                ExternalDataAnnotationGrid.GridContext.LoadData();
            }
            ForceGridRepaint();
        }

        private void ForceGridRepaint()
        {
            CamstarWebControl.SetRenderToClient(ExternalDataAnnotationGrid);
        }

        protected virtual DataRow ResolveRowByClientId(string clientId)
        {
            if (string.IsNullOrWhiteSpace(clientId))
                return null;

            var normalized = clientId;

            if (_externalAnnotationTable != null)
            {
                var row = _externalAnnotationTable.Rows.Find(normalized);
                if (row != null) return row;
            }

            if (ExternalDataAnnotationGrid?.GridContext is BoundContext context && context.DataWindow != null)
            {
                if (context.DataWindow.Columns.Contains("DataID"))
                {
                    var dw = context.DataWindow;
                    if (dw.PrimaryKey != null && dw.PrimaryKey.Length > 0 && dw.PrimaryKey[0].ColumnName == "DataID")
                    {
                        var row = dw.Rows.Find(normalized);
                        if (row != null) return row;
                    }
                    else
                    {
                        var matches = dw.Select($"DataID = '{normalized.Replace("'", "''")}'");
                        if (matches.Length > 0) return matches[0];
                    }
                }
            }

            return null;
        }

        protected virtual void AddDataPointToLocalSession(string datapointIDs)
        {
            var ids = datapointIDs.Split(',');
            var parentCallStack = Page.PortalContext.LocalSession["ParentCallStack"] as CallStack;
            var annotated = parentCallStack.Context.LocalSession["LatestAnnotatedDataPointID"] as List<string> ?? new List<string>();

            annotated.AddRange(ids);
            parentCallStack.Context.LocalSession["LatestAnnotatedDataPointID"] = annotated;
        }

        protected virtual FormsFramework.ISPCAnnotation _annotation
        {
            get
            {
                var annotation = Page.PortalContext.LocalSession["LatestAnnotation"] as ISPCAnnotation;
                if (annotation == null)
                {
                    var serviceRepository = new Camstar.WebPortal.FormsFramework.SPCRepository();
                    annotation = new Camstar.WebPortal.FormsFramework.SPCAnnotation(serviceRepository, IsCustomDataSource?.IsChecked == true);
                    Page.PortalContext.LocalSession["LatestAnnotation"] = annotation;
                }
                return annotation;
            }
        }

        protected virtual string GetAddAnnotationTextValue()
        {
            var value = AddAnnotationTextField?.Data as string;
            return value;
        }

        protected virtual void ClearAddAnnotationText()
        {
            if (AddAnnotationTextField != null)
            {
                AddAnnotationTextField.Data = string.Empty;
                AddAnnotationTextField.ClearData();
            }
        }

        protected virtual void SetAnnotationInputEnabled(bool enabled)
        {
            if (AddAnnotationTextField != null)
                AddAnnotationTextField.Enabled = enabled;
        }

        protected virtual void UpdateAnnotationInputState()
        {
            var hasSelection = !string.IsNullOrWhiteSpace(DataPointIDField?.Data as string);
            SetAnnotationInputEnabled(hasSelection);
        }

        protected virtual (string DataId, string ExternalTable, DataRow Row) ResolveCurrentSelection()
        {
            DataRow selectedRow = null;
            var grid = ExternalDataAnnotationGrid;

            if (grid?.GridContext is BoundContext context)
            {
                selectedRow = context.SelectedItem as DataRow
                               ?? ResolveRowByClientId(context.SelectedRowID);

                if (selectedRow == null && context.DataWindow != null && context.DataWindow.Rows.Count > 0)
                    selectedRow = context.DataWindow.Rows[0];
            }

            if (selectedRow == null && _externalAnnotationTable != null && _externalAnnotationTable.Rows.Count > 0)
                selectedRow = _externalAnnotationTable.Rows[0];

            var dataId = selectedRow?["DataID"]?.ToString();
            DataPointIDField.Data = dataId;
            var externalTable = selectedRow?.Table?.Columns.Contains("ExternalTableName") == true
                ? selectedRow["ExternalTableName"]?.ToString()
                : null;

            return (string.IsNullOrWhiteSpace(dataId) ? null : dataId,
                    string.IsNullOrWhiteSpace(externalTable) ? null : externalTable,
                    selectedRow);
        }
    }
}
