// Copyright Siemens 2024  
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Dynamic;

namespace Camstar.WebPortal.WebPortlets.Modeling
{
    /// <summary>
    /// Code behind for the Factory Hierarchy Model (FHM) page for Semiconductor. This class will get the data from the WCF call to the FactoryHierarchyModelInquiry 
    /// Service, and serialize it into JSON. This class will also define the structure and metadata of the tree from here in a JSON document.
    /// That data will be be sent to the client to be processed by Javascript.
    /// 
    /// KEY CONCEPTS:
    /// Tree Definition Data Document - The tree definition data defines the structure, and metadata about elements in the tree. 
    /// The reason why this is done on the server instead of hard coded in the Javascript is so other workspaces can inherit
    /// this class. Using inheritence this child class cna and then alter and expand on the tree as needed in an OOP way.
    /// So the structure will be defined on the server and then sent to the serer as a JSON document
    /// </summary>
    public class scsFactoryHierarchy : isFactoryHierarchy
    {
        #region Constants
        protected const string _assemblyEquipmentCollectionKey = "AssemblyEquipment";
        protected const string _waferEquipmentCollectionKey = "WaferEquipment";
        protected const string _waferSortEquipmentCollectionKey = "WaferSortEquipment";
        protected const string _backGrindEquipmentCollectionKey = "BackGrindEquipment";
        protected const string _testEquipmentCollectionKey = "TestEquipment";
        #endregion

        #region Properties
        #endregion

