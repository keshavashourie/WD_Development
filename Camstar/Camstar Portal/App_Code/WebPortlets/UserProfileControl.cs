// Copyright Siemens 2023  
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.Constants;
using CamstarPortal.WebControls.Constants;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;

namespace Camstar.WebPortal.WebPortlets
{
    public class UserProfileControl : WebPartBase
    {
        public UserProfileControl()
        {
            this.Title = "User Profile";
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
        }

        protected override void CreateContentControls(ControlCollection contentControls)
        {
            mContainer = new Table();
            mContainer.CellPadding = 0;
            mContainer.CellSpacing = 0;
            mContainer.CssClass = CSSClass.UserProfileContainer;
            mContainer.Attributes.Add(HtmlAttributes.ID, HtmlAttributeValues.UserProfileContainerID);

            BuildContainerTable();

            contentControls.Add(mContainer);
        }

        protected override WebPartWrapperBase CreateWebPartWrapper()
        {
            return null;
        }

        /// <summary>
        /// Builds the main container table. (Grey portion of the UserProfile control)
        /// </summary>
        protected virtual void BuildContainerTable()
        {

            mContainerRow = new TableRow();
            mContainer.Rows.Add(mContainerRow);

            mContainerCell = new TableCell();
            mContainerCell.ColumnSpan = 3;
            mContainerCell.CssClass = CSSClass.TopCell;

            // build and add teh inner container
            BuildProfileTable();
            Div div = new Div();
            div.Controls.Add(mProfile);
            mContainerCell.Controls.Add(div);
            mContainerRow.Cells.Add(mContainerCell);
        }

        /// <summary>
        /// Builds the inner containter table.  (Blue portion of the UserProfile control)
        /// </summary>
        protected virtual void BuildProfileTable()
        {
            mProfile = new Table();
            mProfile.CellPadding = 0;
            mProfile.CellSpacing = 0;
            mProfile.CssClass = CSSClass.UserProfile;
            mProfile.Attributes.Add(HtmlAttributes.ID, HtmlAttributeValues.UserProfileID);

            mProfileRow = new TableRow();
            mProfile.Rows.Add(mProfileRow);

            mProfileCell = new TableCell();
            mProfileCell.ColumnSpan = 3;
            mProfileCell.CssClass = CSSClass.TopCell;

            BuildInnerProfileTable();
            Div div = new Div();
            //div.Controls.Add(BuildUserProfileHeader());
            div.Controls.Add(mInnerProfile);
            mProfileCell.Controls.Add(div);
            mProfileRow.Cells.Add(mProfileCell);
        }

