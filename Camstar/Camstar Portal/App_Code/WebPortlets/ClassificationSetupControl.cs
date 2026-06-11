// Copyright Siemens 2023  
using System;
using System.Data;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Camstar.WebPortal.Constants;
using Camstar.WebPortal.WCFUtilities;
using CPF = Camstar.WebPortal.PortalFramework;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using CWF = Camstar.WebPortal.FormsFramework;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework.Utilities;

namespace Camstar.WebPortal.WebPortlets
{
    public class ClassificationSetupControl : MatrixWebPart, CPF.IGenericConnectionProvider
    {
        #region Constructors

        /// <summary>
        /// Specifies Title property of the web part
        /// </summary>
        public ClassificationSetupControl()
        {
            Title = "Classification Setup";
            TitleLabel = "UIWebPart_ClassificationSetup";
            this.Width = Unit.Pixel(706);
        }//ClassificationSetupControl

        #endregion

        [CWF.WebProperty(EditorType = typeof(CPF.EnumEditor))]
        public override OM.CategoryEnum QualityObjectCategory
        {
            get { return base.QualityObjectCategory; }
            set { base.QualityObjectCategory = value; }
        }

        #region Protected Properties

        protected virtual string OrganizationData
        {
            set { Page.PageflowControls.Add("OrganizationData", value); }
            get
            {
                string organizationData = string.Empty;
                if (Page.PageflowControls.GetDataByFieldExpression("OrganizationData") != null)
                {
                    organizationData = Page.PageflowControls.GetDataByFieldExpression("OrganizationData").ToString();
                }

                return organizationData;
            }
        }

        protected virtual OM.NamedObjectRef ClassificationData
        {
            set { Page.PageflowControls.Add("ClassificationData", value); }
            get
            {
                if (Page.PageflowControls.GetDataByFieldExpression("ClassificationData") != null)
                {
                    return Page.PageflowControls.GetDataByFieldExpression("ClassificationData") as OM.NamedObjectRef;
                }
                else
                {
                    return null;
                }
            }
        }

        protected virtual OM.NamedObjectRef SubClassificationData
        {
            set { Page.PageflowControls.Add("SubClassificationData", value); }
            get
            {
                if (Page.PageflowControls.GetDataByFieldExpression("SubClassificationData") != null)
                {
                    return Page.PageflowControls.GetDataByFieldExpression("SubClassificationData") as OM.NamedObjectRef;
                }
                else
                {
                    return null;
                }
            }
        }

        protected virtual string EventDataDetailType
        {
            set { Page.PageflowControls.Add(PageFlowContants.EventDetailsType, value); }
            get { return Page.PageflowControls.GetDataByFieldExpression(PageFlowContants.EventDetailsType) as string; }
        }

        protected virtual DataTable EventDataDetailTypes
        {
            set
            {
                Page.PageflowControls.Add("EventDetailTypes", value);
            }
            get
            {
                return Page.PageflowControls.GetDataByFieldExpression("EventDetailTypes") as DataTable;
            }
        }

        #endregion

        #region Protected Methods

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // "Save" button should not be accessible according to the design.
            if (Page.NavigationButtons != null)
            {
                Page.NavigationButtons.SaveButton.Visible = false;
                Page.NavigationButtons.BackButton.Visible = false;
            }//if
        }//OnInit

        protected override void OnLoad(EventArgs e)
        {
            if (QualityObjectCategory == Camstar.WCF.ObjectStack.CategoryEnum.Event)
            {
                _org.AutoPostBack = true;
                _org.DataChanged += new EventHandler(Organization_DataChanged);
                _classif.AutoPostBack = true;
                _classif.DataChanged += new EventHandler(Classification_DataChanged);
                _subClassif.AutoPostBack = true;
                _subClassif.DataChanged += new EventHandler(SubClassification_DataChanged);

                if (PrimaryServiceType != null && PrimaryServiceType.ToLower().IndexOf("update") >= 0)
                {
                    _org.ReadOnly = true;
                    _classif.ReadOnly = true;
                    _subClassif.ReadOnly = true;
                }
                else
                {
                    RestorePreviousData();
                }
            }

            _subClassif.PickListPanelControl.PostProcessData += new Camstar.WebPortal.FormsFramework.WebControls.PickLists.DataRequestEventHandler(SubClassPanel_PostProcessData);

            base.OnLoad(e);
        }

