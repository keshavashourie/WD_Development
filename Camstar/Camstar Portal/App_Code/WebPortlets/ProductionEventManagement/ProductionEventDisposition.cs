// Copyright Siemens 2024
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework;
using System.Text;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.Tools.ASPXConverter;

namespace Camstar.WebPortal.WebPortlets
{
    public class ProductionEventDisposition : MatrixWebPart
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            ApplyToContainersButton.Click += new EventHandler(ApplyToContainersButton_Click);
            UpdateDispositions.Click += UpdateDispositions_Click;

            //For Shopfloor Actions
            Page.DataContract.SetValueByName("DispositionContainers", null);
            Page.DataContract.SetValueByName("DispositionSplitOrScrapContainer", null);
            var containers = EventLotsGrid.GridContext.GetSelectedItems(false);
            if (containers != null)
            {
                Page.DataContract.SetValueByName("DispositionContainers", containers.Cast<EventLotDetail>().ToArray());
                if (containers.Length > 0)
                {
                    var containerName = containers.Cast<EventLotDetail>().ToArray()[0].Lot;
                    Page.DataContract.SetValueByName("DispositionSplitOrScrapContainer", new ContainerRef(containerName.ToString()));
                }

            }
        }

        private void LoadDisposition()
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
                            FailureModes = new Info(true),
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
                                    FailureMode = new Info(true, true),
                                    FailureModeGroup = new Info(true),
                                    FailureSeverity = new Info(true),
                                    FailureType = new Info(true),
                                    Description = new Info(true)
                                },
                            }
                        }
                    }
                };

                var result = new UpdateEventLots_Result();

                ResultStatus resultStatus = service.Load(serviceData, request, out result);

                if (resultStatus != null && resultStatus.IsSuccess)
                {
                    EventLotsGrid.Data = result.Value.EventLotDetails;
                    Page.SessionVariables["AffectedMaterial"] = result.Value.EventLotDetails;

                }
                else
                {
                    DisplayMessage(resultStatus);
                }
            }
        }

        private void UpdateDispositions_Click(object sender, EventArgs e)
        {
            var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            if (session != null)
            {
                var service = new UpdateEventDispositionsService(session.CurrentUserProfile);
                var serviceData = new UpdateEventDispositions();
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
                        QtyPendingDisp = am.QtyPendingDisp,
                        EventLot = am.EventLot,
                        IsContainer = am.IsContainer,
                        EventDispositionDetails = am.EventDispositionDetails != null ? am.EventDispositionDetails.Select(d => new EventDispositionDetail
                        {
                            EventDisposition = d.EventDisposition,
                            Disposition = d.Disposition,
                            Qty = d.Qty,
                            Comments = d.Comments,
                            ApplyEntireQty = d.ApplyEntireQty,
                            EventFailureDetails = d.EventFailureDetails != null ? d.EventFailureDetails.Select(f => new EventFailureDetail
                            {
                                EventFailure = f.EventFailure,
                                FailureMode = f.FailureMode,
                                Description = f.Description,
                                FailureSeverity = f.FailureSeverity,
                                FailureType = f.FailureType
                            }).ToArray() : null
                        }).ToArray() : null,
                    }).ToArray();
                }


                var request = new UpdateEventDispositions_Request()
                {
                    Info=new UpdateEventDispositions_Info()
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
                            FailureModes = new Info(true),
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
                                    FailureMode = new Info(true, true),
                                    FailureModeGroup = new Info(true),
                                    FailureSeverity = new Info(true),
                                    FailureType = new Info(true),
                                    Description = new Info(true)
                                },
                            }
                        }
                    }
                };
                var result = new UpdateEventDispositions_Result();

                ResultStatus resultStatus = service.ExecuteTransaction(serviceData, request, out result);

                if (resultStatus != null && resultStatus.IsSuccess)
                {
                    DisplayMessage(resultStatus);
                    Page.SessionVariables["AffectedMaterial"] = result.Value.EventLotDetails;
                }
                else
                {
                    DisplayMessage(resultStatus);
                }
                ESigCaptureUtil.CleanQualityESigCaptureDM();
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            Page.DataContract.SetValueByName("SelectedTabsForAction", Tabs.SelectedItem.HeaderText);
            DispositionQty.Hidden = EntireQty.RadioControl.Checked;
            if (FailureMode.ReadOnly)
            {
                ApplyToDispositionsButton.Visible = false;
            }
            if (TabContainer.SelectedItem.Name == "Disposition" && Page.SessionVariables["AffectedMaterial"] == null)
                LoadDisposition();
        }

        //adds new item to the Dispositions grid
        protected virtual void ApplyToContainersButton_Click(object sender, EventArgs e)
        {
            var labelCache = LabelCache.GetRuntimeCacheInstance();

            var eventLotsGridData = EventLotsGrid.Data as EventLotDetail[];
            if (eventLotsGridData == null || EventLotsGrid.GridContext.SelectedRowIDs == null || EventLotsGrid.GridContext.SelectedRowIDs.Count == 0)
            {
                EventLotsGrid.GridContext.BubbleMessage = labelCache.GetLabelByName("Lbl_GridNavAlertText").Value;
                return;
            }

            var message = new StringBuilder();
            if (String.IsNullOrEmpty(DispositionQty.TextControl.Text) && !EntireQty.RadioControl.Checked)
                message.Append(labelCache.GetLabelByName("Lbl_FieldDispositionQtyRequired").Value);
            if (String.IsNullOrEmpty(Disposition.TextEditControl.Text))
                message.Append(labelCache.GetLabelByName("Lbl_FieldDispositionRequired").Value);
            if (message.Length > 0)
            {
                Page.DisplayWarning(message.ToString());
                return;
            }
            var newDisposition = new EventDispositionDetail
            {
                Disposition = new NamedObjectRef(Disposition.TextEditControl.Text),
                Comments = Comments.TextControl.Text,
                ApplyEntireQty = EntireQty.RadioControl.Checked
            };
            double qty;
            if (!newDisposition.ApplyEntireQty.Value && Double.TryParse(DispositionQty.TextControl.Text, out qty))
                newDisposition.Qty = qty;

            EventLotsGrid.Data = eventLotsGridData;

            foreach (var id in EventLotsGrid.GridContext.SelectedRowIDs)
            {
                var currentLot = EventLotsGrid.GridContext.GetItem(id) as EventLotDetail;
                if (currentLot == null)
                    continue;

                // Fix for CPR 311012:PR309567 TAC10505919 -Assigning a disposition on a container with no quantity returns a null pointer exception
                // If container qty is null consider 0 for further deposition
                double currentLotQty = currentLot.Qty?.Value ?? 0;

                //adding Clone to avoid duplicating refrences to the object.
                newDisposition = (EventDispositionDetail)newDisposition.Clone();
                if (newDisposition.ApplyEntireQty.Value)
                    newDisposition.Qty = currentLotQty;

                var selectedLotDispositions = currentLot.EventDispositionDetails;
                if (selectedLotDispositions != null)
                {
                    var newDispositions = new EventDispositionDetail[selectedLotDispositions.Length + 1];
                    Array.Copy(selectedLotDispositions, newDispositions, selectedLotDispositions.Length);
                    newDispositions[newDispositions.Length - 1] = newDisposition;

                    currentLot.EventDispositionDetails = newDispositions;
                    var dispositionedQty = newDispositions.Sum(disp => disp.Qty.Value);
                    currentLot.QtyPendingDisp = currentLotQty - dispositionedQty;
                }
                else
                {
                    currentLot.EventDispositionDetails = new[] { newDisposition };
                    currentLot.QtyPendingDisp = currentLotQty - newDisposition.Qty.Value;
                }

                var state = new ClientGridState()
                {
                    ContextID = EventDispositionsGrid.GridContext.ContextID,
                    CallStackKey = EventDispositionsGrid.GridContext.CallStackKey,
                    RowID = id,
                    RowDataObject = EventDispositionsGrid.GridContext.ContextID
                };
                EventLotsGrid.GridContext.ExpandRow(state);
            }
        }

        public override void RequestValues(Info serviceInfo, Service serviceData)
        {
            base.RequestValues(serviceInfo, serviceData);
            if (serviceData.GetType().Name == "UpdateEvent")
            {
                var info = (UpdateEvent_Info)serviceInfo;
                if (info.ServiceDetail != null)
                    info.ServiceDetail.EventDataDetail.EventLotDetails.QtyPendingDisp = new Info(true);
            }
        }

        #region Properties

        protected virtual JQTabContainer TabContainer { get { return Page.FindCamstarControl("Tabs") as JQTabContainer; } }

        protected virtual Button UpdateDispositions
        {
            get { return Page.FindCamstarControl("UpdateDispositions") as Button; }
        }
        protected virtual Camstar.WebPortal.FormsFramework.WebControls.NamedObject InstanceID
        {
            get { return Page.FindCamstarControl("InstanceID") as Camstar.WebPortal.FormsFramework.WebControls.NamedObject; }
        }
        protected virtual JQDataGrid EventLotsGrid
        {
            get { return Page.FindCamstarControl("EventLotsGrid") as JQDataGrid; }
        }
        protected virtual JQDataGrid EventDispositionsGrid
        {
            get { return Page.FindCamstarControl("EventDispositionsGrid") as JQDataGrid; }
        }

        protected virtual Button ApplyToContainersButton
        {
            get { return Page.FindCamstarControl("ApplyToContainersButton") as Button; }
        }
        protected virtual TextBox DispositionQty
        {
            get { return Page.FindCamstarControl("DispositionQty") as TextBox; }
        }
        protected virtual RadioButton EntireQty
        {
            get { return Page.FindCamstarControl("EntireQty") as RadioButton; }
        }
        protected virtual NamedObject Disposition
        {
            get { return Page.FindCamstarControl("Disposition") as NamedObject; }
        }
        protected virtual TextBox Comments
        {
            get { return Page.FindCamstarControl("Comments") as TextBox; }
        }

        protected virtual NamedObject FailureMode
        {
            get { return Page.FindCamstarControl("FailureMode") as NamedObject; }
        }

        protected virtual Button ApplyToDispositionsButton
        {
            get { return Page.FindCamstarControl("ApplyToDispositionsButton") as Button; }
        }
        protected virtual JQTabContainer Tabs
        {
            get { return Page.FindCamstarControl("Tabs") as JQTabContainer; }
        }
        #endregion
    }
}
