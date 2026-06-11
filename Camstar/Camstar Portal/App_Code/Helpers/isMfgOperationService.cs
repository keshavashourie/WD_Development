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

using OS = Camstar.WCF.ObjectStack;
using System.Data;
using System.Runtime.Serialization;
using Camstar.Util;


namespace WebClientPortal
{
    /// <summary>
    /// Functionality implemented by the MfgOperation CDO
    /// </summary>
    public partial class MfgOperationService
    {
        /// <summary>
        /// Use Recipe Plan (Matrix) to determine the Recipe required for the given Resource
        /// </summary>
        /// <param name="resourceName"></param>
        /// <param name="productName"></param>
        /// <param name="productRev"></param>
        /// <param name="specName"></param>
        /// <param name="specRev"></param>
        /// <param name="mfgOrderName"></param>
        /// <param name="requiredRecipeName">Recipe as determined by Recipe Plan(Matrix) or Reciep Pattern</param>
        /// <param name="localizedErrMsg">Not currently set</param>
        /// <returns></returns>
        [OperationContract]
        public virtual ResultStatus GetRequiredRecipe(
            string resourceName,
            string productName,
            string productRev,
            string specName,
            string specRev,
            string mfgOrderName,
            out string requiredRecipeName,
            out string localizedErrMsg)
        {
            localizedErrMsg = "";
            requiredRecipeName = "";

            // Set up params
            OS.MfgOperation mfgOperation = new MfgOperation()
            {
                Resource = new NamedObjectRef(resourceName),
                isProduct = new RevisionedObjectRef(productName, productRev),
                Spec = new RevisionedObjectRef(specName, specRev),
                MfgOrder = new NamedObjectRef(mfgOrderName),
                isValidate = false
            };

            // Ask for the required recipe
            var request = new Camstar.WCF.Services.MfgOperation_Request()
            {
                Info = new MfgOperation_Info()
                {
                    isRequiredRecipe = new Info(true)
                }
            };

            MfgOperation_Result result = new MfgOperation_Result();
            var mfgOperationSvc = new Camstar.WCF.Services.MfgOperationService(FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);
            ResultStatus status = mfgOperationSvc.GetEnvironment(mfgOperation, request, out result);
            if (status.IsSuccess && result.Value.isRequiredRecipe != null)
            {
                requiredRecipeName = result.Value.isRequiredRecipe.Value;
            }

            return new ResultStatus(string.Empty, true);
        }

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        public virtual SimpleResponse GetMfgOperationComments(string mfgOrderId, string workflowStepId, string resourceName, string preactorScheduledOrdersId)
        {
            ResultStatus resultStatus = new ResultStatus();
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);

            var service = new Camstar.WCF.Services.MfgOperationService(session.CurrentUserProfile);

            SimpleResponse response = new SimpleResponse();

            var serviceData = new MfgOperation
            {
                MfgOrder = new NamedObjectRef { ID = mfgOrderId },
                Resource = new NamedObjectRef(resourceName),
                WorkflowStep = new NamedSubentityRef { ID = workflowStepId },
                isPreactorScheduledOrdersId = preactorScheduledOrdersId
            };

            var serviceRequest = new MfgOperation_Request
            {
                Info = new MfgOperation_Info
                {
                    MfgOperationComments = new Info(true)
                }
            };

            try
            {
                MfgOperation_Result result = new MfgOperation_Result();
                var status = service.Load(serviceData, serviceRequest, out result);
                response = new SimpleResponse(status.IsSuccess, status.Message, status.ExceptionData != null ? status.ExceptionData.ToString() : "");
                if (status.IsSuccess)
                {
                    if (result.Value.MfgOperationComments != null)
                        response.Data = result.Value.MfgOperationComments.Value;
                }
                else
                {
                    response.Message = status.ExceptionData != null ? status.ExceptionData.Description : "Error getting MfgOperation comments";
                }
            }
            catch (Exception ex)
            {
                response = new SimpleResponse(ex);
            }

