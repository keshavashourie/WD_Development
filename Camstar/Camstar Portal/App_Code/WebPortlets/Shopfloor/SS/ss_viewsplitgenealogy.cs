/* Copyright 2020 Siemens */
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.WCFUtilities;

using OM = Camstar.WCF.ObjectStack;
using CGC = Camstar.WebPortal.FormsFramework.WebGridControls;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;

using SEMI.AppCode;

/// <summary>
/// Class / methods for the view split genealogy virtual page.
/// Resolves the lot based on the selection id entered and populates split genealogy grid.
/// </summary>
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
	public class SS_ViewSplitGenealogy : MatrixWebPart
	{
		#region PrivateProperties

		// named objects
		CWC.NamedObject _ndoFactory { get { return Page.FindCamstarControl("ViewContainerStatus_Factory") as CWC.NamedObject; } }
		// TextBoxs
		CWC.TextBox _txtComputerNameField { get { return Page.FindCamstarControl("ViewContainerStatus_ComputerNameField") as CWC.TextBox; } }
		CWC.TextBox _txtContainerField { get { return Page.FindCamstarControl("ViewContainerStatus_ContainerField") as CWC.TextBox; } }
		CWC.TextBox _txtNextStep { get { return Page.FindCamstarControl("ViewContainerStatus_NextStep") as CWC.TextBox; } }
		CWC.TextBox _txtStep { get { return Page.FindCamstarControl("ViewContainerStatus_Step") as CWC.TextBox; } }
		CWC.TextBox _txtQty { get { return Page.FindCamstarControl("ViewContainerStatus_Qty") as CWC.TextBox; } }
		CWC.TextBox _txtQty2 { get { return Page.FindCamstarControl("ViewContainerStatus_Qty2") as CWC.TextBox; } }
		CWC.TextBox _txtProduct { get { return Page.FindCamstarControl("ViewContainerStatus_Product") as CWC.TextBox; } }
		CWC.TextBox _txtProdRev { get { return Page.FindCamstarControl("ViewContainerStatus_ProductRevision") as CWC.TextBox; } }
		CWC.TextBox _txtWorkflow { get { return Page.FindCamstarControl("ContainerStatusDetails_Workflow") as CWC.TextBox; } }
		CWC.TextBox _txtWFRev { get { return Page.FindCamstarControl("ContainerStatusDetails_WorkflowRev") as CWC.TextBox; } }
		// Buttons
		CWC.Button _btnLotAttributes { get { return Page.FindCamstarControl("ViewContainerStatus_LotAttributesButton") as CWC.Button; } }
		CWC.Button _btnMaterialsRequired { get { return Page.FindCamstarControl("ViewContainerStatus_MaterialsRequiredButton") as CWC.Button; } }
		CWC.Button _btnOnlineTraveler { get { return Page.FindCamstarControl("ViewContainerStatus_OnlineTravelerButton") as CWC.Button; } }
		CWC.Button _btnDocumentSets { get { return Page.FindCamstarControl("ViewContainerStatus_DocumentSetsButton") as CWC.Button; } }
		// Grids
		protected JQDataGrid _grdLotLevels { get { return Page.FindCamstarControl("SplitLevelsGrid") as JQDataGrid; } }

		#endregion // PrivateProperties

		#region PrivateClasses

		private class SS_SplitGenealogy_Row
		{
			//private string sFromLot;
			//private string sToLot;
			//private int iLevel;
			//private int iLvlCnt;
			public string FromLot;
			public string ToLot;
			public int Level;
			public int LvlCnt;

			public SS_SplitGenealogy_Row()
			{ }

			public SS_SplitGenealogy_Row(string from, string to, int lvl, int cnt)
			{
				//sfromLot = from; sToLot = to; iLevel = lvl; iLvlCnt = cnt;
				FromLot = from; ToLot = to; Level = lvl; LvlCnt = cnt;
			}

			//public string FromLot { get { return sFromLot; } set { sFromLot = value; } }
			//public string ToLot { get { return sToLot; } set { sToLot = value; } }
			//public int Level { get { return iLevel; } set { iLevel = value; } }
			//public int LvlCnt { get { return iLvlCnt; } set { iLvlCnt = value; } }
		}

		private class SS_SplitGenealogy_Map
		{
			//private string sColumnName;
			//private int iLevel;
			public string ColumnName;
			public int Level;

			public SS_SplitGenealogy_Map()
			{ }

			public SS_SplitGenealogy_Map(string col, int lev)
			{
				//sColumnName=col; iLevel=lev;
				ColumnName = col; Level = lev;
			}

			//public string ColumnName { get { return sColumnName; } set { sColumnName = value; }	}
			//public int Level { get { return iLevel; } set { iLevel = value; } }
		}

		 SS_SplitGenealogy_Map[] gColMap 
		{
			get { return _gColMap!=null ? _gColMap : BuildSplitGenealogy_Map(); }
			set { _gColMap = value; }
		}

		private SS_SplitGenealogy_Map[] _gColMap;

		 SS_SplitGenealogy_Map[] BuildSplitGenealogy_Map(){
			_gColMap= new SS_SplitGenealogy_Map[ciCntColMap];
			for(int i=0;i<ciCntColMap;i++){
				_gColMap[i]= new SS_SplitGenealogy_Map("L"+i , 99);
			}
		
			return _gColMap;
		}

		public int ciCntColMap { get { return _ciCntColMap; } set { _ciCntColMap = value; } }

		
		private int _ciCntColMap =0;

		#endregion // PrivateClasses

		#region PrivateFunctions

		/// <summary>
		/// reset all input fields to empty / nothing
		/// </summary>
		private void ResetFields(Boolean bIncludeSelectionId = true)
		{
			if (bIncludeSelectionId)
			    _txtContainerField.ClearData();
			_grdLotLevels.ClearData();
			_btnLotAttributes.Enabled = _btnOnlineTraveler.Enabled = false;
			_btnDocumentSets.Enabled = _btnMaterialsRequired.Enabled = false;
			for (int k = 0; k < ciCntColMap; ++k)
			{
				(_grdLotLevels.GridContext as BoundContext).Fields[gColMap[k].ColumnName].Visible = false;
				gColMap[k].Level = 99;
			}
			(_grdLotLevels.GridContext as BoundContext).Fields["LotName"].Visible = false;
			(_grdLotLevels.GridContext as BoundContext).Fields["ColName"].Visible = false;
		} //end ResetFields

		/// <summary>
		/// helper function to create a new record set for the levels grid
		/// </summary>
		private RecordSet NewLevelsRecordSet(int iRowCnt)
		{
			RecordSet rsNew = new RecordSet();
			rsNew.Headers = new Header[ciCntColMap + 2];
			for (int i = 0; i < ciCntColMap; ++i)
			{
				rsNew.Headers[i] = new Header();
				rsNew.Headers[i].Name = gColMap[i].ColumnName;
				//rsNew.Headers[i].Label = new OM.Label(gColMap[i].Level.ToString());
			}
			rsNew.Headers[ciCntColMap] = new Header();
			rsNew.Headers[ciCntColMap].Name = "LotName";
			rsNew.Headers[ciCntColMap + 1] = new Header();
			rsNew.Headers[ciCntColMap + 1].Name = "ColName";
			rsNew.Rows = new Row[iRowCnt];
			return rsNew;
		} // end NewLevelsRecordSet

		/// <summary>
		/// helper function to initialize an output row
		/// </summary>
		private void AddLevelsRow(RecordSet rs, int idx, string sLotName, int iLvlIdx)
		{
			if (rs == null || rs.Headers == null || rs.Rows == null)
				return;

			int iColCnt = rs.Headers.GetLength(0);
			int iRowCnt = rs.Rows.GetLength(0);
			if (idx < 0 || idx >= iRowCnt || rs.Rows[idx] != null || iColCnt < ciCntColMap + 2)
				return;

			rs.Rows[idx] = new Row();
			rs.Rows[idx].Values = new string[iColCnt];
			for (int i = 0; i < iColCnt; ++i)
			{
				string sNewVal = (i == iLvlIdx) ? sLotName : "";
				rs.Rows[idx].Values[i] = sNewVal;
			}
			rs.Rows[idx].Values[ciCntColMap] = sLotName;
			rs.Rows[idx].Values[ciCntColMap + 1] = gColMap[iLvlIdx].ColumnName;
		} // end AddLevelsRow


		/// <summary>
		/// populate levels grid using genealogy query results 
		/// </summary>
		private Boolean AdjustGridData(RecordSet rsInput)
		{
			Boolean bHaveContainer = !string.IsNullOrWhiteSpace(_txtContainerField.TextControl.Text);
			if (!bHaveContainer || _grdLotLevels == null || rsInput == null) // no lot or no rows or no grid
				return false;
			
			int iOldRows = (rsInput.Rows == null) ? 0 : rsInput.Rows.GetLength(0);
			int iCntHeads = (rsInput.Headers == null) ? 0 : rsInput.Headers.GetLength(0);
			if (iOldRows < 1 || iCntHeads < 1) // no data
				return false;

			// establish index of fields of interest from header data
			_grdLotLevels.ClearData();
			int iToIdx = -1, iFromIdx = -1, iLvlIdx = -1, iCntIdx = -1, iFound = 0;
			for (int i = 0; i < iCntHeads && iFound < 4; ++i)
			{
				switch (rsInput.Headers[i].Name.ToUpper())
				{
					case "TOCONTAINER":         iToIdx = i;   ++iFound; break;
					case "FROMCONTAINER":       iFromIdx = i; ++iFound; break;
					case "LEVEL":               iLvlIdx = i;  ++iFound; break;
					case "TOCONTAINERSEQUENCE": iCntIdx = i;  ++iFound; break;
				}
			}
			if (iFound != 4) // missing column(s) / changed column name(s)?
				return false;

			// loop through the input data and create a sort array with (parent,lot,level,lvlcnt) rows
			int iLvlSpan = 0, iMinLvl = 99, iMaxLvl = -99, iCntFrom = 0, iCntMisses = 0, iStartIdx = -1;
			// making it 1 entry larger to accommodate 1st node with parent
			SS_SplitGenealogy_Row[] aRowsIn = new SS_SplitGenealogy_Row[iOldRows+1];
			for (int i = 0; i < iOldRows; ++i)
			{
				string sLotName = rsInput.Rows[i].Values[iToIdx];
				if (string.IsNullOrWhiteSpace(sLotName)) sLotName = "";
				string sParentName = rsInput.Rows[i].Values[iFromIdx];
				if (string.IsNullOrWhiteSpace(sParentName)) sParentName = "";
				string sLotLevel = rsInput.Rows[i].Values[iLvlIdx];
				if (string.IsNullOrWhiteSpace(sLotLevel)) sLotLevel = "0"; // don't allow empty level, use 0 as default
				string sLvlCount = rsInput.Rows[i].Values[iCntIdx];
				if (string.IsNullOrWhiteSpace(sLvlCount)) sLvlCount = "1"; // don't allow empty counter, use 1 as default
				if (!string.IsNullOrWhiteSpace(sLotName) && !string.IsNullOrWhiteSpace(sLotLevel) && !string.IsNullOrWhiteSpace(sLvlCount))
				{
					int iLvlCount = int.Parse(sLvlCount);
					int iLotLevel = int.Parse(sLotLevel);
					aRowsIn[i - iCntMisses] = new SS_SplitGenealogy_Row(sParentName, sLotName, iLotLevel, iLvlCount);
					if (string.IsNullOrWhiteSpace(sParentName))
						iStartIdx = i - iCntMisses;
					else
						++iCntFrom;
					if (iLotLevel < iMinLvl) 
						iMinLvl = iLotLevel;
					if (iLotLevel > iMaxLvl) // find start entry: highest level (==MaxLevel) and lowest count (==1)
					{
						iMaxLvl = iLotLevel;
						if (iStartIdx < 0)
							for (int j = 0; j <= i - iCntMisses; ++j)
								if (aRowsIn[j].Level == iMaxLvl && aRowsIn[j].LvlCnt == 1)
									iStartIdx = j;
					}
				}
				else // skip entries that don't have necessary data (toLot, level and count): really shouldn't happen
				{
					++iCntMisses;
				}
			}

			int iNewRows = iOldRows - iCntMisses;  // adjust for incomplete data rows: valid indices in aRowsIn == 0 .. iNewRows-1
			if (iOldRows - iCntMisses < 1) // no usable rows ...
				return false;

			if (iOldRows - iCntMisses == iCntFrom) // every node has non-empty parent: need 1 extra row with Max Level + 1
				++iMaxLvl;


			ciCntColMap=iLvlSpan = iMaxLvl - iMinLvl + 1;
			if (iLvlSpan < 1 || iLvlSpan > ciCntColMap) // no data, corrupt data or data overflow otherwise
				return false;
			for (int j = 0; j < ciCntColMap; ++j) // adjust levels mapping in gColMap
			{
				int iNewLevel = (j < iLvlSpan) ? iMaxLvl - j : 99;
				gColMap[j].Level = iNewLevel;
			}

			if (iStartIdx > 0) // not sure if this can happen (depends on "order by" in query), but adjust just in case ...
			{
				SS_SplitGenealogy_Row tmp = aRowsIn[iStartIdx];
				aRowsIn[iStartIdx] = new SS_SplitGenealogy_Row(aRowsIn[0].FromLot, aRowsIn[0].ToLot, aRowsIn[0].Level, aRowsIn[0].LvlCnt);
				aRowsIn[0] = new SS_SplitGenealogy_Row(tmp.FromLot, tmp.ToLot, tmp.Level, tmp.LvlCnt);
			}

			// find / initialize 1st element(s)
			if (aRowsIn[0].FromLot != "") // the 1st element has a parent: create a input row for that parent 
			{
				for (int i = iNewRows - 1; i >= 0; i--) // make room for the additional row
					aRowsIn[i + 1] = new SS_SplitGenealogy_Row(aRowsIn[i].FromLot, aRowsIn[i].ToLot, aRowsIn[i].Level, aRowsIn[i].LvlCnt);
				aRowsIn[0] = new SS_SplitGenealogy_Row("", aRowsIn[1].FromLot, iMaxLvl, 1);
				++iNewRows; // aRowsIn was allocated with iOldRows+1, so ++iNewRows is OK
			}

			// scan array aRowsIn "depth-first" to create appropriately sorted output record set
			int[] aIdx = new int[iNewRows]; // keeping track of already used indices in input array (aIdx[inputIdx] = outputIdx)
			for (int i = 0; i < iNewRows; ++i) aIdx[i] = -1; // initialize to -1 for unused
			string[] aParent = new string[iLvlSpan]; // stack of parents, initialized to null
			int[] aNodeUsed = new int[iLvlSpan];     // stack of used nodes counts per level
			for (int j = 0; j < iLvlSpan; ++j) aNodeUsed[j] = 0; // initialize to 0
			int[] aNodeMax = new int[iLvlSpan];      // stack of max nodes per level, initialized in search loop below

			aParent[0] = ""; // no parent at level 0
			if (iLvlSpan > 1) // for iLvlSpan == 1 there would be only 1 row
				aParent[1] = aRowsIn[0].ToLot;
			else
				aParent[0] = aRowsIn[0].ToLot;
			// initialize max node per level counter (to shorten searches later)
			for (int i = 0; i < iNewRows; ++i)
			{
				for (int j = 0; j < iLvlSpan; ++j)
				{
					if (aRowsIn[i].Level == gColMap[j].Level)
					{
						++aNodeMax[j]; j = iLvlSpan; // stop looking after finding the level for node i
					}
				}
			}

			// create output rows list / record set
			RecordSet rsOutput = NewLevelsRecordSet(iNewRows);
			if (rsOutput == null)
				return false;

			// add 1st output row for levels grid (iOffset == 1 implies iLvlSpan > 1)
			AddLevelsRow(rsOutput, 0, aParent[(iLvlSpan>1)?1:0], 0);
			aNodeUsed[0] = 1; aIdx[0] = 0;

			// find the next output element, i.e. the one with current parent, and lowest level count
			int iLvlPtr = 1, iLoopCnt = 0;
			for (int i = 1; 0 < iLvlPtr && i < iNewRows; ++i) // output[0] already set
			{
				++iLoopCnt;  // keep track of number of iterations, should be < iNewRows^2
				int iFoundIdx = -1; // init to "not found"
				int iFoundCnt = -1; ; // init to 1 more than max nodes per level
				if (aNodeUsed[iLvlPtr] < aNodeMax[iLvlPtr])  // search only if there's still children at this level
				{
					for (int j = 1; j < iNewRows; ++j) // input[0] already used
					{
						if (0 <= aIdx[j]) // skip used entries
							continue;
						if (aRowsIn[j].FromLot == aParent[iLvlPtr] && (iFoundCnt < 0 || aRowsIn[j].LvlCnt < iFoundCnt)) // found a child with lowest count so far
						{
							iFoundIdx = j; iFoundCnt = aRowsIn[j].LvlCnt; // not done yet: keep looking for sibling with lower count
						}
					}
				}

				if (iFoundIdx >= 0) // successful search: copy to output, adjust bookkeeping
				{
					aIdx[iFoundIdx] = i; // mark aRowsIn[iFoundIdx] as used (as output[i])
					++aNodeUsed[iLvlPtr]; // increase node counter for this level
					AddLevelsRow(rsOutput, i, aRowsIn[iFoundIdx].ToLot, iLvlPtr); // insert ouput[i]
					if (i + 1 < iNewRows && iLvlPtr + 1 < iLvlSpan) // if more output rows to find, and we can increase the level
					{
						++iLvlPtr; aParent[iLvlPtr] = aRowsIn[iFoundIdx].ToLot; // set up search on new level with found lot as parent
					}
					// else: try a sibling ==> stay at same level
				}
				else // no assignment on this level ==> go back to previous level (decrease level)
				{
					if (iLvlPtr > 1) // can backtrack (decrease level)
					{
						iLvlPtr--; i--; // output node i not assigned: counteract the ++i at for loop statement
					}
					else // algorithm or input data error: return with partial result
					{
						return false;
					}
				}
			}

			// copy sorted data from list to lot level grid
			JQDataGrid grid = _grdLotLevels; // Page.FindCamstarControl("SplitLevelsGrid") as JQDataGrid;
			if (grid != null)
			{
				SEMI.AppCode.GridUtility.ItemListGrid_SetColumns(this, rsOutput.GetAsExplicitlyDataTable(), "SplitLevelsGrid", null, "_Query_Split_Genealogy_");
				SEMI.AppCode.GridUtility.ItemListGrid_BindDataTable(this, rsOutput.GetAsExplicitlyDataTable(), ref grid, "_Query_Split_Genealogy_" + rsOutput.Headers.Length);
			}

			// adjust grid column visibility
			for (int j = 0; j < ciCntColMap; ++j)
			{
				Boolean bLevelInUse = (j < iLvlSpan);
				(_grdLotLevels.GridContext as BoundContext).Fields[gColMap[j].ColumnName].Visible = bLevelInUse;
				(_grdLotLevels.GridContext as BoundContext).Fields[gColMap[j].ColumnName].LabelText = gColMap[j].Level.ToString();
			}
			(_grdLotLevels.GridContext as BoundContext).Fields["LotName"].Visible = false;
			(_grdLotLevels.GridContext as BoundContext).Fields["ColName"].Visible = false;
			CamstarWebControl.SetRenderToClient(_grdLotLevels);
			//Page.RenderToClient = true;
			return true;
		} // end AdjustGridData

		/// <summary>
		// execute the split genealogy query
		/// </summary>
		private Boolean ExecuteQuery(string sLotName)
		{
			// get the session and user profile
			var fs = FrameworkManagerUtil.GetFrameworkSession();

			QueryService objSvc = new QueryService(fs.CurrentUserProfile);
			QueryParameters objParameters = new QueryParameters();
			objParameters.Parameters = new QueryParameter[1];
			objParameters.Parameters[0] = new QueryParameter("ContainerName", sLotName);

			QueryOptions objOptions = new QueryOptions();
			objOptions.QueryType = OM.QueryType.User;
			objOptions.StartRow = 1;
			//objOptions.RowSetSize = intRowSize;
			RecordSet objResult;

			// execute the query
			ResultStatus objRS = objSvc.Execute("Split Genealogy", objParameters, objOptions, out objResult);

			if (objRS.IsSuccess)
			{
				Page.StatusBar.ClearMessage();
				return AdjustGridData(objResult); // repopulate the grid data
			}
			else
			{
				Page.DisplayMessage(objRS);
				return false;
			}
		} // end ExecuteQuery


		/// <summary>
		/// initializes the wafer details grid
		/// </summary>
		private OM.ResultStatus InitLotData()
		{
			// get the session and user profile
			var fs = FrameworkManagerUtil.GetFrameworkSession();

			ViewContainerStatusService oService = new ViewContainerStatusService(fs.CurrentUserProfile);
			ViewContainerStatus oSvcData = new ViewContainerStatus();
			oSvcData.Container = new ContainerRef(_txtContainerField.TextControl.Text);
			ViewContainerStatus_Info oSvcInfo = new ViewContainerStatus_Info();

			oSvcInfo.ComputerName = new Info(true);
			oSvcInfo.Container = new Info(true);
			oSvcInfo.ContainerStatusDetails = new ViewContainerAsParent_Info();
			oSvcInfo.ContainerStatusDetails.Workflow = new Info(true);
			oSvcInfo.ContainerStatusDetails.Product = new Info(true);
			oSvcInfo.Factory = new Info(true);
			oSvcInfo.Qty = new Info(true);
			oSvcInfo.Qty2 = new Info(true);
			oSvcInfo.NextStep = new Info(true);
			oSvcInfo.Step = new Info(true);

			ViewContainerStatus_Request oRequest = new ViewContainerStatus_Request();
			oRequest.Info = oSvcInfo;
			ViewContainerStatus_Result oResult = new ViewContainerStatus_Result();

			// execute to request the value(s)
			// OM.ResultStatus oResStat = oService.ExecuteTransaction(oRequest, out oResult);
			OM.ResultStatus oResStat = oService.GetEnvironment(oSvcData, oRequest, out oResult);

			if (oResStat.IsSuccess)
			{
				//if (oResult.Value.ComputerName != null)
				//    _txtComputerNameField.TextControl.Text = oResult.Value.ComputerName.ToString();
				if (oResult.Value.Container != null)
					_txtContainerField.TextControl.Text = oResult.Value.Container.Name;
				if (oResult.Value.Factory != null)
					_ndoFactory.Data = oResult.Value.Factory;
				if (oResult.Value.NextStep != null)
					_txtNextStep.Data = oResult.Value.NextStep.ToString();
				if (oResult.Value.Step != null)
					_txtStep.Data = oResult.Value.Step.ToString();
				if (oResult.Value.Qty != null)
					_txtQty.Data = oResult.Value.Qty.ToString();
				if (oResult.Value.Qty2 != null)
					_txtQty2.Data = oResult.Value.Qty2.ToString();
				if (oResult.Value.ContainerStatusDetails != null)
				{
					ViewContainerAsParent details = oResult.Value.ContainerStatusDetails;
					if (details.Product != null)
					{
						_txtProduct.Data = details.Product.Name;
						_txtProdRev.Data = details.Product.Revision;
					}
					if (details.Workflow != null)
					{
						_txtWorkflow.Data = details.Workflow.Name;
						_txtWFRev.Data = details.Workflow.Revision;
					}
				}
			}
			return oResStat;
		} // end InitLotData

		#endregion // PrivateFunctions

		#region Overrides

		/// <summary>
		/// override for the On Load method 
		/// </summary>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
            // _txtContainerField.TextChanged += new EventHandler(ContainerField_TextChanged);

            //hide or show the lot detail button based on current theme
            HideMoveLotDetailIcon();

            if (!Page.IsPostBack) // initial page load
			{
				_txtComputerNameField.Data = SEMI.AppCode.UIUtility.GetComputerName(this);
			}
			else // some post-back
			{
				if (_grdLotLevels != null && _grdLotLevels.GridContext != null)
				{
					string sSelRowId = _grdLotLevels.GridContext.SelectedRowID;
					if (!string.IsNullOrWhiteSpace(sSelRowId)) // post-back from grid selection
					{
						var oSelLotName = (_grdLotLevels.GridContext as BoundContext).GetCell(sSelRowId, "LotName");
						if (oSelLotName != null)
						{
							_txtContainerField.Data = oSelLotName.ToString();
						}
					}
				}
				//Page.RenderToClient = true;
			}
		} // end OnLoad

		/// <summary>
		/// override Web Part Custom Action method
		/// </summary>
		public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
		{
			base.WebPartCustomAction(sender, e);
			var action = e.Action as CustomAction;
			if (action != null)
			{
				switch (action.Parameters)
				{
					case "Clear":
					{
						Page.StatusBar.ClearMessage();
						Page.ClearValues();
						ResetFields();
						_txtContainerField.Focus();
						break;
					}
				}
			}
		} //end WebPartCustomAction

        public void HideMoveLotDetailIcon()
        {
            var theme = Page.Session["CurrentTheme"] != null ? Page.Session["CurrentTheme"].ToString() : string.Empty;

            if (theme.ToLower() == "horizon")
            {
                _btnLotAttributes.Visible = false;
                _btnOnlineTraveler.Visible = false;
                _btnDocumentSets.Visible = false;
                _btnMaterialsRequired.Visible = false;
            }
            else if (theme.ToLower() == "camstar")
            {
                _btnLotAttributes.Visible = true;
                _btnOnlineTraveler.Visible = true;
                _btnDocumentSets.Visible = true;
                _btnMaterialsRequired.Visible = true;

                //Disable command bar button on classic theme
                this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "LotAttributesPopup").First().IsHidden = true;
                this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "OnlineTraveler").First().IsHidden = true;
                this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "MaterialsRequired").First().IsHidden = true;
                this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "Documents").First().IsHidden = true;
            }
        }

        #endregion // Overrides

        #region Handlers

        /// <summary>
        /// call-back for text-changed on container field (_txtContainerField)
        /// </summary>
        public void ContainerField_TextChanged(object sender, EventArgs e)
		{
			Page.StatusBar.ClearMessage();
			if (_txtContainerField.Data != null)
			{
				OM.ResultStatus oResStat = InitLotData();
				if (oResStat.IsSuccess)
				{
					Boolean bGridOK = ExecuteQuery(_txtContainerField.TextControl.Text);
					if (!bGridOK)
					{
						ResetFields(false);
					}
					else
					{
						_btnLotAttributes.Enabled = _btnOnlineTraveler.Enabled = true;
						_btnDocumentSets.Enabled = _btnMaterialsRequired.Enabled = true;
					}
				}
				else
				{
					ResetFields(false);
					DisplayMessage(oResStat);
				}
				//Page.RenderToClient = true;
			}
			else
			{
				Page.ClearValues();
				ResetFields();
				_txtContainerField.Focus();
			}
		} // end ContainerField_TextChanged

		#endregion // Handlers
	}
}



