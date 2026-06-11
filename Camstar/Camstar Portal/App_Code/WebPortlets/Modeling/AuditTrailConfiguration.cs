// Copyright Siemens 2023
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.PortalFramework;
using Newtonsoft.Json;
using Label = Camstar.WCF.ObjectStack.Label;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;

namespace Camstar.WebPortal.WebPortlets
{
    /// <summary>
    /// Used on the AuditTrailConfiguration_VP
    /// </summary>
    public class AuditTrailConfiguration : MatrixWebPart
    {
        protected virtual JQTabContainer DescriptionTabContainer { get { return Page.FindCamstarControl("DescriptionTabContainer") as JQTabContainer; } }

        protected virtual JQDataGrid HistoryMainlineGridAvailableFields
        {
            get { return Page.FindCamstarControl("HistoryMainlineGridAvailableFields") as JQDataGrid; }
        }

        protected virtual JQDataGrid HistoryMainlineGridSelectedFields
        {
            get { return Page.FindCamstarControl("HistoryMainlineGridSelectedFields") as JQDataGrid; }
        }

        protected virtual Button AddHistoryMainlineFieldButton
        {
            get { return Page.FindCamstarControl("AddHistoryMainlineFieldButton") as Button; }
        }

        protected virtual Button RemoveHistoryMainlineFieldButton
        {
            get { return Page.FindCamstarControl("RemoveHistoryMainlineFieldButton") as Button; }
        }

        protected virtual Button HistoryMainlineMoveUpSelectedFieldButton
        {
            get { return Page.FindCamstarControl("HistoryMainlineMoveUpSelectedFieldButton") as Button; }
        }

        protected virtual Button HistoryMainlineMoveDownSelectedFieldButton
        {
            get { return Page.FindCamstarControl("HistoryMainlineMoveDownSelectedFieldButton") as Button; }
        }

        protected virtual JQDataGrid TransactionDetailAvailableFieldsGrid
        {
            get { return Page.FindCamstarControl("TransactionHistoryMainlineGridAvailableFields") as JQDataGrid; }
        }

        protected virtual JQDataGrid TransactionDetailSelectedFieldsGrid
        {
            get { return Page.FindCamstarControl("TransactionHistoryMainlineGridSelectedFields") as JQDataGrid; }
        }

        protected virtual Button AddFieldToTransactionDetailButton
        {
            get { return Page.FindCamstarControl("AddTransactionDetailFieldButton") as Button; }
        }

        protected virtual Button RemoveFieldFromTransactionDetailButton
        {
            get { return Page.FindCamstarControl("RemoveTransactionDetailFieldButton") as Button; }
        }
        
        private const string HISTORY_MAINLINE_TRANSACTION = "HistoryMainline";
        private const string CDOTYPE_HISTORY_VIEW = "HistoryView";
        
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (Page.EventTarget == "TransactionSelected")
            {
                TransactionDetailAvailableFieldsGrid.GridContext.GetSelectedItems(true);
                TransactionDetailSelectedFieldsGrid.GridContext.GetSelectedItems(true);
                SetSelectedFieldsOfTransactionFromUI();
                SetSelectedTransaction(Page.EventArgument);
                PopulateAvailableAndSelectedFieldsOfTransaction(Page.EventArgument);                
            }
            else
            {
                // Remove Change Mgt tab
                //DescriptionTabContainer.Tabs.RemoveAt(2);
                AddHistoryMainlineFieldButton.Click += AddHistoryMainlineFieldButton_Click;
                RemoveHistoryMainlineFieldButton.Click += RemoveHistoryMainlineFieldButton_Click;
                AddFieldToTransactionDetailButton.Click += AddTransactionDetailFieldButton_Click;
                RemoveFieldFromTransactionDetailButton.Click += RemoveTransactionDetailFieldButton_Click;

                GetLabels();

                if (Page.Session["TreeDefinitionData"] == null)
                {
                    Page.Session["TreeDefinitionData"] = GenerateCollectionDefinitionJsonDocument();
                }
                if (Page.Session["TreeDataData"] == null)
                {
                    Page.Session["TreeDataData"] = GenerateJSONDataDocument();
                }
            }
        }

        private void SetSelectedTransaction(string transactionName)
        {            
            Page.DataContract.SetValueByName("SelectedTransaction", transactionName);
            var changedTransactionDetails = (Dictionary<string, TransactionDetail>)Page.DataContract.GetValueByName("changedTransactionDetails");
            if (changedTransactionDetails.ContainsKey(transactionName))
            {
                changedTransactionDetails[transactionName].IsSelected = true;
            }
        }

        private void PopulateAvailableAndSelectedFieldsOfTransaction(string transactionName)
        {
            var transactionDetails = (Dictionary<string, TransactionDetail>)Page.DataContract.GetValueByName("changedTransactionDetails");
            List<AuditTrailConfigurationRow> selectedFields = new List<AuditTrailConfigurationRow>();
            if (transactionDetails.ContainsKey(transactionName))
            {
                selectedFields = transactionDetails[transactionName].SelectedFields;
            }
            TransactionDetailAvailableFieldsGrid.Data = SortRows(
                GetAuditTrailConfigurationRows(
                    GetAvailableFields(
                        transactionName,
                        selectedFields.Select(
                            field => new Primitive<string>
                            {
                                Value = field.ColumnName
                            }
                        ).ToArray()
                    )
                )
            );
            TransactionDetailSelectedFieldsGrid.Data = selectedFields.ToArray();
        }


