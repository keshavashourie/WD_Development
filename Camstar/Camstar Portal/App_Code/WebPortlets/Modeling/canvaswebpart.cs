/* Copyright 2019 Siemens */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.WCFUtilities;
using CamstarPortal.WebControls;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;

namespace Camstar.WebPortal.WebPortlets
{
    public class CanvasWebPart : MatrixWebPart
    {
                protected override void OnPreLoad(object sender, EventArgs e)
        {
            base.OnPreLoad(sender, e);
            WorkflowsControl.PickListPanelControl.NoCacheOutput = true;
        }
		
		protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            SpecsControl.DisplayPanelOnly = true;
            WorkflowsControl.DisplayPanelOnly = true;
            SpecsControl.PickListPanelControl.Attributes.CssStyle.Add(HtmlTextWriterStyle.ZIndex, "0");
            WorkflowsControl.PickListPanelControl.Attributes.CssStyle.Add(HtmlTextWriterStyle.ZIndex, "0");
            if (!string.IsNullOrEmpty(Page.PrimaryServiceType))
            {
                var service = new WSDataCreator().CreateObject(Page.PrimaryServiceType);
                if (service.GetType().IsAssignableFrom(typeof (ChangeMgtWorkflowMaint)))
                    CanvasControl.DisableReworkPath = true;
            }
            Page.BeforeSubmitTransactionCommit += Page_BeforeSubmitTransactionCommit;
            _portalContext = Page.PortalContext as MaintenanceBehaviorContext;
        }

        public override void ClearValues()
        {
            base.ClearValues();
            SpecsControl.ClearData();
            WorkflowsControl.ClearData();
            CanvasControl.Redraw = true;
        }

