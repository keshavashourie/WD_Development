// Copyright Siemens 2023  
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Camstar.WCF.ObjectStack;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.WCFUtilities;
using Environment = Camstar.WCF.ObjectStack.Environment;

namespace Camstar.WebPortal.WebPortlets.Modeling
{
    public class PackageCreationTemplate : MatrixWebPart
    {
        #region Controls
        protected virtual JQDataGrid TargetSystemsGrid { get { return Page.FindCamstarControl("TargetSystemsGrid") as JQDataGrid; } }
        protected virtual CWC.RadioButton OwnerOnlyRadioButton { get { return Page.FindCamstarControl("FalseControl") as CWC.RadioButton; } }
        protected virtual CWC.RadioButton OwnerAndCollaboratorRadioButton { get { return Page.FindCamstarControl("TrueControl") as CWC.RadioButton; } }

        protected virtual CWC.NamedObject CollaboratorTemplate { get { return Page.FindCamstarControl("CollaboratorTemplateName") as CWC.NamedObject; } }
        protected virtual CWC.NamedObject ApprovalTemplate { get { return Page.FindCamstarControl("ApprovalTemplate") as CWC.NamedObject; } }
        protected virtual CWC.RevisionedObject ChangeMgtWorkflow { get { return Page.FindCamstarControl("ChangeMgtWorkflow") as CWC.RevisionedObject; } }
        protected virtual CWC.CheckBox WorkflowAssignApprovers { get { return Page.FindCamstarControl("WorkflowAssignApprovers") as CWC.CheckBox; } }

        #endregion

        public override void RequestSelectionValues(Info serviceInfo, Service serviceData)
        {
            if(serviceData as PackageCreationTemplateMaint==null)
                return;
            base.RequestSelectionValues(serviceInfo, serviceData);
            new WCFObject(serviceInfo).SetValue("ObjectChanges.TargetSystems", new Info(false, true));
        }
        public override void DisplaySelectionValues(Environment environment)
        {
            if (environment as PackageCreationTemplateMaint_Environment == null)
                return;
            base.DisplaySelectionValues(environment);
            Page.PortalContext.LocalSession["TargetSystemsSelVal"] = (environment as Camstar.WCF.ObjectStack.PackageCreationTemplateMaint_Environment).ObjectChanges.TargetSystems.SelectionValues;
        }

        public override void RequestValues(Info serviceInfo, Service serviceData)
        {
            base.RequestValues(serviceInfo, serviceData);
            new WCFObject(serviceInfo).SetValue("ObjectChanges.TargetSystems", new Info(true, false));
        }

        public override void DisplayValues(Service serviceData)
        {
            base.DisplayValues(serviceData);
            var packageCreationTemplateData = serviceData as PackageCreationTemplateMaint;
            if (packageCreationTemplateData != null)
            {
                var objChanges = packageCreationTemplateData.ObjectChanges;
                if (objChanges != null)
                    Page.PortalContext.LocalSession["TargetSystems"] = objChanges.TargetSystems;
            }
            TargetSystemsGrid.SetSelectionValues(Page.PortalContext.LocalSession["TargetSystemsSelVal"] as RecordSet);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
           
            OwnerOnlyRadioButton.DataChanged += (sender, eventArgs) =>
              {
                  CollaboratorTemplate.Enabled = false;
                  CollaboratorTemplate.ClearData();
              };
            OwnerAndCollaboratorRadioButton.DataChanged += (sender, eventArgs) =>
            {
                if ((bool)OwnerOnlyRadioButton.Data != true && (bool)OwnerAndCollaboratorRadioButton.Data != true)
                {
                    CollaboratorTemplate.Enabled = false;
                }
                else
                {
                    CollaboratorTemplate.Enabled = true;
                    Page.SetFocus(CollaboratorTemplate.Editor.ClientID);
                }
            };

            ChangeMgtWorkflow.DataChanged += (sender, eventArgs) =>
            {
                if ((bool)WorkflowAssignApprovers.Data)
                    ApprovalTemplate.Enabled = true;
                else
                {
                    ApprovalTemplate.Enabled = false;
                    ApprovalTemplate.ClearData();
                }
            };
        }

        protected override void OnPreRender(EventArgs e)
        {
            if (!(bool)OwnerAndCollaboratorRadioButton.Data)
                OwnerOnlyRadioButton.Data = true;

            if ((bool)OwnerOnlyRadioButton.Data && !CollaboratorTemplate.Enabled)
                CollaboratorTemplate.Data = null;

            if ((bool)WorkflowAssignApprovers.Data)
                ApprovalTemplate.Enabled = true;

            base.OnPreRender(e);

            var targetSelValData = TargetSystemsGrid.Data as DataTable;
            var targetSystemsData = Page.PortalContext.LocalSession["TargetSystems"] as NamedObjectRef[];
            if (targetSelValData != null)
            {
                if (targetSystemsData != null)
                {
                    var i = 0;
                    foreach (var row in targetSelValData.Rows)
                    {
                        if (targetSystemsData.Any(t => t.ID == (row as DataRow).ItemArray[0].ToString()))
                        {
                            var rowId = TargetSystemsGrid.BoundContext.MakeAutoRowId(i);
                            TargetSystemsGrid.GridContext.SelectRow(rowId, true);
                        }
                        i++;
                    }
                }
            }
        }

        public override void GetInputData(Service serviceData)
        {
            base.GetInputData(serviceData);
            var packageCreationTemplateMaint = serviceData as PackageCreationTemplateMaint;
            var originalData = Page.PortalContext.LocalSession["TargetSystems"];
            var selectedRows = TargetSystemsGrid.GridContext.GetSelectedItems(false);
            NamedObjectRef[] selectedTargets = null;
            if (selectedRows != null)
            {
                selectedTargets =
                    selectedRows.Select(s =>
                    {
                        var dataRow = s as DataRow;
                        return dataRow != null ? new NamedObjectRef { Name = dataRow.ItemArray[1].ToString() } : null;
                    }).ToArray();
            }
            var dcObject = new NamedObjectRef() as DCObject;
            var resData = new WCFObject(selectedTargets).GetArrayDifference(originalData as Array, dcObject.GetType(),
                ArrayCompareOptions.IndexIdentificationWFields);
            if (packageCreationTemplateMaint != null)
            {
                var data = new WCFObject(serviceData);

                data.SetValue("ObjectChanges.TargetSystems", resData as NamedObjectRef[]);
                if (!CollaboratorTemplate.Enabled)
                    data.SetValue("ObjectChanges.CollaboratorTemplate", new NamedObjectRef(String.Empty));
                if (!ApprovalTemplate.Enabled)
                    data.SetValue("ObjectChanges.ApprovalTemplate", new NamedObjectRef(String.Empty));
            }
        }
    }
}