        /// <summary>
        /// Add checked fields from AvailableFieldGrid to SelectedFieldGrid
        /// </summary>
        /// <param name="availableFieldGrid">AvailableFieldGrid</param>
        /// <param name="selectedfieldGrid">SelectedFieldGrid</param>
        /// <returns>List of AuditTrailConfigurationRow in SelectedFieldGrid after addition</returns>
        protected List<AuditTrailConfigurationRow> AddSelectedFields(JQDataGrid availableFieldGrid, JQDataGrid selectedfieldGrid)
        {
            var selectedAvailableFields = availableFieldGrid.GridContext.GetSelectedItems(false);
            var selectedFields = (selectedfieldGrid.Data as AuditTrailConfigurationRow[]).ToList();
            var availableFields = (availableFieldGrid.Data as AuditTrailConfigurationRow[]).ToList();
            foreach (AuditTrailConfigurationRow row in selectedAvailableFields)
            {
                selectedFields.Add(row);
                availableFields.RemoveAt(
                     availableFields.FindLastIndex(field => field.ColumnName.Equals(row.ColumnName))
                );
            }
            availableFieldGrid.Data = availableFields.ToArray();
            selectedfieldGrid.Data = selectedFields.ToArray();
            availableFieldGrid.GridContext.GetSelectedItems(true);
            return selectedFields;
        }
        /// <summary>
        /// Remove checked fields from SelectedFieldGrid and add them to AvailableFieldGrid
        /// </summary>
        /// <param name="availableFieldGrid">AvailableFieldGrid</param>
        /// <param name="selectedfieldGrid">SelectedFieldGrid</param>
        /// <returns>List of AuditTrailConfigurationRow in SelectedFieldGrid after deletion</returns>
        protected List<AuditTrailConfigurationRow> RemoveSelectedFields(JQDataGrid availableFieldGrid, JQDataGrid selectedFieldGrid)
        {
            var checkedSelectedFields = selectedFieldGrid.GridContext.GetSelectedItems(false);
            var selectedFields = (selectedFieldGrid.Data as AuditTrailConfigurationRow[]).ToList();
            var availableFields = (availableFieldGrid.Data as AuditTrailConfigurationRow[]).ToList();

            foreach (AuditTrailConfigurationRow row in checkedSelectedFields)
            {
                availableFields.Add(row);
                selectedFields.RemoveAt(
                    selectedFields.FindLastIndex(field => field.ColumnName.Equals(row.ColumnName))
                );
            }
            availableFieldGrid.Data = SortRows(availableFields.ToArray());
            selectedFieldGrid.Data = selectedFields.ToArray();
            selectedFieldGrid.GridContext.GetSelectedItems(true);
            return selectedFields;
        }
        /// <summary>
        /// Click handler for Add button
        /// Adds available fields of History Mainline to selected fields and
        /// remove those fields from "Available Fields" grid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void AddHistoryMainlineFieldButton_Click(object sender, EventArgs e)
        {
            if (ValidateMoveRequest(
                HistoryMainlineGridAvailableFields,
                HistoryMainlineGridSelectedFields,
                HistoryMainlineGridAvailableFields.GridContext.GetSelectedItems(false)
                ))
            {
                AddSelectedFields(HistoryMainlineGridAvailableFields, HistoryMainlineGridSelectedFields);
            }
        }
        /// <summary>
        /// Click handler for Remove button
        /// Removes selected fields of History Mainline and adds them to available fields
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void RemoveHistoryMainlineFieldButton_Click(object sender, EventArgs e)
        {
            if (ValidateMoveRequest(
                HistoryMainlineGridAvailableFields,
                HistoryMainlineGridSelectedFields,
                HistoryMainlineGridSelectedFields.GridContext.GetSelectedItems(false)
                ))
            {
                RemoveSelectedFields(HistoryMainlineGridAvailableFields, HistoryMainlineGridSelectedFields);
            }
        }
        /// <summary>
        /// Validate the move operation i.e. Add/Remove operation in between AvailableFieldGrid to SelectedFieldGrid
        /// </summary>
        /// <param name="availableFieldGrid">AvailableFieldGrid</param>
        /// <param name="selectedfieldGrid">SelectedFieldGrid</param>
        /// <param name="selectedFields">Array of checked fields to move</param>
        /// <returns>true/false based on the validity of data</returns>
        protected bool ValidateMoveRequest(JQDataGrid availableFieldGrid,
            JQDataGrid selectedfieldGrid,
            object[] selectedFields)
        {
            if (availableFieldGrid.Data == null || selectedfieldGrid.Data == null)
                return false;
            if (selectedFields == null)
                return false;
            if (selectedFields.Length == 0)
                return false;
            return true;
        }
        /// <summary>
        /// Build the dictionary from transactiondetails
        /// </summary>
        /// <param name="detailChanges">Array of HistViewDetailChanges</param>
        /// <returns>Dictionary of transactionName -> list of selected fields</returns>
        private Dictionary<string, TransactionDetail> ConvertFieldsToDictionary(
            HistViewDtlChanges[] detailChanges
        )
        {
            Dictionary<string, TransactionDetail> dictionary = new Dictionary<string, TransactionDetail>();
            foreach (var transaction in detailChanges)
            {
                dictionary.Add(transaction.Name.Value, new TransactionDetail(transaction.ListItemIndex, GetAuditTrailConfigurationRows(transaction.UIFields).ToList()));
            }
            return dictionary;
        }
        /// <summary>
        /// Get Selected Transaction
        /// </summary>
        /// <returns>Selected Transaction</returns>
        private string GetSelectedTransaction()
        {            
            object selectedTransaction = Page.DataContract.GetValueByName("SelectedTransaction");
            if (selectedTransaction !=null)
            {
                return Convert.ToString(selectedTransaction);
            }
            return string.Empty;
        }

