// Copyright Siemens 2023  
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls.WebParts;

using Camstar.WebPortal.PortalFramework;

namespace Camstar.WebPortal.WebPortlets
{
    public class ButtonsBarWrapper : WebPartWrapperBase
    {
        public ButtonsBarWrapper(WebPartBase webPart)
            : base(webPart)
        {
        }

        public override void Wrap(ControlCollection controls, WebPartCreateContentMethod createContentMethod)
        {
            bool isEmpty = !(_WebPart as IButtonsBarBase).HasButtons;
            bool isCatalogMode = _WebPart.Page.Manager.DisplayMode.AllowPageDesign;

            Table table = new Table(); controls.Add(table);
            table.CellPadding = 0;
            table.CellSpacing = 0;
            table.Width = Unit.Percentage(100);
            table.Attributes.Add("id", GetWebPartTableClientID(_WebPart));

            if(isCatalogMode)
            {
                TableRow r = CreateTableRow(table);
                TableCell c = CreateTableCell(r, string.Empty);
                c.ID = _WebPart.IsEditing ? GetWebPartTitleClientID(_WebPart) : c.ID;
                WebControl span = new WebControl(HtmlTextWriterTag.Span);
                span.CssClass = "wpHeaderText";
                span.Controls.Add(new LiteralControl(_WebPart.Title));
                span.Style.Add(HtmlTextWriterStyle.Cursor, "move");
                c.Controls.Add(span);
            }

            if(isCatalogMode || !isEmpty)
            {
                TableRow row = CreateTableRow(table);
                TableCell cell = CreateTableCell(row, string.Empty);
                cell.ID = _WebPart.IsEditing ? cell.ID : GetWebPartTitleClientID(_WebPart);
                createContentMethod(cell.Controls);

                if(isEmpty)
                {
                    cell.Style.Add(HtmlTextWriterStyle.Height, "20px");
                    cell.Style.Add(HtmlTextWriterStyle.BackgroundColor, "#f0f0f0");
                }

                if(isCatalogMode)
                {
                    TableCell buttonsCell = CreateTableCell(row, string.Empty);
                    buttonsCell.Style.Add(HtmlTextWriterStyle.Position, "relative");
                    Panel buttonsPanel = new Panel();
                    buttonsCell.Controls.Add(buttonsPanel);
                    buttonsPanel.Style.Add(HtmlTextWriterStyle.Position, "absolute");
                    buttonsPanel.Style.Add(HtmlTextWriterStyle.WhiteSpace, "nowrap");
                    buttonsPanel.Style.Add("right", "0px");
                    buttonsPanel.Style.Add(HtmlTextWriterStyle.MarginTop, "-7px");
                    buttonsPanel.Controls.Add(CreateButton("edit", _WebPart, "wpEditButton"));
                    buttonsPanel.Controls.Add(CreateButton("close", _WebPart, "wpCloseButton"));
                }
            }
        }

        protected virtual string GetWebPartTableClientID(WebPartBase webPart)
        {
            return "WebPartTable_" + webPart.ID;
        }

        protected virtual string GetWebPartTitleClientID(WebPartBase webPart)
        {
            return "WebPartTitle_" + webPart.ID;
        }

        protected virtual Control CreateButton(string buttonName, WebPartBase webPart, string cssName)
        {
            // create helper table for js trick
            Table buttonTable = new Table();
            buttonTable.BorderWidth = Unit.Pixel(0);
            buttonTable.CellPadding = 0;
            buttonTable.CellSpacing = 0;
            buttonTable.Style.Add(HtmlTextWriterStyle.Display, "inline");
            buttonTable.Style.Add(HtmlTextWriterStyle.BorderCollapse, "collapse");

            TableRow buttonRow = new TableRow();
            buttonTable.Rows.Add(buttonRow);

            TableCell buttonCell = new TableCell();
            buttonCell.Attributes.Add("webPartButtonCellFlag", "true");
            buttonRow.Cells.Add(buttonCell);
            buttonCell.Style[HtmlTextWriterStyle.Cursor] = "hand";

            HtmlImage img = new HtmlImage();
            buttonCell.Controls.Add(img);
            img.Src = String.Format("~/Images/Icons/{0}.png", buttonName);

            img.Attributes.Add("onmouseover", String.Format("javascript:this.src='{0}';", webPart.Page.ResolveClientUrl(string.Format("~/Images/Icons/{0}-h.png", buttonName))));
            img.Attributes.Add("onmouseout", String.Format("javascript:this.src='{0}';", webPart.Page.ResolveClientUrl(string.Format("~/Images/Icons/{0}.png", buttonName))));
            
            img.Attributes.Add("onclick", String.Format("__wpm.SubmitPage('{0}', '{2}:{1}');", webPart.Zone.ClientID.Replace('_', '$'), webPart.ID, buttonName));
            img.Attributes.Add("class", cssName);

            return buttonTable;
        } // CreateButton

        protected virtual TableRow CreateTableRow(Table table)
        {
            TableRow row = new TableRow();
            table.Rows.Add(row);

            return row;
        } // CreateTableRow

        protected virtual TableCell CreateTableCell(TableRow row, string cssClass)
        {
            TableCell cell = new TableCell();
            row.Cells.Add(cell);
            cell.CssClass = cssClass;

            return cell;
        } // CreateTableCell

        protected virtual TableCell CreateTableCell(TableRow row, string cssClass, Control childControl)
        {
            TableCell cell = CreateTableCell(row, cssClass);
            cell.Controls.Add(childControl);

            return cell;
        } // CreateTableCell
    }
}
