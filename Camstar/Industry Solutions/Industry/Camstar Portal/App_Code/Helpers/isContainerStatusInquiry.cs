// Copyright Siemens 2019  
using System;
using System.Collections.Generic;
using System.Linq;
using OM = Camstar.WCF.ObjectStack;
using Newtonsoft.Json;
using Camstar.Util;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.WebPortlets.Shopfloor;
using static WebClientPortal.isDefectService;
using System.Data;
using OS = Camstar.WCF.ObjectStack;
using Camstar.WebPortal.WebPortlets;
using System.Collections;

namespace Camstar.WebPortal.Helpers
{
    /// <summary>
    /// Summary description for isContainerStatusInquiry
    /// </summary>
    public class isContainerStatusInquiry : ContainerStatusInquiry
    {
        public isContainerStatusInquiry()
        {

        }

        public override PanelNameValueData GetPanelData(CommandBarCallBackArgs callbackArgs)
        {
            PanelNameValueData panelData = base.GetPanelData(callbackArgs);
            if (panelData != null)
            {
                if (callbackArgs.fun == "getCarrierContainersInfo")
                {
                    getCarrierContainersInfo(panelData, callbackArgs.containerName, callbackArgs.filter);
                }
            }
            return panelData;
        }

        protected override List<OM.DocumentSet> GetAdditionalDocSets(string containerName, string filter, string tasklistName, string taskName, string taskListId, string taskListRev, string isCurrentDefectsId)
        {
            List<OM.DocumentSet> docSets = new List<OM.DocumentSet>();

           // load documents for the defect
           var currentUserProfile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
            QueryService querySvc = new QueryService(currentUserProfile);
            OS.RecordSet docRecords = new OS.RecordSet();

            OS.QueryParameters queryParams = new OS.QueryParameters()
            {
                Parameters = new OS.QueryParameter[1]
                {
                    new QueryParameter("ISCURRENTDEFECTID", isCurrentDefectsId)
                }
            };

            OS.ResultStatus resultStatus = querySvc.Execute("isDefect_GetDefectDocuments", queryParams, new OS.QueryOptions(), out docRecords);

            // parse query results, create a doc set object
            if (!resultStatus.IsSuccess)
                return docSets;

            DataTable docTable = docRecords.GetAsDataTable();
            if (docTable != null && docTable.Rows.Count > 0)
            {
                DocumentSet docSet = new DocumentSet()
                {
                    Name = FrameworkManagerUtil.GetFrameworkSession().GetLabelCache().GetLabelByName("Web_Defect").Value// "Defect" // TODO?
                };
                List<DocumentEntry> docs = new List<DocumentEntry>();

                foreach (DataRow docRow in docTable.Rows)
                {
                    string browseMode = Util.Utilities.GetDataRowField(docRow, "BrowseMode");

                    docs.Add(new DocumentEntry() { 
                        Name = Util.Utilities.GetDataRowField(docRow, "DocumentName"),
                        Description = Util.Utilities.GetDataRowField(docRow, "Description"),
                        Document = new RevisionedObjectRef()
                        {
                            Name = Util.Utilities.GetDataRowField(docRow, "DocumentName"),
                            Revision = Util.Utilities.GetDataRowField(docRow, "DocumentRevision")
                        },
                        DocumentBrowseMode = string.IsNullOrWhiteSpace(browseMode) ? (int)BrowseModeEnum.LocalFile : Convert.ToInt32(browseMode),
                        DocumentIdentifier = Util.Utilities.GetDataRowField(docRow, "AttachedFileName")
                    });
                }

                docSet.DocumentEntries = docs.ToArray();
                docSets.Add(docSet);
            }

            return docSets;
        }

