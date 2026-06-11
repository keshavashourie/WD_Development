// Copyright Siemens 2023  
using System;
using System.Collections.Generic;
using System.Linq;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.WebControls.PickLists;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.WCFUtilities;
using CamstarPortal.WebControls;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.WebGridControls;

namespace Camstar.WebPortal.WebPortlets
{
    public class CM_SpecStepDetailsWebPart: MatrixWebPart
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            Page.LoadComplete += Page_LoadComplete;
            Page.OnCreateServiceData += Page_OnCreateServiceData;
            Page.OnCreateServiceInfo += Page_OnCreateServiceInfo;
        }

        protected virtual void Page_OnCreateServiceInfo(object sender, FormsFramework.ServiceInfoEventArgs e)
        {
            var info = e.Info;
            if (InPopupData != null)
            {
                var wcfObjHelper = new WCFObject(info);
                wcfObjHelper.SetValue(SubentityFieldExpression, new WSDataCreator().CreateObject(InPopupData.GetType().Name + "_Info"));
            }
        }

        protected virtual void Page_OnCreateServiceData(object sender, FormsFramework.ServiceDataEventArgs e)
        {
            var data = e.Data;
            if (InPopupData != null)
            {
                var wcfObjHelper = new WCFObject(data);
                wcfObjHelper.SetValue(SubentityFieldExpression, new WSDataCreator().CreateArrayObject(InPopupData.GetType().Name, 1));
            }
        }

        protected virtual void Page_LoadComplete(object sender, EventArgs e)
        {
            if (!_popupClosing)
                DisplayDetails();
        }

        public override void GetInputData(OM.Service serviceData)
        {
            var details = InPopupData;
            if (details != null)
            {
                var wcfObjHelper = new WCFObject(serviceData) { ReplaceValue = true };
                wcfObjHelper.SetValue(SubentityFieldExpression, new[] { details });
            }
            
            base.GetInputData(serviceData);

            var stepDetails = new OM.StepChanges();
            var wcfObj = new WCFObject(serviceData);
            var stepDetailsList = wcfObj.GetValue(SubentityFieldExpression) as OM.StepChanges[];
            if (stepDetailsList != null && stepDetailsList.Length > 0)
                stepDetails = stepDetailsList[0];
                
            Page.DataContract.SetValueByName("StepChanges", StoreDetails(stepDetails));
            _popupClosing = true;
        }

        protected virtual OM.StepChanges StoreDetails(OM.StepChanges details) // part of GetInputData
        {
            var workflow = Page.DataContract.GetValueByName<OM.RevisionedObjectRef>("Workflow");
            if (PathSelectorsField.Data != null)
            {
                details.PathSelectors = PathSelectorsField.Data as OM.MovePathSelectorChanges[];
                if (details.PathSelectors != null)
                    foreach (var pathSelector in details.PathSelectors.Where(pathSelector => pathSelector.Path != null))
                    {
                        pathSelector.Path.CDOTypeName = string.Empty;
                        pathSelector.Path.ID = null;
                        pathSelector.Path.Parent = new OM.NamedSubentityRef { Name = details.Name.Value, Parent = workflow };
                    }
            }
            return details;
        }

        protected virtual OM.StepChanges DisplayDetails()
        {
            var stepPaths = Page.DataContract.GetValueByName<List<WorkfowContext.WorkflowPath>>("StepPaths");
            var details = InPopupData;
            Page.ProcessingContext[ProcessingFlagType.PopupDataProcessing] = true;

            if (stepPaths != null && stepPaths.Count > 0)
            {
                var pathEditor = PathSelectorsField.GetInlineControl("Path") as NamedSubentity;
                var paths = stepPaths.Where(p => p.Connection != WorkfowContext.ConnectionType.Rework).ToList();
                if (pathEditor != null && paths.Count > 0)
                {
                    var selVals = new OM.RecordSet
                    {
                        Headers = new[] { new OM.Header { Name = "Name" }, new OM.Header { Name = "Step" } },
                        Rows = paths.Select(p => new OM.Row { Values = new[] { p.Name, details.Name.Value } }).ToArray(),
                        TotalCount = paths.Count
                    };

                    pathEditor.PickListPanelControl.DataProvider = new StaticValuesDataProvider(selVals);
                    pathEditor.SetSelectionValues(selVals);
                    pathEditor.RetrieveList = RetrieveListType.OnPageLoad;
                }
            }

            if (!Page.IsPostBack)
            {
                if (details == null)
                    return null;

                var serviceData = Page.CreateServiceData(PrimaryServiceType);
                var wcfObjHelper = new WCFObject(serviceData) { ReplaceValue = true };
                wcfObjHelper.SetValue(SubentityFieldExpression, new[] { details });

                DisplayValues(serviceData);
                IsFirstStepField.ReadOnly = details.IsFirstStep != null && details.IsFirstStep.Value;
            }
            return details;
        }

        protected virtual string SubentityFieldExpression
        {
            get
            {
                return "ObjectChanges.Steps";
            }
        }

        protected virtual OM.StepChanges InPopupData
        {
            get
            {
                return Page.DataContract.GetValueByName<OM.StepChanges>("StepChanges");
            }
        }

        #region Controls
        
        protected virtual CheckBox IsFirstStepField
        {
            get { return Page.FindCamstarControl("IsFirstStepField") as CheckBox; }
        }
        
        protected virtual JQDataGrid PathSelectorsField
        {
            get { return Page.FindCamstarControl("PathSelectorsField") as JQDataGrid; }
        }
        #endregion

        private bool _popupClosing;
    }
}