        /// <summary>
        /// Builds the table element which will contain inputs, buttons and
        /// table containing the user details.
        /// </summary>
        protected virtual void BuildInnerProfileTable()
        {
            BuildInputAndButtonControls();

            mInnerProfile = new Table();
            mInnerProfile.CellPadding = 0;
            mInnerProfile.CellSpacing = 0;
            mInnerProfile.CssClass = CSSClass.UserProfileInner;

            // build row for "User ID", "Full Name" and "Title" textboxes
            mInnerProfileRow = new TableRow();
            mInnerProfileRow.CssClass = CSSClass.ControlRow;
            mInnerProfile.Rows.Add(mInnerProfileRow);


            // add "User ID" textbox
            mInnerProfileCell = new TableCell();
            mInnerProfileCell.HorizontalAlign = HorizontalAlign.Left;
            mInnerProfileCell.Controls.Add(mTextBoxUserID);
            mInnerProfileRow.Cells.Add(mInnerProfileCell);


            // add "Full Name" textbox
            mInnerProfileCell = new TableCell();
            mInnerProfileCell.HorizontalAlign = HorizontalAlign.Left;
            mInnerProfileCell.Controls.Add(mTextBoxFullName);
            mInnerProfileRow.Cells.Add(mInnerProfileCell);



            // add "Title" textbox
            mInnerProfileCell = new TableCell();
            mInnerProfileCell.HorizontalAlign = HorizontalAlign.Left;
            mInnerProfileCell.Controls.Add(mTextBoxTitle);
            mInnerProfileRow.Cells.Add(mInnerProfileCell);

            // build row for "Primary Language", "Secondary Language" and "Session Language" textboxes
            mInnerProfileRow = new TableRow();
            mInnerProfile.Rows.Add(mInnerProfileRow);
            mInnerProfileRow.CssClass = CSSClass.ControlRow;


            // add "Primary Language" textbox
            mInnerProfileCell = new TableCell();
            mInnerProfileCell.HorizontalAlign = HorizontalAlign.Left;
            mInnerProfileCell.Controls.Add(mTextBoxPrimaryLanguage);
            mInnerProfileRow.Cells.Add(mInnerProfileCell);

            // add "Secondary Language" textbox
            mInnerProfileCell = new TableCell();
            mInnerProfileCell.HorizontalAlign = HorizontalAlign.Left;
            mInnerProfileCell.Controls.Add(mTextBoxSecondaryLanguage);
            mInnerProfileRow.Cells.Add(mInnerProfileCell);


            // add "Session Language" textbox
            mInnerProfileCell = new TableCell();
            mInnerProfileCell.HorizontalAlign = HorizontalAlign.Left;
            mInnerProfileCell.Controls.Add(mTextBoxSessionLanguage);
            mInnerProfileRow.Cells.Add(mInnerProfileCell);

            // build row for user details table
            mInnerProfileRow = new TableRow();
            mInnerProfile.Rows.Add(mInnerProfileRow);
            mInnerProfileRow.CssClass = CSSClass.ControlRow;


            // add "Primary Organization" textbox
            mInnerProfileCell = new TableCell();
            mInnerProfileCell.HorizontalAlign = HorizontalAlign.Left;
            mInnerProfileCell.Controls.Add(mTextBoxOrganization);
            mInnerProfileRow.Cells.Add(mInnerProfileCell);
            mInnerProfileRow.Cells.Add(new TableCell());
            mInnerProfileRow.Cells.Add(new TableCell());
            mInnerProfileRow.Cells.Add(new TableCell());
            mInnerProfileRow.Cells.Add(new TableCell());

            // build row for user details table
            mInnerProfileRow = new TableRow();
            mInnerProfile.Rows.Add(mInnerProfileRow);
            mInnerProfileRow.CssClass = CSSClass.ControlRow;

            // add user details table
            mInnerProfileCell = new TableCell();
            mInnerProfileCell.ColumnSpan = 6;

            Div detailsContainer = new Div();
            detailsContainer.Controls.Add(BuildUserProfileDetailsTable());
            detailsContainer.CssClass = CSSClass.UserProfileDetailsContainer;
            mInnerProfileCell.Controls.Add(detailsContainer);
            mInnerProfileRow.Cells.Add(mInnerProfileCell);

        }

