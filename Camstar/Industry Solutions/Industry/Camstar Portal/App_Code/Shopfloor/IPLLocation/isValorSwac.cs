// Copyright Siemens 2020  
using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Linq;
using System.Collections.Generic;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;


namespace Camstar.WebPortal.WebPortlets
{
    public class isValorSwacWP : MatrixWebPart
    {
        private double width = 1400;
        private double height = 750;
		
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            IsDirectUpdated = Page.IsPostBack;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
			if (!lblSwacContainer.Height.IsEmpty)
				height = lblSwacContainer.Height.Value - 10;
			else
				lblSwacContainer.Height = new Unit(height + 10);
			if (!lblSwacContainer.Width.IsEmpty)
				width = lblSwacContainer.Width.Value - 10;
			else
				lblSwacContainer.Width = new Unit(width + 10);
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (!Page.IsPostBack)
                RegisterSwacComponent();
        }

        private void RegisterSwacComponent()
        {
            var swacUrl = Page.DataContract.GetValueByName<string>("SwacURL");
            var componentName = Page.DataContract.GetValueByName<string>("ComponentNameDM");
            ScriptManager.RegisterStartupScript(this, Page.GetType(), "valorSwac", string.Format("isMaterialRequestStatusBoard.valorSwac('{0}', {{width: '{1}px', height: '{2}px'}}, '{3}', '{4}');", lblSwacContainer.ClientID, width, height, swacUrl, componentName), true);
        }
		
        protected override IEnumerable<ScriptReference> GetScriptReferences()
        {
            yield return new ScriptReference("~/Scripts/User/Industry Solutions/materialrequeststatusboard.js");
        }

        protected CWC.Label lblSwacContainer
        {
            get { return Page.FindCamstarControl("lblSwacContainer") as CWC.Label; }
        }

    }
}
