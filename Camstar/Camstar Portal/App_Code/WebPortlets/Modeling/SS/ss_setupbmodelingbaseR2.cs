/* Copyright 2025 Siemens */
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
using Camstar.WebPortal.PortalFramework;

using System.Collections;

/// <summary>
/// Summary description for SS_PackingQtyMaint
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_SetupBModelingBaseR2 : MatrixWebPart
    {
        protected string _dynamicTypeName = "__dynamicname_SelectionGrid_DataType_";
        protected string _dynamicTypeNameEx = "__dynamicname_SelectionGrid_DataTypeEx_";
        protected string _Data_ViewStateConstant = "";
        protected string _StackData_ViewStateConstant = "";

        protected JQDataGrid _gridSelection { get { return Page.FindCamstarControl("SelectionGrid") as JQDataGrid; } }
        protected CWC.TextBox _txtSelectionName { get { return Page.FindCamstarControl("SelectionName") as CWC.TextBox; } }
        protected CWC.TextBox _txtSelectionInstanceId { get { return Page.FindCamstarControl("SelectionInstanceId") as CWC.TextBox; } }
        protected CWC.TextBox _txtGridPage { get { return Page.FindCamstarControl("SelectionPageNumber") as CWC.TextBox; } }
        protected CWC.Button _btnSelectionRefresh { get { return Page.FindCamstarControl("Selection_PageRefresh") as CWC.Button; } }
        protected CWC.Button _btnSelectionNext { get { return Page.FindCamstarControl("Selection_PageNext") as CWC.Button; } }
        protected CWC.Button _btnSelectionPrev { get { return Page.FindCamstarControl("Selection_PagePrev") as CWC.Button; } }
        protected CWC.NamedObject _ndoObjectToChange { get { return Page.FindCamstarControl("SelectionInstanceRef") as CWC.NamedObject; } }

        protected CWC.TextBox _txtObjectChangesName { get { return Page.FindCamstarControl("ObjectChanges_Name") as CWC.TextBox; } }

        protected CWC.Button _btnSave { get { return Page.FindCamstarControl("Act_Save") as CWC.Button; } }
        protected CWC.Button _btnSaveAsNew { get { return Page.FindCamstarControl("Act_SaveAsNew") as CWC.Button; } }
        protected CWC.Button _btnDelete { get { return Page.FindCamstarControl("Act_Delete") as CWC.Button; } }
        protected CWC.Button _btnReload { get { return Page.FindCamstarControl("Act_Reload") as CWC.Button; } }
        protected CWC.Button _btnViewAudit { get { return Page.FindCamstarControl("Act_ViewAudit") as CWC.Button; } }

        protected CWC.Button _btnClear { get { return Page.FindCamstarControl("Act_Clear") as CWC.Button; } }

        protected CWC.Button _btnClearSelection { get { return Page.FindCamstarControl("Act_ClearSelection") as CWC.Button; } }


        protected CWC.Label _lblRows { get { return Page.FindCamstarControl("SelectionPageRows") as CWC.Label; } }

        protected CWC.CheckBox _chkExistingInstance { get { return Page.FindCamstarControl("IsExistingInstance") as CWC.CheckBox; } }

        protected Hashtable htControlState;

        protected CamstarControlsCollection _SelectionControls;
        protected CamstarControlsCollection _CriteriaControls;
        protected CamstarControlsCollection _SubentityGridControls;
        protected CamstarControlsCollection _CriteriaWorkflowNavigators;
        protected int _iBlockOfRows;
        protected List<string> _HiddenGridColumns;

        protected bool bIsColumnsDefined;
        protected Hashtable htStackValues;

        protected MatrixWebPart wpSelectionWP
        {
            get
            {
                MatrixWebPart wp = null;
                int iParentControlsCount = Parent.Controls.Count;
                for (int z = 0; z < iParentControlsCount; z++)
                {
                    if (Parent.Controls[z].ID == "SS_SetupB_SelectionWP")
                        wp = Parent.Controls[z] as MatrixWebPart;
                }
                return wp;
            }
        }

        protected MatrixWebPart SelectionControlsWP
        {
            get
            {
                MatrixWebPart wp = null;
                int iParentControlsCount = Parent.Controls.Count;
                for (int z = 0; z < iParentControlsCount; z++)
                {
                    if (Parent.Controls[z].ID == "SelectionControlsWP")
                        wp = Parent.Controls[z] as MatrixWebPart;
                }
                return wp;
            }
        }

        protected MatrixWebPart SS_SetupB_ButtonPanelWP
        {
            get
            {
                MatrixWebPart wp = null;
                int iParentControlsCount = Parent.Controls.Count;
                for (int z = 0; z < iParentControlsCount; z++)
                {
                    if (Parent.Controls[z].ID == "SS_SetupB_ButtonPanelWP")
                        wp = Parent.Controls[z] as MatrixWebPart;
                }
                return wp;
            }
        }


        protected override void OnPreRender(EventArgs e)
        {           
            ScriptManager.RegisterStartupScript(this, this.GetType(), "Matrix_AddSlideoutToogler", string.Format("Matrix_AddSlideoutToogler('{0}');", this._gridSelection.ClientID), true);
            base.OnPreRender(e);

            if (SEMI.AppCode.UIUtility.IsPopupClose(this))
            {
                _SetupB_SelectionRefresh();
            }
        }
        //--------------------------------------
        //
        //--------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            Title = "AddButton";
            base.OnLoad(e);

            var theme = Page.Session["CurrentTheme"] != null ? Page.Session["CurrentTheme"].ToString() : string.Empty;

            if (theme.ToLower() == "horizon")
            {
                List<Camstar.WebPortal.FormsFramework.WebControls.Button> lsb = Page.FindCamstarControls<Camstar.WebPortal.FormsFramework.WebControls.Button>();
                foreach (var x in lsb)
                {
                    if (x.DefaultAction != null && x.DefaultAction.ToString().Contains(UIActionTypeEnum.FloatPageOpenAction.ToString("G")))
                    {
                        var floatPageOpenAction = x.DefaultAction as FloatPageOpenAction;
                        if (floatPageOpenAction.FrameLocation != null && floatPageOpenAction.FrameLocation.Height != 0 && floatPageOpenAction.FrameLocation.Width != 0)
                        {
                            floatPageOpenAction.FrameLocation.Height = 0;
                            floatPageOpenAction.FrameLocation.Width = 0;
                        }
                    }
                }
            }

            _dynamicTypeName = "__" + Page.PrimaryServiceType + "_SelectionGrid_DataType_";
            _dynamicTypeNameEx = "__" + Page.PrimaryServiceType + "_SelectionGrid_DataTypeEx_";
            _Data_ViewStateConstant = "__" + Page.PrimaryServiceType + "_SelectionGrid_SessionConstant";
            _StackData_ViewStateConstant = "__" + Page.PrimaryServiceType + "_StackData_SessionConstant";
            _SelectionControls = new CamstarControlsCollection();
            _CriteriaControls = new CamstarControlsCollection();
            _SubentityGridControls = new CamstarControlsCollection();
            _CriteriaWorkflowNavigators = new CamstarControlsCollection();
            _HiddenGridColumns = new List<string>();
            _iBlockOfRows = 2000;

            _gridSelection.RowSelected += new JQGridEventHandler(_gridSelection_RowSelected);

            if (!Page.IsPostBack)
            {
                ResetBasePageControls();
                bIsColumnsDefined = false;
                SEMI.AppCode.UIUtility.MaximizePopUp(this);
                if (Page.IsAJAXFloatingFrame)
                {
                    wpSelectionWP.Hidden = true;
                    SelectionControlsWP.Hidden = true;
                    _btnReload.IsPrimary = false;
                    _btnClear.Text = "Reset";
                    _btnReload.Text = "Reset";
                    SS_SetupB_ButtonPanelWP.Hidden = true;
                }else
                {
                    try
                    {
                        this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "Reset").First().IsHidden = true;
                        this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "Reload").First().IsHidden = true;
                        this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "Save").First().IsHidden = true;
                        this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "SaveAsNew").First().IsHidden = true;
                        this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "Close").First().IsHidden = true;
                    }catch(Exception ex)
                    {

                    }

                }

                _SetupB_SelectionRefresh();
                _btnClearSelection.Text = "Reset";

                var addDM = Page.PortalContext.DataContract.GetValueByName<string>("PopupDM");
                var selectionName = Page.PortalContext.DataContract.GetValueByName<string>("SelectionNameDM");
                if (addDM != null && addDM == "Edit")
                {
                    _txtSelectionName.Data = selectionName;
                    _SetupB_LoadObject();
                    _btnClear.Visible = false;
                    this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "Reset").First().IsHidden = true;
                }

                if (addDM != null && addDM == "View")
                {
                    _txtSelectionName.Data = selectionName;
                    _SetupB_LoadObject();
                    _btnClear.Visible = false;
                    this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "Reset").First().IsHidden = true;
                    this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "Reload").First().IsHidden = true;
                    this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "Save").First().IsHidden = true;
                    this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "SaveAsNew").First().IsHidden = true;
                }
            }

           

            if (!_chkExistingInstance.IsChecked && !Page.IsAJAXFloatingFrame)
            {
                Page.DataContract.SetValueByName("InstanceName", null);
                Page.DataContract.SetValueByName("InstanceId", null);
                _txtSelectionInstanceId.ClearData();
                _txtSelectionName.ClearData();
            }

            //comment out this code to maintain focus after postback
            //if (SEMI.AppCode.UIUtility.IsPopupClose(this))
            //    Page.SetFocus(_txtSelectionName);
        }

        //--------------------------------------
        //
        //--------------------------------------
        ResponseData _gridSelection_RowSelected(object sender, JQGridEventArgs args)
        {
            string sRowID = "";
            string sNameColumn = "Name";
            if ((sender as Camstar.WebPortal.FormsFramework.WebGridControls.JQDataGrid).SelectedRowID != null)
                sRowID = (sender as Camstar.WebPortal.FormsFramework.WebGridControls.JQDataGrid).SelectedRowID;

            if (!string.IsNullOrEmpty(sRowID))
            {
                // additional code to handle column name case sensitivity
                foreach (JQField oField in (_gridSelection.GridContext as BoundContext).Fields)
                {
                    if (oField.ID.ToUpper() == "NAME")
                        sNameColumn = oField.ID;
                }

                _txtSelectionName.Data = (_gridSelection.GridContext as BoundContext).GetCell(sRowID, sNameColumn);
                _SetupB_LoadObject();
            }

            return args.Response;
        }

        //--------------------------------------
        //
        //--------------------------------------
        private void ResetBasePageControls()
        {
            EnableObjectButtons(false);

            _txtGridPage.Data = 1;
            _txtSelectionName.ClearData();
            _txtSelectionInstanceId.ClearData();
            _txtSelectionName.Data = null;
            _txtSelectionInstanceId.Data = null;
            _chkExistingInstance.CheckControl.Checked = false;

            _btnSelectionNext.Enabled = false;
            _btnSelectionPrev.Enabled = false;
            Page.DisplayMessage("", true);
        } // ResetBasePageControls

        //--------------------------------------
        //
        //--------------------------------------
        private void EnableObjectButtons(bool Enabled)
        {
            if (Page.IsAJAXFloatingFrame)
            {
                _btnSaveAsNew.Visible = Enabled;
                _btnDelete.Visible = false;
                _btnReload.Visible = Enabled;
                _btnViewAudit.Visible = false;
                this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "SaveAsNew").First().IsHidden = !Enabled;
                this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "Reload").First().IsHidden = !Enabled;
            }
            else
            {
                _btnSaveAsNew.Enabled = Enabled;
                _btnDelete.Enabled = Enabled;
                _btnReload.Enabled = Enabled;
                _btnViewAudit.Enabled = Enabled;
            }
        } // EnableObjectButtons

        //--------------------------------------
        //
        //--------------------------------------
        private void _SetupB_SelectionRefresh()
        {
            _SetupB_FetchSelection("REFRESH");
        } // _SetupB_SelectionRefresh

        //--------------------------------------
        //
        //--------------------------------------
        private void _SetupB_SelectionNext()
        {
            _SetupB_FetchSelection("NEXT");
        } // _SetupB_SelectionNext

        //--------------------------------------
        //
        //--------------------------------------
        private void _SetupB_SelectionPrev()
        {
            _SetupB_FetchSelection("PREVIOUS");
        } // _SetupB_SelectionPrev

        //--------------------------------------
        //
        //--------------------------------------
        private void _SetupB_FetchSelection(string FetchType)
        {
            string sServiceType = Page.PrimaryServiceType;
            ResultStatus oResultStatus = new ResultStatus();
            Result oResult = new Result();

            var fs = FrameworkManagerUtil.GetFrameworkSession();
            var oServiceData = WCFObject.CreateObject(sServiceType) as ICreator;
            var oServiceInfo = WCFObject.CreateObject(sServiceType + "_Info") as ICreator;
            var oObjectChanges = WCFObject.CreateObject(sServiceType.Replace("Maint", "") + "Changes") as ICreator;
            var oObjectChangesInfo = WCFObject.CreateObject(sServiceType.Replace("Maint", "") + "Changes_Info") as ICreator;
            var oServiceRequest = WCFObject.CreateObject(sServiceType + "_Request") as ICreator;
            var oService = new WSDataCreator().CreateService(sServiceType, fs.CurrentUserProfile);

            int iPageBlock = 1;
            if (_txtGridPage.Data != null)
                iPageBlock = int.Parse(_txtGridPage.Data.ToString());

            if (FetchType == "PREVIOUS")
            {
                if (iPageBlock > 1)
                    iPageBlock--;
            }
            else if (FetchType == "NEXT")
                iPageBlock++;

            // add the input data from the inheriting class's _SelectionControls
            for (int x = 0; x < _SelectionControls.Count; x++)
            {
                string sControlID = _SelectionControls[x].Control.ID;
                string sControlType = _SelectionControls[x].Control.GetType().Name;

                string sFieldExpression = "";
                string sFieldToAdd = "";

                switch (sControlType)
                {
                    case "NamedObject":
                        CWC.NamedObject ndoControl = Page.FindCamstarControl(sControlID) as CWC.NamedObject;
                        if (ndoControl.FieldExpressions != null)
                        {
                            sFieldExpression = ndoControl.FieldExpressions;
                            if (!string.IsNullOrEmpty(sFieldExpression))
                            {
                                sFieldToAdd = GetFieldFromFieldExpression(sFieldExpression);

                                if (ndoControl.Data != null)
                                    oObjectChanges.SetValue(sFieldToAdd, ndoControl.Data as NamedObjectRef);
                                else
                                    oObjectChanges.SetValue(sFieldToAdd, new NamedObjectRef());
                            }
                        }
                        break;
                    case "RevisionedObject":
                        CWC.RevisionedObject rdoControl = Page.FindCamstarControl(sControlID) as CWC.RevisionedObject;
                        if (rdoControl.FieldExpressions != null)
                        {
                            sFieldExpression = rdoControl.FieldExpressions;
                            if (!string.IsNullOrEmpty(sFieldExpression))
                            {
                                sFieldToAdd = GetFieldFromFieldExpression(sFieldExpression);

                                if (rdoControl.Data != null)
                                    oObjectChanges.SetValue(sFieldToAdd, rdoControl.Data as RevisionedObjectRef);
                                else
                                    oObjectChanges.SetValue(sFieldToAdd, new RevisionedObjectRef());
                            }
                        }
                        break;

                    case "TextBox":
                        CWC.TextBox txtControl = Page.FindCamstarControl(sControlID) as CWC.TextBox;
                        if (txtControl.FieldExpressions != null)
                        {
                            sFieldExpression = txtControl.FieldExpressions;
                            if (!string.IsNullOrEmpty(sFieldExpression))
                            {
                                sFieldToAdd = GetFieldFromFieldExpression(sFieldExpression);

                                if (txtControl.Data != null)
                                    oObjectChanges.SetValue(sFieldToAdd, txtControl.Data.ToString());
                                else
                                    oObjectChanges.SetValue(sFieldToAdd, "");
                            }
                        }
                        break;

                    case "CheckBox":
                        CWC.CheckBox chkControl = Page.FindCamstarControl(sControlID) as CWC.CheckBox;
                        if (chkControl.FieldExpressions != null)
                        {
                            sFieldExpression = chkControl.FieldExpressions;
                            if (!string.IsNullOrEmpty(sFieldExpression))
                            {
                                sFieldToAdd = GetFieldFromFieldExpression(sFieldExpression);

                                if (chkControl.CheckControl.Checked)
                                    oObjectChanges.SetValue(sFieldToAdd, true);
                                else
                                    oObjectChanges.SetValue(sFieldToAdd, false);
                            }
                        }
                        break;

                    case "DropDownList":
                        CWC.DropDownList ddlControl = Page.FindCamstarControl(sControlID) as CWC.DropDownList;
                        if (ddlControl.FieldExpressions != null)
                        {
                            sFieldExpression = ddlControl.FieldExpressions;
                            if (!string.IsNullOrEmpty(sFieldExpression))
                            {
                                sFieldToAdd = GetFieldFromFieldExpression(sFieldExpression);

                                if (ddlControl.Data != null)
                                    oObjectChanges.SetValue(sFieldToAdd, ddlControl.Data);
                                else
                                    oObjectChanges.SetValue(sFieldToAdd, null);
                            }
                        }
                        break;

                    case "DateChooser":
                        CWC.DateChooser dateControl = Page.FindCamstarControl(sControlID) as CWC.DateChooser;

                        if (dateControl.FieldExpressions != null)
                        {
                            sFieldExpression = dateControl.FieldExpressions;
                            if (!string.IsNullOrEmpty(sFieldExpression))
                            {
                                sFieldToAdd = GetFieldFromFieldExpression(sFieldExpression);

                                if (dateControl.Data != null)
                                    oObjectChanges.SetValue(sFieldToAdd, dateControl.Data);
                                else
                                    oObjectChanges.SetValue(sFieldToAdd, null);
                            }
                        }
                        break;

                    case "WorkflowNavigator":
                        CWC.WorkflowNavigator WfNavigatorControl = Page.FindCamstarControl(sControlID) as CWC.WorkflowNavigator;
                        var stackControl = Page.FindCamstarControl(WfNavigatorControl.ClientID + "_Stack") as FieldControl;

                        string sWfExp = "";
                        string sWfField = "";

                        string sWfStepExp = "";
                        string sWfStepField = "";

                        string sWfStackExp = "";
                        string sWfStackField = "";

                        if (WfNavigatorControl.FieldExpressions != null)
                        {
                            sWfExp = WfNavigatorControl.FieldExpressions;
                            sWfStepExp = WfNavigatorControl.ToStepValuesExpressions;
                            sWfStackExp = WfNavigatorControl.StackFieldExpressions;
                            if (!string.IsNullOrEmpty(sWfExp) || !string.IsNullOrEmpty(sWfStepExp))
                            {
                                sWfField = GetFieldFromFieldExpression(sWfExp);
                                sWfStepField = GetFieldFromFieldExpression(sWfStepExp);
                                sWfStackField = GetFieldFromFieldExpression(sWfStackExp);

                                string sStepPath = "ObjectChanges." + sWfStepField;
                                string sParent = "ObjectChanges." + sWfField;
                                string sStack = "ObjectChanges." + sWfStackField;

                                if (WfNavigatorControl.StepControl.Data != null)
                                {
                                    NamedSubentityRef WIPStep = new NamedSubentityRef() { Name = (WfNavigatorControl.StepControl.Data as NamedSubentityRef).Name, Parent = WfNavigatorControl.Data as RevisionedObjectRef };
                                    oServiceData.SetValue(sStepPath, WIPStep);
                                    oServiceData.SetValue(sParent, WIPStep.Parent);
                                    if (stackControl.Data != null)
                                    {
                                        NamedSubentityRef[] stack = (stackControl.Data) as NamedSubentityRef[];
                                        oServiceData.SetValue(sStack, stack);
                                    }
                                }
                                else
                                {
                                    oObjectChanges.SetValue(sWfStepField, null);
                                    oObjectChanges.SetValue(sWfField, null);
                                    oObjectChanges.SetValue(sWfStackField, null);
                                }
                            }
                        } // if (WfNavigatorControl.FieldExpressions != null)
                        break;
                }
            }

            oObjectChanges.SetValue("STARTROWNUM", (_iBlockOfRows * (iPageBlock - 1)) + 1);
            oObjectChanges.SetValue("STOPROWNUM", (_iBlockOfRows * iPageBlock) + 1);

            oServiceData.SetValue("ObjectChanges", oObjectChanges);

            oObjectChangesInfo.SetValue("Selection", new OM.Info(false, true));
            oServiceInfo.SetValue("ObjectChanges", oObjectChangesInfo);
            oServiceRequest.SetValue("Info", oServiceInfo);

            _btnSelectionPrev.Enabled = (iPageBlock > 1);
            _btnSelectionNext.Enabled = false;

            oResultStatus = oService.GetEnvironment(oServiceData as DCObject, oServiceRequest as Request, out oResult);
            if (oResultStatus.IsSuccess)
            {
                OM.Environment oEnv = (oResult as ICreator).GetValue("Environment.ObjectChanges.Selection") as OM.Environment;
                if (oEnv.SelectionValues != null)
                {
                    RecordSet oData = oEnv.SelectionValues as RecordSet;
                    DataTable dtData = oData.GetAsExplicitlyDataTable();

                    // remove the last row if the total records exceed the blockOfRows
                    if (dtData.Rows.Count > _iBlockOfRows)
                        dtData.Rows.RemoveAt(dtData.Rows.Count - 1);

                    _gridSelection.ClearData();

                    // check if the data columns returned are uppercase (usually during Oracle)
                    // blanket assumption that if one is uppercase, all will be uppercase
                    string sDynamicTypeName = _dynamicTypeName;
                    foreach (DataColumn d in dtData.Columns)
                    {
                        string sColumnNameUpper = d.ColumnName.ToUpper();
                        if (sColumnNameUpper == d.ColumnName)
                        {
                            sDynamicTypeName = _dynamicTypeNameEx;
                            break;
                        }
                    }

                    if (oData != null)
                    {
                        JQDataGrid _gridSelectionTemp = Page.FindCamstarControl("SelectionGrid") as JQDataGrid;
                        int iHiddenColumns = _HiddenGridColumns.Count;
                        string[] sHiddenColumns = new string[iHiddenColumns];
                        int iIndex = 0;
                        foreach (string sColumnName in _HiddenGridColumns)
                        {
                            sHiddenColumns[iIndex] = sColumnName;
                            iIndex++;
                        }

                        if (!bIsColumnsDefined)
                        {
                            SEMI.AppCode.GridUtility.ItemListGrid_SetColumns(wpSelectionWP, oData.GetAsExplicitlyDataTable(), _gridSelectionTemp.ID, null, sDynamicTypeName, true, sHiddenColumns, true, null, oData.Headers);
                            bIsColumnsDefined = true;
                        }
                        SEMI.AppCode.GridUtility.ItemListGrid_BindDataTable(wpSelectionWP, dtData, ref _gridSelectionTemp, sDynamicTypeName);

                        int iStartRowNum = (_iBlockOfRows * (iPageBlock - 1)) + 1;
                        _lblRows.LabelText = iStartRowNum.ToString() + " - " + (iStartRowNum + dtData.Rows.Count - 1).ToString();
                        if (oData != null)
                            if (oData.Rows != null)
                                _btnSelectionNext.Enabled = (oData.Rows.Count() > _iBlockOfRows);
                    }

                    // save the data recordset to view state
                    ViewState[_Data_ViewStateConstant] = oData.GetAsExplicitlyDataTable();
                }
            }

            // set the page block
            _txtGridPage.Data = iPageBlock.ToString();
        } // _SetupB_FetchSelection

        //--------------------------------------
        //
        //--------------------------------------
        private RecordSet _SetupB_FetchSelectionSpecific()
        {
            string sServiceType = Page.PrimaryServiceType;
            RecordSet rsReturn = null;
            ResultStatus oResultStatus = new ResultStatus();
            Result oResult = new Result();

            var fs = FrameworkManagerUtil.GetFrameworkSession();
            var oServiceData = WCFObject.CreateObject(sServiceType) as ICreator;
            var oServiceInfo = WCFObject.CreateObject(sServiceType + "_Info") as ICreator;
            var oObjectChanges = WCFObject.CreateObject(sServiceType.Replace("Maint", "") + "Changes") as ICreator;
            var oObjectChangesInfo = WCFObject.CreateObject(sServiceType.Replace("Maint", "") + "Changes_Info") as ICreator;
            var oServiceRequest = WCFObject.CreateObject(sServiceType + "_Request") as ICreator;

            var oService = new WSDataCreator().CreateService(sServiceType, fs.CurrentUserProfile);

            // add the input data from the inheriting class's _CriteriaControls
            for (int x = 0; x < _CriteriaControls.Count; x++)
            {
                string sControlID = _CriteriaControls[x].Control.ID;
                string sControlType = _CriteriaControls[x].Control.GetType().Name;

                string sFieldExpression = "";
                string sFieldToAdd = "";

                switch (sControlType)
                {
                    case "NamedObject":
                        CWC.NamedObject ndoControl = Page.FindCamstarControl(sControlID) as CWC.NamedObject;
                        if (ndoControl.FieldExpressions != null)
                        {
                            sFieldExpression = ndoControl.FieldExpressions;
                            if (!string.IsNullOrEmpty(sFieldExpression))
                            {
                                sFieldToAdd = GetFieldFromFieldExpression(sFieldExpression);

                                if (ndoControl.Data != null)
                                    oObjectChanges.SetValue(sFieldToAdd, ndoControl.Data as NamedObjectRef);
                                else
                                    oObjectChanges.SetValue(sFieldToAdd, new NamedObjectRef());
                            }
                        }
                        break;

                    case "RevisionedObject":
                        CWC.RevisionedObject rdoControl = Page.FindCamstarControl(sControlID) as CWC.RevisionedObject;
                        if (rdoControl.FieldExpressions != null)
                        {
                            sFieldExpression = rdoControl.FieldExpressions;
                            if (!string.IsNullOrEmpty(sFieldExpression))
                            {
                                sFieldToAdd = GetFieldFromFieldExpression(sFieldExpression);

                                if (rdoControl.Data != null)
                                    oObjectChanges.SetValue(sFieldToAdd, rdoControl.Data as RevisionedObjectRef);
                                else
                                    oObjectChanges.SetValue(sFieldToAdd, new RevisionedObjectRef());
                            }
                        }
                        break;

                    case "TextBox":
                        CWC.TextBox txtControl = Page.FindCamstarControl(sControlID) as CWC.TextBox;
                        if (txtControl.FieldExpressions != null)
                        {
                            sFieldExpression = txtControl.FieldExpressions;
                            if (!string.IsNullOrEmpty(sFieldExpression))
                            {
                                sFieldToAdd = GetFieldFromFieldExpression(sFieldExpression);

                                if (txtControl.Data != null)
                                    oObjectChanges.SetValue(sFieldToAdd, txtControl.Data.ToString());
                                else
                                    oObjectChanges.SetValue(sFieldToAdd, "");
                            }
                        }
                        break;
                    case "CheckBox":
                        CWC.CheckBox chkControl = Page.FindCamstarControl(sControlID) as CWC.CheckBox;
                        if (chkControl.FieldExpressions != null)
                        {
                            sFieldExpression = chkControl.FieldExpressions;
                            if (!string.IsNullOrEmpty(sFieldExpression))
                            {
                                sFieldToAdd = GetFieldFromFieldExpression(sFieldExpression);

                                if (chkControl.CheckControl.Checked)
                                    oObjectChanges.SetValue(sFieldToAdd, true);
                                else
                                    oObjectChanges.SetValue(sFieldToAdd, false);
                            }
                        }
                        break;

                    case "DropDownList":
                        CWC.DropDownList ddlControl = Page.FindCamstarControl(sControlID) as CWC.DropDownList;
                        if (ddlControl.FieldExpressions != null)
                        {
                            sFieldExpression = ddlControl.FieldExpressions;
                            if (!string.IsNullOrEmpty(sFieldExpression))
                            {
                                sFieldToAdd = GetFieldFromFieldExpression(sFieldExpression);

                                if (ddlControl.Data != null)
                                    oObjectChanges.SetValue(sFieldToAdd, ddlControl.Data);
                                else
                                    oObjectChanges.SetValue(sFieldToAdd, null);
                            }
                        }
                        break;

                    case "DateChooser":
                        CWC.DateChooser dateControl = Page.FindCamstarControl(sControlID) as CWC.DateChooser;

                        if (dateControl.FieldExpressions != null)
                        {
                            sFieldExpression = dateControl.FieldExpressions;
                            if (!string.IsNullOrEmpty(sFieldExpression))
                            {
                                sFieldToAdd = GetFieldFromFieldExpression(sFieldExpression);

                                if (dateControl.Data != null)
                                    oObjectChanges.SetValue(sFieldToAdd, dateControl.Data);
                                else
                                    oObjectChanges.SetValue(sFieldToAdd, null);
                            }
                        }
                        break;

                    case "WorkflowNavigator":
                        CWC.WorkflowNavigator WfNavigatorControl = Page.FindCamstarControl(sControlID) as CWC.WorkflowNavigator;
                        var stackControl = Page.FindCamstarControl(WfNavigatorControl.ClientID + "_Stack") as FieldControl;

                        string sWfExp = "";
                        string sWfField = "";

                        string sWfStepExp = "";
                        string sWfStepField = "";

                        string sWfStackExp = "";
                        string sWfStackField = "";

                        if (WfNavigatorControl.FieldExpressions != null)
                        {
                            sWfExp = WfNavigatorControl.FieldExpressions;
                            sWfStepExp = WfNavigatorControl.ToStepValuesExpressions;
                            sWfStackExp = WfNavigatorControl.StackFieldExpressions;
                            if (!string.IsNullOrEmpty(sWfExp) || !string.IsNullOrEmpty(sWfStepExp))
                            {
                                sWfField = GetFieldFromFieldExpression(sWfExp);
                                sWfStepField = GetFieldFromFieldExpression(sWfStepExp);
                                sWfStackField = GetFieldFromFieldExpression(sWfStackExp);

                                if (WfNavigatorControl.Data != null && WfNavigatorControl.StepControl.Data != null)
                                {
                                    NamedSubentityRef WIPStep = new NamedSubentityRef() { Name = (WfNavigatorControl.StepControl.Data as NamedSubentityRef).Name, Parent = WfNavigatorControl.Data as RevisionedObjectRef };
                                    oObjectChanges.SetValue(sWfStepField, WIPStep);
                                    oObjectChanges.SetValue(sWfField, WIPStep.Parent);

                                    NamedSubentityRef[] oStackData = null;
                                    ////if (stackControl.Data != null)                                    
                                    ////     oStackData  = (stackControl.Data) as NamedSubentityRef[];                                                                      
                                    ////else                                       
                                    oStackData = StackData_Get(sWfStackField);

                                    oObjectChanges.SetValue(sWfStackField, oStackData);
                                }
                                else
                                {
                                    oObjectChanges.SetValue(sWfField, null);
                                    oObjectChanges.SetValue(sWfStepField, null);
                                    oObjectChanges.SetValue(sWfStackField, null);
                                }
                            }
                        } // if (WfNavigatorControl.FieldExpressions != null)
                        break;
                }
            }

            oServiceData.SetValue("ObjectChanges", oObjectChanges);

            oObjectChangesInfo.SetValue("SelectionSpecific", new OM.Info(false, true));
            oServiceInfo.SetValue("ObjectChanges", oObjectChangesInfo);
            oServiceRequest.SetValue("Info", oServiceInfo);

            oResultStatus = oService.GetEnvironment(oServiceData as DCObject, oServiceRequest as Request, out oResult);
            if (oResultStatus.IsSuccess)
            {
                OM.Environment oEnv = (oResult as ICreator).GetValue("Environment.ObjectChanges.SelectionSpecific") as OM.Environment;
                RecordSet oData = oEnv.SelectionValues as RecordSet;
                DataTable dtData = oData.GetAsExplicitlyDataTable();

                rsReturn = oData;

            }

            return rsReturn;
        } // _SetupB_FetchSelectionSpecific

        //--------------------------------------
        //
        //--------------------------------------
        public virtual void _SetupB_LoadObject()
        {
            if (_txtSelectionName.Data != null)
            {
                string sService = Page.PrimaryServiceType;
                string sSelectionName = _txtSelectionName.Data.ToString();
                htControlState = new Hashtable();
                htControlState = SaveControlData(_SelectionControls);

                System.Collections.Hashtable htLocalSessions = Page.PortalContext.LocalSession;
                UIComponentDataContract dcPageContracts = Page.PortalContext.DataContract;

                Page.PortalContext = new MaintenanceBehaviorContext();
                var pc = Page.PortalContext as MaintenanceBehaviorContext;
                pc.DataContract = dcPageContracts;
                pc.LocalSession = htLocalSessions;

                pc.Current = new OM.NamedObjectRef(_txtSelectionName.Data.ToString());
                if (!string.IsNullOrEmpty(pc.CDOTypeName))
                    pc.Current.CDOTypeName = pc.CDOTypeName;
                else
                {
                    pc.CDOTypeName = sService.Replace("Maint", "");
                    pc.Current.CDOTypeName = sService.Replace("Maint", "");
                }

                //set any Selection WfNavigator child controls dataSubmissionMode to Skip
                SelectionWfNavigators_SetSubmissionMode();

                Page.ShopfloorReset(null, null);
                _txtSelectionName.Data = sSelectionName;
                Page.LoadModelingValues(true);
                _txtSelectionName.Data = sSelectionName;
                RetrieveInstanceId(sSelectionName);
                RestoreControlData(_SelectionControls, htControlState);
                bool bEnableButtons = !(string.IsNullOrEmpty(_txtSelectionName.Data.ToString()));
                EnableObjectButtons(bEnableButtons);
            }
        } // _SetupB_LoadObject         

        //--------------------------------------
        // function not used as it seems to be returning some bogus InstanceId
        //--------------------------------------
        private void _SetupB_LoadInstanceId()
        {
            string sSelectionName = _txtSelectionName.Data.ToString();
            string sSelectionInstanceId = "";
            if (_txtSelectionName.Data != null)
            {
                //------- manual load begin-------
                string sServiceType = Page.PrimaryServiceType;

                ResultStatus oResultStatus = new ResultStatus();
                Result oResult = new Result();

                var fs = FrameworkManagerUtil.GetFrameworkSession();
                var oServiceData = WCFObject.CreateObject(sServiceType) as ICreator;
                var oServiceInfo = WCFObject.CreateObject(sServiceType + "_Info") as ICreator;
                var oObjectChanges = WCFObject.CreateObject(sServiceType.Replace("Maint", "") + "Changes") as ICreator;
                var oObjectChangesInfo = WCFObject.CreateObject(sServiceType.Replace("Maint", "") + "Changes_Info") as ICreator;
                var oServiceRequest = WCFObject.CreateObject(sServiceType + "_Request") as ICreator;

                var oService = new WSDataCreator().CreateService(sServiceType, fs.CurrentUserProfile);

                if (_txtSelectionName.Data != null)
                {
                    oServiceData.SetValue("ObjectToChange", new NamedObjectRef(_txtSelectionName.Data.ToString()));
                    oObjectChangesInfo.SetValue("Name", new OM.Info(true));
                    oServiceInfo.SetValue("ObjectChanges", oObjectChangesInfo);
                    oServiceRequest.SetValue("Info", oServiceInfo);

                    oResultStatus = (oService as INamedDataObjectMaintBase).Load(oServiceData as DCObject, oServiceRequest as Request, out oResult);

                    if (oResultStatus.IsSuccess)
                    {
                        sSelectionInstanceId = ((oResult.Value) as NamedDataObjectMaint).ObjectChanges.Self.ID.ToString();
                    }
                }

                //------- manual load end----------                
                _txtSelectionInstanceId.Data = sSelectionInstanceId;
            }
        } // _SetupB_LoadObjectValues        

        //--------------------------------------
        //
        //--------------------------------------
        private ResultStatus _SetupB_UpdateObject()
        {
            string sServiceType = Page.PrimaryServiceType;

            ResultStatus oResultStatus = new ResultStatus();
            Result oResponseData = new Result();

            var fs = FrameworkManagerUtil.GetFrameworkSession();
            var oServiceData = WCFObject.CreateObject(sServiceType) as ICreator;
            var oServiceData2 = WCFObject.CreateObject(sServiceType) as ICreator;
            var oServiceInfo = WCFObject.CreateObject(sServiceType + "_Info") as ICreator;
            var oObjectChanges = WCFObject.CreateObject(sServiceType.Replace("Maint", "") + "Changes") as ICreator;
            var oObjectChangesInfo = WCFObject.CreateObject(sServiceType.Replace("Maint", "") + "Changes_Info") as ICreator;
            var oServiceRequest = WCFObject.CreateObject(sServiceType + "_Request") as ICreator;

            var oService = new WSDataCreator().CreateService(sServiceType, fs.CurrentUserProfile);

            if (_txtSelectionName.Data != null)
            {
                //set any Selection WfNavigator child controls dataSubmissionMode to Skip
                SelectionWfNavigators_SetSubmissionMode();

                Page.GetInputData(oServiceData2 as Service);
                var validateresult = Page.ValidateInputData();
                if (validateresult.IsSuccess)
                {
                    (oService as INamedDataObjectMaintBase).BeginTransaction();
                    oServiceData.SetValue("ObjectToChange", new NamedObjectRef(_txtSelectionName.Data.ToString()));
                    (oService as INamedDataObjectMaintBase).Load(oServiceData as DCObject);

                    List<CWC.NamedObject> _ndoControls = Page.FindCamstarControls<CWC.NamedObject>();
                    List<CWC.TextBox> _txtControls = Page.FindCamstarControls<CWC.TextBox>();
                    List<CWC.CheckBox> _chkControls = Page.FindCamstarControls<CWC.CheckBox>();
                    //List<CWC.DateChooser> _dateControls = Page.FindCamstarControl<CWC.DateChooser>();
                    List<CWC.DropDownList> _ddlControls = Page.FindCamstarControls<CWC.DropDownList>();

                    for (int x = 0; x < _ndoControls.Count; x++)
                    {
                        if (_ndoControls[x].DataSubmissionMode != DataSubmissionModeType.Skip
                            && _ndoControls[x].Data == null
                            && _ndoControls[x].FieldExpressions.Trim() != "")
                        {
                            string sControlType = _ndoControls[x].Control.GetType().Name;
                            switch (sControlType)
                            {
                                case "NamedObject":
                                    oServiceData2.SetValue(_ndoControls[x].FieldExpressions.Substring(1), new NamedObjectRef(""));
                                    break;
                                case "RevisionedObject":
                                    //oServiceData2.SetValue(_ndoControls[x].FieldExpressions.Substring(1), new RevisionedObjectRef(""));
                                    RevisionedObjectRef oRevObj = new RevisionedObjectRef("");
                                    oRevObj.RevisionOfRecord = false;

                                    oServiceData2.SetValue(_ndoControls[x].FieldExpressions.Substring(1), oRevObj);
                                    break;
                            }
                        }
                    } // for (int x = 0; x < _ndoControls.Count; x++)

                    for (int x = 0; x < _txtControls.Count; x++)
                    {
                        if (_txtControls[x].DataSubmissionMode != DataSubmissionModeType.Skip && _txtControls[x].Data == null && _txtControls[x].FieldExpressions.Trim() != "")
                            try { oServiceData2.SetValue(_txtControls[x].FieldExpressions.Substring(1), ""); }
                            catch { oServiceData2.SetValue(_txtControls[x].FieldExpressions.Substring(1), "0"); }

                    } // (int x = 0; x < _txtControls.Count; x++)

                    for (int x = 0; x < _chkControls.Count; x++)
                    {
                        if (_chkControls[x].DataSubmissionMode != DataSubmissionModeType.Skip && _chkControls[x].Data == null && _chkControls[x].FieldExpressions.Trim() != "")
                            oServiceData2.SetValue(_chkControls[x].FieldExpressions.Substring(1), false);
                    } // for (int x = 0; x < _chkControls.Count; x++)

                    for (int x = 0; x < _ddlControls.Count; x++)
                    {
                        if (_ddlControls[x].DataSubmissionMode != DataSubmissionModeType.Skip && _ddlControls[x].Data == null && _ddlControls[x].FieldExpressions.Trim() != "")
                        {
                            try { oServiceData2.SetValue(_ddlControls[x].FieldExpressions.Substring(1), 0); }
                            catch { oServiceData2.SetValue(_ddlControls[x].FieldExpressions.Substring(1), ""); }
                        }

                    } // for (int x = 0; x < _ddlControls.Count; x++)

                    for (int x = 0; x < _CriteriaControls.Count; x++)
                    {
                        string sControlID = _CriteriaControls[x].Control.ID;
                        string sControlType = _CriteriaControls[x].Control.GetType().Name;

                        if (sControlType == "WorkflowNavigator")
                        {
                            CWC.WorkflowNavigator WfNavigatorControl = Page.FindCamstarControl(sControlID) as CWC.WorkflowNavigator;
                            var stackControl = Page.FindCamstarControl(WfNavigatorControl.ClientID + "_Stack") as FieldControl;

                            string sWfStepExp = "";
                            string sWfStepField = "";

                            string sWfStackExp = "";
                            string sWfStackField = "";

                            if (WfNavigatorControl.FieldExpressions != null)
                            {
                                sWfStepExp = WfNavigatorControl.ToStepValuesExpressions;
                                sWfStackExp = WfNavigatorControl.StackFieldExpressions;

                                if (!string.IsNullOrEmpty(sWfStepExp))
                                {
                                    sWfStepField = GetFieldFromFieldExpression(sWfStepExp);
                                    sWfStackField = GetFieldFromFieldExpression(sWfStackExp);

                                    string sStepPath = "ObjectChanges." + sWfStepField;
                                    string sStackPath = "ObjectChanges." + sWfStackField;

                                    if ((WfNavigatorControl.StepControl.Data != null) && (WfNavigatorControl.Data != null))
                                    {
                                        NamedSubentityRef WIPStep = new NamedSubentityRef() { Name = (WfNavigatorControl.StepControl.Data as NamedSubentityRef).Name, Parent = WfNavigatorControl.Data as RevisionedObjectRef };
                                        oServiceData2.ReplaceValue(sStepPath, WIPStep);
                                        // manually set the stack data from the hashtable
                                        NamedSubentityRef[] oStackData = StackData_Get(sWfStackField);
                                        oServiceData2.ReplaceValue(sStackPath, oStackData);
                                    }
                                    else
                                    {
                                        oServiceData2.ReplaceValue(sStepPath, null);
                                        oServiceData2.ReplaceValue(sStackPath, null);
                                        oServiceData2.SetValue(WfNavigatorControl.FieldExpressions.Substring(1), new RevisionedObjectRef(""));
                                        oServiceData2.SetValue(sStepPath, new NamedSubentityRef(""));
                                    }

                                }
                            } // if (WfNavigatorControl.FieldExpressions != null)
                        }
                    }
                    (oService as INamedDataObjectMaintBase).ExecuteTransaction(oServiceData2 as DCObject);
                    oResultStatus = (oService as INamedDataObjectMaintBase).CommitTransaction();
                }
                else
                {
                    Page.DisplayMessage(validateresult);
                }
            }

            return oResultStatus;
        } // _SetupB_UpdateObject

        //--------------------------------------
        //
        //--------------------------------------
        private ResultStatus _SetupB_NewObject(bool IsSaveAsNew = false)
        {
            string sServiceType = Page.PrimaryServiceType;

            ResultStatus oResultStatus = new ResultStatus();
            Result oResponseData = new Result();

            var fs = FrameworkManagerUtil.GetFrameworkSession();
            var oServiceData = WCFObject.CreateObject(sServiceType) as ICreator;
            var oServiceInfo = WCFObject.CreateObject(sServiceType + "_Info") as ICreator;
            var oObjectChanges = WCFObject.CreateObject(sServiceType.Replace("Maint", "") + "Changes") as ICreator;
            var oObjectChangesInfo = WCFObject.CreateObject(sServiceType.Replace("Maint", "") + "Changes_Info") as ICreator;
            var oServiceRequest = WCFObject.CreateObject(sServiceType + "_Request") as ICreator;

            var oService = new WSDataCreator().CreateService(sServiceType, fs.CurrentUserProfile);

            // for each subentity grid
            for (int x = 0; x < _SubentityGridControls.Count; x++)
            {
                string sControlID = _SubentityGridControls[x].Control.ID;

                // nullify the ObjectToChange, ListItemAction and ListItemIndex to trick the grid into thinking its new rows.
                JQDataGrid _gridControl = Page.FindCamstarControl(sControlID) as JQDataGrid;
                OM.SubentityChanges[] oDetails = _gridControl.Data as OM.SubentityChanges[];
                if (oDetails != null)
                {
                    foreach (OM.SubentityChanges oDetail in oDetails)
                    {
                        oDetail.ObjectToChange = null;
                        oDetail.ListItemAction = null;
                        oDetail.ListItemIndex = null;
                    }
                }

                // set the data and nullify original data
                _gridControl.Data = oDetails;
                _gridControl.OriginalData = null;
                _gridControl.DataBind();
            }

            //set any Selection WfNavigator child controls dataSubmissionMode to Skip
            SelectionWfNavigators_SetSubmissionMode();

            Page.GetInputData(oServiceData as Service);

            // manually add the step data from the inheriting class's _Criteria Controls
            for (int x = 0; x < _CriteriaControls.Count; x++)
            {
                string sControlID = _CriteriaControls[x].Control.ID;
                string sControlType = _CriteriaControls[x].Control.GetType().Name;

                if (sControlType == "WorkflowNavigator")
                {
                    CWC.WorkflowNavigator WfNavigatorControl = Page.FindCamstarControl(sControlID) as CWC.WorkflowNavigator;


                    string sWfStepExp = "";
                    string sWfStepField = "";

                    if (WfNavigatorControl.FieldExpressions != null)
                    {
                        sWfStepExp = WfNavigatorControl.ToStepValuesExpressions;

                        if (!string.IsNullOrEmpty(sWfStepExp))
                        {
                            sWfStepField = GetFieldFromFieldExpression(sWfStepExp);
                            string sStepPath = "ObjectChanges." + sWfStepField;

                            if (WfNavigatorControl.StepControl.Data != null)
                            {
                                NamedSubentityRef WIPStep = new NamedSubentityRef() { Name = (WfNavigatorControl.StepControl.Data as NamedSubentityRef).Name, Parent = WfNavigatorControl.Data as RevisionedObjectRef };
                                oServiceData.ReplaceValue(sStepPath, WIPStep);

                                if (IsSaveAsNew)
                                {
                                    var stackControl = Page.FindCamstarControl(WfNavigatorControl.ClientID + "_Stack") as FieldControl;
                                    string sWfStackExp = "";
                                    string sWfStackField = "";
                                    sWfStackExp = WfNavigatorControl.StackFieldExpressions;
                                    sWfStackField = GetFieldFromFieldExpression(sWfStackExp);
                                    string sStackPath = "ObjectChanges." + sWfStackField;
                                    NamedSubentityRef[] oStackData = StackData_Get(sWfStackField);
                                    oServiceData.ReplaceValue(sStackPath, oStackData);
                                }
                            }
                            else
                                oObjectChanges.SetValue(sWfStepField, null);
                        }
                    } // if (WfNavigatorControl.FieldExpressions != null)
                }
            }

            // set the scsIsNDOMatrix field to trigger the validation check
            oServiceData.SetValue("ObjectChanges.scsIsNDOMatrix", true);

            var validateresult = Page.ValidateInputData();
            if (validateresult.IsSuccess)
            {
                (oService as INamedDataObjectMaintBase).BeginTransaction();
                (oService as INamedDataObjectMaintBase).New(oServiceData as DCObject);
                (oService as INamedDataObjectMaintBase).ExecuteTransaction();
                oResultStatus = (oService as INamedDataObjectMaintBase).CommitTransaction();
            }
            else
            {
                Page.DisplayMessage(validateresult);
            }

            return oResultStatus;
        } // _SetupB_NewObject

        //--------------------------------------
        //
        //--------------------------------------
        private ResultStatus _SetupB_DeleteObject()
        {
            string sServiceType = Page.PrimaryServiceType;

            ResultStatus oResultStatus = new ResultStatus();
            Result oResponseData = new Result();

            var fs = FrameworkManagerUtil.GetFrameworkSession();
            var oServiceData = WCFObject.CreateObject(sServiceType) as ICreator;
            var oServiceInfo = WCFObject.CreateObject(sServiceType + "_Info") as ICreator;
            var oObjectChanges = WCFObject.CreateObject(sServiceType.Replace("Maint", "") + "Changes") as ICreator;
            var oObjectChangesInfo = WCFObject.CreateObject(sServiceType.Replace("Maint", "") + "Changes_Info") as ICreator;
            var oServiceRequest = WCFObject.CreateObject(sServiceType + "_Request") as ICreator;

            var oService = new WSDataCreator().CreateService(sServiceType, fs.CurrentUserProfile);

            Page.GetInputData(oServiceData as Service);

            if (_txtSelectionName.Data != null)
            {
                (oService as INamedDataObjectMaintBase).BeginTransaction();
                oServiceData.SetValue("ObjectToChange", new NamedObjectRef(_txtSelectionName.Data.ToString()));
                (oService as INamedDataObjectMaintBase).Delete(oServiceData as DCObject);
                (oService as INamedDataObjectMaintBase).ExecuteTransaction();
                oResultStatus = (oService as INamedDataObjectMaintBase).CommitTransaction();
            }
            return oResultStatus;
        } // _SetupB_DeleteObject

        //--------------------------------------
        //
        //--------------------------------------
        public override void WebPartCustomAction(object sender, CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as CustomAction;
            RecordSet rsSelectionSpecificData;
            if (action != null)
            {
                ResultStatus oResultStatus = new ResultStatus();
                bool bNewObject = false;
                switch (action.Parameters)
                {
                    case "Save":
                        if (_txtSelectionName.Data != null)
                            oResultStatus = _SetupB_UpdateObject();
                        else
                        {
                            rsSelectionSpecificData = _SetupB_FetchSelectionSpecific();
                            if (rsSelectionSpecificData == null || rsSelectionSpecificData.Rows == null)
                            {
                                oResultStatus = _SetupB_NewObject();
                                bNewObject = true;
                            }
                            else
                            {
                                _txtSelectionName.Data = rsSelectionSpecificData.GetAsExplicitlyDataTable().Rows[0]["Name"].ToString();
                                UpdateObjectChangesNameControl(rsSelectionSpecificData.GetAsExplicitlyDataTable().Rows[0]["Name"].ToString());
                                oResultStatus = _SetupB_UpdateObject();
                            }
                        }

                        if (oResultStatus.IsSuccess)
                        {
                            rsSelectionSpecificData = _SetupB_FetchSelectionSpecific();
                            UpdateSelectionData(rsSelectionSpecificData);

                            if (bNewObject || _txtObjectChangesName.Data == null)
                            {
                                _txtSelectionName.Data = rsSelectionSpecificData.GetAsExplicitlyDataTable().Rows[0]["Name"].ToString();
                                _txtObjectChangesName.Data = rsSelectionSpecificData.GetAsExplicitlyDataTable().Rows[0]["Name"].ToString();
                            }
                            else
                            {
                                _txtSelectionName.Data = _txtObjectChangesName.Data;
                            }

                            _SetupB_LoadObject();
                            EnableObjectButtons(true);

                            if (Page.IsAJAXFloatingFrame)
                            {
                                Page.CloseFloatingFrame(true);
                            }
                        }
                        break;
                    case "SaveAsNew":
                        oResultStatus = _SetupB_NewObject(true);
                        if (oResultStatus.IsSuccess)
                        {
                            _txtSelectionName.ClearData();
                            rsSelectionSpecificData = _SetupB_FetchSelectionSpecific();
                            UpdateSelectionData(rsSelectionSpecificData);

                            _txtSelectionName.Data = rsSelectionSpecificData.GetAsExplicitlyDataTable().Rows[0]["Name"].ToString();
                            _txtObjectChangesName.Data = rsSelectionSpecificData.GetAsExplicitlyDataTable().Rows[0]["Name"].ToString();
                            _SetupB_LoadObject();
                            EnableObjectButtons(true);

                            if (Page.IsAJAXFloatingFrame)
                            {
                                Page.CloseFloatingFrame(true);
                            }
                        }
                        break;
                    case "Reload":
                        _SetupB_LoadObject();
                        break;
                    case "Delete":
                        oResultStatus = _SetupB_DeleteObject();
                        _txtSelectionName.ClearData();
                        EnableObjectButtons(false);
                        htControlState = new Hashtable();
                        htControlState = SaveControlData(_SelectionControls);
                        Page.ShopfloorReset(null, null);
                        RestoreControlData(_SelectionControls, htControlState);
                        _SetupB_FetchSelection("REFRESH");
                        break;
                    case "ViewAudit":
                        break;
                    case "Clear":
                        DisableValueClearing(_SelectionControls);
                        Page.ShopfloorReset(null, null);
                        //_gridSelection.ClearData();
                        ViewState[_Data_ViewStateConstant] = null;
                        ResetBasePageControls();
                        _gridSelection.SelectedRowID = null;
                        break;
                    case "ClearSelection":
                        _gridSelection.ClearData();
                        _txtSelectionName.ClearData();
                        ViewState[_Data_ViewStateConstant] = null;
                        EnableObjectButtons(false);
                        ClearControlData(_SelectionControls);
                        _SetupB_SelectionRefresh();
                        break;
                    case "PageRefresh":
                        _SetupB_SelectionRefresh();
                        break;
                    case "PagePrev":
                        _SetupB_SelectionPrev();
                        break;
                    case "PageNext":
                        _SetupB_SelectionNext();
                        break;
                }

                e.Result = oResultStatus;
            }
        } // WebPartCustomAction

        //--------------------------------------
        //
        //--------------------------------------
        private Hashtable SaveControlData(CamstarControlsCollection ccCollection)
        {
            Hashtable htControlState = new Hashtable();
            for (int x = 0; x < ccCollection.Count; x++)
            {
                string sControlID = ccCollection[x].Control.ID;
                string sControlType = ccCollection[x].Control.GetType().Name;

                object oControlData = null;

                switch (sControlType)
                {
                    case "NamedObject":
                        CWC.NamedObject ndoControl = Page.FindCamstarControl(sControlID) as CWC.NamedObject;
                        oControlData = ndoControl.Data;
                        break;
                    case "RevisionedObject":
                        CWC.RevisionedObject rdoControl = Page.FindCamstarControl(sControlID) as CWC.RevisionedObject;
                        oControlData = rdoControl.Data;
                        break;
                    case "TextBox":
                        CWC.TextBox txtControl = Page.FindCamstarControl(sControlID) as CWC.TextBox;
                        oControlData = txtControl.Data;
                        break;
                    case "CheckBox":
                        CWC.CheckBox chkControl = Page.FindCamstarControl(sControlID) as CWC.CheckBox;
                        oControlData = chkControl.Data;
                        break;
                    case "DropDownList":
                        CWC.DropDownList ddlControl = Page.FindCamstarControl(sControlID) as CWC.DropDownList;
                        oControlData = ddlControl.Data;
                        break;
                    case "DateChooser":
                        CWC.DateChooser dateControl = Page.FindCamstarControl(sControlID) as CWC.DateChooser;
                        oControlData = dateControl.Data;
                        break;
                    case "WorkflowNavigator":
                        CWC.WorkflowNavigator WfNavigatorControl = Page.FindCamstarControl(sControlID) as CWC.WorkflowNavigator;                       
                        oControlData = WfNavigatorControl.Data;
                        break;
                }

                htControlState.Add(sControlID, oControlData);
            }
            return htControlState;
        } // SaveControlState

        //--------------------------------------
        //
        //--------------------------------------
        private void RestoreControlData(CamstarControlsCollection ccCollection, Hashtable htControlState)
        {
            for (int x = 0; x < ccCollection.Count; x++)
            {
                string sControlID = ccCollection[x].Control.ID;
                string sControlType = ccCollection[x].Control.GetType().Name;

                if (htControlState.ContainsKey(sControlID))
                {
                    object oControlData = htControlState[sControlID];

                    switch (sControlType)
                    {
                        case "NamedObject":
                            CWC.NamedObject ndoControl = Page.FindCamstarControl(sControlID) as CWC.NamedObject;
                            ndoControl.Data = oControlData;
                            break;
                        case "RevisionedObject":
                            CWC.RevisionedObject rdoControl = Page.FindCamstarControl(sControlID) as CWC.RevisionedObject;
                            rdoControl.Data = oControlData;
                            break;
                        case "TextBox":
                            CWC.TextBox txtControl = Page.FindCamstarControl(sControlID) as CWC.TextBox;
                            txtControl.Data = oControlData;
                            break;
                        case "CheckBox":
                            CWC.CheckBox chkControl = Page.FindCamstarControl(sControlID) as CWC.CheckBox;
                            chkControl.Data = oControlData;
                            break;
                        case "DropDownList":
                            CWC.DropDownList ddlControl = Page.FindCamstarControl(sControlID) as CWC.DropDownList;
                            ddlControl.Data = oControlData;
                            break;
                        case "DateChooser":
                            CWC.DateChooser dateControl = Page.FindCamstarControl(sControlID) as CWC.DateChooser;
                            oControlData = dateControl.Data;
                            break;
                        case "WorkflowNavigator":
                            CWC.WorkflowNavigator WfNavigatorControl = Page.FindCamstarControl(sControlID) as CWC.WorkflowNavigator;
                            WfNavigatorControl.Data = oControlData;
                            break;
                    }
                }
            }
        } // RestoreControlData

        //--------------------------------------
        //
        //--------------------------------------
        private void ClearControlData(CamstarControlsCollection ccCollection)
        {
            for (int x = 0; x < ccCollection.Count; x++)
            {
                string sControlID = ccCollection[x].Control.ID;
                string sControlType = ccCollection[x].Control.GetType().Name;

                switch (sControlType)
                {
                    case "NamedObject":
                        CWC.NamedObject ndoControl = Page.FindCamstarControl(sControlID) as CWC.NamedObject;
                        ndoControl.ClearData();
                        break;
                    case "RevisionedObject":
                        CWC.RevisionedObject rdoControl = Page.FindCamstarControl(sControlID) as CWC.RevisionedObject;
                        rdoControl.ClearData();
                        break;
                    case "TextBox":
                        CWC.TextBox txtControl = Page.FindCamstarControl(sControlID) as CWC.TextBox;
                        txtControl.ClearData();
                        break;
                    case "CheckBox":
                        CWC.CheckBox chkControl = Page.FindCamstarControl(sControlID) as CWC.CheckBox;
                        chkControl.ClearData();
                        break;
                    case "DropDownList":
                        CWC.DropDownList ddlControl = Page.FindCamstarControl(sControlID) as CWC.DropDownList;
                        ddlControl.ClearData();
                        break;
                    case "WorkflowNavigator":
                        CWC.WorkflowNavigator WfNavigatorControl = Page.FindCamstarControl(sControlID) as CWC.WorkflowNavigator;
                        WfNavigatorControl.ClearData();
                        break;
                    case "DateChooser":
                        CWC.DateChooser dateControl = Page.FindCamstarControl(sControlID) as CWC.DateChooser;
                        dateControl.ClearData();
                        break;
                }
            }
        } // ClearControlData

        //--------------------------------------
        //
        //--------------------------------------
        private void UpdateSelectionData(RecordSet UpdateData)
        {
            bool bRowFound = false;
            bool bAddAsLastRow = false;
            string sCurrentName = "";
            DataTable dtUpdateData = UpdateData.GetAsExplicitlyDataTable();
            DataTable dtData = ViewState[_Data_ViewStateConstant] as DataTable;

            // get the old name from txtSelectionName
            if (_txtSelectionName.Data != null)
                sCurrentName = _txtSelectionName.Data.ToString();

            int iRowIndex = 0;
            if (dtData != null)
            {
                foreach (DataRow drRow in dtData.Rows)
                {
                    if (drRow["Name"].ToString() == sCurrentName)
                    {
                        bRowFound = true;
                        break;
                    }
                    iRowIndex++;
                }
            }

            if (bRowFound)
            {
                // insert the update row data at the original position
                DataRow drUpdateData = dtData.NewRow();
                foreach (Header hData in UpdateData.Headers)
                    drUpdateData[hData.Name] = dtUpdateData.Rows[0][hData.Name];

                dtData.Rows.InsertAt(drUpdateData, iRowIndex);

                // remove the old row data
                if ((dtData.Rows.Count - 1) >= iRowIndex + 1)
                    dtData.Rows.RemoveAt(iRowIndex + 1);
            }
            else
            {
                if (dtData != null)
                {
                    if (dtData.Rows != null)
                    {
                        if (dtData.Rows.Count > 0)
                            bAddAsLastRow = true;
                    }
                }

                if (bAddAsLastRow) // add the update data as the last row                  
                {
                    DataRow drUpdateData = dtData.NewRow();
                    foreach (Header hData in UpdateData.Headers)
                        drUpdateData[hData.Name] = dtUpdateData.Rows[0][hData.Name];

                    dtData.Rows.Add(drUpdateData);
                    iRowIndex = dtData.Rows.Count - 1;
                }
                else // don't bother adding, just set the UpdateData as dtData
                {
                    dtData = UpdateData.GetAsExplicitlyDataTable();
                    iRowIndex = 0;
                }
            }

            _gridSelection.ClearData();

            // check if the data columns returned are uppercase (usually during Oracle)
            // blanket assumption that if one is uppercase, all will be uppercase
            string sDynamicTypeName = _dynamicTypeName;
            foreach (DataColumn d in dtData.Columns)
            {
                string sColumnNameUpper = d.ColumnName.ToUpper();
                if (sColumnNameUpper == d.ColumnName)
                {
                    sDynamicTypeName = _dynamicTypeNameEx;
                    break;
                }
            }


            if (dtData != null)
            {
                JQDataGrid _gridSelectionTemp = Page.FindCamstarControl("SelectionGrid") as JQDataGrid;
                int iHiddenColumns = _HiddenGridColumns.Count;
                string[] sHiddenColumns = new string[iHiddenColumns];
                int iIndex = 0;
                foreach (string sColumnName in _HiddenGridColumns)
                {
                    sHiddenColumns[iIndex] = sColumnName;
                    iIndex++;
                }

                if (!bIsColumnsDefined)
                {
                    SEMI.AppCode.GridUtility.ItemListGrid_SetColumns(wpSelectionWP, dtData, _gridSelectionTemp.ID, null, sDynamicTypeName, true, sHiddenColumns, true, null, UpdateData.Headers);
                    bIsColumnsDefined = true;
                }
                SEMI.AppCode.GridUtility.ItemListGrid_BindDataTable(wpSelectionWP, dtData, ref _gridSelectionTemp, sDynamicTypeName);
            }

            ViewState[_Data_ViewStateConstant] = dtData;

            // select the row            
            string sRowId = iRowIndex.ToString().PadLeft(6, '0');
            _gridSelection.Action_SelectRow(sRowId, "select");

        } // UpdateSelectionData

        //--------------------------------------
        //
        //--------------------------------------
        private void RetrieveInstanceId(string ObjectName)
        {
            string sService = Page.PrimaryServiceType;
            string sDataSQL = "";
            string sInstanceIdSQL = "";
            sDataSQL = " SELECT D.DBTableName, C.CDOName FROM CDODefinition C, DBTableDefinition D WHERE C.DefaultTableID = D.DBTableID AND C.CDODefID = (SELECT E.MaintenanceTypeID FROM CDODefinition E WHERE E.CDOName = '" + sService + "')";

            // get the session and user profile
            var fs = FrameworkManagerUtil.GetFrameworkSession();
            QueryService oService = new QueryService(fs.CurrentUserProfile);
            QueryOptions oOptions = new QueryOptions();
            RecordSet oData = new RecordSet();
            RecordSet oInstanceID = new RecordSet();

            ResultStatus oResult = oService.ExecuteAdHoc(sDataSQL, oOptions, out oData);
            if (oResult.IsSuccess)
            {
                string sTableName = "";
                string sCDOName = "";

                if (oData != null)
                {
                    if (oData.Rows != null)
                    {
                        if (oData.Rows.Length > 0)
                        {
                            sTableName = oData.Rows[0].Values[0].ToString();
                            sCDOName = oData.Rows[0].Values[1].ToString();

                            if (sTableName != "" && sCDOName != "")
                            {
                                sInstanceIdSQL = "SELECT " + sCDOName + "Id FROM " + sTableName + " WHERE " + sCDOName + "Name = N'" + ObjectName + "'";
                                ResultStatus oInstanceIDResult = oService.ExecuteAdHoc(sInstanceIdSQL, oOptions, out oInstanceID);

                                if (oInstanceIDResult.IsSuccess)
                                    if (oInstanceID != null)
                                        if (oInstanceID.Rows != null)
                                            if (oInstanceID.Rows.Length > 0)
                                            {
                                                _txtSelectionInstanceId.Data = oInstanceID.Rows[0].Values[0].ToString();
                                                _chkExistingInstance.CheckControl.Checked = true;
                                            }
                            }
                        }
                    }
                }
            }
        } // RetrieveInstanceId

        //--------------------------------------
        // To set the disable value clearing of the controls collection to true
        //--------------------------------------
        private void DisableValueClearing(CamstarControlsCollection ccCollection)
        {
            for (int x = 0; x < ccCollection.Count; x++)
            {
                string sControlID = ccCollection[x].Control.ID;
                string sControlType = ccCollection[x].Control.GetType().Name;

                switch (sControlType)
                {
                    case "NamedObject":
                        CWC.NamedObject ndoControl = Page.FindCamstarControl(sControlID) as CWC.NamedObject;
                        ndoControl.DisableValueClearing = true;
                        break;
                    case "RevisionedObject":
                        CWC.RevisionedObject rdoControl = Page.FindCamstarControl(sControlID) as CWC.RevisionedObject;
                        rdoControl.DisableValueClearing = true;
                        break;
                    case "TextBox":
                        CWC.TextBox txtControl = Page.FindCamstarControl(sControlID) as CWC.TextBox;
                        txtControl.DisableValueClearing = true;
                        break;
                    case "CheckBox":
                        CWC.CheckBox chkControl = Page.FindCamstarControl(sControlID) as CWC.CheckBox;
                        chkControl.DisableValueClearing = true;
                        break;
                    case "DropDownList":
                        CWC.DropDownList ddlControl = Page.FindCamstarControl(sControlID) as CWC.DropDownList;
                        ddlControl.DisableValueClearing = true;
                        break;
                    case "WorkflowNavigator":
                        CWC.WorkflowNavigator WfNavigatorControl = Page.FindCamstarControl(sControlID) as CWC.WorkflowNavigator;
                        WfNavigatorControl.DisableValueClearing = true;
                        var stackControl = Page.FindCamstarControl(WfNavigatorControl.ClientID + "_Stack") as FieldControl;
                        stackControl.DisableValueClearing = true;
                        WfNavigatorControl.StepControl.DisableValueClearing = true;
                        break;
                    case "DateChooser":
                        CWC.DateChooser dateControl = Page.FindCamstarControl(sControlID) as CWC.DateChooser;
                        dateControl.DisableValueClearing = true;
                        break;
                }
            }
        } // ClearControlData

        //--------------------------------------
        //
        //--------------------------------------
        private void UpdateObjectChangesNameControl(string sInstanceName)
        {
            List<CWC.TextBox> _txtControls = Page.FindCamstarControls<CWC.TextBox>();
            for (int x = 0; x < _txtControls.Count; x++)
            {
                if (_txtControls[x].FieldExpressions.Trim().ToUpper().Contains(".OBJECTCHANGES.NAME"))
                    if (_txtControls[x].Data == null || _txtControls[x].Data.ToString() == "")
                        _txtControls[x].Data = sInstanceName;
            } // UpdateObjectChangesNameControl

        }

        //--------------------------------------
        //
        //--------------------------------------
        private void SelectionWfNavigators_SetSubmissionMode()
        {
            //set any Selection WfNavigator child controls dataSubmissionMode to Skip
            for (int x = 0; x < _SelectionControls.Count; x++)
            {
                string sControlID = _SelectionControls[x].Control.ID;
                string sControlType = _SelectionControls[x].Control.GetType().Name;

                if (sControlType == "WorkflowNavigator")
                {
                    CWC.WorkflowNavigator WfNavigatorControl = Page.FindCamstarControl(sControlID) as CWC.WorkflowNavigator;
                    WfNavigatorControl.StepControl.DataSubmissionMode = DataSubmissionModeType.Skip;
                    var stackControl = Page.FindCamstarControl(WfNavigatorControl.ClientID + "_Stack") as FieldControl;
                    stackControl.DataSubmissionMode = DataSubmissionModeType.Skip;
                }
            }
        }

        //--------------------------------------
        //
        //--------------------------------------
        private void CollateControlData(ref CamstarControlsCollection ControlsSet, ref ICreator oObjectChanges, ref ICreator oServiceData)
        {
            // add the input data from the inheriting class's _SelectionControls
            for (int x = 0; x < ControlsSet.Count; x++)
            {
                string sControlID = ControlsSet[x].Control.ID;
                string sControlType = ControlsSet[x].Control.GetType().Name;

                string sFieldExpression = "";
                string sFieldToAdd = "";

                switch (sControlType)
                {
                    case "NamedObject":
                        CWC.NamedObject ndoControl = Page.FindCamstarControl(sControlID) as CWC.NamedObject;
                        if (ndoControl.FieldExpressions != null)
                        {
                            sFieldExpression = ndoControl.FieldExpressions;
                            if (!string.IsNullOrEmpty(sFieldExpression))
                            {
                                sFieldToAdd = GetFieldFromFieldExpression(sFieldExpression);

                                if (ndoControl.Data != null)
                                    oObjectChanges.SetValue(sFieldToAdd, ndoControl.Data as NamedObjectRef);
                                else
                                    oObjectChanges.SetValue(sFieldToAdd, new NamedObjectRef());
                            }
                        }
                        break;
                    case "RevisionedObject":
                        CWC.RevisionedObject rdoControl = Page.FindCamstarControl(sControlID) as CWC.RevisionedObject;
                        if (rdoControl.FieldExpressions != null)
                        {
                            sFieldExpression = rdoControl.FieldExpressions;
                            if (!string.IsNullOrEmpty(sFieldExpression))
                            {
                                sFieldToAdd = GetFieldFromFieldExpression(sFieldExpression);

                                if (rdoControl.Data != null)
                                    oObjectChanges.SetValue(sFieldToAdd, rdoControl.Data as RevisionedObjectRef);
                                else
                                    oObjectChanges.SetValue(sFieldToAdd, new RevisionedObjectRef());
                            }
                        }
                        break;

                    case "TextBox":
                        CWC.TextBox txtControl = Page.FindCamstarControl(sControlID) as CWC.TextBox;
                        if (txtControl.FieldExpressions != null)
                        {
                            sFieldExpression = txtControl.FieldExpressions;
                            if (!string.IsNullOrEmpty(sFieldExpression))
                            {
                                sFieldToAdd = GetFieldFromFieldExpression(sFieldExpression);

                                if (txtControl.Data != null)
                                    oObjectChanges.SetValue(sFieldToAdd, txtControl.Data.ToString());
                                else
                                    oObjectChanges.SetValue(sFieldToAdd, "");
                            }
                        }
                        break;

                    case "CheckBox":
                        CWC.CheckBox chkControl = Page.FindCamstarControl(sControlID) as CWC.CheckBox;
                        if (chkControl.FieldExpressions != null)
                        {
                            sFieldExpression = chkControl.FieldExpressions;
                            if (!string.IsNullOrEmpty(sFieldExpression))
                            {
                                sFieldToAdd = GetFieldFromFieldExpression(sFieldExpression);

                                if (chkControl.CheckControl.Checked)
                                    oObjectChanges.SetValue(sFieldToAdd, true);
                                else
                                    oObjectChanges.SetValue(sFieldToAdd, false);
                            }
                        }
                        break;

                    case "DropDownList":
                        CWC.DropDownList ddlControl = Page.FindCamstarControl(sControlID) as CWC.DropDownList;
                        if (ddlControl.FieldExpressions != null)
                        {
                            sFieldExpression = ddlControl.FieldExpressions;
                            if (!string.IsNullOrEmpty(sFieldExpression))
                            {
                                sFieldToAdd = GetFieldFromFieldExpression(sFieldExpression);

                                if (ddlControl.Data != null)
                                    oObjectChanges.SetValue(sFieldToAdd, ddlControl.Data);
                                else
                                    oObjectChanges.SetValue(sFieldToAdd, null);
                            }
                        }
                        break;

                    case "WorkflowNavigator":
                        CWC.WorkflowNavigator WfNavigatorControl = Page.FindCamstarControl(sControlID) as CWC.WorkflowNavigator;
                        var stackControl = Page.FindCamstarControl(WfNavigatorControl.ClientID + "_Stack") as FieldControl;

                        string sWfExp = "";
                        string sWfField = "";

                        string sWfStepExp = "";
                        string sWfStepField = "";

                        string sWfStackExp = "";
                        string sWfStackField = "";

                        if (WfNavigatorControl.FieldExpressions != null)
                        {
                            sWfExp = WfNavigatorControl.FieldExpressions;
                            sWfStepExp = WfNavigatorControl.ToStepValuesExpressions;
                            sWfStackExp = WfNavigatorControl.StackFieldExpressions;
                            if (!string.IsNullOrEmpty(sWfExp) || !string.IsNullOrEmpty(sWfStepExp))
                            {
                                sWfField = GetFieldFromFieldExpression(sWfExp);
                                sWfStepField = GetFieldFromFieldExpression(sWfStepExp);
                                sWfStackField = GetFieldFromFieldExpression(sWfStackExp);

                                string sStepPath = "ObjectChanges." + sWfStepField;
                                string sParent = "ObjectChanges." + sWfField;
                                string sStack = "ObjectChanges." + sWfStackField;

                                if (WfNavigatorControl.StepControl.Data != null)
                                {
                                    NamedSubentityRef WIPStep = new NamedSubentityRef() { Name = (WfNavigatorControl.StepControl.Data as NamedSubentityRef).Name, Parent = WfNavigatorControl.Data as RevisionedObjectRef };
                                    oServiceData.SetValue(sStepPath, WIPStep);
                                    oServiceData.SetValue(sParent, WIPStep.Parent);
                                    if (stackControl.Data != null)
                                    {
                                        NamedSubentityRef[] stack = (stackControl.Data) as NamedSubentityRef[];
                                        oServiceData.SetValue(sStack, stack);
                                    }
                                }
                                else
                                {
                                    oObjectChanges.SetValue(sWfStepField, null);
                                    oObjectChanges.SetValue(sWfField, null);
                                    oObjectChanges.SetValue(sWfStackField, null);
                                }
                            }
                        } // if (WfNavigatorControl.FieldExpressions != null)
                        break;
                }
            }
        }

        //--------------------------------------
        //
        //--------------------------------------
        public override void DisplayValues(Service serviceData)
        {
            base.DisplayValues(serviceData);

            // this bit of code is to store any stack data into a hashtable
            // apparently the Portal does not handle storing the return data (from the server) back into the appropriate WfNavigator control's stack control
            // store the stack data into the hashtable for use during updates/save new later on
            var oServiceData = WCFObject.Clone(serviceData) as ICreator;

            for (int x = 0; x < _CriteriaWorkflowNavigators.Count; x++)
            {
                string sControlID = _CriteriaWorkflowNavigators[x].Control.ID;
                string sWfStackField = "";
                string sWfStackExp = "";

                CWC.WorkflowNavigator WfNavigatorControl = Page.FindCamstarControl(sControlID) as CWC.WorkflowNavigator;

                if (WfNavigatorControl.StackFieldExpressions != null)
                {
                    sWfStackExp = WfNavigatorControl.StackFieldExpressions;
                    sWfStackField = GetFieldFromFieldExpression(sWfStackExp);

                    if (oServiceData.GetValue("ObjectChanges." + sWfStackField) != null)
                    {
                        NamedSubentityRef[] oStackData = oServiceData.GetValue("ObjectChanges." + sWfStackField) as NamedSubentityRef[];
                        StackData_Set(sWfStackField, oStackData);
                    }
                } // if (WfNavigatorControl.StackFieldExpressions != null)                
            }
        }

        //--------------------------------------
        //
        //--------------------------------------
        private string GetFieldFromFieldExpression(string sExpression)
        {
            string sField = "";
            if (!string.IsNullOrEmpty(sExpression))
            {
                int iLastDotIndex = sExpression.LastIndexOf('.');
                if (iLastDotIndex >= 0)
                    sField = sExpression.Substring(iLastDotIndex + 1);
                else
                    sField = sExpression;
            }

            return sField;
        } // GetFieldFromFieldExpression

        //--------------------------------------
        //
        //--------------------------------------
        protected void SS_SetupB_StackData_Update(string sWorkflowNavControlID)
        {
            CWC.WorkflowNavigator WfNavigatorControl = Page.FindCamstarControl(sWorkflowNavControlID) as CWC.WorkflowNavigator;
            var stackControl = Page.FindCamstarControl(WfNavigatorControl.ClientID + "_Stack") as FieldControl;

            if (WfNavigatorControl.StackFieldExpressions != null)
            {
                string sWfStackField = "";
                string sWfStackExp = "";

                sWfStackExp = WfNavigatorControl.StackFieldExpressions;
                sWfStackField = GetFieldFromFieldExpression(sWfStackExp);
                NamedSubentityRef[] oStackData = stackControl.Data as NamedSubentityRef[];

                StackData_Set(sWfStackField, oStackData);

            } // if (WfNavigatorControl.StackFieldExpressions != null)    
        }

        //--------------------------------------
        //
        //--------------------------------------
        private void StackData_Set(string sKey, NamedSubentityRef[] oStackData)
        {
            if (ViewState[_StackData_ViewStateConstant] != null)
                htStackValues = ViewState[_StackData_ViewStateConstant] as Hashtable;
            else
                htStackValues = new Hashtable();

            if (!htStackValues.ContainsKey(sKey))
                htStackValues.Add(sKey, oStackData);
            else
                htStackValues[sKey] = oStackData;

            ViewState[_StackData_ViewStateConstant] = htStackValues;
        }

        //--------------------------------------
        //
        //--------------------------------------
        private NamedSubentityRef[] StackData_Get(string sKey)
        {
            NamedSubentityRef[] oStackData = null;
            htStackValues = new Hashtable();

            if (ViewState[_StackData_ViewStateConstant] != null)
                htStackValues = ViewState[_StackData_ViewStateConstant] as Hashtable;

            if (htStackValues.ContainsKey(sKey))
                oStackData = htStackValues[sKey] as NamedSubentityRef[];

            return oStackData;
        }
    }
}

