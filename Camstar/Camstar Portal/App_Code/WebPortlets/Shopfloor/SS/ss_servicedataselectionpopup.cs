/* Copyright 2023 Siemens */
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
using CGC = Camstar.WebPortal.FormsFramework.WebGridControls;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_ServiceDataSelectionPopUp : MatrixWebPart
    {
        //Controls declaration
        private CWC.DropDownList _ndoOnlineQuerySetupField { get { return Page.FindCamstarControl("OnlineQuerySetupField") as CWC.DropDownList; } }
        private CWC.TextBox _txtHiddenPrimaryServiceType { get { return Page.FindCamstarControl("HiddenPrimaryServiceType") as CWC.TextBox; } }
        private SEMI.AppCode.DataEnvelopControl _envSelectedLots { get { return Page.FindCamstarControl("SelectedLotsList") as SEMI.AppCode.DataEnvelopControl; } }
        private CWC.TextBox _txtQueryTypeTextBox { get { return Page.FindCamstarControl("QueryTypeTextBox") as CWC.TextBox; } }
        private CWC.TextBox _txtColNameTextBox { get { return Page.FindCamstarControl("ColNameTextBox") as CWC.TextBox; } }
        private CWC.TextBox _txtHiddenNoResultsFound { get { return Page.FindCamstarControl("HiddenNoResultsFound") as CWC.TextBox; } }
        private JQDataGrid _gridParametersField { get { return Page.FindCamstarControl("ParametersField") as JQDataGrid; } }
        private JQDataGrid _gridQueryGrid { get { return Page.FindCamstarControl("QueryGrid") as JQDataGrid; } }
        private DataTable myParamsGrid = new DataTable();

        //-----------------------------------------
        // Fetch available query setup and bind it into NDO control
        //-----------------------------------------
        public void FetchServiceQueries()
        {
            try
            {
                //Initialize Service & Objects
                UserProfile profile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
                ServiceSetupMaintService Svc = new ServiceSetupMaintService(profile);
                ServiceSetupMaint SvcData = new ServiceSetupMaint();
                ServiceSetupMaint_Info SvcInfo = new ServiceSetupMaint_Info();
                ServiceSetupMaint_Request ReqData = new ServiceSetupMaint_Request();
                ServiceSetupMaint_Result ResData = new ServiceSetupMaint_Result();
                ServiceSetupChanges objChangesData = new ServiceSetupChanges();
                ServiceSetupChanges_Info objChangesInfo = new ServiceSetupChanges_Info();

                //Request Queries Data
                objChangesInfo.Queries = new ServiceSetupQueriesChanges_Info();
                objChangesInfo.Queries.OnlineQuerySetup = new Info(true);
                objChangesInfo.Queries.QueryType = new Info(true);
                SvcInfo.ObjectChanges = objChangesInfo;
                ReqData.Info = SvcInfo;

                //Register Page Primary Service Type
                SvcData.ObjectToChange = new NamedObjectRef();
                if (_txtHiddenPrimaryServiceType.Data != null)
                    SvcData.ObjectToChange.Name = _txtHiddenPrimaryServiceType.Data.ToString();

                //Execute Service
                ResultStatus Results = Svc.Load(SvcData, ReqData, out ResData);
                if (Results.IsSuccess)
                {
                    if (ResData.Value.ObjectChanges != null)
                    {
                        //Populate Online Query Setup Field selection
                        objChangesData = ResData.Value.ObjectChanges;
                        CWC.DropDownList _DropDownListControl = Page.FindCamstarControl("OnlineQuerySetupField") as CWC.DropDownList;
                        List<NamedObjectRef> NamedObjectRefList = new List<NamedObjectRef>();
                        int x = 0;
                        if (objChangesData.Queries != null)
                        {
                            foreach (ServiceSetupQueriesChanges objQueries in objChangesData.Queries)
                            {
                                if (objQueries.QueryType == _txtQueryTypeTextBox.Data.ToString())
                                {
                                    NamedObjectRefList.Add(new NamedObjectRef { Name = objQueries.OnlineQuerySetup.Name });
                                    x = x + 1;
                                }
                            }


                            //Bind object to the NDO control
                            SEMI.AppCode.ControlsUtility.DropDownListControl_SetSelectionValues(ref _DropDownListControl, NamedObjectRefList.ToArray());

                            if (_ndoOnlineQuerySetupField.Data == null)
                            {
                                _ndoOnlineQuerySetupField.Data = NamedObjectRefList[0].Name;
                                _ndoOnlineQuerySetupField.DropDownControl.SelectedIndex = 0;
                            }
                        }
                    }
                }
                else
                {
                    this.DisplayMessage(Results);
                    _ndoOnlineQuerySetupField.Enabled = false;
                }
            }
            catch (Exception ex) //Catch errors
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

        //-----------------------------------------
        // Fetch query parameters
        //-----------------------------------------
        public void FetchQueryData()
        {
            try
            {
                //Initialize Service & Objects
                UserProfile profile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;

                OnlineQuerySetupMaintService Svc = new OnlineQuerySetupMaintService(profile);
                OnlineQuerySetupMaint SvcData = new OnlineQuerySetupMaint();
                OnlineQuerySetupMaint_Info SvcInfo = new OnlineQuerySetupMaint_Info();
                OnlineQuerySetupMaint_Request ReqData = new OnlineQuerySetupMaint_Request();
                OnlineQuerySetupMaint_Result ResData = new OnlineQuerySetupMaint_Result();

                OnlineQuerySetupChanges objChangesData = new OnlineQuerySetupChanges();
                OnlineQuerySetupChanges_Info objChangesInfo = new OnlineQuerySetupChanges_Info();
                OnlineQuerySetupParamsChanges objParamData = new OnlineQuerySetupParamsChanges();
                OnlineQuerySetupParamsChanges_Info objParamInfo = new OnlineQuerySetupParamsChanges_Info();

                string onlineQueryName = "";
                if (_ndoOnlineQuerySetupField.Data != null)
                    onlineQueryName = _ndoOnlineQuerySetupField.Data.ToString();

                if (_txtQueryTypeTextBox.Data != null)
                {
                    switch (_txtQueryTypeTextBox.Data.ToString().ToUpper())
                    {
                        case "CARRIER":
                            objChangesInfo.CarrierColName = new Info(true);
                            break;
                        case "EQUIPMENT":
                            objChangesInfo.EquipmentColName = new Info(true);
                            break;
                        default:
                            objChangesInfo.LotColName = new Info(true);
                            break;
                    }
                }
                objChangesInfo.ResultsetSizeLimit = new Info(true);

                objParamInfo.Name = new Info(true);
                objParamInfo.DisplayText = new Info(true);
                objParamInfo.IsRequired = new Info(true);
                objParamInfo.DefaultValue = new Info(true);
                objParamInfo.UseForBlockRows = new Info(true);
                objParamInfo.UseForBlockRows = new Info(true);

                objChangesInfo.UserQueryParameters = objParamInfo;
                SvcInfo.ObjectChanges = objChangesInfo;
                ReqData.Info = SvcInfo;

                SvcData.ObjectToChange = new NamedObjectRef();
                SvcData.ObjectToChange.Name = onlineQueryName;

                RecordSet objRecordSet = new RecordSet();
                ResultStatus Results = Svc.Load(SvcData, ReqData, out ResData);
                if (Results.IsSuccess)
                {
                    if (ResData.Value.ObjectChanges != null)
                    {
                        var newObjChanges = new List<OnlineQuerySetupParamsChanges>();
                        objChangesData = ResData.Value.ObjectChanges;
                        if (objChangesData.UserQueryParameters != null)
                        {
                            objChangesData.UserQueryParameters = objChangesData.UserQueryParameters.OrderByDescending(p => p.Name?.Value).ToArray();
                            if (_txtQueryTypeTextBox.Data != null)
                            {
                                switch (_txtQueryTypeTextBox.Data.ToString().ToUpper())
                                {
                                    case "CARRIER":
                                        if (objChangesData.CarrierColName != null)
                                            _txtColNameTextBox.Data = objChangesData.CarrierColName;
                                        break;
                                    case "EQUIPMENT":
                                        if (objChangesData.EquipmentColName != null)
                                            _txtColNameTextBox.Data = objChangesData.EquipmentColName;
                                        break;
                                    case "LOT":
                                        if (objChangesData.LotColName != null)
                                            _txtColNameTextBox.Data = objChangesData.LotColName;
                                        break;
                                    default:
                                        break;
                                }
                            }
                            int queryCount = objChangesData.UserQueryParameters.Count();
                            for (int i = 0; i < queryCount; i++)
                            {
                                if (objChangesData.UserQueryParameters[i].DefaultValue == null)
                                {
                                    // Set the Param Value using line assignment data if Param Name matches and a value exist
                                    switch (objChangesData.UserQueryParameters[i].Name.ToString().ToUpper())
                                    {
                                        case "FACTORY":
                                            if (Page.SessionDataContract.GetValueByName(Camstar.WebPortal.Constants.DataMemberConstants.Factory) != null)
                                                objChangesData.UserQueryParameters[i].DefaultValue = Page.SessionDataContract.GetValueByName(Camstar.WebPortal.Constants.DataMemberConstants.Factory).ToString();
                                            break;
                                        case "OPERATION":
                                            if (Page.SessionDataContract.GetValueByName(Camstar.WebPortal.Constants.DataMemberConstants.Operation) != null)
                                                objChangesData.UserQueryParameters[i].DefaultValue = Page.SessionDataContract.GetValueByName(Camstar.WebPortal.Constants.DataMemberConstants.Operation).ToString();
                                            break;
                                        case "RESOURCE":
                                            if (Page.SessionDataContract.GetValueByName(Camstar.WebPortal.Constants.DataMemberConstants.Resource) != null)
                                                objChangesData.UserQueryParameters[i].DefaultValue = Page.SessionDataContract.GetValueByName(Camstar.WebPortal.Constants.DataMemberConstants.Resource).ToString();
                                            break;
                                        case "WORKCENTER":
                                            if (Page.SessionDataContract.GetValueByName(Camstar.WebPortal.Constants.DataMemberConstants.WorkCenter) != null)
                                                objChangesData.UserQueryParameters[i].DefaultValue = Page.SessionDataContract.GetValueByName(Camstar.WebPortal.Constants.DataMemberConstants.WorkCenter).ToString();
                                            break;
                                        case "WORKSTATION":
                                            if (Page.SessionDataContract.GetValueByName(Camstar.WebPortal.Constants.DataMemberConstants.WorkStation) != null)
                                                objChangesData.UserQueryParameters[i].DefaultValue = Page.SessionDataContract.GetValueByName(Camstar.WebPortal.Constants.DataMemberConstants.WorkStation).ToString();
                                            break;
										case "SPEC":
                                            if (Page.SessionDataContract.GetValueByName(Camstar.WebPortal.Constants.DataMemberConstants.Spec) != null)
                                                objChangesData.UserQueryParameters[i].DefaultValue = Page.SessionDataContract.GetValueByName(Camstar.WebPortal.Constants.DataMemberConstants.Spec).ToString();
                                            else
                                                objChangesData.UserQueryParameters[i].DefaultValue = "%";
                                                break;
                                        default:
                                            break;
                                    }
                                }

                                if (objChangesData.UserQueryParameters[i].DisplayText != null)
                                {
                                    newObjChanges.Add(new OnlineQuerySetupParamsChanges
                                    {
                                        Name = objChangesData.UserQueryParameters[i].Name,
                                        DisplayText = objChangesData.UserQueryParameters[i].DisplayText,
                                        DefaultValue = objChangesData.UserQueryParameters[i].DefaultValue
                                    });
                                }
                                else
                                {
                                    newObjChanges.Add(new OnlineQuerySetupParamsChanges
                                    {
                                        Name = objChangesData.UserQueryParameters[i].Name,
                                        DisplayText = objChangesData.UserQueryParameters[i].Name,
                                        DefaultValue = objChangesData.UserQueryParameters[i].DefaultValue
                                    });
                                }
                            }
                            _gridParametersField.ClearData();
                            _gridParametersField.Data = newObjChanges.ToArray();
                            _gridParametersField.OriginalData = newObjChanges.ToArray();
                            CamstarWebControl.SetRenderToClient(_gridParametersField);
                        }
                        else
                        {
                            _gridParametersField.ClearData();
                        }
                    }
                    else
                    {
                        _gridParametersField.ClearData();
                    }
                }
                else
                {
                    _gridParametersField.ClearData();
                }
            }
            catch (Exception ex) //Catch errors
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

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

        public void DisplayNoDataError()
        {
            try
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

                    _txtHiddenNoResultsFound.ClearData();
                }
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
                //Add parameters
                int paramCount = _gridParametersField.TotalRowCount;
                List<QueryParameter> paramChanges = new List<QueryParameter>();

                if (paramCount != null)
                {

                    for (int i = 0; i < paramCount; i++)
                    {
                        if (_gridParametersField.GridContext.GetCell(Convert.ToString(i), "Name") != null && !string.IsNullOrWhiteSpace(_gridParametersField.GridContext.GetCell(Convert.ToString(i), "Name").ToString()))
                        {
                            QueryParameter currentparam = new QueryParameter() { Name = _gridParametersField.GridContext.GetCell(Convert.ToString(i), "Name").ToString() };

                            if (_gridParametersField.GridContext.GetCell(Convert.ToString(i), "DefaultValue") != null)
                                currentparam.Value = _gridParametersField.GridContext.GetCell(Convert.ToString(i), "DefaultValue").ToString();
                            paramChanges.Add(currentparam);
                        }
                    }
                }
                queryState.Parameters = paramChanges.ToArray();
                queryState.Options.QueryType = WCF.ObjectStack.QueryType.User;
                //queryState.Options.RequestRecordSetAndCount = true;
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

        //-----------------------------------------
        // OnlineQuerySetupField data changed event
        //-----------------------------------------
        public void OnlineQuerySetupField_DataChanged(object sender, EventArgs e)
        {
            try
            {
                FetchQueryData();
            }
            catch (Exception ex) //Catch errors
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

        //-----------------------------------------
        // Collect selected lots from the grid
        //-----------------------------------------
        public void CollectSelectedLot()
        {
            try
            {
                if (_gridQueryGrid.GridContext.SelectedRowIDs != null)
                {
                    int countSelectedLots = _gridQueryGrid.GridContext.SelectedRowIDs.Count;
                    List<string> selectedLots = new List<string>();
                    for (int i=0; i<countSelectedLots; i++)
                    {
                        selectedLots.Add((_gridQueryGrid.GridContext as DataGridContext).SelectedRowsTable.Rows[i].Field<String>(_txtColNameTextBox.Data.ToString()));
                    }
                    _envSelectedLots.SS_ContainersList = selectedLots.ToArray();
                }
            }
            catch (Exception ex) //Catch errors
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

        //-----------------------------------------
        // Override WebPartCustomAction
        //-----------------------------------------
        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e) //Custom action settings
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as CustomAction;
            if (action != null)
            {
                if (action.Parameters == "OK")
                {
                    CollectSelectedLot();
                    Page.CloseFloatingFrameOnSubmit(new ResultStatus());
                }
            }
        }

        //-----------------------------------------
        // Override OnLoad event
        //-----------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);
                Page.CollectDataContract();
                _ndoOnlineQuerySetupField.DataChanged += new EventHandler(OnlineQuerySetupField_DataChanged);
                (_gridQueryGrid.GridContext as CGC.QueryContext).BeforeQueryExecution += ResultsGrid_BeforeQueryExecution;
                //(_gridQueryGrid.GridContext as CGC.QueryContext).AfterQueryExecution += ResultsGrid_AfterQueryExecution;
                if (!this.Page.IsPostBack)
                {
                    FetchServiceQueries();
                    if (_ndoOnlineQuerySetupField.Data != null)
                        FetchQueryData();

                    SEMI.AppCode.UIUtility.MaximizePopUp(this);
                }
                else
                {
                    CollectSelectedLot();
                }
            }
            catch (Exception ex) //Catch errors
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }
    }
}



