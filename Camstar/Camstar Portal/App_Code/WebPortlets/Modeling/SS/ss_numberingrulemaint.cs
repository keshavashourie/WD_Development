/* Copyright 2019 Siemens */
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Camstar.WCF.ObjectStack;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.WebControls.PickLists;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.HtmlControls;
using Camstar.WebPortal.FormsFramework;

namespace Camstar.WebPortal.WebPortlets.Modeling
{

    /// <summary>
    /// TODO: Add a Summary description for this Camstar Web Part
    /// </summary>
    public class SS_NumberingRuleMaint : NumberingRuleMaint
    {
        #region Controls

        JQDataGrid _gridValues { get { return Page.FindCamstarControl("ObjectChanges_Values") as JQDataGrid; } }
        CWC.Button _upArrowButton { get { return Page.FindCamstarControl("upArrowButton") as CWC.Button; } }
        CWC.Button _downArrowButton { get { return Page.FindCamstarControl("downArrowButton") as CWC.Button; } }
        CWC.DropDownList NumberingRuleType { get { return Page.FindCamstarControl("ObjectChanges_NumberingRuleType") as CWC.DropDownList; } }

        #endregion

        #region PageEvents

        /// <summary>
        /// On Page load
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            //_upArrowButton.Click += new EventHandler(_upArrowButton_Click);
            //_downArrowButton.Click += new EventHandler(_downArrowButton_Click);
            base.OnLoad(e);

        }

        void _downArrowButton_Click(object sender, EventArgs e)
        {
            CarrierGrid_RowMoveDown();
        }

        void _upArrowButton_Click(object sender, EventArgs e)
        {
            CarrierGrid_RowMoveUp();
        }

        public override void PostExecute(ResultStatus status, Service serviceData)
        {
            LastAssignedSequence.OriginalData = LastAssignedSequence.Data;
            base.PostExecute(status, serviceData);
        }

        #endregion

        #region Public Functions


        /// <summary>
        /// triggered when a carrier grid row should move up in the array (params not used)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void CarrierGrid_RowMoveUp()
        {
            if (_gridValues.Data != null)
                if (_gridValues.GridContext.SelectedRowIDs != null
                    && _gridValues.GridContext.SelectedRowIDs.Count > 0)
                {
                    reassignRows(1);
                }
        }

        /// <summary>
        /// triggered when a carrier grid row should move down in the array (params not used)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void CarrierGrid_RowMoveDown()
        {
            if (_gridValues.Data != null)
                if (_gridValues.GridContext.SelectedRowIDs != null
                    && _gridValues.GridContext.SelectedRowIDs.Count > 0)
                {
                    reassignRows(-1);
                }
        }

        #endregion

        #region Private Functions

        /// <summary>
        /// worker function for row adjustments: -1 is move down, +1 is move up, 0 is delete
        /// "down" refers to index (closer to 0), which is actually the up in the grid, therefore triggered by up-arrow button
        /// similarly "up" means "higher index value", which is down in the grid, therefore triggered by down-arrow button
        /// </summary>
        private void reassignRows(int opCode)
        {
            // get current rows as array
            NumberingRuleValuesChanges[] curRows = (_gridValues.GridContext as BoundContext).Data as NumberingRuleValuesChanges[];
            // get counters
            int iCurRows = curRows == null ? 0 : curRows.Length;
            int iSelRows = _gridValues.GridContext.SelectedRowIDs.Count;
            if (iCurRows > 0 && iSelRows > 0)
            {
                // get selected row IDs as array
                string[] sSelRowIds = _gridValues.GridContext.SelectedRowIDs.ToArray();
                Boolean[] bSelRows = new Boolean[iCurRows];
                int[] iNewIdxs = new int[iCurRows];
                Boolean bRowsMoved = false;

                // init helper arrays, mark selected rows
                for (int i = 0; i < iCurRows; i++)
                {
                    bSelRows[i] = false; iNewIdxs[i] = -1;
                }
                for (int i = 0; i < iSelRows; i++)
                {
                    int index = Int32.Parse(sSelRowIds[i]);
                    if (0 <= index && index < iCurRows)
                        bSelRows[index] = true;
                }

                // setup iteration boundaries, number of rows in new grid
                int iStart, iInc, iLast, iNewRows;
                if (opCode > 0) // moving rows up: traverse down from second-to-last row to first row
                {
                    iStart = iCurRows - 2; iInc = -1; iLast = 0; iNewRows = iCurRows;
                }
                else if (opCode < 0) // moving rows down: traverse up from second row to last row
                {
                    iStart = 1; iInc = 1; iLast = iCurRows - 1; iNewRows = iCurRows;
                }
                else // deleting rows: traverse up from first row to last row
                {
                    iStart = 0; iInc = 1; iLast = iCurRows - 1; iNewRows = iCurRows - iSelRows;
                    // alternative: count the non-selected rows
                    // iNewRows=0; for (int i = 0; i < iCurRows; i++) if (!bSelRows[i]) ++iNewRows;
                }

                // start with an empty newRows list
                List<NumberingRuleValuesChanges> newRows = null;
                // determine new row sequence
                if (opCode == 0 && iNewRows > 0) // new row sequence when deleting rows
                {
                    int iLastUsed = -1;
                    for (int j = 0; j < iNewRows; ++j) // assign new rows, skipping deleted ones
                    {
                        // find next unused row to assign, adjust iLastUsed and stop loop when assigned
                        for (int i = iLastUsed + iInc; i != iLast + iInc && iNewIdxs[j] < 0; i += iInc)
                            if (!bSelRows[i])
                                iNewIdxs[j] = iLastUsed = i;
                    }
                }
                else if (opCode != 0 && iCurRows > 1) // new row sequence when moving rows
                {
                    iNewIdxs[iStart - iInc] = iStart - iInc;
                    for (int i = iStart; i != iLast + iInc; i += iInc)
                    {
                        if (bSelRows[i]) // swap with up/down neighbor
                        {
                            int x = iNewIdxs[i - iInc]; iNewIdxs[i - iInc] = i; iNewIdxs[i] = x;
                            bRowsMoved = true;
                        }
                        else // copy as is
                        {
                            iNewIdxs[i] = i;
                        }
                    }
                }

                // build up the newRows list if there are any rows in the new grid and op was delete or rows were moved
                if (iNewRows > 0 && (bRowsMoved || opCode == 0))
                {
                    newRows = new List<NumberingRuleValuesChanges>();
                    for (int j = 0; j < iNewRows; ++j)
                    {
                        NumberingRuleValuesChanges row = new NumberingRuleValuesChanges();
                        row.ValueString = curRows[iNewIdxs[j]].ValueString;
                        newRows.Add(row);
                    }
                }

                // clear old grid and setup the new grid, if rows were deleted or actually moved
                if (bRowsMoved || opCode == 0)
                {
                    _gridValues.ClearData();
                    (_gridValues.GridContext as BoundContext).Data = (newRows != null) ? newRows.ToArray() : null;
                    _gridValues.BoundContext.LoadData();
                    CamstarWebControl.SetRenderToClient(_gridValues);
                }
            } // if iCurRows > 0 && iSelRows > 0
        } //end reassignRows

        #endregion

        #region Constants

        #endregion

        #region Private Member Variables

        #endregion

    }

}





