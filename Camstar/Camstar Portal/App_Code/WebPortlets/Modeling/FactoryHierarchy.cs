// Copyright Siemens 2024
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.UI;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;

namespace Camstar.WebPortal.WebPortlets.Modeling
{
    /// <summary>
    /// Code behind for the Factory Hierarchy Model (FHM) page. This class will get the data from the WCF call to the FactoryHierarchyModelInquiry 
    /// Service, and serialize it into JSON. This class will also define the structure and metadata of the tree from here in a JSON document.
    /// That data will be be sent to the client to be processed by Javascript.
    /// 
    /// KEY CONCEPTS:
    /// Tree Definition Data Document - The tree definition data defines the structure, and metadata about elements in the tree. 
    /// The reason why this is done on the server instead of hard coded in the Javascript is so other workspaces can inherit
    /// this class. Using inheritence this child class cna and then alter and expand on the tree as needed in an OOP way.
    /// So the structure will be defined on the server and then sent to the serer as a JSON document
    /// </summary>
    public class FactoryHierarchy : MatrixWebPart
    {
        #region Constants
        protected const string _enterpriseCollectionKey = "Enterprises";
        protected const string _factoryCollectionKey = "Factories";
        protected const string _areaCollectionKey = "Areas";
        protected const string _cellCollectionKey = "Cells";
        protected const string _equipmentCollectionKey = "Equipment";
        #endregion

        #region Properties

        /// <summary>
        /// Labels for localization
        /// </summary>
        LabelCache CachedLabels { get; set; }

        /// <summary>
        /// A hidden control that will store the JSON data for the tree
        /// </summary>
        protected virtual CWC.TextBox TreeData
        {
            get { return Page.FindCamstarControl("treeData") as CWC.TextBox; }
        }

        /// <summary>
        /// The hidden field storing definition JSON data of the tree nodes.
        /// </summary>
        protected virtual CWC.TextBox TreeDefinition
        {
            get { return Page.FindCamstarControl("treeDefinition") as CWC.TextBox; }
        }
        #endregion

        #region Methods
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            Dictionary<string, string> clientLabels = GetClientLabels();
            string labelsJson = JsonConvert.SerializeObject(clientLabels, Formatting.Indented);
            string factoryName;
            string settingsName;
            string srcApiUrl;
            var resultStatus = WebClientPortal.FactoryHierarchyService.GetSrcApi(out factoryName, out settingsName, out srcApiUrl);

            ScriptManager.RegisterStartupScript(
            this,
            this.GetType(),
            "initializeFactoryHierarchy",
            $"factoryHierarchy.initialize('{srcApiUrl}', " + labelsJson + ");",
            true);
        }

        protected Dictionary<string, string> GetClientLabels()
        {
            var labels = new Dictionary<string, string>()
            {
                { "AddButton", GetLocalizedLabel("AddButton", "Add") },
                //{ "LinkButton", GetLocalizedLabel("LinkButton", "Link Existing") },
                { "ExpandButton", GetLocalizedLabel("Lbl_ExpandAll", "Expand") },
                { "CollapseButton", GetLocalizedLabel("Lbl_CollapseAll", "Collapse") },
                { "Search", GetLocalizedLabel("LblMenuSearch", "Search") },
                { "ImportButton", GetLocalizedLabel("ImportButton", "Import") },
                { "NoImportContent", GetLocalizedLabel("ExpImpNoImportContents", "Unable to find import content record(s).") },
                { "StatusMessage_ServerError", GetLocalizedLabel("StatusMessage_ServerError", "An error has occurred. Please contact your system administrator.") },
                { "FhmLinkChildren", GetLocalizedLabel("FhmLinkChildren", "Add Existing Objects") },
                { "Name", GetLocalizedLabel("Lbl_Name", "Name") },
                { "Description", GetLocalizedLabel("Lbl_Description", "Description") }
            };

            return labels;
        }

