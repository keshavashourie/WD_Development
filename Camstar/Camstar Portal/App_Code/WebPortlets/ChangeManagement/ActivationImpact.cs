// Copyright Siemens 2023  
using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Data.Linq;
using System.Web;
using System.Web.UI;
using System.Xml.Linq;
using System.Threading.Tasks;

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
using Camstar.XMLClient.API;

namespace Camstar.WebPortal.WebPortlets
{

    /// <summary>
    /// Code behind to use Impact Engine to show change package differences from Source to Target
    /// </summary>
    public class ActivationImpact : MatrixWebPart
    {
        #region Controls        
        protected virtual Button btnView
        {
            get
            {
                return Page.FindCamstarControl("btnView") as Button;
            }
        }
        protected virtual DropDownList objectTypes
        {
            get
            {
                return Page.FindCamstarControl("objectTypes") as DropDownList;
            }
        }
        protected virtual DropDownList targetSystem
        {
            get
            {
                return Page.FindCamstarControl("targetSystem") as DropDownList;
            }
        }
        protected virtual NamedObject selectedPackage
        {
            get
            {
                return Page.FindCamstarControl("selectedPackage") as NamedObject;
            }
        }
        protected virtual JQDataGrid differenceGrid
        {
            get
            {
                return Page.FindCamstarControl("differenceGrid") as JQDataGrid;
            }
        }
        #endregion

        #region Protected Functions

        /// <summary>
        /// OnLoad override
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                if (!Page.IsPostBack)
                {
                    selectedPackage.LoadOrClearDependentValues();
                    LoadDetailsSummary();
                    if (targetSystem != null)
                    {
                        GetTargetSystemInfo();
                    }
                    if (CurrentTargetInfo != null)
                    {
                        targetSystem.Data = CurrentTargetInfo.TargetSystemName;
                    }
                }
                Page.OnRequestFormValues += Page_OnRequestFormValues;
                targetSystem.DataChanged += targetSystem_DataChanged;
                selectedPackage.DataChanged += selectedPackage_DataChanged;
                objectTypes.DataChanged += objectTypes_DataChanged;
                btnView.Click += btnView_Click;
                base.OnLoad(e);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Changed Data event for Object Types Picklist
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void objectTypes_DataChanged(object sender, EventArgs e)
        {
            var d = GetPackageDetails();
            foreach (var pair in d)
            {
                if (pair.Value.IsSuccess)
                {
                    if (objectTypes.Data != null && objectTypes.Data.ToString() != "undefined")
                    {
                        Instances = pair.Key.Value.PackageDetails.Instances.ToList().Where(i => i.ObjectType.Value.ToString() == objectTypes.Data.ToString()).ToArray();
                    }
                    else
                    {
                        Instances = pair.Key.Value.PackageDetails.Instances;
                    }
                }
            }
        }

        /// <summary>
        /// Changed Data event for Target System Picklist
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void targetSystem_DataChanged(object sender, EventArgs e)
        {
            if (targetSystem.Data != null)
            {
                GetTargetSystemInfo();
            }
        }

