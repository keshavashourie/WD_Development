// Copyright Siemens 2023  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using Camstar.WebPortal.Constants;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.Utilities;
using Camstar.WCF.Services;
using OM = Camstar.WCF.ObjectStack;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Xml.Linq;

namespace Camstar.WebPortal.WebPortlets.Concierge
{
    [PortalStudio(false)]
    public class ConciergePanel : WebPartBase, IWebPartNoConnect
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the ConciergePanel.
        /// </summary>
        public ConciergePanel()
        {
            Initialize();
        }

        #endregion

        #region Public Methods

        #region Overridden Methods

        #endregion

        /// <summary>
        /// 
        /// </summary>
        public virtual void InitialDataLoad()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Refresh()
        {
        }

        static public bool ToggleConciergeState(AjaxTransition transition)
        {
            bool isNotEmptyResponse = true;
            if (HttpContext.Current.Session[SessionConstants.ConciergeDataHTML] != null)
            {
                transition.Response = new ResponseSection[1];
                transition.Response[0] = new ResponseSection(ResponseType.Command, string.Empty, new CommandData(true, HttpContext.Current.Session[SessionConstants.ConciergeDataHTML].ToString()));
            }
            else
            {
                transition.Response = null;
                isNotEmptyResponse = false;
            }

            return isNotEmptyResponse;
        }

        static public bool RefreshConciergeSection(AjaxTransition transition)
        {
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            if (session == null)
                HttpContext.Current.Response.Redirect("Default.aspx", true);

            if (HttpContext.Current.Session[SessionConstants.ToDoListCache] == null)
                HttpContext.Current.Session[SessionConstants.ToDoListCache] = new ToDoListCache(100);

            ToDoListCache todoListCache = HttpContext.Current.Session[SessionConstants.ToDoListCache] as ToDoListCache;
            //if (todoListCache.IsOutdated)
            {
                todoListCache.Data = GetToDoListData(todoListCache);
            }

            string type = transition.CommandParameters;

            todoListCache.SelectedSection = todoListCache.PortalCategories.Rows.Select((n, index) => new { item = n, position = index }).First(n => n.item.Values[0] == type).position;
            
            ConciergeItems items = new ConciergeItems();
            if (todoListCache.Data != null)
            {
                foreach (OM.ToDoListDetail td in todoListCache.Data)
                {
                    if (string.Compare(td.MessageCategory.ID, type, true) == 0)
                    {
                        items.Add(new ConciergeItem(td));
                    }
                }
            }

            Table tbl = items.ToTable();

            StringWriter sw = new StringWriter();
            HtmlTextWriter hw = new HtmlTextWriter(sw);
            tbl.RenderControl(hw);

            bool isNotEmptyResponse = true;
            transition.Response = new ResponseSection[1];
            transition.Response[0] = new ResponseSection(ResponseType.Command, string.Empty, new CommandData(true, sw.ToString()));

            //better solution should be implemented
            if (HttpContext.Current.Session[SessionConstants.IsConciergeSaved] != null)
            {
                string sConcierge = HttpContext.Current.Session[SessionConstants.ConciergeDataHTML] as string;

                int startPos = 0;
                for (int i = 0; i < todoListCache.PortalCategories.Rows.Length; i++ )
                {
                    startPos = sConcierge.IndexOf("<table border=\"0\">", startPos+1);
                    if (startPos == -1)
                        break;
                    int endPos = sConcierge.IndexOf("</div>", startPos);

                    sConcierge = sConcierge.Remove(startPos, endPos - startPos);
                    if (i == todoListCache.SelectedSection)
                        sConcierge = sConcierge.Insert(startPos, sw.ToString());
                    else
                        sConcierge = sConcierge.Insert(startPos, "<table border=\"0\"></table>");
                }

                HttpContext.Current.Session[SessionConstants.ConciergeDataHTML] = sConcierge;
            }

            return isNotEmptyResponse;
        }
        #endregion

        #region Public Properties

        [WebProperty(Enabled = false)]
        public override string Title
        {
            get { return base.Title; }
            set { base.Title = value; }
        }

