// Copyright Siemens 2023  
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls.WebParts;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.WebPortlets;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.FormsFramework.WebControls;
using System.Data;
using System.Linq;
using System.Collections;
using Camstar.WCF.ObjectStack;

using OM = Camstar.WCF.ObjectStack;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using CWGC = Camstar.WebPortal.FormsFramework.WebGridControls;

using PERS = Camstar.WebPortal.Personalization;

using Camstar.WebPortal.WCFUtilities;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework;

namespace Camstar.WebPortal.WebPortlets
{
    public class TrainingRecordManagement : MatrixWebPart
    {
        private const string mFloatingFrameSubmitParentPostBackArgument = "FloatingFrameSubmitParentPostBackArgument";

        protected virtual CWC.Button UpdateBtn
        {
            get { return Page.FindCamstarControl("UpdateBtn") as Button; }
        }

        protected virtual CWC.Button DeleteBtn
        {
            get { return Page.FindCamstarControl("DeleteBtn") as Button; }
        }

        protected virtual JQDataGrid TrainingRequirementsListGrid
        {
            get { return Page.FindCamstarControl("TrainingRequirementsListGrid") as JQDataGrid; }
        }

        protected override void OnPreRender(EventArgs e)
        {
            if (Page.Request["__EVENTARGUMENT"] == mFloatingFrameSubmitParentPostBackArgument)
            {
                var currPage = TrainingRequirementsListGrid.BoundContext.CurrentPage.ToString();
                TrainingRequirementsListGrid.ClearData();
                TrainingRequirementsListGrid.Action_Reload(currPage);
            }
            base.OnPreRender(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            SetEditButtons();
        }

        protected virtual void SetEditButtons()
        {
            if (TrainingRequirementsListGrid.Data != null && TrainingRequirementsListGrid.IsRowSelected)
            {
                UpdateBtn.Enabled = true;
                DeleteBtn.Enabled = true;
                DeleteBtn.Attributes["actionCommandName"] = ((int)ESigMaintActions.Delete).ToString();
                DeleteBtn.ServiceType = "TrainingRecordMaint";
            }
            else
            {
                UpdateBtn.Enabled = false;
                DeleteBtn.Enabled = false;
            }
        }


        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as PERS.CustomAction;
            if (action != null)
            {
                switch (action.Name)
                {
                    case "DeleteBtn":
                        {
                            e.Result = ExecuteDeleteAction();
                            if (e.Result != null && e.Result.IsSuccess)
                                TrainingRequirementsListGrid.Action_Reload("");
                            break;
                        }
                }
            }
        }

        protected virtual ResultStatus ExecuteDeleteAction()
        {
            if (TrainingRequirementsListGrid.GridContext.SelectedRowID == null)
                return null;

            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(Page.Session);
            WSDataCreator creator = new WSDataCreator();

            TrainingRecordMaint serviceData = creator.CreateServiceData("TrainingRecordMaint") as TrainingRecordMaint;
            TrainingRecordMaint_Info serviceInfo = creator.CreateServiceInfo("TrainingRecordMaint") as TrainingRecordMaint_Info;

            IMaintenanceBase service = creator.CreateService("TrainingRecordMaint", session.CurrentUserProfile) as IMaintenanceBase;
            Request request = creator.CreateObject("TrainingRecordMaint_Request") as Request;
            Result result = creator.CreateObject("TrainingRecordMaint_Result") as Result;

            request.Info = serviceInfo;

            service.BeginTransaction();

            TrainingRecordMaint data1 = creator.CreateServiceData("TrainingRecordMaint") as TrainingRecordMaint;

            var gridData = TrainingRequirementsListGrid.Data as DataTable;
            if (gridData != null && !string.IsNullOrEmpty(TrainingRequirementsListGrid.GridContext.SelectedRowID))
            {
                var selectedRow = TrainingRequirementsListGrid.GridContext.GetItem(TrainingRequirementsListGrid.GridContext.SelectedRowID) as DataRow;
                if (selectedRow != null)
                {
                    data1.ParentDataObject = new OM.NamedObjectRef((selectedRow as DataRow)["Employee"].ToString());
                    data1.TrainingRequirement = new OM.RevisionedObjectRef((selectedRow as DataRow)["TrainingRequirement"].ToString(), (selectedRow as DataRow)["Revision"].ToString());
                }
            }
            var eSigDetails = ESigCaptureUtil.CollectESigServiceDetailsAll();
            if (eSigDetails != null)
            {
                if (eSigDetails.Item1 != null && eSigDetails.Item1.Length > 0)
                    data1.ESigDetails = eSigDetails.Item1;
            }
            
            service.Delete(data1);
            service.ExecuteTransaction();

            var status = service.CommitTransaction(request, out result);
            ESigCaptureUtil.CleanESigCaptureDM();
           return status;
        }         
    }
}
