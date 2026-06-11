/* Copyright 2022 Siemens */
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
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.Personalization;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.PortalFramework;

namespace Camstar.WebPortal.WebPortlets.Parts
{
    public class PartCreate : MatrixWebPart
    {
        //Controls declaration
        private CWC.TextBox _txtName { get { return Page.FindCamstarControl("PartDetails_Name") as CWC.TextBox; } }
        private CWC.RevisionedObject _rdoProduct { get { return Page.FindCamstarControl("PartDetails_Product") as CWC.RevisionedObject; } }
        private CWC.NamedObject _ndoResourceFamily { get { return Page.FindCamstarControl("PartDetails_ResourceFamily") as CWC.NamedObject; } }
        private CWC.TextBox _txtPartQty { get { return Page.FindCamstarControl("PartDetails_PartQty") as CWC.TextBox; } }
        private CWC.NamedObject _ndoFactory { get { return Page.FindCamstarControl("PartDetails_Factory") as CWC.NamedObject; } }
        private CWC.NamedObject _ndoVendor { get { return Page.FindCamstarControl("PartDetails_Vendor") as CWC.NamedObject; } }
        private CWC.TextBox _txtVendorModel { get { return Page.FindCamstarControl("PartDetails_VendorModel") as CWC.TextBox; } }
        private CWC.TextBox _txtVendorSerialNumber { get { return Page.FindCamstarControl("PartDetails_VendorSerialNumber") as CWC.TextBox; } }
        private CWC.DateChooser _datePartExpiryDate { get { return Page.FindCamstarControl("PartDetails_PartExpiryDate") as CWC.DateChooser; } }
        private CWC.NamedObject _ndoPhysicalLocation { get { return Page.FindCamstarControl("PartDetails_PhysicalLocation") as CWC.NamedObject; } }
        private CWC.NamedObject _ndoPhysicalPosition { get { return Page.FindCamstarControl("PartDetails_PhysicalPosition") as CWC.NamedObject; } }
        private CWC.NamedObject _ndoResourceStatusCode { get { return Page.FindCamstarControl("PartCreate_ResourceStatusCode") as CWC.NamedObject; } }
        private CWC.TextBox _txtComments { get { return Page.FindCamstarControl("Shopfloor_Comments") as CWC.TextBox; } }
        private ToggleContainer _toggleComments { get { return Page.FindCamstarControl("PartCreateComments_Toggle") as ToggleContainer; } }
        //Hidden controls declaration
        private CWC.RevisionedObject _rdoHiddenProduct { get { return Page.FindCamstarControl("Hidden_Product") as CWC.RevisionedObject; } }
        private CWC.NamedObject _ndoHiddenResourceFamily { get { return Page.FindCamstarControl("Hidden_ResourceFamily") as CWC.NamedObject; } }
        private CWC.NamedObject _ndoHiddenFactory { get { return Page.FindCamstarControl("Hidden_Factory") as CWC.NamedObject; } }
        private CWC.NamedObject _ndoHiddenVendor { get { return Page.FindCamstarControl("Hidden_Vendor") as CWC.NamedObject; } }
        private CWC.TextBox _txtHiddenVendorModel { get { return Page.FindCamstarControl("Hidden_VendorModel") as CWC.TextBox; } }
        private CWC.TextBox _txtHiddenVendorSerialNumber { get { return Page.FindCamstarControl("Hidden_VendorSerialNumber") as CWC.TextBox; } }
        private CWC.DateChooser _dateHiddenPartExpiryDate { get { return Page.FindCamstarControl("Hidden_PartExpiryDate") as CWC.DateChooser; } }
        private CWC.NamedObject _ndoHiddenPhysicalLocation { get { return Page.FindCamstarControl("Hidden_PhysicalLocation") as CWC.NamedObject; } }
        private CWC.NamedObject _ndoHiddenPhysicalPosition { get { return Page.FindCamstarControl("Hidden_PhysicalPosition") as CWC.NamedObject; } }
        private CWC.NamedObject _ndoHiddenResourceStatusCode { get { return Page.FindCamstarControl("Hidden_ResourceStatusCode") as CWC.NamedObject; } }
        //------------------------
        // Web Part Custom Action
        //------------------------

        //-----------------------------------------
        // Part create details popup
        //-----------------------------------------
        public virtual void PopupPartCreateDetails(bool EndResponse = false)
        {
            try
            {
                if (_rdoProduct.Data != null)
                {
                    _rdoProduct.Data = Page.DataContract.GetValueByName("MaterialPartName_Create");
                    _rdoProduct.RevisionValue = Page.DataContract.GetValueByName("MaterialPartRevision_Create").ToString();
                    _ndoResourceFamily.Data = Page.DataContract.GetValueByName("ResourceFamily_Create");
                    _txtPartQty.Data = Page.DataContract.GetValueByName("PartQty_Create");
                    _ndoFactory.Data = Page.DataContract.GetValueByName("Factory_Create");
                    _ndoVendor.Data = Page.DataContract.GetValueByName("Vendor_Create");
                    _txtVendorModel.Data = Page.DataContract.GetValueByName("VendorModel_Create");
                    _txtVendorSerialNumber.Data = Page.DataContract.GetValueByName("VendorSerialNumber_Create");
                    _datePartExpiryDate.Data = Page.DataContract.GetValueByName("PartExpiryDate_Create");
                    _ndoPhysicalLocation.Data = Page.DataContract.GetValueByName("PhysicalLocation_Create");
                    _ndoPhysicalPosition.Data = Page.DataContract.GetValueByName("PhysicalPosition_Create");
                    _ndoResourceStatusCode.Data = Page.DataContract.GetValueByName("ResourceStatusCode_Create");
                }
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

        //------------------------
        //
        //------------------------
        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e) //Custom action settings
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as CustomAction;
            if (action != null)
            {
                if (action.Parameters == "ResetFields")
                {
                    _txtName.ClearData();
                    _txtComments.ClearData();
                    _toggleComments.Reset();
                    PopupPartCreateDetails();
                }
                else if (action.Parameters == "ResetPage")
                    Page.ShopfloorReset(sender, e);
            }
        }

        //---------------------------------------------------
        // Override OnLoad event
        //---------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);
                if (!Page.IsPostBack)
                {
                    ScriptManager.RegisterStartupScript(this.Page.Form, this.Page.GetType(), "PopUpMaximize", "pop.GetCallerPage().pop.maximize();", true);
                    if (_rdoProduct.Data != null)
                    {
                        _rdoProduct.Data = Page.DataContract.GetValueByName("MaterialPartName_Create");

                        if (_rdoProduct.RevisionValue != null)
                        {
                            _rdoProduct.RevisionValue = Page.DataContract.GetValueByName("MaterialPartRevision_Create").ToString();
                            _rdoProduct.RevisionOfRecord = false;
                        }
                    }

                    Personalization.UIAction[] actUIActions = Page.ActionDispatcher.PageActions();
                    foreach (Personalization.UIAction act in actUIActions)
                    {
                        switch (act.Name)
                        {
                            case "btnReload":
                                {
                                    if (!Page.IsAJAXFloatingFrame)
                                    {
                                        act.IsHidden = true;
                                        act.IsDisabled = true;
                                    }
                                    break;
                                }

                            case "btnReset":
                                {
                                    if (Page.IsAJAXFloatingFrame)
                                    {
                                        act.IsHidden = true;
                                        act.IsDisabled = true;
                                    }
                                    break;
                                }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }
    }
}




