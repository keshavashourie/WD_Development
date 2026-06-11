// Copyright Siemens 2024  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Xml;
using System.Xml.Linq;
using Camstar.WCF.Services;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.Utilities;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework.Utilities;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using PERS = Camstar.WebPortal.Personalization;
using CamstarPortal.WebControls;
using Camstar.WCF.ObjectStack;

namespace Camstar.WebPortal.WebPortlets
{
    public class InstanceList : MatrixWebPart
    {
        #region Controls
        protected virtual JQDataGrid ItemsSelectedGrid { get { return Page.FindCamstarControl("ItemsSelectedGrid") as JQDataGrid; } }
        protected virtual CWC.DropDownList ObjectsList { get { return Page.FindCamstarControl("ObjectsList") as CWC.DropDownList; } }
        protected virtual CWC.CheckBox SelectAllChk { get { return Page.FindCamstarControl("SelectAllChk") as CWC.CheckBox; } }
        protected virtual CWC.Button PopupFilterBtn { get { return Page.FindCamstarControl("PopupFilterBtn") as CWC.Button; } }
        protected virtual CWC.TextBox InstanceNameTxt { get { return Page.FindCamstarControl("InstanceNameTxt") as CWC.TextBox; } }
        protected virtual JQDataGrid InstanceListGrid { get { return Page.FindCamstarControl("InstanceListGrid") as JQDataGrid; } }
        protected virtual JQDataGrid ObjectTypeGrid { get; set; }

        protected virtual CWC.Button InstListGridIsFilteredBtn { get { return Page.FindCamstarControl("InstListGridIsFilteredBtn") as CWC.Button; } }
        #endregion

        #region Protected Functions

        protected override void OnPreLoad(object sender, EventArgs e)
        {
            base.OnPreLoad(sender, e);
            LoadObjectsList();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ObjectTypeGrid = Page.FindCamstarControl("ObjectTypeGrid") as JQDataGrid;

            ObjectsList.DataChanged += ObjectList_DataChanged;
            InstListGridIsFilteredBtn.Click += InstListGridIsFilteredBtn_Click;
            ItemsSelectedGrid.GridContext.RowDeleted += GridContext_RowDeleted;

            // Get session data when popup page is closed
            if (Page.EventArgument == "FloatingFrameSubmitParentPostBackArgument")
            {
                if (Page.SessionVariables.GetValueByName("InstanceFilters") != null)  
                    InstanceFilters = Page.SessionVariables.GetValueByName("InstanceFilters") as ExportInstanceFilters; 
                ReloadInstanceGrid();
            }

            InstListGridIsFilteredBtn.Visible = InstanceFilters != null && !InstanceFilters.IsEmpty();
            SetupInstanceGrid();
        }

        #endregion

        #region Public Functions

        public override void GetSelectionData(OM.Service serviceData)
        {
            base.GetSelectionData(serviceData);
            if (serviceData is OM.CDOInquiry)
            {
                var data = serviceData as OM.CDOInquiry;
                data.CDODefId = new OM.Enumeration<OM.MaintainableObjectEnum, string>(ObjectsList.Data as string);
                data.IsRDO = isSelectedObjectRDO();

                if (InstanceFilters != null)
                {
                    var filter = InstanceFilters;
                    data.EmployeeFilter = filter.EmployeeFilter;
                    data.StatusFilter = filter.StatusFilter;
                    data.RORFilter = filter.RORFilter;
                    data.PrefixExcludeFilter = filter.PrefixExcludeFilter;
                    data.PrefixIncludeFilter = filter.PrefixIncludeFilter;
                    data.SufixExcludeFilter = filter.SufixExcludeFilter;
                    data.SufixIncludeFilter = filter.SufixIncludeFilter;
                    data.BeginDateFilter = filter.BeginDateFilter;
                    data.EndDateFilter = filter.EndDateFilter;
                }
            }
        }

        #endregion

        #region Private Functions

