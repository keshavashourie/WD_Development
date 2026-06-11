// Copyright Siemens 2023  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using System.Data;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.PortalFramework;

namespace Camstar.WebPortal.WebPortlets
{
    public class ProductionEventCrossReferencesR2 : MatrixWebPart
    {
        #region Properties
        protected virtual JQTabContainer TabContainer
        {
            get { return Page.FindCamstarControl("Tabs") as JQTabContainer; }
        }
        protected virtual Button UpdateCrossRefs
        {
            get { return Page.FindCamstarControl("UpdateCrossReferences") as Button; }
        }
        protected virtual Button AddReferenceButton
        {
            get { return Page.FindCamstarControl("AddReferenceButton") as Button; }
        }
        protected virtual Button ClearAllButton
        {
            get { return Page.FindCamstarControl("ClearAllButton") as Button; }
        }
        protected virtual JQDataGrid CrossReferencesGrid
        {
            get { return Page.FindCamstarControl("CrossReferencesGrid") as JQDataGrid; }
        }
        protected virtual JQDataGrid SearchResultsGrid
        {
            get { return Page.FindCamstarControl("XRefsSearchResultsGrid") as JQDataGrid; }
        }
        protected virtual NamedObject InstanceId
        {
            get { return Page.FindCamstarControl("InstanceID") as NamedObject; }
        }
        protected virtual Button ViewSearchDetailsButton
        {
            get { return Page.FindCamstarControl("ViewSearchDetailsButton") as Button; }
        }
        protected virtual Button ViewXRefsDetailsButton
        {
            get { return Page.FindCamstarControl("ViewXRefsDetailsButton") as Button; }
        }
        #endregion

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            UpdateCrossRefs.Click += UpdateCrossRefs_Click;
            ClearAllButton.Click += ClearAllButton_Click;
            AddReferenceButton.Click += AddReferenceButton_Click;
            ViewSearchDetailsButton.Click += OpenFloatPEManageFromSearch_Click;
            ViewXRefsDetailsButton.Click += OpenFloatPEManageFromXRefs_Click;
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (TabContainer.SelectedItem.Name == "Cross References" && Page.SessionVariables.GetValueByName("AddRefToGrid") == null)
                LoadEventCrossRefs();
        }

        private void UpdateCrossRefs_Click(object sender, EventArgs e)
        {
            var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            if (session != null)
            {
                var service = new UpdateEventCrossRefsService(session.CurrentUserProfile);
                var serviceData = new UpdateEventCrossRefs();
                serviceData.QualityESigDetail = ESigCaptureUtil.CollectQualityESigDetail();
                serviceData.QualityObject = new NamedObjectRef() { CDOTypeName = "Event", Name = InstanceId.Data.ToString() };
                var crossReferences = CrossReferencesGrid.Data as QualityCrossReferenceDetail[];
                if (crossReferences != null)
                    serviceData.QualityCrossReferences = (from cr in crossReferences
                                                          select new QualityCrossReferenceDetail
                                                          {
                                                              TrackingId = new NamedObjectRef { Name = InstanceId.Data.ToString(), CDOTypeName = "Event" },
                                                              CrossReference = cr.QualityObject
                                                          }
                                ).ToArray();

                var request = new UpdateEventCrossRefs_Request();
                var result = new UpdateEventCrossRefs_Result();

                ResultStatus resultStatus = service.ExecuteTransaction(serviceData, request, out result);

                if (resultStatus != null && resultStatus.IsSuccess)
                {
                    DisplayMessage(resultStatus);
                }
                else
                {
                    DisplayMessage(resultStatus);
                }
                ESigCaptureUtil.CleanQualityESigCaptureDM();
                Page.SessionVariables.SetValueByName("AddRefToGrid", null);
            }
        }

        protected virtual void LoadEventCrossRefs()
        {
            var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            if (session != null)
            {
                var service = new UpdateEventCrossRefsService(session.CurrentUserProfile);
                var serviceData = new UpdateEventCrossRefs();

                serviceData.QualityObject = new NamedObjectRef() { CDOTypeName = "Event", Name = InstanceId.Data.ToString() };

                var request = new UpdateEventCrossRefs_Request()
                {
                    Info = new UpdateEventCrossRefs_Info()
                    {
                        QualityObject = new Info(true),
                        QualityCrossReferences = new QualityCrossReferenceDetail_Info() { RequestValue = true }
                    }
                };

                var result = new UpdateEventCrossRefs_Result();

                ResultStatus resultStatus = service.Load(serviceData, request, out result);

                if (resultStatus != null && resultStatus.IsSuccess)
                {
                    CrossReferencesGrid.Data = result.Value.QualityCrossReferences;
                    CrossReferencesGrid.DataBind();
                }
                else
                {
                    DisplayMessage(resultStatus);
                }
            }
        }

        public virtual void ClearAllButton_Click(object sender, EventArgs e)
        {
            MatrixWebPart searchWP = Page.CamstarControls.FindControl("ProductionEventXRefs_WP12") as MatrixWebPart;
            if (searchWP != null)
            {
                searchWP.ClearValues(CreateServiceData("QualityObjectInquiry"));
                searchWP.ClearValues(CreateServiceData("UpdateEvent"));
            }
            MatrixWebPart gridWP = Page.CamstarControls.FindControl("ProductionEventXRefs_WP2") as MatrixWebPart;
            if (gridWP != null)
            {
                gridWP.ClearValues(CreateServiceData("QualityObjectInquiry"));
                gridWP.ClearValues(CreateServiceData("UpdateEvent"));
            }
        }

