// Copyright Siemens 2023  
using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.FormsFramework.HtmlControls;
using Camstar.WebPortal.FormsFramework.Utilities;
using System.Web;

namespace Camstar.WebPortal.WebPortlets
{
    public class ConciergeWrapper : WebPartWrapperBase
    {
        public ConciergeWrapper(WebPartBase webPart)
            : base(webPart)
        {
        }

        public override void Wrap(ControlCollection controls, WebPartCreateContentMethod createContentMethod)
        {
            LabelCache labelCache = LabelCache.GetRuntimeCacheInstance();

            DivElement mainContainer = new DivElement();
            mainContainer.AddCssClass("ui-flyout");
            controls.Add(mainContainer);

            DivElement pointer = new DivElement();
            pointer.AddCssClass("pointer");
            mainContainer.Controls.Add(pointer);

            DivElement contentContainer = new DivElement();
            contentContainer.AddCssClass("ui-flyout-container");
            mainContainer.Controls.Add(contentContainer);

            DivElement headerContainer = new DivElement();
            headerContainer.AddCssClass("ui-flyout-header");
            contentContainer.Controls.Add(headerContainer);

            SpanElement titleSpan = new SpanElement();
            titleSpan.InnerText = labelCache.GetLabelTextByName("Concierge_Concierge", val => titleSpan.InnerText = val);
            headerContainer.Controls.Add(titleSpan);

            CreateRefreshButton(headerContainer);

            _ContentPanel.CssClass = ThemeConstants.CamstarUIConciergeContent;
            createContentMethod(_ContentPanel.Controls);
            _ContentPanel.CssClass = "ui-flyout-panel";
            contentContainer.Controls.Add(_ContentPanel);
            DivElement footerContainer = new DivElement();
            footerContainer.AddCssClass("ui-flyout-footer");

            HtmlAnchor closeLink = new HtmlAnchor();
            closeLink.ID = "wrapperCloseLink";
            closeLink.Attributes["class"] = "close-concierge-button";

            if (string.Compare(HttpContext.Current.Session["CurrentTheme"].ToString(), "camstar",
                    StringComparison.InvariantCultureIgnoreCase) != 0)
            {
                headerContainer.Controls.Add(closeLink);
            }
            else
            {
                closeLink.InnerText = labelCache.GetLabelTextByName("CloseBtn", "Close", val => closeLink.InnerText = val);
                footerContainer.Controls.Add(closeLink);
            }

            contentContainer.Controls.Add(footerContainer);
        }

        public virtual Panel ContentPanel
        {
            get { return _ContentPanel; }
            set { _ContentPanel = value; }
        }

        public virtual Table ContentTable
        {
            get { return _ContentTable; }
            set { _ContentTable = value; }
        }

        protected virtual string GetWebPartTableClientID(WebPartBase webPart)
        {
            return "WebPartTable_" + webPart.ID;
        }

        protected virtual string GetWebPartTitleClientID(WebPartBase webPart)
        {
            return "WebPartTitle_" + webPart.ID;
        }

        protected virtual void CreateRefreshButton(Control owner)
        {
            _RefreshButton = new ImageButton(); 
            _RefreshButton.Attributes.Add("class", "wpRefreshButton");
            _RefreshButton.Click += new ImageClickEventHandler(RefreshButton_Click);
            _RefreshButton.ID = "RefreshButton";
            if (string.Compare(HttpContext.Current.Session["CurrentTheme"].ToString(), "camstar", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                // create helper table for js trick
                string buttonName = "refresh";
                Table buttonTable = new Table(); owner.Controls.Add(buttonTable);
                buttonTable.BorderWidth = Unit.Pixel(0);
                buttonTable.CellPadding = 0;
                buttonTable.CellSpacing = 0;
                buttonTable.Style.Add(HtmlTextWriterStyle.Display, "inline-block");
                buttonTable.Style.Add(HtmlTextWriterStyle.MarginTop, "6px");
                buttonTable.Style.Add(HtmlTextWriterStyle.MarginLeft, "5px");
                buttonTable.Style.Add(HtmlTextWriterStyle.BorderCollapse, "collapse");

                TableRow buttonRow = new TableRow();
                buttonTable.Rows.Add(buttonRow);

                TableCell buttonCell = new TableCell();
                buttonCell.Attributes.Add("webPartButtonCellFlag", "true");
                buttonRow.Cells.Add(buttonCell);
                buttonCell.Style[HtmlTextWriterStyle.Cursor] = "hand";
                buttonCell.Controls.Add(_RefreshButton);
                _RefreshButton.ImageUrl = String.Format("~/Images/Icons/{0}.png", buttonName);
                _RefreshButton.Attributes.Add("onmouseover", String.Format("javascript:this.src='{0}';", _WebPart.Page.ResolveClientUrl(string.Format("~/Images/Icons/{0}-h.png", buttonName))));
                _RefreshButton.Attributes.Add("onmouseout", String.Format("javascript:this.src='{0}';", _WebPart.Page.ResolveClientUrl(string.Format("~/Images/Icons/{0}.png", buttonName))));
            }
            else
            {
                _RefreshButton.ImageUrl = "Themes/Horizon/Images/Icons/cmdRefresh24.svg";
                owner.Controls.Add(_RefreshButton);
            }
        } // CreateRefreshButton

        protected virtual void RefreshButton_Click(object sender, ImageClickEventArgs e)
        {
            _WebPart.Page.Session[SessionConstants.IsConciergeSaved] = null;
            _WebPart.Page.Session[SessionConstants.ConciergeDataHTML] = null;

            IsRefreshing = true;
            _WebPart.Update();
        } // RefreshButton_Click

        Panel _ContentPanel = new Panel();
        Table _ContentTable = new Table();
        ImageButton _RefreshButton;
    }
}