        protected virtual ResponseData GridContext_RowDeleted(object sender, JQGridEventArgs e)
        {
            SelectedInstances = ((sender as ItemDataContext).ParentGridContext as ItemDataContext).Data as ObjectTypeItem[];
            ReloadInstanceGrid();

            var response = e.Response as DirectUpdateData;
            response.PropertyValue = response.PropertyValue.Replace("PostBackRequested:false", "PostBackRequested:true");
            return response;
       }

        protected virtual void InstListGridIsFilteredBtn_Click(object sender, EventArgs e)
        {
            ResetFilter();
        }

        public virtual void ResetFilter()
        {
            InstanceFilters = null;
            InstanceNameTxt.Data = null;
            InstListGridIsFilteredBtn.Visible = false;

            Page.SessionVariables.SetValueByName("InstanceFilters", null);           

            ReloadInstanceGrid();
        }

        protected virtual void ObjectList_DataChanged(object sender, EventArgs e)
        {
            InstanceNameTxt.ClearData();
            SelectAllChk.ClearData();
            ReloadInstanceGrid();
        }

        protected virtual void ReloadInstanceGrid()
        {
            SetupInstanceGrid();

            if (InstanceListGrid != null)
            {
                InstanceListGrid.ClearData();
                InstanceListGrid.Action_Reload("");
                RenderToClient = true;
            }
        }

        protected virtual void SetupInstanceGrid()
        {
            bool isRDO = isSelectedObjectRDO();
                (InstanceListGrid.BoundContext as SelValGridContext).RequestFieldExpression = "CDOInquiry.CDOFilteredInstances";
            InstanceListGrid.BoundContext.Attributes["listViewMode"] = ("list " + (isRDO ? "rdo" : "ndo") + " export");

            InstanceListGrid.BoundContext.SnapCompleted += InstanceListGrid_SnapCompleted;
            InstanceListGrid.BoundContext.Fields["Name"].Visible = !isRDO;
            InstanceListGrid.BoundContext.Fields["Displayed"].Visible = isRDO;
            InstanceListGrid.BoundContext.Settings.Grouping = !isRDO ? null : new PERS.GroupingView()
                {
                    GroupFields = new PERS.GroupField[] { new PERS.GroupField { DataField = "Name" } },
                };
        }

        protected virtual bool isSelectedObjectRDO()
        {
            var exportObjectList = Page.PortalContext.LocalSession["ObjectsList"] as Dictionary<string, CDOData>;
            var selectObj = exportObjectList.Values.FirstOrDefault(o => o.CDODefID == (ObjectsList.Data as string));

            if (selectObj.IsRDO != null)
                return (bool) selectObj.IsRDO;

            return false;
        }

