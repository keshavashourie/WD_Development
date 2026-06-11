/* Copyright 2019 Siemens */
using System;
using System.Collections;
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
using SEMI.AppCode;

/// <summary>
/// Summary description for SS_TrackLabelReprint
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_TrackLabelReprint : MatrixWebPart
    {
        protected CWC.TextBox _txtLotID { get { return Page.FindCamstarControl("ss_TrackLabelReprint_LotID") as CWC.TextBox; } }
        protected CWC.TextBox _txtLabelID { get { return Page.FindCamstarControl("ss_TrackLabelReprint_LabelID") as CWC.TextBox; } }
        protected CWC.TextBox _txtComputerName { get { return Page.FindCamstarControl("ss_TrackLabelReprint_ComputerName") as CWC.TextBox; } }

        protected CWC.NamedObject _ndoEmployee { get { return Page.FindCamstarControl("ss_TrackLabelReprint_Employee") as CWC.NamedObject; } }

        protected CWC.CheckBox _chkESigRequired { get { return Page.FindCamstarControl("ss_TrackLabelReprint_ESigRequired") as CWC.CheckBox; } }

        protected JQDataGrid _gridLabels { get { return Page.FindCamstarControl("ss_TrackLabelReprint_Labels") as JQDataGrid; } }

        //---------------------------------------------------
        // On Load Event
        //---------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            _txtLotID.DataChanged += new EventHandler(_txtLotID_DataChanged);
            _txtLabelID.DataChanged += new EventHandler(_txtLabelID_DataChanged);
            _gridLabels.RowSelected += new JQGridEventHandler(LabelsGrid_RowSelected);
            _txtComputerName.TextControl.Text = SEMI.AppCode.UIUtility.GetComputerName(this);
        }

        //---------------------------------------------------
        // Lot ID Data Changed
        //---------------------------------------------------
        void _txtLotID_DataChanged(object sender, EventArgs e)
        {
            if (_txtLotID.Data != null)
                FetchQueryResult(1, _txtLotID.Data.ToString());
            else
                _gridLabels.ClearData();
        } // _txtSelectionId_DataChanged


        //---------------------------------------------------
        // Label ID Data Changed
        //---------------------------------------------------
        void _txtLabelID_DataChanged(object sender, EventArgs e)
        {
            if (_txtLabelID.Data != null)
                FetchQueryResult(2, _txtLabelID.Data.ToString());
            else
                _gridLabels.ClearData();
        } // _txtSelectionId_DataChanged

        //---------------------------------------------------
        // Labels Grid Row Selected Event
        //---------------------------------------------------
        protected virtual ResponseData LabelsGrid_RowSelected(object sender, JQGridEventArgs args)
        {
            _chkESigRequired.Data = false;
            _chkESigRequired.CheckControl.Checked = false;
            if (_gridLabels.SelectedRowIDs != null)
            {
                if (_gridLabels.SelectedRowIDs.Length > 0)
                {
                    int SelectedRowCount = _gridLabels.SelectedRowIDs.Length;
                    for (int i = 0; i < SelectedRowCount; i++)
                    {
                        if ((_gridLabels.GridContext as DataGridContext).SelectedRowsTable.Rows[i].Field<Boolean>("ESigRequiredForReprint"))
                        {
                            _chkESigRequired.Data = true;
                            _chkESigRequired.CheckControl.Checked = true;
                        }
                    }
                    return new StatusData(true, "Row selected");
                }
                else
                {
                    return new StatusData(false, "Row not selected");
                }
            }
            else
            {
                return new StatusData(false, "Row not selected");
            }
        }

        //--------------------------------------
        // Retrive Available Labels
        //--------------------------------------
        private RecordSet RetrieveAvailableLabels(int sQueryType, string sParamValue)
        {
            // get the session and user profile
            var fs = FrameworkManagerUtil.GetFrameworkSession();
            QueryService oService = new QueryService(fs.CurrentUserProfile);
            QueryOptions oOptions = new QueryOptions();
            RecordSet oData = new RecordSet();

            // prepare the query and query parameters
            string oQueryName = "";
            QueryParameters oParameters = new QueryParameters();
            QueryParameter[] oParameterList = new QueryParameter[1];
            oParameterList[0] = new QueryParameter();
            if (sQueryType == 1)
            {
                oParameterList[0].Name = "ContainerName";
                oParameterList[0].Value = sParamValue;
                oQueryName = "ss_TrackLabel_GetLabelsByLot";
            }
            else if (sQueryType == 2)
            {
                oParameterList[0].Name = "LabelID";
                oParameterList[0].Value = sParamValue;
                oQueryName = "ss_TrackLabel_GetLabelsByLabelID";
            }

            oParameters.Parameters = oParameterList;

            ResultStatus oResult = oService.Execute(oQueryName, oParameters, oOptions, out oData);
            if (oResult.IsSuccess)
            {
                if (oData != null)
                {
                    if (oData.Rows != null)
                    {
                        if (oData.Rows.Length > 0)
                        {
                            if (oData.Headers[3].TypeCode != TypeCode.Boolean)
                            {
                                oData.Headers[3].TypeCode = TypeCode.Boolean;
                                for (int i = 0; i < oData.Rows.Length; i++)
                                {
                                    if (oData.Rows[i].Values[3].ToString() == "1")
                                        oData.Rows[i].Values[3] = "true";
                                    else
                                        oData.Rows[i].Values[3] = "false";
                                }
                            }
                        }
                    }
                }

                return oData;
            }

            return null;
        } // RetrieveRuntimeLayout

        //---------------------------------------------------
        // Fetch Query Result
        //---------------------------------------------------
        private void FetchQueryResult(int sQueryType, string sParamValue)
        {
            RecordSet oAvailableLabelsRecordSet = RetrieveAvailableLabels(sQueryType, sParamValue);
            _gridLabels.ClearData();
            JQDataGrid _tempLabelsGrid = _gridLabels;
            GridUtility.SelectionValuesGrid_AddDataRow(ref _tempLabelsGrid, oAvailableLabelsRecordSet);

            if (oAvailableLabelsRecordSet.TotalCount < 1)
            {
                string labelText = "";
                var labelCache = FrameworkManagerUtil.GetLabelCache(System.Web.HttpContext.Current.Session);
                if (labelCache != null)
                {
                    var label = labelCache.GetLabelByName("LotSearch_NoResultsFound");
                    if (label != null)
                        labelText = label.Value;
                }
                Page.StatusBar.WriteError(labelText);
            }
            else
            {
                Page.StatusBar.ClearMessage();
            }
        }

        //---------------------------------------------------
        // GetInputData
        //---------------------------------------------------
        public override void GetInputData(Service serviceData)
        {
            base.GetInputData(serviceData);
            int gridCount = _gridLabels.GridContext.GetTotalRows();
            if (gridCount > 0)
            {
                if (_gridLabels.SelectedRowIDs != null)
                {
                    int SelectedRowCount = _gridLabels.SelectedRowIDs.Length;
                    (serviceData as ss_TrackLabelReprint).LabelIDToReprint = new Primitive<string>[SelectedRowCount];
                    for (int i = 0; i < SelectedRowCount; i++)
                    {
                        (serviceData as ss_TrackLabelReprint).LabelIDToReprint[i] = new Primitive<string>();
                        (serviceData as ss_TrackLabelReprint).LabelIDToReprint[i].Value = (_gridLabels.GridContext as DataGridContext).SelectedRowsTable.Rows[i].Field<String>("LabelID").ToString();
                        if ((_gridLabels.GridContext as DataGridContext).SelectedRowsTable.Rows[i].Field<Boolean>("ESigRequiredForReprint"))
                        {
                            (serviceData as ss_TrackLabelReprint).ESigRequired = true;
                        }
                    }
                }
            }
        }

        //---------------------------------------------------
        // Web Part Custom Action
        //---------------------------------------------------
        public override void WebPartCustomAction(object sender, CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as Camstar.WebPortal.Personalization.CustomAction;

            if (action != null && action.Parameters == "Reset")
            {
                Page.ShopfloorReset(sender, e);
            }
        } // WebPartCustomAction
    }
}



