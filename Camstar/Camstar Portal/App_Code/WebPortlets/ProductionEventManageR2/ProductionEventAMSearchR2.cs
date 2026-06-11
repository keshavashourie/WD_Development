using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls.WebParts;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using System.Data;
using System.Linq;
using System.Web;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework;
using Camstar.WCF.Services;
using Camstar.WebPortal.WCFUtilities;

namespace Camstar.WebPortal.WebPortlets
{
    public class ProductionEventAMSearchR2 : MatrixWebPart
    {
        #region Properties

        protected virtual CWC.NamedObject InstanceID
        {
            get { return Page.FindCamstarControl("InstanceID") as CWC.NamedObject; }
        }

        protected virtual JQTabContainer TabContainer { get { return Page.FindCamstarControl("Tabs") as JQTabContainer; } }

        protected virtual JQDataGrid SearchResultsGrid
        {
            get { return Page.FindCamstarControl("AMSearchResultsGrid") as JQDataGrid; }
        }

        protected virtual JQDataGrid AffectedMaterialsGrid
        {
            get { return Page.FindCamstarControl("AffectedMaterialsGrid") as JQDataGrid; }
        }

        protected virtual JQDataGrid AffectedMaterialsGridAdd
        {
            get { return Page.FindCamstarControl("AffectedMaterialsGridAdd") as JQDataGrid; }
        }

        protected virtual CWC.Button AddSelectedButton
        {
            get { return Page.FindCamstarControl("AddSelectedButton") as CWC.Button; }
        }

        protected virtual CWC.Button AddManuallyButton
        {
            get { return Page.FindCamstarControl("AddManuallyButton") as CWC.Button; }
        }

        protected virtual CWC.DropDownList AddManuallyProduct
        {
            get { return Page.FindCamstarControl("AddManuallyProduct") as CWC.DropDownList; }
        }

        protected virtual CWC.Button AddManuallyUpdateButton
        {
            get { return Page.FindCamstarControl("AddManuallyUpdateButton") as CWC.Button; }
        }

        protected virtual CWC.Button UpdateAffectedMaterial
        {
            get { return Page.FindCamstarControl("UpdateAffectedMaterial") as CWC.Button; }
        }

        protected virtual CWC.Button ClearAllButton
        {
            get { return Page.FindCamstarControl("ClearALLButton") as CWC.Button; }
        }

        protected virtual CWC.RevisionedObject ProductFilter
        {
            get { return Page.FindCamstarControl("ProductFilter") as CWC.RevisionedObject; }
        }

        protected virtual CWC.RadioButton SearchTypeMaterialMatch
        {
            get { return Page.FindCamstarControl("SearchTypeMaterialMatch") as CWC.RadioButton; }
        }

        protected virtual CWC.RadioButton SearchTypeContainersOnly
        {
            get { return Page.FindCamstarControl("SearchTypeContainersOnly") as CWC.RadioButton; }
        }

        protected virtual CWC.RadioButton SearchTypeContainersWithMaterial
        {
            get { return Page.FindCamstarControl("SearchTypeContainersWithMaterial") as CWC.RadioButton; }
        }

        protected virtual CWC.CheckBox SearchEmployeeHistory
        {
            get { return Page.FindCamstarControl("SearchEmployeeHistory") as CWC.CheckBox; }
        }

        #endregion

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            AddSelectedButton.Click += new EventHandler(AddSelectedButton_Click);
            AddManuallyProduct.DataChanged += new EventHandler(AddManuallyProduct_DataChanged);
            UpdateAffectedMaterial.Click += UpdateAffectedMaterialOnClick;
            AffectedMaterialsGrid.GridContext.DataChanged += AffectedMaterialsGrid_DataChanged;
            AffectedMaterialsGridAdd.GridContext.DataChanged += AffectedMaterialsGridAdd_DataChanged;
            ClearAllButton.Click += ClearAllButton_Click;
            SearchTypeContainersOnly.RadioControl.CheckedChanged += RadioControl_CheckedChanged;
            SearchTypeContainersWithMaterial.RadioControl.CheckedChanged += RadioControl_CheckedChanged;
            SearchTypeMaterialMatch.RadioControl.CheckedChanged += RadioControl_CheckedChanged;