        protected virtual void InstanceListGrid_SnapCompleted(DataTable dataWindowTable)
        {
            if (dataWindowTable.Columns.Contains("Revision"))
            {
                foreach (var r in dataWindowTable.Rows.OfType<DataRow>())
                {
                    r.BeginEdit();
                    var isROR = string.Compare(r["InstanceId"] as string, r["RevOfRcd"] as string, true) == 0;
                    r["Displayed"] = HttpContext.Current.Server.HtmlEncode(r["Name"] as string) +
                        CamstarPortalSection.GetRevisionDelimiter() + HttpContext.Current.Server.HtmlEncode(r["Revision"] as string) +
                            (isROR ? "<span class=\"ui-rdo-ror\" />" : "<span class=\"ui-rdo-non-ror\" />");
                }
                dataWindowTable.AcceptChanges();
            }

             //for all rows - exclude selected items
            var cdoName = (ObjectsList != null && !string.IsNullOrEmpty(ObjectsList.Data as string)) ?
                (Page.PortalContext.LocalSession["ObjectsList"] as Dictionary<string, CDOData>).Values.FirstOrDefault(cv => cv.CDODefID == (ObjectsList.Data as string)).CDOName :
                null;

            if (ObjectTypeGrid != null && ObjectTypeGrid.Data != null && ObjectTypeGrid.Data as ObjectTypeItem[] != null && cdoName != null)
            {
                var objSelected = (ObjectTypeGrid.Data as ObjectTypeItem[]).FirstOrDefault(x => x.Name == cdoName);
                if (objSelected != null && objSelected.Instances != null)
            {
                // Modify grid rows 
                foreach (var r in dataWindowTable.Rows.OfType<DataRow>())
                {
                    r.BeginEdit();
                        r["ItemAlreadySelected"] =
                            objSelected.Instances.Any(s => s.CDOTypeName == cdoName && s.InstanceID == r.Field<string>("InstanceId"));
                }
                dataWindowTable.AcceptChanges();
                }
            }
        }
        protected virtual void LoadObjectsList()
        {
            if (ObjectsList != null)
            {
                Dictionary<string, CDOData> exportObject;
                if (Page.PortalContext != null && Page.PortalContext.LocalSession["ObjectsList"] != null)
                {
                    exportObject = Page.PortalContext.LocalSession["ObjectsList"] as Dictionary<string, CDOData>;
                }
                else
                {
                    exportObject = ModelingObjectList.GetExportObjects(false);
                    Page.PortalContext.LocalSession["ObjectsList"] = exportObject;
                }
                ObjectsList.CustomListValues =
                    (from o in exportObject
                     orderby o.Value.CDODisplayName
                     select
                         new PERS.CustomListValueMapItem()
                             {
                                 DisplayName = o.Value.CDODisplayName,
                                 Value = o.Value.CDODefID
                             }).ToArray();
            }
        }
        #endregion

        #region Constants

        #endregion

        #region Private Member Variables
        public virtual ObjectTypeItem[] SelectedInstances
        {
            
            get
            {
                return Page.PortalContext.DataContract == null ? null : Page.PortalContext.DataContract.GetValueByName<ObjectTypeItem[]>("SelectedInstances");
            }
            set
            {
                Page.PortalContext.DataContract.SetValueByName("SelectedInstances", value);
            }           
         }
        protected virtual ExportInstanceFilters InstanceFilters
        {
            get { return Page.PortalContext.DataContract == null ? null : Page.PortalContext.DataContract.GetValueByName<ExportInstanceFilters>("InstanceFilters"); }
            set { Page.PortalContext.DataContract.SetValueByName("InstanceFilters", value); }
        }

        #endregion
    }

    public class ObjectTypeItem : ICloneable
    {
        public virtual string Name { get; set; }
        public virtual string DisplayName { get; set; }
        public virtual int CDOID { get; set; }
        public virtual SelectedInstanceItem[] Instances { get; set; }

        public static int GetInstancesCount(ObjectTypeItem[] objTypes, bool excludeRefs)
        {
            if (objTypes != null)
                return excludeRefs
                    ? objTypes.Sum(it => it.Instances != null ? it.Instances.Count(i => !i.IsRef) : 0)
                    : objTypes.Sum(it => it.Instances != null ? it.Instances.Count() : 0);
            else
                return 0;
        }

        #region ICloneable Members

        public virtual object Clone()
        {
            return new ObjectTypeItem()
            {
                Name = Name,
                DisplayName = DisplayName,
                CDOID = CDOID,
                Instances = Instances.Clone() as SelectedInstanceItem[]
            };
        }

        #endregion
    }


    public class SelectedInstanceItem : ICloneable
    {
        public virtual string Name { get; set; }
        public virtual string Revision { get; set; }
        public virtual string DisplayedName { get; set; }
        public virtual string Description { get; set; }
        public virtual string InstanceID { get; set; }
        public virtual bool IsROR { get; set; }
        public virtual bool IsRef { get; set; }
        public virtual string CDOTypeID { get; set; }
        public virtual string CDOTypeName { get; set; }
        public virtual string CDODisplayName { get; set; }
        public virtual string CDOTypeValue { get; set; }
        public virtual string LastEditedBy { get; set; }
        public virtual DateTime? LastEditedTime { get; set; }
        public virtual DateTime? AddedToPackageGMT { get; set; }
        public virtual int Order { get; set; }
        public virtual string ReferenceParent { get; set; }
        public virtual bool ShouldOpen { get; set; }
        public virtual bool IsSave { get; set; }
        public virtual string[] Dependencies { get; set; }