        /// <summary>
        /// View Impact Button click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void btnView_Click(object sender, EventArgs e)
        {
            var lstGridRows = new List<Row>();
            Header[] headerGrid = { new Header { Name = "Impact" }, new Header { Name = "Object" }, new Header { Name = "Instance" }, new Header { Name = "Description" }, new Header { Name = "ChangedField" }, new Header { Name = "SourceSystemValue" }, new Header { Name = "TargetSystemValue" } };

            if (Instances == null || Instances.Count() == 0)
            {
                var noContent = FrameworkManagerUtil.GetLabelValue("DeployChangePkgDetail_InstancesMustBeAssigned");
                DisplayMessage(new ResultStatus(noContent, false));
            }
            else
            {
                foreach (var i in Instances)
                {
                    string impact = string.Empty;
                    string objType = i.ObjectTypeName != null ? i.ObjectTypeName.Value : string.Empty;
                    string instance = i.ObjectName != null ? i.ObjectName.Value : string.Empty;
                    string descr = i.Description != null ? i.Description.Value : string.Empty;
                    string changedField = string.Empty;
                    string sourceSystemValue = string.Empty;
                    string targetSystemValue = string.Empty;
                    string[] vals = { impact, objType, instance, descr, changedField, sourceSystemValue, targetSystemValue };
                    var rowGrid = new Row
                    {
                        Values = vals
                    };
                    lstGridRows.Add(rowGrid);
                }
                var rsGrid = new RecordSet
                {
                    Rows = lstGridRows.ToArray()
                    ,
                    Headers = headerGrid
                };
                if (rsGrid.Rows.Count() > 0)
                {
                    differenceGrid.ClearData();
                    DataTable dt = new DataTable();
                    dt.Columns.Add("Impact");
                    dt.Columns.Add("Object");
                    dt.Columns.Add("Instance");
                    dt.Columns.Add("Description");
                    dt.Columns.Add("ChangedField");
                    dt.Columns.Add("SourceSystemValue");
                    dt.Columns.Add("TargetSystemValue");

                    // Call Impact Engine                
                    XName xName = XName.Get("Fields");
                    ImpactEngine engine = new ImpactEngine();

                    engine.Instances = Instances;
                    engine.CurrentCDOCache = FrameworkManagerUtil.GetCDOCache(Page.Session);

                    // Get Source Info
                    TargetSystem sourceSystem = GetSourceSystemInfo();
                    if (sourceSystem != null && sourceSystem.IPAddress != null && sourceSystem.IPAddress != null)
                    {
                        engine.SourceHost = sourceSystem.IPAddress.Value;
                        engine.SourcePort = sourceSystem.Port.Value;
                    }
                    else
                    {
                        LabelCache labelCache = FrameworkManagerUtil.GetLabelCache(Page.Session);
                        if (labelCache != null)
                        {
                            Camstar.WCF.ObjectStack.Label targetInfoNotFoundLabel = new WCF.ObjectStack.Label();
                            targetInfoNotFoundLabel = labelCache.GetLabelByName("Lbl_TargetNotFound");
                            //Page.DisplayMessage(new ResultStatus(targetInfoNotFoundLabel.Value ?? targetInfoNotFoundLabel.DefaultValue, false));
                        }
                        else
                        {
                            Page.DisplayMessage(new ResultStatus("Target System Information not found", false));
                        }
                        return;
                    }

                    engine.SourceUsername = base.GetUserName();
                    engine.SourcePassword = base.GetPassword();

                    // Get Target Info
                    if (targetSystem.Data != null)
                    {
                        engine.TargetHost = CurrentTargetInfo.TargetSystem.IPAddress.Value;
                        engine.TargetPort = CurrentTargetInfo.TargetSystem.Port.Value;
                        engine.TargetUsername = CurrentTargetInfo.TargetSystem.UserName.Value;
                        string decryptedPwd = Camstar.Util.CryptUtil.Decrypt(CurrentTargetInfo.TargetSystem.Password.Value);
                        engine.TargetPassword = decryptedPwd;

                        try
                        {
                            engine.ProcessInstances();
                        }
                        catch (csiClientException)
                        {
                            LabelCache labelCache = FrameworkManagerUtil.GetLabelCache(Page.Session);
                            if (labelCache != null)
                            {
                                Camstar.WCF.ObjectStack.Label targetNotFoundLabel = new WCF.ObjectStack.Label();
                                targetNotFoundLabel = labelCache.GetLabelByName("Lbl_TargetNotFound");
                                HtmlString html = new HtmlString(targetNotFoundLabel.Value);
                                Page.StatusBar.WriteError(html.ToHtmlString());
                            }
                        }
                        foreach (var item in engine.CDODifferences.OrderBy(c => c.ObjectType))
                        {
                            DataRow dr = dt.NewRow();
                            dr["Impact"] = item.Result;
                            dr["Object"] = item.ObjectType;
                            dr["Instance"] = item.DisplayedName;
                            dr["Description"] = item.Description;
                            dr["ChangedField"] = !string.IsNullOrEmpty(item.FieldName) ? item.FieldName : string.Empty;

                            //Added below code to fix CPR 329369:2304 (TAC 10344887) (PR?) SMTP user acct's password is visible when Modeler run an audit
                            if (Convert.ToString(dr["ChangedField"]).ToLower().Contains("password"))
                            {
                                dr["SourceSystemValue"] = !string.IsNullOrEmpty(item.SourceValue) ? (!Camstar.Util.CryptUtil.IsEncrypted(item.SourceValue)?Camstar.Util.CryptUtil.Encrypt(item.SourceValue): item.SourceValue) : string.Empty;
                                dr["TargetSystemValue"] = !string.IsNullOrEmpty(item.TargetValue) ? (!Camstar.Util.CryptUtil.IsEncrypted(item.TargetValue) ?Camstar.Util.CryptUtil.Encrypt(item.TargetValue): item.TargetValue) : string.Empty;
                            }
                            else
                            {
                                dr["SourceSystemValue"] = !string.IsNullOrEmpty(item.SourceValue) ? item.SourceValue : string.Empty;
                                dr["TargetSystemValue"] = !string.IsNullOrEmpty(item.TargetValue) ? item.TargetValue : string.Empty;
                            }

                            dt.Rows.Add(dr);
                        }

                        differenceGrid.Data = dt;
                    }
                    else
                    {
                        LabelCache labelCache = FrameworkManagerUtil.GetLabelCache(Page.Session);
                        if (labelCache != null)
                        {
                            Camstar.WCF.ObjectStack.Label noTargetLabel = new WCF.ObjectStack.Label();
                            noTargetLabel = labelCache.GetLabelByName("Lbl_TargetSystemRequired");
                            HtmlString html = new HtmlString(noTargetLabel.Value);
                            Page.StatusBar.WriteError(html.ToHtmlString());
                        }
                    }
                }
                else
                {
                    LabelCache labelCache = FrameworkManagerUtil.GetLabelCache(Page.Session);
                    if (labelCache != null)
                    {
                        Camstar.WCF.ObjectStack.Label noTargetLabel = new WCF.ObjectStack.Label();
                        noTargetLabel = labelCache.GetLabelByName("Lbl_TargetSystemRequired");
                        HtmlString html = new HtmlString(noTargetLabel.Value);
                        Page.StatusBar.WriteError(html.ToHtmlString());
                    }
                }
            }
        }