        protected override IEnumerable<ScriptReference> GetScriptReferences()
        {
            yield return new ScriptReference("~/Scripts/CRModules.js");
            yield return new ScriptReference("~/Scripts/CRPickGrid.js");
            yield return new ScriptReference("~/Scripts/TreeConfiguration.js");
            yield return new ScriptReference("~/Scripts/TreeCollectionDefinition.js");
            yield return new ScriptReference("~/Scripts/TreeDataTypeDefinition.js");
            yield return new ScriptReference("~/Scripts/TreeDataProvider.js");
            yield return new ScriptReference("~/Scripts/TreeControl.js");
            yield return new ScriptReference("~/Scripts/FactoryHierarchy.js");
        }

        protected string GetLocalizedLabel(string labelId, string alternateText)
        {
            string text = CachedLabels != null ? CachedLabels.GetLabelByName(labelId).Value : string.Empty;

            if (string.IsNullOrEmpty(text))
                text = $"~{alternateText}";

            return text;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            GetLabels();

            if (!Page.IsPostBack)
            {
                TreeDefinition.Data = GenerateCollectionDefinitionJsonDocument();
                TreeData.Data = GenerateJSONDataDocument();
            }

        }

        protected virtual void OnGenerateCollectionDefinitionJsonDocument(Dictionary<string, dynamic> definitions) { }

        protected string GetFormName(string service, string defaultValue)
        {
            string result = defaultValue;
            if (CamstarPortalSection.Settings.CDOFormsSettings != null && CamstarPortalSection.Settings.CDOFormsSettings.CDOForms != null)
            {
                var formInfo = CamstarPortalSection.Settings.CDOFormsSettings.CDOForms.FirstOrDefault(cdoForm => string.Equals(cdoForm.Service, service, StringComparison.OrdinalIgnoreCase));
                if (formInfo != null)
                    result = formInfo.PageName;
            }
            return result;
        }

