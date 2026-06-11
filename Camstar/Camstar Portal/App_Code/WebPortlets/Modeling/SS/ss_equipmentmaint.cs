/* Copyright 2024 Siemens */
using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.PortalFramework;
using SEMI.AppCode;

using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework.Utilities;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WCF.ObjectStack;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WebPortal.Constants;

namespace Camstar.WebPortal.WebPortlets.Modeling
{

    /// <summary>
    /// TODO: Add a Summary description for this Camstar Web Part
    /// </summary>
    public class SS_EquipmentMaint : MatrixWebPart
    {
        #region Controls
        //use this region to make properties that reference controls on the Portal page using Page.FindCamstarControl. 
        //Example:
        //private CWC.RevisionedObject MaintenanceReqField
        //{
        //    get { return Page.FindCamstarControl("ResourceActivation_MaintenanceReq") as CWC.RevisionedObject; }
        //}

        private CWC.TextBox NameField
        {
            get { return Page.FindCamstarControl("NameTxt") as CWC.TextBox; }
        }

        private CWC.CheckBox SysForceToUpperField
        {
            get { return Page.FindCamstarControl("ObjectChanges_SysForceToUpper") as CWC.CheckBox; }
        }

        private CWC.CheckBox UseSPCMatrixField
        {
            get { return Page.FindCamstarControl("ObjectChanges_UseSPCMatrix") as CWC.CheckBox; }
        }

        private CWC.CheckBox TrackMaterialsHistoryField
        {
            get { return Page.FindCamstarControl("ObjectChanges_TrackMaterialsHistory") as CWC.CheckBox; }
        }

        private CWC.CheckBox RequireMinTimeWindowField
        {
            get { return Page.FindCamstarControl("ObjectChanges_RequireMinTimeWindow") as CWC.CheckBox; }
        }

        private CWC.NamedObject SPCSetupField
        {
            get { return Page.FindCamstarControl("ObjectChanges_SPCSetup") as CWC.NamedObject; }
        }

        private CWC.NamedObject InitialStatusField
        {
            get { return Page.FindCamstarControl("ObjectChanges_InitialStatus") as CWC.NamedObject; }
        }

        protected JQDataGrid _gridChildEquipments
        {
            get { return Page.FindCamstarControl("ObjectChanges_ChildEquipments") as JQDataGrid; }
        }

        protected JQDataGrid _gridSlots
        {
            get { return Page.FindCamstarControl("ObjectChanges_ss_Slots") as JQDataGrid; }
        }

        private CWC.NamedObject ParentResource
        {
            get { return Page.FindCamstarControl("scsParentResource") as CWC.NamedObject; }
        }

        private CWC.RevisionedObject RDOFeederPlan
        {
            get { return Page.FindCamstarControl("ObjectChanges_ss_FeederPlan") as CWC.RevisionedObject; }
        }

        private CWC.TextBox SubEquipmentLogicalIDField
        {
            get { return Page.FindCamstarControl("ObjectChanges_SubEquipmentLogicalID") as CWC.TextBox; }
        }

        private CWC.DropDownList EquipmentTypeField
        {
            get { return Page.FindCamstarControl("Control") as CWC.DropDownList; }
        }

        protected JQDataGrid _gridEqpStorageLocations
        {
            get { return Page.FindCamstarControl("ObjectChanges_scsEqpStorageLocations") as JQDataGrid; }
        }

        protected CWC.NamedObject _ndoPhysicalLocationInline
        {
            get { return _gridEqpStorageLocations.FindControl("ObjectChanges_scsEqpStorageLocations_PhysicalLocation_InlineEditorControl") as CWC.NamedObject; }
        }


        #endregion

