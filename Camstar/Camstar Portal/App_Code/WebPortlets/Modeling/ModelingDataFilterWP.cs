// Copyright Siemens 2023  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using Camstar.WCF.Services;
using Camstar.WebPortal.Personalization;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework.Utilities;
using CWGC = Camstar.WebPortal.FormsFramework.WebGridControls;
using OM = Camstar.WCF.ObjectStack;

namespace Camstar.WebPortal.WebPortlets.Modeling
{

    /// <summary>
    /// TODO: Add a Summary description for this Camstar Web Part
    /// </summary_
    public class ModelingDataFilterWP : MatrixWebPart
    {
        #region Controls

        protected virtual CWGC.JQDataGrid ModelingDataFilterSessionGrid
        {
            get
            {
                return (CWGC.JQDataGrid)Page.FindCamstarControl("ModelingDataFilterSessionGrid");
            }
        }
        #endregion

        #region Protected Functions       

        /// <summary>
        /// TODO: Summary Description of function
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!Page.IsPostBack)
            {
                PopulateModelingDataFilterGrid();
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            GetSessionFilterTags();
        }

        protected virtual void GetEmpFilterTags()
        {
            var instName = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile.Name != null ? FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile.Name : string.Empty;
            if (!string.IsNullOrEmpty(instName))
            {

                var service = new FilterTagInquiryService(FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);
                var cdo = new OM.FilterTagInquiry
                {
                    CurrentEmployee = new OM.NamedObjectRef(instName)
                };
                var req = new FilterTagInquiry_Request
                {
                    Info = new OM.FilterTagInquiry_Info
                    {
                        FilterTag = new OM.FilterTag_Info
                        {
                            InstanceID = new OM.Info(true),
                            Name = new OM.Info(true)
                        },
                        EmployeeSessionFilterTag = new OM.FilterTag_Info
                        {
                            InstanceID = new OM.Info(true),
                            Name = new OM.Info(true),
                            Description = new OM.Info(true)
                            
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
                    ModelingDataFilterSessionGrid.Data = res.Value.FilterTag;
                    if (res.Value.EmployeeSessionFilterTag != null)
                    {
                        EmployeeSessionTags = res.Value.EmployeeSessionFilterTag;
                    }
                }
            }
        }

        #endregion

        #region Public Functions
        public override void WebPartCustomAction(object sender, CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as CustomAction;
            var grid = ModelingDataFilterSessionGrid.GridContext;
            var rowsSelected = grid.SelectedRowIDs;            
            if (action.CommandName == "Save")
            {              
                if (rowsSelected != null && FilterTags != null)
                {
                    var ids = string.Join(",", rowsSelected);
                    var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
                    var cdoToChanges = new SetSessionFilterTagMaint
                    {
                        ObjectToChange = new NamedObjectRef(session.CurrentUserProfile.Name)
                    };

                    var service = new SetSessionFilterTagMaintService(session.CurrentUserProfile);
                    var cdoChanges = new SetSessionFilterTagMaint
                    {
                        ObjectChanges = new SetSessionFilterTagChanges
                        {
                            FilterTagsSession = ids
                        }

                    };

                    var req = new SetSessionFilterTagMaint_Request();
                    service.BeginTransaction();
                    service.Load(cdoToChanges);
                    service.ExecuteTransaction(cdoChanges);
                    var resultStatus = service.CommitTransaction();

                    if (resultStatus.IsSuccess)
                    {
                        session.CurrentUserProfile.FilterTags = ids;
                        Page.CloseFloatingFrame(sender, e);
                    }
                    else
                    {
                        e.Result = resultStatus;
                    }

                }
            }
        }
        #endregion

        #region Private Functions        
        protected virtual void PopulateModelingDataFilterGrid()
        {
            var rows = new List<OM.Row>();
            OM.Header[] headers = { new OM.Header { Name = _lbl_InstanceId }, new OM.Header { Name = _lbl_Name }, new OM.Header { Name = _lbl_Description } };

            if (ModelingDataFilterSessionGrid != null)
            {
                var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
                var service = new FilterTagInquiryService(session.CurrentUserProfile);
                var serviceData = new OM.FilterTagInquiry
                {
                    CurrentEmployee = new OM.NamedObjectRef(service.UserProfile.Name)
                };
                var request = new FilterTagInquiry_Request
                {
                    Info = new OM.FilterTagInquiry_Info()
                    {
                        FilterTag = new OM.FilterTag_Info
                        {
                            InstanceID = new OM.Info(true),
                            Name = new OM.Info(true),
                            Description = new OM.Info(true)
                        },
                        EmployeeSessionFilterTag = new OM.FilterTag_Info
                        {
                            InstanceID = new OM.Info(true),
                            Name = new OM.Info(true),
                            Description = new OM.Info(true)
                        }
                    }
                };

                FilterTagInquiry_Result result;

                var resultStatus = service.GetEmpFilterTags(serviceData, request, out result);
                if (resultStatus.IsSuccess)
                {
                    if (result.Value != null && result.Value.FilterTag != null)
                    {
                        foreach (var tag in result.Value.FilterTag)
                        {
                            string descr = tag.Description != null ? tag.Description.Value : string.Empty;
                            string[] values = { tag.InstanceID.ID, tag.InstanceID.Name, descr };
                            var row = new OM.Row
                            {
                                Values = values
                            };
                            rows.Add(row);
                        }
                        FilterTags = result.Value.FilterTag;
                    }
                    if (result != null && result.Value != null && result.Value.EmployeeSessionFilterTag != null)
                    {
                        if (result.Value.EmployeeSessionFilterTag != null)
                        {
                            EmployeeSessionTags = result.Value.EmployeeSessionFilterTag;
                        }
                    }
                }
            }
            var rs = new OM.RecordSet
            {
                Headers = headers,
                Rows = rows.ToArray()
            };
            ModelingDataFilterSessionGrid.SetSelectionValues(rs);
        }

        protected virtual void GetSessionFilterTags()
        {
            if (EmployeeSessionTags != null)
            {
                if (ModelingDataFilterSessionGrid != null && ModelingDataFilterSessionGrid.Data != null)
                {                    
                    foreach (var tag in EmployeeSessionTags)
                    {                        
                        if (FilterTags.Any(x => x.InstanceID.ID == tag.InstanceID.ID))
                        {
                            ModelingDataFilterSessionGrid.GridContext.SelectRow(tag.InstanceID.ID, true);
                        }                        
                    }
                }
            }
        }

        #endregion

        #region Constants

        #endregion

        #region Private Member Variables
        protected virtual OM.FilterTag[] FilterTags
        {
            get
            {               
                return ViewState["FilterTags"] as OM.FilterTag[];                
            }
            set
            {                
                ViewState["FilterTags"] = value;                
            }
        }
        protected virtual OM.FilterTag[] EmployeeSessionTags
        {
            get
            {                
                return ViewState["EmployeeSessionTags"] as OM.FilterTag[];                
            }
            set
            {
                ViewState["EmployeeSessionTags"] = value;
            }
        }
        private string _lbl_InstanceId = "InstanceId";
        private string _lbl_Name = FrameworkManagerUtil.GetLabelValue("Lbl_Name") ?? string.Empty;
        private string _mustSelectAtLeastOneFilterTagLbl = FrameworkManagerUtil.GetLabelValue("Lbl_MustSelectAtLeastOneFilterTag") ?? string.Empty;
        private string _lbl_Description = FrameworkManagerUtil.GetLabelValue("Lbl_Description") ?? string.Empty;
        #endregion

    }

}

