// Copyright Siemens 2023  
using System;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WCF.ObjectStack;

namespace Camstar.WebPortal.WebPortlets
{
    /// <summary>
    /// Call CDOInquiry to filter result for CDO selection
    /// </summary>
    public class InstanceFilter : MatrixWebPart
    {
        #region Controls

        protected virtual CWC.RadioButton IsNotFilterByTime { get { return Page.FindCamstarControl("IsNotFilterByTime") as CWC.RadioButton; } }
        protected virtual CWC.RadioButton IsFilterByTime { get { return Page.FindCamstarControl("IsFilterByTime") as CWC.RadioButton; } }
        protected virtual CWC.DateChooser BeginDateFilter { get { return Page.FindCamstarControl("CDOInquiry_BeginDateFilter") as CWC.DateChooser; } }
        protected virtual CWC.DateChooser EndDateFilter { get { return Page.FindCamstarControl("CDOInquiry_EndDateFilter") as CWC.DateChooser; } }
        protected virtual CWC.NamedObject EmployeeFilter { get { return Page.FindCamstarControl("CDOInquiry_EmployeeFilter") as CWC.NamedObject; } }
        protected virtual CWC.DropDownList StatusFilter { get { return Page.FindCamstarControl("CDOInquiry_StatusFilter") as CWC.DropDownList; } }
        protected virtual CWC.CheckBox RORFilter { get { return Page.FindCamstarControl("CDOInquiry_RORFilter") as CWC.CheckBox; } }
        protected virtual CWC.CheckBox OnlyActiveInstancesFilter { get { return Page.FindCamstarControl("CDOInquiry_IsActiveOnly") as CWC.CheckBox; } }
        protected virtual CWC.TextBox PrefixIncludeFilter { get { return Page.FindCamstarControl("CDOInquiry_PrefixIncludeFilter") as CWC.TextBox; } }
        protected virtual CWC.TextBox PrefixExcludeFilter { get { return Page.FindCamstarControl("CDOInquiry_PrefixExcludeFilter") as CWC.TextBox; } }
        protected virtual CWC.TextBox SufixIncludeFilter { get { return Page.FindCamstarControl("CDOInquiry_SufixIncludeFilter") as CWC.TextBox; } }
        protected virtual CWC.TextBox SufixExcludeFilter { get { return Page.FindCamstarControl("CDOInquiry_SufixExcludeFilter") as CWC.TextBox; } }
        protected virtual CWC.Button ClearAllBtn { get { return Page.FindCamstarControl("ClearAllBtn") as CWC.Button; } }
        protected virtual CWC.Button ApplyBtn { get { return Page.FindCamstarControl("ApplyBtn") as CWC.Button; } }

        #endregion
        
        #region Public Functions
        #endregion
        #region Protected Functions

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            ApplyBtn.Click += ApplyBtn_Click;
            ClearAllBtn.Click += ClearAllBtn_Click;
            Page.LoadComplete += Page_LoadComplete;
        }





        protected virtual void ApplyBtn_Click(object sender, EventArgs e)
        {
            SaveFilterToSession();
        }

        protected virtual void SaveFilterToSession()
        {
            var filters = new ExportInstanceFilters()
            {
                isFilteredByTime = false,
                EmployeeFilter = EmployeeFilter.Data as NamedObjectRef,
                RORFilter = RORFilter.Data as bool?,
                PrefixIncludeFilter = PrefixIncludeFilter.Data as string,
                PrefixExcludeFilter = PrefixExcludeFilter.Data as string,
                SufixIncludeFilter = SufixIncludeFilter.Data as string,
                SufixExcludeFilter = SufixExcludeFilter.Data as string
            };

            if (StatusFilter != null)
                filters.StatusFilter = new Enumeration<StatusEnum, int>(Convert.ToInt32(StatusFilter.Data));
            else if (OnlyActiveInstancesFilter != null && (OnlyActiveInstancesFilter.Data as bool?) == true)
                filters.StatusFilter = StatusEnum.Active;

            if (CheckTimeFilterStatus())
            {
                filters.isFilteredByTime = true;
                filters.BeginDateFilter = BeginDateFilter.Data as DateTime?;
                filters.EndDateFilter = EndDateFilter.Data as DateTime?;
            }

            Page.SessionVariables.SetValueByName("InstanceFilters", filters);
        }