            AddManuallyUpdateButton.Hidden = !AddManuallyButton.Hidden;

            var postbackSource = Page.Request.Params["__EVENTTARGET"];
            if (Page.IsPostBack && !string.IsNullOrEmpty(postbackSource))
            {
                var ctrl = Page.FindControl(postbackSource);
                if (ctrl != null && AffectedMaterialsGrid.ID.Equals(ctrl.ID))
                    DeleteDuplicateContainers(AffectedMaterialsGrid);
            }
            //For Shopfloor Actions
            Page.DataContract.SetValueByName("Containers", null);
            Page.DataContract.SetValueByName("SplitOrScrapContainer", null);
            var containers = AffectedMaterialsGrid.GridContext.GetSelectedItems(false);
            if (containers != null)
            {
                Page.DataContract.SetValueByName("Containers", containers.Cast<EventLotDetail>().ToArray());
                if (containers.Length > 0)
                {
                    var containerName = containers.Cast<EventLotDetail>().ToArray()[0].Lot;
                    Page.DataContract.SetValueByName("SplitOrScrapContainer", new ContainerRef(containerName.ToString()));
                }

            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (TabContainer.SelectedItem.Name == "AffectedMaterial" && Page.SessionVariables["AffectedMaterial"] == null)
                LoadLots();
        }

        private void UpdateAffectedMaterialOnClick(object sender, EventArgs eventArgs)
        {
            var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            if (session != null)
            {
                var service = new UpdateEventLotsService(session.CurrentUserProfile);

                //// Set up parameters for the service here
                var serviceData = new UpdateEventLots();
                serviceData.QualityESigDetail = ESigCaptureUtil.CollectQualityESigDetail();
                serviceData.QualityObject = new NamedObjectRef() { CDOTypeName = "Event", Name = InstanceID.Data.ToString() };
                var affectedMaterials = Page.SessionVariables["AffectedMaterial"] as EventLotDetail[];
                if (affectedMaterials != null)
                {
                    serviceData.EventLotDetails = affectedMaterials.Select(am => new EventLotDetail
                    {
                        Lot = am.Lot,
                        ProductName = am.ProductName,
                        ProductDescription = am.ProductDescription,
                        Qty = am.Qty,
                        UOMName = am.UOMName,
                        ReferenceDesignator = am.ReferenceDesignator,
                        QtySampled = am.QtySampled,
                        QtyDefective = am.QtyDefective,
                        IsContainer = am.IsContainer,
                        ProductRev = am.ProductRev,
                        EventLot = am.EventLot
                    }).ToArray();
                }


                var request = new UpdateEventLots_Request()
                {
                    Info = new UpdateEventLots_Info()
                    {
                        EventLotDetails = new EventLotDetail_Info()
                        {
                            Lot = new Info(true),
                            ProductName = new Info(true),
                            ProductDescription = new Info(true),
                            Qty = new Info(true),
                            UOMName = new Info(true),
                            ReferenceDesignator = new Info(true),
                            QtySampled = new Info(true),
                            QtyDefective = new Info(true),
                            QtyPendingDisp = new Info(true),
                            Container = new Info(true),
                            IsContainer = new Info(true),
                            ProductRev = new Info(true),
                            EventLot = new Info(true),
                            EventDispositionDetails = new EventDispositionDetail_Info()
                            {
                                ApplyEntireQty = new Info(true),
                                Comments = new Info(true),
                                Disposition = new Info(true),
                                Qty = new Info(true),
                                EventDisposition = new Info(true),
                                EventFailureDetails = new EventFailureDetail_Info()
                                {
                                    Comments = new Info(true),
                                    EventFailure = new Info(true),
                                    EventFailureCauseDetails = new EventFailureCauseDetail_Info(),
                                    FailureMode = new Info(true),
                                    FailureModeGroup = new Info(true),
                                    FailureSeverity = new Info(true),
                                    FailureType = new Info(true)
                                },
                            }
                        }
                    }
                };

                var result = new UpdateEventLots_Result();

                ResultStatus resultStatus = service.ExecuteTransaction(serviceData, request, out result);

                if (resultStatus != null && resultStatus.IsSuccess)
                {
                    DisplayMessage(resultStatus);
                    Page.SessionVariables["AffectedMaterial"] = result.Value.EventLotDetails;
                    SearchResultsGrid.ClearData();
                }
                else
                {
                    DisplayMessage(resultStatus);
                }
                ESigCaptureUtil.CleanQualityESigCaptureDM();
            }
        }