        private void SetSelectedFieldsOfTransactionFromUI(string transaction)
        {
            SetSelectedFieldsOfTransaction(
                transaction,
                (TransactionDetailSelectedFieldsGrid.Data as AuditTrailConfigurationRow[]).ToList()
            );
        }

        private void SetSelectedFieldsOfTransactionFromUI()
        {
            string selectedTransaction = GetSelectedTransaction();
            if (!string.IsNullOrEmpty(selectedTransaction))
            {
                SetSelectedFieldsOfTransactionFromUI(selectedTransaction);
            }
        }

        /// <summary>
        /// Set value of fields to transaction in the dictionary of transactionDetails
        /// </summary>
        /// <param name="transactionName">Transaction Name</param>
        /// <param name="fields">List of fields</param>
        private void SetSelectedFieldsOfTransaction(string transactionName,
            List<AuditTrailConfigurationRow> fields)
        {
            var changedTransactionDetails = (Dictionary<string, TransactionDetail>)Page.DataContract.GetValueByName("changedTransactionDetails");
            var oldTransactionDetails = (Dictionary<string, TransactionDetail>)Page.DataContract.GetValueByName("transactionDetails");

            if (fields.Count == 0 && !oldTransactionDetails.ContainsKey(transactionName) && changedTransactionDetails.ContainsKey(transactionName))
            {
                changedTransactionDetails.Remove(transactionName);
            }
            else if (changedTransactionDetails.ContainsKey(transactionName))
            {
                changedTransactionDetails[transactionName].SelectedFields = fields;
            }
            else if (fields.Count > 0)
            {
                changedTransactionDetails.Add(transactionName, new TransactionDetail(null, fields));
            }
            Page.DataContract.SetValueByName("changedTransactionDetails", changedTransactionDetails);
        }
        /// <summary>
        /// Click handler for Add button
        /// Adds available fields of selected transaction to selected fields and
        /// remove those fields from "Available Fields" grid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void AddTransactionDetailFieldButton_Click(object sender, EventArgs e)
        {
            if (ValidateMoveRequest(
                TransactionDetailAvailableFieldsGrid,
                TransactionDetailSelectedFieldsGrid,
                TransactionDetailAvailableFieldsGrid.GridContext.GetSelectedItems(false)
                ))
            {
                // Get the transaction
                string transactionName = GetSelectedTransaction();
                SetSelectedFieldsOfTransaction(
                    transactionName,
                    AddSelectedFields(
                        TransactionDetailAvailableFieldsGrid,
                        TransactionDetailSelectedFieldsGrid
                    )
                );
            }
        }
        /// <summary>
        /// Click handler for Remove button
        /// Removes selected fields of selected transaction and adds them to available fields
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void RemoveTransactionDetailFieldButton_Click(object sender, EventArgs e)
        {
            if (ValidateMoveRequest(
                TransactionDetailAvailableFieldsGrid,
                TransactionDetailSelectedFieldsGrid,
                TransactionDetailSelectedFieldsGrid.GridContext.GetSelectedItems(false)
                ))
            {
                // Get the transaction
                string transactionName = GetSelectedTransaction();

                SetSelectedFieldsOfTransaction(
                    transactionName,
                    RemoveSelectedFields(
                        TransactionDetailAvailableFieldsGrid,
                        TransactionDetailSelectedFieldsGrid
                    )
                );
            }
        }
        public override void GetInputData(Service serviceData)
        {
            base.GetInputData(serviceData);
            var selectedFieldRows = HistoryMainlineGridSelectedFields.Data as AuditTrailConfigurationRow[];
            var selectedFields = new List<Primitive<string>>();
            // For creating new instance
            if (((Camstar.WebPortal.PortalFramework.MaintenanceBehaviorContext)Page.PortalContext).State == MaintenanceBehaviorContext.MaintenanceState.New)
            {
                foreach (AuditTrailConfigurationRow row in selectedFieldRows)
                {
                    selectedFields.Add(GetPrimitiveString(row, ListItemAction.Add, -1));
                }
                if (((HistoryViewMaint)serviceData).ObjectChanges == null)
                {
                    return;
                }
                // Set selected fields for HistoryMainline transaction
                ((HistoryViewMaint)serviceData).ObjectChanges.HistViewHistMainlineDtl = new HistViewHistMainlineDtlChanges();
                ((HistoryViewMaint)serviceData).ObjectChanges.HistViewHistMainlineDtl.UIFields = selectedFields.ToArray();
                // Set selected fields for HistoryDetails transaction
                HistViewDtlChanges historyViewDetailsChanges = new HistViewDtlChanges();
                historyViewDetailsChanges.Name = "HistoryDetails";
                historyViewDetailsChanges.UIFields = new Primitive<string>[] { "DisplayName" };
                ((HistoryViewMaint)serviceData).ObjectChanges.HistViewDtls = new HistViewDtlChanges[] { historyViewDetailsChanges };
                SetSelectedFieldsOfTransactionFromUI();
                ((HistoryViewMaint)serviceData).ObjectChanges.HistViewDtls = GetDiffOfHistViewDtlChanges(
                    (Dictionary<string, TransactionDetail>)Page.DataContract.GetValueByName("transactionDetails"),
                    (Dictionary<string, TransactionDetail>)Page.DataContract.GetValueByName("changedTransactionDetails")
                );
                SetSelectedTransaction(string.Empty);
            }// For editing existing instance
            else if (((Camstar.WebPortal.PortalFramework.MaintenanceBehaviorContext)Page.PortalContext).State == MaintenanceBehaviorContext.MaintenanceState.Edit)
            {
                if (((HistoryViewMaint)serviceData).ObjectChanges == null)
                {
                    ((HistoryViewMaint)serviceData).ObjectChanges = new HistoryViewChanges();
                }
                ((HistoryViewMaint)serviceData).ObjectChanges.HistViewHistMainlineDtl = new HistViewHistMainlineDtlChanges();

                var storedSelectedFields = Page.Session["HistoryMainlineSelectedFields"] as List<AuditTrailConfigurationRow>;
                var selectedHistoryMainlineFields = (HistoryMainlineGridSelectedFields.Data as AuditTrailConfigurationRow[]).ToList();
                selectedFields = GetChangedFields(storedSelectedFields, selectedHistoryMainlineFields);
                Primitive<string>[] fields = selectedFields.ToArray();
                ((HistoryViewMaint)serviceData).ObjectChanges.HistViewHistMainlineDtl.UIFields = fields;
                SetSelectedFieldsOfTransactionFromUI();
                ((HistoryViewMaint)serviceData).ObjectChanges.HistViewDtls = GetDiffOfHistViewDtlChanges(
                    (Dictionary<string, TransactionDetail>)Page.DataContract.GetValueByName("transactionDetails"),
                    (Dictionary<string, TransactionDetail>)Page.DataContract.GetValueByName("changedTransactionDetails")
                );
                SetSelectedTransaction(string.Empty);

            }
        }