        /// <summary>
        /// Builds the input and button controls.
        /// </summary>
        protected virtual void BuildInputAndButtonControls()
        {
            // build "User ID" textbox
            mTextBoxUserID.ID = HtmlAttributeValues.UserIDTextBoxID;
            mTextBoxUserID.LabelControl.Text = GetLabelCacheValue(HtmlAttributeValues.UserIDLabelName, HtmlAttributeValues.UserIDTextBoxLabelText);
            mTextBoxUserID.ReadOnly = true;
            mTextBoxUserID.TextControl.Text = Context.User.Identity.Name;
            mTextBoxUserID.TextControl.ReadOnly = true;

            // build "Full Name" texbox
            mTextBoxFullName.ID = HtmlAttributeValues.FullNameTextBoxID;
            mTextBoxFullName.LabelControl.Text = GetLabelCacheValue(HtmlAttributeValues.FullNameLabelName, HtmlAttributeValues.FullNameTextBoxLabelText);
            mTextBoxFullName.ReadOnly = true;
            mTextBoxFullName.TextControl.Text = Context.Session[SessionConstants.FullName] == null ? string.Empty : Context.Session[SessionConstants.FullName].ToString();
            mTextBoxFullName.TextControl.ReadOnly = true;

            // build "Title" textbox
            mTextBoxTitle.ID = HtmlAttributeValues.TitleTextBoxID;
            mTextBoxTitle.LabelControl.Text = GetLabelCacheValue(HtmlAttributeValues.TitleTextLabelName, HtmlAttributeValues.TitleTextBoxLabelText);
            mTextBoxTitle.ReadOnly = true;
            mTextBoxTitle.TextControl.Text = "";
            mTextBoxTitle.TextControl.ReadOnly = true;

            // build "Primary Language" textbox
            mTextBoxPrimaryLanguage.ID = HtmlAttributeValues.PrimaryLanguageTextBoxID;
            mTextBoxPrimaryLanguage.LabelControl.Text = GetLabelCacheValue(HtmlAttributeValues.PrimaryLanguageLabelName, HtmlAttributeValues.PrimaryLanguageTextBoxLabelText);
            mTextBoxPrimaryLanguage.ReadOnly = true;
            mTextBoxPrimaryLanguage.TextControl.Text = Context.Session[SessionConstants.PrimaryLanguage] == null ? string.Empty : Context.Session[SessionConstants.PrimaryLanguage].ToString();
            mTextBoxPrimaryLanguage.TextControl.ReadOnly = true;

            // build "Secondary Language" textbox
            mTextBoxSecondaryLanguage.ID = HtmlAttributeValues.SecondaryLanguageTextBoxID;
            mTextBoxSecondaryLanguage.LabelControl.Text = GetLabelCacheValue(HtmlAttributeValues.SecondaryLanguageLabelName, HtmlAttributeValues.SecondaryLanguageTextBoxLabelText);
            mTextBoxSecondaryLanguage.ReadOnly = true;
            mTextBoxSecondaryLanguage.TextControl.Text = Context.Session[SessionConstants.SecondaryLanguage] == null ? string.Empty : Context.Session[SessionConstants.SecondaryLanguage].ToString();
            mTextBoxSecondaryLanguage.TextControl.ReadOnly = true;

            // build "Session Language" textbox
            mTextBoxSessionLanguage.ID = HtmlAttributeValues.SessionLanguageTextBoxID;
            mTextBoxSessionLanguage.LabelControl.Text = GetLabelCacheValue(HtmlAttributeValues.SessionLanguageLabelName, HtmlAttributeValues.SessionLanguageTextBoxLabelText);
            mTextBoxSessionLanguage.ReadOnly = true;
            FrameworkSession session = Camstar.WebPortal.FormsFramework.Utilities.FrameworkManagerUtil.GetFrameworkSession();
            mTextBoxSessionLanguage.TextControl.Text = !string.IsNullOrEmpty(session.CurrentUserProfile.Dictionary) ? session.CurrentUserProfile.Dictionary : mTextBoxPrimaryLanguage.TextControl.Text;
            mTextBoxSessionLanguage.TextControl.ReadOnly = true;


            // build "Primary Organization" textbox
            mTextBoxOrganization.ID = HtmlAttributeValues.PrimaryOrganizationTextBoxID;
            mTextBoxOrganization.LabelControl.Text = GetLabelCacheValue(HtmlAttributeValues.PrimaryOrganizationLabelName, HtmlAttributeValues.PrimaryOrganizationTextBoxLabelText);
            mTextBoxOrganization.ReadOnly = true;
            mTextBoxOrganization.TextControl.ReadOnly = true;
        }

        /// <summary>
        /// Builds the table element that will contain the user's primary,
        /// organization and role.
        /// </summary>
        /// <returns>System.Web.UI.WebControls.Table</returns>
        protected virtual Table BuildUserProfileDetailsTable()
        {
            GetEmployeeDetails();

            TableCell cell;
            TableRow row;
            Table tblDetails = new Table();

            tblDetails.CellPadding = 0;
            tblDetails.CellSpacing = 0;
            tblDetails.Attributes.Add(HtmlAttributes.ID, HtmlAttributeValues.UserProfileDetailsID);
            tblDetails.CssClass = CSSClass.UserProfileDetails;

            // build header row
            row = new TableRow();
            row.CssClass = CSSClass.HeaderRow;

            // organization column header
            mOrganizationHeader = new TableCell();
            mOrganizationHeader.ID = "OrganizationHeaderColumn";
            mOrganizationHeader.Controls.Add(new System.Web.UI.LiteralControl(string.Format("<span>{0}</span>", GetLabelCacheValue(ColumnHeader.OrganizationLabelName, ColumnHeader.Organization))));
            mOrganizationHeader.Controls.Add(BuildSortDirectionImage());
            mOrganizationHeader.Attributes.Add("column", "0");
            row.Cells.Add(mOrganizationHeader);

            // role column header
            mRoleHeader = new TableCell();
            mRoleHeader.ID = "RoleHeaderColumn";
            mRoleHeader.Controls.Add(new System.Web.UI.LiteralControl(string.Format("<span>{0}</span>", GetLabelCacheValue(ColumnHeader.RoleLabelName, ColumnHeader.Role))));
            mRoleHeader.Controls.Add(BuildSortDirectionImage());
            mRoleHeader.Attributes.Add("column", "1");
            row.Cells.Add(mRoleHeader);
            tblDetails.Rows.Add(row);

            // primary column header
            mPrimaryHeader = new TableCell();
            mPrimaryHeader.ID = "PrimaryHeaderColumn";
            mPrimaryHeader.Text = ColumnHeader.Primary;
            mPrimaryHeader.Controls.Add(new System.Web.UI.LiteralControl(string.Format("<span>{0}</span>", GetLabelCacheValue(ColumnHeader.PrimaryLabelName, ColumnHeader.Primary))));
            mPrimaryHeader.Controls.Add(BuildSortDirectionImage());
            mPrimaryHeader.Attributes.Add("column", "2");
            row.Cells.Add(mPrimaryHeader);

            // add user's primary, organization and role data...this will be removed once role
            // functionality is complete.
            if (mEmployeeDetails != null)
                for (int x = 0; x < mEmployeeDetails.Count; x++)
                {
                    row = new TableRow();
                    row.CssClass = x % 2 == 0 ? CSSClass.ItemRow : CSSClass.AlternateItemRow;

                    cell = new TableCell();
                    cell.Text = mEmployeeDetails[x].Organization;
                    row.Cells.Add(cell);

                    cell = new TableCell();
                    cell.Text = mEmployeeDetails[x].Roles;
                    row.Cells.Add(cell);

                    cell = new TableCell();
                    cell.Text = mEmployeeDetails[x].Propagate;
                    row.Cells.Add(cell);

                    tblDetails.Rows.Add(row);
                }

            return tblDetails;
        }

