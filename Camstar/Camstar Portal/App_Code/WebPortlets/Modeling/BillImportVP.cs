// Copyright Siemens 2023 
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.UI;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Newtonsoft.Json;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework.Utilities;
using Siemens.OPCR.Diagnostics;
using System.Web;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.Personalization;

namespace Camstar.WebPortal.WebPortlets.Modeling
{
    /// <summary>
    /// Any page that imports bills of material (BOM Maint, Mfg Order Maint, etc...)
    /// Expanded to include functionality to UPDATE bills of material as well
    /// </summary>
    /// <typeparam name="TItemChanges">Item grid contains changes of this type</typeparam>
    public class BillImportVP<TItemChanges> : MatrixWebPart
        where TItemChanges : MaterialListItemChanges, new()
    {
        #region Controls
        protected CWC.TextBox BomName { get { return Page.FindCamstarControl("NameTxt") as CWC.TextBox; } }
        protected CWC.TextBox BomRevision { get { return Page.FindCamstarControl("RevisionTxt") as CWC.TextBox; } }
        protected CWC.Button UpdateBomItems { get { return Page.FindCamstarControl("UpdateBomItems") as CWC.Button; } }
        protected CWC.TextBox BomItemsJson { get { return Page.FindCamstarControl("BomItemsJson") as CWC.TextBox; } }
        protected CWC.TextBox UpdatedBomItemsJson { get { return Page.FindCamstarControl("UpdatedBomItemsJson") as CWC.TextBox; } }
        protected virtual JQDataGrid MaterialListItemGrid { get { return Page.FindCamstarControl("MaterialListControl") as JQDataGrid; } }
        protected CWC.Button ApplyFilter { get { return Page.FindCamstarControl("ApplyFilter") as CWC.Button; } }
        protected CWC.TextBox FilterValue { get { return Page.FindCamstarControl("FilterValue") as CWC.TextBox; } }
        #endregion Controls

        const string BOM_ITEMS_JSON = "BomItemsJson";
        string ErrorMessage = "";
        string WarningMessage = "";
        string PageSessionId = "";

        #region Page Lifecycle Methods
        public BillImportVP()
        {
            _log.ClassName = "BillImportVP";
        }

        static protected LogClient _log = new LogClient("BomImport");

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if(string.IsNullOrEmpty(PageSessionId))
            {
                PageSessionId = "ExistingItems" + Page.PageInstanceId;
            }

            if (UpdateBomItems != null)
                UpdateBomItems.Click += UpdateBomItems_Click;

            HandleUpdated();
            UpdateGridForImport();
        }

        /// <summary>
        /// Restore any cached grid items so they will all be saved
        /// </summary>
        /// <param name="serviceData"></param>
        public override void GetInputData(Service serviceData)
        {
            TItemChanges[] allItems = HttpContext.Current.Session[PageSessionId] as TItemChanges[];
            if(allItems != null)
            {
                MaterialListItemGrid.Data = allItems;
                FilterValue.Data = "";
                HttpContext.Current.Session[PageSessionId] = null;
            }

            base.GetInputData(serviceData);
        }

        /// <summary>
        /// Filter list items and rebind data
        /// </summary>
        protected void ApplyFilterToGrid()
        {
            TItemChanges[] allItems = HttpContext.Current.Session[PageSessionId] as TItemChanges[];
            if (allItems == null)
            {
                HttpContext.Current.Session[PageSessionId] = MaterialListItemGrid.Data;
                allItems = HttpContext.Current.Session[PageSessionId] as TItemChanges[];
            }

            if (allItems == null)
                return;

            MaterialListItemGrid.ClearData();

            string filterValue = FilterValue.Data?.ToString().ToLower();

            if (filterValue != null)
            {
                var matchingItems = GetMatchingItems(allItems, filterValue);

                MaterialListItemGrid.Data = matchingItems.ToArray();
            }
            else
            {
                MaterialListItemGrid.Data = allItems;
                HttpContext.Current.Session[PageSessionId] = null;
            }

            FilterValue.Focus();
        }

