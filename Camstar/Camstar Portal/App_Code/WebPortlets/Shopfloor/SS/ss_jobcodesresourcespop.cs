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
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.Personalization;
using CWF = Camstar.WebPortal.FormsFramework;
using WC = CamstarPortal.WebControls;


/// <summary>
/// Summary description
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_JobCodesResourcesPop : MatrixWebPart
    {
        #region Property

        private CWC.NamedObject resourceCtr { get { return Page.FindCamstarControl("Code_Resource") as CWC.NamedObject; } }
        private CWC.NamedObject symptomCtr { get { return Page.FindCamstarControl("Code_SymptomCode") as CWC.NamedObject; } }
        private CWC.NamedObject causeCtr { get { return Page.FindCamstarControl("Code_CauseCode") as CWC.NamedObject; } }
        private CWC.NamedObject repairCtr { get { return Page.FindCamstarControl("Code_RepairCode") as CWC.NamedObject; } }
        private CWC.TextBox hidServiceCtr { get { return Page.FindCamstarControl("hdnCodeServices") as CWC.TextBox; } }
        private CWC.ChartControl _chartJobCodes { get { return Page.FindCamstarControl("JobChartField") as CWC.ChartControl; } }
        private enum CodeServices { Symptom = 1, Cause = 2, Repair = 3, None = 4 };

        #endregion //Property

        #region Page Event        

        //-----------------------------------------
        //
        //-----------------------------------------
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            Page.LoadComplete += Page_LoadComplete;
            string _serviceType = "JobProgress";
            if (Page.PortalContext.DataContract.GetValueByName("DCMWP_ServiceType") != null)
                _serviceType = Page.PortalContext.DataContract.GetValueByName("DCMWP_ServiceType").ToString();

            PrimaryServiceType = _serviceType;
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                string _resource = "";
                string _symptom = "";
                string _cause = "";
                string _repair = "";

                if (Page.PortalContext.DataContract.GetValueByName("DCM_Grid_Resource") != null)
                {
                    _resource = Page.PortalContext.DataContract.GetValueByName("DCM_Grid_Resource").ToString();
                    if (Page.PortalContext.DataContract.GetValueByName("DCMWP_CodeSymptom") != null)
                        _symptom = Page.PortalContext.DataContract.GetValueByName("DCMWP_CodeSymptom").ToString();
                    if (Page.PortalContext.DataContract.GetValueByName("DCMWP_CodeCause") != null)
                        _cause = Page.PortalContext.DataContract.GetValueByName("DCMWP_CodeCause").ToString();
                    if (Page.PortalContext.DataContract.GetValueByName("DCMWP_CodeRepair") != null)
                        _repair = Page.PortalContext.DataContract.GetValueByName("DCMWP_CodeRepair").ToString();
                }
                else
                {
                    if (Page.PortalContext.DataContract.GetValueByName("DCVP_CodeResource") != null)
                        _resource = Page.PortalContext.DataContract.GetValueByName("DCVP_CodeResource").ToString();
                    if (Page.PortalContext.DataContract.GetValueByName("DCVP_CodeSymptom") != null)
                        _symptom = Page.PortalContext.DataContract.GetValueByName("DCVP_CodeSymptom").ToString();
                    if (Page.PortalContext.DataContract.GetValueByName("DCVP_CodeCause") != null)
                        _cause = Page.PortalContext.DataContract.GetValueByName("DCVP_CodeCause").ToString();
                    if (Page.PortalContext.DataContract.GetValueByName("DCVP_CodeRepair") != null)
                        _repair = Page.PortalContext.DataContract.GetValueByName("DCVP_CodeRepair").ToString();
                }

                ManageControl(_resource, _symptom, _cause, _repair);
            }

            base.OnLoad(e);
            
            GenerateChart();
            this.Page.RenderToClient = true;
            Page.Service.LoadData();
        }

        void repairCtr_DataChanged(object sender, EventArgs e)
        {
            GenerateChart();
            this.Page.RenderToClient = true;
            Page.Service.LoadData();
        }

        void causeCtr_DataChanged(object sender, EventArgs e)
        {
            GenerateChart();
            this.Page.RenderToClient = true;
            Page.Service.LoadData();
        }

        void symptomCtr_DataChanged(object sender, EventArgs e)
        {
            GenerateChart();
            this.Page.RenderToClient = true;
            Page.Service.LoadData();
        }

        void resourceCtr_DataChanged(object sender, EventArgs e)
        {
            GenerateChart();
            this.Page.RenderToClient = true;
            Page.Service.LoadData();
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        void Page_LoadComplete(object sender, EventArgs e)
        {
            symptomCtr.SelectionDependencies.Add(new CWF.DependsOnItem(resourceCtr.ID));
            causeCtr.SelectionDependencies.Add(new CWF.DependsOnItem(resourceCtr.ID));
            causeCtr.SelectionDependencies.Add(new CWF.DependsOnItem(symptomCtr.ID));
            repairCtr.SelectionDependencies.Add(new CWF.DependsOnItem(resourceCtr.ID));
            repairCtr.SelectionDependencies.Add(new CWF.DependsOnItem(symptomCtr.ID));
            repairCtr.SelectionDependencies.Add(new CWF.DependsOnItem(causeCtr.ID));
            //  wei sun: edit to allow popup to open without data
        }
        
        #endregion //Page Event

        #region Chart

        public void GenerateChart()
        {
            String Sql = GetSQL();
            _chartJobCodes.ClearData();
            _chartJobCodes.SelectionDataSource = new SelectionDataSourceMap();
            _chartJobCodes.SelectionDataSource.SourceType = SelectionDataSourceType.Query;
            _chartJobCodes.SelectionDataSource.QueryExecutionMode = QueryExecutionModeType.AdHocQuery;
            _chartJobCodes.SelectionDataSource.Query = Sql;
            CamstarWebControl.SetRenderToClient(_chartJobCodes);
        }

        public string GetSQL()
        {
            string Sql = "";
            // wei sun: edit to allow popup to open without data
            if (hidServiceCtr.Text != null)
            {
                int intResult;
                if (int.TryParse(hidServiceCtr.Text.ToString(), out intResult))
                {
                    switch ((CodeServices)Convert.ToInt16(hidServiceCtr.Text.ToString()))
                    {
                        case CodeServices.Symptom:
                            Sql = "SELECT " +
                               " SC.JobSymptomCodeName NAME, SUM(JC.NumberOfOccurrences) NAMECOUNT " +
                            " FROM  ResourceDef R  " +
                               " INNER JOIN A_JobCodesByResource JC ON R.ResourceId = JC.ResourceId  " +
                               " INNER JOIN A_JobSymptomCode SC ON JC.SymptomCodeId = SC.JobSymptomCodeId " +
                            " WHERE " +
                               "R.ResourceName LIKE '" + resourceCtr.TextEditControl.Text + "' ";

                            if (symptomCtr.TextEditControl.Text == "")
                                Sql += "AND SC.JobSymptomCodeName LIKE '%' ";
                            else
                                Sql += "AND SC.JobSymptomCodeName = '" + symptomCtr.TextEditControl.Text + "' ";
                            Sql += "GROUP BY " +
                                   "SC.JobSymptomCodeName " +
                              "ORDER BY " +
                                   "NAMECOUNT DESC, NAME ASC ";
                            break;
                        case CodeServices.Cause:
                            Sql = "SELECT " +
                                    "CC.JobCauseCodeName NAME" +
                                    ", SUM(JC.NumberOfOccurrences) NAMECOUNT " +
                               "FROM " +
                                    "ResourceDef R " +
                                    "INNER JOIN A_JobCodesByResource JC ON R.ResourceId = JC.ResourceId " +
                                    "LEFT OUTER JOIN A_JobSymptomCode SC ON JC.SymptomCodeId = SC.JobSymptomCodeId " +
                                    "INNER JOIN A_JobCauseCode CC ON JC.CauseCodeId = CC.JobCauseCodeId " +
                               "WHERE " +
                                    "R.ResourceName LIKE '" + resourceCtr.TextEditControl.Text + "' " +
                                    "AND CC.JobCauseCodeName LIKE '" + causeCtr.TextEditControl.Text + "%' " +
                                    "AND (SC.JobSymptomCodeName IS NULL OR SC.JobSymptomCodeName LIKE '" + symptomCtr.TextEditControl.Text + "%')" +
                               "GROUP BY " +
                                    "CC.JobCauseCodeName " +
                               "ORDER BY " +
                                    "NAMECOUNT DESC, NAME ASC ";
                            break;

                        case CodeServices.Repair:
                            Sql = "SELECT " +
                                    "RC.JobRepairCodeName NAME" +
                                    ", SUM(JC.NumberOfOccurrences) NAMECOUNT " +
                               "FROM " +
                                    "ResourceDef R " +
                                    "INNER JOIN A_JobCodesByResource JC ON R.ResourceId = JC.ResourceId " +
                                    "LEFT OUTER JOIN A_JobSymptomCode SC ON JC.SymptomCodeId = SC.JobSymptomCodeId " +
                                    "LEFT OUTER JOIN A_JobCauseCode CC ON JC.CauseCodeId = CC.JobCauseCodeId " +
                                    "INNER JOIN A_JobRepairCode RC ON JC.RepairCodeId = RC.JobRepairCodeId " +
                               "WHERE " +
                                    "R.ResourceName LIKE '" + resourceCtr.TextEditControl.Text + "' " +
                                    "AND RC.JobRepairCodeName LIKE '" + repairCtr.TextEditControl.Text + "%' " +
                                    "AND (SC.JobSymptomCodeName IS NULL OR SC.JobSymptomCodeName LIKE '" + symptomCtr.TextEditControl.Text + "%')" +
                                    "AND (CC.JobCauseCodeName IS NULL OR CC.JobCauseCodeName LIKE '" + causeCtr.TextEditControl.Text + "%')" +
                               "GROUP BY " +
                                    "RC.JobRepairCodeName " +
                               "ORDER BY " +
                                    "NAMECOUNT DESC, NAME ASC ";
                            break;
                        case CodeServices.None:
                            Sql = "SELECT " +
                                    "RC.JobRepairCodeName NAME" +
                                    ", SUM(JC.NumberOfOccurrences) NAMECOUNT " +
                               "FROM " +
                                    "ResourceDef R " +
                                    "INNER JOIN A_JobCodesByResource JC ON R.ResourceId = JC.ResourceId " +
                                    "LEFT OUTER JOIN A_JobSymptomCode SC ON JC.SymptomCodeId = SC.JobSymptomCodeId " +
                                    "LEFT OUTER JOIN A_JobCauseCode CC ON JC.CauseCodeId = CC.JobCauseCodeId " +
                                    "INNER JOIN A_JobRepairCode RC ON JC.RepairCodeId = RC.JobRepairCodeId " +
                               "WHERE " +
                                    "R.ResourceName LIKE '" + resourceCtr.TextEditControl.Text + "' " +
                                    "AND RC.JobRepairCodeName LIKE '" + repairCtr.TextEditControl.Text + "%' " +
                                    "AND (SC.JobSymptomCodeName IS NULL OR SC.JobSymptomCodeName LIKE '" + symptomCtr.TextEditControl.Text + "%')" +
                                    "AND (CC.JobCauseCodeName IS NULL OR CC.JobCauseCodeName LIKE '" + causeCtr.TextEditControl.Text + "%')" +
                               "GROUP BY " +
                                    "RC.JobRepairCodeName " +
                               "ORDER BY " +
                                    "NAMECOUNT DESC, NAME ASC ";
                            break;
                    }
                }

            }
            return Sql;
        }

        #endregion //Chart

        //-----------------------------------------
        //
        //-----------------------------------------
        public void ManageControl(string paramResource, string paramSymptom, string paramCause, string paramRepair)
        {
            resourceCtr.Visible = false;
            symptomCtr.Visible = false;
            causeCtr.Visible = false;
            repairCtr.Visible = false;

            if (paramResource != "" && paramSymptom != "" && paramCause == "" && paramRepair == "")
            {
                resourceCtr.Text = paramResource;
                resourceCtr.Visible = true;
                resourceCtr.ReadOnly = true;
                symptomCtr.Visible = true;
                hidServiceCtr.Text = ((int)CodeServices.Symptom).ToString();

            }
            else if (paramResource != "" && paramSymptom != "" && paramCause != "" && paramRepair == "")
            {
                resourceCtr.Text = paramResource;
                resourceCtr.Visible = true;
                resourceCtr.ReadOnly = true;
                symptomCtr.Text = paramSymptom;
                symptomCtr.Visible = true;
                symptomCtr.ReadOnly = false;
                causeCtr.Visible = true;
                hidServiceCtr.Text = ((int)CodeServices.Cause).ToString();

            }
            else if (paramResource != "" && paramSymptom != "" && paramCause != "" && paramRepair != "")
            {
                resourceCtr.Text = paramResource;
                resourceCtr.Visible = true;
                resourceCtr.ReadOnly = true;
                symptomCtr.Text = paramSymptom;
                symptomCtr.Visible = true;
                symptomCtr.ReadOnly = false;
                causeCtr.Text = paramCause;
                causeCtr.Visible = true;
                causeCtr.ReadOnly = false;
                repairCtr.Visible = true;
                hidServiceCtr.Text = ((int)CodeServices.Repair).ToString();

            }
            else
            {
                resourceCtr.Visible = true;
                symptomCtr.Visible = true;
                causeCtr.Visible = true;
                repairCtr.Visible = true;
                hidServiceCtr.Text = ((int)CodeServices.None).ToString();

            }

        }

        //-----------------------------------------
        //
        //-----------------------------------------
        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as CustomAction;
            if (action != null)
                if (action.Parameters == "ClosePop")
                {
                    ScriptManager.RegisterStartupScript(Page.Form, GetType(), "OKButton", "window.parent.__page._isTabContainerPage=null;window.parent.CloseFloatingFrame(true);", true);
                }
        }
    }
}




