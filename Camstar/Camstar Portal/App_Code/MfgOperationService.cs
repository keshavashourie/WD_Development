// © Siemens 2021 Siemens Product Lifecycle Management Software Inc.
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework.Utilities;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using OM = Camstar.WCF.ObjectStack;

using System.Data;
using System;
using System.Runtime.Serialization;
using Camstar.Util;
using Camstar.WebPortal.Utilities;
using System.Web;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Linq;

namespace WebClientPortal
{
    /// <summary>
    /// Functionality implemented by the MfgOperation CDO.  Partially implemented in IS.
    /// </summary>
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public partial class MfgOperationService
    {
        /// <param name="mfgOrderId"></param>
        /// <param name="productId"></param>
        /// <param name="specId"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        public DocumentSet[] GetMfgOperationDocs(string mfgOrderId, string productId = null, string specId = null, string productName = null, string productRev = null)
        {
            DocumentSet[] docJSON = null;

            var serviceData = new MfgOperation
            {
                Spec = new RevisionedObjectRef { ID = specId },
                Product = !string.IsNullOrEmpty(productId) ? new RevisionedObjectRef { ID = productId } : new RevisionedObjectRef(productName, productRev),
                MfgOrder = new NamedObjectRef { ID = mfgOrderId }
            };

            if (string.IsNullOrEmpty(productId) && string.IsNullOrEmpty(productRev))
            {
                serviceData.Product.RevisionOfRecord = true;
            }

            var serviceRequest = new MfgOperation_Request
            {
                Info = new MfgOperation_Info
                {
                    DocumentSets = new DocumentSet_Info
                    {
                        DocumentEntries = new DocumentEntry_Info() { RequestValue = true },
                        Name = new Info(),
                        RequestValue = true,
                        DocSetModelObjectName = new Info(true),
                        DocSetModelObjectTypeName = new Info(true)
                    },
                    Product = new Info(true)
                }
            };

            MfgOperation_Result result = new MfgOperation_Result();
            var mfgOperationSvc = new Camstar.WCF.Services.MfgOperationService(FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);
            ResultStatus status = mfgOperationSvc.Load(serviceData, serviceRequest, out result);
            if (status.IsSuccess)
            {
                if (result.Value.DocumentSets != null)
                {
                    docJSON = result.Value.DocumentSets;
                }
                else
                {
                    docJSON = null;
                }

            }

            return docJSON;
        }

        [OperationContract]
        public ResourceDetailsResponse GetResourceComponents(string resourceName)
        {
            ResourceDetailsResponse resourceDetails = new ResourceDetailsResponse();

            var currentUserProfile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
            QueryService service = new QueryService(currentUserProfile);
            OM.QueryOptions options = new OM.QueryOptions();
            OM.RecordSet componentRecords = new OM.RecordSet();

            OM.QueryParameters queryParamsObj = new OM.QueryParameters();
            OM.QueryParameter[] queryParams = new OM.QueryParameter[1]
            {
                new OM.QueryParameter("ResourceName", resourceName)
            };
            queryParamsObj.Parameters = queryParams;

            string query = "GetCurrentHVResourceSetupDetails";
            OM.ResultStatus resultStatus = service.Execute(query, queryParamsObj, options, out componentRecords);
            resourceDetails = new ResourceDetailsResponse(CreateResponse(resultStatus));
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
                        rc.UseHVTraceability = Utilities.GetBool(Utilities.GetDataRowField(dr, "UseHVTraceability"));
                        if (rc.UseHVTraceability)
                        {
                            rc.Lot = Utilities.GetDataRowField(dr, "CompId");
                            rc.Product = Utilities.GetDataRowField(dr, "CompName");
                            rc.SlotName = Utilities.GetDataRowField(dr, "Slot");
                            rc.SubSlotName = Utilities.GetDataRowField(dr, "SubSlot");

                            resourceDetails.Components.Add(rc);
                        }
                    }
                }
            }
            return resourceDetails;
        }

        public bool HasMaterialList(string mfgOrderId)
        {
            bool hasMaterialList = false;

            var currentUserProfile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
            QueryService oService = new QueryService(currentUserProfile);
            OM.QueryOptions opts = new OM.QueryOptions();
            OM.RecordSet oData = new OM.RecordSet();

            OM.QueryParameters objQueryParameters = new OM.QueryParameters();
            OM.QueryParameter[] queryParameters = new OM.QueryParameter[1]
            {
                    new QueryParameter("MfgOrderId", mfgOrderId)
            };
            objQueryParameters.Parameters = queryParameters;
            string query = "GetMfgOrderMaterialCount";
            ResultStatus status = oService.Execute(query, objQueryParameters, opts, out oData);

            if (status.IsSuccess && oData != null)
            {
                DataTable dt = oData.GetAsDataTable();
                if (dt != null && dt.Rows.Count > 0)
                {
                    int count = Convert.ToInt32(Utilities.GetDataRowField(dt.Rows[0], "CNT"));
                    hasMaterialList = count > 0;
                }
            }

            return hasMaterialList;
        }

        public BomPartNumbers GetMfgOrderMaterialRequirements(string mfgOrderId, string routeStepId, out bool hasMaterialList)
        {
            hasMaterialList = false;
            BomPartNumbers bom = new BomPartNumbers();

            hasMaterialList = HasMaterialList(mfgOrderId);
            if (hasMaterialList)
            {
                var currentUserProfile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
                QueryService oService = new QueryService(currentUserProfile);
                OM.QueryOptions opts = new OM.QueryOptions();
                OM.RecordSet oData = new OM.RecordSet();

                OM.QueryParameters objQueryParameters = new OM.QueryParameters();
                OM.QueryParameter[] queryParameters = new OM.QueryParameter[2]
                {
                    new QueryParameter("MfgOrderId", mfgOrderId),
                    new QueryParameter("RouteStepId", routeStepId)
                };
                objQueryParameters.Parameters = queryParameters;
                string query = "GetMfgOrderStepMaterials";
                OM.ResultStatus resStat = oService.Execute(query, objQueryParameters, opts, out oData);

                // convert to objects to return
                if (resStat.IsSuccess && oData.Rows != null)
                {
                    DataTable dt = oData.GetAsDataTable();
                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            string productName = Utilities.GetDataRowField(dr, "ProductName");
                            string productRev = Utilities.GetDataRowField(dr, "ProductRevision");
                            double qty = Utilities.GetDouble(Utilities.GetDataRowField(dr, "QtyRequired"));

                            BomItem bomItem = new BomItem();
                            bomItem.PartNumber = productName;
                            bomItem.Revision = productRev;
                            bomItem.QtyRequired = qty;
                            bomItem.ProducingOrder = Utilities.GetDataRowField(dr, "ProducingOrder");
                            bomItem.AssemblySequence = Utilities.GetInt(Utilities.GetDataRowField(dr, "AssemblySequence"));
                            if (string.IsNullOrEmpty(routeStepId))
                            {
                                bomItem.Operation = Utilities.GetDataRowField(dr, "RouteStepName");
                                bomItem.Spec = bomItem.Operation;
                                bomItem.RouteStepId = Utilities.GetDataRowField(dr, "RouteStepId");
                                bomItem.SpecRevision = "1";
                                bomItem.RouteStepName = Utilities.GetDataRowField(dr, "RouteStepName");
                            }
                            bom.BomPartNumber.Add(bomItem);
                        }
                    }
                }
            }


            return bom;
        }
        public string GetBomForMfgOrder(string mfgOrderId, out bool erpBOM)
        {
            string bomId = string.Empty;
            erpBOM = false;

            var currentUserProfile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
            QueryService oService = new QueryService(currentUserProfile);
            OM.QueryOptions opts = new OM.QueryOptions();
            OM.RecordSet oData = new OM.RecordSet();

            OM.QueryParameters objQueryParameters = new OM.QueryParameters();
            OM.QueryParameter[] queryParameters = new OM.QueryParameter[1]
            {
                new OM.QueryParameter("MfgOrderId", mfgOrderId)
            };
            objQueryParameters.Parameters = queryParameters;

            string query = "GetMfgOrderBomId";
            OM.ResultStatus resStat = oService.Execute(query, objQueryParameters, opts, out oData);

            if (resStat.IsSuccess)
            {
                if (oData.Rows != null)
                {
                    DataTable dt = oData.GetAsDataTable();
                    if (dt.Rows.Count > 0)
                    {
                        DataRow dr = dt.Rows[0];
                        string bomID = Utilities.GetDataRowField(dr, "BomID");
                        string bomBaseID = Utilities.GetDataRowField(dr, "RevOfRcdId");
                        string erpBOMID = Utilities.GetDataRowField(dr, "ERPBOMId");
                        string erpBOMBaseID = Utilities.GetDataRowField(dr, "ERPBOMBaseId");

                        if (!string.IsNullOrEmpty(bomID) && !Utilities.IsEmptyId(bomID))
                            bomId = bomID;
                        else if (!string.IsNullOrEmpty(bomBaseID) && !Utilities.IsEmptyId(bomBaseID))
                            bomId = bomBaseID;
                        else if (!string.IsNullOrEmpty(erpBOMID) && !Utilities.IsEmptyId(erpBOMID))
                        {
                            bomId = erpBOMID;
                            erpBOM = true;
                        }
                        else if (!string.IsNullOrEmpty(erpBOMBaseID) && !Utilities.IsEmptyId(erpBOMBaseID))
                        {
                            bomId = erpBOMBaseID;
                            erpBOM = true;
                        }
                    }
                }
            }


            return bomId;
        }

        public BomPartNumbers GetMfgOrderMaterialList(string routeStepId, string mfgOrderId, out bool hasMaterialList)
        {
            hasMaterialList = false;
            BomPartNumbers bom = new BomPartNumbers();

            if (!string.IsNullOrEmpty(mfgOrderId))
            {
                return this.GetMfgOrderMaterialRequirements(mfgOrderId, routeStepId, out hasMaterialList);
            }

            return bom;
        }

        public BomPartNumbers GetBomSpecData(string bomId, string specId, string routeStepId, string mfgOrderId, bool grouped)
        {
            BomPartNumbers bom = new BomPartNumbers();

            bool hasMaterialList = false;
            bom = GetMfgOrderMaterialList(routeStepId, mfgOrderId, out hasMaterialList);

            bool isERPBOM = string.IsNullOrEmpty(specId) && !string.IsNullOrEmpty(routeStepId);

            if (!hasMaterialList)
            {
                // No Material List Items for the Mfg Order.  Check either BOMMaterialListItems or ProductMaterialListItems 
                var currentUserProfile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
                QueryService oService = new QueryService(currentUserProfile);
                QueryOptions opts = new QueryOptions();
                RecordSet oData;

                QueryParameters objQueryParameters = new QueryParameters();
                QueryParameter[] queryParameters = new QueryParameter[2]
                {
                    new QueryParameter("BomId", bomId),
                    isERPBOM ? new QueryParameter("RouteStepId", routeStepId) : new QueryParameter("SpecId", specId)
                };
                objQueryParameters.Parameters = queryParameters;
                string query = isERPBOM ? "GetBomRouteStepItemCount" : "GetBomSpecItemCount";

                ResultStatus status = oService.Execute(query, objQueryParameters, opts, out oData);

                if (status.IsSuccess)
                {
                    if (oData != null)
                    {
                        DataTable dt = oData.GetAsDataTable();
                        if (dt != null && dt.Rows.Count > 0)
                        {
                            int itemCount = Utilities.GetInt(Utilities.GetDataRowField(dt.Rows[0], "ItemCount"));

                            string queryText = "";

                            QueryParameters objQueryParameters1 = new QueryParameters();
                            QueryParameter[] queryParameters1 = new QueryParameter[1];
                            // If we have some list items, load them
                            if (itemCount > 0)
                            {

                                queryParameters1 = new QueryParameter[2]
                                {
                                        new QueryParameter("BomId", bomId),
                                        new QueryParameter("SpecId", specId)
                                };
                                // Figure out correct query
                                if (grouped)
                                {
                                    if (isERPBOM)
                                        queryText = "GetRouteStepBomMaterialRequirement";
                                    else
                                        queryText = "GetBomMaterialRequirement";
                                }
                                objQueryParameters1.Parameters = queryParameters1;
                                status = oService.Execute(queryText, objQueryParameters1, opts, out oData);
                                if (status.IsSuccess && oData != null)
                                {
                                    dt = oData.GetAsDataTable();
                                    if (dt != null && dt.Rows.Count > 0)
                                    {
                                        foreach (DataRow dr in dt.Rows)
                                        {
                                            string productName = Utilities.GetDataRowField(dr, "ProductName"); ;
                                            string productRev = Utilities.GetDataRowField(dr, "ProductRevision");
                                            double qty = Utilities.GetDouble(Utilities.GetDataRowField(dr, "QtyRequired"));

                                            BomItem bomItem = new BomItem();
                                            bomItem.PartNumber = productName;
                                            bomItem.Revision = productRev;
                                            bomItem.QtyRequired = qty;
                                            bomItem.ProducingOrder = Utilities.GetDataRowField(dr, "ProducingOrder");

                                            if (!grouped)
                                            {
                                                int issueType = isERPBOM ? Utilities.GetInt(Utilities.GetDataRowField(dr, "IssueControl")) : Utilities.GetInt(Utilities.GetDataRowField(dr, "MaterialTxnLogic"));

                                                bomItem.ID = isERPBOM ? Utilities.GetDataRowField(dr, "BOMMaterialListItemId") : Utilities.GetDataRowField(dr, "ProductMaterialListItemId");
                                                bomItem.BomItemID = bomItem.ID;
                                                if (isERPBOM)
                                                {
                                                    bomItem.Operation = Utilities.GetDataRowField(dr, "Name");
                                                    bomItem.Spec = bomItem.Operation;
                                                }
                                                else
                                                {
                                                    bomItem.Operation = Utilities.GetDataRowField(dr, "Operation");
                                                    bomItem.Spec = Utilities.GetDataRowField(dr, "SpecName");
                                                    bomItem.SpecRevision = Utilities.GetDataRowField(dr, "SpecRevision");
                                                }
                                                bomItem.IssueControl = (IssueControlType)issueType;
                                            }
                                            bom.BomPartNumber.Add(bomItem);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return bom;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mfgOrderId"></param>
        /// <param name="errMsg"></param>
        public void LoadMfgOrderAttributes(string mfgOrderId, ref AttributesResponse response)
        {
            UserProfile profile = HttpContext.Current.Session[Camstar.WebPortal.Constants.SessionConstants.UserProfile] as UserProfile;
            MfgOrderMaint mfgOrderMaint = new MfgOrderMaint() 
            {
                ObjectToChange = new NamedObjectRef()
                {
                   ID = mfgOrderId
                }
            };
            MfgOrderMaint_Info mfgOrderMaintInfo = new MfgOrderMaint_Info()
            {
                ObjectChanges = new MfgOrderChanges_Info()
                {
                    Name = FieldInfoUtil.RequestValue(),
                    Attributes = new UserAttributeChanges_Info { RequestValue = true}
                }
            };

            MfgOrderMaint_Result result = null;

            MfgOrderMaintService mfgOrderMaintService = new MfgOrderMaintService(profile);

            ResultStatus resultStatus = mfgOrderMaintService.Load(mfgOrderMaint, new MfgOrderMaint_Request { Info = mfgOrderMaintInfo }, out result);
            response.Success = resultStatus.IsSuccess;
            if (resultStatus.IsSuccess)
            {
                response.MfgOrderAttributes = result.Value.ObjectChanges?.Attributes?.ToDictionary(a => a.Name.ToString(), a => a.AttributeValue.ToString());
                response.Message = resultStatus.Message;
            }
            else
            {
                response.Exception = resultStatus.ExceptionData.Description;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public void LoadProductAttributes(string productId, ref AttributesResponse response)
        {
            UserProfile profile = HttpContext.Current.Session[Camstar.WebPortal.Constants.SessionConstants.UserProfile] as UserProfile;
            ProductMaint productMaint = new ProductMaint()
            {
                ObjectToChange = new RevisionedObjectRef()
                {
                    ID = productId
                }
            };
            ProductMaint_Info productMaintInfo = new ProductMaint_Info()
            {
                ObjectChanges = new ProductChanges_Info()
                {
                    Name = FieldInfoUtil.RequestValue(),
                    Attributes = new UserAttributeChanges_Info { RequestValue = true }
                }
            };
            
            ProductMaint_Result resultDoc = null;

            ProductMaintService productMaintService = new ProductMaintService(profile);
            
            ResultStatus resultStatus = productMaintService.Load(productMaint, new ProductMaint_Request { Info = productMaintInfo }, out resultDoc);
            response.Success = resultStatus.IsSuccess;

            if (resultStatus.IsSuccess)
            {
                response.ProductAttributes = resultDoc.Value.ObjectChanges?.Attributes?.ToDictionary(a => a.Name.ToString(), a => a.AttributeValue.ToString());
                response.Message = resultStatus.Message;
            }
            else
            {
                response.Exception = resultStatus.ExceptionData.Description;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourceName"></param>
        /// <param name="response"></param>
        public void LoadResourceAttributes(string resourceName, ref AttributesResponse response)
        {
            UserProfile profile = HttpContext.Current.Session[Camstar.WebPortal.Constants.SessionConstants.UserProfile] as UserProfile;
            ResourceMaint resourceMaint = new ResourceMaint()
            {
                ObjectToChange = new NamedObjectRef()
                {
                    Name = resourceName
                }
            };
            ResourceMaint_Info resourceMaintInfo = new ResourceMaint_Info()
            {
                ObjectChanges = new ResourceChanges_Info()
                {
                    Name = FieldInfoUtil.RequestValue(),
                    Attributes = new UserAttributeChanges_Info { RequestValue = true }
                }
            };

            ResourceMaint_Result resultDoc = null;

            ResourceMaintService productMaintService = new ResourceMaintService(profile);

            ResultStatus resultStatus = productMaintService.Load(resourceMaint, new ResourceMaint_Request { Info = resourceMaintInfo }, out resultDoc);
            response.Success = resultStatus.IsSuccess;

            if (resultStatus.IsSuccess)
            {
                response.ResourceAttributes = resultDoc.Value.ObjectChanges?.Attributes?.ToDictionary(a => a.Name.ToString(), a => a.AttributeValue.ToString());
                response.Message = resultStatus.Message;
            }
            else
            {
                response.Exception = resultStatus.ExceptionData.Description;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mfgOrderId"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        [OperationContract]
        public AttributesResponse LoadAttributes(string mfgOrderId, string productId, string resourceName)
        {
            AttributesResponse response = new AttributesResponse();

            try
            {
                if (mfgOrderId != null)
                {
                    LoadMfgOrderAttributes(mfgOrderId, ref response);
                }

                if (response.Success && productId != null)
                {
                    LoadProductAttributes(productId, ref response);
                }

                if(response.Success && resourceName != null)
                {
                    LoadResourceAttributes(resourceName, ref response);
                }
            }
            catch (Exception ex)
            {
                response = new AttributesResponse(ex);
            }

            return response;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="SpecId"></param>
        /// <param name="MfgOrderId"></param>
        /// <param name="RouteStepId"></param>
        /// <param name="ResourceName"></param>
        /// <returns></returns>
        [OperationContract]
        public ResourceDetailsResponse LoadMfgOrderResourceMaterialRequirements(string SpecId, string MfgOrderId, string RouteStepId, string ResourceName)
        {
            ResourceDetailsResponse responseData = null;
            try
            {
                string resourceName = ResourceName;
                string mfgOrderId = MfgOrderId;
                string specId = SpecId;
                string routeStepId = RouteStepId;
                responseData = GetResourceComponents(resourceName);

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
                                if (comp.QtyRequired == 0)
                                    comp.QtyRequired = item.QtyRequired;
                                comp.BomItemId = item.ID;
                                comp.ProducingOrder = item.ProducingOrder;
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


        static protected MfgOperationResponse CreateResponse(ResultStatus status)
        {
            string exception = status.ExceptionData != null ? status.ExceptionData.Description : string.Empty;
            return new MfgOperationResponse(status.IsSuccess, status.Message, exception);
        }

        public class ResourceDetailsResponse : MfgOperationResponse
        {
            public string StatusReason { get; set; }
            public string StatusCode { get; set; }
            public string ResourceName { get; set; }
            public List<ResourceComponentItem> Components { get; set; }
            public ResourceDetailsResponse(MfgOperationResponse simple) : base(simple.Success, simple.Message, simple.Exception)
            {
                Components = new List<ResourceComponentItem>();
            }
            public ResourceDetailsResponse()
            {
                StatusReason = "";
            }
            public List<BomItem> MaterialList { get; set; }
        }

        public class ResourceComponentItem
        {
            public string Product { get; set; }
            public string Lot { get; set; }
            public string SlotName { get; set; }
            public string SubSlotName { get; set; }
            public double QtyRequired { get; set; }
            public string ProducingOrder { get; set; }
            public bool Satisfied { get; set; }
            public string BomItemId { get; set; }
            public string RowNumber { get; set; }
            public string RowId { get; set; }
            public bool UseHVTraceability { get; set; }
            public bool PartiallySatisfied { get; set; }
            public double TotalQuantity { get; set; }
            public double BomQty { get; set; }
        }

        protected bool ItemIsAvailable(DateTime from, DateTime to)
        {
            DateTime currentTime = DateTime.UtcNow;

            // No Effective From and To Times set
            if (from == DateTime.MinValue && to == DateTime.MinValue)
                return true;

            // Current Time is between the Effective From and To Times
            if (from != DateTime.MinValue && to != DateTime.MinValue && (from <= currentTime && to > currentTime))
                return true;

            // Current Time is after Effective From Time, Effective To Time not set
            if (from != DateTime.MinValue && to == DateTime.MinValue && (from <= currentTime))
                return true;

            // Current Time is before Effective To Time, Effective From Time not set
            if (from == DateTime.MinValue && to != DateTime.MinValue && (to > currentTime))
                return true;

            return false;
        }
        public partial class BomRequest
        {
            public string SpecId;
            public string MfgOrderId;
            public string RouteStepId;
            public string ResourceName;
        }
        public partial class BomItem
        {
            public BomItem()
            {
                uniqueIDField = Guid.NewGuid().ToString("N");
            }
            public BomItem(BomItem copy, bool deep)
            {
                IDField = copy.ID;
                uniqueIDField = Guid.NewGuid().ToString("N");
                partNumberField = copy.PartNumber;
                revisionField = copy.Revision;
                productIDField = copy.ProductID;
                qtyRequiredField = copy.QtyRequired;
                specNameField = copy.Spec;
                specRevisionField = copy.SpecRevision;
                specIdField = copy.SpecId;
                operationField = copy.Operation;
                routeStepIdField = copy.RouteStepId;
                routeStepNameField = copy.RouteStepName;
                ProducingOrder = copy.ProducingOrder;
                AssemblySequence = copy.AssemblySequence;
            }
            private string IDField;
            private string bomItemIDField;
            private string partNumberField;
            private string revisionField;
            private string productIDField;
            private double qtyRequiredField;
            private string specIdField;
            private string specNameField;
            private string specRevisionField;
            private string operationField;
            private string routeStepIdField;
            private string routeStepNameField;
            private string uniqueIDField;
            public int AssemblySequence;
            public string RowNumber { get; set; }
            public string RowId { get; set; }
            public DateTime EffectiveFromDateGMT;
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public DateTime EffectiveFromDate;
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public DateTime EffectiveThruDateGMT;
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public DateTime EffectiveThruDate;

            private IssueControlType issueControlField;
            public IssueControlType IssueControl
            {
                get
                {
                    return this.issueControlField;
                }
                set
                {
                    this.issueControlField = value;
                }
            }

            public string ID
            {
                get { return !string.IsNullOrEmpty(this.IDField) ? this.IDField : this.bomItemIDField; }
                set { this.IDField = value; }
            }
            public string BomItemID
            {
                get { return this.bomItemIDField; }
                set { this.bomItemIDField = value; }
            }

            [System.Xml.Serialization.XmlAttributeAttribute("partNumber")]
            public string PartNumber
            {
                get
                {
                    return this.partNumberField;
                }
                set
                {
                    this.partNumberField = value;
                }
            }


            [System.Xml.Serialization.XmlAttributeAttribute("productID")]
            public string ProductID
            {
                get
                {
                    return this.productIDField;
                }
                set
                {
                    this.productIDField = value;
                }
            }

            [System.Xml.Serialization.XmlAttributeAttribute("qtyRequired")]
            public double QtyRequired
            {
                get
                {
                    return this.qtyRequiredField;
                }
                set
                {
                    this.qtyRequiredField = value;
                }
            }
            [System.Xml.Serialization.XmlAttributeAttribute("revision")]
            public string Revision
            {
                get
                {
                    return this.revisionField;
                }
                set
                {
                    this.revisionField = value;
                }
            }

            [System.Xml.Serialization.XmlAttributeAttribute("specId")]
            public string SpecId
            {
                get
                {
                    return this.specIdField;
                }
                set
                {
                    this.specIdField = value;
                }
            }


            [System.Xml.Serialization.XmlAttributeAttribute("spec")]
            public string Spec
            {
                get
                {
                    return this.specNameField;
                }
                set
                {
                    this.specNameField = value;
                }
            }


            [System.Xml.Serialization.XmlAttributeAttribute("specRevision")]
            public string SpecRevision
            {
                get
                {
                    return this.specRevisionField;
                }
                set
                {
                    this.specRevisionField = value;
                }
            }


            [System.Xml.Serialization.XmlAttributeAttribute("operation")]
            public string Operation
            {
                get
                {
                    return this.operationField;
                }
                set
                {
                    this.operationField = value;
                }
            }


            [System.Xml.Serialization.XmlAttributeAttribute("routeStepId")]
            public string RouteStepId
            {
                get
                {
                    return this.routeStepIdField;
                }
                set
                {
                    this.routeStepIdField = value;
                }
            }


            [System.Xml.Serialization.XmlAttributeAttribute("routeStepName")]
            public string RouteStepName
            {
                get
                {
                    return this.routeStepNameField;
                }
                set
                {
                    this.routeStepNameField = value;
                }
            }


            [System.Xml.Serialization.XmlAttributeAttribute("id")]
            public string UniqueID
            {
                get
                {
                    return this.uniqueIDField;
                }
                set
                {
                    this.uniqueIDField = value;
                }
            }

            [System.Xml.Serialization.XmlAttributeAttribute("producingOrder")]
            public string ProducingOrder { get; set; }

        }
        public partial class BomPartNumbers
        {
            const string ModuleName = "BomPartNumbers";
            public string Message;
            public string Exception;
            public bool Success;
            private List<BomItem> bomPartNumberField;

            [System.Xml.Serialization.XmlElementAttribute("BomPartNumber")]
            public List<BomItem> BomPartNumber
            {
                get
                {
                    if ((this.bomPartNumberField == null))
                    {
                        this.bomPartNumberField = new List<BomItem>();
                    }
                    return this.bomPartNumberField;
                }
                set
                {
                    this.bomPartNumberField = value;
                }
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="id"></param>
            /// <returns></returns>
            public BomItem FindItem(string id)
            {
                return BomPartNumber.Find(s => string.Compare(s.ID, id, true) == 0);
            }

        }

        public class MfgOperationResponse
        {
            /// <summary>
            /// Success message(?)
            /// </summary>
            public string Message;

            /// <summary>
            /// Exception message
            /// </summary>
            public string Exception;
            public bool Success;
            public string User;
            public string Pwd;
            public string Data;

            public MfgOperationResponse() { }

            public MfgOperationResponse(Exception ex)
            {
                Success = false;
                Message = string.Empty;
                Exception = ex.Message;
            }

            public MfgOperationResponse(bool success, string message, string exception)
            {
                Success = success;
                Message = message;
                Exception = exception;
            }

        }

        /// <summary>
        /// For getting User Attributes 
        /// </summary>
        public class AttributesResponse : MfgOperationResponse
        {
            public AttributesResponse() { }

            public AttributesResponse(Exception ex) : base(ex) { }

            /// <summary>
            /// Key: attribute name, Value: attribute value
            /// </summary>
            public Dictionary<string, string> MfgOrderAttributes { get; set; }
            public Dictionary<string, string> ProductAttributes { get; set; }
            public Dictionary<string, string> ResourceAttributes { get; set; }
        }

        public enum IssueControlType
        {
            Undefined = 0,
            Serialized = 1,
            Bulk = 2,
            LotAndStockPoint = 3,
            StockPointOnly = 4,
            NoTracking = 5,
            CommentOnly = 6
        }

    }

}
