// Copyright Siemens 2023
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using Camstar.WCF.Services;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.FormsFramework.Utilities;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using PERS = Camstar.WebPortal.Personalization;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.FormsFramework;
using DocumentFormat.OpenXml.Drawing.Charts;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using DocumentFormat.OpenXml.Office2010.Excel;
using Org.BouncyCastle.Ocsp;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    /// <summary>
    /// For Industry Defect page
    /// </summary>
    public class isIndustryDefect : MatrixWebPart
    {
        /// <summary>
        /// Key: "friendly" label name (as defined in this file), Value: localized label value
        /// </summary>

        #region Properties

        protected virtual JQDataGrid DefectGrid { get { return Page.FindCamstarControl("isCurrentDefectGrid") as JQDataGrid; } }
        protected CWC.TextBox LabelDictionaryJson { get { return Page.FindCamstarControl("LabelDictionaryJson") as CWC.TextBox; } }
        protected CWC.Button RepairAdvisor { get { return Page.FindCamstarControl("RepairAdvisor") as CWC.Button; } }
        protected CWC.TextBox ContainerProductId { get { return Page.FindCamstarControl("ContainerProductId") as CWC.TextBox; } }
        protected CWC.NamedObject Resource {  get {  return Page.FindCamstarControl("DropDownResource") as CWC.NamedObject; } }

        protected CWC.TextBox RepairAdvisorSelectedActions { get { return Page.FindCamstarControl("RepairAdvisorSelectedActions") as CWC.TextBox; } }
        
        protected virtual ContainerListGrid ContainerName { get { return Page.FindCamstarControl("ContainerStatus_ContainerName") as ContainerListGrid; } }

        protected Dictionary<string, string> labelValues;

        protected Dictionary<string, string> LabelValues
        {
            get { return ViewState["LabelValues"] as Dictionary<string, string>; }
            set { ViewState["LabelValues"] = value; }
        }

        #endregion

        #region Public Methods
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
        }

        protected string selResourceNameAtLoad = "";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(System.EventArgs e)
        {
            selResourceNameAtLoad = Resource.Data?.ToString();

            base.OnLoad(e);
            if (!Page.IsPostBack)
                LoadLabels();
            ContainerName.DataChanged += ContainerName_DataChanged;
            DefectGrid.RowSelected += DefectGrid_RowSelected;

            SetTranslationDictionary();
			if (!Page.IsPostBack)
				ScriptManager.RegisterStartupScript(this, this.GetType(), "initializeisDefect", $"isIndustryDefect.initialize({GetClientLabels()});", true);
				
            if (this.Page.EventArgument == "FloatingFrameSubmitParentPostBackArgument" && ContainerName.Data != null)
                LoadDefectGrid();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public virtual void ContainerName_DataChanged(object sender, EventArgs e)
        {
            //load grid
            LoadDefectGrid();

            bool haveContainer = ContainerName.Data != null;
            Page.ActionDispatcher.GetActionByName("Add Defect").IsHidden = !haveContainer;
            Page.ActionDispatcher.GetActionByName("Reset").IsHidden = !haveContainer;

            string selResourceName = Resource.Data?.ToString();

            // now there is no selected Resource
            if (string.IsNullOrEmpty(selResourceName))
            {
                // tell client to select an appropriate resource - either previously selected or the line assignment Resource
                object lineResource = Page.SessionDataContract.GetValueByName(Camstar.WebPortal.Constants.DataMemberConstants.Resource);
                ScriptManager.RegisterStartupScript(this, this.GetType(), "selectResource", $"isIndustryDefect.selectResource('{selResourceNameAtLoad}', '{lineResource ?? ""}');", true);
            }
        }

        /// <summary>
        /// TODO - remove?
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public virtual ResponseData DefectGrid_RowSelected(object sender, JQGridEventArgs args)
        {
            string message = "Row added";
            if (DefectGrid.SelectedRowID == null) //means unselect row
            {
                message = "Row removed";
            }
            return new StatusData(true, message);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void LoadDefectGrid()
        {
            if (DefectGrid != null)
            {
                FrameworkSession fs = FrameworkManagerUtil.GetFrameworkSession(this.Page.Session);
                QueryService queryService = new QueryService(fs.CurrentUserProfile);

                QueryParameters queryParam = new QueryParameters();
                queryParam.Parameters = new QueryParameter[1];
                queryParam.Parameters[0] = new QueryParameter();
                queryParam.Parameters[0].Name = "Container";
                queryParam.Parameters[0].Value = ContainerName.Data != null ? ContainerName.Data.ToString() : "";

                RecordSet record = new RecordSet();
                ResultStatus Result = queryService.Execute("isDefect_ContainerDisplayAllDefects", queryParam, new QueryOptions(), out record);

                if (Result.IsSuccess && record.TotalCount.ToString() != null)
                {
                    System.Data.DataTable resultDt = record.GetAsDataTable();
                    List<isCurrentDefectRow> isCurrentDefectList = new List<isCurrentDefectRow>();
                    if (resultDt != null && resultDt.Rows != null && resultDt.Rows.Count > 0)
                    {
                        foreach (DataRow row in resultDt.Rows)
                        {
                            var isCurrentDefectRow = new isCurrentDefectRow();

                            isCurrentDefectRow.isCurrentDefectsIDString = row[0].ToString();
                            isCurrentDefectRow.isInspectNote = row[1].ToString();
                            isCurrentDefectRow.isInspectUser = row[2].ToString();
                            isCurrentDefectRow.isDefectReasonName = row[3].ToString();
                            isCurrentDefectRow.isRefDes = row[4].ToString();
                            isCurrentDefectRow.isResource = row[5].ToString();
                            isCurrentDefectRow.isSeverity = row[6].ToString();
                            isCurrentDefectRow.isStatus = GetDefectStatus(row[7].ToString());
                            isCurrentDefectRow.isStepPass = row[8].ToString();
                            isCurrentDefectRow.isWorkFlow = row[9].ToString();
                            isCurrentDefectRow.isWorkFlowStep = row[10].ToString();
                            isCurrentDefectRow.isX = row[11].ToString();
                            isCurrentDefectRow.isY = row[12].ToString();
                            isCurrentDefectRow.isRepairNotes = row[13].ToString();
                            isCurrentDefectRow.isRepairResource = row[14].ToString();
                            isCurrentDefectRow.isRepairSpec = row[15].ToString();
                            isCurrentDefectRow.isRepairUser = row[16].ToString();
                            isCurrentDefectRow.isCreateDateConvertToLocal = row[17].ToString();// GetDateTime(row[17].ToString());
                            isCurrentDefectRow.isRepairDateConvertToLocal = row[18].ToString();// GetDateTime(row[18].ToString());
                            isCurrentDefectRow.isSpec = row[19].ToString();
                            isCurrentDefectRow.isDefectReasonId = row[20].ToString();
                            isCurrentDefectRow.isUpdateDateConvertToLocal = row[21].ToString();// GetDateTime(row[18].ToString());

                            isCurrentDefectList.Add(isCurrentDefectRow);
                        }
                    }
                    DefectGrid.Action_SelectRow(null, "deselect");
                    DefectGrid.Data = isCurrentDefectList.ToArray();
                    RepairAdvisorSelectedActions.Data = "";
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "initializeisDefect", $"isIndustryDefect.update();", true);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as Camstar.WebPortal.Personalization.CustomAction;
            if (action != null)
            {
                switch (action.Parameters)
                {
                    case "Reset":
                    {
                        Page.ClearValues();
                        break;
                    }
                }
            }
        }

        private string GetDateTime(string dateTime)
        {
            if (!String.IsNullOrEmpty(dateTime) || !String.IsNullOrWhiteSpace(dateTime) || dateTime.Length.Equals(23))
                return DateTime.ParseExact(dateTime.Substring(0, dateTime.Length - 4), "yyyy-MM-ddTHH:mm:ss", null).ToString("MM/dd/yyyy hh:mm tt");

            return "";
        }

        /// <summary>
        /// TODO - this should probably be a localized string
        /// </summary>
        /// <param name="defectStatusId">string representation of an integer</param>
        /// <returns></returns>
        private string GetDefectStatus(string defectStatusId)
        {
            // string -> int -> enum ex: isOpen
            var defectStatus = new Enumeration<isDefectStatusEnum, int>(int.Parse(defectStatusId));

            // chop off the leading "is"
            return defectStatus.ToString().Substring(2);
        }

        #endregion

        #region Labels

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>Other workspaces should not need to override this</remarks>
        protected virtual void LoadLabels()
        {
            // create mapping of friendly label names to actual label names
            Dictionary<string, string> labelNames = new Dictionary<string, string>
            {
                // Defect bar buttons
                {"NewDefect", "isCurrentDefects_isNewDefect" },
                {"RepairDefect", "isCurrentDefects_isRepairDefect" },
                {"RepairActions", "isCurrentDefects_isRepairActions" },
                {"RepairAdvisor", "isCurrentDefects_isRepairAdvisor" },
                {"NoFaultFound", "isCurrentDefects_isNoFaultFound" },
                {"Reopen", "isCurrentDefects_isReopen" },
                //{"ChangeType", "isCurrentDefects_isChangeType" },
                {"ChangeReason", "isCurrentDefects_isChangeReason" },
                {"Notes", "isCurrentDefects_isNotes" },
                {"DeleteDefect", "isCurrentDefects_isDeleteDefect" }
            };

            // load labels to the cache
            var labelCache = FrameworkManagerUtil.GetLabelCache(System.Web.HttpContext.Current.Session);
            LabelList labelList = new LabelList();
            foreach (KeyValuePair<string, string> item in labelNames)
            {
                labelList.Add(new OM.Label(item.Value));
            }
            labelCache.GetLabels(labelList);

            // create mapping of friendly names to lable values
            labelValues = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> labelName in labelNames)
            {
                var label = labelCache.GetLabelByName(labelName.Value);
                labelValues.Add(labelName.Key, label.Value);
            }
            LabelValues = labelValues;
        }

        /// <summary>
        /// Load labels, serialize to JSON and set to hidden field
        /// </summary>
        protected void SetTranslationDictionary()
        {
            // Build list of all the labels we need on the client side
            LabelList labelList = new LabelList(new List<Label>() {
                new Label("AlertConfirmDelete"),
                new Label("UIInSiteConfirmDeleteDlgTitle")
            });

            // load them to cache
            var labelCache = FrameworkManagerUtil.GetLabelCache(Page.Session);
            ResultStatus getLabelsResult = labelCache.GetLabels(labelList);

            // change label cache to a dictionary and serialize it
            string labelDictionaryJson = new JavaScriptSerializer() { MaxJsonLength = Int32.MaxValue }.Serialize(labelList.ToDictionary(x => x.Name, x => x.Value));
            LabelDictionaryJson.Data = labelDictionaryJson;
        }

        /// <summary>
        /// Creates a JSON string representing an object with label name values for passing to the client
        /// Other workspaces should not need to override this
        /// </summary>
        /// <returns></returns>
        protected virtual string GetClientLabels()
        {
            List<string> labelMap = new List<string>();
            foreach (KeyValuePair<string, string> label in LabelValues)
            {
                labelMap.Add("\"" + label.Key + "\"" + ":" + "\"" + label.Value + "\"");
            }

            return "{" + String.Join(",", labelMap) + "}";
        }

        #endregion

        protected override IEnumerable<ScriptReference> GetScriptReferences()
        {
            yield return new ScriptReference("~/Scripts/User/Industry Solutions/isIndustryDefect.js");
        }
    }

    class isCurrentDefectRow
    {
        public virtual string isInspectNote { get; set; }
        public virtual string isSeverity { get; set; }
        public virtual string isDefectReasonName { get; set; }
        public virtual string isCurrentDefectsIDString { get; set; }
        public virtual string isInspectUser { get; set; }
        public virtual string isStatus { get; set; }
        public virtual string isRefDes { get; set; }
        public virtual string isX { get; set; }
        public virtual string isY { get; set; }
        public virtual string isWorkFlow { get; set; }
        public virtual string isWorkFlowStep { get; set; }
        public virtual string isResource { get; set; }
        public virtual string isCreateDate { get; set; }
        public virtual string isRepairDate { get; set; }
        public virtual string isCreateDateConvertToLocal { get; set; }
        public virtual string isRepairDateConvertToLocal { get; set; }
        public virtual string isUpdateDateConvertToLocal { get; set; }

        public virtual string isRepairUser { get; set; }
        public virtual string isRepairNotes { get; set; }
        public virtual string isRepairResource { get; set; }
        public virtual string isRepairSpec { get; set; }
        public virtual string isStepPass { get; set; }
        public virtual string isSpec { get; set; }
        public virtual string isDefectReasonId { get; set; }
    }
}