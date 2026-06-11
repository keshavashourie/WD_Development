/* Copyright 2022 Siemens */
using System.Collections.Generic;
using System.Linq;
using Camstar.WebPortal.FormsFramework.WebControls.PickLists;
using Camstar.WebPortal.Personalization;
using CamstarPortal.WebControls;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.WebGridControls;

namespace Camstar.WebPortal.WebPortlets
{
    public class SpecStepDetailsWebPart : CM_SpecStepDetailsWebPart
    {
        protected override OM.StepChanges StoreDetails(OM.StepChanges strpDetails) // part of GetInputData
        {
            var details = base.StoreDetails(strpDetails);
            
            var workflow = Page.DataContract.GetValueByName<OM.RevisionedObjectRef>("Workflow");
            if (ReworkPathSelectorsField.Data != null)
            {
                details.ReworkPathSelectors = ReworkPathSelectorsField.Data as OM.ReworkPathSelectorChanges[];

                if (details.ReworkPathSelectors != null)
                    foreach (var reworkPathSelector in details.ReworkPathSelectors.Where(reworkPathSelector => reworkPathSelector.Path != null))
                    {
                        reworkPathSelector.Path.CDOTypeName = string.Empty;
                        reworkPathSelector.Path.ID = null;
                        reworkPathSelector.Path.Parent = new OM.NamedSubentityRef { Name = details.Name.Value, Parent = workflow };
                    }
            }
            return details;
        }

        protected override OM.StepChanges DisplayDetails()
        {
            var details = base.DisplayDetails();

            var stepPaths = Page.DataContract.GetValueByName<List<WorkfowContext.WorkflowPath>>("StepPaths");
            if (stepPaths != null && stepPaths.Count > 0)
            {
                var reworkPathEditor = ReworkPathSelectorsField.GetInlineControl("ReworkPath") as NamedSubentity;
                var reworkPaths = stepPaths.Where(p => p.Connection == WorkfowContext.ConnectionType.Rework).ToList();
                if (reworkPathEditor != null && reworkPaths.Count > 0)
                {
                    var selVals = new OM.RecordSet
                    {
                        Headers = new[] { new OM.Header { Name = "Name" }, new OM.Header { Name = "Step" } },
                        Rows = reworkPaths.Select(p => new OM.Row { Values = new[] { p.Name, details.Name.Value } }).ToArray(),
                        TotalCount = reworkPaths.Count
                    };
                    reworkPathEditor.PickListPanelControl.DataProvider = new StaticValuesDataProvider(selVals);
                    reworkPathEditor.SetSelectionValues(selVals);
                    reworkPathEditor.RetrieveList = RetrieveListType.OnPageLoad;
                }
            }
            return details;
        }
		
		public override void WebPartCustomAction(object sender, CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as Camstar.WebPortal.Personalization.CustomAction;

            if (action != null && action.Parameters == "NotifyParent")
            {
                //var details = Page.DataContract.GetValueByName<OM.StepChanges>("StepChanges");
                //var workflow = Page.DataContract.GetValueByName<OM.RevisionedObjectRef>("Workflow");
                //if (ReworkPathSelectorsField.Data != null)
                //{
                //    details.ReworkPathSelectors = ReworkPathSelectorsField.Data as OM.ReworkPathSelectorChanges[];
	
                //    if (details.ReworkPathSelectors != null)
                //        foreach (var reworkPathSelector in details.ReworkPathSelectors.Where(reworkPathSelector => reworkPathSelector.Path != null))
                //        {
                //            reworkPathSelector.Path.CDOTypeName = string.Empty;
                //            reworkPathSelector.Path.ID = null;
                //            reworkPathSelector.Path.Parent = new OM.NamedSubentityRef { Name = details.Name.Value, Parent = workflow };
                //        }
                //}
                //Page.DataContract.SetValueByName("StepChanges", details);
                CollectInputData();
                Page.CloseFloatingFrame(true);
            }
        }

        public void CollectInputData()
        {
            var serviceData = Page.CreateServiceData(PrimaryServiceType);
            base.GetInputData(serviceData);
        }

        #region Controls

        protected virtual JQDataGrid ReworkPathSelectorsField
        {
            get { return Page.FindCamstarControl("ReworkPathSelectorsField") as JQDataGrid; }
        }

        #endregion
    }
}