        protected virtual void LoadLots()
        {
            var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            if (session != null)
            {
                var service = new UpdateEventLotsService(session.CurrentUserProfile);

                //// Set up parameters for the service here
                var serviceData = new UpdateEventLots
                {
                    QualityObject = new NamedObjectRef() { CDOTypeName = "Event", Name = InstanceID.Data.ToString() }
                };


                var request = new UpdateEventLots_Request()
                {
                    Info = new UpdateEventLots_Info()
                    {
                        QualityObject = new Info(true),
                        EventLotDetails = new EventLotDetail_Info()
                        {
                            Lot = new Info(true),
                            ProductName = new Info(true),
                            ProductDescription = new Info(true),
                            Qty = new Info(true),
                            UOMName = new Info(true),
                            ReferenceDesignator = new Info(true),
                            QtySampled = new Info(true),
                            QtyDefective = new Info(true),
                            QtyPendingDisp = new Info(true),
                            Container = new Info(true),
                            IsContainer = new Info(true),
                            ProductRev = new Info(true),
                            EventLot = new Info(true),
                            EventDispositionDetails = new EventDispositionDetail_Info()
                            {
                                ApplyEntireQty = new Info(true),
                                Comments = new Info(true),
                                Disposition = new Info(true),
                                Qty = new Info(true),
                                EventDisposition = new Info(true),
                                EventFailureDetails = new EventFailureDetail_Info()
                                {
                                    Comments = new Info(true),
                                    EventFailure = new Info(true),
                                    EventFailureCauseDetails = new EventFailureCauseDetail_Info(),
                                    FailureMode = new Info(true),
                                    FailureModeGroup = new Info(true),
                                    FailureSeverity = new Info(true),
                                    FailureType = new Info(true)
                                },
                            }
                        }
                    }
                };

                var result = new UpdateEventLots_Result();

                ResultStatus resultStatus = service.Load(serviceData, request, out result);

                if (resultStatus != null && resultStatus.IsSuccess)
                {
                    AffectedMaterialsGrid.Data = result.Value.EventLotDetails;
                    AffectedMaterialsGridAdd.Data = result.Value.EventLotDetails;
                    Page.SessionVariables["AffectedMaterial"] = result.Value.EventLotDetails;
                }
                else
                {
                    DisplayMessage(resultStatus);
                }
            }
        }

        protected virtual void AddManuallyProduct_DataChanged(object sender, EventArgs e)
        {
            RenderToClient = true;
        }

        //Prevents adding duplicate entries from AddManually tab.
        public virtual void DeleteDuplicateContainers(JQDataGrid affectedMaterialsGrid)
        {
            var currentLotDetails = (EventLotDetail[])affectedMaterialsGrid.Data;
            if (currentLotDetails == null || currentLotDetails.Length < 2)
                return;
            affectedMaterialsGrid.Data = currentLotDetails.Distinct(new LotComparer()).ToArray();

            CamstarWebControl.SetRenderToClient(affectedMaterialsGrid);
        }

