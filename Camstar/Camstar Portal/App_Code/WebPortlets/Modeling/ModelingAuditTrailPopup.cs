// Copyright Siemens 2023  
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.Personalization;
using OM = Camstar.WCF.ObjectStack;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.Utilities;
using Camstar.Util;

/// <summary>
/// Summary description for ModelingAuditTrail
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Modeling
{
    public class ModelingAuditTrailPopup : MatrixWebPart
    {
        protected virtual OM.Enumeration<OM.MaintainableObjectEnum, string> ObjectTypeName
        {
            get { return ViewState["ObjectTypeName"] as OM.Enumeration<OM.MaintainableObjectEnum, string>; }
            set { ViewState["ObjectTypeName"] = value; }
        }

        
        protected override void OnLoad(EventArgs e)
        {
            (DetailsGrid.GridContext as BoundContext).IsTree = true;
            (DetailsGrid.GridContext as BoundContext).TreeExpandColumn = "DisplayLabel";
            (DetailsGrid.GridContext as BoundContext).DataLoad += ModelingAuditTrailPopup_DataLoad;
            (DetailsGrid.GridContext as BoundContext).SnapCompleted += ModelingAuditTrailPopup_SnapCompleted;

            ClearAllButton.Click += ClearAllButton_Click;

            base.OnLoad(e);
            if (!Page.IsPostBack)
            {
                var objectTypeName = Page.DataContract.GetValueByName("ObjectTypeName") as OM.Enumeration<OM.MaintainableObjectEnum, string>;
                if (objectTypeName != null)
                    ObjectTypeName = objectTypeName;
            }
            TxnGrid.Load += TxnGrid_Load;
        }

        protected virtual void TxnGrid_Load(object sender, EventArgs e)
        {
            var service = new WCFService(Page);
            var res = service.ExecuteFunction(Page.PrimaryServiceType, "GetAuditTrailHeaders");
            if (!res.IsSuccess)
                TxnGrid.ClearData();
            else
                Page.StatusBar.ClearMessage();
        }

        public override void RequestValues(OM.Info serviceInfo, OM.Service serviceData)
        {
            base.RequestValues(serviceInfo, serviceData);
            var servData = serviceData as OM.ModelingAuditTrailInquiry;
            if (servData == null)
                return;
            var objectTypeName = ObjectTypeName ?? Page.DataContract.GetValueByName("ObjectTypeName");
            if (objectTypeName is string && !string.IsNullOrEmpty(objectTypeName as string))
                servData.ObjectTypeName = new OM.Enumeration<OM.MaintainableObjectEnum, string>(objectTypeName as string);
            else if (objectTypeName is OM.Enumeration<OM.MaintainableObjectEnum, string>)
                servData.ObjectTypeName = objectTypeName as OM.Enumeration<OM.MaintainableObjectEnum, string>;
             var dcRevision = Page.DataContract.GetValueByName("Revision");
            if (dcRevision != null) 
            { 
                var revision = dcRevision.ToString();
            if (!string.IsNullOrEmpty(revision))
                servData.ObjectRevisionOrParent = revision;
            }
        }

        protected virtual void ModelingAuditTrailPopup_SnapCompleted(DataTable dataWindowTable)
        {
            foreach (var r in dataWindowTable.Rows.OfType<DataRow>())
            {
                r.BeginEdit();
                int level = 0;
                if (!(r["parent"] == null || r["parent"] is DBNull))
                {
                    var rparent = dataWindowTable.Select("_id_column='" + (r["parent"] as string) + "'");
                    if (rparent.Any())
                    {
                        level = (int)rparent[0]["level"] + 1;
                    }
                }
                r["level"] = level;
                r["isLeaf"] = (r["ObjectInstanceId"] == null || r["ObjectInstanceId"] is System.DBNull);
            }
            dataWindowTable.AcceptChanges();
        }

        protected virtual bool ModelingAuditTrailPopup_DataLoad(GridContext context)
        {
            var pcnt = (context.ParentGridContext as ItemDataContext);
            var dataHeaders = pcnt.Data as OM.ModelingAuditTrailHeader[];
            if (dataHeaders != null)
            {
                int position;
                if (int.TryParse(context.ParentRowID, out position))
                {
                    var hdrId = dataHeaders[position].Self.ID;
                    var objId = dataHeaders[position].ObjectInstanceId.Value;
                    if (context.ExpandedRowID != null)
                    {
                        // User expands the field
                        var item = (context as ItemDataContext).GetItem(context.ExpandedRowID);
                        if (item is OM.AuditTrailSubentityField)
                        {
                            objId = (item as OM.AuditTrailSubentityField).FieldValues[0].ObjectInstanceId.Value;
                        }
                    }
                    var sc = (context.SubGridTemplateContext as ItemDataContext);
                    var parent = context.ExpandedRowID ?? string.Empty;

                    List<OM.AuditTrailField> newfields;
                    int isertionIndex;
                    // load existing items
                    if (sc.AssociatedChildData != null && sc.AssociatedChildData.ContainsKey(context.ParentRowID) && context.ExpandedRowID != null)
                    {
                        newfields = new List<OM.AuditTrailField>(sc.AssociatedChildData[context.ParentRowID] as OM.AuditTrailField[]);
                        isertionIndex = int.Parse(context.ExpandedRowID) + 1;
                    }
                    else
                    {
                        newfields = new List<OM.AuditTrailField>();
                        isertionIndex = 0;
                    }

                    // skip already requested and displayed items.
                    if (newfields.Any(field => string.Equals(field.DisplayName.Value, parent)))
                        return false;

                    var details = GetModelingAuditTrailDetails(hdrId, objId);
                    if (details?.Fields != null)
                    {
                        int addedItemsCount = 0;
                        // Multiple values transformed to Field with one-item array of values
                        foreach (var fd in details.Fields)
                        {
                            if (fd is OM.AuditTrailSubentityField)
                            {
                                if ((fd as OM.AuditTrailSubentityField).FieldValues != null)
                                    foreach (var fv in (fd as OM.AuditTrailSubentityField).FieldValues)
                                    {
                                        if (fv.Action != null && !string.IsNullOrEmpty(fv.Action.Value) && string.Equals(fv.Action.Value, "create", StringComparison.CurrentCultureIgnoreCase))
                                        {
                                            fv.Action = GetLabelCacheValue("Lbl_Create", "Create");
                                        }
                                        if (fv.Action != null && !string.IsNullOrEmpty(fv.Action.Value) && string.Equals(fv.Action.Value, "update", StringComparison.CurrentCultureIgnoreCase))
                                        {
                                            fv.Action = GetLabelCacheValue("Lbl_Update", "Update");
                                        }
                                        if (fv.Action != null && !string.IsNullOrEmpty(fv.Action.Value) && string.Equals(fv.Action.Value, "delete", StringComparison.CurrentCultureIgnoreCase))
                                        {
                                            fv.Action = GetLabelCacheValue("Lbl_Delete", "Delete");
                                        }
                                        newfields.Insert(isertionIndex++,
                                        new OM.AuditTrailSubentityField
                                            {
                                                DisplayLabel = fd.DisplayLabel,
                                                Action = fd.Action,
                                                FieldAction = fd.FieldAction,
                                                FieldName = fd.FieldName,
                                                FieldValues = new[] { fv },
                                                DisplayName = parent,
                                                PackageName = fd.PackageName
                                            });
                                        addedItemsCount++;
                                    }
                            }
                            else if (fd is OM.AuditTrailTrivialField)
                            {
                                var fieldDef = fd as OM.AuditTrailTrivialField;
                                if (fieldDef.FieldValues != null)
                                {
                                    foreach (var fv in fieldDef.FieldValues)
                                    {
                                        if (fv.OldValue != null)
                                        {
                                            fv.OldValue = fv.OldValue.Value.Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&#x22;").Replace("'", "&#x27;");
                                        }
                                        if (fv.Action != null && !string.IsNullOrEmpty(fv.Action.Value) && string.Equals(fv.Action.Value, "create", StringComparison.CurrentCultureIgnoreCase))
                                        {
                                            fv.OldValue = string.Empty;
                                            fv.Action = GetLabelCacheValue("Lbl_Create", "Create");
                                        }
                                        if (fv.Action != null && !string.IsNullOrEmpty(fv.Action.Value) && string.Equals(fv.Action.Value, "update", StringComparison.CurrentCultureIgnoreCase))
                                        {
                                            fv.Action = GetLabelCacheValue("Lbl_Update", "Update");
                                        }
                                        if (fv.Action != null && !string.IsNullOrEmpty(fv.Action.Value) && string.Equals(fv.Action.Value, "delete", StringComparison.CurrentCultureIgnoreCase))
                                        {
                                            fv.Action = GetLabelCacheValue("Lbl_Delete", "Delete");
                                        }
                                        if (fieldDef.FieldType == OM.DataTypeEnum.Float && fieldDef.FieldSubType == OM.DataTypeEnum.String) // duration data case.
                                        {
                                            if (fv.OldValue != null && !string.IsNullOrEmpty(fv.OldValue.Value))
                                                fv.OldValue = CWC.Duration.ConvertDataToDuration(fv.OldValue.Value, true);
                                            if (fv.NewValue != null && !string.IsNullOrEmpty(fv.NewValue.Value))
                                                fv.NewValue = CWC.Duration.ConvertDataToDuration(fv.NewValue.Value, true);
                                        }
                                        if (fieldDef.FieldType == OM.DataTypeEnum.Timestamp)
                                        {
                                            DateTime oldDateTime;
                                            if (fv.OldValue != null && !string.IsNullOrEmpty(fv.OldValue.Value) &&
                                                DateTime.TryParse(fv.OldValue.Value, out oldDateTime))
                                            {
                                                string fieldName = fieldDef.FieldName.Value;
                                                string  parsedDateTime = ProcessDateTimeLocalValue(fieldName, details.Fields, true);
                                                if (!string.IsNullOrEmpty(parsedDateTime))
                                                    fv.OldValue = parsedDateTime;
                                                else
                                                    fv.OldValue = oldDateTime.ToString();
                                            }

                                            DateTime newDateTime;
                                            if (fv.NewValue != null && !string.IsNullOrEmpty(fv.NewValue.Value) &&
                                                DateTime.TryParse(fv.NewValue.Value, out newDateTime))
                                            {
                                                string fieldName = fieldDef.FieldName.Value;
                                                string parsedDateTime = ProcessDateTimeLocalValue(fieldName, details.Fields, false);
                                                if (!string.IsNullOrEmpty(parsedDateTime))
                                                    fv.NewValue = parsedDateTime;
                                                else
                                                    fv.NewValue = newDateTime.ToString();
                                           
                                            }
                                        }

                                        if (fieldDef.FieldName == "TimeZone")
                                        {
                                            if (fv.OldValue != null && !string.IsNullOrEmpty(fv.OldValue.Value))
                                                fv.OldValue = GetTimeZoneNameByCode(fv.OldValue.Value);

                                            if (fv.NewValue != null && !string.IsNullOrEmpty(fv.NewValue.Value))
                                                fv.NewValue = GetTimeZoneNameByCode(fv.NewValue.Value);
                                        }

                                        newfields.Insert(isertionIndex++,
                                            new OM.AuditTrailTrivialField
                                            {
                                                DisplayLabel = fd.DisplayLabel,
                                                Action = fd.Action,
                                                FieldAction = fd.FieldAction,
                                                FieldName = fd.FieldName,
                                                FieldValues = new[] { fv },
                                                DisplayName = parent,
                                                PackageName = fd.PackageName
                                            });
                                        addedItemsCount++;
                                    }
                                }
                            }
                        }

                        int expandedIndex;
                        if (addedItemsCount > 0 && int.TryParse(parent, out expandedIndex))
                        {
                            foreach (var field in newfields)
                            {
                                int fieldParentIndex;
                                if (int.TryParse(field.DisplayName.Value, out fieldParentIndex))
                                {
                                    if (expandedIndex < fieldParentIndex)
                                        field.DisplayName = (fieldParentIndex + addedItemsCount).ToString("D" + parent.Length);
                                }
                            }
                        }

                        if (sc.AssociatedChildData == null)
                            sc.AssociatedChildData = new Dictionary<string, object>();
                        if (sc.AssociatedChildData.ContainsKey(context.ParentRowID))
                            sc.AssociatedChildData[context.ParentRowID] = newfields.ToArray();
                        else
                            sc.AssociatedChildData.Add(context.ParentRowID, newfields.ToArray());
                    }
                }
            }
            return false;
        }

        protected virtual OM.ModelingAuditTrail GetModelingAuditTrailDetails(string headerId, string objectInstanceId)
        {
            var data = new OM.ModelingAuditTrailInquiry()
                {
                    SelectedHeader = new WCF.ObjectStack.SubentityRef(headerId),
                    SelectedObjectId = objectInstanceId
                };
            var info = new OM.ModelingAuditTrailInquiry_Info()
                {
                    AuditTrailDetail = new OM.ModelingAuditTrail_Info()
                            {
                                DisplayName = new OM.Info(true),
                                Fields = new OM.AuditTrailField_Info() { RequestValue = true }
                            }
                };
            var service = new WCF.Services.ModelingAuditTrailInquiryService(FormsFramework.Utilities.FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);
            var request = new WCF.Services.ModelingAuditTrailInquiry_Request() { Info = info };
            var result = new WCF.Services.ModelingAuditTrailInquiry_Result();
            OM.ResultStatus rs = service.GetAuditTrailDetails(data, request, out result);
            if (rs.IsSuccess)
                return result.Value.AuditTrailDetail;
            else
                Page.DisplayMessage(rs);
            return null;
        }

        protected virtual void ClearAllButton_Click(object sender, EventArgs e)
        {
            TxnGrid.ClearData();
            PackageName.ClearData();
            FromDate.ClearData();
            ToDate.ClearData();
        }

        private string ProcessDateTimeLocalValue(string fieldName, OM.AuditTrailField[] fields, bool isOldValue)
        {
            TimeSpan utcOffset = FrameworkManagerUtil.GetFrameworkSession(Page.Session).CurrentUserProfile.UTCOffset;
            DateTime gmtDateTime;
            if (!string.IsNullOrEmpty(fieldName) && !fieldName.EndsWith("GMT"))
            {
                var gmtField = fields.FirstOrDefault(item => item.FieldName.Value.Equals(fieldName + ("GMT"))) as OM.AuditTrailTrivialField;
                if (gmtField != null)
                {
                    string gmtValue = isOldValue ? gmtField.FieldValues.FirstOrDefault(item => item.OldValue != null && !string.IsNullOrEmpty(item.OldValue.Value))?.OldValue.Value
                                              : gmtField.FieldValues.FirstOrDefault(item => item.NewValue != null && !string.IsNullOrEmpty(item.NewValue.Value))?.NewValue.Value;

                    if (!string.IsNullOrEmpty(gmtValue) && DateTime.TryParse(gmtValue, out gmtDateTime))
                    {
                        return gmtDateTime.Add(utcOffset).ToString();
                    }

                }

            }

            return "";
        }

        private string GetTimeZoneNameByCode(string timeZoneCode)
        {
            var timeZone = CamstarPortalSection.Settings.TimeZoneSettings.TimeZones.Where(t => t.TimeZoneCode == timeZoneCode).Select(t => String.Format("(UTC{0}) {1}", DateUtil.ExtractTimeOffset(t.TimeZoneCode), t.Name)).FirstOrDefault();

            if (String.IsNullOrWhiteSpace(timeZone))
                timeZone = timeZoneCode;

            return timeZone;
        }

        #region Properties
        protected virtual JQDataGrid DetailsGrid
        {
            get { return Page.FindCamstarControl("DetailsGrid") as JQDataGrid; }
        }
        protected virtual CWC.TextBox PackageName
        {
            get { return Page.FindCamstarControl("PackageName") as CWC.TextBox; }
        }
        protected virtual CWC.DateChooser FromDate
        {
            get { return Page.FindCamstarControl("FromDate") as CWC.DateChooser; }
        }
        protected virtual CWC.DateChooser ToDate
        {
            get { return Page.FindCamstarControl("ToDate") as CWC.DateChooser; }
        }
        protected virtual JQDataGrid TxnGrid
        {
            get { return Page.FindCamstarControl("TxnGrid") as JQDataGrid; }
        }
        protected virtual CWC.TextBox ObjectTypeNameField
        {
            get { return Page.FindCamstarControl("ObjectTypeNameField") as CWC.TextBox; }
        }
        protected virtual CWC.Button ClearAllButton
        {
            get { return Page.FindCamstarControl("ClearAllButton") as CWC.Button; }
        }
        #endregion
    }
}
