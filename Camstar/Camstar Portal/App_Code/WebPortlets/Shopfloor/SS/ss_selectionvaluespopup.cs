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
using CGC = Camstar.WebPortal.FormsFramework.WebGridControls;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.Personalization;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    /// <summary>
    /// Summary description for SS_SelectionValuesPopup
    /// </summary>
    public class SS_SelectionValuesPopup: MatrixWebPart
    {
        private CWC.TextBox _txtKeyField { get { return FindCamstarControl("KeyField") as CWC.TextBox; } }        
        private CWC.TextBox _txtValueField { get { return FindCamstarControl("ValueField") as CWC.TextBox; } }
        private CWC.TextBox _txtRevisionField { get { return FindCamstarControl("RevisionField") as CWC.TextBox; } }
        private CWC.CheckBox _chkRORField { get { return FindCamstarControl("RORField") as CWC.CheckBox; } }
        private CWC.TextBox _txtNameFilterField { get { return FindCamstarControl("FilterField") as CWC.TextBox; } }
        private CWC.TextBox _txtHiddenNoResultsFound { get { return Page.FindCamstarControl("HiddenNoResultsFound") as CWC.TextBox; } }
        private CWC.TextBox _txtSelectionQuery { get { return Page.FindCamstarControl("SelectionQuery") as CWC.TextBox; } }

        private CWC.CheckBox _chkIsNameAvail { get { return FindCamstarControl("IsNameAvail") as CWC.CheckBox; } }
        private CWC.CheckBox _chkIsRevisionAvail { get { return FindCamstarControl("IsRevisionAvail") as CWC.CheckBox; } }
        private CWC.CheckBox _chkIsRORAvail { get { return FindCamstarControl("IsRORAvail") as CWC.CheckBox; } }
        private JQDataGrid _gridSelectionValuesField {get { return FindCamstarControl("SelectionValuesField") as JQDataGrid; }}       

        private CWC.TextBox _txtPageBlock { get { return FindCamstarControl("PageBlock") as CWC.TextBox; } }
        private CWC.Button _btnPrev { get { return FindCamstarControl("PagePrev") as CWC.Button; } }
        private CWC.Button _btnNext { get { return FindCamstarControl("PageNext") as CWC.Button; } }

        //---------------------------------------
        //
        //---------------------------------------
        protected override void OnLoad(EventArgs e)
        {                       
            
            base.OnLoad(e);

            Page.CollectDataContract();

            _txtSelectionQuery.Data = "_SelValEx_" + _txtKeyField.TextControl.Text;

            if (!Page.IsPostBack)
                LoadSelectionValues();
            
            (_gridSelectionValuesField.GridContext as CGC.QueryContext).BeforeQueryExecution += ResultsGrid_BeforeQueryExecution;
            (_gridSelectionValuesField.GridContext as CGC.QueryContext).AfterQueryExecution += ResultsGrid_AfterQueryExecution;
                        
        }

        //---------------------------------------
        //
        //---------------------------------------
        public void LoadSelectionValues()
        {
            var fs = FrameworkManagerUtil.GetFrameworkSession();
            QueryService objSvc = new QueryService(fs.CurrentUserProfile);

            // set the query options
            OM.QueryOptions objQueryOptions = new OM.QueryOptions();
            objQueryOptions.QueryType = OM.QueryType.User;  

            // set the query parameters
            int intTotalParams = 2;
            OM.QueryParameters objQueryParameters = new OM.QueryParameters();
            OM.QueryParameter[] objParameters = new OM.QueryParameter[intTotalParams];

            objParameters[0] = new OM.QueryParameter();
            objParameters[0].Name = _txtKeyField.TextControl.Text;

            objParameters[1] = new OM.QueryParameter();
            objParameters[1].Name = "NameFilter";
            string strFilterValue = "%";
            if (_txtNameFilterField.TextControl.Text == "")
            {
                strFilterValue = "%";
            }
            else
            {
                if (_txtNameFilterField.TextControl.Text.Contains('%'))
                    strFilterValue = _txtNameFilterField.TextControl.Text;
                else
                    strFilterValue = _txtNameFilterField.TextControl.Text + "%";
            }

            objParameters[0].Value = strFilterValue;
            objParameters[1].Value = strFilterValue;
            objQueryParameters.Parameters = objParameters;

            OM.RecordSet objRecordSet = new OM.RecordSet();
            OM.ResultStatus objRS = objSvc.Execute("_SelValEx_" + _txtKeyField.TextControl.Text, objQueryParameters, objQueryOptions, out objRecordSet);
            if (objRS.IsSuccess)
            {
                foreach (OM.Header objHeader in objRecordSet.Headers)
                {
                    switch (objHeader.Name.ToUpper())
                    {
                        case "NAME":
                            _chkIsNameAvail.CheckControl.Checked = true;
                            break;
                        case "REVISION":
                            _chkIsRevisionAvail.CheckControl.Checked = true;
                            break;
                        case "ISROR":
                            _chkIsRORAvail.CheckControl.Checked = true;
                            break;
                    }
                }

                _gridSelectionValuesField.ClearData();
                _gridSelectionValuesField.OriginalData = objRecordSet.GetAsExplicitlyDataTable();
                _gridSelectionValuesField.Data = objRecordSet.GetAsExplicitlyDataTable() ;
            
                CamstarWebControl.SetRenderToClient(_gridSelectionValuesField);               
            }
            else
            {
                DisplayMessage(objRS);
            }                       

        } // LoadSelectionValues   

        //-----------------------------------------
        // Set dummy data to indicate button click event
        //-----------------------------------------
        public void SetIsButtonClicked()
        {
            try
            {
                _txtHiddenNoResultsFound.Data = "Clicked";
            }
            catch (Exception ex) //Catch errors
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

        //-----------------------------------------
        // Fetch Results
        //-----------------------------------------
        bool ResultsGrid_BeforeQueryExecution(QueryState queryState)
        {
            try
            {
                // set the query parameters
                int intTotalParams = 2;
                OM.QueryParameters objQueryParameters = new OM.QueryParameters();
                OM.QueryParameter[] objParameters = new OM.QueryParameter[intTotalParams];

                objParameters[0] = new OM.QueryParameter();
                objParameters[0].Name = _txtKeyField.TextControl.Text;

                objParameters[1] = new OM.QueryParameter();
                objParameters[1].Name = "NameFilter";
                string strFilterValue = "%";
                if (_txtNameFilterField.TextControl.Text == "")
                {
                    strFilterValue = "%";
                }
                else
                {
                    if (_txtNameFilterField.TextControl.Text.Contains('%'))
                        strFilterValue = _txtNameFilterField.TextControl.Text;
                    else
                        strFilterValue = _txtNameFilterField.TextControl.Text + "%";
                }          

                objParameters[0].Value = strFilterValue;
                objParameters[1].Value = strFilterValue;     
                           
                queryState.Parameters = objParameters;
                queryState.Options.QueryType = WCF.ObjectStack.QueryType.User;                
                return true;
            }
            catch (Exception ex) //Catch errors
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
                return false;
            }
        }

        //-----------------------------------------
        // Return Error message when no result found
        //-----------------------------------------
        bool ResultsGrid_AfterQueryExecution(QueryState queryState)
        {
            try
            {
                if (queryState.returnedRecords != null)
                {
                    if (queryState.returnedRecords.Headers != null && queryState.returnedRecords.Rows == null)
                    {
                        if (_txtHiddenNoResultsFound.Data != null)
                        {
                            if (_txtHiddenNoResultsFound.Data.ToString().Equals("Clicked"))
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
                        }
                    }
                }
                _txtHiddenNoResultsFound.ClearData();
                return true;
            }
            catch (Exception ex) //Catch errors
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
                return false;
            }
        }
    
        //---------------------------------------
        //
        //---------------------------------------
        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as Personalization.CustomAction;
            if (action != null)
            {
                switch (action.Parameters)
                {
                    case "Close":
                        {                               
                            Page.CloseFloatingFrame(true);
                            break;
                        }
                    case "OK":
                        {
                            if (_gridSelectionValuesField.GridContext.SelectedRowID != null)
                            {
                                if (_chkIsNameAvail.CheckControl.Checked)
                                    _txtValueField.Data = (_gridSelectionValuesField.GridContext as DataGridContext).SelectedRowsTable.Rows[0].Field<String>("Name");//(_gridSelectionValuesField.BoundContext).GetSelectedCell("Name");

                                if (_chkIsRevisionAvail.CheckControl.Checked)
                                    _txtRevisionField.Data = (_gridSelectionValuesField.GridContext as DataGridContext).SelectedRowsTable.Rows[0].Field<String>("Revision"); //(_gridSelectionValuesField.BoundContext).GetSelectedCell("Revision");

                                if (_chkIsRORAvail.CheckControl.Checked)
                                {
                                    if ((_gridSelectionValuesField.GridContext as DataGridContext).SelectedRowsTable.Rows[0].Field<Int32>("IsROR") == 1)
                                    {
                                        _chkRORField.CheckControl.Checked = true;
                                    }
                                    else
                                    {
                                        _chkRORField.CheckControl.Checked = false;
                                    }
                                }
                            }
                            Page.CloseFloatingFrameOnSubmit(new ResultStatus());
                            break;
                        }
                }
            }
        } // WebPartCustomAction    

        //---------------------------------------
        //
        //---------------------------------------
        public void Grid_Refresh()
        {
            LoadSelectionValues();
        }
    }

}



