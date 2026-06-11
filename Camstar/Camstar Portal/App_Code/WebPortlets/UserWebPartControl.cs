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

using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.Constants;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework;


namespace Camstar.WebPortal.WebPortlets
{
    public class UserWebPartControl : WebPartBase
    {
        public UserWebPartControl()
        {
                Title = "User Control";
        }

        [WebProperty()]
        [WebDisplayName("User Control Path")]
        [WebDescription("URL to a user control")]
        public virtual string UserControlPath
        {
            get { return _UserControlPath; }
            set
            {
                if(value != null)
                {
                    _UserControlPath = value;
                    _RequresChildRecreation = _RequresChildRecreation || ChildControlsCreated;
        }
            }
        } // UserControlPath

        protected override void CreateContentControls(ControlCollection contentControls)
        {
            Table table = new Table(); contentControls.Add(table);
            table.Width = Camstar.WebPortal.Constants.UnitValues.Percent100;
            table.Height = Camstar.WebPortal.Constants.UnitValues.Percent100;
            TableRow row = new TableRow(); table.Rows.Add(row);
            TableCell cell = new TableCell(); row.Cells.Add(cell);
            cell.HorizontalAlign = HorizontalAlign.Left;
            cell.Width = Camstar.WebPortal.Constants.UnitValues.Percent100;
            cell.Height = Camstar.WebPortal.Constants.UnitValues.Percent100; 

            _UserControlPlaceHolder = cell;

            CreateUserControl();
        } // CreateContentControls

        protected virtual void CreateUserControl()
        {
            string ascxPath;
            if (!string.IsNullOrEmpty(UserControlPath) &&
                System.IO.File.Exists(Page.Server.MapPath(UserControlPath)))
            {
                ascxPath = UserControlPath;
            }
            else
            {
                ascxPath = ResolveClientUrl("~/Controls/DummyUserControl.ascx"); 
            }

            Control ctrl = userControl = Page.LoadControl(ascxPath);
            if (ctrl != null)
            {
                _UserControlPlaceHolder.Controls.Clear();
                _UserControlPlaceHolder.Controls.Add(ctrl);
            }
        } // GreateUserControl

        private Control userControl;
        public virtual Control UserControl
        {
            get { return userControl; }
        }

        protected virtual Control FindUserControl(Camstar.WebPortal.FormsFramework.CamstarControlsCollection controls)
        {
            Control control = null;

            foreach (CamstarControlProxy proxy in controls)
            {
                Control ctrl = proxy.Control;
                if (ctrl != null && ctrl is CompositeUserControl)
                {
                    control = ctrl;
                    break;
                }
            }

            return control;
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if(_RequresChildRecreation)
            {
                CreateUserControl();
        }
        } // OnPreRender

        private Control _UserControlPlaceHolder = null;
        private string _UserControlPath = string.Empty;
        private bool _RequresChildRecreation = false;
    }
}
