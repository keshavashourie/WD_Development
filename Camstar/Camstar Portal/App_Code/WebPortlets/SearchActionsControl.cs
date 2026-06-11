// Copyright Siemens 2023  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.FormsFramework.WebGridControls;

namespace Camstar.WebPortal.WebPortlets
{
    [Obsolete]
    [PortalStudio(false)]
    public class SearchActionsControl : ActionsControl
    {
        public SearchActionsControl()
        {
            Title = "Search Actions";
        }

        public override void RequestSingleSelectionValues(FormsFramework.IFieldSelection control, WCF.ObjectStack.Info serviceInfo, WCF.ObjectStack.Service serviceData)
        {
            base.RequestSingleSelectionValues(control, serviceInfo, serviceData);
            if (IsRestoring)
                serviceInfo = Activator.CreateInstance(serviceInfo.GetType()) as WCF.ObjectStack.Info;
        }
        
        protected override void RestoreState(FormsFramework.CallStackStateBase state)
        {
            base.RestoreState(state);
            object[] data = Page.DataContract.GetValueByName("SearchFilters") as object[];
            int index = 0;
            IsRestoring = true;
            Page.FindCamstarControls<Camstar.WebPortal.FormsFramework.IFieldData>().ToList().ForEach(c => c.Data = data[index++]);
            IsRestoring = false;
            int page = (int)Page.DataContract.GetValueByName("SearchResultPage");
            string selection = Page.DataContract.GetValueByName("SearchResultSelection") as string;
            (Page.FindCamstarControl("SearchResultsGrid") as JQDataGrid).GridContext.CurrentPage = page;
            Page.Service.LoadSingleSelectionValues(Page.FindCamstarControl("SearchResultsGrid") as FormsFramework.IFieldSelection);
            (Page.FindCamstarControl("SearchResultsGrid") as JQDataGrid).GridContext.CurrentPage = page;
            if (!string.IsNullOrEmpty(selection))
            {
                (Page.FindCamstarControl("SearchResultsGrid") as JQDataGrid).SelectedRowID = selection;
                (Page.FindCamstarControl("SearchResultsGrid") as JQDataGrid).Action_SelectRow(selection, "select");
            }
            if (page > 1 && (((Page.FindCamstarControl("SearchResultsGrid") as JQDataGrid).GridContext as SelValGridContext).Data as System.Data.DataTable).Rows.Count == 0)
            {
                (Page.FindCamstarControl("SearchResultsGrid") as JQDataGrid).GridContext.CurrentPage = 1;
                Page.Service.LoadSingleSelectionValues(Page.FindCamstarControl("SearchResultsGrid") as FormsFramework.IFieldSelection);
            }
        }

        protected override void StoreState(FormsFramework.CallStackMethodBase method, FormsFramework.CallStackStateBase state)
        {
            base.StoreState(method, state);
            state.IsDirty = true;
            object[] data = Page.FindCamstarControls<Camstar.WebPortal.FormsFramework.IFieldData>().Select(c => c.Data).ToArray();
            Page.DataContract.SetValueByName("SearchFilters", data);
            Page.DataContract.SetValueByName("SearchResultPage", (Page.FindCamstarControl("SearchResultsGrid") as JQDataGrid).GridContext.CurrentPage);
            Page.DataContract.SetValueByName("SearchResultSelection", (Page.FindCamstarControl("SearchResultsGrid") as JQDataGrid).SelectedRowID);
        }

        bool IsRestoring;
	}
}
