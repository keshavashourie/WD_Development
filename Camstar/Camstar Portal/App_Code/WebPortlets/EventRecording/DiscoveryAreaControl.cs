// Copyright Siemens 2023  
using System;
using System.Linq;
using System.Data;
using System.Web.UI;

using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.WebControls.PickLists;

namespace Camstar.WebPortal.WebPortlets
{
    public class DiscoveryAreaControl : DropDownList
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            ListDisplayExpression = "Name";
            ListValueColumn = "Name";
            DisplayMode = DisplayModeType.PickList;
            SelectChildOnly = true;
        }

        public override void ConvertToJsonObj(DataRequestEventArgs e)
        {
            _DiscoveryAreaDataSource = e.Data as DataTable;

            if (_DiscoveryAreaDataSource != null && _DiscoveryAreaDataSource.Columns.Count > 0)
            {
                if (string.IsNullOrEmpty(ListDisplayExpression))
                    ListDisplayExpression = ListValueColumn;

                var item = new tree_row("Root", "", "", "", false);
                CollectChildren(item, null);
                e.Data = item.children;
            }
            e.ViewMode = "tree";
        }

        public override bool IsEnum
        {
            get { return false; }
            set { base.IsEnum = value; }
        }

        protected virtual void CollectChildren(tree_row parent, string type)
        {
            type = type ?? "";
            foreach (var o in _DiscoveryAreaDataSource.Rows)
            {
                var row = o as DataRow;
                if (row != null)
                {
                    var rowType = row["OwnerName"] as string ?? string.Empty;
                    if (rowType == type)
                    {
                        var item = parent.Children.FirstOrDefault(ch => ch.text == row["Type"] as string);
                        if (item == null) // Add new Group
                        {
                            if (string.IsNullOrEmpty(rowType))
                            {
                                item = new tree_row(row["Type"] as string, "", "", "", true);
                                parent.AddChild(item);    
                            }
                        }
                        var item2 = new tree_row(Convert.ToString(row[ListDisplayExpression]), row[ListValueColumn] as string, "", "", true);
                        if (item == null)
                            parent.AddChild(item2);
                        else
                            item.AddChild(item2); // Add item to the group.
                        CollectChildren(item2, Convert.ToString(row[ListDisplayExpression]));
                    }
                }
            }
        }

        protected override IViewControl CreateViewControl()
        {
            return new JSTreeViewControl();
        }

        private DataTable _DiscoveryAreaDataSource;
    }
}
