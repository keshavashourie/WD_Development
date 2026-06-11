/* Copyright 2020 Siemens */
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


/// <summary>
/// Summary description for SS_OnlineQuery
/// </summary>
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_OnlineQuery : MatrixWebPart
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            string sEvent = Page.EventArgument.ToString();
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        public void ExecuteGrid()
        {
            JQDataGrid _gridQueryParamsGrid = FindCamstarControl("OnlineQuery_Parameters") as JQDataGrid;
            JQDataGrid _gridResultsDataGrid = FindCamstarControl("OnlineQuery_ResultData") as JQDataGrid;
            CWC.NamedObject _ndoQueryList = FindCamstarControl("OnlineQuery_Query") as CWC.NamedObject;
            CWC.TextBox _txtResultsetSizeLimit = FindControl("OnlineQuery_ResultsetSizeLimit") as CWC.TextBox;

            // get the session and user profile
            var fs = FrameworkManagerUtil.GetFrameworkSession();

            // get the total number of params in the grid
            int intTotalParams = _gridQueryParamsGrid.GridContext.GetTotalRows();
            int intRowSize = 0;
            if (_txtResultsetSizeLimit.Data != null)
                if (_txtResultsetSizeLimit.Data.ToString() != "")
                    intRowSize = int.Parse(_txtResultsetSizeLimit.Data.ToString());

            // init the service, service data and service info objects
            OnlineQueryService objSvc = new OnlineQueryService(fs.CurrentUserProfile);
            OnlineQuery_Info objSvcInfo = new OnlineQuery_Info();
            OnlineQuery objSvcData = new OnlineQuery();
            // init the parameters object
            OnlineQuerySetupParamsChanges[] objParameters = new OnlineQuerySetupParamsChanges[intTotalParams];

            // loop thru the available parameters in the Params grid and get the name/value to set in the parameters object.
            for (int x = 0; x < intTotalParams; x++)
            {
                string strRowId = x.ToString().PadLeft(6, '0');
                _gridQueryParamsGrid.GridContext.SelectRow(strRowId, true);

                objParameters[x] = new OnlineQuerySetupParamsChanges();
                if (_gridQueryParamsGrid.GridContext.GetCell(strRowId, "Name") != null)
                    objParameters[x].Name = _gridQueryParamsGrid.GridContext.GetCell(strRowId, "Name").ToString();
				if (_gridQueryParamsGrid.GridContext.GetCell(strRowId, "scsDisplayText") != null)
                    objParameters[x].scsDisplayText = _gridQueryParamsGrid.GridContext.GetCell(strRowId, "scsDisplayText").ToString();
                if (_gridQueryParamsGrid.GridContext.GetCell(strRowId, "scsDefaultValue") != null)
                    objParameters[x].DefaultValue = _gridQueryParamsGrid.GridContext.GetCell(strRowId, "scsDefaultValue").ToString();
                else
                    objParameters[x].DefaultValue = "";
            }

            // set the input data
            objSvcData.OnlineQuerySetup = _ndoQueryList.Data as NamedObjectRef;
            objSvcData.Parameters = objParameters;

            // set the request 
            objSvcInfo.OnlineQuerySelection = FieldInfoUtil.RequestSelectionValue();

            // set the selection value options
            objSvcInfo.RequestSelectionValuesInfo = new SelectionValuesInfo();
            objSvcInfo.RequestSelectionValuesInfo.Options = new QueryOptions();
            objSvcInfo.RequestSelectionValuesInfo.Options.StartRow = 1;
            objSvcInfo.RequestSelectionValuesInfo.Options.RowSetSize = intRowSize;
            objSvcInfo.RequestSelectionValuesInfo.Options.QueryType = OM.QueryType.User;

            // init the request object
            OnlineQuery_Request objRequest = new OnlineQuery_Request();
            objRequest.Info = objSvcInfo;

            // init the result object
            OnlineQuery_Result objResult = new OnlineQuery_Result();

            // execute the request selection values
            ResultStatus objRS = objSvc.GetEnvironment(objSvcData, objRequest, out objResult);

            if (objRS.IsSuccess)
            {
                // clear the grid data
                Page.DisplayMessage("", true);
                _gridResultsDataGrid.ClearData();
                JQFieldCollection oFieldClear = new JQFieldCollection();
                (_gridResultsDataGrid.GridContext as GenericGridContext).Fields = oFieldClear;

                if (objResult.Environment.OnlineQuerySelection.SelectionValues.Rows != null)
                {
                    DataTable dtResults = new DataTable();
                    RecordSet rsResults = objResult.Environment.OnlineQuerySelection.SelectionValues;
                    List<string> sHeaders = new List<string>();

                    foreach (Header oHeader in rsResults.Headers)
                    {
                        dtResults.Columns.Add(oHeader.Name);
                        sHeaders.Add(oHeader.Name);

                        DataColumn dc = new DataColumn(oHeader.Name);
                        _gridResultsDataGrid.AddField(dc);
                    }

                    JQFieldCollection oFieldDef = (_gridResultsDataGrid.GridContext as GenericGridContext).Fields;
                    foreach (JQField oField in oFieldDef)
                        oField.Resizable = true;

                    foreach (Row oRow in rsResults.Rows)
                    {
                        DataRow dtRow = dtResults.NewRow();
                        string[] sValues = oRow.Values;
                        for (int x = 0; x < sValues.Length; x++)
                            dtRow[sHeaders[x]] = sValues[x];

                        dtResults.Rows.Add(dtRow);
                    }

                    (_gridResultsDataGrid.GridContext as GenericGridContext).Data = dtResults;
                    _gridResultsDataGrid.DataBind();
                }
                _gridResultsDataGrid.Hidden = false;
                //this.LoadPersonalization();
                CamstarWebControl.SetRenderToClient(_gridResultsDataGrid);
            }
            else
            {
                _gridResultsDataGrid.ClearData();
                DisplayMessage(objRS);
            }


        } // ExecuteGrid

    }
}