        private void Page_BeforeSubmitTransactionCommit(object sender, TransactionEventHandler e)
        {
            var pc = Page.PortalContext as MaintenanceBehaviorContext;
            string sPrimaryService = Page.PrimaryServiceType;
            if (pc.State != MaintenanceBehaviorContext.MaintenanceState.None)
            {
                var service = e.Service;
                if (Page.PrimaryServiceType == "WorkflowMaint" || Page.PrimaryServiceType == "ChangeMgtWorkflowMaint")
                {
                    var inputData = WCFObject.CreateObject(e.Data.GetType()) as BusinessProcessWorkflowMaint;
                    if (inputData == null)
                        return;
                    inputData.ObjectChanges = WCFObject.CreateObject(e.Data.ObjectChanges.GetType()) as BusinessProcessWorkflowChanges;
                    if (inputData.ObjectChanges == null)
                        return;
                    inputData.ObjectChanges.FirstStep = CanvasControl.GetFirstStepData(GetCurrentWorkflow());
                    inputData.ObjectChanges.Steps = PrepareSecondInputData(_stepsToSubmit);
                    service.ExecuteTransaction(inputData);
                }
                else
                {
                    var inputData = new WorkflowMaint { ObjectChanges = new WorkflowChanges() };
                    string sChangesTypeCDOName = "WorkflowChanges";
                    switch (sPrimaryService.ToUpper())
                    {
                        case "DIEBANKWORKFLOWMAINT":
                            inputData = new DieBankWorkflowMaint { ObjectChanges = new DieBankWorkflowChanges() };
                            sChangesTypeCDOName = "DieBankWorkflowChanges";
                            break;
                        case "MATERIALINVENTORYWFMAINT":
                            inputData = new MaterialInventoryWFMaint { ObjectChanges = new MaterialInventoryWFChanges() };
                            sChangesTypeCDOName = "MaterialInventoryWFChanges";
                            break;
                        case "TESTSTOREWORKFLOWMAINT":
                            inputData = new TestStoreWorkflowMaint { ObjectChanges = new TestStoreWorkflowChanges() };
                            sChangesTypeCDOName = "TestStoreWorkflowChanges";
                            break;
                        case "WAFERINVENTORYWORKFLOWMAINT":
                            inputData = new WaferInventoryWorkflowMaint { ObjectChanges = new WaferInventoryWorkflowChanges() };
                            sChangesTypeCDOName = "WaferInventoryWorkflowChanges";
                            break;
                        case "WAFERSORTINVENTORYWFMAINT":
                            inputData = new WaferSortInventoryWFMaint { ObjectChanges = new WaferSortInventoryWFChanges() };
                            sChangesTypeCDOName = "WaferSortInventoryWFChanges";
                            break;
                        case "ASSEMBLYWORKFLOWMAINT":
                            inputData = new AssemblyWorkflowMaint { ObjectChanges = new AssemblyWorkflowChanges() };
                            sChangesTypeCDOName = "AssemblyWorkflowChanges";
                            break;
                        case "BACKGRINDWORKFLOWMAINT":
                            inputData = new BackGrindWorkflowMaint { ObjectChanges = new BackGrindWorkflowChanges() };
                            sChangesTypeCDOName = "BackGrindWorkflowChanges";
                            break;
                        case "MATERIALWIPWORKFLOWMAINT":
                            inputData = new MaterialWIPWorkflowMaint { ObjectChanges = new MaterialWIPWorkflowChanges() };
                            sChangesTypeCDOName = "MaterialWIPWorkflowChanges";
                            break;
                        case "SUPPLEMENTARYWORKFLOWMAINT":
                            inputData = new SupplementaryWorkflowMaint { ObjectChanges = new SupplementaryWorkflowChanges() };
                            sChangesTypeCDOName = "SupplementaryWorkflowChanges";
                            break;
                        case "TESTWORKFLOWMAINT":
                            inputData = new TestWorkflowMaint { ObjectChanges = new TestWorkflowChanges() };
                            sChangesTypeCDOName = "TestWorkflowChanges";
                            break;
                        case "FINALTESTWORKFLOWMAINT":
                            inputData = new FinalTestWorkflowMaint { ObjectChanges = new FinalTestWorkflowChanges() };
                            sChangesTypeCDOName = "FinalTestWorkflowChanges";
                            break;
                        case "WAFERSORTWORKFLOWMAINT":
                            inputData = new WaferSortWorkflowMaint { ObjectChanges = new WaferSortWorkflowChanges() };
                            sChangesTypeCDOName = "WaferSortWorkflowChanges";
                            break;
                        case "WAFERWIPWORKFLOWMAINT":
                            inputData = new WaferWIPWorkflowMaint { ObjectChanges = new WaferWIPWorkflowChanges() };
                            sChangesTypeCDOName = "WaferWIPWorkflowChanges";
                            break;
                    }

                    inputData.ObjectChanges.FirstStep = CanvasControl.GetFirstStepData(GetCurrentWorkflow());
                    inputData.ObjectChanges.Steps = PrepareSecondInputData(_stepsToSubmit);
                    if (inputData.ObjectChanges.FirstStep != null)
                        inputData.ObjectChanges.FirstStep.Parent.CDOTypeName = sChangesTypeCDOName;

                    if (inputData.ObjectChanges.Steps != null)
                    {
                        foreach (StepChanges oStepChange in inputData.ObjectChanges.Steps)
                        {
                            if (oStepChange.DefaultPath != null)
                            {
                                NamedSubentityRef oPathChanges = oStepChange.DefaultPath;
                                if (oPathChanges.Parent != null)
                                {
                                    NamedSubentityRef oSpecStepChanges = oPathChanges.Parent as NamedSubentityRef;
                                    if (oSpecStepChanges.Parent != null)
                                        oSpecStepChanges.Parent.CDOTypeName = sChangesTypeCDOName;
                                }
                            }

                            // alternate paths
                            if (oStepChange.Paths != null)
                            {
                                foreach (MovePathChanges oPath in oStepChange.Paths)
                                {
                                    NamedSubentityRef oToStep = oPath.ToStep;
                                    if (oToStep != null)
                                        oToStep.Parent.CDOTypeName = sChangesTypeCDOName;
                                }
                            }

                            // rework paths
                            if (oStepChange.ReworkPaths != null)
                            {
                                foreach (ReworkPathChanges oReworkPath in oStepChange.ReworkPaths)
                                {
                                    NamedSubentityRef oToStep = oReworkPath.ToStep;
                                    if (oToStep != null)
                                        oToStep.Parent.CDOTypeName = sChangesTypeCDOName;
                                }
                            }

                            if (oStepChange.StepType != null)
                                if (oStepChange.StepType.ToString() == "")
                                    oStepChange.StepType = null;
                        }
                    }
                    service.ExecuteTransaction(inputData);
                } //else
            }
        }

        public override void GetInputData(Service serviceData)
        {
            base.GetInputData(serviceData);
            string sPrimaryService = Page.PrimaryServiceType;

            var serviceData1 = serviceData as BusinessProcessWorkflowMaint;
            if (serviceData1 != null)
            {
                if (_portalContext != null && _portalContext.State != MaintenanceBehaviorContext.MaintenanceState.None)
                {
                    _stepsToSubmit = CanvasControl.GetData(GetCurrentWorkflow());
                        
                    var serviceObject = new WCFObject(serviceData) { ReplaceValue = true };
                    serviceObject.SetValue("ObjectChanges.Steps", PrepareFirstInputData(_stepsToSubmit));
                    
                    if (Page.PrimaryServiceType != "WorkflowMaint" && Page.PrimaryServiceType != "ChangeMgtWorkflowMaint")
                    {
                        (serviceData as MainWorkflowMaint).ObjectChanges.InTransitStepCount = null;
                        (serviceData as MainWorkflowMaint).ObjectChanges.InTransitStepCountOnDefault = null;
                        (serviceData as MainWorkflowMaint).ObjectChanges.InventoryStepCount = null;
                        (serviceData as MainWorkflowMaint).ObjectChanges.InventoryStepCountOnDefault = null;
                        (serviceData as MainWorkflowMaint).ObjectChanges.ScheduledStepCount = null;
                        (serviceData as MainWorkflowMaint).ObjectChanges.ScheduledStepCountOnDefault = null;
                        (serviceData as MainWorkflowMaint).ObjectChanges.WIPStepCount = null;
                        (serviceData as MainWorkflowMaint).ObjectChanges.WIPStepCountOnDefault = null;
                    }

                } // if (_portalContext.State != MaintenanceBehaviorContext.MaintenanceState.None)
            } // servicedata1 != null
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            var pc = Page.PortalContext as MaintenanceBehaviorContext;
            if (pc.State == MaintenanceBehaviorContext.MaintenanceState.Edit)
                CanvasControl.WorkflowValue = pc.Current as RevisionedObjectRef;
            else
                CanvasControl.WorkflowValue = null;
        }

