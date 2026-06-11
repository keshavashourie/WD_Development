// Copyright Siemens 2023  
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using System;
using System.Collections.Generic;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using OM = Camstar.WCF.ObjectStack;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class ComponentDefect : MatrixWebPart
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            OM.ContainerRef containerData = (OM.ContainerRef)CurrentContainer.Data;

            if (containerData != null && !String.IsNullOrEmpty(containerData.Name))
                LoadContainerData();

            CurrentContainer.DataChanged += new EventHandler(CurrentContainer_DataChanged);
        }

        protected virtual void CurrentContainer_DataChanged(object sender, EventArgs e) => LoadContainerData();

        private void LoadContainerData()
        {
            OM.ContainerRef container = (OM.ContainerRef)CurrentContainer.Data;

            var dcContainer = (Page.FindCamstarControl("DCContainer") as CWC.ContainerList);
            dcContainer.Data = null;

            if (container == null || string.IsNullOrEmpty(container.Name))
                Page.ClearValues();
            else
                ReloadMaterialIssueGrid();

            dcContainer.Data = container;
        }

        protected virtual void ReloadMaterialIssueGrid()
        {
            Page.Service.ExecuteFunction("ComponentDefect", "GetActuals");
        }

        public override void GetSelectionData(OM.Service serviceData)
        {
            base.GetSelectionData(serviceData);
        }

        public override void GetInputData(OM.Service serviceData)
        {
            base.GetInputData(serviceData);
            OM.ComponentDefect svcData = (serviceData as OM.ComponentDefect);
            if (svcData != null)
            {
                JQDataGrid grid = Page.FindCamstarControl("DefectList") as JQDataGrid;
                if (grid != null && grid.Data != null)
                {
                    var childData = (grid.GridContext.SubGridTemplateContext as SubentityDataContext).AssociatedChildData;
                    if (childData != null)
                    {
                        List<OM.ComponentDefectDetail> items = new List<OM.ComponentDefectDetail>();
                        foreach (KeyValuePair<string, object> pair in childData)
                        {
                            var parent = (grid.GridContext as BoundContext).GetItem(pair.Key);
                            var childDetails = pair.Value as OM.ComponentDefectDetail[];
                            if (childDetails != null)
                            {
                                foreach (OM.ComponentDefectDetail detail in childDetails)
                                {
                                    detail.ListItemAction = OM.ListItemAction.Add;
                                    detail.ActualComponentIssue = new OM.IssueActualsHistory()
                                    {
                                        Self = (parent as OM.IssueActualsHistory).Self
                                    };
                                    items.Add(detail);
                                }
                            }
                        }
                        (serviceData as OM.ComponentDefect).ServiceDetails = items.ToArray();
                    }
                }
            }
        }

        public override void PostExecute(OM.ResultStatus status, OM.Service serviceData)
        {
            base.PostExecute(status, serviceData);
            if( status.IsSuccess)
            Page.ShopfloorReset(null, null);
        }

        public override void ClearValues(OM.Service serviceData)
        {
            base.ClearValues(serviceData);
            JQDataGrid defectSubGrid = Page.FindCamstarControl("DefectSubGrid") as JQDataGrid;
            defectSubGrid.ClearData();
        }

        protected virtual Container CurrentContainer
        {
            get
            {
                return Page.FindCamstarControl("HiddenSelectedContainer") as Container;
            }
        }
    }
}
