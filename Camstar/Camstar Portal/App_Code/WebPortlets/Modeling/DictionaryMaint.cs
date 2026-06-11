// Copyright Siemens 2023  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.WebPortlets;
using OM = Camstar.WCF.ObjectStack;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.FormsFramework.Utilities;

namespace Camstar.WebPortal.WebPortlets.Modeling
{
    /// <summary>
    /// Summary description for DictionaryMaint
    /// </summary>
    public class DictionaryMaint : MatrixWebPart
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            Page.OnRequestFormValues += new EventHandler<FormsFramework.FormProcessingEventArgs>(Page_OnRequestFormValues);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            //clear the grid, if a new item is selected
            JQGridBase grid = (JQGridBase)Page.FindCamstarControl("InstanceGrid");
            grid.RowSelected += new JQGridEventHandler(grid_RowSelected);

            //it is not possible to subscribe to onclick event in Portal Studio. It looks for handler in Accordion object, 
            //instead of WebPart.
            CWC.Button searchButton = (CWC.Button)Page.FindCamstarControl("SearchButton");
            searchButton.Click += delegate { LoadLabelSet(); };          
         }

        /// <summary>
        /// Clears the grid when a new instance is selected.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        protected virtual FormsFramework.Utilities.ResponseData grid_RowSelected(object sender, JQGridEventArgs args)
        {
            LabelGrid.ClearData();
            return null;
        }

        
        protected virtual void Page_OnRequestFormValues(object sender, FormsFramework.FormProcessingEventArgs e)
        {
            //Dictionary label set should not be loaded during value request, it is loaded on the search request
            OM.DictionaryMaint_Info info = (OM.DictionaryMaint_Info)e.Info;
            info.DictionaryLabelsWorkingSet = null;
        }

        public virtual void LoadLabelSet()
        {
            OM.DictionaryMaint maint = new OM.DictionaryMaint();
            Page.GetInputData(maint);

            //stop searching if the filter is empty
            if (maint.Category == null && maint.LabelNameFilter == null && maint.LabelValueFilter == null)
            {
                LabelGrid.Data = LabelGrid.OriginalData = null;
                return;
            }

            if (maint.ObjectChanges == null || maint.ObjectChanges.Name == null)            
                maint.ObjectToChange = (Page.PortalContext as MaintenanceBehaviorContext).Current as OM.NamedObjectRef;                         
            else
                maint.ObjectToChange = new OM.NamedObjectRef(maint.ObjectChanges.Name.Value);            
                                   
            OM.DictionaryMaint_Info info = new OM.DictionaryMaint_Info { 
                DictionaryLabelsWorkingSet = new OM.DictionaryLabelChanges_Info { RequestValue = true }                
            };
            OM.Service output = new OM.DictionaryMaint();            
            OM.ResultStatus status = Service.ExecuteFunction(maint, info, "Load", ref output);
            if (status.IsSuccess)
            {
                OM.DictionaryLabelChanges[] data = ((OM.DictionaryMaint)output).DictionaryLabelsWorkingSet;
                // create a deep copy
                if (data != null)
                {
                    LabelGrid.OriginalData = data.Select(it => (OM.DictionaryLabelChanges) it.Clone()).ToArray();
                    LabelGrid.Data = data.Select(it => (OM.DictionaryLabelChanges) it.Clone()).ToArray();
                }
                else
                {
                    LabelGrid.Data = null;
                }                
            }
            else
                Page.StatusBar.WriteStatus(status);
        }
        
        public override void GetInputData(OM.Service serviceData)
        {
            base.GetInputData(serviceData);                         
            OM.DictionaryLabelChanges[] labelDifferences = GetLabelDictionaryDifference();
            ((OM.DictionaryMaint)serviceData).DictionaryLabelsWorkingSet = labelDifferences;                       
        }

        public override void PostExecute(OM.ResultStatus status, OM.Service serviceData)
        {
            base.PostExecute(status, serviceData);
            if (status.IsSuccess)
            {
                // Update label cache                      
                var labelChanges = LabelGrid.Data as OM.DictionaryLabelChanges[];
                if (labelChanges != null)
                {
                    // clear from iis cache
                    FrameworkManagerUtil.GetLabelCache().ClearCache();
                }
            }
        }

        /// <summary>
        /// Get Diffrences for Label grid.
        /// If Label value is changed to empty value, the label will be deleted.
        /// If Label value is changed from empty value, the label will be added.
        /// If non-empty label value is changed to non-empty value, the label will be just changed. 
        /// </summary>
        /// <returns></returns>
        protected virtual OM.DictionaryLabelChanges[] GetLabelDictionaryDifference()
        {
            JQDataGrid labelGrid = LabelGrid;
            OM.DictionaryLabelChanges[] originalArr = labelGrid.OriginalData as OM.DictionaryLabelChanges[];
            OM.DictionaryLabelChanges[] currentArr = labelGrid.Data as OM.DictionaryLabelChanges[];
            if (originalArr == null)
                return null;
            int length = currentArr.Length;
            List<OM.DictionaryLabelChanges> arrChanges = new List<OM.DictionaryLabelChanges>();
            for (int i = 0; i < length; i++)
            {
                var original = originalArr[i];
                var current = currentArr[i];
                if (original.LabelValue != current.LabelValue || original.DefaultValue != current.DefaultValue)
                {
                    OM.DictionaryLabelChanges newItem = new OM.DictionaryLabelChanges
                    {
                        DefaultValue = current.DefaultValue,
                        LabelID = current.LabelID,
                        LabelValue = current.LabelValue,
                        Name = current.Name
                    };
                    newItem.ListItemAction = OM.ListItemAction.Add;
                    newItem.ListItemIndex = null;
                    arrChanges.Add(newItem);
                }
            }
            return arrChanges.ToArray();
        }

        protected virtual JQDataGrid LabelGrid
        {
            get
            {
                return (JQDataGrid)Page.FindCamstarControl("DictionaryLabelSet");
            }
        }        
    }
}
