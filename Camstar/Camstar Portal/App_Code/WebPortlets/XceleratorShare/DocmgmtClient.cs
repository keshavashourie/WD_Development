using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Siemens.PLM.SDK.Lifecycle.DocMgmt;
using Siemens.PLM.SDK.Lifecycle.DocMgmt.Model;
using System.Threading.Tasks;
using System.Reflection;
using Camstar.WebPortal.FormsFramework.Utilities;
using Siemens.PLM.SDK.Core.Client;

/// <summary>
/// Summary description for DocmgmtClient
/// </summary>
/// 
namespace Camstar.WebPortal.WebPortlets.XceleratorShare
{
    public class DocmgmtClient : IDisposable
    {
        private ILifecycleDocmgmtClient docmgmtClient;
        private bool disposed = false;


        public DocmgmtClient(string accessKey, string secretAccessKey)
        {
            try
            {
                var docmMgmtConfiguration = new LifecycleDocmgmtConfiguration
                {                    
                    ACCESS_KEY_ID = accessKey,
                    SECRET_ACCESS_KEY = secretAccessKey,
                    docmgmt = LCSSetting.DOCDomain
                };

                docmgmtClient = LifecycleDocmgmtClientFactory.GetLifecycleDocmgmtClient(docmMgmtConfiguration);
            }
            catch (Exception)
            {
                docmgmtClient = null;
                throw new DocumentMgtException(ErrorMessage.DocClientConnectionError);
            }
           
        }

        ~DocmgmtClient()
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
            docmgmtClient = null;
            disposed = true;
        }

        public DocumentInfo CreateDocumentAsync(string collaborationspaceId, string containerType, string containerId, CreateDocumentsInput createDocumentsInput)
        {
            try
            {
                var result = new BulkApiResponse<DocumentInfo>();
                var taskStat = Task.Run(async () => result = await docmgmtClient.CreateDocumentsAsync(collaborationspaceId, containerType, containerId, createDocumentsInput));
                taskStat.Wait();

                if (result is null || result.Results is null || result.Results.First().Data is null || result.Results.First().Data.Attachments is null)
                {
                    throw new DocumentMgtException(MethodBase.GetCurrentMethod().Name + "  " + ErrorMessage.DOCcreateDocValidateOutputError);
                }
                else if (result.ErrorIndices.Any())
                {
                    throw new DocumentMgtException(MethodBase.GetCurrentMethod().Name + "  " + ErrorMessage.DOCcreateDocError);

                }
                return result.Results.First().Data;

            }
            catch (Exception)
            {

                throw;
            }
            

        }

        public string GetDownloadTicket(string collabspaceId, string containerType, string containerId, string urn)
        {
            GetDocumentRevisionsInput input = new GetDocumentRevisionsInput()
            {
                Urns = new List<string> { urn },
                DownloadTickets = new List<String> { "content" },
                DownloadTicketType = "Standard"
            };
            string downloadTicket = string.Empty;
            var response = this.docmgmtClient.GetDocumentRevisions(collabspaceId, containerType, containerId, input);
            if (response.Results[0].Errors?.Count > 0)
            {
                if (response.Results[0].Errors.First().ErrorCode.Equals(ErrorMessage.RevisionNotFound))
                {
                    throw new Exception(ErrorMessage.FileNotFound);
                }
                throw new Exception(response.Results[0].Errors.First().ErrorCode);
            }
            response.Results[0].Data.DownloadTickets.TryGetValue("content", out downloadTicket);
            return downloadTicket;
        }

        public string DeleteDocumentAsync(string collaborationspaceId, string containerType, string containerId, string urn)
        {
            try
            {
                List<string> Urns = new List<string> { urn };
                DeleteDocumentRevisionsInput deleteDocumentRevisionsInput = new DeleteDocumentRevisionsInput(Urns);
                var result = new DeleteDocumentRevisionsOutput();
                var taskStat = Task.Run(async () => result = await docmgmtClient.DeleteDocumentRevisionsAsync(collaborationspaceId, containerType, containerId, deleteDocumentRevisionsInput));
                taskStat.Wait();
                return result.Task.Id;
            }
            catch (Exception)
            {
                throw new DocumentMgtException(MethodBase.GetCurrentMethod().Name + "  " + ErrorMessage.DOCdeleteDocumentError);
            }

        }


    }
}