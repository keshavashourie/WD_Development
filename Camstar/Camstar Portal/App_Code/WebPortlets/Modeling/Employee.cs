// Copyright Siemens 2023  
using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.PortalFramework;

using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework.Utilities;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.WCFUtilities;
using System.Text;
using Camstar.WebPortal.FormsFramework.WebControls.PickLists;

namespace Camstar.WebPortal.WebPortlets
{

    /// <summary>
    /// Used on the Employee Maint VP to encrypt the password
    /// </summary>
    public class Employee : MatrixWebPart
    {
        protected virtual JQDataGrid EmpFilterTagGrid
        {
            get
            {
                return Page.FindCamstarControl("EmpFilterTagGrid") as JQDataGrid;
            }
        }
        protected virtual TextBox PasswordText
        {
            get { return Page.FindCamstarControl("DocManagerPassword") as TextBox; }
        }

        protected virtual MultiSelectPickList ObjectChanges_FilterTags
        {
            get { return Page.FindCamstarControl("ObjectChanges_FilterTags") as MultiSelectPickList; }
        }
        protected virtual RadioButtonList FilterTagAccessRBList
        {
            get { return Page.FindCamstarControl("FilterTagAccessRBList") as RadioButtonList; }
        }


        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            // Get all filter tags in the system
            ObjectChanges_FilterTags.Visible = false;

            GetAllFilterTags();

            if (EmpFilterTagGrid != null)
            {           
                var cntrl = EmpFilterTagGrid.GetInlineControl("InstanceID") as CWC.NamedObject;
                cntrl.PickListPanelControl.DataProvider.AfterDataLoad += AvailableFilters_AfterDataLoad;                
            }

        }

