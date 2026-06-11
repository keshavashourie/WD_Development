// Copyright Siemens 2021 
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework;
using System;
using System.Collections.Generic;
using System.Dynamic;

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
    public class isFactoryHierarchy : FactoryHierarchy
    {
        #region Constants
        protected const string _inventoryLocationCollectionKey = "InventoryLocations";

        #endregion

        #region Properties


        #endregion

        #region Methods
        protected override void OnGenerateCollectionDefinitionJsonDocument(Dictionary<string, dynamic> definitions)
        {
            string inventoryForm = GetFormName("isInventoryLocationMaint", "isInventoryLocation_VP");
            string inventoryLocationNodeTitle = GetLocalizedLabel("CSICDOName_isInventoryLocation", "Inventory Location");
            string inventoryLocationCDOTitle = inventoryLocationNodeTitle;
            string resourcesTabTitle = GetLocalizedLabel("MaintenanceClass_Resources", "Resources");
            string inventoryLocationsTabTitle = GetLocalizedLabel("Resource_isResolvedInventoryLocations", "Inventory Locations");
                                                                
            // Create new collection definition objects and add them to the definition's dictionary
            var inventoryLocationGetOperation = CreateCustomDataGetOperation("inventoryLocationId", "GetIsInventoryLocation", "inventoryLocation");
            var inventoryLocationDataType = CreateDataType("InventoryLocation", inventoryLocationNodeTitle, "ParentResourceId");
            var inventoryLocationCustomData = CreateCollectionDefinitionCustomData(4841508, 4841508, "isInventoryLocation", inventoryLocationCDOTitle, inventoryForm, inventoryLocationGetOperation, null);
            var inventoryLocation = CreateCollectionDefinition(inventoryLocationNodeTitle, "tree-inventoryLocation", "", "", inventoryLocationDataType, "", inventoryLocationCustomData);
 
            definitions.Add(_inventoryLocationCollectionKey, inventoryLocation);
            definitions[_cellCollectionKey].childCollections.Add(_inventoryLocationCollectionKey);

            definitions[_cellCollectionKey].customData.LinkItemPopupConfiguration.getOperationName = "GetLinkableIplEquipment";
            definitions[_cellCollectionKey].customData.LinkItemPopupConfiguration.addOperationName = "LinkIplEquipmentToCell";

            AddTabToLinkablePopupConfig(definitions[_cellCollectionKey], resourcesTabTitle, _equipmentCollectionKey);
            AddTabToLinkablePopupConfig(definitions[_cellCollectionKey], inventoryLocationsTabTitle, _inventoryLocationCollectionKey);            
        }

        protected override dynamic CreateImportCollection()
        {
            dynamic operation = new ExpandoObject();
            operation.paramName = "area";
            operation.operationName = "isImportAreaResources";
            operation.responseFieldName = "isImportAreaResourcesResult";

            return operation;
        }


        protected override void OnGetLabels(LabelList labelsList)
        {
            labelsList.Add(new Label("CSICDOName_isInventoryLocation"));
        }

        protected override void OnConfigureRequestObject(FactoryHierarchyModelInquiry_Request request)
        {
            request.Info.AllFHMEnterprises.FHMResolvedFactories.FHMResolvedAreaResources.
                FHMResolvedChildResources.isFHMResolvedInventoryLocations = new isInventoryLocation_Info()
                {
                    RequestValue = true,
                    Name = new Info(true)
                };
        }

        protected override void OnNewCellResource(Resource resource, dynamic cell)
        {
            if (resource.isFHMResolvedInventoryLocations != null && resource.isFHMResolvedInventoryLocations.Length > 0)
            {
                cell.InventoryLocations = GetInventoryLocations(resource.isFHMResolvedInventoryLocations);
            }
        }

        protected virtual void OnNewInventoryLocation(Camstar.WCF.ObjectStack.isInventoryLocation inventoryLocationInquiryResults, dynamic inventoryLocation) { }

        /// <summary>
        /// Fill a data structure with inventory location data to be serialized to JSON and sent to the client.
        /// </summary>
        /// <param name="enterpriseInquiryResults">Data from the WCF call</param>
        /// <returns>An list inventory location data for serialization</returns>
        protected List<dynamic> GetInventoryLocations(Camstar.WCF.ObjectStack.isInventoryLocation[] locationInquiryResults)
        {
            List<dynamic> locations = new List<dynamic>();

            foreach (var inventoryLocation in locationInquiryResults)
            {
                dynamic nodeData = new ExpandoObject();
                nodeData.Name = inventoryLocation.Name.ToString();
                nodeData.ID = inventoryLocation.Self.ID;

                OnNewInventoryLocation(inventoryLocation, nodeData);

                locations.Add(nodeData);
            }

            return locations;
        }

        #endregion
    }

}