/* Copyright 2019 Siemens */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Collections;

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
using SEMI.AppCode;
using SWC = System.Web.UI.WebControls;

/// <summary>
/// Summary description for SS_ViewLotStatusVP
/// </summary>
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_ViewLotStatusVPR2 : MatrixWebPart
    {
        #region Properties

        // ContainerLists
        CWC.ContainerList _ctlContainerField { get { return Page.FindCamstarControl("ViewContainerStatus_Container") as CWC.ContainerList; } }
        // TextBoxs
        CWC.TextBox _txtComputerNameField { get { return Page.FindCamstarControl("ViewContainerStatus_ComputerNameField") as CWC.TextBox; } }
        CWC.TextBox _txtContainerField { get { return Page.FindCamstarControl("ViewContainerStatus_ContainerField") as CWC.TextBox; } }
        //Buttons
        CWC.Button _btnLotAttributes { get { return Page.FindCamstarControl("ViewContainerStatus_LotAttributesButton") as CWC.Button; } }
        CWC.Button _btnOnlineTraveler { get { return Page.FindCamstarControl("ViewContainerStatus_OnlineTravelerButton") as CWC.Button; } }
        CWC.Button _btnDocumentSets { get { return Page.FindCamstarControl("ViewContainerStatus_DocumentSetsButton") as CWC.Button; } }
        CWC.Button _btnMaterialsRequired { get { return Page.FindCamstarControl("ViewContainerStatus_MaterialsRequiredButton") as CWC.Button; } }

        CWC.Button _btnLotAttributesCmdBar { get { return Page.FindCamstarControl("Lot Attribute") as CWC.Button; } }
        CWC.Button _btnOnlineTravelerCmdBar { get { return Page.FindCamstarControl("OnlineTraveler") as CWC.Button; } }
        CWC.Button _btnDocumentSetsCmdBar { get { return Page.FindCamstarControl("Document") as CWC.Button; } }
        CWC.Button _btnMaterialsRequiredCmdBar { get { return Page.FindCamstarControl("RequiredMaterial") as CWC.Button; } }

        #endregion

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            HideMoveLotDetailIcon();

            if (!Page.IsPostBack)
            {
                _txtComputerNameField.Data = SEMI.AppCode.UIUtility.GetComputerName(this);
            }

            _txtContainerField.TextChanged += new EventHandler(ContainerField_TextChanged);
        }

        public void ContainerField_TextChanged(object sender, EventArgs e)
        {
            if (_txtContainerField.Data != null)
            {
                GetData(_txtContainerField.Data.ToString());
            }
            else
            {
                ClearValues();

                if (!IsHorizon())
                {
                    _btnLotAttributes.Enabled = false;
                    _btnOnlineTraveler.Enabled = false;
                    _btnDocumentSets.Enabled = false;
                    _btnMaterialsRequired.Enabled = false;
                }
            }
        }

        public void HideMoveLotDetailIcon()
        {
            if (IsHorizon())
            {
                //hide classic button
                _btnLotAttributes.Visible = false;
                _btnOnlineTraveler.Visible = false;
                _btnDocumentSets.Visible = false;
                _btnMaterialsRequired.Visible = false;
            }
            else
            {
                //hide horizon cmd bar button
                _btnLotAttributesCmdBar.Visible = false;
                _btnOnlineTravelerCmdBar.Visible = false;
                _btnDocumentSetsCmdBar.Visible = false;
                _btnMaterialsRequiredCmdBar.Visible = false;
            }
        }

        protected bool IsHorizon()
        {
            var theme = Page.Session["CurrentTheme"] != null ? Page.Session["CurrentTheme"].ToString() : string.Empty;

            if (theme.ToLower() == "horizon")
            {
                return true;
            }
            return false;
        }

        private void GetData(string lot)
        {
            try
            {
                OM.ViewContainerStatus_Info oServiceInfo = new OM.ViewContainerStatus_Info();
                ViewContainerStatus_Result resultData = new ViewContainerStatus_Result();
                UserProfile profile = HttpContext.Current.Session[Camstar.WebPortal.Constants.SessionConstants.UserProfile] as UserProfile;
                ViewContainerStatusService svc = new ViewContainerStatusService(profile);
                ViewContainerStatus oServiceData = new ViewContainerStatus();
                ViewContainerStatus_Request reqData = new ViewContainerStatus_Request();


                oServiceData.Container = new ContainerRef(lot);
                oServiceInfo.Container = FieldInfoUtil.RequestValue();

                reqData.Info = oServiceInfo;
                ResultStatus results = svc.ExecuteTransaction(oServiceData, reqData, out resultData);

                if (results.IsSuccess)
                {
                    if (resultData.Value.Container != null)
                        _ctlContainerField.Data = resultData.Value.Container.Name;

                    if (!IsHorizon())
                    {
                        _btnLotAttributes.Enabled = true;
                        _btnOnlineTraveler.Enabled = true;
                        _btnDocumentSets.Enabled = true;
                        _btnMaterialsRequired.Enabled = true;
                    }
                }
                else
                {
                    DisplayMessage(results);
                    ClearValues();

                    if (!IsHorizon())
                    {
                        _btnLotAttributes.Enabled = false;
                        _btnOnlineTraveler.Enabled = false;
                        _btnDocumentSets.Enabled = false;
                        _btnMaterialsRequired.Enabled = false;
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayMessage(new ResultStatus(ex.Message, false));
            }
        }
    }
}



