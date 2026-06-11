// Copyright Siemens 2023  
using Camstar.WebPortal.CommonWebControls;
using Camstar.WebPortal.FormsFramework.WebControls.PickLists;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using System;
using System.Data;
using System.Linq;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using OM = Camstar.WCF.ObjectStack;

/// <summary>
/// Summary description for ModelingAuditTrail
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Modeling
{
    public class ModelingAuditTrail : MatrixWebPart
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            Page.Load += new EventHandler(Page_Load);
        }

        protected virtual void Page_Load(object sender, EventArgs e)
        {
            CdoTypeField.DisplayingData += CdoTypeField_DisplayingData;
            CdoTypeField.DataChanged += CdoTypeField_DataChanged;
            CdoParentTypeField.DataChanged += CdoParentTypeField_DataChanged;
            ParentNdoField.DataChanged += ParentNdoField_DataChanged;
            ParentRdoField.DataChanged += ParentRdoField_DataChanged;
            NdoField.TextEditControl.TextChanged += (s, args) => { PopupButton.Enabled = CheckCdoExistence((s as TextBox).Text, null); };
            RdoField.DataChanged += (s, args) =>
            {
                var rev = (s as CWC.RevisionedObject).Data as OM.RevisionedObjectRef;
                if (rev == null)
                {
                    PopupButton.Enabled = false;
                    return;
                }
                PopupButton.Enabled = CheckCdoExistence(rev.Name, rev.Revision);
            };
            PopupButton.Click += PopupButton_Click;

            if (Page.IsPostBack)
                return;
            PopupButton.Enabled = false;
            NdoField.Hidden = RdoField.Hidden = ParentNdoField.Hidden = ParentRdoField.Hidden = SubentitiesGrid.Hidden = CdoParentTypeField.Hidden = true;
            GridLabel.Visible = false;
            var data = new OM.ModelingAuditTrailInquiry();
            var info = new OM.ModelingAuditTrailInquiry_Info() { ObjectType = new OM.Info(false, true) };
            var request = new WCF.Services.ModelingAuditTrailInquiry_Request() { Info = info };
            var service = new WCF.Services.ModelingAuditTrailInquiryService(FormsFramework.Utilities.FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);
            var result = new WCF.Services.ModelingAuditTrailInquiry_Result();
            OM.ResultStatus rs = service.GetEnvironment(data, request, out result);
            if (rs.IsSuccess)
            {
                Page.Session["CdoTypes"] = result.Environment.ObjectType.SelectionValues.GetAsDataTable();
            }
            else
                Page.DisplayMessage(rs);

        }

        protected virtual bool CheckCdoExistence(string instanceName, string instanceRevision)
        {
            var ids = Page.Session["ObjectInstanceIDs"] as DataTable;
            if (ids == null || !ids.Rows.OfType<DataRow>().Any(r => r.Field<string>("Name").Equals(instanceName)) || (!string.IsNullOrEmpty(instanceRevision) && !ids.Rows.OfType<DataRow>().Any(r => r.Field<string>("Revision").Equals(instanceRevision))))
            {
                var cdoTypes = Page.Session["CdoTypes"] as DataTable;
                var cdoTypeDescription = cdoTypes.Rows.OfType<DataRow>().First(r => r["CDODisplayName"].Equals(CdoTypeField.TextEditControl.Text));
                var input = new OM.ModelingAuditTrailInquiry();
                input.ObjectName = instanceName;
                input.ObjectType = int.Parse(cdoTypeDescription.Field<string>("CDODefID"));
                var service = new WCF.Services.ModelingAuditTrailInquiryService(FormsFramework.Utilities.FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);
                var info = new OM.ModelingAuditTrailInquiry_Info() { ObjectInstanceId = new Camstar.WCF.ObjectStack.Info(false, true) };
                var request = new WCF.Services.ModelingAuditTrailInquiry_Request() { Info = info };
                var result = new WCF.Services.ModelingAuditTrailInquiry_Result();
                OM.ResultStatus rs = service.GetEnvironment(input, request, out result);
                if (rs.IsSuccess && (result.Environment.ObjectInstanceId.SelectionValues.GetAsDataTable()).Rows.Count > 0)
                {
                    Page.Session["ObjectInstanceIDs"] = result.Environment.ObjectInstanceId.SelectionValues.GetAsDataTable();
                    ids = Page.Session["ObjectInstanceIDs"] as DataTable;
                }
            }
            return ids != null && ids.Rows.OfType<DataRow>().Any(r => r.Field<string>("Name").Equals(instanceName));
        }

        protected virtual void CdoTypeField_DisplayingData(object sender, DataRequestEventArgs e)
        {
            _cdoDAtaSource = Page.Session["CdoTypes"] as DataTable;
            if (_cdoDAtaSource != null && _cdoDAtaSource.Columns.Count > 0)
            {
                if (string.IsNullOrEmpty(CdoTypeField.ListDisplayExpression))
                    CdoTypeField.ListDisplayExpression = CdoTypeField.ListValueColumn;
                var item = new tree_row("Root", "", "", "", false);
                CollectChildren(item, "BaseObject");
                e.Data = item.children;
            }
            e.ViewMode = "tree";

        }
        protected virtual void CollectChildren(tree_row parent, string type)
        {
            type = type ?? "";
            foreach (var o in _cdoDAtaSource.Rows)
            {
                var row = o as DataRow;
                if (row != null)
                {
                    var rowType = row["ParentCDOName"] as string ?? string.Empty;
                    if (rowType == type)
                    {
                        var item = parent.Children.FirstOrDefault(ch => ch.text == row["CDOName"] as string);
                        if (item == null) // Add new Group
                        {
                            item = new tree_row(Convert.ToString(row[CdoTypeField.ListDisplayExpression]), row[CdoTypeField.ListValueColumn] as string, "", "", true);
                            parent.AddChild(item);
                        }
                        CollectChildren(item, row["CDOName"] as string);
                    }
                }
            }
        }

        protected virtual void PopupButton_Click(object sender, EventArgs e)
        {
            string instanceName = string.Empty;
            string revision = string.Empty;
            if (!SubentitiesGrid.Hidden)
            {
                var selectedRow = SubentitiesGrid.GridContext.GetSelectedItems(false)[0] as DataRow;
                instanceName = selectedRow.Field<string>("Name");
            }

            var ids = Page.Session["ObjectInstanceIDs"] as DataTable;
            var objectTypeName = CdoTypeField.Data;
            if (objectTypeName != null)
            {
                var cdoTypes = Page.Session["CdoTypes"] as DataTable;
                var cdoTypeRow = cdoTypes.Rows.OfType<DataRow>().FirstOrDefault(r => string.Equals(r["CDODefID"], objectTypeName.ToString()));
                if (cdoTypeRow != null)
                    objectTypeName = cdoTypeRow.Field<string>("CDOName");
            }
            if (!NdoField.Hidden)
            {
                instanceName = NdoField.TextEditControl.Text;
                revision = string.Empty;
            }
            else if (!RdoField.Hidden)
            {
                instanceName = RdoField.TextEditControl.Text;
                revision = RdoField.RevisionValue;
            }
            var id = ids.Rows.OfType<DataRow>().First(r => revision != null && (r.Field<string>("Name").Equals(instanceName) && (revision == string.Empty ? true : r.Field<string>("Revision").Equals(revision)))).Field<string>("ObjectInstanceId");
            Page.DataContract.SetValueByName("InstanceName", instanceName);
            Page.DataContract.SetValueByName("ObjectTypeName", objectTypeName);
            Page.DataContract.SetValueByName("Revision", revision);
            Page.DataContract.SetValueByName("InstanceId", id);

            SubentitiesGrid.GridContext.GetSelectedItems(false);
        }

        protected virtual void ParentRdoField_DataChanged(object sender, EventArgs e)
        {
            SubentitiesGrid.ClearData();
            if (string.IsNullOrEmpty(ParentRdoField.TextEditControl.Text))
            {                
                GridLabel.Visible = false;
                SubentitiesGrid.Hidden = true;
                PopupButton.Enabled = false;
                return;
            }
            FillSubentitiesGrid(true);
        }

        protected virtual void ParentNdoField_DataChanged(object sender, EventArgs e)
        {
            SubentitiesGrid.ClearData();
            if (string.IsNullOrEmpty(ParentNdoField.TextEditControl.Text))
            {                
                GridLabel.Visible = false;
                SubentitiesGrid.Hidden = true;
                PopupButton.Enabled = false;
                return;
            }
            FillSubentitiesGrid(false);
        }

        protected virtual void FillSubentitiesGrid(bool isRdo)
        {
            GridLabel.Visible = false;
            GridLabel.LabelText = CdoTypeField.TextEditControl.Text;
            SubentitiesGrid.Hidden = false;
            (SubentitiesGrid.GridContext as BoundContext).Fields[0].LabelText = CdoTypeField.TextEditControl.Text;
            var cdoTypes = Page.Session["CdoTypes"] as DataTable;
            var cdoTypeDescription = cdoTypes.Rows.OfType<DataRow>().First(r => r["CDODisplayName"].Equals(CdoTypeField.TextEditControl.Text));
            var input = new OM.ModelingAuditTrailInquiry();
            if (cdoTypeDescription.Field<string>("OwnerCDODefID") != null)
                input.ParentType = int.Parse(cdoTypeDescription.Field<string>("OwnerCDODefID"));
            if (cdoTypeDescription.Field<string>("CDODefID") != null)
                input.ObjectType = int.Parse(cdoTypeDescription.Field<string>("CDODefID"));
            var info = new OM.ModelingAuditTrailInquiry_Info() { ObjectInstanceId = new Camstar.WCF.ObjectStack.Info(false, true) };
            if (isRdo)
            {
                input.ObjectRevisionOrParent = new OM.Primitive<string>(ParentRdoField.TextEditControl.Text);
                input.ParentRevision = new OM.Primitive<string>(ParentRdoField.RevisionValue);
            }
            else
                input.ObjectRevisionOrParent = new OM.Primitive<string>(ParentNdoField.TextEditControl.Text);
            var service = new WCF.Services.ModelingAuditTrailInquiryService(FormsFramework.Utilities.FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);
            var request = new WCF.Services.ModelingAuditTrailInquiry_Request() { Info = info };
            var result = new WCF.Services.ModelingAuditTrailInquiry_Result();
            OM.ResultStatus rs = service.GetEnvironment(input, request, out result);
            if (rs.IsSuccess)
            {
                // added by jsaunders
                Page.Session["ObjectInstanceIDs"] = result.Environment.ObjectInstanceId.SelectionValues.GetAsDataTable();
                // added by jsaunders
                SubentitiesGrid.SetSelectionValues(result.Environment.ObjectInstanceId.SelectionValues);
            }
            else
                Page.DisplayMessage(rs);
        }

        protected virtual void CdoParentTypeField_DataChanged(object sender, EventArgs e)
        {
            ParentNdoField.ClearData();
            ParentRdoField.ClearData();
            SubentitiesGrid.ClearData();
            SubentitiesGrid.Hidden = true;
            GridLabel.Visible = false;
            if (CdoParentTypeField.IsEmpty)
            {
                ParentNdoField.Hidden =
                ParentRdoField.Hidden =
                SubentitiesGrid.Hidden = true;
                PopupButton.Enabled = false;
                return;
            }
            var cdoTypes = Page.Session["CdoTypes"] as DataTable;
            var cdoTypeDescription = cdoTypes.Rows.OfType<DataRow>().FirstOrDefault(r => r["CDODefID"].Equals(CdoParentTypeField.Data.ToString()));
            if (cdoTypeDescription == null)
            {
                CdoTypeField.ClearData();
                ToggleFieldsVisibility(string.Empty, string.Empty);
                return;
            }
            var cdoBaseType = cdoTypeDescription.Field<string>("CDOBaseType");
            var cdoType = cdoTypeDescription.Field<string>("CDOName");
            _currentCdoTypeName = cdoType;
            _currentCdoBaseTypeName = cdoBaseType;
            _currentCdoDisplayName = cdoTypeDescription.Field<string>("CDODisplayName");
            OM.Info info = new OM.ModelingAuditTrailInquiry_Info() { ObjectInstanceId = new OM.Info(false, true) };
            switch (cdoBaseType)
            {
                case "1":
                    ObjectTypeTextBox.TextControl.Text = (string)cdoTypeDescription["CDODefID"];
                    ParentNdoField.Hidden = false;
                    ParentNdoField.LabelText = CdoParentTypeField.Data as string;
                    ParentRdoField.ClearData();
                    ParentRdoField.Hidden = true;
                    break;
                case "2":
                    ParentRdoField.CDOTypeName = cdoType;
                    ObjectTypeTextBox.TextControl.Text = (string)cdoTypeDescription["CDODefID"];
                    ParentNdoField.ClearData();
                    ParentNdoField.Hidden = true;
                    ParentRdoField.Hidden = false;
                    ParentRdoField.LabelText = CdoParentTypeField.Data as string;
                    break;
                case "3":
                case "4":
                    ParentNdoField.Hidden =
                    ParentRdoField.Hidden =
                    SubentitiesGrid.Hidden = true;
                    GridLabel.Visible = false;
                    break;
            }
        }
        protected virtual void CdoTypeField_DataChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty((sender as CWC.DropDownList).TextEditControl.Text))
            {
                CdoTypeField.ClearData();
                ToggleFieldsVisibility(string.Empty, string.Empty);
                return;
            }
            var cdoTypes = Page.Session["CdoTypes"] as DataTable;
            var cdoTypeDescription = cdoTypes.Rows.OfType<DataRow>().FirstOrDefault(r => r["CDODisplayName"].Equals(CdoTypeField.TextEditControl.Text));
            if (cdoTypeDescription == null)
            {
                CdoTypeField.ClearData();
                ToggleFieldsVisibility(string.Empty, string.Empty);
                return;
            }
            Page.ClearValues();
            PopupButton.Enabled = false;
            var cdoBaseType = cdoTypeDescription.Field<string>("CDOBaseType");
            var cdoType = cdoTypeDescription.Field<string>("CDOName");
            _currentCdoTypeName = cdoType;
            _currentCdoBaseTypeName = cdoBaseType;
            _currentCdoDisplayName = cdoTypeDescription.Field<string>("CDODisplayName");
            OM.Info info = new OM.ModelingAuditTrailInquiry_Info() { ObjectInstanceId = new OM.Info(false, true) };
            switch (cdoBaseType)
            {
                case "1":
                    ObjectTypeTextBox.TextControl.Text = (string)cdoTypeDescription["CDODefID"];
                    ToggleFieldsVisibility(cdoBaseType, _currentCdoDisplayName);
                    break;
                case "2":
                    RdoField.CDOTypeName = _currentCdoTypeName;
                    ObjectTypeTextBox.TextControl.Text = (string)cdoTypeDescription["CDODefID"];
                    ToggleFieldsVisibility(cdoBaseType, _currentCdoDisplayName);
                    break;
                case "3":
                case "4":
                    ObjectTypeTextBox.TextControl.Text = (string)cdoTypeDescription["CDODefID"];
                    ToggleFieldsVisibility(cdoBaseType, _currentCdoDisplayName);
                    var ownerCdoDefId = cdoTypeDescription.Field<string>("OwnerCDODefId");
                    if (string.IsNullOrEmpty(ownerCdoDefId))
                        break;
                    CdoParentTypeField.Visible = true;
                    CdoParentTypeField.Data = int.Parse(ownerCdoDefId);
                    break;
            }
        }

        protected virtual void ToggleFieldsVisibility(string cdoBaseType, string currentCdoDisplayName)
        {
            switch (cdoBaseType)
            {
                case "":
                    NdoField.Hidden =
                        RdoField.Hidden =
                        ParentNdoField.Hidden =
                        ParentRdoField.Hidden =
                        CdoParentTypeField.Hidden =
                        SubentitiesGrid.Hidden = true;
                    GridLabel.Visible = false;
                    NdoField.ClearData();
                    RdoField.ClearData();
                    ParentNdoField.ClearData();
                    ParentRdoField.ClearData();
                    CdoParentTypeField.ClearData();
                    PopupButton.Enabled = false;
                    break;
                case "1":
                    NdoField.Hidden = false;
                    NdoField.LabelText = currentCdoDisplayName;
                    RdoField.Hidden =
                        ParentNdoField.Hidden =
                        ParentRdoField.Hidden =
                        CdoParentTypeField.Hidden =
                        SubentitiesGrid.Hidden = true;
                    GridLabel.Visible = false;
                    RdoField.ClearData();
                    ParentNdoField.ClearData();
                    ParentRdoField.ClearData();
                    break;
                case "2":
                    RdoField.Hidden = false;
                    RdoField.LabelText = currentCdoDisplayName;
                    NdoField.Hidden =
                        ParentNdoField.Hidden =
                        ParentRdoField.Hidden =
                        CdoParentTypeField.Hidden =
                        SubentitiesGrid.Hidden = true;
                    GridLabel.Visible = false;
                    NdoField.ClearData();
                    ParentNdoField.ClearData();
                    ParentRdoField.ClearData();
                    break;
                case "3":
                case "4":
                    CdoParentTypeField.Hidden = false;
                    NdoField.Hidden =
                        RdoField.Hidden =
                        ParentNdoField.Hidden =
                        ParentRdoField.Hidden =
                        SubentitiesGrid.Hidden = true;
                    GridLabel.Visible = false;
                    NdoField.ClearData();
                    RdoField.ClearData();
                    ParentNdoField.ClearData();
                    ParentRdoField.ClearData();
                    PopupButton.Enabled = false;
                    break;
            }
        }

        #region Properties
        protected virtual CWC.RevisionedObject RdoField
        {
            get { return Page.FindCamstarControl("RdoField") as CWC.RevisionedObject; }
        }
        protected virtual CWC.RevisionedObject ParentRdoField
        {
            get { return Page.FindCamstarControl("ParentRdoField") as CWC.RevisionedObject; }
        }
        protected virtual CWC.NamedObject NdoField
        {
            get { return Page.FindCamstarControl("NdoField") as CWC.NamedObject; }
        }
        protected virtual CWC.NamedObject ParentNdoField
        {
            get { return Page.FindCamstarControl("ParentNdoField") as CWC.NamedObject; }
        }
        protected virtual CWC.Button PopupButton
        {
            get { return Page.FindCamstarControl("PopupButton") as CWC.Button; }
        }
        protected virtual CWC.DropDownList CdoTypeField
        {
            get { return Page.FindCamstarControl("CdoTypeField") as CWC.DropDownList; }
        }
        protected virtual CWC.DropDownList CdoParentTypeField
        {
            get { return Page.FindCamstarControl("ParentTypeField") as CWC.DropDownList; }
        }
        protected virtual JQDataGrid SubentitiesGrid
        {
            get { return Page.FindCamstarControl("SubentitiesGrid") as JQDataGrid; }
        }
        protected virtual CWC.Label GridLabel
        {
            get { return Page.FindCamstarControl("GridLabel") as CWC.Label; }
        }
        protected virtual CWC.TextBox ObjectTypeTextBox
        {
            get { return Page.FindCamstarControl("ObjectType") as CWC.TextBox; }
        }
        private string _currentCdoTypeName;
        private string _currentCdoBaseTypeName;
        private string _currentCdoDisplayName;

        #endregion

        private DataTable _cdoDAtaSource;
    }
}
