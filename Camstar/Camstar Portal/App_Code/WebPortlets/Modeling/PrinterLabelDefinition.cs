// Copyright Siemens 2023 
using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework.Utilities;
using System.Web;

namespace Camstar.WebPortal.WebPortlets.Modeling
{
    public class PrinterLabelDefinition : MatrixWebPart
    {
        protected virtual JQDataGrid LabelTagsGrid
        {
            get { return Page.FindCamstarControl("LabelTagsGrid") as JQDataGrid; }
        }

        public override ValidationStatus ValidateInputData(OM.Service serviceData)
        {
            ValidationStatus status = base.ValidateInputData(serviceData);

            var tags = (serviceData as OM.PrinterLabelDefinitionMaint)?.ObjectChanges?.LabelTags;
            if (tags != null)
            {
                var data = LabelTagsGrid.Data;

                foreach (var tag in tags)
                {
                    var isListTag = (data as OM.LabelTagChanges[])?.Where(x => (bool)x.IsList && x.Name == tag.Name).FirstOrDefault();
                    if (isListTag != null && string.IsNullOrEmpty(isListTag.ListItemExpression.ToString()))
                    {
                        var labelCache = FrameworkManagerUtil.GetLabelCache(HttpContext.Current.Session);
                        var label = labelCache.GetLabelByName("ListItemExpressionValidation").Value;

                        status.Add(new RequiredFieldStatusItem(LabelTagsGrid.Caption, null) { RequiredMessage = string.Format(label, isListTag.Name) });
                        return status;
                    }                    
                }
            }

            return status;
        }
    }

}