        class LotComparer : IEqualityComparer<EventLotDetail>
        {
            public bool Equals(EventLotDetail x, EventLotDetail y)
            {
                if (Object.ReferenceEquals(x, y))
                    return true;
                if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                    return false;
                return x.Lot.Value == y.Lot.Value;
            }

            public int GetHashCode(EventLotDetail obj)
            {
                return base.GetHashCode();
            }
        }

        protected virtual void LoadContainerDetails(EventLotDetail[] lotDetails)
        {
            var fs = FrameworkManagerUtil.GetFrameworkSession();
            var data = new UpdateEvent();
            var serviceDetail = new EventDetail();
            var obj = new WCFObject(serviceDetail);
            var eventDetail = WCFObject.CreateObject(obj.GetFieldType("EventDataDetail")) as EventDataDetails;
            if (eventDetail != null)
            {
                eventDetail.EventLotDetails = lotDetails;
                obj.SetValue("EventDataDetail", eventDetail);
            }
            data.ServiceDetail = serviceDetail;

            var info = new UpdateEvent_Info();
            var serviceDetailInfo = new EventDetail_Info();
            var objInf = new WCFObject(serviceDetailInfo);
            var eventDataDetailsInfo = WCFObject.CreateObject(objInf.GetFieldType("EventDataDetail")) as EventDataDetails_Info;
            if (eventDataDetailsInfo != null)
            {
                eventDataDetailsInfo.EventLotDetails = new EventLotDetail_Info
                {
                    RequestValue = true
                };
                objInf.SetValue("EventDataDetail", eventDataDetailsInfo);
            }
            info.ServiceDetail = serviceDetailInfo;

            UpdateEvent_Request request = new UpdateEvent_Request() { Info = info };
            UpdateEventService service = new UpdateEventService(fs.CurrentUserProfile);
            UpdateEvent_Result result;
            ResultStatus rs = service.GetEnvironment(data, request, out result);
            if (rs.IsSuccess)
            {
                foreach (var detail in lotDetails)
                    detail.IsContainer = result.Value.ServiceDetail.EventDataDetail.EventLotDetails.Where(n => n.Lot == detail.Lot).Select(n => n.IsContainer).FirstOrDefault();
            }
        }