        /// <summary>
        /// Generate a tree definition JSON document to send to the client. The Javascript functions will
        /// use this data to define how nodes in the FHM tree should be created. The reason why we have this
        /// JSON document versus it being hard coded in the Javascript as this methodology gives us the ability 
        /// to create new definitions in higher workspaces.
        /// </summary>
        /// <returns>A string representation of the definition data in JSON format</returns>
        string GenerateCollectionDefinitionJsonDocument()
        {
            string enterpriseForm = GetFormName("EnterpriseMaint", "EnterpriseMaint_VP");
            string factoryForm = GetFormName("FactoryMaint", "FactoryMaint_VP");
            string resourceForm = GetFormName("ResourceMaint", "Resource_VP");

            // Get label values for localization
            string enterpriseTreeNodeTitle = GetLocalizedLabel("CSICDOName_Enterprise", "Enterprise");
            string enterpriseCDOTitle = GetLocalizedLabel("CSICDOName_Enterprise", "Enterprise");
            string factoryTreeNodeTitle = GetLocalizedLabel("CSICDOName_Factory", "Factory");
            string factoryCDOTitle = GetLocalizedLabel("CSICDOName_Factory", "Factory");
            string areaTreeNodeTitle = GetLocalizedLabel("FactoryLevelEnum_Area", "Area");
            string cellTreeNodeTitle = GetLocalizedLabel("FactoryLevelEnum_Cell", "Cell");
            string equipmentTreeNodeTitle = GetLocalizedLabel("FactoryLevelEnum_Equipment", "Equipment");
            string resourceCDOTitle = GetLocalizedLabel("CSICDOName_Resource", "Resource");
            string linkPopupEnterprise = GetLocalizedLabel("Lbl_SelectFactories", "Select Factories");
            string linkPopupResourceAreas = GetLocalizedLabel("Lbl_SelectResourcesAsAreas", "Select Resources to Link as Areas");
            string linkPopupResourceCells = GetLocalizedLabel("Lbl_SelectResourcesAsCells", "Select Resources to Link as Cells");
            string linkPopupResourceEquipment = GetLocalizedLabel("Lbl_SelectResourcesAsEquipment", "Select Resources to Link as Equipment");

            Dictionary<string, dynamic> definitions = new Dictionary<string, dynamic>();

            // Create new collection definition objects and add them to the definition's dictionary

            // Create Enterprise Config
            var enterpriseGetOperation = CreateCustomDataGetOperation("enterpriseId", "GetEnterprise", "enterprise");
            var enterpriseDataType = CreateDataType("Enterprise", enterpriseTreeNodeTitle, "");
            var enterpriseLinkItemPopupConfig = CreateLinkableObjectsPopupConfig("GetLinkableFactories", "LinkFactoriesToEnterprise", linkPopupEnterprise);
            var enterpriseCustomData = CreateCollectionDefinitionCustomData(5590, 1240, "Enterprise", enterpriseCDOTitle, enterpriseForm, enterpriseGetOperation, null, enterpriseLinkItemPopupConfig);
            var enterprise = CreateCollectionDefinition(enterpriseTreeNodeTitle, "tree-enterprise", "", "", enterpriseDataType, _factoryCollectionKey, enterpriseCustomData);
            definitions.Add(_enterpriseCollectionKey, enterprise);

            // Create Factory Config
            var factoryGetOperation = CreateCustomDataGetOperation("factoryId", "GetFactory", "factory");
            var factoryDataType = CreateDataType("Factory", factoryTreeNodeTitle, "EnterpriseId");
            var factorylinkItemPopupData = CreateLinkableObjectsPopupConfig("GetLinkableResources", "LinkResourcesToParent", linkPopupResourceAreas);
            var factoryCustomData = CreateCollectionDefinitionCustomData(5610, 1250, "Factory", factoryCDOTitle, factoryForm, factoryGetOperation, null, factorylinkItemPopupData);
            var factory = CreateCollectionDefinition(factoryTreeNodeTitle, "tree-factory", "", "", factoryDataType, _areaCollectionKey, factoryCustomData);
            definitions.Add(_factoryCollectionKey, factory);

            // Shared Resource Settings
            var resourceGetOperation = CreateCustomDataGetOperation("resourceId", "GetResource", "resource");
            var importGetOperation = CreateImportCollection();

            // Create Area Config
            var areaDataType = CreateDataType("Area", areaTreeNodeTitle, "FactoryId", "FactoryLevel");
            var areaLinkItemPopupConfig = CreateLinkableObjectsPopupConfig("GetLinkableResources", "LinkResourcesToParent", linkPopupResourceCells);
            var areaCustomData = CreateCollectionDefinitionCustomData(3970, 3970, "Resource", resourceCDOTitle, resourceForm, resourceGetOperation, importGetOperation, areaLinkItemPopupConfig);
            var area = CreateCollectionDefinition(areaTreeNodeTitle, "tree-area", "", "", areaDataType, _cellCollectionKey, areaCustomData);
            definitions.Add(_areaCollectionKey, area);

            // Create Cell Config
            var cellDataType = CreateDataType("Cell", cellTreeNodeTitle, "ParentResourceId", "FactoryLevel");
            var cellLinkItemPopupConfig = CreateLinkableObjectsPopupConfig("GetLinkableResources", "LinkResourcesToParent", linkPopupResourceEquipment);
            var cellCustomData = CreateCollectionDefinitionCustomData(3970, 3970, "Resource", resourceCDOTitle, resourceForm, resourceGetOperation, importGetOperation, cellLinkItemPopupConfig);
            var cell = CreateCollectionDefinition(cellTreeNodeTitle, "tree-cell", "", "", cellDataType, _equipmentCollectionKey, cellCustomData);
            definitions.Add(_cellCollectionKey, cell);

            // Create Equipment Config
            var equipmentDataType = CreateDataType("Equipment", equipmentTreeNodeTitle, "ParentResourceId", "FactoryLevel");
            var equipmentCustomData = CreateCollectionDefinitionCustomData(3970, 3970, "Resource", resourceCDOTitle, resourceForm, resourceGetOperation, importGetOperation);
            var equipment = CreateCollectionDefinition(equipmentTreeNodeTitle, "tree-equipment", "", "", equipmentDataType, "", equipmentCustomData);
            definitions.Add(_equipmentCollectionKey, equipment);

            OnGenerateCollectionDefinitionJsonDocument(definitions);

            string jsonDoc = JsonConvert.SerializeObject(definitions, Formatting.Indented);
            return jsonDoc;
        }