            return response;
        }

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        public virtual SimpleResponse SetMfgOperationComments(string mfgOrderId, string workflowStepId, string resourceName, string preactorScheduledOrdersId, string comments)
        {
            SimpleResponse responseData = null;

            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);

            var service = new Camstar.WCF.Services.MfgOperationService(session.CurrentUserProfile);

            var serviceData = new MfgOperation
            {
                MfgOrder = new NamedObjectRef { ID = mfgOrderId },
                Resource = new NamedObjectRef(resourceName),
                WorkflowStep = new NamedSubentityRef { ID = workflowStepId },
                isPreactorScheduledOrdersId = preactorScheduledOrdersId,
                ClearComments = string.IsNullOrWhiteSpace(comments),
                MfgOperationComments = comments
            };

            var serviceRequest = new MfgOperation_Request();

            try
            {
                MfgOperation_Result result = new MfgOperation_Result();
                var status = service.ExecuteTransaction(serviceData, serviceRequest, out result);
                responseData = new SimpleResponse(status.IsSuccess, status.Message, status.ExceptionData != null ? status.ExceptionData.ToString() : "");

                if (status.IsSuccess)
                {
                    //LogMessage(LogSeverity.Debug, ProcedureName, "Success setting MfgOperation comments");
                }
                else
                {
                    responseData.Message = status.ExceptionData != null ? status.ExceptionData.Description : status.ToString();
                    responseData.Exception = responseData.Message;
                }
            }
            catch (Exception ex)
            {
                responseData = new SimpleResponse(ex);
                //throw ex;
            }