        /// <summary>
        /// Builds a table row elements that acts as a splitter.
        /// </summary>
        /// <returns></returns>
        protected virtual TableRow BuildSplitterRow()
        {
            TableRow splitterRow = new TableRow();
            TableCell splitterCell = new TableCell();
            splitterRow.Cells.Add(splitterCell);

            splitterCell.ColumnSpan = 6;
            splitterCell.CssClass = CSSClass.SplitterCell;

            return splitterRow;
        }

        /// <summary>
        /// Builds a div element that contains the user profile icon and title.
        /// </summary>
        /// <returns></returns>
        protected virtual Div BuildUserProfileHeader()
        {
            Div divHeader = new Div();
            divHeader.CssClass = CSSClass.HeaderDiv;
            divHeader.Controls.Add(new System.Web.UI.LiteralControl("<span>" + GetLabelCacheValue(HtmlAttributeValues.UserProfilePageTitleLabelName, HtmlAttributeValues.UserProfileTitle) + "</span>"));

            return divHeader;
        }

        /// <summary>
        /// Builds the temporary employee detail collection.  Remove once user roles
        /// are implemented.
        /// </summary>
        protected virtual void GetEmployeeDetails()
        {
            EmployeeChanges employee = Context.Session[SessionConstants.Employee] as EmployeeChanges;
            if (employee == null)
            {
                EmployeeMaintService maintService = new EmployeeMaintService(FrameworkManagerUtil.GetFrameworkSession(Page.Session).CurrentUserProfile);
                EmployeeMaint data = new EmployeeMaint();
                data.ObjectToChange = new NamedObjectRef(FrameworkManagerUtil.GetFrameworkSession(Page.Session).CurrentUserProfile.Name);
                EmployeeMaint_Request request = new EmployeeMaint_Request();
                request.Info = new EmployeeMaint_Info();
                request.Info.ObjectChanges = new EmployeeChanges_Info();
                request.Info.ObjectChanges.Name = new Info(true);
                request.Info.ObjectChanges.Description = new Info(true);
                request.Info.ObjectChanges.PrimaryOrganization = new Info(true);
                request.Info.ObjectChanges.Roles = new EmployeeRoleChanges_Info();
                request.Info.ObjectChanges.Roles.RequestValue = true;

                EmployeeMaint_Result result = null;
                ResultStatus status = maintService.Load(data, request, out result);
                if (status.IsSuccess && result.Value != null)
                {
                    employee = result.Value.ObjectChanges as EmployeeChanges;
                    Context.Session[SessionConstants.Employee] = employee;
                }
            }

            if (employee != null)
            {
                Camstar.WCF.ObjectStack.EmployeeRoleChanges[] roles = employee.Roles;
                mTextBoxTitle.TextControl.Text = employee.Description == null ? string.Empty : employee.Description.ToString();
                mTextBoxOrganization.TextControl.Text = employee.PrimaryOrganization == null ? string.Empty : employee.PrimaryOrganization.ToString();
                mEmployeeDetails = new EmployeeDetails();

                for (int x = 0; x < roles.Length; x++)
                {
                    mEmployeeDetails.Add(new EmployeeDetail(Convert.ToBoolean(roles[x].PropagateToChildOrgs.Value) ? "Y" : "N",
                                                            roles[x].Organization == null ? string.Empty : roles[x].Organization.Name.ToString(),
                                                            roles[x].Role == null ? string.Empty : roles[x].Role.Name.ToString()));
                }

                //ensure that at least 8 rows exist in grid
                int diff = 8 - roles.Length;

                for (int x = 0; x < diff; x++)
                {
                    mEmployeeDetails.Add(new EmployeeDetail("&nbsp;", "&nbsp;", "&nbsp;"));
                }
            }
        }

