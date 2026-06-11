// Copyright Siemens 2024
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework.WebControls;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.FormsFramework;
using System.Web.Script.Serialization;
using System.Text;
using System.Net;
using Newtonsoft.Json;
using System.Drawing;
using Camstar.WebPortal.PortalFramework;

/// <summary>
/// Displays items to import and lets the user configure the columns
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Modeling
{
    public class BomItemImportPopup : MatrixWebPart
    {
        protected CWC.TextBox DefaultQuantity { get { return Page.FindCamstarControl("DefaultQuantity") as CWC.TextBox; } }
        protected CWC.TextBox DefaultProductRevision { get { return Page.FindCamstarControl("DefaultProductRevision") as CWC.TextBox; } }
        protected CWC.TextBox DefaultBomRevision { get { return Page.FindCamstarControl("DefaultBomRevision") as CWC.TextBox; } }
        protected CWC.TextBox DefaultSpecRevision { get { return Page.FindCamstarControl("DefaultSpecRevision") as CWC.TextBox; } }
        protected CWC.TextBox BomImportConfigJson { get { return Page.FindCamstarControl("BomImportConfigJson") as CWC.TextBox; } }
        protected CWC.TextBox BomItemsJson { get { return Page.FindCamstarControl("BomItemsJson") as CWC.TextBox; } }
        protected CWC.DropDownList DefaultIssueControl { get { return Page.FindCamstarControl("DefaultIssueControl") as CWC.DropDownList; } }
        protected CWC.CheckBox ImportMultipleBoms { get { return Page.FindCamstarControl("ImportMultipleBoms") as CWC.CheckBox; } }

        protected BomImportConfig BomImportConfig = new BomImportConfig();

        protected override void OnLoad(EventArgs e)
        {
            // load config
            BomImportConfig = BomImportConfig.LoadConfig(Page);

            // apply config 
            DefaultQuantity.Data = BomImportConfig.DefaultQuantity;
            DefaultIssueControl.Data = BomImportConfig.DefaultIssueControl.ToString();
            DefaultProductRevision.Data = BomImportConfig.DefaultProductRevision;
            DefaultBomRevision.Data = BomImportConfig.DefaultBomRevision;
            DefaultSpecRevision.Data = BomImportConfig.DefaultSpecRevision;
            ImportMultipleBoms.CheckControl.Checked = BomImportConfig.ImportMultipleBoms;

            base.OnLoad(e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);

            var action = e.Action as Personalization.CustomAction;
            if (action != null)
            {
                switch (action.Parameters)
                {
                    case "Import":
                    {
                        // save config
                        BomImportConfig.SaveConfig(Page, BomImportConfigJson.Data.ToString());
                        Page.CloseFloatingFrameOnSubmit(new ResultStatus());
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (!Page.IsPostBack)
            {
                string initScript = $"BomItemImportPopup.initialize('{JsonConvert.SerializeObject(BomImportConfig.ColumnMaps, Formatting.None)}', '{JsonConvert.SerializeObject(BomImportConfig.AdditionalColumnOptions, Formatting.None)}', '{JsonConvert.SerializeObject(BomImportConfig.EnumMaps, Formatting.None)}');";
                ScriptManager.RegisterStartupScript(this, GetType(), "BomItemImportPopupInitialize", initScript, true);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override IEnumerable<ScriptReference> GetScriptReferences()
        {
            yield return new ScriptReference("~/Scripts/CRModules.js");
            yield return new ScriptReference("~/Scripts/BomItemImportPopup.js");
        }
    }

    /// <summary>
    /// Column at a given index maps to what value?
    /// </summary>
    public class ColumnMap
    {
        [JsonProperty("columnId")]
        public string ColumnId { get; set; }
        [JsonProperty("index")]
        public int Index { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ColumnOption
    {
        [JsonProperty("columnId")]
        public string Value { get; set; }
        [JsonProperty("text")]
        public string Text { get; set; }
        [JsonProperty("labelName")]
        public string LabelName { get; set; }
    }

    /// <summary>
    /// How an individual value imported from a file maps to the Opcenter enum integer value
    /// </summary>
    public partial class ValueMap
    {
        /// <summary>
        /// Imported from file
        /// </summary>
        [JsonProperty("fromValue")]
        public string FromValue { get; set; }

        /// <summary>
        /// Opcenter Enum value
        /// </summary>
        [JsonProperty("toValue")]
        public int ToValue { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class EnumMap
    {
        [JsonProperty("columnId")]
        public string ColumnId { get; set; }

        [JsonProperty("valueMaps")]
        public List<ValueMap> ValueMaps { get; set; }

    }

    /// <summary>
    /// Read/written to file and used for defaults
    /// </summary>
    public class BomImportConfig
    {
        [JsonProperty("defaultQuantity")]
        public int DefaultQuantity { get; set; } = 1;

        [JsonProperty("defaultIssueControl")]
        public int DefaultIssueControl { get; set; } = 3;

        [JsonProperty("defaultProductRevision")]
        public string DefaultProductRevision { get; set; } = "1";

        [JsonProperty("defaultBomRevision")]
        public string DefaultBomRevision { get; set; } = "1";

        [JsonProperty("defaultSpecRevision")]
        public string DefaultSpecRevision { get; set; } = "1";

        [JsonProperty("columnMaps")]
        public List<ColumnMap> ColumnMaps { get; set; }

        [JsonProperty("enumMaps")]
        public List<EnumMap> EnumMaps { get; set; } = new List<EnumMap>();

        [JsonProperty("additionalColumnOptions")]
        public List<ColumnOption> AdditionalColumnOptions { get; set; } = new List<ColumnOption>();

        [JsonProperty("importMultipleBoms")]
        public bool ImportMultipleBoms { get; set; } = false;

        /// <summary>
        /// Load BOM import config from file
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public static BomImportConfig LoadConfig(WebPartPageBase page)
        {
            BomImportConfig config = new BomImportConfig();

            if (File.Exists(page.Server.MapPath(@"~/Config/BomImportConfig.json")))
            {
                var fileContents = File.ReadAllText(page.Server.MapPath(@"~/Config/BomImportConfig.json"));
                config = JsonConvert.DeserializeObject<BomImportConfig>(fileContents);
            }

            return config;
        }

        /// <summary>
        /// Write BOM import config to file
        /// </summary>
        /// <param name="page"></param>
        /// <param name="config"></param>
        public static void SaveConfig(WebPartPageBase page, string config)
        {
            BomImportConfig configObj = JsonConvert.DeserializeObject<BomImportConfig>(config);
            string formattedConfig = JsonConvert.SerializeObject(configObj, Formatting.Indented);
            File.WriteAllText(page.Server.MapPath(@"~/Config/BomImportConfig.json"), formattedConfig);
        }
    }

}