        // Check if time filtering is selected
        protected virtual bool CheckTimeFilterStatus()
        {
            var positiveStatus = (bool)IsFilterByTime.Data;
            var negativeStatus = (bool)IsNotFilterByTime.Data;

            return positiveStatus && !negativeStatus;
        }

        protected virtual void ClearAllBtn_Click(object sender, EventArgs e)
        {
            Page.ClearValues();
            IsNotFilterByTime.Data = true;
        }

        protected virtual void Page_LoadComplete(object sender, EventArgs e)
        {
            // Load saved filters only if the page is reopened
            var postbackSource = Page.Request.Params["__EVENTTARGET"];
            if (postbackSource == null && Page.SessionVariables.GetValueByName("InstanceFilters") != null)
            {
                var existingFilters = Page.SessionVariables.GetValueByName("InstanceFilters") as ExportInstanceFilters;
                if (existingFilters.isFilteredByTime)
                {
                    IsFilterByTime.Data = true;
                    IsNotFilterByTime.Data = false;
                    if (existingFilters.BeginDateFilter != null)
                        BeginDateFilter.Data = existingFilters.BeginDateFilter.Value;
                    if (existingFilters.EndDateFilter != null)
                        EndDateFilter.Data = existingFilters.EndDateFilter.Value;
                }
                EmployeeFilter.Data = existingFilters.EmployeeFilter;
                if (StatusFilter != null && existingFilters.StatusFilter != null)
                    StatusFilter.Data = existingFilters.StatusFilter.Value;
                else if (OnlyActiveInstancesFilter != null && existingFilters.OnlyActiveInstances != null && existingFilters.OnlyActiveInstances.Value == true)
                    StatusFilter.Data = StatusEnum.Active;
                RORFilter.Data = existingFilters.RORFilter.Value;
                PrefixExcludeFilter.Data = existingFilters.PrefixExcludeFilter;
                PrefixIncludeFilter.Data = existingFilters.PrefixIncludeFilter;
                SufixExcludeFilter.Data = existingFilters.SufixExcludeFilter;
                SufixIncludeFilter.Data = existingFilters.SufixIncludeFilter;
            }
        }

        #endregion
    }

    public class ExportInstanceFilters
    {
        public virtual bool isFilteredByTime { get; set; }
        public virtual Primitive<DateTime> BeginDateFilter { get; set; }
        public virtual Primitive<DateTime> EndDateFilter { get; set; }
        public virtual NamedObjectRef EmployeeFilter { get; set; }
        public virtual Primitive<bool> RORFilter { get; set; }
        public virtual Primitive<bool> OnlyActiveInstances { get; set; }
        public virtual Enumeration<StatusEnum, int> StatusFilter { get; set; }
        public virtual Primitive<string> PrefixExcludeFilter { get; set; }
        public virtual Primitive<string> PrefixIncludeFilter { get; set; }
        public virtual Primitive<string> SufixExcludeFilter { get; set; }
        public virtual Primitive<string> SufixIncludeFilter { get; set; }

        public virtual bool IsEmpty()
        {
            return (EmployeeFilter == null &&
                        StatusFilter == null &&
                        RORFilter == false &&
                        OnlyActiveInstances == false &&
                        PrefixExcludeFilter == null &&
                        PrefixIncludeFilter == null &&
                        SufixExcludeFilter == null &&
                        SufixIncludeFilter == null &&
                        BeginDateFilter == null &&
                        EndDateFilter == null);
        }
    }
}
