// Copyright Siemens 2023  
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

using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework.Utilities;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework.WebControls;

namespace Camstar.WebPortal.WebPortlets
{

    /// <summary>
    /// Used on the Server Catalog Maint VP to encrypt the password
    /// </summary>
    public class ServerCatalog : MatrixWebPart
    {
	    #region Controls

        protected virtual TextBox PasswordText
        {
            get { return Page.FindCamstarControl("ObjectChanges_Password") as TextBox; }
        }
        
        // Server Details - Is Local System
        protected virtual CWC.CheckBox IsLocalSystem
        {
            get { return Page.FindCamstarControl("ObjectChanges_IsSourceSystem") as CWC.CheckBox; }
        }

        // Server Details - Is Not a Camstar Server
        protected virtual CWC.CheckBox IsNotACamstarServer
        {
            get { return Page.FindCamstarControl("ObjectChanges_IsNotACamstarServer") as CWC.CheckBox; }
        }

        // Server Details - MDB Version
        protected virtual CWC.TextBox MDBVersion
        {
            get { return Page.FindCamstarControl("ObjectChanges_MDBVersion") as CWC.TextBox; }
        }

        // Server Details - Camstar Version
        protected virtual CWC.TextBox CamstarVersion
        {
            get { return Page.FindCamstarControl("ObjectChanges_CamstarVersion") as CWC.TextBox; }
        }

        // Connectivity - Port
        protected virtual CWC.TextBox Port
        {
            get { return Page.FindCamstarControl("ObjectChanges_Port") as CWC.TextBox; }
        }

        // Connectivity - IP Address
        private CWC.TextBox IPAddress
        {
            get { return Page.FindCamstarControl("ObjectChanges_IPAddress") as CWC.TextBox; }
        }

        #endregion  
     
        #region Protected Functions 
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            IsLocalSystem.DataChanged += IsLocalSystem_DataChanged;
            IsNotACamstarServer.DataChanged += IsNotACamstarServer_DataChanged;
        }

        // Make the IsLocalSystem and IsNotACamstarServer CheckBox controls mutually exclusive ...

        protected virtual void IsLocalSystem_DataChanged(object sender, EventArgs e)
        {
            if (IsLocalSystem.IsChecked == true)
            {
                MDBVersion.Enabled = true;
                CamstarVersion.Enabled = true;
                Port.Enabled = true;

                if (IsNotACamstarServer.IsChecked == true)
                {
                    IsNotACamstarServer.Data = false;

                    MDBVersion.ClearData();
                    CamstarVersion.ClearData();
                    Port.ClearData();
                }
            }
        }

        protected virtual void IsNotACamstarServer_DataChanged(object sender, EventArgs e)
        {
            if(IsNotACamstarServer.IsChecked == true)
            {
                if (IsLocalSystem.IsChecked == true)
                {
                    IsLocalSystem.Data = false;
                }
                string na = FrameworkManagerUtil.GetLabelValue("Lbl_NA") ?? string.Empty;
                MDBVersion.Data = na;
                MDBVersion.Enabled = false;

                CamstarVersion.Data = na;
                CamstarVersion.Enabled = false;

                Port.Data = portDefault;
                Port.Enabled = false;
            }
            else
            {
                MDBVersion.Enabled = true;
                MDBVersion.ClearData();

                CamstarVersion.Enabled = true;
                CamstarVersion.ClearData();

                Port.Enabled = true;
                Port.ClearData();
            }
        }

                
        #endregion

        #region Public Functions

        public override void GetInputData(Service serviceData)
        {
            base.GetInputData(serviceData);
            TargetSystemMaint changes = (TargetSystemMaint)serviceData;
            if (changes.ObjectChanges != null)
            {
                if (changes.ObjectChanges.Password != null)
                {
                    if (PasswordText.Data != null)
                    {
                        changes.ObjectChanges.Password = Camstar.Util.CryptUtil.Encrypt(PasswordText.Data.ToString());
                    }
                    else
                    {
                        LabelCache cache = FrameworkManagerUtil.GetLabelCache(Page.Session);
                        if (cache != null)
                        {
                            WCF.ObjectStack.Label fieldReqLabel = new WCF.ObjectStack.Label();
                            fieldReqLabel = cache.GetLabelByName("Lbl_FieldIsRequired");
                            Page.StatusBar.WriteError(fieldReqLabel.Value);
                        }
                    }
                }
            }
        }

        #endregion

        #region Private Functions

        #endregion

        #region Constants
        private const string portDefault = "0000";
        #endregion

	    #region Private Member Variables

        #endregion

    }

}

