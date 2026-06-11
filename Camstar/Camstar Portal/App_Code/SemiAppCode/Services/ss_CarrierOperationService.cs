/* Copyright 2021 Siemens */
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
/// Summary description for SS_CarrierOperationService
/// IMPORTANT NOTE: 
///     The web config needs to be modified for this service to be enabled/exposed
///     Ensure that the following tags are set in the web.config
///     1) within the <endpointBehaviors></endpointBehaviors> tag, add a new endpoint behavior like below
///         <behavior name="SS_CarrierOperationServiceAjaxBehavior">
///             <webHttp />
///             <enableWebScript />
///         </behavior>
///     2) within the <services></services> tag, add the following service
///         <service name="SS_CarrierOperationService">
///             <endpoint address="" behaviorConfiguration="SS_CarrierOperationServiceAjaxBehavior" binding="webHttpBinding" contract="SS_CarrierOperationService"/>
///         </service>
/// </summary>

[ServiceContract(Namespace = "")]
[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
public class SS_CarrierOperationService
{
    //--------------------------------------
    //
    //--------------------------------------
    [OperationContract]
    [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json)]

    public string ValidateContainers(string inputDataString)
    {

        string[] items = inputDataString.Split(','); //spcsetupname,employeename,spcqueryparams
        string carrierName = items[0];
        string containerName = items[1];
        bool isAuto = bool.Parse(items[2]);
        bool isLoad = bool.Parse(items[3]);
        string containerToValidate = items[4]; //used to compare its property with current assigning container
        string resultMessage = "";

        resultMessage = ValidateContainer(containerName, containerToValidate, carrierName);

        if (string.IsNullOrWhiteSpace(resultMessage))
        {
            resultMessage = RetrieveCarrierPosSlotMap(containerName, containerToValidate, carrierName);
        }

        return resultMessage;
    }

    public string ValidateContainer(string containerName, string containerToValidate, string carrierName)
    {
        // get the session and user profile

        var fs = FrameworkManagerUtil.GetFrameworkSession();
        scsCarrierPositionAssignService oService = new scsCarrierPositionAssignService(fs.CurrentUserProfile);
        scsCarrierPositionAssign inputData = new scsCarrierPositionAssign();
        inputData.Container = new ContainerRef(containerName);
        inputData.ContainerToValidate = new ContainerRef(containerToValidate);
        inputData.Carrier = new NamedObjectRef(carrierName);
        scsCarrierPositionAssign_Info oServiceInfo = new scsCarrierPositionAssign_Info();

        scsCarrierPositionAssign_Parameters oParameters = new scsCarrierPositionAssign_Parameters();
        scsCarrierPositionAssign_Result oServiceResult = new scsCarrierPositionAssign_Result();

        oServiceInfo.ContainerValidationErrorMsg = FieldInfoUtil.RequestValue();
        oServiceInfo.ContainerValidationResult = FieldInfoUtil.RequestValue();

        // Set the request
        scsCarrierPositionAssign_Request request = new scsCarrierPositionAssign_Request();
        request.Info = oServiceInfo;

        // execute the request selection values
        ResultStatus oResult = oService.ValidateContainers(inputData, oParameters, request, out oServiceResult);
        string errMsg = null;

        if (!oServiceResult.IsEmpty)
        {
            if (oServiceResult.Value.ContainerValidationErrorMsg != null)
            {
                errMsg = oServiceResult.Value.ContainerValidationErrorMsg.Value.ToString();
                return errMsg;
            }
            else
            {
                return errMsg;
            }
        }
        else
        {
            if (oResult.ExceptionData.ToString().Contains("not found."))
                return "Invalid Container";
            else
                return oResult.Message;
        }
    }

    //--------------------------------------
    //
    //--------------------------------------
    private string RetrieveCarrierPosSlotMap(string containerName, string containerToValidate, string carrierName)
    {
        // get the session and user profile
        var fs = FrameworkManagerUtil.GetFrameworkSession();
        QueryService oService = new QueryService(fs.CurrentUserProfile);
        QueryOptions oOptions = new QueryOptions();
        RecordSet oData = new RecordSet();
        QueryParameters oParameters = new QueryParameters();
        QueryParameter[] oParameterList = new QueryParameter[1];
        oParameterList[0] = new QueryParameter();
        oParameterList[0].Name = "ContainerName";
        oParameterList[0].Value = containerName;

        oParameters.Parameters = oParameterList;

        ResultStatus oResult = oService.Execute("scsCarrrierPositionAssign_ContainerValidation", oParameters, oOptions, out oData);
        if (oResult.IsSuccess)
        {
            string sContainerName = "";
            string sCarrierName = "";
            string sStatus = "";
            string sScheduleData = "";
            bool isInWIP = false;
            string sStartParentContainer = "";
            string sWorkflowStepId = "";
            string sSlotNo = "";

            if (oData != null)
            {
                if (oData.Rows != null)
                {
                    if (oData.Rows.Length > 0)
                    {
                        sContainerName = oData.Rows[0].Values[1].ToString();
                        sCarrierName = oData.Rows[0].Values[2].ToString();
                        sStatus = oData.Rows[0].Values[3].ToString(); // 1= ACTIVE container
                        sScheduleData = oData.Rows[0].Values[4].ToString();
                        sStartParentContainer = oData.Rows[0].Values[5].ToString();
                        sWorkflowStepId = oData.Rows[0].Values[6].ToString();
                        sSlotNo = oData.Rows[0].Values[7].ToString();

                        if (!string.IsNullOrWhiteSpace(sScheduleData))
                        {
                            isInWIP = true;
                        }

                        // validate exisiting container loaded in different container.
                        if (!string.IsNullOrWhiteSpace(sCarrierName) && sCarrierName != carrierName)
                        {
                            return sContainerName + " was assigned in another carrier. Please UNLOAD container from Carrier: " + sCarrierName + " on Slot Number: " + sSlotNo + ".";
                        }
                    }
                }
            }
        }
        return null;
    } // RetrieveCarrierPosSlotMap
}