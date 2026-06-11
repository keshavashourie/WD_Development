/* Copyright 2020 Siemens */
using System;
using System.Data;
using System.Web;
using System.Linq;
using System.Collections.Generic;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using OM = Camstar.WCF.ObjectStack;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.Utilities;
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;

using SEMI.AppCode;
using System.Web.UI.WebControls;

/// <summary>
/// Summary description for Class1
/// </summary>
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_LotModifyDefects : scsShopfloorBase
    {
        protected CWC.TextBox _txtSelectionId { get { return Page.FindCamstarControl("LotModifyDefects_SelectionId") as CWC.TextBox; } }
        protected CWC.TextBox _txtEmployee { get { return Page.FindCamstarControl("LotModifyDefects_Employee") as CWC.TextBox; } }
        protected CWC.Button _btnLotInfoCmdBar { get { return Page.FindCamstarControl("LotInfoPopCmdBar") as CWC.Button; } }
        protected CWC.Button _btnLotInfo { get { return Page.FindCamstarControl("LotInfoPop") as CWC.Button; } }
        protected JQDataGrid _gridDefects { get { return Page.FindCamstarControl("LotModifyDefects_Defects") as JQDataGrid; } }
        protected JQDataGrid _gridDetails { get { return Page.FindCamstarControl("LotModifyDefects_Details") as JQDataGrid; } }
        protected CWC.TextBox txtComputerName { get { return Page.FindCamstarControl("LotModifyDefects_ComputerName") as CWC.TextBox; } }
        private SEMI.AppCode.DataEnvelopControl _envSelectedLots { get { return Page.FindCamstarControl("LotModifyDefects_Lots") as SEMI.AppCode.DataEnvelopControl; } }

        public SS_LotModifyDefects()
        {
            //
            // TODO: Add constructor logic here
            //
        }
        public void SelectionId_DataChanged(Object sender, EventArgs e)
        {
            if (_txtSelectionId.Data != null)
            {
                JQDataGrid _gridDetails = Page.FindCamstarControl("LotModifyDefects_Details") as JQDataGrid;
                _gridDefects.ClearData();
                _gridDetails.ClearData();
                SEMI.AppCode.UIUtility.GetLotQuerySelection(this, "LotModifyDefects", _txtSelectionId.Data.ToString(), false, ref _gridDetails, "LotModifyDefects_Details", true);
                GetSelectedLotDefects(_txtSelectionId.Data.ToString());
                _btnLotInfo.Enabled = true;
                CommandBarBehaviourCtrl();
            }
            else
            {
                _btnLotInfo.Enabled = false;
                _gridDefects.ClearData();
                _gridDetails.ClearData();
                CommandBarBehaviourCtrl();
            }
        }
        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as Camstar.WebPortal.Personalization.CustomAction;

            if (action != null && action.Parameters == "Reset")
            {
                Page.ClearValues();
                _gridDefects.ClearData();
                _gridDetails.ClearData();
                _txtSelectionId.Focus();
            }
        }
        public override void PostExecute(ResultStatus status, Service serviceData)
        {
            base.PostExecute(status, serviceData);
            if (status.IsSuccess)
            {
                Page.ClearValues();
                _gridDefects.ClearData();
                _gridDetails.ClearData();
                _txtSelectionId.Focus();
            }
        }
        public override void GetInputData(Service serviceData)
        {
            if (_txtEmployee.Data != null)
            {
                (serviceData as LotModifyDefects).Employee = new NamedObjectRef();
                (serviceData as LotModifyDefects).Employee.Name = _txtEmployee.Data.ToString();
            }
            if (_gridDefects.Data != null && _txtSelectionId.Data != null)
            {
                LotDefects[] getLotModifyDefects = _gridDefects.Data as LotDefects[];

                (serviceData as LotModifyDefects).Defects = new ModifyDefectsDetails[getLotModifyDefects.Count()];
                (serviceData as LotModifyDefects).Container = new ContainerRef();
                (serviceData as LotModifyDefects).Container.Name = _txtSelectionId.Data.ToString();

                for (int i = 0; i < getLotModifyDefects.Count(); i++)
                {
                    (serviceData as LotModifyDefects).Defects[i] = new ModifyDefectsDetails();
                    (serviceData as LotModifyDefects).Defects[i].DefectComment = getLotModifyDefects[i].DefectComment;
                    (serviceData as LotModifyDefects).Defects[i].DefectQty = (int)getLotModifyDefects[i].DefectQty;
                    (serviceData as LotModifyDefects).Defects[i].LossReason = getLotModifyDefects[i].LossReason;
                    (serviceData as LotModifyDefects).Defects[i].ProcessType = getLotModifyDefects[i].ProcessType;
                    (serviceData as LotModifyDefects).Defects[i].Spec = getLotModifyDefects[i].Spec;
                    (serviceData as LotModifyDefects).Defects[i].TxnTimestamp = getLotModifyDefects[i].TxnTimestamp;
                    (serviceData as LotModifyDefects).Defects[i].Username = getLotModifyDefects[i].Username;
                }
            }
            base.GetInputData(serviceData);
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            CommandBarBehaviourCtrl();

            txtComputerName.TextControl.Text = SEMI.AppCode.UIUtility.GetComputerName(this);

            if (Page.IsPostBack)
            {
                // Check if it is a pop up close, get the return result
                if (SEMI.AppCode.UIUtility.IsPopupClose(this))
                {
                    if (_envSelectedLots != null)
                        // manually initialize the containers list data contract since it is not triggered when we do a manual popup close
                        if (Page.DataContract.GetValueByName("LotModifyDefects_Lots") != null)
                            _envSelectedLots.SS_ContainersList = Page.DataContract.GetValueByName("LotModifyDefects_Lots") as string[];

                    if (_envSelectedLots.SS_ContainersList != null)
                    {
                        string[] sContainers;
                        sContainers = _envSelectedLots.SS_ContainersList;
                        _gridDefects.ClearData();
                        _gridDetails.ClearData();

                        _txtSelectionId.Data = sContainers[0].ToString();
                        //nullify the containers list
                        _envSelectedLots.SS_ContainersList = null;
                    }

                    var lotModifyDefects = (Page.FindCamstarControl("LotModifyDefects_Details")) as JQDataGrid;

                    if (IsResponsive)
                    {
                        if (lotModifyDefects.Settings.Automation == null)
                            lotModifyDefects.Settings.Automation = new GridAutomation();

                        lotModifyDefects.Settings.Automation.ShrinkColumnWidthToFit = false;
                    }
                }
            }
        }

        protected void CommandBarBehaviourCtrl()
        {
            if (IsHorizon())
            {
                _btnLotInfoCmdBar.Visible = true;
                _btnLotInfo.Visible = false;
            }
            else
            {
                _btnLotInfoCmdBar.Visible = false;
                _btnLotInfo.Visible = true;
            }
        }
        protected void GetSelectedLotDefects(string LotId)
        {
            try
            {
                // get the session and user profile
                var fs = FrameworkManagerUtil.GetFrameworkSession();

                // init service objects

                LotModifyDefectsService oService = new LotModifyDefectsService(fs.CurrentUserProfile);
                LotModifyDefects oServiceData = new LotModifyDefects();
                LotModifyDefects_Info oServiceInfo = new LotModifyDefects_Info();
                LotModifyDefects_Result oServiceResult = new LotModifyDefects_Result();

                oServiceData.Container = new ContainerRef();
                oServiceData.Container.Name = LotId;

                oServiceInfo.Container = FieldInfoUtil.RequestValue();
                oServiceInfo.LotDefects = new LotDefects_Info();
                oServiceInfo.LotDefects.DefectComment = FieldInfoUtil.RequestValue();
                oServiceInfo.LotDefects.DefectQty = FieldInfoUtil.RequestValue();
                oServiceInfo.LotDefects.DisplayName = FieldInfoUtil.RequestValue();
                oServiceInfo.LotDefects.LossReason = FieldInfoUtil.RequestValue();
                oServiceInfo.LotDefects.ProcessType = FieldInfoUtil.RequestValue();
                oServiceInfo.LotDefects.Spec = FieldInfoUtil.RequestValue();
                oServiceInfo.LotDefects.TxnTimestamp = FieldInfoUtil.RequestValue();
                oServiceInfo.LotDefects.Username = FieldInfoUtil.RequestValue();

                // init request
                LotModifyDefects_Request oServiceRequest = new LotModifyDefects_Request();
                oServiceRequest.Info = oServiceInfo;
                // execute!
                ResultStatus oResultStatus = oService.GetEnvironment(oServiceData, oServiceRequest, out oServiceResult);

                if (oResultStatus.IsSuccess)
                {
                    if (!oServiceResult.Value.LotDefects.IsNullOrEmpty())
                    {
                        Array oDefectArray = oServiceResult.Value.LotDefects.ToArray();
                        (_gridDefects.GridContext as BoundContext).Data = oDefectArray;
                        _gridDefects.BoundContext.LoadData();
                        CamstarWebControl.SetRenderToClient(_gridDefects);
                    }
                }
                else
                {
                    _gridDefects.ClearData();
                    _gridDetails.ClearData();
                    DisplayMessage(oResultStatus);
                }
            }
            catch (Exception ex)
            {
                DisplayMessage(new ResultStatus(ex.Message, false));
            }
        }
    }
}



