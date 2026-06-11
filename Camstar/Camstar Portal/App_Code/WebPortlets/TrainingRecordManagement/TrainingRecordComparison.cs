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
using Camstar.Constants;
using DocumentFormat.OpenXml.Drawing.Charts;
using DataTable = System.Data.DataTable;

namespace Camstar.WebPortal.WebPortlets
{

    /// <summary>
    /// Training Record comparison
    /// </summary>
    public class TrainingRecordComparison : MatrixWebPart
    {
        #region Controls
        protected virtual CWC.NamedObject FromEmployee
        {
            get { return Page.FindCamstarControl("FromEmployee") as CWC.NamedObject; }
        }
        protected virtual JQDataGrid FromEmployeeGrid
        {
            get { return Page.FindCamstarControl("FromEmployeeGrid") as JQDataGrid; }
        }
        protected virtual CWC.NamedObject ToEmployee
        {
            get { return Page.FindCamstarControl("ToEmployee") as CWC.NamedObject; }
        }
        protected virtual JQDataGrid ToEmployeeGrid
        {
            get { return Page.FindCamstarControl("ToEmployeeGrid") as JQDataGrid; }
        }
        protected virtual CWC.Button CopyButton
        {
            get { return Page.FindCamstarControl("CopyButton") as CWC.Button; }
        }
        #endregion

        #region Protected Functions

        /// <summary>
        /// Handle offset adjustment for dates in the grid
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            DataTable data = (DataTable)FromEmployeeGrid.Data;
            DataTable data1 = (DataTable) ToEmployeeGrid.Data;
            if (data != null && FromEmployee.IsChanged==true && Page.IsNotPostBack )
            {
                AdjustDateColumn(data);
            }

            if (data1 != null && ToEmployee.IsChanged==true && Page.IsNotPostBack)
            {
               
                AdjustDateColumn(data1);
            }

        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

           if (ToEmployeeGrid.Data == null)
                Page.ActionDispatcher.PageActions().First().IsDisabled = true;

            FromEmployeeGrid.RowSelected += FromEmployeeGrid_RowSelected;
        }

        protected virtual ResponseData FromEmployeeGrid_RowSelected(object sender, JQGridEventArgs args)
        {
            CopyButton.Enabled = !FromEmployeeGrid.IsEmpty && !ToEmployee.IsEmpty;
            return args.Response;
        }
        #endregion

        #region Public Functions
        public virtual void FromEmployee_DataChanged(object sender, EventArgs e)
        {
            CopyButton.Enabled = false;
            FromEmployeeGrid.ClearData();

            if (!FromEmployee.IsEmpty)
            {
                mActiveEmployee = FromEmployee.Data as NamedObjectRef;
                mActiveGrid = FromEmployeeGrid;
                Page.Service.LoadSelectionValues();
                CopyButton.Enabled = !FromEmployeeGrid.IsEmpty && !ToEmployee.IsEmpty;
            }
        }
        public virtual void ToEmployee_DataChanged(object sender, EventArgs e)
        {
            CopyButton.Enabled = false;
            ToEmployeeGrid.ClearData();

            if (!ToEmployee.IsEmpty)
            {
                mActiveEmployee = ToEmployee.Data as NamedObjectRef;
                mActiveGrid = ToEmployeeGrid;
                Page.Service.LoadSelectionValues();
                CopyButton.Enabled = !FromEmployeeGrid.IsEmpty && !FromEmployee.IsEmpty;
            }
        }
        public virtual void CopyButton_Click(object sender, EventArgs e)
        {
            if (!IsRecordExists())
            {
                CopyRecord();
                this.Page.ActionDispatcher.PageActions()[0].IsDisabled = false;
            }
            else
            {
                LabelCache cache = FrameworkManagerUtil.GetFrameworkSession().GetLabelCache();
                Page.DisplayMessage(cache.GetLabelByName("TrainingComparisonDuplicateCopyMessage").Value, false);
            }
        }

        public virtual bool IsRecordExists()
        {
            bool found = false;
            var item = FromEmployeeGrid.GridContext.GetItem(FromEmployeeGrid.SelectedRowID) as DataRow;

            DataTable dt = (ToEmployeeGrid.GridContext as BoundContext).DataWindow;
            foreach (DataRow dr in dt.Rows)
            {
                if (dr["TrainingRequirement"].Equals(item["TrainingRequirement"]) && dr["Revision"].Equals(item["Revision"]))
                {
                    found = true;
                    break;
                }
            }

            return found;
        }

