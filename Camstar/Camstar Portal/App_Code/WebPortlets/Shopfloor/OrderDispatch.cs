// Copyright Siemens 2023  
using System.Collections.Generic;
using System.Data;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.Personalization;
using OM = Camstar.WCF.ObjectStack;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using System.Web;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class OrderDispatch : MatrixWebPart
    {
        #region Controls

        protected virtual CWC.FileInput UploadField
        {
            get { return Page.FindCamstarControl("DocumentPath") as CWC.FileInput; }
        } // UploadField 

        protected virtual CWC.TextBox DocRevision
        {
            get { return Page.FindCamstarControl("AttachDocument_DocumentRevision") as CWC.TextBox; }
        } // DocRevision

        protected virtual CWC.TextBox DocName
        {
            get { return Page.FindCamstarControl("AttachDocument_DocumentName") as CWC.TextBox; }
        } // DocName

        protected virtual CWC.TextBox DocDescription
        {
            get { return Page.FindCamstarControl("AttachDocument_DocumentDescription") as CWC.TextBox; }
        } // DocDescription

        protected virtual CWC.RadioButton RadioBtn_NewDocNOReuse
        {
            get { return Page.FindCamstarControl("RadioBtn_AttachDocument_NewDocumentNOReuse") as CWC.RadioButton; }
        } // RadioBtn_NewDocNOReuse

        protected virtual CWC.RadioButton RadioBtn_NewDocReuse
        {
            get { return Page.FindCamstarControl("RadioBtn_AttachDocument_NewDocumentReuse") as CWC.RadioButton; }
        } // RadioBtn_NewDocReuse

        protected virtual CWC.RadioButton RadioBtn_ExistingDoc
        {
            get { return Page.FindCamstarControl("RadioBtn_AttachDocument_Existing") as CWC.RadioButton; }
        } // RadioBtn_ExistingDoc

        protected virtual CWC.DropDownList AttachmentType
        {
            get { return Page.FindCamstarControl("AttachDocument_AttachmentType") as CWC.DropDownList; }
        } // AttachmentType

        protected virtual CWC.TextBox StoredFileNameField
        {
            get { return Page.FindCamstarControl("AttachDocument_AttachedFileName") as CWC.TextBox; }
        } // StoredFileNameField

        protected virtual CWC.CheckBox IsContainerField
        {
            get { return Page.FindCamstarControl("AttachDocument_IsContainer") as CWC.CheckBox; }
        } // IsContainerField


        protected virtual CWC.RevisionedObject DocumentInstanceField
        {
            get { return Page.FindCamstarControl("AttachDocument_DocumentInstance") as CWC.RevisionedObject; }
        } // DocumentInstanceField

        protected virtual CWC.Button ViewDocumentButton
        {
            get { return Page.FindCamstarControl("ViewDocumentButton") as CWC.Button; }
        } //DocumentActionsButton

        protected virtual CWC.NamedObject NumberingRule
        {
            get { return Page.FindCamstarControl("Details_AutoNumberRule") as CWC.NamedObject; }
        }

        protected virtual CWC.CheckBox AutoNumber
        {
            get { return Page.FindCamstarControl("Details_AutoNumber") as CWC.CheckBox; }
        }

        #endregion

        protected override void OnLoad(System.EventArgs e)
        {
            base.OnLoad(e);
            if (Page.DataContract.GetValueByName(mkRowSelectionChangedDM) != null)
            {
                SetSelectionValuesToControls();
                Page.DataContract.SetValueByName(mkRowSelectionChangedDM, null);
            }
            else if (Page.DataContract.GetValueByName(mkSelectedMfgRowDM) != null && Page.DataContract.GetValueByName(mkRowsCountDM) != null)
            {
                string selectedRowId = Page.DataContract.GetValueByName<string>(mkSelectedMfgRowDM);
                int rows = Page.DataContract.GetValueByName<int>(mkRowsCountDM);
                int page = Page.DataContract.GetValueByName<int>(mkCurrentPageDM);

                int currRowsCount = MfgOrderContainer.GridContext.GetTotalRows();
                if (currRowsCount != rows)
                {
                    int currSelectedRowId = int.Parse(selectedRowId);
                    if (currSelectedRowId >= currRowsCount)
                    {
                        selectedRowId = (MfgOrderContainer.GridContext as BoundContext).MakeAutoRowId(0);
                        page = 1;
                    }
                }

                MfgOrderContainer.GridContext.CurrentPage = page;
                MfgOrderContainer.GridContext.LoadData();
                MfgOrderContainer.GridContext.SelectedRowIDs = new List<string> { selectedRowId };
                MfgOrderContainer.GridContext.SelectedRowID = selectedRowId;

                SetSelectionValuesToControls();
                Page.DataContract.SetValueByName(mkRowsCountDM, null);
            }
            MfgOrderContainer.GridContext.RowSelected += MfgOrderContainer_RowSelected;
        }

        public override bool PreExecute(OM.Info serviceInfo, OM.Service serviceData)
        {
            bool result = base.PreExecute(serviceInfo, serviceData);

            if (NumberingRule != null && NumberingRule.Data == null && AutoNumber != null && AutoNumber.IsChecked)
            {
                result = false;
                var labelCache = FrameworkManagerUtil.GetLabelCache(HttpContext.Current.Session);
                Page.DisplayWarning(labelCache.GetLabelByName("Start_NoNumberingRuleDefined").Value);
            }

            return result;
        }

        public virtual ResponseData MfgOrderContainer_RowSelected(object sender, JQGridEventArgs args)
        {
            Page.DataContract.SetValueByName(mkRowSelectionChangedDM, true);
            Page.DataContract.SetValueByName(mkSelectedMfgRowDM, MfgOrderContainer.GridContext.SelectedRowID);
            return args.Response;
        }

        public override void PostExecute(OM.ResultStatus status, OM.Service serviceData)
        {
            base.PostExecute(status, serviceData);
            if (status.IsSuccess)
            {
                MfgOrderContainer.Action_Reload(MfgOrderContainer.GridContext.CurrentPage.ToString());
                keepSelectedRowAfterSubmit = true;
            }
        }

        protected override void OnPreRender(System.EventArgs e)
        {
            base.OnPreRender(e);
            if (keepSelectedRowAfterSubmit)
            {
                MfgOrderContainer.GridContext.SelectedRowID = Page.DataContract.GetValueByName<string>(mkSelectedMfgRowDM);
                if (!string.IsNullOrEmpty(MfgOrderContainer.GridContext.SelectedRowID))
                {
                    MfgOrderContainer.GridContext.SelectedRowIDs.Add(MfgOrderContainer.GridContext.SelectedRowID);
                    SetSelectionValuesToControls();
                }
            }
        }

        /// Resets the page value to default 
        /// </summary> 
        /// <param name="sender"></param> 
        /// <param name="e"></param> 
        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as Camstar.WebPortal.Personalization.CustomAction;

            if (action.Parameters == "Reset")
            {
                RadioBtn_NewDocNOReuse.RadioControl.Checked = false;
                RadioBtn_NewDocReuse.RadioControl.Checked = false;
                RadioBtn_ExistingDoc.RadioControl.Checked = false;
                AttachmentType.ClearData();
                DocName.ClearData();
                DocRevision.ClearData();
                StoredFileNameField.ClearData();
                DocumentInstanceField.ClearData();
                ViewDocumentButton.Enabled = false;
                ViewDocumentButton.Visible = false;
                UploadField.ClearData();
                ClearValues();
                Page.ShopfloorReset(sender, e as CustomActionEventArgs);
                MfgOrderContainer.GridContext.SelectedRowID = null;
                if (MfgOrderContainer.GridContext.SelectedRowIDs != null)
                    MfgOrderContainer.GridContext.SelectedRowIDs.Clear();
                Page.DataContract.SetValueByName(mkSelectedMfgRowDM, null);
                Page.DataContract.SetValueByName(mkRowsCountDM, null);
                Page.DataContract.SetValueByName(mkCurrentPageDM, null);
            }
        }

        protected virtual void SetSelectionValuesToControls()
        {
            var mfgData = MfgOrderContainer.Data as DataTable;
            if (mfgData != null && !string.IsNullOrEmpty(MfgOrderContainer.GridContext.SelectedRowID))
            {
                var selectedRow = MfgOrderContainer.GridContext.GetItem(MfgOrderContainer.GridContext.SelectedRowID) as DataRow;
                if (selectedRow != null)
                {
                    Details_Qty.Data = GetColumnValue(selectedRow, mkColumnStdStartQty);
                    Details_ProductDescription.Data = GetColumnValue(selectedRow, mkColumnProdDescr);
                    Details_PriorityCode.Data = GetColumnValue(selectedRow, mkColumnPriority);
                    Details_UOM.Data = GetColumnValue(selectedRow, mkUOM);
                    string mfgOrder = GetColumnValue(selectedRow, mkMfgOrder);
                    Details_MfgOrder.Data = mfgOrder;
                    Details_Product.Data = GetProductObjectFromMfgOrder(mfgOrder);
                }
            }
        }

        protected virtual OM.RevisionedObjectRef GetProductObjectFromMfgOrder(string mfgOrderName)
        {
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(Page.Session);
            MfgOrderMaintService serv = Page.Service.GetService<Camstar.WCF.Services.MfgOrderMaintService>();
            var data = new OM.MfgOrderMaint { ObjectToChange = new OM.NamedObjectRef(mfgOrderName) };
            var req = new MfgOrderMaint_Request();
            req.Info = new OM.MfgOrderMaint_Info { ObjectChanges = new OM.MfgOrderChanges_Info { Product = new OM.Info(true) } };
            var result = new MfgOrderMaint_Result();
            var res = serv.Load(data, req, out result);
            OM.RevisionedObjectRef retVal = null;
            if (res.IsSuccess)
                retVal = result.Value.ObjectChanges.Product;
            else
                Page.DisplayMessage(res);
            return retVal;
        }

        protected virtual string GetColumnValue(DataRow row, string columnName)
        {
            string retVal = string.Empty;
            if (row[columnName] != null)
                retVal = row[columnName].ToString();
            return retVal;
        }

        protected virtual JQDataGrid MfgOrderContainer
        {
            get { return FindControl("OrderDispatch_MfgOrder") as JQDataGrid; }
        }
        protected virtual CWC.TextBox Details_Qty
        {
            get { return FindControl("Details_Qty") as CWC.TextBox; }
        }
        protected virtual CWC.NamedObject Details_UOM
        {
            get { return FindControl("Details_UOM") as CWC.NamedObject; }
        }
        protected virtual CWC.TextBox Details_ProductDescription
        {
            get { return FindControl("Details_ProductDescription") as CWC.TextBox; }
        }
        protected virtual CWC.NamedObject Details_PriorityCode
        {
            get { return FindControl("Details_PriorityCode") as CWC.NamedObject; }
        }
        protected virtual CWC.NamedObject Details_MfgOrder
        {
            get { return FindControl("Details_MfgOrder") as CWC.NamedObject; }
        }
        protected virtual CWC.RevisionedObject Details_Product
        {
            get { return FindControl("Details_Product") as CWC.RevisionedObject; }
        }

        private const string mkColumnProdDescr = "ProductDescription";
        private const string mkColumnPriority = "Priority";
        private const string mkColumnStdStartQty = "StdStartQty";
        private const string mkMfgOrder = "MfgOrder";
        private const string mkUOM = "UOM";
        private const string mkRowSelectionChangedDM = "RowSelectionChangedDM";
        private const string mkSelectedMfgRowDM = "SelectedMfgRowDM";
        private const string mkRowsCountDM = "RowsCountDM";
        private const string mkCurrentPageDM = "CurrentPageDM";
        private bool keepSelectedRowAfterSubmit;
    }
}
