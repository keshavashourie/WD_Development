using System;
using System.Collections.Generic;
using System.Web;
using Siemens.PLM.SDK.Lifecycle.Collab.Model;
using Siemens.PLM.SDK.Lifecycle.DocMgmt.Model;
using Siemens.PLM.SDK.Lifecycle.Design.Model;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Net.Http;
using Camstar.WCF.Services;
using Camstar.WebPortal.Helpers;

/// <summary>
/// Namespace responsible for communication with XceleratorShare APIs
/// </summary>
namespace Camstar.WebPortal.WebPortlets.XceleratorShare
{
    public class XceleratorShareDataManager : IDisposable
    {
        private bool disposed = false;
        public XceleratorShareDataManager()
        {
            Utils.GetMCADExtensions();
        }

        ~XceleratorShareDataManager()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }
            disposed = true;
        }
        public const string ACCESS_KEY = "accessKey";
        public const string SECRET_ACCESS_KEY = "secretAccessKey";
        string designType = string.Empty;

        public string GetCollaborationSpace(string accessKey, string secretAccessKey)
        {
            try
            {
                var isValidaccessToken = SingleSignOnHelper.ValidateRefreshAccessToken();
                if (isValidaccessToken)
                {
                    LCSClient lcsClient = new LCSClient(accessKey, secretAccessKey);
                    return lcsClient.GetCollaborationSpaceAsync();
                }
                else throw new FieldAccessException(ErrorMessage.RefreshTokenExtError);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }
        /// <summary>
        ///getProject Method to popualte list of project assign to uese
        /// </summary>
        /// <param name="accessKey"></param>
        /// <param name="secretAccessKey"></param>
        /// <returns></returns>

        public List<LCSProject> GetProjects(string accessKey, string secretAccessKey, string collabspaceId)
        {
            List<LCSProject> filteredProjects = new List<LCSProject>();
            try
            {
                var isValidaccessToken = SingleSignOnHelper.ValidateRefreshAccessToken();
                if (isValidaccessToken)
                {
                    LCSClient lcsClient = new LCSClient(accessKey, secretAccessKey);
                    List<string> roleList = new List<string> {
                LCSRole.PROJECT_ADMIN, LCSRole.PROJECT_AUTHOR
                };
                    List<ListProjectOutput> projects = lcsClient.ListProjectAsync(collabspaceId, roleList);

                    foreach (var project in projects)
                    {
                        LCSProject lcsProject = new LCSProject(project.ProjectId, project.Name);
                        filteredProjects.Add(lcsProject);
                    }
                }
                else throw new FieldAccessException(ErrorMessage.RefreshTokenExtError);
            }
            catch (Exception e)
            {
                throw new XceleratorShareDataManagerException(ErrorMessage.fileuploadProjectError);
            }
            return filteredProjects;
        }

        /// <summary>
        /// Uploaddocuments method internally calll LCS APIs 
        /// </summary>
        /// <param name="accessKey"></param>
        /// <param name="secretAccessKey"></param>
        /// <param name="collaborationspaceId"></param>
        /// <param name="containerType"></param>
        /// <param name="containerId"></param>
        /// <param name="filePath"></param>
        /// <param name="aliasFileName"></param>
        /// <param name="description"></param>
        /// <returns></returns>

        public Attachment UploadAttachment(string accessKey,
               string secretAccessKey,
               string collaborationspaceId,
               string containerType,
               string containerId,
               string filePath,
               string aliasFileName,
               string description,
               string projectName,
               string parentFolderId = null)
        {

            string urn = string.Empty;
            string uploadTicket = string.Empty;
            FileInfo fileInfo;
            string designType = string.Empty;
            bool isFileTypeDesign = false;
            string fileExt = string.Empty;
            string fileName = string.Empty;
            AttachmentDomain domain = AttachmentDomain.DOCUMENTMANAGEMENT;
            try
            {
                var isValidaccessToken = SingleSignOnHelper.ValidateRefreshAccessToken();
                if (isValidaccessToken)
                {
                    DSSClient dssClient = new DSSClient(accessKey, secretAccessKey);
                    fileInfo = new FileInfo(filePath);
                    fileExt = fileInfo.Extension.Replace(".", "");
                    fileName = $"{aliasFileName}.{fileExt}";
                    if (fileExt == null && !File.Exists(filePath))
                    {
                        throw new FileUploadException(MethodBase.GetCurrentMethod().Name + "  " + ErrorMessage.FileUploadFileExtError);
                    }
                    Regex invalidFileNameRegEx = new Regex("[*<>:?|/\\\\\"]+");
                    if (invalidFileNameRegEx.IsMatch(fileName))
                    {
                        throw new FileUploadException(ErrorMessage.InvalidFileName);
                    }
                    if (!(IsUniqueFileInProject(accessKey, secretAccessKey, collaborationspaceId, containerId, fileName, parentFolderId)))
                    {
                        throw new DuplicateFileUploadException(fileName);
                    }
                    string mimeType = Siemens.PLM.FileTransferUtility.Common.ClientHelper.GetContentType(fileName);
                    designType = dssClient.GetDesignTypeName(fileExt);

                    isFileTypeDesign = designType != null;

                    if (isFileTypeDesign)
                    {
                        domain = AttachmentDomain.MCAD;
                        var createDesignInput = new CreateDesignInput
                        {
                            CreateDesignInfo = new List<DesignInput>
                        {
                            new DesignInput
                            {
                                TypeName = "Design",
                                Properties = new JObject
                                {
                                    { "name", fileName },
                                    { "description", description },
                                    { "revid", "A" }
                                },
                                MasterInfo = new MasterInfo
                                {
                                    TypeName = designType,
                                    Properties = new JObject
                                    {
                                        { "name", fileName },
                                        { "filename", fileName },
                                        { "contentType", mimeType }
                                    },
                                    AdhocProperties = new List<CollabAdhocProperty>
                                    {
                                        new CollabAdhocProperty
                                        {
                                            Name = "size",
                                            Value = $"{fileInfo.Length}"
                                        }
                                    },
                                    TicketInfo = new Siemens.PLM.SDK.Lifecycle.Design.Model.TicketInfo
                                    {
                                        MaxBytes = fileInfo.Length
                                    }
                                }
                            }
                        },
                            OutputProperties = new List<string> { "*" },
                            ParentFolder = parentFolderId
                        };

                        using (McadClient mcadClient = new McadClient(accessKey, secretAccessKey))
                        {
                            CreateDesignOutput uploadedDesign = mcadClient.CreateDesignAsync(collaborationspaceId,
                                containerType,
                                containerId,
                                createDesignInput);
                            urn = uploadedDesign.Revision.Urn;
                            uploadTicket = Convert.ToString(uploadedDesign.Attachments[0].UploadTicket);
                        }
                    }
                    else
                    {
                        CreateDocumentsInput createDocsInput = new CreateDocumentsInput
                        {
                            Documents = new List<DocumentInput>
                        {
                            new DocumentInput
                            {
                                DocumentMimeType = mimeType,
                                ContentFileName = fileName,
                                Properties = new Dictionary<string, object>
                                {
                                  { "name", fileName },
                                  { "description", description }
                                },
                                TicketInfo = new Siemens.PLM.SDK.Lifecycle.DocMgmt.Model.TicketInfo(MaxBytes: fileInfo.Length),
                            }
                        },
                            ParentFolder = parentFolderId,
                            FetchUploadTicket = true,
                            AttachmentTypesToReturn = new List<string> { "content" },
                            OutputProperties = new List<string> { "name", "description", "statusInfo", "documentUrn" }
                        };
                        using (DocmgmtClient docmgmtClient = new DocmgmtClient(accessKey, secretAccessKey))
                        {
                            DocumentInfo uploadedDocument = docmgmtClient.CreateDocumentAsync(collaborationspaceId, containerType, containerId, createDocsInput);
                            urn = uploadedDocument.Urn;
                            uploadTicket = uploadedDocument.Attachments[0].UploadTicket;
                        }
                    }
                    dssClient.UploadDocumentAsync(uploadTicket, filePath, fileName);
                    string fileUrl = BuildURL(aliasFileName, "1");
                    return new Attachment(collaborationspaceId, containerType, containerId, urn, domain, projectName, parentFolderId, fileUrl);
                }
                throw new FieldAccessException(ErrorMessage.RefreshTokenExtError);
            }
            catch (Exception ex)
            {
                if (urn != string.Empty)
                {
                    DeleteAttachment(
                        accessKey,
                        secretAccessKey,
                        collaborationspaceId,
                        containerType,
                        containerId,
                        domain,
                        urn);
                }
                // Throwing exception to get handled by parent method
                throw ex;
            }
        }

        /// <summary>
        ///isUniqueFileInProject- method is used to validate duplicate file on Xshare
        /// </summary>
        /// <param name="accessKey"></param>
        /// <param name="secretAccessKey"></param>
        /// <param name="collabspaceId"></param>
        /// <param name="projectId"></param>
        /// <param name="filename"></param>
        /// <returns></returns>

        public bool IsUniqueFileInProject(string accessKey, string secretAccessKey, string collabspaceId, string projectId, string filename, string parentFolderId)
        {
            LCSClient lcsClient = new LCSClient(accessKey, secretAccessKey);
            List<XShareDocument> documents = lcsClient.ListProjectContentAsync(collabspaceId, projectId, parentFolderId);
            bool isUnique = true;

            foreach (var document in documents)
            {
                if (document.properties.name.Equals(filename))
                {
                    isUnique = false;
                }
            }

            return isUnique;
        }

        /// <summary>
        /// This method is use to build URL for future operation on uoloaded document
        /// </summary>
        /// <param name="documentName"></param>
        /// <param name="documentRevision"></param>
        /// <returns></returns>

        private string BuildURL(string documentName, string documentRevision)
        {

            return $"/CamstarPortal/ApolloPortalService.svc/web/GetShareFile/name/{documentName}/rev/{documentRevision}";
        }

        public void DeleteAttachment(string accessKey,
            string secretAccessKey,
            string collabspaceId,
            string containerType,
            string containerId,
            AttachmentDomain domain,
            string urn)
        {
            var isValidaccessToken = SingleSignOnHelper.ValidateRefreshAccessToken();
            if (isValidaccessToken)
            {
                if (domain == AttachmentDomain.DOCUMENTMANAGEMENT)
                {
                    using (DocmgmtClient docmgmtClient = new DocmgmtClient(accessKey, secretAccessKey))
                    {
                        docmgmtClient.DeleteDocumentAsync(collabspaceId,
                            containerType,
                            containerId,
                            urn);
                    }
                }
                else
                {
                    using (McadClient mcadClient = new McadClient(accessKey, secretAccessKey))
                    {
                        mcadClient.DeleteDesignAsync(collabspaceId,
                            containerType,
                            containerId,
                            urn);
                    }
                }
            }
            else
            {
                throw new FieldAccessException(ErrorMessage.RefreshTokenExtError);
            }
        }
    }
}