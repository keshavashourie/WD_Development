// Copyright Siemens 2019  
using System;

using OM = Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;

using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;

namespace Camstar.WebPortal.WebPortlets.Modeling
{
    public class OperatorLogMaint : MatrixWebPart
    {
        #region Constants
        private const string ObjectTypeDisplayField = "ObjectTypeDisplayField";
        private const string CDODefID = "CDODefID";
        private const string CDOTypes = "CDOTypes";
        private const string CDODisplayNameColumn = "CDODisplayName";
        private const string CDOTypeNameColumn = "CDOName";
        private const string CDOBaseTypeColumn = "CDOBaseType";

        //JJR: change this be more meaningful
        private const string One = "1";
        private const string Two = "2";
        private const string Three = "3";
        private const string Four = "4";

        private string mCurrentCDOBaseType;
        private string mCurrentCDOTypeName;
        private string mCurrentCDODisplayName;
        #endregion

        protected virtual CWC.DropDownList LogObjectType { get { return Page.FindCamstarControl("ObjectChanges_LogObjectType") as CWC.DropDownList; } }
        protected virtual CWC.NamedObject NamedDataObject { get { return Page.FindCamstarControl("ObjectChanges_NamedDataObject") as CWC.NamedObject; } }
        protected virtual CWC.RevisionedObject RevisionedObject { get { return Page.FindCamstarControl("ObjectChanges_RevisionedObject") as CWC.RevisionedObject; } }
        protected virtual CWC.CheckBox IsNDO { get { return Page.FindCamstarControl("ObjectChanges_IsNDO") as CWC.CheckBox; } }
        protected virtual CWC.CheckBox IsRDO { get { return Page.FindCamstarControl("ObjectChanges_IsRDO") as CWC.CheckBox; } }
        protected virtual CWC.CheckBox IsContainer { get { return Page.FindCamstarControl("ObjectChanges_IsContainer") as CWC.CheckBox; } }
        protected virtual CWC.ContainerList Container { get { return Page.FindCamstarControl("ObjectChanges_Container") as CWC.ContainerList; } }
        protected virtual CWC.CheckBox IsQualityObject { get { return Page.FindCamstarControl("ObjectChanges_IsQualityObject") as CWC.CheckBox; } }
        protected virtual CWC.NamedObject QualityObject { get { return Page.FindCamstarControl("ObjectChanges_QualityObject") as CWC.NamedObject; } }
        
