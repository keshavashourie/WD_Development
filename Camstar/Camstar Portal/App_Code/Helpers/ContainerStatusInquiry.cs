// Copyright Siemens 2025  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using OM = Camstar.WCF.ObjectStack;
using Newtonsoft.Json;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.WCFUtilities;
using cbArgs = Camstar.WebPortal.WebPortlets.CommandBarCallBackArgs;

namespace Camstar.WebPortal.Helpers
{
    /// <summary>
    /// Summary description for ContainerStatusInquiry
    /// </summary>
    public class ContainerStatusInquiry : CommandBarHelper
    {
        public ContainerStatusInquiry()
        {
        }

        public override PanelNameValueData GetPanelData(cbArgs callbackArgs)
        {
            PanelNameValueData panelData = base.GetPanelData(callbackArgs);
            if (panelData != null)
            {
                if (callbackArgs.fun == "getContainerInfo")
                {
                    // pass in task documents if needed.
                    getContainerStatus(panelData, callbackArgs.containerName, callbackArgs.filter, callbackArgs.tasklistName, callbackArgs.taskName, callbackArgs.tasklistId, callbackArgs.tasklistRevision, callbackArgs.tag);
                }
                else if (callbackArgs.fun == "openDocument")
                {
                    openDocument(panelData, new OM.RevisionedObjectRef(callbackArgs.documentName, callbackArgs.documentRev ?? ""));
                }
                else if (callbackArgs.fun == "openTasklistSummary")
                {
                    openTasklistSummary(panelData, callbackArgs.containerName, callbackArgs.tasklistName, callbackArgs.taskName, callbackArgs.tasklistId, callbackArgs.tasklistRevision);
                }
                else
                {
                    //return
                }
            }
            return panelData;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="filter"></param>
        /// <param name="tasklistName"></param>
        /// <param name="taskName"></param>
        /// <param name="taskListId"></param>
        /// <param name="taskListRev"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        protected virtual List<OM.DocumentSet> GetAdditionalDocSets(string containerName, string filter, string tasklistName, string taskName, string taskListId, string taskListRev, string tag)
        {
            // derived class may override to add workspace-specific information to panelData
            return new List<OM.DocumentSet>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="panelData">Add info about the Container to this Dictionary</param>
        /// <param name="containerName"></param>
        /// <param name="filter">Indicates type of info to load</param>
        /// <param name="tasklistName"></param>
        /// <param name="taskName"></param>
        /// <param name="taskListId"></param>
        /// <param name="taskListRev"></param>
        /// <param name="tag">Piece of info that could be used for any purpose</param>
        public virtual void getContainerStatus(PanelNameValueData panelData, string containerName, string filter, string tasklistName, string taskName, string taskListId, string taskListRev, string tag)
        {
            var frmSess = FrameworkManagerUtil.GetFrameworkSession();
            var svc = new ContainerTxnService(frmSess.CurrentUserProfile);
            var data = new OM.ContainerTxn { Container = new OM.ContainerRef(containerName) };
            var req = new ContainerTxn_Request
            {
                Info = new OM.ContainerTxn_Info { CurrentContainerStatus = new OM.CurrentContainerStatus_Info(), Factory = new OM.Info(true) }
            };

            List<contStatusField> fieldList = null;

            if (filter == "all")
            {
                req.Info.CurrentContainerStatus = new OM.CurrentContainerStatus_Info
                {
                    ContainerLevelName = new OM.Info(true),
                    StatusName = new OM.Info(true),
                    Qty = new OM.Info(true),
                    UOMName = new OM.Info(true),
                    ProductName = new OM.Info(true),
                    MfgOrderName = new OM.Info(true),
                    Workflow = new OM.Info(true),
                    SpecName = new OM.Info(true),
                    Operation = new OM.Info(true),
                    ResourceName = new OM.Info(true),
                    IsOnHold = new OM.Info(true),
                    Factory = new WCF.ObjectStack.Info(true),
                    Attributes = new OM.UserAttribute_Info { RequestValue = true },
                    ResolvedAttributes = new OM.UserAttribute_Info { Name = new OM.Info(true), AttributeValue = new OM.Info(true) }
                };
                fieldList = new string[]
                {
                    "ContainerLevelName",
                    "StatusName",
                    "Qty",
                    "UOMName",
                    "ProductName",
                    "MfgOrderName",
                    "Factory",
                    "Workflow",
                    "SpecName",
                    "Operation",
                    "ResourceName",
                    "IsOnHold"
                }.Select(s => new contStatusField { name = s }).ToList();

                LabelCache lc = frmSess.GetLabelCache();
                var ll = getLabelList(fieldList);
                lc.GetLabels(ll);
                foreach (var f in fieldList)
                {
                    var l = ll.FirstOrDefault(lx => lx.ID == f.labelId);
                    if (l != null)
                        f.labelText = l.Value;
                }
            }
            else if (filter == "documents")
                req.Info.CurrentContainerStatus = new OM.CurrentContainerStatus_Info { DocumentSets = new OM.DocumentSet_Info { RequestValue = true } };
            else if (filter == "workflow")
                req.Info.CurrentContainerStatus = new OM.CurrentContainerStatus_Info { Workflow = new OM.Info(true), Step = new OM.Info(true) };

            ContainerTxn_Result res;
            var state = svc.Load(data, req, out res);
            if (state.IsSuccess && res.Value.CurrentContainerStatus != null)
            {
                var s = res.Value.CurrentContainerStatus;
                if (filter == "all")
                {
                    var prps = typeof(OM.CurrentContainerStatus).GetProperties();
                    foreach (var f in fieldList)
                    {
                        //  This would always get the Factory for the Employee - not the Container
                        /*if (f.name == "Factory")
                        {
                            panelData.Add(f.labelText, new { Value = res.Value.Factory != null ? res.Value.Factory.Name : "" });
                        }
                        else
                        {*/
                        var prp = prps.FirstOrDefault(p => p.Name == f.name);
                        if (prp != null)
                            f.value = prp.GetValue(s, null) as object;
                        if (f.value is OM.BaseObjectRef || f.value is OM.Primitive<bool>)
                            f.value = new { Value = f.value.ToString() };
                        panelData.Add(f.labelText, f.value);
                        //}
                    }

                    if (s.Attributes == null)
                        s.Attributes = new OM.UserAttribute[] { };


                    List<KeyValuePair<string, string>> resolvedAttributeList = new List<KeyValuePair<string, string>>();

                    if (s.ResolvedAttributes != null)
                    {
                        for (int i = 0; i < s.ResolvedAttributes.Length; i++)
                        {
                            if (s.ResolvedAttributes[i].AttributeValue == null)
                            {
                                resolvedAttributeList.Add(new KeyValuePair<string, string>(s.Attributes[i].Name.Value, s.Attributes[i].AttributeValue != null ? s.Attributes[i].AttributeValue.ToString() : ""));
                            }
                            else
                            {
                                resolvedAttributeList.Add(new KeyValuePair<string, string>(s.ResolvedAttributes[i].Name.Value, s.ResolvedAttributes[i].AttributeValue != null ? s.ResolvedAttributes[i].AttributeValue.ToString() : ""));
                            }
                        };
                    }

                    panelData.Add("Attributes", resolvedAttributeList.Select(a =>
                    {
                        return new
                        {
                            Name = a.Key,
                            AttributeValue = a.Value != null ? a.Value.ToString() : ""
                        };
                    }).ToArray());
                }
                else if (filter == "documents")
                {
                    panelData.Add("ContainerName", containerName);
                    var docSets = s.DocumentSets != null ? new List<OM.DocumentSet>(s.DocumentSets) : new List<OM.DocumentSet>();
                    if (!string.IsNullOrEmpty(tasklistName) && !string.IsNullOrEmpty(taskName))
                    {
                        var taskCDO = getTransactionTasksDetails(containerName, tasklistName, taskName, taskListId, taskListRev);
                        if (taskCDO != null && taskCDO.DocumentEntries != null)
                        {
                            var entries = new OM.DocumentSet { Name = taskCDO.DocumentSet.Name, DocumentEntries = taskCDO.DocumentEntries };
                            docSets.Add(entries);
                        }
                    }

                    // allow derived classes to get more docs before we construct the final data structure
                    List<OM.DocumentSet> additionalDocSets = GetAdditionalDocSets(containerName, filter, tasklistName, taskName, taskListId, taskListRev, tag);
                    docSets.AddRange(additionalDocSets);

                    panelData.Add("DocumentSets", docSets != null ? docSets.Select(ds =>
                    {
                        return new
                        {
                            Name = ds.Name.Value,
                            DocumentEntries = ds.DocumentEntries.Select(de =>
                            {
                                return new
                                {
                                    Name = de.Name != null ? de.Name.Value : string.Empty,
                                    Description = de.Description != null ? de.Description.Value : "",
                                    Document = new { de.Document.Name, de.Document.Revision },
                                    BrowseMode = de.DocumentBrowseMode != null ? Enum.GetName(typeof(OM.BrowseModeEnum), de.DocumentBrowseMode.Value) : Enum.GetName(typeof(OM.BrowseModeEnum), OM.BrowseModeEnum.LocalFile),
                                    FileType = GetFileExt(de),
                                    DocumentIdentifier = de.DocumentIdentifier != null ? de.DocumentIdentifier.Value : ""
                                };
                            })
                        };
                    }).ToArray() : new object[0]);
                }
                else if (filter == "workflow")
                {
                    panelData.Add("ContainerName", containerName);
                    panelData.Add("WorkflowRef", s.Workflow);
                    panelData.Add("StepName", s.Step != null ? s.Step.Name : null);
                    //(Page as CamstarForm).SessionVariables["WorkflowRef"] = s.Workflow;
                }
            }
            else
            {
                panelData.Add("Error", state.ExceptionData.ToString());
            }

        }

        protected string GetFileExt(OM.DocumentEntry docEntry)
        {
            var browseMode = docEntry.DocumentBrowseMode != null ? (OM.BrowseModeEnum)docEntry.DocumentBrowseMode.Value : OM.BrowseModeEnum.LocalFile;
            if (browseMode == OM.BrowseModeEnum.Url)
            {
                return "url";
            }
            else
            {
                var identifierIsKnown = docEntry.DocumentIdentifier != null && !string.IsNullOrEmpty(docEntry.DocumentIdentifier.Value);
                var extension = identifierIsKnown
                    ? System.IO.Path.GetExtension(docEntry.DocumentIdentifier.Value)
                    : null;
                return !string.IsNullOrEmpty(extension) ? extension : "unknown";
            }
        }

        protected LabelList getLabelList(List<contStatusField> flds)
        {
            const string fexPref = "ContainerTxn.CurrentContainerStatus.";
            var ll = new LabelList();
            foreach (var f in flds)
            {
                var metadata = OM.Environment.GetMetadata(fexPref + f.name);
                if (metadata != null)
                    f.labelId = metadata.LabelID;
            }
            var lbs =
                from f in flds
                where f.labelId != null
                select new OM.Label(f.labelId.Value);
            return new LabelList(lbs.ToArray());
        }

        private void openTasklistSummary(PanelNameValueData response, string containerName, string tasklistName, string txnName, string taskListId, string taskListRev)
        {
            var txnTaskDetails = getTransactionTasksDetails(containerName, tasklistName, txnName, taskListId, taskListRev);
            if (txnTaskDetails != null)
            {
                var tasklistSummaryLabel = "Task List Summary";
                var labelCache = LabelCache.GetRuntimeCacheInstance();
                if (labelCache != null)
                {
                    tasklistSummaryLabel = labelCache.GetLabelByName("TaskListSummary_Title") != null
                        ? labelCache.GetLabelByName("TaskListSummary_Title").Value
                        : tasklistSummaryLabel;
                }

                response.Add("OpenTaskListSummary", new
                {
                    TaskCDO = txnTaskDetails,
                    TasklistSummaryLabel = tasklistSummaryLabel
                });
            }
        }

        protected virtual void openDocument(PanelNameValueData response, OM.RevisionedObjectRef doc)
        {
            OM.ResultStatus resultStatus;
            doc.RevisionOfRecord = string.IsNullOrEmpty(doc.Revision);

            var session = FrameworkManagerUtil.GetFrameworkSession(System.Web.HttpContext.Current.Session);
            var configFolder = Utilities.CamstarPortalSection.Settings.DefaultSettings.UploadDirectory;
            LabelCache labelCache = LabelCache.GetRuntimeCacheInstance();
            var label = labelCache.GetLabelByName("Lbl_SharedFolderDoesntExists");
            var message = string.Format(label != null && !string.IsNullOrEmpty(label.Value) ? label.Value : "Shared folder '{0}' does not exist.", configFolder);

            var docInfo = AttachmentExecutor.DownloadDocumentRef(doc, session.CurrentUserProfile, message, out resultStatus);
            if (!resultStatus.IsSuccess)
            {
                message = resultStatus.Message;
                if (string.IsNullOrEmpty(message) && resultStatus.ExceptionData != null)
                    message = resultStatus.ExceptionData.Description;
                if (string.IsNullOrEmpty(message))
                    message = "Unknown error";
                response.Add("Error", message);
            }
            else
            {
                if (docInfo != null && !(string.IsNullOrEmpty(docInfo.FileName) && string.IsNullOrEmpty(docInfo.URI)))
                {
                    response.Add("DocInfo", new
                    {
                        BrowseMode = Enum.GetName(typeof(OM.BrowseModeEnum), docInfo.BrowseMode),
                        docInfo.URI,
                        docInfo.FileName,
                        docInfo.IsRemote,
                        docInfo.DocumentRef,
                        AuthenticationType = Enum.GetName(typeof(OM.AuthenticationTypeEnum), docInfo.AuthenticationType)
                    }
                    );
                }
                else
                {
                    response.Add("Error", "File name error");
                }
            }
        }

        private OM.ExecuteTask getTransactionTasksDetails(string containerName, string tasklistName, string txnName, string taskListId, string taskListRev)
        {

            var frmSess = FrameworkManagerUtil.GetFrameworkSession();
            var svc = new ExecuteTaskService(frmSess.CurrentUserProfile);

            var parent = new OM.RevisionedObjectRef(tasklistName, taskListRev);
            var data = new OM.ExecuteTask { Container = new OM.ContainerRef(containerName), Task = new OM.NamedSubentityRef(txnName, parent) };
            var req = new ExecuteTask_Request
            {
                Info = new OM.ExecuteTask_Info
                {
                    DocumentSet = new OM.Info(true),
                    DocumentEntries = new OM.DocumentEntry_Info
                    {
                        Name = new OM.Info(true),
                        Document = new OM.Info(true),
                        DocumentBrowseMode = new OM.Info(true),
                        DocumentIdentifier = new OM.Info(true),
                        Description = new OM.Info(true)
                    },
                    Container = new OM.Info(true)
                }
            };
            ExecuteTask_Result res;
            var resultStatus = svc.Load(data, req, out res);
            if (resultStatus.IsSuccess && res.Value != null)
            {
                return res.Value;
            }
            return null;
        }
    }

    public class contStatusField
    {
        public string name { get; set; }
        public int? labelId { get; set; }
        public string labelText { get; set; }
        public object value { get; set; }
    }
}
