//Copyright Siemens 2025  

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.Personalization;
using CamstarPortal.WebControls;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.Utilities;
using System.Web.UI;
using Camstar.WebPortal.PortalConfiguration;

namespace Camstar.WebPortal.WebPortlets.ChangeManagement
{
    public class AssignReviewContentWhereUsed : MatrixWebPart
    {
        #region Controls
        protected virtual JQDataGrid TypesGrid { get { return Page.FindCamstarControl("TypesGrid") as JQDataGrid; } }
        protected virtual JQDataGrid ItemsGrid { get { return Page.FindCamstarControl("ItemsGrid") as JQDataGrid; } }
        protected virtual CWC.TextBox ChangePackage { get { return Page.FindCamstarControl("ChangePackage") as CWC.TextBox; } }
        protected virtual CWC.NamedObject NDOInstanceName { get { return Page.FindCamstarControl("NDOInstanceName") as CWC.NamedObject; } }
        protected virtual CWC.RevisionedObject RDOInstanceName { get { return Page.FindCamstarControl("RDOInstanceName") as CWC.RevisionedObject; } }
        protected virtual CWC.Button CancelBtn { get { return Page.FindCamstarControl("CancelBtn") as CWC.Button; } }

        protected MatrixWebPart RerenderWP { get { return Page.FindCamstarControl("RerenderWP") as MatrixWebPart; } }

        #endregion
        #region Protected Functions
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (Page.IsPostBack)
                return;

            if (Page.DataContract.GetValueByName("WhereCame").Equals("Modeling"))
            {
                LabelCache labelCache = FrameworkManagerUtil.GetLabelCache(Page.Session);
                CancelBtn.Text = labelCache.GetLabelByName("Action_Close").Value;
            }

            var cp = Page.DataContract.GetValueByName("ChangePackage") as NamedObjectRef;
            if (cp != null && ChangePackage != null)
                this.ChangePackage.Data = cp.Name;

            var instanceName = Page.DataContract.GetValueByName<string>("InstanceName");
            var instanceRevision = Page.DataContract.GetValueByName<string>("InstanceRevision");
            var instanceIsROR = Page.DataContract.GetValueByName<bool>("InstanceIsROR");
            var cdoTypeName = Page.DataContract.GetValueByName<string>("CDOTypeName");

            if (instanceName != null && NDOInstanceName != null)
                this.NDOInstanceName.Data = WSObjectRef.AssignNamedObject(instanceName);

            if (instanceName != null && instanceRevision != null && RDOInstanceName != null)
                this.RDOInstanceName.Data = WSObjectRef.AssignRevisionedObject(instanceName, instanceRevision, instanceIsROR);

            var whereUsedObjs = Page.DataContract.GetValueByName("WhereUsedObject") as List<object>;
            if (whereUsedObjs == null)
            {
                if (instanceName == null)
                    return;
                else
                {
                    whereUsedObjs = new List<object>()
                                    {
                                        new SelectedInstanceItem() { Name = instanceName, Revision = instanceRevision, CDOTypeName = cdoTypeName, IsRef = instanceIsROR }
                                    };

                    ItemsGrid.BoundContext.RowSelectionMode = JQGridSelectionMode.Disable;
                }
            }

            var service = new WhereUsedInquiryService(FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session).CurrentUserProfile);
            var data = new WhereUsedInquiry() { IsChangeMgtSettingsRequired = Page.DataContract.GetValueByName<bool>("IsChangeMgtSettingsRequired") };
            List<RevisionedObjectRef> revisionList = new List<RevisionedObjectRef>();
            List<NamedObjectRef> namedList = new List<NamedObjectRef>();
            foreach (var whereUsedObj in whereUsedObjs.Cast<SelectedInstanceItem>())
                if (!string.IsNullOrEmpty(whereUsedObj.Revision))
                    revisionList.Add(new RevisionedObjectRef
                    {
                        Name = whereUsedObj.Name,
                        CDOTypeName = whereUsedObj.CDOTypeName,
                        Revision = whereUsedObj.Revision,
                        RevisionOfRecord = whereUsedObj.IsRef
                    });
                else
                    namedList.Add(new NamedObjectRef
                     {
                         Name = whereUsedObj.Name,
                         CDOTypeName = whereUsedObj.CDOTypeName
                     });
            data.SelectedRevisionedObjects = revisionList.ToArray();
            data.SelectedNamedObjects = namedList.ToArray();
            var request = new WhereUsedInquiry_Request { Info = new WhereUsedInquiry_Info { WhereUsedFiltered = new Info(false, true) } };
            WhereUsedInquiry_Result response;
            ResultStatus res = service.GetEnvironment(data, request, out response);