        public SelectedInstanceItem() { }

        public SelectedInstanceItem(OM.ObjectReferencesInfo rf)
        {
            //  CPR 290892:(2404) PR290832 TAC10377394 - Error when trigger "Hierarchy"
            //  Verify we have an ObjectName - otherwise ignore this reference
            if (rf != null && rf.ObjectName != null)
            {
                Name = rf.ObjectName.Value;
                InstanceID = rf.ObjectInstanceId.Value;
                CDOTypeID = rf.ObjectType.Value.ToString();
                CDOTypeName = rf.ObjectTypeName.Value;
                CDOTypeValue = rf.ObjectTypeValue.Value;
                Revision = rf.Revision != null ? rf.Revision.Value : null;
                IsRef = true;
                IsROR = (bool)rf.IsROR;

                DisplayedName = Name;
                if (!string.IsNullOrEmpty(Revision))
                {
                    DisplayedName += $"{CamstarPortalSection.GetRevisionDelimiter()}{Revision}";
                }
                Description = rf.Description != null ? rf.Description.Value : string.Empty;

                LastEditedBy = rf.LastEditBy != null ? rf.LastEditBy.Value : "";
                LastEditedTime = rf.LastEditDateGMT != null ? rf.LastEditDateGMT.Value : DateTime.Now;
                ShouldOpen = true;
            }
            else
                ShouldOpen = false;
        }

        public SelectedInstanceItem(DataRow r)
        {
            Name = r["Name"] as string;
            InstanceID = r["InstanceID"] as string;
            DisplayedName = r["Name"] as string;
            if (!string.IsNullOrEmpty(r["Revision"] as string))
            {
                Revision = r["Revision"] as string;
                DisplayedName += $"{CamstarPortalSection.GetRevisionDelimiter()}{Revision}";
            }        
            if (r.Table.Columns.Contains("Description"))
            {
                Description = r["Description"] as string;
            }

            if (r.Table.Columns.Contains("LastEditedBy"))
            {
                LastEditedBy = r["LastEditedBy"] as string;
                if(r["LastEditTimeGMT"] != DBNull.Value)
                    LastEditedTime = (DateTime)r["LastEditTimeGMT"];
            }
            else
            {
                LastEditedBy = "Camstar-Admin";
                LastEditedTime = DateTime.Now;
            }
        }

        public override string ToString()
        {
            string s = "";
            if (!string.IsNullOrEmpty(Name))
                s = Name;
            else if (!string.IsNullOrEmpty(InstanceID))
                s = InstanceID;

            if (!string.IsNullOrEmpty(CDOTypeName))
                s += " {" + CDOTypeName + "}";

            return s;
        }

        #region ICloneable Members

        public virtual object Clone()
        {
            return new SelectedInstanceItem()
            {
                Name = Name,
                Revision = Revision,
                DisplayedName = DisplayedName,
                Description = Description,
                InstanceID = InstanceID,
                IsROR = IsROR,
                IsRef = IsRef,
                CDOTypeID = CDOTypeID,
                CDOTypeName = CDOTypeName,
                LastEditedBy = LastEditedBy,
                LastEditedTime = LastEditedTime,
                AddedToPackageGMT = AddedToPackageGMT,
                Order = Order,
                ShouldOpen = ShouldOpen,
                IsSave = IsSave
            };
        }

        #endregion
    }

    public class InstanceComparer<T> : IEqualityComparer<T>
    {
        private readonly Func<T, T, bool> getEquals;
        private readonly Func<T, int> getHashCode;

        public InstanceComparer(Func<T, T, bool> equals, Func<T, int> hashCode)
        {
            getEquals = equals;
            getHashCode = hashCode;
        }

        public virtual bool Equals(T x, T y)
        {
            return getEquals(x, y);
        }

        public virtual int GetHashCode(T obj)
        {
            return getHashCode(obj);
        }
    }
}