        /// <summary>
        /// Get changes made in the transaction details
        /// </summary>
        /// <param name="originalDict">Dictionary of old transaction details</param>
        /// <param name="changedDict">Dictionary of new transaction details</param>
        /// <returns>Changed maded in the transaction details</returns>
        public HistViewDtlChanges[] GetDiffOfHistViewDtlChanges(
            Dictionary<string, TransactionDetail> originalDict,
            Dictionary<string, TransactionDetail> changedDict)
        {
            List<HistViewDtlChanges> changes = new List<HistViewDtlChanges>();

            // Check for added transaction
            if (changedDict.Count > originalDict.Count)
            {
                changes.AddRange(GetNewTransactions(originalDict, changedDict));
            }

            // Check for changed selected fields inside the transaction
            foreach (var transaction in originalDict)
            {
                if (changedDict[transaction.Key].IsSelected)
                {
                    List<Primitive<string>> changedFields = GetChangedFields(
                        transaction.Value.SelectedFields.ToList(),
                        changedDict[transaction.Key].SelectedFields.ToList()
                    );
                    if (changedFields.Count > 0)
                    {
                        HistViewDtlChanges change = GetHistViewDtlChanges(transaction.Key, changedFields.ToArray());
                        if (transaction.Value.ListItemIndex.HasValue)
                        {
                            //  Check to see if all fields are being removed - if so just delete the main item
                            int delCount = 0;
                            foreach (Camstar.WCF.ObjectStack.Primitive<string> item in change.UIFields)
                            {
                                if (item.ListItemAction == ListItemAction.Delete)
                                    delCount++;
                            }
                            if (delCount == change.UIFields.Length)
                            {
                                change.ListItemAction = ListItemAction.Delete;
                                change.ListItemIndex = transaction.Value.ListItemIndex;
                                change.UIFields = null;
                                change.Name = null;
                            }
                            else
                            {
                                change.ListItemAction = ListItemAction.Change;
                                change.ListItemIndex = transaction.Value.ListItemIndex;
                            }
                        }
                        changes.Add(change);
                    }
                }
            }

            return changes.ToArray();
        }
        /// <summary>
        /// Get new transaction added in the transaction details
        /// </summary>
        /// <param name="originalDict">Dictionary of old transaction details</param>
        /// <param name="changedDict">Dictionary of new transaction details</param>
        /// <returns>List of new transaction details</returns>
        private List<HistViewDtlChanges> GetNewTransactions(
            Dictionary<string, TransactionDetail> originalDict,
            Dictionary<string, TransactionDetail> changedDict
        )
        {
            List<HistViewDtlChanges> changes = new List<HistViewDtlChanges>();
            foreach (var item in changedDict)
            {
                string transactionName = item.Key;
                if (!originalDict.ContainsKey(transactionName))
                {
                    List<AuditTrailConfigurationRow> selectedFields = changedDict[transactionName].SelectedFields;
                    changes.Add(GetHistViewDtlChanges(transactionName,
                    selectedFields.Select(
                        row => GetPrimitiveString(row, ListItemAction.Add, -1)).ToArray()
                    ));
                }
            }
            return changes;
        }
        /// <summary>
        /// Builds the HistViewDtlChanges object from name and fields
        /// </summary>
        /// <param name="name">Name of the transaction</param>
        /// <param name="fields">Selected fields associated to the transaction</param>
        /// <returns>HistViewDtlChanges object</returns>
        private HistViewDtlChanges GetHistViewDtlChanges(string name, Primitive<string>[] fields)
        {
            HistViewDtlChanges histViewDtl = new HistViewDtlChanges();
            histViewDtl.Name = name;
            histViewDtl.UIFields = fields;
            return histViewDtl;
        }