        protected virtual BaseObjectRef GetCurrentWorkflow()
        {
            var instanceHeaderWp = Page.Manager.WebParts["MDL_InstanceHeader"];
            if (instanceHeaderWp != null)
            {
                var nameTxt = instanceHeaderWp.FindControl("NameTxt") as CWC.TextBox;
                var revTxt = instanceHeaderWp.FindControl("RevisionTxt") as CWC.TextBox;
                if (nameTxt != null && revTxt != null)
                {
                    var name = nameTxt.Data == null ? "" : nameTxt.Data.ToString();
                    var rev = revTxt.Data == null ? "" : revTxt.Data.ToString();
                    var changesType = WCFObject.GetFieldMetadata(PrimaryServiceType + ".ObjectChanges").CDOTypeName;
                    return new RevisionedObjectRef(name, rev) { CDOTypeName = changesType };
                }
            }
            return null;
        }

        //Returns added steps only without paths.
        protected virtual StepChanges[] PrepareFirstInputData(IEnumerable<StepChanges> allSteps)
        {
            var retVal = new List<StepChanges>();
            if (allSteps != null)
            {
                foreach (var step in allSteps.Where(step => step.ListItemAction.HasValue && step.ListItemAction.Value == ListItemAction.Add))
                {
                    StepChanges newStep;
                    if (WorkflowHelper.IsSpec(step, CanvasControl.SpecStepChangesType))
                    {
                        newStep = WorkflowHelper.CreateSpecStepChanges(CanvasControl.SpecStepChangesType);
                        WorkflowHelper.SetSpec(newStep, WorkflowHelper.GetSpec(step, CanvasControl.SpecStepFieldName), CanvasControl.SpecStepFieldName);
                    }
                    else
                    {
                        newStep = WorkflowHelper.CreateSubWorkflowStepChanges(CanvasControl.SubWorkflowStepChangesType);
                        WorkflowHelper.SetSubWorkflow(newStep, WorkflowHelper.GetSubWorkflow(step, CanvasControl.SubWorkflowStepFieldName), CanvasControl.SubWorkflowStepFieldName);
                    }
                    newStep.ListItemAction = ListItemAction.Add;
                    newStep.Name = step.Name;
                    newStep.XLocation = step.XLocation;
                    newStep.YLocation = step.YLocation;
                    retVal.Add(newStep);
                }
            }
            return retVal.Count > 0 ? retVal.ToArray() : null;
        }

        // Returns modified/deleted steps.
        private static StepChanges[] PrepareSecondInputData(IEnumerable<StepChanges> allSteps)
        {
            var retVal = new List<StepChanges>();
            if (allSteps != null)
            {
                foreach (var step in allSteps.Where(step => step.ListItemAction.HasValue))
                {
                    if (step.ListItemAction.HasValue && step.ListItemAction.Value == ListItemAction.Add)
                    {
                        step.ListItemAction = ListItemAction.Change;
                        step.Key = new NamedObjectRef(step.Name.Value);
                    }
                    retVal.Add(step);
                }
            }
            return retVal.Count > 0 ? retVal.ToArray() : null;
        }

        protected virtual WorkflowViewerControl CanvasControl
        {
            get { return Page.FindCamstarControl("CanvasControl") as WorkflowViewerControl; }
        }

        protected virtual FormsFramework.WebControls.RevisionedObject SpecsControl
        {
            get
            {
                return Page.FindCamstarControl("SpecsControl") as FormsFramework.WebControls.RevisionedObject;
            }
        }

        protected virtual FormsFramework.WebControls.RevisionedObject WorkflowsControl
        {
            get
            {
                return Page.FindCamstarControl("WorkflowsControl") as FormsFramework.WebControls.RevisionedObject;
            }
        }

        private StepChanges[] _stepsToSubmit;
        private MaintenanceBehaviorContext _portalContext;
    }
}



