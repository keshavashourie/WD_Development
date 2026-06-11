// Copyright Siemens 2023
using System;
using System.Linq;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using System.Data;

namespace Camstar.WebPortal.WebPortlets.Modeling
{
    public class SPCRulesMaint : MatrixWebPart
    {
        protected virtual JQDataGrid ViolationsGrid { get { return Page.FindCamstarControl("SPCRuleViolationsGrid") as JQDataGrid; } }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ViolationsGrid.BoundContext.SnapCompleted += BoundContext_SnapCompleted;
        }

        protected virtual void BoundContext_SnapCompleted(DataTable dataWindowTable)
        {
            foreach (var r in dataWindowTable.Rows.OfType<DataRow>())
            {
                r.BeginEdit();
                var id = r["_id_column"] as string;
                var item = ViolationsGrid.BoundContext.GetItem(id) as SPCRuleViolationsChanges;
                if (item.ViolationParams != null)
                {
                    string str = string.Empty;
                    Array.ForEach(item.ViolationParams, n =>
                    {
                        if (!n.ParamValue.IsNullOrEmpty() && n.ParamValue != "")
                        {
                            str += n.ParamName + "=" + n.ParamValue + ";";
                        }
                    });
                    r["SPCViolationParams"] = str;
                }
            }
            dataWindowTable.AcceptChanges();
        }
    }
}
