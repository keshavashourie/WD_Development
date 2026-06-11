/* Copyright 2019 Siemens */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Web;
using System.Web.UI;

using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.FormsFramework.WebControls;


/// <summary>
/// The code for the move non standard virtual page.  Resolves the lot based on the 
/// selection id entered and populates the lot details grid.
/// </summary>
/// 
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_LotModifyRunNum : scsShopfloorBase
    {
        #region Properties

        protected JQDataGrid _gridContainers { get { return Page.FindCamstarControl("LotGrid") as JQDataGrid; } }
        protected JQDataGrid _gridWafers { get { return Page.FindCamstarControl("LotModifyWaferRunNumbers_WaferRunNumbers") as JQDataGrid; } }
        private SEMI.AppCode.DataEnvelopControl _envSelectedLots { get { return Page.FindCamstarControl("LotModifyWaferRunNumbers_SelectedLotsList") as SEMI.AppCode.DataEnvelopControl; } }
        protected CWC.TextBox txtComputerName { get { return Page.FindCamstarControl("LotModifyWaferRunNumbers_ComputerNameField") as CWC.TextBox; } }
        protected CWC.TextBox _txtSelectionId { get { return Page.FindCamstarControl("LotModifyWaferRunNumbers_SelectionIdField") as CWC.TextBox; } }
        protected ContainerList _ContainerList { get { return Page.FindCamstarControl("LotModifyWaferRunNumbers_Container") as ContainerList; } }
        protected Button _LotAttributesBttn { get { return Page.FindCamstarControl("LotModifyBins_LotAttributesButton") as Button; } }
        protected Button _LotAttributesCmmdBar { get { return Page.FindCamstarControl("LotAttributesPopup") as Button; } }

        bool isProcessTypeValue = false;
        int count = 0;

        #endregion

        #region DataChangeEvents

        /// <summary>
        /// This is an event that is fire when data is change in the SelectionId field is changed.
        /// </summary>
        public void SelectionIdField_DataChanged(object sender, EventArgs e)
        {
            try
            {
                if (!_txtSelectionId.IsEmpty)
                {
                    _ContainerList.ClearData();
                    SetGridData(_txtSelectionId.Data.ToString(), _gridContainers);
                    CamstarWebControl.SetRenderToClient(_gridContainers);
                    GetData(_txtSelectionId.Data.ToString());
                    _ContainerList.Data = new ContainerRef(_txtSelectionId.TextControl.Text);
                }
                else
                {
                    clearData(true);
                }
            }
            catch (Exception Ex)
            {
                DisplayMessage(new OM.ResultStatus(Ex.TargetSite.Name + "(): " + Ex.Message, false));
            }
            finally
            {
                _txtSelectionId.Focus();
            }
        }

        /// <summary>
        /// triggered when a carrier grid row should move up in the array (params not used)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void CarrierGrid_RowMoveUp()
        {
            if (_gridWafers.Data != null)
                if (_gridWafers.GridContext.SelectedRowIDs != null
                    && _gridWafers.GridContext.SelectedRowIDs.Count > 0)
                {
                    reassignRows(1);
                }
        } //end CarrierGrid_RowMoveUp

        /// <summary>
        /// triggered when a carrier grid row should move down in the array (params not used)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void CarrierGrid_RowMoveDown()
        {
            if (_gridWafers.Data != null)
                if (_gridWafers.GridContext.SelectedRowIDs != null
                    && _gridWafers.GridContext.SelectedRowIDs.Count > 0)
                {
                    reassignRows(-1);
                }
        } //end CarrierGrid_RowMoveDown

        #endregion

        #region Private Methods

        /// <summary>
        /// worker function for row adjustments: -1 is move down, +1 is move up, 0 is delete
        /// "down" refers to index (closer to 0), which is actually the up in the grid, therefore triggered by up-arrow button
        /// similarly "up" means "higher index value", which is down in the grid, therefore triggered by down-arrow button
        /// </summary>
        private void reassignRows(int opCode)
        {
            // get current rows as array
            ModifyWaferRunNumbersDetails[] curRows = (_gridWafers.GridContext as BoundContext).Data as ModifyWaferRunNumbersDetails[];
            // get counters
            int iCurRows = curRows == null ? 0 : curRows.Length;
            int iSelRows = _gridWafers.GridContext.SelectedRowIDs.Count;
            if (iCurRows > 0 && iSelRows > 0)
            {
                // get selected row IDs as array
                string[] sSelRowIds = _gridWafers.GridContext.SelectedRowIDs.ToArray();
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
                List<ModifyWaferRunNumbersDetails> newRows = null;
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
                    newRows = new List<ModifyWaferRunNumbersDetails>();
                    for (int j = 0; j < iNewRows; ++j)
                    {
                        ModifyWaferRunNumbersDetails row = new ModifyWaferRunNumbersDetails();
                        row.WaferRunNumber = curRows[iNewIdxs[j]].WaferRunNumber;
                        row.ReferenceAttributeNumber = curRows[iNewIdxs[j]].ReferenceAttributeNumber;
                        row.TxnTimestamp = curRows[iNewIdxs[j]].TxnTimestamp;
                        row.Username = curRows[iNewIdxs[j]].Username;
                        newRows.Add(row);
                    }
                }

                // clear old grid and setup the new grid, if rows were deleted or actually moved
                if (bRowsMoved || opCode == 0)
                {
                    _gridWafers.ClearData();
                    (_gridWafers.GridContext as BoundContext).Data = (newRows != null) ? newRows.ToArray() : null;
                    _gridWafers.BoundContext.LoadData();
                    CamstarWebControl.SetRenderToClient(_gridWafers);
                }
            } // if iCurRows > 0 && iSelRows > 0
        } //end reassignRows

        /// <summary>
        /// Gets data for form based off of return container(s)
        /// </summary>
        /// <param name="lot">string lot name</param>
        private void GetData(string lot)
        {
            try
            {

                OM.LotModifyWaferRunNumbers_Info oServiceInfo = new OM.LotModifyWaferRunNumbers_Info();
                LotModifyWaferRunNumbers_Result resultData = new LotModifyWaferRunNumbers_Result();
                UserProfile profile = HttpContext.Current.Session[Camstar.WebPortal.Constants.SessionConstants.UserProfile] as UserProfile;
                LotModifyWaferRunNumbersService svc = new LotModifyWaferRunNumbersService(profile);
                LotModifyWaferRunNumbers oServiceData = new LotModifyWaferRunNumbers();
                LotModifyWaferRunNumbers_Request reqData = new LotModifyWaferRunNumbers_Request();


                oServiceData.SelectionId = lot;
                oServiceInfo.SelectionContainer = FieldInfoUtil.RequestValue();
                oServiceInfo.LotWaferRunNumbers = new LotWaferRunNumbers_Info();
                oServiceInfo.LotWaferRunNumbers.WaferRunNumber = FieldInfoUtil.RequestValue();
                oServiceInfo.LotWaferRunNumbers.ReferenceAttributeNumber = FieldInfoUtil.RequestValue();
                oServiceInfo.LotWaferRunNumbers.TxnTimestamp = FieldInfoUtil.RequestValue();
                oServiceInfo.LotWaferRunNumbers.Username = FieldInfoUtil.RequestValue();

                reqData.Info = oServiceInfo;
                ResultStatus results = svc.ResolveSelectionId(oServiceData, reqData, out resultData);

                if (results.IsSuccess)
                {
                    DisplayValues(resultData.Value);

                    if (resultData.Value.SelectionContainer != null)
                        _txtSelectionId.TextControl.Text = resultData.Value.SelectionContainer.Name;

                    if (resultData.Value.LotWaferRunNumbers != null)
                    {

                        if (resultData.Value.LotWaferRunNumbers != null)
                        {
                            ///Gets Lot Materials array and changing the type for submission.
                            LotWaferRunNumbers[] allDetails = resultData.Value.LotWaferRunNumbers.ToArray();

                            if (allDetails != null)
                            {
                                int waferIndex = 0;
                                ModifyWaferRunNumbersDetails[] newDetails = new ModifyWaferRunNumbersDetails[allDetails.Length];

                                foreach (LotWaferRunNumbers detail in allDetails)
                                {
                                    newDetails[waferIndex] = new ModifyWaferRunNumbersDetails();
                                    newDetails[waferIndex].ListItemAction = ListItemAction.Add;
                                    newDetails[waferIndex].WaferRunNumber = detail.WaferRunNumber;
                                    newDetails[waferIndex].ReferenceAttributeNumber = detail.ReferenceAttributeNumber;
                                    newDetails[waferIndex].TxnTimestamp = detail.TxnTimestamp;
                                    newDetails[waferIndex].Username = detail.Username;
                                    waferIndex++;
                                }

                                (_gridWafers.GridContext as BoundContext).Data = newDetails;
                                _gridWafers.GridContext.LoadData();
                                CamstarWebControl.SetRenderToClient(_gridWafers);
                            }
                        }
                    }
                }
                else
                {
                    DisplayMessage(results);
                }

            }
            catch (Exception ex)
            {
                DisplayMessage(new ResultStatus(ex.Message, false));
            }
        }

        /// <summary>
        /// Clears data from the form.
        /// </summary>
        private void clearData(bool Allvalues)
        {
            Page.ClearValues();
            if (Allvalues)
            {
                _gridContainers.ClearData();
            }
            _gridWafers.ClearData();
            _txtSelectionId.Focus();
        }

        /// <summary>
        /// Set data for the lot grid
        /// </summary>
        /// <param name="sContainersItem">Container to insert</param>
        /// <param name="_gridContainersTemp">grid host</param>
        private void SetGridData(string sContainersItem, JQDataGrid _gridContainersTemp)
        {
            // check if lot not exist in the grid else add to the grid
            string strSelectedGridId = (_gridContainersTemp.GridContext as BoundContext).GetRowIdByCellValue("Lot", sContainersItem);

            if (string.IsNullOrEmpty(strSelectedGridId))
            {
                _gridContainersTemp.ClearData();
                //setting them to the grid
                SEMI.AppCode.UIUtility.GetLotQuerySelection(this, "LotModifyWaferRunNumbers", sContainersItem, false, ref _gridContainersTemp, "LotGrid", true);//_gridContainersTemp.LabelName for lotGrid string
            }
        }

        #endregion

        #region Page Events

        /// <summary>
        /// This event is fired when the submit action button is clicked.
        /// </summary>
        /// <param name="serviceData"></param>
        public override void GetInputData(Service serviceData)
        {
            (serviceData as LotModifyWaferRunNumbers).Container = new ContainerRef(_txtSelectionId.TextControl.Text);
            (serviceData as LotModifyWaferRunNumbers).SelectionId = _txtSelectionId.TextControl.Text;
            if (_gridWafers.Data != null)
            {
                ModifyWaferRunNumbersDetails[] waferItems = (_gridWafers.GridContext as BoundContext).Data as ModifyWaferRunNumbersDetails[];

                if (waferItems != null)
                {
                    int waferIndex = 0;
                    ModifyWaferRunNumbersDetails[] newWafers = new ModifyWaferRunNumbersDetails[waferItems.Length];

                    foreach (ModifyWaferRunNumbersDetails wafer in waferItems)
                    {
                        newWafers[waferIndex] = new ModifyWaferRunNumbersDetails();
                        newWafers[waferIndex].ListItemAction = ListItemAction.Add;
                        newWafers[waferIndex].ReferenceAttributeNumber = wafer.ReferenceAttributeNumber;
                        newWafers[waferIndex].TxnTimestamp = wafer.TxnTimestamp;
                        newWafers[waferIndex].Username = wafer.Username;
                        newWafers[waferIndex].WaferRunNumber = wafer.WaferRunNumber;
                        waferIndex++;
                    }

                    (serviceData as LotModifyWaferRunNumbers).WaferRunNumbers = newWafers;
                }
            }
            base.GetInputData(serviceData);
        }

        /// <summary>
        /// On page load
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            //hide or show the lot detail button based on current theme
            if (IsHorizon())
            {
                _LotAttributesBttn.Visible = false;
                _LotAttributesCmmdBar.Visible = true;
            }
            else
            {
                _LotAttributesBttn.Visible = true;
                _LotAttributesCmmdBar.Visible = false;
            }

            _txtSelectionId.DataChanged += new EventHandler(SelectionIdField_DataChanged);

            txtComputerName.TextControl.Text = SEMI.AppCode.UIUtility.GetComputerName(this);

            if (Page.IsPostBack)
                // Check if it is a pop up close, get the return result
                if (SEMI.AppCode.UIUtility.IsPopupClose(this))
                {

                    if (_envSelectedLots != null)


                        // manually initialize the containers list data contract since it is not triggered when we do a manual popup close
                        if (Page.DataContract.GetValueByName("LotModifyWaferRunNumbers_SelectedLotsListDM") != null)
                        {
                            //gets the list of returned lot from the popup form
                            _envSelectedLots.SS_ContainersList = Page.DataContract.GetValueByName("LotModifyWaferRunNumbers_SelectedLotsListDM") as string[];
                        }

                    if (_envSelectedLots.SS_ContainersList != null)
                    {
                        JQDataGrid _gridContainersTemp = Page.FindCamstarControl("LotGrid") as JQDataGrid;

                        SetGridData(_envSelectedLots.SS_ContainersList[0].ToString(), _gridContainersTemp);

                        GetData(_envSelectedLots.SS_ContainersList[0].ToString());

                        _txtSelectionId.TextControl.Text = _envSelectedLots.SS_ContainersList[0].ToString();
                        CamstarWebControl.SetRenderToClient(_gridContainersTemp);
                        //nullify the containers list
                        _envSelectedLots.SS_ContainersList = null;
                    }
                }
        }

        /// <summary>
        /// Resets the page value to default
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);

            var action = e.Action as Camstar.WebPortal.Personalization.CustomAction;

            if (action != null && action.Parameters == "Reset")
            {
                Page.ClearValues();
                _gridContainers.ClearData();
                _txtSelectionId.Focus();
                clearData(true);
                Page.ShopfloorReset(sender, e);
            }

        }

        /// <summary>
        /// Excutes after the submit button is clicked
        /// </summary>
        /// <param name="status"></param>
        /// <param name="serviceData"></param>
        public override void PostExecute(OM.ResultStatus status, OM.Service serviceData)
        {
            base.PostExecute(status, serviceData);
            if (status.IsSuccess)
            {
                clearData(true);
            }
        }

        #endregion
    }
}



