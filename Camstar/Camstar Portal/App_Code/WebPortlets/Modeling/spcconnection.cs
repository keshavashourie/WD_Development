// Copyright Siemens 2023
using System;
using System.Linq;
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.Personalization;
using System.Collections.Generic;
/// <summary>
/// Summary description for SPCChartModeling
/// </summar
namespace Camstar.WebPortal.WebPortlets.Modeling
{
    public class SPCConnection : MatrixWebPart
    {
        #region Properties
        protected virtual TextBox PasswordText { get { return Page.FindCamstarControl("ObjectChanges_Password") as TextBox; } }
        #endregion

        #region Protected Methods
        public override void GetInputData(Service serviceData)
        {
            base.GetInputData(serviceData);
            SPCConnectionMaint changes = (SPCConnectionMaint)serviceData;
            if (changes.ObjectChanges != null)
            {
                if (changes.ObjectChanges.Password != null)
                {
                    if (PasswordText.Data != null)
                    {
                        changes.ObjectChanges.Password = Camstar.Util.CryptUtil.Encrypt(PasswordText.Data.ToString());
                    }
                }
            }

        }
        #endregion
    }
}