        /// <summary>
        /// Create the definition of a popup to 'link' objects to the FHM. That is resources, factories, etc that are not currently part of the 
        /// FHM. The popup definition will then be added to the customData section of the tree configuration.
        /// </summary>
        /// <param name="getItemsServiceName">The name of the service for retrieving a list in unlinked items from</param>
        /// <param name="addItemsServiceName">The name of the service for adding a list in unlinked items to the FHM</param>
        /// <param name="popupTitle">The title to use for the popup used to select items</param>
        /// <returns>A new dynamic object containing properties for the popup</returns>
        protected dynamic CreateLinkableObjectsPopupConfig(string getItemsServiceName, string addItemsServiceName, string popupTitle)
        {
            dynamic popupConfig = new ExpandoObject();
            popupConfig.getOperationName = getItemsServiceName;
            popupConfig.addOperationName = addItemsServiceName;
            popupConfig.getResultsPropertyName = $"{getItemsServiceName}Result";
            popupConfig.popupTitle = popupTitle;

            // Add and empty array for defining tabs. If we have a single child object this property will not be used. 
            // Otherwise it can be populated later on.
            popupConfig.tabDefinition = new List<dynamic>();

            return popupConfig;
        }

        /// <summary>
        /// Popups for linking child objects usually only reference one cdo. However if the popup needs more than one object
        /// then we define when with this method. The tab will be added to the popup config's tabDefinition field. The
        /// </summary>
        /// <param name="serviceInfo"></param>
        /// <param name="title"></param>
        /// <param name="collectionKey"></param>
        protected void AddTabToLinkablePopupConfig(dynamic serviceInfo, string title, string collectionKey)
        {
            var tabConfig = serviceInfo.customData.LinkItemPopupConfiguration.tabDefinition;
            dynamic popupTab = new ExpandoObject();
            popupTab.title = title;
            popupTab.collectionKey = collectionKey;

            tabConfig.Add(popupTab);
        }

        protected virtual dynamic CreateImportCollection()
        {
            dynamic operation = new ExpandoObject();
            operation.paramName = "area";
            operation.operationName = "ImportAreaResources";
            operation.responseFieldName = "ImportAreaResourcesResult";

            return operation;
        }

        protected dynamic CreateDataType(string dataTypeName, string dataTypeTitle, string parentIdFieldMapping, string dataTypeFieldMapping = null)
        {
            dynamic dataTypeDefinition = new ExpandoObject();
            dataTypeDefinition.name = dataTypeName;
            dataTypeDefinition.title = dataTypeTitle;

            dataTypeDefinition.fieldMappings = new ExpandoObject();
            dataTypeDefinition.fieldMappings.id = string.Empty;
            dataTypeDefinition.fieldMappings.name = string.Empty;
            dataTypeDefinition.fieldMappings.parentIdForUpdate = parentIdFieldMapping;
            dataTypeDefinition.fieldMappings.dataTypeForUpdate = dataTypeFieldMapping;

            return dataTypeDefinition;
        }

        protected dynamic CreateCollectionDefinition(string iconTitle, string iconCSSClass, string titleFormat, string userPermissions, dynamic dataTypeDefinition,
            string childCollectionKey, dynamic customData)
        {
            List<string> childCollectionKeys = new List<string>();

            if (!string.IsNullOrEmpty(childCollectionKey))
                childCollectionKeys.Add(childCollectionKey);

            return CreateCollectionDefinition(iconTitle, iconCSSClass, titleFormat, userPermissions, dataTypeDefinition, childCollectionKeys, customData);
        }

        protected dynamic CreateCollectionDefinition(string iconTitle, string iconCSSClass, string titleFormat, string userPermissions, dynamic dataTypeDefinition,
            List<string> childCollectionKeys, dynamic customData)
        {

            dynamic definitionItem = new ExpandoObject();
            definitionItem.iconTitle = iconTitle;
            definitionItem.iconCSSClass = iconCSSClass;
            definitionItem.titleFormat = titleFormat;
            definitionItem.permissions = userPermissions;
            definitionItem.childCollections = childCollectionKeys;
            definitionItem.customData = customData;
            definitionItem.dataType = dataTypeDefinition;
            return definitionItem;
        }

