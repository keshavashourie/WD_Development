// Copyright Siemens 2023
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;

namespace Camstar.WebPortal.WebPortlets.Modeling
{
    /// <summary>
    /// Code behind for the Factory Hierarchy Model (FHM) Import page. 
    /// TreeData and TreeDefnition will be passed from FH Page to this popup page through Data Contract
    /// </summary>
    public class FactoryHierarchyImport : MatrixWebPart
    {
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

        /// <summary>
        /// Either Valor or SRC
        /// </summary>
        protected CWC.TextBox ImportType
        {
            get { return Page.FindCamstarControl("importType") as CWC.TextBox; }
        }

        #endregion

        #region Methods
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            Dictionary<string, string> clientLabels = GetClientLabels();
            string labelsJson = JsonConvert.SerializeObject(clientLabels, Formatting.Indented);

            // default to original Valor import
            string initScript = $"factoryHierarchyImport.initialize({labelsJson})";
            if (ImportType.Data.ToString() == "SRC")
                initScript = $"srcImport.initialize({labelsJson})";

            ScriptManager.RegisterStartupScript(
                this, 
                this.GetType(), 
                "initializeFactoryHierarchy",
                initScript, 
                true);
        }

        protected Dictionary<string, string> GetClientLabels()
        {
            var labels = new Dictionary<string, string>()
            {
                { "ObjectAlreadyExists", GetLocalizedLabel("ImportContent_ObjectAlreadyExists", "Object Already Exists") },
                { "NoImportContent", GetLocalizedLabel("ExpImpNoImportContents", "Unable to find import content record(s).") },
                { "Error", GetLocalizedLabel("Lbl_Error", "Error") },
                { "Errors", GetLocalizedLabel("NAVIGATOR_APP_PREF_TAB_ERRORS", "Errors") },
                { "IsCompletionImportSRC", GetLocalizedLabel("IsCompletionImportSRC", "Import SRC settings completed successfully.") }
            };

            return labels;
        }

        protected override IEnumerable<ScriptReference> GetScriptReferences()
        {
            yield return new ScriptReference("~/Scripts/CRModules.js");
            yield return new ScriptReference("~/Scripts/TreeConfiguration.js");
            yield return new ScriptReference("~/Scripts/TreeCollectionDefinition.js");
            yield return new ScriptReference("~/Scripts/TreeDataTypeDefinition.js");
            yield return new ScriptReference("~/Scripts/TreeDataProvider.js");
            yield return new ScriptReference("~/Scripts/TreeControl.js");
            yield return new ScriptReference("~/Scripts/FactoryHierarchyImport.js");
            yield return new ScriptReference("~/Scripts/SrcImport.js");
        }

        protected string GetLocalizedLabel(string labelId, string alternateText)
        {
            string text = CachedLabels != null ? CachedLabels.GetLabelByName(labelId).Value : string.Empty;

            if (string.IsNullOrEmpty(text))
                text = alternateText;

            return text;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            GetLabels();

            if (!Page.IsPostBack)
            {
                if (ImportType.Data.ToString() == "Valor")
                {
                    if (TreeData.Data != null)
                        TreeData.Data = ModifyImportData(TreeData.Data.ToString());
                }
            }

        }

        protected List<string> GetAllResourceName()
        {
            var svcParams = new Camstar.WCF.ObjectStack.ResourceMaint();

            var request = new ResourceMaint_Request()
            {
                Info = new ResourceMaint_Info
                {
                    ObjectListInquiry = new Info(false, true)
                }
            };

            var result = new ResourceMaint_Result();
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = new ResourceMaintService(session.CurrentUserProfile);
            ResultStatus resultStatus = service.GetEnvironment(svcParams, request, out result);

            if (resultStatus.IsSuccess && result.Environment.ObjectListInquiry.SelectionValues.Rows != null)
                return result.Environment.ObjectListInquiry.SelectionValues.Rows.Select(s => s.Values[0]).ToList();
            else
                return new List<string>();
        }

        /// <summary>
        /// Modify Tree Data by adding isCheck and checkboxDisable property
        /// [isCheck] - Flag to indicate the creation of checkbox in tree node and its status when created 
        ///             (TRUE-Checkbox checked, FALSE-Checkbox unchecked oncreate)
        /// [checkboxDisabled] - Flag to indicate whether the checkbox is enabled/disabled
        /// </summary>
        /// <returns>A string representation of the import data in JSON format</returns>
        protected virtual string ModifyImportData(string importData)
        {
            List<string> allResourceName = GetAllResourceName();
            dynamic importTreeData = JsonConvert.DeserializeObject<dynamic>(importData);

            // If import data already exist, checkbox is disabled
            dynamic selectedArea = importTreeData.Areas[0];
            if (!selectedArea.ContainsKey("Cells"))
                return importData;

            foreach (dynamic cell in selectedArea.Cells)
            {
                bool isExist = allResourceName.Contains(cell.Name.Value);

                cell.checkboxDisable = isExist;
                cell.isChecked = true;

                if (cell.ContainsKey("Equipment"))
                {
                    foreach (dynamic equipment in cell.Equipment)
                    {
                        isExist = allResourceName.Contains(equipment.Name.Value);
                        equipment.checkboxDisable = isExist;
                        equipment.isChecked = true;
                    }
                }
            }

            return JsonConvert.SerializeObject(importTreeData);
        }

        protected virtual void OnGetLabels(LabelList labelsList) { }

        /// <summary>
        /// Get the labels to use for localization
        /// </summary>
        void GetLabels()
        {
            LabelList labels = new LabelList(new List<Label>() {
                new Label("ImportContent_ObjectAlreadyExists"),
                new Label("ExpImpNoImportContents")
            });

            OnGetLabels(labels);

            // load them to cache
            CachedLabels = FrameworkManagerUtil.GetLabelCache(Page.Session);
        }
        #endregion
    }

}
