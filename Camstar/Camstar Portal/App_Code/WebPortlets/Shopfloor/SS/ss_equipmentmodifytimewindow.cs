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
	public class SS_EquipmentModifyTimeWindow : MatrixWebPart
	{
		#region Properties
		
		protected JQDataGrid _gridMinDetails { get { return Page.FindCamstarControl("EquipmentModifyTimeWindow_MinDetails") as JQDataGrid; } }
		private SEMI.AppCode.DataEnvelopControl _envSelectedLots { get { return Page.FindCamstarControl("EquipmentModifyTimeWindow_SelectedLotsList") as SEMI.AppCode.DataEnvelopControl; } }
		protected CWC.TextBox txtComputerName { get { return Page.FindCamstarControl("EquipmentModifyTimeWindow_ComputerNameField") as CWC.TextBox; } }

		#endregion

		#region DataChangeEvents

		#endregion

		#region Private Methods
	

		#endregion

		#region Page Events

		/// <summary>
		/// This event is fired when the submit action button is clicked.
		/// </summary>
		/// <param name="serviceData"></param>
		public override void GetInputData(Service serviceData)
		{
			JQDataGrid MinDetailsGrid = Page.FindCamstarControl("EquipmentModifyTimeWindow_MinDetails") as JQDataGrid;

			MinTimeWindowDetails[] allDetails = (MinDetailsGrid.GridContext as BoundContext).Data as MinTimeWindowDetails[];

			if (allDetails != null)
			{
				int waferIndex = 0;
				MinTimeWindowDetails[] newDetails = new MinTimeWindowDetails[allDetails.Length];

				foreach (MinTimeWindowDetails detail in allDetails)
				{
					newDetails[waferIndex] = new MinTimeWindowDetails();
					newDetails[waferIndex].ListItemAction = ListItemAction.Add;
					newDetails[waferIndex].Container = detail.Container;
					newDetails[waferIndex].Equipment = detail.Equipment;
					newDetails[waferIndex].TrackInTimeStamp = detail.TrackInTimeStamp;
					newDetails[waferIndex].TrackInEmployeeName = detail.TrackInEmployeeName;
					newDetails[waferIndex].MinTime = detail.MinTime;
					newDetails[waferIndex].AlertFrame = detail.AlertFrame;
					newDetails[waferIndex].OvertimeFrame = detail.OvertimeFrame;
					newDetails[waferIndex].BeforetimeAction = detail.BeforetimeAction;
					newDetails[waferIndex].OvertimeAction = detail.OvertimeAction;
					newDetails[waferIndex].EmailGroup = detail.EmailGroup;
					newDetails[waferIndex].Comments = detail.Comments;
					waferIndex++;
				}
				(serviceData as EquipmentModifyTimeWindow).MinDetails = newDetails;
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

			//_txtSelectionId.DataChanged += new EventHandler(_txtSelectionId_DataChanged);
			txtComputerName.TextControl.Text = SEMI.AppCode.UIUtility.GetComputerName(this);		
		}

		/// <summary>
		/// Resets the page value to default
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
		{

			var action = e.Action as Camstar.WebPortal.Personalization.CustomAction;

			if (action != null && action.Parameters == "Reset")
			{
				Page.ClearValues();
                _gridMinDetails.ClearData();
			}
			base.WebPartCustomAction(sender, e);
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
				
			}
		}

		#endregion
	}
}



