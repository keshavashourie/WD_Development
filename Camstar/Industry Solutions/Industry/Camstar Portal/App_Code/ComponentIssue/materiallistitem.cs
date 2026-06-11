// Copyright Siemens 2023  
using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Collections.Generic;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.WebPortlets.ComponentIssue;
using CamstarPortal.WebControls;

using OM = Camstar.WCF.ObjectStack;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using DocumentFormat.OpenXml.Bibliography;

namespace Camstar.WebPortal.WebPortlets.ComponentIssue
{
    public class MaterialListItem
    {
        public enum HeaderEnum
        {
            MaterialListItemNameIndex,
            MaterialListItemIdIndex,
            ProductIndex,
            ProductRevisionIndex,
            ProductDescriptionIndex,
            QtyRequiredIndex,
            QtyIssuedIndex,
            UOMIndex,
            IssueControlIndex,
            AssemblySequenceIndex,
            ParentNameIndex,
            ParentRevisionIndex,
            PhantomBillIdIndex,
            EffectiveFromDateGMT,
            EffectiveThruDateGMT,
            SpecIdIndex,
            SetupQtyIndex,
            ReferenceDesignatorIndex,
            SpecNameIndex
        }

        /// <summary>
        /// Construct dictionary for looking up index for a given column
        /// </summary>
        /// <param name="headers"></param>
        /// <returns>Key: enum identifying a column in IssueDetails data, Value: index of column in data</returns>
        public static Dictionary<HeaderEnum, int> GetColumnIndexes(OM.Header[] headers)
        {
            Dictionary<HeaderEnum, int> columnIndexes = new Dictionary<HeaderEnum, int>();

            columnIndexes.Add(HeaderEnum.MaterialListItemNameIndex, Array.FindIndex(headers, h => h.Name.Equals("MaterialListItem")));
            columnIndexes.Add(HeaderEnum.MaterialListItemIdIndex, Array.FindIndex(headers, h => h.Name.Equals("MaterialListItemId")));
            columnIndexes.Add(HeaderEnum.ProductIndex, Array.FindIndex(headers, h => h.Name.Equals("ComponentProduct")));
            columnIndexes.Add(HeaderEnum.ProductRevisionIndex, Array.FindIndex(headers, h => h.Name.Equals("ProductRevision")));
            columnIndexes.Add(HeaderEnum.ProductDescriptionIndex, Array.FindIndex(headers, h => h.Name.Equals("ComponentDescription")));
            columnIndexes.Add(HeaderEnum.QtyRequiredIndex, Array.FindIndex(headers, h => h.Name.Equals("QtyRequired")));
            columnIndexes.Add(HeaderEnum.QtyIssuedIndex, Array.FindIndex(headers, h => h.Name.Equals("QtyIssued")));
            columnIndexes.Add(HeaderEnum.UOMIndex, Array.FindIndex(headers, h => h.Name.Equals("UOM")));
            columnIndexes.Add(HeaderEnum.AssemblySequenceIndex, Array.FindIndex(headers, h => h.Name.Equals("AssemblySequence")));
            columnIndexes.Add(HeaderEnum.IssueControlIndex, Array.FindIndex(headers, h => h.Name.Equals("IssueControl")));
            columnIndexes.Add(HeaderEnum.ParentNameIndex, Array.FindIndex(headers, h => h.Name.Equals("ParentName")));
            columnIndexes.Add(HeaderEnum.ParentRevisionIndex, Array.FindIndex(headers, h => h.Name.Equals("ParentRevision")));
            columnIndexes.Add(HeaderEnum.PhantomBillIdIndex, Array.FindIndex(headers, h => h.Name.Equals("PhantomBillId")));
            columnIndexes.Add(HeaderEnum.EffectiveFromDateGMT, Array.FindIndex(headers, h => h.Name.Equals("EffectiveFromDateGMT")));
            columnIndexes.Add(HeaderEnum.EffectiveThruDateGMT, Array.FindIndex(headers, h => h.Name.Equals("EffectiveThruDateGMT")));
            columnIndexes.Add(HeaderEnum.SpecIdIndex, Array.FindIndex(headers, h => h.Name.Equals("SpecId")));
            columnIndexes.Add(HeaderEnum.SetupQtyIndex, Array.FindIndex(headers, h => h.Name.Equals("SetupQty")));
            columnIndexes.Add(HeaderEnum.ReferenceDesignatorIndex, Array.FindIndex(headers, h => h.Name.Equals("ReferenceDesignator")));
            columnIndexes.Add(HeaderEnum.SpecNameIndex, Array.FindIndex(headers, h => h.Name.Equals("SpecName")));
            return columnIndexes;
        }

        public static T GetRowValue<T>(OM.Row row, int key)
        {
            return GetRowValue<T>(row, key, null);
        }

        public static T GetRowValue<T>(OM.Row row, int key, Func<object, object> f)
        {
            T result = default(T);
            try
            {
                object r = null;
                if (key >= 0)
                {
                    if (f != null)
                        r = f(row.Values[key]);
                    else r = row.Values[key];

                    result = (T)r;
                }
            }
            catch { }

            return result;
        }
    }
}