        /// <summary>
        /// Get list of changed fields
        /// </summary>
        /// <param name="oldFields"></param>
        /// <param name="newFields"></param>
        /// <returns>list of changed fields</returns>
        public List<Primitive<string>> GetChangedFields(
            List<AuditTrailConfigurationRow> oldFields,
            List<AuditTrailConfigurationRow> newFields
        )
        {
            var selectedFields = new List<Primitive<string>>();

            // Check for elements to be deleted
            if (oldFields.Count > newFields.Count)
            {
                int deleteElementsAtIndex = newFields.Count;
                for (
                    int index = deleteElementsAtIndex;
                    index < oldFields.Count;
                    index++
                )
                {
                    selectedFields.Add(
                        GetPrimitiveString(
                            oldFields[index],
                            ListItemAction.Delete,
                            deleteElementsAtIndex
                            )
                        );
                }
            }

            int oldFieldIndex = 0;
            for (int newFieldIndex = 0; newFieldIndex < newFields.Count; newFieldIndex++)
            {
                AuditTrailConfigurationRow oldRow = null;
                if (oldFieldIndex < oldFields.Count)
                {
                    oldRow = oldFields[oldFieldIndex];
                    oldFieldIndex++;
                }
                AuditTrailConfigurationRow newRow = newFields[newFieldIndex];
                if (oldRow != null && oldRow.ColumnName != newRow.ColumnName)
                {
                    // Check for elements which are changed
                    selectedFields.Add(GetPrimitiveString(newRow, ListItemAction.Change, newFieldIndex));
                } else if (oldRow == null)
                {
                    // Check for elements which are added
                    selectedFields.Add(GetPrimitiveString(newRow, ListItemAction.Add, -1));
                }
            }
            return selectedFields;
        }
        /// <summary>
        /// Create Primitive String from AuditTrailConfigurationRow, ListItemAction and Index to update
        /// </summary>
        /// <param name="row"></param>
        /// <param name="action"></param>
        /// <param name="index"></param>
        /// <returns>Primitive string which contains field needed to add/update/delete</returns>
        public Primitive<string> GetPrimitiveString(AuditTrailConfigurationRow row,
            ListItemAction action,
            int index)
        {
            Primitive<string> field = new Primitive<string>();
            field.Value = row.ColumnName;
            if (index != -1)
            {
                field.ListItemIndex = index;
            }
            field.ListItemAction = action;
            return field;
        }

        public override void RequestValues(Info serviceInfo, Service serviceData)
        {
            base.RequestValues(serviceInfo, serviceData);
            var info = (HistoryViewMaint_Info)serviceInfo;
            info.ObjectChanges.HistViewHistMainlineDtl = new HistViewHistMainlineDtlChanges_Info();
            info.ObjectChanges.HistViewHistMainlineDtl.UIFields = new Info(true);
            info.ObjectChanges.HistViewDtls = new HistViewDtlChanges_Info();
            info.ObjectChanges.HistViewDtls.UIFields = new Info(true);
            info.ObjectChanges.HistViewDtls.Name = new Info(true);
            info.ObjectToChange = new Info(true);
        }
        /// <summary>
        /// Fetch all the fields of a Transaction/CDO
        /// </summary>
        /// <param name="transactionName"></param>
        /// <returns>Array of fields</returns>
        /// 
        public string GetCdoDefId(string transactionName)
        {
            CDOInquiry cdoInquiry = new CDOInquiry
            {
                CDODefName = new Enumeration<MaintainableObjectEnum, string>(transactionName)
            };
            var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var cdoServ = new CDOInquiryService(session.CurrentUserProfile);
            var request = new CDOInquiry_Request
            {
                Info = new CDOInquiry_Info
                {
                    CDODefId = new Info(true)
                }
            };
            CDOInquiry_Result result = null;
            ResultStatus resultStatus = cdoServ.GetReferences(
                cdoInquiry,
                request,
                out result);
            if (resultStatus.IsSuccess)
            {
                return result.Value.CDODefId.Value.ToString();
            }
            throw new Exception("Unable to fetch available fields, please try again!");
        }

        public Primitive<string>[] GetAllFieldsForCdo(string cdodefId)
        {
            var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);

            V4_CDOInquiryService cdoInqServ = new V4_CDOInquiryService(session.CurrentUserProfile);
            V4_CDOInquiry v4_CDOInquiry = new V4_CDOInquiry();
            v4_CDOInquiry.CDODefID = Int32.Parse(cdodefId);
            V4_CDOInquiry_Request req = new V4_CDOInquiry_Request
            {
                Info = new V4_CDOInquiry_Info
                {
                    NameList = new Info(true)
                }
            };
            V4_CDOInquiry_Result res = null;
            var resultStatus = cdoInqServ.CDOInquiry_GetFields(v4_CDOInquiry, req, out res);
            if (resultStatus.IsSuccess)
            {
                return res.Value.NameList;
            }

