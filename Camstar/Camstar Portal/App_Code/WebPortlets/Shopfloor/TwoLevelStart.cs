// Copyright Siemens 2023 
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Web;
using System.Web.UI;

using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.Personalization;

/// <summary>
/// Portal Studio requirements for TwoLevelStart:
/// - Adding to Child Container Grid:
///     - requires Parent ContainerList element named "Details_ContainerName"
///     - requires Add Button element named "AddButton"
/// </summary>
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class TwoLevelStart : MatrixWebPart, IPostBackEventHandler
    {

        protected virtual JQDataGrid ChildContainer { get { return Page.FindCamstarControl("ChildDetails_ChildContainersGrid") as JQDataGrid; } }
        protected virtual CWC.Button AddButton { get { return Page.FindCamstarControl("ChildDetails_Add") as CWC.Button; } }
        protected virtual CWC.TextBox ParentContainerName { get { return Page.FindCamstarControl("Details_ContainerName") as CWC.TextBox; } }
        protected virtual CWC.NamedObject ChildLevel { get { return Page.FindCamstarControl("ChildDetails_Level") as CWC.NamedObject; } }
        protected virtual CWC.TextBox ChildCount { get { return Page.FindCamstarControl("ChildDetails_Count") as CWC.TextBox; } }
        protected virtual CWC.CheckBox GenerateNameParent { get { return Page.FindCamstarControl("Details_AutoNumber") as CWC.CheckBox; } }
        protected virtual CWC.CheckBox GenerateNameChild { get { return Page.FindCamstarControl("Details_AutoNumberChild") as CWC.CheckBox; } }
        protected virtual CWC.NamedObject ParentNumberingRule { get { return Page.FindCamstarControl("Details_AutoNumberRule") as CWC.NamedObject; } }
        protected virtual CWC.NamedObject ChildNumberingRule { get { return Page.FindCamstarControl("Details_AutoNumberRuleChild") as CWC.NamedObject; } }

        protected virtual CWC.NamedObject Level_Editing { get { return Page.FindCamstarControl("Level_Editing") as CWC.NamedObject; } }
        protected virtual CWC.TextBox ContainerName_Editing { get { return Page.FindCamstarControl("ContainerName_Editing") as CWC.TextBox; } }
        protected virtual CWC.CheckBox AutoNumber_Hidden { get { return Page.FindCamstarControl("AutoNumber_Hidden") as CWC.CheckBox; } }
        protected virtual CWC.CheckBox AutoNumberChild_Hidden { get { return Page.FindCamstarControl("AutoNumberChild_Hidden") as CWC.CheckBox; } }
        protected virtual CWC.CheckBox IsIndividualContainer { get { return Page.FindCamstarControl("DBStart_IsIndividualContainer") as CWC.CheckBox; } }
        protected virtual CWC.TitleControl ChildContainerInfoTitle { get { return Page.FindCamstarControl("ChildDetails_ContainerInfo") as CWC.TitleControl; } }
        protected virtual ToggleContainer AdditionalFields { get { return Page.FindCamstarControl("AdditionalChildInfo") as ToggleContainer; } }

        protected string _ParentNumRuleFormat = string.Empty;
        protected string _ChildNumRuleFormat = string.Empty;
        private const string _SequenceNumPlaceholder = "seq_num";

        protected virtual string NoNumberingRuleDefinedMessage
        {
            get
            {
                var labelCache = FrameworkManagerUtil.GetLabelCache(HttpContext.Current.Session);
                return labelCache.GetLabelByName("Start_NoNumberingRuleDefined").Value;
            }
        }

        protected virtual string ParentNumberingRuleChangedMessage
        {
            get
            {
                var labelCache = FrameworkManagerUtil.GetLabelCache(HttpContext.Current.Session);
                return labelCache.GetLabelByName("TwoLevelStart_ParentNumberingRuleChange").Value;
            }
        }

        protected virtual string ChildNumberingRuleChangedMessage
        {
            get
            {
                var labelCache = FrameworkManagerUtil.GetLabelCache(HttpContext.Current.Session);
                return labelCache.GetLabelByName("TwoLevelStart_ChildNumberingRuleChange").Value;
            }
        }

        //---------------------------------------------------
        // Label Text: Child Count
        //---------------------------------------------------
        protected virtual string ChildCountLbl
        {
            get
            {
                var labelCache = FrameworkManagerUtil.GetLabelCache(HttpContext.Current.Session);
                return labelCache.GetLabelByName("Container_ChildCount").Value;
            }
        }

        //---------------------------------------------------
        // Label Text: Child Containers
        //---------------------------------------------------
        protected virtual string ChildContainersGridLbl
        {
            get
            {
                var labelCache = FrameworkManagerUtil.GetLabelCache(HttpContext.Current.Session);
                return labelCache.GetLabelByName("Container_ChildContainers").Value;
            }
        }

        //---------------------------------------------------
        // Label Text: Container Count
        //---------------------------------------------------
        protected virtual string ContainerCountLbl
        {
            get
            {
                var labelCache = FrameworkManagerUtil.GetLabelCache(HttpContext.Current.Session);
                return labelCache.GetLabelByName("DBStartContainerCountLbl").Value;
            }
        }

        //---------------------------------------------------
        // Label Text: Containers
        //---------------------------------------------------
        protected virtual string ContainersGridLbl
        {
            get
            {
                var labelCache = FrameworkManagerUtil.GetLabelCache(HttpContext.Current.Session);
                return labelCache.GetLabelByName("DBStartContainersGridLbl").Value;
            }
        }

        //---------------------------------------------------
        // Label Text: Hide additional fields (Child Container details)
        //---------------------------------------------------
        protected virtual string HideFields_ChildContainerDetails
        {
            get
            {
                var labelCache = FrameworkManagerUtil.GetLabelCache(HttpContext.Current.Session);
                return labelCache.GetLabelByName("HideFields_ChildContainerDetails").Value;
            }
        }

        //---------------------------------------------------
        // Label Text: Show additional fields (Child Container details)
        //---------------------------------------------------
        protected virtual string Lbl_ShowMoreFields_ChildContainerDetails
        {
            get
            {
                var labelCache = FrameworkManagerUtil.GetLabelCache(HttpContext.Current.Session);
                return labelCache.GetLabelByName("Lbl_ShowMoreFields_ChildContainerDetails").Value;
            }
        }

        //---------------------------------------------------
        // Label Text: Hide additional fields (Individual Container details)
        //---------------------------------------------------
        protected virtual string Lbl_HideFields_IndContainerDetails
        {
            get
            {
                var labelCache = FrameworkManagerUtil.GetLabelCache(HttpContext.Current.Session);
                return labelCache.GetLabelByName("Lbl_HideFields_IndContainerDetails").Value;
            }
        }

        //---------------------------------------------------
        // Label Text: Show additional fields (Individual Container details)
        //---------------------------------------------------
        protected virtual string Lbl_ShowMoreFields_IndContainerDetails
        {
            get
            {
                var labelCache = FrameworkManagerUtil.GetLabelCache(HttpContext.Current.Session);
                return labelCache.GetLabelByName("Lbl_ShowMoreFields_IndContainerDetails").Value;
            }
        }

        //---------------------------------------------------
        // Label Text: Generate Child Names Automatically
        //---------------------------------------------------
        protected virtual string AutoNumber_GenerateChildNamesAutomatically
        {
            get
            {
                var labelCache = FrameworkManagerUtil.GetLabelCache(HttpContext.Current.Session);
                return labelCache.GetLabelByName("AutoNumber_GenerateChildNamesAutomatically").Value;
            }
        }

        //---------------------------------------------------
        // Label Text: Generate Names Automatically
        //---------------------------------------------------
        protected virtual string AutoNumber_GenerateNamesAutomatically
        {
            get
            {
                var labelCache = FrameworkManagerUtil.GetLabelCache(HttpContext.Current.Session);
                return labelCache.GetLabelByName("AutoNumber_GenerateNamesAutomatically").Value;
            }
        }

        //---------------------------------------------------
        // Label Text: Child Container Info
        //---------------------------------------------------
        protected virtual string ChildContainerInfoSection
        {
            get
            {
                var labelCache = FrameworkManagerUtil.GetLabelCache(HttpContext.Current.Session);
                return labelCache.GetLabelByName("ChildContainerInfoSection").Value;
            }
        }

        //---------------------------------------------------
        // Label Text: Individual Container Info
        //---------------------------------------------------
        protected virtual string DBStartIndContainerInfoLbl
        {
            get
            {
                var labelCache = FrameworkManagerUtil.GetLabelCache(HttpContext.Current.Session);
                return labelCache.GetLabelByName("DBStartIndContainerInfoLbl").Value;
            }
        }

        public TwoLevelStart()
        {

        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // For responsive mode check box is moved to the ContainerName cell 
            if (IsResponsive)
            {
                var anCell = GenerateNameParent.Parent;
                var i = anCell.Controls.IndexOf(GenerateNameParent);
                if (i != -1)
                {
                    var ctl = anCell.Controls[i];
                    anCell.Controls.RemoveAt(i);
                    ParentContainerName.Parent.Controls.Add(ctl);
                }

                anCell = GenerateNameChild.Parent;
                i = anCell.Controls.IndexOf(GenerateNameChild);
                if (i != -1)
                {
                    var ctl = anCell.Controls[i];
                    anCell.Controls.RemoveAt(i);
                    ChildLevel.Parent.Controls.Add(ctl);
                }

                (Page.FindCamstarControl("ChildDetails_ContainerInfo") as CWC.TitleControl).Width = System.Web.UI.WebControls.Unit.Empty;
                ChildContainer.Width = System.Web.UI.WebControls.Unit.Empty;
            }

            ChildContainer.GridContext.RowUpdated += ChildContainersGrid_RowUpdated;
            ChildContainer.GridContext.RowDeleted += ChildContainersGrid_RowDeleted;

            ParentNumberingRule.DataChanged += ParentNumberingRule_DataChanged;
            GenerateNameParent.CheckControl.CheckedChanged += GenerateNameParentCheckControl_CheckedChanged;
            GenerateNameChild.CheckControl.CheckedChanged += GenerateNameChildCheckControl_CheckedChanged;

            if (PrimaryServiceType.Equals("DBStart") || PrimaryServiceType.Equals("DBStartSimple"))
            {
                IsIndividualContainer.Visible = true;
            }

            if (PrimaryServiceType.Equals("DBStartSimple"))
            {
                ChildContainer.DataSubmissionMode = Personalization.DataSubmissionModeType.Skip;
                Personalization.GridDataSettingsBase childContainerSettings = new Personalization.GridDataSettingsBase();
                Personalization.JQNavigatorAction[] childNavigatorActions = new Personalization.JQNavigatorAction[5];

                childNavigatorActions[0] = new Personalization.JQNavigatorAction
                {
                    Action = Personalization.JQGridNavActionType.Refresh
                    ,
                    Enable = true
                    ,
                    Visible = true
                };

                childNavigatorActions[1] = new Personalization.JQNavigatorAction
                {
                    Action = Personalization.JQGridNavActionType.Delete
                    ,
                    Enable = true
                    ,
                    Visible = true
                };

                childNavigatorActions[2] = new Personalization.JQNavigatorAction
                {
                    Action = Personalization.JQGridNavActionType.Add
                   ,
                    Enable = false
                   ,
                    Visible = false
                };

                childNavigatorActions[3] = new Personalization.JQNavigatorAction
                {
                    Action = Personalization.JQGridNavActionType.Edit
                   ,
                    Enable = false
                   ,
                    Visible = false
                };

                childNavigatorActions[4] = new Personalization.JQNavigatorAction
                {
                    Action = Personalization.JQGridNavActionType.Delete
                   ,
                    Enable = true
                   ,
                    Visible = true
                };

                childContainerSettings = ChildContainer.Settings;

                (childContainerSettings as Personalization.GridDataSettingsItemList).EditorSettings.EditingMode = Personalization.JQEditingModes.Disabled;
                (childContainerSettings as Personalization.GridDataSettingsItemList).EditorSettings.AddRowMode = Personalization.AddRowModes.AddButton;

                childContainerSettings.NavigatorActions = childNavigatorActions;

                ChildContainer.Settings = childContainerSettings;

            }
        }

        protected virtual void GenerateNameChildCheckControl_CheckedChanged(object sender, EventArgs e)
        {
            if (ChildContainer.Data != null && (ChildContainer.Data as StartDetails[]).Length > 0)
                ClearChildContainersGrid(ChildNumberingRuleChangedMessage);
        }

        protected virtual void GenerateNameParentCheckControl_CheckedChanged(object sender, EventArgs e)
        {
            if (ChildContainer.Data != null && (ChildContainer.Data as StartDetails[]).Length > 0)
                ClearChildContainersGrid(ParentNumberingRuleChangedMessage);
        }

        protected virtual void ParentNumberingRule_DataChanged(object sender, EventArgs e)
        {
            string parentNumberingRule_PreviousValue = Page.DataContract.GetValueByName<string>("_ParentNumberingRule_PreviousValue");

            if (!string.IsNullOrEmpty(parentNumberingRule_PreviousValue) &&
                GenerateNameParent.IsChecked &&
                ParentNumberingRule.Data.ToString() != parentNumberingRule_PreviousValue &&
                ChildContainer.Data != null &&
                (ChildContainer.Data as StartDetails[]).Length > 0)
            {
                ClearChildContainersGrid(ParentNumberingRuleChangedMessage);
            }
        }

        protected virtual ResponseData ChildContainersGrid_RowDeleted(object sender, JQGridEventArgs args)
        {
            if (ChildContainer.Data != null)
            {
                if ((ChildContainer.Data as StartDetails[]).Length == 0)
                {
                    Page.DataContract.SetValueByName("_GenerateNameParent_PreviousValue", false);
                    Page.DataContract.SetValueByName("_GenerateNameChild_PreviousValue", false);
                }
            }

            args.State.Action = "Reload";
            args.Cancel = true;

            return ChildContainer.GridContext.Reload(args.State);
        }

        protected virtual ResponseData ChildContainersGrid_RowUpdated(object sender, JQGridEventArgs args)
        {
            StartDetails startDetails = ((sender as ItemDataContext).Data as StartDetails[])[Convert.ToInt16(args.Context.SelectedRowID)];
            if (startDetails != null)
            {
                // Adding to Grid
                if (startDetails.ContainerName == null && startDetails.Level == null)
                {

                    if (ChildLevel.Data != null)
                    {
                        string childLevel = Page.DataContract.GetValueByName<string>("_ChildLevel");
                        if (string.IsNullOrEmpty(childLevel))
                            childLevel = ChildLevel.Data.ToString();
                        startDetails.Level = new NamedObjectRef(childLevel, "ContainerLevel");
                    }

                    string childContainerPrefix = Page.DataContract.GetValueByName<string>("_ChildContainerPrefix");
                    string childContainerSuffix = Page.DataContract.GetValueByName<string>("_ChildContainerSuffix");

                    if (!string.IsNullOrEmpty(childContainerPrefix) && !string.IsNullOrEmpty(childContainerSuffix) && (ChildContainer.Data as StartDetails[]).Length > 1)
                        startDetails.ContainerName = childContainerPrefix + childContainerSuffix;

                    if ((ChildContainer.Data as StartDetails[]).Length == 1)
                    {
                        Page.DataContract.SetValueByName("_GenerateNameParent_PreviousValue", GenerateNameParent.IsChecked);
                        Page.DataContract.SetValueByName("_GenerateNameChild_PreviousValue", GenerateNameChild.IsChecked);
                    }
                }
            }

            args.State.Action = "Reload";
            args.Cancel = true;

            return ChildContainer.GridContext.Reload(args.State);
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            //maintain Add button state
            AddButton.Enabled = !(ParentContainerName.IsEmpty || ChildLevel.IsEmpty);

            //add Javascript for textbox onchange events
            string jsToggleAddButton = "var x=(this.value!='' && $('#{0}').val()!='' && $('#{1}').val()!='');if(x){{$('#" + AddButton.ClientID + "').removeAttr('disabled');}}";
            ParentContainerName.TextControl.Attributes.Add("onchange", string.Format(jsToggleAddButton, ChildLevel.TextEditControl.ClientID, ChildCount.TextControl.ClientID));
            ChildLevel.TextEditControl.Attributes.Add("onchange", string.Format(jsToggleAddButton, ParentContainerName.TextControl.ClientID, ChildCount.TextControl.ClientID));
            ChildCount.TextControl.Attributes.Add("onchange", string.Format(jsToggleAddButton, ChildLevel.TextEditControl.ClientID, ParentContainerName.TextControl.ClientID));
            AddButton.Attributes.Add("onclick", string.Format("if($('#{0}').val()=='' && $('#{1}').val()=='' && $('#{2}').val()==''){{alert('Container Name,  Container Level and Child Count are required fields'); return false;}}", ChildLevel.TextEditControl.ClientID, ParentContainerName.TextControl.ClientID, ChildCount.TextControl.ClientID));
        }

        protected override void OnPreLoad(object sender, EventArgs e)
        {
            base.OnPreLoad(sender, e);
            if (IsIndividualContainer.IsChecked)
            {
                ChildCount.LabelText = ContainerCountLbl;
                ChildContainer.LabelText = ContainersGridLbl;
                GenerateNameChild.LabelText = AutoNumber_GenerateNamesAutomatically;
                ChildContainerInfoTitle.LabelText = DBStartIndContainerInfoLbl;
                AdditionalFields.ExpandedLabelName = null;
                AdditionalFields.ExpandedLabelText = Lbl_HideFields_IndContainerDetails;
                AdditionalFields.CollapsedLabelName = null;
                AdditionalFields.CollapsedLabelText = Lbl_ShowMoreFields_IndContainerDetails;

                Personalization.GridDataSettingsBase childContainerSettings = new Personalization.GridDataSettingsBase();
                childContainerSettings = ChildContainer.Settings;
                GridLocaleText localeTextAdd = new GridLocaleText();
                localeTextAdd.Path = "camstar.EditingDialog.addTitle";
                localeTextAdd.LabelText = DBStartIndContainerInfoLbl;
                GridLocaleText localeTextEdit = new GridLocaleText();
                localeTextEdit.Path = "camstar.EditingDialog.editTitle";
                localeTextEdit.LabelText = DBStartIndContainerInfoLbl;
                (childContainerSettings as Personalization.GridDataSettingsItemList).Locales[0] = localeTextAdd;
                (childContainerSettings as Personalization.GridDataSettingsItemList).Locales[1] = localeTextEdit;
                ChildContainer.Settings = childContainerSettings;

            }
            else
            {
                ChildCount.LabelText = ChildCountLbl;
                GenerateNameChild.LabelText = AutoNumber_GenerateChildNamesAutomatically;
                ChildContainerInfoTitle.LabelText = ChildContainerInfoSection;
                ChildContainer.LabelText = ChildContainersGridLbl;
                AdditionalFields.ExpandedLabelName = null;
                AdditionalFields.ExpandedLabelText = HideFields_ChildContainerDetails;
                AdditionalFields.CollapsedLabelName = null;
                AdditionalFields.CollapsedLabelText = Lbl_ShowMoreFields_ChildContainerDetails;

                Personalization.GridDataSettingsBase childContainerSettings = new Personalization.GridDataSettingsBase();
                childContainerSettings = ChildContainer.Settings;
                GridLocaleText localeTextAdd = new GridLocaleText();
                localeTextAdd.Path = "camstar.EditingDialog.addTitle";
                localeTextAdd.LabelText = ChildContainerInfoSection;
                GridLocaleText localeTextEdit = new GridLocaleText();
                localeTextEdit.Path = "camstar.EditingDialog.editTitle";
                localeTextEdit.LabelText = ChildContainerInfoSection;
                (childContainerSettings as Personalization.GridDataSettingsItemList).Locales[0] = localeTextAdd;
                (childContainerSettings as Personalization.GridDataSettingsItemList).Locales[1] = localeTextEdit;
                ChildContainer.Settings = childContainerSettings;
            }
            ChildContainer.ApplyFieldPersonalization();
        }

        public virtual void Add(object sender, EventArgs e)
        {
            Page.StatusBar.ClearMessage();

            bool shouldAdd = this.ValidateAutoNumbering();
            string parentNumberRuleData = null;
            if (ParentNumberingRule.Data != null)
                parentNumberRuleData = ParentNumberingRule.Data.ToString();
            else
                parentNumberRuleData = null;

            // Current Values now become Previous Values
            Page.DataContract.SetValueByName("_GenerateNameParent_PreviousValue", GenerateNameParent.IsChecked);
            Page.DataContract.SetValueByName("_GenerateNameChild_PreviousValue", GenerateNameChild.IsChecked);
            Page.DataContract.SetValueByName("_ParentNumberingRule_PreviousValue", parentNumberRuleData);

            if (shouldAdd)
            {
                //Child Details
                CWC.TextBox _Qty = Page.FindCamstarControl("ChildDetails_Qty") as CWC.TextBox;
                CWC.NamedObject _UOM = Page.FindCamstarControl("ChildDetails_UOM") as CWC.NamedObject;
                CWC.TextBox _Qty2 = Page.FindCamstarControl("ChildDetails_Qty2") as CWC.TextBox;
                CWC.NamedObject _UOM2 = Page.FindCamstarControl("ChildDetails_UOM2") as CWC.NamedObject;
                CWC.TextBox _Count = Page.FindCamstarControl("ChildDetails_Count") as CWC.TextBox;

                if (ChildLevel.Data != null)
                    Page.DataContract.SetValueByName("_ChildLevel", ChildLevel.Data.ToString());

                try
                {
                    //Validate Entry
                    if (ParentContainerName.IsEmpty && !GenerateNameParent.IsChecked)
                        ThrowInvalidInputError(ParentContainerName, "Container Name", false);
                    else if (ChildLevel.IsEmpty)
                        ThrowInvalidInputError(ChildLevel, "Container Level", false);
                    else if (_Count.IsEmpty || !System.Text.RegularExpressions.Regex.IsMatch(_Count.TextControl.Text, @"^\d*\.?\d*$"))
                        ThrowInvalidInputError(_Count, "Child Count", true);

                    Page.DataContract.SetValueByName("_usingParentNumberingRule", GenerateNameParent.IsChecked);
                    Page.DataContract.SetValueByName("_usingChildNumberingRule", GenerateNameChild.IsChecked);

                    AutoNumber_Hidden.Data = GenerateNameParent.IsChecked;
                    AutoNumberChild_Hidden.Data = GenerateNameChild.IsChecked;

                    bool usingParentNumberingRule = Page.DataContract.GetValueByName<bool>("_usingParentNumberingRule");
                    if (!usingParentNumberingRule)
                        Page.DataContract.SetValueByName("_usingParentNumberingRule", false);

                    bool usingChildNumberingRule = Page.DataContract.GetValueByName<bool>("_usingChildNumberingRule");
                    if (!usingChildNumberingRule)
                        Page.DataContract.SetValueByName("_usingChildNumberingRule", false);

                    if (usingParentNumberingRule)
                    {
                        _ParentNumRuleFormat = this.GetParentNumRuleFormat();
                    }

                    if (usingChildNumberingRule)
                    {
                        _ChildNumRuleFormat = this.GetChildNumRuleFormat();
                    }

                    if (ChildLevel.Data != null)
                        Level_Editing.Data = ChildLevel.Data;

                    //Prep Entry
                    List<StartDetails> newChildContainers = GetContainersToSubmit(true).ToList();
                    int startPosition = newChildContainers.Count;
                    int endPosition = startPosition + int.Parse(_Count.Data.ToString());

                    StartDetails c;//New Child Container

                    for (int i = startPosition; i < endPosition; i++)
                    {
                        this.SetChildContainerPrefix();
                        this.SetChildContainerSuffix(i, endPosition);

                        c = new StartDetails()
                        {
                            ContainerName = Page.DataContract.GetValueByName<string>("_ChildContainerPrefix") + Page.DataContract.GetValueByName<string>("_ChildContainerSuffix"),
                            Level = new NamedObjectRef(ChildLevel.Data.ToString())
                        };

                        if (_Qty.Data != null)
                            c.Qty = (double)_Qty.Data;


                        if (_UOM.Data != null)
                            c.UOM = new NamedObjectRef(_UOM.Data.ToString());

                        if (_Qty2.Data != null)
                            c.Qty2 = (double)_Qty2.Data;

                        if (_UOM2.Data != null)
                            c.UOM2 = new NamedObjectRef(_UOM2.Data.ToString());

                        //add new child containers
                        newChildContainers.Add(c);
                    }
                    var headers = ChildContainer.Settings.Columns;
                    RecordSet newReCSet = new RecordSet()
                    {
                        Headers = headers.Select(h => new Header { Name = h.Name, TypeCode = TypeCode.String }).ToArray(),
                        TotalCount = endPosition,
                        Rows = newChildContainers.Select(r1 => new Row
                        {
                            Values = new string[] { r1.ContainerName.Value.ToString(), r1.Level.Name, ParentContainerName.TextControl.Text,
                            (r1.Qty!=null&&r1.Qty.Value!=0.0?r1.Qty.Value.ToString():""), (r1.UOM!=null?r1.UOM.Name:""),
                            (r1.Qty2!=null&&r1.Qty2.Value!=0.0?r1.Qty2.Value.ToString():""), (r1.UOM2!=null?r1.UOM2.Name:"")}
                        }).ToArray()
                    };

                    //Update Child Containers
                    ChildContainer.Data = newChildContainers.ToArray();
                    //NOTE: above call does not seem to populate whatever datasource is needed for the data to be persisted, submitted.  This should be reviewed
                    ChildContainer.OriginalData = newChildContainers.ToArray();


                    //update counts
                    CWC.TextBox totalQty = Page.FindCamstarControl("Details_TotalQty") as CWC.TextBox;
                    CWC.TextBox totalQty2 = Page.FindCamstarControl("Details_TotalQty2") as CWC.TextBox;
                    CWC.TextBox totalChildCount = Page.FindCamstarControl("Details_TotalChildCount") as CWC.TextBox;

                    totalQty.Text = newReCSet.Rows.Sum(r => double.Parse(!string.IsNullOrEmpty(r.Values[3]) ? r.Values[3] : "0")).ToString();
                    totalQty2.Text = newReCSet.Rows.Sum(r => double.Parse(!string.IsNullOrEmpty(r.Values[5]) ? r.Values[5] : "0")).ToString();
                    totalChildCount.Text = newReCSet.TotalCount.ToString();

                }
                catch (Exception ex)
                {
                    DisplayMessage(new ResultStatus(ex.Message, false));
                }
            }
        }

        public virtual void ThrowInvalidInputError(Control c, string title, bool isNumeric)
        {
            if (c as CWC.TextBox != null)
                (c as CWC.TextBox).BorderColor = System.Drawing.Color.Red;
            if (c as CWC.NamedObject != null)
                (c as CWC.NamedObject).BorderColor = System.Drawing.Color.Red;

            LabelCache labelCache = FrameworkManagerUtil.GetLabelCache(Page.Session);
            string error = string.Format(isNumeric
                ? labelCache.GetLabelByName("Lbl_InvalidDigitInput").Value
                : labelCache.GetLabelByName("Lbl_InvalidInput").Value, title);
            throw new Exception(error);
        }

        public override void GetInputData(Service serviceData)
        {
            base.GetInputData(serviceData);
            if (serviceData is Start)
                (serviceData as Start).Details.ChildContainers = GetContainersToSubmit(false);

            if (PrimaryServiceType.Equals("DBStart"))
            {
                (serviceData as DBStart).IsIndividualContainer = IsIndividualContainer.IsChecked;
            }

            if (PrimaryServiceType.Equals("DBStartSimple"))
            {
                (serviceData as DBStartSimple).IsIndividualContainer = IsIndividualContainer.IsChecked;
                if (ChildContainer != null && ChildContainer.TotalRowCount > 0)
                {
                    StartDetails[] childData = ChildContainer.Data as StartDetails[];

                    string[] childNames = ((IEnumerable)childData).Cast<StartDetails>()
                                .Select(x => x.ContainerName.ToString())
                                .ToArray();

                    string sNames = String.Join("|", childNames);
                    (serviceData as DBStartSimple).ContainerNamesStr = String.Join("|", sNames);
                }

                CWC.TextBox _ChildQty = Page.FindCamstarControl("ChildDetails_Qty") as CWC.TextBox;
                CWC.NamedObject _ChildUOM = Page.FindCamstarControl("ChildDetails_UOM") as CWC.NamedObject;
                CWC.TextBox _ChildQty2 = Page.FindCamstarControl("ChildDetails_Qty2") as CWC.TextBox;
                CWC.NamedObject _ChildUOM2 = Page.FindCamstarControl("ChildDetails_UOM2") as CWC.NamedObject;
                CWC.TextBox _ChildCount = Page.FindCamstarControl("ChildDetails_Count") as CWC.TextBox;


                (serviceData as DBStartSimple).Details.DefaultChildLevel = ChildLevel.Data as NamedObjectRef;
                if (_ChildQty.Data != null)
                    (serviceData as DBStartSimple).Details.DefaultChildQty = (double)_ChildQty.Data;

                if (_ChildUOM.Data != null)
                    (serviceData as DBStartSimple).Details.DefaultChildUOM = new NamedObjectRef(_ChildUOM.Data.ToString());

                if (_ChildQty2.Data != null)
                    (serviceData as DBStartSimple).Details.DefaultChildQty2 = (double)_ChildQty2.Data;

                if (_ChildUOM2.Data != null)
                    (serviceData as DBStartSimple).Details.DefaultChildUOM2 = new NamedObjectRef(_ChildUOM2.Data.ToString());

                (serviceData as DBStartSimple).Details.ChildContainers = null;
            }
        }

        public override bool PreExecute(Info serviceInfo, Service serviceData)
        {
            bool result = base.PreExecute(serviceInfo, serviceData);

            // Make sure that Numbering Rules are defined if they are being used
            if ((GenerateNameParent.IsChecked && ParentNumberingRule.Data == null) ||
                (GenerateNameChild.IsChecked && ChildNumberingRule.Data == null))
            {
                result = false;
                Page.DisplayWarning(NoNumberingRuleDefinedMessage);
            }

            return result;
        }

        public override void PostExecute(ResultStatus status, Service serviceData)
        {
            base.PostExecute(status, serviceData);

            if (status.IsSuccess)
                ResetHistory();
            // If transaction is not successfull, display the correct Child Container Names in the Grid
            else
            {
                int index = 0;
                if (ChildContainer.Data != null)
                {
                    List<string> childContainerNames_Previous = Page.DataContract.GetValueByName<List<string>>("_ChildContainerNames_Previous");
                    if (childContainerNames_Previous != null && (ChildContainer.Data as Array).Cast<StartDetails>().ToArray().Length <= childContainerNames_Previous.Count)
                    {
                        foreach (var row in (ChildContainer.Data as Array).Cast<StartDetails>().ToArray())
                            row.ContainerName = childContainerNames_Previous[index++];
                    }
                }
            }
        }

        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);

            if (e.Action.Caption.Equals("Reset"))
            {
                ResetHistory();
            }
        }

        // Because we are displaying the format of the Child Container Names, it is likely that the length of the string
        // will be greater than 30 characters. Given the different AutoNumbering scenarios, the Child Container Names
        // will be changed to something shorter than 30 characters to ensure that the BL will not validate the format
        // string which most likely will be longer than 30 characters and cause the transaction to fail. The BL uses
        // the - 01, - 02, - 03, etc in the case of using No Child Numbering Rule so we set the Child Container Names
        // to - 01, - 02, etc. The other cases do not really matter in terms of what is passed in.
        protected virtual StartDetails[] GetContainersToSubmit(bool addToGrid)
        {
            if (!addToGrid)
            {
                if (ChildContainer.Data != null)
                {
                    StartDetails[] details = (ChildContainer.Data as Array).Cast<StartDetails>().ToArray();
                    int index = 0;

                    List<string> childContainerNames_Previous = Page.DataContract.GetValueByName<List<string>>("_ChildContainerNames_Previous");
                    if (childContainerNames_Previous == null)
                        childContainerNames_Previous = new List<string>();

                    List<string> childContainerSuffixes = Page.DataContract.GetValueByName<List<string>>("_ChildContainerSuffixes");

                    bool usingParentNumberingRule = Page.DataContract.GetValueByName<bool>("_usingParentNumberingRule");
                    bool usingChildNumberingRule = Page.DataContract.GetValueByName<bool>("_usingChildNumberingRule");

                    foreach (var detail in details)
                    {
                        childContainerNames_Previous.Add(Convert.ToString(detail.ContainerName));
                        if (usingParentNumberingRule || usingChildNumberingRule)
                        {
                            if (usingParentNumberingRule && usingChildNumberingRule)
                                detail.ContainerName = string.Empty;

                            // Need to pass in just the - 01, - 02, - 03, etc.
                            else if (usingParentNumberingRule && !usingChildNumberingRule)
                                detail.ContainerName = childContainerSuffixes[index];

                            else if (!usingParentNumberingRule && usingChildNumberingRule)
                                detail.ContainerName = string.Empty;
                            index++;
                        }
                    }

                    Page.DataContract.SetValueByName("_ChildContainerNames_Previous", childContainerNames_Previous);

                    return details;
                }

            }
            return ChildContainer.Data != null ? (ChildContainer.Data as Array).Cast<StartDetails>().ToArray() : new StartDetails[] { };
        }

        // Reset all the Previous Values, to keep the state in sync.
        protected virtual void ResetHistory()
        {
            Page.DataContract.SetValueByName("_GenerateNameParent_PreviousValue", false);
            Page.DataContract.SetValueByName("_GenerateNameChild_PreviousValue", false);
            Page.DataContract.SetValueByName("_ParentNumberingRule_PreviousValue", string.Empty);

            Page.DataContract.SetValueByName("_usingParentNumberingRule", false);
            Page.DataContract.SetValueByName("_usingChildNumberingRule", false);

            Page.DataContract.SetValueByName("_ChildContainerSuffix", string.Empty);
            Page.DataContract.SetValueByName("_ChildContainerPrefix", string.Empty);

            Page.DataContract.SetValueByName("_ChildContainerSuffixes", new List<string>());
            Page.DataContract.SetValueByName("_ChildContainerNames_Previous", new List<string>());

            Page.DataContract.SetValueByName("_ChildLevel", string.Empty);

            AutoNumber_Hidden.Data = false;
            AutoNumberChild_Hidden.Data = false;

            ChildCount.LabelText = ChildCountLbl;
            GenerateNameChild.LabelText = AutoNumber_GenerateChildNamesAutomatically;
            ChildContainerInfoTitle.LabelText = ChildContainerInfoSection;
            ChildContainer.LabelText = ChildContainersGridLbl;
            AdditionalFields.ExpandedLabelName = null;
            AdditionalFields.ExpandedLabelText = HideFields_ChildContainerDetails;
            AdditionalFields.CollapsedLabelName = null;
            AdditionalFields.CollapsedLabelText = Lbl_ShowMoreFields_ChildContainerDetails;
            AdditionalFields.Reset();

            Personalization.GridDataSettingsBase childContainerSettings = new Personalization.GridDataSettingsBase();
            childContainerSettings = ChildContainer.Settings;
            GridLocaleText localeTextAdd = new GridLocaleText();
            localeTextAdd.Path = "camstar.EditingDialog.addTitle";
            localeTextAdd.LabelText = ChildContainerInfoSection;
            GridLocaleText localeTextEdit = new GridLocaleText();
            localeTextEdit.Path = "camstar.EditingDialog.editTitle";
            localeTextEdit.LabelText = ChildContainerInfoSection;
            (childContainerSettings as Personalization.GridDataSettingsItemList).Locales[0] = localeTextAdd;
            (childContainerSettings as Personalization.GridDataSettingsItemList).Locales[1] = localeTextEdit;
            ChildContainer.Settings = childContainerSettings;

            ChildContainer.ApplyFieldPersonalization();

            IsIndividualContainer.Data = false;
            IsIndividualContainer.IsChecked = false;

        }

        protected virtual void SetChildContainerSuffix(int i, int endPosition)
        {
            List<string> childContainerSuffixes = Page.DataContract.GetValueByName<List<string>>("_ChildContainerSuffixes");
            if (childContainerSuffixes == null)
                childContainerSuffixes = new List<string>();

            string childContainerSuffix = string.Empty;

            if (GenerateNameChild.IsChecked)
                childContainerSuffix = _ChildNumRuleFormat;
            else
                childContainerSuffix = " - " + (i + 1).ToString().PadLeft((endPosition < 100 ? 2 : endPosition.ToString().Length), char.Parse("0"));

            Page.DataContract.SetValueByName("_ChildContainerSuffix", childContainerSuffix);

            childContainerSuffixes.Add(childContainerSuffix);
            Page.DataContract.SetValueByName("_ChildContainerSuffixes", childContainerSuffixes);
        }

        protected virtual void SetChildContainerPrefix()
        {
            string childContainerPrefix = string.Empty;
            if (GenerateNameParent.IsChecked)
                childContainerPrefix = _ParentNumRuleFormat;
            else
                childContainerPrefix = ParentContainerName.TextControl.Text;

            Page.DataContract.SetValueByName("_ChildContainerPrefix", childContainerPrefix);
        }

        protected virtual string GetParentNumRuleFormat()
        {
            return GetNumRuleFormat(ParentNumberingRule.Data.ToString(), true);
        }

        protected virtual string GetChildNumRuleFormat()
        {
            return GetNumRuleFormat(ChildNumberingRule.Data.ToString(), false);
        }

        protected virtual string GetNumRuleFormat(string numberingRule, bool isParent)
        {
            string prefix = string.Empty;
            string suffix = string.Empty;

            if (numberingRule != null)
            {
                var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
                if (session != null)
                {
                    var service = new NumberingRuleMaintService(session.CurrentUserProfile);
                    var serviceData = new NumberingRuleMaint
                    {
                        ObjectToChange = new NamedObjectRef(numberingRule, "NumberingRule")
                    };

                    var request = new NumberingRuleMaint_Request
                    {
                        Info = new NumberingRuleMaint_Info
                        {
                            ObjectToChange = new Info(true),
                            ObjectChanges = new NumberingRuleChanges_Info
                            {
                                Prefix = new Info(true),
                                Suffix = new Info(true)
                            }
                        }
                    };

                    var result = new NumberingRuleMaint_Result();
                    ResultStatus status = service.Load(serviceData, request, out result);
                    if (status != null && status.IsSuccess)
                    {
                        prefix = result.Value.ObjectChanges.Prefix != null ? Convert.ToString(result.Value.ObjectChanges.Prefix.Value) : string.Empty;
                        suffix = result.Value.ObjectChanges.Suffix != null ? Convert.ToString(result.Value.ObjectChanges.Suffix.Value) : string.Empty;

                        if (!string.IsNullOrEmpty(prefix) && prefix.Substring(0, 1) == "\"" && prefix.Substring(prefix.Length - 1, 1) == "\"")
                            prefix = prefix.Substring(1, prefix.Length - 2);

                        if (!string.IsNullOrEmpty(suffix) && suffix.Substring(0, 1) == "\"" && suffix.Substring(suffix.Length - 1, 1) == "\"")
                            suffix = suffix.Substring(1, suffix.Length - 2);
                    }

                }
            }
            else
                return string.Empty;


            return prefix + _SequenceNumPlaceholder + suffix;
        }

        protected virtual bool ValidateChildLevelNumberingRule()
        {
            if (ChildLevel.Data != null)
            {
                var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
                if (session != null)
                {
                    var service = new ContainerLevelMaintService(session.CurrentUserProfile);
                    var serviceData = new ContainerLevelMaint()
                    {
                        ObjectToChange = new NamedObjectRef(ChildLevel.Data.ToString(), "ContainerLevel")
                    };

                    var request = new ContainerLevelMaint_Request()
                    {
                        Info = new ContainerLevelMaint_Info()
                        {
                            ObjectToChange = new Info(),
                            ObjectChanges = new ContainerLevelChanges_Info()
                            {
                                ContainerNumberingRule = new Info(true)
                            }
                        }
                    };

                    var result = new ContainerLevelMaint_Result();
                    ResultStatus resultStatus = service.Load(serviceData, request, out result);

                    if (result != null && resultStatus.IsSuccess)
                    {
                        if (result.Value.ObjectChanges.ContainerNumberingRule != null)
                            return true;
                    }
                }
            }

            return false;
        }

        protected virtual void ClearChildContainersGrid(string warningMessage)
        {
            if (warningMessage != null)
                Page.DisplayWarning(warningMessage);

            ChildContainer.Data = null;

            ResetHistory();
        }
        // Validate the different scenarios:
        //  1) If a numbering rule should be used, then one should be defined.
        //  2) All Child Containers either use a Parent Numbering Rule or they don't, Numbering Rules must be the same.
        //  3) All Child Containers either use a Child Numbering Rule or they don't, Numbering Rules can be different
        protected virtual bool ValidateAutoNumbering()
        {
            string parentNumberingRule_PreviousValue = Page.DataContract.GetValueByName<string>("_ParentNumberingRule_PreviousValue");
            if (parentNumberingRule_PreviousValue == null)
                parentNumberingRule_PreviousValue = string.Empty;

            bool generateNameParent_PreviousValue = Page.DataContract.GetValueByName<bool>("_GenerateNameParent_PreviousValue");
            bool generateNameChild_PreviousValue = Page.DataContract.GetValueByName<bool>("_GenerateNameChild_PreviousValue");


            string parentNumberRuleData = null;
            if (ParentNumberingRule.Data != null)
                parentNumberRuleData = ParentNumberingRule.Data.ToString();


            if ((GenerateNameParent.IsChecked && ParentNumberingRule.Data == null) ||
                (GenerateNameChild.IsChecked && ChildNumberingRule.Data == null))
            {
                Page.DisplayWarning(NoNumberingRuleDefinedMessage);
                return false;
            }
            else if (ChildContainer.Data != null && generateNameParent_PreviousValue != GenerateNameParent.IsChecked)
            {
                if ((ChildContainer.Data as StartDetails[]).Length > 0)
                {
                    ClearChildContainersGrid(ParentNumberingRuleChangedMessage);
                    return false;
                }
            }
            else if (ChildContainer.Data != null && generateNameChild_PreviousValue != GenerateNameChild.IsChecked)
            {
                if ((ChildContainer.Data as StartDetails[]).Length > 0)
                {
                    ClearChildContainersGrid(ChildNumberingRuleChangedMessage);
                    return false;
                }
            }
            else if (ChildContainer.Data != null &&
                ((parentNumberingRule_PreviousValue != parentNumberRuleData) &&
                (generateNameParent_PreviousValue && GenerateNameParent.IsChecked)))
            {
                if ((ChildContainer.Data as StartDetails[]).Length > 0)
                {
                    ClearChildContainersGrid(ParentNumberingRuleChangedMessage);
                    return false;
                }
            }

            if (!ValidateChildLevelNumberingRule() && GenerateNameChild.IsChecked)
            {
                ClearChildContainersGrid(NoNumberingRuleDefinedMessage);
                return false;
            }

            return true;
        }

        public virtual void RaisePostBackEvent(string eventArgument)
        {

        }
    }
}
