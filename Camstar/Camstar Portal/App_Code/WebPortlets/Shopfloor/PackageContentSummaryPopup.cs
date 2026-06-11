// Copyright Siemens 2023  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WCF.Services;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework.Utilities;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using PERS = Camstar.WebPortal.Personalization;
using CamstarPortal.WebControls;
using System.Collections;
using Camstar.WebPortal.WCFUtilities;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{

    public class PackageContentSummaryPopup : MatrixWebPart
    {
        #region Controls

        public virtual JQDataGrid InstCountsGrid { get { return Page.FindCamstarControl("InstCountsGrid") as JQDataGrid; } }

        #endregion

        public static PackageContentSummary_CountGridItem[] SelectedCountsDefault
        {
            get
            {
                var labelCache = FrameworkManagerUtil.GetLabelCache(HttpContext.Current.Session);
                GetAllLabelsInOneRequest(labelCache);
                return new PackageContentSummary_CountGridItem[]
                {
                    new PackageContentSummary_CountGridItem{ Name = labelCache.GetLabelByName("Lbl_TotalInSourceSystem").Value, Count1=null, Count2=null},
                    new PackageContentSummary_CountGridItem{ Name = labelCache.GetLabelByName("Lbl_ExplicityAssignedToPackage").Value, Count1=null, Count2=null},
                    new PackageContentSummary_CountGridItem{ Name = labelCache.GetLabelByName("Lbl_ReferencesAdded").Value, Count1=null, Count2=null}
                };
            }
        }

        private static void GetAllLabelsInOneRequest(LabelCache labelCache)
        {
            var labels = new[]{
                new OM.Label("Lbl_TotalInSourceSystem"),
                new OM.Label("Lbl_ExplicityAssignedToPackage"),
                new OM.Label("Lbl_ReferencesAdded"),
            };
            labelCache.GetLabels(new LabelList(labels));
        }

        public virtual int? totalObjects { get; set; }

        public virtual int? totalInstances { get; set; }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            SelectedInstances = Page.PortalContext.DataContract == null
                ? null
                : Page.PortalContext.DataContract.GetValueByName<ObjectTypeItem[]>("SelectedInstances");

            if (InstCountsGrid != null)
                InstCountsGrid.Data = GetCounts();
        }

        protected virtual PackageContentSummary_CountGridItem[] GetCounts()
        {
            var countData = SelectedCountsDefault;

            if (totalObjects == null)
                GetTotalCounts();

            countData.First().Count1 = totalObjects ?? 0;
            countData.First().Count2 = totalInstances ?? 0;

            if (SelectedInstances != null)
            {
                var instWithoutRefs = ObjectTypeItem.GetInstancesCount(SelectedInstances, true);

                //fill "Selected" row
                countData.ElementAt(1).Count1 = SelectedInstances.Count(o => o.Instances != null && o.Instances.Any(f => !f.IsRef));
                countData.ElementAt(1).Count2 = instWithoutRefs;

                //fill "Reference" row
                countData.Last().Count1 = SelectedInstances.Count(o => o.Instances != null && o.Instances.Any(f => f.IsRef));
                countData.Last().Count2 = ObjectTypeItem.GetInstancesCount(SelectedInstances, false) - instWithoutRefs;
            }
            else
            {
                var dtls = GetDetails();
                if (dtls != null)
                {
                    //fill "Selected" row
                    countData.ElementAt(1).Count1 = dtls.Where(s1 => s1.IsReference == false).GroupBy(x1 => x1.ObjectTypeName.Value).Count();
                    countData.ElementAt(1).Count2 = dtls.Where(s2 => s2.IsReference == false).Count();

                    //fill "Reference" row
                    countData.Last().Count1 = dtls.Where(s3 => s3.IsReference == true).GroupBy(x3 => x3.ObjectTypeName.Value).Count();
                    countData.Last().Count2 = dtls.Where(s4 => s4.IsReference == true).Count();
                }
            }
            return countData;
        }


        public virtual ObjectTypeItem[] SelectedInstances { get; set; }


        protected virtual bool GetTotalCounts()
        {
            var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);

            var countTotalObject = 0;
            var countTotalInstances = 0;

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

            totalObjects = countTotalObject;
            totalInstances = countTotalInstances;

            return true;
        }

        protected virtual OM.CPModelingInstanceDtl[] GetDetails()
        {
            var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var package = Page.PortalContext.DataContract.GetValueByName("ChangePackage") as OM.NamedObjectRef;

            var service = new GetChangePackageDetailsService(session.CurrentUserProfile);
            var data = new OM.GetChangePackageDetails()
            {
                ChangePackage = package
            };
            var info = new GetChangePackageDetails_Request()
            {
                Info = new OM.GetChangePackageDetails_Info() { PackageDetails = new OM.ChangePackageDetails_Info() { RequestValue = true } }
            };


            GetChangePackageDetails_Result result;
            OM.ResultStatus rs = service.GetPackageDetails(data, info, out result);
            if(rs.IsSuccess)
                return result.Value.PackageDetails.Instances;

            return null;
        }



        public class PackageContentSummary_CountGridItem
        {
            public virtual string Name { get; set; }
            public virtual int? Count1 { get; set; }
            public virtual int? Count2 { get; set; }
        }
    }
}