            if (!res.IsSuccess)
            {
                Page.DisplayMessage(res);
                return;
            }
            if (response.Environment == null || response.Environment.WhereUsedFiltered == null ||
                response.Environment.WhereUsedFiltered.SelectionValues == null)
            {
                Page.DisplayWarning(EmptyResultMsg);
                return;
            }
            var rows = response.Environment.WhereUsedFiltered.SelectionValues.Rows;
            var headers = response.Environment.WhereUsedFiltered.SelectionValues.Headers;

            string cdoName;
            int cdoId;
            string cdoDisplayName;
            DateTime lastEditedTime;
            bool isRor;

            int cdoTypeIdIndex = Array.FindIndex(headers, h => h.Name.Equals("CDOTypeId"));
            int usedByIndex = Array.FindIndex(headers, h => h.Name.Equals("UsedBy"));
            int nameIndex = Array.FindIndex(headers, h => h.Name.Equals("Name"));
            int lastEditedTimeIndex = Array.FindIndex(headers, h => h.Name.Equals("LastEditedTime"));
            int lastEditedByIndex = Array.FindIndex(headers, h => h.Name.Equals("LastEditedBy"));
            int descriptionIndex = Array.FindIndex(headers, h => h.Name.Equals("Description"));
            int revisionIndex = Array.FindIndex(headers, h => h.Name.Equals("Revision"));
            int isRorIndex = Array.FindIndex(headers, h => h.Name.Equals("IsRevisionOfRecord"));

            List<SelectedInstanceItem> items = new List<SelectedInstanceItem>();
            for (int i = 0; i < rows.Length; i++)
            {
                GetCurrentCdoInfo(rows[i].Values[cdoTypeIdIndex], out cdoId, out cdoName, out cdoDisplayName);
                //  If we failed to get a CDOName - it means there is no modeling page, so don't show it in results.
                if (!string.IsNullOrEmpty(cdoName))
                {
                    var hasLastEditedTime = DateTime.TryParse(rows[i].Values[lastEditedTimeIndex], out lastEditedTime);
                    Boolean.TryParse(rows[i].Values[isRorIndex], out isRor);
                    items.Add(new SelectedInstanceItem
                    {
                        CDOTypeName = cdoName,
                        CDOTypeID = rows[i].Values[cdoTypeIdIndex],
                        CDODisplayName = cdoDisplayName,
                        InstanceID = rows[i].Values[usedByIndex],
                        DisplayedName = rows[i].Values[nameIndex] == null ? null :
                            String.IsNullOrEmpty(rows[i].Values[revisionIndex]) ? rows[i].Values[nameIndex] :
                            $"{rows[i].Values[nameIndex]}{CamstarPortalSection.GetRevisionDelimiter()}{rows[i].Values[revisionIndex]}",
                        Name = rows[i].Values[nameIndex],
                        LastEditedTime = hasLastEditedTime ? lastEditedTime : (DateTime?)null,
                        LastEditedBy = rows[i].Values[lastEditedByIndex],
                        Description = rows[i].Values[descriptionIndex],
                        Revision = rows[i].Values[revisionIndex],
                        IsROR = isRor
                    });
                } 
            }