        /// <summary>
        /// Changed Data event for hidden Package proxy
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void selectedPackage_DataChanged(object sender, EventArgs e)
        {
            LoadDetailsSummary();
        }

        /// <summary>
        /// Clears data on request form values
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void Page_OnRequestFormValues(object sender, FormsFramework.FormProcessingEventArgs e)
        {
            var data = e.Data as GetImpactDetailsInquiry;
            if (data != null)
            {
                data.ChangePackageHeader = null;
            }
        }
        #endregion

        #region Public Functions

        #endregion

        #region Private Functions  
        /// <summary>
        /// Gets Target System Info
        /// </summary>
        protected virtual void GetTargetSystemInfo()
        {
            // Get Change Package Details
            Dictionary<GetImpactDetailsInquiry_Result, ResultStatus> d = GetPackageDetails();
            foreach (var pair in d)
            {
                if (pair.Value.IsSuccess)
                {
                    // Set Current Target Info based on target system picklist value
                    if (pair.Key.Value.PackageDetails.TargetSystems != null)
                    {
                        List<TargetSystemDetail> lst = pair.Key.Value.PackageDetails.TargetSystems.ToList();
                        if (targetSystem.Data != null)
                        {
                            CurrentTargetInfo = lst.Find(delegate (TargetSystemDetail tsd) { return tsd.TargetSystemName.Value == targetSystem.Data.ToString(); });
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get Source System Info
        /// </summary>
        /// <returns></returns>
        protected virtual TargetSystem GetSourceSystemInfo()
        {
            TargetSystem sourceSystem = null;
            // Get Change Package Details
            Dictionary<GetImpactDetailsInquiry_Result, ResultStatus> d = GetPackageDetails();
            foreach (var pair in d)
            {
                if (pair.Value.IsSuccess)
                {
                    // Set the source system value
                    sourceSystem = pair.Key.Value.SourceSystem;
                }
            }

            return sourceSystem;
        }

        /// <summary>
        /// Private method to grab PackageDetails
        /// </summary>
        /// <returns>Dictionary of GetChangePackageDetails_Result and ResultStatus</returns>
        protected virtual Dictionary<GetImpactDetailsInquiry_Result, ResultStatus> GetPackageDetails()
        {
            // Create service
            var service = new GetImpactDetailsInquiryService(
                        Camstar.WebPortal.FormsFramework.Utilities.FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);
            var cdo = new GetImpactDetailsInquiry { ChangePackage = selectedPackage.Data as NamedObjectRef };
            var request = new GetImpactDetailsInquiry_Request();
            request.Info = new GetImpactDetailsInquiry_Info
            {
                SourceSystem = new TargetSystem_Info
                {
                    IPAddress = new Info(true)
                    ,
                    Port = new Info(true)
                }
                ,
                PackageDetails = new ChangePackageDetails_Info
                {
                    TargetSystems = new TargetSystemDetail_Info
                    {
                        TargetSystem = new TargetSystem_Info
                        {
                            Name = new Info(true)
                            ,
                            IsSourceSystem = new Info(true)
                            ,
                            IsNotACamstarServer = new Info(true)
                            ,
                            IPAddress = new Info(true)
                            ,
                            Port = new Info(true)
                            ,
                            UserName = new Info(true)
                            ,
                            Password = new Info(true)
                        }
                        ,
                        TargetSystemName = new Info(true)
                    }
                    ,
                    Instances = new CPModelingInstanceDtl_Info
                    {
                        ObjectName = new Info(true)
                        ,
                        ObjectTypeValue = new Info(true)
                        ,
                        ObjectType = new Info(true)
                        ,
                        ObjectTypeName = new Info(true)
                        ,
                        ObjectInstanceId = new Info(true)
                        ,
                        Description = new Info(true)
                        ,
                        Revision = new Info(true)
                    }
                }
                ,
                ChangePackageHeader = new ChangePackageHeader_Info
                {
                    Step = new Info(true)
                    ,
                    StepIcon = new Info(true)
                    ,
                    Workflow = new Info(true)
                    ,
                    Description = new Info(true)
                    ,
                    LastUpdatedDate = new Info(true)
                    ,
                    OwnerName = new Info(true)
                    ,
                    TargetSystemName = new Info(true)

                }

            };

            // Call service and get results
            GetImpactDetailsInquiry_Result result;
            ResultStatus res = service.GetImpactPackageDetails(cdo, request, out result);

            var d = new Dictionary<GetImpactDetailsInquiry_Result, ResultStatus>();
            d.Add(result, res);
            return d;
        }

        /// <summary>
        /// Load Change Package Details on inital load
        /// </summary>
        protected virtual void LoadDetailsSummary()
        {
            if (selectedPackage != null && !selectedPackage.IsEmpty)
            {
                PopulateControls();
            }
            else
            {
                Page.ClearValues();
            }
        }

        /// <summary>
        /// Populate Drop Downs with GetChangePackageDetails_Result
        /// </summary>
        protected virtual void PopulateControls()
        {
            if (selectedPackage != null)
            {
                // Get Change Package Details
                Dictionary<GetImpactDetailsInquiry_Result, ResultStatus> d = GetPackageDetails();
                foreach (KeyValuePair<GetImpactDetailsInquiry_Result, ResultStatus> pair in d)
                {
                    GetImpactDetailsInquiry_Result result = pair.Key;
                    ResultStatus res = pair.Value;
                    if (res.IsSuccess && result.Value.PackageDetails != null)
                    {
                        // Instantiate Record Set variables
                        List<Row> rows = new List<Row>();
                        Header[] headers = { new Header { Name = "Name" }, new Header { Name = "Value" } };
                        List<Row> oRows = new List<Row>();

                        // Get Target Systems for recordset
                        if (result.Value.PackageDetails.TargetSystems == null)
                        {
                            LabelCache labelCache = FrameworkManagerUtil.GetLabelCache(Page.Session);
                            if (labelCache != null)
                            {
                                Camstar.WCF.ObjectStack.Label noTargetLabel = new WCF.ObjectStack.Label();
                                noTargetLabel = labelCache.GetLabelByName("DeployChangePkg_MustAssignTargetToChangePkg");
                                HtmlString html = new HtmlString(noTargetLabel.Value);
                                Page.StatusBar.WriteError(html.ToHtmlString());
                            }
                        }
                        else
                        {
                            foreach (var ts in result.Value.PackageDetails.TargetSystems.Select(s => s.TargetSystemName))
                            {
                                string[] values = { ts.Value, ts.Value };
                                Row r = new Row
                                {
                                    Values = values
                                };
                                rows.Add(r);
                            }
                            // Set Instances Session State for Change Package Instances
                            if (result.Value.PackageDetails.Instances == null)
                            {
                                Instances = null;
                                LabelCache labelCache = FrameworkManagerUtil.GetLabelCache(Page.Session);
                                if (labelCache != null)
                                {
                                    Camstar.WCF.ObjectStack.Label noContentLabel = new WCF.ObjectStack.Label();
                                    noContentLabel = labelCache.GetLabelByName("DeployChangePkgDetail_InstancesMustBeAssigned");
                                    HtmlString html = new HtmlString(noContentLabel.Value);
                                    Page.StatusBar.WriteError(html.ToHtmlString());
                                }

                            }
                            else
                            {
                                if (objectTypes.Data == null)
                                {
                                    Instances = result.Value.PackageDetails.Instances;
                                }
                                else
                                {
                                    Instances = result.Value.PackageDetails.Instances.ToList().Where(i => i.ObjectType.Value.ToString() == objectTypes.Data.ToString()).ToArray();
                                }
                                // Get distinct object types for recordset
                                //                                var list = result.Value.PackageDetails.Instances.GroupBy(item => item.ObjectTypeName.Value).Select(grp => grp.First()).OrderBy(item => item.ObjectTypeName.Value);
                                var list = result.Value.PackageDetails.Instances.GroupBy(item => item.ObjectTypeName.Value).Select(grp => grp.First()).OrderBy(item => item.ObjectTypeValue.Value);
                                foreach (var ot in list)
                                {
                                    //                                    string[] values = { ot.ObjectTypeName.Value, ot.ObjectType.Value.ToString() };
                                    string[] values = { ot.ObjectTypeValue.Value, ot.ObjectType.Value.ToString() };
                                    Row r = new Row
                                    {
                                        Values = values
                                    };
                                    oRows.Add(r);
                                }
                                var rs = new RecordSet
                                {
                                    Rows = rows.ToArray()
                                    ,
                                    Headers = headers
                                };

                                // Set recordset for target systems
                                if (rs.Rows.Count() > 0)
                                {
                                    targetSystem.ClearData();
                                    targetSystem.SetSelectionValues(rs);
                                }

                                //Set recordset for object types
                                var oRs = new RecordSet
                                {
                                    Rows = oRows.ToArray()
                                    ,
                                    Headers = headers
                                };
                                if (oRs.Rows.Count() > 0)
                                {
                                    objectTypes.ClearData();
                                    objectTypes.SetSelectionValues(oRs);
                                }
                            }
                        }
                    }
                    else if (!res.IsSuccess)
                    {
                        Page.StatusBar.WriteError(res.ExceptionData.Description);
                    }
                }
            }
        }

        #endregion

        #region Constants

        #endregion

        #region Private Member Variables
        // Current Target Info in Session State
        protected virtual TargetSystemDetail CurrentTargetInfo
        {
            get
            {
                if (Page.Session["CurrentTargetInfo"] != null)
                {
                    return Page.Session["CurrentTargetInfo"] as TargetSystemDetail;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (Page.Session["CurrentTargetInfo"] != null)
                {
                    Page.Session["CurrentTargetInfo"] = value;
                }
                else
                {
                    Page.Session.Add("CurrentTargetInfo", value);
                }
            }
        }

        // Change package instances in session state
        protected virtual CPModelingInstanceDtl[] Instances
        {
            get
            {
                if (Page.Session["Instances"] != null)
                {
                    return Page.Session["Instances"] as CPModelingInstanceDtl[];
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (Page.Session["Instances"] != null)
                {
                    Page.Session["Instances"] = value;
                }
                else
                {
                    Page.Session.Add("Instances", value);
                }
            }
        }
        #endregion

    }

}
