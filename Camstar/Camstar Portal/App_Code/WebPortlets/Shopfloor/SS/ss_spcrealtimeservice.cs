/* Copyright 2025 Siemens */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;

using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.Personalization;

using SWC = System.Web.UI.WebControls;
using System.Collections;
using System.Web;
using System.Data;
using OM = Camstar.WCF.ObjectStack;
using System.IO;
using System.Text.RegularExpressions;

/// <summary>
/// Summary description for SS_SPCRealTimeService
/// IMPORTANT NOTE: 
///     The web config needs to be modified for this service to be enabled/exposed
///     Ensure that the following tags are set in the web.config
///     1) within the <endpointBehaviors></endpointBehaviors> tag, add a new endpoint behavior like below
///         <behavior name="SS_SPCRealTimeServiceAjaxBehavior">
///             <webHttp />
///             <enableWebScript />
///         </behavior>
///     2) within the <services></services> tag, add the following service
///         <service name="SS_SPCRealTimeService">
///             <endpoint address="" behaviorConfiguration="SS_SPCRealTimeServiceAjaxBehavior" binding="webHttpBinding" contract="SS_SPCRealTimeService"/>
///         </service>
/// </summary>

[ServiceContract(Namespace = "")]
[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
public class SS_SPCRealTimeService
{
    //--------------------------------------
    //
    //--------------------------------------
    [OperationContract]
    [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json)]
    public List<string> ExecuteSPCService(string inputDataString)
    {
        List<string> returnResult = new List<string>();

        string[] items = inputDataString.Split(','); //spcsetupname,employeename,spcqueryparams
        string SPCSetupname = items[0];
        string EmployeeName = items[1];

        string[] spcQueryParams = new string[0];
        if (!string.IsNullOrEmpty(items[2]))
        {
            spcQueryParams = items[2].Split(';');
        }

        string spcResultFilename = items[3];

        if (!string.IsNullOrEmpty(spcResultFilename))
        {
            deleteSPCTxnData(spcResultFilename);
        }
        else
        {
            spcResultFilename = null;
        }

        // prepare request
        ss_SPCRealTime inputData = new ss_SPCRealTime
        {
            SPCSetup = new NamedObjectRef(SPCSetupname),
            Employee = new NamedObjectRef(items[1]),
            scsSPCResultFilename = spcResultFilename
        };

        inputData.SPCParams = new SPCTxnDataParamsChanges[spcQueryParams.Length];
        for (int x = 0; x < spcQueryParams.Length; x++)
        {
            string ParamName = spcQueryParams[x].Split(':')[0];
            string paramValue = spcQueryParams[x].Split(':')[1];

            inputData.SPCParams[x] = new OM.SPCTxnDataParamsChanges();
            inputData.SPCParams[x].ParamName = ParamName;
            inputData.SPCParams[x].ParamValue = paramValue;
        }

        ss_SPCRealTime_Info info = new ss_SPCRealTime_Info
        {
            SPCTxnDataList = new SPCTxnData_Info
            {
                SPCFailureAction = FieldInfoUtil.RequestValue(),
                SPCResult = FieldInfoUtil.RequestValue(),
                SPCResultFilename = FieldInfoUtil.RequestValue(),
                FailureDocumentSet = FieldInfoUtil.RequestValue(),
                SPCErrorMessage = FieldInfoUtil.RequestValue(),
                ChartHeight = FieldInfoUtil.RequestValue(),
                ChartWidth = FieldInfoUtil.RequestValue(),
                Name = FieldInfoUtil.RequestValue()
            }
        };

        UserProfile profile = HttpContext.Current.Session[Camstar.WebPortal.Constants.SessionConstants.UserProfile] as UserProfile;
        ss_SPCRealTimeService serv = new ss_SPCRealTimeService(profile);
        ss_SPCRealTime_Result result = null;
        ResultStatus resultStatus = serv.ExecuteTransaction(inputData, new ss_SPCRealTime_Request { Info = info }, out result);

        if (result.Value.SPCTxnDataList[0].SPCResultFilename != null)

            returnResult.Add(result.Value.SPCTxnDataList[0].SPCResultFilename.ToString());
        else
            returnResult.Add(resultStatus.Message);

        return returnResult;

    }

    [OperationContract]
    [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json)]
    public void deleteOldFiles(string inputDataString)
    {
        string[] items = inputDataString.Split(',');
        string oldChartURL = items[0];
        string oldSPCImgURL = items[1];
        string SourceDir = @"C:\Program Files (x86)\Camstar\SPC\Chart Output";//default SPC Chart output directory
                                                                              //string[] htmlFile = null;
                                                                              //string[] imgFile = null;
        try
        {
            //string folderpath = oldChartURL.Substring(oldChartURL.IndexOf("/SPC/") + 4, oldChartURL.Length - oldChartURL.LastIndexOf('/') + 3);
            string sFolderPath = oldChartURL.Substring(0, oldChartURL.LastIndexOf('/') + 1);
            sFolderPath = sFolderPath.Substring(oldChartURL.IndexOf("/SPC/") + 4);
            Directory.Delete(SourceDir + sFolderPath, true);
        }
        catch
        { }

        //      htmlFile = Directory.GetFiles(SourceDir, Regex.Match(oldChartURL, @"\w+\.htm").Value);
        //imgFile = Directory.GetFiles(SourceDir, Regex.Match(oldSPCImgURL, @"\w+\.png").Value);

        //      if (htmlFile != null)
        //          File.Delete(htmlFile[0]);
        //      if (imgFile != null)
        //          File.Delete(imgFile[0]);

        deleteSPCTxnData(oldChartURL);
    }

    public void deleteSPCTxnData(string oldChartURL)
    {
        Camstar.WCF.ObjectStack.UserProfile profile = HttpContext.Current.Session[Camstar.WebPortal.Constants.SessionConstants.UserProfile] as Camstar.WCF.ObjectStack.UserProfile;
        QueryUtil QueryService = new QueryUtil(profile);
        ResultStatus resultStatus = null;
        RecordSet queryResult = null;
        QueryOptions queryOption = new QueryOptions();

        queryOption.QueryType = Camstar.WCF.ObjectStack.QueryType.User; /// querytype = system
		queryOption.StartRow = 1;// Start Row;
        queryOption.RowSetSize = 10000;// Row Set Size;

        string sqlQueryText = "";

        //sql to delete spctxndatadetails
        sqlQueryText = "delete from A_SPCTXNDATADETAILS where spctxndataid = (select A_SPCTXNDATA.SPCTXNDATAID from A_SPCTXNDATA where SPCRESULTFILENAME  = '" + oldChartURL + "'";
        QueryService.ExecuteSQL(sqlQueryText, queryOption, ref queryResult, ref resultStatus);

        //sql to delete spctxndata
        sqlQueryText = "delete from A_SPCTXNDATA where SPCRESULTFILENAME = '" + oldChartURL + "'";
        QueryService.ExecuteSQL(sqlQueryText, queryOption, ref queryResult, ref resultStatus);
    }
}





