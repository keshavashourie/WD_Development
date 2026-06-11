/* Copyright 2019 Siemens */
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Camstar.WCF.Services;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.Personalization;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using OM = Camstar.WCF.ObjectStack;

/// <summary>
/// Summary description for ss_ToolManagement
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_ToolManagement : MatrixWebPart
    {
        //Controls declaration
        private CWC.NamedObject _ddlTool { get { return (Page.FindCamstarControl("ss_ToolManagement_ss_Tool") as CWC.NamedObject); } }
        private CWC.NamedObject _ddlToolFamily { get { return (Page.FindCamstarControl("ss_ToolManagement_ss_ToolFamily") as CWC.NamedObject); } }
        private CWC.NamedObject _ddlToolStatus { get { return Page.FindCamstarControl("ss_ToolManagement_ss_ToolStatus") as CWC.NamedObject; } }
        private CWC.NamedObject _ddlReserveEquipment { get { return Page.FindCamstarControl("ss_ToolManagement_ss_ReserveEquipment") as CWC.NamedObject; } }
        private CWC.NamedObject _ddlAvailableToolAction { get { return Page.FindCamstarControl("ss_ToolManagement_ss_AvailableToolAction") as CWC.NamedObject; } }
        private CWC.Button _btnSearch { get { return Page.FindCamstarControl("SearchButton") as CWC.Button; } }
        private CWC.Button _btnClear { get { return Page.FindCamstarControl("SearchClear") as CWC.Button; } }
        private JQDataGrid _gridToolField { get { return Page.FindCamstarControl("ToolRecords_Results") as JQDataGrid; } }
        protected CWC.TextBox _txtQueryStartRowNum { get { return Page.FindCamstarControl("StartRowNum") as CWC.TextBox; } }
        protected CWC.TextBox _txtQueryStopRowNum { get { return Page.FindCamstarControl("StopRowNum") as CWC.TextBox; } }
        private CWC.Button _btnToolAction { get { return Page.FindCamstarControl("ToolActionButton") as CWC.Button; } }


        protected MatrixWebPart ToolResultsWP
        {
            get
            {
                MatrixWebPart wp = null;
                int iParentControlsCount = Parent.Controls.Count;
                for (int z = 0; z < iParentControlsCount; z++)
                {
                    if (Parent.Controls[z].ID == "ToolResultsWP")
                        wp = Parent.Controls[z] as MatrixWebPart;
                }
                return wp;
               
            }
        }

        //---------------------------------------------------
        // On Load Event
        //---------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            _ddlAvailableToolAction.DataChanged += new EventHandler(_ddlAvailableToolAction_DataChanged);
            if (Page.IsPostBack && Page.EventTarget == "" && Page.EventArgument == "FloatingFrameSubmitParentPostBackArgument")
            {
                RetrieveToolRecords();
                _btnToolAction.Visible = false;
                _btnToolAction.LabelText = "";
                _btnToolAction.ToolTip = "";
                _ddlAvailableToolAction.Data = null;
				_ddlAvailableToolAction.ClearSelectionValues();
            }

        }

        void _ddlAvailableToolAction_DataChanged(object sender, EventArgs e)
        {
            _btnToolAction.Visible = false;
            _btnToolAction.LabelText = "";
            _btnToolAction.ToolTip = "";
            if (_ddlAvailableToolAction.DropDownControl.SelectedValue != "")
            {
                _btnToolAction.Visible = true;
                _btnToolAction.LabelText = _ddlAvailableToolAction.DropDownControl.SelectedValue;
                _btnToolAction.ToolTip = _ddlAvailableToolAction.DropDownControl.SelectedValue;
            }

        } // _ddlAvailableToolAction_DataChanged

        public void ClearSearchFields()
        {
            _ddlTool.ClearData();
            _ddlToolFamily.ClearData();
            _ddlToolStatus.ClearData();
            _ddlReserveEquipment.ClearData();
            _gridToolField.ClearData();
            _btnToolAction.Visible = false;
            _ddlAvailableToolAction.DropDownControl.Items.Clear();
        } // ClearSearchFields


        public void RetrieveToolRecords()
        {
            FrameworkSession currentSession = FrameworkManagerUtil.GetFrameworkSession(this.Page.Session);
            QueryUtil QueryService = new QueryUtil(currentSession.CurrentUserProfile);
            RecordSet queryResult = null;
            ResultStatus resultStatus = null;
            QueryOptions queryOption = new QueryOptions();
            QueryParameter[] queryParams = new QueryParameter[6];

            queryOption.QueryType = Camstar.WCF.ObjectStack.QueryType.System; /// querytype = system
            queryOption.StartRow = 1;// Start Row;
            queryOption.RowSetSize = 10000;// Row Set Size;

            queryParams[0] = new QueryParameter();
            queryParams[0].Name = "Tool";
            queryParams[0].Value = _ddlTool.Data != null ? (_ddlTool.Data.ToString() != "" ? _ddlTool.Data.ToString() : "%") : "%";

            queryParams[1] = new QueryParameter();
            queryParams[1].Name = "ToolFamily";
            queryParams[1].Value = _ddlToolFamily.Data != null ? (_ddlToolFamily.Data.ToString() != "" ? _ddlToolFamily.Data.ToString() : "%") : "%";

            queryParams[2] = new QueryParameter();
            queryParams[2].Name = "ToolStatus";
            queryParams[2].Value = _ddlToolStatus.Data != null ? (_ddlToolStatus.Data.ToString() != "" ? _ddlToolStatus.Data.ToString() : "%") : "%";

            queryParams[3] = new QueryParameter();
            queryParams[3].Name = "ReservedEquipment";
            queryParams[3].Value = _ddlReserveEquipment.Data != null ? (_ddlReserveEquipment.Data.ToString() != "" ? _ddlReserveEquipment.Data.ToString() : "%") : "%";

            queryParams[4] = new QueryParameter();
            queryParams[4].Name = "STARTROWNUM";
            queryParams[4].Value = _txtQueryStartRowNum.Data.ToString();

            queryParams[5] = new QueryParameter();
            queryParams[5].Name = "STOPROWNUM";
            queryParams[5].Value = _txtQueryStopRowNum.Data.ToString();

            if (queryParams[3].Value != "%")
            {
                QueryService.Execute("ss_ToolRecords_ReservedEquip", queryParams, queryOption, ref queryResult, ref resultStatus);
            }
            else
            {
                QueryService.Execute("ss_ToolRecords", queryParams, queryOption, ref queryResult, ref resultStatus);
            }


            if (resultStatus.IsSuccess)
            {
                DataTable dtResult = new DataTable();
                dtResult = queryResult.GetAsExplicitlyDataTable();
                _gridToolField.ClearData();
                JQDataGrid gridToolField = _gridToolField;

                string[] sHiddenColumns = new string[] { "ToolAction" };
                SEMI.AppCode.GridUtility.ItemListGrid_SetColumns(ToolResultsWP, dtResult, _gridToolField.ID, null, "RefTargetGrid", true, sHiddenColumns);
                 SEMI.AppCode.GridUtility.ItemListGrid_BindDataTable(ToolResultsWP, dtResult, ref gridToolField, "RefTargetGrid");
				 for (int i = 0; i <= _gridToolField.Settings.Columns.Count(); i++)
				 {
					 if ((_gridToolField.GridContext as BoundContext).Fields[i].ID == "Tool")
						 (_gridToolField.GridContext as BoundContext).Fields[i].LabelName = "Tools_Tool";
					 else if ((_gridToolField.GridContext as BoundContext).Fields[i].ID == "ToolFamily")
						 (_gridToolField.GridContext as BoundContext).Fields[i].LabelName = "ToolPlanItemDetails_ToolFamily";
					 else if ((_gridToolField.GridContext as BoundContext).Fields[i].ID == "ToolStatus")
						 (_gridToolField.GridContext as BoundContext).Fields[i].LabelName = "ss_ToolManagement_ss_ToolStatus";
					 else if ((_gridToolField.GridContext as BoundContext).Fields[i].ID == "ReserveEquipment")
						 (_gridToolField.GridContext as BoundContext).Fields[i].LabelName = "WIPMain_ReservedEquipment";
					 else if ((_gridToolField.GridContext as BoundContext).Fields[i].ID == "ReserveEmployee")
						 (_gridToolField.GridContext as BoundContext).Fields[i].LabelName = "ss_ToolUpdateStatus_ss_ReserveEmp";
					 else if ((_gridToolField.GridContext as BoundContext).Fields[i].ID == "MountedEquipment")
						 (_gridToolField.GridContext as BoundContext).Fields[i].LabelName = "ss_WebUI_MountedEquip";
					 else if ((_gridToolField.GridContext as BoundContext).Fields[i].ID == "PhysicalLocation")
						 (_gridToolField.GridContext as BoundContext).Fields[i].LabelName = "CSICDOName_PhysicalLocation";
				 }

                CamstarWebControl.SetRenderToClient(_gridToolField);
            }
            else
            {
                DisplayMessage(resultStatus);
            }
        } // RetrieveToolRecords




    }
}



