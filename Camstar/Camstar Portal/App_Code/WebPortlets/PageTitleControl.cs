// Copyright Siemens 2023  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls.WebParts;

using Camstar.WebPortal.Constants;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.HtmlControls;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.Personalization;

namespace Camstar.WebPortal.WebPortlets
{
    public class PageTitleControl : WebPartBase, IWebPartNoConnect 
    {
        #region Constructors

        public PageTitleControl()
        { 
        }

        #endregion

        #region  Public Methods

        #endregion

        #region Public Properties
        #endregion

        #region Protected Methods

        protected override WebPartWrapperBase CreateWebPartWrapper()
        {
            return null;
        }

        protected override void CreateContentControls(ControlCollection contentControls)
        {
            DivElement div = new DivElement();
            div.AddCssClass("webpart-pagetitle");
            div.Controls.Add(mPageTitleLabel);
            
            contentControls.Add(div);
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            PageMapping mapping = new PageMapping();
            string pageTitle = string.Empty;

            if(Page.MergedContent.TitleLabel != null)
            {                
                //If the LabelName is present, get the value for the label from the labelcache
                if (!string.IsNullOrEmpty(Page.MergedContent.TitleLabel))
                {
                    var labelCache = FrameworkManagerUtil.GetLabelCache(System.Web.HttpContext.Current.Session);
                    if (labelCache != null)
                    {
                        var label = labelCache.GetLabelByName(Page.MergedContent.TitleLabel);
                        if (label != null)
                            if (label.Value != null)
                                pageTitle = label.Value;
                            else
                                pageTitle = label.DefaultValue;
                        
                    }
                }
                else
                {
                    //Otherwise use the description (Title) value
                    pageTitle = mapping.GetPageDescription(PageMapping.ExtractPageName(this.Page.Request.Url.AbsoluteUri));
                }

                if (!string.IsNullOrEmpty(pageTitle))
                {
                    mPageTitleLabel.Text = pageTitle;
                }
            }
            else
            {
                //if the PageTitleControl object is null (mainly the case for backwards compatibility), use the description (Title).
                pageTitle = mapping.GetPageDescription(PageMapping.ExtractPageName(this.Page.Request.Url.AbsoluteUri));

                if (!string.IsNullOrEmpty(pageTitle))
                {
                    mPageTitleLabel.Text = pageTitle;
                }
            }
            // below does not make sense
            //if (!string.IsNullOrEmpty(pageTitle))
            //{
            //    if (!Page.IsPostBack)
            //        this.RenderToClient = true;
            //}
        }


        #endregion

        #region Protected Properties
        #endregion

        #region Private Methods
        #endregion

        #region Private Member Variables

        private Camstar.WebPortal.FormsFramework.WebControls.Label mPageTitleLabel = new Camstar.WebPortal.FormsFramework.WebControls.Label();

        #endregion
    }
}
