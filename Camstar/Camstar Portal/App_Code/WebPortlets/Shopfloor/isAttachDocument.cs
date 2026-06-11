// Copyright Siemens 2022 
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.Utilities;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.WebPortlets.Modeling;
using Camstar.WebPortal.WCFUtilities;


namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
	public class isAttachDocument : AttachDocument
	{
        string ErrorMessage = string.Empty;
        
        protected virtual CWC.TextBox ObjectTypeDefect { get { return Page.FindCamstarControl("ObjectTypeDefect") as CWC.TextBox; } }

        protected bool HaveDocumentToAttach
        {
            get
            {
                return (bool)HttpContext.Current.Session["HaveDocumentToAttach"];
            }
            set
            {
                HttpContext.Current.Session["HaveDocumentToAttach"] = value;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }
        public override void WebPartCustomAction(object sender, CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);

            if (e.Action.Name == "DelayAttachButton")
            {
                //Page.CloseFloatingFrameOnSubmit(new ResultStatus());
                if (ValidatePageData())
                {
                    DocsToAttach.CheckControl.Checked = true;
                    HaveDocumentToAttach = true;
                    Page.CloseFloatingFrameOnSubmit(new ResultStatus());
                }
            }
        }

        private bool ValidatePageData()
        {
            if (AttachmentType.Data == null)
            {
                ErrorMessage = GetLabelValue("isAttachDocument_TypeMustBeSelected");
            }
            else
            {
                bool isMissed = false;
                AttachmentTypeEnum attachType = Enumeration<AttachmentTypeEnum, int>.ConvertToEnum(Convert.ToInt32(AttachmentType.Data.ToString()), out isMissed);

                if (attachType == AttachmentTypeEnum.NewDocumentReuse || attachType == AttachmentTypeEnum.NewDocumentNOReuse)
                {
                    // must have file and doc name set
                    if ((int)AttachMode.Data == 2 && (UploadField.Data == null || string.IsNullOrWhiteSpace(UploadField.Data.ToString())))
                    {
                        ErrorMessage = GetLabelValue("AttachDocument_FileMustBeSelected");
                    }
                    if (DocName.Data == null || string.IsNullOrWhiteSpace(DocName.Data.ToString()))
                    {
                        ErrorMessage = GetLabelValue("AttachDocument_NameRequired");
                    }
                }

                if (attachType == AttachmentTypeEnum.NewDocumentReuse)
                {
                    //must have revision set
                    if (DocRevision.Data == null || string.IsNullOrWhiteSpace(DocRevision.Data.ToString()))
                    {
                        ErrorMessage = GetLabelValue("AttachDocument_RevisionRequired");
                    }
                }

                if (attachType == AttachmentTypeEnum.ExistingDocument)
                {
                    if (DocumentInstanceField.Data == null)
                    {
                        ErrorMessage = GetLabelValue("AttachDocument_ExistingDocumentRequired");
                    }
                }
            }

            return string.IsNullOrEmpty(ErrorMessage);
        }
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            DelayAttachButton.Visible = false;

            if (DelayAttach.IsChecked && !IsContainerField.IsChecked)
            {
                SubmitButton.Visible = false;
                DelayAttachButton.Visible = true;
                ObjectTypeField.Visible = false;
                ObjectTypeDefect.Visible = true;
                ObjectTypeDefect.Data = GetLabelValue("CSICDOName_isDefect");
                ObjectTypeDefect.Enabled = false;
            }
            else if (IsContainerField.IsChecked)
            {
                SubmitButton.Visible = true;
                DelayAttachButton.Visible = false;
                ObjectTypeField.Visible = true;
                ObjectTypeDefect.Visible = false;
            }
            if (ErrorMessage != string.Empty)
            {
                Page.StatusBar.WriteError(ErrorMessage);
            }
        }

        protected override void ClearObjects()
        {
            base.ClearObjects();
            Page.DataContract.SetValueByName("DocumentNameDM", null);
            Page.DataContract.SetValueByName("DocumentRevisionDM", null);
            Page.DataContract.SetValueByName("DocumentCommentsDM", null);
            Page.DataContract.SetValueByName("DocumentDescriptionDM", null);
        }

        protected override void ClearDataValues()
        {
            base.ClearDataValues();
            Page.DataContract.SetValueByName("DocumentNameDM", null);
            Page.DataContract.SetValueByName("DocumentRevisionDM", null);
            Page.DataContract.SetValueByName("DocumentCommentsDM", null);
            Page.DataContract.SetValueByName("DocumentDescriptionDM", null);
        }
        private string GetLabelValue(string labelId)
        {
            try
            {
                var labelCache = FrameworkManagerUtil.GetLabelCache(System.Web.HttpContext.Current.Session);
                Label label = labelCache.GetLabelByName(labelId);
                return label.Value;
            }
            catch (Exception ex) //Catch errors
            {
                return ex.ToString();
            }
        }
    }
}