        [WebProperty(Enabled = false)]
        public override Unit Width
        {
            get { return base.Width; }
            set { base.Width = value; }
        }

        [WebProperty(Enabled = false)]
        public override Unit Height
        {
            get { return base.Height; }
            set { base.Height = value; }
        }

        [WebProperty(Enabled = false)]
        public override OM.CategoryEnum QualityObjectCategory
        {
            get { return base.QualityObjectCategory; }
            set { base.QualityObjectCategory = value; }
        }

        [WebProperty(Enabled = false)]
        public override string PrimaryServiceType
        {
            get { return base.PrimaryServiceType; }
            set { base.PrimaryServiceType = value; }
        }

        [WebProperty(LabelName = "WebProperty_RefreshInterval", ValidatorType = typeof(IntegerValidator))]
        public virtual int RefreshInterval
        {
            get { return mRefreshInterval; }
            set { mRefreshInterval = value; }
        }

        #region Overridden Properties

        /// <summary>
        /// 
        /// </summary>
        public override PartChromeState ChromeState
        {
            get { return base.ChromeState; }
            set { base.ChromeState = value; }
        }

        #endregion

        #endregion

        #region Protected Methods

        #region Overridden Methods

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            LabelCache labelCache = FrameworkManagerUtil.GetLabelCache(Page.Session);
            if (labelCache != null)
            {
                Title = labelCache.GetLabelTextByName("Concierge_Concierge", val => Title = val);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            
            if (!Page.IsPostBack && Page.IsAjax1stCall)
            {
                CreateConciergeSections();
                GetData();
            }
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if (!Page.IsPostBack || mIsRefreshClicked)
            {
                if (Page.Session[SessionConstants.IsConciergeSaved] == null)
                {
                    StringWriter sw = new StringWriter();
                    HtmlTextWriter hw = new HtmlTextWriter(sw);

                    _ConciergeWrapper.ContentTable.RenderControl(hw);
                    Page.Session[SessionConstants.IsConciergeSaved] = true;
                    Page.Session[SessionConstants.ConciergeDataHTML] = sw.ToString();
                }
            }

            base.Render(writer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contentControls"></param>
        protected override void CreateContentControls(ControlCollection contentControls)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override WebPartWrapperBase CreateWebPartWrapper()
        {
            if (_ConciergeWrapper == null)
            {
                _ConciergeWrapper = new ConciergeWrapper(this);
            }
            return _ConciergeWrapper;
        }

        protected virtual void CreateConciergeSections()
        {
            OM.RecordSet portalMessageCategoryDef = GetPortalMessageCategoryDef();
            ToDoList.PortalCategories = portalMessageCategoryDef;

            if (ToDoList.PortalCategories != null && ToDoList.PortalCategories.Rows != null)
            {
                for (int i = 0; i < ToDoList.PortalCategories.Rows.Length; i++)
                {
                    ConciergeSection section = new ConciergeSection(ToDoList.PortalCategories.Rows[i].Values[0], ToDoList.PortalCategories.Rows[i].Values[2], ToDoList.PortalCategories.Rows[i].Values[3]);
                    _ConciergeWrapper.ContentPanel.Controls.Add(section);
                    mSections.Add(section);
                }
            }
        }

        #endregion



        /// <summary>
        /// Initializes all private member variables.
        /// </summary>
        protected virtual void Initialize()
        {
            mSections = new List<ConciergeSection>();
        }

        protected virtual OM.RecordSet GetPortalMessageCategoryDef()
        {
            OM.RecordSet portalMessageCategoryDef = null;
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            GetToDoListService service = new GetToDoListService(session.CurrentUserProfile);

            OM.GetToDoList data = new OM.GetToDoList();

            GetToDoList_Request request = new GetToDoList_Request();
            request.Info = new OM.GetToDoList_Info();

            request.Info.MessageCategoriesToRetrieve = new OM.Info();
            request.Info.MessageCategoriesToRetrieve.RequestSelectionValues = true;

            GetToDoList_Result result;
            OM.ResultStatus res = service.GetEnvironment(request, out result);
            if (result != null && result.Environment != null && result.Environment.MessageCategoriesToRetrieve != null)
            {
                portalMessageCategoryDef = result.Environment.MessageCategoriesToRetrieve.SelectionValues;
            }
            return portalMessageCategoryDef;
        }

        protected virtual void ConciergeEditor_PropertiesChanged()
        {
            ToDoList.Initialize(RefreshInterval);
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void GetData()
        {
            if (ToDoList.IsOutdated)
            {
                ToDoListCache todoListCache = HttpContext.Current.Session[SessionConstants.ToDoListCache] as ToDoListCache;
                ToDoList.Data = GetToDoListData(todoListCache);
                this.RenderToClient = true;
            }

            for (int i = 0; i < mSections.Count; i++)
                mSections[i].Items.Clear();

            if (ToDoList.Data != null && mSections != null && mSections.Count > 0)
            {
                foreach (OM.ToDoListDetail td in ToDoList.Data)
                {
                    //Only load the first category when the ToDoList Data is initially loaded or refreshed
                    if (string.Compare(mSections[0].SectionType, td.MessageCategory.ID, true) == 0)
                    {
                        mSections[0].Items.Add(new ConciergeItem(td));
                        mSections[0].ItemsExist = true;
                    }

                    foreach (ConciergeSection section in mSections)
                    {
                        if (string.Compare(section.SectionType, td.MessageCategory.ID, true) == 0 && !section.ItemsExist)
                        {
                            section.ItemsExist = true;
                        }
                    }
                }

                ToDoList.SelectedSection = 0;
            }
        }

        static private OM.ToDoListDetail[] GetToDoListData(ToDoListCache todoListCache)
        {
            OM.ToDoListDetail[] toDoListData = null;
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            GetToDoListService service = new GetToDoListService(session.CurrentUserProfile);

            OM.GetToDoList data = new OM.GetToDoList();

            data.TODOListStartRow = 1;
            data.TODOListSize = todoListCache.ItemPerSection;
            data.FilterForConcierge = true;

            if (todoListCache.PortalCategories != null && todoListCache.PortalCategories.Rows != null)
                data.MessageCategoriesToRetrieve = todoListCache.PortalCategories.Rows.Select(n => new Camstar.WCF.ObjectStack.NamedObjectRef() { ID = n.Values[0] }).ToArray();

            GetToDoList_Request request = new GetToDoList_Request();
            request.Info = new OM.GetToDoList_Info();

            request.Info.ToDoListDetail = new OM.ToDoListDetail_Info();
            request.Info.ToDoListDetail.Name = new OM.Info(true);
            request.Info.ToDoListDetail.DueDate = new OM.Info(true);
            request.Info.ToDoListDetail.PriorityStatus = new OM.Info(true);

            request.Info.ToDoListDetail.Identifier = new OM.Info(true);
            request.Info.ToDoListDetail.DataCollectionDefined = new OM.Info(true);

            request.Info.ToDoListDetail.NotificationType = new OM.Info(true);
            request.Info.ToDoListDetail.MessageCategory = new OM.Info(true);
            request.Info.ToDoListDetail.MessageCategoryName = new OM.Info(true);

            request.Info.ToDoListDetail.ToDoListItemType = new OM.Info(true);
            request.Info.ToDoListDetail.Category = new OM.Info(true);
            request.Info.ToDoListDetail.DisplayMessage = new OM.Info(true);
            request.Info.ToDoListDetail.ApprovalSheetParent = new OM.Info(true);
            request.Info.ToDoListDetail.ApprovalEntryRole = new OM.Info(true);

            request.Info.ToDoListDetail.Organization = new OM.Info(true);
            request.Info.ToDoListDetail.Classification = new OM.Info(true);
            request.Info.ToDoListDetail.ClassificationName = new OM.Info(true);
            request.Info.ToDoListDetail.SubClassification = new OM.Info(true);
            request.Info.ToDoListDetail.SubclassificationName = new OM.Info(true);
            request.Info.ToDoListDetail.PriorityLevel = new OM.Info(true);
            request.Info.ToDoListDetail.QualityObject = new Camstar.WCF.ObjectStack.Info(true);

            GetToDoList_Result result;
            OM.ResultStatus res = service.GetToDoList(data, request, out result);
            if (result != null && result.Value != null && result.Value.ToDoListDetail != null)
                toDoListData = result.Value.ToDoListDetail;

            return toDoListData;
        }

        public override void Update()
        {
            base.Update();

            ToDoList.Data = null;
            CreateConciergeSections();
            GetData();

            mIsRefreshClicked = true;
        }
        #endregion

        protected virtual ToDoListCache ToDoList
        {
            get
            {
                if (Page.Session[SessionConstants.ToDoListCache] == null)
                    Page.Session[SessionConstants.ToDoListCache] = new ToDoListCache(RefreshInterval);

                mToDoListCache = Page.Session[SessionConstants.ToDoListCache] as ToDoListCache;

                return mToDoListCache;
            }
        }

        #region Private Member Variables

        protected override string ClientControlTypeName
        {
            get { return "Camstar.WebPortal.WebPortlets.Concierge"; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override IEnumerable<ScriptReference> GetScriptReferences()
        {
            List<ScriptReference> refs = base.GetScriptReferences().ToList<ScriptReference>();
            refs.Add(new ScriptReference("~/Scripts/ClientFramework/Camstar.WebPortal.WebPortlets/Concierge.js"));
            return refs;
        }

        protected override IEnumerable<ScriptDescriptor> GetScriptDescriptors()
        {
            List<ScriptDescriptor> list = base.GetScriptDescriptors().ToList<ScriptDescriptor>();

            if (list.Count > 0)
            {
                List<string> ids = new List<string>();
                foreach (ConciergeSection section in mSections)
                    ids.Add(section.ClientID);
                (list[0] as ScriptControlDescriptor).AddProperty("sections", ids.ToArray<string>());
                (list[0] as ScriptControlDescriptor).AddProperty("selectedSection", ToDoList.SelectedSection);
            }
            return list;
        }

        private List<ConciergeSection> mSections;
        private ToDoListCache mToDoListCache;
        private ConciergeWrapper _ConciergeWrapper;

        private int mRefreshInterval;
        private bool mIsRefreshClicked = false;
        #endregion
    }

    public class ToDoListCache
    {
        public ToDoListCache(int? refreshInterval)
        {
            Initialize(refreshInterval);
        }

        public virtual void Initialize(int? refreshInterval)
        {
            mLastUpdated = DateTime.MinValue;

            int intValue = CamstarPortalSection.Settings.DefaultSettings.ConciergeRefreshInterval;
                if (refreshInterval != null)
                    intValue = refreshInterval.Value;
            mRefreshInterval = new TimeSpan(0, 0, intValue);

            int itemPerSection = CamstarPortalSection.Settings.DefaultSettings.ConciergeItemsDisplayedPerSection;
            if (itemPerSection != 0)
            {
                mItemPerSection = itemPerSection;
            }
            else
            {
                mItemPerSection = 10;
            }
        }

        public virtual bool IsOutdated
        {
            get
            {
                bool outdated = (DateTime.Now - mLastUpdated) > mRefreshInterval;
                if (mRefreshInterval == TimeSpan.Zero)
                    outdated = false;
                return outdated || mToDoListData == null;
            }
        }

        public virtual OM.ToDoListDetail[] Data
        {
            get
            {
                return mToDoListData;
            }
            set
            {
                mLastUpdated = DateTime.Now;
                mToDoListData = value;
            }
        }

        public virtual OM.RecordSet PortalCategories
        {
            get { return mPortalCategoriesDef; }
            set { mPortalCategoriesDef = value; }
        }

        public virtual TimeSpan RefreshInterval
        {
            get { return mRefreshInterval; }
            set { mRefreshInterval = value; }
        }

        public virtual int ItemPerSection
        {
            get { return mItemPerSection; }
            set { mItemPerSection = value; }
        }

        public virtual int SelectedSection
        {
            get { return mSelectedSection; }
            set { mSelectedSection = value; }
        }

        private OM.ToDoListDetail[] mToDoListData;
        private OM.RecordSet mPortalCategoriesDef;

        private DateTime mLastUpdated;
        private TimeSpan mRefreshInterval;
        private int mItemPerSection;
        private int mSelectedSection;
    }
}