        protected virtual void AddSelectedButton_Click(object sender, EventArgs e)
        {
            if (SearchResultsGrid.GridContext.SelectedRowIDs == null || SearchResultsGrid.GridContext.SelectedRowIDs.Count == 0)
                return;
            var selectedRows = SearchResultsGrid.GridContext.SelectedRowIDs.Select(rowId => SearchResultsGrid.GridContext.GetItem(rowId));
            int newLotsCount = 0;
            var newLotDetails = new List<EventLotDetail>();
            var currentLotDetails = (EventLotDetail[])AffectedMaterialsGrid.Data;

            foreach (DataRow selectedRow in selectedRows)
            {
                //Eliminates possibility to add duplicate entries
                if (currentLotDetails != null && currentLotDetails.FirstOrDefault(currentRef => currentRef.Lot.Value.Equals(selectedRow.Field<String>("Container"))) != null)
                    continue;

                EventLotDetail newEventLotdetail = new EventLotDetail
                {
                    Lot = selectedRow.Field<String>("Container"),
                    ProductName = selectedRow.Field<String>("Product"),
                    ProductDescription = selectedRow.Field<String>("ProductDescription"),
                    UOMName = selectedRow.Field<String>("UOM")
                };
                if (selectedRow.Field<Double?>("Qty") != null)
                    newEventLotdetail.Qty = (Primitive<double>)selectedRow.Field<Double?>("Qty");
                if (selectedRow.Field<Double?>("QtySampled") != null)
                    newEventLotdetail.QtySampled = (Primitive<double>)selectedRow.Field<Double?>("QtySampled");
                if (selectedRow.Field<Double?>("QtyAffected") != null)
                    newEventLotdetail.QtyDefective = (Primitive<double>)selectedRow.Field<Double?>("QtyAffected");

                newLotDetails.Add(newEventLotdetail);
                newLotsCount++;
            }

            LoadContainerDetails(newLotDetails.ToArray());

            if (AffectedMaterialsGrid.Data != null) //Merges arrays data
            {
                var newCrossReferencesGridData =
                    new EventLotDetail[currentLotDetails.Length + newLotsCount];

                Array.Copy(currentLotDetails, newCrossReferencesGridData,
                           currentLotDetails.Length);
                Array.Copy(newLotDetails.ToArray(), 0, newCrossReferencesGridData,
                           currentLotDetails.Length, newLotsCount);

                AffectedMaterialsGrid.Data = newCrossReferencesGridData;
            }
            else
            {
                AffectedMaterialsGrid.Data = newLotDetails.ToArray();
            }

            Page.SessionVariables.SetValueByName("AffectedMaterial", AffectedMaterialsGrid.Data);
            AffectedMaterialsGridAdd.Data = AffectedMaterialsGrid.Data;
            SearchResultsGrid.GridContext.SelectedRowIDs.Clear();

            Control eventLotsGrid = Page.Manager.WebParts.OfType<WebPart>().Where(wp => wp.FindControl("EventLotsGrid") != null)
                .Select(wp => wp.FindControl("EventLotsGrid")).FirstOrDefault();
            if (eventLotsGrid != null && eventLotsGrid as JQDataGrid != null)
            {
                (eventLotsGrid as JQDataGrid).Data = AffectedMaterialsGrid.Data;
            }
        }

        private void RadioControl_CheckedChanged(object sender, EventArgs e)
        {
            if ((bool)(SearchTypeContainersWithMaterial.RadioControl.Checked) || (bool)(SearchTypeContainersOnly.RadioControl.Checked))
            {
                ProductFilter.LabelControl.Text = FrameworkManagerUtil.GetLabelValue("CSICDOName_Product");
            }
            else
            {
                ProductFilter.LabelControl.Text = FrameworkManagerUtil.GetLabelValue("IssueActualsHistory_Product");
            }
        }

        protected virtual void ClearAllButton_Click(object sender, EventArgs e)
        {
            MatrixWebPart amSearchWP = Page.CamstarControls.FindControl("ProductionEventAMSearch_WP1") as MatrixWebPart;
            if (amSearchWP != null)
            {
                amSearchWP.ClearValues(CreateServiceData("UpdateEvent"));
                amSearchWP.ClearValues(CreateServiceData("ContainerInquiry"));
                SearchEmployeeHistory.Data = true;
                SearchResultsGrid.ClearData();
            }
            MatrixWebPart amAddWP = Page.CamstarControls.FindControl("ProductionEventAMAdd_WP") as MatrixWebPart;
            if (amAddWP != null)
            {
                amAddWP.ClearValues(CreateServiceData("UpdateEvent"));
                amAddWP.ClearValues(CreateServiceData("ContainerInquiry"));
            }
        }

        protected virtual ResponseData AffectedMaterialsGrid_DataChanged(object sender, JQGridEventArgs args)
        {
            Page.SessionVariables.SetValueByName("AffectedMaterial", AffectedMaterialsGrid.Data);
            AffectedMaterialsGridAdd.Data = AffectedMaterialsGrid.Data;
            return null;
        }

        protected virtual ResponseData AffectedMaterialsGridAdd_DataChanged(object sender, JQGridEventArgs args)
        {
            Page.SessionVariables.SetValueByName("AffectedMaterial", AffectedMaterialsGridAdd.Data);
            AffectedMaterialsGrid.Data = AffectedMaterialsGridAdd.Data;
            return null;
        }

    }
}