        public void getCarrierContainersInfo(PanelNameValueData panelData, string containerName, string filter)
        {
            var frmSess = FrameworkManagerUtil.GetFrameworkSession();
            var svc = new CarrierMaintService(frmSess.CurrentUserProfile);

            CarrierMaint_Request request = new CarrierMaint_Request()
            {
                Info = new OM.CarrierMaint_Info()
                {
                    RequestValue = true,
                    isDocumentSets = new OM.DocumentSet_Info()
                    {
                        DocumentEntries = new OM.DocumentEntry_Info() { RequestValue = true },
                        Name = new OM.Info(true),
                        RequestValue = true,
                    }
                }
            };
            OM.CarrierMaint data = getCarrierMaintInput(containerName);

            CarrierMaint_Result res;
            var state = svc.GetEnvironment(data, request, out res);

            if (state.IsSuccess && res.Value != null)
            {
                var s = res.Value;

                panelData.Add("ContainerName", containerName);
                var docSets = s.isDocumentSets != null ? new List<OM.DocumentSet>(s.isDocumentSets) : new List<OM.DocumentSet>();

                panelData.Add("CarrierDocumentSets", docSets != null ? docSets.Select(ds =>
                {
                    return new
                    {
                        Name = ds.Name.Value,
                        DocumentEntries = ds.DocumentEntries.Select(de =>
                        {
                            return new
                            {
                                Name = de.Name.Value,
                                Description = de.Description != null ? de.Description.Value : "",
                                Document = new { de.Document.Name, de.Document.Revision },
                                BrowseMode = Enum.GetName(typeof(OM.BrowseModeEnum), de.DocumentBrowseMode.Value),
                                FileType = base.GetFileExt(de),
                                DocumentIdentifier = de.DocumentIdentifier
                            };
                        })
                    };
                }).ToArray() : new object[0]);

            }
            else
            {
                panelData.Add("Error", state.ExceptionData.ToString());
            }
        }

        protected DocumentRefInfo DownloadImage(OM.RevisionedObjectRef documentRev)
        {
            OM.ResultStatus resultStatus = new OM.ResultStatus();
            var session = FrameworkManagerUtil.GetFrameworkSession(System.Web.HttpContext.Current.Session);
            var configFolder = Camstar.WebPortal.Utilities.CamstarPortalSection.Settings.DefaultSettings.UploadDirectory;
            LabelCache labelCache = LabelCache.GetRuntimeCacheInstance();
            var label = labelCache.GetLabelByName("Lbl_SharedFolderDoesntExists");
            var message = string.Format(label != null ? label.Value : "Shared folder '{0}' does not exist.", configFolder);

            var docInfo = Camstar.WebPortal.WebPortlets.Modeling.isImageAttachmentExecutor.DownloadDocumentRef(documentRev, session.CurrentUserProfile, message, out resultStatus);
            if (!resultStatus.IsSuccess)
            {
                docInfo = null;
            }
            return docInfo;
        }
        protected override void openDocument(PanelNameValueData response, OM.RevisionedObjectRef doc)
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
                docInfo = DownloadImage(doc);
                if (docInfo == null)
                {
                    message = resultStatus.Message;
                    if (string.IsNullOrEmpty(message) && resultStatus.ExceptionData != null)
                        message = resultStatus.ExceptionData.Description;
                    if (string.IsNullOrEmpty(message))
                        message = "Unknown error";
                    response.Add("Error", message);
                }
            }
            //else
            //{
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
            //}
        }

        private OM.CarrierMaint getCarrierMaintInput(string containerName)
        {
            var session = FrameworkManagerUtil.GetFrameworkSession(System.Web.HttpContext.Current.Session);
            string queryString = String.Format(@"SELECT cs.SpecId, Child.ProductId, Child.MfgOrderId 
                                                FROM Container Parent 
	                                                 INNER JOIN Container Child ON Parent.ContainerId = Child.ParentContainerId
	                                                 INNER JOIN CurrentStatus cs ON cs.CurrentStatusId = Parent.CurrentStatusId
                                                WHERE Parent.ContainerName = '{0}'", containerName);

            var qs = new QueryService(session.CurrentUserProfile);
            OM.QueryOptions qr = new OM.QueryOptions();
            OM.RecordSet rs;
            OM.CarrierMaint carrierMaint = new OM.CarrierMaint();

            var res = qs.ExecuteAdHoc(queryString, qr, out rs);
            if (res.IsSuccess)
            {
                if ((rs as OM.RecordSet).Rows != null && (rs as OM.RecordSet).Rows.Count() > 0)
                {
                    carrierMaint.isDocSpec = rs.Rows.Select(r => new OM.RevisionedObjectRef { ID = r.Values[0] }).FirstOrDefault();
                    carrierMaint.isDocProducts = rs.Rows.Select(r => r.Values[1]).Distinct().Select(r => new OM.RevisionedObjectRef { ID = r }).ToArray();
                    carrierMaint.isDocMfgOrders = rs.Rows.Select(r => r.Values[2]).Distinct().Select(r => new OM.NamedObjectRef { ID = r }).ToArray();
                }
            }

            return carrierMaint;
        }
    }

    
}