        #region Protected Functions
        protected override void OnPreLoad(object sender, EventArgs e)
        {
            base.OnPreLoad(sender, e);

            //Clear the expression for the Equipment specific fields if the primary service is Resource or Tool or EL Feeder or EL Feeder Bank
            if (Page.PrimaryServiceType == "ResourceMaint" || Page.PrimaryServiceType == "ToolMaint" || Page.PrimaryServiceType == "FeederMaint" || Page.PrimaryServiceType == "FeederBankMaint")
            {
                SPCSetupField.FieldExpressions = null;
                UseSPCMatrixField.FieldExpressions = null;
                TrackMaterialsHistoryField.FieldExpressions = null;
                RequireMinTimeWindowField.FieldExpressions = null;
                _gridEqpStorageLocations.FieldExpressions = null;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _ndoPhysicalLocationInline.DataChanged += new EventHandler(_dSPCChartTypeInline_DataChanged);
            _ndoPhysicalLocationInline.AutoPostBack = true;

            if (!Page.IsPostBack)
            {
                if (!string.IsNullOrEmpty(Page.Request.QueryString[QueryStringConstants.NodeDefinition_Mod]))
                {
                    string name = Page.Request.QueryString[QueryStringConstants.NodeDefinition_Mod];
                    string parentTitle = string.Empty;
                    if (!string.IsNullOrEmpty(Page.Request.QueryString[QueryStringConstants.ParentTitle_Mod]) && !name.Equals("Area"))
                    {
                        parentTitle = Page.Request.QueryString[QueryStringConstants.ParentTitle_Mod];
                        ParentResource.Data = parentTitle;
                        ParentResource.Enabled = false;
                    }
                }
            }

            if (Page.IsPostBack)
            {
                if (PrimaryServiceType != "MaskMaint" && Page.PrimaryServiceType != "ToolMaint" && Page.PrimaryServiceType != "ResourceMaint" && Page.PrimaryServiceType != "FeederMaint" && Page.PrimaryServiceType != "FeederBankMaint")
                {
                    SPCSetupField.Enabled = true;
                    SPCSetupField.ReadOnly = false;
                    UseSPCMatrixField.Enabled = true;
                    UseSPCMatrixField.ReadOnly = false;
                    TrackMaterialsHistoryField.Enabled = true;
                    TrackMaterialsHistoryField.ReadOnly = false;
                    RequireMinTimeWindowField.Enabled = true;
                    RequireMinTimeWindowField.ReadOnly = false;
                }
                else
                {
                    SPCSetupField.Enabled = false;
                    SPCSetupField.ReadOnly = true;
                    SPCSetupField.TextEditControl.Text = "";
                    UseSPCMatrixField.Enabled = false;
                    UseSPCMatrixField.ReadOnly = true;
                    UseSPCMatrixField.CheckControl.Checked = false;
                    TrackMaterialsHistoryField.Enabled = false;
                    TrackMaterialsHistoryField.ReadOnly = true;
                    TrackMaterialsHistoryField.CheckControl.Checked = false;
                    RequireMinTimeWindowField.Enabled = false;
                    RequireMinTimeWindowField.ReadOnly = true;
                    RequireMinTimeWindowField.CheckControl.Checked = false;
                    _gridEqpStorageLocations.Enabled = false;
                    _gridEqpStorageLocations.ReadOnly = true;
                    (_gridEqpStorageLocations.Settings as GridDataSettingsItemList).EditorSettings.AddRowMode = AddRowModes.AddButton;
                }
                if (PrimaryServiceType == "ss_FeederMaint")
                {
                    ParentResource.Enabled = false;
                    ParentResource.ReadOnly = true;
                    _gridSlots.Visible = false;
                    RDOFeederPlan.Enabled = false;
                    SubEquipmentLogicalIDField.Enabled = false;
                    EquipmentTypeField.Enabled = false;
                    _gridEqpStorageLocations.Enabled = false;
                    _gridEqpStorageLocations.ReadOnly = true;
                    (_gridEqpStorageLocations.Settings as GridDataSettingsItemList).EditorSettings.AddRowMode = AddRowModes.AddButton;

                }
                if (PrimaryServiceType == "ss_FeederBankMaint")
                {
                    ParentResource.Enabled = false;
                    ParentResource.ReadOnly = true;
                    RDOFeederPlan.Enabled = false;
                    SubEquipmentLogicalIDField.Enabled = false;
                    EquipmentTypeField.Enabled = false;
                    _gridEqpStorageLocations.Enabled = false;
                    _gridEqpStorageLocations.ReadOnly = true;
                    (_gridEqpStorageLocations.Settings as GridDataSettingsItemList).EditorSettings.AddRowMode = AddRowModes.AddButton;
                }
            }
        }

        protected void GetChildEquipments()
        {
            var fs = FrameworkManagerUtil.GetFrameworkSession(this.Page.Session);

            QueryService oSvc = new QueryService(fs.CurrentUserProfile);

            QueryParameters oQueryParam = new QueryParameters();
            oQueryParam.Parameters = new QueryParameter[1];
            oQueryParam.Parameters[0] = new QueryParameter();
            oQueryParam.Parameters[0].Name = "EquipmentName";
            oQueryParam.Parameters[0].Value = NameField.Data == null ? null : NameField.Data.ToString();

            QueryOptions oQueryOptions = new QueryOptions();
            oQueryOptions.QueryType = OM.QueryType.User;
            oQueryOptions.StartRow = 1;

            RecordSet oQueryResult = new RecordSet();
            ResultStatus oResultStatus = new ResultStatus();
            DataSet oDataSet = new DataSet();

            oResultStatus = oSvc.Execute("_EquipmentMaint_GetChildEquipments", oQueryParam, oQueryOptions, out oQueryResult);

            if (oResultStatus.IsSuccess)
            {
                JQDataGrid _gridSearchResultTemp = _gridChildEquipments;
                SEMI.AppCode.GridUtility.ItemListGrid_BindDataTable(this, oQueryResult.GetAsExplicitlyDataTable(), ref _gridSearchResultTemp);
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            InitialStatusField.Enabled = (InitialStatusField.TextEditControl.Text == "");
            base.OnPreRender(e);

            //if (PrimaryServiceType != "MaskMaint" && Page.PrimaryServiceType != "ToolMaint")
            // {
            GetChildEquipments();
            // }

            try
            {
                UIUtility.SetCapital(NameField, SysForceToUpperField.CheckControl.Checked);
            }
            catch (Exception ex)
            {
                DisplayMessage(new ResultStatus(ex.Message, false));
            }
        }

        #endregion

        #region Public Functions
        public void _dSPCChartTypeInline_DataChanged(object sender, EventArgs e)
        {
            try
            {
                if ((sender as CWC.NamedObject).Data != null)
                {
                    int selectedRowId = Convert.ToInt32(_gridEqpStorageLocations.SelectedRowID);
                    List<scsEqpStorageLocationChanges> existingStorageLocations = new List<scsEqpStorageLocationChanges>();
                    if (_gridEqpStorageLocations.Data != null)
                        existingStorageLocations = (_gridEqpStorageLocations.Data as scsEqpStorageLocationChanges[]).ToList();

                    string selectedPhysicalLocation = (sender as CWC.NamedObject).Data.ToString();
                    var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
                    if (session != null)
                    {
                        var service = new PhysicalLocationMaintService(session.CurrentUserProfile);
                        var serviceData = new PhysicalLocationMaint
                        {
                            ObjectToChange = new NamedObjectRef(selectedPhysicalLocation, "PhysicalLocation")
                        };

                        var request = new PhysicalLocationMaint_Request
                        {
                            Info = new PhysicalLocationMaint_Info
                            {
                                ObjectToChange = new Info(true),
                                ObjectChanges = new PhysicalLocationChanges_Info
                                {
                                    scsStorageType = new Info(true)
                                }
                            }
                        };

                        var result = new PhysicalLocationMaint_Result();
                        ResultStatus status = service.Load(serviceData, request, out result);
                        if (status != null && status.IsSuccess)
                        {
                            string storageType = result.Value.ObjectChanges.scsStorageType.ToString();
                            //if (selectedRowId >= existingStorageLocations.Count)
                            //{
                            //    scsEqpStorageLocationChanges newStorageLocation = new scsEqpStorageLocationChanges();
                            //    newStorageLocation.PhysicalLocation = new NamedObjectRef(selectedPhysicalLocation, "PhysicalLocation");
                            //    newStorageLocation.scsStorageType = new Enumeration<scsStorageTypeEnum, int>(result.Value.ObjectChanges.scsStorageType.Value);

                            //    existingStorageLocations.Add(newStorageLocation);
                            //    _gridEqpStorageLocations.ClearData();
                            //    _gridEqpStorageLocations.Data = existingStorageLocations;
                            //}
                            //else
                            //{
                            existingStorageLocations[selectedRowId] = new scsEqpStorageLocationChanges();
                            existingStorageLocations[selectedRowId].ListItemIndex = selectedRowId;
                            existingStorageLocations[selectedRowId].PhysicalLocation = new NamedObjectRef(selectedPhysicalLocation, "PhysicalLocation");
                            existingStorageLocations[selectedRowId].scsStorageType = new Enumeration<scsStorageTypeEnum, int>(result.Value.ObjectChanges.scsStorageType.Value);

                            _gridEqpStorageLocations.ClearData();
                            _gridEqpStorageLocations.Data = existingStorageLocations.ToArray();
                            //}
                        }
                        else
                        {
                            Page.DisplayMessage(status);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayMessage(new ResultStatus(ex.Message, false));
            }
        }

        #endregion

    }

}