        protected virtual void AvailableFilters_AfterDataLoad(object sender, DataLoadEventArgs e)
        {
            var selVals = sender as SelectionValuesDataProvider;
            if (selVals != null)
            {
                var assignedFTs = EmpFilterTagGrid.Data as FilterTag[];
                var comparer = new FilterTagsComparer();
                var newAssignable = AllFilterTags.Except(assignedFTs, comparer);
                if (newAssignable != null)
                {
                    var rs = new RecordSet();
                    rs.Headers = new Header[]
                    {                        
                        new Header
                        {
                             Name = "ID"
                        },
                        new Header
                        {
                            Name = "Name"
                        },
                        new Header
                        {
                            Name = "Description"
                        }
                    };
                   
                    var rows = new List<Row>();
                    foreach (var assignable in newAssignable)
                    {
                        var Descr = assignable.Description != null  ? assignable.Description.Value : string.Empty;
                        var row = new Row
                        {
                            Values = new string[]
                            {
                                assignable.InstanceID.ID, assignable.InstanceID.Name,Descr
                            }
                        };
                        rows.Add(row);
                    }
                    rs.Rows = rows.ToArray();
                    selVals.LastQueryResult = rs;
                }
            }                        
        }
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (EmpFilterTagGrid != null)
            {
                EmpFilterTagGrid.PreRender += EmpFilterTagGrid_PreRender;                               
            }

        }             
        protected virtual void EmpFilterTagGrid_PreRender(object sender, EventArgs e)
        {
            // Call FilterTagInquiry to get assigned filter tags for instance          
            GetEmpFilterTags();
            var ctx = EmpFilterTagGrid.GridContext as ItemDataContext;
            if (ctx != null)
            {
                ctx.SnapCompleted += FilterTags_SnapCompleted; ;
            }
        }

        protected virtual void FilterTags_SnapCompleted(DataTable dataWindowTable)
        {
            if (dataWindowTable != null)
            {
                for (var i = 0; i < dataWindowTable.Rows.Count; i++)
                {
                    var rowid = EmpFilterTagGrid.GridContext.GetRowId(i);
                    if (!string.IsNullOrEmpty(rowid))
                    {
                        var rowData = EmpFilterTagGrid.GridContext.GetItem(rowid) as FilterTag;
                        if (!rowData.IsNullOrEmpty() && SessionFilterTags != null && SessionFilterTags.Count() > 0)
                        {
                            var found = SessionFilterTags.FirstOrDefault(x => x.InstanceID.ID == rowData.InstanceID.ID);
                            if (found != null)
                            {
                                EmpFilterTagGrid.GridContext.SelectRow(rowid, true);
                                
                            }
                        }
                    }
                }
            }
        }

        public override void GetInputData(Service serviceData)
        {
            base.GetInputData(serviceData);
            EmployeeMaint changes = (EmployeeMaint)serviceData;        
            if (changes.ObjectChanges != null)
            {
                if (changes.ObjectChanges.DocManagerPassword != null)
                {
                    if (PasswordText.Data != null)
                    {
                        changes.ObjectChanges.DocManagerPassword = Camstar.Util.CryptUtil.Encrypt(PasswordText.Data.ToString());
                    }                    
                }
                if (EmpFilterTagGrid != null)
                {
                    var type = (FilterTagAccessEnum)Enum.Parse(typeof(FilterTagAccessEnum), FilterTagAccessRBList.Data as string);
                    if (type == FilterTagAccessEnum.All)
                    {
                        changes.ObjectChanges.FilterTags = string.Empty;
                        changes.ObjectChanges.FilterTagsSession = string.Empty;
                    }
                    else
                    {

                        var assignedTags = EmpFilterTagGrid.Data as FilterTag[];
                        if (assignedTags != null)
                        {
                            var assignedIds = new List<string>();
                            foreach (var assignedInst in assignedTags)
                            {
                                if (assignedInst.InstanceID != null)
                                {
                                    var found = AllFilterTags.FirstOrDefault(x => x.InstanceID.Name == assignedInst.InstanceID.Name);
                                    if (found != null)
                                    {
                                        if (!assignedIds.Any(x => x == found.InstanceID.ID))
                                        {
                                            assignedIds.Add(found.InstanceID.ID);
                                        }
                                    }
                                }
                            }
                            changes.ObjectChanges.FilterTags = string.Join(",", assignedIds);
                            var selectedIds = EmpFilterTagGrid.SelectedRowIDs;
                            if (selectedIds != null)
                            {
                                var sessionIds = new List<string>();
                                foreach (var id in selectedIds)
                                {
                                    var row = EmpFilterTagGrid.GridContext.GetItem(id.ToString());
                                    if (row != null)
                                    {
                                        var ft = row as FilterTag;
                                        if (ft != null && ft.InstanceID != null)
                                        {
                                            var found = AllFilterTags.FirstOrDefault(x => x.InstanceID.Name == ft.InstanceID.Name);
                                            if (found != null && !sessionIds.Any(x => x == found.InstanceID.ID))
                                            {
                                                sessionIds.Add(found.InstanceID.ID);
                                            }
                                        }
                                    }
                                }
                                changes.ObjectChanges.FilterTagsSession = string.Join(",", sessionIds);
                            }
                            else
                            {
                                changes.ObjectChanges.FilterTagsSession = null;
                            }
                        }
                    }
                }
                else
                {
                    changes.ObjectChanges.FilterTags = null;
                    changes.ObjectChanges.FilterTagsSession = null;
                }
            }
        }
        protected virtual void GetAllFilterTags()
        {
            var service = new FilterTagInquiryService(FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);
            var cdo = new FilterTagInquiry();            

            var req = new FilterTagInquiry_Request
            {
                Info = new FilterTagInquiry_Info
                {
                    FilterTag = new FilterTag_Info
                    {
                        InstanceID = new Info(true),
                        Name = new Info(true),
                        Description = new Info(true)
                    }
                }

            };

            FilterTagInquiry_Result res;
            var resStatus = service.GetFilterTags(cdo, req, out res);
            if (!resStatus.IsSuccess)
            {
                Page.DisplayMessage(resStatus);
                return;
            }
            if (res != null && res.Value != null && res.Value.FilterTag != null)
            {
                AllFilterTags = res.Value.FilterTag.ToList();
            }
        }
        protected virtual void GetEmpFilterTags()
        {
            var instName = Page.DataContract.GetValueByName("SelectedInstanceRef") != null ? Page.DataContract.GetValueByName("SelectedInstanceRef").ToString() : string.Empty;
            if (!string.IsNullOrEmpty(instName))
            {

                var service = new FilterTagInquiryService(FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);
                var cdo = new FilterTagInquiry
                {
                    CurrentEmployee = new NamedObjectRef(instName)
                };
                var req = new FilterTagInquiry_Request
                {
                    Info = new FilterTagInquiry_Info
                    {
                        FilterTag = new FilterTag_Info
                        {
                            InstanceID = new Info(true),
                            Name = new Info(true),
                            Description = new Info(true)
                        },
                        EmployeeSessionFilterTag = new FilterTag_Info
                        {
                            InstanceID = new Info(true)
                        }
                    }
                };
                FilterTagInquiry_Result res;
                var resStatus = service.GetEmpFilterTags(cdo, req, out res);
                if (!resStatus.IsSuccess)
                {
                    Page.DisplayMessage(resStatus);
                    return;
                }
                if (res != null && res.Value != null && res.Value.FilterTag != null)
                {                    
                    EmpFilterTagGrid.Data = res.Value.FilterTag;                
                    SessionFilterTags = res.Value.EmployeeSessionFilterTag;
                }
            }
        }

        #region Protected Members

        protected virtual List<FilterTag> AllFilterTags
        {
            get
            {
                if (ViewState["AllFilterTags"] == null)
                {
                    return new List<FilterTag>();
                }
                else
                {
                    return ViewState["AllFilterTags"] as List<FilterTag>;
                }
            }
            set
            {
                ViewState["AllFilterTags"] = value;
            }
        }    
        protected virtual FilterTag[] SessionFilterTags
        {
            get
            {
                return ViewState["SessionFilterTags"] as FilterTag[];
            }
            set
            {
                ViewState["SessionFilterTags"] = value;
            }
        }

        #endregion
    }

    public class FilterTagsComparer : IEqualityComparer<FilterTag>
    {
        public bool Equals(FilterTag x, FilterTag y)
        {
            if (x.InstanceID != null && y.InstanceID != null)
            {
                return x.InstanceID.Name == y.InstanceID.Name;
            }
            else
            {
                return false;
            }
        }
        public int GetHashCode(FilterTag ft)
        {
            if (ft.InstanceID != null)
            {
                return ft.InstanceID.Name.GetHashCode();
            }
            else
            {
                return -1;
            }
        }
    }


}
