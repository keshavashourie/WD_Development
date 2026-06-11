// Copyright Siemens 2023  
using System;
using System.Linq;
using System.Web;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.WebPortlets;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using OM = Camstar.WCF.ObjectStack;

namespace Camstar.WebPortal.WebPortlets.DataTransfer
{
    public class InfoPanelWP : MatrixWebPart
    {

        #region Controls

        public virtual CWC.TextBox DT_TypeTxt { get { return Page.FindCamstarControl("DT_TypeTxt") as CWC.TextBox; } }
        public virtual CWC.TextBox SourceSystemTxt { get { return Page.FindCamstarControl("SourceSystemTxt") as CWC.TextBox; } }
        public virtual CWC.TextBox TargetSystemTxt { get { return Page.FindCamstarControl("TargetSystemTxt") as CWC.TextBox; } }
        public virtual CWC.TextBox InstSelectionModeTxt { get { return Page.FindCamstarControl("InstSelectionModeTxt") as CWC.TextBox; } }

        public virtual JQDataGrid InstCountsGrid { get { return Page.FindCamstarControl("InstCountsGrid") as JQDataGrid; } }
        public virtual CWC.Label InfoTxt { get { return Page.FindCamstarControl("InfoTxt") as CWC.Label; } }
        public virtual CWC.Label CountsLbl { get { return Page.FindCamstarControl("CountsLbl") as CWC.Label; } }

        #endregion

        public InfoPanelWP()
        { 
        }