            throw new Exception("Unable to fetch available fields, please try again!");
        }

        public Primitive<string>[] GetAllCdosForCdo(string cdodefId)
        {
            var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);

            V4_CDOInquiryService cdoInqServ = new V4_CDOInquiryService(session.CurrentUserProfile);
            V4_CDOInquiry v4_CDOInquiry = new V4_CDOInquiry();
            v4_CDOInquiry.CDODefID = Int32.Parse(cdodefId);
            //v4_CDOInquiry.CDOTypeName = ;
            V4_CDOInquiry_Request req = new V4_CDOInquiry_Request
            {
                Info = new V4_CDOInquiry_Info
                {
                    NameList = new Info(true),
                    CDODefID = new Info(true),
                    
                }
            };
            V4_CDOInquiry_Result res = null;
            var resultStatus = cdoInqServ.CDOInquiry_GetCDOs(v4_CDOInquiry, req, out res);
            if (resultStatus.IsSuccess)
            {
                return res.Value.NameList;
            }

            throw new Exception("Unable to fetch available fields, please try again!");
        }

        /// <summary>
        /// Converts array of primitive strings to array of AuditTrailConfigurationRow which can be mapped to grid
        /// </summary>
        /// <param name="fields"></param>
        /// <returns>Array of AuditTrailConfigurationRow which can be mapped to grid</returns>
        public AuditTrailConfigurationRow[] GetAuditTrailConfigurationRows(Primitive<string>[] fields)
        {
            List<AuditTrailConfigurationRow> rows = new List<AuditTrailConfigurationRow>();

            for (int i = 0; i < fields.Length; i++)
            {
                Primitive<string> field = fields[i];
                rows.Add(new AuditTrailConfigurationRow(field.Value, i));
            }

            return rows.ToArray();
        }
        /// <summary>
        /// Return array of fields which can be added in HistoryMainlineGridAvailableFields grid
        /// </summary>
        /// <param name="allFields"></param>
        /// <param name="selectedFields"></param>
        /// <returns>Array of fields which can be added in HistoryMainlineGridAvailableFields grid</returns>
        public Primitive<string>[] GetAvailableFields(string transactionName, Primitive<string>[] selectedFields)
        {
            var allFields = GetAllFieldsForCdo(GetCdoDefId(transactionName));
            List<Primitive<string>> availableFields = new List<Primitive<string>>();

            foreach (string field in allFields)
            {
                if (IsFieldAvailable(selectedFields, field))
                {
                    availableFields.Add(field);
                }
            }

            return availableFields.ToArray();
        }
        public override void DisplayValues(Service serviceData)
        {
            base.DisplayValues(serviceData);
            Primitive<string>[] selectedHistoryMainlineValues = { "TxnDate" };
            Primitive<string>[] selectedHistoryDetailsTransactionValues = { "DisplayName" };
            bool isEditAction = ((Camstar.WebPortal.PortalFramework.MaintenanceBehaviorContext)Page.PortalContext).State != MaintenanceBehaviorContext.MaintenanceState.New;
            if (isEditAction)
            {
                selectedHistoryMainlineValues = ((HistoryViewMaint)serviceData).ObjectChanges.HistViewHistMainlineDtl.UIFields;
            }
            else
            {
                ((HistoryViewMaint)serviceData).ObjectChanges.HistViewDtls = new HistViewDtlChanges[] {
                    GetHistViewDtlChanges("HistoryDetails", selectedHistoryDetailsTransactionValues)
                };
            }
            var transactions = ConvertFieldsToDictionary(((HistoryViewMaint)serviceData).ObjectChanges.HistViewDtls);
            Page.DataContract.SetValueByName("changedTransactionDetails", transactions);
            if (isEditAction)
            {
                Page.DataContract.SetValueByName("transactionDetails", ConvertFieldsToDictionary(((HistoryViewMaint)serviceData).ObjectChanges.HistViewDtls));
            }
            else
            {
                Page.DataContract.SetValueByName("transactionDetails", new Dictionary<string, TransactionDetail>());
            }
            AuditTrailConfigurationRow[] selectedFieldRows = GetAuditTrailConfigurationRows(selectedHistoryMainlineValues);
            HistoryMainlineGridSelectedFields.Data = selectedFieldRows;
            Page.Session["HistoryMainlineSelectedFields"] = selectedFieldRows.ToList();

            if (Page.Session["TreeDefinitionData"] == null)
            {
                Page.Session["TreeDefinitionData"] = GenerateCollectionDefinitionJsonDocument();
            }

            if (Page.Session["TreeDataData"] == null)
            {
                Page.Session["TreeDataData"] = GenerateJSONDataDocument();
            }

            TreeDefinition.Data = Page.Session["TreeDefinitionData"];
            TreeData.Data = Page.Session["TreeDataData"];

            Dictionary<string, string> labelsForClient = GetClientLabels();
            string jsonTranslationDoc = JsonConvert.SerializeObject(labelsForClient, Formatting.Indented);

            ScriptManager.RegisterStartupScript(this, this.GetType(), "initializeFactoryHierarchy",
                                               $"factoryHierarchy.initialize(" + jsonTranslationDoc + ");", true);

            Page.DataContract.SetValueByName("SelectedTransaction", string.Empty);

            try
            {
                HistoryMainlineGridAvailableFields.Data = SortRows(GetAuditTrailConfigurationRows(GetAvailableFields(HISTORY_MAINLINE_TRANSACTION, selectedHistoryMainlineValues)));
            }
            catch (Exception exception)
            {
                Page.DisplayMessage(exception.Message, false);
            }
        }
        /// <summary>
        /// Check whether value is present in selectedFields or list of predefined values
        /// { "BaseTxnType","CDOTypeId","ChangeCount","InstanceID","ReversalStatus","StepEntryTxnId","TxnId","TxnType"}
        /// </summary>
        /// <param name="selectedValues"></param>
        /// <param name="value"></param>
        /// <returns>Is value present in selectedFields or list of predefined values</returns>
        public bool IsFieldAvailable(Primitive<string>[] selectedFields, string value)
        {
            List<string> fieldsToIgnore = new List<string>()
            {
                "BaseTxnType",
                "CDOTypeId",
                "ChangeCount",
                "InstanceID",
                "ReversalStatus",
                "StepEntryTxnId",
                "TxnId",
                "TxnType"
            };
            bool isAvailable = true;
            foreach (string selectedField in selectedFields)
            {
                if (selectedField.Equals(value))
                {
                    isAvailable = false;
                    break;
                }
            }
            if (fieldsToIgnore.Contains(value))
            {
                isAvailable = false;
            }
            return isAvailable;
        }
        /// <summary>
        /// Sort array of AuditTrailConfigurationRow by it's ColumnName property in ascending order
        /// </summary>
        /// <param name="rows"></param>
        /// <returns>Sorted array of AuditTrailConfigurationRow in ascending order</returns>
        private AuditTrailConfigurationRow[] SortRows(AuditTrailConfigurationRow[] rows)
        {
            if (rows.Length > 1)
            {
                Array.Sort(rows, delegate (AuditTrailConfigurationRow row1, AuditTrailConfigurationRow row2)
                {
                    return row1.ColumnName.CompareTo(row2.ColumnName);
                });
            }
            return rows;
        }

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
        }

        protected Dictionary<string, string> GetClientLabels()
        {
            var labels = new Dictionary<string, string>()
            {
                { "AddButton", GetLocalizedLabel("AddButton", "Add") },
                { "ExpandButton", GetLocalizedLabel("Lbl_ExpandAll", "Expand") },
                { "CollapseButton", GetLocalizedLabel("Lbl_CollapseAll", "Collapse") },
                { "Search", GetLocalizedLabel("Search", "Search...") },
                { "SearchButton", GetLocalizedLabel("SearchButton", "Search") },

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
            yield return new ScriptReference("~/Scripts/AuditTrailTransactionHirearchy.js");

        }

        protected string GetLocalizedLabel(string labelId, string alternateText)
        {
            string text = CachedLabels != null ? CachedLabels.GetLabelByName(labelId).Value : string.Empty;

            if (string.IsNullOrEmpty(text))
                text = alternateText;

            return text;
        }

        protected virtual void OnGenerateCollectionDefinitionJsonDocument(Dictionary<string, dynamic> definitions) { }

        /// <summary>
        /// Generate a tree definition JSON document to send to the client. The Javascript functions will
        /// use this data to define how nodes in the FHM tree should be created. The reason why we have this
        /// JSON document versus it being hard coded in the Javascript as this methodology gives us the ability 
        /// to create new definitions in higher workspaces.
        /// </summary>
        /// <returns>A string representation of the definition data in JSON format</returns>
        string GenerateCollectionDefinitionJsonDocument()
        {
            // Get label values for localization
            string enterpriseTreeNodeTitle = GetLocalizedLabel("CSICDOName_Enterprise", "Enterprise");
            string enterpriseCDOTitle = GetLocalizedLabel("CSICDOName_Enterprise", "Enterprise");
            string factoryTreeNodeTitle = GetLocalizedLabel("CSICDOName_Factory", "Factory");
            string factoryCDOTitle = GetLocalizedLabel("CSICDOName_Factory", "Factory");
            string areaTreeNodeTitle = GetLocalizedLabel("FactoryLevelEnum_Area", "Area");
            string cellTreeNodeTitle = GetLocalizedLabel("FactoryLevelEnum_Cell", "Cell");
            string equipmentTreeNodeTitle = GetLocalizedLabel("FactoryLevelEnum_Equipment", "Equipment");
            string resourceCDOTitle = GetLocalizedLabel("CSICDOName_Resource", "Resource");

            Dictionary<string, dynamic> definitions = new Dictionary<string, dynamic>();

            //Create new collection definition objects and add them to the definition's dictionary
            var enterpriseGetOperation = CreateCustomDataGetOperation("enterpriseId", "GetEnterprise", "enterprise");
            var enterpriseDataType = CreateDataType("Enterprise", enterpriseTreeNodeTitle, "");
            var enterpriseCustomData = CreateCollectionDefinitionCustomData(5590, 1240, "Enterprise", enterpriseCDOTitle, "EnterpriseMaint_VP", enterpriseGetOperation);
            var enterprise = CreateCollectionDefinition("", "ATCtree-enterprise", "", "", enterpriseDataType, _factoryCollectionKey, enterpriseCustomData);
            definitions.Add(_enterpriseCollectionKey, enterprise);

            var factoryGetOperation = CreateCustomDataGetOperation("factoryId", "GetFactory", "factory");
            var factoryDataType = CreateDataType("Factory", factoryTreeNodeTitle, "EnterpriseId");
            var factoryCustomData = CreateCollectionDefinitionCustomData(5610, 1250, "Factory", factoryCDOTitle, "FactoryMaint_VP", factoryGetOperation);
            var factory = CreateCollectionDefinition("", "ATCtree-factory", "", "", factoryDataType, _areaCollectionKey, factoryCustomData);
            definitions.Add(_factoryCollectionKey, factory);

            var resourceGetOperation = CreateCustomDataGetOperation("resourceId", "GetResource", "resource");
            var areaDataType = CreateDataType("Area", areaTreeNodeTitle, "FactoryId", "FactoryLevel");
            var resourceCustomData = CreateCollectionDefinitionCustomData(3970, 3970, "Resource", resourceCDOTitle, "Resource_VP", resourceGetOperation);
            var area = CreateCollectionDefinition("", "ATCtree-area", "", "", areaDataType, _cellCollectionKey, resourceCustomData);
            definitions.Add(_areaCollectionKey, area);

            var cellDataType = CreateDataType("Cell", cellTreeNodeTitle, "ParentResourceId", "FactoryLevel");
            var cell = CreateCollectionDefinition("", "ATCtree-cell", "", "", cellDataType, _equipmentCollectionKey, resourceCustomData);
            definitions.Add(_cellCollectionKey, cell);

            var equipmentDataType = CreateDataType("Equipment", equipmentTreeNodeTitle, "ParentResourceId", "FactoryLevel");
            var equipment = CreateCollectionDefinition("", "ATCtree-equipment", "", "", equipmentDataType, "", resourceCustomData);
            definitions.Add(_equipmentCollectionKey, equipment);

            OnGenerateCollectionDefinitionJsonDocument(definitions);

            string jsonDoc = JsonConvert.SerializeObject(definitions, Formatting.Indented);
            return jsonDoc;
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
            dynamic getOperation)
        {

            dynamic customData = new ExpandoObject();
            customData.CDOName = cdoName;
            customData.CDOTitle = cdoTitle;
            customData.CDODefID = cdoDefID;
            customData.ServiceName = $"{cdoName}Maint";
            customData.MaintTypeID = maintTypeID;
            customData.ModelingPage = modelingPage;
            customData.GetOperation = getOperation;

            return customData;
        }

        /// <summary>
        /// Generate the JSON document containing the Factory Hierarchical Model's data. This method calls the 
        /// WCF service, then calls functions to create the serialzed data
        /// </summary>
        /// <returns>The FHM data document in strng format</returns>
        private string GenerateJSONDataDocument()
        {
            return CreateJSONFromServiceResults();
        }

        private dynamic CreateTreeDataStructure()
        {
            dynamic enterpriseNodeData;
            enterpriseNodeData = new ExpandoObject();
            enterpriseNodeData.Name = "HistoryDetails";
            var transactionDetails = GetTrasanctions("HistoryDetails", 1);
            enterpriseNodeData.ID = transactionDetails.Item1;
            enterpriseNodeData.Factories = transactionDetails.Item2;

            dynamic nodeData;
            nodeData = new ExpandoObject();
            nodeData.Enterprises = new List<dynamic> { enterpriseNodeData };
            return nodeData;
        }

        /// <summary>
        /// After the data from WCF has been used to populate data classes, this function will serialize the data into
        /// JSON format
        /// </summary>
        /// <param name="enterpriseInquiryResults">The results data from the WCF service call</param>
        /// <returns>The string value of the JSON data from serialization</returns>
        private string CreateJSONFromServiceResults()
        {
            // Create a root element and then populate it with children
            dynamic tree = CreateTreeDataStructure();

            // Serialize the data for the tree
            string json = JsonConvert.SerializeObject(tree, Formatting.Indented, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            return json;
        }

        /// <summary>
        /// Fill a data structure with Enterprise data to be serialized to JSON and sent to the client.
        /// </summary>
        /// <param name="enterpriseInquiryResults">Data from the WCF call</param>
        /// <returns>An list Enterprise data for serialization</returns>
        public Tuple<string, List<dynamic>> GetTrasanctions(string transactionName, int depth)
        {
            depth++;
            List<dynamic> transactionNames = new List<dynamic>();
            string cdoDefId = (GetCdoDefId(transactionName));
            var allCdos = GetAllCdosForCdo(cdoDefId);

            if (allCdos != null && allCdos.Length >= 1)
            {
                foreach (var item in allCdos)
                {
                    dynamic nodeData = new ExpandoObject();
                    nodeData.Name = item.ToString();
                    var transactionDetails = GetTrasanctions(item.ToString(), depth);
                    nodeData.ID = transactionDetails.Item1;
                    var childNodes = transactionDetails.Item2;
                    if (childNodes.Count > 0)
                    {
                        switch (depth)
                        {
                            case 2:
                                nodeData.Areas = childNodes;
                                break;

                            case 3:
                                nodeData.Cells = childNodes;
                                break;

                            case 4:
                                nodeData.Equipment = childNodes;
                                break;
                            default:
                                break;
                        }
                    }
                    transactionNames.Add(nodeData);
                }
            }
            return new Tuple<string, List<dynamic>>(cdoDefId, transactionNames);
        }

        protected virtual void OnNewEquipmentResource(Resource equipmentInquiryResults, dynamic equipment) { }

        /// <summary>
        /// Fill a data structure with Equipment resource data to be serialized to JSON and sent to the client.
        /// </summary>
        /// <param name="enterpriseInquiryResults">Data from the WCF call</param>
        /// <returns>A list of Equipment data for serialization</returns>
        List<dynamic> GetEquipment(Resource[] equipmentInquiryResults)
        {
            List<dynamic> equipment = new List<dynamic>();

            foreach (Resource resource in equipmentInquiryResults)
            {
                dynamic nodeData = CreateResourceDataObject(resource);

                if (resource.FHMResolvedChildResources != null && resource.FHMResolvedChildResources.Length > 0)
                {
                    nodeData.Equipment = GetEquipment(resource.FHMResolvedChildResources);
                }

                OnNewEquipmentResource(resource, nodeData);

                equipment.Add(nodeData);
            }

            return equipment;
        }

        dynamic CreateResourceDataObject(Resource resource)
        {
            dynamic nodeData = new ExpandoObject();
            nodeData.Name = resource.Name.ToString();
            nodeData.ID = resource.Self.ID;
            nodeData.Nickname = resource.NickName != null ? resource.NickName.ToString() : "";
            nodeData.FactoryLevel = resource.FactoryLevel != null ? resource.FactoryLevel.Value : 0;

            return nodeData;
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
                new Label("Search")
            });

            OnGetLabels(labels);

            // load them to cache
            CachedLabels = FrameworkManagerUtil.GetLabelCache(Page.Session);
        }       
        #endregion
    }

    public class AuditTrailConfigurationRow: DCObject
    {
        public AuditTrailConfigurationRow(string columnName, int index)
        {
            ColumnName = columnName;
            Index = index;
        }
        public string ColumnName { get; set; }
 
        public int Index { get; set; }
    }

    public class TransactionDetail
    {
        public int? ListItemIndex { get; set; }
        public List<AuditTrailConfigurationRow> SelectedFields;
        public bool IsSelected { get; set; }
        public TransactionDetail(int? index, List<AuditTrailConfigurationRow> fields)
        {
            this.ListItemIndex = index;
            this.SelectedFields = fields;
            this.IsSelected = false;
        }
    }
}