        /// <summary>
        /// Inheriting class must override
        /// </summary>
        /// <param name="allItems"></param>
        /// <param name="filterValue"></param>
        /// <returns></returns>
        virtual protected IEnumerable<TItemChanges> GetMatchingItems(TItemChanges[] allItems, string filterValue)
        {
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateBomItems_Click(object sender, EventArgs e)
        {
            var labelCache = FrameworkManagerUtil.GetLabelCache(HttpContext.Current.Session);
            LabelList issueControlEnumLabels = new LabelList() 
            {
                new Label("ReportFiledWithFDAField_Unknown"),
                new Label("IssueControlEnum_Serialized"),
                new Label("IssueControlEnum_Bulk"),
                new Label("IssueControlEnum_LotAndStockPoint"),
                new Label("IssueControlEnum_StockPointOnly"),
                new Label("IssueControlEnum_NoTracking"),
                new Label("IssueControlEnum_CommentOnly")
            };
            labelCache.GetLabels(issueControlEnumLabels);

            // serialize data from grid and set to hidden field for data contract
            var itemsToUpdate = GetItemsToUpdate(MaterialListItemGrid.Data as TItemChanges[]);
            BomItemsJson.Data = JsonConvert.SerializeObject(itemsToUpdate);
        }

        /// <summary>
        /// Get localized label for the given issue control
        /// </summary>
        /// <param name="issueControl"></param>
        /// <returns></returns>
        protected string GetIssueControlLabel(IssueControlEnum issueControl)
        {
            var labelCache = FrameworkManagerUtil.GetLabelCache(HttpContext.Current.Session);

            string labelName = "ReportFiledWithFDAField_Unknown";

            switch(issueControl)
            {
                case IssueControlEnum.Serialized:
                    labelName = "IssueControlEnum_Serialized";
                    break;
                case IssueControlEnum.NoTracking:
                    labelName = "IssueControlEnum_NoTracking";
                    break;
                case IssueControlEnum.CommentOnly:
                    labelName = "IssueControlEnum_CommentOnly";
                    break;
                case IssueControlEnum.StockPointOnly:
                    labelName = "IssueControlEnum_StockPointOnly";
                    break;
                case IssueControlEnum.Bulk:
                    labelName = "IssueControlEnum_Bulk";
                    break;
                case IssueControlEnum.LotAndStockPoint:
                    labelName = "IssueControlEnum_LotAndStockPoint";
                    break;
            }

            return labelCache.GetLabelByName(labelName).Value;
        }

        /// <summary>
        /// Inheriting class must override
        /// </summary>
        /// <param name="changes"></param>
        /// <returns></returns>
        protected virtual dynamic GetItemsToUpdate(TItemChanges[] changes)
        {
            return new { };
        }

        /// <summary>
        /// User has updated items in the popup and it has closed
        /// </summary>
        protected void HandleUpdated()
        {
            if (MaterialListItemGrid?.Data == null || string.IsNullOrEmpty(UpdatedBomItemsJson?.Data?.ToString()))
                return;

            // deserialize to objects
            List<UpdateItem> updateItems = null;
            try
            {
                updateItems = JsonConvert.DeserializeObject<List<UpdateItem>>(UpdatedBomItemsJson.Data.ToString());
            }
            catch (JsonReaderException jre)
            {
                Page.StatusBar.WriteError(jre.Message);
                return;
            }

            UpdateBom(updateItems);
        }

        /// <summary>
        /// Save BOM
        /// </summary>
        /// <param name="updateItems"></param>
        protected virtual void UpdateBom(List<UpdateItem> updateItems)
        {

        }

        /// <summary>
        /// update grid with info from BOM item import popup
        /// </summary>
        protected void UpdateGridForImport()
        {
            if (string.IsNullOrEmpty(BomItemsJson.Data?.ToString()))
                return;

            bool updateGridSuccess = true;

            // deserialize to objects
            List<ImportItem> importItems = null;
            try
            {
                importItems = JsonConvert.DeserializeObject<List<ImportItem>>(BomItemsJson.Data.ToString());
            }
            catch (JsonReaderException jre)
            {
                Page.StatusBar.WriteError(jre.Message);
                return;
            }

            List<TItemChanges> newItemChangesList = new List<TItemChanges>();

            // grab existing objects
            TItemChanges[] existingItemChanges = new TItemChanges[0];
            if (MaterialListItemGrid?.Data != null)
                existingItemChanges = MaterialListItemGrid.Data as TItemChanges[];
            MaterialListItemGrid.ClearData();

            List<TItemChanges> alreadyUpdated = new List<TItemChanges>();

            // add new objects to array
            foreach (ImportItem importItem in importItems)
            {
                if (!string.IsNullOrEmpty(importItem.BomName))
                    continue;

                // create new or update existing?
                var matching = existingItemChanges
                    .Where(existing =>
                    {
                        // update each existing only ONCE
                        if (alreadyUpdated.Contains(existing))
                            return false;

                        bool match = false;

                        // ref des is empty - look for existing items with empty ref des and matching product
                        if (string.IsNullOrEmpty(importItem.RefDes))
                        {
                            match = string.IsNullOrEmpty(existing?.ReferenceDesignator?.ToString()) && importItem.ProductName == existing.Product.Name && importItem.ProductRevision == existing.Product.Revision;
                        }
                        else
                        {
                            match = importItem.RefDes == existing?.ReferenceDesignator?.ToString();
                        }

                        return match;
                    }).ToArray();

                // doesn't exist - create a new material list item
                if (matching.Length == 0)
                {
                    TItemChanges newItemChanges = new TItemChanges();

                    updateGridSuccess = UpdateFromImport(newItemChanges, importItem) && updateGridSuccess;

                    newItemChangesList.Add(newItemChanges);
                }
                // already exists - update first match
                else
                {
                    updateGridSuccess = UpdateFromImport(matching[0], importItem) && updateGridSuccess;
                    matching[0].ListItemAction = ListItemAction.Change;
                    alreadyUpdated.Add(matching[0]);
                }
            }

            // rebind array of objects (rows)
            newItemChangesList.InsertRange(0, existingItemChanges);
            MaterialListItemGrid.Data =  newItemChangesList.ToArray();

            // hang onto this for when we save
            HttpContext.Current.Session[BOM_ITEMS_JSON] = BomItemsJson.Data.ToString();

            // clear out import items from hidden field 
            BomItemsJson.ClearData();
            UpdatedBomItemsJson?.ClearData();

            if (!updateGridSuccess)
                WarningMessage = $"Unable to import all values.  See {_log.GetDefaultLogFilePath()} for details.";  // TODO - localize
        }

        /// <summary>
        /// Construct ItemChanges based on the given updates (from the popup)
        /// </summary>
        /// <param name="updateItems"></param>
        /// <returns></returns>
        protected TItemChanges[] GetUpdatedChanges(List<UpdateItem> updateItems)
        {
            List<TItemChanges> updatedChanges = new List<TItemChanges>();

            // apply updates
            TItemChanges[] listItems = MaterialListItemGrid.Data as TItemChanges[];
            foreach (TItemChanges listItem in listItems)
            {
                // find matching update (need item index)
                var matchingUpdate = updateItems.Find(ui => ui.Name == listItem.Name);

                TItemChanges updatedItemChanges = GetItemChangesFromUpdate(listItem.ListItemIndex, matchingUpdate);
                updatedChanges.Add(updatedItemChanges);
            }

            return updatedChanges.ToArray();
        }


        /// <summary>
        /// Null out the Product of any Changes object that has an UnverifiedProduct
        /// </summary>
        /// <param name="materialList"></param>
        /// <returns></returns>
        public void UpdateForUnverified(MaterialListItemChanges[] materialList)
        {
            foreach (MaterialListItemChanges mlic in materialList)
            {
                if (!string.IsNullOrEmpty(mlic?.UnverifiedProductName?.Value))
                {
                    mlic.Product = null;
                }
            }
        }

        /// <summary>
        /// Update the given item changes object with info from the given line item from the import file.
        /// Used for updating grid items.
        /// </summary>
        /// <param name="updateMe"></param>
        /// <param name="importItem"></param>
        /// <returns>success/fail</returns>
        protected virtual bool UpdateFromImport(TItemChanges updateMe, ImportItem importItem)
        {
            updateMe.ReferenceDesignator = importItem.RefDes;
            updateMe.UnverifiedProductName = importItem.ProductName;
            updateMe.UnverifiedProductRevision = importItem.ProductRevision;
            updateMe.QtyRequired = importItem.Quantity;
            updateMe.IssueControl = (IssueControlEnum)importItem.IssueControl;
            updateMe.Product = new RevisionedObjectRef(importItem.ProductName, importItem.ProductRevision);

            if (importItem.AllowOverConsumption != null)
                updateMe.AllowOverConsumption = importItem.AllowOverConsumption;
            else
                // Designer, grid and popup default this to true, but not showing checked in popup.  Bandaid for now.
                updateMe.AllowOverConsumption = true;

            if (importItem.AllowUnderConsumption != null)
                updateMe.AllowUnderConsumption = importItem.AllowUnderConsumption;

            return true;
        }

        /// <summary>
        /// Construct an ItemChanges object based on the given updated item
        /// </summary>
        /// <param name="listItemIndex">Assign this index to the newly constructed changes object</param>
        /// <param name="updateItem"></param>
        protected virtual TItemChanges GetItemChangesFromUpdate(int? listItemIndex, UpdateItem updateItem)
        {
            // return value
            TItemChanges changes = new TItemChanges();

            if(!string.IsNullOrWhiteSpace(updateItem.IssueControl))
            {
                // change string -> int
                if(int.TryParse(updateItem.IssueControl, out int issueControl))
                {
                    changes.IssueControl = (IssueControlEnum)issueControl;
                }
            }

            if (!string.IsNullOrWhiteSpace(updateItem.ProductNameRev))
            {
                string[] prodNameRev = ParseRevisionedObject(updateItem.ProductNameRev);

                changes.Product = prodNameRev.Length == 1
                    ? new RevisionedObjectRef(prodNameRev[0])
                    : new RevisionedObjectRef(prodNameRev[0], prodNameRev[1]);
            }

            if(updateItem.Quantity != null)
            {
                changes.QtyRequired = updateItem.Quantity.Value;
            }

            changes.ListItemAction = ListItemAction.Change;
            changes.Name = updateItem.Name;
            changes.ListItemIndex = listItemIndex;

            return changes;
        }

        /// <summary>
        /// Use configured delimiter
        /// </summary>
        /// <param name="nameRev">string containing name and optionally revision: "Foo:1" </param>
        /// <returns>Array with 1 elem for name and possibly a second for the revision</returns>
        protected static string[] ParseRevisionedObject(string nameRev)
        {
            string[] delimiters = { Utilities.CamstarPortalSection.GetRevisionDelimiter() };

            return nameRev.Split(delimiters, 2, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (!string.IsNullOrEmpty(ErrorMessage))
                Page.StatusBar.WriteError(ErrorMessage);
            else if(!string.IsNullOrEmpty(WarningMessage))
                Page.StatusBar.WriteWarning(WarningMessage);

            ErrorMessage = "";
            WarningMessage = "";

            //if(!Page.IsPostBack)
            //{
            string initScript = $"BomItemImportVP.initialize();";
            ScriptManager.RegisterStartupScript(this, GetType(), "BomItemImportVPInitialize", initScript, true);
            //}
        }

        /// <summary>
        /// 
        /// </summary>
        public override void ClearValues()
        {
            base.ClearValues();

            // do NOT hang onto any values that may have been filtered out
            HttpContext.Current.Session[PageSessionId] = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="status"></param>
        /// <param name="serviceData"></param>
        public override void PostExecute(ResultStatus status, Service serviceData)
        {
            base.PostExecute(status, serviceData);
            ErrorMessage = "";
            WarningMessage = "";

            if (status.IsSuccess == true)
            {
                var bomItemsJson = HttpContext.Current.Session[BOM_ITEMS_JSON];

                // key: BOM name, rev, values: error message
                Dictionary<string, string> bomErrs = new Dictionary<string, string>();
                Dictionary<string, string> productErrs = new Dictionary<string, string>();

                if (bomItemsJson != null)
                {
                    List<ImportItem> importItems = JsonConvert.DeserializeObject<List<ImportItem>>(bomItemsJson.ToString());

                    var bomNameRevs = importItems
                        .Select(ii => new { ii.BomName, ii.BomRevision })
                        .Where(ii => !string.IsNullOrEmpty(ii.BomName))
                        .Distinct();

                    foreach (var bomNameRev in bomNameRevs)
                    {
                        var itemsThisBom = importItems.Where(ii => !string.IsNullOrEmpty(ii.BomName) && ii.BomName == bomNameRev.BomName && ii.BomRevision == bomNameRev.BomRevision);
                        // create BOM
                        ResultStatus createBomStatus = CreateBom(bomNameRev.BomName, bomNameRev.BomRevision, itemsThisBom.ToList());
                        if (!createBomStatus.IsSuccess)
                        {
                            bomErrs.Add($"{bomNameRev.BomName}, {bomNameRev.BomRevision}", createBomStatus.Message ?? createBomStatus?.ExceptionData?.Description);
                        }
                        else
                        {
                            // any BOM items created during this import with a matching Product?
                            int itemsWithMatchingProd = importItems.Where(ii => ii.ProductName == bomNameRev.BomName && ii.ProductRevision == bomNameRev.BomRevision).Count();
                            if (itemsWithMatchingProd > 0)
                            {
                                // update Product to use newly created BOM
                                ResultStatus updateProdStatus = UpdateProduct(bomNameRev.BomName, bomNameRev.BomRevision);
                                if (!updateProdStatus.IsSuccess)
                                {
                                    productErrs.Add($"{bomNameRev.BomName}, {bomNameRev.BomRevision}", updateProdStatus.Message ?? updateProdStatus?.ExceptionData?.Description);
                                }
                            }
                        }
                    }
                }

                // build messages for any errors
                var labelCache = FrameworkManagerUtil.GetLabelCache(HttpContext.Current.Session);
                var labels = new[]
                {
                    new Label("WebUI_NotImportBOMs"),
                    new Label("WebUI_NotUpdateProducts"),
                    new Label("WebUI_SeeLogFileX")
                };
                labelCache.GetLabels(new LabelList(labels));

                if (bomErrs.Count() > 0)
                {
                    ErrorMessage = labelCache.GetLabelByName("WebUI_NotImportBOMs").Value;
                    ErrorMessage += string.Join(", ", bomErrs.Keys);
                }

                if (productErrs.Count() > 0)
                {
                    if (!string.IsNullOrEmpty(ErrorMessage))
                        ErrorMessage += " ";

                    ErrorMessage += labelCache.GetLabelByName("WebUI_NotUpdateProducts").Value;
                    ErrorMessage += string.Join(", ", productErrs.Keys);
                }

                if (!string.IsNullOrEmpty(ErrorMessage))
                {
                    ErrorMessage += " " + string.Format(labelCache.GetLabelByName("WebUI_SeeLogFileX").Value, _log.GetDefaultLogFilePath());

                    // build error message for log file
                    var bomErrorMessages = string.Join(System.Environment.NewLine, bomErrs.Select(be => $"{be.Key}: {be.Value}"));
                    var productErrorMessages = string.Join(System.Environment.NewLine, productErrs.Select(be => $"{be.Key}: {be.Value}"));

                    _log.LogMessage(LogSeverity.Error, "PostExecute", string.Join(System.Environment.NewLine, new List<string>() { "Error(s) importing BOM Items", bomErrorMessages, productErrorMessages }));
                }

            }

            HttpContext.Current.Session[BOM_ITEMS_JSON] = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="rev"></param>
        /// <param name="importItems"></param>
        /// <returns></returns>
        protected virtual ResultStatus CreateBom(string name, string revision, List<ImportItem> importItems)
        {
            // Must be implemented by child classes
            throw new NotImplementedException("CreateBom must be impemented.");
            //return new ResultStatus("Not Implemented", false);
        }

        /// <summary>
        /// Set the BOM name/rev of the given Product
        /// </summary>
        /// <param name="name">Product and BOM name</param>
        /// <param name="revision">Product and BOM revision</param>
        /// <returns></returns>
        protected ResultStatus UpdateProduct(string name, string revision)
        {
            ProductMaintService svc = new ProductMaintService(FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);

            ProductMaint maint = new ProductMaint();
            maint.ObjectToChange = new RevisionedObjectRef(name, revision);

            maint.ObjectChanges = GetProductChanges(name, revision);

            svc.BeginTransaction();

            svc.Load(maint);
            svc.ExecuteTransaction(maint);
            return svc.CommitTransaction();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="revision"></param>
        /// <returns></returns>
        protected virtual ProductChanges GetProductChanges(string name, string revision)
        {
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="enumType"></param>
        /// <param name="enumValue"></param>
        /// <returns></returns>
        protected bool EnumIsDefined(Type enumType, int enumValue)
        {
            bool isDefined = Enum.IsDefined(enumType, enumValue);
            if (!isDefined)
                _log.LogMessage(LogSeverity.Error, "EnumIsDefined", $"Unable to convert {enumValue} to a {enumType.Name} value.");
            return isDefined;
        }
        #endregion Page Lifecycle Methods

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override IEnumerable<ScriptReference> GetScriptReferences()
        {
            // for parsing .csv
            yield return new ScriptReference("~/scripts/jquery/d3.v3.min.js");
            yield return new ScriptReference("~/Scripts/BomItemImportVP.js");
        }
    }

    /// <summary>
    /// Deserialized from hidden text field - array of these is sent back from the import popup
    /// </summary>
    public partial class ImportItem
    {
        public string BomName { get; set; }
        public string BomRevision { get; set; }
        public string RefDes { get; set; }
        public string ProductName { get; set; }
        public string ProductRevision { get; set; }
        public double Quantity { get; set; }
        public int IssueControl { get; set; }
        public bool? AllowUnderConsumption { get; set; } = null;
        public bool? AllowOverConsumption { get; set; } = null;
    }

    /// <summary>
    /// Deserialized from hidden text field - array of these is sent back from the update popup
    /// </summary>
    public partial class UpdateItem
    {
        public string Name { get; set; }
        public string RefDes { get; set; }
        public string IssueControl { get; set; }
        public string ProductNameRev { get; set; }
        public string SpecNameRev { get; set; }
        public double? Quantity { get; set; }
    }
}