        protected dynamic CreateCustomDataGetOperation(string paramName, string operationName, string responseFieldName)
        {
            dynamic operation = new ExpandoObject();
            operation.paramName = paramName;
            operation.operationName = operationName;
            operation.responseFieldName = responseFieldName;

            return operation;
        }

        protected dynamic CreateCollectionDefinitionCustomData(int maintTypeID, int cdoDefID, string cdoName, string cdoTitle, string modelingPage,
           dynamic getOperation, dynamic import = null, dynamic linkItemPopupData = null)
        {

            dynamic customData = new ExpandoObject();
            customData.CDOName = cdoName;
            customData.CDOTitle = cdoTitle;
            customData.CDODefID = cdoDefID;
            customData.ServiceName = $"{cdoName}Maint";
            customData.MaintTypeID = maintTypeID;
            customData.LinkItemPopupConfiguration = linkItemPopupData;
            customData.ModelingPage = modelingPage;
            customData.GetOperation = getOperation;
            if (import != null)
                customData.Import = import;

            return customData;
        }

        protected virtual void OnConfigureRequestObject(FactoryHierarchyModelInquiry_Request request) { }

        /// <summary>
        /// Generate the JSON document containing the Factory Hierarchical Model's data. This method calls the 
        /// WCF service, then calls functions to create the serialzed data
        /// </summary>
        /// <returns>The FHM data document in strng format</returns>
        private string GenerateJSONDataDocument()
        {
            string jsonDocument = string.Empty;

            // Call the web service and get the data
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);

            FactoryHierarchyModelInquiryService svc = new FactoryHierarchyModelInquiryService(session.CurrentUserProfile);
            FactoryHierarchyModelInquiry_Result result = new FactoryHierarchyModelInquiry_Result();
            FactoryHierarchyModelInquiry_Request request = new FactoryHierarchyModelInquiry_Request();
            FactoryHierarchyModelInquiry inquiry = new FactoryHierarchyModelInquiry();

            request.Info = new FactoryHierarchyModelInquiry_Info();
            request.Info.RequestValue = true;
            request.Info.AllFHMEnterprises = new Enterprise_Info()
            {
                //RequestValue = true,
                Name = new Info(true),
                FHMResolvedFactories = new Factory_Info()
                {
                    //RequestValue = true,
                    Name = new Info(true),
                    FactoryLevelIndex = new Info(true),
                    FHMResolvedAreaResources = new Resource_Info()
                    {
                        //RequestValue = true,
                        FactoryLevel = new Info(true),
                        FactoryLevelIndex = new Info(true),
                        Name = new Info(true),
                        NickName = new Info(true),
                        FHMResolvedChildResources = new Resource_Info()
                        {
                            //RequestValue = true,
                            FactoryLevel = new Info(true),
                            FactoryLevelIndex = new Info(true),
                            Name = new Info(true),
                            NickName = new Info(true),
                            FHMResolvedChildResources = new Resource_Info()
                            {
                                //RequestValue = true,
                                FactoryLevel = new Info(true),
                                FactoryLevelIndex = new Info(true),
                                Name = new Info(true),
                                NickName = new Info(true)
                            }
                        }
                    }
                }

            };

            OnConfigureRequestObject(request);

            ResultStatus resultsStatus = svc.ExecuteTransaction(request, out result);

            if (result.Value != null)
            {
                FactoryHierarchyModelInquiry fhmResults = result.Value;
                jsonDocument = CreateJSONFromWCFResults(fhmResults);
            }
            else if (!resultsStatus.IsSuccess)
            {
                string message = resultsStatus.Message;
                if (string.IsNullOrEmpty(message) && resultsStatus.ExceptionData != null)
                    message = resultsStatus.ExceptionData.Description;

                if (string.IsNullOrEmpty(message))
                    message = GetLocalizedLabel("StatusMessage_ServerError", "An error has occurred. Please contact your system administrator.");

                Page.DisplayMessage(message, false);
            }
            else
            {
                string message = GetLocalizedLabel("StatusMessage_ServerError", "An error has occurred. Please contact your system administrator.");
                Page.DisplayMessage(message, false);
            }