        #region Methods
        protected override void OnGenerateCollectionDefinitionJsonDocument(Dictionary<string, dynamic> definitions)
        {
            base.OnGenerateCollectionDefinitionJsonDocument(definitions);

            var resourceGetOperation = CreateCustomDataGetOperation("resourceId", "GetResource", "resource");

            //Assembly Equipment
            string assemblyEquipmentForm = GetFormName("AssemblyEquipment", "Resource_VP");
            string assemblyEquipmentNodeTitle = GetLocalizedLabel("CSICDOName_AssemblyEquipment", "Assembly Equipment");
            string assemblyEquipmentCDOTitle = assemblyEquipmentNodeTitle;
            string assemblyEquipmentTabTitle = GetLocalizedLabel("CSICDOName_AssemblyEquipment", "Assembly Equipment");

            // Create new collection definition objects and add them to the definition's dictionary
            var assemblyEquipmentGetOperation = CreateCustomDataGetOperation("assemblyEquipmentId", "GetAssemblyEquipment", "assemblyEquipment");
            var assemblyEquipmentDataType = CreateDataType("AssemblyEquipment", assemblyEquipmentNodeTitle, "ParentResourceId");
            var assemblyEquipmentCustomData = CreateCollectionDefinitionCustomData(4752449, 4752449, "AssemblyEquipment", assemblyEquipmentCDOTitle, assemblyEquipmentForm, resourceGetOperation, null);
            var assemblyEquipment = CreateCollectionDefinition(assemblyEquipmentNodeTitle, "tree-equipment", "", "", assemblyEquipmentDataType, "", assemblyEquipmentCustomData);

            definitions.Add(_assemblyEquipmentCollectionKey, assemblyEquipment);
            definitions[_cellCollectionKey].childCollections.Add(_assemblyEquipmentCollectionKey);

            //Back Grind Equipment
            string backGrindEquipmentForm = GetFormName("BackGrindEquipment", "Resource_VP");
            string backGrindEquipmentNodeTitle = GetLocalizedLabel("CSICDOName_BackGrindEquipment", "Back Grind Equipment");
            string backGrindEquipmentCDOTitle = backGrindEquipmentNodeTitle;
            string backGrindEquipmentTabTitle = GetLocalizedLabel("CSICDOName_BackGrindEquipment", "Back Grind Equipment");

            // Create new collection definition objects and add them to the definition's dictionary
            var backGrindEquipmentGetOperation = CreateCustomDataGetOperation("backGrindEquipmentId", "GetBackGrindEquipment", "backGrindEquipment");
            var backGrindEquipmentDataType = CreateDataType("backGrindEquipment", backGrindEquipmentNodeTitle, "ParentResourceId");
            var backGrindEquipmentCustomData = CreateCollectionDefinitionCustomData(4752451, 4752451, "BackGrindEquipment", backGrindEquipmentCDOTitle, backGrindEquipmentForm, resourceGetOperation, null);
            var backGrindEquipment = CreateCollectionDefinition(backGrindEquipmentNodeTitle, "tree-equipment", "", "", backGrindEquipmentDataType, "", backGrindEquipmentCustomData);

            definitions.Add(_backGrindEquipmentCollectionKey, backGrindEquipment);
            definitions[_cellCollectionKey].childCollections.Add(_backGrindEquipmentCollectionKey);

            //Test Equipment
            string testEquipmentForm = GetFormName("testEquipment", "Resource_VP");
            string testEquipmentNodeTitle = GetLocalizedLabel("CSICDOName_TestEquipment", "Test Equipment");
            string testEquipmentCDOTitle = testEquipmentNodeTitle;
            string testEquipmentTabTitle = GetLocalizedLabel("CSICDOName_TestEquipment", "Test Equipment");

            // Create new collection definition objects and add them to the definition's dictionary
            var testEquipmentGetOperation = CreateCustomDataGetOperation("testEquipmentId", "GetTestEquipment", "testEquipment");
            var testEquipmentDataType = CreateDataType("testEquipment", testEquipmentNodeTitle, "ParentResourceId");
            var testEquipmentCustomData = CreateCollectionDefinitionCustomData(4752448, 4752448, "TestEquipment", testEquipmentCDOTitle, testEquipmentForm, resourceGetOperation, null);
            var testEquipment = CreateCollectionDefinition(testEquipmentNodeTitle, "tree-equipment", "", "", testEquipmentDataType, "", testEquipmentCustomData);

            definitions.Add(_testEquipmentCollectionKey, testEquipment);
            definitions[_cellCollectionKey].childCollections.Add(_testEquipmentCollectionKey);

            //Wafer Equipment
            string waferEquipmentForm = GetFormName("WaferEquipment", "Resource_VP");
            string waferEquipmentNodeTitle = GetLocalizedLabel("CSICDOName_WaferEquipment", "Wafer Equipment");
            string waferEquipmentCDOTitle = waferEquipmentNodeTitle;
            string waferEquipmentTabTitle = GetLocalizedLabel("CSICDOName_WaferEquipment", "Wafer Equipment");

            // Create new collection definition objects and add them to the definition's dictionary
            var waferEquipmentGetOperation = CreateCustomDataGetOperation("waferEquipmentId", "GetWaferEquipment", "waferEquipment");
            var waferEquipmentDataType = CreateDataType("WaferEquipment", waferEquipmentNodeTitle, "ParentResourceId");
            var waferEquipmentCustomData = CreateCollectionDefinitionCustomData(4752452, 4752452, "WaferEquipment", waferEquipmentCDOTitle, waferEquipmentForm, resourceGetOperation, null);
            var waferEquipment = CreateCollectionDefinition(waferEquipmentNodeTitle, "tree-equipment", "", "", waferEquipmentDataType, "", waferEquipmentCustomData);

            definitions.Add(_waferEquipmentCollectionKey, waferEquipment);
            definitions[_cellCollectionKey].childCollections.Add(_waferEquipmentCollectionKey);

            //Wafer Sort Equipment
            string waferSortEquipmentForm = GetFormName("WaferSortEquipment", "Resource_VP");
            string waferSortEquipmentNodeTitle = GetLocalizedLabel("CSICDOName_WaferSortEquipment", "Wafer Sort Equipment");
            string waferSortEquipmentCDOTitle = waferSortEquipmentNodeTitle;
            string waferSortEquipmentTabTitle = GetLocalizedLabel("CSICDOName_WaferSortEquipment", "Wafer Sort Equipment");

            // Create new collection definition objects and add them to the definition's dictionary
            var waferSortEquipmentGetOperation = CreateCustomDataGetOperation("waferSortEquipmentId", "GetWaferSortEquipment", "waferSortEquipment");
            var waferSortEquipmentDataType = CreateDataType("waferSortEquipment", waferSortEquipmentNodeTitle, "ParentResourceId");
            var waferSortEquipmentCustomData = CreateCollectionDefinitionCustomData(4752450, 4752450, "WaferSortEquipment", waferSortEquipmentCDOTitle, waferSortEquipmentForm, resourceGetOperation, null);
            var waferSortEquipment = CreateCollectionDefinition(waferSortEquipmentNodeTitle, "tree-equipment", "", "", waferSortEquipmentDataType, "", waferSortEquipmentCustomData);

            definitions.Add(_waferSortEquipmentCollectionKey, waferSortEquipment);
            definitions[_cellCollectionKey].childCollections.Add(_waferSortEquipmentCollectionKey);

            definitions[_cellCollectionKey].customData.LinkItemPopupConfiguration.getOperationName = "GetLinkableEquipmentWS40";
            definitions[_cellCollectionKey].customData.LinkItemPopupConfiguration.addOperationName = "LinkEquipmentWS40ToCell";

            AddTabToLinkablePopupConfig(definitions[_cellCollectionKey], assemblyEquipmentTabTitle, _assemblyEquipmentCollectionKey);
            AddTabToLinkablePopupConfig(definitions[_cellCollectionKey], backGrindEquipmentTabTitle, _backGrindEquipmentCollectionKey);
            AddTabToLinkablePopupConfig(definitions[_cellCollectionKey], testEquipmentTabTitle, _testEquipmentCollectionKey);
            AddTabToLinkablePopupConfig(definitions[_cellCollectionKey], waferEquipmentTabTitle, _waferEquipmentCollectionKey);
            AddTabToLinkablePopupConfig(definitions[_cellCollectionKey], waferSortEquipmentTabTitle, _waferSortEquipmentCollectionKey);

        }

