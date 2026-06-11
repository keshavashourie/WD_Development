// Copyright Siemens 2023
using System;
using System.Data;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.Utilities;
using System.Runtime.Serialization;
using System.Web;
using System.Linq;
using CI = Camstar.WebPortal.WebPortlets.ComponentIssue;
using OM = Camstar.WCF.ObjectStack;
using System.Collections.Generic;
using Camstar.WebPortal.FormsFramework;
using PERS = Camstar.WebPortal.Personalization;
using Camstar.WCF.ObjectStack;
using System.Web.UI;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    /// <summary>
    /// Represents an existing isCurrentDefects that is passed into the popup
    /// </summary>
    [DataContract]
    public class CurrentDefect
    {
        [DataMember]
        public string isCurrentDefectsIDString;
        [DataMember]
        public string isInspectNote;
        [DataMember]
        public string isRepairNotes;
        [DataMember]
        public string isContainerName;
        [DataMember]
        public string isX;
        [DataMember]
        public string isY;
        [DataMember]
        public string isRefDes;
        [DataMember]
        public string isProductName;
        [DataMember]
        public string isStatus;
        [DataMember]
        public string isDefectReasonName;
        [DataMember]
        public string isDefectReasonId;
        [DataMember]
        public string isLotNumber;
        [DataMember]
        public string isContainer;
        [DataMember]
        public string ContainerProductId;

        /// <summary>
        /// Parse the given JSON string and return list of CurrentDefect objects
        /// </summary>
        /// <param name="defectsJson"></param>
        /// <returns></returns>
        public static CurrentDefect[] DeserializeDefects(string defectsJson)
        {
            if (string.IsNullOrWhiteSpace(defectsJson) || string.IsNullOrEmpty(defectsJson))
            {
                return new CurrentDefect[0];
            }

            System.IO.MemoryStream existingDefectsStream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(defectsJson));
            var serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(CurrentDefect[]));
            return (CurrentDefect[])serializer.ReadObject(existingDefectsStream);
        }

        public static SubentityRef[] GetSubentityRefs(List<CurrentDefect> iscurrentdefects)
        {
            SubentityRef[] subentityRefs = new SubentityRef[iscurrentdefects.Count];

            for (int i = 0; i < iscurrentdefects.Count; i++)
            {
                subentityRefs[i] = new SubentityRef(iscurrentdefects[i].isCurrentDefectsIDString);
            }
            return subentityRefs;
        }
    }

    /// <summary>
    /// Represents an item that is defective.  Deserialized from a string sent as a data contract
    /// </summary>
    [DataContract]
    public class isDefectItem
    {
        private static LabelCache _labelCache;

        public static void SetLabelCache(LabelCache labelCache)
        {
            _labelCache = labelCache;
        }

        [DataMember(Name = "isRefDes")]
        public string isRefDes;
        [DataMember(Name = "isX")]
        public string isX;
        [DataMember(Name = "isY")]
        public string isY;
        [DataMember(Name = "containerName")]
        public string ContainerName;
        [DataMember(Name = "isContainer")]
        public string isContainer;
        [DataMember(Name = "isStatus")]
        public string isStatus;

        /// <summary>
        /// Convert the given list of defects into an array of DefectItems
        /// </summary>
        /// <param name="isCurrentDefects"></param>
        /// <returns></returns>
        public static isDefectItem[] GetDefectItemArray(List<CurrentDefect> isCurrentDefects)
        {
            List<isDefectItem> isDefectItems = new List<isDefectItem>(isCurrentDefects.Count());

            foreach (CurrentDefect defect in isCurrentDefects)
            {
                isDefectItems.Add(new isDefectItem()
                {
                    isRefDes = defect.isRefDes,
                    isX = defect.isX,
                    isY = defect.isY,
                    ContainerName = defect.isContainerName,
                });
            }

            return isDefectItems.ToArray();
        }
    }

    public class isLogDefectPopup : MatrixWebPart
    {
        #region Properties

        protected virtual CWC.ContainerList Container
        {
            get { return Page.FindCamstarControl("isDefect_Container") as CWC.ContainerList; }
        }

        protected virtual CWC.NamedObject DefectReason
        {
            get { return Page.FindCamstarControl("ServiceDetails_isDefectReason") as CWC.NamedObject; }
        }

        /// <summary>
        /// Passed in through DataContract, used as transaction parameter (NOT set on service details)
        /// </summary>
        protected virtual CWC.NamedObject Resource
        {
            get { return Page.FindCamstarControl("Resource") as CWC.NamedObject; }
        }
        
        protected virtual CWC.TextBox InspectNote
        {
            get { return Page.FindCamstarControl("ServiceDetails_isInspectNote") as CWC.TextBox; }
        }
        protected virtual CWC.TextBox isX
        {
            get { return Page.FindCamstarControl("ServiceDetails_isX") as CWC.TextBox;}
        }
        protected virtual CWC.TextBox isY
        {
            get { return Page.FindCamstarControl("ServiceDetails_isY") as CWC.TextBox; }
        }

        /// <summary>
        /// Defect item ref des and X/Y location
        /// </summary>
        protected virtual JQDataGrid DefectiveItems
        {
            get
            {
                return Page.FindCamstarControl("isDefect_ServiceDetails") as JQDataGrid;
            }
        }
        protected virtual JQDataGrid MaterialListItemsGrid { get { return Page.FindCamstarControl("MaterialListItemsGrid") as JQDataGrid; } }
        protected CWC.Button AddMaterialListItems { get { return Page.FindCamstarControl("AddMaterialListItems") as CWC.Button; } }

        protected virtual JQDataGrid _gridDefectForUpdateDetails
        {
            get { return Page.FindCamstarControl("isDefect_isCurrentDefectsToDelete") as JQDataGrid; }
        }

        protected virtual CWC.TextBox ExistingCurrentDefectsJson
        {
            get { return Page.FindCamstarControl("isDefect_ExistingCurrentDefectsJson") as CWC.TextBox; }
        }

        protected virtual JQDataGrid DocumentsGrid
        {
            get { return Page.FindCamstarControl("Defect_isAttachDocumentDetails") as JQDataGrid; }
        }

        #endregion

        protected override void OnLoad(System.EventArgs e)
        {
            base.OnLoad(e);

            if (!Page.IsPostBack)
            {
                FieldPropertiesHandling();

                if (Container.Data != null)
                {
                    //List<OM.MaterialListItem> listItems;// = new List<OM.MaterialListItem>();
                    LoadMaterialListItems(Container.Data.ToString(), out List<OM.MaterialListItem> listItems);
                    MaterialListItemsGrid.Data = listItems.ToArray();
                }

                ScriptManager.RegisterStartupScript(this, this.GetType(), "initializeisLogDefectPopup", $"isLogDefectPopup.initialize();", true);
            }

            AddMaterialListItems.Click += AddMaterialListItems_Click;

            if (ExistingCurrentDefectsJson.Data != null)
            {
                // we are given no defective items.  See if we have any existing defects we are modifying
                CurrentDefect[] existingDefects = CurrentDefect.DeserializeDefects((string)ExistingCurrentDefectsJson.Data);

                //Populate Json Data into defects grid
                PopulateDefectsGrid(existingDefects);

                //Update defect grid to read-only
                UpdateDefectiveItemsGrid();

                if (!Page.IsPostBack)
                {
                    // Get inspect note if we are updating a single defect
                    if (existingDefects.Length == 1)
                    {
                        InspectNote.Data = existingDefects[0].isInspectNote;
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override IEnumerable<ScriptReference> GetScriptReferences()
        {
            yield return new ScriptReference("~/Scripts/User/Industry Solutions/isLogDefectPopup.js");
        }

        /// <summary>
        /// create entries in the DefectiveItems grid for each selected item in the Material Items List
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddMaterialListItems_Click(object sender, EventArgs e)
        {
            if (MaterialListItemsGrid.SelectedRowCount == 0)
                return;

            List<OM.isDefectDetail> newDetails = new List<isDefectDetail>();

            // Build list of defect details to add
            foreach (OM.MaterialListItem matListItem in MaterialListItemsGrid.GridContext.GetSelectedItems(true))
            {
                newDetails.Add(CreateDefectDetail(matListItem));
            }

            // set list or append to existing
            if(DefectiveItems.Data == null)
                DefectiveItems.Data = newDetails.ToArray();
            else
            {
                DefectiveItems.Data = (DefectiveItems.Data as OM.isDefectDetail[]).Concat<isDefectDetail>(newDetails).ToArray();
            }
        }

        protected OM.isDefectDetail CreateDefectDetail(OM.MaterialListItem matListItem)
        {
            OM.isDefectDetail detail = new isDefectDetail()
            { 
                isRefDes = matListItem.ReferenceDesignator,
                isProduct = matListItem.Product
            };

            return detail;
        }

        /// <summary>
        /// 
        /// </summary>
        protected ResultStatus LoadMaterialListItems(string containerName, out List<OM.MaterialListItem> listItems)
        {
            listItems = new List<MaterialListItem>();

            // prepare service call
            var request = new WCF.Services.ComponentIssueR2_Request()
            {
                Info = new OM.ComponentIssueR2_Info
                {
                    RequestValue = true,
                    ServiceDetails = new OM.IssueDetails_Info() { RequestSelectionValues = true }
                }
            };

            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var compIssueService = new WCF.Services.ComponentIssueR2Service(session.CurrentUserProfile);
            var result = new WCF.Services.ComponentIssueR2_Result();
            var serviceParams = new OM.ComponentIssueR2()
            {
                Container = new OM.ContainerRef(containerName)
            };

            // Load Material List Items (Selection Values of the ServiceDetails)
            ResultStatus resultStatus = compIssueService.Load(serviceParams, request, out result);

            if (resultStatus != null && resultStatus.IsSuccess && result.Environment.ServiceDetails.SelectionValues != null)
            {

                // SelectionValues are the untyped result of a query - must convert to typed objects
                OM.Header[] headers = result.Environment.ServiceDetails.SelectionValues.Headers;
                OM.Row[] rows = result.Environment.ServiceDetails.SelectionValues.Rows;

                if (headers != null && rows != null)
                {
                    Dictionary<CI.MaterialListItem.HeaderEnum, int> columnIndexes = CI.MaterialListItem.GetColumnIndexes(headers);

                    foreach (Row row in rows)
                    {
                        // TODO - is Name garaunteed to be unique?  MaterialListItem object does not have an ID field
                        string matListItemName = Convert.ToString(row.Values[columnIndexes[CI.MaterialListItem.HeaderEnum.MaterialListItemNameIndex]]);
                        if (!listItems.Any(li => li.Name == matListItemName)) // Make sure the list item has not already been added
                        {
                            if (string.IsNullOrEmpty(CI.MaterialListItem.GetRowValue<string>(row, columnIndexes[CI.MaterialListItem.HeaderEnum.PhantomBillIdIndex]))) // Not a phantom bill.
                            {
                                OM.MaterialListItem listItem = CreateMaterialListItem(row, columnIndexes);

                                if (listItem != null)
                                    listItems.Add(listItem);
                            }
                        }
                    }
                }
            }

            return resultStatus;
        }

        /// <summary>
        /// Use the given dictionary to convert the given row of data into a MaterialListItem object
        /// </summary>
        /// <param name="row"></param>
        /// <param name="columnIndexes"></param>
        /// <returns></returns>
        protected OM.MaterialListItem CreateMaterialListItem(OM.Row row, Dictionary<CI.MaterialListItem.HeaderEnum, int> columnIndexes)
        {
            MaterialListItem listItem = new MaterialListItem();

            OM.Enumeration<OM.IssueControlEnum, int> issueControl = (OM.Enumeration<OM.IssueControlEnum, int>)CI.MaterialListItem.GetRowValue<OM.IssueControlEnum>(row, columnIndexes[CI.MaterialListItem.HeaderEnum.IssueControlIndex], (x) => Convert.ToInt32(x));

            string productName = CI.MaterialListItem.GetRowValue<string>(row, columnIndexes[CI.MaterialListItem.HeaderEnum.ProductIndex]);
            string productRevision = CI.MaterialListItem.GetRowValue<string>(row, columnIndexes[CI.MaterialListItem.HeaderEnum.ProductRevisionIndex]);
            if (!string.IsNullOrEmpty(productName) && !string.IsNullOrEmpty(productRevision))
                listItem.Product = new OM.RevisionedObjectRef(productName, productRevision, "Product");

            listItem.ReferenceDesignator = CI.MaterialListItem.GetRowValue<string>(row, columnIndexes[CI.MaterialListItem.HeaderEnum.ReferenceDesignatorIndex]);

            return listItem;
        }


        protected void UpdateDefectiveItemsGrid()
        {
            (DefectiveItems.Settings as PERS.GridDataSettingsItemList).EditorSettings.EditingMode = PERS.JQEditingModes.Disabled;
            (DefectiveItems.Settings as PERS.GridDataSettingsItemList).Navigator = PERS.JQGridNavigatorMode.Disabled;
            (DefectiveItems.Settings as PERS.GridDataSettingsItemList).SelectionMode = PERS.JQGridSelectionMode.Disable;
            DefectiveItems.ApplyFieldPersonalization();
        }

        protected void PopulateDefectsGrid(CurrentDefect[] existingDefects)
        {
            DataTable defectiveItemDataTable = new DataTable();

            DataColumn refDesColumn = new DataColumn("isRefDes", typeof(string));
            defectiveItemDataTable.Columns.Add(refDesColumn);

            DataColumn xColumn = new DataColumn("isX", typeof(string));
            defectiveItemDataTable.Columns.Add(xColumn);

            DataColumn yColumn = new DataColumn("isY", typeof(string));
            defectiveItemDataTable.Columns.Add(yColumn);

            isDefectItem[] gridItems = isDefectItem.GetDefectItemArray(new List<CurrentDefect>(existingDefects));

            if (gridItems != null)
            {
                // populate items into grid
                foreach (isDefectItem defectitem in gridItems)
                {
                    if (!string.IsNullOrEmpty(defectitem.isRefDes ?? defectitem.isX ?? defectitem.isY))
                    {
                        DataRow defectiveItemRow = defectiveItemDataTable.NewRow();
                        defectiveItemRow[refDesColumn.ColumnName] = defectitem.isRefDes;
                        defectiveItemRow[xColumn.ColumnName] = defectitem.isX;
                        defectiveItemRow[yColumn.ColumnName] = defectitem.isY;
                        defectiveItemDataTable.Rows.Add(defectiveItemRow);
                    }
                }

                JQDataGrid defectiveItemDetailsGrid = DefectiveItems;
                WebClientPortal.GridUtility.ItemListGrid_BindDataTable(this, defectiveItemDataTable, ref defectiveItemDetailsGrid);

                CamstarWebControl.SetRenderToClient(defectiveItemDetailsGrid);
            }
        }

        public void FieldPropertiesHandling()
        {
            if (_gridDefectForUpdateDetails.Data != null)
                if (_gridDefectForUpdateDetails.TotalRowCount > 1)
                {
                    DefectReason.Data = null;
                    InspectNote.Data = null;
                }
        }

        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as PERS.CustomAction;
            if (action != null)
            {
                switch (action.Parameters)
                {
                    case "AddOpen":
                        {
                            // Page.CloseFloatingFrameOnSubmit(new OM.ResultStatus());

                            e.Result = AddOpenDefect();

                            DisplayMessage(e.Result);
                            break;
                        }
                    case "AddUpdate":
                        {
                            e.Result = AddUpdateDefect();

                            DisplayMessage(e.Result);
                            break;
                        }
                }
                Page.CloseFloatingFrame(true);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected virtual OM.ResultStatus AddOpenDefect()
        {
            if (DefectReason.Data == null)
                return new OM.ResultStatus("Defect Reason is required.", false);
            
            var serviceParams = new OM.isDefect() 
            {
                Container = Container.Data as OM.ContainerRef,
                isTxnExecute = "AddOpen"
            };
            if (Resource.Data != null)
                serviceParams.Resource = Resource.Data as NamedObjectRef;

            SetDocsToAttach(serviceParams);

            int defectiveItemsCount = DefectiveItems.TotalRowCount;

            // set service details (list of defects to log) based on defects in grid
            if (defectiveItemsCount > 0)
            {
                List<OM.isDefectDetail> newDefects = new List<isDefectDetail>();

                foreach (OM.isDefectDetail defectiveItem in DefectiveItems.Data as OM.isDefectDetail[])
                {
                    OM.isDefectDetail newDefect = new OM.isDefectDetail()
                    {
                        isDefectReason = DefectReason.Data as OM.NamedObjectRef,
                        Container = Container.Data as OM.ContainerRef,
                        isProduct = defectiveItem.isProduct,                           // TODO - there is no Product column in this grid.  Should we be getting this from somewhere else???
                        isRefDes = defectiveItem.isRefDes
                    };

                    if (InspectNote.Data != null)
                        newDefect.isInspectNote = InspectNote.Data.ToString();

                    // default X and Y to hidden TextBox value, override with grid value (?)
                    if (isX.Data != null)
                        newDefect.isX = int.Parse(isX.Data.ToString());
                    if (defectiveItem.isX != null)
                        newDefect.isX = defectiveItem.isX;

                    if (isY.Data != null)
                        newDefect.isY = int.Parse(isY.Data.ToString());
                    if (defectiveItem.isY != null)
                        newDefect.isY = defectiveItem.isY;
                    
                    newDefects.Add(newDefect);
                }

                serviceParams.ServiceDetails = newDefects.ToArray();
            }
            else
            {
                serviceParams.ServiceDetails = new OM.isDefectDetail[1];
                serviceParams.ServiceDetails[0] = new OM.isDefectDetail();
                serviceParams.ServiceDetails[0].Container = Container.Data as OM.ContainerRef;
                serviceParams.ServiceDetails[0].isDefectReason = DefectReason.Data as OM.NamedObjectRef;
                if (InspectNote.Data != null)
                    serviceParams.ServiceDetails[0].isInspectNote = InspectNote.Data.ToString();
            }

            var request = new Camstar.WCF.Services.isDefect_Request();
            request.Info = new OM.isDefect_Info();

            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var isDefectSvc = new Camstar.WCF.Services.isDefectService(session.CurrentUserProfile);
            var result = new Camstar.WCF.Services.isDefect_Result();
            
            OM.ResultStatus resultStatus = isDefectSvc.ExecuteTransaction(serviceParams, request, out result);

            return resultStatus;
        }

        protected virtual void SetDocsToAttach(OM.Defect serviceData)
        {
            //TODO: first implementation only allows attaching docs to a defect.
            //      no detach or mgt
            //      if we support that, then this needs to distinguish docs to attach from all docs in grid.
            JQDataGrid docsGrid = DocumentsGrid;
            if (docsGrid != null && docsGrid.TotalRowCount > 0)
            {
                serviceData.AttachDocumentDetails = docsGrid.Data as OM.AttachDocumentDetails[];
            }
        }

        protected virtual OM.ResultStatus AddUpdateDefect()
        {
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = new Camstar.WCF.Services.isDefectUpdateService(session.CurrentUserProfile);
            var servicedata = new OM.isDefectUpdate();
            var serviceinfo = new OM.isDefectUpdate_Info();
            var result = new Camstar.WCF.Services.isDefectUpdate_Result();

            if (DefectReason.Data == null)
                return new OM.ResultStatus("Defect Reason is required.", false);

            servicedata.Container = Container.Data as OM.ContainerRef;
            servicedata.isTransactionType = "Update";

            int i = 0;
            CurrentDefect[] existingDefects = CurrentDefect.DeserializeDefects((string)ExistingCurrentDefectsJson.Data);
            isDefectItem[] gridItems = isDefectItem.GetDefectItemArray(new List<CurrentDefect>(existingDefects));
            OM.isDefectDetail[] DefectServiceDetails = new OM.isDefectDetail[gridItems.Count()];

            foreach (isDefectItem resultDetaillist in gridItems)
            {
                DefectServiceDetails[i] = new OM.isDefectDetail();
                {
                    DefectServiceDetails[i].isDefectReason = DefectReason.Data as OM.NamedObjectRef;
                    DefectServiceDetails[i].Container = Container.Data as OM.ContainerRef;

                    if (!string.IsNullOrEmpty(resultDetaillist.isX))
                        DefectServiceDetails[i].isX = int.Parse(resultDetaillist.isX.ToString());
                    if (!string.IsNullOrEmpty(resultDetaillist.isY))
                        DefectServiceDetails[i].isY = int.Parse(resultDetaillist.isY.ToString());
                    if (InspectNote.Data != null)
                        DefectServiceDetails[i].isInspectNote = InspectNote.Data.ToString();
                }

                DefectServiceDetails[i].isStatus = isDefectStatusEnum.isOpen;
                DefectServiceDetails[i].isRefDes = resultDetaillist.isRefDes;

                i++;
            }

            i = 0;
            servicedata.ServiceDetails = new OM.isDefectDetail[gridItems.Count()];
            foreach (OM.isDefectDetail resultDetaillist in DefectServiceDetails)
            {
                servicedata.ServiceDetails[i] = new OM.isDefectDetail();
                {
                    servicedata.ServiceDetails[i].Container = resultDetaillist.Container;
                    servicedata.ServiceDetails[i].isDefectReason = resultDetaillist.isDefectReason;

                    servicedata.ServiceDetails[i].isX = resultDetaillist.isX;
                    servicedata.ServiceDetails[i].isY = resultDetaillist.isY;

                    servicedata.ServiceDetails[i].isProduct = resultDetaillist.isProduct;
                    servicedata.ServiceDetails[i].isRefDes = resultDetaillist.isRefDes;
                    servicedata.ServiceDetails[i].isInspectNote = resultDetaillist.isInspectNote;
                    servicedata.ServiceDetails[i].isStatus = resultDetaillist.isStatus;
                }
                i++;
            }

            servicedata.isCurrentDefectsToDelete = CurrentDefect.GetSubentityRefs(new List<CurrentDefect>(existingDefects));

            var iServiceRequest = new Camstar.WCF.Services.isDefectUpdate_Request();
            iServiceRequest.Info = serviceinfo;

            var resultStatus = new OM.ResultStatus();

            OM.ResultStatus iResultStatus = service.ExecuteTransaction(servicedata, iServiceRequest, out result);

            return iResultStatus;
        }
    }
}
