// Copyright Siemens 2022 
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Web;
using Camstar.WCF.Services;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework.Utilities;
using OS = Camstar.WCF.ObjectStack;
using System.Data;
using Camstar.Util;

namespace WebClientPortal
{

    /// </summary>
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    /// <summary>
    /// Functionality implemented by the isDefect CDO
    /// </summary>
    public partial class isDefectService
    {
        /// <summary>
        /// Delete selected defect items
        /// </summary>
        /// <param name="selectedDefects"></param>  
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        public isDefectDocumentResponse getDefectDocuments(string CurrentDefectId)
        {
            isDefectDocumentResponse responseData = new isDefectDocumentResponse();
            var iscurrentDefectId = CurrentDefectId;
            var currentUserProfile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
            QueryService service = new QueryService(currentUserProfile);
            OS.QueryOptions options = new OS.QueryOptions();
            OS.RecordSet componentRecords = new OS.RecordSet();

            OS.QueryParameters queryParamsObj = new OS.QueryParameters();
            OS.QueryParameter[] queryParams = new OS.QueryParameter[1]
            {
                new QueryParameter("ISCURRENTDEFECTID", iscurrentDefectId)

            };
            queryParamsObj.Parameters = queryParams;
            string query = "isDefect_GetDefectDocuments";
            OS.ResultStatus resultStatus = service.Execute(query, queryParamsObj, options, out componentRecords);
            responseData = new isDefectDocumentResponse(CreateResponseInfo(resultStatus));

            if (resultStatus.IsSuccess)
            {
                DataTable componentTable = componentRecords.GetAsDataTable();
                if (componentTable != null && componentTable.Rows.Count > 0)
                {
                    responseData.isDefectDocumentList = new List<isDefectDocuments>(componentTable.Rows.Count);
                    foreach (DataRow dr in componentTable.Rows)
                    {
                        isDefectDocuments rc = new isDefectDocuments();

                        rc.ID = Utilities.GetDataRowField(dr, "AttachedDocumentId");
                        rc.FileName = Utilities.GetDataRowField(dr, "AttachedFileName");
                        rc.Name = Utilities.GetDataRowField(dr, "DocumentName");
                        rc.Revision = Utilities.GetDataRowField(dr, "DocumentRevision");
                        rc.FileExtension = Utilities.GetDataRowField(dr, "AttachedFileExtension");
                        rc.FilePath = Utilities.GetDataRowField(dr, "FilePath");
                        rc.Identifier = Utilities.GetDataRowField(dr, "AttachedFileName");
                        responseData.isDefectDocumentList.Add(rc);

                    }

                }
            }
            return responseData;
        }

        [OperationContract]
        public virtual SimpleResponse setDeleteDefect(DefectActionRequest requestDelete, string requestContainer)
        {

            SimpleResponse response = new SimpleResponse();
            SimpleResponse responseData = null;
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);

            try
            {
                var deleteService = new Camstar.WCF.Services.isDefectDeleteService(session.CurrentUserProfile);
                Dictionary<string, List<ProductionDefect>> defectsPerContainer = GetDefectsPerContainer(requestDelete.Defects, requestContainer);

                // call service once for each container
                foreach (string containerName in defectsPerContainer.Keys)
                {

                    var deleteServiceData = new isDefectDelete()
                    {
                        Container = new ContainerRef(containerName)
                    };

                    List<SubentityRef> defectsToDelete = new List<SubentityRef>();

                    foreach (ProductionDefect defect in defectsPerContainer[containerName])
                    {
                        defectsToDelete.Add(new SubentityRef(defect.isCurrentDefectsIDString));
                    }
                    deleteServiceData.isCurrentDefectsToDelete = defectsToDelete.ToArray();

                    var deleteRequest = new Camstar.WCF.Services.isDefectDelete_Request();
                    var deleteResult = new Camstar.WCF.Services.isDefectDelete_Result();

                    ResultStatus resultStatus = deleteService.ExecuteTransaction(deleteServiceData, deleteRequest, out deleteResult);
                    responseData = new SimpleResponse(resultStatus.IsSuccess, resultStatus.Message, resultStatus.ExceptionData != null ? resultStatus.ExceptionData.ToString() : "");
                    if (!resultStatus.IsSuccess)
                    {
                        responseData.Message = resultStatus.ExceptionData != null ? resultStatus.ExceptionData.Description : resultStatus.ToString();
                        responseData.Exception = responseData.Message;
                    }
                }

            }
            catch (Exception ex)
            {
                responseData = new SimpleResponse(ex);
                //throw ex;
            }

