/* Copyright 2023 Siemens */
using Camstar.WebPortal.FormsFramework.Utilities;
using System.Data;
using System.Web;
using System.Linq;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using OM = Camstar.WCF.ObjectStack;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using System;
using Camstar.WebPortal.Utilities;
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.Personalization;
using System.Collections.Generic;
using Camstar.WebPortal.FormsFramework;

/// <summary>
/// SS_SetWaferBatchId virtual page
/// </summary>
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_SetWaferBatchId : scsShopfloorBase
    {
        # region Methods

        public SS_SetWaferBatchId()
        {
        }

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if(!Page.IsPostBack)
				ComputerNameTextBox.TextControl.Text = SEMI.AppCode.UIUtility.GetComputerName(this);
			else
				if (SEMI.AppCode.UIUtility.IsPopupClose(this))
				{

					if (Page.DataContract.GetValueByName("LotListDM") != null)
					{
						//gets the list of returned lot from the popup form
                        string[] sContainers = Page.DataContract.GetValueByName("LotListDM") as string[];
                        JQDataGrid _gridContainers = Page.FindCamstarControl("ContainersField") as JQDataGrid;
                        foreach (string sContainersItem in sContainers)
                        {
                            FetchData(sContainersItem);
						}

						Page.DataContract.SetValueByName("LotListDM", null);
					}


					if(Page.DataContract.GetValueByName("RetWaferBatchIdValueDM") != null)
					{
						NamedObjectRef wafer = new NamedObjectRef();

                        wafer.Name = Page.DataContract.GetValueByName<string>("RetWaferBatchIdValueDM");
                        Page.DataContract.SetValueByName("RetWaferBatchIdValueDM", null);

                        BatchIdControl.Data = wafer;

					}
				}
		}

		/// <summary>
		/// Triggered when a lot / carrier etc... is entered on the page
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void SelectionIdField_DataChanged()
		{
			try
			{
				if (SelectionIdTextBox.Data != null)
				{
					Page.StatusBar.ClearMessage();
                    FetchData(SelectionIdTextBox.Data.ToString());
				}
			}
			catch (Exception Ex)
			{
				DisplayMessage(new OM.ResultStatus(Ex.TargetSite.Name + "(): " + Ex.Message, false));
			}
			finally
			{
				SelectionIdTextBox.TextControl.Text = "";
				SelectionIdTextBox.Focus();
			}
		}

        public void FetchData(string lot)
        {

            // get the session and user profile
            var fs = FrameworkManagerUtil.GetFrameworkSession();

            // init the service, service data and service info objects
            SetWaferBatchIdService objSvc = new SetWaferBatchIdService(fs.CurrentUserProfile);
            SetWaferBatchId objSvcData = new SetWaferBatchId { SelectionId = (string)lot };
            Camstar.WCF.ObjectStack.Primitive<string>[] selTypes = new Camstar.WCF.ObjectStack.Primitive<string>[5];
            selTypes[0] = "WAFERBATCHID";
            selTypes[1] = "WAFER";
            selTypes[2] = "LOT";
            selTypes[3] = "BATCHID";
            selTypes[4] = "CARRIER";
            objSvcData.SelectionIdTypes = (Camstar.WCF.ObjectStack.Primitive<string>[])selTypes;
            SetWaferBatchId_Info objSvcInfo = new SetWaferBatchId_Info();
            objSvcInfo.SelectionWafers = new WIPLotTxnWafersDetails_Info
            {
                LotWafersItem = FieldInfoUtil.RequestValue(),
                WaferScribeNumber = FieldInfoUtil.RequestValue(),
                Container = FieldInfoUtil.RequestValue(),
                WaferBatchId = FieldInfoUtil.RequestValue(),
                NDPW = FieldInfoUtil.RequestValue(),
                GoodQty = FieldInfoUtil.RequestValue()
            };

            // init the result object
            SetWaferBatchId_Result objResult = new SetWaferBatchId_Result();

            // execute to request the value
            ResultStatus resultStatus = objSvc.ResolveSelectionId(objSvcData, new SetWaferBatchId_Request { Info = objSvcInfo }, out objResult);

            if (resultStatus.IsSuccess)
            {
                if (objResult.Value.SelectionWafers != null)
                {
                    int j = 0;
                    JQDataGrid WafersFieldGrid = FindCamstarControl("SetWaferBatchId_Wafers") as JQDataGrid;
                    WIPLotTxnWafersDetails[] oExistingList = (WafersFieldGrid.GridContext as BoundContext).Data as WIPLotTxnWafersDetails[];
                    WIPLotTxnWafersDetails[] oNewList = new WIPLotTxnWafersDetails[0];

                    //scrub the container levels off the data so the grid doesn't display them
                    foreach (OM.WIPLotTxnWafersDetails oWafer in objResult.Value.SelectionWafers)
                    {                        
                        bool isUnique = true;
                        oWafer.Container.Level = null;
                        oWafer.LotWafersItem.Parent = null;                        
                        for (int i = 0; i < WafersFieldGrid.BoundContext.GetTotalRows(); i++)
                        {
                            string sExistingName = (WafersFieldGrid.GridContext as BoundContext).GetCell(i.ToString().PadLeft(6, '0'), "WaferScribeNumber").ToString();
                            string sWaferName = oWafer.WaferScribeNumber.ToString();
                            if (sWaferName.Equals(sExistingName))
                            {
                                isUnique = false;
                            }
                        }
                        if (isUnique)
                        {
                            Array.Resize(ref oNewList, oNewList.Length + 1);
                            oNewList[j] = oWafer;                            
                            j++;                            
                        }
                    }
                    if (oExistingList != null && oNewList != null)
                    {
                        WIPLotTxnWafersDetails[] oMergedList = new WIPLotTxnWafersDetails[oExistingList.Length + oNewList.Length];
                        Array.Copy(oExistingList, oMergedList, oExistingList.Length);
                        Array.Copy(oNewList, 0, oMergedList, oExistingList.Length, oNewList.Length);
                        (WafersFieldGrid.GridContext as BoundContext).Data = oMergedList;
                        WafersFieldGrid.BoundContext.LoadData();
                        CamstarWebControl.SetRenderToClient(WafersFieldGrid);
                    }
                    else
                    {
                        WafersFieldGrid.Data = objResult.Value.SelectionWafers;
                    }                    
                }
            }
            else
                DisplayMessage(resultStatus);
        }

		/// <summary>
		/// This event is fired when the submit action button is clicked.
		/// </summary>
		/// <param name="serviceData"></param>
		public override void GetInputData(Service serviceData)
		{
			base.GetInputData(serviceData);

            if (WafersFieldGrid.Data != null)
			{
                (serviceData as SetWaferBatchId).BatchId = BatchIdControl.Data.ToString();
                int x = 0;

                foreach (OM.WIPLotTxnWafersDetails item in (WafersFieldGrid.Data as Array))
                { 
                    (serviceData as SetWaferBatchId).Wafers[x] = new WIPLotTxnWafersDetails();
                    (serviceData as SetWaferBatchId).Wafers[x].LotWafersItem = new SubentityRef();
                    (serviceData as SetWaferBatchId).Wafers[x].LotWafersItem.ID = item.LotWafersItem.ToString();
                    (serviceData as SetWaferBatchId).Wafers[x].WaferScribeNumber = item.WaferScribeNumber.ToString();
                    (serviceData as SetWaferBatchId).Wafers[x].Container = new ContainerRef();
                    (serviceData as SetWaferBatchId).Wafers[x].Container.Name = item.Container.ToString();
                    x++;
                }
            }
		}



		/// <summary>
		/// simply clears the grid
		/// </summary>
		private void ClearWaferDetailsGrid()
		{
			//Whack any previous data
			WafersFieldGrid.ClearData();
		}

		/// <summary>
		/// Clears the query grid
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
		{
            base.WebPartCustomAction(sender, e);
            Page.ShopfloorReset(sender, e);
            DisplayMessage(new ResultStatus("", true));
		} // WebPartCustomAction(object sender, CustomActionEventArgs e)

        public void WaferGrid_RowMoveUp(/*object sender, EventArgs e*/)
        {
            if (WafersFieldGrid.GridContext.SelectedRowIDs != null
                && WafersFieldGrid.GridContext.SelectedRowIDs.Count > 0)
            {
                ReassignRows(1);
            }
        } //end WaferGrid_RowMoveUp

        /// <summary>
        /// triggered when a wafer grid row should move down in the array (params not used)
        /// </summary>
        /// <param name="sender" type="object"></param>
        /// <param name="e" type="EventArgs"></param>
        public void WaferGrid_RowMoveDown(/*object sender, EventArgs e*/)
        {
            if (WafersFieldGrid.GridContext.SelectedRowIDs != null
                && WafersFieldGrid.GridContext.SelectedRowIDs.Count > 0)
            {
                ReassignRows(-1);
            }
        } //end WaferGrid_RowMoveDown
        /// <summary>
        /// worker function for row adjustments: -1 is move down, +1 is move up, 0 is delete
        /// "down" refers to index (closer to 0), which is actually the up in the grid, therefore triggered by up-arrow button
        /// similarly "up" means "higher index value", which is down in the grid, therefore triggered by down-arrow button
        /// </summary>
        private void ReassignRows(int opCode)
        {
            // get current rows as array
            WIPLotTxnWafersDetails[] curRows = (WafersFieldGrid.GridContext as BoundContext).Data as WIPLotTxnWafersDetails[];
            // get counters
            int iCurRows = curRows == null ? 0 : curRows.Length;
            int iSelRows = WafersFieldGrid.GridContext.SelectedRowIDs.Count;
            if (iCurRows > 0 && iSelRows > 0)
            {
                // get selected row IDs as array
                string[] sSelRowIds = WafersFieldGrid.GridContext.SelectedRowIDs.ToArray();
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
                List<WIPLotTxnWafersDetails> newRows = null;
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
                    newRows = new List<WIPLotTxnWafersDetails>();
                    for (int j = 0; j < iNewRows; ++j)
                    {
                        //ModifyWafersDetails row = new ModifyWafersDetails();
                        //row = curRows[iNewIdxs[j]];
                        newRows.Add(curRows[iNewIdxs[j]]);
                    }
                }

                // clear old grid and setup the new grid, if rows were deleted or actually moved
                if (bRowsMoved || opCode == 0)
                {
                    WafersFieldGrid.ClearData();
                    (WafersFieldGrid.GridContext as BoundContext).Data = (newRows != null) ? newRows.ToArray() : null;
                    WafersFieldGrid.BoundContext.LoadData();
                    CamstarWebControl.SetRenderToClient(WafersFieldGrid);
                }
            } // if iCurRows > 0 && iSelRows > 0
        } //end ReassignRows


		/// <summary>
		/// Just tidies up the form
		/// </summary>
		/// <param name="status"></param>
		/// <param name="serviceData"></param>
		public override void PostExecute(ResultStatus status, Service serviceData)
		{
			base.PostExecute(status, serviceData);

			if (status.IsSuccess == true)
			{
				ClearWaferDetailsGrid();
				BatchIdControl.ClearData();
				CommentsTextBox.ClearData();
			}
		}

        # endregion

        # region Properties

        private CWC.TextBox SelectionIdTextBox { get { return FindCamstarControl("SelectionIdTextBox") as CWC.TextBox; } }
		private CWC.TextBox ComputerNameTextBox	{get { return FindCamstarControl("SetWaferBatchId_ComputerName") as CWC.TextBox; }	}
        private JQDataGrid WafersFieldGrid { get { return FindCamstarControl("SetWaferBatchId_Wafers") as JQDataGrid; } }
		private SEMI.AppCode.DataEnvelopControl _envSelectedLots { get { return Page.FindCamstarControl("WafersList") as SEMI.AppCode.DataEnvelopControl; } }
        private CWC.TextBox BatchIdControl { get { return FindCamstarControl("SetWaferBatchId_BatchId") as CWC.TextBox; } }
		private CWC.TextBox CommentsTextBox { get { return Page.FindCamstarControl("Shopfloor_Comments") as CWC.TextBox; } }

        # endregion

        # region Constants

        private const string cReset = "Reset";
        private const string cUp = "DownArrow";
        private const string cDown = "UpArrow";

        # endregion
    }
}




