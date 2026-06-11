// Copyright Siemens 2024
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
using Newtonsoft.Json;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.Personalization;
using CWF = Camstar.WebPortal.FormsFramework;
using WC = CamstarPortal.WebControls;
using System.Windows.Forms;


/// <summary>
/// Code behind for Supervisor and Technicians Jobs pages
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Jobs
{
    public class JobMain : MatrixWebPart
    {
        #region Property

        private JQDataGrid SearchResultGrid { get { return Page.FindCamstarControl("mainSearchGrid") as JQDataGrid; } }
        private CWC.NamedObject JobTypes { get { return Page.FindCamstarControl("JobTxn_JobTypes") as CWC.NamedObject; } }
        private CWC.DropDownList JobStatus { get { return Page.FindCamstarControl("JobTxn_JobStatus") as CWC.DropDownList; } }
        private CWC.TextBox QueryText { get { return Page.FindCamstarControl("HdnQuery") as CWC.TextBox; } }
        protected virtual JQDataGrid ResultsGrid { get { return Page.FindCamstarControl("mainSearchGrid") as JQDataGrid; } }
        private CWC.DropDownList IsSimpleMode { get { return Page.FindCamstarControl("JobTxn_IsSimpleMode") as CWC.DropDownList; } }


        bool gridClear = false; // to indicate whether the grid need selection value or not
        bool isSubmit = false;  // detect for submit / load from popup page 

        #endregion // property

        #region modified Event
        public JobMain()
        {
            //
            // TODO: Add constructor logic here
            //

        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            SearchResultGrid.RowSelected += ResultsGrid_RowSelected;

            string selectedRowID = SearchResultGrid.SelectedRowID;
            int currentGridPage = SearchResultGrid.GridContext.CurrentPage;
            if (SearchResultGrid.SelectedRowID != null)
            {
                Page.PortalContext.DataContract.SetValueByName("DCM_Resource", SearchResultGrid.GridContext.GetSelectedCell("Name"));
                Page.PortalContext.DataContract.SetValueByName("DCM_JobOrder", SearchResultGrid.GridContext.GetSelectedCell("JobOrder"));
            }

            if (!Page.IsPostBack) // first time load
                AskForRefresh();
            else
            {
                if (Page.EventArgument == "FloatingFrameSubmitParentPostBackArgument") // from javascript after submited
                {
                    isSubmit = true;
                    AskForRefresh();
                    if (selectedRowID != null && SearchResultGrid.TotalRowCount > 0)
                    {
                        SearchResultGrid.GridContext.CurrentPage = currentGridPage;
                        SearchResultGrid.GridContext.LoadData();
                        SearchResultGrid.GridContext.SelectRow(selectedRowID, true);
                        SearchResultGrid.GridContext.AdjustCurrentPage(selectedRowID);
                        CamstarWebControl.SetRenderToClient(SearchResultGrid);
                        ManagePanelButon();
                    }
                }
            }
        }
		
		private ResponseData ResultsGrid_RowSelected(object sender, JQGridEventArgs args)
        {
            ManagePanelButon(); // show & hide panel button 
            Page.PortalContext.DataContract.SetValueByName("ReturnJobStatusDM", null);
            Page.PortalContext.DataContract.SetValueByName("ReturnTechStatusDM", null);
            return null;
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
			ScriptManager.RegisterStartupScript(this, this.GetType(), "SearchLayout_AddSlideoutToogler", string.Format("SearchLayout_AddSlideoutToogler('{0}');", ResultsGrid.ClientID), true);
            SearchResultGrid.BoundContext.LBL("Lbl_NoDataToDisplay", "No DATA to display");
        }

        #endregion // modified event 

        #region Custom Procedure

        public void ManagePanelButon()
        {
            string varAssign = "pnlJobAssign";
            string varComplete = "pnlJobComplete";
            string varCancel = "pnlJobCancel";
            string varAcknowledge = "pnlJobAcknowledge";
            string varClockOn = "pnlJobClockOn";
            string varProgress = "pnlJobProgress";
            string varClockOff = "pnlClockOff";

            if ((QueryText.Data != null) && (QueryText.Data.ToString().Equals("_JobTxn_SelVal_JobOrders2")))// QueryText.Data.ToString().IndexOf("JobOrders2") > 0)
            {

                // set all disable
                if (this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varAcknowledge) != null)
                {
                    this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varAcknowledge).FirstOrDefault().IsHidden = true;
                    this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varClockOn).FirstOrDefault().IsHidden = true;
                    this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varProgress).FirstOrDefault().IsHidden = true;
                    this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varClockOff).FirstOrDefault().IsHidden = true;

                    if (SearchResultGrid.GridContext.SelectedRowID != null)
                    {
                        try
                        {
                            int SelectedRowInt = Convert.ToInt32(SearchResultGrid.GridContext.SelectedRowID);
                            DataTable dtGridTable = (SearchResultGrid.GridContext as BoundContext).Data as DataTable;
                            /// get selected row column value 
                            //string JobStatus = Page.PortalContext.DataContract.GetValueByName("ReturnJobStatusDM") != null ? Page.PortalContext.DataContract.GetValueByName("ReturnJobStatusDM").ToString() : (SearchResultGrid.GridContext.GetSelectedItems(false).GetValue(0) as DataRow)["JobStatus"].ToString();
                            //string JobStatus = dtGridTable.Rows[SelectedRowInt]["JobStatus"].ToString();
                            //string TecStatus = Page.PortalContext.DataContract.GetValueByName("ReturnTechStatusDM") != null ? Page.PortalContext.DataContract.GetValueByName("ReturnTechStatusDM").ToString() : (SearchResultGrid.GridContext.GetSelectedItems(false).GetValue(0) as DataRow)["TechnicianStatus"].ToString();
                            //string TecStatus = dtGridTable.Rows[SelectedRowInt]["TechnicianStatus"].ToString();

                            string JobStatus = (SearchResultGrid.GridContext as BoundContext).GetCell(SearchResultGrid.GridContext.SelectedRowID, "JobStatus").ToString();
                            string TecStatus = (SearchResultGrid.GridContext as BoundContext).GetCell(SearchResultGrid.GridContext.SelectedRowID, "TechnicianStatus").ToString();
       
                            // enable criteria for panel button 
                            bool enAcknowledge = (" ASSIGNED,ACKNOWLEDGED,ACTIVE".IndexOf(JobStatus) > 0 && TecStatus == "ACTIVE");
                            bool enClockOn = (" ACKNOWLEDGED,INPROGRESS,ACTIVE".IndexOf(JobStatus) > 0 && TecStatus == "ACTIVE");
                            bool enProgress = TecStatus == "INPROGRESS";
                            bool enClockOff = JobStatus == "INPROGRESS" && TecStatus == "INPROGRESS";

                            // set disable == !(enable)
                            this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varAcknowledge).FirstOrDefault().IsHidden = !enAcknowledge;
                            this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varClockOn).FirstOrDefault().IsHidden = !enClockOn;
                            this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varProgress).FirstOrDefault().IsHidden = !enProgress;
                            var clockOffBtn = this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varClockOff).FirstOrDefault();
                            clockOffBtn.IsHidden = !enClockOff;

                        }
                        catch (Exception)
                        {
                            this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varAcknowledge).FirstOrDefault().IsHidden = true;
                            this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varClockOn).FirstOrDefault().IsHidden = true;
                            this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varProgress).FirstOrDefault().IsHidden = true;
                            this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varClockOff).FirstOrDefault().IsHidden = true;
                        }
                    }
                }
            }
            else if ((QueryText.Data != null) && (QueryText.Data.ToString().Equals("_JobTxn_SelVal_JobOrders1")))
            {
                // set all disable
                if (this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varAssign) != null)
                {
                    this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varAssign).FirstOrDefault().IsHidden = true;
                    this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varComplete).FirstOrDefault().IsHidden = true;
                    this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varCancel).FirstOrDefault().IsHidden = true;

                    if (SearchResultGrid.GridContext.SelectedRowID != null)
                    {
                        try
                        {
                            int SelectedRowInt = Convert.ToInt32(SearchResultGrid.GridContext.SelectedRowID);
                            DataTable dtGridTable = (SearchResultGrid.GridContext as BoundContext).Data as DataTable;

                            /// get selected row column value
                            //string JobStatus = dtGridTable.Rows[SelectedRowInt]["JobStatus"].ToString();
                            //string AllowComplete = dtGridTable.Rows[SelectedRowInt]["AllowComplete"].ToString();
                            //string SimpleMode = dtGridTable.Rows[SelectedRowInt]["IsSimpleMode"].ToString();

                            string JobStatus = (SearchResultGrid.GridContext as BoundContext).GetCell(SearchResultGrid.GridContext.SelectedRowID, "JobStatus").ToString();
                            string AllowComplete = (SearchResultGrid.GridContext as BoundContext).GetCell(SearchResultGrid.GridContext.SelectedRowID, "AllowComplete").ToString();
                            string SimpleMode = (SearchResultGrid.GridContext as BoundContext).GetCell(SearchResultGrid.GridContext.SelectedRowID, "IsSimpleMode").ToString();

                            // enable criteria for panel button 
                            bool enAssign = JobStatus != "";
                            bool enComplete = AllowComplete == "True" && JobStatus == "ACTIVE";
                            bool enCancel = JobStatus != "" && JobStatus != "INPROGRESS";


                            // set disable == !(enable)
                            this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varAssign).FirstOrDefault().IsHidden = !enAssign;
                            this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varComplete).FirstOrDefault().IsHidden = !enComplete;
                            this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varCancel).FirstOrDefault().IsHidden = !enCancel;
                        }
                        catch (Exception)
                        {
                            this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varAssign).FirstOrDefault().IsHidden = true;
                            this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varComplete).FirstOrDefault().IsHidden = true;
                            this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varCancel).FirstOrDefault().IsHidden = true;
                        }
                    }
                }
            }
        }

        public void AskForRefresh()
        {
            string resource = (Page.FindCamstarControl("JobTxn_Resource") as CWC.NamedObject).Text;
            string jobType = JobTypes.Text;
            string jobStatus = (Page.FindCamstarControl("JobTxn_JobStatus") as CWC.DropDownList).Text;
            string resFamily = (Page.FindCamstarControl("JobTxn_ResourceFamily") as CWC.NamedObject).Text;
            string jobModel = (Page.FindCamstarControl("JobTxn_JobModel") as CWC.NamedObject).Text;
            string jobStage = (Page.FindCamstarControl("JobTxn_Stage") as CWC.NamedObject).Text;
            string queryTxt = (Page.FindCamstarControl("HdnQuery") as CWC.TextBox).Data.ToString();
            string isSimpleMode = IsSimpleMode.Data != null ? IsSimpleMode.Data.ToString() : "";


            gridClear = true;

            FrameworkSession currentSession = FrameworkManagerUtil.GetFrameworkSession(this.Page.Session);
            QueryUtil QueryService = new QueryUtil(currentSession.CurrentUserProfile);
            RecordSet queryResult = null;
            ResultStatus resultStatus = null;
            QueryOptions queryOption = new QueryOptions();
            queryOption.QueryType = Camstar.WCF.ObjectStack.QueryType.System; /// querytype = system
            queryOption.StartRow = 1; //(SearchResultGrid.GridContext.RowsPerPage * (SearchResultGrid.BoundContext.CurrentPage - 1)) + 1;// Start Row;
            queryOption.RowSetSize = 1000; // SearchResultGrid.GridContext.RowsPerPage;// Row Set Size;
                                           //queryOption.RequestRecordCount= true;

            QueryParameter[] qParam = new QueryParameter[7];
            qParam[0] = new QueryParameter("NameFilter", resource == "" ? "%" : resource);
            qParam[1] = new QueryParameter("JobType", jobType == "" ? "%" : jobType);
            qParam[2] = new QueryParameter("JobModel", jobModel == "" ? "%" : jobModel);
            qParam[3] = new QueryParameter("Stage", jobStage == "" ? "%" : jobStage);
            qParam[4] = new QueryParameter("JobStatus", jobStatus == "" ? "%" : jobStatus);
            qParam[5] = new QueryParameter("ResourceFamily", resFamily == "" ? "%" : resFamily);
            qParam[6] = new QueryParameter("IsSimpleMode", isSimpleMode == "" ? "%" : isSimpleMode);

            QueryService.Execute(queryTxt, qParam, queryOption, ref queryResult, ref resultStatus);

            if (queryResult.Rows != null)
            {
                //queryResult.TotalCount = queryResult.Rows.Count();
                //SearchResultGrid.Data = queryResult.GetAsDataTable();
                //SearchResultGrid.OriginalData = queryResult.GetAsDataTable();
                SearchResultGrid.ClearData();
                //SearchResultGrid.ClearSelectionValues();
                //JQDataGrid _gridSearchResultTemp = SearchResultGrid;
                SelectionValuesGrid_AddDataRow(SearchResultGrid, queryResult);
                //CamstarWebControl.SetRenderToClient(SearchResultGrid);
            }
            else
            {
                SearchResultGrid.ClearData();
                /*  05-11-2018  RDB     Previously was adding 'blank' rows for some reason...
                DataTable dt = new DataTable();
                dt.Columns.Add("nullCol", typeof(string)); 
                for (int i = 0; i < 11; i++)
                   dt.Rows.Add(""); 
                SearchResultGrid.Data = dt;
                SearchResultGrid.OriginalData = dt;
                */
            }
            if (!isSubmit)
            {
                SearchResultGrid.GridContext.SelectedRowIDs = null;
                SearchResultGrid.GridContext.SelectedRowIndex = null;
                SearchResultGrid.GridContext.SelectedRowID = null;

                //ManagePanelButon();
            }


        }

        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as CustomAction;
            if (action != null)
                if (action.Parameters == "AskRefresh")
                {
                    AskForRefresh();
                }
            if (action.Parameters == "Reset")
            {
                Page.ClearValues();
                AskForRefresh();
            }
        }

        protected void SelectionValuesGrid_AddDataRow(JQDataGrid TargetGrid, RecordSet RecordSetData)
        {
            try
            {
                //Add the lot details to grid.
                DataTable dtGridTable = (TargetGrid.GridContext as BoundContext).Data as DataTable;

                if (dtGridTable == null)
                {
                    
                    dtGridTable = new DataTable();
                    for (int x = 0; x <= RecordSetData.Headers.Length - 1; x++)
                        dtGridTable.Columns.Add(RecordSetData.Headers[x].Name);

                    foreach (Row rsRow in RecordSetData.Rows)
                    {
                        DataRow dtRow = dtGridTable.NewRow();
                        for (int x = 0; x <= RecordSetData.Headers.Length - 1; x++)
                        {
                            dtRow.SetField(RecordSetData.Headers[x].Name, rsRow.Values[x]);
                        }
                        dtGridTable.Rows.Add(dtRow);
                    }
                    //TargetGrid.Data = dtGridTable;
                    //TargetGrid.OriginalData = dtGridTable;
                    TargetGrid.SetSelectionValues(RecordSetData);
                }
                else
                {
                    DataRow drGridRow = dtGridTable.NewRow();

                    for (int x = 0; x <= RecordSetData.Headers.Length - 1; x++)
                    {
                        drGridRow.SetField(RecordSetData.Headers[x].Name, RecordSetData.Rows[0].Values[x]);
                    }
                    dtGridTable.Rows.Add(drGridRow);
                    TargetGrid.Data = dtGridTable;
                    TargetGrid.OriginalData = dtGridTable;
                }
                CamstarWebControl.SetRenderToClient(TargetGrid);
            }
            catch (Exception ex)
            { }
        } // SelectionValuesGrid_AddDataRow

        #endregion // Custom Procedure

    }
}