            return responseData;
        }
        [OperationContract]
        public ResourceDetailsResponse GetResourceComponentsMaterialQueue(string resourceName, string operationId)
        {
            if (string.IsNullOrEmpty(resourceName))
            {
                resourceName = string.Empty;
            }

            if (string.IsNullOrEmpty(operationId))
            {
                operationId = string.Empty;
            }
            ResourceDetailsResponse resourceDetails = new ResourceDetailsResponse();

            var currentUserProfile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
            QueryService service = new QueryService(currentUserProfile);
            OS.QueryOptions options = new OS.QueryOptions();
            OS.RecordSet componentRecords = new OS.RecordSet();

            OS.QueryParameters queryParamsObj = new OS.QueryParameters();
            OS.QueryParameter[] queryParams = new OS.QueryParameter[2]
            {
                new QueryParameter("RESOURCENAME", resourceName),
                new QueryParameter("OPERATIONID", operationId)
            };
            queryParamsObj.Parameters = queryParams;

            string query = "isGetMaterialQueueDetails";
            OS.ResultStatus resultStatus = service.Execute(query, queryParamsObj, options, out componentRecords);
            resourceDetails = new ResourceDetailsResponse(CreateResponse(resultStatus));
            SimpleResponse matresourceDetails = new SimpleResponse();
            resourceDetails.ResourceName = resourceName;

            if (resultStatus.IsSuccess)
            {
                // Add component info to the resource
                DataTable componentTable = componentRecords.GetAsDataTable();
                if (componentTable != null && componentTable.Rows.Count > 0)
                {
                    resourceDetails.Components = new List<ResourceComponentItem>(componentTable.Rows.Count);
                    foreach (DataRow dr in componentTable.Rows)
                    {
                        ResourceComponentItem rc = new ResourceComponentItem();

                        rc.Lot = Utilities.GetDataRowField(dr, "ContainerOrLot");
                        rc.Product = Utilities.GetDataRowField(dr, "Product");
                        rc.QtyRequired = Convert.ToDouble(Utilities.GetDataRowField(dr, "Qty"));
                        rc.SlotName = Utilities.GetDataRowField(dr, "Position");
                        rc.TotalQuantity = Convert.ToDouble(Utilities.GetDataRowField(dr, "totalQTY"));
                        resourceDetails.Components.Add(rc);

                    }

                }
            }
            return resourceDetails;
        }
        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        public ResourceDetailsResponse LoadMaterialQueueTest(string SpecId, string MfgOrderId, string RouteStepId, string ResourceName)
        {
            return null;
        }

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        public ResourceDetailsResponse LoadMaterialQueue(string SpecId, string MfgOrderId, string RouteStepId, string ResourceName, string OperationId, double MfgOrderQty)
        {
            ResourceDetailsResponse responseData = null;
            try
            {

                string resourceName = ResourceName;
                string operationId = OperationId;
                string mfgOrderId = MfgOrderId;
                string specId = SpecId;
                string routeStepId = RouteStepId;
                double mfgOrderQty = MfgOrderQty;
                responseData = GetResourceComponentsMaterialQueue(resourceName, operationId);

                bool erpBOM = false;
                bool hasMaterialList = false;
                string bomId = GetBomForMfgOrder(mfgOrderId, out erpBOM);
                BomPartNumbers bom = GetMfgOrderMaterialList(routeStepId, mfgOrderId, out hasMaterialList);
                if (!string.IsNullOrEmpty(bomId) && !hasMaterialList)
                {
                    if (erpBOM && !string.IsNullOrEmpty(routeStepId))
                        specId = string.Empty;
                    bom = GetBomSpecData(bomId, specId, routeStepId, mfgOrderId, true);
                }

                if (bom.BomPartNumber.Count > 0)
                {
                    responseData.MaterialList = new List<BomItem>(bom.BomPartNumber.Count);

                    foreach (BomItem item in bom.BomPartNumber)
                    {
                        if (responseData.Components != null)
                        {
                            ResourceComponentItem comp = responseData.Components.Find(s => s.Product != null && string.Compare(s.Product, item.PartNumber, true) == 0);

                            if (comp != null)
                            {
								
                                comp.BomQty = item.QtyRequired * mfgOrderQty;
                                comp.BomItemId = item.ID;
                                comp.ProducingOrder = item.ProducingOrder;
                                if (comp.TotalQuantity < comp.BomQty)
                                    comp.PartiallySatisfied = true;
                                else if (comp.TotalQuantity >= comp.BomQty)
                                    comp.Satisfied = true;
                            }
                            else
                            {
                                responseData.MaterialList.Add(item);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                responseData = new ResourceDetailsResponse(new MfgOperationResponse(ex));
            }

            return responseData;
        }


        /// <param name="resourceName"></param>
        /// <returns></returns>

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        public bool ValidateIsHVTraceabilityenable(string resourceName)
        {
            bool isHVTraceabilityEnabled = false;

            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);

            if (session != null)
            {
                var queryParameters = new QueryParameters()
                {
                    Parameters = new QueryParameter[]
                    {
                      new QueryParameter("RESOURCENAME", resourceName)
                    }
                };

                var recordSet = new RecordSet();
                var service = new QueryService(session.CurrentUserProfile);
                var options = new QueryOptions();

                var resultStatus = service.Execute("isGetUseHVTraceability", queryParameters, options, out recordSet);

                if (resultStatus.IsSuccess && recordSet.Rows != null && recordSet.Rows.Length > 0)
                {
                    var queryList = new List<QueryData>();
                    foreach (var row in recordSet.Rows)
                        queryList.Add(new QueryData()
                        {
                            Text = row.Values[0],
                        });

                    if (queryList[0].Text.ToString() == "true")
                    {
                        isHVTraceabilityEnabled = true;
                    }
                    else if (queryList[0].Text.ToString() == "false")
                    {
                        isHVTraceabilityEnabled = false;
                    }
                }
            }

            return isHVTraceabilityEnabled;
        }



        public abstract class ResponseBase
        {
            public string Message;
            public string Exception;
            public bool Success;
            public string User;
            public string Pwd;
        }

        public class SimpleResponse : ResponseBase
        {
            public string Data;

            public SimpleResponse() { }

            public SimpleResponse(Exception ex)
            {
                Success = false;
                Message = string.Empty;
                Exception = ex.Message;
            }

            public SimpleResponse(bool success, string message, string exception)
            {
                Success = success;
                Message = message;
                Exception = exception;
            }

        }
    }
}