        protected virtual void SubClassPanel_PostProcessData(object sender, Camstar.WebPortal.FormsFramework.WebControls.PickLists.DataRequestEventArgs e)
        {
            EventDataDetailTypes = e.Data as DataTable;
        }

        public override void PostExecute(Camstar.WCF.ObjectStack.ResultStatus status, Camstar.WCF.ObjectStack.Service serviceData)
        {
            base.PostExecute(status, serviceData);

            if (QualityObjectCategory == Camstar.WCF.ObjectStack.CategoryEnum.Event && status.IsSuccess)
            {
                _org.ReadOnly = true;
                _classif.ReadOnly = true;
                _subClassif.ReadOnly = true;

                RenderToClient = true;
            }
        }

        public override void GetInputData(Camstar.WCF.ObjectStack.Service serviceData)
        {
            if (serviceData is OM.UpdateEvent)
            {
                _classif.DataSubmissionMode = Camstar.WebPortal.Personalization.DataSubmissionModeType.Skip;
                _subClassif.DataSubmissionMode = Camstar.WebPortal.Personalization.DataSubmissionModeType.Skip;
            }

            base.GetInputData(serviceData);

            if (serviceData != null && !string.IsNullOrEmpty(EventDataDetailType))
            {
                if (WCFObject.IsFieldExist(string.Format("{0}.{1}", serviceData.GetType().Name, mkEventDetailExpression)))
                {
                    OM.EventDetail detail = null;
                    WCFObject wcfo = new WCFObject(serviceData);
                    wcfo.SetValue(mkEventDetailExpression, detail);
                }
            }
        }

        protected override void AddFieldControls()
        {
            this.ControlAlignment = ControlAlignmentType.LabelLeftInputRight;

            BaseFieldExpression = ".";
            CreateField(_org as Control, "Organization");
            _org.PageFlowRequired = true;
            _org.Required = true;
            _org.DisplayMode = Camstar.WebPortal.Personalization.DisplayModeType.PickList;
            _org.AllowFreeFormTextEntry = false;
            _org.DefaultValue = "@Session:PrimaryOrganization";
            this[0, 0] = _org;

            CreateField(_classif as Control, "Classification");

            _classif.DisplayMode = Camstar.WebPortal.Personalization.DisplayModeType.PickList;
            _classif.ID = "ClassificationField";
            _classif.PageFlowRequired = true;
            _classif.Required = true;
            _classif.AllowFreeFormTextEntry = false;

            this[0, 1] = _classif;

            CreateField(_subClassif as Control, "SubClassification");

            _subClassif.DisplayMode = Camstar.WebPortal.Personalization.DisplayModeType.PickList;
            _subClassif.ID = "SubClassificationField";
            _subClassif.PageFlowRequired = true;
            _subClassif.Required = true;
            _subClassif.AllowFreeFormTextEntry = false;

            if (QualityObjectCategory == Camstar.WCF.ObjectStack.CategoryEnum.Event)
            {
                _classif.SelectionDependencies.Add(new CWF.DependsOnItem(_org.ID));
                _subClassif.SelectionDependencies.Add(new CWF.DependsOnItem(_org.ID));
                _subClassif.SelectionDependencies.Add(new CWF.DependsOnItem(_classif.ID));
            }

            this[0, 2] = _subClassif;

            _confirmationLabel.LabelName = "SelectionChangeConfirmation";
            _confirmationLabel.ID = "ConfirmationLabel";
            _confirmationLabel.Visible = false;
            this[1, 0] = _confirmationLabel;

        }//AddFieldControls

        public override void SetDisplayMode(CWF.FormDisplayModeType displayMode)
        {
            
        }
        #endregion

        #region Protected Methods

