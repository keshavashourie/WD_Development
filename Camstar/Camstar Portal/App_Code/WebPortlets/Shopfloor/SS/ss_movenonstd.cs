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
	public class SS_MoveNonStd : scsShopfloorBase
    {

		# region Methods

		/// <summary>
		/// constructor for the move non standard class
		/// </summary>
		public SS_MoveNonStd()
		{
			
		}
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
				//GetLotQuerySelection(PrimaryServiceType, moveNonStdContainer.Data.ToString());
				JQDataGrid _gridLotSelectionX = lotDetailsGrid;
                SEMI.AppCode.UIUtility.GetLotQuerySelection(this, PrimaryServiceType, moveNonStdContainer.Data.ToString(), true, ref _gridLotSelectionX, "LotDetailsGrid");

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
				OM.WIPMoveNonStandard_Info txnInfo = new OM.WIPMoveNonStandard_Info();
				//the split bins check box is hidden if the container is at an item processing spec
				txnInfo.IsWaferProcessing = FieldInfoUtil.RequestValue();
				txnInfo.SelectionContainer = FieldInfoUtil.RequestValue();

				UserProfile profile = HttpContext.Current.Session[Camstar.WebPortal.Constants.SessionConstants.UserProfile] as UserProfile;

				WIPMoveNonStandardService svc = new WIPMoveNonStandardService(profile);
				WIPMoveNonStandard svcData = new WIPMoveNonStandard();
				svcData.SelectionId = selectionIdTextBox.Data.ToString();

				WIPMoveNonStandard_Request reqData = new WIPMoveNonStandard_Request();
				reqData.Info = txnInfo;
				WIPMoveNonStandard_Result resultData = new WIPMoveNonStandard_Result();
	
				WIPMoveNonStandard txn = new WIPMoveNonStandard();
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
			//Whack any previous data
			lotDetailsGrid.ClearData();

			// Whack the headers, and add back the magical space col (for looks)
			if (lotDetailsGrid.BoundContext.Fields.Count > 0)
			{
				JQFieldCollection objFieldClear = new JQFieldCollection();
				lotDetailsGrid.BoundContext.Fields = objFieldClear;
				lotDetailsGrid.BoundContext.Fields.Add(new JQField("_spacer") { LabelText = "&nbsp;", Visible= true });
			}
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
			
		} // WebPartCustomAction(object sender, CustomActionEventArgs e)

		public override void PostExecute(ResultStatus status, Service serviceData)
		{
			base.PostExecute(status, serviceData);

			if (status.IsSuccess == true)
				ClearLotDetailsGrid();
		}

        protected override void OnLoad(EventArgs e)
        {
			if (Page.IsPostBack)
			{
				base.OnLoad(e);
				if (_txtComputerName.Data == null)
					_txtComputerName.Data = SEMI.AppCode.UIUtility.GetComputerName(this);
				if (selectionIdTextBox.Data == null)
					ClearLotDetailsGrid();
			}
			else
			{
                string sContainer = Page.DataContract.GetValueByName("SelectedContainerNameDM") is ContainerRef ? (Page.DataContract.GetValueByName("SelectedContainerNameDM") as ContainerRef).Name : Page.DataContract.GetValueByName("SelectedContainerNameDM") as string;

                if (!string.IsNullOrEmpty(sContainer))
				{
					selectionIdTextBox.Data = sContainer;
					SelectionIdControl_DataChanged(null, null);
				}
			}
        }

#endregion

		#region PrivateProperites
				
		private CWC.TextBox selectionIdTextBox
		{
			get { return FindCamstarControl("WIPMoveNonStandard_SelectionId") as CWC.TextBox; }
		}
		private CWC.ContainerList moveNonStdContainer
		{
			get { return FindCamstarControl("WIPMoveNonStandard_Container") as CWC.ContainerList; }
		}
		private CWC.CheckBox splitBinsCheckBox
		{
			get { return FindCamstarControl("WIPMoveNonStandard_SplitBins") as CWC.CheckBox; }
		}

		private JQDataGrid lotDetailsGrid
		{
			get { return FindCamstarControl("LotDetailsGrid") as JQDataGrid; }
		}
        private CWC.TextBox _txtComputerName { get { return Page.FindCamstarControl("ComputerName") as CWC.TextBox; } }
		#endregion
	}
}