            if (TypesGrid != null)
                TypesGrid.Data = items.GroupBy(i => i.CDODisplayName).Select(group => new ObjectTypeItem
                {
                    DisplayName = group.Key,
                    Instances = group.ToArray()
                }).OrderBy(i => i.DisplayName).ToArray();
        }

        #endregion
        #region Public Functions

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            if (whereUsedClicked)
            {
                OpenWhereUsed();
            }
            base.OnPreRender(e);
        }

        /// <summary>
        /// Register script to open modeling page for the selected CDO
        /// </summary>
        protected void OpenWhereUsed()
        {
            // Values set by grid RowDataContract
            string cdoDefId = Page.DataContract.GetValueByName<string>("CDODefID");
            string instanceId = Page.DataContract.GetValueByName<string>("InstanceID");

            // Look up info on selected CDO
            var cache = new MaintCDOCache();
            CDOData cdoData = cache.MaintCdoData.Find(cdo => cdo.CDODefID == cdoDefId);

            // Look up VP in Portal config
            CDOForm[] cdoForms = null;
            if (CamstarPortalSection.Settings.CDOFormsSettings != null && CamstarPortalSection.Settings.CDOFormsSettings.CDOForms != null)
                cdoForms = CamstarPortalSection.Settings.CDOFormsSettings.CDOForms;
            
            if(cdoForms == null)
            {
                Page.DisplayMessage("No CDO forms configured.", false);
                return;
            }

            CDOForm cdoForm = cdoForms.FirstOrDefault(form => form.Service == cdoData.MaintService);
            if(cdoForm == null)
            {
                Page.DisplayMessage($"No CDO form configured for {cdoData.MaintService}.", false);
                return;
            }

            // construct query string and script to open tab
            string queryString = $"HideInstanceList=false&ResetCallStack=true&id={cdoDefId}&isRDO={cdoData.IsRDO}&maint={cdoData.MaintService}&wip={cdoData.IsWIPSupported}&preventNew=true&name={cdoData.CDOName}&CDOName={cdoData.CDOName}&maintTypeId={cdoData.MaintTypeID}&InstanceID={instanceId}";// &pStackId=j3d6enlggesxjygxml1zuq";

            string openTabScript = $"parent.parent.OpenModelingPageWitinTab('ctl00_WebPartManager_ModelingTabWP_TabContainer', '{cdoForm.PageName}', '{cdoData.CDOName}', '{queryString}');";

            ScriptManager.RegisterStartupScript(
                RerenderWP,
                RerenderWP.GetType(),
                "openWhereUsed",
                openTabScript,
                true);
            this.WebPartManager.RenderToClient(RerenderWP);
        }

        bool whereUsedClicked = false;
        public override void WebPartCustomAction(object sender, CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as CustomAction;
            if (action == null || (!action.Parameters.Equals("NotifyParent") && action.Parameters != "WhereUsed") || TypesGrid.Data == null)
                return;

            if(action.Parameters == "WhereUsed")
            {
                whereUsedClicked = true;
            }
            else
            {
                var selectedObjects = new List<ObjectTypeItem>();
                var subGrids = ItemsGrid != null ? ItemsGrid.GridContext.GetSubContexts() : null;
                if (subGrids != null)
                {
                    foreach (var grid in subGrids)
                    {
                        var items = grid.GetSelectedItems(false);
                        if (items == null || items.Length <= 0)
                            continue;
                        var objItem = (TypesGrid.Data as ObjectTypeItem[]).FirstOrDefault(o => o.Instances.Contains(items.First()));
                        if (objItem != null)
                        {
                            var instanceArray = items.Select(i => (SelectedInstanceItem)i).ToArray();
                            selectedObjects.Add(new ObjectTypeItem()
                            {
                                Name = ((SelectedInstanceItem)items.First()).CDOTypeName,
                                CDOID = Convert.ToInt32(((SelectedInstanceItem)items.First()).CDOTypeID),
                                DisplayName = objItem.DisplayName,
                                Instances = instanceArray
                            });
                            foreach (var item in instanceArray)
                            {
                                bool all = true;
                                SelectedInstanceItem first = null;
                                foreach (SelectedInstanceItem ins in objItem.Instances)
                                {
                                    if (ins.InstanceID != item.InstanceID)
                                        continue;
                                    all = false;
                                    first = ins;
                                    break;
                                }
                                if (all || !first.IsSave)
                                    item.IsSave = true;
                            }
                        }
                    }

                }
                Page.CurrentCallStack.Parent.Context.LocalSession[WhereUsedObjects] = selectedObjects.ToArray();
                Page.CloseFloatingFrame(true);
            }
        }
        #endregion
        #region Private Functions
        protected virtual void GetCurrentCdoInfo(string cdoIdString, out int cdoId, out string cdoName, out string cdoDisplayName)
        {
            cdoName = null;
            cdoDisplayName = null;
            int.TryParse(cdoIdString, out cdoId);
            Dictionary<string, CDOData> cdoData;
            if (Page.PortalContext != null && Page.PortalContext.LocalSession["ObjectsList"] != null)
                cdoData = Page.PortalContext.LocalSession["ObjectsList"] as Dictionary<string, CDOData>;
            else
            {
                cdoData = ModelingObjectList.GetExportObjects(false);
                Page.PortalContext.LocalSession["ObjectsList"] = cdoData;
            }
            if (cdoData != null)
            {
                var d = cdoData.FirstOrDefault(s => s.Value.CDODefID == cdoIdString);
                cdoName = d.Value.CDOName;
                cdoDisplayName = d.Value.CDODisplayName;
            }
        }
        #endregion
        public static readonly string WhereUsedObjects = "AssignReviewContentWhereUsed.WhereUsedObjects";
        

        public static readonly string EmptyResultMsg = FrameworkManagerUtil.GetLabelCache().GetLabelByName("Lbl_NoRowsReturned").Value.ToString();
    }
}

