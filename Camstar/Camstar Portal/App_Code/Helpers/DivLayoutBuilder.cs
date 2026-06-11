using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.WebPortlets;
using Camstar.WebPortal.FormsFramework.WebControls;

namespace Helpers
{
    /// <summary>
    /// Summary description for DivLayoutBuilder
    /// </summary>
    public class DivLayoutBuilder : IMatrixBuilder
    {
        private MatrixWebPartItem GetItem(List<MatrixWebPartItem> items, int r, int c)
        {
            return items.FirstOrDefault(it =>
            {
                if (it.ColPosition == c)
                {
                    if (it.RowSpan > 1)
                    {
                        return r >= it.RowPosition && r <= (it.RowPosition + it.RowSpan - 1);
                    }
                    else
                    {
                        return it.RowPosition == r;
                    }
                }
                return false;
            });
        }

        public WebControl BuildLayoutMatrix(List<MatrixWebPartItem> items, int maxCols, int maxRows)
        {
            var container = new DivBlock();
            int rowNumber = items.Max(it => it.RowPosition) + 1;
            int columnNumber = items.Max(it => it.ColPosition) + 1;

            if (columnNumber < maxCols)
                columnNumber = maxCols;

            List<MatrixWebPartItem> spanItems = new List<MatrixWebPartItem>();

            for (int r = 0; r < rowNumber; r++)
            {
                var row = new DivBlock(null, "row");
                row.Attr("key", string.Format("{0}_r{1}", Id, r));
                container.Controls.Add(row);

                for (int c = 0; c < columnNumber; c++)
                {
                    var cell = new DivBlock(null, "cell-m col");
                    cell.Attr("key", string.Format("{0}_r{1}_c{2}", Id, r, c));

                    var item = GetItem(items, r, c);
                    if (item != null)
                    {

                        item.Controls.ForEach(ci => cell.Controls.Add(ci));
                        if (item.ColSpan > 1)
                        {
                            spanItems.Add(item);
                            cell.Attributes["colspan"] = item.ColSpan.ToString();
                        }

                        // apply styles
                        if (item.Style != null)
                        {
                            if (!item.Style.Orientation.HasValue)
                                item.Style.Orientation = default(Orientations);
                            if (item.Style.Orientation == Orientations.Vertical)
                                cell.Attr("orientation", "vertical");

                            // TODO: add custom css mearging.
                            if (!string.IsNullOrEmpty(item.Style.CSSClass))
                                cell.CssClass = string.Format("{0} {1}", cell.CssClass, item.Style.CSSClass);


                            if (!item.Style.HorizontalAlignment.HasValue)
                                item.Style.HorizontalAlignment = default(HorizontalAlignment);
                            if (!item.Style.VerticalAlignment.HasValue)
                                item.Style.VerticalAlignment = default(VerticalAlignment);

                            if (item.Style.HorizontalAlignment != HorizontalAlignment.Left &&
                                item.Style.HorizontalAlignment != HorizontalAlignment.NotSet)
                                cell.Attributes["align"] = item.Style.HorizontalAlignment.ToString().ToLower();
                            if (item.Style.VerticalAlignment != VerticalAlignment.NotSet)
                                cell.Style.Add(HtmlTextWriterStyle.VerticalAlign,
                                    item.Style.VerticalAlignment.ToString().ToLower());
                        }
                    }
                    if (cell.Controls.Count == 0)
                        cell.CssClass = string.Format("{0} empty", cell.CssClass);
                    if (cell.Controls.Count > 1)
                        cell.CssClass = string.Format("{0} multi", cell.CssClass);

                    row.Controls.Add(cell);
                }
            }

            ApplyColspans(spanItems, container, columnNumber);
            CleanUp(container);

            return container;
        }

        protected virtual void ApplyColspans(List<MatrixWebPartItem> spanItems, WebControl container, int columnNumber)
        {
            if (spanItems == null) return;
            // remove hidden cells.
            foreach (var item in spanItems.OrderBy(span => span.RowPosition).ThenBy(span => span.ColPosition))
            {
                var rows = DivBlock.Divs(container);
                var row = rows.FirstOrDefault(c => c.Attributes["key"] == string.Format("{0}_r{1}", Id, item.RowPosition));
                if (row == null) continue;
                {
                    var cells = DivBlock.Divs(row);
                    for (var i = 1; i < item.ColSpan; i++)
                    {
                        var emptyCell = cells.FirstOrDefault(c =>
                            c.Attributes["key"] == string.Format("{0}_r{1}_c{2}", Id, item.RowPosition, item.ColPosition + i));
                        if (emptyCell != null)
                            row.Controls.Remove(emptyCell);
                    }
                }
            }
                
            // apply colspans.
            foreach (var item in spanItems.OrderBy(span => span.RowPosition))
            {
                var rows = DivBlock.Divs(container);
                var row = rows.FirstOrDefault(c =>
                    c.Attributes["key"] == string.Format("{0}_r{1}", Id, item.RowPosition));
                if (row == null) continue;
                foreach (var control in row.Controls)
                {
                    var cell = control as DivBlock;
                    if (cell == null || cell.CssClass.Contains("xs-12")) continue;
                    var colspan = 1;
                    if (!string.IsNullOrEmpty(cell.Attributes["colspan"]))
                    {
                        colspan = int.Parse(cell.Attributes["colspan"]);
                        cell.Attributes.Remove("colspan");
                    }

                    var cellClass = string.Empty;
                    foreach (var cssClass in ColumnClasses)
                    {
                        var colSize = (double)12 / Math.Min(columnNumber, cssClass.Value);
                        var curColSize = colSize * colspan;
                        if (curColSize > 12)
                            curColSize = 12;
                        cellClass += string.Format(" col-{0}-{1}", cssClass.Key, curColSize);
                    }

                    cell.CssClass += cellClass;
                }
            }
        }
        
        protected virtual void CleanUp(WebControl container)
        {
            foreach (var row in DivBlock.Divs(container))
            {
                row.Attributes.Remove("key");
            }
        }

        protected virtual string Id
        {
            get { return "item"; }
        }
        
        protected ReadOnlyDictionary<string, int> ColumnClasses = new ReadOnlyDictionary<string, int>(new Dictionary<string, int>()
        {
            {"xs", 1},
            {"sm", 2},
            {"md", 3},
            {"lg", 4}
        });
    }
}