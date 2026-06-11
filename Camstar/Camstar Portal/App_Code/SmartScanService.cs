// © Siemens 2019 Siemens Product Lifecycle Management Software Inc.
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Web;

using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.PortalConfiguration;
using Camstar.WCF.Services;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.Personalization;

using Camstar.SmartParser;

namespace WebClientPortal
{
    /// <summary>
    /// For parsing individual values from a barcode
    /// </summary>
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class SmartScanService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="barcode">Parse values for this</param>
        /// <param name="patterns">List of patterns for extracting values from the barcode.</param>
        /// <param name="bcValues">Values that have been extracted from the barcode.</param>
        /// <param name="localizedErrMsg">Can be shown to the user.  English error message set in ResultStatus.</param>
        /// <returns></returns>
        [OperationContract]
        public virtual ResultStatus ParseBarcode(
            string barcode, 
            List<Pattern> patterns, 
            out Dictionary<string, string> bcValues, 
            out string localizedErrMsg)
        {
            localizedErrMsg = "";

            bcValues = Parser.ParseBarcode(barcode, patterns, out string errMsg, out string errMsgLabelName, out var errMsgParams);

            if (!string.IsNullOrEmpty(errMsgLabelName))
            {
                // load label value
                LabelCache labelCache = FrameworkManagerUtil.GetLabelCache(System.Web.HttpContext.Current.Session);
                localizedErrMsg = labelCache.GetLabelByName(errMsgLabelName).Value;

                // replace param placeholders
                for(int i = errMsgParams.Count()-1; i >= 0; i--)
                {
                    string paramIndex = (i == 0) ? "" : (i + 1).ToString();
                    string placeHolder = "#ErrorMsg.Name" + paramIndex;

                    localizedErrMsg = localizedErrMsg.Replace(placeHolder, errMsgParams.ElementAt(i));
                }
            }
            return new ResultStatus(errMsg, string.IsNullOrEmpty(errMsg));
        }

    }
}
