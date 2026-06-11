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
    public class RDOCopyControl : MatrixWebPart
    {
        /// <summary>
        /// Specifies Title property of the web part.
        /// </summary>
        public RDOCopyControl()
        {
            Title = "RDO Copy";
            TitleLabel = "UIWebPart_RDOCopy";
        }

        /// <summary>
        /// Creates webpart controls and defines their layout.
        /// </summary>
        protected override void AddFieldControls()
        {
            this.ControlAlignment = ControlAlignmentType.LabelLeftInputRight;
            _nameField.Text = "RDO Name here";
            _nameField.Width = 330;
            _nameField.TextControl.Width = 280;
            _nameField.Required = true;
            this[1, 0] = _nameField;
            //SetSpan(1, 0, 2);

            _revisionField.LabelName = "NewRevisionNameLabel";
            _revisionField.Width = 200;
            _revisionField.TextControl.Width = 180;
            _revisionField.Required = true;
            this[2, 0] = _revisionField;
            //SetSpan(2, 0, 2);

            _actionField.Visible = false;
            this[3, 0] = _actionField;
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
                _actionField.Data = Page.Request.QueryString[QueryStringConstants.CurrentAction].ToString();

                if (string.Compare("CopyRDO", Page.Request.QueryString[QueryStringConstants.CurrentAction].ToString()) == 0)
                    _copyRDO = true;
                else
                    _copyRDO = false;

                if (_copyRDO)
                {
                    _nameField.TextControl.Text = _kCopyOf + Page.Request.QueryString[QueryStringConstants.InstanceName].ToString();
                    _nameField.Enabled = true;
                    _revisionField.TextControl.Text = Page.Request.QueryString[QueryStringConstants.InstanceRev].ToString();
                }
                else
                {
                    _nameField.TextControl.Text = Page.Request.QueryString[QueryStringConstants.InstanceName].ToString();
                    _nameField.Enabled = false;
                    _revisionField.TextControl.Text = _kCopyOf + Page.Request.QueryString[QueryStringConstants.InstanceRev].ToString();
                }
            }
            base.OnLoad(e);
        }

        public CWC.TextBox _nameField = new CWC.TextBox();
        public CWC.TextBox _revisionField = new CWC.TextBox();
        public CWC.TextBox _actionField = new CWC.TextBox();

        private bool _copyRDO = true;

        private const string _kCopyOf = "Copy of ";
    }
}
