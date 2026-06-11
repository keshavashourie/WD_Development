/* Copyright 2019 Siemens */
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

/// <summary>
/// The code for the move non standard virtual page.  Resolves the lot based on the 
/// selection id entered and populates the lot details grid.
/// </summary>
/// 
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
	public class SS_LotRework : MatrixWebPart
	{
		#region PrivateProperites

		private CWC.TextBox selectionIdTextBox
		{
			get { return FindCamstarControl("LotRework_SelectionId") as CWC.TextBox; }
		}
		private CWC.ContainerList moveNonStdContainer
		{
			get { return FindCamstarControl("LotRework_Container") as CWC.ContainerList; }
		}
		private CWC.CheckBox splitBinsCheckBox
		{
			get { return FindCamstarControl("LotRework_SplitBins") as CWC.CheckBox; }
		}

		private JQDataGrid lotDetailsGrid
		{
			get { return FindCamstarControl("LotDetailsGrid") as JQDataGrid; }
		}

        private CWC.TextBox _txtComputerName { get { return Page.FindCamstarControl("LotRework_ComputerName") as CWC.TextBox; } }
		#endregion

		# region Methods
		
		/// <summary>
		/// Triggered when a lot / carrier etc... is entered on the page
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void SelectionIdControl_DataChanged(object sender, EventArgs e)
		{

			if (selectionIdTextBox.IsEmpty)
				return;

			ClearLotDetailsGrid();
			moveNonStdContainer.Data = null;
			DisplayMessage(new ResultStatus("", true));

			//first, resolve the lot / container
			if (GetSelectionId() && moveNonStdContainer.Data != null)
			{
				//then populate the lot details grid

				JQDataGrid _gridLotSelectionX = lotDetailsGrid;
				SEMI.AppCode.UIUtility.GetLotQuerySelection(this, this.PrimaryServiceType, moveNonStdContainer.Data.ToString(), true, ref _gridLotSelectionX, "LotDetailsGrid");

			}
		}

		/// <summary>
		/// resolves the container name (lot name) from the value of the selection id field
		/// </summary>
		/// <returns></returns>
		private Boolean GetSelectionId()
		{

			try
			{
				OM.LotRework_Info txnInfo = new OM.LotRework_Info();
				//the split bins check box is hidden if the container is at an item processing spec
				txnInfo.IsWaferProcessing = FieldInfoUtil.RequestValue();
				txnInfo.SelectionContainer = FieldInfoUtil.RequestValue();

				UserProfile profile = HttpContext.Current.Session[Camstar.WebPortal.Constants.SessionConstants.UserProfile] as UserProfile;

				LotReworkService svc = new LotReworkService(profile);
				LotRework svcData = new LotRework();
				svcData.SelectionId = selectionIdTextBox.Data.ToString();

				LotRework_Request reqData = new LotRework_Request();
				reqData.Info = txnInfo;
				LotRework_Result resultData = new LotRework_Result();

				LotRework txn = new LotRework();
				ResultStatus results = svc.ResolveSelectionId(svcData, reqData, out resultData);

				if (results.IsSuccess)
				{
					moveNonStdContainer.Data = resultData.Value.SelectionContainer;
					splitBinsCheckBox.Visible = (resultData.Value.IsWaferProcessing == false);
					return true;
				}
				else
				{
					DisplayMessage(results);
					return false;
				}

			}
			catch (Exception ex)
			{
				DisplayMessage(new ResultStatus(ex.Message, false));
				return false;
			}

		} //end Get Selection Id

		/// <summary>
		/// simply clears the grid
		/// </summary>
		private void ClearLotDetailsGrid()
		{
			//Clears any previous data
			lotDetailsGrid.ClearData();

			// Clears the headers
			if (lotDetailsGrid.BoundContext.Fields.Count > 0)
			{
				JQFieldCollection objFieldClear = new JQFieldCollection();
				lotDetailsGrid.BoundContext.Fields = objFieldClear;
				lotDetailsGrid.BoundContext.Fields.Add(new JQField("_spacer") { LabelText = "&nbsp;", Visible = true });
			}
			OnLoad(null);
		}

		/// <summary>
		/// Clears the query grid
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
		{
			base.WebPartCustomAction(sender, e);

			ClearLotDetailsGrid();
            Page.ShopfloorReset(sender, e);
			DisplayMessage(new ResultStatus("", true));

		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			splitBinsCheckBox.Visible = false;
            if (_txtComputerName.Data == null)
                _txtComputerName.Data = SEMI.AppCode.UIUtility.GetComputerName(this);
            
		}

		/// <summary>
		/// Runs after Submit is clicked
		/// </summary>
		/// <param name="status"></param>
		/// <param name="serviceData"></param>
		public override void PostExecute(ResultStatus status, Service serviceData)
		{
			base.PostExecute(status, serviceData);

			if (status.IsSuccess == true)
				ClearLotDetailsGrid();
		}

		#endregion
	}
}