        public static InfoPanel_CountGridItem[] SelectedCountsDefault
        {           
            get 
            {
                string total = FrameworkManagerUtil.GetLabelValue("Lbl_Total");
                string selected = FrameworkManagerUtil.GetLabelValue("Lbl_Selected");
                string references = FrameworkManagerUtil.GetLabelValue("ReferencesTitle");                
                return new InfoPanel_CountGridItem[]
                {
                    new InfoPanel_CountGridItem{ Name = total, Count1=null, Count2=null},
                    new InfoPanel_CountGridItem{ Name = selected, Count1=null, Count2=null},
                    new InfoPanel_CountGridItem{ Name = references, Count1=null, Count2=null}
                };
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (InstCountsGrid.GridContext != null)
            {
                InstCountsGrid.GridContext.GridReloading += GridContext_GridReloading;
            }
        }

        protected virtual ResponseData GridContext_GridReloading(object sender, JQGridEventArgs args)
        {
            if( Page != null  && _pfc == null)
            {
                _pfc = Page.PageflowControls;
            }
            if (Transfer != null && args.Context is ItemDataContext)
            {
                if (Transfer.Type == TransferType.Import)
                {
                    var count = GetCounts();
                    Array.Resize(ref count, count.Length - 1);
                    (args.Context as ItemDataContext).Data = count;
                    InstCountsGrid.BoundContext.VisibleRows = 2;
                }
                else
                {
                    (args.Context as ItemDataContext).Data = GetCounts();
                }
            }
            return null;
        }

        protected virtual string GetRadioButtonValue(string[] rbList)
        {
            CWC.RadioButton selectedRb = null;
            string res = string.Empty;

            var liveCtls = 
                from r in rbList
                select Page.FindCamstarControl(r);

            if (!liveCtls.Any(c => c == null))
            { 
                selectedRb = liveCtls.OfType<CWC.RadioButton>().FirstOrDefault(c=> (bool)c.Data == true);
            }

            if (selectedRb == null)
            {
                var ctls =
                    from r in rbList
                    select _pfc[r];
                var selected = ctls.FirstOrDefault(c => c != null && (bool)c.Data == true);
                if (selected != null)
                    res = selected.HumanName;
            }

            if (selectedRb != null)
            {
                res = selectedRb.LabelControl.Text;
            }
            return res;
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (Page.VirtualPageName == "DT_TransferType_VP")
            {
                Hidden = true;
            }
            else if (Page.VirtualPageName == "DT_Target_VP")
            {
                InstCountsGrid.Hidden = true;
                TargetSystemTxt.Hidden = false;
                CountsLbl.Visible = false;
            }
            else
            {
                InstCountsGrid.Hidden = false;
                TargetSystemTxt.Hidden = false;
                //CountsLbl.Visible = true;
            }
            _pfc = Page.PageflowControls;
            if (_pfc != null)
            {
                if (DT_TypeTxt != null)
                    DT_TypeTxt.Data = GetRadioButtonValue(new string[] { "AutoBtr", "ManualExportBtr", "ManualImportBtr" });

                if (InstSelectionModeTxt != null)
                    InstSelectionModeTxt.Data = GetRadioButtonValue(new string[] { "SpecificBtr", "GroupBtr", "SelectAllBtr" });

                if (InstCountsGrid != null && !InstCountsGrid.Hidden)
                {
                    InstCountsGrid.Action_Reload(null);
                }

                if (InfoTxt != null)
                {
                    string info = string.Empty;
                    switch (Page.VirtualPageName)
                    {
                        case "DT_Selection_VP":
                            if (Transfer.Type == TransferType.Import)
                            {
                                info = FrameworkManagerUtil.GetLabelValue("Lbl_SelectItemsToImport");
                            }
                            else
                            {
                                info = FrameworkManagerUtil.GetLabelValue("Lbl_DT_SelectItemsExport");
                            }
                            break;
                        case "DT_ExportExecution_VP":
                            info = FrameworkManagerUtil.GetLabelValue("Lbl_DT_ExportExecutionInfo");
                            break;
                        case "DT_ImportExecution_VP":
                            info = FrameworkManagerUtil.GetLabelValue("Lbl_DT_ExportExecutionInfo");
                            break;
                        case "DT_References_VP":
                            info = FrameworkManagerUtil.GetLabelValue("Lbl_DT_References");
                            break;
                        case "DT_SelectionMode_VP":
                            info = FrameworkManagerUtil.GetLabelValue("Lbl_DT_SelectionMode");
                            break;
                        case "DT_TransferType_VP":
                            info = FrameworkManagerUtil.GetLabelValue("Lbl_DT_Info");
                            break;
                        default:
                            break;
                    }
                    if (!string.IsNullOrEmpty(info))
                        InfoTxt.LabelText = info;                    
                }

                if (TargetSystemTxt != null && !TargetSystemTxt.Hidden && Page.PageflowControls != null )
                {
                    var d = Page.PageflowControls.GetDataByFieldExpression("TargetGridData");
                    if (d != null )
                    {
                        var targetLines = "";
                        Array.ForEach(d as TargetSystem[], t => targetLines += (t.IsSelected ? (t.SystemName + "\n") : ""));
                        TargetSystemTxt.Data = targetLines;
                    }
                    if (TargetSystemTxt.IsEmpty)
                    {
                        TargetSystemTxt.Data = "(" + FrameworkManagerUtil.GetLabelValue("Lbl_Pending") + ")";
                    }
                }
            }
        }

        protected virtual string GetSource()
        {
            var res = string.Empty;
            var sn = Page.FindCamstarControl("SourceNameTxt") as CWC.TextBox;
            if (sn != null)
            {
                res = sn.Data as string;
            }
            else
            {
                var ctl = _pfc["SourceNameTxt"];
                if (ctl != null)
                {
                    res = ctl.Data as string;
                }
            }
            return res;
        }

        protected virtual InfoPanel_CountGridItem[] GetCounts()
        {
            var countData = SelectedCountsDefault;
            var selectedInstances = SavedSelectedInstances;
            if (_pfc != null)
            {
                var totalObjects = _pfc.GetDataByDataPath("TotalObjects");
                var totalInstances = _pfc.GetDataByDataPath("TotalInstances");

                if (totalObjects == null)
                {
                    if (GetTotalCounts())
                    {
                        totalObjects = _pfc.GetDataByDataPath("TotalObjects");
                        totalInstances = _pfc.GetDataByDataPath("TotalInstances");
                    }
                }

                countData.First().Count1 = totalObjects != null ? (int)totalObjects : 0;
                countData.First().Count2 = totalInstances != null ? (int)totalInstances : 0;
            }

            if (SavedSelectedInstances != null)
            {
                // Selected all Instances 
                countData.ElementAt(1).Count1 = selectedInstances.Count();                                  // Objects
                countData.ElementAt(1).Count2 = ObjectTypeItem.GetInstancesCount(selectedInstances, false); // Instances

                // Selected references
                var instWithoutRefs = ObjectTypeItem.GetInstancesCount(selectedInstances, true);
                // Select objects where at least one ref
                countData.Last().Count1 = selectedInstances.Count(o => o.Instances != null && o.Instances.Any(f=> f.IsRef));
                // All references
                countData.Last().Count2 = countData.ElementAt(1).Count2 - instWithoutRefs;
            }
            return countData;
        }

        protected virtual ObjectTypeItem[] SavedSelectedInstances
        {
            get { return Page.PortalContext.DataContract.GetValueByName<ObjectTypeItem[]>("DT_SelectedInstances"); }
        }

        protected virtual bool GetTotalCounts()
        {
            var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            if (Page.VirtualPageName != "DT_TransferType_VP")
            {

                var countTotalObject = 0;
                var countTotalInstances = 0;
                if (Transfer.Type == TransferType.Export)
                {
                    var export = new ExportService(session.CurrentUserProfile);
                    var data = new OM.Export { Details = new OM.ExportImportItem[] { new OM.ExportImportItem() } };
                    var request = new Export_Request
                    {
                        Info = new OM.Export_Info { Details = new OM.ExportImportItem_Info { ObjectTypeName = new OM.Info(false, true) } }
                    };

                    Export_Result response;
                    var res = export.GetEnvironment(data, request, out response); // request exportable cdo.

                    if (res.IsSuccess && response != null && response.Environment != null && response.Environment.Details != null)
                    {
                        var details = response.Environment.Details;
                        if (details.ObjectTypeName != null && details.ObjectTypeName.SelectionValues != null)
                        {
                            var myDataSet = details.ObjectTypeName.SelectionValues;
                            // collect all exportable cdo
                            var objs = myDataSet.Rows.Where(r => r.Values[3/*IsAbstract*/] == "false" && !string.IsNullOrEmpty(r.Values[6/*InstanceCount*/]));
                            countTotalObject = objs.Count();
                            countTotalInstances = objs.Sum(o => int.Parse(o.Values[6/*InstanceCount*/]));
                        }
                    }
                }
                if (Transfer.Type == TransferType.Import)
                {
                    
                }
                Page.PageflowControls.Add("TotalObjects", countTotalObject);
                Page.PageflowControls.Add("TotalInstances", countTotalInstances);
            }
           

            return true;
        }

        protected virtual DataTransfer Transfer
        {
            get { return (DataTransfer) Page.PortalContext.LocalSession["Transfer"]; }
        }
        private PageflowControlsCollection _pfc;        
    }

    public class InfoPanel_CountGridItem
    {
        public virtual string Name { get; set; }
        public virtual int? Count1 { get; set; }
        public virtual int? Count2 { get; set; }
    }
}
