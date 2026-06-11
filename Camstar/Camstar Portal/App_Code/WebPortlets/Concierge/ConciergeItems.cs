// Copyright Siemens 2023  
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.FormsFramework.Utilities;
using System.Collections.Generic;
using System.Web;
using System.Web.UI.WebControls;

namespace Camstar.WebPortal.WebPortlets.Concierge
{
    /// <summary>
    /// Collection of the TaskItem class.
    /// </summary>
    public class ConciergeItems : List<ConciergeItem>
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the TaskItems class.
        /// </summary>
        public ConciergeItems()
        {
            Initialize();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the value that indicates if this is the InitialLoad of this 
        /// collection.
        /// </summary>
        public virtual bool IsInitialDataLoad
        {
            get { return mIsInitialLoad; }
            set { mIsInitialLoad = value; }
        }

        /// <summary>
        /// Gets or sets the value that indicates the number of TaskItems that are
        /// to be displayed when IsInitialDataLoad is true.
        /// </summary>
        public virtual int InitialDisplayCount
        {
            get { return mInitialDisplayCount; }
            set { mInitialDisplayCount = value; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a Table populated with all TaskItems in this collection.
        /// </summary>
        /// <returns>System.Web.UI.WebControls.Table</returns>
        public virtual Table ToTable()
        {
            if (this.Count > 0)
            {
                Table tbl = new Table();
                for (int x = 0; x < this.Count; x++)
                {
                    tbl.Rows.Add(this[x].ToTableRow(true));   
                }

                TableRow row = new TableRow();
                tbl.Rows.Add(row);

                row = new TableRow();
                TableCell moreCell = new TableCell();

                Camstar.WebPortal.FormsFramework.WebControls.LinkButton messageCenterLink = new Camstar.WebPortal.FormsFramework.WebControls.LinkButton();
                messageCenterLink.ID = "ViewAdditionalMessages";

                var viewMessagesLabel = FrameworkManagerUtil.GetLabelValue("Concierge_ViewAdditionalMessages") ?? "View Additional Messages";
                messageCenterLink.Text = viewMessagesLabel;
                messageCenterLink.ToolTip = viewMessagesLabel;

                ToDoListCache todoListCache = HttpContext.Current.Session[SessionConstants.ToDoListCache] as ToDoListCache;

                messageCenterLink.OnClientClick = string.Format("__toppage.openInExistingTab('MessageCenterVP.aspx', 'ResetCallStack=true&SelectedSection={0}','',null); return false;", todoListCache.SelectedSection);
                messageCenterLink.Attributes["href"] = string.Empty;

                moreCell.Controls.Add(messageCenterLink);
                row.Cells.Add(moreCell);
                tbl.Rows.Add(row);

                return tbl;
            }
            return new Table();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Initializes all private member variables.
        /// </summary>
        protected virtual void Initialize()
        {
            mIsInitialLoad = true;
            mInitialDisplayCount = 10;
        }

        #endregion

        #region Private Member Variables

        private bool mIsInitialLoad;
        private int mInitialDisplayCount;

        #endregion
    }
}
