// Copyright Siemens 2023  
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.WebPortlets;
using Camstar.WebPortal.Constants;
using CamstarPortal.WebControls.Constants;
using System.Drawing;

namespace Camstar.WebPortal.WebPortlets.Concierge
{
    /// <summary>
    /// 
    /// </summary>
    public class ConciergeSection : WebControl, ICallbackEventHandler
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the ConciergeSection.
        /// </summary>
        public ConciergeSection()
        {
            Initialize();
        }

        public ConciergeSection(string sectionCategoryValue, string sectionCategoryTitle, string iconImagePath)
        {
            Initialize();
            this.SectionTitle = sectionCategoryTitle;
            this.SectionValue = sectionCategoryValue;

            if (string.Compare(HttpContext.Current.Session["CurrentTheme"].ToString(), "camstar", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                bool fileExistingAndSupported = IsImageFileSupported(iconImagePath);
                if (fileExistingAndSupported)
                    this.SectionIconImagePath = Images.ConciergeIconPath + iconImagePath;
                else
                    this.SectionIconImagePath = Images.DefaultIconImagePath;
            }
            else {
                this.SectionIconImagePath = Images.ConciergeTriangleRightPath;
            }

            this.Items = new ConciergeItems();
            this.SectionType = this.SectionValue;

        }

        static ConciergeSection()
        {
        }

        #endregion

        #region Public Methods
        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the value that indicates the SectionTitle of the ConciergeSection.
        /// </summary>
        public virtual string SectionTitle
        {
            get { return mSectionTitle; }
            set { mSectionTitle = value; }
        }

        public virtual string SectionValue
        {
            get { return mSectionValue; }
            set { mSectionValue = value; }
        }

        /// <summary>
        /// Gets or sets the value that indicates the the path to the image that will be used
        /// for the ConciergeSection icon.
        /// </summary>
        public virtual string SectionIconImagePath
        {
            get { return mSectionIconImagePath; }
            set { mSectionIconImagePath = value; }
        }

        /// <summary>
        /// Gets or sets the value that indicates the CSS class that will be applied to the 
        /// ConciergeSection div container.
        /// </summary>
        public virtual string SectionCSSClass
        {
            get { return mSectionCSSClass; }
            set { mSectionCSSClass = value; }
        }

        /// <summary>
        /// Gets or sets the value that indicates the ClientID of this section to be
        /// referenced by client-side code.
        /// </summary>
        public virtual string SectionID
        {
            get { return mSectionID; }
            set { mSectionID = value; }
        }

        public virtual string SectionType
        {
            get { return mSectionType; }
            set { mSectionType = value; }
        }

        /// <summary>
        /// Gets or sets the value the indicates the collection of TaskItems that will be
        /// included in this ConciergeSection.
        /// </summary>
        public virtual ConciergeItems Items
        {
            get { return mConciergeItems; }
            set { mConciergeItems = value; }
        }

        public virtual bool ItemsExist
        {
            get { return mItemsExist; }
            set { mItemsExist = value; }
        }

        #endregion

        #region Protected Methods

        #region Overridden Methods

        /// <summary>
        /// Creates the section header and the TaskItem collection in a table format.
        /// </summary>
        protected override void CreateChildControls()
        {
            this.Attributes["SectionType"] = SectionType;

            Camstar.WebPortal.FormsFramework.WebControls.Image imgSectionIcon = new Camstar.WebPortal.FormsFramework.WebControls.Image();
            mPanelSectionHeader = new Panel();

            imgSectionIcon.ImageUrl = mSectionIconImagePath;
            // Would be nice to have cutting message functionality on client side
            string cutSectionTitle = mSectionTitle;
            mSectionTitleLabel.Text = cutSectionTitle;
            mSectionTitleLabel.ID = SectionValue + "_title";
            mPanelSectionHeader.Controls.Add(imgSectionIcon);
            mPanelSectionHeader.Controls.Add(mSectionTitleLabel);
            mPanelSectionHeader.CssClass = mSectionCSSClass;

            Controls.Add(mPanelSectionHeader);

            mPanelContainer = new Panel();
            mPanelContainer.CssClass = ThemeConstants.CamstarUIConciergeSectionContent;
            mPanelContainer.ID = this.ClientID + "_Panel";
            Controls.Add(mPanelContainer);


            base.CreateChildControls();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventArgument"></param>
        public virtual void RaiseCallbackEvent(string eventArgument)
        {
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual string GetCallbackResult()
        {
            return string.Empty;
        }
                        
        #endregion

        /// <summary>
        /// Initializes all private member variables.
        /// </summary>
        protected virtual void Initialize()
        {
            mSectionCSSClass = ThemeConstants.CamstarUIConciergeSection;
            mSectionIconImagePath = string.Empty;
            mSectionID = string.Empty;
            mSectionTitle = string.Empty;
            mSectionValue = string.Empty;
            mSectionTitleLabel = new Camstar.WebPortal.FormsFramework.WebControls.Label();
            mItemsExist = false;
		}

        protected override void OnPreRender(EventArgs e)
        {
            if (mPanelContainer != null && mConciergeItems != null)
            {
                mPanelContainer.Controls.Add(mConciergeItems.ToTable());
            }

            mPanelSectionHeader.Attributes.Add("items", ItemsExist.ToString());
            
            base.OnPreRender(e);
        }

        protected virtual bool IsImageFileSupported(string iconImagePath)
        {
            if (string.IsNullOrEmpty(iconImagePath)) return false;
            //checks file existence
            string defaultIconDirectory = System.Web.HttpContext.Current.Server.MapPath(".") + Images.ConciergeIconPhysicalPath;
            bool fileExist = File.Exists(defaultIconDirectory + iconImagePath);
            if (!fileExist) return false;
            //checks whether extension is supported
            string extension = Path.GetExtension(iconImagePath);
            bool isSupported = supportedExtensions.Any(supportedExtension => 
                string.Equals(extension, "." + supportedExtension, StringComparison.InvariantCultureIgnoreCase));
            return isSupported;
        }

        #endregion

        #region Constants

            private const int CategoryMessageAreaWidth = 250;

            private const int CategoryMessageAreaHeight = 20;

            private const float CategoryMessageWidth = 160f;

            private const int CategoryMessageFontSize = 11;
        
        #endregion

        #region Protected Properties
        #endregion

        #region Private Methods
        #endregion

        #region Private Member Variables

        private Camstar.WebPortal.CommonWebControls.Label mSectionTitleLabel;
        private string mSectionTitle;
        private string mSectionValue;
        private string mSectionIconImagePath;
        private string mSectionCSSClass;
        private string mSectionID;
        private string mSectionType;
        private bool mItemsExist;

        private ConciergeItems mConciergeItems;
        private Panel mPanelContainer;
        private Panel mPanelSectionHeader;

        private readonly string[] supportedExtensions = { FileExtentions.PNG, FileExtentions.JPG, FileExtentions.JPEG, FileExtentions.GIF };

        #endregion
    }
}
