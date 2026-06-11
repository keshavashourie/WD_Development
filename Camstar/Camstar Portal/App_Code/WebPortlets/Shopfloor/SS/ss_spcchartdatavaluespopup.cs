/* Copyright 2025 Siemens */
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
/// Summary description for SS_SPChartDataValuesPopup
/// </summary>
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_SPCChartDataValuesPopup : MatrixWebPart
    {
        protected CWC.DropDownList _ddlEntryType { get { return Page.FindCamstarControl("EntryType") as CWC.DropDownList; } }
        protected JQDataGrid _gridSubGroupDataList { get { return Page.FindCamstarControl("SubGroupDataList") as JQDataGrid; } }
        protected CWC.TextBox _txtSubGroupAnnotation { get { return Page.FindCamstarControl("SubGroupAnnotation") as CWC.TextBox; } }
        protected CWC.NamedObject _ndoSPCAnnotationCategory { get { return Page.FindCamstarControl("ObjectChanges_ss_SPCAnnotationCategory") as CWC.NamedObject; } }
        protected CWC.CheckBox _chkSubGroupExcludeData { get { return Page.FindCamstarControl("SubGroupExcludeData") as CWC.CheckBox; } }
        protected CWC.NamedObject _ndoSubGroupExcludeReason { get { return Page.FindCamstarControl("SubGroupExcludeReason") as CWC.NamedObject; } }

        protected CWC.TextBox _txtDataPointID { get { return Page.FindCamstarControl("DataPointID") as CWC.TextBox; } }
        protected CWC.TextBox _txtDataPointName { get { return Page.FindCamstarControl("DataPointName") as CWC.TextBox; } }
        protected CWC.TextBox _txtSPCTxnDataName { get { return Page.FindCamstarControl("SPCTxnDataName") as CWC.TextBox; } }
        protected CWC.TextBox _txtSPCName { get { return Page.FindCamstarControl("SPCSetupName") as CWC.TextBox; } }
        protected CWC.TextBox _txtIsAnnotationRequired { get { return Page.FindCamstarControl("IsAnnotationRequired") as CWC.TextBox; } }

        protected CWC.Label _lblBySubGroupLabel { get { return Page.FindCamstarControl("BySubGroupLabel") as CWC.Label; } }
        protected CWC.Label _lblByDataValueLabel { get { return Page.FindCamstarControl("ByDataValueLabel") as CWC.Label; } }

        protected DataEnvelopControl _DataEnvelop { get { return Page.FindCamstarControl("Envelop") as DataEnvelopControl; } }

        //-----------------------------------------
        //
        //-----------------------------------------
        private void AddDataEnvelopDataMember()
        {
            int intConfiguredDataMemberCount = 0;

            if (Page.DataContract != null)
            {
                if (Page.DataContract.DataMembers != null)
                    intConfiguredDataMemberCount = Page.DataContract.DataMembers.Length;
            }
            else
                Page.DataContract = new UIComponentDataContract();

            // manually add the dataContractMember since the custom control's property does not show up at design time
            UIComponentDataMember[] objPageDataMembers = new UIComponentDataMember[intConfiguredDataMemberCount + 1];
            int intDMIndex = 0;

            if (Page.DataContract.DataMembers != null)
            {
                foreach (UIComponentDataMember objDM in Page.DataContract.DataMembers)
                {
                    objPageDataMembers[intDMIndex] = new UIComponentDataMember();
                    objPageDataMembers[intDMIndex] = objDM;
                    intDMIndex++;
                }
            }

            // add the new envelop data member
            objPageDataMembers[intDMIndex] = new UIComponentDataMember();
            objPageDataMembers[intDMIndex].Key = "SPCTxnDataControlWP.Envelop";
            objPageDataMembers[intDMIndex].Name = "envelopSubPopupOutDM";
            objPageDataMembers[intDMIndex].ConnectionType = DataMemberConnectionType.Control;
            objPageDataMembers[intDMIndex].Property = "SS_DataPacket";

            Page.DataContract.DataMembers = objPageDataMembers;
        }

        void _ndoSubGroupExcludeReason_DataChanged(object sender, EventArgs e)
        {
            if (_ndoSubGroupExcludeReason.Data != null)
                _chkSubGroupExcludeData.Enabled = true;
            else
                _chkSubGroupExcludeData.Enabled = false;
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        void _EntryType_DataChanged(object sender, EventArgs e)
        {
            foreach (JQField field in _gridSubGroupDataList.BoundContext.Fields)
            {
                switch (field.ID)
                {
                    case "Description":
                        field.Visible = _ddlEntryType.DropDownControl.SelectedIndex == 0 ? false : true;
                        break;
                    case "ExcludeData":
                        field.Visible = _ddlEntryType.DropDownControl.SelectedIndex == 0 ? false : true;
                        break;
                    case "ExcludeReason":
                        field.Visible = _ddlEntryType.DropDownControl.SelectedIndex == 0 ? false : true;
                        break;
                    case "ss_SPCAnnotationCategory":
                        field.Visible = _ddlEntryType.DropDownControl.SelectedIndex == 0 ? false : true;
                        break;
                }
            }

            _txtSubGroupAnnotation.Visible = _ddlEntryType.DropDownControl.SelectedIndex == 0 ? true : false;
            _chkSubGroupExcludeData.Visible = _ddlEntryType.DropDownControl.SelectedIndex == 0 ? true : false;
            _ndoSubGroupExcludeReason.Visible = _ddlEntryType.DropDownControl.SelectedIndex == 0 ? true : false;
            _ndoSPCAnnotationCategory.Visible = _ddlEntryType.DropDownControl.SelectedIndex == 0 ? true : false;

            CamstarWebControl.SetRenderToClient(_gridSubGroupDataList);
        } // _EntryType_DataChanged

        //-----------------------------------------
        //
        //-----------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!Page.IsPostBack)
            {
                AddDataEnvelopDataMember();

                // add the data contract member        
                if (Page.DataContract.GetValueByName("envelopSubPopupInDM") != null)
                    _DataEnvelop.SS_DataPacket = Page.DataContract.GetValueByName("envelopSubPopupInDM") as DataPacket;
                LoadEntryType();
                LoadDataPointValues();
            }

            _ddlEntryType.DataChanged += new EventHandler(_EntryType_DataChanged);
            _ndoSubGroupExcludeReason.DataChanged += _ndoSubGroupExcludeReason_DataChanged;

        } // OnLoad

        //-----------------------------------------
        //
        //-----------------------------------------
        public void LoadEntryType()
        {
            _ddlEntryType.DropDownControl.Items.Add(_lblBySubGroupLabel.Text);
            _ddlEntryType.DropDownControl.Items.Add(_lblByDataValueLabel.Text);
            _ddlEntryType.DropDownControl.SelectedIndex = 0;
        } // LoadEntryType

        //-----------------------------------------
        //
        //-----------------------------------------
        public void LoadDataPointValues()
        {

            // have to use two grids since the service grid is strongly typed
            DataTable dtDataValues = new DataTable();
            DataRow drDataValue;
            DataColumn dc;
            dc = new DataColumn("Name");
            dtDataValues.Columns.Add(dc);

            dc = new DataColumn("DataValue");
            dtDataValues.Columns.Add(dc);

            dc = new DataColumn("Description");
            dtDataValues.Columns.Add(dc);

            dc = new DataColumn("OverrideValue");
            dtDataValues.Columns.Add(dc);

            dc = new DataColumn("ExcludeData");
            dtDataValues.Columns.Add(dc);

            dc = new DataColumn("ExcludeReason");
            dtDataValues.Columns.Add(dc);

            dc = new DataColumn("IsExisting");
            dtDataValues.Columns.Add(dc);

            dc = new DataColumn("SPCAnnotationCategoryName");
            dtDataValues.Columns.Add(dc);

            dc = new DataColumn("SPCAnnotationCategory");
            dtDataValues.Columns.Add(dc);

            if (_txtDataPointID.Data != null)
            {
                string[] strData = _txtDataPointID.Data.ToString().Split(',');
                SPCTxnDataPointChanges[] objTxnDataPoints = new SPCTxnDataPointChanges[strData.Length];
                int intItemCount = 0;
                Hashtable htDataValueIndex = new Hashtable();

                string strWhereClause = "";
                string firstDataValue = null;

                foreach (string strItem in strData)
                {
                    string[] strValues = strItem.Split('|');

                    // use notes field to hold the datavalue for display only
                    objTxnDataPoints[intItemCount] = new SPCTxnDataPointChanges();
                    objTxnDataPoints[intItemCount].Name = strValues[0];
                    objTxnDataPoints[intItemCount].Notes = strValues[strValues.Length - 1];
                    htDataValueIndex.Add(strValues[0], intItemCount);

                    drDataValue = dtDataValues.NewRow();
                    drDataValue["Name"] = strValues[0];
                    drDataValue["DataValue"] = strValues[strValues.Length - 1];
                    drDataValue["Description"] = "";
                    drDataValue["OverrideValue"] = "";
                    drDataValue["ExcludeData"] = "False";
                    drDataValue["ExcludeReason"] = "";
                    drDataValue["IsExisting"] = "0";
                    drDataValue["SPCAnnotationCategory"] = "";
                    dtDataValues.Rows.Add(drDataValue);

                    intItemCount++;

                    if (String.IsNullOrEmpty(firstDataValue))
                    {
                        firstDataValue = strValues[0];
                    }

                    if (strWhereClause == "")
                        strWhereClause = "'" + strValues[0] + "'";
                    else
                        strWhereClause = strWhereClause + ", '" + strValues[0] + "'";
                }

                string strSQL = "SELECT A.SPCTxnDataPointName, A.Description, A.OverrideValue, A.ExcludeData, B.SPCExcludeReasonName , A.SS_SPCANNOTATIONCATEGORYNAME FROM A_SPCTxnDataPoint A LEFT OUTER JOIN A_SPCExcludeReason B ON A.ExcludeReasonId = B.SPCExcludeReasonId WHERE A.SPCTxnDataPointName ";
                if (intItemCount > 0)
                    strSQL = strSQL + "IN (" + strWhereClause + ")";
                else
                    strSQL = strSQL + "= " + strWhereClause;

                var fs = FrameworkManagerUtil.GetFrameworkSession();
                QueryService objSvc = new QueryService(fs.CurrentUserProfile);

                // set the query options
                OM.QueryOptions objQueryOptions = new OM.QueryOptions();
                objQueryOptions.StartRow = 1;
                objQueryOptions.RowSetSize = intItemCount;

                OM.RecordSet objRecordSet = new OM.RecordSet();
                OM.ResultStatus objRS = objSvc.ExecuteAdHoc(strSQL, objQueryOptions, out objRecordSet);
                if (objRS.IsSuccess)
                {
                    int intRowIndex = 0;
                    DataTable dtResult = objRecordSet.GetAsExplicitlyDataTable();
                    bool blnFirstData = true;
                    foreach (DataRow dt in dtResult.Rows)
                    {
                        //get the row index of both arrays
                        if (htDataValueIndex.ContainsKey(dt["SPCTxnDataPointName"].ToString()))
                        {
                            intRowIndex = int.Parse(htDataValueIndex[dt["SPCTxnDataPointName"].ToString()].ToString());
                            // update control list
                            dtDataValues.Rows[intRowIndex]["Description"] = dt["Description"].ToString();
                            dtDataValues.Rows[intRowIndex]["OverrideValue"] = dt["OverrideValue"].ToString();
                            dtDataValues.Rows[intRowIndex]["ExcludeData"] = dt["ExcludeData"].ToString();
                            dtDataValues.Rows[intRowIndex]["ExcludeReason"] = dt["SPCExcludeReasonName"].ToString();
                            dtDataValues.Rows[intRowIndex]["IsExisting"] = "1";
                            dtDataValues.Rows[intRowIndex]["SPCAnnotationCategory"] = dt["ss_SPCAnnotationCategoryName"].ToString();
                            dtDataValues.Rows[intRowIndex]["SPCAnnotationCategoryName"] = dt["ss_SPCAnnotationCategoryName"].ToString();
                            dtDataValues.Rows[intRowIndex]["DataValue"] = "44";

                            // update txnDataPointList,
                            objTxnDataPoints[intRowIndex].Description = dt["Description"].ToString();
                            if (!string.IsNullOrEmpty(dt["OverrideValue"].ToString()))
                                objTxnDataPoints[intRowIndex].OverrideValue = double.Parse(dt["OverrideValue"].ToString());

                            bool bIsExcluded = false;
                            try
                            { bIsExcluded = string.IsNullOrEmpty(dt["ExcludeData"].ToString()) ? false : bool.Parse(dt["ExcludeData"].ToString()); }
                            catch
                            { bIsExcluded = false; }

                            objTxnDataPoints[intRowIndex].ExcludeData = bIsExcluded; //string.IsNullOrEmpty(dt["ExcludeData"].ToString()) ? false : bool.Parse(dt["ExcludeData"].ToString());                            
                            objTxnDataPoints[intRowIndex].ExcludeReason = new NamedObjectRef(dt["SPCExcludeReasonName"].ToString());
                            objTxnDataPoints[intRowIndex].ss_SPCAnnotationCategory = new NamedObjectRef(dt["ss_SPCAnnotationCategoryName"].ToString());

                            if (dt["SPCTxnDataPointName"].ToString() == firstDataValue)
                            {
                                _txtSubGroupAnnotation.Data = dt["Description"].ToString();
                                _ndoSPCAnnotationCategory.Data = new NamedObjectRef(dt["ss_SPCAnnotationCategoryName"].ToString());
                                _chkSubGroupExcludeData.CheckControl.Checked = bIsExcluded;// string.IsNullOrEmpty(dt["ExcludeData"].ToString()) ? false : bool.Parse(dt["ExcludeData"].ToString());
                                _ndoSubGroupExcludeReason.Data = new NamedObjectRef(dt["SPCExcludeReasonName"].ToString());
                            }
                        }
                    }
                }

                ViewState["SPCChartDataValue_ControlList"] = dtDataValues;

                _gridSubGroupDataList.ClearData();
                (_gridSubGroupDataList.GridContext as ItemDataContext).Data = objTxnDataPoints;
                _gridSubGroupDataList.BoundContext.LoadData();

                CamstarWebControl.SetRenderToClient(_gridSubGroupDataList);
            }
        } // LoadDataPointValues

        //-----------------------------------------
        //
        //-----------------------------------------
        public override void WebPartCustomAction(object sender, CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as CustomAction;
            if (action != null)
            {
                if (action.Parameters == "Close")
                {
                    ScriptManager.RegisterStartupScript(Page.Form, GetType(), "OKButton", "window.parent.__page._isTabContainerPage=null;window.parent.CloseFloatingFrame(false);", true);
                }

                if (action.Parameters == "Submit")
                {
                    e.Result = ExecuteSubmit();
                }
            }
        } // WebPartCustomAction

        //-----------------------------------------
        //
        //-----------------------------------------
        public ResultStatus ExecuteSubmit()
        {
            ResultStatus ReturnResultStatus = new ResultStatus();
            try
            {
                if (_txtIsAnnotationRequired.Data != null && _txtIsAnnotationRequired.Data.ToString() == "1") // if the data override checkbox is checked as true
                {
                    // Create & initialize a boolean flag to false
                    bool isAnnotationRequired = false;
                    if (_ddlEntryType.DropDownControl.SelectedIndex == 0)
                    {
                        isAnnotationRequired = (_chkSubGroupExcludeData.CheckControl.Checked && _txtSubGroupAnnotation.Data == null);

                        for (int i = 0; i < _gridSubGroupDataList.GridContext.GetTotalRows(); i++)
                        {
                            string id = IdGenerator(i);
                            var OverrideDataValue = (_gridSubGroupDataList.GridContext.GetItem(id) as SPCTxnDataPointChanges).OverrideValue;
                            if (OverrideDataValue != null && _txtSubGroupAnnotation.Data == null) { isAnnotationRequired = true; break; }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < _gridSubGroupDataList.GridContext.GetTotalRows(); i++)
                        {
                            string id = IdGenerator(i);
                            var OverrideDataValue = (_gridSubGroupDataList.GridContext.GetItem(id) as SPCTxnDataPointChanges).OverrideValue;
                            var Annotation = (_gridSubGroupDataList.GridContext.GetItem(id) as SPCTxnDataPointChanges).Description;
                            var EcxludeData = (_gridSubGroupDataList.GridContext.GetItem(id) as SPCTxnDataPointChanges).ExcludeData;
                            if (OverrideDataValue != null || EcxludeData == true)
                            {
                                if (Annotation == null) { isAnnotationRequired = true; break; }
                                if (Annotation.ToString() == "") { isAnnotationRequired = true; break; }
                            }
                        }
                    }

                    // if the data value been override and the Annotation is not set then the system will trigger this logic
                    if (isAnnotationRequired)
                    {
                        OM.ResultStatus resultStatus = new OM.ResultStatus("Annotation is required", false);
                        return resultStatus;
                    }
                }

                UserProfile profile = HttpContext.Current.Session[Camstar.WebPortal.Constants.SessionConstants.UserProfile] as UserProfile;
                SPCTxnDataPointMaintService objService = new SPCTxnDataPointMaintService(profile);

                SPCTxnDataPointMaint objServiceData = new SPCTxnDataPointMaint();
                SPCTxnDataPointChanges objChanges = new SPCTxnDataPointChanges();
                SPCTxnDataPointMaint_Info objServiceInfo = new SPCTxnDataPointMaint_Info();
                SPCTxnDataPointChanges_Info objChangesInfo = new SPCTxnDataPointChanges_Info();
                SPCTxnDataPointMaint_Result result = null;

                SPCTxnDataPointChanges[] objTxnData = new SPCTxnDataPointChanges[_gridSubGroupDataList.GridContext.GetTotalRows()];
                objTxnData = _gridSubGroupDataList.Data as SPCTxnDataPointChanges[];

                // pull the control dataset from viewstate
                DataTable dtControl = new DataTable();
                dtControl = ViewState["SPCChartDataValue_ControlList"] as DataTable;
                int intIndex = 0;

                string strSuccessMsg = "";
                string strErrorMsg = "";

                foreach (SPCTxnDataPointChanges objDataPoint in objTxnData)
                {
                    if (_ddlEntryType.DropDownControl.SelectedIndex == 0) //SUBGROUP ENTRY TYPE
                    {
                        objDataPoint.Description = _txtSubGroupAnnotation.Data != null ? _txtSubGroupAnnotation.Data.ToString() : "";
                        objDataPoint.scsReqOverrideAnnotations = _txtSubGroupAnnotation.Required;
                        objDataPoint.ExcludeData = _chkSubGroupExcludeData.CheckControl.Checked;
                        objDataPoint.ExcludeReason = _ndoSubGroupExcludeReason.Data as NamedObjectRef;
                        objDataPoint.ss_SPCAnnotationCategory = _ndoSPCAnnotationCategory.Data as NamedObjectRef;
                        objDataPoint.ss_SPCAnnotationCategoryName = _ndoSPCAnnotationCategory.Data != null ? _ndoSPCAnnotationCategory.Data.ToString() : "";
                    }

                    if ((dtControl.Rows[intIndex]["Description"].ToString() != (objDataPoint.Description != null ? objDataPoint.Description : "")) ||
                        (dtControl.Rows[intIndex]["OverrideValue"].ToString() != (objDataPoint.OverrideValue != null ? objDataPoint.OverrideValue.ToString() : "")) ||
                        (dtControl.Rows[intIndex]["ExcludeData"].ToString() != (objDataPoint.ExcludeData != null ? (objDataPoint.ExcludeData == true ? "True" : "False") : "False")) ||
                        (dtControl.Rows[intIndex]["ExcludeReason"].ToString() != (objDataPoint.ExcludeReason != null ? objDataPoint.ExcludeReason.Name : "")) ||
                        (dtControl.Rows[intIndex]["SPCAnnotationCategory"].ToString() != (objDataPoint.ss_SPCAnnotationCategory != null ? objDataPoint.ss_SPCAnnotationCategory.Name : "")))
                    {
                        objServiceData = new SPCTxnDataPointMaint();
                        objChanges = new SPCTxnDataPointChanges();
                        result = new SPCTxnDataPointMaint_Result();

                        objChanges.Name = objDataPoint.Name;

                        objChanges.Description = objDataPoint.Description != null ? objDataPoint.Description : "";
                        objChanges.OverrideValue = objDataPoint.OverrideValue != null ? objDataPoint.OverrideValue : null;
                        objChanges.scsReqOverrideAnnotations = objDataPoint.scsReqOverrideAnnotations != null ? objDataPoint.scsReqOverrideAnnotations : false;
                        objChanges.ExcludeData = objDataPoint.ExcludeData != null ? objDataPoint.ExcludeData : false;
                        objChanges.ExcludeReason = objDataPoint.ExcludeReason != null ? objDataPoint.ExcludeReason : null;
                        objChanges.ss_SPCAnnotationCategory = objDataPoint.ss_SPCAnnotationCategory != null ? objDataPoint.ss_SPCAnnotationCategory : null;
                        objChanges.ss_SPCAnnotationCategoryName = objDataPoint.ss_SPCAnnotationCategory != null ? objDataPoint.ss_SPCAnnotationCategory.ToString() : "";

                        OM.ResultStatus resultStatus = null;

                        objService.BeginTransaction();

                        if (dtControl.Rows[intIndex]["IsExisting"].ToString() == "1")
                        {
                            objServiceData.ObjectToChange = new NamedObjectRef(objDataPoint.Name.ToString());

                            if (objChanges.Description == "" && objChanges.OverrideValue == null && objChanges.ExcludeData == false && objChanges.ss_SPCAnnotationCategoryName == "")
                            {
                                objService.Delete(objServiceData);
                                objService.ExecuteTransaction();
                            }
                            else
                            {
                                if (dtControl.Rows[intIndex]["OverrideValue"].ToString() != "" && objChanges.OverrideValue == null)
                                {
                                    objChanges.OverrideValue = new Primitive<double>();
                                }
                                objService.Load(objServiceData);
                                objServiceData.ObjectChanges = objChanges;
                                objService.ExecuteTransaction(objServiceData);
                            }
                        }
                        else
                        {
                            objServiceData.ObjectChanges = objChanges;
                            objService.New(objServiceData);
                            objService.ExecuteTransaction();
                        }

                        resultStatus = objService.CommitTransaction();

                        if (resultStatus.IsSuccess)
                        {
                            if (strSuccessMsg != "")
                                strSuccessMsg = strSuccessMsg + ";";

                            strSuccessMsg = strSuccessMsg + _lblByDataValueLabel.Text + " " + objDataPoint.Notes + ": " + resultStatus.Message + System.Environment.NewLine;
                        }
                        else
                        {
                            strErrorMsg = strErrorMsg + _lblByDataValueLabel.Text + " " + objDataPoint.Notes + ": " + resultStatus.ExceptionData + System.Environment.NewLine;
                        }
                    } // if

                    intIndex++;
                } // foreach

                string strCompletionMsg = strSuccessMsg + strErrorMsg;
                if (strSuccessMsg != "")
                {
                    DataPacket SS_DataPacket = new DataPacket();
                    if (_DataEnvelop.SS_DataPacket != null)
                        SS_DataPacket = _DataEnvelop.SS_DataPacket;

                    SS_DataPacket.ResultStatusMessage = strCompletionMsg;
                    _DataEnvelop.SS_DataPacket = SS_DataPacket;

                    ScriptManager.RegisterStartupScript(Page.Form, GetType(), "OKButton", "window.parent.__page._isTabContainerPage=null;window.parent.CloseFloatingFrame(true);", true);
                }
                else if (strCompletionMsg != "")
                {
                    ReturnResultStatus = new ResultStatus(strCompletionMsg, false);
                }
            }
            catch (Exception ex)
            {
                ReturnResultStatus = new ResultStatus((ex.Message.ToString()), false);
            }
            return ReturnResultStatus;
        } // ExecuteSubmit

        private string IdGenerator(int num) { return num.ToString().PadLeft(6, '0'); }
    }
}



