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
/// The code for the adjust material lot quantity virtual page.  Resolves the lot based on the 
/// selection id entered (or selected via popup) and populates the lot details grid (1 row only).
/// </summary>
/// 
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
	public class SS_MaterialAdjustQty : scsShopfloorBase
	{

		# region Methods

		/// <summary>
		/// constructor for the move non standard class
		/// </summary>
		public SS_MaterialAdjustQty() {}

		/// <summary>
		/// Triggered when a (material) lot etc... is entered on the page
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void SelectionIdControl_DataChanged() // used to be (object sender, EventArgs e)
		{

			if (selectionIdTextBox.IsEmpty)
				return;

			ClearLotDetailsGrid();
			adjustQtyContainer.Data = null;
			DisplayMessage(new ResultStatus("", true));

			//first, resolve the lot / container
			if (GetSelectionId() && adjustQtyContainer.Data != null)
			{
				//then populate the lot details grid
				//GetLotQuerySelection(PrimaryServiceType, moveNonStdContainer.Data.ToString());
				JQDataGrid _gridLotSelectionX = lotDetailsGrid;
                SEMI.AppCode.UIUtility.GetLotQuerySelection(this, PrimaryServiceType, adjustQtyContainer.Data.ToString()
															, true, ref _gridLotSelectionX, "LotDetailsGrid");

			}
		} //end SelectionIdControl_DataChanged()

		/// <summary>
		/// resolves the container name (lot name) from the value of the selection id field
		/// </summary>
		/// <returns></returns>
		private Boolean GetSelectionId()
		{

			try
			{
				OM.MaterialAdjustQty_Info txnInfo = new OM.MaterialAdjustQty_Info();
				txnInfo.SelectionContainer = FieldInfoUtil.RequestValue();

				UserProfile profile = HttpContext.Current.Session[Camstar.WebPortal.Constants.SessionConstants.UserProfile] as UserProfile;

				MaterialAdjustQtyService svc = new MaterialAdjustQtyService(profile);
				MaterialAdjustQty svcData = new MaterialAdjustQty();
				svcData.SelectionId = selectionIdTextBox.Data.ToString();

				MaterialAdjustQty_Request reqData = new MaterialAdjustQty_Request();
				reqData.Info = txnInfo;
				MaterialAdjustQty_Result resultData = new MaterialAdjustQty_Result();
	
				MaterialAdjustQty txn = new MaterialAdjustQty();
				ResultStatus results = svc.ResolveSelectionId(svcData, reqData, out resultData);

				if (results.IsSuccess)
				{
					adjustQtyContainer.Data = resultData.Value.SelectionContainer;
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

		} //end GetSelectionId()

		/// <summary>
		/// simply clears the grid (and adds 1 empty placeholder row)
		/// </summary>
		private void ClearLotDetailsGrid()
		{
			// Whack any previous data
			lotDetailsGrid.ClearData();

			// Whack the headers, and add back the magical space col (for looks)
			if (lotDetailsGrid.BoundContext.Fields.Count > 0)
			{
				JQFieldCollection objFieldClear = new JQFieldCollection();
				lotDetailsGrid.BoundContext.Fields = objFieldClear;
				lotDetailsGrid.BoundContext.Fields.Add(new JQField("_spacer") { LabelText = "&nbsp;", Visible= true });
			}
		} //end ClearLotDetailsGrid()

		/// <summary>
		/// clears all input fields on the page, focus on selection ID input field
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
				ClearLotDetailsGrid();
				selectionIdTextBox.Focus();
			}
			
			DisplayMessage(new ResultStatus("", true));
			
		} //end WebPartCustomAction(object sender, CustomActionEventArgs e)

		/// <summary>
		/// on successful TXN call ClearLotDetailsGrid()
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public override void PostExecute(ResultStatus status, Service serviceData)
		{
			base.PostExecute(status, serviceData);

			if (status.IsSuccess == true)
				ClearLotDetailsGrid();
		} //end PostExecute(ResultStatus status, Service serviceData)

		/// <summary>
		/// page reload action (also for PostBack (on popupClose))
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			txtComputerName.TextControl.Text = SEMI.AppCode.UIUtility.GetComputerName(this);

			if (Page.IsPostBack)
			{
				// Check if it is a pop up close, get the return result
				if (SEMI.AppCode.UIUtility.IsPopupClose(this))
				{
					if (adjustQtyDataEnv != null)
					{
						// manually initialize the containers list data contract since it is not triggered when we do a manual popup close
						if (Page.DataContract.GetValueByName("MaterialAdjustQty_ContainerDataEnvDM") != null)
							adjustQtyDataEnv.SS_ContainersList = Page.DataContract.GetValueByName("MaterialAdjustQty_ContainerDataEnvDM") as string[];
					}

					if (adjustQtyDataEnv.SS_ContainersList != null)
					{
						// string[] sContainers = adjustQtyDataEnv.SS_ContainersList;
						// selectionIdTextBox.TextControl.Text = sContainers[0];
						selectionIdTextBox.TextControl.Text = adjustQtyDataEnv.SS_ContainersList[0];
						SelectionIdControl_DataChanged();

						// nullify the containers list
						adjustQtyDataEnv.SS_ContainersList = null;
					}
				}
			}
		} //end OnLoad(EventArgs e)

		#endregion // Methods

		#region PrivateProperites
				
		private CWC.TextBox selectionIdTextBox
		{
			get { return FindCamstarControl("MaterialAdjustQty_SelectionId") as CWC.TextBox; }
		}

		private CWC.ContainerList adjustQtyContainer
		{
			get { return FindCamstarControl("MaterialAdjustQty_ContainerField") as CWC.ContainerList; }
		}

		private SEMI.AppCode.DataEnvelopControl adjustQtyDataEnv
		{
			get { return FindCamstarControl("MaterialAdjustQty_ContainerDataEnv") as SEMI.AppCode.DataEnvelopControl; }
		}

		private CWC.TextBox txtComputerName
		{
			get { return FindCamstarControl("MaterialAdjustQty_ComputerName") as CWC.TextBox; }
		}

		private JQDataGrid lotDetailsGrid
		{
			get { return FindCamstarControl("LotDetailsGrid") as JQDataGrid; }
		}

		#endregion // PrivateProperties
	}
}



