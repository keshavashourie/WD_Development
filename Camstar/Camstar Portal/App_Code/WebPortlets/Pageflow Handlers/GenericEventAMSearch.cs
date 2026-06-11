// Copyright Siemens 2023  
using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls.WebParts;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.WebPortlets;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.FormsFramework.WebControls;
using System.Data;
using System.Linq;
using System.Collections;
using System.Web;
using Camstar.WCF.ObjectStack;
using Action = Camstar.WCF.ObjectStack.Action;
using Camstar.WebPortal.FormsFramework;
using Camstar.WCF.Services;
using Camstar.WebPortal.WCFUtilities;

namespace Camstar.WebPortal.WebPortlets
{

    public class GenericEventAMSearch : MatrixWebPart
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            AddSelectedButton.Click += new EventHandler(AddSelectedButton_Click);
            ClearAllButton.Click += ClearAllButton_Click;

            if (UpdateAffectedMaterial != null)
                UpdateAffectedMaterial.Click += UpdateAffectedMaterialOnClick;

            AddManuallyUpdateButton.Hidden = !AddManuallyButton.Hidden;

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
            //Using for Shopfloor Action. GE have only Affected Material Tab 
            Page.DataContract.SetValueByName("SelectedTabsForAction", "Affected Material");
            if (TabContainer!=null && TabContainer.SelectedItem.Name == "AffectedMaterial"
                && Page.SessionVariables["AffectedMaterial"] == null)
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
                            Container = new Info(true),
                            IsContainer = new Info(true),
                            ProductRev = new Info(true),
                            EventLot = new Info(true)
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

        protected virtual void ClearAllButton_Click(object sender, EventArgs e)
        {
            MatrixWebPart amSearchWP = Page.CamstarControls.FindControl("GenericEventAMSearch_WP") as MatrixWebPart;
            if (amSearchWP != null)
            {
                amSearchWP.ClearValues(CreateServiceData("CreateEvent"));
                amSearchWP.ClearValues(CreateServiceData("ContainerInquiry"));
            }

            MatrixWebPart amAddWP = Page.CamstarControls.FindControl("GenericEventAMAdd_WP") as MatrixWebPart;
            if (amAddWP != null)
            {
                amAddWP.ClearValues(CreateServiceData("CreateEvent"));
                amAddWP.ClearValues(CreateServiceData("ContainerInquiry"));
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
                            Container = new Info(true),
                            IsContainer = new Info(true),
                            ProductRev = new Info(true),
                            EventLot = new Info(true)
                        }
                    }
                };

                var result = new UpdateEventLots_Result();

                ResultStatus resultStatus = service.Load(serviceData, request, out result);

                if (resultStatus != null && resultStatus.IsSuccess)
                {
                    AffectedMaterialsGrid.Data = result.Value.EventLotDetails;
                    Page.SessionVariables["AffectedMaterial"] = result.Value.EventLotDetails;
                }
                else
                {
                    DisplayMessage(resultStatus);
                }
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
                var qty = selectedRow.Field<Double?>("Qty");
                var uom = selectedRow.Field<String>("UOM");
                //Eliminates possibility to add elements without Qty and UOM fields
                if (String.IsNullOrEmpty(uom) || qty == null)
                    continue;
                EventLotDetail newEventLotdetail = new EventLotDetail
                {
                    Lot = selectedRow.Field<String>("Container"),
                    ProductName = selectedRow.Field<String>("Product"),
                    ProductDescription = selectedRow.Field<String>("ProductDescription"),
                    Qty = qty,
                    UOMName = uom
                };
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

            SearchResultsGrid.GridContext.SelectedRowIDs.Clear();
            Page.SessionVariables.SetValueByName("AffectedMaterial", AffectedMaterialsGrid.Data);
        }

        public override void GetInputData(Service serviceData)
        {
            base.GetInputData(serviceData);

            if (serviceData is UpdateEvent)
            {
                var lotDetails = AffectedMaterialsGrid.Data as EventLotDetail[];

                if (lotDetails != null && lotDetails.Length > 0)
                {
                    if (((UpdateEvent) serviceData).ServiceDetail == null)
                    {
                        var serviceDetail = new EventDetail();
                        var obj = new WCFObject(serviceDetail);
                        var eventDetail = WCFObject.CreateObject(obj.GetFieldType("EventDataDetail")) as EventDataDetails;
                        if (eventDetail != null)
                        {
                            obj.SetValue("EventDataDetail", eventDetail);
                        }
                        ((UpdateEvent)serviceData).ServiceDetail = serviceDetail;
                    }
                    var eventLotDetailsCount = lotDetails.Length;
                    var eventLotDetails = (Page.SessionVariables.GetValueByName("AffectedMaterial")) as EventLotDetail[];
                    for (int i = 0; i < eventLotDetailsCount; i++)
                    {
                        eventLotDetails[i] = new EventLotDetail
                        {
                            Lot = lotDetails[i].Lot,
                            ProductName = lotDetails[i].ProductName,
                            ProductDescription = lotDetails[i].ProductDescription,
                            Qty = lotDetails[i].Qty,
                            UOMName = lotDetails[i].UOMName,
                            ReferenceDesignator = lotDetails[i].ReferenceDesignator
                        };
                        if (!lotDetails[i].QtySampled.IsNullOrEmpty())
                            eventLotDetails[i].QtySampled = lotDetails[i].QtySampled;
                        if (!lotDetails[i].QtyDefective.IsNullOrEmpty())
                            eventLotDetails[i].QtyDefective = lotDetails[i].QtyDefective;
                    }
                }
            }
        }


        #region Properties

        protected virtual JQTabContainer TabContainer { get { return Page.FindCamstarControl("Tabs") as JQTabContainer; } }
        protected virtual Camstar.WebPortal.FormsFramework.WebControls.NamedObject InstanceID
        {
            get { return Page.FindCamstarControl("InstanceID") as Camstar.WebPortal.FormsFramework.WebControls.NamedObject; }
        }

        protected virtual JQDataGrid SearchResultsGrid
        {
            get { return Page.FindCamstarControl("AMSearchResultsGrid") as JQDataGrid; }
        }

        protected virtual JQDataGrid AffectedMaterialsGrid
        {
            get { return Page.FindCamstarControl("AffectedMaterialsGrid") as JQDataGrid; }
        }

        protected virtual Button AddSelectedButton
        {
            get { return Page.FindCamstarControl("AddSelectedButton") as Button; }
        }

        protected virtual Button ClearAllButton
        {
            get { return Page.FindCamstarControl("ClearALLButton") as Button; }
        }

        protected virtual Button AddManuallyButton
        {
            get { return Page.FindCamstarControl("AddManuallyButton") as Button; }
        }

        protected virtual Button AddManuallyUpdateButton
        {
            get { return Page.FindCamstarControl("AddManuallyUpdateButton") as Button; }
        }

        protected virtual Button UpdateAffectedMaterial
        {
            get { return Page.FindCamstarControl("UpdateAffectedMaterial") as Button; }
        }

        #endregion
    }
}
