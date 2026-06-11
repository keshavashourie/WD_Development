using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.WebPortlets;

namespace Helpers
{
    /// <summary>
    /// Summary description for TableLayoutBuilder
    /// </summary>
    public class TableLayoutBuilder : IMatrixBuilder
    {
        public virtual WebControl BuildLayoutMatrix(List<MatrixWebPartItem> items, int maxCols, int maxRows)
        {
            Table container = new Table();
            int rowNumber = items.Max(it => it.RowPosition) + 1;
            int columnNumber = items.Max(it => it.ColPosition) + 1;

            if (columnNumber < maxCols)
                columnNumber = maxCols;

            TableRow row;
            TableCell cell;

            List<MatrixWebPartItem> spanItems = new List<MatrixWebPartItem>();

            for (int x = 0; x < rowNumber; x++)
            {
                row = new TableRow();
                container.Rows.Add(row);
                for (int y = 0; y < columnNumber; y++)
                {
                    cell = new TableCell();
                    cell.CssClass = "cell";
                    row.Cells.Add(cell);
                    MatrixWebPartItem item = items.FirstOrDefault(it => it.RowPosition == x && it.ColPosition == y);

                    if (item != null)
                    {
                        foreach (var ci in item.Controls)
                        {
                            cell.Controls.Add(ci);
                        }
                        if (item.RowSpan > 1 || item.ColSpan > 1)
                            spanItems.Add(item);

                        cell.ColumnSpan = item.ColSpan;
                        cell.RowSpan = item.RowSpan;
                        // apply styles
                        if (item.Style != null)
                        {
                            if (!item.Style.Orientation.HasValue)
                                item.Style.Orientation = default(Orientations);
                            if (item.Style.Orientation == Orientations.Vertical)
                                cell.Attributes["orientation"] = "vertical";
                            else if (item.Style.Orientation == Orientations.Horizontal)
                                cell.Attributes["orientation"] = "horizontal";

                            cell.CssClass = string.Format("{0} {1}", cell.CssClass, item.Style.CSSClass);

                            if (item.Style.Padding != null && (item.Style.Padding.Top > 0 || item.Style.Padding.Right > 0 || item.Style.Padding.Bottom > 0 || item.Style.Padding.Left > 0))
                            {
                                string padding = string.Format("{0}px {1}px {2}px {3}px", item.Style.Padding.Top, item.Style.Padding.Right, item.Style.Padding.Bottom, item.Style.Padding.Left);
                                cell.Style.Add(HtmlTextWriterStyle.Padding, padding);
                            }

                            if (!item.Style.HorizontalAlignment.HasValue)
                                item.Style.HorizontalAlignment = default(HorizontalAlignment);
                            if (!item.Style.VerticalAlignment.HasValue)
                                item.Style.VerticalAlignment = default(VerticalAlignment);

                            if (item.Style.HorizontalAlignment != HorizontalAlignment.Left && item.Style.HorizontalAlignment != HorizontalAlignment.NotSet)
                            {
                                string ta = item.Style.HorizontalAlignment != HorizontalAlignment.Middle ? item.Style.HorizontalAlignment.ToString().ToLower() : "center";
                                cell.Style.Add(HtmlTextWriterStyle.TextAlign, ta);
                            }

                            if (item.Style.VerticalAlignment != VerticalAlignment.NotSet)
                            {
                                cell.Style.Add(HtmlTextWriterStyle.VerticalAlign, item.Style.VerticalAlignment.ToString().ToLower());
                            }
                        }
                    }
                }
            }

            List<TableCell> cellItems = new List<TableCell>();

            // apply column and row spans
            foreach (MatrixWebPartItem item in spanItems.OrderBy(span => span.RowPosition).ThenByDescending(span => span.ColPosition))
            {
                if (item.ColSpan > 1 && container.Rows[item.RowPosition].Cells.Count - item.ColPosition + 1 >= item.ColSpan)
                {
                    container.Rows[item.RowPosition].Cells[item.ColPosition].ColumnSpan = item.ColSpan;

                    for (int x = 1; x < item.ColSpan; x++)
                    {
                        if ((item.ColPosition + x) < container.Rows[item.RowPosition].Cells.Count)
                            cellItems.Add(container.Rows[item.RowPosition].Cells[item.ColPosition + x]);
                    }
                }

                if (item.RowSpan > 1 && container.Rows.Count - item.RowPosition + 1 >= item.RowSpan)
                {
                    container.Rows[item.RowPosition].Cells[item.ColPosition].RowSpan = item.RowSpan;

                    for (int x = 1; x < item.RowSpan; x++)
                    {
                        if (container.Rows.Count > item.RowPosition + x)
                            if (container.Rows[item.RowPosition + x].Cells.Count > item.ColPosition)
                                cellItems.Add(container.Rows[item.RowPosition + x].Cells[item.ColPosition]);
                    }
                }
            }

            for (int i = container.Rows.Count - 1; i >= 0; i--)
            {
                for (int j = container.Rows[i].Cells.Count - 1; j >= 0; j--)
                {
                    if (cellItems.Contains(container.Rows[i].Cells[j]))
                        container.Rows[i].Cells.Remove(container.Rows[i].Cells[j]);
                }
            }

            return container;
        }
    }
}