            return jsonDocument;
        }

        private dynamic CreateTreeDataStructure(FactoryHierarchyModelInquiry enterpriseInquiryResults)
        {
            dynamic nodeData = null;

            nodeData = new ExpandoObject();
            nodeData.Enterprises = GetEnterprises(enterpriseInquiryResults);

            return nodeData;
        }

        /// <summary>
        /// After the data from WCF has been used to populate data classes, this function will serialize the data into
        /// JSON format
        /// </summary>
        /// <param name="enterpriseInquiryResults">The results data from the WCF service call</param>
        /// <returns>The string value of the JSON data from serialization</returns>
        private string CreateJSONFromWCFResults(FactoryHierarchyModelInquiry enterpriseInquiryResults)
        {
            // Create a root element and then populate it with children
            dynamic tree = CreateTreeDataStructure(enterpriseInquiryResults);

            // Serialize the data for the tree
            string json = JsonConvert.SerializeObject(tree, Formatting.Indented, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            return json;
        }

        protected virtual void OnNewEnterprise(Enterprise enterpriseInquiryResults, dynamic enterprise) { }

        /// <summary>
        /// Fill a data structure with Enterprise data to be serialized to JSON and sent to the client.
        /// </summary>
        /// <param name="enterpriseInquiryResults">Data from the WCF call</param>
        /// <returns>An list Enterprise data for serialization</returns>
        List<dynamic> GetEnterprises(FactoryHierarchyModelInquiry enterpriseInquiryResults)
        {
            List<dynamic> enterprises = new List<dynamic>();


            if (enterpriseInquiryResults.AllFHMEnterprises != null)
            {
                foreach (Enterprise enterprise in enterpriseInquiryResults.AllFHMEnterprises)
                {
                    dynamic nodeData = new ExpandoObject();
                    if (enterprise.Name != null)
                        nodeData.Name = enterprise.Name.ToString();
                    if (enterprise.Self != null)
                    {
                        if (enterprise.Name == null)
                            nodeData.Name = enterprise.Self.ToString();
                        nodeData.ID = enterprise.Self.ID;
                    }

                    if (enterprise.FHMResolvedFactories != null && enterprise.FHMResolvedFactories.Length > 0)
                    {
                        nodeData.Factories = GetFactories(enterprise.FHMResolvedFactories);
                    }

                    OnNewEnterprise(enterprise, nodeData);

                    enterprises.Add(nodeData);
                }
            }

            return enterprises;
        }

        protected virtual void OnNewFactory(Factory[] factoryInquiryResults, dynamic factory) { }

        static protected int CompareItems(Primitive<int> level1, Primitive<int> level2, string name1, string name2)
        {
            if (level1 == null && level2 == null)
                return 0;
            else if (level1 == null)
                return -1;
            else if (level2 == null)
                return 1;
            else if (level1.Value == level2.Value)
                return name1.CompareTo(name2);
            else
                return level1.Value.CompareTo(level2.Value);
        }

        static protected int CompareName(Primitive<string> name1, Primitive<string> name2)
        {
            if (name1 == null)
                return -1;
            else if (name2 == null)
                return 1;
            else
                return name1.Value.CompareTo(name2.Value);
        }

        static protected int CompareLevel(Primitive<int> x, Primitive<int> y)
        {
            if (x == null && y == null)
                return 0;
            else if (x == null)
                return -1;
            else if (y == null)
                return 1;
            else
                return x.Value.CompareTo(y.Value);
        }
        /// <summary>
        /// Fill a data structure with Factory data to be serialized to JSON and sent to the client.
        /// </summary>
        /// <param name="enterpriseInquiryResults">Data from the WCF call</param>
        /// <returns>A list of Factory data for serialization</returns>
        List<dynamic> GetFactories(Factory[] factoryInquiryResults)
        {
            List<dynamic> factories = new List<dynamic>();

            List<Factory> all = factoryInquiryResults.ToList();
            List<Factory> noLevels = all.FindAll(s => s.FactoryLevelIndex == null || s.FactoryLevelIndex.IsEmpty);
            noLevels.Sort((x, y) => CompareName(x.Name, y.Name));
            List<Factory> levels = all.FindAll(s => s.FactoryLevelIndex != null && !s.FactoryLevelIndex.IsEmpty);
            levels.Sort((x, y) => CompareLevel(x.FactoryLevelIndex, y.FactoryLevelIndex));
            List<Factory> sorted = new List<Factory>(levels);
            sorted.AddRange(noLevels);
            foreach (Factory factory in sorted)
            {
                if (!factory.IsEmpty)
                {
                    dynamic nodeData = new ExpandoObject();
                    if (factory.Name != null)
                        nodeData.Name = factory.Name.ToString();
                    if (factory.Self != null)
                    {
                        if (factory.Name == null)
                            nodeData.Name = factory.Self.ToString();
                        nodeData.ID = factory.Self.ID;
                    }
                    if (factory.FHMResolvedAreaResources != null && factory.FHMResolvedAreaResources.Length > 0)
                    {
                        nodeData.Areas = GetAreas(factory.FHMResolvedAreaResources);
                    }

                    OnNewFactory(factoryInquiryResults, nodeData);

                    factories.Add(nodeData);
                }
            }

            return factories;
        }

        protected virtual void OnNewAreaResource(Resource resource, dynamic area) { }

        /// <summary>
        /// Fill a data structure with Area resource data to be serialized to JSON and sent to the client.
        /// </summary>
        /// <param name="enterpriseInquiryResults">Data from the WCF call</param>
        /// <returns>A list of Area data for serialization</returns>
        List<dynamic> GetAreas(Resource[] areaInquiryResults)
        {
            List<dynamic> areas = new List<dynamic>();

            List<Resource> all = areaInquiryResults.ToList();
            List<Resource> noLevels = all.FindAll(s => s != null && s.Name != null && (s.FactoryLevelIndex == null || s.FactoryLevelIndex.IsEmpty));
            noLevels.Sort((x, y) => CompareName(x.Name, y.Name));
            List<Resource> levels = all.FindAll(s => s != null && s.Name != null && s.FactoryLevelIndex != null && !s.FactoryLevelIndex.IsEmpty);
            levels.Sort((x, y) => CompareLevel(x.FactoryLevelIndex, y.FactoryLevelIndex));
            List<Resource> sorted = new List<Resource>(levels);
            sorted.AddRange(noLevels);

            foreach (Resource resource in sorted)
            {
                dynamic nodeData = CreateResourceDataObject(resource);
                if (nodeData != null)
                {
                    if (resource.FHMResolvedChildResources != null && resource.FHMResolvedChildResources.Length > 0)
                    {
                        nodeData.Cells = GetCells(resource.FHMResolvedChildResources);
                    }

                    OnNewAreaResource(resource, nodeData);
                    areas.Add(nodeData);
                }
            }

            return areas;
        }

        protected virtual void OnNewCellResource(Resource cellInquiryResults, dynamic cell) { }
        protected virtual void OnNewCellResourceWS40(Resource cellInquiryResults, dynamic cell) { }

        /// <summary>
        /// Fill a data structure with Cell resource data to be serialized to JSON and sent to the client.
        /// </summary>
        /// <param name="enterpriseInquiryResults">Data from the WCF call</param>
        /// <returns>A list of Cell data  for serialization</returns>
        List<dynamic> GetCells(Resource[] cellInquiryResults)
        {
            List<dynamic> cells = new List<dynamic>();

            List<Resource> all = cellInquiryResults.ToList();
            List<Resource> noLevels = all.FindAll(s => s != null && s.Name != null && (s.FactoryLevelIndex == null || s.FactoryLevelIndex.IsEmpty));
            noLevels.Sort((x, y) => CompareName(x.Name, y.Name));
            List<Resource> levels = all.FindAll(s => s != null && s.Name != null && s.FactoryLevelIndex != null && !s.FactoryLevelIndex.IsEmpty);
            levels.Sort((x, y) => CompareLevel(x.FactoryLevelIndex, y.FactoryLevelIndex));
            List<Resource> sorted = new List<Resource>(levels);
            sorted.AddRange(noLevels);
            foreach (Resource resource in sorted)
            {
                dynamic nodeData = CreateResourceDataObject(resource);
                if (nodeData != null)
                {
                    if (resource.FHMResolvedChildResources != null && resource.FHMResolvedChildResources.Length > 0)
                    {
                        nodeData.Equipment = GetEquipment(resource.FHMResolvedChildResources);
                    }

                    OnNewCellResource(resource, nodeData);
                    OnNewCellResourceWS40(resource, nodeData);
                    cells.Add(nodeData);
                }
            }

            return cells;
        }

        protected virtual void OnNewEquipmentResource(Resource equipmentInquiryResults, dynamic equipment) { }

        /// <summary>getfac
        /// Fill a data structure with Equipment resource data to be serialized to JSON and sent to the client.
        /// </summary>
        /// <param name="enterpriseInquiryResults">Data from the WCF call</param>
        /// <returns>A list of Equipment data for serialization</returns>
        protected List<dynamic> GetEquipment(Resource[] equipmentInquiryResults)
        {
            List<dynamic> equipment = new List<dynamic>();

            List<Resource> all = equipmentInquiryResults.ToList();
            List<Resource> noLevels = all.FindAll(s => s != null && s.Name != null && (s.FactoryLevelIndex == null || s.FactoryLevelIndex.IsEmpty));
            noLevels.Sort((x, y) => CompareName(x.Name, y.Name));
            List<Resource> levels = all.FindAll(s => s != null && s.Name != null && s.FactoryLevelIndex != null && !s.FactoryLevelIndex.IsEmpty);
            levels.Sort((x, y) => CompareLevel(x.FactoryLevelIndex, y.FactoryLevelIndex));
            List<Resource> sorted = new List<Resource>(levels);
            sorted.AddRange(noLevels);
            foreach (Resource resource in sorted)
            {
                dynamic nodeData = CreateResourceDataObject(resource);
                if (nodeData != null)
                {
                    if (resource.FHMResolvedChildResources != null && resource.FHMResolvedChildResources.Length > 0)
                    {
                        nodeData.Equipment = GetEquipment(resource.FHMResolvedChildResources);
                    }

                    OnNewEquipmentResource(resource, nodeData);

                    equipment.Add(nodeData);
                }
            }

            return equipment;
        }

        dynamic CreateResourceDataObject(Resource resource)
        {
            dynamic nodeData = new ExpandoObject();
            if (resource.Name != null || resource.Self != null)
            {
                if (resource.Name != null)
                    nodeData.Name = resource.Name.ToString();
                else if (resource.Self != null)
                    nodeData.Name = resource.Self.ToString();
                nodeData.ID = resource.Self != null ? resource.Self.ID : "";
                nodeData.Nickname = resource.NickName != null ? resource.NickName.ToString() : "";
                nodeData.FactoryLevel = resource.FactoryLevel != null ? resource.FactoryLevel.Value : 0;
                nodeData.FactoryLevelIndex = resource.FactoryLevelIndex != null ? resource.FactoryLevelIndex.Value : 0;

                return nodeData;
            }
            else
                return null;
        }

        protected virtual void OnGetLabels(LabelList labelsList) { }

        /// <summary>
        /// Get the labels to use for localization
        /// </summary>
        void GetLabels()
        {
            LabelList labels = new LabelList(new List<Label>() {
                new Label("CSICDOName_Enterprise"),
                new Label("CSICDOName_Factory"),
                new Label("FactoryLevelEnum_Area"),
                new Label("FactoryLevelEnum_Cell"),
                new Label("FactoryLevelEnum_Equipment"),
                new Label("StatusMessage_ServerError"),
                new Label("FHMNotDefined"),
                new Label("CSICDOName_Resource"),
                new Label("LblMenuSearch")
            });

            OnGetLabels(labels);

            // load them to cache
            CachedLabels = FrameworkManagerUtil.GetLabelCache(Page.Session);
        }
        #endregion


    }

}