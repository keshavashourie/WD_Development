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
/// Summary description for ss_ToolUpdateStatus
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_ToolUpdateStatus : MatrixWebPart
    {
        //Controls declaration
        private CWC.CheckBox _chkIsSetReserveEquip { get { return FindCamstarControl("ss_ToolUpdateStatus_ss_IsSetReserveEquip") as CWC.CheckBox; } }
        private CWC.CheckBox _chkIsSetReserveEmp { get { return FindCamstarControl("ss_ToolUpdateStatus_ss_IsSetReserveEmp") as CWC.CheckBox; } }
        private CWC.CheckBox _chkIsSetLocation { get { return FindCamstarControl("ss_ToolUpdateStatus_ss_IsSetLocation") as CWC.CheckBox; } }
        private CWC.CheckBox _chkIsClearEquip { get { return FindCamstarControl("ss_ToolUpdateStatus_ss_IsClearEquip") as CWC.CheckBox; } }
        private CWC.CheckBox _chkIsClearEmp { get { return FindCamstarControl("ss_ToolUpdateStatus_ss_IsClearEmp") as CWC.CheckBox; } }
        private CWC.CheckBox _chkIsClearLocation { get { return FindCamstarControl("ss_ToolUpdateStatus_ss_IsClearLocation") as CWC.CheckBox; } }

        private CWC.NamedObject _ddlToolAction { get { return (Page.FindCamstarControl("ss_ToolUpdateStatus_ss_ToolAction") as CWC.NamedObject); } }
        private CWC.NamedObject _ddlReserveEquip { get { return (Page.FindCamstarControl("ss_ToolUpdateStatus_ss_ReserveEquipment") as CWC.NamedObject); } }
        private CWC.NamedObject _ddlReserveEmp { get { return (Page.FindCamstarControl("ss_ToolUpdateStatus_ss_ReserveEmployee") as CWC.NamedObject); } }
        private CWC.NamedObject _ddlToStatus { get { return (Page.FindCamstarControl("ss_ToolUpdateStatus_ResourceStatusCode") as CWC.NamedObject); } }
        private CWC.NamedObject _ddlSetLocation { get { return (Page.FindCamstarControl("ss_ToolUpdateStatus_ss_Location") as CWC.NamedObject); } }
       
        private CWC.NamedObject _ddlEnvDataLocation { get { return (Page.FindCamstarControl("EnvDataLocationCollection") as CWC.NamedObject); } }
        private CWC.NamedObject _ddlEnvDataEmp { get { return (Page.FindCamstarControl("EnvDataEmp") as CWC.NamedObject); } }
        private CWC.NamedObject _ddlEnvDataEquip { get { return (Page.FindCamstarControl("EnvDataEquip") as CWC.NamedObject); } }

        private CWC.NamedObject _ddlStatusReason { get { return (Page.FindCamstarControl("ss_ToolUpdateStatus_ResourceStatusReason") as CWC.NamedObject); } }
        private CWC.NamedObject _ddlEmp { get { return (Page.FindCamstarControl("ss_ToolUpdateStatus_Employee") as CWC.NamedObject); } }
        private CWC.TextBox _txtComment { get { return (Page.FindCamstarControl("ss_ToolUpdateStatus_Comments") as CWC.TextBox); } }


        //---------------------------------------------------
        // On Load Event
        //---------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _ddlToolAction.Load += new EventHandler(FetchToolActionDetails);
            if (Page.IsPostBack != true)
            {
                _ddlEnvDataLocation.Data = _ddlSetLocation.Data;
                _ddlEnvDataEquip.Data = _ddlReserveEquip.Data;
                _ddlEnvDataEmp.Data = _ddlReserveEmp.Data;
            }


        }
        private void CustomReset()
        {

            _ddlSetLocation.ClearData();
            _ddlReserveEquip.ClearData();
            _ddlReserveEmp.ClearData();
            _ddlStatusReason.ClearData();
            _ddlEmp.ClearData();
            _txtComment.TextControl.Text = "";

        } // CustomReset

        public override void WebPartCustomAction(object sender, CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as Camstar.WebPortal.Personalization.CustomAction;

            if (action != null && action.Parameters == "Reset")
            {
                CustomReset();
                //Page.ShopfloorReset(sender, e);
                _ddlSetLocation.Data = _ddlEnvDataLocation.Data;
                _ddlReserveEquip.Data = _ddlEnvDataEquip.Data;//Page.PortalContext.DataContract.GetValueByName<OM.NamedObjectRef>("ToolUpdateStatus_ReserveEquipDM");
                _ddlReserveEmp.Data = _ddlEnvDataEmp.Data;//Page.PortalContext.DataContract.GetValueByName<OM.NamedObjectRef>("ToolUpdateStatus_ReserveEmpDM");



            }

        } // WebPartCustomAction 

        protected virtual void FetchToolActionDetails(object sender, EventArgs e)
        //protected override void OnPreRender(EventArgs e)
        {


            var fs = FrameworkManagerUtil.GetFrameworkSession();

            //Initialize Service & Objects
            ss_ToolActionMaintService oService = new ss_ToolActionMaintService(fs.CurrentUserProfile);
            ss_ToolActionMaint oServiceData = new ss_ToolActionMaint();
            ss_ToolActionChanges objChanges = new ss_ToolActionChanges();
            ss_ToolActionMaint_Info oServiceInfo = new ss_ToolActionMaint_Info();
            ss_ToolActionMaint_Request oServiceRequest = new ss_ToolActionMaint_Request();
            ss_ToolActionMaint_Result oServiceResult = new ss_ToolActionMaint_Result();

            //set input data
            oServiceData.ObjectToChange = _ddlToolAction.Data as NamedObjectRef;
           // oServiceData.ObjectToChange.Name = _ddlToolAction.TextEditControl.Text;

            //Set Request Value
            ss_ToolActionChanges_Info objChangesInfo = new ss_ToolActionChanges_Info
            {
                ss_IsSetLocation = FieldInfoUtil.RequestValue(),
                ss_IsSetReserveEmp = FieldInfoUtil.RequestValue(),
                ss_IsSetReserveEquip = FieldInfoUtil.RequestValue(),
                ss_ToStatus = FieldInfoUtil.RequestValue(),
                ss_IsClearLocation = FieldInfoUtil.RequestValue(),
                ss_IsClearReserveEmp = FieldInfoUtil.RequestValue(),
                ss_IsClearReserveEquip = FieldInfoUtil.RequestValue()

            };
            oServiceInfo.ObjectChanges = objChangesInfo;


            oServiceRequest.Info = oServiceInfo;

            //Execute Request
            ResultStatus Results = oService.Load(oServiceData, oServiceRequest, out oServiceResult);



            if (Results.IsSuccess)
            {
                _chkIsSetLocation.IsChecked = false;
                _chkIsSetReserveEmp.IsChecked = false;
                _chkIsSetReserveEquip.IsChecked = false;
                _ddlSetLocation.Enabled = false;
                _ddlReserveEmp.Enabled = false;
                _ddlReserveEquip.Enabled = false;

                _ddlToStatus.Data = oServiceResult.Value.ObjectChanges.ss_ToStatus;

                if (oServiceResult.Value.ObjectChanges.ss_IsSetLocation.Value == true)
                {
                    _chkIsSetLocation.IsChecked = true;
                    _ddlSetLocation.Enabled = true;
                    _ddlSetLocation.DataSubmissionMode = Camstar.WebPortal.Personalization.DataSubmissionModeType.NotSet;
                }
                if (oServiceResult.Value.ObjectChanges.ss_IsSetReserveEmp.Value == true)
                {
                    _chkIsSetReserveEmp.IsChecked = true;
                    _ddlReserveEmp.Enabled = true;
                    _ddlReserveEmp.DataSubmissionMode = Camstar.WebPortal.Personalization.DataSubmissionModeType.NotSet;
                }
                if (oServiceResult.Value.ObjectChanges.ss_IsSetReserveEquip.Value == true)
                {
                    _chkIsSetReserveEquip.IsChecked = true;
                    _ddlReserveEquip.Enabled = true;
                    _ddlReserveEquip.DataSubmissionMode = Camstar.WebPortal.Personalization.DataSubmissionModeType.NotSet;
                }
                
                if (oServiceResult.Value.ObjectChanges.ss_IsClearReserveEquip.Value == true)
                {
                    _chkIsClearEquip.IsChecked = true;
                    _ddlReserveEquip.Enabled = false;
                    _ddlReserveEquip.DataSubmissionMode = Camstar.WebPortal.Personalization.DataSubmissionModeType.Skip;
                }
                if (oServiceResult.Value.ObjectChanges.ss_IsClearReserveEmp.Value == true)
                {
                    _chkIsClearEmp.IsChecked = true;
                    _ddlReserveEmp.Enabled = false;
                    _ddlReserveEmp.DataSubmissionMode = Camstar.WebPortal.Personalization.DataSubmissionModeType.Skip;

                }
                if (oServiceResult.Value.ObjectChanges.ss_IsClearLocation.Value == true)
                {
                    _chkIsClearLocation.IsChecked = true;
                    _ddlSetLocation.Enabled = false;
                    _ddlSetLocation.DataSubmissionMode = Camstar.WebPortal.Personalization.DataSubmissionModeType.Skip;
                }

            }
        } // FetchToolActionDetails






    }
}