        protected override void OnGetLabels(LabelList labelsList)
        {
            base.OnGetLabels(labelsList);
            labelsList.Add(new Label("CSICDOName_AssemblyEquipment"));
            labelsList.Add(new Label("CSICDOName_TestEquipment"));
            labelsList.Add(new Label("CSICDOName_WaferEquipment"));
            labelsList.Add(new Label("CSICDOName_BackGrindEquipment"));
            labelsList.Add(new Label("CSICDOName_WaferSortEquipment"));
        }

        protected override void OnConfigureRequestObject(FactoryHierarchyModelInquiry_Request request)
        {
            base.OnConfigureRequestObject(request);
            request.Info.AllFHMEnterprises.FHMResolvedFactories.FHMResolvedAreaResources.
               FHMResolvedChildResources.scsFHMResolvedAssemblyEquipment = new AssemblyEquipment_Info()
               {
                   FactoryLevel = new Info(true),
                   FactoryLevelIndex = new Info(true),
                   Name = new Info(true),
                   NickName = new Info(true)
               };
            request.Info.AllFHMEnterprises.FHMResolvedFactories.FHMResolvedAreaResources.
               FHMResolvedChildResources.scsFHMResolvedBackGrindEquipment = new BackGrindEquipment_Info()
               {
                   FactoryLevel = new Info(true),
                   FactoryLevelIndex = new Info(true),
                   Name = new Info(true),
                   NickName = new Info(true)
               };
            request.Info.AllFHMEnterprises.FHMResolvedFactories.FHMResolvedAreaResources.
               FHMResolvedChildResources.scsFHMResolvedTestEquipment = new TestEquipment_Info()
               {
                   FactoryLevel = new Info(true),
                   FactoryLevelIndex = new Info(true),
                   Name = new Info(true),
                   NickName = new Info(true)
               };
            request.Info.AllFHMEnterprises.FHMResolvedFactories.FHMResolvedAreaResources.
               FHMResolvedChildResources.scsFHMResolvedWaferEquipment = new WaferEquipment_Info()
               {
                   FactoryLevel = new Info(true),
                   FactoryLevelIndex = new Info(true),
                   Name = new Info(true),
                   NickName = new Info(true)
               };
            request.Info.AllFHMEnterprises.FHMResolvedFactories.FHMResolvedAreaResources.
               FHMResolvedChildResources.scsFHMResolvedWaferSortEquipment = new WaferSortEquipment_Info()
               {
                   FactoryLevel = new Info(true),
                   FactoryLevelIndex = new Info(true),
                   Name = new Info(true),
                   NickName = new Info(true)
               };
        }

        protected override void OnNewCellResourceWS40(Resource resource, dynamic cell)
        {
            if (resource.scsFHMResolvedAssemblyEquipment != null && resource.scsFHMResolvedAssemblyEquipment.Length > 0)
            {
                cell.AssemblyEquipment = GetEquipment(resource.scsFHMResolvedAssemblyEquipment);
            }
            if (resource.scsFHMResolvedBackGrindEquipment != null && resource.scsFHMResolvedBackGrindEquipment.Length > 0)
            {
                cell.BackGrindEquipment = GetEquipment(resource.scsFHMResolvedBackGrindEquipment);
            }
            if (resource.scsFHMResolvedTestEquipment != null && resource.scsFHMResolvedTestEquipment.Length > 0)
            {
                cell.TestEquipment = GetEquipment(resource.scsFHMResolvedTestEquipment);
            }
            if (resource.scsFHMResolvedWaferEquipment != null && resource.scsFHMResolvedWaferEquipment.Length > 0)
            {
                cell.WaferEquipment = GetEquipment(resource.scsFHMResolvedWaferEquipment);
            }
            if (resource.scsFHMResolvedWaferSortEquipment != null && resource.scsFHMResolvedWaferSortEquipment.Length > 0)
            {
                cell.WaferSortEquipment = GetEquipment(resource.scsFHMResolvedWaferSortEquipment);
            }
        }
        #endregion
    }

}