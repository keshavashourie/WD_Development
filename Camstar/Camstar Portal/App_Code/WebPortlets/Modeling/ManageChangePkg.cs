// Copyright Siemens 2023  
using System;
using System.Data;
using System.Linq;
using System.Web;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using CWGC = Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WCF.ObjectStack;
using Action = Camstar.WCF.ObjectStack.Action;

namespace Camstar.WebPortal.WebPortlets.Modeling
{
    public class ManageChangePkg : MatrixWebPart
    {
        #region Controls
        protected virtual CWGC.JQDataGrid AddToPackageGrid
        {
            get { return Page.FindCamstarControl("AddToPackage") as CWGC.JQDataGrid; }
        }

        protected virtual CWC.Button AddToPckButton
        {
            get { return Page.FindCamstarControl("AddToPkg") as CWC.Button; }
        }
        protected virtual CWGC.JQDataGrid RemovePackageGrid
        {
            get { return Page.FindCamstarControl("AssociatedViaDep") as CWGC.JQDataGrid; }
        }

        protected virtual Camstar.WebPortal.PortalFramework.JQTabContainer Tabs
        {
            get { return Page.FindCamstarControl("Tabs") as Camstar.WebPortal.PortalFramework.JQTabContainer; }
        }
        #endregion

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!Page.IsPostBack)
            {
                var index = Page.PortalContext.DataContract.GetValueByName("AddToPkgTabIndex");
                if (index != null)
                    Tabs.SelectedIndex = (int)index;
            }
        }

        public override FormsFramework.ValidationStatus ValidateInputData(Service serviceData)
        {
            ValidationStatus status = base.ValidateInputData(serviceData);
            var labelCache = FrameworkManagerUtil.GetLabelCache(System.Web.HttpContext.Current.Session);
            if (labelCache != null)
            {
                var label = labelCache.GetLabelByName("Lbl_RequiredGridMessage");
                if (label != null)
                {
                    if (Tabs.SelectedItem.Name.Equals("AddToPackage") && AddToPackageGrid.SelectedItem == null)
                    {
                        ValidationStatusItem statusItem = new RequiredFieldStatusItem(AddToPackageGrid.Caption, null) { ID = AddToPackageGrid.ID, RequiredMessage = label.Value };
                        status.Add(statusItem);
                    }

                    else if (Tabs.SelectedItem.Name.Equals("RemoveFromPackage") && RemovePackageGrid.SelectedRowCount == 0)
                    {
                        ValidationStatusItem statusItem = new RequiredFieldStatusItem(RemovePackageGrid.Caption, null) { ID = RemovePackageGrid.ID, RequiredMessage = label.Value };
                        status.Add(statusItem);
                    }
                }

            }
            return status;
        }

        public override void RequestSelectionValues(Info serviceInfo, Service serviceData)
        {
            base.RequestSelectionValues(serviceInfo, serviceData);
            var parentContext = Page.PortalContext as MaintenanceBehaviorContext;
            if (parentContext != null && parentContext.Current != null)
            {
                (serviceData as ChangePackageModelingInquiry).ObjectInstanceId = parentContext.Current.ID;
                Page.SessionVariables["ObjectType"] = parentContext.Current.CDOTypeName;
                Page.SessionVariables["Instance"] = parentContext.Current.ToString();
            }
        }

        public override void GetInputData(Service serviceData)
        {
            base.GetInputData(serviceData);
            if (RemovePackageGrid != null && RemovePackageGrid.GridContext != null)
            {
                var selectedItems = RemovePackageGrid.GridContext.SelectedRowIDs;
                if (selectedItems != null && selectedItems.Count != 0)
                {
                    if (Page.SessionVariables.GetValueByName("SaveButtonClicked") != null)
                    {
                        Page.SessionVariables.SetValueByName("RemoveFromPackage", selectedItems);
                    }
                    else
                    {
                        RemoveChgPackage(selectedItems.Select(selectedItem => new NamedObjectRef { ID = selectedItem }).ToArray());
                    }
                }
            }
            if (AddToPackageGrid != null && AddToPackageGrid.GridContext != null)
            {
                var selectedItems = AddToPackageGrid.GridContext.SelectedRowID;
                if (selectedItems != null)
                {
                    if (Page.SessionVariables.GetValueByName("SaveButtonClicked") != null)
                    {
                        Page.SessionVariables.SetValueByName("AddToPackage", selectedItems);
                    }
                    else
                    {
                        AssignChgPackage(selectedItems);
                    }
                }
            }
        }

        protected virtual void RemoveChgPackage(NamedObjectRef[] packages)
        {
            var sesn = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            if (sesn != null)
            {
                var service = new DetachSingleCPContentService(sesn.CurrentUserProfile);
                var serviceData = new WCF.ObjectStack.DetachSingleCPContent();
                var parentContext = Page.PortalContext as MaintenanceBehaviorContext;
                if (parentContext != null)
                {
                    serviceData.ChangePackage = packages[0];
                    serviceData.ServiceDetail = new DetachSingleCPContentDtl
                    {
                        ModelingInstanceID = new BaseObjectRef(parentContext.Current.ID),
                        ChangePackages = packages,
                        FieldAction = Action.Create,
                    };
                }
                ResultStatus resultStatus = service.ExecuteTransaction(serviceData);
                if (resultStatus.IsSuccess)
                    Page.SessionVariables.SetValueByName("DisplayMessage", resultStatus);//The transfer resultStatus on the parent page
            }
        }

        protected virtual void AssignChgPackage(string packageId)
        {
            var sesn = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            if (sesn != null)
            {
                var service = new AssignSingleCPContentService(sesn.CurrentUserProfile);
                var serviceData = new WCF.ObjectStack.AssignSingleCPContent();
                var parentContext = Page.PortalContext as MaintenanceBehaviorContext;
                if (parentContext != null)
                {
                    serviceData.ChangePackage = new NamedObjectRef { ID = packageId };
                    serviceData.ServiceDetail = new AssignSingleCPContentDtl
                    {
                        ModelingInstanceID = new BaseObjectRef(parentContext.Current.ID),
                        FieldAction = Action.Create
                    };
                }
                ResultStatus resultStatus = service.ExecuteTransaction(serviceData);
                if (resultStatus.IsSuccess)
                    Page.SessionVariables.SetValueByName("DisplayMessage", resultStatus);//The transfer resultStatus on the parent page
            }
        }
    }
}
