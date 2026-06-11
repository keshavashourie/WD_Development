// © 2018 Siemens Product Lifecycle Management Software Inc.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.FormsFramework;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class TeamTrackingTxn : MatrixWebPart
    {
        protected JQDataGrid TeamWorkAreaGrid { get { return Page.FindCamstarControl("TeamTrackingTxn_TeamTrackingStatus") as JQDataGrid; } }
        protected CWC.NamedObject SigningEmployee { get { return Page.FindCamstarControl("TeamTrackingTxn_SigningEmployee") as CWC.NamedObject; } }
        protected CWC.Button SignInButton { get { return Page.FindCamstarControl("SignIn") as CWC.Button; } }
        protected CWC.Button SignOutButton { get { return Page.FindCamstarControl("SignOut") as CWC.Button; } }
		protected CWC.CheckBox AllowMultipleSignIn { get { return Page.FindCamstarControl("WorkAreaSignOut_EmpAllowMultipleSignIn") as CWC.CheckBox; } }
		protected CWC.CheckBox TeamAllowMultipleSignIn { get { return Page.FindCamstarControl("WorkAreaSignOut_TeamAllowMultipleSignIn") as CWC.CheckBox; } }
        protected CWC.NamedObject Operation { get { return Page.FindCamstarControl("TeamTrackingTxn_Operation") as CWC.NamedObject; } }
        protected CWC.RevisionedObject Spec { get { return Page.FindCamstarControl("TeamTrackingTxn_Spec") as CWC.RevisionedObject; } }
        protected CWC.NamedObject Workstation { get { return Page.FindCamstarControl("TeamTrackingTxn_Workstation") as CWC.NamedObject; } }
        protected CWC.NamedObject WorkCenter { get { return Page.FindCamstarControl("TeamTrackingTxn_WorkCenter") as CWC.NamedObject; } }
        protected CWC.NamedObject WorkCell { get { return Page.FindCamstarControl("TeamTrackingTxn_WorkCell") as CWC.NamedObject; } }
		protected CWC.NamedObject Team { get { return Page.FindCamstarControl("WorkAreaSignIn_Team") as CWC.NamedObject; } }
        protected CWC.NamedObject Resource { get { return Page.FindCamstarControl("TeamTrackingTxn_Resource") as CWC.NamedObject; } }

        protected override void OnLoad(System.EventArgs e)
        {
            base.OnLoad(e);
            TeamWorkAreaGrid.GridContext.PostBackOnSelect = true;
            TeamWorkAreaGrid.BoundContext.SnapCompleted += new SnapCompletedHandler(BoundContext_SnapCompleted);
            SigningEmployee.DataChanged += new EventHandler(SigningEmployee_DataChanged);
			Team.DataChanged += new EventHandler(Team_DataChanged);
            TeamWorkAreaGrid.PreRender += new EventHandler(TeamWorkAreaGrid_PreRender);
        }

        void BoundContext_SnapCompleted(System.Data.DataTable dataWindowTable)
        {
            if (TeamWorkAreaGrid.TotalRowCount == 1)
            {
                TeamWorkAreaGrid.GridContext.SelectRow(TeamWorkAreaGrid.GridContext.GetRowId(0), true);
                CamstarWebControl.SetRenderToClient(TeamWorkAreaGrid);
                SignOutButton.Enabled = true;
            }
        }
        
        public override void GetInputData(OM.Service serviceData)
        {
            base.GetInputData(serviceData);
            if (serviceData is OM.mdWorkAreaSignOut)
            {
                int selectedCount = TeamWorkAreaGrid.GridContext.GetSelectedCount();
                var selectedStatuses = TeamWorkAreaGrid.GridContext.GetSelectedItems(false);
                OM.Primitive<string>[] statusIds = new OM.Primitive<string>[selectedCount];
                for (int i = 0; i < selectedCount; i++)
                {
                    statusIds[i] = new OM.Primitive<string>((selectedStatuses[i] as OM.mdTeamTrackingStatus).Self.ID);
                }
                (serviceData as OM.mdWorkAreaSignOut).mdSelectedTeamStatusIds = statusIds;
		        (serviceData as OM.mdWorkAreaSignOut).mdTeamTrackingStatuses = null;
            }
            else if(serviceData is OM.mdWorkAreaSignIn)
            {
                (serviceData as OM.mdWorkAreaSignIn).mdTeamTrackingStatuses = null;
            }
        }

        public void SigningEmployee_DataChanged(object sender, EventArgs e)
        {
			Team.ClearData();
            TeamWorkAreaGrid.ClearData();
        }

		public void Team_DataChanged(object sender, EventArgs e)
		{
			SigningEmployee.ClearData();
			TeamWorkAreaGrid.ClearData();
		}

        public void TeamWorkAreaGrid_PreRender(object sender, EventArgs e)
        {
            if (((TeamWorkAreaGrid.TotalRowCount != 0) && (!AllowMultipleSignIn.IsChecked) && (Team.IsEmpty)) ||
                ((TeamWorkAreaGrid.TotalRowCount != 0) && (!TeamAllowMultipleSignIn.IsChecked) && (SigningEmployee.IsEmpty)) ||
                (SigningEmployee.IsEmpty && Team.IsEmpty) || (Workstation.IsEmpty && WorkCell.IsEmpty && WorkCenter.IsEmpty && Operation.IsEmpty && Spec.IsEmpty && Resource.IsEmpty))
                SignInButton.Enabled = false;
            else
                SignInButton.Enabled = true;
            if (TeamWorkAreaGrid.GridContext.GetSelectedCount() == 0)
                SignOutButton.Enabled = false;
            else
                SignOutButton.Enabled = true;
        }

        public override void WebPartCustomAction(object sender, CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            if (e.Action.Name == "ClearActions")
            {
                Spec.Visible = false;
                WorkCell.Hidden = true;
                WorkCenter.Hidden = true;
                Workstation.Hidden = true;
                Resource.Hidden = true;
                Operation.Hidden = false;
                Operation.Visible = true;
            }
        }

        public override void PostExecute(OM.ResultStatus status, OM.Service serviceData)
        {
            base.PostExecute(status, serviceData);
            if (status.IsSuccess)
            {
                Operation.Data = null;
                WorkCell.Data = null;
                Workstation.Data = null;
                WorkCenter.Data = null;
                Spec.Data = null;
				Team.Data = null;
                Resource.Data = null;
            }
        }
    }
}