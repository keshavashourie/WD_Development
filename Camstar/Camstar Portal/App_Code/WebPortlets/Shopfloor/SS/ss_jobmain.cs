/* Copyright 2022 Siemens */
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


/// <summary>
/// Summary description for SS_JobGeneral
/// </summary>


namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_JobMain : MatrixWebPart, IPostBackEventHandler
    {
        #region Property

        // Controls
        protected virtual JQDataGrid _gridSearchResult { get { return Page.FindCamstarControl("mainSearchGrid") as JQDataGrid; } }
        private CWC.NamedObject _ndoJobTypes { get { return Page.FindCamstarControl("JobTxn_JobTypes") as CWC.NamedObject; } }
        private CWC.DropDownList _ddlJobStatus { get { return Page.FindCamstarControl("JobTxn_JobStatus") as CWC.DropDownList; } }
        private CWC.TextBox _txtQueryText { get { return Page.FindCamstarControl("HdnQuery") as CWC.TextBox; } }
        private CWC.DropDownList _ddlIsSimpleMode { get { return Page.FindCamstarControl("JobTxn_IsSimpleMode") as CWC.DropDownList; } }
          
        // global Variable
        private bool gridClear = false; // to indicate whether the grid need selection value or not
        private bool isSubmit = false;  // detect for submit / load from popup page 
      
        #endregion // property

        #region PageEvent

        //
        // set event handler on Page Init
        //
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            Page.OnPostExecute += new EventHandler<ResultEventArgs>(Page_OnPostExecute);
        }

        //
        // override setGeneral Fields, and addon necessary field to set 
        //
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (_gridSearchResult.SelectedRowID != null)
            {
                Page.PortalContext.DataContract.SetValueByName("DCM_Resource", _gridSearchResult.GridContext.GetSelectedCell("Name"));
                Page.PortalContext.DataContract.SetValueByName("DCM_JobOrder", _gridSearchResult.GridContext.GetSelectedCell("JobOrder")); 
            }

            if (!Page.IsPostBack) // first time load
                AskForRefresh();
            else if (Page.EventArgument == "FloatingFrameSubmitParentPostBackArgument" || Page.EventArgument == "OnRowSelected") // from javascript after submited
            {
                isSubmit = true;
                AskForRefresh();
            }

            if (!Page.IsPostBack)
                _ddlJobStatus.PickListPanelControl.ViewControl.ReloadData();
            
            ManagePanelButon(); // show & hide panel button 
        }

        //
        // Before Render , manage panel button 
        //
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            ManagePanelButon();
        }

        //
        // Refresh the Grid after Execute 
        //
        void Page_OnPostExecute(object sender, ResultEventArgs e)
        {
            if (e.Status.IsSuccess)
                AskForRefresh();
        }

        //
        // Constructor currenltly still nothing to do
        //
        public SS_JobMain()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        //
        // Postback event , currently still empty 
        //
        public void RaisePostBackEvent(string eventArgument)
        {

        }

        #endregion // Page Event

        #region Custom Procedure

        //
        // Dinamically Enable or Disable button in THe Menu 
        //
        public void ManagePanelButon()
        {
            string varAcknowledge = "pnlJobAcknowledge";
            string varClockOn = "pnlJobClockOn";
            string varProgress = "pnlJobProgress";
            string varClockOff = "pnlClockOff";

            if ((_txtQueryText.Data != null) && (_txtQueryText.Data.ToString().Equals("_JobTxn_SelVal_JobOrders2")))// _txtQueryText.Data.ToString().IndexOf("JobOrders2") > 0)
            {

                // set all disable
                if (this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varAcknowledge) != null)
                {
                    this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varAcknowledge).FirstOrDefault().IsDisabled = true;
                    this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varClockOn).FirstOrDefault().IsDisabled = true;
                    this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varProgress).FirstOrDefault().IsDisabled = true;
                    this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varClockOff).FirstOrDefault().IsDisabled = true;

                    if (_gridSearchResult.GridContext.SelectedRowID != null)
                    {
                        try
                        {
                            /// get selected row column value 
                            string _ddlJobStatus = Page.PortalContext.DataContract.GetValueByName("Return_ddlJobStatusDM") != null ? Page.PortalContext.DataContract.GetValueByName("Return_ddlJobStatusDM").ToString() : _gridSearchResult.GridContext.GetSelectedCell("JobStatus").ToString();

                            string TecStatus = Page.PortalContext.DataContract.GetValueByName("ReturnTechStatusDM") != null ? Page.PortalContext.DataContract.GetValueByName("ReturnTechStatusDM").ToString() : _gridSearchResult.GridContext.GetSelectedCell("TechnicianStatus").ToString();

                            string IsSimpleMode = Page.PortalContext.DataContract.GetValueByName("ReturnIsSimpleModeStatusDM") != null ? Page.PortalContext.DataContract.GetValueByName("ReturnIsSimpleModeStatusDM").ToString() : _gridSearchResult.GridContext.GetSelectedCell("IsSimpleMode").ToString();

                            // enable criteria for panel button 
                            bool enAcknowledge = (" ASSIGNED,ACKNOWLEDGED,ACTIVE".IndexOf(_ddlJobStatus) > 0 && TecStatus == "ACTIVE");
                            bool enClockOn = (" ACKNOWLEDGED,INPROGRESS,ACTIVE".IndexOf(_ddlJobStatus) > 0 && TecStatus == "ACTIVE");
                            bool enProgress = TecStatus == "INPROGRESS";
                            bool enClockOff = _ddlJobStatus == "INPROGRESS" && TecStatus == "INPROGRESS";



                            // set disable == !(enable)
                            this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varAcknowledge).FirstOrDefault().IsDisabled = !enAcknowledge;
                            this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varClockOn).FirstOrDefault().IsDisabled = !enClockOn;
                            this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varProgress).FirstOrDefault().IsDisabled = !enProgress;
                            this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varClockOff).FirstOrDefault().IsDisabled = !enClockOff;

                        }
                        catch (Exception)
                        {
                            this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varAcknowledge).FirstOrDefault().IsDisabled = true;
                            this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varClockOn).FirstOrDefault().IsDisabled = true;
                            this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varProgress).FirstOrDefault().IsDisabled = true;
                            this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varClockOff).FirstOrDefault().IsDisabled = true;
                        }
                    }
                }
            }


        }

        //
        // Refresh the Grid 
        //
        public void AskForRefresh()
        {
            string resource = (Page.FindCamstarControl("JobTxn_Resource") as CWC.NamedObject).Text;
            string jobType = _ndoJobTypes.Text;
            string jobStatus = (Page.FindCamstarControl("JobTxn_JobStatus") as CWC.DropDownList).Text;
            string resFamily = (Page.FindCamstarControl("JobTxn_ResourceFamily") as CWC.NamedObject).Text;
            string jobModel = (Page.FindCamstarControl("JobTxn_JobModel") as CWC.NamedObject).Text;
            string jobStage = (Page.FindCamstarControl("JobTxn_Stage") as CWC.NamedObject).Text;
            string queryTxt = (Page.FindCamstarControl("HdnQuery") as CWC.TextBox).Data.ToString();
            string isSimpleMode = _ddlIsSimpleMode.Data != null ? _ddlIsSimpleMode.Data.ToString() : "";

            gridClear = true; 

            FrameworkSession currentSession = FrameworkManagerUtil.GetFrameworkSession(this.Page.Session);
            QueryUtil QueryService = new QueryUtil(currentSession.CurrentUserProfile);
            RecordSet queryResult = null;
            ResultStatus resultStatus = null;
            QueryOptions queryOption = new QueryOptions();
            queryOption.QueryType = Camstar.WCF.ObjectStack.QueryType.System; /// querytype = system
            //queryOption.StartRow = (_gridSearchResult.GridContext.RowsPerPage * (_gridSearchResult.BoundContext.CurrentPage - 1)) + 1;// Start Row;
            queryOption.RowSetSize = 1000; // _gridSearchResult.GridContext.RowsPerPage;// Row Set Size;
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
                JQDataGrid _gridSearchResultTemp = _gridSearchResult;             
                SEMI.AppCode.GridUtility.ItemListGrid_BindDataTable(this, queryResult.GetAsExplicitlyDataTable(), ref _gridSearchResultTemp, "_JobTxn_DataDisplayGrid");

                //set updated value to Data Contracts
                if (_gridSearchResult.SelectedRowID != null && _gridSearchResult.GridContext.GetSelectedCell("JobOrder") != null)
                {
                    string selectedJobOrder = _gridSearchResult.GridContext.GetSelectedCell("JobOrder").ToString();
                    int selectedRowInt = Convert.ToInt32(_gridSearchResult.SelectedRowID);
                    if (selectedRowInt < queryResult.Rows.Count() && selectedJobOrder == queryResult.Rows[selectedRowInt].Values[2].ToString())
                    {
                        //Save the updated value to the data contracts
                        Page.PortalContext.DataContract.SetValueByName("Return_ddlJobStatusDM", queryResult.Rows[selectedRowInt].Values[8].ToString());
                        Page.PortalContext.DataContract.SetValueByName("ReturnTechStatusDM", queryResult.Rows[selectedRowInt].Values[11].ToString());
                        Page.PortalContext.DataContract.SetValueByName("ReturnIsSimpleModeStatusDM", queryResult.Rows[selectedRowInt].Values[4].ToString());
                    }
                    else
                    {
                        //Reset the data contracts and grid selected row
                        Page.PortalContext.DataContract.SetValueByName("Return_ddlJobStatusDM", null);
                        Page.PortalContext.DataContract.SetValueByName("ReturnTechStatusDM", null);
                        Page.PortalContext.DataContract.SetValueByName("ReturnIsSimpleModeStatusDM", null);
                        _gridSearchResult.GridContext.SelectedRowIDs = null;
                        _gridSearchResult.GridContext.SelectedRowIndex = null;
                        _gridSearchResult.GridContext.SelectedRowID = null;
                    }
                }

            }

            else
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("nullCol", typeof(string)); 
                for (int i = 0; i < 11; i++)
                   dt.Rows.Add(""); 
                _gridSearchResult.Data = dt;
                _gridSearchResult.OriginalData = dt;
            }
            
            if (!isSubmit)
            {
                _gridSearchResult.GridContext.SelectedRowIDs = null;
                _gridSearchResult.GridContext.SelectedRowIndex = null;
                _gridSearchResult.GridContext.SelectedRowID = null;

                ManagePanelButon();
            }

           
        }

        //
        // Custom Action Handling 
        // 
        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as CustomAction;
            if (action != null)
                if (action.Parameters == "AskRefresh")
                    AskForRefresh(); 
        }

        #endregion // Custom Procedure

    }
}