        public virtual void CopyRecord()
        {
            var item = FromEmployeeGrid.GridContext.GetItem(FromEmployeeGrid.SelectedRowID) as DataRow;

            DataTable dt = (ToEmployeeGrid.GridContext as BoundContext).Data as DataTable;
            if (dt == null)
            {
                DataTable dataTable = (ToEmployeeGrid.GridContext as BoundContext).DataWindow;
                dt = dataTable.Clone();
            }
            DataRow dr = dt.NewRow();

            foreach (DataColumn dc in item.Table.Columns)
                dr.SetField(dc.ColumnName, item[dc.ColumnName]);

            dr.SetField("Employee", (ToEmployee.Data as NamedObjectRef).Name);
            dr.SetField("IsNewRow", true);
            dt.Rows.Add(dr);
            ToEmployeeGrid.Data = dt;
        }

        public override void GetSelectionData(Service serviceData)
        {
            base.GetSelectionData(serviceData);
            var trainingMaintData = serviceData as TrainingRecordMaint;
            if (trainingMaintData != null)
            {
                trainingMaintData.TrainingRequirement = null;
                trainingMaintData.ParentDataObject = mActiveEmployee;
                trainingMaintData.StatusFilter = null;    
            }
        }

        public override void RequestSelectionValues(Info serviceInfo, Service serviceData)
        {
            base.RequestSelectionValues(serviceInfo, serviceData);

            (serviceInfo as TrainingRecordMaint_Info).ObjectToChange = FieldInfoUtil.RequestSelectionValue();
        }

        public override void DisplaySelectionValues(WCF.ObjectStack.Environment environment)
        {
            base.DisplaySelectionValues(environment);

            if (mActiveGrid != null)
                mActiveGrid.SetSelectionValues((environment as TrainingRecordMaint_Environment).ObjectToChange.SelectionValues);
        }

        public override void WebPartCustomAction(object sender, CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);

            var action = e.Action as CustomAction;
            if (action != null)
            {
                switch (action.Parameters)
                {
                    case "Save":
                        {
                            DataTable dt = (ToEmployeeGrid.GridContext as BoundContext).Data as DataTable;
                            foreach (DataRow dr in dt.Rows)
                            {
                                if (!dr.IsNull("IsNewRow") && dr["IsNewRow"].ToString() == "True")
                                {
                                    UserProfile profile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
                                    TrainingRecordMaintService service = new TrainingRecordMaintService(profile);
                                    service.BeginTransaction();

                                    TrainingRecordMaint data = new TrainingRecordMaint();
                                    data.ParentDataObject = WSObjectRef.AssignNamedObject(dr["Employee"] as string);
                                    data.TrainingRequirement = WSObjectRef.AssignRevisionedObject(dr["TrainingRequirement"] as string, dr["Revision"] as string);
                                    service.New(data);

                                    TrainingRecordMaint data1 = new TrainingRecordMaint();
                                    data1.ObjectChanges = new TrainingRecordChanges();
                                    data1.ObjectChanges.Status = WSObjectRef.AssignNamedObject(dr["Status"] as string);
                                    if (!dr.IsNull("ExpirationDate"))
                                    {
                                        var dateTime = dr["ExpirationDate"].ToString();
                                        data1.ObjectChanges.ExpirationDate = new Primitive<DateTime>(DateTime.Parse(dateTime));
                                    }
                                    if (!dr.IsNull("ESigRequirement"))
                                    {
                                        data1.ObjectChanges.ESigRequirement = WSObjectRef.AssignNamedObject(dr["ESigRequirement"] as string);
                                    }

                                    service.ExecuteTransaction(data1);

                                    ResultStatus status = service.CommitTransaction();
                                    if (status.IsSuccess)
                                        ToEmployee_DataChanged(null, null);

                                    e.Result = status;
                                }
                            }
                            break;
                        }
                }
            }
        }
        #endregion

        #region Private Functions
        /// <summary>
        /// Handle offset adjustment for dates in the grid
        /// </summary>
        /// <param name="data"></param>
        protected virtual void AdjustDateColumn(DataTable data)
        {
            if (data != null)
            {
                var columns = data.Columns.Cast<DataColumn>().Where(column => column.DataType == typeof(DateTime)).ToList();
                var utcOffset = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session).CurrentUserProfile.UTCOffset;
                foreach (var column in columns)
                {
                    var col = column;
                    foreach (var row in data.Rows.Cast<DataRow>().Where(row => (row[col] != null) && !string.IsNullOrEmpty(row[col].ToString())))
                        row[column] = ((DateTime)row[column]).Add(utcOffset.Negate());
                }
            }
        }

        #endregion

        #region Constants

        #endregion

        #region Private Member Variables
        private NamedObjectRef mActiveEmployee = null;
        private JQDataGrid mActiveGrid = null;
        #endregion
    }
}