        public virtual void AddReferenceButton_Click(object sender, EventArgs e)
        {
            if (SearchResultsGrid.GridContext.SelectedRowIDs == null || SearchResultsGrid.GridContext.SelectedRowIDs.Count == 0)
                return;

            var selectedRows = (SearchResultsGrid.GridContext as SelValGridContext).GetSelectedItems(false);
            int newReferencesCount = 0;
            var newReferences = new List<QualityCrossReferenceDetail>();
            var currentCrossReferencesGridData = (QualityCrossReferenceDetail[])CrossReferencesGrid.Data;
            var utcOffset = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session).CurrentUserProfile.UTCOffset;

            foreach (DataRow selectedRow in selectedRows)
            {
                //Eliminates possibility to add duplicate entries
                if (currentCrossReferencesGridData != null && currentCrossReferencesGridData.FirstOrDefault(currentRef => currentRef.QualityObject.Name.Equals(selectedRow.Field<String>("Name"))) != null)
                    continue;

                //Constaructs new entries for CrossReferencesGrid
                QualityObjectStatusDetail newQualityObject = new QualityObjectStatusDetail
                {
                    CategoryName = selectedRow.Field<String>("CategoryName"),
                    StatusName = selectedRow.Field<String>("StatusName"),
                    ClassificationName = selectedRow.Field<String>("ClassificationName"),
                    SubclassificationName = selectedRow.Field<String>("SubClassificationName"),
                    PriorityLevel = new NamedObjectRef { Name = selectedRow.Field<String>("PriorityLevel") },
                    IsCARRequiredToClose = selectedRow.Field<String>("IsCARRequiredToCloseYesNoLabel").Equals("Yes"),
                    Owner = new NamedObjectRef { Name = selectedRow.Field<String>("Owner") }
                };
                if (selectedRow.Field<DateTime?>("ReportedDateGMT") != null)
                    newQualityObject.ReportedDateGMT = selectedRow.Field<DateTime>("ReportedDateGMT").Subtract(utcOffset);
                if (selectedRow.Field<DateTime?>("OccurrenceDateGMT") != null)
                    newQualityObject.OccurrenceDateGMT = selectedRow.Field<DateTime>("OccurrenceDateGMT").Subtract(utcOffset);

                newReferences.Add(new QualityCrossReferenceDetail
                {
                    QualityObject = new NamedObjectRef(selectedRow.Field<String>("Name"), selectedRow.Field<String>("CDOName")),
                    QualityObjectDetail = newQualityObject
                });
                newReferencesCount++;
            }
            if (newReferencesCount != 0)
            {
                if (CrossReferencesGrid.Data != null) //Merges arrays data
                {
                    var newCrossReferencesGridData = new QualityCrossReferenceDetail[currentCrossReferencesGridData.Length + newReferencesCount];

                    Array.Copy(currentCrossReferencesGridData, newCrossReferencesGridData,
                               currentCrossReferencesGridData.Length);
                    Array.Copy(newReferences.ToArray(), 0, newCrossReferencesGridData,
                               currentCrossReferencesGridData.Length, newReferencesCount);

                    CrossReferencesGrid.Data = newCrossReferencesGridData;
                }
                else
                    CrossReferencesGrid.Data = newReferences.ToArray();

                SearchResultsGrid.GridContext.SelectedRowIDs.Clear();
                Page.SessionVariables.SetValueByName("AddRefToGrid", true);
            }
        }

        public virtual void OpenFloatPEManageFromSearch_Click(object sender, EventArgs e)
        {
            if (SearchResultsGrid.GridContext.SelectedRowIDs == null || SearchResultsGrid.GridContext.SelectedRowIDs.Count != 1)
                return;
            var selectedRowId = SearchResultsGrid.GridContext.SelectedRowIDs[0];
            var selectedEventType = (SearchResultsGrid.GridContext.GetItem(selectedRowId) as DataRow).Field<String>("CategoryName");
            var managePageToOpen = selectedEventType.Equals("Nonconformance") ? "ProductionEventManage_VPR2" : "GenericEventManage_VP";
            var floatPageOpenAction = ViewSearchDetailsButton.DefaultAction as FloatPageOpenAction;
            if (floatPageOpenAction == null)
                return;
            floatPageOpenAction.PageName = managePageToOpen;
            Page.DataContract.SetValueByName("PopupQualityObject", selectedRowId);
        }

        public virtual void OpenFloatPEManageFromXRefs_Click(object sender, EventArgs e)
        {
            if (CrossReferencesGrid.GridContext.SelectedRowIDs == null || CrossReferencesGrid.GridContext.SelectedRowIDs.Count != 1)
                return;
            var selectedRowId = CrossReferencesGrid.GridContext.SelectedRowIDs[0];
            var selectedEventType = (CrossReferencesGrid.GridContext.GetItem(selectedRowId) as QualityCrossReferenceDetail).QualityObjectDetail.CategoryName.Value;
            var managePageToOpen = selectedEventType.Equals("Nonconformance") ? "ProductionEventManage_VPR2" : "GenericEventManage_VP";
            var floatPageOpenAction = ViewXRefsDetailsButton.DefaultAction as FloatPageOpenAction;
            if (floatPageOpenAction == null)
                return;
            floatPageOpenAction.PageName = managePageToOpen;
            Page.DataContract.SetValueByName("PopupQualityObject", selectedRowId);
        }
    }
}
