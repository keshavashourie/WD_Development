// Copyright Siemens 2023  
using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using Camstar.WebPortal.Constants;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework.Utilities;
using CWF = Camstar.WebPortal.FormsFramework;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.Utilities;

namespace Camstar.WebPortal.WebPortlets
{
    /// <summary>
    /// "Event" quality object header webpart.
    /// </summary>
    public class NDOCopyControl : MatrixWebPart
    {
        /// <summary>
        /// Specifies Title property of the web part.
        /// </summary>
        public NDOCopyControl()
        {
            Title = "NDO Copy";
            TitleLabel = "UIWebPart_NDOCopy";
        }

        /// <summary>
        /// Creates webpart controls and defines their layout.
        /// </summary>
        protected override void AddFieldControls()
        {
            this.ControlAlignment = ControlAlignmentType.LabelLeftInputRight;
            _nameField.Text = "NDO Name here";
            _nameField.Width = 330;
            _nameField.TextControl.Width = 280;
            _nameField.Required = true;
            this[1, 0] = _nameField;
        }

        protected override void OnLoad(EventArgs e)
        {
            if (Page.Request.QueryString[QueryStringConstants.CDOType] != null && !Page.IsPostBack)
            {
                var cdoType = Page.Request.QueryString[QueryStringConstants.CDOType];
                if (!string.IsNullOrEmpty(cdoType))
                {
                    LabelCache labelCache = FrameworkManagerUtil.GetLabelCache(System.Web.HttpContext.Current.Session);
                    if (labelCache != null)
                    {
                        Camstar.WCF.ObjectStack.Label label = labelCache.GetLabelByName(cdoType);
                        if (label != null)
                            cdoType = label.Value;
                    }
                }
                _nameField.Text = cdoType;
                _nameField.TextControl.Text = _kCopyOf + Page.Request.QueryString[QueryStringConstants.InstanceName].ToString();
            }
            base.OnLoad(e);
        }

        public CWC.TextBox _nameField = new CWC.TextBox();

        private const string _kCopyOf = "Copy of ";

    }
}
