// Copyright Siemens 2023  

using System;
using System.Linq;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.WCFUtilities;
using Helpers;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class ChangeQtyM : ChangeQty
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ChangeTypesList.DataChanged += ChangeTypesList_DataChanged;
            Page.OnDisplayFormSelectionValues += Page_OnDisplayFormSelectionValues;
        }

        protected virtual void ChangeTypesList_DataChanged(object sender, EventArgs e)
        {
            if (ChangeTypesList.Data != null)
            {
                var text = ChangeTypesList.Text;
                var data = GetChangeTypeListData(ChangeTypesList.Data.ToString());
                RefreshControlsLabels(text);
                LoadReasonCodes(data);
            }
        }

        // Store change type list sel vals.
        protected virtual void Page_OnDisplayFormSelectionValues(object sender, FormProcessingEventArgs e)
        {
            if (e.Environment != null)
            {
                var env = new WCFObject(e.Environment);
                var changeTypesList = env.GetValue(ChangeTypesList.FieldExpressions) as WCF.ObjectStack.Environment;
                if (changeTypesList != null && changeTypesList.SelectionValues != null)
                {
                    var selectionValues = changeTypesList.SelectionValues;
                    if (selectionValues.Rows != null && selectionValues.Rows.Length > 0)
                    {
                        var cdoDefIdColumn = GetColumnIndex(selectionValues, ChangeTypesList.ListValueColumn);
                        if (cdoDefIdColumn > -1)
                            _firstChangeTypeData = selectionValues.Rows[0].Values[cdoDefIdColumn];
                        var displayNameColumn = GetColumnIndex(selectionValues, ChangeTypesList.ListDisplayExpression);
                        if (displayNameColumn > -1)
                            _firstChangeTypeText = selectionValues.Rows[0].Values[displayNameColumn];
                    }
                    ChangeTypeListSelVal = changeTypesList.SelectionValues;
                }
            }
        }

        protected override void ContainersGrid_DataChanged(object sender, EventArgs e)
        {
            base.ContainersGrid_DataChanged(sender, e);
            if (!string.IsNullOrEmpty(_firstChangeTypeData))
            {
                ChangeTypesList.Data = _firstChangeTypeData;
                ChangeTypesList.Text = _firstChangeTypeText;
                ChangeTypesList_DataChanged(ChangeTypesList, EventArgs.Empty);
            }
        }

        protected override void ResetFieldStates()
        {
            if (UseCurrentQtyField != null)
                UseCurrentQtyField.ClearData();
        }

        protected override string GetChangeTypeListValue()
        {
            return ChangeTypesList.Data != null ? GetChangeTypeListData(ChangeTypesList.Data.ToString()) : null;
        }

        protected override IMatrixBuilder MatrixBuilder { get {return new DivLayoutBuilder();} }

        protected virtual DropDownList ChangeTypesList
        {
            get
            {
                return Page.FindCamstarControl("ChangeQty_ChangeTypesList") as DropDownList;
            }
        }

        protected virtual string GetChangeTypeListData(string cdoDefId)
        {
            string retVal = string.Empty;
            if (!string.IsNullOrEmpty(cdoDefId) && ChangeTypeListSelVal != null)
            {
                var rows = ChangeTypeListSelVal.Rows;
                if (rows != null && rows.Length > 0)
                {
                    var cdoDefIdColumn = GetColumnIndex(ChangeTypeListSelVal, ChangeTypesList.ListValueColumn);
                    var nameColumn = GetColumnIndex(ChangeTypeListSelVal, "CDOName");

                    if (cdoDefIdColumn > -1 && nameColumn > -1)
                    {
                        var row = rows.FirstOrDefault(r => r.Values[cdoDefIdColumn] == cdoDefId);
                        if (row != null)
                            retVal = row.Values[nameColumn];
                    }
                }
            }
            return retVal;
        }

        private static int GetColumnIndex(RecordSet table, string columnName)
        {
            return table.Headers.ToList().IndexOf(table.Headers.FirstOrDefault(header => header.Name.Equals(columnName)));
        }

        private RecordSet ChangeTypeListSelVal
        {
            get { return ViewState["ChangeTypeListSelVal"] as RecordSet; }
            set { ViewState["ChangeTypeListSelVal"] = value; }
        }
        private string _firstChangeTypeData;
        private string _firstChangeTypeText;
    }
}