            return responseData;
        }

        public static Dictionary<string, List<ProductionDefect>> GetDefectsPerContainer(List<ProductionDefect> currentDefects, string requestContainer)
        {
            Dictionary<string, List<ProductionDefect>> defectsPerContainer = new Dictionary<string, List<ProductionDefect>>();

            foreach (ProductionDefect currentDefect in currentDefects)
            {
                if (defectsPerContainer.ContainsKey(requestContainer))
                {
                    // already have entry for this container - add the defect to its list
                    defectsPerContainer[requestContainer].Add(currentDefect);
                }
                else
                {
                    // Create a new Dictonary entry for it
                    List<ProductionDefect> containerDefects = new List<ProductionDefect>() { currentDefect };
                    defectsPerContainer.Add(requestContainer, containerDefects);
                }
            }
            return defectsPerContainer;
        }

        [OperationContract]
        public virtual SimpleResponse setReOpenDefect(DefectActionRequest requestOpen, string requestContainer)
        {
            SimpleResponse responseData = null;
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);

            try
            {

                var reopenService = new Camstar.WCF.Services.isDefectReopenService(session.CurrentUserProfile);
                Dictionary<string, List<ProductionDefect>> defectsPerContainer = GetDefectsPerContainer(requestOpen.Defects, requestContainer);


                // call service once for each container
                foreach (string containerName in defectsPerContainer.Keys)
                {

                    var reopenServiceData = new isDefectReopen()
                    {
                        Container = new ContainerRef(containerName)
                    };

                    // create details (one for each defect we are going to reopen)
                    List<isDefectDetail> details = new List<isDefectDetail>();

                    foreach (ProductionDefect defect in defectsPerContainer[containerName])
                    {
                        isDefectDetail detail = new isDefectDetail()
                        {
                            isCurrentDefects = new SubentityRef(defect.isCurrentDefectsIDString)
                        };
                        details.Add(detail);
                    }
                    reopenServiceData.ServiceDetails = details.ToArray();

                    var reopenRequest = new Camstar.WCF.Services.isDefectReopen_Request();
                    var reopenResult = new Camstar.WCF.Services.isDefectReopen_Result();

                    ResultStatus resultStatus = reopenService.ExecuteTransaction(reopenServiceData, reopenRequest, out reopenResult);
                    responseData = new SimpleResponse(resultStatus.IsSuccess, resultStatus.Message, resultStatus.ExceptionData != null ? resultStatus.ExceptionData.ToString() : "");

                    if (!resultStatus.IsSuccess)
                    {
                        responseData.Message = resultStatus.ExceptionData != null ? resultStatus.ExceptionData.Description : resultStatus.ToString();
                        responseData.Exception = responseData.Message;
                    }
                }

            }
            catch (Exception ex)
            {
                responseData = new SimpleResponse(ex);
                //throw ex;
            }
            return responseData;
        }

        public class SimpleResponse
        {
            public string Message;
            public string Exception;
            public bool Success;
            public string User;
            public string Pwd;

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

        static protected SimpleResponse CreateResponseInfo(ResultStatus status)
        {
            string exception = status.ExceptionData != null ? status.ExceptionData.Description : string.Empty;
            return new SimpleResponse(status.IsSuccess, status.Message, exception);
        }

        public class isDefectDocumentResponse : SimpleResponse
        {
            public string StatusReason { get; set; }
            public string StatusCode { get; set; }
            public string ResourceName { get; set; }
            public List<isDefectDocuments> isDefectDocumentList { get; set; }
            public isDefectDocumentResponse(SimpleResponse simple) : base(simple.Success, simple.Message, simple.Exception)
            {
                isDefectDocumentList = new List<isDefectDocuments>();
            }
            public isDefectDocumentResponse()
            {
                StatusReason = "";
            }
        }

        public class isDefectDocuments
        {
            public string FileExtension { get; set; }
            public string Name { get; set; }
            public string ID { get; set; }
            public string FilePath { get; set; }
            public string Revision { get; set; }
            public string FileName { get; set; }
            public string Identifier { get; set; }
        }

        [DataContract]
        public class DefectActionRequest
        {
            private List<ProductionDefect> _defects = new List<ProductionDefect>();
            [DataMember]
            public List<ProductionDefect> Defects
            {
                get { return _defects; }
                set { _defects = value; }
            }

        }

        public class ProductionDefect
        {
            public string isCurrentDefectsIDString { get; set; }
        }

    }
}