        public virtual void Organization_DataChanged(object sender, EventArgs e)
        {
            string positiveScript = CreateCallScript(mkClearEventArgumentParameter, OrganizationData, _org.ID);
            string negativeScript = CreateCallScript(mkNoClearEventArgumentParameter, OrganizationData, _org.ID);
            IsDirectUpdated = Page.IsPostBack;

            if (Page.PortalContext != null && (Page.PortalContext is Camstar.Portal.QualityObjectContext))
            {

                if (sender == null && !string.IsNullOrEmpty(OrganizationData) && OrganizationData != _org.Data.ToString())               
                    _org.Data = OrganizationData;
                
                if (OrganizationData == "" || (Page.Session[SessionConstants.PrimaryOrganization] != null && Page.Session[SessionConstants.PrimaryOrganization].ToString() == _org.Data.ToString()))
                    OrganizationData = _org.Data.ToString();

                if (!string.IsNullOrEmpty((Page.PortalContext as Camstar.Portal.QualityObjectContext).QualityObject.CDOTypeName) &&
                    (Page.PortalContext as Camstar.Portal.QualityObjectContext).QualityObject.CDOTypeName == "Event")
                {
                    if (!string.IsNullOrEmpty(_org.Data.ToString()) && OrganizationData != _org.Data.ToString() && ClassificationData != null)
                    {
                        LabelCache labelCache = FrameworkManagerUtil.GetLabelCache(System.Web.HttpContext.Current.Session);
                        string dialogTitle = labelCache.GetLabelByName("Lbl_Information").Value;
                        string script = JavascriptUtil.GetConfirmationCall(_confirmationLabel.Text, positiveScript, negativeScript, false, dialogTitle);
                        ScriptManager.RegisterStartupScript(this, this.GetType(), mkConfirmationScriptKey, script, true);
                        IsDirectUpdated = false;
                    }//if
                    if (ClassificationData != null)
                        _classif.Data = ClassificationData;
                    if (SubClassificationData != null)
                        _subClassif.Data = SubClassificationData;
                }//if
                else if(!string.IsNullOrEmpty(_org.Data.ToString()) && OrganizationData != _org.Data.ToString())
                {
                    OrganizationData = _org.Data.ToString();
                }//else
            }//if
            this.RenderToClient = true;
        }//Organization_DataChanged

        protected virtual void SubClassification_DataChanged(object sender, EventArgs e)
        {
            IsDirectUpdated = true;
            if (Page.PortalContext != null && (Page.PortalContext is Camstar.Portal.QualityObjectContext))
            {
                if (!string.IsNullOrEmpty((Page.PortalContext as Camstar.Portal.QualityObjectContext).QualityObject.CDOTypeName) &&
                    (Page.PortalContext as Camstar.Portal.QualityObjectContext).QualityObject.CDOTypeName == "Event")
                {
                    string positiveScript = CreateCallScript(mkClearEventArgumentParameter, SubClassificationData != null ? SubClassificationData.Name : string.Empty, _subClassif.ID);
                    string negativeScript = CreateCallScript(mkNoClearEventArgumentParameter, SubClassificationData != null ? SubClassificationData.Name : string.Empty, _subClassif.ID);

                    if (SubClassificationData == null)
                        SubClassificationData = (_subClassif.Data ?? null) as OM.NamedObjectRef;

                    if (SubClassificationData != (_subClassif.Data ?? null) as OM.NamedObjectRef)
                    {
                        LabelCache labelCache = FrameworkManagerUtil.GetLabelCache(System.Web.HttpContext.Current.Session);
                        string dialogTitle = labelCache.GetLabelByName("Lbl_Information").Value;
                        string script = JavascriptUtil.GetConfirmationCall(_confirmationLabel.Text, positiveScript, negativeScript, false, dialogTitle);
                        ScriptManager.RegisterStartupScript(this, this.GetType(), mkConfirmationScriptKey, script, true);
                        IsDirectUpdated = false;
                    }//if

                    // Extract Event Data Detail type for selected SubClassification
                    EventDataDetailType = WCFObject.GetEventDataDetail(ClassificationData, SubClassificationData);
                }//if
            }//if
        }//SubClassification_DataChanged

