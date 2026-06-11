// Copyright Siemens 2023 
using System;
using System.Collections.Generic;
using System.Data;
using Camstar.SmartParser;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebControls;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using System.ServiceModel.Syndication;
using System.Linq;
using Camstar.WCF.ObjectStack;

/// <summary>
/// 
/// </summary>
namespace Camstar.WebPortal.WebPortlets.Modeling
{
    public class SmartScanRuleMaint : MatrixWebPart
    {
        #region Controls
        protected JQDataGrid PatternGrid { get { return Page.FindCamstarControl("ObjectChanges_SmartScanPattern") as JQDataGrid; } }

        protected Button ApplyRuleButton { get { return Page.FindCamstarControl("ApplyRule") as Button; } }

        protected TextBox SmartScanValue { get { return Page.FindCamstarControl("SmartScanValue") as TextBox; } }

        protected JQDataGrid ExtractedValuesGrid { get { return Page.FindCamstarControl("ExtractedValues") as JQDataGrid; } }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            ApplyRuleButton.Click += ApplyRuleButton_Click;
        }

        public override FormsFramework.ValidationStatus ValidateInputData(Service serviceData)
        {
            ValidationStatus status = base.ValidateInputData(serviceData);
            var patterns = GetPatterns();
            Pattern badActionPattern = patterns.FirstOrDefault<Pattern>(p => p.smartScanType.Equals("Action", StringComparison.OrdinalIgnoreCase) && (p.pattern.Contains("[") || p.pattern.Contains("]")));
            if(badActionPattern != null)
            {
                var labelCache = FrameworkManagerUtil.GetLabelCache(System.Web.HttpContext.Current.Session);
                string message = labelCache.GetLabelByName("SmartScanActionBracket").Value.Replace("#ErrorMsg.Name", badActionPattern.pattern);

                ValidationStatusItem statusItem = new RegularValidationStatusItem(message);
                status.Add(statusItem);
            }
            return status;
        }


        private IEnumerable<Pattern> GetPatterns()
        {
            var patterns = ((object[])(PatternGrid.Data)).Where(pc => (bool)(pc as SmartScanPatternChanges).IsActive).Select(pc => new Pattern()
            {
                pattern = (string)(pc as SmartScanPatternChanges).Pattern,
                removeOnMatch = (bool)(pc as SmartScanPatternChanges).RemoveOnMatch,
                smartScanType = Enum.GetName(typeof(SmartScanTypeEnum), (SmartScanTypeEnum)(pc as SmartScanPatternChanges).SmartScanType),
                isRegex = (bool)(pc as SmartScanPatternChanges).IsRegex,
                isActive = (bool)(pc as SmartScanPatternChanges).IsActive
            });
            return patterns;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ApplyRuleButton_Click(object sender, EventArgs e)
        {
            string barcode = (string)(SmartScanValue.Data ?? "");
            if (string.IsNullOrEmpty(barcode))
                return;

            ExtractedValuesGrid.ClearData();

            // Get patterns from grid
            var patternChanges = PatternGrid.Data as SmartScanPatternChanges[];

            // Convert to the Pattern objects that ParseBarcode() accepts
            var patterns = GetPatterns();

            // use patterns to parse values from the barcode
            WebClientPortal.SmartScanService sss = new WebClientPortal.SmartScanService();
            sss.ParseBarcode(barcode, patterns.ToList<Pattern>(), out var bcValues, out string errMsg); 

            if (string.IsNullOrEmpty(errMsg))
            {
                // put values into results grid
                ExtractedValuesGrid.Data = bcValues.Select(bcVal => new { Type = bcVal.Key, Value = bcVal.Value }).ToArray();
            }
            else
                Page.StatusBar.WriteError(errMsg);

            SmartScanValue.Focus();
        }
    }
}
