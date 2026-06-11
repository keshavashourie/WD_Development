/* Copyright 2019 Siemens */
using System;
using System.Collections.Generic;
using System.Linq;
using Camstar.WebPortal.WebPortlets;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.Utilities;
using Camstar.WCF.Services;
using System.Web;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.FormsFramework.WebControls;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;

/// <summary>
/// Summary description for SS_ViewLotStatusVP
/// </summary>
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_ViewLotStatusVP : MatrixWebPart
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

        #endregion

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!Page.IsPostBack)
                _txtComputerNameField.Data = SEMI.AppCode.UIUtility.GetComputerName(this);

            if (_txtContainerField.Data != null)
            {
                _btnLotAttributes.Enabled = true;
                _btnOnlineTraveler.Enabled = true;
                _btnDocumentSets.Enabled = true;
                _btnMaterialsRequired.Enabled = true;
            }
            _txtContainerField.TextChanged += new EventHandler(ContainerField_TextChanged);
        }

        public void ContainerField_TextChanged(object sender, EventArgs e)
        {
            if (_txtContainerField.Data != null)
                _ctlContainerField.Data = _txtContainerField.Data.ToString();
            else
            {
                ClearValues();
                _btnLotAttributes.Enabled = false;
                _btnOnlineTraveler.Enabled = false;
                _btnDocumentSets.Enabled = false;
                _btnMaterialsRequired.Enabled = false;
            }
        }
    }
}



