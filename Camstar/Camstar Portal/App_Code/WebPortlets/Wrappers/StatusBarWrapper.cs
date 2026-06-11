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
    public partial class StatusBarWrapper : WebPartWrapperBase
    {
        public StatusBarWrapper(WebPartBase webPart)
            : base(webPart)
        {
        }

        public override void Wrap(ControlCollection controls, WebPartCreateContentMethod createWebPartMethod)
        {
            bool isEmpty = string.IsNullOrEmpty((_WebPart as StatusBarControl).Message);
            bool isCatalogMode = _WebPart.Page.Manager.DisplayMode.AllowPageDesign;

            Table table = new Table(); controls.Add(table);
            table.CellPadding = 0;
            table.CellSpacing = 0;
            table.Width = Unit.Percentage(100);
            table.Attributes.Add("border", "0");
            table.Attributes.Add("id", GetWebPartTableClientID(_WebPart));

            if(isCatalogMode)
            {
                TableRow r = CreateTableRow(table);
                TableCell c = CreateTableCell(r, string.Empty);
                c.ID = _WebPart.IsEditing ? GetWebPartTitleClientID(_WebPart) : c.ID;
                WebControl span = new WebControl(HtmlTextWriterTag.Span);
                span.CssClass = "wpHeaderText";
                span.Controls.Add(new LiteralControl(_WebPart.Title));
                c.Controls.Add(span);
            }
           
            if(isCatalogMode || !isEmpty)
            {
                TableRow row = CreateTableRow(table);
                TableCell cell = CreateTableCell(row, string.Empty);
                cell.ID = _WebPart.IsEditing ? cell.ID : GetWebPartTitleClientID(_WebPart);
                createWebPartMethod(cell.Controls);
            }
        } // Wrap

        protected virtual string GetWebPartTableClientID(WebPartBase webPart)
        {
            return "WebPartTable_" + webPart.ID;
        } // GetWebPartTableClientID

        protected virtual string GetWebPartTitleClientID(WebPartBase webPart)
        {
            return "WebPartTitle_" + webPart.ID;
        } // GetWebPartTitleClientID

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
