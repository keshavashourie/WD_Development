// Copyright Siemens 2023  
using System;
using System.Linq;
using System.Configuration;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.FormsFramework.HtmlControls;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WCF.ObjectStack;

namespace Camstar.WebPortal.WebPortlets.Concierge
{
    public class ConciergeItem
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the TaskItem class.
        /// </summary>
        public ConciergeItem()
        {
            Initialize();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="detail"></param>
        public ConciergeItem(ToDoListDetail detail)
            : this()
        {
            mToDoListDetail = detail;
            LoadDetail();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns the Task Item in a TableRow.
        /// </summary>
        /// <returns></returns>
        public virtual TableRow ToTableRow(bool isShown)
        {
            TableRow row = new TableRow();
            TableCell cell = new TableCell();

            cell = new TableCell();
            cell.Controls.Add(this.TaskItemLink);
            cell.Attributes.Add("priority", GetPriorityAttribute());
            row.Cells.Add(cell);

            if (!isShown)
            {
                row.CssClass = CSSConstants.TaskItemHidden;
            }

            return row;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the value the indicates the ToDoListDetail object for this ConciergeItem.
        /// </summary>
        public virtual ToDoListDetail Detail
        {
            get { return mToDoListDetail; }
            set { mToDoListDetail = value; }
        }

        public virtual System.Collections.Hashtable GlobalCallStackSession
        {
            get
            {
                return new Camstar.WebPortal.FormsFramework.CallStack().GlobalCallStackSession;
            }
        }

        /// <summary>
        /// Gets or sets the value that indicates the ToDoListEntryType of this ConciergeItem.
        /// (i.e. Ownership, WorkItem)
        /// </summary>
        public virtual NotificationTypeEnum ToDoListEntryType
        {
            get { return mToDoListEntryType; }
            set { mToDoListEntryType = value; }
        }

        /// <summary>
        /// Gets or sets the value the indicate the ToDoListItemType of this ConciergeItem.
        /// (ProcessModel = 7693, Phase = 7695, Plan = 7696, Activity = 7694, Event = 7489, CAPA = 7647) 
        /// </summary>
        public virtual int ToDoListItemType { get; set; }

        /// <summary>
        /// Gets or sets the value that indicates the Name of the TaskItem.
        /// </summary>
        public virtual string Name
        {
            get { return mName; }
            set { mName = value; }
        }

        public virtual int YellowMinRange
        {
            get { return mYellowMinRange; }
            set { mYellowMinRange = value; }
        }

        public virtual int YellowMaxRange
        {
            get { return mYellowMaxRange; }
            set { mYellowMaxRange = value; }
        }

        /// <summary>
        /// Gets the description of the ToDoListItemType of this ConciergeItem.
        /// </summary>
        public virtual string ToDoListItemTypeDescription
        {
            get
            {
                switch (ToDoListItemType)
                {
                    case ToDoListItemTypeIDs.ProcessModel:
                        return ToDoListItemTypeDescriptions.ProcessModel;
                    case ToDoListItemTypeIDs.Phase:
                        return ToDoListItemTypeDescriptions.Phase;
                    case ToDoListItemTypeIDs.Plan:
                        return ToDoListItemTypeDescriptions.Plan;
                    case ToDoListItemTypeIDs.Activity:
                        return ToDoListItemTypeDescriptions.Activity;
                    case ToDoListItemTypeIDs.Event:
                        return ToDoListItemTypeDescriptions.Event;
                    case ToDoListItemTypeIDs.CAPA:
                        return ToDoListItemTypeDescriptions.CAPA;
                    default: return string.Empty;
                }
            }
        }

        /// <summary>
        /// Gets or sets the value that indicates the PageURL where the 
        /// ConciergeItem is to be performed.
        /// </summary>
        public virtual string PageURL
        {
            get { return GetPageURL(); }
            set { mPageURL = value; }
        }

        /// <summary>
        /// Gets or sets the value that indicates the DueDate in GMT format
        /// for which the ConciergeItem should be performed by.
        /// </summary>
        public virtual DateTime DueDate
        {
            get { return mDueDate; }
            set { mDueDate = value; }
        }

        /// <summary>
        /// Gets the international formatted string representation 
        /// of DueDateGMT in user's time zone. 
        /// </summary>
        public virtual string FormattedDueDate
        {
            get { return GetFormattedDueDate(); }
        }

        /// <summary>
        /// Gets the PriorityImage based on the due date of the ConciergeItem
        /// Red Square: Overdue or due now; 
        /// Yellow Triangle: Due in 2-5 business days
        /// Green Circle: Due in over 5 business days
        /// </summary>
        public virtual Image PriorityImage
        {
            get { return GetPriorityImage(); }
        }

        /// <summary>
        /// Gets the LinkButton that will be used for this ConciergeItem.
        /// </summary>
        public virtual LinkButton TaskItemLink
        {
            get { return GetTaskItemLink(); }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Initializes all private member variables
        /// </summary>
        protected virtual void Initialize()
        {
            mToDoListDetail = null;

            mToDoListEntryType = 0;
            mMessage = string.Empty;
            mCategory = 0;

            mConciergeItemLinkButton = new LinkButton();
            mPriorityImage = new Image();

            mPriorityStatus = PriorityStatusEnum.FutureDue;//green
        }

        /// <summary>
        /// Loads all private member varaiables with the values of the ToDoListDetail object.
        /// </summary>
        protected virtual void LoadDetail()
        {
            mToDoListEntryType = (NotificationTypeEnum)mToDoListDetail.NotificationType;
            mDueDate = mToDoListDetail.DueDate != null ? (DateTime)mToDoListDetail.DueDate : new DateTime();
            mMessage = mToDoListDetail.DisplayMessage != null ? (string)mToDoListDetail.DisplayMessage : string.Empty;
            mCategory = (CategoryEnum)mToDoListDetail.Category;
            mName = mToDoListDetail.Name != null ? (string)mToDoListDetail.Name : string.Empty;
            mPriorityStatus = (PriorityStatusEnum)mToDoListDetail.PriorityStatus;
        }

        /// <summary>
        /// Returns the international formatted string representation of the Task Item's
        /// due date in the user's time zone.
        /// </summary>
        /// <returns>System.String</returns>
        protected virtual string GetFormattedDueDate()
        {
            //TODO: Convert GMT time to user's time zone
            return mDueDate.ToString(ConciergeConstants.InternationalFormat);
        }

        /// <summary>
        /// Returns an Image based on the priority of the Task Item's due date.
        /// </summary>
        /// <returns>System.Web.UI.WebControls.Image</returns>
        protected virtual Image GetPriorityImage()
        {
            mPriorityImage = new Image();
            if (ToDoListEntryType == NotificationTypeEnum.QualityRecPendingAssignment)
            {
                mPriorityImage.ImageUrl = Images.GreenSquare;
            }
            else
            {
                //TODO: Convert GMT time to user's time zone.
                DateTime dueDate = mDueDate;

                if (dueDate.Year == 1)//Duedate from server is empty
                {
                    mPriorityImage.ImageUrl = Images.GreenSquare;
                }
                else
                {
                    GetYelloMaxMinRangeValue();
                    TimeSpan timespanDiff = dueDate - DateTime.Now;
                    TimeSpan timespanYyellowMin = new TimeSpan(mYellowMinRange, 0, 0, 0);
                    TimeSpan timespanYyellowMax = new TimeSpan(mYellowMaxRange, 0, 0, 0);
                    
                    if (timespanDiff < timespanYyellowMin)
                        mPriorityImage.ImageUrl = Images.RedSquare;
                    else if (timespanDiff > timespanYyellowMax)
                        mPriorityImage.ImageUrl = Images.GreenSquare;
                    else
                        mPriorityImage.ImageUrl = Images.YellowTriange;
                }

                if (dueDate.Year != 1)
                {
                    mPriorityImage.ToolTip = string.Format("{0:dd | MMM | yyyy}", dueDate);
                }
            }
            return mPriorityImage;
        }

        public virtual string GetPriorityAttribute()
        {
            string priority = string.Empty;

            if (ToDoListEntryType == NotificationTypeEnum.QualityRecPendingAssignment)
            {
                priority = "green"; 
            }
            else
            {
                if (mPriorityStatus == PriorityStatusEnum.FutureDue)
                    priority = "green";
                else if (mPriorityStatus == PriorityStatusEnum.NowDue)
                    priority = "yellow";
                else if (mPriorityStatus == PriorityStatusEnum.PastDue)
                    priority = "red";

                if (mDueDate.Year != 1)
                {
                    mPriorityImage.ToolTip = string.Format("{0:dd | MMM | yyyy}", mDueDate);
                }
            }
            return priority;
        }

        protected virtual void GetYelloMaxMinRangeValue()
        {
            ToDoListCache todoListCache = HttpContext.Current.Session[SessionConstants.ToDoListCache] as ToDoListCache;
            if (todoListCache.PortalCategories != null && todoListCache.PortalCategories.Rows != null && todoListCache.PortalCategories.Rows.Length > 0)
            {
                var headers = todoListCache.PortalCategories.Headers;

                int minIndex = headers.ToList().IndexOf(headers.Where(header => header.Name.Equals(YellowMinRangeColumn)).FirstOrDefault());
                int maxIndex = headers.ToList().IndexOf(headers.Where(header => header.Name.Equals(YellowMaxRangeColumn)).FirstOrDefault());
                
                mYellowMinRange = int.Parse(todoListCache.PortalCategories.Rows[0].Values[minIndex]);
                mYellowMaxRange = int.Parse(todoListCache.PortalCategories.Rows[0].Values[maxIndex]);
            }
            else
            {
                mYellowMinRange = ConciergeConstants.YellowMinRange;
                mYellowMaxRange = ConciergeConstants.YellowMaxRange;
            }

        }

        /// <summary>
        /// Returns the LinkButton with text formatted based on the TaskItemType and MaxLength.
        /// </summary>
        /// <returns>System.Web.UI.WebControls.LinkButton</returns>
        protected virtual LinkButton GetTaskItemLink()
        {
            string pageUrl = GetPageURL();
            mConciergeItemLinkButton = new LinkButton();
            mConciergeItemLinkButton.Text = mMessage.Length > 40 ? mMessage.Remove(40) + "..." : mMessage;
            mConciergeItemLinkButton.ToolTip = mMessage;
            if (pageUrl != null)
                mConciergeItemLinkButton.OnClientClick = string.Format("{0};return false;", pageUrl);

            return mConciergeItemLinkButton;
        }

        /// <summary>
        /// Returns the page url with query string for where the ConciergeItem is to be performed.
        /// </summary>
        /// <returns></returns>
        protected virtual string GetPageURL()
        {
            Camstar.WebPortal.PortalFramework.PageRoutingCache cache = new PortalFramework.PageRoutingCache();
            Personalization.PageMappingItem item = cache.GetItem(Detail.ToDoListItemType.ToString());
            if (item != null)
            {
                Camstar.WebPortal.FormsFramework.PortalContextBase context = new FormsFramework.PortalContextBase();
                context.DataContract = new Personalization.UIComponentDataContract();
                context.DataContract.DataMembers = typeof(ToDoListDetail).GetProperties().Select(pi => new Personalization.UIComponentDataMember { Name = pi.Name, Value = pi.GetValue(Detail, null) }).ToArray();
                if (Detail.Identifier != null)
                    context.DataContract.SetValueByName("IdentifierCDOType", Detail.Identifier.CDOTypeName);
                Personalization.PageMappingTarget target = new PortalFramework.PageRoutingCache().GetTarget(context, item);
                FormsFramework.CallStackMethodBase call = null;
                new PortalFramework.PageRoutingCache().GetPageMappingTarget(context, ref target, ref call);
                if (target != null)
                {
                    // Was left as potential feature, which allows redirection from QOApproval to QOManage
                    //***
                    //FormsFramework.CallStackMethodBase returncall = null;
                    //if (Detail.ToDoListItemType == ConciergePageMappingEnum.ApprovalQualityObjectEntry ||
                    //    Detail.ToDoListItemType == ConciergePageMappingEnum.ApprovalQualityObjectComplete)
                    //{
                    //    Camstar.WebPortal.FormsFramework.PortalContextBase returncontext = new FormsFramework.PortalContextBase();
                    //    returncontext.DataContract = new Personalization.UIComponentDataContract();
                    //    returncontext.DataContract.DataMembers = typeof(ToDoListDetail).GetProperties().Select(pi => new Personalization.UIComponentDataMember { Name = pi.Name, Value = pi.GetValue(Detail, null) }).ToArray();
                    //    if (Detail.Identifier != null)
                    //        returncontext.DataContract.SetValueByName("IdentifierCDOType", Detail.Identifier.CDOTypeName);
                    //    Personalization.PageMappingItem returnitem = cache.GetItem("EventRecordView");
                    //    Personalization.PageMappingTarget returnitemtarget = new PortalFramework.PageRoutingCache().GetTarget(returncontext, returnitem);
                    //    new PortalFramework.PageRoutingCache().GetPageMappingTarget(returncontext, ref returnitemtarget, ref returncall);
                    //    returncall.Context = returncontext;
                    //}
                    string url;
                    string key = Detail.Self.ID;
                    call.Context = context;
                    //GlobalCallStackSession[key] = new FormsFramework.CallStackDelayedItem() { TargetMethod = call, ReturnMethod = returncall };
                    GlobalCallStackSession[key] = new FormsFramework.CallStackDelayedItem() { TargetMethod = call};
                    string[] urlParts = target.Target.Split('?');
                    string pageName = urlParts[0].Replace(".aspx", "");
                    string query = "";
                    if (urlParts.Length == 2)
                        query = urlParts[1];

                    if (target.TargetType == Personalization.PageMappingTargetType.Page)
                    {
                        url = string.Format("__toppage.openInExistingTab('{0}','GlobalCall=true&GlobalCallId={1}&Call=true','',null); return false;", pageName, key);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(query))
                            query += "&";
                        else
                            query = "?";
                        if (Detail.ToDoListItemType == ConciergePageMappingEnum.EventCreatePageflow)
                        {
                            // take the label Lbl_PageTitleControl_EventRecording
                            url =
                                string.Format(
                                    "__toppage.openPageflow('{0}','ResumeWorkflow=true&QualityObject={1}', 'Event Recording',null); return false;",
                                    pageName, HttpContext.Current.Server.UrlEncode(context.DataContract.GetValueByName("QualityObject").ToString()));
                        }
                        else
                        {
                            query += string.Format("GlobalCall=true&GlobalCallId={0}&Call=true&ResumeWorkflow=true", key);
                            url = string.Format("__toppage.openPageflow('{0}','{1}',''); return false;", pageName, query);
                        }
                    }
                    return url;
                }
            }
            // Prepare mapping keys collection for the current concierge item
            KeyValueConfigurationCollection keys = new KeyValueConfigurationCollection();
            Detail.ToKeyValueCollection(ref keys);
            keys.Add("CDOTypeName", Detail.Identifier.CDOTypeName);
            keys.Add("CategoryName", ((CategoryEnum)Detail.Category).ToString());
            if (mToDoListDetail.ToDoListItemType == ConciergePageMappingEnum.ProcessModelOwnership ||
                mToDoListDetail.ToDoListItemType == ConciergePageMappingEnum.PhaseOwnership ||
                mToDoListDetail.ToDoListItemType == ConciergePageMappingEnum.PlanOwnership)
            {
                keys.Add("POID", mToDoListDetail.Identifier.ID);
            }

            // Lookup for a page mapping entry
            MapItemConfigElement mapItem;
            ResultStatus status = FrameworkManagerUtil.PageMapLookup(keys, out mapItem);
            if (status.IsSuccess)
            {
                // Prepare query string parameters collection
                keys["Category"].Value = mToDoListDetail.Category.Value.ToString("d");
                keys.Add("QOT", QualityObjectUtil.CategoryToQualityObjectTypeName(mCategory));
                keys.Add( "AID", mToDoListDetail.Identifier.ID);
                keys.Add("CDOType", mToDoListDetail.Identifier.CDOTypeName);
                string dcDefined = (Detail.DataCollectionDefined != null ? Detail.DataCollectionDefined.ToString() : string.Empty);
                keys.Add("DCDef", dcDefined);
                keys.Add("InstanceId", mToDoListDetail.Identifier.ID);
                keys.Add("QON", mToDoListDetail.QualityObject.Name);
                if (mToDoListDetail.ApprovalSheetParent != null)
                {
                    keys.Add("ApprovalSheetEntry", mToDoListDetail.Identifier.ID);
                    keys.Add("Parent", mToDoListDetail.ApprovalSheetParent.ID);
                    keys.Add("ParentType", mToDoListDetail.ApprovalSheetParent.CDOTypeName);
                }
        
                // Apply and construct query string parameters
                string url = mapItem.ApplyParameters(keys);
                string[] urlParts = url.Split('?');
                string pageName = urlParts[0].Replace(".aspx", "");
                string query = "";
                if (urlParts.Length == 2)
                    query = urlParts[1];
                if (!string.IsNullOrEmpty(query))
                    query += "&ResetCallStack=true";
                else
                    query = "?ResetCallStack=true";

                // Wrap URL for AJAX framework invocation
                if (mapItem.LinkType == MapItemLinkTypeEnum.Pageflow)
                    url = string.Format("__toppage.openPageflow('{0}','{1}',''); return false;", pageName, query);
                else
                    url = string.Format("__toppage.openInExistingTab('{0}','{1}','',null); return false;", pageName, query);

                return url;
            }
            else
            {
                return string.Format("alert('{0}'); return false;", status.Message);
            }
        }

        #endregion

        #region Private Member Variables

        private ToDoListDetail mToDoListDetail;

        private NotificationTypeEnum mToDoListEntryType;
        private DateTime mDueDate;
        private PriorityStatusEnum mPriorityStatus;
        private CategoryEnum mCategory;
        private string mMessage;
        private string mName;
        private string mPageURL;

        private LinkButton mConciergeItemLinkButton;
        private Image mPriorityImage;

        private int mYellowMinRange;
        private int mYellowMaxRange;

        private const string YellowMaxRangeColumn = "YellowMaxRange";
        private const string YellowMinRangeColumn = "YellowMinRange";


        #endregion
    }
}