        /// <summary>
        /// Returns a down arrow that will be added to the column header of the user details
        /// table to signal sort direction.
        /// </summary>
        /// <returns>System.Web.UI.WebControls.Image</returns>
        protected virtual Image BuildSortDirectionImage()
        {
            Image imgSortDirection = new Image();
            imgSortDirection.ImageUrl = Images.Ascending;

            return imgSortDirection;
        }

        protected override IEnumerable<ScriptReference> GetScriptReferences()
        {
            List<ScriptReference> refs = base.GetScriptReferences().ToList<ScriptReference>();
            refs.Add(new ScriptReference("~/Scripts/ClientFramework/CamstarPortal.WebControls/UserProfile.js"));
            return refs;
        }

        protected override IEnumerable<ScriptDescriptor> GetScriptDescriptors()
        {
            List<ScriptDescriptor> list = base.GetScriptDescriptors().ToList<ScriptDescriptor>();

            ScriptControlDescriptor descriptor = new ScriptControlDescriptor("CamstarPortal.WebControls.UserProfile", UIComponentID);

            list.Add(descriptor);

            return list;
        }

        #region Private Member Variables

        private Table mContainer;
        private TableRow mContainerRow;
        private TableCell mContainerCell;

        private Table mProfile;
        private TableRow mProfileRow;
        private TableCell mProfileCell;

        private Table mInnerProfile;
        private TableRow mInnerProfileRow;
        private TableCell mInnerProfileCell;

        private TableCell mOrganizationHeader;
        private TableCell mRoleHeader;
        private TableCell mPrimaryHeader;

        private Camstar.WebPortal.FormsFramework.WebControls.TextBox mTextBoxUserID = new Camstar.WebPortal.FormsFramework.WebControls.TextBox();
        private Camstar.WebPortal.FormsFramework.WebControls.TextBox mTextBoxFullName = new Camstar.WebPortal.FormsFramework.WebControls.TextBox();
        private Camstar.WebPortal.FormsFramework.WebControls.TextBox mTextBoxTitle = new Camstar.WebPortal.FormsFramework.WebControls.TextBox();
        private Camstar.WebPortal.FormsFramework.WebControls.TextBox mTextBoxPrimaryLanguage = new Camstar.WebPortal.FormsFramework.WebControls.TextBox();
        private Camstar.WebPortal.FormsFramework.WebControls.TextBox mTextBoxSecondaryLanguage = new Camstar.WebPortal.FormsFramework.WebControls.TextBox();
        private Camstar.WebPortal.FormsFramework.WebControls.TextBox mTextBoxSessionLanguage = new Camstar.WebPortal.FormsFramework.WebControls.TextBox();
        private Camstar.WebPortal.FormsFramework.WebControls.TextBox mTextBoxOrganization = new Camstar.WebPortal.FormsFramework.WebControls.TextBox();
        private Camstar.WebPortal.FormsFramework.WebControls.SubmitButton mOKButton = new Camstar.WebPortal.FormsFramework.WebControls.SubmitButton();

        /// <summary>
        /// Tempory member variable.  Romove when user roles is implemented.
        /// </summary>
        private EmployeeDetails mEmployeeDetails;

        #endregion
    }

    /// <summary>
    /// Allows the capability to add child controls.
    /// </summary>
    public class Div : WebControl
    {
        public Div() : base(System.Web.UI.HtmlTextWriterTag.Div) { }
    }

    /// <summary>
    /// Temporary class.  Remove once user roles are implemented.
    /// </summary>
    public class EmployeeDetail
    {
        public EmployeeDetail(string primary, string organization, string role)
        {
            mPropagate = primary;
            mOrganization = organization;
            mRole = role;
        }

        public virtual string Propagate
        {
            get { return mPropagate; }
            set { mPropagate = value; }
        }

        public virtual string Organization
        {
            get { return mOrganization; }
            set { mOrganization = value; }
        }

        public virtual string Roles
        {
            get { return mRole; }
            set { mRole = value; }
        }

        private string mPropagate;
        private string mOrganization;
        private string mRole;
    }

    /// <summary>
    /// Temporary class.  Remove once user roles are implemented.
    /// </summary>
    public class EmployeeDetails : List<EmployeeDetail> { }
}