        public override void GetInputData(OM.Service serviceData)
        {
            base.GetInputData(serviceData);
            if (IsNDO.CheckControl.Checked && ((OM.OperatorLogMaint)(serviceData)).ObjectChanges != null && ((OM.OperatorLogMaint)(serviceData)).ObjectChanges.NamedDataObject != null)
                ((OM.OperatorLogMaint)(serviceData)).ObjectChanges.NamedDataObject.CDOTypeName = NamedDataObject.CDOTypeName;
            if (IsRDO.CheckControl.Checked && ((OM.OperatorLogMaint)(serviceData)).ObjectChanges != null && ((OM.OperatorLogMaint)(serviceData)).ObjectChanges.RevisionedObject != null)
                ((OM.OperatorLogMaint)(serviceData)).ObjectChanges.RevisionedObject.CDOTypeName = RevisionedObject.CDOTypeName;
            if (IsContainer.CheckControl.Checked && Container.Data != null)
            {
                if(((OM.OperatorLogMaint)(serviceData)).ObjectChanges == null)
                    ((OM.OperatorLogMaint)(serviceData)).ObjectChanges = new OM.OperatorLogChanges();
                ((OM.OperatorLogMaint)(serviceData)).ObjectChanges.Container = new OM.ContainerRef(Container.TextEditControl.Text);
                ((OM.OperatorLogMaint)(serviceData)).ObjectChanges.Container.CDOTypeName = Container.CDOTypeName;
            }
            if (IsQualityObject.CheckControl.Checked && QualityObject.Data != null)
            {
                if (((OM.OperatorLogMaint)(serviceData)).ObjectChanges == null)
                    ((OM.OperatorLogMaint)(serviceData)).ObjectChanges = new OM.OperatorLogChanges();
                ((OM.OperatorLogMaint)(serviceData)).ObjectChanges.QualityObject = new OM.NamedObjectRef(QualityObject.TextEditControl.Text);
                ((OM.OperatorLogMaint)(serviceData)).ObjectChanges.QualityObject.CDOTypeName = QualityObject.CDOTypeName;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            LogObjectType.DataChanged += LogObjectType_DataChanged;

            if(!Page.IsPostBack)
            {
                //Get all the object types to do the resolutions on the page
                NamedDataObject.Visible = false;
                RevisionedObject.Visible = false;
                Container.Visible = false;
                QualityObject.Visible = false;

                var service = new OperatorLogMaintService(FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);
                var data = new OM.OperatorLogMaint();
                var changes = new OM.OperatorLogChanges_Info();
                var info = new OM.OperatorLogMaint_Info();
                var request = new OperatorLogMaint_Request();
                OM.ResultStatus rs = new OM.ResultStatus();
                info.ObjectChanges = changes;                                
                changes.LogObjectType = FieldInfoUtil.RequestSelectionValue();
                changes.Container = FieldInfoUtil.RequestValue();
                changes.QualityObject = FieldInfoUtil.RequestValue();
                request.Info = info;

                OperatorLogMaint_Result result = null;
                rs = service.GetEnvironment(data, request, out result);
                if (rs.IsSuccess)
                    new CallStack(Page.CallStackKey).Context.LocalSession.Add("ObjectTypes", result.Environment.ObjectChanges.LogObjectType.SelectionValues); //JJR: does this need to be "LogObjectTypes"?
                else
                    DisplayMessage(rs);
            }
         }        

        private void LogObjectType_DataChanged(object sender, EventArgs e)
        {
            ClearData();
            if (LogObjectType == null || LogObjectType.Data == null || string.IsNullOrEmpty(LogObjectType.Data.ToString()))
            {
                SwitchFieldsVisibility(string.Empty, string.Empty);
                return;
            }

            UpdateCDOInstanceField(GetCDOTypeAttributes(LogObjectType.Data.ToString()));
            SwitchFieldsVisibility(mCurrentCDOBaseType, mCurrentCDODisplayName);
        }             

        private void SwitchFieldsVisibility(string cdoType, string instanceTypeName)
        {
            if (string.IsNullOrEmpty(cdoType))
            {
                NamedDataObject.Visible = false;
                RevisionedObject.Visible = false;
                Container.Visible = false;
                QualityObject.Visible = false;
                NamedDataObject.Required = false;
                RevisionedObject.Required = false;
                Container.Required = false;
                QualityObject.Required = false;
                NamedDataObject.LabelText = String.Empty;
                RevisionedObject.LabelText = String.Empty;
                Container.LabelText = String.Empty;
                QualityObject.LabelText = instanceTypeName;
                NamedDataObject.ClearData();
                RevisionedObject.ClearData();
                Container.ClearData();
                IsNDO.CheckControl.Checked = false;
                IsRDO.CheckControl.Checked = false;
                IsContainer.CheckControl.Checked = false;
                IsQualityObject.CheckControl.Checked = true;
            }
            else if (cdoType == One)//NamedObject
            {
                NamedDataObject.Visible = true;
                RevisionedObject.Visible = false;
                Container.Visible = false;
                QualityObject.Visible = false;
                IsNDO.CheckControl.Checked = true;
                IsRDO.CheckControl.Checked = false;
                IsQualityObject.CheckControl.Checked = false;
                IsContainer.CheckControl.Checked = false;
                NamedDataObject.Required = true;
                RevisionedObject.Required = false;
                QualityObject.Required = false;
                Container.Required = false;
                NamedDataObject.LabelText = instanceTypeName;
                RevisionedObject.LabelText = String.Empty;
                RevisionedObject.ClearData();
                Container.LabelText = String.Empty;
                QualityObject.LabelText = String.Empty;
                QualityObject.ClearData();
                Container.ClearData();
            }
            else if (cdoType == Two)//RevisionedObject
            {
                NamedDataObject.Visible = false;
                RevisionedObject.Visible = true;
                Container.Visible = false;
                QualityObject.Visible = false;
                IsNDO.CheckControl.Checked = false;
                IsRDO.CheckControl.Checked = true;
                IsQualityObject.CheckControl.Checked = false;
                IsContainer.CheckControl.Checked = false;
                RevisionedObject.LabelText = instanceTypeName;
                NamedDataObject.Required = false;
                RevisionedObject.Required = true;
                Container.Required = false;
                QualityObject.Required = false;
                NamedDataObject.LabelText = String.Empty;
                Container.LabelText = String.Empty;
                QualityObject.LabelText = String.Empty;
                QualityObject.ClearData();
                NamedDataObject.ClearData();
                Container.ClearData();
            }
            else if (cdoType == Three)//Container
            {
                NamedDataObject.Visible = false;
                RevisionedObject.Visible = false;
                Container.Visible = true;
                QualityObject.Visible = false;
                NamedDataObject.Required = false;
                RevisionedObject.Required = false;
                Container.Required = true;
                QualityObject.Required = false;
                NamedDataObject.LabelText = String.Empty;
                RevisionedObject.LabelText = String.Empty;
                Container.LabelText = instanceTypeName;
                QualityObject.LabelText = String.Empty;
                NamedDataObject.ClearData();
                RevisionedObject.ClearData();
                QualityObject.ClearData();
                IsNDO.CheckControl.Checked = false;
                IsRDO.CheckControl.Checked = false;
                IsContainer.CheckControl.Checked = true;
                IsQualityObject.CheckControl.Checked = false;
            }
            else if ((cdoType == Four))//QualityObject
            {
                NamedDataObject.Visible = false;
                RevisionedObject.Visible = false;
                Container.Visible = false;
                QualityObject.Visible = true;
                NamedDataObject.Required = false;
                RevisionedObject.Required = false;
                Container.Required = false;
                QualityObject.Required = true;
                NamedDataObject.LabelText = String.Empty;
                RevisionedObject.LabelText = String.Empty;
                Container.LabelText = String.Empty;
                QualityObject.LabelText = instanceTypeName;
                NamedDataObject.ClearData();
                RevisionedObject.ClearData();
                Container.ClearData();
                IsNDO.CheckControl.Checked = false;
                IsRDO.CheckControl.Checked = false;
                IsContainer.CheckControl.Checked = false;
                IsQualityObject.CheckControl.Checked = true;
            }
        }

        private void UpdateCDOInstanceField(OM.Row cdoTypeDescription)
        {
            if(cdoTypeDescription == null)
                return;

            string cdoBaseType = cdoTypeDescription.Values[6].ToString();// (Constants.CDOBaseTypeColumn)
            string cdoType = cdoTypeDescription.Values[0].ToString();// (Constants.CDOTypeNameColumn)

            //Dynamically change Field Expression depend on selected CDO Type
            //Store in view state generated Field Expression to restore it after AutoPostback
            mCurrentCDOTypeName = cdoType;
            mCurrentCDOBaseType = cdoBaseType;
            mCurrentCDODisplayName = cdoTypeDescription.Values[2].ToString();// (Constants.CDODisplayNameColumn)

            //NDO or RDO was chosen
            if (cdoBaseType == One)
                NamedDataObject.CDOTypeName = cdoType;
            else if (cdoBaseType == Two)
                RevisionedObject.CDOTypeName = cdoType;
            else if (cdoBaseType == Three)
                Container.CDOTypeName = cdoType;
            else if (cdoBaseType == Four)
                QualityObject.CDOTypeName = cdoType;

            var olmservice = new OperatorLogMaintService(FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);
            var data = new OM.OperatorLogMaint();
            if(Page.DataContract.DataMembers[2].Value != null) 
                data.ObjectToChange = new OM.NamedObjectRef(Page.DataContract.DataMembers[2].Value.ToString());
            data.ObjectChanges = new OM.OperatorLogChanges();
            if (Page.DataContract.DataMembers[2].Value != null)
                data.ObjectChanges.Name = Page.DataContract.DataMembers[2].Value.ToString();

            data.ObjectChanges.LogObjectType = new OM.Enumeration<OM.LogObjectTypeEnum, int>(Convert.ToInt32(LogObjectType.Data));
            if(IsNDO != null && IsNDO.CheckControl.Checked == true)
                data.ObjectChanges.NamedDataObject = NamedDataObject.Data as OM.NamedObjectRef;
            if (IsRDO != null && IsRDO.CheckControl.Checked == true)
                data.ObjectChanges.RevisionedObject = RevisionedObject.Data as OM.RevisionedObjectRef;
            if (IsContainer != null && IsContainer.CheckControl.Checked == true)
                data.ObjectChanges.Container = Container.Data as OM.ContainerRef;
            if (IsQualityObject != null && IsQualityObject.CheckControl.Checked == true)
                data.ObjectChanges.QualityObject = QualityObject.Data as OM.NamedObjectRef;
            var request = new OperatorLogMaint_Request();
            request.Info = new OM.OperatorLogMaint_Info();
            request.Info.ObjectChanges = new OM.OperatorLogChanges_Info();
            request.Info.ObjectChanges.ObjectInstanceId = new OM.Info() { RequestSelectionValues = true };            
            request.Info.ObjectChanges.RevisionedObject = new OM.Info() { RequestValue = true };
            request.Info.ObjectChanges.NamedDataObject = new OM.Info() { RequestValue = true };
            request.Info.ObjectChanges.Container = new OM.Info() { RequestValue = true };
            request.Info.ObjectChanges.QualityObject = new OM.Info() { RequestValue = true };
            var result = new OperatorLogMaint_Result();
            olmservice.GetEnvironment(data, request, out result);
            OM.RecordSet selectionValues = null;
            if (result.Environment != null && result.Environment.ObjectChanges != null && result.Environment.ObjectChanges.ObjectInstanceId != null)          
                selectionValues = result.Environment.ObjectChanges.ObjectInstanceId.SelectionValues;
            var localSession = new CallStack(Page.CallStackKey).Context.LocalSession;
            if (selectionValues == null)
            {
                if (localSession.ContainsKey("ObjectInstances"))
                    localSession.Remove("ObjectInstances");
            }
            else
            {
                localSession["ObjectInstances"] = selectionValues;
            }

            if (cdoBaseType == One)
                NamedDataObject.SetSelectionValues(selectionValues);//psNDOInstanceSelVals.SetSelectionValues(selectionValues);
            if (cdoBaseType == Two)
                RevisionedObject.SetSelectionValues(selectionValues);//psRDOInstanceSelVals.SetSelectionValues(selectionValues);            
            if (cdoBaseType == Three)
                Container.SetSelectionValues(selectionValues);
            if (cdoBaseType == Four)
                QualityObject.SetSelectionValues(selectionValues);
        }

        private OM.Row GetCDOTypeAttributes(string CDODefIDValue)
        {
            OM.RecordSet rs = new CallStack(Page.CallStackKey).Context.LocalSession["ObjectTypes"] as OM.RecordSet;
            OM.Row result = null;
            if (rs != null)
            {
                foreach (OM.Row dr in rs.Rows)
                    if (string.Compare(dr.Values[1].ToString(), CDODefIDValue, true) == 0)
                    {
                        result = dr;
                        break;
                    }
            }
            return result;
        }
        private OM.Row GetInstanceAttributes(string CDOName)
        {
            OM.RecordSet rs = new CallStack(Page.CallStackKey).Context.LocalSession["ObjectInstances"] as OM.RecordSet;
            OM.Row result = null;
            if (rs != null)
            {
                foreach (OM.Row dr in rs.Rows)
                    if (string.Compare(dr.Values[0].ToString(), CDOName, true) == 0)
                    {
                        result = dr;
                        break;
                    }
            }
            return result;
        }

        protected void ClearData()
        {
            NamedDataObject.ClearData();
            RevisionedObject.ClearData();
            Container.ClearData();
            QualityObject.ClearData();
            IsQualityObject.CheckControl.Checked = false;
            IsNDO.CheckControl.Checked = false;
            IsRDO.CheckControl.Checked = false;
            IsContainer.CheckControl.Checked = false;
        }
	}
}