        protected virtual void Classification_DataChanged(object sender, EventArgs e)
        {
            this.IsDirectUpdated = Page.IsPostBack;
            if (Page.PortalContext != null && (Page.PortalContext is Camstar.Portal.QualityObjectContext))
            {
                if (!string.IsNullOrEmpty((Page.PortalContext as Camstar.Portal.QualityObjectContext).QualityObject.CDOTypeName) &&
                    (Page.PortalContext as Camstar.Portal.QualityObjectContext).QualityObject.CDOTypeName == "Event")
                {
                    string positiveScript = CreateCallScript(mkClearEventArgumentParameter, ClassificationData != null ? ClassificationData.Name : string.Empty, _classif.ID);
                    string negativeScript = CreateCallScript(mkNoClearEventArgumentParameter, ClassificationData != null ? ClassificationData.Name : string.Empty, _classif.ID);

                    if (ClassificationData == null)
                        ClassificationData = _classif.Data as OM.NamedObjectRef;

                    if (!ClassificationData.Equals(_classif.Data as OM.NamedObjectRef) && SubClassificationData != null)
                    {
                        LabelCache labelCache = FrameworkManagerUtil.GetLabelCache(System.Web.HttpContext.Current.Session);
                        string dialogTitle = labelCache.GetLabelByName("Lbl_Information").Value;
                        string script = JavascriptUtil.GetConfirmationCall(_confirmationLabel.Text, positiveScript, negativeScript, false, dialogTitle);
                        ScriptManager.RegisterStartupScript(this, this.GetType(), mkConfirmationScriptKey, script, true);
                        this.IsDirectUpdated = false;
                    }//if
                    if (SubClassificationData != null)
                        _subClassif.Data = SubClassificationData;
                }//if
            }//if
            this.RenderToClient = true;
        }//Classification_DataChanged

        #endregion

        #region Protected Methods

        /// <summary>
        /// Creates a script that is to be called on passing confirmation
        /// </summary>
        /// <param name="clearData"></param>
        /// <param name="data"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        protected virtual string CreateCallScript(string clearData, string data, string id)
        {
            //Parameters : clearData, data, controlID, eventTarget
            string script = string.Format("DoResetDataPostback('{0}','{1}','{2}','{3}');",
                                            clearData, data, id, "__Page");
            return script;
        }//CreateCallScript


        protected virtual void RestorePreviousData()
        {
            if (!string.IsNullOrEmpty(Page.Request["__EVENTARGUMENT"]))
            {
                string[] args = Page.Request["__EVENTARGUMENT"].Split(':');
                if (args.Length > 0 && args[0] == mkClearEventArgumentParameter)
                {
                    Page.CallPageflow(new Camstar.WebPortal.FormsFramework.PageflowCallStackMethod("CreateEventPageflow.1"));
                }//if
                else if (args[0] == mkNoClearEventArgumentParameter)
                {
                    if (args[2] == _classif.ID)
                    {
                        _classif.Data = Convert.ToInt32(args[1]);
                    }//if
                    else if (args[2] == _org.ID)
                    {
                        _org.Data = args[1];
                    }//else if
                    else if (args[2] == _subClassif.ID)
                    {
                        _subClassif.Data = Convert.ToInt32(args[1]);
                    }//else if
                }//else if

                this.RenderToClient = true;
            }//if
        }//RestorePreviousData

        #endregion

        #region IGenericConnectionProvider

        [CPF.ClassificationProvider(CPF.ConnectionConstants.ClassificationConnectionName, CPF.ConnectionConstants.ClassificationConnectionID, typeof(CPF.CamstarProviderConnectionPoint), AllowsMultipleConnections = true, ConnectionDataType = typeof(OM.SubClassificationEnum), ActivationType = CPF.ConnectionActivationType.OnDemand)]
        public override object GetGenericConnectionData(Type connectionDataType)
        {
            object result = null;

            if (connectionDataType == typeof(OM.SubClassificationEnum))
                result = EventDataDetailType;

            return result;
        }

        #endregion

        #region Private Member Variables

        private CWC.NamedObject _org = new CWC.NamedObject();
        private CWC.NamedObject _classif = new CWC.NamedObject();
        private CWC.NamedObject _subClassif = new CWC.NamedObject();
        private CWC.Label _confirmationLabel = new CWC.Label();

        private const string mkEventDetailExpression = "ServiceDetail.EventDataDetail";
        private const string mkClearEventArgumentParameter = "cleardata";
        private const string mkNoClearEventArgumentParameter = "notcleardata";
        private const string mkConfirmationScriptKey = "StartConfirmMessage";

        #endregion

    }//ClassificationSetupControl
}
